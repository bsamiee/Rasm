# [RASM_GRASSHOPPER_API_GH2_FLEX]

Catalog scope: the Grasshopper2 flex/animation substrate — the UI-intent rail, paint hooks, repaint requests, motion pacing/springs, subscriptions, and the response vocabulary.

[NAMESPACES]:
- `Grasshopper2.UI.Flex` — `FlexControl`, `ContentPosition`, `CoordinateSystem` (Content/Control), `Response` (Handled/Capture/Release/Ignored), `PaintScope`, `CanvasPaintPhase`, `RepaintRequest` (canvas/object/batch), `DrawPlan`, `Paint.Hook`, `Subscription` (`Bind`, `DisposeOnce`, `PaintPacer`, composition operator), `Events.BindMarshaled`, `CanvasLease.RedrawLease`, `UiRail` (`PixelScale`, `PickAt`, `RunDocumentMutation`), `CanvasPickPolicy`, `UiDocumentIdentity`, `MotionClock` (`MessageLoop`), `Motion.Pacer`/`PacerOption`/`Spring`, `MotionVector` (`PointF`, `Float.RestEpsilon`), `SpringConfig.Response`, `SpringHandle<T>`.
- `Grasshopper2.UI.Animation` — `Duration` (Abrupt/Normal), `Motion`/`State` enums, `MotionEquations.Blend` (the native easing evaluator), `Animated<T>` (`CreateUnfinished`/`CreateFinished`, value/state members, curve-chaining operator), `Animators.DurationToTimeSpan`.
- `Grasshopper2.Extensions` — `TextAnchor` (9-way anchor enum).
- `Grasshopper2.UI` — `ZoomThreshold` (motion-gated zoom factors).
