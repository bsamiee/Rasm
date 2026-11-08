# [JAVA]

google-java-format owns formatting, javac owns compile checks, PMD owns lint, jdtls owns editor diagnostics.

## [01]-[FORMATTER]

- `google-java-format` takes files and no directory, `--aosp` sets its 4-space indent
- Formatter has no configuration file and no `.editorconfig` row, the mise row installs a native binary needing no JDK

## [02]-[CHECKS]

- `typecheck:javac`, PMD, and the Eclipse project read the `Ghidra/**/lib/*.jar` set a script loads
- `-Xlint:-path` drops warnings for `Class-Path` entries Ghidra's JAR manifests name and its release omits
- `pmd check` runs on the mise `java` row's JDK and takes files or directories
- Ruleset references each category whole and excludes rules by name, a threshold rule keeps its default or goes
- Host API checks that need resolved types are XPath rules in `.config/pmd.xml` under `[GHIDRA]`, `pmd-java:matchesSig` names the method

## [03]-[LANGUAGE_SERVER]

- Argus and VS Code set the script source root and load Ghidra JARs through `java.project.referencedLibraries`
- Argus starts JDT LS in the scripts directory and stores its workspace and OSGi data under `.cache/jdtls`
- VS Code reads the project from the repository root and excludes the `.codex/skills` link
- Both clients set the project runtime through `java.configuration.runtimes` at `${JAVA_HOME}`. VS Code runs its server on the extension's JRE
- Open Java definitions in VS Code or pass the `jdt://contents` URI from Argus `definition` to `java_source` with the requested line range
