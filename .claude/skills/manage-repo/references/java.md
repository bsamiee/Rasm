# [JAVA]

google-java-format owns formatting, PMD owns lint, jdtls owns editor diagnostics.

## [01]-[FORMATTER]

- `google-java-format` takes files and no directory, `--dry-run --set-exit-if-changed` lints and `--replace` writes, `--aosp` sets 4-space indent
- Formatter has no configuration file and no `.editorconfig` row, the mise row is a native binary that runs without a JDK

## [02]-[CHECKS]

- `pmd check` runs on the mise `java` row's JDK
- `pmd check` takes files or directories, `--rulesets <file>` names the ruleset, `--use-version java-<n>` Java level, `--cache <file>` cache location
- Ruleset references each category whole and excludes rules by name, a threshold rule keeps its default or goes
- PMD release tags take a `pmd_releases/` prefix and a `-SNAPSHOT` tag follows each release, a mise `github:` row pins the release

## [03]-[LANGUAGE_SERVER]

jdtls type-checks Ghidra scripts against Ghidra jars on each edit, a check no `rasm:lint` command runs:
- `workspaceFolder` names the scripts directory, a workspace at repository root refreshes every file under it before Ghidra jars attach
- Settings sit in the `settings` field, Claude Code sends it by `workspace/didChangeConfiguration` after `initialize`
- Nix `jdtls` runs on its own JDK 21, the default `java.configuration.runtimes` row at `${JAVA_HOME}` sets the project JRE and compliance
- Runtime `name` is an execution environment id jdtls requires, a name outside the ids raises an error notice
- Compliance caps at the newest level bundled ECJ supports, syntax past the cap errors in jdtls alone
- Runtime paths and `referencedLibraries` expand `${VAR}`, `java.home` and runtime `name` stay literal
- Files outside the workspace folder get syntax diagnostics alone
- Workspace data stays at the Nix launcher default under `~/Library/Caches/jdtls`, a `-data` folder inside the repository root breaks the project link
- Running sessions keep the server configuration they started with, `/reload-plugins` applies an `.lsp.json` edit
