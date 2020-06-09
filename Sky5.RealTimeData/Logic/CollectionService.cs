using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sky5.RealTimeData.Logic
{
    public class CollectionService
    {
        public IMongoCollection<BsonDocument> Collection { get; set; }

        #region Bind
        readonly List<BindInfo> Binds = new List<BindInfo>();
        CancellationTokenSource CancellationWatchSource;
        internal void AddBind(BindInfo bind)
        {
            lock (Binds)
            {
                if (timerCloseChangeStreams != null)
                {
                    timerCloseChangeStreams.Dispose();
                    timerCloseChangeStreams = null;
                }
                Binds.Add(bind);
                var task = Query(bind);
                if (Binds.Count == 1)
                    task = OpenChangeStreams();
            }
        }
        Timer timerCloseChangeStreams;
        internal void RemoveBind(HubCallerContext context)
        {
            lock (Binds)
            {
                Binds.RemoveAll(bind => bind.Client.Context == context
                || bind.Client.Context.ConnectionAborted.IsCancellationRequested);
                if (Binds.Count == 0 && timerCloseChangeStreams == null)
                    timerCloseChangeStreams = new Timer(
                        state => ((CancellationTokenSource)state).Cancel()
                    , CancellationWatchSource, 10000, Timeout.Infinite);
            }
        }
        #endregion

        public async Task Query(BindInfo bind)
        {
            var find = Collection.Find(bind.Filter);
            if (bind.Page != null)
            {
                if (bind.Page.Sort != null)
                    find = find.Sort(bind.Page.Sort);
                if (bind.Page.Skip.HasValue)
                    find = find.Skip(bind.Page.Skip);
                if (bind.Page.Limit.HasValue)
                    find = find.Limit(bind.Page.Limit);
                bind.Page.FirstSort = null;
                bind.Page.LastSort = null;
            }

            if (bind.Projection != null)
                find = find.Project(bind.Projection);

            using (var cursor = await find.ToCursorAsync(bind.Client.Context.ConnectionAborted))
            {
                while (await cursor.MoveNextAsync(bind.Client.Context.ConnectionAborted))
                {
                    var proxy = bind.Patch(cursor.Current);
                    bind.Client.Patch(proxy);
                }
            }

            if (bind.Page != null && bind.Page.FirstSort != null)
            {
                bind.Page.Total = await find.CountDocumentsAsync(bind.Client.Context.ConnectionAborted);
            }
        }

        public async Task OpenChangeStreams()
        {
            if (CancellationWatchSource != null && !CancellationWatchSource.IsCancellationRequested)
                return;

            ModifyItems modify = new ModifyItems();
            CancellationWatchSource = new CancellationTokenSource();
            using (var cursor=await Collection.WatchAsync(new ChangeStreamOptions
            {

            }, CancellationWatchSource.Token))
            {
                while (await cursor.MoveNextAsync() && cursor.Current.Count() == 0)
                {
                    lock (Binds)
                    {
                        foreach (var item in cursor.Current)
                        {
                            switch (item.OperationType)
                            {
                                case ChangeStreamOperationType.Insert:
                                    foreach (var bind in Binds)
                                        bind.OnInserted(item);
                                    break;
                                case ChangeStreamOperationType.Update:
                                    foreach (var bind in Binds)
                                        bind.OnUpdated(item);
                                    break;
                                case ChangeStreamOperationType.Replace:
                                    foreach (var bind in Binds)
                                        bind.OnUpdated(item);
                                    break;
                                case ChangeStreamOperationType.Delete:
                                    foreach (var bind in Binds)
                                        bind.OnDeleted(item);
                                    break;
                                case ChangeStreamOperationType.Invalidate:
                                    foreach (var bind in Binds)
                                        bind.OnInvalidated(item);
                                    break;
                                case ChangeStreamOperationType.Rename:
                                    foreach (var bind in Binds)
                                        bind.OnRenamed(item);
                                    break;
                            }
                        }

                        foreach (var bind in Binds)
                            bind.BeginPublishLoop(null);
                    }
                }
            }
        }
    }
}
