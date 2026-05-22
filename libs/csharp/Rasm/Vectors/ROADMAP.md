# Rasm.Vectors Roadmap

This folder is a single-rail vector geometry/numerics layer for RhinoCommon-backed geometry, MathNet-backed linear algebra, LanguageExt rails, and Thinktecture-generated shape dispatch.

Testing work is intentionally excluded from this pass. Static gates remain the completion proof.

## Public Contract

- `VectorIntent.Project<TOut>(Context, Op?)` is the only consumer projection rail.
- Primitive factories may construct atoms, spaces, fields, clouds, matrices, and population operators, but they do not bypass `VectorIntent` for intent projection.
- Unsupported public cases are removed instead of retained as placeholder failures.
- RhinoCommon owns geometry queries, interpolation, point clouds, mesh reduction/remeshing, closest mesh projection, and transforms.
- MathNet owns dense/sparse matrix construction, dense decompositions, iterative sparse solves, and numeric diagnostics.
- Local kernels remain only where RhinoCommon/MathNet do not expose the algorithm directly.

## Supported Surface

| Concern | Supported API | Owner |
| --- | --- | --- |
| Atoms | axes, directions, spans, cones, angles, bounce/refraction, curve/surface sampling, pose interpolation | `Atoms.cs` |
| Clouds | ring/polyline/cluster construction, mass properties, PCA metrics, Bishop frames, normal orientation | `Cloud.cs` |
| Fields | scalar/vector/tensor unions, SDF primitives, noise, geodesic/MCF/vector heat/cross-field sampling with validation | `Field.cs` |
| Intents | singular projection rail plus `Tensor`, `MeshOperator`, `Surface`, `Flatten`, `Hull`, `Sample`, `Register`, `Remesh`, `Transport`, `Topology`, `Features`, `Descriptor` cases | `Intent.cs` |
| Matrices | dense matrix/SVD/LU/QR/eigen/Cholesky, sparse CSR/Hermitian CSR, MathNet sparse iterative solve, local LOBPCG | `Matrix.cs` |
| Meshes | cached laplacian/field state, LSCM flattening, heat geodesic, mean-curvature flow, vector heat, cross-field, topology/features/descriptors | `Mesh.cs` |
| Population | registration, convex hull, sampling, remesh, Rhino/RTree normal orientation kernels | `Population.cs` |
| Space | shared context/tolerance vocabulary | `Space.cs` |

## Removed Public Claims

- `VectorIntent.Populate(...)` was removed. Use `VectorIntent.Sample(...)`.
- `HullKind.Alpha(...)` and `HullKind.Chi(...)` were removed. RhinoCommon owns convex hull here; alpha/chi hulls are not shipped without a complete API-backed implementation.
- `SurfaceParameterization.BFF` and `BFFWithCones` were removed. `LSCM` is the truthful flattening mode.
- Public `DualQuaternion` output was removed. Screw interpolation may use dual-quaternion math internally, but public registration projects `Transform`.
- Sparse Cholesky/LDL/AMD public claims were removed. Sparse systems solve through MathNet iterative solvers and preconditioners; dense Cholesky remains a dense factorization result.
- Testing rows and spec-file commitments were removed from this roadmap because testing is out of scope for this pass.

## Algorithm Notes

### Support Projection

`SupportProjection.Project<TOut>` routes semantic projections before generic `ClosestHit.Project<TOut>`, so `Direction`, `Span`, `SignedSpanAway`, `Normal`, and `Tangent` cannot be intercepted by generic closest-hit output.

### Motion And Frames

`MotionInterpolation.Linear` and `Slerp` use Rhino `Quaternion` interpolation over `Transform`/`Plane` state. `Screw` keeps dual-quaternion math internal for screw interpolation only.

`CurveProjection.RotationMinimizing` evaluates a double-reflection rotation-minimizing frame using the same kernel as cloud Bishop frames, rather than delegating to a generic perpendicular frame.

### Clouds And Population

`VectorCloud.OrientNormals(Context, Op?)` estimates local normals from Rhino point-cloud/RTree k-nearest neighborhoods and orients them by MST propagation.

Hull support is convex-only through RhinoCommon. Sampling remains `SamplingKind`-driven for Poisson disk, farthest point, farthest point optimization, Lloyd, and capacity-constrained modes.

### Matrix Core

Dense matrix operations delegate to MathNet decompositions and norms. Sparse real matrices are assembled with MathNet sparse builders and solved through MathNet iterative solvers with diagonal preconditioning plus residual checks. `SmallestEigenpairs` remains a named local LOBPCG kernel because MathNet does not provide that API.

Sparse Hermitian systems retain dense Hermitian solve where the current algorithm requires complex Cholesky; callers should treat that as a bounded fallback, not a sparse-factorization guarantee.

### Mesh Algorithms

Mesh caches are owned by the `ConditionalWeakTable`-backed `LaplacianCache`, including geodesic, mean-curvature-flow, vector-heat, and cross-field fields with parameter-sensitive keys.

Scalar and complex mesh sampling use `Mesh.ClosestMeshPoint`, `MeshPoint.T`, and face topology for barycentric/quad interpolation. Mesh curvature, reduction, and quad remeshing prefer RhinoCommon APIs.

Topology projection returns `(int Euler, int Genus, int BoundaryComponents)`. Features return topology edge pairs. Descriptors use generalized spectral semantics where stiffness and mass are both required.

### Transport

`Transport` computes a Sinkhorn coupling. The same case projects scalar cost, coupling `Matrix`, or transported `VectorCloud`; the scalar remains one output shape on the same rail, not a sibling service.

### Field Validation

SDF parameters are validated per shape before evaluation. Periodic scalar fields reject zero-period axes. Cross-field construction rejects guidance until guided cross-field assembly is implemented. Unsupported noise/projection combinations fail at construction rather than being ignored during sampling.

## Static Verification

Run these gates after implementation:

```bash
dotnet build libs/csharp/Rasm/Rasm.csproj --no-restore
bash scripts/check-cs.sh check
dotnet format Workspace.slnx --verify-no-changes --severity warn --no-restore
git diff --check
```

No Rhino runtime bridge checks are part of this pass.
