using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using Improbable.SpatialOS.Deployment.V1Alpha1;
using Improbable.SpatialOS.Platform.Common;
using Improbable.SpatialOS.Snapshot.V1Alpha1;

namespace platform_sdk_test
{
    internal static class Platform
    {
        private static bool _isSetup;
        
        private const int Port = 8080;
        private const string Hostname = "localhost";
        
        public static readonly SnapshotServiceClient SnapshotService = SnapshotServiceClient.Create(
            new PlatformApiEndpoint
            (
                Hostname,
                Port,
                true
            )
        );

        public static readonly DeploymentServiceClient DeploymentService = DeploymentServiceClient.Create(
            new PlatformApiEndpoint
            (
                Hostname,
                Port,
                true
            )
        );

        public static void Setup()
        {
            if (_isSetup) return;

            var id = Test.Deployment.GenerateId();
            
            CreateSnapshot(id);
            CreateDeployment(id);

            _isSetup = true;
        }

        private static void CreateDeployment(string id)
        {
            var projectName = Test.Project.Prefix + id;
            var deploymentName = Test.Deployment.Prefix + id;
            var launchConfig = File.ReadAllText(Test.Project.LaunchConfigFile);
            
            var operation = DeploymentService.CreateDeployment(new CreateDeploymentRequest
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
            operation.PollUntilCompleted().GetResultOrNull();
        }

        private static void CreateSnapshot(string id)
        {
            var projectName = Test.Project.Prefix + id;
            var deploymentName = Test.Deployment.Prefix + id;
            
            var snapshot = new Snapshot
            {
                ProjectName = projectName,
                DeploymentName = deploymentName,
            };

            var bytes = File.ReadAllBytes(Test.Snapshot.File);
            using (var md5 = MD5.Create())
            {
                snapshot.Checksum = Convert.ToBase64String(md5.ComputeHash(bytes));
                snapshot.Size = bytes.Length;
            }
            
            var uploadResponse = SnapshotService.UploadSnapshot(
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
            
            Test.Snapshot.Ids.Add(newSnapshot.Id);
        }
    }
}