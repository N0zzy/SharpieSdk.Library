using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Pchp.Core;

namespace PchpSdkLibrary.Service
{
    public static class SharpieExtension
    {
        public static string ToReversSlash(this string s)
        {
            return s.Replace("\\", "/");
        }
        
        public static string ToReplaceDot(this string s, string c)
        {
            return s.Replace(".",c);
        }
        
        public static string ToOriginalName(this string s)
        {
            return Regex.Match(
                s, "^[a-z0-9\\._]+", RegexOptions.IgnoreCase
            ).ToString();
        }
        
        public static string ToOriginalTypeName(this string s)
        {
            return Regex.Match(
                s, "^[a-z0-9\\._\\[\\]]+", RegexOptions.IgnoreCase
            ).ToString();
        }

        public static string ToMd5(this string s, string ns = "")
        {
            return MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(
                s + ns
            )).ToString();
        }

        public static Boolean IsPhpValue(this PhpValue phpValue)
        {
            return phpValue.ToString().Equals("Pchp.Core.PhpValue");
        }
        
        public static Boolean IsPhpNumber(this PhpValue phpValue)
        {
            return phpValue.ToString().Equals("Pchp.Core.PhpNumber");
        }
    }
}

