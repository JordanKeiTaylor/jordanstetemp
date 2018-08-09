using System;
using System.IO;
using Improbable.SpatialOS.Deployment.V1Alpha1;
using Improbable.SpatialOS.Platform.Common;
using Improbable.SpatialOS.Snapshot.V1Alpha1;

namespace platform_sdk_test
{
    internal class Platform
    { 
        private static Deployment _deployment;

        private static string LocalProjectName => "navmesh_walker";

        public static string ProjectName => "navmesh_walker";

        public static string DeploymentName => "local_navmesh_walker";

        private static string LaunchConfigFilePath => Path.Combine(Utility.ProjectPath(), "navmesh-worker-example/default_launch.json");

        public const int Port = 9090;
        
        public static readonly SnapshotServiceClient LocalSnapshotServiceClient = SnapshotServiceClient.Create(
            new PlatformApiEndpoint
            (
                "localhost",
                Port,
                true
            )
        );

        public static readonly DeploymentServiceClient LocalDeploymentServiceClient = DeploymentServiceClient.Create(
            new PlatformApiEndpoint
            (
                "localhost",
                Port,
                true
            )
        );

        public static void Setup()
        {
            var launchConfig = File.ReadAllText(LaunchConfigFilePath);

            var operation = LocalDeploymentServiceClient.CreateDeployment(new CreateDeploymentRequest
            {
                Deployment = new Deployment
                {
                    ProjectName = LocalProjectName,
                    Name = DeploymentName,
                    LaunchConfig = new LaunchConfig
                    {
                        ConfigJson = launchConfig
                    }, 
                }
            });
//            operation.PollUntilCompleted();
            _deployment = operation.GetResultOrNull();
        }

        public static void Cleanup()
        {
            if (_deployment != null)
            {
                LocalDeploymentServiceClient.StopDeployment(new StopDeploymentRequest
                {
                    Id = _deployment.Id,
                    ProjectName = _deployment.ProjectName
                });
            }
        }
    }
}