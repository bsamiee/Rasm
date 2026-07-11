# [RASM_GRASSHOPPER_API_GH2_INTERACTION]

`Grasshopper2` routes every canvas pointer, key, and layout event through one responsive dispatch spine — `Grasshopper2.UI.Flex.IFlexControl` registers `IResponsive` handlers, focuses them, and relays mouse, key, and text input as a `Response` verdict that governs event capture and redraw. Object dragging is `Grasshopper2.UI.Canvas.ObjectDragInteraction` over a live `Document`; alignment and gap snapping are `SnappingConstraints` producing `SnappingAction`s under `SnappingSettings`; numeric-space snapping is `Grasshopper2.UI.Snap.SnapSpace`; and stretch layout is `StretchLayoutSolver` with `ResizingFrame` owning the resize-edge state. This surface crosses the `Grasshopper2.Doc` object graph at `Document` and `IDocumentObject` and reports geometry in `Eto.Drawing` coordinates.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grasshopper2` canvas interaction
- host: `Grasshopper2.dll` inside `Grasshopper2Plugin.rhp`, loaded in-process by Rhino 9 WIP
- namespace: `Grasshopper2.UI.Flex` — `IFlexControl`, `IResponsive`, `Response`, `ResponseMouseArgs`, `Responses`
- namespace: `Grasshopper2.UI.Canvas` — `ObjectDragInteraction`, `SnappingConstraints`, `SnappingSettings`, `SnappingAction`
- namespace: `Grasshopper2.UI.Snap` — `SnapSpace` numeric snapping
- namespace: `Grasshopper2.UI` — `StretchLayoutSolver`, `ResizingFrame`
- crossing: `Grasshopper2.Doc` — `Document`, `IDocumentObject`; `Eto.Drawing` — `PointF`, `RectangleF`, `SizeF`, `Padding`, `Graphics`
- rail: gh2-interaction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: responsive dispatch spine (`Grasshopper2.UI.Flex`)
- rail: gh2-interaction

| [INDEX] | [SYMBOL]            | [KIND]    | [CAPABILITY]                                                                              |
| :-----: | :------------------ | :-------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `IFlexControl`      | interface | the canvas host seam — focus stack, responsive registration, window select, coordinates   |
|  [02]   | `IResponsive`       | interface | one registered interaction handler, addressed through its `Responder`                     |
|  [03]   | `Response`          | enum      | the event verdict — `Ignored`, `Release`, `Handled`, `Capture` — governing propagation    |
|  [04]   | `ResponseMouseArgs` | class     | a mouse event in control and content coordinates with buttons, modifiers, delta, pressure |
|  [05]   | `Responses`         | class     | the relay folding raw mouse, key, and text events into `Response`-returning handler calls |

[PUBLIC_TYPE_SCOPE]: object drag and snapping (`Grasshopper2.UI.Canvas`)
- rail: gh2-interaction

| [INDEX] | [SYMBOL]                | [KIND] | [CAPABILITY]                                                                     |
| :-----: | :---------------------- | :----- | :------------------------------------------------------------------------------- |
|  [01]   | `ObjectDragInteraction` | class  | a live drag of document objects — control, document, count, first/last points    |
|  [02]   | `SnappingConstraints`   | class  | the per-document snap target set — feedback boxes and rectangle or wire snapping |
|  [03]   | `SnappingSettings`      | class  | the active snap rule, feedback, gap, and radius policy the solver reads          |
|  [04]   | `SnappingAction`        | class  | one resolved snap adjustment produced as an out-parameter                        |

[PUBLIC_TYPE_SCOPE]: numeric snapping and layout (`Grasshopper2.UI.Snap`, `Grasshopper2.UI`)
- rail: gh2-interaction

| [INDEX] | [SYMBOL]              | [KIND] | [CAPABILITY]                                                                      |
| :-----: | :-------------------- | :----- | :-------------------------------------------------------------------------------- |
|  [01]   | `SnapSpace`           | class  | a numeric snap lattice over `ISnapElement`s — orthogonal grids, merge, snapping   |
|  [02]   | `StretchLayoutSolver` | class  | a min/preferred/max stretch solver distributing a total across segments           |
|  [03]   | `ResizingFrame`       | class  | an interactive resize of a bounded rectangle — per-edge state and cursor hit-test |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: focus, registration, responsive relay
- rail: gh2-interaction

The pointer relay is `MouseOver`, `MouseLeave`, `MouseDown`, `MouseDrag`, `MouseUp`, `MouseWheel`, `MouseSingleClick`, and `MouseDoubleClick`; a `ResponseMouseArgs` carries `ControlLocation`, `ContentLocation`, `Buttons`, `Modifiers`, `Delta`, `Pressure`, and `Handled`.

| [INDEX] | [SURFACE]                                            | [CALL_SHAPE]          | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------- | :-------------------- | :-------------------------------------- |
|  [01]   | `IFlexControl.PushFocus` / `PopFocus`                | `(IResponsive)`       | grant and release exclusive focus       |
|  [02]   | `IFlexControl.RegisterIResponsive` / `Unregister…`   | `(IResponsive)`       | add or drop a handler                   |
|  [03]   | `IFlexControl.ResponsivesForwards`                   | property              | the ordered live responsive set         |
|  [04]   | `IFlexControl.BeginWindowSelect` / `EndWindowSelect` | `()`                  | open and close window selection         |
|  [05]   | `Responses.Invoke*Relay`                             | `(Func, args)`        | drive a handler and fold its `Response` |
|  [06]   | `Responses.Mouse*`                                   | `(ResponseMouseArgs)` | the pointer family returning `Response` |
|  [07]   | `Responses.KeyDown` / `KeyUp` / `TextInput`          | `(KeyEventArgs, …)`   | keyboard and text → `Response`          |
|  [08]   | `Responses.RedrawRequired` / `OnRedrawRequired`      | event / `()`          | request a canvas repaint                |
|  [09]   | `ResponseMouseArgs.ContentLocation` / `Pressure`     | property              | dual-coordinate state and pressure      |

[ENTRYPOINT_SCOPE]: object drag and constraint snapping
- rail: gh2-interaction

A drag exposes `Control`, `Document`, `Count`, `FirstPoint`, and `LastPoint`.

| [INDEX] | [SURFACE]                                | [CALL_SHAPE]                           | [CAPABILITY]                     |
| :-----: | :--------------------------------------- | :------------------------------------- | :------------------------------- |
|  [01]   | `new ObjectDragInteraction`              | `(IFlexControl, Document, PointF)`     | begin a drag at a canvas point   |
|  [02]   | `ObjectDragInteraction.*`                | property                               | control, document, count, travel |
|  [03]   | `SnappingConstraints.CreateFromDocument` | `(Document, …, HashSet<Guid>)`         | snap set excluding dragged ids   |
|  [04]   | `SnappingConstraints.SnapRectangle`      | `(RectangleF×2, out SnappingAction×2)` | X/Y snap for a moving rectangle  |
|  [05]   | `SnappingConstraints.SnapWires`          | `(IDocumentObject, …)`                 | wire-alignment snapping          |
|  [06]   | `SnappingConstraints.DrawSnappingBoxes`  | `(Graphics)`                           | paint the snap-target feedback   |

[ENTRYPOINT_SCOPE]: numeric snapping, stretch layout, resize
- rail: gh2-interaction

`ResizingFrame` edge flags are `ResizeTopEdge`, `ResizeLeftEdge`, `ResizeRightEdge`, and `ResizeBottomEdge`.

| [INDEX] | [SURFACE]                                     | [CALL_SHAPE]                           | [CAPABILITY]                       |
| :-----: | :-------------------------------------------- | :------------------------------------- | :--------------------------------- |
|  [01]   | `SnapSpace.Create` / `CreateOrthogonal`       | `(ISnapElement[])` / `(double×4)`      | element or orthogonal-grid lattice |
|  [02]   | `SnapSpace.Merge` / `Empty`                   | `(SnapSpace)` / property               | compose lattices and the identity  |
|  [03]   | `SnapSpace.Snap`                              | `(double×2, out double×2, out string)` | snap a pair, reporting the rule    |
|  [04]   | `StretchLayoutSolver.Add` / `Solve` / `Round` | `(float×3)` / `(float)`                | segment, distribute, round         |
|  [05]   | `StretchLayoutSolver.Sum`                     | `(IReadOnlyList<float>)`               | total a length list                |
|  [06]   | `new ResizingFrame`                           | `(RectangleF, SizeF×2, …)`             | resize bounded by size and snap    |
|  [07]   | `ResizingFrame.Begin` / `Continue`            | `(PointF, …)` / `(PointF)`             | start and track a resize           |
|  [08]   | `ResizingFrame.CursorAt`                      | `(PointF, Padding)`                    | the resize cursor for an edge      |

## [04]-[IMPLEMENTATION_LAW]

[GH2_INTERACTION_TOPOLOGY]:
- Every canvas event enters through `Grasshopper2.UI.Flex.IFlexControl` and reaches handlers as registered `IResponsive`s; `PushFocus` grants one handler exclusive delivery until `PopFocus`, and the handler's `Response` verdict — `Handled`, `Capture`, `Release`, or `Ignored` — decides propagation and focus retention.
- Pointer events carry `ResponseMouseArgs` in both control and content coordinates; a handler snaps at content coordinates and paints at control coordinates, and its `RedrawRequired` request schedules the repaint rather than painting inline.
- `ObjectDragInteraction` owns a live object drag over a `Document`; `SnappingConstraints.CreateFromDocument` excludes the dragged ids, and `SnapRectangle`/`SnapWires` return `SnappingAction` out-parameters under a `SnappingSettings` policy.
- Numeric snapping is `Grasshopper2.UI.Snap.SnapSpace`, distinct from the canvas `SnappingConstraints`: a `SnapSpace` snaps abstract coordinate pairs against a lattice while constraints snap canvas rectangles against document objects.
- Layout is two owners — `StretchLayoutSolver` distributes one length across min/preferred/max segments, and `ResizingFrame` holds the per-edge resize state bounded by size limits and the same snapping surface.

[STACKING]:
- `api-languageext`: the responsive relay returns `Response`, and the folder lifts the mouse, key, and text relays onto one polymorphic `Eff<Response>` handler so a drag, snap, or resize gesture is one effect; `SnappingConstraints.SnapRectangle`/`SnapWires` out-parameters surface as `Option<SnappingAction>`; `SnapSpace.Snap` and `StretchLayoutSolver.Solve` fold onto `Fin`; and the live `IFlexControl.ResponsivesForwards` set carries as `Seq<IResponsive>`.
- `api-thinktecture-runtime-extensions`: `Response` is owned as a `[SmartEnum]` so relay dispatch is exhaustive; the pointer, key, and text event kinds collapse into one `[Union]` interaction event that the responsive spine matches; and `ResizingFrame`'s four edge flags and a `SnappingSettings` rule/feedback/gap/radius policy are `[ComplexValueObject]` records with structural equality.

[LOCAL_ADMISSION]:
- Canvas interaction is the Rasm.Grasshopper folder's own domain; it composes the Rasm kernel for host-agnostic geometry and easing math and never references a sibling Rasm package.
- An interaction enters the folder's owner as an `IResponsive` registered on an `IFlexControl`; a handler that paints or mutates outside the `Response`/`RedrawRequired` contract is not admitted.

[RAIL_LAW]:
- Package: `Grasshopper2` (canvas interaction)
- Owns: responsive event dispatch, object dragging, constraint and numeric snapping, stretch layout, and interactive resize over document objects
- Accept: `IResponsive` registration, `Response`-returning event handling, snap resolution, and layout and resize solving
- Reject: canvas paint and skinning (`api-gh2-canvas`), editor chrome and toolbars (`api-gh2-editor`), the document graph and mutation verbs (`api-gh2-document`), and animation timing (`api-gh2-flex`)
