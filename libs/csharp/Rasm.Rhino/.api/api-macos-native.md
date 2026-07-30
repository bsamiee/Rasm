# [RASM_RHINO_API_MACOS_NATIVE]

The Rhino host boundary reaches macOS natively for exactly one concern: high-fidelity motion pacing behind a host-polymorphic pace rail portable code never sees. The vsync clock, its rate range, the run loop, the display facts bounding a requested rate, the accessibility gates, and the screen-reconfiguration signal are the branch pacing core this partition registers; what it owns is the rail that selects, gates, brackets, and rebinds that core, and falls back to the portable clock off macOS.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.macOS` — Rhino pace-rail partition
- package: `Microsoft.macOS` (MIT)
- assembly: `Microsoft.macOS` — loaded by the Rhino host under `net10.0-macos`; the whole surface is absent off-macOS
- namespace: `AppKit`, `CoreAnimation`, `Foundation`, `ObjCRuntime`
- rail: macos-native

## [02]-[BOUNDARY_REACH]

- Registers the macOS pacing core (`libs/csharp/.api/api-macos-native.md`): the `NSView`-to-`NSWindow`-to-`NSScreen` anchor chain with its refresh ceiling, refresh intervals, backing scale, and EDR headroom, `CADisplayLink`/`CAFrameRateRange`, `NSRunLoop`/`NSRunLoopMode`, the `NSWorkspace` accessibility gates, the screen and accessibility observation tokens, and the `Runtime` handle bridge carry their algebra there. This boundary composes the layer graph, capture, gesture, and event subsystems nowhere, so it adds no carrier and states the pace-rail law over the registered core.

| [INDEX] | [RAIL_STAGE]       | [REGISTERED_MEMBERS]                                                                          |
| :-----: | :----------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | host selection     | `Platform.IsMac` gate against the portable clock (`api-eto-runtime.md`)                       |
|  [02]   | accessibility gate | `NSWorkspace.SharedWorkspace`, `AccessibilityDisplayShouldReduceMotion` and its four siblings |
|  [03]   | anchor resolution  | `NSView.Window`, `NSWindow.Screen`                                                            |
|  [04]   | rate negotiation   | `NSScreen.MaximumFramesPerSecond`, `MaximumRefreshInterval`, `CAFrameRateRange.Create`        |
|  [05]   | link construction  | `NSScreen.GetDisplayLink(NSObject, Selector)`, `CADisplayLink.PreferredFrameRateRange`        |
|  [06]   | loop bracket       | `CADisplayLink.AddToRunLoop`, `Paused`, `RemoveFromRunLoop`, `Invalidate`                     |
|  [07]   | frame advance      | `CADisplayLink.TargetTimestamp`, `Timestamp`, `Duration`                                      |
|  [08]   | reconfigure rebind | `NSApplication.Notifications.ObserveDidChangeScreenParameters`                                |
|  [09]   | density read       | `NSScreen.BackingScaleFactor`                                                                 |
|  [10]   | handle crossing    | `Runtime.GetNSObject<T>`, `Runtime.GetINativeObject<T>`                                       |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Pacing is one polymorphic rail discriminated by host: the display-link path runs under the macOS gate and the portable clock path otherwise, never a compile-time fork bleeding macOS types into portable code.
- The rail stages run in order and each stage forecloses the next on failure: an unavailable host stops at selection, a reduce-motion preference collapses a paced transition to its end state before a link is ever built, an unresolved anchor screen stops at resolution, and only a negotiated rate reaches construction.
- The callback advances against `TargetTimestamp`, so frame progress is presentation-relative and a dropped frame advances the same distance a delivered one does.
- Reconfiguration is a signal, not a poll: the observer fires when a display is added, removed, or re-rated, and the rail re-reads the refresh ceiling and rebinds the link's target screen on that edge alone.
- An invalidated link is dead and rebuilt, never resumed; the bracket releases in exact inverse and disposes the link and its callback target.

[STACKING]:
- `api-macos-native`(`../../.api/api-macos-native.md`): the registered pacing core; this boundary composes it and re-tables none of it.
- `api-eto-runtime`(`api-eto-runtime.md`): the portable clock the rail falls back to off macOS, and the density surface the backing-scale read supersedes on macOS.
- `api-eto-platform`(`api-eto-platform.md`): the native host resolves the view whose window and screen the anchor stage reads.
- `LanguageExt.Core`(`../../.api/api-languageext.md`): the create, attach, and invalidate bracket is resource-scoped through the `use` rail so a link never outlives its scope, the per-frame callback composes as an `IO<A>`/`Eff<A>` step, `Fin<A>` carries every platform-gated stage so an off-macOS or unavailable-screen path is a typed rail, and `Option<A>` lifts the nullable anchor resolution.
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): the run-loop mode binds as a `[SmartEnum<string>]` so an attachment is keyed by a validated owner, and a frame-rate policy binds as a `[ComplexValueObject]` so the minimum, maximum, and preferred triple is one validated owner routed by generated equality.

[LOCAL_ADMISSION]:
- `Microsoft.macOS` is host-provided under the macOS target and never re-declared; the pace owner internalizes the display-link, accessibility-gate, and screen-observation concern behind one canonical rail.
- Portable code holds a paced effect and an accessibility verdict, never a screen handle, a raw display link, or an `nint`.

[RAIL_LAW]:
- Partition: `Microsoft.macOS` Rhino host boundary — the host-polymorphic pace rail over the registered pacing core
- Owns: host selection, the accessibility gate, anchor resolution, rate negotiation, the run-loop bracket, presentation-relative frame advance, and the reconfiguration rebind
- Accept: macOS-target high-fidelity animation pacing, accessibility-gated motion, screen-parameter observation, vsync-locked per-frame callbacks
- Reject: a re-tabling of the registered pacing core, the layer graph, capture, gesture, and event subsystems this boundary never reaches, portable clock pacing under the macOS gate, any use off the macOS target, and leaking a screen handle, display link, or `nint` past the pace rail
