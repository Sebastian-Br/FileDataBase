using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileSerializationDemo.Classes;

namespace FileSerializationDemoTests
{
    public class Room
    {
        public Room()
        {
        }

        public List<Item> Items { get; set; }

        public string RoomName { get; set; }
    }
}
