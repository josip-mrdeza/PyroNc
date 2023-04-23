
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using OpenCL.Net;
using OpenCL.Net.Extensions;

namespace Pyro.IO.Memory.Gpu;

public class Invoker : IDisposable
{
    public Context CurrentContext { get; private set; }
    public Kernel CurrentKernel { get; private set; }
    public Program CurrentProgram { get; private set; }
    public CommandQueue CurrentCommandQueue { get; private set; }
    public IMem[] Buffers { get; private set; }
    public List<Device> Devices { get; private set; }
    public Device FastestDevice { get; private set; }
    public Device DeviceWithMostMemory { get; private set; }
    public Platform CurrentPlatform { get; private set; }
    public bool HasInitialized { get; internal set; }
    public string Arguments { get; set; }
    public string[] Includes { get; set; }
    public static event Cl.ContextNotify Notification;
    internal static Invoker Instance;
    public static void InvokeNotification(ErrorCode code, string desc)
    {
        Notification?.Invoke($"({desc}) " + code, null, IntPtr.Zero, IntPtr.Zero);
    }
    public Invoker(string kernelName, string pathToIncludes, string arguments = null)
    {
        Instance = this;
        Arguments = arguments;
        if (pathToIncludes != null)
        {
            Includes = Directory.GetFiles(pathToIncludes).Select(File.ReadAllText).ToArray();
        }
        if (kernelName != null)
        {
            Init(kernelName);
        }
        Console.WriteLine("Created new instance!");
    }

    public void Init(string kernelName = default)
    {
        if (HasInitialized)
        {
            throw new Exception("Invoker has already been initialized!");
        }
        var platforms = Cl.GetPlatformIDs(out var erc);
        erc.ThrowIfInvalid("Platforms");
        foreach (var platform in platforms)
        {
            Devices = Cl.GetDeviceIDs(platform, DeviceType.All, out erc).ToList();
            erc.ThrowIfInvalid("Device");
            CurrentPlatform = platform;
        }
        long lastMax = 0;
        foreach (var device in Devices)
        {
            var cc = device.CoreClock();
            if (cc > lastMax)
            {
                lastMax = cc;
                FastestDevice = device;
            }
        }
        lastMax = 0;
        foreach (var device in Devices)
        {
            var maxm = device.MemorySize();
            if (maxm > lastMax)
            {
                lastMax = maxm;
                DeviceWithMostMemory = device;
            }
        }
        var d = FastestDevice;
        if (!d.IsValid())
        {
            ErrorCode.InvalidDevice.ThrowIfInvalid("InvalidDevice_Check");
        }
        CurrentContext = d.CreateContextWithDevice(InvokeNotification);
        var program = CurrentProgram = CurrentContext.CreateProgramFromSource(Includes);
        program.BuildProgram(d, Arguments).ThrowIfBuildFailed(d);
        CurrentKernel = program.CreateKernel(kernelName);
        CurrentCommandQueue =
            Cl.CreateCommandQueue(CurrentContext, d, CommandQueueProperties.None, out var err);
        err.ThrowIfInvalid("CreateCommandQueue");
        HasInitialized = true;
    }

    public unsafe void Invoke<T>(T** inputArrays, T** outputArrays, int numInputArrays, int numOutputArrays, int itemsInArrays, int sizeOfT, bool writeInputs = true) where T : unmanaged
    {
        ThrowIfNotInitialized();
        var context = CurrentContext;
        var kernel = CurrentKernel;
        Buffers = new IMem[numInputArrays + numOutputArrays];
        CommandQueue cq = CurrentCommandQueue;
        for (int i = 0; i < numInputArrays; i++)
        {
            var arr = inputArrays[i];
            IMem mem;
            if (writeInputs)
            {
                mem = Buffers[i] = context.CreateClStackBuffer<T>(arr, sizeOfT, out var ptr, MemFlags.CopyHostPtr | MemFlags.WriteOnly);
            }
            else
            {
                mem = Buffers[i] = context.CreateClBuffer<T>(itemsInArrays);
            }
            kernel.AssignParameter(i, mem);
        }
        for (int i = numInputArrays; i < numInputArrays + numOutputArrays; i++)
        {
            var arr = (void*) outputArrays[i-numInputArrays];
            var mem = Buffers[numInputArrays] = context.CreateClStackBuffer<T>(arr, sizeOfT, out var ptr, MemFlags.CopyHostPtr | MemFlags.ReadWrite);
            kernel.AssignParameter(i, mem);
        }
        
        Cl.EnqueueNDRangeKernel(cq, kernel, 1, null, new IntPtr[]
        {
            (IntPtr) itemsInArrays
        }, null, 0, null, out var clev);
        for (int i = numInputArrays; i < numOutputArrays + numInputArrays; i++)
        {
            var buffer = Buffers[i];
            Cl.EnqueueReadBuffer(cq, buffer, Bool.True, 
                                 (IntPtr) 0, (IntPtr) itemsInArrays, (IntPtr) outputArrays[i - numInputArrays], 
                                 0, null, out clev);
        }
        Cl.Finish(cq); 
    }
    public void Invoke<T>(T[][] inputArrays, T[][] outputArrays, int length, bool writeInputs = true) where T : struct
    {
        ThrowIfNotInitialized();
        var context = CurrentContext;
        var kernel = CurrentKernel;
        var list = new List<T[]>(inputArrays.Length + outputArrays.Length);
        list.AddRange(inputArrays);
        list.AddRange(outputArrays);
        ClearBuffers();
        Buffers = new IMem[list.Count];
        CommandQueue cq = CurrentCommandQueue;
        for (int i = 0; i < inputArrays.Length; i++)
        {
            var arr = list[i];
            IMem mem;
            if (writeInputs)
            {
                mem = Buffers[i] = context.CreateClBuffer(arr, (length > 25_000 ? MemFlags.CopyHostPtr : MemFlags.UseHostPtr) | MemFlags.WriteOnly);
            }
            else
            {
                mem = Buffers[i] = context.CreateClBuffer<T>(arr.Length);
            }
            kernel.AssignParameter(i, mem);
        }
        for (int i = inputArrays.Length; i < outputArrays.Length + inputArrays.Length; i++)
        {
            var arr = list[i];
            var mem = Buffers[i] = context.CreateClBuffer<T>(arr.Length);
            kernel.AssignParameter(i, mem);
        }
        
        Cl.EnqueueNDRangeKernel(cq, kernel, 1, null, new IntPtr[]
        {
            (IntPtr) length
        }, null, 0, null, out var clev);
        for (int i = inputArrays.Length; i < outputArrays.Length + inputArrays.Length; i++)
        {
            var buffer = Buffers[i];
            Cl.EnqueueReadBuffer(cq, buffer, Bool.True, 0, length, outputArrays[i - inputArrays.Length], 0, null, out clev);
        }
        Cl.Finish(cq);
        ClearBuffers();
    }
    public void Invoke<T>(T[] inputArray, T[] outputArray, int length, bool writeInput = true) where T : struct
    {
        ThrowIfNotInitialized();
        if (inputArray == null && outputArray == null)
        {
            //throw new Exception("Cannot invoke code on the gpu, both arrays are null!");
            inputArray = new T[length];
            outputArray = new T[length];
        }
        
        var context = CurrentContext;
        var kernel = CurrentKernel;
        var inBuffer = context.CreateClBuffer<T>(length);
        var outBuffer = context.CreateClBuffer<T>(length);
        Buffers = new IMem[]
        {
            inBuffer,
            outBuffer
        };
        kernel.AssignParameter(0, inBuffer);
        kernel.AssignParameter(1, outBuffer);
        CommandQueue cq = CurrentCommandQueue;
        if (writeInput)
        {
            Cl.EnqueueWriteBuffer(cq, inBuffer, Bool.True, inputArray, 0, null, out var e).ThrowIfInvalid();
        }

        Cl.EnqueueNDRangeKernel(cq, kernel, 1, null, new IntPtr[]
        {
            (IntPtr) length
        }, null, 0, null, out var clev);
        Cl.EnqueueReadBuffer(cq, outBuffer, Bool.True, 0, length, outputArray, 0, null, out clev);
        Cl.Finish(cq);
        ClearBuffers();
    }

    public async Task InvokeAsync<T>(T[] inputArray, T[] outputArray, int length, bool writeInput = true) where T : struct
    {
        await Task.Run(() =>
        {
            Invoke(inputArray, outputArray, length);
        });
    }

    public void Dispose()
    {
        ThrowIfNotInitialized();
        CurrentContext.Dispose();
        CurrentKernel.Dispose();
        CurrentProgram.Dispose();
        CurrentCommandQueue.Dispose();
        ClearBuffers();
        Buffers = null;
        HasInitialized = false;
    }

    public void ClearBuffers()
    {
        if (Buffers is null)
        {
            return;
        }
        ThrowIfNotInitialized();
        for (var i = 0; i < Buffers.Length; i++)
        {
            ref var buffer = ref Buffers[i];
            buffer?.Dispose();
            buffer = null;
        }
    }


    private void ThrowIfNotInitialized()
    {
        if (!HasInitialized)
        {
            throw new Exception("Invoker has not been initialized. Use Init() to initialize.");
        }
    }
    
    private void InvokeNotification(string errinfo, byte[] data, IntPtr cb, IntPtr userdata)
    {
        Notification?.Invoke(errinfo, data, cb, userdata);
    }
    
    ~Invoker()
    {
        Dispose();
    }
}