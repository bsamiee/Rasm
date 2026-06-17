# [RASM]

`Rasm` is the geometry and numeric kernel below the C# app strata: it references no sibling and is referenced by every stratum above. It carries three mature sub-domains with co-located source — `Vectors` (the typed operator/spectral vocabulary), `Analysis` (the measure/query/intersect/topology algebra), and `Domain` (Rhino-normalization, context, stats, validation) — plus the greenfield robust-core `Geometry` sub-domain whose design pages live under `.planning/`. The robust-core composes `Rasm.Vectors` as settled vocabulary and never re-mints a primitive, admits NO external geometry library (every predicate, index, naming, healing, and constraint kernel is authored from first principles), and is pure-managed osx-arm64 with one tier-3 native gate (the boolean/CSG arrangement asset). This README routes the design pages and registers every external package the folder uses.

## [1]-[ROUTER]

The mature `Vectors`/`Analysis`/`Domain` source carries no `.planning/`; its design surface is `Vectors/_ARCHITECTURE.md` and its open work rides this folder's `TASKLOG.md`. The greenfield robust-core's pages live under `Geometry/.planning/<sub-domain>/`, one page per eventual source file, mirroring the `Rasm.Geometry.*` namespaces, because a package-root `.planning/` would wrongly imply the mature siblings are also in planning. `ARCHITECTURE.md` maps the full sub-domain structure; `IDEAS.md` and `TASKLOG.md` carry the forward pool and the distilled work.

- `Geometry/.planning/numerics/predicates.md` — the adaptive-precision exact-predicate floor: `Predicate` (Orient2D/Orient3D/InCircle/InSphere) over `Expansion` sign-exact arithmetic and the `ErrorBound` filter table, the `NumericsPolicy` interior-double scope.
- `Geometry/.planning/spatial/index.md` — one polymorphic `SpatialIndex` (SAH-BVH + Morton linear octree) over one `NodeStore`, the `SpatialQuery` nearest/range/ray/overlap fold, `Refit`, and the `ToAcceleration` Compute seam.
- `Geometry/.planning/topology/naming.md` — persistent topological naming: one `TopoName` lineage algebra, the `NameTable` registry, and the `Track` re-anchor-by-signature fold.
- `Geometry/.planning/topology/reconciliation.md` — the naming↔hash fence: `CanonicalTopology` canonical-adjacency encoder and the `Reconcile` projection onto the Persistence `GeometryHash`.
- `Geometry/.planning/healing/repair.md` — the repair rail: the `HealOp` closed algebra (six author-kernels + one native-gate boolean) and the `Heal.Repair` session fold composing the predicate floor.
- `Geometry/.planning/healing/receipts.md` — the typed `RebuildReceipt` family, the `ManifoldStatus` projection, and the `HealSession`/`RebuildLog` fold feeding the naming re-anchor.
- `Geometry/.planning/constraints/solver.md` — one author-kernel geometric constraint solver: the closed `Constraint` residual/Jacobian algebra, the `DofAnalysis` verdict, and the Levenberg-Marquardt `Solve` iterate.
- `Geometry/.planning/faults/faults.md` — the consolidated band-2400 `GeometryFault` union every geometry rail routes through, and the `GeometryKeyPolicy` ordinal key accessor.

## [2]-[PACKAGES]

Every external library the kernel uses, planned or implemented, as a flat list. Versions are centralized in the one language manifest; no per-folder manifest exists. The robust-core admits NO external geometry library; the packages below back the numeric lane, the generated dispatch, the result rails, and the content-hash federation.

- Thinktecture.Runtime.Extensions — `[Union]`/`[SmartEnum]`/`[ValueObject]` generated dispatch and value objects.
- LanguageExt.Core — `Fin`/`Validation`/`Eff` result rails, `Seq`/`Option`/`HashMap`/`Set` immutable collections, `Error`.
- MathNet.Numerics — dense linear algebra (`Matrix<double>`/`DenseMatrix`/`Cholesky`) for the constraint solver's normal-equations solve.
- CSparse — sparse direct solves for the mature `Vectors` mesh/matrix lane.
- System.Numerics.Tensors — `TensorPrimitives` SIMD primitives for the mature `Vectors` field/spectral lane (BCL inbox).
- System.IO.Hashing — `XxHash128`, the one content-address hash the topology reconciliation reuses.
- System.Buffers.Binary — `BinaryPrimitives` for the canonical-adjacency byte encoding (BCL inbox).
- System.Numerics.BigInteger — the exact rational oracle the predicate law-matrix proves against (BCL inbox).
- System.Collections.Frozen — `FrozenDictionary` for the spatial-index builder table (BCL inbox).
- System.Math — `FusedMultiplyAdd`/IEEE-754 primitives for the expansion arithmetic (BCL inbox).
- Rhino.Geometry / RhinoCommon — `Point3d`/`Vector3d`/`BoundingBox`/`Ray3d`/`Sphere`/`Mesh` geometry, composed via `Rasm.Vectors`, never re-minted.
- Rasm.Vectors (project) — the settled operator/mesh vocabulary the robust-core composes.
- Rasm.Compute.Solver (project) — the `AccelerationStructure` clash seam the spatial index projects to, composed never re-minted.
