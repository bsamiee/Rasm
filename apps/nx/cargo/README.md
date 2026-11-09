# @rasm/nx-cargo

Nx plugin for Cargo workspaces. It reads each workspace through one `cargo metadata --format-version 1 --no-deps --offline` call and its `Cargo.lock`, and infers a project per member with targets that run the commands a terminal runs from the workspace root.

## [01]-[SETUP]

```json
{ "plugins": ["@rasm/nx-cargo"] }
```

No plugin options exist, since Cargo's own configuration states every fact it reads: `build.target-dir` and `build.build-dir` place output, `rust-toolchain.toml` picks the toolchain, `[profile.*]` tables name build configurations and panic strategies, `clippy.toml`, `.config/nextest.toml`, `[workspace.metadata.dylint]`, and `deny.toml` decide which checkers run. Under CI (Nx `isCI()`), compile and publish commands add `--locked`.

## [02]-[TARGETS]

| [INDEX] | [TARGET]                           | [PROJECT]          | [COMMAND]                                                                                                                                   | [CACHE]                   |
| :-----: | :--------------------------------- | :----------------- | :------------------------------------------------------------------------------------------------------------------------------------------ | :------------------------ |
|  [01]   | `build--cargo`                     | Member             | `cargo build`; `--profile <name>` per configuration (`release`, custom profiles)                                                            | No                        |
|  [02]   | `test`                             | Member             | `cargo nextest run` and `cargo test --doc` with nextest configured, else `cargo test`                                                       | Yes                       |
|  [03]   | `check--clippy`                    | Member             | `cargo clippy <targets> --keep-going` when a `clippy.toml` or a `[lints.clippy]` table declares Clippy, else `cargo check` (`check--cargo`) | Yes                       |
|  [04]   | `fix--clippy`                      | Member             | `cargo clippy --fix --allow-dirty <targets> --keep-going`, else `cargo fix` (`fix--cargo`)                                                  | No                        |
|  [05]   | `check--dylint`                    | Member             | `cargo dylint --all --fail-on-no-libraries -- <targets> …` when `[workspace.metadata.dylint]` lists libraries                               | Yes                       |
|  [06]   | `check--cargo-deny`                | Workspace          | `cargo deny check --workspace --deny warnings <checks deny.toml configures>`                                                                | No when `advisories` runs |
|  [07]   | `upgrade--cargo`                   | Workspace          | `cargo upgrade --incompatible allow`, then `cargo update`                                                                                   | No                        |
|  [08]   | `build`, `check`, `fix`, `upgrade` | Both               | `nx:noop` over every `<verb>--*` target, with `...` and the union of their inputs, so other plugins on the same project add theirs; arguments after `--` reach every member          | —                         |
|  [09]   | `nx-release-publish`               | Publishable member | `cargo publish`, skipped when crates.io already holds the version                                                                           | —                         |

Checkers select `--lib --bins --tests --benches`, every target but examples, as Dylint's own lint pass does: a Dylint library keeps its lint fixtures as `[[example]]` targets, the form that gives fixtures the package's dev-dependencies, and `--fix` over them would rewrite the fixtures; `cargo test` and nextest still compile every example. Each profile whose `panic` strategy differs from `dev` adds one checker run under `--profile <name>`: Cargo passes a profile's `-C panic` to every crate it builds except test harnesses, so `cfg(panic = "abort")` code compiles under that profile alone.

Every task of one command runs in one cargo process (`preferBatch`), selecting each package by package ID spec, so Cargo takes its build-directory lock once and unifies features across the selection. Each task takes its result from its own package's report: compile commands read cargo's JSON messages (an `error` fails the package, a `compiler-artifact` proves it built), `cargo nextest run` reads nextest's `libtest-json-plus` suites per package (format version `0.1`, enabled through `NEXTEST_EXPERIMENTAL_LIBTEST_JSON=1`, nextest's documented switch), and `cargo test` and `cargo deny` take the exit status alone. A task whose package depends on a package that failed is skipped, as is a task with no report while another package failed; with no failure named, every unreported task fails with the whole log. Lint levels come from `[lints]` alone: a level of `deny` makes a finding an error, so no warning rule sits in the plugin. Green tasks cache however many others fail. Edges are `implicit`; external nodes `cargo:<name>@<version>` carry the lock checksum (or Git source) as their hash, so a lock change reruns only the projects reaching the moved package.

`upgrade--cargo` moves every requirement cargo-edit treats as unpinned: requirements holding `=`, `<`, `<=`, or `*`, and renamed dependencies, stay, so a hold reads `">=x.y.z, <next"` and `cargo update` still takes patch releases inside it. cargo-edit moves a requirement to a prerelease only when it already names one, and leaves Git dependencies to `cargo update`.

## [03]-[RELEASE]

Publishable members (a `version` and no `publish = false`) get `release.version.versionActions: "@rasm/nx-cargo/release/version-actions"`, which writes `[package] version` or the inherited `[workspace.package] version`, rewrites every requirement on a released member (dependency tables, target tables, `[workspace.dependencies]`) keeping its operator and the file's layout, and refreshes each `Cargo.lock` with `cargo update --workspace`.
