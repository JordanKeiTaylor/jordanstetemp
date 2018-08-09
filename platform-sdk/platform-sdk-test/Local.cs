using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace platform_sdk_test
{
    internal static class Local
    {
        private static readonly int StartChecks = 10;
        
        private static readonly string PlatformPath = Path.Combine(Utility.ProjectPath(), "platform-sdk");
        
        private static readonly string ProjectPath = Path.Combine(Utility.ProjectPath(), "navmesh-worker-example");
        
        public const int Port = 9090;

        public static void Start()
        {
            var startInfo = new ProcessStartInfo
            {
//                FileName = Path.Combine(PlatformPath, "lib/spatiald"),
//                Arguments = $"start " +
//                            $"--port={Port} " +
//                            $"--project_file={Path.Combine(ProjectPath, "spatialos.json")} " +
//                            $"--dev_components=thor, gcontroller " +
//                            $"--log_level=debug " +
//                            $"--data_directory={Path.Combine(PlatformPath, "lib/data")}",
                FileName = Path.Combine("spatial"),
                Arguments = "service start --port=9090",
                RedirectStandardOutput = true,
                WorkingDirectory = ProjectPath,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            Process.Start(startInfo);

            WaitForStart(StartChecks);
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

        private static Boolean HasStarted(out string output)
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
                output = process.StandardOutput.ReadToEnd();
                if (output.Contains("is running"))
                {
                    return true;
                }
                return false;
            }
            output = "Process to check spatiald status failed to start";
            return false;
        }

        private static void WaitForStart(int checkAttempts)
        {
            var output = "";
            var checks = 0;
            while (!HasStarted(out output) && checks < checkAttempts)
            {
                Thread.Sleep(500);
                checks++;
            }

            if (checks == checkAttempts)
            {
                throw new Exception("spatiald process failed to start: " + output);
            }
        }
    }
}