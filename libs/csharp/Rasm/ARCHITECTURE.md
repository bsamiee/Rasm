# [RASM_ARCHITECTURE]

`Rasm` is the suite kernel: the existing typed-vector, domain, and analysis layers plus the robust-geometry domain under `Geometry/`. Every robust-geometry concern is an axis owner with closed cases, every entrypoint is a typed verdict or `Fin`/`Validation`/`Eff` rail, and every cross-package fact crosses through one in-process seam. Mechanics live in the `Geometry/.planning/` pages; this page is the atlas — the implementation source tree (the build order), the owner registry (the one owner-state surface), dependency direction, cross-package seams, the ratified boundaries, and the prohibitions.

## [1]-[SOURCE_TREE]

The existing `Vectors/`, `Domain/`, and `Analysis/` source is settled and unannotated below. The robust-geometry layout under `Geometry/` IS the build order: each leaf is one transcription unit, vocabulary owners before consumers, the numeric floor before the predicates that ride it, predicates before the index/topology/healing/constraint consumers, the consolidated fault family last. Each robust-geometry leaf is annotated with the owning `Geometry/<page>#CLUSTER`; sub-folders group the flat file set by concern axis and mirror the `Rasm.Geometry.*` namespaces.

```text codemap
Rasm/
├── Vectors/                       # settled operator vocabulary — Atoms, Space, Field, Cloud, Mesh, Matrix, Spectral, Intent (Vectors/_ARCHITECTURE.md)
├── Domain/                        # Rhino-normalization, Context, Geometry, Stats, Validation
├── Analysis/                      # analysis algebra — Analyze, Measure, Query, Topology, Spatial, …
└── Geometry/                      # robust-geometry domain — Rasm.Geometry.* namespaces
    ├── Numerics/
    │   ├── Expansion.cs           # Expansion sign-exact arithmetic (TwoProduct/TwoSum/Grow/Sum/Scale/SignOf) — geometry-kernel#INTERIOR_NUMERICS
    │   └── Predicate.cs           # Sign, NumericsPolicy, ErrorBound filter rows; Orient2D/Orient3D/InCircle/InSphere — geometry-kernel#ROBUST_PREDICATES, #INTERIOR_NUMERICS
    ├── Spatial/
    │   └── SpatialIndex.cs        # Bvh/LinearOctree union, NodeStore SoA, SpatialQuery union, Build/Refit/Query/ToAcceleration — spatial-index#SPATIAL_INDEX
    ├── Topology/
    │   ├── TopoName.cs            # EntityKind, TopoSignature, TopoName, TrackOutcome, NameTable, Track re-anchor — topology#TOPO_NAMING
    │   └── NamingHash.cs          # CanonicalTopology, Encode canonical-adjacency bytes, Reconcile (naming↔content-hash) — topology#NAMING_HASH
    ├── Healing/
    │   └── Heal.cs                # HealKind, HealOp union, RepairPolicy, Kernels, Heal.Repair fold, RebuildReceipt — healing#HEALING, #REBUILD_RECEIPTS
    ├── Constraints/
    │   └── ConstraintSolver.cs    # SketchEntityKind, Entity, Constraint union, DofAnalysis, ConstraintSystem, LM Solve — constraints#CONSTRAINT_SOLVER
    └── Faults.cs                  # GeometryFault union (band 2400), GeometryKeyPolicy ordinal accessor — consolidated last
```

`Predicate.cs` and `Expansion.cs` land first because every higher robust-geometry owner reads a `Sign` verdict. `SpatialIndex.cs` follows the numeric floor — healing broad-phases through it. `TopoName.cs` precedes `NamingHash.cs` because `Reconcile` projects the `NameTable` the `Track` fold emits. `Heal.cs` composes the predicates (degenerate/self-intersection robustness) and the index (self-intersection broad-phase). `ConstraintSolver.cs` composes the admitted MathNet dense lane. `Faults.cs` consolidates the one `GeometryFault` band-2400 family and the one `GeometryKeyPolicy` last.

## [2]-[OWNER_REGISTRY]

The single owner-state surface for the absorbed robust-geometry domain. Implementation collapses to one owner per axis and one entrypoint family per rail; density means no parallel rails, no near-duplicate shapes, no re-derived logic — a file is as large as its owner's concern requires, never trimmed to a line count. A new feature is a row or case, never a new surface; a public type outside these owner regions is the named defect. `[STATE]` is `FINALIZED` where the owner is a transcription-complete fence with no open gate, `SPIKE` where the owner is fence-complete but its proof carries a residual numeric/cross-lane/native probe named in the page RESEARCH cluster; a SPIKE owner is fully shaped now, never a deferred surface. This is the ONLY place owner state lives — FEATURES, TASKLOG, and README route here.

| [INDEX] | [AXIS/RAIL]                  | [OWNER]            | [KIND]                                                                 | [CASES]                          | [PAGE#CLUSTER]                       |  [STATE]  |
| :-----: | :--------------------------- | :----------------- | :-------------------------------------------------------------------- | :------------------------------- | :----------------------------------- | :-------: |
|   [1]   | sign verdict                 | `Sign`             | `[SmartEnum<int>]` ternary                                            | 3 rows                           | geometry-kernel#ROBUST_PREDICATES    | FINALIZED |
|   [2]   | exact predicates             | `Predicate`        | static surface, filter-then-exact members                            | 4 (Orient2D/Orient3D/InCircle/InSphere) | geometry-kernel#ROBUST_PREDICATES | FINALIZED |
|   [3]   | expansion arithmetic         | `Expansion`        | `readonly struct` + construction folds + `SignOf`                    | 7 folds                          | geometry-kernel#INTERIOR_NUMERICS    | FINALIZED |
|   [4]   | filter stage                 | `ErrorBound`       | static-permanence `record` rows + `Of → Stage`                       | 4 rows                           | geometry-kernel#INTERIOR_NUMERICS    | FINALIZED |
|   [5]   | interior-double policy        | `NumericsPolicy`   | static const owner (the units-boundary exception + coefficients)     | —                                | geometry-kernel#INTERIOR_NUMERICS    | FINALIZED |
|   [6]   | spatial index                | `SpatialIndex`     | `[Union]` (`Bvh`/`LinearOctree`) over one `NodeStore` + Build/Refit/ToAcceleration | 2 cases             | spatial-index#SPATIAL_INDEX          | SPIKE     |
|   [7]   | spatial query                | `SpatialQuery`     | `[Union]` folded by one `Query`                                      | 4 (Nearest/Range/Ray/Overlap)    | spatial-index#SPATIAL_INDEX          | FINALIZED |
|   [8]   | entity modality              | `EntityKind`       | `[SmartEnum<int>]` Vertex/Edge/Face + signature-arity column          | 3 rows                           | topology#TOPO_NAMING                 | FINALIZED |
|   [9]   | stable lineage reference     | `TopoName`         | `[ValueObject<UInt128>]` one naming algebra over all kinds + `Mint`   | —                                | topology#TOPO_NAMING                 | FINALIZED |
|  [10]   | persistent topological naming | `NameTable`/`TopoNaming` | immutable registry + signature index + `Track` re-anchor fold | 3 outcomes (Survived/Migrated/Born) | topology#TOPO_NAMING              | SPIKE     |
|  [11]   | canonical adjacency           | `CanonicalTopology` | immutable hash-friendly record + `OfMesh` canonical-order encoder    | —                                | topology#NAMING_HASH                 | SPIKE     |
|  [12]   | naming↔hash reconciliation   | `NamingHash`       | static surface + `Encode` + `Reconcile` fold                         | 2                                | topology#NAMING_HASH                 | SPIKE     |
|  [13]   | healing rail                 | `Heal`/`HealOp`    | `[SmartEnum<string>]` `HealKind` + `[Union]` repair algebra + `Repair` fold | 6 author-kernel + 1 SPIKE   | healing#HEALING                      | SPIKE     |
|  [14]   | rebuild receipt              | `RebuildReceipt`   | `[Union]` typed per-heal-kind evidence                               | 7 cases                          | healing#REBUILD_RECEIPTS             | FINALIZED |
|  [15]   | constraint solver            | `ConstraintSolver`/`Constraint` | `[Union]` residual+Jacobian algebra + author-kernel LM `Solve` | 9 constraints           | constraints#CONSTRAINT_SOLVER        | FINALIZED |
|  [16]   | DOF verdict                  | `DofAnalysis`      | `[SmartEnum<int>]` Well/Under/Over + structural-rank `Analyze`        | 3 rows                           | constraints#CONSTRAINT_SOLVER        | FINALIZED |
|  [17]   | fault family                 | `GeometryFault`    | `[Union]` fault, band 2400                                            | consolidated                     | (Geometry/Faults.cs)                 | FINALIZED |
|  [18]   | key policy                   | `GeometryKeyPolicy` | ordinal comparer accessor                                            | ordinal                          | (Geometry/Faults.cs)                 | FINALIZED |

One rail per entrypoint, named in the return type: a `Sign`/`QueryResult`/`DofAnalysis` verdict where the result is total, `Fin<T>` where a band-2400 `GeometryFault` can route. The interior numerics owners (`Expansion`/`ErrorBound`/`NumericsPolicy`) and the healing weld inner loop are the sanctioned raw-`double` scope ([5]-[BOUNDARIES]).

## [3]-[DEPENDENCY_DIRECTION]

| [INDEX] | [PROJECT]          | [MAY_REFERENCE_RASM] | [RASM_MAY_REFERENCE] | [BOUNDARY]                                          |
| :-----: | :----------------- | :------------------: | :------------------: | :------------------------------------------------- |
|   [1]   | `Rasm.AppHost`     |         yes          |          no          | kernel sits below the runtime spine                |
|   [2]   | `Rasm.AppUi`       |         yes          |          no          | consumes the kernel predicates, index, and naming below the UI |
|   [3]   | `Rasm.Compute`     |         yes          |          no          | `ClashScale` consumes the spatial index            |
|   [4]   | `Rasm.Persistence` |         yes          |          no          | `GeometryHash` content-addresses the canonical adjacency |
|   [5]   | `Rasm.Vectors`     |    operator source    |        yes (compose)  | OPERATOR territory the geometry domain composes, never modifies |
|   [6]   | host packages      |    inbox/numeric      |          no          | RhinoCommon/MathNet/CSparse stay host/library-owned |

`Rasm` is the kernel: it is referenced by AppHost, AppUi, Compute, Persistence, and the AEC-domain `Rasm.Fabrication` owners and references none of them. `Rasm.Vectors` is OPERATOR territory the robust-geometry domain composes as settled vocabulary — predicates take `Vectors` coordinates, the index stores `Vectors` AABBs, healing and naming operate over `Vectors` mesh adjacency — and never re-mints a vector, matrix, frame, or mesh type. The robust-geometry domain adds a higher-order capability layer (exactness, naming, healing, constraints) FROM these primitives, never a thinner face over them; the portable fabrication frontier lives in `Rasm.Fabrication`, which composes the predicate floor and the index.

## [4]-[SEAMS]

Every two-package fact splits by altitude: mechanics live at the named `Rasm` domain cluster, consequences land at the consumer. Intra-language seams ride `pkg/page#CLUSTER`; the cross-language consequences ride the Tier-0 `region-map/seam-splits.md`.

| [INDEX] | [SEAM]                | [MECHANICS_AT]                | [CONSEQUENCE_AT]                                                              |
| :-----: | :-------------------- | :---------------------------- | :--------------------------------------------------------------------------- |
|   [1]   | clash acceleration    | spatial-index#SPATIAL_INDEX   | `ClashScale.Detect` collision primitive consumed by `csharp:Compute/solver-and-optimization#CLASH_AND_TWIN` |
|   [2]   | structural-diff identity | topology#NAMING_HASH       | `GeometryHash` content-address consumed by `csharp:Persistence/version-control#STRUCTURAL_DIFF`; `NamingHash.Reconcile` maps refs to hashes |

## [5]-[BOUNDARIES]

- `Rasm` is the geometry/numeric kernel: not a runtime spine, UI package, persistence package, compute implementation, or host-boundary package. It owns geometric substance; app packages own attachment, projection, durability, and execution policy.
- The robust-geometry domain admits NO external geometry library (no ManifoldNET, SpatialMapping.Core, GPL constraint solver); every kernel is authored from first principles. The numeric-lane packages (CSparse, MathNet) back the constraint solver's dense linear algebra only, composed never re-minted.
- The robust-geometry domain mints NO second content hash, NO second acceleration structure, and NO re-minted `Vectors` primitive. The Persistence `GeometryHash` is the one content-address identity; `TopoName` is the orthogonal reference identity; `NamingHash.Reconcile` is the one bridge.
- RATIFIED UNITS-BOUNDARY EXCEPTION (settled law): the exact/expansion-arithmetic predicate kernels (`orientation`/`incircle`/`insphere`) and the tolerance-weld interior of the healing rail are a SANCTIONED interior exception to the Compute `units-boundary#QUANTITY_ADMISSION` law that interior numerics stay raw doubles converted once at admission. Filter-then-exact robust arithmetic — a fast IEEE-754 `double` determinant filter with an exact expansion (two-product/two-sum, Shewchuk-style nonoverlapping expansions) fallback only where the filtered interval straddles zero — is the only correct robust formulation and is mathematically defined over IEEE-754 doubles, never unit-bearing quantities. `Expansion`, `ErrorBound`, `NumericsPolicy`, and the healing weld inner loop are the sanctioned interior-`double` owners; the kernel runs filter-then-exact internally and emits only a `double` that is itself the canonical geometric coordinate at the seam (a coordinate is the domain's native scalar, not a quantity), so NO quantity type and NO second numeric-domain owner crosses the boundary. An interior `double` escaping a public signature outside this scope is the named seam violation; introducing interior doubles anywhere else is the named defect.

## [6]-[PROHIBITIONS]

The closed NEVER list — the deleted patterns the owner registry forecloses.

- NEVER a public type outside the OWNER_REGISTRY owner regions; a new capability is a row or case on an existing axis, never a sibling surface.
- NEVER an external geometry library admission; the predicate, index, naming, healing, and constraint kernels are author-kernel.
- NEVER a re-minted `Rasm.Vectors` primitive — no domain-local vector, matrix, frame, or mesh type; compose the operator-owned source and modify it never.
- NEVER a second content hash or node-identity; the Persistence `GeometryHash` is the one identity, `NamingHash.Reconcile` is the one bridge, and `TopoName.Value` is never equality-tested against a content hash.
- NEVER a second acceleration structure beside `SpatialIndex`; the domain deepens the substance app packages project and consume.
- NEVER an interior `double` escaping the sanctioned predicate/expansion/weld scope ([5]); a public-signature interior double outside that scope is the named defect.
- NEVER a loosened predicate to pass a near-degenerate case — fix the expansion/error-bound stage; a sign verdict is exact or it is a defect.
- NEVER exception-style control flow in domain logic; faults route through the one `GeometryFault` union (band 2400) and `Fin`/`Validation`/`Eff` rails.
- NEVER a generic `IReceipt`/ledger abstraction; `RebuildReceipt` and `SolveReceipt` stay typed per operation.
- NEVER sibling factory methods over predicate/query/heal/constraint surfaces; one polymorphic owner discriminates by input value or case.
- Analyzer diagnostics are architecture pressure: fix the shape, refine the rule on a false positive, never suppress.
