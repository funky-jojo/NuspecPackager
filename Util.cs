using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandOfJoe.NuspecPackager
{
    public static class Util
    {
        private const string DIRECTORY_CONFIG_FILE_NAME = @"nuspec-packager.config";
        public static string ResolveRelativePath(string referencePath, string relativePath)
        {
            return Path.GetFullPath(Path.Combine(referencePath, relativePath));
        }
        public static void Serialize(NuspecItemConfig o, Stream stream)
        {
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(o.GetType());
            x.Serialize(stream, o);
        }
        public static T Deserialize<T>(string filePath)
        {
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var o = (T)x.Deserialize(fs);
                return o;
            }
        }

        internal static NuspecItemConfig GetDirectoryConfig(NuspecItemInfo item)
        {
            var filePath = Path.Combine(item.Directory, DIRECTORY_CONFIG_FILE_NAME);
            if (File.Exists(filePath))
            {
                return Deserialize<NuspecItemConfig>(filePath);
            }
            else
            {
                return new NuspecItemConfig();
            }
        }
        internal static NuspecItemConfig GetNuspecItemConfig(NuspecItemInfo item)
        {
            var filePath = Path.Combine(item.Directory, item.Name + "." + DIRECTORY_CONFIG_FILE_NAME);
            if (File.Exists(filePath))
            {
                return Deserialize<NuspecItemConfig>(filePath);
            }
            else
            {
                return new NuspecItemConfig();
            }
        }
        internal static string EnsureAbsolutePath(string path, string relativeFromDirectory)
        {
            if (!Path.IsPathRooted(path))
            {
                return Util.ResolveRelativePath(relativeFromDirectory, path);
            }
            else
            {
                return path;
            }
        }
    }
}
