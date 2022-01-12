using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileSerializationDemo;
using FileSerializationDemo.Classes;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace FileSerializationDemoTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestReserialization()
        {
            RoomDataBase roomDB = RoomDataBase.GetTestDB();

            string expectedJson = JsonConvert.SerializeObject(roomDB, Formatting.Indented);

            roomDB.Serialize();
            RoomDataBase deserRoomDB = new();
            deserRoomDB = deserRoomDB.Deserialize<RoomDataBase>(1);

            string actualJson = NullDBid(JsonConvert.SerializeObject(deserRoomDB, Formatting.Indented));

            //logger.Info("\n\n +++Expected:\n" + expectedJson + "\n-------------------\n\n +++Received:\n" + actualJson);
            Assert.IsTrue(actualJson == expectedJson);
        }

        public static string NullDBid(string input)
        {
            return Regex.Replace(input, "\\\"DBid\\\":\\s\\d+", "\"DBid\": 0");
        }
    }
}
