# [CONFIGURATION]

Durable ast-grep configuration registers through the root `sgconfig.yml`.

```text
sgconfig.yml                                       # Project config scan and test read at the root
rules/<language>/<package>/<rule-id>.yml           # Rules grouped by language and the package or syntax they read
utils/<language>/<util-id>.yml                     # Global utils with explicit id and language, shadowed by a local utils: entry of the same id
rewrites/<language>/<package>/<id>.yml             # On-demand rewrites under their own ruleDirs entry, ids read <before>-to-<after>
tests/<language>/<package>/<rule-id>-test.yml      # Test bound to its rule by id, one file per rule, the tree matches rules/
tests/rewrites/<language>/<package>/<id>-test.yml  # Rewrite tests, one file per rewrite, the tree matches rewrites/
tests/__snapshots__/<rule-id>-snapshot.yml         # Flat, rewrite snapshots beside the rest
```

## [01]-[REGISTRATION]

- `ruleDirs`, `utilDirs`, and `testDir` resolve relative to `sgconfig.yml`, `snapshotDir` relative to `testDir`, default `__snapshots__`
- `customLanguages.<name>` holds `libraryPath` and `extensions`, with optional `expandoChar`, `metaVarChar`, and `outlineRules`
- Missing `ruleDirs` or `utilDirs` directories abort the scan, empty ones hold a `.gitkeep`
- `sgconfig.yml` accepts unknown keys silently
- Scoping is per-rule `files:` and `ignores:`
- `files:` globs match relative paths from the config directory, `scan -r` reads them from the rule file
- Wildcard globs take an implied `**/` prefix, a plain file name matches the one file beside `sgconfig.yml`, and `**/<name>` every file of that name
- `./` prefixes and `!` globs in `files:` match nothing, exclusion is `ignores:`
- Dot directories under a scope need `--no-ignore hidden`, `lint` target passes it for `.github`

## [02]-[LANGUAGES]

- `grammar` target builds the XML parser with `tree-sitter build --output .cache/ast-grep/xml.so`, every run aborts until the file exists
- `expandoChar` is a character the grammar accepts in identifier positions
- `outlineRules` names one extractor file relative to `sgconfig.yml`
- `languageGlobs` overrides extension detection, `.ts` files parse as `tsx`, `NuGet.config` as `xml`
- Injected regions parse as the injected language, `kind: program` rule fires per region and takes `files: ['**/*.sh']` to stay on scripts
- When inspecting a host parse, pass `-l <language>`, a run without it can report injected-language recovery

Injection entries capture the embedded source as `$CONTENT` and name the parser in `injected`, a language or a candidate list with `$LANG`:
- Injections parse source ranges without decoding host strings, plain and literal block YAML scalars and JSON strings with no `escape_sequence`
- Rewrites over an injected region keep host quoting, delimiters, indentation, and offsets, the parsed host and executed value read after them
- YAML injection reads the `run` scalar of a workflow job step, `parallel` groups included, or a composite action step as bash, the declared shell
- JSON injection reads `command` and `commands` of a target under `targets` or `targetDefaults` at the root or under `nx`, no `env` or `metadata`
- Run-commands executor spawns `sh`, Bash rewrites under `rewrites/bash` hold `files:` keeping them out of JSON
- Shell rules read both hosts
- Injection entries hold ownership predicates in their `utils` map beside `rule` and `injected`, injection compilation loads no `utilDirs`
- Kind lists inside a flow map are quoted (`{kind: 'block_mapping_pair, flow_pair'}`), an unquoted second kind reads as a key
- Injected findings use the host path and host-relative ranges

## [03]-[RULES]

- Ids are unique across languages
- Ids take `no-<construct>` or `require-<shape>` form
- Before creating a file, check id uniqueness with `rg -l '^id: <id>$' tools/ast-grep/{rules,utils,rewrites}`
- Fixes importing a package skip host globs loading no package import (`.claude/plugins/**`) through `ignores:`
- Rules over a domain form skip `*.spec.ts`, `*.test.ts`, and `*.bench.ts`, a spec translates the value at its assertion boundary
- Rewrites under `rewrites/` are `severity: off`, no plain `scan` or `scan -r` runs them
- Rewrites hold `fix`, `message`, and `note`, one correction each
- Rules hold one `fix` template, a correction with another template is its own rewrite file
- Scan without `-U` after the last rewrite pass exits 0 on no remaining match and 3 on an unknown id

```yaml
id: <rule-id>
language: <language>
severity: error
files: ['<scope-glob>']
utils:
  <util-id>: { <family-shape> }
rule: { <search rule> }
constraints: { <VAR>: { regex: '<grammar>' } }
fix: <template>
message: <one line naming the violation, captures and transform variables interpolate>
note: <the correction as the shape to produce>
labels: { <VAR>: { style: primary, message: '<span fact>' } }
```

- One `labels:` entry per rule, more entries serialize in random order and break the snapshot run
- Second spans take their own rule and case
- Names bound under `not:` alone expand empty
- `note` and a label `message` interpolate nothing
- `labels` take rule and constraints captures alone

Integrate a durable rule in sequence:
1. Adapt the matching template, share repeated predicates through `utils`, and set `constraints` and relation `stopBy`
2. Write `message` naming the violation and `note` stating the correction, add `fix` once the replacement is proven
3. Put an import the fix needs in the header rewrite
4. Write the test file with the matching id and accept its snapshots
5. Place the rule in its directory and prove registration with `ast-grep scan --inspect entity`
6. Scan the codebase, classify every match as a finding or a rule defect, and correct the code or the rule before committing the rule
7. Check the corrected form under every other checker of the language
8. Prove with `ast-grep scan --error=unused-suppression --error=no-suppress-all` and `pnpm exec nx run rasm:rules`

| [INDEX] | [RULE_CLASS]           | [MECHANISM]                                                                                                   |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------------------------------ |
|  [01]   | Banned construct       | Construct kinds with `ignores:` on exempt boundary globs, or `not: { inside: <marker-comment rule> }`         |
|  [02]   | Required shape         | Owner kind with `not: { has: <required child, argument, or modifier> }`                                       |
|  [03]   | Entry-point names      | Declaration-name `regex` over mode-suffix and option grammars, one-call forwarders as patterns                |
|  [04]   | Layer boundary         | One rule per forbidden edge: import kind with path `regex`, `files:` scoping the consumer layer               |
|  [05]   | Policy literal         | Literal kinds in their owning argument or initializer positions, boundary contracts establish the values      |
|  [06]   | Dispatch shape         | Dispatch kind `inside` a dispatch arm, catch-all arms beside sealed-hierarchy arms                            |
|  [07]   | Naming grammar         | Name-position `regex` over word limit and banned generic suffixes, role-suffix bijection through `not: has`   |
|  [08]   | Self-nesting           | Same-family calls nested as arguments, or a run call inside one, the outermost reported through `not: inside` |
|  [09]   | Repeated fact          | Pair bound on the first row, `not: {has: <row>, not: {has: <bound pair>}}` proves every row repeats it        |

## [04]-[UTILITIES]

- Global utils are named `<package>-<shape>` and hold `id`, `language`, `arguments`, `rule`, `constraints`, `utils`, and `transform`, no `fix`
- Global util ids share one namespace across languages, one id in two languages fails the load as `Duplicate rule id`
- Syntax utils of another language take the language name as package, `python-function-boundary` beside `syntax-function-boundary`
- Consumers name the util directory under `utilDirs`, `scan -r`, `--inline-rules`, and the MCP load none
- `--inspect entity` emits rule and file entities alone, a calling rule's `entity|rule` line proves a util's registration
- Utils under `not:` or `inside:` supply no kind to their caller, a kindless caller aborts with `Rule must specify a set of AST kinds to match`
- Parameterized utils take `arguments` at the global level alone
- Arguments are mandatory, a string `matches: <id>` of a parameterized util exits 8
- Rules call a parameterized util as `matches: {<util-id>: {<arg>: <rule>}}`, each argument a rule
- Calls under one `matches` combine as `all`
- Argument rules match in an isolated environment and export captures after the parameterized rule succeeds
- Conflicting exports fail the call without retrying another `any:` arm, prove equality through caller captures or shared argument-slot exports
- Argument rules can be a `matches` to a zero-argument util, a caller's local util included, or a parameterized call
- Separate parameterized calls bind apart
- Parameterized utils call another at the rule root or under `all`, `any`, or `not`, forwarding a slot as `{<slot>: {matches: <own-slot>}}`
- Parameterized calls under `has`, `inside`, or `ofRule` in a util record no load-order edge and fail random loads as `Rule <id> is not defined`
- Slots the called util's body matches export their captures, a forwarded slot exports none and the caller binds it by its own `has`
- String `matches: <name>` resolves a parameter, then a local util, then a global util
- Local utils inside a global util read its arguments and declare no `arguments`
- Inline copies of a parameterized util exit 8
- Direct self-reference in a global util fails cycle validation, string and parameterized calls included
- Zero-argument global utils recurse into themselves and each other through `has`, `inside`, or `follows` under `stopBy`
- Utils rooted on a kind `any:` take a further alternative as a two-item `all:` of both lists, a sibling `any` key fails as `duplicate field any`
- Global utils called under `constraints` alone need no kind set
- Local util captures reach the caller's `fix`, global util captures stay private, a private capture proves no caller equality
- `$NAME` in a local util pattern unifies with the caller's capture of that name, a rewriter calling the util binds it again
- Captures an argument rule binds reach the caller's `fix`, `message`, and `labels`

## [05]-[EXECUTION]

- Unparsable rule files of any language and duplicate ids fail every load of the root config, `scan` and `test` included
- Quote scalars holding `: ` or a comma inside a flow map, or use a block scalar, malformed YAML aborts the load
- `pnpm exec nx run rasm:rules` runs `ast-grep test --include-off`
- Root `lint` target scans `tools` under the yaml family
- Omitted `severity` is `hint`, `--min-severity` drops rules
- `scan --inspect summary <dir> 2>&1 >/dev/null | rg RuleCount` prints `effectiveRuleCount` and `skippedRuleCount`, the `severity: off` rules skipped
- `metadata:` holds routing facts and appears under `--json --include-metadata`

## [06]-[SUPPRESSION]

- Suppression is rule-scoped, `ast-grep-ignore: <rule-id>` opens the comment on the same, last, or preceding line of the match
- `ast-grep-ignore: <rule-id>, <reason>` keeps the reason after the comma
- `unused-suppression` reports an id with no match at `hint` while no `--filter`, `--off`, or `--min-severity` narrows the rules
- `no-suppress-all` is `off`, `--error=<id>` gates both
- Rules report the smallest offending node, the suppression comment sits beside the defect
- Whole-file rules waive file-wide alone, the comment on line 1 over an empty line 2, proven by `scan --error=unused-suppression` exit 0
