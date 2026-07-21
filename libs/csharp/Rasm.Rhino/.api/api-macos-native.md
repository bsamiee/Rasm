# [RASM_RHINO_API_MACOS_NATIVE]

`Microsoft.macOS` is the .NET AppKit/CoreAnimation binding the Rhino host loads under the macOS target, and this catalog owns the native seam behind high-fidelity motion pacing and display observation. `NSScreen` is the display-link factory and carries the panel's `MaximumFramesPerSecond`, `MaximumRefreshInterval`, and `BackingScaleFactor`; `CADisplayLink` is the vsync-locked clock whose `TargetTimestamp` and `PreferredFrameRateRange` drive per-frame animation, attached to and detached from an `NSRunLoop`. `NSWorkspace` projects the accessibility display state — reduce-motion, increased-contrast, differentiate-without-color, reduce-transparency — that gates whether an animation runs at all; `NSApplication.Notifications` observes screen-parameter changes when a display is added, removed, or reconfigured. `Foundation.NSRunLoop` is the loop a display link or timer attaches to; `ObjCRuntime.Runtime` owns the managed-to-native object bridge. Every member is platform-gated to the macOS target — a non-macOS host paces on the `UITimer`/idle clock (`api-eto-runtime`), never on this surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.macOS`
- package: `Microsoft.macOS` — the .NET macOS platform bindings, host-provided under the macOS TFM, not a central `PackageReference`
- assembly: `Microsoft.macOS`
- namespace: `AppKit`, `CoreAnimation`, `Foundation`, `ObjCRuntime`
- asset: the `Microsoft.macOS` binding assembly the Rhino host loads on the `net10.0-macos` target; the whole surface is absent off-macOS
- verify: `tools.assay api query <symbol> --key microsoft-macos` decompiles the bundled `/Applications/RhinoWIP.app/Contents/Frameworks/RhCore.framework/Versions/A/Resources/Microsoft.macOS.dll` directly; `--key eto-macos` covers the sibling `Eto.macOS.dll`
- rail: macos-native

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: display and refresh state
- namespace: `AppKit`
- rail: macos-native

`NSScreen` is the display: it owns the display-link factory and the panel's frame-rate, refresh-interval, and backing-scale facts. `NSView.Window` is the verified relation from a view to its `NSWindow`, and thus to the `NSScreen` it occupies.

| [INDEX] | [SYMBOL]      | [KIND]         | [CAPABILITY]                                                           |
| :-----: | :------------ | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `NSScreen`    | display        | display-link factory, `MaximumFramesPerSecond`, refresh, backing scale |
|  [02]   | `NSView`      | view           | `Window` relation to the owning `NSWindow` and its screen              |
|  [03]   | `NSWorkspace` | shared service | accessibility display state via `SharedWorkspace`                      |

[PUBLIC_TYPE_SCOPE]: motion clock and run loop
- namespace: `CoreAnimation`, `Foundation`
- rail: macos-native

`CADisplayLink` is the vsync clock and `CAFrameRateRange` its rate policy; both attach to an `NSRunLoop` under a run-loop mode.

| [INDEX] | [SYMBOL]           | [KIND]      | [CAPABILITY]                                                |
| :-----: | :----------------- | :---------- | :---------------------------------------------------------- |
|  [01]   | `CADisplayLink`    | vsync clock | per-frame callback with `TargetTimestamp` and rate range    |
|  [02]   | `CAFrameRateRange` | rate policy | preferred/min/max frame-rate range for a display link       |
|  [03]   | `NSRunLoop`        | run loop    | the loop a display link or timer attaches to, keyed by mode |
|  [04]   | `NSRunLoopMode`    | mode key    | the run-loop mode under which an attachment fires           |

[PUBLIC_TYPE_SCOPE]: screen-parameter observation and object bridge
- namespace: `AppKit`, `ObjCRuntime`
- rail: macos-native

`NSApplication.Notifications` observes display reconfiguration; `Runtime` is the managed-to-native object bridge behind an `nint` handle.

| [INDEX] | [SYMBOL]                      | [KIND]        | [CAPABILITY]                                               |
| :-----: | :---------------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `NSApplication.Notifications` | notifier      | `ObserveDidChangeScreenParameters` display-reconfig hook   |
|  [02]   | `Runtime`                     | object bridge | typed `NSObject`/`INativeObject` resolution from an `nint` |
|  [03]   | `Selector`                    | selector      | the Objective-C message selector a display link targets    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: display state and accessibility gating
- rail: macos-native

`NSScreen.GetDisplayLink` is the display-link factory keyed by a target and selector; the frame-rate, refresh, and backing-scale members read the panel's capability. `NSWorkspace.SharedWorkspace` exposes the four `bool` accessibility gates read before motion runs — `AccessibilityDisplayShouldReduceMotion`, `AccessibilityDisplayShouldIncreaseContrast`, `AccessibilityDisplayShouldDifferentiateWithoutColor`, and `AccessibilityDisplayShouldReduceTransparency`.

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]                           | [CAPABILITY]                      |
| :-----: | :-------------------------------- | :------------------------------------- | :-------------------------------- |
|  [01]   | `NSScreen.GetDisplayLink`         | `(NSObject, Selector) → CADisplayLink` | build a display link for a screen |
|  [02]   | `NSScreen.MaximumFramesPerSecond` | `→ nint`                               | the panel's max frame rate        |
|  [03]   | `NSScreen.MaximumRefreshInterval` | `→ double`                             | the panel's max refresh interval  |
|  [04]   | `NSScreen.BackingScaleFactor`     | `→ nfloat`                             | the panel's device-pixel scale    |
|  [05]   | `NSView.Window`                   | `→ NSWindow`                           | the owning window, and its screen |
|  [06]   | `NSWorkspace.SharedWorkspace`     | `→ NSWorkspace`                        | the shared workspace singleton    |

[ENTRYPOINT_SCOPE]: display-link lifecycle and run-loop attachment
- rail: macos-native

A display link is created against a screen, configured with a `PreferredFrameRateRange`, attached to a run loop under a mode, read per frame through `TargetTimestamp`, and torn down through `RemoveFromRunLoop`/`Invalidate`.

| [INDEX] | [SURFACE]                               | [CALL_SHAPE]                                     | [CAPABILITY]                       |
| :-----: | :-------------------------------------- | :----------------------------------------------- | :--------------------------------- |
|  [01]   | `CADisplayLink.Create`                  | `(NSObject, Selector) → CADisplayLink`           | build a display link               |
|  [02]   | `CADisplayLink.TargetTimestamp`         | `→ double`                                       | the next frame's presentation time |
|  [03]   | `CADisplayLink.Timestamp`               | `→ double`                                       | the current frame's timestamp      |
|  [04]   | `CADisplayLink.Duration`                | `→ double`                                       | the frame interval                 |
|  [05]   | `CADisplayLink.Paused`                  | `get/set → bool`                                 | pause and resume the callback      |
|  [06]   | `CADisplayLink.PreferredFrameRateRange` | `get/set → CAFrameRateRange`                     | the requested frame-rate range     |
|  [07]   | `CADisplayLink.AddToRunLoop`            | `(NSRunLoop, NSRunLoopMode)`                     | attach the link to a run loop      |
|  [08]   | `CADisplayLink.RemoveFromRunLoop`       | `(NSRunLoop, NSRunLoopMode)`                     | detach the link from a run loop    |
|  [09]   | `CADisplayLink.Invalidate`              | `()`                                             | permanently tear down the link     |
|  [10]   | `CAFrameRateRange.Create`               | `(float min, max, preferred) → CAFrameRateRange` | build a rate range                 |
|  [11]   | `CAFrameRateRange.Default`              | `→ CAFrameRateRange`                             | the unconstrained rate range       |

[ENTRYPOINT_SCOPE]: screen-parameter observation, run loop, and bridge
- rail: macos-native

`NSApplication.Notifications.ObserveDidChangeScreenParameters` fires on display reconfiguration; `NSRunLoop` exposes the current and main loops and timer attachment; `Runtime` resolves a native handle to a typed managed object.

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]                              | [CAPABILITY]                              |
| :-----: | :------------------------------------- | :---------------------------------------- | :---------------------------------------- |
|  [01]   | `ObserveDidChangeScreenParameters`     | `(EventHandler<NSNotificationEventArgs>)` | observe display reconfig, returns a token |
|  [02]   | `NSRunLoop.Current` / `NSRunLoop.Main` | `→ NSRunLoop`                             | the current and main run loops            |
|  [03]   | `NSRunLoop.AddTimer`                   | `(NSTimer, NSRunLoopMode)`                | attach a timer to the loop                |
|  [04]   | `NSRunLoop.Perform`                    | `(NSRunLoopMode[], Action)`               | schedule a block on the loop              |
|  [05]   | `NSRunLoop.WakeUp`                     | `()`                                      | wake a blocked run loop                   |
|  [06]   | `Runtime.GetNSObject<T>`               | `(nint ptr, bool owns = false) → T`       | resolve a handle to a typed object        |
|  [07]   | `Runtime.GetINativeObject<T>`          | `(nint ptr, bool owns) → T`               | resolve a handle to a native object       |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_GATE]:
- The whole surface exists only on the macOS target; a non-macOS host has no `Microsoft.macOS` assembly and paces animation on the `UITimer`/idle clock (`api-eto-runtime`). A pacing owner selects the display-link path under a macOS platform gate and the timer path otherwise, so the two clocks are one polymorphic pace rail discriminated by host, never a compile-time fork bleeding macOS types into portable code.
- Motion is additionally gated by accessibility state: `NSWorkspace.SharedWorkspace.AccessibilityDisplayShouldReduceMotion` (and its increase-contrast, differentiate-without-color, and reduce-transparency siblings) is read before an animation starts, so a reduce-motion preference collapses a paced transition to its end state rather than running the display link.

[DISPLAY_LINK_LAW]:
- A display link is built from the screen it will drive: `NSScreen.GetDisplayLink(target, selector)` is the factory, and a view reaches its screen through `NSView.Window`. The link's `PreferredFrameRateRange` (a `CAFrameRateRange`) requests a rate the panel's `MaximumFramesPerSecond`/`MaximumRefreshInterval` bound; the callback reads `TargetTimestamp` to advance animation against the next presentation, never wall-clock.
- The link lifecycle is create → `AddToRunLoop(NSRunLoop.Main, mode)` → `Paused` toggling → `RemoveFromRunLoop`/`Invalidate`; an invalidated link is dead and rebuilt, never resumed.
- Screen reconfiguration invalidates cached display facts: `ObserveDidChangeScreenParameters` fires when a display is added, removed, or re-rated, and the pace owner re-reads `MaximumFramesPerSecond` and rebinds the link's target screen on that signal.

[BRIDGE_LAW]:
- `Runtime.GetNSObject<T>`/`GetINativeObject<T>` resolve a raw `nint` handle to a typed managed object; the `owns` flag decides whether the managed wrapper takes native ownership. This is the only sanctioned crossing from a native pointer to a managed AppKit object.

[STACKING]:
- `LanguageExt`(`.api/api-languageext`): a display link's create → attach → invalidate lifecycle is resource-scoped through the `use` rail, so `AddToRunLoop` acquires and `RemoveFromRunLoop`/`Invalidate` releases inside one bracket and a link never outlives its scope; the per-frame callback composes as an `IO<A>`/`Eff<A>` step advancing off `TargetTimestamp`. `Fin<A>` carries every platform-gated call so an off-macOS or unavailable-screen path is a typed rail, not an exception. `Option<A>` lifts the nullable `NSView.Window` → `NSScreen` resolution and a null shared-workspace read so an unresolved screen is `None`.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions`): `NSRunLoopMode` binds as a `[SmartEnum<string>]` over the known mode identities so a run-loop attachment is keyed by a validated owner, not a raw mode constant; a `CAFrameRateRange` policy binds as a `[ValueObject]`/`[ComplexValueObject]` so the min/max/preferred triple is one validated owner routed by generated equality rather than three loose floats.

[LOCAL_ADMISSION]:
- `Microsoft.macOS` is host-provided under the macOS TFM and never re-declared; a Rasm pace owner internalizes the display-link, accessibility-gate, and screen-observation concern behind one canonical macOS rail composed by the host-polymorphic pace surface, so portable code holds a paced effect and an accessibility verdict, never an `NSScreen`, a raw `CADisplayLink`, or an `nint` handle.

[RAIL_LAW]:
- Package: `Microsoft.macOS`
- Owns: `NSScreen` display facts and display-link factory, `CADisplayLink`/`CAFrameRateRange` motion clock, `NSRunLoop`/`NSRunLoopMode` attachment, `NSWorkspace` accessibility state, `NSApplication.Notifications` screen observation, `Runtime` object bridge
- Accept: macOS-target high-fidelity animation pacing, accessibility-gated motion, screen-parameter observation, vsync-locked per-frame callbacks
- Reject: portable clock pacing (`api-eto-runtime` `UITimer`), any use off the macOS target, and leaking `NSScreen`/`CADisplayLink`/`nint` handles past the host-polymorphic pace rail
