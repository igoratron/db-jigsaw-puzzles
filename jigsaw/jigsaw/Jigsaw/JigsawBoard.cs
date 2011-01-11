using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Xml;
using jigsaw.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace jigsaw.Jigsaw
{
    class JigsawBoard : Panel
    {
        private const int MARGIN = 20;
        private const double RATIO = 300;
        private List<Table> renderingOrder = new List<Table>();

        public static readonly DependencyProperty XProperty = DependencyProperty.RegisterAttached("X", typeof(double),
                                typeof(JigsawBoard),
                                new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static readonly DependencyProperty YProperty = DependencyProperty.RegisterAttached("Y", typeof(double),
                                typeof(JigsawBoard),
                                new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.AffectsParentArrange));

        [TypeConverter(typeof(LengthConverter)), AttachedPropertyBrowsableForChildren]
        public static double GetX(UIElement element)
        {
            if (element == null) { throw new ArgumentNullException("element"); }
            return (double)element.GetValue(XProperty);
        }

        [TypeConverter(typeof(LengthConverter)), AttachedPropertyBrowsableForChildren]
        public static void SetX(UIElement element, double length)
        {
            if (element == null) { throw new ArgumentNullException("element"); }
            element.SetValue(XProperty, length);
        }

        [TypeConverter(typeof(LengthConverter)), AttachedPropertyBrowsableForChildren]
        public static double GetY(UIElement element)
        {
            if (element == null) { throw new ArgumentNullException("element"); }
            return (double)element.GetValue(YProperty);
        }

        [TypeConverter(typeof(LengthConverter)), AttachedPropertyBrowsableForChildren]
        public static void SetY(UIElement element, double length)
        {
            if (element == null) { throw new ArgumentNullException("element"); }
            element.SetValue(YProperty, length);
        }
        
        protected override Size MeasureOverride(Size availableSize)
        {
            double totalWidth = 0;
            double totalHeight = 0;

            foreach (UIElement child in Children)
            {
                Table t = ((TreeViewItem)child).DataContext as Table;
                double width = Math.Sqrt(t.Size * RATIO);

                child.Measure(new Size(width, width));

                Size childSize = child.DesiredSize;
                totalWidth += childSize.Width;
                totalHeight += childSize.Height;
            }

            return new Size(totalWidth, totalHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Point currentPosition = new Point();

            foreach (UIElement child in Children)
            {
                Table t = ((TreeViewItem)child).DataContext as Table;

                if (t.ForeignKey.Count != 0)
                {
                    currentPosition.X -= MARGIN;
                }

                Rect childRect = new Rect(currentPosition, child.DesiredSize);

                child.Arrange(childRect);

                //HACK: force the piece to have correct X and Y coordinates
                TreeViewItem current = (TreeViewItem)child;
                Piece p = (Piece)current.Template.FindName("piece", current);
                p.X = currentPosition.X;
                p.Y = currentPosition.Y;
                //end HACK

                //solution 1?
                JigsawBoard.SetX(child, currentPosition.X);
                JigsawBoard.SetY(child, currentPosition.Y);
               

                currentPosition.Offset(childRect.Width + MARGIN, 0);
            }

            return new Size(currentPosition.X, currentPosition.Y);
        }
    }
}
