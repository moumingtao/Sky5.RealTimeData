using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sky5.RealTimeData.Logic
{
    public class BindInfo
    {
        public CollectionService Collection;
        public BindClientProxy Client { get; set; }

        public BsonDocument Filter { get; set; }
        public BsonDocument Projection { get; set; }
        public PageInfo Page;

        ModifyItems Modify = new ModifyItems();

        internal void OnInserted(ChangeStreamDocument<BsonDocument> item)
        {
            if (!FilterUtils.IsMatch(Filter, item.FullDocument)) return;
            InsertCore(item);
        }

        void InsertCore(ChangeStreamDocument<BsonDocument> item)
        {
            if (Page == null)
                Modify.Insert(item);
            else
            {
                if (Page.BeforeFirst(item.FullDocument))
                    Modify.Skip++;
                else if (Page.BeforeLast(item.FullDocument))
                {
                    Modify.Insert(item);
                    Modify.Limit++;
                }
                Modify.Total++;
            }
        }

        internal void OnUpdated(ChangeStreamDocument<BsonDocument> item)
        {
            bool mOld = FilterUtils.IsMatch(Filter, item.BackingDocument);
            bool mNew = FilterUtils.IsMatch(Filter, item.FullDocument);
            if (mOld)
            {
                if (mNew)
                {
                    if (Page == null)
                        Modify.Update(item);
                    else
                    {
                        if (Page.BeforeFirst(item.FullDocument))
                        {
                            Modify.Skip++;
                            Modify.Limit--;
                            Modify.Remove(item);
                        }
                        else if (Page.BeforeLast(item.FullDocument))
                        {
                            Modify.Update(item);
                        }
                        else
                        {
                            Modify.Limit--;
                            Modify.Remove(item);
                        }
                    }
                }
                else OnDeleted(item);
            }
            else if(mNew) InsertCore(item);
        }

        internal void OnDeleted(ChangeStreamDocument<BsonDocument> item)
        {
            if (!FilterUtils.IsMatch(Filter, item.FullDocument)) return;
            DeleteCore(item);
        }
        void DeleteCore(ChangeStreamDocument<BsonDocument> item)
        {
            if (Page == null)
                Modify.Remove(item);
            else
            {
                Modify.Total--;
                if (Page.BeforeFirst(item.BackingDocument))
                    Modify.Skip--;
                else if (Page.BeforeLast(item.BackingDocument))
                {
                    Modify.Limit--;
                    Modify.Remove(item);
                }
            }
        }

        internal void OnRenamed(ChangeStreamDocument<BsonDocument> item)
        {

        }

        internal IEnumerable<BsonDocument> Patch(IEnumerable<BsonDocument> current)
        {
            foreach (var item in current)
            {
                if (Page != null && Page.FirstSort == null)
                    Page.FirstSort = item;
                yield return item;
                if (Page != null)
                    Page.LastSort = item;
            }
        }

        internal void OnInvalidated(ChangeStreamDocument<BsonDocument> item)
        {

        }

        bool Looping;
        public void BeginPublishLoop(Task last)
        {
            if (last == null)
            {
                if (Looping) return;
            }
            else if(last.IsFaulted)
            {
                Looping = false;
                return;
            }
            if (Modify?.SetPageIfHasChanges(Page) == true)
            {
                Looping = true;
                var modify = Modify;
                Modify = null;
                Client.Modify(modify).ContinueWith(BeginPublishLoop);
            }
            else Looping = false;
        }
    }
}
