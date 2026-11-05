# [CONFIG]

`sgconfig.yml` decides what a command loads, which parser reads each file, and which regions parse as a second language.

## [01]-[DISCOVERY]

- `ast-grep` loads the `sgconfig.yml` the nearest ancestor of the working directory holds, `sgconfig.yml` beats `sgconfig.yaml`, a run outside the tree reads none
- Paths inside a config resolve from its canonical folder, a symlinked config resolves from its target's folder
- `-c` naming a missing file or a directory exits 6
- Empty `sgconfig.yml` files are valid, the schema requires `ruleDirs` and the loader defaults it, `scan` with no rules exits 0 silently
- Missing `ruleDirs` or `utilDirs` folders exit 6 on `scan`, `run` reads neither
- `ruleDirs` and `utilDirs` walkers skip hidden paths and honor `.ignore`, `noIgnore` never reaches them, a hidden folder named as the root loads
- `noIgnore` governs source walks of `scan`, `run`, and `outline`, `noIgnore: [hidden]` includes dot directories, nonempty CLI `--no-ignore` lists replace it
- Unknown keys load silently at the top level, in a language row, and on an injection entry, inside `rule:` they exit 8
- `testConfigs: [tests]` exits 8, `testConfigs` entries hold `testDir` with `snapshotDir` resolving under it, a project with none prints `Running 0 tests` at exit 0

Load runs in a fixed order, the first failure names its exit:

| [INDEX] | [STEP]                              | [FAILURE]                                      | [EXIT]  |
| :-----: | :---------------------------------- | :--------------------------------------------- | :------ |
|  [01]   | YAML parse of `sgconfig.yml`        | Syntax, alias budget, duplicate key            | `8`     |
|  [02]   | Custom library load                 | Missing file, platform map without host triple | `79`    |
|  [03]   | `languageGlobs`                     | Unknown language `33`, invalid glob `8`        | `33`, 8 |
|  [04]   | `languageInjections` in entry order | Unknown `hostLanguage` `1`, rule error `8`     | `1`, 8  |
|  [05]   | CLI argument parse                  | Unknown language or flag                       | `2`     |
|  [06]   | `utilDirs`                          | Parse or compile error, second `---`           | `1`     |
|  [07]   | `ruleDirs`, `-r`, `--inline-rules`  | Parse or compile error                         | `8`     |

- `--version`, `--help`, and subcommand `--help` exit 0 over any config failure, `completions` exits with it
- Non-TTY `--custom-languages trust --version` exits 0 and writes no store, with `run` it exits 7

Exit codes of a completed command:

| [INDEX] | [EXIT] | [CONDITION]                                                                                    |
| :-----: | :----- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `0`    | `scan` without an `error` match, `[]` under `--json`, missing path printing `ERROR: <path>: …` |
|  [02]   | `1`    | `scan` with an `error` match, `run` without a match or with a missing path, `outline` load     |
|  [03]   | `3`    | `--filter` matching no rule id, a filter matching only `severity: off` rules exits 0           |
|  [04]   | `101`  | Non-alphanumeric custom name or empty `extensions` once `-l` or a rule names that language     |

- Exit 1 marks load failures (unknown `hostLanguage`, a util parse error) beside findings, a script separates them on stderr or `--json`
- `--filter` is an unanchored regex over ids, an invalid regex exits 2, `--error=<unknown id>` is ignored

## [02]-[TRUST]

Custom-language libraries load under `--custom-languages allow` for one call or through trust in a canonical config path:
- Default `ignore` skips only the library, a name it would supply fails, `-l` at exit 2, a rule at 8, `languageGlobs` at 33, `hostLanguage` and a util at 1
- `injected` names of a skipped library drop silently
- Explicit `ignore` beats a trust entry, a corrupt store reads as untrusted with the same warning
- `ast-grep --custom-languages trust --version` records trust from an interactive terminal, `AST_GREP_TRUST_DIR` relocates the store
- `revoke` forgets trust and works without a TTY
- `--config` naming another file loads untrusted, a symlink shares its target's canonical trust key
- Subcommands precede options, default-run detection can read a later subcommand as a path

## [03]-[LANGUAGES]

File-to-language resolution:
- `languageGlobs` reads the basename alone (`src/*.foo` matches nothing, `*.baz` and `exact.name` match at any depth) and beats extension detection
- Overlaps change winner per process, `customLanguages` and `languageGlobs` load from `HashMap`s, one extension or glob owns one language
- `-l` never forces a parser in path mode, it selects files already resolved to that language, `languageGlobs` or `--stdin` changes the parser
- `-l` and MCP `language` take configured names, a built-in name misses files mapped to a custom replacement
- Rules of `language: json` under a custom `json` mapping report `appliedRuleCount=0` with no error
- Rule languages widen the walk to every host injecting them, through `languageGlobs` hosts as well

`customLanguages` rows:
- Names are alphanumeric and case-sensitive, names resolve built-in languages first and extensions custom languages first, built-in aliases fold case
- Extensions take no dots, `a.b` and `.dot` walk and resolve to no language, `languageGlobs: ["*.a.b"]` holds them, injection-only languages keep a placeholder extension
- `libraryPath` takes one path or a map per target triple, `languageSymbol` defaults to `tree_sitter_<name>`
- `metaVarChar` works and sits outside the schema, fixes and transform sources take it in place of `$`, `expandoChar` defaults to it and holds one character
- Patterns rewrite each sigil to `expandoChar`, literal text of that character before a capital reads as a capture (`Q`: `{"QA": 1}` matches `{"b": 1}`)
- Parser-valid `expandoChar` values are grammar facts, an ASCII identifier class forces an ASCII choice
- Literal `$0` and lowercase dollar names survive preprocessing
- `outlineRules` reads under load alone, only `outline` fails on a missing file

## [04]-[YAML]

`sgconfig.yml`, rule files, and catalogs share one `serde-saphyr` budget, catalogs at 250,000 nodes, 1,000,000 events, and 64 nesting levels:
- Past 100 aliases across a file `aliases <= anchors × 10` holds, else exit 8 with `AliasAnchorRatio`
- Repeated `has` nests reach 64 levels, `all` lists flatten them
- Duplicate keys, a second `---` in a one-document file, tags, and forward aliases exit 8
- `<<` merges nothing: filling required fields exits 8 `missing field`, an extra entry key loads silently ignored
- `<<` inside `rule:` exits 8 `unknown field`, under `utils` or `transform` it loads as an entry named `<<`
- Unread top-level keys hold anchors (`x-shared: [&site {...}]`), schema-validating editors flag them
- Anchors save source text and no load cost, aliases replay into the node count and an aliased entry compiles as a written-out one
- Whole-map aliases compile their unread utils at each use
- Flow and block forms load equally, a leading backtick in a plain value and `"\d"` exit 8

## [05]-[SELECTION]

Rule scope:
- `files:` and `ignores:` drop a rule per path after the file parses, `ignores:` reads first, CLI `--globs` alone skips parsing
- Globs root at the config directory for configured rules, the rule file's folder under `-r`, the working directory under `--inline-rules`
- Paths outside that root stay absolute, `**/src/a.js` matches and `src/a.js` misses
- `*` crosses `/` (`src/*.js` matches `src/deep/b.js`, `*.js` matches every path), a plain file name matches the one file at that root
- `./src/**` and a folder name match nothing
- Glob entries take a string or `{glob, caseInsensitive}`
- `files:` on an injected document reads the host path

CLI scope:
- `--globs` roots at the working directory, later globs win, broad inclusions precede exclusions
- Positive globs accept an ignored file and no pruned directory, `ig/**` finds nothing until `--globs ig` joins it
- Explicit file paths bypass hidden, ignore files, negative `--globs`, and the type filter
- `.gitignore` applies inside a Git repository alone, `.ignore` always
- `--off` or zero rules still walks every file

Stdin:
- `--stdin` parses with the first rule's language and never injects, on `run` and `scan`
- Configured rules never run on stdin, a one-rule project exits 3, stdin scans take `-r` or `--inline-rules`
- `--inline-rules` with `--stdin` reports every inline rule, `--filter` selects among them
- `severity: off` rules report on stdin at exit 0, `--min-severity hint` or higher excludes them

Inspection:
- `--inspect entity` prints one `language=` line per document under `scan`, regions no rule reads print `appliedRuleCount=0`
- `appliedRuleCount` tests the working-directory path where matching uses the project path, a run from a subfolder undercounts, read matches for truth
- `--inspect summary` prints file and rule counts, `skippedRuleCount` counts `severity: off` and `--filter` drops for project rules and `off` alone under `-r` and `--inline-rules`

Suppression and severity:
- `unused-suppression` reports a comment suppressing nothing at `hint`, `--filter`, `--off`, `--min-severity`, `-r`, or `--inline-rules` disables it
- `no-suppress-all` reports an idless comment and is disabled, `--error=<id>` raises either built-in rule
- `--error`, `--warning`, `--info`, `--hint`, and `--off` set one run's severity per rule id, `--format github` and `--format sarif` print findings for CI

## [06]-[INJECTIONS]

Entries capture embedded source as `$CONTENT` and name its parser in `injected`, a built-in language, `customLanguages` key, or list:
- `injected` lists set the walk and inferred-`run` matchers alone, `$LANG` takes the parser and beats a static value, off-list names parse
- List entries without a `$LANG` capture inject nothing
- `$LANG` resolves raw node text, built-in aliases fold case, a quoted `"css"` or unknown name drops silently, the capture takes `string_fragment` over `string`
- `transform` cannot supply `LANG` or `CONTENT`, only single-node captures read, `$$$CONTENT` injects nothing
- Captures reach through relations and local utils, one entry binds them per `any` arm
- Missing `$CONTENT`, `$$$CONTENT`, and an unknown `injected` load at exit 0 and inject nothing, `id`, `fix`, and unknown keys are ignored
- `constraints` filter entries, `utils` qualify them, injection compilation loads no `utilDirs`

Host resolution:
- `hostLanguage` compares a file's resolved language, `ts` skips `.tsx`, `json` never fires once `.json` maps to a custom language
- `ts` and `TypeScript` entries merge on one host
- Hosts with a string content child capture it (JSON `string_content`), a hidden content token takes an alias first, else `$CONTENT` holds delimiters
- `hostLanguage: HTML` adds to native extraction, a `style` entry beside `<style>` gives a second CSS document
- `run -l <injected>` selects other-language hosts, same-language reinjection needs inferred-language `run` or `scan`

Documents:
- Each `$CONTENT` match is one document over the full host source with one included range, nothing merges across matches
- Hole-bearing regions reach the parser as source, grammar-level hole tokens parse them
- Injected coordinates are host coordinates, nothing maps back
- Root-kind rules report the first token, a region's bounds come from the `$CONTENT` kind
- Injections parse raw captured source without decoding strings, removing block-scalar headers, or deindenting
- Injected documents never reinject, one entry yields nested regions separately through `find_all`, `` `x ${ `y` } z` `` parses each template apart
- Macro token trees follow their macro grammar, host-language reinjection expands no macro

Consumers:
- Nothing dedupes across documents, entries overlapping one region or an alias repeating an entry print and count each finding per document
- Same-language injected ranges parse again, a leaf rule reports from each document unless it excludes one side
- Suppression comments apply per document, a host `// ast-grep-ignore: <id>` leaves an injected finding and adds `unused-suppression`
- Comments inside the region suppress its findings
- `-U` writes the whole file once per document from original text, the last document wins, each later pass persists one more
- Rewrites across documents take reruns to a clean scan
- LSP reads injections, `test` does not

Dynamic parser under one entry, the list bounding the walk and the capture choosing the parser:

```yaml
languageInjections:
    - hostLanguage: js
      injected: [css, html]
      rule:
          kind: call_expression
          all:
              - has: {field: function, has: {field: property, pattern: $LANG}}
              - has: {field: arguments, kind: template_string, has: {kind: string_fragment, pattern: $CONTENT}}
```

Over `` tag.css`.a { top: 0 }` ``, `` tag.html`<b>x</b>` ``, and `` tag.python`x = 1` `` with one rule per language, `--inspect entity` prints each document, and `tag.python` parses though the list omits it:

```text
sg: entity|file|a.js: language=JavaScript,appliedRuleCount=0
sg: entity|file|a.js: language=Css,appliedRuleCount=1
sg: entity|file|a.js: language=Html,appliedRuleCount=1
sg: entity|file|a.js: language=Python,appliedRuleCount=1
a.js:1:14: error[c]: css
a.js:2:10: error[h]: html
a.js:3:12: error[p]: python
```

`ast-grep run -l python -p 'x = 1' .` exits 1 over that tree, the list omits Python and walks no host, `ast-grep run -l python -p 'x = 1' a.js` finds it.

Disjoint routing keeps one site predicate and one scope predicate, one anchor per shared value:

```yaml
languageInjections:
    - hostLanguage: js
      injected: css
      rule:
          kind: string_fragment
          pattern: $CONTENT
          all:
              - &site {inside: {kind: template_string, inside: {kind: call_expression, field: arguments, has: {field: function, regex: '^render$'}}}}
              - not: {inside: {stopBy: end, matches: markup-scope}}
      utils: &scopes
          markup-scope: {kind: function_declaration, has: {field: name, regex: '^markup'}}
    - hostLanguage: js
      injected: html
      rule:
          kind: string_fragment
          pattern: $CONTENT
          all:
              - *site
              - inside: {stopBy: end, matches: markup-scope}
      utils: *scopes
```
