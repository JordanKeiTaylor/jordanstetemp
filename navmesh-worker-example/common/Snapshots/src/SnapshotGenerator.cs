using System;
using System.Reflection;
using CommandLine;
using CommandLine.Text;

namespace Snapshots {
    class Options {
        [Value(1, MetaName = "output", HelpText = "Output file.")]
        public string OutputFile { get; set; }
    }

    public class SnapshotGenerator {
        public static int Main(string[] args) {
            var parser = new Parser();
            var result = parser.ParseArguments<Options>(args);

            return result.MapResult(opts => Run(opts), errs => {
                Console.Error.Write(HelpText.AutoBuild(result));
                return 1;
            });
        }

        static int Run(Options options) {
            Assembly.Load("GeneratedCode");

            var entityIdGenerator = new EntityIdGenerator();

            Console.WriteLine("Writing to: {0}", options.OutputFile);
            using (var outputStream = new EntityOutputStream(options.OutputFile)) {
                new WalkerSupplier(entityIdGenerator).Generate(outputStream);
                Console.WriteLine("Summary:");
                outputStream.PrintSummary();
            }
            return 0;
        }
    }
}
