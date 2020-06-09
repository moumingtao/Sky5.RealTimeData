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

        [TestMethod]
        public void TestMethod2()
        {
            var array1 = new BsonArray(new int[] { 1, 2, 3 });
            var array2 = new BsonArray(new int[] { 1, 2, 3 });
            Assert.IsTrue(array1.Equals(array2));
            new BsonInt32(1).Equals(array1);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var doc = new { Name = (string)null }.ToBsonDocument();
        }

    }
}
