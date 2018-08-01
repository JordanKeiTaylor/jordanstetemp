using System;
using System.IO;

namespace platform_sdk_test
{
    internal class Utility
    {
        private static string _projectPath = null;
        
        public static string ProjectPath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var tokens = baseDir.Split(Path.DirectorySeparatorChar);
            if (_projectPath == null)
            {
                _projectPath = "";
                foreach (var token in tokens)
                {
                    _projectPath += token + Path.DirectorySeparatorChar;
                    if (token == "ste") { break; }
                }
            }
            return _projectPath;
        }
    }
}