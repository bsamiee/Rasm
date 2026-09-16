# [SWIFT]

Xcode project files hold every build decision, `.swift-format` at the root holds lint and format rules.

## [01]-[PROJECT_FILES]

- `objectVersion` at the newest value Xcode writes takes a `PBXFileSystemSynchronizedRootGroup` with no source file list
- Synchronized group's exceptions name the files outside the build
- Build setting default comes from the spec `DefaultValue` under Xcode's `XCBSpecifications.ideplugin`
- `xcodebuild -showBuildSettings` prints each setting's resolved value
- Rows restating a spec default go, rows a template writes over the default stay with their reason
- `LastUpgradeCheck`, `LastSwiftUpdateCheck`, and `BuildIndependentTargetsInParallel` are Xcode's own rows at the installed version
- One shared scheme per project sits under `xcshareddata`, `xcuserdata/` stays ignored
- `-disableAutomaticPackageResolution` on a build enforces `Package.resolved`

## [02]-[FORMAT]

- `xcrun swift-format dump-configuration` prints the defaults, a rule row set to its default goes
- Rule rows state one lint decision
