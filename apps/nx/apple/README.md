# [NX_APPLE]

`@rasm/nx-apple` is an Nx 24 plugin for SwiftPM packages, Xcode projects, and Xcode workspaces. Projects, targets, inputs, and edges come from `swift package describe` and from a bundled core reading `project.xcproj` through Apple's `XcodeProjectFormat` and schemes and workspaces through their XML.

## [01]-[USE]

```json
{ "plugins": [{ "plugin": "@rasm/nx-apple", "options": { "trustPackages": true, "outputPath": ".artifacts" } }] }
```

| [INDEX] | [OPTION]        | [DEFAULT]           | [EFFECT]                                                                                 |
| :-----: | :-------------- | :------------------ | :--------------------------------------------------------------------------------------- |
|  [01]   | `trustPackages` | `false`             | `-skipMacroValidation -skipPackagePluginValidation` for third-party macros and plugins   |
|  [02]   | `outputPath`    | Each tool's default | SwiftPM at `<outputPath>/swiftpm/<package folder>/.build`, Xcode at `<outputPath>/xcode` |

- Without `trustPackages`, `xcodebuild` stops at the first third-party macro or package plugin
- Under CI, as Nx detects it, builds pass `--disable-automatic-resolution` and `-disableAutomaticPackageResolution`, so a stale `Package.resolved` fails the build

## [02]-[BUILD_STATE]

Callers share one location per package and project:

| [INDEX] | [TOOL]        | [LOCATION]                                     | [STATED_BY]                                                             |
| :-----: | :------------ | :--------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | SwiftPM       | `<outputPath>/swiftpm/<package folder>/.build` | Nx `outputPath`, direct `--scratch-path`, `SWIFTPM_BUILD_DIR` over both |
|  [02]   | SourceKit-LSP | Package's SwiftPM location                     | `swiftPM.scratchPath` in each package's `.sourcekit-lsp/config.json`    |
|  [03]   | Xcode         | `<outputPath>/xcode/<Project>`                 | Per-user `WorkspaceSettings.xcsettings`, read by IDE and `xcodebuild`   |

- Without `outputPath`, packages build in `<package>/.build`, SwiftPM's default
- Nx passes `--scratch-path` to `describe`, `build`, and `test`, since every SwiftPM command writes `.buildSystem_<config>` into its scratch path
- Direct `swift build --scratch-path <the same path>` after an Nx build runs on the on-disk description and compiles nothing
- SourceKit-LSP resolves a relative `scratchPath` against the package root and reads the config only after its trust prompt, once per user
- Nx passes no `-derivedDataPath`, which moves `ModuleCache.noindex` and `CompilationCache.noindex` from beside the project folders into one
- Switching between a custom location and `-derivedDataPath` changes every compile's module cache path and recompiles every Swift file
- `xcodebuild` honors the per-user `WorkspaceSettings.xcsettings` and ignores the shared `xcshareddata` copy, so no committed file moves derived data
- Repositories write the per-user file from `outputPath` through `plutil`, `WorkspaceRelativePath` style with `<outputPath>/xcode` relative to each project's folder
- Without that file, Nx, `xcodebuild`, and the IDE share Xcode's default derived data
- `xcodebuild` writes `<package>/.swiftpm/xcode/` for each local package it resolves
- Per-user caches (`~/Library/Caches/org.swift.swiftpm`, Xcode's package cache) stay where SwiftPM and Xcode share them
- Xcode `test` keeps its result bundles in derived data `Logs/Test`, where the IDE's report navigator lists them

## [03]-[INFERENCE]

| [INDEX] | [MARKER]                                      | [READER]                             | [PROJECTS]                                       |
| :-----: | :-------------------------------------------- | :----------------------------------- | :----------------------------------------------- |
|  [01]   | `Package.swift`                               | `swift package describe --type json` | Package with its namesake target, one per module |
|  [02]   | `<Name>.xcodeproj/project.xcproj`             | Core over `XcodeProjectFormat`       | One per project folder                           |
|  [03]   | `<Name>.xcworkspace/contents.xcworkspacedata` | Core over the workspace XML          | One per workspace folder                         |

- Modules are targets a product holds, other than the target named like its package
- Projects one workspace lists build through that workspace
- Packages and projects in one folder merge into one project, and fail when their names differ
- `project.pbxproj` with no `project.xcproj` beside it fails by name
- Workspaces inside an `.xcodeproj` and paths `.gitignore` lists stay out
- Xcode inputs per target: `INFOPLIST_FILE`, `CODE_SIGN_ENTITLEMENTS`, each configuration's base xcconfig, test plans, the governing `Package.resolved`, file and folder references outside the project folder
- Files an xcconfig `#include`s are no inputs, since no Xcode output names them
- `INFOPLIST_FILE` and `CODE_SIGN_ENTITLEMENTS` come from `xcodebuild -showBuildSettings -json` per shared scheme and configuration, resolved against `SRCROOT`
- Missing files, scheme entries naming no target, and unlocatable base xcconfig files fail their project or workspace with target, setting, and path
- Local packages sharing a folder name fail as undecodable, since `XcodeProjectFormat` refuses the project as Xcode does
- Target edges follow `target_dependencies` and each local dependency's `products`; targets outside products stand for their package
- Packages reach their local packages; Xcode projects reach linked modules, local packages, remote targets, and workspace members
- Every edge is implicit, since each one reads files beyond its source project
- `Package.swift` inside another package's target folder logs a warning, since SwiftPM compiles it into that target
- Inference runs one `describe` per package with `--experimental-skip-acquiring-lock`, as SourceKit-LSP's build server does, so it never waits on a running build
- Inference caches per options set by manifest, project, scheme, workspace, and xcconfig content, paths, plugin sources, and the built core
- `createDependencies` reads the report `createNodes` built
- Xcode projects reach each project whose declared `outputs` hold a file or folder they reference
- Failures reach Nx as plain errors carrying their message, since Nx copies only an error's own properties out of the plugin worker

## [04]-[TARGETS]

| [INDEX] | [TARGET]             | [PACKAGE]              | [MODULE]                     | [XCODE]                         | [CACHE] |
| :-----: | :------------------- | :--------------------- | :--------------------------- | :------------------------------ | :-----: |
|  [01]   | `build--swift-build` | `swift build`          | Package `build--swift-build` | None                            |   No    |
|  [02]   | `build--xcodebuild`  | None                   | None                         | `xcodebuild build` per scheme   |   No    |
|  [03]   | `test`               | `swift test`           | Package `test`               | `xcodebuild test` per test plan |   Yes   |
|  [04]   | `check--swift-build` | `build--swift-build`   | Package `check--swift-build` | None                            |   No    |
|  [05]   | `check--xcodebuild`  | None                   | None                         | `build--xcodebuild`             |   No    |
|  [06]   | `build`, `check`     | `build--*`, `check--*` | `build--*`, `check--*`       | `build--*`, `check--*`          |   No    |

- `check--swift-build` and `check--xcodebuild` run the compile as the type check, since the Swift compiler is the type checker
- `build` and `check` are `nx:noop` targets whose `dependsOn` glob reaches every `build--*` or `check--*` target with `params: "forward"`, so plugins sharing a project each add `<verb>--<tool>` targets and declare the same aggregate, and arguments after `--` reach every member
- `build--xcodebuild` depends on `^build`, so the producers whose outputs a project references (an embedded `.ccx`, a bundled script) build first; it also builds the linked local packages through SwiftPM, which `xcodebuild` builds again in its own derived data
- Projects with no `<verb>--*` target get no aggregate
- Aggregate inputs open with `'...'`, then the union of their `<verb>--*` inputs, so `nx affected` selects them for any file a dependency reads
- `swift format` and SwiftLint are hk steps in `.config/hk.pkl`; no Swift tool fixes with a project model, so no `fix` target exists
- Module `test` exists when a test target depends on the module
- Xcode `build--xcodebuild` covers each shared scheme building a target, `test` each test plan of a scheme with a test action; a project with no shared scheme gets neither
- Xcode targets run no producer's `build` before them: an edge to a project producing a referenced file marks the Xcode project affected and orders no task
- Targets run through the `run` executor, which removes `NX_*` and `LERNA_PACKAGE_NAME` from each child's environment
- SwiftPM keys its manifest cache on the whole environment, and Nx sets per-task values
- `run` appends the arguments after `--` to every invocation
- SwiftPM locks its scratch path for a whole run, so `build--swift-build` and `test` run once per package, `test` after the build
- `build--swift-build` and `test` pass the same flags, so neither invalidates the other's objects
- Targets state `externalDependencies: []`, so a `pnpm-lock.yaml` change reruns none of them
- Package targets read target folders, manifests, `Package.resolved`, `.swiftpm/configuration`, and the root `.swift-version`
- Xcode targets read their folder, each referenced local package's sources, and the files `[03]` lists
- Every target hashes `DEVELOPER_DIR` and `TOOLCHAINS`, which `xcrun` reads to select Xcode and its toolchain
- `xcode-select -s` and an in-place Xcode update change no input, so `nx reset` clears results cached under the earlier Xcode
- Package-only schemes take `-destination generic/platform=<name>` per `describe` platform, else the host's `target.platform` from `swift -print-target-info`
- App schemes take their own destination
- Xcode targets carry one Nx configuration per build configuration the project declares, lowercased, so `-c release` passes `-configuration Release`
- `defaultConfiguration` is the shared schemes' launch configuration when they agree
- Targets carry `metadata.description` and `help`, which `nx show project` renders

## [05]-[RELEASE]

- Each `.xcodeproj` project names `@rasm/nx-apple/release/version-actions` with its `project.xcproj` as the manifest
- Version actions read the project-level `MARKETING_VERSION` as the current version
- New versions write `MARKETING_VERSION` and raise the project-level `CURRENT_PROJECT_VERSION` by one, through `XcodeProjectFormat` in the core
- Projects publish through their host channel, so the registry current-version resolver fails; use the disk or git-tag resolver

## [06]-[REQUIREMENTS]

- Hosts run macOS 27 with Xcode 27 and Swift 6.4, and projects use the JSON `project.xcproj` format
- Package script `prepare` builds the core in release mode on `pnpm install`, at the `swiftPM.scratchPath` in the core's `.sourcekit-lsp/config.json`
- Plugin runs the core from that path whatever `outputPath` says; workspaces with no Xcode project or workspace never run it
- Core sits in its own SwiftPM package, so the published plugin carries it without the repository's root package

## [07]-[LAYOUT]

| [INDEX] | [PATH]                       | [HOLDS]                                                                             |
| :-----: | :--------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts`   | Package entry: options, `createNodes`, `createDependencies`, the graph cache        |
|  [02]   | `plugins/swift-package.ts`   | `describe` decoding, package and module projects, package edges                     |
|  [03]   | `plugins/xcode-project.ts`   | Core report decoding, build setting evaluation, Xcode projects                      |
|  [04]   | `plugins/targets.ts`         | Toolchain inputs, core location, target builder, child process runner, plugin error |
|  [05]   | `executors/run/`             | `run` executor, its Nx schema, the invocation shape as Effect schema                |
|  [06]   | `version-actions.ts`         | Nx release version actions over the core's `version` subcommand                     |
|  [07]   | `Sources/nx-apple-analyzer/` | Swift core over `XcodeProjectFormat`: `analyze` reports, `version` writes versions  |
