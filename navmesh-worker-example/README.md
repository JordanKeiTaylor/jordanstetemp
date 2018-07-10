# Enterprise starter project
Contains a barebones snapshot generator and example worker layout.

## Publish the STE SDK
```
cd ../ste-sdk
STE_PUBLISH_DIR=../publish ./gradlew nugetPush
```

## Nuget restore
Within the `navmesh-worker-example` directory:
```
export STE_SDK_VERSION=`cat ../ste-sdk/VERSION`
nuget restore -configFile nuget.config -PackagesDirectory packages
```

## Build
Within the `navmesh-worker-example` directory:
```
export STE_SDK_VERSION=`cat ../ste-sdk/VERSION`

# build all workers
spatial build

# build and run snapshot generator
./generate_snapshots.sh
```

## Run
```
spatial local start
```
