using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Improbable.SpatialOS.Deployment.V1Alpha1;
using Improbable.SpatialOS.Platform.Common;

namespace Commands
{
    public class DeploymentOptions : GlobalOptions
    {
        protected static DeploymentServiceClient GetDeploymentServiceClient(string host, int port)
        {
            return DeploymentServiceClient.Create(new PlatformApiEndpoint(host, port, true));
        }
    }

    [Verb("deployment-create", HelpText = "interact with the deployment service: create deployment")]
    public class DeploymentCreateOptions : DeploymentOptions
    {
        [Option('a', "assembly-id", HelpText = "assembly id", Required = true)]
        public string AssemblyId { get; set; }

        [Option('c', "launch-config-filepath", HelpText = "JSON launch config absolute filepath", Required = true)]
        public string LaunchConfigFilePath { get; set; }
        
        [Option('t', "tags", Separator = ',', HelpText = "deployment tags (comma separated with no leading or trailing spaces)")]
        public IEnumerable<string> Tags { get; set; }
        
        [Option('s', "snapshot-id", HelpText = "snapshot id to use", Required = true)]
        public string SnapshotId { get; set; }
        
        public static void ExecuteVerb(DeploymentCreateOptions opts)
        {
            Console.WriteLine("Create deployment");
            
            var request = new CreateDeploymentRequest
            {
                Deployment = new Deployment
                {
                    Id = opts.DeploymentName,
                    ProjectName = opts.ProjectName,
                    Name = opts.DeploymentName,
                    LaunchConfig = new LaunchConfig
                    {
                        ConfigJson = File.ReadAllText(opts.LaunchConfigFilePath)
                    },
                    Tag = { opts.Tags },
                    StartingSnapshotId = opts.SnapshotId,
                    AssemblyId = opts.AssemblyId
                }
            };
            
            try
            {
                var response = GetDeploymentServiceClient(opts.Host, opts.Port)
                    .CreateDeployment(request)
                    .PollUntilCompleted()
                    .GetResultOrNull();

                if (response != null && response.Status == Deployment.Types.Status.Running)
                {
                    Console.WriteLine("Successfully made a new deployment.");
                }
                else
                {
                    Console.Error.WriteLine("Failed to create new deployment!");
                }
            }
            catch (Grpc.Core.RpcException rpce)
            {
                Console.Error.WriteLine(rpce);
            }
        }
    }
    
    [Verb("deployment-list", HelpText = "interact with the deployment service: list all deployments")]
    public class DeploymentListOptions : DeploymentOptions
    {
        public static void ExecuteVerb(DeploymentListOptions opts)
        {
            Console.WriteLine("List deployments");
            var response = GetDeploymentServiceClient(opts.Host, opts.Port)
                .ListDeployments(new ListDeploymentsRequest{
                    ProjectName = opts.ProjectName,
                });
            var deployments = response.ToList();
            Console.WriteLine("Found [{0}] deployment(s)", deployments.Count);
            deployments.ForEach(deployment =>
            {
                if (opts.Verbose)
                {
                    ListVerboseOutput(deployment);
                }
                else
                {
                    ListNonVerboseOutput(deployment);
                }
            });
        }
            
        private static void ListVerboseOutput(Deployment deployment) {
            Console.WriteLine("Deployment found: {0} [{1}]", deployment.Name, deployment.Id);
            Console.WriteLine("    Status: {0}", deployment.Status);
            Console.WriteLine("    Project name: {0}", deployment.ProjectName);
            Console.WriteLine("    Assembly id: {0}", deployment.AssemblyId);
            Console.WriteLine("    Start snapshot id: {0}", deployment.StartingSnapshotId);
            Console.WriteLine("    Start-time: {0}", deployment.StartTime);
            Console.WriteLine("    Stop-time: {0}", deployment.StopTime);
            Console.WriteLine("    Tags: {0}", deployment.Tag);
            Console.WriteLine("    Worker flags: {0}", deployment.WorkerFlags);
            Console.WriteLine("    Launch config: {0}", deployment.LaunchConfig);
            Console.WriteLine("    Note: did not display [ClusterCode, PlayerInfo, PlayerName, RegionCode]");
        }
        
        private static void ListNonVerboseOutput(Deployment deployment) {
            Console.WriteLine("Deployment found: {0} [{1}] ({2})", deployment.Name, deployment.Id, deployment.Status);
        }
    }
}
