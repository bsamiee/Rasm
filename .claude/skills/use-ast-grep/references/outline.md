# [OUTLINE]

`ast-grep outline` rows come from parsed source ranges, no reference, type, or re-export resolves.

## [01]-[READING]

Flags select what compiles before the walk, a skipped container's inner declarations appear as items:

| [INDEX] | [FLAG]              | [SKIPS_COMPILE]                                          |
| :-----: | :------------------ | :------------------------------------------------------- |
|  [01]   | `--items structure` | Literal `isImport: true`                                 |
|  [02]   | `--items exports`   | Literal `isExported: false`                              |
|  [03]   | `--items imports`   | Literal `isImport: false` or omitted `isImport`          |
|  [04]   | `--view names`      | Every member document                                    |
|  [05]   | `--view signatures` | Every member document                                    |
|  [06]   | `--pub-members`     | Literal `isPublic: false`, members below its node appear |
|  [07]   | `--type <a,b>`      | Other symbol types                                       |

Modes and views print by resolved value:
- Any directory argument resolves `auto` for the whole command to `exports` and `names`, a file or stdin to `structure` and `digest`
- `exports` keeps imports, `isExported` defaulting true
- `names` prints one line per `symbolType` with repeats as `name ×N`, `digest` lists member names per `symbolType` with public first
- `--pub-members` acts in `digest` and `expanded`, a rule `isPublic` drops false matches and prunes their subtrees, without the flag false members stay dim
- `--type` folds case, `-`, and `_`, exits 1 on an unknown type
- `--match <regex>` tests item names and signatures after member collection, renders item signatures in every view, and saves no member work
- Item filters never reach members, members follow their item
- Text marks exported items bold outside `--items exports`, imports italic outside `--items imports`, and private members dim
- `--color never` erases every mark, select those flags through `--items`, `--pub-members`, or JSON
- Text prints the name in place of an empty signature
- `-l <lang>` walks that language's files plus every file of a host that injects it, and parses each walked file as `<lang>`
- Injected rows come through a host path or `-l <host>`, `-l <injected>` over a tree holding hosts yields host files misparsed as the injected language
- `--stdin -l <lang>` outlines piped code with its injections
- Directory runs walk the languages holding extractors plus every host injecting one of them, and omit files without items
- Named files without items print `nothing found` at exit 0 when the command holds no directory argument
- Hosts without extractors show their injections in directory runs, through a named file, and under `-l <host>`
- Rows sort by byte offset, injected rows take the host's path, `language`, and positions

`--json=stream` prints one object per file, the example below under `--items exports --view expanded` with one member kept:

```json
{"path":"lib.rs","language":"Rust","items":[{"role":"item","symbolType":"module","name":"store","range":{"byteOffset":{"start":0,"end":266},"start":{"line":0,"column":0},"end":{"line":14,"column":1}},"signature":"pub mod store {","astKind":"mod_item","isImport":false,"isExported":true,"members":[{"role":"member","symbolType":"function","name":"Store::get","range":{"byteOffset":{"start":63,"end":182},"start":{"line":3,"column":8},"end":{"line":8,"column":9}},"signature":"pub fn get<T>(&self, key: &str) -> Option<T> where T: Default","astKind":"function_item","isPublic":true}]}]}
```

- `byteOffset` counts UTF-8 bytes, `line` counts from zero, `column` counts Unicode scalars from zero
- `members` is absent when empty and under `names` or `signatures`, `signature` is `""` under `names` without `--match`

## [02]-[LOADING]

Every command loads and compiles before it walks:
- Bundled catalog, each `customLanguages.<lang>.outlineRules` file, then each `--outline-rules` file deserialize in full
- Documents group by `language` and every group compiles less the documents a flag skips, input paths and `-l` narrow the walk alone
- Splitting a catalog per file, folder, or concern changes neither cost nor output
- Pass every path to one command, load cost repeats per command and not per file
- `outlineRules` names one file relative to the config directory, a directory fails with `Is a directory`
- Config entries load in `HashMap` order, a language split across files changes precedence between runs, one file holds a language's documents
- Bundled documents load first and match a node before project documents under the same built-in language name
- Custom language names or `--no-default-outline-rules` replace bundled extractors, the flag drops every bundled language at once
- Repeated paths compile their documents again and add no rows
- `outlineRules` files load and skip with their custom-language library
- Missing `outlineRules` files fail every outline command at exit 1
- Each file parses under a YAML budget of 250,000 nodes, 1,000,000 events, and depth 64, empty documents are skipped

Load errors stop every outline command in every language:
- One invalid document exits 1, a grammar kind renamed under a catalog included
- Parse errors name the file, compile errors (`Invalid Kind`, a rule without a kind set) name neither file nor id
- Parent errors name the member id and the missing or member parent id
- Unknown or misplaced keys load silently, a misspelled `isExported` leaves the item exported

## [03]-[EXTRACTORS]

Documents hold extractor keys and rule-core keys alone:
- Every document holds `id`, `language`, `role`, `symbolType`, `rule`, `name`, and an optional `signature`
- Items take optional `isImport` (default false) and `isExported` (default true)
- Members take `parentRuleIds` and optional `isPublic` (default true)
- `constraints`, `utils`, `transform`, and `rewriters` follow scan rule semantics, `rule` needs a candidate kind set and flag rules need none
- Flags take a literal or a rule, a rule tests the matched node with the document's captures and `utils`
- `parentRuleIds` name item ids of the same language, an absent or member id fails the load
- Ids need no uniqueness, a member naming a repeated id attaches under every item holding it
- Documents hold every util, rewriter, and transform they use, YAML anchors stop at `---` and `utilDirs` never load
- `<<` merges nothing, it fails the load inside `rule`, `utils`, or `transform` and loads silently as an unknown key at a document root

`symbolType` takes one LSP symbol kind per construct family across every catalog, `file`, `namespace`, `package`, `operator`, `typeParameter`, and data-file `string`, `number`, `boolean`, `array`, `key`, `null` name their construct:

| [INDEX] | [SYMBOL_TYPE] | [DECLARATIONS]                           |
| :-----: | :------------ | :--------------------------------------- |
|  [01]   | `module`      | Modules and imports                      |
|  [02]   | `class`       | Classes and reference records            |
|  [03]   | `struct`      | Structs, value records, and type aliases |
|  [04]   | `enum`        | Enums and tagged unions                  |
|  [05]   | `interface`   | Interfaces, protocols, and traits        |
|  [06]   | `function`    | Free functions and function values       |
|  [07]   | `method`      | Functions a type owns                    |
|  [08]   | `constructor` | Initializers and deinitializers          |
|  [09]   | `property`    | Accessors and computed members           |
|  [10]   | `field`       | Stored members                           |
|  [11]   | `enumMember`  | Enum cases and variants                  |
|  [12]   | `variable`    | Mutable bindings                         |
|  [13]   | `constant`    | Immutable bindings                       |
|  [14]   | `object`      | Object values and type extensions        |
|  [15]   | `event`       | Events, signals, and registrations       |

Documents follow these forms:
- One document per role, `symbolType`, parent set, and literal-flag set, syntax forms join as `any:` arms
- Arms binding different names share one template (`name: $NAME$ENUM`)
- Item ids read `<catalog>-<construct>`, `<catalog>` the catalog file's stem, so an error naming only an id locates its file
- Members shared across parents read `<catalog>-member-<construct>`, single-parent members `<catalog>-<parent>-<construct>`
- One construct split by owner group reads `<catalog>-<family>-<construct>`
- Names take the source's own token, keyword constructs (`default`, `init`, `static`) bind `$$NAME` beside a `regex`, lists take their clause's name
- Grammar fields, kinds, `has`, and captures precede any regex over node text
- Flags constant across a document take a literal (imports, injection-only catalogs, containers whose inner declarations appear under exports)
- Other flags take a predicate, O(1) tests precede scans

Module items under the built-in Rust grammar, with `\s` extras, take a member matching each `impl` type token and qualified function members:

```yaml
id: rust-mod
language: Rust
role: item
symbolType: module
rule:
    kind: mod_item
    has: {field: name, pattern: $NAME}
name: $NAME
isExported: {has: {kind: visibility_modifier}}
---
id: rust-mod-impl
language: Rust
role: member
parentRuleIds: [rust-mod]
symbolType: object
rule:
    kind: 'type_identifier, generic_type, scoped_type_identifier'
    pattern: $NAME
    inside: {kind: impl_item, field: type, inside: {kind: declaration_list, inside: {kind: mod_item}}}
name: $NAME
signature: impl $NAME
---
id: rust-mod-function
language: Rust
role: member
parentRuleIds: [rust-mod]
symbolType: function
rule:
    kind: function_item
    pattern: $DECLARATION
    has: {field: name, pattern: $NAME}
    inside:
        kind: declaration_list
        inside:
            any:
                - {kind: impl_item, has: {field: type, pattern: $OWNER}}
                - {kind: mod_item}
rewriters:
    - id: body
      rule: {kind: block, inside: {kind: function_item, field: body}}
      fix: ''
transform:
    QUALIFIER: {replace: {source: $OWNER, replace: '^(.+)$', by: '$1::'}}
    HEADER: {rewrite: {source: $DECLARATION, rewriters: [body]}}
    LINE: {replace: {source: $HEADER, replace: '\p{White_Space}*\n\p{White_Space}*', by: ' '}}
    SIGNATURE: {replace: {source: $LINE, replace: '[\p{White_Space},]+$', by: ''}}
name: $QUALIFIER$NAME
signature: $SIGNATURE
isPublic: {has: {kind: visibility_modifier}}
```

Over `pub mod store` holding `pub struct Store`, an `impl Store` with a `pub fn get<T>` under a multi-line `where` clause and a private `fn evict`, and a `pub fn open`, beside a private `mod private {}`, `--items exports --pub-members --view expanded` prints:

```text
 1: pub mod store {
 3:   impl Store
 4:   pub fn get<T>(&self, key: &str) -> Option<T> where T: Default
12:   pub fn open() -> Store
```

## [04]-[TRAVERSAL]

One cursor walk from the root serves items and members:
- Item documents index by node kind, the first document in load order matching a node wins
- Specific documents precede generic ones, inside one `any:` the wrapped form precedes the unwrapped one
- Wrapper kinds (`export_statement`, `decorated_definition`) match before inner declarations because the walk is top-down
- Unmatched nodes are descended, items appear below unmatched blocks, conditionals, and containers at any depth
- Matched items without member documents prune their subtree, items never nest and need no guard against an ancestor item
- Items that must not appear inside unmatched blocks take one guard, `inside: {kind: <root>, stopBy: {not: {kind: '<wrappers>'}}}`
- Matched items with member documents walk their subtree, the first member match on each path prunes that path at any depth
- Member walks start at the item's children, so the item's own name and header tokens are member candidates
- Member guards name the direct body kind under parent owner kinds, `inside: {kind: <body>, inside: {kind: <owners>}}`
- Members owning a body match their name token so the walk continues into the body, the row's `range` and `astKind` are that token's
- Members below a nested owner attach to the enclosing item, named `$OWNER.$NAME` with the language's own path separator
- Parents share a member document only when no parent's scope exposes another parent's owner once `--pub-members` skips a literal-false member
- Never-true predicates (`isExported: {not: {kind: <own kind>}}`) keep a container compiled and its inner declarations hidden
- Items meant to appear when their container is skipped keep the rule naming that container context
- Items a predicate flag or `--match` drops still prune their subtree

## [05]-[TEMPLATES]

Signatures take the first tier that fits:
1. Omitted `signature` fits when the name line is the header
2. Templates over field captures (`$PREFIX$NAME$TYPES$PARAMS$RETURN`) take an optional prefix from `replace: '^(.+)$'` with `by: '$1 '`
3. Rewriters cover headers no capture spans, read `{kind: <body kinds>, inside: {kind: <owner kinds>, field: body}}`, one definition per name per file

Template behavior:
- Omitted `signature` prints the first nonempty matched line
- `name` referencing one bound untransformed capture once, literal text around it allowed, moves that print to the name line trimmed
- Templates render an unbound capture empty with no load error, unlike scan `fix`
- `transform` and `rewriters` run on every match in every view, `signature` renders only in a view that prints it
- Regex body cuts break on attribute strings
- Signatures print one line, fold `<ws>*\n<ws>*` (`<ws>` the grammar's whitespace class) to one space where extras can split a header
- Folds also join line breaks inside literals, fold only captures that can span lines and trim the end only where a rewriter leaves whitespace
- Multi-line `object_type` or `arguments` collapse by layout, a rewriter on `{kind: <kind>, regex: "\n"}` with fix `{…}` or `(…)`
- `replace` groups expand in `by`, a missing group leaves its separators
- Per-binding signatures read that binding's fields, rewriting a grouped declaration per binding costs quadratic work
- Signatures keep generics, constraints, attributes, heritage, and parameters, quoted names keep delimiters when unquoting changes their spelling
- Slicing at captured positions is unavailable

## [06]-[CATALOGS]

Each outlined language holds one catalog file, registered as its `outlineRules`:
- Kinds and fields come from the grammar's `node-types.json`, never from what current sources hold
- Every named kind a body admits, supertypes expanded through `subtypes`, holds a document or a named exclusion
- Every catalog covers items, members, nested owners, visibility, and signatures at the depth of the strongest one
- Catalogs classify by local syntax with no import resolution or shadowing analysis, a library call shape enters only as a callee test
- Injected roots run the same documents as native roots, a catalog serving injected roots alone sets literal `isExported: false` on every item
- Catalogs serving native files as well cannot mark injected rows, those rows enter host exports
- Sites injected under more than one entry emit one row per injected language over one range
- Scan rules over catalog root keys report keys the loader accepts silently

Kinds a body list admits, less the kinds a catalog names, print each kind that needs a named exclusion:

```bash
jq -r --arg body '<body kinds>' 'INDEX(.[] | select(.named); .type) as $t | def leaf: if $t[.type].subtypes then ($t[.type].subtypes[] | leaf) else .type end; [.[] | select(.type | IN($body | split(", ")[])) | ((.children.types // []) + [.fields[]?.types[]])[] | select(.named) | leaf] | unique[]' <grammar>/src/node-types.json | sort > <admitted>
yq -N '.. | select(key == "kind") | select(tag == "!!str") | split(",")[] | trim' <catalog> | sort -u > <named>
comm -23 <admitted> <named>
```

Drafts load through `--outline-rules`, and a draft of a registered language runs under `-c <config>`, `<config>` a copy whose entry drops `outlineRules`, since registered documents match first:

```bash
ast-grep outline -c <config> --custom-languages allow --no-default-outline-rules --outline-rules <catalog> --items all --view expanded --json=stream <path>
```
