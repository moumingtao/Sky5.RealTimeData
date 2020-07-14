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
        static readonly Dictionary<string, Func<BsonValue, BsonValue, bool>> methods = new Dictionary<string, Func<BsonValue, BsonValue, bool>>();
        static FilterUtils()
        {
            Register(and);
            Register(eq);
            Register(gt);
            Register(gte);
            Register(@in);
            Register(lt);
            Register(lte);
            Register(ne);
            Register(nin);

        }
        static void Register(Func<BsonValue, BsonValue, bool> method) => methods.Add("$" + method.Method.Name, method);

        public static bool IsMatch(BsonDocument filter, BsonValue value)
        {
            if (filter == null) return true;
            foreach (var item in filter)
            {
                if (methods.TryGetValue(item.Name, out var method))
                {
                    if (!method(item.Value, value)) return false;
                }
                else if (((BsonDocument)value).TryGetValue(item.Name, out var val))
                {
                    if (item.Value is BsonDocument doc)
                    {
                        if (!IsMatch(doc, val)) return false;
                    }
                    else if (!item.Value.Equals(val)) return false;
                }
                else return false;
            }
            return true;
        }
        

        // { <field>: { $eq: <value> } }
        static bool eq(BsonValue filter, BsonValue value)
        {
            if (object.Equals(filter, value)) return true;
            if (value is BsonArray array)
                return array.Contains(value);
            return filter.Equals(value);
        }
        // {field: {$gt: value} }
        static bool gt(BsonValue filter, BsonValue value) => value > filter;
        static bool gte(BsonValue filter, BsonValue value) => value >= filter;
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
        static bool lt(BsonValue filter, BsonValue value) => value < filter;
        static bool lte(BsonValue filter, BsonValue value) => value <= filter;
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
