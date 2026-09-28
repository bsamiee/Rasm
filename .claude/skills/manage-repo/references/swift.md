# [SWIFT]

Xcode projects build every Swift product, one root build configuration and the root checker files serve all of them.

## [01]-[PROJECT_FILES]

Project files are edited by hand in the form Xcode's writer produces:
- `objectVersion` and `preferredProjectObjectVersion` hold 110, a `compatibilityVersion` row makes an Xcode 27 save rewrite the file at 71
- `LastUpgradeCheck`, `LastSwiftUpdateCheck`, and each scheme's `LastUpgradeVersion` hold the installed Xcode's 2700
- Every Xcode or `xcode` MCP save adds a self `projectReferences` entry, a `.xcodeproj` file reference, and a `Products` group, hand edits revert them
- Objects sit in one section per `isa`, sorted by id, while lists inside an object keep their written order
- Values holding `-` take quotes, a value opening with `.` stays bare although the `syntax-project-pbxproj` editor extension flags it
- `PBXFileSystemSynchronizedRootGroup` at `path = .` routes every project directory file into the target's existing phases with no source list
- Sources stay at `.`, the new-project template's sibling source folder gives a tool a one-file folder
- Apps name each non-source file (`CLAUDE.md`, `LICENSE`) in `membershipExceptions`, their Resources phase copies every unnamed file into the bundle
- Tools take no Resources phase and no exception set
- `.xcodeproj` takes no exception, it joins no target without one
- Structural keys a save writes back stay (`knownRegions`, `projectRoot`, an empty `buildRules`)
- Apps keep an empty Resources phase, their asset catalog compiles only through an existing phase
- Each project holds one shared scheme in `xcshareddata/xcschemes`, Xcode 27 writes a new project's schemes under ignored `xcuserdata/` alone
- Shared schemes hold a `BuildAction` and a `LaunchAction` naming the target, Xcode autocreates a duplicate of a scheme with no `LaunchAction`
- `LaunchAction` sets `customLLDBInitFile` to root `.lldbinit` by a `$(SRCROOT)`-relative path
- App icons are an `AppIcon.icon` document `actool` renders into the bundle at build, no rendered image is checked in
- `IDETemplateMacros.plist` stays out, an empty `FILEHEADER` leaves each Swift template's `//` line and no repository-wide location exists

## [02]-[ROOT_CONFIGURATION]

`Xcode.xcconfig` is each project's project-level base configuration, every project and target row overrides it:
- Defaults are the `DefaultValue` rows of the `.xcspec` files under Xcode's `SwiftBuild.framework`
- `xcodebuild -showBuildSettings -json` on a copy with its base configuration detached shows what each row changes
- Project-level Debug and Release configurations take it as `baseConfigurationReference` with empty `buildSettings`
- `PBXFileReference` with `sourceTree = SOURCE_ROOT` and a project-relative path is the one reference form
- `baseConfigurationReferenceAnchor` ignores a path outside the synchronized folder without a message
- `-xcconfig` and `XCODE_XCCONFIG_FILE` stay out, they override every level and never reach the IDE
- `PRODUCT_NAME = $(TARGET_NAME)` stays, the spec default is empty
- `ALWAYS_SEARCH_USER_PATHS = NO` stays, the `YES` default prints a header map warning in Swift-only targets
- Dead code stripping and user script sandboxing stay, Xcode's recommended-settings check proposes both again
- Release builds write a dSYM, crash reports from an installed product carry no symbols otherwise
- `MACOSX_DEPLOYMENT_TARGET` holds the oldest macOS the installed Xcode runs on (26.6 for Xcode 27), the 27.0 default launches on no macOS 26 host
- Below a 27.0 deployment target `ARCHS_STANDARD` includes x86_64, `ARCHS = arm64` narrows it until the floor reaches 27.0
- `-destination platform=macOS` takes no arch under `ARCHS`, a missing destination also matches Any Mac and warns
- Automatic signing with the root team resolves an unset `CODE_SIGN_IDENTITY` to Apple Development
- Apple Development's designated requirement keeps TCC grants (a tool's Apple Events) across rebuilds, an ad hoc signature prompts again
- Apps add hardened runtime and `MainActor` default isolation, tools `CREATE_INFOPLIST_SECTION_IN_BINARY` for their Info.plist keys
- Tools take no hardened runtime, Apple Events need an entitlement under it

## [03]-[COMPILER]

Each toolchain upgrade re-derives the strictest compiler policy the installed Swift accepts in `Xcode.xcconfig`:
- `SWIFT_VERSION` holds the highest mode `swiftc -swift-version` accepts
- Swift 6 mode implies complete concurrency checking and every feature enabled in 6, none takes a row
- Every upcoming feature `xcrun swiftc -print-supported-features` lists as `enabled_in` a mode above the language mode is on by name
- Mode moves delete each feature row the new mode implies
- `SWIFT_APPROACHABLE_CONCURRENCY` stays out, its Swift 6 features are on by name
- Features without a `SWIFT_UPCOMING_FEATURE_*` setting (`ImmutableWeakCaptures`) join `OTHER_SWIFT_FLAGS` as `-enable-upcoming-feature <Name>`
- `OTHER_SWIFT_FLAGS` takes no `$(inherited)`, root file is the lowest configurable level and the setting has no default
- Strict memory safety takes an `unsafe` marker on each expression using an API with no safe form
- Warnings as errors covers every on-by-default diagnostic group, `SWIFT_WARNINGS_AS_ERRORS_GROUPS` adds only off-by-default groups
- Off-by-default groups stay off, `UntypedThrows` fails inside Apple's `@Observable` expansion
- Enhanced security rows stay out, each is Clang-only or needs a hardened-process entitlement
- Compilation caching stays out, Nx caches `build`
- String catalog rows join with a second language

## [04]-[PACKAGES]

Remote packages join as an `XCRemoteSwiftPackageReference` of the project that imports them:
- Remote packages take `upToNextMajorVersion`, the one requirement kind that leaves the pin to `Package.resolved`
- Prereleases join through a bound naming one, a new major through a `minimumVersion` edit
- `-resolvePackageDependencies` honors `Package.resolved` and `SourcePackages/workspace-state.json`, `upgrade` deletes both before resolving
- `-packageCachePath` takes an absolute `.cache/swiftpm/cache`, a relative path resolves nothing and leaves `Package.resolved` deleted
- Project build settings reach no package target, a remote package's warnings never fail a build
- Root `.xcworkspace` stays out, a workspace owns a second `Package.resolved` while Nx builds each project alone

## [05]-[SWIFT_FORMAT]

swift-format comes from the selected Xcode through `xcrun`:
- Mise holds no swift-format row, mise has no swift-format entry and a mise `swift` puts a second binary on `PATH`
- `--version` prints `main`, the `xcodebuild -version` input identifies the binary
- `.editorconfig` holds no `[*.swift]` section, `.swift-format` owns Swift indentation
- `rules` lists every enabled rule with default-valued rows from `dump-configuration`, an omitted rule is off
- Xcode upgrades judge each rule `dump-configuration` lists that `rules` omits
- Unknown keys and rule names load without a message, a misspelled row turns its rule off
- Nested option objects decode only with every key present
- Every installed rule runs except `AllPublicDeclarationsHaveDocumentation`, member docs stay optional and no declaration is public
- Every top-level key absent from `.swift-format` loosens or lengthens code when set
- `respectsExistingLineBreaks: false` collapses SwiftUI modifier chains, format keeps existing breaks
- Splits format keeps but does not write are joined by hand
- `multilineTrailingCommaBehavior: alwaysUsed` needs `lineBreakBeforeEachArgument`, without it a wrapped call ends in `,)`

## [06]-[SWIFTLINT]

SwiftLint rules join when no other checker covers their form:
- `only_rules` lists every enabled rule, each row a tightening where `disabled_rules` needs a relaxing row per unwanted rule
- Rules a SwiftLint release adds stay off until listed, an upgrade judges each new id `swiftlint rules` shows
- `included` stays unset, when present SwiftLint ignores the paths a command passes
- Layout rules and duplicates of a swift-format rule stay off, swift-format owns layout and writes last
- Rules the compiler rejects under warnings as errors stay off (`duplicate_enum_cases`, `legacy_hashing`, `empty_parameters`)
- Size and complexity rules stay off, the repository sets no limit
- Rules forcing a form the file organization standard rejects stay off (kind order, one declaration per file, sorted enum cases)
- `explicit_type_interface` stays on and `redundant_type_annotation` off, the Swift form of the C# no-`var` policy
- `explicit_acl` stays off, Swift's default `internal` stays unwritten
- Rules with an open fix or false-positive defect stay off until a release fixes it (`trailing_closure`, `void_function_in_ternary`)
- `prefer_key_path` keeps `restrict_to_standard_functions` at its default, its fix on other functions breaks the call
- `nesting` at `type_level: 3` caps declaration depth, an ast-grep rule caps closure and control depth
- `custom_rules` stays empty, ast-grep rules under `tools/ast-grep/rules/swift` match structure by syntax where SwiftLint matches text by regex
- `swiftlint analyze` has no target, `unused_import`'s fix breaks builds under `MemberImportVisibility`

## [07]-[CI]

CI's macOS job runs on the Xcode local builds use:
- `runs-on: xcode-27` selects GitHub's preview image with default Xcode 27, `macos-26` includes Xcode 26 alone
- `runs-on` label alone selects Xcode, `DEVELOPER_DIR` names an image-only path and no build tool reads `.xcode-version`
- Settings after `--` reach `build` alone, `lint` takes no forwarded arguments
- `COMPILER_INDEX_STORE_ENABLE=NO` joins the forwarded settings, nothing in CI reads the index
- Swift takes no format-and-diff step, `swift-format lint --strict` and `swiftlint lint` fail on every change `format` writes
- `xcbeautify` stays out, it needs a pipe
- SwiftPM and derived data caches stay out, a restore keyed on sources misses on each source change

## [08]-[SWIFTPM]

SwiftPM joins with the first Swift library, until then the `mise.toml` `SWIFTPM_BUILD_DIR` row is its one fact:
- Every `swift package` run writes a scratch directory, `--help` included, the row keeps `.build` out of the tree
- `SWIFTPM_BUILD_DIR` overrides `--scratch-path`, a package target sets `SWIFTPM_BUILD_DIR={workspaceRoot}/.cache/swiftpm/{projectRoot}` in its `env`
- Package targets pass `--cache-path $NX_WORKSPACE_ROOT/.cache/swiftpm/cache`, the clone cache `xcodebuild` uses
- Apps stay `.xcodeproj`, PackageDescription has no application product, Info.plist, icon, or signing API
- Libraries take `libs/swift/<Name>/Package.swift` at the newest tools version the installed toolchain accepts
- Tools version 6 implies language mode 6, manifests hold no `swiftLanguageModes`
- Library targets take `path: "."` with domain folders, `Sources/` is SwiftPM's `src/`
- Library targets exclude their test target folders, sources overlap otherwise
- One `let settings: [SwiftSetting]` per manifest applies to every target, no mechanism shares settings across manifests
- Settings state the compiler policy in typed forms (`.strictMemorySafety()`, `.enableUpcomingFeature`), `unsafeFlags` stays out
- Sibling packages reference each other by `.package(path:)` with no lock entry, each keeps its own settings under a sibling's build
- Apps reference a library by `XCLocalSwiftPackageReference` with `relativePath`
- `upgrade` runs `swift package update`, `Package.resolved` appears at the package root with the first remote dependency
- SwiftLint and swift-format plugins stay out, each adds a remote dependency and a second tool version beside the root one
- `workspace.ts` gains a `Package.swift` kind tagged `language:swift` alone, xcodebuild `build` and `install` bodies move to `tag:host:macos`

## [09]-[NEW_PROJECT]

New Swift projects join as `<Name>.xcodeproj` with its shared scheme, root files serve every other item of a language join:
- `workspace.ts` infers tags and targets from `project.pbxproj`, `nx.json` bodies apply by tag, CI runs `host:macos` projects in the macOS job
- Swift is built into ast-grep, `tools/ast-grep/rules/swift` and `tools/ast-grep/outline/swift.yml` cover every Swift file with no `sgconfig.yml` row
- Template projects drop each row a root file or default covers and each row nothing reads (Clang and Metal rows, `DEBUG` condition)
- README changes with a new owner or project kind alone, `.xcodeproj` is a listed project file
- `nx run <Name>:format` precedes the first `check`
