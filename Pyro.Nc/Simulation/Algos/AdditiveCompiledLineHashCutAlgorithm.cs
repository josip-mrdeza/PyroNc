using System;
using System.Threading.Tasks;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Tools;
using Pyro.Nc.Simulation.Workpiece;
using UnityEngine;

namespace Pyro.Nc.Simulation.Algos;

public class AdditiveCompiledLineHashCutAlgorithm : CompiledLineHashAlgorithm
{
    public override string Name { get; set; } = "Additive Compiled Line Hash";
    public override int Id => AlgorithmId;
    public new const int AlgorithmId = (int)CutType.LineHashAdditive;

    public override void Postfix(int index, Transform tr, Vector3 v, Color color)
    {
        base.Postfix(index, tr, v, color);

        throw new NotImplementedException();
    }
}