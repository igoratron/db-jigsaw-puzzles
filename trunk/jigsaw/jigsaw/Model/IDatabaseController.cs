using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jigsaw.Model
{
    public interface IDatabaseController
    {
        List<Table> GetSchema();

        Table JoinTables(Table a, Table b);

        bool IsValidJoin(Table a, Table b);
    }
}
