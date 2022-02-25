using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileSerializationDemo;
using FileSerializationDemo.Classes;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using NLog;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using FileSerializationDemo.ObjectFileSystemSerializer;
using EZDB_Tests.SimpleTestClasses;

namespace FileSerializationDemoTests
{
    [TestClass]
    public class Just
    {
        [TestMethod]
        public void JustSerialize()
        {
            RoomDataBase roomDB = RoomDataBase.GetTestDB();
            SerializationMain serializationMain = new();
            Assert.IsTrue(serializationMain.SerializeRoot(roomDB));
        }

        [TestMethod]
        public void JustSerializeX2()
        {
            RoomDataBase roomDB = RoomDataBase.GetTestDB();
            SerializationMain serializationMain = new();
            Assert.IsTrue(serializationMain.SerializeRoot(roomDB) && serializationMain.SerializeRoot(roomDB));
        }

        [TestMethod]
        public void JustSerializeSimpleDB()
        {
            SimpleClassDB simpleDB = SimpleClassDB.GetTestDB();
            SerializationMain serializationMain = new();
            Assert.IsTrue(serializationMain.SerializeRoot(simpleDB));
        }
    }

    [TestClass]
    public class ReferencesTests
    {
        [TestMethod]
        public void SaveLoadAndTest_ListReference_nonempty()
        {
            // Set up classes
            RoomDataBase roomDB = new();
            roomDB.DataBaseName = "ListReference_nonempty_test";
            roomDB.Rooms = new();
            Room room1 = new();
            room1.RoomName = "Reference_testroom";
            room1.Items = new();
            Item item1 = new() { ComposedOf = null, Generalization = null, ItemName = "TestItem", ItemWeight = 1.0 };
            Room room2 = room1;

            //Serialization
            SerializationMain s = new();
            s.SerializeRoot(roomDB);

            //Deserialization: TODO

            Assert.IsTrue(true);
        }
    }

    [TestClass]
    public class Unrelated
    {
        [TestMethod]
        public void TestListCopiesNestedReferences()
        {
            Room room1 = new();
            Item item1 = new() { ItemWeight = 5.5, ItemName = "item1" };
            Item item2 = new() { ItemWeight = 77, ItemName = "item2" };
            room1.Items = new();
            room1.Items.Add(item1);
            room1.Items.Add(item2);

            Room room2 = room1;
            room2.Items.Add(new() { ItemName = "123" });
            Assert.IsTrue(room1 == room2 && room2.Items == room1.Items && room1.Items.First() == room2.Items.First());
            /*EXPLANATION:
             *          Reference 'room' is the same.
             *                      References, room.Items are the same.
             *                                                          Reference to the first item in each room is the same.
             */
        }

        [TestMethod]
        public void TestElementAt()
        {
            List<int> intList = new();
            intList.Add(1);
            intList.Add(2);
            intList.Add(3);
            IEnumerable<int> collection = intList;
            Assert.IsTrue((int)collection.ElementAt(0) == 1);
        }

        [TestMethod]
        public void TestWalkObject()
        {
            RoomDataBase roomDB = RoomDataBase.GetTestDB();
            List<PropertyLinq> propertyLinqs = new();
            propertyLinqs.Add(new() { PropertyName = "DataBaseName", DBid = 1 });
            object returnObject = ReflectionX.WalkObject(propertyLinqs, roomDB);
            
            Assert.IsTrue(roomDB.DataBaseName == (string)returnObject);
        }
    }
}