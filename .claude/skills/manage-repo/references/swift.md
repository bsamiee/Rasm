# [SWIFT]

Xcode projects inherit root `Xcode.xcconfig`, SwiftPM packages build libraries, root checker files serve both.

## [01]-[PROJECT_FILES]

`project.xcproj` uses Xcode's hierarchical JSON format, use Apple's Schema for keys and defaults:
- Convert existing projects through Xcode's Project Format inspector or `xcodebuild -convert-project 'Xcode Project'`
- Preserve Xcode's compact printing, array order, and trailing commas
- Target and product IDs stay stable across edits, schemes and external projects reference them
- Other relationships use unique names or name paths, IDs resolve ambiguity
- Project `configurations` define names once, target `specialized-configurations` appear only for configuration files or IDs
- `build-settings` holds settings shared across configurations, conditional keys select configuration differences
- `last-upgrade` and `last-swift-update` hold marketing versions, scheme `LastUpgradeVersion` holds Xcode's version code
- `files` holds navigator references, each target's `product` names a product reference under `Products`
- `kind: "folder"` at `path: "."` discovers sources through `target-membership`, files join existing build phases by type
- Apps with assets alone take a `resources` phase compiling assets and bundling resources, with `membership-exceptions` excluding non-sources
- Each project holds one shared scheme in `xcshareddata/xcschemes`, Xcode writes a new project's schemes under ignored `xcuserdata/` alone
- Shared schemes hold a `BuildAction` and a `LaunchAction` naming the target, Xcode autocreates a duplicate of a scheme with no `LaunchAction`
- `LaunchAction` sets `customLLDBInitFile` to `.config/.lldbinit` by a `$(SRCROOT)`-relative path
- App icons are an `AppIcon.icon` document `actool` renders into the bundle at build, no rendered image is checked in
- `actool` resolves `Assets.xcassets` and `AppIcon.icon` by name through the folder reference
- `IDETemplateMacros.plist` stays out, an empty `FILEHEADER` leaves each Swift template's `//` line and no repository-wide location exists

## [02]-[ROOT_CONFIGURATION]

Project and target settings override root `Xcode.xcconfig`:
- Defaults are the `DefaultValue` rows of the `.xcspec` files under Xcode's `SwiftBuild.framework`
- `xcodebuild -showBuildSettings -json` resolves effective settings through every inheritance level
- Each project configuration's `file` names the root configuration's navigator reference
- Root configuration's file reference uses a `<PROJECT>/`-relative path outside the synchronized source folder
- `-xcconfig` and `XCODE_XCCONFIG_FILE` stay out, each overrides every level and never reaches the IDE
- `PRODUCT_NAME = $(TARGET_NAME)` stays, the spec default is empty
- `ALWAYS_SEARCH_USER_PATHS = NO` stays, the `YES` default prints a header map warning in Swift-only targets
- Dead code stripping and user script sandboxing stay, Xcode's recommended-settings check proposes both again
- Release builds write a dSYM, crash reports from an installed product hold no symbols otherwise
- `MACOSX_DEPLOYMENT_TARGET` holds the minimum macOS supported by every product
- `ARCHS_STANDARD` at the deployment target is arm64 alone, project targets take no `ARCHS` row
- `-destination platform=macOS` takes no arch, a missing destination matches Any Mac and warns
- `ENABLE_DEBUG_DYLIB = NO` stays, the debug dylib and stub executor serve SwiftUI previews no product holds
- Automatic signing with the root team resolves an unset `CODE_SIGN_IDENTITY` to Apple Development
- Apple Development's designated requirement keeps TCC grants (Apple Events) across rebuilds, an ad hoc signature prompts again
- TCC and `tccutil` address a bundle identifier LaunchServices registered, an executable outside a bundle gets one Automation row per path and no reset
- Products needing a TCC grant are `application` bundles installed at the default `INSTALL_PATH` (`/Applications`), `LSBackgroundOnly` when they show no interface
- Interface apps add `MainActor` default isolation, tools `CREATE_INFOPLIST_SECTION_IN_BINARY` for their Info.plist keys
- Interface apps sending no Apple Events add hardened runtime, Apple Events need an entitlement under it

## [03]-[COMPILER]

Toolchain upgrades re-derive compiler policy in root `Xcode.xcconfig` and each library manifest's `settings`:
- `SWIFT_VERSION` holds the highest mode both Xcode's build system and bundled compiler accept
- Language mode implies complete concurrency checking and features with matching or lower `enabled_in` versions
- Every upcoming feature `xcrun swiftc -print-supported-features` lists as `enabled_in` a mode above the language mode is on by name
- Mode moves delete each feature row the new mode implies
- `SWIFT_APPROACHABLE_CONCURRENCY` stays out, language mode and individual feature rows cover its features
- Features without a `SWIFT_UPCOMING_FEATURE_*` setting join `OTHER_SWIFT_FLAGS` as `-enable-upcoming-feature <Name>`
- `OTHER_SWIFT_FLAGS` takes no `$(inherited)`, root file is the lowest configurable level and the setting has no default
- Strict memory safety takes an `unsafe` marker on each expression using an API with no safe form
- Warnings as errors covers every on-by-default diagnostic group, `SWIFT_WARNINGS_AS_ERRORS_GROUPS` adds only off-by-default groups
- `-warn-soft-deprecated` joins `OTHER_SWIFT_FLAGS` and is each manifest's one `unsafeFlags` entry, a member deprecated at version 100000 passes silently otherwise
- `StrictLanguageFeatures` is on, the compiler otherwise ignores a misspelled feature name
- Performance hint groups stay off, `UntypedThrows` fails in `@Observable` expansions, `ExistentialType` on a `Codable` witness's `any Decoder`
- Enhanced security rows stay out, each is Clang-only or needs a hardened-process entitlement
- String catalog rows join with a second language

## [04]-[PACKAGES]

Remote packages join `packages` in each importing project:
- `kind: "remote"` declares a repository, target `package-product-members` connects imported products to build phases
- `version.up-to-next-major-version` sets a lower bound, `Package.resolved` owns the exact version
- Prereleases join through a bound naming one, a new major through a bound edit
- `-resolvePackageDependencies` honors `Package.resolved` and `SourcePackages/workspace-state.json`, `upgrade` deletes both before resolving
- `-packageCachePath` stays out, `xcodebuild` and `swift` share SwiftPM's per-user repository cache with packages outside the repository
- Settings every package target needs go on `build` and `install` commands, project build settings reach no package target
- Remote package warnings never fail a build
- `LM_SKIP_METADATA_EXTRACTION=YES` skips App Intents extraction in every Swift target, no product declares an intent
- `install` passes `ARCHS=arm64`, package targets otherwise build at their manifest's lower deployment target and add x86_64 in Release
- `install` builds in its own derived data folder, `-clonedSourcePackagesDirPath` reads the package checkout under `build` derived data
- Root `.xcworkspace` stays out, a workspace owns a second `Package.resolved` while Nx builds each project alone

## [05]-[SWIFT_FORMAT]

swift-format comes from the selected toolchain through `xcrun`:
- `mise.toml` holds no swift-format row, `xcrun` selects the toolchain formatter
- `--version` prints the toolchain release, the `lint:swift-format` runtime input
- `.editorconfig` holds no `[*.swift]` section, `.swift-format` owns Swift indentation
- `rules` lists every enabled rule with default-valued rows from `dump-configuration`, an omitted rule is off
- Xcode upgrades judge each rule `dump-configuration` lists that `rules` omits
- Unknown keys and rule names load without a message, a misspelled row turns its rule off
- Nested option objects decode only with every key present
- Every installed rule runs except `AllPublicDeclarationsHaveDocumentation`, member docs stay optional on library API as on app code
- Every top-level key absent from `.swift-format` loosens or lengthens code when set
- `respectsExistingLineBreaks: false` collapses SwiftUI modifier chains, format keeps existing breaks
- Splits format keeps but does not write are joined by hand
- `multilineTrailingCommaBehavior: alwaysUsed` needs `lineBreakBeforeEachArgument`, without it a wrapped call ends in `,)`

## [06]-[SWIFTLINT]

- `only_rules` lists every enabled rule, each row a tightening where `disabled_rules` needs a relaxing row per unwanted rule
- Rules a SwiftLint release adds stay off until listed, an upgrade judges each new id `swiftlint rules` shows
- `included` stays unset, when present SwiftLint ignores the paths a command passes
- Layout rules stay off, swift-format owns layout and writes last
- Rules the compiler rejects under warnings as errors stay off (`duplicate_enum_cases`, `legacy_hashing`, `empty_parameters`)
- Size and complexity rules stay off, the repository sets no limit
- Rules forcing a form the file organization standard rejects stay off (kind order, one declaration per file, sorted enum cases)
- `explicit_type_interface` stays on and `redundant_type_annotation` off, the Swift form of the C# no-`var` policy
- `explicit_acl` stays off, Swift's default `internal` stays unwritten
- Rules with an open fix or false-positive defect stay off until a release fixes it (`trailing_closure`, `void_function_in_ternary`)
- `prefer_key_path` keeps `restrict_to_standard_functions` at its default, its fix on other functions breaks the call
- `nesting` at `type_level: 3` caps declaration depth, an ast-grep rule caps closure and control depth
- `custom_rules` stays empty, ast-grep rules under `tools/ast-grep/rules/swift` match structure by syntax where SwiftLint matches text by regex
- `analyzer_rules` run in `lint:swiftlint` over the compilation database `xcodebuild -showBuildSettingsForIndex -json` prints
- `swiftlint analyze` runs no `--fix`, `unused_import`'s fix breaks builds under `MemberImportVisibility`
- SwiftPM libraries take no analyzer pass, `unused_import` over a `swift build -v` log through `--compiler-log-path` reports `System` that a `FilePath` literal needs under `MemberImportVisibility`
- `deployment_target` holds `MACOSX_DEPLOYMENT_TARGET`, SwiftLint reads no xcconfig

## [07]-[CI]

CI's macOS job runs `setup-xcode` first in shared setup, selecting the runner image's newest installed Xcode, betas included:
- Settings CI alone needs (unsigned build, `COMPILER_INDEX_STORE_ENABLE=NO`) join `build` configuration `ci`, workflows select it with `--configuration ci`
- Settings in `ci` and after `--` reach `build`'s xcodebuild alone
- `xcbeautify` reads a pipe alone and stays out
- SwiftPM and derived data caches stay out, a restore keyed on sources misses on each source change

## [08]-[SWIFTPM]

SwiftPM joins with the first Swift library:
- Every `swift package` run writes a build directory, `--help` included
- `build` and `upgrade` pass `--scratch-path` under `.cache/swiftpm/build/<projectRoot>`, SwiftPM writes `<package>/.build` otherwise
- `SWIFTPM_BUILD_DIR` stays out, it overrides every `--scratch-path` and sends sourcekit-lsp preparation builds away from its index workspace
- Apps stay `.xcodeproj`, PackageDescription has no application product, Info.plist, icon, or signing API
- Library packages take the newest tools version the installed toolchain accepts, SwiftPM's package identity is the folder name
- Tools version 6 implies language mode 6, manifests hold no `swiftLanguageModes`
- Library targets take `path: "."` in place of SwiftPM's `Sources/` default
- Library targets exclude their test target folders, sources overlap otherwise
- One `let settings: [SwiftSetting]` per manifest applies to every target, no mechanism shares settings across manifests
- Settings state compiler policy in typed forms (`.strictMemorySafety()`, `.enableUpcomingFeature`, `.treatAllWarnings`)
- Sibling packages reference each other by `.package(path:)` with no lock entry, each keeps its own settings under a sibling's build
- Apps reference a library as a `packages` row with `kind: "local"` and project-relative `path`, `package-product-members` name it by folder
- `upgrade` runs `swift package update`, `Package.resolved` appears at the package root with the first remote dependency
- SwiftLint and swift-format plugins stay out, each adds a remote dependency and a second tool version beside the root one

## [09]-[NEW_PROJECT]

New Swift projects join as `<Name>.xcodeproj`, root files serve every other item of a language join:
- `tools/ast-grep/rules/swift` and `tools/ast-grep/outline/swift.yml` cover every Swift file
- Template Clang and Metal rows and the `DEBUG` condition have no reader in a Swift product
- README changes with a new owner or project kind alone, `.xcodeproj` is a listed project file
- `nx run rasm:format` formats a new project's files, root writers own Swift formatting

## [10]-[LANGUAGE_SERVER]

sourcekit-lsp from the selected toolchain reads each Xcode project's compiler arguments through xcode-build-server:
- Folders below the session root holding `buildServer.json`, `Package.swift`, or a compilation database alone open as sourcekit-lsp workspaces
- `swift` `.lsp.json` row takes no `workspaceFolder`, no setting opens a `.xcodeproj` as a workspace
- `xcode-build-server config` writes `buildServer.json` beside each `.xcodeproj` after `xcodebuild` in `build`, the one location it reads
- Each `build` refreshes the file arguments xcode-build-server parses from the newest derived data build log
- Xcode owns incremental compilation, Nx leaves builds uncached so build logs and indexes stay available to the language server
- Background indexing needs a build server with `prepareProvider`, xcode-build-server declares none and Xcode's build writes the index store
- `buildServer.json` holds absolute tool and derived data paths and stays ignored
- Parsed arguments and xcode-build-server's index database sit under `~/Library/Caches/xcode-build-server`, a folder no setting moves
- Index store and database paths come from xcode-build-server's `build/initialize` reply, sourcekit-lsp's `index` options hold no path
- `--scratch-path` in the `swift` `.lsp.json` row moves the SwiftPM index build from `<package>/.build/index-build` to `.cache/swiftpm/index`
- VS Code `swift.buildPath` passes `--scratch-path` to the Swift extension's sourcekit-lsp and its `swift package` runs on a focused package
- `swift.buildPath` names a path apart from the `.lsp.json` row's, SwiftPM locks a scratch path per process
- `.sourcekit-lsp/` and `.bsp/` stay out, sourcekit-lsp asks to trust a workspace holding either and Claude Code answers no server request
