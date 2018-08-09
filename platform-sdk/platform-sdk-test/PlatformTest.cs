using System;
using System.Linq;
using System.Text.RegularExpressions;
using Improbable.SpatialOS.Deployment.V1Alpha1;
using NUnit.Framework;

namespace platform_sdk_test
{
    [TestFixture]
    public class PlatformTest : IDisposable
    {
        public PlatformTest()
        {
            Local.Start();
            Platform.Setup();
        }
        
        [Test]
        public void Should_ListDeployments()
        {
            var listRequest = new ListDeploymentsRequest
            {
                ProjectName = Platform.ProjectName,
                DeploymentName = Platform.DeploymentName
            };
            
            var listResponse = Platform.LocalDeploymentServiceClient.ListDeployments(listRequest);
            
            Assert.AreEqual(1, listResponse.Count());
            
            Console.WriteLine("Deployment List Response");
            
            foreach (var deployment in listResponse)
            {
                Assert.AreEqual(Platform.ProjectName, deployment.ProjectName);
                Assert.AreEqual(Platform.DeploymentName, deployment.Name);
                
                Console.WriteLine($"\tProject: {deployment.ProjectName}, " +
                                  $"Deployment Name: {deployment.Name}, " +
                                  $"Deployment Id: {deployment.Id}");
            }
        }

        [Test]
        public void Should_GetDeployment()
        {
            const string id = "0";
            const string projectName = "navmesh_walker";
            
            var getDeployment = new GetDeploymentRequest()
            {
                Id = id,
                ProjectName = projectName
            };

            var deployment = Platform.LocalDeploymentServiceClient.GetDeployment(getDeployment).Deployment;
            
            Assert.NotNull(deployment);
            Assert.True(id.Equals(deployment.Id));
            Assert.True(projectName.Equals(deployment.ProjectName));

            Console.WriteLine($"Deployment {id}:\n{Regex.Replace(deployment.ToString(), @"\s+", "")}");
        }

        public void Dispose()
        {
            Platform.Cleanup();
            Local.Stop();
        }
    }
}