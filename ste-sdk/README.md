# STESDK

## Build
```
msbuild /p:Configuration=Release
```

## Publish
```
nuget pack STESDK/STESDK.csproj -Prop Configuration=Release
nuget add Improbable.Enterprise.STESDK.*.nupkg -source ~/.nuget/packages/
```