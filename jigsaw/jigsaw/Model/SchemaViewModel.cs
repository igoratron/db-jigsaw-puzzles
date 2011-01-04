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
        public ObservableCollection<Table> Schema { get; set; }
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
                        Schema.Add(new Table(reader.GetAttribute("name"), reader.GetAttribute("size")));
                    }
                }
            }
        }

        public SchemaViewModel()
        {
            Schema = new ObservableCollection<Table>();
        }


    }
}
