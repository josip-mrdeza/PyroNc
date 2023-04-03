using System;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Parsing.Cycles;

public class CYCLE81 : Cycle
{
    public CYCLE81(ToolBase toolBase, ICommandParameters parameters, float[] splitParameters) : base(toolBase, parameters)
    {
        for (int i = 0; i < splitParameters.Length; i++)
        {
            parameters.AddValue(ParameterTexts[i], splitParameters[i]);
        }

        if (GetType() == typeof(CYCLE82))
        {
            HoldAtBottom = true;
        }
    }
    
    public bool HoldAtBottom { get; }

    public static readonly string[] ParameterDescriptions = new[]
    {
       "Retraction plane absolute",
       "Reference plane absolute",
       "Safety distance",
       "Drilling depth",
       "Final drilling depth relative to reference plane",
       "Dwell time at bottom"
    };
    
    public static readonly string[] ParameterTexts = new[]
    {
        "RTP",
        "RFP",
        "SDIS",
        "DP",
        "DPR",
        "DTB"
    };
    
    public override async Task Execute(bool draw)
    {
        var rtp = Parameters.GetValue("RTP");
        var rfp = Parameters.GetValue("RFP");

        if (rtp < rfp)
        {
            throw new CycleParameterException($"Parameter RTP[{rtp}]('Retraction Plane Absolute') must be greater than RFP[{rfp}]('ReferencePlaneAbsolute')!");
        }

        var sdis = Parameters.GetValue("SDIS").Abs();

        var currentPos = ToolBase.Position;
        currentPos.y = rfp + sdis;
        var g0 = new G00(ToolBase, new GCommandParameters(currentPos));
        SetOp(g0);
        await g0.Execute(true);

        var dp = Parameters.GetValue("DP");
        var dpr = Parameters.GetValue("DPR");
        G01 g1 = null;
        if (!float.IsNaN(dpr))
        {
            currentPos.y = rfp - dp.Abs();
            g1 = new G01(ToolBase, new GCommandParameters(currentPos));
            SetOp(g1);
            await g1.Execute(true); 
        }
        else if (!float.IsNaN(dp))
        {
            currentPos.y = dp;
            g1 = new G01(ToolBase, new GCommandParameters(currentPos));
            SetOp(g1);
            await g1.Execute(true);
        }

        if (HoldAtBottom)
        {
            var dtp = Parameters.GetValue("DTB");
            var g04 = new G04(ToolBase, new GCommandParameters(default));
            g04.Parameters.AddValue("P", dtp*1000);
            SetOp(g04);
            await g04.Execute(true);
        }
        
        g1.Parameters.Values["Y"] = rtp;
        SetOp(g1);
        await g1.Execute(true);
    }
    
}