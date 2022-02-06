using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileSerializationDemo.Classes;

namespace FileSerializationDemoTests
{
    public class Item
    {
        public Item()
        {
            ComposedOf = new();
        }

        public Item Generalization { get; set; }

        //[FileDataBaseIgnore] test successful!
        public double ItemWeight { get; set; }

        public List<Item> ComposedOf { get; set; }
        public string ItemName { get; set; }

        public void SetWeight()
        {
            if(ComposedOf == null)
            {
                ItemWeight = 0.0;
            }
            else
            {
                double weight = 0;
                foreach(Item item in ComposedOf)
                {
                    weight += item.ItemWeight;
                }

                ItemWeight = weight;
            }
        }
    }
}