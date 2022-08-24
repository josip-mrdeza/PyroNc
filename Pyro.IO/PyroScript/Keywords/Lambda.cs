using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pyro.IO.PyroScript.Keywords
{
    public class Lambda : LinkedPiece
    {
        public Lambda(LinkedPiece prev, LinkedPiece next) : base(false, "=>", prev, next)
        {
            IsLambda = true;
        }

        public Type RequiredType { get; set; }
        public MethodInfo RequiredMethod { get; set; }
        
        public object Result { get; set; }

        public override void Init()
        {
            var tuple = (Previous.Value, Next.Value); 
            if (LambdaCache.ContainsKey(tuple))
            {
                var lambda = LambdaCache[tuple];
                RequiredType = lambda.RequiredType;
                RequiredMethod = lambda.RequiredMethod;
            }
            else
            {
                RequiredType = Type.GetType(Previous.Value, false, true) ?? LambdaTypes[Previous.Value];
                if (RequiredType is null)
                {
                    throw new ScriptLambdaInvalidPiecesException(Previous, Next, "Type name was invalid.");
                }
                
                RequiredMethod = RequiredType.GetMethod(Next.Value.Split('(').First(), (Next.Value.Split('(').Skip(1).FirstOrDefault() ?? "").Replace(')', '\0').Split(',').Select(Type.GetType).Where(t => t != null).ToArray()) ?? RequiredType.GetMethod(Next.Value);
                if (RequiredMethod is null)
                {
                    throw new ScriptLambdaInvalidPiecesException(Previous, Next, "Method name was invalid.");
                }
                
                if (!RequiredMethod.IsStatic)
                {
                    throw new ScriptLambdaInvalidPiecesException(Previous, Next, "Method must be one of static type.");
                }
            }
        }

        public override object Run()
        {
            List<object> arr = new List<object>();
            LinkedPiece last = Next.Next;
            StringBuilder builder = new StringBuilder();
            bool isString = false;
            for (int i = 0; i < 4;)
            {
                if (last != null && last.IsParameter)
                {
                    bool isLast = false;
                    if (last.Value[0] == '"')
                    {
                        isString = true;
                    }
                    if (last.Value.Last() == '"')
                    {
                        isString = false;
                        isLast = true;
                    }

                    if (isString)
                    {
                        builder.Append(last.Value).Append(' ');
                    }
                    else if (isLast)
                    {
                        builder.Append(last.Value);
                        builder.Replace('"', '\0');
                        arr.Add(builder.ToString());
                        builder.Clear();
                    }
                    else
                    {
                        arr.Add(last.Value);
                    }
                    
                    last = last.Next;
                    i++;

                    continue;
                }
                break;
            }
            Result = RequiredMethod.Invoke(null, Next.Next.IsParameter ? arr.ToArray() : _objects);

            return Result;
        }

        static Lambda()
        {
            AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetTypes()).ToArray().Mutate(t =>
            {
                Type[] ty = new Type[t.Select(z => z.Length).Sum()];
                var index = 0;
                foreach (var typeArray in t)
                {
                    foreach (var type in typeArray)
                    {
                        ty[index] = type;
                        var name = type.Name;
                        if (!LambdaTypes.ContainsKey(name))
                        {
                            LambdaTypes.Add(name, type);
                        }
                        foreach (var method in type.GetMethods(BindingFlags.Static))
                        {
                            var tuple = (type.FullName, method.Name);
                            LambdaCache.Add(tuple, new Lambda(null, null)
                            {
                                RequiredType = type,
                                RequiredMethod = method
                            });
                        }
                        index++;
                    }
                }

                return ty;
            });
        }
        
        internal static Dictionary<(string type, string method), Lambda> LambdaCache = new Dictionary<(string type, string method), Lambda>();
        internal static Dictionary<string, Type> LambdaTypes = new Dictionary<string, Type>();
        internal static object[] _objects = new object[0];
    }
}