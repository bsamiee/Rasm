# [NX_APPLE]

`@rasm/nx-apple` is an Nx 24 plugin for SwiftPM packages, Xcode projects, and Xcode workspaces. Projects, targets, inputs, and edges come from `swift package describe` and from a bundled core reading `project.xcproj` through Apple's `XcodeProjectFormat` and schemes and workspaces through their XML.

## [01]-[USE]

```json
{ "plugins": [{ "plugin": "@rasm/nx-apple", "options": { "trustPackages": true, "outputPath": ".artifacts", "notaryProfile": "<profile>" } }] }
```

| [INDEX] | [OPTION]        | [DEFAULT]                              | [EFFECT]                                                                                                        |
| :-----: | :-------------- | :------------------------------------- | :-------------------------------------------------------------------------------------------------------------- |
|  [01]   | `trustPackages` | `false`                                | `-skipMacroValidation -skipPackagePluginValidation` for third-party macros and plugins                          |
|  [02]   | `outputPath`    | Archives at Xcode's Organizer location | Archives and zips under `<outputPath>/xcode/<project>/<scheme>`, executable zips under `<outputPath>/swiftpm/<product>`, `build--ditto`, `build--notarytool`, and executable `nx-release-publish` inferred |
|  [03]   | `notaryProfile` | None                                   | `notarytool submit --keychain-profile`, read when `build--notarytool` runs                                      |

- Without `trustPackages`, `xcodebuild` stops at the first third-party macro or package plugin
- `outputPath` exists because `-exportArchive` and `ditto` take their destination from their caller alone, and Nx reads outputs only inside the workspace
- Projects never need `notaryProfile` to infer: `build--notarytool` follows the project's own signing settings, and a run without the profile fails naming it
- `notaryProfile` exists because `notarytool` reads credentials from arguments alone; CI saves the profile with `notarytool store-credentials`
- Under CI, as Nx detects it, builds pass `--disable-automatic-resolution` and `-disableAutomaticPackageResolution`, so a stale `Package.resolved` fails the build

## [02]-[BUILD_STATE]

Each tool builds at its own default location, which every caller reaches with no option, flag, or file:

| [INDEX] | [TOOL]  | [LOCATION]                                               | [SHARED_BY]                            |
| :-----: | :------ | :------------------------------------------------------- | :------------------------------------- |
|  [01]   | SwiftPM | `<package>/.build`                                       | Nx, direct `swift` runs, SourceKit-LSP |
|  [02]   | Xcode   | `~/Library/Developer/Xcode/DerivedData/<Project>-<hash>` | Nx, direct `xcodebuild`, Xcode app     |

- SwiftPM has no configuration file for its build folder, so Nx passes no `--scratch-path`, and repositories list `.build/` in `.gitignore`
- Every SwiftPM command, `describe` included, writes `.buildSystem_<config>` into `.build`, so inference and builds share one folder
- Direct `swift build` after an Nx build runs on the on-disk description and compiles nothing
- SourceKit-LSP builds in `.build` with background indexing off
- `SWIFTPM_BUILD_DIR` overrides every caller's location, so it stays unset
- Nx passes no `-derivedDataPath`, so `xcodebuild` resolves the folder the Xcode app keys by workspace path and neither recompiles the other's files
- `-derivedDataPath` moves `ModuleCache.noindex` and `CompilationCache.noindex` inside the project folder, which changes every compile's module cache path and recompiles every Swift file
- `xcodebuild` writes `<package>/.swiftpm/xcode/` for each local package it resolves
- Per-user caches (`~/Library/Caches/org.swift.swiftpm`, Xcode's package cache) stay where SwiftPM and Xcode share them
- Xcode `test` keeps its result bundles in derived data `Logs/Test`, where the Xcode app's report navigator lists them

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
- Xcode inputs per target: `INFOPLIST_FILE`, `CODE_SIGN_ENTITLEMENTS`, each configuration's base xcconfig, test plans, the governing `Package.resolved`, build-phase files outside the project folder, the sources of each referenced local package
- Files an xcconfig `#include`s are no inputs, since no Xcode output names them
- `INFOPLIST_FILE` and `CODE_SIGN_ENTITLEMENTS` come from `xcodebuild -showBuildSettings -json` per shared scheme and configuration, resolved against `SRCROOT`
- Missing files, scheme entries naming no target, and unlocatable base xcconfig files fail their project or workspace with target, setting, and path
- Local packages sharing a folder name fail as undecodable, since `XcodeProjectFormat` refuses the project as Xcode does
- Target edges follow `target_dependencies` and each local dependency's `products`; targets outside products stand for their package
- Packages reach their local packages; Xcode projects reach no linked module, local package, remote target, or workspace member, since Xcode builds each itself and their sources are task inputs
- Every edge is implicit, since each one reads files beyond its source project
- `Package.swift` inside another package's target folder logs a warning, since SwiftPM compiles it into that target
- Inference runs one `describe` per package with `--experimental-skip-acquiring-lock`, as SourceKit-LSP's build server does, so it never waits on a running build
- Inference caches per options set by manifest, project, scheme, workspace, and xcconfig content, the `package.json` beside each manifest, paths, plugin sources, and the built core
- `createDependencies` reads the report `createNodes` built
- Xcode projects reach each project whose declared `outputs` hold a build-phase file (`FileReference.buildFiles`), and no other project
- Failures reach Nx as plain errors carrying their message, since Nx copies only an error's own properties out of the plugin worker
- Every project carries the `os:darwin` host tag (Node's `process.platform` value), so a Linux runner excludes them by tag
- Hosts lacking `swift`, `xcodebuild`, or the core infer each project from its file alone: the folder names it, `build--<tool>` runs the tool on it, and no edge or scheme is read
- Runs of an absent program fail with one line naming the program and the host platform, and inference logs which tools are absent
- Inference results from such a host stay out of the graph cache, so the next run with the tools reads everything
- Warnings go through Nx's `logger.warn`, which writes to standard error, so `nx show project --json` prints JSON alone

## [04]-[TARGETS]

| [INDEX] | [TARGET]             | [PACKAGE]              | [MODULE]                     | [XCODE]                         | [CACHE] |
| :-----: | :------------------- | :--------------------- | :--------------------------- | :------------------------------ | :-----: |
|  [01]   | `build--swift-build` | `swift build`          | Package `build--swift-build` | None                            |   No    |
|  [02]   | `build--xcodebuild`  | None                   | None                         | `xcodebuild build` per scheme   |   No    |
|  [03]   | `test`               | `swift test`           | Package `test`               | `xcodebuild test` per test plan |   Yes   |
|  [04]   | `check--swift-build` | `build--swift-build`   | Package `check--swift-build` | None                            |   No    |
|  [05]   | `check--xcodebuild`  | None                   | None                         | `build--xcodebuild`             |   No    |
|  [06]   | `build--ditto`       | Executable product zip | Executable product zip       | Zip of each archived app        |   No    |
|  [07]   | `build--notarytool`  | None                   | None                         | Export, notarize, staple, zip   |   No    |
|  [08]   | `build`, `check`     | `build--*`, `check--*` | `build--*`, `check--*`       | `build--*`, `check--*`          |   No    |
|  [09]   | `install`            | Executable product     | Executable product           | `xcodebuild install` per scheme |   No    |
|  [10]   | `nx-release-publish` | Executable product     | Executable product           | None                            |   No    |

- `check--swift-build` and `check--xcodebuild` run the compile as the type check, since the Swift compiler is the type checker
- `build` and `check` are `nx:noop` targets over every `build--*` or `check--*` target, so plugins sharing a project add to one aggregate
- Aggregates forward no arguments: Nx passes a command's arguments down every dependency's `dependsOn`, so forwarding would reach each `<verb>--*` tool of every project `^build` reaches; one-run switches go to the tool-named target (`nx run <project>:build--xcodebuild -- -jobs 4`)
- `build--xcodebuild` depends on `^build`, so producers of its build-phase files (an embedded helper, a bundled script) build first, and no SwiftPM build runs before it
- Projects with no `<verb>--*` target get no aggregate
- Aggregate inputs open with `'...'`, then the union of their `<verb>--*` inputs, so `nx affected` selects them for any file a dependency reads
- `swift format` and SwiftLint are hk steps in `.config/hk.pkl`; no Swift tool fixes with a project model, so no `fix` target exists
- Module `test` exists when a test target depends on the module
- Xcode `build--xcodebuild` covers each shared scheme building a target, `test` each test plan of a scheme with a test action; a project with no shared scheme gets neither
- Targets run through the `run` executor, which removes `NX_*` and `LERNA_PACKAGE_NAME` from each child's environment
- SwiftPM keys its manifest cache on the whole environment, and Nx sets per-task values
- `run` appends the arguments after `--` to every invocation
- SwiftPM locks its scratch path for a whole run, so `build--swift-build` and `test` run once per package, `test` after the build
- `build--swift-build` and `test` pass the same flags, so neither invalidates the other's objects
- Targets run by this plugin's executors read the `default` input of `@rasm/nx-apple` and every optional dependency its manifest names, since Nx hashes executor code only for `@nx/` packages, and `externalDependencies: []`, so a `pnpm-lock.yaml` change reruns none of them
- Installed from a registry, the plugin states those packages as `externalDependencies` in place of projects
- Package targets read target folders (test targets included), manifests, `Package.resolved`, `.swiftpm/configuration`, and the root `.swift-version`
- Xcode targets read their folder, each referenced local package's sources, and the files `[03]` lists
- Every target hashes `DEVELOPER_DIR` and `TOOLCHAINS`, which `xcrun` reads to select Xcode and its toolchain
- `xcode-select -s` and an in-place Xcode update change no input, so `nx reset` clears results cached under the earlier Xcode
- Package-only schemes take `-destination generic/platform=<name>` per `describe` platform, else the host's `target.platform` from `swift -print-target-info`
- App schemes take their own destination
- Xcode targets carry one Nx configuration per build configuration the project declares, lowercased, so `-c release` passes `-configuration Release`
- `defaultConfiguration` is the shared schemes' launch configuration when they agree
- Package `build--swift-build` takes `-c release` as `--configuration release`, the build a platform package publishes
- Inference reads scheme settings through `xcodebuild archive -showBuildSettings -json`: unflagged for the archive configuration, then per other configuration
- Schemes archive what their targets install (`SKIP_INSTALL = NO`), so `build--xcodebuild` in the archive configuration runs `xcodebuild archive`
- Archives land at `<outputPath>/xcode/<project>/<scheme>/<scheme>.xcarchive`, else at Xcode's Organizer location, which the Xcode app lists
- Under `outputPath`, archives whose installed targets sign in the archive configuration with a `CODE_SIGN_IDENTITY` naming a `Developer ID Application` certificate and `ENABLE_HARDENED_RUNTIME = YES` go to `build--notarytool`, every other archive to `build--ditto`
- `build--ditto` zips the archived app into `<app>-<CFBundleShortVersionString>.zip` for a GitHub release, and an executable product's release build (the folder `swift build --configuration release --show-bin-path` prints) into `<outputPath>/swiftpm/<product>/<product>.zip` with the binary at the archive's root
- Both run nothing outside the archive configuration
- Export options hold the method alone, since team and signing style default to the archive's
- Exports run `xcodebuild -exportArchive`, `ditto -c -k --keepParent` into `<app>-<CFBundleShortVersionString>.zip`, `notarytool submit --wait`, `stapler staple`, and `ditto` again
- Arguments after `--` reach `notarytool submit` on `build--notarytool` and `ditto` on `build--ditto`
- Export fails until the team holds a Developer ID Application certificate, naming the team
- Targets carry `metadata.description` and `help`, which `nx show project` renders

## [05]-[INSTALL]

`install` places the current local build where a user's copy runs, through the toolchain's own install, with no version, release, quit, or relaunch:

| [INDEX] | [PRODUCT]                                               | [RUNS]                                                                                                           | [PLACES]                                       |
| :-----: | :------------------------------------------------------ | :--------------------------------------------------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | SwiftPM executable product                              | `swift build -c release --product`, `swift package experimental-uninstall`, `swift package experimental-install` | SwiftPM's install folder, which SwiftPM prints |
|  [02]   | Xcode scheme building a target with `SKIP_INSTALL = NO` | `xcodebuild install -scheme <scheme> DSTROOT=<INSTALL_PATH> INSTALL_PATH=/`                                      | `INSTALL_PATH`, `/Applications` for an app     |

- SwiftPM refuses to install over an installed product and to uninstall an absent one
- `install` runs `experimental-uninstall` only when SwiftPM's install folder (`$XDG_CONFIG_HOME/swiftpm/bin`, else `~/.swiftpm/bin`) lists the product
- `install` builds first, so a failing build leaves the installed copy in place
- `install` sits on the project of the product's executable target, which `swift package describe` names, unless the package folder holds a `package.json` without `private: true`, whose npm package delivers the executable (the core's platform package)
- Xcode's `install` action builds the scheme's archive configuration in derived data `ArchiveIntermediates`, signs, writes into `DSTROOT` at `INSTALL_PATH`, and registers the bundle with Launch Services
- `DSTROOT=/` fails in Xcode 27 with `Cycle inside a single target`, since the build directory it creates at `/` contains the derived data folders, so the run moves the evaluated `INSTALL_PATH` into `DSTROOT`; a scheme whose installing targets state more than one `INSTALL_PATH` fails inference by name
- `install` depends on `^build`, so producers of build-phase files (an embedded helper, a launch agent plist) build first; it never depends on the project's own `build`, since the install action is that build
- Installs merge into the existing bundle and remove stale files only when the same derived data produced them, so the first install over a copy another source placed keeps that copy's extra files
- Running copies keep running on their old image: each written file is a new file, and the next launch, login, or agent restart runs the installed build; a launchd agent the app registered through `SMAppService` runs its new helper when launchd next starts it
- Every install reruns, since it writes outside the workspace
- Arguments after `--` reach `xcodebuild install` (`nx install <app> -- -jobs 4`) and SwiftPM's release build (`-- -j 4`), which `experimental-install` then reuses, since it refuses build options after the subcommand; `-c` selects no configuration, since the install action builds the scheme's archive configuration and SwiftPM installs release builds

## [06]-[RELEASE]

- Each `.xcodeproj` project names `@rasm/nx-apple/release/version-actions` with its `project.xcproj` as the manifest and carries the `release` tag, which `nx.json` selects
- Version actions read the project-level `MARKETING_VERSION` as the current version
- New versions write `MARKETING_VERSION` and raise the project-level `CURRENT_PROJECT_VERSION` by one, through `XcodeProjectFormat` in the core
- SwiftPM library modules carry no release, since SwiftPM resolves a dependency's versions only from plain repository-wide `X.Y.Z` tags
- Executable products carry the `release` tag and the version actions with no manifest, since SwiftPM manifests hold no version: the current version is the project's newest `{projectName}@{version}` tag, which Nx's `git-tag` resolver reads, and a project with no tag reads `0.0.0`, the version Nx implies for an unreleased project, so a first release needs no starting tag
- New versions write nothing, and `nx release` records each in its tag and GitHub release
- Executable `nx-release-publish` attaches the zip `build--ditto` wrote in the release configuration to the GitHub release of the project's tag (`gh release view`, then `gh release upload`), skipping an asset the release holds, failing an immutable release or a missing zip by name, and logging each upload under `dryRun` or `NX_DRY_RUN`; run alone, the newest reachable tag (`git describe --tags --match`) names the release
- Registry current-version resolution finds nothing, so Nx takes its configured fallback
- Distributables come from `nx build <project> -c release`; GitHub release assets and Sparkle feeds belong to `@rasm/nx-distribution`

## [07]-[REQUIREMENTS]

- Hosts run macOS 27 with Xcode 27 and Swift 6.4, and projects use the JSON `project.xcproj` format
- Core ships as `@rasm/nx-apple-darwin-arm64`, an optional dependency with `os` and `cpu` set, so npm and pnpm install it on Apple silicon Macs alone
- Plugin resolves the core through its `#analyzer` import of the platform package's `./nx-apple-analyzer` export
- Workspaces with no Xcode project or workspace never run the core
- Platform package exports the path `swift build --configuration release --show-bin-path` prints, so the tree and the tarball share one path
- Platform package `prepare` builds the core on `pnpm install`, and pnpm strips `prepare` from the published manifest
- Core sits in its own SwiftPM package, so the published plugin carries it without the repository's root package

## [08]-[LAYOUT]

| [INDEX] | [PATH]                                | [HOLDS]                                                                             |
| :-----: | :------------------------------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts`            | Package entry: options, `createNodes`, `createDependencies`, the graph cache        |
|  [02]   | `plugins/swift-package.ts`            | `describe` decoding, package and module projects, package edges                     |
|  [03]   | `plugins/xcode-project.ts`            | Core report decoding, build setting evaluation, Xcode projects                      |
|  [04]   | `plugins/targets.ts`                  | Toolchain inputs, core location, target builder, child process runner, plugin error |
|  [05]   | `executors/run/`                      | `run` executor, its Nx schema, the invocation shape as Effect schema                |
|  [06]   | `executors/install/`                  | `install` executor replacing a SwiftPM executable product, its Nx schema            |
|  [07]   | `executors/zip/`                      | `zip` executor zipping archives and executable products, notarizing Developer ID apps |
|  [08]   | `executors/release-publish/`          | `nx-release-publish` executor attaching executable zips to their GitHub release      |
|  [09]   | `version-actions.ts`                  | Nx release version actions over the core's `version` subcommand                     |
|  [10]   | `analyzer/`                           | `@rasm/nx-apple-darwin-arm64`: the core's SwiftPM package and its npm manifest      |
|  [11]   | `analyzer/Sources/nx-apple-analyzer/` | Swift core over `XcodeProjectFormat`: `analyze` reports, `version` writes versions  |
