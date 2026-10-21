# [SWIFT]

SwiftPM builds modules and executables through root `Package.swift`, Xcode builds apps through `.xcodeproj` files linking its products.

## [01]-[PACKAGE]

- `Package.swift` takes the newest tools version the toolchain accepts, tools version 6 implies language mode 6 and no `swiftLanguageModes` row
- Target paths hold Swift sources alone, a `Package.swift` inside one compiles as a source and any other file warns as unhandled
- Target paths are disjoint, SwiftPM fails a folder inside another target's path on overlapping sources
- Module names avoid SDK module names, a target named `Accessibility`, `System`, or `ApplicationServices` cycles through AppKit
- Products list library modules alone, SwiftPM synthesizes each executable's product
- Test targets take `<Module>Tests` and depend on their module, Nx attaches each to that module's `test` target
- Remote packages take `.package(url:from:)` in the manifest and `remote` `up-to-next-major-version` rows in a project, `upgrade:swift` moves both forms
- `settings` restates `Xcode.xcconfig` compiler policy in typed forms, `unsafeFlags` where no typed form exists, and every target's `linkerSettings` restates `OTHER_LDFLAGS`
- Checkers run from root tool rows, a SwiftLint or swift-format package plugin adds a second tool version

## [02]-[SWIFTPM]

- SwiftPM takes its scratch path from `SWIFTPM_BUILD_DIR` over `--scratch-path` and reads it from no file
- SwiftPM writes `.build/` in the working folder when neither is set, `--help` included
- `SWIFTPM_BUILD_DIR` sits on Nx SwiftPM targets alone, in `mise.toml` `[env]` it moves sourcekit-lsp's build off `swiftPM.scratchPath`
- SwiftPM locks its scratch path per process, each SwiftPM target takes `parallelism: false`

## [03]-[PROJECT_FILES]

`project.xcproj` uses Xcode's hierarchical JSON format under Apple's schema for keys and defaults:
- Edits keep target and product IDs, schemes and external projects reference them
- Project `configurations` define names once, target `specialized-configurations` appear only for configuration files or IDs
- `build-settings` holds settings shared across configurations, conditional keys select configuration differences
- `kind: "folder"` at `path: "."` discovers sources through `target-membership`, files join existing build phases by type
- Apps with assets take a `resources` phase with `membership-exceptions` excluding non-sources
- Projects hold one shared scheme in `xcshareddata/xcschemes`, its `BuildAction` and `LaunchAction` naming the target
- Xcode autocreates a duplicate of a shared scheme with no `LaunchAction`
- `LaunchAction` sets `customLLDBInitFile` to `.config/.lldbinit` by a `$(SRCROOT)`-relative path
- App icons are an `AppIcon.icon` document `actool` renders at build, no rendered image is checked in
- `packages` holds the root package as `{ "kind": "local", "path": "<parents to root>" }`, product rows name it `..`
- `xcprojformatter` drops `name` and `id` keys from a `packages` row
- `package-product-members` holds one row per module the app imports, Nx draws the app's module edges from those rows
- New apps join as `<Name>.xcodeproj`, Nx infers each from its `project.xcproj`
- PackageDescription has no application product, Info.plist, icon, or signing API

## [04]-[BUILD_SETTINGS]

Project and target settings override root `Xcode.xcconfig`:
- Project configurations' `file` names the root configuration's navigator reference, a `<PROJECT>/` path outside the synchronized folder
- `MACOSX_DEPLOYMENT_TARGET`, manifest `platforms`, and SwiftLint `deployment_target` hold one macOS version
- Automatic signing under the root team gives an Apple Development designated requirement keeping TCC grants, ad hoc signing prompts again
- Products needing a TCC grant are `application` bundles at `/Applications`, `LSBackgroundOnly` when they show no interface
- Interface apps add `MainActor` default isolation, command-line tools `CREATE_INFOPLIST_SECTION_IN_BINARY` for their Info.plist keys
- Apple Events senders add `AUTOMATION_APPLE_EVENTS`, Xcode signs the `com.apple.security.automation.apple-events` entitlement from it
- Settings every package target needs go on `build` and `install` commands, project build settings reach no package target
- CI-only settings join `build` configuration `ci`, which keeps the index store `lint:periphery` reads
- Xcode reads every non-hidden file under the package root on each load and ignores `.gitignore`
- Large trees no build reads take a dot prefix or the hidden flag (`chflags hidden <dir>`)

## [05]-[COMPILER]

Toolchain upgrades re-derive compiler policy in root `Xcode.xcconfig` and the manifest `settings`:
- `SWIFT_VERSION` holds the highest mode Xcode lists and the compiler accepts
- Upcoming features `xcrun swiftc -print-supported-features` lists above the language mode are on by name
- Mode moves delete each feature row the new mode implies
- Features without a `SWIFT_UPCOMING_FEATURE_*` setting join `OTHER_SWIFT_FLAGS` as `-enable-upcoming-feature <Name>`
- `OTHER_SWIFT_FLAGS` and `OTHER_LDFLAGS` take no `$(inherited)`, the root file is the lowest configurable level
- Warnings as errors covers on-by-default groups, `StrictLanguageFeatures` joins them to fail a misspelled feature the compiler accepts silently
- `PerformanceHints` groups are off, `UntypedThrows` fails in `@Observable` expansions and `ExistentialType` on a `Codable` witness's `any Decoder`
- Strict memory safety takes an `unsafe` marker on each expression using an API with no safe form

## [06]-[LOCKS]

- Xcode reads no lock of a local package and re-resolves a copied one, each app resolves through its committed `project.xcworkspace/xcshareddata/swiftpm/Package.resolved`
- Xcode commands (`xcodebuild`, `xcode-build-server config`) pass `-project`, a resolve under `-project` takes no scheme
- `-disableAutomaticPackageResolution` fails a stale lock on fresh derived data alone, `upgrade:xcode` deletes the lock and `workspace-state.json` before resolving

## [07]-[SWIFT_FORMAT]

`.swift-format` owns Swift layout and indentation, `.editorconfig` holds no `[*.swift]` section:
- `rules` lists every rule `dump-configuration` prints but `AllPublicDeclarationsHaveDocumentation`, code holds no doc comments, and rules a SwiftLint rule reports at every site
- Rules `rules` omits are off, toolchain upgrades judge each rule `dump-configuration` adds
- Unknown top-level keys load with no message, an unknown rule name fails `lint --strict`
- Nested option objects decode only with every key present
- `multilineTrailingCommaBehavior: alwaysUsed` needs `lineBreakBeforeEachArgument`, without it a wrapped call ends in `,)`

## [08]-[SWIFTLINT]

`only_rules` lists every enabled rule, a rule a release adds is off until an upgrade judges its id in `swiftlint rules`:
- `included` is unset, when present SwiftLint lints its paths in place of a directory a command passes
- Layout rules are off, `format:swift-format` runs after `format:swiftlint` and removes the sole `return` `direct_return` writes
- Rules another owner reports first are off: compiler diagnostics, Periphery's unused parameters, `array_init` sites of `typesafe_array_init`
- Size and complexity rules are off, and so are rules forcing a form the file organization standard rejects
- `explicit_type_interface` is on, `redundant_type_annotation` and `explicit_acl` are off
- `no_empty_block` is off for required empty witnesses
- `discouraged_optional_boolean` and `discouraged_optional_collection` are off for wire fields whose absence differs from `false` or empty
- Rules with an open fix or false-positive defect are off until a release fixes it (`trailing_closure`, `void_function_in_ternary`)
- `nesting` caps declaration depth, an ast-grep rule caps closure and control depth
- `custom_rules` is unset, its regexes match text where ast-grep matches syntax
- `swiftlint analyze` runs with no `--fix`, `unused_import`'s fix breaks builds under `MemberImportVisibility`

## [09]-[PERIPHERY]

- `.periphery.yml` names a generic configuration, `--project` rejects a `project.xcproj` with no `.pbxproj`
- Bodyless `@_silgen_name` declarations name parameters `_:`, Periphery reports each named one unused

## [10]-[LANGUAGE_SERVER]

sourcekit-lsp from the selected toolchain reads the root package through SwiftPM and each app's compiler arguments through xcode-build-server:
- Xcode `build` runs uncached, its `xcode-build-server config` step writes the ignored `buildServer.json` beside each `.xcodeproj`
- Xcode's build writes the index store, xcode-build-server declares no `prepareProvider` for background indexing
- Client options hold sourcekit-lsp configuration, sourcekit-lsp asks to trust a workspace holding `.sourcekit-lsp/` or `.bsp/`
