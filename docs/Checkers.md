# [CHECKERS]

Strictness switches and diagnostics of the checkers `hk` and Nx run.

## [01]-[PRINCIPLES]

Rules every checker configuration follows:
- Checkers select their widest rule set and fail on every finding by default or by a switch their configuration or command line states
- Rules leave a selection where another checker owns their defect or a repository rule contradicts them, and their row comment names the owner or rule
- Formatters check by diff and print the change `fix` applies as their finding

Owners of language checker strictness:
- Use `python/Typing.md` and `python/Testing.md` for Python checker strictness
- Use `typescript/TSConfig.md`, `typescript/Testing.md`, and `typescript/Publishing.md` for TypeScript checker strictness

## [02]-[STRICTNESS]

Switch failing each checker on every finding, and the file stating it:

| [INDEX] | [CHECKER]          | [SWITCH]                                                          | [STATED_IN]                                  |
| :-----: | :----------------- | :---------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `hk`               | `fail_fast = false`, `hk validate --deny-warnings`                | `.config/hk.pkl`                             |
|  [02]   | `ast-grep`         | `severity: error` on every rule                                   | `tools/ast-grep/rules`                       |
|  [03]   | `oxlint`           | Every category at `error`, `typeAware`, `--deny-warnings`         | `.oxlintrc.json`, `.config/hk.pkl`           |
|  [04]   | ESLint             | `@nx/nx-plugin-checks` at `error`                                 | `products/developer/nx/eslint.config.ts`     |
|  [05]   | `knip`             | `treatConfigHintsAsErrors`                                        | `.config/knip.json`                          |
|  [06]   | `jscpd`            | `exitCode: 1` on any clone                                        | `.config/jscpd.json`                         |
|  [07]   | `ruff`             | `select = ["ALL"]` under `preview = true`                         | Root `pyproject.toml`                        |
|  [08]   | `rustc`            | `warnings` at `deny`                                              | `Cargo.toml` `[workspace.lints.rust]`        |
|  [09]   | Clippy             | `all`, `pedantic`, `nursery`, `cargo` at `deny`                   | `Cargo.toml`, `.config/clippy.toml`          |
|  [10]   | Cargo              | `default` and `pedantic` at `deny`                                | `Cargo.toml` `[workspace.lints.cargo]`       |
|  [11]   | `cargo deny`       | `--deny warnings`, `bans`, `licenses`, and `sources` allow lists  | `@rasm/nx-cargo`, `.config/deny.toml`        |
|  [12]   | `rustdoc`          | `all` at `deny`                                                   | `Cargo.toml` `[workspace.lints.rustdoc]`     |
|  [13]   | Dylint             | Lints declared at `Deny`, `--fail-on-no-libraries`                | `tools/dylint`, `@rasm/nx-cargo`             |
|  [14]   | `rustfmt`          | `error_on_line_overflow`, `error_on_unformatted`                  | `.config/rustfmt.toml`                       |
|  [15]   | `cargo llvm-cov`   | `CARGO_LLVM_COV_DENY_WARNINGS`                                    | `.config/mise.toml`                          |
|  [16]   | `cargo nextest`    | `leak-timeout` result `fail`                                      | `.config/nextest.toml`                       |
|  [17]   | C# compiler        | `TreatWarningsAsErrors`, `AnalysisLevel` `latest-all`             | `Directory.Build.props`                      |
|  [18]   | C# code style      | `EnforceCodeStyleInBuild`                                         | `Directory.Build.props`                      |
|  [19]   | Roslyn analyzers   | `dotnet_analyzer_diagnostic.severity = error`                     | `.config/.globalconfig`                      |
|  [20]   | Meziantou          | `MeziantouAnalysisMode` `all-errors`                              | `Directory.Build.props`                      |
|  [21]   | MSBuild            | `MSBuildTreatWarningsAsErrors`                                    | `Directory.Build.props`                      |
|  [22]   | Clang              | `CMAKE_COMPILE_WARNING_AS_ERROR`, `-Wall -Wextra -Wpedantic`      | `CMakeLists.txt`                             |
|  [23]   | `clang-tidy`       | `WarningsAsErrors: '*'`                                           | `.config/.clang-tidy`                        |
|  [24]   | CMake              | Every `errors` key of configure preset `tree`                     | `CMakePresets.json`                          |
|  [25]   | Swift compiler     | `.treatAllWarnings(as: .error)`, linker `-fatal_warnings`         | `Package.swift`                              |
|  [26]   | Xcode              | `SWIFT_TREAT_WARNINGS_AS_ERRORS`, `GCC_TREAT_WARNINGS_AS_ERRORS`  | `Xcode.xcconfig`                             |
|  [27]   | SwiftLint          | `strict: true`                                                    | `.config/swiftlint.yml`                      |
|  [28]   | `swift format`     | `lint --strict`                                                   | `.config/hk.pkl`                             |
|  [29]   | `plutil`           | `-lint -s`, exit 1 on any invalid file                            | `.config/hk.pkl`                             |
|  [30]   | `javac`            | `-Xlint:all,-path -Werror`                                        | `.config/hk.pkl`                             |
|  [31]   | Error Prone        | `-XepAllDisabledChecksAsWarnings`, `-XepAllSuggestionsAsWarnings` | `.config/hk.pkl`                             |
|  [32]   | PMD                | Exit 4 on any violation                                           | Default                                      |
|  [33]   | ShellCheck         | `enable=all`                                                      | `.config/shellcheckrc`                       |
|  [34]   | `ryl`              | `ALL = "enable"`, `[lint] strict`                                 | `.config/ryl.toml`                           |
|  [35]   | `check-jsonschema` | Exit 1 on any schema error                                        | Default                                      |
|  [36]   | Tombi              | `--error-on-warnings`, mise schema                                | `.config/hk.pkl`, root `pyproject.toml`      |
|  [37]   | Squawk             | Exit 1 on any finding                                             | Default                                      |
|  [38]   | `clang-format`     | `--dry-run -Werror`                                               | `.config/hk.pkl`                             |
|  [39]   | `gersemi`          | `--warnings-as-errors`                                            | `.config/hk.pkl`                             |
|  [40]   | `jactionlint`      | `--strict-exit`, profile `correctness`, rules raised to `error`   | `.config/hk.pkl`, `.github/jactionlint.yaml` |

Rule selection behind each switch:
- Clippy documents its `restriction` group as unfit to enable whole, and its lints join by name
- `@rasm/nx-cargo` runs each `cargo deny` check with a table in `.config/deny.toml`
- `cargo doc` applies `[workspace.lints.rustdoc]`, and `cargo clippy` skips it
- Error Prone raises off and suggestion checks to warnings, and `-Werror` fails each one
- SwiftLint runs the rules `only_rules` lists alone
- `ryl` enables no rule without a configuration
- `ast-grep` parses SQL templates as PostgreSQL beside `@effect/sql-pg`, as SQLite beside `@effect/sql-sqlite-*`, `-d1`, or `-libsql`, else as both
- Files importing no client name their dialect by suffix `.sqlite.ts` or `.pg.ts`, and rules of the other dialect list it in `ignores`
- Squawk prints each finding as `warning` and exits 1, and `excluded_rules` alone decides what passes
- `mise` exits 0 on unknown fields, and Tombi lints every mise file against mise's schema through its catalog and `[[tool.tombi.schemas]]`
- `dotnet format style --verify-no-changes` reports the IDE rules no build runs
- `jactionlint` profiles widen from `correctness` through `default` to `pedantic`, and `--strict-exit` fails `warn` and `info` findings

## [03]-[DIAGNOSTICS]

Output each failing step prints:
- `hk check` runs every step before exiting nonzero and prints each failing step's output
- Steps setting `diagnostic_format` feed the diagnostics `hk check --sarif <file>` and `--format json` write, other steps keep raw output alone
- `hk` prints `oxlint` SARIF as `<path>:<line>:<column>: <severity>: <message> [<rule>]` lines
- Steps without `diagnostic_format` print a form no `hk` parser reads:
    - `plutil` puts the line inside its message
    - `tombi lint` writes JSON to a file alone
    - `check-jsonschema` names JSON paths
    - Diff steps print patches
- `grammars` skips while its `cache_inputs` contents pass, and `HK_CACHE=0 hk check -S grammars` rebuilds a removed `.artifacts/cmake/grammars`
- C# compiles write SARIF 2.1 to `$(IntermediateOutputPath)<project>.sarif`, and `check--dotnet-build` fails on its unsuppressed warnings and errors
- `check--clang-tidy` reports failed header-set verification beside each finding
