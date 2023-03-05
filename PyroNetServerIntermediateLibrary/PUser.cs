namespace PyroNetServerIntermediateLibrary
{
    public class PUser
    {
        public string Name { get; set; }
        public string Password { get; set; }

        public PUser(string name, string password)
        {
            Name = name;
            Password = password;
        }
    }
}