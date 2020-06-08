using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sky5.RealTimeData.Logic
{
    public abstract class BindClientProxy
    {
        public HubCallerContext Context;
        public abstract void Remove(IEnumerable<BsonValue> keys);
        public abstract void Patch(IEnumerable<BsonDocument> rows);
    }
}
