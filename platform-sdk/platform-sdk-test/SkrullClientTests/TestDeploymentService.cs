using System.IO;
using Improbable.SpatialOS.Deployment.V1Alpha1;
using NUnit.Framework;

namespace platform_sdk_test.SkrullClientTests
{
    [TestFixture]
    public class TestDeployment
    {
        private static string LaunchConfigFilePath => Path.Combine(Utility.ProjectPath(), "/Users/christophergatto/Development/repos/enterprise-starter-project/default_launch.json");
        
        private static readonly string LaunchConfig = File.ReadAllText(LaunchConfigFilePath);
        
        private static readonly Deployment Deployment = new Deployment
        {
            Id = "0",
            ProjectName = "solutions",
            Name = "enterprise_starter_deployment",
            // RegionCode = null,
            // ClusterCode = null,
            // AssemblyId = null,
            // StartingSnapshotId = null,
            // Tag = { },
            // Status = Deployment.Types.Status.Unknown,
            LaunchConfig = new LaunchConfig {ConfigJson = LaunchConfig},
            // PlayerInfo = null,
            // StartTime = null,
            // StopTime = null
        };
        
        [Test]
        public void CreateDeploymentTest()
        {
            var createDeploymentResponse = SkrullPlatformClients.DeploymentServiceClient.CreateDeployment(
                new CreateDeploymentRequest { Deployment = Deployment });

            createDeploymentResponse.PollUntilCompleted();
            
            Assert.True(createDeploymentResponse.Result.Id.Equals(Deployment.Id));
        }

        [Test]
        public void GetDeploymentTest()
        {
            var getDeploymentResponse = SkrullPlatformClients.DeploymentServiceClient.GetDeployment(new GetDeploymentRequest
            {
                Id = "0",
                ProjectName = "test_project_name"
            });

            Assert.True(getDeploymentResponse.Deployment.Id.Equals(Deployment.Id));
        }

        [Test]
        public void UpdateDeploymentTest()
        {
            Assert.Fail();
        }

        [Test]
        public void ListDeploymentsTest()
        {
            Assert.Fail();
        }

        [Test]
        public void StopDeploymentTest()
        {
            Assert.Fail();
        }
    }
}