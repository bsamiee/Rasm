# [RUST]

Cargo owns resolution, lock, and build through root `Cargo.toml`.

## [01]-[WORKSPACE]

- Virtual manifest sits at repository root, rust-analyzer searches one level down and Cargo reads `.cargo/` from the working directory up
- Virtual manifests set `resolver`, Cargo reads no edition from a virtual root and falls back to resolver 1
- `members` globs match crate directories alone, Cargo fails on a glob matching no directory or a directory without `Cargo.toml`
- Toolchain channel, `profile`, and components sit in root `rust-toolchain.toml` alone, mise reads it through `idiomatic_version_file_enable_tools` and a mise `rust` row would outrank it
- The channel is the dated nightly the pinned `clippy_utils` revision builds against, `cargo dylint` strips `RUSTUP_TOOLCHAIN` and reads the file
- The file names a dated nightly and `profile = "minimal"`, a rolling `nightly` resolves a new date per machine and day and an omitted profile reads the machine's rustup profile
- Nightly-only manifest, config, and source forms land with the toolchain file in one change, stable Cargo and rustc refuse them
- Manifests leave `rust-version` out, a stale one holds `cargo add`, resolver fallback, and Clippy MSRV lints below the toolchain mise installs
- `CARGO_HOME` keeps its user default, its `bin/` holds rustup proxies and installed binaries a cache clean must keep
- Cargo subcommands take `cargo:` tool rows beside a `cargo-binstall` row, mise then installs release binaries in place of compiling

## [02]-[CATALOG]

- Catalog rows set `default-features = false` and name each feature a member uses, default features compile code nothing calls
- Members inherit catalog rows whole through `workspace = true`, a feature one member adds compiles its dependency again for per-member builds
- Members reach another member through its catalog `path` row, the catalog states each member's location once

## [03]-[LINTS]

- Lint levels sit in `[workspace.lints]` alone, a crate attribute splits policy and a `RUSTFLAGS` change rebuilds every crate
- Lint rows set `warn` and `.cargo/config.toml` `build.warnings` fails each one, a `-D warnings` flag invalidates the build cache
- Lints no reason excuses take `forbid`, no `#[expect]` lifts a forbidden lint
- Lint groups take a priority below single lints, Clippy's `lint_groups_priority` fails a group and a lint sharing one
- Restriction lints join one row at a time with the repository rule each enforces, `blanket_clippy_restriction_lints` flags the whole group
- Lints take an `allow` row only for findings no code change clears (a required form, a contradicting lint)
- Rust files hold no comment except section dividers, names and types state each contract, and every lint demanding docs takes `allow`
- `clippy.toml` holds lint options and API bans naming their replacement, `[workspace.lints]` holds levels alone

## [04]-[BUILD]

- `build.target-dir` and `build.build-dir` name separate directories, Cargo writes intermediates into the output directory by default
- Profiles sit in root `Cargo.toml`, Cargo ignores a member's profile with a warning
- Unstable Cargo flags sit in `.cargo/config.toml` `[unstable]`, an unstable profile key takes a root `cargo-features` row, a compiler `-Z` flag every unit takes sits in `build.rustflags`
- `build.rustflags` enters every fingerprint, one config row every caller reads keeps one artifact set and a `RUSTFLAGS` variable replaces it whole
- Coverage builds keep the LLVM backend, instrumentation exists in no other backend, and cranelift dev builds abort on every panic on macOS
- Dev builds keep line tables for members and no debug info for dependencies, backtraces read file and line and fuller debug info slows each link
- Installed binaries build under a `dist` profile inheriting `release`, its rows spend compile time on binary speed and size, `cargo dylint` builds lint libraries under `release` and fails on `panic = "abort"`
- Members add `build.rs` for work no manifest key or crate does, a build script compiles and runs before its crate on every cold build
- Cargo targets that compile pass `--locked`, a stale `Cargo.lock` fails the run in place of a rewrite
- Cargo targets that compile take `parallelism: false`, one lock on the build directory serializes every compile

## [05]-[MEMBERS]

- Members declare `name`, `edition.workspace = true`, their target, and `[lints] workspace = true`, Cargo assumes edition 2015 and no lints otherwise
- Members leave `version` out, Cargo reads `0.0.0` and refuses to publish
- Members name their crate root beside `Cargo.toml` through `[lib] path` or `[[bin]] path`, Cargo discovers targets under `src/` alone
- Crate roots hold their crate's code until a measured split, a split crate's module paths are its API with no `pub use` at root
- A crate root's `#![feature]` names a form a line of that crate uses, `unused_features` and `stable_features` fail an idle or stabilized gate under `build.warnings = "deny"`
- Platform code is one function with `cfg` on the one call that differs, or a cross-platform API, a host checks no body its `cfg` excludes
- Binary packages take their binary's name, the Nx project and installed command then share one name
- Binaries a launchd row runs install through target `install` into `$CARGO_HOME/bin`, the row names a command on mise's `PATH`

## [06]-[CHECKERS]

- Clippy is each member's type check, a separate `cargo check` repeats its compile
- Editors and harnesses run rust-analyzer checks through `clippy`, reusing the compile `typecheck` writes
- Editor settings leave rust-analyzer `cargo.targetDir` unset, its `--target-dir` leaves `build-dir` and its lock shared
- `cargo fmt` runs once at the virtual root, one invocation formats every member
- `rustfmt.toml` holds the keys the pinned nightly rustfmt applies, a nightly key needs no `unstable_features` row and stable rustfmt ignores it
- `rustfmt.toml` sets `edition` and `style_edition`, plain `rustfmt` reads neither from `Cargo.toml`
- `rustfmt.toml` fails the check on a line it cannot fit, a passing check means every line fits `max_width`
- Comment width keys split the column-90 section dividers, code holds no other comment for them to wrap
- Unused-dependency checks cover members, a check over catalog rows no member inherits stays off

## [07]-[UPGRADES]

- `cargo upgrade` from cargo-edit moves each catalog row to its newest release, breaking releases included, Cargo itself moves no requirement
- `cargo update` follows every catalog move, a stale `Cargo.lock` fails each `--locked` target
- `cargo dylint upgrade` on `upgrade:rust` moves the channel and the `clippy_utils` revision together, `mise install` then installs the new date
- A date move runs the checks, a renamed lint fails `unknown_lints` and a stabilized feature fails `stable_features` under `build.warnings = "deny"`
- Prereleases join by hand through a catalog row naming one, `cargo upgrade` takes no prerelease option
- Edition moves run `cargo fix --edition`, then set `[workspace.package]` and `rustfmt.toml` `edition` together

## [08]-[LINT_LIBRARIES]

- Lint libraries hold lints reading a type, trait, resolved path, MIR, or cross-item fact, Clippy rows and ast-grep rules own every syntactic form
- One crate per lint under `tools/dylint/`, registering several lints from one crate needs a hand-written `#[unsafe(no_mangle)]` export that `unsafe_code = "forbid"` rejects
- Lint crates are workspace members `[workspace.metadata.dylint]` names, catalog rows and `[workspace.lints]` reach them as any member
- `clippy_utils` takes a git `rev` row and the toolchain channel is the nightly that revision names, `cargo dylint upgrade` moves both in one command
- Lints declare `Warn` and `build.warnings` fails each finding, a kept site takes `#[cfg_attr(dylint_lib = "<lint>", expect(<lint>, reason = ".."))]` under the `unexpected_cfgs` check-cfg row, Clippy reads a bare `#[expect]` of a library lint as unknown
- Fixtures are `examples/<lint>.rs` beside `<lint>.stderr`, run through `dylint_testing::ui_test_examples` from one test, `cargo dylint --fix` over the example proves a fix, the workspace run passes `--lib --bins --tests` so examples stay out of it
- Lint crates test through `cargo nextest run`, coverage instrumentation hides the rustc invocation the fixture runner reads
- Drivers stay under `~/.dylint_drivers`, dylint refuses a missing driver directory and `rasm:clean` removes `.cache`
- Diagnostic messages, help text, and suggestion labels read as one instruction and pass the clean-prose check
