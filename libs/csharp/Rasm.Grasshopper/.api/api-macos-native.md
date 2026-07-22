# [RASM_GRASSHOPPER_API_MACOS_NATIVE]

Installed `Microsoft.macOS.dll` bindings own the native surface beneath an Eto-hosted Grasshopper 2 canvas: `AppKit` owns views, input, accessibility, and notifications; `CoreAnimation` owns layers and display links; `ScreenCaptureKit`, `CoreMedia`, and `CoreVideo` own capture and raster egress; `Foundation` owns native lifetimes. `Eto.macOS.dll` owns AppKit extraction and value conversion across the managed boundary. Each retained native object carries its removal, invalidation, or disposal inverse.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: installed macOS bindings
- host: `Grasshopper2` inside the Rhino WIP macOS process
- assemblies: `Microsoft.macOS.dll`; `Eto.macOS.dll`
- namespaces: `AppKit`, `CoreAnimation`, `CoreGraphics`, `CoreImage`, `Foundation`, `ObjCRuntime`, `Eto.Mac`, `Eto.Mac.Forms`
- platform: `Eto.Mac.Platform`; `IMacControlHandler`; `MacControlExtensions`
- rail: UI-affine native interop

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: views, screens, events, gestures, and pressure

| [INDEX] | [SYMBOL]                                                                    | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :-------------------------------------------------------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `NSView`; `NSWindow`; `NSScreen`                                            | class         | native view and hosting-display state   |
|  [02]   | `NSEvent`; `NSEventMask`; `NSEventType`                                     | family        | local event monitoring and discriminant |
|  [03]   | `NSEventPhase`; `NSEventModifierMask`                                       | enum          | event phase and modifier ABI            |
|  [04]   | `NSGestureRecognizer`; `NSClickGestureRecognizer`; `NSPanGestureRecognizer` | class         | click and translation recognition       |
|  [05]   | `NSMagnificationGestureRecognizer`; `NSRotationGestureRecognizer`           | class         | magnification and rotation input        |
|  [06]   | `NSPressGestureRecognizer`; `NSGestureRecognizerState`                      | family        | press input and recognizer state        |
|  [07]   | `NSPressureConfiguration`; `NSPressureBehavior`                             | family        | pressure behavior                       |

[PUBLIC_TYPE_SCOPE]: layer graph, timing, and conversion

| [INDEX] | [SYMBOL]                                                                  | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------------ | :------------ | :----------------------------------------- |
|  [01]   | `CALayer`; `CAShapeLayer`; `CAGradientLayer`; `CATextLayer`               | class         | composited layer graph                     |
|  [02]   | `CAReplicatorLayer`; `CAEmitterLayer`                                     | class         | replicated and emitted layers              |
|  [03]   | `CABasicAnimation`; `CASpringAnimation`; `CAKeyFrameAnimation`            | class         | native animation values                    |
|  [04]   | `CAAnimationGroup`; `CAMediaTimingFunction`                               | class         | animation grouping and timing              |
|  [05]   | `CATransaction`; `CADisplayLink`; `CAFrameRateRange`                      | family        | mutation transaction and display pacing    |
|  [06]   | `CGPath`; `CGColor`; `CGAffineTransform`; `CGPoint`; `CGRect`; `CIFilter` | family        | geometry, colour, transform, and filtering |
|  [07]   | `NSObject`; `NSString`; `NSRunLoop`; `NSRunLoopMode`                      | family        | Objective-C object and run-loop            |
|  [08]   | `Selector`; `ExportAttribute`; `Runtime`                                  | family        | selector export and object marshal         |
|  [09]   | `MacConversions`; `CGConversions`; `MacControlExtensions`                 | static        | Eto extraction and conversion              |
|  [10]   | `IMacControlHandler`; `IMacViewHandler`                                   | interface     | native view roles                          |

[PUBLIC_TYPE_SCOPE]: chrome, accessibility, and observation

| [INDEX] | [SYMBOL]                                                                     | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------------- | :------------ | :------------------------------------ |
|  [01]   | `NSVisualEffectView`; `NSVisualEffectMaterial`; `NSVisualEffectBlendingMode` | family        | AppKit vibrancy and blur              |
|  [02]   | `NSColor`; `NSColorSpace`; `NSFont`; `NSAttributedString`                    | class         | AppKit colour and styled text         |
|  [03]   | `NSHapticFeedbackManager`; `NSHapticFeedbackPattern`                         | family        | haptic performance                    |
|  [04]   | `NSHapticFeedbackPerformanceTime`                                            | enum          | haptic performance timing             |
|  [05]   | `NSWorkspace`; `NSApplication`; `NSNotificationEventArgs`                    | class         | accessibility and display observation |

[PUBLIC_TYPE_SCOPE]: display and window capture

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------- | :------------ | :----------------------------------- |
|  [01]   | `SCShareableContent`; `SCDisplay`; `SCWindow`; `SCRunningApplication` | family        | shareable-content enumeration        |
|  [02]   | `SCContentFilter`; `SCContentFilterOption`                            | family        | display and window capture filters   |
|  [03]   | `SCStream`; `SCStreamConfiguration`; `SCStreamOutputType`             | family        | leased frame streaming               |
|  [04]   | `ISCStreamOutput`; `ISCStreamDelegate`; `SCFrameStatus`               | family        | frame delivery and stop protocols    |
|  [05]   | `SCScreenshotManager`; `SCStreamFrameInfoKeys`                        | family        | one-shot capture and frame-info keys |
|  [06]   | `SCRecordingOutput`; `SCRecordingOutputConfiguration`                 | family        | file recording over the same stream  |
|  [07]   | `CMSampleBuffer`; `CMTime`                                            | family        | delivered sample and capture timing  |
|  [08]   | `CVImageBuffer`; `CVPixelBuffer`; `CVPixelBufferLock`; `CVReturn`     | family        | locked pixel-row raster egress       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Eto-to-AppKit extraction

| [INDEX] | [SURFACE]                                                                           | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `MacControlExtensions.GetMacControl(Control) -> IMacControlHandler?`                | static   | nullable handler extraction      |
|  [02]   | `MacControlExtensions.GetMacViewHandler(Control) -> IMacViewHandler?`               | static   | nullable view-handler extraction |
|  [03]   | `MacControlExtensions.GetContainerView(Widget) -> NSView?`                          | static   | nested container-view extraction |
|  [04]   | `IMacControlHandler.{Container, Content, Event, Focus, TextInput}Control -> NSView` | property | five AppKit view roles           |

- `GetMacControl`, `GetMacViewHandler`, and `GetContainerView`: non-null signatures returning runtime null for an absent control or handler; `GetContainerView` follows nested Eto controls and finally admits a direct `NSView`.
- `IMacViewHandler` inherits the five roles and declares no `Control`; `IMacWindow.Control` is the separate `NSWindow` window-handler property, and `Canvas.ControlObject` yields a native anchor only when it is an `NSView`.

[ENTRYPOINT_SCOPE]: view, window, and screen

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                |
| :-----: | :--------------------------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `NSView.Window -> NSWindow?`                                                 | property | hosting window              |
|  [02]   | `NSView.WantsLayer / Layer -> CALayer?`                                      | property | backing-layer opt-in        |
|  [03]   | `NSView.PressureConfiguration -> NSPressureConfiguration?`                   | property | per-view pressure config    |
|  [04]   | `NSView.{MakeBackingLayer, AddGestureRecognizer, RemoveGestureRecognizer}`   | instance | layer and recognizer attach |
|  [05]   | `NSView.ConvertPointFromView(CGPoint, NSView?) -> CGPoint`                   | instance | window-relative point map   |
|  [06]   | `NSView.GetDisplayLink(NSObject, Selector) -> CADisplayLink`                 | instance | view-bound vsync source     |
|  [07]   | `NSWindow.Screen -> NSScreen`                                                | property | hosting display             |
|  [08]   | `NSScreen.MainScreen / Screens -> NSScreen[]`                                | static   | display enumeration         |
|  [09]   | `NSScreen.MaximumFramesPerSecond -> nint`                                    | property | refresh ceiling             |
|  [10]   | `NSScreen.MinimumRefreshInterval / MaximumRefreshInterval -> double`         | property | refresh-interval bounds     |
|  [11]   | `NSScreen.MaximumExtendedDynamicRangeColorComponentValue -> NFloat`          | property | EDR headroom                |
|  [12]   | `NSScreen.MaximumPotentialExtendedDynamicRangeColorComponentValue -> NFloat` | property | EDR potential headroom      |
|  [13]   | `NSScreen.MaximumReferenceExtendedDynamicRangeColorComponentValue -> NFloat` | property | EDR reference headroom      |
|  [14]   | `NSScreen.GetDisplayLink(NSObject, Selector) -> CADisplayLink`               | instance | screen-bound vsync source   |

- `CADisplayLink`, `NSView.GetDisplayLink`, and `NSScreen.GetDisplayLink` carry `SupportedOSPlatform("macos14.0")` and declare non-null while the native result still needs runtime validation.
- `NSWindow.Screen` resolves to native null before the window belongs to a screen; a view-bound pacing decision reads `view.Window?.Screen`, never `NSScreen.MainScreen`, which describes the application main screen.

[ENTRYPOINT_SCOPE]: local event monitor ABI

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]          |
| :-----: | :---------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `NSEvent.Type -> NSEventType`                               | property | event discriminant    |
|  [02]   | `NSEvent.Phase / MomentumPhase -> NSEventPhase`             | property | scroll phase          |
|  [03]   | `NSEvent.ModifierFlags -> NSEventModifierMask`              | property | modifier mask         |
|  [04]   | `NSEvent.KeyCode -> ushort`                                 | property | key code              |
|  [05]   | `NSEvent.ScrollingDeltaX / ScrollingDeltaY -> NFloat`       | property | scroll delta          |
|  [06]   | `NSEvent.Magnification / StageTransition -> NFloat`         | property | zoom and stage        |
|  [07]   | `NSEvent.Rotation / Pressure / TangentialPressure -> float` | property | rotation and pressure |
|  [08]   | `NSEvent.Stage -> nint`                                     | property | pressure stage        |

- `NSEvent.AddLocalMonitorForEventsMatchingMask(NSEventMask, LocalEventHandler)` returns an `NSObject` token; `LocalEventHandler` is `NSEvent(NSEvent)`, and `RemoveMonitor(NSObject)` removes it.
- Returning the original event preserves delivery; returning runtime null absorbs it despite the non-null return annotation, and removal and token disposal are distinct inverse steps.

[ENTRYPOINT_SCOPE]: gesture and pressure ABI

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `NSGestureRecognizer(NSObject?, Selector?)`                          | ctor     | target/action or `Action` ctor   |
|  [02]   | `NSGestureRecognizer.{Action, Target, State}`                        | property | callback and recognizer state    |
|  [03]   | `NSGestureRecognizer.LocationInView(NSView?) -> CGPoint`             | instance | hit location                     |
|  [04]   | `NSClickGestureRecognizer(Action)`                                   | ctor     | click and touch counts           |
|  [05]   | `NSPanGestureRecognizer(Action)`                                     | ctor     | translation and velocity         |
|  [06]   | `NSMagnificationGestureRecognizer.Magnification -> NFloat`           | property | magnification                    |
|  [07]   | `NSRotationGestureRecognizer.Rotation / RotationInDegrees -> NFloat` | property | rotation                         |
|  [08]   | `NSPressGestureRecognizer(Action)`                                   | ctor     | movement and duration            |
|  [09]   | `NSPressureConfiguration() / (NSPressureBehavior)`                   | ctor     | `PressureBehavior` get + `Set()` |

- `NSGestureRecognizer.State` is public get/set, but the generated binding sets `State` only from recognizer subclasses; `View` is non-null yet runtime-null before attachment.
- `NSGestureRecognizer.PressureConfiguration` is non-null and settable, while `NSView.PressureConfiguration` is nullable and accepts null to remove a view configuration.

[ENTRYPOINT_SCOPE]: display link and run loop

| [INDEX] | [SURFACE]                                                                            | [SHAPE]  | [CAPABILITY]             |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :----------------------- |
|  [01]   | `CADisplayLink.{Duration, Timestamp, TargetTimestamp} -> double`                     | property | frame timing             |
|  [02]   | `CADisplayLink.{Paused, PreferredFrameRateRange}`                                    | property | pause and rate range     |
|  [03]   | `CADisplayLink.{AddToRunLoop, RemoveFromRunLoop, Invalidate}`                        | instance | loop attach and teardown |
|  [04]   | `CADisplayLink.Create(NSObject, Selector) -> CADisplayLink`                          | factory  | target/selector link     |
|  [05]   | `CAFrameRateRange.{Minimum, Maximum, Preferred} -> float`                            | property | mutable rate fields      |
|  [06]   | `CAFrameRateRange.Create(float, float, float) -> CAFrameRateRange`                   | factory  | rate-range value         |
|  [07]   | `NSRunLoop.{Main, Current} -> NSRunLoop`                                             | static   | run-loop handles         |
|  [08]   | `NSRunLoopMode.{Default, Common, ConnectionReply, ModalPanel, EventTracking, Other}` | static   | typed loop modes         |

- `CADisplayLink.{AddToRunLoop, RemoveFromRunLoop}` each expose `(NSRunLoop, NSString)` and `(NSRunLoop, NSRunLoopMode)` overloads; `NSRunLoop.Main` with `NSRunLoopMode.Common` is the typed common-mode attachment.
- `NSView.GetDisplayLink` and `NSScreen.GetDisplayLink` bind the link to a display source; teardown removes the link from the same loop and mode, invalidates it, then disposes link and callback target.

[ENTRYPOINT_SCOPE]: layer, animation, and filter state

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]            |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :---------------------- |
|  [01]   | `CALayer.{Frame, BorderWidth, CornerRadius, MasksToBounds}`                           | property | non-null layout values  |
|  [02]   | `CALayer.{BackgroundColor, BorderColor, Mask, Sublayers, Filters, BackgroundFilters}` | property | nullable composition    |
|  [03]   | `CALayer.{AddSublayer, RemoveFromSuperLayer}`                                         | instance | sublayer graph          |
|  [04]   | `CALayer.AddAnimation(CAAnimation, string?) / RemoveAnimation(string)`                | instance | keyed animation         |
|  [05]   | `CAShapeLayer.{Path, FillColor, StrokeColor}`                                         | property | nullable shape paint    |
|  [06]   | `CAShapeLayer.{LineWidth, LineCap, LineJoin, CapRound, JoinRound}`                    | property | non-null stroke         |
|  [07]   | `CATransaction.{DisableActions, CompletionBlock, Begin, Commit}`                      | static   | mutation batch          |
|  [08]   | `CIFilter.FromName(string) -> CIFilter? / Name / Copy(NSZone?)`                       | factory  | nullable filter by name |

- `CABasicAnimation.FromKeyPath(string?)` and `CAKeyFrameAnimation.FromKeyPath(string?)` are nullable-key factories; `CAKeyFrameAnimation.GetFromKeyPath(string)` is the non-null-key wrapper.
- `CAMediaTimingFunction` exposes the four-float control-point constructor and `FromName(NSString)`; a transaction pairs `Begin()` with `Commit()` and `CompletionBlock` is nullable.

[ENTRYPOINT_SCOPE]: geometry, colour, marshal, and Eto conversions

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]             |
| :-----: | :----------------------------------------------------------------- | :------- | :----------------------- |
|  [01]   | `CGPath.{MoveToPoint, AddLineToPoint, AddRoundedRect}`             | instance | path building            |
|  [02]   | `CGAffineTransform.MakeIdentity() -> CGAffineTransform`            | factory  | identity transform       |
|  [03]   | `NSColor.FromDisplayP3(NFloat, NFloat, NFloat, NFloat) -> NSColor` | factory  | Display-P3 color         |
|  [04]   | `Runtime.GetNSObject<T>(nint) -> T?`                               | static   | handle-to-object marshal |
|  [05]   | `MacConversions.{ToNSUI, ToNS, ToCG, ToEto}`                       | static   | Eto/AppKit value bridge  |
|  [06]   | `CGConversions.{ToCG, ToEto}`                                      | static   | Eto/CoreGraphics bridge  |

- `MacConversions.ToEto(CGPoint, NSView)` treats the point as window coordinates through `ConvertPointFromView(point, null)` and flips Y when the view is not flipped.
- `MacConversions.ToNS(CGColor)` and `CGConversions.ToCG(NSColor)` return runtime null for null inputs; `CGConversions.ToCG(IMatrix)` returns identity for a null matrix; `ToEto(CGColor)` throws for unsupported component layouts, and conversion ownership stays in `Eto.Mac`.

[ENTRYPOINT_SCOPE]: screen capture and raster egress

| [INDEX] | [SURFACE]                                                                                             | [SHAPE]  | [CAPABILITY]       |
| :-----: | :---------------------------------------------------------------------------------------------------- | :------- | :----------------- |
|  [01]   | `SCShareableContent.GetShareableContentAsync(bool, bool) -> Task<SCShareableContent>`                 | static   | enumerate content  |
|  [02]   | `SCShareableContent.{Displays, Windows, Applications}`                                                | property | content rosters    |
|  [03]   | `SCDisplay.{DisplayId, Frame, Width, Height}`                                                         | property | display geometry   |
|  [04]   | `SCWindow.{WindowId, Frame, Title, OnScreen, Active}`                                                 | property | window facts       |
|  [05]   | `SCWindow.OwningApplication -> SCRunningApplication?`                                                 | property | owning app         |
|  [06]   | `SCRunningApplication.{ApplicationName, BundleIdentifier, ProcessId}`                                 | property | app identity       |
|  [07]   | `SCContentFilter(SCDisplay, SCWindow[], SCContentFilterOption)`                                       | ctor     | capture filter     |
|  [08]   | `SCStreamConfiguration.{Width, Height, MinimumFrameInterval, QueueDepth}`                             | property | stream sizing      |
|  [09]   | `SCStreamConfiguration.{ShowsCursor, SourceRect}`                                                     | property | cursor and crop    |
|  [10]   | `SCStream(SCContentFilter, SCStreamConfiguration, ISCStreamDelegate?)`                                | ctor     | leased stream      |
|  [11]   | `SCStream.AddStreamOutput(ISCStreamOutput, SCStreamOutputType, DispatchQueue?, out NSError?) -> bool` | instance | attach frame sink  |
|  [12]   | `SCStream.RemoveStreamOutput(ISCStreamOutput, SCStreamOutputType, out NSError?) -> bool`              | instance | detach frame sink  |
|  [13]   | `SCStream.StartCapture(Action<NSError>?) / StopCapture(Action<NSError>?)`                             | instance | capture lifecycle  |
|  [14]   | `SCStream.UpdateConfigurationAsync(SCStreamConfiguration) -> Task`                                    | instance | live reconfigure   |
|  [15]   | `SCStream.UpdateContentFilterAsync(SCContentFilter) -> Task`                                          | instance | live refilter      |
|  [16]   | `SCScreenshotManager.CaptureImageAsync(SCContentFilter, SCStreamConfiguration)`                       | static   | one-shot `CGImage` |
|  [17]   | `SCScreenshotManager.CaptureSampleBufferAsync(SCContentFilter, SCStreamConfiguration)`                | static   | one-shot sample    |
|  [18]   | `CMSampleBuffer.{IsValid, PresentationTimeStamp} / GetImageBuffer() -> CVImageBuffer?`                | instance | sample validity    |
|  [19]   | `CMTime.FromSeconds(double, int) -> CMTime / Seconds -> double`                                       | factory  | capture timing     |
|  [20]   | `CVPixelBuffer.{Width, Height, BytesPerRow, BaseAddress} -> nint`                                     | property | raster geometry    |
|  [21]   | `CVPixelBuffer.PixelFormatType -> CVPixelFormatType`                                                  | property | pixel format       |
|  [22]   | `CVPixelBuffer.Lock(CVPixelBufferLock) / Unlock(CVPixelBufferLock) -> CVReturn`                       | instance | pixel-row lock     |

- `ISCStreamOutput.DidOutputSampleBuffer(SCStream, CMSampleBuffer, SCStreamOutputType)` binds `stream:didOutputSampleBuffer:ofType:`; `ISCStreamDelegate.DidStop(SCStream, NSError)` binds `stream:didStopWithError:` and `UserDidStop(SCStream)` binds `userDidStopStream:`; optional protocol members live on an `NSObject` subclass under matching `[Export]`.
- `SCStreamOutputType`, `SCContentFilterOption`, `SCFrameStatus`, and `CVPixelBufferLock` close their installed enum rows, and `CVReturn.Success` is the zero verdict.

[ENTRYPOINT_SCOPE]: accessibility and display observation
- `NSWorkspace.SharedWorkspace` exposes `AccessibilityDisplayShouldDifferentiateWithoutColor`, `AccessibilityDisplayShouldIncreaseContrast`, `AccessibilityDisplayShouldInvertColors`, `AccessibilityDisplayShouldReduceMotion`, and `AccessibilityDisplayShouldReduceTransparency`.
- `NSWorkspace.Notifications.ObserveDisplayOptionsDidChange` and `NSApplication.Notifications.ObserveDidChangeScreenParameters` each expose unfiltered and `(NSObject objectToObserve)` overloads returning an `NSObject` observer token; disposing the token releases its registration.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every retained native object carries its exact inverse and disposal order, and a native op never widens or narrows the `nint`, `double`, `NFloat`, or `float` carrier inside the boundary.
- `CATransaction` owns mutation batching, `CADisplayLink` owns native pacing, `NSRunLoop` owns callback scheduling, `Eto.Mac` owns managed/native conversion, and anchor-screen pacing reads the display hosting the active view.
- ScreenCaptureKit capture is leased: an opened `SCStream` pairs stop-capture, output removal, and disposal of stream, filter, configuration, sink, and delegate as one inverse chain; a delivered `CMSampleBuffer` never outlives its callback, and detached pixel rows are the only raster that crosses.

[STACKING]:
- `Eto` platform substrate (`.api/api-eto-platform`): `GetContainerView` and `IMacControlHandler.ContainerControl` yield the Eto-backed `NSView`, and every AppKit call, `CALayer` mount, and `CADisplayLink` pace runs inside that view's valid host lifetime while `MacConversions`/`CGConversions` carry values across the boundary.
- `Thinktecture.Runtime.Extensions` (`.api/api-thinktecture-runtime-extensions`): the installed `NSEventType`, `NSEventPhase`, `NSRunLoopMode`, `SCStreamOutputType`, `SCContentFilterOption`, and `CVReturn` enums map at the folder boundary onto `[SmartEnum]` owners, so an event or capture branch is exhaustive dispatch rather than an `NSString` compare.
- `LanguageExt.Core` (`.api/api-languageext`): the runtime-nullable extractions and native results — `GetMacControl`, `GetContainerView`, `NSWindow.Screen`, `GetDisplayLink`, and `CMSampleBuffer.GetImageBuffer` — lower onto `Option<T>`/`Fin<T>` at the boundary, and throwing conversions stay a caught boundary on the same rail.
- within-lib: `MacGate` admits the seam; `Compose` mounts the `CALayer`/`CAGradientLayer` graph with Display-P3 colour; `SessionCapture` leases the `SCStream`/`SCScreenshotManager` capture into stamped frame rings; motion pacing consumes `CADisplayLink` through the one shared step fold.

[LOCAL_ADMISSION]:
- `MacGate` admits the seam only after the macOS process check and a valid active `Eto.Mac.Platform`; installed AppKit types carry no application-level admission themselves.
- `IMacControlHandler` owns the five Eto-native view roles; `GetContainerView` owns nullable convenience extraction, and canvas extraction stays the explicit `Canvas.ControlObject as NSView` branch.

[RAIL_LAW]:
- Package: `Microsoft.macOS.dll`; `Eto.macOS.dll`
- Owns: AppKit extraction, ABI-faithful input, gesture and pressure attachment, display facts, accessibility observation, layer composition, display-link pacing, screen and window capture, and native conversion
- Accept: explicit view roles, runtime-null validation, exact numeric carriers, paired native lifecycles, screen-local pacing, ScreenCaptureKit capture with locked pixel egress, and the installed conversion owners
- Reject: `IMacViewHandler.Control`, `NSScreen.MainScreen` as an anchor-display substitute, unpaired native retention, or a local conversion beside `Eto.Mac`
