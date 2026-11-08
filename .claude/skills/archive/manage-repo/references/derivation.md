# [DERIVATION]

Sections list one system's mechanisms in axis order: members, policy, modes, values, live state where the system manages it, then derivation.

## [01]-[NX]

- Members come from a plugin's `createNodes` glob, `plugins[]` `include` and `exclude` narrow the files one plugin reads
- Edges and project data no file states come from plugin `createDependencies` and `createMetadata`
- Policy inherits through `nx.json` `extends`, `targetDefaults`, `namedInputs`, and `dependsOn` objects (`{ target, dependencies, params }`)
- Modes take target `configurations` with `defaultConfiguration`, `.env.<target>.<configuration>` and `.env.<configuration>` files
- Project patterns (`tag:<tag>`, `directory:<glob>`, globs, `!`) select members in `run-many`, `affected`, `show projects`, `release.groups`
- Values read `{options.<name>}` in `outputs`, `{args.<name> ?? env.<NAME>}` in `nx:run-commands`, and project `tags` and `metadata`
- Derivation covers values read from file contents and members without a marker file, both in a `createNodes` plugin

## [02]-[MISE]

- Members come from config files in each folder from filesystem root to working folder, `conf.d/*.toml` fragments in name order
- Tasks come from `mise-tasks/`, `.mise/tasks/`, or `.config/mise/tasks/` files, subfolders naming `<group>:<task>`, or `[task_config] includes`
- `monorepo_root = true` with `[monorepo] config_roots` single-level globs loads one config root per member
- Policy merges nearer files over farther ones per key in `[env]`, `[vars]`, `[settings]`, per tool in `[tools]`, per name in `[tasks]`
- `include` rows rank below the including file, task `extends` reads `[task_templates]`, `[task_config] cascade = true` passes task defaults
- Modes load `mise.<env>.toml` per `MISE_ENV` or `-E` value, `[env_sets]` in `miserc.toml` names a set of environments
- `auto_env = true` loads `mise.unix.toml`, `mise.<os>.toml`, and `mise.<os>-<arch>.toml` below explicit environments
- Tool rows take `os = ["<os>/<arch>"]`, `http:` rows a `platforms.<os>-<arch>` table, `mise lock -p` the locked platforms
- Values render Tera: `config_root`, `env`, `vars`, `tools` under `tools = true`, `exec()`, `read_toml()`, `os()`, `arch()`, `join_path`
- Task `usage` specs parse arguments into `{{ usage.<name> }}`, task `sources` and `outputs` skip a fresh task
- `mise ls --missing`, `mise outdated`, and `mise lock --dry-run --json` compare rows with installed and locked tools
- Derivation covers keys and nested includes, templates render values alone and `config_roots` takes no `**`

## [03]-[HK]

- Members come from step `glob`, `types` (extension, shebang, content), and `exclude`, a folder name excluding its contents, `match_any` selectors in place of `glob` and `types`
- `workspace_indicator = "<marker>"` runs one job per nearest marker, `subprojects` loads each listed folder's `hk.pkl` one level deep
- Policy amends hk's `Config.pkl`, `(Builtins.<step>) { … }` amends one builtin, top-level `steps` generate `check`, `fix`, and `pre-commit`, a hook step of the same name replaces one
- Root configuration alone sets `fail_fast`, `jobs`, `exclude`, and `profiles`, a subproject's own copies are ignored
- `new Group { … }` passes `dir`, `exclude`, and `workspace_indicator` to child steps setting none, lists replace
- Modes take step `profiles` (`!` negates) through `HK_PROFILE` or `--profile`, `condition` expressions, and `required` variables
- Values render `{{files}}`, `{{workspace}}`, and `{{workspace_files}}` per job
- Pkl `import*` and `read*` map a glob to modules or resources, `for` and `when` generators build one step per entry
- `hk check --plan --json` prints step and file selection, `--why <step>` its reasons, `hk config explain <key>` a setting's source layers
- Derivation covers more than one project file, and files `read()` pulls in stay out of hk's evaluation cache key

## [04]-[GITHUB_ACTIONS]

- Members come from `.github/workflows/` alone, `on.<event>.paths` and `paths-ignore` select runs, `hashFiles('<glob>')` hashes files
- Policy sits in reusable workflows, `on.workflow_call` declaring typed `inputs`, `outputs`, and `secrets`
- Callers set `jobs.<id>.uses: ./.github/workflows/<file>` with `with` and `secrets: inherit`, a caller `strategy` fans it out
- Composite actions (`runs.using: composite`) read `inputs`, `vars` resolve organization, repository, then environment
- Modes take `strategy.matrix` with `include` and `exclude`, `exclude` applying first, 256 jobs per matrix
- Dynamic matrices read `${{ fromJSON(needs.<job>.outputs.<name>) }}`, `environment` gates a job's secrets and vars
- `jobs.<id>.if` reads `github`, `needs`, `vars`, and `inputs`, never `matrix`
- Values read `runner.os`, `runner.arch`, `matrix`, and `needs.<id>.outputs` through `fromJSON`, `toJSON`, `format`, `join`, `case`
- Derivation covers member lists, a prior job printing matrix JSON from declarations, and `uses` taking no expression

## [05]-[MSBUILD]

- Members come from item `Include` globs with `Exclude`, the SDK's default `Compile` glob, and `Remove` with `MatchOnMetadata`
- `Update` sets metadata on existing items, item functions (`WithMetadataValue`, `Distinct`, `DirectoryName`) filter lists
- Policy imports the first `Directory.Build.props` and `Directory.Build.targets` found walking up, `GetPathOfFileAbove` chains the next
- `Directory.Packages.props` holds `PackageVersion` rows, `GlobalPackageReference` adds a package to every project
- `CentralPackageTransitivePinningEnabled` pins transitive packages, `VersionOverride` states one project's exception
- `ItemDefinitionGroup` sets default item metadata, `Directory.Build.rsp` sets command-line switches
- Modes take `Condition` on any element over `$(Configuration)`, `$(Platform)`, and `$(TargetFramework)`
- `TargetFrameworks` runs one inner build per framework, `GetTargetPlatformIdentifier` and `IsOSPlatform` test platform
- `.slnx` `<Configurations>` maps solution build types and platforms to project configurations
- Values compute through property functions (`[System.IO.Path]::Combine`, `MakeRelative`, `NormalizeDirectory`, `ValueOrDefault`)
- `MSBuildThisFileDirectory` and well-known metadata (`%(Filename)`, `%(RecursiveDir)`) locate files
- Batching on `%(<Item>.<Metadata>)` runs a task or target once per distinct value
- `-getProperty`, `-getItem`, and `-getTargetResult` print one project's evaluated values, a `.slnx` fails with MSB1063
- `dotnet package list --outdated --format json` compares packages with the feed, `RestoreLockedMode` fails a stale lock
- Derivation covers solution membership and values spanning projects, `.slnx` takes literal paths and evaluation reads one project

## [06]-[SWIFTPM]

- Members come from target `path` (`Sources/<name>` by default), `sources: nil` takes every file under it, `exclude` takes exact paths
- `Package.swift` is Swift: target arrays compute from lists and `Context` values, its sandbox reads files and denies writes and network
- Policy sits in one manifest with no include, targets reuse a settings array, `platforms` and `swiftLanguageModes` hold package defaults
- Modes take `.when(platforms:configuration:traits:)` on every setting, dependencies `.when(platforms:traits:)` with no configuration
- Configurations are `debug` and `release` alone, package `traits` select through `--traits` and `--enable-all-traits`
- Values come from `SwiftSetting` rows and `Context.packageDirectory`, `Context.environment`, and `Context.gitInformation`
- `swift package update --dry-run`, `show-traits`, and `show-dependencies --format json` report resolved state
- Derivation covers globs in `sources` and `exclude`, custom configurations, and configuration conditions on dependencies
- Manifest cache keys omit the file tree, a manifest listing folders reads stale until its text or environment changes

## [07]-[XCCONFIG]

- Policy takes `#include "<file>"` and `#include? "<file>"`, an include path expanding no `$(<VAR>)`
- `$(inherited)` appends the lower level, target files override project files, `-xcconfig` and `XCODE_XCCONFIG_FILE` override both
- Modes take `<SETTING>[sdk=<sdk>*][arch=<arch>][config=<config>]` and `[variant=<variant>]`, `SUPPORTED_PLATFORMS` per target
- Values substitute `$(<VAR>)` and nested names (`$(<NAME>_$(CONFIGURATION))`)
- Operators `:lower`, `:upper`, `:identifier`, `:rfc1034identifier`, `:dir`, `:file`, `:base`, `:suffix`, `:standardizepath` chain
- Replace operators `:default=`, `:dir=`, `:file=`, `:base=`, `:suffix=`, and `:relativeto=` rewrite one part or fill an empty value
- `xcodebuild -showBuildSettings -json` prints resolved settings per target
- Derivation covers file membership, arithmetic, and list functions, xcconfig takes no globs

## [08]-[UV]

- Members come from `[tool.uv.workspace]` `members` and `exclude` globs, `exclude` winning, uv has no `default-members`
- Policy reads `[tool.uv]` at workspace root alone, root `[tool.uv.sources]` apply to every member
- `<dependency> = { workspace = true }` names a member, `{ include-group = "<group>" }` composes groups, `default-groups` installs them
- Modes take PEP 508 markers (`sys_platform`, `platform_machine`, `python_version`, `extra`) and source `marker`, `extra`, `group` keys
- `environments` narrows universal resolution, `required-environments` demands wheels for each listed marker
- `conflicts` resolves exclusive extras or groups apart, `[tool.uv.dependency-groups] <group> = { requires-python }` narrows one group
- `.python-version`, `--python`, `--package`, `--all-packages`, `--group`, and `--python-platform` select a run
- Values come from `cache-keys` (`file`, `git`, `env`), `extra-build-variables`, and `[[tool.uv.dependency-metadata]]`
- `uv lock --check`, `uv sync --check`, `--dry-run`, `--locked`, and `--frozen` report lock and environment differences
- Derivation covers per-member `requires-python` and group-gated dependencies, `dependency_groups` markers act in lock files alone

## [09]-[PYTHON_CHECKERS]

- Members take the nearest `.ruff.toml`, `ruff.toml`, or `pyproject.toml` holding `[tool.ruff]`, with no parent merge
- Policy takes `extend = "<path>"`, `lint.extend-select` and `lint.extend-per-file-ignores` add where `lint.select` replaces
- Modes take `target-version`, else `requires-python` beside the found config, `per-file-target-version` maps globs to versions
- `ty` reads the nearest `ty.toml` or `[tool.ty]` with no `extend`, `[[tool.ty.overrides]]` scope rules by `include` and `exclude`
- `ty` `environment.python-version` falls back to `requires-python`, then to the environment `VIRTUAL_ENV` or `.venv` names
- Derivation covers subtree configs, each subtree states its own `extend`

## [10]-[PIXI]

- Members come from `path` dependencies, Pixi has no member glob
- Policy forms a `default` feature from top-level tables, each environment includes it unless `no-default-feature = true`
- `[dependency-groups]` and optional dependencies become features, `[workspace.dependencies]` with `workspace = true` covers conda
- Modes take `workspace.platforms` entries with virtual packages (`cuda`, `glibc`, `macos`)
- `[target.<unix|osx|linux|win|<platform>>]` tables override `dependencies`, `constraints`, `activation`, and `tasks`
- `[environments] <env> = { features, solve-group }` unions dependencies and intersects platforms, `-e` and `-p` select one
- Values come from `activation.env` and `activation.scripts`, task MiniJinja `{{ pixi.platform }}` and `{{ pixi.environment.name }}`
- Tasks take `args` with `default` and `choices`, `depends-on` with an `environment`, `inputs` and `outputs` caching a run
- `pixi lock --check`, `--locked`, `--frozen`, and `pixi info --json` report lock and environment differences
- Derivation covers member discovery and PyPI rows `[workspace.dependencies]` omits

## [11]-[CARGO]

- Members come from `[workspace] members` globs, `exclude`, and path dependencies under the root, `-p` and `--workspace` select them
- Policy inherits `[workspace.package]` keys through `<key>.workspace = true`, `[workspace.lints]` through `[lints] workspace = true`
- `[workspace.dependencies]` rows inherit through `workspace = true`, a member adding `features` or `optional`
- Profiles sit in root `Cargo.toml` with `inherits`, `.cargo/config.toml` merges each ancestor folder and `include` files
- Modes take `[target.'cfg(<predicate>)'.dependencies]` and `[features]` with `dep:` and `<dependency>?/<feature>`
- Config `[target.<triple>]` and `[target.'cfg(<predicate>)']` set `rustflags`, `runner`, and `linker`
- Values come from `build.rs` (`cargo::rustc-cfg`, `cargo::rustc-check-cfg`, `cargo::rustc-env`) reading `CARGO_CFG_*`
- `[env]` with `relative = true`, `CARGO_PKG_*`, `CARGO_MANIFEST_DIR`, and `[package.metadata]` rows `cargo metadata` prints
- `cargo metadata --format-version 1`, `--locked`, `cargo update --dry-run`, and `cargo tree -i` report resolution
- Derivation covers dependencies by feature and resolution by `rustc-cfg`, Cargo reads neither

## [12]-[PNPM]

- Members come from `packages` globs with `!` exclusions, the root package always included
- Policy sits in `catalog` and `catalogs.<name>`, referenced as `catalog:` and `catalog:<name>`, `workspace:` resolving members
- `include` hands named settings to local YAML files or config dependencies, one owner per setting, the root alone holding `include`
- `packageConfigs` sets per-project settings, resolution and layout settings applying per project under `sharedWorkspaceLockfile: false` alone
- Modes take `--filter` selectors (`<name>...`, `...<name>`, `./<glob>`, `[<ref>]`, `!`) and `supportedArchitectures`
- Values expand `${<NAME>}` and `${<NAME>:-<default>}` in `pnpm-workspace.yaml` outside registry URLs, `pnpm_config_<name>` variables override it
- `pnpm outdated -r --format json` compares members with the registry, `--frozen-lockfile` fails an install whose lock its manifests contradict
- Derivation covers computed per-member settings in `.pnpmfile.mjs` hooks (`readPackage`, `updateConfig`, `afterAllResolved`)

## [13]-[TSCONFIG]

- Members come from `include`, `exclude`, and `files`, `exclude` filtering `include` alone
- Policy takes `extends` as a string or array, later entries win, every field except `references` inherits, and a list replaces the inherited one
- `types` defaults to `[]`, a project lists each typings package declaring a global or ambient module, `"*"` restoring every `@types` package
- Modes take `customConditions` under `node16`, `nodenext`, or `bundler` resolution, `moduleSuffixes` for platform files
- `imports` `#/` specifiers resolve under `nodenext` and `bundler` resolution, `node16` refusing them
- `${configDir}` at a path's start resolves to the leaf config's folder
- `tsc --showConfig`, `--explainFiles`, and `tsc -b --dry` print the resolved program
- Derivation covers `references`, which take literal paths and never inherit, and every condition or token past `${configDir}`

## [14]-[VITE]

- Members come from Vitest `test.projects` globs, folders, config files, inline objects, and functions, `--project` patterns select them
- Inline projects inherit their declaring config unless `extends: false`, project files inherit nothing and merge shared values through `mergeConfig`
- Root configs decide global options (`reporters`, `coverage`) alone, `defineProject` rejecting them in a project file
- Modes take `defineConfig(({ command, mode, isSsrBuild, isPreview }) => …)` and named `environments`, `vite build --app` building each
- `loadEnv(mode, envDir, prefixes)` reads `.env`, `.env.local`, `.env.<mode>`, and `.env.<mode>.local`, `envPrefix` filters keys
- Values come from plugin hooks `config`, `configEnvironment`, `applyToEnvironment`, and `perEnvironmentPlugin`
- `vitest list --json` prints the tests each project collects
- Derivation covers workspace members, Vite reads one root

## [15]-[PULUMI]

- Members are stacks: `LocalWorkspace.createOrSelectStack({ stackName, projectName, program })`, `workspace.listStacks()`
- Policy sits in `Pulumi.yaml` project `config` rows with `type` and `default`, stack keys override them
- `stack.addEnvironments` imports ESC environments in order, `LocalWorkspaceOptions` seeds project and stack settings
- Modes take the stack name, the program reads `pulumi.getStack()` and `Pulumi.<stack>.yaml`
- Values come from `stack.setAllConfig`, program `Config.requireObject<T>`, and `StackReference.requireOutput`
- `stack.preview({ diff })` returns `changeSummary`, `previewRefresh` compares state with live resources, `stack.refresh` writes them
- Derivation covers structured config, `ConfigValue.value` holds a string set by `path` or JSON

## [16]-[BIOME]

- Members come from nested `biome.json` files with `"root": false`, `files.includes` globs (`!` excludes, `!!` skips scanning)
- Policy takes `extends` (`"//"` or a path list), later files win and lists combine, `overrides` scope settings by `includes`
- Modes take `linter.domains`, each turning on when its dependencies appear, `--changed` and `--since` select files
- Derivation covers tokens, environment selection, and computed values, Biome holds static globs alone

## [17]-[EDITORCONFIG]

- Members come from section globs (`*`, `**`, `{<a>,<b>}`, `{<n>..<m>}`), a glob without `/` matching at any depth
- Policy reads files upward until `root = true`, nearer files and later sections win, `unset` removes a value
- Derivation covers includes, variables, and conditions

## [18]-[AST_GREP]

- Members come from `ruleDirs` walked recursively under `.gitignore`, `utilDirs` holding utilities rules reach through `matches`
- Modes take `languageGlobs`, `customLanguages`, and `languageInjections`, rule `files` and `ignores` scope paths
- Values come from rule `utils`, `transform`, and `rewriters`
- Derivation covers inheritance, neither schema holds `extends` or an include, and each rule takes one `language`
