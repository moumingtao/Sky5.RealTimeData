using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Sky5.RealTimeData.Logic
{
    public class FilterUtils
    {
        static readonly Dictionary<string, Func<BsonDocument, BsonDocument, bool>> methods = new Dictionary<string, Func<BsonDocument, BsonDocument, bool>>();
        static FilterUtils()
        {
            Register(and);
        }
        static void Register(Func<BsonDocument, BsonDocument, bool> method) => methods.Add(method.Method.Name, method);

        public static bool IsMatch(BsonDocument filter, BsonDocument value)
        {
            if (filter == null) return true;
            return and(filter, value);
        }

        static bool and(BsonValue filter, BsonDocument value)
        {
            if(filter is BsonArray array)
        }
    }
}
