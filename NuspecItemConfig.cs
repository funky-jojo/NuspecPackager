using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandOfJoe.NuspecPackager
{
    public class NuspecItemConfig
    {
        public string NuGetExe { get; set; }
        public string OutputPath { get; set; }

        /// <summary>
        /// populate this object's empty properties with values from the source object
        /// </summary>
        internal void MergeFrom(NuspecItemConfig source)
        {
            if (String.IsNullOrWhiteSpace(this.NuGetExe))
            {
                this.NuGetExe = source.NuGetExe;
            }
            if (String.IsNullOrWhiteSpace(this.OutputPath))
            {
                this.OutputPath = source.OutputPath;
            }
        }

        internal void EnsureAbsolutePaths(NuspecItemInfo item)
        {
            this.NuGetExe = Util.EnsureAbsolutePath(this.NuGetExe, item.Directory);
            this.OutputPath = Util.EnsureAbsolutePath(this.OutputPath, item.Directory);
        }
    }
}
