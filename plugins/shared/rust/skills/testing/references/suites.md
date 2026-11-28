# [SUITES]

Test packages hold a workspace's tests, and `cargo nextest` builds every test binary, lists its tests, and runs each test in a process of its own, reading `.config/nextest.toml` at the workspace root.

## [01]-[PACKAGES]

Tests reach code where its callers do, and each package compiles its tests into one binary:

| [INDEX] | [SUBJECT]             | [PLACE]                                                     |
| :-----: | :-------------------- | :---------------------------------------------------------- |
|  [01]   | Crates of a workspace | Test package, members as dev-dependencies                   |
|  [02]   | Lone crate            | `tests/<crate>/main.rs` of that crate, a module per subject |
|  [03]   | Built binary          | Test package holding it through an artifact dependency      |
|  [04]   | Real services         | Package of its own outside nextest's default filter         |

```toml
[package]
name             = "<package>"
edition.workspace = true
publish          = false
autotests        = false

[lib]
path    = "lib.rs"
test    = false
doctest = false

[[test]]
name = "<target>"
path = "<target>/main.rs"

[[bin]]
name = "peer"
path = "peer/main.rs"
test = false

[dev-dependencies]
<crate>.workspace   = true
<support>.workspace = true
```

`<package>` holds the tests of `<crate>`, `[lib]` the types its fixture binary `peer` and its tests share, and `<support>` the workspace's support crate:
- Cargo builds every `tests/*.rs` file and every `tests/*/main.rs` as a crate of its own, each linking every dependency again, and one `[[test]]` target holds `mod <module>;` lines
- Packages whose test root is no auto-discovered path declare that `[[test]]` target and set `autotests = false`
- `[[test]]` targets receive `CARGO_BIN_EXE_<bin>` and `CARGO_TARGET_TMPDIR`, which a library harness (`#![cfg(test)]` in `lib.rs`) lacks, and a test package takes a `[[test]]` target for them
- Second targets exist for a case needing a crate attribute of its own (`#![feature(..)]`, a global allocator) or `harness = false`, and a feature set takes a second package
- Modules name the subject they test, never a crate or a mechanism

## [02]-[PROCESSES]

One process per test decides what state a test can share:
- Statics, `OnceLock` values, and `#[once]` fixtures build once per test, and nothing built in one test reaches another
- In-process locks between tests order nothing, and a test group serializes tests sharing an outside resource
- Process environment stays unchanged inside a test, and a value a test sets reaches code as a parameter or a child `Command`'s `env`
- Doctests stay out of nextest, and `cargo test --doc` runs them where a crate keeps any

## [03]-[CONFIGURATION]

Profiles state each limit once, and an override names the tests it changes and its reason beside the configuration:

```toml
nextest-version = { required = "<version>" }

[profile.default]
fail-fast      = false
failure-output = "immediate-final"
default-filter = "not package(<services>) | package(<services>) & test(/^<module>::/)"
leak-timeout   = { period = "200ms", result = "fail" }
slow-timeout   = { period = "60s", terminate-after = 3 }

[[profile.default.overrides]]
filter       = "package(<package>) & test(/^<module>::/)"
slow-timeout = { period = "15s", terminate-after = 2 }

[profile.services]
default-filter = "package(<services>)"

[[profile.services.overrides]]
filter     = "test(/^<module>::/)"
test-group = "<group>"

[test-groups]
<group> = { max-threads = 1 }
```

`<services>` is the package of tests against real services, its `<module>` of in-process cases running in the default profile, `<group>` a resource its tests share, and `<version>` the nextest release checks before merge run:
- `default-filter` selects what a plain run takes, `-E` intersects with it, and `-P services` runs that profile, which inherits every other key of `default`
- `fail-fast = false` runs every test after a failure, and a mutant's log then lists each test it fails
- Overrides state the measured time behind each raised `slow-timeout` beside the configuration
- `leak-timeout` with `result = "fail"` fails a test whose child outlives it while holding its output
- `slow-timeout` with `terminate-after` ends a hung test: SIGTERM to its process group, then SIGKILL to the test process once it outlives the grace period
- Filters name tests by `package()`, `binary_id(<package>::<target>)`, and `test()` over the whole test name
- Filters naming an absent package or binary refuse the configuration with exit status 96, and the same filter in `-E` exits 94
- Removals of a package, binary, or test module delete each override naming it in the same change
- `test-group` caps how many tests sharing one resource run at once, and `threads-required = "num-test-threads"` runs a test alone
- Setup scripts (`[scripts.setup.<script>]` with `[[profile.<profile>.scripts]]`) need `experimental = ["setup-scripts"]`, run before the tests their filter matches, and pass variables through `$NEXTEST_ENV`

## [04]-[BINARIES]

Tests run a binary the way a person does, through its path:
- Binaries of a test's own package are `env!("CARGO_BIN_EXE_<bin>")`, built before the test target
- Peers a test runs as a process are `[[bin]]` targets of the test package with `test = false`, each flag selecting one behavior a case needs (`--silent`, `--oversized`, a refused start)
- Binaries of another package come through an artifact dependency, read as `env!("CARGO_BIN_FILE_<DEPENDENCY>_<bin>")`
- `<DEPENDENCY>` is the dependency's name uppercased with underscores for dashes, and `<bin>` keeps its spelling
- `CARGO_BIN_FILE_<DEPENDENCY>` with no suffix exists only where the bin name equals the dependency's package name
- `lib = true` keeps the dependency's library importable beside its binary
- Artifact dependencies are a nightly Cargo feature, and `.cargo/config.toml` holds `[unstable] bindeps = true`, as nextest's own `cargo metadata` call reads that key and no `-Z` flag
- Stable toolchains reach a binary through `CARGO_BIN_EXE_<bin>` alone, from tests inside the binary's own package
- Artifact variables reach `env!` at compile time under both runners, and the runtime environment holds them under `cargo test` alone

`Cargo.toml` holds the artifact dependency, `<cli>` the package holding the binary and `<folder>` its path:

```toml
[workspace.dependencies]
<cli> = { path = "<folder>", artifact = "bin", lib = true }
```

`.cargo/config.toml` enables it for every runner:

```toml
[unstable]
bindeps = true
```

## [05]-[HARNESSES]

`harness = false` targets run their own `main`:
- `libtest-mimic` builds one `Trial` per case discovered at run time (a corpus file, a fixture binary), and nextest lists, selects, and reports each one alone
- `Trial::bench` prints `: bench`, which nextest's listing refuses, and a harness run under nextest registers `Trial::test` trials alone
- `criterion` benches take `[[bench]]` with `harness = false` and answer nextest's listing as `: benchmark`
