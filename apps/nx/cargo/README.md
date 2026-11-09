# @rasm/nx-cargo

Nx plugin for Cargo workspaces. It reads each workspace through one `cargo metadata --format-version 1 --no-deps --offline` call and its `Cargo.lock`, and infers a project per member with targets that run the commands a terminal runs from the workspace root.

## [01]-[SETUP]

```json
{ "plugins": ["@rasm/nx-cargo"] }
```

No plugin options exist, since Cargo's own configuration states every fact it reads: `build.target-dir` and `build.build-dir` place output, `rust-toolchain.toml` picks the toolchain, `clippy.toml`, `.config/nextest.toml`, `[workspace.metadata.dylint]`, and `deny.toml` decide which checkers run. Under CI (Nx `isCI()`), compile and publish commands add `--locked`.

## [02]-[TARGETS]

| [INDEX] | [TARGET]                  | [PROJECT]          | [COMMAND]                                                                                                                           | [CACHE]                   |
| :-----: | :------------------------ | :----------------- | :---------------------------------------------------------------------------------------------------------------------------------- | :------------------------ |
|  [01]   | `build`                   | Member             | `cargo build`; `--profile <name>` per configuration (`release`, custom profiles)                                                    | No                        |
|  [02]   | `test`                    | Member             | `cargo nextest run` and `cargo test --doc` with nextest configured, else `cargo test`                                               | Yes                       |
|  [03]   | `check--clippy`           | Member             | `cargo clippy --all-targets --keep-going` under `CARGO_BUILD_WARNINGS=deny`; `cargo check` without a `clippy.toml` (`check--cargo`) | Yes                       |
|  [04]   | `fix--clippy`             | Member             | `cargo clippy --fix --allow-dirty --all-targets --keep-going`; `cargo fix` without a `clippy.toml` (`fix--cargo`)                   | No                        |
|  [05]   | `check--dylint`           | Member             | `cargo dylint --all --fail-on-no-libraries -- --all-targets …` when `[workspace.metadata.dylint]` lists libraries                   | Yes                       |
|  [06]   | `check--cargo-deny`       | Workspace          | `cargo deny check --workspace --deny warnings <checks deny.toml configures>`                                                        | No when `advisories` runs |
|  [07]   | `upgrade--cargo`          | Workspace          | `cargo upgrade --incompatible allow`, then `cargo update`                                                                           | No                        |
|  [08]   | `check`, `fix`, `upgrade` | Both               | `nx:noop` over every `<verb>--*` target, with `...` and the union of their inputs, so other plugins on the same project add theirs  | —                         |
|  [09]   | `nx-release-publish`      | Publishable member | `cargo publish`, skipped when crates.io already holds the version                                                                   | —                         |

Every task of one command runs in one cargo process (`preferBatch`), selecting each package by package ID spec, so Cargo takes its build-directory lock once and unifies features across the selection. Each task gets its own result from cargo's JSON messages: a package with an error, or under `deny` a lint warning, fails its task alone, a task whose package depends on a crate that failed to compile is skipped, and the remaining tasks run again in the same command to take cargo's own verdict; a failure no message names (a test, a Cargo manifest lint) splits the selection in halves until each failing task stands alone. Green tasks cache however many others fail. Edges are `implicit`; external nodes `cargo:<name>@<version>` carry the lock checksum (or Git source) as their hash, so a lock change reruns only the projects reaching the moved package.

## [03]-[RELEASE]

Publishable members (a `version` and no `publish = false`) get `release.version.versionActions: "@rasm/nx-cargo/release/version-actions"`, which writes `[package] version` or the inherited `[workspace.package] version`, rewrites every requirement on a released member (dependency tables, target tables, `[workspace.dependencies]`) keeping its operator and the file's layout, and refreshes each `Cargo.lock` with `cargo update --workspace`.
