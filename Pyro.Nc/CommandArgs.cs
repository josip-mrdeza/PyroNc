using System;
using Pyro.Math;
using Pyro.Math.Geometry;

namespace Pyro.Nc
{
    public struct CommandArgs
    {
        /// <summary>
        /// The id of the command being executed.
        /// </summary>
        public string Id;

        public GArgs GCommandArgs;
        public MArgs MCommandArgs;
        public LineTranslationSmoothness TranslationSmoothness;
        public CircleSmoothness CircleSmoothness;

        public CommandArgs(string line)
        {
            GCommandArgs = default;
            MCommandArgs = default;
            TranslationSmoothness = LineTranslationSmoothness.Fine;
            CircleSmoothness = CircleSmoothness.Fine;
            Id = null;
        }

        public override string ToString()
        {
            if (GCommandArgs.IsValid)
            {
                return GCommandArgs.ToString();
            }
            else
            {
                return MCommandArgs.ToString();
            }
        }
    }
}