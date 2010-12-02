using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using DavidWynne.TreeMap.Silverlight.Model;

namespace jigsaw
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Random random = new Random();
        public MainWindow()
        {
            InitializeComponent();

            generateTreeMap(loadDbDescription());
        }

        public List<InputValue> loadDbDescription()
        {
            List<InputValue> result = new List<InputValue>();
            XmlTextReader reader = new XmlTextReader("../../database.xml");

            while (reader.Read())
            {
                if (XmlNodeType.Element.Equals(reader.NodeType) &&
                    "table".Equals(reader.Name))
                {
                    string name = null;
                    int size = 0;
                    string relationTo = null;
                    while (reader.MoveToNextAttribute())
                    {
                        switch (reader.Name)
                        {
                            case "size":
                                size = int.Parse(reader.Value);
                                break;
                            case "name":
                                name = reader.Value;
                                break;
                            case "foreignKey":
                                relationTo = reader.Value;
                                break;
                        }
                    }
                    result.Add(new InputValue(name, size, relationTo));
                    dbView.Items.Add(new InputValue(name, size, relationTo));
               } 
            }

            return result;
        }

        public void generateTreeMap(List<InputValue> values)
        {
            List<Sector> sectors = TreeMap.Plot(values, new Size(400, 300));

            for(int i = 0; i < sectors.Count; i++)
            {
                Piece p = new Piece();
                p.Rect = sectors[i].Rect;
                p.Direction = sectors[i].RelationTo;
                p.Color = getBrush(i);
                canvas.Children.Add(p);
            }
        }

        private Brush getBrush(Int32 i)
        {
            Brush[] brushes = {
                                  Brushes.Red, Brushes.Lime, Brushes.Purple, Brushes.Orange, Brushes.Yellow, Brushes.Blue, Brushes.Green, 
                              };

            return brushes[i % brushes.Length];
        }
    }
}