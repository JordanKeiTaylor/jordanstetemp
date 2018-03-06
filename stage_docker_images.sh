#!/bin/sh
set -ex
docker pull registry
docker pull quay.io/prometheus/node-exporter
docker tag quay.io/prometheus/node-exporter node-exporter

PLATFORM_VERSION=273c0f1e56
docker save \
    registry \
    node-exporter \
    fnanny:$PLATFORM_VERSION \
    toolbelt:$PLATFORM_VERSION \
    prometheus:$PLATFORM_VERSION-prometheus.e9cd0b5 \
    -o roles/docker_registry/files/repository.tar
