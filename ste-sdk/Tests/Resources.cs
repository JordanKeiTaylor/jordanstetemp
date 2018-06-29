using System;
using System.Dynamic;
using System.IO;

namespace Tests
{
    internal static class Resources
    {
        #region Defined Resources
        
        public static string SanfranMicroGraphCsv => ToFullPath("graph/sanfran-micro.graph.csv");

        public static string SanfranMicroPointsCsv => ToFullPath("graph/sanfran-micro.points.csv");

        public static string L19ObjBin64 => ToFullPath("L19.obj.bin64");
        
        #endregion
        
        #region Private
        
        private static string _resourcePath = null;
        
        private static string ToFullPath(string path)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var tokens = baseDir.Split(Path.DirectorySeparatorChar);
            if (_resourcePath == null)
            {
                _resourcePath = "";
                foreach (var token in tokens)
                {
                    _resourcePath += token + Path.DirectorySeparatorChar;
                    if (token == "ste-sdk") { break; }
                }
            }
            return Path.Combine(_resourcePath, "resources", path);
        }
        
        #endregion
    }
}