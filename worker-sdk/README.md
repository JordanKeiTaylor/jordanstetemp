# Worker SDK NuGet Artifacts

This repository builds the standard library schema classes and packages them, as well as packaging the Worker SDK separately. 

## Build `Improbable.StandardLibraryCsharp.csproj`
```
./setup.sh
msbuild Improbable.StandardLibraryCsharp.csproj /p:Configuration=Release
```

which should produce

```
.
├── ...
├── standard-library
│   ├── Improbable.StandardLibraryCsharp.dll
│   └── Improbable.StandardLibraryCsharp.pdb
└── worker-sdk
    ├── Improbable.WorkerSdkCsharp.dll
    ├── Improbable.WorkerSdkCsharp.xml
    └── target.spatialos_worker_packages.json.md5

```

## Package WorkerSDK and StandardLibrary Artifacts
```
nuget pack standard-library/Improbable.StandardLibraryCsharp.nuspec
nuget pack worker-sdk/Improbable.WorkerSdkCsharp.nuspec
```

which should produce

```
.
├── Improbable.StandardLibraryCsharp.12.2.0.nupkg
├── Improbable.WorkerSdkCsharp.12.2.0.nupkg
└── ...
```

## Publish WorkerSDK and StandardLibrary Artifacts
```
nuget add Improbable.StandardLibraryCsharp.12.2.0.nupkg -source ~/.nuget/packages/
nuget add Improbable.WorkerSdkCsharp.12.2.0.nupkg -source ~/.nuget/packages/ 
```

which should add the artifacts to your specified NuGet `source`

```
~/.nuget/packages/
├── improbable.standardlibrarycsharp
│   └── 12.2.0
├── improbable.workersdkcsharp
│   └── 12.2.0
└── ...
```

## (OPTIONAL) First Time Setup of Local NuGet Cache Source
```
nuget source add -name local -source ~/.nuget/packages
```

## (OPTIONAL) Recreate NuGet Spec Files
```
(cd standard-library && nuget spec Improbable.StandardLibraryCsharp.dll)
(cd worker-sdk && nuget spec Improbable.WorkerSdkCsharp.dll)
```

The newly generated `.nuspec` files must be modified heavily. 