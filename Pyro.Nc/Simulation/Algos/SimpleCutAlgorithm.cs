using System.Threading.Tasks;
using Pyro.Nc.Simulation.Tools;
using Pyro.Nc.Simulation.Workpiece;
using UnityEngine;

namespace Pyro.Nc.Simulation.Algos;

public class SimpleCutAlgorithm : IMillAlgorithm
{
    public string Name { get; set; } = "Simple Cut";
    public int Id => AlgorithmId;
    public const int AlgorithmId = (int)CutType.Legacy;
    public void Prefix(Vector3[] toolPathPoints)
    {
    }

    public Task PrefixAsync(Vector3[] toolPathPoints)
    {
        return Task.CompletedTask;
    }

    public void Postfix(int index, Transform tr, Vector3 v, Color color)
    {
    }

    public void Mill(ToolBase tool, WorkpieceControl workpiece)
    {
    }

    public Task MillAsync(ToolBase tool, WorkpieceControl workpiece)
    {
        return Task.CompletedTask;
    }
}