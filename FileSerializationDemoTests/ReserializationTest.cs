using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileSerializationDemo;
using FileSerializationDemo.Classes;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using NLog;
using System.Linq;

namespace FileSerializationDemoTests
{
    [TestClass]
    public class SerializationThenDeserializationTests
    {
        /// <summary>
        /// Serializes the Test-DB.
        /// Asserts true if the deserialized DB matches the Test-DB.
        /// Expected Result: True.
        /// </summary>
        [TestMethod]
        public void TestSerializationThenDeserialization()
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            RoomDataBase roomDB = RoomDataBase.GetTestDB();

            string expectedJson = JsonConvert.SerializeObject(roomDB, Formatting.Indented);

            roomDB.Serialize(FileDBEnums.SerializationType.ADD_DISCARDUNUSED); // usage 1/2

            RoomDataBase deserRoomDB = new();
            deserRoomDB = deserRoomDB.Deserialize<RoomDataBase>(1); // usage 2/2

            string actualJson = NullDBid(JsonConvert.SerializeObject(deserRoomDB, Formatting.Indented));

            logger.Info("\n\n +++Expected:\n" + expectedJson + "\n-------------------\n\n +++Received:\n" + actualJson);

            Assert.IsTrue(actualJson == expectedJson);
        }

        /// <summary>
        /// Tests whether change-recognition via the object-hash is working.
        /// </summary>
        [TestMethod]
        public void TestDeserializationThenSerialization()
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            RoomDataBase deserRoomDB = new();
            deserRoomDB = deserRoomDB.Deserialize<RoomDataBase>(1);

            string actualJson = JsonConvert.SerializeObject(deserRoomDB, Formatting.Indented);
            logger.Info("\n\n TestDeserializationThenSerialization()+++Received:\n" + actualJson);

            deserRoomDB.Serialize(FileDBEnums.SerializationType.ADD_DISCARDUNUSED);

            Assert.IsTrue(true);
        }

        /// <summary>
        /// When the object is serialized the first time, it will contain the correct the DBids.
        /// To check whether the object is the same after deserialization, the Newtonsoft.Json serialization output
        /// has to be changed such that DBid always appears to be 0.
        /// </summary>
        /// <param name="input">The input Json.</param>
        /// <returns>Output json where all DBid = 0.</returns>
        public static string NullDBid(string input)
        {
            return Regex.Replace(input, "\\\"DBid\\\":\\s\\d+", "\"DBid\": 0");
        }
    }

    [TestClass]
    public class Just
    {
        [TestMethod]
        public void JustSerialize()
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            RoomDataBase roomDB = RoomDataBase.GetTestDBWithNewList();
            Assert.IsTrue(roomDB.Serialize(FileDBEnums.SerializationType.ADD_DISCARDUNUSED));
        }

        [TestMethod]
        public void JustDeserialize()
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            RoomDataBase roomDB = new();
            roomDB = roomDB.Deserialize<RoomDataBase>(1);
            Assert.IsTrue(roomDB != default); // check manually.
        }
    }

    [TestClass]
    public class TestAddDiscardUnused
    {
        /// <summary>
        /// Serializes the Test-Database, then removes a Room-object and serializes again.
        /// If, upon deserialization, it is found that the database changed (the room was deleted in the database),
        /// asserts true.
        /// Expected Result: True.
        /// </summary>
        [TestMethod]
        public void SerializeThenSerialize()
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            RoomDataBase roomDB = RoomDataBase.GetTestDB();

            string expectedJson = JsonConvert.SerializeObject(roomDB, Formatting.Indented); // with comfy prison cell

            roomDB.Serialize(FileDBEnums.SerializationType.ADD_DISCARDUNUSED); // serialize with cpc
            roomDB.Rooms.Remove(roomDB.Rooms.Last()); // delete cpc
            roomDB.Serialize(FileDBEnums.SerializationType.ADD_DISCARDUNUSED); // serialize without cpc

            roomDB = roomDB.Deserialize<RoomDataBase>(1); // deserialize, then check if cpc is missing.
            string actualJson = NullDBid(JsonConvert.SerializeObject(roomDB, Formatting.Indented));

            logger.Info("\n\n +++NOT-Expected:\n" + expectedJson + "\n-------------------\n\n +++Received:\n" + actualJson);

            Assert.IsTrue(actualJson != expectedJson);
        }

        /// <summary>
        /// When the object is serialized the first time, it will contain the correct the DBids.
        /// To check whether the object is the same after deserialization, the Newtonsoft.Json serialization output
        /// has to be changed such that DBid always appears to be 0.
        /// </summary>
        /// <param name="input">The input Json.</param>
        /// <returns>Output json where all DBid = 0.</returns>
        public static string NullDBid(string input)
        {
            return Regex.Replace(input, "\\\"DBid\\\":\\s\\d+", "\"DBid\": 0");
        }
    }

    [TestClass]
    public class TestObjectLinQ
    {
        /// <summary>
        /// Serializes the Test-Database, then removes a Room-object and serializes again.
        /// If, upon deserialization, it is found that the database changed (the room was deleted in the database),
        /// asserts true.
        /// Expected Result: True.
        /// </summary>
        [TestMethod]
        public void DeserializeThenTest()
        {
            Logger logger = LogManager.GetCurrentClassLogger();

            RoomDataBase roomDB = new();
            roomDB = roomDB.Deserialize<RoomDataBase>(1); // deserialize, then call EditLightBulb() and check if the Item changed in both room 1 and 2.
            RoomDataBase.EditLightBulbItem(roomDB);
            string resultJson = NullDBid(JsonConvert.SerializeObject(roomDB, Formatting.Indented));

            logger.Info("\n\n +++DeserializeThenTest()-Result-Json:\n" + resultJson);

            Assert.IsTrue(!RoomDataBase.HasOldLightBulb(roomDB));
        }

        /// <summary>
        /// When the object is serialized the first time, it will contain the correct the DBids.
        /// To check whether the object is the same after deserialization, the Newtonsoft.Json serialization output
        /// has to be changed such that DBid always appears to be 0.
        /// </summary>
        /// <param name="input">The input Json.</param>
        /// <returns>Output json where all DBid = 0.</returns>
        public static string NullDBid(string input)
        {
            return Regex.Replace(input, "\\\"DBid\\\":\\s\\d+", "\"DBid\": 0");
        }
    }
}