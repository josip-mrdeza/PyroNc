using System.Reflection;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.UI.Menu.InputHandlers
{
    public class FeedRateHandler : InputHandler
    {
        public override void OnEndEdit(string s)
        {
            if (CheckForInvalidTool()) return;
            if (string.IsNullOrEmpty(s)) return;
            Limiter limiter = ToolValuesIdTypeInfo.GetValue(ToolBase.Values) as Limiter;
            limiter!.Set(float.Parse(s));
        }
    }
}