# [RASM_GRASSHOPPER_API_GH2_FLEX]

`Grasshopper2.UI.Flex` and `Grasshopper2.UI.Animation` own the canvas motion substrate. `IFlexControl` is the typed host seam for coordinate mapping, viewport navigation, focus and redraw scheduling, event dispatch, and `Animated<T>` consumption; `MotionEquations.Blend` is the sole easing evaluator every animation routes through, and `Responses` folds each mouse, key, text, and rotation event to a `Response` verdict. Pacing rides `MotionEquations.Blend` and `Animated<T>`, repaint rides `IFlexControl.ScheduleRedraw`; no host pacer, spring, or subscription carrier exists.

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

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :---------------- | :------------ | :-------------------------------- |
|  [01]   | `Motion`          | enum          | base and delayed easing kinds     |
|  [02]   | `Duration`        | enum          | named spans; value equals ms      |
|  [03]   | `State`           | enum          | pending, busy, finished           |
|  [04]   | `MotionEquations` | static        | normalized easing evaluation      |
|  [05]   | `Animated<T>`     | value carrier | endpoint, time, chain, and sample |
|  [06]   | `Animators`       | static        | typed animation factories         |
|  [07]   | `AnimatedPath`    | class         | feedback-stroke draw set          |
|  [08]   | `Interpolate<T>`  | delegate      | per-value interpolation           |
|  [09]   | `IAnimatedStroke` | interface     | one animated-path stroke          |

[PUBLIC_TYPE_SCOPE]: the IFlexControl seam
- namespace: `Grasshopper2.UI.Flex`, `Grasshopper2.UI`

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :----------------- | :------------ | :------------------------------- |
|  [01]   | `IFlexControl`     | interface     | projection, dispatch, and redraw |
|  [02]   | `FlexControl`      | class         | concrete response source         |
|  [03]   | `CoordinateSystem` | enum          | content and control frames       |
|  [04]   | `ContentPosition`  | enum          | named navigation anchor          |
|  [05]   | `ZoomThreshold`    | enum          | animated zoom threshold          |

[PUBLIC_TYPE_SCOPE]: response dispatch
- namespace: `Grasshopper2.UI.Flex`

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :------------------ | :------------ | :-------------------------------------- |
|  [01]   | `Response`          | enum          | ignored-to-capture verdict precedence   |
|  [02]   | `IResponsive`       | interface     | hit-test target with bound responder    |
|  [03]   | `ResponseMouseArgs` | args          | both frames, input state, invalidation  |
|  [04]   | `Responses`         | abstract      | handler, relay, hook, region, and focus |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Animated<T> construction and evaluation
- namespace: `Grasshopper2.UI.Animation`

| [INDEX] | [SURFACE]                                   | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :------------------------------------------ | :------- | :---------------------------------- |
|  [01]   | `Animated<T>.CreateUnfinished`              | factory  | start an animating curve            |
|  [02]   | `Animated<T>.CreateFinished`                | factory  | settled value                       |
|  [03]   | `Animated<T>.Chain`                         | instance | append a leg from the current value |
|  [04]   | `Animated<T>.Evaluate(DateTime) -> T`       | instance | sample at clock time                |
|  [05]   | `Animated<T>.Motion` / `State` / `ValueNow` | property | curve, lifecycle, current value     |
|  [06]   | `Animated<T>` implicit `T` / `operator +`   | operator | value-erase and chain-append        |

[ENTRYPOINT_SCOPE]: easing and typed animators
- namespace: `Grasshopper2.UI.Animation`

| [INDEX] | [SURFACE]                                            | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------------- | :------ | :-------------------------------------------------- |
|  [01]   | `MotionEquations.Blend(Motion, double) -> double`    | static  | map normalized time through an easing kind          |
|  [02]   | `Animators.DurationToTimeSpan(Duration) -> TimeSpan` | static  | resolve a named span to a duration                  |
|  [03]   | `Animators.Finished(value, Duration, Motion)`        | factory | settled typed animation per numeric/geometry type   |
|  [04]   | `Animators.Unfinished(from, to, Duration, Motion)`   | factory | animating typed animation per numeric/geometry type |

[ENTRYPOINT_SCOPE]: AnimatedPath feedback factories
- namespace: `Grasshopper2.UI.Animation`

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `AnimatedPath.CreateErrorPath` / `Warning` / `Success` / `Message` | factory  | semantic notice glyph paths                      |
|  [02]   | `AnimatedPath.CreateArrowPath(float, float)`                       | factory  | directional arrow                                |
|  [03]   | `new AnimatedPath(IEnumerable<IAnimatedStroke>)`                   | ctor     | custom path from strokes                         |
|  [04]   | `AddGap` / `AddLine` / `AddCircle` / `AddArc`                      | instance | append a stroke                                  |
|  [05]   | `AnimatedPath.Draw`                                                | instance | time-parameterized draw over key or t0–t1 window |

[ENTRYPOINT_SCOPE]: IFlexControl coordinate, navigation, and redraw seam
- namespace: `Grasshopper2.UI.Flex`

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `IFlexControl.Map(PointF\|RectangleF, CoordinateSystem, CoordinateSystem)` | instance | frame conversion                        |
|  [02]   | `IFlexControl.Navigate`                                                    | instance | animated move to anchor, point, or rect |
|  [03]   | `BeginWindowSelect` / `EndWindowSelect`                                    | instance | marquee lifecycle                       |
|  [04]   | `FlexControl.PushFocus` / `PopFocus`                                       | instance | focus-stack push and pop                |
|  [05]   | `RegisterIResponsive` / `UnregisterIResponsive`                            | instance | responsive target roster                |
|  [06]   | `ScheduleRedraw`                                                           | instance | repaint now or after a delay            |
|  [07]   | `Animate<T>(Animated<T>) -> T`                                             | instance | control-clock-driven value              |
|  [08]   | `AnimatedZoomFactor(ZoomThreshold) -> float`                               | instance | motion-gated zoom factor                |
|  [09]   | `DrawStartTime` / `DrawEndTime`                                            | property | frame-window times                      |
|  [10]   | `Draw` / `WindowSelection` / `PopulateContextMenu`                         | event    | draw, selection, and menu signals       |
|  [11]   | `FloatingButtons`                                                          | property | floating-button collection              |

[ENTRYPOINT_SCOPE]: response dispatch
- namespace: `Grasshopper2.UI.Flex`

| [INDEX] | [SURFACE]                                                                              | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `MouseOver` / `MouseDown` / `MouseDrag` / `MouseUp` / `MouseWheel`                     | instance | pointer handlers                  |
|  [02]   | `MouseSingleClick` / `MouseDoubleClick` / `MouseLeave`                                 | instance | click and leave handlers          |
|  [03]   | `KeyDown` / `KeyUp` / `TextInput` / `Rotation`                                         | instance | keyboard, text, rotation handlers |
|  [04]   | `InvokeMouseRelay` / `InvokeKeyRelay` / `InvokeTextInputRelay` / `InvokeRotationRelay` | static   | caller-handler routing            |
|  [05]   | `RedrawRequired` / `OnRedrawRequired`                                                  | event    | handler-raised repaint            |
|  [06]   | `new ResponseMouseArgs`                                                                | ctor     | dual-frame capture                |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- two namespaces meet at one seam: `Grasshopper2.UI.Animation` owns generic value interpolation, `Grasshopper2.UI.Flex` owns control projection and dispatch, joined where `IFlexControl.Animate` consumes an `Animated<T>`
- `Animated<T>` is a two-state curve: `CreateUnfinished` animates, `CreateFinished` holds a settled value, `Evaluate(DateTime)` samples, `State` reports `Pending`/`Busy`/`Finished`, and `Chain` or `operator +` appends the next leg
- `MotionEquations.Blend(Motion, double)` is the single easing evaluator every `Animated<T>` and `Animators` factory routes through; `Motion` names the base and delayed easing kinds and `Duration` the alphabetical spans whose enum value equals the millisecond count
- response dispatch is a folded verdict: each `Responses` handler returns a `Response` under `Ignored` < `Release` < `Handled` < `Capture` precedence, and each `Invoke*Relay` threads a caller handler through the typed args
- `IFlexControl` is the coordinate authority: `Map` converts between `Content` and `Control` frames, `Navigate` animates the viewport to a `ContentPosition`, point, or rect over a `Duration`, and `FlexControl`'s focus stack (`PushFocus`/`PopFocus`) routes capture
- `AnimatedPath` models a named feedback-stroke set with time-parameterized `Draw` — the notice glyph the canvas and chrome compose

[STACKING]:
- `api-thinktecture-runtime-extensions.md`(`.api/api-thinktecture-runtime-extensions.md`): `Motion`, `Duration`, `State`, and `Response` lower onto `SmartEnum` owners; the easing kind and dispatch verdict become closed generated vocabularies
- `api-languageext.md`(`.api/api-languageext.md`): a `Response`-returning handler folds through a `Fin`-shaped verdict, `ScheduleRedraw`/`Animate` ride `Eff`, `Animated<T>.Evaluate` yields a pure sample, and the responsive registry is a `Seq`/`HashMap`
- `api-unicolour.md`(`.api/api-unicolour.md`): `Animators.Finished`/`Unfinished(Color, …)` blends the `Eto.Drawing.Color` endpoints in a perceptual space
- Rasm kernel: the easing/interpolation math the host `Motion` enum names composes the kernel motion owner (`MotionInterpolation`), never a second in-folder easing derivation

[LOCAL_ADMISSION]:
- animation enters through `Animated<T>` + `Animators`; pacing is `MotionEquations.Blend`, never a hand-rolled tween loop
- redraw is `IFlexControl.ScheduleRedraw`; no host repaint-request or subscription object carries it
- coordinate conversion is `IFlexControl.Map`; a parallel content/control transform is the deleted form

[RAIL_LAW]:
- Package: `Grasshopper2` (host assembly)
- Owns: the `IFlexControl` coordinate/redraw/navigation seam, the animation value vocabulary, the easing evaluator, `Animated<T>`, `AnimatedPath` feedback factories, and the response mouse/key/text/rotation dispatch family
- Accept: value animation, viewport navigation, coordinate mapping, redraw scheduling, responsive registration, event dispatch
- Reject: canvas paint composition (`api-gh2-canvas.md`), floating-button chrome (`api-gh2-editor.md`), a host pacer/spring/subscription carrier, the GH1 event idiom
