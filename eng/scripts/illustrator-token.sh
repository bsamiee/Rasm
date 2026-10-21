#!/bin/sh
set -eu

# --- [COMPOSITION] ----------------------------------------------------------------------
settings=$(find "$HOME/Library/Preferences" -maxdepth 1 -name 'Adobe Illustrator *Settings' | sort -V | tail -n 1)
rg -o '/AuthToken \[ [0-9]+[[:space:]]+([0-9a-f[:space:]]+)\]' -r '$1' "$settings/en_US/Adobe Illustrator Prefs" | tr -d ' \t\n' | xxd -r -p | jq -R '{Authorization: "Bearer \(.)"}'
