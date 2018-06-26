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
./gradlew check
cd ..

tc_progress "building ste-sdk"
cd ste-sdk
msbuild
cd ..

tc_progress "checking deploy"
cd deploy
./check.sh
cd ..
