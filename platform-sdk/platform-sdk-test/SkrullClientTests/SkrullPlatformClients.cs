using Improbable.SpatialOS.Deployment.V1Alpha1;
using Improbable.SpatialOS.Platform.Common;
using Improbable.SpatialOS.Snapshot.V1Alpha1;

namespace platform_sdk_test.SkrullClientTests
{
    internal class SkrullPlatformClients
    {
        private const string Hostname = "localhost";
        private const int Port = 8080;
        
        public static readonly SnapshotServiceClient SnapshotServiceClient = SnapshotServiceClient.Create(
            new PlatformApiEndpoint(Hostname, Port, true));

        public static readonly DeploymentServiceClient DeploymentServiceClient = DeploymentServiceClient.Create(
            new PlatformApiEndpoint(Hostname, Port, true));
    }
}