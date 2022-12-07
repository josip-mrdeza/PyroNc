using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Nc.Parsing;
using Pyro.Nc.UI;

namespace Pyro.Nc.Configuration.Managers
{
    public class ConfigurationFileManager : IManager
    {
        private readonly Dictionary<string, string> Required = new()
        {
            {"baseAddress.txt", BaseAddress}
        };
        private readonly Dictionary<string, string> ConfigurationRequired = new()
        {
            {"commandId.txt", CommandId},
            {"referencePoints.txt", ReferencePoints}
        };
        private string[] JGeneralRecommended = new[]
        {
            "JGeneral.IO.dll",
            "JGeneral.IO.Interop.dll"
        };

        private const string BaseAddress = "https://solidlink.azurewebsites.net/statisticsapi";
        private const string CommandId = "//You can tweak, add, or remove commands from this file as you wish.\n//To add comments for yourself use a prefix '//'.\n//Command ids begin with an integer ID, ex. (00) for G0/G00; Arbitrary commands do not require this since none of them contain an integer ID in their string id.\n//'##{SUFIX}' where 'SUFIX' is either 'G', 'M', or 'A' for GCommand, ModularCommand, ArbitraryCommand is used for notation the of a command sequence's end, and has to be used in order for the ValueStorage type to read the elements properly.\n00|G0\n01|G1\n02|G2\n03|G3\n04|G4\n05|G5\n09|G9\n40|G40\n41|G41\n42|G42\n54|G54\n61|G61\n64|G64\n70|G70\n71|G71\n80|G80\n81|G81\n82|G82\n90|G90\n91|G91\n96|G96\n97|G97\n##G\n00|M0\n01|M1\n02|M2\n03|M3\n04|M4\n05|M5\n06|M6\n30|M30\n##M\nCycle|Cycle\nDEF|DEF\nNotation|N\nLims|LIMS\nTrans|TRANS\nComment|;\nToolSetter|T\nFeedRateSetter|F\nSpindleSpeedSetter|S\n##A";
        private const string ReferencePoints = "//PointID:Vector3\n//Vector3 is a structure which can be interpreted as a json object as follows - (x,y,z) / (69,420,69)\n//M - Machine Zero Point\n//W - Workpiece zero point\n//A - Temporary workpiece point\n//N - Tool mount reference point\n//R - Reference point\n//B - Begin point\nM:(0,0,0)\nW:(0,0,0)\nA:(0,0,0)\nN:(0,0,0)\nR:(0,0,0)\nB:(0,0,0)";

        public bool IsAsync { get; }
        public bool DisableAutoInit { get; }

        public void Init()
        {
            LocalRoaming roaming = LocalRoaming.OpenOrCreate("PyroNc");
            foreach (var kvp in Required)
            {
                if (!roaming.Exists(kvp.Key))
                {
                    PyroConsoleView.PushTextStatic($"File '{kvp.Key}' does not exist in path '{roaming.Site}', adding from consts...");
                    roaming.AddFile(kvp.Key, kvp.Value);
                }
            }
            
            roaming = LocalRoaming.OpenOrCreate("PyroNc\\Configuration");
            foreach (var kvp in ConfigurationRequired)
            {
                if (!roaming.Exists(kvp.Key))
                {
                    PyroConsoleView.PushTextStatic($"File '{kvp.Key}' does not exist in path '{roaming.Site}', adding from consts...");
                    roaming.AddFile(kvp.Key, kvp.Value);
                }
            }
        }

        public Task InitAsync() => throw new System.NotImplementedException();
    }
}