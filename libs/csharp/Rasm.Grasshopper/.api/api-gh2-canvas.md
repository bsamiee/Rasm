# [RASM_GRASSHOPPER_API_GH2_CANVAS]

`Canvas` is the Grasshopper2 document host — a sealed `FlexControl` that paints the node graph, resolves picks off an offscreen id buffer, and gates every canvas edit. It owns the paint pipeline, wire-route geometry, snapping and alignment, skin interpolation, and sparkle overlays; projection, navigation, redraw scheduling, and animation cross the `IFlexControl` seam that `api-gh2-flex.md` owns and this surface composes, never re-derives.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grasshopper2`
- package: `Grasshopper2` (Rhino 9 WIP host plug-in bundle)
- assembly: `Grasshopper2`
- namespace: `Grasshopper2.UI.Canvas`, `Grasshopper2.UI.Skinning`, `Grasshopper2.UI.Primitives`, `Grasshopper2.UI.Sparkles`, `Grasshopper2.UI.Flex`
- namespace: `Eto.Drawing` — the host assembly declares `SlabF`, `CornerRadii`, `SlabVertex`, `SlabArc`, and `ArcF` into the Eto namespace, so they resolve bare beside genuine Eto types
- asset: in-process `Grasshopper2.dll` under `Grasshopper2Plugin.rhp`, painting over `Eto.Drawing.Graphics` in the Rhino assembly-load context
- rail: host-grasshopper

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: canvas control, paint arguments, action policy (`Grasshopper2.UI.Canvas`)

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :------------------------------- | :------------ | :------------------------------------------- |
|  [01]   | `Canvas`                         | sealed class  | document host, paint, pick, and editor owner |
|  [02]   | `CanvasPaintEventArgs`           | args          | `Canvas` + `Skin` + `Graphics` carrier       |
|  [03]   | `CanvasBackgroundPaintEventArgs` | args          | default-background suppression carrier       |
|  [04]   | `CanvasActions`                  | sealed class  | edit gates and two wire predicates           |

- `CanvasActions`: Boolean edit gates (host spells wire-select `AlloWireSelect`) with `MakeWireFilter`/`DeleteWireFilter` `((IParameter, IParameter) -> bool)` predicate slots

[PUBLIC_TYPE_SCOPE]: wire geometry, snapping, alignment (`Grasshopper2.UI.Canvas`)

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :----------------- | :------------ | :-------------------------------------- |
|  [01]   | `WireShape`        | abstract      | route creation, query, bounds, and draw |
|  [02]   | `WireShapeDefault` | sealed        | default spline creation                 |
|  [03]   | `SnappingAction`   | class         | align, gap, and straighten candidates   |
|  [04]   | `SnappingSettings` | sealed class  | rule and feedback derivation            |

- `SnappingAction`: carries the `LabelAnchor`, `LabelText`, `LabelPoint`, `Lines`, and `Magnitude` feedback columns

[PUBLIC_TYPE_SCOPE]: skinning and sparkle overlays (`Grasshopper2.UI.Skinning`, `Grasshopper2.UI.Sparkles`)

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :-------------- | :------------ | :----------------------------------------- |
|  [01]   | `Skin`          | sealed class  | aggregate palette, folds, and persistence  |
|  [02]   | `WireSkin`      | sealed class  | state pens, end colours, and interpolation |
|  [03]   | `BlastSparkle`  | sealed class  | radial blast overlay                       |
|  [04]   | `EdgeSparkle`   | sealed class  | edge-highlight overlay                     |
|  [05]   | `FaceSparkle`   | sealed class  | face-highlight overlay                     |
|  [06]   | `NoticeSparkle` | sealed class  | notice-badge overlay                       |
|  [07]   | `ISparkle`      | interface     | custom-overlay contract                    |

[PUBLIC_TYPE_SCOPE]: capsule primitives (`Grasshopper2.UI.Primitives`, `Eto.Drawing`)

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :----------------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `Primitives.Capsule`                 | sealed class  | slab plus plugs plus message bar; five draw legs |
|  [02]   | `Primitives.Plug` / `PlugSide`       | class, enum   | one grip location with its side and kind         |
|  [03]   | `Primitives.Parts`                   | flags enum    | `Plugs`/`Interior`/`Messaging`/`Overlay`/`Edge`  |
|  [04]   | `Eto.Drawing.SlabF`                  | struct        | the rounded apex-and-side capsule geometry       |
|  [05]   | `Eto.Drawing.CornerRadii`            | struct        | the four-corner radius set a slab carries        |
|  [06]   | `Eto.Drawing.SlabVertex` / `SlabArc` | enum          | the named vertex and arc a slab answers          |
|  [07]   | `Eto.Drawing.ArcF` / `CircleF`       | struct        | the arc and circle the slab and grips project    |

[PUBLIC_TYPE_SCOPE]: composed flex seam (`Grasshopper2.UI.Flex`, owned by `api-gh2-flex.md`)

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :------------- | :------------ | :------------------------------------------- |
|  [01]   | `IFlexControl` | interface     | projection, selection, redraw, and animation |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the eight ordered paint events (`Grasshopper2.UI.Canvas`)

| [INDEX] | [SURFACE]                                               | [SHAPE]                      | [CAPABILITY]                |
| :-----: | :------------------------------------------------------ | :--------------------------- | :-------------------------- |
|  [01]   | `Canvas.BeforePaintBackground` / `AfterPaintBackground` | `event`                      | grid/background layer fence |
|  [02]   | `Canvas.BeforePaintGroups` / `AfterPaintGroups`         | `event CanvasPaintEventArgs` | group-hull layer fence      |
|  [03]   | `Canvas.BeforePaintWires` / `AfterPaintWires`           | `event CanvasPaintEventArgs` | wire layer fence            |
|  [04]   | `Canvas.BeforePaintObjects` / `AfterPaintObjects`       | `event CanvasPaintEventArgs` | object-capsule layer fence  |

- `Canvas.BeforePaintBackground`: sole suppressible event, carries `CanvasBackgroundPaintEventArgs`; `AfterPaintBackground` and the six remaining events carry plain `CanvasPaintEventArgs`

[ENTRYPOINT_SCOPE]: picking, capture, bitmap (`Grasshopper2.UI.Canvas`)

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `Canvas.DrawPickMap() -> Bitmap`                         | instance | render the offscreen pick id buffer               |
|  [02]   | `Canvas.ResolvePick(PointF, bool×5) -> SelectionResult`  | instance | pick-buffer read with admission gates             |
|  [03]   | `Canvas.DrawToBitmap(int, int, bool, bool, bool)` / `()` | instance | canvas raster; bare overload is 500×300 all-layer |
|  [04]   | `Canvas.AllowedActions -> CanvasActions`                 | property | live action policy (get-only)                     |
|  [05]   | `Canvas.DocumentChanged` / `DocumentModified`            | event    | document-swap and dirty-state facts               |

- `Canvas.ResolvePick`: five bools — `includeGrips`, `includeForeground`, `includeBackground`, `includeWires`, `recursive`; inherited from `FlexControl`, reads the `DrawPickMap` buffer
- `DragPickingMode` (`Grasshopper2.UI.Canvas`, public enum) is GH2's own drag-grain vocabulary: `Default` defers to keyboard state, `SubObject` drags sub-objects where supported, `OneObject` drags whole objects, `AllObjects` drags every object in a parameter at once. It is the ONLY published pick vocabulary — a category roster naming surfaces, wires, or "everything" is a local invention with no host counterpart.
- Host's own resolver is unreachable: `ResolvePickMode(bool shift, bool control)` is `public` on `Canvas.ViewportMouseDragger`, a PRIVATE nested class, so no consumer can call it. Its law is what composes instead — `Default` resolves to `AllObjects` under shift and `OneObject` otherwise, and `control` is accepted and unread — and a folder re-spelling that law states it as a row column against this line, never as a fresh policy.

[ENTRYPOINT_SCOPE]: hosted editors, window-select gates, snap axes (`Grasshopper2.UI.Canvas`)

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `Canvas.ShowInlineEditor(RectangleF, string, Func<string,IResult>, Action?)` | instance | in-place parse-and-commit editor      |
|  [02]   | `Canvas.WindowSelect{Objects,Wires,Groups}`                                  | property | per-category marquee gates            |
|  [03]   | `Canvas.SnapXAction` / `SnapYAction -> SnappingAction`                       | property | active-axis nudge; setter invalidates |
|  [04]   | `Canvas.ShowUndoHistory`                                                     | property | get/set undo-history pane toggle      |
|  [05]   | `Canvas.SkinLit` / `SkinDim` / `Skin`                                        | property | lit, dimmed, interpolated palettes    |

[ENTRYPOINT_SCOPE]: WireShape geometry (`Grasshopper2.UI.Canvas`)

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `WireShape.Create(IParameterAttributes, IParameterAttributes) -> WireShape` | static   | route a wire between two pins      |
|  [02]   | `WireShape.Create(PointF, PointF) -> WireShape`                             | static   | route a wire between two endpoints |
|  [03]   | `WireShape.Project(PointF) -> PointF`                                       | instance | closest route point to a query     |
|  [04]   | `WireShape.DistanceTo(PointF) -> float`                                     | instance | route-to-point distance            |
|  [05]   | `WireShape.Intersects(RectangleF) -> bool`                                  | instance | marquee-hit test                   |
|  [06]   | `WireShape.IsCoincident(PointF, float) -> bool`                             | instance | on-route test within a tolerance   |
|  [07]   | `WireShape.Draw(Graphics, Pen)`                                             | instance | stroke the route with a pen        |
|  [08]   | `WireShapeDefault.CreateSpline(PointF, PointF) -> BezierF`                  | static   | default spline factory             |

[ENTRYPOINT_SCOPE]: snapping and alignment (`Grasshopper2.UI.Canvas`)

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :--------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `SnappingAction.Create{Left,Right,Top,Bottom,Centre}AlignAction` | static   | edge or centre candidate        |
|  [02]   | `SnappingAction.Create{Vertical,Horizontal}GapAction*`           | static   | equal-gap candidate             |
|  [03]   | `SnappingAction.CreateStraightenWireAction(PointF, PointF)`      | static   | straight-wire candidate         |
|  [04]   | `SnappingAction.SmallerMagnitude`                                | static   | shortest-nudge fold             |
|  [05]   | `SnappingSettings.WithRules` / `WithoutRules` / `WithFeedback`   | instance | rule and feedback variants      |
|  [06]   | `SnappingSettings.Default` / `Current` / `Colour` / `Feedback`   | property | settings roster and live policy |

- align and gap factories take a `(RectangleF source, RectangleF target)` frame pair with a `float`/`int` offset; the wire factory takes a `PointF` pair

[ENTRYPOINT_SCOPE]: capsule construction, geometry, and the five draw legs (`Grasshopper2.UI.Primitives`, `Eto.Drawing`)

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Capsule(SlabF slab, float barHeight = 0f)`                                  | ctor     | throws on an empty slab bounds                |
|  [02]   | `Capsule.CreateFromOuter(Shape, RectangleF)` / `CreateFromInner(...)`        | static   | outer-bounds and apex-bounds construction     |
|  [03]   | `Capsule.Bounds` / `Slab` / `BarHeight`                                      | property | extent, geometry, and message-bar height      |
|  [04]   | `Capsule.AddInputPlug(float, GripKind)` / `AddOutputPlug(...)`               | instance | append a grip at an elevation                 |
|  [05]   | `Capsule.AddCustomPlug(Plug)` / `InputPlug(int)` / `OutputPlug(int)`         | instance | custom append and indexed read                |
|  [06]   | `InputPlugCount` / `OutputPlugCount` / `HasInputPlugs` / `HasOutputPlugs`    | property | grip census                                   |
|  [07]   | `HasInfoMessage` / `HasWarningMessage` / `HasErrorMessage` / `HasAnyMessage` | property | the three message flags and their fold        |
|  [08]   | `Capsule.Draw(Graphics, Shade, Skin)` / `Draw(Graphics, Parts, ...)`         | instance | full draw and the `Parts`-selected draw       |
|  [09]   | `DrawGrips` / `DrawFaces` / `DrawMessaging` / `DrawOverlay` / `DrawEdges`    | instance | the five ordered legs `Draw` folds            |
|  [10]   | `SlabF(RectangleF)` / `SlabF(RectangleF, float thickness, CornerRadii)`      | ctor     | flat and thick rounded construction           |
|  [11]   | `SlabF.Bounds` / `Apex` / `Side` / `Corners` / `Thickness`                   | property | the outer, top-face, and side-face rectangles |
|  [12]   | `SlabF.FormPath` / `EdgePath` / `ApexPath` / `SidePath` / `SidePathExact`    | property | the five `GraphicsPath` projections           |
|  [13]   | `SlabF.Contains(PointF)`                                                     | instance | the exact rounded-capsule hit test            |
|  [14]   | `SlabF.WithCorners(CornerRadii)` / `Move` / `MoveTo` / `Depress(float)`      | instance | non-destructive geometry edits                |
|  [15]   | `SlabF.VertexAt(SlabVertex)` / `ArcAt(SlabArc)`                              | instance | named vertex and arc reads                    |
|  [16]   | `SlabF.PointOnLeftEdge(float)` / `PointOnRightEdge(float)`                   | instance | parameterized edge points for grip placement  |
|  [17]   | `SlabF.MessageBar(float, out GraphicsPath, out RectangleF)`                  | instance | message-bar geometry, `false` when absent     |

- `Capsule` mutates: `AddInputPlug`/`AddOutputPlug` grow internal arrays in blocks of six and `HasInfoMessage`/`HasWarningMessage`/`HasErrorMessage` are settable, so a capsule is a per-paint scratch value built inside the draw, never a cached one — `Attributes<T>.Draw(Context, Skin)` re-mints it every frame through `CreateFromOuter(skin.Shape, Bounds)`.
- Draw legs are ordered — grips, faces, messaging, overlay, edges — and `Draw(Graphics, Parts, Shade, Skin)` selects a subset by flag; each leg fades its own geometry through `skin.Fades.Form`/`.Grip`, so a partial draw still matches the animated skin.
- `SlabF.Contains` is the exact answer and `RectangleF.Contains` the coarse pre-filter; a hit test on bounds alone accepts the four rounded corners the capsule excludes.

[ENTRYPOINT_SCOPE]: skin interpolation and sparkle draws (`Grasshopper2.UI.Skinning`, `Grasshopper2.UI.Sparkles`)

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :-------------------------------- |
|  [01]   | `Skin.Interpolate(Skin, float) -> Skin`                                               | instance | palette blend                     |
|  [02]   | `Skin.With{Shape,Shades,Wires,Grips,Messages,Canvasses,Fades}`                        | instance | one-row sub-skin replacement      |
|  [03]   | `WireSkin.ResolveColours(bool, bool, out Color, out Color)`                           | instance | source/target colour by state     |
|  [04]   | `WireSkin.Interpolate(WireSkin, float) -> WireSkin`                                   | instance | wire-palette blend                |
|  [05]   | `WireSkin.Modify(Nullable ×4, EdgeDescription ×2) -> WireSkin`                        | instance | present-slot palette rewrite      |
|  [06]   | `WireSkin.{Fade, Outer, Inner, Normal, Selected, Unselected, SelectedGlow}`           | member   | fade fold and colour/edge columns |
|  [07]   | `Skinning.EdgeDescription.{AssignToPen, Width, Cap, Dash}`                            | member   | pen-projection edge policy        |
|  [08]   | `Skin.{Shape, Shades, Wires, Grips, Messaging, Canvasses, Fades}`                     | property | the seven skin subsystems         |
|  [09]   | `Flex.WindowSelection.Selects(RectangleF\|CircleF\|LineF\|BezierF\|WireShape, float)` | instance | five-shape marquee test           |
|  [10]   | `BlastSparkle` ctor + `Draw`                                                          | ctor     | radial blast overlay              |
|  [11]   | `EdgeSparkle` / `FaceSparkle` / `NoticeSparkle` `.Draw`                               | instance | edge, face, notice overlays       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- paint runs four layers in fixed order (background → groups → wires → objects), each fenced by a `Before`/`After` event pair; a hook attaches to the pair and the phase IS the event, never a `CanvasPaintPhase` enum
- picking is a two-surface contract: `DrawPickMap` renders the offscreen id buffer, `ResolvePick` reads it back under per-category admission gates
- every paint arg carries `Canvas` + `Skin` + `Graphics`; `SkinLit`/`SkinDim`/`Skin` supply the interpolated palette and `Graphics` is the `Eto.Drawing` target
- `CanvasActions` is the sole edit gate: its Boolean toggles and two wire-filter predicates admit or deny drag, selection, wire and object mutation, file drop, response, and the three context menus
- `WireShape` is the closed wire-route family; `Create` discriminates endpoint pair versus parameter-attribute pair and every route answers `Project`/`DistanceTo`/`Intersects`/`IsCoincident`/`Draw`
- snapping is document-scoped: `api-gh2-interaction.md` owns the `SnappingConstraints` resolution surface whole (`CreateFromDocument`, `SnapRectangle`, `SnapWires`, `DrawSnappingBoxes`); this partition keeps the `SnappingAction` candidate factories, its feedback columns, and the `SnappingSettings` policy rows, and `SmallerMagnitude` folds the winning nudge
- skin interpolation is value-parametric: `Skin.Interpolate`/`WireSkin.Interpolate` blend at a parameter and `WireSkin.ResolveColours` emits the wire end-colour pair by state
- Capsule is the object silhouette every attributes draw folds: `Capsule.CreateFromOuter(Skin.Shape, Bounds)` mints it per frame, `SlabF` carries the rounded geometry and answers `Contains` exactly, and the `Parts` flag set selects which of the five ordered legs run

[STACKING]:
- `api-gh2-flex.md`(`.api/api-gh2-flex.md`): the `IFlexControl` seam owns projection (`Map`), navigation (`Navigate`), window-select (`BeginWindowSelect`/`EndWindowSelect`), responsive registration, redraw scheduling (`ScheduleRedraw`), and `Animate`; the canvas composes it and holds no parallel viewport transform
- `api-languageext.md`(`.api/api-languageext.md`): `ResolvePick` lands as `Option<Pick>`, paint hooks and pick reads ride `Eff`/`Fin`, and snap candidates fold as a `Seq<SnappingAction>` reduced through `SmallerMagnitude`
- `api-thinktecture-runtime-extensions.md`(`.api/api-thinktecture-runtime-extensions.md`): `WireShape.ShapeType` and the sparkle set lower onto a `SmartEnum`/`Union`, and `CanvasActions` is a `ValueObject` policy record
- `api-unicolour.md`(`.api/api-unicolour.md`): `Skin.Interpolate` and `WireSkin.ResolveColours` blend in a perceptual space, crossing the `Eto.Drawing.Color` pen boundary at the edge

[LOCAL_ADMISSION]:
- canvas surface enters only through the host `Canvas`/`Flex` seam; a paint hook attaches to the eight `Canvas` events
- `WireShape`/`Skin`/`Sparkle` compose as host types; perceptual blending and easing math compose the Rasm kernel motion/colour owner

[RAIL_LAW]:
- Package: `Grasshopper2` (host assembly)
- Owns: the canvas paint pipeline, pick-map resolution, wire-route geometry, snapping and alignment, skin interpolation, sparkle overlays, the `Capsule`/`SlabF` primitive geometry every attributes draw folds, canvas-hosted inline editors
- Accept: paint-event composition, pick resolution, window selection, wire and skin rendering, snap solving
- Reject: the `IFlexControl` seam internals (`api-gh2-flex.md`), document mutation, component execution, a re-derived paint-phase enum, an in-folder wire route, palette, or overlay, the GH1 `GH_Canvas`/`IGH_*` paint idiom
