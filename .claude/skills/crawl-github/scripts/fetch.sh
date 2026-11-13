#!/usr/bin/env bash
set -euo pipefail

# --- [CONSTANTS] ------------------------------------------------------------------------

shopt -s nullglob
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
    local work=$1 rules rule number=0
    rules=$(if (($# > 1)); then printf '+%s %s\n' "${3:-0}" "$2"; else rg -v '^(#|$)' - || (($? == 1)); fi)
    [[ -n ${rules} ]] || {
        echo 'no rule on standard input' >&2
        return 1
    }
    awk -F'\t' '$1 == "blob" || $1 == "link" { print $2 "\t" $3 }' "${work}/tree.tsv" >"${work}/files.tsv"
    cut -f2- "${work}/files.tsv" >"${work}/files.txt"
    while IFS= read -r rule; do
        number=$((number + 1))
        [[ ${rule} =~ ^(\+[0-9]*|-)\ (.+)$ ]] || {
            printf 'rule %d is neither "+[<bytes>] <pattern>" nor "- <pattern>": %s\n' "${number}" "${rule}" >&2
            return 1
        }
        { rg -n -- "${BASH_REMATCH[2]}" "${work}/files.txt" || (($? == 1)); } | awk -F: -v verdict="${BASH_REMATCH[1]}" '{ print $1 "\t" verdict }'
    done <<<"${rules}" >"${work}/verdicts.tsv"
    awk -F'\t' '
        FILENAME == ARGV[1] { cap[$1] = substr($2, 2) + 0; taken[$1] = $2 ~ /^\+/; next }
        taken[FNR] && (!cap[FNR] || $1 <= cap[FNR]) { print substr($0, length($1) + 2); paths++; bytes += $1 }
        END { printf "paths=%d bytes=%d\n", paths, bytes >"/dev/stderr" }' "${work}/verdicts.tsv" "${work}/files.tsv"
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

cited() {
    local work=$1 dest pattern=${3:-.} link target
    dest=$(realpath "$2")
    fd --base-directory "${dest}" --hidden --no-ignore --type file --type symlink . >"${work}/held.txt"
    {
        rg --only-matching --no-line-number --with-filename --no-heading --no-ignore --hidden --glob '!/COMMIT' --field-match-separator $'\t' \
            -e '[\w@+~.-]*(?:/[\w@+~.-]+)+|[\w@+~-]+(?:\.[\w+-]+)*' "${dest}" || (($? == 1))
        fd --hidden --no-ignore --type symlink --absolute-path . "${dest}" | while IFS= read -r link; do
            target=$(readlink "${link}")
            printf '%s\t%s\n' "${link}" "${target}"
        done
    } |
        awk -F'\t' -v skip="$((${#dest} + 2))" '
            function resolved(path,    part, count, depth, step, name, joined) {
                count = split(path, part, "/")
                for (step = 1; step <= count; step++) {
                    if (part[step] == "..") depth = depth > 0 ? depth - 1 : 0
                    else if (part[step] != "" && part[step] != ".") name[++depth] = part[step]
                }
                for (step = 1; step <= depth; step++) joined = joined (step > 1 ? "/" : "") name[step]
                return joined
            }
            FILENAME == ARGV[1] { if ($1 == "tree") folder[$3]; else if ($1 != "commit") file[$3]; next }
            FILENAME == ARGV[2] { held[$0]; for (path = $0; sub(/\/[^\/]*$/, "", path);) entered[path]; next }
            {
                base = substr($1, skip)
                token = $2
                sub(/[.\/]+$/, "", token)
                if (!sub(/\/[^\/]*$/, "", base)) base = ""
                if (token == "" || (base, token) in tried) next
                tried[base, token]
                for (climbs = token ~ /\//; ; climbs = climbs && sub(/(^|\/)[^\/]*$/, "", base)) {
                    path = resolved(base "/" token)
                    if (path in file && !(path in held)) print path
                    if (path in folder && !(path in entered) && token ~ /[.\/]/) print path "/"
                    if (path in file || path in folder || !climbs || base == "") break
                }
            }' "${work}/tree.tsv" "${work}/held.txt" - | sort -u | { rg -- "${pattern}" || (($? == 1)); }
}

tally() {
    local root=$1 pattern=$2 folder name files rows='' held=0 total=0
    for folder in "${root}"/[!_]*/; do
        total=$((total + 1))
        files=$(fd --base-directory "${folder}" --hidden --no-ignore --type file --type symlink . | { rg -- "${pattern}" || (($? == 1)); })
        [[ -z ${files} || $# -lt 3 ]] || files=$(cd "${folder}" && tr '\n' '\0' <<<"${files}" | { xargs -0 rg --files-with-matches --no-messages -e "$3" -- || (($? == 1 || $? == 123)); })
        [[ -n ${files} ]] || continue
        held=$((held + 1))
        name=$(basename "${folder}")
        rows+=$(awk -F/ -v name="${name}" '!depth || NF < depth { depth = NF; first = $0 } END { printf "%s/%s\t%d", name, first, NR }' <<<"${files}")$'\n'
    done
    printf '%d/%d\n%s' "${held}" "${total}" "${rows}"
}

# --- [COMPOSITION] ----------------------------------------------------------------------

"$@"
