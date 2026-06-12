# [RASM_APPUI_API_MACOS]

`Microsoft.macOS` supplies macOS AppKit, Foundation, CoreGraphics, and ObjCRuntime bindings needed for host-aware window, screen, native handle, and main-thread evidence.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Microsoft.macOS`
- package: `Microsoft.macOS`
- assembly: `Microsoft.macOS`
- namespace: `AppKit`
- namespace: `Foundation`
- namespace: `CoreGraphics`
- namespace: `ObjCRuntime`
- asset: host assembly
- rail: host-macos

## [2]-[PUBLIC_TYPES]

[APPKIT_TYPES]: application, window, view, and screen surface
- rail: host-macos

| [INDEX] | [SYMBOL]        | [RAIL]           |
| :-----: | :-------------- | :--------------- |
|   [1]   | `NSApplication` | application root |
|   [2]   | `NSWindow`      | native window    |
|   [3]   | `NSView`        | native view      |
|   [4]   | `NSScreen`      | screen state     |
|   [5]   | `NSResponder`   | responder chain  |
|   [6]   | `NSMenu`        | native menu      |

[FOUNDATION_TYPES]: object, handle, and geometry surface
- rail: host-macos

| [INDEX] | [SYMBOL]       | [RAIL]          |
| :-----: | :------------- | :-------------- |
|   [1]   | `NSObject`     | native object   |
|   [2]   | `NSString`     | native string   |
|   [3]   | `NSArray`      | native array    |
|   [4]   | `NSDictionary` | native map      |
|   [5]   | `CGRect`       | rectangle value |
|   [6]   | `CGSize`       | size value      |
|   [7]   | `CGPoint`      | point value     |
|   [8]   | `NativeHandle` | native handle   |

## [3]-[ENTRYPOINTS]

[APPKIT_ENTRYPOINTS]: window and main-thread operations
- rail: host-macos

| [INDEX] | [SURFACE]            | [SURFACE_ROOT]  | [RAIL]        |
| :-----: | :------------------- | :-------------- | :------------ |
|   [1]   | `SharedApplication`  | `NSApplication` | app lookup    |
|   [2]   | `InvokeOnMainThread` | `NSObject`      | main dispatch |
|   [3]   | `MainWindow`         | `NSApplication` | active window |
|   [4]   | `KeyWindow`          | `NSApplication` | key window    |
|   [5]   | `Frame`              | `NSWindow`      | window bounds |
|   [6]   | `ContentView`        | `NSWindow`      | content view  |
|   [7]   | `VisibleFrame`       | `NSScreen`      | screen bounds |
|   [8]   | `BackingScaleFactor` | `NSScreen`      | scale factor  |

[NATIVE_ENTRYPOINTS]: handles and lifecycle operations
- rail: host-macos

| [INDEX] | [SURFACE]                 | [SURFACE_ROOT] | [RAIL]         |
| :-----: | :------------------------ | :------------- | :------------- |
|   [1]   | `Handle`                  | `NSObject`     | native handle  |
|   [2]   | `DangerousRetain`         | `NSObject`     | retain handle  |
|   [3]   | `DangerousRelease`        | `NSObject`     | release handle |
|   [4]   | `Dispose`                 | `NSObject`     | native release |
|   [5]   | `BeginInvokeOnMainThread` | `NSObject`     | async dispatch |

## [4]-[IMPLEMENTATION_LAW]

[HOST_MACOS_LAW]:
- Package: `Microsoft.macOS`
- Owns: macOS native interop for window handles, screen metrics, main-thread dispatch, and native object lifecycle
- Accept: native evidence stays adapter-local and feeds AppUi receipts
- Reject: AppKit as product shell

[BOUNDARY_LAW]:
- Package: `Microsoft.macOS`
- Owns: macOS-specific host support only
- Accept: AppUi product shells remain Avalonia-first and enter AppKit only through host adapters
- Reject: AppKit as a parallel UI engine
