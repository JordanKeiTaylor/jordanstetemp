#!/bin/sh
set -eu
DIR="`dirname \"$0\"`"
DIR="`( cd \"$DIR\" && pwd )`"
MONO_ARGS=""

if [ $# -lt 1 ]; then
    set ""
fi

RELEASE="DebugWindows"
if [ "$(uname)" == "Darwin" ]; then
    RELEASE="DebugMacOS"
    MONO_ARGS+="--arch=64"
elif [ "$(expr substr $(uname -s) 1 5)" == "Linux" ]; then
    RELEASE="DebugLinux"
fi

# Build so that we can run against test dlls
if [ "$1" != "nobuild" ]; then
    echo ">> Building Test code"
    msbuild "$DIR/../STESDK.Tests/STESDK.Tests.csproj" /p:Configuration=$RELEASE /p:Platform=x64 /verbosity:quiet
fi

echo $RELEASE

NUNIT_CMD="mono ${MONO_ARGS} $DIR/../packages/NUnit.ConsoleRunner.3.7.0/tools/nunit3-console.exe"
TESTS="$DIR/../build/Tests/bin/**/$RELEASE/*Tests.dll"

echo ">> Running Unit Tests found in $TESTS"
$NUNIT_CMD $TESTS
