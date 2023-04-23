using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.IO.Memory.Gpu;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Simulation.Algos;

public class GPUAcceleratedLineHashAlgorithm : CompiledLineHashAlgorithm, IDisposable
{
    public override string Name => $"{base.Name} (GPU)";
    public override int Id => AlgorithmId;
    public new const int AlgorithmId = (int)CutType.GpuLineHash;
    public const string ClKernelName = "gpu_acc_lh_a";
    public string ClCode { get; }
    public string ErrorLog { get; private set; }
    public Invoker Gpu { get; private set; } 

    public GPUAcceleratedLineHashAlgorithm()
    {
        var dirId = "ACLHA";
        var lr = LocalRoaming.OpenOrCreate($"PyroNc\\GPU-Source\\{dirId}");
        ClCode = lr.ReadFileAsText($"{ClKernelName}.cpp");
        var site = lr.Site;
        Globals.Console.Push($"[GPU] - Creating invoker...");
        Invoker.Notification += (info, data, cb, userData) =>
        {
            Globals.Console.Push($"[GPU] - {info}!");
        };
        Gpu = new Invoker(ClKernelName, site);
        Application.quitting += Dispose;
    }

    public override async Task<Dictionary<Vector3, List<int>>> CompileLine(Vector3[] toolPathPoints)
    {
        if (Gpu == null)
        {
            Gpu = new Invoker(ClKernelName, ClCode);
        }
        else if (!Gpu.HasInitialized)
        {
            Gpu.Init();
        }
        Stopwatch s = Stopwatch.StartNew();
        Dictionary<Vector3, List<int>> vecToListHash = new Dictionary<Vector3, List<int>>();
        var machine = Machine;
        var control = machine.Workpiece;
        var vertices = control.Vertices;
        var toolConfig = Globals.Tool.ToolConfig;
        var radius = toolConfig.Radius;
        var transform = await machine.Queue.Run(GetTransformMainThread, control);
        var transformedVertices = await machine.Queue.Run(GetTransformedVerticesMainThread,
                                                          vertices, transform);
        var ta = new float[transformedVertices.Count];
        await Gpu.InvokeAsync(null, ta, transformedVertices.Count, false);
        Dispose();//TODO
        return vecToListHash;
    }

    public void Dispose()
    {
        Gpu?.Dispose();
    }
}