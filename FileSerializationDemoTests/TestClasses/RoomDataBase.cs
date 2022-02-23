using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileSerializationDemo.Classes;

namespace FileSerializationDemoTests
{
    /// <summary>
    /// The Root-Class that holds the a list of the Test-Class "Room".
    /// </summary>
    public class RoomDataBase
    {
        public RoomDataBase()
        {
            Rooms = new();
        }

        public List<Room> Rooms { get; set; }

        public string DataBaseName { get; set; }

        public static RoomDataBase GetTestDB()
        {
            RoomDataBase roomDB = new();
            roomDB.DataBaseName = "TestDBName";

            Room room1 = new();
            room1.RoomName = "Living Room";

            Item television = new();
            television.ItemName = "Television";
            Item Screen = new();
            Screen.ItemName = "LCD-Display";
            Screen.ItemWeight = 1.0;
            Screen.Generalization = new() { ItemName = "Display", ComposedOf = null};
            Screen.ComposedOf = null;
            television.ComposedOf.Add(Screen);
            Item Receiver = new();
            Receiver.ItemName = "Television-Receiver";
            Receiver.ItemWeight = 0.875;
            television.ComposedOf.Add(Receiver);
            television.SetWeight();
            room1.Items = new();
            room1.Items.Add(television); // television is first item. First composed-of, screen, should have "Display" generalization.

            Item LightBulb = new();
            LightBulb.ItemName = "50W Lightbulb";
            Item InertGas = new();
            InertGas.ItemName = "N2 Inert Gas";
            InertGas.ItemWeight = 0.000001;
            InertGas.Generalization = new() { ItemName = "Inert Gas" };
            Item BulbCasing = new();
            BulbCasing.ItemName = "Light Bulb Glass Casing";
            BulbCasing.ItemWeight = 0.02;
            LightBulb.ComposedOf.Add(BulbCasing);
            LightBulb.SetWeight();
            room1.Items.Add(LightBulb);

            roomDB.Rooms.Add(room1);

            Room PrisonCell = new();
            PrisonCell.RoomName = "Comfy Prison Cell";
            PrisonCell.Items = new();
            PrisonCell.Items.Add(LightBulb);

            roomDB.Rooms.Add(PrisonCell);

            return roomDB;
        }

        public static void EditLightBulbItem(RoomDataBase roomDataBase)
        {
            foreach(Room room in roomDataBase.Rooms)
            {
                if(room.RoomName == "Living Room")
                {
                    foreach(Item item in room.Items)
                    {
                        if(item.ItemName == "50W Lightbulb")
                        {
                            item.ComposedOf = null;
                            item.ItemName = "20W Energy Saving Lightbulb";
                            return;
                        }
                    }
                }
            }
        }

        public static bool HasOldLightBulb(RoomDataBase roomDataBase)
        {
            foreach (Room room in roomDataBase.Rooms)
            {
                if (room.RoomName == "Living Room")
                {
                    foreach (Item item in room.Items)
                    {
                        if (item.ItemName == "50W Lightbulb")
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static RoomDataBase GetTestDBWithNewList()
        {
            RoomDataBase roomDB = new();
            roomDB.DataBaseName = "TestDB-NewListTest";

            Room room1 = new();
            room1.RoomName = "My Room";
            roomDB.Rooms.Add(room1);

            return roomDB;
        }
    }
}