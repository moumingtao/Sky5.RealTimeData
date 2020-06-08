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

        internal void OnInserted(ChangeStreamDocument<BsonDocument> item)
        {

        }

        internal void OnUpdated(ChangeStreamDocument<BsonDocument> item)
        {

        }

        internal void OnReplaced(ChangeStreamDocument<BsonDocument> item)
        {

        }

        internal void OnDeleted(ChangeStreamDocument<BsonDocument> item)
        {

        }

        internal void OnRenamed(ChangeStreamDocument<BsonDocument> item)
        {

        }

        internal IEnumerable<BsonDocument> Patch(IEnumerable<BsonDocument> current)
        {
            foreach (var item in current)
            {
                if (Page != null && Page.FirstRow == null)
                    Page.FirstRow = item;
                yield return item;
                if (Page != null)
                    Page.LastRow = item;
            }
        }
    }
}
