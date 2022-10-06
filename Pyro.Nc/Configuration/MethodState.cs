namespace Pyro.Nc.Configuration
{
    public class MethodState
    {
        public string Id { get; set; }
        public sbyte Index { get; set; }

        public MethodState(string id, sbyte index)
        {
            Id = id;
            Index = index;
        }
    }
}