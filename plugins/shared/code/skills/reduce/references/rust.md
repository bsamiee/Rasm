# [RUST]

Rust targets hold `.rs` files of the packages root `Cargo.toml` lists as workspace members, `<package>` naming a package as Cargo prints it (`rasm-stats`), `<crate_name>` its library name as rustdoc names its JSON file (`rasm_stats`), `<crate>` a dependency, `<paths>` files a pass touches, `<dir>` session scratch folder, and `<roots>` `libs products tools tests`.

## [01]-[SKILLS]

| [INDEX] | [TARGET] | [SKILLS]       | [CONDITION]                                   |
| :-----: | :------- | :------------- | :-------------------------------------------- |
|  [01]   | Any Rust | `rust:coding`  | `.rs` file                                    |
|  [02]   | Tests    | `rust:testing` | Package under `tests/`, `#[cfg(test)]` module |

Packages take no skill of their own, as `rust:coding` names the crate serving each field and its rules name their forms.

## [02]-[SOURCES]

Rust targets read `core`, `alloc`, `std`, the crates `rust:coding` names, and each dependency at the release `Cargo.lock` resolves, as rustdoc JSON lists every public path, trait member, and inherent method of a crate. Package reads, one command per step:
1. Version: catalog pin of `<crate>`, named to Cargo as `<crate>@<version>`, as `Cargo.lock` can hold more than one release of a crate
2. Guide: crate root documentation, read whole, as a Rust crate holds its guide in its root `//!` docs
3. Index: every module with its item count and first doc line, each module naming the job's field opened at step 4
4. Inventory: a row per public path, trait member, and inherent method, into `<dir>/inv-<crate>.tsv`
5. Re-exports: crates each `use` item of a facade crate names (`bon`, `futures`), each read from step 2
6. Standard library: `core`, `alloc`, and `std` JSON of the `rust-docs-json` component through step 4, as `std` re-exports `core` and `alloc` items
7. Compiler: `rustc_*` source under `.cargo/rustc-source/compiler` at the pinned nightly's commit, for lints under `tools/dylint`
8. Workspace: each `rasm-*` dependency through steps 2 to 4 with `--document-private-items`, its folder from its catalog row
9. Uncatalogued: catalog row no member declares sits outside `Cargo.lock`, read from docs.rs JSON at its catalog version

```bash
yq -p toml -oy '.workspace.dependencies."<crate>" | (.path // .version // .)' Cargo.toml
cargo rustdoc --locked --target-dir <dir>/cargo -p <crate>@<version> --lib -- -Zunstable-options --output-format json
jq -r '.index[.root | tostring].docs // ""' <dir>/cargo/doc/<crate_name>.json
jq -r '.index as $i | .paths | to_entries[] | select(.value.crate_id == 0 and .value.kind == "module") | ($i[.key] // empty) as $m
    | "\(.value.path | join("::"))\t\($m.inner.module.items | length)\t\($m.docs // "" | split("\n")[0])"' <dir>/cargo/doc/<crate_name>.json
jq -r '.index as $i | .paths | to_entries[] | select(.value.crate_id == 0) | ($i[.key].inner | objects) as $inner | (.value.path | join("::")) as $path
    | ([$path, .value.kind, "", ($i[.key].docs // "" | split("\n")[0])],
       ($inner.trait.items[]? | $i[tostring] | [$path, "member", .name, (.docs // "" | split("\n")[0])]),
       (($inner.struct // $inner.enum // $inner.union).impls[]? | $i[tostring] | select(.inner.impl.trait == null and (.inner.impl.is_synthetic | not))
        | .inner.impl.items[] | $i[tostring] | select(.visibility == "public") | [$path, "method", .name, (.docs // "" | split("\n")[0])])) | @tsv' \
    <dir>/cargo/doc/<crate_name>.json > <dir>/inv-<crate>.tsv
jq -r '.index[] | select(.crate_id == 0 and .inner.use != null) | .inner.use.source' <dir>/cargo/doc/<crate_name>.json | cut -d: -f1 | sort | uniq -c
ls "$(rustc --print sysroot)/share/doc/rust/json"
rustc -vV | rg -o '^commit-hash: (\w+)' -r '$1' | diff - <(cat .cargo/rustc-source/git-commit-hash; echo) && rg -n 'pub fn <member>' .cargo/rustc-source/compiler/<rustc_crate>
xh -F "https://docs.rs/crate/<crate>/<version>/json" -o <dir>/<crate>.json.zst && zstd -dqf <dir>/<crate>.json.zst
```

Inventories list items under the features the workspace enables, and an item behind another feature reads from docs.rs JSON, which builds under the crate's docs.rs features.

## [03]-[MAPPING]

Maps list what each file of the target declares at each visibility level and who reads it, from rustdoc JSON of `<package>` with private items, one command per step:
1. Levels: items per file at `private`, `restricted` (`pub(super)`, `pub(in ..)`), `crate`, and `public`, the list a pass narrows
2. Name families: paths differing by case, a builder word, or a suffix (`Fit` beside `fit`), each family read whole before judging it
3. Constants: `const`, `static`, and associated constants at `public`, each traced to its reader and the source that should state it
4. Dependents: workspace packages declaring `<package>`, each built to rustdoc JSON with private items
5. Importers: `use` items of `<package>` and each dependent resolving into `<crate_name>`, by the item id rustc resolved
6. Text: sites spelling a name in a full path, macro, string, or message, and mentions across docs, skills, and manifests
7. Like work: clone pairs touching the target across `<roots>`, ranked by lines times similarity
8. Nesting: blocks past one level below the project limit, from Clippy's block count
9. Manifest rows: dependency rows no code of their package reads, and files no module tree reaches

```bash
cargo rustdoc --locked --target-dir <dir>/cargo -p <package> --lib -- -Zunstable-options --output-format json --document-private-items
jq -r '.index as $i | .paths as $p
    | [$i | to_entries[] | select(.value.crate_id == 0 and .value.inner.module != null) | ("::" + ($p[.key].path[1:] | join("::"))) as $module
       | .value.inner.module.items[] | $i[tostring] | select((.inner | keys[0]) as $k | ($k | IN("impl", "module") | not) and ($k != "use" or .visibility == "public"))
       | {file: .span.filename, level: (.visibility | if type == "string" then . elif .restricted.path == $module then "private" else "restricted" end)}]
    | group_by(.file)[] | "\(.[0].file)\t" + (group_by(.level) | map("\(.[0].level)=\(length)") | join(" "))' <dir>/cargo/doc/<crate_name>.json
jq -r '[.paths[] | select(.crate_id == 0 and (.kind | IN("function", "struct", "enum", "trait", "type_alias", "constant", "static", "macro")))
    | {name: .path[-1], kind, path: (.path | join("::"))}]
    | group_by(.name | ascii_downcase | gsub("_"; "") | sub("^(new|make|build|create|to|from|into|run|with)"; "") | sub("(ed|s|of|for|error)$"; ""))[]
    | select(map(.name) | unique | length > 1) | map("\(.kind) \(.path)") | join("  ")' <dir>/cargo/doc/<crate_name>.json
jq -r '.index[] | select(.crate_id == 0 and .visibility == "public" and (.inner | has("constant") or has("static") or has("assoc_const")))
    | "\(.span.filename):\(.span.begin[0])\t\(.name)\t\(.inner | keys[0])"' <dir>/cargo/doc/<crate_name>.json
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
- Levels: `private` counts an item its own module reads alone, and `public` one the crate root reaches, as `unreachable_pub` denies every other `pub`
- Narrowing: `check` after a narrowed item reports a `pub` item the root never reaches (`unreachable_pub`) and an item no code reads (`dead_code`)
- Generated paths: `nutype` declares its type inside a hidden `__nutype_<Name>__` module, so families and importers read the last path segment
- Importers: a full path at a call site (`<crate>::<item>(..)`) and a macro body hold no `use` item, which `rg -nw` reads
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Nesting levels: each `{}` block and each `impl`, `trait`, and inline `mod` item, so a method holds one nested block fewer than a free function
- Nesting gaps: `match` arms without braces, closures without braces, and combinator chains count none and are read per member
- Nesting scope: `--no-deps` lints the package alone, as a lowered threshold failing a path dependency stops the build first
- Nesting rerun: `-D clippy::excessive_nesting` reruns Clippy over a package Cargo already checked, and the next check reruns it once more
- Shear: `shear/unused_workspace_dependency` lists catalog rows held for later members, so its rows drop
- Shear readers: an artifact dependency read through `env!("CARGO_BIN_FILE_<NAME>")` and a lint fixture file outside any module tree read as unused

## [04]-[MECHANISMS]

Rust splits names by case (`Fit` a type, `fit` a function, `FIT` a constant), so one concept under one name is a type holding its constructors, constants, conversions, and members as associated items:
- Associated items: `Fit::new`, `Fit::try_from`, `Fit::LIMIT`, and methods reach readers through one import, in place of `fit` and `FIT_LIMIT`
- Orphan rule: an inherent `impl` attaches to a local type alone, so behavior over a foreign type is a function or a trait implementation
- Conversions: `From`, `TryFrom`, `FromStr`, `Display`, and `AsRef` state each conversion once, readers calling `.into()`, `?`, `parse`, and `{}`
- Literals: a struct with public fields and no cross-field rule takes a literal and struct update (`..base`), in place of a `new` restating fields
- Defaults: field defaults under `default_field_values` (`retries: u8 = 3`) replace a hand `Default` impl and setters of constant values
- Derives: `derive_more`, `strum`, `thiserror`, `serde` with `serde_with`, `derive_where`, `nutype`, catalog `bon` and `o2o` write impls
- Newtypes: `typed_floats` classes, `NormalizedF64`, `NonZero`, `uom` quantities, and jiff spans state numeric rules, `nutype` a rule none states
- Enums: variants hold their state's data, `strum::EnumDiscriminants` derives the fieldless kind, and `EnumMap` holds one value per kind
- Visibility: private, `pub(super)`, `pub(crate)`, and `pub` reached from the crate root, the compiler reporting an item wider than its reach
- Exports: one `pub use` list under `[EXPORTS]` of the crate root names each public item once, its modules private
- Iterators: `Iterator` and `itertools` adapters, `try_collect`, `partition_result`, set operators, and `gen` blocks hold each walk over a sequence
- Errors: one `thiserror` enum per crate, `#[from]` fields and `?` converting a source, `#[error(transparent)]` passing a whole foreign error through
- Platforms: `cfg_select!` chooses the differing expression inside one item, and `#[cfg]` gates an item one platform holds alone
- Traits: extension traits take the public blanket form (`impl<T: Iterator> IterExt for T`), and trait aliases name a repeated bound list
- Flat chains: let chains, `let else`, `?` inside `try` blocks, and a tuple `match` with arm guards replace nested `match`, `if let`, and closures
- Macros: a local `macro_rules!` states one table a derive cannot reach (an enum with its `Display` and `FromStr` from one variant list) once

Host and package rules bind where a probe proved them:
- Nightly: `rust-toolchain.toml` pins a nightly, and a form needing a library or language feature adds it to its crate root's `#![feature]` line
- Float classes: `typed_floats` `PositiveFinite` refuses -0.0 that `nutype` `greater_or_equal = 0.0` accepts, and `NormalizedF64` accepts -0.0
- Edge values: -0.0, NaN, subnormals, and infinities are probed against a package class before a newtype's readers move onto it
- Adoption: `check--cargo-deny` of root project `rasm` refuses a crate whose license `.config/deny.toml` omits, so a candidate passes it first

## [05]-[PATTERNS]

Patterns common in Rust, each a lead the map confirms before any rebuild, its rebuild holding under the condition beside it:

| [INDEX] | [PATTERN]            | [LOOK_FOR]                                                       | [REBUILD]                                     |
| :-----: | :------------------- | :--------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | Free builders        | `fn fit(..) -> Result<Fit, _>` exported beside `Fit`             | `Fit::new` or `TryFrom`, one export           |
|  [02]   | Hand trait impls     | `impl Display`, `Error`, `From`, `Default` a derive writes       | Derive, field defaults                        |
|  [03]   | Restated newtypes    | `nutype` or tuple struct over a float, integer, or range         | `typed_floats`, `strict-num`, `NonZero`       |
|  [04]   | Cross-crate newtypes | One rule declared under a name per sibling crate                 | One type in the field's library               |
|  [05]   | Flags beside data    | `bool` or kind field beside `Option` fields one state sets       | Enum variants holding each state's data       |
|  [06]   | Kind tables          | Fieldless kind enum with a `fn` per column matching it           | `EnumDiscriminants`, one `match`, `EnumMap`   |
|  [07]   | Side collections     | `let mut` buffer filled in a `for`, index zip, second pass       | Adapters, `partition_result`, `gen` block     |
|  [08]   | Error restatement    | `String` error, per-call `map_err`, `Box<dyn Error>`, prefixes   | One `thiserror` enum, `#[from]`, `?`          |
|  [09]   | Platform twins       | Same-named items under `#[cfg(unix)]` and `#[cfg(windows)]`      | One item, `cfg_select!` around what differs   |
|  [10]   | Private trait spread | Crate-private trait over foreign types, `Ext` trait in a crate   | Functions, std trait, or public blanket `Ext` |
|  [11]   | Split export forms   | `pub mod` beside root `pub use` lists, `pub use` of a dependency | One root `pub use` list, dependency imported  |
|  [12]   | Unwrap towers        | `match` in `if let` in `match`, closures in `and_then` chains    | Tuple `match`, let chain, `try` block, `?`    |

Patterns hold under these conditions:
- Row [01] -> a function returning a foreign type stays a function, as the orphan rule refuses an inherent `impl` on it
- Row [02] -> an impl computing what no derive states (an encoding, a mapping onto a foreign enum adding a rule) stays
- Row [03] -> a rule no package class states (-0.0 accepted, a bound of 1, a cross-field relation) keeps `nutype`
- Row [05] -> a `bool` holding a computed fact no other field depends on stays
- Row [07] -> a buffer a callback API fills, or one reused across calls, stays
- Row [09] -> an item one platform holds alone keeps `#[cfg]`
- Row [10] -> a trait a host or package calls (`LateLintPass`, an `rmcp` handler, solver operator traits) and a trait alias of bounds stay

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
- Windows: arms compile in CI alone, as this machine holds no `x86_64-pc-windows-msvc` standard library
