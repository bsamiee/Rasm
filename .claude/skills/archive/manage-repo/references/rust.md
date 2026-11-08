# [RUST]

Cargo owns resolution, lock, and build through root `Cargo.toml`.

## [01]-[WORKSPACE]

- Virtual manifest sits at repository root, rust-analyzer searches one level down and Cargo reads `.cargo/` from the working directory up
- Virtual manifests set `resolver`, Cargo reads no edition from a virtual root and falls back to resolver 1
- `members` globs match crate directories alone, Cargo fails on a glob matching no directory or a directory without `Cargo.toml`
- Manifests leave `rust-version` out, a stale one holds `cargo add`, resolver fallback, and Clippy MSRV lints below the toolchain mise installs
- `CARGO_HOME` keeps its user default, its `bin/` holds rustup proxies and installed binaries a cache clean must keep

## [02]-[TOOLCHAIN]

- `.config/mise.toml` holds no `rust` row, a `rust` row outranks the `rust-toolchain.toml` mise reads
- Channel is the dated nightly pinned `clippy_utils` builds against, `cargo dylint` strips `RUSTUP_TOOLCHAIN` and reads the file
- Files pair the date with `profile = "minimal"`, a rolling `nightly` or an omitted profile resolves per machine and day
- `[env]` `RUSTUP_TOOLCHAIN = false` stops mise's idiomatic export and leaves selection to the file, builds outside the tree take rustup's default
- Nightly-only manifest, config, and source forms join in the change that sets the toolchain file, stable Cargo and rustc refuse them
- Members take the newest stable edition, `future` edition has no behavior of its own and each change in it needs a `#![feature]` opt-in
- Crate `#![feature]` gates name a form the crate uses, `unused_features` fails an idle gate
- Gates stay per crate, `-Zcrate-attr` in `build.rustflags` reaches every unit and `unused_features` fails each member writing no gated form
- Incomplete features stay out, `incomplete_features` fails under `build.warnings`
- Compiler-internal gates (`unboxed_closures`, `fn_traits`) stay out with no stabilization path

## [03]-[CATALOG]

- Catalog rows take crate default features and add each non-default feature every consumer needs, a row adding none is a version string
- Features some consumers need sit on those consumers' rows beside `workspace = true`, the catalog row holds none of them
- `default-features = false` sits only where a default fails a build or run, harms all members, or duplicates a TLS, crypto, runtime, or clock crate
- Restricted rows add back only defaults their role needs that no dependency turns on, their comment stating the fact that excludes others
- Members inherit catalog rows whole through `workspace = true`, `feature-unification = "workspace"` builds each member under workspace features
- Members reach a member through its catalog row naming `path`, an unpublished package needs no `version` on a path dependency
- Catalog rows of a package with no library take `artifact = "bin"`, the form a dependency on a binary takes, and a member row beside `workspace = true` ignores the key
- Native libraries a `links` key names take one owner row, a second binding crate of the same library fails resolution
- Rows pulling ring beside aws-lc-rs with no feature avoiding it stay until a member imports their crate

## [04]-[LINTS]

- Lint levels sit in `[workspace.lints]` alone, a crate attribute splits policy
- Lint rows set `warn` and `.cargo/config.toml` `build.warnings` fails each one
- Lints no reason excuses take `forbid`, no `#[expect]` lifts a forbidden lint
- Clippy restriction lints take `warn`, clap derives emit `#[allow(clippy::restriction)]` and a `forbid` fails it with E0453
- Lint groups take a priority below single lints, Clippy's `lint_groups_priority` fails a group and a lint sharing one
- Restriction lints join one row at a time with repository rule each enforces, `blanket_clippy_restriction_lints` flags the whole group
- Lints demanding docs take `allow`, names and types state each contract
- Suppressions take `#[expect]` with `reason`, Clippy `allow_attributes` fails an `#[allow]` and `allow_attributes_without_reason` a bare one
- Lints take `allow` where their fix writes worse code than the flagged form, row comment naming that form
- `.config/clippy.toml` reaches Clippy through `.cargo/config.toml` `CLIPPY_CONF_DIR`, every cargo caller rust-analyzer included
- `clippy.toml` API bans name their replacement
- `clippy.toml` bans hold paths no dependency macro or derive writes, Clippy reports inside proc-macro output (`#[tokio::test]` writes `tokio::pin!`)
- Bans on a path proc-macro output writes sit in ast-grep or a Dylint lint behind `expanded_externally`
- Ban rows take `allow-invalid` where their crate loads without the item (platform module, disabled feature), Clippy skips unloaded crates
- Ban rows name a path code writes, a type a default type parameter supplies (`RandomState` under `HashMap::new`) escapes its ban
- Cargo checks normal dependencies under default selection or a library selection with all binaries, rustc `unused_crate_dependencies` stays off

## [05]-[BUILD]

- `build.target-dir` and `build.build-dir` name separate directories, Cargo writes intermediates into the output directory by default
- `build.build-dir` takes the `{cargo-cache-home}/build/{workspace-path-hash}` template, its folders stay out of the walks Xcode, editors, and Git make
- Profiles sit in root `Cargo.toml`, Cargo ignores a member's profile with a warning
- Unstable Cargo flags sit in `.cargo/config.toml` `[unstable]`, profile keys in root `cargo-features`, every unit's `-Z` flags in `build.rustflags`
- `build.rustflags` enters every fingerprint, one row every caller reads keeps one artifact set, a `RUSTFLAGS` variable replaces it and rebuilds all
- Coverage builds keep the LLVM backend, instrumentation exists in no other backend, and cranelift dev builds abort on every panic on macOS
- Coverage builds keep cargo-llvm-cov's own artifact set, Cargo's fingerprint misses the instrumentation its rustc wrapper adds
- Members add `build.rs` for work no manifest key or crate does, a build script compiles and runs before its crate on every cold build
- Cargo targets that compile pass `--locked`, a stale `Cargo.lock` fails the run in place of a rewrite
- Cargo targets that compile take `parallelism: false`, each cargo run spreads its units across every core through its own jobserver
- Clippy and Dylint read the `dev` profile, a profile of their own builds and runs each shared unit again
- Workspace wrappers key member units apart, every wrapper shares dependency, proc macro, and build script units
- `cargo dylint` builds its library into `dylint/libraries` and passes `--target-dir dylint/target` under the target directory, `build.build-dir` keeps units in one folder whatever target directory a run names
- Cargo configs hold no `fine-grain-locking`, concurrent runs dirtying shared units deadlock on unit locks

## [06]-[MEMBERS]

- Members declare `name`, `edition.workspace = true`, their target, and `[lints] workspace = true`, Cargo assumes edition 2015 and no lints otherwise
- `[workspace.package]` states `edition`, the key every member shares
- Members state no `version`, Cargo then reading `publish` as empty and `cargo_common_metadata` skipping the package
- Member `[dependencies]` rows list what the crate's code imports, a row no code imports leaves with that code, and its catalog row stays
- Members state one `description` sentence naming their concern, which `cargo metadata`, `cargo info`, and Nx project metadata print and clap's `command!()` and `#[command(about)]` take as the command `about`
- Executables state their version on their `clap` command, the commit describe string `install` compiles in as `RASM_VERSION`, else `0.0.0`, and libraries read `Command::get_version` for `service.version` and MCP identity
- Members name their crate root beside `Cargo.toml` through `[lib] path` or `[[bin]] path`, Cargo discovers targets under `src/` alone
- Product library and binary targets take `test = false`, their suite holding every test, and leave `doctest` and `bench` unset, no entry point running either
- Suites are packages named `<package>-tests` at `tests/rust/<package path minus the language segment>/`, a manifest naming the tested package in `[dev-dependencies]` and a `lib.rs` of one newline under `[lib] test = false`
- Suite tests build as one binary over topic modules at `tests/it/main.rs`, the lint suite's harness at `tests/ui/main.rs`, since each `tests/<name>.rs` Cargo discovers links the package closure again
- Fixtures are targets of the suite using them with their crates in its `[dev-dependencies]`, a fixture that compiles and never runs taking `test = false`
- Binary packages take their binary's name, the Nx project and installed command then share one name

## [07]-[CHECKERS]

- Clippy is the workspace's type check, a separate `cargo check` repeats its compile
- Clippy and Dylint run once at root under `--workspace`, per-package runs repeat `cargo metadata` and hold build folder lock in turn for units a workspace run finds fresh
- Joins stay on `futures::future::join` until a rust-analyzer release resolves the def-site helper macro of `core::future::join!`
- VS Code sets `cargo.targetDir` and Argus sets `CARGO_BUILD_TARGET_DIR` to keep each client's outputs separate
- Both clients set `CARGO_BUILD_BUILD_DIR` through `cargo.extraEnv` with Cargo's path template to keep build locks separate from Nx
- Both clients set `rustc.source = "discover"`, reading `rustc_*` crate source from the sysroot folder `rustc-dev` installs, and turn `checkOnSave` off, a client check running unlocked over the lint fixtures
- `cargo fmt` runs once at the virtual root, one invocation formats every member
- `cargo deny` checks advisories and sources once at root against `Cargo.lock`, `--deny warnings` failing each finding, uncached since the advisory database moves without a repository input
- `.cargo/deny.toml` reports unmaintained crates a member names, a transitive one has no member row to change, and licenses and bans stay off
- nextest ends a test after three 60 s slow periods, a hung test failing its suite in place of the CI job timeout
- `.config/rustfmt.toml` reaches rustfmt through `--config-path` alone: hk, `lint:rustfmt` after `--`, and rust-analyzer `rustfmt.extraArgs` pass it, `function-hooks` adds it to an agent `cargo fmt` or `rustfmt` lacking it
- `rustfmt.toml` holds keys the pinned nightly rustfmt applies, a nightly key needs no `unstable_features` row and stable rustfmt ignores it
- `rustfmt.toml` sets `edition` and `style_edition`, plain `rustfmt` reads neither from `Cargo.toml`
- `rustfmt.toml` fails the check on each line it cannot fit in `max_width`
- Comment wrapping keys stay unset, they split section dividers
- Dylint `orphan_module` fails on a `.rs` file or `mod.rs` folder in a module's folder no module declares
- `cargo modules orphans` loads the whole workspace through rust-analyzer per package

## [08]-[UPGRADES]

- `cargo upgrade` from cargo-edit moves each catalog row to its newest release, breaking releases included, Cargo itself moves no requirement
- `channel` and `clippy_utils.rev` move by hand, Dylint's driver reads `sym.rs` at the first rust-clippy `Rustup` merge holding the channel
- `clippy_utils.rev` is the newest first-parent rust-clippy commit before the next `sym.rs` change
- `cargo dylint upgrade` reads a `rust-toolchain` file in library package and moves to the first commit of a `clippy_utils` version
- `mise install` installs the declared channel with its profile and components through rustup, and matching `http:rustc-source`
- `RUSTUP_AUTO_INSTALL = "0"` leaves toolchain installs to mise
- Prereleases join by hand through a catalog row naming one, `cargo upgrade` then moves that row to newer releases and prereleases

## [09]-[LINT_LIBRARIES]

`[workspace.metadata.dylint]` selects workspace lint libraries with inherited dependencies and lint policy:
- Export `register_lints` through `__maybe_mangle!` beside `dylint_library!()` for native exports under `unsafe_code = "forbid"`
- `register_lints` adds the crate root's folder to `file_depinfo`, and `mtime` freshness reads a folder's newest entry, so an added file anywhere under it rechecks the crate
- Resolved types and effective visibility belong to Dylint; overlapping ast-grep rules exclude those forms through shared utilities
- Lint libraries stay outside application coverage; Dylint's driver executes their diagnostics
- Dylint keeps drivers in `~/.dylint_drivers` and refuses a missing `DYLINT_DRIVER_PATH` directory

## [10]-[MODULES]

- Roots that `[lib] path` or `[[bin]] path` names load `mod a;` from `a.rs` beside them and `a/b.rs` below, no `#[path]` reshapes layout
- Module files take `foo.rs` beside `foo/`, a `foo/mod.rs` beside `foo.rs` fails the build
- Modules beside `Cargo.toml` take no `build`, `tests`, `examples`, or `benches` name, Cargo builds or discovers each as a target
- Types parallel to a dependency's family take its word order, a router beside `<Kind>Router` is `<Kind>Router`, a conversion `Into<Target>`
- Crate roots and module files holding file modules keep them private and list public items in `pub use` lines, a module staying `pub mod` when another package imports the module itself (`rasm_stats::descriptive`) or one of its public names collides with a name bound in its parent (E0252, E0255)
