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
./stage_dependencies.sh <prometheus_rules_repo>
```

This will tar/copy files into `roles/project/files` and `roles/fabric/files`. NOTE: Changes to the code, snapshots or configuration files will necessitate rerunning this script.

### Configuring the machines
To configure one or more machines to run `spatial local cluster`, first point `ansible` at your inventory file, e.g.
```
export INVENTORY_FILE=<inventory_file>
```

NOTE: There is an example inventory file under `inventories/gce.yml.tmpl`. First copy it to a new file: `cp inventories/gce.yml{.tmpl,}` You'll need to change the IP addresses.

Then bootstrap the machines to create/setup the demo user:
```
./do bootstrap <remote_username>
```

NOTE: `remote_username` should be an existing user on the remote host with `sudo` or `root` privileges.

Then setup the machines with:
```
./do apply_base_roles
```

This should be idempotent so can be rerun if needed.

### Start a `docker swarm`
To start a `docker swarm` on a set of hosts:

```
./do swarm_up
```

### Launching deployment
```
./do run_stack
```

### Launching a deployment
First set up a proxy to `skrull`

```
./do proxy
```

Then deploy your SpatialOS project:
```
PLATFORM_REPO_DIR=<path_to_platform_repo> ./yolo.sh  <spatial_project_dir> <spatial_legacy_config_path>
```

### Stopping deployment
```
./do stop_stack
```

### Proxying stuff
```
./do proxy
```

### Proxying Prometheus
There is a utility script to create an SSH tunnel to the Prometheus port (9090) so that the prometheus UI/API can be accessed locally. To run this;

```
./do proxy_prometheus demo@<slave1_ip>
```
and open http://localhost:9092/

### Proxying Inspector
There is a utility script to create an SSH tunnel to the Inspector port (21000) so that the inpsector UI/API can be accessed locally. To run this;

```
./do proxy_inspector demo@<slave1_ip>
````

and open http://localhost:21002/inspector

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
