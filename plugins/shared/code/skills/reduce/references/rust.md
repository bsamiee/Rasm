# [RUST]

Rust targets read the crates `rust:coding` names, `core`, `alloc`, and `std`, and each dependency at the version the workspace catalog pins, `<package>` naming the Cargo package holding the target and `<crate>` a dependency.

## [01]-[SKILLS]

| [INDEX] | [TARGET] | [SKILLS]       | [CONDITION]                                   |
| :-----: | :------- | :------------- | :-------------------------------------------- |
|  [01]   | Any Rust | `rust:coding`  | `.rs` file                                    |
|  [02]   | Tests    | `rust:testing` | Package under `tests/`, `#[cfg(test)]` module |

## [02]-[SOURCES]

Package reads, one command per step:
1. Version: catalog pin of `<crate>`, as `Cargo.lock` holds more than one version of a crate and a lock search reads the oldest
2. Inventory: rustdoc JSON of `<crate>@<version>`, a row per public path, trait member, and method, into `<scratch>/reduce/inv-<crate>.tsv`
3. Re-exports: crates each `use` item of a facade crate (`futures`) names, each listed by step 2
4. Standard library: `core`, `alloc`, and `std` JSON of the `rust-docs-json` component through step 2, as `std` re-exports `core` and `alloc` items
5. Compiler: `rustc_*` crates from `.cargo/rustc-source/compiler` and `clippy_utils` through step 2, for `tools/dylint` lints
6. Uncatalogued lock: catalog row no member declares sits outside `Cargo.lock`, read from docs.rs JSON at its catalog version under docs.rs features

```bash
yq -p toml -oy '.workspace.dependencies."<crate>" | (.version // .)' Cargo.toml
cargo rustdoc --locked --target-dir <scratch>/reduce/cargo -p <crate>@<version> --lib -- -Zunstable-options --output-format json
jq -r '.index as $i | .paths | to_entries[] | select(.value.crate_id == 0) | ($i[.key].inner | objects) as $inner | (.value.path | join("::")) as $path
    | ([$path, .value.kind, "", ($i[.key].docs // "" | split("\n")[0])],
       ($inner.trait.items[]? | $i[tostring] | [$path, "member", .name, (.docs // "" | split("\n")[0])]),
       (($inner.struct // $inner.enum // $inner.union).impls[]? | $i[tostring] | select(.inner.impl.trait == null and (.inner.impl.is_synthetic | not))
        | .inner.impl.items[] | $i[tostring] | select(.visibility == "public") | [$path, "method", .name, (.docs // "" | split("\n")[0])])) | @tsv' <json>
jq -r '.index[] | select(.crate_id == 0 and .inner.use != null) | .inner.use.source' <json> | cut -d: -f1 | sort | uniq -c
ls "$(rustc --print sysroot)/share/doc/rust/json"
xh -F "https://docs.rs/crate/<crate>/<version>/json" -o <scratch>/reduce/<crate>.json.zst && zstd -dq <scratch>/reduce/<crate>.json.zst
```

## [03]-[CONSUMERS]

Readers and duplicates across `<roots>` (`libs products tools tests`), one command per step:
1. Projects: every package depending on a file the unit changes, workspace dependents included
2. Duplicates by member or literal: sites spelling a member call, key, path, or message the declaration holds
3. Duplicates by structure: function bodies repeated across crates, as `duplicate_body` and `parameterizable_body` compare within one crate
4. Duplicates by name: type names declared in more than one crate, each pair read whole before a merge
5. Manifest rows: dependency rows no code of their package reads and files no module tree reaches, catalog rows held for later members dropped

```bash
nx show projects --affected --files=<file>
rg -nF '<member or literal>' -g '*.rs' <roots>
jscpd --config .config/jscpd.json --similarity 0.6 --min-nodes 60 --format rust <target> <roots>
rg -oNI '^pub(\([^)]*\))? (struct|enum|trait|type) (\w+)' -r '$3' -g '*.rs' <roots> | sort | uniq -c | awk '$1 > 1'
cargo shear --locked --format json | jq -r '.findings[] | select(.code != "shear/unused_workspace_dependency") | [.code, (.file // "-"), .message] | @tsv'
```

## [04]-[COUNTS]

Counts per file, one command per step:
1. Lines: per file, and lines added and removed per file of the uncommitted pass
2. Declarations: top-level items by visibility from rustdoc JSON, `private`, `restricted` (`pub(super)`, `pub(in ..)`), `crate`, and `public`
3. Exports: `public` items of step 2, as `unreachable_pub` denies a `pub` item the crate root never reaches
4. Nesting: `clippy::excessive_nesting` sites under a scratch `.config/clippy.toml` with its threshold lowered by one, as check denies the limit

```bash
wc -l <files> && git diff --numstat -- <paths>
cargo rustdoc --locked --target-dir <scratch>/reduce/cargo -p <package> --lib -- -Zunstable-options --output-format json --document-private-items
jq -r '.index as $i | .paths as $p
    | [$i | to_entries[] | select(.value.crate_id == 0 and .value.inner.module != null) | ("::" + ($p[.key].path[1:] | join("::"))) as $module
       | .value.inner.module.items[] | $i[tostring] | select((.inner | keys[0]) as $k | ($k | IN("impl", "module") | not) and ($k != "use" or .visibility == "public"))
       | {file: .span.filename, level: (.visibility | if type == "string" then . elif .restricted.path == $module then "private" else "restricted" end)}]
    | group_by(.file)[] | "\(.[0].file)\t" + (group_by(.level) | map("\(.[0].level)=\(length)") | join(" "))' <scratch>/reduce/cargo/doc/<crate_name>.json
mkdir -p <scratch>/reduce/clippy && sd 'excessive-nesting-threshold( *)= *(\d+)' 'excessive-nesting-threshold${1}= <threshold - 1>' < .config/clippy.toml > <scratch>/reduce/clippy/clippy.toml
CLIPPY_CONF_DIR=<scratch>/reduce/clippy cargo clippy --locked -p <package> --no-deps --all-targets --all-features --message-format json -- -D clippy::excessive_nesting | jq -r 'select(.message.code.code == "clippy::excessive_nesting") | .message.spans[0] | "\(.file_name):\(.line_start)"' | sort -u
```

Nesting levels follow clippy's block count:
- Levels: each `{}` block and each `impl`, `trait`, and inline `mod` item, so a method holds one nested block fewer than a free function
- Uncounted: `match` arms without braces, closures without braces, and combinator chains, each read per member
- Scope: `--no-deps` lints the package alone, as a lowered threshold failing a path dependency stops the build first
- Rerun: the `-D` argument reruns clippy over a package cargo already checked, and the next check reruns it once more

## [05]-[FORMS]

Rust forms of each category, each rebuild holding under its stated condition alone, each source the lint `check--dylint` or `check--clippy` reports or the commit of a past pass. The largest cuts replaced a hand-written implementation of a package's trait or math with the package member (`1aa6fe328`), so trait impls of a package trait (`Serializer`, `Visitor`) and hand math beside an autodiff or units type rank first:

| [INDEX] | [CATEGORY]   | [FOUND]                                  | [REBUILD]                                   | [SOURCE]                      |
| :-----: | :----------- | :--------------------------------------- | :------------------------------------------ | :---------------------------- |
|  [01]   | Pass-through | `fn` passing each parameter on unchanged | Callee at each call site                    | `forwarding_function`         |
|  [02]   | Pass-through | Crate-private item with one reader       | Body at its reader                          | `single_reader`               |
|  [03]   | Pass-through | Closure each branch calls with its value | Branches yield an `Option`, tail runs once  | `77e8308f2`                   |
|  [04]   | Pass-through | Helper restating a package conversion    | Package `From` or `?` at each site          | `030711c27`                   |
|  [05]   | Literals     | Literal equal to a dependency constant   | Dependency constant, `env!` for Cargo facts | `restated_constant`           |
|  [06]   | Literals     | Functions mapping variants to constants  | One `match` returning every value           | `variant_constant_functions`  |
|  [07]   | Literals     | Slice listing every variant              | `strum::VariantArray`                       | `manual_variant_array`        |
|  [08]   | Literals     | Invented `io::Error` text for absence    | `Option` source, `None` as absence          | `7440db583`                   |
|  [09]   | Literals     | Map or JSON value read by fixed key      | Struct field per key, serde decode          | `stringly_typed_value`        |
|  [10]   | Redundant    | Hand `core::error::Error` impl           | `thiserror::Error` derive                   | `manual_error_impl`           |
|  [11]   | Redundant    | Hand `AsRef` impl on a newtype           | `derive_more` `AsRef`                       | `manual_reference_impl`       |
|  [12]   | Redundant    | `Default` impl of constant fields        | Field defaults `= <value>`                  | `manual_default_field_values` |
|  [13]   | Redundant    | `map_err` that `?` applies               | `?`, `#[from]` field                        | `redundant_error_conversion`  |
|  [14]   | Redundant    | `bool` set from an `Option` being `Some` | `Option` alone                              | `redundant_option_flag`       |
|  [15]   | Redundant    | Default tested right after `unwrap_or`   | `match` on the `Option`                     | `sentinel_default`            |
|  [16]   | Duplicated   | Hand `serde::Serializer` of every kind   | `serde_content` `Value`, one `match`        | `1aa6fe328`                   |
|  [17]   | Duplicated   | Derivatives beside a dual-number type    | Function over `DualNum` operators           | `1aa6fe328`                   |
|  [18]   | Duplicated   | Bodies differing by a literal or type    | One function taking that parameter          | `parameterizable_body`        |
|  [19]   | Duplicated   | Newtype or body repeated across crates   | One declaration in the field's library      | `jscpd`, name survey          |
|  [20]   | Types        | Fieldless catch-all dropping its value   | Variant holding it (`Other(Box<str>)`)      | `199b551ee`                   |
|  [21]   | Types        | `Option` value beside `Option` error     | `Result`                                    | `option_fields_as_result`     |
|  [22]   | Types        | Kind enum kept beside a data enum        | `strum::EnumDiscriminants`, `EnumMap`       | `rust:coding`                 |
|  [23]   | Types        | Crate-private trait, one implementor     | Inherent `impl`, function on foreign type   | `single_impl_trait`           |
|  [24]   | Types        | One-field struct holding no invariant    | Its field type                              | `needless_struct`             |
|  [25]   | Types        | Struct holding only a `Display` impl     | `fmt::from_fn` closure                      | `manual_fmt_from_fn`          |
|  [26]   | Nesting      | `let` over `Ok` patterns with `else`     | One `match` over the tuple                  | `a3e63536b`                   |
|  [27]   | Nesting      | Closure in `and_then` in `find_map`      | Outer value in a `let`, `is_some_and`       | `a3e63536b`                   |
|  [28]   | Nesting      | Arm matching again on a narrowed value   | Arm guard or let chain                      | `rust:coding`                 |
|  [29]   | Names        | Free `fn` building the type it names     | Associated constructor on the type          | `rust:coding`                 |
|  [30]   | Exports      | `pub use` of a dependency item           | Import from the dependency                  | `reexported_dependency`       |
|  [31]   | Exports      | `pub mod` beside root `pub use` lists    | One export form per crate                   | Visibility count              |
|  [32]   | Exports      | `pub` field a cross-field rule spans     | Private field, `derive_more` `#[as_ref]`    | `rust:coding`                 |

Rows hold under these conditions:
- Row [23] -> a foreign trait a host calls (`rmcp` handler, `LateLintPass`) and a trait alias of bounds stay

## [06]-[CHECK]

Check per pass, each command alone in the background, and the Linux build where the unit holds `cfg` arms, as a macOS check compiles macOS arms alone:

```bash
hk fix <paths>
nx affected -t check --files=<paths> --exclude=rasm
cargo clippy --locked -p <package> --all-targets --all-features --target x86_64-unknown-linux-gnu
```

- Scope: root project `rasm` runs repository-wide checks (knip, tsc, ty, jscpd) that fail on other work in a shared tree, so a pass excludes it
- Manifest: `<paths>` holding `Cargo.toml` or `Cargo.lock` -> same `nx affected` line without `--exclude=rasm`, as root project `rasm` runs cargo-deny
