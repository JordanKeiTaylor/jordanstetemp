#!/bin/sh
set -ex

NAME=$1
TEMPLATE=$2
SIZE=${3:-4}

gcloud compute instance-groups managed create $NAME \
--size $SIZE \
--template $TEMPLATE \
--zone europe-west1-b
