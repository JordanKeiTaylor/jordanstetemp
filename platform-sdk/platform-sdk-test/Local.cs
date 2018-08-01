using System.Diagnostics;
using System.IO;

namespace platform_sdk_test
{
    internal static class Local
    {
        private static readonly string ProjectPath = Path.Combine(Utility.ProjectPath(), "navmesh-worker-example");
        
        public const int Port = 9090;

        public static void Start()
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(ProjectPath, "lib/spatiald"),
                    Arguments = $"start " +
                                $"--port={Port} " +
                                $"--project_file={Path.Combine(ProjectPath, "spatialos.json")} " +
                                $"--dev_components=thor, gcontroller " +
                                $"--log_level=debug " +
                                $"--data_directory={Path.Combine(ProjectPath, "lib/data")}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
        }

        public static void Stop()
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "spatial",
                    Arguments = "service stop",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
        }
    }
}