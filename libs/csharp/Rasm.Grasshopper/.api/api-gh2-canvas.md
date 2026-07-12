# [RASM_GRASSHOPPER_API_GH2_CANVAS]

The Grasshopper2 `Canvas` is the document-hosting `ControlObject` that renders the node graph: eight ordered paint events, an offscreen pick-map read back at a point, per-category window-selection gates, the closed `WireShape` geometry family, `Skin`/`WireSkin` value interpolation, and the `Sparkles` overlay set. Projection, navigation, redraw scheduling, and animation cross the `IFlexControl` seam that `api-gh2-flex.md` owns; the canvas composes it and never re-derives it. Every member is catalog-verified against the installed RhinoWIP `Grasshopper2.xml` adjacent to `Grasshopper2.dll`; the GH1 `GH_Canvas`/`IGH_*` paint idiom is absent by construction.

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

`CanvasActions` carries 13 Boolean gates: `AllowDrag`, `AllowObjectSelect`, host-spelled `AlloWireSelect`, wire make/delete/modify, object make/delete, file drop, object response, and object/wire/canvas menus. `MakeWireFilter` and `DeleteWireFilter` are its two `(source, target) → bool` predicate slots.

| [INDEX] | [SYMBOL]                         | [KIND]     | [CAPABILITY]                                 |
| :-----: | :------------------------------- | :--------- | :------------------------------------------- |
|  [01]   | `Canvas`                         | control    | document host, paint, pick, and editor owner |
|  [02]   | `CanvasPaintEventArgs`           | paint args | `Canvas`, `Skin`, and `Graphics` carrier     |
|  [03]   | `CanvasBackgroundPaintEventArgs` | paint args | default-background suppression carrier       |
|  [04]   | `CanvasActions`                  | policy     | 13 gates and two wire predicates             |

[PUBLIC_TYPE_SCOPE]: wire geometry, snapping, and alignment
- namespace: `Grasshopper2.UI.Canvas`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]              | [KIND] | [CAPABILITY]                            |
| :-----: | :-------------------- | :----- | :-------------------------------------- |
|  [01]   | `WireShape`           | shape  | route creation, query, bounds, and draw |
|  [02]   | `WireShapeDefault`    | shape  | default spline creation                 |
|  [03]   | `SnappingAction`      | action | align, gap, and straighten candidates   |
|  [04]   | `SnappingConstraints` | solver | document-scoped snap resolution         |
|  [05]   | `SnappingSettings`    | policy | rules and feedback derivation           |

[PUBLIC_TYPE_SCOPE]: skinning and sparkle overlays
- namespace: `Grasshopper2.UI.Skinning`, `Grasshopper2.UI.Sparkles`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]        | [KIND]  | [CAPABILITY]                               |
| :-----: | :-------------- | :------ | :----------------------------------------- |
|  [01]   | `Skin`          | skin    | aggregate palette, folds, and persistence  |
|  [02]   | `WireSkin`      | skin    | state pens, end colours, and interpolation |
|  [03]   | `BlastSparkle`  | overlay | radial blast                               |
|  [04]   | `EdgeSparkle`   | overlay | edge highlight                             |
|  [05]   | `FaceSparkle`   | overlay | face highlight                             |
|  [06]   | `NoticeSparkle` | overlay | notice badge                               |
|  [07]   | `ISparkle`      | seam    | public custom-overlay contract             |

[PUBLIC_TYPE_SCOPE]: composed flex seam
- namespace: `Grasshopper2.UI.Flex`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]       | [KIND]    | [CAPABILITY]                                 |
| :-----: | :------------- | :-------- | :------------------------------------------- |
|  [01]   | `IFlexControl` | host seam | projection, selection, redraw, and animation |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the eight ordered paint events
- namespace: `Grasshopper2.UI.Canvas`
- rail: host-grasshopper

The four layers paint in fixed order — background, groups, wires, objects — each fenced by a `Before`/`After` pair; the pair IS the paint phase, so there is no `CanvasPaintPhase` host type.

| [INDEX] | [SURFACE]                                               | [CALL_SHAPE]                           | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------ | :------------------------------------- | :------------------------------- |
|  [01]   | `Canvas.BeforePaintBackground` / `AfterPaintBackground` | `event CanvasBackgroundPaintEventArgs` | fences the grid/background layer |
|  [02]   | `Canvas.BeforePaintGroups` / `AfterPaintGroups`         | `event CanvasPaintEventArgs`           | fences the group-hull layer      |
|  [03]   | `Canvas.BeforePaintWires` / `AfterPaintWires`           | `event CanvasPaintEventArgs`           | fences the wire layer            |
|  [04]   | `Canvas.BeforePaintObjects` / `AfterPaintObjects`       | `event CanvasPaintEventArgs`           | fences the object-capsule layer  |

[ENTRYPOINT_SCOPE]: picking, capture, and bitmap
- namespace: `Grasshopper2.UI.Canvas`
- rail: host-grasshopper

`ResolvePick(PointF, includeGrips, includeForeground, includeBackground, includeWires, recursive)` returns `SelectionResult`. `DrawToBitmap` admits the `(int, int, bool, bool, bool)` layer-toggle overload and the parameterless 500×300 all-layer overload. Projection, window-selection, dwell, and draw events remain inherited `FlexControl` members.

| [INDEX] | [SURFACE]                                     | [SHAPE]          | [CAPABILITY]                   |
| :-----: | :-------------------------------------------- | :--------------- | :----------------------------- |
|  [01]   | `Canvas.DrawPickMap`                          | `() → Bitmap`    | offscreen pick buffer          |
|  [02]   | `Canvas.ResolvePick`                          | gated point read | pick-map result                |
|  [03]   | `Canvas.DrawToBitmap`                         | two overloads    | canvas raster                  |
|  [04]   | `Canvas.AllowedActions`                       | property         | live action policy             |
|  [05]   | `Canvas.ActiveResponsive` / `OverResponsive`  | properties       | captured and hovered targets   |
|  [06]   | `Canvas.DocumentChanged` / `DocumentModified` | events           | document and dirty-state facts |

[ENTRYPOINT_SCOPE]: hosted editors, window-select gates, and snap axes
- namespace: `Grasshopper2.UI.Canvas`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                             | [SHAPE]    | [CAPABILITY]                 |
| :-----: | :------------------------------------ | :--------- | :--------------------------- |
|  [01]   | `Canvas.ShowInlineEditor`             | validated  | in-place parse and commit    |
|  [02]   | `Canvas.WindowSelect*`                | properties | category marquee gates       |
|  [03]   | `Canvas.SnapXAction` / `SnapYAction`  | properties | active axis nudges           |
|  [04]   | `Canvas.SkinLit` / `SkinDim` / `Skin` | properties | lit, dimmed, active palettes |

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
|  [08]   | `WireShapeDefault.CreateSpline` | `(PointF, PointF)` → `BezierF`                 | the default spline curve factory            |

[ENTRYPOINT_SCOPE]: snapping and alignment
- namespace: `Grasshopper2.UI.Canvas`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                          | [SHAPE]       | [CAPABILITY]               |
| :-----: | :------------------------------------------------- | :------------ | :------------------------- |
|  [01]   | `SnappingAction.Create*AlignAction`                | two frames    | edge or centre candidate   |
|  [02]   | `SnappingAction.Create*GapAction*`                 | frames + gap  | equal-gap candidate        |
|  [03]   | `SnappingAction.CreateStraightenWireAction`        | point pair    | straight-wire candidate    |
|  [04]   | `SnappingAction.SmallerMagnitude`                  | action pair   | shortest-nudge fold        |
|  [05]   | `SnappingConstraints.CreateFromDocument`           | two overloads | document constraint set    |
|  [06]   | `SnappingConstraints.SnapObject` / `SnapRectangle` | settings      | X/Y frame resolution       |
|  [07]   | `SnappingConstraints.SnapWires`                    | two overloads | wire resolution            |
|  [08]   | `SnappingConstraints.DrawSnappingBoxes`            | graphics      | live guide overlay         |
|  [09]   | `SnappingSettings.With*`                           | derivation    | rule and feedback variants |

[ENTRYPOINT_SCOPE]: skin interpolation and sparkle draws
- namespace: `Grasshopper2.UI.Skinning`, `Grasshopper2.UI.Sparkles`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                       | [SHAPE]        | [CAPABILITY]                |
| :-----: | :---------------------------------------------- | :------------- | :-------------------------- |
|  [01]   | `Skin.Interpolate`                              | skin + t       | palette blend               |
|  [02]   | seven `Skin.With*` folds                        | sub-skin       | one-row replacement         |
|  [03]   | `WireSkin.ResolveColours`                       | selected state | source and target colours   |
|  [04]   | `WireSkin.Interpolate`                          | wire skin + t  | wire-palette blend          |
|  [05]   | `BlastSparkle` ctor + `Draw`                    | blast + draw   | radial overlay              |
|  [06]   | `EdgeSparkle` / `FaceSparkle` / `NoticeSparkle` | draw           | edge, face, notice overlays |

## [04]-[IMPLEMENTATION_LAW]

[CANVAS_TOPOLOGY]:
- the paint pipeline is four layers in fixed order (background → groups → wires → objects), each fenced by a `Before`/`After` event pair; a hook attaches to the pair, and the phase is the event, never a `CanvasPaintPhase` enum
- picking is a two-surface contract: `DrawPickMap` renders the offscreen id buffer, `ResolvePick(point, …gates)` reads it back with per-category admission
- every paint arg carries `Canvas` + `Skin` + `Graphics`; the skin rows (`SkinLit`/`SkinDim`/`Skin`) supply the interpolated palette and `Graphics` is the `Eto.Drawing` target
- `CanvasActions` is the sole action gate: its 13 allow-toggles plus the two wire-filter predicate slots admit or deny drag, selection, wire make/delete/modify, object make/delete, file drop, object response, and the three context menus
- `WireShape` is the closed wire-route family; `Create` discriminates endpoint pair versus parameter-attribute pair and every route answers `Project`/`DistanceTo`/`Intersects`/`IsCoincident`/`Draw`
- snapping is document-scoped: `SnappingConstraints.CreateFromDocument` builds the constraint set, the `SnappingAction` factories mint align/gap/straighten candidates, and `SmallerMagnitude` folds the winning nudge under a `SnappingSettings` rule set
- skin interpolation is value-parametric: `Skin.Interpolate`/`WireSkin.Interpolate` blend at a parameter and `WireSkin.ResolveColours` emits the wire end-colour pair by state

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
