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
        private List<Piece> hitTestResults;

        public Piece()
        {
            InitializeComponent();
            shadow.ShadowDepth = 5;
            hitTestResults = new List<Piece>();
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

                //FIXME: don't create a clone everytime!
                Geometry g = path.RenderedGeometry.CloneCurrentValue();
                g.Transform = new TranslateTransform(X, Y);

                if (parent != null)
                {
                    hitTestResults.Clear();
                    VisualTreeHelper.HitTest(parent,
                                            new HitTestFilterCallback(HitTestFilter),
                                            new HitTestResultCallback(HitTestCallback),
                                            new GeometryHitTestParameters(g));
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("Parent null in filter", "ERROR");
                }

                foreach (Piece p in hitTestResults)
                {
                    this.Snap(p, mouse);
                    this.Connect(p);
                }
            }
        }

        /// <summary>
        /// Filters elements that cannot have a replationship, e.g. discards non-Pieces
        /// </summary>
        /// <param name="obj">An element in the visual tree</param>
        /// <returns></returns>
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

        /// <summary>
        /// Called when a valid element has been found. Creates a tab on the Piece in the direction of the found element
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private HitTestResultBehavior HitTestCallback(HitTestResult result)
        {
            Piece other = FindAncestor<Piece>(result.VisualHit);
            System.Diagnostics.Trace.WriteLine("hit " + other.GetValue(NameProperty), "VERBOSE");
            hitTestResults.Add(other);
            return HitTestResultBehavior.Continue;
        }

        /// <summary>
        /// Find a parent element of a given type and return the first match
        /// </summary>
        /// <typeparam name="T">Type of the parent</typeparam>
        /// <param name="current">The element which parent we want to find</param>
        /// <returns></returns>
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

        /// <summary>
        /// Create a tab in the direction of the other piece
        /// </summary>
        /// <param name="other">The piece we want to lock with</param>
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

        private void Snap(Piece other, Point mouse)
        {
            double width = other.Width + Width;
            double height = other.Height + Height;

            double angle = Math.Atan2((Y + Height / 2) - (other.Y + other.Height / 2), (X + Width / 2) - (other.X + other.Width / 2));
            
            double r1sqrd = width * width / (4 * Math.Cos(angle) * Math.Cos(angle));
            double r2sqrd = height * height / (4 * Math.Sin(angle) * Math.Sin(angle));

            double dsqrd = Math.Min(r1sqrd, r2sqrd);

            X = Math.Sqrt(dsqrd) * Math.Cos(angle) + (other.X - Width) + width / 2;
            Y = Math.Sqrt(dsqrd) * Math.Sin(angle) + (other.Y - Height) + height / 2;

        }
    }
}
