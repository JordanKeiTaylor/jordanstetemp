
namespace platform_sdk_test
{
    internal class Project
    {
        public static string Id => "0";
        
        public static string Name => "navmesh_walker";

        public static string DeploymentName => "local_navmesh_walker";

        public static string Path => "some/where";
        
        public static string LaunchConfigPath => System.IO.Path.Combine(Utility.ProjectPath(), "navmesh-worker-example/default_launch.json");
    }
}