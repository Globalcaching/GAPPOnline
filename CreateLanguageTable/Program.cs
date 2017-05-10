using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CreateLanguageTable
{
    class Program
    {
        static HashSet<string> allEntries;

        static void Main(string[] args)
        {
            var curDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (string.Compare(curDir.Name, "GAPPOnline", true) != 0 && curDir.Parent!=null)
            {
                curDir = curDir.Parent;
            }
            if (string.Compare(curDir.Name, "GAPPOnline", true) == 0)
            {
                var projDir = curDir.GetDirectories("GAPPOnline");
                if (projDir.Length==1)
                {
                    curDir = projDir[0];
                }

                allEntries = new HashSet<string>();
                var sb = new StringBuilder();
                sb.AppendLine(@"namespace GAPPOnline.Data
    {
        //THIS FILE IS AUTO GENERATED DO NOT EDIT
        public static class OriginalText
        {
            public static string[] Entries = {
");

                //parse all cs and cshtml files
                ParseDirectory(curDir, sb);

                sb.AppendLine(@"        };
        }
    }");
                File.WriteAllText(Path.Combine(curDir.FullName, "Data", "OriginalText.cs"), sb.ToString());
            }
        }

        static void ParseDirectory(DirectoryInfo di, StringBuilder sb)
        {
            var allFiles = di.GetFiles("*.cs").ToList();
            allFiles.AddRange(di.GetFiles("*.cshtml"));
            foreach (var f in allFiles)
            {
                ParseFile(f, sb);
            }
            var allDirs = di.GetDirectories();
            foreach (var d in allDirs)
            {
                ParseDirectory(d, sb);
            }
        }
        static void ParseFile(FileInfo fi, StringBuilder sb)
        {
            Regex regex;
            if (string.Compare(fi.Extension, ".cs", true) == 0)
            {
                regex = new Regex("_T\\(\"(.*?)\"\\)");
            }
            else if (string.Compare(fi.Extension, ".cshtml", true) == 0)
            {
                regex = new Regex("Html.T\\(\"(.*?)\"\\)");
            }
            else
            {
                return;
            }
            var matches = regex.Matches(File.ReadAllText(fi.FullName));
            if (matches.Count > 0)
            {
                foreach (Match m in matches)
                {
                    var v = m.Groups[1].Value;
                    var entry = $"\"{v}\", //{fi.Name}";
                    if (allEntries.Contains(v))
                    {
                        entry = $"//{entry}";
                        //sb.AppendLine(entry);
                    }
                    else
                    {
                        allEntries.Add(v);
                        sb.AppendLine(entry);
                    }
                }
            }
        }
    }
}
