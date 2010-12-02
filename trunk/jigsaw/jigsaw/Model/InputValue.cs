namespace DavidWynne.TreeMap.Silverlight.Model
{
    /// <summary>
    /// The InputValue for a TreeMap
    /// </summary>
    public class InputValue
    {
        public InputValue(string name, double value)
        {
            this.Name = name;
            this.Value = value;
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
    }
}