# STESDK

## Build
```
./recast-wrapper/gradlew build -p recast-wrapper
msbuild /p:Configuration=Release
```

## NuGet Configuration
If you have not done so, you must create a NuGet `source` for the local NuGet cache. Otherwise, specify a different source than `~/.nuget/packages/`
```
nuget source add -name local -source ~/.nuget/packages/
```

## Publish
```
nuget pack stesdk/STESDK.csproj -Prop Configuration=Release
nuget add Improbable.STESDK.*.nupkg -source ~/.nuget/packages/
```
Or you can use the release script, which packages and publishes the project to the local NuGet source. This removes the currently published package if it exists.
```
./scripts/release_local.sh
```


