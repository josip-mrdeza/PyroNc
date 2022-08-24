using System;

namespace Pyro.IO.PyroScript
{
    public class ScriptLambdaInvalidPiecesException : Exception
    {
        public ScriptLambdaInvalidPiecesException(LinkedPiece prev, LinkedPiece next) : base($"This Lambda's pieces were invalid;" +
            $" Previous: {prev?.Value}, Next: {next?.Value}")
        {
            
        }
        
        public ScriptLambdaInvalidPiecesException(LinkedPiece prev, LinkedPiece next, string message) : base($"This Lambda's pieces were invalid;" +
            $" Previous: {prev?.Value}, Next: {next?.Value}; {message}")
        {
            
        }
    }
}