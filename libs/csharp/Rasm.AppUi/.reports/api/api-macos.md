# [RASM_APPUI_API_MACOS]

`Microsoft.macOS` supplies macOS AppKit bindings needed for host-aware window, screen, and native handle evidence.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Microsoft.macOS`
- host_assembly: `Microsoft.macOS`
- assembly: `Microsoft.macOS`
- namespace: `AppKit`
- asset: host assembly
- rail: host-macos

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: macOS family
- rail: host-macos

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE] | [CAPABILITY]                |
| :-----: | :--------------------- | :------------- | :-------------------------- |
|   [1]   | `AppKit.NSApplication` | rail contract  | anchors host-macos contract |
|   [2]   | `AppKit.NSWindow`      | UI surface     | renders product surface     |
|   [3]   | `AppKit.NSView`        | UI surface     | renders product surface     |
|   [4]   | `AppKit.NSScreen`      | rail contract  | anchors host-macos contract |
|   [5]   | `Foundation.NSObject`  | rail contract  | anchors host-macos contract |
|   [6]   | `CoreGraphics.CGRect`  | rail contract  | anchors host-macos contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: macOS operations
- rail: host-macos

| [INDEX] | [SURFACE]            | [CALL_SHAPE]     | [CAPABILITY]               |
| :-----: | :------------------- | :--------------- | :------------------------- |
|   [1]   | `InvokeOnMainThread` | member surface   | drives host-macos behavior |
|   [2]   | `MainWindow`         | property surface | binds surface state        |
|   [3]   | `Frame`              | property surface | binds surface state        |
|   [4]   | `BackingScaleFactor` | member surface   | drives host-macos behavior |
|   [5]   | `VisibleFrame`       | member surface   | drives host-macos behavior |
|   [6]   | `Dispose`            | operation call   | executes operation         |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Microsoft.macOS`
- Owns: macOS native interop
- Accept: native evidence stays adapter-local
- Reject: AppKit as product shell

