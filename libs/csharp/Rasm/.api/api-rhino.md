# [RASM_API_RHINO]

`Rhino.Geometry` supplies the value-type geometry vocabulary the kernel composes through `Rasm.Vectors` — points, vectors, transforms, intervals, bounding volumes, primitive solids, curves, and meshes — never re-minted and never reached through a document, view, command, or display surface.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `RhinoCommon`
- package: `RhinoCommon`
- assembly: `RhinoCommon`
- namespace: `Rhino.Geometry`
- namespace: `Rhino.Geometry.Intersect`
- asset: host assembly
- rail: host-rhino

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: point, vector, and transform surface
- rail: host-rhino

| [INDEX] | [SYMBOL]     | [CAPABILITY]      |
| :-----: | :----------- | :---------------- |
|   [1]   | `Point3d`    | double point      |
|   [2]   | `Point3f`    | single point      |
|   [3]   | `Vector3d`   | double vector     |
|   [4]   | `Vector3f`   | single vector     |
|   [5]   | `Transform`  | affine transform  |
|   [6]   | `Quaternion` | rotation rotor    |
|   [7]   | `Interval`   | scalar span       |
|   [8]   | `Plane`      | oriented frame    |

[PUBLIC_TYPE_SCOPE]: bounding and primitive-solid surface
- rail: host-rhino

| [INDEX] | [SYMBOL]      | [CAPABILITY]      |
| :-----: | :------------ | :---------------- |
|   [1]   | `BoundingBox` | axis-aligned box  |
|   [2]   | `Box`         | oriented box      |
|   [3]   | `Sphere`     | sphere primitive  |
|   [4]   | `Cylinder`    | cylinder primitive|
|   [5]   | `Cone`        | cone primitive    |
|   [6]   | `Torus`       | torus primitive   |
|   [7]   | `Circle`      | circle primitive  |
|   [8]   | `Arc`         | arc primitive     |
|   [9]   | `Ray3d`       | ray primitive     |
|  [10]   | `Line`        | segment primitive |

[PUBLIC_TYPE_SCOPE]: curve, mesh, and brep surface
- rail: host-rhino

| [INDEX] | [SYMBOL]      | [CAPABILITY]         |
| :-----: | :------------ | :------------------- |
|   [1]   | `Curve`       | curve geometry       |
|   [2]   | `NurbsCurve`  | nurbs curve          |
|   [3]   | `Polyline`    | polyline geometry    |
|   [4]   | `Mesh`        | mesh geometry        |
|   [5]   | `MeshFace`    | mesh face record     |
|   [6]   | `Brep`        | boundary geometry    |
|   [7]   | `GeometryBase`| geometry root        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: vector, transform, and bounds operations
- rail: host-rhino

| [INDEX] | [SURFACE]        | [SURFACE_ROOT] | [CAPABILITY]      |
| :-----: | :--------------- | :------------- | :---------------- |
|   [1]   | `CrossProduct`   | `Vector3d`     | vector cross      |
|   [2]   | `Unitize`        | `Vector3d`     | normalize         |
|   [3]   | `Multiply`       | `Transform`    | compose transform |
|   [4]   | `TryGetInverse`  | `Transform`    | invert transform  |
|   [5]   | `Union`          | `BoundingBox`  | bounds merge      |
|   [6]   | `Contains`       | `BoundingBox`  | point test        |
|   [7]   | `ClosestPoint`   | `Plane`        | frame projection  |

[ENTRYPOINT_SCOPE]: curve and mesh operations
- rail: host-rhino

| [INDEX] | [SURFACE]          | [SURFACE_ROOT] | [CAPABILITY]      |
| :-----: | :----------------- | :------------- | :---------------- |
|   [1]   | `PointAt`          | `Curve`        | curve evaluation  |
|   [2]   | `ClosestPoint`     | `Curve`        | curve projection  |
|   [3]   | `Vertices`         | `Mesh`         | vertex access     |
|   [4]   | `Faces`            | `Mesh`         | face access       |
|   [5]   | `Normals`          | `Mesh`         | normal access     |
|   [6]   | `TopologyVertices` | `Mesh`         | adjacency access  |
|   [7]   | `Faces`            | `Brep`         | brep face access  |
|   [8]   | `Edges`            | `Brep`         | brep edge access  |

## [4]-[IMPLEMENTATION_LAW]

[GEOMETRY_VALUE_LAW]:
- Package: `RhinoCommon` (`Rhino.Geometry`)
- Owns: the value-type geometry vocabulary the kernel reads and re-emits at the seam
- Accept: `Rhino.Geometry` value types composed through `Rasm.Vectors`, never re-minted as a parallel kernel primitive
- Reject: a kernel-local re-mint of a Rhino value type, an epsilon-snapped coordinate where the robust-core owns an exact construction

[BOUNDARY_LAW]:
- Package: `RhinoCommon` (`Rhino.Geometry`)
- Owns: the geometry-only Rhino surface below the document, view, command, and display strata
- Accept: geometry values cross the seam as `Rasm.Vectors` carriers, the robust-core re-emitting `Polyline`/`Point3d`/`Mesh` results at the boundary
- Reject: a `RhinoDoc`/`RhinoApp`/`RhinoView`/`DisplayConduit`/`ObjectTable` reach from the kernel — the document/view/command/display surface is the host-boundary stratum's concern, never the kernel's
