﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using jigsaw.Jigsaw;
using jigsaw.Model;
using jigsaw.TypeConverters;
using jigsaw.Jigsaw.View.Panels;

namespace jigsaw.View.Jigsaw
{
    /// <summary>
    /// Interaction logic for Piece.xaml
    /// </summary>
    public partial class Piece : TreeViewItem
    {
        private const double RATIO = 300;

        #region Dependency properties
        public static readonly DependencyProperty TableNameProperty = DependencyProperty.Register("TableName", typeof(String), typeof(Piece));
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Brush), typeof(Piece));
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(Piece));
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(Piece));
        public static readonly DependencyProperty DeltaXProperty = DependencyProperty.Register("DeltaX", typeof(double), typeof(Piece));
        public static readonly DependencyProperty DeltaYProperty = DependencyProperty.Register("DeltaY", typeof(double), typeof(Piece));
        public static readonly DependencyProperty ForeignKeyPiecesProperty = DependencyProperty.Register("ForeignKeyPieces", typeof(List<Piece>), typeof(Piece));
        public static readonly DependencyProperty IsLeftPressedProperty = DependencyProperty.Register("IsLeftPressed", typeof(Boolean), typeof(Piece));
        public static readonly DependencyProperty IsRightPressedProperty = DependencyProperty.Register("IsRightPressed", typeof(Boolean), typeof(Piece));
        #endregion

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
        public List<Piece> ForeignKeyPieces {
            get
            {
                return (List<Piece>)GetValue(ForeignKeyPiecesProperty);
            }
            set
            {
                SetValue(ForeignKeyPiecesProperty, value);
            }
        }
        public Boolean IsLeftPressed
        {
            get
            {
                return (Boolean)GetValue(IsLeftPressedProperty);
            }
            set
            {
                SetValue(IsLeftPressedProperty, value);
            }
        }
        public Boolean IsRightPressed
        {
            get
            {
                return (Boolean)GetValue(IsRightPressedProperty);
            }
            set
            {
                SetValue(IsRightPressedProperty, value);
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
            
            Utils.HSLtoRGB(hue, saturation, lightness, out r, out g, out b);
            start = System.Windows.Media.Color.FromRgb((byte)(r * 255.0), (byte)(g * 255.0), (byte)(b * 255.0));
            Utils.HSLtoRGB(hue, saturation, lightness - 0.15, out r, out g, out b);
            end = System.Windows.Media.Color.FromRgb((byte)(r * 255.0), (byte)(g * 255.0), (byte)(b * 255.0));

            LinearGradientBrush gradient = new LinearGradientBrush(start, end, 90);

            Color = gradient;
        }

        private void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            UIElement el = (UIElement)sender;
            el.CaptureMouse();

            if (MouseButtonState.Pressed.Equals(e.RightButton))
            {
                IsRightPressed = true;
            }
            if (MouseButtonState.Pressed.Equals(e.LeftButton))
            {
                IsLeftPressed = true;
            }
            
            parent = Utils.FindAncestor<ForceDirectedPanel>(this);

            startingPosition = el.TranslatePoint(e.GetPosition(this), parent);
            startingPosition.X -= DeltaX;
            startingPosition.Y -= DeltaY;
        }

        private void MouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (MouseButtonState.Released.Equals(e.RightButton))
            {
                IsRightPressed = false;
            }
            if (MouseButtonState.Released.Equals(e.LeftButton))
            {
                IsLeftPressed = false;
            }

            UIElement el = (UIElement)sender;
            el.ReleaseMouseCapture();
            Panel.SetZIndex(this, 0);
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
                Path path = (Path)this.Template.FindName("path", this);
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
                    this.Snap(p);
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
            Piece other = Utils.FindAncestor<Piece>(result.VisualHit);
            System.Diagnostics.Trace.WriteLine("hit " + other.GetValue(TableNameProperty), "VERBOSE");
            hitTestResults.Add(other);
            return HitTestResultBehavior.Continue;
        }

        /// <summary>
        /// Create a tab in the direction of the other piece
        /// </summary>
        /// <param name="other">The piece we want to lock with</param>
        private void Connect(Piece other)
        {
            Path path = (Path) this.Template.FindName("path", this);
            CombinedGeometry shape = (CombinedGeometry)this.Template.FindName("shape", this);

            if (other == null)
            {
                path.Data = (RectangleGeometry)this.Resources["MainRectangle"];
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Connecting {0} with {1}", this.TableName, other.TableName);
                Geometry currentGeomtery = path.Data;
                
                //FIXME: get rid of this hack! It appears that at this stage rectangle is not yet bound to Width and Height thus
                //thus the rectangle does not have any area -> combinegeometry discards it!
                RectangleGeometry rectangle = (RectangleGeometry)this.Resources["MainRectangle"];
                rectangle.Rect = new Rect(new Size(Width, Height));
                shape.Geometry1 = rectangle;

                path.Data = CombinedGeometry.Combine(path.Data, CreateTab(other), GeometryCombineMode.Union, null);
            }
        }

        private void Reconnect()
        {
            Connect(null);
            if (ForeignKeyPieces != null)
            {
                foreach (Piece p in ForeignKeyPieces)
                {
                    Connect(p);
                }
            }
        }

        private EllipseGeometry CreateTab(Piece other)
        {
            EllipseGeometry tab = (EllipseGeometry)this.Resources["Tab"];

            Binding thisPiece = new Binding();
            thisPiece.Source = this;

            Binding otherPiece = new Binding();
            otherPiece.Source = other;

            MultiBinding binding = new MultiBinding();
            binding.Bindings.Add(thisPiece);
            binding.Bindings.Add(otherPiece);
            binding.Converter = new TabCenterConverter();

            BindingOperations.SetBinding(tab, EllipseGeometry.CenterProperty, binding);

            return tab;
        }

        private void Snap(Piece other)
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

        protected override Size MeasureOverride(Size constraint)
        {
            Table t = DataContext as Table;
            double width;

            if (t != null)
            {
                 width = Math.Sqrt(t.Size * RATIO);
            }
            else
            {
                width = RATIO;
            }
            

            return new Size(width, width);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Width = arrangeBounds.Width;
            Height = arrangeBounds.Height;
            //Reconnect();
            return base.ArrangeOverride(arrangeBounds);
        }
    }
}
//