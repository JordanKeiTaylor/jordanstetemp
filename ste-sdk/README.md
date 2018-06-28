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
To publish a `nuget` package locally, run:

```
./gradlew nugetPush
```

By default this will package native code from the `recast-wrapper` subproject. If you want to include additional native libraries from other platforms, place them in the following paths:

```
native_libs/darwin/librecastwrapper.dylib # mac
native_libs/linux/librecastwrapper.so # linux
native_libs/windows/recastwrapper.dll # windows
```

These paths will take preference over anything built in the `recast-wrapper` subprtoject.