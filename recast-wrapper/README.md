# recast-wrapper
## What?
A wrapper around the [Recast/Detour](https://github.com/recastnavigation/recastnavigation) C++ library to allow basic generation and querying of [navigation meshes](https://en.wikipedia.org/wiki/Navigation_mesh) in Java and C#.

## Why?
The Enterprise STE project needs to interact with navigation meshes to allow entities to route around virtual representations of real world terrain. There are source ports of the library to [Java](https://github.com/ppiastucki/recast4j) and [C#](https://github.com/Robmaister/SharpNav) but we wanted to bind to the original C++ code which is battle tested and actively maintained.

## How?
- The `recast` project clones a pinned reivion of the `recast` github repository and builds the core libraries using `cmake` (disabling the `RecastDemo` project which depends on `SDL`). This in turn is used to compile a thin C++ wrapper library (`recastwrapper`) which exposes a C ABI for convenient binding from Java and C#.
- `recast-java` uses [JNA](https://github.com/java-native-access/jna) to bind to `recastwrapper`.
- `recast-csharp` uses [P/Invoke](https://en.wikipedia.org/wiki/Platform_Invocation_Services) to bind to `recastwrapper`.

## Building
### Mac/Linux

```
./gradlew build
```

### Linux (cross-compiling from Mac)
First make a docker image in which to build the library:

```
docker build -t recast-wrapper-builder .
```

Then build:
```
docker run -v $PWD:/home/project recast-wrapper-builder ./gradlew clean build
```


