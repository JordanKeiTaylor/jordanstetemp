#!/bin/sh

cd recast-wrapper
export PATH="$PWD/cmake-3.12.0-rc1-win64-x64/bin;$PWD/vswhere;$PATH"
echo "PATH is: $PATH"
echo

echo "Fetching cmake 3.12"
wget -q https://cmake.org/files/v3.12/cmake-3.12.0-rc1-win64-x64.zip
unzip -q cmake-3.12.0-rc1-win64-x64.zip
echo

echo "Fetching vswhere"
mkdir vswhere
cd vswhere
wget -q https://github.com/Microsoft/vswhere/releases/download/2.5.2/vswhere.exe
echo
cd ..

vswhere.exe -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
which msbuild

cmake --version
echo

echo "Installing gcc, make"
pacman -S --noconfirm gcc make
echo

./gradlew :recast-csharp:build
