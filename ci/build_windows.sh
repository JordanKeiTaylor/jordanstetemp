#!/bin/sh
set -eux

# Preflight checks
export MSBUILD_DIR="C:/Program Files (x86)/Microsoft Visual Studio/2017/BuildTools/MSBuild/15.0/Bin/"
export PATH="$PWD/cmake-3.12.0-rc1-win64-x64/bin:/mingw64/bin:$PATH"



echo "Checking Skrull client build"
cd skrull_client
dotnet build --configuration Release csharp.sln
cd ..



echo "Checking recast-csharp"
cd recast-wrapper
echo "PATH is: $PATH"
echo

echo "Fetching cmake 3.12"
wget -q https://cmake.org/files/v3.12/cmake-3.12.0-rc1-win64-x64.zip
unzip -q cmake-3.12.0-rc1-win64-x64.zip
echo

make --version
gcc --version
which gcc

cmake --version
echo

./gradlew :recast-csharp:check
cd ..
