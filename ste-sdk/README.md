# STESDK

## Build
```
msbuild /p:Configuration=Release
```

## Publish
```
nuget pack stesdk/STESDK.csproj -Prop Configuration=Release
nuget add Improbable.STESDK.*.nupkg -source ~/.nuget/packages/
```