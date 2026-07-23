# [RASM_RHINO_API_MACOS_NATIVE]

`Microsoft.macOS` is the .NET AppKit/CoreAnimation binding the Rhino host loads under the macOS target, and this catalog owns the native seam behind high-fidelity motion pacing and display observation. It hands the branch a vsync-locked per-frame clock, the panel's refresh capability, the accessibility gates that decide whether motion runs at all, and the screen-reconfiguration signal — all behind a host-polymorphic pace rail portable code never sees. Every member is platform-gated to macOS; a non-macOS host paces on the `UITimer`/idle clock (`api-eto-runtime`), never on this surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.macOS`
- package: `Microsoft.macOS` (MIT)
- assembly: `Microsoft.macOS` — loaded by the Rhino host under `net10.0-macos`; the whole surface is absent off-macOS
- namespace: `AppKit`, `CoreAnimation`, `Foundation`, `ObjCRuntime`
- rail: macos-native

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: display and refresh state (`AppKit`)

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :------------ | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `NSScreen`    | class         | display-link factory, `MaximumFramesPerSecond`, refresh, backing scale |
|  [02]   | `NSView`      | class         | `Window` relation to the owning `NSWindow` and its screen              |
|  [03]   | `NSWorkspace` | class         | the four accessibility display gates via `SharedWorkspace`             |

[PUBLIC_TYPE_SCOPE]: motion clock and run loop (`CoreAnimation`, `Foundation`)

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :----------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `CADisplayLink`    | class         | per-frame callback with `TargetTimestamp` and rate range    |
|  [02]   | `CAFrameRateRange` | struct        | preferred/min/max frame-rate range for a display link       |
|  [03]   | `NSRunLoop`        | class         | the loop a display link or timer attaches to, keyed by mode |
|  [04]   | `NSRunLoopMode`    | enum          | the run-loop mode under which an attachment fires           |

[PUBLIC_TYPE_SCOPE]: screen-parameter observation and object bridge (`AppKit`, `ObjCRuntime`)

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :---------------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `NSApplication.Notifications` | class         | `ObserveDidChangeScreenParameters` display-reconfig hook   |
|  [02]   | `Runtime`                     | class         | typed `NSObject`/`INativeObject` resolution from an `nint` |
|  [03]   | `Selector`                    | class         | the Objective-C message selector a display link targets    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: display state and accessibility gates

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------ | :------- | :-------------------------------- |
|  [01]   | `NSScreen.GetDisplayLink(NSObject, Selector) -> CADisplayLink`            | factory  | build a display link for a screen |
|  [02]   | `NSScreen.MaximumFramesPerSecond -> nint`                                 | property | panel max frame rate              |
|  [03]   | `NSScreen.MaximumRefreshInterval -> double`                               | property | panel max refresh interval        |
|  [04]   | `NSScreen.BackingScaleFactor -> nfloat`                                   | property | device-pixel scale                |
|  [05]   | `NSView.Window -> NSWindow`                                               | property | owning window, and its screen     |
|  [06]   | `NSWorkspace.SharedWorkspace -> NSWorkspace`                              | static   | shared workspace singleton        |
|  [07]   | `NSWorkspace.AccessibilityDisplayShouldReduceMotion -> bool`              | property | reduce-motion gate                |
|  [08]   | `NSWorkspace.AccessibilityDisplayShouldIncreaseContrast -> bool`          | property | increase-contrast gate            |
|  [09]   | `NSWorkspace.AccessibilityDisplayShouldDifferentiateWithoutColor -> bool` | property | differentiate-without-color gate  |
|  [10]   | `NSWorkspace.AccessibilityDisplayShouldReduceTransparency -> bool`        | property | reduce-transparency gate          |

[ENTRYPOINT_SCOPE]: display-link lifecycle and run-loop attachment

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `CADisplayLink.Create(NSObject, Selector) -> CADisplayLink`        | static   | build a display link                    |
|  [02]   | `CADisplayLink.TargetTimestamp -> double`                          | property | next frame's presentation time          |
|  [03]   | `CADisplayLink.Timestamp -> double`                                | property | current frame's timestamp               |
|  [04]   | `CADisplayLink.Duration -> double`                                 | property | the frame interval                      |
|  [05]   | `CADisplayLink.Paused -> bool`                                     | property | pause and resume the callback           |
|  [06]   | `CADisplayLink.PreferredFrameRateRange -> CAFrameRateRange`        | property | the requested frame-rate range          |
|  [07]   | `CADisplayLink.AddToRunLoop(NSRunLoop, NSRunLoopMode)`             | instance | attach the link to a run loop           |
|  [08]   | `CADisplayLink.RemoveFromRunLoop(NSRunLoop, NSRunLoopMode)`        | instance | detach the link from a run loop         |
|  [09]   | `CADisplayLink.Invalidate()`                                       | instance | permanently tear down the link          |
|  [10]   | `CAFrameRateRange.Create(float, float, float) -> CAFrameRateRange` | static   | build a rate range: min, max, preferred |
|  [11]   | `CAFrameRateRange.Default -> CAFrameRateRange`                     | static   | the unconstrained rate range            |

[ENTRYPOINT_SCOPE]: screen-parameter observation, run loop, and bridge

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Notifications.ObserveDidChangeScreenParameters(EventHandler) -> NSObject` | static   | observe display reconfig, returns a token |
|  [02]   | `NSRunLoop.Current` / `NSRunLoop.Main -> NSRunLoop`                        | static   | the current and main run loops            |
|  [03]   | `NSRunLoop.AddTimer(NSTimer, NSRunLoopMode)`                               | instance | attach a timer to the loop                |
|  [04]   | `NSRunLoop.Perform(NSRunLoopMode[], Action)`                               | instance | schedule a block on the loop              |
|  [05]   | `NSRunLoop.WakeUp()`                                                       | instance | wake a blocked run loop                   |
|  [06]   | `Runtime.GetNSObject<T>(nint, bool) -> T`                                  | static   | resolve a handle to a typed object        |
|  [07]   | `Runtime.GetINativeObject<T>(nint, bool) -> T`                             | static   | resolve a handle to a native object       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Surface presence is macOS-only; a non-macOS host has no `Microsoft.macOS` assembly and paces on the `UITimer`/idle clock (`api-eto-runtime`). A pacing owner selects the display-link path under the macOS host gate and the timer path otherwise — one polymorphic pace rail discriminated by host, never a compile-time fork bleeding macOS types into portable code.
- Motion is accessibility-gated: `NSWorkspace.SharedWorkspace.AccessibilityDisplayShouldReduceMotion` (and its increase-contrast, differentiate-without-color, reduce-transparency siblings) is read before an animation starts, so a reduce-motion preference collapses a paced transition to its end state instead of running the display link.
- A display link is built from the screen it drives — `NSScreen.GetDisplayLink(target, selector)`, the view reaching its screen via `NSView.Window`. Its `PreferredFrameRateRange` requests a rate `MaximumFramesPerSecond`/`MaximumRefreshInterval` bound; the callback reads `TargetTimestamp` to advance against the next presentation, never wall-clock.
- Link lifecycle is create → `AddToRunLoop(NSRunLoop.Main, mode)` → `Paused` toggle → `RemoveFromRunLoop`/`Invalidate`; an invalidated link is dead and rebuilt, never resumed.
- `ObserveDidChangeScreenParameters` fires when a display is added, removed, or re-rated; the pace owner re-reads `MaximumFramesPerSecond` and rebinds the link's target screen on that signal.
- `Runtime.GetNSObject<T>`/`GetINativeObject<T>` resolve a raw `nint` handle to a typed managed object, the `owns` flag deciding native ownership — the only sanctioned crossing from a native pointer to a managed AppKit object.

[STACKING]:
- `LanguageExt`(`.api/api-languageext`): a display link's create → attach → invalidate lifecycle is resource-scoped through the `use` rail, so `AddToRunLoop` acquires and `RemoveFromRunLoop`/`Invalidate` releases inside one bracket and a link never outlives its scope; the per-frame callback composes as an `IO<A>`/`Eff<A>` step advancing off `TargetTimestamp`. `Fin<A>` carries every platform-gated call so an off-macOS or unavailable-screen path is a typed rail, not an exception; `Option<A>` lifts the nullable `NSView.Window` → `NSScreen` resolution so an unresolved screen is `None`.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions`): `NSRunLoopMode` binds as a `[SmartEnum<string>]` over the known mode identities so a run-loop attachment is keyed by a validated owner, not a raw constant; a `CAFrameRateRange` policy binds as a `[ComplexValueObject]` so the min/max/preferred triple is one validated owner routed by generated equality rather than three loose floats.

[LOCAL_ADMISSION]:
- `Microsoft.macOS` is host-provided under the macOS TFM and never re-declared; a Rasm pace owner internalizes the display-link, accessibility-gate, and screen-observation concern behind one canonical macOS rail composed by the host-polymorphic pace surface, so portable code holds a paced effect and an accessibility verdict, never an `NSScreen`, a raw `CADisplayLink`, or an `nint` handle.

[RAIL_LAW]:
- Package: `Microsoft.macOS`
- Owns: `NSScreen` display facts and display-link factory, `CADisplayLink`/`CAFrameRateRange` motion clock, `NSRunLoop`/`NSRunLoopMode` attachment, `NSWorkspace` accessibility state, `NSApplication.Notifications` screen observation, `Runtime` object bridge
- Accept: macOS-target high-fidelity animation pacing, accessibility-gated motion, screen-parameter observation, vsync-locked per-frame callbacks
- Reject: portable clock pacing (`api-eto-runtime` `UITimer`), any use off the macOS target, and leaking `NSScreen`/`CADisplayLink`/`nint` handles past the host-polymorphic pace rail
