# [RASM_API_RHINOCOMMON]

`RhinoCommon` is the Rhino-native geometry substrate. `MeshFace` preserves triangle-versus-quad topology, `Line` owns closest-parameter projection, `Transform` composes affine placement, and `Point3d`, `Vector3d`, and `BoundingBox` expose geometry admission predicates. This catalog homes at the branch tier because the `Rasm` kernel is RhinoCommon-aware and both host-boundary folders compose the same surface; each host-boundary folder `.api/` carries its own host-assembly overlay.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`
- assembly: `RhinoCommon`
- namespace: `Rhino.Geometry`
- asset: Rhino host SDK assembly
- rail: Rhino-native geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mesh topology and linear geometry
- rail: Rhino-native geometry

| [INDEX] | [SYMBOL]      | [SHAPE]                        | [CAPABILITY]                                   |
| :-----: | :------------ | :----------------------------- | :--------------------------------------------- |
|  [01]   | `MeshFace`    | mutable indexed value          | triangle or quad source-face identity          |
|  [02]   | `Line`        | finite-endpoint geometry value | infinite-line parameter and bounded projection |
|  [03]   | `Transform`   | homogeneous transform value    | affine composition and geometry mutation       |
|  [04]   | `Plane`       | mutable frame value            | frame validity and origin projection           |
|  [05]   | `Point3d`     | coordinate value               | Euclidean point distance                       |
|  [06]   | `Vector3d`    | direction and magnitude value  | normalization and vector product               |
|  [07]   | `BoundingBox` | axis-aligned bounds value      | finite spatial extent admission                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: mesh-face topology
- rail: Rhino-native geometry

| [INDEX] | [SURFACE]                         | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :-------------------------------- | :------- | :------------------------------------ |
|  [01]   | `MeshFace.A` / `.B` / `.C` / `.D` | property | source vertex indices                 |
|  [02]   | `MeshFace.IsTriangle`             | property | `C == D` triangle discrimination      |
|  [03]   | `MeshFace.IsQuad`                 | property | `C != D` quadrilateral discrimination |

[ENTRYPOINT_SCOPE]: line projection
- rail: Rhino-native geometry

| [INDEX] | [SURFACE]                          | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :--------------------------------- | :------- | :------------------------------------- |
|  [01]   | `Line.ClosestParameter(Point3d)`   | instance | closest parameter on the infinite line |
|  [02]   | `Line.ClosestPoint(Point3d, bool)` | instance | bounded or infinite closest point      |

[ENTRYPOINT_SCOPE]: affine construction
- rail: Rhino-native geometry

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :---------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Transform.Identity`                            | static   | neutral composition value          |
|  [02]   | `Transform.Translation(Vector3d)`               | static   | displacement transform             |
|  [03]   | `Transform.Rotation(double, Vector3d, Point3d)` | static   | axis-angle transform               |
|  [04]   | `Transform.PlaneToPlane(Plane, Plane)`          | static   | frame-to-frame object orientation  |
|  [05]   | `Transform.operator *(Transform, Transform)`    | operator | ordered affine composition         |
|  [06]   | `Transform.operator *(Transform, Point3d)`      | operator | expression-shaped point mapping    |
|  [07]   | `GeometryBase.GetBoundingBox(Transform)`        | instance | bounds of the transformed geometry |
|  [08]   | `GeometryBase.GetBoundingBox(bool)`             | instance | accurate or estimated world bounds |

An axis-angle rotation inverts by negating its angle over the same axis and center, so a frame and its inverse derive from one `Transform.Rotation` declaration without an inversion probe.

[ENTRYPOINT_SCOPE]: fixture geometry
- rail: Rhino-native geometry

| [INDEX] | [SURFACE]                                   | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------------------------ | :------- | :------------------------------ |
|  [01]   | `Plane.IsValid`                             | property | coherent frame admission        |
|  [02]   | `Plane.Origin`                              | property | frame-origin projection         |
|  [03]   | `Point3d.DistanceTo(Point3d)`               | instance | Euclidean distance              |
|  [04]   | `Vector3d.Unitize()`                        | instance | in-place unit-vector projection |
|  [05]   | `Vector3d.CrossProduct(Vector3d, Vector3d)` | static   | oriented vector product         |
|  [06]   | `Point3d.IsValid`                           | property | finite-coordinate admission     |
|  [07]   | `Vector3d.IsValid`                          | property | finite-component admission      |
|  [08]   | `BoundingBox.IsValid`                       | property | ordered finite-bounds admission |
|  [09]   | `Point3d.Origin`                            | static   | world-origin rotation center    |
|  [10]   | `Vector3d.ZAxis`                            | static   | reference tool-axis direction   |
|  [11]   | `Vector3d.VectorAngle(Vector3d, Vector3d)`  | static   | unsigned radian angle           |
|  [12]   | `Vector3d.IsTiny()`                         | instance | degenerate-direction admission  |
|  [13]   | `Vector3d.IsZero`                           | property | null-direction admission        |
|  [14]   | `Vector3d.Multiply(Vector3d, Vector3d)`     | static   | `double` dot product            |
|  [15]   | `Plane(Point3d, Vector3d, Vector3d)`        | ctor     | frame from origin and two axes  |
|  [16]   | `Plane.XAxis` / `.YAxis` / `.ZAxis`         | property | orthonormal frame directions    |
|  [17]   | `Plane.Rotate(double, Vector3d)`            | instance | in-place rotation about origin  |
|  [18]   | `Point3d.operator -(Point3d, Point3d)`      | operator | displacement between points     |
|  [19]   | `Point3d.operator +(Point3d, Vector3d)`     | operator | point translated by a vector    |
|  [20]   | `BoundingBox.Center` / `.Diagonal`          | property | frame pivot and extent span     |
|  [21]   | `BoundingBox.Contains(Point3d)`             | instance | inclusive containment test      |
|  [22]   | `BoundingBox.Union(BoundingBox)`            | instance | in-place bounds accumulation    |
|  [23]   | `BoundingBox.Volume`                        | property | enclosed-volume projection      |
|  [24]   | `BoundingBox.Empty`                         | static   | inverted seed for accumulation  |
|  [25]   | `BoundingBox.Min` / `.Max`                  | property | corner-point projection         |
|  [26]   | `Vector3d.Unset`                            | static   | unset-direction sentinel value  |
|  [27]   | `Vector3d.Length`                           | property | magnitude projection            |
|  [28]   | `Vector3d.operator /(Vector3d, double)`     | operator | scalar-divided direction        |

`Vector3d.Multiply(Vector3d, Vector3d)` and `operator *(Vector3d, Vector3d)` return `double`, not a vector; the scalar-scaling overloads `Multiply(Vector3d, double)` and `Multiply(double, Vector3d)` return `Vector3d`, so the receiver types decide the result kind at every call site.

`Plane.Rotate` mutates the receiver in place and reports success as `bool`; a frame built from a rotated `Plane` proves validity through `Plane.IsValid` at its owning admission gate rather than by branching on that flag. `Vector3d.Unitize()` mutates its receiver and returns `bool`, so a detached unit projection composes `Length` with `operator /` rather than copying-then-unitizing at each call site; `BoundingBox.Union` likewise mutates, so an accumulating fold threads the struct by value from the `BoundingBox.Empty` seed.
