# spatial local cluster deployment
## Setup
[ansible](https://www.ansible.com/) is used to configure machines for running `spatial local cluster`. To get started,

```
pip install ansible
```

## Launching a cluster
### Preparing the machines
First you need to grab some files from your SpatialOS project, a `fabric` bundle zip and prometheus rules:

```
./prepare.sh <spatial_project_dir> <path_to_fabric_bundle_zip> <prometheus_rules_repo>
```
This will tar/copy files into `roles/project/files` and `roles/fabric/files`.

To configure one or more machines to run `spatial local cluster`
```
ansible-playbook prepare.yml -i <host_ip>,
```
NOTE: The `,` is significant.

or 
```
ansible-playbook prepare.yml -i <inventory_file>
```
This should be idempotent so can be rerun if needed.

### Launch `docker swarm`
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
