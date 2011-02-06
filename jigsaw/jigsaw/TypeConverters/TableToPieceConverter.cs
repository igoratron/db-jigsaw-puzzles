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
    class TableToPieceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<Piece> result = new List<Piece>();
            
            List<Table> tables = (List<Table>)value;
            if (tables.Count > 0)
            {
                JigsawTreemapContainer itemsControl = Utils.FindAncestor<JigsawTreemapContainer>(JigsawTreemap.getChild(tables[0]));
                foreach (Table t in tables)
                {
                    Piece p = JigsawTreemap.getChild(t);
                    System.Diagnostics.Debug.Assert(p != null, "Unable to find a Piece for given Table");
                    result.Add(p);
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
