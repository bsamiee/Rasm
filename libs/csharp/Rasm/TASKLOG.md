# [RASM_TASKLOG]

The kernel's open and closed work, distilled from the ideas and the design-page RESEARCH residuals. Each task is a card whose leader carries a status marker — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` open, `[COMPLETE]`/`[DROPPED]` closed — plus the capability or file to build, the external packages to integrate, the integration points and boundaries, and the key considerations. `[1]-[OPEN]` carries live work; `[2]-[CLOSED]` records settled or dropped tasks.

## [1]-[OPEN]

[QUEUED] Predicate sign-exactness law-matrix (`numerics/predicates#NUMERIC_DETERMINISM`).
- A `PredicateLaws` CsCheck property suite asserting every `Predicate` member returns the exact sign of the `BigInteger` rational determinant on perturbed near-degenerate input, plus antisymmetry, filter-vs-exact agreement, and translation invariance.
- Integrate CsCheck and the active repo testkit under `testing-cs`; `System.Numerics.BigInteger` as the exact oracle.
- Internal to the numerics sub-domain — proves the predicate floor every higher owner rides; no cross-package wire. The FMA-availability assumption (a future FMA-free RID routes the Dekker-split fallback) is the only deferred numeric probe, gating a fallback row, never the FMA-path predicates.
- The kernel is total by construction; the harness is the law authority, not a structure gate.

[QUEUED] Re-anchor injectivity harness (`topology/naming#REANCHOR_INJECTIVITY`).
- A tier-2 property harness driving rebuild-matching: apply a labelled topological operation (rigid move/face split/vertex insert/edge collapse), assert the `TrackOutcome` matches the expected lineage class and the next-generation `NameTable` is injective per `EntityKind`.
- Integrate CsCheck under `testing-cs`; a golden rebuild fixture frozen against `HashMap` enumeration order.
- Internal to the topology sub-domain; the `TopoSignature.Subsumes` parent-subset predicate and the deterministic smallest-`TopoName` tiebreak are under test. The OPEN residual is the Survive×Migrate cross-case injectivity the per-`EntityKind` `claimed` set must prove non-colliding.
- The boundary-storage shape is pinned (`NameEntry.Boundary` + `NameTable.VertexNames`); only the injectivity proof remains.

[BLOCKED] Canonical-adjacency byte-identity golden fixture vs the Persistence `GeometryHash` (`topology/reconciliation#CANONICAL_BYTE_IDENTITY`).
- A cross-package byte-equality harness feeding the single-triangle reference through both `NamingHashOps.Encode` and the Persistence `GeometryHash` path, asserting the 52-byte stream and the `0x9462A71A5DD13DCFA3B1D6D225FCBE70` digest plus the morph (equal hash) and topology-break (distinct hash) discriminating laws against the shared frozen golden-bytes fixture.
- Integrate `System.IO.Hashing` (`XxHash128`); CsCheck and the testkit under `testing-cs`.
- The canonical int32-LE adjacency layout is the AGREED frozen contract this page solely owns; `csharp:Persistence/versioning/version-control#STRUCTURAL_DIFF` `GeometryHash`/`StructuralMerge` reads the IDENTICAL layout VERBATIM and never re-derives a second encoding. The live-host `Mesh.TopologyVertices.IndicesFromFace` boundary-cycle winding spelling is confirmed via `Rasm.Vectors`.
- Blocked only on the frozen golden-bytes fixture existing in both packages' test scope; confirm `IndicesFromFace` returns consistent winding before the rotation law finalizes.

[BLOCKED] Clash-seam node-link golden fixture (`spatial/index#CLASH_SEAM`, `#CLASH_GOLDEN`).
- The node-link acceleration layout is the AGREED frozen contract: `SpatialIndex.ToAcceleration`/`NodeLinkProjection` emits the per-node interleaved AABB plus the `(FirstChild << 21) | ChildCount` descriptor with a leaf primitive-id tail, and the Compute `ClashScale.BvhPairs` decode descends the same contiguous `[FirstChild, FirstChild+ChildCount)` child range — the prior flat per-primitive O(N²) decode is the deleted form on both pages.
- No new package; the seam is the `Rasm.Compute.Solver` `AccelerationStructure` union the projection returns directly; integrate the testkit under `testing-cs`.
- The contract is settled across the kernel/app-platform boundary, aligned to `csharp:Compute/solver/clash#CLASH_AND_TWIN`; the residual is the `[CLASH_GOLDEN]` two-sided frozen golden-bytes fixture — the canonical 8-primitive `BoundingBox[]` set built with `BuildPolicy.Canonical`, this page's spec asserting `NodeLinkProjection` emits those bytes and the Compute `solver/clash#CLASH_GOLDEN` spec asserting `BvhPairs` decodes them into the agreed clash-pair set.
- Blocked only on that two-sided golden-bytes fixture existing and passing on both sides; the degradation-keyed refit and agglomerative builder ride the same projection transparently.

[BLOCKED] Boolean tier-3 native arrangement asset gate (`healing/repair#BOOLEAN_NATIVE_ASSET`).
- The `HealOp.Boolean` row is fence-complete in shape (the case, the `BooleanOp` discriminant, the `BooleanReceipt`, the `Apply` gate routing `NativeAssetMissing`) but carries no managed CSG kernel; the algorithm contract is exact-arithmetic mesh-arrangement classification with a robust ray-parity inside/outside test grounded on `Predicate.Orient3D`.
- Integration awaits a robust exact-arithmetic mesh-arrangement native asset (no admissible managed manifold library exists; ManifoldNET alpha rejected); the kept-cell boundary welds through the `DuplicateWeld` kernel.
- Internal to the healing sub-domain; the row gains its managed body only on a charter package admission with its RID burden assessed, then must pass the repair kernels' post-conditions against a golden boolean fixture.
- Blocked on the native-asset admission; the `INDIRECT_PREDICATES` idea offers a fully-managed exact path that could retire this dependency for the common cases.

[QUEUED] LM convergence harness (`constraints/solver#LM_CONVERGENCE`).
- A `ConstraintLaws` CsCheck property suite asserting `Solve` converges on every feasible system, the analytic Jacobian matches a central-finite-difference Jacobian, an over-constrained-inconsistent system routes `GeometryFault.OverConstrained`, rigid-transform invariance, and idempotence.
- Integrate CsCheck and the testkit under `testing-cs`; MathNet.Numerics (`Cholesky`) as the linear-solve substrate under test.
- Internal to the constraints sub-domain. The singular-Jacobian path (λ-ladder climbing to `SingularSystem` before the ceiling, including NaN-trial rejection on a numerically-indefinite damped matrix) and the bounded inner reject-chain budget are the convergence-edge probes.
- The LM iterate is a total damped descent by construction; the harness asserts the worst-case step budget terminates at the λ ceiling.

[QUEUED] Indirect-predicate family extension (idea `INDIRECT_PREDICATES`).
- Add LPI/TPI implicit-point predicates to `numerics/predicates.md` as new `Predicate` members riding the same `ErrorBound` stage and `Expansion` fold, evaluating the exact sign over constructed intersection points without rounding.
- Integrate the existing `Expansion`/`ErrorBound` numerics and `System.Numerics.BigInteger` for the law-matrix oracle.
- Internal to the numerics sub-domain; consumed by the healing self-intersect split and the planned constrained-Delaunay owner. New error bands are `ErrorBound` rows; zero new surface.
- Each new predicate needs its forward-error coefficient derived once from `Epsilon`; the law-matrix extends to assert exact-sign agreement over implicit points.

[QUEUED] Generalized-winding-number query (idea `GENERALIZED_WINDING`).
- A fast hierarchical GWN evaluated as a `SpatialQuery` case over the existing `SpatialIndex` BVH (Barill/Jacobson tree-based), returning a robust inside/outside scalar over defective triangle soups.
- No new external package; composes the spatial BVH and the `Predicate.Orient3D` sign.
- Internal to the spatial sub-domain as a query case, consumed by the healing boolean-arrangement cell classification and the watertight-repair verdict; reuses the BVH so it is a query, not a new structure.
- The GWN tree node aggregates per-node solid-angle/dipole moments onto the `NodeStore`; the query is a best-first descent with the standard far-field approximation.

[QUEUED] Witness-configuration DOF analysis (idea `WITNESS_DOF`).
- Upgrade `DofAnalysis` in `constraints/solver.md` to derive the true numeric DOF and detect over/under/redundant constraints by analyzing the Jacobian rank at a witness configuration, beside the structural row-count verdict.
- Integrate MathNet.Numerics for the rank/SVD analysis at the witness.
- Internal to the constraints sub-domain; the verdict gates the same `Solve` rail and feeds UI over-constraint diagnosis. A new verdict refinement is a `DofAnalysis` row or column, never a parallel analyzer.
- The witness is a known feasible configuration; the numeric rank at the witness distinguishes redundant-but-consistent from structurally-determined systems the row count misclassifies.

[QUEUED] Delaunay correctness law-matrix (`tessellation/delaunay#BOWYER_WATSON_DELAUNAY`, `#CONSTRAINT_RECOVERY`).
- A `DelaunayLaws` CsCheck property suite asserting the empty-circumcircle/sphere property after every insertion against a `BigInteger` exact Delaunay oracle, the valid-simplicial-complex invariant (reciprocal neighbour links, no overlap), rigid-transform invariance, and that every constraint segment/facet is present in the recovered complex within the flip/Steiner budget.
- Integrate CsCheck and the testkit under `testing-cs`; `System.Numerics.BigInteger` as the exact in-circum oracle; the `Rasm.Geometry.Numerics` `Predicate` floor under test.
- Internal to the `Tessellation` sub-domain; proves the Bowyer-Watson cavity and the constraint recovery the page seats. The `healing/repair#HEALING` `SelfIntersectResolve`, the `healing/repair#BOOLEAN_NATIVE_ASSET` arrangement gate, and the AEC-domain fabrication/nesting consumers ALIGN to `Build`/`ToMesh` as future wires, never by coupling into the `SimplexStore` interior.
- Depends on `INDIRECT_PREDICATES` for the constructed-point (Steiner-vertex) flip robustness; the constrained-recovery law row is held until the LPI/TPI implicit-point predicate family lands in `numerics/predicates.md`.

[QUEUED] Degradation-keyed refit and agglomerative builder (idea `DEGRADATION_REFIT`).
- Extend `spatial/index.md`: add a surface-area-cost degradation field to `Refit` that rebuilds when refitted quality drops past a `BuildPolicy` threshold, and add an agglomerative bottom-up `SpatialKind`/`Builders` row (Morton-presorted nearest-neighbour clustering) writing the same `NodeStore`.
- No new external package; composes the existing `SpatialIndex` `Build`/`Refit` rail, the SAH cost scan, and the Morton sort already authored.
- Internal to the spatial sub-domain — a new builder is one `SpatialKind` row plus one `Builders` `FrozenDictionary` row, the degradation trigger is `BuildPolicy` columns; the `csharp:Compute/solver/clash#CLASH_AND_TWIN` consumer ALIGNS to the same `ToAcceleration` projection unchanged (the rebuild is transparent to the seam), never a coupling edit into the Compute owner.
- The degradation metric is the refitted root surface-area against the last full-build cost; the rebuild trigger must be deterministic so an incremental session is reproducible, and the agglomerative builder's wider nodes ride the existing contiguous `[FirstChild, FirstChild+ChildCount)` child run with no layout change.

[QUEUED] Mature-folder `Geometry` naming reconciliation (`Domain/Geometry.cs` vs the `Geometry` sub-domain).
- The mature `Domain/Geometry.cs` owner and the greenfield `Geometry/` robust-core sub-domain share the name `Geometry`; reconcile so the `Rasm.Geometry.*` robust-core namespaces do not collide with the `Domain` geometry-normalization owner.
- No external package; a co-located source rename or namespace re-scope.
- Internal to the kernel — aligns the mature `Domain` source with the greenfield `Geometry` sub-domain tree before transcription lands `Rasm.Geometry.Numerics`/`Spatial`/`Topology`/`Healing`/`Constraints`/`Faults`.
- Settle the rename before the transcription task creates the `Geometry/` source tree so no two owners claim the name.

## [2]-[CLOSED]

None.
