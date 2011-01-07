using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Xml;

namespace jigsaw.Model
{
    class SchemaViewModel
    {
        public ObservableCollection<Table> Schema { get; private set; }
        private Dictionary<String, Table> StoredTables { get;  set; }

        public String XmlSchemaPath
        {
            set
            {
                XmlTextReader reader = new XmlTextReader("../../" + value);
                reader.Read();
                while (reader.Read())
                {
                    if(XmlNodeType.Element.Equals(reader.NodeType) && "table".Equals(reader.Name))
                    {
                        Table t = new Table(reader.GetAttribute("name"), reader.GetAttribute("size"));
                        Schema.Add(t);
                        StoredTables.Add(t.Name, t);
                        
                        if (!reader.IsEmptyElement) //is made up from other tables
                        {
                            while (reader.Read() && !XmlNodeType.EndElement.Equals(reader.NodeType))
                            {
                                if (XmlNodeType.Element.Equals(reader.NodeType) && "table".Equals(reader.Name))
                                {
                                    //TODO: add sizes
                                    t.From.Add(StoredTables[reader.GetAttribute("name")]);
                                }
                            }
                        }
                    }
                }
            }
        }

        public SchemaViewModel()
        {
            Schema = new ObservableCollection<Table>();
            StoredTables = new Dictionary<string, Table>();
        }
    }
}
