# [RUST]

Cargo owns resolution, lock, and build through root `Cargo.toml`.

## [01]-[WORKSPACE]

- Virtual manifest sits at repository root, rust-analyzer searches one level down and Cargo reads `.cargo/` from the working directory up
- Virtual manifests set `resolver`, Cargo reads no edition from a virtual root and falls back to resolver 1
- `members` globs match crate directories alone, Cargo fails on a glob matching no directory or a directory without `Cargo.toml`
- Manifests leave `rust-version` out, a stale one holds `cargo add`, resolver fallback, and Clippy MSRV lints below the toolchain mise installs
- `CARGO_HOME` keeps its user default, its `bin/` holds rustup proxies and installed binaries a cache clean must keep

## [02]-[TOOLCHAIN]

- `mise.toml` holds no `rust` row, one would outrank the `rust-toolchain.toml` mise reads
- Channel is the dated nightly pinned `clippy_utils` builds against, `cargo dylint` strips `RUSTUP_TOOLCHAIN` and reads the file
- Files pair the date with `profile = "minimal"`, a rolling `nightly` or an omitted profile resolves per machine and day
- `[env]` `RUSTUP_TOOLCHAIN = false` stops mise's idiomatic export and leaves selection to the file, builds outside the tree take rustup's default
- Nightly-only manifest, config, and source forms join in the change that sets the toolchain file, stable Cargo and rustc refuse them
- Members take the newest stable edition, the `future` edition has no behavior of its own and each change in it needs a `#![feature]` opt-in
- Crate `#![feature]` gates name a form the crate uses, `unused_features` fails an idle gate
- Gates stay per crate, `-Zcrate-attr` in `build.rustflags` reaches every unit and `unused_features` fails each member writing no gated form
- Incomplete features stay out, `incomplete_features` fails under `build.warnings`
- Compiler-internal gates (`unboxed_closures`, `fn_traits`) stay out with no stabilization path

## [03]-[CATALOG]

- Catalog rows take the crate's default features and add each non-default feature the crate's role needs, a row adding none is a version string
- `default-features = false` sits only where a default fails a build or run, harms all members, or duplicates a TLS, crypto, runtime, or clock crate
- Restricted rows re-add every default except the offending one, their comment stating the fact that excludes it
- Members inherit catalog rows whole through `workspace = true`, `feature-unification = "workspace"` builds each member under workspace features
- Members reach a member through its catalog row pairing `path` with the workspace `version`, Cargo packages no unversioned path dependency
- Native libraries a `links` key names take one owner row, a second binding crate of the same library fails resolution
- Rows pulling ring beside aws-lc-rs with no feature avoiding it stay until a member imports their crate

## [04]-[LINTS]

- Lint levels sit in `[workspace.lints]` alone, a crate attribute splits policy
- Lint rows set `warn` and `.cargo/config.toml` `build.warnings` fails each one
- Lints no reason excuses take `forbid`, no `#[expect]` lifts a forbidden lint
- Clippy restriction lints take `warn`, clap derives emit `#[allow(clippy::restriction)]` and a `forbid` fails it with E0453
- Lint groups take a priority below single lints, Clippy's `lint_groups_priority` fails a group and a lint sharing one
- Restriction lints join one row at a time with the repository rule each enforces, `blanket_clippy_restriction_lints` flags the whole group
- Lints demanding docs take `allow`, names and types state each contract
- Lints take `allow` where their fix writes worse code than the flagged form, the row comment naming that form
- `clippy.toml` API bans name their replacement
- `clippy.toml` bans hold paths no dependency macro or derive writes, Clippy reports inside proc-macro output (`#[tokio::test]` writes `tokio::pin!`)
- Bans on a path proc-macro output writes sit in ast-grep or a Dylint lint behind `expanded_externally`
- Ban rows take `allow-invalid` where their crate loads without the item (platform module, disabled feature), Clippy skips unloaded crates
- Ban rows name a path code writes, a type a default type parameter supplies (`RandomState` under `HashMap::new`) escapes its ban
- Cargo checks normal dependencies under default selection or a library selection with all binaries, rustc `unused_crate_dependencies` stays off
- Clippy's `cargo` group fails a duplicate crate, an `allowed-duplicate-crates` row names one no catalog move clears with each release's dependents

## [05]-[BUILD]

- `build.target-dir` and `build.build-dir` name separate directories, Cargo writes intermediates into the output directory by default
- Profiles sit in root `Cargo.toml`, Cargo ignores a member's profile with a warning
- Unstable Cargo flags sit in `.cargo/config.toml` `[unstable]`, profile keys in root `cargo-features`, every unit's `-Z` flags in `build.rustflags`
- `build.rustflags` enters every fingerprint, one row every caller reads keeps one artifact set, a `RUSTFLAGS` variable replaces it and rebuilds all
- Coverage builds keep the LLVM backend, instrumentation exists in no other backend, and cranelift dev builds abort on every panic on macOS
- Coverage builds keep cargo-llvm-cov's own artifact set, Cargo's fingerprint misses the instrumentation its rustc wrapper adds
- Dev builds keep line tables for members and no debug info for dependencies, backtraces read file and line and fuller debug info slows each link
- Installed binaries build under `dist` inheriting `release`, `cargo dylint` builds lint libraries under `release` and fails on `panic = "abort"`
- Members add `build.rs` for work no manifest key or crate does, a build script compiles and runs before its crate on every cold build
- Cargo targets that compile pass `--locked`, a stale `Cargo.lock` fails the run in place of a rewrite
- Cargo targets that compile take `parallelism: false`, each cargo run spreads its units across every core through its own jobserver
- Insta pending snapshots stay beside their test file, `INSTA_PENDING_DIR` panics on a test path holding `..` (`[[test]] path`)

## [06]-[MEMBERS]

- Members declare `name`, `edition.workspace = true`, their target, and `[lints] workspace = true`, Cargo assumes edition 2015 and no lints otherwise
- `[workspace.package]` states shared package keys with `version` from `1.0.0`, a member inherits each through `<key>.workspace = true` alone
- Publishable packages inherit every `[workspace.package]` key and state `description`, `keywords`, and `categories` for `cargo_common_metadata`
- Unpublishable members state `publish = false` and inherit no `[workspace.package]` key beyond `edition`
- Untested members set `test` and `bench` false on `[lib]` and `[[bin]]`, `doctest` false on `[lib]`, and `clippy --tests` compiles no empty harness
- Members name their crate root beside `Cargo.toml` through `[lib] path` or `[[bin]] path`, Cargo discovers targets under `src/` alone
- Binary packages take their binary's name, the Nx project and installed command then share one name
- `install` places a binary in `$CARGO_HOME/bin` and restarts each `[daemons]` row running it, rows name the command on mise's `PATH`

## [07]-[CHECKERS]

- Clippy is each member's type check over its library, binary, and test targets, a separate `cargo check` repeats its compile
- Tested libraries select `--lib --bins --tests`, tested binary-only packages `--all-targets`, untested packages Cargo defaults
- Joins stay on `futures::future::join` until a rust-analyzer release resolves the def-site helper macro of `core::future::join!`
- `check.allTargets` and `check.extraArgs` check a saved example or binary alone by `--example` or `--bin`, an override runs one command per save
- Editors and harnesses set `check.overrideCommand`, `rustc.source`, and `cargo.extraEnv`, `rust-analyzer.toml` flycheck and Cargo keys go unread
- `cargo.extraEnv` points `CARGO_BUILD_TARGET_DIR` and `CARGO_BUILD_BUILD_DIR` at `rust-analyzer` folders no Nx target shares
- Both clients read `.cargo/rustc-source/Cargo.toml` through an ignored symlink to mise's matching official compiler source archive
- `cargo fmt` runs once at the virtual root, one invocation formats every member
- `rustfmt.toml` holds the keys the pinned nightly rustfmt applies, a nightly key needs no `unstable_features` row and stable rustfmt ignores it
- `rustfmt.toml` sets `edition` and `style_edition`, plain `rustfmt` reads neither from `Cargo.toml`
- `rustfmt.toml` fails the check on each line it cannot fit in `max_width`
- Comment wrapping keys stay unset, they split section dividers
- `cargo modules orphans` fails on a `.rs` file or `mod.rs` folder beside a module file that no module declares

## [08]-[UPGRADES]

- `cargo upgrade` from cargo-edit moves each catalog row to its newest release, breaking releases included, Cargo itself moves no requirement
- `cargo update` follows every catalog move
- `channel` and `clippy_utils.rev` move by hand, Dylint's driver reads `sym.rs` at the first rust-clippy `Rustup` merge holding the channel
- `clippy_utils.rev` is the newest first-parent rust-clippy commit before the next `sym.rs` change, a later one fails `lints` UI tests
- `cargo dylint upgrade` takes the first commit of the newest `clippy_utils` version and refuses a nightly older than the declared one
- `cargo dylint upgrade --rust-version` reads stable `rust-<version>` tags alone
- `mise upgrade` installs the declared channel with its profile and components through rustup, and the matching `http:rustc-source`
- Nightly or Clippy revision moves require unchanged UI fixtures and strict Clippy to pass
- Prereleases join by hand through a catalog row naming one, `cargo upgrade` then moves that row to newer releases and prereleases
- Edition moves run `cargo fix --edition`, then set `[workspace.package]` and `rustfmt.toml` `edition` together

## [09]-[LINT_LIBRARIES]

- One crate `lints` at `tools/dylint/` holds every lint
- `lints` is a workspace member `[workspace.metadata.dylint]` names, catalog rows and `[workspace.lints]` reach it as any member
- `lints` exports `register_lints` through `__maybe_mangle!` beside `dylint_library!()`, `unsafe_code = "forbid"` passes the macro-written export
- Lint modules hold one pass and its `Warn` lints in Clippy file order: one root `lint!` call, the pass's own `new` if any, then `impl LateLintPass`
- Lint modules are plain `mod` items rustfmt reaches, one root `lints!` call registers each pass by `<module path>::<Pass>`
- `lint!` writes a unit `Pass` and `new`, `default <Pass>` derives `new` from `Default`, passes holding resolved items write `new` from `TyCtxt`
- Forms a lint splits by export belong to the lint whatever their `pub` text, an ast-grep rule over the same form excludes it through one shared util
- `host:dylint` members take no `coverage`, the dylint driver runs their lints outside instrumentation
- Drivers stay under `~/.dylint_drivers`, dylint refuses a missing driver directory and `rasm:clean` removes `.cache`

## [10]-[MODULES]

- Roots that `[lib] path` or `[[bin]] path` names load `mod a;` from `a.rs` beside them and `a/b.rs` below, no `#[path]` reshapes layout
- Module files take `foo.rs` beside `foo/`, a `foo/mod.rs` beside `foo.rs` fails the build
- Modules beside `Cargo.toml` take no `build`, `tests`, `examples`, or `benches` name, Cargo builds or discovers each as a target
- Types parallel to a dependency's family take its word order, a router beside `<Kind>Router` is `<Kind>Router`, a conversion `Into<Target>`
- Public items sit in public modules reached by path, `pub_use` fails a re-export
- Crate roots close with their `mod` lines under `[COMPOSITION]`
