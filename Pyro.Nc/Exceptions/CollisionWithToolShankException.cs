using Pyro.Nc.Configuration;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;

namespace Pyro.Nc.Exceptions;

public class CollisionWithToolShankException : NotifyException
{
    public CollisionWithToolShankException() : base($"~[{CurrentContext.GetType().Name}]: "+
                                                    $"Tool shank crashed into workpiece at {Globals.Tool.Position.SwitchYZ()},\n" +
                                                    $"At line {Globals.GCodeInputHandler.Line}~")
    {                                                      
    }
}