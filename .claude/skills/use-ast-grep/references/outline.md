# [OUTLINE]

`ast-grep outline` lists file items and their members by parsed source range, use it to choose what to read or edit when no richer language navigator exists. Outline resolves no reference, type, or re-export. Use the [CLI reference](https://ast-grep.github.io/reference/cli/outline.html) for items, views, defaults, `--pub-members`, and JSON shape, and the [field reference](https://ast-grep.github.io/reference/outline-rule-fields.html) for extractor fields, `symbolType` values, and flag defaults.

## [01]-[READING]

Upstream documentation omits these behaviors:
- `--match` filters after member collection and forces item signatures in names view, filtering an expanded outline saves no member work
- `--type` folds case, `-`, and `_`, an unknown type exits 1
- `--color never` erases the marks for exported, imported, and private entries, select them through `--items`, `--pub-members`, or JSON
- `-l <host>` walks host files and extracts their injections, `-l <injected>` walks files of that language alone
- `--stdin -l <lang>` extracts injections, `run --stdin` extracts none
- Directory runs walk only languages holding extractors, a named file without items prints `nothing found` at exit 0
- Load errors exit 1, upstream states 2, and name no catalog path or extractor id
- One invalid document fails every outline command in every language, a grammar kind renamed under a catalog included

## [02]-[LOADING]

Every command loads before it walks:
- Bundled catalog, each `customLanguages.<lang>.outlineRules` file, then each `--outline-rules` file deserialize in full
- Documents group by `language` and every group compiles on every run, input paths and `-l` narrow the walk alone
- Literal flags `--items` or `--pub-members` exclude, types outside `--type`, and members under `names` or `signatures` skip compilation
- YAML parsing covers every document, pruning never avoids it
- YAML node count, per-document regex compiles, and per-match transforms and rewriters set the cost, bytes and formatting move none
- Splitting a catalog per file, folder, or concern changes neither cost nor output
- Pass every path to one command, load cost repeats per command and not per file
- `outlineRules` names one file per custom language, a directory fails with `Is a directory`
- Config entries load in `HashMap` order, a language split across files changes precedence between runs, one file holds its documents
- Unknown or misplaced keys load silently, a misspelled `isExported` leaves the item exported
- Repeated paths load their documents again, a catalog registered in `outlineRules` takes no `--outline-rules` copy
- `--no-default-outline-rules` drops every bundled language at once, built-in language names alone reach bundled extractors

## [03]-[EXTRACTORS]

One document per role, `symbolType`, parent set, and literal-flag set, syntax forms join as `any:` arms and an unbound capture renders empty:

```yaml
id: rust-type
language: rustlang
role: item
symbolType: struct
rule:
    kind: struct_item
    pattern: $DECLARATION
    has: {field: name, pattern: $NAME}
rewriters:
    - id: body
      rule: {kind: field_declaration_list, inside: {kind: struct_item, field: body}}
      fix: ''
transform:
    HEADER: {rewrite: {source: $DECLARATION, rewriters: [body]}}
    LINE: {replace: {source: $HEADER, replace: '\p{White_Space}*\n\p{White_Space}*', by: ' '}}
    SIGNATURE: {replace: {source: $LINE, replace: '[\p{White_Space},;]+$', by: ''}}
name: $NAME
signature: $SIGNATURE
isExported: {has: {kind: visibility_modifier}}
---
id: rust-type-field
language: rustlang
role: member
parentRuleIds: [rust-type]
symbolType: field
rule:
    kind: field_declaration
    inside: {kind: field_declaration_list, inside: {kind: struct_item}}
    all:
        - has: {field: name, pattern: $NAME}
        - has: {field: type, pattern: $TYPE}
name: $NAME
signature: '$NAME: $TYPE'
isPublic: {has: {kind: visibility_modifier}}
```

Over `pub struct Config<T>` with a multi-line `where` clause, a `pub name: String` field, and a private `value: T` field, beside `struct Unit;`, `--items exports --pub-members --view expanded` prints `pub struct Config<T> where T: Clone` and `name: String` alone.

- Documents hold only `id`, `language`, `role`, `symbolType`, `rule`, `name`, `signature`, `constraints`, `utils`, `transform`, `rewriters`, and flags
- `isImport` and `isExported` go on items alone, `isPublic` and `parentRuleIds` on members alone
- Item ids read `<key>-<construct>`, shared members `<key>-member-<construct>`, single-parent members `<key>-<parent>-<construct>`
- One construct split by owner group takes `<key>-<family>-<construct>`, `<key>` the catalog's language key that prefixes every id
- `parentRuleIds` name item ids of the same language, an absent or member id fails the load
- Names take the source's own token, keyword constructs (`default`, `init`, `static`) bind `$$NAME` beside a `regex`, lists take their clause's name
- Grammar fields, kinds, `has`, and captures precede any regex over node text
- Classes take the grammar's identifier and whitespace sets, Unicode Perl classes (`\s`, `\w`, `\d`, `\b`) never appear
- `nx run rasm:lint:ast-grep:classes` judges each class against its grammar
- Flags constant across a document take a literal (imports, containers whose members surface, injection-only catalogs), others a predicate
- Predicates test the matched node with the document's captures and `utils`, O(1) tests precede scans
- Documents hold every util, rewriter, and transform they use, anchors stop at `---`, `<<` fails, global `utilDirs` never load

## [04]-[TRAVERSAL]

One cursor walk from the root serves items and members:
- Item extractors index by node kind, the first document in file order matching a node wins
- Specific documents precede generic ones, inside one `any:` the wrapped form precedes the unwrapped one
- Unmatched nodes are descended, items surface below unmatched blocks, conditionals, and containers at any depth
- Matched items without member extractors prune their subtree, items never nest and need no guard against an ancestor item
- Items that must not surface from unmatched blocks take one guard, `inside: {kind: <root>, stopBy: {not: {kind: '<wrappers>'}}}`
- Items meant to surface when their container is pruned (Rust methods) keep their inner-context rule
- Matched items with member extractors walk their subtree, the first member match on each path prunes that path at any depth
- Member guards name only the direct body kind under parent owner kinds, `inside: {kind: <body>, inside: {kind: <owners>}}`
- Parents share a member document only when no parent's scope exposes another parent's owner once `--pub-members` drops a member
- Members owning a body match their name node so the walk continues into the body, the row's `range` and `astKind` are then the name token's
- Members below a nested owner attach to the enclosing item named `$OWNER.$NAME`, Rust `$OWNER::$NAME`
- Containers `--items exports` drops by literal flag or `--type` misses never compile, their inner declarations surface as items
- Items a predicate flag or `--match` drops still consume their subtree
- Wrapper kinds (`export_statement`, `decorated_definition`) match before inner declarations because the walk is top-down

## [05]-[TEMPLATES]

Signatures take the first form that fits:
- Omitted `signature`, when the name line is the header and `name` is one untransformed metavariable, prints that line trimmed
- Omitted `signature` under any other `name` prints the first nonempty matched line
- Templates over field captures (`$PREFIX$NAME$TYPES$PARAMS$RETURN`) take an optional prefix from `replace: '^(.+)$'` with `by: '$1 '`
- Rewriters cover headers no capture spans, a regex body cut breaks on attribute strings
- Rewriters read `{kind: <body kinds>, inside: {kind: <owner kinds>, field: body}}`, one definition per name per file
- `transform` and `rewriters` run on every match in every view, `signature` renders only in a view that prints it
- `rewrite` tries its rules at every captured node and descendant, a kindless rewriter costs most
- Signatures print one line, the fold also joins line breaks inside literals
- Fold `<ws>*\n<ws>*` to one space only in documents with kinds that print multi-line headers, trim the end only where a rewriter leaves whitespace
- Multi-line `object_type` or `arguments` collapse by layout, a rewriter on `{kind: <kind>, regex: "\n"}` with fix `{…}` or `(…)`
- `replace` groups expand in `by`, a missing group leaves its separators
- Per-binding signatures read that binding's fields, rewriting a grouped declaration per binding costs quadratic work
- Signatures keep generics, constraints, attributes, heritage, and parameters, quoted names keep delimiters when unquoting changes their spelling
- Slicing at captured positions is unavailable

## [06]-[CATALOGS]

Each outlined language holds one hand-written catalog at `tools/ast-grep/outline/<key>.yml`, `<key>` its `rules/<key>` folder:
- Kinds and fields come from `.cache/ast-grep/<lang>/src/node-types.json`, every declaration kind a body admits is covered or excluded by name
- Catalogs classify by local syntax with no import resolution or shadowing analysis, a library call shape enters only as a callee test
- Injected rows take the host's path, positions, and `language`, the engine runs the same extractors on injected and native roots
- Catalogs serving only injected roots (`sql`, `pgsql`) set literal `isExported: false` on every item
- Catalogs serving native files as well cannot mark injected rows, those rows enter host exports
- Sites injected into `sql` and `pgsql` emit one row per dialect over one range
- Registered `outlineRules` paths without their file fail every outline command, a key and its catalog arrive in one commit

Registered catalogs load from `sgconfig.yml`, a draft catalog outside `outlineRules` loads through `--outline-rules`, function-hooks adds `--custom-languages allow` to each agent `ast-grep` call:

```bash
ast-grep outline --no-default-outline-rules --outline-rules <catalog> --items all --view expanded --json=stream <path>
```
