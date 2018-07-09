# Enterprise starter project
Contains a barebones snapshot generator and example worker layout.

## Publish the STE SDK
```
cd ../ste-sdk
./gradlew nugetPush
```

## Nuget restore
```
export STE_SDK_VERSION=`cat ../ste-sdk/VERSION`
nuget restore -configFile nuget.config -PackagesDirectory packages
```

## Build
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
