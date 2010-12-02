using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace jigsaw
{
    class DirectionConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.Length == 2 && value[0] is String && value[1] is Rect)
            {
                String direction = ((String)value[0]).ToLower();
                Rect rect = ((Rect)value[1]);

                switch (direction)
                {
                    case "north":
                        return new Point(rect.Width / 2, -10);
                    case "east":
                        return new Point(rect.Width + 10, rect.Height / 2);
                    case "south":
                        return new Point(rect.Width / 2, rect.Height + 10);
                    case "west":
                        return new Point(-10, rect.Height / 2);
                    case "none":
                        return new Point(20, 20);
                    default:
                        throw new ArgumentException("Invalid direction");
                }
            }
            else
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
