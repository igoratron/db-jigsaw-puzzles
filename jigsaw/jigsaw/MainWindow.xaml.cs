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
                    String name = null;
                    Int32 size = 0;
                    while (reader.MoveToNextAttribute())
                    {
                        switch (reader.Name)
                        {
                            case "size":
                                size = Int32.Parse(reader.Value);
                                break;
                            case "name":
                                name = reader.Value;
                                break;
                        }
                    }
                    result.Add(new InputValue(name, size));
                }
            }

            return result;
        }

        public void generateTreeMap(List<InputValue> values)
        {
            List<Sector> sectors = TreeMap.Plot(values, new Size(400, 300));

            foreach (Sector sector in sectors)
            {
                Piece p = new Piece();
                p.Rect = sector.Rect;
                p.Direction = "South";
                p.Color = getRandomBrush();
                canvas.Children.Add(p);
            }
        }

        private Brush getRandomBrush()
        {
            Brush[] brushes = {
                                  Brushes.Red, Brushes.Orange, Brushes.Yellow, Brushes.Lime, Brushes.Green, Brushes.Purple, Brushes.Blue
                              };

            return brushes[random.Next(brushes.Length)];
        }
    }
}