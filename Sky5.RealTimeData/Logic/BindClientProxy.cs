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
        public abstract void Patch(IEnumerable<BsonDocument> rows);
        internal abstract void Insert(BsonDocument fullDocument);

        public abstract Task Modify(ModifyItems modify);
    }
}
