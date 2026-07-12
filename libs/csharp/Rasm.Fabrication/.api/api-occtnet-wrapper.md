# [RASM_FABRICATION_API_OCCTNET_WRAPPER]

`OcctNet.Wrapper` is the first managed 3D solid-CAD / B-rep manufacturing-geometry ingress in `Rasm.Fabrication` beyond the 2D `ACadSharp` DXF/DWG path — a thin.NET wrapper over Open CASCADE Technology (OCCT) through a stable native C ABI bridge. It reads STEP (AP203/AP214/AP242) and IGES into an `OcctShape` B-rep with `OcctFace`/`OcctEdge`/`OcctWire` topology, tessellates the solid to a triangle `OcctMesh`, writes STEP/IGES/STL, models primitives (box/cylinder/sphere/edge/face/wire), runs boolean fuse/cut/common, and applies extrude/revolve/translate with a bounding-box query. The managed surface is COMPACT and value-typed (`OcctVector3d`/`OcctAxis1d`/`OcctBoundingBox`/`OcctPointCoordinates`/`OcctMeshVertex` are `readonly record struct`s); the heavy modeling lives in the bundled native OCCT toolkits. `plan-cs` boundary-maps at the `OcctShape`/`OcctMesh` seam — a kernel `Rasm` geometry type is NEVER passed into the OCCT ABI, and the `OcctMesh` triangle soup crosses to the `Compute`/`geometry3Sharp` mesh owner at the wire.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OcctNet.Wrapper`
- package: `OcctNet.Wrapper`
- license: wrapper `MIT` (nuspec `<license type="expression">MIT</license>`); the bundled native OCCT binaries are `LGPL-2.1-with-OCCT-exception-1.0` (dynamic-link clean — the exception waives the LGPL relink obligation for the shipped `.dylib`s, no source disclosure for the consuming application)
- assembly: `OcctNet.Wrapper`
- namespace: `OcctNet.Wrapper`, `.Native`
- managed asset: `lib/net10.0/OcctNet.Wrapper.dll` (pure managed; the nuspec declares an empty `net10.0` dependency group — zero transitive NuGet deps)
- native asset: `runtimes/{osx-arm64,linux-x64,win-x64}/native/` — a REAL osx-arm64 OCCT build IS present (`libOcctNetNative.dylib` + 170 `libTK*.dylib` OCCT toolkits, versioned `*..dylib`/`*.7.9.dylib`/`*.dylib`); the RID-native asset resolves on the `osx-arm64` consumer. `OcctRuntime.NativeVersion` P/Invokes `GetVersion` to confirm the loaded native build
- ABI boundary: the managed types are `IDisposable` handle owners (`OcctShape`/`OcctPoint3d` hold `OcctShapeHandle`/`OcctPointHandle` SafeHandles over native memory) — every imported/constructed shape MUST be disposed; the `.Native.NativeMethods` P/Invoke layer + `OcctStatus` return codes are internal plumbing, not a consumer surface
- runtime cost: NOT ALC-safe across an in-Rhino net48 plugin boundary — the native OCCT toolkits are process-global unmanaged libraries; this is a host-neutral net10.0 portable-fabrication owner, loaded once per process, never into a per-plugin AssemblyLoadContext
- rail: fabrication (`Ingress/profile` solid-ingress; the B-rep peer of the 2D `ACadSharp` reader)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: B-rep shape and topology
- rail: fabrication
- note: `OcctShape` is the disposable B-rep root for uniform boolean, sweep, transform, and tessellation operations. Every primitive and topology type derives from it, and `OcctException` carries the native `StatusCode` into managed code.

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
- rail: fabrication
- note: `OcctMesh` carries immutable `Vertices`, `TriangleIndices`, and `TriangleCount`, and `FromShape` constructs the tessellation result. Every value type is a `readonly record struct`; `OcctVector3d.Zero` and the `OcctAxis1d` axis members are canonical anchors.
- point forms: `OcctPointCoordinates` carries edge endpoints and axis origins by value. `OcctPoint3d` owns the disposable native handle with `Coordinates`, `Origin()`, `SetCoordinates`, and `DistanceTo`.

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

[ENTRYPOINT_SCOPE]: import / export — `OcctShape` (static read, instance write)
- rail: fabrication
- note: import is the manufacturing-geometry ingress (STEP/IGES B-rep, STL mesh-as-shape); export round-trips the shape back to a neutral CAD file. ImportStep reads a SINGLE shape — the package surfaces no assembly/XCAF document reader.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `OcctShape.ImportStep(string filePath)`                | static import  | read STEP AP203/AP214/AP242 → `OcctShape` B-rep  |
|  [02]   | `OcctShape.ImportIges(string filePath)`                | static import  | read IGES → `OcctShape` B-rep                    |
|  [03]   | `OcctShape.ImportStl(string filePath)`                 | static import  | read STL → `OcctShape` (mesh-as-shape)           |
|  [04]   | `shape.ExportStep(string filePath)`                    | export         | write the shape to STEP                          |
|  [05]   | `shape.ExportIges(string filePath)`                    | export         | write the shape to IGES                          |
|  [06]   | `shape.ExportStl(string filePath, bool ascii = false)` | export         | write the shape to binary (default) or ASCII STL |

[ENTRYPOINT_SCOPE]: primitive construction and topology
- rail: fabrication
- note: constructed edges expose `Start` and `End`; wires and faces expose `Edges` and `OuterWire`. `OcctPoint3d` carries `DistanceTo` and `SetCoordinates`.

| [INDEX] | [SURFACE]                                                            | [KIND]       | [CAPABILITY]       |
| :-----: | :------------------------------------------------------------------- | :----------- | :----------------- |
|  [01]   | `new OcctBox(double sizeX, double sizeY, double sizeZ)`              | primitive    | axis-aligned solid |
|  [02]   | `new OcctCylinder(double radius, double height)`                     | primitive    | cylinder solid     |
|  [03]   | `new OcctSphere(double radius)`                                      | primitive    | sphere solid       |
|  [04]   | `new OcctEdge(OcctPointCoordinates start, OcctPointCoordinates end)` | topology     | straight edge      |
|  [05]   | `new OcctWire(params OcctEdge[] edges)`                              | topology     | ordered edge chain |
|  [06]   | `new OcctFace(OcctWire outerWire)`                                   | topology     | boundary face      |
|  [07]   | `new OcctPoint3d(double x, double y, double z)`                      | point handle | coordinate point   |
|  [08]   | `OcctPoint3d.Origin()`                                               | point handle | origin point       |

[ENTRYPOINT_SCOPE]: modeling operations — `OcctShape` instance
- rail: fabrication
- note: every operation returns a NEW `OcctShape` (the source is unmodified, each result disposable); boolean and sweep are the manufacturing-modeling primitives.

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `shape.Fuse(OcctShape other)`                              | boolean        | union (BRepAlgoAPI_Fuse)                            |
|  [02]   | `shape.Cut(OcctShape other)`                               | boolean        | difference (BRepAlgoAPI_Cut)                        |
|  [03]   | `shape.Common(OcctShape other)`                            | boolean        | intersection (BRepAlgoAPI_Common)                   |
|  [04]   | `shape.Extrude(OcctVector3d vector)`                       | sweep          | linear extrude a face/wire along a vector           |
|  [05]   | `shape.Revolve(OcctAxis1d axis, double angleRadians = 2π)` | sweep          | revolve a profile about an axis (default full turn) |
|  [06]   | `shape.Translate(OcctVector3d vector)`                     | transform      | rigid translation                                   |
|  [07]   | `shape.BoundingBox` / `shape.IsNull`                       | query          | the `OcctBoundingBox` extents / null-shape guard    |

[ENTRYPOINT_SCOPE]: tessellation — `OcctShape` → `OcctMesh`
- rail: fabrication
- note: tessellation bridges the exact B-rep to an indexed triangle mesh. Both tessellation entrypoints default `linearDeflection` to `0.1` and `angularDeflection` to `0.5`; smaller values refine chord and angle accuracy.
- mesh layout: `TriangleIndices` carries flat index triples into `Vertices`, and `TriangleCount` reports the resulting face count.

| [INDEX] | [SURFACE]                                                                                  | [KIND]     | [CAPABILITY]          |
| :-----: | :----------------------------------------------------------------------------------------- | :--------- | :-------------------- |
|  [01]   | `shape.Triangulate(linearDeflection, angularDeflection)` → `OcctMesh`                      | tessellate | instance tessellation |
|  [02]   | `OcctMesh.FromShape(OcctShape, linearDeflection, angularDeflection)`                       | tessellate | static tessellation   |
|  [03]   | `mesh.Vertices`                                                                            | mesh read  | triangle vertices     |
|  [04]   | `mesh.TriangleIndices`                                                                     | mesh read  | flat index triples    |
|  [05]   | `mesh.TriangleCount`                                                                       | mesh read  | triangle count        |
|  [06]   | `new OcctMesh(IReadOnlyList<OcctMeshVertex> vertices, IReadOnlyList<int> triangleIndices)` | construct  | external indexed mesh |

[ENTRYPOINT_SCOPE]: native runtime probe — `OcctRuntime`
- rail: fabrication
- note: `NativeVersion` P/Invokes `GetVersion()` and confirms native asset resolution. `TryGetNativeVersion` returns the version or load error without throwing for the startup gate.

| [INDEX] | [SURFACE]                                                                | [KIND] | [CAPABILITY]      |
| :-----: | :----------------------------------------------------------------------- | :----- | :---------------- |
|  [01]   | `OcctRuntime.NativeVersion`                                              | probe  | loaded version    |
|  [02]   | `OcctRuntime.TryGetNativeVersion(out string version, out string? error)` | probe  | native-load check |

## [04]-[IMPLEMENTATION_LAW]

[ABI_TOPOLOGY]:
- `OcctShape` and `OcctPoint3d` own native unmanaged handles (`OcctShapeHandle`/`OcctPointHandle` SafeHandles) and are `IDisposable` — wrap EVERY shape (imported, constructed, or operation-result) in `using`, and treat a boolean/sweep/translate result as a fresh disposable shape distinct from its operands; a leaked shape leaks native OCCT memory
- the value types (`OcctVector3d`/`OcctPointCoordinates`/`OcctAxis1d`/`OcctBoundingBox`/`OcctMeshVertex`) are `readonly record struct`s — pure managed, copyable, not disposable; `OcctPoint3d` is the DISTINCT native handle point (use the value-type `OcctPointCoordinates` for edge endpoints and axis origins, reserve `OcctPoint3d` for the rare native-distance query)
- a native failure surfaces as `OcctException` carrying the `StatusCode`; the `.Native.OcctStatus` codes and `NativeMethods` P/Invokes are internal — a consumer catches `OcctException` and lowers it to the fabrication fault rail, never reads the raw status enum
- the osx-arm64 native asset MUST resolve at load (`runtimes/osx-arm64/native/libOcctNetNative.dylib` + its 170 OCCT toolkit `.dylib`s) — gate startup on `OcctRuntime.TryGetNativeVersion` and lower a load failure to a typed capability-miss rather than letting a `DllNotFoundException` escape

[SCOPE_LIMIT]:
- SINGLE-SHAPE import only — `ImportStep` and `ImportIges` return one `OcctShape`; the managed wrapper binds no assembly/XCAF document reader for STEP color, material, PMI, or assembly trees despite the bundled `libTKXCAF.dylib` and `libTKBinXCAF.dylib` assets
- assembly-tree and AP242 PMI surfaces remain wrapper-binding pressure and become the `Spec/` plane's secondary GD&T source once bound
- NO hidden-line removal — `libTKHLR.dylib` ships in the osx-arm64 native bundle but is managed-unbound because no projection entry exists; the `DrawingProjection` consumer owns HLR
- NO general B-rep editing — the managed surface is import/export + primitives + boolean + linear-extrude/revolve/translate + tessellate; fillet/chamfer/loft/sweep-along-spine, face/edge filleting, and parametric features are absent (the native toolkits exist but are unbound)
- the `OcctEdge` is straight-only (point-to-point); there is no arc/spline edge constructor in the managed surface — a curved profile is imported (STEP/IGES) or approximated, never authored edge-by-edge with curvature

[LOCAL_ADMISSION]:
- the solid-ingress entry is `OcctShape.ImportStep(path)` / `ImportIges(path)` under a `using`, the imported B-rep tessellated through `shape.Triangulate(linearDeflection, angularDeflection)` to an `OcctMesh`, the deflection chosen by the fabrication tolerance (a finer linear deflection for a small precision part); the `OcctMesh` triangle soup is the cross-seam payload, never the live `OcctShape` handle
- boolean stock-removal modeling (rough-stock minus the part, a fixture-clearance cut) is `Fuse`/`Cut`/`Common` returning fresh disposable shapes; extrude/revolve build a solid from a profile face for the stock or a swept feature
- the `BoundingBox` extents drive stock sizing and the `Nesting`/`Workholding` keep-out envelope; read it from `shape.BoundingBox` rather than folding the mesh vertices
- gate the whole rail on `OcctRuntime.TryGetNativeVersion(out v, out err)` at first use and surface `err` as a capability-miss fault — the rest of the portable-fabrication owner degrades gracefully when the native asset is absent

[INTEGRATION_STACK]:
- mesh seam: `OcctMesh` crosses through `Vertices` and `TriangleIndices` to the kernel `MeshSpace` vocabulary, while dirty STL routes through the predicate-gated `HealOp`
- mesh consumers: a STEP-imported solid flows `OcctShape.ImportStep` → `Triangulate` → `OcctMesh` → `MeshSpace` for planar-section and `DrawingProjection` rails; only the admitted mesh crosses the boundary
- 2D ingress complement: this is the 3D B-rep peer of the 2D `ACadSharp` DXF/DWG reader (`api-acadsharp`) — `ACadSharp` owns 2D profile ingress (closed polylines, arcs → `Loop`), `OcctNet.Wrapper` owns 3D solid ingress (STEP/IGES → B-rep → mesh); a part arriving as a 3D STEP solid is tessellated then planar-sectioned to 2D loops, a part arriving as a 2D DXF profile goes straight to `Loop` — neither duplicates the other
- arc-offset seam: a planar section of the tessellated `OcctMesh` produces line-only contours (the mesh has no curvature); when the section needs arc-comp toolpaths it is re-fit through the owned Bolton biarc fold (`Posting/program`, line-sourced chains only) or carried as bulge into `CavalierContours` (`api-cavaliercontours`) — the OCCT path is line-sourced, so the biarc refit applies here (unlike a natively-bulge `ACadSharp` arc)
- kernel atoms: `OcctVector3d`/`OcctPointCoordinates`/`OcctBoundingBox` map at the boundary to the kernel `Rasm` `Vector3d`/`Point3d`/box — a kernel geometry type is NEVER passed INTO the OCCT ABI (the named seam rule); the conversion is one-directional at the `OcctShape`/`OcctMesh` boundary
- persistence: a STEP/IGES/STL export (`ExportStep`/`ExportStl`) mints through `ContentKey.Of(EgressKind, ReadOnlySpan<byte>)` into kernel `ContentHash.Of`; artifact enrollment lands in the Persistence artifact index, never a local hasher or a schema-local content-key fork

[RAIL_LAW]:
- Package: `OcctNet.Wrapper` (wrapper MIT, native OCCT LGPL-2.1-with-OCCT-exception-1.0 dynamic-link; `lib/net10.0` managed + real `runtimes/osx-arm64/native` OCCT build, NOT ALC-safe, native handles `IDisposable`)
- Owns: STEP/IGES/STL read into an `OcctShape` B-rep, B-rep → `OcctMesh` tessellation, STEP/IGES/STL write, primitive construction (box/cylinder/sphere/edge/wire/face), boolean fuse/cut/common, linear-extrude/revolve/translate, bounding-box query, and native-version probe
- Accept: a single imported `OcctShape` under `using`, tessellated to `OcctMesh` at a tolerance-driven deflection, the mesh crossed at the boundary; boolean/sweep modeling returning fresh disposable shapes; the value-type `OcctVector3d`/`OcctAxis1d`/`OcctPointCoordinates` for direction/axis/point inputs; the rail gated on `OcctRuntime.TryGetNativeVersion`
- Reject: a leaked (undisposed) `OcctShape`/`OcctPoint3d`; passing a kernel `Rasm` geometry type into the OCCT ABI; seeking an assembly/XCAF/color/PMI reader from this package (managed-unbound — `Posting`/`Bim` own that elsewhere or it is out of scope); seeking HLR from OCCT (`libTKHLR` ships but is unbound — `Documentation/projection` owns HLR as the kernel `DrawingProjection` consumer); reading the raw `.Native.OcctStatus` enum instead of catching `OcctException`; loading this wrapper into an in-Rhino net48 plugin ALC (the native toolkits are process-global)
