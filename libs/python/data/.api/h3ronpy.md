# [PY_DATA_API_H3RONPY]

`h3ronpy` supplies the vectorized H3 discrete-global-grid surface for the data `GRID_DGGS` rail: a Rust-backed function family operating on whole Arrow arrays of `u64` cell indexes — resolution change, traversal, area, parse/format, compaction — with geometry (`h3ronpy.vector`), raster (`h3ronpy.raster`), and polars `.h3` (`h3ronpy.polars`) bridges. `data` composes these into the `GRID_DGGS` index path, cells flowing zero-copy as `arro3.core.Array`/`RecordBatch` through the Arrow/polars pipeline, the Rust `h3o` kernel owning H3 indexing.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `h3ronpy`
- package: `h3ronpy`
- import: `h3ronpy`
- owner: `data`
- rail: GRID_DGGS
- entry points: import-only; no console script
- capability: vectorized H3 cell operations over Arrow `arro3.core` arrays, with geometry, raster, and polars `.h3` bridges feeding the `GRID_DGGS` index

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: containment vocabulary and re-exported Arrow carriers

Single-column results carry `arro3.core.Array`; multi-column results (paired arrays, distances, local-IJ) carry `RecordBatch`. Re-exported `arro3.core` carriers accept any `ArrowArrayExportable`/`ArrowStreamExportable` input and read the schema without importing `arro3.core`.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [CAPABILITY]                                             |
| :-----: | :------------------------- | :---------------- | :------------------------------------------------------- |
|  [01]   | `ContainmentMode`          | enum              | geometry-to-cell coverage policy                         |
|  [02]   | `Array` / `ChunkedArray`   | arro3 carrier     | re-exported zero-copy cell-index column / chunked column |
|  [03]   | `RecordBatch` / `DataType` | arro3 carrier     | re-exported multi-column result / Arrow type descriptor  |
|  [04]   | `polars.H3Expr`            | expression plugin | `.h3` namespace registered on `polars.Expr`              |
|  [05]   | `polars.H3SeriesShortcuts` | series plugin     | `.h3` namespace registered on `polars.Series`            |
|  [06]   | `raster.Transform`         | affine transform  | 6-coefficient raster geotransform `(a, b, c, d, e, f)`   |

`ContainmentMode` defaults to `ContainsCentroid`, with `ContainsBoundary`/`Covers`/`IntersectsBoundary` the alternatives; `H3_CRS = 'EPSG:4326'` fixes the geometry CRS and `DEFAULT_CELL_COLUMN_NAME = 'cell'` the canonical cell column.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cell array operations (`h3ronpy`)

Shared keyword defaults omitted from rows: traversal `flatten=False`, `compact` `mixed_resolutions=False`, parse and local-IJ `set_failing_to_invalid=False` (null on failure keeps array length), validate `booleanarray=False` (boolean mask, not a filtered array).

| [INDEX] | [SURFACE]                                                                | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `change_resolution(arr, resolution)`                                     | move each cell to a target resolution            |
|  [02]   | `change_resolution_list(arr, resolution)`                                | per-cell list of target-resolution children      |
|  [03]   | `change_resolution_paired(arr, resolution) -> RecordBatch`               | source/target cell pairing across resolution     |
|  [04]   | `compact(arr)`                                                           | compact a uniform cell set to coarser parents    |
|  [05]   | `uncompact(arr, target_resolution)`                                      | expand compacted cells to a target resolution    |
|  [06]   | `grid_disk(cellarray, k)`                                                | k-ring disk of neighbors per cell                |
|  [07]   | `grid_disk_distances(cellarray, k) -> RecordBatch`                       | disk cells with their grid distance              |
|  [08]   | `grid_disk_aggregate_k(cellarray, k, aggregation_method) -> RecordBatch` | disk reduced by `min`/`max` k aggregation        |
|  [09]   | `grid_ring_distances(cellarray, k_min, k_max) -> RecordBatch`            | annulus rings `k_min..k_max` with distances      |
|  [10]   | `cells_resolution(arr)`                                                  | resolution of each cell                          |
|  [11]   | `cells_area_km2(cellarray)`                                              | spherical cell area in km2                       |
|  [12]   | `cells_area_m2(cellarray)`                                               | spherical cell area in m2                        |
|  [13]   | `cells_area_rads2(cellarray)`                                            | spherical cell area in radians2                  |
|  [14]   | `cells_valid(arr)`                                                       | validate cell indexes (mask or filtered)         |
|  [15]   | `cells_parse(arr)`                                                       | parse string cells to `u64` indexes              |
|  [16]   | `cells_to_string(cellarray)`                                             | format cells to hex strings                      |
|  [17]   | `cells_to_localij(cellarray, anchor) -> RecordBatch`                     | local-IJ coordinates relative to an anchor       |
|  [18]   | `localij_to_cells(anchor, i, j)`                                         | cells from anchor and local-IJ coordinate arrays |
|  [19]   | `vertexes_valid(arr)`                                                    | validate vertex indexes                          |
|  [20]   | `vertexes_parse(arr)`                                                    | parse vertex strings to indexes                  |
|  [21]   | `vertexes_to_string(vertexesarray)`                                      | format vertexes to hex strings                   |
|  [22]   | `directededges_valid(arr)`                                               | validate directed-edge indexes                   |
|  [23]   | `directededges_parse(arr)`                                               | parse directed-edge strings to indexes           |
|  [24]   | `directededges_to_string(directededgearray)`                             | format directed edges to hex strings             |
|  [25]   | `version() -> str`                                                       | installed h3ronpy version string                 |

[ENTRYPOINT_SCOPE]: geometry bridge (`h3ronpy.vector`)

`geometry_to_cells`/`wkb_to_cells` discriminate coverage by `ContainmentMode` (default `ContainsCentroid`); `radians`, `compact`, `flatten`, `link_cells` default `False`.

| [INDEX] | [SURFACE]                                                           | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------ | :----------------------------------- |
|  [01]   | `geometry_to_cells(geom, resolution, containment_mode, compact)`    | cover one geometry with cells        |
|  [02]   | `wkb_to_cells(arr, resolution, containment_mode, compact, flatten)` | cover a WKB array with cells         |
|  [03]   | `coordinates_to_cells(latarray, lngarray, resarray, radians)`       | cells from lat/lng/resolution arrays |
|  [04]   | `cells_to_coordinates(arr, radians) -> RecordBatch`                 | cell centroids as lat/lng columns    |
|  [05]   | `cells_to_wkb_points(arr, radians)`                                 | cell centroids as WKB points         |
|  [06]   | `cells_to_wkb_polygons(arr, radians, link_cells)`                   | cell boundaries as WKB polygons      |
|  [07]   | `directededges_to_wkb_linestrings(arr, radians)`                    | directed edges as WKB linestrings    |
|  [08]   | `vertexes_to_wkb_points(arr, radians)`                              | vertexes as WKB points               |
|  [09]   | `cells_bounds(arr) -> Tuple \| None`                                | aggregate `(minx, miny, maxx, maxy)` |
|  [10]   | `cells_bounds_arrays(arr) -> RecordBatch`                           | per-cell bounding-box columns        |

[ENTRYPOINT_SCOPE]: raster bridge (`h3ronpy.raster`) and polars namespace (`h3ronpy.polars`)

Raster defaults omitted from rows: `nodata_value=None`, `axis_order='yx'`, `compact=True`, `search_mode='min_diff'`; `rasterize_cells` `size` is an `int` or `(int, int)`.

| [INDEX] | [SURFACE]                                                                               | [CAPABILITY]                                |
| :-----: | :-------------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `raster.raster_to_dataframe(in_raster, transform, h3_resolution, ...) -> pyarrow.Table` | raster pixels to cell/value table           |
|  [02]   | `raster.rasterize_cells(cells, values, size, nodata_value=0) -> (ndarray, transform)`   | cells/values to raster array + transform    |
|  [03]   | `raster.nearest_h3_resolution(shape, transform, ...) -> int`                            | resolution matching a raster cell size      |
|  [04]   | `polars.H3Expr` — `pl.col(name).h3.<op>(...)`                                           | `.h3` expression namespace on `polars.Expr` |
|  [05]   | `polars.H3SeriesShortcuts` — `series.h3.<op>(...)`                                      | `.h3` namespace on `polars.Series`          |

`.h3` mirrors the cell-scalar family only; the geometry and raster bridges and the distance-bearing traversal stay off the accessor — call those module functions on `series.to_arrow()` and re-wrap, the column passing as a zero-copy `arro3` array.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every operation is whole-array over `arro3.core.Array`/`RecordBatch`: cells stay `u64` indexes flowing zero-copy through the Arrow/polars pipeline, never a per-row Python loop over scalar cells.
- One named function owns each concept (`change_resolution`, `grid_disk`, `compact`, `cells_area_km2`); arity and modality ride call arguments (`k`, `resolution`, `flatten`, `mixed_resolutions`, `radians`), never a per-variant function family.
- `geometry_to_cells`/`wkb_to_cells` discriminate coverage by the `ContainmentMode` enum, never a boolean-flag tangle or a per-mode function.
- `set_failing_to_invalid` and `booleanarray` emit null or a boolean mask on parse and validation failure, keeping array length stable; an invalid cell is a data row, never a raised exception.
- `h3o`, the Rust kernel, owns cell indexing and traversal; `h3ronpy.vector` and `h3ronpy.raster` meet the pipeline at the Arrow buffer, never re-decoding WKB or pixels locally.
- Each operation captures resolution, cell count, k-radius, containment mode, area unit, and Arrow schema as a `GRID_DGGS` receipt.

[STACKING]:
- `arro3-core`(`.api/arro3-core.md`): every surface consumes and returns `arro3.core.Array`/`RecordBatch`; cells cross as `u64` columns with no copy and no direct `arro3.core` import.
- `polars`(`.api/polars.md`): the `.h3` namespace registers on `pl.Expr`/`pl.Series` at import; `series.to_arrow()` hands a column to a module function as a zero-copy `arro3` array, re-wrapped without a pandas detour.
- `shapely`(`.api/shapely.md`): `wkb_to_cells` ingests shapely `to_wkb` output and `cells_to_wkb_polygons`/`cells_to_wkb_points` feed `from_wkb`, exchanging geometry at the WKB buffer.
- `rasterio`(`.api/rasterio.md`)/`rioxarray`: `raster.raster_to_dataframe`/`rasterize_cells` move between a numpy raster under an affine `transform` and cells.
- `data` GRID_DGGS owner: `geometry_to_cells`/`wkb_to_cells` ingest, `grid_disk`/`change_resolution`/`compact` traverse and index, `cells_area_*` measure — the whole-array form driven end to end, the polars `.h3` column staying zero-copy through the pipeline.

[LOCAL_ADMISSION]:
- h3ronpy is the sole H3 discrete-global-grid surface for `data`; its Arrow-array functions admit for cell indexing, traversal, measurement, and geometry/raster bridging feeding the `GRID_DGGS` index.

[RAIL_LAW]:
- Package: `h3ronpy`
- Owns: vectorized H3 cell operations over Arrow — resolution change, grid-disk/ring traversal, spherical area, parse/validate/stringify, local-IJ transforms, geometry/raster bridging, the polars `.h3` namespace
- Accept: whole-array cell indexing and traversal feeding the `GRID_DGGS` index over the Arrow/polars pipeline
- Reject: per-row scalar loops where the array functions vectorize; a hand-rolled H3 codec or traversal kernel the Rust `h3o` backend owns; a pandas detour when the polars `.h3` namespace owns the column; a wrapper-rename of a cell op; a per-resolution or per-mode function family; CRS or identity the runtime fixes
