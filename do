#!/bin/sh
set -e
set -o posix

usage() {
	echo "$(basename $0) <command> [OPTIONS]"
	echo ""
	echo "Variables"
	echo "   INVENTORY_FILE - env variable pointing to ansible inventory file"
	echo "Commands"
	echo "    -h/help                  - This help message"
	echo "    swarm_up                 - bring up the swarm"
	echo "    swarm_down               - take down the swarm"
	echo "    apply_roles              - apply all roles to all hosts"
	echo "    apply_base_roles         - apply base roles (base,docker) to all hosts"
	echo "    apply_proj_roles         - apply project roles (fabric,project) to all hosts"
	echo "    run_stack                - start the SpatialOS stack"
	echo "    stop_stack               - stop the SpatialOS stack"
	echo "    proxy_prometheus         - proxy prometheus port 9090"
	echo "    cmd <role|all> <command> - Run an ad-hoc command on hosts"
	echo "OPTIONS will be passed through to ansible-playbook or ansible"
	exit 0
}

if [ "$1" = "help" ] || [ "$1" = "-h" ] || [ $# -eq 0 ]; then
	usage
fi

if [ -z ${INVENTORY_FILE} ]; then
	echo "INVENTORY_FILE not set"
	exit 1
fi

if ! command -v ansible-playbook 2>&1 >/dev/null; then
	echo "ansible-playbook not in \$PATH"
	exit 1
fi

run_playbook() {
	local playbook=playbooks/${1}
	set -x
	ansible-playbook -i ${INVENTORY_FILE} ${playbook} ${@:2}
	set +x
}

do_swarm_up() {
	run_playbook swarm_up.yml ${@}
}

do_swarm_down() {
	run_playbook swarm_down.yml ${@}
}

do_apply_roles() {
	run_playbook apply_roles.yml ${@}
}

do_apply_base_roles() {
	run_playbook apply_base_roles.yml ${@}
}

do_apply_proj_roles() {
	run_playbook apply_proj_roles.yml ${@}
}

do_run_stack() {
	run_playbook run_stack.yml ${@}
}

do_stop_stack() {
	run_playbook stop_stack.yml ${@}
}

do_proxy_prometheus() {
	local host="$1"
	set -x
	ssh -N -L 9092:localhost:9090 $host &
	set +x
}

do_adhoc_command() {
	local host_pattern="$1"
	local cmd="$2"
	set -x
	ansible ${host_pattern} -i ${INVENTORY_FILE} -a "${cmd}" ${@:3}
	set +x
}

case "$1" in
	swarm_up)
		do_swarm_up ${@:2}
	;;
	swarm_down)
		do_swarm_down ${@:2}
	;;
	apply_roles)
		do_apply_roles ${@:2}
	;;
	apply_base_roles)
		do_apply_base_roles ${@:2}
	;;
	apply_proj_roles)
		do_apply_proj_roles ${@:2}
	;;
	run_stack)
		do_run_stack ${@:2}
	;;
	stop_stack)
		do_stop_stack ${@:2}
	;;
	proxy_prometheus)
		if [ -z "$2" ]; then
			echo "CMD MISSING HOST" >&2
			echo
			usage
		fi
		do_proxy_prometheus $2
	;;
	cmd)
		if [ -z "$2" ] || [ -z "$3" ]; then
			echo "CMD MISSING PARAMETERS" >&2
			echo
			usage
		fi
		do_adhoc_command "$2" "$3" ${@:4}
	;;
esac
		