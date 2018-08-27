using System;
using System.IO;
using Improbable.SpatialOS.Deployment.V1Alpha1;
using Improbable.SpatialOS.Platform.Common;
using Improbable.SpatialOS.Snapshot.V1Alpha1;

namespace platform_sdk_test
{
    internal class Platform
    {
        public const string Hostname = "localhost";
        public const int Port = 8080;
        
        public static readonly SnapshotServiceClient SnapshotService = SnapshotServiceClient.Create(
            new PlatformApiEndpoint
            (
                Hostname,
                Port,
                true
            )
        );

        public static readonly DeploymentServiceClient DeploymentService = DeploymentServiceClient.Create(
            new PlatformApiEndpoint
            (
                Hostname,
                Port,
                true
            )
        );

        public static void Setup()
        {
            // TODO: Tag = {"my_live_tag"}
//            var launchConfig = File.ReadAllText(LaunchConfigFilePath);
//
//            var operation = LocalDeploymentServiceClient.CreateDeployment(new CreateDeploymentRequest
//            {
//                Deployment = new Deployment
//                {
//                    Id = "0",
//                    ProjectName = LocalProjectName,
//                    Name = DeploymentName,
//                    LaunchConfig = new LaunchConfig
//                    {
//                        ConfigJson = launchConfig
//                    }, 
//                }
//            });
////            operation.PollUntilCompleted();
//            _deployment = operation.GetResultOrNull();
        }

        public static void Cleanup()
        {
//            if (_deployment != null)
//            {
//                LocalDeploymentServiceClient.StopDeployment(new StopDeploymentRequest
//                {
//                    Id = _deployment.Id,
//                    ProjectName = _deployment.ProjectName
//                });
//            }
        }
    }
}