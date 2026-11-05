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
|  [07]   | `--type <a,b>`      | Item documents of other symbol types, members compile    |

Modes and views print by resolved value:
- Any directory argument resolves `auto` for the whole command to `exports` and `names`, a file or stdin to `structure` and `digest`
- `exports` keeps imports, `isExported` defaulting true
- `names` prints one line per `symbolType` in a fixed type order with repeats as `name ×N` and no line numbers, JSON keeps source order
- `digest` lists member names per `symbolType` with public first, member `signature` is `""` there, member signatures need `expanded`
- `--pub-members` acts in `digest` and `expanded`, a rule `isPublic` drops false matches and prunes their subtrees, without the flag false members stay dim
- `--type` folds case, `-`, and `_`, exits 1 on an unknown type, a skipped container stops pruning and can raise cost
- `--match <regex>` tests item names and signatures alone, renders item signatures in every view, and saves no member work, member lookups read JSON
- Item filters never reach members, members follow their item
- Text marks exported items bold outside `--items exports`, imports italic outside `--items imports`, and private members dim
- `--color never` erases every mark, select those flags through `--items`, `--pub-members`, or JSON
- Text prints the name in place of an empty signature, its line number is `range.start`, a decorated definition prints the decorator's line
- `-l <lang>` walks that language's files plus every file of a host that injects it, and parses each walked file as `<lang>`
- Injected rows come through a host path or `-l <host>`, `-l <injected>` over a tree holding hosts yields host files misparsed as the injected language
- `--stdin -l <lang>` outlines piped code with its injections, `--stdin` ignores path arguments
- Directory runs walk the languages holding extractors plus every host injecting one of them, and omit files without items
- HTML hosts inject each `<script>` and `<style>` by its `lang` attribute with no config, `.html` joins directory runs
- Named files without items print `nothing found` at exit 0 when the command holds no directory argument
- Named paths bypass hidden, ignore, and `--globs` filters
- Hosts without extractors show their injections in directory runs, through a named file, and under `-l <host>`
- Rows sort by byte offset, injected rows take the host's path, `language`, and positions

`--json=stream` prints one object per file, the example below under `--items exports --view expanded` with one member kept:

```json
{"path":"lib.rs","language":"Rust","items":[{"role":"item","symbolType":"module","name":"store","range":{"byteOffset":{"start":0,"end":266},"start":{"line":0,"column":0},"end":{"line":14,"column":1}},"signature":"pub mod store {","astKind":"mod_item","isImport":false,"isExported":true,"members":[{"role":"member","symbolType":"function","name":"Store::get","range":{"byteOffset":{"start":63,"end":182},"start":{"line":3,"column":8},"end":{"line":8,"column":9}},"signature":"pub fn get<T>(&self, key: &str) -> Option<T> where T: Default","astKind":"function_item","isPublic":true}]}]}
```

- `byteOffset` counts UTF-8 bytes, `line` counts from zero, `column` counts Unicode scalars from zero
- `members` is absent when empty and under `names` or `signatures`, `signature` is `""` under `names` without `--match`

## [02]-[CONSUMERS]

Scripts read JSON with these contracts:
- Records: an empty file or no language gives none, a named file with no items gives `items: []`, the same file beside a directory gives none
- Empty stdin gives `{"path":"STDIN",…,"items":[]}`, zero records print `nothing found` in text, `[]` under `--json` and `compact`, zero bytes under stream
- Failures on one file exit 0 with `ERROR:` on stderr alone (missing path, non-UTF-8, oversized), a script tests stderr
- Files skip only when over 3,000,000 bytes and over 200,000 lines together, `--stdin` bypasses the limit
- File order varies between runs at default threads, items inside a file stay stable, `-j 1` repeats across runs and keeps argument order for named paths
- Default threads cap at 12
- Paths print as given (`./a.rs` under `.`), `run` normalizes them (`a.rs`), a join across both normalizes first
- `range` holds the same shape and values as `run --json`, an enclosing item is an interval join on `byteOffset` that tests lines
- Items start at a declaration keyword past any `export` or `declare`, bindings of a grouped declaration at each declarator
- Token members end before the first later row no `$OWNER` and separator of theirs or of a row in their span starts, else at their item's `range.end`
- Rows whose signature names a receiver their name lacks (`self.x` named `x`) sit inside the chain of the token row before them
- `astKind` alone separates an injected row from a native one, same-range rows can be byte-identical, dedupe on range, name, and `astKind`
- Ties keep native rows first, then entry order
- Closed consumers stop the walk through a 256-file queue with per-file flush, the command exits 1 with `Broken pipe`, which fails under `pipefail`

Config is found from the working directory, never from a target path:
- Working directories inside a project with a broken injection fail a target outside it at exit 8, the reverse exits 0
- Invalid `languageInjections` exit 8, catalog errors exit 1

Rows compare by key, a broken file still yields partial rows and a diff reads them as false removals:
- Consumers key rows by `(symbolType, owner, name)`, a key holds a bag because overloads repeat
- `ERROR` and `MISSING` nodes print a `symbolType` `null` row named `ERROR`, marking its file unknown, its other rows neither removed nor unchanged
- Error rows print under `--items all` and `structure`, `digest` and `expanded` list one as a private member of an item whose walked body failed

## [03]-[LOADING]

Every command loads and compiles before it walks:
- Bundled catalog, each `customLanguages.<lang>.outlineRules` file, then each `--outline-rules` file deserialize in full
- Documents group by `language` and every group compiles less the documents a flag skips, input paths and `-l` narrow the walk alone
- `outlineRules` names one file relative to the config directory, a directory fails with `Is a directory`
- Config entries load in `HashMap` order, a language split across files changes precedence between runs, one file holds a language's documents
- Bundled documents load first and match a node before project documents under the same built-in language name
- Custom language names or `--no-default-outline-rules` replace bundled extractors, the flag drops every bundled language at once
- Project member documents attach to bundled item ids, a member naming `rust-function` extends a bundled extractor with no replacement
- Repeated paths compile their documents again and add no rows
- `outlineRules` files load and skip with their custom-language library
- Missing `outlineRules` files fail every outline command at exit 1, empty documents are skipped

Load errors follow what each command compiles:
- YAML and parent errors stop every outline command in every language at exit 1
- Compile errors fail only commands compiling that document, a broken member passes under `names` and `signatures`, a literal-flag item passes under modes that skip it
- `--items all --view expanded` compiles a whole catalog
- Parse errors name the file, compile errors (`Invalid Kind`, a rule without a kind set) name neither file nor id
- Parent errors name the member id and the missing or member parent id
- Item and member ids share one role map and the last document loaded wins, a member after an item of one id fails with `cannot use member rule`
- Unknown or misplaced keys load silently and pay full deserialization, a misspelled `isExported` leaves the item exported

## [04]-[EXTRACTORS]

Documents hold extractor keys and rule-core keys alone:
- Every document holds `id`, `language`, `role`, `symbolType`, `rule`, `name`, and an optional `signature`
- Items take optional `isImport` (default false) and `isExported` (default true)
- Members take `parentRuleIds` and optional `isPublic` (default true)
- `constraints`, `utils`, `transform`, and `rewriters` follow scan rule semantics, `rule` needs a candidate kind set and flag rules need none
- `constraints` reject inside the match and the next document in the kind bucket gets the node, a false flag claims and prunes it
- One kind routes across documents by constraint
- Flags take a literal or a rule, a rule tests the matched node with the document's captures and `utils`
- Flag predicates run on a borrowed environment, a template reading a capture one binds renders empty, a transform reading it fails the load
- `parentRuleIds` name item ids of the same language, an absent or member id fails the load
- Ids need no uniqueness, a member naming a repeated id attaches under every item holding it
- Documents hold every util, rewriter, and transform they use, YAML anchors stop at `---` and `utilDirs` never load

`symbolType` takes any LSP symbol kind, one kind per construct family across every catalog keeps rows comparable, `file`, `namespace`, `package`, `operator`, `typeParameter`, and data-file `string`, `number`, `boolean`, `array`, `key` name their construct, quoted `'null'` names a parse failure:

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
- One document per role, `symbolType`, parent set, and literal-flag set, syntax forms join as `any:` arms over one `utils` map
- Arms binding different names share one template (`name: $NAME$ENUM`)
- Ids start with the catalog file's stem, compile and parent errors name no file and an id alone locates one
- Names take the source's own token, keyword constructs (`default`, `init`, `static`) bind `$$NAME` beside a `regex`, lists take their clause's name
- `name` has no fold, a multi-line capture breaks the one-line `names` view, a `pub use a::{…}` prints across lines unless its name is rewritten
- Empty names keep their row (`function:  ×2`)
- Rows stay unique in a file and stable across edits, imports unquoted from their module root
- Repeated elements (array tables, sequence items) name their rows by a stable key (`name`, `id`), keyless ones fold into their parent's row
- Export flags test bare `pub` or `export`, a restricted visibility (`pub(crate)`) exports nothing
- Clauses restating declarations (`export { … }` with no source) emit no row
- Flags constant across a document (imports, injection-only catalogs, containers whose inner declarations appear under exports) take a literal

Flags reach across a run of sibling attributes with a rule stop, default `follows` sees the adjacent sibling alone:

```yaml
id: macro
language: Rust
role: item
symbolType: function
rule: {kind: macro_definition, has: {field: name, pattern: $NAME}}
name: $NAME
isExported:
    follows:
        kind: attribute_item
        regex: 'macro_export'
        stopBy: {not: {kind: attribute_item}}
```

```text
[{"name":"exported","isExported":true},{"name":"hidden","isExported":false},{"name":"near","isExported":true}]
```

`exported` sits under `#[inline]`, `#[macro_export]`, `#[allow(dead_code)]`, `hidden` under `#[inline]`, `near` under `#[macro_export]`, and without `stopBy` `exported` prints `false`.

Flags reach elsewhere in a file through capture equality, comparing by text across node kinds:

```yaml
id: py-def
language: Python
role: item
symbolType: function
rule:
    kind: 'function_definition, class_definition'
    has: {field: name, pattern: $NAME}
utils:
    listed:
        inside:
            kind: module
            stopBy: end
            has:
                kind: expression_statement
                has:
                    kind: assignment
                    all:
                        - {has: {field: left, regex: '^__all__$'}}
                        - {has: {field: right, has: {kind: string_content, stopBy: end, pattern: $NAME}}}
name: $NAME
isExported: {matches: listed}
```

Over `__all__ = ["run", "Kept"]`, `def run`, `def plain`, `class Kept`, `class Hidden`, `def _private`, `--items exports --view names` prints `function: run, Kept`.

Module items under the built-in Rust grammar, its `\s` extras reading ASCII, take a member matching each `impl` type token and qualified function members:

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
    QUALIFIER: {replace: {source: $OWNER, replace: '$', by: '::'}}
    HEADER: {rewrite: {source: $DECLARATION, rewriters: [body]}}
    LINE: {replace: {source: $HEADER, replace: '[\t\n\v\f\r ]*\n[\t\n\v\f\r ]*', by: ' '}}
    SIGNATURE: {replace: {source: $LINE, replace: '[\t\n\v\f\r ,]+$', by: ''}}
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

## [05]-[TRAVERSAL]

One cursor walk from the root serves items and members:
- Item documents index by node kind, the first document in load order matching a node wins
- Specific documents precede generic ones
- Wrapper kinds (`export_statement`, `decorated_definition`) match before inner declarations in the top-down walk
- Unmatched nodes are descended, items appear below unmatched blocks, conditionals, and containers at any depth
- Matched items without member documents prune their subtree, items never nest and need no guard against an ancestor item
- Items that must not appear inside unmatched blocks take one guard, `inside: {kind: '<root>, ERROR', stopBy: {not: {kind: '<wrappers>'}}}`, one failed token roots a file at `ERROR`
- Matched items with member documents walk their subtree, the first member match on each path prunes that path at any depth
- Root-kind items claim the whole file, every member at any depth attaches to that row, an item on a root `ERROR` costs every other row
- Member walks start at the item's children, a header-token arm lists its own item, so its copy for same-kind items needs a strict same-kind ancestor
- Member guards name the direct body kind under parent owner kinds, `inside: {kind: <body>, inside: {kind: <owners>}}`
- Members owning a body match the whole declaration while the body holds no member kind at any depth
- Members whose body holds one match a header token rare in bodies (parameter list, first modifier) to reach the body, its range the row's
- Items whose leaf bodies hold member candidate kinds split into a leaf document no member names and an owner document members name
- Nested owner members name the full owner path, a separator no name segment holds (`.`, Rust `::`), and `$NAME`
- Nearest-owner names collide in a file, a nested owner repeating its parent's name runs over the parent's later members
- Paths come from a `lineage` rewriter over the owner's name token that calls itself once per enclosing owner and writes the separator in its fix
- Lineage rules end in an always-true arm, a climb missing its owner under a broken tree still printing the separator
- Type arguments stating an instance shape (`Data.Class<{…}>`) are a member scope, a brand argument is none
- Owner tests climb (`inside: {stopBy: end, kind: <owners>}`) past unnamed scopes, a parent-step chain misreads owners recovered under `ERROR`
- Each lineage level keeps captures it leaves unbound from the level below, so arms binding different captures take separate rewriters
- Parents share a member document only when no parent's scope exposes another parent's owner once `--pub-members` skips a literal-false member
- Never-true predicates (`isExported: {not: {kind: <own kind>}}`) keep a container compiled and its inner declarations hidden
- Items meant to appear when their container is skipped keep the rule naming that container context
- Items a predicate flag or `--match` drops still prune their subtree

## [06]-[TEMPLATES]

Signatures take the first tier that fits:
1. Omitted `signature` fits when the name line is the header, a row matched on a name token prints the token alone
2. Templates over field captures (`$PREFIX$NAME$TYPES$PARAMS$RETURN`) take an optional prefix from `replace: '$'` with `by: ' '`
3. Rewriters on the name token compose the header from optional field captures

Template behavior:
- Omitted `signature` prints, trimmed, the line of a capture `name` reads once untransformed, literal text around it allowed
- Omitted `signature` under any other `name` or a capture bound above the matched node prints the first nonempty matched line
- Transform sources are bound by some arm of `rule` or `utils`, else the load fails `Undefined meta var`, an arm unmatched at runtime yields empty text
- Rewriter `fix` reads outer captures, rewriter `transform` reads captures its rule binds
- Rewriters whose own `rewrite` reads a list capture (`record R($$$P);` under `parameter_list`) confine a kindless rewriter to it
- Templates render an unbound capture empty with no load error, unlike scan `fix`
- `transform` and `rewriters` run on every match in every view, `signature` renders only in a view that prints it
- Regex body cuts break on `{` in a header or attribute string
- `field: body` cuts leave an `ERROR` between a closing header token (Python `:`) and the body, a cut on both kinds that `follows` the token takes it
- Signatures hold types, shapes, parameters, payloads, and declared roles on one line stable under whitespace folding, a placeholder hides them
- Values (what an initializer, default, or argument computes) print as `…`, a default as `= …` so its parameter reads optional
- Values selecting the configuration a row states (MSBuild `Condition`) stay, eliding them merges distinct rows
- Conditional blocks in a header give their first branch to the signature and to every flag, one row states one configuration
- Rows under a guard that changes their meaning (Python `if TYPE_CHECKING:`) take the guard as a signature prefix
- Signatures drop attributes only a development tool reads (debugger, coverage, analyzer suppression, IDE completion), others keep name and arguments
- Attribute arguments that compute (a call, lambda, operator, collection) print as `…`, a name, attribute, or literal stays
- Extras can split a header, fold `<ws>*\n<ws>*` (`<ws>` the grammar's whitespace class) to one space
- Folds join line breaks inside literals, fold only captures that can span lines and trim the end only where a rewriter leaves whitespace
- Multi-line `arguments` collapse to `(…)` through a rewriter on `{kind: <kind>, regex: "\n"}`, a shape (`object_type`) to `{…}` only where its members print as member rows, elsewhere the fold joins its lines
- `replace` groups expand in `by`, a missing group leaves its separators
- Lone declarators match their declaration, a grouped declaration's bindings match per declarator and read that binding's fields
- Rewriting a grouped declaration per binding costs quadratic work
- Quoted names keep delimiters when unquoting changes their spelling
- Slicing at captured positions comes from `joinBy` extraction or field captures

Values elide through kinded rewriters, on the value's own kind where the grammar gives one (XML `AttText`), else on the node holding it, keeping unmatched text:

```yaml
id: py-fn
language: Python
role: item
symbolType: function
rule:
    kind: function_definition
    has: {field: name, pattern: $NAME}
    all:
        - has: {field: parameters, pattern: $PARAMS}
rewriters:
    - {id: default, rule: {kind: default_parameter, has: {field: name, pattern: $P}}, fix: '$P=…'}
    - {id: typed, rule: {kind: typed_default_parameter, all: [{has: {field: name, pattern: $P}}, {has: {field: type, pattern: $T}}]}, fix: '$P: $T = …'}
transform:
    ARGS: {rewrite: {source: $PARAMS, rewriters: [default, typed]}}
name: $NAME
signature: def $NAME$ARGS
```

Over `def fn(alpha, beta=1, *rest, gamma: int = 2):` and `def none(): pass`, `--items all --view signatures` prints:

```text
1: def fn(alpha, beta=…, *rest, gamma: int = …)
3: def none()
```

## [07]-[CATALOGS]

Each outlined language holds one catalog file, registered as its `outlineRules`:
- Catalogs sharing fragments are written from `tools/ast-grep/outline/sources/<lang>.yml` by a sync generator, edits go to the source since sync overwrites the catalog
- Sources open with an idless document of shared utils, rewriters, transforms, and variants, each document receiving what it calls and its `language`
- Kinds and fields come from the grammar's `node-types.json`, never from what current sources hold
- Every declaration kind a body accepts, supertypes expanded through `subtypes`, holds a document, expression, statement, and text kinds hold none
- Every catalog covers items, members, nested owners, visibility, signatures, and parse failures, a failed parse never reading as absence
- Parse failure documents match an `ERROR` holding at no depth a node their rows need, by kind or by the field test making it a row (Bash `command` by `name`)
- Tests on node naming (`$_`, `$$_`) swallow a token-only construct's row or print none for a punctuation-only failure
- Parse failure items also match the named leaf opening any other `ERROR`, the leaf and each ancestor below it first
- Members take that arm where owner-body failures open on a non-row leaf, failures at the item or closed by zero-width and all-rows arms need none
- Opening leaves are first named children that are no row kind, `ERROR`, or statement wrapping a row, a later row-free sibling opens nothing
- Opening leaves test a kind-only climb to the nearest `ERROR`, statement list, or root first, so clean files pay one climb per walked leaf
- Leaf row documents take `not: {has: {stopBy: end, any: [{kind: ERROR}, {regex: '^$'}]}}` so the walk reaches the failure, a kinded test misses anonymous `MISSING`
- Failures whose named children are all rows print one, the `ERROR` itself (Swift `if #foo`) or its first row child (XML second root) giving up its rows
- `MISSING` tokens match one regex-first arm, `{regex: '^$', any: [<named kinds>, {pattern: <token>}]}` with a pattern per token recovery inserts, a statement separator (`;`) included, less kinds zero-width when valid
- Anonymous `MISSING` tokens match a pattern peeling to them (SQL `)`), else the node they close (`has: {regex: '^$'}` on the kinds closing with them)
- Parse failure rows take `name: ERROR`, `signature: ''`, and literal false flags
- Catalogs classify by local syntax with no import resolution or shadowing analysis, a library call shape enters only as a callee test
- Injected roots run the same documents as native roots, a catalog serving injected roots alone sets literal `isExported: false` on every item
- Catalogs serving native files as well cannot mark injected rows, those rows enter host exports
- Literal `isExported: false` saves document compile and no injected parse, a language holding any group parses every site with all documents skipped
- Sites injected under more than one entry emit one row per entry over one range, repeated entries for one language repeat the row
- Sites holding a host hole (`def ${name}(): pass`) yield no row under a holeless grammar
- Scan rules over catalog root keys (`{field: key, regex: '^["'']?<key>["'']?$'}` under a root `block_mapping` or `flow_mapping`) report keys the loader skips

Kinds a body list accepts, less the kinds a catalog names, print each accepted kind with no document, every one of them holding no declaration, the catalog listing reads each word of a `kind` selector or `selector` value:

```bash
jq -r --arg body '<body kinds>' 'INDEX(.[] | select(.named); .type) as $t | def leaf: if $t[.type].subtypes then ($t[.type].subtypes[] | leaf) else .type end; [.[] | select(.type | IN($body | split(", ")[])) | ((.children.types // []) + [.fields[]?.types[]])[] | select(.named) | leaf] | unique[]' <grammar>/src/node-types.json | sort > <accepted>
yq -N '.. | select(key == "kind" or key == "selector") | select(tag == "!!str") | match("[A-Za-z_][A-Za-z0-9_]*"; "g").string' <catalog> | sort -u > <named>
comm -23 <accepted> <named>
```

Drafts load through `--outline-rules`, and a draft of a registered language runs under `-c <config>`, `<config>` a copy whose entry drops `outlineRules`, since registered documents match first:

```bash
ast-grep outline -c <config> --custom-languages allow --no-default-outline-rules --outline-rules <catalog> --items all --view expanded --json=stream <path>
```
