# [RASM]

`Rasm` owns the RhinoCommon-aware geometry and numeric kernel below the C# app strata: the exact-arithmetic computational-geometry floor and the operational geometry plane above it, one body. Its robust tier — adaptive exact predicates, constrained Delaunay, exact arrangement and booleans, the predicate-exact crossing lattice, robust skeleton and medial, predicate-gated heal, decimate, flatten, fit, and solve — is necessary but not sufficient; the consumer tier composes it into the parametric forms behind every generated assembly, the toolpath-grade offsets, slices, and clearances behind every CNC and additive program, the encoded payloads behind every GPU residency lane, and the persistent topological naming behind every persistence diff. Where no admissible package carries the robustness guarantee the kernel authors from first principles; where the ecosystem owns the concern it composes the categorical-best engine; never both for one concern.

`Rasm` references no sibling while every upper stratum composes it, and it is the one C# geometry owner — the host-free peers meet it only at the content-identity and tessellation wire. Pure-numeric floor is host-neutral-shaped without minting a host-free assembly, and `Context.Of(RhinoDoc)` and `Analyze.From(RhinoDoc)` are its only doc-coupled entries.

## [01]-[ROUTER]

[DOMAIN]:
- [01]-[RAILS](.planning/Domain/rails.md): Kernel ROP substrate — result union, boundary-exception and resource rails, and the `Op`-threading law.
- [02]-[CONTEXT](.planning/Domain/context.md): Tolerance/units substrate — value objects and immutable context bundle with its doc-coupled adapter.
- [03]-[IDENTITY](.planning/Domain/identity.md): Determinism owner — seed-zero `ContentHash.Of` federation content key and deterministic derivation.
- [04]-[VALIDATION](.planning/Domain/validation.md): One acceptance/readiness oracle — the readiness algebra and the canonical admission vocabulary.
- [05]-[NORMALIZATION](.planning/Domain/normalization.md): Rhino-kind taxonomy and coercion owner — coercion lattice and its projection carrier.
- [06]-[EVALUATION](.planning/Domain/evaluation.md): Closest-point evaluation lattice — `ClosestHit` over frames, sampling, signed distance.
- [07]-[STATS](.planning/Domain/stats.md): Statistics substrate — streaming moments, tolerance-banded extrema, and distribution quantiles.

[NUMERICS]:
- [08]-[PREDICATES](.planning/Numerics/predicates.md): Exact-predicate floor — orientation, in-circle, and constructed-point tests up the ladder.
- [09]-[FAULTS](.planning/Numerics/faults.md): Consolidated `GeometryFault` union every geometry rail routes through and lowers onto the error rail.
- [10]-[ATOMS](.planning/Numerics/atoms.md): Typed vector-algebra floor and the raw-to-typed projection dispatch every projection routes through.
- [11]-[MATRIX](.planning/Numerics/matrix.md): Dense/sparse/complex linear-algebra owner and the `MatrixKernel` solve and eigen family.
- [12]-[INTEGRATE](.planning/Numerics/integrate.md): ODE integration floor — data-driven tableau vocabulary and `FieldIntegrator` adaptive stepper.
- [13]-[SPECTRAL](.planning/Numerics/spectral.md): Mesh-free spectral algebra — `DiscreteCalculus` with transfer-function and descriptor algebra.
- [14]-[CALCULUS](.planning/Numerics/calculus.md): Sample-anywhere math floor — central-difference stencils, noise lattices, and falloff profiles.

[SPATIAL]:
- [15]-[INDEX](.planning/Spatial/index.md): Polymorphic `SpatialIndex` behind one apply entry — queries, refit, and compute node-array wire.
- [16]-[NAMING](.planning/Spatial/naming.md): Persistent topological naming — lineage algebra, name registry, and the re-anchor-by-signature fold.
- [17]-[RECONCILIATION](.planning/Spatial/reconciliation.md): Naming-to-hash fence — canonical byte streams onto `NamingHash` Persistence consumes.
- [18]-[SUPPORT](.planning/Spatial/support.md): Proximity boundary adapter — capability-gated `SupportProjection` behind its projection gate.
- [19]-[CLOUD](.planning/Spatial/cloud.md): Point-cloud owner — `VectorCloud` union with lazy indexed admission, metric surface, and hull rail.
- [20]-[NEIGHBORS](.planning/Spatial/neighbors.md): One neighborhood substrate — queries, orientation, curvature, and rotation-minimizing frame.
- [21]-[TRANSPORT](.planning/Spatial/transport.md): Optimal-transport owner — log-domain Sinkhorn with plan projections and cloud correspondences.
- [22]-[FIELDS](.planning/Spatial/fields.md): Implicit-field algebra — scalar/vector/tensor unions, the SDF family, and status-tagged sampling seam.

[PARAMETRIC]:
- [23]-[NURBS](.planning/Parametric/nurbs.md): Vendored in-kernel NURBS engine — one `Nurbs.Of` admission over evaluation, fitting, and identity.
- [24]-[CURVE](.planning/Parametric/curve.md): Host-neutral curve op rail — one `ParametricOp` union folded over the vendored curve form.
- [25]-[SURFACE](.planning/Parametric/surface.md): Host-neutral surface rail — `SurfaceOp` union with UV-provenance tessellation and pullback.
- [26]-[SUBDIVIDE](.planning/Parametric/subdivide.md): Subdivision surfaces as stencil rows over sparse-operator refinement with limit evaluation.
- [27]-[DEVELOP](.planning/Parametric/develop.md): Isometric developable strips — geodesic rails, rigid unroll, and per-strip isometry witness.
- [28]-[PANELIZE](.planning/Parametric/panelize.md): Cross-field-guided panelization — family rows, placement frames, and `PanelField` SoA wire.
- [29]-[PATTERNMAP](.planning/Parametric/patternmap.md): Pattern-to-surface instancing — wallpaper fold, log-map inversion, transported frames.
- [30]-[PROJECTIONS](.planning/Parametric/projections.md): Rhino-native selector/motion owner — easing, cycle, and spring under monotonic timing.
- [31]-[LOCATE](.planning/Parametric/locate.md): Curve/surface location algebra — curvature, orientation, containment, and perpendicular frames.

[MESHING]:
- [32]-[DELAUNAY](.planning/Meshing/delaunay.md): Constrained Delaunay owner — predicate-guarded insertion with constraint recovery.
- [33]-[ARRANGEMENT](.planning/Meshing/arrangement.md): Exact-arithmetic mesh and polygon arrangement — boolean cell welds and the typed receipt.
- [34]-[INTERSECT](.planning/Meshing/intersect.md): Predicate-exact intersection lattice — one `IntersectOp` over an exact-sign crossing carrier.
- [35]-[SLICE](.planning/Meshing/slice.md): Slice-stack owner — the section fold over a generated plane family into the `SliceStack` SoA wire.
- [36]-[OFFSET](.planning/Meshing/offset.md): Predicate-exact offsetting — `OffsetOp` union over the wavefront with loop assembly via arrangement.
- [37]-[SKELETON](.planning/Meshing/skeleton.md): MCF skeleton owner — contraction and collapse into `CurveSkeleton` wire and clearance family.
- [38]-[MESH](.planning/Meshing/mesh.md): Mesh substrate owner — the `MeshSpace` snapshot, Laplacian memoization, and intrinsic triangulation.
- [39]-[EDIT](.planning/Meshing/edit.md): Mutable-arena tier — the single-writer `MeshEdit` SoA build arena and the publish-by-freeze seam.
- [40]-[DEC](.planning/Meshing/dec.md): Mesh-bound DEC assembly owner — connection heat, holonomy, and the Hodge decomposition family.
- [41]-[RECONSTRUCT](.planning/Meshing/reconstruct.md): Implicit-reconstruction owner — signed-heat spine, mesh-SDF methods, and iso-extraction.

[PROCESSING]:
- [42]-[REPAIR](.planning/Processing/repair.md): Repair rail — closed `HealOp` algebra over the mesh arena with the typed rebuild-receipt chain.
- [43]-[RECEIPTS](.planning/Processing/receipts.md): Typed `RebuildReceipt` family and the heal-session fold feeding the naming re-anchor.
- [44]-[DECIMATE](.planning/Processing/decimate.md): Predicate-guarded decimation and LOD — one `SimplifyOp` union over the quadric collapse queue.
- [45]-[REMESH](.planning/Processing/remesh.md): Remesh substrate — isotropic and cross-field rewriting minting quad provenance panelize consumes.
- [46]-[FLATTEN](.planning/Processing/flatten.md): Robust UV-flattening — one `ParamOp` over the DEC substrate returning the typed chart atlas.
- [47]-[INTENT](.planning/Processing/intent.md): Kernel consumer rail — `VectorIntent` union with admission dispatching the owning pages.
- [48]-[SAMPLE](.planning/Processing/sample.md): Point-sampling owner — `SampleKind` union with grouped preset policies over the sampling domains.
- [49]-[EXTRACT](.planning/Processing/extract.md): Extraction/projection rail — ingress, native-first sectioning, and typed projection rows.
- [50]-[FLOW](.planning/Processing/flow.md): Streamline/trace owner — dense-output event localization over any vector field.
- [51]-[REGISTER](.planning/Processing/register.md): Registration owner — the `AlignKind` ICP dispatcher behind one alignment policy record.
- [52]-[GEODESICS](.planning/Processing/geodesics.md): On-mesh distance suite — heat-method and geodesics, log/exp maps, and parallel transport.
- [53]-[SEGMENT](.planning/Processing/segment.md): Spectral shape-analysis owner — descriptors, segmentation, cross-fields, and host-capture tier.

[SOLVING]:
- [54]-[SOLVER](.planning/Solving/solver.md): One nonlinear least-squares owner — `Lm.Minimize` and island-decomposed geometric constraint solver.
- [55]-[FIT](.planning/Solving/fit.md): Robust primitive-fit — the MLESAC sampler and orthogonal-distance refine returning the typed `FitReceipt`.

[DRAWING]:
- [56]-[VIEW](.planning/Drawing/view.md): Exact hidden-line and silhouette projection — invisibility kernel returning `DrawingProjection` carrier.
- [57]-[PACK](.planning/Drawing/pack.md): Canonical encoding owner — `PackOp` into the dtype-strided byte arena with a lossless round-trip witness.

[ANALYSIS]:
- [58]-[QUERY](.planning/Analysis/query.md): Measured-query runtime and the public analysis entry — one `AnalysisQuery` request algebra and facade.
- [59]-[MEASURE](.planning/Analysis/measure.md): Metrology owner — mass properties, enclosing bounds, and conformance residual sampling.
- [60]-[INSPECT](.planning/Analysis/inspect.md): Diagnostics owner — genus and Euler topology folds with the full mesh defect and quality capture.
- [61]-[SELECT](.planning/Analysis/select.md): Selection/extraction owner — the edge taxonomy, silhouette and draft capture, and PCA spread.
- [62]-[RELATIONS](.planning/Analysis/relations.md): Pairwise-relation owner — RhinoCommon intersection lattice beside the meshing altitude.

## [02]-[DOMAIN_PACKAGES]

Kernel-specific libraries admitted by this folder; versions centralize in the C# manifest and corroborate against this folder's `.api/`.

[NUMERIC_FLOOR]:
- `CSparse` — sparse direct-solve substrate under the matrix owner.
- `TYoshimura.DoubleDouble` — middle-precision tier of the predicate ladder.
- `ExtendedNumerics.BigRational` — exact-rational oracle the predicate ladders prove against.
- `PeterO.Numbers` — directed-rounding interval-filter tier of the predicate ladder.

[COMPUTATIONAL_GEOMETRY]:
- `Supercluster.KDTree.Net` — flat 3D kd-tree exact k-NN and radius-search leaf.
- `MIConvexHull` — 2D/3D incremental hulls and Delaunay complexes realizing the cloud hull rail.
- `manifoldc` — in-house P/Invoke over `elalish/manifold`, the guaranteed-manifold scale companion.

[HOST_SURFACE]:
- `RhinoCommon` — host compile surface; the kernel reads geometry values, never `RhinoDoc`/`RhinoApp`/UI.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry; the registry and its charters own the full contracts, and `libs/csharp/.api/` holds the shared API evidence.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[NUMERIC]:
- `MathNet.Numerics` — dense linear-algebra and distribution substrate under the matrix owner.
- `System.Numerics.Tensors`
- `CommunityToolkit.HighPerformance` — 2D spans and pooled owners on the SoA build arenas.

[BOUNDED_LANES]:
- `System.IO.Hashing` — reached only through the seed-zero content-hash mint.
- `QuikGraph` — bounded graph-algorithm lane; every graph result leaves as a kernel-owned SoA wire.
- `Wacton.Unicolour` — perceptual color model behind the one atoms color owner.
