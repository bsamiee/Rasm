# [RUST]

Rust targets hold `.rs` files of the packages root `Cargo.toml` lists as workspace members, `<package>` naming a package as Cargo prints it (`rasm-stats`), `<crate_name>` its library name as rustdoc names its JSON file and `nx show projects` prints it (`rasm_stats`), `<crate>` a dependency, `<paths>` files a pass touches, `<target>` folder a session reduces, `<dir>` session scratch folder, and `<roots>` `libs products tools tests`.

## [01]-[SKILLS]

| [INDEX] | [TARGET] | [SKILLS]                    | [CONDITION]                                                |
| :-----: | :------- | :-------------------------- | :--------------------------------------------------------- |
|  [01]   | Any Rust | `rust:coding`               | Every session, whatever the target imports                 |
|  [02]   | Tests    | `rust:testing`, `code:test` | Package under `tests/`, `#[cfg(test)]` module, pass on one |

`rust:coding` states core crate forms across its references, each read whole with it: `tokio`, `tokio-util`, `futures`, and `tracing` in async, `serde` and `sysexits` in boundaries, `validated` and `nonempty-collections` in results, `itertools` in iterators, `strum` and `enum-map` in types, `typed_floats` in numerics. Catalog crates take no skill of their own.

## [02]-[SOURCES]

Rust targets read the crates `rust:coding` opens with, `typed_floats`, `core`, `alloc`, and `std` every session at their `Cargo.lock` release, as most hand-rolled Rust in this tree restates one of them, and rustdoc JSON lists every public path, trait member, inherent method, and derive a crate holds. Package reads, one command per step:
1. Version: catalog pin of `<crate>`, named to Cargo as `<crate>@<version>`, as `Cargo.lock` can hold more than one release of a crate
2. Build: rustdoc JSON of `<crate>@<version>`, a crate outside `Cargo.lock` read from docs.rs JSON at its catalog version
3. Guide: crate root `//!` documentation, read whole, as a Rust crate holds its guide there
4. Index: every module with its item count and first doc line, each module naming the job's field opened at step 5
5. Inventory: row per public path, trait member, and inherent method of a type or primitive, into `<dir>/inv-<crate_name>.tsv`
6. Facades: crates a facade re-exports, each built from the package `cargo tree` prints, and each derive with its helper attributes
7. Errors and exit status: `thiserror` guide whole, `sysexits` `ExitCode` variants with the conversions into them
8. Validated values and derives: `validated` and `nutype` guides whole, `derive_more` and `strum` derives through step 6
9. Collections and iteration: `Itertools` members, `NEVec` and `NonEmptyIterator` members, and `EnumMap` members whole
10. Async: `tokio` and `tokio-util` modules through step 4, `futures` extension traits from `futures-util` through step 6
11. Wire, time, and money: `jiff` and `rust_decimal` guides whole, `serde` attributes from serde.rs through `search:web`
12. Tracing: `tracing` guide whole and the `instrument` attribute of `tracing-attributes` through step 6
13. Floats: `typed_floats` class table deciding each float newtype by its signs, zeros, infinities, and NaN
14. Standard library: `core`, `alloc`, and `std` JSON of the `rust-docs-json` component through steps 3 to 5
15. Compiler: `rustc_*` source at the pinned nightly's commit and `clippy_utils` through steps 4 and 5, for lints under `tools/dylint`
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

Reads hold under these conditions:
- Features: inventories list items under the features the workspace enables, and an item behind another feature reads from docs.rs JSON
- Facades: `serde`, `futures`, `tracing`, `thiserror`, `derive_more`, `strum`, `nutype`, and `enum-map` hold items of other crates step 5 misses
- Derives: derive docs sit in the proc-macro crate alone (`derive_more-impl`, `strum_macros`), and `serde_derive` and `thiserror-impl` hold none
- Proc macros: rustdoc keys a proc-macro crate's `paths` apart from its `index`, so step 5 skips its derives and step 6's command reads them
- Standard library: steps 3 to 5 read `$(rustc --print sysroot)/share/doc/rust/json/<crate>.json`, `std` re-exporting `core` and `alloc` items
- Catalog: catalog rows no member declares sit outside `Cargo.lock`, so step 2 reads them from docs.rs JSON

## [03]-[MAPPING]

Maps list what each file of the target declares at each visibility level and who reads it, from rustdoc JSON of `<package>` with private and hidden items, one command per step:
1. Levels: items per file at `private`, `restricted` (`pub(super)`, `pub(in ..)`), `crate`, and `public`, the list a pass narrows
2. Name families: paths differing by case, a builder word, or a suffix (`Fit` beside `fit`), each family read whole before judging it
3. Constants: `const`, `static`, and associated constants at `public`, each traced to its reader and the source that should state it
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
- Hidden items: `nutype` declares each type inside a `#[doc(hidden)]` module `__nutype_<Name>__`, which JSON without `--document-hidden-items` omits
- Generated names: families and importers read the last path segment, and a `nutype` type beside its `<Name>Error` is one declaration
- Levels: `private` counts an item its own module reads alone, and `public` one the crate root reaches, as `unreachable_pub` denies every other `pub`
- Narrowing: `check` after a narrowed item reports a `pub` item the root never reaches (`unreachable_pub`) and an item no code reads (`dead_code`)
- Derive readers: derive dropped from its type makes `cargo check` over the package and its dependents name each reader (E0599, E0369, E0277)
- Importers: full paths at a call site (`<crate>::<item>(..)`) and macro bodies hold no `use` item, which `rg -nw` reads
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Nesting levels: each `{}` block and each `impl`, `trait`, and inline `mod` item, so a method holds one nested block fewer than a free function
- Nesting gaps: `match` arms and closures without braces and combinator chains count none, and are read per member
- Nesting scope: Clippy exits 101 on a site, and `--no-deps` lints the package alone, as a path dependency failing the threshold stops the build
- Shear: `shear/unused_workspace_dependency` lists catalog rows held for later members, so its rows drop
- Shear readers: artifact dependencies read through `env!("CARGO_BIN_FILE_<NAME>")` and lint fixture files outside any module tree read as unused

## [04]-[MECHANISMS]

Rust resolves types, values, and macros in separate namespaces and attaches members to a type through `impl` blocks, so one concept under one name is a type holding its constructors, constants, conversions, and members as associated items, and most cuts here delete a free item, a hand `impl`, or a constant by moving its job onto the type, a derive, or a std member. Root `Cargo.toml` lints decide which forms compile: `unreachable_pub` sets each item's level, `multiple_inherent_impl` holds one inherent `impl` per type, `partial_pub_fields` makes a struct plain data or guarded whole, and `inline_modules` puts each module in its own file.

[NAMES]:
- Namespaces: one `use` imports a module and a same-named function, and a tuple or unit struct's name is its type and its constructor (`Meters(1.0)`)
- Associated items: `Fit::new`, `Fit::try_from`, `Fit::LIMIT`, and methods reach readers through one import, in place of `fit` and `FIT_LIMIT`
- Orphan rule: inherent `impl` blocks attach to a local type alone, so behavior over a foreign type is a function or a trait implementation
- Visibility: private, `pub(super)`, `pub(crate)`, and `pub` reached from the crate root, the compiler reporting an item wider than its reach
- Exports: one `pub use` list under `[EXPORTS]` of the crate root names each public item once, its modules private

[DERIVATION]:
- Conversions: `From`, `TryFrom`, `FromStr`, `Display`, and `AsRef` state each conversion once, readers calling `.into()`, `?`, `parse`, and `{}`
- Derives: `thiserror`, `derive_more`, `strum`, `serde`, `nutype`, and `enum-map` write the impls a hand `impl` restates
- Spellings: `serde` and `derive_more` `rename_all` and `strum` `serialize_all` derive each wire or display name from its field or variant
- Kinds: `strum::EnumDiscriminants` derives a variant's fieldless kind, `VariantArray` lists variants, and `EnumMap` holds one value per kind
- Defaults: field defaults under `default_field_values` (`retries: u8 = 3`) replace a hand `Default` impl and setters of constant values
- Literals: structs with public fields and no cross-field rule take a literal and struct update (`..base`), in place of a `new` restating fields
- Stages: defaulted type parameter (`Job<R = Option<Retry>>`) holds each stage of one record, in place of a struct restating its fields
- Kinds as parameters: `ConstParamTy` enum as const parameter (`Id<{ Kind::Order }>`) replaces a marker type per kind

[VALUES]:
- Newtypes: `typed_floats` classes, `NonZero`, jiff spans and timestamps, and `Decimal` state numeric rules, and `nutype` a rule none states
- Enums: variants hold their state's data, and a `match` over every variant replaces a flag tested beside an `Option`
- Iterators: `Iterator` and `itertools` adapters, `try_collect`, `partition_result`, set operators, and `gen` blocks hold each walk over a sequence
- Errors: one `thiserror` enum per crate, `#[from]` fields and `?` converting a source, `#[error(transparent)]` passing a whole foreign error through
- Flat chains: let chains, `let else`, `?` inside `try` blocks, and a tuple `match` with arm guards replace nested `match`, `if let`, and closures
- Platforms: `cfg_select!` chooses the differing expression inside one item, and `#[cfg]` gates an item one platform holds alone
- Traits: extension traits take the public blanket form (`impl<T: Iterator> IterExt for T`), and trait aliases name a repeated bound list
- Macros: local `macro_rules!` states one table no derive reaches once (a field list generating a codec's read and write)

Host and package rules bind where a probe or the installed source proved them:
- Nightly: `rust-toolchain.toml` pins a nightly, and a form needing a library or language feature adds it to its crate root's `#![feature]` line
- Float classes: `typed_floats` `PositiveFinite` refuses -0.0, which `nutype` `greater_or_equal = 0.0` accepts as IEEE comparison does
- Edge values: -0.0, NaN, subnormals, and infinities are probed against a class before a newtype's readers move onto it
- Accessors: `derive_more` `AsRef` implements `AsRef<T>` per field type, so `#[as_ref]` fields sharing one type fail with E0119
- Sources: `thiserror` `#[from]` writes one `From` per source type, so a source type more than one variant holds takes `map_err` at each call
- Adoption: `check--cargo-deny` of root project `rasm` refuses a crate whose license `.config/deny.toml` omits, so a candidate passes it first

## [05]-[PATTERNS]

Patterns common in Rust, each a lead the map confirms before any rebuild, its rebuild holding under the condition beside it:

| [INDEX] | [PATTERN]            | [LOOK_FOR]                                                       | [REBUILD]                                   |
| :-----: | :------------------- | :--------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | Free builders        | `fn fit(..) -> Result<Fit, _>` exported beside `Fit`             | `Fit::new` or `TryFrom`, one export         |
|  [02]   | Hand trait impls     | `impl Display`, `From`, `Default`, getter a derive writes        | Derive, field defaults, `#[as_ref]`         |
|  [03]   | Restated newtypes    | `nutype` over a float sign class or nonzero integer              | `typed_floats` class, `NonZero`             |
|  [04]   | Cross-crate newtypes | One rule declared under a name per sibling crate                 | One type in the field's library             |
|  [05]   | Kind tables          | Fieldless kind enum with a `fn` per column matching it           | `EnumDiscriminants`, one `match`, `EnumMap` |
|  [06]   | Field lists          | Macro or array spelling one struct's fields at each reader       | `EnumMap` keyed by a fieldless enum         |
|  [07]   | Side collections     | `let mut` buffer filled in a `for`, index zip, second pass       | Adapters, `partition_result`, `gen` block   |
|  [08]   | Error restatement    | `map_err` to a variant holding its source alone, `String` error  | `#[from]` and `?`, `#[error(transparent)]`  |
|  [09]   | Unread derives       | Derive list copied across types, `Hash` no reader keys on        | Derives each reader reads                   |
|  [10]   | Spelled names        | `const X: &str` per wire field or variant, `match` arm per name  | `rename_all`, `serialize_all`               |
|  [11]   | Split export forms   | `pub mod` beside root `pub use` lists, `pub use` of a dependency | One root `pub use` list                     |
|  [12]   | Unwrap towers        | `match` in `if let` in `match`, closures in `and_then` chains    | Tuple `match`, let chain, `try` block, `?`  |

Patterns hold under these conditions:
- Row [01] -> function returning a foreign type stays a function, as the orphan rule refuses an inherent `impl` on it
- Row [02] -> impl computing what no derive states stays, and fields sharing one type keep named getters, as `AsRef` selects by type
- Row [03] -> rule no `typed_floats` class states (an open interval, -0.0 accepted, a bound of 1, a cross-field relation) keeps `nutype`
- Row [04] -> rules differing at an edge value (-0.0, a subnormal) stay apart, each named for its own rule
- Row [06] -> fields of distinct types (quantities of different dimensions) keep their names, one macro table generating every site
- Row [07] -> buffer a callback API fills, or one reused across calls, stays
- Row [08] -> source type more than one variant holds keeps `map_err` naming its operation, as `#[from]` writes one `From` per type
- Row [09] -> public type keeps `Debug` under `missing_debug_implementations`, and a wire type the serde derives its codec reads
- Row [10] -> spelling no case rule derives (`"3.0"`, `"central directory"`) stays one literal at its codec

## [06]-[CHECK]

Check per pass, each command its own background call, `--files` taking `<paths>` joined by commas:

```bash
hk fix <paths>
nx affected -t check --files=<paths> --exclude=rasm
cargo clippy --locked -p <package> --all-targets --all-features --target x86_64-unknown-linux-gnu
```

- Scope: root project `rasm` runs repository-wide checks that fail on other work in a shared tree, and a pass excludes it
- Manifest: `<paths>` holding `Cargo.toml` or `Cargo.lock` -> same `nx affected` line without `--exclude=rasm`, as project `rasm` runs `cargo deny`
- Targets: `check` runs `check--clippy`, `check--dylint`, and `check--rustdoc` of each package a change reaches, dependents included
- Platform: unit holding `cfg` arms -> Linux line, as a macOS check compiles macOS arms alone
- Windows: arms compile in CI alone, as no local toolchain holds the `x86_64-pc-windows-msvc` standard library
