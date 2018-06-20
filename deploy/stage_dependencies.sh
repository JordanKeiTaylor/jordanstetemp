#!/usr/bin/env bash
set -e
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_DIR=$1
PROMETHEUS_RULES_DIR="$2"
PINNED_FABRIC_BUNDLE='for_chris_2'
PINNED_FABRIC_BUNDLE_PATH="${SCRIPT_DIR}/fabric_bundle-${PINNED_FABRIC_BUNDLE}.zip"
FABRIC_BUNDLE_PATH="${3:-${PINNED_FABRIC_BUNDLE_PATH}}"

if [ -z "$PROJECT_DIR" ] || [ -z "$PROMETHEUS_RULES_DIR" ] ; then
    echo "Invalid usage: prepare.sh <spatial_os_project_path> <prometheus_rules_dir> [<fabric_bundle_zip>]"
    exit 1
fi
if [ ! -f "${FABRIC_BUNDLE_PATH}" ]; then
	if [ "${FABRIC_BUNDLE_PATH}" == "${PINNED_FABRIC_BUNDLE_PATH}" ]; then
		echo "No bundle at ${FABRIC_BUNDLE_PATH}, downloading"
		bundle_client get "${PINNED_FABRIC_BUNDLE}" -d \
		  && mv "${PINNED_FABRIC_BUNDLE}" "${PINNED_FABRIC_BUNDLE_PATH}"
	else
		echo "No fabric bundle exists at override path: ${FABRIC_BUNDLE_PATH}"
		exit 1
	fi
fi

TAR_GIT_IGNORES="--exclude .git \
    --exclude '.gitignore' \
    --exclude '.gitkeep'" \

set -x
mkdir -p roles/fabric/files
mkdir -p roles/project/files
tar cvz \
    -C ~ \
    ${TAR_GIT_IGNORES} \
    -f roles/fabric/files/local_inspector.tar.gz \
    .improbable/local_inspector/

tar cvz \
    --exclude .git \
    --exclude '*.m4' \
    --exclude '*.sh' \
    ${TAR_GIT_IGNORES} \
    -C "$PROMETHEUS_RULES_DIR" \
    -f roles/fabric/files/prometheus_rules.tar.gz \
    .

tar cvz \
    -C $PROJECT_DIR \
    --exclude='*.csv' \
    --exclude='*jsonnet*' \
    ${TAR_GIT_IGNORES} \
    -f roles/project/files/project.tar.gz \
    snapshots/ \
    configs/ \
    spatialos.json \
    build/assembly

cp $FABRIC_BUNDLE_PATH roles/project/files/fabric_bundle.zip
