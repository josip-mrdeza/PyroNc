using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pyro.IO.Memory.Gpu;

public static class ClTranslator
{
    private static Dictionary<string, string> CSharpToCMapping = new Dictionary<string, string>()
    {
        {
            "[]", "*"
        },
        {
            "single", "float"
        }
    };
    public static string ConvertToKernelCode<T>(string cSharpInsideFunction) where T : IKernelCode
    {
        var method = typeof(T).GetMethods()[1];
        var arguments = method.GetParameters().Select(x =>
        {
            var type = x.ParameterType;
            string typeStr;
            typeStr = type.Name.ToLower();
            return $"__global {typeStr} {x.Name}";
        }).ToArray();
        StringBuilder builder = new StringBuilder();
        builder.Append("__kernel void ");
        builder.Append(method.Name)
               .Append('(');
        builder.Append(string.Join(", ", arguments));
        builder.Append(')');
        builder.AppendLine().Append("{");
        StringBuilder sb = new StringBuilder(cSharpInsideFunction);
        foreach (var kvp in CSharpToCMapping)
        {
            sb.Replace(kvp.Key, kvp.Value);
        }
        builder.Append(sb);
        sb.Clear();
        builder.AppendLine("}");
        foreach (var kvp in CSharpToCMapping)
        {
            builder.Replace(kvp.Key, kvp.Value);
        }

        return builder.ToString();
    }
}

/// <summary>
/// Must implement a desired function.
/// </summary>
public interface IKernelCode
{
    public int get_global_id(int rank);
    
}