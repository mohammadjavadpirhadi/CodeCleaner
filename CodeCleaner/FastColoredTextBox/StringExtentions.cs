using System.Text.RegularExpressions;

namespace CodeCleaner
{
    public static class StringExtensions
    {
        public static string[] SplitCamelCase(string str)
        {
            return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2").Split(' ');
        }

        public static bool StartsWithUpperCase(string str)
        {
            return (str[0] >= 'A' && str[0] <= 'Z');
        }
    }
}
