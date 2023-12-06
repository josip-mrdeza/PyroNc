
using System.Text;

namespace Pyro.IO;

public static class CastHelper
{
    public static TResult CastInto<TResult>(this object obj)
    {
        return (TResult) obj;
    }

    public static TResult SafeCastInto<TResult>(this object obj) where TResult : class
    {
        return obj as TResult;
    }

    public static string ConvertToUTF8Text(this byte[] data)
    {
        return Encoding.UTF8.GetString(data);
    }

    public static byte[] ConvertToUTF8Bytes(this string text)
    {
        return Encoding.UTF8.GetBytes(text);
    }
}