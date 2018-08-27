using System.IO;
using System.Linq;
using Improbable.SpatialOS.Deployment.V1Alpha1;
using NUnit.Framework;

namespace platform_sdk_test
{
    [TestFixture]
    public class DeploymentServiceTest
    {
        [Test]
        public void Should_CreateDeployment()
        {
            var launchConfig = File.ReadAllText(Project.LaunchConfigPath);
            var operation = Platform.DeploymentService.CreateDeployment(new CreateDeploymentRequest
            {
                Deployment = new Deployment
                {
                    Id = "1",
                    ProjectName = Project.Name,
                    Name = Project.DeploymentName,
                    LaunchConfig = new LaunchConfig
                    {
                        ConfigJson = launchConfig
                    }, 
                }
            });
            var deployment = operation.PollUntilCompleted().GetResultOrNull();
            
            Assert.IsNotNull(deployment);
            Assert.AreEqual(deployment.ProjectName, Project.Name);
            Assert.AreEqual(deployment.Name, Project.DeploymentName);
            Assert.AreEqual(Deployment.Types.Status.Running, deployment.Status);
        }
        
        [Test]
        public void Should_ListDeployments()
        {
            var deployments = Platform.DeploymentService.ListDeployments(
                new ListDeploymentsRequest
                {
                    ProjectName = Project.Name,
                    DeploymentName = Project.DeploymentName
                }
            );
            
            Assert.Greater(deployments.Count(), 0);
            
            foreach (var deployment in deployments)
            {
                Assert.AreEqual(Project.Name, deployment.ProjectName);
                Assert.AreEqual(Project.DeploymentName, deployment.Name);
            } 
        }

        [Test]
        public void Should_GetDeployment()
        {
            var deployment = Platform.DeploymentService.GetDeployment(
                new GetDeploymentRequest
                {
                    Id = Project.Id,
                    ProjectName = Project.Name
                }
            ).Deployment;
            
            Assert.NotNull(deployment);
            Assert.AreEqual(Project.Id, deployment.Id);
            Assert.AreEqual(Project.Name, deployment.Name);
        }
        
        [Test]
        public void Should_UpdateDeployment()
        {
            const string tag = "new_tag";
            
            var deployment = Platform.DeploymentService.GetDeployment(
                new GetDeploymentRequest
                {
                    Id = Project.Id,
                    ProjectName = Project.Name
                }
            ).Deployment;

            deployment.Tag.Add(tag);
            
            var updatedDeployment = Platform.DeploymentService.UpdateDeployment(new UpdateDeploymentRequest
            {
                Deployment = deployment
            }).Deployment;
            
            Assert.IsFalse(updatedDeployment.Tag.Contains(tag));
        }

        [Test]
        public void Should_StopDeployment()
        {
            Platform.DeploymentService.StopDeployment(new StopDeploymentRequest
            {
                Id = Project.Id,
                ProjectName = Project.Name
            });
            
            var deployment = Platform.DeploymentService.GetDeployment(
                new GetDeploymentRequest
                {
                    Id = Project.Id,
                    ProjectName = Project.Name
                }
            ).Deployment;
            
            Assert.IsTrue(deployment.Status == Deployment.Types.Status.Stopping || 
                          deployment.Status == Deployment.Types.Status.Stopped);
        }
    }
}