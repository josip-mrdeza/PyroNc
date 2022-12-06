using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.Configuration.Sim3D_Legacy;

public static class RustBindings
{
    private const string NativeLibrary = "pyronc_rust.dll";
    private static RustWrapper Wrapper;
    private static Transform CubeTransform;
    private static Transform TempTransform;
    private static void CFunc(int i)
    {
        Wrapper.vert = Wrapper.vertices[i]; //new vector
        Wrapper.realVert = CubeTransform.TransformPoint(Wrapper.vert);
    }
    
    [DllImport(NativeLibrary, EntryPoint = "check_position_for_cut",
               ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
    public static extern CutResult check_position_for_cut(RustWrapper wrapper, bool throwIfCut, IntPtr func);

    public static CutResult check_position_for_cut_Rust(Transform cubeTr, Transform tempTr, RustWrapper wrapper, bool throwIfCut)
    {
        Wrapper = wrapper;
        CubeTransform = cubeTr;
        TempTransform = tempTr;
        return check_position_for_cut(wrapper, throwIfCut, Marshal.GetFunctionPointerForDelegate(CFunc));
    }

}