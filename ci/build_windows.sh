#!/bin/sh
set -eux

# Preflight checks
pacman -Ss mingw64/mingw-w64-x86_64-gcc
make --version
gcc --version
g++ --version
mono --version

export MSBUILD_DIR="C:/Program Files (x86)/Microsoft Visual Studio/2017/BuildTools/MSBuild/15.0/Bin/"
export PATH="$PWD/cmake-3.12.0-rc1-win64-x64/bin;$PATH"

cd recast-wrapper
echo "PATH is: $PATH"
echo

echo "Fetching cmake 3.12"
wget -q https://cmake.org/files/v3.12/cmake-3.12.0-rc1-win64-x64.zip
unzip -q cmake-3.12.0-rc1-win64-x64.zip
echo

cmake --version
echo

./gradlew :recast-csharp:assemble

# Yuck. Can't get tests working from within gradle so this is what we're left with
# Probably something to do with this: https://github.com/nunit/nunit-console/issues/370
# which may be fixed with NUnit.Console 3.9.0
mkdir nunit
cd nunit
wget -q https://github.com/nunit/nunit-console/releases/download/3.8/NUnit.Console-3.8.0.zip
unzip -q NUnit.Console-3.8.0.zip
cd ..
ls ./recast-csharp/build/msbuild/bin/Release/Improbable.Recast.Tests.dll
./nunit/nunit3-console.exe --inprocess --teamcity --trace=Verbose --where "cat != NotOnWindows" --labels=All ./recast-csharp/build/msbuild/bin/Release/Improbable.Recast.Tests.dll