using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Improbable.SpatialOS.Snapshot.V1Alpha1;
using NUnit.Framework;

namespace platform_sdk_test
{
    [TestFixture]
    public class TestSnapshotService
    {
        public TestSnapshotService()
        {
            Platform.Setup();
        }
        
        [Test]
        public static void Should_ListSnapshots()
        {
            var id = Test.Deployment.Ids().First();
            var projectName = Test.Project.Prefix + id;
            var deploymentName = Test.Deployment.Prefix + id;
            
            var snapshots = Platform.SnapshotService.ListSnapshots(
                new ListSnapshotsRequest
                {
                    ProjectName = projectName,
                    DeploymentName = deploymentName
                }
            );
            
            Assert.IsTrue(snapshots.Any(snapshot => snapshot.DeploymentName == deploymentName));
            Assert.IsTrue(snapshots.Any(snapshot => snapshot.ProjectName == projectName));
        }

        [Test]
        public static void Should_GetSnapshot()
        {
            var id = Test.Deployment.Ids().First();
            var projectName = Test.Project.Prefix + id;
            var deploymentName = Test.Deployment.Prefix + id;
            
            var snapshot = Platform.SnapshotService.GetSnapshot(
                new GetSnapshotRequest
                {
                    Id = Test.Snapshot.Ids.First(),
                    ProjectName = projectName,
                    DeploymentName = deploymentName
                }
            ).Snapshot;
            
            Assert.IsTrue(Test.Snapshot.Ids.Any(snapshotId => snapshotId == snapshot.Id));
            Assert.AreEqual(deploymentName, snapshot.DeploymentName);
            Assert.AreEqual(projectName, snapshot.ProjectName);
        }
        
        [Test]
        [Ignore("Deployments are currently created in the 'Error' state and snapshots cannot be taken")]
        public static void Should_TakeSnapshot()
        {
            var id = Test.Deployment.Ids().First();
            var projectName = Test.Project.Prefix + id;
            var deploymentName = Test.Deployment.Prefix + id;
            
            var operation = Platform.SnapshotService.TakeSnapshot(
                new TakeSnapshotRequest
                {
                    Snapshot = new Snapshot
                    {
                        ProjectName = projectName,
                        DeploymentName = deploymentName
                    }
                }
            );
            operation.PollUntilCompleted();
            var snapshot = operation.GetResultOrNull();
            
            Assert.IsNotEmpty(snapshot.Id);
            Assert.Greater(snapshot.Size, 0);
            Assert.AreEqual(id, snapshot.Id);
            Assert.AreEqual(deploymentName, snapshot.DeploymentName);
            Assert.AreEqual(projectName, snapshot.ProjectName);
        }

        [Test]
        public static void Should_UploadSnapshot_And_ConfirmUpload()
        {
            var id = Test.Deployment.Ids().First();
            var projectName = Test.Project.Prefix + id;
            var deploymentName = Test.Deployment.Prefix + id;
            
            var snapshot = new Snapshot
            {
                ProjectName = projectName,
                DeploymentName = deploymentName
            };
            
            var bytes = File.ReadAllBytes(Test.Snapshot.File);
            using (var md5 = MD5.Create())
            {
                snapshot.Checksum = Convert.ToBase64String(md5.ComputeHash(bytes));
                snapshot.Size = bytes.Length;
            }

            var uploadResponse = Platform.SnapshotService.UploadSnapshot(
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
                    dataStream.Write(bytes, 0, bytes.Length);
                }
                httpRequest.GetResponse();
            }

            var confirmResponse = Platform.SnapshotService.ConfirmUpload(
                new ConfirmUploadRequest
                {
                    Id = newSnapshot.Id,
                    ProjectName = projectName,
                    DeploymentName = deploymentName
                }
            );
            
            Assert.IsNotNull(confirmResponse.Snapshot);
        }
    }
}