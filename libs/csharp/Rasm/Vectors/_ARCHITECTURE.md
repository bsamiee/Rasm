# Rasm.Vectors Architecture

## TODO

### TASK-01 — Connection Laplacian real-2V×2V variant

**Goal**: Extract `Re(M)` and `Im(M)` blocks from the existing complex Hermitian connection Laplacian (`MeshKernel.BuildConnectionLaplacian`, currently `SparseHermitian` of order V) and assemble the real block matrix `[[M_R, -M_I], [M_I, M_R]]` as a `SparseMatrix` of order 2V.

**Why**: Enables `CholeskySparse` factorisation on the connection system. Replaces the current spectral-expansion `MeshKernel.VectorHeatAt` (24 Hermitian eigenpairs + projection) with a single sparse-Cholesky back-substitute. Expected cost: ≈3-4× one geodesic heat solve (versus the current spectral-truncation cost which degrades on dense meshes and high-curvature vertices).

**Touches**: `Mesh.cs` `BuildConnectionLaplacian` (add real-2V×2V emitter), `MeshKernel.VectorHeatAt` (switch from `EvolveVectorHeat` to direct solve), new `LaplacianCache.ConnectionCholesky` lazy mass-pinned for SPD invariance.

---

### TASK-02 — `SpectralFilter` partial monoidal compose

**Goal**: Add `SpectralFilter.Compose(SpectralFilter other) -> Option<SpectralFilter>` returning `Some` only where filter composition is algebraically closed:
- `Heat(t1) ∘ Heat(t2) = Heat(t1+t2)` (exponent additivity)
- `Diffusion(t1) ∘ Diffusion(t2) = Diffusion(t1+t2)` (same)
- `Identity ∘ X = X` (identity element)
- `X ∘ Identity = X`
- All other pairs return `None`

**Why**: Clarifies the partial-monoid structure without forcing operator overloads that would lie about closure. `Wave × Wave`, `Biharmonic × Heat`, `CommuteTime × *` are NOT closed and must surface as `None` rather than silently producing approximate or incorrect filters.

**Touches**: `Spectral.cs` `SpectralFilter` Union — add `Compose` instance method using `Switch(state: other, ...)`.

---

### TASK-03 — `SignedHeat` method: full Feng-Crane 2024 boundary-conditioned solve

**Goal**: Replace the current `Mesh.cs:SmoothSignedDistance` closed-form post-process with the rigorous Feng-Crane SIGGRAPH 2024 boundary-conditioned heat solve.

**Current state**: `MeshKernel.SignedDistanceFromMeshAt` (SignedHeat branch) computes raw GWN distance via `Mesh.IsPointInside` + `Mesh.ClosestPoint`, then post-processes with `Sign(d) * √|d| * √(mean_edge_length)`. Magnitude is order-correct; gradient quality near boundary is approximate and degrades on sharp features.

**Why**: Production-quality SDF for non-watertight / non-manifold input. The Feng-Crane 2024 paper provides a 10× speedup over generalized winding number contouring with provably smoother gradients.

**Touches**: `Mesh.cs:SignedDistanceFromMeshAt`, requires boundary detection (existing `GetNakedEdges`), boundary-conditioned heat solve on `LaplacianCache.Cholesky`, and FEM gradient lift for sign propagation.

---

### TASK-04 — Constrained N-RoSy + Trivial Connections: rigorous construction

**Status**: VALIDATION FOUND TWO DEVIATIONS from the published algorithms.

**Constraints (Knöppel-Crane-Pinkall-Schröder GODF 2013)**:
- Current: `MeshKernel.AugmentConnectionLaplacian` adds a large real-valued diagonal penalty (`1.0e6`) at constrained vertices. The `Direction Hint` in the case data is **captured but never read by the kernel**.
- Correct: GODF adds a hinted-direction quadratic penalty `||u_v - hint_v||²` which contributes `+α` to the diagonal AND `+α·hint_v` to the RHS — making this a *linear system* (smallest constrained eigenvector via Lagrange multipliers), not the unconstrained eigenproblem with a diagonal shift.
- Effect: constrained vertices are biased toward zero magnitude rather than toward the hint direction.

**Cones (Crane-Desbrun-Schröder 2010 trivial connections)**:
- Current: `AugmentConnectionLaplacian` adds `symmetry * deficit` as a real diagonal entry at each cone vertex.
- Correct: Trivial connections require modifying the **off-diagonal** per-edge transport angles `ρ_ij` by distributing each deficit along a dual spanning tree. Cone vertices contribute holonomy that propagates across edges, not a diagonal shift.
- Effect: produces an approximate field with low magnitude near cones but does not give the exact prescribed holonomy.

**Gauss-Bonnet constraint**: For a closed surface of Euler characteristic χ, trivial connections require `Σ deficits = 2π·χ`. The current code does NOT validate this; arbitrary deficit assignments produce ill-formed fields. Consumers must enforce the constraint upstream.

**Touches**: `Mesh.cs:AugmentConnectionLaplacian` (rewrite both branches), new dual-spanning-tree helper for cone holonomy distribution, optional Gauss-Bonnet validator with `Fault.InvalidInput` if violated.

---

`Rasm.Vectors` is the typed vector geometry and numerics layer over RhinoCommon geometry, MathNet linear algebra, CSparse.NET sparse Cholesky, LanguageExt result rails, and Thinktecture-generated dispatch. Factories create atoms, spaces, fields, clouds, matrices, meshes, and intent cases; `VectorIntent.Project<TOut>(Context, Op?)` remains the singular consumer rail for executing an intent into a requested output shape. `Spectral.cs` is the shared substrate owning DEC operator assembly, the eigenpair cache, and the polymorphic `SpectralFilter` algebra consumed by both mesh descriptors and scalar spectral fields.

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
flowchart LR
    accTitle: Rasm.Vectors projection rail with spectral substrate
    accDescr: Factories build typed vector values and VectorIntent cases. Project validates context, dispatches to owning vector modules, and returns Fin of the requested output. Spectral.cs holds DEC operators, eigenpair cache, and spectral filter algebra shared by Mesh and Field.

    subgraph Build["Construction"]
        Values["Value models<br/>Atoms, Space, Field, Cloud, Matrix, Mesh"]
        Policies["Selectors<br/>Modes, Flow, Sample, Align"]
    end

    subgraph Rail["Intent Rail"]
        Intent["VectorIntent cases"] -->|single consumer rail| Project["Project&lt;TOut&gt;(Context, Op?)"]
        Project -->|validate Context + Op| Dispatch["Dispatch&lt;TOut&gt;"]
    end

    subgraph Owners["Owning Modules"]
        Atoms["Atoms<br/>directions, frames, parallel transport"]
        Modes["Modes<br/>curve / surface / cone / pose / shape operator"]
        Space["Space<br/>support projection, closest, distance"]
        Field["Field<br/>scalar / vector / tensor algebra<br/>Hodge, vector heat, log map, stripe, SDF"]
        Flow["Flow<br/>streamline trace, RK tableaus"]
        Cloud["Cloud<br/>metrics (PCA, principal curvature, curvedness)<br/>transport (unbalanced Sinkhorn), winding, hull"]
        Mesh["Mesh<br/>Laplacian (cotangent / IDT / robust)<br/>spectral basis, connection Laplacian, descriptors"]
        Sample["Sample<br/>Poisson disk, farthest, Lloyd, capacity"]
        Align["Align<br/>ICP (Point, Plane, Symmetric, Robust, GICP)"]
    end

    subgraph Spectral["Spectral Substrate"]
        Dec["DiscreteCalculus<br/>d0, d1, star0, star1, star2"]
        Basis["SpectralBasis<br/>eigenpairs cache"]
        Filter["SpectralFilter Union<br/>Heat | Wave | Biharmonic<br/>Diffusion | CommuteTime | Identity"]
        Dec --> Filter
        Basis --> Filter
    end

    subgraph Providers["Shared Providers"]
        Domain["Domain.Geometry<br/>ClosestHit, context, tolerances"]
        Matrix["Matrix<br/>MathNet + CSparse Cholesky"]
        Native["RhinoCommon<br/>geometry, normals, shape operator"]
    end

    Values -->|shape inputs| Intent
    Policies -->|select behavior| Intent
    Dispatch -->|axis, angle, frame, parallel transport| Atoms
    Dispatch -->|curve, surface, cone, pose, shape operator| Modes
    Dispatch -->|closest, span, normal, distance| Space
    Dispatch -->|scalar / vector / tensor / Hodge / heat / log map / stripe / SDF| Field
    Dispatch -->|streamline| Flow
    Dispatch -->|cloud metrics, curvature, transport, winding, hull| Cloud
    Dispatch -->|Laplacian, topology, features, descriptors, remesh| Mesh
    Dispatch -->|sample| Sample
    Dispatch -->|align (Point / Plane / Symmetric / Robust / GICP)| Align

    Space -->|closest queries| Domain
    Field -->|mesh-backed fields, Hodge, heat, log map| Mesh
    Mesh -->|Laplacian, spectral basis, DEC| Spectral
    Field -->|spectral distance, biharmonic, diffusion, HKS, WKS| Spectral
    Cloud -->|curvature via Modes shape operator| Modes
    Cloud -->|transport (Sinkhorn unbalanced)| Matrix
    Sample -->|emits cluster| Cloud
    Align -->|normal estimation, correspondence| Cloud
    Cloud -->|covariance, coupling| Matrix
    Mesh -->|sparse operators, eigensolves, Cholesky| Matrix
    Align -->|SVD, least squares, rigid decomposition| Matrix
    Cloud -->|hull| Native
    Mesh -->|unwrap, reduce, remesh, isosurface, unweld| Native
    Modes -->|Rhino normals, curvature| Native

    Atoms & Modes & Space & Field & Flow & Cloud & Mesh & Sample & Align -->|typed result| Result["Fin&lt;TOut&gt;"]

    classDef rail fill:#44475a,stroke:#bd93f9,color:#f8f8f2,stroke-width:2px
    classDef owner fill:#282a36,stroke:#8be9fd,color:#f8f8f2
    classDef spectral fill:#282a36,stroke:#50fa7b,color:#f8f8f2,stroke-width:2px
    classDef provider fill:#282a36,stroke:#ffb86c,color:#f8f8f2,stroke-dasharray:5\,5
    classDef result fill:#50fa7b,stroke:#f8f8f2,color:#282a36,stroke-width:2px
    class Intent,Project,Dispatch rail
    class Atoms,Modes,Space,Field,Flow,Cloud,Mesh,Sample,Align owner
    class Dec,Basis,Filter spectral
    class Domain,Matrix,Native provider
    class Result result
```

## Ownership

- `Intent.cs`: `VectorIntent` cases, factories, context validation, dispatch delegation.
- `Atoms.cs`: dimensions, magnitudes, axes, angles, directions, spans, frames, cones, relations, `Direction.ParallelTransport(Seq<Plane>)`.
- `Modes.cs`: curve / surface / cone / pose projection selectors; `SurfaceProjection.ShapeOperator` projects Rhino `SurfaceCurvature` into `(k1, k2, e1, e2)`.
- `Space.cs`: `SupportSpace`, `SurfaceSpace`, `SupportProjection`, signed distance, containment, closest-hit projection.
- `Field.cs`: scalar/vector/tensor field algebra (CSG blending, falloff, kernels, noise, finite difference). Mesh-aware extensions: `ScalarField` adds `Geodesic`, `MeanCurvatureFlow`, `SpectralDistance`, `LogMap`, `Stripe`, `SignedDistanceFromMesh`; `VectorField` adds `CrossField` (with optional `Constraints` + `Cones`), `HodgeIrrotational`, `HodgeSolenoidal`, `VectorHeat`, `GeodesicTangent`. `SdfMeshMethod` SmartEnum selects between `GeneralizedWindingNumber` and `SignedHeat`.
- `Flow.cs`: Runge-Kutta tableaus, fixed/adaptive integration, streamline state, termination predicates.
- `Cloud.cs`: cloud construction (Ring / Polyline / Cluster), `VectorCloudMetric` SmartEnum (PCA, principal curvature, curvedness, shape index, winding, hull). `CloudKernel.Sinkhorn` supports unbalanced transport via `Option<PositiveMagnitude> massRelaxation`.
- `Sample.cs`: mesh-surface sampling -- Poisson disk, farthest, optimize, Lloyd, capacity.
- `Align.cs`: cloud alignment -- `AlignKind` SmartEnum admits `Point`, `Plane`, `Symmetric` (Rusinkiewicz 2019), `Robust` (Welsch IRLS), `Generalized` (Segal-Haehnel-Thrun GICP 2009).
- `Mesh.cs`: mesh snapshots, `LaplacianCache` (cotangent / IDT / robust Laplacian, Cholesky factor, spectral basis, field cache, mean edge length), `MeshLaplacian` SmartEnum (`Cotangent`, `IntrinsicDelaunay`, `Robust`), `MeshDescriptor` Union (single `SpectralCase`), topology, features, remesh kernels, Hodge / vector heat / geodesic tangent / stripe / SDF-from-mesh kernels.
- `Matrix.cs`: dense and sparse matrix models, MathNet conversion, decompositions, iterative solves, sparse Hermitian products, local LOBPCG eigensolves, `CholeskySparse` (CSparse.NET-backed SPD factor with Span-based solve).
- `Spectral.cs`: `DiscreteCalculus` (DEC operators `d0`, `d1`, `star0`, `star1`, `star2`), `SpectralBasis` (eigenpairs cache), `SpectralFilter` Union (`Heat`, `Wave`, `Biharmonic`, `Diffusion`, `CommuteTime`, `Identity`) with unified `EvaluateFiltered` kernel handling both per-vertex signatures and pairwise distances.

## Invariants

- `VectorIntent.Project<TOut>(Context, Op?)` is the only consumer projection rail.
- `Spectral.cs` owns DEC operators, eigenpair cache (`LaplacianCache.SpectralBasisOf`), and `SpectralFilter` dispatch. Multiple downstream consumers (Field, Mesh, Cloud) route spectral queries through this single substrate.
- `MeshDescriptor` is a single `SpectralCase` parameterised by `SpectralFilter` and optional source set. HKS / WKS / ShapeDNA = `Heat` / `Wave` / `Identity` filter cases respectively.
- `MeshLaplacian` admits `Cotangent`, `IntrinsicDelaunay`, `Robust` -- all route through `LaplacianCache`. `Robust` follows Sharp-Crane SGP 2020 (tufted cover via `Mesh.UnweldEdge` + intrinsic Delaunay flips on the locally-manifold cover).
- `LaplacianCache` exposes lazy `Cotangent`, `IntrinsicDelaunay`, `Robust`, `Cholesky` (mass-pinned SPD regularisation), `DefaultSpectralBasis` (32 pairs, truncatable), and `SpectralBasisOf(k)` (parametric).
- `Field.ScalarField` extends a continuous scalar with mesh-aware cases that delegate to `MeshKernel`. `VectorField` extends with mesh-aware Hodge decomposition, vector heat, geodesic tangent, and cross-field with constrained / cone variants.
- `Cloud.CloudKernel.Sinkhorn` accepts `Option<PositiveMagnitude>` for unbalanced transport (Chizat-Peyre-Schmitzer-Vialard 2018 KL-divergence relaxation).
- Greenfield renames (no shims): `IterationCap` -> `MaxIterations`, `EigenpairCount` -> `Pairs`, `TargetEdge` -> `TargetLength`, `Sigma` -> `Spread`, `Unbiased` -> `Debiased`.
- Domain owns shared Rhino geometry normalization and `ClosestHit`.
- Vectors owns vector-specific intent, polymorphic field algebra, cloud metrics, mesh operators, sampling, alignment, and spectral substrate.
- RhinoCommon provides native geometry, closest queries, transforms, convex hulls, mesh reduction, remeshing, mesh unwrap, normals, marching-cubes isosurface, point-in-solid, edge-unweld for tufted covers, and shape operator (`RhinoMath.EvaluateNormalPartials`).
- MathNet owns dense and sparse numerical operations (decompositions, BiCGStab iterative solve).
- CSparse.NET 4.3.0 provides sparse Cholesky factorisation with AMD ordering, Span-based solve, and rank-1 update / downdate for shift-invert preconditioning.
- Local kernels exist only where dependencies do not expose the required algorithm.
