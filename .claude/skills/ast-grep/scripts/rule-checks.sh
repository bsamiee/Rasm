#!/usr/bin/env bash
# Checks configured rule registration, fixture matches, mutation coverage, and syntax
# shellcheck disable=SC2250,SC2312  # Unbraced names read as the code, and a capture is read for its output alone
set -euo pipefail
shopt -s globstar nullglob dotglob lastpipe

# --- [ARGUMENTS] ------------------------------------------------------------------------

case ${1-} in
    pairing) (($# == 1)) ;;
    width | arms | parse) (($# == 2 || $# == 3)) ;;
    measure) (($# >= 3)) ;;
    *) false ;;
esac || {
    printf '%s\n' 'usage: rule-checks.sh pairing | width <ext> [<id-regex>] | arms <ext> [<id-regex>] | parse <ext> [<id-regex>] | measure <ts|py> <path>...'
    exit 1
}
command=$1 filter=${3-} ext=${2-} ext=${ext#.}
findings=0
finding() {
    printf '%s\n' "$*"
    findings=1
}
tmp=$(mktemp -d)
trap 'rm -rf "$tmp"' EXIT
cases=$tmp/cases
# Facts per id as field,id, path and text per case label, base hits, FAIL ids, running jobs by pid
declare -A rule=() case_path=() text=() hits=() failed=() job_of=()
owned=() test_filter='' lang=''

# --- [PROGRAMS] -------------------------------------------------------------------------

# jq programs over the documents in one assignment, the one directive covers every `$` name jq owns
# roles: role and directory of the deepest configured directory holding the file, id and language as the loader reads them
# pairing: rules paired with tests and snapshots by id over the whole tree, file stems, severity, fixture keys, and case sets
# reach: utils a rule reaches through matches, and the rules reaching a util
# facts: language rules with invalid cases as field,id pairs, case texts at paths their files: globs match, the test filter and util findings under arms
# jobs: one record per batch of eight arms of a rule or util a scoped rule reaches, callers reduced to the passing rules with cases
# shellcheck disable=SC2016
roles='. as $doc | [{role: "rule", dir: $config.ruleDirs[]}, {role: "util", dir: $config.utilDirs[]}, {role: "test", dir: $config.testConfigs[].testDir}, {role: "snapshot", dir: $config.testConfigs[].snapshotDir}]
  | map(select(.dir as $dir | $doc.file | startswith("\($dir)/"))) | max_by(.dir | length)
  | . + $doc + {id: ($doc.doc.id // "" | tostring), language: ($doc.doc.language // "" | tostring | ascii_downcase)}' \
    pairing='def cases($side): [.[$side] | arrays[] | strings];
  [inputs] as $docs | ($docs | map(select(.role == "rule" or .role == "util"))) as $rules | ($rules | INDEX(.id)) as $rule_of
  | ($rules | group_by(.id | ascii_downcase) | map(.[0]) | INDEX(.id | ascii_downcase)) as $first_of
  | ($docs | map(select(.role == "test")) | group_by(.id)) as $tests | ($tests | map(.[0]) | INDEX(.id)) as $test_of
  | ($docs | map(select(.role == "snapshot")) | INDEX(.id)) as $snapshot_of
  | ($rules[] | .id as $id | (select(.file | endswith("/\($id).yml") | not) | "id differs from file stem: \(.file)"),
      (select($first_of[$id | ascii_downcase].id != $id) | "ids differ by case alone: \($first_of[$id | ascii_downcase].id) \($id)"),
      (select(.role == "rule") | (.doc.severity // "hint" | tostring) as $severity
        | if "/\(.file)" | contains("/rewrites/") then select($severity != "off") | "severity \($severity) under rewrites: \($id)"
          else select($severity != "error") | "severity \($severity) under rules: \($id)" end),
      (select(.role == "rule" and $test_of[$id] == null) | "no test: \($id)")),
    ($tests[] | .[0].id as $id | (.[1:][] | "duplicate test id: \($id)"),
      (.[].doc | ([("valid", "invalid") as $side | .[$side] | select(type != "array" or any(.[]?; type != "string")) | "non-string case or non-array side: \($id) \($side)"]
        + [cases("valid") + cases("invalid") | group_by(.)[] | select(length > 1) | "duplicate or contradictory case: \($id)"] | unique[]))),
    ($test_of[] | .id as $id | .doc as $doc
      | (select(($rule_of[$id].role // "") != "rule") | "no rule: \($id)"),
        ($doc | objects | keys[] | select(IN("id", "valid", "invalid") | not) | "unknown key in \($id): \(.)"),
        (select(($doc | cases("valid")) == [] or ($doc | cases("invalid")) == []) | "one side empty: \($id)"),
        (if $snapshot_of[$id] == null then "no snapshot: \($id)"
         else ($snapshot_of[$id].doc.snapshots // {}) as $keys | ($doc | cases("invalid")) as $invalid | ([$invalid[] | select(in($keys))] | length) as $n
           | select($n != ($invalid | length) or $n != ($keys | length)) | "orphan or missing snapshot key: \($id)" end)),
    ($snapshot_of[] | select($test_of[.id] == null) | "orphan snapshot: \(.id)")' \
    reach='def calls: [.. | objects | (.matches? // empty) | (strings, (objects | keys[]))] | unique;
  def closure($rule_of; $ids): ([$ids[], ($ids[] | $rule_of[.].doc // {} | calls[])] | unique) as $next | if $next == $ids then $ids else closure($rule_of; $next) end;
  def reach($rules): ($rules | INDEX(.id)) as $rule_of | $rules | map(select(.role == "rule") | {key: .id, value: closure($rule_of; [.id])}) | from_entries;
  def callers($reach; $id): [$reach | to_entries[] | select(.value | index($id)) | .key];' \
    facts='def leaf($id): (.files[0] // "*.\($ext)") as $glob | ($glob | ltrimstr("**/")) as $rest
    | (if $rest | test("\\*\\*") then $rest | gsub("\\*\\*"; "\($id)/@N@") | gsub("\\*"; $id)
       elif $glob | test("^(\\*\\*/|\\*)") then "\($id)/@N@/\($rest | gsub("\\*"; $id))" else $rest end)
    | (if test("\\.\\{[[:alnum:]_-]+(,[[:alnum:]_-]+)+\\}$") then
         capture("^(?<stem>.*)\\.\\{(?<extensions>[^{}]+)\\}$") | (.extensions | split(",")) as $extensions
         | "\(.stem).\(if $extensions | index($ext) then $ext else $extensions[0] end)" else . end)
    | (if test("\\.[^/]*$") then . else "\(.)/\($id).\($ext)" end);
  [inputs] as $docs | ($docs | map(select(.language == $lang and (.role == "rule" or .role == "util")))) as $rules
  | ($docs | map(select(.role == "test")) | group_by(.id) | map(.[0]) | INDEX(.id)) as $test_of
  | ($docs | map(select(.role == "snapshot")) | INDEX(.id)) as $snapshot_of
  | def cases($id; $side): [$test_of[$id].doc[$side] | arrays[] | strings];
  [$rules[] | select(.role == "rule" and cases(.id; "invalid") != [])] as $with_cases
  | [$with_cases[] | select($filter == "" or (.id | test($filter)))] as $scoped
  | (if $command == "arms" then $with_cases else $scoped end) as $owned
  | ("config", ($config | .ruleDirs = [$rules[] | select(.role == "rule") | .file] | .utilDirs = [$rules[] | select(.role == "util") | .file] | tojson)),
    (select($scoped == []) | "finding", "no rule reports under .\($ext)"),
    (select($command == "arms") | reach($rules) as $reach
      | ("tested", ([$rules[] | select(.role == "rule") | .id | select($filter == "" or test($filter))] + [$scoped[] | $reach[.id][] | callers($reach; .)[]] | unique
          | if . == [] then "" else "^(\(join("|")))$" end)),
        ($rules[] | select(.role == "util" and $filter == "") | callers($reach; .id) as $callers
          | if $callers == [] then "finding", "no rule calls util: \(.id)"
            elif (.doc.arguments // []) == [] and ($callers | length) < 2 then "finding", "one rule calls util: \(.id)" else empty end)),
    ($owned[] | .id as $id | (.doc | leaf($id)) as $leaf | ($leaf | test("@N@")) as $glob | ($leaf | split("/")[-1]) as $name
      | [$snapshot_of[$id].doc.snapshots // {} | .[] | select(has("fixed")) | .fixed] as $fixed
      | ([.doc | .. | objects | has("expandStart") or has("expandEnd")] | any) as $expanded
      | ("target,\($id)", (if $glob then "\($cases)/\($leaf | split("/@N@")[0])" else $leaf end)),
        ("anchored,\($id)", (if $glob then 0 else 1 end)), ("expanded,\($id)", (if $expanded then 1 else 0 end)),
        ("invalid,\($id)", (cases($id; "invalid") | length)), ("fixed,\($id)", ($fixed | length)),
        ("test,\($id)", ($test_of[$id].doc | tojson)), ("snapshot,\($id)", (if $fixed == [] then "" else $snapshot_of[$id].file end)),
        (cases($id; "invalid") | to_entries[] | (.key + 1) as $n | "invalid \($id) case \($n)" as $label
          | ("path,\($label)", (if $glob then "\($cases)/\($leaf | gsub("@N@"; "\($n)"))" else "\($cases)/anchored/\($id)/\($n)/\($name)" end)), ("text,\($label)", .value)),
        (cases($id; "valid") | to_entries[] | "valid \($id) case \(.key + 1)" as $label | ("path,\($label)", "\($cases)/valid/\($id)/\(.key + 1)/\($name)"), ("text,\($label)", .value)),
        (if $expanded then (select($fixed != []) | range(cases($id; "invalid") | length) | ("path,fixed \($id) case \(. + 1)", "\($cases)/fixed/\($id)/\(. + 1)/\($name)"))
         else ($fixed | to_entries[] | "fixed \($id) case \(.key + 1)" as $label | ("path,\($label)", "\($cases)/fixed/\($id)/\(.key + 1)/\($name)"), ("text,\($label)", .value)) end))' \
    jobs='def chunks($n): range(0; length; $n) as $i | .[$i:$i + $n];
  def arms: . as $full | {rule, utils, constraints, rewriters} as $doc
    | def climb($p): if ($p | length) < 2 then $p else ($doc | getpath($p[:-1])) as $parent
        | if ($parent | type) == "array" then (if ($parent | length) > 1 then $p else climb($p[:-1]) end)
          elif (($parent | keys) - [$p[-1]] - ["field", "stopBy"] | length) > 0 then $p else climb($p[:-1]) end end;
    [($doc | paths(type == "object") as $p | getpath($p) | keys[] | select(IN("not", "stopBy", "nthChild")) | {op: "delete", p: climb($p + [.])}),
     ($doc | paths(type == "array") as $p | select($p[-1] == "any") | getpath($p) | select(length > 1) | keys[] | {op: "delete", p: $p + [.]}),
     ($doc.constraints // {} | keys[] | {op: "delete", p: climb(["constraints", .])}),
     ($doc | paths(type == "object" and has("regex")) as $p | {op: "blank", p: $p + ["regex"]})]
    | unique[] | .p as $p | [.op, ($p | tojson), (if .op == "blank" then $full | setpath($p; "") else $full | delpaths([$p]) end | tojson)] | join("\t");
  [inputs] as $docs | ($docs | map(select(.language == $lang and (.role == "rule" or .role == "util")))) as $rules | reach($rules) as $reach
  | ($ARGS.positional | map(select($filter == "" or test($filter)))) as $scoped | (env.FAILED | split(" ")) as $failed
  | $rules[] | .id as $id | select(.role == "util" or IN($id; $ARGS.positional[])) | select(any($scoped[]; $reach[.] | index($id)))
  | [callers($reach; $id)[] | select(IN($ARGS.positional[]) and (IN($failed[]) | not))] as $callers | select($callers != [])
  | .file as $file | [.doc | arms] | chunks(8) | ($id, $file, ($callers | join(" ")), (join("\n") + "\n"))'

# --- [RUNS] -----------------------------------------------------------------------------

# Findings exit 1, loader and execution failures keep their diagnostics and status
scan_matches() {
    local rc=0 errors=$tmp/scan-$BASHPID.log
    ast-grep scan --json=stream "$@" 2>"$errors" || rc=$?
    ((rc == 0 || rc == 1)) || {
        cat "$errors" >&2
        return "$rc"
    }
}

# Hits per invalid case into the named array, one scan over every batched id's case directories and one per anchored case at its leaf
# shellcheck disable=SC2034,SC2004  # Nameref writes the caller's array, and its subscript is a string
hits_of() {
    local -n found=$1
    local root=$2 sgconfig=$3 id c key threads=0 lines=() dirs=() ids=() anchored=() parents=()
    local -A counts case_keys
    [[ $root == "$cases" ]] || threads=1
    for id in "${@:4}"; do
        for ((c = 1; c <= ${rule["invalid,$id"]}; c++)); do
            counts["$id,$c"]=0 case_keys["$id,${case_path["invalid $id case $c"]}"]=$id,$c
        done
        # --error=<id> loads a rewrite the scan skips at off
        if ((${rule["anchored,$id"]})); then anchored+=("$id") parents+=("$root/${rule["target,$id"]}"); else dirs+=("${rule["target,$id"]}") ids+=("$id"); fi
    done
    mkdir -p "$root" "${parents[@]%/*}"
    for id in "${anchored[@]}"; do
        for ((c = 1; c <= ${rule["invalid,$id"]}; c++)); do
            printf '%s' "${text["invalid $id case $c"]}" >"$root/${rule["target,$id"]}"
            scan_matches -c "$root/sgconfig.yml" --filter "^$id\$" --error="$id" --threads 1 --no-ignore hidden "$root/${rule["target,$id"]}" | mapfile -t lines
            counts["$id,$c"]=${#lines[@]}
        done
    done
    rm -f "${parents[@]}"
    if ((${#dirs[@]})); then
        printf -v key '%s|' "${ids[@]}"
        scan_matches -c "$sgconfig" --filter "^(${key%|})\$" "${ids[@]/#/--error=}" --threads "$threads" --no-ignore hidden "${dirs[@]}" |
            jq --raw-output0 '"\(.ruleId),\(.file)"' |
            while IFS= read -r -d '' key; do [[ ! -v case_keys[$key] ]] || ((++counts["${case_keys[$key]}"])); done
    fi
    for id in "${@:4}"; do
        key=
        for ((c = 1; c <= ${rule["invalid,$id"]}; c++)); do key+=" ${counts["$id,$c"]}"; done
        found[$id]=${key# }
    done
}

# One test run over the language's rules the regex names and the callers of every util they reach, a FAIL caller stays out of the arms
run_test() {
    local out rc=0 line
    [[ -n $test_filter ]] || return 0
    out=$(ast-grep test -c "$cases/sgconfig.yml" --include-off --filter "$test_filter" 2>&1) || rc=$?
    while IFS= read -r line; do
        [[ $line =~ ^FAIL\ ([^[:space:]]+) ]] && failed[${BASH_REMATCH[1]}]=1
        ((rc == 0)) || [[ -z $line || $line == PASS\ * ]] || finding "$line"
    done <<<"$out"
}

# --- [CHECKS] ---------------------------------------------------------------------------

# Width: past one hit a once-reporting gap, zero a files: glob the case path misses
check_width() {
    local id i h
    for id in "${owned[@]}"; do
        i=0
        for h in ${hits[$id]}; do
            ((++i))
            ((h == 1)) || finding "width $id case $i: $h hits, ${text["invalid $id case $i"]%%$'\n'*}"
        done
    done
}

# One arm batch of one id over its callers under a job root, proved by classification, hit counts, and fixed source, never by label changes
cover_arms() {
    local n=$1 id=$2 file=$3 block=$4 root=$tmp/jobs/$1 caller op p mutation rc out regex expected='' actual snapshots=() originals=()
    local canon='{"id": .id, "snapshots": (.snapshots | map_values(.fixed))} | sort_keys(..)' # Fixed texts of a snapshot file as one canonical JSON line
    local -A got
    shift 4
    mkdir -p "$root/tests" "$root/snapshots"
    # Mutated file replaces its original in the rule lists, one text under the job root for the test run and under the cases root where the files: globs match
    FROM=$file TO=$root/$id.yml ROOT=$root jq -c '(.ruleDirs, .utilDirs) |= map(if . == env.FROM then env.TO else . end) | .testConfigs = [{testDir: env.ROOT + "/tests", snapshotDir: env.ROOT + "/snapshots"}]' "$cases/sgconfig.yml" |
        tee "$root/sgconfig.yml" >"$cases/job-$n.yml"
    printf -v regex '%s|' "$@"
    regex="^(${regex%|})\$"
    for caller in "$@"; do
        printf -- '---\n%s\n' "${rule["test,$caller"]}" >>"$root/tests/cases.yml" # JSON is YAML the runner reads
        [[ -z ${rule["snapshot,$caller"]} ]] || snapshots+=("$root/snapshots/$caller-snapshot.yml") originals+=("${rule["snapshot,$caller"]}")
    done
    ((${#originals[@]} == 0)) || expected=$(yq -N -o=json -I=0 "$canon" "${originals[@]}")
    while IFS=$'\t' read -r op p mutation; do
        printf '%s' "$mutation" >"$root/$id.yml" # JSON is YAML the loader reads, no yq run per arm
        rc=0
        out=$(ast-grep test -c "$root/sgconfig.yml" --include-off --filter "$regex" --skip-snapshot-tests 2>&1) || rc=$?
        case $rc in
            0) ;;
            4) continue ;;
            8)
                printf 'invalid mutation: %s %s %s, excluded from coverage\n' "$id" "$op" "$p" >>"$root/invalid-mutations.log" # Loader's lines stay out, the arm's case proves it
                continue
                ;;
            *)
                finding "mutation run failed: $id $op $p exit $rc"$'\n'"$out"
                continue
                ;;
        esac
        hits_of got "$root" "$cases/job-$n.yml" "$@"
        for caller in "$@"; do [[ ${got[$caller]} == "${hits[$caller]}" ]] || continue 2; done
        if ((${#snapshots[@]})); then
            # -U run writes every caller's snapshot, a cost paid on the arms the cheaper proofs left alone
            out=$(ast-grep test -c "$root/sgconfig.yml" --include-off --filter "$regex" --update-all 2>&1) || {
                finding "mutation fix comparison failed: $id $op $p"$'\n'"$out"
                continue
            }
            actual=$(yq -N -o=json -I=0 "$canon" "${snapshots[@]}")
            [[ $actual == "$expected" ]] || continue
        fi
        finding "uncovered arm: $id $op $p"
    done <<<"${block%$'\n'}"
}

# One job reaped by wait -n, a job dying under -e prints nothing and its nonzero exit is the finding
reap() {
    local rc=0 pid
    wait -n -p pid "${!job_of[@]}" || rc=$?
    ((rc == 0)) || finding "job died, arms unproven: ${job_of[$pid]} exit $rc"
    unset 'job_of[$pid]'
}

# One job per processor over the arm batches, each against the callers the test run passed and the base scan counted
check_arms() {
    local id file callers block names limit out n ids=() rule_files=() caller_lists=() blocks=()
    limit=$(getconf _NPROCESSORS_ONLN)
    mkdir -p "$tmp/jobs"
    FAILED=${!failed[*]} jq -n --raw-output0 --arg lang "$lang" --arg filter "$filter" --args "$reach $jobs" "${owned[@]}" <"$tmp/docs" |
        while IFS= read -r -d '' id && IFS= read -r -d '' file && IFS= read -r -d '' callers && IFS= read -r -d '' block; do
            ids+=("$id") rule_files+=("$file") caller_lists+=("$callers") blocks+=("$block")
        done
    for ((n = 0; n < ${#ids[@]}; n++)); do # Jobs start after the pipeline, wait -n inside its last stage sees no child
        read -ra names <<<"${caller_lists[n]}"
        ((${#job_of[@]} < limit)) || reap
        cover_arms "$((n + 1))" "${ids[n]}" "${rule_files[n]}" "${blocks[n]}" "${names[@]}" >"$tmp/jobs/$((n + 1)).out" 2>&1 & # Jobs read stdin from /dev/null, no step blocks
        job_of[$!]="${ids[n]} job $((n + 1))"
    done
    while ((${#job_of[@]})); do reap; done
    for out in "$tmp"/jobs/*.out; do [[ ! -s $out ]] || finding "$(<"$out")"; done
    for out in "$tmp"/jobs/*/invalid-mutations.log; do printf '%s\n' "$(<"$out")"; done
}

# Every case and fixed text under the language's parser, the fixed text of an expanding fix written by the CLI first
check_parse() {
    local id key c file out inputs=() outputs=()
    local -A roots=() label_of=()
    for key in "${!case_path[@]}"; do label_of[${case_path[$key]}]=$key; done
    for key in "${!text[@]}"; do [[ $key == fixed* || ${text[$key]} == *[![:space:]]* ]] || finding "empty ${key%% *} case: ${key#* }"; done
    for id in "${owned[@]}"; do
        ((${rule["expanded,$id"]} && ${rule["fixed,$id"]})) || continue
        inputs=() outputs=()
        for ((c = 1; c <= ${rule["invalid,$id"]}; c++)); do
            inputs+=("${case_path["invalid $id case $c"]}") outputs+=("${case_path["fixed $id case $c"]}")
        done
        if ((${rule["anchored,$id"]})); then
            mkdir -p "$(dirname "$cases/${rule["target,$id"]}")" # Anchored leaf under a files: directory the case tree never created
            for ((c = 0; c < ${#inputs[@]}; c++)); do
                cp "${inputs[c]}" "$cases/${rule["target,$id"]}"
                out=$(ast-grep scan -c "$cases/sgconfig.yml" --filter "^$id\$" --error="$id" --threads 1 --no-ignore hidden -U "$cases/${rule["target,$id"]}" 2>&1) || finding "$out"
                mv "$cases/${rule["target,$id"]}" "${outputs[c]}"
            done
            continue
        fi
        # Snapshot text omits expanded ranges, the originals move aside and the CLI writes the fixed source at the case path the glob matches
        for ((c = 0; c < ${#inputs[@]}; c++)); do
            cp "${inputs[c]}" "${outputs[c]}"
            label_of[${outputs[c]}]=${label_of[${inputs[c]}]} label_of[${inputs[c]}]="fixed $id case $((c + 1))"
        done
        out=$(ast-grep scan -c "$cases/sgconfig.yml" --filter "^$id\$" --error="$id" --threads 1 --no-ignore hidden -U "${inputs[@]}" 2>&1) || finding "$out"
    done
    ((${#label_of[@]})) || return 0 # No path operand: ast-grep run reads the working directory
    if [[ $lang == bash ]]; then
        # Parser's lines read from a file on the failing path alone, a capture forks a subshell per case
        for file in "${!label_of[@]}"; do
            bash -n "$file" 2>"$tmp/parse.log" || finding "Bash syntax error in ${label_of[$file]}"$'\n'"$(<"$tmp/parse.log")"
        done
        return 0
    fi
    # Scan case directories once, one argument per fixture can exceed the system argument limit
    for file in "${!label_of[@]}"; do roots[${file%/*/*}]=1; done
    { ast-grep run -c "$cases/sgconfig.yml" -k ERROR -l "$lang" --no-ignore hidden --json=stream "${!roots[@]}" || [[ $? == 1 ]]; } |
        jq -s --raw-output0 'map(.file) | unique[]' |
        while IFS= read -r -d '' file; do finding "ERROR node in ${label_of[$file]}"; done
}

measure() {
    local found hits
    # Element selector and function-depth rule per language, the counts a fix is measured by
    local -A elements=(
        [tsx]=':is(program, export_statement) > :is(lexical_declaration, variable_declaration, function_declaration, generator_function_declaration, type_alias_declaration, interface_declaration, class_declaration, abstract_class_declaration, enum_declaration, ambient_declaration), export_statement > :is(function_expression, arrow_function), program > expression_statement > internal_module'
        [python]='module > :is(function_definition, class_definition, type_alias_statement), module > expression_statement > assignment'
    ) nesting=([tsx]=no-fourth-nesting-level [python]=no-fourth-python-nesting-level)
    [[ -v elements[$lang] ]] || {
        printf 'measure reads %s, and sgconfig.yml maps .%s to %s\n' "${!elements[*]}" "$ext" "$lang"
        exit 1
    }
    # run exits 1 over no match, a capture under -e ends the script at zero elements, the stream holds one line per match
    { ast-grep run -k "${elements[$lang]}" -l "$lang" --json=stream "$@" || [[ $? == 1 ]]; } | mapfile -t found
    scan_matches --filter "^${nesting[$lang]}\$" "$@" | mapfile -t hits
    printf 'elements %s nesting %s\n' "${#found[@]}" "${#hits[@]}"
}

# --- [ENTRY] ----------------------------------------------------------------------------

if [[ $command != pairing ]]; then
    # Language sgconfig.yml maps the extension to, the file entity of an empty file under no rule, and the loader's lines without its own on a refused tree
    printf '\n' >"$tmp/probe.$ext"
    entity=$(ast-grep scan --inspect entity --off "$tmp/probe.$ext" 2>&1) || {
        grep -v '^sg:' <<<"$entity"
        exit 1
    }
    [[ $entity =~ language=([^,]*) ]] || {
        printf 'no language owns .%s\n' "$ext"
        exit 1
    }
    lang=${BASH_REMATCH[1]@L}
fi
if [[ $command == measure ]]; then
    measure "${@:3}"
    exit 0
fi
# Configuration with every path absolute and without a trailing slash, the snapshot directory joined to its test directory
config=$(ROOT=$PWD yq -o=json -I=0 '.utilDirs = (.utilDirs // []) | .testConfigs[].snapshotDir |= (. // "__snapshots__")
  | (.ruleDirs[], .utilDirs[], .testConfigs[].testDir) |= sub("/$"; "")
  | ((.ruleDirs[], .utilDirs[], .testConfigs[].testDir, ((.customLanguages // {})[].libraryPath | .. | select(tag == "!!str"))) | select(test("^/") | not)) |= strenv(ROOT) + "/" + .
  | .testConfigs[] |= (.snapshotDir = .testDir + "/" + .snapshotDir)' sgconfig.yml)
jq --raw-output0 '(.ruleDirs[], .utilDirs[], .testConfigs[].testDir)' <<<"$config" | mapfile -d '' -t dirs
files=()
for dir in "${dirs[@]}"; do files+=("$dir"/**/*.yml); done
yq -N -o=json -I=0 '{"doc": ., "file": filename}' "${files[@]}" | # Document before its name, the reverse order drops shared map keys
    jq -c --argjson config "$config" "$roles" >"$tmp/docs"
if [[ $command == pairing ]]; then
    out=$(jq -nr "$pairing" "$tmp/docs")
    [[ -z $out ]] || finding "$out"
    exit "$findings"
fi
jq -n --raw-output0 --arg lang "$lang" --arg ext "$ext" --arg filter "$filter" --arg command "$command" --arg cases "$cases" --argjson config "$config" "$reach $facts" "$tmp/docs" |
    while IFS= read -r -d '' key && IFS= read -r -d '' value; do
        case $key in
            config) config=$value ;;
            finding) finding "$value" ;;
            tested) test_filter=$value ;;
            invalid,*) rule[$key]=$value owned+=("${key#*,}") ;;
            path,*) case_path[${key#*,}]=$value ;;
            text,*) text[${key#*,}]=$value ;;
            *) rule[$key]=$value ;;
        esac
    done
# Invalid cases sit under the root at their leaf, an anchored leaf under anchored/<id>/<n>, valid and fixed under their kind
mkdir -p "$cases" "${case_path[@]%/*}" # One process for every directory, a run per file costs seconds
printf '%s' "$config" >"$cases/sgconfig.yml"
for key in "${!text[@]}"; do printf '%s' "${text[$key]}" >"${case_path[$key]}"; done
case $command in
    parse) check_parse ;;
    width)
        hits_of hits "$cases" "$cases/sgconfig.yml" "${owned[@]}"
        check_width
        ;;
    arms)
        run_test
        hits_of hits "$cases" "$cases/sgconfig.yml" "${owned[@]}"
        check_arms
        ;;
    *) ;;
esac
exit "$findings"
