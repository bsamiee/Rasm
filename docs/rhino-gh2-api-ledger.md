# [H1][RHINO_GH2_API_LEDGER]
>**Dictum:** *SDK leverage is tracked against the installed RhinoWIP source of truth.*

<br>

[IMPORTANT] Local RhinoWIP `9.0.26125.12306` XML docs are authoritative for implementation choices. Official web links are secondary orientation links when the public site exposes the same RhinoCommon surface.

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
| **[1]** | `IDataAccess.GetPear<T>`, `GetPears<T>`, `GetTwig<T>`, `GetTree<T>` | Universal `Bridge.Read<T>` reads item, twig, and tree ports with metadata and null state. |
| **[2]** | `IDataAccess.SetPear`, `SetTwig<T>`, `SetTree` | `Bridge.Write<T>` writes GH2-native pears, twigs, and trees instead of item/list emulation. |
| **[3]** | `Garden.TreeFromPears<T>` | Tree outputs use GH2 pears and `SetTree`; current bridge flattens input tree pears and writes a single output tree from pear data. |
| **[4]** | `VectorParameter.UnitiseVectors`, `ReverseVectors` | `PortPolicy.Vector` applies native vector collection behavior during parameter construction. |
| **[5]** | `AngleParameter.EnforceKind`, `ReduceAngles` | `PortPolicy.Angle` exposes native angle kind and reduction behavior. |
| **[6]** | `CurveParameter.NormaliseDomains`, `FlipCurves` | `PortPolicy.Curve` exposes native curve domain and orientation policy. |
| **[7]** | `SurfaceParameter.AcceptMeshes`, `NormaliseDomains`, `FlipSurfaces` | `PortPolicy.Surface` exposes native surface acceptance and domain policy. |
| **[8]** | `IntegerParameter.IsIndex`, `Indexing` | `Port.Index` configures native index semantics with `IndexModifier.Clip`. |
| **[9]** | `GeometryBase.IsValid`, `GetBoundingBox`, transform-aware bounds | Core analysis uses common `GeometryBase` APIs before type-specific branches. |
| **[10]** | `Brep.TryConvertBrep`, `GetConnectedComponents`, `SplitDisjointPieces` | Topology extraction uses native Brep conversion and component splitting. |
| **[11]** | `RTree.CreateFromPointArray`, `CreatePointCloudTree`, `CreateMeshFaceTree` | Spatial analysis uses native bulk constructors for point, point-cloud, and mesh-face trees. |
| **[12]** | `RTree.SearchOverlaps`, `Point3dKNeighbors`, `Point3dClosestPoints` | Spatial overlap and nearest-neighbor queries use RhinoCommon search APIs directly. |
| **[13]** | `LengthMassProperties.WeightedSum`, `AreaMassProperties.WeightedSum`, `VolumeMassProperties.WeightedSum` | `Query<TGeometry,TOut>.Aggregate()` composes tolerance-aware itemwise mass properties through native mass-property summation. |
| **[14]** | `SubD.UpdateSurfaceMeshCache` | SubD topology extraction refreshes native surface mesh cache before mesh-based extraction. |
| **[15]** | `IDataAccess.GetIndex` | Indexed extraction asks GH2 to apply the active `IndexModifier` after candidate count is known. |

---
## [3][UNDERUSED]
>**Dictum:** *Underused APIs are next refactor targets with known local evidence.*

<br>

| [INDEX] | [API] | [GAP] |
| :-----: | ----- | ----- |
| **[1]** | `AreaMassProperties.Compute(IEnumerable<GeometryBase>)` | Deferred for aggregate area because per-item Brep tolerance arguments must remain identical. |
| **[2]** | `VolumeMassProperties.Compute(IEnumerable<GeometryBase>)` | Deferred for aggregate volume because per-item Brep tolerance arguments must remain identical. |
| **[3]** | `LengthMassProperties.Compute(IEnumerable<Curve>)` | Available for future benchmarking against current tolerance-preserving weighted composition. |
| **[4]** | `Box(Plane, GeometryBase)` | Oriented bounds currently use `GeometryBase.GetBoundingBox(Plane, out Box)`; constructor can simplify some direct box creation flows. |
| **[5]** | `SubD.ToBrep(SubDToBrepOptions)` | SubD conversion is present indirectly; explicit options should be promoted when tolerances or extraordinary vertices matter. |
| **[6]** | `Leaf<T>` and `Garden.TreeFromLeaves<T>` | Full branch topology preservation requires carrying path/site data instead of the current flattened `Seq<Pear<T>>` bridge shape. |
| **[7]** | `IDataAccess.Verify*` and `Rectify*` methods | Parameter verification remains implicit in GH2; explicit policy-driven rectification is not yet wrapped. |

---
## [4][INTENTIONALLY_UNUSED]
>**Dictum:** *Unused APIs stay out when they would add ceremony without current capability.*

<br>

| [INDEX] | [API] | [REASON] |
| :-----: | ----- | -------- |
| **[1]** | Full DI composition | Scrutor is used for component discovery validation only; no runtime service graph is needed yet. |
| **[2]** | FluentValidation | Domain validation already uses typed `Fin`/`Validation` rails and RhinoCommon validity APIs. |
| **[3]** | BenchmarkDotNet | No measured hot path is being optimized in this slice. |
| **[4]** | GH1 `.gha` APIs | The workspace targets GH2 `.rhp` plugins on RhinoWIP only. |
| **[5]** | RhinoCode publishing APIs | Runtime packaging is Yak-based and driven by `scripts/rhino.sh`. |

---
## [5][IMPLEMENTATION_NOTES]
>**Dictum:** *Foundation code should shrink by moving behavior to native SDK surfaces.*

<br>

- `libs/csharp/Rasm.Grasshopper` models ports as item, twig, or tree data with metadata and null state.
- GH2 imperative calls remain quarantined in boundary adapters such as `Bridge`, `Port`, and component overrides.
- `Query<TGeometry,TOut>.Aggregate()` lets aggregate-capable queries consume the full input; unsupported aggregate requests fail instead of silently falling back.
- Future query slices should prioritize GH2 tree topology transforms and measured Rhino mass collection overloads before adding local routing code.
