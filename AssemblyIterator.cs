using System;
using System.IO;
using System.Reflection;
using PchpSdkLibrary.Service;

namespace PchpSdkLibrary
{
    public abstract class AssemblyIterator: Configurations
    {

        protected Disassembler disassembler = new ();

        protected Manager manager;
        protected Assembly[] GetAssemblies() { return AppDomain.CurrentDomain.GetAssemblies(); }
    }

    public abstract class Configurations
    {
        protected string PathRoot { get; init; }
        protected string PathSdk { get; init; }

        protected Boolean IsAssemblyConfig()
        {
            string foundFolderPath = FindFolder(PathRoot, PathSdk);
            return foundFolderPath != null;
        }
        
        private string FindFolder(string directory, string folderName)
        {
            string[] folders = Directory.GetDirectories(directory, folderName, SearchOption.TopDirectoryOnly);
            if (folders.Length > 0)
            {
                return folders[0]; 
            }
            else
            {
                string[] subdirectories = Directory.GetDirectories(directory);
                foreach (string subdirectory in subdirectories)
                {
                    string result = FindFolder(subdirectory, folderName);
                    if (result != null)
                        return result;
                }
            }
    
            return null;
        }
    }
}