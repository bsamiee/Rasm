# [PY_GEOMETRY_API_RTREE]

`rtree` supplies the libspatialindex R-tree bounding-box index for the geometry `mesh/spatial` Bounds worker-lane arm — a polymorphic `Index` over disk or memory storage, a `Property` tuning surface, and window queries with numpy-vectorized batch counterparts. `geometry` composes `Index(properties=Property(dimension=3))`, the stream-load constructor, and the `objects='raw'` payload query, never re-implementing the R-tree split heuristics, MBR packing, or k-nearest traversal. This GATED ENRICHMENT manifest row owns the persistent large-N index the `trimesh`+`numpy` in-triangulation spine cannot reach at repeated-query scale over a static element set; that spine's own AABB proximity stays for one-shot queries and never depends on it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rtree`
- package: `rtree`
- import: `from rtree import index`
- owner: `geometry`
- rail: mesh/spatial / bounds-index-enrichment
- license: `MIT` (own) over libspatialindex `MIT`
- entry points: none (library only)
- capability: multi-dimensional R-tree/R*-tree/MVR-tree/TPR-tree index over disk or memory storage, generator-stream bulk load, intersection/nearest/count/containment window queries, numpy-vectorized batch intersection and nearest, and object-payload storage with pickle or live-object container variants

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: index, container, and result-item family
- rail: mesh/spatial

`Index` (aliased `Rtree`) is the query carrier; `RtreeContainer` is the sibling variant storing live Python objects instead of pickled payloads; `Item` is the hydrated query entry returned under `objects=True`.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                                                              |
| :-----: | :--------------- | :------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `Index`          | index carrier | insert/delete, `intersection`/`nearest`/`count`/`contains`; disk/memory; stream bulk load |
|  [02]   | `RtreeContainer` | object index  | same query surface keyed by live Python objects, no pickling of the payload               |
|  [03]   | `Item`           | result entry  | `id`, `bbox`, `bounds`, `object`, `get_object()` for a hydrated hit (`objects=True`)      |

[PUBLIC_TYPE_SCOPE]: property and enum family
- rail: mesh/spatial

`Property` carries the index configuration; the `RT_*` module constants seed its `type`/`variant`/`storage` slots. `RTreeError` is the single failure type.

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :--------------------------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `Property`                               | config        | slots fenced below; `as_dict`/`initialize_from_dict` round-trip         |
|  [02]   | `RT_RTree` / `RT_MVRTree` / `RT_TPRTree` | index type    | seeds `Property.type`: static, multi-version, or time-parameterized     |
|  [03]   | `RT_Linear` / `RT_Quadratic` / `RT_Star` | split variant | seeds `Property.variant`: linear/quadratic/R* split (default `RT_Star`) |
|  [04]   | `RT_Memory` / `RT_Disk`                  | storage       | seeds `Property.storage`: in-memory or file-backed pages                |
|  [05]   | `RTreeError`                             | failure       | libspatialindex fault surfaced as a Python exception                    |

```python signature
# rtree.index.Property config slots (settable attributes):
dimension; type; variant; storage
leaf_capacity; index_capacity; fill_factor; near_minimum_overlap_factor; tight_mbr
pagesize; buffering_capacity; overwrite
filename; dat_extension; idx_extension; tpr_horizon
```

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction (`index.Index`)
- rail: mesh/spatial

`Index` is one polymorphic constructor discriminated by the first positional argument: a `str`/path opens or creates disk storage, a generator triggers packed bulk load, an absent first argument yields a memory index. `properties=Property(...)` sets dimension and variant; `interleaved=True` fixes the coordinate order for every query.

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [CAPABILITY]                                             |
| :-----: | :-------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `Index(properties=Property(dimension=3))`           | memory         | in-memory index over the given dimension                 |
|  [02]   | `Index(filename, properties=..., interleaved=True)` | disk           | open or create a file-backed index                       |
|  [03]   | `Index(stream, properties=...)`                     | bulk load      | packed construction from a `(id, coords, obj)` generator |

[ENTRYPOINT_SCOPE]: mutation and query
- rail: mesh/spatial

`insert`/`add` and `delete` carry the interleaved `(min, max)` coordinate tuple; every query row accepts `objects=False` (ids), `objects=True` (`Item`), or `objects='raw'` (the stored payload). `count` returns the hit tally without materializing ids.

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `insert(id, coordinates, obj=None)` / `add(...)`     | mutate         | index one bounding box with an optional payload |
|  [02]   | `delete(id, coordinates)`                            | mutate         | remove the entry matching id and box            |
|  [03]   | `intersection(coordinates, objects=False)`           | query          | ids/items/payloads overlapping the query window |
|  [04]   | `nearest(coordinates, num_results=1, objects=False)` | query          | k-nearest entries to the query window           |
|  [05]   | `count(coordinates)`                                 | query          | intersecting-entry tally                        |
|  [06]   | `contains(coordinates, objects=False)`               | query          | entries fully contained by the query window     |

[ENTRYPOINT_SCOPE]: vectorized batch, persistence, and payload
- rail: mesh/spatial

`intersection_v`/`nearest_v` take numpy `(n, d)` min/max arrays and return numpy id-and-count arrays for a whole batch in one native call, `nearest_v` carrying an optional per-query distance cutoff. `dumps`/`loads` are the pickle payload codec; `flush`/`close` commit and release disk storage; `bounds`/`get_bounds`/`leaves`/`get_size`/`valid` introspect the tree.

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [CAPABILITY]                                     |
| :-----: | :--------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `intersection_v(mins, maxs)`                         | batch          | ids plus per-box counts for a numpy query batch  |
|  [02]   | `nearest_v(mins, maxs, ...)`                         | batch          | batched k-nearest with optional distance cutoff  |
|  [03]   | `bounds` / `get_bounds(coordinate_interleaved=None)` | introspect     | overall index MBR in the chosen coordinate order |
|  [04]   | `leaves()` / `get_size()` / `valid()`                | introspect     | leaf-node layout, entry count, and validity      |
|  [05]   | `flush()` / `close()`                                | persist        | commit dirty pages and release the disk index    |
|  [06]   | `dumps(obj) -> bytes` / `loads(bytes) -> object`     | payload        | pickle codec for the stored object payload       |

## [04]-[IMPLEMENTATION_LAW]

[MESH_SPATIAL_BOUNDS]:
- import: `from rtree import index` at boundary scope only; module-level import is banned by the manifest import policy.
- construction axis: `Index` discriminates on the first argument — path (disk), generator stream (packed bulk load), or absent (memory); a per-storage or per-load function family is the deleted form.
- coordinate axis: `interleaved=True` fixes queries to `(xmin, ymin, zmin, xmax, ymax, zmax)`; the Bounds arm builds a 3-D index via `Property(dimension=3)` over element AABBs and windows candidate elements before the exact `trimesh` proximity test refines them.
- payload axis: `objects='raw'` returns the stored element handle with zero pickle round-trip; `RtreeContainer` is the variant for a live Python object payload, and `dumps`/`loads` back the persistent path.
- bulk axis: the `(id, coords, obj)` generator constructor packs the tree in one pass — the required path for a static element set, since repeated `insert` rebalances incrementally and loses the packed-tree query locality.
- batch axis: `intersection_v`/`nearest_v` fold a whole query batch into one native call over numpy `(n, d)` arrays, the vectorized path the Bounds arm uses for multi-element clearance candidate generation.
- evidence: each build captures the entry count and the overall `bounds` MBR; a query receipt keys the candidate count the exact refinement narrows and the R-tree pre-filter ratio the spatial owner folds against the caller ceiling.
- boundary: rtree owns the persistent bounding-box index and candidate pre-filter; exact nearest-surface distance, ray, and containment stay `trimesh` proximity, narrow-phase collision and signed separation stay `python-fcl`, and fine point-cloud registration stays `small-gicp`/`open3d`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `rtree`
- Owns: multi-dimensional R-tree/R*-tree/TPR-tree bounding-box index, disk or memory storage, generator bulk load, intersection/nearest/count/containment queries, numpy-vectorized batch queries, and object-payload storage
- Accept: the `mesh/spatial` Bounds arm's persistent large-N AABB index and candidate pre-filter feeding the exact `trimesh` refinement
- Reject: wrapper-renames of `Index`/`intersection`/`nearest`; a hand-rolled AABB tree or R-tree split where rtree is admitted; a per-storage or per-query-kind function family over the polymorphic constructor and the `objects` discriminant; incremental `insert` where a packed stream-load fits a static set
