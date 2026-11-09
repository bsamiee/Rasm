# @rasm/nx-apple

Nx 24 plugin for SwiftPM packages, Xcode projects, and Xcode workspaces. Projects, targets, inputs, and edges come from `swift package describe` and from a bundled core reading `project.xcproj` through Apple's `XcodeProjectFormat`, schemes and workspaces through their XML.

## Use

```json
{ "plugins": [{ "plugin": "@rasm/nx-apple", "options": { "outputPath": ".artifacts/apple" } }] }
```

| Option             | Default             | Effect                                                                                                                                   |
| ------------------ | ------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| `outputPath`       | tool defaults       | Per project root `<outputPath>/<root>/`: `swiftpm/` scratch path, `xcode/` derived data, `results/` xUnit and `.xcresult` bundles, `describe/` inference scratch. Relative to the workspace, absolute, or `~` |
| `trustPackages`    | `false`             | `-skipMacroValidation -skipPackagePluginValidation`, without which `xcodebuild` stops at the first third-party macro or plugin             |
| `lockedResolution` | `false`             | `--disable-automatic-resolution` and `-disableAutomaticPackageResolution`, failing on a stale `Package.resolved`                          |

Per-user caches (`~/Library/Caches/org.swift.swiftpm`, Xcode's package cache) stay where SwiftPM and Xcode share them.

## Inference

| Marker                                   | Read through                          | Project                                                                 |
| ---------------------------------------- | ------------------------------------- | ----------------------------------------------------------------------- |
| `Package.swift`                          | `swift package describe --type json`  | The package, and one per library, executable, macro, and plugin target  |
| `<Name>.xcodeproj/project.xcproj`        | Core over `XcodeProjectFormat`        | One per project folder                                                  |
| `<Name>.xcworkspace/contents.xcworkspacedata` | Core over the workspace XML      | One per workspace folder; a project listed by one workspace builds through it |

- A package and a project in one folder merge into one project, and fail when their names differ
- A `project.pbxproj` with no `project.xcproj` beside it fails by name; `.build`, `.swiftpm`, `DerivedData`, `outputPath`, and workspaces inside an `.xcodeproj` are skipped
- Inputs per Xcode target: `INFOPLIST_FILE`, `CODE_SIGN_ENTITLEMENTS`, every configuration's xcconfig with its `#include` chain, every test plan a shared scheme names, and the governing `Package.resolved`; a reference to a missing file fails that project naming target, setting, and path, and a value holding `$(…)` evaluates through `xcodebuild -showBuildSettings -json`
- Edges: module to module from `target_dependencies`, and through each local dependency's `products` from `product_dependencies`; package to its modules and to local packages; Xcode targets to package modules by package path, and to other projects through remote targets, imported products, and workspace members
- A `Package.swift` inside another package's target folder logs a warning: SwiftPM compiles it into that target
- One `describe` per package on its own scratch path, so inference never waits on a build's lock, and one core call for every Xcode container, cached by the content hash of every file inference reads, the options, and the plugin and core themselves

## Targets

| Target   | Package                                      | Module                     | Xcode project or workspace                                              | Cache |
| -------- | -------------------------------------------- | -------------------------- | ----------------------------------------------------------------------- | :---: |
| `build`  | `swift build`                                | the package's `build`      | `xcodebuild build` per shared scheme building a target                  |  no   |
| `test`   | `swift test` when a test target exists       | the package's `test`, when a suite depends on the module | `xcodebuild test` per test plan of each shared scheme with a test action | yes   |
| `lint`   | `swift format lint --strict --parallel` over manifests and non-module targets | over the module folder | over the project folder                         |  yes  |
| `format` | `swift format format --in-place --parallel`  | the same paths             | the same paths                                                          |  no   |
| `check`  | `lint`, `build`, `test`                      | `lint`, `build`, `test`    | `lint`, `build`, `test`                                                 |       |

- SwiftPM locks its scratch path for a whole run, so `build` and `test` run once per package, `test` after `build`, and module targets resolve to them
- `swift` and `xcodebuild` run with every `NX_*` variable and `LERNA_PACKAGE_NAME` unset: SwiftPM keys its manifest cache on the whole environment, and Nx sets per-task values
- `build` and `test` pass the same flags, so neither invalidates the other's objects; no color or coverage flag is added
- `test` caches on its result files alone: xUnit under `results/`, and each `.xcresult` bundle, removed before its run
- A scheme building only package products takes `-destination platform=macOS`; app schemes take their own destination

## Requirements

macOS 27 with Xcode 27 and projects in the JSON `project.xcproj` format. `pnpm install` builds the core through the package's `prepare` script into `.build/release/nx-apple-analyzer`. The core is its own SwiftPM package so the published plugin carries it without the repository's root package.
