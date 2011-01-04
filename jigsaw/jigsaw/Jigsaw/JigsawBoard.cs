using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Xml;
using jigsaw.Model;

namespace jigsaw.Jigsaw
{
    class JigsawBoard : Panel
    {
        private double totalArea = 0;
        private double ratio = 0;

        protected override Size MeasureOverride(Size availableSize)
        {
            double totalWidth = 0;
            double totalHeight = 0;

            //calculate the sum
            foreach (UIElement child in Children)
            {
                if ((child as ContentPresenter).Content is Table)
                {
                    Table t = (child as ContentPresenter).Content as Table;
                    totalArea += t.Size;
                }
            }

            ratio = availableSize.Height * availableSize.Width / totalArea;

            foreach (UIElement child in Children)
            {
                Table t = (child as ContentPresenter).Content as Table;
                double width = Math.Sqrt(t.Size * ratio);
                child.Measure(new Size(width, width));
                
                Size childSize = child.DesiredSize;
                totalWidth += childSize.Width;
                totalHeight += childSize.Height;
            }

            return new Size(totalWidth, totalHeight);
        }

         protected override Size ArrangeOverride( Size finalSize ) {
            Point currentPosition = new Point( );

            foreach( UIElement child in Children ) {
                Rect childRect = new Rect( currentPosition, child.DesiredSize );
                child.Arrange( childRect );
                currentPosition.Offset( childRect.Width, childRect.Height );
            }

            return new Size( currentPosition.X, currentPosition.Y );
        }
    }
}
