# [JAVA]

google-java-format owns formatting, PMD owns lint, jdtls owns editor diagnostics.

## [01]-[FORMATTER]

- `google-java-format` takes files and no directory, `--dry-run --set-exit-if-changed` lints and `--replace` writes, `--aosp` sets 4-space indent
- Formatter has no configuration file and no `.editorconfig` row, the mise row is a native binary that runs without a JDK

## [02]-[CHECKS]

- `pmd check` runs on the mise `java` row's JDK
- `pmd check` takes files or directories, `--rulesets <file>` names the ruleset, `--use-version java-<n>` Java level, `--cache <file>` cache location
- Ruleset references each category whole and excludes rules by name, a threshold rule keeps its default or goes
- PMD release tags take a `pmd_releases/` prefix, and a `-SNAPSHOT` tag follows each release

## [03]-[LANGUAGE_SERVER]

jdtls type-checks Ghidra scripts against Ghidra jars on each edit, a check no `rasm:lint` command runs:
- `workspaceFolder` names the scripts directory, a root workspace refreshes every repository file before Ghidra jars attach
- Nix `jdtls` runs on its own JDK, the default `java.configuration.runtimes` row at `${JAVA_HOME}` sets the project JRE and compliance
- Workspace data stays at the Nix launcher default under `~/Library/Caches/jdtls`, a `-data` folder inside the repository root breaks the project link
