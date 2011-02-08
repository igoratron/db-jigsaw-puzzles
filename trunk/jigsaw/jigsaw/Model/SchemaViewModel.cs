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
        public ObservableCollection<ObservableCollection<Table>> Schema { get; private set; }

        private const String TABLE = "table";
        private const String F_KEY = "foreignKey";
        private const String NAME = "name";
        private const String SIZE = "size";

        public String XmlSchemaPath
        {
            set
            {
                String path = "../../" + value;
                List<List<Table>> reordered = Reorder(ParseXML(path).Values.ToList());

                Schema = new ObservableCollection<ObservableCollection<Table>>();

                //resolve dependencies for drawing
                foreach(List<Table> group in reordered) 
                {
                    List<Table> collection = new List<Table>();
                    collection.AddRange(ResolveDependencies(group[0]));
                    Schema.Add(new ObservableCollection<Table>(collection));
                }
            }
        }

        public String DbSchema
        {
            set
            {
                //fixe: path to database / connection string
                List<List<Table>> reordered = Reorder(new MySQLDriver().GetSchema());

                Schema = new ObservableCollection<ObservableCollection<Table>>();

                //resolve dependencies for drawing
                foreach (List<Table> group in reordered)
                {
                    List<Table> collection = new List<Table>();
                    collection.AddRange(ResolveDependencies(group[0]));
                    Schema.Add(new ObservableCollection<Table>(collection));
                }
            }
        }

        public SchemaViewModel()
        {
            Schema = new ObservableCollection<ObservableCollection<Table>>();
        }

        /// <summary>
        /// Takes in a XML file and outputs a coresponding list with tables
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private Dictionary<string, Table> ParseXML(String path)
        {
            Dictionary<string, Table> instantiated = new Dictionary<string, Table>();
            Dictionary<Table, List<String>> foreignNames = new Dictionary<Table, List<string>>();
            Dictionary<Table, List<String>> fromNames = new Dictionary<Table, List<string>>();

            XmlTextReader reader = new XmlTextReader(path);
            while (reader.Read())
            {
                if (XmlNodeType.Element.Equals(reader.NodeType) && TABLE.Equals(reader.Name))
                {
                    Table t = new Table(reader.GetAttribute(NAME), reader.GetAttribute(SIZE));
                    instantiated.Add(t.Name, t);

                    if (!reader.IsEmptyElement) //is made up from other tables
                    {
                        while (reader.Read() && !XmlNodeType.EndElement.Equals(reader.NodeType))
                        {
                            if (XmlNodeType.Element.Equals(reader.NodeType) && TABLE.Equals(reader.Name))
                            {
                                String fromName = reader.GetAttribute(NAME);
                                if (!fromNames.ContainsKey(t))
                                {
                                    fromNames.Add(t,new List<string>());
                                }
                                fromNames[t].Add(fromName);
                                    
                            }
                            else if (XmlNodeType.Element.Equals(reader.NodeType) && F_KEY.Equals(reader.Name))
                            {
                                String foreignName = reader.GetAttribute(NAME);

                                if (!foreignNames.ContainsKey(t))
                                {
                                    foreignNames.Add(t, new List<string>());
                                }

                                foreignNames[t].Add(foreignName);
                            }
                        }
                    }
                }
            }

            //convert string names into table objects
            foreach (Table t in foreignNames.Keys)
            {
                foreach(String tableName in foreignNames[t]) 
                {
                    t.ForeignKey.Add(instantiated[tableName]);
                }
            }

            foreach (Table t in fromNames.Keys)
            {
                foreach (String tableName in fromNames[t])
                {
                    t.From.Add(instantiated[tableName]);
                }
            }

            return instantiated;
        }

        /// <summary>
        /// Reorders elements so that they are grouped by the foreignKey Tables (foreignKey on top)
        /// </summary>
        /// <param name="tables"></param>
        /// <returns></returns>
        private List<List<Table>> Reorder(List<Table> tables)
        {
            List<List<Table>> result = new List<List<Table>>();
            /// <summary>
            /// Maps Tables to integers in the resulting list;
            /// </summary>
            Dictionary<Table, int> _map = new Dictionary<Table, int>();

            foreach (Table t in tables)
            {
                if (!_map.ContainsKey(t))
                {
                    _map.Add(t, result.Count);
                    result.Add(new List<Table>());
                }
                result[_map[t]].Add(t);
                
                if (t.ForeignKey.Count > 0)
                {
                    foreach (Table foreignTable in t.ForeignKey)
                    {
                        if (!_map.ContainsKey(foreignTable))
                        {
                            _map.Add(foreignTable, result.Count - 1);
                            result[_map[foreignTable]].Add(t);
                        }
                        else
                        {
                            //copy all tables from the resuls collection to the new "bucket"
                            //and update all entires in the map to point to the new "bucket"

                            foreach(Table existing in result[_map[foreignTable]]) 
                            {
                                result[result.Count - 1].Add(existing);
                            }

                            //cant remove or the map will get all messed up. Remove later
                            result[_map[foreignTable]] = null;

                            //update all entires pointing to this 
                            int currentIndex = _map[foreignTable];
                            for(int i = 0; i < _map.Count; i++)
                            {
                                KeyValuePair<Table, int> entry = _map.ElementAt(i);
                                if (entry.Value == currentIndex)
                                {
                                    _map[entry.Key] = result.Count - 1; 
                                }
                            }
                        }
                    }
                }
            }

            //remove all elements that have been put into some other group
            for(int i = result.Count - 1; i >= 0; i--) 
            {
                if (result[i] == null)
                {
                    result.RemoveAt(i);
                }
            }

            return result;
        }

        private List<Table> ResolveDependencies(Table table)
        {
            List<Table> result = new List<Table>();

            foreach (Table key in table.ForeignKey)
            {
                result.AddRange(ResolveDependencies(key));
            }

            result.Add(table);

            return result;
        }
    }
}
