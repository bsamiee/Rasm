#!/usr/bin/env bash
# Proves the rules tree sgconfig.yml names: the test run green, ids paired, arms covered, cases and fixes parsed, cases reported once
# Usage: rule-checks.sh <command> <ext>, from the directory holding sgconfig.yml, over the rules of the language that owns the extension
# rule-checks.sh alone lists the commands, one line per finding, exit 1 on any, and the directories sgconfig.yml names are siblings
# shellcheck disable=SC2250,SC2312  # Unbraced names read as the code, and a capture is read for its output alone
set -u
shopt -s globstar nullglob

# --- [ARGUMENTS] ------------------------------------------------------------------------

declare -A describe=(
    [gate]='Runs test, pairing, width, arms, and parse in order, the full proof of the tree, before a rule, test, util, or snapshot lands'
    [test]='Runs ast-grep test --include-off, prints FAIL, SKIP, Configuration not found, Error, and ╰▻ lines and nonzero exit, after a test changes'
    [pairing]='Pairs rules, tests, and snapshots by id, checks file stems, id case, severity per directory, test and snapshot keys, after one lands'
    [width]='Scans each invalid case, prints width <id> case <n>: <hits> hits, past one a once-reporting gap, zero a missed glob, after a rule lands'
    [arms]='Deletes each arm, prints uncovered arm, unchecked arm, no rule calls util, one rule calls util, no kind at util root, before a rule lands'
    [parse]='Parses each invalid, valid, and fixed case of the language, prints ERROR node in <kind> <id> case <n>, after a case or fix changes'
)
usage() {
    local name
    printf 'usage: rule-checks.sh <command> <ext>, from the directory holding sgconfig.yml, one line per finding and exit 1 on any\n'
    for name in gate test pairing width arms parse; do printf '%-8s %s\n' "$name" "${describe[$name]}"; done
}
if (($# != 2)) || [[ ! -v describe[$1] ]]; then
    usage
    exit $(($# > 0)) # Exit 0 for the argument-less listing, 1 for a wrong arity or command
fi
[[ -f sgconfig.yml ]] || {
    echo "sgconfig.yml not found in $PWD"
    exit 1
}
ext=${2#.}
findings=0
finding() {
    printf '%s\n' "$*"
    findings=1
}

# --- [CONSTANTS] ------------------------------------------------------------------------

# Rows per rule or util as role, id, file, language, glob, leaf, severity, and a kind at the root, then per file of the language
# an arm a case can fail as op, id, path, and yq mutation, and a calls row per util read, rewriters hold arms and calls too
# shellcheck disable=SC2016  # The $ names are jq variables
facts='.id as $id | (.files[0] // "*.\($ext)") as $glob | ($glob | ltrimstr("**/") | gsub("\\*+"; $id)) as $leaf # The leaf: ** dropped, * as the id
  | (if $leaf | test("\\.[^/]*$") then $leaf else "\($leaf)/\($id).\($ext)" end) as $leaf # Directory glob: the case takes <id>.<ext>
  | "\(.role)\t\($id)\t\(.filename)\t\(.language)\t\($glob)\t\($leaf)\t\(.severity // "hint")\t\(.rule | has("kind") or has("any"))",
  (select((.language // "" | ascii_downcase) == $lang) | {rule, utils, constraints, rewriters} | . as $doc
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
test_lines=()
out=${ ast-grep test --include-off --color never 2>&1;}
rc=$?
while IFS= read -r line; do
    [[ $line =~ ^(FAIL|SKIP|Configuration\ not\ found|Error:|╰▻) ]] && test_lines+=("$line")
    [[ $line =~ ^FAIL\ ([^[:space:]]+) ]] && failed[${BASH_REMATCH[1]}]=1
done <<<"$out"
((rc == 0)) || test_lines+=("ast-grep test exit $rc")
((rc == 0 || rc == 4)) || { # Exit 8 or 79: no test ran, every command misreads the tree
    printf '%s\n' "${test_lines[@]}"
    exit 1
}

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

declare -A file_of lang_of severity_of rooted lower rule_glob rule_leaf count text snapshot_key arms_of calls callers base_hits
rule_ids=() util_ids=() test_ids=() snapshot_ids=() test_files=() snapshot_files=() unreadable=() owned=() f=()
# yq exits 0 on a dangling link with an Error line alone, and every test and snapshot id reads as missing
for t in "$tests"/**/*.yml; do
    [[ -r $t ]] || {
        unreadable+=("$t")
        continue
    }
    if [[ $t == "$tests"/__snapshots__/* ]]; then snapshot_files+=("$t"); else test_files+=("$t"); fi
done

while IFS=$'\t' read -r role id a b c d e g; do # IFS tabs collapse, an empty middle field shifts the row
    case $role in
        calls) calls[$id]+=" $a" ;;
        delete | blank) arms_of[$id]+=$role$'\t'$a$'\t'$b$'\n' ;;
        *)
            if [[ $role == rule ]]; then rule_ids+=("$id") rule_glob[$id]=$c rule_leaf[$id]=$d; else util_ids+=("$id"); fi
            file_of[$id]=$a lang_of[$id]=${b,,} severity_of[$id]=$e rooted[$id]=$g
            [[ -v lower[${id,,}] ]] || lower[${id,,}]=$id # Earliest id per lowercase spelling, a later one differs by case alone
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
    local root=$1 id i h file lines trees=() errors=()
    local -A hits
    for id in "${@:2}"; do
        for ((i = 1; i <= ${count[invalid,$id]}; i++)); do hits[$id,$i]=0; done
        # Globs without a leading * match at the config root alone, and --error=<id> loads a rewrite the scan skips at off
        [[ ${rule_glob[$id]} == '*'* ]] && trees+=("$scratch/invalid/$id") && errors+=(--error="$id") && continue
        file=$root/${rule_leaf[$id]}
        mkdir -p "${file%/*}"
        for ((i = 1; i <= ${count[invalid,$id]}; i++)); do
            ln -f "$scratch/invalid/$id/$i/${rule_leaf[$id]}" "$file"
            mapfile -t lines < <(ast-grep scan -c "$root/sgconfig.yml" --filter "^$id\$" --error="$id" --no-ignore hidden --json=stream "$file" \
                2>/dev/null)
            hits[$id,$i]=${#lines[@]}
        done
    done
    ((${#trees[@]})) && while IFS= read -r i; do ((hits[$i]++)); done < <(ast-grep scan -c "$root/sgconfig.yml" "${errors[@]}" --no-ignore hidden \
        --json=stream "${trees[@]}" 2>/dev/null | jq -r --arg s "$scratch/invalid/" '(.file | ltrimstr($s) | split("/")) as $p
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

check_test() {
    local line
    for line in "${test_lines[@]}"; do finding "$line"; done
}

check_pairing() {
    local id k c n t
    for t in "${unreadable[@]}"; do finding "unreadable test: $t"; done
    for id in "${rule_ids[@]}" "${util_ids[@]}"; do
        [[ ${file_of[$id]} == */"$id".yml ]] || finding "id differs from file stem: ${file_of[$id]}"
        [[ ${lower[${id,,}]} == "$id" ]] || finding "ids differ by case alone: ${lower[${id,,}]} $id" # APFS: one snapshot file serves both ids
        [[ -v rule_glob[$id] ]] || continue
        # Rewrites under rewrites/ are off and run under --error=<id>, lint rules under rules/ are error and gate the scan
        case ${file_of[$id]} in
            */rewrites/*) [[ ${severity_of[$id]} == off ]] || finding "severity ${severity_of[$id]} under rewrites: $id" ;;
            *) [[ ${severity_of[$id]} == error ]] || finding "severity ${severity_of[$id]} under rules: $id" ;;
        esac
    done
    for id in "${rule_ids[@]}"; do [[ -v text[keys,$id] ]] || finding "no test: $id"; done
    for id in "${test_ids[@]}"; do [[ -v rule_glob[$id] ]] || finding "no rule: $id"; done
    for id in "${snapshot_ids[@]}"; do [[ -v text[keys,$id] ]] || finding "orphan snapshot: $id"; done
    for id in "${test_ids[@]}"; do
        for k in ${text[keys,$id]}; do [[ $k =~ ^(id|valid|invalid)$ ]] || finding "unknown key in $id: $k"; done
        ((${count[valid,$id]} && ${count[invalid,$id]})) || finding "one side empty: $id" # Unbraced count[valid,$id] fails the shellcheck parse
        [[ -v count[keys,$id] ]] || {
            finding "no snapshot: $id"
            continue
        }
        n=0
        for ((c = 1; c <= ${count[invalid,$id]}; c++)); do [[ -v snapshot_key[$id,${text[invalid,$id,$c]}] ]] && ((n++)); done
        ((n == ${count[invalid,$id]} && n == ${count[keys,$id]})) || finding "orphan or missing snapshot key: $id"
    done
}

# Width: past one hit a once-reporting gap, zero a files: glob the case path misses
check_width() {
    local id i h
    ((${#owned[@]})) || finding "no rule reports under .$ext"
    for id in "${owned[@]}"; do
        i=0
        for h in ${base_hits[$id]}; do
            ((++i))
            ((h == 1)) || finding "width $id case $i: $h hits"
        done
    done
}

# Arm coverage: test exit 4 is a failed case, exit 0 compares the hit counts
cover_arms() {
    local id=$1 file=${file_of[$1]} ids=${*:2} root=$scratch/jobs/$1 op p mutation base caller
    tree "$root" "$file"
    for caller in "${@:2}"; do base+=$caller$'\t'${base_hits[$caller]}$'\n'; done
    while IFS=$'\t' read -r op p mutation; do
        yq "$mutation" "$file" >"$root/$file"
        ast-grep test -c "$root/sgconfig.yml" --include-off --filter "^(${ids// /|})\$" --color never >/dev/null 2>&1
        case $? in
            4) ;;
            # ${ } runs in the job shell, an exit inside ends it
            0) [[ ${ hits_of "$root" "${@:2}";} == "${base%$'\n'}" ]] && finding "uncovered arm: $id $op $p" ;;
            *) finding "unchecked arm: $id $op $p exit $?" ;;
        esac
    done <<<"${arms_of[$id]%$'\n'}"
}

check_arms() {
    local id list n caller out
    for id in "${util_ids[@]}"; do
        [[ ${rooted[$id]} == true ]] || finding "no kind at util root: $id" # Kind-less utils walk in quadratic time
    done
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
            [[ -v failed[$caller] || ! -v base_hits[$caller] ]] || list+=("$caller") # FAIL callers read every arm as covered
        done
        [[ -v rule_glob[$id] ]] || ((n > 1)) || finding "one rule calls util: $id"
        [[ -v arms_of[$id] && ${#list[@]} -gt 0 ]] || continue
        cover_arms "$id" "${list[@]}" >"$scratch/jobs/$id.out" 2>&1 & # Asynchronous jobs read stdin from /dev/null, no step blocks
    done
    wait
    for out in "$scratch"/jobs/*.out; do [[ -s $out ]] && finding "$(<"$out")"; done
}

check_parse() {
    local line
    while IFS= read -r line; do finding "$line"; done < <(ast-grep run -c "$scratch/sgconfig.yml" -k ERROR --no-ignore hidden --json=stream \
        "$scratch"/{invalid,valid,fixed} 2>/dev/null |
        jq -rs --arg s "$scratch/" '[.[].file | ltrimstr($s) | split("/") | "ERROR node in \(.[0]) \(.[1]) case \(.[2])"] | unique[]')
}

check_gate() {
    check_test
    check_pairing
    check_width
    check_arms
    check_parse
}

# --- [ENTRY] ----------------------------------------------------------------------------

case $1 in
    gate) check_gate ;;
    test) check_test ;;
    pairing) check_pairing ;;
    width) check_width ;;
    arms) check_arms ;;
    parse) check_parse ;;
    *) exit 1 ;;
esac
exit "$findings"
