#!/usr/bin/env bash
set -euo pipefail

# --- [CONSTANTS] ------------------------------------------------------------------------

here=$(dirname "${BASH_SOURCE[0]}")
readonly here
export LC_ALL=C

# --- [OPERATIONS] -----------------------------------------------------------------------

repos() {
    local since=$1 probes query
    shift
    probes=$(jq -cn '$ARGS.positional' --args "$@")
    jq -Rrn -L "${here}" --argjson probes "${probes}" 'include "discover"; queries($probes)' | while IFS= read -r query; do
        { gh api graphql -F since="${since}T00:00:00Z" -f query="${query}" || true; } |
            jq -cn -L "${here}" --argjson probes "${probes}" 'include "discover"; input | rows($probes)'
    done | jq -cn -L "${here}" 'include "discover"; distinct'
}

search() {
    local query=$1 since=$2 max=$3 cursor=null text reply
    shift 3
    text=$(jq -rn -L "${here}" 'include "discover"; search')
    while ((max > 0)); do
        reply=$(gh api graphql -F q="${query}" -F first="$((max < 100 ? max : 100))" -F after="${cursor}" -f query="${text}")
        [[ ${cursor} != null ]] || jq -r '"matches=\(.data.search.repositoryCount)"' <<<"${reply}" >&2
        jq -r '.data.search.nodes[].nameWithOwner | values' <<<"${reply}"
        cursor=$(jq -r '.data.search.pageInfo | if .hasNextPage then .endCursor else "" end' <<<"${reply}")
        [[ -n ${cursor} ]] || break
        max=$((max - 100))
    done | repos "${since}" "$@"
}

baseline() {
    jq -rs -L "${here}" 'include "discover"; baseline'
}

slate() {
    local bar=$1
    shift
    sort -t$'\t' -k2,2nr -k1,1 "$@" | awk -F'\t' -v bar="${bar}" '
        seen[tolower($1)]++ { next }
        { print ++rank, ($2 < bar ? "below" : $3 == "-" ? "keep" : "near"), $2, $1, $4, $3, $5 }' OFS='\t'
}

code() {
    local query=$1 max=$2 page rows pause lines
    for ((page = 1; page <= 10 && (page - 1) * 100 < max; page++)); do
        until rows=$(gh api -X GET search/code -f q="${query}" -F per_page=100 -F page="${page}" --jq '.items[] | [.repository.full_name, .path] | @tsv'); do
            pause=$(gh api rate_limit --jq '.resources.code_search | if .remaining == 0 then .reset - now + 2 | floor else error("search/code refused the query") end')
            sleep "${pause}"
        done
        [[ -z ${rows} ]] || printf '%s\n' "${rows}"
        lines=$(wc -l <<<"${rows}")
        ((lines == 100)) || break
    done | awk -v max="${max}" 'NR <= max'
}

# --- [COMPOSITION] ----------------------------------------------------------------------

"$@"
