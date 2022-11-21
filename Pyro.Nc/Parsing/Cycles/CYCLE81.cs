using System;
using System.Threading.Tasks;
using Pyro.Math;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.ArbitraryCommands;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Parsing.Cycles;

public class CYCLE81 : Cycle
{
    public CYCLE81(ITool tool, ICommandParameters parameters, float[] splitParameters) : base(tool, parameters)
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
    
    public static string[] ParameterTexts = new[]
    {
        "RTP",
        "RFP",
        "SDIS",
        "DP",
        "DPR",
        "DTP"
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
        
        var currentPos = Tool.Position;
        currentPos.y = rfp + sdis;
        var g0 = new G00(Tool, new GCommandParameters(currentPos));
        await g0.Execute(true);

        var dp = Parameters.GetValue("DP");
        var dpr = Parameters.GetValue("DPR");
        
        if (!float.IsNaN(dpr))
        {
            currentPos.y = rfp - dp.Abs();
            var g1 = new G01(Tool, new GCommandParameters(currentPos));
            await g1.Execute(true); 
        }
        else if (!float.IsNaN(dp))
        {
            currentPos.y = dp;
            var g1 = new G01(Tool, new GCommandParameters(currentPos));
            await g1.Execute(true);
        }

        if (HoldAtBottom)
        {
            var dtp = Parameters.GetValue("DTP");
            await Task.Delay(TimeSpan.FromSeconds(dtp));
        }
        
        g0.Parameters.Values["Y"] = rtp;
        await g0.Execute(true);
    }
    
}