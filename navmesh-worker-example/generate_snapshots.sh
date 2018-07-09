#!/bin/sh
set -e
DIR="`dirname \"$0\"`"
DIR="`( cd \"$DIR\" && pwd )`"
MONO_ARGS=""

if [ $# -lt 1 ]; then
    set ""
fi

RELEASE="ReleaseWindows"
if [ "$(uname)" == "Darwin" ]; then
    RELEASE="ReleaseMacOS"
    MONO_ARGS+="--arch=64"
elif [ "$(expr substr $(uname -s) 1 5)" == "Linux" ]; then
    RELEASE="ReleaseLinux"
fi

SNAPSHOT_CMD=""
if [ "$RELEASE" != "ReleaseWindows" ]; then
    SNAPSHOT_CMD="mono ${MONO_ARGS} "
fi

SNAPSHOT_CMD+="$DIR/common/Snapshots/bin/x64/$RELEASE/snapshot.exe"
SNAPSHOT_DIR="$DIR/snapshots"

spatial codegen

MSBUILD_CMD="msbuild"
if [ "$RELEASE" == "ReleaseWindows" ]; then
    MSBUILD_CMD+=".exe"
fi

MSBUILD_CMD+=" $DIR/common/Snapshots/Snapshots.csproj /property:Configuration=$RELEASE /property:Platform=x64 /verbosity:quiet"

set -x
${MSBUILD_CMD}
${SNAPSHOT_CMD} $SNAPSHOT_DIR/default.snapshot
