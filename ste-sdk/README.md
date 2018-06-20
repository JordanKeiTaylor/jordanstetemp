# STESDK

## Build
```
msbuild /p:Configuration=Release
```

## Publish
```
nuget pack Improbable.STESDK.dll.nuspec
nuget add Improbable.STESDK.*.nupkg -source ~/.nuget/packages/
```