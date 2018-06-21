#!/bin/sh
set -eux

./do bootstrap root --syntax-check
./do apply_roles --syntax-check
./do swarm_up --syntax-check
./do swarm_down --syntax-check
./do run_stack --syntax-check
./do stop_stack --syntax-check
