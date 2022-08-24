using System;
using System.Collections.Generic;
using System.Text;
using Pyro.Math;
using Pyro.Nc.Parsing;

namespace Pyro.Nc
{
    public static class Randomizer
    {
        private static Random _random = new Random();
        public static IEnumerable<string> GenerateRandomCommands(long numberOf)
        {
            for (int i = 0; i < numberOf; i++)
            {
                yield return GenerateRandomCommand();
            }
        }

        public static string GenerateRandomCommand()
        {
            void generateCircle(StringBuilder stringBuilder)
            {
                bool tr = false;
                if (_random.Next(0, 4) == 2)
                {
                    stringBuilder.Append("X");
                    generateNum();
                    stringBuilder.Append(' ');
                    tr = true;
                }

                if (_random.Next(0, 4) == 2)
                {
                    stringBuilder.Append("Y");
                    generateNum();
                    stringBuilder.Append(' ');
                    tr = true;
                }

                if (_random.Next(0, 3) == 0)
                {
                    stringBuilder.Append("I");
                    generateNum();
                    stringBuilder.Append(' ');
                    tr = true;
                }

                if (_random.Next(0, 3) == 0)
                {
                    stringBuilder.Append("J");
                    generateNum();
                    stringBuilder.Append(' ');
                    tr = true;
                }

                if (!tr)
                {
                    stringBuilder.Append("I");
                    generateNum();
                    stringBuilder.Append(" J");
                    generateNum();
                }
            }

            var val = _random.Next(1, 5);
            StringBuilder builder = new StringBuilder();

            void generateNum(bool canbeNegative = true)
            {
                builder.Append(((float) (_random.NextDouble() * 23.93d * (_random.Next(0, 2) == 1 && canbeNegative ? -1 : 1))).Round());
            }
            if (val == 0)
            {
                builder.Append("G00;G00 ");
                bool tr = false;
                if (_random.Next(0, 2) == 0)
                {
                    builder.Append("X");
                    generateNum();
                    builder.Append(' ');
                    tr = true;
                }
                if (_random.Next(0, 2) == 0)
                {
                    builder.Append("Y");
                    generateNum();
                    builder.Append(' ');
                    tr = true;
                }
                if(_random.Next(0, 2) == 0)
                {
                    builder.Append("Z");
                    generateNum();
                    builder.Append(' ');
                    tr = true;
                }

                if (!tr)
                {
                    builder.Append("X");
                    generateNum();
                }
            }
            else if (val == 1)
            {
                builder.Append("G01;G01 ");
                bool tr = false;
                if (_random.Next(0, 2) == 0)
                {
                    builder.Append("X");
                    generateNum();
                    builder.Append(' ');
                    tr = true;
                }
                if (_random.Next(0, 2) == 0)
                {
                    builder.Append("Y");
                    generateNum();
                    builder.Append(' ');
                    tr = true;
                }
                if(_random.Next(0, 2) == 0)
                {
                    builder.Append("Z");
                    generateNum();
                    builder.Append(' ');
                    tr = true;
                }

                if (!tr)
                {
                    builder.Append("X");
                    generateNum();
                }
            }
            else if (val == 2)
            {
                builder.Append("G02;G02 ");
                generateCircle(builder);
            }
            else if (val == 3)
            {
                builder.Append("G03;G03 ");
                generateCircle(builder);
            }
            else if (val == 4)
            {
                builder.Append("G04;G04 ");
                if (_random.Next(0, 3) == 2)
                {
                    builder.Append("P");
                    generateNum(false);
                }
                else
                {
                    builder.Append("S");
                    generateNum(false);
                }
            }

            return builder.ToString();
        }
    }
}