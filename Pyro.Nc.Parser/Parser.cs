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
        private static readonly StringBuilder FixUnknownStringBuilder = new();
        public static IEnumerable<Block> FindBlocks(this string line)
        {
            using IEnumerator<string> enumerator = line.SplitNoAlloc(' ').GetEnumerator();

            for (int i = 0; enumerator.MoveNext(); i++)
            {
                Block current;
                var str = enumerator.Current;
                char first = str[0];
                if ((first is 'G' or 'M') || Database.ArbitraryCommands.Contains(str))
                {
                    if (str.Length < 1)
                    {
                        continue;
                    }
                    current = new Block(str, true);
                    switch (current.Text[0])
                    {
                        case ';':
                        {
                            current.AdditionalInfo = -69;
                            break;
                        }
                        case 'N':
                        {
                            current.AdditionalInfo = -420;
                            break;
                        }
                        case 'T':
                        {
                            current.AdditionalInfo = 1;
                            break;
                        }
                        case 'D':
                        {
                            current.AdditionalInfo = -1;
                            break;
                        }
                        case 'S':
                        {
                            current.AdditionalInfo = -42069;
                            break;
                        }
                        case 'F':
                        {
                            current.AdditionalInfo = 42069;
                            break;
                        }
                    }
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
            
            for (int i = 0; enumerator.MoveNext(); i++)
            {
                var current = enumerator.Current;
                if (current.IsCommand && Database.ModularCommands.Contains(current.Text))
                {
                    LastModular = current;
                    lastCommand = current;
                    yield return current;
                }
                else if (!current.IsCommand)
                {
                    if (lastCommand == null)
                    {
                        if (LastModular == null)
                        {
                            enumerator.Dispose();
                            throw new MissingModularCommandException(current);
                        }

                        yield return LastModular;
                        yield return current;
                        lastCommand = LastModular;
                        continue;
                    }

                    lastCommand = current;
                    yield return current;
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
    }
}