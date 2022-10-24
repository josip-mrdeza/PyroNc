using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pyro.IO;
using Pyro.Nc.Parser.Exceptions;

namespace Pyro.Nc.Parser
{
    public static class Parser
    {
        private static Block LastModular;
        private static int Line;
        private static readonly StringBuilder FixUnknownStringBuilder = new();
        public static IEnumerable<Block> FindBlocks(this string line)
        {
            using IEnumerator<string> enumerator = line.SplitNoAlloc(' ').GetEnumerator();
            for (int i = 0; enumerator.MoveNext(); i++)
            {
                Block current;
                var str = enumerator.Current;
                char first = str[0];
                if ((first is 'G' or 'M') || Database.ArbitraryCommands.Exists(t => str.StartsWith(t)) || Database.Cycles.Contains(str))
                {
                    if (str.Length < 1)
                    {
                        continue;
                    }
                    current = new Block(str, true);
                    if (current.Text.StartsWith("Cycle"))
                    {
                        current.AdditionalInfo = Information.Cycle;
                        yield return current;
                        continue;
                    }

                    current.AdditionalInfo = current.Text[0] switch
                    {
                        ';' => Information.Comment,
                        'N' => Information.Notation,
                        'T' => Information.ToolSelection,
                        'D' => Information.DiameterCorrection,
                        'S' => Information.SpindleSpeed,
                        'F' => Information.FeedRate,
                        _   => Information.None
                    };
                    yield return current;
                }
                else
                {
                    current = new Block(str, false);
                    yield return current;
                }
            }
        }

        public static IEnumerable<Block> FixUnknown(this IEnumerable<Block> blocks)
        {
            using IEnumerator<Block> enumerator = blocks.GetEnumerator();

            Block lastCommand = null;
            
            for (int i = 0; enumerator.MoveNext(); i++, Line++)
            {
                var current = enumerator.Current;
                current.Line = Line;
                if (current.IsCommand && Database.ModularCommands.Contains(current.Text))
                {
                    LastModular = current;
                    lastCommand = current;
                    yield return current;
                }
                else if (current.IsCommand)
                {
                    lastCommand = current;
                    yield return current;
                }
                else// (!current.IsCommand)
                {
                    if (lastCommand == null)
                    {
                        if (LastModular == null)
                        {
                            enumerator.Dispose();
                            Line = 0;
                            lastCommand = null;
                            LastModular = null;
                            throw new MissingModularCommandException(current);
                        }

                        yield return LastModular;
                        yield return current;
                        lastCommand = LastModular;
                        continue;
                    }

                    // lastCommand = current;
                    // yield return current;
                }
            }
        }

        public static IEnumerable<Block> FindParameterBlocks(this IEnumerator<Block> blocks)
        {
            while (blocks.MoveNext() && !blocks.Current!.IsCommand)
            {
                yield return blocks.Current;
            }
        }
        
        public static IEnumerable<BuildingBlock> CreateBuildingBlocks(this IEnumerable<Block> basisBlocks)
        {
            Line = 0;
            using IEnumerator<Block> enumerator = basisBlocks.GetEnumerator(); 
            while (enumerator.MoveNext())
            {
                start:
                var current = enumerator.Current;
                if (current.IsCommand)
                {
                    var bb = new BuildingBlock(current, enumerator.FindParameterBlocks().ToArray());
                    bb.Full = Enumerable.Empty<Block>().Append(current).Concat(bb.Parameters);
                    yield return bb;

                    goto start;
                } 
            }
        }

        public static List<List<string[]>> FindBlocksAndCreateAll(this string text)
        {
            var lines = text.SplitNoAlloc('\n')
                            .Select(x => x.FindBlocks().FixUnknown().CreateBuildingBlocks().ToArray()).ToArray();
            List<List<string[]>> start = new List<List<string[]>>(lines.Length);
            List<string[]> list = new List<string[]>();
            foreach (var block in lines)
            {
                foreach (var buildingBlock in block)
                {
                    list.Add(buildingBlock.Full.Select(x => x.Text).ToArray());
                }
                start.Add(list);
                list.Clear();
            }

            return start;
        }
    }
}