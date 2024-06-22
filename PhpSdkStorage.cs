using System.Collections.Generic;

namespace PhpieSdk.Library;

public class PhpSdkStorage
{
    public class Assembly
    {
        public static string Name { get; set; } = null;
        
        public static void Clear()
        {
            Name = null;
        }
    }
    
    public class Type
    {
        public static System.Type Instance { get; set; } = null;
        
        public static string EventType { get; set; } = null;
        
        public class Model
        {
            public static string Path { get; set; } = null;
            public static string Name { get; set; } = null;
            public static string Namespace { get; set; } = null;
            public static string Extends { get; set; } = null;
            public static string[] Implements { get; set; } = null;
            
            public static void Clear()
            {
                Path = null;
                Name = null;
                Namespace = null;
                Extends = null;
                Implements = null;
            }
        }
        
        public static string Name { get; set; } = null;
        public static string Title { get; set; } = null;
        public static string FullName { get; set; } = null;
        public static string Namespace { get; set; } = null;
        
        public static void Clear()
        {
            Name = null;
            FullName = null;
            Namespace = null;
        }
    }
    public static Dictionary<string, List<string>> Files { get; set; } = new();
}