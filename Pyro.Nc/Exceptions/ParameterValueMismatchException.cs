using System.Globalization;
using Pyro.Nc.Parsing;
using Pyro.Nc.Parsing.GCommands;

namespace Pyro.Nc.Exceptions;

public class ParameterValueMismatchException : NotifyException
{
    public ParameterValueMismatchException(BaseCommand command, string parameterId, bool asWarning = false) 
        : base($"[{command.GetType().Name}] - Invalid value: {command.Parameters.GetValue(parameterId).ToString(CultureInfo.InvariantCulture)}", asWarning)
    {
    }
}