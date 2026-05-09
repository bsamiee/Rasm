# Wave 1 Agent A — Rhino 9/WIP API Capability Sweep

## Methodology

Used `System.Reflection.MetadataLoadContext` against the on-disk Rhino 9/WIP build at `/Applications/RhinoWIP.app/Contents/Frameworks/RhCore.framework/Versions/A/Resources`. The Rhino bundle ships a self-contained .NET runtime, so the resolver was constructed with **Rhino-shipped DLLs first** and host BCL only as fallback for missing simple-names — this avoided the duplicate `System.Drawing.Primitives` collision that vanilla `PathAssemblyResolver` produces. No `Mono.Cecil` fallback was needed.

**Skip list** (excluded as plumbing): `Rhino.UI`, `Rhino.Display`, `Rhino.Render`, `Rhino.PlugIns`, `Rhino.DocObjects`, `Rhino.FileIO`, `Rhino.Commands`, `Rhino.Input`, `Rhino.Runtime`, `Rhino.NodeInCode`, `Rhino.ApplicationSettings`, `Rhino.Collections`, `Rhino.Compute`. From the bare `Rhino` namespace only `RhinoMath` and `RhinoDoc` were retained.

Cross-reference greps used token-bounded regex (`\bMember\b`) over `libs/csharp/` and `apps/grasshopper/` (excluding `bin`/`obj`). 29 noise names (`Equals`, `ToString`, common single-letter members) were filtered to suppress false positives.

## Sweep totals

| Namespace                     | Types reflected | Members reflected | Members touched in repo | Coverage |
| ----------------------------- | --------------: | ----------------: | ----------------------: | -------: |
| `Rhino.Geometry`              |             252 |              4106 |                     727 |    17.7% |
| `Rhino.Geometry.Intersect`    |              15 |               137 |                      53 |    38.7% |
| `Rhino.Geometry.Collections`  |              24 |               399 |                      53 |    13.3% |
| `Rhino` (RhinoMath, RhinoDoc) |               2 |               240 |                      15 |     6.2% |
| `Grasshopper2.*`              |            1846 |             20525 |                    2053 |    10.0% |
| `GrasshopperIO`               |              37 |               491 |                      33 |     6.7% |
| **TOTAL**                     |        **2176** |         **25898** |                **2934** |   ~11.3% |

The 11.3% headline figure overstates "real" coverage because the noise-suppression list cannot fully eliminate name collisions (e.g., `IsValid` appears across hundreds of types but is one repo idiom). Hand-validated through 20 findings below.

## Top-10 highest-leverage unused-API findings (ordered by LOC reduction)

| Rank | Finding | Estimated LOC Δ | Site |
|-----:|---------|---------------:|------|
| 1 | `RTree` callback collection should use immutable `Seq<T>` accumulation, not mutable `int[]`/`SpatialPair[]` `SearchState` (W1A-014) | -32 | Spatial.cs:219 |
| 2 | `SpatialIndex.ValidatePoints` 35-line aggregate replaceable by `Point3d.CullDuplicates` + simple `IsValid` filter (W1A-003) | -32 | Spatial.cs:151 |
| 3 | `ExtractCardinals`/`ExtremeAlongDirection` should funnel through `Curve.ExtremeParameters` + `Point3d.SortAndCullPointList` instead of manual fold (W1A-002) | -26 | Locate.cs:47 |
| 4 | World-cardinal extrema collapse to `BoundingBox.GetCorners` for the canonical 8/4-point shape (W1A-006) | -12 | Locate.cs:62 |
| 5 | `FrameAtCentroid` (W1A-019 + W1A-012) should use `BrepFace.NormalAt` + `BrepFace.PointClosestTo` instead of `AreaMassProperties.Centroid` → `ClosestPoint` → `FrameAt` chain | -9 | Locate.cs:640 |
| 6 | `DedupeCorners` O(n²) `acc.Exists(...DistanceTo)` replaceable by `Point3d.CullDuplicates(values, tolerance)` (W1A-001) | -9 | Measure.cs:55 |
| 7 | `SpatialIndex.DisposeTree` indirection serves no purpose beyond a one-shot bool flip (W1A-013) | -6 | Spatial.cs:146 |
| 8 | `Topology.Adjacency`/`NonManifold` four branches duplicate `Enumerable.Range(0, edges.Count)` — extract a shared `BrepEdgeList`/`MeshTopologyEdgeList` enumerator (W1A-020 + W1A-007) | -8 | Extract.cs:386–450 |
| 9 | `Locate.Dot(point, direction)` reinvents `Vector3d * Vector3d` operator (W1A-005) | -3 | Locate.cs:85 |
| 10 | `BridgeOutput.PointCloud` exists in `SpatialIndex` but no boundary component consumes it — either expose or remove (W1A-004) | 0 | Spatial.cs:23 |

Composed reductions (W1A-002 + W1A-006 + W1A-005) collapse the entire `Locate.cs` cardinal-extraction block (~50 LOC) into roughly 10 LOC of native `BoundingBox.GetCorners` + planar projection. (W1A-001 + W1A-003) standardize point-set hygiene through `CullDuplicates` everywhere it is needed.

## Blind-spot probe results

The orchestration plan listed 12 blind-spot APIs to verify. Results:

| Probe                  | Status      | Replacement (if missing)                               |
| ---------------------- | ----------- | ------------------------------------------------------ |
| `Point3d.EpsilonEquals`| PRESENT     | —                                                      |
| `SortAndCullPoints`    | RENAMED     | `Point3d.SortAndCullPointList(IEnumerable, double)`    |
| `CullDuplicates`       | PRESENT     | —                                                      |
| `RemoveAt`             | PRESENT     | (on `PointCloud`)                                      |
| `MakeAccurateRegions`  | ABSENT      | No equivalent — was a guess; no Rhino9 region API matches. |
| `ProjectPointsToBreps` | PRESENT     | `Intersect.Intersection.ProjectPointsToBreps`          |
| `ProjectPointsToMeshes`| PRESENT     | `Intersect.Intersection.ProjectPointsToMeshes`         |
| `MakeClosed`           | PRESENT     | (on `Curve`)                                           |
| `RawMeshFaceParameters`| ABSENT      | No equivalent; mesh-face metric APIs go via `Mesh.Faces.GetFaceAspectRatio` (already used) |
| `PointCloudKDTree`     | ABSENT      | No `KDTree` exposed; use `RTree.CreatePointCloudTree(PointCloud)` (already wired) |
| `MeshIntersectionCache`| PRESENT     | (already used, Intersect.cs:142)                       |
| `ClosedCurveOrientation`| PRESENT    | (already used, Locate.cs:129)                          |

Three blind-spots are real **non-existent** APIs (`MakeAccurateRegions`, `RawMeshFaceParameters`, `PointCloudKDTree`); one is a name typo (`SortAndCullPoints` → `SortAndCullPointList`). Wave 2 should not propose these.

## Architectural notes

`Rhino.Geometry.Intersect` shows the highest density of repo coverage (38.7%), indicating the intersect dispatch table at `Intersect.cs:27` already drives most of the namespace. By contrast, `Grasshopper2.*` coverage (10.0%) is dominated by parameter and UI types — the boundary surface is small and the absence of usage reflects that, not under-utilization.

The single largest functional-purity violation is `Spatial.cs SearchState` (W1A-014). Replacing it with an `Atom<Seq<T>>` closure satisfies the doctrine and is the only finding that compresses a >30-LOC mutable-buffer block.

## Further Considerations

- `RTree.SearchOverlaps` already accepts a tolerance argument; the existing `Overlaps(other, tolerance)` API correctly threads it. No public boundary component invokes `Overlaps` yet — Wave 2 may want to add one.
- `Curve.ExtremeParameters(Vector3d)` is unique in the API: it returns *all* parameters where the curve is tangent to the given direction. The current `ExtremeAlongDirection` retains only the max/min, but for self-intersecting curves the multi-extrema shape is information-dense and discarding it loses real signal.
- `Mesh.Faces.GetFaceAspectRatio` (already used) is one of ~15 face-metric helpers on `MeshFaceList` — `GetFaceCenter`, `GetFaceNormal`, `GetClashingFacePairs`, `GetFaceVertices`. The dispatch table in `Analyze.MeshFaceMetric` only handles `AspectRatio`; this is an obvious extension axis that costs ~3 LOC per case.
