#!/bin/sh
set -eux

tc_progress() {
    echo "##teamcity[progressMessage '$1']"
}

tc_progress "building obj-tools"
cd obj-tools
./gradlew check
cd ..

tc_progress "building minisseur"
cd minisseur
./gradlew check
cd ..

tc_progress "building recast-wrapper"
cd recast-wrapper
./gradlew build
cd ..

tc_progress "building geographiclib"
cd geographiclib
./gradlew build
cd ..

tc_progress "building ste-sdk"
cd ste-sdk
nuget restore
./gradlew printVersion
./gradlew build
./gradlew nunit
STE_PUBLISH_DIR=../publish ./gradlew nugetPush
export STE_SDK_VERSION=`cat VERSION`
cd ..

tc_progress "building navmesh-worker-example"
cd navmesh-worker-example
nuget restore -configFile nuget.config -PackagesDirectory packages
bash ./generate_snapshots.sh
spatial build
cd ..

tc_progress "building platform-sdk"
cd platform-sdk
./gradlew build
cd ..

tc_progress "checking deploy"
cd deploy
./check.sh
cd ..
