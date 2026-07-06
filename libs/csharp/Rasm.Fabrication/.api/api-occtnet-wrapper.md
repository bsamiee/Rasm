# [RASM_FABRICATION_API_OCCTNET_WRAPPER]

`OcctNet.Wrapper` is the first managed 3D solid-CAD / B-rep manufacturing-geometry ingress in `Rasm.Fabrication` beyond the 2D `ACadSharp` DXF/DWG path — a thin .NET wrapper over Open CASCADE Technology (OCCT) 7.9.3 through a stable native C ABI bridge. It reads STEP (AP203/AP214/AP242) and IGES into an `OcctShape` B-rep with `OcctFace`/`OcctEdge`/`OcctWire` topology, tessellates the solid to a triangle `OcctMesh`, writes STEP/IGES/STL, models primitives (box/cylinder/sphere/edge/face/wire), runs boolean fuse/cut/common, and applies extrude/revolve/translate with a bounding-box query. The managed surface is COMPACT and value-typed (`OcctVector3d`/`OcctAxis1d`/`OcctBoundingBox`/`OcctPointCoordinates`/`OcctMeshVertex` are `readonly record struct`s); the heavy modeling lives in the bundled native OCCT toolkits. `plan-cs` boundary-maps at the `OcctShape`/`OcctMesh` seam — a kernel `Rasm` geometry type is NEVER passed into the OCCT ABI, and the `OcctMesh` triangle soup crosses to the `Compute`/`geometry3Sharp` mesh owner at the wire.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OcctNet.Wrapper`
- package: `OcctNet.Wrapper`
- version: `0.1.1` (centrally pinned)
- license: wrapper `MIT` (nuspec `<license type="expression">MIT</license>`); the bundled native OCCT 7.9.3 binaries are `LGPL-2.1-with-OCCT-exception-1.0` (dynamic-link clean — the exception waives the LGPL relink obligation for the shipped `.dylib`s, no source disclosure for the consuming application)
- assembly: `OcctNet.Wrapper`
- namespace: `OcctNet.Wrapper`, `.Native`
- managed asset: `lib/net10.0/OcctNet.Wrapper.dll` (pure managed; the nuspec declares an empty `net10.0` dependency group — zero transitive NuGet deps)
- native asset: `runtimes/{osx-arm64,linux-x64,win-x64}/native/` — a REAL osx-arm64 OCCT 7.9.3 build IS present (`libOcctNetNative.dylib` + 170 `libTK*.dylib` OCCT toolkits, versioned `*.7.9.3.dylib`/`*.7.9.dylib`/`*.dylib`); the RID-native asset resolves on the `osx-arm64` consumer. `OcctRuntime.NativeVersion` P/Invokes `GetVersion()` to confirm the loaded native build
- ABI boundary: the managed types are `IDisposable` handle owners (`OcctShape`/`OcctPoint3d` hold `OcctShapeHandle`/`OcctPointHandle` SafeHandles over native memory) — every imported/constructed shape MUST be disposed; the `.Native.NativeMethods` P/Invoke layer + `OcctStatus` return codes are internal plumbing, not a consumer surface
- runtime cost: NOT ALC-safe across an in-Rhino net48 plugin boundary — the native OCCT toolkits are process-global unmanaged libraries; this is a host-neutral net10.0 portable-fabrication owner, loaded once per process, never into a per-plugin AssemblyLoadContext
- rail: fabrication (`Ingress/profile` solid-ingress; the B-rep peer of the 2D `ACadSharp` reader)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: B-rep shape and topology
- rail: fabrication
- note: `OcctShape` is the root B-rep handle; the primitives and topology types all derive `: OcctShape`, so a box, an extrusion, and an imported STEP solid are the same disposable shape type — operations (`Fuse`/`Cut`/`Common`/`Extrude`/`Revolve`/`Translate`/`Triangulate`) are uniform across them.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]   | [CAPABILITY]                                                              |
| :-----: | :--------------- | :-------------- | :----------------------------------------------------------------------- |
|  [01]   | `OcctShape`      | B-rep handle    | `IDisposable` root shape — boolean, extrude/revolve/translate, triangulate, import/export, `IsNull`, `BoundingBox` |
|  [02]   | `OcctFace`       | topology shape  | `: OcctShape` — a planar face built from an `OuterWire` boundary; exposes `OuterWire` |
|  [03]   | `OcctEdge`       | topology shape  | `: OcctShape` — a straight edge between `Start`/`End` `OcctPointCoordinates` |
|  [04]   | `OcctWire`       | topology shape  | `: OcctShape` — an ordered `Edges` chain (`params OcctEdge[]` ctor) forming a closed/open boundary |
|  [05]   | `OcctBox`        | primitive shape | `: OcctShape` — an axis-aligned box (`SizeX`/`SizeY`/`SizeZ`)             |
|  [06]   | `OcctCylinder`   | primitive shape | `: OcctShape` — a cylinder (`Radius`/`Height`)                            |
|  [07]   | `OcctSphere`     | primitive shape | `: OcctShape` — a sphere (`Radius`)                                       |
|  [08]   | `OcctException`  | error type      | `: Exception` carrying the native `StatusCode` (the `OcctStatus` failure surfaced to managed code) |

[PUBLIC_TYPE_SCOPE]: mesh and value types
- rail: fabrication
- note: `OcctMesh` is the tessellation result — an immutable indexed triangle mesh; the value types are `readonly record struct`s carrying static well-known anchors (`OcctVector3d.Zero`, `OcctAxis1d.{X,Y,Z}Axis`).

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]   | [CAPABILITY]                                                              |
| :-----: | :----------------------- | :-------------- | :----------------------------------------------------------------------- |
|  [01]   | `OcctMesh`               | mesh result     | `sealed` — `Vertices : IReadOnlyList<OcctMeshVertex>`, `TriangleIndices : IReadOnlyList<int>`, `TriangleCount`; `FromShape(OcctShape, linearDeflection, angularDeflection)` |
|  [02]   | `OcctMeshVertex`         | record struct   | `(double X, double Y, double Z)` triangle vertex                          |
|  [03]   | `OcctVector3d`           | record struct   | `(double X, double Y, double Z)` — `Zero`, `Length`; the extrude/translate direction |
|  [04]   | `OcctPointCoordinates`   | record struct   | `(double X, double Y, double Z)` — the value-type point (edge endpoints, axis origin) |
|  [05]   | `OcctAxis1d`             | record struct   | `(OcctPointCoordinates Origin, OcctVector3d Direction)` — `XAxis`/`YAxis`/`ZAxis`; the revolve axis |
|  [06]   | `OcctBoundingBox`        | record struct   | `(MinX, MinY, MinZ, MaxX, MaxY, MaxZ)` — `SizeX`/`SizeY`/`SizeZ`; the `OcctShape.BoundingBox` |
|  [07]   | `OcctPoint3d`            | handle point    | `IDisposable` native point handle — `X`/`Y`/`Z`, `Coordinates`, `Origin()`, `SetCoordinates`, `DistanceTo` (distinct from the value-type `OcctPointCoordinates`) |
|  [08]   | `OcctRuntime`            | runtime probe   | `static` — `NativeVersion` (P/Invoke `GetVersion()`), `TryGetNativeVersion(out version, out error)` native-load confirmation |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: import / export — `OcctShape` (static read, instance write)
- rail: fabrication
- note: import is the manufacturing-geometry ingress (STEP/IGES B-rep, STL mesh-as-shape); export round-trips the shape back to a neutral CAD file. ImportStep reads a SINGLE shape — the package surfaces no assembly/XCAF document reader.

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `OcctShape.ImportStep(string filePath)`    | static import  | read STEP AP203/AP214/AP242 → `OcctShape` B-rep        |
|  [02]   | `OcctShape.ImportIges(string filePath)`    | static import  | read IGES → `OcctShape` B-rep                           |
|  [03]   | `OcctShape.ImportStl(string filePath)`     | static import  | read STL → `OcctShape` (mesh-as-shape)                  |
|  [04]   | `shape.ExportStep(string filePath)`        | export         | write the shape to STEP                                 |
|  [05]   | `shape.ExportIges(string filePath)`        | export         | write the shape to IGES                                 |
|  [06]   | `shape.ExportStl(string filePath, bool ascii = false)` | export | write the shape to binary (default) or ASCII STL    |

[ENTRYPOINT_SCOPE]: primitive construction and topology
- rail: fabrication

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `new OcctBox(double sizeX, double sizeY, double sizeZ)` | primitive     | an axis-aligned box solid                               |
|  [02]   | `new OcctCylinder(double radius, double height)`       | primitive     | a cylinder solid                                        |
|  [03]   | `new OcctSphere(double radius)`                        | primitive     | a sphere solid                                          |
|  [04]   | `new OcctEdge(OcctPointCoordinates start, OcctPointCoordinates end)` | topology | a straight edge; `Start`/`End` read back the endpoints |
|  [05]   | `new OcctWire(params OcctEdge[] edges)`                | topology       | an ordered edge chain; `Edges` reads back the chain     |
|  [06]   | `new OcctFace(OcctWire outerWire)`                     | topology       | a face from a boundary wire; `OuterWire` reads it back   |
|  [07]   | `new OcctPoint3d(double x, double y, double z)` / `OcctPoint3d.Origin()` | point handle | a native point handle (`DistanceTo`, `SetCoordinates`) |

[ENTRYPOINT_SCOPE]: modeling operations — `OcctShape` instance
- rail: fabrication
- note: every operation returns a NEW `OcctShape` (the source is unmodified, each result disposable); boolean and sweep are the manufacturing-modeling primitives.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `shape.Fuse(OcctShape other)`                          | boolean        | union (BRepAlgoAPI_Fuse)                                |
|  [02]   | `shape.Cut(OcctShape other)`                           | boolean        | difference (BRepAlgoAPI_Cut)                            |
|  [03]   | `shape.Common(OcctShape other)`                        | boolean        | intersection (BRepAlgoAPI_Common)                       |
|  [04]   | `shape.Extrude(OcctVector3d vector)`                   | sweep          | linear extrude a face/wire along a vector               |
|  [05]   | `shape.Revolve(OcctAxis1d axis, double angleRadians = 2π)` | sweep      | revolve a profile about an axis (default full turn)     |
|  [06]   | `shape.Translate(OcctVector3d vector)`                 | transform      | rigid translation                                       |
|  [07]   | `shape.BoundingBox` / `shape.IsNull`                   | query          | the `OcctBoundingBox` extents / null-shape guard        |

[ENTRYPOINT_SCOPE]: tessellation — `OcctShape` → `OcctMesh`
- rail: fabrication
- note: tessellation is the bridge from the exact B-rep to a triangle mesh the rest of the fabrication/compute stack consumes; the two deflection knobs trade triangle count against chord/angle accuracy (smaller = finer).

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `shape.Triangulate(double linearDeflection = 0.1, double angularDeflection = 0.5)` → `OcctMesh` | tessellate | incremental-mesh the B-rep to triangles |
|  [02]   | `OcctMesh.FromShape(OcctShape, double linearDeflection = 0.1, double angularDeflection = 0.5)` | tessellate | the static-factory mirror of `Triangulate` |
|  [03]   | `mesh.Vertices` / `mesh.TriangleIndices` / `mesh.TriangleCount` | mesh read | the indexed triangle soup (flat `int` index triples) |
|  [04]   | `new OcctMesh(IReadOnlyList<OcctMeshVertex> vertices, IReadOnlyList<int> triangleIndices)` | construct | wrap an externally-built triangle mesh |

[ENTRYPOINT_SCOPE]: native runtime probe — `OcctRuntime`
- rail: fabrication

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `OcctRuntime.NativeVersion`                            | probe          | the loaded native OCCT version (`GetVersion()` P/Invoke) — confirms the osx-arm64 native asset resolved |
|  [02]   | `OcctRuntime.TryGetNativeVersion(out string version, out string? error)` | probe | the non-throwing native-load check the boundary runs at startup |

## [04]-[IMPLEMENTATION_LAW]

[ABI_TOPOLOGY]:
- `OcctShape` and `OcctPoint3d` own native unmanaged handles (`OcctShapeHandle`/`OcctPointHandle` SafeHandles) and are `IDisposable` — wrap EVERY shape (imported, constructed, or operation-result) in `using`, and treat a boolean/sweep/translate result as a fresh disposable shape distinct from its operands; a leaked shape leaks native OCCT memory
- the value types (`OcctVector3d`/`OcctPointCoordinates`/`OcctAxis1d`/`OcctBoundingBox`/`OcctMeshVertex`) are `readonly record struct`s — pure managed, copyable, not disposable; `OcctPoint3d` is the DISTINCT native handle point (use the value-type `OcctPointCoordinates` for edge endpoints and axis origins, reserve `OcctPoint3d` for the rare native-distance query)
- a native failure surfaces as `OcctException` carrying the `StatusCode`; the `.Native.OcctStatus` codes and `NativeMethods` P/Invokes are internal — a consumer catches `OcctException` and lowers it to the fabrication fault rail, never reads the raw status enum
- the osx-arm64 native asset MUST resolve at load (`runtimes/osx-arm64/native/libOcctNetNative.dylib` + its 170 OCCT toolkit `.dylib`s) — gate startup on `OcctRuntime.TryGetNativeVersion` and lower a load failure to a typed capability-miss rather than letting a `DllNotFoundException` escape

[SCOPE_LIMIT]:
- SINGLE-SHAPE import only — `ImportStep`/`ImportIges` return ONE `OcctShape`; the package surfaces NO assembly/document reader, so STEP color/material/PMI and the assembly tree (the OCCT TKXCAF / XDE document) are UNREACHABLE from managed code even though `libTKXCAF.dylib` and `libTKBinXCAF.dylib` ship in the native bundle — the managed wrapper binds no entry to them. This is the STANDING DEMAND ROW: assembly-tree + AP242 PMI surfacing is durable wrapper pressure (a future managed-binding lane), and it is the `Spec/` plane's secondary GD&T source once bound
- NO hidden-line removal — `libTKHLR.dylib` ships in the osx-arm64 native bundle but is MANAGED-UNBOUND (no `OcctShape.HiddenLine` / projection entry exists); the HLR projection is the `Documentation/projection` kernel `DrawingProjection` consumer (`Drawing/view.md#View.Apply`), NOT delegated to OCCT
- NO general B-rep editing — the managed surface is import/export + primitives + boolean + linear-extrude/revolve/translate + tessellate; fillet/chamfer/loft/sweep-along-spine, face/edge filleting, and parametric features are absent (the native toolkits exist but are unbound)
- the `OcctEdge` is straight-only (point-to-point); there is no arc/spline edge constructor in the managed surface — a curved profile is imported (STEP/IGES) or approximated, never authored edge-by-edge with curvature

[LOCAL_ADMISSION]:
- the solid-ingress entry is `OcctShape.ImportStep(path)` / `ImportIges(path)` under a `using`, the imported B-rep tessellated through `shape.Triangulate(linearDeflection, angularDeflection)` to an `OcctMesh`, the deflection chosen by the fabrication tolerance (a finer linear deflection for a small precision part); the `OcctMesh` triangle soup is the cross-seam payload, never the live `OcctShape` handle
- boolean stock-removal modeling (rough-stock minus the part, a fixture-clearance cut) is `Fuse`/`Cut`/`Common` returning fresh disposable shapes; extrude/revolve build a solid from a profile face for the stock or a swept feature
- the `BoundingBox` extents drive stock sizing and the `Nesting`/`Workholding` keep-out envelope; read it from `shape.BoundingBox` rather than folding the mesh vertices
- gate the whole rail on `OcctRuntime.TryGetNativeVersion(out v, out err)` at first use and surface `err` as a capability-miss fault — the rest of the portable-fabrication owner degrades gracefully when the native asset is absent

[INTEGRATION_STACK]:
- mesh seam: the `OcctMesh` (`Vertices`/`TriangleIndices`) crosses at the boundary to the kernel `MeshSpace` vocabulary (`Meshing/mesh.md`), dirty STL routed through the kernel predicate-gated `HealOp` (`Processing/repair.md`, K15) — a STEP-imported solid flows `OcctShape.ImportStep` → `Triangulate` → `OcctMesh` → kernel `MeshSpace` for the planar-section (kernel `Meshing/slice` slice-stack, K3) and HLR projection (`Documentation/projection` kernel `DrawingProjection` consumer, K6) rails; the OCCT handle never crosses, only the admitted mesh
- 2D ingress complement: this is the 3D B-rep peer of the 2D `ACadSharp` DXF/DWG reader (`api-acadsharp`) — `ACadSharp` owns 2D profile ingress (closed polylines, arcs → `Loop`), `OcctNet.Wrapper` owns 3D solid ingress (STEP/IGES → B-rep → mesh); a part arriving as a 3D STEP solid is tessellated then planar-sectioned to 2D loops, a part arriving as a 2D DXF profile goes straight to `Loop` — neither duplicates the other
- arc-offset seam: a planar section of the tessellated `OcctMesh` produces line-only contours (the mesh has no curvature); when the section needs arc-comp toolpaths it is re-fit through the owned Bolton biarc fold (`Posting/program`, line-sourced chains only) or carried as bulge into `CavalierContours` (`api-cavaliercontours`) — the OCCT path is line-sourced, so the biarc refit applies here (unlike a natively-bulge `ACadSharp` arc)
- kernel atoms: `OcctVector3d`/`OcctPointCoordinates`/`OcctBoundingBox` map at the boundary to the kernel `Rasm` `Vector3d`/`Point3d`/box — a kernel geometry type is NEVER passed INTO the OCCT ABI (the named seam rule); the conversion is one-directional at the `OcctShape`/`OcctMesh` boundary
- persistence: a STEP/IGES export (`ExportStep`/`ExportStl`) mints its content key through the kernel `ContentHash.Of` egress (the ONE mint, K9) as a content-addressed durable artifact alongside the `CutProgram` AST; the durable-row fold is the Fabrication-authored demand on the held-open `[ARTIFACT_CONTENT_KEY_FEDERATION]` blocker, never a composed Persistence contract

[RAIL_LAW]:
- Package: `OcctNet.Wrapper` (0.1.1; wrapper MIT, native OCCT 7.9.3 LGPL-2.1-with-OCCT-exception-1.0 dynamic-link; `lib/net10.0` managed + real `runtimes/osx-arm64/native` OCCT build, NOT ALC-safe, native handles `IDisposable`)
- Owns: STEP/IGES/STL read into an `OcctShape` B-rep, B-rep → `OcctMesh` tessellation, STEP/IGES/STL write, primitive construction (box/cylinder/sphere/edge/wire/face), boolean fuse/cut/common, linear-extrude/revolve/translate, bounding-box query, and native-version probe
- Accept: a single imported `OcctShape` under `using`, tessellated to `OcctMesh` at a tolerance-driven deflection, the mesh crossed at the boundary; boolean/sweep modeling returning fresh disposable shapes; the value-type `OcctVector3d`/`OcctAxis1d`/`OcctPointCoordinates` for direction/axis/point inputs; the rail gated on `OcctRuntime.TryGetNativeVersion`
- Reject: a leaked (undisposed) `OcctShape`/`OcctPoint3d`; passing a kernel `Rasm` geometry type into the OCCT ABI; seeking an assembly/XCAF/color/PMI reader from this package (managed-unbound — `Posting`/`Bim` own that elsewhere or it is out of scope); seeking HLR from OCCT (`libTKHLR` ships but is unbound — `Documentation/projection` owns HLR as the kernel `DrawingProjection` consumer); reading the raw `.Native.OcctStatus` enum instead of catching `OcctException`; loading this wrapper into an in-Rhino net48 plugin ALC (the native toolkits are process-global)
