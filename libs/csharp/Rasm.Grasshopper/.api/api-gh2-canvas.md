# [RASM_GRASSHOPPER_API_GH2_CANVAS]

The Grasshopper2 `Canvas` is the document-hosting `ControlObject` that renders the node graph: eight ordered paint
events, an offscreen pick-map read back at a point, per-category window-selection gates, the closed `WireShape`
geometry family, `Skin`/`WireSkin` value interpolation, and the `Sparkles` overlay set. Projection, navigation,
redraw scheduling, and animation cross the `IFlexControl` seam that `api-gh2-flex.md` owns; the canvas composes it and
never re-derives it. Every member is catalog-verified against the installed RhinoWIP `Grasshopper2.xml` adjacent to
`Grasshopper2.dll`; the GH1 `GH_Canvas`/`IGH_*` paint idiom is absent by construction.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Grasshopper2`
- package: `Grasshopper2` (Rhino 9 WIP host plug-in bundle; not a NuGet pin — the in-process `Grasshopper2.dll` under `Grasshopper2Plugin.rhp` is the resolved asset)
- assembly: `Grasshopper2`
- namespace: `Grasshopper2.UI.Canvas`
- namespace: `Grasshopper2.UI.Skinning`
- namespace: `Grasshopper2.UI.Sparkles`
- namespace: `Grasshopper2.UI.Flex` (the `IFlexControl` seam this catalog composes; the member table is `api-gh2-flex.md`)
- asset: host assembly; managed WIP plug-in loaded in the Rhino assembly-load context, painting over `Eto.Drawing.Graphics`
- rail: host-grasshopper

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: canvas control, paint arguments, and action policy
- namespace: `Grasshopper2.UI.Canvas`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]                         | [KIND]     | [CAPABILITY]                                                                                                                                                                        |
| :-----: | :------------------------------- | :--------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Canvas`                         | control    | document-hosting `ControlObject`; owns the eight ordered paint events, pick map, skin rows, window-select gates, snap actions, and the inline/value editors                         |
|  [02]   | `CanvasPaintEventArgs`           | paint args | per-event carrier; `Canvas`, `Skin` (the interpolated palette), `Graphics` (the `Eto.Drawing` target)                                                                               |
|  [03]   | `CanvasBackgroundPaintEventArgs` | paint args | background-event carrier; `OverrideDefaultPainting` suppresses the built-in grid fill                                                                                               |
|  [04]   | `CanvasActions`                  | policy     | the nine allow-gates: `AllowDrag`/`AllowObjectSelect`/`AllowMakeWire`/`AllowDeleteWire`/`AllowModifyWire`/`AllowObjectResponse`/`AllowWireMenu`/`AllowObjectMenu`/`AllowCanvasMenu` |

[PUBLIC_TYPE_SCOPE]: wire geometry, snapping, and alignment
- namespace: `Grasshopper2.UI.Canvas`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]              | [KIND] | [CAPABILITY]                                                                                                                                           |
| :-----: | :-------------------- | :----- | :----------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `WireShape`           | shape  | abstract wire-route base; `ShapeType`, `Create` (endpoint-pair or parameter-attribute-pair), `Project`/`DistanceTo`/`Intersects`/`IsCoincident`/`Draw` |
|  [02]   | `WireShapeDefault`    | shape  | default spline route under `WireShape`; `CreateSpline(PointF, PointF)`                                                                                 |
|  [03]   | `SnappingAction`      | action | the align/gap/straighten candidate; the `Create*Align`/`Create*Gap`/`CreateStraightenWire` factory family, `SmallerMagnitude` picks the shorter nudge  |
|  [04]   | `SnappingConstraints` | solver | document-scoped snap resolution; built by `CreateFromDocument`, resolves objects/rectangles/wires against `SnappingSettings`                           |
|  [05]   | `SnappingSettings`    | policy | the active rule set; `Default`/`Current`, `WithRules`/`WithoutRules` toggle a `SnappingRule`, `WithFeedback` sets the guide colour                     |

[PUBLIC_TYPE_SCOPE]: skinning and sparkle overlays
- namespace: `Grasshopper2.UI.Skinning`, `Grasshopper2.UI.Sparkles`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]        | [KIND]  | [CAPABILITY]                                                                                                               |
| :-----: | :-------------- | :------ | :------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Skin`          | skin    | the aggregate palette; `Interpolate` blends two skins at `t`, `WithShades`/`WithWires`/`WithCanvasses` swap a sub-skin row |
|  [02]   | `WireSkin`      | skin    | wire-pen palette; `ResolveColours` emits the outer/inner pen colours by selection/proximity state, `Interpolate` blends    |
|  [03]   | `BlastSparkle`  | overlay | radial blast overlay; ctor over `BlastRadius` + `PointF` + `Color`, `Draw(ControlGraphics)`                                |
|  [04]   | `EdgeSparkle`   | overlay | edge-highlight overlay; `Draw(ControlGraphics)`                                                                            |
|  [05]   | `FaceSparkle`   | overlay | face-highlight overlay; `Draw(ControlGraphics)`                                                                            |
|  [06]   | `NoticeSparkle` | overlay | notice-badge overlay; `Draw(ControlGraphics)`                                                                              |

[PUBLIC_TYPE_SCOPE]: composed flex seam
- namespace: `Grasshopper2.UI.Flex`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]       | [KIND]    | [CAPABILITY]                                                                                                                                                                              |
| :-----: | :------------- | :-------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `IFlexControl` | host seam | the flex control seam `api-gh2-flex.md` owns; the canvas composes `Map`/`Navigate` for projection, `BeginWindowSelect`/`EndWindowSelect` for the marquee, `ScheduleRedraw`, and `Animate` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the eight ordered paint events
- namespace: `Grasshopper2.UI.Canvas`
- rail: host-grasshopper

The four layers paint in fixed order — background, groups, wires, objects — each fenced by a `Before`/`After` pair; the
pair IS the paint phase, so there is no `CanvasPaintPhase` host type.

| [INDEX] | [SURFACE]                                               | [CALL_SHAPE]                           | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------ | :------------------------------------- | :------------------------------- |
|  [01]   | `Canvas.BeforePaintBackground` / `AfterPaintBackground` | `event CanvasBackgroundPaintEventArgs` | fences the grid/background layer |
|  [02]   | `Canvas.BeforePaintGroups` / `AfterPaintGroups`         | `event CanvasPaintEventArgs`           | fences the group-hull layer      |
|  [03]   | `Canvas.BeforePaintWires` / `AfterPaintWires`           | `event CanvasPaintEventArgs`           | fences the wire layer            |
|  [04]   | `Canvas.BeforePaintObjects` / `AfterPaintObjects`       | `event CanvasPaintEventArgs`           | fences the object-capsule layer  |

[ENTRYPOINT_SCOPE]: picking, capture, and bitmap
- namespace: `Grasshopper2.UI.Canvas`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                     | [CALL_SHAPE]                             | [CAPABILITY]                                                                                                                                                                     |
| :-----: | :-------------------------------------------- | :--------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Canvas.DrawPickMap`                          | `()`                                     | renders the offscreen id buffer the pick read samples                                                                                                                            |
|  [02]   | `Canvas.ResolvePick`                          | `(PointF, bool, bool, bool, bool, bool)` | reads the pick map at a point with the per-category gates                                                                                                                        |
|  [03]   | `Canvas.DrawToBitmap`                         | `(int, int, bool, bool, bool)` → bitmap  | rasterizes the canvas at a size with layer toggles                                                                                                                               |
|  [04]   | `Canvas.WireDrawCache`                        | property                                 | the cached wire-route geometry the wire paint reuses                                                                                                                             |
|  [05]   | `Canvas.AllowedActions`                       | property (`CanvasActions`)               | the live action-policy gate                                                                                                                                                      |
|  [06]   | `Canvas.ActiveResponsive` / `OverResponsive`  | property                                 | the captured / hovered responsive                                                                                                                                                |
|  [07]   | `Canvas.DocumentChanged` / `DocumentModified` | `event EventHandler`                     | document-swap and dirty-flag observation; the flex-seam `ProjectionChanged`/`WindowSelection`/`MouseDwell`/`Draw` events are inherited `FlexControl` members (`api-gh2-flex.md`) |

[ENTRYPOINT_SCOPE]: hosted editors, window-select gates, and snap axes
- namespace: `Grasshopper2.UI.Canvas`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                                                 | [CALL_SHAPE]                                          | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------------------------------ | :---------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `Canvas.ShowInlineEditor`                                                 | `(RectangleF, string, Func<string, IResult>, Action)` | opens the in-place text editor with a parse validator + commit |
|  [02]   | `Canvas.ShowValueEditorPopup`                                             | `(AbstractParameter, Control)`                        | floats the parameter value editor over a control               |
|  [03]   | `Canvas.WindowSelectObjects` / `WindowSelectWires` / `WindowSelectGroups` | property                                              | the per-category marquee-selection gates                       |
|  [04]   | `Canvas.SnapXAction` / `SnapYAction`                                      | property (`SnappingAction`)                           | the active horizontal / vertical snap nudge                    |
|  [05]   | `Canvas.SkinLit` / `SkinDim` / `Skin`                                     | property (`Skin`)                                     | the lit / dimmed / active palette rows                         |

[ENTRYPOINT_SCOPE]: WireShape geometry
- namespace: `Grasshopper2.UI.Canvas`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                   | [CAPABILITY]                                |
| :-----: | :------------------------------ | :--------------------------------------------- | :------------------------------------------ |
|  [01]   | `WireShape.Create`              | `(IParameterAttributes, IParameterAttributes)` | routes a wire between two pin attributes    |
|  [02]   | `WireShape.Create`              | `(PointF, PointF)`                             | routes a wire between two raw endpoints     |
|  [03]   | `WireShape.Project`             | `(PointF)` → `PointF`                          | closest point on the route to a query point |
|  [04]   | `WireShape.DistanceTo`          | `(PointF)` → `float`                           | distance from the route to a query point    |
|  [05]   | `WireShape.Intersects`          | `(RectangleF)` → `bool`                        | marquee-hit test against the route          |
|  [06]   | `WireShape.IsCoincident`        | `(PointF, float)` → `bool`                     | on-route test within a tolerance            |
|  [07]   | `WireShape.Draw`                | `(Graphics, Pen)`                              | strokes the route with a pen                |
|  [08]   | `WireShapeDefault.CreateSpline` | `(PointF, PointF)` → `WireShape`               | the default spline route factory            |

[ENTRYPOINT_SCOPE]: snapping and alignment
- namespace: `Grasshopper2.UI.Canvas`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                                                          | [CALL_SHAPE]                                                                | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------------------------------------------- | :-------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `SnappingAction.CreateLeftAlignAction` / `Right` / `Top` / `Bottom` / `Centre`     | `(RectangleF, RectangleF, float)`                                           | an edge/centre alignment candidate between two frames |
|  [02]   | `SnappingAction.CreateVerticalGapActionOnRight` / `CreateHorizontalGapActionBelow` | `(RectangleF, RectangleF, int, float)`                                      | an equal-gap distribution candidate                   |
|  [03]   | `SnappingAction.CreateStraightenWireAction`                                        | `(PointF, PointF)`                                                          | a wire-straighten candidate                           |
|  [04]   | `SnappingAction.SmallerMagnitude`                                                  | `(SnappingAction, SnappingAction)` → `SnappingAction`                       | folds two candidates to the shorter nudge             |
|  [05]   | `SnappingConstraints.CreateFromDocument`                                           | `(Document, Guid[])` / `(Document, bool, bool, HashSet<Guid>)`              | builds the document snap constraint set               |
|  [06]   | `SnappingConstraints.SnapObject` / `SnapRectangle`                                 | `(…, SnappingSettings, RectangleF, out SnappingAction, out SnappingAction)` | resolves the X/Y snap for an object or frame          |
|  [07]   | `SnappingConstraints.SnapWires`                                                    | `(IDocumentObject, SnappingSettings)` / `(…, RectangleF, …)`                | resolves the wire snap                                |
|  [08]   | `SnappingConstraints.DrawSnappingBoxes`                                            | `(Graphics)`                                                                | draws the live snap-guide overlay                     |
|  [09]   | `SnappingSettings.WithRules` / `WithoutRules` / `WithFeedback`                     | `(SnappingRule)` / `(bool, Color)`                                          | derives a settings variant                            |

[ENTRYPOINT_SCOPE]: skin interpolation and sparkle draws
- namespace: `Grasshopper2.UI.Skinning`, `Grasshopper2.UI.Sparkles`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                               | [CALL_SHAPE]                                             | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------ | :------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `Skin.Interpolate`                                      | `(Skin, float)` → `Skin`                                 | blends two skins at a parameter                           |
|  [02]   | `Skin.WithShades` / `WithWires` / `WithCanvasses`       | `(Shades)` / `(WiresSkin)` / `(CanvassesSkin)`           | swaps one sub-skin row                                    |
|  [03]   | `WireSkin.ResolveColours`                               | `(bool, bool, out Color, out Color)`                     | emits outer/inner pen colours by selected/proximate state |
|  [04]   | `WireSkin.Interpolate`                                  | `(WireSkin, float)` → `WireSkin`                         | blends two wire skins                                     |
|  [05]   | `BlastSparkle` ctor + `Draw`                            | `(BlastRadius, PointF, Color)` / `Draw(ControlGraphics)` | mints and strokes a radial blast overlay                  |
|  [06]   | `EdgeSparkle` / `FaceSparkle` / `NoticeSparkle` `.Draw` | `(ControlGraphics)`                                      | strokes an edge / face / notice overlay                   |

## [04]-[IMPLEMENTATION_LAW]

[CANVAS_TOPOLOGY]:
- the paint pipeline is four layers in fixed order (background → groups → wires → objects), each fenced by a `Before`/`After` event pair; a hook attaches to the pair, and the phase is the event, never a `CanvasPaintPhase` enum
- picking is a two-surface contract: `DrawPickMap` renders the offscreen id buffer, `ResolvePick(point, …gates)` reads it back with per-category admission
- every paint arg carries `Canvas` + `Skin` + `Graphics`; the skin rows (`SkinLit`/`SkinDim`/`Skin`) supply the interpolated palette and `Graphics` is the `Eto.Drawing` target
- `CanvasActions` is the sole action gate: its nine allow-toggles admit or deny drag, object select, wire make/delete/modify, object response, and the three context menus
- `WireShape` is the closed wire-route family; `Create` discriminates endpoint pair versus parameter-attribute pair and every route answers `Project`/`DistanceTo`/`Intersects`/`IsCoincident`/`Draw`
- snapping is document-scoped: `SnappingConstraints.CreateFromDocument` builds the constraint set, the `SnappingAction` factories mint align/gap/straighten candidates, and `SmallerMagnitude` folds the winning nudge under a `SnappingSettings` rule set
- skin interpolation is value-parametric: `Skin.Interpolate`/`WireSkin.Interpolate` blend at a parameter and `WireSkin.ResolveColours` emits the pen pair by state

[STACKING]:
- `api-gh2-flex.md`(`.api/api-gh2-flex.md`): the `IFlexControl` seam owns projection (`Map`), navigation (`Navigate`), window-select (`BeginWindowSelect`/`EndWindowSelect`), responsive registration, redraw scheduling (`ScheduleRedraw`), and `Animate`; the canvas composes it and holds no parallel viewport transform
- `api-languageext.md`(`.api/api-languageext.md`): `ResolvePick` lands as `Option<Pick>`, paint hooks and pick reads ride `Eff`/`Fin`, and the snap candidates fold as a `Seq<SnappingAction>` reduced through `SmallerMagnitude`
- `api-thinktecture-runtime-extensions.md`(`.api/api-thinktecture-runtime-extensions.md`): `WireShape.ShapeType` and the sparkle set lower onto a `SmartEnum`/`Union`, and `CanvasActions` is a `ValueObject` policy record
- `api-unicolour.md`(`.api/api-unicolour.md`): `Skin.Interpolate` and `WireSkin.ResolveColours` blend in a perceptual space, crossing the `Eto.Drawing.Color` pen boundary at the edge

[LOCAL_ADMISSION]:
- the canvas surface is admitted only through the host `Canvas`/`Flex` seam; a paint hook attaches to the eight `Canvas` events, never a re-derived phase enum
- `WireShape`/`Skin`/`Sparkle` are composed as host types; a parallel in-folder wire route, palette, or overlay is the deleted form
- perceptual blending and easing math compose the Rasm kernel motion/colour owner, never a second in-folder derivation

[RAIL_LAW]:
- Package: `Grasshopper2` (host assembly)
- Owns: the canvas paint pipeline, pick-map resolution, wire-route geometry, snapping/alignment, skin interpolation, sparkle overlays, canvas-hosted inline/value editors
- Accept: paint-event composition, pick resolution, window selection, wire/skin rendering, snap solving
- Reject: the `IFlexControl` seam internals (`api-gh2-flex.md`), document mutation, component execution, the GH1 `GH_Canvas`/`IGH_*` paint idiom
