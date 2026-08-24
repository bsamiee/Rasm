# [RASM_API_MACOS_NATIVE]

`Microsoft.macOS` is the .NET AppKit, CoreAnimation, and Foundation binding the Rhino host loads under the macOS target, and this branch catalogue owns the pacing core both host boundaries pace on: the view-to-window-to-screen chain that resolves the anchor display, the `CADisplayLink` vsync clock and its rate range, the run loop the link attaches to, the accessibility gates that decide whether motion runs at all, the screen-reconfiguration signal, and the handle-to-object bridge. Every member is platform-gated to macOS; a non-macOS host paces on the `UITimer` clock (`.api/api-eto-runtime.md`). Each host-boundary folder registers this core and tables only the native subsystem its own boundary reaches.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: installed macOS bindings — pacing core
- host: Rhino host runtime, in-process under `net10.0-macos`; the whole surface is absent off-macOS (MIT)
- assembly: `Microsoft.macOS` (`Microsoft.macOS.dll`) from the installed RhinoWIP bundle, `HintPath`-referenced and never a NuGet admission
- namespace: `AppKit`, `CoreAnimation`, `CoreGraphics`, `Foundation`, `ObjCRuntime`
- rail: macos-native

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: view, window, and display state (`AppKit`)

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :-------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `NSView`        | class         | native view, its owning window, and its coordinate conversions     |
|  [02]   | `NSWindow`      | class         | native window and the screen hosting it                            |
|  [03]   | `NSScreen`      | class         | display-link factory, refresh ceiling, backing scale, EDR headroom |
|  [04]   | `NSWorkspace`   | class         | the five accessibility display gates through `SharedWorkspace`     |
|  [05]   | `NSApplication` | class         | the screen-reconfiguration notification host                       |

[PUBLIC_TYPE_SCOPE]: motion clock and run loop (`CoreAnimation`, `Foundation`)

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :----------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `CADisplayLink`    | class         | per-frame callback carrying `Timestamp` and `TargetTimestamp` |
|  [02]   | `CAFrameRateRange` | struct        | minimum, maximum, and preferred frame-rate range              |
|  [03]   | `NSRunLoop`        | class         | the loop a display link or timer attaches to, keyed by mode   |
|  [04]   | `NSRunLoopMode`    | static class  | the typed loop modes an attachment fires under                |

[PUBLIC_TYPE_SCOPE]: object bridge and marshal (`Foundation`, `ObjCRuntime`)

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :---------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `NSObject`        | class         | Objective-C object base and observer-token carrier         |
|  [02]   | `NSString`        | class         | native string carrier keying mode and appearance constants |
|  [03]   | `Selector`        | class         | the Objective-C message selector a display link targets    |
|  [04]   | `ExportAttribute` | attribute     | binds a managed method to a selector                       |
|  [05]   | `Runtime`         | static class  | typed `NSObject`/`INativeObject` resolution from an `nint` |

[NUMERIC_CARRIERS]: `nint` `NFloat` `double` `float` — a native op never widens or narrows the carrier inside the boundary.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: view, window, and display facts

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]              |
| :-----: | :--------------------------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `NSView.Window -> NSWindow?`                                                 | property | hosting window            |
|  [02]   | `NSView.ConvertPointFromView(CGPoint, NSView?) -> CGPoint`                   | instance | window-relative point map |
|  [03]   | `NSView.GetDisplayLink(NSObject, Selector) -> CADisplayLink`                 | instance | view-bound vsync source   |
|  [04]   | `NSWindow.Screen -> NSScreen`                                                | property | hosting display           |
|  [05]   | `NSScreen.MainScreen / Screens -> NSScreen[]`                                | static   | display enumeration       |
|  [06]   | `NSScreen.GetDisplayLink(NSObject, Selector) -> CADisplayLink`               | instance | screen-bound vsync source |
|  [07]   | `NSScreen.MaximumFramesPerSecond -> nint`                                    | property | refresh ceiling           |
|  [08]   | `NSScreen.MinimumRefreshInterval / MaximumRefreshInterval -> double`         | property | refresh-interval bounds   |
|  [09]   | `NSScreen.BackingScaleFactor -> NFloat`                                      | property | device-pixel scale        |
|  [10]   | `NSScreen.MaximumExtendedDynamicRangeColorComponentValue -> NFloat`          | property | EDR headroom              |
|  [11]   | `NSScreen.MaximumPotentialExtendedDynamicRangeColorComponentValue -> NFloat` | property | EDR potential headroom    |
|  [12]   | `NSScreen.MaximumReferenceExtendedDynamicRangeColorComponentValue -> NFloat` | property | EDR reference headroom    |

- `CADisplayLink`, `NSView.GetDisplayLink`, and `NSScreen.GetDisplayLink` carry `SupportedOSPlatform("macos14.0")` and declare non-null while the native result still needs runtime validation.
- `NSWindow.Screen` resolves to native null before the window belongs to a screen; a view-bound pacing decision reads `view.Window?.Screen`, never `NSScreen.MainScreen`, which describes the application main screen. A windowless anchor resolves by the window walk — the key window's screen, else the first application window resolving one, else a typed refusal selecting the portable row.

[ENTRYPOINT_SCOPE]: display-link lifecycle and run-loop attachment

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `CADisplayLink.Create(NSObject, Selector) -> CADisplayLink`        | static   | target and selector link                |
|  [02]   | `CADisplayLink.Timestamp -> double`                                | property | current frame timestamp                 |
|  [03]   | `CADisplayLink.TargetTimestamp -> double`                          | property | next presentation time                  |
|  [04]   | `CADisplayLink.Duration -> double`                                 | property | the frame interval                      |
|  [05]   | `CADisplayLink.Paused -> bool`                                     | property | pause and resume the callback           |
|  [06]   | `CADisplayLink.PreferredFrameRateRange -> CAFrameRateRange`        | property | the requested frame-rate range          |
|  [07]   | `CADisplayLink.AddToRunLoop(NSRunLoop, NSRunLoopMode)`             | instance | attach the link to a run loop           |
|  [08]   | `CADisplayLink.RemoveFromRunLoop(NSRunLoop, NSRunLoopMode)`        | instance | detach the link from a run loop         |
|  [09]   | `CADisplayLink.Invalidate()`                                       | instance | permanently tear down the link          |
|  [10]   | `CAFrameRateRange.Create(float, float, float) -> CAFrameRateRange` | static   | rate range: minimum, maximum, preferred |
|  [11]   | `CAFrameRateRange.Default -> CAFrameRateRange`                     | static   | the unconstrained rate range            |
|  [12]   | `CAFrameRateRange.Minimum / Maximum / Preferred -> float`          | property | mutable rate fields                     |
|  [13]   | `NSRunLoop.Main / NSRunLoop.Current -> NSRunLoop`                  | static   | the main and current run loops          |
|  [14]   | `NSRunLoop.AddTimer(NSTimer, NSRunLoopMode)`                       | instance | attach a timer to the loop              |
|  [15]   | `NSRunLoop.Perform(NSRunLoopMode[], Action)`                       | instance | schedule a block on the loop            |
|  [16]   | `NSRunLoop.WakeUp()`                                               | instance | wake a blocked run loop                 |

[RUN_LOOP_MODES]: `Default` `Common` `ConnectionReply` `ModalPanel` `EventTracking` `Other`

- `AddToRunLoop` and `RemoveFromRunLoop` each expose `(NSRunLoop, NSString)` and `(NSRunLoop, NSRunLoopMode)` overloads; `NSRunLoop.Main` with `NSRunLoopMode.Common` is the typed common-mode attachment.
- Teardown removes the link from the same loop and mode, invalidates it, then disposes the link and its callback target; an invalidated link is dead and rebuilt, never resumed.

[ENTRYPOINT_SCOPE]: accessibility gates, display observation, and the object bridge

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------------------------- |
|  [01]   | `NSWorkspace.SharedWorkspace -> NSWorkspace`                                    | static   | shared workspace singleton             |
|  [02]   | `NSWorkspace.AccessibilityDisplayShouldReduceMotion -> bool`                    | property | reduce-motion gate                     |
|  [03]   | `NSWorkspace.AccessibilityDisplayShouldIncreaseContrast -> bool`                | property | increase-contrast gate                 |
|  [04]   | `NSWorkspace.AccessibilityDisplayShouldDifferentiateWithoutColor -> bool`       | property | differentiate-without-colour gate      |
|  [05]   | `NSWorkspace.AccessibilityDisplayShouldReduceTransparency -> bool`              | property | reduce-transparency gate               |
|  [06]   | `NSWorkspace.AccessibilityDisplayShouldInvertColors -> bool`                    | property | invert-colours gate                    |
|  [07]   | `NSWorkspace.Notifications.ObserveDisplayOptionsDidChange(...) -> NSObject`     | static   | accessibility-change observer token    |
|  [08]   | `NSApplication.Notifications.ObserveDidChangeScreenParameters(...) -> NSObject` | static   | display-reconfiguration observer token |
|  [09]   | `Runtime.GetNSObject<T>(nint[, bool]) -> T?`                                    | static   | resolve a handle to a typed object     |
|  [10]   | `Runtime.GetINativeObject<T>(nint, bool) -> T`                                  | static   | resolve a handle to a native object    |

- Each observer overload comes unfiltered and in an `(NSObject objectToObserve)` form, and both return an `NSObject` token whose disposal releases the registration.
- `Runtime.GetNSObject<T>`/`GetINativeObject<T>` are the only sanctioned crossing from a native pointer to a managed object, the `owns` flag deciding native ownership.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Surface presence is macOS-only: a pacing owner selects the display-link path under the macOS host gate and the `UITimer` path otherwise — one polymorphic pace rail discriminated by host, never a compile-time fork bleeding macOS types into portable code.
- A display link is built from the display it drives, the view reaching its screen through `NSView.Window`; its `PreferredFrameRateRange` requests a rate the screen's `MaximumFramesPerSecond`/`MaximumRefreshInterval` bound, and the callback advances against `TargetTimestamp`, never wall-clock.
- Link lifecycle is create, `AddToRunLoop(NSRunLoop.Main, mode)`, `Paused` toggle, then `RemoveFromRunLoop` and `Invalidate`; every retained native object carries its exact inverse and disposal order.
- Motion is accessibility-gated: `AccessibilityDisplayShouldReduceMotion` and its four siblings are read before an animation starts, so a reduce-motion preference collapses a paced transition to its end state instead of running the link.
- `ObserveDidChangeScreenParameters` fires when a display is added, removed, or re-rated; the pace owner re-reads the refresh ceiling and rebinds the link's target screen on that signal.

[STACKING]:
- `LanguageExt.Core`(`.api/api-languageext.md`): the create, attach, and invalidate lifecycle is resource-scoped through the `use` rail so a link never outlives its scope, and the per-frame callback composes as an `IO<A>`/`Eff<A>` step advancing off `TargetTimestamp`; `Fin<A>` carries every platform-gated call so an off-macOS or unavailable-screen path is a typed rail rather than an exception; `Option<A>` lifts the nullable `NSView.Window` to `NSScreen` resolution and every `Runtime` handle read.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions.md`): `NSRunLoopMode` binds as a `[SmartEnum<string>]` over the known mode identities so an attachment is keyed by a validated owner rather than a raw constant, and a `CAFrameRateRange` policy binds as a `[ComplexValueObject]` so the minimum, maximum, and preferred triple is one validated owner routed by generated equality.
- `api-eto-runtime`(`.api/api-eto-runtime.md`): `UITimer` and `Screen.LogicalPixelSize` are the host-neutral pace and density surface this core supersedes on macOS and falls back to elsewhere.
- `api-eto-platform`(`.api/api-eto-platform.md`): the managed-tree native host resolves the Eto-backed `NSView` every call, layer mount, and pace here runs inside, so a native op executes only within that view's valid host lifetime.

[LOCAL_ADMISSION]:
- The seam admits only after the macOS process check and a valid active platform; installed AppKit types carry no application-level admission themselves.
- A boundary internalizes the display-link, accessibility-gate, and screen-observation concern behind one canonical pace rail, so portable code holds a paced effect and an accessibility verdict, never an `NSScreen`, a raw `CADisplayLink`, or an `nint` handle.

[RAIL_LAW]:
- Package: `Microsoft.macOS`
- Owns: the view-window-screen anchor chain, `NSScreen` display and EDR facts with its display-link factory, the `CADisplayLink`/`CAFrameRateRange` motion clock, `NSRunLoop`/`NSRunLoopMode` attachment, the `NSWorkspace` accessibility gates, screen and accessibility observation tokens, and the `Runtime` handle bridge
- Accept: vsync-locked per-frame callbacks, accessibility-gated motion, screen-parameter observation, exact numeric carriers, paired native lifecycles, screen-local pacing
- Reject: portable clock pacing off the macOS target (`.api/api-eto-runtime.md`), `NSScreen.MainScreen` as an anchor-display substitute, unpaired native retention, leaking `NSScreen`, `CADisplayLink`, or an `nint` handle past the pace rail, and a folder partition re-tabling this pacing core at member depth
