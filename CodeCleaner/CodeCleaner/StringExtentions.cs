using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeCleaner
{
    public static class StringExtensions
    {
        public static string[] SplitCamelCase(this string str)
        {
            return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2").Split(' ');
        }

        public static bool StartsWithUpperCase(this string str)
        {
            return (str[0] >= 'A' && str[0] <= 'Z');
        }
    }
}
