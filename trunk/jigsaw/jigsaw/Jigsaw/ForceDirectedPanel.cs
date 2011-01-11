using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using jigsaw.Model;

namespace jigsaw.Jigsaw
{
    class ForceDirectedPanel : Panel
    {
        private const double ATTRACTION_CONSTANT = 1;		// spring constant
        private const double REPULSION_CONSTANT = 40000;	// charge constant

        private const double DEFAULT_DAMPING = 0.5;
        //private const int DEFAULT_SPRING_LENGTH = 100;
        private const int DEFAULT_MAX_ITERATIONS = 500;

        private const double RATIO = 300;

        private List<Piece> mNodes;

        protected override Size ArrangeOverride(Size finalSize)
        {
            Arrange(DEFAULT_DAMPING, /*DEFAULT_SPRING_LENGTH, */DEFAULT_MAX_ITERATIONS, true, finalSize);

            foreach (UIElement child in mNodes)
            {
                if (child != null)
                {
                    double x = 0;
                    double y = 0;
                    // Respect any Left and Top attached properties, 
                    // otherwise the child is placed at (0,0) 
                    double elementX = JigsawBoard.GetX(child);
                    double elementY = JigsawBoard.GetY(child);
                    if (!Double.IsNaN(elementX))
                        x = elementX;
                    if (!Double.IsNaN(elementY)) 
                        y = elementY;
                    // Place at the chosen (x,y) location with the child’s DesiredSize 
                    child.Arrange(new Rect(new Point(x, y), child.DesiredSize));
                }
            }
            // Whatever size you gave me is fine 
            return finalSize;
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

        /// <summary>
        /// Runs the force-directed layout algorithm on this Diagram, using the specified parameters.
        /// </summary>
        /// <param name="damping">Value between 0 and 1 that slows the motion of the nodes during layout.</param>
        /// <param name="springLength">Value in pixels representing the length of the imaginary springs that run along the connectors.</param>
        /// <param name="maxIterations">Maximum number of iterations before the algorithm terminates.</param>
        /// <param name="deterministic">Whether to use a random or deterministic layout.</param>
        public void Arrange(double damping, /*int springLength, */int maxIterations, bool deterministic, Size availableSize)
        {
            // random starting positions can be made deterministic by seeding System.Random with a constant
            Random rnd = deterministic ? new Random(0) : new Random();

            // copy nodes into an array of metadata and randomise initial coordinates for each node
            mNodes = new List<Piece>();

            foreach (UIElement element in Children)
            {
                TreeViewItem current = (TreeViewItem)element;
                Piece p = (Piece)current.Template.FindName("piece", current);
                mNodes.Add(p);
            }

            NodeLayoutInfo[] layout = new NodeLayoutInfo[mNodes.Count];

            for (int i = 0; i < mNodes.Count; i++)
            {
                layout[i] = new NodeLayoutInfo(mNodes[i], new Vector(), new Point());
                JigsawBoard.SetX(layout[i].Element, rnd.Next(-50, 50));
                JigsawBoard.SetY(layout[i].Element, rnd.Next(-50, 50));
            }

            int stopCount = 0;
            int iterations = 0;

            while (true)
            {
                double totalDisplacement = 0;

                for (int i = 0; i < layout.Length; i++)
                {
                    NodeLayoutInfo current = layout[i];

                    // express the node's current position as a vector, relative to the origin
                    Point currentElemLocation = new Point(JigsawBoard.GetX(current.Element), JigsawBoard.GetY(current.Element));
                    Vector currentPosition = new Vector(CalcDistance(new Point(), currentElemLocation),
                                                        GetBearingAngle(new Point(), currentElemLocation));
                    Vector netForce = new Vector(0, 0);

                    // determine repulsion between nodes
                    foreach (Piece other in mNodes)
                    {
                        if (other != current.Element)
                            netForce += CalcRepulsionForce(current.Element, other);
                    }

                    // determine attraction caused by connections
                    foreach (Piece child in current.Element.ForeignKeyPieces)
                    {
                        netForce += CalcAttractionForce(current.Element, child/*, springLength*/);
                    }
                    foreach (Piece parent in mNodes)
                    {
                        if (parent.ForeignKeyPieces.Contains(current.Element))
                            netForce += CalcAttractionForce(current.Element, parent/*, springLength*/);
                    }

                    // apply net force to node velocity
                    current.Velocity = (current.Velocity + netForce) * damping;

                    // apply velocity to node position
                    current.NextPosition = (currentPosition + current.Velocity).ToPoint();
                }

                // move nodes to resultant positions (and calculate total displacement)
                for (int i = 0; i < layout.Length; i++)
                {
                    NodeLayoutInfo current = layout[i];

                    Point currentElemLocation = new Point(JigsawBoard.GetX(current.Element), JigsawBoard.GetY(current.Element));

                    totalDisplacement += CalcDistance(currentElemLocation, current.NextPosition);
                    JigsawBoard.SetX(current.Element, current.NextPosition.X);
                    JigsawBoard.SetY(current.Element, current.NextPosition.Y);
                }

                iterations++;
                if (totalDisplacement < 10)
                    stopCount++;
                if (stopCount > 15)
                    break;
                if (iterations > maxIterations)
                    break;
            }

            // center the diagram around the origin
            Rect logicalBounds = new Rect(availableSize);
            Point midPoint = new Point(logicalBounds.X + (logicalBounds.Width / 2), logicalBounds.Y + (logicalBounds.Height / 2));

            foreach (Piece element in mNodes)
            {
                Point currentElemLocation = new Point(JigsawBoard.GetX(element), JigsawBoard.GetY(element));

                JigsawBoard.SetX(element, currentElemLocation.X + midPoint.X);
                JigsawBoard.SetY(element, currentElemLocation.Y + midPoint.Y);
            }
        }

        /// <summary>
        /// Calculates the attraction force between two connected nodes, using the specified spring length.
        /// </summary>
        /// <param name="elemA">The node that the force is acting on.</param>
        /// <param name="elemB">The node creating the force.</param>
        /// <param name="springLength">The length of the spring, in pixels.</param>
        /// <returns>A Vector representing the attraction force.</returns>
        private Vector CalcAttractionForce(UIElement elemA, UIElement elemB/*, double springLength*/)
        {
            Point elemALocation = new Point(JigsawBoard.GetX(elemA), JigsawBoard.GetY(elemA));
            Point elemBLocation = new Point(JigsawBoard.GetX(elemB), JigsawBoard.GetY(elemB));

            Size elemASize = elemA.DesiredSize;
            Size elemBSize = elemB.DesiredSize;

            double width = elemASize.Width + elemBSize.Width;
            double height = elemASize.Height + elemBSize.Height;

            double angle = Math.Atan2((elemALocation.Y + elemASize.Height / 2) - (elemBLocation.Y + elemBSize.Height / 2), (elemALocation.X + elemASize.Width / 2) - (elemBLocation.X + elemBSize.Width / 2));

            double r1sqrd = width * width / (4 * Math.Cos(angle) * Math.Cos(angle));
            double r2sqrd = height * height / (4 * Math.Sin(angle) * Math.Sin(angle));

            double dsqrd = Math.Min(r1sqrd, r2sqrd);

            Point point = new Point();

            point.X = Math.Sqrt(dsqrd) * Math.Cos(angle) + (elemBLocation.X - elemASize.Width) + width / 2;
            point.Y = Math.Sqrt(dsqrd) * Math.Sin(angle) + (elemBLocation.Y - elemASize.Height) + height / 2;

            int proximity = Math.Max(CalcDistance(elemALocation, elemBLocation), 1);
            int springLength = CalcDistance(elemBLocation, point);
            //// Hooke's Law: F = -kx
            double force = ATTRACTION_CONSTANT * Math.Max(proximity - springLength, 0);
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
        private Vector CalcRepulsionForce(UIElement elemA, UIElement elemB)
        {
            Point elemALocation = new Point(JigsawBoard.GetX(elemA), JigsawBoard.GetY(elemA));
            Point elemBLocation = new Point(JigsawBoard.GetX(elemB), JigsawBoard.GetY(elemB));

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

            public Piece Element;			// reference to the node in the simulation
            public Vector Velocity;		// the node's current velocity, expressed in vector form
            public Point NextPosition;	// the node's position after the next iteration

            /// <summary>
            /// Initialises a new instance of the Diagram.NodeLayoutInfo class, using the specified parameters.
            /// </summary>
            /// <param name="node"></param>
            /// <param name="velocity"></param>
            /// <param name="nextPosition"></param>
            public NodeLayoutInfo(Piece node, Vector velocity, Point nextPosition)
            {
                Element = node;
                Velocity = velocity;
                NextPosition = nextPosition;
            }
        }
    }
}
