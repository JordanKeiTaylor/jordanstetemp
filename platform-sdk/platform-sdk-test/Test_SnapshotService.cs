using System.IO;
using System.Linq;
using System.Net;
using Improbable.SpatialOS.Snapshot.V1Alpha1;
using NUnit.Framework;

namespace platform_sdk_test
{
    [TestFixture]
    public class SnapshotServiceTest
    {
        [Test]
        public static void Should_ListSnapshots()
        {
            var snapshots = Platform.SnapshotService.ListSnapshots(
                new ListSnapshotsRequest
                {
                    ProjectName = Project.Name,
                    DeploymentName = Project.DeploymentName
                }
            );
            
            Assert.Greater(snapshots.Count(), 0);
            foreach (var snapshot in snapshots)
            {
                Assert.AreEqual(Project.Name, snapshot.ProjectName);
                Assert.AreEqual(Project.DeploymentName, snapshot.DeploymentName);
            }
        }

        [Test]
        public static void Should_GetSnapshot()
        {
            var id = "0";
            var snapshot = Platform.SnapshotService.GetSnapshot(
                new GetSnapshotRequest
                {
                    Id = id
                }
            ).Snapshot;
            
            Assert.AreEqual(id, snapshot.Id);
            Assert.Greater(snapshot.Size, 0);
            Assert.AreEqual(Project.DeploymentName, snapshot.DeploymentName);
            Assert.AreEqual(Project.Name, snapshot.ProjectName);
        }
        
        [Test]
        public static void Should_TakeSnapshot()
        {
            var operation = Platform.SnapshotService.TakeSnapshot(
                new TakeSnapshotRequest
                {
                    Snapshot = new Snapshot
                    {
                        ProjectName = Project.Name,
                        DeploymentName = Project.DeploymentName
                    }
                }
            );
            operation.PollUntilCompleted();
            var snapshot = operation.GetResultOrNull();
            
            Assert.IsNotEmpty(snapshot.Id);
            Assert.Greater(snapshot.Size, 0);
            Assert.AreEqual(Project.DeploymentName, snapshot.DeploymentName);
            Assert.AreEqual(Project.Name, snapshot.ProjectName);
        }

        [Test]
        public static void Should_UploadSnapshot_And_ConfirmUpload()
        {
            var snapshotFile = "path_to_snapshot_file";
            var snapshot = new Snapshot
            {
                ProjectName = Project.Name,
                DeploymentName = Project.DeploymentName
            };

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
                    var bytes = File.ReadAllBytes(snapshotFile);
                    dataStream.Write(bytes, 0, bytes.Length);
                }
                httpRequest.GetResponse();
            }

            var confirmResponse = Platform.SnapshotService.ConfirmUpload(
                new ConfirmUploadRequest
                {
                    Id = newSnapshot.Id,
                    ProjectName = Project.Name,
                    DeploymentName = Project.DeploymentName
                }
            );
            
            Assert.IsNotNull(confirmResponse.Snapshot);
        }
    }
}