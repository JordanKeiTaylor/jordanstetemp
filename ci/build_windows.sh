#!/bin/sh

ls "C:/Windows/Microsoft.Net/Framework/v4.0.30319/MSBuild/15.0/Bin/msbuild.exe"
export MSBUILD_DIR="C:/Program Files (x86)/Microsoft Visual Studio/2017/BuildTools/MSBuild/15.0/Bin/"

cd recast-wrapper
export PATH="$PWD/cmake-3.12.0-rc1-win64-x64/bin;$PATH"
echo "PATH is: $PATH"
echo

echo "Fetching cmake 3.12"
wget -q https://cmake.org/files/v3.12/cmake-3.12.0-rc1-win64-x64.zip
unzip -q cmake-3.12.0-rc1-win64-x64.zip
echo

cmake --version
echo

echo "Installing gcc, make"
pacman -S --noconfirm gcc make
echo

./gradlew :recast-csharp:build
