#!/bin/sh
set -eux

export INVENTORY_FILE=inventories/demo.yml

./do bootstrap root --syntax-check
./do apply_base_roles --syntax-check
./do swarm_up --syntax-check
./do swarm_down --syntax-check
./do run_stack --syntax-check
./do stop_stack --syntax-check
