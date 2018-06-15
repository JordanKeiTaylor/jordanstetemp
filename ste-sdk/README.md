# STESDK

## Pre Configuration
The C# Worker SDK must be accessible by NuGet with the ID `Improbable.WorkerSdkCsharp`

From the directory containing `Improbable.WorkerSdkCsharp.dll`:

1. Create NuSpec File
```
nuget spec Improbable.WorkerSdkCsharp.dll
```

This spec file describes how the NuGet package will be deployed. Changes to this file may impact how the library is referenced in a `package.config`. 

2. Create NuGet Package
```
nuget pack Improbable.WorkerSdkCsharp.dll.nuspec
```

This creates a `.nupkg` file which contains both the spec and `dll` library.

3. Add Package to NuGet Soruce
```
nuget add Improbable.WorkerSdkCsharp.1.0.0.nupkg -source ~/.nuget/packages
```

This deploys a package to the specified source which in this example is the local NuGet cache.

To declare the local NuGet cache as a source:
```
nuget source add -name local -source
```

## Build
```
nuget restore
msbuild STESDK.sln /p:Configuration=Release
```

## Publish
```
nuget pack STESDK/STESDK.csproj
nuget add STESDK.*.nupkg -source ~/.nuget/packages/
```