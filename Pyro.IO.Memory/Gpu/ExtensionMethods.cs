using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using OpenCL.Net;

namespace Pyro.IO.Memory.Gpu;

public static class ExtensionMethods
{
    private static void Thrower(string errinfo, byte[] data, IntPtr cb, IntPtr userdata)
    {
        throw new Exception(errinfo + $" with {data?.Length} bytes of data\n in cb (IntPtr): {cb.ToString()}\n & userdata (IntPtr): {userdata.ToString()}");
    }
    internal static void ThrowIfInvalid(this ErrorCode error, string desc = null)
    {
        Invoker.InvokeNotification(error, desc);
        if (error != ErrorCode.Success)
        {
            Invoker.Instance.HasInitialized = true;
            Invoker.Instance.Dispose();
            throw new Cl.Exception(error);
        }
    }

    public static Context CreateContextWithDevice(this Device device, Cl.ContextNotify eventNotification = null)
    {
        var context = Cl.CreateContext(null, 1, new Device[]
        {
            device
        }, eventNotification ?? Thrower, IntPtr.Zero, out var err);
        err.ThrowIfInvalid("CreateContext");
        return context;
    }

    public static Program CreateProgramFromSource(this Context context, string[] sources)
    {
        var program = Cl.CreateProgramWithSource(context, 1, sources, null, out var err);
        err.ThrowIfInvalid("CreateProgramWithSource");
        return program;
    }

    public static Program BuildProgram(this Program program, Device device, string options = null)
    {
        var error = Cl.BuildProgram(program, 1, new Device[]
        {
            device
        }, options ?? string.Empty, null, IntPtr.Zero);
        //error.ThrowIfInvalid("BuildProgram");
        return program;
    }

    public static Program ThrowIfBuildFailed(this Program program, Device device)
    {
        var buildInfo = Cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.Status, out var error);
        error.ThrowIfInvalid("GetProgramBuildInfo_Status");
        var buildStatus = buildInfo.CastTo<BuildStatus>();
        if (buildStatus != BuildStatus.Success)
        {
            var err = Cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.Log, (IntPtr) 0, new InfoBuffer(), out var size);
            err.ThrowIfInvalid("GetProgramBuildInfo_Log_Size");
            var info = new InfoBuffer(size);
            err = Cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.Log, size, info, out size);
            //err.ThrowIfInvalid("GetProgramBuildInfo_Log_InfoBuffer");
            ErrorCode.BuildProgramFailure.ThrowIfInvalid(info.ToString());
        }

        return program;
    }

    public static Kernel CreateKernel(this Program program, string functionName)
    {
        var kernel = Cl.CreateKernel(program, functionName, out var error);
        error.ThrowIfInvalid("CreateKernel");
        return kernel;
    }

    public static IMem<T> CreateClBuffer<T>(this Context context, T[] data, MemFlags flags = MemFlags.None) where T : struct
    {
        var buffer = Cl.CreateBuffer<T>(context, flags, data, out var err);
        err.ThrowIfInvalid();
        return buffer;
    }
    
    public static IMem<T> CreateClBuffer<T>(this Context context, int length, MemFlags flags = MemFlags.None) where T : struct
    {
        var buffer = Cl.CreateBuffer<T>(context, flags, length, out var err);
        err.ThrowIfInvalid("CreateBuffer");
        return buffer;
    }
    
    public static unsafe IMem CreateClStackBuffer<T>(this Context context, void* ptr, int structSize, out ManagedStackPointer<T> pointer, MemFlags flags = MemFlags.None) where T : struct
    {
        pointer = new ManagedStackPointer<T>(ptr);
        var buffer = Cl.CreateBuffer(context, flags, (IntPtr) structSize, (IntPtr) ptr, out var err);
        err.ThrowIfInvalid("CreateBuffer");
        return buffer;
    } 
    
    public static unsafe IMem CreateClStackBuffer<T>(this Context context, int length, out ManagedStackPointer<T> pointer, MemFlags flags = MemFlags.None) where T : unmanaged
    {
        T* ptr = stackalloc T[length];
        pointer = ManagedStackPointer<T>.GetStackPointer(ptr);
        var buffer = Cl.CreateBuffer(context, flags, (IntPtr) sizeof(T), (IntPtr) ptr, out var err);
        err.ThrowIfInvalid("CreateBuffer");
        return buffer;
    } 

    public static void AssignParameter<T>(this Kernel kernel, int argSpot, IMem<T> array) where T : struct
    {
        Cl.SetKernelArg(kernel, (uint) argSpot, array);
    }
    
    public static void AssignParameter(this Kernel kernel, int argSpot, IMem array)
    {
        Cl.SetKernelArg(kernel, (uint) argSpot, array);
    }
    
    public static void AssignParameter<T>(this Kernel kernel, int argSpot, T value) where T : struct
    {
        Cl.SetKernelArg(kernel, (uint) argSpot, value);
    }
    
    public static void AssignParameter<T>(this Kernel kernel, int argSpot, T value, int tSize) where T : struct
    {
        Cl.SetKernelArg(kernel, (uint) argSpot, new IntPtr(tSize), value);
    }
    
    public static string Name(this Device device)
    {
        if (!device.IsValid())
        {
            ErrorCode.InvalidDevice.ThrowIfInvalid("InvalidDevice_Name");
        }
        var name = Cl.GetDeviceInfo(device, DeviceInfo.Name, out var error);
        error.ThrowIfInvalid("DeviceName");
        return name.ToString();
    }

    public static long CoreClock(this Device device)
    {
        if (!device.IsValid())
        {
            ErrorCode.InvalidDevice.ThrowIfInvalid("InvalidDevice_CoreClock");
        }
        var clock = Cl.GetDeviceInfo(device, DeviceInfo.MaxClockFrequency, out var error);
        error.ThrowIfInvalid("CoreClock");
        return clock.CastTo<long>();
    }
    
    public static float CoreClockInGHz(this Device device)
    {
        return (float) System.Math.Round(device.CoreClock() / 1000f, 2);
    }

    public static long MemorySize(this Device device)
    {
        if (!device.IsValid())
        {
            ErrorCode.InvalidDevice.ThrowIfInvalid("InvalidDevice_MemorySize");
        }
        var maxMalloc = Cl.GetDeviceInfo(device, DeviceInfo.GlobalMemSize, out var error);
        error.ThrowIfInvalid("MemorySize");
        return maxMalloc.CastTo<long>();
    }
    
    public static float MemorySizeInGB(this Device device)
    {
        return (float) System.Math.Round(device.MemorySize() / 1_000_000_000f, 2);
    }
}