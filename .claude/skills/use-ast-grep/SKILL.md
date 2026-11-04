---
name: use-ast-grep
description: "Use when outlining, searching, or rewriting code with ast-grep or writing a rule, covering matching and rule configuration"
---

# [AST_GREP]

MCP tools return matches or syntax trees. Use native CLI for project scans, outlines, rewrites, and execution limits. Use `manage-repo` for repository task discovery. Search rules stay inline, durable rules belong under `ruleDirs` of `sgconfig.yml`.

[REFERENCES]:
- [01]-[RULE_BUILDING](references/rule-building.md): Deriving rules from diffs, code smells, and principles
- [02]-[RULE_HARDENING](references/rule-hardening.md): Widening, collapsing, and fixing existing rules
- [03]-[OUTLINE](references/outline.md): `ast-grep outline` usage and extractor construction
- [04]-[PERFORMANCE](references/performance.md): Candidate selection, traversal, recursion, process costs, and measurements
- [05]-[GRAMMARS](references/grammars.md): Parser contracts, lexical composition, scanners, and native builds

[TEMPLATES]:
- [01]-[RULE](templates/rule.yml): Lint rule

## [01]-[MATCHING]

Patterns are valid code under the language's tree-sitter grammar with whole-node metavariables:
- `$VAR` one named node, `$$VAR` one named or unnamed node (operators and keywords included)
- `$$$MULTI` lazy zero-or-more without backtracking, `$_` and `$_NAME` non-capturing
- Wrong-kind patterns take `pattern: { context: <full-code>, selector: <kind> }`
- `$$$` before a node ends at the first fitting sibling, `f($$$H, { $$$P }, $$$T)` matches `f(x, { a }, { b })`
- `smart` and `cst` require the comma before `$$$`, `f($A, $$$R)` matches `f(x,)` and misses `f(x)`, `ast` and below bind `R` empty on `f(x)`
- `strictness` per value: `cst` skips nothing, `smart` skips unnamed target nodes and comments, `ast` skips unnamed nodes on both sides
- `relaxed` skips unnamed nodes on both sides and comments, `signature` matches leaves by kind alone and keeps capture identity
- `template` matches text alone with kinds ignored and needs a `kind` beside it
- `regex` is a Rust regex with Unicode `\w`, `\s`, `\d`, `\b`, no look-around or back-reference, `|` inside a name escapes, `(?i)` sets flags inline
- Classes come from the grammar's `word` and `extras` tokens alone, ASCII tokens take `[0-9A-Za-z_]`, `[\t\n\v\f\r ]`, `[0-9]`, and `(?-u:\b)`
- Identifiers match `[_\p{XID_Start}][_\p{XID_Continue}]*` in Rust and Python, `[$_\p{ID_Start}][$\p{ID_Continue}]*` in JavaScript and TypeScript
- Negated ASCII classes (`[^\t\n\v\f\r ]`) match every non-ASCII character
- `(?x)` in a `|-` block scalar holds one alternative per line with `\ ` for a space inside a phrase, `(?-x:...)` keeps a phrase verbatim
- MCP `dump_syntax_tree` with `format=cst` prints node kinds, fields, and tokens, `format=pattern` shows parsed patterns
- Multi-statement snippets take `ast-grep run -l <lang> -p '<code>' --debug-query=cst`, tree prints on stderr before exit 8

## [02]-[RULES]

- Ids share one namespace across languages, a category checked in more than one names each rule `<category>-<language>`, in one the category alone
- Rules leave out forms a configured linter or type-aware checker reports and keep an arm for a defect that checker's fix writes
- Rule maps conjoin `pattern`, `kind`, `regex`, `nthChild`, `range`, `all`, `any`, `not`, `matches`, then relations in fixed order
- Ordered `all` lists control clause evaluation, YAML map key order does not
- Positive `kind`, `pattern`, `matches`, or `nthChild.ofRule` predicates can derive candidate kinds
- Scan rules of `regex`, `range`, `not`, or unfiltered `nthChild` alone lack candidate kinds and fail to load
- `kind` takes a named node, an anonymous token (`then`) fails the load with `Cannot parse rule`, exit 8
- `kind` accepts ESQuery selectors (`call_expression > identifier`), field and attribute selectors remain unsupported
- `not` guards over a capture sit after its binding clause in `all:`, the first clause naming `$VAR` binds it
- Rules test one node, `has: {all: [<a>, <b>]}` demands one child with both shapes, one `has` per required child
- Repeated `not:` or `has:` keys in one map fail the load as `duplicate field <key>`, `not: {any: [<guards>]}` or an `all:` holds both
- `pattern: $X` alone supplies no kind set, `matches` inherits kinds from its selected util
- Kindless `any:` arms widen the kind set to every kind, `all:` narrows it to the intersection of its items
- Repeated `$VAR` requires syntactic equality and stores the last matching node and range, `$_VAR` skips capture and comparison
- `not` reads current captures and discards new bindings, double negation still exports none
- Relations retain the first qualifying node without later constraint retry, select nearest declarations before testing values
- Captures re-match their text in a later `not: {has: ...}` or `not: {inside: ...}` stopping at owner kinds, the form for a fact every sibling repeats
- `nthChild` counts named siblings, counts include comments the block holds, `ofRule: {not: {kind: comment}}` counts semantic slots
- `stopBy` default `neighbor` reads direct relations, a rule `stopBy` ends the walk at the first node it matches, that node included
- `stopBy` rules start fresh captures and discard bindings, relation targets retain caller captures
- `stopBy: end` walks a candidate's whole axis (`has` the subtree, `inside` every ancestor, `follows` and `precedes` every sibling on that side)
- `stopBy: {kind: <function-kind>}` on `has` keeps an inner function's `return` from satisfying the outer owner
- `field:` binding names the final relation and survives `stopBy: end` (callee vs argument, key vs value)
- `field` beside a rule `stopBy` reads the field child and its direct children alone, `has: {field: <role>, has: {stopBy: <stop>, ...}}` descends
- Unnamed token fields (`operator`) take `has: {field: <role>, pattern: $$_X}`
- `has` and `inside` with `field` select repeated fields' first child, use fieldless relations for later children
- Relational objects hold a rule key beside `field` and `stopBy`, `has: {field: <role>}` alone fails the load
- Default `follows` reads the adjacent sibling alone, a comment or unnamed token (`,`) between siblings needs `stopBy: end`
- `has` visits unnamed children, restrict named children with `not: {has: {pattern: $_, not: <allowed>}}`
- `constraints:` needs no kind set, negative and relational capture guards belong under it
- Constraints evaluate single captures in unspecified order, `$$$` captures are not filtered
- Metavariables bound in `all`, relational rules, and the matching `any:` arm export to `fix`, `message`, and `transform`
- Failed `any:` arms bind nothing, the next arm binds the same name again
- `any:` keeps the first structurally matching arm with no `constraints` retry, the wrapped pattern precedes the unwrapped one

## [03]-[UTILS]

- Global utils are named `<package>-<shape>` and hold `id`, `language`, `arguments`, `rule`, `constraints`, `utils`, and `transform`, no `fix`
- Global util ids share one namespace across languages, one id in more than one language fails the load as `Duplicate rule id`
- Project `utilDirs` load for directory scans, `scan -r`, and `--inline-rules`, stdin included
- Utils under `not:` or `inside:` supply no candidate kinds, kindless scan rules fail with `Rule must specify a set of AST kinds to match`
- Parameterized utils take `arguments` at the global level alone, arguments are mandatory, a string `matches: <id>` of a parameterized util exits 8
- Rules call a parameterized util as `matches: {<util-id>: {<arg>: <rule>}}`, each argument a rule, calls under one `matches` combine as `all`
- Parameterized global calls hidden under relations can load before definitions and fail as undefined
- Dependencies include composite calls, constraints, and local utility definitions, but skip relational children
- Plain relational references resolve lazily, retain explicit candidate kinds when load order cannot infer them
- String `matches: <name>` resolves a parameter, then a local util, then a global util
- Global utils called under `constraints` alone need no kind set
- Local utilities read caller captures, globals use private captures through nested calls
- Argument rules start without caller captures and merge advertised bindings only after template success
- Local references passed as arguments do not advertise their bindings for export

## [04]-[FIX]

`fix` replaces a match with a template, `transform` derives text, and `rewriters` change structure inside captures:
- Templates are unparsed text, captures substitute anywhere in them without a syntax or precedence check
- Undefined metavariables fail the rule load under `scan` and substitute empty under `run -r`
- Statement templates include their `;`, a template without the terminator drops it
- YAML fixes indent captured and transformed multiline text without a raw mode, even same-line edits can change literal contents
- Outputs are named without `$` and read their source with `$`
- Dependent transforms take an earlier output as source and end in one `fix: $RESULT`
- `NEW: replace($OLD, replace=<regex>, by=<text>)` replaces text
- `NEW: substring($OLD, startChar=1, endChar=-1)` uses constant character indexes, negatives count from the end
- `NEW: convert($OLD, toCase=snakeCase, separatedBy=[underscore])` converts case over the named separators
- `NEW: rewrite($OLD, rewriters=[<id>])` rewrites nodes
- Object forms (`replace: {source: $OLD, replace: <regex>, by: <text>}`) hold a regex or replacement with commas or quotes the string form splits
- `replace` reads Rust regex captures in its `by` field as `\$1` or `${NAME}`, with `$$` for a literal dollar
- Templates read every `$NAME` as a metavariable and fail the load on an unbound one, a literal `$NAME` comes from a `replace` with `by: $$NAME`
- Rewriters sit in the document `rewriters` list, `rewrite()` selects them by id
- Rewriters hold `id`, `rule`, and `fix`, optional `constraints`, `transform`, and `utils`, and accept kindless field predicates
- Traversal covers the captured root and its descendants, each node of a `$$$` capture included
- Rewriters try listed rules at every captured node and descendant, keep each node's first match, then discard overlapping edits
- `joinBy` omitted keeps unmatched text, separators and comments between rewritten nodes included, `joinBy` set joins the replacements alone
- Rewriters read captured syntax trees, enclosing rule captures, and enclosing local utils, no transformed string or transform output
- `fix: {template, expandStart, expandEnd}` widens the replaced range to the sibling the sub-rule matches, a miss keeps the matched range
- Expansion stops at the first sibling the sub-rule matches, the adjacent sibling alone by default and any sibling under `stopBy: end`
- `fix` as a list of `{title, template}` offers alternatives, `-U` and `--json` take the first and `-i` offers each
- Overlapping fixes never compose, a `program`-level fix blocks every other fix
- `labels` highlights captured AST ranges, transformed text has no source range and cannot label a node

## [05]-[CONFIG]

Custom-language libraries load under `--custom-languages allow` for one call or through trust in a canonical config path, default `ignore` skips them with a warning. `ast-grep --custom-languages trust --version` records trust from an interactive terminal, `revoke` forgets it, `AST_GREP_TRUST_DIR` relocates the per-user store, and `--config` naming another file loads untrusted. MCP calls pass no flag and load through trust. Place subcommands before options, default-run detection can reinterpret a later subcommand as a path.

- `ruleDirs` and `utilDirs` resolve relative to `sgconfig.yml`
- `languageGlobs.<language>` maps file globs to a language ahead of extension detection
- `customLanguages.<name>` sets `libraryPath`, `extensions`, optional `languageSymbol`, `outlineRules`, `metaVarChar`, and `expandoChar`
- Custom language names are alphanumeric, names resolve built-in languages first and extensions custom languages first
- Newer grammars of a built-in language take a new name
- `-l` and MCP `language` use configured language names, a built-in name misses files mapped to a custom replacement
- `ast-grep` loads the `sgconfig.yml` the nearest ancestor of the working directory holds, a run outside the tree reads none
- Missing paths print `ERROR: <path>: No such file or directory` at `scan` exit 0, exit 1 under `run`, `No matches found` from MCP
- `--filter` matching no rule id exits 3 with `Rule not found`
- `scan` exits 1 on an `error` match, 0 on a miss with `[]` under `--json`, and 8 on a rule that fails to load
- Rules below `error` print `warning[<id>]`, `note[<id>]` under `info`, or `help[<id>]` under `hint`, at exit 0
- `--inline-rules` with `--stdin` reports every inline rule, `--filter` selects among them
- `scan --stdin --filter <regex>` needs exactly one configured rule, multi-rule stdin uses `--inline-rules`
- Use `--inspect entity` for applied rules and skipped-file reasons, `--inspect summary` for file and rule counts
- `files:` and `ignores:` globs match paths relative to the config directory with no `./` prefix, `ignores:` reads first
- `files:` and `ignores:` drop a rule per path after the file parses, CLI `--globs` alone skips parsing
- `scan -r` reads `files:` from the rule file
- Wildcard globs take an implied `**/` prefix, a plain file name matches the one file beside `sgconfig.yml`, `**/<name>` every file of that name
- Scopes name the folders a rule's cause covers, a package rule's the folders of projects referencing the package
- `noIgnore: [hidden]` includes dot directories in project traversal, an explicitly named dot path needs no override
- Nonempty CLI `--no-ignore` lists replace configured `noIgnore` lists
- Injection entries capture the embedded source as `$CONTENT` and name the parser in `injected`, a language or a candidate list with `$LANG`
- `injected` names a built-in language or a `customLanguages` key
- `run -l <injected>` selects other-language hosts, same-language reinjection needs inferred-language `run` or `scan`
- `run --stdin` skips injection extraction
- Injections parse raw captured source without decoding strings, removing block-scalar headers, or deindenting content
- Macro token trees follow their macro grammar; host-language reinjection does not expand macros
- Injection `utils` qualify ownership through host syntax or API context, injection compilation loads no `utilDirs`
- Same-language injected ranges parse twice, a node both documents hold reports from each unless its rule excludes one side
- Leaf-token rules exclude duplicate host regions through a shared predicate; rules needing enclosing host context skip injected copies
- Kind lists inside a flow map are quoted (`{kind: 'block_mapping_pair, flow_pair'}`), an unquoted second kind reads as a key
- Pattern preprocessing converts metavariable sigils to `expandoChar` forms, `metaVarChar` defaults to `$`
- Choose parser-valid `expandoChar` distinct from `_` after native matches demonstrate named, dropped, and unnamed captures
- Literal `$0` and lowercase dollar names survive preprocessing, fixes and transform sources use `$` captures
- Invalid rule files and duplicate ids fail project rule loading, unknown rule-document keys fail deserialization
- Unknown keys in `sgconfig.yml` load silently
- `unused-suppression` reports a comment suppressing nothing at `hint`, `--filter`, `--off`, `--min-severity`, `-r`, or `--inline-rules` disables it
- `no-suppress-all` reports an idless comment and is disabled, `--error=<id>` raises either built-in rule
