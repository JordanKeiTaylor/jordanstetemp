# spatial local cluster deployment
## Setup
[ansible](https://www.ansible.com/) is used to configure machines for running `spatial local cluster`. To get started,

```
pip install ansible
```

### Launching a cluster (Google Cloud)
There is a convenience script to create a cluster in google cloud from an instance template. You'll need the `gcloud` command to be installed and configured to use the `disappointing-tode` project.

For instance, to launch a deployment of 4 nodes using the `ff-hexadeca-rhel7` template:
```
./gcloud_cluster_create.sh <your deployment name> ff-hexadeca-rhel7 4
```

### Gathering files
First you need to grab some files from your SpatialOS project, a `fabric` bundle zip and prometheus rules:

```
./prepare.sh <spatial_project_dir> <path_to_fabric_bundle_zip> <prometheus_rules_repo>
```
This will tar/copy files into `roles/project/files` and `roles/fabric/files`.

NOTE: Changes to the code, snapshots or configuration files will necessitate rerunning this script.

### Configuring the machines
To configure one or more machines to run `spatial local cluster`
```
ansible-playbook prepare.yml -i <inventory_file>
```
This should be idempotent so can be rerun if needed.

NOTE: There is an example inventory file under `inventories/gce.yml`. You'll need to change the IP addresses.

### Start a `docker swarm`
To sart a `docker swarm` on a set of hosts:

```
ansible-playbook swarm.yml -i <inventory_file>
```
**NOTE: Rerunning this will probably not work.** You'll need to run `docker swarm leave` on the relevant hosts.

### Launching `fabric`
```
ansible-playbook run.yml -i <inventory_file>
```
**NOTE: Rerunning this will probably not work.** You'll need to `docker stack rm fabric` first.

### Proxying Prometheus
There is a utility script to create an SSH tunnel to the Prometheus port (9090) so that the prometheus UI/API can be accessed locally. To run this;

```
./proxy.sh <master_ip>
```

This backgrounds the `ssh` process so you will need to kill it if you want to proxy to a different IP (e.g. if you spin up a new cluster).

## Handy commands
### Stopping the stack

```
docker stack rm fabric
```

### Listing running processes

```
docker stack ps fabric
```

and without truncated errors:

```
docker stack ps fabric --no-trunc
```

### Viewing logs

```
docker service logs -f fabric_toolbelt
```

(or any of the other services: `fabric_fabric-master`, `fabric_prometheus` etc.)