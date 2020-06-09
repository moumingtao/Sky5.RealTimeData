using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sky5.RealTimeData.Logic
{
    public class SortUtils
    {
        public static int Compare(BsonDocument sort, BsonDocument x, BsonDocument y)
        {
            foreach (var element in sort)
            {
                x.TryGetValue(element.Name, out var vx);
                y.TryGetValue(element.Name, out var vy);
                if (vx == vy) continue;
                if (vx > vy)
                    return element.Value.AsInt32;
            }
            return 0;
        }
    }
}
