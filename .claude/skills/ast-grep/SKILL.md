---
name: ast-grep
description: Use when reading, searching, or rewriting code by its syntax tree, or when deriving, writing, testing, and placing ast-grep rules that enforce a correction across a codebase.
---

# [AST_GREP]

Structural code work (map, find, prove, lint, or rewrite code by its syntax tree) runs on ast-grep. The MCP tools (`find_code`, `find_code_by_rule`, `dump_syntax_tree`, `test_match_code_rule`) run every search and proof that answers with a match list or a tree, and the CLI runs what maps (`ast-grep outline`), scans project rules (`ast-grep scan`), tests (`ast-grep test`), writes (`-U`, `-i`), or needs an exit code, because a failed tool call reaches the agent as `Error executing tool` with no cause. Search rules stay inline, and durable rules are project rule files discovered through `sgconfig.yml`. Examples use the packages of the workspace as vocabulary (Effect modules, generated unions, result types), and a rule names the package it reads.

[REQUIRED]:
- Run `tree .claude/skills/ast-grep/assets/examples` to list every example, a new language or package takes the directory the rules tree layout names
- Start a rule from the template of its file kind and the example of its construct, keep the shape the example proves and the fields the task needs
- Run `.claude/skills/ast-grep/scripts/rule-checks.sh` with no argument to list its commands and the moment to run each

[REFERENCES]: Criteria and runner facts the workflows read:
- [01]-[SKILL_IMPROVEMENT](references/skill-improvement.md): Source rank, comparison sequence, text smells, and opportunity checks for the skill
- [02]-[RULE_BUILDING](references/rule-building.md): Sources, smell table, finding judgment, the fix bar with one pair, and the derivation criteria
- [03]-[RULE_HARDENING](references/rule-hardening.md): Weakness table, widening proof, collapse, devices with failure checks, and the rules tree
- [04]-[RULE_TESTING](references/rule-testing.md): Runner outcomes and flags, what a green run hides, case criteria, snapshots, disproving cases

[TEMPLATES]: One template per file kind, each proven by a filled copy the binary loads, placeholders in angle brackets:
- [01]-[SGCONFIG](assets/templates/sgconfig.template.yml): Project config, the scaffolded layout with a custom grammar and an injected language
- [02]-[RULE](assets/templates/rule.yml): Lint rule under `ruleDirs`, every field the scan reads and the fact each field hides
- [03]-[RULE_REWRITE](assets/templates/rule-rewrite.yml): On-demand rewrite, `severity: off` enabled per run, rewriters and a chain behind one fix
- [04]-[UTIL](assets/templates/util.yml): Parameterized global util, a slot filled direct or through a refinement, a base util called at the root
- [05]-[RULE_TEST](assets/templates/rule-test.yml): Test document bound by id, one case per sibling and per guard, one violation per case
- [06]-[OUTLINE_RULES](assets/templates/outline-rules.yml): Outline extractors, an item document and the member document that attaches under it

[SCRIPTS]:
- [01]-[RULE_CHECKS](scripts/rule-checks.sh): Proves the rules tree `sgconfig.yml` names, one line per finding and exit 1 on any line

## [01]-[OUTLINE]

`ast-grep outline` maps source structure before any full read or edit: line-numbered top-level items (imports, functions, classes, structs, interfaces, modules, enums, flagged imported/exported) with their direct members (fields, methods, constructors, variants, flagged public). Output is syntax-local, and reference resolution, type inference, re-export chains, and call graphs come from structural search after the outline names the files.

Resolve target paths from the task, search hits, or `git diff --name-only`, run the row for the task, narrow the located symbol with `--match <symbol> --view expanded`, then `Read` only the printed line range.

| [INDEX] | [TASK]                           | [COMMAND]                                                                        |
| :-----: | :------------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | Map a directory                  | `ast-grep outline <dir>` — grouped exported names, `--type <t1>,<t2>` narrows    |
|  [02]   | Understand a file before editing | `ast-grep outline <file>` — local structure with member digests                  |
|  [03]   | List a file's dependencies       | `ast-grep outline <file> --items imports`                                        |
|  [04]   | Find importers of a module       | `ast-grep outline <dir> --items imports --match <module> --view signatures`      |
|  [05]   | Enumerate public entry points    | `ast-grep outline <dir> --items exports --view signatures`                       |
|  [06]   | Zoom into one symbol             | `ast-grep outline <file> --match <symbol> --type <type> --view expanded`         |
|  [07]   | Map structure after edits        | `ast-grep outline $(git diff --name-only HEAD) --items exports`                  |
|  [08]   | Outline piped code               | `<producer> \| ast-grep outline --stdin -l <lang>`                               |
|  [09]   | Post-process entries             | `ast-grep outline <path> --json=stream` — one file object per line, jq pipelines |

- `--items structure\|exports\|imports\|all` selects top-level entries, default `structure` for a file or stdin and `exports` for a directory
- `--view names\|signatures\|digest\|expanded` sets detail ascending, default `digest` for a file or stdin and `names` for a directory
- `--match` is case-sensitive Rust regex over item names, signatures, and first source lines, `--type` filters symbol types, neither reaches members
- `--pub-members` hides private members, a member without extractable visibility counts as public
- JSON entries carry `symbolType`, `role`, zero-based `range` with byte offsets, `signature`, `astKind`, and import/export/public flags
- Uncovered syntax takes extractors from `--outline-rules <file>` or `customLanguages.<name>.outlineRules` in `sgconfig.yml`

## [02]-[SEARCH]

Structural search runs on the MCP tools with inline YAML rules. Patterns are valid code under the language's tree-sitter grammar with whole-node metavariables: `$VAR` one named node, `$$VAR` one unnamed node (an operator, a keyword), `$$$MULTI` lazy zero-or-more without backtracking, `$_` and `$_NAME` non-capturing. Smart matching skips unnamed target nodes: the less a pattern specifies, the more it matches. Specify only what the query fixes.

Each search runs in sequence:
1. When the query fits one AST node, run `find_code` with the pattern, an absolute `project_folder`, and a bounded `max_results`
2. For a structural query, start from the most specific positive rule, refine relationally, then filter captures
3. For an unknown node kind, run `dump_syntax_tree` with `format=cst` on one top-level node, or `format=pattern` for a mis-parsing pattern
4. Multi-statement snippets take `ast-grep run -l <lang> -p '<code>' --debug-query=cst`, the tree prints on stderr before exit 8
5. Prove with `test_match_code_rule` on the matching snippet, then on the non-matching one, and return to the tree on a miss
6. When a trusted rule fails the call, run `printf '<code>' | ast-grep scan --inline-rules '<yaml>' --json --stdin; echo $?` and read the exit code
7. Run `find_code_by_rule` with absolute `project_folder`, bounded `max_results`, `output_format=json` when captures or ranges feed the next step

The proof and the search fold three outcomes into one failure, and the exit code of the `--stdin` scan separates them:
- `[]` with 0 is no match, 8 prints the parse error of a rule the binary rejects, and 1 with the JSON is an `error` diagnostic
- The matching snippet run first proves the rule parses, and the failure on the non-matching one reads as no match
- Both calls take severity omitted or under `error`, and the durable file sets `error`
- The search tools run `--json=stream` over the path as given, and a relative path resolves against the server working directory, the repository root
- `language` follows the `languageGlobs` entry of `sgconfig.yml`, a `.ts` file mapped to `tsx` matches under `tsx` and `typescript` finds nothing
- Results past the harness cap land in a file the result names, `jq -r .result <file>` reads it, and `max_results` bounds the context

```yaml
id: <query-id>
language: <language>
utils:
  <util-id>:                                        # Define once, reference via matches, recurse through has/inside, never through a composite
    any: [ { kind: <kind-a> }, { kind: <kind-b> } ]
rule:
  all:                                              # Explicit all: capture order matters, the defining pattern comes first
    - pattern: <code with $VAR and $$$ARGS>
    - has: { pattern: <sub-pattern>, stopBy: end }  # stopBy picks the axis: neighbor pins depth, end opens it, a rule bounds the walk
    - not: { inside: { kind: <kind>, stopBy: end } }
constraints:
  <VAR>: { regex: '<rust-regex>' }                  # Post-rule filter, a full rule object on a single-node capture, a $$$ name parses and never runs
```

- Kind-only structure uses ESQuery in `kind`: `<a> > <b>`, `<a> <b>`, `<a> + <b>`, `<a> ~ <b>`, `<a>:has(> <b>)`, `:not(<b>)`, `:is(<a>,<b>)`
- `run -k '<selector>' -l <lang>` runs a selector with no rule file, `:nth-child(2n+1 of <b>)` counts named siblings of one shape
- The rightmost compound is the subject, `<a> > <b>` matches `<b>`, and `:has(> <a> > <b>)` matches nothing because `<b>` is then the direct child
- `pattern` and `kind` never combine to reparse, a wrong-kind pattern takes `pattern: { context: <full-code>, selector: <kind> }`
- Prefix name searches capture whole nodes with `$NAME($$$)` and `constraints: NAME: { regex: '^<prefix>' }`, and `use$HOOK` is no metavariable
- `$$$` before a node ends at the first sibling the node fits, `f($$$H, { $$$P })` misses `f(x, { a }, { b })` and `f($$$H, { $$$P }, $$$T)` hits it
- `field: <role>` on `has`/`inside` pins same-kind children by parent relation, `field: key` splits an object key from its value
- Rules need a kind set from `pattern` or `kind`, `regex`, `range`, `nthChild`, or `not` alone aborts with a missing-kind error
- Empty `run` results read their exit code, 1 is no match, 8 a rejected pattern (a lone `$$$VAR`), and 0 with a warning an `ERROR` root
- Rule objects are unordered `all`s, the keys apply atomic, composite, then relational, and `all:` keeps its list order
- The first clause naming `$VAR` binds it and later ones re-match it, a binding a later clause needs puts both under `all:`
- Rules test one node, `has: {all: [<a>, <b>]}` demands one child with both shapes, one `has` per required child
- `strictness` per value: `cst` skips nothing, `smart` skips unnamed target nodes and comments, `ast` skips unnamed nodes on both sides, no comment
- `relaxed` skips unnamed nodes on both sides and comments, `signature` skips like `relaxed` and matches leaves by kind alone, `template` drops kinds
- Skipped comments face a leaf, a metavariable, or the match end, and a comment before a compound node (a statement, a call) fails every value
- `signature` over files keeps the fixed-text prefilter of the walk, `a = b` finds `a = y` and misses `x = y`, and `--stdin` matches both
- `range: {start: {line, column}, end: {line, column}}` (zero-based, end exclusive) pins a rule to the node an external report names
- `find_code` returns `No matches found` for an `ERROR` pattern, a missing path, and a `language` no glob maps, `ast-grep run` separates them
- On Windows the server runs `ast-grep.cmd` through a shell, and a call holding `$` or parentheses takes the CLI form
- `template` needs a `kind` beside it, and a comment inside the code takes `kind` with relational rules over a lower strictness

## [03]-[REWRITE]

Rewrite extends a proven search rule with patching fields, each match replacing exactly one target node's text with the instantiated template, and templates are unparsed text:
- Metavariables substitute anywhere, and an undefined one fails the rule parse under `scan` and substitutes empty under `run -r`
- Under a custom language with `expandoChar`, a pattern spells a metavariable with the expando (`_VAR`, `___VAR`)
- `fix`, `transform`, and `constraints` name an expando-spelled metavariable with `$`
- Metavariables bind whole nodes alone: `<Name>_TEXT</Name>` binds the content, `_ID` inside a quoted `AttValue` is string text and binds nothing
- `AttValue` text holds its quotes, `["']` at both ends of a `regex` pins either, one alternation per delimiter when the value holds the other quote
- Whole-element patterns pin the attribute count, and a name alone in attribute position parses as `ERROR`
- Attribute lists rewrite through a whole-node capture (`pattern: _E` under the element kind) and a `transform` chain over the start tag text
- `transform` chains insert an attribute or normalize quotes, and a `replace` that consumes the character after its match needs a second `-U` pass
- Inter-element whitespace is a `CharData` node, element deletion takes `fix: {template: '', expandEnd: {kind: CharData, regex: '^\s*$'}}`
- `rewrite()` over a single capture applies its rewriters to the capture's descendants
- Declared but unmatched metavariables substitute empty in `fix` and as a `rewrite()` source
- `$VARName` lexes as `$VARN` followed by `ame`, and appended text takes a `replace` transform
- Multiline templates re-indent relative to the match's column
- `run -r` covers the pattern-only path, and `fix`, `FixConfig`, titled alternatives, `transform`, and `rewriters` run under `scan --inline-rules`

Each rewrite runs in sequence:
1. Prove the match set through search until the results are exactly the edit set, `--json` match and file counts bound the affected set
2. Attach the rewrite row for the edit, derived text goes in `transform` and multi-node edits in `rewriters`
3. Prove the fix through `test_match_code_rule`, the JSON holds `replacement` and `replacementOffsets`, and the replacement must re-parse
4. Preview the tree diff: `ast-grep scan --inline-rules '<yaml>' <paths>` prints diffs and writes nothing
5. Apply with `-U`, which overrides `-i`, and re-run until stderr prints no `Applied N changes`, nested matches rewrite outer-first per pass
6. Re-run once per region of an injected host file, each pass writes the last matching region alone and `Applied N` counts every match
7. Close with `fmt <target>`, comments never rewrite, find leftovers through a search `regex`

| [INDEX] | [REWRITE]      | [SHAPE]                                                                                                                |
| :-----: | :------------- | :--------------------------------------------------------------------------------------------------------------------- |
|  [01]   | Rename/replace | `ast-grep run -l <lang> -p '<pattern>' -r '<template>' -U <paths>`                                                     |
|  [02]   | Mass removal   | `fix: ''`, a dangling separator goes through `fix: { template: '', expandEnd: { regex: ',' } }`                        |
|  [03]   | Derived names  | `NEW: replace($VAR, replace=<re>, by=<txt>)` or `convert($VAR, toCase=<case>)`, fix takes `$NEW`                       |
|  [04]   | List rewrite   | `rewriters: [{id: <r>, rule: <sub>, fix: <t>}]` with `OUT: rewrite($$$L, rewriters=[<r>], joinBy=<sep>)`, `fix: $OUT`  |
|  [05]   | Element filter | Rewriter emits survivors: `rule: { pattern: $ARG, not: <dropped-shape> }`, `fix: $ARG`, `joinBy: ', '`                 |
|  [06]   | Recursion      | Rewriter `transform` names its own id over a strictly smaller `source`, a non-descending self-match overflows          |
|  [07]   | Alternatives   | `fix: [{title: <a>, template: <t1>}, {title: <b>, template: <t2>}]`, `-U` and `--json` take the first, `-i` offers all |
|  [08]   | Bundle         | Inline YAML split by `---`, overlapping fixes keep the lower rule id, a `program`-level fix blocks the other fixes     |
|  [09]   | Stream         | `<producer> \| ast-grep scan --inline-rules '<yaml>' --stdin -U` — source on stdout, one fixing rule per pass          |
|  [10]   | Separator      | `SEP: replace($$$REST, replace='^.+', by=', ')` — the separator when the list matched, empty otherwise                 |
|  [11]   | Header         | `kind: program` with `pattern: $P`, a rewriter for the inner edit, `fix: <import>\n$NEW_P` adds the import it needs    |

- `rewriters` run in order: per node the first match wins, a consumed subtree never re-matches, an unmatched node keeps its text, descendants match
- `joinBy` set drops an unmatched node's text and joins its rewritten descendants into the output, a member filter pins the list with `inside`
- Rewriters hold `id`, `rule`, `fix`, and their own `constraints`, `transform`, and `utils`, and `rewrite()` names them by id
- Rewriters read the outer rule's captures and none of its `transform` outputs, and export none to the rule or to another rewriter
- Rewriter rules re-match an outer capture, and `inside` in a rewriter rule reads the real ancestors of the node
- Statement templates spell their `;`, the pattern matches the statement without it and the fix drops it otherwise
- Two filtering rewriters over one `$$$` capture partition it, one `rewrite` per group, and the fix places each joined group
- `replace` reads regex capture groups, `by` names them `$1` or `${NAME}` and doubles `$$` for a literal dollar
- `transform` takes the object form `<NEW>: {<op>: {source: $VAR, <key>: <value>}}` or the string form `<NEW>: <op>($VAR, <key>=<value>)`
- Ops with no key keep the comma in the string form, `substring($VAR,)`
- Transform entries chain, a later `source` names an earlier output, and a five-stage `replace` pipeline ends in one `fix: $LAST`
- Rewriters narrow nothing, the parent rule reports every match when no rewriter fires, membership goes in `rule` or `constraints`
- `-i` prompts per diff: `y` accepts, `n` skips, `a` accepts the rest, `q` keeps accepted diffs, `e` opens `$EDITOR` and skips, Tab cycles a fix list
- Sequenced rewrites run the specific pattern before the general one, the general form consumes the arguments the specific one keeps
- `$$$` after a pattern comma binds nothing from `ast` down, a trailing comma matches at every value, `joinBy` unset splices, set drops unmatched text
- Risk splits into sibling documents, the guarded rule holds `fix` and the residual holds `severity: error` with a `note` for manual action
- Captures in an operator, member-access, or return position take template parentheses, because substitution ignores precedence
- `run` exits 1 on zero matches and 0 on a hit, the inverse of `scan`, and `run` ignores suppression comments that `scan` honors
- `--json` beside `-U` writes nothing, and a `run -U` needs `-r`

## [04]-[RULE_CRAFT]

Each device is the standard form for its problem, developed through search and written unchanged into durable files.

- Anchor every `regex` at both ends the rule fixes, because matching is unanchored substring search and `Exemption` matches `NoExemptionHere`
- `regex` is a Rust regex with no look-around or back-reference, `|` inside a name escapes, and `(?i)` sets flags inline
- `kind` and `pattern` restrict the candidate nodes by kind, `regex` restricts none, and `matches: <param>` prunes only beside a `kind`
- Captures unify by default, the same `$VAR` across clauses proves sameness, `not:` on a rebound pattern proves difference, `$_VAR` skips both
- `has` binds `$VAR` to the earliest matching child and a later clause rejecting that child fails the rule, the narrower `has` precedes the wider
- Unification compares nodes, a name wrapped in another kind never unifies, `has: {field: name, pattern: $VAR}` reaches the identifier
- Named leaves unify by text, an `identifier` capture re-matches a `shorthand_property_identifier` and a `property_identifier` of its text
- Captures bound on one node re-match their text inside a later `not: {has: ...}`, the device for a fact every sibling repeats
- `nthChild: <n>` counts named siblings, comments included, and `{position: <n>, ofRule: <rule>, reverse: true}` counts matching siblings from the end
- `nthChild: {position: 1, ofRule: {not: {kind: comment}}}` is the first non-comment child, the device for a required first statement
- Pattern bodies match by containment, `not: {has: {nthChild: 2}}` on the container proves exactly one child
- `nthChild: 1` with `nthChild: {position: 1, reverse: true}` under `all` proves the only child from the child's side
- Rules that count, bound, or measure a structure match the role a node plays: its parent kind, its field, and what it declares
- Kind chains count every node of the kind, and each shape with the kind and without the role (a thunk, an initializer) is a valid case
- `$$$` captures take structural guards or a `rewrite` transform, `constraints` on them never runs, one element takes `pattern: $ITEM` in the list
- Two `$$$` around one element never backtrack, the element takes `pattern: $ITEM` with `inside: {pattern: <list with $$$>, stopBy: end}`
- `stopBy` forms: default neighbor for direct relations, `end` for the whole axis, a rule for a bounded walk that includes the stop node
- Relational rules matching nothing take `stopBy: end` before any other change, the neighbor default is the common cause
- Same-kind stoppers (`stopBy: {kind: <owner-kind>}`) pin the nearest enclosing owner, and bound a `has` walk where the grammar nests (C `case`)
- Closure stoppers on `has` (`stopBy: {kind: <function-kind>}`, or `any:` of them) keep an inner function's `return` from satisfying the outer owner
- `stopBy: {not: {any: [<kinds>]}}` walks up while every ancestor is an allowed kind, the stopper stated as the allowed set
- Aliases of a callee resolve through `inside: {kind: <scope>, stopBy: end, has: {pattern: $ALIAS = <callee>, stopBy: end}}`, `$ALIAS` bound first
- Patterns a grammar update can reshape take `kind` with `field` and `regex` on the name, the longer form keeps matching
- `inside: {not: <shape>, stopBy: end}` matches when any ancestor lacks the shape, nearly every node, absence is `not: {inside: {<shape>}}` at `end`
- `field:` binds the final relation and survives `stopBy: end` (callee vs argument, key vs value), field names read off the `dump_syntax_tree` cst
- Relational objects hold a rule key beside `field` and `stopBy`, `has: {field: <role>}` alone aborts on `Rule must have one positive matcher`
- The positive-matcher abort names `utils` when the relation sits in a util, and `has: {field: <role>, kind: <kind>}` is the parsing form
- `has: {field: <role>, stopBy: end}` tests the field node, then its whole subtree, `has: {stopBy: end}` without a field skips the target node
- `has: {field: <role>, stopBy: <rule>}` tests the field node and its direct children alone, `stopBy: end` beside `field` walks its whole subtree
- `precedes` and `follows` walk the sibling list alone, unnamed siblings included, and reject `field`
- Nested `follows` under `stopBy: end` count earlier siblings, three levels fire on the third counted sibling with no arithmetic
- `constraints:` needs no kind set, negative and relational capture guards belong there
- `has` visits unnamed children, a totality closure over a delimited container reads `not: {has: {pattern: $_, not: <allowed>}}`
- Totality closures prove the container's children only, the inside of an allowed member (an `elif` arm, a nested body) takes its own arm
- Constraints run after the whole rule, a capture guard cannot narrow a `not:`, the negation matches everything and the rule fails silently
- Marker exemptions bind structurally: a comment `precedes: { kind: <body> }` marks its owner, a mark on a descendant proves nothing
- Ordering rules bind statement nodes, an expression pattern alone has no statement siblings, wrap `precedes`/`follows` in `context`/`selector`
- Self-nesting shapes (a chain of arms, a call nested as its own first argument) report once through `not: {inside: <the nesting position>}`
- Fixes fire behind a guard stack: every unfixable variant (guards, discards, exports, valueless members) is a `not:` arm before the template
- `kind` alone defines no metavariable, a fixable capture pairs `kind` with `pattern: $NODE`
- `expandStart`/`expandEnd` extend the fix range to the first sibling matching the sub-rule, the adjacent one by default and any under `stopBy: end`
- An `expandStart`/`expandEnd` miss keeps the match's own range, a sibling-less inner node no-ops silently, and a key with no value fails the parse
- Metavariables bound in `all`, relational rules, and the matching `any:` branch export to `fix`, `message`, and `transform`
- Names bound only under `not:` expand empty, `note` and a label `message` interpolate nothing, `labels` take rule/constraints captures only
- Two `labels:` entries serialize in random order and the snapshot run flakes, one entry per rule, a second span is its own rule and case
- Suppression binds to the match's first or last line, report the tight offender node to put the waiver beside the defect
- `strictness: signature` matches shape while keeping capture identity, the device for a duplicate-shape search and useless as a name ban
- Scan a real codebase and count the matches before a rule is written to its file, a rule firing wider than meant is a semantic invariant in disguise
- `dump_syntax_tree` decides node wrapping and field names, a construct parsing as `ERROR` is unenforceable

## [05]-[DURABLE_RULES]

Durable rules are project structural rules enforced by a scanned gate, `sgconfig.yml` at the root and every YAML under `ruleDirs` a rule. Rules start from a correction made to real code under the workspace standards and generalize it: the correction ends with fewer elements and no extraction, wrapper, indirection, throw, drop, or deferral, and a run proves it before any rule is derived, the higher-order pattern is the shape before the correction, the shape after it, and the reason, and the rule catches every form the same correction applies to. Rules qualify on proofs: the violation is provable by node shape alone, no existing gate (linter, analyzer, type checker, compiler, generator diagnostic) reports it, and it encodes a project rule, generic hygiene stays with the linter. Scope-, type-, or cross-file-dependent invariants fail the first proof. Every lint rule under `rules/` is `severity: error`, and the scan exits nonzero and blocks.

```text
sgconfig.yml                                       # Project config the scan and the test runner read at the root
rules/<language>/<package>/<rule-id>.yml           # Directories name the package or syntax the rules read, one rule per file, the id is the stem
utils/<language>/<util-id>.yml                     # Global utils with explicit id and language, shadowed by a local utils: entry of the same id
rewrites/<language>/<package>/<id>.yml             # On-demand rewrites under a second ruleDirs entry, the fix is the rule, ids read <before>-to-<after>
tests/<language>/<package>/<rule-id>-test.yml      # Test bound to its rule by id, one file per rule, the tree matches rules/
tests/rewrites/<language>/<package>/<id>-test.yml  # Run by ast-grep test --include-off, one file per rewrite, the tree matches rewrites/
tests/__snapshots__/<rule-id>-snapshot.yml         # Written by ast-grep test -U, flat under the test directory, rewrite snapshots beside the rest
```

- `testConfigs` holds `testDir` and `snapshotDir`, relative to `testDir` and `__snapshots__` by default
- `customLanguages.<name>` holds `libraryPath`, `extensions`, `expandoChar`, `languageSymbol`, and `outlineRules`
- Rule directories declare the policy and the test tree validates it, a directory is a category under its language mirrored under `tests/`
- Directories are named for the package (`effect`, `pulumi`) or the language (`syntax`) their rules read, and a later rule over it joins them
- Directories subdivide by module or construct when they outgrow one screen, and the move renames no id
- Files hold one rule each, the id is the file stem, unique project-wide, in `no-<construct>` or `require-<shape>` form
- Rewrites under `rewrites/` are `severity: off` (no plain `scan` or `scan -r` runs them), hold `fix`, `message`, and `note`, and apply one correction
- Rewrite ids read `<before>-to-<after>` in the vocabulary of the package they read, `object-static-to-record`, `ternary-to-boolean-match`
- `ast-grep scan --filter '^<id>$' --error=<id> -U <paths>` applies one rewrite under the root config, re-run until `Applied N changes` stops
- The rewrite call without `-U` exits 0 when no match remains, the check after every apply, and an unknown id exits 3
- Rewrites report only what they write, every unfixable variant is a `not:` arm or a util the rule matches against
- Rewrites over one construct share a guard util, each refuses the arms another rewrite owns, and the set applies in any order
- Rules cover one violation family through `any:` and `utils:` and split when the message or the fix diverges
- Density is logic (utils built on one another, `nthChild` objects, `stopBy` stoppers, `transform` chains, composed rewriters, a guard per variant)
- Sub-rules copied into a second arm or a second rule become a util, local at one rule, global at two, and size alone extracts nothing
- Deep structure shared across split rules goes in a parameterized global util with an explicit `kind` guard
- Global utils are named `<package>-<shape>` and hold `id`, `language`, `arguments`, `rule`, `constraints`, `utils`, and `transform`, no `fix`
- Consumers name the util directory under `utilDirs`, and `scan -r`, `--inline-rules`, and the MCP load none
- Parameterized utils take `arguments` at the global level alone, every argument is mandatory, and a string `matches: <id>` of one exits 8
- Rules call a parameterized util as `matches: {<util-id>: {<arg>: <rule>}}`, each argument a rule, and calls under one `matches` combine as `all`
- Argument rules can be a `matches` to a zero-argument util or a parameterized call, and two calls of one util in a rule bind apart
- Parameterized utils call another at the rule root or under `all`, `any`, or `not`, forwarding a slot as `{<slot>: {matches: <own-slot>}}`
- Parameterized calls under `has` or `inside` in a util file record no load-order edge and fail random loads as `Rule <id> is not defined`
- Callers read a capture from a slot the called util's own body matches, a slot forwarded into a nested call exports nothing
- String `matches: <name>` resolves a parameter, then a local util, then a global util
- Parameterized utils never recurse, their own id as a string in the body matches nothing and the id with arguments fails the load as a cycle
- Zero-argument global utils recurse into themselves and each other through `has` or `inside`, and a global util file holds a local `utils:` block
- Patterns holding a comma take the block form or quotes, a YAML flow map splits at the comma into an unknown field
- Local utils declare no `arguments`, an inline copy of a parameterized util exits 8, and a draft naming one sits under a scratch `ruleDirs`
- `scan -c <config> <path>` scans outside the config's directory, a scratch config with `utilDirs` set to the real utils counts a util alone
- Captures a local util binds reach the caller's `fix`, a global util file's own never do, a collision with a caller binding fails the call
- Argument rules bind their captures at the call site (`source: {pattern: $SOURCE, regex: '<re>'}`, `pattern: _NAME` under an `expandoChar`)
- Captures an argument rule binds reach the caller's `fix`, `message`, and `labels`
- Missing `ruleDirs` or `utilDirs` directories abort the scan, an empty one holds a `.gitkeep`, a shared rule set joins as a submodule or package
- Symlinked directories under `ruleDirs` load, a symlinked rule file is skipped with `Configuration not found!`, a hard link loads

```yaml
id: <rule-id>                    # Imperative grammar: no-<construct> / require-<shape>
language: <language>
severity: error
files: ['<scope-glob>']          # ignores: excludes exempt boundaries, both relative to sgconfig.yml without a ./ prefix
utils:
  <util-id>: { <family-shape> }  # The shape every sibling shares, referenced through matches
rule: { <search rule> }
constraints: { <VAR>: { regex: '<grammar>' } }
fix: <template>                  # When the replacement re-parses and compiles, under the rewrite rules
message: <one line naming the violation, captures and transform variables interpolate>
note: <the correction as the shape to produce>
labels: { <VAR>: { style: primary, message: '<span fact>' } }   # rule/constraints vars only
```

Each rule is added in sequence:
1. Start from a correction made to real code, and state the higher-order pattern in one line: the shape before, the shape after, and the reason
2. Run the qualifying proofs, a failed proof ends the candidate
3. Enumerate the siblings, every form the correction applies to: each module function and carrier, each `dual` overload, container, and spelling
4. Enumerate every position the shape occupies (spread, argument, local, return), and match the shape where it is produced
5. Enumerate the near misses the correction does not apply to, an arm with a replacement that adds a wrapper among them, each becomes a test case
6. Prove the node shape with `dump_syntax_tree` on the real violating code and each sibling, a shape parsing as `ERROR` is unenforceable, stop
7. Author from the matching `assets/templates/` file: the shared shape in a `utils` entry, `constraints` for the name grammar, `stopBy` per relation
8. Write `message` naming the violation and `note` stating the correction, and add `fix` when the replacement re-parses
9. Route an import the fix needs through the header rewrite, which blocks every other fix in the file, or leave the fix out
10. Write the test file with matching id, the corrected code and each near miss under `valid:`, the instance and each sibling under `invalid:`
11. Comment each case with the shape it covers, run `ast-grep test -U` to write the snapshots the first run fails without, and keep them
12. Read a fix as the snapshot's fixed text, and an `expandStart`/`expandEnd` consumption in `scan --json` `replacementOffsets` alone
13. Place the rule in its directory, `ast-grep scan --inspect entity` proves registration, `--filter '<rule-id>'` iterates it alone
14. Scan the codebase, read every hit as a real finding or a rule defect, and correct the code or the rule before the rule joins the gate
15. Check the correction the `note` prescribes against every other gate, a data-last step another gate rejects binds a local and stays data-first
16. Gate with `ast-grep scan --error=unused-suppression --error=no-suppress-all` and `rule-checks.sh gate <ext>`, `--format github` annotates CI

| [INDEX] | [RULE_CLASS]           | [MECHANISM]                                                                                                   |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------------------------------ |
|  [01]   | Banned construct       | Construct kinds with `ignores:` on exempt boundary globs, or `not: { inside: <marker-comment rule> }`         |
|  [02]   | Required shape         | Owner kind with `not: { has: <required child, argument, or modifier> }`                                       |
|  [03]   | Entry-point discipline | Declaration-name `regex` over mode-suffix and option grammars, single-hop forwarders as patterns              |
|  [04]   | Layer boundary         | One rule per forbidden edge: import kind + path `regex`, `files:` scoping the consumer layer                  |
|  [05]   | Policy literal         | Literal kinds `inside` policy argument and initializer positions, named constants own the values              |
|  [06]   | Dispatch shape         | Dispatch kind `inside` a dispatch arm, catch-all arms beside sealed-hierarchy arms                            |
|  [07]   | Naming grammar         | Name-position `regex`: word budget, banned generic suffixes, role-suffix bijection via `not: has`             |
|  [08]   | Self-nesting           | Same-family calls nested as arguments, or a run call inside one, the outermost reported through `not: inside` |
|  [09]   | Repeated fact          | Pair bound on the first row, `not: {has: <row>, not: {has: <bound pair>}}` proves every row repeats it        |

- Unparseable rules or duplicate ids abort the whole scan, an inline `---` bundle tolerates duplicate ids alone, drafts stay outside `ruleDirs`
- `sgconfig.yml` accepts unknown keys silently, scoping uses per-rule `files:`/`ignores:` only
- `language:` is single-valued, similar languages share rules via a `languageGlobs` superset entry, embedded languages use `languageInjections`
- Copied examples take the `language` the `languageGlobs` entry names for their files, `--inspect entity` lists it and `--filter` counts a known hit
- Suppression is rule-scoped, `ast-grep-ignore: <rule-id>` opens the comment on the same, last, or preceding line of the match
- `unused-suppression` is `hint` while no `--filter`, `--off`, or `--min-severity` narrows the rules, `no-suppress-all` is `off`, `--error=` gates
- Whole-file rules waive only file-wide, the suppression comment on line 1 over an empty line 2, a line-scoped comment joins the match
- `files:` globs match the path relative to `sgconfig.yml`, or to the working directory under `--inline-rules`
- Wildcard globs take an implied `**/` prefix, a plain file name matches the one file beside `sgconfig.yml`, and `**/<name>` every file so named
- A `./` prefix or a `!` glob in `files:` matches nothing, exclusion is `ignores:`, and `scan -r` reads globs relative to the rule file
- Dot-directory scopes (`.github/`, `.claude/`) need `--no-ignore hidden` on the gate command
- Green can prove nothing: omitted `severity` is `hint`, `--min-severity` drops rules, `test` passes zero cases, the snapshot run is the gate
- Cases hold one violation each, and every arm of the rule has the case that fails when the arm is deleted
- `metadata:` holds routing facts and appears under `--json --include-metadata`, `url:` shows in the editor and SARIF and never in `--json`

## [06]-[INTEGRATIONS]

Hosts consume the scan through its exit codes, its output formats, and the library bindings, and each host takes the row for the result it needs.

| [INDEX] | [HOST]        | [SHAPE]                                                                                                                |
| :-----: | :------------ | :--------------------------------------------------------------------------------------------------------------------- |
|  [01]   | CI annotation | `ast-grep scan --format github` prints `::error file=,line=,title=<rule-id>::` per finding above `hint`, no upload     |
|  [02]   | Code scanning | `ast-grep scan --format sarif > <file>` then `github/codeql-action/upload-sarif`, `--format` excludes `--json`         |
|  [03]   | Hook          | `ast-grep scan --report-style short --color never <staged>`, then `rule-checks.sh gate <ext>` per language             |
|  [04]   | Changed files | `ast-grep scan $(git diff --name-only <base>... -- '*.<ext>')`, an empty list exits before the scan                    |
|  [05]   | Pipeline      | `ast-grep scan --json=stream \| jq -c '<filter>'` — one match per line, `ruleId`, `range.byteOffset`, captures         |
|  [06]   | Baseline      | `ast-grep scan --filter '^<rule-id>$' --json=stream \| wc -l` against a recorded count, one rule's width over the tree |
|  [07]   | Parse gate    | `ast-grep run -k ERROR -l <lang> --json=compact <paths>` exits 1 when every file parses                                |
|  [08]   | Editor        | `ast-grep lsp` over the root `sgconfig.yml`: diagnostics, `labels`, a code action per `fix`, reload on any YAML change |
|  [09]   | Model text    | `--json=stream` selects the nodes, the model returns one replacement per match, edits splice by `byteOffset`           |
|  [10]   | Library       | `@ast-grep/napi` or `ast-grep-py` when a replacement is computed, arguments take per-position checks, or files cross   |

- Match objects hold `text`, `range` (`byteOffset`, zero-based `start`/`end`), `replacement`, `replacementOffsets`, and `metaVariables`
- `findInFiles` resolves with the file count before every callback ran, count the callbacks against it before reading results
- `getMultipleMatches` returns separator tokens, filter `kind() !== ','` before indexing an argument list
- `NapiConfig` has no `fix`, expand `$$$` then `$` from `getMatch`/`getMultipleMatches`, and `replace` substitutes no metavariable
- Edits are non-nested byte ranges committed on the root, an insertion is a zero-width edit, match the inner node when an outer edit consumes it
- `registerDynamicLanguage` runs once per process with every `@ast-grep/lang-*` package in one call, a missing registration fails at parse time
- `ast-grep-py` is `SgRoot(src, language)` alone, rules pass as keyword arguments (`find(pattern=<code>)`), and file discovery is the caller's
- One directory argument beats a batched file list, the walk parses in parallel and `--globs '!<glob>'` excludes inside it

## [07]-[WORKFLOWS]

Each workflow extends the skill with a reference and an agent that runs one scope per pass, and a main agent working alone reads the reference:

| [INDEX] | [WORKFLOW]        | [REFERENCE]         | [AGENT]                   | [SCOPE]                                                       |
| :-----: | :---------------- | :------------------ | :------------------------ | :------------------------------------------------------------ |
|  [01]   | Rule building     | `rule-building`     | `ast-grep-rule-builder`   | Source directory or package, weak code fixed, rules derived   |
|  [02]   | Rule hardening    | `rule-hardening`    | `ast-grep-rule-hardener`  | Rules directory, language, or family, widened and collapsed   |
|  [03]   | Rule testing      | `rule-testing`      | `ast-grep-rule-tester`    | Rules directory, language, or family, disproved by cases      |
|  [04]   | Skill improvement | `skill-improvement` | `ast-grep-skill-improver` | Section, reference, agent file, or the set, checked at source |

The main agent orchestrates the agents and keeps its own understanding of their files current as they change:
- Dispatch each agent fresh over a disjoint scope, with the scope, the direction, the standards, the proof, and the messaging rule in its brief
- Edit no file inside a dispatched scope while its agent runs
- Brief each agent to message `main` with a finding outside its scope, related or tangential, and a smell in any file
- Brief each agent to message an active `ast-grep-*` agent directly with a change that agent adjusts to or integrates
- Act on each finding as it arrives: relay it to the agent that holds the scope, or dispatch a focused general-purpose agent for it
- Agents' peer lists hold the agents alive when they started, and a finding for an agent started later goes through `main`
- Read each changed file as it lands, and judge agents converging on one approach against their scopes, the shared approach is a finding
- Hold no work back for a later pass, and defer, store, or hedge nothing
- After each agent returns, dispatch a fresh agent over the same scope that attacks every decision

Implement each improvement to the skill, a reference, or an agent file that a run or an agent identifies in place: delete, reframe, or correct.
