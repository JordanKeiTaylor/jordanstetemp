#!/bin/sh
set -ex
docker pull registry
docker pull quay.io/prometheus/node-exporter
docker tag quay.io/prometheus/node-exporter node-exporter

docker save \
    registry \
    node-exporter \
    fnanny:17b8242f39 \
    toolbelt:17b8242f39 \
    prometheus:17b8242f39-prometheus.e9cd0b5 \
    -o roles/docker_registry/files/repository.tar
