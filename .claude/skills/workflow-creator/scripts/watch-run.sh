#!/usr/bin/env bash
# Workflow-run watcher — the Monitor-armed observation loop over one run's scratch dir and journal: phase-bucket first-artifact
# signals, heartbeats, silence/stall/drain detection. Marker-file state dedupes one-shot signals across watcher restarts; growth
# state re-arms stall detection after recovery, so a second wedge still fires. ONESHOT=1 runs a single poll (the pre-arm test).
# Usage: watch-run.sh <scratch-dir> <journal.jsonl> <state-dir> [buckets-file]
#   buckets-file: one `KEY|glob` line per phase — glob resolved inside scratch-dir; the first match fires `PHASE SIGNAL [KEY]` once.
# Env: POLL_S=60 poll interval; HEARTBEAT_S=900 heartbeat cadence; STALL_POLLS=10 unchanged polls before a silence/stall signal.
# Signals: WATCHER ARMED · PHASE SIGNAL [KEY] · HEARTBEAT · RUN SILENT (no journal activity past the window — mis-pathed or
# launch-dead) · RUN STALLED (agents outstanding, zero journal/artifact growth past the window) · RUN DRAINED (results caught
# started on two consecutive polls; the watcher exits). Silence is never health — every terminal state has a signal.
set -u

[ "$#" -ge 3 ] || {
    echo 'usage: watch-run.sh <scratch-dir> <journal.jsonl> <state-dir> [buckets-file]' >&2
    echo '  env: POLL_S=60 HEARTBEAT_S=900 STALL_POLLS=10 ONESHOT=0' >&2
    exit 2
}
S=$1
J=$2
STATE=$3
BUCKETS=${4:-}
HEARTBEAT_S=${HEARTBEAT_S:-900}
POLL_S=${POLL_S:-60}
STALL_POLLS=${STALL_POLLS:-10}
mkdir -p "$STATE" || { echo "watch-run: cannot create state dir: $STATE" >&2; exit 2; }
[ -n "$BUCKETS" ] && [ ! -f "$BUCKETS" ] && echo "watch-run: buckets file not found: $BUCKETS — continuing without phase signals" >&2

mark() { [ -f "$STATE/$1" ] && return 1; : >"$STATE/$1"; return 0; }
stamp() { date +%H:%M; }
count() {
    local n
    n=$(grep -c "$1" "$J" 2>/dev/null) || true
    printf '%s' "${n:-0}"
}

poll() {
    local started results arts jsize
    started=$(count '"type":"started"')
    results=$(count '"type":"result"')
    arts=$(find "$S" -mindepth 1 -maxdepth 1 2>/dev/null | wc -l | tr -d ' ')
    jsize=0
    [ -f "$J" ] && jsize=$(wc -c <"$J" | tr -d ' ')

    # Phase buckets: the first artifact matching a bucket glob fires its signal once, ever (marker-file dedup across restarts).
    if [ -n "$BUCKETS" ] && [ -f "$BUCKETS" ]; then
        local key pat first
        while IFS='|' read -r key pat; do
            { [ -n "$key" ] && [ -n "$pat" ]; } || continue
            first=$(cd "$S" 2>/dev/null && compgen -G "$pat" | head -1) || true
            if [ -n "${first:-}" ] && mark "phase-$key"; then
                echo "$(stamp) PHASE SIGNAL [$key] first artifact: $first (agents $results/$started)"
            fi
        done <"$BUCKETS"
    fi

    # Silence: no journal activity within the stall window means the run is mis-pathed or died at launch — say so, never poll a void quietly.
    if [ "$started" -eq 0 ]; then
        local quiet
        quiet=$(($(cat "$STATE/quiet-polls" 2>/dev/null || echo 0) + 1))
        printf '%s' "$quiet" >"$STATE/quiet-polls"
        if [ "$quiet" -ge "$STALL_POLLS" ] && mark run-silent; then
            echo "$(stamp) RUN SILENT: no journal activity at $J after $quiet polls — wrong paths, or the run died at launch"
        fi
        return 0
    fi
    rm -f "$STATE/quiet-polls"

    if [ "$results" -ge "$started" ]; then
        # Drain: results caught started on two consecutive polls — the run's terminal quiet; the watcher's work is done.
        if [ -f "$STATE/drain-pending" ]; then
            if mark run-drained; then
                echo "$(stamp) RUN DRAINED: agents $results/$started; artifacts: $arts"
                return 1
            fi
        else
            : >"$STATE/drain-pending"
        fi
        rm -f "$STATE/stall-count" "$STATE/stall-fired"
    else
        rm -f "$STATE/drain-pending"
        # Stall: agents outstanding with zero journal/artifact growth across the window. Fires once per episode; growth clears
        # the episode marker so a later wedge re-fires. A stalled run gets inspected, never assumed alive.
        local sig last stall
        sig="$jsize|$arts|$results"
        last=$(cat "$STATE/growth-sig" 2>/dev/null || true)
        if [ "$sig" = "$last" ]; then
            stall=$(($(cat "$STATE/stall-count" 2>/dev/null || echo 0) + 1))
            printf '%s' "$stall" >"$STATE/stall-count"
            if [ "$stall" -ge "$STALL_POLLS" ] && mark stall-fired; then
                echo "$(stamp) RUN STALLED: no journal or artifact growth for $stall polls (agents $results/$started; artifacts $arts) — inspect the run"
            fi
        else
            printf '%s' "$sig" >"$STATE/growth-sig"
            rm -f "$STATE/stall-count" "$STATE/stall-fired"
        fi
    fi

    local now_s last_hb
    now_s=$(date +%s)
    last_hb=$(cat "$STATE/last-hb" 2>/dev/null || echo 0)
    if [ $((now_s - last_hb)) -ge "$HEARTBEAT_S" ]; then
        printf '%s' "$now_s" >"$STATE/last-hb"
        echo "$(stamp) HEARTBEAT: agents $results/$started; artifacts: $arts"
    fi
    return 0
}

if mark watcher-armed; then
    echo "$(stamp) WATCHER ARMED: scratch=$S journal=$J poll=${POLL_S}s stall-window=${STALL_POLLS} polls heartbeat=${HEARTBEAT_S}s"
fi
if [ "${ONESHOT:-0}" = "1" ]; then
    poll
    exit 0
fi
while poll; do sleep "$POLL_S"; done
