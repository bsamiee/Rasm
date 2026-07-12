# [RASM_GRASSHOPPER_API_GH2_FLEX]

The Grasshopper2 flex/animation substrate splits generic value interpolation (`Grasshopper2.UI.Animation`) from control projection and input dispatch (`Grasshopper2.UI.Flex`). `IFlexControl` is the typed host seam for coordinate mapping, viewport navigation, responsive registration, focus routing, redraw scheduling, draw timing, and `Animated<T>` consumption. `MotionEquations.Blend` is the single easing evaluator; `Animated<T>` is the two-state animation carrier; `Responses` folds every mouse/key/text event to a `Response` verdict. Motion pacing lives in `MotionEquations.Blend` and `Animated<T>`, repaint in `IFlexControl.ScheduleRedraw` — there is no host pacer, spring, repaint-request, or subscription carrier. Every member is catalog-verified against the installed RhinoWIP `Grasshopper2.xml`.

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

`Motion` is the 16-member base/delayed lattice over linear, ease-in/out, ease-in-out, snap-in/out, bounce, and twang. `Duration` encodes `Abrupt` 0, `Brief` 50, `Fast` 150, `Normal` 300, `Slow` 500, `Tedious` 1000, `Torpid` 1500, and `Ĝlāçïāľ` 5000 milliseconds. `Animated<T>` carries both endpoints and times, five `Chain` overloads, two finished and two unfinished factories, implicit conversions, and retarget operators.

| [INDEX] | [SYMBOL]          | [KIND]        | [CAPABILITY]                        |
| :-----: | :---------------- | :------------ | :---------------------------------- |
|  [01]   | `Motion`          | enum          | base and delayed easing lattice     |
|  [02]   | `Duration`        | enum          | eight millisecond-valued spans      |
|  [03]   | `State`           | enum          | pending, busy, finished             |
|  [04]   | `MotionEquations` | static        | normalized easing evaluation        |
|  [05]   | `Animated<T>`     | value carrier | endpoint, time, chain, and sample   |
|  [06]   | `Animators`       | static        | ten typed animation families        |
|  [07]   | `AnimatedPath`    | path          | four feedback-stroke draw overloads |
|  [08]   | `Interpolate<T>`  | delegate      | per-value interpolation             |
|  [09]   | `IAnimatedStroke` | interface     | one animated-path stroke            |

[PUBLIC_TYPE_SCOPE]: the IFlexControl seam
- namespace: `Grasshopper2.UI.Flex`, `Grasshopper2.UI`
- rail: host-grasshopper

`IFlexControl.Navigate` has exactly three `Duration`-based overloads: content anchor, point with zoom range, and rectangle with zoom range. The seam also owns responsive registration and focus, redraw, animation, sparkles, dwell, context menu, selection, draw, projection, and floating-button surfaces.

| [INDEX] | [SYMBOL]           | [KIND]    | [CAPABILITY]                     |
| :-----: | :----------------- | :-------- | :------------------------------- |
|  [01]   | `IFlexControl`     | host seam | projection, dispatch, and redraw |
|  [02]   | `FlexControl`      | control   | concrete response source         |
|  [03]   | `CoordinateSystem` | enum      | content and control frames       |
|  [04]   | `ContentPosition`  | position  | named navigation anchor          |
|  [05]   | `ZoomThreshold`    | struct    | animated zoom threshold          |

[PUBLIC_TYPE_SCOPE]: response dispatch
- namespace: `Grasshopper2.UI.Flex`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]            | [KIND]    | [CAPABILITY]                              |
| :-----: | :------------------ | :-------- | :---------------------------------------- |
|  [01]   | `Response`          | enum      | ignored-to-capture verdict precedence     |
|  [02]   | `IResponsive`       | interface | hit-test target with bound responder      |
|  [03]   | `ResponseMouseArgs` | args      | both frames, input state, invalidation    |
|  [04]   | `Responses`         | abstract  | 11 handlers, relays, hooks, region, focus |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Animated<T> construction and evaluation
- namespace: `Grasshopper2.UI.Animation`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                   | [SHAPE]           | [CAPABILITY]                    |
| :-----: | :------------------------------------------ | :---------------- | :------------------------------ |
|  [01]   | `Animated<T>.CreateUnfinished`              | endpoints + curve | start an animation              |
|  [02]   | `Animated<T>.CreateFinished`                | two overloads     | settled value                   |
|  [03]   | `Animated<T>.Chain`                         | five overloads    | append from current value       |
|  [04]   | `Animated<T>.Evaluate` / `EvaluateThis`     | `DateTime → T`    | sample at clock time            |
|  [05]   | `Animated<T>.Motion` / `State` / `ValueNow` | properties        | curve, lifecycle, current value |

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

| [INDEX] | [SURFACE]                                     | [SHAPE]        | [CAPABILITY]            |
| :-----: | :-------------------------------------------- | :------------- | :---------------------- |
|  [01]   | `CreateErrorPath` / `Warning` / `Success`     | float          | semantic notice glyphs  |
|  [02]   | `AnimatedPath.CreateArrowPath`                | float pair     | directional arrow       |
|  [03]   | `new AnimatedPath`                            | stroke set     | custom path             |
|  [04]   | `AddGap` / `AddLine` / `AddCircle` / `AddArc` | primitives     | append stroke           |
|  [05]   | `AnimatedPath.Draw`                           | four overloads | time-parameterized draw |

[ENTRYPOINT_SCOPE]: IFlexControl coordinate, navigation, and redraw seam
- namespace: `Grasshopper2.UI.Flex`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                       | [SHAPE]         | [CAPABILITY]               |
| :-----: | :---------------------------------------------- | :-------------- | :------------------------- |
|  [01]   | `IFlexControl.Map`                              | point or rect   | frame conversion           |
|  [02]   | `IFlexControl.Navigate`                         | three overloads | animated viewport movement |
|  [03]   | `BeginWindowSelect` / `EndWindowSelect`         | unit            | marquee lifecycle          |
|  [04]   | `PushFocus` / `PopFocus`                        | responsive      | focus stack                |
|  [05]   | `RegisterIResponsive` / `UnregisterIResponsive` | responsive      | target roster              |
|  [06]   | `ScheduleRedraw`                                | now or delay    | repaint request            |
|  [07]   | `Animate<T>`                                    | animated value  | control-clock drive        |
|  [08]   | `AnimatedZoomFactor`                            | threshold       | motion-gated zoom          |
|  [09]   | `Draw` / `DrawStartTime` / `DrawEndTime`        | event + times   | frame signal and window    |
|  [10]   | `FloatingButtons`                               | property        | floating-button collection |

[ENTRYPOINT_SCOPE]: response dispatch
- namespace: `Grasshopper2.UI.Flex`
- rail: host-grasshopper

Each handler returns a `Response`; the relay forms thread a caller handler through the args and fold the verdict.

| [INDEX] | [SURFACE]                                          | [SHAPE]         | [CAPABILITY]               |
| :-----: | :------------------------------------------------- | :-------------- | :------------------------- |
|  [01]   | `MouseOver` / `Down` / `Drag` / `Up` / `Wheel`     | mouse args      | pointer handlers           |
|  [02]   | `MouseSingleClick` / `DoubleClick` / `Leave`       | mouse args      | click and leave handlers   |
|  [03]   | `KeyDown` / `KeyUp` / `TextInput`                  | typed args      | keyboard and text handlers |
|  [04]   | `InvokeMouseRelay` / `KeyRelay` / `TextInputRelay` | relay           | caller-handler routing     |
|  [05]   | `RedrawRequired` / `OnRedrawRequired`              | event + hook    | handler-raised repaint     |
|  [06]   | `new ResponseMouseArgs`                            | control + event | dual-frame capture         |

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
