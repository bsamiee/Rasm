# Rasm Agent Instructions

## Purpose

`Rasm` is the foundational geometry kernel and higher-order concern library. It is not a thin Rhino API boundary and not a place for extracted wrappers.

Build reusable category logic for advanced downstream code: analysis, vectors, detection, orientation, transformation, manipulation, topology, measurement, spatial search, and future concern categories. Downstream consumers should get powerful operations with minimal ceremony, no repeated sequencing, and no local reinvention.

## Design Contract

- Build concern categories, not method collections. Each folder owns one coherent category through one public OOP surface and typed FP/ROP internals.
- Add real algorithmic value. Encode geometry reasoning, validation, statistics, projection, ownership, batching, sampling, dispatch, and consistency rules inside the category boundary.
- Keep public usage small and powerful. Consumers should pass intent and context, then receive typed results without knob spam, boilerplate, or native call choreography.
- Preserve capability through dense polymorphism. Prefer operation algebras, discriminated unions, smart enums, folds, and projection carriers over helper functions or parallel APIs.
- Keep logic variable-driven. Avoid hardcoded values. When defaults add value, express them as named policies, constructor defaults, or caller-overridable values tied to domain semantics.

## Folder Ownership

- `Domain/` is the kernel. It owns `Context`, tolerances, unit semantics, geometry kind detection, coercion, lifecycle ownership, validity, requirement checks, faults, statistics, residuals, distributions, and shared projection carriers.
- `Analysis/` owns higher-order geometry analysis through `Analyze`, `Operation<TGeometry,TOut>`, and `IAspect`. It imports and extends `Domain` rather than duplicating validation, stats, coercion, or geometry-kind logic.
- `Vectors/` owns vector intent, direction, support-space projection, fields, rays, spans, signed axes, relation logic, and intent projection through `VectorIntent.Project<TOut>`.
- Future folders should follow the same pattern: one concern category, one consumer surface, compact intent/state records, and internal algorithms that reuse `Domain`.

## Domain Extension Rules

- Treat `Domain` as shared kernel, not a dumping ground. Extend it only for concepts reused across multiple concern categories or required by acceptance, validation, stats, context, ownership, or geometry identity.
- Update `Domain` surgically when a new folder needs shared semantics. Extend existing bodies such as `Validation`, `Stats`, `Context`, `GeometryKernel`, `Kind`, `Requirement`, `OpAcceptance`, or projection carriers instead of creating duplicate local logic.
- Flow outward from `Domain` into category folders. Do not make `Domain` depend on `Analysis`, `Vectors`, or future concern folders.
- Keep stats and validation canonical. New residuals, distributions, validity checks, operation faults, tolerance rules, and acceptance rules belong on existing domain rails when reusable.

## Surface Rules

- Expose one access path per folder. `Analysis` routes through `Analyze`; `Vectors` routes through `VectorIntent.Project<TOut>` for intent projection; future folders should have one equivalent owner.
- Do not give every file its own consumer API. Files inside a folder are parts of one unified boundary.
- Do not create wrapper-only abstractions around RhinoCommon or existing domain code.
- Do not bolt on new needs beside existing rails. Integrate into current owners, update callers, and remove obsolete paths.
- Do not duplicate `Domain` logic locally. Import, extend, and compose it.

## Implementation Rules

- Read `Domain/` before adding category logic. Reuse `Context`, `Requirement`, `Stat`, `Distribution`, `GeometryKernel`, `TopologyProjection`, `ClosestHit`, `IntersectionHit`, and `OpAcceptance` where they fit.
- Model category intent as typed data. Convert many primitive parameters into compact intent records, smart enums, or union cases when that reduces ceremony and clarifies semantics.
- Keep operation internals functional. Use `Fin<T>`, `Validation`, `Eff`, `Option`, `Seq`, `TraverseM`, folds, projections, and typed failures.
- Keep native interop at boundary adapters. Convert nullable, bool, disposable, and native ownership semantics into typed rails immediately.
- Prefer advanced C# and approved libraries when they reduce surface area or strengthen invariants. Use `LanguageExt` and `Thinktecture` to collapse behavior, not to decorate unchanged imperative code.
- Use `docs/system-api-map` for BCL, `System.*`, and package/reference policy; use `docs/external-libs/mathnet` before writing numerical algorithms by hand. MathNet is for proven numeric/symbolic value, not decorative wrapping around unchanged logic.

## Vectors Subsystem (Spectral + Mesh-Aware Field Algebra)

`Vectors/Spectral.cs` is the substrate boundary owning three tightly-coupled concerns: DEC operator assembly (`DiscreteCalculus` carrying d0/d1/star0/star1/star2), the smallest-k eigenpair cache (`SpectralBasis` cached on `LaplacianCache`), and the `SpectralFilter` Union (`Heat | Wave | Biharmonic | Diffusion | CommuteTime | Identity`). Both `Mesh.MeshDescriptor` (single collapsed `SpectralCase`) and `Field.ScalarField.SpectralDistanceCase` route through `SpectralFilter.Apply`; duplicating the dispatch would force three identical implementations in Mesh, Field, and Cloud.

`CSparse.NET 4.3.0` is added centrally in `Directory.Packages.props` and referenced only by `Rasm`. It backs `Matrix.CholeskySparse` (sparse SPD factorisation with AMD ordering and Span-based solve) for the vector-heat / log-map / spectral-shift-invert family. Without it those kernels would cost 50-150x one geodesic via BiCGStab; with it they cost ~3-4x.

`Mesh.MeshLaplacian` SmartEnum admits three cases: `Cotangent`, `IntrinsicDelaunay`, `Robust`. `Robust` follows Sharp-Crane SGP 2020 -- unweld non-manifold edges into per-face tufts via `Mesh.UnweldEdge`, then run intrinsic Delaunay flips on the locally-manifold result. The cotangent weights on the tufted cover are guaranteed non-negative even when the input is non-manifold.

`Field.cs` Union extensions (greenfield, no shims): `VectorField` gains `HodgeIrrotational`, `HodgeSolenoidal`, `VectorHeat`, `GeodesicTangent` cases plus `CrossField` extension with optional `Constraints` (Knoppel-Crane-Pinkall GODF 2013) and `Cones` (Crane-Desbrun-Schroeder trivial connections). `ScalarField` gains `SpectralDistance`, `LogMap`, `Stripe`, `SignedDistanceFromMesh` cases plus a `SdfMeshMethod` SmartEnum (`GeneralizedWindingNumber | SignedHeat`). All cases route through `MeshKernel` dispatch and reuse `LaplacianCache.Cholesky` + Hermitian connection Laplacian where applicable.

`Cloud.cs` `VectorCloudMetric` SmartEnum gains `PrincipalCurvature` (local quadric fit -> shape operator eigendecomp), `Curvedness`, `ShapeIndex`. `CloudKernel.Sinkhorn` accepts `Option<PositiveMagnitude> massRelaxation`; `Some(lambda)` activates Chizat-Peyre-Schmitzer-Vialard 2018 unbalanced transport via the lambda/(lambda+reg) exponent. `Align.AlignKind` SmartEnum admits five cases: `Point`, `Plane`, `Symmetric` (Rusinkiewicz 2019), `Robust` (Welsch IRLS), `Generalized` (Segal-Haehnel-Thrun GICP 2009 with covariance-weighted residuals).

`Atoms.Direction.ParallelTransport(Seq<Plane>)` discretely transports a unit vector along a frame chain via `Transform.PlaneToPlane`. `Modes.SurfaceProjection.ShapeOperator` projects Rhino native `SurfaceCurvature` into a `(k1, k2, e1, e2)` ValueTuple.

Greenfield renames applied across the subsystem (no shims): `IterationCap` -> `MaxIterations`, `EigenpairCount` -> `Pairs`, `TargetEdge` -> `TargetLength`, `Sigma` -> `Spread`, `Unbiased` -> `Debiased`. `MeshDescriptor` collapses from three cases (HKS / WKS / ShapeDNA) to single `SpectralCase(SpectralFilter, Option<Seq<int>>)`; consumers migrate to `MeshDescriptor.Spectral(SpectralFilter.Heat(...), sources)`.
