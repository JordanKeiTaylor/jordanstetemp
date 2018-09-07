#!/bin/sh
set -eux
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

PROJECT_DIR=$(realpath $1)
LAUNCH_CONFIG=$(realpath $2)
FABRIC_BUNDLE_DIR="$DIR/fabric_bundles"

if [ -z "$PLATFORM_REPO_DIR" ];
then
    echo "variable $PLATFORM_REPO_DIR not set"
fi

# Download fabric bundle
FABRIC_VERSION="11-20180801T143038Z-da5fae1"
mkdir -p "$FABRIC_BUNDLE_DIR"
if [ ! -f "$FABRIC_BUNDLE_DIR/$FABRIC_VERSION" ];
then
    echo "Downloading fabric: $FABRIC_VERSION"
    pushd "$FABRIC_BUNDLE_DIR"
    bundle_client get $FABRIC_VERSION -d
    popd
fi

FABRIC_ASSEMBLY_BUNDLE_DIR="$PROJECT_DIR/build/assembly/fabric"
mkdir -p "$FABRIC_ASSEMBLY_BUNDLE_DIR"
cp "$FABRIC_BUNDLE_DIR/$FABRIC_VERSION" "$FABRIC_ASSEMBLY_BUNDLE_DIR/fabric_bundle.zip"

GSIM_DIR="$PROJECT_DIR/build/assembly/gsim"
if [ ! -f "$GSIM_DIR/FAKE" ];
then
    mkdir -p "$GSIM_DIR"
    touch "$GSIM_DIR/FAKE"
fi

SKRULL_CLIENT_PATH="$DIR/../skrull_client"
pushd "$SKRULL_CLIENT_PATH"
dotnet build --configuration Release csharp.sln 
popd

pushd $PLATFORM_REPO_DIR
source .envrc
popd

go run "$PLATFORM_REPO_DIR/go/src/improbable.io/cmd/skrull-client/main.go" assembly create  -s "$PROJECT_DIR"
RESULT=$(dotnet "$SKRULL_CLIENT_PATH/bin/Release/netcoreapp2.0/SkrullClient.dll" snapshot-upload -s "$PROJECT_DIR/snapshots/default.snapshot" -d test -j test)
echo "$RESULT"
SNAPSHOT_ID=$(echo "$RESULT" | awk -F':' '{print $2}' | tr -d '[:space:]')

dotnet "$SKRULL_CLIENT_PATH/bin/Release/netcoreapp2.0/SkrullClient.dll" deployment-create -c "$LAUNCH_CONFIG" -a test -d test -j test -s "$SNAPSHOT_ID"
