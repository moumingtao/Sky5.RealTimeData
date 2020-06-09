using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sky5.RealTimeData.Logic
{
    public class ModifyItems
    {
        public List<ModifyItem> Items { get; set; }

        public int Total { get; set; }
        public int Skip { get; set; }
        public int Limit { get; set; }

        internal void Insert(ChangeStreamDocument<BsonDocument> item)
        {
            Items.RemoveAll(i => i.DocumentKey == item.DocumentKey);
            Items.Add(new ModifyItem { Type = "insert", DocumentKey = item.DocumentKey, Data = item.FullDocument });
        }

        internal void Remove(ChangeStreamDocument<BsonDocument> item)
        {
            Items.RemoveAll(i => i.DocumentKey == item.DocumentKey);
            Items.Add(new ModifyItem { Type = "remove", DocumentKey = item.DocumentKey});
        }

        internal void Update(ChangeStreamDocument<BsonDocument> item)
        {
            Items.Add(new ModifyItem { Type = "update", DocumentKey = item.DocumentKey, Data = item.FullDocument });
        }

        public bool SetPageIfHasChanges(PageInfo page)
        {
            if (page != null && (page.Total != Total || page.Skip != Skip || page.Limit != Limit))
            {
                page.Total = Total;
                page.Skip = Skip;
                page.Limit = Limit;
                return true;
            }
            return Items.Count > 0;
        }
        //=> Items.Count > 0 || (page != null && page.Total == Total && page.Skip == Skip && page.Limit == Limit);
    }
    public class ModifyItem
    {
        public string Type { get; set; }
        public BsonDocument DocumentKey { get; set; }
        public BsonDocument Data { get; set; }
    }
}
