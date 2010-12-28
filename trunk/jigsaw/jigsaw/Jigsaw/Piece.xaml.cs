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
using System.Windows.Media.Effects;


namespace jigsaw
{
    /// <summary>
    /// Interaction logic for Piece.xaml
    /// </summary>
    public partial class Piece : UserControl
    {
        //Dependency properties
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Brush), typeof(Piece));
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(Piece));
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(Piece));

        //Properties
        public Brush Color
        {
            get
            {
                return (Brush) GetValue(ColorProperty);
            }
            set
            {
                SetValue(ColorProperty, value);
            }
        }
        public double X
        {
            get
            {
                return (double)GetValue(XProperty);
            }
            set
            {
                SetValue(XProperty, value);
            }
        }
        public double Y
        {
            get
            {
                return (double)GetValue(YProperty);
            }
            set
            {
                SetValue(YProperty, value);
            }
        }

        private static DropShadowEffect shadow = new DropShadowEffect();
        private Point startingPosition;
        private Canvas parent;

        public Piece()
        {
            InitializeComponent();
            shadow.ShadowDepth = 5;
        }

        private void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            UIElement el = (UIElement)sender;
            el.CaptureMouse();
            
            path.Effect = shadow;
            
            Grid.SetZIndex(this, 1);

            parent = FindAncestor<Canvas>(this);

            startingPosition = el.TranslatePoint(e.GetPosition(this), parent);
            startingPosition.X -= X;
            startingPosition.Y -= Y;
        }

        private void MouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            UIElement el = (UIElement)sender;
            el.ReleaseMouseCapture();

            path.Effect = null;
            Grid.SetZIndex(this, 0);
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (MouseButtonState.Pressed.Equals(e.LeftButton))
            {
                Connect(null);
                UIElement el = (UIElement)sender;
                Point mouse = el.TranslatePoint(e.GetPosition(this), parent);

                X = mouse.X - startingPosition.X;
                Y = mouse.Y - startingPosition.Y;

                if (parent != null)
                {
                    VisualTreeHelper.HitTest(parent,
                                            new HitTestFilterCallback(HitTestFilter),
                                            new HitTestResultCallback(HitTestCallback),
                                            new PointHitTestParameters(mouse));
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("Parent null in filter", "ERROR");
                }

            }
        }

        private HitTestFilterBehavior HitTestFilter(DependencyObject obj)
        {
            if (this == obj)
            {
                return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
            } 
            else if (typeof(Path).Equals(obj.GetType()))
            {
                return HitTestFilterBehavior.Continue;
            }
            else
            {
                return HitTestFilterBehavior.ContinueSkipSelf;
            }
        }

        private HitTestResultBehavior HitTestCallback(HitTestResult result)
        {
            Piece other = FindAncestor<Piece>(result.VisualHit);
            System.Diagnostics.Trace.WriteLine("hit " + other.GetValue(NameProperty), "VERBOSE");
            this.Connect(other);
            return HitTestResultBehavior.Continue;
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = LogicalTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        private void Connect(Piece other)
        {
            if (other == null)
            {
                tab.Center = new Point(20, 20);
                return;
            }

            Point tabLocation = new Point();

            double angle = Math.Atan2(other.Y + other.Height/2 - Y - Height/2, other.X + other.Width/2 - X - Width/2);
            double r1sqrd = Width * Width / (4 * Math.Cos(angle) * Math.Cos(angle));
            double r2sqrd = Height * Height / (4 * Math.Sin(angle) * Math.Sin(angle));

            double dsqrd = Math.Min(r1sqrd, r2sqrd);

            tabLocation.X = Math.Sqrt(dsqrd) * Math.Cos(angle) + Width / 2;
            tabLocation.Y = Math.Sqrt(dsqrd) * Math.Sin(angle) + Height / 2;

            tab.Center = tabLocation;
        }
    }
}
