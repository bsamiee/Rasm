# [RASM_FABRICATION_API_OCCTNET_WRAPPER]

`OcctNet.Wrapper` is the managed 3D solid-CAD / B-rep ingress owner in `Rasm.Fabrication` — a thin .NET wrapper over Open CASCADE Technology (OCCT) through a stable native C ABI. It admits STEP (AP203/AP214/AP242), IGES, and STL into a disposable `OcctShape` B-rep, tessellates the solid to an indexed `OcctMesh`, and models through boolean, sweep, and transform ops on native handles. At the boundary the `OcctMesh` triangle soup crosses to the mesh owner, and a kernel `Rasm` geometry type never enters the OCCT ABI.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OcctNet.Wrapper`
- package: `OcctNet.Wrapper` (MIT)
- assembly: `OcctNet.Wrapper`
- namespaces: `OcctNet.Wrapper` (public surface), `OcctNet.Wrapper.Native` (internal P/Invoke plumbing)
- managed asset: `lib/net10.0/OcctNet.Wrapper.dll` — pure managed, zero transitive NuGet deps (empty `net10.0` dependency group)
- native asset: `runtimes/{osx-arm64,linux-x64,win-x64}/native/` — the osx-arm64 OCCT toolkit set (`libOcctNetNative.dylib` + the `libTK*` OCCT toolkits) ships and resolves on the osx-arm64 consumer; `OcctRuntime.NativeVersion` P/Invokes `GetVersion` to confirm the loaded build
- ABI boundary: `OcctShape` and `OcctPoint3d` own native `SafeHandle`s and are `IDisposable`; `OcctNet.Wrapper.Native` and its `OcctStatus` codes are internal plumbing, never a consumer surface
- rail: fabrication solid-ingress — the 3D B-rep peer of the 2D `ACadSharp` reader, loaded once per process (the native OCCT toolkits are process-global)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: B-rep shape and topology
`OcctShape` is the disposable B-rep root every primitive and topology type derives from; `OcctException` carries the native `StatusCode` into managed code.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]   | [CAPABILITY]              |
| :-----: | :-------------- | :-------------- | :------------------------ |
|  [01]   | `OcctShape`     | B-rep handle    | disposable operation root |
|  [02]   | `OcctFace`      | topology shape  | planar outer-wire face    |
|  [03]   | `OcctEdge`      | topology shape  | straight endpoint edge    |
|  [04]   | `OcctWire`      | topology shape  | ordered edge chain        |
|  [05]   | `OcctBox`       | primitive shape | axis-aligned box          |
|  [06]   | `OcctCylinder`  | primitive shape | radius-height cylinder    |
|  [07]   | `OcctSphere`    | primitive shape | radius sphere             |
|  [08]   | `OcctException` | error type      | native status exception   |

[PUBLIC_TYPE_SCOPE]: mesh and value types
Value types are `readonly record struct`s with canonical anchors `OcctVector3d.Zero` and `OcctAxis1d.{XAxis,YAxis,ZAxis}`; `OcctPointCoordinates` carries endpoints and axis origins by value, while `OcctPoint3d` is the distinct disposable native-handle point.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]            |
| :-----: | :--------------------- | :------------ | :---------------------- |
|  [01]   | `OcctMesh`             | mesh result   | indexed triangle mesh   |
|  [02]   | `OcctMeshVertex`       | value type    | triangle vertex         |
|  [03]   | `OcctVector3d`         | value type    | direction and length    |
|  [04]   | `OcctPointCoordinates` | value type    | point coordinates       |
|  [05]   | `OcctAxis1d`           | value type    | revolution axis         |
|  [06]   | `OcctBoundingBox`      | value type    | shape extents and sizes |
|  [07]   | `OcctPoint3d`          | handle point  | native distance point   |
|  [08]   | `OcctRuntime`          | runtime probe | native-load status      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: import / export — `OcctShape`
Import is the manufacturing-geometry ingress (STEP/IGES B-rep, STL mesh-as-shape); export round-trips the shape to a neutral CAD file. `ImportStep` reads a single shape.

| [INDEX] | [SURFACE]                                   | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :------------------------------------------ | :------- | :---------------------------------- |
|  [01]   | `OcctShape.ImportStep(string) -> OcctShape` | static   | read STEP AP203/AP214/AP242 B-rep   |
|  [02]   | `OcctShape.ImportIges(string) -> OcctShape` | static   | read IGES B-rep                     |
|  [03]   | `OcctShape.ImportStl(string) -> OcctShape`  | static   | read STL as mesh-shape              |
|  [04]   | `shape.ExportStep(string)`                  | instance | write shape to STEP                 |
|  [05]   | `shape.ExportIges(string)`                  | instance | write shape to IGES                 |
|  [06]   | `shape.ExportStl(string, bool)`             | instance | write binary (default) or ASCII STL |

[ENTRYPOINT_SCOPE]: primitive construction and topology
Constructed edges expose `Start`/`End`, wires expose `Edges`, faces expose `OuterWire`.

| [INDEX] | [SURFACE]                                                  | [SHAPE] | [CAPABILITY]        |
| :-----: | :--------------------------------------------------------- | :------ | :------------------ |
|  [01]   | `new OcctBox(double, double, double)`                      | ctor    | axis-aligned solid  |
|  [02]   | `new OcctCylinder(double, double)`                         | ctor    | cylinder solid      |
|  [03]   | `new OcctSphere(double)`                                   | ctor    | sphere solid        |
|  [04]   | `new OcctEdge(OcctPointCoordinates, OcctPointCoordinates)` | ctor    | straight edge       |
|  [05]   | `new OcctWire(params OcctEdge[])`                          | ctor    | ordered edge chain  |
|  [06]   | `new OcctFace(OcctWire)`                                   | ctor    | boundary face       |
|  [07]   | `new OcctPoint3d(double, double, double)`                  | ctor    | native handle point |
|  [08]   | `OcctPoint3d.Origin() -> OcctPoint3d`                      | static  | origin point        |

[ENTRYPOINT_SCOPE]: modeling operations — `OcctShape` instance
Every operation returns a NEW disposable `OcctShape`; the source is unmodified.

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :----------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `shape.Fuse(OcctShape) -> OcctShape`             | instance | boolean union                                      |
|  [02]   | `shape.Cut(OcctShape) -> OcctShape`              | instance | boolean difference                                 |
|  [03]   | `shape.Common(OcctShape) -> OcctShape`           | instance | boolean intersection                               |
|  [04]   | `shape.Extrude(OcctVector3d) -> OcctShape`       | instance | linear extrude a face/wire along a vector          |
|  [05]   | `shape.Revolve(OcctAxis1d, double) -> OcctShape` | instance | revolve a profile about an axis, default full turn |
|  [06]   | `shape.Translate(OcctVector3d) -> OcctShape`     | instance | rigid translation                                  |
|  [07]   | `shape.BoundingBox`                              | property | `OcctBoundingBox` extents and sizes                |
|  [08]   | `shape.IsNull`                                   | property | null-shape guard                                   |

[ENTRYPOINT_SCOPE]: tessellation — `OcctShape` to `OcctMesh`
Tessellation bridges the exact B-rep to an indexed triangle mesh; `linearDeflection` defaults `0.1` and `angularDeflection` defaults `0.5`, smaller values refining chord and angle accuracy.

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]          |
| :-----: | :---------------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `shape.Triangulate(double, double) -> OcctMesh`                   | instance | instance tessellation |
|  [02]   | `OcctMesh.FromShape(OcctShape, double, double) -> OcctMesh`       | static   | static tessellation   |
|  [03]   | `mesh.Vertices`                                                   | property | `OcctMeshVertex` list |
|  [04]   | `mesh.TriangleIndices`                                            | property | flat index triples    |
|  [05]   | `mesh.TriangleCount`                                              | property | triangle count        |
|  [06]   | `new OcctMesh(IReadOnlyList<OcctMeshVertex>, IReadOnlyList<int>)` | ctor     | external indexed mesh |

[ENTRYPOINT_SCOPE]: native runtime probe — `OcctRuntime`
`TryGetNativeVersion` returns the version or the load error without throwing, for the startup gate.

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]      |
| :-----: | :----------------------------------------------------------------- | :------- | :---------------- |
|  [01]   | `OcctRuntime.NativeVersion`                                        | property | loaded version    |
|  [02]   | `OcctRuntime.TryGetNativeVersion(out string, out string?) -> bool` | static   | native-load check |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `OcctShape` and `OcctPoint3d` own native `SafeHandle`s and are `IDisposable` — wrap every shape (imported, constructed, operation-result) in `using`, and treat a boolean/sweep/translate result as a fresh disposable distinct from its operands; a leaked shape leaks native OCCT memory.
- value types are managed and copyable, not disposable — use `OcctPointCoordinates` for edge endpoints and axis origins, reserving the disposable `OcctPoint3d` for the native distance query.
- a native failure surfaces as `OcctException` carrying `StatusCode`; a consumer catches `OcctException` and lowers it to the fabrication fault rail, never reading the internal `OcctStatus` enum.
- gate startup on `OcctRuntime.TryGetNativeVersion` and lower a load failure to a typed capability-miss so the portable-fabrication owner degrades gracefully, never letting a `DllNotFoundException` escape.
- managed binding covers import/export, primitives, boolean, extrude/revolve/translate, and tessellation only; `OcctEdge` is straight point-to-point, so a curved profile is imported (STEP/IGES), never authored edge-by-edge with curvature.

[STACKING]:
- `ACadSharp`(`.api/api-acadsharp.md`): the 2D complement — `ACadSharp` admits 2D profiles (polylines, arcs into `Loop`), this admits 3D solids (STEP/IGES into B-rep into mesh); a 3D STEP solid tessellates then planar-sections to 2D loops, a 2D DXF profile goes straight to `Loop`, neither duplicating the other.
- `CavalierContours`(`.api/api-cavaliercontours.md`): a planar section of the tessellated `OcctMesh` is line-only (the mesh carries no curvature), so arc-comp toolpaths re-fit the line chains through the owned Bolton biarc fold (`Posting/program`) or carry bulge into `CavalierContours`.
- `geometry3Sharp`(`.api/api-geometry3sharp.md`): the `OcctMesh` triangle soup crosses through `Vertices` and `TriangleIndices` to the geometry3Sharp mesh owner — only the admitted mesh crosses, never the live `OcctShape` handle.
- kernel mesh seam: an imported solid flows `ImportStep` into `Triangulate` into `OcctMesh` into the kernel `MeshSpace` vocabulary for the planar-section and `DrawingProjection` rails, while dirty STL routes through the predicate-gated `HealOp`; `OcctVector3d`/`OcctPointCoordinates`/`OcctBoundingBox` map one-directionally at the boundary to the kernel `Vector3d`/`Point3d`/box.
- kernel persistence seam: a `ExportStep`/`ExportIges`/`ExportStl` mints through `ContentKey.Of` into `ContentHash.Of` and enrolls in the Persistence artifact index, never a local hasher or schema-local content-key fork.

[LOCAL_ADMISSION]:
- solid ingress enters at `ImportStep`/`ImportIges` under a `using`, tessellated through `Triangulate` at a tolerance-driven deflection (finer linear deflection for a small precision part); the `OcctMesh` triangle soup is the cross-seam payload, never the live `OcctShape` handle.
- boolean stock-removal (rough-stock minus the part, a fixture-clearance cut) is `Fuse`/`Cut`/`Common` returning fresh disposable shapes; extrude/revolve build a solid from a profile face.
- stock sizing and the keep-out envelope read from `shape.BoundingBox`, never folded from the mesh vertices.

[RAIL_LAW]:
- Package: `OcctNet.Wrapper` (MIT)
- Owns: STEP/IGES/STL read into an `OcctShape` B-rep, B-rep into `OcctMesh` tessellation, STEP/IGES/STL write, primitive construction (box/cylinder/sphere/edge/wire/face), boolean fuse/cut/common, extrude/revolve/translate, bounding-box query, and native-version probe.
- Accept: a single imported `OcctShape` under `using`, tessellated to `OcctMesh` at a tolerance-driven deflection and crossed at the boundary; boolean and sweep modeling returning fresh disposable shapes; value-type `OcctVector3d`/`OcctAxis1d`/`OcctPointCoordinates` inputs; the rail gated on `OcctRuntime.TryGetNativeVersion`.
- Reject: a leaked `OcctShape`/`OcctPoint3d`; a kernel `Rasm` geometry type entering the OCCT ABI; reading the raw `OcctStatus` enum instead of catching `OcctException`; loading this wrapper into an in-Rhino net48 plugin ALC; seeking an OCCT capability the managed wrapper leaves unbound though the native toolkit ships — assembly/XCAF/color/PMI reading, hidden-line removal (the kernel `DrawingProjection` owns HLR), or fillet/chamfer/loft/general B-rep editing.
