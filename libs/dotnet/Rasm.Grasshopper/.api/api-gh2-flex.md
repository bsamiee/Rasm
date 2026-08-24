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

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                                      |
| :-----: | :---------------- | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `Motion`          | enum          | base and delayed easing kinds on decade ordinals (`Linear`=0 … `TwangDelayed`=71) |
|  [02]   | `Duration`        | enum          | named spans; value equals ms (`Abrupt`=0 … `Torpid`=1500, `Ĝlāçïāľ`=5000)         |
|  [03]   | `State`           | enum          | pending, busy, finished                                                           |
|  [04]   | `MotionEquations` | static        | normalized easing evaluation                                                      |
|  [05]   | `Animated<T>`     | value carrier | endpoint, time, chain, and sample                                                 |
|  [06]   | `Animators`       | static        | typed animation factories                                                         |
|  [07]   | `AnimatedPath`    | class         | feedback-stroke draw set                                                          |
|  [08]   | `Interpolate<T>`  | delegate      | per-value interpolation                                                           |
|  [09]   | `IAnimatedStroke` | interface     | one animated-path stroke                                                          |

- `Duration.Ĝlāçïāľ`: ships with its diacritics; spell the member exactly.

[PUBLIC_TYPE_SCOPE]: the IFlexControl seam
- namespace: `Grasshopper2.UI.Flex`, `Grasshopper2.UI`

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :----------------- | :------------ | :----------------------------------------------- |
|  [01]   | `IFlexControl`     | interface     | projection, dispatch, and redraw                 |
|  [02]   | `FlexControl`      | class         | concrete response source                         |
|  [03]   | `CoordinateSystem` | enum          | content and control frames                       |
|  [04]   | `ContentPosition`  | enum          | named navigation anchor                          |
|  [05]   | `ZoomThreshold`    | enum          | animated zoom threshold — `Detailed`, `Standard` |

[PUBLIC_TYPE_SCOPE]: response dispatch
- namespace: `Grasshopper2.UI.Flex`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :--------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `Response`             | enum          | ignored-to-capture verdict precedence          |
|  [02]   | `IResponsive`          | interface     | hit-test target with bound responder           |
|  [03]   | `ResponseMouseArgs`    | args          | both frames, input state, invalidation         |
|  [04]   | `ResponseRotationArgs` | args          | clockwise-degree rotation gesture delta        |
|  [05]   | `Responses`            | abstract      | virtual handlers beside ignored-fallback hooks |

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

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `MotionEquations.Blend(Motion, double) -> double`                             | static   | map normalized time through an easing kind |
|  [02]   | `Animators.DurationToTimeSpan(Duration) -> TimeSpan`                          | static   | resolve a named span to a duration         |
|  [03]   | `AnimatedPath.{AddGap, AddLine, AddLines, AddCircle, AddArc, Count, Gaps}`    | member   | stroke-set build and tallies               |
|  [04]   | `AnimatedPath.Create{Error, Warning, Success, Message, Arrow}Path`            | static   | the five semantic glyph factories          |
|  [05]   | `FlexControl.FocusObject -> IResponsive`; `IFlexControl.ResponsivesForwards`  | property | focus head and responder walk              |
|  [06]   | `Flex.PopulateContextMenuEventArgs.{Control, MouseEvent, Menu, IsMenu}`       | property | context-menu population payload            |
|  [07]   | `ResizingFrame.{Original, Resized, MinimumSize, MaximumSize}`                 | property | resize-frame geometry columns              |
|  [08]   | `Flex.{ProjectionChanged, WindowSelection, MouseDwell, ControlDraw}EventArgs` | class    | canvas event-args wires                    |
|  [09]   | `Animators.Finished(value, Duration, Motion)`                                 | factory  | settled typed animation per type           |
|  [10]   | `Animators.Unfinished(from, to, Duration, Motion)`                            | factory  | animating typed animation per type         |

- `Flex.MouseDwellEventArgs`: carries `Control`, `ControlPoint`, and `ContentPoint`.

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

[ENTRYPOINT_SCOPE]: `Responses` virtual handlers — the primary dispatch path a responder owns by override
- namespace: `Grasshopper2.UI.Flex`

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `protected Responses(CoordinateSystem system = CoordinateSystem.Content)`   | ctor     | declares the frame the responder reads    |
|  [02]   | `virtual void MouseOver(ResponseMouseArgs)` / `virtual void MouseLeave()`   | instance | hover entry and exit, no verdict          |
|  [03]   | `virtual Response Mouse{Down, Drag, Up, Wheel}(ResponseMouseArgs)`          | instance | pointer verdicts                          |
|  [04]   | `virtual Response MouseSingleClick` / `MouseDoubleClick(ResponseMouseArgs)` | instance | click verdicts                            |
|  [05]   | `virtual Response KeyDown(KeyEventArgs)` / `KeyUp(KeyEventArgs)`            | instance | key verdicts                              |
|  [06]   | `virtual Response TextInput(TextInputEventArgs)`                            | instance | text-entry verdict                        |
|  [07]   | `virtual Response Rotation(ResponseRotationArgs)`                           | instance | rotation-gesture verdict                  |
|  [08]   | `virtual bool HadEffect`                                                    | property | `false` downgrades `Release` to `Ignored` |
|  [09]   | `bool HasFocus` / `CoordinateSystem CoordinatesContext`                     | property | host-set focus flag, declared frame       |
|  [10]   | `RectangleF RegionBoundary` / `Func<PointF, bool> RegionFilter`             | property | coarse region and exact in-region filter  |
|  [11]   | `bool IsCoincident(PointF controlPoint, PointF contentPoint)`               | instance | frame-selecting hit test over both        |
|  [12]   | `new ResponseMouseArgs`                                                     | ctor     | dual-frame capture                        |

[ENTRYPOINT_SCOPE]: `Responses` hook events — the plug-in path an attribute subclass takes without subclassing the responder
- namespace: `Grasshopper2.UI.Flex`

| [INDEX] | [SURFACE]                                                                       | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------------------------ | :------ | :------------------------------------------- |
|  [01]   | `event Action<ResponseMouseArgs> MouseOverHook` / `event Action MouseLeaveHook` | event   | unconditional post-handler hover taps        |
|  [02]   | `event Func<ResponseMouseArgs, Response> Mouse{Down, Drag, Up, Wheel}Hook`      | event   | pointer taps on the ignored fallback         |
|  [03]   | `event Func<ResponseMouseArgs, Response> Mouse{Single, Double}ClickHook`        | event   | click taps on the ignored fallback           |
|  [04]   | `event Func<KeyEventArgs, Response> KeyDownHook` / `KeyUpHook`                  | event   | key taps on the ignored fallback             |
|  [05]   | `event Func<TextInputEventArgs, Response> TextInputHook`                        | event   | text tap on the ignored fallback             |
|  [06]   | `event Func<ResponseRotationArgs, Response> RotationHook`                       | event   | rotation tap on the ignored fallback         |
|  [07]   | `event EventHandler GotFocus` / `LostFocus`                                     | event   | `HasFocus` transition edges                  |
|  [08]   | `event EventHandler RedrawRequired` / `void OnRedrawRequired()`                 | event   | handler-raised repaint request               |
|  [09]   | `protected static Response InvokeMouseRelay(Func<…>, ResponseMouseArgs)`        | static  | first-non-`Ignored` invocation-list walk     |
|  [10]   | `protected static Response Invoke{Key, TextInput, Rotation}Relay`               | static  | the same walk over the other three arg kinds |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- two namespaces meet at one seam: `Grasshopper2.UI.Animation` owns generic value interpolation, `Grasshopper2.UI.Flex` owns control projection and dispatch, joined where `IFlexControl.Animate` consumes an `Animated<T>`
- `Animated<T>` is a two-state curve: `CreateUnfinished` animates, `CreateFinished` holds a settled value, `Evaluate(DateTime)` samples, `State` reports `Pending`/`Busy`/`Finished`, and `Chain` or `operator +` appends the next leg
- `MotionEquations.Blend(Motion, double)` is the single easing evaluator every `Animated<T>` and `Animators` factory routes through; `Motion` names the base and delayed easing kinds and `Duration` the alphabetical spans whose enum value equals the millisecond count
- response dispatch is a folded verdict: each `Responses` handler returns a `Response` under `Ignored` < `Release` < `Handled` < `Capture` precedence, and `HadEffect` returning `false` downgrades that responder's `Release` to `Ignored` so a no-drag right click still reaches context-menu population
- `Responses` carries TWO member families over one event set and the override is the primary one: each `*Hook` event fires only where the virtual member's own logic returns `Ignored`, because every base body is exactly `Invoke*Relay(hook, e)` and an override reaching `base.Member(e)` is what re-enters that relay; the relay walks the invocation list in subscription order and the first non-`Ignored` result wins, so a subscriber cannot pre-empt an override and an override that never calls base silences every subscriber on that event. `MouseOverHook`/`MouseLeaveHook` are the two exceptions — `Action`-shaped, no verdict, invoked unconditionally after the member's own logic
- Family split is the extension seam: a responder the boundary constructs owns behavior by override, while an attribute subclass extends a host responder it cannot subclass (`ComponentAttributes` and `ResizableAttributes<T>` both expose theirs as a private sealed nested class behind `public Responses Responder`) by subscribing hooks, which fire exactly where that host responder declines
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
- Owns: the `IFlexControl` coordinate/redraw/navigation seam, the animation value vocabulary, the easing evaluator, `Animated<T>`, `AnimatedPath` feedback factories, and both `Responses` families — the virtual mouse/key/text/rotation handlers and the ignored-fallback hook events beside them
- Accept: value animation, viewport navigation, coordinate mapping, redraw scheduling, responsive registration, event dispatch
- Reject: canvas paint composition (`api-gh2-canvas.md`), floating-button chrome (`api-gh2-editor.md`), a host pacer/spring/subscription carrier, the GH1 event idiom
