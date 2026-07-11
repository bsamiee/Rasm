# [RASM_GRASSHOPPER_API_GH2_FLEX]

The Grasshopper2 flex/animation substrate splits generic value interpolation (`Grasshopper2.UI.Animation`) from
control projection and input dispatch (`Grasshopper2.UI.Flex`). `IFlexControl` is the typed host seam for coordinate
mapping, viewport navigation, responsive registration, focus routing, redraw scheduling, draw timing, and `Animated<T>`
consumption. `MotionEquations.Blend` is the single easing evaluator; `Animated<T>` is the two-state animation carrier;
`Responses` folds every mouse/key/text event to a `Response` verdict. Motion pacing lives in `MotionEquations.Blend`
and `Animated<T>`, repaint in `IFlexControl.ScheduleRedraw` — there is no host pacer, spring, repaint-request, or
subscription carrier. Every member is catalog-verified against the installed RhinoWIP `Grasshopper2.xml`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Grasshopper2`
- package: `Grasshopper2` (Rhino 9 WIP host plug-in bundle; not a NuGet pin — the in-process `Grasshopper2.dll` under `Grasshopper2Plugin.rhp` is the resolved asset)
- assembly: `Grasshopper2`
- namespace: `Grasshopper2.UI.Animation`
- namespace: `Grasshopper2.UI.Flex`
- namespace: `Grasshopper2.UI` (`ZoomThreshold`)
- asset: host assembly; managed WIP plug-in loaded in the Rhino assembly-load context, animating over `Eto.Drawing`
- rail: host-grasshopper

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: animation value vocabulary
- namespace: `Grasshopper2.UI.Animation`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]          | [KIND]        | [CAPABILITY]                                                                                                                                         |
| :-----: | :---------------- | :------------ | :--------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Motion`          | enum          | the closed easing vocabulary: `Linear`/`LinearDelayed`/`EaseIn`/`EaseOut`/`EaseInOut`/`SnapIn`/`SnapOut`/`Bounce`/`Twang`                            |
|  [02]   | `Duration`        | enum          | the named animation span: `Abrupt`/`Brief`/`Fast`/`Normal`/`Slow`                                                                                    |
|  [03]   | `State`           | enum          | the animation lifecycle: `Pending`/`Busy`/`Finished`                                                                                                 |
|  [04]   | `MotionEquations` | static        | the native easing evaluator; `Blend(Motion, double)` maps a normalized time through an easing kind                                                   |
|  [05]   | `Animated<T>`     | value carrier | the two-state animation value; from/to over a span+motion (unfinished) or a settled value (finished), `Evaluate`/`Chain`/`Motion`/`State`/`ValueNow` |
|  [06]   | `Animators`       | static        | typed `Animated<T>` factories for `Color`/`PointF`/`RectangleF`; `DurationToTimeSpan` lowers a `Duration`                                            |
|  [07]   | `AnimatedPath`    | path          | named feedback-stroke set (error/warning/success/message/arrow) with time-parameterized `Draw`                                                       |
|  [08]   | `Interpolate<T>`  | delegate      | the per-`T` lerp `Animated<T>` composes for its value channel                                                                                        |
|  [09]   | `IAnimatedStroke` | interface     | one stroke element of an `AnimatedPath`                                                                                                              |

[PUBLIC_TYPE_SCOPE]: the IFlexControl seam
- namespace: `Grasshopper2.UI.Flex`, `Grasshopper2.UI`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]           | [KIND]    | [CAPABILITY]                                                                                                                                                      |
| :-----: | :----------------- | :-------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `IFlexControl`     | host seam | the typed control seam: coordinate `Map`, viewport `Navigate`, responsive registration + focus stack, `ScheduleRedraw`, draw timing, `Animate`, `FloatingButtons` |
|  [02]   | `FlexControl`      | control   | the concrete flex control the `ResponseMouseArgs` capture originates from                                                                                         |
|  [03]   | `CoordinateSystem` | enum      | the `Content` / `Control` coordinate frames `Map` converts between                                                                                                |
|  [04]   | `ContentPosition`  | position  | the named content-space anchor `Navigate` targets                                                                                                                 |
|  [05]   | `ZoomThreshold`    | struct    | the motion-gated zoom factor `AnimatedZoomFactor` resolves                                                                                                        |

[PUBLIC_TYPE_SCOPE]: response dispatch
- namespace: `Grasshopper2.UI.Flex`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]            | [KIND]    | [CAPABILITY]                                                                                                                              |
| :-----: | :------------------ | :-------- | :---------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Response`          | enum      | the dispatch verdict, precedence `Ignored` < `Release` < `Handled` < `Capture`                                                            |
|  [02]   | `IResponsive`       | interface | a hit-testable input target; `Responder` is its bound handler                                                                             |
|  [03]   | `ResponseMouseArgs` | args      | the mouse event in both frames; `ControlLocation`/`ContentLocation`, `Buttons`, `Modifiers`, `Delta`, `Pressure`, `Handled`, `Invalidate` |
|  [04]   | `Responses`         | static    | the mouse/key/text handler + relay family and the `RedrawRequired` signal                                                                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Animated<T> construction and evaluation
- namespace: `Grasshopper2.UI.Animation`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                   | [CALL_SHAPE]                                                    | [CAPABILITY]                                               |
| :-----: | :------------------------------------------ | :-------------------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `Animated<T>.CreateUnfinished`              | `(T from, T to, TimeSpan, Motion, Interpolate<T>)`              | starts an animation between two values                     |
|  [02]   | `Animated<T>.CreateFinished`                | `(T, Interpolate<T>)` / `(T, TimeSpan, Motion, Interpolate<T>)` | a settled value, optionally with a resolved curve          |
|  [03]   | `Animated<T>.Chain`                         | `(T, Duration, Motion)` → `Animated<T>`                         | appends the next leg to the current value                  |
|  [04]   | `Animated<T>.Evaluate` / `EvaluateThis`     | `(DateTime)` → `T`                                              | samples the value at a clock time                          |
|  [05]   | `Animated<T>.Motion` / `State` / `ValueNow` | property                                                        | the active easing kind, lifecycle state, and current value |

[ENTRYPOINT_SCOPE]: easing and typed animators
- namespace: `Grasshopper2.UI.Animation`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                        | [CAPABILITY]                                  |
| :-----: | :----------------------------- | :-------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `MotionEquations.Blend`        | `(Motion, double)` → `double`                       | maps a normalized time through an easing kind |
|  [02]   | `Animators.DurationToTimeSpan` | `(Duration)` → `TimeSpan`                           | resolves a named span to a concrete duration  |
|  [03]   | `Animators.Finished`           | `(Color \| PointF \| RectangleF, Duration, Motion)` | a settled typed `Animated<T>`                 |
|  [04]   | `Animators.Unfinished`         | `(from, to, Duration, Motion)` per typed overload   | an animating typed `Animated<T>`              |

[ENTRYPOINT_SCOPE]: AnimatedPath feedback factories
- namespace: `Grasshopper2.UI.Animation`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                                               | [CALL_SHAPE]                                                  | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------------------------- | :------------------------------------------------------------ | :----------------------------------- |
|  [01]   | `AnimatedPath.CreateErrorPath` / `Warning` / `Success` / `Message`      | `(float)` → `AnimatedPath`                                    | the semantic notice-glyph factories  |
|  [02]   | `AnimatedPath.CreateArrowPath`                                          | `(float, float)` → `AnimatedPath`                             | a directional arrow glyph            |
|  [03]   | `new AnimatedPath`                                                      | `(IEnumerable<IAnimatedStroke>)`                              | a custom stroke set                  |
|  [04]   | `AnimatedPath.AddGap` / `AddLine` / `AddLines` / `AddCircle` / `AddArc` | `(…)` primitive overloads                                     | appends a stroke primitive           |
|  [05]   | `AnimatedPath.Draw`                                                     | `(Graphics, Pen, double t, [double], PointF, [float, float])` | strokes the path to a time parameter |

[ENTRYPOINT_SCOPE]: IFlexControl coordinate, navigation, and redraw seam
- namespace: `Grasshopper2.UI.Flex`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                                    | [CALL_SHAPE]                                                                                                    | [CAPABILITY]                                                                  |
| :-----: | :----------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `IFlexControl.Map`                                           | `(PointF \| RectangleF, CoordinateSystem, CoordinateSystem)`                                                    | converts a point/rect between content and control frames                      |
|  [02]   | `IFlexControl.Navigate`                                      | `(ContentPosition, Duration)` / `(PointF, (float, float), Duration)` / `(RectangleF, (float, float), Duration)` | animates the viewport to an anchor/point/rect                                 |
|  [03]   | `IFlexControl.BeginWindowSelect` / `EndWindowSelect`         | `()`                                                                                                            | opens / closes the marquee-selection interaction                              |
|  [04]   | `IFlexControl.PushFocus` / `PopFocus`                        | `(IResponsive)`                                                                                                 | routes input capture onto / off the focus stack                               |
|  [05]   | `IFlexControl.RegisterIResponsive` / `UnregisterIResponsive` | `(IResponsive)`                                                                                                 | admits / removes a hit-testable target; `ResponsivesForwards` enumerates them |
|  [06]   | `IFlexControl.ScheduleRedraw`                                | `()` / `(TimeSpan)`                                                                                             | requests an immediate or deferred repaint                                     |
|  [07]   | `IFlexControl.Animate<T>`                                    | `(Animated<T>)`                                                                                                 | drives an animation on the control clock                                      |
|  [08]   | `IFlexControl.AnimatedZoomFactor`                            | `(ZoomThreshold)` → factor                                                                                      | resolves the motion-gated zoom factor                                         |
|  [09]   | `IFlexControl.Draw` / `DrawStartTime` / `DrawEndTime`        | `event` / property                                                                                              | the per-frame draw signal and its timing window                               |
|  [10]   | `IFlexControl.FloatingButtons`                               | property                                                                                                        | the control's floating-button collection (`api-gh2-editor.md`)                |

[ENTRYPOINT_SCOPE]: response dispatch
- namespace: `Grasshopper2.UI.Flex`
- rail: host-grasshopper

Each handler returns a `Response`; the relay forms thread a caller handler through the args and fold the verdict.

| [INDEX] | [SURFACE]                                                                | [CALL_SHAPE]                                        | [CAPABILITY]                                                       |
| :-----: | :----------------------------------------------------------------------- | :-------------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | `Responses.MouseOver` / `Down` / `Drag` / `Up` / `Wheel`                 | `(ResponseMouseArgs)` → `Response`                  | the pointer-event handlers                                         |
|  [02]   | `Responses.MouseSingleClick` / `MouseDoubleClick` / `MouseLeave`         | `(ResponseMouseArgs)` / `()` → `Response`           | the click and leave handlers                                       |
|  [03]   | `Responses.KeyDown` / `KeyUp` / `TextInput`                              | `(KeyEventArgs \| TextInputEventArgs)` → `Response` | the keyboard/text handlers                                         |
|  [04]   | `Responses.InvokeMouseRelay` / `InvokeKeyRelay` / `InvokeTextInputRelay` | `(Func<Args, Response>, Args)` → `Response`         | routes an event through a caller handler                           |
|  [05]   | `Responses.RedrawRequired` / `OnRedrawRequired`                          | `event` / `()`                                      | the handler-raised repaint signal                                  |
|  [06]   | `new ResponseMouseArgs`                                                  | `(FlexControl, MouseEventArgs)`                     | captures a mouse event in both frames; `Invalidate` marks a redraw |

## [04]-[IMPLEMENTATION_LAW]

[FLEX_TOPOLOGY]:
- two namespaces, one seam: `Grasshopper2.UI.Animation` is generic value interpolation and `Grasshopper2.UI.Flex` is control projection + dispatch; they meet where `IFlexControl.Animate` consumes an `Animated<T>`
- `Animated<T>` is a two-state curve: `CreateUnfinished(from, to, span, motion, interpolate)` animates and `CreateFinished` holds a settled value; `Evaluate(DateTime)` samples, `State` reports `Pending`/`Busy`/`Finished`, and `Chain` appends the next leg
- `MotionEquations.Blend(Motion, double)` is the single easing evaluator every `Animated<T>` and `Animators` factory routes through; `Motion` is the closed nine-kind easing vocabulary and `Duration` the five-step named span
- response dispatch is a folded verdict: each `Responses` handler returns a `Response` under `Ignored` < `Release` < `Handled` < `Capture` precedence, and the `Invoke*Relay` forms thread a caller handler through the typed args
- `IFlexControl` is the coordinate authority: `Map` converts between `Content` and `Control` frames, `Navigate` animates the viewport to a `ContentPosition`/point/rect over a `Duration`, and the focus stack (`PushFocus`/`PopFocus`) routes capture
- `AnimatedPath` models a named feedback-stroke set with time-parameterized `Draw` — the notice/overlay glyph the canvas and chrome compose

[STACKING]:
- `api-thinktecture-runtime-extensions.md`(`.api/api-thinktecture-runtime-extensions.md`): `Motion`, `Duration`, `State`, and `Response` lower onto `SmartEnum` owners; the easing kind and dispatch verdict become closed generated vocabularies
- `api-languageext.md`(`.api/api-languageext.md`): a `Response`-returning handler folds through a `Fin`-shaped verdict, `ScheduleRedraw`/`Animate` ride `Eff`, `Animated<T>.Evaluate` yields a pure sample, and the responsive registry is a `Seq`/`HashMap`
- `api-unicolour.md`(`.api/api-unicolour.md`): `Animators.Finished`/`Unfinished(Color, …)` blends the `Eto.Drawing.Color` endpoints in a perceptual space
- Rasm kernel: the easing/interpolation math the host `Motion` enum names composes the kernel motion owner (`MotionInterpolation`), never a second in-folder easing derivation

[LOCAL_ADMISSION]:
- animation enters through `Animated<T>` + `Animators`; pacing is `MotionEquations.Blend`, never a hand-rolled tween loop
- redraw is `IFlexControl.ScheduleRedraw`; there is no host repaint-request or subscription object to carry it
- coordinate conversion is `IFlexControl.Map`; a parallel content/control transform is the deleted form

[RAIL_LAW]:
- Package: `Grasshopper2` (host assembly)
- Owns: the `IFlexControl` coordinate/redraw/navigation seam, the animation value vocabulary, the easing evaluator, `Animated<T>`, `AnimatedPath` feedback factories, and the response mouse/key/text dispatch family
- Accept: value animation, viewport navigation, coordinate mapping, redraw scheduling, responsive registration, event dispatch
- Reject: canvas paint composition (`api-gh2-canvas.md`), floating-button chrome (`api-gh2-editor.md`), a host pacer/spring/subscription carrier, the GH1 event idiom
