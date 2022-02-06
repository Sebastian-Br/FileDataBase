using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileSerializationDemo;
using FileSerializationDemo.Classes;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using NLog;
using System.Linq;
using System.Collections.Generic;
using FileSerializationDemo.ObjectFileSystemSerializer;

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
    }
}