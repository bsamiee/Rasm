# [RASM_RHINO_API_RHINOCOMMON_GEOMETRY]

This catalog owns the host-bound `GeometryBase` crossing — the native-geometry members command input, camera, blocks, and exchange operate on where they meet the document and display: shallow-duplicate and document-control identity, user-string carriage, `DataCRC` content hashing, instance transform mutation, bounding-box derivation, the `Transform` factory and decomposition/inverse surface, and `ClippingPlaneSurface` clip participation. Kernel-grade geometry algorithms live in `Rasm` and are composed, never re-derived here; this catalog carries only the host-boundary crossing set and the value structs it rides, projecting host outcomes onto the `LanguageExt` rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon geometry-crossing surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Geometry`
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

[PUBLIC_TYPE_SCOPE]: value carriers the crossing rides
- rail: geometry-crossing

| [INDEX] | [SYMBOL]               | [KIND]       | [CAPABILITY]                                                  |
| :-----: | :--------------------- | :----------- | :------------------------------------------------------------ |
|  [01]   | `Point3d` / `Vector3d` | value struct | position and direction the transform and bounds members carry |
|  [02]   | `Plane`                | value struct | the frame factories and plane-bound bounding boxes reference  |
|  [03]   | `Line` / `Box`         | value struct | bounding-box edge and oriented-box outputs                    |

## [03]-[ENTRYPOINTS]

[GEOMETRY_IDENTITY]:
- `Rhino.Geometry.GeometryBase.DuplicateShallow() : GeometryBase` — shares the underlying native pointer without a deep copy.
- `Rhino.Geometry.GeometryBase.IsShallowDuplicate : bool` — whether this instance shares another's native pointer.
- `Rhino.Geometry.GeometryBase.Duplicate() : GeometryBase` — deep-copies the geometry off document control.
- `Rhino.Geometry.GeometryBase.IsDocumentControlled : bool` — whether the document owns the lifetime of this geometry.
- `Rhino.Geometry.GeometryBase.ObjectType : ObjectType` — the geometry-type discriminant.
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
- `Rhino.Geometry.GeometryBase.GetUserStrings() : NameValueCollection` — the full user-string collection.
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
- `Rhino.Geometry.Transform.TransformBoundingBox(BoundingBox bbox) : BoundingBox` — the bounds of a transformed box.
- `Rhino.Geometry.Transform.TryGetInverse(out Transform inverse) : bool` — the inverse transform when invertible.

[BOUNDING_BOX]:
- `Rhino.Geometry.BoundingBox.Center : Point3d` — box center.
- `Rhino.Geometry.BoundingBox.Diagonal : Vector3d` — min-to-max diagonal vector.
- `Rhino.Geometry.BoundingBox.Inflate(double xAmount, double yAmount, double zAmount) : void` — grows the box per axis.
- `Rhino.Geometry.BoundingBox.GetCorners() : Point3d[]` — the eight corner points.
- `Rhino.Geometry.BoundingBox.GetEdges() : Line[]` — the twelve edge lines.
- `Rhino.Geometry.BoundingBox.Transform(Transform xform) : bool` — transforms the box corners in place.

[CLIP_PARTICIPATION]:
- `Rhino.Geometry.ClippingPlaneSurface.PlaneDepth : double` — the finite clip depth below the plane.
- `Rhino.Geometry.ClippingPlaneSurface.PlaneDepthEnabled : bool` — whether the finite clip depth is active.
- `Rhino.Geometry.ClippingPlaneSurface.ParticipationListsEnabled : bool` — whether the participation lists are active.
- `Rhino.Geometry.ClippingPlaneSurface.AddClipViewportId(Guid viewportId) : void` — adds a clipped viewport.
- `Rhino.Geometry.ClippingPlaneSurface.RemoveClipViewportId(Guid viewportId) : bool` — removes a clipped viewport.
- `Rhino.Geometry.ClippingPlaneSurface.SetClipParticipation(IEnumerable<Guid> objectIds, IEnumerable<int> layerIndices, bool isExclusionList) : void` — sets the per-object and per-layer participation lists.
- `Rhino.Geometry.ClippingPlaneSurface.GetClipParticipation(out IEnumerable<Guid> objectIds, out IEnumerable<int> layerIndices, out bool isExclusionList) : void` — reads the participation lists.
- `Rhino.Geometry.ClippingPlaneSurface.ClearClipParticipationLists() : void` — clears the participation lists.

## [04]-[IMPLEMENTATION_LAW]

[CROSSING_TOPOLOGY]:
- `GeometryBase` is the native crossing owner: `DuplicateShallow` shares the native pointer (`IsShallowDuplicate` reports it), `Duplicate` lifts geometry off `IsDocumentControlled` document ownership, and `DataCRC` is the content-identity hash. Geometry read from a document is document-controlled; a boundary that mutates or retains it duplicates first.
- transform mutation (`Transform`/`Translate`/`Scale`/`Rotate`) and bounds derivation (`GetBoundingBox`) act on the native geometry, while the `Transform` value struct owns the frame factories and the decomposition/inverse surface; the decomposition splits a matrix into translation, rotation, dilation, and scaling, and the boundary reads the `TransformSimilarityType` classification rather than inspecting matrix entries.
- clip participation is a triple on `ClippingPlaneSurface` — the clipped-viewport set, the object/layer participation lists (inclusion or exclusion), and the finite `PlaneDepth` — each gated by its `*Enabled` flag and mutated only through the participation members.

[STACKING]:
- `LanguageExt.Core`(`api-languageext`): a `bool` transform or user-string mutation projects to `Fin<Unit>`; a `TryGetInverse` and a nullable `GetUserString` lift to `Option<Transform>`/`Option<string>`; `GetUserStrings` and the clip participation lists land as `HashMap<string, string>` and `Seq<Guid>`/`Seq<int>`; `GetCorners`/`GetEdges` land as `Arr<Point3d>`/`Arr<Line>`; a `DecomposeSimilarity`/`DecomposeRigid`/`DecomposeAffine` fans its parallel out-parameters into one decomposition record on a `Fin` rail keyed by the classification.
- `Thinktecture.Runtime.Extensions`(`api-thinktecture-runtime-extensions`): `ObjectType` and `TransformSimilarityType` wrap as `[SmartEnum<TKey>]` / `[Flags]`-backed owners; a `DataCRC` content key wraps as a `[ValueObject<T>]`; the clip participation state models as a `[Union]` over disabled, inclusion-list, and exclusion-list cases.
- `Rasm` kernel: the pure linear algebra behind transform composition, decomposition, and bounding-box math is the kernel's; the boundary crosses the native `Transform`/`BoundingBox` at the wire and re-derives none of it.

[LOCAL_ADMISSION]:
- native geometry enters through the crossing owner: a document-controlled instance is `Duplicate`d before retention or mutation, keyed by its `DataCRC` content hash and carried user strings; a transform enters as a factory-built or decomposed `Transform` value and applies through the instance mutation members.
- host geometry types never leak past the boundary; downstream code holds canonical `Rasm` geometry keyed by the content hash, and the crossing is the only place native `GeometryBase` state is read or written.

[RAIL_LAW]:
- Surface: `Rhino.Geometry` host-boundary crossing
- Owns: native geometry identity and duplication, user-string carriage, `DataCRC` content hashing, instance transform mutation, bounding-box derivation, the `Transform` factory/decomposition/inverse surface, and clip participation.
- Accept: document-control-aware duplication, content-keyed identity, transform composition and decomposition, plane and world bounds, and clip participation projected onto `Fin`/`Option` rails.
- Reject: re-deriving kernel geometry algorithms (composed from `Rasm`), retaining document-controlled geometry without duplication, matrix-entry inspection where a decomposition exists, and leaking host geometry types past the boundary.
