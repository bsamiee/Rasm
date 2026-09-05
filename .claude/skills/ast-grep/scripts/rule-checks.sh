#!/usr/bin/env bash
# Proves the rules tree sgconfig.yml names: the test run green, ids paired, arms covered, cases and fixes parsed, cases reported once
# Usage: rule-checks.sh <extension>, from the directory holding sgconfig.yml, over the rules of the language that owns the extension
# One line per finding, exit 1 on any, and the directories sgconfig.yml names are siblings
# shellcheck disable=SC2250,SC2312  # Bare names read as the code, and a capture is read for its output alone
set -u
shopt -s globstar nullglob

# --- [ARGUMENTS] ------------------------------------------------------------------------

(($# == 1)) || {
    echo 'usage: rule-checks.sh <extension>'
    exit 1
}
[[ -f sgconfig.yml ]] || {
    echo "sgconfig.yml not found in $PWD"
    exit 1
}
ext=${1#.}
findings=0
finding() {
    printf '%s\n' "$*"
    findings=1
}

# --- [CONSTANTS] ------------------------------------------------------------------------

# A row per rule or util as role, id, file, language, glob, leaf, severity, and a kind at the root, then per file of the language
# an arm a case can flip as op, id, path, and yq mutation, and a calls row per util read
# shellcheck disable=SC2016  # The $ names are jq variables
facts='.id as $id | (.files[0] // "*.\($ext)") as $glob | ($glob | ltrimstr("**/") | gsub("\\*+"; $id)) as $leaf # The leaf: ** dropped, * as the id
  | (if $leaf | test("\\.[^/]*$") then $leaf else "\($leaf)/\($id).\($ext)" end) as $leaf # A directory glob: the case takes <id>.<ext>
  | "\(.role)\t\($id)\t\(.filename)\t\(.language)\t\($glob)\t\($leaf)\t\(.severity // "error")\t\(.rule | has("kind") or has("any"))",
  (select((.language // "" | ascii_downcase) == $lang) | {rule, utils, constraints} | . as $doc
  | def climb($p): if ($p | length) < 2 or ($doc | getpath($p[:-1]) | type) == "array" or ($doc | getpath($p[:-1]) | length) > 1
      then $p else climb($p[:-1]) end; # Deletes at a list element or a map with siblings, an empty map fails to load
  ([
    (paths(type == "object") as $p | getpath($p) | length as $n | keys[]
      | select(IN("not", "stopBy", "nthChild")) # has and inside drop captures a fix needs, field mutants are equivalent
      | {op: "delete", p: (if $n == 1 then climb($p) else $p + [.] end)}),
    (paths(type == "array") as $p | select($p[-1] == "any") | getpath($p) | keys[] | {op: "delete", p: $p + [.]}),
    (.constraints // {} | keys[] | {op: "delete", p: ["constraints", .]}),
    (paths(type == "object" and has("regex")) as $p | {op: "blank", p: $p + ["regex"]}) # has: // binds tighter than !=
  ] | unique[] | (.p | tojson) as $p
    | "\(.op)\t\($id)\t\($p)\t\(if .op == "blank" then "setpath(\($p); \"\")" else "delpaths([\($p)])" end)"),
  ([.. | objects | (.matches? // empty) | (strings, (objects | keys[]))] | unique[] | "calls\t\($id)\t\(.)"))'

# --- [TREE] -----------------------------------------------------------------------------

declare -A failed
out=${ ast-grep test --color never 2>&1;}
rc=$?
while IFS= read -r line; do
    [[ $line =~ ^(FAIL|SKIP|Configuration\ not\ found|Error:|╰▻) ]] && finding "$line"
    [[ $line =~ ^FAIL\ ([^[:space:]]+) ]] && failed[${BASH_REMATCH[1]}]=1
done <<<"$out"
((rc == 0)) || finding "ast-grep test exit $rc"
((rc == 0 || rc == 4)) || exit 1 # Exit 8 or 79: no test ran, later checks would misread the tree

dirs=() rule_files=() util_files=()
while IFS=$'\t' read -r role d; do
    d=${d%/}
    dirs+=("$d")
    case $role in rule) rule_files+=("$d"/**/*.yml) ;; util) util_files+=("$d"/**/*.yml) ;; test) tests=$d ;; *) ;; esac
done < <(yq -r '(.ruleDirs[] | "rule\t" + .), ((.utilDirs // [])[] | "util\t" + .), ("test\t" + .testConfigs[0].testDir),
  ((.customLanguages // {})[].libraryPath | "library\t" + .)' sgconfig.yml)
scratch=${ mktemp -d;}
trap 'rm -rf "$scratch"' EXIT

tree() {
    local root=$1 file=$2 d target
    for d in "${dirs[@]}"; do
        target=$root/$d
        mkdir -p "${target%/*}"
        # -L copies a linked file, a mutation never writes through
        if [[ $file == "$d"/* ]]; then cp -RL "$PWD/$d" "$target"; else ln -s "$PWD/$d" "$target"; fi
    done
    cp sgconfig.yml "$root/sgconfig.yml"
}

tree "$scratch" ''
printf '\n' >"$scratch/probe.$ext"
[[ ${ ast-grep scan -c "$scratch/sgconfig.yml" --inspect entity "$scratch/probe.$ext" 2>&1;} =~ language=([^,]*) ]] || {
    echo "no language owns .$ext"
    exit 1
}
lang=${BASH_REMATCH[1],,}

# --- [FACTS] ----------------------------------------------------------------------------

declare -A file_of lang_of lower rule_glob rule_leaf count text snapshot_key arms_of calls callers base_hits
rule_ids=() util_ids=() test_ids=() snapshot_ids=() test_files=() owned=() f=()
for t in "$tests"/**/*.yml; do [[ $t == "$tests"/__snapshots__/* ]] || test_files+=("$t"); done
snapshot_files=("$tests"/__snapshots__/*.yml)

while IFS=$'\t' read -r role id a b c d e g; do # IFS tabs collapse, an empty middle field shifts the row
    case $role in
        calls) calls[$id]+=" $a" ;;
        delete | blank) arms_of[$id]+=$role$'\t'$a$'\t'$b$'\n' ;;
        *)
            if [[ $role == rule ]]; then rule_ids+=("$id") rule_glob[$id]=$c rule_leaf[$id]=$d; else util_ids+=("$id"); fi
            [[ $role == rule || $g == true ]] || finding "no kind at util root: $id" # A kind-less util walks in quadratic time
            [[ $e == off ]] && finding "severity off: $id" && b=                     # ast-grep runs no test for an off rule, no check reads it
            file_of[$id]=$a lang_of[$id]=${b,,}
            [[ $a == */"$id".yml ]] || finding "id differs from file stem: $a"
            [[ -v lower[${id,,}] ]] && finding "ids differ by case alone: ${lower[${id,,}]} $id" # APFS: one snapshot file serves both ids
            lower[${id,,}]=$id
            ;;
    esac
done < <({
    ((${#rule_files[@]})) && yq -N -o=json -I=0 '.role = "rule" | .filename = filename' "${rule_files[@]}" # No file operand: yq reads stdin
    ((${#util_files[@]})) && yq -N -o=json -I=0 '.role = "util" | .filename = filename' "${util_files[@]}"
} | jq -r --arg lang "$lang" --arg ext "$ext" "$facts")

# -N: a --- between files shifts every later field, -0 -r keeps empty fields
((${#test_files[@]})) && mapfile -d '' -t f < <(yq -N -0 -r '[.id, (keys | join(" ")), (.valid // [] | length),
  (.invalid // [] | length)][], (.valid // [])[], (.invalid // [])[]' "${test_files[@]}")
for ((i = 0; i < ${#f[@]}; i += 4 + v + n)); do
    id=${f[i]} v=${f[i + 2]} n=${f[i + 3]}
    test_ids+=("$id")
    text[keys,$id]=${f[i + 1]} count[valid,$id]=$v count[invalid,$id]=$n # , joins keys: no id holds one, shfmt rewrites / as arithmetic
    for ((c = 1; c <= v; c++)); do text[valid,$id,$c]=${f[i + 3 + c]}; done
    for ((c = 1; c <= n; c++)); do text[invalid,$id,$c]=${f[i + 3 + v + c]}; done
done

f=()
((${#snapshot_files[@]})) && mapfile -d '' -t f < <(yq -N -0 -r '[.id, (.snapshots // {} | length),
  ([.snapshots // {} | .[] | select(has("fixed"))] | length)][], (.snapshots // {} | keys | .[]),
  (.snapshots // {} | .[] | select(has("fixed")) | .fixed)' "${snapshot_files[@]}")
for ((i = 0; i < ${#f[@]}; i += 3 + k + m)); do
    id=${f[i]} k=${f[i + 1]} m=${f[i + 2]}
    snapshot_ids+=("$id")
    count[keys,$id]=$k count[fixed,$id]=$m
    for ((c = 1; c <= k; c++)); do snapshot_key[$id,${f[i + 2 + c]}]=1; done
    for ((c = 1; c <= m; c++)); do text[fixed,$id,$c]=${f[i + 2 + k + c]}; done
done

reach() {
    local u
    callers[$1]+=" $2"
    for u in ${calls[$1]:-}; do [[ " ${callers[$u]-} " == *" $2 "* ]] || reach "$u" "$2"; done
}
for id in "${rule_ids[@]}"; do
    [[ ${lang_of[$id]} == "$lang" ]] || continue
    reach "$id" "$id"
    [[ ${count[invalid,$id]:-0} -gt 0 ]] && owned+=("$id")
done

# --- [CASES] ----------------------------------------------------------------------------

hits_of() {
    local root=$1 id i h file lines trees=()
    local -A hits
    for id in "${@:2}"; do
        for ((i = 1; i <= ${count[invalid,$id]}; i++)); do hits[$id,$i]=0; done
        [[ ${rule_glob[$id]} == '*'* ]] && trees+=("$scratch/invalid/$id") && continue # A glob without a leading * matches at the config root alone
        file=$root/${rule_leaf[$id]}
        mkdir -p "${file%/*}"
        for ((i = 1; i <= ${count[invalid,$id]}; i++)); do
            ln -f "$scratch/invalid/$id/$i/${rule_leaf[$id]}" "$file"
            mapfile -t lines < <(ast-grep scan -c "$root/sgconfig.yml" --filter "^$id\$" --no-ignore hidden --json=stream "$file" 2>/dev/null)
            hits[$id,$i]=${#lines[@]}
        done
    done
    ((${#trees[@]})) && while IFS= read -r i; do ((hits[$i]++)); done < <(ast-grep scan -c "$root/sgconfig.yml" --no-ignore hidden --json=stream \
        "${trees[@]}" 2>/dev/null | jq -r --arg s "$scratch/invalid/" '(.file | ltrimstr($s) | split("/")) as $p
        | select($p[0] == .ruleId) | "\(.ruleId),\($p[1])"')
    for id in "${@:2}"; do
        h=
        for ((i = 1; i <= ${count[invalid,$id]}; i++)); do h+=" ${hits[$id,$i]}"; done
        printf '%s\t%s\n' "$id" "${h# }"
    done
}

# Layout <kind>/<id>/<n>/<leaf>: the jq split indices in hits_of and the parse proof read it
mkdir -p "$scratch"/{invalid,valid,fixed}
for id in "${owned[@]}"; do
    for kind in invalid valid fixed; do
        for ((c = 1; c <= ${count[$kind,$id]:-0}; c++)); do
            file=$scratch/$kind/$id/$c/${rule_leaf[$id]}
            mkdir -p "${file%/*}"
            printf '%s' "${text[$kind,$id,$c]}" >"$file"
        done
    done
done
while IFS=$'\t' read -r id h; do base_hits[$id]=$h; done < <(hits_of "$scratch" "${owned[@]}")

# --- [CHECKS] ---------------------------------------------------------------------------

# Pairing
for id in "${rule_ids[@]}"; do [[ -v text[keys,$id] ]] || finding "no test: $id"; done
for id in "${test_ids[@]}"; do [[ -v rule_glob[$id] ]] || finding "no rule: $id"; done
for id in "${snapshot_ids[@]}"; do [[ -v text[keys,$id] ]] || finding "orphan snapshot: $id"; done

# Test shape
for id in "${test_ids[@]}"; do
    for k in ${text[keys,$id]}; do [[ $k =~ ^(id|valid|invalid)$ ]] || finding "unknown key in $id: $k"; done
    ((${count[valid,$id]} && ${count[invalid,$id]})) || finding "one side empty: $id" # Bare count[valid,$id] in (( )) fails the shellcheck parse
    [[ -v count[keys,$id] ]] || {
        finding "no snapshot: $id"
        continue
    }
    n=0
    for ((c = 1; c <= ${count[invalid,$id]}; c++)); do [[ -v snapshot_key[$id,${text[invalid,$id,$c]}] ]] && ((n++)); done
    ((n == ${count[invalid,$id]} && n == ${count[keys,$id]})) || finding "orphan or missing snapshot key: $id"
done

# Width: past one hit a once-reporting gap, zero a files: glob the case path misses
((${#owned[@]})) || finding "no rule reports under .$ext"
for id in "${owned[@]}"; do
    i=0
    for h in ${base_hits[$id]}; do
        ((++i))
        ((h == 1)) || finding "width $id case $i: $h hits"
    done
done

# Arm coverage: test exit 4 is a flipped case, exit 0 compares the hit counts
cover_arms() {
    local id=$1 file=${file_of[$1]} ids=${*:2} root=$scratch/jobs/$1 op p mutation base caller
    tree "$root" "$file"
    for caller in "${@:2}"; do base+=$caller$'\t'${base_hits[$caller]}$'\n'; done
    while IFS=$'\t' read -r op p mutation; do
        yq "$mutation" "$file" >"$root/$file"
        ast-grep test -c "$root/sgconfig.yml" --filter "^(${ids// /|})\$" --color never >/dev/null 2>&1
        case $? in
            4) ;;
            # ${ } runs in the job shell, an exit inside ends it
            0) [[ ${ hits_of "$root" "${@:2}";} == "${base%$'\n'}" ]] && finding "uncovered arm: $id $op $p" ;;
            *) finding "unchecked arm: $id $op $p exit $?" ;;
        esac
    done <<<"${arms_of[$id]%$'\n'}"
}

mkdir -p "$scratch/jobs"
for id in "${owned[@]}" "${util_ids[@]}"; do
    [[ ${lang_of[$id]} == "$lang" ]] || continue
    [[ -v callers[$id] ]] || {
        finding "no rule calls util: $id"
        continue
    }
    list=() n=0
    for caller in ${callers[$id]}; do
        ((n++))
        [[ -v failed[$caller] || ! -v base_hits[$caller] ]] || list+=("$caller") # A FAIL caller reads every arm as covered
    done
    [[ -v rule_glob[$id] ]] || ((n > 1)) || finding "one rule calls util: $id"
    [[ -v arms_of[$id] && ${#list[@]} -gt 0 ]] || continue
    cover_arms "$id" "${list[@]}" >"$scratch/jobs/$id.out" 2>&1 & # An asynchronous job reads stdin from /dev/null, no step blocks
done
wait
for out in "$scratch"/jobs/*.out; do [[ -s $out ]] && finding "$(<"$out")"; done

# Parse proof
while IFS= read -r line; do finding "$line"; done < <(ast-grep run -c "$scratch/sgconfig.yml" -k ERROR --no-ignore hidden --json=stream \
    "$scratch"/{invalid,valid,fixed} 2>/dev/null |
    jq -rs --arg s "$scratch/" '[.[].file | ltrimstr($s) | split("/") | "ERROR node in \(.[0]) \(.[1]) case \(.[2])"] | unique[]')

# --- [ENTRY] ----------------------------------------------------------------------------

exit "$findings"
