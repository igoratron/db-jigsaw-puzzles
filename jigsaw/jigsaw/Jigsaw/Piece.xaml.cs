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
using jigsaw.Jigsaw;


namespace jigsaw
{
    /// <summary>
    /// Interaction logic for Piece.xaml
    /// </summary>
    public partial class Piece : UserControl
    {
        //Dependency properties
        public static readonly DependencyProperty TableNameProperty = DependencyProperty.Register("TableName", typeof(String), typeof(Piece));
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Brush), typeof(Piece));
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(Piece));
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(Piece));
        public static readonly DependencyProperty DeltaXProperty = DependencyProperty.Register("DeltaX", typeof(double), typeof(Piece));
        public static readonly DependencyProperty DeltaYProperty = DependencyProperty.Register("DeltaY", typeof(double), typeof(Piece));

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
        public double DeltaX
        {
            get
            {
                return (double)GetValue(DeltaXProperty);
            }
            set
            {
                SetValue(DeltaXProperty, value);
            }
        }
        public double DeltaY
        {
            get
            {
                return (double)GetValue(DeltaYProperty);
            }
            set
            {
                SetValue(DeltaYProperty, value);
            }
        }
        public String TableName
        {
            get
            {
                return (String)GetValue(TableNameProperty);
            }
            set
            {
                SetValue(TableNameProperty, value);
            }
        }

        //Constants
        private const double TAB_RADIUS = 20;

        private static DropShadowEffect shadow = new DropShadowEffect();
        private Point startingPosition;
        private Panel parent;
        private List<Piece> hitTestResults;

        private static Random random = new Random();

        public Piece()
        {
            InitializeComponent();
            shadow.ShadowDepth = 5;
            hitTestResults = new List<Piece>();

            System.Windows.Media.Color start;
            System.Windows.Media.Color end;
            
            double r, g, b;
            double lightness = random.NextDouble() * 0.5 + 0.4; // not too dark nor too light
            double hue = random.NextDouble() * 360.0; // full hue spectrum
            double saturation = random.NextDouble() * 0.8 + 0.2; // not too grayish
            
            HSLtoRGB(hue, saturation, lightness, out r, out g, out b);
            start = System.Windows.Media.Color.FromRgb((byte)(r * 255.0), (byte)(g * 255.0), (byte)(b * 255.0));
            HSLtoRGB(hue, saturation, lightness - 0.2, out r, out g, out b);
            end = System.Windows.Media.Color.FromRgb((byte)(r * 255.0), (byte)(g * 255.0), (byte)(b * 255.0));

            LinearGradientBrush gradient = new LinearGradientBrush(start, end, 90);

            Color = gradient;
        }

        private void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            UIElement el = (UIElement)sender;
            el.CaptureMouse();
            
            parent = FindAncestor<JigsawBoard>(this);

            startingPosition = el.TranslatePoint(e.GetPosition(this), parent);
            startingPosition.X -= DeltaX;
            startingPosition.Y -= DeltaY;
        }

        private void MouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            UIElement el = (UIElement)sender;
            el.ReleaseMouseCapture();
            Grid.SetZIndex(this, 0);
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (MouseButtonState.Pressed.Equals(e.LeftButton))
            {
                Connect(null);
                Point relativeToParent = e.GetPosition(parent);
                Point relativeToThis = e.GetPosition(this);

                X = relativeToParent.X - relativeToThis.X;
                Y = relativeToParent.Y - relativeToThis.Y;

                DeltaX = relativeToParent.X - startingPosition.X;
                DeltaY = relativeToParent.Y - startingPosition.Y;

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
                    //this.Snap(p, mouse);
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
            System.Diagnostics.Trace.WriteLine("hit " + other.GetValue(TableNameProperty), "VERBOSE");
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
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        /// <summary>
        /// Create a tab in the direction of the other piece
        /// </summary>
        /// <param name="other">The piece we want to lock with</param>
        public void Connect(Piece other)
        {
            if (other == null)
            {
                path.Data = (RectangleGeometry)this.Resources["MainRectangle"];
            }
            else
            {
                Geometry currentGeomtery = path.Data;
                path.Data = CombinedGeometry.Combine(CreateTab(other), currentGeomtery, GeometryCombineMode.Union, null);
            }
        }

        private EllipseGeometry CreateTab(Piece other)
        {
            Point centre = new Point();

            double angle = Math.Atan2(other.Y + other.Height / 2 - Y - Height / 2, other.X + other.Width / 2 - X - Width / 2);
            double r1sqrd = Width * Width / (4 * Math.Cos(angle) * Math.Cos(angle));
            double r2sqrd = Height * Height / (4 * Math.Sin(angle) * Math.Sin(angle));

            double dsqrd = Math.Min(r1sqrd, r2sqrd);

            centre.X = Math.Sqrt(dsqrd) * Math.Cos(angle) + Width / 2;
            centre.Y = Math.Sqrt(dsqrd) * Math.Sin(angle) + Height / 2;

            EllipseGeometry tab = new EllipseGeometry(centre, TAB_RADIUS, TAB_RADIUS);

            return tab;
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

        private static void HSLtoRGB(double hue, double saturation, double luminance, out double red, out double green, out double blue)
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

        protected override Size MeasureOverride(Size constraint)
        {
            return constraint;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Width = arrangeBounds.Width;
            Height = arrangeBounds.Height;
            return base.ArrangeOverride(arrangeBounds);
        }
    }
}
