---
name: ast-grep
description: Structural code work on ast-grep, outline maps before reading source, AST search through the MCP tools with inline rules, durable rule folders enforcing project quality, and syntax-tree rewrites. Use when reading, editing, or searching any code/source file, when authoring or organizing a project's ast-grep rules, and when applying structural rewrites.
---

# [AST_GREP]

Structural code work (map, find, prove, lint, or rewrite code by its syntax tree) runs on ast-grep. Read the section for the task in full and follow its steps before touching a source file. The MCP tools run search and rule development, and the CLI runs structural maps (`ast-grep outline`), project rule scans (`ast-grep scan`), and applied rewrites.

- `find_code`: pattern search over a path
- `find_code_by_rule`: inline YAML rule search for structures a pattern alone cannot express
- `dump_syntax_tree`: node kinds and structure of a snippet, the debugging tool for every non-match
- `test_match_code_rule`: proves a rule against representative code before it runs anywhere

Prove every rule through `test_match_code_rule` with severity omitted or `warning`, because an `error` diagnostic exits nonzero and the tool discards the JSON, and set the final severity in the durable file. Debug every non-match through `dump_syntax_tree` against the target snippet. Search rules stay inline, and durable rules are project rule files discovered through `sgconfig.yml`.

[TEMPLATES]:
- [01]-[SGCONFIG](assets/templates/sgconfig.template.yml): Project config with every key the scan, the tests, and the parsers read
- [02]-[RULE](assets/templates/rule.yml): Lint rule with the full field set and a `---` sibling, the topic-file form every durable rule starts from
- [03]-[RULE_REWRITE](assets/templates/rule-rewrite.yml): Rewrite rule with rewriters over a list capture, and a deletion rule as its sibling
- [04]-[UTIL](assets/templates/util.yml): Parameterized global utility rule with its kind guard and call form
- [05]-[RULE_TEST](assets/templates/rule-test.yml): Test file bound to its rule by id, with snapshots from `ast-grep test -U`

[EXAMPLES]: proven rules with test files under `rule-tests/`, each picked by its mechanism and adapted:
- [01]-[YAML](assets/examples/require-job-timeout.yml): Required shape by depth: relational descent to the owner, direct-child absence chain
- [02]-[BASH](assets/examples/require-strict-mode.yml): Container-scoped absence: the file is the match, a direct-child `not: has` proves it
- [03]-[TYPESCRIPT](assets/examples/require-concurrency-option.yml): Missing option: neighbor-`has` precision no nested callback satisfies
- [04]-[TYPESCRIPT](assets/examples/enum-to-const-object.yml): Construct conversion: rewriters fold members, guards refuse unfixable variants
- [05]-[CSHARP](assets/examples/no-forward-referenced-smart-enum-item.yml): Ordering proof: capture unification across `has` and `precedes`
- [06]-[CSHARP](assets/examples/switch-expression-to-generated-switch.yml): Dispatch conversion: context/selector, derived names, dual guards
- [07]-[PYTHON](assets/examples/no-raise-in-result-function.yml): Signature-scoped ban: nearest-enclosing `stopBy` pins the return type
- [08]-[PYTHON](assets/examples/guard-clauses-to-conditional-expression.yml): Fold to expression: totality closure, parenthesized arms

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

Structural search runs on the MCP tools with inline YAML rules. Patterns are valid code under the language's tree-sitter grammar with whole-node metavariables: `$VAR` one named node, `$$VAR` one unnamed node, `$$$MULTI` lazy zero-or-more, `$_` non-capturing. Smart matching skips unnamed target nodes: the less a pattern specifies, the more it matches. Specify only what the query fixes.

Each search runs in sequence:
1. When the query fits one AST node, run `find_code` with the pattern
2. For a structural query, start from the most specific positive rule, refine relationally, then filter captures
3. For an unknown node kind, run `dump_syntax_tree` on representative target code with `format=cst`, or `format=pattern` for a mis-parsing pattern
4. Prove with `test_match_code_rule` against a matching and a non-matching snippet, and return to `dump_syntax_tree` on every non-match
5. Run `find_code_by_rule` with absolute `project_folder` and bounded `max_results`, `output_format=json` when captures or ranges feed the next step

```yaml
id: <query-id>
language: <language>
utils:
  <util-id>:                                        # Define once, reference via matches, recurse through has/inside
    any: [ { kind: <kind-a> }, { kind: <kind-b> } ]
rule:
  all:                                              # Explicit all: capture order matters, the defining pattern comes first
    - pattern: <code with $VAR and $$$ARGS>
    - has: { pattern: <sub-pattern>, stopBy: end }  # stopBy picks the axis: neighbor pins depth, end opens it, a rule bounds the walk
    - not: { inside: { kind: <kind>, stopBy: end } }
constraints:
  <VAR>: { regex: '<rust-regex>' }                  # Post-rule text filter on a single-node capture
```

- Kind-only structure uses ESQuery in `kind`: `<a> > <b>`, `<a> <b>`, `<a>:has(> <b>)`, `:not(<b>)`, `:is(<a>,<b>)`, `:nth-child(2n+1 of <b>)`
- `pattern` and `kind` never combine to reparse, a wrong-kind pattern takes `pattern: { context: <full-code>, selector: <kind> }`
- Prefix name searches capture whole nodes with `$NAME($$$)` and `constraints: NAME: { regex: '^<prefix>' }`, and `use$HOOK` is no metavariable
- `field: <role>` on `has`/`inside` pins same-kind children by parent relation, `field: key` splits an object key from its value
- Rules need a positive field (`pattern` or `kind`), and `regex`, `field`, or a negation alone matches nothing
- The first rule naming `$VAR` defines its content and later rules re-match it, order `all` by that
- `strictness: relaxed` widens past comments and unnamed nodes, `signature` alone drops text, take either only when a proven match is blocked

## [03]-[REWRITE]

Rewrite extends a proven search rule with patching fields, each match replacing exactly one target node's text with the instantiated template. Templates are unparsed text: metavariables substitute anywhere, an undefined metavariable fails the parse under `scan` and substitutes empty under `run -r`, a declared but unmatched one substitutes empty in both, and `$VARName` lexes as `$VARN` followed by `ame` (appended text takes a `replace` transform). Multiline templates re-indent relative to the match's column. `run` covers the pattern-only path. The full field set (`fix`, `FixConfig`, `transform`, `rewriters`) runs under `scan --inline-rules`.

Each rewrite runs in sequence:
1. Prove the match set through search until the results are exactly the edit set, `--json` match and file counts bound the affected set
2. Attach the rewrite row for the edit, derived text goes in `transform` and multi-node edits in `rewriters`
3. Prove the fix through `test_match_code_rule`, the JSON holds `replacement` and `replacementOffsets`, and the replacement must re-parse
4. Preview the tree diff: `ast-grep scan --inline-rules '<yaml>' <paths>` prints diffs and writes nothing, `--json` stays read-only even with `-U`
5. Apply with `-U`, which overrides `-i`, and re-run until zero changes, nested matches rewrite outer-first and the re-run proves idempotence
6. Close with `fmt <target>`, comments never rewrite, find leftovers through a search `regex`

| [INDEX] | [REWRITE]      | [SHAPE]                                                                                                             |
| :-----: | :------------- | :------------------------------------------------------------------------------------------------------------------ |
|  [01]   | Rename/replace | `ast-grep run -l <lang> -p '<pattern>' -r '<template>' -U <paths>`                                                  |
|  [02]   | Mass removal   | `fix: ''`, a dangling separator goes through `fix: { template: '', expandEnd: { regex: ',' } }`                     |
|  [03]   | Derived names  | `NEW: replace($VAR, replace=<re>, by=<txt>)` or `convert($VAR, toCase=<case>)`, fix takes `$NEW`                    |
|  [04]   | List rewrite   | `rewriters: [{id: <r>, rule: <sub>, fix: <t>}]` with `OUT: rewrite($$$L, rewriters=[<r>], joinBy=<sep>)`, `fix: $OUT` |
|  [05]   | Element filter | Rewriter emits survivors: `rule: { pattern: $ARG, not: <dropped-shape> }`, `fix: $ARG`, `joinBy: ', '`              |
|  [06]   | Recursion      | Rewriter `transform` names its own id over a strictly smaller `source`, a non-descending self-match overflows       |
|  [07]   | Bundle         | Inline YAML, rules split by `---`, identical-range fix collisions resolve by ascending rule id                      |
|  [08]   | Stream         | `<producer> \| ast-grep scan --inline-rules '<yaml>' --stdin -U` — rewritten source on stdout, disk untouched       |

- `rewriters` is an ordered sequence: per node the first matching rewriter wins, a consumed subtree never re-matches
- Metavariables, `transform`, and `utils` stay inside their rewriter, and rewriters cross-reference by id inside `rewrite()`
- Optional trailing commas take another pattern variant under `any:`, and `joinBy` absent splices in place while present it discards unmatched text
- Risk splits into sibling documents, the guarded rule holds `fix` and the residual holds `severity: error` with a `note` for manual action
- Captures in an operator, member-access, or return position take template parentheses, because substitution ignores precedence
- `run` exits 1 on zero matches and 0 on a hit, the inverse of `scan`, and shell chains over the subcommands read opposite ways

## [04]-[RULE_CRAFT]

Each device is the standard form for its problem, developed through search and written unchanged into durable files.

- Anchor every `regex` at both ends the rule fixes, because matching is unanchored substring search and `Exemption` matches `NoExemptionHere`
- Captures unify by default, the same `$VAR` across clauses proves sameness, `not:` on a rebound pattern proves difference, `$_VAR` skips both
- `nthChild` takes `ofRule` for nth-of-kind and counts all named siblings without it, `{position: 1, reverse: true, ofRule}` pins the last arm
- `$$$` captures take structural guards or a `rewrite` transform, `constraints` on them parses and does nothing
- `stopBy` forms: default neighbor for direct relations, `end` for the whole axis, a rule for a bounded walk, same-kind stoppers pin the nearest owner
- `field:` binds the final relation and survives `stopBy: end` (callee vs argument, key vs value), field names read off the `dump_syntax_tree` cst
- `rule:` demands a positive field, `constraints:` does not, negative and relational capture guards belong there
- Totality closures (`not: {has: {not: {any: [...]}}}`) need a positive member allowlist in delimiter grammars, unnamed children defeat the negation
- Totality closures prove the container's children only, an allowed member's interior (an `elif` arm, a nested body) takes its own arm
- Constraints run after the whole rule, a capture guard cannot narrow a `not:`, the negation matches everything and the rule fails silently
- Marker exemptions bind structurally: a comment `precedes: { kind: <body> }` marks its owner, a mark on a descendant proves nothing
- Ordering rules bind statement nodes, an expression pattern alone has no statement siblings, wrap `precedes`/`follows` in `context`/`selector`
- Fixes fire behind a guard stack: every unfixable variant (guards, discards, exports, valueless members) is a `not:` arm before the template
- `expandStart`/`expandEnd` consume exactly one adjacent sibling matching the sub-rule (`stopBy` inert), a sibling-less inner node no-ops silently
- Metavariables bound anywhere export to `fix`, `message`, and `transform`, `note` interpolates nothing, `labels` take rule/constraints captures only
- Suppression binds to the match's first or last line, report the tight offender node to put the waiver beside the defect
- `strictness: signature` matches shape while keeping capture identity, the device for a duplicate-shape search and useless as a name ban
- Scan a real codebase and count the matches before a rule is written to its file, a rule firing wider than meant is a semantic invariant in disguise
- `dump_syntax_tree` decides node wrapping and field names, a construct parsing as `ERROR` is unenforceable

## [05]-[DURABLE_RULES]

Durable rules are project structural rules enforced by a scanned gate, `sgconfig.yml` at the root and every YAML under `ruleDirs` a rule. Rules qualify on proofs: the violation is provable by node shape alone, no existing gate (linter, analyzer, type checker, compiler, generator diagnostic) reports it, and it encodes a project rule rather than generic hygiene. Scope-, type-, or cross-file-dependent invariants fail the first proof. Each rule covers a whole violation family through `any:` and `utils:`, splits when message or fix diverges, and places deep structure shared across split rules in a parameterized global util with an explicit `kind` guard. Every rule is `severity: error`, and the scan exits nonzero and blocks.

```text
sgconfig.yml                     # Project config the scan reads at the root
rules/<language>/<area>.yml      # One topic file per rule family, rules split by ---, ids unique project-wide
utils/<util-id>.yml              # Global utils with explicit id and language, shadowed by a local utils: entry of the same id
rule-tests/<rule-id>-test.yml    # Test bound to its rule by id, snapshots beside it in __snapshots__/
```

```yaml
id: <rule-id>                    # Imperative grammar: no-<construct> / require-<shape>
language: <language>
severity: error
files: ['<scope-glob>']          # ignores: excludes exempt boundaries, both relative to sgconfig.yml without a ./ prefix
utils:
  <util-id>: { <family-shape> }
rule: { <[02] rule> }
constraints: { <VAR>: { regex: '<grammar>' } }
fix: <template>                  # When the repair is mechanical, under the rewrite rules
message: <one line naming the violation>
note: <the exact repair, the shape to produce>
labels: { <VAR>: { style: primary, message: '<span fact>' } }   # rule/constraints vars only
```

Each rule is added in sequence:
1. Derive the candidate from an existing project rule, run the qualifying proofs, a failed proof ends the candidate
2. Prove the violating node shape with `dump_syntax_tree` on real violating code, a shape the grammar parses as `ERROR` is unenforceable, stop
3. Author from the matching `assets/templates/` file, develop the rule through search and the fix through rewrite, proved both ways
4. Add the test file with matching id, conforming cases under `valid:` and violating under `invalid:`, then `ast-grep test -U` writes snapshots
5. Place the rule in its folder, `ast-grep scan --inspect entity` proves registration, `--filter '<rule-id>'` iterates it alone
6. Gate with `ast-grep scan --no-ignore hidden --error=unused-suppression --error=no-suppress-all`, `test` uses the same gate

| [INDEX] | [RULE_CLASS]           | [MECHANISM]                                                                                            |
| :-----: | :--------------------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | Banned construct       | Construct kinds with `ignores:` on exempt boundary globs, or `not: { inside: <marker-comment rule> }`  |
|  [02]   | Required shape         | Owner kind with `not: { has: <required child, argument, or modifier> }`                                   |
|  [03]   | Entry-point discipline | Declaration-name `regex` over mode-suffix and option grammars, single-hop forwarders as patterns       |
|  [04]   | Layer boundary         | One rule per forbidden edge: import kind + path `regex`, `files:` scoping the consumer layer           |
|  [05]   | Policy literal         | Literal kinds `inside` policy argument and initializer positions, named constants own the values       |
|  [06]   | Dispatch shape         | Dispatch kind `inside` a dispatch arm, catch-all arms beside sealed-hierarchy arms                     |
|  [07]   | Naming grammar         | Name-position `regex`: word budget, banned generic suffixes, role-suffix bijection via `not: has`      |

- Unparseable rules or duplicate ids abort the whole scan, inline `---` bundles tolerate both, drafts stay outside `ruleDirs`
- `sgconfig.yml` accepts unknown keys silently, scoping uses per-rule `files:`/`ignores:` only
- `language:` is single-valued, similar languages share rules via a `languageGlobs` superset entry, embedded languages use `languageInjections`
- Suppression is rule-scoped, `ast-grep-ignore: <rule-id>` on the next or same line, the `--error` built-ins keep unscoped and stale waivers fatal
- Whole-file rules waive only file-wide, the suppression comment on line 1 over an empty line 2, a line-scoped comment joins the match
- `files:` takes no `!` negation (silently inert), exclusion is an `ignores:` entry, a dot-directory scope is dead without `--no-ignore hidden`
- Green can prove nothing: omitted `severity` defaults to `hint`, `test` passes zero cases, a `files:` rule under `scan -r` scans nothing
- Rules matching no `invalid:` fixture are dead, `test` never notices a missing test file, rule-to-test pairing proves outside the gate
- `metadata:` holds routing facts (owner, decision id) and appears under `--json --include-metadata`
