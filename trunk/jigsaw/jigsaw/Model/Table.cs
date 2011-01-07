using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jigsaw.Model
{
    class Table
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
        public List<Table> KeyRelations
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
            KeyRelations = new List<Table>();
            From = new List<Table>();
            Size = int.Parse(size);
            Name = name;
        }
    }
}
