# [JAVA]

google-java-format owns formatting, PMD owns lint, jdtls owns editor diagnostics.

## [01]-[PROJECTS]

- `settings.gradle.kts` is the project file, `rootProject.name` names its project, a `build.gradle.kts` holds no name
- First project joins through a workspace plugin entry for `settings.gradle.kts`, target bodies sit in `nx.json` by tag
- `@nx/gradle` requires a root Gradle build file, a companion Gradle plugin, and the wrapper
- Gradle tool row joins with the first project, `java` row runs PMD

## [02]-[FORMATTER]

- `google-java-format` takes files and no directory, `--dry-run --set-exit-if-changed` lints and `--replace` writes, `--aosp` is the 4 space indent
- Formatter has no configuration file and no `.editorconfig` row, the mise row is a native binary that runs without a JDK

## [03]-[CHECKS]

- `pmd check` takes files or directories, `--rulesets <file>` names the ruleset, `--use-version java-<n>` Java level, `--cache <file>` cache location
- Ruleset references each category whole and excludes rules by name, a threshold rule keeps its default or goes
- PMD release tags take a `pmd_releases/` prefix and a `-SNAPSHOT` tag follows each release, a mise `github:` row pins the release
- Java is a built-in ast-grep language, `sgconfig.yml` takes no row for it
- CodeQL `java-kotlin` supports `build-mode: none`, a matrix language with no source fails its scan, its row joins with the first project

## [04]-[LANGUAGE_SERVER]

jdtls type-checks the Ghidra script bundle against the Ghidra jars on each edit, a check no `rasm:lint` command runs:
- `.lsp.json` starts jdtls through `mise exec`, which supplies the `JAVA_HOME` and `GHIDRA_INSTALL_DIR` the settings expand
- `workspaceFolder` names the scripts directory, a repository root workspace refreshes every file under the root before the Ghidra jars attach
- Settings sit in the `settings` field, Claude Code sends it by `workspace/didChangeConfiguration` after `initialize`
- Nix `jdtls` runs on its own JDK 21, the default `java.configuration.runtimes` row at `${JAVA_HOME}` sets the project JRE and compliance
- Runtime `name` is an execution environment id jdtls requires, a name outside the ids raises an error notice
- Compliance caps at the newest level the bundled ECJ supports, syntax past that cap errors in jdtls alone while PMD and Ghidra accept it
- Runtime paths and `referencedLibraries` expand `${VAR}`, `java.home` and runtime `name` stay literal
- Files outside the workspace folder get syntax diagnostics alone, `java.diagnostic.filter` drops `**/.cache/**` rule fixtures by absolute path
- Workspace data stays at the Nix launcher default under `~/Library/Caches/jdtls`, a `-data` folder inside the repository root breaks the project link
- Running sessions keep the server configuration they started with, `/reload-plugins` applies an `.lsp.json` edit
