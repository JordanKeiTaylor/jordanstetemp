#!/bin/sh
set -eux

tc_progress() {
    echo "##teamcity[progressMessage '$1']"
}

tc_progress "building OBJ-Tools"
cd OBJ-Tools
./gradlew check
cd ..

tc_progress "building minisseur"
cd minisseur
./gradlew check
cd ..
