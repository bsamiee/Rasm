# [RASM_BIM_API_SWIFTCOLLECTIONS]

`SwiftCollections.Lean` owns the 3D AABB broad-phase behind the `Model/systems#INTERFERENCE` clash-candidate build and the `Review/coordination#COORDINATION` `ClashProposal` fold. `SwiftBVH`, `SwiftOctree`, and `SwiftSpatialHash` implement one generic `IBoundVolume<TVolume>` contract, so each answers the modality its partition fits — the BVH tight-volume overlap, the hash the padded neighborhood ring. The handle-stable `SwiftBucket`/`SwiftSparseMap` collections own the handle↔volume registry a co-indexed pair shares.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: spatial broad-phase structures

Each of `SwiftBVH`/`SwiftOctree`/`SwiftSpatialHash` ships a built-in `<TKey>` form over `BoundVolume` and a `<TKey, TVolume>` form over a custom `IBoundVolume<TVolume>` with its partition strategy.

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :---------------------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `SwiftBVH`                          | class         | refittable BVH; SAH-cost insertion, `NodePool`/`RootNode` default phase |
|  [02]   | `SwiftOctree`                       | class         | depth/node-capacity bounded subdivision with merge-on-remove            |
|  [03]   | `SwiftSpatialHash`                  | class         | uniform-grid hash; `QueryNeighborhood` over the padded cell ring        |
|  [04]   | `BoundVolume`                       | struct        | `Vector3` AABB: `Union`/`Intersects`/`GetCost`/`BoundsEquals`           |
|  [05]   | `IBoundVolume<TVolume>`             | interface     | CRTP-self-constrained AABB contract plugging all three structures       |
|  [06]   | `SwiftOctreeOptions`                | struct        | `MaxDepth`/`NodeCapacity`/`EnableMergeOnRemove` tuning                  |
|  [07]   | `SwiftSpatialHashOptions`           | struct        | `NeighborhoodPadding`, static `.Default` — the neighborhood ring        |
|  [08]   | `IOctreeBoundsPartitioner<TVolume>` | interface     | pluggable octree bounds-partition strategy                              |
|  [09]   | `ISpatialHashCellMapper<TVolume>`   | interface     | pluggable spatial-hash cell-mapping strategy                            |

- `IBoundVolume<TVolume>` is `where TVolume : struct, IBoundVolume<TVolume>` (CRTP); `BoundVolume` is the built-in `IBoundVolume<BoundVolume>`/`IEquatable<BoundVolume>` volume every `<TKey>` structure defaults to.
- Key constraints diverge: `SwiftOctree` binds `where TKey : notnull, IEquatable<TKey>` against `SwiftBVH`'s `where TKey : notnull`, so a key type usable in the BVH is not automatically admissible to the octree.

[PUBLIC_TYPE_SCOPE]: handle-stable backing collections

Each backing collection holds the member→AABB mapping under stable integer handles, so a model update mutates one slot instead of rebuilding the index.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :--------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `SwiftBucket<T>`       | class         | stable-handle dense slab: `Add`/`TryRemoveAt`/`TryGetValue`, `PeakCount` |
|  [02]   | `SwiftSparseMap<T>`    | class         | sparse int-keyed map; O(1) add/remove/lookup — the GlobalId→volume map   |
|  [03]   | `SwiftSparseSet`       | class         | sparse int set for the candidate-pair dedupe                             |
|  [04]   | `SwiftList<T>`         | class         | low-overhead ordered `Query` sink; `IStateBacked<SwiftArrayState<T>>`    |
|  [05]   | `SwiftHashSet<T>`      | class         | deduped `Query` sink; `IStateBacked<SwiftArrayState<T>>`                 |
|  [06]   | `IStateBacked<TState>` | interface     | `State` snapshot property + state ctor; spatial structures omit it       |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: broad-phase build and query

`TKey` is the element handle and `TVolume` the AABB; the design binds this shared surface, not a concrete structure.

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `new SwiftBVH(int)`                                                          | ctor     | pre-sized BVH node pool                       |
|  [02]   | `new SwiftOctree(TVolume, SwiftOctreeOptions, IOctreeBoundsPartitioner)`     | ctor     | bounded subdivision, pluggable partition      |
|  [03]   | `new SwiftOctree<TKey>(BoundVolume, SwiftOctreeOptions, float)`              | ctor     | built-in-volume form, minimum node size       |
|  [04]   | `new SwiftSpatialHash(int, ISpatialHashCellMapper, SwiftSpatialHashOptions)` | ctor     | uniform grid, pluggable cell mapper           |
|  [05]   | `new SwiftSpatialHash<TKey>(int, float[, SwiftSpatialHashOptions])`          | ctor     | built-in volume form: capacity, CELL SIZE     |
|  [06]   | `Insert(TKey, TVolume) -> bool`                                              | instance | index an element AABB                         |
|  [07]   | `UpdateEntryBounds(TKey, TVolume) -> void\|bool`                             | instance | in-place refit; return diverges per structure |
|  [08]   | `Query(TVolume, ICollection<TKey>)`                                          | instance | sink of overlapping entries — candidates      |
|  [09]   | `SwiftSpatialHash.QueryNeighborhood(TVolume, ICollection<TKey>)`             | instance | widen the query by the padded cell ring       |
|  [10]   | `Remove(TKey) -> bool`                                                       | instance | drop an element from the index                |
|  [11]   | `SwiftSpatialHash.Contains(TKey) -> bool`                                    | instance | membership probe — hash only                  |
|  [12]   | `TryGetBounds(TKey, out TVolume) -> bool`                                    | instance | read back a stored AABB (octree/hash)         |
|  [13]   | `SwiftBVH.FindEntry(TKey) -> int` (`-1` absent)                              | instance | leaf index — the BVH's only membership probe  |
|  [14]   | `Count`                                                                      | property | indexed entry count                           |
|  [15]   | `EnsureCapacity(int)` / `Clear()`                                            | instance | pre-grow the node pool / reset                |

- `UpdateEntryBounds` is the ONE surface whose return shape diverges across the three structures: `SwiftBVH<TKey,TVolume>` declares it `void` while `SwiftSpatialHash<TKey,TVolume>` and `SwiftOctree<TKey,TVolume>` both declare it `bool`, so a structure-generic refit delegate wraps the BVH leg in a block returning `true` and passes the other two as a bare method group. A pair of structures co-indexed on one handle takes the BOOLEAN leg as the refit verdict, because the void leg reports nothing.
- Membership probes diverge with it: `SwiftBVH` carries `FindEntry` and NEITHER `Contains` nor `TryGetBounds`, while `SwiftSpatialHash` carries `Contains` and `TryGetBounds` and NO `FindEntry`. `SwiftBVH.UpdateEntryBounds` silently no-ops on an unindexed key, so a BVH refit gates on `FindEntry` or on the owning handle registry first.
- `SwiftSpatialHash`'s built-in-volume ctor takes a CELL SIZE, not a padding distance: `SwiftSpatialHashOptions.NeighborhoodPadding` is a ring count over cells (`.Default` is 1), so a neighborhood covers `padding × cellSize` beyond the query volume and the cell size is what a clearance distance derives.

[ENTRYPOINT_SCOPE]: handle registry (`SwiftBucket<T>`) — the key space a co-indexed structure pair shares

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :-------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `new SwiftBucket<T>(int capacity)`      | ctor     | pre-sized dense slab                                 |
|  [02]   | `Add(T) -> int`                         | instance | seat a value and RETURN its stable slot handle       |
|  [03]   | `TryGetValue(int, out T) -> bool`       | instance | read a slot; false on an unallocated handle          |
|  [04]   | `IsAllocated(int) -> bool`              | instance | slot occupancy probe — the refit and iteration gate  |
|  [05]   | `this[int]`                             | property | get and set a slot; both throw on an unallocated one |
|  [06]   | `TryRemoveAt(int) -> bool` / `RemoveAt` | instance | free a slot, leaving every other handle stable       |
|  [07]   | `Count` / `PeakCount` / `Capacity`      | property | live count, high-water slot bound, backing length    |
|  [08]   | `Contains(T)` / `Exists(Predicate<T>)`  | instance | value membership and predicate probe                 |

- `PeakCount` is the high-water slot bound, so a full traversal ranges `0..PeakCount` and filters on `IsAllocated` — `Count` is the LIVE count and skips the freed slots a stable-handle space deliberately keeps.

[ENTRYPOINT_SCOPE]: BoundVolume — AABB algebra

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :---------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `new BoundVolume(Vector3, Vector3)`             | ctor     | AABB from a geometry bounding box           |
|  [02]   | `BoundVolume.Union(BoundVolume) -> BoundVolume` | instance | merged AABB — the BVH internal-node bound   |
|  [03]   | `BoundVolume.Intersects(BoundVolume) -> bool`   | instance | AABB overlap — the broad-phase predicate    |
|  [04]   | `BoundVolume.GetCost(BoundVolume) -> long`      | instance | SAH surface-area cost driving BVH insertion |
|  [05]   | `BoundVolume.Center` / `Size` / `Volume`        | property | derived AABB metrics                        |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every structure binds one `IBoundVolume<TVolume>` contract (`Union`/`Intersects`/`GetCost`/`BoundsEquals`), so the clash engine folds through a single code path and the concrete structure and volume type are tuning choices, never forked implementations.

[STACKING]:
- `NetTopologySuite`(`libs/dotnet/.api/api-nettopologysuite.md`): `SwiftBVH`/`SwiftOctree` own the 3D AABB volumetric broad-phase while the NTS `STRtree<TItem>`/`Quadtree<TItem>` own the 2D planar Simple-Features index — the `Model/systems#INTERFERENCE` owner routes element-vs-element clash to the 3D index and footprint/site predicates to the NTS 2D index, neither reimplementing the other's dimension.
- `Smino.Bcf.Toolkit`(`.api/api-smino-bcf-toolkit`): the `ClashProposal` fold consumes the `Query` candidate set, runs the narrow-phase exact test, and authors one `BcfTopic` per confirmed clash through `BcfBuilder.AddMarkup` → `Build` → `Worker.ToBcf` — broad-phase, narrow-phase, and issue exchange meet at the candidate set and the `BimLeaf` term algebra.
- within-lib: `Model/systems#INTERFERENCE` retains TWO structures over one `SwiftBucket` handle space — the `SwiftBVH` tight-volume tree answering hard overlap through `Query` and the `SwiftSpatialHash` answering the clearance modality through `QueryNeighborhood` — and refits both through `UpdateEntryBounds` so a `ModelDiff` `moved` arm re-clashes incrementally against the handles the registry gates.

[LOCAL_ADMISSION]:
- `SwiftCollections.Query` and the handle-stable registry collections are admitted for broad-phase indexing only; the general-purpose `Pool`/`Dimensions`/`Diagnostics` surfaces are not this folder's owners.
- The handle a `SwiftBucket.Add` returns is the index's whole key space: a second registry keyed on a domain id beside it desynchronizes on the first partial refit, so a co-indexed structure pair takes the bucket handle and gates every refit on `IsAllocated`/`TryGetValue`.
- Narrow-phase exact intersection, clash policy, and `BcfTopic` authoring stay COORDINATION concerns; structure kind, entry count, and candidate-pair count are the facts the INTERFERENCE/COORDINATION fold carries on its result.
