#!/usr/bin/env bash
set -euo pipefail

# --- [CONSTANTS] ------------------------------------------------------------------------

export GIT_LFS_SKIP_SMUDGE=1 GIT_TERMINAL_PROMPT=0 LC_ALL=C

# --- [OPERATIONS] -----------------------------------------------------------------------

walk() {
    local repo=$1 tree=$2 prefix=$3 reply=$4 folders='' folder
    gh api "repos/${repo}/git/trees/${tree}?recursive=1" >"${reply}"
    if jq -e '.truncated' "${reply}" >/dev/null; then
        gh api "repos/${repo}/git/trees/${tree}" >"${reply}"
        folders=$(jq -r '.tree[] | select(.type == "tree") | [.sha, .path] | @tsv' "${reply}")
    fi
    jq -r --arg prefix "${prefix}" '.tree[] | [(if .mode == "120000" then "link" else .type end), (.size // 0 | tostring), $prefix + .path] + (if .type == "commit" then [.sha] else [] end) | join("\t")' "${reply}"
    while IFS=$'\t' read -r tree folder; do
        [[ -z ${tree} ]] || walk "${repo}" "${tree}" "${prefix}${folder}/" "${reply}"
    done <<<"${folders}"
}

map() {
    local repo=$1 ref=${3:-HEAD} work git commit tree
    mkdir -p "$2"
    work=$(realpath "$2")
    git=(git -C "${work}/git")
    [[ -d ${work}/git ]] || git init --quiet --bare "${work}/git"
    "${git[@]}" config remote.origin.url "https://github.com/${repo}"
    "${git[@]}" fetch --quiet --depth 1 --filter=blob:none --no-tags origin "${ref}"
    commit=$("${git[@]}" rev-parse 'FETCH_HEAD^{commit}')
    tree=$("${git[@]}" rev-parse 'FETCH_HEAD^{tree}')
    walk "${repo}" "${tree}" '' "${work}/reply.json" >"${work}/tree.tsv"
    rm "${work}/reply.json"
    printf '%s\n' "${commit}" >"${work}/COMMIT"
    awk -F'\t' -v repo="${repo}" -v commit="${commit}" '
        $1 == "blob" || $1 == "link" { files++; bytes += $2 }
        $1 == "commit" { submodules++ }
        END { printf "%s %s files=%d bytes=%d submodules=%d\n", repo, commit, files, bytes, submodules }' "${work}/tree.tsv"
}

survey() {
    local work=$1 folder=${2:+${2%/}/}
    awk -F'\t' -v folder="${folder}" '
        index($3, folder) != 1 { next }
        { path = substr($3, length(folder) + 1); depth = split(path, part, "/"); name = part[depth] }
        $1 == "commit" { print "5 submodules: commit path", 0, path, $4 "  " path }
        $1 == "tree" || $1 == "commit" { next }
        depth == 1 { print "1 files: bytes name", $2, path, sprintf("%12d  %s", $2, path) }
        depth > 1 { files[part[1] "/"]++; bytes[part[1] "/"] += $2 }
        { kind = match(name, /.\.[A-Za-z0-9_+-]+$/) ? substr(name, RSTART + 1) : "(none)"; kinds[kind]++; weight[kind] += $2; names[name]++ }
        { print "6 largest: bytes path", $2, path, sprintf("%12d  %s", $2, path) }
        END {
            for (key in files) print "2 folders: files bytes name", files[key], key, sprintf("%7d %12d  %s", files[key], bytes[key], key)
            for (key in kinds) print "3 extensions: files bytes extension", kinds[key], key, sprintf("%7d %12d  %s", kinds[key], weight[key], key)
            for (key in names) if (names[key] > 1 || key !~ /.\.[A-Za-z0-9_+-]+$/) print "4 names: files name", names[key], key, sprintf("%7d  %s", names[key], key)
        }' OFS='\t' "${work}/tree.tsv" |
        sort -t$'\t' -k1,1 -k2,2nr -k3,3 |
        awk -F'\t' '
            $1 != section { if (hidden) print "  +" hidden " more"; section = $1; shown = hidden = 0; print "[" substr($1, 3) "]" }
            ++shown > (section ~ /largest/ ? 15 : 60) { hidden++; next }
            { print $4 }
            END {
                if (hidden) print "  +" hidden " more"
                if (!NR) { print "no tree rows under that folder" >"/dev/stderr"; exit 1 }
            }'
}

pick() {
    local work=$1 pattern=$2 cap=${3:-0}
    awk -F'\t' -v cap="${cap}" '($1 == "blob" || $1 == "link") && (cap == 0 || $2 <= cap) { print $3 }' "${work}/tree.tsv" | { rg -- "${pattern}" || (($? == 1)); }
}

take() {
    local paths=$3 work dest git commit
    mkdir -p "$2"
    work=$(realpath "$1")
    dest=$(realpath "$2")
    git=(git -C "${work}/git")
    commit=$(<"${work}/COMMIT")
    "${git[@]}" ls-tree -r -z --full-tree "${commit}" | tr '\0' '\n' | awk -F'\t' -v OFS='\t' '
        FILENAME == ARGV[1] { sub(/\/+$/, ""); if ($0 != "") want[$0] = 0; next }
        { split($1, entry, " ") }
        entry[2] == "commit" && $2 in want { want[$2] = -1 }
        entry[2] != "blob" { next }
        {
            hit = 0
            for (path = $2; path != ""; path = (match(path, /\/[^\/]*$/) ? substr(path, 1, RSTART - 1) : "")) if (path in want) hit = ++want[path]
            if (hit) print entry[1], entry[3], $2
        }
        END {
            if (!length(want)) { print "path list holds no path" >"/dev/stderr"; exit 1 }
            for (path in want) if (want[path] < 1) { print (want[path] ? "submodule at " : "no file at ") path >"/dev/stderr"; missing = 1 }
            exit missing
        }
    ' "${paths}" - >"${work}/taken.tsv"
    cut -f2 "${work}/taken.tsv" | sort -u | "${git[@]}" -c fetch.negotiationAlgorithm=noop fetch --quiet --no-tags --no-write-fetch-head \
        --recurse-submodules=no --filter=blob:none --stdin origin
    GIT_INDEX_FILE="${work}/index" "${git[@]}" read-tree "${commit}"
    cut -f3 "${work}/taken.tsv" | tr '\n' '\0' | GIT_INDEX_FILE="${work}/index" "${git[@]}" --work-tree="${dest}" checkout-index -z --force --prefix="${dest}/" --stdin
    cp "${work}/COMMIT" "${dest}/COMMIT"
    awk -F'\t' '$1 == "120000" { links++; next } { files++ } END { printf "files=%d links=%d\n", files, links }' "${work}/taken.tsv"
}

left() {
    local work=$1 dest=$2 pattern=${3:-.}
    fd --base-directory "${dest}" --hidden --no-ignore --type file --type symlink . | sort >"${work}/held.txt"
    awk -F'\t' '$1 == "blob" || $1 == "link" { print $3 }' "${work}/tree.tsv" | sort | comm -23 - "${work}/held.txt" | { rg -- "${pattern}" || (($? == 1)); }
}

# --- [COMPOSITION] ----------------------------------------------------------------------

"$@"
