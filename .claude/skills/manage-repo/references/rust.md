# [RUST]

Cargo owns resolution, lock, and build through root `Cargo.toml`.

## [01]-[WORKSPACE]

- Virtual manifest sits at repository root, rust-analyzer searches one level down and Cargo reads `.cargo/` from the working directory up
- Virtual manifests set `resolver`, Cargo reads no edition from a virtual root and falls back to resolver 1
- `members` globs match crate directories alone, Cargo fails on a glob matching no directory or a directory without `Cargo.toml`
- Manifests leave `rust-version` out, a stale one holds `cargo add`, resolver fallback, and Clippy MSRV lints below the toolchain mise installs
- `CARGO_HOME` keeps its user default, its `bin/` holds rustup proxies and installed binaries a cache clean must keep
- Cargo subcommands take `cargo:` tool rows beside a `cargo-binstall` row, mise then installs release binaries in place of compiling

## [02]-[TOOLCHAIN]

- `mise.toml` holds no `rust` row, one would outrank the `rust-toolchain.toml` mise reads
- Channel is the dated nightly pinned `clippy_utils` builds against, `cargo dylint` strips `RUSTUP_TOOLCHAIN` and reads the file
- Files pair the date with `profile = "minimal"`, a rolling `nightly` or an omitted profile resolves per machine and day
- `[env]` `RUSTUP_TOOLCHAIN = false` stops mise's idiomatic export and leaves selection to the file, builds outside the tree take rustup's default
- Nightly-only manifest, config, and source forms join in the change that sets the toolchain file, stable Cargo and rustc refuse them
- Crate `#![feature]` gates name a form the crate uses, `unused_features` fails an idle gate

## [03]-[CATALOG]

- Catalog rows take the crate's default features and add each non-default feature the crate's role needs, a row adding none is a version string
- Rows set `default-features = false` only where a default fails a build or run, harms every member, or adds a second TLS, crypto, runtime, or clock crate
- Restricted rows re-add every default except the offending one, their comment stating the fact that excludes it
- Members inherit catalog rows whole through `workspace = true`, `feature-unification = "workspace"` builds each member under workspace features
- Members reach a member through its catalog row pairing `path` with the workspace `version`, Cargo packages no unversioned path dependency

## [04]-[LINTS]

- Lint levels sit in `[workspace.lints]` alone, a crate attribute splits policy
- Lint rows set `warn` and `.cargo/config.toml` `build.warnings` fails each one
- Lints no reason excuses take `forbid`, no `#[expect]` lifts a forbidden lint
- Clippy restriction lints take `warn`, clap derives emit `#[allow(clippy::restriction)]` and a `forbid` fails it with E0453
- Lint groups take a priority below single lints, Clippy's `lint_groups_priority` fails a group and a lint sharing one
- Restriction lints join one row at a time with the repository rule each enforces, `blanket_clippy_restriction_lints` flags the whole group
- Lints take an `allow` row only for findings no code change clears (a required form, a contradicting lint)
- Lints demanding docs take `allow`, names and types state each contract
- `clippy.toml` API bans name their replacement
- Clippy's `cargo` group fails a duplicate crate, an `allowed-duplicate-crates` row names one no catalog move clears with each release's dependents

## [05]-[BUILD]

- `build.target-dir` and `build.build-dir` name separate directories, Cargo writes intermediates into the output directory by default
- Profiles sit in root `Cargo.toml`, Cargo ignores a member's profile with a warning
- Unstable Cargo flags sit in `.cargo/config.toml` `[unstable]`, profile keys in a root `cargo-features` row, every unit's `-Z` flags in `build.rustflags`
- `build.rustflags` enters every fingerprint, one row every caller reads keeps one artifact set, a `RUSTFLAGS` variable replaces it and rebuilds all
- Coverage builds keep the LLVM backend, instrumentation exists in no other backend, and cranelift dev builds abort on every panic on macOS
- Coverage builds keep cargo-llvm-cov's own artifact set, Cargo's fingerprint misses the instrumentation its rustc wrapper adds
- Dev builds keep line tables for members and no debug info for dependencies, backtraces read file and line and fuller debug info slows each link
- Installed binaries build under a `dist` profile inheriting `release`, `cargo dylint` builds lint libraries under `release` and fails on `panic = "abort"`
- Members add `build.rs` for work no manifest key or crate does, a build script compiles and runs before its crate on every cold build
- Cargo targets that compile pass `--locked`, a stale `Cargo.lock` fails the run in place of a rewrite
- Cargo targets that compile take `parallelism: false`, each cargo run spreads its units across every core through its own jobserver

## [06]-[MEMBERS]

- Members declare `name`, `edition.workspace = true`, their target, and `[lints] workspace = true`, Cargo assumes edition 2015 and no lints otherwise
- `[workspace.package]` states shared package keys with `version` from `1.0.0`, a member inherits each through `<key>.workspace = true` alone
- Publishable packages inherit every `[workspace.package]` key and state their own `description`, `keywords`, and `categories` for `cargo_common_metadata`
- Unpublishable members state `publish = false` and inherit no `[workspace.package]` key beyond `edition`
- Untested members set `test` and `bench` to `false` on `[lib]` and `[[bin]]` and `doctest = false` on `[lib]`, `clippy --tests` compiles no empty harness
- Members name their crate root beside `Cargo.toml` through `[lib] path` or `[[bin]] path`, Cargo discovers targets under `src/` alone
- Binary packages take their binary's name, the Nx project and installed command then share one name
- Binaries a launchd row runs install through target `install` into `$CARGO_HOME/bin`, the row names a command on mise's `PATH`

## [07]-[CHECKERS]

- Clippy is each member's type check over its library, binary, and test targets, a separate `cargo check` repeats its compile
- Editors and harnesses run rust-analyzer checks as one `check.overrideCommand` Clippy run over those targets
- `check.allTargets` and `check.extraArgs` check a saved example or binary alone through `--example` or `--bin`, an override runs one command per save
- Each rust-analyzer client sets `check.overrideCommand`, `rustc.source`, and `cargo.extraEnv`, flycheck and Cargo keys in `rust-analyzer.toml` go unread
- `cargo.extraEnv` points `CARGO_BUILD_TARGET_DIR` and `CARGO_BUILD_BUILD_DIR` at `rust-analyzer` folders, client Cargo runs then share no directory with Nx targets
- `rustc-dev` installs its lock at `rustc-src/rust/Cargo.lock`, a `mise.toml` `postinstall` hook copies it beside the compiler manifest
- `cargo fmt` runs once at the virtual root, one invocation formats every member
- `rustfmt.toml` holds the keys the pinned nightly rustfmt applies, a nightly key needs no `unstable_features` row and stable rustfmt ignores it
- `rustfmt.toml` sets `edition` and `style_edition`, plain `rustfmt` reads neither from `Cargo.toml`
- `rustfmt.toml` fails the check on a line it cannot fit, a passing check means every line fits `max_width`
- Comment wrapping keys stay unset, they split section dividers
- `cargo modules orphans` fails on a `.rs` file or `mod.rs` folder beside a module file that no module declares

## [08]-[UPGRADES]

- `cargo upgrade` from cargo-edit moves each catalog row to its newest release, breaking releases included, Cargo itself moves no requirement
- `cargo update` follows every catalog move
- `cargo dylint upgrade` on `upgrade:rust` moves the channel and the `clippy_utils` revision together, `mise install` then installs the new date
- Date moves run the checks, a renamed lint fails `unknown_lints` and a stabilized feature fails `stable_features` under `build.warnings = "deny"`
- Prereleases join by hand through a catalog row naming one, `cargo upgrade` takes no prerelease option
- Edition moves run `cargo fix --edition`, then set `[workspace.package]` and `rustfmt.toml` `edition` together

## [09]-[LINT_LIBRARIES]

- Lint libraries hold lints reading a type, trait, resolved path, or cross-item fact, Clippy rows and ast-grep rules own every syntactic form
- One crate `lints` holds every lint
- `lints` is a workspace member `[workspace.metadata.dylint]` names, catalog rows and `[workspace.lints]` reach it as any member
- `lints` exports `register_lints` through `__maybe_mangle!` beside `dylint_library!()`, `unsafe_code = "forbid"` passes the macro-written export
- Lints are one `Warn` module each, ordered as a Clippy lint file (`declare_lint!`, the pass with `impl_lint_pass!` and `impl LateLintPass`)
- Folders group passes sharing queries no other pass reads, their parent module holding those queries, every other lint sits at root
- Queries one pass reads sit in its module, a query read across modules sits in `lib.rs`
- Lint modules are plain `mod` items rustfmt reaches, one root `lints!` call registers each pass by `<module path>::<Pass>`
- Passes holding resolved items construct through a `::new` row from `TyCtxt`, every other pass derives `Default` or comes from `declare_lint_pass!`
- Fixtures are cargo examples `examples/<lint>.rs` and `examples/<lint>_<case>.rs` per entry-function case, importing dev-dependencies the library lacks
- `examples/` stays flat, Cargo discovers `examples/*.rs` and `examples/*/main.rs` alone and a nested fixture takes an `[[example]]` row
- `tests/ui.rs` runs `cargo dylint --no-deps` once over every fixture, comparing diagnostics and suggestion applicability with its inline snapshot
- `cargo insta test -p lints` records a changed snapshot as pending, `cargo insta review` writes it
- Fixtures compile in that run alone under `[workspace.lints]` rustc rows, `typecheck` and `lint:dylint` read library, binary, and test targets
- `host:dylint` members take no `coverage`, the dylint driver runs their lints outside instrumentation
- Drivers stay under `~/.dylint_drivers`, dylint refuses a missing driver directory and `rasm:clean` removes `.cache`
