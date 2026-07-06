# [API_MACOS_NATIVE]

Catalog scope: the macOS-native seam behind motion pacing and high-fidelity input — display-link vsync, reduce-motion accessibility, and screen-parameter observation. Every member is platform-gated; Windows paths pace on `UITimer`/idle.

[NAMESPACES]:
- `AppKit` — `NSWorkspace.AccessibilityDisplayShouldReduceMotion`, `NSView` (`GetDisplayLink`, `Window.Screen`), `NSScreen` (`MaximumFramesPerSecond`, `MaximumRefreshInterval`), `NSApplication.Notifications.ObserveDidChangeScreenParameters`.
- `CoreAnimation` — `CADisplayLink` (`PreferredFrameRateRange`, `Paused`, `AddToRunLoop`, `Invalidate`, `TargetTimestamp`), `CAFrameRateRange.Create`.
- `Foundation` / `ObjCRuntime` — `NSObject`, `Selector`, `Runtime.GetNSObject<T>`, `NSRunLoop`/`NSRunLoopMode`.
