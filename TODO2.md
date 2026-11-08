# [TODO2]

## [01]-[WHY]

`tools/nx/`, `eng/typescript/`, and `nx.json` grew one target at a time: each host's build, pack, install, and publish became shell strings in `targetDefaults`, hand-decoded project files in `tools/nx/graph/`, and one-off programs in `eng/`. A host plugin stating projects, targets, and edges from the host's own JSON commands replaces each layer at once. megamono's `@megamono/nx-apple` shows the size of such a plugin, about 900 lines, and the defect to avoid: it reads project files by glob and regex, so it misses `project.xcproj` and maps a root `Package.swift` to one project.

- `tools/nx/` and `eng/typescript/` hold nothing a rebuilt plugin must keep, each host's logic leaves them as its plugin lands
- Teardown comes first and whole, a host's targets are absent until its plugin states them
- Each plugin is a publishable package another workspace installs, coupled to no path, tag, or file of this repository

## [02]-[ORGANIZATION]

- Products sit at `apps/<host>/<name>/`, the host being the program that loads the product: `apps/nx/` for Nx plugins, `apps/vscode/` for VS Code extensions
- Host decides the publish channel (`nx` to npm, `vscode` to the Marketplace and Open VSX), so release and publish derive from the `host:<host>` tag inference already states
- Packages take `@rasm/<host>-<name>` (`@rasm/nx-apple`), the name each registry lists

## [03]-[SWIFT_TEARDOWN]

- [x] Periphery: tool row and lock, CI tool list, `.config/periphery.{yml,json}`, Nx targets and inputs, inference, skill section; SwiftLint `unused_parameter` on
- [x] xcode-build-server: mise tool row and lock, Xcode `build` step, `.gitignore` row, `project.xcproj` membership exclusions, skill section
- [x] `nx.json`: `typecheck:swift`, `upgrade:swift`, `upgrade:xcode`, `swift` input, `macos` release group, Xcode and SwiftPM `build`, `pack`, `install`, `publish`, `test`, `check` rows
- [x] `tools/nx/graph/`: Xcode project marker, Swift module inference, `swiftProject`, Xcode-app components, SEA marker's Xcode inputs
- [x] `tools/nx/workspace.ts`: `swift package describe` and `dump-package` reads
- [x] `tools/nx/sync/settings/build-settings.ts`: `Xcode.xcconfig`, SwiftLint, mise, and pixi macOS-floor writers and the manifest read behind them
- [x] `eng/typescript/`: `Xcconfig`, `XcodeProject`, `XcodeRemote`, `XCODE_REQUIREMENT`, `rasm-upgrade-swift`
- [x] `infra/repository-stack.ts`: `output:appcast` feeds, CI upload paths for `.artifacts/{swift,xcode}`
- [x] Skills and README: lines stating the removed wiring

Kept: `Package.swift`, both apps and their `project.xcproj`, `Xcode.xcconfig`, `.config/Sparkle-Info.plist`, the hk checker steps and their `lint:swift-format`, `lint:swiftlint`, `lint:xcprojformatter` root targets.

Open until the plugin lands:
- `nx run rasm:infra --apply` deletes the `Relay-updates` and `Atelier-updates` feed releases, installed copies stop updating
- Atelier's SEA `pack` reads `PRODUCT_BUNDLE_IDENTIFIER`, `CODE_SIGN_ENTITLEMENTS`, and `DEVELOPMENT_TEAM` from an environment no target sets
- `Xcode.xcconfig` compiler, linker, and version rows, mise `MACOSX_DEPLOYMENT_TARGET`, and pixi macOS platforms hold their last written values

## [04]-[NX_APPLE]

`apps/nx/apple/` as `@rasm/nx-apple`, an Nx 24+ inference plugin for SwiftPM packages and Xcode projects.

### [04.1]-[SOURCES]

Every project, target, setting, and edge comes from the toolchain's JSON, never a project file's text:

| [INDEX] | [FACT]                                         | [SOURCE]                                                                    |
| :-----: | :--------------------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | Package modules, products, platforms, edges    | `swift package describe --type json`                                        |
|  [02]   | Package settings, tools version, remote pins   | `swift package dump-package`, `Package.resolved`                            |
|  [03]   | Xcode schemes, targets, configurations         | `xcodebuild -list -json`                                                    |
|  [04]   | Xcode product, bundle id, versions, xcconfigs  | `xcodebuild -showBuildSettings -json`                                       |
|  [05]   | Xcode package references (local and remote)    | Research: `XcodeProjectFormat` (`apple/xcode-project-format`) or `xcodebuild` |
|  [06]   | Toolchain identity for task hashes             | `swift --version`, `xcodebuild -version`                                    |

### [04.2]-[SCOPE]

- [ ] Research: Nx 24 `createNodesV2` and `createDependencies` contracts, plugin options, generators, release `versionActions`, how `@nx/gradle` and `@nx/dotnet` batch tool calls
- [ ] Research: every `swift package` and `xcodebuild` subcommand with JSON output, scheme sharing, `-resultBundlePath`, `-derivedDataPath`, index store, package caching
- [ ] Inference: one project per package module, executable, and Xcode project, edges from package dependencies and an app's local package products
- [ ] Targets: `build`, `test`, `lint`, `archive`, `run` per project kind, cached with toolchain and lock inputs, outputs under one artifacts root an option names
- [ ] Release: version actions writing `MARKETING_VERSION` and `CURRENT_PROJECT_VERSION` through settings, not text
- [ ] Generators: package library and executable, Xcode application, each through the toolchain's own writer
- [ ] Tests: inference over fixture workspaces, toolchain JSON recorded once per release
- [ ] Publish: npm with provenance, the `nx` host release group
- [ ] Acceptance: root `Package.swift` modules, `watch-apps`, Relay, Atelier

## [05]-[NEXT_HOSTS]

Same order per host, teardown first, then its plugin under `apps/nx/`:
- Python (`uv`, pixi, Blender extensions), .NET and Rhino (`.rhp`, Yak, NuGet), UXP and CEP, Cloudflare, Rust
- Shared release, pack, install, and publish chains leave `nx.json` as each host plugin states its own
- `tools/nx/` and `eng/typescript/` shrink to nothing a host plugin covers
