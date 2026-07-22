# [RASM_RHINO_API_RHINOCOMMON_CUSTOM_OBJECTS]

This catalog owns the quarantined host-subclass authoring boundary: `ClassIdAttribute`-registered `RhinoObject` derivations whose protected virtuals forward every host callback to a value-typed override program, the delegate-populated custom-grip authoring surface, the value-shaped `GripObject` edit surface, and the `Rhino.UI` on-canvas grip widgets. Subclassing enters only at the registration boundary, and no derivation owns domain state or dispatch — each is a registration shim over an immutable program.

Object read/mutate and history route to the objects catalog, placement and events to the document catalog, pick projection to the commands catalog, and geometry custody to the geometry catalog.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon custom-object and grip authoring
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon.dll`
- namespaces: `Rhino.DocObjects`, `Rhino.DocObjects.Custom`, `Rhino.UI`
- kernel: `Rasm` (host-agnostic vocabularies and numeric owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: custom-authoring-boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: custom object derivations

| [INDEX] | [SYMBOL]            | [KIND]                        | [CAPABILITY]                                                    |
| :-----: | :------------------ | :---------------------------- | :-------------------------------------------------------------- |
|  [01]   | `CustomBrepObject`  | abstract class, `BrepObject`  | registrable brep-backed custom object over the virtual program  |
|  [02]   | `CustomCurveObject` | abstract class, `CurveObject` | registrable curve-backed custom object over the virtual program |
|  [03]   | `CustomMeshObject`  | abstract class, `MeshObject`  | registrable mesh-backed custom object over the virtual program  |
|  [04]   | `CustomPointObject` | abstract class, `PointObject` | registrable point-backed custom object over the virtual program |
|  [05]   | `ClassIdAttribute`  | sealed attribute              | GUID registration stamp resolving a subclass at construction    |

[PUBLIC_TYPE_SCOPE]: grip authoring and editing

| [INDEX] | [SYMBOL]                  | [KIND]                        | [CAPABILITY]                                                      |
| :-----: | :------------------------ | :---------------------------- | :---------------------------------------------------------------- |
|  [01]   | `CustomObjectGrips`       | abstract class, `IDisposable` | grip-set authoring: enabler registration and geometry rebuild     |
|  [02]   | `CustomGripObject`        | class, `GripObject`           | authored grip carrying an index, origin, weight, and new location |
|  [03]   | `GripObject`              | class, `RhinoObject`          | value-shaped grip edit surface: move, weight, and CV parameters   |
|  [04]   | `GripStatus`              | class                         | per-grip draw-state carrier                                       |
|  [05]   | `GripsDrawEventArgs`      | class, `DrawEventArgs`        | grip draw callback payload                                        |
|  [06]   | `TurnOnGripsEventHandler` | delegate                      | grips-enabler callback registered against a custom-grips type     |

[PUBLIC_TYPE_SCOPE]: on-canvas grip widgets

| [INDEX] | [SYMBOL]                           | [KIND]                           | [CAPABILITY]                                                 |
| :-----: | :--------------------------------- | :------------------------------- | :----------------------------------------------------------- |
|  [01]   | `UserInterfaceObjectBase`          | abstract class                   | widget base: visibility, registration, mouse and draw hooks  |
|  [02]   | `GripUserInterfaceObject`          | class, `UserInterfaceObjectBase` | draggable on-canvas grip widget with snap constraints        |
|  [03]   | `DirectionGripUserInterfaceObject` | class                            | axis-constrained grip-widget variant                         |
|  [04]   | `RotationGripUserInterfaceObject`  | class                            | rotation grip-widget variant                                 |
|  [05]   | `TextDotUserInterfaceObject`       | class, `UserInterfaceObjectBase` | on-canvas text-dot widget                                    |
|  [06]   | `ViewUserInterfaceTable`           | sealed table                     | per-document widget registration via `doc.ViewUserInterface` |
|  [07]   | `GripUserInterfaceObjectShape`     | enum                             | grip-widget glyph vocabulary                                 |

[ENUM_ROSTERS]:
- `public enum Rhino.UI.GripUserInterfaceObjectShape` — `Circle = 0`, `Square = 1`, `Triangle = 2`, `X = 3`.

## [03]-[ENTRYPOINTS]

[CUSTOM_OBJECT_REGISTRATION]:
- `Rhino.DocObjects.Custom.CustomBrepObject.CustomBrepObject()` / `CustomBrepObject(Brep brep)` — parameterless and geometry-seeded protected constructors; the host constructs the subclass, so the derivation carries no state.
- `Rhino.DocObjects.Custom.CustomCurveObject.CustomCurveObject()` / `CustomCurveObject(Curve curve)` — curve-backed constructors; `SetCurve(Curve curve) : Curve` restages the backing curve.
- `Rhino.DocObjects.Custom.CustomMeshObject.CustomMeshObject()` / `CustomMeshObject(Mesh mesh)` — mesh-backed constructors.
- `Rhino.DocObjects.Custom.CustomPointObject.CustomPointObject()` / `CustomPointObject(Point point)` — point-backed constructors.
- `Rhino.DocObjects.Custom.ClassIdAttribute.ClassIdAttribute(string id)` — stamps a subclass with the GUID Rhino keys construction on; `Id : Guid` reads it back. Each custom kind carries exactly one stamp.

[CUSTOM_OBJECT_HOOKS]:
- `Rhino.DocObjects.RhinoObject.OnDraw(DrawEventArgs e) : void` — protected draw override; a custom kind forwards to a draw-mark composition against the display catalog.
- `Rhino.DocObjects.RhinoObject.OnDuplicate(RhinoObject source) : void` — sole copy hook rebuilding derived state from the source; `DuplicateGeometry()` is public and non-virtual, never an override point.
- `Rhino.DocObjects.RhinoObject.OnTransform(Transform transform) : void` / `OnSpaceMorph(SpaceMorph morph) : void` — transform and morph hooks applying the deformation to backing geometry.
- `Rhino.DocObjects.RhinoObject.OnAddToDocument(RhinoDoc doc) : void` / `OnDeleteFromDocument(RhinoDoc doc) : void` — document-membership lifecycle hooks.
- `Rhino.DocObjects.RhinoObject.OnPick(PickContext context) : IEnumerable<ObjRef>` / `OnPicked(PickContext context, IEnumerable<ObjRef> pickedItems) : void` — pick-participation hooks; the projection off the returned refs stays with the commands catalog.
- `Rhino.DocObjects.RhinoObject.OnSelectionChanged() : void` / `GetBoundingBox(RhinoViewport viewport) : BoundingBox` / `GetTightBoundingBox(ref BoundingBox tightBox, bool growBox, Transform xform) : bool` — selection and view-dependent extent hooks; `IsActiveInViewport(RhinoViewport viewport) : bool` is the one PUBLIC virtual in the roster.
- hook-roster closure: `OnDraw`, `GetBoundingBox`, `GetTightBoundingBox`, `OnDuplicate`, `OnDeleteFromDocument`, `OnAddToDocument`, `OnPick`, `OnPicked`, `OnSelectionChanged`, `OnTransform`, `OnSpaceMorph` is the complete protected-virtual set — no `OnGeometryChanged`, `OnAttributesChanged`, or `OnFlash` exists.

[CUSTOM_GRIPS_AUTHORING]:
- `Rhino.DocObjects.Custom.TurnOnGripsEventHandler(RhinoObject rhObj) : void` — enabler delegate signature.
- `Rhino.DocObjects.Custom.CustomObjectGrips.RegisterGripsEnabler(TurnOnGripsEventHandler enabler, Type customGripsType) : void` — registers the enabler Rhino invokes to turn grips on; resolution keys on the grips type's own `[Guid]` (`GetType().GUID`), never `ClassIdAttribute`, throws `ArgumentException` on a non-`CustomObjectGrips` type, and re-registering a GUID replaces its enabler.
- `Rhino.DocObjects.Custom.CustomObjectGrips.CustomObjectGrips()` (protected) / `AddGrip(CustomGripObject grip) : void` (protected) / `Grip(int index) : CustomGripObject` (public, throws `ArgumentOutOfRangeException` out of bounds) / `GripCount : int` / `OwnerObject : RhinoObject` — grip-set population is subclass-only; the roster reads are public.
- `Rhino.DocObjects.Custom.CustomObjectGrips.NewLocation : bool` (get/set — derived classes clear it after a temp-display update) / `GripsMoved : bool` (get) — per-instance rebuild gates; `Dragging() : bool` is `public static` and answers the GLOBAL drag state, never an instance flag.
- `Rhino.DocObjects.Custom.CustomObjectGrips.OnReset() : void` / `OnResetMeshes() : void` / `OnUpdateMesh(MeshType meshType) : void` — protected reset and mesh-refresh overrides.
- `Rhino.DocObjects.Custom.CustomObjectGrips.NewGeometry() : GeometryBase` — protected override fired ONCE at the end of a grip drag, returning geometry computed from current grip locations; the default answers null (keep existing).
- `Rhino.DocObjects.Custom.CustomObjectGrips.OnDraw(GripsDrawEventArgs args) : void` — protected grip-draw override; the base call draws the grips themselves, so a custom body draws dynamic elements first and then calls base.
- `Rhino.DocObjects.Custom.CustomObjectGrips.NeighborGrip(int gripIndex, int dr, int ds, int dt, bool wrap) : GripObject` / `NurbsSurfaceGrip(int i, int j) : GripObject` / `NurbsSurface() : NurbsSurface` — protected topology overrides for grid-shaped grip sets.
- `Rhino.DocObjects.Custom.CustomObjectGrips.Dispose(bool disposing) : void` (protected virtual) — the real disposal override point behind `IDisposable`.

[CUSTOM_GRIP_OBJECT]:
- `Rhino.DocObjects.Custom.CustomGripObject.CustomGripObject()` — parameterless construction; `NewLocation() : void` is the virtual invoked when the grip moves.
- `Rhino.DocObjects.Custom.CustomGripObject.Index : int` (new) / `OriginalLocation : Point3d` (new) — authored grip identity and origin shadowing the base members.
- `Rhino.DocObjects.Custom.CustomGripObject.Weight : double` (override) — the base override returns the sentinel `-1.23432101234321E+308` and its setter is a no-op, so an authored grip MUST override `Weight` again to carry a real value; only the plain `GripObject.Weight` is host-backed.

[GRIPS_DRAW_PAYLOAD]:
- `Rhino.DocObjects.Custom.GripsDrawEventArgs : DrawEventArgs` — `DrawStaticStuff : bool` / `DrawDynamicStuff : bool` — which pass this callback serves; `ControlPolygonStyle : int` (get/set: `0` none, `1` solid, `2` dotted) / `GripColor : Color` / `LockedGripColor : Color` / `SelectedGripColor : Color` (get/set).
- `Rhino.DocObjects.Custom.GripsDrawEventArgs.GripStatusCount : int` / `GripStatus(int index) : GripStatus` — lazily materialized per-grip draw state.
- `Rhino.DocObjects.Custom.GripsDrawEventArgs.DrawControlPolygonLine(Line line, GripStatus startStatus, GripStatus endStatus) : void` / `DrawControlPolygonLine(Line line, int startStatus, int endStatus) : void` / `DrawControlPolygonLine(Point3d start, Point3d end, int startStatus, int endStatus) : void` / `RestoreViewportSettings() : void` — control-polygon drawing and viewport-state restore.
- `Rhino.DocObjects.Custom.GripStatus` — public surface is exactly `Culled : bool` (get/set) and `Visible : bool` (get); constructed only by `GripsDrawEventArgs.GripStatus(int)`.

[GRIP_EDIT_SURFACE]:
- `Rhino.DocObjects.GripObject.CurrentLocation : Point3d` / `OriginalLocation : Point3d` / `Moved : bool` / `Weight : double` / `OwnerId : Guid` / `Index : int` — grip position, movement state, weight, and owner identity.
- `Rhino.DocObjects.GripObject.Move(Point3d newLocation) : void` / `Move(Vector3d delta) : void` / `Move(Transform xform) : void` / `UndoMove() : void` — grip relocation by absolute point, delta, or transform, and single-step undo.
- `Rhino.DocObjects.GripObject.NeighborGrip(int directionR, int directionS, int directionT, bool wrap) : GripObject` — adjacent grip in a grid-shaped set.
- `Rhino.DocObjects.GripObject.GetGripDirections(out Vector3d u, out Vector3d v, out Vector3d normal) : bool` — local frame at the grip.
- `Rhino.DocObjects.GripObject.GetSurfaceParameters(out double u, out double v) : bool` / `GetCurveParameters(out double t) : bool` / `GetCageParameters(out double u, out double v, out double w) : bool` — parameter-space coordinates for surface, curve, and cage grips.
- `Rhino.DocObjects.GripObject.GetCurveCVIndices(out int[] cvIndices) : int` / `GetSurfaceCVIndices(out Tuple<int, int>[] cvIndices) : int` — control-vertex indices this grip drives.

[GRIP_WIDGETS]:
- `Rhino.UI.GripUserInterfaceObject.GripUserInterfaceObject(Point3d location)` — mints an on-canvas grip widget at a location.
- `Rhino.UI.GripUserInterfaceObject.GripShape : GripUserInterfaceObjectShape` / `GripShapeRotationRadians : float` / `GripStrokeWidth : float` / `GripRadius : float` / `GripColor : Color` / `GripFillColor : Color` / `GripLocation : Point3d` — glyph, geometry, and color of the widget.
- `Rhino.UI.GripUserInterfaceObject.ObjectSnapPermitted : bool` / `ObjectSnapCursorsEnabled : bool` / `OnObjectCursorsEnabled : bool` — snapping and cursor behavior.
- `Rhino.UI.GripUserInterfaceObject.Constrain(Curve curve) : void` / `Constrain(Line line) : void` / `Constrain(Arc arc) : void` / `Constrain(Circle circle) : void` — motion constraint onto a curve, line, arc, or circle.
- `Rhino.UI.GripUserInterfaceObject.SetSnapPoints(IEnumerable<Point3d> points) : void` / `ClearSnapPoints() : void` — discrete snap targets on the base widget.
- `Rhino.UI.DirectionGripUserInterfaceObject.DirectionGripUserInterfaceObject(Point3d location, Vector3d direction)` — axis-constrained variant minted at a location and direction; `GripDirection : Vector3d` / `DirectionLineLength : float` / `ArrowShape : GripUserInterfaceObjectShape` / `ArrowRadius : float` / `OneWay : bool` / `GripPointVisible : bool` shape the arrow glyph, and `ArrowsVisibleInViewport(RhinoViewport viewport) : bool` reads per-viewport arrow visibility.
- `Rhino.UI.RotationGripUserInterfaceObject.RotationGripUserInterfaceObject(Plane plane, double radius)` — rotation variant minted on a plane and radius; `GripPointVisible : bool` toggles the center grip, and `ArcVisibleInViewport(RhinoViewport viewport) : bool` reads per-viewport arc visibility.
- `Rhino.UI.GripUserInterfaceObject.GripUserInterfaceObject()` (protected) / `OnDrag(Point3d newLocation, MouseState mouse) : void` (protected virtual) — the widgets are derivation surfaces too: the drag channel is a protected virtual, and `Rhino.UI.RotationGripUserInterfaceObject.OnRotationDrag(double angle, MouseState mouse) : void` (protected virtual) defaults to rotating the plane about its Z axis.
- `Rhino.UI.TextDotUserInterfaceObject.TextDotUserInterfaceObject(Point3d location, string text)` — text-dot widget; `Text : string` / `TextColor : Color` / `TextHeight : int` / `MouseOverTextHeight : int` / `DotBackgroundColor : Color` / `DotBorderColor : Color` (all get/set).

[WIDGET_BASE_AND_REGISTRATION]:
- `Rhino.UI.UserInterfaceObjectBase` (abstract) — `Visible : bool` (get/set; invisible widgets neither draw nor receive mouse events) / `BoundToActiveView : bool` (get/set) / `RegisterForAllDocuments() : bool` / `Unregister() : void` / `IsRegistered() : bool` — visibility and the all-documents registration path.
- `Rhino.UI.UserInterfaceObjectBase` protected virtuals — `OnDraw(DrawEventArgs) : void`, `IsMouseOver(MouseState) : bool`, `OnMouseDown` / `OnMouseUp` / `OnMouseMove` / `OnMouseEnter` / `OnMouseLeave` / `OnMouseClick` / `OnMouseDoubleClick(MouseState) : void` — the widget mouse and draw hook roster; the drag callback fires only on `GripUserInterfaceObject`.
- `Rhino.DocObjects.Tables.ViewUserInterfaceTable` (`RhinoDoc.ViewUserInterface`) — `Add(UserInterfaceObjectBase) : bool` / `Add(UserInterfaceObjectBase, Guid userInterfaceGroupId) : bool` / `Remove(UserInterfaceObjectBase) : int` / `Remove(IEnumerable<UserInterfaceObjectBase>) : int` / `RemoveByGroupId(Guid) : int` / `Find<T>() : T[] where T : UserInterfaceObjectBase` / `Document : RhinoDoc` — the per-document widget registration table.

## [04]-[IMPLEMENTATION_LAW]

[AUTHORING_TOPOLOGY]:
- Rhino owns construction: it instantiates a `ClassIdAttribute`-stamped subclass and invokes its protected virtuals, so each custom kind is one thin sealed adapter forwarding every host callback to a value-typed override program of `Func<..., Fin<...>>` hooks over union-typed draw-mark and transform-policy payloads.
- grips register the same way: `RegisterGripsEnabler` binds an enabler keyed on the grips type's own `[Guid]`, `NewGeometry` fires once at drag end returning geometry as a pure function of grip positions, and the instance `GripsMoved`/`NewLocation` flags and the static `Dragging()` probe gate temp-display rebuilds.
- `GripObject` needs no subclassing — its move, weight, and parameter reads are value-shaped edits of an existing grip resolved from `RhinoObject.GetGrips` in the objects catalog.
- grip widgets are constructed carriers AND derivation surfaces: visual state, constraints, and snap targets are plain values, while the drag channel (`OnDrag`/`OnRotationDrag`) and the base mouse roster are protected virtuals a thin shim forwards to a value program; registration rides the document `ViewUserInterfaceTable`.

[STACKING]:
- `LanguageExt.Core`(`libs/csharp/.api/api-languageext.md`): each override hook returns `Fin<Unit>` or `Fin<A>` so a custom-object program is a total record of result-returning delegates; grip topology reads fold to `Option<GripObject>` and `Seq<A>`, and the disposable `CustomObjectGrips` rides a leased using-scope.
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the grip-widget shape vocabulary wraps as a keyed `SmartEnum`; draw marks, transform policies, and the four custom-object geometry kinds collapse into generated unions owning total dispatch inside the override program.
- `Rasm` kernel: grip frames, CV parameters, widget constraint geometry, and rebuilt custom geometry compose the kernel numeric owners; the override program is an immutable value unit-tested off the host before any registration.

[LOCAL_ADMISSION]:
- subclassing enters only at the registration boundary: one thin adapter per custom kind stamped with `ClassIdAttribute`, one enabler per custom-grips type through `RegisterGripsEnabler`; no derivation owns state or a dispatch table.
- an immutable override program is admitted as the value the adapter forwards to; a custom object crosses back to the objects catalog for read/mutate and to the document catalog for placement, and pick output crosses to the commands catalog for projection.
- grip editing and grip widgets are admitted as value owners; a live `GripObject` resolves from the owning object inside its document grant and never leases outward.

[RAIL_LAW]:
- Surface: `Rhino.DocObjects.Custom` authoring, `Rhino.DocObjects.GripObject` editing, and `Rhino.UI` grip widgets
- Owns: custom-object registration and the virtual override program, grip-set authoring and rebuild, the grip edit surface, and on-canvas grip widgets.
- Accept: registration shims over immutable override programs, value-shaped grip edits, and constrained grip widgets projected onto `Fin`/`Option` rails.
- Reject: a second host-derived base owning state or dispatch, domain logic inside a subclass, and live custom objects or grips crossing the session boundary.
