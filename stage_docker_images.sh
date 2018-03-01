#!/bin/sh
set -ex
docker pull registry
docker pull quay.io/prometheus/node-exporter
docker tag quay.io/prometheus/node-exporter node-exporter

docker save \
    registry \
    node-exporter \
    fnanny:273c0f1e56 \
    toolbelt:273c0f1e56 \
    prometheus:273c0f1e56-prometheus.e9cd0b5 \
    -o roles/docker_registry/files/repository.tar
