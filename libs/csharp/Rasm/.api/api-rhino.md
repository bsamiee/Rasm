# [RASM_API_RHINO]

`Rhino.Geometry` supplies the value-type geometry vocabulary the kernel composes through `Rasm.Vectors` — points, vectors, transforms, intervals, bounding volumes, primitive solids, curves, and meshes — never re-minted and never reached through a document, view, command, or display surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `RhinoCommon`
- package: `RhinoCommon`
- assembly: `RhinoCommon`
- namespace: `Rhino.Geometry`
- namespace: `Rhino.Geometry.Intersect`
- asset: host assembly
- rail: host-rhino

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: point, vector, and transform surface
- rail: host-rhino

| [INDEX] | [SYMBOL]     | [CAPABILITY]     |
| :-----: | :----------- | :--------------- |
|  [01]   | `Point3d`    | double point     |
|  [02]   | `Point3f`    | single point     |
|  [03]   | `Vector3d`   | double vector    |
|  [04]   | `Vector3f`   | single vector    |
|  [05]   | `Transform`  | affine transform |
|  [06]   | `Quaternion` | rotation rotor   |
|  [07]   | `Interval`   | scalar span      |
|  [08]   | `Plane`      | oriented frame   |

[PUBLIC_TYPE_SCOPE]: bounding and primitive-solid surface
- rail: host-rhino

| [INDEX] | [SYMBOL]      | [CAPABILITY]       |
| :-----: | :------------ | :----------------- |
|  [01]   | `BoundingBox` | axis-aligned box   |
|  [02]   | `Box`         | oriented box       |
|  [03]   | `Sphere`      | sphere primitive   |
|  [04]   | `Cylinder`    | cylinder primitive |
|  [05]   | `Cone`        | cone primitive     |
|  [06]   | `Torus`       | torus primitive    |
|  [07]   | `Circle`      | circle primitive   |
|  [08]   | `Arc`         | arc primitive      |
|  [09]   | `Ray3d`       | ray primitive      |
|  [10]   | `Line`        | segment primitive  |

[PUBLIC_TYPE_SCOPE]: curve, mesh, and brep surface
- rail: host-rhino

| [INDEX] | [SYMBOL]       | [CAPABILITY]      |
| :-----: | :------------- | :---------------- |
|  [01]   | `Curve`        | curve geometry    |
|  [02]   | `NurbsCurve`   | nurbs curve       |
|  [03]   | `Polyline`     | polyline geometry |
|  [04]   | `Mesh`         | mesh geometry     |
|  [05]   | `MeshFace`     | mesh face record  |
|  [06]   | `Brep`         | boundary geometry |
|  [07]   | `GeometryBase` | geometry root     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: vector, transform, and bounds operations
- rail: host-rhino

| [INDEX] | [SURFACE]       | [SURFACE_ROOT] | [CAPABILITY]      |
| :-----: | :-------------- | :------------- | :---------------- |
|  [01]   | `CrossProduct`  | `Vector3d`     | vector cross      |
|  [02]   | `Unitize`       | `Vector3d`     | normalize         |
|  [03]   | `Multiply`      | `Transform`    | compose transform |
|  [04]   | `TryGetInverse` | `Transform`    | invert transform  |
|  [05]   | `Union`         | `BoundingBox`  | bounds merge      |
|  [06]   | `Contains`      | `BoundingBox`  | point test        |
|  [07]   | `ClosestPoint`  | `Plane`        | frame projection  |

[ENTRYPOINT_SCOPE]: curve and mesh operations
- rail: host-rhino

| [INDEX] | [SURFACE]          | [SURFACE_ROOT] | [CAPABILITY]     |
| :-----: | :----------------- | :------------- | :--------------- |
|  [01]   | `PointAt`          | `Curve`        | curve evaluation |
|  [02]   | `ClosestPoint`     | `Curve`        | curve projection |
|  [03]   | `Vertices`         | `Mesh`         | vertex access    |
|  [04]   | `Faces`            | `Mesh`         | face access      |
|  [05]   | `Normals`          | `Mesh`         | normal access    |
|  [06]   | `TopologyVertices` | `Mesh`         | adjacency access |
|  [07]   | `Faces`            | `Brep`         | brep face access |
|  [08]   | `Edges`            | `Brep`         | brep edge access |

## [04]-[IMPLEMENTATION_LAW]

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
