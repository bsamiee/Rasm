# [RASM_GRASSHOPPER_API_GH2_CANVAS]

Catalog scope: the Grasshopper2 canvas — paint phases, picking, navigation/projection, window selection, wire rendering, skinning, and canvas-hosted editors.

[NAMESPACES]:
- `Grasshopper2.UI.Canvas` — `Canvas` (`ControlObject`, `Navigate`, `Map`, `Projection`, `VisibleFrame`/`ContentBounds`/`InnerBounds`, allow-pan/zoom/window-select toggles, `AllowedActions`, ZUI thresholds/state, `CursorMode`, `FocusObject`, `ResolvePick`/`DrawPickMap`, `DrawToBitmap`, `ScheduleRedraw`/`Invalidate`, `Animate`, `AnimatedZoomFactor`, `AddSparkle`, snap actions, inline/value/instantiation editors, undo-history view, `WireDrawCache`), `CanvasActions`, `CanvasPaintPhase` (before/after background/groups/wires/objects, `Attach`/`Detach`), `CanvasPaintEventArgs`/`CanvasBackgroundPaintEventArgs`, `CursorMode`, `Pick`, `SelectionResult`, `SnappingAction`/`SnappingSettings`/`SnappingConstraints`/`SnapSpace`/`SnapGuideStyle`, `WireStyle`, `WireRoutingProfile`, `ZoomThreshold`, `Capsule`, `Shade`, `ObjectSolutionState`.
- `Grasshopper2.UI.Canvas` events — `DocumentChanged`, `ProjectionChanged`, `WindowSelection`, `DocumentModified`, `MouseDwell`, `PopulateContextMenu`, `KeyDown`/`KeyUp`, `Draw`, cursor statics.
- GH2 wire-shape family — `WireShape` (bounds/project/distance/intersects/draw, `ShapeType`/`Create`), `WireShapeDefault`, `WireShapeElbow`, `WireShapeLinear`/`WireShapeBiArc` (internal, reflection-bridged), wire repository rows (`WireData`, `WireAt`, `MostRecentlyDrawnWires`, frames).
- `Grasshopper2.UI.Skinning` — `Skin` (`Interpolate`, `Shades`/`Canvasses`/`Wires` indexers), `Shade` families, `ShadeKind`, `CanvasSkin`, `CanvasKind`, `WireSkin` (outer/inner/glow pen assignment), `Shape`.
- `Grasshopper2.UI.Sparkles` — `ISparkle`, `BlastSparkle`/`EdgeSparkle`/`FaceSparkle`/`NoticeSparkle`, `BlastRadius`, `NoticeType`.
- `Grasshopper2.UI.Snap` — align-action factories, `SnapProjection`, `StretchLayoutSolver`.
- `Grasshopper2.UI.Primitives` — `PickTolerance`, `ResponseMouseArgs`.
- `Grasshopper2.Extensions` — `SelectionMode` and the `.SelectionMode()` extensions.
