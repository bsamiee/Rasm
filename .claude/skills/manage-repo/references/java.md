# [JAVA]

google-java-format owns formatting, PMD owns lint and clone detection, jdtls owns editor diagnostics.

## [01]-[FORMATTER]

- `google-java-format` takes files and no directory, `--dry-run --set-exit-if-changed` lints and `--replace` writes, `--aosp` sets 4-space indent
- Formatter has no configuration file and no `.editorconfig` row, the mise row is a native binary that runs without a JDK

## [02]-[CHECKS]

- `pmd check` runs on the mise `java` row's JDK
- `pmd check` takes files or directories, `--rulesets <file>` names the ruleset, `--use-version java-<n>` Java level, `--cache <file>` cache location
- Ruleset references each category whole and excludes rules by name, a threshold rule keeps its default or goes
- PMD release tags take a `pmd_releases/` prefix, and a `-SNAPSHOT` tag follows each release

## [03]-[CLONES]

`pmd cpd` reports token sequences repeated inside the repository, the derivation that belongs to one owner:
- `--language` takes one language per `pmd cpd` run, `pmd cpd` exits 4 on a clone
- `--minimum-tokens` sits above the forms a language states twice by rule, a protocol member's signature restated by its implementation

## [04]-[LANGUAGE_SERVER]

jdtls type-checks scripts a tool loads against that tool's jars on each edit, a check no `rasm:lint` command runs:
- `workspaceFolder` names the scripts directory, a root workspace refreshes every repository file before the jars attach
- Nix `jdtls` runs on its own JDK, the default `java.configuration.runtimes` row at `${JAVA_HOME}` sets the project JRE and compliance
- Nix launcher defaults put workspace data under `~/Library/Caches/jdtls` keyed on the folder basename and OSGi configuration under `~/.eclipse`
- `-data` and `--jvm-arg=-Dosgi.configuration.area=` move both under `.cache/jdtls`, and the tool's jars outside the scripts folder keep resolving
