using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using jigsaw.Model;
using System.Windows.Controls;
using jigsaw.View.Jigsaw;

namespace jigsaw.TypeConverters
{
    class TableToPieceConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<Piece> result = new List<Piece>();

            if (values.Length == 2)
            {
                List<Table> tables = (List<Table>)values[0];
                if (tables.Count > 0)
                {
                    Piece piece = (Piece)values[1];
                    TreeView itemsControl = Utils.FindAncestor<TreeView>(piece);
                    foreach (Table t in tables)
                    {
                        //TreeViewItem templateParent = (TreeViewItem)itemsControl.ItemContainerGenerator.ContainerFromItem(t);
                        //Piece p = (Piece)templateParent.Template.FindName("piece", templateParent);
                        //System.Diagnostics.Debug.Assert(p != null, "Unable to find a Piece for given Table");
                        //result.Add(p);
                    }
                }
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
