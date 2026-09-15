#!/usr/bin/env bash
# run.sh <app> — read one running application's accessibility surfaces into <app>.json beside this script.
#
# Apps: illustrator, photoshop, indesign, acrobat, creative-cloud, typeface.
# Reads only: menu bar tree, window element tree, window-server geometry. Nothing is clicked or set.
# The application must already be running; a missing process is recorded, never launched.

set -uo pipefail

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
slug="${1:?usage: run.sh <illustrator|photoshop|indesign|acrobat|creative-cloud|typeface>}"

case "$slug" in
  illustrator)   process="Adobe Illustrator" ;;
  photoshop)     process="Adobe Photoshop 2026" ;;
  indesign)      process="Adobe InDesign 2026 (Beta)" ;;
  acrobat)       process="AdobeAcrobat" ;;
  creative-cloud) process="Creative Cloud" ;;
  typeface)      process="Typeface-beta" ;;
  *) echo "unknown app: $slug" >&2; exit 2 ;;
esac

out="$here/$slug.json"
pid=$(osascript -e "tell application \"System Events\" to get unix id of process \"$process\"" 2>/dev/null)
if [ -z "$pid" ]; then
  jq -n --arg process "$process" '{ok: false, process: $process, error: "process not running"}' > "$out"
  echo "$slug: not running -> $out"
  exit 0
fi

cpu=$(ps -o %cpu= -p "$pid" | tr -d ' ')
jq -n \
  --arg process "$process" --argjson pid "$pid" --argjson cpu "${cpu:-0}" \
  --arg at "$(date -u +%Y-%m-%dT%H:%M:%SZ)" \
  '{status: {ok: true, process: $process, pid: $pid, cpuPercentAtStart: $cpu, capturedAt: $at},
    menus: input, tree: input, windowlist: input}' \
  <("$here/ax-dump" menus "$pid") \
  <("$here/ax-dump" tree "$pid") \
  <("$here/ax-dump" windowlist "$pid") > "$out" \
  && echo "$slug: pid $pid, cpu ${cpu}% -> $out"
