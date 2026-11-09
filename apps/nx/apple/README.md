# [NX_APPLE]

`@rasm/nx-apple` is an Nx 24 plugin for SwiftPM packages, Xcode projects, and Xcode workspaces. Projects, targets, inputs, and edges come from `swift package describe` and from a bundled core reading `project.xcproj` through Apple's `XcodeProjectFormat` and schemes and workspaces through their XML.

## [01]-[USE]

```json
{ "plugins": [{ "plugin": "@rasm/nx-apple", "options": { "trustPackages": true } }] }
```

| [INDEX] | [OPTION]        | [DEFAULT] | [EFFECT]                                                                               |
| :-----: | :-------------- | :-------- | :------------------------------------------------------------------------------------- |
|  [01]   | `trustPackages` | `false`   | `-skipMacroValidation -skipPackagePluginValidation` for third-party macros and plugins |

- Without `trustPackages`, `xcodebuild` stops at the first third-party macro or package plugin
- Under CI, as Nx detects it, builds pass `--disable-automatic-resolution` and `-disableAutomaticPackageResolution`, so a stale `Package.resolved` fails the build

## [02]-[BUILD_STATE]

Callers share one location per package and project, chosen by the tool that owns it:

| [INDEX] | [TOOL]        | [LOCATION]                       | [STATED_BY]                                                                     |
| :-----: | :------------ | :------------------------------- | :------------------------------------------------------------------------------ |
|  [01]   | SwiftPM       | `<package>/.build`               | SwiftPM's default, `SWIFTPM_BUILD_DIR` when the environment sets it             |
|  [02]   | Xcode         | `<DerivedData>/<Project>-<hash>` | Xcode's Locations setting, or a per-user `WorkspaceSettings.xcsettings`         |
|  [03]   | SourceKit-LSP | Package's SwiftPM location       | Client options `backgroundIndexing: false`, `swiftPM.buildSystem: "swiftbuild"` |

- `swift build`, `swift test`, and Nx run the same command on the same scratch path, so neither rebuilds what the other built
- `xcodebuild` without `-derivedDataPath` and the Xcode IDE key derived data by the same project path
- `xcodebuild` ignores a shared `xcshareddata/WorkspaceSettings.xcsettings` location, so no project file moves derived data for every user
- SourceKit-LSP with those options reads the modules builds write, through a build server that takes no lock on the scratch path
- SourceKit-LSP's default background indexing builds into `.build/index-build` with the native build system
- SourceKit-LSP loads a workspace's `.sourcekit-lsp/config.json` only after its trust prompt, so clients state the options themselves
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
- Xcode inputs per target: `INFOPLIST_FILE`, `CODE_SIGN_ENTITLEMENTS`, xcconfig chains, test plans, the governing `Package.resolved`
- Missing files, scheme entries naming no target, and unlocatable xcconfig files fail their project or workspace with target, setting, and path
- Values holding `$(…)` evaluate through `xcodebuild -showBuildSettings -json -scheme`
- Target edges follow `target_dependencies` and each local dependency's `products`; targets outside products stand for their package
- Packages reach their local packages; Xcode projects reach linked modules, local packages, remote targets, and workspace members
- Every edge is implicit, since each one reads files beyond its source project
- `Package.swift` inside another package's target folder logs a warning, since SwiftPM compiles it into that target
- Inference runs one `describe` per package with `--experimental-skip-acquiring-lock`, as SourceKit-LSP's build server does, so it never waits on a running build
- Inference caches by manifest, project, scheme, workspace, and xcconfig content, paths, options, and plugin source, per options set
- `createDependencies` reads the report `createNodes` built

## [04]-[TARGETS]

| [INDEX] | [TARGET]             | [PACKAGE]     | [MODULE]                     | [XCODE]                         | [CACHE] |
| :-----: | :------------------- | :------------ | :--------------------------- | :------------------------------ | :-----: |
|  [01]   | `build`              | `swift build` | Package `build`              | `xcodebuild build` per scheme   |   No    |
|  [02]   | `test`               | `swift test`  | Package `test`               | `xcodebuild test` per test plan |   Yes   |
|  [03]   | `check--swift-build` | `build`       | Package `check--swift-build` | None                            |   No    |
|  [04]   | `check--xcodebuild`  | None          | None                         | `build`                         |   No    |
|  [05]   | `check`              | `check--*`    | `check--*`                   | `check--*`                      |   No    |

- `check--swift-build` and `check--xcodebuild` run the compile as the type check, since the Swift compiler is the type checker
- `check` is an `nx:noop` target whose `dependsOn` glob reaches every `check--*` target, so plugins sharing a project each add `check--<tool>` targets and declare the same `check`
- `check` inputs open with `'...'`, then the union of its `check--*` inputs, so `nx affected -t check` selects it for any file a dependency reads
- `swift format` and SwiftLint are hk steps in `.config/hk.pkl`; no Swift tool fixes with a project model, so no `fix` target exists
- Module `test` exists when a test target depends on the module
- Xcode `build` covers each shared scheme building a target, `test` each test plan of a scheme with a test action
- Targets run through the `run` executor, which removes `NX_*` and `LERNA_PACKAGE_NAME` from each child's environment
- SwiftPM keys its manifest cache on the whole environment, and Nx sets per-task values
- `run` appends the arguments after `--` to every invocation
- SwiftPM locks its scratch path for a whole run, so `build` and `test` run once per package, `test` after `build`
- `build` and `test` pass the same flags, so neither invalidates the other's objects
- Targets state `externalDependencies: []`, so a `pnpm-lock.yaml` change reruns none of them
- Package targets read target folders, manifests, `Package.resolved`, `.swiftpm/configuration`, and the root `.swift-version`
- Xcode targets read their folder, each referenced local package's sources, and the files `[03]` lists
- Every target hashes `DEVELOPER_DIR` and `TOOLCHAINS`, which `xcrun` reads to select Xcode and its toolchain
- `xcode-select -s` and an in-place Xcode update change no input, so `nx reset` clears results cached under the earlier Xcode
- Package-only schemes take `-destination generic/platform=<name>` per `describe` platform, else the host's `target.platform` from `swift -print-target-info`
- App schemes take their own destination
- Targets carry `metadata.description` and `help`, which `nx show project` renders

## [05]-[REQUIREMENTS]

- Hosts run macOS 27 with Xcode 27 and Swift 6.4, and projects use the JSON `project.xcproj` format
- Package script `prepare` builds the core into `.build/release/nx-apple-analyzer` on `pnpm install`; workspaces with no Xcode project or workspace never run it
- Core sits in its own SwiftPM package, so the published plugin carries it without the repository's root package

## [06]-[LAYOUT]

| [INDEX] | [PATH]                       | [HOLDS]                                                                       |
| :-----: | :--------------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts`   | Package entry: `createNodes`, `createDependencies`, the graph cache           |
|  [02]   | `plugins/swift-package.ts`   | `describe` decoding, package and module projects, package edges               |
|  [03]   | `plugins/xcode-project.ts`   | Core report decoding, build setting evaluation, Xcode projects                |
|  [04]   | `plugins/targets.ts`         | Options, toolchain inputs, target builder, child process runner, plugin error |
|  [05]   | `executors/run/`             | `run` executor, its Nx schema, the invocation shape as Effect schema          |
|  [06]   | `Sources/nx-apple-analyzer/` | Swift core over `XcodeProjectFormat`, schemes, and workspaces                 |
