using System.Threading.Tasks;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Tools;
using Pyro.Nc.Simulation.Workpiece;
using UnityEngine;

namespace Pyro.Nc.Simulation.Algos;

public class SimpleCutAlgorithm : MachineComponent, IMillAlgorithm
{
    public string Name { get; set; } = "Simple Cut";
    public int Id => AlgorithmId;
    public const int AlgorithmId = (int)CutType.Legacy;
    public Vector3[] ToolPathPoints { get; private set; }
    public void Prefix(Vector3[] toolPathPoints)
    {
        ToolPathPoints = toolPathPoints;
    }

    public Task PrefixAsync(Vector3[] toolPathPoints)
    {
        Prefix(toolPathPoints);
        return Task.CompletedTask;
    }

    public void Postfix(int index, Transform tr, Vector3 v, Color color)
    {
    }

    public void Mill(ToolBase tool, WorkpieceControl workpiece)
    {
        var color = tool.ToolConfig.GetColor();
        var machine = Machine;
        var control = machine.Workpiece;
        var vertices = control.Vertices;
        var pos = tool.Position;
        var tr = control.transform;
        
        for (int i = 0; i < workpiece.Vertices.Count; i++)
        {
            var transformedVertex = tr.TransformPoint(workpiece.Vertices[i]);
            for (int j = 0; j < ToolPathPoints.Length; j++)
            {
                
            }
        }
    }

    public Task MillAsync(ToolBase tool, WorkpieceControl workpiece)
    {
        Mill(tool, workpiece);
        return Task.CompletedTask;
    }
}