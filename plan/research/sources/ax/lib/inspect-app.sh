#!/usr/bin/env bash
# inspect-app — read one application through the accessibility API, read-only.
#
# Sourced by ax/<app>/inspect.sh, which sets:
#   APP_PROCESS   System Events process name
#   APP_SLUG      output directory name
#   APP_DIALOGS   newline-separated "label<TAB>menu<TAB>item[<TAB>item...]" rows
#
# Every step records what it found, including failures, as JSON. A step that
# cannot read a surface writes the AXError rather than omitting the surface.

set -uo pipefail

AX_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BIN="$AX_ROOT/.bin/ax-dump"
OUT="$AX_ROOT/$APP_SLUG/out"
mkdir -p "$OUT"

pid=$(osascript -e "tell application \"System Events\" to get unix id of process \"$APP_PROCESS\"" 2>/dev/null)
if [ -z "$pid" ]; then
  printf '{"ok":false,"process":"%s","error":"process not running"}\n' "$APP_PROCESS" > "$OUT/status.json"
  echo "$APP_SLUG: not running"
  exit 0
fi

cpu=$(ps -o %cpu= -p "$pid" | tr -d ' ')
printf '{"ok":true,"process":"%s","pid":%s,"cpuPercentAtStart":%s,"capturedAt":"%s"}\n' \
  "$APP_PROCESS" "$pid" "${cpu:-0}" "$(date -u +%Y-%m-%dT%H:%M:%SZ)" > "$OUT/status.json"

echo "$APP_SLUG: pid $pid, cpu ${cpu}%"

# Every dump lands through this: a run killed mid-write would otherwise leave a
# truncated file, and an unreadable file is indistinguishable from a missing one.
# A failure is recorded as JSON naming the command, never left empty.
dump() {
  local cmd="$1" dest="$2"
  "$BIN" "$cmd" "$pid" > "$dest.part" 2>"$dest.err"
  if python3 -c "import json,sys; json.load(open(sys.argv[1]))" "$dest.part" 2>/dev/null; then
    mv "$dest.part" "$dest"
  else
    CMD="$cmd" PID="$pid" ERR="$(head -c 400 "$dest.err" 2>/dev/null)" python3 -c '
import json, os, sys
json.dump({"ok": False, "command": os.environ["CMD"], "pid": int(os.environ["PID"]),
           "error": "ax-dump produced no parseable JSON: the application did not answer, "
                    "or the run was interrupted",
           "stderr": os.environ["ERR"] or None}, sys.stdout, indent=2)' > "$dest"
    rm -f "$dest.part"
  fi
  [ -s "$dest.err" ] || rm -f "$dest.err"
}

# (a) menu bar tree, read without opening any menu
dump menus "$OUT/menus.json"

# (b) window element tree
dump tree "$OUT/tree.json"

# window-server geometry, which survives an app too busy to answer AX
dump windowlist "$OUT/windowlist.json"

# the AXManualAccessibility / AXEnhancedUserInterface unlock attempt
dump unlock "$OUT/ax-unlock.json"

# derived layout
python3 "$AX_ROOT/lib/derive-layout.py" "$OUT/tree.json" "$OUT/layout.json" >/dev/null 2>&1 \
  || printf '{"ok":false,"error":"derivation failed: tree had no usable windows"}\n' > "$OUT/layout.json"

# (e) window screenshot, by window-server id so it works across spaces
winid=$(python3 -c "
import json
try:
    ws = json.load(open('$OUT/windowlist.json'))['windows']
    ws = [w for w in ws if w['layer'] == 0 and w['bounds']['w'] > 200]
    print(ws[0]['windowId'] if ws else '')
except Exception:
    print('')
" 2>/dev/null)
if [ -n "$winid" ]; then
  screencapture -x -o -l "$winid" "$OUT/window.png" 2>/dev/null
fi

# (c) dialogs: open via System Events, read via ax-dump, then cancel
: > "$OUT/dialogs.jsonl"
row_to_json() {
  export MSG="$1" OPENED="$2" CLOSED="$3" DUMP="$4" LABEL="$5"
  shift 5
  python3 -c '
import json, os, sys
json.dump({"label": os.environ["LABEL"], "menuPath": sys.argv[1:],
           "opened": os.environ["OPENED"] == "1",
           "window": os.environ["MSG"] if os.environ["OPENED"] == "1" else None,
           "message": None if os.environ["OPENED"] == "1" else os.environ["MSG"],
           "closedBy": os.environ["CLOSED"] or None,
           "paneCount": (int(os.environ["PANE_COUNT"])
                         if os.environ.get("PANE_COUNT", "").isdigit() else None),
           "paneError": (None if os.environ.get("PANE_COUNT", "").isdigit()
                         else (os.environ.get("PANE_COUNT") or None)),
           "axVisible": os.environ.get("DIALOG_AX_VISIBLE") not in (None, "", "0"),
           "windowId": os.environ.get("DIALOG_WINDOW_ID") or None,
           "capture": os.environ.get("DIALOG_CAPTURE") or None,
           "dump": os.environ["DUMP"] or None}, sys.stdout)
print()' "$@"
}

while IFS= read -r row; do
  [ -z "$row" ] && continue
  label=$(printf '%s' "$row" | cut -f1)
  IFS=$'\t' read -r -a path <<< "$(printf '%s' "$row" | cut -f2-)"

  # Record the window server's view first. A Drover-drawn dialog never enters the
  # AX window list, so this diff is the only reliable way to see that it opened.
  "$BIN" windowlist "$pid" > "$OUT/.before.json"
  clicked=$(osascript "$AX_ROOT/lib/menu-click.applescript" "$APP_PROCESS" "${path[@]}" 2>&1)
  if [ "${clicked#ERROR}" != "$clicked" ]; then
    row_to_json "$clicked" 0 "" "" "$label" "${path[@]}" >> "$OUT/dialogs.jsonl"
    continue
  fi

  new=""
  for _ in 1 2 3 4 5 6 7 8 9 10; do
    "$BIN" windowlist "$pid" > "$OUT/.after.json"
    new=$(python3 -c "
import json
before = {w['windowId'] for w in json.load(open('$OUT/.before.json'))['windows']}
after = json.load(open('$OUT/.after.json'))['windows']
fresh = [w for w in after if w['windowId'] not in before]
print(json.dumps(fresh[0]) if fresh else '')
" 2>/dev/null)
    [ -n "$new" ] && break
    osascript -e 'delay 0.4' >/dev/null 2>&1
  done
  if [ -z "$new" ]; then
    row_to_json "no new window appeared at the window server" 0 "" "" "$label" "${path[@]}" >> "$OUT/dialogs.jsonl"
    continue
  fi

  win=$(printf '%s' "$new" | python3 -c "import json,sys; print(json.load(sys.stdin)['name'])")
  winid=$(printf '%s' "$new" | python3 -c "import json,sys; print(json.load(sys.stdin)['windowId'])")
  # A dialog AX cannot see still has pixels; capture them so its values are recorded.
  screencapture -x -o -l "$winid" "$OUT/dialog-$label.png" 2>/dev/null
  axVisible=$(osascript -e "tell application \"System Events\" to tell process \"$APP_PROCESS\" to get (count of (every window whose name is \"$win\"))" 2>/dev/null)
  export DIALOG_AX_VISIBLE="${axVisible:-0}" DIALOG_WINDOW_ID="$winid"

  "$BIN" tree "$pid" > "$OUT/dialog-$label.json" 2>/dev/null

  # Walk the left pane list, dumping each pane's controls.
  panes=$(osascript "$AX_ROOT/lib/dialog-pane.applescript" "$APP_PROCESS" "$win" count 2>&1)
  if [[ "$panes" =~ ^[0-9]+$ ]] && [ "$panes" -gt 0 ]; then
    : > "$OUT/dialog-$label-panes.jsonl"
    for i in $(seq 1 "$panes"); do
      name=$(osascript "$AX_ROOT/lib/dialog-pane.applescript" "$APP_PROCESS" "$win" select "$i" 2>&1)
      "$BIN" tree "$pid" > "$OUT/dialog-$label-pane-$i.json" 2>/dev/null
      PANE_NAME="$name" PANE_INDEX="$i" PANE_FILE="dialog-$label-pane-$i.json" python3 -c '
import json, os, sys
json.dump({"index": int(os.environ["PANE_INDEX"]), "name": os.environ["PANE_NAME"],
           "dump": os.environ["PANE_FILE"]}, sys.stdout)
print()' >> "$OUT/dialog-$label-panes.jsonl"
    done
  fi

  closed=$(osascript "$AX_ROOT/lib/dialog-close.applescript" "$APP_PROCESS" "$win" 2>&1)
  export PANE_COUNT="$panes" DIALOG_CAPTURE="dialog-$label.png"
  row_to_json "$win" 1 "$closed" "dialog-$label.json" "$label" "${path[@]}" >> "$OUT/dialogs.jsonl"

  # Leaving a dialog open would change the application's state for the next run.
  "$BIN" windowlist "$pid" | python3 -c "
import json, sys
left = [w for w in json.load(sys.stdin)['windows'] if str(w['windowId']) == '$winid']
if left: print('WARNING: $label dialog still open (windowId $winid)', file=sys.stderr)
"
done <<< "$APP_DIALOGS"
rm -f "$OUT/.before.json" "$OUT/.after.json"

echo "$APP_SLUG: done -> $OUT"
