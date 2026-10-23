# [JAVA]

google-java-format owns formatting, javac owns compile checks, PMD owns lint and clone detection, jdtls owns editor diagnostics.

## [01]-[FORMATTER]

- `google-java-format` takes files and no directory, `--aosp` sets its 4-space indent
- Formatter has no configuration file and no `.editorconfig` row, the mise row is a native binary that runs without a JDK

## [02]-[CHECKS]

- `typecheck:javac`, PMD, and the editor's `java.project.referencedLibraries` read the `Ghidra/**/lib/*.jar` set a script loads
- `-Xlint:-path` drops warnings for `Class-Path` entries Ghidra's JAR manifests name and its release omits
- `pmd check` runs on the mise `java` row's JDK and takes files or directories
- Ruleset references each category whole and excludes rules by name, a threshold rule keeps its default or goes
- Host API checks that need resolved types are XPath rules in `.config/pmd.xml` under `[GHIDRA]`, `pmd-java:matchesSig` names the method
- PMD tags a `-SNAPSHOT` development build after each release

## [03]-[CLONES]

`pmd cpd` reports token sequences repeated inside the repository, the derivation that belongs to one owner:
- `--language` takes one language per `pmd cpd` run, `pmd cpd` exits 4 on a clone
- `--minimum-tokens` sits above the forms a language states twice by rule, a protocol member's signature restated by its implementation

## [04]-[LANGUAGE_SERVER]

jdtls type-checks scripts a tool loads against that tool's jars on each edit:
- `workspaceFolder` names the scripts directory, a root workspace refreshes every repository file before the jars attach
- Nix `jdtls` runs on its own JDK, the default `java.configuration.runtimes` row at `${JAVA_HOME}` sets the project JRE and compliance
- `jdtls` expands `${NAME}` in runtime and library paths from its own process environment, `mise exec` gives the agent server the `mise.toml` values
- Nix launcher defaults put workspace data under `~/Library/Caches/jdtls` keyed on the folder basename and OSGi configuration under `~/.eclipse`
- `-data` and `--jvm-arg=-Dosgi.configuration.area=` move both under `.cache/jdtls`, and the tool's jars outside the scripts folder keep resolving
- VS Code jdtls imports every Eclipse `.project` under the root, `java.import.exclusions` skips the agent server's under `.cache`
- Maven and Gradle importers stay off, they find vendored builds alone (poms under `.cargo/rustc-source`)
