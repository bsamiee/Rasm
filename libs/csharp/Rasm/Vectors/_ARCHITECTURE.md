# Rasm.Vectors Architecture

## TODO

### Status By Owner

| Owner | Status | Shipped | Residual |
|-------|--------|---------|----------|
| `Matrix.cs` | `[implemented]` | Dense LU/QR/Cholesky, sparse BiCGStab + MathNet fallback, CSparse Cholesky, dense/sparse/generalized eigen, LOBPCG, QR least-squares; `SolveReceipt` / `EigenSolveReceipt` cover all six solve and five eigen paths with finite RHS/output admission and distinct fallback stops. | None at the rail level; downstream consumers continue to treat eigen receipts as advisory until `IsUsable`. |
| `Extraction.cs` | `[implemented]` | Domain-backed glyph/grid/bundle sampling, mesh scalar isolines, surface `IsoStatus`, point-cloud section curves, raw native contour counts; `ExtractionReceipt` embeds scalar-isoline + sample child receipts and preserves iso-surface facts even when mesh output is invalid. | Glyph/bundle runtime geometry proof remains bridge-owned. |
| `Cloud.cs` + `Align.cs` | `[implemented]` | Rings/polylines/clusters/weighted clusters, PCA/covariance, oriented normals, curvature receipts, convex 3D + 2D footprint hulls, winding, Sinkhorn with `CloudTransportPolicy` and retained-coupling confidence; ICP (Point/Plane/Symmetric/Robust/NormalWeighted) returns `AlignmentReceipt` with correspondences; `CloudKernel.MassOf` is the mass admission rail (8 call sites). | Concave/alpha hulls explicit unsupported; GICP not implemented (symmetric is linearized normal-sum); `NeighborhoodReceipt` and duplicate-coordinate facts moved to Open Work. |
| `Mesh.cs` + `Spectral.cs` | `[implemented]` mesh rail, `[partial]` spectral | Topology, feature classification, flatten, remesh, descriptors, 4-algorithm `MeshSegmentation`, per-vertex scalar isolines, heat/geodesic, Hodge/vector-heat/cross-field, generalized winding SDF, boundary-source SignedHeat, closed `VolumeGrid` SignedHeat; `LaplacianCache` caller-keyed success-only caches; CR connection Laplacian + CDS holonomy. | Robust tufted Laplacian, signpost transport for flipped intrinsic, watershed/normalized-cut, true tangent log-map, harmonic basis for genus > 0, and ridge/valley/region-boundary features remain unsupported. **Drift:** `RemeshStatus.NativeRejected` declared but never emitted; `FlattenReceipt.EdgeLengthDistortionRms` always `None`. |
| `Sample.cs` + `Flow.cs` + `Modes.cs` + `Space.cs` + `Atoms.cs` + `Intent.cs` | `[implemented]` | All 10 `SampleKind` variants execute via `SampleReceipt`; Yuksel weighted sample elimination is the proven blue-noise rail; RK tableaus, fixed/adaptive integration, bounded-bisection event localization; `SupportSpace` / `SurfaceSpace` projectors; `AtomProjection.Raw` utility shared by `Modes`; `VectorIntent.Project<TOut>` (39 cases) is the singular consumer rail with factory-owned admission. | Density/adaptive/weighted sampling labels remain diagnostic and deterministic; Bridson/Dwork/OT-CCVT/mesh-spectrum unsupported; higher-order Butcher moment consistency and RK dense output remain future work. |
| `Field.cs` | `[partial]` | Scalar/vector/tensor field algebra, kernel profiles + anisotropic falloff, RBF + oriented MLS detailed receipts, SDF primitives + closed planar profile extrusion, mesh-backed fields, iso-surface receipts with `IsoSurfaceGrid` decompile-shaped facts (fixed `0.001` root tolerance, `1e-5` normal sample distance); `FieldNabla` owns shared validation; `ScalarField` owns the recursive 30-case scalar-payload fold; closed SignedHeat executable through `SdfMeshPolicy.ClosedSignedHeat(...)`. | Native iso-surface live RhinoWIP bridge proof pending; MLS labeled approximate, not paper-faithful (Levin/APSS/Poisson); kernel spatial Hessian explicit non-claim. **Drift:** `SdfMeshDomain.VolumeTet` declared but no executable rail in `Field.cs:SdfMeshPolicy`. |

Matrix, extraction admission, mesh segmentation, mesh/spectral receipt facts, cloud/alignment/transport mass diagnostics, and the Flow/Space/Modes/Sample projection polish are close enough to mark implemented. Field/SDF stays partial because product-strength native marching-cubes proof and the `VolumeTet` rail are still open.

### Drift Items

- **`RemeshStatus.NativeRejected`** — declared at `Mesh.cs:157`, never emitted; `RemeshReceiptOf` at `Mesh.cs:945` hardcodes `Completed`. Wire on native remesh failure or remove the enum case; failure currently returns `InvalidResult` and never reaches the `Completed` constructor.
- **`SdfMeshDomain.VolumeTet`** — declared at `Mesh.cs:166`, no executable rail; `SdfMeshPolicy` factories at `Field.cs:211–216` expose only 3 methods. Implement tet FEM (deferred per project memory) or remove the enum case; the cross-file `SdfMeshDomain` (Mesh) + `SdfMeshMethod` (Field) dual encoding should collapse to one owner when resolved.
- **`FlattenReceipt.EdgeLengthDistortionRms`** — declared at `Mesh.cs:155`, always assigned `None` at `Mesh.cs:645`. Compute distortion RMS on `MeshUnwrapper` output or remove the field.

### Open Work — Sequenced

Dependency-ordered tiers. Pull from a higher tier only when the prior tier is stable.

- **Tier 1 — Cleanup (no dependencies):** resolve the three Drift Items above; harmonize `SdfMeshPolicy` factory ordering if the `VolumeTet` rail lands.
- **Tier 2 — Validation centralization (LOC win, no behavior change):** see the next block.
- **Tier 3 — Receipt gaps (must land before bridge proofs):** `NeighborhoodReceipt` in `Cloud.cs` (kNN/radius proof surface beyond curvature context); duplicate-coordinate dedup receipt in cluster admission; explicit plane/sphere/saddle curvature range receipt (currently only `ShapeIndex` scalar at `Cloud.cs:933–934`); `SpectralFilter.Wave` WKS-normalization receipt completion.
- **Tier 4 — Algorithm rails (independent):** Bridson active-list Poisson disk; variable-density Dwork-style sampling; OT/CCVT capacity-satisfying sampling; mesh-spectrum validated sampling; ridge/valley/region-boundary feature classification; watershed/normalized-cut segmentation; tangent log-map (exponential connection map).
- **Tier 5 — Advanced math / research (defer until 1–3 stable):** robust tufted Laplacian (Sharp-Crane); signpost transport for flipped intrinsic; harmonic basis construction for genus > 0; true GICP; paper-faithful MLS (Levin/APSS/Poisson); `VolumeTet` executable FEM rail; higher-order Butcher tableau moment consistency; RK dense-output coefficients.
- **Tier 6 — Native bridge proofs (RhinoWIP runtime, gated on Tier 3 receipts):** iso-surface native callback proof; scalar isoline runtime geometry; hull facet/footprint proofs; kNN/radius neighborhood proofs; plane/sphere/saddle curvature range proofs.

### Validation Centralization

Concrete hotspots from the file audit, sequenced by leverage. Aggregate honest estimate ~150–300 LOC reduction across the folder (within the documented density ceiling). Each entry: pattern → file:line → collapse target.

1. `Optional(x).ToFin(op.InvalidInput())` — `Intent.cs` factories 15+ sites — extract `Admit.NotNull<T>` rail.
2. `MeshSpace.Native` non-null guard — `Intent.cs` Flatten/Hull/Remesh/Topology/Features/Descriptor/DiscreteCalculus/Segmentation — extract `MeshIntent.AdmitNative`.
3. Hull receipt construction — `Cloud.cs:578,579,581,585,590,591` — three helpers (`HullReceiptRejected/Unsupported/Completed`).
4. `count >= minimum` finite-count gates — `Cloud.cs` 11+ sites — single `Admit.CountAtLeast` predicate.
5. `DenseSolveInputIsValid` — `Matrix.cs:417,421,429,438` — fold into shared `DenseSolveGated` factory.
6. `RhinoMath.IsValidDouble` finite chains — 20+ sites in `Atoms.cs` / `Align.cs` / `Space.cs` / `Cloud.cs` / `FieldNabla` — extend `FieldNabla.Finite` into a folder-wide `Validation.AllFinite(params)` helper.
7. `AdmitScalarSource` / `AdmitVectorSource` / `AdmitTensorField` triad — `Field.cs:996–1108` 25+ recursive call sites — collapse to `AdmitSource<TField>` polymorphic helper.
8. `SampleKind.Admit()` dual-validation — `Sample.cs:52–64` — eliminate re-call by caching admission at construction.
9. `ContourPolicy.Admit()` dual-validation — `Extraction.cs:101–107` — same pattern.
10. `ButcherTableau` dual-validation — `Flow.cs:21–26,128–142` — single computed `IsValid` (currently recomputes via `Admit`).
11. Plane finite checks — `Atoms.cs:VectorFrame.Of` / `Space.cs:107–113` / `FieldNabla.Plane` (`Field.cs:1321–1334`) — unify into one `Validation.PlaneOrthonormal` rail.
12. Align solve admission triplet — `Align.cs:157,181,201` — extract shared `n-array-length + finite` predicate.
13. `Counted()` helper — `Sample.cs:31–32` — already factored; verify all three callers route through it.
14. `WeightedCentroidOf` — `Align.cs:200–210` — single-call helper with two call sites at `Align.cs:195–196`; accept as cohesive or inline.
15. `WithCurvature` 4-case repetition — `Modes.cs:46–54` — already factored; verify no inline duplicates remain.

### Backlog By Owner

- `[implemented]` `Intent.cs`: `VectorIntent.Project<TOut>(Context, Op?)` is the sole consumer rail across 39 Union cases; null/raw admission lives in every factory; capability checks delegate to owner modules.
- `[implemented]` `Extraction.cs`: domain-backed glyph/grid/bundle sampling, mesh scalar isolines, surface `IsoStatus`, point-cloud section curves; `ExtractionReceipt` embeds scalar-isoline and sample child receipts and preserves iso-surface facts even when mesh output is invalid.
- `[implemented]` `Flow.cs`: RK tableaus, embedded-order adaptive stepping, fixed/adaptive integrator construction, bounded-bisection event localization with positive-iteration admission, `StreamlineTrace` receipts. Higher-order Butcher moments and RK dense output remain future work.
- `[implemented]` `Sample.cs`: 10 `SampleKind` variants execute via `SampleReceipt`; weighted mass routes through `CloudKernel.MassOf`; Yuksel weighted sample elimination is the proven blue-noise rail; density/adaptive remain deterministic priority labels.
- `[implemented]` `Space.cs`: `SupportSpace` admission/closest/signed-distance/containment/tangent/frame; 14-variant `SupportProjection` collapsed through one `SupportProjectionState` projector; cluster validity routes through `CloudKernel.MassOf`.
- `[implemented]` `Atoms.cs`: dimensions, magnitudes, intervals, axes, angles, directions, spans, frames, cones, relations, transport. `AtomProjection.Raw` is a polymorphic projection *utility* consumed by `Modes.cs` (and transitively by `Intent.cs`); it is not an "owner" of projection logic.
- `[implemented]` `Modes.cs`: 8 curve + 13 surface + 4 cone projection variants; surface derivative modes share `SurfaceDerivatives` + `Derivatives` factory; all raw outputs route through `AtomProjection.Raw`.
- `[implemented]` `Cloud.cs`: rings/polylines/clusters/weighted clusters, PCA/covariance, oriented normals (MST), curvature receipts, convex 3D + 2D footprint hulls, winding, Sinkhorn with retained-coupling confidence; `CloudKernel.MassOf` is the mass admission rail; concave/alpha hulls explicit unsupported.
- `[implemented]` `Align.cs`: Point/Plane/Symmetric/Robust/NormalWeightedPointToPlane ICP return `AlignmentReceipt` + correspondences; source/target row weights route through the cloud mass rail; symmetric ICP correctly labeled linearized normal-sum, not true GICP.
- `[implemented]` `Mesh.cs`: topology, feature basics, flatten, remesh, descriptors, 4 segmentation algorithms, isolines, heat/geodesic, Hodge/vector-heat/cross-field, generalized winding SDF, boundary-source SignedHeat, closed `VolumeGrid` SignedHeat; `LaplacianCache` caller-keyed success-only caches. Two drift items (`RemeshStatus.NativeRejected`, `EdgeLengthDistortionRms`) listed above.
- `[partial]` `Spectral.cs`: DEC, spectral basis, filters, descriptor receipts + normalization/ranking, CR connection Laplacian + receipts, FEM heat scaffold, CDS holonomy; flipped-intrinsic / genus-positive Hodge-CDS / robust / harmonic-basis gaps remain unsupported.
- `[partial]` `Field.cs`: scalar/vector/tensor field algebra, kernel profiles + anisotropic falloff, RBF + MLS detailed receipts, SDF primitives + closed planar profile extrusion, mesh-backed fields, iso-surface receipts; `FieldNabla` owns shared validation; `ScalarField` owns the recursive 30-case scalar-payload fold; closed SignedHeat executable. Live RhinoWIP iso-surface bridge proof pending; paper-faithful MLS, kernel spatial Hessian, and `VolumeTet` executable rail remain partial/future.

### Intent And Projection Gaps

- `[implemented]` `Intent.cs`: sample, contour, glyph, grid, and stream-bundle intents route through `ExtractionDomain + SampleKind` under `VectorIntent.Project<TOut>`. New projection work extends this rail.
- `[partial]` Streamline projection: `Curve` output, localized event point, trace health, and event kind/status metadata exist. RK dense output missing; localization is bounded bisection, not interpolant evaluation.
- `[partial]` Sample projection: `Seq<Point3d>`, `PointCloud`, `VectorCloud`, and `SampleReceipt` project through one `SampleKind` execution. Weighted/scalar-density/adaptive modes remain deterministic priority labels, not proven density blue-noise.
- `[partial]` Feature projection: `FeatureEdge` / `FeatureReceipt` covers boundary, crease, nonmanifold, unwelded, ngon-interior. Ridge/valley and region-boundary classification unsupported.
- `[partial]` Flatten projection: UV flatten through `MeshUnwrapper`; `EdgeLengthDistortionRms` declared but always `None` (drift item).
- `[partial]` Descriptor projection: `SpectralDescriptorPolicy` owns basis-time zero-mode, scale, energy, and eigenmode-crop normalization; `SpectralRankingPolicy` owns deterministic full-vector ranking; `SpectralFilter.Wave` remains raw log-Gaussian weighting until a full WKS receipt rail lands.
- `[implemented]` Topology projection: `TopologyReceipt` reports counts, boundary components, nonmanifold facts, optional genus, and Euler validation; reuse via optional pass-through is verified at `Mesh.cs:1380` (SDF, features, flatten).

### Flow And Numerical Integration

- `[implemented]` `Flow.cs`: Runge-Kutta tableaus carry stage count, method order, optional embedded order, and structural admission used by fixed/adaptive integrator factories.
- `[partial]` `ButcherTableau`: structural validation covers row sums, primary/embedded weight sums, abscissae. Higher-order Butcher moment consistency remains future work.
- `[implemented]` Adaptive stepping: embedded-pair order and exponent are per method because Bogacki-Shampine 3(2), Cash-Karp 5(4), and Dormand-Prince 5(4) do not share one truthful metadata model.
- `[partial]` Event handling: `CrossSurface` requires closest + signed-distance support; `RegionThreshold` admits finite thresholds; both use bounded bisection/chord localization with positive-iteration admission. RK dense-output coefficients remain future work.
- `[implemented]` Trace receipts: method order, embedded order, errors, min/max step, termination point, event values, event kind, and event status all derive from one `StreamlineState` accumulation; product layers correctly describe localization as bounded bisection.

### Fields, Kernels, And SDFs

- `[partial]` `Field.cs`: iso-surface routes through intent/extraction; extraction preserves native `IsoSurfaceReceipt` / `IsoSurfaceResult` facts even when evaluator failures make the mesh invalid. `ScalarField.IsoSurfaceDetailed` owns the Rhino callback, prewarms mesh-backed SDF/cache work before native marching, and rejects invalid public mesh results through `Fin`. `Mesh.CreateFromIsosurface` is labeled parallel-callback with fixed `0.001` root tolerance and `1e-5` normal sampling; effective grid dimensions are reported as `IsoSurfaceGrid` facts (decompiled floor-cell formula + corner/center/initial sample counts). Runtime RhinoWIP bridge proof remains pending.
- `[implemented]` `KernelKind`: radial profiles expose value, first derivative, second derivative, and smooth/boundary/nonsmooth/outside-support status; `Weight` is `Profile.Value`; the profile rail is the single derivative owner. No spatial gradient/Hessian claim.
- `[implemented]` Field admission: invariant-bearing field/falloff/integrator cases construct through validated factories; direct external case construction is closed where Thinktecture regular unions still require public case types; `FieldNabla` owns the shared finite/plane/kernel/periodic/reconstruction-sample/iso-surface validators; `ScalarField` owns the recursive 30-case scalar-payload fold.
- `[implemented]` Kernels: anisotropic falloff uses `TensorField` / `SymmetricMatrix` metrics through `sqrt(v^T M v)` and rejects invalid, nonfinite, or nonpositive metric distances. Kernel derivatives remain radial-profile facts only.
- `[partial]` Reconstruction: RBF interpolation/approximation records mode, smoothing, sample count, residual, solve path, and factor facts via matrix solve receipts. Oriented MLS admits samples through `Context` tolerance, returns per-query detailed reconstruction receipts (neighborhood count, rejected weights, rank, conditioning, normal agreement, gradient norm, solve facts), and fails unsupported neighborhoods without constructing invalid matrix dimensions; labeled approximate SDF rather than paper-faithful Levin/APSS/Poisson.
- `[implemented]` SDF primitives: half-space, slab, capped cone, and closed planar single-region profile extrusion exist; profile extrusion requires a closed planar Rhino `Curve`, explicit plane, positive half-height, self-intersection-free single region, native containment, and closest-point distance, with receipt facts for tolerance source, containment, closest acceptance, and active feature.
- `[implemented]` SDF outputs: `ScalarField.LipschitzBound()` exists; mesh-backed signing routes through admitted `SdfMeshPolicy` values for generalized winding, boundary-source SignedHeat, and closed `VolumeGrid` SignedHeat. Mesh SDF receipts carry the shared `TopologyReceipt` + `SdfMeshDomain` (`SurfaceMesh`, `BoundarySource`, `VolumeGrid`, `VolumeTet`); closed SignedHeat requires oriented closed/solid/watertight meshes and at least one strict-inside grid node for sign calibration. `ClosedSurfaceSignedHeat` uses a regular padded Cartesian lattice, triangle-source vector heat from `Mesh.SolidOrientation()`, gauge-pinned finite-difference Poisson solve, residual-tolerance enforcement, finite-domain trilinear interpolation, and explicit `VolumeGridReceipt` facts. This is approximate volumetric SHM, not exact Euclidean distance or tet FEM. `VolumeTet` enum declared in `SdfMeshDomain` but no executable rail in `Field.cs:SdfMeshPolicy` (drift item).

### Mesh And Spectral Operators

- `[partial]` `Mesh.cs` per-vertex scalar isolines: local PL contours with payload-length admission, finite-level validation, Rhino quad triangulation, edge interpolation, exact-edge dedupe, plateau rejection, branch-safe stitching, and stitched/branch/incident-degree counts. Runtime bridge proof remains pending.
- `[partial]` Mesh features: classified boundary/crease/nonmanifold/unwelded/ngon-skipped receipt facts via native topology-edge APIs. Ridge/valley and region-boundary classification unsupported.
- `[implemented]` Mesh segmentation: `MeshSegmentation` executes scalar threshold components, scalar band components, seeded scalar region growing, and descriptor-scalar clustering; `MeshSegmentationResult` carries face + vertex assignments; `MeshSegmentationReceipt` reports algorithm/status/region counts/seeds/iteration/tolerance/descriptor/spectral facts where available. True watershed and normalized-cut segmentation remain unsupported.
- `[implemented]` Mesh diagnostics: `LaplacianCache` exposes spectral cache-hit facts; cotangent/IDT Laplacians count skipped degenerate faces; descriptor receipts carry truncated eigenpair/cache/skip facts; CR edge-connection factors carry kinded assembly receipts into boundary SignedHeat; sparse fallback solve status is distinct from residual convergence; boundary-source signed heat surfaces topology/source/heat/Poisson receipts; closed SignedHeat surfaces `VolumeGrid` counts/source/operator/interpolation/solve facts; SDF receipts reuse topology/domain facts. Robust tufted Laplacian, flipped signpost vector heat, runtime scalar-isoline geometry proof, executable `VolumeTet`, and true tangent log-map remain unsupported.
- `[partial]` Spectral descriptors: detailed descriptors expose raw/normalized status, pairwise/source metadata, basis-time zero-mode/eigenmode-crop facts, value-only post-hoc energy normalization, comparison readiness, deterministic full-vector ranking, skipped-degenerate/factor facts, and genus-positive harmonic unsupported facts. Full harmonic bases and paper-complete WKS normalization remain unsupported.
- `[partial]` Remesh outputs: native remesh returns `RemeshResult` / `RemeshReceipt` with target/count/reduction/validity/hard-edge-request/topology-change facts. `RemeshStatus.NativeRejected` declared but never emitted (drift item).

### Clouds, Alignment, And Transport

- `[implemented]` `Cloud.cs` Sinkhorn: `SinkhornReceipt` reports coupling summaries, source/target residual semantics, numeric status, canonical `CloudTransportPolicy`, convergence tolerance, positive coupling cutoff, debiased self-costs, and retained-correspondence summaries; internals consume normalized mass admitted by `CloudKernel.MassOf`; correspondence confidence is retained-coupling coverage, not geometric probability.
- `[implemented]` `Cloud.cs` weighted clusters: centroid/covariance, principal axes/frame, shape/spread, density, transport mass, alignment row weights, oriented normals, and local curvature receipts route through `CloudKernel.MassOf` and one neighborhood/PCA policy rail. Curvature proof covers accepted/rejected samples, rank/residual rejection, eigen-gap and fit-residual tolerances, finite principal directions, and scalar derived curvedness/shape-index outputs.
- `[implemented]` `Cloud.cs` hulls: 3D convex and 2D convex/footprint hull results carry tolerance, angle tolerance, native facet count, planarity/coplanar rejection, input/output counts, and containment proof counts; convex 3D uses Rhino native hull facets, footprint hulls use Rhino native 2D hull indices plus local containment proof. Concave outline and alpha-style requests return explicit unsupported receipts.
- `[implemented]` `Align.cs`: receipt includes kind, approximation status, solve receipt, correspondences, residual quantiles/max, robust scale/weight range, final step delta, and mass-admitted row weights. Symmetric ICP remains a normal-sum linearized approximation, not true GICP.
- `[implemented]` `Align.cs` correspondences: per-point residual vectors project from the matching pass; target IDs resolve through `PointCloud.PointAt(index)`.
- `[implemented]` Transport: coupling, distance, receipt, transported cloud, retained-mass correspondence summaries, canonical Sinkhorn policy admission, and row-mass payload transfer. Product IDs and module attributes stay outside this library.

### Sampling And Domain Coverage

- `[implemented]` `Sample.cs`: weighted and scalar-field-driven sampling route through `SampleKind`; density maps and programmatic priorities control deterministic selection and output mass through `CloudKernel.MassOf`. Yuksel weighted sample elimination is the proven candidate-batch blue-noise rail; density/adaptive priority selection remains diagnostic and deterministic, not a density blue-noise claim.
- `[partial]` `Sample.cs` domain coverage: sampling routes through `ExtractionDomain` for explicit samples, mesh policies, support count-backed sampling, deterministic cloud-vertex candidates, weighted input, and scalar/adaptive density policies. Boundary domains and non-mesh Poisson density remain unsupported.
- `[implemented]` `Sample.cs` adaptive: scalar-field intensity varies local spacing; correctly labeled deterministic priority/adaptive spacing rather than a proven density blue-noise kernel.
- `[implemented]` Blue-noise sampling: `Sample.cs` owns Yuksel admission/execution/receipts; `Mesh.cs` owns mesh candidate/domain geometry; `Spectral.cs` is the only appropriate owner if spectral quality metrics are explicitly reopened. Bridson active-list, variable-density Dwork, OT/CCVT capacity satisfaction, and mesh-spectrum validation remain Tier 4 Open Work, not blockers for the current implemented status.
- `[implemented]` `SampleReceipt`: attempted/emitted/rejected, candidate count, spacing stats, count-density error, density admission counts, iteration count, stop kind, domain status, and `SampleAlgorithmReceipt` facts derive from one execution.

### Modes, Matrices, And Product Boundary

- `[implemented]` `Modes.cs`: surface point/frame/UV frame/Jacobian/metric/area-scale projections, explicit curve frame/perpendicular-frame normal/binormal policies, cone projections; curve/surface/cone raw outputs share `AtomProjection.Raw`; surface derivative modes share one `SurfaceDerivatives` helper. Native runtime behavior remains bridge-owned.
- `[implemented]` `Matrix.cs`: `SolveReceipt` / `EigenSolveReceipt` cover all six solve paths (`DenseLu`, `DenseQrLeastSquares`, `DenseCholesky`, `SparseBiCgStabDiagonal`, `SparseMathNetDirectFallback`, `SparseCholesky`) and five eigen paths (`DenseSymmetricEvd`, `DenseGeneralEvd`, `SparseLobpcg`, `SparseHermitianLobpcg`, `SparseGeneralizedCholeskyCongruence`). Sparse direct fallback uses a distinct `DirectFallbackSolved` stop; LOBPCG normalizes symmetric storage before solving; max-iteration exhaustion returns finite diagnostic pairs/residuals as `MaxIterationsExhausted` (not usable).
- `[implemented]` Matrix receipts and failures: no sentinel-style fallback returns success when it should fail. Jacobi preconditioner diagonal clamp (`Matrix.cs:648`) and Rayleigh-quotient zero-denominator clamp (`Matrix.cs:660`) are intentional algorithmic guards. Nonconvergence, unsupported topology, invalid factorization, missing native capability, lossy fallback, and approximate output surface through `Fin<T>` or typed statuses; materialized diagnostic statuses are not permission for downstream consumers to use invalid payloads.
- `[implemented]` Product boundary: no UI, preview conduits, bake commands, GH2 parameter wrappers, or command receipts belong in `Rasm.Vectors`. Return typed geometry, weights, coupling, correspondences, residuals, and factual diagnostics only.

`Rasm.Vectors` is the typed vector geometry and numerics layer over RhinoCommon geometry, MathNet linear algebra, CSparse.NET sparse Cholesky, LanguageExt result rails, and Thinktecture-generated dispatch. Factories create atoms, spaces, fields, clouds, matrices, meshes, and intent cases; `VectorIntent.Project<TOut>(Context, Op?)` remains the singular consumer rail for executing an intent into a requested output shape. `Spectral.cs` is the shared substrate owning DEC operator assembly, spectral basis values, FEM heat-method scaffolding, the Crouzeix-Raviart connection Laplacian (Stein-Wardetzky-Jacobson-Grinspun 2020), the Crane-Desbrun-Schröder trivial-connection 1-form, and the polymorphic `SpectralFilter` algebra consumed by both mesh descriptors and scalar spectral fields. `Mesh.cs` owns `LaplacianCache`, which memoises spectral bases and factorisations per mesh snapshot.

```mermaid
---
config:
  layout: elk
  look: neo
  theme: base
  themeVariables:
    background: "#282a36"
    primaryColor: "#44475a"
    primaryTextColor: "#f8f8f2"
    primaryBorderColor: "#bd93f9"
    lineColor: "#6272a4"
    secondaryColor: "#50fa7b"
    tertiaryColor: "#282a36"
    clusterBkg: "#282a36"
    clusterBorder: "#6272a4"
    edgeLabelBackground: "#282a36"
---
flowchart TB
    accTitle: Rasm.Vectors projection rail with spectral substrate
    accDescr: Factories build typed vector values and VectorIntent cases. Project validates context, dispatches to owning vector modules, and returns Fin of the requested output. Spectral.cs holds DEC operators, spectral basis values, and spectral filter algebra shared by Mesh and Field; Mesh-owned LaplacianCache memoises per-mesh bases and factors.

    subgraph Build["Typed Construction"]
        Factories["Factories<br/>Atoms, Space, Field, Cloud, Mesh, Matrix<br/>Modes, Flow, Sample, Align"]
    end

    subgraph Rail["Singular Intent Rail"]
        direction LR
        Intent["VectorIntent cases"]
        Project["Project&lt;TOut&gt;(Context, Op?)"]
        Dispatch["generated dispatch"]
        Intent -->|one consumer rail| Project
        Project -->|validate context| Dispatch
    end

    subgraph Bands["Capability Bands"]
        direction LR
        Geometry["Geometry projections<br/>axis, angle, frame, support, surface"]
        FieldBand["Field algebra<br/>CSG, kernels, noise, gradients, streamlines"]
        CloudBand["Cloud workflows<br/>metrics, winding, hull, transport, ICP"]
        MeshBand["Mesh workflows<br/>Laplacian, topology, remesh, descriptors, caches"]
    end

    subgraph Spectral["Spectral Substrate"]
        direction LR
        Dec["DiscreteCalculus<br/>d0, d1, star0, star1, star2"]
        Basis["SpectralBasis<br/>eigenpair values"]
        Filter["SpectralFilter<br/>Heat, Wave, Biharmonic, Diffusion, CommuteTime, Identity"]
        Dec -->|operators| Filter
        Basis -->|modes| Filter
    end

    Providers["Shared providers<br/>Domain.Geometry, RhinoCommon, Matrix"]

    Factories --> Intent
    Dispatch --> Geometry
    Dispatch --> FieldBand
    Dispatch --> CloudBand
    Dispatch --> MeshBand

    MeshCache["LaplacianCache<br/>mesh snapshot caches"]

    FieldBand -->|mesh-aware fields| MeshBand
    MeshBand -->|memoises| MeshCache
    MeshCache -->|bases + factors| Spectral
    MeshBand -->|DEC + filters| Spectral
    Spectral --> Providers
    Geometry & CloudBand & MeshBand --> Providers

    Geometry & FieldBand & CloudBand & MeshBand --> Result["Fin&lt;TOut&gt;"]

    classDef rail fill:#44475a,stroke:#bd93f9,color:#f8f8f2,stroke-width:2px
    classDef owner fill:#282a36,stroke:#8be9fd,color:#f8f8f2
    classDef spectral fill:#282a36,stroke:#50fa7b,color:#f8f8f2,stroke-width:2px
    classDef provider fill:#282a36,stroke:#ffb86c,color:#f8f8f2,stroke-dasharray:5\,5
    classDef result fill:#50fa7b,stroke:#f8f8f2,color:#282a36,stroke-width:2px
    class Intent,Project,Dispatch rail
    class Factories,Geometry,FieldBand,CloudBand,MeshBand owner
    class Dec,Basis,Filter spectral
    class Providers provider
    class Result result
```

## Ownership

- `Intent.cs`: `VectorIntent` cases, factories, context validation, dispatch delegation.
- `Atoms.cs`: dimensions, magnitudes, axes, angles, directions, spans, frames, cones, relations, shared raw-output projection, and `Direction.ParallelTransport(Seq<Plane>)`.
- `Modes.cs`: curve / surface / cone / pose projection selectors; shared `AtomProjection.Raw` output projection for curve, surface, and cone raw values; `SurfaceProjection.ShapeOperator` projects Rhino `SurfaceCurvature` into a `SymmetricMatrix`.
- `Space.cs`: `SupportSpace`, `SurfaceSpace`, `SupportProjection`, signed distance, containment, closest-hit projection.
- `Field.cs`: scalar/vector/tensor field algebra (CSG blending, falloff, kernels, noise, finite difference). Mesh-aware extensions: `ScalarField` adds `Geodesic`, `MeanCurvatureFlow`, `SpectralDistance`, `Stripe`, and `SignedDistanceFromMesh`; `VectorField` adds `CrossField`, one `Hodge` case carrying `BoundarySense`, `VectorHeat`, and `GeodesicTangent`.
- `Flow.cs`: validated Runge-Kutta tableaus, fixed/adaptive integration, streamline state, termination predicates, and `StreamlineTrace` projection receipts.
- `Cloud.cs`: cloud construction (Ring / Polyline / Cluster / WeightedCluster), `VectorCloudMetric` SmartEnum (PCA, oriented normals, principal curvature, curvedness, shape index), plus separate intent rails for winding, hull, and transport. `CloudKernel.Sinkhorn` uses `CloudTransportPolicy` and log-domain scaling; policy mass relaxation changes KL marginal penalties over validated normalized masses.
- `Sample.cs`: canonical `SampleKind` owner for explicit points, mesh-surface policies, support count-backed sampling, deterministic cloud candidates, and `SampleReceipt`.
- `Align.cs`: cloud alignment -- `AlignKind` SmartEnum admits `Point`, `Plane`, `Symmetric` (Rusinkiewicz 2019 with oriented normal sum), `Robust` (MAD-scaled Welsch IRLS), and `NormalWeightedPointToPlane`.
- `Mesh.cs`: mesh snapshots, local PL scalar isolines, `LaplacianCache` (cotangent / IDT / explicitly unsupported robust Laplacian, scalar Cholesky factor, parametric scalar-heat / vector-connection / edge-connection Cholesky caches via `Atom<HashMap>`, spectral basis with cache-hit facts, mean edge length, mesh-invariant boundary-source SHM phi plus source/heat/Poisson receipts, policy-keyed closed `VolumeGrid` SHM solves, and typed per-kernel `Atom<HashMap<TKey, TValue>>` success-only caches for geodesic / MCF / cross-field / Hodge / vector-heat / signed heat with structurally-equal record keys), `MeshLaplacian` SmartEnum (`Cotangent`, `IntrinsicDelaunay`, `Robust`), `MeshDescriptor` Union (single `SpectralCase`), `MeshSegmentation` Union, `IntrinsicMesh` (post-IDT-flip frozen edge index + face-edge map + face areas + first-incident-edge per vertex), topology, features, remesh kernels, Hodge, vector heat, geodesic tangent, stripe, cross-field, triangle solid-angle winding SDF, boundary-source SignedHeat kernels, and closed regular-grid SignedHeat kernels.
- `Matrix.cs`: dense and sparse matrix models, MathNet conversion, dense decompositions, dense QR least-squares, BiCGStab sparse solves with MathNet QR fallback receipts, sparse Hermitian products, local LOBPCG eigensolves without hidden dense fallback, solve/eigen receipts, and `CholeskySparse` for CSparse.NET-backed SPD-intended factorisation with typed factorisation failure.
- `Spectral.cs`: `DiscreteCalculus` (DEC operators `d0`, `d1`, `star0` barycentric/lumped mass, `star1`, `star2`), `SpectralBasis` eigenpair values, `SpectralFilter` algebra, FEM heat scaffold, Crouzeix-Raviart connection Laplacian, `ComputeIntrinsicStar1`, and CDS 2010 holonomy distribution over intrinsic incidence operators.

## Invariants

- `VectorIntent.Project<TOut>(Context, Op?)` is the only consumer projection rail.
- `ExtractionDomain + SampleKind` is the only sampling/extraction rail for sample, glyph, grid, stream-bundle, contour, and sample receipt projections; no parallel sample-source union exists.
- `Spectral.cs` owns DEC operators, spectral basis values, `SpectralFilter` dispatch + partial-monoid `Compose`, FEM heat scaffolding, the Crouzeix-Raviart edge connection Laplacian for SHM, and the CDS holonomy 1-form for trivial connections. Mesh-owned `LaplacianCache` memoises `SpectralBasisOf(k)` and downstream factors. Field and Mesh route spectral queries through this single substrate.
- `MeshDescriptor` is a single `SpectralCase` parameterised by `SpectralFilter` and optional source set. HKS-like heat signatures and unnormalized WKS-style wave weights route through `Heat` and `Wave`; `Identity` exposes raw spectral signatures and is not a full ShapeDNA implementation.
- `MeshLaplacian` admits `Cotangent`, `IntrinsicDelaunay`, and `Robust`; `Robust` currently returns typed `Unsupported` until a true Sharp-Crane tufted cover lands.
- `LaplacianCache` exposes caller-keyed success-only `Cotangent`, `IntrinsicDelaunay`, `Robust`, `Cholesky` (mass-pinned SPD-intended regularisation), `IntrinsicMeshSnapshot` (post-flip frozen `IntrinsicMesh` with stable edge index), boundary-source signed heat values plus topology/source/heat/Poisson receipts, closed `VolumeGrid` SignedHeat values plus topology/grid/operator/interpolation/Poisson receipts keyed by grid/heat/solver/interpolation/boundary-condition policy, default and parametric spectral bases with cache-hit and truncated eigenpair facts, connection/scalar/edge Cholesky caches, and success-only typed `Atom<HashMap<TKey, TValue>>` memoisers keyed by structurally-equal records.
- Vector heat uses cached CSparse Cholesky solves for the connection, magnitude, and indicator heat systems; recovery remains approximate and rejects flipped intrinsic meshes until signpost transport exists.
- Constrained cross-field is available on unflipped intrinsic meshes only; flipped intrinsic edges return typed `Unsupported` until signpost transport is implemented.
- Trivial connections (CDS 2010, closed genus-0 default) use intrinsic incidence operators and `ValidateGaussBonnet`; Rhino closed-mesh admission treats `GetNakedEdges() == null` as closed, and bounded meshes return invalid-input faults.
- `SdfMeshMethod.BoundarySignedHeat` is boundary-source and unflipped-only. Closed/no-boundary meshes use `SdfMeshMethod.ClosedSurfaceSignedHeat` through `SdfMeshPolicy.ClosedSignedHeat(...)`; default or constructor-bypassed policies fail admission, and open/non-solid/nonwatertight/unoriented meshes reject that rail before grid assembly. The closed rail is approximate regular-grid volumetric SHM over a finite padded domain, requires a strict-inside grid node for sign calibration, and does not claim exact Euclidean distance or tet FEM behavior.
- `Field.ScalarField` extends a continuous scalar with mesh-aware cases that delegate to `MeshKernel`. `VectorField` extends with mesh-aware Hodge decomposition, vector heat, geodesic tangent, and cross-field with constrained / cone variants.
- `Cloud.CloudKernel.Sinkhorn` accepts `CloudTransportPolicy` for balanced/unbalanced transport over normalized cluster masses and measures relaxed convergence by scaling change.
- Greenfield canonical names have no shims: `MaxIterations`, `MaxIterationsExhausted`, `RegionThresholdCrossing`, `Pairs`, `TargetLength`, `Spread`, and `Debiased`.
- Domain owns shared Rhino geometry normalization and `ClosestHit`.
- Vectors owns vector-specific intent, polymorphic field algebra, cloud metrics, mesh operators, sampling, alignment, and spectral substrate.
- RhinoCommon provides native geometry, closest queries, transforms, convex hulls, mesh reduction, remeshing, mesh unwrap, normals, marching-cubes isosurface, point-in-solid, and surface-curvature principal directions via `SurfaceCurvature`.
- MathNet owns dense decompositions, dense LU/QR solve primitives, sparse products, BiCGStab iteration, MathNet QR fallback solve projection, and local LOBPCG primitives.
- CSparse.NET 4.3.0 owns cached sparse Cholesky factorisation with AMD ordering and Span-based solve for SPD-intended systems.
- Local kernels exist only where dependencies do not expose the required algorithm.

## Potential Use Cases And Value

`Rasm.Vectors` is a downstream design-geometry kernel for Rhino WIP and GH2. It turns design intent into typed points, vectors, curves, meshes, frames, scalar fields, transforms, descriptors, and diagnostics through `VectorIntent.Project<TOut>(Context, Op?)`.

### Intent And Projection Rails

- Build one GH2 component family around `VectorIntent` instead of one-off commands for each vector operation.
- Expose typed dropdown modes from SmartEnums (`SupportProjection`, `CurveProjection`, `SurfaceProjection`, `MeshLaplacian`, `SampleKind`, `AlignKind`, `RemeshKind`).
- Project the same intent into alternate outputs: `Point3d`, `Vector3d`, `Plane`, `Curve`, `Polyline`, `Mesh`, scalar values, matrices, transforms, and descriptor values.
- Surface predictable `Fin<TOut>` failures in Rhino/GH UI without exceptions or silent fallback geometry.
- Share the same projection vocabulary between command plugins, GH2 components, and future app-layer tools.

### Placement, Snapping, And Support Geometry

- Place panels, fixtures, annotations, profiles, furniture-scale design objects, and facade modules onto Breps, meshes, curves, planes, and point clouds.
- Generate tangent frames, normals, signed distances, containment distances, UV values, support parameters, mesh points, and component metadata at picked locations.
- Create surface-aware handles that move objects along support geometry while preserving local frame orientation.
- Build proximity masks, clearance previews, inside/outside classifiers, and design-envelope checks from signed distance and containment projections.
- Convert selected Rhino geometry into reusable `SupportSpace` and `SurfaceSpace` inputs for downstream fields, sampling, routing, and alignment.

### Frames, Rails, And Curve-Based Design

- Generate stable section frames along rails for ribs, louvers, mullions, fins, stair strings, handrails, pipes, and ceiling baffles.
- Use Frenet, Bishop, tangent, curvature, arc-length, and parallel-transport frames to avoid orientation flips on long curves.
- Orient repeated components along paths with explicit angle pivots, signed axes, spans, cones, and vector relations.
- Build sweep-ready profile frames for facade ribs, contour-following strips, ceiling tracks, and sculptural rails.
- Evaluate curvature-driven local behavior for path smoothing, section rotation, and component spacing.

### Field-Driven Layout And Patterning

- Create attractor, repulsor, vortex, Coulomb, dipole, harmonic, saddle, helical, ring, curl-noise, and cross-product design fields.
- Turn vector fields into streamlines for circulation sketches, facade flow lines, floor inlays, ceiling tracks, and generated path curves.
- Drive aperture density, screen porosity, perforation radius, tile scale, fixture spacing, lighting density, and ornamental intensity from scalar fields.
- Combine gradients, curls, divergences, Laplacians, clamps, scales, blends, and warps into controllable design fields.
- Split fields with Hodge decomposition into gradient-like behavior and circulation-like behavior for simple UI controls.

### Implicit Massing And Soft Boolean Geometry

- Model concept solids from SDF primitives: sphere, box, capsule, cylinder, cone, capped cone, torus, hex prism, octahedron, and ellipsoid.
- Blend, union, subtract, intersect, round, onion, elongate, displace, twist, and bend implicit volumes for early massing studies.
- Generate Rhino meshes from scalar iso-surfaces for blob massing, carved voids, inflated envelopes, clearance solids, and smooth transitions.
- Use mesh signed-distance fields to preview offsets, shrink-wrap behavior, proximity coloring, and inside/outside styling.
- Route watertight mesh signing through generalized winding or signed-heat policy instead of ad-hoc point-in-solid guesses.

### Surface And Mesh Pattern Systems

- Generate facade panel directions, seam candidates, tile rotation, hatch grain, surface stripes, and anisotropic module orientation from cross-fields.
- Interpolate designer strokes over meshes with vector heat for louver direction, panel rotation, surface grain, and facade flow.
- Use tensor fields and principal curvature directions for curvature-responsive ornament, rib direction, panel alignment, and surface grain.
- Build stripe, band, contour, and wave families from scalar fields, geodesic fields, and spectral filters.
- Apply cone and hint constraints to cross-fields for controlled singularities and design-authored orientation anchors.

### Geodesic Routing And Surface Distance

- Compute heat geodesic, spectral distance, stripe distance, and geodesic-tangent behavior over mesh surfaces; true tangent log-map coordinates remain deferred.
- Route seams, cables, wayfinding marks, projected measurements, surface traces, and on-surface paths across curved forms.
- Create distance-to-source scalar previews for zoning, panel influence, local falloff, and surface-aware selection.
- Convert scalar geodesic output into contour-ready bands, isoline sources, or placement weights for downstream tools.
- Cache mesh-local factors so repeated source edits reuse the same `LaplacianCache` substrate.

### Sampling, Population, And Distribution

- Distribute anchors, panels, lights, apertures, seats, paving marks, acoustic nodes, and facade modules across mesh surfaces.
- Select Poisson disk, farthest-point, Lloyd, optimized, or capacity sampling depending on uniformity, coverage, and density goals.
- Use sampled points as seeds for field traces, panel centers, fixture locations, perforation maps, and component placement.
- Preserve deterministic sampling behavior for repeatable GH definitions and command previews.
- Combine sampling with scalar fields to turn design intensity into population density once scalar/weighted sampling lands.

### Mesh Preparation, Flattening, And Descriptors

- Prepare meshes for design workflows with topology summaries, feature edges, remeshing, reduction, unwrap, and flattening.
- Generate fabrication previews, unrolled pattern studies, panel layout sheets, and texture-coordinate working surfaces.
- Use cotangent and intrinsic-Delaunay Laplacians as selectable mesh-operator policies; robust nonmanifold Laplacian is typed unsupported until tufted cover is implemented.
- Compute spectral descriptors for shape matching, option comparison, ornament families, and similarity sliders.
- Reuse intrinsic mesh snapshots for connection Laplacian, cone holonomy, signed heat, cross-field, vector-heat, and Hodge workflows.

### Point Clouds, Alignment, And As-Built Workflows

- Align scans, imported context, reference layouts, module kits, and repeated facade parts with point, plane, symmetric, robust, and normal-weighted point-to-plane ICP.
- Extract best-fit planes, principal axes, principal frames, covariance, spread, curvature, curvedness, shape index, and oriented normals.
- Build quick design diagnostics for sampled geometry: local direction, compactness, anisotropy, footprint shape, and surface-like behavior.
- Generate hulls, rough envelopes, footprint wrappers, containment regions, and selection boundaries from point or ring inputs.
- Use robust alignment and cloud metrics as preflight checks before baking component arrays or matching as-built fragments.

### Transport, Morphing, And Layout Transfer

- Transfer point distributions between facade options, surface versions, module families, and sampled design layouts.
- Use unbalanced Sinkhorn transport to relax normalized weighted marginal constraints between alternatives.
- Morph landmark layouts, aperture maps, panel centers, fixture plans, and ornamental seed sets between design states.
- Compare alternatives by correspondence cost, transport plan structure, and distribution mismatch.
- Use transport output as a bridge from analysis-like point sets back into editable design geometry.

### Rhino/GH Product Surfaces

- `Project Intent`: single component or command for projecting `VectorIntent` into requested Rhino-native output.
- `Support Projection`: closest point, tangent, normal, signed distance, containment, UV, frame, and component projections.
- `Sample Mesh`: Poisson, farthest, optimize, Lloyd, and capacity sampling with preview and bake paths.
- `Trace Streamline`: vector-field seeds to curves or polylines with fixed/adaptive integration and termination modes.
- `Cross Field`: mesh plus hints/cones to directional panel, stripe, or tile orientation fields.
- `SDF IsoSurface`: primitive and mesh-backed scalar fields to Rhino mesh output.
- `Mesh Distance`: heat geodesic, spectral distance, stripe, and signed-distance previews.
- `Align Clouds`: scan or module point sets to transforms with residual/convergence display.
- `Transport Cloud`: remap point distributions between surfaces, options, or facade states.
- `Mesh Prep`: topology, feature edges, remesh, reduce, unwrap, flatten, and spectral descriptor workflows.

### Productization Boundaries

- App UI, preview conduits, bake commands, GH2 parameter wrappers, and user-facing receipts live outside `Rasm.Vectors`.
- Brep-heavy workflows need a canonical meshing or parameterization intake before mesh-only kernels run.
- Advanced solvers benefit from exposed convergence and cache diagnostics for designer-facing feedback.
- Cross-field cone flows need preflight guidance for topology, boundaries, and cone charge validity.
- Contour and isoline extraction from scalar fields now has a local mesh PL rail; richer plateau receipts, product contour integration, and runtime Rhino proofs remain productization work.
