using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace jigsaw
{
    class Utils
    {

        /// <summary>
        /// Find a parent element of a given type and return the first match
        /// </summary>
        /// <typeparam name="T">Type of the parent</typeparam>
        /// <param name="current">The element which parent we want to find</param>
        /// <returns></returns>
        public static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        #region Color stuff (refactor?)
        public static void HSLtoRGB(double hue, double saturation, double luminance, out double red, out double green, out double blue)
        {
            double q;
            double p;
            if (luminance < 0.5)
            {
                q = luminance * (1.0 + saturation);
            }
            else
            {
                q = luminance + saturation - (luminance * saturation);
            }
            p = 2 * luminance - q;
            double hk = hue / 360.0;
            double tr = hk + 1.0 / 3.0;
            double tg = hk;
            double tb = hk - 1.0 / 3.0;
            tr = Normalize(tr);
            tg = Normalize(tg);
            tb = Normalize(tb);
            red = ComputeColor(q, p, tr);
            green = ComputeColor(q, p, tg);
            blue = ComputeColor(q, p, tb);
        }

        private static double ComputeColor(double q, double p, double tc)
        {
            if (tc < 1.0 / 6.0)
            {
                return p + ((q - p) * 6.0 * tc);
            }
            if (tc < 0.5)
            {
                return q;
            }
            if (tc < 2.0 / 3.0)
            {
                return p + ((q - p) * 6.0 * (2.0 / 3.0 - tc));
            }
            return p;
        }

        private static double Normalize(double tr)
        {
            if (tr < 0)
            {
                return tr + 1.0;
            }
            if (tr > 1.0)
            {
                return tr - 1.0;
            }
            return tr;
        }
        #endregion
    }
}
