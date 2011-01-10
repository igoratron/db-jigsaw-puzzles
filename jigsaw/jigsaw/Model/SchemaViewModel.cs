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
                //parse
                XmlTextReader reader = new XmlTextReader("../../" + value);
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
                                    //TODO: what if encountered a table that is not in Stored tables?
                                    t.From.Add(StoredTables[reader.GetAttribute("name")]);
                                }
                                else if (XmlNodeType.Element.Equals(reader.NodeType) && "foreignKey".Equals(reader.Name))
                                {
                                    t.ForeignKey.Add(StoredTables[reader.GetAttribute("name")]);
                                }
                            }
                        }
                    }
                }

                //reorder
                for(int i = 0; i < Schema.Count; i++)
                {
                    Table t = Schema[i];
                    if (t.ForeignKey.Count != 0)
                    {
                        Schema.RemoveAt(i);
                        Schema.Insert(Schema.IndexOf(t.ForeignKey[0])+1, t);
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
