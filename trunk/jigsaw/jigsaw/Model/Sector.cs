namespace DavidWynne.TreeMap.Silverlight.Model
{
    using System.Windows;

    /// <summary>
    /// A Sector of the output from a TreeMap
    /// </summary>
    public class Sector
    {
        public string Name
        {
            get;
            set;
        }

        public double OriginalValue
        {
            get;
            set;
        }

        public double Area
        {
            get;
            set;
        }

        public double Percentage
        {
            get;
            set;
        }

        public Rect Rect
        {
            get;
            set;
        }

        public string RelationTo
        {
            get;
            set;
        }

    }
}