#!/bin/sh

cd recast-wrapper
set path=%%cd%%/cmake-3.12.0-rc1-win64-x64/bin;%%path%%
echo "PATH is: %%path%%"
echo

echo "Fetching cmake 3.12"
wget -q https://cmake.org/files/v3.12/cmake-3.12.0-rc1-win64-x64.zip
unzip -q cmake-3.12.0-rc1-win64-x64.zip
echo

echo "CMake version"
cmake --version
echo

echo "Installing cmake, gcc, make"
pacman -S --noconfirm cmake gcc make
echo

./gradlew :recast-wrapper:build
