# Worker SDK NuGet Artifacts

This repository builds the standard library schema classes and packages them, as well as packaging the Worker SDK separately. 

## Build `Improbable.StandardLibraryCsharp.csproj`
```
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

## (OPTIONAL) Rebuild NuGet Spec Files
```

```