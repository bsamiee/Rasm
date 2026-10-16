# [RUST]

Cargo owns resolution, lock, and build through root `Cargo.toml`.

## [01]-[WORKSPACE]

- Virtual manifest sits at repository root, rust-analyzer searches one level down and Cargo reads `.cargo/` from the working directory up
- Virtual manifests set `resolver`, Cargo reads no edition from a virtual root and falls back to resolver 1
- `members` globs match crate directories alone, Cargo fails on a glob matching no directory or a directory without `Cargo.toml`
- Toolchain channel, `profile`, and components sit in root `rust-toolchain.toml` alone, mise reads it through `idiomatic_version_file_enable_tools` and a mise `rust` row would outrank it
- The channel is the dated nightly the pinned `clippy_utils` revision builds against, `cargo dylint` strips `RUSTUP_TOOLCHAIN` and reads the file
- `[env]` `RUSTUP_TOOLCHAIN = false` stops mise's idiomatic export and leaves selection to the file, builds outside the tree take rustup's default
- The file names a dated nightly and `profile = "minimal"`, a rolling `nightly` resolves a new date per machine and day and an omitted profile reads the machine's rustup profile
- Nightly-only manifest, config, and source forms join in the change that sets the toolchain file, stable Cargo and rustc refuse them
- Manifests leave `rust-version` out, a stale one holds `cargo add`, resolver fallback, and Clippy MSRV lints below the toolchain mise installs
- `CARGO_HOME` keeps its user default, its `bin/` holds rustup proxies and installed binaries a cache clean must keep
- Cargo subcommands take `cargo:` tool rows beside a `cargo-binstall` row, mise then installs release binaries in place of compiling

## [02]-[CATALOG]

- Catalog rows take the crate's default features and add each non-default feature the crate's role needs, a row adding none is a bare version string
- Rows set `default-features = false` only where a default fails the build, adds an implementation the repo replaces (native-tls beside rustls, ring beside aws-lc-rs, a second runtime, allocator, or date-time crate), or harms every member
- A restricted row re-adds every default except the offending one, and its comment states the fact that excludes it
- Members inherit catalog rows whole through `workspace = true`, `feature-unification = "workspace"` builds each member under workspace features
- Members reach another member through its catalog `path` row, the catalog states each member's location once
- Member rows pair `path` with the workspace `version`, Cargo packages no path dependency without a version requirement

## [03]-[LINTS]

- Lint levels sit in `[workspace.lints]` alone, a crate attribute splits policy and a `RUSTFLAGS` change rebuilds every crate
- Lint rows set `warn` and `.cargo/config.toml` `build.warnings` fails each one, a `-D warnings` flag invalidates the build cache
- Lints no reason excuses take `forbid`, no `#[expect]` lifts a forbidden lint
- Clippy restriction lints take `warn`, clap derives emit `#[allow(clippy::restriction)]` and a `forbid` fails it with E0453
- Lint groups take a priority below single lints, Clippy's `lint_groups_priority` fails a group and a lint sharing one
- Restriction lints join one row at a time with the repository rule each enforces, `blanket_clippy_restriction_lints` flags the whole group
- Lints take an `allow` row only for findings no code change clears (a required form, a contradicting lint)
- Names and types state each contract, every lint demanding docs takes `allow`
- `clippy.toml` holds lint options and API bans naming their replacement, `[workspace.lints]` holds levels alone
- Clocks take one owner each, `jiff` for wall-clock time and zones and `tokio::time::Instant` for monotonic time, `clippy.toml` redirects every other clock read to its owner, and code reads the clock at the boundary and passes `Timestamp`, `Zoned`, or `Instant` values inward

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
- Cargo targets that compile take `parallelism: false`, each cargo run spreads its units across every core through its own jobserver

## [05]-[MEMBERS]

- Members declare `name`, `edition.workspace = true`, their target, and `[lints] workspace = true`, Cargo assumes edition 2015 and no lints otherwise
- `[workspace.package]` states shared package keys with `version` from `1.0.0`, a member inherits each through `<key>.workspace = true` alone
- Publishable packages inherit every `[workspace.package]` key
- Publishable packages state their own `description`, `keywords`, and `categories`, Clippy's `cargo_common_metadata` fails a package lacking one
- Unpublishable members state `publish = false` beside `edition.workspace = true` alone
- Untested members set `test` and `bench` to `false` on `[lib]` and `[[bin]]` and `doctest = false` on `[lib]`, `clippy --tests` compiles no empty harness
- Members name their crate root beside `Cargo.toml` through `[lib] path` or `[[bin]] path`, Cargo discovers targets under `src/` alone
- Crate roots hold their crate's code until a measured split, a split crate's module paths are its API with no `pub use` at root
- Crate-private functions, constants, and statics with one reader sit inside that reader, `single_caller` skips a reader that is the entry point
- A crate root's `#![feature]` names a form a line of that crate uses, `unused_features` and `stable_features` fail an idle or stabilized gate under `build.warnings = "deny"`
- Platform code is one function with `cfg` on the one call that differs, or a cross-platform API, a host checks no body its `cfg` excludes
- Binary packages take their binary's name, the Nx project and installed command then share one name
- Binaries a launchd row runs install through target `install` into `$CARGO_HOME/bin`, the row names a command on mise's `PATH`

## [06]-[CHECKERS]

- Clippy is each member's type check over its library, binary, and test targets, a separate `cargo check` repeats its compile
- Editors and harnesses run rust-analyzer checks as one `check.overrideCommand` Clippy run over those targets, the harness reusing the compile `typecheck` writes
- `check.allTargets` and `check.extraArgs` still check a saved example or binary alone through `--example` or `--bin`, an override runs one command on every save
- Each rust-analyzer client's settings state `check.overrideCommand` and `rustc.source`, flycheck and Cargo keys in a project `rust-analyzer.toml` go unread
- `rustc-dev` ships the lock at `rustc-src/rust/Cargo.lock`, a `mise.toml` `postinstall` hook copies it beside the compiler manifest
- Editor rust-analyzer sets `cargo.extraEnv` `CARGO_BUILD_TARGET_DIR` and `CARGO_BUILD_BUILD_DIR`, every Cargo run it starts then shares no directory with agents
- `cargo fmt` runs once at the virtual root, one invocation formats every member
- `rustfmt.toml` holds the keys the pinned nightly rustfmt applies, a nightly key needs no `unstable_features` row and stable rustfmt ignores it
- `rustfmt.toml` sets `edition` and `style_edition`, plain `rustfmt` reads neither from `Cargo.toml`
- `rustfmt.toml` fails the check on a line it cannot fit, a passing check means every line fits `max_width`
- Comment width keys split the column-90 section dividers, code holds no other comment for them to wrap
- `cargo modules orphans` fails on a `.rs` file or `mod.rs` folder beside a module file that no module declares
- Coverage keeps cargo-llvm-cov's own artifact set, Cargo's fingerprint misses the instrumentation its rustc wrapper adds

## [07]-[UPGRADES]

- `cargo upgrade` from cargo-edit moves each catalog row to its newest release, breaking releases included, Cargo itself moves no requirement
- `cargo update` follows every catalog move, a stale `Cargo.lock` fails each `--locked` target
- `cargo dylint upgrade` on `upgrade:rust` moves the channel and the `clippy_utils` revision together, `mise install` then installs the new date
- A date move runs the checks, a renamed lint fails `unknown_lints` and a stabilized feature fails `stable_features` under `build.warnings = "deny"`
- Prereleases join by hand through a catalog row naming one, `cargo upgrade` takes no prerelease option
- Edition moves run `cargo fix --edition`, then set `[workspace.package]` and `rustfmt.toml` `edition` together

## [08]-[LINT_LIBRARIES]

- Lint libraries hold lints reading a type, trait, resolved path, or cross-item fact, Clippy rows and ast-grep rules own every syntactic form
- One crate `lints` holds every lint
- `lints` exports `register_lints` in `__maybe_mangle!` beside `dylint_library!()`, `unsafe_code = "forbid"` passes an export an external macro writes
- Lints are one module each, ordered as a Clippy lint file (`declare_lint!`, the pass with `impl_lint_pass!` and `impl LateLintPass`)
- Folders group passes sharing queries no other pass reads, the parent module holds those queries and `lints!` rows, other lints sit at root
- Queries one pass reads sit in its module, a query read across modules sits in `lib.rs`
- `lints` is a workspace member `[workspace.metadata.dylint]` names, catalog rows and `[workspace.lints]` reach it as any member
- Lints declare `Warn` and `build.warnings` fails each finding
- Lint modules are plain `mod` items rustfmt reaches, each `lints!` row `<module>::<Pass>` registers the pass, a row ending in `::new` constructs the pass from the `TyCtxt`
- Passes holding resolved items construct through `new(tcx)`, every other pass derives `Default`, a `declare_lint_pass!` pass inside the macro
- Fixtures are cargo examples `examples/<lint>.rs`, one per lint and `examples/<lint>_<case>.rs` per case a lint reads from the entry function, a build target alone resolves the dev-dependencies a fixture imports
- `examples/` stays flat, Cargo discovers `examples/*.rs` and `examples/*/main.rs` alone and a nested fixture takes an `[[example]]` row
- `tests/ui.rs` runs `cargo dylint` once over every fixture and compares every lint's diagnostics and suggestion applicability with `tests/ui.snap`
- `cargo insta test -p lints` records a changed snapshot as pending, `cargo insta review` writes it
- Fixtures compile in that run alone under the `[workspace.lints]` rustc rows, `typecheck` and `lint:dylint` take the library, binary, and test targets each member's metadata lists and read no example
- `host:dylint` members take no `coverage`, the dylint driver runs their lints outside instrumentation
- Drivers stay under `~/.dylint_drivers`, dylint refuses a missing driver directory and `rasm:clean` removes `.cache`
- Diagnostic messages, help text, and suggestion labels read as one instruction

## [09]-[MCP]

MCP servers build on `rasm-mcp`, and `rasm-mcp-bridge` serves third-party stdio servers through the same host.

- Servers start through `rasm_mcp::run` with a clap `Parser` deriving `#[command(version, about)]`, which names `serverInfo` and log service fields
- `MCP_LISTEN` names the bound address, a non-loopback address takes `MCP_RESOURCE` and `MCP_AUTHORIZATION_SERVERS`
- Loopback endpoints run unauthenticated on a single-user host, authorized endpoints validate bearer JWTs through `tower-oauth2-resource-server`
- `Server::new` takes rmcp `#[tool_router]`, `PromptRouter`, and `ResourceRouter` values, the library implements `ServerHandler` and its versions
- Parameters are structs with `#[serde(deny_unknown_fields)]`, descriptions go through `#[tool(description)]` and `#[schemars(description)]`
- Tools return `Json<T>` on success, image tools return content blocks and declare no output schema
- Failures return `Failure<E>` over a `#[serde(tag = "_tag")]` thiserror enum with a fact then an action, `ErrorData` holds protocol faults
- Read tools set `read_only_hint = true`, local-host tools `open_world_hint = false`, additive writes `destructive_hint = false`
- Destructive calls a client cancels run to completion and record their result in the `mcp://<server>/executions` resource
- Long tools set stage text through `Extension<watch::Sender<Cow<'static, str>>>`, the first progress leaves at call start
- Task results go to clients declaring `io.modelcontextprotocol/tasks` alone, progress keeps every other long call alive
- Tools ask the user through `rasm_mcp::elicit` and return `Elicited<Json<T>>`, `Peer::elicit` alone breaks the 2026-07-28 revision
- Resource templates use RFC 6570 level 1 `{var}` variables, reference data is a `ResourceRouter` row, and large outputs link a resource
- Tool, prompt, and resource changes publish through `Server` watches, `server.changes().update(uri)` notifies subscribers of a resource
- Instructions keep their first 512 characters self-contained and stay under 4,096, tool descriptions open with search words in one sentence
- Tool outputs stay under 10,000 tokens, a larger result sets `Anthropic::MaxResultSizeChars` or moves to a resource
- Request outcome fields sit on the `request` span, events hold `exception.*` alone, and errors render through `std::error::Report`
- Debug filters write tool arguments into `.artifacts/logs`, and into OTLP when `OTEL_EXPORTER_OTLP_ENDPOINT` is set
- Codex runs a stdio server on the 2026-07-28 lifecycle when its `env` sets `CODEX_MCP_PROTOCOL_VERSION`, HTTP servers take no setting
- Codex `tool_timeout_sec` defaults to 300 seconds, Claude Code's first-byte timer stays at 60 seconds unless `.mcp.json` `timeout` raises it
- Bridge rows in `mise.toml` pass `--listen 127.0.0.1:<port>` and one `--hide <glob>` per tool a skill replaces, with `MISE_PREFER_OFFLINE` set
