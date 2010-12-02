using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DavidWynne.TreeMap.Silverlight.Model;

namespace jigsaw.Model
{
    class GroupSector : Sector
    {
        public List<Sector> Sectors
        {
            get;
            set;
        }

        public GroupSector() {
            Sectors = new List<Sector>();
        }
    }
}
