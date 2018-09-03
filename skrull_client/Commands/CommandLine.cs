using System;
using System.Collections.Generic;
using CommandLine;

namespace Commands
{
    public class GlobalOptions
    {
        [Option('h', "host", Default = "localhost", HelpText = "Skrull hostname")]
        public string Host { get; set; }

        [Option('p', "port", Default = 8080, HelpText = "Skrull port")]
        public int Port { get; set; }
        
        [Option('j', "project-name", Required = true, HelpText = "project name")]
        public string ProjectName { get; set; }
        
        [Option('d', "deployment-name", Required = true, HelpText = "deployment name")]
        public string DeploymentName { get; set; }
        
        [Option('v', "verbose", Required = false, HelpText = "verbose messages (only used in some commands)")]
        public bool Verbose { get; set; }
    }
    
    internal class OptionsParser
    {
        public static void Parse(IEnumerable<string> args)
        {
            var parser = new Parser(config => config.HelpWriter = Console.Out);
            
            var results = parser
                .ParseArguments<
                    DeploymentCreateOptions, DeploymentListOptions,
                    SnapshotListOptions, SnapshotUploadOptions>
                    (args)
                .WithParsed<DeploymentCreateOptions>(DeploymentCreateOptions.ExecuteVerb)
                .WithParsed<DeploymentListOptions>(DeploymentListOptions.ExecuteVerb)
                .WithParsed<SnapshotListOptions>(SnapshotListOptions.ExecuteVerb)
                .WithParsed<SnapshotUploadOptions>(SnapshotUploadOptions.ExecuteVerb)
                .WithNotParsed(PrintErrors);
        }

        private static void PrintErrors(IEnumerable<Error> errors)
        {
            Console.Error.WriteLine("ERROR");
            foreach (var err in errors)
            {
                Console.Error.WriteLine(err);
            }
        }
    }
}
