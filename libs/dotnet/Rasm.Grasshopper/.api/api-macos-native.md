# [RASM_GRASSHOPPER_API_MACOS_NATIVE]

Installed `Microsoft.macOS.dll` bindings own the native subsystem beneath an Eto-hosted Grasshopper 2 canvas that the pacing core does not reach: `CoreAnimation` owns the composited layer graph and its animation values, `AppKit` owns the local event monitor, gesture and pressure recognizers, vibrancy chrome, and haptics, and `ScreenCaptureKit`, `CoreMedia`, and `CoreVideo` own leased capture and locked pixel-row egress. View-window-screen anchor chain, display link, run loop, accessibility gates, and object bridge are the branch pacing core this partition registers. Each retained native object carries its removal, invalidation, or disposal inverse.

## [01]-[PUBLIC_TYPES]

- Registers the macOS pacing core (`libs/dotnet/.api/api-macos-native.md`): the `NSView`-to-`NSWindow`-to-`NSScreen` anchor chain with its display facts, coordinate maps (`NSView.ConvertPointFromView`), and EDR headroom, `CADisplayLink`/`CAFrameRateRange`, `NSRunLoop`/`NSRunLoopMode`, the `NSWorkspace` accessibility gates, the screen and accessibility observation tokens, and the `Runtime` handle bridge carry their algebra there; the rows below are the subsystem this canvas boundary adds beyond it.
- `Eto.Mac` value bridge — `MacConversions`, `CGConversions`, `MacControlExtensions` — is tabled at member depth by `.api/api-eto-platform`, which owns the `Eto.macOS` partition; this catalog composes it and adds only the AppKit-side facts a conversion call site needs.

[PUBLIC_TYPE_SCOPE]: layer graph, animation, and filtering

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------- | :------------ | :------------------------------------- |
|  [01]   | `CALayer`; `CAShapeLayer`; `CAGradientLayer`; `CATextLayer`    | class         | composited layer graph                 |
|  [02]   | `CAReplicatorLayer`; `CAEmitterLayer`                          | class         | replicated and emitted layers          |
|  [03]   | `CABasicAnimation`; `CASpringAnimation`; `CAKeyFrameAnimation` | class         | native animation values                |
|  [04]   | `CAAnimationGroup`; `CAMediaTimingFunction`                    | class         | animation grouping and timing          |
|  [05]   | `CATransaction`                                                | class         | mutation batching                      |
|  [06]   | `CGPath`; `CGColor`; `CGAffineTransform`; `CGPoint`; `CGRect`  | family        | geometry, colour, and transform values |
|  [07]   | `CIFilter`                                                     | class         | named Core Image filter                |

[PUBLIC_TYPE_SCOPE]: local events, gestures, and pressure

| [INDEX] | [SYMBOL]                                                                    | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :-------------------------------------------------------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `NSEvent`; `NSEventMask`; `NSEventType`                                     | family        | local event monitoring and discriminant |
|  [02]   | `NSEventPhase`; `NSEventModifierMask`                                       | enum          | event phase and modifier ABI            |
|  [03]   | `NSGestureRecognizer`; `NSClickGestureRecognizer`; `NSPanGestureRecognizer` | class         | click and translation recognition       |
|  [04]   | `NSMagnificationGestureRecognizer`; `NSRotationGestureRecognizer`           | class         | magnification and rotation input        |
|  [05]   | `NSPressGestureRecognizer`; `NSGestureRecognizerState`                      | family        | press input and recognizer state        |
|  [06]   | `NSPressureConfiguration`; `NSPressureBehavior`                             | family        | pressure behaviour                      |

[PUBLIC_TYPE_SCOPE]: vibrancy chrome, styled text, and haptics

| [INDEX] | [SYMBOL]                                                                     | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :--------------------------------------------------------------------------- | :------------ | :---------------------------- |
|  [01]   | `NSVisualEffectView`; `NSVisualEffectMaterial`; `NSVisualEffectBlendingMode` | family        | AppKit vibrancy and blur      |
|  [02]   | `NSColor`; `NSColorSpace`; `NSFont`; `NSAttributedString`                    | class         | AppKit colour and styled text |
|  [03]   | `NSHapticFeedbackManager`; `NSHapticFeedbackPattern`                         | family        | haptic performance            |
|  [04]   | `NSHapticFeedbackPerformanceTime`                                            | enum          | haptic performance timing     |

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

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: layer attachment on the registered view

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :--------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `NSView.WantsLayer / Layer -> CALayer?`                    | property | backing-layer opt-in         |
|  [02]   | `NSView.MakeBackingLayer() -> CALayer`                     | instance | supply the backing layer     |
|  [03]   | `NSView.{AddGestureRecognizer, RemoveGestureRecognizer}`   | instance | recognizer attach and detach |
|  [04]   | `NSView.PressureConfiguration -> NSPressureConfiguration?` | property | per-view pressure config     |

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

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `NSGestureRecognizer(NSObject?, Selector?)`                          | ctor     | target and action or `Action` ctor |
|  [02]   | `NSGestureRecognizer.{Action, Target, State}`                        | property | callback and recognizer state      |
|  [03]   | `NSGestureRecognizer.LocationInView(NSView?) -> CGPoint`             | instance | hit location                       |
|  [04]   | `NSClickGestureRecognizer(Action)`                                   | ctor     | click and touch counts             |
|  [05]   | `NSPanGestureRecognizer(Action)`                                     | ctor     | translation and velocity           |
|  [06]   | `NSMagnificationGestureRecognizer.Magnification -> NFloat`           | property | magnification                      |
|  [07]   | `NSRotationGestureRecognizer.Rotation / RotationInDegrees -> NFloat` | property | rotation                           |
|  [08]   | `NSPressGestureRecognizer(Action)`                                   | ctor     | movement and duration              |
|  [09]   | `NSPressureConfiguration() / (NSPressureBehavior)`                   | ctor     | `PressureBehavior` get and `Set()` |

- `NSGestureRecognizer.PressureConfiguration` is non-null and settable, while `NSView.PressureConfiguration` is nullable and accepts null to remove a view configuration.

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
- `CAMediaTimingFunction` exposes the four-float control-point constructor, `FromName(NSString)`, and the five `NSString` name statics (`Default`, `EaseIn`, `EaseOut`, `EaseInEaseOut`, `Linear`); a transaction pairs `Begin()` with `Commit()` and `CompletionBlock` is nullable.
- `CASpringAnimation` carries the unit-mass spring columns (`Mass`, `Stiffness`, `Damping`, `InitialVelocity` — `float`) beside the inherited `From`/`To` (`NSObject`) and `Duration`; `CABasicAnimation.FromKeyPath(string)` mints per subclass.
- `CGColorSpace.CreateWithName(string)` with `CGColorSpaceNames.DisplayP3` mints the wide-gamut space, and `CGColor(CGColorSpace, NFloat[])` builds a component colour in it — the direct Display-P3 mint the wide-colour crossing prefers over `CGConversions.ToCG`'s silent-fallback arms.
- `NSView.GetDisplayLink(NSObject, Selector)` carries a `macos14.0` availability floor; a mount below it refuses at its own version gate rather than throwing native.
- `SCStreamConfiguration.PixelFormat` (`CVPixelFormatType`, `CV32BGRA` among its values), `CVPixelBuffer.IsPlanar`, and the single-window `SCContentFilter(SCWindow)` constructor complete the capture plumbing the session composes.

[ENTRYPOINT_SCOPE]: geometry, colour, and marshal at the layer edge

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]       |
| :-----: | :----------------------------------------------------------------- | :------- | :----------------- |
|  [01]   | `CGPath.{MoveToPoint, AddLineToPoint, AddRoundedRect}`             | instance | path building      |
|  [02]   | `CGAffineTransform.MakeIdentity() -> CGAffineTransform`            | factory  | identity transform |
|  [03]   | `NSColor.FromDisplayP3(NFloat, NFloat, NFloat, NFloat) -> NSColor` | factory  | Display-P3 colour  |

- `MacConversions.ToEto(CGPoint, NSView)` treats the point as window coordinates through `ConvertPointFromView(point, null)` and flips Y when the view is not flipped.
- `MacConversions.ToNS(CGColor)` returns runtime null for a null input; `CGConversions.ToCG(NSColor)` returns the colour in ITS OWN space on its primary arm, so a `FromDisplayP3` mint reaches `CALayer.BackgroundColor`/`BorderColor`/`FillColor`/`StrokeColor` wide-gamut intact, while its fallback arms re-space to sRGB or floor at opaque black without signalling — a wide-colour crossing asserts the returned `ColorSpace` instead of trusting the call.

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

[ENTRYPOINT_SCOPE]: vibrancy and haptics

- `NSVisualEffectView` carries `Material`, `BlendingMode`, `State`, and `EmphasizedAppearance`; the material is a semantic role the host re-resolves on an appearance flip, so a captured blur value stales at the flip.
- `NSHapticFeedbackManager.DefaultPerformer` performs an `NSHapticFeedbackPattern` at an `NSHapticFeedbackPerformanceTime`; a snap or alignment confirmation performs once at the commit, never per motion frame.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every retained native object carries its exact inverse and disposal order, and a native op never widens or narrows the `nint`, `double`, `NFloat`, or `float` carrier inside the boundary.
- `CATransaction` owns mutation batching over the layer graph, and a layer mount runs inside the registered anchor view's valid host lifetime.
- ScreenCaptureKit capture is leased: an opened `SCStream` pairs stop-capture, output removal, and disposal of stream, filter, configuration, sink, and delegate as one inverse chain; a delivered `CMSampleBuffer` never outlives its callback, and detached pixel rows are the only raster that crosses.
- Event monitoring, gesture recognition, and pressure configuration each attach to the registered anchor view and detach through their own inverse; a monitor token and its removal are distinct steps.

[STACKING]:
- `api-macos-native`(`libs/dotnet/.api/api-macos-native.md`): the registered pacing core — the anchor chain, display link, run loop, accessibility gates, observation tokens, and handle bridge every subsystem here runs against.
- `api-eto-platform`(`libs/dotnet/Rasm.Grasshopper/.api/api-eto-platform.md`): `GetContainerView` and `IMacControlHandler.ContainerControl` yield the Eto-backed `NSView` every layer mount and recognizer attach binds to, and the conversion owners carry values across the boundary.
- `api-thinktecture-runtime-extensions`(`libs/dotnet/.api/api-thinktecture-runtime-extensions.md`): the installed `NSEventType`, `NSEventPhase`, `SCStreamOutputType`, `SCContentFilterOption`, `NSVisualEffectMaterial`, and `CVReturn` enums map at the folder boundary onto `[SmartEnum]` owners, so an event or capture branch is exhaustive dispatch rather than a string compare.
- `api-languageext`(`libs/dotnet/.api/api-languageext.md`): the runtime-nullable native results — `CIFilter.FromName`, `CMSampleBuffer.GetImageBuffer`, `SCWindow.OwningApplication` — lower onto `Option<T>`/`Fin<T>` at the boundary, and throwing conversions stay caught boundaries on the same rail.
- Within-folder: the seam gate admits the boundary, the compositor mounts the layer graph with Display-P3 colour, the capture owner leases the stream into stamped frame rings, and motion pacing consumes the registered display link through one shared step fold.

[LOCAL_ADMISSION]:
- Seam admits only after the macOS process check and a valid active `Eto.Mac.Platform`; installed AppKit types carry no application-level admission themselves.
- Layer, recognizer, monitor, and capture work binds to the extracted container view; canvas extraction stays the explicit control-object branch and never a widened cast.
- Value crossings take the `Eto.Mac` conversion owners; a local conversion beside them is the deleted form.
