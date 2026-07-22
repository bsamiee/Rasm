# [RASM_GRASSHOPPER_API_GH2_CANVAS]

`Canvas` is the Grasshopper2 document host — a sealed `FlexControl` that paints the node graph, resolves picks off an offscreen id buffer, and gates every canvas edit. It owns the paint pipeline, wire-route geometry, snapping and alignment, skin interpolation, and sparkle overlays; projection, navigation, redraw scheduling, and animation cross the `IFlexControl` seam that `api-gh2-flex.md` owns and this surface composes, never re-derives.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grasshopper2`
- package: `Grasshopper2` (Rhino 9 WIP host plug-in bundle)
- assembly: `Grasshopper2`
- namespace: `Grasshopper2.UI.Canvas`, `Grasshopper2.UI.Skinning`, `Grasshopper2.UI.Sparkles`, `Grasshopper2.UI.Flex`
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

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :-------------------- | :------------ | :-------------------------------------- |
|  [01]   | `WireShape`           | abstract      | route creation, query, bounds, and draw |
|  [02]   | `WireShapeDefault`    | sealed        | default spline creation                 |
|  [03]   | `SnappingAction`      | class         | align, gap, and straighten candidates   |
|  [04]   | `SnappingConstraints` | sealed class  | document-scoped snap resolution         |
|  [05]   | `SnappingSettings`    | sealed class  | rule and feedback derivation            |

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

[ENTRYPOINT_SCOPE]: hosted editors, window-select gates, snap axes (`Grasshopper2.UI.Canvas`)

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `Canvas.ShowInlineEditor(RectangleF, string, Func<string,IResult>, Action?)` | instance | in-place parse-and-commit editor      |
|  [02]   | `Canvas.WindowSelect{Objects,Wires,Groups}`                                  | property | per-category marquee gates            |
|  [03]   | `Canvas.SnapXAction` / `SnapYAction -> SnappingAction`                       | property | active-axis nudge; setter invalidates |
|  [04]   | `Canvas.SkinLit` / `SkinDim` / `Skin`                                        | property | lit, dimmed, interpolated palettes    |

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

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :-------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `SnappingAction.Create{Left,Right,Top,Bottom,Centre}AlignAction`      | static   | edge or centre candidate                |
|  [02]   | `SnappingAction.Create{Vertical,Horizontal}GapAction*`                | static   | equal-gap candidate                     |
|  [03]   | `SnappingAction.CreateStraightenWireAction(PointF, PointF)`           | static   | straight-wire candidate                 |
|  [04]   | `SnappingAction.SmallerMagnitude`                                     | static   | shortest-nudge fold                     |
|  [05]   | `SnappingConstraints.CreateFromDocument(Document, …)`                 | static   | document constraint set (two overloads) |
|  [06]   | `SnappingConstraints.SnapObject` / `SnapRectangle`                    | instance | X/Y frame resolution                    |
|  [07]   | `SnappingConstraints.SnapWires(IDocumentObject, …) -> SnappingAction` | static   | wire resolution (two overloads)         |
|  [08]   | `SnappingConstraints.DrawSnappingBoxes(Graphics)`                     | instance | live guide overlay                      |
|  [09]   | `SnappingSettings.WithRules` / `WithoutRules` / `WithFeedback`        | instance | rule and feedback variants              |

- align and gap factories take a `(RectangleF source, RectangleF target)` frame pair with a `float`/`int` offset; the wire factory takes a `PointF` pair

[ENTRYPOINT_SCOPE]: skin interpolation and sparkle draws (`Grasshopper2.UI.Skinning`, `Grasshopper2.UI.Sparkles`)

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :------------------------------------------------------------- | :------- | :---------------------------- |
|  [01]   | `Skin.Interpolate(Skin, float) -> Skin`                        | instance | palette blend                 |
|  [02]   | `Skin.With{Shape,Shades,Wires,Grips,Messages,Canvasses,Fades}` | instance | one-row sub-skin replacement  |
|  [03]   | `WireSkin.ResolveColours(bool, bool, out Color, out Color)`    | instance | source/target colour by state |
|  [04]   | `WireSkin.Interpolate(WireSkin, float) -> WireSkin`            | instance | wire-palette blend            |
|  [05]   | `BlastSparkle` ctor + `Draw`                                   | ctor     | radial blast overlay          |
|  [06]   | `EdgeSparkle` / `FaceSparkle` / `NoticeSparkle` `.Draw`        | instance | edge, face, notice overlays   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- paint runs four layers in fixed order (background → groups → wires → objects), each fenced by a `Before`/`After` event pair; a hook attaches to the pair and the phase IS the event, never a `CanvasPaintPhase` enum
- picking is a two-surface contract: `DrawPickMap` renders the offscreen id buffer, `ResolvePick` reads it back under per-category admission gates
- every paint arg carries `Canvas` + `Skin` + `Graphics`; `SkinLit`/`SkinDim`/`Skin` supply the interpolated palette and `Graphics` is the `Eto.Drawing` target
- `CanvasActions` is the sole edit gate: its Boolean toggles and two wire-filter predicates admit or deny drag, selection, wire and object mutation, file drop, response, and the three context menus
- `WireShape` is the closed wire-route family; `Create` discriminates endpoint pair versus parameter-attribute pair and every route answers `Project`/`DistanceTo`/`Intersects`/`IsCoincident`/`Draw`
- snapping is document-scoped: `SnappingConstraints.CreateFromDocument` builds the constraint set, the `SnappingAction` factories mint align/gap/straighten candidates, and `SmallerMagnitude` folds the winning nudge under a `SnappingSettings` rule set
- skin interpolation is value-parametric: `Skin.Interpolate`/`WireSkin.Interpolate` blend at a parameter and `WireSkin.ResolveColours` emits the wire end-colour pair by state

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
- Owns: the canvas paint pipeline, pick-map resolution, wire-route geometry, snapping and alignment, skin interpolation, sparkle overlays, canvas-hosted inline editors
- Accept: paint-event composition, pick resolution, window selection, wire and skin rendering, snap solving
- Reject: the `IFlexControl` seam internals (`api-gh2-flex.md`), document mutation, component execution, a re-derived paint-phase enum, an in-folder wire route, palette, or overlay, the GH1 `GH_Canvas`/`IGH_*` paint idiom
