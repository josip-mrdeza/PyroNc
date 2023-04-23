using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Pyro.IO.Memory.Gpu;

namespace TriangulationBenchmarks;

[MemoryDiagnoser(true)]
public class GPUInvoker
{
    public static int Count = 10_000;
    public static float[] Array1 = new float[Count];  
    public static float[] Array2 = new float[Count];
    public static float[] Output = new float[Count];
    public static string Code = @"
            __kernel void double_kernel(__global float* input1, __global float* input2, __global float* output)
            {
                int i = get_global_id(0); 
                float f1 = input1[i]-input2[i];
                float f2 = input1[i+1]-input2[i+1];
                float f3 = input1[i+2]-input2[i+2];

                float r1 = pow(f1,2);  
                float r2 = pow(f2,2);         
                float r3 = pow(f3,2);         
                float r = r1+r2+r3;
                output[i] = sqrt(r) + 0.01F; 
            }";

    public Invoker GPUInv = new Invoker("double_kernel", Code);

    private void double_kernel_cpu(float[] input1, float[] input2, float[] output)
    {
        for (int i = 0; i < input1.Length; i++)
        {
            output[i] = (float)Math.Sqrt(Math.Pow(input1[i] - input2[i], 2) + Math.Pow(input1[i] - input2[i], 2) + Math.Pow(input1[i] - input2[i], 2)) + 0.01f;
        }
    }
    
    [Benchmark]
    public void CPU_Invoke()
    {
        double_kernel_cpu(Array1, Array2, Output);
    }

    [Benchmark]
    public void GPU_Invoke()
    {
        GPU_Invoke_Static(GPUInv);
    }

    [Benchmark]
    public void GPU_StackInvoke()
    {
        GPU_Invoke_Stack_Static(GPUInv);
    }

    public static void GPU_Invoke_Static(Invoker invoker)
    {
        invoker.Invoke(new float[][]{Array1, Array2}, new float[][]{Output}, Array1.Length);
    }
    
    public static void GPU_Invoke_Stack_Static(Invoker invoker)
    {
        unsafe
        {
            try
            {
                float* ptr1 = stackalloc float[Array1.Length];
                float* ptr2 = stackalloc float[Array2.Length];
                float* outPtr = stackalloc float[Output.Length];

                float** pp1 = stackalloc float*[2];
                pp1[0] = ptr1;
                pp1[1] = ptr2;
            
                float** pp2 = stackalloc float*[1];
                pp2[0] = outPtr;
                
                invoker.Invoke(pp1, pp2, 2, 1, Output.Length, sizeof(float));
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to allocate stack arrays or invoke on the gpu!");
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                Console.WriteLine(e);
            }
        }
    }
    
    
    // [Benchmark]
    // public void GPU_Invoke_Write()
    // {
    //     GPUInv.Invoke(Array, Output, true);
    // } 
    
    // [Benchmark]
    // public async Task GPU_Invoke_NoWrite_Async()
    // {
    //     await GPUInv.InvokeAsync(Array, Output, false);
    // } 
    //
    // [Benchmark]
    // public async Task GPU_Invoke_Write_Async()
    // {
    //     await GPUInv.InvokeAsync(Array, Output, true);
    // } 
}