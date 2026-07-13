# [PY_DATA_API_H3RONPY]

`h3ronpy` supplies the vectorized H3 discrete-global-grid surface for the data `GRID_DGGS` rail: a Rust-backed function family that operates on whole Arrow arrays of `u64` cell indexes — resolution change, grid-disk traversal, area measurement, parse/format, and compaction — plus a geometry bridge (`h3ronpy.vector`) that converts WKB/coordinate arrays to and from cells, a raster bridge (`h3ronpy.raster`) over numpy/affine transforms, and a polars `.h3` expression namespace. The package owner composes these array operations into the `GRID_DGGS` index path; it consumes and returns zero-copy `arro3.core.Array`/`RecordBatch` so cells flow through the Arrow/polars pipeline without per-row Python, and it never re-implements the H3 indexing kernel the underlying Rust `h3o` crate already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `h3ronpy`
- package: `h3ronpy`
- import: `h3ronpy`
- owner: `data`
- rail: GRID_DGGS
- version: `0.22.0`
- entry points: library use is import-only; no console script
- capability: vectorized H3 cell operations over Arrow arrays — resolution change/compaction/uncompaction, grid-disk and grid-ring traversal with distances and aggregation, spherical area in km2/m2/rads2, cell/vertex/directed-edge parse-validate-stringify, local-IJ coordinate transforms, geometry-to-cells and cells-to-WKB conversion (`h3ronpy.vector`), raster-to-cells and cells-to-raster bridging (`h3ronpy.raster`), and a polars `.h3` expression/series namespace (`h3ronpy.polars`, registered on import when `polars` is present), all zero-copy over `arro3.core` Arrow buffers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: containment vocabulary and namespace roots
- rail: GRID_DGGS

`ContainmentMode` is the Rust-backed enum selecting how a geometry covers cells during `geometry_to_cells`/`wkb_to_cells`. `H3Expr` and `H3SeriesShortcuts` register the `.h3` namespace on `polars.Expr` and `polars.Series`. `h3ronpy` re-exports the `arro3.core` carrier types (`Array`, `ChunkedArray`, `RecordBatch`, `DataType`) so callers accept any `ArrowArrayExportable`/`ArrowStreamExportable` input and read the schema without importing `arro3.core` directly. Function returns carry `arro3.core.Array` for one column and `arro3.core.RecordBatch` for multi-column results (paired arrays, distances, local-IJ).

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [RAIL]                                                   |
| :-----: | :------------------------- | :---------------- | :------------------------------------------------------- |
|  [01]   | `ContainmentMode`          | enum              | geometry-to-cell coverage policy (4 members)             |
|  [02]   | `Array` / `ChunkedArray`   | arro3 carrier     | re-exported zero-copy cell-index column / chunked column |
|  [03]   | `RecordBatch` / `DataType` | arro3 carrier     | re-exported multi-column result / Arrow type descriptor  |
|  [04]   | `polars.H3Expr`            | expression plugin | `.h3` namespace registered on `polars.Expr`              |
|  [05]   | `polars.H3SeriesShortcuts` | series plugin     | `.h3` namespace registered on `polars.Series`            |
|  [06]   | `raster.Transform`         | affine transform  | 6-coefficient raster geotransform `(a, b, c, d, e, f)`   |

`ContainmentMode` members: `ContainsCentroid` (default), `ContainsBoundary`, `Covers`, `IntersectsBoundary`. The cell-index columns are `arro3.core.Array` of `u64`; module-level constants `H3_CRS = 'EPSG:4326'` and `DEFAULT_CELL_COLUMN_NAME = 'cell'` fix the geometry CRS and the canonical cell-column name.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cell array operations (`h3ronpy`)
- rail: GRID_DGGS

Every surface takes an Arrow cell array (`u64` indexes) and returns `arro3.core.Array`; the multi-column surfaces `change_resolution_paired`, `grid_disk_distances`, `grid_disk_aggregate_k`, `grid_ring_distances`, and `cells_to_localij` return a `RecordBatch` instead. The `[ARGS]` column carries each surface's argument list. Shared keyword defaults, omitted from the rows: traversal takes `flatten=False`, `compact` takes `mixed_resolutions=False`, parse/local-IJ take `set_failing_to_invalid=False` (null on failure, keeping array length), and validate takes `booleanarray=False` (a boolean mask instead of a filtered array).

| [INDEX] | [SURFACE]                  | [ARGS]                               | [CAPABILITY]                                     |
| :-----: | :------------------------- | :----------------------------------- | :----------------------------------------------- |
|  [01]   | `change_resolution`        | `(arr, resolution)`                  | move each cell to a target resolution            |
|  [02]   | `change_resolution_list`   | `(arr, resolution)`                  | per-cell list of target-resolution children      |
|  [03]   | `change_resolution_paired` | `(arr, resolution)`                  | source/target cell pairing across resolution     |
|  [04]   | `compact`                  | `(arr)`                              | compact a uniform cell set to coarser parents    |
|  [05]   | `uncompact`                | `(arr, target_resolution)`           | expand compacted cells to a target resolution    |
|  [06]   | `grid_disk`                | `(cellarray, k)`                     | k-ring disk of neighbors per cell                |
|  [07]   | `grid_disk_distances`      | `(cellarray, k)`                     | disk cells with their grid distance              |
|  [08]   | `grid_disk_aggregate_k`    | `(cellarray, k, aggregation_method)` | disk reduced by `min`/`max` k aggregation        |
|  [09]   | `grid_ring_distances`      | `(cellarray, k_min, k_max)`          | annulus rings `k_min..k_max` with distances      |
|  [10]   | `cells_resolution`         | `(arr)`                              | resolution of each cell                          |
|  [11]   | `cells_area_km2`           | `(cellarray)`                        | spherical cell area in km2                       |
|  [12]   | `cells_area_m2`            | `(cellarray)`                        | spherical cell area in m2                        |
|  [13]   | `cells_area_rads2`         | `(cellarray)`                        | spherical cell area in radians2                  |
|  [14]   | `cells_valid`              | `(arr)`                              | validate cell indexes (mask or filtered)         |
|  [15]   | `cells_parse`              | `(arr)`                              | parse string cells to `u64` indexes              |
|  [16]   | `cells_to_string`          | `(cellarray)`                        | format cells to hex strings                      |
|  [17]   | `cells_to_localij`         | `(cellarray, anchor)`                | local-IJ coordinates relative to an anchor       |
|  [18]   | `localij_to_cells`         | `(anchor, i, j)`                     | cells from anchor and local-IJ coordinate arrays |
|  [19]   | `vertexes_valid`           | `(arr)`                              | validate vertex indexes                          |
|  [20]   | `vertexes_parse`           | `(arr)`                              | parse vertex strings to indexes                  |
|  [21]   | `vertexes_to_string`       | `(vertexesarray)`                    | format vertexes to hex strings                   |
|  [22]   | `directededges_valid`      | `(arr)`                              | validate directed-edge indexes                   |
|  [23]   | `directededges_parse`      | `(arr)`                              | parse directed-edge strings to indexes           |
|  [24]   | `directededges_to_string`  | `(directededgearray)`                | format directed edges to hex strings             |
|  [25]   | `version`                  | `() -> str`                          | installed h3ronpy version string                 |

[ENTRYPOINT_SCOPE]: geometry bridge (`h3ronpy.vector`)
- rail: GRID_DGGS

`geometry_to_cells`/`wkb_to_cells` discriminate coverage by `ContainmentMode` (default `ContainmentMode.ContainsCentroid`); `radians`, `compact`, `flatten`, and `link_cells` are `bool` flags defaulting `False`. The `[ARGS]` column carries each surface's argument list; returns are `arro3.core.Array` unless `-> RecordBatch` or `-> Tuple` is noted.

| [INDEX] | [SURFACE]                          | [ARGS]                                                  | [CAPABILITY]                         |
| :-----: | :--------------------------------- | :------------------------------------------------------ | :----------------------------------- |
|  [01]   | `geometry_to_cells`                | `(geom, resolution, containment_mode, compact)`         | cover one geometry with cells        |
|  [02]   | `wkb_to_cells`                     | `(arr, resolution, containment_mode, compact, flatten)` | cover a WKB array with cells         |
|  [03]   | `coordinates_to_cells`             | `(latarray, lngarray, resarray, radians)`               | cells from lat/lng/resolution arrays |
|  [04]   | `cells_to_coordinates`             | `(arr, radians) -> RecordBatch`                         | cell centroids as lat/lng columns    |
|  [05]   | `cells_to_wkb_points`              | `(arr, radians)`                                        | cell centroids as WKB points         |
|  [06]   | `cells_to_wkb_polygons`            | `(arr, radians, link_cells)`                            | cell boundaries as WKB polygons      |
|  [07]   | `directededges_to_wkb_linestrings` | `(arr, radians)`                                        | directed edges as WKB linestrings    |
|  [08]   | `vertexes_to_wkb_points`           | `(arr, radians)`                                        | vertexes as WKB points               |
|  [09]   | `cells_bounds`                     | `(arr) -> Tuple \| None`                                | aggregate `(minx, miny, maxx, maxy)` |
|  [10]   | `cells_bounds_arrays`              | `(arr) -> RecordBatch`                                  | per-cell bounding-box columns        |

[ENTRYPOINT_SCOPE]: raster bridge (`h3ronpy.raster`) and polars namespace (`h3ronpy.polars`)
- rail: GRID_DGGS

The raster bridge moves between numpy arrays under an affine `transform` and cells; the polars surface registers a `.h3` namespace on `Expr`/`Series`. The `[ARGS]` column carries each function's argument list; the polars rows carry the accessor pattern. Raster defaults, omitted from the rows: `nodata_value=None`, `axis_order='yx'`, `compact=True`, `search_mode='min_diff'`; `rasterize_cells`'s `size` is an `int` or `(int, int)`. Returns: `raster_to_dataframe -> pyarrow.Table`, `rasterize_cells -> (ndarray, transform tuple)`, `nearest_h3_resolution -> int`.

| [INDEX] | [SURFACE]                      | [ARGS]                                       | [CAPABILITY]                                |
| :-----: | :----------------------------- | :------------------------------------------- | :------------------------------------------ |
|  [01]   | `raster.raster_to_dataframe`   | `(in_raster, transform, h3_resolution, ...)` | raster pixels to cell/value table           |
|  [02]   | `raster.rasterize_cells`       | `(cells, values, size, nodata_value=0)`      | cells/values to raster array + transform    |
|  [03]   | `raster.nearest_h3_resolution` | `(shape, transform, ...)`                    | resolution matching a raster cell size      |
|  [04]   | `raster.cells_bounds`          | `(arr)`                                      | aggregate cell bound                        |
|  [05]   | `raster.cells_to_wkb_polygons` | `(arr, radians, link_cells)`                 | cell boundaries as WKB polygons             |
|  [06]   | `polars.H3Expr`                | `pl.col(name).h3.<op>(...)`                  | `.h3` expression namespace on `polars.Expr` |
|  [07]   | `polars.H3SeriesShortcuts`     | `series.h3.<op>(...)`                        | `.h3` namespace on `polars.Series`          |

The `.h3` namespace mirrors the cell-scalar operation family only — `change_resolution`, `change_resolution_list`, `compact`, `uncompact`, `grid_disk`, `cells_resolution`, `cells_area_km2`/`_m2`/`_rads2`, `cells_valid`, `cells_parse`, `cells_to_string`, and the `vertexes_*`/`directededges_*` validate/parse/stringify trio. The geometry bridge (`geometry_to_cells`/`wkb_to_cells`/`cells_to_coordinates`), the raster bridge, and the distance-bearing traversal (`grid_disk_distances`/`grid_ring_distances`/`grid_disk_aggregate_k`/`cells_to_localij`) are NOT on the `.h3` accessor; call those module functions on the underlying Arrow array (`series.to_arrow()`) and re-wrap, since polars passes the column through `h3ronpy` as an `arro3` array with no copy.

## [04]-[IMPLEMENTATION_LAW]

[GRID_DGGS_H3RONPY]:
- import: `import h3ronpy` (and `h3ronpy.vector`/`h3ronpy.raster`/`h3ronpy.polars`) at boundary scope only; module-level import is banned by the manifest import policy.
- array axis: every operation is whole-array over `arro3.core.Array`/`RecordBatch`; cells stay `u64` indexes flowing zero-copy through the Arrow/polars pipeline — never a per-row Python loop over scalar cells, never a pandas detour when the polars `.h3` namespace owns the column.
- operation axis: one named function owns each concept (`change_resolution`, `grid_disk`, `cells_area_km2`, `compact`); arity and modality live in call rows (`k`, `resolution`, `flatten`, `mixed_resolutions`, `radians`), never a parallel per-variant function or a `get`/`getMany` family.
- containment axis: `geometry_to_cells`/`wkb_to_cells` discriminate coverage by `ContainmentMode` (`ContainsCentroid`/`ContainsBoundary`/`Covers`/`IntersectsBoundary`) as an enum row, never a boolean-flag tangle or a per-mode function.
- failure axis: `set_failing_to_invalid` and `booleanarray` keep array length stable by emitting null or a boolean mask on parse/validation failure; invalid cells are a data row, never a raised exception in the array pipeline.
- bridge axis: `h3ronpy.vector` owns geometry/coordinate to-and-from cells over WKB and lat/lng arrays; `h3ronpy.raster` owns numpy/affine-transform to-and-from cells; both meet the rest of the pipeline at the Arrow buffer, never re-decoding WKB or pixels in local code.
- evidence: each operation captures resolution, cell count, k-radius, containment mode, area unit, and Arrow schema as a `GRID_DGGS` receipt.
- boundary: h3ronpy owns vectorized H3 cell indexing, traversal, measurement, and geometry/raster bridging over Arrow; the underlying H3 kernel routes through its Rust `h3o` backend, never a hand-rolled cell-index codec; geometry CRS is fixed to `H3_CRS = 'EPSG:4326'`; the default cell column is `DEFAULT_CELL_COLUMN_NAME = 'cell'`.

[RAIL_LAW]:
- Package: `h3ronpy`
- Owns: vectorized H3 cell operations over Arrow arrays — resolution change/compaction, grid-disk/ring traversal, spherical area, parse-validate-stringify, local-IJ transforms, geometry/coordinate bridging, raster bridging, and a polars `.h3` namespace
- Accept: whole-array H3 cell indexing and traversal feeding the `GRID_DGGS` index over the Arrow/polars pipeline
- Reject: wrapper-renames of `grid_disk`/`change_resolution`/`compact`; a hand-rolled H3 cell-index codec or grid-traversal kernel; per-row scalar cell loops where the array functions vectorize; a pandas detour when the polars `.h3` namespace owns the column; a parallel per-resolution or per-mode function family; identity/CRS minting the runtime owns
