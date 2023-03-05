using System;
using Pyro.Nc.Configuration;
using Pyro.Nc.UI.Debug;
using Pyro.Nc.UI.PointViewers;
using UnityEngine;

namespace Pyro.Nc.Simulation;

public class ReferencePointHandler : MonoBehaviour
{
    public MachinePointViewer MachineZeroPoint;
    public WorkpiecePointViewer WorkpieceZeroPoint;
    public TemporaryPointViewer TemporaryWorkpiecePoint;
    public ToolMountPointViewer ToolMountReferencePoint;
    public ReferencePointViewer ReferencePoint;
    public BeginPointViewer BeginPoint;
    public TransPointViewer Trans;
    public void Awake()
    {
        Globals.ReferencePointHandler = this;
        MachineZeroPoint = new GameObject("machinezero").AddComponent<MachinePointViewer>();
        MachineZeroPoint.Init();
        MachineZeroPoint.Renderer.useWorldSpace = true;
        //MachineZeroPoint.Position = ReferencePointParser.MachineZeroPoint;
        
        WorkpieceZeroPoint = new GameObject("workpiecezero").AddComponent<WorkpiecePointViewer>();
        WorkpieceZeroPoint.Init();
        WorkpieceZeroPoint.Renderer.useWorldSpace = true;
        //WorkpieceZeroPoint.Position = ReferencePointParser.WorkpieceZeroPoint;

        TemporaryWorkpiecePoint = new GameObject("temporaryzero").AddComponent<TemporaryPointViewer>();
        TemporaryWorkpiecePoint.Init();
        TemporaryWorkpiecePoint.Renderer.useWorldSpace = true;
        //TemporaryWorkpiecePoint.Position = ReferencePointParser.TemporaryWorkpiecePoint;

        ToolMountReferencePoint = new GameObject("toolzero").AddComponent<ToolMountPointViewer>();
        ToolMountReferencePoint.Init();
        ToolMountReferencePoint.Renderer.useWorldSpace = true;
        //ToolMountReferencePoint.Position = ReferencePointParser.ToolMountReferencePoint;

        ReferencePoint = new GameObject("refzero").AddComponent<ReferencePointViewer>();
        ReferencePoint.Init();
        ReferencePoint.Renderer.useWorldSpace = true;
        //ReferencePoint.Position = ReferencePointParser.ReferencePoint;

        BeginPoint = new GameObject("beginzero").AddComponent<BeginPointViewer>();
        BeginPoint.Init();
        BeginPoint.Renderer.useWorldSpace = true;
        //BeginPoint.Position = ReferencePointParser.BeginPoint;

        Trans = new GameObject("transzero").AddComponent<TransPointViewer>();  
        Trans.Init();
        Trans.Renderer.useWorldSpace = true;
    }
}