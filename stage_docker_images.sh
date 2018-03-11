#!/usr/bin/env bash

REPO_PATH=roles/docker_registry/files/repository.tar
PLATFORM_REV=17b8242f39
PROMETHEUS_REV=e9cd0b5
set -e

check_image() {
	if ! jq -e ".\"${1}\".\"${2}\"" ${3} >/dev/null; then
		echo "Bad ${1} version"
		exit 1
	fi
}
check_repository() {
	local repo_path="${1}"
	local repo_extract=$(mktemp -d)
	tar -xf ${repo_path} -C ${repo_extract} repositories
	echo "Testing existing repo at ${repo_path}"
	jq . ${repo_extract}/repositories

	check_image fnanny "${PLATFORM_REV}" ${repo_extract}/repositories
	check_image toolbelt "${PLATFORM_REV}" ${repo_extract}/repositories
	check_image prometheus "${PLATFORM_REV}-prometheus.${PROMETHEUS_REV}" ${repo_extract}/repositories
	check_image "node-exporter" "latest" ${repo_extract}/repositories
	check_image "registry" "latest" ${repo_extract}/repositories
	return 0
}

download_repository() {
	echo "Downloading repository"
	local cookie_file=$(mktemp)
	local repo_file_id=1Gd5WQEOpJDSBU5rSokUPzZnGnK9ZuCS2
	local out_file=$1
	local confirm=$(wget --quiet --save-cookies $cookie_file \
					--keep-session-cookies \
					--no-check-certificate "https://docs.google.com/uc?export=download&id=$repo_file_id" \
					-O- \
					  | sed -rn 's/.*confirm=([0-9A-Za-z_]+).*/\1\n/p')
	wget --load-cookies $cookie_file \
		"https://docs.google.com/uc?export=download&confirm=$confirm&id=$repo_file_id" \
		-O $out_file
}

is_correct_repo() {
	if [ -f ${REPO_PATH} ] && check_repository ${REPO_PATH}; then
		return 0
	fi
	return 1
}

test_and_fetch_repo() {
	if is_correct_repo; then
		echo "Existing repo appears to be correct: ${REPO_PATH}"
		exit 0
	else
		echo "No existing repo found at ${REPO_PATH}"
		download_repository ${REPO_PATH}
		if is_correct_repo; then
			echo "Successfully downloaded repo to ${REPO_PATH}"
			exit 0
		else 
			echo "Downloaded repo appears malformed. Please check!"
			exit 1
		fi
	fi
}


set -x

if [ "$1" = "--fetch" ]; then
	echo "Testing and fetching docker repository"
	test_and_fetch_repo
else
	docker pull registry
	docker pull quay.io/prometheus/node-exporter
	docker tag quay.io/prometheus/node-exporter node-exporter

	docker save \
	    registry:latest \
	    node-exporter:latest \
	    fnanny:${PLATFORM_REV} \
	    toolbelt:${PLATFORM_REV} \
	    prometheus:${PLATFORM_REV}-prometheus.${PROMETHEUS_REV} \
	    -o "${REPO_PATH}"
fi
