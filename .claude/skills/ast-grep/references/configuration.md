# [CONFIGURATION]

Register durable ast-grep configuration through the root `sgconfig.yml`. Keep one rule per file and one project-wide id per rule.

```text
sgconfig.yml                                       # Project config the scan and the test runner read at the root
rules/<language>/<package>/<rule-id>.yml           # Directories name the package or syntax the rules read, one rule per file, the id is the stem
utils/<language>/<util-id>.yml                     # Global utils with explicit id and language, shadowed by a local utils: entry of the same id
rewrites/<language>/<package>/<id>.yml             # On-demand rewrites under a second ruleDirs entry, the fix performs the correction, ids read <before>-to-<after>
tests/<language>/<package>/<rule-id>-test.yml      # Test bound to its rule by id, one file per rule, the tree matches rules/
tests/rewrites/<language>/<package>/<id>-test.yml  # Run by ast-grep test --include-off, one file per rewrite, the tree matches rewrites/
tests/__snapshots__/<rule-id>-snapshot.yml         # Written by ast-grep test -U --filter '^<id>$', flat, rewrite snapshots beside the rest
```

## [01]-[REGISTRATION]

- Resolve `ruleDirs`, `utilDirs`, and `testDir` relative to `sgconfig.yml`, and `snapshotDir` relative to `testDir`, defaulting to `__snapshots__`
- Keep `snapshotDir` within its `testDir`, a parent path (`../saved`) can pass snapshot generation but fail native baseline discovery
- Entries of `testConfigs` sharing one `snapshotDir` fail `No <id> baseline found` on the snapshot `-U` wrote, one entry per snapshot directory
- Register `customLanguages.<name>` with `libraryPath` and `extensions`, with optional `expandoChar`, `languageSymbol`, and `outlineRules`
- Register directories containing rules or utilities, a missing `ruleDirs` or `utilDirs` directory aborts the scan, an empty one holds a `.gitkeep`
- Share rule sets through a submodule or package
- Symlinked directories under `ruleDirs` load, a symlinked rule file is skipped with `Configuration not found!`, a hard link loads
- `files:` globs never match a path reached through a symlinked directory component, a scoped case under a linked tree reads as no hit
- `files:` globs match the path relative to `sgconfig.yml`, or to the working directory under `--inline-rules`
- Wildcard globs take an implied `**/` prefix, a plain file name matches the one file beside `sgconfig.yml`, and `**/<name>` every file so named
- A `./` prefix or a `!` glob in `files:` matches nothing, exclusion is `ignores:`, and `scan -r` reads globs relative to the rule file
- Dot directories beneath a scope need `--no-ignore hidden`, and a dot-directory root named on the command is walked

## [02]-[LANGUAGES]

Build custom parsers with `tree-sitter build --output <library>` from the grammar directory. `libraryPath` accepts a path or a map from Rust target triples to paths when distributing platform-specific libraries. `languageSymbol` defaults to `tree_sitter_<name>`. Choose an `expandoChar` accepted in the grammar's identifier positions. XML patterns use `_NAME` with `_`, while fixes and transforms refer to `$NAME`. Keep the grammar build in the existing toolchain target.

`outlineRules` names one extractor file relative to `sgconfig.yml`. Add independent extractors through repeated `--outline-rules <file>` arguments, configured extractors load first. Keep one extractor per file.

- `sgconfig.yml` accepts unknown keys silently, scoping uses per-rule `files:`/`ignores:` only
- `language:` is single-valued, `languageGlobs` overrides extension detection for shared grammars, and `languageInjections` parses embedded code
- Injected regions parse as the injected language, a `kind: program` rule fires per region and takes `files: ['**/*.sh']` to stay on scripts
- Pass the host `-l <language>` when inspecting its parse, a run without it can report injected-language recovery instead
- Scanned sources take the `language` the `languageGlobs` entry names for their files, `--inspect entity` lists it and `--filter` counts a known hit

Capture the complete embedded source as `$CONTENT`. Set `injected` to a language name for a fixed parser or a candidate list with `$LANG` captured from source for dynamic selection. For a tag pattern `$LANG` followed by a template containing `$CONTENT`, candidates `[css, javascript]` select only those tags. Custom languages can be injection targets after registration.

Injections parse source ranges without decoding host strings, so the root configuration injects a plain or literal block YAML scalar and a JSON string with no `escape_sequence`, and a quoted or folded YAML scalar and an escaped JSON string stay outside. A rewrite over an injected region keeps the host quoting, delimiters, indentation, and offsets, and the parsed host and the executed value are read after it.

The root configuration holds two injections scoped to the execution forms the repository uses, because a `run` or `command` key alone occurs in data. The YAML injection reads the `run` scalar of a workflow job step, a `parallel` group included, or a composite action step as bash, the shell every step declares, and actionlint resolves a step in another shell itself. The JSON injection reads the `command` string and the `commands` items of a target under `targets` or `targetDefaults` at the document root or under the `nx` field, and reads no `env` value and no `metadata` entry. The run-commands executor spawns `sh`, so the Bash rewrites under `rewrites/bash` carry `files:` that keep them out of JSON, and the shell rules read both hosts.

Each injection entry declares its ownership predicates in its own `utils` map beside `rule` and `injected`, because injection compilation loads no project `utilDirs`, and a kind list inside a flow map is quoted (`{kind: 'block_mapping_pair, flow_pair'}`), because an unquoted second kind reads as a key. `tests/typescript/ast-grep/shell-injections.test.ts` proves the selected regions and the excluded data through `no-npm-command` and a Bash rewrite. Injected findings use the host path and host-relative ranges, and repeated regions take repeated rewrite passes, the remaining matches read after each.

## [03]-[RULES]

- Group rules by language and package (`effect`, `pulumi`) or syntax category, mirroring their paths under `tests/`
- Subdivide directories by module or construct when navigation needs it, preserving rule ids
- Files hold one rule each, the id is the file stem, unique across every language, in `no-<construct>` or `require-<shape>` form
- Check id uniqueness with `rg -l '^id: <id>$' tools/ast-grep/{rules,utils,rewrites}` before creating a file
- Fixes that import a package skip the host globs that load no package import (`.claude/plugins/**`) through `ignores:`
- Rules over a domain form skip `*.spec.ts`, `*.test.ts`, and `*.bench.ts`, because a spec translates the value at its assertion boundary
- Rewrites under `rewrites/` are `severity: off` (no plain `scan` or `scan -r` runs them), hold `fix`, `message`, and `note`, one correction each
- Rules hold one `fix` template, and a correction with a second template is its own rewrite file
- A titled fix list in a rewrite offers the choices, and `-U` takes the first
- Rewrite ids read `<before>-to-<after>` in the vocabulary of the package they read, `flat-map-to-map`, `boolean-statement-to-if`
- Apply a rewrite through `nx run rasm:rewrite -- --filter='^<id>$' --error=<id> <path>`, the root target over `ast-grep scan -U`
- Re-run the rewrite until `Applied N changes` stops, nested matches rewrite outer-first per pass
- After applying, scan without `-U`: no remaining matches exit 0, and an unknown id exits 3
- Rewrites report only what they write
- Share predicates across rewrites of one construct and distinguish their cases, sequencing dependent corrections explicitly

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

- One `labels:` entry per rule, because two entries serialize in random order and the snapshot run flakes, and a second span is its own rule and case
- Names bound only under `not:` expand empty, `note` and a label `message` interpolate nothing, and `labels` take rule and constraints captures alone

Integrate the durable rule in sequence:
1. Adapt the matching template, share repeated predicates through `utils`, and set `constraints` and relation `stopBy` where needed
2. Write `message` naming the violation and `note` stating the correction, and add `fix` after establishing replacement behavior
3. Route an import the fix needs through the header rewrite, because it blocks every other fix in the file
4. Write the test file with matching id, the corrected code and each near miss under `valid:`, the instance and each sibling under `invalid:`
5. Comment each case with the shape it covers, run `ast-grep test -U --filter '^<id>$'` to write the snapshots, and keep them
6. Read a fix as the snapshot's fixed text, and an `expandStart`/`expandEnd` consumption in `scan --json` `replacementOffsets` alone
7. Place the rule in its directory, `ast-grep scan --inspect entity` proves registration, `--filter '<rule-id>'` iterates it alone
8. Scan the codebase, read every hit as a real finding or a rule defect, and correct the code or the rule before the rule joins the gate
9. Check the correction the `note` prescribes against every other gate, a data-last step another gate rejects binds a local and stays data-first
10. Gate with `ast-grep scan --error=unused-suppression --error=no-suppress-all` and `pnpm exec nx run rasm:rules:<ext>`

| [INDEX] | [RULE_CLASS] | [MECHANISM] |
| :-----: | :----- | :----- |
| [01] | Banned construct | Construct kinds with `ignores:` on exempt boundary globs, or `not: { inside: <marker-comment rule> }` |
| [02] | Required shape | Owner kind with `not: { has: <required child, argument, or modifier> }` |
| [03] | Entry-point discipline | Declaration-name `regex` over mode-suffix and option grammars, single-hop forwarders as patterns |
| [04] | Layer boundary | One rule per forbidden edge: import kind + path `regex`, `files:` scoping the consumer layer |
| [05] | Policy literal | Literal kinds in their owning argument or initializer positions, boundary contracts establish the values |
| [06] | Dispatch shape | Dispatch kind `inside` a dispatch arm, catch-all arms beside sealed-hierarchy arms |
| [07] | Naming grammar | Name-position `regex`: word budget, banned generic suffixes, role-suffix bijection via `not: has` |
| [08] | Self-nesting | Same-family calls nested as arguments, or a run call inside one, the outermost reported through `not: inside` |
| [09] | Repeated fact | Pair bound on the first row, `not: {has: <row>, not: {has: <bound pair>}}` proves every row repeats it |

## [04]-[UTILITIES]

- Share repeated structural predicates through local utils within a rule and global utils across rules, and keep unique logic at its caller
- Parameterize shared predicates where callers supply different structural slots, retaining fixed predicates without unused arguments
- Global utils are named `<package>-<shape>` and hold `id`, `language`, `arguments`, `rule`, `constraints`, `utils`, and `transform`, no `fix`
- Global util ids share one namespace across languages, a second language's util of one id fails the load as `Duplicate rule id`
- Syntax utils of a second language take the language name as their package, `python-function-boundary` beside `syntax-function-boundary`
- Share construct predicates between diagnostics and rewrites, and place replacement-specific conditions in each caller
- Consumers name the util directory under `utilDirs`, and `scan -r`, `--inline-rules`, and the MCP load none
- Parameterized utils take `arguments` at the global level alone, every argument is mandatory, and a string `matches: <id>` of one exits 8
- Rules call a parameterized util as `matches: {<util-id>: {<arg>: <rule>}}`, each argument a rule, and calls under one `matches` combine as `all`
- Argument rules match in an isolated environment and export captures only after the parameterized rule succeeds
- Conflicting exports fail the call without retrying another `any:` branch, prove equality through caller captures or shared argument-slot exports
- Argument rules can be a `matches` to a zero-argument util or a parameterized call, and two calls of one util in a rule bind apart
- Parameterized utils call another at the rule root or under `all`, `any`, or `not`, forwarding a slot as `{<slot>: {matches: <own-slot>}}`
- Parameterized calls under `has` or `inside` in a util file record no load-order edge and fail random loads as `Rule <id> is not defined`
- Slots the called util's body matches export their captures, a forwarded slot exports none and the caller binds it by its own `has`
- String `matches: <name>` resolves a parameter, then a local util, then a global util
- Local utils inside a global util read its arguments
- Direct self-reference in a global utility fails cycle validation, including string and parameterized calls
- Zero-argument global utils recurse into themselves and each other through `has` or `inside`, and a global util file holds a local `utils:` block
- Local utils declare no `arguments`, an inline copy of a parameterized util exits 8, and a draft naming one sits under a scratch `ruleDirs`
- Utils rooted on a kind `any:` take a further alternative under `all: [{any: ...}]`, a sibling `any` key fails the load as `duplicate field any`
- `scan -c <config> <path>` and `test -c <config>` run from any directory, and a scratch config counts a util alone through a rule `matches: <id>`
- Scratch configs name `ruleDirs` and `testConfigs` at the family and `utilDirs` at the language directory, and a custom language they lack fails
- Local utility captures reach the caller's `fix`, while global utility-owned captures remain private
- Argument rules bind their captures at the call site (`source: {pattern: $SOURCE, regex: '<re>'}`, `pattern: _NAME` under an `expandoChar`)
- Captures an argument rule binds reach the caller's `fix`, `message`, and `labels`, and a private utility capture proves no caller equality

## [05]-[EXECUTION]

- Unparseable rules or duplicate ids abort the whole scan, an inline `---` bundle tolerates duplicate ids alone, and drafts stay outside `ruleDirs`
- Overlapping fixes keep the lower rule id, and a `program`-level fix blocks the other fixes
- Drafts, fixtures, and scratch configs sit under the scratchpad in a directory named for the agent, because one session's agents share it
- Quote scalars containing `: ` or commas inside flow maps, or use a block scalar, malformed YAML aborts configuration loading
- Each rule file proves by `ast-grep scan --filter '^<id>$' <path>` on the real tree before adding the next rule
- Sibling files mid-edit fail every load of the root config, and test the family through `-c <scratch>/sgconfig.yml` over the real directories
- Per-rule proofs are `rule-checks.sh gate <ext> '^<id>$'`, and the whole gate runs once per completed family as `pnpm exec nx run rasm:rules:<ext>`
- Rule cost is measured under a one-rule scratch config, and the root-tree number adds the tree's load under the 100 ms bar of the edit-time hook
- Scratch projects take the `languageGlobs` entry of the root `sgconfig.yml`, because a `.ts` file with no entry parses as `typescript`
- Under the `typescript` parse every `tsx` rule and every `-l tsx` run finds nothing in the scratch project
- Confirm selected rule and test counts, omitted `severity` is `hint`, `--min-severity` drops rules, and `test` can pass zero cases
- Each invalid case holds one intended diagnostic, and distinct behavioral branches have distinguishing cases
- `metadata:` holds routing facts and appears under `--json --include-metadata`, `url:` shows in the editor and SARIF and never in `--json`

## [06]-[SUPPRESSION]

- Suppression is rule-scoped, `ast-grep-ignore: <rule-id>` opens the comment on the same, last, or preceding line of the match
- `ast-grep-ignore: <rule-id>, <reason>` keeps the reason after the comma, and an id with no corresponding match reports as `unused-suppression`
- `unused-suppression` is `hint` while no `--filter`, `--off`, or `--min-severity` narrows the rules, `no-suppress-all` is `off`, `--error=` gates
- Whole-file rules waive only file-wide, the suppression comment on line 1 over an empty line 2, a line-scoped comment joins the match
- Suppression binds to the match's first or last line, and a rule reports the smallest offending node to place the waiver beside the defect
- Whole-file rules prove their waivers under `scan --error=unused-suppression`, exit 0 when every mark matches a hit
