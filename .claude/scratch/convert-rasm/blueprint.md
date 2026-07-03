# [BLUEPRINT] — Rasm kernel conversion: the one structure ruling

Binding rulings for the convert-rasm-kernel run. Sources: ledger-vectors-a/b, ledger-domain-analysis, ledger-boundary, ledger-ecosystem (all in this scratch dir), the 18 settled Geometry pages, the kernel index docs, `libs/.planning/architecture.md`. Backup `.archive/Rasm/{Vectors,Domain,Analysis,Geometry,.api}` verified present; the executor re-verifies before any delete.

## [00]-[HEADLINE]

- The kernel becomes an ordinary planning-scoped package: ONE `libs/csharp/Rasm/.planning/` root, eight sub-domain folders, Rasm.Bim shape. `Rasm.csproj` survives empty-source with its RhinoCommon machinery and package roster (minus three drops). 31 mature `.cs` retire; 18 settled Geometry pages re-home path-only with ZERO surgical content edits (the partition preserves every name they reference); 34 new pages re-express the Vectors/Domain/Analysis capability ground-up.
- Analysis SOURCE deletes as slated — its ledger-proven load-bearing capability re-lands compacted 14 files → 5 ground-up pages (`.planning/Analysis/`) plus two absorptions (Location → `Parametric/locate.md`, Spatial-RTree → `Spatial/neighbors.md`). Zero families silently dropped; the GH/Radyab/Overlay entry contract keeps a fence to re-enter against.
- Namespaces are CONTRACT, folders are DOMAIN: `Rasm.Domain`/`Rasm.Vectors`/`Rasm.Analysis` stay the fence namespaces of the re-expressed capability (frozen by the `[Union]` generator emit `global::Rasm.Domain.Op`, 11 Grasshopper `using Op =` aliases, Rasm.Rhino Camera members, cs-analyzer docIDs, and 162 sibling-corpus design anchors); `Rasm.Geometry.*` stays the robust-core root untouched.
- Rhino ruling: Tier-0 `libs/.planning/architecture.md` [03] stands UNAMENDED — the kernel remains RhinoCommon-aware; the pure-numeric floor becomes host-neutral-SHAPED (drops `RhinoMath` for `double.IsFinite` + owned epsilon policies) without minting a host-free assembly.
- Consumer ruling: `Rasm.Rhino` + `Rasm.Grasshopper` source stays frozen on disk but leaves the solution build surface (4 slnx rows out: 2 libs + their 2 test shells); `Rasm.csproj` + `Rasm.Tests` shell stay in. Re-entry recorded as a kernel TASKLOG card.
- Roster delta: DROP `Triangle` (pin + reference), DROP `geometry3Sharp` and `MathNet.Numerics.Providers.MKL` from the kernel csproj (pins stay — Bim/Fabrication/Compute still reference them; flagged to the ripple wave). No adds admissible; four WATCH candidates recorded as one IDEAS card.

## [01]-[THE_PARTITION]

```text
libs/csharp/Rasm/
├── .planning/
│   ├── Domain/        # ns Rasm.Domain — the kernel substrate floor (7 new)
│   │   ├── rails.md  context.md  identity.md  validation.md
│   │   ├── normalization.md  evaluation.md  stats.md
│   ├── Numerics/      # exact floor + host-neutral-shaped numerics (2 re-homed + 5 new)
│   │   ├── predicates.md*  faults.md*
│   │   ├── atoms.md  matrix.md  integrate.md  spectral.md  calculus.md
│   ├── Spatial/       # proximity, clouds, neighborhoods, transport, fields (3 + 5)
│   │   ├── index.md*  naming.md*  reconciliation.md*
│   │   ├── support.md  cloud.md  neighbors.md  transport.md  fields.md
│   ├── Parametric/    # curve/surface evaluation (1 + 2 — kills the one-file folder)
│   │   ├── curve.md*
│   │   ├── projections.md  locate.md
│   ├── Meshing/       # mesh substrate + construction lattice (4 + 3)
│   │   ├── delaunay.md*  arrangement.md*  intersect.md*  offset.md*
│   │   ├── mesh.md  dec.md  reconstruct.md
│   ├── Processing/    # algorithm pipelines (6 + 7)
│   │   ├── repair.md*  receipts.md*  decimate.md*  flatten.md*  fit.md*  solver.md*
│   │   ├── intent.md  sample.md  extract.md  flow.md  register.md  geodesics.md  segment.md
│   ├── Drawing/       # 2D producers (2 + 0)
│   │   ├── view.md*  pack.md*
│   └── Analysis/      # ns Rasm.Analysis — measured-query surface (0 + 5)
│       ├── query.md  measure.md  inspect.md  select.md  relations.md
├── .api/              # kernel overlay (api-geometry3sharp.md, api-triangle.md deleted; api-mathnet-providers.md MKL rows trimmed)
├── ARCHITECTURE.md  README.md  IDEAS.md  TASKLOG.md   # rebuilt for the new root
├── Rasm.csproj        # 3 PackageReference rows removed; otherwise intact
└── packages.lock.json # regenerated
```

`*` = re-homed settled page (content untouched; the standing RASM-CS-GEOMETRY-BRIEF campaign owns its rebuild). 34 new + 18 re-homed = 52 pages. No one-file folders; every folder is a real growth-conducive domain.

### [NAMESPACE_MAP]

Folder = domain grouping; fence namespace = frozen contract axis. The rebuilt ARCHITECTURE.md states this matrix and does NOT re-assert the stale 5-namespace law.

| Fence namespace | Pages | Why frozen |
|---|---|---|
| `Rasm.Domain` | Domain/* (7) | `[Union]` generator emit `global::Rasm.Domain.Op.Of`, `GenerateUnionOpsAttribute` marker, 11 GH `using Op =` aliases, props global usings, `ContentHash` federation seams, `Topology`/`Kind`/`Context` composed by 10+ settled pages |
| `Rasm.Vectors` | Numerics/{atoms,matrix,integrate,spectral,calculus}, Spatial/{support,cloud,neighbors,transport,fields}, Parametric/projections, Meshing/{mesh,dec,reconstruct}, Processing/{intent,sample,extract,flow,register,geodesics,segment} (21) | Rasm.Rhino Camera (`VectorIntent`/`VectorFrame`/`MotionInterpolation`), 12/18 settled pages compose `MeshSpace` + the DEC/field/cloud vocabulary, Materials/Fabrication/Element design anchors |
| `Rasm.Analysis` | Analysis/* (5), Parametric/locate (1) | cs-analyzer docIDs (`IntersectionHit`, `RayQuery`), Rasm.Rhino Commands/Overlay, GH props usings, Radyab factory calls (`Measure.*`, `Points.*`, `Bounds.*`, `AnalysisQuery`, `Analyze.Run`, `Env`) |
| `Rasm.Geometry.*` | the 18 re-homed | Settled robust-core law; geometry campaign owns its V1 namespace reconciliation |

Frozen-name law for the new pages (the reason the surgical-edit list in [07] is EMPTY): `MeshSpace`, `LaplacianCache`, `IntrinsicMesh`, `MeshAdjointSnapshot`, `MeshKernel.TopologyDetailed`, `TopologyReceipt`, `DiscreteCalculus`, `VectorIntent(.Topology/.Project<TOut>)`, `ScalarField` + case names (`Geodesic`/`MeanCurvatureFlow`/`SpectralDistance`/`Stripe`/`SignedDistanceFromMesh`), `VectorField` + case names (`CrossField`/`Hodge`/`VectorHeat`/`GeodesicTangent`/`TangentLogMap`), `SampleDetailed` (exposed, resolving pack.md's unverified-seam admission), `VectorCloud`, `VectorCloudMetric`, `CloudKernel`, `SampleKind`, `MeshSegmentation`, `Op`, `Context`, `ContentHash`, `Topology`, `Kind`, `CurveForm`, `Expected`. `MeshEdit` is owned by settled `repair.md` itself — no new page mints it.

### [PAGE_CHARTERS] — Domain/ (ns `Rasm.Domain`)

| Page | Charter | Absorbs (ledger rows) |
|---|---|---|
| `rails.md` | The kernel ROP substrate: `Op` caller-member-name operation key + full fault/acceptance factory (`Catch`/`Side` boundary-exception rail); the `Expected`/`Fault` 12-case union (band-distinct from `Rasm.Geometry` `GeometryFault` 2400 by explicit decision — two families, never merged); `Lease<T>` Owned/Borrowed resource rail (Use/Resource/Dispose/Project folds); `IValidityEvidence` + the corpus-wide validity-fold mechanism (finite-nonnegative field fold / generated aspect) that collapses the ~40-receipt hand-rolled `IsValid` swarm; `GenerateUnionOps`/`SkipUnionOps` generator contracts carried as designed vocabulary; the ONE Op-threading law: `Op` stays the explicit value key, `Eff<Env>` reader carries it in long pipelines — every page follows this, no dual paradigm. | DA:A.3 Op/IValidityEvidence/attrs/Expected+Fault; DA:A.4 `Lease<T>`; VA cross-cut receipt-swarm + Op-threading rulings |
| `context.md` | The tolerance/units substrate: `AbsoluteTolerance`/`RelativeTolerance`/`AngleTolerance` value objects; the immutable `Context` bundle with `Of(double…)`/`Of(UnitSystem)`/`Of(RhinoDoc)`/`Millimeters` validated builders + `Fractional`/`MeshIntersectionTolerance` derivations. `Of(RhinoDoc)` is the ONE doc-coupled boundary adapter; Context stays host-neutral-shaped for the federation runtimes. | DA:A.1 all |
| `identity.md` | The determinism owner: `ContentHash.Of` seed-zero XxHash128→UInt128 — THE federation content key (Element/Persistence/python/ts/rhino-bridge seams; NON-NEGOTIABLE verbatim fence) — plus the ONE splitmix64 deterministic-derivation owner (order keys, unit intervals, signed units for reproducible algorithms), collapsing the Matrix/Sample PRNG duplication. Mines `System.IO.Hashing`. | DA:A.3 ContentHash; VA Matrix `NextSignedUnit` + Sample `CandidateOrderKey`/`UnitInterval` |
| `validation.md` | The one acceptance/readiness oracle: `Requirement`/`Check` delegate-backed smart-enum readiness algebra (`ForKind` dispatch, lease-aware `RunChecks`, the full Rhino check matrix); `OpAcceptance.ValidityOf` compiled-lambda validity oracle EXTENDED by `IValidityEvidence` registration to absorb the Analysis result types — the `AnalysisAcceptance` parallel rail is killed; `TryCreateValidated` Thinktecture bridge; `OpExtensions` + `RequirementContext.Pair` two-operand combinator; absorbs `FieldNabla`'s validation/guard half as the canonical admission vocabulary (NotNull/AllFinite/Finite/Positive/Plane/Direction/Cone/Period/input guards) serving every module. | DA:A.3 Requirement/Check/OpAcceptance/OpExtensions/Pair; DA:B.1 AnalysisAcceptance (absorbed); VB Field.cs FieldNabla validation half |
| `normalization.md` | The Rhino-kind taxonomy + coercion owner: `Topology` (10) + `Kind` (21) smart enums with the frozen capability web and `Can*` predicates (the `CanEvaluateTopology`/`CanEvaluateSolidTopology` byte-identical twin collapsed to one); `CurveForm` union; the polymorphic coercion lattice (`KindOf`/`BoundsOf`/`CoerceTo<T>`/`PrimitiveOf`/`CurveForm`/`SurfaceForm`/`BrepForm`, Lease-returning tolerance-aware primitive recovery); ONE disposable projection carrier — the `TopologyProjection` superset absorbs `GeometryProjection` — with the face↔brep transfer/disposal protocol preserved. `GeometryRequest` is NOT re-minted here: the one request algebra lives in `Analysis/query.md`. | DA:A.4 Topology/Kind/CurveForm/coercion/predicates/carriers; DA request-unification + twin-collapse rulings |
| `evaluation.md` | The closest-point/evaluation lattice: `ClosestHit` 9-field receipt (`At`/`IsValid`/`Project<TOut>`/`SignedDistanceFrom`); `ClosestOf` polymorphic evaluation over Point/PointCloud/Line/Polyline/Plane/Sphere/Box/Curve/BrepFace/Surface/Brep/Mesh with normal/tangent/frame/component recovery; `NormalAt`/`FrameAt`/`SurfaceUv`/`SurfaceSampleUv`; `SamplePoints` + `CurveSampleParameters`; `SignedDistanceOf`. The substrate `support.md`, `locate.md`, and the Analysis families all compose. | DA:A.4 ClosestHit + GeometryKernel evaluation/sampling/closest |
| `stats.md` | The statistics substrate: `ScalarMetric`/`ExtremumDirection`/`StatContext` vocabulary; Welford single-pass `Stat` + the generic tolerance-banded `Extrema` fold; `Distribution` median/IQR/quantile; `SampleMoment` weighted mean/upper-triangular covariance — stays HERE by decision: cloud PCA (`cloud.md`) and Analysis spread (`select.md`) compose it; settled `fit.md`'s "cloud PCA vocabulary" is a consumer, never a second owner. | DA:A.2 all; DA ruling 7 |

### [PAGE_CHARTERS] — Numerics/ (new pages ns `Rasm.Vectors`)

| Page | Charter | Absorbs |
|---|---|---|
| `atoms.md` | The typed vector-algebra primitive floor + the PROMOTED output-projection owner: `Dimension`/`PositiveMagnitude`/`UnitInterval` value objects; `BoundarySense`/`SignedAxis`/`VectorRelation`/`AnglePivot`/`VectorAngle`; `Direction` (Snell refract, plane-sequence parallel transport), `VectorSpan`, `VectorFrame` (Bishop chain delegates to `neighbors.md`'s one RMF owner), `VectorCone` (solid angle, envelope merge, sector partition); `AtomProjection`/`ProjectionRow` promoted from hidden internal to the NAMED corpus-wide raw→typed projection dispatch every `.Project<TOut>` routes through. Finiteness via `double.IsFinite` + owned epsilon policy, not `RhinoMath`. | VA:Atoms.cs all rows (incl. ruling 6) |
| `matrix.md` | The dense/sparse/complex linear-algebra owner: `Matrix`/`SymmetricMatrix`/`SparseMatrix` (CSR invariants)/`SparseHermitian` facades over MathNet+CSparse; `CholeskySparse` Lock-guarded AMD SPD factor cache; `GaugePolicy` pin/mean-zero-deflation/KKT singular-system algebra; decomposition receipts on the rails validity fold; `MatrixKernel`: dense Svd/Lu/Qr/Cholesky/eigen, BiCgStab + typed-receipt fallback, sparse LU, `SingularGaugeSolve` (M-orthogonal projection + Tikhonov shift + `GaugeShift`), generalized-eigen Cholesky congruence, LOBPCG real+Hermitian (Knyazev; Rayleigh-Ritz, MGS reorthonormalization, Jacobi precond, deterministic basis via `identity.md`, NO hidden dense fallback). Registration's raw-MathNet bypass re-routes through these owners. Host-neutral-shaped (`double.IsFinite` + owned epsilons). Mines `CSparse` + `MathNet.Numerics`; documents OpenBLAS as the sole opt-in native accel. | VA:Matrix.cs all rows; VA Align MathNet-bypass + strata rulings |
| `integrate.md` | The ODE/RK integration floor: `IntegratorKind` 9-tableau data-driven vocabulary (Euler…DormandPrince, Fixed/Adaptive); `ButcherTableau` with numeric order-condition MOMENT validation; `DenseOutputCoefficientFamily` exact-rational continuous-extension tables + Horner eval; `ButcherDenseOutput` least-squares moment fit via `matrix.md`; the `FieldIntegrator` RK stage stepper + adaptive PI error control + dense output; ONE `Combine(coeffs,vectors)` fold (kills the Flow.cs verbatim duplicate). Pure numerics — geometry enters only at the `flow.md` consumer. | VA:Flow.cs IntegratorKind/ButcherTableau/DenseOutputCoefficientFamily/ButcherDenseOutput/FieldIntegrator + moment/dense receipts |
| `spectral.md` | The mesh-free spectral algebra: `DiscreteCalculus` DEC operator bundle (D0/D1/Star0/Star1/Star2 + transport + harmonic — the Rasm.Compute adjoint seam, name frozen); `SpectralBasis` eigenpair carrier + `Truncate`; `SpectralFilter` closed transfer-function algebra (Heat/Wave/Biharmonic/Diffusion/CommuteTime/Identity, partial-monoid `Compose`); assembly/normalization/zero-mode/distance vocabularies; `SpectralDescriptor` normalize/rank/distance algebra + descriptor receipts; `SpectralAssemblyReceipt`/`HarmonicOneFormReceipt`/`HarmonicOneFormBasis` carriers. Mesh-bound assembly lives in `Meshing/dec.md`. | VB:Spectral.cs policy enums/SpectralFilter/receipt+carrier models/descriptor eval algebra |
| `calculus.md` | The sample-anywhere analytic math floor: the differential half of `FieldNabla` promoted — 6-axis central-difference stencil; Gradient/Curl/CurlNoise/Divergence/Laplacian/StrainMagnitude over any field; `FieldNoise` procedural lattices (Perlin/simplex/skewed-simplex/Worley, canonical perm table); the weight-kernel/falloff profile math (Wendland/quintic/cosine/cubic/linear/Epanechnikov, `Falloff` family) that `fields.md` cases and `reconstruct.md` weighting compose. | VB:Field.cs FieldNabla differential ops + FieldNoise + KernelKind/Falloff math (ruling 6) |

### [PAGE_CHARTERS] — Spatial/ (new pages ns `Rasm.Vectors`)

| Page | Charter | Absorbs |
|---|---|---|
| `support.md` | The proximity boundary adapter: `SupportProjection` 14-case capability-gated closest-hit projection SmartEnum with the single `Project<TOut>` gate — the corpus model of one-enum-all-modalities; `SupportSpace` `[BoundaryAdapter]` wrapping any Rhino geometry or cloud cluster (`Of`/`Closest`/`SignedDistance`/`ContainmentDistance`/`Admits*`); composes `Domain/evaluation.md` `ClosestOf`. `SurfaceSpace` departs to `Parametric/projections.md` (welded here by filename only). | VA:Space.cs SupportProjection/SupportSpace |
| `cloud.md` | The point-cloud owner: `VectorCloud` Ring/Polyline/Cluster(+Weighted) union with lazy indexed cache + admission (tolerance dedup + mass renormalize + `OriginalToUnique`); `VectorCloudMetric` 30-case table-driven metric surface; `CloudKernel` (name frozen for settled `fit.md`) — the canonical covariance/PCA owner (composing `stats.md` `SampleMoment`), ring/polyline metrics, planar winding, mass-property shapes; the hull rail: convex 3D/footprint 2D PLUS `ConcaveOutline`/`AlphaShape` REALIZED via the admitted comp-geo cluster (MIConvexHull/SharpVoronoiLib), killing the declared-but-Unsupported dead cases; receipts on the rails fold. | VB:Cloud.cs status enums/VectorCloud/VectorCloudMetric/receipt models/admission+PCA/winding+shape/polyline metrics/hull |
| `neighbors.md` | THE one neighborhood substrate (kills the three parallel RTree wrappers): kNN/radius/graph queries with typed receipts over Rhino `RTree` (native-backed) + `Supercluster.KDTree.Net` (static point sets — this package's mining owner); neighborhood-PCA samples; Hoppe-DeRose MST normal orientation (Prim, mining `QuikGraph`); covariance normal estimation; quadric-fit principal curvature + Koenderink shape-index/curvedness + range classification; the ONE rotation-minimizing-frame owner (Wang double-reflection Bishop chains — `atoms.md` `Direction.ParallelTransport` delegates); ABSORBS the Analysis Spatial family (`SpatialProbe` Nearest/Within, `SpatialHit`/`SpatialPair`, `SpatialIndex` Points/PointCloud/MeshFaces/FromBounds, box/sphere search, overlaps, KNN/radius point-pairs) as query modalities on the same substrate. Serves cloud metrics, `register.md` correspondences, `mesh.md` power-diagram seeding. | VB:Cloud.cs neighborhoods/normals/curvature/Bishop; DA:B.2 Spatial; VB rulings 5 + RMF/parallel-transport dedup |
| `transport.md` | The optimal-transport owner: log-domain stabilized Sinkhorn (balanced + unbalanced KL-relaxed + debiased); `SinkhornPlan` typed projections (distance/receipt/correspondence/matrix/barycentric transport) via `ProjectionRow`; `CloudCorrespondence`/`Set`; marginal/scaling residual receipts; documented underflow-floor policy as a named constant. Growth: Wasserstein barycenters, Gromov-Wasserstein non-rigid matching. | VB:Cloud.cs Sinkhorn OT + CloudTransportPolicy/SinkhornReceipt/correspondences |
| `fields.md` | The implicit-field algebra: `ScalarField` ~35-case union (+operators, Lipschitz bounds, Blend-chain flattening); `VectorField` ~25-case union (+20 factories collapsed to case-family specs); `TensorField` (Curvature via `Parametric/projections` shape operator — single second-fundamental-form owner; R·M·Rᵀ transforms); `SdfKind` 12 exact analytic SDF primitives with TYPED per-primitive parameter records — the string-keyed `ImmutableDictionary` param bag is killed; `BlendKind` smin algebra with erosion factors as named policy rows; Falloff/Kernel/Noise/Ray/Bounce vocabularies referencing `calculus.md` math; the 35/25-arm `SampleScalar`/`SampleVector` dispatch with mesh-aware cases delegating to the Meshing/Processing solvers (case names frozen); the status-tagged `SampleSdfDetailed`/`SampleDetailed` sampling seam EXPOSED as public rail — resolving pack.md's admitted-unverified seam; the 58-factory construction spam collapsed into case-family specs + policy records; admission unified against the case set (one structure to keep in sync, not two). | VB:Field.cs vocab/ScalarField/VectorField/TensorField/SdfKind/sample dispatch/iso+admit (reconstruction kernels depart to `reconstruct.md`); VB rulings 2, 9 |

### [PAGE_CHARTERS] — Parametric/

| Page | Charter | Absorbs |
|---|---|---|
| `projections.md` (ns `Rasm.Vectors`) | The Rhino-native parametric evaluation selectors: `CurveProjection` 9-case + `SurfaceProjection` 13-case SmartEnums (shape operator → `matrix.md` `SymmetricMatrix`; THE single second-fundamental-form owner `TensorField` composes) + `ConeProjection` + `MotionInterpolation` quaternion pose slerp — the ONE slerp site; `intent.md` dispatch delegates here, killing the duplicate; `SurfaceSpace` `[BoundaryAdapter]` re-homed to its parametric family. Composes `Domain/evaluation.md` + `atoms.md` projection rail. | VA/VB:Modes.cs all; VA:Space.cs SurfaceSpace; VB slerp-dedup ruling |
| `locate.md` (ns `Rasm.Analysis`) | The curve/surface location algebra: `Locator` (CurveParameter/ArcLength/NormalizedMid/SurfaceParameter/ClosestTo/PerpendicularParameters); `LocationValue` (Point/Frame/Normal/Tangent/Curvature/Derivative/Parameter/Length); `Division` by-count/by-length; `CurvatureMode` samples + tolerance-banded extrema (via `stats.md` `Extrema`); orientation/containment/short-path/perpendicular-frames. Routed by `Analysis/query.md`; sited with its parametric domain. | DA:B.2 Location |

### [PAGE_CHARTERS] — Meshing/ (new pages ns `Rasm.Vectors`)

| Page | Charter | Absorbs |
|---|---|---|
| `mesh.md` | The mesh substrate owner: `MeshSpace` validated snapshot handle (frozen — 12/18 settled pages compose it); `LaplacianCache` memoization service with identity/GC/concurrency semantics preserved VERBATIM (`ConditionalWeakTable` keyed by `Mesh` reference identity + `Atom<HashMap>` success-only memos + Lock-guarded CSparse solves + `SpdMassShift` scale-derived Tikhonov); `IntrinsicMesh`/`IntrinsicEdge` frozen intrinsic triangulation + `MeshAdjointSnapshot` public cross-package handle (names frozen); THE one cotangent primitive — intrinsic edge-length and extrinsic face-geometry paths under one owner, killing the three-path duplication; cotangent/IDT/tufted-intrinsic assembly (FlipToDelaunay normal coordinates, Sharp-Crane tufted cover), `MeshLaplacian` selector, (M+tL) SPD assembly; topology diagnostics kept as `MeshKernel.TopologyDetailed` (settled receipts.md reference frozen) + `TopologyReceipt` via the public `VectorIntent.Topology` projection; signpost transport + common-subdivision overlay (partition-of-unity gated); `RestrictedPowerDiagram`/`PowerCell`/`PowerFacet` Laguerre cells restricted to mesh (Sutherland-Hodgman radical clip; sole consumer `sample.md` — the coupling preserved by decision). | VA:Mesh.cs MeshSpace/LaplacianCache/IntrinsicMesh/policies/COTANGENT_ASSEMBLY/IDT_AND_NONMANIFOLD/NORMAL_COORDINATES/SELECTION_SPD/TANGENT_FRAMES_SIGNPOST/COMMON_SUBDIVISION/POWER_CELLS/METRICS(topology); VA rulings 8, 9; VB ruling 4 |
| `dec.md` | The mesh-bound DEC assembly owner: `AssembleDecOperators` (d0 incidence/d1 curl/star0 mass/star1 cotan/star2 area, ∂∂=0 residual gate); Crouzeix-Raviart connection heat system (Hermitian-real blocks, symmetry-residual gated); CDS trivial-connection holonomy (angle defects, Gauss-Bonnet integer gate, cone 1-forms); harmonic 1-form basis (genus-dim, Star1-orthonormalized) + Hodge decomposition ω=dα+δβ+η (gauge-fixed Poisson + harmonic projection) — the DDG receipt family UNIFIED here: `HodgeDecompositionReceipt` lands beside its algorithm, healing the Spectral↔Mesh fracture; extrinsic face-gradient/divergence heat scaffold; mesh-side spectral basis computation (generalized eigen via `matrix.md`) + `SpectralBasisBundle` caching. | VB:Spectral.cs SpectralCore DEC assembly/CR/CDS/harmonic+Hodge/extrinsic scaffold; VA:Mesh.cs SPECTRAL_BASIS; VB ruling 8 |
| `reconstruct.md` | The implicit-reconstruction owner (points/mesh → field → mesh): RBF interp/approx kernel-matrix solve; MLS 4-eqn normal-constrained LSQ; Levin 2-step MLS (Brent energy-min offset/normal + ridge-regularized poly height fit); APSS Pratt-normalized algebraic sphere fit; screened-Poisson dense-lattice FEM (trilinear splat, 7-pt Laplacian, Dirichlet/screening) — the ~6 vestigial octree-era `PoissonPolicy` knobs DROPPED with standing reason; THE unified signed-heat spine (heat→divergence→Poisson→sign-calibrate) parameterized by discretization — P1 tet FEM / boundary-source CR surface / closed-surface regular grid — reuniting the `SdfMeshPolicy`/`SignedHeatTime`/`VolumeGrid*` policy vocabulary with its kernels and receipts (heals the Field↔Mesh policy-kernel-receipt fracture); the 3 mesh-SDF methods (GWN solid-angle winding, boundary signed-heat, closed volumetric signed-heat) + `SdfMeshReceipt`/`SignedHeatReceipt`/`VolumeGridReceipt`; native marching-cubes `IsoSurface` extraction + evaluator-failure receipts; volume-grid ceilings as policy rows, not consts. | VB:Field.cs reconstruction kernels (RBF/MLS/Levin/APSS/Poisson/tet-FEM) + SDF-mesh policy vocab + reconstruction/volume models; VA:Mesh.cs SDF_FROM_MESH; VB rulings 3 (SHM spine) + policy-fracture |

### [PAGE_CHARTERS] — Processing/ (new pages ns `Rasm.Vectors`)

| Page | Charter | Absorbs |
|---|---|---|
| `intent.md` | THE kernel consumer rail: `VectorIntent` union — the 32 case families re-derived against the new owners — with `Project<TOut>(Context, Op?)` preserved verbatim (frozen: Rasm.Rhino Camera + 18 settled pages); the 41-factory construction spam collapsed into bounded case-family constructors + policy records with internalized admission; the dispatch DELEGATES domain math to owners (slerp → `Parametric/projections`, mirror/projectOnto/lerp → `atoms.md` `Direction`) — it composes, never re-implements. | VB:Intent.cs all rows; VB rulings 2 + slerp dedup; VA ruling 4 |
| `sample.md` | The point-sampling owner: `SampleKind` union — Explicit/PoissonDisk(Bridson)/Farthest/Optimize/Lloyd/Capacity/Weighted/ScalarDensity/Adaptive/SampleElimination(Yuksel)/DworkVariableDensity/PowerCcvt(de Goes BNOT) — with the 22-param PowerCcvt constructor COLLAPSED into preset policy records + one advanced override (the in-lane `TuftedCoverPolicy.Default` pattern); `SampleKernel` dispatch over support/mesh/cloud/candidate domains; `PowerCcvtRun` gauge-fixed concave Newton + two-phase site motion + `Schedule.recurs` convergence, composing `mesh.md` `RestrictedPowerDiagram` + `matrix.md` `SingularSolve`/`GaugePolicy`; Dwork spatial-hash annulus sampling; ONE hexagonal-packing reference-spacing fold (kills three recomputes); deterministic order keys via `identity.md`; `PowerCcvtReceipt` and kin on the rails validity fold. | VA:Sample.cs all rows; VA rulings 4, 8 |
| `extract.md` | The extraction/projection rail: `ExtractionDomain` polymorphic ingress (`Of(object?)` over Mesh/VectorCloud/PointCloud/Brep/Surface); `ContourPolicy` plane/axis/surface-iso/mesh-scalar sectioning, native-first (CreateContourCurves/IsoCurve/TrimAwareIso/CreateSectionCurve) + the marching-triangles scalar-isoline PL kernel (dedup/stitch/branch-plateau receipts); `ExtractionProbe` + `Extraction` union (Probe/Contour/IsoSurface/Glyph/SampleGrid/StreamBundle) re-dispatched through typed `ProjectionRow` rows — the `typeof(TOut)` reflection branching KILLED; Glyph/Grid/StreamBundle policies collapsed to one sampled-extraction policy family; route/status/tolerance receipt metadata folded to one enum family; composes `sample.md` + `flow.md` + `fields.md` — the ONE sampling/extraction rail invariant. | VB:Extraction.cs all rows; VB ruling 7 |
| `flow.md` | The streamline/trace owner: `Termination` 6-stop union (StepCount/ArcLength/MagnitudeFloor/CrossSurface/RegionThreshold/LoopDetected) with dense-output root bisection event localization; stop/event vocabularies; `FlowKernel.Trace<TOut>` over any `VectorField` + the `integrate.md` stepper — fold-shaped immutable state, `MaxIterations` a policy row not a const; `ProjectTrace` → points/polyline/curve; `StreamlineTrace`/`TraceEvent` receipts on the rails fold. | VA:Flow.cs Termination/vocab/FlowKernel/DenseOutputState/StreamlineStep/StreamlineState |
| `register.md` | The registration owner: `AlignKind` 6-variant ICP dispatcher — point Umeyama-SVD / plane Chen-Medioni / symmetric Rusinkiewicz / robust MAD-Welsch IRLS / normal-weighted / generalized GICP (Mahalanobis precision field + Armijo SE(3) line search) — behind one `AlignmentPolicy` record with `Default`; correspondences via `neighbors.md` (kills the private RTree wrapper); ALL linear algebra through `matrix.md` owners (kills the raw-MathNet bypass); MAD/line-search constants as policy rows; alignment receipts on the rails fold. | VA:Align.cs all rows; VA parallel-rail rulings |
| `geodesics.md` | The on-mesh distance/transport suite: heat-method geodesic distance (Crane-Weischedel-Ovsjanikov); MMP exact window-propagation geodesics (Surazhsky-Surazhsky); tangent log maps — vector-heat approximate / exact straightest-exp / exact window-propagation; exp map + geodesic BVP backtrace + straightest tracing; Sharp-Soliman-Crane vector-heat parallel transport; implicit mean-curvature flow; `GeodesicTracePolicy`/`WindowPropagationPolicy` records with `Default` presets; all over `mesh.md` caches + `dec.md` systems; `TangentLogMapReceipt` and kin on the rails fold. | VA:Mesh.cs HEAT_METHOD/MEAN_CURVATURE_FLOW/VECTOR_HEAT/GEODESIC_TANGENT/TANGENT_LOG_MAP/WINDOW_PROPAGATION/BACKTRACE_GEODESIC_BVP/STRAIGHTEST_GEODESIC_EXP + trace policies |
| `segment.md` | The spectral shape-analysis + restructure owner: `MeshDescriptor` HKS/WKS via `spectral.md` filters; spectral distance; sampling-spectrum blue-noise validation (feeds `sample.md`); dihedral/curvature feature-edge detection (`MeshFeaturePolicy`/`FeatureReceipt`); `MeshSegmentation` 6-algorithm union (threshold/bands/region-grow/descriptor-clusters/watershed/normalized-cut spectral clustering); Knöppel globally-optimal cross-fields (cone/constraint variants) + stripe patterns — the direction-field pipeline segmentation and remesh guides consume; the RhinoCommon-native restructure tier as host capture per Tier-0 [03]: QuadRemesh/Reduce (`QuadTarget`/`RemeshKind` unions, guide influence) + native LSCM unwrap/flatten — the host-capture counterparts to the settled robust `decimate.md`/`flatten.md` owners, coexisting by law. | VA:Mesh.cs DESCRIPTORS/SEGMENTATION/MESH_SPECTRUM_SAMPLING/CROSS_FIELD/STRIPE_PATTERN/METRICS(features+flatten)/REMESH + MeshDescriptor/MeshSegmentation/QuadTarget/RemeshKind/MeshFeature vocab |

### [PAGE_CHARTERS] — Analysis/ (ns `Rasm.Analysis`)

| Page | Charter | Absorbs |
|---|---|---|
| `query.md` | The measured-query runtime — the kernel's public analysis entry: the ONE request algebra, `AnalysisQuery` union (23 case families, name frozen — Radyab/GH bind it) ABSORBING Domain's `GeometryRequest` 11 geometry-request cases as its geometry band (kills the two-vocabulary split); `Operation<TGeometry,TOut>` Build/Reject/Service algebra with `Prepare` cancellation+requirement gating and `Apply` → `Eff<Env,Seq<TOut>>`; `Env` reader record (Context+IProgress+CancellationToken, frozen — GH Binding constructs it); `Analyze` Scope/From(RhinoDoc)/In/Run/Query facade (frozen — Rasm.Rhino Commands/Overlay bind it); `AnalysisOutput<TOut>` One/Many/Objects/Unsupported projection with acceptance DELEGATING to `Domain/validation.md`'s one oracle. | DA:B.1 all (AnalysisQuery/Analyze/Operation/Env/AnalysisOutput); DA rulings 1, 2, 6 |
| `measure.md` | The metrology owner: `Measure` union (Length/SpatialMidpoint/Area/Volume/MassError/Centroid/CentroidError/Radii/PrincipalAxes/Inertia/InertiaProducts) over `MassKind` compute/aggregate delegates + `MassProperty` projections — the full Length/Area/Volume MassProperties capture; `Bounds` union (AxisAligned/Oriented/Transformed/Principal/Center/Corners/Edges/Area/Volume/Diagonal/AspectRatio/Tightness/EnclosingSphere/Circle/Cylinder) with Ritter enclosing fits + principal-frame OBB; `ConformanceMetric` residual sampling (Distance/Rms/WithinTolerance/Summary/Maximum/SignedResidual/Containment/Distribution) via `stats.md` + `support.md` distances + `RequirementContext.Pair`. | DA:B.2 Measure/Bounds/Conformance |
| `inspect.md` | The diagnostics owner: `Topologies` union (Kind/Domains/SolidOrientation/Components/ContainsPoint/Scalar) + `TopologyScalar` (Manifold/Euler/BoundaryLoops/Genus/HoleCount/Face/Edge/VertexCount) with genus/Euler component-count folds over Brep and Mesh; `Meshes` union (Validity/Counts/Defects/Quality/FaceQuality/FaceShape/AtVisiblePolygon/VisiblePolygonCount/NakedEdges/Outline) over `Mesh.Check` + `MeshCheckParameters` full defect capture, the 30+ `MeshSampleKind` quality samples, `MeshMetric`/`MeshFaceShape` ngon-aware face metrics via `cloud.md` ring metrics. | DA:B.2 Topology/Mesh |
| `select.md` | The selection/extraction owner: `Curves` union (All/Boundary/NakedOuter/NakedInner/Interior/NonManifold/OuterLoop/InnerLoop/Segments/Iso/Silhouette/Draft/At/Form) + `CurveFeature` (14) over the Brep/Mesh/SubD/Surface edge taxonomy + `Silhouette.Compute`/draft-curve capture (the host-native tier beside settled `view.md`); `Faces` union (All/Top/Bottom/At) axis-ranked face decomposition + projections; `Points` union (Quadrants/Extrema/EdgeMidpoints/Vertices/ControlPoints/Spread) + `SpreadAspect` (Frame/PrincipalFrame/Distribution/Collinear/Coplanar) PCA spread via `stats.md` `SampleMoment` + `matrix.md` eigen. | DA:B.2 Curves/Faces/Points |
| `relations.md` | The pairwise-relation owner: the 25-row RhinoCommon-native intersection lattice (line/plane/circle/sphere/box/curve/surface/brep/mesh/ray, trim-aware RayShoot, `MeshIntersectionCache`) as table rows; `IntersectionKind`/`Tangency` enrichment (via `intent.md` Relation); `IntersectionHit` union + `IntersectionResult` + `RayQuery` (names frozen — cs-analyzer docIDs); `CurveDeviation`; self-intersection; `Classification` — the Rhino-parametric altitude the settled predicate-exact `Meshing/intersect.md` explicitly never owns; the two coexist per the Tier-0 capture law. | DA:B.2 Intersect; DA ruling 8 |

### [AUTHOR_UNITS] — dependency-ordered; each unit = one author → merged hostile review

| # | Unit | Pages | Rationale |
|---|---|---|---|
| 1 | `domain-substrate` | rails, context, identity, validation | The floor nothing compiles without; rails legislates the Op-threading + validity-fold laws every later page obeys |
| 2 | `domain-geometry` | normalization, evaluation, stats | The Rhino taxonomy/evaluation/statistics substrate over unit 1 |
| 3 | `numerics-floor` | atoms, matrix, integrate, spectral, calculus | The host-neutral-shaped numeric floor; atoms carries the projection rail all `.Project<TOut>` use |
| 4 | `spatial-clouds` | support, cloud, neighbors, transport, fields | Proximity + cloud + the one kNN substrate + OT + the field algebra |
| 5 | `meshing-substrate` | mesh, dec, reconstruct | MeshSpace/caches/intrinsic + DEC assembly + reconstruction/SHM spine |
| 6 | `parametric` | projections, locate | Curve/surface evaluation selectors + location algebra |
| 7 | `processing-solvers` | register, geodesics, segment | The algorithm suites over units 3-6 |
| 8 | `processing-rail` | sample, extract, flow, intent | The sampling/extraction/trace rail; intent LAST — it dispatches into every owner |
| 9 | `analysis` | query, measure, inspect, select, relations | The measured-query surface; query composes the whole corpus |

## [02]-[THE_RHINO_RULING]

RhinoCommon REMAINS the kernel's compile surface. Tier-0 `libs/.planning/architecture.md` [03] (the C# branch is RhinoCommon-aware, the kernel included) stands UNAMENDED — no Tier-0 edit enters the ripple sweep for this ruling.

- COMPILE MACHINERY: `Rasm.csproj` keeps `CspScope=Domain`, the `Rhino`/`Rhino.FileIO`/`Rhino.Geometry`/`Rhino.Geometry.Intersect` + `Rasm.Domain` global usings, and every surviving PackageReference. The root `Directory.Build.props:46` `IsRhinoCommonAwareProject` clause still matches the source-less kernel dir; `RhinoCommonReferencePath` (`props:155-182`) + `VerifyRhinoHostBundle` (`targets:20-26`) stay untouched — an empty compile resolves the `<Reference>` harmlessly. Zero MSBuild edits for the kernel.
- BOUNDARY_LAW HOLDS (api-rhino.md:168-172): the kernel reads `Rhino.Geometry` VALUE structs + Mesh/Curve/Brep reference geometry + `Rhino.Geometry.Intersect`, composed through the kernel vocabulary and re-emitted at the seam. It never reaches `RhinoDoc`/`RhinoApp`/`RhinoView`/`DisplayConduit`/`ObjectTable`. The TWO doc-facing adapters are named and bounded: `Context.Of(RhinoDoc)` (`Domain/context.md`) and `Analyze.From(RhinoDoc)` (`Analysis/query.md`) — thin boundary adapters, never core dependencies.
- HOST-NEUTRAL-SHAPED NUMERIC FLOOR: the pure-numeric pages (`matrix.md`, `integrate.md`, `spectral.md`, `calculus.md`, the `atoms.md` value objects) replace `RhinoMath.IsValidDouble`/`SqrtEpsilon`/`ZeroTolerance` with `double.IsFinite` + owned epsilon policy constants. This resolves the vectors-a strata violation WITHIN the standing law: the floor becomes portable by inspection while the assembly stays RhinoCommon-aware. No host-free assembly is minted — per Tier-0 [03] a host-neutral owner exists only for a genuinely non-Rhino consumer, and Matrix/Butcher have none (Compute consumes wire arrays; Python owns its own numerics).
- GENUINELY RHINO-BOUNDARY SURFACES (rich host capture, never thinned): `Domain/normalization.md` + `evaluation.md` (coercion/closest over RhinoCommon types), `Domain/validation.md` (the Check matrix + compiled `IsValid` lambdas), `Spatial/support.md` (`SupportSpace`), `Parametric/projections.md` (`Curve/Surface` evaluation), `Spatial/neighbors.md` (Rhino `RTree` backing), `Processing/extract.md` (native contouring) + `segment.md` (QuadRemesh/Reduce/Unwrap), `Meshing/reconstruct.md` (native marching cubes), and the whole `Analysis/` folder (`Intersection.*`, `*MassProperties`, `Mesh.Check`, `Silhouette.*` — the folder's asset IS the coupling).

## [03]-[THE_CONSUMER_DISPOSITION]

One explicit action per compile consumer:

| Consumer | Action |
|---|---|
| `libs/csharp/Rasm/Rasm.csproj` | KEEP — planning-scoped empty-source package (Rasm.Bim shape). Slnx row `/libs/csharp/Rasm/` STAYS. Roster edits per [06]; `packages.lock.json` regenerated via `dotnet restore` after the edit. |
| `libs/csharp/Rasm.Rhino/` (32 .cs, frozen) | Source UNTOUCHED on disk; csproj intact. Slnx row `/libs/csharp/Rasm.Rhino/` REMOVED from `Workspace.slnx` — with `Rasm.Vectors`/`Rasm.Analysis`/`Rasm.Domain` retired it cannot compile (VectorIntent/VectorFrame/MotionInterpolation in Camera, Analyze/AnalysisQuery in Commands+Overlay, `[Union]`→`Rasm.Domain.Op` generator emit). It joins the planning-phase non-compiling majority. |
| `libs/csharp/Rasm.Grasshopper/` (16 .cs, frozen) | Source UNTOUCHED; csproj intact. Slnx row `/libs/csharp/Rasm.Grasshopper/` REMOVED — 11 `using Op = Rasm.Domain.Op;` aliases + props-injected `Rasm.Domain`/`Rasm.Analysis` usings + `[Union]` generator emit are hard edges against retired source. |
| `tests/csharp/libs/Rasm/Rasm.Tests.csproj` | KEEP + slnx row STAYS — spec-free AssayTestShell referencing the surviving (empty) `Rasm.csproj`; compiles green. |
| `tests/csharp/libs/Rasm.Rhino/`, `tests/csharp/libs/Rasm.Grasshopper/` | Slnx rows REMOVED — shells reference now-unbuildable targets. Csproj files stay on disk untouched. |
| `apps/grasshopper/Radyab/Radyab.csproj` | Slnx row REMOVED — it composes `Rasm.Grasshopper` + `AnalysisQuery` factories; un-compilable by the same edges. Source untouched. |
| `tools/cs-analyzer` | UNTOUCHED. `UnionOpsGenerator.cs:95` emit (`global::Rasm.Domain.Op.Of`) + `:31` marker stay valid: `rails.md` carries `Op` + `GenerateUnionOpsAttribute` as designed contracts with those exact names. The 4 `Kernel/Vocabulary.cs` docID strings stay valid: `OpAcceptance`/`ClosestHit` land in `validation.md`/`evaluation.md`; `IntersectionHit`/`RayQuery` land in `relations.md` — all names preserved. |
| Re-entry record | The rebuilt `libs/csharp/Rasm/TASKLOG.md` gains one BLOCKED card: `[HOST_BOUNDARY_REENTRY]-[BLOCKED]` — restore the `Rasm.Rhino` + `Rasm.Grasshopper` + their test-shell + `Radyab` slnx rows when kernel realization lands compiled `Rasm.Domain`/`Rasm.Vectors`/`Rasm.Analysis` types; anchors: the four removed row paths + the frozen-name law in blueprint [01]. |

Evidence basis: every other csharp package is already source-less and green-in-solution as an empty compile; keeping un-compilable rows would redden the whole solution build surface the planning phase gates on. `Rasm.Element`'s slnx row (planning-scoped, referenced by Rasm.Bim) is the precedent for empty packages staying IN.

## [04]-[DOMAIN_RULINGS]

`Domain/Geometry.cs` — the harshest-scrutiny verdict: NOT naive (9/10 member craft, refuted with evidence), but a god-file fusing four owner categories and a 40-member six-concern `GeometryKernel`. Disposition:
- CARRY (earned): the `Topology`/`Kind` taxonomy + capability web, the coercion lattice, `CurveForm`, `Lease<T>`, `ClosestHit` + the `ClosestOf` evaluation lattice, the sampling helpers, ONE projection carrier.
- COLLAPSE: `CanEvaluateTopology`/`CanEvaluateSolidTopology` byte-identical twin → one predicate (`normalization.md`); `GeometryProjection` → absorbed by the `TopologyProjection` superset carrier; `GeometryRequest` → absorbed into the `query.md` request algebra (never re-minted as a second ADT).
- SPLIT: vocabulary+coercion+carriers → `normalization.md`; evaluation+closest+sampling → `evaluation.md`; `Lease<T>` → `rails.md`. The `GeometryKernel` god-static dies; each page owns one dense polymorphic surface.
- `Validation.cs` splits four ways as ruled: `rails.md` (Op/Fault/attrs) + `validation.md` (Requirement/Check/acceptance) + `identity.md` (ContentHash — the mis-home corrected). `Domain.Fault` vs `Rasm.Geometry` `GeometryFault` band-2400: EXPLICITLY two families — kernel-substrate faults vs robust-core geometry faults; `faults.md` (settled) and `rails.md` each state the seam, neither absorbs the other.
- `Stats.cs`/`Context.cs` improve in place into `stats.md`/`context.md` per their charters; `SampleMoment` stays in stats by decision (ruling 7 of the domain ledger).

## [05]-[ANALYSIS_DISPOSITION]

Analysis/ DELETE CONFIRMED — all 14 `.cs` retire, no 1:1 page conversion, the `Analysis` source folder dies. The ledger proves every family load-bearing (the "capability notes" clause), so the capability re-lands compacted under the zero-loss gate: 14 files → 5 ground-up pages (`.planning/Analysis/`: query, measure, inspect, select, relations) + 2 absorptions (Location → `Parametric/locate.md`; Spatial-RTree → `Spatial/neighbors.md` as query modalities on the one neighborhood substrate). Per-family map:

| Family | Disposition |
|---|---|
| Runtime (AnalysisQuery/Operation/Env/Analyze/AnalysisOutput) | CONVERT → `query.md` (the frozen GH/Radyab/Overlay contract keeps a fence) |
| Measure, Bounds, Conformance | CONVERT → `measure.md` |
| Topology, Mesh | CONVERT → `inspect.md` |
| Curves, Faces, Points | CONVERT → `select.md` |
| Intersect (Rhino-parametric) | CONVERT → `relations.md` (settled `intersect.md` explicitly never owns this altitude) |
| Location | CONVERT → `Parametric/locate.md` |
| Spatial (RTree) | ABSORB → `Spatial/neighbors.md` (kills the parallel spatial rail; capability preserved as modalities) |
| AnalysisAcceptance oracle | ABSORB → `Domain/validation.md` one-oracle law |

Nothing in Analysis is silently dropped. The robust-core Geometry pages are confirmed disjoint (no settled page imports `Rasm.Analysis`); the delete touches only frozen host source (ruled in [03]) and zero cs-analyzer compile edges.

## [06]-[ROSTER_DELTA]

From ledger-ecosystem, kernel-scoped. Apply to `Rasm.csproj`, `Directory.Packages.props`, kernel `.api/`, and the rebuilt README [02]:

| Action | Package | Mechanics | Owner/reason |
|---|---|---|---|
| DROP (pin + ref) | `Triangle@0.0.6-Beta3` | Remove `Rasm.csproj:30` (`Aliases="TriangleNet"`) + `Directory.Packages.props:78` pin (kernel is the SOLE consumer) + delete `libs/csharp/Rasm/.api/api-triangle.md` + README row | Shewchuk non-commercial encumbrance, 2016-dead; settled `delaunay.md` authors Bowyer-Watson CDT; Ruppert refinement is a delaunay growth item, never an encumbered admission |
| DROP (kernel ref) | `geometry3Sharp@1.0.324` | Remove `Rasm.csproj:27` + delete `libs/csharp/Rasm/.api/api-geometry3sharp.md` + README row. Pin STAYS in props — `Rasm.Bim.csproj:22` + `Rasm.Fabrication.csproj:17` still reference it; their drops are flagged to the ripple wave as sibling-roster debt (ecosystem verdict: archived/dead, no consumer should carry it) | Archived DMesh3 (2019); the kernel authors its own mesh substrate; WATCH geometry4Sharp |
| DROP (kernel ref) | `MathNet.Numerics.Providers.MKL@6.0.0-beta2` | Remove `Rasm.csproj:11` + trim the MKL rows from `libs/csharp/Rasm/.api/api-mathnet-providers.md` + README row. Pin STAYS — `Rasm.Compute.csproj:21` references it; Compute's arm64 mismatch flagged to the ripple wave | Intel MKL is x86-64-only — cannot load on the osx-arm64 target; OpenBLAS stays the sole opt-in native accel, documented by `matrix.md` |
| ADD | none | No admissible add exists today (every finalist verified on feed + license + arm64 gates) | — |
| WATCH (IDEAS card) | `geometry4Sharp` (admit on first NuGet), `CSparse.Extensions` (managed iterative solvers, on first NuGet), `TVGL` (if repackaged current), `Kemsekov.GraphSharp` (optional graph enrichment) | One card in the rebuilt kernel `IDEAS.md` | — |
| KEEP + mining owners | `CSparse`, `MathNet.Numerics`, `MathNet.Numerics.Providers.OpenBLAS` → `matrix.md`; `TYoshimura.DoubleDouble`, `ExtendedNumerics.BigRational`, `PeterO.Numbers` → settled `predicates.md` (+ `matrix.md`/`integrate.md` 106-bit accumulation); `System.Numerics.Tensors` → `matrix.md`/`calculus.md`/`fields.md`; `System.IO.Hashing` → `identity.md`; `GShark` → settled `curve.md`; `Supercluster.KDTree.Net` → `neighbors.md`; `LibTessDotNet` → settled `view.md`/`pack.md`; `Clipper2`, `CavalierContours`, `SharpVoronoiLib`, `MIConvexHull` → settled `offset.md`/`delaunay.md` + `cloud.md` (hull realization); `QuikGraph` (substrate) → `neighbors.md` (MST) + `segment.md` (cuts); `CommunityToolkit.HighPerformance` (substrate) → span substrate corpus-wide | Each categorical-best or unmatched | — |

## [07]-[MOVE_DELETE_MANIFEST]

Execution order is backup-gate → moves → deletes → manifest edits → index-doc rebuild. `.archive/Rasm/{Vectors,Domain,Analysis,Geometry,.api}` MUST be re-verified (file counts: Vectors 13+`_ARCHITECTURE.md`, Domain 4, Analysis 14) before any delete.

MOVES (git mv, content byte-identical — 18 pages):
- `libs/csharp/Rasm/Geometry/.planning/Numerics/{predicates,faults}.md` → `libs/csharp/Rasm/.planning/Numerics/`
- `libs/csharp/Rasm/Geometry/.planning/Spatial/{index,naming,reconciliation}.md` → `libs/csharp/Rasm/.planning/Spatial/`
- `libs/csharp/Rasm/Geometry/.planning/Meshing/{delaunay,arrangement,intersect,offset}.md` → `libs/csharp/Rasm/.planning/Meshing/`
- `libs/csharp/Rasm/Geometry/.planning/Processing/{repair,receipts,decimate,flatten,fit,solver}.md` → `libs/csharp/Rasm/.planning/Processing/`
- `libs/csharp/Rasm/Geometry/.planning/Drawing/{view,pack}.md` → `libs/csharp/Rasm/.planning/Drawing/`
- `libs/csharp/Rasm/Geometry/.planning/Parametric/curve.md` → `libs/csharp/Rasm/.planning/Parametric/`

SURGICAL EDITS to the 18 moved pages: NONE — the partition preserves every referenced name (the frozen-name law, blueprint [01]). An executor finding a broken reference reports it as a residual, never silently edits settled content.

DELETES (after moves + backup gate):
- `libs/csharp/Rasm/Geometry/` — the emptied tree (nothing but `.planning/` lived under it)
- `libs/csharp/Rasm/Analysis/` — 14 `.cs`
- `libs/csharp/Rasm/Domain/` — 4 `.cs`
- `libs/csharp/Rasm/Vectors/` — 13 `.cs` + `_ARCHITECTURE.md` (its architecture facts are superseded by the new pages + rebuilt index docs)
- `libs/csharp/Rasm/.api/api-triangle.md`, `libs/csharp/Rasm/.api/api-geometry3sharp.md`

MANIFEST EDITS:
- `libs/csharp/Rasm/Rasm.csproj`: remove lines 11 (`MathNet.Numerics.Providers.MKL`), 27 (`geometry3Sharp`), 30 (`Triangle`). Everything else — including the `Rasm.Domain` + Rhino global usings — stays.
- `Directory.Packages.props`: remove line 78 (`Triangle`) only.
- `Workspace.slnx`: remove the four folder/project rows `/libs/csharp/Rasm.Rhino/`, `/libs/csharp/Rasm.Grasshopper/`, `/tests/csharp/libs/Rasm.Rhino/`, `/tests/csharp/libs/Rasm.Grasshopper/`, and `/apps/grasshopper/Radyab/` (five rows total per [03]).
- `libs/csharp/Rasm/packages.lock.json`: regenerate (`dotnet restore` on the kernel project).
- `libs/csharp/Rasm/.api/api-mathnet-providers.md`: trim to OpenBLAS-only (kernel tier; the shared `libs/csharp/.api/` copy still serves Compute and is untouched).

INDEX-DOC REBUILD (kernel root, this conversion's ripple wave):
- `README.md`: re-route ALL 52 pages (`.planning/<Sub>/<page>.md` paths — the unrouted `Parametric/curve.md` ENTERS the router), package sections updated per [06].
- `ARCHITECTURE.md`: new codemap = the eight-folder tree above with every node named; `[02]-[SEAMS]` re-anchored from `Geometry/...` to `.planning/...` paths and extended with the mature seams (`Domain/identity → Rasm.Element`/python/ts, `spectral.DiscreteCalculus ⇄ Rasm.Compute`, `intent/fields → Rasm.Rhino Camera` recorded as dormant host edges); `[03]` namespace law replaced by the [01] folder/namespace matrix — the stale 5-namespace law is NOT re-asserted.
- `TASKLOG.md`: keep `[T-BOOLEAN-NATIVE-ASSET]-[BLOCKED]`; add `[HOST_BOUNDARY_REENTRY]-[BLOCKED]` per [03].
- `IDEAS.md`: add the WATCH-packages card per [06].

DOWNSTREAM RE-ANCHOR (flagged to the ripple sweep, not executed here): `RASM-CS-GEOMETRY-BRIEF.md` path anchors re-map `Geometry/.planning/` → `.planning/`, and its `assay api over restored Rasm.Vectors` verification spine re-anchors to `.api` catalogues + the new Vectors/Domain page fences; the 162 sibling-corpus `Rasm.Vectors`/`Rasm.Domain` design anchors remain VALID (namespaces preserved) and need no edit.

## [08]-[COVERAGE_AND_DROPS]

Every ledger capability row maps to a page above ([01] absorb columns) — the zero-loss verify gate checks fences against the vectors-a ZERO-CAPABILITY-LOSS checklist + the vectors-b anchors + the domain-analysis family table. Explicit non-page dispositions (the complete drop/absorb register):

| # | Item | Disposition + standing reason |
|---|---|---|
| D1 | `PoissonPolicy` octree-era knobs (FullDepth/CgDepth/KernelDepth/Confidence/ConfidenceBias/LinearFit/PrimalGrid) | DROP — dead parameterization unused by the dense-lattice implementation; `reconstruct.md` parameterizes the real lattice policy only |
| D2 | `GeometryRequest` standalone ADT | ABSORB into the `query.md` request algebra — one request vocabulary; every case preserved |
| D3 | `GeometryProjection` carrier | ABSORB into the one projection carrier (`TopologyProjection` superset) — transfer protocol preserved |
| D4 | `AnalysisAcceptance.ValidityOf` | ABSORB into `OpAcceptance` via `IValidityEvidence` registration — one validity oracle |
| D5 | `GeometryKernel.CanEvaluateSolidTopology` | COLLAPSE — byte-identical twin of `CanEvaluateTopology`; one predicate survives |
| D6 | `CloudHullKind.ConcaveOutline`/`AlphaShape` Unsupported stubs | REALIZE (not drop) in `cloud.md` via the admitted comp-geo cluster |
| D7 | Duplicate kernels: splitmix64 ×2, cotangent ×3, RTree wrapper ×3, `Combine` ×2, slerp ×2, hex-spacing ×3 | COLLAPSE into single owners: `identity.md`, `mesh.md`, `neighbors.md`, `integrate.md`, `projections.md`, `sample.md` |
| D8 | `Triangle` + `geometry3Sharp` package capability | DROP — zero mature-source members mined (no ledger row cites either); removal loses nothing |
| D9 | Analysis SOURCE files (14) + Domain SOURCE files (4) + Vectors SOURCE files (13) + `_ARCHITECTURE.md` | DELETE — capability fully re-expressed per [01]/[05]; backup in `.archive/Rasm` |
