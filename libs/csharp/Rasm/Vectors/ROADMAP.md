# Rasm.Vectors Roadmap

This folder is a single-rail vector geometry/numerics layer for RhinoCommon-backed geometry, MathNet-backed linear algebra, LanguageExt rails, and Thinktecture-generated shape dispatch.

Testing work is intentionally excluded from this pass. Static gates remain the completion proof.

## Public Contract

- `VectorIntent.Project<TOut>(Context, Op?)` is the only consumer projection rail.
- Primitive factories may construct atoms, spaces, fields, clouds, matrices, and population operators, but they do not bypass `VectorIntent` for intent projection.
- Unsupported public cases are removed instead of retained as inert failures.
- RhinoCommon owns geometry queries, interpolation, point clouds, mesh reduction/remeshing, closest mesh projection, transforms, convex hull mesh output, and LSCM unwrapping.
- MathNet owns dense/sparse matrix construction, dense decompositions, iterative sparse solves, and numeric diagnostics.
- Local kernels remain only where RhinoCommon/MathNet do not expose the algorithm directly.

## Supported Surface

| Concern | Supported API | Owner |
| --- | --- | --- |
| Atoms | axes, directions, spans, cones, angles, bounce/refraction, curve/surface sampling, pose interpolation | `Atoms.cs` |
| Clouds | ring/polyline/cluster construction, mass properties, PCA metrics, Bishop frames, `VectorCloudMetric.OrientedNormals` | `Cloud.cs` |
| Fields | scalar/vector/tensor unions, SDF primitives, noise, geodesic/MCF/cross-field sampling with validation | `Field.cs` |
| Intents | singular projection rail plus `Tensor`, `MeshOperator`, `Surface`, `Flatten`, `Hull`, `Sample`, `Register`, `Remesh`, `Transport`, `Topology`, `Features`, `Descriptor` cases | `Intent.cs` |
| Matrices | dense matrix/SVD/opaque LU/QR/eigen/Cholesky, sparse CSR/Hermitian CSR, MathNet sparse iterative solve, local LOBPCG | `Matrix.cs` |
| Meshes | snapshot mesh state, cached laplacian/field state, Rhino LSCM flattening, heat geodesic, mean-curvature flow, cross-field, topology/features/descriptors | `Mesh.cs` |
| Population | registration, convex hull, sampling, remesh, Rhino/RTree normal orientation kernels | `Population.cs` |
| Space | shared context/tolerance vocabulary | `Space.cs` |

## Removed Public Claims

- `VectorIntent.Populate(...)` was removed. Use `VectorIntent.Sample(...)`.
- `HitProjection` and `VectorField.Normal(...)`/`VectorField.Tangent(...)` were removed. `VectorField.Hit(...)` uses `SupportProjection`.
- `HullKind` and `SurfaceParameterization` were removed. `VectorIntent.Hull(...)` is Rhino convex hull; `VectorIntent.Flatten(...)` is Rhino LSCM.
- `HullKind.Alpha(...)` and `HullKind.Chi(...)` were removed. RhinoCommon owns convex hull here; alpha/chi hulls are not shipped without a complete API-backed implementation.
- `SurfaceParameterization.BFF` and `BFFWithCones` were removed. `LSCM` is the truthful flattening mode.
- Public `DualQuaternion` output and screw interpolation were removed. Registration projects `Transform` directly.
- Sparse Cholesky/LDL/AMD public claims, sparse-Hermitian dense direct solve, fake nonmanifold tufted cover, and vector-heat projection were removed. Sparse real systems solve through MathNet iterative solvers and preconditioners; dense Cholesky remains a dense factorization result.
- Inert boundary-condition, cone-vertex, quad cross-field, isotropic remesh, and `ShapeDna` count fields were removed from public cases.
- Misleading `OpenSimplex2F` and `OpenSimplex2S` names were replaced with implementation-truthful simplex noise names.
- Testing rows and spec-file commitments were removed from this roadmap because testing is out of scope for this pass.

## Algorithm Notes

### Support Projection

`SupportProjection.Project<TOut>` routes semantic projections before generic `ClosestHit.Project<TOut>`, so `Direction`, `Span`, `SignedSpanAway`, `Normal`, and `Tangent` cannot be intercepted by generic closest-hit output.

### Motion And Frames

`MotionInterpolation.Linear` and `Slerp` use Rhino `Quaternion` interpolation over `Transform`/`Plane` state.

`CurveProjection.BishopFrame` delegates to Rhino `Curve.PerpendicularFrameAt`; no duplicate RMF path is retained.

### Clouds And Population

`VectorCloudMetric.OrientedNormals` estimates local normals from Rhino point-cloud k-nearest neighborhoods and orients them by MST propagation over the same neighborhood graph through the `VectorIntent.Cloud(...)` rail. Underdetermined or rank-deficient neighborhoods fail instead of returning fabricated normals.

Hull support is convex-only through RhinoCommon. Sampling remains `SamplingKind`-driven for Poisson disk, farthest point, farthest point optimization, Lloyd, and capacity-constrained modes. Farthest point optimization takes its iteration count as input and only accepts objective-improving swaps; capacity-constrained sampling fails when the requested capacity cannot cover the candidate set.

### Matrix Core

Dense matrix operations delegate to MathNet decompositions, determinant, pseudo-inverse, rank, and norms. `LuResult` is source/determinant backed and no longer exposes `L/U` without the pivot rail needed to reconstruct MathNet's pivoted factorization. Sparse real matrices are assembled with MathNet sparse builders and solved through MathNet iterative solvers with diagonal preconditioning plus residual checks. Generalized spectral descriptors use Cholesky congruence and MathNet matrix right-hand solves to back-transform eigenvectors through the mass factor. `SmallestEigenpairs` remains a named local LOBPCG kernel because MathNet does not provide that API.

Sparse Hermitian matrices expose multiplication and LOBPCG eigenpairs only; direct solve is not exposed without a truthful sparse Hermitian solver.

### Mesh Algorithms

`MeshSpace` snapshots and triangulates the input mesh before caching. Mesh caches are owned by the `ConditionalWeakTable`-backed `LaplacianCache`, including geodesic, mean-curvature-flow, and cross-field fields with parameter-sensitive keys.

Vector heat is not exposed until a truthful sparse complex transport solve is available; sparse Hermitian systems stay sparse and do not use a dense fallback.

Scalar and complex mesh sampling use `Mesh.ClosestMeshPoint`, `MeshPoint.T`, and face topology for barycentric/quad interpolation. Mesh curvature, reduction, and quad remeshing prefer RhinoCommon APIs.

Topology projection returns `(int Euler, int Genus, int BoundaryComponents)`. Features return topology edge pairs. Descriptors use generalized spectral semantics where stiffness and mass are both required.

### Transport

`Transport` computes a Sinkhorn coupling through MathNet matrix/vector storage. The same case projects scalar cost, coupling `Matrix`, or transported `VectorCloud`; the scalar remains one output shape on the same rail, not a sibling service. Marginal residuals, iteration count, and numeric validity gate convergence before projection.

### Field Validation

SDF parameters are validated per shape before evaluation. Periodic scalar fields reject zero-period axes. Cross-field construction accepts only symmetry because guided assembly is not exposed. Unsupported noise/projection combinations fail at construction rather than being ignored during sampling.

## Static Verification

Run these gates after implementation:

```bash
dotnet build libs/csharp/Rasm/Rasm.csproj --no-restore
bash scripts/check-cs.sh check
dotnet format Workspace.slnx --verify-no-changes --severity warn --no-restore
git diff --check
pnpm exec ast-grep run --pattern 'var $X' libs/csharp/Rasm/Vectors --lang csharp
```

No Rhino runtime bridge checks are part of this pass.
