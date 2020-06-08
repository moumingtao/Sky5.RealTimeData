using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Sky5.RealTimeData.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("name", "Tom");
            var doc1 = BsonDocument.Parse("{'name', '233'}");
        }
    }
}
