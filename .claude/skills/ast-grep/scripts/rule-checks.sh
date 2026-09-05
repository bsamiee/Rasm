#!/usr/bin/env bash
# Proves the rules tree sgconfig.yml names: ids paired, arms covered, fixes re-parsed, cases reported once, the test run green
# Usage: rule-checks.sh <extension>, from the directory holding sgconfig.yml, over the rules of the language that owns the extension
# One line per finding, exit 1 on any
set -uo pipefail

[ $# -eq 1 ] || { echo "usage: rule-checks.sh <extension>"; exit 1; }
[ -f sgconfig.yml ] || { echo "sgconfig.yml not found in $PWD"; exit 1; }

ext=$1
rule_dirs=$(yq -r '.ruleDirs[]' sgconfig.yml)
util_dirs=$(yq -r '.utilDirs[]' sgconfig.yml)
tests=$(yq -r '.testConfigs[0].testDir' sgconfig.yml)
snapshots="$tests/__snapshots__"
scratch=$(mktemp -d)
trap 'rm -rf "$scratch"' EXIT
findings=0

finding() { printf '%s\n' "$*"; findings=1; }

rule_files() { fd -e yml . $rule_dirs; }
test_files() { fd -e yml . "$tests" -E __snapshots__; }

# The copy takes the arm mutations, and the grammar libraries link in because a missing library aborts every run
copy_tree() {
  cp sgconfig.yml "$scratch/"
  for d in $rule_dirs $util_dirs "$tests"; do mkdir -p "$scratch/$(dirname "$d")"; cp -R "$d" "$scratch/$d"; done
  for lib in $(yq -r '.customLanguages[].libraryPath' sgconfig.yml); do
    mkdir -p "$scratch/$(dirname "$lib")"; ln -s "$PWD/$lib" "$scratch/$lib"
  done
}

scan_count() { ast-grep scan -c "$scratch/sgconfig.yml" --filter "^$1\$" --json=stream "$2" 2>/dev/null | wc -l | tr -d ' '; }

# The language ast-grep maps the extension to, read from the file entity of an empty probe file, lowercased like a rule's language
owning_language() {
  printf '\n' > "$scratch/probe.$ext"
  ast-grep scan -c "$scratch/sgconfig.yml" --inspect entity "$scratch/probe.$ext" 2>&1 \
    | sed -n 's/^sg: entity|file|.*: language=\([^,]*\),.*/\1/p' | tr '[:upper:]' '[:lower:]'
}

# The case file sits where the rule's first files: glob matches, relative to the copied sgconfig.yml, or at <id>.<ext>
case_path() {
  local glob
  glob=$(yq -r '.files[0] // ""' "$1"); glob=${glob#\*\*/}
  [ -n "$glob" ] && printf '%s' "${glob//\*/$2}" || printf '%s.%s' "$2" "$ext"
}

# Hits per invalid case, each case written alone to the rule's case path and counted under the copy's config
count_rule() {
  local id=$1 t=$2 path=$3 b64
  mkdir -p "$(dirname "$scratch/$path")"
  for b64 in $(yq -o=json -I=0 '.invalid' "$t" | jq -r '.[] | @base64'); do
    printf '%s' "$b64" | base64 -d > "$scratch/$path"
    scan_count "$id" "$scratch/$path"
  done | tr '\n' ' '
  rm -f "$scratch/$path"
}

# One row per paired test: id, language, case path, hits per case, case count, and test path, hits blank for a language the run skips
count_cases() {
  local t id rule lang path hits
  ext_lang=$(owning_language)
  for t in $(test_files); do
    id=$(yq -r '.id' "$t"); rule=$(fd -e yml "^$id\.yml$" $rule_dirs | head -1)
    [ -n "$rule" ] || continue
    lang=$(yq -r '.language' "$rule" | tr '[:upper:]' '[:lower:]'); path=$(case_path "$rule" "$id"); hits=
    [ "$lang" = "$ext_lang" ] && hits=$(count_rule "$id" "$t" "$path")
    printf '%s\t%s\t%s\t%s\t%s\t%s\n' "$id" "$lang" "$path" "$hits" "$(yq '.invalid | length' "$t")" "$t" >> "$scratch/counts"
  done
  [ -n "$ext_lang" ] && awk -F '\t' -v l="$ext_lang" '$2 == l' "$scratch/counts" | rg -q . || finding "no rule reports under .$ext"
}

# 1 pairing: every rule id has a test id and every test id a rule id
check_pairing() {
  while IFS= read -r id; do finding "unpaired id: $id"; done \
    < <(comm -3 <(rule_files | xargs yq -r '.id' | sort) <(test_files | xargs yq -r '.id' | sort) | tr -d '\t')
}

# 2 test shape: the keys the runner reads, one case each side at least, every invalid case a snapshot key and every key a case
check_test_shape() {
  local t id s k
  for t in $(test_files); do
    id=$(yq -r '.id' "$t"); s="$snapshots/$id-snapshot.yml"
    for k in $(yq -r 'keys | .[] | select(test("^(id|valid|invalid)$") | not)' "$t"); do finding "unknown key in $id: $k"; done
    [ "$(yq '.valid | length' "$t")" -ge 1 ] && [ "$(yq '.invalid | length' "$t")" -ge 1 ] || finding "one side empty: $id"
    if [ -f "$s" ]; then
      diff <(yq -o=json -I=0 '.invalid | sort' "$t") <(yq -o=json -I=0 '.snapshots | keys | sort' "$s") >/dev/null \
        || finding "orphan or missing snapshot key: $id"
    else
      finding "no snapshot: $id"
    fi
  done
}

# 3 arm coverage: delete one arm into the copy, a case flips (test exit 4) or a hit count over the cases moves, else no case covers it
check_arm_coverage() {
  local arms paths prune rule id t path base p rc
  paths='.[] | select(.p != null and (.p | length) > 0) | if .n == 1 then .p elif .n == 0 then .p else .p + ["not"] end'
  prune='del(.constraints | select(length==0)) | (.. | select(tag=="!!seq")) |= map(select(tag!="!!map" or length>0))'
  arms='[(.. | select(tag=="!!map" and has("not")) | {"p": path, "n": length}),
    (.. | select(tag=="!!map" and has("any")) | .any[] | {"p": path, "n": 0}),
    (.. | select(tag=="!!map" and has("stopBy")) | {"p": path + ["stopBy"], "n": 0}),
    (.constraints // {} | to_entries[] | {"p": ["constraints", .key], "n": 0})]'
  for rule in $(rule_files); do
    id=$(yq -r '.id' "$rule")
    IFS=$'\t' read -r path base t < <(awk -F '\t' -v id="$id" -v l="$ext_lang" '$1 == id && $2 == l {print $3 "\t" $4 "\t" $6}' "$scratch/counts")
    [ -n "$path" ] || continue
    while IFS= read -r p; do
      yq "delpaths([$p]) | $prune" "$rule" > "$scratch/$rule"
      (cd "$scratch" && ast-grep test --filter "^$id\$" --color never >/dev/null 2>&1); rc=$?
      case $rc in
        4) ;;
        0) [ "$(count_rule "$id" "$t" "$path")" != "$base" ] || finding "uncovered arm: $id $p";;
        *) finding "unchecked arm: $id $p exit $rc";;
      esac
    done < <(yq -o=json -I=0 "$arms" "$rule" | jq -c "$paths" | sort -u)
    cp "$rule" "$scratch/$rule"
  done
}

# 4 fix proof: every fixed text re-parses, run -k ERROR exits 1 when the text holds no ERROR node
check_fix_reparse() {
  local s id lang b64
  for s in "$snapshots"/*.yml; do
    id=$(yq -r '.id' "$s"); lang=$(yq -r '.language' "$(fd -e yml "^$id\.yml$" $rule_dirs | head -1)")
    for b64 in $(yq -o=json -I=0 '[.snapshots[] | select(has("fixed")) | .fixed]' "$s" | jq -r '.[] | @base64'); do
      printf '%s\n' "$(printf '%s' "$b64" | base64 -d)" | ast-grep run -k ERROR -l "$lang" --stdin --json=compact >/dev/null 2>&1
      [ $? -eq 1 ] || finding "ERROR node in fixed: $id"
    done
  done
}

# 5 width: each invalid case alone yields one hit, more is a once-reporting gap and zero a files: glob the case path misses
check_width() {
  local id hits i h
  while IFS=$'\t' read -r id _ _ hits _; do
    i=0
    for h in $hits; do i=$((i + 1)); [ "$h" = 1 ] || finding "width $id case $i: $h hits"; done
  done < <(awk -F '\t' -v l="$ext_lang" '$2 == l' "$scratch/counts")
}

# 6 the run itself
check_test_run() {
  local out rc line
  out=$(ast-grep test --color never 2>&1); rc=$?
  while IFS= read -r line; do finding "$line"; done < <(printf '%s\n' "$out" | rg 'Configuration not found|SKIP|FAIL')
  [ $rc -eq 0 ] || finding "ast-grep test exit $rc"
}

copy_tree
count_cases
check_pairing
check_test_shape
check_arm_coverage
check_fix_reparse
check_width
check_test_run
exit $findings
