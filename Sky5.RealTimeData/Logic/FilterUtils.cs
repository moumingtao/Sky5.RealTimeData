using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Sky5.RealTimeData.Logic
{
    // https://docs.mongodb.com/manual/reference/operator/query/
    public class FilterUtils
    {
        static readonly Dictionary<string, Func<BsonDocument, BsonDocument, bool>> methods = new Dictionary<string, Func<BsonDocument, BsonDocument, bool>>();
        static FilterUtils()
        {
            Register(and);
        }
        static void Register(Func<BsonDocument, BsonDocument, bool> method) => methods.Add(method.Method.Name, method);

        public static bool IsMatch(BsonDocument filter, BsonValue value)
        {
            if (filter == null) return true;
            return and(filter, value);
        }

        // { <field>: { $eq: <value> } }
        static bool eq(BsonValue filter, BsonValue value)
        {
            if (object.Equals(filter, value)) return true;
            if (value is BsonArray array)
                return array.Contains(value);
            return false;
        }
        // {field: {$gt: value} }
        static bool gt(BsonValue filter, BsonValue value) => filter > value;
        static bool gte(BsonValue filter, BsonValue value) => filter >= value;
        static bool @in(BsonValue filter, BsonValue value)
        {
            var array = (BsonArray)filter;
            if (value == null) return false;
            foreach (var item in array)
            {
                if (Equals(item, value)) return true;
                if (item is BsonRegularExpression reg && reg.ToRegex().IsMatch(value.AsString))
                    return true;
            }
            return false;
        }
        static bool lt(BsonValue filter, BsonValue value) => filter < value;
        static bool lte(BsonValue filter, BsonValue value) => filter <= value;
        static bool ne(BsonValue filter, BsonValue value) => filter != value;
        static bool nin(BsonValue filter, BsonValue value)
        {
            var array = (BsonArray)filter;
            if (value == null) return true;
            foreach (var item in array)
                if (Equals(item, value))
                    return false;
            return true;
        }

        static bool and(BsonValue filter, BsonValue value)
        {
            foreach (BsonDocument item in (BsonArray)filter)
                if (!IsMatch(item, value)) return false;
            return true;
        }

    }
}
