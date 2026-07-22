# [RASM_API_RHINOCOMMON]

`RhinoCommon` owns the Rhino-native geometry value substrate every host boundary crosses. Each carrier is a mutable struct whose in-place mutators report success as `bool` and whose admission rides its own `IsValid` predicate, so a fold threads the struct by value and gates on the predicate rather than the mutator's return. `Rasm` kernel algorithms compose these carriers and never re-derive their algebra.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`
- host: Rhino host runtime, in-process
- assembly: `RhinoCommon`
- namespace: `Rhino.Geometry`
- rail: Rhino-native geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry value carriers

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `Point3d`      | struct        | Euclidean position, distance, and set hygiene        |
|  [02]   | `Vector3d`     | struct        | direction, magnitude, and vector products            |
|  [03]   | `Plane`        | struct        | orthonormal frame with plane-space remapping         |
|  [04]   | `Line`         | struct        | finite segment over an infinite parameter line       |
|  [05]   | `BoundingBox`  | struct        | axis-aligned extent and corner topology              |
|  [06]   | `Transform`    | struct        | 4x4 homogeneous affine matrix and its decompositions |
|  [07]   | `MeshFace`     | struct        | triangle-or-quad source-face index quad              |
|  [08]   | `GeometryBase` | class         | host geometry identity carrying bounds derivation    |

[PUBLIC_TYPE_SCOPE]: companion carriers and outcome discriminants

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------------------ | :------------ | :----------------------------------------------- |
|  [01]   | `Box`                     | struct        | oriented box off frame-aligned bounds derivation |
|  [02]   | `Quaternion`              | struct        | rotation carrier off `Transform.GetQuaternion`   |
|  [03]   | `Interval`                | struct        | ordered parameter span                           |
|  [04]   | `TransformSimilarityType` | enum          | orientation-preserving classification            |
|  [05]   | `TransformRigidType`      | enum          | rigid classification                             |
|  [06]   | `PlaneFitResult`          | enum          | least-squares plane-fit verdict                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: point algebra

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `Point3d.Origin`                                             | static   | world-origin anchor and rotation center |
|  [02]   | `Point3d.Unset`                                              | static   | unset-coordinate sentinel               |
|  [03]   | `Point3d.IsValid`                                            | property | finite-coordinate admission             |
|  [04]   | `Point3d.DistanceTo(Point3d)`                                | instance | Euclidean distance                      |
|  [05]   | `Point3d.DistanceToSquared(Point3d)`                         | instance | squared distance for comparison folds   |
|  [06]   | `Point3d.EpsilonEquals(Point3d, double)`                     | instance | tolerance-banded equality               |
|  [07]   | `Point3d.Transform(Transform)`                               | instance | in-place affine mapping                 |
|  [08]   | `Point3d.Interpolate(Point3d, Point3d, double)`              | instance | in-place parametric blend               |
|  [09]   | `Point3d.ArePointsCoplanar(IEnumerable<Point3d>, double)`    | static   | coplanarity admission over a set        |
|  [10]   | `Point3d.CullDuplicates(IEnumerable<Point3d>, double)`       | static   | tolerance-deduplicated point set        |
|  [11]   | `Point3d.SortAndCullPointList(IEnumerable<Point3d>, double)` | static   | minimum-spacing ordered point set       |
|  [12]   | `Point3d.operator -(Point3d, Point3d) -> Vector3d`           | operator | displacement between points             |
|  [13]   | `Point3d.operator +(Point3d, Vector3d)`                      | operator | point translated by a vector            |
|  [14]   | `Point3d.operator *(Point3d, double)`                        | operator | scaled position                         |

[ENTRYPOINT_SCOPE]: vector algebra

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `Vector3d.XAxis` / `.YAxis` / `.ZAxis`                                     | static   | reference axis directions                |
|  [02]   | `Vector3d.Zero` / `.Unset`                                                 | static   | null and unset-direction sentinels       |
|  [03]   | `Vector3d.IsValid`                                                         | property | finite-component admission               |
|  [04]   | `Vector3d.IsZero`                                                          | property | null-direction admission                 |
|  [05]   | `Vector3d.IsUnitVector`                                                    | property | unit-length admission                    |
|  [06]   | `Vector3d.IsTiny(double)`                                                  | instance | degenerate-direction admission           |
|  [07]   | `Vector3d.Length` / `.SquareLength`                                        | property | magnitude and its comparison-fold square |
|  [08]   | `Vector3d.Unitize()`                                                       | instance | in-place unit projection                 |
|  [09]   | `Vector3d.Reverse()`                                                       | instance | in-place direction flip                  |
|  [10]   | `Vector3d.Rotate(double, Vector3d)`                                        | instance | in-place rotation about an axis          |
|  [11]   | `Vector3d.Transform(Transform)`                                            | instance | in-place affine mapping                  |
|  [12]   | `Vector3d.PerpendicularTo(Vector3d)`                                       | instance | in-place perpendicular construction      |
|  [13]   | `Vector3d.IsParallelTo(Vector3d, double) -> int`                           | instance | signed parallel verdict                  |
|  [14]   | `Vector3d.IsPerpendicularTo(Vector3d, double)`                             | instance | tolerance-banded perpendicularity        |
|  [15]   | `Vector3d.CrossProduct(Vector3d, Vector3d)`                                | static   | oriented vector product                  |
|  [16]   | `Vector3d.Multiply(Vector3d, Vector3d) -> double`                          | static   | dot product                              |
|  [17]   | `Vector3d.Multiply(Vector3d, double)`                                      | static   | scalar scaling                           |
|  [18]   | `Vector3d.VectorAngle(Vector3d, Vector3d)`                                 | static   | unsigned radian angle                    |
|  [19]   | `Vector3d.VectorAngle(Vector3d, Vector3d, Plane)`                          | static   | signed radian angle in a frame           |
|  [20]   | `Vector3d.AreOrthonormal(Vector3d, Vector3d, Vector3d)`                    | static   | orthonormal basis admission              |
|  [21]   | `Vector3d.AreRighthanded(Vector3d, Vector3d, Vector3d)`                    | static   | handedness admission                     |
|  [22]   | `Vector3d.Decompose(Vector3d, Vector3d, Vector3d, out double, out double)` | static   | two-axis coefficient split               |
|  [23]   | `Vector3d.operator *(Vector3d, Vector3d) -> double`                        | operator | dot product                              |
|  [24]   | `Vector3d.operator *(Vector3d, double)`                                    | operator | scaled direction                         |
|  [25]   | `Vector3d.operator /(Vector3d, double)`                                    | operator | scalar-divided direction                 |
|  [26]   | `Vector3d.operator -(Vector3d)`                                            | operator | negated direction                        |

- `Vector3d.Multiply(Vector3d, Vector3d)`: dot product returning `double`; the vector-scalar overloads return `Vector3d`, so argument types decide the result kind.
- `Vector3d.Unitize()`: mutates the receiver and returns `bool`, so a detached unit projection composes `Length` with `operator /`.
- `Vector3d.IsParallelTo(Vector3d)`: `1` parallel, `-1` anti-parallel, `0` neither or a zero operand.

[ENTRYPOINT_SCOPE]: plane frames

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :-------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `Plane.WorldXY` / `.WorldYZ` / `.WorldZX`                             | static   | world reference frames                  |
|  [02]   | `Plane.Unset`                                                         | static   | unset-frame sentinel                    |
|  [03]   | `Plane(Point3d, Vector3d, Vector3d)`                                  | ctor     | frame from origin and two axes          |
|  [04]   | `Plane(Point3d, Vector3d)`                                            | ctor     | frame from origin and normal            |
|  [05]   | `Plane.CreateFromNormalYup(Point3d, Vector3d, Vector3d)`              | static   | normal-driven frame with a y-up hint    |
|  [06]   | `Plane.CreateFromPoints(Point3d, Point3d, Point3d)`                   | static   | frame through three positions           |
|  [07]   | `Plane.FitPlaneToPoints(IEnumerable<Point3d>, out Plane, out double)` | static   | least-squares fit with deviation        |
|  [08]   | `Plane.IsValid`                                                       | property | coherent-frame admission                |
|  [09]   | `Plane.Origin`                                                        | property | frame-origin projection                 |
|  [10]   | `Plane.XAxis` / `.YAxis` / `.ZAxis` / `.Normal`                       | property | orthonormal frame directions            |
|  [11]   | `Plane.PointAt(double, double)`                                       | instance | plane-space to world position           |
|  [12]   | `Plane.ClosestPoint(Point3d)`                                         | instance | orthogonal projection onto the frame    |
|  [13]   | `Plane.ClosestParameter(Point3d, out double, out double)`             | instance | plane-space coordinates of a position   |
|  [14]   | `Plane.RemapToPlaneSpace(Point3d, out Point3d)`                       | instance | world position in frame coordinates     |
|  [15]   | `Plane.DistanceTo(Point3d)`                                           | instance | signed distance to the frame            |
|  [16]   | `Plane.DistanceTo(BoundingBox, out double, out double)`               | instance | signed distance band over an extent     |
|  [17]   | `Plane.ValueAt(Point3d)`                                              | instance | plane-equation evaluation               |
|  [18]   | `Plane.GetPlaneEquation()`                                            | instance | four-coefficient equation array         |
|  [19]   | `Plane.IsCoplanar(Plane, double)`                                     | instance | tolerance-banded coplanarity            |
|  [20]   | `Plane.Rotate(double, Vector3d, Point3d)`                             | instance | in-place rotation about axis and center |
|  [21]   | `Plane.Translate(Vector3d)`                                           | instance | in-place displacement                   |
|  [22]   | `Plane.Transform(Transform)`                                          | instance | in-place affine mapping                 |
|  [23]   | `Plane.Flip()`                                                        | instance | in-place x-y swap inverting the normal  |
|  [24]   | `Plane.ExtendThroughBox(BoundingBox, out Interval, out Interval)`     | instance | frame-space span covering an extent     |

- `Plane.Rotate(double, Vector3d)`: mutates the receiver and returns `bool`; frame coherence reads from `Plane.IsValid`.

[ENTRYPOINT_SCOPE]: line projection

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :-------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `Line(Point3d, Point3d)`                                  | ctor     | segment from endpoints                      |
|  [02]   | `Line(Point3d, Vector3d, double)`                         | ctor     | segment from start, direction, and length   |
|  [03]   | `Line.Unset`                                              | static   | unset-segment sentinel                      |
|  [04]   | `Line.From` / `.To`                                       | property | endpoint projection                         |
|  [05]   | `Line.Direction` / `.UnitTangent`                         | property | span vector and its unit form               |
|  [06]   | `Line.Length`                                             | property | settable magnitude moving the `To` endpoint |
|  [07]   | `Line.IsValid`                                            | property | finite-endpoint admission                   |
|  [08]   | `Line.PointAt(double)`                                    | instance | position at a normalized parameter          |
|  [09]   | `Line.PointAtLength(double)`                              | instance | position at an arc-length offset            |
|  [10]   | `Line.ClosestParameter(Point3d)`                          | instance | closest parameter on the infinite line      |
|  [11]   | `Line.ClosestPoint(Point3d, bool)`                        | instance | bounded or infinite closest point           |
|  [12]   | `Line.DistanceTo(Point3d, bool)`                          | instance | bounded or infinite point distance          |
|  [13]   | `Line.MinimumDistanceTo(Line)`                            | instance | closest approach between segments           |
|  [14]   | `Line.MaximumDistanceTo(Line)`                            | instance | furthest separation between segments        |
|  [15]   | `Line.Extend(double, double)`                             | instance | in-place per-end extension                  |
|  [16]   | `Line.ExtendThroughBox(BoundingBox, double)`              | instance | in-place extension to an extent boundary    |
|  [17]   | `Line.Flip()`                                             | instance | in-place endpoint swap                      |
|  [18]   | `Line.Transform(Transform)`                               | instance | in-place affine mapping                     |
|  [19]   | `Line.TryGetPlane(out Plane)`                             | instance | perpendicular frame admission               |
|  [20]   | `Line.TryFitLineToPoints(IEnumerable<Point3d>, out Line)` | static   | least-squares segment fit                   |
|  [21]   | `Line.BoundingBox`                                        | property | axis-aligned extent of the segment          |
|  [22]   | `Line.ToNurbsCurve()`                                     | instance | degree-one curve projection                 |

[ENTRYPOINT_SCOPE]: mesh-face topology

| [INDEX] | [SURFACE]                         | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :-------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `MeshFace(int, int, int)`         | ctor     | triangle face from three indices            |
|  [02]   | `MeshFace(int, int, int, int)`    | ctor     | quad face from four indices                 |
|  [03]   | `MeshFace.Unset`                  | static   | unset-face sentinel                         |
|  [04]   | `MeshFace.A` / `.B` / `.C` / `.D` | property | source vertex indices                       |
|  [05]   | `MeshFace.this[int]`              | property | index-addressed vertex slot                 |
|  [06]   | `MeshFace.IsTriangle`             | property | `C == D` triangle discrimination            |
|  [07]   | `MeshFace.IsQuad`                 | property | `C != D` quadrilateral discrimination       |
|  [08]   | `MeshFace.IsValid(int)`           | instance | vertex-count-bounded index admission        |
|  [09]   | `MeshFace.Set(int, int, int)`     | instance | in-place triangle re-indexing               |
|  [10]   | `MeshFace.Flip()`                 | instance | orientation-reversed face copy              |
|  [11]   | `MeshFace.Repair(Point3d[])`      | instance | invalid-face repair against a vertex buffer |

[ENTRYPOINT_SCOPE]: affine construction

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `Transform.Identity`                                   | static   | neutral composition value                  |
|  [02]   | `Transform.ZeroTransformation` / `.Unset`              | static   | zero and unset sentinels                   |
|  [03]   | `Transform.Translation(Vector3d)`                      | static   | displacement transform                     |
|  [04]   | `Transform.Rotation(double, Vector3d, Point3d)`        | static   | axis-angle transform about a center        |
|  [05]   | `Transform.Rotation(Vector3d, Vector3d, Point3d)`      | static   | direction-to-direction rotation            |
|  [06]   | `Transform.RotationZYX(double, double, double)`        | static   | yaw-pitch-roll transform                   |
|  [07]   | `Transform.PlaneToPlane(Plane, Plane)`                 | static   | frame-to-frame object orientation          |
|  [08]   | `Transform.ChangeBasis(Plane, Plane)`                  | static   | frame-to-frame coordinate rebasing         |
|  [09]   | `Transform.Scale(Plane, double, double, double)`       | static   | per-axis scaling in a frame                |
|  [10]   | `Transform.Scale(Point3d, double)`                     | static   | uniform scaling about an anchor            |
|  [11]   | `Transform.Mirror(Plane)`                              | static   | frame reflection                           |
|  [12]   | `Transform.PlanarProjection(Plane)`                    | static   | orthogonal projection onto a frame         |
|  [13]   | `Transform.ProjectAlong(Plane, Vector3d)`              | static   | oblique projection onto a frame            |
|  [14]   | `Transform.Shear(Plane, Vector3d, Vector3d, Vector3d)` | static   | frame-axis shear                           |
|  [15]   | `Transform.Diagonal(Vector3d)`                         | static   | per-axis diagonal matrix                   |
|  [16]   | `Transform.TransformBoundingBox(BoundingBox)`          | instance | transformed axis-aligned extent            |
|  [17]   | `Transform.TransformList(IEnumerable<Point3d>)`        | instance | batch point mapping                        |
|  [18]   | `Transform.ToFloatArray(bool)`                         | instance | row- or column-dominant float buffer       |
|  [19]   | `Transform.operator *(Transform, Transform)`           | operator | ordered affine composition                 |
|  [20]   | `Transform.operator *(Transform, Point3d)`             | operator | expression-shaped point mapping            |
|  [21]   | `Transform.operator *(Transform, Vector3d)`            | operator | expression-shaped direction mapping        |
|  [22]   | `GeometryBase.GetBoundingBox(bool)`                    | instance | accurate or estimated world bounds         |
|  [23]   | `GeometryBase.GetBoundingBox(Transform)`               | instance | bounds of the transformed geometry         |
|  [24]   | `GeometryBase.GetBoundingBox(Plane, out Box)`          | instance | frame-aligned bounds with the oriented box |
|  [25]   | `GeometryBase.Transform(Transform)`                    | instance | in-place affine mutation                   |

[ENTRYPOINT_SCOPE]: affine inspection and decomposition

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :-------------------------------- |
|  [01]   | `Transform.TryGetInverse(out Transform)`                                              | instance | invertibility-gated inverse       |
|  [02]   | `Transform.Transpose()`                                                               | instance | transposed copy                   |
|  [03]   | `Transform.Determinant`                                                               | property | volume scale factor               |
|  [04]   | `Transform.IsRotation` / `.IsLinear` / `.IsAffine`                                    | property | matrix-class admission            |
|  [05]   | `Transform.SimilarityType` / `.RigidType`                                             | property | orientation and rigidity class    |
|  [06]   | `Transform.DecomposeSimilarity(out Vector3d, out double, out Transform, double)`      | instance | translation, dilation, rotation   |
|  [07]   | `Transform.DecomposeRigid(out Vector3d, out Transform, double)`                       | instance | translation and rotation          |
|  [08]   | `Transform.DecomposeAffine(out Vector3d, out Transform, out Transform, out Vector3d)` | instance | rotation, orthogonal, scale split |
|  [09]   | `Transform.GetQuaternion(out Quaternion)`                                             | instance | rotation as a quaternion          |
|  [10]   | `Transform.GetYawPitchRoll(out double, out double, out double)`                       | instance | Euler-angle extraction            |

[ENTRYPOINT_SCOPE]: bounding-box extent

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :--------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `BoundingBox(Point3d, Point3d)`                      | ctor     | extent from min and max corners      |
|  [02]   | `BoundingBox(IEnumerable<Point3d>, Transform)`       | ctor     | transformed extent over a point set  |
|  [03]   | `BoundingBox.Empty`                                  | static   | inverted seed for accumulation folds |
|  [04]   | `BoundingBox.Unset`                                  | static   | unset-extent sentinel                |
|  [05]   | `BoundingBox.IsValid`                                | property | ordered finite-bounds admission      |
|  [06]   | `BoundingBox.Min` / `.Max`                           | property | corner-point projection              |
|  [07]   | `BoundingBox.Center` / `.Diagonal`                   | property | frame pivot and extent span          |
|  [08]   | `BoundingBox.Volume` / `.Area`                       | property | enclosed volume and surface area     |
|  [09]   | `BoundingBox.Corner(bool, bool, bool)`               | instance | per-axis-selected corner point       |
|  [10]   | `BoundingBox.GetCorners()`                           | instance | eight-corner point buffer            |
|  [11]   | `BoundingBox.GetEdges()`                             | instance | twelve-edge segment buffer           |
|  [12]   | `BoundingBox.PointAt(double, double, double)`        | instance | normalized interior position         |
|  [13]   | `BoundingBox.ClosestPoint(Point3d, bool)`            | instance | surface or interior closest point    |
|  [14]   | `BoundingBox.FurthestPoint(Point3d)`                 | instance | furthest corner from a position      |
|  [15]   | `BoundingBox.MinimumDistanceTo(Point3d)`             | instance | distance to the nearest boundary     |
|  [16]   | `BoundingBox.MaximumDistanceTo(Point3d)`             | instance | distance to the furthest boundary    |
|  [17]   | `BoundingBox.Contains(Point3d, bool)`                | instance | inclusive or strict containment      |
|  [18]   | `BoundingBox.Contains(BoundingBox, bool)`            | instance | extent containment                   |
|  [19]   | `BoundingBox.IsDegenerate(double)`                   | instance | collapsed-axis count                 |
|  [20]   | `BoundingBox.Inflate(double, double, double)`        | instance | in-place per-axis growth             |
|  [21]   | `BoundingBox.Union(BoundingBox)`                     | instance | in-place extent accumulation         |
|  [22]   | `BoundingBox.Union(BoundingBox, BoundingBox)`        | static   | expression-shaped extent merge       |
|  [23]   | `BoundingBox.Intersection(BoundingBox, BoundingBox)` | static   | overlapping extent                   |
|  [24]   | `BoundingBox.MakeValid()`                            | instance | in-place corner reordering           |
|  [25]   | `BoundingBox.Transform(Transform)`                   | instance | in-place affine mapping              |
|  [26]   | `BoundingBox.ToBrep()`                               | instance | boundary-representation solid        |

- `BoundingBox.Union(BoundingBox)`: mutates the receiver and returns `void`; the static overload carries the expression-shaped form.
- `BoundingBox.Empty`: negative-width seed failing `IsValid`, so a first `Union` yields its operand exactly and the fold gates validity at the end.
- `BoundingBox.IsDegenerate(double)`: `0` full, `1` rectangle, `2` line, `3` point, `4` invalid.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every carrier is a mutable value struct, so a fold threads it by value and an in-place mutator's `bool` return marks the operation while admission reads the type's own `IsValid` predicate.
- Coordinate validity is a finite-double predicate, so an `Unset` sentinel fails `IsValid` by construction while a default-constructed struct is the zero value.
- `Transform` composes right-to-left under `operator *`, so a placement folds its factory sequence in application order and `TryGetInverse` gates the reverse leg.
- `MeshFace` encodes arity in its index quad — `C == D` is the triangle — so a quad-to-triangle split re-indexes on one type.
- `GeometryBase` derives world, transformed, and frame-aligned bounds off one geometry instance, so a placement bound reads from the source and its `Transform` directly.

[STACKING]:
- `LanguageExt.Core`(`.api/api-languageext.md`): every `bool` mutator and `out`-parameter probe on this surface projects onto `Fin<A>` or `Option<A>` at the boundary, so a geometry pipeline threads one rail where the host hands back a flag.
- `MathNet.Numerics`(`.api/api-mathnet-numerics.md`): a `Point3d`/`Vector3d` component triple admits through `CreateVector.DenseOfArray<double>` as the nonlinear-minimizer carrier, and a fitted parameter vector crosses back through the `Plane` and `Transform` factories.
- `NetTopologySuite`(`.api/api-nettopologysuite.md`): a `Point3d` set flattens to `Coordinate` sequences for planar predicate topology, and `BoundingBox.Min`/`.Max` supply the `Envelope` every spatial index keys on.
- Within-library: `Rasm` kernel owners consume these structs as the sole host-crossing carrier — a frame lands as one `Plane`, a placement as one `Transform` folded from its factories, and a spatial extent as one `BoundingBox` accumulated from the `Empty` seed.

[LOCAL_ADMISSION]:
- A frame enters as one `Plane` and reads world-space through `PointAt`, `ClosestParameter`, and `RemapToPlaneSpace`.
- An accumulating extent seeds from `BoundingBox.Empty` and folds the instance `Union`; a two-extent merge in expression position takes the static overload.
- A placement composes `Transform` factories under `operator *` and inverts through `TryGetInverse`; a rebasing between frames takes `ChangeBasis` and an object re-orientation takes `PlaneToPlane`.
- Angle work takes `Vector3d.VectorAngle`, the frame overload carrying the sign; parallelism and perpendicularity read the `IsParallelTo` and `IsPerpendicularTo` verdicts.
- Point-set hygiene runs through `CullDuplicates` and `SortAndCullPointList`, and set-level planarity through `ArePointsCoplanar`.

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: Rhino-native point, vector, frame, segment, extent, affine-transform, and mesh-face value algebra at the host boundary
- Accept: `Point3d`, `Vector3d`, `Plane`, `Line`, `BoundingBox`, `Transform`, and `MeshFace` values, and `GeometryBase` bounds derivations
- Reject: hand-rolled 4x4 composition, hand-spelled basis change and inverse solves, per-call unit-vector copies, and the kernel-grade geometry algorithms `Rasm` owns
