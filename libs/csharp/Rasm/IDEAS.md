# [RASM_IDEAS]

The forward pool of higher-order kernel concepts, grounded in the robust-geometry domain and the monorepo geometry-flow. Each is a card: a bracketed slug leader plus the capability, what it unlocks, and the gap or modern technique it draws on. `[1]-[OPEN]` carries live ideas; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition so it is never re-litigated.

## [1]-[OPEN]

[INDIRECT_PREDICATES]: extend the numerics floor from the four classical direct predicates to the indirect-predicate family (Attene/Cherchi).
- Predicates that evaluate the exact sign over IMPLICIT points (LPI line-plane and TPI three-plane intersection points) without ever materializing a rounded coordinate, riding the same `ErrorBound` filter-then-exact stage and `Expansion` fold the direct predicates use.
- Unlocks exact-arithmetic constructed-point pipelines: the self-intersect split in healing chains robustly, constrained-Delaunay becomes admissible as an author-kernel, and the tier-3 boolean gate gains a fully-managed exact path that retires the native-asset dependency for the common cases.
- The current `Predicate` surface accepts only explicit `Point3d` coordinates, so any constructed intersection point must round before the next predicate runs — exactly where robust mesh-arrangement and constrained-Delaunay pipelines lose their guarantee; Levy 2025 mesh-CSG combinatorics build directly on this implicit-point family.

[GENERALIZED_WINDING]: a fast hierarchical generalized-winding-number owner (Barill/Jacobson tree-based GWN) as a numerics/spatial co-owned capability.
- Robust inside/outside classification over imperfect triangle soups regardless of holes, self-intersection, or non-manifold defects — a single robust scalar field that subsumes inside/outside, evaluated as a query case over the existing `SpatialIndex` BVH rather than a new structure.
- Unlocks a defect-tolerant inside/outside primitive the boolean-arrangement cell classification, the watertight-repair verdict, and a future SDF-from-soup rail all compose.
- Healing's manifold/boolean classification and the `Vectors` SDF rails currently lean on point-in-solid or ray-parity tests that are fragile on defective input — the exact case the kernel exists to harden; fTetWild-class pipelines use fast winding for final filtering.

[CONSTRAINED_DELAUNAY]: an author-kernel constrained Delaunay triangulation/tetrahedralization owner in the planned `Tessellation` sub-domain.
- Grounded on the `InCircle`/`InSphere` exact predicates and the indirect-predicate family, with the incremental flip algorithm and robust segment/facet recovery.
- Unlocks robust remeshing, gap-filling that respects constraint edges, the arrangement substrate the boolean gate needs, and a tessellation owner the AEC-domain fabrication/nesting packages compose — all from the predicates already authored.
- The kernel owns the exact in-circle/in-sphere predicates but no triangulator consumes them; healing's `SelfIntersectResolve` retriangulates a single offending face against the appended crossing point — a local fan, not a real Delaunay re-mesh that restores the empty-circumcircle property across the patch — and robust tet-meshing pipelines are built exactly on this predicate-grounded CDT core.

[WITNESS_DOF]: upgrade the constraint solver's structural DOF verdict to the witness-configuration method.
- Derive the true numeric DOF and detect over/under/redundant constraints by analyzing the Jacobian rank at a witness (a known feasible configuration), not only by counting residual rows against parameters.
- Unlocks accurate well/under/over/redundant diagnosis, identification of the specific over-constraining constraint for UI feedback, and a foundation for incremental DOF-based graph decomposition that scales the solver to large sketches.
- `DofAnalysis` is currently structural-only (row count vs parameter count), which misclassifies redundant-but-consistent and conditionally-redundant systems — the precise failure modes professional parametric sketchers must diagnose; the witness method (Michelucci-Foufou) is the field-standard refinement for exactly this.

[DEGRADATION_REFIT]: a degradation-keyed refit/rebuild policy and an agglomerative bottom-up builder on the spatial owner, beside the top-down SAH.
- A surface-area-cost degradation metric tracked across refits triggers a full rebuild only when the refitted hierarchy quality drops past a `BuildPolicy` threshold, and a locally-ordered-clustering (Morton-presorted nearest-neighbour agglomeration) bottom-up builder produces a higher-quality tree than top-down SAH on the clustered workloads incremental clash favors — both as `BuildPolicy` columns and `Builders` `FrozenDictionary` rows over the same `NodeStore`, never a parallel index class.
- Unlocks stable incremental clash performance under animation/edit churn (refit until quality degrades, then rebuild) and a build-quality knob per workload, feeding the Compute clash broad-phase a tree that does not silently rot across many small edits.
- The owner has SAH-BVH (top-down) and a Morton octree; `Refit` is topology-stable but unconditional — it never measures the quality it loses across repeated edits, so a long edit session degrades the tree with no rebuild trigger, and the SAH builder is the only quality builder despite the agglomerative bottom-up family being the field-standard for the clustered, incrementally-mutated geometry a clash workload presents; the `NodeStore` contiguous-child-run layout already admits the wider nodes an agglomerative builder emits.

## [2]-[CLOSED]

None.
