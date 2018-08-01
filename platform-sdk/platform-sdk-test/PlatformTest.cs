using System;
using System.IO;
using System.Linq;
using Improbable.SpatialOS.Deployment.V1Alpha1;
using NUnit.Framework;

namespace platform_sdk_test
{
    [TestFixture]
    public class PlatformTest
    {
//        [SetUp]
//        public void SetUp()
//        {
//            Local.Start();
//            Platform.Setup();
//        }
//
//        [TearDown]
//        public void TearDown()
//        {
//            Platform.Cleanup();
//            Local.Stop();
//        }
        
        [Test]
        public void Should_ListDeployments()
        {
            Local.Start();
            
            var listRequest = new ListDeploymentsRequest
            {
                ProjectName = Platform.ProjectName,
                DeploymentName = Platform.DeploymentName
            };
            
            var listResponse = Platform.LocalDeploymentServiceClient.ListDeployments(listRequest);
            
            Assert.AreEqual(1, listResponse.Count());
            
            foreach (var deployment in listResponse)
            {
                Assert.AreEqual(Platform.ProjectName, deployment.ProjectName);
                Assert.AreEqual(Platform.DeploymentName, deployment.Name);
            }
        }
    }
}