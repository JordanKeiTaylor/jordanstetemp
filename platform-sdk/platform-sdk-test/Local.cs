using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace platform_sdk_test
{
    internal static class Local
    {
        private static readonly string PlatformPath = Path.Combine(Utility.ProjectPath(), "platform-sdk");
        
        private static readonly string ProjectPath = Path.Combine(Utility.ProjectPath(), "navmesh-worker-example");
        
        public const int Port = 9090;

        public static void Start()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(PlatformPath, "lib/spatiald"),
                Arguments = $"start " +
                            $"--port={Port} " +
                            $"--project_file={Path.Combine(ProjectPath, "spatialos.json")} " +
                            $"--dev_components=thor, gcontroller " +
                            $"--log_level=debug " +
                            $"--data_directory={Path.Combine(PlatformPath, "lib/data")}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            Process.Start(startInfo);

            while (!HasStarted())
            {
                Thread.Sleep(500);
            }
        }

        public static void Stop()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "spatial",
                Arguments = "service stop",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            Process.Start(startInfo)?.WaitForExit();
        }

        private static Boolean HasStarted()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "spatial",
                Arguments = "service status",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                if (output.Contains("is running"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}