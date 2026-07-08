# [RASM_BIM_API_SWIFTCOLLECTIONS]

`SwiftCollections.Lean` is the 3D AABB broad-phase owner backing the
`Model/systems#INTERFERENCE` clash-candidate build and the
`Review/coordination#COORDINATION` `ClashProposal` fold, replacing the O(N^2)
pairwise element test. The `SwiftCollections.Query` namespace provides three
interchangeable spatial structures over one generic AABB contract
(`IBoundVolume<TVolume>`): a refittable `SwiftBVH` (SAH-cost insertion, the
default broad-phase), a `SwiftOctree`, and a `SwiftSpatialHash` — each exposing
the identical `Insert(key, bounds)` / `UpdateEntryBounds` / `Query(bounds,
results)` / `Remove` surface, so the clash engine is written once against the
contract and the structure is a tuning choice. The built-in `BoundVolume` is a
`System.Numerics.Vector3` AABB; the handle-stable `SwiftBucket`/`SwiftSparseMap`
collections back the entity-id↔volume registry without per-element allocation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SwiftCollections.Lean`
- package: `SwiftCollections.Lean`
- license: file `LICENSE` (MIT)
- assembly: `SwiftCollections`
- namespace: `SwiftCollections.Query`, `SwiftCollections`, `SwiftCollections.Pool`, `SwiftCollections.Dimensions`, `SwiftCollections.Diagnostics`
- asset: multi-target net8.0 + netstandard2.1; the net10.0 consumer binds `lib/net8.0` (highest applicable; `BoundVolume` uses `System.Numerics.Vector3`, AnyCPU, no `runtimes/` folder) — `.Lean` is the MemoryPack-free variant
- asset: managed transitive `Chronicler.Core.Lean (the zero-dep diagnostic core), floor-pinned centrally
- rail: clash

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: spatial broad-phase structures
- package: `SwiftCollections.Lean`
- namespace: `SwiftCollections.Query`
- rail: clash

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:-------------------------------- |:----- |:---------------------------------------------------------------------------------------------------------------------------------- |
| [01] | `SwiftBVH<TKey>` / `SwiftBVH<TKey, TVolume>` | clash | refittable bounding-volume hierarchy; SAH-cost insertion, `EnsureCapacity`, `NodePool`/`RootNode` exposed — the default broad-phase |
| [02] | `SwiftOctree<TKey>` / `SwiftOctree<TKey, TVolume>` | clash | spatial octree; depth/node-capacity bounded subdivision with merge-on-remove |
| [03] | `SwiftSpatialHash<TKey>` / `SwiftSpatialHash<TKey, TVolume>` | clash | uniform-grid spatial hash; adds `QueryNeighborhood` over the padded cell ring |
| [04] | `BoundVolume` | clash | `struct` AABB over `Vector3 Min`/`Max`; `Center`/`Size`/`Volume`, `Union`, `Intersects`, `GetCost` (SAH surface-area cost), `BoundsEquals` |
| [05] | `IBoundVolume<TVolume>` | clash | the generic AABB contract (`Union`/`Intersects`/`GetCost`/`BoundsEquals`) a custom volume implements to plug into all three structures |
| [06] | `SwiftOctreeOptions` | clash | `(int maxDepth, int nodeCapacity[, bool enableMergeOnRemove])` octree tuning struct |
| [07] | `SwiftSpatialHashOptions` | clash | `(int neighborhoodPadding)` with static `.Default`; controls the `QueryNeighborhood` ring radius |

[PUBLIC_TYPE_SCOPE]: handle-stable backing collections
- package: `SwiftCollections.Lean`
- namespace: `SwiftCollections`
- rail: clash

The clash registry maps a `BimElement` GlobalId to its AABB; these allocation-
conscious structures hold that mapping with stable integer handles so a model
update mutates one slot instead of rebuilding the index.

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:-------------------------------- |:----- |:-------------------------------------------------------------------------------------------------------------------- |
| [01] | `SwiftBucket<T>` | clash | dense slab with stable int handles: `int Add(T)` returns a reusable index, `TryRemoveAt`/`TryGetValue`, `PeakCount` |
| [02] | `SwiftSparseMap<T>` | clash | sparse int-keyed map; O(1) add/remove/lookup with dense iteration — the GlobalId-hash→volume registry |
| [03] | `SwiftSparseSet` | clash | sparse int set for the candidate-pair dedupe |
| [04] | `SwiftList<T>` / `SwiftHashSet<T>` | clash | low-overhead list/set the `Query(bounds, ICollection<TKey>)` result sink fills; both implement `IStateBacked<SwiftArrayState<T>>` for a snapshot of the candidate buffer |
| [05] | `IStateBacked<TState>` | clash | snapshot/restore contract the backing collections (`SwiftList`/`SwiftHashSet`/`SwiftBucket`/`SwiftSparseMap`) implement — the spatial `Query` structures do NOT; a deterministic clash receipt snapshots the registry, not the index node-pool |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: broad-phase build and query
- package: `SwiftCollections.Lean`
- namespace: `SwiftCollections.Query`
- rail: clash

All three structures share this polymorphic surface (`TKey` = element handle,
`TVolume` = AABB); the design binds the contract, not a concrete structure.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:----------------------- |:--------------------------------------------------- |:--------------------------------------------------------------- |
| [01] | `new SwiftBVH<T>` | `(int capacity)` | pre-sized BVH; `SwiftOctree`/`SwiftSpatialHash` take their options struct |
| [02] | `Insert` | `(TKey key, TVolume bounds)` → `bool` | inserts an element AABB into the index |
| [03] | `UpdateEntryBounds` | `(TKey key, TVolume newBounds)` | refits one entry in place — the incremental update on a moved element, no full rebuild |
| [04] | `Remove` | `(TKey key)` → `bool` | removes an element from the index |
| [05] | `Query` | `(TVolume queryBounds, ICollection<TKey> results)` | fills the caller's result sink with every entry overlapping `queryBounds` — the broad-phase candidate set |
| [06] | `SwiftSpatialHash.QueryNeighborhood` | `(TVolume queryBounds, ICollection<TKey> results)` | widens the query by the padded cell ring for proximity/clearance checks |
| [07] | `TryGetBounds` | `(TKey key, out TVolume bounds)` → `bool` | reads back a stored AABB (octree/spatial-hash) |
| [08] | `Contains` / `Count` | `(TKey key)` → `bool` / property | membership and entry count |
| [09] | `EnsureCapacity` / `Clear` | `(int capacity)` / `()` | pre-grow the node pool / reset the index for the next model snapshot |

[ENTRYPOINT_SCOPE]: BoundVolume — AABB algebra
- package: `SwiftCollections.Lean`
- namespace: `SwiftCollections.Query`
- rail: clash

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------- |:--------------------------------------- |:------------------------------------------------------- |
| [01] | `new BoundVolume` | `(Vector3 min, Vector3 max)` | constructs an AABB from a geometry bounding box |
| [02] | `BoundVolume.Union` | `(BoundVolume other)` → `BoundVolume` | merged AABB — the BVH internal-node bound |
| [03] | `BoundVolume.Intersects` | `(BoundVolume other)` → `bool` | AABB overlap test — the broad-phase predicate |
| [04] | `BoundVolume.GetCost` | `(BoundVolume other)` → `long` | SAH surface-area cost driving BVH insertion placement |
| [05] | `BoundVolume.Center` / `Size` / `Volume` | properties | derived AABB metrics |

## [04]-[IMPLEMENTATION_LAW]

[IDENTITY_PROFILE]:
- namespace: `SwiftCollections.Query`
- broad-phase root: `SwiftBVH<TKey, TVolume>` (default) over `SwiftOctree` / `SwiftSpatialHash`
- volume contract: `IBoundVolume<TVolume>` (`Union`/`Intersects`/`GetCost`/`BoundsEquals`)
- built-in volume: `BoundVolume` (`Vector3` AABB)
- receipt root: structure kind, entry count, and candidate-pair count

[CLASH_COMPOSE]:
- structure-polymorphic broad-phase: the INTERFERENCE engine binds the shared `Insert`/`UpdateEntryBounds`/`Query` surface, so `SwiftBVH` (default, SAH-cost), `SwiftOctree`, and `SwiftSpatialHash` are interchangeable behind one code path — the structure is a `CoordinationRule` tuning choice, never three forked clash implementations.
- two-phase clash: this package is the BROAD phase — `Query(elementBounds, results)` returns AABB-overlapping candidates; the NARROW phase (exact mesh/solid intersection) is the kernel `Rasm` geometry concern. The `ClashProposal` fold consumes the candidate set, runs the narrow test, and authors a `BcfTopic` (`Smino.Bcf.Toolkit`) per confirmed clash — broad-phase, narrow-phase, and issue-exchange are three distinct owners meeting at the candidate set and the `ElementPredicate` algebra.
- 3D vs. 2D index split: `SwiftBVH`/`SwiftOctree` own the 3D AABB broad-phase (volumetric clash); `NetTopologySuite`'s STRtree/Quadtree owns the 2D planar Simple-Features index (georeferenced-BIM overlay). The COORDINATION owner selects the 3D index for element-vs-element clash and the NTS 2D index for footprint/site predicates — neither reimplements the other's dimension.
- incremental refit: `UpdateEntryBounds` refits a single moved element in place, so a `ModelDiff`'s `moved` arm updates only the changed AABBs instead of rebuilding the whole BVH — the clash re-run over a design iteration is incremental.
- handle-stable registry: `SwiftSparseMap<T>`/`SwiftBucket<T>` hold the GlobalId-hash→`BoundVolume` map with stable integer handles (`TKey`), so the `Insert`/`UpdateEntryBounds`/`Remove` keys never re-hash a GlobalId string per query, and `Clear` + reuse resets the index for the next snapshot with zero re-allocation.
- snapshot receipt: the spatial `Query` structures expose no state snapshot, but the `IStateBacked`-backed registry collections (`SwiftSparseMap`/`SwiftBucket`/`SwiftList`) do — a deterministic, replayable `ClashProposal` receipt snapshots the GlobalId→AABB registry and the candidate-pair buffer, then re-builds the index by replaying `Insert` over the restored registry.

[LOCAL_ADMISSION]:
- `SwiftCollections.Lean` provides spatial broad-phase indexing and allocation-conscious collections only; it carries no exact-intersection geometry, no clash policy, and no issue model.
- Narrow-phase intersection, clash rules, and `BcfTopic` authoring are kernel/COORDINATION concerns, never the index's.
- Structure kind, entry count, and candidate-pair count are receipt facts the INTERFERENCE/COORDINATION fold records.
- The broader `SwiftCollections.Pool`/`Dimensions`/`Diagnostics` surfaces are general-purpose; this folder admits the package for `SwiftCollections.Query` and the handle-stable registry collections, not as a general logging or pooling owner.

[RAIL_LAW]:
- Package: `SwiftCollections.Lean`
- Owns: 3D AABB broad-phase (BVH/octree/spatial-hash) candidate generation
- Accept: the INTERFERENCE clash-candidate build and the COORDINATION clash fold's broad phase
- Reject: exact narrow-phase intersection, clash policy/rules, the 2D planar Simple-Features index (NTS), the BCF issue model
