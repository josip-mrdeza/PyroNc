
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

}