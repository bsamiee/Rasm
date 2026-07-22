# [RASM_RHINO_API_RHINOCOMMON_GEOMETRY]

This catalog owns the host-bound `Rhino.Geometry` crossing surface the substrate value algebra does not carry: the `GeometryBase` identity and duplication seam (shallow-duplicate/document-control identity, user strings, `DataCRC` hashing), `ClippingPlaneSurface` clip participation, the `Mesh` texture-coordinate cache, the `HiddenLineDrawing`/`Silhouette` Make2D projection surface, the `Rhino.Geometry.Intersect` roster recorded as host parity, the `Rhino.Collections` point and curve buffers, and the `Light` geometry type.

Substrate `api-rhinocommon` owns the `Transform` factory/decomposition/inverse algebra, the `BoundingBox` member set, the `GeometryBase.GetBoundingBox` bounds triple, `GeometryBase.Transform(Transform)`, and the `Point3d`/`Vector3d`/`Plane`/`Line`/`Box` carriers, which this crossing registers. Kernel-grade geometry algorithms live in `Rasm` and compose the host surface; the hidden-line and intersection families catalog the host offering a design page reads for parity, and the kernel reservation binds those pages.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon geometry-crossing surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Geometry`, `Rhino.Geometry.Intersect`, `Rhino.Collections`
- kernel: `Rasm` (host-agnostic geometry and numeric owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: geometry-crossing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: crossing owners

`Transform`, `BoundingBox`, `Point3d`/`Vector3d`/`Plane`/`Line`/`Box`, and `TransformSimilarityType` register from the substrate `api-rhinocommon`.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `GeometryBase`         | class         | identity, duplication, user-strings, `DataCRC`, and the type discriminant |
|  [02]   | `ClippingPlaneSurface` | class         | clip-plane depth and per-object/per-layer clip participation              |
|  [03]   | `ObjectType`           | enum          | geometry-type discriminant returned by `GeometryBase.ObjectType`          |
|  [04]   | `Mesh`                 | class         | texture-coordinate cache lifecycle                                        |

[PUBLIC_TYPE_SCOPE]: Make2D hidden-line drawing family (`Rhino.Geometry`)

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :----------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `HiddenLineDrawing`            | class         | projection compute, segment/point enumeration, projection frame   |
|  [02]   | `HiddenLineDrawingParameters`  | class         | viewport, geometry/tag/clip add, tolerance/tangent/hidden flags   |
|  [03]   | `HiddenLineDrawingObject`      | class         | one added object: geometry, transform, tag, occluding-section     |
|  [04]   | `HiddenLineDrawingObjectCurve` | class         | silhouette type and per-side segment split of one source curve    |
|  [05]   | `HiddenLineDrawingSegment`     | class         | one 2D segment: visibility class and per-side surface fills       |
|  [06]   | `HiddenLineDrawingPoint`       | class         | one 2D point: location, visibility, and source classification     |
|  [07]   | `Silhouette`                   | class         | standalone silhouette-curve extraction: type, component, 3D curve |
|  [08]   | `SilhouetteType`               | enum          | `[Flags]` silhouette origin classification shared with the curves |

[PUBLIC_TYPE_SCOPE]: intersection roster and event carriers (`Rhino.Geometry.Intersect`)

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :---------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `Intersection`          | class         | curve/surface/brep/mesh/plane/ray/projection intersection statics |
|  [02]   | `CurveIntersections`    | class         | indexed disposable list of curve and surface `IntersectionEvent`s |
|  [03]   | `IntersectionEvent`     | class         | one point-or-overlap event: points, parameters, overlap intervals |
|  [04]   | `MeshCurveIntersection` | class         | one mesh-curve event: incidence, face, barycentric, tolerances    |
|  [05]   | `RayShootEvent`         | struct        | one ray-reflection hit: geometry index, brep face, point          |
|  [06]   | `MeshClash`             | class         | mesh-pair clash point/radius with `Search`/`FindDetail` statics   |
|  [07]   | `MeshInterference`      | struct        | object-set interference: index pair and hit points                |
|  [08]   | `MeshIntersectionCache` | class         | disposable cache for repeated `MeshPlane` sections                |

[PUBLIC_TYPE_SCOPE]: point and curve buffers (`Rhino.Collections`)

`ArchivableDictionary` registers from the persistence catalog `api-rhinocommon-persistence`.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :-------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `RhinoList<T>`        | class         | `IList<T>`-shaped mutable buffer base the typed lists extend    |
|  [02]   | `Point3dList`         | class         | per-axis accessors, bounds, closest-index, in-place transform   |
|  [03]   | `CurveList`           | class         | curve buffer with primitive-to-curve add and insert convenience |
|  [04]   | `TransformObjectList` | class         | object and grip staging list with display-feedback transform    |
|  [05]   | `RhinoList`           | class         | point-cloud and point k-nearest-neighbour statics               |

[PUBLIC_TYPE_SCOPE]: light geometry (`Rhino.Geometry`)

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :------------------ | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `Light`             | class         | point/spot/directional/linear/rectangular/sun params, color, intensity |
|  [02]   | `LightStyle`        | enum          | camera-vs-world light-kind discriminant                                |
|  [03]   | `Light.Attenuation` | enum          | attenuation fall-off model                                             |

[ENUM_ROSTERS]:
- `HiddenLineDrawingSegment.Visibility`: `Unset`, `Visible`, `Hidden`, `Duplicate`, `Projecting`, `Clipped`.
- `HiddenLineDrawingSegment.SideFill`: `SideUnset`, `SideSurface`, `SideVoid`, `OtherSurface`.
- `HiddenLineDrawingPoint.Visibility`: `Unset`, `Visible`, `Hidden`, `Duplicate`.
- `SilhouetteType` (`[Flags]`): `None=0`, `Projecting=1`, `TangentProjects=2`, `Tangent=4`, `Crease=8`, `Boundary=0x10`, `NonSilhouetteCrease=0x100`, `NonSilhouetteTangent=0x200`, `NonSilhouetteSeam=0x400`, `SectionCut=0x1000`, `MiscellaneousFeature=0x2000`, `DraftCurve=0x8000`.
- `Intersection` outcomes: `PlaneCircleIntersection` (`None`/`Tangent`/`Secant`/`Parallel`/`Coincident`), `PlaneSphereIntersection` (`None`/`Point`/`Circle`), `LineCircleIntersection` and `LineSphereIntersection` (`None`/`Single`/`Multiple`), `LineCylinderIntersection` (`None`/`Single`/`Multiple`/`Overlap`), `SphereSphereIntersection` (`None`/`Point`/`Circle`/`Overlap`), `ArcArcIntersection` and `CircleCircleIntersection` (`None`/`Single`/`Multiple`/`Overlap`), `MeshIncidence` (`Face`/`Edge`/`Vertex`).
- `LightStyle`: `None=0`, `CameraDirectional=4`, `CameraPoint=5`, `CameraSpot=6`, `WorldDirectional=7`, `WorldPoint=8`, `WorldSpot=9`, `Ambient=10`, `WorldLinear=11`, `WorldRectangular=12`.
- `Light.Attenuation`: `Constant`, `Linear`, `InverseSquared`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: geometry identity, mutation, and user strings

`GeometryBase.Transform(Transform)` and the `GetBoundingBox(bool)`/`GetBoundingBox(Transform)`/`GetBoundingBox(Plane, out Box)` bounds triple register from the substrate `api-rhinocommon`.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `GeometryBase.DuplicateShallow() -> GeometryBase`        | instance | shares the native pointer, no deep copy  |
|  [02]   | `GeometryBase.IsShallowDuplicate`                        | property | shares another instance's native pointer |
|  [03]   | `GeometryBase.Duplicate() -> GeometryBase`               | instance | deep copy off document control           |
|  [04]   | `GeometryBase.IsDocumentControlled`                      | property | document owns the geometry lifetime      |
|  [05]   | `GeometryBase.ObjectType`                                | property | geometry-type discriminant               |
|  [06]   | `GeometryBase.DataCRC(uint) -> uint`                     | instance | content hash for change and identity     |
|  [07]   | `GeometryBase.Translate(Vector3d) -> bool`               | instance | in-place translation                     |
|  [08]   | `GeometryBase.Scale(double) -> bool`                     | instance | in-place uniform scale                   |
|  [09]   | `GeometryBase.Rotate(double, Vector3d, Point3d) -> bool` | instance | in-place rotation about an axis          |
|  [10]   | `GeometryBase.SetUserString(string, string) -> bool`     | instance | write a key-value user string            |
|  [11]   | `GeometryBase.GetUserString(string) -> string`           | instance | read a user string by key                |
|  [12]   | `GeometryBase.GetUserStrings() -> NameValueCollection`   | instance | full user-string collection              |
|  [13]   | `GeometryBase.DeleteUserString(string) -> bool`          | instance | remove a user string by key              |

[ENTRYPOINT_SCOPE]: mesh texture-coordinate cache

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `Mesh.HasCachedTextureCoordinates`                                    | property | any mapping-derived set is cached     |
|  [02]   | `Mesh.SetCachedTextureCoordinatesFromMaterial(RhinoObject, Material)` | instance | prime every material-channel set      |
|  [03]   | `Mesh.GetCachedTextureCoordinates(Guid) -> CachedTextureCoordinates`  | instance | read-only set by mapping identity     |
|  [04]   | `Mesh.InvalidateCachedTextureCoordinates(bool)`                       | instance | clear all or surface-parameter subset |

- `Mesh.GetCachedTextureCoordinates(RhinoObject, Texture)`: resolves the set by texture channel; the returned wrapper is nullable.

[ENTRYPOINT_SCOPE]: clip participation

Members dot off `ClippingPlaneSurface`.

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `PlaneDepth`                                                                  | property | finite clip depth below the plane     |
|  [02]   | `PlaneDepthEnabled`                                                           | property | finite clip depth active              |
|  [03]   | `ParticipationListsEnabled`                                                   | property | participation lists active            |
|  [04]   | `DimensionStyleId`                                                            | property | label dimension style; `Empty` clears |
|  [05]   | `AddClipViewportId(Guid) -> bool`                                             | instance | add a clipped viewport                |
|  [06]   | `RemoveClipViewportId(Guid) -> bool`                                          | instance | remove a clipped viewport             |
|  [07]   | `SetClipParticipation(IEnumerable<Guid>, IEnumerable<int>, bool)`             | instance | set object/layer participation lists  |
|  [08]   | `GetClipParticipation(out IEnumerable<Guid>, out IEnumerable<int>, out bool)` | instance | read the participation lists          |
|  [09]   | `ClearClipParticipationLists()`                                               | instance | clear the participation lists         |

[ENTRYPOINT_SCOPE]: Make2D projection compute

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :---------------------------------- |
|  [01]   | `HiddenLineDrawing.Compute(HiddenLineDrawingParameters, bool) -> HiddenLineDrawing`   | static   | compute from a filled parameter set |
|  [02]   | `HiddenLineDrawingParameters.SetViewport(ViewportInfo)`                               | instance | set the projection frame            |
|  [03]   | `HiddenLineDrawingParameters.AddClippingPlane(Plane)`                                 | instance | add a clipping plane                |
|  [04]   | `HiddenLineDrawingParameters.AddGeometry(GeometryBase, object)`                       | instance | add tagged source geometry          |
|  [05]   | `HiddenLineDrawingParameters.AddGeometryAndPlanes(GeometryBase, object, List<Plane>)` | instance | add geometry with per-object clips  |

- `HiddenLineDrawing.Compute(HiddenLineDrawingParameters, bool, IProgress<double>, CancellationToken)`: cancellable, progress-reporting compute.
- `HiddenLineDrawingParameters.SetViewport(RhinoViewport)`: sets the projection frame from a viewport.
- `HiddenLineDrawingParameters.AddGeometry`: overloads add `bool occludingSections`, `Transform`, and both; `AddGeometryAndPlanes` overloads add `Transform` and `bool occludingSections`.
- `[HiddenLineDrawingParameters flags]`: `AbsoluteTolerance` (default `0.01`) `Flatten` `IncludeTangentEdges` `IncludeTangentSeams` `IncludeHiddenCurves` `OccludingSectionOption`.

[ENTRYPOINT_SCOPE]: Make2D results and source objects

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :-------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `HiddenLineDrawing.Segments -> IEnumerable<HiddenLineDrawingSegment>` | property | computed 2D segments                   |
|  [02]   | `HiddenLineDrawing.Points -> IEnumerable<HiddenLineDrawingPoint>`     | property | computed 2D points                     |
|  [03]   | `HiddenLineDrawing.Viewport -> ViewportInfo`                          | property | projection viewport                    |
|  [04]   | `HiddenLineDrawing.WorldToHiddenLine -> Transform`                    | property | world-to-drawing projection frame      |
|  [05]   | `HiddenLineDrawing.BoundingBox(bool) -> BoundingBox`                  | instance | drawing bounds, optionally with hidden |
|  [06]   | `HiddenLineDrawing.RejoinCompatibleVisible()`                         | instance | merge compatible visible segments      |

- `[HiddenLineDrawingSegment]`: `CurveGeometry` `SegmentVisibility` `CurveSideFills` `ParentCurve` `IsSceneSilhouette`.
- `[HiddenLineDrawingObjectCurve]`: `SilhouetteType` `SourceObject` `SourceObjectComponentIndex` `Segments` `Curve(double)`.
- `[HiddenLineDrawingPoint]`: `Location` `PointVisibility` `SourceObject` `SourceObjectComponentIndex`.
- `[HiddenLineDrawingObject]`: `Geometry` `Transform` `Tag` `OccludingSections`.

[ENTRYPOINT_SCOPE]: silhouette compute

Members dot off `Silhouette` and return `Silhouette[]`.

| [INDEX] | [SURFACE]                                                             | [SHAPE] | [CAPABILITY]                   |
| :-----: | :-------------------------------------------------------------------- | :------ | :----------------------------- |
|  [01]   | `Compute(GeometryBase, SilhouetteType, Point3d, double, double)`      | static  | perspective-camera silhouettes |
|  [02]   | `Compute(GeometryBase, SilhouetteType, Vector3d, double, double)`     | static  | parallel-camera silhouettes    |
|  [03]   | `Compute(GeometryBase, SilhouetteType, ViewportInfo, double, double)` | static  | viewport-frame silhouettes     |
|  [04]   | `ComputeDraftCurve(GeometryBase, double, Vector3d, double, double)`   | static  | constant-draft-angle curves    |

- `Silhouette.Compute`: each camera overload adds `IEnumerable<Plane> clippingPlanes, CancellationToken`; `ComputeDraftCurve` adds `CancellationToken`.
- `[Silhouette]`: `SilhouetteType` `GeometryComponentIndex` `Curve` — the 3D `Curve` is null on a projecting region.

[ENTRYPOINT_SCOPE]: primitive intersection statics

`Intersection` statics are recorded as host parity; a design page reserves the composed `Rasm` kernel rather than calling the host static. Members dot off `Intersection`.

| [INDEX] | [SURFACE]                                                                                              | [SHAPE] | [CAPABILITY]       |
| :-----: | :----------------------------------------------------------------------------------------------------- | :------ | :----------------- |
|  [01]   | `LineLine(Line, Line, out double, out double) -> bool`                                                 | static  | infinite-line      |
|  [02]   | `LinePlane(Line, Plane, out double) -> bool`                                                           | static  | line-plane         |
|  [03]   | `PlanePlane(Plane, Plane, out Line) -> bool`                                                           | static  | plane-pair line    |
|  [04]   | `PlanePlanePlane(Plane, Plane, Plane, out Point3d) -> bool`                                            | static  | plane-triple point |
|  [05]   | `LineBox(Line, BoundingBox, double, out Interval) -> bool`                                             | static  | line-box interval  |
|  [06]   | `PlaneBoundingBox(Plane, BoundingBox, out Polyline) -> bool`                                           | static  | plane-box polyline |
|  [07]   | `PlaneCircle(Plane, Circle, out double, out double) -> PlaneCircleIntersection`                        | static  | plane-circle       |
|  [08]   | `PlaneSphere(Plane, Sphere, out Circle) -> PlaneSphereIntersection`                                    | static  | plane-sphere       |
|  [09]   | `LineCircle(Line, Circle, out double, out Point3d, out double, out Point3d) -> LineCircleIntersection` | static  | line-circle        |
|  [10]   | `LineSphere(Line, Sphere, out Point3d, out Point3d) -> LineSphereIntersection`                         | static  | line-sphere        |
|  [11]   | `LineCylinder(Line, Cylinder, out Point3d, out Point3d) -> LineCylinderIntersection`                   | static  | line-cylinder      |
|  [12]   | `SphereSphere(Sphere, Sphere, out Circle) -> SphereSphereIntersection`                                 | static  | sphere-sphere      |
|  [13]   | `ArcArc(Arc, Arc, out Point3d, out Point3d) -> ArcArcIntersection`                                     | static  | arc-arc            |
|  [14]   | `CircleCircle(Circle, Circle, out Point3d, out Point3d) -> CircleCircleIntersection`                   | static  | circle-circle      |

- `LineLine`: a `(…, double tolerance, bool finiteSegments)` overload does segment intersection; `LineBox` takes an oriented `Box` overload.

[ENTRYPOINT_SCOPE]: curve intersection statics

| [INDEX] | [SURFACE]                                                                    | [SHAPE] | [CAPABILITY]             |
| :-----: | :--------------------------------------------------------------------------- | :------ | :----------------------- |
|  [01]   | `CurveCurve(Curve, Curve, double, double) -> CurveIntersections`             | static  | curve-curve events       |
|  [02]   | `CurveSelf(Curve, double) -> CurveIntersections`                             | static  | self-intersection events |
|  [03]   | `CurvePlane(Curve, Plane, double) -> CurveIntersections`                     | static  | curve-plane events       |
|  [04]   | `CurveLine(Curve, Line, double, double) -> CurveIntersections`               | static  | curve-line events        |
|  [05]   | `CurveSurface(Curve, Surface, double, double) -> CurveIntersections`         | static  | curve-surface events     |
|  [06]   | `CurveBrep(Curve, Brep, double, out Curve[], out Point3d[]) -> bool`         | static  | curve-brep               |
|  [07]   | `CurveBrepFace(Curve, BrepFace, double, out Curve[], out Point3d[]) -> bool` | static  | curve against one face   |

- `CurveCurve`: `CurveCurveValidate(…, out int[], out TextLog)` reports invalid spans; `CurveSurface` adds an `Interval curveDomain` overload and two `CurveSurfaceValidate` forms.
- `CurveBrep`: overloads add `out double[] curveParameters` and the angle-tolerance form `(…, double angleTolerance, out double[])`.

[ENTRYPOINT_SCOPE]: surface and brep intersection statics

| [INDEX] | [SURFACE]                                                                                        | [SHAPE] | [CAPABILITY]             |
| :-----: | :----------------------------------------------------------------------------------------------- | :------ | :----------------------- |
|  [01]   | `SurfaceSurface(Surface, Surface, double, out Curve[], out Point3d[]) -> bool`                   | static  | surface-surface          |
|  [02]   | `BrepPlane(Brep, Plane, double, out Curve[], out Point3d[]) -> bool`                             | static  | brep-plane               |
|  [03]   | `GeometryPlane(GeometryBase, Plane, double, bool, RhinoDoc, out Curve[], out Point3d[]) -> bool` | static  | generic geometry section |
|  [04]   | `BrepBrep(Brep, Brep, double, out Curve[], out Point3d[]) -> bool`                               | static  | brep-brep                |
|  [05]   | `BrepSurface(Brep, Surface, double, out Curve[], out Point3d[]) -> bool`                         | static  | brep-surface             |

- `BrepPlane`/`BrepBrep`/`BrepSurface`: a `bool joinCurves` overload joins the section.

[ENTRYPOINT_SCOPE]: mesh intersection statics

| [INDEX] | [SURFACE]                                                                  | [SHAPE] | [CAPABILITY]                |
| :-----: | :------------------------------------------------------------------------- | :------ | :-------------------------- |
|  [01]   | `MeshPlane(Mesh, Plane) -> Polyline[]`                                     | static  | mesh section polylines      |
|  [02]   | `MeshMeshFast(Mesh, Mesh) -> Line[]`                                       | static  | fast unwelded section lines |
|  [03]   | `MeshMeshAccurate(Mesh, Mesh, double) -> Polyline[]`                       | static  | accurate section polylines  |
|  [04]   | `MeshMeshPredicate(IEnumerable<Mesh>, double, out int[], TextLog) -> bool` | static  | mesh-pair predicate         |
|  [05]   | `MeshRay(Mesh, Ray3d) -> double`                                           | static  | ray parameter               |
|  [06]   | `MeshLine(Mesh, Line) -> Point3d[]`                                        | static  | mesh-line hit points        |
|  [07]   | `MeshPolyline(Mesh, PolylineCurve, out int[]) -> Point3d[]`                | static  | mesh-polyline hit points    |
|  [08]   | `MeshCurve(Mesh, Curve, double) -> MeshCurveIntersection[]`                | static  | mesh-curve events           |

- `MeshPlane`: overloads section against many planes and take a `MeshIntersectionCache`, `double tolerance`, and optional `bool overlaps`.
- `MeshMesh(IEnumerable<Mesh>, double, out Polyline[], bool, out Polyline[], bool, out Mesh, TextLog, CancellationToken, IProgress<double>) -> bool`: full multi-mesh intersection with overlap outputs.
- `MeshLineSorted`/`MeshPolylineSorted`: order the hit points along the line.
- `MeshCurve`: a `bool includeOverlaps` overload and two point-returning overloads emit `out Curve[] overlapCurves` and face-id arrays.

[ENTRYPOINT_SCOPE]: ray and projection intersection statics

| [INDEX] | [SURFACE]                                                                                       | [SHAPE] | [CAPABILITY]                |
| :-----: | :---------------------------------------------------------------------------------------------- | :------ | :-------------------------- |
|  [01]   | `RayShoot(Ray3d, IEnumerable<GeometryBase>, int) -> Point3d[]`                                  | static  | reflection points           |
|  [02]   | `ProjectPointsToMeshes(IEnumerable<Mesh>, IEnumerable<Point3d>, Vector3d, double) -> Point3d[]` | static  | directional mesh projection |
|  [03]   | `ProjectPointsToBreps(IEnumerable<Brep>, IEnumerable<Point3d>, Vector3d, double) -> Point3d[]`  | static  | directional brep projection |

- `RayShoot(IEnumerable<GeometryBase>, Ray3d, int) -> RayShootEvent[]`: per-hit event overload.
- `ProjectPointsToMeshes`/`ProjectPointsToBreps`: the `…Ex(…, out int[] indices)` form reports source indices.

[ENTRYPOINT_SCOPE]: intersection event carriers
- `[CurveIntersections]`: `Count` `this[int]` `CopyTo(IntersectionEvent[], int)` `Dispose()` — indexed disposable event list.
- `[IntersectionEvent]`: `IsPoint` `IsOverlap` `PointA` `PointA2` `PointB` `PointB2` `ParameterA` `ParameterB` `OverlapA` `OverlapB`; `SurfacePointParameter(out double, out double)` and `SurfaceOverlapParameter(out Interval, out Interval)` read surface parameters.
- `[MeshCurveIntersection]`: `Incidence` `FaceIndex` `CurveParameter` `PointA` `PointB` `BarycentricA` `BarycentricB` `IsPoint` `IsOverlap`.
- `[MeshClash]`: `MeshA` `MeshB` `ClashPoint` `ClashRadius`; static `Search(IEnumerable<Mesh>, IEnumerable<Mesh>, double, int) -> MeshClash[]` and `FindDetail(RhinoObject, RhinoObject, double) -> Mesh[]`.
- `[RayShootEvent]`: `GeometryIndex` `BrepFaceIndex` `Point`. `[MeshInterference]`: `IndexA` `IndexB` `HitPoints`.

[ENTRYPOINT_SCOPE]: collection buffers

| [INDEX] | [SURFACE]                                                                                  | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :----------------------------------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `Point3dList(int)`                                                                         | ctor     | capacity-seeded point buffer    |
|  [02]   | `Point3dList.ClosestIndex(Point3d) -> int`                                                 | instance | nearest-point index query       |
|  [03]   | `Point3dList.Add(double, double, double)`                                                  | instance | coordinate add                  |
|  [04]   | `Point3dList.Transform(Transform)`                                                         | instance | in-place buffer transform       |
|  [05]   | `CurveList.Add(Line)`                                                                      | instance | primitive-to-curve add          |
|  [06]   | `TransformObjectList.AddObjects(GetObject, bool) -> int`                                   | instance | stage command objects and grips |
|  [07]   | `RhinoList.Point3dKNeighbors(PointCloud, IEnumerable<Point3d>, int) -> IEnumerable<int[]>` | static   | point k-nearest-neighbours      |

- `Point3dList`: `Point3dList(IEnumerable<Point3d>)` and `Point3dList(params Point3d[])` ctors; `BoundingBox`, per-axis `X`/`Y`/`Z` accessors, `SetAllX`/`SetAllY`/`SetAllZ`, and static `ClosestIndexInList`/`ClosestPointInList`.
- `CurveList.Add`: `Circle`/`Arc`/`Ellipse`/`IEnumerable<Point3d>` overloads; `Insert(int, …)` mirrors each and `Transform(Transform) -> bool` transforms every curve.
- `TransformObjectList`: `Add(RhinoObject)`/`Add(ObjRef)`, `ObjectArray() -> RhinoObject[]`, `GripArray() -> GripObject[]`, `UpdateDisplayFeedbackTransform(Transform) -> bool`.
- `RhinoList`: `PointCloudKNeighbors`/`Point3fKNeighbors`/`Point2dKNeighbors`/`Point2fKNeighbors` share the KNN shape; the generic `RhinoList<T>` is the `IList<T>` base the typed lists extend.

[ENTRYPOINT_SCOPE]: light geometry

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `Light()`                                                         | ctor     | empty light                      |
|  [02]   | `Light.LightStyle`                                                | property | kind discriminant                |
|  [03]   | `Light.SetAttenuation(double, double, double)`                    | instance | set the fall-off model           |
|  [04]   | `Light.GetSpotLightRadii(out double, out double) -> bool`         | instance | spot inner/outer radii           |
|  [05]   | `Light.CreateSunLight(double, double, double) -> Light`           | static   | sun light from angles            |
|  [06]   | `Light.CreateSunLight(double, DateTime, double, double) -> Light` | static   | sun light from time and location |
|  [07]   | `Light.CreateSunLight(Sun) -> Light`                              | static   | sun light from a `Sun`           |

- `[Light kind]`: `IsEnabled` `IsPointLight` `IsDirectionalLight` `IsSpotLight` `IsLinearLight` `IsRectangularLight` `IsSunLight` `CoordinateSystem`.
- `[Light pose]`: `Location` `Direction` `PerpendicularDirection` `Length` `Width` — linear and rectangular lights read `Length`/`Width`.
- `[Light power]`: `Intensity` `PowerWatts` `PowerLumens` `PowerCandela`; `[Light shade]`: `Ambient` `Diffuse` `Specular` `ShadowIntensity`.
- `[Light attenuation]`: `AttenuationType` `AttenuationVector` `GetAttenuation(double) -> double`, with static `ConstantAttenuationVector`/`LinearAttenuationVector`/`InverseSquaredAttenuationVector` presets.
- `[Light spot]`: `SpotAngleRadians` `SpotExponent` `HotSpot`. `[Light identity]`: `Name` `Id`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `GeometryBase` owns the native crossing: `DuplicateShallow` shares the native pointer (`IsShallowDuplicate` reports it), `Duplicate` lifts geometry off `IsDocumentControlled` document ownership, and `DataCRC` is the content-identity hash. A boundary that mutates or retains document-controlled geometry duplicates first, keys it by `DataCRC`, and carries its user strings. In-place mutation and bounds derivation cross the substrate `Transform`/`BoundingBox` value algebra at the wire.
- Clip participation is a triple on `ClippingPlaneSurface` — the clipped-viewport set, the object/layer participation lists (inclusion or exclusion), and the finite `PlaneDepth` — each gated by its `*Enabled` flag and mutated only through the participation members.
- Mesh texture coordinates form one cache lifecycle: material and object mappings prime the sets, mapping identity selects a read-only `CachedTextureCoordinates` wrapper, `HasCachedTextureCoordinates` reports presence, and scoped invalidation clears all rows or only the surface-parameter subset.
- Make2D is a compute pipeline, not live geometry: `HiddenLineDrawingParameters` accumulates the viewport frame, tagged source geometry, clip planes, and tangent/hidden flags, and `HiddenLineDrawing.Compute` projects them once into an immutable `Segments`/`Points` result keyed by the `WorldToHiddenLine` frame; a segment's `Visibility` and per-side `SideFill` classify it, and the source object and component index trace each result to its input.
- `Intersection` and the hidden-line families are host geometry-algorithm surfaces recorded as parity: a design page reads them to know the host offering, and the `Rasm` kernel owns the composed intersection algorithm the page reserves.
- `Rhino.Collections` buffers are host list carriers: `Point3dList`/`CurveList` extend `RhinoList<T>` with typed add/insert and, on `Point3dList`, per-axis accessors, bounds, and closest-index; a boundary fills a buffer, hands the canonical `Rasm` collection onward, and never retains the host list, while `TransformObjectList` stages a transform command's objects and grips.
- `Light` is one geometry type discriminated by `LightStyle`: the point, spot, directional, linear, rectangular, ambient, and sun kinds share the pose, intensity/power, color, shadow, and attenuation members, and the boundary reads the kind through the `Is*Light` predicates and the `LightStyle` value rather than branching on construction.

[STACKING]:
- `RhinoCommon` value substrate(`api-rhinocommon`): the `Transform` factory/decomposition/inverse surface, the `BoundingBox` member set, `GeometryBase.GetBoundingBox` and `GeometryBase.Transform(Transform)`, and the `Point3d`/`Vector3d`/`Plane`/`Line`/`Box` carriers cross the wire from the substrate; this crossing re-derives none of it.
- `LanguageExt.Core`(`api-languageext`): a `bool` transform or user-string mutation projects to `Fin<Unit>`; a nullable `GetUserString` or coordinate wrapper lifts to `Option<string>`/`Option<CachedTextureCoordinates>`; `GetUserStrings` and the clip participation lists land as `HashMap<string, string>` and `Seq<Guid>`/`Seq<int>`; `HiddenLineDrawing.Compute` and every `Intersection` static land on `Fin<A>` with the parallel out-parameter arrays fanned into one result record (`Seq<Curve>`/`Seq<Point3d>`/`Seq<IntersectionEvent>`); a `CurveIntersections` or `MeshIntersectionCache` disposable is `use`-bracketed, and an empty or nullable host array crosses as `Option`/`Seq`.
- `Thinktecture.Runtime.Extensions`(`api-thinktecture-runtime-extensions`): `ObjectType`, the `Intersection` outcome enums, `LightStyle`, and `Light.Attenuation` wrap as `[SmartEnum<TKey>]`/`[Flags]`-backed owners; a `DataCRC` content key wraps as a `[ValueObject<T>]`; the clip participation state models as a `[Union]` over disabled, inclusion-list, and exclusion-list cases.
- `Rasm` kernel: composed intersection and hidden-line algorithms are the kernel's — the catalog records the host `Intersection`/`HiddenLineDrawing` surface as parity, and a design page reserves the kernel algorithm rather than calling the host static.

[LOCAL_ADMISSION]:
- Native geometry enters through the crossing owner: a document-controlled instance is `Duplicate`d before retention or mutation, keyed by its `DataCRC` content hash and carried user strings.
- Host geometry types hold only inside the crossing; downstream code holds canonical `Rasm` geometry keyed by the content hash, and the crossing is the only place native `GeometryBase` state is read or written.

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: native geometry identity and duplication, user-string carriage, `DataCRC` content hashing, mesh texture-coordinate cache lifecycle, clip participation, the `HiddenLineDrawing`/`Silhouette` Make2D projection surface, the `Rhino.Geometry.Intersect` roster recorded as host parity, the `Rhino.Collections` point/curve buffers, and the `Light` geometry type.
- Accept: document-control-aware duplication, content-keyed identity, coordinate-cache prime/read/invalidation, clip participation projected onto `Fin`/`Option` rails, Make2D projection compute and result classification, intersection results fanned onto `Fin`/`Seq` rails, host list buffers read into canonical collections, and a `Light` kind read through `LightStyle`/`Is*Light`.
- Reject: re-deriving the kernel geometry algorithms `Rasm` owns, retaining document-controlled geometry without duplication, leaking host geometry types past the boundary, calling the host `Intersection`/`HiddenLineDrawing` static where a design page reserves the kernel, retaining a host `RhinoList`/`Point3dList` past the boundary, and branching a light on construction rather than its `LightStyle`.
