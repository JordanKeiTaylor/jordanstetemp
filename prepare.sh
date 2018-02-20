#!/bin/sh
set -e

PROJECT_DIR=$1
FABRIC_BUNDLE="$2"
PROMETHEUS_RULES_DIR="$3"

if [ -z "$PROJECT_DIR" ] || [ -z "${FABRIC_BUNDLE}" ] || [ -z "$PROMETHEUS_RULES_DIR" ] ; then
    echo "Invalid usage: prepare.sh <spatial_os_project_path> <fabric_bundle_path>"
    exit 1
fi

set -x
mkdir -p roles/fabric/files
mkdir -p roles/project/files
tar cvz -C ~ -f roles/fabric/files/local_inspector.tar.gz .improbable/local_inspector/
tar cvz -C ~ -f roles/fabric/files/bundle_resolution_cache.tar.gz .improbable/cache/bundle_resolution_cache
tar cvz --exclude .git -C "$PROMETHEUS_RULES_DIR" -f roles/fabric/files/prometheus_rules.tar.gz .
tar cvz -C $PROJECT_DIR --exclude="*.csv" -f roles/project/files/project.tar.gz snapshots/ spatialos.json build/assembly

cp $FABRIC_BUNDLE roles/project/files
