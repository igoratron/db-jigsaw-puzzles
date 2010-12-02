namespace DavidWynne.TreeMap.Silverlight.Model
{
    /// <summary>
    /// The InputValue for a TreeMap
    /// </summary>
    public class InputValue
    {
        public InputValue(string name, double value, string relationTo)
        {
            this.Name = name;
            this.Value = value;
            this.RelationTo = relationTo;
        }

        public string Name
        {
            get;
            set;
        }

        public double Value
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