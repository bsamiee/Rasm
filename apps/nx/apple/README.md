# [NX_APPLE]

`@rasm/nx-apple` is an Nx 24 plugin for SwiftPM packages, Xcode projects, and Xcode workspaces. Projects, targets, inputs, and edges come from `swift package describe` and from a bundled core reading `project.xcproj` through Apple's `XcodeProjectFormat` and schemes and workspaces through their XML.

## [01]-[USE]

```json
{ "plugins": [{ "plugin": "@rasm/nx-apple", "options": { "outputPath": ".artifacts/apple", "trustPackages": true } }] }
```

| [INDEX] | [OPTION]        | [DEFAULT]     | [EFFECT]                                                                                |
| :-----: | :-------------- | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `outputPath`    | Tool defaults | Root of `<outputPath>/<root>/{swiftpm,xcode,results,describe}`, relative, absolute, `~` |
|  [02]   | `trustPackages` | `false`       | `-skipMacroValidation -skipPackagePluginValidation` for third-party macros and plugins  |

- Folders under `outputPath` hold the SwiftPM scratch path, Xcode derived data, xUnit files and `.xcresult` bundles, and the inference scratch path
- Without `trustPackages`, `xcodebuild` stops at the first third-party macro or package plugin
- Under CI, as Nx detects it, builds pass `--disable-automatic-resolution` and `-disableAutomaticPackageResolution`, so a stale `Package.resolved` fails the build
- Per-user caches (`~/Library/Caches/org.swift.swiftpm`, Xcode's package cache) stay where SwiftPM and Xcode share them
- Without `outputPath`, `describe` runs on a scratch path under Nx's workspace data

## [02]-[INFERENCE]

| [INDEX] | [MARKER]                                      | [READER]                             | [PROJECTS]                                       |
| :-----: | :-------------------------------------------- | :----------------------------------- | :----------------------------------------------- |
|  [01]   | `Package.swift`                               | `swift package describe --type json` | Package with its namesake target, one per module |
|  [02]   | `<Name>.xcodeproj/project.xcproj`             | Core over `XcodeProjectFormat`       | One per project folder                           |
|  [03]   | `<Name>.xcworkspace/contents.xcworkspacedata` | Core over the workspace XML          | One per workspace folder                         |

- Modules are targets a product holds, other than the target named like its package
- Projects one workspace lists build through that workspace
- Packages and projects in one folder merge into one project, and fail when their names differ
- `project.pbxproj` with no `project.xcproj` beside it fails by name
- Workspaces inside an `.xcodeproj`, files under `outputPath`, and paths `.gitignore` lists stay out
- Xcode inputs per target: `INFOPLIST_FILE`, `CODE_SIGN_ENTITLEMENTS`, xcconfig chains, test plans, the governing `Package.resolved`
- Missing files, scheme entries naming no target, and unlocatable xcconfig files fail their project with target, setting, and path
- Values holding `$(…)` evaluate through `xcodebuild -showBuildSettings -json -scheme` on the build's derived data
- Target edges follow `target_dependencies` and each local dependency's `products`; targets outside products stand for their package
- Packages reach their local packages; Xcode projects reach linked modules, local packages, remote targets, and workspace members
- Every edge is implicit, since each one reads files beyond its source project
- `Package.swift` inside another package's target folder logs a warning, since SwiftPM compiles it into that target
- Inference runs one `describe` per package on its own scratch path and one core call for every Xcode container
- Inference caches by the content of manifests, projects, schemes, workspaces, and xcconfig files, the referenced paths, the options, and the plugin
- `createDependencies` reads the report `createNodes` built

## [03]-[TARGETS]

| [INDEX] | [TARGET] | [PACKAGE]               | [MODULE]        | [XCODE]                         | [CACHE] |
| :-----: | :------- | :---------------------- | :-------------- | :------------------------------ | :-----: |
|  [01]   | `build`  | `swift build`           | Package `build` | `xcodebuild build` per scheme   |   No    |
|  [02]   | `test`   | `swift test`            | Package `test`  | `xcodebuild test` per test plan |   Yes   |
|  [03]   | `lint`   | `swift format lint`     | Module folder   | Project folder                  |   Yes   |
|  [04]   | `format` | `swift format format`   | Module folder   | Project folder                  |   No    |
|  [05]   | `check`  | `lint`, `build`, `test` | Same            | Same                            |   No    |

- Package `lint` and `format` cover manifests and targets outside modules, with `--strict --parallel --recursive`
- Module `test` exists when a test target depends on the module
- Xcode `build` covers each shared scheme building a target, `test` each test plan of a scheme with a test action
- Targets run through the `run` executor, which removes `NX_*` and `LERNA_PACKAGE_NAME` from each child's environment
- SwiftPM keys its manifest cache on the whole environment, and Nx sets per-task values
- `run` removes each declared result path, creates its folder, and appends the arguments after `--`
- SwiftPM locks its scratch path for a whole run, so `build` and `test` run once per package, `test` after `build`
- `build` and `test` pass the same flags, so neither invalidates the other's objects
- `test` caches on its result files, with target folders, manifests, `Package.resolved`, and `.swiftpm/configuration` as inputs
- Xcode `test` reads its folder, each referenced local package's sources, and the files `[02]` lists
- Package-only schemes take `-destination generic/platform=<name>` per `describe` platform, else the host's from `swift -print-target-info`
- App schemes take their own destination

## [04]-[REQUIREMENTS]

- Hosts run macOS 27 with Xcode 27, and projects use the JSON `project.xcproj` format
- Package script `prepare` builds the core into `.build/release/nx-apple-analyzer` on `pnpm install`
- Core sits in its own SwiftPM package, so the published plugin carries it without the repository's root package

## [05]-[LAYOUT]

| [INDEX] | [PATH]                       | [HOLDS]                                                              |
| :-----: | :--------------------------- | :------------------------------------------------------------------- |
|  [01]   | `index.ts`                   | Plugin entry: `createNodes`, `createDependencies`, the graph cache   |
|  [02]   | `graph/swift-package.ts`     | `describe` decoding, package and module projects, package edges      |
|  [03]   | `graph/xcode-project.ts`     | Core report decoding, build setting evaluation, Xcode projects       |
|  [04]   | `graph/targets.ts`           | Options, target builders, child process runner, plugin error         |
|  [05]   | `executors/run/`             | `run` executor, its Nx schema, the invocation shape as Effect schema |
|  [06]   | `Sources/nx-apple-analyzer/` | Swift core over `XcodeProjectFormat`, schemes, and workspaces        |
