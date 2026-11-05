# [SWIFT]

SwiftPM builds modules and executables through root `Package.swift`, Xcode builds apps through `.xcodeproj` files linking its products.

## [01]-[PACKAGE]

- `Package.swift` takes the newest tools version the toolchain accepts, tools version 6 implies language mode 6 and no `swiftLanguageModes` row
- Target paths hold Swift sources alone, a `Package.swift` inside one compiles as a source and any other file warns as unhandled
- Target paths are disjoint, SwiftPM fails a folder inside another target's path on overlapping sources
- Module names avoid SDK module names, a target named `Accessibility`, `System`, or `ApplicationServices` cycles through AppKit
- Products list library modules alone, SwiftPM synthesizes each executable's product
- Manifest `support` targets hold test libraries and fixture executables
- Modules take their suites' dependencies as Nx edges, a `support` target importing a package module cycles with each module whose suite uses it
- Test targets take `<Module>Tests`, or `<Module>IntegrationTests` for cases needing a GUI session or TCC grant
- Nx attaches each test target to its module's `test` or `test:integration` through `--test-product`, `--filter` builds every test target
- `OMIT_DYNAMIC_TEST_SUPPORT` links `IssueReportingTestSupport` static for swift-custom-dump tests, the dynamic form prints linker version warnings
- Remote packages take `.package(url:from:)`, a package only an app imports takes an `up-to-next-major-version` row in that app instead, `upgrade:swift` moves both
- Manifest `settings` own compiler and linker policy in typed forms, `unsafeFlags` where none exists
- Checkers run from root tool rows, a SwiftLint or swift-format package plugin adds a second tool version

## [02]-[SWIFTPM]

- SwiftPM takes its scratch path from `SWIFTPM_BUILD_DIR` over `--scratch-path` and reads it from no file
- SwiftPM writes `.build/` in the working folder when neither is set, `--help` included
- `SWIFTPM_BUILD_DIR` holds an absolute path in `mise.toml` `[env]`, SwiftPM resolves a relative one against the working folder
- `sourcekit-lsp` reads `swiftPM.scratchPath` and its `swift build` preparation reads `SWIFTPM_BUILD_DIR`, each client sets both to one folder
- SwiftPM locks its scratch path from workspace load to exit, `--skip-build` included, targets that compile take `parallelism: false`
- `typecheck:swift` writes every product `test`, `test:integration`, and `lint:periphery` read, and runs uncached since a hit leaves them another checkout's binaries
- `test` and `test:integration` pass `--skip-build --experimental-skip-acquiring-lock`, no running or hung case holds the scratch path every SwiftPM target shares
- `swift test` stdout buffers until exit off a TTY, the `events.jsonl` beside attachments streams one flushed record per event for one test product
- Executables outside `tests/` `install` through `experimental-install` into `$XDG_CONFIG_HOME/swiftpm/bin`, mise `[env]` states that variable and puts the folder on `_.path`
- Executables a terminal or harness runs hold its TCC grants, a reinstall keeps them and no bundle is needed

## [03]-[PROJECT_FILES]

`project.xcproj` uses Xcode's hierarchical JSON format under Apple's schema for keys and defaults:
- `xcrun xcodeproj` writes project edits, `target add` writes target and product IDs, schemes reference them
- Project `configurations` define names once, target `specialized-configurations` appear only for configuration files or IDs
- `kind: "folder"` at `path: "."` discovers sources through `target-membership`, files join existing build phases by type
- Apps with assets take a `resources` phase with `membership-exceptions` excluding non-sources
- Projects hold one shared scheme in `xcshareddata/xcschemes`, its `BuildAction` and `LaunchAction` naming the target
- Xcode autocreates a duplicate of a shared scheme with no `LaunchAction`
- App icons are an `AppIcon.icon` document `actool` renders at build, no rendered image is checked in
- `icon.json` holds the values an app states, appearances and platforms follow from rows it omits, `position` needs `scale` and `translation-in-points` together, `scale` multiplies the master's point size on the 1024 canvas
- Views take system colors, a color an app states is a color set its target names through `ASSETCATALOG_COMPILER_GLOBAL_ACCENT_COLOR_NAME`, a root row warns in every app holding no such set
- `ictool` for `.icon` documents sits at `$(xcode-select -p)/../Applications/Icon Composer.app/Contents/Executables/ictool`, `xcrun --find ictool` and `/usr/bin/ictool` resolve to an `actool` front end
- `packages` holds the root package as `{ "kind": "local", "path": "<parents to root>" }`, product rows name it `..`
- `xcprojformatter` drops `name` and `id` keys from a `packages` row
- `package-product-members` holds one row per module the app imports, Nx draws app module edges from rows naming a root library product
- `..` rows link a product of any package the root manifest depends on, an app lists a remote row only for a package the manifest omits
- Requirement changes run `pkg remove`, `pkg add`, and `pkg link` per linked product, in `packages` order
- `pkg link` finds no `..` package while the synchronized folder holds a subdirectory, its rows join `package-product-members` by hand under `xcprojformatter`
- Synchronized folders outside the project join through `group add --filesystem-path`, which writes a group-relative path
- Info.plist keys with no `INFOPLIST_KEY_*` setting sit in `.config/Sparkle-Info.plist`, named by a `SRCROOT`-relative `INFOPLIST_FILE`
- AddInfoPlist on a `.` folder writes `<Target>-Info.plist` into the resources phase and copies root `INFOPLIST_KEY_*` rows onto target
- Asset catalog and `.icon` JSON keep Apple's serializer form, `plutil -convert json -r` writes it, Biome excludes both
- Schemes have no command-line writer, Xcode's scheme editor alone writes one
- New apps join as `<Name>.xcodeproj`, Nx infers each from its `project.xcproj`
- PackageDescription has no application product, Info.plist, icon, or signing API

## [04]-[BUILD_SETTINGS]

Project and target settings override root `Xcode.xcconfig`:
- Project configurations' `file` names the root configuration's navigator reference, a `<PROJECT>/` path outside the synchronized folder
- Nx sync generator `build-settings` writes `Xcode.xcconfig` compiler and linker rows from `dump-package`
- `build-settings` writes `Xcode.xcconfig` and `Directory.Build.props` identity rows from root `package.json`
- `build-settings` writes the `dump-package` macOS floor to `Xcode.xcconfig`, SwiftLint, pixi platform, and mise for rustc, clang, CMake and maturin
- `registerAgents` in `CurrentProcess` re-registers every plist in `Contents/Library/LaunchAgents` on each launch, launchd otherwise keeps the job and environment of the version first registered
- Imageset SVGs are Git symlinks, `<Mark>Symbol` into `AppIcon.icon/Assets` and a third-party mark into a catalog package under `node_modules`, each master root holds the 16-point size `Image` reads, `actool` refuses a symlinked `.icon` layer
- Automatic signing under the root team gives an Apple Development designated requirement keeping TCC grants, ad hoc signing prompts again
- Products needing a TCC grant are `application` bundles at `/Applications`, `LSBackgroundOnly` when they show no interface
- Xcode `install` replaces `/Applications/<Name>.app` from the zip `pack` writes through `ditto`
- Xcode registers each macOS app it builds with LaunchServices and unregisters it on `clean`, a bundle id lookup resolves the copy `install` registers
- Interface apps add `MainActor` default isolation, command-line tools `CREATE_INFOPLIST_SECTION_IN_BINARY` for their Info.plist keys
- Apple Events senders add `AUTOMATION_APPLE_EVENTS`, Xcode signs the `com.apple.security.automation.apple-events` entitlement from it
- Settings every package target needs go on `build` and `install` commands, project build settings reach no package target
- `xcodebuild` builds pass `-skipMacroValidation`, without it a package macro stops the build on Xcode's per-version approval
- `Xcode.xcconfig` holds `MARKETING_VERSION = 0.0.0` and `CURRENT_PROJECT_VERSION = 0`, `build` and `pack` override them with MinVer's core and the first-parent count
- Root `LD_RUNPATH_SEARCH_PATHS` holds `@executable_path/../Frameworks` for apps embedding a package framework, the app template row `target add` omits
- CI-only settings join `build` configuration `ci`, which keeps the index store `lint:periphery` reads
- Each project holds one derived data folder under `.cache/xcode/<root>`, `archive` writes `ArchiveIntermediates` and no index store beside the `build` state in it
- `COMPILATION_CACHE_ENABLE_CACHING` stays unset, derived data already holds incremental work and the store filled half of `.cache/xcode` with results a rebuild alone reuses
- Release builds `dwarf-with-dsym`, `pack` zips the archive's dSYMs and `publish` uploads them to the versioned release alone
- `xcodebuild` walks every folder under the package root except `.git` on each project load and ignores dot prefixes, the hidden flag, and `.gitignore`

## [05]-[COMPILER]

Toolchain upgrades edit the `Package.swift` tools version and `settings`, `build-settings` writes `Xcode.xcconfig` from them:
- `SWIFT_VERSION` derives from `dump-package` tools version major as `<major>.0`
- Upcoming features `xcrun swiftc -print-supported-features` lists above the language mode are on by name
- Mode moves delete each feature row the new mode implies
- `build-settings` writes every feature into one `OTHER_SWIFT_FLAGS` row as `-enable-upcoming-feature <Name>`
- `OTHER_SWIFT_FLAGS` and `OTHER_LDFLAGS` take no `$(inherited)`, root file is the lowest configurable level
- Warnings as errors covers on-by-default groups, `StrictLanguageFeatures` joins them to fail a misspelled feature the compiler accepts silently
- `PerformanceHints` groups are off, `UntypedThrows` fails in `@Observable` expansions and `ExistentialType` on a `Codable` witness's `any Decoder`
- Strict memory safety takes an `unsafe` marker on each expression using an API with no safe form
- `-warn-long-expression-type-checking-scopes 200` covers test targets

## [06]-[LOCKS]

- Apps resolve through their committed `xcshareddata/swiftpm/Package.resolved`, Xcode reads no local package lock
- `upgrade:xcode` copies root `Package.resolved` into each app, Xcode keeps root pins and prunes packages no app product reaches
- `xcodebuild` and `xcode-build-server config` pass `-project` and `-scheme`, `-derivedDataPath` holds `SourcePackages` and needs the scheme on a resolve
- `-disableAutomaticPackageResolution` fails a stale lock on fresh derived data alone

## [07]-[SWIFT_FORMAT]

`.swift-format` owns Swift layout and indentation, `.editorconfig` holds no `[*.swift]` section:
- `rules` lists every `dump-configuration` rule except comment and documentation rules and those SwiftLint reports at every site
- Rules `rules` omits are off, toolchain upgrades judge each rule `dump-configuration` adds
- Unknown top-level keys load with no message, an unknown rule name fails `lint --strict`
- Nested option objects decode only with every key present
- `multilineTrailingCommaBehavior: alwaysUsed` needs `lineBreakBeforeEachArgument`, without it a wrapped call ends in `,)`

## [08]-[SWIFTLINT]

`only_rules` lists every enabled rule, a rule a release adds is off until an upgrade judges its id in `swiftlint rules`:
- `included` is unset, when present SwiftLint lints its paths in place of a directory a command passes
- Layout rules are off, `format:swift-format` runs after `format:swiftlint` and removes the sole `return` `direct_return` writes
- Rules another owner reports first are off: compiler diagnostics, Periphery's unused parameters, `array_init` sites of `typesafe_array_init`
- Size and complexity rules are off, as are rules forcing a form file organization rejects
- `explicit_type_interface` is on, `redundant_type_annotation` and `explicit_acl` are off
- `no_empty_block` is off for required empty witnesses
- `discouraged_optional_boolean` and `discouraged_optional_collection` are off for wire fields where absence differs from `false` or empty
- Rules with an open fix or false-positive defect are off until a release fixes it (`trailing_closure`, `void_function_in_ternary`)
- `redundant_self` keeps its closures-only default, its method-body mode reports the `self` an `@autoclosure` argument (`Logger` message) requires
- `nesting` caps declaration depth, an ast-grep rule caps closure and control depth
- `custom_rules` is unset, its regexes match text where ast-grep matches syntax
- `analyzer_rules` is unset, `swiftlint analyze` needs a finished build and `xcodebuild -showBuildSettingsForIndex` per app and reads no package source, neither `xcodebuild` nor `debug.yaml` lists a package's compiler arguments

## [09]-[PERIPHERY]

- `.config/periphery.yml` names a generic config, `--project` needs a `.pbxproj` no app holds, SwiftPM mode runs `swift package describe` under the scratch path lock
- `rasm:lint:periphery` reads the one index store `typecheck:swift` writes, each test target's C99 name passes to `--exclude-targets`, a per-module scan rereads the same store
- `index_exclude` adds `.cache/**`, its defaults name `SourcePackages/checkouts` alone and SwiftPM checkouts then report as unused code
- Each app scans its own derived data store under `--report-include`, that store holds every package module the app links
- `retain_unused_imported_modules` names `CurrentProcess`, the marker misses a reference to an initializer it declares on `Logger` and `MemberImportVisibility` requires the import
- Bodyless `@_silgen_name` declarations name parameters `_:`, Periphery reports each named one unused

## [10]-[LANGUAGE_SERVER]

sourcekit-lsp from the selected toolchain reads the root package through SwiftPM and each app's compiler arguments through xcode-build-server:
- Xcode `build` runs uncached, its `xcode-build-server config` step writes the ignored `buildServer.json` beside each `.xcodeproj`
- Xcode's build writes the index store, xcode-build-server declares no `prepareProvider` for background indexing
- Client options hold sourcekit-lsp configuration, sourcekit-lsp asks to trust a workspace holding `.sourcekit-lsp/` or `.bsp/`
