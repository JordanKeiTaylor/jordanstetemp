
using System.IO;

namespace platform_sdk_test
{
    internal static class Project
    {
        public static string Id => "2";
        
        public static string Name => "solutions";

        public static string DeploymentName => "ebu_starter_proj";
        
        public static string LaunchConfigPath => Path.Combine(Utility.ProjectPath(), "../platform/local-cluster/2nodes.pb.json");
    }
}