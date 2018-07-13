# STESDK

## Pre-Build
To build the STESDK, please ensure that the following libraries are built first:

1. Build `recast-wrapper`:
```
cd ../recast-wrapper/
./gradlew build
```

2. Build `geographiclib`:
```
cd ../geographiclib/
./gradlew build
```

## Build
To build the STESDK:
```
./gradlew build
```

## Test
To test the STESDK:

- First `nuget restore` if you haven't already to get the build targets used in the `.csproj` file.

- Build the `recast-wrapper`
```
./recast-wrapper/gradlew build -p recast-wrapper
```

- Run the tests:
```
./gradlew nunit
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

## Creating a Release (Mac)
1. Checkout `master` (or a specific commit if you prefer).

2. Tag `master` with  an appropriate version number `x.y.z`.

```
git tag -a x.y.z
```

3. `git push origin x.y.z` to upload your tag to github

4. Grab the following files from the artifacts in CI:

- `librecastwrapper.so` from the `Build and Test` job. Place this in `ste-sdk/native_libs/linux`.
- `recastwrapper.dll` from the `Windows Build` job. Place this in `ste-sdk/native_libs/windows`

5. Build `recast-wrapper`

```
cd ../recast-wrapper
./gradlew clean :recast-csharp:build
cd ../ste-sdk
```

6. Build the `nuget` package

```
./gradlew clean nugetPack
```

7. Check under `build/distributions` for the `.nupkg` file. Check the version number makes sense. Use `unzip -l` to check that it has `recastwrapper` native code for all 3 platforms.

8. Add the `.nupkg` file to the ste-artifacts repo, `commit` and `push`.

9. Go to the `Releases` page on Github for the `ste` repository. You should find the newly created release there. Click `Edit`. Attach the `.nupkg` file to the release here.

10. Let people in `#combined_ste_dev` know that you've made the release ideally with a link to Github.
