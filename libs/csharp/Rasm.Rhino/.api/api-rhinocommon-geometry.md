# [RASM_RHINO_API_RHINOCOMMON_GEOMETRY]

This catalog owns the host-bound `Rhino.Geometry` boundary the document, display, command, and exchange surfaces cross: the `GeometryBase` identity and duplication seam (shallow-duplicate and document-control identity, user-string carriage, `DataCRC` content hashing, instance transform mutation, bounding-box derivation), the `Transform` factory/decomposition/inverse algebra, mesh texture-coordinate caches, `ClippingPlaneSurface` clip participation, the `Light` geometry type, and the `Rhino.Collections` point and curve buffers. Two host geometry-algorithm surfaces enter as host-parity information: the `HiddenLineDrawing` Make2D projection and the `Rhino.Geometry.Intersect` roster. Kernel-grade geometry algorithms live in `Rasm` and are composed, never re-derived; the hidden-line and intersection families are cataloged as the host surface a design page reads for parity, and the kernel reservation binds those pages, never this catalog. Value structs the boundary rides — `Point3d`/`Vector3d`/`Plane`/`Line`/`Box` — and the `LanguageExt` rails every host outcome projects onto complete the surface.

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
- rail: geometry-crossing

| [INDEX] | [SYMBOL]                  | [KIND]          | [CAPABILITY]                                                                |
| :-----: | :------------------------ | :-------------- | :-------------------------------------------------------------------------- |
|  [01]   | `GeometryBase`            | native base     | identity, duplication, transform mutation, bounds, user-strings, and CRC    |
|  [02]   | `Transform`               | value struct    | 4x4 transform factories, decomposition, and inverse                         |
|  [03]   | `BoundingBox`             | value struct    | axis-aligned bounds with corners, edges, center/diagonal, and inflation     |
|  [04]   | `ClippingPlaneSurface`    | native geometry | clip-plane depth and per-object/per-layer clip participation                |
|  [05]   | `ObjectType`              | flags enum      | the geometry-type discriminant returned by `GeometryBase.ObjectType`        |
|  [06]   | `TransformSimilarityType` | enum            | the orientation-preserving classification returned by `DecomposeSimilarity` |
|  [07]   | `Mesh`                    | native geometry | texture-coordinate cache lifecycle                                           |

[PUBLIC_TYPE_SCOPE]: value carriers the crossing rides
- rail: geometry-crossing

| [INDEX] | [SYMBOL]               | [KIND]       | [CAPABILITY]                                                  |
| :-----: | :--------------------- | :----------- | :------------------------------------------------------------ |
|  [01]   | `Point3d` / `Vector3d` | value struct | position and direction the transform and bounds members carry |
|  [02]   | `Plane`                | value struct | the frame factories and plane-bound bounding boxes reference  |
|  [03]   | `Line` / `Box`         | value struct | bounding-box edge and oriented-box outputs                    |

[PUBLIC_TYPE_SCOPE]: Make2D hidden-line drawing family
- namespace: `Rhino.Geometry`
- rail: geometry-crossing

`HiddenLineDrawing.Compute` projects added geometry to a 2D hidden-line drawing against a `ViewportInfo` frame; each source object becomes `HiddenLineDrawingObject`s whose curves split into visible/hidden `HiddenLineDrawingSegment`s and `HiddenLineDrawingPoint`s carrying visibility and source classification.

| [INDEX] | [SYMBOL]                        | [KIND]           | [CAPABILITY]                                                     |
| :-----: | :------------------------------ | :--------------- | :-------------------------------------------------------------- |
|  [01]   | `HiddenLineDrawing`             | compute engine   | projection compute, segment/point enumeration, projection frame |
|  [02]   | `HiddenLineDrawingParameters`   | input builder    | viewport, geometry/tag/clip add, tolerance/tangent/hidden flags |
|  [03]   | `HiddenLineDrawingObject`       | source object    | one added object: geometry, transform, tag, occluding-section   |
|  [04]   | `HiddenLineDrawingObjectCurve`  | source curve     | silhouette type and per-side segment split of one source curve  |
|  [05]   | `HiddenLineDrawingSegment`      | result segment   | one 2D segment: visibility class and per-side surface fills      |
|  [06]   | `HiddenLineDrawingPoint`        | result point     | one 2D point: location, visibility, and source classification   |
|  [07]   | `Silhouette`                    | compute roster   | standalone silhouette-curve extraction: type, component, 3D curve |
|  [08]   | `SilhouetteType`                | flags enum       | silhouette origin classification shared with the hidden-line curves |

[PUBLIC_TYPE_SCOPE]: intersection roster and event carriers
- namespace: `Rhino.Geometry.Intersect`
- rail: geometry-crossing

`Intersection` is the host static roster documented as parity information; `Rasm` owns the composed intersection kernel a design page reserves, and the catalog records the host surface a page reads, never a page's algorithm. `CurveIntersections`/`IntersectionEvent`/`MeshCurveIntersection` carry curve, surface, and mesh events; the outcome enums classify the primitive results.

| [INDEX] | [SYMBOL]                | [KIND]         | [CAPABILITY]                                                     |
| :-----: | :---------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `Intersection`          | static roster  | curve/surface/brep/mesh/plane/ray/projection intersection statics |
|  [02]   | `CurveIntersections`    | event carrier  | indexed disposable list of curve and surface `IntersectionEvent`s |
|  [03]   | `IntersectionEvent`     | event          | one point-or-overlap event: points, parameters, overlap intervals |
|  [04]   | `MeshCurveIntersection` | event          | one mesh-curve event: incidence, face, barycentric, tolerances  |
|  [05]   | `RayShootEvent`         | value struct   | one ray-reflection hit: geometry index, brep face, point        |
|  [06]   | `MeshClash`             | clash carrier  | mesh-pair clash point and radius with `Search`/`FindDetail` statics |
|  [07]   | `MeshInterference`      | value struct   | object-set interference: index pair and hit points              |
|  [08]   | `MeshIntersectionCache` | reuse cache    | disposable cache for repeated `MeshPlane` sections              |

[PUBLIC_TYPE_SCOPE]: point and curve buffers
- namespace: `Rhino.Collections`
- rail: geometry-crossing

`Point3dList` and `CurveList` are the host typed geometry buffers over the `RhinoList<T>` base; `TransformObjectList` stages objects and grips for a transform command. `ArchivableDictionary` (also `Rhino.Collections`) is the persistence catalog's, cross-referenced through `api-rhinocommon-persistence.md`, never re-rostered here.

| [INDEX] | [SYMBOL]              | [KIND]            | [CAPABILITY]                                                    |
| :-----: | :-------------------- | :---------------- | :------------------------------------------------------------- |
|  [01]   | `RhinoList<T>`        | generic list base | the `IList<T>`-shaped mutable buffer base the typed lists extend |
|  [02]   | `Point3dList`         | point buffer      | per-axis accessors, bounds, closest-index, in-place transform  |
|  [03]   | `CurveList`           | curve buffer      | curve buffer with primitive-to-curve add and insert convenience |
|  [04]   | `TransformObjectList` | transform staging | object and grip staging list with display-feedback transform   |
|  [05]   | `RhinoList`           | static KNN helper | point-cloud and point k-nearest-neighbour statics (arity-0)    |

[PUBLIC_TYPE_SCOPE]: light geometry
- namespace: `Rhino.Geometry`
- rail: geometry-crossing

`Light : GeometryBase` is the host light geometry — a single type discriminated by `LightStyle` over point, spot, directional, linear, rectangular, ambient, and sun kinds, carrying location/direction/length/width geometry, intensity/power, ambient/diffuse/specular color, shadow intensity, and the attenuation model.

| [INDEX] | [SYMBOL]             | [KIND]          | [CAPABILITY]                                                          |
| :-----: | :------------------- | :-------------- | :------------------------------------------------------------------- |
|  [01]   | `Light`              | native geometry | point/spot/directional/linear/rectangular/sun params, color, intensity |
|  [02]   | `LightStyle`         | enum            | camera-vs-world light-kind discriminant                              |
|  [03]   | `Light.Attenuation`  | nested enum     | attenuation fall-off model                                           |

[ENUM_ROSTERS]:
- `HiddenLineDrawingSegment.Visibility`: `Unset`, `Visible`, `Hidden`, `Duplicate`, `Projecting`, `Clipped`.
- `HiddenLineDrawingSegment.SideFill`: `SideUnset`, `SideSurface`, `SideVoid`, `OtherSurface`.
- `HiddenLineDrawingPoint.Visibility`: `Unset`, `Visible`, `Hidden`, `Duplicate`.
- `SilhouetteType` (`[Flags]`): `None=0`, `Projecting=1`, `TangentProjects=2`, `Tangent=4`, `Crease=8`, `Boundary=0x10`, `NonSilhouetteCrease=0x100`, `NonSilhouetteTangent=0x200`, `NonSilhouetteSeam=0x400`, `SectionCut=0x1000`, `MiscellaneousFeature=0x2000`, `DraftCurve=0x8000`.
- `Intersection` outcome enums: `PlaneCircleIntersection` (`None`/`Tangent`/`Secant`/`Parallel`/`Coincident`), `PlaneSphereIntersection` (`None`/`Point`/`Circle`), `LineCircleIntersection` and `LineSphereIntersection` (`None`/`Single`/`Multiple`), `LineCylinderIntersection` (`None`/`Single`/`Multiple`/`Overlap`), `SphereSphereIntersection` (`None`/`Point`/`Circle`/`Overlap`), `ArcArcIntersection` and `CircleCircleIntersection` (`None`/`Single`/`Multiple`/`Overlap`), `MeshIncidence` (`Face`/`Edge`/`Vertex`).
- `LightStyle`: `None=0`, `CameraDirectional=4`, `CameraPoint=5`, `CameraSpot=6`, `WorldDirectional=7`, `WorldPoint=8`, `WorldSpot=9`, `Ambient=10`, `WorldLinear=11`, `WorldRectangular=12`.
- `Light.Attenuation`: `Constant`, `Linear`, `InverseSquared`.

## [03]-[ENTRYPOINTS]

[GEOMETRY_IDENTITY]:
- `Rhino.Geometry.GeometryBase.DuplicateShallow() : GeometryBase` — shares the underlying native pointer without a deep copy.
- `Rhino.Geometry.GeometryBase.IsShallowDuplicate : bool` — whether this instance shares another's native pointer.
- `Rhino.Geometry.GeometryBase.Duplicate() : GeometryBase` — deep-copies the geometry off document control.
- `Rhino.Geometry.GeometryBase.IsDocumentControlled : bool` — whether the document owns the lifetime of this geometry.
- `Rhino.Geometry.GeometryBase.ObjectType : ObjectType` — names the geometry-type discriminant.
- `Rhino.Geometry.GeometryBase.DataCRC(uint currentRemainder) : uint` — content hash for change and identity detection.

[GEOMETRY_TRANSFORM]:
- `Rhino.Geometry.GeometryBase.Transform(Transform xform) : bool` — applies a transform in place.
- `Rhino.Geometry.GeometryBase.Translate(Vector3d translationVector) : bool` — translates in place.
- `Rhino.Geometry.GeometryBase.Scale(double scaleFactor) : bool` — uniformly scales in place.
- `Rhino.Geometry.GeometryBase.Rotate(double angleRadians, Vector3d rotationAxis, Point3d rotationCenter) : bool` — rotates in place about an axis.

[GEOMETRY_BOUNDS]:
- `Rhino.Geometry.GeometryBase.GetBoundingBox(bool accurate) : BoundingBox` — world-aligned bounds, tight or estimated.
- `Rhino.Geometry.GeometryBase.GetBoundingBox(Transform xform) : BoundingBox` — bounds of the geometry under a transform.
- `Rhino.Geometry.GeometryBase.GetBoundingBox(Plane plane, out Box worldBox) : BoundingBox` — plane-aligned bounds with the oriented world box.

[USER_STRINGS]:
- `Rhino.Geometry.GeometryBase.SetUserString(string key, string value) : bool` — writes a key-value user string onto the geometry.
- `Rhino.Geometry.GeometryBase.GetUserString(string key) : string` — reads a user string by key.
- `Rhino.Geometry.GeometryBase.GetUserStrings() : NameValueCollection` — answers the full user-string collection.
- `Rhino.Geometry.GeometryBase.DeleteUserString(string key) : bool` — removes a user string by key.

[TRANSFORM_FACTORY]:
- `Rhino.Geometry.Transform.Translation(Vector3d motion) : Transform` — translation transform.
- `Rhino.Geometry.Transform.Scale(Plane plane, double xScaleFactor, double yScaleFactor, double zScaleFactor) : Transform` — non-uniform scale about a plane.
- `Rhino.Geometry.Transform.PlaneToPlane(Plane plane0, Plane plane1) : Transform` — rigid mapping between two frames.
- `Rhino.Geometry.Transform.ChangeBasis(Plane plane0, Plane plane1) : Transform` — change-of-basis between two frames.
- `Rhino.Geometry.Transform.ProjectAlong(Plane plane, Vector3d direction) : Transform` — oblique projection onto a plane.

[TRANSFORM_DECOMPOSITION]:
- `Rhino.Geometry.Transform.DecomposeSimilarity(out Vector3d translation, out double dilation, out Transform rotation, double tolerance) : TransformSimilarityType` — splits a similarity into translation, dilation, and rotation.
- `Rhino.Geometry.Transform.DecomposeRigid(out Vector3d translation, out Transform rotation, double tolerance) : int` — splits a rigid transform into translation and rotation.
- `Rhino.Geometry.Transform.DecomposeAffine(out Vector3d translation, out Transform linear, out Transform rotation, out Vector3d diagonal) : bool` — splits an affine transform into its linear, rotation, and scaling factors.
- `Rhino.Geometry.Transform.TransformBoundingBox(BoundingBox bbox) : BoundingBox` — answers the bounds of a transformed box.
- `Rhino.Geometry.Transform.TryGetInverse(out Transform inverse) : bool` — answers the inverse transform when invertible.

[BOUNDING_BOX]:
- `Rhino.Geometry.BoundingBox.Center : Point3d` — box center.
- `Rhino.Geometry.BoundingBox.Diagonal : Vector3d` — min-to-max diagonal vector.
- `Rhino.Geometry.BoundingBox.Inflate(double amount) : void` — grows the box uniformly; negative amounts shrink, invalid boxes never inflate.
- `Rhino.Geometry.BoundingBox.Inflate(double xAmount, double yAmount, double zAmount) : void` — grows the box per axis.
- `Rhino.Geometry.BoundingBox.GetCorners() : Point3d[]` — answers the eight corner points.
- `Rhino.Geometry.BoundingBox.GetEdges() : Line[]` — answers the twelve edge lines.
- `Rhino.Geometry.BoundingBox.Transform(Transform xform) : bool` — transforms the box corners in place.

[MESH_TEXTURE_CACHE]:
- `Rhino.Geometry.Mesh.HasCachedTextureCoordinates : bool` — reports whether any mapping-derived coordinate set is cached.
- `Rhino.Geometry.Mesh.SetCachedTextureCoordinatesFromMaterial(RhinoObject rhinoObject, Material material) : void` — populates every coordinate set required by the material's texture channels against the object's mappings.
- `Rhino.Geometry.Mesh.GetCachedTextureCoordinates(Guid textureMappingId) : CachedTextureCoordinates` / `GetCachedTextureCoordinates(RhinoObject rhinoObject, Texture texture) : CachedTextureCoordinates` — returns a nullable read-only coordinate wrapper by mapping identity or resolved texture channel.
- `Rhino.Geometry.Mesh.InvalidateCachedTextureCoordinates(bool bOnlyInvalidateCachedSurfaceParameterMapping = false) : void` — invalidates all cached coordinates or only the surface-parameter subset.

[CLIP_PARTICIPATION]:
- `Rhino.Geometry.ClippingPlaneSurface.PlaneDepth : double` — reads the finite clip depth below the plane.
- `Rhino.Geometry.ClippingPlaneSurface.PlaneDepthEnabled : bool` — whether the finite clip depth is active.
- `Rhino.Geometry.ClippingPlaneSurface.ParticipationListsEnabled : bool` — whether the participation lists are active.
- `Rhino.Geometry.ClippingPlaneSurface.AddClipViewportId(Guid viewportId) : bool` — adds a clipped viewport.
- `Rhino.Geometry.ClippingPlaneSurface.DimensionStyleId : Guid` — get/set label dimension style; `Guid.Empty` clears it.
- `Rhino.Geometry.ClippingPlaneSurface.RemoveClipViewportId(Guid viewportId) : bool` — removes a clipped viewport.
- `Rhino.Geometry.ClippingPlaneSurface.SetClipParticipation(IEnumerable<Guid> objectIds, IEnumerable<int> layerIndices, bool isExclusionList) : void` — sets the per-object and per-layer participation lists.
- `Rhino.Geometry.ClippingPlaneSurface.GetClipParticipation(out IEnumerable<Guid> objectIds, out IEnumerable<int> layerIndices, out bool isExclusionList) : void` — reads the participation lists.
- `Rhino.Geometry.ClippingPlaneSurface.ClearClipParticipationLists() : void` — clears the participation lists.

[MAKE2D_COMPUTE]:
- `Rhino.Geometry.HiddenLineDrawing.Compute(HiddenLineDrawingParameters parameters, bool multipleThreads) : HiddenLineDrawing` — computes the hidden-line drawing from a filled parameter set.
- `Rhino.Geometry.HiddenLineDrawing.Compute(HiddenLineDrawingParameters parameters, bool multipleThreads, IProgress<double> progress, CancellationToken cancelToken) : HiddenLineDrawing` — cancellable, progress-reporting compute.
- `Rhino.Geometry.HiddenLineDrawingParameters.SetViewport(ViewportInfo viewport) : void` / `SetViewport(RhinoViewport viewport) : void` — sets the projection frame.
- `Rhino.Geometry.HiddenLineDrawingParameters.AddClippingPlane(Plane plane) : void` — adds a clipping plane to the projection.
- `Rhino.Geometry.HiddenLineDrawingParameters.AddGeometry(GeometryBase geometry, object tag) : bool` — adds tagged source geometry; overloads add `bool occluding_sections`, `Transform xform`, and both.
- `Rhino.Geometry.HiddenLineDrawingParameters.AddGeometryAndPlanes(GeometryBase geometry, object tag, List<Plane> clips) : bool` — adds geometry with per-object clips; overloads add `Transform xform` and `bool occluding_sections`.
- `Rhino.Geometry.HiddenLineDrawingParameters.AbsoluteTolerance : double` (default `0.01`), `Flatten : bool`, `IncludeTangentEdges : bool`, `IncludeTangentSeams : bool`, `IncludeHiddenCurves : bool`, `OccludingSectionOption : bool` — the build flags.

[MAKE2D_RESULTS]:
- `Rhino.Geometry.HiddenLineDrawing.Segments : IEnumerable<HiddenLineDrawingSegment>` — computed 2D segments.
- `Rhino.Geometry.HiddenLineDrawing.Points : IEnumerable<HiddenLineDrawingPoint>` — computed 2D points.
- `Rhino.Geometry.HiddenLineDrawing.Viewport : ViewportInfo` — projection viewport.
- `Rhino.Geometry.HiddenLineDrawing.WorldToHiddenLine : Transform` — world-to-drawing projection frame.

[SILHOUETTE_COMPUTE]:
- `Rhino.Geometry.Silhouette.Compute(GeometryBase geometry, SilhouetteType silhouetteType, Point3d perspectiveCameraLocation, double tolerance, double angleToleranceRadians) : Silhouette[]` — perspective-camera silhouettes; an overload adds `IEnumerable<Plane> clippingPlanes, CancellationToken cancelToken`.
- `Rhino.Geometry.Silhouette.Compute(GeometryBase geometry, SilhouetteType silhouetteType, Vector3d parallelCameraDirection, double tolerance, double angleToleranceRadians) : Silhouette[]` — parallel-camera silhouettes; an overload adds `IEnumerable<Plane> clippingPlanes, CancellationToken cancelToken`.
- `Rhino.Geometry.Silhouette.Compute(GeometryBase geometry, SilhouetteType silhouetteType, ViewportInfo viewport, double tolerance, double angleToleranceRadians) : Silhouette[]` — viewport-frame silhouettes; an overload adds `IEnumerable<Plane> clippingPlanes, CancellationToken cancelToken`.
- `Rhino.Geometry.Silhouette.ComputeDraftCurve(GeometryBase geometry, double draftAngle, Vector3d pullDirection, double tolerance, double angleToleranceRadians) : Silhouette[]` — constant-draft-angle curve extraction; an overload adds `CancellationToken cancelToken`.
- `Rhino.Geometry.Silhouette.SilhouetteType : SilhouetteType` / `GeometryComponentIndex : ComponentIndex` / `Curve : Curve` — per-result classification, source component, and the 3D silhouette curve (`Curve` is null-capable on a projecting region).
- `Rhino.Geometry.HiddenLineDrawing.BoundingBox(bool includeHidden) : BoundingBox` — drawing bounds, optionally including hidden curves.
- `Rhino.Geometry.HiddenLineDrawing.RejoinCompatibleVisible() : void` — merges compatible visible segments after compute.
- `Rhino.Geometry.HiddenLineDrawingSegment.CurveGeometry : Curve` / `SegmentVisibility : Visibility` / `CurveSideFills : SideFill[]` / `ParentCurve : HiddenLineDrawingObjectCurve` / `IsSceneSilhouette : bool` — per-segment geometry, visibility, and per-side surface fills.
- `Rhino.Geometry.HiddenLineDrawingObjectCurve.SilhouetteType : SilhouetteType` / `SourceObject : HiddenLineDrawingObject` / `SourceObjectComponentIndex : ComponentIndex` / `Segments : HiddenLineDrawingSegment[]` / `Curve(double t) : HiddenLineDrawingSegment` — source-curve topology and segment lookup.
- `Rhino.Geometry.HiddenLineDrawingPoint.Location : Point3d` / `PointVisibility : Visibility` / `SourceObject : HiddenLineDrawingObject` / `SourceObjectComponentIndex : ComponentIndex` — per-point result and source.
- `Rhino.Geometry.HiddenLineDrawingObject.Geometry : GeometryBase` / `Transform : Transform` / `Tag : object` / `OccludingSections : bool` — source-object accessors.

`Intersection` members below name `Rhino.Geometry.Intersect.Intersection` statics, dotted bare after the first in each group; the outcome enums classify each primitive result.

[INTERSECT_PRIMITIVE]:
- `Intersection.LineLine(Line lineA, Line lineB, out double a, out double b) : bool` — infinite-line intersection; overload adds `double tolerance, bool finiteSegments` for segment intersection.
- `Intersection.LinePlane(Line line, Plane plane, out double lineParameter) : bool` — line-plane parameter.
- `Intersection.PlanePlane(Plane planeA, Plane planeB, out Line intersectionLine) : bool` / `PlanePlanePlane(Plane planeA, Plane planeB, Plane planeC, out Point3d intersectionPoint) : bool` — plane-pair and plane-triple.
- `Intersection.LineBox(Line line, BoundingBox box, double tolerance, out Interval lineParameters) : bool` — line-box; a `Box` overload takes an oriented box.
- `Intersection.PlaneBoundingBox(Plane plane, BoundingBox boundingBox, out Polyline polyline) : bool` — plane-box section polyline.
- `Intersection.PlaneCircle(Plane plane, Circle circle, out double firstCircleParameter, out double secondCircleParameter) : PlaneCircleIntersection` — plane-circle.
- `Intersection.PlaneSphere(Plane plane, Sphere sphere, out Circle intersectionCircle) : PlaneSphereIntersection` — plane-sphere.
- `Intersection.LineCircle(Line line, Circle circle, out double t1, out Point3d point1, out double t2, out Point3d point2) : LineCircleIntersection` — line-circle.
- `Intersection.LineSphere(Line line, Sphere sphere, out Point3d p1, out Point3d p2) : LineSphereIntersection` / `LineCylinder(Line line, Cylinder cylinder, out Point3d p1, out Point3d p2) : LineCylinderIntersection` — line-sphere and line-cylinder.
- `Intersection.SphereSphere(Sphere a, Sphere b, out Circle intersectionCircle) : SphereSphereIntersection` / `ArcArc(Arc a, Arc b, out Point3d p1, out Point3d p2) : ArcArcIntersection` / `CircleCircle(Circle a, Circle b, out Point3d p1, out Point3d p2) : CircleCircleIntersection` — analytic pair intersections.

[INTERSECT_CURVE]:
- `Intersection.CurveCurve(Curve curveA, Curve curveB, double tolerance, double overlapTolerance) : CurveIntersections` — curve-curve events; `CurveCurveValidate(…, out int[] invalidIndices, out TextLog textLog)` reports invalid spans.
- `Intersection.CurveSelf(Curve curve, double tolerance) : CurveIntersections` / `CurvePlane(Curve curve, Plane plane, double tolerance) : CurveIntersections` / `CurveLine(Curve curve, Line line, double tolerance, double overlapTolerance) : CurveIntersections` — self, plane, and line events.
- `Intersection.CurveSurface(Curve curve, Surface surface, double tolerance, double overlapTolerance) : CurveIntersections` — curve-surface; a `(…, Interval curveDomain, …)` overload restricts the domain, and two `CurveSurfaceValidate` overloads add `out int[] invalidIndices, out TextLog textLog`.
- `Intersection.CurveBrep(Curve curve, Brep brep, double tolerance, out Curve[] overlapCurves, out Point3d[] intersectionPoints) : bool` — curve-brep; overloads add `out double[] curveParameters` and the angle-tolerance form `(…, double angleTolerance, out double[] t)`.
- `Intersection.CurveBrepFace(Curve curve, BrepFace face, double tolerance, out Curve[] overlapCurves, out Point3d[] intersectionPoints) : bool` — curve against one brep face.

[INTERSECT_SURFACE_BREP]:
- `Intersection.SurfaceSurface(Surface surfaceA, Surface surfaceB, double tolerance, out Curve[] intersectionCurves, out Point3d[] intersectionPoints) : bool` — surface-surface.
- `Intersection.BrepPlane(Brep brep, Plane plane, double tolerance, out Curve[] intersectionCurves, out Point3d[] intersectionPoints) : bool` — brep-plane; a `bool joinCurves` overload joins the section.
- `Intersection.GeometryPlane(GeometryBase geometry, Plane plane, double tolerance, bool joinCurves, RhinoDoc doc, out Curve[] intersectionCurves, out Point3d[] intersectionPoints) : bool` — generic geometry section against a plane.
- `Intersection.BrepBrep(Brep brepA, Brep brepB, double tolerance, out Curve[] intersectionCurves, out Point3d[] intersectionPoints) : bool` — brep-brep; a `bool joinCurves` overload joins.
- `Intersection.BrepSurface(Brep brep, Surface surface, double tolerance, out Curve[] intersectionCurves, out Point3d[] intersectionPoints) : bool` — brep-surface; a `bool joinCurves` overload joins.

[INTERSECT_MESH]:
- `Intersection.MeshPlane(Mesh mesh, Plane plane) : Polyline[]` — mesh section; an `IEnumerable<Plane>` overload sections against many planes, and cache overloads take a `MeshIntersectionCache`, `double tolerance`, and optional `bool overlaps`.
- `Intersection.MeshMeshFast(Mesh meshA, Mesh meshB) : Line[]` / `MeshMeshAccurate(Mesh meshA, Mesh meshB, double tolerance) : Polyline[]` — fast unwelded lines vs accurate polylines.
- `Intersection.MeshMesh(IEnumerable<Mesh> meshes, double tolerance, out Polyline[] intersections, bool overlapsPolylines, out Polyline[] overlapsPolylinesResult, bool overlapsMesh, out Mesh overlapsMeshResult, TextLog textLog, CancellationToken cancel, IProgress<double> progress) : bool` — full multi-mesh intersection with overlap outputs.
- `Intersection.MeshMeshPredicate(IEnumerable<Mesh> meshes, double tolerance, out int[] pairs, TextLog textLog) : bool` — mesh-pair predicate.
- `Intersection.MeshRay(Mesh mesh, Ray3d ray) : double` — ray parameter; an `out int[] meshFaceIndices` overload reports the hit faces.
- `Intersection.MeshLine(Mesh mesh, Line line) : Point3d[]` / `MeshLineSorted(…, out int[] faceIds) : Point3d[]` / `MeshPolyline(Mesh mesh, PolylineCurve curve, out int[] faceIds) : Point3d[]` / `MeshPolylineSorted(…) : Point3d[]` — mesh-line and mesh-polyline hit points, sorted variants ordering along the line.
- `Intersection.MeshCurve(Mesh mesh, Curve curve, double tolerance) : MeshCurveIntersection[]` — event array; a `bool includeOverlaps` overload includes overlaps, and two point-returning overloads emit `out Curve[] overlapCurves` and face-id arrays.

[INTERSECT_RAY_PROJECT]:
- `Intersection.RayShoot(Ray3d ray, IEnumerable<GeometryBase> geometry, int maxReflections) : Point3d[]` — reflection points; the `RayShoot(IEnumerable<GeometryBase> geometry, Ray3d ray, int maxReflections) : RayShootEvent[]` overload returns per-hit events.
- `Intersection.ProjectPointsToMeshes(IEnumerable<Mesh> meshes, IEnumerable<Point3d> points, Vector3d direction, double tolerance) : Point3d[]` — directional point projection; the `…Ex(…, out int[] indices)` form reports source indices.
- `Intersection.ProjectPointsToBreps(IEnumerable<Brep> breps, IEnumerable<Point3d> points, Vector3d direction, double tolerance) : Point3d[]` — brep projection; the `…Ex(…, out int[] indices)` form reports source indices.

[INTERSECT_CARRIERS]:
- `Rhino.Geometry.Intersect.CurveIntersections.Count : int` / `this[int index] : IntersectionEvent` / `CopyTo(IntersectionEvent[] array, int arrayIndex) : void` / `Dispose() : void` — indexed disposable event list.
- `Rhino.Geometry.Intersect.IntersectionEvent.IsPoint : bool` / `IsOverlap : bool` / `PointA`/`PointA2`/`PointB`/`PointB2 : Point3d` / `ParameterA`/`ParameterB : double` / `OverlapA`/`OverlapB : Interval` — one curve or surface event; `SurfacePointParameter(out double u, out double v) : void` and `SurfaceOverlapParameter(out Interval uDomain, out Interval vDomain) : void` read surface parameters.
- `Rhino.Geometry.Intersect.MeshCurveIntersection.Incidence : MeshIncidence` / `FaceIndex : int` / `CurveParameter : Interval` / `PointA`/`PointB : Point3d` / `BarycentricA`/`BarycentricB : Vector3d` / `IsPoint`/`IsOverlap : bool` — one mesh-curve event.
- `Rhino.Geometry.Intersect.MeshClash.MeshA`/`MeshB : Mesh` / `ClashPoint : Point3d` / `ClashRadius : double`; static `Search(IEnumerable<Mesh> setA, IEnumerable<Mesh> setB, double distance, int maxEventCount) : MeshClash[]` and `FindDetail(RhinoObject objA, RhinoObject objB, double distance) : Mesh[]` — clash search and detail meshing.
- `Rhino.Geometry.Intersect.RayShootEvent` (`GeometryIndex : int`, `BrepFaceIndex : int`, `Point : Point3d`) and `MeshInterference` (`IndexA : int`, `IndexB : int`, `HitPoints : Point3d[]`) — value-struct hit records.

[COLLECTION_BUFFERS]:
- `Rhino.Collections.Point3dList(int initialCapacity)` / `Point3dList(IEnumerable<Point3d> collection)` / `Point3dList(params Point3d[] initialPoints)` — point-buffer constructors.
- `Rhino.Collections.Point3dList.BoundingBox : BoundingBox` / `X : XAccess` / `Y : YAccess` / `Z : ZAccess` — bounds and per-axis `double this[int]` accessors.
- `Rhino.Collections.Point3dList.ClosestIndex(Point3d testPoint) : int` / `Add(double x, double y, double z) : void` / `Transform(Transform xform) : void` / `SetAllX(double xValue) : void` / `SetAllY(double) : void` / `SetAllZ(double) : void` — closest-index query, coordinate add, and in-place edit.
- `Rhino.Collections.Point3dList.ClosestIndexInList(IList<Point3d> list, Point3d testPoint) : int` (static) / `ClosestPointInList(IList<Point3d> list, Point3d testPoint) : Point3d` (static) — free-function closest lookups.
- `Rhino.Collections.CurveList.Add(Line line) : void` / `Add(Circle) : void` / `Add(Arc) : void` / `Add(Ellipse) : void` / `Add(IEnumerable<Point3d> polyline) : void` — primitive-to-curve add; `Insert(int index, …)` mirrors each, and `Transform(Transform xform) : bool` transforms every curve.
- `Rhino.Collections.TransformObjectList.Add(RhinoObject rhinoObject) : void` / `Add(ObjRef objref) : void` / `AddObjects(GetObject go, bool allowGrips) : int` / `ObjectArray() : RhinoObject[]` / `GripArray() : GripObject[]` / `UpdateDisplayFeedbackTransform(Transform xform) : bool` — transform-command staging and display feedback.
- `Rhino.Collections.RhinoList.Point3dKNeighbors(PointCloud cloud, IEnumerable<Point3d> needlePts, int amount) : IEnumerable<int[]>` — arity-0 static KNN helper (`PointCloudKNeighbors`/`Point3fKNeighbors`/`Point2dKNeighbors`/`Point2fKNeighbors` share the shape); the arity-1 `RhinoList<T>` is the separate `IList<T>` list base the typed lists extend.

[LIGHT_GEOMETRY]:
- `Rhino.Geometry.Light()` — empty light; `Rhino.Geometry.Light.LightStyle : LightStyle` and `IsEnabled : bool` set the kind and enablement, with `IsPointLight`/`IsDirectionalLight`/`IsSpotLight`/`IsLinearLight`/`IsRectangularLight`/`IsSunLight : bool` reading it and `CoordinateSystem : CoordinateSystem` naming the frame.
- `Rhino.Geometry.Light.Location : Point3d` / `Direction : Vector3d` / `PerpendicularDirection : Vector3d` / `Length : Vector3d` / `Width : Vector3d` — light-geometry pose; linear and rectangular lights read `Length`/`Width`.
- `Rhino.Geometry.Light.Intensity : double` / `PowerWatts : double` / `PowerLumens : double` / `PowerCandela : double` — intensity and photometric power.
- `Rhino.Geometry.Light.Ambient`/`Diffuse`/`Specular : System.Drawing.Color` / `ShadowIntensity : double` — color and shadow.
- `Rhino.Geometry.Light.AttenuationType : Attenuation` / `AttenuationVector : Vector3d` / `SetAttenuation(double a0, double a1, double a2) : void` / `GetAttenuation(double d) : double` — fall-off model, with static `ConstantAttenuationVector`/`LinearAttenuationVector`/`InverseSquaredAttenuationVector` presets.
- `Rhino.Geometry.Light.SpotAngleRadians : double` / `SpotExponent : double` / `HotSpot : double` / `GetSpotLightRadii(out double innerRadius, out double outerRadius) : bool` — spot-cone parameters.
- `Rhino.Geometry.Light.CreateSunLight(double northAngleDegrees, double azimuthDegrees, double altitudeDegrees) : Light` (static) / `CreateSunLight(double northAngleDegrees, DateTime when, double latitudeDegrees, double longitudeDegrees) : Light` (static) / `CreateSunLight(Sun sun) : Light` (static) — sun-light factories; `Name : string` and `Id : Guid` carry identity.

## [04]-[IMPLEMENTATION_LAW]

[CROSSING_TOPOLOGY]:
- `GeometryBase` is the native crossing owner: `DuplicateShallow` shares the native pointer (`IsShallowDuplicate` reports it), `Duplicate` lifts geometry off `IsDocumentControlled` document ownership, and `DataCRC` is the content-identity hash. Geometry read from a document is document-controlled; a boundary that mutates or retains it duplicates first.
- transform mutation (`Transform`/`Translate`/`Scale`/`Rotate`) and bounds derivation (`GetBoundingBox`) act on the native geometry, while the `Transform` value struct owns the frame factories and the decomposition/inverse surface; the decomposition splits a matrix into translation, rotation, dilation, and scaling, and the boundary reads the `TransformSimilarityType` classification rather than inspecting matrix entries.
- clip participation is a triple on `ClippingPlaneSurface` — the clipped-viewport set, the object/layer participation lists (inclusion or exclusion), and the finite `PlaneDepth` — each gated by its `*Enabled` flag and mutated only through the participation members.
- mesh texture coordinates form one cache lifecycle — material and object mappings prime the sets, mapping identity selects a read-only `CachedTextureCoordinates` wrapper, `HasCachedTextureCoordinates` reports presence, and scoped invalidation clears all rows or only surface-parameter rows.

[HOST_ALGORITHM_TOPOLOGY]:
- Make2D is a compute pipeline, not live geometry: `HiddenLineDrawingParameters` accumulates the viewport frame, tagged source geometry, clip planes, and tangent/hidden flags, and `HiddenLineDrawing.Compute` projects them once into an immutable `Segments`/`Points` result keyed by the `WorldToHiddenLine` frame; a segment's `Visibility` and per-side `SideFill` classify it, and the source object and component index trace each result back to its input.
- `Intersection` and the hidden-line families are host geometry-algorithm surfaces recorded as parity information — a design page reads them to know the host offering, and the `Rasm` kernel owns the composed intersection algorithm the page reserves; the catalog documents the surface, never a page's algorithm choice.
- `Rhino.Collections` buffers are host list carriers: `Point3dList`/`CurveList` extend the `RhinoList<T>` base with typed add/insert and, on `Point3dList`, per-axis accessors, bounds, and closest-index; a boundary reads or fills a buffer and hands the canonical `Rasm` collection onward, never retaining the host list, and `TransformObjectList` stages a transform command's objects and grips.
- `Light` is one geometry type discriminated by `LightStyle`: the point, spot, directional, linear, rectangular, ambient, and sun kinds share the location/direction/length/width pose, intensity/power, color, shadow, and attenuation members, and the boundary reads the kind through the `Is*Light` predicates and the `LightStyle` value rather than branching on construction.

[STACKING]:
- `LanguageExt.Core`(`api-languageext`): a `bool` transform or user-string mutation projects to `Fin<Unit>`; a `TryGetInverse`, nullable `GetUserString`, or nullable coordinate wrapper lifts to `Option<Transform>`/`Option<string>`/`Option<CachedTextureCoordinates>`; `GetUserStrings` and the clip participation lists land as `HashMap<string, string>` and `Seq<Guid>`/`Seq<int>`; `GetCorners`/`GetEdges` and detached coordinate rows land as `Arr<Point3d>`/`Arr<Line>`; a `DecomposeSimilarity`/`DecomposeRigid`/`DecomposeAffine` fans its parallel out-parameters into one decomposition record on a `Fin` rail keyed by the classification.
- `Thinktecture.Runtime.Extensions`(`api-thinktecture-runtime-extensions`): `ObjectType` and `TransformSimilarityType` wrap as `[SmartEnum<TKey>]` / `[Flags]`-backed owners; a `DataCRC` content key wraps as a `[ValueObject<T>]`; the clip participation state models as a `[Union]` over disabled, inclusion-list, and exclusion-list cases.
- `Rasm` kernel: the pure linear algebra behind transform composition, decomposition, and bounding-box math is the kernel's; the boundary crosses the native `Transform`/`BoundingBox` at the wire and re-derives none of it. Composed intersection and hidden-line algorithms are the kernel's too — the catalog records the host `Intersection`/`HiddenLineDrawing` surface as parity, and a design page reserves the kernel algorithm rather than calling the host static.
- Make2D and intersection rails (`api-languageext`): `HiddenLineDrawing.Compute` and every `Intersection` static land on `Fin<A>` with the parallel out-parameter arrays fanned into one result record (`Seq<Curve>`/`Seq<Point3d>`/`Seq<IntersectionEvent>`); a `CurveIntersections` or `MeshIntersectionCache` disposable is `use`-bracketed, and an empty or nullable host array crosses as `Option`/`Seq`, never raw.
- intersection and light vocabularies (`api-thinktecture-runtime-extensions`): the outcome enums (`LineCircleIntersection`, `MeshIncidence`, and siblings) and `LightStyle`/`Light.Attenuation` map at the edge to `[SmartEnum]` owners, and a `Light` reads its kind through the `Is*Light` predicates rather than a construction branch.

[LOCAL_ADMISSION]:
- native geometry enters through the crossing owner: a document-controlled instance is `Duplicate`d before retention or mutation, keyed by its `DataCRC` content hash and carried user strings; a transform enters as a factory-built or decomposed `Transform` value and applies through the instance mutation members.
- host geometry types never leak past the boundary; downstream code holds canonical `Rasm` geometry keyed by the content hash, and the crossing is the only place native `GeometryBase` state is read or written.

[RAIL_LAW]:
- Surface: `Rhino.Geometry` host-boundary crossing, plus the `Rhino.Geometry.Intersect` and `Rhino.Collections` host surfaces
- Owns: native geometry identity and duplication, user-string carriage, `DataCRC` content hashing, instance transform mutation, bounding-box derivation, mesh texture-coordinate cache lifecycle, the `Transform` factory/decomposition/inverse surface, clip participation, the `HiddenLineDrawing` Make2D projection surface, the `Rhino.Geometry.Intersect` roster recorded as host parity, the `Rhino.Collections` point/curve buffers, and the `Light` geometry type.
- Accept: document-control-aware duplication, content-keyed identity, transform composition and decomposition, plane and world bounds, coordinate-cache prime/read/invalidation, clip participation projected onto `Fin`/`Option` rails, Make2D projection compute and result classification, intersection results fanned onto `Fin`/`Seq` rails, host list buffers read into canonical collections, and a `Light` kind read through `LightStyle`/`Is*Light`.
- Reject: re-deriving kernel geometry algorithms (composed from `Rasm`), retaining document-controlled geometry without duplication, matrix-entry inspection where a decomposition exists, leaking host geometry types past the boundary, calling the host `Intersection`/`HiddenLineDrawing` static where a design page reserves the `Rasm` kernel, retaining a host `RhinoList`/`Point3dList` past the boundary, and branching a light on construction rather than its `LightStyle`.
