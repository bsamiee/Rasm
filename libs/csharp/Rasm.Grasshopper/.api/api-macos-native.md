# [RASM_GRASSHOPPER_API_MACOS_NATIVE]

The macOS-native seam owns the platform-gated compositing, input, and pacing layer beneath Grasshopper2 canvas cosmetics. Every member here binds only after an `Eto.Forms` control or GH2 canvas yields its backing `NSView`: `AppKit` supplies views, events, gestures, pressure, local monitors, visual-effect chrome, haptics, and P3/EDR color; `CoreAnimation` supplies the `CALayer` graph, the animation family, and `CADisplayLink` vsync pacing; `CoreGraphics` and `CoreImage` supply path, color, transform, and filter primitives; `Foundation`/`ObjCRuntime` supply the object, string, and selector interop base. The non-macOS branch paces on message-loop timers and skips this layer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: macOS-native seam

- host: `Grasshopper2` inside the Rhino 9 WIP process, macOS branch only
- assembly: `Microsoft.macOS.dll` (AppKit/CoreAnimation/CoreGraphics/CoreImage/Foundation interop), `Eto.macOS.dll` (platform bridge)
- namespace: `AppKit`, `CoreAnimation`, `CoreGraphics`, `CoreImage`, `Foundation`, `ObjCRuntime`
- gate: every member binds under `Platform.IsMac`; the `NSView` entry arrives from `IMacViewHandler.Control` or `Grasshopper2.UI.Canvas.Canvas.ControlObject`
- rail: native-compositing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: AppKit views, screens, and events

- rail: native-compositing

| [INDEX] | [SYMBOL]                   | [KIND] | [CAPABILITY]                                              |
| :-----: | :------------------------- | :----- | :-------------------------------------------------------- |
|  [01]   | `NSView`                   | class  | backing view; gestures, pressure, point conversion, links |
|  [02]   | `NSWindow`                 | class  | backing window of a hosted control                        |
|  [03]   | `NSScreen`                 | class  | display refresh bounds and frame-rate ceilings            |
|  [04]   | `NSEvent`                  | class  | typed input event; local-monitor source                   |
|  [05]   | `NSEventMask`              | enum   | event-class subscription mask                             |
|  [06]   | `NSEventType`              | enum   | event discriminant                                        |
|  [07]   | `NSEventPhase`             | enum   | scroll/gesture phase and momentum phase                   |
|  [08]   | `NSGestureRecognizer`      | class  | attached gesture recognizer                               |
|  [09]   | `NSGestureRecognizerState` | enum   | recognizer lifecycle state                                |
|  [10]   | `NSPressureConfiguration`  | class  | force-click pressure behavior binding                     |
|  [11]   | `NSPressureBehavior`       | enum   | pressure-response behavior                                |

[PUBLIC_TYPE_SCOPE]: AppKit chrome, color, and accessibility

- rail: native-compositing

| [INDEX] | [SYMBOL]                     | [KIND] | [CAPABILITY]                                          |
| :-----: | :--------------------------- | :----- | :---------------------------------------------------- |
|  [01]   | `NSVisualEffectView`         | class  | blur/vibrancy backing view                            |
|  [02]   | `NSVisualEffectMaterial`     | enum   | material of a visual-effect view                      |
|  [03]   | `NSVisualEffectBlendingMode` | enum   | behind-window versus within-window blending           |
|  [04]   | `NSColorSpace`               | class  | color-space carrier                                   |
|  [05]   | `NSColor`                    | class  | AppKit color; `FromDisplayP3` wide-gamut construction |
|  [06]   | `NSFont`                     | class  | AppKit font                                           |
|  [07]   | `NSHapticFeedbackManager`    | class  | trackpad haptic performer                             |
|  [08]   | `NSHapticFeedbackPattern`    | enum   | haptic pattern                                        |
|  [09]   | `NSWorkspace`                | class  | accessibility display flags and change notifications  |
|  [10]   | `NSApplication`              | class  | screen-parameter change notifications                 |

[PUBLIC_TYPE_SCOPE]: CoreAnimation layer graph and pacing

- rail: native-compositing

| [INDEX] | [SYMBOL]                | [KIND] | [CAPABILITY]                                         |
| :-----: | :---------------------- | :----- | :--------------------------------------------------- |
|  [01]   | `CALayer`               | class  | composited layer; frame, mask, sublayers, animations |
|  [02]   | `CAShapeLayer`          | class  | vector layer over a `CGPath`                         |
|  [03]   | `CAGradientLayer`       | class  | gradient-filled layer                                |
|  [04]   | `CATextLayer`           | class  | text layer                                           |
|  [05]   | `CAReplicatorLayer`     | class  | instanced-copy layer                                 |
|  [06]   | `CAEmitterLayer`        | class  | particle layer over `CAEmitterCell`                  |
|  [07]   | `CATransform3D`         | struct | layer transform                                      |
|  [08]   | `CABasicAnimation`      | class  | single-property keyframe interpolation               |
|  [09]   | `CASpringAnimation`     | class  | physically-damped property animation                 |
|  [10]   | `CAKeyFrameAnimation`   | class  | multi-keyframe property animation                    |
|  [11]   | `CAAnimationGroup`      | class  | grouped animation set                                |
|  [12]   | `CAMediaTimingFunction` | class  | named timing curve                                   |
|  [13]   | `CATransaction`         | class  | implicit-animation batching and completion           |
|  [14]   | `CADisplayLink`         | class  | vsync-synchronized frame callback                    |
|  [15]   | `CAFrameRateRange`      | struct | preferred/min/max frame-rate window                  |

[PUBLIC_TYPE_SCOPE]: CoreGraphics, CoreImage, and interop base

- rail: native-compositing

| [INDEX] | [SYMBOL]             | [KIND]    | [CAPABILITY]                                 |
| :-----: | :------------------- | :-------- | :------------------------------------------- |
|  [01]   | `CGPath`             | class     | native path geometry                         |
|  [02]   | `CGColor`            | class     | native color                                 |
|  [03]   | `CGAffineTransform`  | struct    | native 2D affine transform                   |
|  [04]   | `CGPoint`/`CGRect`   | struct    | native point and rectangle                   |
|  [05]   | `CIFilter`           | class     | Core Image filter by name                    |
|  [06]   | `NSObject`           | class     | ObjC object base                             |
|  [07]   | `NSAttributedString` | class     | styled-text carrier                          |
|  [08]   | `Selector`           | class     | ObjC selector handle                         |
|  [09]   | `Runtime`            | static    | managed-to-native object marshal             |
|  [10]   | `ExportAttribute`    | attribute | exposes a managed member to the ObjC runtime |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `NSView` / `NSScreen` — attachment and pacing bounds

- `AppKit.NSView`: `NSWindow Window { get; }`, `CGRect Bounds { get; }`, `NSPressureConfiguration PressureConfiguration { get; set; }`
- `void AddGestureRecognizer(NSGestureRecognizer recognizer)`, `void RemoveGestureRecognizer(NSGestureRecognizer recognizer)`, `CGPoint ConvertPointFromView(CGPoint point, NSView view)`
- `CADisplayLink GetDisplayLink(NSObject target, Selector selector)`
- `AppKit.NSScreen`: `int MaximumFramesPerSecond { get; }`, `double MaximumRefreshInterval { get; }`, `double MinimumRefreshInterval { get; }`, `static NSScreen MainScreen { get; }`

[ENTRYPOINT_SCOPE]: `NSEvent` — local monitors and rich input

- `static NSObject AddLocalMonitorForEventsMatchingMask(NSEventMask mask, Func<NSEvent,NSEvent> handler)`, `static void RemoveMonitor(NSObject monitor)`
- `NSEventType Type { get; }`, `NSEventPhase Phase { get; }`, `NSEventPhase MomentumPhase { get; }`, `ulong ModifierFlags { get; }`, `ulong KeyCode { get; }`
- `double ScrollingDeltaX { get; }`, `double ScrollingDeltaY { get; }`, `double Magnification { get; }`, `double Rotation { get; }`
- `double Pressure { get; }`, `double TangentialPressure { get; }`, `long Stage { get; }`, `double StageTransition { get; }`

[ENTRYPOINT_SCOPE]: AppKit chrome, color, and accessibility

- `AppKit.NSColor.FromDisplayP3(float red, float green, float blue, float alpha): NSColor`
- `AppKit.NSHapticFeedbackManager.DefaultPerformer`, `NSHapticFeedbackPattern`, `NSHapticFeedbackPerformanceTime`
- `NSWorkspace.SharedWorkspace`, `bool AccessibilityDisplayShouldReduceMotion { get; }`, `bool AccessibilityDisplayShouldDifferentiateWithoutColor { get; }`, `bool AccessibilityDisplayShouldReduceTransparency { get; }`
- `NSWorkspace.DisplayOptionsDidChangeNotification`, `NSApplication.Notifications.ObserveDidChangeScreenParameters(EventHandler<NSNotificationEventArgs> handler)`

[ENTRYPOINT_SCOPE]: `CALayer` / `CAShapeLayer` — the layer graph

- `AppKit`/`CoreAnimation` `CALayer`: `CGRect Frame { get; set; }`, `CGColor BackgroundColor { get; set; }`, `CGColor BorderColor { get; set; }`, `float BorderWidth { get; set; }`, `float CornerRadius { get; set; }`, `bool MasksToBounds { get; set; }`, `CALayer Mask { get; set; }`, `CALayer[] Sublayers { get; }`
- `void AddSublayer(CALayer layer)`, `void RemoveFromSuperLayer()`, `void AddAnimation(CAAnimation animation, NSString key)`, `void RemoveAnimation(NSString key)`
- `CAShapeLayer`: `CGPath Path { get; set; }`, `CGColor FillColor { get; set; }`, `CGColor StrokeColor { get; set; }`, `float LineWidth { get; set; }`, `NSString LineCap { get; set; }`, `NSString LineJoin { get; set; }`, `static NSString CapRound { get; }`, `static NSString JoinRound { get; }`

[ENTRYPOINT_SCOPE]: animation, transaction, and vsync pacing

- `CoreAnimation.CABasicAnimation.FromKeyPath(string path)`, `CoreAnimation.CAKeyFrameAnimation.GetFromKeyPath(string path)`, `CoreAnimation.CAMediaTimingFunction.FromName(NSString name)`
- `CoreAnimation.CATransaction.Begin()`, `CoreAnimation.CATransaction.Commit()`, `bool CATransaction.DisableActions { get; set; }`, `Action CATransaction.CompletionBlock { get; set; }`
- `CoreAnimation.CADisplayLink`: `bool Paused { get; set; }`, `void Invalidate()`, `CAFrameRateRange PreferredFrameRateRange { get; set; }`
- `CoreAnimation.CAFrameRateRange.Create(float minimum, float maximum, float preferred): CAFrameRateRange`

[ENTRYPOINT_SCOPE]: CoreGraphics, CoreImage, and marshal

- `CoreGraphics.CGPath`: `void MoveToPoint(CGPoint point)`, `void AddLineToPoint(CGPoint point)`, `void AddRoundedRect(CGAffineTransform transform, CGRect rect, float cornerWidth, float cornerHeight)`
- `CoreGraphics.CGAffineTransform.MakeIdentity()`
- `CoreImage.CIFilter.FromName(string name)`, `CIFilter Copy()`, `NSString Name { get; }`
- `ObjCRuntime.Runtime.GetNSObject<T>(IntPtr handle): T`, `ObjCRuntime.Selector`, `ObjCRuntime.ExportAttribute`

## [04]-[IMPLEMENTATION_LAW]

[MACOS_NATIVE_TOPOLOGY]:

- The seam begins at a managed control object and becomes an `NSView` only on the macOS branch: `Grasshopper2.UI.Canvas.Canvas.ControlObject` or `IMacViewHandler.Control` yields the backing view, and every gesture, pressure, cosmetic, and pacer attachment binds to that extracted `NSView` — never to the managed facade.
- Input capture rides two sources: `NSView.AddGestureRecognizer` for recognizers and `NSEvent.AddLocalMonitorForEventsMatchingMask` for a raw event stream carrying scroll delta, magnification, rotation, pressure, stage, and modifier detail the managed event snapshot omits; both lifecycles pair with `RemoveGestureRecognizer`/`RemoveMonitor`.
- Cosmetics compose a `CALayer` graph on the view: `CAShapeLayer` over a `CGPath`, `CAGradientLayer`, `CATextLayer`, `CAReplicatorLayer`, and `CAEmitterLayer` are added as sublayers, and every layer mutation batches inside a `CATransaction.Begin`/`Commit` with `DisableActions` gating implicit animation.
- Pacing rides `CADisplayLink` off `NSView.GetDisplayLink`, tuned by `CAFrameRateRange.Create` against the `NSScreen.MaximumFramesPerSecond` ceiling; `NSScreen` parameter changes and `NSWorkspace` accessibility flags (reduce-motion, reduce-transparency, differentiate-without-color) gate whether the cosmetic and pacing layer runs at all.

[STACKING]:

- `.api/api-eto-platform`: the `NSView`/`NSWindow` entry is owned there — `IMacViewHandler.Control` and `Grasshopper2.UI.Canvas.Canvas.ControlObject` are the extraction sources this catalog composites onto; the Eto.Mac conversion owners bridge `Eto.Drawing` values into the `CGColor`/`CGPath`/`CGAffineTransform` this layer consumes.
- `.api/api-eto-drawing`: a curved-stroke or text-state operation the managed `Eto.Drawing.GraphicsPath`/`FormattedText` leaves ambiguous resolves on the native branch through `CGPath` and `CATextLayer`; `NSColor.FromDisplayP3` is the wide-gamut projection of an `Eto.Drawing.Color` at the P3/EDR boundary.
- kernel `MotionInterpolation`: host-agnostic easing, spring, and interpolation math composes the kernel `MotionInterpolation` (`Linear`/`Slerp`/`Interpolate`/`Rotate`/`Combine`) under `MotionPolicy`; `CABasicAnimation`/`CASpringAnimation`/`CAKeyFrameAnimation` are the native applicators of the kernel-computed curve, never a second easing derivation in the folder.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions`): the AppKit and CoreAnimation enum vocabulary (`NSEventType`, `NSEventPhase`, `NSEventMask`, `NSGestureRecognizerState`, `NSPressureBehavior`, `NSVisualEffectMaterial`, `NSVisualEffectBlendingMode`, `NSHapticFeedbackPattern`) maps at the folder boundary onto `[SmartEnum]`/flag owners so an input-phase or material decision is exhaustive dispatch, not a raw native enum switch.
- `LanguageExt.Core`(`.api/api-languageext`): the platform gate and every fallible native extraction (`ControlObject as NSView`, `Runtime.GetNSObject<T>`, a missing display link) lower onto `Option<T>`/`Fin<T>`, and the local-monitor and `CADisplayLink` lifecycles are resource-scoped so a monitor or link is always invalidated on teardown.

[RAIL_LAW]:

- Surface: the macOS-native `AppKit`/`CoreAnimation`/`CoreGraphics`/`CoreImage`/`Foundation` seam
- Owns: platform-gated view attachment, rich native input capture, the `CALayer` compositing graph, the animation family, and `CADisplayLink` vsync pacing beneath canvas cosmetics
- Accept: an `NSView` extracted through the platform bridge, `NSEventMask`-scoped local monitors, `CATransaction`-batched layer mutation, `CAFrameRateRange`-tuned display links, `NSWorkspace` accessibility gates
- Reject: an ungated native call on the non-macOS branch, a hand-rolled easing curve beside the kernel `MotionInterpolation`, a raw native enum switch the `[SmartEnum]` boundary owns, an unscoped local monitor or display link that outlives its view, an unguarded `GetNSObject<T>` that throws instead of lowering onto `Option<T>`
