using System.Collections.Generic;

namespace Pyro.Nc.Parser
{
    public static class Database
    {
        public static List<string> ArbitraryCommands = new()
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

        public static List<string> ModularCommands = new List<string>()
        {
            "G00", "G0",
            "G01", "G1",
            "G02", "G2",
            "G03", "G3",
            "G04", "G4"
        };
    }
}