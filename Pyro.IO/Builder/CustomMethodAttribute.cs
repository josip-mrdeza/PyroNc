using System;

namespace Pyro.IO;

[AttributeUsage(AttributeTargets.Method)]
public class CustomMethodAttribute : Attribute
{
    public string Name { get; set; }
    
    public CustomMethodAttribute(string name)
    {
        Name = name;
    }
}