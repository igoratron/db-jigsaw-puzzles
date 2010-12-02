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
        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register("Direction", typeof(String), typeof(Piece), new PropertyMetadata("None"));
        public static readonly DependencyProperty RectProperty = DependencyProperty.Register("Rect", typeof(Rect), typeof(Piece));
        public static readonly DependencyProperty TableNameProperty = DependencyProperty.Register("TableName", typeof(int), typeof(Piece));

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

        public String Direction
        {
            get
            {
                return (String)GetValue(DirectionProperty);
            }
            set
            {
                SetValue(DirectionProperty, value);
            }
        }

        public Rect Rect
        {
            get
            {
                return (Rect)GetValue(RectProperty);
            }
            set
            {
                SetValue(RectProperty, value);
            }
        }

        public int TableName
        {
            get
            {
                return (int)GetValue(TableNameProperty);
            }
            set
            {
                SetValue(TableNameProperty, value);
            }
        }

        private static DropShadowEffect shadow = new DropShadowEffect();
        private TranslateTransform translate;
        private Point startingPosition;

        public Piece()
        {
            InitializeComponent();
            startingPosition = new Point();
            translate = new TranslateTransform();
            shadow.ShadowDepth = 5;
        }

        private void path_MouseDown(object sender, MouseButtonEventArgs e)
        {
            path.Effect = shadow;
            
            Grid.SetZIndex(this, 1);

            startingPosition.X = e.GetPosition(this).X - translate.X;
            startingPosition.Y = e.GetPosition(this).Y - translate.Y;
        }

        private void path_MouseUp(object sender, MouseButtonEventArgs e)
        {
            path.Effect = null;
            Grid.SetZIndex(this, 0);
        }

        private void path_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseButtonState.Pressed.Equals(e.LeftButton))
            {
                translate.X = e.GetPosition(this).X - startingPosition.X;
                translate.Y = e.GetPosition(this).Y - startingPosition.Y;
                path.RenderTransform = translate;
            }
        }
    }
}
