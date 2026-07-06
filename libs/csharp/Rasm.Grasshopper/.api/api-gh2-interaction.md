# [API_GH2_INTERACTION]

Catalog scope: canvas interaction — focus/responsive registration, hover/dwell/context menus, snapping and alignment, drag interactions, and layout solving over document objects.

[NAMESPACES]:
- `Grasshopper2.UI.Canvas` interaction — `Canvas` (`PushFocus`/`PopFocus`, `RegisterIResponsive`/`UnregisterIResponsive`, `MouseDwell`/`PopulateContextMenu` events, `ResponsivesForwards`, `MouseDwellDelay`, `RedrawOnMouseMove`, `SnapXAction`/`SnapYAction`), `IResponsive`/`Responses`, `MouseDwellEventArgs`, `PopulateContextMenuEventArgs`, `ObjectDragInteraction`, `SnappingConstraints` (`CreateFromDocument`, `SnapRectangle`, `SnapWires`), `SnappingSettings` (rules/feedback/gap/radius families), `SnapSpace` (`CreateOrthogonal`, `Snap`), `SnapGuideStyle`.
- `Grasshopper2.UI.Snap` — `SnappingAction` (left/centre/right/top/bottom align factories, `SmallerMagnitude`), `SnapProjection`, `StretchLayoutSolver` (add/solve/round).
- `Grasshopper2.Doc` layout crossing set — `IAttributes` (`Pivot`/`Bounds`/`AggregateBounds`/`Snappable`/`Layout`/`Move`), `IResizableAttributes`, `OCD` (align objects/move parameters, `Fixed`), `Connectivity` (`Find`, `SortCausally`), `Wire.AllWireEnds`, `ActionList`/`PivotAction`.
- `Grasshopper2.UI` — `ResizingFrame` (begin/continue/cursor-at), `GhUiNumber`.
