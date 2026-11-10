#!/usr/bin/env bash
set -euo pipefail

# --- [COMPOSITION] ----------------------------------------------------------------------

export DEBIAN_FRONTEND=noninteractive
distribution=$(lsb_release --id --short)
codename=$(lsb_release --codename --short)
install -d -m 0755 /etc/apt/keyrings
curl -fsSL "https://download.docker.com/linux/${distribution,,}/gpg" -o /etc/apt/keyrings/docker.asc
printf 'Types: deb\nURIs: https://download.docker.com/linux/%s\nSuites: %s\nComponents: stable test\nSigned-By: /etc/apt/keyrings/docker.asc\n' "${distribution,,}" "${codename}" >/etc/apt/sources.list.d/docker.sources
curl -fsSL "https://pkgs.tailscale.com/stable/${distribution,,}/${codename}.noarmor.gpg" -o /usr/share/keyrings/tailscale-archive-keyring.gpg
curl -fsSL "https://pkgs.tailscale.com/stable/${distribution,,}/${codename}.tailscale-keyring.list" -o /etc/apt/sources.list.d/tailscale.list
apt-get update
apt-get install --yes "$@"
