using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Pchp.Core;

namespace PhpieSdk.Library.Service
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
            Match str = Regex.Match(
                s, "^[`a-z0-9\\._\\\\]+", RegexOptions.IgnoreCase
            );
            return (str.Success) ? str.ToString() : s;
        }
        
        public static string ToOriginalTypeName(this string s)
        {
            return Regex.Match(
                s, "^[a-z0-9\\._\\[\\]]+", RegexOptions.IgnoreCase
            ).ToString();
        }

        public static Boolean IsPhpValue(this PhpValue phpValue)
        {
            return phpValue.ToString().Equals("Pchp.Core.PhpValue");
        }
        
        public static Boolean IsPhpNumber(this PhpValue phpValue)
        {
            return phpValue.ToString().Equals("Pchp.Core.PhpNumber");
        }
        
        public static void WriteLn(this string s, string v = "") 
        {
            Console.WriteLine($"[SDK] {V(v)}" + s);
        }
        
        public static void WriteLn(this object s, string v = "")
        {
            Console.WriteLine($"[SDK] {V(v)}" + s.ToString());
        }
    
        public static void WriteLn(this string[] s, string v = "")
        {
            Console.WriteLine($"[SDK] {V(v)}" + string.Join(", ", s));
        }
        
        public static void WarnLn(this string s, string v = "")
        {
            Console.Error.WriteLine($"[Warning] {V(v)}" + s);
        }
        
        public static void BenchmarkWriteLn(this string s, string v = "") 
        {
            Console.WriteLine($"[Benchmark] {V(v)}" + s);
        }

        private static string V(string v)
        {
            return v.Length <= 0 ? "" : $"{v} ";
        }

        public static bool IsPhpNameError(this string s)
        {
            return s.Contains('<') || s.Contains('>');
        }
        
        public static bool IsPhpNameFoundDot(this string s)
        {
            return s.Contains('.');
        }
        
        public static bool IsPhpNameGeneric(this string s)
        {
            return s.Contains('`') || s.Contains('[') || s.Contains(']');
        }
        
        public static string GetPhpImplName(this string s)
        {
            return s.Split('.').Last();
        }

        public static string GetMD5(this string s)
        {
            MD5 md5 = MD5.Create();
            return BitConvert(md5.ComputeHash(Encoding.UTF8.GetBytes(s)));
        }
        
        public static string GetMD5(this FileStream f)
        {
            MD5 md5 = MD5.Create();
            return BitConvert(md5.ComputeHash(f));
        }

        private static string BitConvert(byte[] hashBytes)
        {
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}



