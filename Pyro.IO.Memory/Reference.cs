namespace Pyro.IO.Memory
{
    public class Reference<T>
    {
        private T val;
        public ref T Value
        {
            get => ref val;
        }

        public Reference(ref T value)
        {
            this.val = value;
        }
    }
}