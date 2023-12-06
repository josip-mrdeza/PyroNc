namespace PyroNetServer.Models;

public class Integer
{
    public int Value { get; set; }

    public Integer(int value)
    {
        Value = value;
    }

    public static implicit operator int(Integer i)
    {
        return i.Value;
    }
}