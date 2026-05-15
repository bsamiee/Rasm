# [H1][RHINO_GH2_API_LEDGER]
>**Dictum:** *SDK leverage is tracked against the installed RhinoWIP source of truth.*

<br>

[IMPORTANT] Local RhinoWIP `9.0.26132.12306` XML docs are authoritative for implementation choices. Official web links are secondary orientation links when the public site exposes the same RhinoCommon surface.

---
## [1][EVIDENCE]
>**Dictum:** *Every API claim names its source.*

<br>

| [INDEX] | [SOURCE] | [PATH_OR_LINK] | [STATUS] |
| :-----: | -------- | -------------- | :------: |
| **[1]** | RhinoCommon XML | `/Applications/RhinoWIP.app/Contents/Frameworks/RhCore.framework/Versions/A/Resources/RhinoCommon.xml` | Primary |
| **[2]** | GH2 XML | `/Applications/RhinoWIP.app/Contents/Frameworks/RhCore.framework/Versions/A/Resources/ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.xml` | Primary |
| **[3]** | RhinoCommon RTree | [developer.rhino3d.com/api/rhinocommon/rhino.geometry.rtree](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.rtree) | Secondary |
| **[4]** | RhinoCommon Brep | [developer.rhino3d.com/api/rhinocommon/rhino.geometry.brep](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.brep) | Secondary |
| **[5]** | RhinoCommon Mass Properties | [developer.rhino3d.com/api/rhinocommon/rhino.geometry.areamassproperties/compute](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.areamassproperties/compute) | Secondary |

---
## [2][USED]
>**Dictum:** *Used APIs are already part of the foundation contract.*

<br>

| [INDEX] | [API] | [CURRENT_USE] |
| :-----: | ----- | ------------- |
| **[1]** | `IDataAccess.GetPear<T>`, `GetPears<T>`, `GetTree<T>` | Universal `Bridge.Read<T>` reads item, twig, and tree ports with metadata and null state. |
| **[2]** | `IDataAccess.SetPear`, `SetTwig<T>`, `SetTree` | `Bridge.Write<T>` writes GH2-native pears, twigs, and trees instead of item/list emulation. |
| **[3]** | `Leaf<T>`, `Site`, `Garden.TreeFromLeaves<T>` | Tree ports carry branch paths through read and projection; GH2 output prefixing prevents branch merge but is not a sparse `Site.Item` round-trip. |
| **[4]** | `VectorParameter.UnitiseVectors`, `ReverseVectors` | `PortPolicy.Vector` applies native vector collection behavior during parameter construction. |
| **[5]** | `AngleParameter.EnforceKind`, `ReduceAngles` | `PortPolicy.Angle` exposes native angle kind and reduction behavior. |
| **[6]** | `IntegerParameter.IsIndex`, `Indexing` | `Port.Index` configures native index semantics with `IndexModifier.Clip`. |
| **[7]** | `GeometryBase.IsValid`, `GetBoundingBox`, transform-aware bounds | Core analysis uses common `GeometryBase` APIs before type-specific branches. |
| **[8]** | `Brep.TryConvertBrep`, `GetConnectedComponents` | Topology extraction converts native Brep forms, reads connected components first, and duplicates valid single-component Breps only when native components are absent. |
| **[9]** | `RTree.CreateFromPointArray`, `CreatePointCloudTree`, `CreateMeshFaceTree` | Spatial analysis uses native bulk constructors for point, point-cloud, and mesh-face trees. |
| **[10]** | `RTree.SearchOverlaps`, `Point3dKNeighbors`, `Point3dClosestPoints` | Spatial overlap and nearest-neighbor queries use RhinoCommon search APIs directly. |
| **[11]** | `LengthMassProperties.Compute(IEnumerable<Curve>)`, mass-property `Sum(...)` | `Operation<TGeometry,TOut>.Aggregate()` uses native enumerable length computation for pure curve batches and instance summation for mixed itemwise mass properties. |
| **[12]** | `SubD.UpdateSurfaceMeshCache` | SubD topology extraction refreshes native surface mesh cache before mesh-based extraction. |
| **[13]** | `IDataAccess.GetIndex` | Indexed extraction asks GH2 to apply the active `IndexModifier` after candidate count is known. |
| **[14]** | `Mesh.Faces.GetFaceVertices`, `Mesh.FaceNormals.ComputeFaceNormals`, `Mesh.GetNakedEdges` | Mesh topology extraction reads face vertices, normals, and naked edges through native mesh APIs without constructing filtered meshes. |
| **[15]** | `Box(Plane, GeometryBase)` | Oriented bounds now use the native box constructor and keep validity checks on the returned box. |
| **[16]** | `IntersectionEvent.PointA2`, `OverlapA`, `OverlapB` | Curve overlap events now preserve endpoint and interval data on the single `IntersectionHit` model. |
| **[17]** | Boolean Rhino intersection APIs | `CurveBrep`, `CurveBrepFace`, `SurfaceSurface`, `BrepPlane`, `BrepSurface`, and `BrepBrep` now treat `false` as `InvalidResult` unless cancellation applies. |
| **[18]** | `OutputAdder.AddEnum<T>` | Enum outputs route through GH2 native enum output creation via the modular adder's `RegularAdder`; manual preset mutation was removed. |

---
## [3][DEFERRED]
>**Dictum:** *Deferred APIs need an explicit policy or benchmark before integration.*

<br>

| [INDEX] | [API] | [REASON] |
| :-----: | ----- | ----- |
| **[1]** | `AreaMassProperties.Compute(IEnumerable<GeometryBase>)` | Deferred for aggregate area because per-item Brep tolerance arguments must remain identical. |
| **[2]** | `VolumeMassProperties.Compute(IEnumerable<GeometryBase>)` | Deferred for aggregate volume because per-item Brep tolerance arguments must remain identical. |
| **[3]** | `SubD.ToBrep(SubDToBrepOptions.Default)` | Stale as an underused finding; current code does not need SubD-to-Brep conversion beyond existing extraction paths. Promote only when a Brep output requires it. |
| **[4]** | `CurveParameter.NormaliseDomains`, `FlipCurves` | Curve parameter policy is intentionally absent until curve normalization semantics are component-specific. |
| **[5]** | `SurfaceParameter.AcceptMeshes`, `NormaliseDomains`, `FlipSurfaces` | Surface parameter policy is intentionally absent until mesh acceptance and orientation policy are component-specific. |
| **[6]** | `IDataAccess.Verify*` and `Rectify*` methods | Parameter verification remains implicit in GH2; explicit policy-driven rectification belongs at GH UX boundaries, not domain rails. |

---
## [4][INTENTIONALLY_UNUSED]
>**Dictum:** *Unused APIs stay out when they would add ceremony without current capability.*

<br>

| [INDEX] | [API] | [REASON] |
| :-----: | ----- | -------- |
| **[1]** | Full DI composition | Component validation now scans concrete component types directly; no runtime service graph is needed yet. |
| **[2]** | FluentValidation | Domain validation already uses typed `Fin`/`Validation` rails and RhinoCommon validity APIs. |
| **[3]** | BenchmarkDotNet | No measured hot path is being optimized in this slice. |
| **[4]** | GH1 `.gha` APIs | The workspace targets GH2 `.rhp` plugins on RhinoWIP only. |
| **[5]** | RhinoCode publishing APIs | Runtime packaging is Yak-based and driven by `scripts/rhino.sh`. |
| **[6]** | `Mesh.CreateFromFilteredFaceList` | Current mesh face handling projects from the source mesh and face index; filtered mesh construction would add ownership and topology-copying cost. |

---
## [5][IMPLEMENTATION_NOTES]
>**Dictum:** *Foundation code should shrink by moving behavior to native SDK surfaces.*

<br>

- `libs/csharp/Rasm.Grasshopper` models ports as item, twig, or tree data with metadata, null state, and tree path state.
- GH2 imperative calls remain quarantined in boundary adapters such as `Bridge`, `Port`, and component overrides.
- `PortKind.Index` owns native `InputAdder.AddIndex` construction; runtime selection still uses `IDataAccess.GetIndex`.
- `Context` owns tolerances and model units only; unused custom meter-scale state was removed.
- `Operation<TGeometry,TOut>.Aggregate()` lets aggregate-capable queries consume the full input; unsupported aggregate requests fail instead of silently falling back.
- Future query slices should prioritize GH2 tree topology transforms and measured Rhino mass collection overloads before adding local routing code.
