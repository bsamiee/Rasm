#!/usr/bin/env bash
# Proves the rules tree sgconfig.yml names: ids paired, every invalid case reported once, arms covered by mutation, cases and fixes parsed
# Usage: rule-checks.sh <command> [<ext> [<id-regex>]], from the directory holding sgconfig.yml, one line per finding and exit 1 on any
# shellcheck disable=SC2250,SC2312  # Unbraced names read as the code, and a capture is read for its output alone
set -euo pipefail
shopt -s globstar nullglob dotglob

# --- [ARGUMENTS] ------------------------------------------------------------------------

declare -A describe=(
    [pairing]='Pairs rules, tests, and snapshots by id over the whole tree, file stems, id case, severity per directory, test and snapshot keys'
    [width]='<ext> [<id-regex>]  Scans each invalid case alone, width <id> case <n>: <hits> hits, past one a once-reporting gap, zero a missed glob'
    [arms]='<ext> [<id-regex>]  Deletes each arm, uncovered arm, unchecked arm, no rule calls util, one rule calls util, no kind at util root'
    [parse]='<ext> [<id-regex>]  Parses each invalid, valid, and fixed case, ERROR node in <kind> <id> case <n>'
    [gate]='<ext> [<id-regex>]  ast-grep test over the ids, then pairing, width, arms, and parse, the proof before a rule lands'
    [measure]='<ext> <path>...  Prints elements <n> nesting <n>, the top-level element count and the callback-depth hits over the paths'
)
usage() {
    local name
    printf 'usage: rule-checks.sh <command> [<ext> [<id-regex>]], from the directory holding sgconfig.yml, one line per finding and exit 1 on any\n'
    for name in pairing width arms parse gate measure; do printf '%-8s %s\n' "$name" "${describe[$name]}"; done
}
case ${1-} in
    pairing) (($# == 1)) ;;
    width | arms | parse | gate) (($# == 2 || $# == 3)) ;;
    measure) (($# >= 3)) ;;
    *) false ;;
esac || {
    usage
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

# Rows per rule or util as role, id, file, language, glob, leaf, severity, and a kind at the root, then per file of the language
# an arm a case can fail as op, id, path, and the mutated file as JSON, and a calls row per util read, rewriters hold arms and calls too
# The leaf is the case path under the config root: a glob with ** or an implied **/ prefix takes <id>/@N@ per case, an anchored
# name stays fixed and its cases scan one at a time, and a leaf with no extension takes <id>.<ext> as its file
# shellcheck disable=SC2016  # The $ names are jq variables
facts='.id as $id | (.files[0] // "*.\($ext)") as $glob | ($glob | ltrimstr("**/")) as $rest
  | (if $rest | test("\\*\\*") then $rest | gsub("\\*\\*"; "\($id)/@N@") | gsub("\\*"; $id)
     elif $glob | test("^(\\*\\*/|\\*)") then "\($id)/@N@/\($rest | gsub("\\*"; $id))" else $rest end) as $leaf
  | (if $leaf | test("\\.[^/]*$") then $leaf else "\($leaf)/\($id).\($ext)" end) as $leaf
  | "\(.role)\t\($id)\t\(.filename)\t\(.language)\t\($glob)\t\($leaf)\t\(.severity // "hint")\t\(.rule | has("kind") or has("any"))",
  (select((.language // "" | ascii_downcase) == $lang) | del(.role, .filename) as $full | {rule, utils, constraints, rewriters} | . as $doc
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
    | "\(.op)\t\($id)\t\($p | tojson)\t\(if .op == "blank" then $full | setpath($p; "") else $full | delpaths([$p]) end | tojson)"),
  ([.. | objects | (.matches? // empty) | (strings, (objects | keys[]))] | unique[] | "calls\t\($id)\t\(.)"))'

# Element selector and callback-depth rule per language, the two counts a fix is measured by
declare -A elements=(
    [tsx]=':is(program, export_statement) > :is(lexical_declaration, type_alias_declaration, interface_declaration, class_declaration, enum_declaration)'
    [python]='module > :is(function_definition, class_definition, type_alias_statement), module > expression_statement > assignment'
)
declare -A nesting=([tsx]=no-fourth-callback-level [python]=no-fourth-lambda-level)

# --- [TREE] -----------------------------------------------------------------------------

dirs=() rule_dirs=() util_dirs=() rule_files=() util_files=()
while IFS=$'\t' read -r role d; do
    d=${d%/}
    dirs+=("$d")
    case $role in
        rule) rule_dirs+=("$d") rule_files+=("$d"/**/*.yml) ;;
        util) rule_dirs+=("$d") util_dirs+=("$d") util_files+=("$d"/**/*.yml) ;;
        test) tests=$d ;;
        *) ;;
    esac
done < <(yq -r '(.ruleDirs[] | "rule\t" + .), ((.utilDirs // [])[] | "util\t" + .), ("test\t" + .testConfigs[0].testDir),
  ((.customLanguages // {})[].libraryPath | "library\t" + .)' sgconfig.yml)
scratch=$(mktemp -d)
trap 'rm -rf "$scratch"' EXIT
cases=$scratch/cases
lang=
job_rules=() job_utils=() # The language's package directories, or a language directory holding a rule file or no directory
job_config=               # sgconfig.yml over them under @JOB@, the test directory and library absolute, one text per job

# A root holding sgconfig.yml over every directory it names, each linked
link_tree() {
    local root=$1 d
    mkdir -p "${dirs[@]/#/$root/}" # The parents, then each leaf directory gives way to its link
    rmdir "${dirs[@]/#/$root/}"
    for d in "${dirs[@]}"; do ln -s "$PWD/$d" "$root/$d"; done
    cp sgconfig.yml "$root/sgconfig.yml"
}

# A root for one mutation batch over the id's file: the language's package directories linked and the one holding the file
# copied, each a rule or util directory of the job config, written at the root for the anchored cases and at the cases root
# for the rest, a files glob matches no case reached through a linked path and the shared cases copy into no job
job_root() {
    local root=$1 config=$2 id=$3 d text parents=("${job_rules[@]}" "${job_utils[@]}")
    parents=("${parents[@]/#/$root/}")
    mkdir -p "${parents[@]%/*}"
    for d in "${job_rules[@]}" "${job_utils[@]}"; do
        if [[ ${file_of[$id]} == "$d"/* ]]; then cp -R "$PWD/$d" "$root/$d"; else ln -s "$PWD/$d" "$root/$d"; fi
    done
    text=${job_config//@JOB@/$root}
    printf '%s' "$text" >"$root/sgconfig.yml"
    printf '%s' "$text" >"$config"
}

# The language sgconfig.yml maps the extension to, read from the file entity of an empty probe file
owning_language() {
    local probe rc=0
    link_tree "$cases"
    printf '\n' >"$cases/probe.$ext"
    probe=$(ast-grep scan -c "$cases/sgconfig.yml" --inspect entity "$cases/probe.$ext" 2>&1) || rc=$?
    [[ $probe =~ language=([^,]*) ]] || {
        # A tree the loader refuses prints its own lines, a loaded tree with no owner prints the extension
        if ((rc)); then grep -v '^sg:' <<<"$probe"; else printf 'no language owns .%s\n' "$ext"; fi
        exit 1
    }
    lang=${BASH_REMATCH[1],,}
}

# --- [FACTS] ----------------------------------------------------------------------------

declare -A file_of lang_of severity_of rooted lower rule_leaf count text snapshot_key arms_of calls callers base_hits lang_dirs failed
declare -A scoped=() # The one associative array counted, a count over an array with no assignment is unbound under -u
rule_ids=() util_ids=() test_ids=() snapshot_ids=() test_files=() snapshot_files=() unreadable=() lang_rules=() owned=() f=()

read_facts() {
    local t role id a b c d e g i v n k m
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
                if [[ $role == rule ]]; then rule_ids+=("$id") rule_leaf[$id]=$d; else util_ids+=("$id"); fi
                file_of[$id]=$a lang_of[$id]=${b,,} severity_of[$id]=$e rooted[$id]=$g
                [[ -v lower[${id,,}] ]] || lower[${id,,}]=$id # Earliest id per lowercase spelling, a later one differs by case alone
                ;;
        esac
    done < <({ # No file operand: yq reads stdin, blocked on a terminal and usage text on /dev/null
        ((${#rule_files[@]} == 0)) || yq -N -o=json -I=0 '.role = "rule" | .filename = filename' "${rule_files[@]}"
        ((${#util_files[@]} == 0)) || yq -N -o=json -I=0 '.role = "util" | .filename = filename' "${util_files[@]}"
    } | jq -r --arg lang "$lang" --arg ext "$ext" "$facts")

    # -N: a --- between files shifts every later field, -0 -r keeps empty fields, and a scalar side counts its characters and
    # iterates none, so a side reads as a list or as empty
    if ((${#test_files[@]})); then
        mapfile -d '' -t f < <(yq -N -0 -r '[.id, (keys | join(" ")), ((.valid | select(tag == "!!seq")) // [] | length),
          ((.invalid | select(tag == "!!seq")) // [] | length)][], ((.valid | select(tag == "!!seq")) // [])[],
          ((.invalid | select(tag == "!!seq")) // [])[]' "${test_files[@]}")
    fi
    for ((i = 0; i < ${#f[@]}; i += 4 + v + n)); do
        id=${f[i]} v=${f[i + 2]} n=${f[i + 3]}
        test_ids+=("$id")
        text["keys,$id"]=${f[i + 1]} count["valid,$id"]=$v count["invalid,$id"]=$n
        for ((c = 1; c <= v; c++)); do text["valid,$id,$c"]=${f[i + 3 + c]}; done
        for ((c = 1; c <= n; c++)); do text["invalid,$id,$c"]=${f[i + 3 + v + c]}; done
    done
    f=()
    if ((${#snapshot_files[@]})); then
        mapfile -d '' -t f < <(yq -N -0 -r '[.id, (.snapshots // {} | length),
          ([.snapshots // {} | .[] | select(has("fixed"))] | length)][], (.snapshots // {} | keys | .[]),
          (.snapshots // {} | .[] | select(has("fixed")) | .fixed)' "${snapshot_files[@]}")
    fi
    for ((i = 0; i < ${#f[@]}; i += 3 + k + m)); do
        id=${f[i]} k=${f[i + 1]} m=${f[i + 2]}
        snapshot_ids+=("$id")
        count["keys,$id"]=$k count["fixed,$id"]=$m
        for ((c = 1; c <= k; c++)); do snapshot_key["$id,${f[i + 2 + c]}"]=1; done
        for ((c = 1; c <= m; c++)); do text["fixed,$id,$c"]=${f[i + 2 + k + c]}; done
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
# the last three segments of every path read <id>/<n>/<file>
write_cases() {
    local id kind c leaf file d
    local -A path
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
            if [[ $leaf == *@N@* ]]; then path["invalid,$id,$c"]=$cases/${leaf//@N@/$c}; else path["invalid,$id,$c"]=$cases/anchored/$id/$c/${leaf##*/}; fi
        done
        for kind in valid fixed; do
            for ((c = 1; c <= ${count["$kind,$id"]:-0}; c++)); do path["$kind,$id,$c"]=$cases/$kind/$id/$c/${leaf##*/}; done
        done
    done
    mkdir -p "$cases"/{valid,fixed,anchored} "${path[@]%/*}" # One process for every directory, a run per file costs seconds
    for file in "${!path[@]}"; do printf '%s' "${text[$file]}" >"${path[$file]}"; done
}

# Hits per invalid case into the named associative array, one scan under the cases-root config over the case directories of
# every batched id and one per anchored case at its leaf under the root
# shellcheck disable=SC2034,SC2004  # The nameref writes the caller's array, and its subscript is a string
hits_of() {
    local -n found=$1
    local root=$2 config=$3 id c h key leaf file lines regex dirs=() ids=() errors=() anchored=() files=()
    local -A hits
    for id in "${@:4}"; do
        leaf=${rule_leaf[$id]}
        for ((c = 1; c <= ${count["invalid,$id"]}; c++)); do hits["$id,$c"]=0; done
        # --error=<id> loads a rewrite the scan skips at off
        if [[ $leaf == *@N@* ]]; then dirs+=("$cases/${leaf%%/@N@*}") ids+=("$id") errors+=(--error="$id"); else anchored+=("$id") files+=("$root/$leaf"); fi
    done
    if ((${#anchored[@]})); then
        mkdir -p "${files[@]%/*}"
        for id in "${anchored[@]}"; do
            leaf=${rule_leaf[$id]} file=$root/$leaf
            for ((c = 1; c <= ${count["invalid,$id"]}; c++)); do
                ln -f "$cases/anchored/$id/$c/${leaf##*/}" "$file"
                # ast-grep-ignore: no-spawn-per-loop-item, one root path serves one anchored case
                mapfile -t lines < <(ast-grep scan -c "$root/sgconfig.yml" --filter "^$id\$" --error="$id" --no-ignore hidden --json=stream "$file" 2>/dev/null)
                hits["$id,$c"]=${#lines[@]}
            done
        done
        rm -f "${files[@]}"
    fi
    if ((${#dirs[@]})); then
        alternation regex "${ids[@]}"
        while IFS= read -r key; do ((++hits["$key"])); done < <(ast-grep scan -c "$config" --filter "$regex" "${errors[@]}" \
            --no-ignore hidden --json=stream "${dirs[@]}" 2>/dev/null |
            jq -r '(.file | split("/")) as $p | select($p[-3] == .ruleId) | "\(.ruleId),\($p[-2])"')
    fi
    for id in "${@:4}"; do
        h=
        for ((c = 1; c <= ${count["invalid,$id"]}; c++)); do h+=" ${hits["$id,$c"]}"; done
        found[$id]=${h# }
    done
}

# One test run over the language's rules, or the ids the regex names, its own lines printed on a nonzero exit, none over no rule
run_test() {
    local out rc=0 line regex=$filter
    [[ -n $regex ]] || ((${#lang_rules[@]})) || return 0
    [[ -n $regex ]] || alternation regex "${lang_rules[@]}"
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

# Arm coverage over one batch of arms in one root: test exit 4 is a failed case, exit 0 compares the hit counts, another exit is
# a mutant the loader refuses, a local util of one key or a capture bound in one arm
cover_arms() {
    local root=$scratch/jobs/$1 config=$cases/job-$1.yml id=$2 block=$3 moved rc regex caller op p mutation
    local -A got
    shift 3
    job_root "$root" "$config" "$id"
    alternation regex "$@"
    while IFS=$'\t' read -r op p mutation; do
        printf '%s' "$mutation" >"$root/${file_of[$id]}" # JSON is YAML the loader reads, no yq run per arm
        rc=0
        ast-grep test -c "$root/sgconfig.yml" --include-off --filter "$regex" --color never >/dev/null 2>&1 || rc=$?
        case $rc in
            4) ;;
            0)
                hits_of got "$root" "$config" "$@"
                moved=
                for caller in "$@"; do [[ ${got[$caller]} == "${base_hits[$caller]}" ]] || moved=1; done
                [[ -n $moved ]] || finding "uncovered arm: $id $op $p"
                ;;
            *) finding "unchecked arm: $id $op $p exit $rc" ;;
        esac
    done <<<"${block%$'\n'}"
}

# One job reaped by wait -n, a nonzero exit is a finding: a job that dies under -e prints nothing, and its arms stay unproven
declare -A job_of
reap() {
    local rc=0 pid
    wait -n -p pid || rc=$?
    ((rc == 0)) || finding "job died, arms unproven: ${job_of[$pid]} exit $rc"
}

check_arms() {
    local id list caller n=0 k out limit chunk=8 lines block in_scope d units files
    limit=$(getconf _NPROCESSORS_ONLN)
    mkdir -p "$scratch/jobs"
    for d in "${!lang_dirs[@]}"; do
        units=("$d"/*/) files=("$d"/*.yml)
        if ((${#units[@]} == 0 || ${#files[@]})); then units=("$d/"); fi
        if [[ " ${util_dirs[*]} " == *" ${d%/*} "* ]]; then job_utils+=("${units[@]%/}"); else job_rules+=("${units[@]%/}"); fi
    done
    job_config=$(RULES="${job_rules[*]}" UTILS="${job_utils[*]}" ROOT=$PWD yq '.ruleDirs = (strenv(RULES) | split(" ") | map(select(. != "") | "@JOB@/" + .))
      | .utilDirs = (strenv(UTILS) | split(" ") | map(select(. != "") | "@JOB@/" + .)) | .testConfigs[0].testDir |= strenv(ROOT) + "/" + .
      | (.customLanguages // {})[].libraryPath |= strenv(ROOT) + "/" + .' sgconfig.yml)
    for id in "${util_ids[@]}"; do
        [[ ${lang_of[$id]} == "$lang" ]] || continue
        [[ ${rooted[$id]} == true ]] || finding "no kind at util root: $id" # Kind-less utils walk in quadratic time
        [[ -z $filter ]] || continue                                        # A narrowed run reads the callers of its own ids alone
        [[ -v callers[$id] ]] || {
            finding "no rule calls util: $id"
            continue
        }
        [[ ${callers[$id]} == *" "*" "* ]] || finding "one rule calls util: $id"
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
            ((n++ < limit)) || reap                                                    # One job per processor, the next batch starts as any job ends
            cover_arms "$n" "$id" "$block" "${list[@]}" >"$scratch/jobs/$n.out" 2>&1 & # Jobs read stdin from /dev/null, no step blocks
            job_of[$!]="$id job $n"
        done
    done
    for ((k = 0; k < n && k < limit; k++)); do reap; done # The jobs still running, the earlier ones reaped as each batch started
    for out in "$scratch"/jobs/*.out; do [[ ! -s $out ]] || finding "$(<"$out")"; done
}

check_parse() {
    local line id paths=()
    ((${#scoped[@]})) || return 0 # No path operand: ast-grep run reads the working directory
    for id in "${!scoped[@]}"; do
        paths+=("$cases"/{valid,fixed,anchored}/"$id")
        [[ ${rule_leaf[$id]} != *@N@* ]] || paths+=("$cases/${rule_leaf[$id]%%/@N@*}")
    done
    # -l keeps an injected region of a host language out of the parse, a yaml run step reads as yaml and not as bash
    while IFS= read -r line; do finding "$line"; done < <(ast-grep run -c "$cases/sgconfig.yml" -k ERROR -l "$lang" --no-ignore hidden \
        --json=stream "${paths[@]}" 2>/dev/null |
        jq -rs --arg s "$cases/" '[.[].file | ltrimstr($s) | split("/")
          | "ERROR node in \(if .[0] == "valid" or .[0] == "fixed" then .[0] else "invalid" end) \(.[-3]) case \(.[-2])"] | unique[]')
}

measure() {
    local found hits
    [[ -v elements[$lang] ]] || {
        printf 'no measure for %s\n' "$lang"
        exit 1
    }
    # run exits 1 over no match, a capture under -e ends the script at zero elements, the stream holds one line per match
    mapfile -t found < <(ast-grep run -k "${elements[$lang]}" -l "$lang" --json=stream "${@:3}")
    mapfile -t hits < <(ast-grep scan --filter "^${nesting[$lang]}\$" --json=stream "${@:3}")
    printf 'elements %s nesting %s\n' "${#found[@]}" "${#hits[@]}"
}

# --- [ENTRY] ----------------------------------------------------------------------------

[[ $command == pairing ]] || owning_language
read_facts
[[ $command == pairing || $command == measure ]] || ((${#scoped[@]})) || finding "no rule reports under .$ext"
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
    measure) measure "$@" ;;
    *) ;;
esac
exit "$findings"
