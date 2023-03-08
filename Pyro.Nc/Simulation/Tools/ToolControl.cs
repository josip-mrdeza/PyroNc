using System.Threading.Tasks;
using Pyro.Nc.Configuration;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Simulation.Machines;
using Pyro.Nc.Simulation.Workpiece;
using Pyro.Threading;
using UnityEngine;

namespace Pyro.Nc.Simulation.Tools;

public class ToolControl : MonoBehaviour
{
    public ToolManager Manager { get; private set; }
    public ToolBase SelectedTool => Globals.Tool;
    public MeshFilter Filter;
    private MachineBase Machine => MachineBase.CurrentMachine;

    public void Start()
    {
        Manager = new ToolManager();
        Manager.Init();
        Filter = SelectedTool.gameObject.GetComponentsInChildren<MeshFilter>()[1];
        Machine.SimControl.ResetSimulation();
    }

    public void LateUpdate()
    {
        //CheckForCollisionTask();
    }
    
    private void CheckForCollisionTask()
    {
        if (Machine is null)
        {
            return;
        }

        if (Machine.Workpiece is null)
        {
            return;
        }

        if (SelectedTool is null)
        {
            return;
        }
        if (Machine.Runner.CurrentContext is null)
        {
            return;
        }
        Vector3Range range = new Vector3Range(Machine.Workpiece.MinValues, Machine.Workpiece.MaxValues);
        if (Machine.Runner.CurrentContext.GetType() == typeof(G00))
        {
            if (IsInsideWorkpiece(range))
            {
                throw new RapidFeedCollisionException(Vector3.negativeInfinity);
            }
        }
        else
        {
            if (Machine.StateControl.IsFree || Machine.StateControl.IsPaused)
            {
                if (IsInsideWorkpiece(range))
                {
                    throw new WorkpieceCollisionException(Machine.Runner.CurrentContext, Vector3.negativeInfinity);
                }
            }
        }
    }

    private bool IsInsideWorkpiece(Vector3Range range)
    {
        return range.Fits(SelectedTool.Position);
    }
}