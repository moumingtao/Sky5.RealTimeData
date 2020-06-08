using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using Sky5.RealTimeData.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sky5.RealTimeData.Hubs
{
    public class BindHub:Hub<BindClientProxy>
    {
        public IMongoDatabase Database;
        ConcurrentDictionary<string, CollectionService> Collections = new ConcurrentDictionary<string, CollectionService>();

        public void Bind(string path, string collectionName, BsonDocument filter, BsonDocument projection, BsonDocument sort, int? skip, int? limit)
        {
            var bind = new BindInfo {
                Filter = filter,
                Projection = projection,
                Client = Clients.Caller,
            };
            if (sort != null || skip.HasValue)
                bind.Page = new PageInfo { Sort = sort, Skip = skip, Limit = limit };
            bind.Client.Context = Context;
            var service = Collections.GetOrAdd(collectionName, AddCollectionService);
            service.AddBind(bind);
            bind.Collection = service;
            Context.Items[path] = bind;
        }

        public void RefreshData(string path)
        {
            if (Context.Items.TryGetValue(path, out var value) && value is BindInfo info)
                info.Collection.Query(info);
        }

        private CollectionService AddCollectionService(string collectionName)
        {
            var coll = new CollectionService {
                Collection = Database.GetCollection<BsonDocument>(collectionName)
            };
            return coll;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            foreach (var item in Collections)
                item.Value.RemoveBind(Context);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
