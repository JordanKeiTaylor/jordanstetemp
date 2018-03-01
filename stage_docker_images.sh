#!/bin/sh
set -ex
docker pull registry

docker save \
    registry \
    fnanny:273c0f1e56 \
    toolbelt:273c0f1e56 \
    prometheus:273c0f1e56-prometheus.e9cd0b5 \
    -o roles/docker_registry/files/repository.tar
