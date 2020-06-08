using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sky5.RealTimeData.Logic
{
    public class PageInfo
    {
        public BsonDocument Sort { get; set; }
        public long Total { get; set; }
        public int? Skip { get; set; }
        public int? Limit { get; set; }

        public BsonDocument FirstRow { get; set; }
        public BsonDocument LastRow { get; set; }
    }
}
