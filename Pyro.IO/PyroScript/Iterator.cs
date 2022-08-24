using System.Collections.Generic;
using System.Linq;
using Pyro.IO.PyroScript.Keywords;

namespace Pyro.IO.PyroScript
{
    public static class Iterator
    {
        public static IEnumerable<LinkedPiece> EnumeratePiecesFromLine(this string s)
        {
            var split = s.Split(' ').ToArray();
            foreach (var str in split)
            {
                yield return new LinkedPiece(true, str, null, null);
            }
        }

        public static IEnumerable<LinkedPiece> LinkPieces(this IEnumerable<LinkedPiece> pieces)
        {
            using var enumerator = pieces.GetEnumerator();
            LinkedPiece last = null;
            while (enumerator.MoveNext())
            {
                var curr = enumerator.Current;
                if (curr.Value == "=>")
                {
                    curr = new Lambda(last, null);
                }
                if (last != null)
                {
                    last.Next = curr;
                    if (last.IsLambda)
                    {
                        last.Init();
                    }
                }
                curr.Previous = last;
                last = curr;
                

                yield return curr;
            }
        }
        
        
    }
}