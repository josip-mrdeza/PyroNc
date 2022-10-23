using System.Collections.Generic;

namespace Pyro.Nc.Parser
{
    public static class Database
    {
        public static readonly List<string> ArbitraryCommands = new()
        {
            "TRANS",
            "LIMS",
            "T",
            "D",
            "S",
            "F",
            ";",
            "N"
        };

        public static readonly List<string> Cycles = new()
        {
            "Cycle81",
            "Cycle82",
            "Cycle83E",
            "Cycle84",
            "Cycle84E",
            "Cycle86",
            "Cycle87",
            "Cycle86",
            "Cycle87",
            "Cycle88",
            "Cycle89"
        };

        public static readonly List<string> ModularCommands = new List<string>()
        {
            "G00", "G0",
            "G01", "G1",
            "G02", "G2",
            "G03", "G3",
            "G04", "G4"
        };
    }
}