# [RASM_BIM_API_SWIFTCOLLECTIONS]

`SwiftCollections.Lean` owns the 3D AABB broad-phase behind the `Model/systems#INTERFERENCE` clash-candidate build and the `Review/coordination#COORDINATION` `ClashProposal` fold. `SwiftBVH`, `SwiftOctree`, and `SwiftSpatialHash` implement one generic `IBoundVolume<TVolume>` contract, so the clash engine binds the contract once and the concrete structure is a tuning choice; the handle-stable `SwiftBucket`/`SwiftSparseMap` collections hold the element-id↔volume registry with zero per-element allocation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SwiftCollections.Lean`
- package: `SwiftCollections.Lean` (MIT)
- assembly: `SwiftCollections`
- namespace: `SwiftCollections.Query`, `SwiftCollections`
- asset: multi-target net8.0 + netstandard2.1; the net10.0 consumer binds `lib/net8.0` (AnyCPU, no `runtimes/` folder), the MemoryPack-free `.Lean` variant carrying `Chronicler.Core.Lean` as its zero-dep diagnostic core
- rail: clash

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: spatial broad-phase structures

Each of `SwiftBVH`/`SwiftOctree`/`SwiftSpatialHash` ships a built-in `<TKey>` form over `BoundVolume` and a `<TKey, TVolume>` form over a custom `IBoundVolume<TVolume>` with its partition strategy.

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :---------------------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `SwiftBVH`                          | class         | refittable BVH; SAH-cost insertion, `NodePool`/`RootNode` default phase |
|  [02]   | `SwiftOctree`                       | class         | depth/node-capacity bounded subdivision with merge-on-remove            |
|  [03]   | `SwiftSpatialHash`                  | class         | uniform-grid hash; `QueryNeighborhood` over the padded cell ring        |
|  [04]   | `BoundVolume`                       | struct        | `Vector3` AABB: `Union`/`Intersects`/`GetCost`/`BoundsEquals`           |
|  [05]   | `IBoundVolume<TVolume>`             | interface     | generic AABB contract plugging all three structures                     |
|  [06]   | `SwiftOctreeOptions`                | struct        | `MaxDepth`/`NodeCapacity`/`EnableMergeOnRemove` tuning                  |
|  [07]   | `SwiftSpatialHashOptions`           | struct        | `NeighborhoodPadding`, static `.Default` — the neighborhood ring        |
|  [08]   | `IOctreeBoundsPartitioner<TVolume>` | interface     | pluggable octree bounds-partition strategy                              |
|  [09]   | `ISpatialHashCellMapper<TVolume>`   | interface     | pluggable spatial-hash cell-mapping strategy                            |

[PUBLIC_TYPE_SCOPE]: handle-stable backing collections

Each backing collection holds the `BimElement` GlobalId→AABB mapping under stable integer handles, so a model update mutates one slot instead of rebuilding the index.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :--------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `SwiftBucket<T>`       | class         | stable-handle dense slab: `Add`/`TryRemoveAt`/`TryGetValue`, `PeakCount` |
|  [02]   | `SwiftSparseMap<T>`    | class         | sparse int-keyed map; O(1) add/remove/lookup — the GlobalId→volume map   |
|  [03]   | `SwiftSparseSet`       | class         | sparse int set for the candidate-pair dedupe                             |
|  [04]   | `SwiftList<T>`         | class         | low-overhead ordered `Query` sink; `IStateBacked<SwiftArrayState<T>>`    |
|  [05]   | `SwiftHashSet<T>`      | class         | deduped `Query` sink; `IStateBacked<SwiftArrayState<T>>`                 |
|  [06]   | `IStateBacked<TState>` | interface     | `State` snapshot property + state ctor; spatial structures omit it       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: broad-phase build and query

`TKey` is the element handle and `TVolume` the AABB; the design binds this shared surface, not a concrete structure.

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :--------------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `new SwiftBVH(int)`                                                          | ctor     | pre-sized BVH node pool                  |
|  [02]   | `new SwiftOctree(TVolume, SwiftOctreeOptions, IOctreeBoundsPartitioner)`     | ctor     | bounded subdivision, pluggable partition |
|  [03]   | `new SwiftSpatialHash(int, ISpatialHashCellMapper, SwiftSpatialHashOptions)` | ctor     | uniform grid, pluggable cell mapper      |
|  [04]   | `Insert(TKey, TVolume) -> bool`                                              | instance | index an element AABB                    |
|  [05]   | `UpdateEntryBounds(TKey, TVolume)`                                           | instance | in-place refit of a moved element        |
|  [06]   | `Query(TVolume, ICollection<TKey>)`                                          | instance | sink of overlapping entries — candidates |
|  [07]   | `SwiftSpatialHash.QueryNeighborhood(TVolume, ICollection<TKey>)`             | instance | widen the query by the padded cell ring  |
|  [08]   | `Remove(TKey) -> bool`                                                       | instance | drop an element from the index           |
|  [09]   | `TryGetBounds(TKey, out TVolume) -> bool`                                    | instance | read back a stored AABB (octree/hash)    |
|  [10]   | `Contains(TKey) -> bool` / `Count`                                           | instance | membership and entry count               |
|  [11]   | `EnsureCapacity(int)` / `Clear()`                                            | instance | pre-grow the node pool / reset           |

[ENTRYPOINT_SCOPE]: BoundVolume — AABB algebra

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :---------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `new BoundVolume(Vector3, Vector3)`             | ctor     | AABB from a geometry bounding box           |
|  [02]   | `BoundVolume.Union(BoundVolume) -> BoundVolume` | instance | merged AABB — the BVH internal-node bound   |
|  [03]   | `BoundVolume.Intersects(BoundVolume) -> bool`   | instance | AABB overlap — the broad-phase predicate    |
|  [04]   | `BoundVolume.GetCost(BoundVolume) -> long`      | instance | SAH surface-area cost driving BVH insertion |
|  [05]   | `BoundVolume.Center` / `Size` / `Volume`        | property | derived AABB metrics                        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every structure binds one `IBoundVolume<TVolume>` contract (`Union`/`Intersects`/`GetCost`/`BoundsEquals`), so the clash engine folds through a single code path and the concrete structure and volume type are tuning choices, never forked implementations.

[STACKING]:
- `NetTopologySuite`(`.api/api-nettopologysuite`): `SwiftBVH`/`SwiftOctree` own the 3D AABB volumetric broad-phase while the NTS `STRtree<TItem>`/`Quadtree<TItem>` own the 2D planar Simple-Features index — the COORDINATION owner routes element-vs-element clash to the 3D index and footprint/site predicates to the NTS 2D index, neither reimplementing the other's dimension.
- `Smino.Bcf.Toolkit`(`.api/api-smino-bcf-toolkit`): the `ClashProposal` fold consumes the `Query` candidate set, runs the narrow-phase exact test, and authors one `BcfTopic` per confirmed clash through `BcfBuilder.AddMarkup` → `Build` → `Worker.ToBcf` — broad-phase, narrow-phase, and issue exchange meet at the candidate set and the `ElementPredicate` algebra.
- within-lib: the INTERFERENCE engine binds the shared `Insert`/`UpdateEntryBounds`/`Query` surface, so `SwiftBVH`, `SwiftOctree`, and `SwiftSpatialHash` are one code path selected by a `CoordinationRule` row; `UpdateEntryBounds` refits a single moved element so a `ModelDiff` `moved` arm re-runs the clash incrementally, and the `IStateBacked` registry (`SwiftSparseMap`/`SwiftBucket`) snapshots the GlobalId→`BoundVolume` map for a replayable `ClashProposal` receipt.

[LOCAL_ADMISSION]:
- `SwiftCollections.Query` and the handle-stable registry collections are admitted for broad-phase indexing only; the general-purpose `Pool`/`Dimensions`/`Diagnostics` surfaces are not this folder's owners.
- Narrow-phase exact intersection, clash policy, and `BcfTopic` authoring stay COORDINATION concerns; structure kind, entry count, and candidate-pair count are the receipt facts the INTERFERENCE/COORDINATION fold records.

[RAIL_LAW]:
- Package: `SwiftCollections.Lean`
- Owns: 3D AABB broad-phase (BVH/octree/spatial-hash) candidate generation
- Accept: the INTERFERENCE clash-candidate build and the COORDINATION clash fold's broad phase
- Reject: exact narrow-phase intersection, clash policy, the 2D planar Simple-Features index, the BCF issue model
