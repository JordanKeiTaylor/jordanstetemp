#!/bin/sh
set -eux

tc_progress() {
    echo "##teamcity[progressMessage '$1']"
}

tc_progress "building OBJ-Tools"
pushd OBJ-Tools
./gradlew check
popd

tc_progress "building minisseur"
pushd minisseur
./gradlew check
popd
