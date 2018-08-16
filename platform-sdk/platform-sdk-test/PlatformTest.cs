using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Improbable.SpatialOS.Deployment.V1Alpha1;
using Improbable.SpatialOS.Snapshot.V1Alpha1;
using NUnit.Framework;

namespace platform_sdk_test
{
    [TestFixture]
    public class PlatformTest : IDisposable 
    {
        public PlatformTest()
        {
            // TODO: Make these singletons for the test suite
            Local.Start();
            Platform.Setup();
        }
        
        [Test]
        public void Should_ListDeployments()
        {
            
            var deployments = Platform.LocalDeploymentServiceClient.ListDeployments(
                new ListDeploymentsRequest
                {
                    ProjectName = Platform.ProjectName,
                    DeploymentName = Platform.DeploymentName
                }
            );
            
            Assert.Greater(deployments.Count(), 0);
            
            Console.WriteLine("Deployment List Response");
            
            foreach (var deployment in deployments)
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
            
            var deployment = Platform.LocalDeploymentServiceClient.GetDeployment(
                new GetDeploymentRequest
                {
                    Id = id,
                    ProjectName = projectName
                }
            ).Deployment;
            
            Assert.NotNull(deployment);
            Assert.True(id.Equals(deployment.Id));
            Assert.True(projectName.Equals(deployment.ProjectName));

            Console.WriteLine($"Deployment {id}:\n{Regex.Replace(deployment.ToString(), @"\s+", "")}");
        }
        
        // TODO: Move to another test class
        [Test]
        public static void Should_ListSnapshots()
        {
            var snapshots = Platform.LocalSnapshotServiceClient.ListSnapshots(
                new ListSnapshotsRequest
                {
                    ProjectName = Platform.ProjectName,
                    DeploymentName = Platform.DeploymentName
                }
            );
            
            Assert.Greater(snapshots.Count(), 0);
            foreach (var snapshot in snapshots)
            {
                Assert.AreEqual(Platform.ProjectName, snapshot.ProjectName);
                Assert.AreEqual(Platform.DeploymentName, snapshot.DeploymentName);
            }
        }

        [Test]
        public static void Should_GetSnapshot()
        {
            var id = "0";
            var snapshot = Platform.LocalSnapshotServiceClient.GetSnapshot(
                new GetSnapshotRequest
                {
                    Id = id
                }
            ).Snapshot;
            
            Assert.AreEqual(id, snapshot.Id);
            Assert.Greater(snapshot.Size, 0);
            Assert.AreEqual(Platform.DeploymentName, snapshot.DeploymentName);
            Assert.AreEqual(Platform.ProjectName, snapshot.ProjectName);
        }
        
        [Test]
        public static void Should_TakeSnapshot()
        {
            var operation = Platform.LocalSnapshotServiceClient.TakeSnapshot(
                new TakeSnapshotRequest
                {
                    Snapshot = new Snapshot
                    {
                        ProjectName = Platform.ProjectName,
                        DeploymentName = Platform.DeploymentName
                    }
                }
            );
            operation.PollUntilCompleted();
            var snapshot = operation.GetResultOrNull();
            
            Assert.IsNotEmpty(snapshot.Id);
            Assert.Greater(snapshot.Size, 0);
            Assert.AreEqual(Platform.DeploymentName, snapshot.DeploymentName);
            Assert.AreEqual(Platform.ProjectName, snapshot.ProjectName);
        }

        [Test]
        public static void Should_UploadSnapshot_And_ConfirmUpload()
        {
            var snapshotFile = "path_to_snapshot_file";
            var snapshot = new Snapshot
            {
                ProjectName = Platform.ProjectName,
                DeploymentName = Platform.DeploymentName
            };

            var uploadResponse = Platform.LocalSnapshotServiceClient.UploadSnapshot(
                new UploadSnapshotRequest
                {
                    Snapshot = snapshot
                }
            );

            var newSnapshot = uploadResponse.Snapshot;
            var httpRequest = WebRequest.Create(uploadResponse.UploadUrl) as HttpWebRequest;
            if (httpRequest != null)
            {
                httpRequest.Method = "PUT";
                httpRequest.ContentLength = newSnapshot.Size;
                httpRequest.Headers.Set("Content-MD5", newSnapshot.Checksum);
                using (var dataStream = httpRequest.GetRequestStream())
                {
                    var bytes = File.ReadAllBytes(snapshotFile);
                    dataStream.Write(bytes, 0, bytes.Length);
                }
                httpRequest.GetResponse();
            }

            var confirmResponse = Platform.LocalSnapshotServiceClient.ConfirmUpload(
                new ConfirmUploadRequest
                {
                    Id = newSnapshot.Id,
                    ProjectName = Platform.ProjectName,
                    DeploymentName = Platform.DeploymentName
                }
            );
            
            Assert.IsNotNull(confirmResponse.Snapshot);
        }

        public void Dispose()
        {
            Platform.Cleanup();
            // Local.Stop();
        }
    }
}