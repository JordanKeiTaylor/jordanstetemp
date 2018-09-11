using System.IO;
using System.Linq;
using Grpc.Core;
using Improbable.SpatialOS.Deployment.V1Alpha1;
using NUnit.Framework;

namespace platform_sdk_test
{
    [TestFixture]
    public class TestDeploymentService
    {
        public TestDeploymentService()
        {
            Platform.Setup();
        }
        
        [Test]
        public void Should_CreateDeployment()
        {
            var id = Test.Deployment.GenerateId();
            var projectName = Test.Project.Prefix + id;
            var deploymentName = Test.Deployment.Prefix + id;
            var launchConfig = File.ReadAllText(Test.Project.LaunchConfigFile);
            
            var operation = Platform.DeploymentService.CreateDeployment(new CreateDeploymentRequest
            {
                Deployment = new Deployment
                {
                    Id = id,
                    ProjectName = projectName,
                    Name = deploymentName,
                    LaunchConfig = new LaunchConfig
                    {
                        ConfigJson = launchConfig
                    }, 
                }
            });
            var deployment = operation.PollUntilCompleted().GetResultOrNull();
            
            Assert.AreEqual(id, deployment.Id);
            Assert.AreEqual(projectName, deployment.ProjectName);
            Assert.AreEqual(deploymentName, deployment.Name);
//            Assert.AreEqual(deployment.Status, Deployment.Types.Status.Running); //TODO: need assembly for this to work
        }
        
        [Test]
        public void Should_ListDeployments()
        {
            var id = Test.Deployment.Ids().First();
            var projectName = Test.Project.Prefix + id;
            var deploymentName = Test.Deployment.Prefix + id;
            
            var deployments = Platform.DeploymentService.ListDeployments(
                new ListDeploymentsRequest
                {
                    ProjectName = projectName,
                    DeploymentName = deploymentName
                }
            );
            
            Assert.IsTrue(deployments.Any(deployment => deployment.Name == deploymentName));
            Assert.IsTrue(deployments.Any(deployment => deployment.ProjectName == projectName));
        }

        [Test]
        public void Should_GetDeployment()
        {
            var id = Test.Deployment.Ids().First();
            var projectName = Test.Project.Prefix + id;
            var deploymentName = Test.Deployment.Prefix + id;
            
            var deployment = Platform.DeploymentService.GetDeployment(
                new GetDeploymentRequest
                {
                    Id = id,
                    ProjectName = projectName
                }
            ).Deployment;
            
            Assert.AreEqual(id, deployment.Id);
            Assert.AreEqual(projectName, deployment.ProjectName);
            Assert.AreEqual(deploymentName, deployment.Name);

        }
        
        [Test]
        public void Should_UpdateDeployment()
        {
            var tag = "new_tag";
            var id = Test.Deployment.Ids().First();
            var projectName = Test.Project.Prefix + id;
            var deploymentName = Test.Deployment.Prefix + id;
            
            var deployment = Platform.DeploymentService.GetDeployment(
                new GetDeploymentRequest
                {
                    Id = id,
                    ProjectName = projectName
                }
            ).Deployment;

            deployment.Tag.Add(tag);

            // Not Implemented
            Assert.Throws<RpcException>(() =>
            {
                var updatedDeployment = Platform.DeploymentService.UpdateDeployment(new UpdateDeploymentRequest
                {
                    Deployment = deployment
                }).Deployment;
            });
//            Assert.IsTrue(updatedDeployment.Tag.Contains(tag));
        }

        [Test]
        [Ignore("Deployments are currently created in the 'Error' state and cannot be stopped")]
        public void Should_StopDeployment()
        {
            var id = Test.Deployment.Ids().First();
            var projectName = Test.Project.Prefix + id;
            
            Platform.DeploymentService.StopDeployment(new StopDeploymentRequest
            {
                Id = id,
                ProjectName = projectName
            });
            
            var deployment = Platform.DeploymentService.GetDeployment(
                new GetDeploymentRequest
                {
                    Id = id,
                    ProjectName = projectName
                }
            ).Deployment;
            
            Assert.IsTrue(deployment.Status == Deployment.Types.Status.Stopping || 
                          deployment.Status == Deployment.Types.Status.Stopped);
        }
    }
}