using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using jigsaw.Model;
using jigsaw.View.Jigsaw;

namespace jigsaw.Jigsaw.View.Panels
{
    class ForceDirectedPanel : Panel
    {
        private const double REPULSION_CONSTANT = 40000;	// charge constant
        private const double DEFAULT_DAMPING = 0.5;
        private const int DEFAULT_MAX_ITERATIONS = 500;
        private const bool DETERMINISTIC = true;

        private const double RATIO = 300;

        private NodeLayoutInfo[] layoutInfo;

        protected override Size ArrangeOverride(Size finalSize)
        {
            System.Diagnostics.Debug.WriteLine("Final size: " + finalSize);

            foreach (NodeLayoutInfo elementLayout in layoutInfo)
            {
                if (elementLayout != null)
                {
                    // Respect any Left and Top attached properties, 
                    // otherwise the child is placed at (0,0) 
                    //double elementX = JigsawBoard.GetX(child);
                    //double elementY = JigsawBoard.GetY(child);
                    //if (!Double.IsNaN(elementX))
                    //    x = elementX;
                    //if (!Double.IsNaN(elementY)) 
                    //    y = elementY;
                    // Place at the chosen (x,y) location with the child’s DesiredSize 
                    Piece child = JigsawTreemap.getChild(elementLayout.Table);
                    child.Arrange(new Rect(elementLayout.Position, elementLayout.DesiredSize));
                    //JigsawBoard.SetX(child, elementLayout.Position.X);
                    //JigsawBoard.SetY(child, elementLayout.Position.Y);
                }
            }
            // Whatever size you gave me is fine 
            return finalSize;
        }

        /// <summary>
        /// Runs the force-directed layout algorithm on this Diagram, using the specified parameters.
        /// </summary>
        /// <param name="damping">Value between 0 and 1 that slows the motion of the nodes during layout.</param>
        /// <param name="maxIterations">Maximum number of iterations before the algorithm terminates.</param>
        /// <param name="deterministic">Whether to use a random or deterministic layout.</param>
        protected override Size MeasureOverride(Size availableSize)
        {

            NodeLayoutInfo.MinPosition = new Point(Double.MaxValue, Double.MaxValue);
            NodeLayoutInfo.MaxPosition = new Point(Double.MinValue, Double.MinValue);

            // random starting positions can be made deterministic by seeding System.Random with a constant
            Random rnd = DETERMINISTIC ? new Random(0) : new Random();

            layoutInfo = new NodeLayoutInfo[Children.Count];

            for (int i = 0; i < layoutInfo.Length; i++)
            {
                Children[i].Measure(availableSize);
                layoutInfo[i] = new NodeLayoutInfo(Children[i], new Vector(), new Point());
                layoutInfo[i].setX(rnd.Next(-50, 50));
                layoutInfo[i].setY(rnd.Next(-50, 50));
            }

            int stopCount = 0;
            int iterations = 0;

            while (true)
            {
                double totalDisplacement = 0;

                for (int i = 0; i < layoutInfo.Length; i++)
                {
                    NodeLayoutInfo current = layoutInfo[i];

                    // express the node's current position as a vector, relative to the origin
                    Vector vectorPosition = new Vector(CalcDistance(new Point(), current.Position),
                                                        GetBearingAngle(new Point(), current.Position));
                    Vector netForce = new Vector(0, 0);

                    // determine repulsion between nodes
                    foreach (NodeLayoutInfo other in layoutInfo)
                    {
                        if (other != current)
                            netForce += CalcRepulsionForce(current, other);
                    }

                    // determine attraction caused by connections
                    foreach (Table table in current.Table.ForeignKey)
                    {
                        NodeLayoutInfo other = NodeLayoutInfo.get(table);
                        netForce += CalcAttractionForce(current, other);
                    }

                    foreach (NodeLayoutInfo parent in layoutInfo)
                    {
                        if (parent.Table.ForeignKey.Contains(current.Table))
                            netForce += CalcAttractionForce(current, parent);
                    }

                    // apply net force to node velocity
                    current.Velocity = (current.Velocity + netForce) * DEFAULT_DAMPING;

                    // apply velocity to node position
                    current.NextPosition = (vectorPosition + current.Velocity).ToPoint();
                }

                // move nodes to resultant positions (and calculate total displacement)
                for (int i = 0; i < layoutInfo.Length; i++)
                {
                    NodeLayoutInfo current = layoutInfo[i];

                    totalDisplacement += CalcDistance(current.Position, current.NextPosition);
                    current.setX(current.NextPosition.X);
                    current.setY(current.NextPosition.Y);
                }

                iterations++;
                if (totalDisplacement < 10)
                    stopCount++;
                if (stopCount > 15)
                    break;
                if (iterations > DEFAULT_MAX_ITERATIONS)
                    break;
            }

            Size desiredSize = new Size(NodeLayoutInfo.MaxPosition.X - NodeLayoutInfo.MinPosition.X,
                                        NodeLayoutInfo.MaxPosition.Y - NodeLayoutInfo.MinPosition.Y);

            foreach (NodeLayoutInfo element in layoutInfo)
            {
                element.setX(element.Position.X - NodeLayoutInfo.MinPosition.X);
                element.setY(element.Position.Y - NodeLayoutInfo.MinPosition.Y);
            }

            
            System.Diagnostics.Debug.WriteLine("Desired size: " + desiredSize);
            return desiredSize;
        }

        /// <summary>
        /// Calculates the attraction force between two connected nodes
        /// </summary>
        /// <param name="elemA">The node that the force is acting on.</param>
        /// <param name="elemB">The node creating the force.</param>
        /// <returns>A Vector representing the attraction force.</returns>
        private Vector CalcAttractionForce(NodeLayoutInfo elemA, NodeLayoutInfo elemB)
        {
            Point elemALocation = elemA.Position;
            Point elemBLocation = elemB.Position;

            Size elemASize = elemA.DesiredSize;
            Size elemBSize = elemB.DesiredSize;

            double width = elemASize.Width + elemBSize.Width;
            double height = elemASize.Height + elemBSize.Height;

            double angle = Math.Atan2((elemALocation.Y + elemASize.Height / 2) - (elemBLocation.Y + elemBSize.Height / 2), (elemALocation.X + elemASize.Width / 2) - (elemBLocation.X + elemBSize.Width / 2));

            double r1sqrd = width * width / (4 * Math.Cos(angle) * Math.Cos(angle));
            double r2sqrd = height * height / (4 * Math.Sin(angle) * Math.Sin(angle));

            double dsqrd = Math.Min(r1sqrd, r2sqrd);

            Point snappingPoint = new Point();

            snappingPoint.X = Math.Sqrt(dsqrd) * Math.Cos(angle) + (elemBLocation.X - elemASize.Width) + width / 2;
            snappingPoint.Y = Math.Sqrt(dsqrd) * Math.Sin(angle) + (elemBLocation.Y - elemASize.Height) + height / 2;

            int proximity = Math.Max(CalcDistance(elemALocation, elemBLocation), 1);
            int distance = CalcDistance(elemBLocation, snappingPoint);
            //// Hooke's Law: F = -kx
            double force = Math.Max(proximity - distance, 0);
            angle = GetBearingAngle(elemALocation, elemBLocation);

            return new Vector(force, angle);
        }

        /// <summary>
        /// Calculates the distance between two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The pixel distance between the two points.</returns>
        public static int CalcDistance(Point a, Point b)
        {
            double xDist = (a.X - b.X);
            double yDist = (a.Y - b.Y);
            return (int)Math.Sqrt(Math.Pow(xDist, 2) + Math.Pow(yDist, 2));
        }

        /// <summary>
        /// Calculates the repulsion force between any two nodes in the diagram space.
        /// </summary>
        /// <param name="elemA">The node that the force is acting on.</param>
        /// <param name="elemB">The node creating the force.</param>
        /// <returns>A Vector representing the repulsion force.</returns>
        private Vector CalcRepulsionForce(NodeLayoutInfo elemA, NodeLayoutInfo elemB)
        {
            Point elemALocation = elemA.Position;
            Point elemBLocation = elemB.Position;

            int proximity = Math.Max(CalcDistance(elemALocation, elemBLocation), 1);

            // Coulomb's Law: F = k(Qq/r^2)
            double force = -(REPULSION_CONSTANT / Math.Pow(proximity, 2));
            double angle = GetBearingAngle(elemALocation, elemBLocation);

            return new Vector(force, angle);
        }

        /// <summary>
        /// Calculates the bearing angle from one point to another.
        /// </summary>
        /// <param name="start">The node that the angle is measured from.</param>
        /// <param name="end">The node that creates the angle.</param>
        /// <returns>The bearing angle, in degrees.</returns>
        private double GetBearingAngle(Point start, Point end)
        {
            Point half = new Point(start.X + ((end.X - start.X) / 2), start.Y + ((end.Y - start.Y) / 2));

            double diffX = (double)(half.X - start.X);
            double diffY = (double)(half.Y - start.Y);

            if (diffX == 0) diffX = 0.001;
            if (diffY == 0) diffY = 0.001;

            double angle;
            if (Math.Abs(diffX) > Math.Abs(diffY))
            {
                angle = Math.Tanh(diffY / diffX) * (180.0 / Math.PI);
                if (((diffX < 0) && (diffY > 0)) || ((diffX < 0) && (diffY < 0))) angle += 180;
            }
            else
            {
                angle = Math.Tanh(diffX / diffY) * (180.0 / Math.PI);
                if (((diffY < 0) && (diffX > 0)) || ((diffY < 0) && (diffX < 0))) angle += 180;
                angle = (180 - (angle + 90));
            }

            return angle;
        }

        /// <summary>
        /// Private inner class used to track the node's position and velocity during simulation.
        /// </summary>
        private class NodeLayoutInfo
        {
            public static Point MinPosition;
            public static Point MaxPosition;
            public static Dictionary<Table, NodeLayoutInfo> resolver = new Dictionary<Table, NodeLayoutInfo>();
            private Point position;

            public Size DesiredSize;
            public Table Table;
            public Vector Velocity;		// the node's current velocity, expressed in vector form
            public Point NextPosition;
            public Point Position 
            {
                get
                {
                    return position;
                }
            }

            /// <summary>
            /// Initialises a new instance of the Diagram.NodeLayoutInfo class, using the specified parameters.
            /// </summary>
            /// <param name="node"></param>
            /// <param name="velocity"></param>
            /// <param name="nextPosition"></param>
            public NodeLayoutInfo(UIElement node, Vector velocity, Point nextPosition)
            {
                Table = (Table)((Piece)node).DataContext;
                Velocity = velocity;
                NextPosition = nextPosition;
                position = new Point();
                DesiredSize = node.DesiredSize;
                resolver.Add(Table, this);
            }

            public void setX(double x) 
            {
                position.X = x;

                if (x < MinPosition.X)
                {
                    MinPosition.X = x;
                }

                if (x + DesiredSize.Width > MaxPosition.X)
                {
                    MaxPosition.X = x + DesiredSize.Width;
                }
            }

            public void setY(double y)
            {
                position.Y = y;
                if (y < MinPosition.Y)
                {
                    MinPosition.Y = y;
                }

                if (y + DesiredSize.Height > MaxPosition.Y)
                {
                    MaxPosition.Y = y + DesiredSize.Height;
                }
            }

            public static NodeLayoutInfo get(Table t)
            {
                return resolver[t];
            }
        }
    }
}
