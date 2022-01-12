using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileSerializationDemo.Classes;

namespace FileSerializationDemoTests
{
    public class Room : FileDataBase
    {
        public Room()
        {
            Items = new();
        }

        public List<Item> Items { get; set; }

        public string RoomName { get; set; }
    }
}
