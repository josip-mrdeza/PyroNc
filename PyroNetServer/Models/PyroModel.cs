namespace PyroNetServer.Models;

public class PyroModel
{
    public string Id { get; set; }
    public int Version { get; set; }

    public PyroModel(int version)
    {
        Id = $"update-V{version}";
        Version = version;
    }
}