using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LandOfJoe.NuspecPackager
{

    public class NuspecItemConfig
    {

        public string NuGetExe { get; set; }

        public string OutputPath { get; set; }

        public bool? PackFromProject { get; set; }

        public bool? UploadToFeed { get; set; }

        public string PublishUrl { get; set; }

        public string RemoteFeedApiKey { get; set; }

        public bool? AppendV2ApiTrait { get; set; }

        public NuspecItemConfig()
        {
            this.PackFromProject = false;
        }

        /// <summary>
        /// populate this object's empty properties with values from the source object
        /// </summary>
        internal void MergeFrom(NuspecItemConfig source)
        {
            //if (String.IsNullOrWhiteSpace(this.NuGetExe))
            //{
            //    this.NuGetExe = source.NuGetExe;
            //}
            //if (String.IsNullOrWhiteSpace(this.OutputPath))
            //{
            //    this.OutputPath = source.OutputPath;
            //}
            var typeInfo = typeof(NuspecItemConfig).GetTypeInfo();
            var pis = typeInfo.GetRuntimeProperties();
            foreach (var pi in pis)
            {
                var currentValue = pi.GetValue(this);
                var sourceValue = pi.GetValue(source);
                if (source != null)
                {
                    pi.SetValue(this, sourceValue);
                }
            }
        }

        internal void EnsureAbsolutePaths(NuspecItemInfo item)
        {
            this.NuGetExe = Util.EnsureAbsolutePath(this.NuGetExe, item.Directory);
            this.OutputPath = Util.EnsureAbsolutePath(this.OutputPath, item.Directory);
        }

    }

}