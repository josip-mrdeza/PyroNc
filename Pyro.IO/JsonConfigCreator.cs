using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pyro.IO;

public static class JsonConfigCreator
{
    private const char OpeningBracket = '{';
    private const char ClosingBracket = '}';
    private const char Quote = '"';
    private const char Colon = ':';
    public static IEnumerable<string> TraverseAssemblyAndCreateJsonFiles(this Assembly assembly, LocalRoaming roaming)
    {
        var types = assembly.GetTypes();
        StringBuilder builder = new StringBuilder();
        foreach (var type in types)
        {
            var num = 0;
            builder.Append('{');
            var props = type.GetRuntimeProperties().Cast<MemberInfo>().ToArray();
            var fields = type.GetRuntimeFields().Cast<MemberInfo>();
            var members = props.Concat(fields).ToArray();
            int proper = 0;
            for (var i = 0; i < members.Length; i++)
            {
                var prop = members[i];
                var attr = prop.GetCustomAttribute<StoreAsJson>();
                if (attr == null)
                {
                    continue;
                }

                object propBaseValue;
                if (prop is FieldInfo info)
                {
                    propBaseValue = Activator.CreateInstance(info.FieldType);
                }
                else
                {
                    propBaseValue = Activator.CreateInstance((prop as PropertyInfo).PropertyType);
                }

                if (proper > 0)
                {
                    builder.Append(',');
                }
                builder.Append(Quote).Append(attr.Name).Append(Quote);
                builder.Append(Colon);
                if (attr.VariableType == typeof(String))
                {
                    builder.Append(Quote);
                    builder.Append(propBaseValue);
                    builder.Append(Quote);
                }
                else
                {
                    builder.Append(propBaseValue);
                }

                proper++;
                num++;
            }

            builder.Append(ClosingBracket);
            if (num == 0)
            {
                builder.Clear();
                continue;
            }
            var str = builder.ToString();
            roaming.ModifyFile($"{type.Name}.json", str);
            yield return str;
            builder.Clear();
        }
    }
}     

public class StoreAsJson : Attribute
{
    public StoreAsJson(string name, Type type)
    {
        Name = name;
        VariableType = type;
    }
    public string Name { get; }
    public Type VariableType { get; }
}