using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LandOfJoe.NuspecPackager
{
    internal class NuspecItemInfo
    {
        public string ProjectPath { get; set; }
        public string FileName { get; set; }
        public string Name
        {
            get
            {
                return Path.GetFileName((FileName ?? ""));
            }
        }
        public string Extension
        {
            get
            {
                return Path.GetExtension((FileName ?? "")).ToLower();
            }
        }
        public string Directory
        {
            get
            {
                return Path.GetDirectoryName((FileName ?? ""));
            }
        }

        public string ProjectUniqueName { get; internal set; }
        public string ProjectName { get; internal set; }
    }
}
