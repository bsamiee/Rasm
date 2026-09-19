# [JAVA]

google-java-format owns formatting, PMD owns lint, the workspace plugin infers a project from its Gradle settings file.

## [01]-[PROJECTS]

- `settings.gradle.kts` is the project file, `rootProject.name` names the project, a `build.gradle.kts` holds no name
- `@nx/gradle` requires a root Gradle build file, a companion Gradle plugin, and the wrapper, target bodies sit in `nx.json` by tag
- JDK and Gradle tool rows join with the first project when a `build` or `test` body runs them

## [02]-[FORMATTER]

- `google-java-format` takes files and no directory, `--dry-run --set-exit-if-changed` lints and `--replace` writes, `--aosp` is the 4 space indent
- Formatter has no configuration file and no `.editorconfig` row, the mise row is a native binary that runs without a JDK

## [03]-[CHECKS]

- `pmd check` takes files or directories, `--rulesets <file>` names the ruleset, `--use-version java-<n>` the Java level, `--cache <file>` the cache
- Ruleset references each category whole and excludes rules by name, a threshold rule keeps its default or goes
- PMD release tags take the `pmd_releases/` prefix and a `-SNAPSHOT` tag follows each release, a mise `github:` row pins the release
- Java is a built-in ast-grep language, `sgconfig.yml` takes no row for it
- CodeQL `java-kotlin` supports `build-mode: none`, a matrix language with no source fails the scan, its row joins with the first project
