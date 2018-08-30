#!/usr/bin/env bash
set -eux
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

PROMETHEUS_RULES_DIR="$1"
DOCKER_REPO_PATH="$SCRIPT_DIR/roles/docker_registry/files/repository.tar"
PROMETHEUS_RULES_OUTPUT_PATH="$SCRIPT_DIR/roles/fabric/files/prometheus_rules.tar.gz"

SKRULL_TAG=5c118e12fc.dirty
PROMETHEUS_TAG=11fe61decd
FNANNY_TAG=143d01256c.dirty

TAR_GIT_IGNORES="--exclude .git \
    --exclude '.gitignore' \
    --exclude '.gitkeep'" \

function stage_docker_images() {
    mkdir -p roles/fabric/files
    docker save registry:latest node-exporter:latest fnanny:$FNANNY_TAG skrull:$SKRULL_TAG prometheus:$PROMETHEUS_TAG -o $DOCKER_REPO_PATH
}

function stage_prometheus_rules() {
    tar cvz \
        --exclude .git \
        --exclude '*.m4' \
        --exclude '*.sh' \
        ${TAR_GIT_IGNORES} \
        -C "$PROMETHEUS_RULES_DIR" \
        -f  "$PROMETHEUS_RULES_OUTPUT_PATH" \
        .
}

function stage_local_inspector() {
    tar cvz \
        -C ~ \
        ${TAR_GIT_IGNORES} \
        -f roles/fabric/files/local_inspector.tar.gz \
        .improbable/local_inspector/
}

stage_docker_images
stage_prometheus_rules
stage_local_inspector