using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jigsaw.Model
{
    public class Table
    {
        public String Name
        {
            get;
            set;
        }
        public int Size
        {
            get;
            set;
        }
        public List<Table> ForeignKey
        {
            get;
            private set;
        }
        public List<Table> From
        {
            get;
            private set;
        }

        public Table(String name, String size)
        {
            ForeignKey = new List<Table>();
            From = new List<Table>();
            Size = int.Parse(size);
            Name = name;
        }
    }
}
