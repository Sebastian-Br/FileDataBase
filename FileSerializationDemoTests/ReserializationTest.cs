using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileSerializationDemo;
using FileSerializationDemo.Classes;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using NLog;

namespace FileSerializationDemoTests
{
    [TestClass]
    public class ReserializationTest
    {
        [TestMethod]
        public void TestReserialization()
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            RoomDataBase roomDB = RoomDataBase.GetTestDB();

            string expectedJson = JsonConvert.SerializeObject(roomDB, Formatting.Indented);

            roomDB.Serialize(); // usage 1/2

            RoomDataBase deserRoomDB = new();

            deserRoomDB = deserRoomDB.Deserialize<RoomDataBase>(1); // usage 2/2

            string actualJson = NullDBid(JsonConvert.SerializeObject(deserRoomDB, Formatting.Indented));

            logger.Info("\n\n +++Expected:\n" + expectedJson + "\n-------------------\n\n +++Received:\n" + actualJson);
            Assert.IsTrue(actualJson == expectedJson);
        }

        public static string NullDBid(string input)
        {
            return Regex.Replace(input, "\\\"DBid\\\":\\s\\d+", "\"DBid\": 0");
        }
    }
}
