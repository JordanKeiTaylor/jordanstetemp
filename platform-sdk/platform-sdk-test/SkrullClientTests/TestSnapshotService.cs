using Improbable.SpatialOS.Snapshot.V1Alpha1;
using NUnit.Framework;

namespace platform_sdk_test.SkrullClientTests
{
    [TestFixture]
    public class TestSnapshotService
    {

        [Test]
        public void ListSnapshotsTest()
        {
            Assert.Fail();
        }

        [Test]
        public void GetSnapshotTest()
        {
            Assert.Fail();
        }

        [Test]
        public void TakeSnapshotTest()
        {
            Assert.Fail();
        }

        [Test]
        public void UploadSnapshotTest()
        {
            SkrullPlatformClients.SnapshotServiceClient.UploadSnapshot(new UploadSnapshotRequest()
            {
                Snapshot = new Snapshot()
                {
                    ProjectName = "test_project_name", // Required
                    DeploymentName = "test_deployment_name" // Required
                }
            });
            Assert.Fail();
        }

        [Test]
        public void ConfirmUploadTest()
        {
            Assert.Fail();
        }
    }
}