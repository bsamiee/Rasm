# [RUST]

Rust targets hold `.rs` files of packages root `Cargo.toml` lists as workspace members. Placeholders: `<package>` names a package as Cargo prints it (`rasm-stats`), `<crate_name>` its library name as rustdoc names its JSON file and `nx show projects` prints it (`rasm_stats`), `<crate>` a dependency, `<paths>` files a pass touches, `<target>` folder a session reduces, `<dir>` session scratch folder, and `<roots>` `libs products tools tests`.

## [01]-[SKILLS]

| [INDEX] | [TARGET] | [SKILLS]                    | [CONDITION]                                                |
| :-----: | :------- | :-------------------------- | :--------------------------------------------------------- |
|  [01]   | Any Rust | `rust:coding`               | Every target, whatever it imports                          |
|  [02]   | Tests    | `rust:testing`, `code:test` | Package under `tests/`, `#[cfg(test)]` module, pass on one |

`rust:coding` states core crate forms across its references, each read whole with it: `tokio`, `tokio-util`, `futures`, and `tracing` in async, `serde` and `sysexits` in boundaries, `validated` and `nonempty-collections` in results, `itertools` in iterators, `strum` and `enum-map` in types, `typed_floats` in numerics. Catalog crates take no skill of their own.

## [02]-[SOURCES]

Rust targets read crates `rust:coding` opens with, `typed_floats`, `core`, `alloc`, and `std` every session at their `Cargo.lock` release, as most hand-rolled Rust in this repository restates one of them. rustdoc JSON lists every public path, trait member, inherent method, and derive a crate holds. Package reads, one command per step:
1. Version: catalog pin of `<crate>`, named to Cargo as `<crate>@<version>`, as `Cargo.lock` can hold more than one release of a crate
2. Build: rustdoc JSON of `<crate>@<version>`, crates outside `Cargo.lock` read from docs.rs JSON at their catalog version
3. Guide: crate root `//!` documentation, read whole, as Rust crates hold their guide there
4. Index: every module with its item count and first doc line, step 5 opening each module naming a target job's field
5. Inventory: row per public path, trait member, and inherent method of a type or primitive, into `<dir>/inv-<crate_name>.tsv`
6. Facades: crates a facade re-exports, each built from the package `cargo tree` prints, and each derive with its helper attributes
7. Errors and exit status: `thiserror` guide whole, `sysexits` `ExitCode` variants with conversions into them
8. Validated values and derives: `validated` and `nutype` guides whole, `derive_more` and `strum` derives through step 6
9. Collections and iteration: `Itertools` members, `NEVec` and `NonEmptyIterator` members, and `EnumMap` members whole
10. Async: `tokio` and `tokio-util` modules through step 4, `futures` extension traits from `futures-util` through step 6
11. Wire, time, and money: `jiff` and `rust_decimal` guides whole, `serde` attributes from serde.rs through `search:web`
12. Tracing: `tracing` guide whole and `instrument` attribute of `tracing-attributes` through step 6
13. Floats: `typed_floats` class table deciding each float newtype by its signs, zeros, infinities, and NaN
14. Standard library: `core`, `alloc`, and `std` JSON of `rust-docs-json` component through steps 3 to 5
15. Compiler: `rustc_*` source at pinned nightly's commit and `clippy_utils` through steps 4 and 5, for lints under `tools/dylint`
16. Workspace: each `rasm-*` dependency through steps 3 to 5, built with private and hidden items, its folder from its catalog row
17. Catalog: `[workspace.dependencies]` listed whole every session, each crate fitting a target job read through steps 2 to 6 and added where it fits

```bash
yq -p toml -oy '.workspace.dependencies."<crate>" | (.path // .version // .)' Cargo.toml
cargo rustdoc --locked --target-dir <dir>/cargo -p <crate>@<version> --lib -- -Zunstable-options --output-format json
xh -F "https://docs.rs/crate/<crate>/<version>/json" -o <dir>/cargo/doc/<crate_name>.json.zst && zstd -dqf <dir>/cargo/doc/<crate_name>.json.zst
jq -r '.index[.root | tostring].docs // ""' <dir>/cargo/doc/<crate_name>.json
jq -r '.index as $i | .paths | to_entries[] | select(.value.crate_id == 0 and .value.kind == "module") | ($i[.key] // empty) as $m
    | "\(.value.path | join("::"))\t\($m.inner.module.items | length)\t\($m.docs // "" | split("\n")[0])"' <dir>/cargo/doc/<crate_name>.json
jq -r '.index as $i | .paths | to_entries[] | select(.value.crate_id == 0) | ($i[.key].inner | objects) as $inner | (.value.path | join("::")) as $path
    | ([$path, .value.kind, "", ($i[.key].docs // "" | split("\n")[0])],
       ($inner.trait.items[]? | $i[tostring] | [$path, "member", .name, (.docs // "" | split("\n")[0])]),
       (($inner.struct // $inner.enum // $inner.union // $inner.primitive).impls[]? | $i[tostring] | select(.inner.impl.trait == null and (.inner.impl.is_synthetic | not))
        | .inner.impl.items[] | $i[tostring] | select(.visibility == "public") | [$path, "method", .name, (.docs // "" | split("\n")[0])])) | @tsv' \
    <dir>/cargo/doc/<crate_name>.json > <dir>/inv-<crate_name>.tsv
jq -r '.external_crates as $e | .paths as $p | .index[] | select(.crate_id == 0 and .inner.use.id != null) | ($p[.inner.use.id | tostring] // empty)
    | select(.crate_id != 0) | $e[.crate_id | tostring].name' <dir>/cargo/doc/<crate_name>.json | sort | uniq -c
cargo tree --locked -p <crate>@<version> -e normal --depth 1 --prefix none
jq -r '.index[] | select(.crate_id == 0 and .inner.proc_macro != null) | "\(.name)\t\(.inner.proc_macro.kind)\t\(.inner.proc_macro.helpers | join(" "))"' <dir>/cargo/doc/<crate_name>.json
jq -r --arg m '<macro>' '.index[] | select(.crate_id == 0 and .name == $m and .inner.proc_macro != null) | .docs' <dir>/cargo/doc/<crate_name>.json
jq -r '.index[.root | tostring].docs' <dir>/cargo/doc/thiserror.json && rg '^sysexits::' <dir>/inv-sysexits.tsv
jq -r '.index[.root | tostring].docs' <dir>/cargo/doc/{validated,nutype}.json
rg '^(itertools::Itertools|nonempty_collections::\w+::(NEVec|NonEmptyIterator)|enum_map::EnumMap)\t' <dir>/inv-{itertools,nonempty_collections,enum_map}.tsv
rg '^futures_util::\S+Ext\tmember' <dir>/inv-futures_util.tsv
jq -r '.index[.root | tostring].docs' <dir>/cargo/doc/{jiff,rust_decimal}.json
jq -r '.index[.root | tostring].docs' <dir>/cargo/doc/tracing.json
jq -r '.index[.root | tostring].docs' <dir>/cargo/doc/typed_floats.json | rg '^\| '
ls "$(rustc --print sysroot)/share/doc/rust/json"
rustc -vV | rg -o '^commit-hash: (\w+)' -r '$1' | diff - <(cat .cargo/rustc-source/git-commit-hash; echo) && rg -n 'pub fn <member>' .cargo/rustc-source/compiler/<rustc_crate>
cargo rustdoc --locked --target-dir <dir>/cargo -p clippy_utils --lib -- -Zunstable-options --output-format json
yq -p toml -oy '.workspace.dependencies' Cargo.toml
```

Reads hold under conditions below:
- Features: inventories list items under features the workspace enables, and items behind another feature read from docs.rs JSON
- Facades: `serde`, `futures`, `tracing`, `thiserror`, `derive_more`, `strum`, `nutype`, and `enum-map` hold items of other crates step 5 misses
- Derives: derive docs sit in the proc-macro crate alone (`derive_more-impl`, `strum_macros`), and `serde_derive` and `thiserror-impl` hold none
- Proc macros: rustdoc keys a proc-macro crate's `paths` apart from its `index`, step 5 skipping its derives and step 6's command reading them
- Standard library: steps 3 to 5 read `$(rustc --print sysroot)/share/doc/rust/json/<crate>.json`, `std` re-exporting `core` and `alloc` items
- Catalog: catalog rows no member declares sit outside `Cargo.lock`, and step 2 reads them from docs.rs JSON

## [03]-[MAPPING]

Maps list what each file of the target declares at each visibility level and who reads it, from rustdoc JSON of `<package>` with private and hidden items, one command per step:
1. Levels: items per file at `private`, `restricted` (`pub(super)`, `pub(in ..)`), `crate`, and `public`, the list a pass narrows
2. Name families: paths differing by case, a builder word, or a suffix (`Fit` beside `fit`), each family read whole before judging it
3. Constants: `const`, `static`, and associated constants at `public`, each traced to its readers and to the source owning its value
4. Derives: derived traits per type, each traced to a reader of its generated members
5. Dependents: workspace packages declaring `<package>`, each built to rustdoc JSON with private and hidden items
6. Importers: `use` items of `<package>` and each dependent resolving into `<crate_name>`, by the item id rustc resolved
7. Text: sites spelling a name in a full path, macro, string, or message, and mentions across docs, skills, and manifests
8. Like work: clone pairs touching the target across `<roots>`, ranked by lines times similarity
9. Nesting: blocks past one level below the project limit, from Clippy's block count
10. Manifest rows: dependency rows no code of their package reads, and files no module tree reaches

```bash
cargo rustdoc --locked --target-dir <dir>/cargo -p <package> --lib -- -Zunstable-options --output-format json --document-private-items --document-hidden-items
jq -r '.index as $i | .paths as $p
    | [$i | to_entries[] | select(.value.crate_id == 0 and .value.inner.module != null) | ("::" + ($p[.key].path[1:] | join("::"))) as $module
       | .value.inner.module.items[] | $i[tostring] | select((.inner | keys[0]) as $k | ($k | IN("impl", "module") | not) and ($k != "use" or .visibility == "public"))
       | {file: .span.filename, level: (.visibility | if type == "string" then . elif .restricted.path == $module then "private" else "restricted" end)}]
    | group_by(.file)[] | "\(.[0].file)\t" + (group_by(.level) | map("\(.[0].level)=\(length)") | join(" "))' <dir>/cargo/doc/<crate_name>.json
jq -r '[.paths[] | select(.crate_id == 0 and (.kind | IN("function", "struct", "enum", "trait", "type_alias", "constant", "static", "macro")))
    | {name: .path[-1], kind, path: (.path | join("::"))}]
    | group_by(.name | ascii_downcase | gsub("_"; "") | sub("^(new|make|build|create|to|from|into|run|with)"; "") | sub("(ed|s|of|for)$"; ""))[]
    | select(map(.name) | unique | length > 1) | map("\(.kind) \(.path)") | join("  ")' <dir>/cargo/doc/<crate_name>.json
jq -r '.index[] | select(.crate_id == 0 and .visibility == "public" and (.inner | has("constant") or has("static") or has("assoc_const")))
    | "\(.span.filename):\(.span.begin[0])\t\(.name)\t\(.inner | keys[0])"' <dir>/cargo/doc/<crate_name>.json
jq -r '[.index[] | select(.crate_id == 0 and .inner.impl.trait != null and any(.attrs[]; . == "automatically_derived"))
    | {type: (.inner.impl.for.resolved_path.path // empty), at: "\(.span.filename):\(.span.begin[0])", trait: .inner.impl.trait.path}]
    | group_by(.type)[] | "\(.[0].at)\t\(.[0].type)\t\(map(.trait) | unique | join(" "))"' <dir>/cargo/doc/<crate_name>.json
cargo metadata --locked --format-version 1 --no-deps | jq -r --arg p <package> '.packages[] | select(any(.dependencies[]; .name == $p)) | .name'
jq -r --arg c <crate_name> '.paths as $p | (if .index[.root | tostring].name == $c then 0 else (.external_crates | to_entries[] | select(.value.name == $c) | .key | tonumber) end) as $k
    | .index[] | select(.crate_id == 0 and .inner.use.id != null) | ($p[.inner.use.id | tostring] // empty) as $t | select($t.crate_id == $k)
    | "\(.span.filename):\(.span.begin[0])\t\($t.path[-1])\t\(.inner.use.name)"' <dir>/cargo/doc/<dependent_crate_name>.json | sort
rg -nw '<name>' .
jscpd --config .config/jscpd.json --similarity 0.6 --min-nodes 60 --format rust --reporters json --output <dir>/jscpd <roots>;
    jq -r --arg t "$PWD/<target>/" '[.duplicates[] | select((.firstFile.name | startswith($t)) or (.secondFile.name | startswith($t)))] | sort_by(-(.lines * .similarity))[]
    | "\(.lines)\t\(.similarity)\t\(.firstFile.name):\(.firstFile.start)\t\(.secondFile.name):\(.secondFile.start)"' <dir>/jscpd/jscpd-report.json
mkdir -p <dir>/clippy && sd 'excessive-nesting-threshold( *)= *(\d+)' 'excessive-nesting-threshold${1}= <threshold - 1>' < .config/clippy.toml > <dir>/clippy/clippy.toml
CLIPPY_CONF_DIR=<dir>/clippy cargo clippy --locked -p <package> --no-deps --all-targets --all-features --message-format json -- -D clippy::excessive_nesting |
    jq -r 'select(.message.code.code == "clippy::excessive_nesting") | .message.spans[0] | "\(.file_name):\(.line_start)"' | sort -u
cargo shear --locked --format json | jq -r '.findings[] | select(.code != "shear/unused_workspace_dependency") | [.code, (.file // "-"), .message] | @tsv'
```

Commands read by their own rules:
- Binaries: packages with no library take `--bin <name>` in place of `--lib`, their JSON named by the binary (`mcp_muxer`)
- Hidden items: `nutype` declares each type inside `#[doc(hidden)]` module `__nutype_<Name>__`, and JSON without `--document-hidden-items` omits it
- Generated names: families and importers read last path segment, and a `nutype` type beside its `<Name>Error` is one declaration
- Levels: `private` counts an item its own module reads alone, and `public` one the crate root reaches, as `unreachable_pub` denies every other `pub`
- Narrowing: `check` after a narrowed item reports `pub` items the root never reaches (`unreachable_pub`) and items no code reads (`dead_code`)
- Derive readers: derive dropped from its type makes `cargo check` over the package and its dependents name each reader (E0599, E0369, E0277)
- Importers: full paths at call sites (`<crate>::<item>(..)`) and macro bodies hold no `use` item, and `rg -nw` reads them
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Nesting levels: each `{}` block and each `impl`, `trait`, and inline `mod` item, and methods hold one nested block fewer than free functions
- Nesting gaps: `match` arms and closures without braces and combinator chains count none, and the map reads them per member
- Nesting scope: Clippy exits 101 on a site, and `--no-deps` lints `<package>` alone, as path dependencies failing the threshold stop compilation
- Shear: `shear/unused_workspace_dependency` lists catalog rows held for later members, and the command drops its rows
- Shear gaps: artifact dependencies read through `env!("CARGO_BIN_FILE_<NAME>")` and lint fixture files outside any module tree read as unused

## [04]-[MECHANISMS]

Rust resolves types, values, and macros in separate namespaces and attaches members to a type through `impl` blocks. One concept under one name is a type holding its constructors, constants, conversions, and members as associated items. Most cuts delete a free item, hand-written `impl`, or constant by moving its job onto its type, a derive, or a std member. Root `Cargo.toml` lints decide which forms compile: `unreachable_pub` sets each item's level, `multiple_inherent_impl` holds one inherent `impl` per type, `partial_pub_fields` makes a struct plain data or guarded whole, and `inline_modules` puts each module in its own file.

[NAMES]:
- Namespaces: one `use` imports a module and its same-named function, and tuple or unit struct names are both type and constructor (`Meters(1.0)`)
- Associated items: `Fit::new`, `Fit::try_from`, `Fit::LIMIT`, and methods reach readers through one import, in place of `fit` and `FIT_LIMIT`
- Orphan rule: inherent `impl` blocks attach to a local type alone, and behavior over foreign types takes a function or trait implementation
- Visibility: private, `pub(super)`, `pub(crate)`, and `pub` reached from crate root, and rustc reports items wider than their reach
- Exports: one `pub use` list under `[EXPORTS]` of the crate root names each public item once, its modules private

[DERIVATION]:
- Conversions: `From`, `TryFrom`, `FromStr`, `Display`, and `AsRef` state each conversion once, readers calling `.into()`, `?`, `parse`, and `{}`
- Derives: `thiserror`, `derive_more`, `strum`, `serde`, `nutype`, and `enum-map` write impls a hand-written `impl` restates
- Spellings: `serde` and `derive_more` `rename_all` and `strum` `serialize_all` derive each wire or display name from its field or variant
- Kinds: `strum::EnumDiscriminants` derives a variant's fieldless kind, `VariantArray` lists variants, and `EnumMap` holds one value per kind
- Defaults: field defaults under `default_field_values` (`retries: u8 = 3`) replace hand-written `Default` impls and setters of constant values
- Literals: structs with public fields and no cross-field rule take literals and update syntax (`..base`) in place of `new` restating fields
- Stages: defaulted type parameter (`Job<R = Option<Retry>>`) holds each stage of one record, in place of structs restating its fields
- Kinds as parameters: `ConstParamTy` enum as const parameter (`Id<{ Kind::Order }>`) replaces marker types per kind

[VALUES]:
- Newtypes: `typed_floats` classes, `NonZero`, jiff spans and timestamps, and `Decimal` state numeric rules, and `nutype` a rule none states
- Enums: variants hold their state's data, and one `match` over every variant replaces flags tested beside an `Option`
- Iterators: `Iterator` and `itertools` adapters, `try_collect`, `partition_result`, set operators, and `gen` blocks hold each walk over a sequence
- Errors: one `thiserror` enum per crate, `#[from]` fields and `?` converting a source, `#[error(transparent)]` passing a whole foreign error through
- Flat chains: let chains, `let else`, `?` inside `try` blocks, and tuple `match` with match guards replace nested `match`, `if let`, and closures
- Platforms: `cfg_select!` chooses differing expressions inside one item, and `#[cfg]` gates items one platform holds alone
- Traits: extension traits take public blanket implementations (`impl<T: Iterator> IterExt for T`), and trait aliases name repeated bound lists
- Macros: local `macro_rules!` states one table no derive reaches once (a field list generating a codec's read and write)

Host and package rules bind where a probe or the installed source proved them:
- Nightly: `rust-toolchain.toml` pins a nightly, and forms needing a library or language feature add it to their crate root's `#![feature]` line
- Float classes: `typed_floats` `PositiveFinite` refuses -0.0, which `nutype` `greater_or_equal = 0.0` accepts as IEEE comparison does
- Boundary values: probes test -0.0, NaN, subnormals, and infinities against a class before newtype readers move onto it
- Accessors: `derive_more` `AsRef` implements `AsRef<T>` per field type, and `#[as_ref]` fields sharing one type fail with E0119
- Error sources: `thiserror` `#[from]` writes one `From` per source type, and source types more than one variant holds take `map_err` at each call
- Licenses: `check--cargo-deny` of root project `rasm` refuses crates with a license `.config/deny.toml` omits, and candidates pass it first

## [05]-[PATTERNS]

Rust code repeats patterns below, each a candidate the map confirms before any rebuild:

| [INDEX] | [PATTERN]            | [LOOK_FOR]                                                      | [REBUILD]                                   |
| :-----: | :------------------- | :-------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | Free builders        | `fn fit(..) -> Result<Fit, _>` exported beside `Fit`            | `Fit::new` or `TryFrom`, one export         |
|  [02]   | Hand-written impls   | `impl Display`, `From`, `Default`, getter a derive writes       | Derive, field defaults, `#[as_ref]`         |
|  [03]   | Restated newtypes    | `nutype` over a float sign class or nonzero integer             | `typed_floats` class, `NonZero`             |
|  [04]   | Cross-crate newtypes | One rule declared under a name per sibling crate                | One type in its field's library             |
|  [05]   | Kind tables          | Fieldless kind enum, one `fn` per column matching it            | `EnumDiscriminants`, one `match`, `EnumMap` |
|  [06]   | Field lists          | Macro or array spelling one struct's fields at each reader      | `EnumMap` keyed by fieldless enum           |
|  [07]   | Manual loops         | `let mut` buffer filled in `for` loop, index zip, second pass   | Adapters, `partition_result`, `gen` block   |
|  [08]   | Error restatement    | `map_err` to variant holding its source alone, `String` error   | `#[from]` and `?`, `#[error(transparent)]`  |
|  [09]   | Unread derives       | Derive list copied across types, `Hash` no reader keys on       | Derives each reader reads                   |
|  [10]   | Spelled names        | `const X: &str` per wire field or variant, `match` arm per name | `rename_all`, `serialize_all`               |
|  [11]   | Split export forms   | `pub mod` beside root `pub use` lists, `pub use` of dependency  | One root `pub use` list                     |
|  [12]   | Nested matches       | `match` in `if let` in `match`, closures in `and_then` chains   | Tuple `match`, let chain, `try` block, `?`  |

Rows hold under conditions below:
- Row [01] -> functions returning a foreign type stay free, as the orphan rule refuses an inherent `impl` on it
- Row [02] -> impls computing what no derive states stay, and fields sharing one type keep named getters, as `AsRef` selects by type
- Row [03] -> rules no `typed_floats` class states (open interval, -0.0 accepted, bound of 1, cross-field relation) keep `nutype`
- Row [04] -> rules differing at a boundary value (-0.0, subnormal) stay apart, each named for its own rule
- Row [06] -> fields of distinct types (quantities of different dimensions) keep their names, one macro table generating every site
- Row [07] -> buffers a callback API fills or calls reuse stay
- Row [08] -> source types more than one variant holds keep `map_err` naming its operation, as `#[from]` writes one `From` per type
- Row [09] -> public types keep `Debug` under `missing_debug_implementations`, and wire types keep the serde derives their codec reads
- Row [10] -> spellings no case rule derives (`"3.0"`, `"central directory"`) stay one literal at their codec

## [06]-[CHECK]

Checks run per pass, each command in its own background call, with `--files` taking `<paths>` joined by commas:

```bash
hk fix <paths>
nx affected -t check --files=<paths> --exclude=rasm
cargo clippy --locked -p <package> --all-targets --all-features --target x86_64-unknown-linux-gnu
```

- Scope: root project `rasm` runs repository-wide checks that fail on other work in a shared tree, and passes exclude it
- Manifest: `<paths>` holding `Cargo.toml` or `Cargo.lock` -> same `nx affected` line without `--exclude=rasm`, as project `rasm` runs `cargo deny`
- Targets: `check` runs `check--clippy`, `check--dylint`, and `check--rustdoc` of each package a change reaches, dependents included
- Platform: units holding `cfg` arms -> Linux line, as macOS checks compile macOS arms alone
- Windows: arms compile in CI alone, as no local toolchain holds the `x86_64-pc-windows-msvc` standard library
