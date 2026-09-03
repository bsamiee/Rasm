<!-- Source for monorepo-build-infrastructure [01]-[ENTRY_POINTS], [03]-[CONFIGURATION], and references/nx-targets.md, nothing integrated yet -->
# [NX_CAPABILITIES]

The newest stable Nx release on 2026-09-03 is 23.2.0, published 2026-09-02 (<https://github.com/nrwl/nx/releases/tag/23.2.0>). Rasm pins 23.1.3 in `pnpm-workspace.yaml` (`nx`, `@nx/devkit`, `@nx/dotnet`, `@nx/js`, `@nx/vite`, `@nx/vitest`), `node_modules/nx/package.json` reports 23.1.3, and Nx moves to 23.2.0 across the six catalog entries by the plan's decision.

Installed sources are the 23.1.3 packages under `node_modules`, repository sources are at tag 23.2.0, and documentation pages are cited per claim.

Nx Cloud and every paid feature are out of scope: `nx.json` sets `"neverConnectToCloud": true`, and the plan fixes local cache only.

## [00]-[WORKSPACE_DECLARATIONS]

`nx.json` sets `defaultBase: main`, `cacheDirectory: .cache/nx/cache`, `neverConnectToCloud: true`, three named inputs, three `targetDefaults` entries, `pluginsConfig`, four `plugins` entries, and one disabled sync generator. `eng/project.json` declares the project `eng` at `eng/` with one target, `provision`, `command: "uv run python -m eng.scripts.provision"`, `cache: false`, `parallelism: false`. `tools/nx/native-packaging.ts` is a local inference plugin exporting `createNodes` and `createDependencies` and globbing `eng/native/*/*.csproj`, and `tools/nx/tsconfig.json` sets `emitDeclarationOnly` with output under `.cache/typescript/`. `tsconfig.json` is the root solution-style config with `references` to `./tests/typescript/support` and `./tools/nx`. `package.json` is the root package `rasm-workspace`, private, with every dependency `catalog:`, and the `pnpm-workspace.yaml` catalog holds `nx: 23.1.3`, `vite: 8.2.2`, `vitest: 4.1.11`, `typescript: 7.0.2`. `README.md` states that every developer and CI action is an Nx target and that steps with control flow are Python under `eng/scripts/` invoked through `uv run`.

Plugins registered in `nx.json`, in order:
1. `@nx/vite/plugin` with `{ compiler: "tsc", typecheckTargetName: "typecheck" }`
2. `@nx/vitest` with `{ testTargetName: "test", discoverTestFiles: "glob" }`
3. `@nx/dotnet` with `exclude: ["eng/native/**"]` and options `{ clean: false, pack: false, publish: false, restore: false, test: true }`
4. `./tools/nx/native-packaging.ts` with no options

Tools with no plugin entry and no inferred target: Biome, ruff, ty, mypy, Stryker (both configs), `dotnet format`, uv, and the `eng/scripts/` automation other than `provision` and `stage`.

## [01]-[PLUGIN_INFERENCE]

### [01.1]-[PLUGIN_ENTRIES]

Sources: <https://nx.dev/docs/reference/nx-json>, <https://nx.dev/docs/concepts/inferred-tasks>, and `node_modules/nx/schemas/nx-schema.json` (`definitions.plugins`).

Entries are a string (the module with default options) or an object with the fields:

| [FIELD] | [TYPE] | [MEANING] |
| :-- | :-- | :-- |
| `plugin` | string | Package name, package subpath (`@nx/vite/plugin`), or workspace-relative file (`./tools/nx/native-packaging.ts`) |
| `options` | object | Passed to `createNodes` and `createDependencies`, the plugin owns the shape |
| `include` | string[] | Globs, matching config files alone reach the plugin |
| `exclude` | string[] | Globs, matching config files are withheld |

`include` and `exclude` match the configuration file path the plugin globbed for, not the project root: "the `@nx/jest/plugin` plugin will only infer tasks for projects where the `jest.config.ts` file path matches the `packages/**/*` glob" (nx-json reference). Negation patterns are supported and order-sensitive: patterns are processed first to last, a `!` pattern removes files, the last matching pattern decides, and a leading negation starts with everything matched.

"Register the same plugin more than once with different scopes when groups of projects need different target names or options" (inferred-tasks, "Scope a plugin to projects").

### [01.2]-[ORDERING_AND_MERGE]

Source: <https://nx.dev/docs/concepts/inferred-tasks>.
- "Nx processes plugins in the order listed in `nx.json`." Compatible contributions merge
- "For conflicting target definitions or fields, configuration from a later plugin takes precedence."
- Nx's own `project.json` and `package.json` readers run after every plugin, and a `project.json` overrides what a plugin inferred
- Resolution order, least to most specific: inferred configuration, matching `targetDefaults`, project configuration

`pluginsConfig` holds workspace-wide options keyed by plugin package name. Rasm uses it for `{"@nx/js": {"analyzeSourceFiles": false}}`.

### [01.3]-[NX_VITE]

Source: `node_modules/@nx/vite/dist/src/plugins/plugin.d.ts` and `plugin.js`, 23.1.3. Entry point `@nx/vite/plugin`. Glob `**/vite.config.{js,ts,mjs,mts,cjs,cts}` (`plugin.js:19`).

Options (`VitePluginOptions`): `buildTargetName`, `devTargetName`, `serveTargetName` (deprecated, "will be removed in Nx 22", still declared), `previewTargetName`, `serveStaticTargetName`, `typecheckTargetName`, `compiler` (`tsc` | `tsgo` | `vue-tsc`, unset defaults to `vue-tsc` for Vue projects and `tsc` otherwise, `tsgo` uses `@typescript/native-preview`), `watchDepsTargetName`, `buildDepsTargetName`. `createDependencies` is exported as a documented no-op.

Rasm passes `{ compiler: "tsc", typecheckTargetName: "typecheck" }`, the plugin contributes a `typecheck` target, and `targetDefaults.typecheck` overrides it with `command: "tsc --build --pretty false"`. With `typescript: 7.0.2` in the catalog, `tsc` is already the Go compiler and `compiler: "tsgo"` is nothing to adopt.

### [01.4]-[NX_VITEST]

Source: `node_modules/@nx/vitest/dist/src/plugins/plugin.d.ts` and `plugin.js`, 23.1.3. Entry point `@nx/vitest`. Glob `**/{vite,vitest}.config.{js,ts,mjs,mts,cjs,cts}` (`vitestConfigGlob`, `plugin.js:19`).

Options (`VitestPluginOptions`): `testTargetName`, `ciTargetName` (atomized per-file targets), `ciGroupName`, `testMode` (`watch` | `run`), `discoverTestFiles` (`glob` | `vitest`, default `glob`).

`discoverTestFiles: "glob"` enumerates specs from the Nx workspace file index without booting Vitest, and never sees files ignored by `.gitignore` or `.nxignore`. The type comment lists configurations that still boot Vitest under `glob`: `test.projects` or `test.workspace`, a plugin with a `configureVitest` hook, `test.changed`/`test.related`, browser `instances` with their own include/exclude, and include patterns a workspace glob reads differently.

In Rasm the root `vitest.config.ts` defines `test.projects` (line 136), the `glob` setting does not take effect for that file, and the plugin boots Vitest. The plan lists "Vitest discovery declared twice" and "`vitest.config.ts` projects forcing Vitest boot" as defects, and the correction is one declaration, the `@nx/vitest` glob, with the root `projects` list removed.

Both `@nx/vite` and `@nx/vitest` refuse to create a project unless the config file's directory contains a `package.json` or `project.json` (`@nx/vite/dist/src/plugins/plugin.js:423-427`, `@nx/vitest/dist/src/plugins/plugin.js:409-413`). The workspace root has `package.json` beside `vite.config.ts` and `vitest.config.ts`, and both plugins create a root project. The settled design attaches lint, format, and typecheck to language-tagged projects through `targetDefaults`, a root project is unwanted, and both plugin entries exclude the root config files.

### [01.5]-[NX_DOTNET_SOURCES]

Installed: `node_modules/@nx/dotnet/dist/plugins/create-nodes.js`, `plugins/create-dependencies.js`, `analyzer/analyzer-client.js`, and `lib/MsbuildAnalyzer.dll` at 23.1.3. Repository source at 23.2.0: `packages/dotnet/src/plugins/create-nodes.ts`, `src/analyzer/analyzer-client.ts`, `analyzer/Analyzer.cs`, `analyzer/Utilities/ProjectUtilities.cs`, and `analyzer/Utilities/TargetBuilder*.cs`. The 23.2.0 changelog adds three dotnet changes: "infer OpenApiDocumentsDirectory as a build output" (#36788), "derive output paths from MSBuild instead of hardcoded assumptions" (#36804), and "make msbuild-analyzer run cancellable" (#36813).

### [01.6]-[NX_DOTNET_OPTIONS]

`DotNetPluginOptions` (`create-nodes.ts:53-93`) has eight keys, `build`, `test`, `clean`, `restore`, `publish`, `pack`, `watch`, `run`, each `TargetConfigurationWithName | false`, where `TargetConfigurationWithName = Partial<TargetConfiguration> & { targetName?: string }`. `false` deletes the target, `{ targetName }` renames it, other fields merge through `mergeTargetConfigurations` with the user's value winning (`create-nodes.js:41-52`), and an absent key leaves the target unchanged. There is no `format` option and no `dotnet format` target.

In Rasm `nx.json` passes `"test": true`, which is not a member of the type. It behaves as "leave the test target alone" because `const { targetName, ...rest } = true` yields `{}`, but it is invalid, and the correction is `{}` or omission.

### [01.7]-[NX_DOTNET_GLOB]

The glob (`create-nodes.js:15`): `**/{*.{csproj,fsproj,vbproj},Directory.Build.{props,targets,rsp},Directory.Solution.{props,targets},Directory.Packages.props}`

`.sln` and `.slnx` are not in the glob. The analyzer constructs `new ProjectGraph(absoluteProjectFiles)` from the project files (`Analyzer.cs:74`). `Workspace.slnx` neither defines the project set nor invalidates the graph, and `nx show projects` and `dotnet restore Workspace.slnx` can disagree with nothing noticing.

The `Directory.*` files are globbed so that `createNodes` re-runs when they change and the analyzer can declare per-project ancestor inputs, and they contribute no project of their own (`create-nodes.js:102-107`).

### [01.8]-[NX_DOTNET_DIRECTORY_INPUTS]

`ProjectUtilities.GetDirectoryBuildInputs` walks from each project's directory to the workspace root and records the closest ancestor occurrence of each of the six filenames (`ProjectUtilities.cs:70-142`, `DirectoryBuildFileNames`), each as `{workspaceRoot}/<relative path>`. Those inputs are added to the targets that already declare an `Inputs` array: `build`, `build:release`, `test`, `pack`, `publish`.

In Rasm `eng/native/Directory.Build.props` and `.targets` exist, the plugin entry excludes `eng/native/**`, `@nx/dotnet` infers nothing under `eng/native/`, and `tools/nx/native-packaging.ts` covers that directory.

### [01.9]-[NX_DOTNET_TARGETS]

`isTest` is true when `IsTestProject` is `true` or a `PackageReference` names `Microsoft.NET.Test.Sdk` or starts with `Microsoft.Testing` (`Analyzer.cs:145`, `IsTestProject`), and `isExe` when `OutputType` is `Exe`.

| [TARGET] | [WHEN] | [COMMAND] |
| :-- | :-- | :-- |
| `build` | always | `dotnet build --no-restore --no-dependencies` |
| `build:release` | always | same, `--configuration Release` |
| `test` | `isTest` | `dotnet test --no-build --no-restore` (`TargetBuilder.Test.cs:28`) |
| `restore` | always | `dotnet restore --no-dependencies` |
| `clean` | always | `dotnet clean` |
| `watch` | always | `dotnet watch` |
| `publish` | `isExe` | `dotnet publish --no-build --no-dependencies --no-restore --configuration Release` |
| `run` | `isExe` | `dotnet run --no-build` |
| `pack` | neither `isExe` nor `isTest` | `dotnet pack --no-dependencies --no-build --configuration Release` |

| [TARGET] | [CACHE] | [DEPENDS_ON] | [OUTPUTS] |
| :-- | :-- | :-- | :-- |
| `build` | `true` | `^build` | `OutputPath`, intermediate (obj), `OpenApiDocumentsDirectory` |
| `build:release` | `true` | `^build:release` | same |
| `test` | `true` | `build` | test results directory when derivable |
| `restore` | not set | `^restore` | none |
| `clean` | `false` | none | none |
| `watch` | `false`, `continuous: true` | `restore` | none |
| `publish` | `true` | `build:release` | `PublishDir`, intermediate |
| `run` | `false` | `build` | none |
| `pack` | `true` | `build:release` | `<PackageOutputPath>/*.nupkg`, intermediate |

Every target sets `options.cwd` to `{projectRoot}`, and `build`, `pack`, `publish`, and `run` declare `debug` and `release` configurations.

The plugin runs restore, pack, and publish, and cannot run `dotnet format`. `restore` is per project, `--no-dependencies`, and not cacheable, and it is no substitute for `dotnet restore Workspace.slnx`. The plan's rule that every developer and CI action is an Nx target makes the solution-level restore a `restore` target on `eng` (`dotnet restore Workspace.slnx`) that `build` depends on.

Inputs on the cacheable targets (`TargetBuilder.Build.cs:62-64`, `TargetBuilder.Test.cs:44-46`): `build` and `build:release` open with the production input, `test`, `pack`, and `publish` open with `"default"`, and all add `"^<production>"`, `"{workspaceRoot}/.editorconfig"`, `{"workingDirectory": "absolute"}`, `{"dependentTasksOutputFiles": "**/*"}`, and the ancestor `Directory.*` files. `<production>` is the named input `production` when `nx.json` defines it and `default` otherwise (`TargetBuilder.cs:62-64`, `GetProductionInput`). Rasm defines `production`.

### [01.10]-[NX_DOTNET_PROJECT_NAMES]

`ProjectUtilities.GetProjectName` reads the MSBuild property `Nx`, parses it as XML, and returns a `<Name>` element if present, otherwise `MSBuildProjectName` (`ProjectUtilities.cs:30-58`):

```xml
<PropertyGroup>
  <Nx><Name>my-nx-name</Name></Nx>
</PropertyGroup>
```

### [01.11]-[NX_DOTNET_DEPENDENCIES]

`createDependencies` reads the analyzer's cached `referencesByRoot` and emits one `DependencyType.static` edge per `ProjectReference` (`create-dependencies.js:16-31`). `PackageReference` items are collected and not turned into edges, the code is commented out in `TargetBuilder.Test.cs:22` ("We should add this back in after external deps support is fleshed out"), and `tools/nx/native-packaging.ts` builds `PackageReference` edges itself for that reason.

### [01.12]-[NX_DOTNET_ANALYZER]

`analyzer-client.ts` at 23.2.0 (the installed `analyzer-client.js` agrees):
- Spawns `dotnet <plugin>/lib/MsbuildAnalyzer.dll <workspaceRoot>` (`:121, :223-231`)
- The options JSON and the file list go over stdin to avoid `ARG_MAX`
- Requires a `dotnet` on `PATH`, which Nx does not install
- Timeout `DEFAULT_ANALYSIS_TIMEOUT_SECONDS = isCI() ? 600 : 120` (`:20`), overridden with `NX_DOTNET_PROJECT_GRAPH_TIMEOUT` (seconds)
- `NX_DOTNET_DISABLE=true` turns the plugin off (`:242-243`), and `NX_PERF_LOGGING=true` emits timing to stderr (`:265`)
- Results are cached in-process and on disk in the workspace data directory as `dotnet-<optionsHash>.hash` (`analyzer-client.js:12, :38, :131-132`)
- The disk cache uses `PluginCache` from `@nx/devkit/internal`, keyed by `hashWithWorkspaceContext` over every globbed file
- Multi-targeting projects produce more than one MSBuild node, the analyzer prefers an inner build with `TargetFramework` set (`Analyzer.cs:118-122`)

### [01.13]-[REMAINING_TOOLS]

The packages published from `nrwl/nx` at tag 23.2.0 (`repos/nrwl/nx/contents/packages?ref=23.2.0`) number 41 and include `devkit`, `dotnet`, `js`, `vite`, `vitest`, `eslint`, `oxlint`, and `workspace`. `@nx/oxlint` was added in 23.2.0 ("linter: add the @nx/oxlint plugin", #36491), and it lints with oxlint, not Biome.

| [TOOL] | [OFFICIAL] | [COMMUNITY] | [VERDICT] |
| :-- | :-- | :-- | :-- |
| Biome | none | `@berenddeboer/nx-biome`, listed on <https://nx.dev/docs/plugin-registry> | Write the target |
| ruff | none | `@nxlv/python` offers ruff as a generator `--linter` choice | Write the target |
| ty | none | none found | Write the target |
| mypy | none | none found | Write the target |
| uv | none | `@mgwilt/nx-uv` (registry), `@nxlv/python` through `options.packageManager: "uv"` | Write the target |
| Stryker (.NET and TS) | none | none found | Write the target |
| `dotnet format` | none | none found | Write the target |

Each reaches Nx through `nx:run-commands` in `targetDefaults`.

## [02]-[LOCAL_PLUGINS]

### [02.1]-[PLUGIN_API]

Source: `node_modules/nx/dist/src/project-graph/plugins/public-api.d.ts`, 23.1.3, and <https://nx.dev/docs/extending-nx/project-graph-plugins>.

A plugin module exports any subset of `name`, `createNodes`, `createNodesV2` ("@deprecated Prefer `createNodes` for new plugins", `public-api.d.ts:97-99`), `createDependencies`, `createMetadata`, `preTasksExecution`, and `postTasksExecution`.

`createNodesV2` is a deprecated alias in Nx 23 and `createNodes` holds the batched signature. <https://nx.dev/docs/extending-nx/createnodes-compatibility> states that 22.x calls both with the v2 signature and 23.x "Prefers `createNodes`" with `createNodesV2` as a "deprecated alias". `CreateNodesContextV2` and `CreateNodesResultV2` are marked "This will be removed in Nx 24" (`:11, :16`). `tools/nx/native-packaging.ts` exports `createNodes` and `createDependencies` (`:337`) and does not export `createNodesV2`, and nothing changes.

The signatures: `CreateNodes<T> = readonly [projectFilePattern, createNodesFunction]`, the function receives `(projectConfigurationFiles, options, context)` and returns `Array<[configFileSource, CreateNodesResult]>` where `CreateNodesResult.projects` is keyed by project root, and `CreateDependencies<T>` receives `(options, context)` and returns `RawProjectGraphDependency[]`.

`CreateDependenciesContext` holds `externalNodes`, `projects` (keyed by name), `nxJsonConfiguration`, `fileMap`, `filesToProcess` (changed since the last invocation), and `workspaceRoot`. The documentation names `filesToProcess` as the field to read so that Nx reanalyzes changed files alone, and `tools/nx/native-packaging.ts:290-291` reads `context.filesToProcess.projectFileMap`.

`validateDependency(dependency, context)` from `@nx/devkit` throws on an invalid edge, and the plugin calls it (`:303`). `createNodesFromFiles` fans out over the file list, collects values and errors, and throws one `AggregateCreateNodesError` holding the successful results, and the plugin goes through it (`:324`).

### [02.2]-[PLUGIN_TRANSPILATION]

Source: `node_modules/nx/dist/src/project-graph/plugins/transpiler.js`, 23.1.3.
1. When the runtime supports native TypeScript stripping and the user has not opted out, Node loads the `.ts` file directly
2. When native stripping is unavailable or the plugin throws `ERR_UNSUPPORTED_TYPESCRIPT_SYNTAX` (`:52`), Nx registers `swc-node` or `ts-node`
3. The fallback adds `tsconfig-paths` (`:16, :50`)
4. The fallback reads compiler options from `tsconfig.base.json` at the workspace root (`:69`) and forces `experimentalDecorators` on (`:81`)

In Rasm `tools/nx/native-packaging.ts:24` uses `declare namespace NativePackaging` for types alone, which is erasable. It imports `@effect/platform`, `@effect/platform-node`, `effect`, and `fast-xml-parser` (`:5-20`), all resolved from `node_modules` at graph-creation time, and `pnpm install` precedes every Nx command. `pnpm-workspace.yaml` is already in `sharedGlobals`.

### [02.3]-[PLUGIN_OPTIONS]

Options come from the `options` field of the `nx.json` entry, an unqualified string registration passes `undefined`, and official plugins normalize with `options ?? {}` for that reason (`create-nodes.js:74`). `tools/nx/native-packaging.ts` is registered without options and keeps `_PROJECT_FILE_GLOB`, `_ARTIFACTS_ROOT`, `_TAG`, and `_LOCAL_SOURCE_KEY` as constants (`:69-73`). Constants mean the plugin describes one library layout, options let it be registered twice with different scopes, and one layout is what `eng/native/` has.

### [02.4]-[PLUGIN_RESULT_CACHES]

Two caches exist beyond the per-file re-run:
- `PluginCache` from `@nx/devkit/internal`, used by `@nx/dotnet`, is an on-disk cache under the workspace data directory
- Its files are `<name>.hash`, keyed by `hashWithWorkspaceContext`
- The `internal` subpath is no stable public API, and 23.2.0 records "move plugin internal imports to devkit/internal" (#36430)
- A module-level variable is valid for one plugin worker
- `@nx/dotnet` keeps `let cache` so that `createDependencies` reuses what `createNodes` computed
- `tools/nx/native-packaging.ts:315` keeps a `ManagedRuntime` the same way ("Nx loads the plugin once per worker")

`tools/nx/native-packaging.ts` re-parses changed `.csproj` files in `createDependencies` rather than reusing what `createNodes` decoded, which is correct because it reads `filesToProcess` alone, and a second parse of the same files.

### [02.5]-[PLUGIN_GRANULARITY]

- `include`/`exclude` and `options` are per registered entry, a module exports one `createNodes`, and one glob exists per module
- Registering the same module twice with different `include` and `options` is the documented way to vary behavior by directory
- A workspace plugin globbing `**/*` re-runs on every file change and loses per-tool scoping

For tools with one root config file (`biome.json`, `pyproject.toml`, `stryker.config.json`, `stryker-config.json`), a plugin is the wrong shape, and a `targetDefaults` entry filtered by language tag states the target with no code.

## [03]-[TARGETS_WITHOUT_PLUGINS]

### [03.1]-[RUN_COMMANDS]

Source: `node_modules/nx/dist/src/executors/run-commands/schema.json` and `node_modules/nx/executors.json`, 23.1.3. Nx includes three executors: `nx:noop`, `nx:run-commands`, `nx:run-script`.

| [OPTION] | [TYPE] | [DEFAULT] | [MEANING] |
| :-- | :-- | :-- | :-- |
| `command` | string or string[] |  | The command, the array form is the command split into parts |
| `commands` | array of string or `{command, forwardAllArgs, prefix, prefixColor, color, bgColor, description}` |  | List of commands |
| `parallel` | boolean | `true` | Run `commands` in parallel |
| `readyWhen` | string or string[] |  | Text in stdout or stderr that marks the task done |
| `args` | string or string[] |  | Extra arguments, interpolated as `{args.name}` |
| `envFile` | string |  | Custom `.env` file path |
| `color` | boolean | `false` | Color the output |
| `cwd` | string | workspace root | Relative paths resolve against the workspace root |
| `env` | object |  | Priority over the `.env` files |
| `forwardAllArgs` | boolean | `true` | Forward arguments when no interpolation is present |

The schema states of `readyWhen`: "When running multiple commands, this option can only be used when `parallel` is set to `true`", and of `env`: "This property has priority over the `.env` files."

Exactly one of `command` or `commands` is required. The `command` shorthand in a target is sugar for this executor: `"command": {"description": "Shorthand for \`nx:run-commands\` — the shell command to run."}` (`nx-schema.json`, `definitions.targetDefaultsConfig`). `eng/project.json` and `tools/nx/native-packaging.ts` use it.

The command is spawned (`node_modules/nx/dist/src/executors/run-commands/running-tasks.js:500-514`) with the environment `process.env` merged with `npm-run-path`'s `node_modules/.bin` entries, then the optional `envFile`, then the target's `env` option, and `PATH` is overridden with the local `node_modules/.bin`-prefixed value. The command never passes through `pnpm run` or `pnpm exec`, and pnpm's `verifyDepsBeforeRun` never fires inside an Nx task.

### [03.2]-[ROOT_PROJECT]

A `project.json` at the workspace root makes the root a project, and the built-in reader globs `**/project.json`. `eng/project.json` sits at `eng/`, `{projectRoot}` means `eng`, and a target on `eng` that reads `pyproject.toml` or `uv.lock` names them with `{workspaceRoot}/`.

Nx assigns a file to the nearest project root above it (`node_modules/nx/dist/src/project-graph/utils/find-project-for-path.js`). Projects rooted at `.` own every file no other project claims, and with the `default` named input rehash all of it on every change. Rasm has no root project once the root `vite.config.ts` and `vitest.config.ts` are excluded from the two plugins.

### [03.3]-[TARGET_DEFAULTS]

Source: `nx-schema.json` `definitions.targetDefaultsConfig` and `definitions.targetDefaultArrayEntry`, 23.1.3, and <https://nx.dev/docs/reference/nx-json>.

The key is a target name, a glob over target names, or an executor. The value is an object or an ordered array of filtered entries. Allowed fields, with `additionalProperties: false`: `cache`, `command`, `configurations`, `continuous`, `defaultConfiguration`, `dependsOn`, `executor`, `inputs`, `metadata`, `options`, `outputs`, `parallelism`, `syncGenerators`.

The array form adds `filter` with three criteria, all of which must match: `filter.plugin` (targets originated by one plugin), `filter.projects` (names, globs, directory patterns, `tag:foo`, `!negation`, the `findMatchingProjects` syntax), `filter.executor`. Entries apply in document order, later entries override earlier ones, and an entry with no `filter` is a catch-all baseline.

`targetDefaults` apply after plugin inference and before `project.json`. Two 23.2.0 fixes bear on the filtered form: "apply targetDefaults options to an inferred target that is not redeclared" (#36717, issue #36700, a regression in 23.1.0) and "maintain cacheability when an executor target default applies" (#36477). Neither is in the installed 23.1.3, and both land with the move to 23.2.0.

The settled shape, the round-five decision: `lint`, `format`, and `typecheck` declared once per language as filtered entries on `tag:language:dotnet`, `tag:language:python`, `tag:language:typescript`, each with `configurations: { check: {}, write: {...} }` and `defaultConfiguration: "check"`, and one `nx:noop` aggregate target with a `dependsOn` naming the whole set. Language tags are applied to every project. Tags come from the plugins that create the projects, `@nx/dotnet` and `@nx/vite` set none, a `tags` contribution is added in `targetDefaults`-adjacent project configuration or through a small `createMetadata` in the local plugin, and `tools/nx/native-packaging.ts` already sets `native` (`:248`).

### [03.4]-[DEPENDS_ON]

An array of strings or objects. Strings: `"build"` (same project), `"^build"` (every dependency's `build`). Object fields, `additionalProperties: false`: `target` (required), `projects` (names, `findMatchingProjects` syntax), `dependencies` (boolean, the `^` equivalent), `params` (`"ignore"` default or `"forward"`). The schema enforces one of `{projects, target}`, `{dependencies, target}`, or `{target}` alone.

`tools/nx/native-packaging.ts:217, :225` uses `[{ projects: ['eng'], target: 'provision' }]` and `[{ projects: [native.name], target: 'stage' }]`, both valid.

### [03.5]-[INPUTS_AND_OUTPUTS]

`definitions.inputs` accepts a string or one of eight object shapes:

| [SHAPE] | [MEANING] |
| :-- | :-- |
| `"<string>"` | Named input or glob, `^`-prefixed names refer to the dependencies' input |
| `{fileset, dependencies?}` | Glob, applied to every dependency when `dependencies: true` |
| `{input, projects?}` / `{input, dependencies?}` / `{input}` | Named input scoped to projects or dependencies |
| `{runtime}` | Shell command with hashed output |
| `{env}` | Environment variable with hashed value |
| `{externalDependencies}` | npm package names with hashed resolved versions |
| `{dependentTasksOutputFiles, transitive?}` | Glob over the outputs of the tasks this one depends on |
| `{json, fields?, excludeFields?}` | JSON file under `{workspaceRoot}` or `{projectRoot}`, hashing an allowlist or all but a denylist of fields |

A ninth shape the schema does not list, `{workingDirectory: "absolute"}`, passes `expandSingleProjectInputs` (`node_modules/nx/dist/src/hasher/task-hasher.js:245-249`) and `@nx/dotnet` emits it on every cacheable target. It is an internal detail of the dotnet plugin, not a shape to author by hand.

Named input definitions cannot start with `^` and cannot use `projects` or `dependencies`, both throw. `outputs` is a plain `string[]` of globs, and anything unlisted is not restored on a cache hit.

### [03.6]-[TARGET_FLAGS]

| [FIELD] | [TYPE] | [DEFAULT] | [MEANING] |
| :-- | :-- | :-- | :-- |
| `cache` | boolean | not set | Cacheable |
| `parallelism` | boolean | `true` | Can run alongside other tasks |
| `continuous` | boolean | `false` | Runs until stopped |
| `syncGenerators` | string[] |  | Generators run before the target |

`eng/project.json` sets `cache: false, parallelism: false` on `provision`, `tools/nx/native-packaging.ts:215-216` sets the same on every `stage` target, and both mutate one shared directory. `continuous: true` is the field for a dev server, and 23.2.0 records "include continuous and default-config dependencies in show target" (#36374).

## [04]-[ENVIRONMENT]

### [04.1]-[ENV_FILE_ORDER]

Source: `node_modules/nx/dist/src/tasks-runner/task-env-paths.js` and `task-env.js`, 23.1.3.

For a task on project root `P`, target `T`, configuration `C` (and, for an atomized task, the non-atomized target `N`), the identifiers are `T.C`, `N.C`, `C` (with a configuration alone), then `T`, `N`, then the empty identifier. Each expands to `.env.x.local`, `.env.x`, `.x.local.env`, `.x.env` (three variants for the empty one: `.env.local`, `.local.env`, `.env`), inside the project root first, then again at the workspace root. Loading uses `dotenv` with `dotenv-expand` and `override = false` (`task-env.js:138`), and an earlier, more specific file wins.

Precedence, low to high: `.env` file values, the parent process environment ("User Process Env Variables override Dotenv Variables", `:34`), Nx's own task variables (`NX_TASK_TARGET_PROJECT`, `NX_TASK_TARGET_TARGET`, `NX_TASK_TARGET_CONFIGURATION`, `NX_TASK_HASH`, `NX_WORKSPACE_ROOT`, `FORCE_COLOR`) ("Nx Env Variables overrides everything", `:36, :71`). The `env` option of `nx:run-commands` sits above the files.

Loading is gated on `NX_LOAD_DOT_ENV_FILES`: `run-one.js:19`, `run-many.js:17`, and `affected.js:19` default it to enabled, and `run-command.js:666` sets it to `'true'`. <https://nx.dev/docs/reference/environment-variables>: "If set to 'false', Nx will not load any environment files".

### [04.2]-[ENV_INPUTS]

Environment variables affect a task's hash when an `{env: "NAME"}` input names them. `vitest.config.ts:12` reads `const isCI = process.env['CI'] === 'true'` and uses it on lines 60, 90, 106, 130, and 143. No `{env: "CI"}` input exists in `nx.json`, a cache entry produced with `CI=true` and one produced without are indistinguishable, and the correction is `{ "env": "CI" }` on the `test` default. 23.2.0 withholds volatile variables from the daemon: "exclude volatile shell and editor env vars from the daemon" (#36642).

### [04.3]-[HOST_PREREQUISITES]

- Nx modifies `PATH` by prefixing `node_modules/.bin` and nothing else, and `dotnet`, `uv`, `python`, and `pnpm` must already resolve
- `@nx/dotnet` spawns an unqualified `dotnet` and fails the graph without one, and mise supplies the runtimes
- Nx installs no tools, and a .NET SDK, a Python interpreter, uv, vcpkg, and every pinned native archive are the workspace's own concern
- `nx run eng:provision` covers the native set, and mise the runtimes
- Nx sources no shell profile
- Nx reads no `.env` for graph construction, `.env` files load per task, and a plugin's `createNodes` runs before any of that
- `NX_WORKSPACE_DATA_DIRECTORY` belongs in mise `[env]` for that reason, and a `.env` file cannot hold it

## [05]-[TASK_PIPELINE_AND_AFFECTED]

### [05.1]-[BASE_AND_HEAD]

Source: <https://nx.dev/docs/features/ci-features/affected>. "The default `base` is your `main` branch, and the default `head` is your current file system." `defaultBase` in `nx.json` sets the base (Rasm: `main`), and `--base` and `--head`, or `NX_BASE` and `NX_HEAD`, override it per invocation. "The recommended approach is to set the base SHA to the latest successful commit on the `main` branch."

### [05.2]-[NX_SET_SHAS]

Source: <https://github.com/nrwl/nx-set-shas> (latest release `v5.0.1`, 2026-03-20) and <https://nx.dev/docs/features/ci-features/github-integration>, which states the workflow "works without Nx Cloud":

```yaml
permissions:
  actions: read
  contents: read
steps:
  - uses: actions/checkout@v7
    with:
      filter: tree:0
      fetch-depth: 0
  - uses: actions/setup-node@v6
    with:
      node-version: 24
  - run: npm ci
  - uses: nrwl/nx-set-shas@v5
  - run: npx nx affected -t lint test build
```

`fetch-depth: 0` gives Nx the full git history, the action sets `NX_BASE` and `NX_HEAD`, and on a push to `main` the base is "the commit of the last successful workflow run, so commits that land while CI is red still get verified". The action's inputs (`README.md`): `gh-token`, `main-branch-name`, `set-environment-variables-for-job`, `error-on-no-successful-workflow` (default false, "we will log a warning and use HEAD~1"), `fallback-sha`, `last-successful-event`, `working-directory`, `workflow-id`, `use-previous-merge-group-commit`. It needs read access to `actions` and `contents`, with `pull-request` permission under merge queues. `nx g @nx/workspace:ci-workflow --ci=github` generates the workflow file. In Rasm the `npm ci` and `setup-node` steps are replaced by `jdx/mise-action@v4` and `pnpm install`.

### [05.3]-[IMPLICIT_DEPENDENCIES]

Two distinct things share the name: `nx.json` `implicitDependencies`, "Map of files to projects that implicitly depend on them" (`nx-schema.json`), and project-level `implicitDependencies`, an array of project names adding a graph edge. `tools/nx/native-packaging.ts:249` sets the latter on managed bindings projects.

### [05.4]-[NAMED_INPUTS]

`default` falls back to `["{projectRoot}/**/*", "sharedGlobals"]`, `production` excludes test and build-output files and is referenced as `^production`, and `sharedGlobals` lists the root files every project depends on. `@nx/dotnet` picks `production` when defined.

Rasm's `sharedGlobals`: `global.json`, `NuGet.config`, `biome.json`, `package.json`, `pnpm-lock.yaml`, `pnpm-workspace.yaml`, `stryker.config.json`, `tsconfig.base.json`, `tsconfig.json`, `vite.config.ts`, `vitest.config.ts`. Root files that change build behavior and are missing:

| [FILE] | [REASON] |
| :-- | :-- |
| `Directory.Build.props`, `.targets`, `Directory.Packages.props` | `@nx/dotnet` declares them per dotnet target, no other target sees them |
| `pyproject.toml`, `uv.lock` | ruff, ty, mypy, and every `uv run` read them |
| `.editorconfig` | Analyzer severity and BuildCheck settings, dotnet targets alone declare it |
| `stryker-config.json` | The .NET mutation config |
| `Workspace.slnx` | Names the project set the solution-level restore uses |
| `nx.json` | Nx does not hash its own configuration into task hashes |

The plan's polyglot survey settles the shape: per-language named inputs so that a manifest change invalidates one language, with `sharedGlobals` holding what every language reads (`nx.json`, `.editorconfig`), and `nx.json` added so that a `targetDefaults` edit invalidates cached tasks.

### [05.5]-[GENERATED_FILES]

Every generated file is an output of the task that generates it and an input of the task that reads it, `{dependentTasksOutputFiles: "**/*"}` is the second half, and `tools/nx/native-packaging.ts` declares it on `pack`. Every path a task writes appears in `outputs`, and paths under `.artifacts/` are named with `{workspaceRoot}/` (`:218, :238-239`). `.gitignore:74-75` ignores `.cache/` and `**/.artifacts/`, Nx's file walker honors `.gitignore` and `.nxignore`, and ignored files are never inputs.

The `typecheck` target default declares `"outputs": ["{projectRoot}/dist"]`, matching `tsconfig.base.json:22, :27` (`outDir: "${configDir}/dist"`, `tsBuildInfoFile: "${configDir}/dist/tsconfig.tsbuildinfo"`), but all three tsconfigs override both: `tsconfig.json` to `.cache/typescript/out/root` and `.cache/typescript/root.tsbuildinfo`, `tools/nx/tsconfig.json` to `.cache/typescript/out/tools-nx`, `tests/typescript/support/tsconfig.json` to `.cache/typescript/out/test-support`. The declared output does not exist and the real outputs are undeclared, a cache hit restores nothing, and the `.tsbuildinfo` that decides what `tsc --build` does sits outside the cache. The correction is that `outputs` names what the compiler writes, and the emit location itself is a kept question.

## [06]-[LOCAL_CACHE]

### [06.1]-[CACHE_LOCATION]

Source: `node_modules/nx/dist/src/utils/cache-directory.js`, 23.1.3.
- The task cache defaults to `<workspaceRoot>/.nx/cache`, overridden by `nx.json` `cacheDirectory`, then `NX_CACHE_DIRECTORY`
- The variable wins over `cacheDirectory` (`:30-35`)
- Workspace data, the project graph database, and plugin caches (`dotnet-<hash>.hash`) default to `<workspaceRoot>/.nx/workspace-data`
- `NX_WORKSPACE_DATA_DIRECTORY`, then `NX_PROJECT_GRAPH_CACHE_DIRECTORY`, override the workspace data directory (`:83-84`)

`cacheDirectory` moves the task cache alone. Rasm sets `.cache/nx/cache`, `.nx/workspace-data` exists at the repository root, and `.gitignore:76` ignores `.nx/*`. The README rule that configurable caches route under `.cache/` is completed by `NX_WORKSPACE_DATA_DIRECTORY = ".cache/nx/workspace-data"`, declared once in mise `[env]` so that it exists before graph construction on every machine and in CI through `mise-action`.

In a git worktree the cache resolves to the main repository's ("In a git worktree this resolves to the main repo's cache dir so all worktrees share the same cache", `:62-63`), and 23.2.0 adds "share worktree cache under ~/.nx so agent sandboxes can reach it" (#36514) and "key the main-worktree-root cache on the workspace root" (#36768).

Documentation discrepancy: <https://nx.dev/docs/reference/environment-variables> describes `NX_CACHE_DIRECTORY` as defaulting to `~/.nx/<id>/cache` and `NX_WORKSPACE_DATA_DIRECTORY` to `~/.nx/<id>/databases`. The installed 23.1.3 source returns workspace-local paths, this machine has no `~/.nx`, and `<repo>/.nx/workspace-data` exists. The installed source is authoritative for 23.1.3, and the paths need a re-check after the 23.2.0 upgrade because #36514 moves the worktree case under `~/.nx`.

### [06.2]-[CACHE_CORRECTNESS]

The cache is keyed on a hash of the task's inputs with its definition:
1. Undeclared inputs make stale hits
2. Undeclared outputs make empty hits
3. Non-deterministic tasks are not cached, `cache: false`, which `eng:provision` and every `stage` target set

`--skip-nx-cache` (alias `--disable-nx-cache`) reruns tasks without deleting anything, `--skip-remote-cache` disables the remote cache alone, and `NX_SKIP_NX_CACHE` and `NX_DISABLE_NX_CACHE` do the same. `nx reset` clears cached artifacts and workspace metadata and stops the daemon, and `--onlyCache`, `--onlyDaemon`, `--onlyWorkspaceData` narrow it. Without a flag it discards the `@nx/dotnet` analyzer cache and forces a full MSBuild re-analysis.

### [06.3]-[REMOTE_CACHE]

There is no free, officially supported self-hosted remote cache. <https://nx.dev/docs/reference/deprecated/self-hosted-cache-packages> deprecates `@nx/s3-cache`, `@nx/gcs-cache`, `@nx/azure-cache`, and `@nx/shared-fs-cache` because of the CREEP flaw (CVE-2025-36852): "The flaw is in their design and cannot be patched", with the guidance "we recommend disabling remote cache". The remaining path is implementing the remote cache OpenAPI specification behind `NX_SELF_HOSTED_REMOTE_CACHE_SERVER` and `NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN` (<https://nx.dev/docs/kb/self-hosted-caching>).

With `neverConnectToCloud: true` permanent, `inputs` and `outputs` correctness is the whole of the caching story.

## [07]-[SYNC_GENERATORS]

Source: <https://nx.dev/docs/concepts/sync-generators> and `nx-schema.json` `properties.sync`.

Task sync generators attach to a target through `syncGenerators` and run before it, and global sync generators are listed in `sync.globalGenerators` and run from `nx sync` / `nx sync:check`. `nx.json` `sync` accepts `globalGenerators`, `generatorOptions`, `applyChanges`, and `disabledTaskSyncGenerators`. On a developer machine a task sync generator runs in `--dry-run` mode and prompts, in CI it fails the task, `nx sync:check` runs early in CI, and `--skipSync` skips them for one run.

`@nx/js:typescript-sync` keeps each project's `tsconfig` `references` in step with the project graph. Rasm lists it in `disabledTaskSyncGenerators`, `@nx/js/typescript` is not registered in `plugins`, the generator is never registered, and the entry is inert. The root `tsconfig.json` holds a hand-maintained `references` array with the comment "One reference per workspace package tsconfig". `pluginsConfig["@nx/js"].analyzeSourceFiles: false` stops `@nx/js` from parsing imports, and TypeScript project edges come from `package.json` dependencies alone. Whether the plugin and generator own the references is a kept question, and the inert `disabledTaskSyncGenerators` entry is removed either way.

## [08]-[INSPECTION_COMMANDS]

Source: `node_modules/nx/dist/src/command-line/show/command-object.js`, `watch/command-object.js`, 23.1.3, and the `nx-workspace` skill in <https://github.com/nrwl/nx-ai-agents-config>.
- `nx show projects` lists projects, with `--projects`/`-p` (names, globs, `tag:x`, `!negation`, directories)
- `nx show projects` takes `--withTarget`, `--type`, `--affected`, `--exclude`, `--json`
- `nx show project <name> --json` prints the resolved configuration including everything plugins inferred
- `nx show target <project:target>` prints the resolved target, and `nx show target inputs` and `outputs` list the resolved files and paths
- `nx graph --file=<path>` writes the graph, `.json` data or `.html` viewer, and the README specifies `nx graph --file=.artifacts/nx/graph.json`
- `nx graph --print` prints the graph JSON
- `nx report` prints Nx and plugin versions
- `nx run-many -t <targets>` and `nx affected -t <targets>` run targets, filtered with the `--projects` syntax
- `nx watch --projects <p> -- <command>` takes `--all`, `--includeDependencies`/`-d`, `--initialRun`
- `--includeDependentProjects` on `nx watch` is deprecated and "will be removed in Nx 24"
- `nx sync`, `nx sync:check`, and `nx reset` manage sync generators and the cache

Shared run flags: `--skipNxCache`, `--skipRemoteCache`, `--excludeTaskDependencies`, `--skipSync`, `--nxBail` (or `NX_BAIL=true`), `--nxIgnoreCycles`.

## [09]-[WORKSPACE_SURVEY]

Six repositories with a live `nx.json`, re-read at their default branches on 2026-09-03, show how workspaces connect tools with no official plugin.

### [09.1]-[JIMMYPAOLINI_CODEBASE]

<https://github.com/JimmyPaolini/codebase/blob/main/nx.json>: `"neverConnectToCloud": true`, `"analytics": false`, `"defaultBase": "main"`, `"cacheDirectory": ".nx/cache"`, `"parallel": 3`. Mixed TypeScript and Python. The one example that uses the filtered `targetDefaults` array form systematically, covering ruff, ty, pyright, vulture, oxlint, and oxfmt.

The pattern, repeated per tool: one `targetDefaults` key, an array with one entry, `filter.projects` on a language tag, `nx:run-commands`, explicit inputs:

```json
"ruff-lint": [{
  "cache": true,
  "configurations": { "check": {}, "write": { "args": "--fix" } },
  "defaultConfiguration": "check",
  "executor": "nx:run-commands",
  "filter": { "projects": ["tag:language:python"] },
  "inputs": ["{workspaceRoot}/pyproject.toml", "{workspaceRoot}/uv.lock", "python-source"],
  "options": { "command": "uv run ruff check {args} .", "cwd": "{projectRoot}" }
}]
```

Techniques the settled design lifts: language-specific named inputs (`python-source`, `typescript-files`), one target name with two language implementations resolved by filter (`typecheck` is an `nx:noop` depending on `pyright` and `ty` for Python and a `tsc` command for TypeScript), and `nx:noop` aggregators (`lint-codebase`, `make-projects`, `test-coverage`) with the description "An nx:noop aggregator — the work is the dependsOn targets, which Nx schedules in one graph rather than spawning a process per tool." It uses `configurations: {check, write}` with `defaultConfiguration: "check"` on every formatter, and `--configuration=write` fixes.

### [09.2]-[AMEL_TECH_MEDARIS]

<https://github.com/amel-tech/medaris/blob/main/nx.json>: `"neverConnectToCloud": true`, `"analytics": false`, `"defaultBase": "main"`. One plugin (`@nx/eslint/plugin` renamed `module-boundaries`), and the rest are `targetDefaults` commands:

```json
"lint": { "executor": "nx:run-commands", "options": { "command": "biome check {projectRoot}" }, "cache": true, "inputs": ["default", "{workspaceRoot}/biome.json"] }
```

The `biome.json` input is the point. It sets `continuous: true` on six long-running targets and hashes gitignored local env files through a `runtime` input in `sharedGlobals`: `{"runtime": "cat .env*.local apps/*/.env*.local 2>/dev/null | shasum"}`.

### [09.3]-[SMOOTHBRICKS_CODEBASE]

<https://github.com/smoothbricks/codebase/blob/main/nx.json>: `"analytics": false`. Its `lint` default runs two linters in sequence (`commands: ["biome check --files-ignore-unknown=true {projectRoot}", "eslint {projectRoot}/src"]`, `parallel: false`) and names every root file they read as an input (`package.json`, `bun.lock`, `patches/**/*`, `biome.json`, `eslint.config.ts`, `tsconfig.base.json`, `tooling/checks/**/*`).

### [09.4]-[BETTERANGELSLA_MONOREPO]

<https://github.com/BetterAngelsLA/monorepo/blob/main/apps/betterangels-backend/project.json>: `@nxlv/python:run-commands` targets running `uv run ruff check .`, `uv run ruff format --check .`, `uv run mypy`, with `cwd` and `parallel: false`. No `inputs`, `outputs`, or `cache` on `lint`, `format`, or `typecheck`, uncached by accident.

### [09.5]-[SSEFICHA_GRAM_PLATFORM_MONOREPO]

<https://github.com/sseficha/gram-platform-monorepo/blob/main/nx.json>: four target names for two ruff modes (`lint`, `lint-check`, `format`, `format-check`), each with `--config ../../pyproject.toml`. The plan adopts the one-target-two-configurations form of JimmyPaolini/codebase.

### [09.6]-[BCGOV_CAS_REGISTRATION]

<https://github.com/bcgov/cas-registration/blob/main/bciers/nx.json>: `"neverConnectToCloud": true`, `"analytics": false`, registers `@nx/next/plugin`, `@nx/playwright/plugin`, `@nx/eslint/plugin`, `@nx/vite/plugin`, `@nx/vitest`, and adds `nx:run-commands` defaults for `e2e:ui` and `e2e:report`. `sharedGlobals` is `[]`, and no root file invalidates any cache.

### [09.7]-[SHARED_PATTERNS]

| [PATTERN] | [REPOSITORIES] |
| :-- | :-- |
| Tools without a plugin become `nx:run-commands` targets, never plugins | All six |
| The tool's root config file is declared as an input | JimmyPaolini, medaris, smoothbricks |
| `cwd` is `{projectRoot}` and the command names the root config | JimmyPaolini, sseficha, BetterAngels |
| `cache: true` on lint, format, typecheck | JimmyPaolini, medaris, smoothbricks |
| Language selection by tag | JimmyPaolini |
| `configurations: {check, write}` | JimmyPaolini |
| `nx:noop` aggregator as the single entry point | JimmyPaolini |
| `continuous: true` on long-running targets | medaris |

## [10]-[NX_AGENT_SKILLS]

Source: <https://nx.dev/docs/reference/nx-mcp>, <https://github.com/nrwl/nx-ai-agents-config>, <https://nx.dev/blog/nx-ai-agent-skills> (2026-02-12).

`npx nx configure-ai-agents` writes agent rule files (`AGENTS.md`, `CLAUDE.md`, equivalents), MCP configuration, and skills, and `npx skills add nrwl/nx-ai-agents-config` installs the skills alone. The skills directory holds `link-workspace-packages`, `monitor-ci` (Nx Cloud, out of scope), `nx-generate`, `nx-import`, `nx-plugins`, `nx-run-tasks`, `nx-workspace`, with `agents/ci-monitor-subagent.md`.

From `nrwl/nx` `AGENTS.md:218`: "When running tasks (for example build, lint, test, e2e, etc.), always prefer running the task through `nx` (i.e. `nx run`, `nx run-many`, `nx affected`) instead of using the underlying tooling directly", and `:222` "NEVER guess CLI flags - always check nx_docs or `--help` first when unsure".

From the `nx-workspace` skill (`SKILL.md:48-52`): "Use `nx show project <name> --json` to get the full resolved configuration for a project. **Important**: Do NOT read `project.json` directly - it only contains partial configuration", and "You can read the full project schema at `node_modules/nx/schemas/project-schema.json`". Its troubleshooting entry for "Cannot find configuration for task" runs `nx show project X --json | jq '.targets | keys'`, then `nx show projects --withTarget`.

`nx mcp` runs in minimal mode by default, exposing `nx_docs`, `nx_visualize_graph`, and the running-task tools, `--no-minimal` re-exposes `nx_workspace` and the generator tools, and `--tools` filters by glob. The blog states the reason: "The Nx MCP server is now lean and focused on what MCP is actually for: connecting to remote services like Nx Cloud". For Rasm, the `AGENTS.md` rules fold into the `monorepo-build-infrastructure` skill, and every Nx Cloud tool is excluded from the MCP configuration.

## [11]-[CONFIGURATION_MISTAKES]

| [INDEX] | [CATEGORY] | [MISTAKE] | [CORRECT_FORM] |
| :-: | :-- | :-- | :-- |
| [01] | Undeclared input | Target reads a file no input names | Name every file read, root config in the language named input or target `inputs` |
| [02] | Empty `sharedGlobals` | `"sharedGlobals": []` (`bcgov/cas-registration`) | List the root files every language reads |
| [03] | Undeclared output | Target writes where `outputs` does not point | Every written path in `outputs`, `{workspaceRoot}/` outside the project |
| [04] | Wrong output path | `{projectRoot}/dist` while the tool writes to `.cache/typescript/` | `nx show target outputs`, then make the two agree |
| [05] | Non-deterministic task cached | `cache: true` on a provisioning task | `cache: false`, `parallelism: false` when writing a shared directory |
| [06] | Missing `{env}` input | `vitest.config.ts` reads `CI`, no input names it | `{"env": "CI"}` on `test` |
| [07] | Undeclared generated input | Another task's output consumed | `{"dependentTasksOutputFiles": "**/*"}`, `transitive` for deep chains |
| [08] | Named input misuse | `namedInputs` entry with `^` or `projects`/`dependencies` | Both throw, `^name` belongs in a target's `inputs` |
| [09] | Root project inheriting `default` | Project rooted at `.` rehashing the repository | No root project, tagged projects hold the targets |
| [10] | Plugin order | Two plugins infer the same target name | Later wins, reorder, scope with `include`/`exclude`, or use `filter.plugin` |
| [11] | `include`/`exclude` aimed at project roots | Globs written as project selectors | They match the config file path |
| [12] | Negation order | Assuming order does not matter | First to last, last match decides |
| [13] | Non-schema plugin options | `"test": true` for `@nx/dotnet` | `false` or an object, `{}` or omission leaves it alone |
| [14] | `createNodesV2` in a new plugin | Exporting it as the primary entry | `createNodes` with the batched signature |
| [15] | Reading all files in `createDependencies` | Walking `context.fileMap` | `filesToProcess` |
| [16] | Ignoring `AggregateCreateNodesError` | One bad file failing the graph | `createNodesFromFiles` |
| [17] | Assuming a solution file drives the graph | Expecting `@nx/dotnet` to read `.slnx` | It globs project files alone |
| [18] | `PackageReference` edges expected | Package edge expected to affect a consumer | `ProjectReference` edges alone, local plugin adds the rest |
| [19] | Expecting the tool to be installed | Assuming Nx puts `dotnet` or `uv` on `PATH` | mise and `eng:provision` supply them |
| [20] | `.env` precedence | Expecting `.env` to override the process environment | Files, process environment, Nx variables, then the `env` option |
| [21] | Deprecated self-hosted cache package | `@nx/s3-cache` and siblings | Local cache only |
| [22] | Assuming `cacheDirectory` moves everything | Expecting `.nx/` to disappear | `NX_WORKSPACE_DATA_DIRECTORY` in mise `[env]` |
| [23] | Affected without git history | Shallow checkout in CI | `fetch-depth: 0` with `nrwl/nx-set-shas@v5`, `actions: read`, and `contents: read` |
| [24] | Silent affected fallback | Letting `nx-set-shas` fall back to `HEAD~1` | `error-on-no-successful-workflow: true` or a `fallback-sha` |
| [25] | Disabled sync generator unchecked | Hand-maintained `references` and nothing verifying them | Kept question on `@nx/js/typescript` |
| [26] | `nx sync` skipped in CI | Drift discovered when a task fails | `nx sync:check` early |
| [27] | `parallel` with `readyWhen` | Commands list with `parallel: false` | `readyWhen` needs `parallel: true` for a commands list |
| [28] | Reading `project.json` as the truth | Inspecting targets by opening `project.json` | `nx show project <name> --json` |

## [12]-[QUESTIONS_KEPT]

The plan does not decide these, and the answer changes the design:
1. `Workspace.slnx` and the project graph: `@nx/dotnet` never reads the solution, and the mechanism that keeps the two in agreement is open (a target that compares them, or generating the solution from the graph)
2. `typecheck` emit location: whether declaration emit and `.tsbuildinfo` move from `.cache/typescript/` to `{projectRoot}/dist` (matching `tsconfig.base.json` and the current `outputs`) or to `.artifacts/typescript/`, with the `outputs` following
3. Whether `@nx/js/typescript` is registered so that `@nx/js:typescript-sync` owns the `references` arrays, given that the plugin infers `build`/`typecheck` targets beside `@nx/vite`
