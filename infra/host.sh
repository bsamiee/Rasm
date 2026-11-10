#!/usr/bin/env bash
set -euo pipefail

# --- [COMPOSITION] ----------------------------------------------------------------------

export DEBIAN_FRONTEND=noninteractive
distribution=$(lsb_release --id --short)
codename=$(lsb_release --codename --short)
install -d -m 0755 /etc/apt/keyrings /etc/docker /etc/systemd/journald.conf.d
curl -fsSL "https://download.docker.com/linux/${distribution,,}/gpg" -o /etc/apt/keyrings/docker.asc
printf 'Types: deb\nURIs: https://download.docker.com/linux/%s\nSuites: %s\nComponents: stable test\nSigned-By: /etc/apt/keyrings/docker.asc\n' "${distribution,,}" "${codename}" >/etc/apt/sources.list.d/docker.sources
printf '{"live-restore": true, "log-driver": "local"}\n' >/etc/docker/daemon.json
printf 'vm.overcommit_memory = 1\nnet.ipv4.tcp_congestion_control = bbr\n' >/etc/sysctl.d/90-platform.conf
printf '[zram0]\nzram-size = ram / 4\ncompression-algorithm = zstd\n' >/etc/systemd/zram-generator.conf
printf 'PasswordAuthentication no\nAcceptEnv *\n' >/etc/ssh/sshd_config.d/00-platform.conf
printf '[Journal]\nSystemMaxUse=1G\n' >/etc/systemd/journald.conf.d/90-platform.conf
apt-get update
apt-get install --yes docker-ce docker-buildx-plugin docker-compose-plugin systemd-zram-generator
sysctl --system
systemctl daemon-reload
systemctl restart systemd-journald docker
systemctl start dev-zram0.swap
systemctl reload ssh
