# STESDK

## Build
```
./recast-wrapper/gradlew build -p recast-wrapper
msbuild /p:Configuration=Release
```

## Publish
```
nuget pack stesdk/STESDK.csproj -Prop Configuration=Release
nuget add Improbable.STESDK.*.nupkg -source ~/.nuget/packages/
```