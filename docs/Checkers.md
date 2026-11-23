# [CHECKERS]

Rule selection and diagnostics of the checkers `hk` and Nx run.

## [01]-[PRINCIPLES]

Rules every checker configuration follows:
- Checkers select their widest rule set, and a switch in their configuration or command line fails each finding their default passes
- Rules leave a selection where another checker owns their defect or a repository rule contradicts them, and their row comment names the owner or rule
- Configurations written as code hold no comment, and `[02]` names the owner or rule of each row leaving their selection
- Formatters check by diff and print the change `fix` applies as their finding

Owners of language checker settings:
- Use `python/Typing.md` and `python/Testing.md` for Python checkers
- Use `typescript/TSConfig.md`, `typescript/Testing.md`, and `typescript/Publishing.md` for TypeScript checkers

## [02]-[RULE_SELECTION]

Checker behavior that decides a switch or a rule row:
- PMD, `check-jsonschema`, and Squawk exit nonzero on any finding with no switch
- Stylelint exits 0 on findings a rule reports at `warning` severity, and `--max-warnings 0` fails them
- Stylelint exits 0 on a deprecated rule it reports as a Node `DeprecationWarning`, and `NODE_OPTIONS=--throw-deprecation` fails every Node deprecation of a run
- Stylelint caches each style file without its reference files, and `--no-cache` overrides a config turning `cache` on
- Stylelint `no-descending-specificity` flags a nested part after a more specific part of another component, as each class rule nests its own parts
- Stylelint `projectwallace/no-duplicate-custom-properties` flags a variant, state, or query reassigning a custom property, as `declaration-block-no-duplicate-custom-properties` owns one block
- Clippy documents its `restriction` group as unfit to enable whole, and its lints join by name
- `@rasm/nx-cargo` runs each `cargo deny` check with a table in `.config/deny.toml`
- `cargo doc` applies `[workspace.lints.rustdoc]`, and `cargo clippy` skips it
- Error Prone raises off and suggestion checks to warnings, and `-Werror` fails each one
- SwiftLint runs the rules `only_rules` lists alone
- `ryl` enables no rule without a configuration
- `ast-grep` parses SQL templates as PostgreSQL beside `@effect/sql-pg`, as SQLite beside `@effect/sql-sqlite-*`, `-d1`, or `-libsql`, else as both
- Files importing no client name their dialect by suffix `.sqlite.ts` or `.pg.ts`, and rules of the other dialect list it in `ignores`
- Squawk prints each finding as `warning`, and `excluded_rules` alone decides what passes
- `mise` exits 0 on unknown fields, and Tombi lints every mise file against mise's schema through its catalog and `[[tool.tombi.schemas]]`
- `dotnet format style --verify-no-changes` reports the IDE rules no build runs
- `hk` lints named CSS files and root `check--stylelint` every CSS file Git lists, as `referenceFiles` rules read tokens another file declares
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
