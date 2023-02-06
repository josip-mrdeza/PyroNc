using System.Linq;
using System.Threading.Tasks;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;

namespace Pyro.Nc.Parsing.ArbitraryCommands;

public class MCALL : BaseCommand
{
    public MCALL(ITool tool, ICommandParameters parameters) : base(tool, parameters)
    {
    }

    public BaseCommand NextSubroutine { get; set; }
    public static BaseCommand Subroutine { get; set; }

    public static void ClearSubroutine()
    {
        Subroutine = null;
    }

    public static void CallSubroutine()
    {
        Globals.Console.Push($"Calling subroutine: '{Subroutine.Description}'!");
        Subroutine?.Execute(true);
    }
    
    public override async Task Execute(bool draw)
    {
        bool hasNewSubroutine = NextSubroutine != null;
        bool hasSelectedSubroutine = Subroutine != null;
        //var nextSubroutine = NextSubroutine.FindVariables().CollectCommands().FirstOrDefault() as BaseCommand;
        switch (hasNewSubroutine)
        {
            case true:
            {
                Subroutine = NextSubroutine;
                Globals.Console.Push($"MCALL selected next subroutine '{Subroutine.Description}'.");
                break;
            }
            case false:
            {
                if (hasSelectedSubroutine)
                {
                    Globals.Console.Push($"MCALL deselected subroutine '{Subroutine.Description}'.");
                    ClearSubroutine();
                }
                else
                {
                    Globals.Console.Push($"MCALL failure; Cause: No new subroutine or selected subroutine.");
                    throw new NoSubroutineIDProvidedException("MCALL requires a subroutine id (name) to be provided in it's initial call.", true);
                }
                break;
            }
        }
    }
}