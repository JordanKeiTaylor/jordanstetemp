using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using CommandLine;
using Improbable.SpatialOS.Platform.Common;
using Improbable.SpatialOS.Snapshot.V1Alpha1;

namespace Commands
{
    public class SnapshotOptions : GlobalOptions
    {
        protected static SnapshotServiceClient GetSnapshotServiceClient(string host, int port)
        {
            return SnapshotServiceClient.Create(new PlatformApiEndpoint(host, port, true));
        }
    }

    [Verb("snapshot_list", HelpText = "list snapshots")]
    public class SnapshotListOptions : SnapshotOptions
    {
        public static void ExecuteVerb(SnapshotListOptions opts)
        {
            Console.WriteLine("List snapshots");

            var response = GetSnapshotServiceClient(opts.Host, opts.Port)
                .ListSnapshots(new ListSnapshotsRequest
                {
                    ProjectName = opts.ProjectName,
                    DeploymentName = opts.DeploymentName
                });

            var snapshots = response.ToList();
            Console.WriteLine("Found [{0}] snapshot(s)", snapshots.Count);
            snapshots.ForEach(snapshot =>
            {
                Console.WriteLine("Snapshot id found: {0} [{1} bytes]", snapshot.Id, snapshot.Size);
            });
        }
    }

    [Verb("snapshot_upload", HelpText = "upload snapshot")]
    public class SnapshotUploadOptions : SnapshotOptions
    {
        [Option('s', "snapshotfilepath", HelpText = "snapshot absolute file path", Required = true)]
        public string SnapshotFilePath { get; set;  }
        
        public static void ExecuteVerb(SnapshotUploadOptions opts)
        {
            Console.WriteLine("Upload snapshot [" + opts.SnapshotFilePath + "]");
            
            var snapshotOnDisk = new Snapshot
            {
                ProjectName = opts.ProjectName,
                DeploymentName = opts.DeploymentName
            };
            using (var md5 = MD5.Create())
            {
                var bytes = File.ReadAllBytes(opts.SnapshotFilePath);
                snapshotOnDisk.Checksum = Convert.ToBase64String(md5.ComputeHash(bytes));
                snapshotOnDisk.Size = bytes.Length;
            }

            var client = GetSnapshotServiceClient(opts.Host, opts.Port);
            var uploadSnapshotResponse = client.UploadSnapshot(new UploadSnapshotRequest {Snapshot = snapshotOnDisk});
            var snapshotToUpload = uploadSnapshotResponse.Snapshot;

            var httpRequest = WebRequest.Create(uploadSnapshotResponse.UploadUrl) as HttpWebRequest;
            httpRequest.Method = "PUT";
            httpRequest.ContentLength = snapshotToUpload.Size;
            httpRequest.Headers.Set("Content-MD5", snapshotToUpload.Checksum);
            using (var dataStream = httpRequest.GetRequestStream())
            {
                var bytesToSend = File.ReadAllBytes(opts.SnapshotFilePath);
                dataStream.Write(bytesToSend, 0, bytesToSend.Length);
            }

            httpRequest.GetResponse();
            var confirmUploadResponse = client.ConfirmUpload(new ConfirmUploadRequest
            {
                ProjectName = snapshotToUpload.ProjectName,
                DeploymentName = snapshotToUpload.DeploymentName,
                Id = snapshotToUpload.Id
            });

            if (!snapshotToUpload.Checksum.Equals(confirmUploadResponse.Snapshot.Checksum) ||
                (snapshotToUpload.Size != confirmUploadResponse.Snapshot.Size))
            {
                Console.Error.WriteLine("Snapshot not uploaded properly.  The size and/or MD5 checksum does not match." +
                                        "  Please consider snapshot id [" + snapshotToUpload.Id + "] to be corrupt.");
            }
            else
            {
                Console.WriteLine("Snapshot uploaded properly.  id: " + snapshotToUpload.Id);
            }
        }
    }
}