
using System.IO;

namespace platform_sdk_test
{
    internal static class Project
    {
        public static string Id => "0";
        
        public static string Name => "solutions";

        public static string DeploymentName => "ebu_starter_proj";
        
        public static string LaunchConfigPath => Path.Combine(Utility.ProjectPath(), "<path-to-config>");
    }
}