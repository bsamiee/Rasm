# Repo integration audit

Agent: Audit Swift repo integration. Date: 2026-09-13. Verbatim final report.

All checks pass. Nothing further is needed; here is the report.

## Findings fixed

- `Swift.xcconfig`, `.swift-format` (root): deleted per the scope change. Swift stays at tool defaults.
- `apps/relay/Relay.xcodeproj/project.pbxproj`: removed the `Swift.xcconfig` PBXFileReference, its main-group child, and both `baseConfigurationReference` lines; moved `SWIFT_VERSION`, `SWIFT_TREAT_WARNINGS_AS_ERRORS`, and the two `SWIFT_UPCOMING_FEATURE_*` settings into the project-level Debug and Release `buildSettings`, alphabetical. Build verified identical (`SWIFT_VERSION = 6.0`, `SWIFT_TREAT_WARNINGS_AS_ERRORS = YES` in `-showBuildSettings`).
- `nx.json` `xcode` named input: dropped `Swift.xcconfig`; trimmed three toolchain `runtime` inputs to one. `xcodebuild -version` (the Xcode build number) determines the bundled Swift compiler and SDK, so `xcrun swift --version` and `--show-sdk-build-version` stated the same fact twice more. Kept `uname -m` because Debug sets `ARCHS = $(NATIVE_ARCH_ACTUAL)`.
- `nx.json` build command: removed `-configuration Debug`. The shared scheme's LaunchAction already owns `Debug`; `xcodebuild -showBuildSettings` without the flag resolves `CONFIGURATION = Debug`. Kept `-destination generic/platform=macOS`: without it xcodebuild warns `Using the first of multiple matching destinations` (`My Mac` and `Any Mac`), so the flag supplies a fact the tool lacks. Also removed the unneeded inner quoting on both commands.
- `nx.json` lint inputs: dropped `.swift-format`.
- `.gitignore`: removed `*.xcuserstate`; it only ever lives inside `xcuserdata/`, which the line above already ignores.
- `README.md`: removed the two layout rows; replaced the three [03] lines that restated nx.json and ci.yml with one principle line (a host's tool runs as a project target under that host's tag, CI runs each host's affected `check` on its runner); folded the Xcode target names into the plugin line; fixed the [04] row [03] to `global.json` for .NET, `xcode-select` for Swift with table alignment; rewrote the [05] Swift and Writers lines without the deleted files; restored the original [07] manifest sentence wording plus `.xcodeproj`; replaced "Native ..." fillers in the flow diagram.
- `apps/README.md`: layout now shows `<Area>/<Feature>.swift` grouped by responsibility with the project, `Info.plist`, and asset catalog at the root; [SWIFT] block reduced to three lines (project owns build settings, synchronized root folder with membership exceptions, swift-format at defaults); removed the `LSUIElement` line (restates Info.plist), the four-line `nx run <App>:*` block (restates nx.json), and the "matching names" duplicate now stated once in README [07].
- `apps/relay/README.md`: removed the paragraph restating that Xcode owns the build and that no JavaScript runtime exists.

## Verified correct

- Graph: `pnpm nx show projects` lists exactly one Swift project, `Relay`, root `apps/relay`, tags `language:swift`, `tool:xcode`, `host:macos`, targets `build`, `run`, `lint`, `format`, `check` and nothing else. Task graph for `Relay:check` is `check -> build, lint` (the `tag:language:*` entry's `typecheck, test` dependsOn is replaced, not merged); `Relay:run` depends on `build` exactly once.
- Plugin glob: `.cache/` is not under `{apps,libs,tests,tools}`, so derived-data pbxproj files cannot double-register; `apps/relay` has no tsconfig or pyproject. Branches are consistent in shape (root, name, tags, empty targets).
- Inputs and outputs: `build` reads `default` (all of `{projectRoot}`) plus `xcode`; output `.cache/xcode/apps/relay/Build/Products/Debug` is the real product path (`Relay.app` confirmed there). `lint` cacheable on `*.swift` plus tool version, `format` uncached, matching the other languages.
- CI exclude syntax, proven on installed Nx 23.2.0 rather than docs: `nx show projects --affected --exclude='!tag:host:macos'` returns `["Relay"]`; `--exclude=tag:host:macos` returns every project except Relay. Both forms are correct.
- `runs-on: macos-26`: actionlint 1.7.12 passes (it validates runner labels).
- pbxproj versus scheme: no setting stated in both; target-level identity settings and project-level compiler settings are disjoint.

## Not verifiable offline

- The setup action on the macOS runner: `mise.toml` has no Linux-only tool, mise-action, `pnpm install`, and `uv sync` are all macOS-capable, and `macos-26` ships Xcode 26 which `MACOSX_DEPLOYMENT_TARGET = 26.0` requires. Whether `python = "3.15.0rc2"` installs on the hosted arm64 runner needs a real run.
- Structural note, not changed: Relay's `.xcodeproj` sits at `apps/relay/` itself rather than `apps/relay/Relay/` like the `apps/<app>/<Project>/` convention for other languages; the docs now describe it as-is.

## Final exit codes

- `pnpm nx run Relay:build`: 0 (no destination warning with the restored flag)
- `pnpm nx run rasm:check`: 0
- `actionlint`: 0
- `pnpm nx run Relay:lint`: not run, per the coordinator's instruction that lint fails on indentation until the Swift sources are reformatted to defaults.
