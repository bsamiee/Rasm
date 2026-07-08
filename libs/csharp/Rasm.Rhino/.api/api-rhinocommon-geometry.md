# [RASM_RHINO_API_RHINOCOMMON_GEOMETRY]

Catalog scope: the `Rhino.Geometry` value and query surface the boundary composes — carrier types, transforms, and the geometry members command input, camera, blocks, and exchange operate on. Kernel-grade geometry algorithms live in `Rasm`; this catalog carries only the host-boundary crossing set.

[NAMESPACES]:
- `Rhino.Geometry` carriers — `Point3d`/`Point2d`, `Vector3d` (axis constants, cross product, validity), `Plane`, `Line`, `Interval`, `BoundingBox` (corners, inflate, center/diagonal), `Sphere`, `Circle`, `Arc`, `Rectangle3d`, `Box`, `Polyline`.
- `Rhino.Geometry` geometry — `GeometryBase` (`Duplicate`, `Transform`, `GetBoundingBox`, `DataCRC`, `ObjectType`, `BoundsOf`), `Curve` (point-at/parameter/perpendicular-frame families), `Surface`, `Brep`/`BrepFace`, `Mesh`/`MeshFace`, `SubD`, `Extrusion`, `Hatch`, `PointCloud`, `TextDot`, `TextEntity`, `AnnotationBase`, `Light`, `InstanceReferenceGeometry`, `ClippingPlaneSurface` (plane depth, participation lists).
- `Rhino.Geometry` transforms — `Transform` (identity/translation/scale/plane-to-plane, inverse), `CoordinateSystem`.
- `Rhino` — `RhinoMath` (`ZeroTolerance`, `UnsetValue`, `IsValidDouble`, `UnitScale`, `Clamp`), `UnitSystem`.
