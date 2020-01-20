#if !BESTHTTP_DISABLE_SIGNALR_CORE

namespace BestHTTP.Examples
{
    /// <summary>
    /// Helper class to demonstrate strongly typed callbacks
    /// </summary>
    internal sealed class Person
    {
        public string Name { get; set; }
        public long Age { get; set; }

        public override string ToString()
        {
            return string.Format("[Person Name: '{0}', Age: '<color=yellow>{1}</color>']", this.Name, this.Age);
        }
    }
}

#endif
