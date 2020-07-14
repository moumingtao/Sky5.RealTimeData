using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Driver;
using Sky5.RealTimeData.Logic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sky5.RealTimeData.Test
{
    [TestClass]
    public class FilterTest
    {
        readonly IMongoDatabase db;

        public FilterTest()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            db = client.GetDatabase("test");
        }

        [TestMethod]
        public async Task Eq()
        {
            var id = new ObjectId("5f0931cd78e4773b0c6bdb67");
            var doc = BsonDocument.Parse("{name:'mingtao', age:23, numbers:[1, 3, 6, 7, 8]}");
            doc["_id"] = id;
            var collect = db.GetCollection<BsonDocument>("testOne");

            async Task Check(string filter, bool m)
            {
                var f = BsonDocument.Parse(filter);
                Assert.AreEqual(m, await collect.Find(f).AnyAsync());
                Assert.AreEqual(m, FilterUtils.IsMatch(f, doc));
            }

            try
            {
                await collect.InsertOneAsync(doc);
                await Check("{name:'mingtao'}", true);
                await Check("{age:23}", true);
                await Check("{age:'23'}", false);

                // eq
                await Check("{age:{$eq:23}}", true);
                await Check("{age:{$eq:23.0}}", true);
                await Check("{age:{$eq:24}}", false);

                // gt
                await Check("{age:{$gt:22}}", true);
                await Check("{age:{$gt:23}}", false);

                // gte
                await Check("{age:{$gte:22}}", true);
                await Check("{age:{$gte:23}}", true);
                await Check("{age:{$gte:24}}", false);
            }
            finally
            {
                await db.DropCollectionAsync(collect.CollectionNamespace.CollectionName);
            }
        }
    }
}
