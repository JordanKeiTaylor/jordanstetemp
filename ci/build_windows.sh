#!/bin/sh

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

./gradlew :recast-csharp:check
