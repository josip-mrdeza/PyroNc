namespace Pyro.Nc.Configuration;

public static class StringHelper
{
    public static string FixEmptyString<T>(this string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return default(T).ToString();
        }

        return s;
    }
}