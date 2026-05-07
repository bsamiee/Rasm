# [H1][RHINO_ANALYSIS_HANDOFF]
>**Dictum:** *Use the archive as vocabulary; let modern typed analysis own the API.*

<br>

[IMPORTANT] Treat this file as the living roadmap/source of truth for `libs/csharp/analysis` and its relevant `core/Domain` support. Do not use chat history as roadmap state.

[IMPORTANT] The current completed branch is `analysis-curve-deviation`. Do not reselect items marked `DONE` or `REJECTED`.

---
## [1][CURRENT_TRUTH]
>**Dictum:** *Start from current repository evidence.*

<br>

**Current branch:** `main`.

**Modern analysis LOC:** `libs/csharp/analysis/*.cs` is 2,597 LOC.

| [INDEX] | [FILE] | [LOC] | [ROLE] |
| :-----: | ------ | ----: | ------ |
| **1** | `libs/csharp/analysis/Analyze.cs` | 131 | Per-input execution surface, scoped context, null/error rails. |
| **2** | `libs/csharp/analysis/Query.cs` | 491 | Descriptor algebra and public vocabulary: bounds, measure, location, topology, conformance, mesh face metrics, spatial hit/pair records, curve deviation evidence. |
| **3** | `libs/csharp/analysis/Measure.cs` | 455 | Bounds, scalar/mass projections, explicit conformance residual/profile/maximum evidence. |
| **4** | `libs/csharp/analysis/Locate.cs` | 545 | Point/frame/normal/curvature/location queries and normalized profile projections. |
| **5** | `libs/csharp/analysis/Extract.cs` | 407 | Primitive extraction, read-only topology, mesh diagnostics, mesh check counts, mesh face aspect-ratio samples. |
| **6** | `libs/csharp/analysis/Intersect.cs` | 308 | Pair intersections, compact intersection classification, native curve deviation, native pair rails. |
| **7** | `libs/csharp/analysis/Spatial.cs` | 256 | Focused collection-level Rhino `RTree` owner. |

**Relevant core LOC:** `libs/csharp/core/Domain/*.cs` is 773 LOC.

| [INDEX] | [FILE] | [LOC] | [ROLE] |
| :-----: | ------ | ----: | ------ |
| **1** | `libs/csharp/core/Domain/GeometryContext.cs` | 204 | Units, tolerances, context construction, mesh-intersection tolerance. |
| **2** | `libs/csharp/core/Domain/GeometryValidation.cs` | 465 | Geometry readiness checks and native operand validation, including `Line`, `Plane`, `Circle`, `Arc`, and `Sphere`. |
| **3** | `libs/csharp/core/Domain/Operation.cs` | 104 | Operation keys, typed faults, output validation. |

**Archive role:** `.archive/libs/rhino/` is only a capability catalogue. Do not port archive request/result records, config tables, workflow facades, compatibility layers, helper folders, score bags, or handrolled algorithms.

---
## [2][PUBLIC_API_SURFACE]
>**Dictum:** *Public capability is compact typed evidence, not workflow machinery.*

<br>

**Core execution remains unchanged:**

- `Analyze.Run(query, input)` and `Analyze.In(...).Run(query, input)` remain per-input query execution.
- `Query<TGeometry,TOut>` remains the scalar/pair geometry descriptor owner.
- Collection-level spatial indexing is deliberately outside `Analyze.Run` because `RTree` owns a collection and has disposable native lifecycle.

**Current public analysis vocabulary includes:**

- `Bounds`, `Measure`, `Location`, `Topology`, `Conformance`, `Deviation`, `MassKind`, `CurvatureScalar`, `MeshCheckCount`, `MeshFaceMetric`, `ConformanceResidual`, `DeviationKind`, `IntersectionKind`.
- Compact records: `CurvatureProfile`, `ResidualProfile`, `ResidualSample`, `MeshFaceSample`, `SpatialHit`, `SpatialPair`, `CurveDeviation`.
- Spatial owner: `SpatialIndex`.

**Spatial API shape:**

- `SpatialIndex.Points(ReadOnlySpan<Point3d>)`
- `SpatialIndex.PointCloud(PointCloud)`
- `SpatialIndex.Bounds<TGeometry>(ReadOnlySpan<TGeometry>) where TGeometry : GeometryBase`
- `SpatialIndex.MeshFaces(Mesh)`
- `SpatialIndex.Search(BoundingBox)`
- `SpatialIndex.Search(Sphere)`
- `SpatialIndex.Overlaps(SpatialIndex other, double tolerance = 0.0)`
- `SpatialIndex.KNearest(ReadOnlySpan<Point3d> points, ReadOnlySpan<Point3d> needles, int count)`
- `SpatialIndex.Closest(ReadOnlySpan<Point3d> points, ReadOnlySpan<Point3d> needles, double limitDistance)`

**Spatial output policy:**

- Canonical outputs are source indices only: `SpatialHit(Id)` and `SpatialPair(A,B)`.
- Duplicate source geometry is represented by duplicate source indices.
- No custom IDs, geometry references, callback tags, insert/remove mutation, distances, result bags, config objects, broad spatial workflows, clustering, fields, or handrolled spatial algorithms are exposed.
- `SpatialIndex` owns and idempotently disposes the native Rhino `RTree`.

**Curve deviation API shape:**

- `Query.Deviation<TA,TB,TOut>(Deviation.Curve)` supports curve-derived pairs only.
- Canonical output is one `CurveDeviation` per pair with native minimum/maximum distance, paired curve points, tolerance, and within-tolerance evidence.
- `CurveDeviation.WithinTolerance` is `MaximumDistance <= GeometryContext.Absolute.Value`.
- No near-miss threshold bands, search-radius knobs, tangent/transverse labels, perturbation stability scores, or sampling workflows are exposed.

---
## [3][DONE_LEDGER]
>**Dictum:** *Completed foundations should not be reopened without a concrete defect.*

<br>

| [ORDER] | [CAPABILITY] | [STATUS] | [OWNER] | [CURRENT_SCOPE] |
| :-----: | ------------ | :------: | ------- | --------------- |
| **1** | Analysis contract stabilization | DONE | `Analyze.cs`, `Query.cs`, `Operation.cs` | Typed rails for null query, null geometry, unsupported output, invalid input/result, context failures, and ordered accumulation. |
| **2** | Curvature profile foundation | DONE | `Query.cs`, `Locate.cs` | Curve magnitude streams/profiles and surface Gaussian/Mean streams/profiles. |
| **3** | Mesh diagnostics | DONE | `Extract.cs`, `GeometryValidation.cs` | Native `Mesh.Check`, `MeshCheckParameters`, typed `MeshCheckCount`, topology parity. |
| **4** | Primitive vocabulary | DONE | `Extract.cs` | Curve `Circle`/`Arc`/`Ellipse`/`Polyline`, surface `Plane`/`Cylinder`/`Sphere`/`Cone`/`Torus`, Brep `Box`. |
| **5** | Intersection semantics | DONE | `Intersect.cs`, `Query.cs` | Native pair outputs plus compact `IntersectionKind`. |
| **6** | Read-only topology diagnostics | DONE | `Query.cs`, `Extract.cs` | Boundary, adjacency, and non-manifold evidence only. |
| **7** | Conformance residual vocabulary | DONE | `Query.cs`, `Measure.cs` | `Distance`, `Rms`, `WithinTolerance` for explicit pairs. |
| **8** | Conformance residual profile | DONE | `Query.cs`, `Measure.cs` | `ResidualProfile` count/min/max/mean/variance/RMS/tolerance/within-tolerance. |
| **9** | Quality/fairness evidence closure | DONE | Existing descriptors | Closed as lower-level evidence; no quality/fairness score APIs. |
| **10** | Spatial indexing | DONE | `Spatial.cs`, `Query.cs` | Collection-level native `RTree` owner with source-index hit/pair evidence. |
| **11** | Conformance max-sample evidence | DONE | `Query.cs`, `Measure.cs` | `ResidualSample` and `Conformance.Maximum(int)`. |
| **12** | Extra explicit primitive pairs | DONE | `Measure.cs`, `GeometryValidation.cs` | Curve-circle, curve-arc, and surface-sphere conformance. |
| **13** | Mesh face-level metric stream | DONE | `Query.cs`, `Extract.cs` | `MeshFaceMetric.AspectRatio` via native `Mesh.Faces.GetFaceAspectRatio(index)`. |
| **14** | Curve deviation evidence | DONE | `Query.cs`, `Intersect.cs` | `Deviation.Curve` via native `Curve.GetDistancesBetweenCurves`, compact `CurveDeviation` output. |

---
## [4][MISSING_OR_DEFERRED]
>**Dictum:** *Deferred means blocked by a named semantic or runtime gap, not vague future interest.*

<br>

| [ITEM] | [STATUS] | [WHY_NOT_DONE] | [OWNER] | [BRIDGE_CRITERION] |
| ------ | :------: | -------------- | ------- | ------------------ |
| Surface-cylinder conformance | DEFERRED | Rhino exposes `Surface.TryGetCylinder`, but `Cylinder` has no native closest-point/distance API. A custom formula would be speculative without an approved standard. | `Measure.cs` | Pick up only when Rhino exposes primitive closest/distance semantics or an approved formula standard is named with testable tolerance behavior. |
| Surface-cone conformance | DEFERRED | Rhino exposes `Surface.TryGetCone`, but `Cone` has no native closest-point/distance API. | `Measure.cs` | Same as cylinder: native API or approved formula standard plus runtime parity tests. |
| Surface-torus conformance | DEFERRED | Rhino exposes `Surface.TryGetTorus`, but `Torus` has no native closest-point/distance API. | `Measure.cs` | Same as cylinder: native API or approved formula standard plus runtime parity tests. |
| Near-miss threshold evidence | DEFERRED | Native curve deviation is now done, but near-miss still needs an approved tolerance-band/search policy. Archive threshold strategy lists and heuristic classifiers remain rejected. | `Intersect.cs` candidate | Pick up only with a named native proximity API, one compact descriptor, one tolerance policy, focused tests, and no config table. |
| Intersection stability evidence | DEFERRED | Archive perturbation sampling creates aggregate scores and unstable flags without Rhino-native stability semantics. | `Intersect.cs` candidate | Pick up only when Rhino exposes native stability semantics or an approved standard defines perturbation geometry, score meaning, and testable tolerance behavior. |
| Mesh skew/Jacobian/solver-like FEA streams | DEFERRED | Aspect ratio has native face-level semantics; skew/Jacobian/solver-like metrics would need formulas or solver semantics not yet approved. | `Extract.cs` candidate | Pick up only with Rhino-native APIs or named standard formulas, per-face typed outputs, and managed/runtime tests. |
| Rich feature/pattern detection | DEFERRED | Archive feature workflows are broad classifiers and would reintroduce request/result/config architecture. | Owner not selected | Pick up only as narrow descriptor-first native evidence with no workflow facade. |
| Rhino runtime execution in default gate | DEFERRED | `Rhino.Testing` resolves below `net10.0` and the RhinoWIP testhost crashes on macOS unless forced. | Test tooling | Pick up when `Rhino.Testing` provides a compatible `net10.0` asset or the testhost/tooling is updated. Runtime specs already compile. |

---
## [5][REJECTED]
>**Dictum:** *Rejected items are architectural exclusions, not backlog.*

<br>

| [ITEM] | [STATUS] | [REASON] |
| ------ | :------: | -------- |
| Archive request/result/config architecture | REJECTED | Conflicts with compact descriptor-first typed rails and would duplicate existing `Analyze`/`Query` ownership. |
| Broad workflow facades | REJECTED | Reintroduces folder-per-capability APIs and broad result bags. |
| Auto-detect conformance | REJECTED | Conflicts with explicit primitive-pair API and encourages `object?` primitive/result bags. |
| Aggregate quality/fairness/FEA scores | REJECTED | Score semantics are subjective and already decomposed into explicit evidence streams. |
| Topology healing and mutating repair | REJECTED | `analysis` is read-only; mutation belongs outside this library. |
| Spatial clustering/fields/proximity systems | REJECTED | Not native `RTree` ownership; would require broad algorithms and knobs. |
| `SpatialIndex` insert/remove mutation exposure | REJECTED | Public spatial API is immutable collection evidence; mutation would complicate source-index lifecycle and duplicate ID semantics. |
| Custom spatial IDs | REJECTED | Source indices are the canonical stable identity. |
| Morphology, fields, and transforms | REJECTED for current analysis roadmap | Separate capability areas, not blockers for analysis. |

---
## [6][RHINO_API_EVIDENCE]
>**Dictum:** *Rhino owns geometry semantics wherever it already provides them.*

<br>

Use current primary RhinoCommon docs before changing API claims:

| [API_AREA] | [PRIMARY_DOC] | [MODERN_USE] |
| ---------- | ------------- | ------------ |
| Spatial indexing | [RTree](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.rtree), [RTreeEventArgs](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.rtreeeventargs), [BoundingBox](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.boundingbox), [Sphere](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.sphere) | Native point, point-cloud, bounds, mesh-face trees; bounding-box/sphere search; overlaps; k-nearest; closest pairs. |
| Curve primitive residuals | [Curve](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.curve), [Line](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.line), [Circle](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.circle), [Arc](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.arc) | `NormalizedLengthParameter`, `PointAt`, primitive `ClosestPoint`. |
| Surface primitive residuals | [Surface](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.surface), [Plane](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.plane), [Sphere](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.sphere) | `Domain`, `PointAt`, `Plane.DistanceTo`, `Sphere.ClosestPoint`. |
| Mesh face metrics | [MeshFaceList](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.collections.meshfacelist) | `GetFaceAspectRatio(index)` for `MeshFaceMetric.AspectRatio`. |
| Mesh diagnostics | [Mesh](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.mesh), [MeshCheckParameters](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.meshcheckparameters) | `Check`, typed `MeshCheckCount`, self-intersections, naked edges, manifold checks. |
| Intersections/proximity candidate | [Intersection](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.intersect.intersection), [Curve](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.curve) | Existing intersections are implemented; near-miss/stability needs a precise native proximity/deviation API and tolerance policy before implementation. |
| Curve deviation | [Curve](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.curve), [Curve.GetDistancesBetweenCurves](https://mcneel.github.io/rhinocommon-api-docs/api/RhinoCommon/html/M_Rhino_Geometry_Curve_GetDistancesBetweenCurves.htm) | `Curve.GetDistancesBetweenCurves` computes native minimum/maximum distances and curve parameters for curve deviation. |

---
## [7][TEST_AND_GATE_TRUTH]
>**Dictum:** *Managed tests prove rails; Rhino specs prove native behavior when the host can run them.*

<br>

**Gates run for `analysis-curve-deviation`:**

- `bash -n scripts/check-cs.sh` passed.
- `scripts/check-cs.sh --self-test` passed.
- `dotnet test tests/csharp/analysis/Analysis.Tests.csproj --configuration Release --no-restore` passed: `Analysis.Tests` 50.
- `dotnet build tests/rhino/Rhino.Tests.csproj --configuration Release --no-restore` passed.
- `pnpm check:cs` passed Debug/Release builds, format/analyzer posture, and managed tests: `Analysis.Tests` 50, `Core.Tests` 10, `Foundation.CsAnalyzer.Tests` 85.
- `RASM_RHINO_TESTS=1 pnpm check:cs` passed managed gates and skipped Rhino runtime execution at the asset guard: `Analysis.Tests` 50, `Core.Tests` 10, `Foundation.CsAnalyzer.Tests` 85.

**Runtime caveat:** native `RTree` construction loads `rhcommon_c`, so managed tests cover only pre-native invalid spatial rails. Spatial native behavior is covered by Rhino runtime specs that compile. Runtime execution is currently skipped unless forced because `Rhino.Testing` resolves below `net10.0` and crashes the RhinoWIP testhost on macOS.

**Runtime specs compile for:**

- `SpatialIndex` point bounding-box search, sphere search, k-nearest, closest, mesh-face overlaps, empty indexes, and disposed-index failure.
- Conformance maximum samples and explicit curve-circle, curve-arc, and surface-sphere pairs.
- Mesh face aspect-ratio streams.
- Curve deviation min/max distance and paired point evidence.
- Existing curvature, topology, primitive extraction, intersection, mass, and mesh diagnostic behavior.

---
## [8][NEXT_BRANCH]
>**Dictum:** *The next branch needs sharper semantics before code.*

<br>

**Recommended next branch:** no `ACTIVE` implementation branch.

**Approval criteria before editing:**

- Choose one blocked item: near-miss threshold evidence or intersection stability evidence.
- Name the primary Rhino-native API or approved formula standard.
- Define one descriptor-first API and one compact typed output.
- Define tolerance semantics without threshold strategy lists, config tables, or knobs.
- Keep unsupported/null/invalid cases on typed rails.
- Add managed rail tests and Rhino runtime specs that compile even if runtime execution is skipped.

**Fallback if not approved:** do not implement more near-miss/stability work. Keep the roadmap at the current completed evidence surface.
