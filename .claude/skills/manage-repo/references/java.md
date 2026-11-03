# [JAVA]

google-java-format owns formatting, javac owns compile checks, PMD owns lint, jdtls owns editor diagnostics.

## [01]-[FORMATTER]

- `google-java-format` takes files and no directory, `--aosp` sets its 4-space indent
- Formatter has no configuration file and no `.editorconfig` row, the mise row installs a native binary needing no JDK

## [02]-[CHECKS]

- `typecheck:javac`, PMD, and editor `java.project.referencedLibraries` read the `Ghidra/**/lib/*.jar` set a script loads
- `-Xlint:-path` drops warnings for `Class-Path` entries Ghidra's JAR manifests name and its release omits
- `pmd check` runs on the mise `java` row's JDK and takes files or directories
- Ruleset references each category whole and excludes rules by name, a threshold rule keeps its default or goes
- Host API checks that need resolved types are XPath rules in `.config/pmd.xml` under `[GHIDRA]`, `pmd-java:matchesSig` names the method
- PMD tags a `-SNAPSHOT` prerelease after each release, mise row at `latest` skips it

## [03]-[LANGUAGE_SERVER]

jdtls type-checks scripts a tool loads against that tool's jars on each edit:
- Mise `http:jdtls` row lists milestone versions, its `url` reads build-stamped archive name from `<version>/latest.txt` at install alone
- `workspaceFolder` names the scripts directory, a root workspace refreshes every repository file before jars attach
- `java.project.sourcePaths` names scripts directory as sole source root, an inferred root takes `rasm/` and fails each `rasm` import
- `jdtls` launcher runs on `$JAVA_HOME/bin/java`, VS Code runs its bundled server on its extension's JRE
- Default `java.configuration.runtimes` row at `${JAVA_HOME}` sets project JRE and compliance in both clients
- `jdtls` expands `${NAME}` in runtime and library paths from its own process environment, `mise exec` gives the agent server `mise.toml` values
- `jdtls` launcher defaults put workspace data under `~/Library/Caches/jdtls` keyed on folder basename and OSGi configuration under `~/.eclipse`
- `-data` and `--jvm-arg=-Dosgi.configuration.area=` move both under `$RASM_CACHE_HOME/jdtls`, and the tool's jars outside scripts folder keep resolving
- VS Code jdtls imports every Eclipse `.project` under root outside `java.import.exclusions`
- Maven and Gradle importers stay off, they find vendored builds alone (poms under `.cargo/rustc-source`)
