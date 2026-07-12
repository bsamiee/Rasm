# [RASM_GRASSHOPPER_API_MACOS_NATIVE]

The macOS-native catalogue covers the installed `Microsoft.macOS.dll` bindings used beneath an Eto-hosted Grasshopper 2 canvas. `AppKit` owns views, screens, input, recognizers, pressure, accessibility, and notification tokens; `CoreAnimation` owns the layer graph, transactions, animations, and display links; `Foundation` owns run-loop and Objective-C object lifetimes. `Eto.macOS.dll` owns managed-to-AppKit extraction and value conversion. Native attachment starts from an explicit `IMacControlHandler` view role or a runtime-typed canvas `ControlObject`, and every retained native object keeps its matching removal, invalidation, or disposal inverse.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: installed macOS bindings

- host: Grasshopper 2 inside the Rhino WIP macOS process
- assemblies: `Microsoft.macOS.dll`; `Eto.macOS.dll`
- namespaces: `AppKit`, `CoreAnimation`, `CoreGraphics`, `CoreImage`, `Foundation`, `ObjCRuntime`, `Eto.Mac`, `Eto.Mac.Forms`
- platform entry: `Eto.Mac.Platform`; `IMacControlHandler`; `MacControlExtensions`
- rail: UI-affine native interop

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: views, screens, events, gestures, and pressure

- rail: UI-affine native interop

| [INDEX] | [SYMBOL]                                                                    | [KIND] | [CAPABILITY]                            |
| :-----: | :-------------------------------------------------------------------------- | :----- | :-------------------------------------- |
|  [01]   | `NSView`; `NSWindow`; `NSScreen`                                            | class  | native view and hosting-display state   |
|  [02]   | `NSEvent`; `NSEventMask`; `NSEventType`                                     | family | local event monitoring and discriminant |
|  [03]   | `NSEventPhase`; `NSEventModifierMask`                                       | family | event phase and modifier ABI            |
|  [04]   | `NSGestureRecognizer`; `NSClickGestureRecognizer`; `NSPanGestureRecognizer` | family | click and translation recognition       |
|  [05]   | `NSMagnificationGestureRecognizer`; `NSRotationGestureRecognizer`           | family | magnification and rotation input        |
|  [06]   | `NSPressGestureRecognizer`; `NSGestureRecognizerState`                      | family | press input and recognizer state        |
|  [07]   | `NSPressureConfiguration`; `NSPressureBehavior`                             | family | pressure behavior                       |

[PUBLIC_TYPE_SCOPE]: layer graph, timing, and conversion

- rail: UI-affine native interop

| [INDEX] | [SYMBOL]                                                                  | [KIND] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------------ | :----- | :----------------------------------------- |
|  [01]   | `CALayer`; `CAShapeLayer`; `CAGradientLayer`; `CATextLayer`               | family | composited layer graph                     |
|  [02]   | `CAReplicatorLayer`; `CAEmitterLayer`                                     | family | replicated and emitted layers              |
|  [03]   | `CABasicAnimation`; `CASpringAnimation`; `CAKeyFrameAnimation`            | family | native animation values                    |
|  [04]   | `CAAnimationGroup`; `CAMediaTimingFunction`                               | family | animation grouping and timing              |
|  [05]   | `CATransaction`; `CADisplayLink`; `CAFrameRateRange`                      | family | mutation transaction and display pacing    |
|  [06]   | `CGPath`; `CGColor`; `CGAffineTransform`; `CGPoint`; `CGRect`; `CIFilter` | family | geometry, colour, transform, and filtering |
|  [07]   | `NSObject`; `NSString`; `NSRunLoop`; `NSRunLoopMode`                      | family | Objective-C object and run-loop            |
|  [08]   | `Selector`; `ExportAttribute`; `Runtime`                                  | family | selector export and object marshal         |
|  [09]   | `MacConversions`; `CGConversions`; `MacControlExtensions`                 | family | Eto extraction and conversion              |
|  [10]   | `IMacControlHandler`; `IMacViewHandler`                                   | family | native view roles                          |

[PUBLIC_TYPE_SCOPE]: chrome, accessibility, and observation

- rail: UI-affine native interop

| [INDEX] | [SYMBOL]                                                                                | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `NSVisualEffectView`; `NSVisualEffectMaterial`; `NSVisualEffectBlendingMode`            | AppKit vibrancy and blur              |
|  [02]   | `NSColor`; `NSColorSpace`; `NSFont`; `NSAttributedString`                               | AppKit colour and styled text         |
|  [03]   | `NSHapticFeedbackManager`; `NSHapticFeedbackPattern`; `NSHapticFeedbackPerformanceTime` | haptic performance                    |
|  [04]   | `NSWorkspace`; `NSApplication`; `NSNotificationEventArgs`                               | accessibility and display observation |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Eto-to-AppKit extraction

- rail: runtime-nullable native extraction

| [INDEX] | [SURFACE]                                                             | [DECLARED_RETURN]    | [RUNTIME]     |
| :-----: | :-------------------------------------------------------------------- | :------------------- | :------------ |
|  [01]   | `MacControlExtensions.GetMacControl(Control)`                         | `IMacControlHandler` | nullable      |
|  [02]   | `MacControlExtensions.GetMacViewHandler(Control)`                     | `IMacViewHandler`    | nullable      |
|  [03]   | `MacControlExtensions.GetContainerView(Widget)`                       | `NSView`             | nullable      |
|  [04]   | `IMacControlHandler.ContainerControl`; `ContentControl`               | `NSView`             | role-specific |
|  [05]   | `IMacControlHandler.EventControl`; `FocusControl`; `TextInputControl` | `NSView`             | role-specific |

`GetMacControl`, `GetMacViewHandler`, and `GetContainerView` carry non-null signatures in the installed Eto assembly but return null for absent controls or handlers. `GetContainerView` follows nested Eto controls and finally accepts a direct `NSView` control object. `IMacViewHandler` inherits the five `IMacControlHandler` roles and declares no `Control` property; `IMacWindow.Control` is the separate `NSWindow` property on the window-handler interface. `Grasshopper2.UI.Canvas.Canvas.ControlObject` remains a runtime object and yields a native anchor only when it is an `NSView`.

[ENTRYPOINT_SCOPE]: view, window, and screen

- rail: AppKit view attachment and display facts

| [INDEX] | [TYPE]     | [MEMBERS]                                                               | [ACCESS_ABI]                 |
| :-----: | :--------- | :---------------------------------------------------------------------- | :--------------------------- |
|  [01]   | `NSView`   | `Window: NSWindow?`; `Bounds: CGRect`                                   | get; get/set                 |
|  [02]   | `NSView`   | `WantsLayer: bool`; `Layer: CALayer?`                                   | get/set                      |
|  [03]   | `NSView`   | `PressureConfiguration: NSPressureConfiguration?`                       | get/set                      |
|  [04]   | `NSView`   | `MakeBackingLayer()`; `AddGestureRecognizer`; `RemoveGestureRecognizer` | method                       |
|  [05]   | `NSView`   | `ConvertPointFromView(CGPoint, NSView?)`                                | `CGPoint`; null means window |
|  [06]   | `NSView`   | `GetDisplayLink(NSObject, Selector)`                                    | `CADisplayLink`; macOS 14    |
|  [07]   | `NSWindow` | `Screen: NSScreen`                                                      | get; runtime-nullable        |
|  [08]   | `NSScreen` | `MainScreen: NSScreen`; `Screens: NSScreen[]`                           | static get                   |
|  [09]   | `NSScreen` | `MaximumFramesPerSecond: nint`                                          | get                          |
|  [10]   | `NSScreen` | `MinimumRefreshInterval: double`; `MaximumRefreshInterval: double`      | get                          |
|  [11]   | `NSScreen` | `MaximumExtendedDynamicRangeColorComponentValue: NFloat`                | get                          |
|  [12]   | `NSScreen` | `MaximumPotentialExtendedDynamicRangeColorComponentValue: NFloat`       | get                          |
|  [13]   | `NSScreen` | `MaximumReferenceExtendedDynamicRangeColorComponentValue: NFloat`       | get                          |
|  [14]   | `NSScreen` | `GetDisplayLink(NSObject, Selector)`                                    | `CADisplayLink`; macOS 14    |

`CADisplayLink`, `NSView.GetDisplayLink`, and `NSScreen.GetDisplayLink` carry `SupportedOSPlatform("macos14.0")`. The display-link getters are declared non-null, while the native result still requires runtime validation. `NSWindow.Screen` is likewise declared non-null but resolves to native null before the window belongs to a screen. A view-bound pacing decision reads `view.Window?.Screen`; `NSScreen.MainScreen` describes the application main screen rather than the view's hosting display.

[ENTRYPOINT_SCOPE]: local event monitor ABI

- rail: AppKit callback

| [INDEX] | [MEMBER]                                             | [ABI]                 |
| :-----: | :--------------------------------------------------- | :-------------------- |
|  [01]   | `NSEvent.Type`                                       | `NSEventType`         |
|  [02]   | `NSEvent.Phase`; `MomentumPhase`                     | `NSEventPhase`        |
|  [03]   | `NSEvent.ModifierFlags`                              | `NSEventModifierMask` |
|  [04]   | `NSEvent.KeyCode`                                    | `ushort`              |
|  [05]   | `NSEvent.ScrollingDeltaX`; `ScrollingDeltaY`         | `NFloat`              |
|  [06]   | `NSEvent.Magnification`; `StageTransition`           | `NFloat`              |
|  [07]   | `NSEvent.Rotation`; `Pressure`; `TangentialPressure` | `float`               |
|  [08]   | `NSEvent.Stage`                                      | `nint`                |

`LocalEventHandler` is declared as `NSEvent LocalEventHandler(NSEvent theEvent)`. `NSEvent.AddLocalMonitorForEventsMatchingMask(NSEventMask, LocalEventHandler)` returns an `NSObject` token, and `NSEvent.RemoveMonitor(NSObject)` removes it. Returning the original event preserves delivery; returning runtime null absorbs it despite the delegate's non-null return annotation. Removal and token disposal are distinct inverse steps.

[ENTRYPOINT_SCOPE]: gesture and pressure ABI

- rail: AppKit recognizers

| [INDEX] | [TYPE]                             | [CONSTRUCTION_AND_STATE]                                                 |
| :-----: | :--------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `NSGestureRecognizer`              | `()`; `(Action)`; `(NSObject? target, Selector? action)`                 |
|  [02]   | `NSGestureRecognizer`              | nullable `Action`; nullable `Target`; `State`; `LocationInView(NSView?)` |
|  [03]   | `NSClickGestureRecognizer`         | `(Action)`; `(Action<NSClickGestureRecognizer>)`; click/touch counts     |
|  [04]   | `NSPanGestureRecognizer`           | `(Action)`; translation and velocity as `CGPoint`                        |
|  [05]   | `NSMagnificationGestureRecognizer` | `(Action)`; `Magnification: NFloat`                                      |
|  [06]   | `NSRotationGestureRecognizer`      | `(Action)`; `Rotation: NFloat`; `RotationInDegrees: NFloat`              |
|  [07]   | `NSPressGestureRecognizer`         | `(Action)`; movement `NFloat`; duration `double`; touch count `nint`     |
|  [08]   | `NSPressureConfiguration`          | `()`; `(NSPressureBehavior)`; `PressureBehavior { get; }`; `Set()`       |

`NSGestureRecognizer.Action`, `Target`, and `State` are public get/set properties; the generated binding advises that only recognizer subclasses set `State`. `NSGestureRecognizer.PressureConfiguration` is declared non-null and settable, while `NSView.PressureConfiguration` is nullable and accepts null to remove a view configuration. `NSGestureRecognizer.View` is declared non-null but remains runtime-null before attachment.

[ENTRYPOINT_SCOPE]: display link and run loop

- rail: CoreAnimation pacing

| [INDEX] | [TYPE]             | [MEMBERS]                                                                      | [ACCESS_ABI]         |
| :-----: | :----------------- | :----------------------------------------------------------------------------- | :------------------- |
|  [01]   | `CADisplayLink`    | `Duration`; `Timestamp`; `TargetTimestamp`                                     | `double` get         |
|  [02]   | `CADisplayLink`    | `Paused`; `PreferredFrameRateRange`                                            | get/set              |
|  [03]   | `CADisplayLink`    | `AddToRunLoop`; `RemoveFromRunLoop`; `Invalidate`                              | method               |
|  [04]   | `CAFrameRateRange` | `Minimum`; `Maximum`; `Preferred`                                              | mutable float fields |
|  [05]   | `CAFrameRateRange` | `Create(float minimum, float maximum, float preferred)`                        | static               |
|  [06]   | `NSRunLoop`        | `Main`; `Current`                                                              | static get           |
|  [07]   | `NSRunLoopMode`    | `Default`; `Common`; `ConnectionReply`; `ModalPanel`; `EventTracking`; `Other` | enum                 |

`CADisplayLink.AddToRunLoop` and `RemoveFromRunLoop` each expose `(NSRunLoop, NSString)` and `(NSRunLoop, NSRunLoopMode)` overloads. `NSRunLoop.Main` plus `NSRunLoopMode.Common` is the typed common-mode attachment. `CADisplayLink.Create(NSObject, Selector)` is also public, while `NSView.GetDisplayLink` and `NSScreen.GetDisplayLink` bind the link to a display source. Teardown removes the link from the same loop and mode, invalidates it, then disposes the link and callback target.

[ENTRYPOINT_SCOPE]: layer, animation, and filter state

- rail: CoreAnimation composition

| [INDEX] | [TYPE]          | [MEMBERS]                                                                             | [NULLABILITY]    |
| :-----: | :-------------- | :------------------------------------------------------------------------------------ | :--------------- |
|  [01]   | `CALayer`       | `Frame`; `BorderWidth`; `CornerRadius`; `MasksToBounds`                               | non-null values  |
|  [02]   | `CALayer`       | `BackgroundColor`; `BorderColor`; `Mask`; `Sublayers`; `Filters`; `BackgroundFilters` | nullable         |
|  [03]   | `CALayer`       | `AddSublayer`; `RemoveFromSuperLayer`                                                 | methods          |
|  [04]   | `CALayer`       | `AddAnimation(CAAnimation, string?)`; `RemoveAnimation(string)`                       | methods          |
|  [05]   | `CAShapeLayer`  | `Path`; `FillColor`; `StrokeColor`                                                    | nullable         |
|  [06]   | `CAShapeLayer`  | `LineWidth`; `LineCap`; `LineJoin`; `CapRound`; `JoinRound`                           | non-null         |
|  [07]   | `CATransaction` | `DisableActions: bool`; `CompletionBlock: Action?`; `Begin()`; `Commit()`             | static           |
|  [08]   | `CIFilter`      | `FromName(string): CIFilter?`; `Name { get; set; }`; `Copy(NSZone?)`                  | nullable factory |

`CABasicAnimation.FromKeyPath(string?)` and `CAKeyFrameAnimation.FromKeyPath(string?)` are public nullable-key factories. `CAKeyFrameAnimation.GetFromKeyPath(string)` is the installed non-null-key wrapper. `CAMediaTimingFunction` exposes the four-float control-point constructor and `FromName(NSString)`. A transaction pairs `Begin()` with `Commit()`; `CompletionBlock` is nullable.

[ENTRYPOINT_SCOPE]: geometry, colour, marshal, and Eto conversions

- rail: native value conversion

| [INDEX] | [OWNER]             | [SURFACE]                                                             |
| :-----: | :------------------ | :-------------------------------------------------------------------- |
|  [01]   | `CGPath`            | `MoveToPoint(CGPoint)`; `AddLineToPoint(CGPoint)`; `AddRoundedRect`   |
|  [02]   | `CGAffineTransform` | `MakeIdentity()`                                                      |
|  [03]   | `NSColor`           | `FromDisplayP3(NFloat red, NFloat green, NFloat blue, NFloat alpha)`  |
|  [04]   | `Runtime`           | `GetNSObject<T>(nint): T? where T : NSObject`                         |
|  [05]   | `MacConversions`    | `Color.ToNSUI()`; `CGColor.ToNS()`; `Image.ToCG()`                    |
|  [06]   | `MacConversions`    | `CGPoint.ToEto(NSView)`; `NSColor.ToEto()`                            |
|  [07]   | `CGConversions`     | `NSColor.ToCG()`; `Color.ToCG()`; `CGColor.ToEto()`                   |
|  [08]   | `CGConversions`     | `CGAffineTransform.ToEto()`; `IMatrix.ToCG()`; `IGraphicsPath.ToCG()` |

`MacConversions.ToEto(CGPoint, NSView)` treats the point as window coordinates, converts it through `NSView.ConvertPointFromView(point, null)`, and flips the Y coordinate when the view is not flipped. `MacConversions.ToNS(CGColor)` and `CGConversions.ToCG(NSColor)` carry non-null signatures but return runtime null for null inputs. `CGConversions.ToCG(IMatrix)` returns identity for a runtime-null matrix. `CGConversions.ToEto(CGColor)` throws for unsupported component layouts. Conversion ownership remains in `Eto.Mac`; the native catalogue does not define parallel conversion helpers.

[ENTRYPOINT_SCOPE]: accessibility and display notifications

- rail: AppKit observation

`NSWorkspace.SharedWorkspace` exposes `AccessibilityDisplayShouldDifferentiateWithoutColor`, `AccessibilityDisplayShouldIncreaseContrast`, `AccessibilityDisplayShouldInvertColors`, `AccessibilityDisplayShouldReduceMotion`, and `AccessibilityDisplayShouldReduceTransparency`. `NSWorkspace.Notifications.ObserveDisplayOptionsDidChange` and `NSApplication.Notifications.ObserveDidChangeScreenParameters` each expose unfiltered and `NSObject objectToObserve` overloads returning an `NSObject` observer token. Disposing each token releases its notification registration.

## [04]-[IMPLEMENTATION_LAW]

[NATIVE_BOUNDARY]:

- `MacGate` admits the seam only after both the macOS process check and a valid active `Eto.Mac.Platform` check; installed AppKit types do not provide that application-level admission themselves.
- `IMacControlHandler` owns the five Eto-native view roles. `GetContainerView` owns nullable convenience extraction, and canvas extraction remains the explicit `Canvas.ControlObject as NSView` runtime branch.
- AppKit view, event, recognizer, pressure, screen, and workspace calls remain UI-affine. A retained monitor, recognizer, pressure configuration, notification token, display link, callback target, or native layer resource carries its exact inverse and disposal order.
- Anchor-screen pacing reads the screen hosting the active view and uses native `nint`, `double`, `NFloat`, and `float` carriers without widening or narrowing inside the boundary model.
- `CATransaction` owns mutation batching, `CADisplayLink` owns native pacing, `NSRunLoop` owns callback scheduling, and `Eto.Mac` owns managed/native conversion.

[RAIL_LAW]:

- Surface: installed `Microsoft.macOS.dll` and `Eto.macOS.dll` bindings
- Owns: AppKit extraction, ABI-faithful input, gesture and pressure attachment, display facts, accessibility observation, layer composition, display-link pacing, and native conversion
- Accept: explicit view roles, runtime-null validation, exact numeric carriers, paired native lifecycles, screen-local pacing, and installed conversion owners
- Reject: `IMacViewHandler.Control`, `NSScreen.MainScreen` as an anchor-display substitute, unpaired native retention, or a local conversion beside `Eto.Mac`
