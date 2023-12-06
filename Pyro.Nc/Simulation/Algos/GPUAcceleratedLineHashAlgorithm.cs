 using System;
 using System.Collections.Generic;
 using System.Diagnostics;
 using System.Linq;
 using System.Text;
 using System.Threading.Tasks;
 using Pyro.IO;using Pyro.IO.Memory;
 using Pyro.IO.Memory.Gpu;
 using Pyro.Nc.Simulation.Tools;
 using UnityEngine;using Debug = UnityEngine.Debug;

 namespace Pyro.Nc.Simulation.Algos;

 [Obsolete("Not done.")]
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
         var fileName = $"{ClKernelName}.cl.cpp";
         var lr = LocalRoaming.OpenOrCreate($"PyroNc\\GPU-Source\\{dirId}");
         var lines = lr.ReadFileAsText(fileName).Split('\n');
         var kernelFunctionName = $"void {ClKernelName}";
         for (int i = 0; i < lines.Length; i++)
         {
             if (lines[i].StartsWith(kernelFunctionName))
             {
                 var str = "__kernel " + lines[i];
                 var indexOfP = str.IndexOf('(');
                 StringBuilder builder = new StringBuilder(str);
                 var s = "__global ";
                 for (int j = indexOfP; j < builder.Length; j++)
                 {
                     char c = builder[j];
                     if (c == ',')
                     {
                         if (builder.HasCharacterUntilNext(j + 1, '*', ','))
                         {
                             builder.Insert(j + 2, s);
                             j += s.Length;
                         }
                     }
                     else if (c == '(' && builder[j + 1] != '_')
                     {
                         if (builder.HasCharacterUntilNext(j, '*', ','))
                         {
                             builder.Insert(j + 1, s);
                             j += s.Length;
                         }
                     }
                     else if (c == ')')
                     {
                         break;
                     }
                 }

                 lines[i] = builder.ToString();
                 builder.Clear();
             }
         }

         ClCode = string.Join("\n", lines);
         Debug.Log(ClCode);
         Globals.Console.Push($"[GPU] - Creating invoker...");
         Invoker.Notification += (info, data, cb, userData) =>
         {
             Globals.Console.Push($"[GPU] - {info}!");
         };
         LocalRoaming lr2 = LocalRoaming.OpenOrCreate($"PyroNc\\GPU-Source\\{dirId}\\PreCompiled");
         lr2.Delete(fileName);
         lr2.AddFile(fileName, ClCode);
         Gpu = new Invoker(ClKernelName, lr2.Site, null, true);
         Application.quitting += Dispose;
     }

     public override async Task<Dictionary<Vector3, List<int>>> CompileLine(Vector3[] toolPathPoints)
     {
         if (Gpu == null)
         {
             var dirId = "ACLHA";
             var lr = LocalRoaming.OpenOrCreate($"PyroNc\\GPU-Source\\{dirId}");
             Gpu = new Invoker(ClKernelName, lr.Site);
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
         Dispose(); //TODO

         return vecToListHash;
     }

     public void Dispose()
     {
         Gpu?.Dispose();
     }
 }