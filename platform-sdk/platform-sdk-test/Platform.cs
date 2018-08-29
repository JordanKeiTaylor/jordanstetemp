using System;
using System.IO;
using Improbable.SpatialOS.Deployment.V1Alpha1;
using Improbable.SpatialOS.Platform.Common;
using Improbable.SpatialOS.Snapshot.V1Alpha1;

namespace platform_sdk_test
{
    internal class Platform
    {
        private const int Port = 8080;
        private const string Hostname = "localhost";
        
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
    }
}