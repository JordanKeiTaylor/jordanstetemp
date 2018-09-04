# spatial local cluster deployment
## Setup
[ansible](https://www.ansible.com/) is used to configure machines for running `spatial local cluster`. To get started,

```
pip install ansible
```

### Launching a cluster (Google Cloud)
There is a convenience script to create a cluster in google cloud from an instance template. You'll need the `gcloud` command to be installed and configured to use the `disappointing-tode` project.

For instance, to launch a deployment of 4 nodes using the `ff-double-rhel7` template:
```
./gcloud_cluster_create.sh <your deployment name> ff-double-rhel7 4
```

### Gathering files
First you need to grab the docker images as well as the prometheus rules.

Update `stage_dependencies.sh` with the latest tags from `docker images`.

You will also need to update `vars.yml` with these same tags.

```
./stage_dependencies.sh <prometheus_rules_repo>
```

This will tar/copy files into `roles/project/files` and `roles/fabric/files`. 
NOTE: Changes to the code, snapshots or configuration files will necessitate rerunning this script.

### Configuring the machines
To configure one or more machines to run `spatial local cluster`, first point `ansible` at your inventory file, e.g.
```
export INVENTORY_FILE=<inventory_file>
```

NOTE: There is an example inventory file under `inventories/gce.yml.tmpl`. First copy it to a new file: `inventories/gce.yml` You'll need to change the IP addresses only.

Ensure your public SSH key is added to `ssh_keys.yml` so that the scripts can connect to the VMs in later steps.

Then bootstrap the machines to create/setup the `demo` user on your VMs.
```
./do bootstrap <remote_username>
```

NOTE-1: `remote_username` should be an existing user on the remote host with `sudo` or `root` privileges. 
This is typically your GCP username. e.g. `jamesbown`

NOTE-2: You may need to add your SSH key to GCP prior to executing this so that the 
script can connect directly to the host using your key.

Once the machines have the `demo` user we can now set them up with:
```
./do apply_base_roles
```

This should be idempotent so can be rerun if needed.

### Start a `docker swarm`
To start a `docker swarm` on a set of hosts:

```
./do swarm_up
```

### Start Skrull
```
./do run_stack
```

### Create a proxy
First set up a local proxy to `skrull`. This allows you to connect to Skrull's inspector using `localhost:21002` 
and Prometheus on `localhost:9002`. This is achieved by creating an SSH tunnel. Note that
this process does not terminate automatically and the tunnel exists for as long as this process is running.

```
./do proxy <ip_of_master_node>
```

### Create and start deployment

Deploy your SpatialOS project:
```
PLATFORM_REPO_DIR=<path_to_platform_repo> ./yolo.sh <spatial_project_dir> <spatial_legacy_config_path>
```

### Connecting to the deployment
- Inspector: [http://localhost:22002](http://localhost:22002)
- Prometheus: [http://localhost:9002](http://localhost:9002)

### Stopping a deployment
```
./do stop_stack
```

### Stopping the `docker swarm`
```
./do swarm_down
```

### Stopping your VMs
Don't forget to stop your VMs in [GCP](https://console.cloud.google.com/compute/instanceGroups/list?project=disappointing-tode).
You can click on your instance-group name and then stop them.

## Handy commands
### Stopping the Docker Swarm

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
