#!/usr/bin/env bash
# Pattern : Resolve which binary is charged for a failing send. tccd charges the audit token, so the
#           join key is pid+pidversion, never the pid a log line prints — a recycled pid silently renames
#           the accused. EndpointSecurity supplies the token pairs, the com.apple.TCC rail supplies the
#           attribution chain, and AEDebugSends proves which token the event itself carried.
# Usage   : attribution-correlation.sh correlate | charge <sender> [arg ...]
# shellcheck disable=SC2016 # $-prefixed names inside single quotes are jq variables, not shell expansions
set -euo pipefail

readonly LOG=/usr/bin/log
readonly ESLOGGER=/usr/bin/eslogger
JQ=$(command -v jq)
readonly JQ

# Growth axis: one row per observation stream, command beside projector. Every producer normalizes to a
# {kind, pid, gen, path} record so the correlator stays source-agnostic.
declare -Ar SOURCE_CMD=(
    [es]="sudo $ESLOGGER tcc_modify"
    [tcc]="$LOG stream --debug --style ndjson --predicate 'subsystem == \"com.apple.TCC\"'"
)
# The ES record is authoritative: instigator_token and responsible_token are audit_token_t by value, so
# pidversion rides beside pid. The TCC log record is not — its processID is tccd itself and the sender
# survives only as text inside eventMessage, which is why that lane resolves to a bare pid.
declare -Ar SOURCE_FILTER=(
    [es]='{kind:"grant", service:.event.tcc_modify.service, identity:.event.tcc_modify.identity,
           pid:.event.tcc_modify.instigator_token.pid, gen:.event.tcc_modify.instigator_token.pidversion,
           path:.event.tcc_modify.instigator.executable.path,
           rgen:.event.tcc_modify.responsible_token.pidversion,
           rpath:.event.tcc_modify.responsible.executable.path}'
    [tcc]='select(.eventMessage|test("AttributionChain|sender_pid"))
           | {kind:"chain", pid:(.eventMessage|capture("pid=(?<p>[0-9]+)").p|tonumber?), chain:.eventMessage}'
)

# PIPE_BUF caps atomic pipe writes at 512 bytes on macOS and a tcc eventMessage row runs past it, so a
# mkdir spin-lock hands one producer the whole FIFO write — a complete row lands or the writer waits.
fanin() {
    local pipe=$1 lock=$1.lock tag
    for tag in "${!SOURCE_CMD[@]}"; do
        # Rows are literals, never caller values, so the eval carries no injection surface.
        eval "${SOURCE_CMD[$tag]}" 2>/dev/null |
            "$JQ" -c --unbuffered "${SOURCE_FILTER[$tag]}" |
            {
                lock_held=false
                trap '[[ $lock_held == false ]] || rmdir "$lock" 2>/dev/null || true' EXIT
                while IFS= read -r record; do
                    until mkdir "$lock" 2>/dev/null; do sleep 0.01; done
                    lock_held=true
                    printf '%s\t%s\n' "$tag" "$record"
                    rmdir "$lock"
                    lock_held=false
                done
            } >"$pipe" &
    done
}

# Correlation state keys on token identity, not pid: charged[pid:gen] outlives that pid, and churn[pid]
# counts generations so a second incarnation degrades its own verdict instead of inheriting the first's.
correlate() {
    local -A charged=() generation=() churn=()
    local tag record pid gen path key verdict
    while IFS=$'\t' read -r tag record; do
        pid=$("$JQ" -r '.pid // empty' <<<"$record")
        [[ -n $pid ]] || continue
        if [[ $tag == es ]]; then
            gen=$("$JQ" -r '.gen // empty' <<<"$record")
            path=$("$JQ" -r '.rpath // .path // empty' <<<"$record")
            key="$pid:$gen"
            [[ ${generation[$pid]-} == "$gen" ]] || churn[$pid]=$((${churn[$pid]-0} + 1))
            generation[$pid]=$gen
            charged[$key]=$path
            "$JQ" -c --arg charged "$path" --arg token "$key" \
                '. + {charged:$charged, token:$token, confidence:"token-exact"}' <<<"$record"
            continue
        fi
        gen=${generation[$pid]-}
        verdict='unattributed'
        [[ -n $gen ]] && verdict='pid-window'
        [[ ${churn[$pid]-0} -gt 1 ]] && verdict='pid-recycled'
        "$JQ" -c --arg charged "${charged["$pid:$gen"]-}" --arg verdict "$verdict" \
            '. + {charged:$charged, confidence:$verdict}' <<<"$record"
    done
}

# keySenderAuditTokenAttr ('tokn') carries the token tccd charges; keyActualSenderAuditToken ('acat')
# appears when a helper sends on behalf of a parent and moves the charge onto the responsible token — the
# split EndpointSecurity reports as responsible_audit_token. Neither attribute in the dump means no
# override rode the wire and the sending process's own audit token takes the charge.
charge() {
    local dump attributes
    dump=$(mktemp "${TMPDIR:-/tmp}/aesend.XXXXXX")
    # AEDebug* traces only a process this shell launches, and the descriptor dump lands on the sender's
    # stdout beside its result, so both streams merge into one capture.
    AEDebugSends=1 AEDebugReceives=1 "$@" >"$dump" 2>&1 || true
    attributes=$(grep -oE 'attr:\{[^}]*\}' "$dump" | tr -d ' ' | paste -sd, - || true)
    "$JQ" -nc --arg sender "$1" --arg attributes "$attributes" \
        --argjson tokn "$(grep -c tokn "$dump" || true)" \
        --argjson acat "$(grep -c acat "$dump" || true)" \
        '{sender:$sender, attributes:$attributes, senderToken:$tokn, responsibleToken:$acat,
          charge:(if $acat > 0 then "responsible-token" elif $tokn > 0 then "sender-token" else "process-token" end)}'
    rm -f "$dump"
}

main() {
    local verb=${1:-correlate} workdir
    if [[ $verb == charge ]]; then
        shift
        [[ $# -ge 1 ]] || {
            printf 'usage: charge <sender> [arg ...]\n' >&2
            exit 64
        }
        charge "$@"
        return
    fi
    workdir=$(mktemp -d "${TMPDIR:-/tmp}/attribution.XXXXXX")
    trap 'rm -rf "$workdir"; kill 0 2>/dev/null' EXIT # kill 0 takes this shell with it, so cleanup runs first
    mkfifo "$workdir/pipe"
    fanin "$workdir/pipe"
    correlate <"$workdir/pipe"
}

main "$@"
