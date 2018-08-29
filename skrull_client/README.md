# Skrull C# Client for Deployment and Snapshot Services

## Prerequisites
- [.NET Core 2.0](https://www.microsoft.com/net/core) or any .NET toolset supporting .NET Core 2.0 projects

## Building and running the C# solution
You can build and run the C# solution in an IDE, or using the .NET Core CLI.

### Using an IDE
1. Open [csharp.sln](csharp.sln) in your preferred IDE, configured to use the .NET Core Runtime.
2. Run any of the projects in the solution.

### Using the .NET Core CLI
Build:
```bash
dotnet build --configuration Release csharp.sln 
```

Execute:
```bash
dotnet ./bin/Release/netcoreapp2.0/SkrullClient.dll
```
Note: Do not use `dotnet run` because it will gobble up command-line parameters meant for the actual DLL program.  Additionally, `run` builds the solution by default-- though one can use `--no-build` to disable that.

## Installing C# Platform SDK
* Please see the [SpatialOS documentation](https://docs.improbable.io/reference/latest/platform-sdk/csharp/introduction)
