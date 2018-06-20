# STESDK

## Build
```
msbuild /p:Configuration=Release
```

## Publish
```
nuget pack STESDK.nuspec -Prop Configuration=Release
nuget add Improbable.STESDK.*.nupkg -source ~/.nuget/packages/
```