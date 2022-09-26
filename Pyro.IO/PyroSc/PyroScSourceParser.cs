using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pyro.IO.PyroSc
{
    public class PyroScSourceParser
    {
        internal const string Namespace = "Pyro.IO.PyroSc.Keywords";
        internal readonly string[] KeywordTypes;
        internal readonly Dictionary<string, string> Mapping;
        internal readonly Dictionary<string, string> AlternateMapping;
        private string Source { get; }
        public KeywordLocalStorage Scope { get; private set; }
        public IEnumerable<Keyword>[] Words { get; private set; }
        private PyroScSourceParser(string text)
        {
            Source = text;
            Scope = new KeywordLocalStorage(); 
            Words = CreateEnumerableOnSource().ToArray();
            KeywordTypes = Assembly.GetAssembly(typeof(PyroScSourceParser)).GetTypes()
                                   .Where(t => t.Namespace == Namespace)
                                   .Select(t => t.Name)
                                   .ToArray();
            Mapping = KeywordTypes.ToDictionary(t => t.ToLower(), t => t);
            AlternateMapping = new Dictionary<string, string>()
            {
                {"{", "FunctionOpen"},
                {"}", "FunctionClose"}
            };
        }

        public static PyroScSourceParser FromFile(string file)
        {
            var text = File.ReadAllText(file);

            return new PyroScSourceParser(text);
        }

        public static PyroScSourceParser FromText(string text)
        {
            return new PyroScSourceParser(text);
        }

        public PyroScCompiler CreateCompiler()
        {
            return new PyroScCompiler(this);
        }

        public Keyword CreateKeyword(int lineIndex, int elementIndex, string content)
        {
            var lowerText = content.ToLower().Trim().TrimEnd();
            bool mapping = false;
            bool altMapping = false;
            string fullTypeName = null;
            
            if (Mapping.ContainsKey(lowerText))
            {
                mapping = true;
            }
            else if (AlternateMapping.ContainsKey(lowerText))
            {
                altMapping = true;
            } 
            
            if (mapping)
            {
                fullTypeName = $"{Namespace}.{Mapping[lowerText]}";
            }
            else if (altMapping)
            {
                fullTypeName = $"{Namespace}.{AlternateMapping[lowerText]}";
            }

            if (mapping || altMapping)
            {
                return Activator.CreateInstance(Type.GetType(fullTypeName, false, true)!, 
                                                new object[]{content, content, lineIndex, elementIndex, this}) as Keyword;
            }

            return new Keyword(null, content, lineIndex, elementIndex, this);
        }
        
        private IEnumerable<IEnumerable<Keyword>> CreateEnumerableOnSource()
        {
            string[] lines = Source.Split('\n');
            IEnumerable<string[]> lineContents = lines.Select(x => x.Split(' '));
            using IEnumerator<string[]> enumerator = lineContents.GetEnumerator();
            for (int i = 0; enumerator.MoveNext(); i++)
            {
                var line = enumerator.Current;
                IEnumerable<Keyword> words = CreateEnumerableOnLine(i, line);

                yield return words;
            }
        }

        public IEnumerable<Keyword> CreateEnumerableOnLine(int lineIndex, string[] line)
        {
            for (var i = 0; i < line.Length; i++)
            {
                var word = line[i];
                var keyword = CreateKeyword(lineIndex, i, word);
                yield return keyword;
            }
        }
        
    }
}