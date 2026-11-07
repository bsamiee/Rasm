#!/usr/bin/env bash
set -euo pipefail

# --- [COMPOSITION] ----------------------------------------------------------------------

executable=$(ps -o comm= -p "$(pgrep -x 'Adobe Illustrator')")
version=$(defaults read "${executable%/Contents/MacOS/*}/Contents/Info" CFBundleShortVersionString)
prefs=$(jq -r --arg version "$version" 'select(has($version)) | input_filename | sub("\\.runConfig$"; "Adobe Illustrator Prefs")' "$HOME"/Library/Preferences/Adobe\ Illustrator*Settings/*/.runConfig)
rg -o '/AuthToken \[ [0-9]+[[:space:]]+([0-9a-f[:space:]]+)\]' -r '$1' "$prefs" | tr -d ' \t\n' | xxd -r -p | jq -R '{Authorization: "Bearer \(.)"}'
