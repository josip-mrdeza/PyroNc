using System;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.UI
{
    public class CommentView : View
    {
        public override void Initialize()
        {
            base.Initialize();
            Globals.Comment = this;
            PyroConsoleView.PushTextStatic("CommentView Initialized.");
        }
    }
}