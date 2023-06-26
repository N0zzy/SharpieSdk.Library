using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace SharpieSdk.Library.Service
{
     public class Manager
     {
          private string Filename { get; set; }
          public List<string> NugetCollection = new();
          public string NugetPackagesPath;
          public string OriginalTargetFrameworks;
          
          public Manager(string path)
          {
               Filename = path;
               Read();
          }

          private void Read()
          {
               JObject obj = ParseJson(GetJson());
               string keyPath = GetRestorePath(obj);

               var lObj = obj["projects"]?.Last().ToList();

               NugetPackagesPath = GetNugetPackagesPath(lObj);
               OriginalTargetFrameworks = GetOriginalTargetFrameworks(lObj);
                    
               Console.WriteLine("[SDK] nuget target framework: " + OriginalTargetFrameworks);
               Console.WriteLine("[SDK] nuget package path: " + NugetPackagesPath);
               
               SetNugetPackageCollection(lObj);
          }

          private String GetJson()
          {
               Console.WriteLine("[SDK] " + Filename);
               return File.ReadAllText(Filename);
          }

          private JObject ParseJson(string json)
          {
               return JObject.Parse(json);
          }

          private string GetRestorePath(JObject obj)
          {
               return obj["restore"]?.First?.ToString()
                    .Replace(":{}","")
                    .Replace("\"","");
          }

          private string GetOriginalTargetFrameworks(List<JToken> lObj)
          {
               return lObj?[0]?["restore"]?["originalTargetFrameworks"]?
                    .ToList()[0]
                    .ToString();
          }
          
          private string GetNugetPackagesPath(List<JToken> lObj)
          {
               return lObj?[0]?["restore"]?["packagesPath"]?
                    .ToString()
                    .Replace("\\", "/");
          }

          private void SetNugetPackageCollection(List<JToken> lObj)
          {
               foreach (var deps in lObj?[0]?
                             ["frameworks"]?
                             [OriginalTargetFrameworks]?
                             ["dependencies"]?
                             .ToList()!)
               {
                    var name = Regex.Match(
                         deps.ToString(), "^\"[a-z0-9\\.]+", RegexOptions.IgnoreCase
                    ).ToString().Replace("\"", "");
                    var version = Regex.Match(
                         deps.ToString(), "\\d+\\.\\d+\\.?\\d?[-a-z\\d]*", RegexOptions.IgnoreCase
                    ).ToString().Replace("\"", "");
                    NugetCollection.Add(name +"|" + version);
               }
          }
     }
}

