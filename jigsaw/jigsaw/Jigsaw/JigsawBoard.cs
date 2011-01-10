using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Xml;
using jigsaw.Model;
using System.Collections.ObjectModel;

namespace jigsaw.Jigsaw
{
    class JigsawBoard : Panel
    {
        private const int MARGIN = 20;
        private const double RATIO = 300;
        private List<Table> renderingOrder = new List<Table>();


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

         protected override Size ArrangeOverride( Size finalSize ) {
            Point currentPosition = new Point( );

            foreach( UIElement child in Children ) {
                Table t = ((TreeViewItem)child).DataContext as Table;
                
                if (t.ForeignKey.Count != 0)
                {
                    currentPosition.X -= MARGIN;
                }

                Rect childRect = new Rect( currentPosition, child.DesiredSize );
                
                child.Arrange( childRect );

                //HACK: force the piece to have correct X and Y coordinates
                TreeViewItem current = (TreeViewItem)child;
                Piece p = (Piece)current.Template.FindName("piece", current);
                p.X = currentPosition.X;
                p.Y = currentPosition.Y;
                //end HACK

                currentPosition.Offset( childRect.Width + MARGIN, 0);
            }

            return new Size( currentPosition.X, currentPosition.Y );
        }
    }
}
