namespace Pyro.IO.PyroScript
{
    public class Keyword : LinkedPiece
    {
        public Keyword(bool isParameter, string value, LinkedPiece previous, LinkedPiece next) : base(isParameter, value, previous, next)
        {
        }
    }
}