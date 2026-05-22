# [ROADMAP] Rasm.Vectors Refactor Blueprint

> Binding contract for all PRs touching `libs/csharp/Rasm/Vectors/*`. Each new primitive cites its `[5.x]` section. Deviations require updating this document first.

---

## [0] Context

The folder owns six polymorphic surfaces today (`Atoms`, `Space`, `Cloud`, `Field`, `Intent`, `Matrix`). Twenty-three audit-ranked additions land as disciplined extensions to every existing file plus **two new files** — `Mesh.cs` (discrete-geometry computational substrate) and `Population.cs` (cloud/mesh-population operators that sit above field algebra). The single consumer entry remains `VectorIntent.Project<TOut>(Context, Op?)`; no new file introduces a parallel consumer surface.

The audit's five categorical gaps land as follows:

| [GAP] | [HOME] |
| --- | --- |
| Discrete operators on meshes | `Mesh.cs` (new) + `Field.cs` cases that delegate |
| SE(3) algebra | `Atoms.cs` (Quaternion, DualQuaternion, MotionInterpolation) |
| Implicit modelling | `Field.cs` (SDF Primitive catalog, smooth BlendKind on CsgKind, Noise, CurlNoise) |
| Surface-side primitives | `Atoms.cs` (SurfaceProjection, SurfaceSpace) + `Mesh.cs` (SurfaceParameterization) + `Field.cs` (TensorField) |
| Population primitives | `Population.cs` (Hull / Sampling / Registration / Remesh kernels) — reachable only via `VectorIntent` |

---

## [1] Operating Principles

| [#] | [PRINCIPLE] | [APPLICATION] |
| :-: | --- | --- |
| 1 | One polymorphic surface per file | Cases extend existing `[Union]`/`[SmartEnum]`; do not create sibling unions for the same concern |
| 2 | Single consumer entry point | All user-facing capability surfaces as a `VectorIntent` case. Sub-surface factories that **construct primitives** (`ScalarField.Geodesic`, `VectorField.ParallelTransport`, `VectorCloud.Ring`) are public; sub-surface methods that **compute outputs** (`MeshLaplacian.Of`, `SurfaceParameterization.Compute`, `Registration.Align`) are `internal` and reachable only through a `VectorIntent` case |
| 3 | Cases not classes | Variants are `[Union]` records with private base constructors; capability catalogs are `[SmartEnum<int>]` with `UseDelegateFromConstructor` |
| 4 | Unified rails (Op, Context, Fin\<T\>) | Every public entry matches `Of(...payload, Context, Op? key = null) → Fin<T>`; failure flows through `Fault`, never exceptions |
| 5 | Boundary validates, internals trust | `[ValueObject<T>]`/`[BoundaryAdapter]` at constructors; downstream code reads validated state without re-checking |
| 6 | Cache at the space, not the call | Per-instance precomputation lives on the space wrapper (`MeshSpace`, `SurfaceSpace`) via `ConditionalWeakTable` |
| 7 | One `*Kernel` per file | Cross-cutting math sits in the file's single static kernel class; no `Helpers/`, `Utils/`, `Common/` proliferation |
| 8 | Greenfield breakage | Existing signatures stay only on merit; `Termination`, `CsgKind`, and `ShouldStop` will break case counts and arity without `[Obsolete]` shims |
| 9 | Dispatch via `Switch`, not `if`/`switch` | Every `[Union]` reads via `Switch(state, ...handlers)`; every `[SmartEnum]` reads via `UseDelegateFromConstructor` |
| 10 | Concept density triggers collapse | When ≥3 parallel cases share a payload field, lift it onto the union root; when ≥3 SmartEnums share a capability predicate, merge into one |
| 11 | Numerical reproducibility | All floating-point reductions inside `MeshKernel` / `PopulationKernel` / `MatrixKernel` use ordered fold (Kahan compensated summation); non-deterministic parallel reduction is disallowed in any spectral solver |

---

## [2] Folder Architecture

### [2.1] File Layout (target)

| [#] | [FILE] | [STATUS] | [OWNS] |
| :-: | --- | --- | --- |
| 1 | `Atoms.cs` | extend | scalar VOs · `Direction`/`VectorSpan`/`VectorFrame`/`VectorCone` · `CurveProjection`/`ConeProjection`. **Add**: `Quaternion`, `DualQuaternion`, `MotionInterpolation` SmartEnum, `SurfaceProjection` SmartEnum, `SurfaceSpace` wrapper (cached curvature samples), `CurveProjection.RotationMinimizing` case |
| 2 | `Matrix.cs` | extend | dense `Matrix`/`SymmetricMatrix` + decompositions. **Add**: `SparseMatrix` (real CSR) + `SparseHermitian` (complex CSR for connection Laplacians) + sparse Cholesky + sparse LDLᵀ + LOBPCG smallest-eigenpair solver |
| 3 | `Space.cs` | unchanged | `SupportSpace` capability wrapper (no structural additions — `MeshSpace` lives in `Mesh.cs`, `SurfaceSpace` in `Atoms.cs`) |
| 4 | `Cloud.cs` | extend | `Ring`/`Polyline`/`Cluster` cases + 23-case metric catalog. **Add**: `VectorCloud.OrientNormals(method)` factory (MST-based for ICP point-to-plane prerequisite) |
| 5 | `Field.cs` | extend | `VectorField`/`ScalarField` unions + 9-method integrator. **Add**: SDF `Primitive` cases on `ScalarField` (with per-case Lipschitz metadata), smooth `BlendKind` payload on `CsgKind`, `Noise` / `CurlNoise` cases, `Geodesic` / `ParallelTransport` / `CrossField` / `MeanCurvatureFlow` cases (delegate to MeshKernel), `TensorField` sibling union, three new `Termination` cases (`CrossSurface`, `EnterRegion`, `LoopDetected`) — `ShouldStop` signature breaks to `(StreamlineState, Vector3d currentSample)` |
| 6 | `Intent.cs` | extend | `VectorIntent` — single consumer entry. **Add**: `Tensor`, `MeshOperator`, `Surface`, `Flatten`, `Hull`, `Sample`, `Register`, `Remesh`, `Transport`, `Pose`, `Topology`, `Features`, `Descriptor` cases |
| 7 | `Mesh.cs` | **NEW** | `MeshSpace` (cached `LaplacianCache` + `MeshIntersectionCache`) · `MeshLaplacian` SmartEnum (Cotangent / IntrinsicDelaunay / Nonmanifold) · `MeshKernel` static substrate: Laplacian assembly, signpost-driven IDT preprocessor, tufted-cover nonmanifold lifting, heat-method geodesic, vector-heat parallel transport, cross-field eigensolve (LOBPCG on Hermitian connection Laplacian), BFF parameterization, mean-curvature flow, discrete exterior calculus (gradient/divergence/curl), Euler characteristic / genus, feature-edge detection |
| 8 | `Population.cs` | **NEW** | `HullKind` (Convex / Alpha / Chi) · `SamplingKind` (PoissonDisk / FarthestPoint / FarthestPointOptimization / Lloyd / CCVT) · `RegistrationKind` (PointToPoint / PointToPlane / Symmetric / Robust) · `RemeshKind` (Isotropic / Quad / Simplify / AdaptiveCurvature) · `PopulationKernel` substrate (Quickhull, Edelsbrunner alpha-complex, FPO blue-noise, Procrustes SVD alignment, Botsch-Kobbelt local-ops remesh, Jakob-Tarini quad integration, Garland-Heckbert QEM). Consumes `Field.cs` (CrossField for Quad, ScalarField for SDF-region sampling) and `Mesh.cs` (MeshSpace + MeshKernel). Surfaces only through `VectorIntent` |

### [2.2] Dependency Graph

```
                    ┌──────────────┐
                    │  Atoms.cs    │  scalars · VOs · Quaternion/DualQuaternion ·
                    │              │  CurveProjection · SurfaceProjection · SurfaceSpace
                    └──────┬───────┘
                           │
        ┌──────────────────┼───────────────────┐
        │                  │                   │
        ▼                  ▼                   ▼
  ┌──────────┐      ┌──────────┐         ┌──────────┐
  │ Matrix.cs│      │ Space.cs │         │ Cloud.cs │
  │ + Sparse │      │ Support  │         │ + Orient │
  │ + Sparse │      │ Space    │         │ Normals  │
  │ Hermitian│      │ only     │         │          │
  │ + LOBPCG │      │          │         │          │
  └─────┬────┘      └────┬─────┘         └────┬─────┘
        │                │                    │
        └────────────────┼────────────────────┘
                         │
                         ▼
                  ┌──────────────┐
                  │   Mesh.cs    │  MeshSpace · MeshLaplacian · MeshKernel:
                  │   (NEW)      │  Laplacian assembly · IDT (signpost) · tufted cover ·
                  │              │  Heat method · Vector Heat · CrossField (LOBPCG) ·
                  │              │  BFF · Mean curvature flow · DEC operators ·
                  │              │  Euler/Genus · Feature edges
                  └──────┬───────┘
                         │
                         ▼
                  ┌──────────────┐
                  │  Field.cs    │  +SDF Primitive · +smooth Blend · +Noise ·
                  │              │  +TensorField · +Geodesic/ParallelTransport/
                  │              │  CrossField/MeanCurvatureFlow cases
                  │              │  (delegate to MeshKernel)
                  └──────┬───────┘
                         │
                         ▼
                ┌──────────────────┐
                │  Population.cs   │  HullKind · SamplingKind · RegistrationKind ·
                │  (NEW)           │  RemeshKind · PopulationKernel
                │                  │  (consumes Field.cs + Mesh.cs)
                └─────────┬────────┘
                          │
                          ▼
                   ┌──────────────┐
                   │  Intent.cs   │  +Tensor +MeshOperator +Surface +Flatten
                   │              │  +Hull +Sample +Register +Remesh +Transport
                   │              │  +Pose +Topology +Features +Descriptor
                   └──────────────┘
```

`Mesh.cs` is the differential-geometry substrate. `Field.cs` cases hold thin `MeshSpace` references and delegate evaluation to `MeshKernel`. `Population.cs` sits *above* `Field.cs` because quad remesh consumes a `VectorField` (CrossField) and SDF-region sampling consumes a `ScalarField`. `Intent.cs` is the sole user-facing dispatcher; sub-surface compute methods (`MeshLaplacian.Of`, `SurfaceParameterization.Compute`, `Registration.Align`, etc.) are `internal`.

---

## [3] Unified Rails (binding contract for all new code)

| [#] | [RAIL] | [IDIOM] |
| :-: | --- | --- |
| R1 | Error | `Fin<T>`; failures: `Op.{InvalidInput, InvalidResult, Unsupported, MissingContext, Caution}` (the new `Caution` variant signals recoverable concerns like Perlin gradient artifacts) |
| R2 | Diagnostic | `Op op = key.OrDefault()` at every public entry; threaded into every internal call |
| R3 | Validation | `op.AcceptValue<T>(value)` for primitives; `op.AcceptValidated<TVO>(candidate)` for value objects |
| R4 | Dispatch | `[Union]` with `Switch(state, ...handlers)`; `[SmartEnum<int>]` with `UseDelegateFromConstructor` |
| R5 | Resource | `Lease<T>.Owned/Borrowed` reserved for genuine native resources (`PolylineCurve`, `AreaMassProperties`); managed objects like sparse factorizations live in `ConditionalWeakTable` caches and are not Lease-wrapped |
| R6 | Cache | `ConditionalWeakTable<TKey, TValue>` on the space wrapper; LRU eviction (capacity-bounded) for caches that grow with sample count (`SurfaceSpace` curvature) |
| R7 | Projection | Every output-producing case has `internal Fin<TOut> Project<TOut>(Op)` that pattern-matches `typeof(TOut)` and routes through `op.AcceptValue` |
| R8 | Context | `Context` carried by every factory |
| R9 | Consumer surface | New capability reachable only through a `VectorIntent` case; sub-surface compute methods are `internal` |
| R10 | Boundary | `[BoundaryAdapter]` and `[ValueObject]` partial validators at construction; downstream is not allowed to re-validate |
| R11 | Numerical determinism | All floating-point reductions in `*Kernel` classes use Kahan compensated fold; sparse-matrix-vector products use canonical row order |

Canonical method shapes:
```
public static Fin<T> Of(...payload, Context context, Op? key = null);   // construction
public Fin<TOut> Project<TOut>(Context context, Op? key = null);        // VectorIntent only — public consumer entry
internal Fin<TOut> Project<TOut>(Op key);                               // sub-surface dispatch
internal static Fin<TResult> X(...payload, Op key);                     // MeshKernel / PopulationKernel substrate
```

---

## [4] Phased Roadmap

### Tier legend

`[F]` foundation (unblocks others) · `[L]` low cost, high leverage · `[H]` heavy algorithm · `[T]` thin RhinoCommon wrapper

| [PHASE] | [TIER] | [SCOPE] | [BLOCKS] | [DEPENDS ON] |
| :-: | :-: | --- | :-: | :-: |
| 0 | [F] | `SparseMatrix` (real CSR) + `SparseHermitian` (complex CSR) + sparse Cholesky + sparse LDLᵀ + LOBPCG smallest-eigenpair in `Matrix.cs`; `Termination` upgrades (`CrossSurface`, `EnterRegion`, `LoopDetected`) + `ShouldStop` signature break in `Field.cs` | 3a, 4, 7 | — |
| 1 | [L] | SDF `Primitive` catalog (with per-case Lipschitz constants) + smooth `BlendKind` payload on `CsgKind` (per-blend Lipschitz erosion delegate) + `Noise` cases (Perlin emits `Op.Caution`; OpenSimplex2F / 2S / Worley silent) + `CurlNoise` in `Field.cs`; corresponding `VectorIntent.Field`/`Scalar` paths exercise the new cases | — | — |
| 2 | [L] | `CurveProjection.RotationMinimizing` case in `Atoms.cs` (reuses `CloudKernel.DoubleReflect`) | — | — |
| 3a | [H] | `MeshSpace` in `Mesh.cs` (Cotangent assembly + obtuse-aspect-rejection + `MeshIntersectionCache`) | 3b, 4, 7, 8 | 0 (SparseMatrix) |
| 3b | [H] | IntrinsicDelaunay signpost-flip preprocessor + tufted-cover nonmanifold lifting in `MeshKernel` (Phase 4+ are unblocked by 3a; 3b upgrades the default Laplacian without re-blocking) | — | 3a |
| 4 | [H] | `ScalarField.GeodesicCase` (heat method, two Cholesky solves) + `VectorField.ParallelTransportCase` (vector heat, three solves on `SparseHermitian` connection Laplacian) + `ScalarField.MeanCurvatureFlowCase` in `Field.cs`; `VectorIntent.MeshOperator` in `Intent.cs` | — | 3a (Laplacian), 0 (SparseHermitian) |
| 5 | [T] | `SurfaceProjection` SmartEnum + `SurfaceSpace` wrapper in `Atoms.cs`; `MotionInterpolation` SmartEnum in `Atoms.cs` (Linear/Slerp/Squad/Screw — Quaternion/DualQuaternion deferred until Phase 10 first consumer); `VectorIntent.Surface` and `VectorIntent.Pose` cases in `Intent.cs` | 6, 8 | — |
| 6 | [H] | `TensorField` sibling union in `Field.cs` (`Constant`/`Curvature`/`Lift`/`Warp`/`Scaled`/`Blend` cases); hyperstreamline integrator (signed-eigenvector-aligned generalization of `FieldIntegrator` — `Step` gains `Vector3d? previousSample` parameter); `VectorIntent.Tensor` in `Intent.cs` | 7 | 5 (curvature lift) |
| 7 | [H] | `VectorField.CrossFieldCase` (Knöppel-Crane-Pinkall-Schröder 2013 N-RoSy) in `Field.cs`; `VectorIntent.MeshOperator` cross-field mode | 11 | 3a (Laplacian), 0 (SparseHermitian + LOBPCG), 6 (curvature guidance) |
| 8 | [H] | `SurfaceParameterization` SmartEnum (LSCM / BFF / BFFWithCones — Cherrier system on cached Laplacian; consistent mass matrix for BFF spectral accuracy) in `Mesh.cs`; `VectorIntent.Flatten` in `Intent.cs` | — | 3a (Laplacian) |
| 9 | [H] | `Quaternion` + `DualQuaternion` in `Atoms.cs` (shipped now because first real consumer arrives in Phase 10) | 10 | — |
| 10 | [H] | `RegistrationKind` SmartEnum (PointToPoint / PointToPlane / Symmetric / Robust) in `Population.cs`; `VectorCloud.OrientNormals` factory in `Cloud.cs` (MST-based, prerequisite for point-to-plane); `VectorIntent.Register` in `Intent.cs` | — | 9 (DualQuaternion as return type) |
| 11 | [H] | `RemeshKind` SmartEnum (Isotropic / Quad / Simplify / AdaptiveCurvature) in `Population.cs`; quad pre-flight validates `Σ singularity index = 4χ` via `MeshKernel.eulerCharacteristic`; `VectorIntent.Remesh` in `Intent.cs` | — | 7 (CrossField for Quad mode) |
| 12 | [H] | `HullKind` SmartEnum (Convex / Alpha / Chi — `KNearestConcave` dropped) in `Population.cs`; `SamplingKind` SmartEnum (PoissonDisk / FarthestPoint / FarthestPointOptimization (default for blue-noise) / Lloyd / CCVT) in `Population.cs`; `VectorIntent.Hull` and `VectorIntent.Sample` in `Intent.cs` | — | 3a (mesh ops for 3D hull / sampling on triangulated domain) |
| 13 | [H] | `VectorIntent.TransportCase` (Sinkhorn; `Unbiased` flag for Sinkhorn-divergence variant, off by default) in `Intent.cs` | — | — |
| 14 | [L] | `MeshDescriptor` SmartEnum (HeatKernelSignature / WaveKernelSignature / ShapeDNA) in `Mesh.cs` — thin wrappers over Laplacian eigenpairs; `VectorIntent.Descriptor` in `Intent.cs`; `VectorIntent.Topology` and `VectorIntent.Features` cases in `Intent.cs` (expose `MeshKernel.eulerCharacteristic` and dihedral-feature detection) | — | 3a (Laplacian + Lanczos), 0 (LOBPCG) |

Phases 0–2 ship without mesh work: pure value-add, no risk. **Phase 3a is the gateway**; Phase 3b is parallel and upgrades the default Laplacian without re-blocking Phase 4. Phase 9 (Quaternion/DualQuaternion) is deferred from Phase 0 to its first real consumer (Phase 10) per the no-speculative-scaffolding rule.

---

## [5] Per-Item Blueprint

Each entry: surface extended · polymorphic shape · invariants enforced at boundary · numerical pitfall mitigated · verification gate.

### [5.1] `SparseMatrix` + `SparseHermitian` + LOBPCG — Matrix.cs

- **Shape (real)**: `readonly record struct SparseMatrix(Dimension Rows, Dimension Cols, Arr<int> RowPtr, Arr<int> ColInd, Arr<double> Values)` (Compressed Sparse Row)
- **Shape (complex)**: `readonly record struct SparseHermitian(Dimension Order, Arr<int> RowPtr, Arr<int> ColInd, Arr<Complex> Values)` — upper-triangular CSR; `Apply(Arr<Complex> x)` performs Hermitian-conjugate multiplication
- **Surface**: `FromTriplets`, `Multiply`, `MultiplyDense`, `DecomposeCholeskySparse(ordering: AMD)`, `DecomposeLdltSparse(BunchKaufmanPivot)`, `Solve` via cached factor, `SmallestEigenpairs(k, tol, preconditioner)` via **LOBPCG** (Knyazev 2001) on either real-symmetric or complex-Hermitian inputs; LOBPCG converges to smallest eigenpairs of generalized problem `A x = λ M x` without requiring factorization or shift-invert
- **Invariants**: `RowPtr.Count == Rows.Value + 1`; `ColInd[RowPtr[i]..RowPtr[i+1]]` sorted; `Values` finite
- **Pitfall (sparse Cholesky)**: fill-in — apply approximate minimum-degree (AMD) ordering before factor; ship SPD-only path
- **Pitfall (LDLᵀ)**: required for any indefinite system (shift-invert eigensolves on near-zero shifts); Bunch-Kaufman pivoting prevents zero-pivot failure
- **Pitfall (Hermitian)**: complex storage avoids the real `2N×2N` block expansion that would otherwise quadruple memory; without this Phase 4 vector heat and Phase 7 cross-field hit a wall
- **Verification**: `tests/csharp/libs/Rasm/Vectors/Matrix.spec.cs` — PBT roundtrip dense ↔ sparse on symmetric matrices; sparse Cholesky residual ≤ machine epsilon on PSD inputs; LOBPCG recovers smallest eigenpair of analytic Laplacian on regular grid

### [5.2] `Termination` upgrades + `ShouldStop` signature — Field.cs

- **Shape**: three new cases on `Termination` union — `CrossSurfaceCase(SupportSpace)`, `EnterRegionCase(ScalarField, double Threshold)`, `LoopDetectedCase(PositiveMagnitude ClosureRadius)`
- **Signature break**: `ShouldStop(StreamlineState state, Vector3d currentSample)` — replaces the unpacked-field signature; `StreamlineState` (currently in `Intent.cs:50`) moves to `Field.cs` adjacent to `Termination`
- **Invariants**: `CrossSurfaceCase` rejects unless `SupportSpace.CanClosest`; `EnterRegionCase` requires the scalar field to be sampleable
- **Pitfall**: CrossSurface ambiguity on tangential grazes — define "cross" as sign change of signed-distance from one step to next; grazing tangency stays uncrossed
- **Verification**: `Field.spec.cs` — streamline on Coulomb field with absorbing Sphere terminates at first intersection; closed-loop orbit terminates within configured closure radius

### [5.3] `Quaternion` (Phase 9) — Atoms.cs

- **Shape**: `[StructLayout(LayoutKind.Auto)] [BoundaryAdapter] readonly record struct Quaternion(double W, double X, double Y, double Z)`
- **Surface**: `Of(double angle, Direction axis, Op? key)`, `OfRotation(Plane from, Plane to, Op? key)`, `*` composition, `Slerp(a, b, t)`, `Squad(a, b, c, d, t)`, `Log`, `Exp`, `Conjugate`, `Inverse`, `ToTransform()`
- **Invariants**: post-construction `|‖q‖² − 1| ≤ √ε`; renormalization triggered when composition drifts past threshold
- **Pitfall**: shortest-arc SLERP requires sign flip when `q · q' < 0`; near-parallel (`q · q' > 1 − ε`) falls back to LERP + normalize
- **Verification**: `Atoms.spec.cs` — roundtrip Quaternion ↔ Transform on random rotations; SLERP at t=0/1 equals endpoints; composition associativity within 8√ε

### [5.4] `DualQuaternion` (Phase 9) — Atoms.cs

- **Shape**: `readonly record struct DualQuaternion(Quaternion Real, Quaternion Dual)`
- **Surface**: `Of(Transform)`, `OfScrew(Direction axis, double angle, double translation, Op? key)`, `*` composition, `ScLerp(a, b, t)` (screw-LERP per Kavan-Collins-Žára-O'Sullivan 2006), `Conjugate`, `Inverse`, `ToTransform()`
- **Invariants**: `Real.IsUnit && |Real · Dual| ≤ √ε`
- **Pitfall**: dual drift under multiplication — normalize `Real` first, then orthogonalize `Dual -= (Real · Dual) Real`
- **Verification**: `Atoms.spec.cs` — ScLerp roundtrip equals Transform interpolation along screw axis; composition preserves SE(3) group identity

### [5.5] `MotionInterpolation` (Phase 5) — Atoms.cs

- **Shape**: `[SmartEnum<int>] sealed partial class MotionInterpolation` — `Linear`, `Slerp`, `Squad`, `Screw`; delegates via `UseDelegateFromConstructor`
- **Surface**: `Interpolate(VectorFrame a, VectorFrame b, double t, Op key) → Fin<VectorFrame>` — `Linear`/`Slerp` paths use `Transform.PlaneToPlane` interpolation without requiring Quaternion (Phase 5 ships before Phase 9); `Screw` path activates once `DualQuaternion` lands in Phase 9
- **Pitfall**: degenerate (a == b) falls back to `Linear`
- **Verification**: `Atoms.spec.cs` — Slerp on coplanar rotations equals Linear within tolerance

### [5.6] Analytic SDF `Primitive` catalog — Field.cs

- **Shape**: new `sealed record PrimitiveCase(SdfKind Kind, ImmutableDictionary<string,double> Params, Plane Pose) : ScalarField` — Lipschitz looked up via `SdfKind.Lipschitz` (static table)
- **`SdfKind`**: `[SmartEnum<int>]` — `Box`, `Sphere`, `Capsule`, `Cylinder`, `Cone`, `ConeBound`, `CappedCone`, `Torus`, `HexPrism`, `Octahedron`, `OctahedronExact`, `Ellipsoid`

- **Per-case Lipschitz constants** (sphere-tracing consumers read this metadata to derate step):

| `SdfKind` | L | Type |
| --- | :-: | --- |
| `Sphere` | 1.0 | exact |
| `Box` | 1.0 | exact |
| `Capsule` | 1.0 | exact |
| `Cylinder` | 1.0 | exact |
| `Cone` (exact) | 1.0 | exact |
| `ConeBound` | ≈1.7 | bound |
| `CappedCone` | ≈1.2 | bound |
| `Torus` | 1.0 | exact |
| `HexPrism` | 1.0 | exact |
| `OctahedronExact` | 1.0 | exact (longer code) |
| `Octahedron` | √3 ≈ 1.73 | bound |
| `Ellipsoid` | f(axis ratio) | bound — closed-form per the longest semi-axis |

- **Modifiers** (unary ScalarField combinators): `OnionCase(ScalarField, PositiveMagnitude Thickness)`, `RoundCase(ScalarField, PositiveMagnitude Radius)`, `ElongateCase(ScalarField, Vector3d Extent)`, `DisplaceCase(ScalarField, ScalarField Displacement)`, `TwistCase(ScalarField, double AnglePerUnit, Direction Axis)`, `BendCase(ScalarField, double Curvature, Direction Axis)` — modifiers inherit and propagate Lipschitz upward
- **Pitfall**: bound-function distance — `FieldIntegrator` and sphere-tracing consumers read `Lipschitz` and derate step by `1/L`
- **Verification**: `Field.spec.cs` — Sphere SDF on grid matches analytic; modifier composition preserves Lipschitz bookkeeping; `rhino-verify` evidence shows correct iso-surface via marching cubes

### [5.7] Smooth `BlendKind` on `CsgKind` — Field.cs

- **Shape**: replace `CsgKind` SmartEnum payload with `BlendKind` discriminant — `Hard`, `Polynomial(k)`, `Exponential(k)`, `Root(k)`, `Cubic(k)`, `Chamfer(k)`, `Groove(k,d)`, `Round(r)`
- **Surface**: `Combine(double a, double b, BlendKind blend)` implements Quilez `smin` family
- **Erosion**: each `BlendKind` carries an `Erode(L_left, L_right) → double` delegate; e.g. `Polynomial.Erode = (l,r) => max(l,r) * 1.25` (polynomial smin bounded by ¼ extra inside blend band), `Chamfer.Erode = (l,r) => max(l,r) * 1.5`, `Hard.Erode = (l,r) => max(l,r)`
- **Invariants**: `k > 0`, `r > 0`
- **Pitfall**: Lipschitz erosion inside the blend region — `CsgCase` post-condition reads `EffectiveLipschitz = blend.Erode(left.Lipschitz, right.Lipschitz)`
- **Verification**: `Field.spec.cs` — smooth-blend monotonicity (`smin(a, b) ≤ min(a, b)`); blend continuity at `k → 0` matches hard min

### [5.8] `Noise` and `CurlNoise` — Field.cs

- **Shape**: `sealed record NoiseCase(NoiseKind Kind, int Seed, int Octaves, double Persistence, double Lacunarity, double Frequency) : ScalarField`; `sealed record CurlNoiseCase(ScalarField Potential) : VectorField`
- **`NoiseKind`**: `[SmartEnum<int>]` — `Perlin`, `OpenSimplex2F`, `OpenSimplex2S`, `Worley`
- **Invariants**: `Octaves ∈ [1, 32]`; `Frequency > 0`; `Persistence ∈ (0, 1]`; `Lacunarity > 1`
- **Pitfall**: Perlin gradient artifacts in derivative-sensitive consumers — `CurlNoiseCase` constructor emits `Op.Caution` (not hard rejection) when wrapped potential is Perlin; emits `Op.Unsupported` only when potential is `Worley` (true discontinuities at cell boundaries); `OpenSimplex2F` and `OpenSimplex2S` both accepted
- **Verification**: `Field.spec.cs` — noise periodicity at integer offsets; CurlNoise divergence ≈ 0 (numerical Curl of any scalar potential)

### [5.9] `CurveProjection.RotationMinimizing` — Atoms.cs

- **Shape**: one additional `static readonly CurveProjection RotationMinimizing = new(key: 5, sample: ...)` on existing SmartEnum
- **Algorithm**: double-reflection (Wang-Jüttler-Zheng-Liu 2008 ACM TOG) — reuses `CloudKernel.DoubleReflect` already in `Cloud.cs`; do not duplicate
- **Invariant**: curve domain inclusion validated
- **Pitfall**: closed-curve holonomy produces residual twist — `Project<TOut>` does not redistribute; callers needing closed-loop behaviour compose with `CloudKernel.RedistributeClosureTwist`
- **Accuracy note**: O(h²) global like Bishop (both are second-order accurate); the win is ~10× smaller leading constant per Wang 2008 Table 1 and superior symmetry preservation
- **Verification**: `Atoms.spec.cs` — RMF on analytic helix matches Bishop's accuracy bound but with measurably smaller error; torsion-free property holds along straight segments

### [5.10] `MeshSpace` — Mesh.cs

- **Shape**: `readonly record struct MeshSpace(Mesh Native, Context Tolerance)` with static `ConditionalWeakTable<MeshSpace, LaplacianCache>` for memoised assembly
- **Cached state**: `LaplacianCache` carries cotangent stiffness (`SparseMatrix`), consistent mass matrix (`SparseMatrix`) and lumped mass (`Arr<double>`), face RTree, `MeanEdgeLength`, signpost data structure (lazy), tufted-cover lifting (lazy), `MeshIntersectionCache` (for any downstream contouring)
- **Invariants**: input mesh must pass `Mesh.IsValid`; constructor rejects meshes with any triangle whose aspect ratio exceeds `1/sin(5°) ≈ 11.5` with `Op.Caution`; `MeshLaplacian.Cotangent` rejects nonmanifold; `MeshLaplacian.Nonmanifold` is the only case accepting nonmanifold edges
- **Pitfall**: cache invalidation — `MeshSpace` is immutable; cache keyed by instance; user wanting refresh constructs a new `MeshSpace`

### [5.11] `MeshLaplacian` + assembly — Mesh.cs

- **Shape**: `[SmartEnum<int>] sealed partial class MeshLaplacian` — `Cotangent`, `IntrinsicDelaunay`, `Nonmanifold` (MeanValue dropped: Floater's mean-value coordinates produce a non-symmetric Laplacian incompatible with the cached Cholesky solver and offer no advantage over IDT for the use cases here)
- **Surface**: `internal Of(MeshSpace space, Op key) → Fin<SparseLaplacian>` where `readonly record struct SparseLaplacian(SparseMatrix Stiffness, SparseMatrix MassConsistent, Arr<double> MassLumped)`
- **Algorithm decomposition**:
  - `Cotangent` — assembles raw `(cot α + cot β) / 2` weights; fails with `Op.InvalidResult` if obtuse-triangle off-diagonal weights violate the maximum principle
  - `IntrinsicDelaunay` — signpost-driven intrinsic edge-flipping (Sharp-Soliman-Crane 2019 "Navigating Intrinsic Triangulations" SIGGRAPH); produces a strictly Delaunay intrinsic triangulation; assembles cotangent weights on the flipped mesh
  - `Nonmanifold` — Sharp & Crane 2020 "A Laplacian for Nonmanifold Triangle Meshes" SGP (tufted cover preprocessing — glues paired copies of each face); composable with IntrinsicDelaunay
- **Invariants**: `Stiffness` symmetric; diagonal non-negative after IDT
- **Pitfall**: cotangent weight `(cot α + cot β) / 2` blows up as triangle aspect ratio → ∞ — rejected at MeshSpace construction (see [5.10]); negative off-diagonals on obtuse triangles caught at Cotangent assembly
- **Verification**: `Mesh.spec.cs` — Laplacian PSD check on icosphere; IDT idempotency (apply twice = apply once); spectral gap matches analytic torus harmonics

### [5.12] `ScalarField.GeodesicCase` + heat method — Field.cs / Mesh.cs

- **Shape**: `sealed record GeodesicCase(MeshSpace Space, Seq<int> SourceVertices, BoundaryCondition BC) : ScalarField`; `BoundaryCondition = SmartEnum { Neumann, Dirichlet, AverageOfBoth }`; default Neumann (Crane-Weischedel-Wardetzky 2013 §3.3)
- **Algorithm**: Crane-Weischedel-Wardetzky 2013/2017 — single backward-Euler heat diffusion with `t = h²` (where `h = MeshSpace.MeanEdgeLength`; user can override) → normalize gradient → Poisson recovery of distance; both solves use cached sparse Cholesky on the LaplacianCache (two solves total, factor reused across all source-vertex queries)
- **Sampling**: `SampleScalar(point)` finds nearest vertex via cached face RTree, interpolates barycentric across containing triangle
- **Invariants**: `SourceVertices` non-empty; indices in vertex range
- **Pitfall**: time-step too small → heat hasn't propagated, distance estimate biased near sources; too large → estimate underestimates far-field (diffusion distance proportional to √t); on meshes with boundary, must specify `BoundaryCondition`
- **Verification**: `Mesh.spec.cs` — geodesic distance on unit sphere matches great-circle within mesh-resolution error bound

### [5.13] `VectorField.ParallelTransportCase` + vector heat — Field.cs / Mesh.cs

- **Shape**: `sealed record ParallelTransportCase(MeshSpace Space, Seq<(int VertexIndex, Vector3d Tangent)> Seeds) : VectorField`
- **Algorithm**: Sharp-Soliman-Crane 2019 "The Vector Heat Method" ACM TOG — three linear systems: (1) scalar heat diffusion on magnitude (real `SparseMatrix`); (2) connection-Laplacian heat diffusion on tangent-vector field (`SparseHermitian` from [5.1]); (3) scalar Poisson recovery of parallel-transported magnitude
- **Invariants**: seeds non-empty; each `Tangent` non-zero; accepts any `MeshLaplacian` variant (vector heat works on raw cotangent — IDT is an accuracy upgrade, not a requirement)
- **Pitfall**: connection Laplacian must use complex storage (`SparseHermitian`); the real `2N×2N` expansion is permitted as a fallback at 4× memory cost
- **Verification**: `Mesh.spec.cs` — parallel transport around closed loop on flat region equals identity; around closed loop on sphere matches analytic holonomy

### [5.14] `SurfaceProjection` SmartEnum — Atoms.cs

- **Shape**: `[SmartEnum<int>] sealed partial class SurfaceProjection` parallel to existing `CurveProjection`
- **Cases**: `PrincipalDirections`, `PrincipalCurvatures`, `Gaussian`, `Mean`, `OsculatingCircle`, `Geodesic(Point3d target)` (wraps `Surface.ShortPath` from RhinoCommon — analytic geodesic on NURBS surfaces)
- **Surface**: `Project<TOut>(Surface, double u, double v, Context, Op) → Fin<TOut>` mirrors `CurveProjection.Project<TOut>`
- **Pitfall**: degenerate UV (poles, seams) — `Surface.CurvatureAt` returns invalid `SurfaceCurvature`; guard with `surface.Domain(0).IncludesParameter && surface.Domain(1).IncludesParameter` plus `SurfaceCurvature.IsValid`
- **Verification**: `Atoms.spec.cs` — analytical sphere principal curvatures equal `1/r` everywhere; cylinder principal directions align with axis

### [5.15] `SurfaceSpace` — Atoms.cs (co-located with SurfaceProjection)

- **Shape**: `readonly record struct SurfaceSpace(Surface Native, Context Tolerance)` with `ConditionalWeakTable<SurfaceSpace, CurvatureCache>` (LRU-bounded sample cache)
- **Cached state**: `CurvatureCache` — capacity ~1024 sampled `SurfaceCurvature` entries keyed by (u,v) bucketed to tolerance grid
- **Invariant**: `Surface.IsValid`
- **Pitfall**: unbounded cache memory — LRU eviction at capacity; capacity tunable via `Tolerance.Absolute`-derived bucket size

### [5.16] `TensorField` + signed-vector hyperstreamline — Field.cs

- **Shape**: new `[Union] public abstract partial record TensorField` sibling to `VectorField` and `ScalarField`
- **Cases**: `ConstantCase(SymmetricMatrix)`, `CurvatureCase(SurfaceSpace)`, `LiftCase(Func<Point3d, SymmetricMatrix>)`, `WarpCase(TensorField, Transform)`, `ScaledCase(TensorField, double)`, `BlendCase(Seq<TensorField>, FieldBlend)`
- **Surface**: `SampleTensor(Point3d, Context, Op) → Fin<SymmetricMatrix>`; `PrincipalDirections(Point3d, Op) → Fin<Seq<(double Eigenvalue, Direction Eigenvector)>>` via existing `SymmetricMatrix.DecomposeEigen`; `Hyperstreamline(seed, eigenIndex, integrator, termination) → Fin<Polyline>` via a generalized `FieldIntegrator.Step(field, point, h, previousSample, context, op)`
- **Integrator generalization**: `FieldIntegrator.Step` gains a `Vector3d? previousSample` parameter; vector-field cases ignore it; tensor-eigenvector evaluation uses it to disambiguate sign via `dot(eigenvec, previousSample) ≥ 0 ? eigenvec : -eigenvec`
- **Invariants**: case constructors validate matrix dimension matches the field's intrinsic dimension (3 for spatial tensor fields)
- **Pitfall**: degenerate point (|λ₁ − λ₂| < ε) — `Hyperstreamline` detects via successive eigenvalue ratio and terminates with `Op.InvalidResult`
- **Stress/Strain dropped**: per scope; user lifts custom tensors via `LiftCase`

### [5.17] `VectorField.CrossFieldCase` + cross-field design — Field.cs / Mesh.cs

- **Shape**: `sealed record CrossFieldCase(MeshSpace Space, int Symmetry, Option<TensorField> Guidance) : VectorField`
- **Algorithm**: Knöppel-Crane-Pinkall-Schröder 2013 "Globally Optimal Direction Fields" SIGGRAPH — smallest generalized eigenpair `A z = λ M z` of Hermitian connection energy `A` and mass `M`; solved by **LOBPCG on SparseHermitian** (LOBPCG converges to smallest extremal eigenpairs without factorization or shift)
- **Output**: per-face N-RoSy field; singularity-index map cached on space
- **Invariants**: `Symmetry ∈ {1, 2, 4, 6}`; `Guidance.IsSome` requires `TensorField.SampleTensor` validity at every face centroid
- **Pitfall**: `Σ singularity index = χ · N / 4` is topologically forced — `MeshKernel.eulerCharacteristic(MeshSpace)` exposed so callers pre-validate sum constraints; Phase 11 Quad remesh runs this pre-flight automatically

### [5.18] `SurfaceParameterization` SmartEnum — Mesh.cs

- **Shape**: `[SmartEnum<int>] sealed partial class SurfaceParameterization` — `LSCM`, `BFF`, `BFFWithCones`
- **Surface**: `internal Compute(MeshSpace, Seq<int> ConeVertices, Op) → Fin<Arr<Point2d>>`
- **Algorithm**: Sawhney & Crane 2018 ACM TOG "Boundary First Flattening" — given target boundary lengths/angles, solve a linear Cherrier system on the boundary then harmonic-extend interior; uses **consistent (Galerkin) mass matrix** from LaplacianCache for spectral accuracy; LSCM is the cone-free linear fallback (Lévy-Petitjean-Ray-Maillot 2002)
- **Invariants**: cone vertices on mesh boundary or interior singularities with total cone angle satisfying Gauss-Bonnet `Σ θ_cones + ∫ κ_g ds = 2π χ` (closed: `Σ θ = 2π χ`); validation at construction
- **Pitfall**: conformal preserves angles but distorts area — `BFFWithCones` lets users place cones at high-curvature locations to bound area distortion; LSCM alone unsuitable for fabrication

### [5.19] `SamplingKind` SmartEnum — Population.cs

- **Shape**: `[SmartEnum<int>]` — `PoissonDisk(PositiveMagnitude Radius)`, `FarthestPoint(int Count)`, `FarthestPointOptimization(int Count)` (default for blue-noise output per Schlömer-Heck-Deussen 2011), `Lloyd(int Iterations)`, `CCVT(int Count, int Capacity)` (Balzer-Schlömer-Deussen 2009)
- **Surface**: `internal Sample(MeshSpace, Context, Op) → Fin<VectorCloud>` (triangulated domain); `internal Sample(ScalarField sdf, BoundingBox region, Context, Op) → Fin<VectorCloud>` (SDF sub-zero region — rejection sampling on volumetric grid)
- **Invariants**: `Count > 0`; `Iterations ≥ 1`
- **Pitfall**: Lloyd converges to hexagonal regularity — `FarthestPointOptimization` is the documented default for blue-noise output; CCVT remains available for capacity-constrained scenarios; Lloyd is correct only for coverage-uniformity goals

### [5.20] `HullKind` SmartEnum — Population.cs (`KNearestConcave` dropped)

- **Shape**: `[SmartEnum<int>]` — `Convex`, `Alpha(double α)`, `Chi(double λ)`; user-facing through `VectorIntent.Hull(source, kind)` returning `Fin<VectorCloud>`
- **Output**: 2D coplanar inputs return `VectorCloud.Ring`; 3D inputs use Quickhull from `PopulationKernel.convexHull3D` returning a boundary polyline; mesh output deferred (use `VectorIntent.Remesh` for triangulated convex hull)
- **Pitfall**: alpha-shape parameter sensitivity — expose `PopulationKernel.alphaSpectrum(cloud) → Seq<double>` (sorted Delaunay edge lengths) for auto-tuning; `KNearestConcave` removed because k-NN concave hull is parameter-fragile and dominated in practice by alpha-shapes with auto-tuned α
- **Verification**: `Cloud.spec.cs` — convex hull on random 2D point cloud has correct vertex count via Euler formula

### [5.21] `RegistrationKind` SmartEnum — Population.cs (Super4PCS dropped)

- **Shape**: `[SmartEnum<int>]` — `PointToPoint`, `PointToPlane`, `Symmetric` (Rusinkiewicz 2019 SIGGRAPH "A Symmetric Objective Function for ICP"), `Robust(double WelschNu)` (yaoyx689 2020+ Fast-Robust-ICP)
- **Surface**: `internal Align(VectorCloud source, VectorCloud target, Context, Op) → Fin<DualQuaternion>`
- **Algorithm**: SVD-based Procrustes for point-to-point (via existing `Matrix.DecomposeSvd`); IRLS for `Robust` (Welsch weighting)
- **Invariants**: source / target both `VectorCloud.Cluster` (registration requires unordered points); point-to-plane and symmetric variants require `target.Normals.IsSome` — user runs `VectorCloud.OrientNormals(MST)` first (or feeds a cloud with pre-existing oriented normals)
- **Pitfall**: point-to-plane needs consistently-oriented normals; `Hoppe-DeRose-Duchamp-McDonald-Stuetzle 1992` MST propagation is exposed via `VectorCloud.OrientNormals` factory (Phase 10)
- **Super4PCS dropped**: it is a coarse-alignment algorithm (Mellado-Aiger-Mitra 2014 CGF — uses congruent-set enumeration with sphere-indexed pair filtering, not FFT) requiring downstream ICP refinement; bundling coarse + fine into one SmartEnum case lies about the contract. Users perform coarse alignment externally (or via a future `CoarseAlignKind` if demand emerges) then feed into `Registration`
- **Verification**: `Mesh.spec.cs` — register cloud against its rotated/translated self recovers identity within √ε

### [5.22] `RemeshKind` SmartEnum — Population.cs

- **Shape**: `[SmartEnum<int>]` — `Isotropic(PositiveMagnitude TargetEdge)`, `Quad(VectorField CrossField, PositiveMagnitude TargetEdge)`, `Simplify(int TargetFaces, bool UseQem)`, `AdaptiveCurvature(ScalarField CurvatureField, double MinEdge, double MaxEdge)`
- **Surface**: `internal Apply(MeshSpace, Context, Op) → Fin<Mesh>`
- **Algorithm**: Botsch-Kobbelt 2004 SGP "A Remeshing Approach to Multiresolution Modeling" local ops (split/collapse/flip/smooth) for isotropic; Jakob-Tarini-Panozzo-Sorkine-Hornung 2015 SIGGRAPH Asia "Instant Field-Aligned Meshes" for quad (consumes CrossField from #5.17); Garland-Heckbert 1997 SIGGRAPH QEM for simplify
- **Quad pre-flight**: validates `Σ singularity index of CrossField = 4 χ` via `MeshKernel.eulerCharacteristic`; fails with `Op.InvalidInput` if violated (eliminates the "quad remesh produced garbage" failure class before it starts)
- **Pitfall**: feature edges — `Isotropic`/`Quad` detect dihedral-angle features (default 30° threshold, parameterizable via `MeshKernel.detectFeatureEdges`) and pin them
- **Verification**: `Mesh.spec.cs` — isotropic remesh of cube preserves 12 feature edges; quad remesh on torus has zero singularities

### [5.23] `VectorIntent.TransportCase` — Intent.cs

- **Shape**: `sealed record TransportCase(VectorCloud Source, VectorCloud Target, double Regularization, int MaxIterations, bool Unbiased) : VectorIntent`
- **Algorithm**: Sinkhorn iteration (Cuturi 2013 NeurIPS) on cost matrix `C[i,j] = ‖source_i − target_j‖²`; returns coupling `Matrix`; `Unbiased = true` activates Sinkhorn-divergence (Feydy-Séjourné-Vialard-Trouvé-Peyré 2019 AISTATS) — three Sinkhorn solves with self-transport caching; off by default (3× cost not always justified)
- **Outputs**: `Project<Matrix>` returns coupling; `Project<VectorCloud>` returns barycentric-projected morphed source
- **Invariants**: source and target both `VectorCloud.Cluster`; `Regularization > 0`; `MaxIterations ≥ 1`
- **Pitfall**: low ε → ill-conditioned linear systems (need larger ε or Sinkhorn-divergence); document Sinkhorn-divergence cost in the `Unbiased = true` path
- **Verification**: `Intent.spec.cs` — Sinkhorn on two synthetic Gaussians recovers analytic Wasserstein distance within `O(ε log n)`; Sinkhorn-divergence on identical distributions returns ≈ 0

### [5.24] `ScalarField.MeanCurvatureFlowCase` — Field.cs / Mesh.cs

- **Shape**: `sealed record MeanCurvatureFlowCase(MeshSpace Space, PositiveMagnitude TimeStep, int Iterations) : ScalarField` — outputs the smoothed vertex-position field; companion factory `MeshKernel.meanCurvatureFlow(MeshSpace, dt, iters) → MeshSpace` for direct mesh fairing
- **Algorithm**: Desbrun-Meyer-Schröder-Barr 1999 SIGGRAPH "Implicit Fairing of Irregular Meshes" — single implicit Euler step `(M + dt L) X' = M X`, reuses cached Laplacian + lumped mass + sparse Cholesky
- **Invariants**: positive time-step; non-zero iteration count
- **Pitfall**: large time-step over-smooths features; couple with feature-edge pinning ([5.22] reference)
- **Verification**: `Mesh.spec.cs` — flow on a perturbed sphere reduces total curvature monotonically per iteration

### [5.25] `VectorIntent.Topology` + `VectorIntent.Features` — Intent.cs

- **Shape**: `sealed record TopologyCase(MeshSpace Space) : VectorIntent` — `Project<(int Euler, int Genus, int BoundaryComponents)>`; `sealed record FeaturesCase(MeshSpace Space, double DihedralRadians) : VectorIntent` — `Project<Seq<(int A, int B)>>` returning feature edges as vertex-index pairs
- **Algorithm**: trivial — `MeshKernel.eulerCharacteristic(MeshSpace) = V − E + F`; `MeshKernel.detectFeatureEdges(MeshSpace, dihedral)` walks topology computing per-edge dihedral angle
- **Verification**: `Intent.spec.cs` — Euler on icosphere = 2; on torus = 0; on disk = 1

### [5.26] `VectorIntent.Descriptor` (Phase 14) — Intent.cs / Mesh.cs

- **Shape**: `sealed record DescriptorCase(MeshSpace Space, MeshDescriptor Kind, int EigenpairCount) : VectorIntent`
- **`MeshDescriptor`**: `[SmartEnum<int>]` — `HeatKernelSignature(Seq<double> Times)`, `WaveKernelSignature(Seq<double> Energies)`, `ShapeDNA(int K)`
- **Algorithm**: each evaluates against the top-K Laplacian eigenpairs (LOBPCG on cached `SparseLaplacian`); HKS = `Σ_i exp(−t λ_i) φ_i²(x)`, WKS = `Σ_i exp(−(log(e) − log(λ_i))² / 2σ²) φ_i²(x)`, ShapeDNA = sorted eigenvalues
- **Output**: per-vertex feature vector (`Arr<Arr<double>>`) for HKS/WKS; scalar `Seq<double>` for ShapeDNA
- **Invariants**: `EigenpairCount` within `[1, V/2]`; times/energies non-empty
- **Pitfall**: HKS biased to low-frequency / large-scale features; WKS resolves finer detail; users frequently want both — `MeshDescriptor.Stacked` future case multiplexes
- **Verification**: `Mesh.spec.cs` — isometry-invariance check: descriptor of mesh vs. rigid-transformed mesh agrees within √ε

---

## [6] VectorIntent Case Census (target)

After all phases land, `VectorIntent` reaches 35 cases. The discriminant remains legible because cases group naturally by **input shape**, not output type:

| [GROUP] | [CASES] |
| --- | --- |
| Vector / Direction primitives | `Axis`, `Direction`, `Axes`, `Components`, `Lerp`, `Slerp`, `ProjectOnto`, `Mirror`, `Pose` (new) |
| Angular relations | `Angular`, `Relation` |
| Support-space queries | `Support`, `Bounce`, `Between` |
| Continuous fields | `Field`, `Scalar`, `Tensor` (new) |
| Curve / surface projections | `Curve`, `Surface` (new), `Ray`, `Frame`, `Cone` |
| Mesh / surface operators | `MeshOperator` (new), `Flatten` (new), `Topology` (new), `Features` (new), `Descriptor` (new) |
| Cloud → cloud transformations | `Hull` (new), `Sample` (new), `Transport` (new) |
| Cloud → pose | `Register` (new) |
| Cloud → mesh / mesh → mesh | `Remesh` (new) |
| Cloud algebra (existing) | `Cloud`, `Winding` |
| Streamlines | `Streamline` |

Note on collapse: `Hull` / `Sample` / `Transport` all have shape "cloud(s) → cloud" but differ in input arity (Hull = 1, Sample = 1+domain, Transport = 2). Forcing them into one case via `Option<VectorCloud> source, Option<VectorCloud> target, CloudOpKind kind` loses input-shape clarity at the type level; the cases stay distinct. Re-evaluate if Principle 10 fires (≥3 cases share a *typed* payload).

---

## [7] Verification Gates (per phase)

Each phase **must** create the corresponding `tests/csharp/libs/Rasm/Vectors/{Spec}.spec.cs` and `libs/csharp/Rasm/Vectors/{Spec}.verify.csx` scenarios; otherwise CLAUDE.md's 90% per-file coverage gate fails.

| [PHASE] | [GATE] |
| :-: | --- |
| 0 | `bash scripts/check-cs.sh check` · `bash scripts/test.sh Matrix` (Cholesky residual, sparse roundtrip, LDLᵀ on indefinite, LOBPCG on analytic grid Laplacian) · `bash scripts/test.sh Field` (Termination cases including LoopDetected closure) |
| 1 | `bash scripts/test.sh Field` (SDF Lipschitz table validation, smooth-blend monotonicity per blend kind, noise periodicity, CurlNoise divergence ≈ 0) · `bash scripts/rhino.sh verify libs/csharp/Rasm/Vectors/Field.verify.csx` (marching-cubes evidence) |
| 2 | `bash scripts/test.sh Atoms` (RMF accuracy bound on analytic helix; constant smaller than Bishop) |
| 3a | `bash scripts/test.sh Mesh` (Cotangent Laplacian PSD on Delaunay mesh; obtuse-detection fails with Op.InvalidResult) · `bash scripts/rhino.sh verify` (eigenvalue spectrum vs analytic torus harmonics) |
| 3b | `bash scripts/test.sh Mesh` (IDT idempotency; signpost-flip convergence; tufted-cover on Möbius / non-orientable input) |
| 4 | `bash scripts/test.sh Mesh` (geodesic distance error vs analytic on sphere/torus; vector-heat holonomy on sphere; mean-curvature flow monotonic convergence) · `bash scripts/rhino.sh verify` (distance-field visualization) |
| 5 | `bash scripts/test.sh Atoms` (analytic sphere/cylinder principal curvatures; Surface.ShortPath roundtrip; MotionInterpolation Slerp / Screw correctness) |
| 6 | `bash scripts/test.sh Field` (TensorField hyperstreamline sign continuity; degenerate-point termination) |
| 7 | `bash scripts/test.sh Mesh` (CrossField singularity sum = `χ N / 4`; LOBPCG smallest eigenpair convergence) |
| 8 | `bash scripts/test.sh Mesh` (BFF Cherrier-system residual; cone Gauss-Bonnet validation; LSCM angle preservation) |
| 9 | `bash scripts/test.sh Atoms` (Quaternion roundtrip; DualQuaternion ScLerp; composition associativity) |
| 10 | `bash scripts/test.sh Population` (registration recovers identity for rotated/translated self; OrientNormals MST consistency) |
| 11 | `bash scripts/test.sh Population` (isotropic remesh feature preservation; quad zero-singularity on torus; pre-flight rejection on χ-violating CrossField) |
| 12 | `bash scripts/test.sh Population` (CCVT / FPO spectral content; convex hull Euler check; alpha-spectrum determinism) |
| 13 | `bash scripts/test.sh Intent` (Sinkhorn vs analytic Wasserstein on Gaussians; Sinkhorn-divergence ≈ 0 on identical input) |
| 14 | `bash scripts/test.sh Mesh` (HKS / WKS isometry invariance under rigid transform; ShapeDNA stability) |

`check-cs.sh full` runs only when trigger files change (per CLAUDE.md `[5.2]`). Standard sequence between phases: `check → test → rhino-verify`.

---

## [8] Out of Scope / Removed from Audit

| [#] | [DROPPED] | [REASON] |
| :-: | --- | --- |
| 1 | `TensorField.StressCase` / `StrainCase` | Outside design/geometry scope per user direction; only `Curvature` and `Lift` ship |
| 2 | Marching-cubes / iso-surface extraction inside `Mesh.cs` | Rhino 9 WIP exposes no first-class implicit kernel; user-side via Cocoon / Dendro / external Grasshopper plugins |
| 3 | Schwarz-Christoffel / T-splines parameterizations | LSCM + BFF cover practical fabrication cases; T-splines licensing prohibits inclusion |
| 4 | FRep / HyperFun | Superseded by SDF Primitive ([5.6]) + Smooth Blend ([5.7]); FRep concepts are the theoretical foundation of the SDF catalog, not a competing implementation |
| 5 | `HullKind.KNearestConcave` | k-NN concave hull (Moreira-Santos 2007) is parameter-fragile, lacks a convergent definition, and is dominated by `Alpha` with auto-tuned α via `PopulationKernel.alphaSpectrum` ([5.20]) |
| 6 | `RegistrationKind.Super4PCS` | Coarse-alignment algorithm requiring downstream ICP refinement; bundling coarse + fine into one SmartEnum case violates the contract. Users perform coarse alignment externally then call `Registration` for fine alignment |
| 7 | `MeshLaplacian.MeanValue` | Floater's mean-value coordinates produce a non-symmetric Laplacian incompatible with cached Cholesky; IDT covers the geometric quality concerns MeanValue would address |

---

## [9] Cross-references

- `libs/csharp/Rasm/AGENTS.md` — folder ownership and surface rules
- `libs/csharp/Rasm/Domain/` — `Context`, `Op`, `Fault`, `GeometryKernel`, `ClosestHit`, `Lease<T>`
- `tests/csharp/libs/Rasm/Vectors/` — per-phase spec files extending existing `Matrix.spec.cs`, `Atoms.spec.cs`, etc.; new `Mesh.spec.cs` and `Population.spec.cs` files required for Phases 3a onward
- `scripts/check-cs.sh`, `scripts/test.sh`, `scripts/rhino.sh` — quality gates

---

## [10] Review Provenance

This blueprint reflects critical review by two parallel reviewers (architectural and algorithmic/numerical) against the v1 draft. Substantive changes incorporated:

- **Architectural**: split Mesh.cs into Mesh.cs + Population.cs (rejecting the dumping-ground objection); flipped Field.cs ↔ Mesh.cs dependency arrow (Population now sits above Field); collapsed dual surfaces (Hull only via VectorIntent, not as a VectorCloud factory; SurfaceSpace co-located with SurfaceProjection in Atoms.cs not Space.cs); generalized `FieldIntegrator.Step` to carry `previousSample` for tensor sign-disambiguation; broke `ShouldStop` signature to accept `StreamlineState`; deferred Quaternion/DualQuaternion to Phase 9 (first real consumer); split Phase 3 into 3a (Cotangent gateway) and 3b (IDT upgrade).
- **Algorithmic/numerical**: corrected Sharp-Soliman-Crane to 2019; Sawhney-Crane to 2018; Kavan ScLERP to 2006; added missing Knöppel-Crane-Pinkall-Schröder 2013 co-authors; corrected Super4PCS to congruent-set enumeration (not FFT); RMF accuracy claim corrected from O(h⁴) to O(h²) with smaller constant; added per-case Lipschitz table for SDF Primitive; replaced multiplicative blend erosion with per-`BlendKind` `Erode` delegate; downgraded Perlin gate from `Op.Unsupported` to `Op.Caution`; added `SparseHermitian` complex CSR (load-bearing for Phases 4 and 7); added LOBPCG for smallest-eigenpair (replaces vanilla Lanczos which converges to wrong end of spectrum); added BoundaryCondition to GeodesicCase (Neumann default); switched to consistent mass matrix for BFF (lumped for heat method); added MeanCurvatureFlow ([5.24]), Topology / Features ([5.25]), and Descriptor ([5.26]); dropped MeanValue Laplacian (non-symmetric), KNearestConcave hull (parameter-fragile), Super4PCS (mis-scoped); demoted CCVT default to FarthestPointOptimization for blue-noise; made Sinkhorn-divergence opt-in via `Unbiased` flag.
