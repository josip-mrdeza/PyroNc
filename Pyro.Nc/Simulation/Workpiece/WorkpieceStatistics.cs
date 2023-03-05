using System.Collections.Generic;

namespace Pyro.Nc.Simulation.Workpiece;

public class WorkpieceStatistics
{
    public Dictionary<string, int> MethodCalls = new Dictionary<string, int>();
    public static WorkpieceStatistics Statistics = new WorkpieceStatistics();
    public WorkpieceStatistics()
    {
        var methods = typeof(WorkpieceControl).GetMethods();
        foreach (var method in methods)
        {
            if (MethodCalls.ContainsKey(method.Name))
            {
                continue;
            }
            MethodCalls.Add(method.Name, 0);
        }
    }

    public void Increment(string method)
    {
        MethodCalls[method]++;
    }

    public void PrintCalls()
    {
        foreach (var methodCall in MethodCalls)
        {
            Globals.Console.Push($"Method '{methodCall.Key}' was called {methodCall.Value} times.");
            MethodCalls[methodCall.Key] = 0;
        }
    }
}