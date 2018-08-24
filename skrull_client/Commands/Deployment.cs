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

    [Verb("deployment_create", HelpText = "interact with the deployment service: create deployment")]
    public class DeploymentCreateOptions : DeploymentOptions
    {
        [Option('a', HelpText = "assembly id", Required = true)]
        public string AssemblyId { get; set; }

        [Option('c', HelpText = "JSON launch config absolute file path", Required = true)]
        public string LaunchConfigFilePath { get; set; }
        
        [Option('t', Separator = ',', HelpText = "tags to match")]
        public IEnumerable<string> Tags { get; set; }
        
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
                    AssemblyId = opts.AssemblyId
                }
            };
            
            try
            {
                var response = GetDeploymentServiceClient(opts.Host, opts.Port)
                    .CreateDeployment(request)
                    .PollUntilCompleted()
                    .GetResultOrNull();

                if (null == response)
                {
                    Console.Error.WriteLine("Failed to create new deployment!");
                }
                else
                {
                    Console.WriteLine("Successfully made a new deployment.");
                }
            }
            catch (Grpc.Core.RpcException rpce)
            {
                Console.Error.WriteLine(rpce);
            }
        }
    }
    
    [Verb("deployment_list", HelpText = "interact with the deployment service: list deployments")]
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
                Console.WriteLine("Deployment found: {0} [{1}]", deployment.Name, deployment.Status);
            });
        }
    }
}