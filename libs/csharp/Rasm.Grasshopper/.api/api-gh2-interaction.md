# [RASM_GRASSHOPPER_API_GH2_INTERACTION]

`Grasshopper2` canvas interaction drives object dragging, alignment and gap snapping, numeric-space snapping, stretch layout distribution, and interactive edge resize over a live `Document`, reporting geometry in `Eto.Drawing` coordinates. Canvas-rectangle snapping against document objects (`SnappingConstraints`) stays distinct from abstract numeric-lattice snapping (`SnapSpace`). Every interaction registers as an `IResponsive` on the `api-gh2-flex` `IFlexControl` seam, which owns the responsive dispatch spine this surface enters through.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Grasshopper2`
- package: `Grasshopper2` (Rhino 9 WIP host plug-in bundle; not a NuGet pin — the in-process `Grasshopper2.dll` under `Grasshopper2Plugin.rhp` is the resolved asset)
- assembly: `Grasshopper2`
- namespace: `Grasshopper2.UI.Canvas` (`ObjectDragInteraction`, `SnappingConstraints`, `SnappingSettings`, `SnappingAction`)
- namespace: `Grasshopper2.UI.Snap` (`SnapSpace`)
- namespace: `Grasshopper2.UI` (`StretchLayoutSolver`, `ResizingFrame`)
- asset: host assembly; `/Applications/RhinoWIP.app/Contents/Frameworks/RhCore.framework/Versions/Current/Resources/ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.dll` resolved from the installed RhinoWIP bundle
- rail: gh2-interaction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: canvas drag, snapping, and layout types

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                                     |
| :-----: | :---------------------- | :------------ | :------------------------------------------------------------------------------- |
|  [01]   | `ObjectDragInteraction` | class         | live drag of document objects — control, document, count, first point, responder |
|  [02]   | `SnappingConstraints`   | class         | per-document snap target set — feedback boxes, rectangle and wire snapping       |
|  [03]   | `SnappingSettings`      | class         | active snap rule, feedback, gap, and radius policy the solver reads              |
|  [04]   | `SnappingAction`        | class         | one resolved snap adjustment                                                     |
|  [05]   | `SnapSpace`             | class         | numeric snap lattice over `ISnapElement`s — orthogonal grids, merge, snap        |
|  [06]   | `StretchLayoutSolver`   | class         | min/ideal/max stretch solver distributing a total across segments                |
|  [07]   | `ResizingFrame`         | class         | interactive resize of a bounded rectangle — per-edge state and cursor hit-test   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: object drag and constraint snapping

A drag exposes `Control`, `Document`, `Count`, `FirstPoint`, and `Responder` (a nested `DragResponsive` carrying the `Orthogonal` toggle); `LastPoint` is private and never drag evidence.

| [INDEX] | [SURFACE]                                | [SHAPE]                   | [CAPABILITY]                   |
| :-----: | :--------------------------------------- | :------------------------ | :----------------------------- |
|  [01]   | `new ObjectDragInteraction`              | host + doc + anchor       | begin a canvas drag            |
|  [02]   | `ObjectDragInteraction.*`                | properties                | drag state and responder       |
|  [03]   | `SnappingConstraints.CreateFromDocument` | doc + filter ids          | snap set excluding dragged ids |
|  [04]   | `SnappingConstraints.SnapRectangle`      | frame + settings, out ×2  | X/Y frame snap                 |
|  [05]   | `SnappingConstraints.SnapWires`          | static → `SnappingAction` | wire-alignment snap            |
|  [06]   | `SnappingConstraints.DrawSnappingBoxes`  | graphics                  | snap-target feedback           |

[ENTRYPOINT_SCOPE]: numeric snapping, stretch layout, resize

`ResizingFrame` edge state is `ResizeTopEdge`, `ResizeLeftEdge`, `ResizeRightEdge`, and `ResizeBottomEdge`.

| [INDEX] | [SURFACE]                                     | [SHAPE]                    | [CAPABILITY]                 |
| :-----: | :-------------------------------------------- | :------------------------- | :--------------------------- |
|  [01]   | `SnapSpace.Create` / `CreateOrthogonal`       | element or grid            | build a numeric lattice      |
|  [02]   | `SnapSpace.Merge` / `Empty`                   | monoid                     | composition and identity     |
|  [03]   | `SnapSpace.Snap`                              | pair + cutoff, out ×2      | snapped pair and rule        |
|  [04]   | `StretchLayoutSolver.Add` / `Solve` / `Round` | min/ideal/max → residual   | segment, distribute, round   |
|  [05]   | `new ResizingFrame`                           | bounds + min/max + snap    | size- and snap-bounded frame |
|  [06]   | `ResizingFrame.Begin` / `Continue`            | mouse + edges              | resize lifecycle             |
|  [07]   | `ResizingFrame.CursorAt`                      | mouse + padding → `Cursor` | edge cursor                  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ObjectDragInteraction` runs a live drag over a `Document` from an anchor; a handler registered on the `api-gh2-flex` `IFlexControl` seam snaps at content coordinates and paints at control coordinates.
- `SnappingConstraints.CreateFromDocument` builds the drag snap set excluding the dragged ids; `SnapRectangle` yields X and Y `SnappingAction` out-parameters and `SnapWires` returns one `SnappingAction`, both under a `SnappingSettings` rule/feedback/gap/radius policy.
- Numeric snapping is `SnapSpace`, distinct from canvas `SnappingConstraints`: `SnapSpace` snaps abstract coordinate pairs against a lattice while constraints snap canvas rectangles against document objects.
- Layout splits two owners — `StretchLayoutSolver` distributes one length across min/ideal/max segments, and `ResizingFrame` holds per-edge resize state bounded by size limits and the same snapping surface.

[STACKING]:
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): `SnapRectangle`'s X/Y out-parameters and `SnapWires`'s return surface as `Option<SnappingAction>`; `SnapSpace.Snap` and `StretchLayoutSolver.Solve` fold onto `Fin` where an unsatisfiable target maps to `Error`; a drag over `ObjectDragInteraction` sequences its snap-resolve steps through `Eff` and carries the excluded-id set as a `Seq<Guid>`.
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): `ResizingFrame`'s four edge flags collapse into one `[Union]` resize-edge state, `SnappingSettings` rule/feedback/gap/radius is a `[ComplexValueObject]` with structural equality, and `SnappingAction` is owned as a `[ValueObject]` snap adjustment equatable as one value.
- Rasm.Grasshopper folder: interaction handlers thread canvas geometry through the Rasm kernel's host-agnostic owners, never a second in-folder snap or easing derivation.

[LOCAL_ADMISSION]:
- Canvas interaction is the Rasm.Grasshopper folder's own domain, composing the Rasm kernel for host-agnostic geometry and referencing no sibling Rasm package.
- An interaction enters as an `IResponsive` registered on the `api-gh2-flex` `IFlexControl` seam; a handler painting or mutating outside the flex `Response`/`RedrawRequired` contract is not admitted.
- Snapping enters through `SnappingConstraints` for canvas rectangles or `SnapSpace` for numeric pairs; a hand-rolled alignment or grid solve is the deleted form.

[RAIL_LAW]:
- Package: `Grasshopper2` (canvas interaction)
- Owns: object dragging, constraint and numeric snapping, stretch layout distribution, and interactive edge resize over document objects
- Accept: drag construction, snap resolution, lattice snapping, and layout and resize solving
- Reject: responsive event dispatch and the `IFlexControl` seam (`api-gh2-flex`), canvas paint and skinning (`api-gh2-canvas`), editor chrome and toolbars (`api-gh2-editor`), and the document graph and mutation verbs (`api-gh2-document`)
