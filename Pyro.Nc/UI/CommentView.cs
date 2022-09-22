using System;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.UI
{
    public class CommentView : View
    {
        public override void Start()
        {
            base.Start();
            Globals.Comment = this;
            PyroConsoleView.PushTextStatic("CommentView Initialized.");
        }
    }
}