using System;
using System.Collections.Generic;

namespace platform_sdk_test
{
    internal class Test
    {
        private const string ProjectPath = "../enterprise-starter-project";
        
        private static string _stePath = null;
        private static readonly Random _random = new Random();
        private static List<string> _snapshotIds = new List<string>();
        private static List<string> _deploymentIds = new List<string>();

        private static string StePath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var tokens = baseDir.Split(System.IO.Path.DirectorySeparatorChar);
            if (_stePath == null)
            {
                _stePath = "";
                foreach (var token in tokens)
                {
                    _stePath += token + System.IO.Path.DirectorySeparatorChar;
                    if (token == "ste") { break; }
                }
            }
            return _stePath;
        }

        internal static class Project
        {
            public static string Prefix => "project_";
            
            public static string LaunchConfigFile => System.IO.Path.Combine(
                StePath(), ProjectPath, "spatialos.json"
            );
        }

        internal static class Deployment
        {
            public static string Prefix => "deployment_";
        
            public static string GenerateId()
            {
                var id = _random.Next(1000).ToString("000");
                _deploymentIds.Add(id);
                return id;
            }

            public static IEnumerable<string> Ids()
            {
                return _deploymentIds;
            }
        }

        internal static class Snapshot
        {
            public static List<string> Ids { get; } = new List<string>();
            
            public static string File => System.IO.Path.Combine(
                StePath(), ProjectPath, "snapshots/default.snapshot"
            );
        }
    }
}