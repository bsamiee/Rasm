#!/usr/bin/env bash
# Checks configured rule registration, fixture matches, mutation coverage, and syntax
# Usage: rule-checks.sh <command> [<ext> [<id-regex>]], from the directory holding sgconfig.yml, one line per finding and exit 1 on any
# shellcheck disable=SC2250,SC2312  # Unbraced names read as the code, and a capture is read for its output alone
set -euo pipefail
shopt -s globstar nullglob dotglob lastpipe

# --- [ARGUMENTS] ------------------------------------------------------------------------

case ${1-} in
    pairing) (($# == 1)) ;;
    width | arms | parse | gate) (($# == 2 || $# == 3)) ;;
    measure) (($# >= 3)) ;;
    *) false ;;
esac || {
    printf '%s\n' \
        'usage: rule-checks.sh <command> [<ext> [<id-regex>]], from the directory holding sgconfig.yml, exit 1 on findings' \
        'pairing  Pairs rules, tests, and snapshots by id, checks stems, severity, and fixture keys' \
        'width    <ext> [<id-regex>]  Reports invalid cases with zero or multiple hits' \
        'arms     <ext> [<id-regex>]  Mutates arms and compares detection and fixes, reporting invalid mutations separately' \
        'parse    <ext> [<id-regex>]  Checks original and fixed cases with bash -n or structural ERROR searches' \
        'gate     <ext> [<id-regex>]  Runs ast-grep test, pairing, width, arms, and parse' \
        'measure  <ext> <path>...  Prints top-level elements and nesting violations'
    exit $(($# > 0)) # Exit 0 for the argument-less listing, 1 for a wrong arity or command
}
[[ -f sgconfig.yml ]] || {
    printf 'sgconfig.yml not found in %s\n' "$PWD"
    exit 1
}
command=$1 ext=${2-} filter=${3-}
ext=${ext#.}
findings=0
finding() {
    printf '%s\n' "$*"
    findings=1
}

# --- [CONSTANTS] ------------------------------------------------------------------------

# Rows per rule or util as role, id, file, language, leaf, and severity, then per file of the language
# an arm a case can fail as op, id, path, and the mutated file as JSON, and a calls row per util read, rewriters hold arms and calls too
# The leaf is the case path under the config root: a glob with ** or an implied **/ prefix takes <id>/@N@ per case, an anchored
# name stays fixed and its cases scan one at a time, and a leaf with no extension takes <id>.<ext> as its file
# Terminal extension alternatives select the requested extension, or the first listed one, only for the fixture path
# shellcheck disable=SC2016  # The $ names are jq variables
facts='.id as $id | (.files[0] // "*.\($ext)") as $glob | ($glob | ltrimstr("**/")) as $rest
  | (if $rest | test("\\*\\*") then $rest | gsub("\\*\\*"; "\($id)/@N@") | gsub("\\*"; $id)
     elif $glob | test("^(\\*\\*/|\\*)") then "\($id)/@N@/\($rest | gsub("\\*"; $id))" else $rest end) as $leaf
  | (if $leaf | test("\\.\\{[[:alnum:]_-]+(,[[:alnum:]_-]+)+\\}$") then
       $leaf | capture("^(?<stem>.*)\\.\\{(?<extensions>[^{}]+)\\}$")
       | (.extensions | split(",")) as $extensions
       | "\(.stem).\(if $extensions | index($ext) then $ext else $extensions[0] end)"
     else $leaf end) as $leaf
  | (if $leaf | test("\\.[^/]*$") then $leaf else "\($leaf)/\($id).\($ext)" end) as $leaf
  | [.role, $id, .filename, .language, $leaf, (.severity // "hint")],
  (select((.arguments // [] | length) > 0) | ["parameterized", $id]),
  (select([.. | objects | has("expandStart") or has("expandEnd")] | any) | ["expand", $id]),
  (select($mutations and ((.language // "" | ascii_downcase) == $lang)) | del(.role, .filename) as $full | {rule, utils, constraints, rewriters} | . as $doc
  # Deletes at a list element with siblings or a map with a rule key left, a map left with field and stopBy alone fails the load
  | def climb($p): if ($p | length) < 2 then $p else ($doc | getpath($p[:-1])) as $parent
      | if ($parent | type) == "array" then (if ($parent | length) > 1 then $p else climb($p[:-1]) end)
        elif (($parent | keys) - [$p[-1]] - ["field", "stopBy"] | length) > 0 then $p else climb($p[:-1]) end end;
  ([
    (paths(type == "object") as $p | getpath($p) | keys[]
      | select(IN("not", "stopBy", "nthChild")) # has and inside drop captures a fix needs, field mutants are equivalent
      | {op: "delete", p: climb($p + [.])}),
    (paths(type == "array") as $p | select($p[-1] == "any") | getpath($p) | select(length > 1) | keys[] | {op: "delete", p: $p + [.]}),
    (.constraints // {} | keys[] | {op: "delete", p: climb(["constraints", .])}),
    (paths(type == "object" and has("regex")) as $p | {op: "blank", p: $p + ["regex"]}) # has: // binds tighter than !=
  ] | unique[] | .p as $p
    | [.op, $id, ($p | tojson), (if .op == "blank" then $full | setpath($p; "") else $full | delpaths([$p]) end | tojson)]),
  ([.. | objects | (.matches? // empty) | (strings, (objects | keys[]))] | unique[] | ["calls", $id, .]))'

# Element selector and function-depth rule per language, the two counts a fix is measured by
declare -A elements=(
    [tsx]=':is(program, export_statement) > :is(lexical_declaration, variable_declaration, function_declaration, generator_function_declaration, type_alias_declaration, interface_declaration, class_declaration, abstract_class_declaration, enum_declaration, ambient_declaration), export_statement > :is(function_expression, arrow_function), program > expression_statement > internal_module'
    [python]='module > :is(function_definition, class_definition, type_alias_statement), module > expression_statement > assignment'
)
declare -A nesting=([tsx]=no-fourth-nesting-level [python]=no-fourth-python-nesting-level)

# --- [TREE] -----------------------------------------------------------------------------

dirs=() rule_dirs=() rule_files=() util_files=() test_dirs=() snapshot_dirs=()
declare -A dir_role
yq -0 -r '(.ruleDirs[] | ["rule", .] | .[]), ((.utilDirs // [])[] | ["util", .] | .[]), (.testConfigs[] | ["test", .testDir] | .[]),
  (.testConfigs[] | ["snapshot", .testDir + "/" + (.snapshotDir // "__snapshots__")] | .[]),
  ([((.customLanguages // {})[].libraryPath | (select(tag == "!!str"), select(tag == "!!map")[]))] | unique | .[]
    | select(test("^/") | not) | ["library", .] | .[])' sgconfig.yml |
    while IFS= read -r -d '' role && IFS= read -r -d '' d; do
        d=${d%/}
        if [[ $role == snapshot ]]; then
            [[ ! -d $d ]] || d=$(cd "$d" && printf '%s/' "$PWD")
            d=${d%/}
            snapshot_dirs+=("$d")
            continue
        fi
        [[ $d == /* ]] || dirs+=("$d")
        dir_role[$d]=$role
        case $role in
            rule) rule_dirs+=("$d") rule_files+=("$d"/**/*.yml) ;;
            util) rule_dirs+=("$d") util_files+=("$d"/**/*.yml) ;;
            test)
                resolved=$(cd "$d" && printf '%s/' "$PWD")
                test_dirs+=("${resolved%/}")
                ;;
            *) ;;
        esac
    done
scratch=$(mktemp -d)
trap 'rm -rf "$scratch"' EXIT
cases=$scratch/cases
lang=
job_rules=() job_utils=() # The language's package directories, or a language directory holding a rule file or no directory
job_config=               # sgconfig.yml over them under @JOB@, the test directory and library absolute, one text per job

# A root for one mutation batch over the id's file: the language's package directories linked and the one holding the file
# copied, each a rule or util directory of the job config, written at the root for the anchored cases and at the cases root
# for the rest, a files glob matches no case reached through a linked path and the shared cases copy into no job
job_root() {
    local root=$1 config=$2 id=$3 d source text parents=("${job_rules[@]}" "${job_utils[@]}")
    parents=("${parents[@]/#/"$root"/}")
    mkdir -p "${parents[@]%/*}"
    for d in "${job_rules[@]}" "${job_utils[@]}"; do
        source=$d
        [[ $source == /* ]] || source=$PWD/$source
        if [[ ${file_of[$id]} == "$d" || ${file_of[$id]} == "$d"/* ]]; then cp -R "$source" "$root/$d"; else ln -s "$source" "$root/$d"; fi
    done
    text=${job_config//@JOB@/"$root"}
    printf '%s' "$text" >"$root/sgconfig.yml"
    printf '%s' "$text" >"$config"
}

# The language sgconfig.yml maps the extension to, read from the file entity of an empty probe file
owning_language() {
    local probe rc=0 d paths=("${dirs[@]/#/"$cases"/}")
    # Link configured paths beneath the probe config without copying the rule tree
    mkdir -p "$cases" "${paths[@]%/*}"
    for d in "${dirs[@]}"; do ln -s "$PWD/$d" "$cases/$d"; done
    cp sgconfig.yml "$cases/sgconfig.yml"
    printf '\n' >"$cases/probe.$ext"
    probe=$(ast-grep scan -c "$cases/sgconfig.yml" --inspect entity "$cases/probe.$ext" 2>&1) || rc=$?
    [[ $probe =~ language=([^,]*) ]] || {
        # A tree the loader refuses prints its own lines, a loaded tree with no owner prints the extension
        if ((rc)); then rg --no-config -v '^sg:' <<<"$probe"; else printf 'no language owns .%s\n' "$ext"; fi
        exit 1
    }
    lang=${BASH_REMATCH[1],,}
}

# --- [FACTS] ----------------------------------------------------------------------------

declare -A file_of lang_of severity_of lower rule_leaf count text snapshot_key snapshot_file_of test_file_of arms_of calls callers base_hits lang_dirs failed
declare -A expanded=() parameterized=() case_path=()
declare -A scoped=() # The one associative array counted, a count over an array with no assignment is unbound under -u
rule_ids=() util_ids=() test_ids=() snapshot_ids=() test_files=() snapshot_files=() unreadable=() lang_rules=() owned=()

read_facts() {
    local t role id c d e i v n k m row f=()
    local -A is_snapshot=() seen=()
    # yq exits 0 on a dangling link with an Error line alone, and every test and snapshot id reads as missing
    for d in "${snapshot_dirs[@]}"; do
        for t in "$d"/**/*.yml; do is_snapshot[$t]=1; done
    done
    f=("${!is_snapshot[@]}")
    for d in "${test_dirs[@]}"; do f+=("$d"/**/*.yml); done
    for t in "${f[@]}"; do
        [[ ! -v seen[$t] ]] || continue
        seen[$t]=1
        [[ -r $t ]] || {
            unreadable+=("$t")
            continue
        }
        if [[ -v is_snapshot[$t] ]]; then snapshot_files+=("$t"); else test_files+=("$t"); fi
    done
    f=()
    { # No file operand: yq reads stdin, blocked on a terminal and usage text on /dev/null
        ((${#rule_files[@]} == 0)) || yq -N -o=json -I=0 '.role = "rule" | .filename = filename' "${rule_files[@]}"
        ((${#util_files[@]} == 0)) || yq -N -o=json -I=0 '.role = "util" | .filename = filename' "${util_files[@]}"
    } | jq -j --arg lang "$lang" --arg ext "$ext" --argjson mutations "$mutations" \
        "$facts | . + [range(length; 6) | \"\"] | .[] + \"\\u0000\"" |
        while mapfile -d '' -t -n 6 row && ((${#row[@]})); do
            role=${row[0]} id=${row[1]}
            case $role in
                expand) expanded[$id]=1 ;;
                parameterized) parameterized[$id]=1 ;;
                calls) calls[$id]+=" ${row[2]}" ;;
                delete | blank) arms_of[$id]+=$role$'\t'${row[2]}$'\t'${row[3]}$'\n' ;;
                rule) rule_ids+=("$id") rule_leaf[$id]=${row[4]} ;;&
                util) util_ids+=("$id") ;;&
                rule | util)
                    file_of[$id]=${row[2]} lang_of[$id]=${row[3],,} severity_of[$id]=${row[5]}
                    [[ -v lower[${id,,}] ]] || lower[${id,,}]=$id # Earliest id per lowercase spelling
                    ;;
                *) ;;
            esac
        done
    # Validate fixture documents before flattening them; NUL records preserve source whitespace and empty fields.
    if ((${#test_files[@]})); then
        yq -N -o=json -I=0 '{"file": filename, "test": .}' "${test_files[@]}" | jq -j '
          .file as $file | .test | . as $doc | ["valid", "invalid"] as $sides
          | [$sides[] as $side | $doc[$side] | if type == "array" then .[] | select(type == "string") else empty end] as $cases
          | [.id, (keys | join(" ")),
             (.valid | if type == "array" then map(strings) | length else 0 end),
             (.invalid | if type == "array" then map(strings) | length else 0 end), $file,
             ([$sides[] as $side | $doc[$side]
               | select(type != "array" or any(.[]?; type != "string")) | "non-string case or non-array side: \($doc.id) \($side)"]
              + [$cases | group_by(.)[] | select(length > 1) | "duplicate or contradictory case: \($doc.id)"] | unique | join("\n"))]
            + $cases | .[] | tostring + "\u0000"' | mapfile -d '' -t f
    fi
    for ((i = 0; i < ${#f[@]}; i += 6 + v + n)); do
        id=${f[i]} v=${f[i + 2]} n=${f[i + 3]}
        [[ -z ${f[i + 5]} ]] || finding "${f[i + 5]}"
        [[ ! -v test_file_of[$id] ]] || {
            finding "duplicate test id: $id"
            continue
        }
        test_ids+=("$id") test_file_of[$id]=${f[i + 4]}
        text["keys,$id"]=${f[i + 1]} count["valid,$id"]=$v count["invalid,$id"]=$n
        for ((c = 1; c <= v; c++)); do text["valid,$id,$c"]=${f[i + 5 + c]}; done
        for ((c = 1; c <= n; c++)); do text["invalid,$id,$c"]=${f[i + 5 + v + c]}; done
    done
    f=()
    if ((${#snapshot_files[@]})); then
        yq -N -0 -r '[.id, (.snapshots // {} | length),
          ([.snapshots // {} | .[] | select(has("fixed"))] | length), filename][], (.snapshots // {} | keys | .[]),
          (.snapshots // {} | .[] | select(has("fixed")) | .fixed)' "${snapshot_files[@]}" | mapfile -d '' -t f
    fi
    for ((i = 0; i < ${#f[@]}; i += 4 + k + m)); do
        id=${f[i]} k=${f[i + 1]} m=${f[i + 2]}
        snapshot_ids+=("$id")
        count["keys,$id"]=$k count["fixed,$id"]=$m
        snapshot_file_of[$id]=${f[i + 3]}
        for ((c = 1; c <= k; c++)); do snapshot_key["$id,${f[i + 3 + c]}"]=1; done
        for ((c = 1; c <= m; c++)); do text["fixed,$id,$c"]=${f[i + 3 + k + c]}; done
    done
    for id in "${rule_ids[@]}" "${util_ids[@]}"; do
        [[ ${lang_of[$id]} == "$lang" ]] || continue
        for d in "${rule_dirs[@]}"; do
            # The directory under the rule directory holding the language's files, or the file itself when none
            [[ ${file_of[$id]} == "$d"/* ]] || continue
            e=${file_of[$id]#"$d"/}
            lang_dirs["$d/${e%%/*}"]=1
        done
        [[ -v rule_leaf[$id] ]] || continue
        lang_rules+=("$id")
        reach "$id" "$id"
        ((${count["invalid,$id"]:-0})) || continue
        owned+=("$id") # Every rule of the language with cases, the callers a util's arms are proven against
        [[ -z $filter || $id =~ $filter ]] || continue
        scoped[$id]=1 # The rules the regex names, the ones width, arms, and parse report on
    done
}

reach() {
    local u
    callers[$1]+=" $2"
    for u in ${calls[$1]:-}; do [[ " ${callers[$u]-} " == *" $2 "* ]] || reach "$u" "$2"; done
}

# The ids as one anchored alternation into the named variable, the --filter regex of a test or scan over them
# shellcheck disable=SC2034  # The nameref writes the caller's variable
alternation() {
    local -n joined=$1
    local IFS='|'
    shift
    joined="^($*)\$"
}

# --- [CASES] ----------------------------------------------------------------------------

# Invalid cases sit under the root at their leaf, an anchored leaf under anchored/<id>/<n>, valid and fixed under their kind,
# case_path records each complete path, including any directories retained from the file glob
write_cases() {
    local id kind c leaf file d n parents=()
    for id in "${owned[@]}"; do
        leaf=${rule_leaf[$id]}
        for d in "${dirs[@]}"; do
            # The configured directories are links into the tree, a case under one writes through the link into the tree
            [[ $leaf != "$d/"* ]] || {
                finding "case path under $d: $id"
                exit 1
            }
        done
        for ((c = 1; c <= ${count["invalid,$id"]}; c++)); do
            if [[ $leaf == *@N@* ]]; then case_path["invalid,$id,$c"]=$cases/${leaf//@N@/$c}; else case_path["invalid,$id,$c"]=$cases/anchored/$id/$c/${leaf##*/}; fi
        done
        if [[ $leaf != *@N@* ]]; then file=$cases/$leaf parents+=("${file%/*}"); fi
        for kind in valid fixed; do
            n=${count["$kind,$id"]:-0}
            [[ $kind != fixed || ! -v expanded[$id] ]] || n=${count["invalid,$id"]}
            for ((c = 1; c <= n; c++)); do case_path["$kind,$id,$c"]=$cases/$kind/$id/$c/${leaf##*/}; done
        done
    done
    mkdir -p "$cases"/{valid,fixed,anchored} "${case_path[@]%/*}" "${parents[@]}" # One process for every directory, a run per file costs seconds
    for file in "${!case_path[@]}"; do
        id=${file#*,} id=${id%,*}
        [[ $file != fixed,* || ! -v expanded[$id] ]] || continue
        printf '%s' "${text[$file]}" >"${case_path[$file]}"
    done
}

# Findings exit 1, while loader and execution failures retain their diagnostics and status
scan_matches() {
    local rc=0 errors=$scratch/scan-$BASHPID.log
    ast-grep scan --json=stream "$@" 2>"$errors" || rc=$?
    ((rc == 0 || rc == 1)) || {
        cat "$errors" >&2
        return "$rc"
    }
}

# Hits per invalid case into the named associative array, one scan under the cases-root config over the case directories of
# every batched id and one per anchored case at its leaf under the root
# shellcheck disable=SC2034,SC2004  # The nameref writes the caller's array, and its subscript is a string
hits_of() {
    local -n found=$1
    local root=$2 config=$3 id c h key leaf file lines regex threads=0 dirs=() ids=() errors=() anchored=() files=()
    local -A hits case_keys
    [[ $root == "$cases" ]] || threads=1
    for id in "${@:4}"; do
        leaf=${rule_leaf[$id]}
        for ((c = 1; c <= ${count["invalid,$id"]}; c++)); do
            hits["$id,$c"]=0 case_keys["$id,${case_path["invalid,$id,$c"]}"]=$id,$c
        done
        # --error=<id> loads a rewrite the scan skips at off
        if [[ $leaf == *@N@* ]]; then dirs+=("$cases/${leaf%%/@N@*}") ids+=("$id") errors+=(--error="$id"); else anchored+=("$id") files+=("$root/$leaf"); fi
    done
    ((${#files[@]} == 0)) || mkdir -p "${files[@]%/*}"
    for id in "${anchored[@]}"; do
        leaf=${rule_leaf[$id]} file=$root/$leaf
        for ((c = 1; c <= ${count["invalid,$id"]}; c++)); do
            ln -f "$cases/anchored/$id/$c/${leaf##*/}" "$file"
            scan_matches -c "$root/sgconfig.yml" --filter "^$id\$" --error="$id" --threads 1 \
                --no-ignore hidden "$file" | mapfile -t lines
            hits["$id,$c"]=${#lines[@]}
        done
    done
    ((${#files[@]} == 0)) || rm -f "${files[@]}"
    if ((${#dirs[@]})); then
        alternation regex "${ids[@]}"
        scan_matches -c "$config" --filter "$regex" "${errors[@]}" --threads "$threads" \
            --no-ignore hidden "${dirs[@]}" |
            jq -j '"\(.ruleId),\(.file)\u0000"' |
            while IFS= read -r -d '' key; do [[ ! -v case_keys[$key] ]] || ((++hits["${case_keys[$key]}"])); done
    fi
    for id in "${@:4}"; do
        h=
        for ((c = 1; c <= ${count["invalid,$id"]}; c++)); do h+=" ${hits["$id,$c"]}"; done
        found[$id]=${h# }
    done
}

# Baseline every language caller before shared utility mutations, including callers outside the requested filter
run_test() {
    local out rc=0 line regex
    ((${#lang_rules[@]})) || return 0
    alternation regex "${lang_rules[@]}"
    out=$(ast-grep test --include-off --filter "$regex" --color never 2>&1) || rc=$?
    while IFS= read -r line; do
        [[ $line =~ ^FAIL\ ([^[:space:]]+) ]] && failed[${BASH_REMATCH[1]}]=1
        ((rc == 0)) || [[ -z $line || $line == PASS\ * ]] || finding "$line"
    done <<<"$out"
}

# --- [CHECKS] ---------------------------------------------------------------------------

check_pairing() {
    local id k c n t
    for t in "${unreadable[@]}"; do finding "unreadable test: $t"; done
    for id in "${rule_ids[@]}" "${util_ids[@]}"; do
        [[ ${file_of[$id]} == */"$id".yml ]] || finding "id differs from file stem: ${file_of[$id]}"
        [[ ${lower[${id,,}]} == "$id" ]] || finding "ids differ by case alone: ${lower[${id,,}]} $id" # APFS: one snapshot file serves both ids
        [[ -v rule_leaf[$id] ]] || continue
        # Rewrites under rewrites/ are off and run under --error=<id>, lint rules under rules/ are error and gate the scan
        case /${file_of[$id]} in # The leading separator lets rewrites/ at the config root match
            */rewrites/*) [[ ${severity_of[$id]} == off ]] || finding "severity ${severity_of[$id]} under rewrites: $id" ;;
            *) [[ ${severity_of[$id]} == error ]] || finding "severity ${severity_of[$id]} under rules: $id" ;;
        esac
    done
    for id in "${rule_ids[@]}"; do [[ -v text["keys,$id"] ]] || finding "no test: $id"; done
    for id in "${test_ids[@]}"; do [[ -v rule_leaf[$id] ]] || finding "no rule: $id"; done
    for id in "${snapshot_ids[@]}"; do [[ -v text["keys,$id"] ]] || finding "orphan snapshot: $id"; done
    for id in "${test_ids[@]}"; do
        for k in ${text["keys,$id"]}; do [[ $k =~ ^(id|valid|invalid)$ ]] || finding "unknown key in $id: $k"; done
        ((${count["valid,$id"]} && ${count["invalid,$id"]})) || finding "one side empty: $id" # Unbraced count["valid,$id"] fails the shellcheck parse
        [[ -v count["keys,$id"] ]] || {
            finding "no snapshot: $id"
            continue
        }
        n=0
        for ((c = 1; c <= ${count["invalid,$id"]}; c++)); do [[ ! -v snapshot_key["$id,${text["invalid,$id,$c"]}"] ]] || ((++n)); done
        ((n == ${count["invalid,$id"]} && n == ${count["keys,$id"]})) || finding "orphan or missing snapshot key: $id"
    done
}

# Width: past one hit a once-reporting gap, zero a files: glob the case path misses
check_width() {
    local id i h
    for id in "${!scoped[@]}"; do
        i=0
        for h in ${base_hits[$id]}; do
            ((++i))
            ((h == 1)) || finding "width $id case $i: $h hits, ${text["invalid,$id,$i"]%%$'\n'*}"
        done
    done
}

# Classification, hit counts, and fixed source prove mutation coverage; diagnostic label changes do not
cover_arms() {
    local root=$scratch/jobs/$1 config=$cases/job-$1.yml id=$2 block=$3 moved rc regex caller op p mutation out expected actual snapshots=() originals=()
    local -A got tests
    local fixes='map({id, snapshots: (.snapshots | map_values(.fixed))}) | sort_by(.id)'
    shift 3
    job_root "$root" "$config" "$id"
    alternation regex "$@"
    mkdir -p "$root/tests"
    for caller in "$@"; do tests[${test_file_of[$caller]}]=1; done
    FILTER=$regex yq 'select(.id | test(strenv(FILTER)))' "${!tests[@]}" >"$root/tests/cases.yml"
    for caller in "$@"; do
        ((${count["fixed,$caller"]:-0})) || continue
        snapshots+=("$root/snapshots/$caller-snapshot.yml")
        originals+=("${snapshot_file_of[$caller]}")
    done
    if ((${#originals[@]})); then
        expected=${ yq -N -o=json '.' "${originals[@]}" | jq -Scs "$fixes";}
    fi
    while IFS=$'\t' read -r op p mutation; do
        printf '%s' "$mutation" >"$root/${file_of[$id]}" # JSON is YAML the loader reads, no yq run per arm
        rc=0
        out=$(ast-grep test -c "$root/sgconfig.yml" -t "$root/tests" --include-off --skip-snapshot-tests --filter "$regex" --color never 2>&1) || rc=$?
        case $rc in
            0) ;;
            4) continue ;;
            8)
                printf 'invalid mutation: %s %s %s, excluded from coverage\n%s\n' "$id" "$op" "$p" "$out" >>"$root/invalid-mutations.log"
                continue
                ;;
            *)
                finding "mutation run failed: $id $op $p exit $rc"$'\n'"$out"
                continue
                ;;
        esac
        hits_of got "$root" "$config" "$@"
        moved=
        for caller in "$@"; do [[ ${got[$caller]} == "${base_hits[$caller]}" ]] || moved=1; done
        [[ -z $moved ]] || continue
        if ((${#snapshots[@]})); then
            # Separate test documents from snapshots, which -t would otherwise load as empty tests
            out=$(ast-grep test -c "$root/sgconfig.yml" -t "$root/tests" --snapshot-dir "$root/snapshots" \
                --include-off --filter "$regex" --update-all --color never 2>&1) || {
                finding "mutation fix comparison failed: $id $op $p"$'\n'"$out"
                continue
            }
            actual=${ yq -N -o=json '.' "${snapshots[@]}" | jq -Scs "$fixes";}
            [[ $actual == "$expected" ]] || moved=1
        fi
        [[ -n $moved ]] || finding "uncovered arm: $id $op $p"
    done <<<"${block%$'\n'}"
}

# One job reaped by wait -n, a nonzero exit is a finding: a job that dies under -e prints nothing, and its arms stay unproven
declare -A job_of=()
reap() {
    local rc=0 pid
    wait -n -p pid "${!job_of[@]}" || rc=$?
    ((rc == 0)) || finding "job died, arms unproven: ${job_of[$pid]} exit $rc"
    unset 'job_of[$pid]'
}

check_arms() {
    local id list caller n=0 k out limit chunk=8 lines block in_scope d units files
    limit=$(getconf _NPROCESSORS_ONLN)
    mkdir -p "$scratch/jobs"
    for d in "${!lang_dirs[@]}"; do
        units=("$d"/*/) files=("$d"/*.yml)
        if ((${#units[@]} == 0 || ${#files[@]})); then units=("$d/"); fi
        if [[ ${dir_role[${d%/*}]} == util ]]; then job_utils+=("${units[@]%/}"); else job_rules+=("${units[@]%/}"); fi
    done
    job_config=$(RULES=$(jq -cn --args '$ARGS.positional' "${job_rules[@]}") \
    UTILS=$(jq -cn --args '$ARGS.positional' "${job_utils[@]}") ROOT=$PWD yq '.ruleDirs = (env(RULES) | map("@JOB@/" + .))
      | .utilDirs = (env(UTILS) | map("@JOB@/" + .)) | (.testConfigs[].testDir | select(test("^/") | not)) |= strenv(ROOT) + "/" + .
      | ((.customLanguages // {})[].libraryPath | .. | select(tag == "!!str") | select(test("^/") | not)) |= strenv(ROOT) + "/" + .' sgconfig.yml)
    for id in "${util_ids[@]}"; do
        [[ ${lang_of[$id]} == "$lang" ]] || continue
        [[ -z $filter ]] || continue # A narrowed run reads the callers of its own ids alone
        [[ -v callers[$id] ]] || {
            finding "no rule calls util: $id"
            continue
        }
        [[ -v parameterized[$id] || ${callers[$id]} == *" "*" "* ]] || finding "one rule calls util: $id"
    done
    for id in "${owned[@]}" "${util_ids[@]}"; do
        [[ ${lang_of[$id]} == "$lang" && -v arms_of[$id] ]] || continue
        list=() in_scope=0
        for caller in ${callers[$id]-}; do
            [[ ! -v scoped[$caller] ]] || in_scope=1
            [[ -v failed[$caller] || ! -v base_hits[$caller] ]] || list+=("$caller") # FAIL callers read every arm as covered
        done
        ((in_scope && ${#list[@]})) || continue # A util's arms are proven against every caller, once a scoped rule reaches it
        mapfile -t lines <<<"${arms_of[$id]%$'\n'}"
        for ((k = 0; k < ${#lines[@]}; k += chunk)); do
            printf -v block '%s\n' "${lines[@]:k:chunk}"
            ((${#job_of[@]} < limit)) || reap
            ((++n))                                                                    # One job per processor, the next batch starts as any job ends
            cover_arms "$n" "$id" "$block" "${list[@]}" >"$scratch/jobs/$n.out" 2>&1 & # Jobs read stdin from /dev/null, no step blocks
            job_of[$!]="$id job $n"
        done
    done
    while ((${#job_of[@]})); do reap; done
    for out in "$scratch"/jobs/*.out; do [[ ! -s $out ]] || finding "$(<"$out")"; done
    for out in "$scratch"/jobs/*/invalid-mutations.log; do printf '%s\n' "$(<"$out")"; done
}

check_parse() {
    local id key kind c out leaf file paths=() inputs=() outputs=()
    local -A labels=() roots=()
    ((${#scoped[@]})) || return 0 # No path operand: ast-grep run reads the working directory
    for key in "${!case_path[@]}"; do
        IFS=',' read -r kind id c <<<"$key"
        [[ -v scoped[$id] ]] || continue
        labels[${case_path[$key]}]="$kind $id case $c"
        [[ $kind != fixed || ! -v expanded[$id] ]] || continue
        [[ $kind == fixed || ${text[$key]} == *[![:space:]]* ]] || finding "empty $kind case: $id case $c"
        paths+=("${case_path[$key]}")
    done
    for id in "${!scoped[@]}"; do
        [[ -v expanded[$id] ]] || continue
        ((${count["fixed,$id"]:-0})) || continue
        leaf=${rule_leaf[$id]} inputs=() outputs=()
        for ((c = 1; c <= ${count["invalid,$id"]}; c++)); do
            inputs+=("${case_path["invalid,$id,$c"]}") outputs+=("${case_path["fixed,$id,$c"]}")
        done
        paths+=("${outputs[@]}")
        # Snapshot text omits expanded ranges; preserve originals and parse the source the CLI writes
        if [[ $leaf == *@N@* ]]; then
            for ((c = 0; c < ${#inputs[@]}; c++)); do
                cp "${inputs[c]}" "${outputs[c]}"
                labels[${outputs[c]}]=${labels[${inputs[c]}]}
                labels[${inputs[c]}]="fixed $id case $((c + 1))"
            done
            out=${ ast-grep scan -c "$cases/sgconfig.yml" --filter "^$id\$" --error="$id" --threads 1 \
                --no-ignore hidden -U "${inputs[@]}" 2>&1;} || finding "$out"
            continue
        fi
        for ((c = 0; c < ${#inputs[@]}; c++)); do
            cp "${inputs[c]}" "$cases/$leaf"
            out=${ ast-grep scan -c "$cases/sgconfig.yml" --filter "^$id\$" --error="$id" --threads 1 \
                --no-ignore hidden -U "$cases/$leaf" 2>&1;} || finding "$out"
            mv "$cases/$leaf" "${outputs[c]}"
        done
    done
    case $lang in
        bash)
            for file in "${paths[@]}"; do
                out=${ bash -n "$file" 2>&1;} || finding "Bash syntax error in ${labels[$file]}"$'\n'"$out"
            done
            ;;
        *)
            # Scan case directories once; one argument per fixture can exceed the system argument limit
            for file in "${paths[@]}"; do roots[${file%/*/*}]=1; done
            ((${#roots[@]} == 0)) || { ast-grep run -c "$cases/sgconfig.yml" -k ERROR -l "$lang" --no-ignore hidden \
                --json=stream "${!roots[@]}" || [[ $? == 1 ]]; } |
                jq -js 'map(.file) | unique[] | . + "\u0000"' |
                while IFS= read -r -d '' file; do finding "ERROR node in ${labels[$file]}"; done
            ;;
    esac
}

measure() {
    local found hits
    [[ -v elements[$lang] ]] || {
        printf 'no measure for %s\n' "$lang"
        exit 1
    }
    # run exits 1 over no match, a capture under -e ends the script at zero elements, the stream holds one line per match
    { ast-grep run -k "${elements[$lang]}" -l "$lang" --json=stream "${@:3}" || [[ $? == 1 ]]; } | mapfile -t found
    scan_matches --filter "^${nesting[$lang]}\$" "${@:3}" | mapfile -t hits
    printf 'elements %s nesting %s\n' "${#found[@]}" "${#hits[@]}"
}

# --- [ENTRY] ----------------------------------------------------------------------------

[[ $command == pairing ]] || owning_language
if [[ $command == measure ]]; then
    measure "$@"
    exit 0
fi
mutations=false
[[ $command != arms && $command != gate ]] || mutations=true
read_facts
[[ $command != parse && $command != width ]] || owned=("${!scoped[@]}")
[[ $command == pairing ]] || ((${#scoped[@]})) || finding "no rule reports under .$ext"
case $command in
    pairing) check_pairing ;;
    parse)
        write_cases
        check_parse
        ;;
    width)
        write_cases
        hits_of base_hits "$cases" "$cases/sgconfig.yml" "${owned[@]}"
        check_width
        ;;
    arms)
        write_cases
        run_test
        hits_of base_hits "$cases" "$cases/sgconfig.yml" "${owned[@]}"
        check_arms
        ;;
    gate)
        write_cases
        run_test
        check_pairing
        hits_of base_hits "$cases" "$cases/sgconfig.yml" "${owned[@]}"
        check_width
        check_arms
        check_parse
        ;;
    *) ;;
esac
exit "$findings"
