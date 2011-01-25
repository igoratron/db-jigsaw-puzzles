using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using jigsaw.View.Jigsaw;

namespace jigsaw.TypeConverters
{
    class TabCenterConverter:IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 2)
            {
                Piece thisPiece = (Piece)values[0];
                Piece otherPiece = (Piece)values[1];

                Point centre = new Point();

                double angle = Math.Atan2(otherPiece.Y + otherPiece.Height / 2 - thisPiece.Y - thisPiece.Height / 2, otherPiece.X + otherPiece.Width / 2 - thisPiece.X - thisPiece.Width / 2);
                System.Diagnostics.Debug.Assert(!double.IsNaN(angle), "Angle is NAN");
                double r1sqrd = thisPiece.Width * thisPiece.Width / (4 * Math.Cos(angle) * Math.Cos(angle));
                double r2sqrd = thisPiece.Height * thisPiece.Height / (4 * Math.Sin(angle) * Math.Sin(angle));

                double dsqrd = Math.Min(r1sqrd, r2sqrd);

                centre.X = Math.Sqrt(dsqrd) * Math.Cos(angle) + thisPiece.Width / 2;
                centre.Y = Math.Sqrt(dsqrd) * Math.Sin(angle) + thisPiece.Height / 2;
                return centre;
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
