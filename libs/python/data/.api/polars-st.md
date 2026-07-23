# [PY_DATA_API_POLARS_ST]

`polars-st` mints the GeoArrow spatial extension for the `data` spatial rail: a `.st` accessor on `polars.Expr`/`Series`/`DataFrame`/`LazyFrame` running GEOS-backed geometry ops as registered plugin expressions over a WKB column, with parsing factories, OGR/GeoPandas IO, and the `GeoExprNameSpace` predicate/measure/overlay/transform vocabulary. Every op folds into the same `LazyFrame` graph as ordinary Polars work and inherits pushdown; GEOS overlay, buffering, and PROJ reprojection stay bound to the extension.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `polars-st`
- package: `polars-st` (`LGPL-2.1`, dynamic-linkage native extension)
- module: `polars_st`
- owner: `data`
- rail: spatial
- asset: GEOS/GDAL-backed native extension registered as a Polars expression plugin over a WKB geometry column, the LGPL obligation held at the shared-library boundary and safe for a host-distributed plugin
- capability: GeoArrow/WKB geometry columns with GEOS-backed predicates, measures, overlay, constructive, affine/topology, linear-referencing, SRID projection, WKB/WKT/EWKT/GeoJSON/shapely serialization, OGR/GeoPandas IO, and spatial joins as vectorized `.st` expression rows

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geo frame/expr roots, accessor namespaces, and geometry vocabulary

`GeoExpr`/`GeoSeries`/`GeoDataFrame`/`GeoLazyFrame` subclass the matching Polars type and expose `.st`, returning the parallel `*NameSpace` that carries the geometry ops; `st(d)` discriminates the namespace by container shape. `GeometryType`/`DimensionType`/`CoordinateType` are `Literal` aliases over the OGC geometry, dimension, and coordinate vocabularies, and the `Polars*` siblings are `pl.Enum` projections of those aliases.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                                          |
| :-----: | :---------------------- | :-------------- | :---------------------------------------------------- |
|  [01]   | `GeoExpr`               | geo container   | `polars.Expr` with the `.st` geometry accessor        |
|  [02]   | `GeoSeries`             | geo container   | `polars.Series` of WKB geometry with `.st`            |
|  [03]   | `GeoDataFrame`          | geo container   | `polars.DataFrame` with a geometry column and `.st`   |
|  [04]   | `GeoLazyFrame`          | geo container   | `polars.LazyFrame` with a geometry column and `.st`   |
|  [05]   | `GeoExprNameSpace`      | accessor        | geometry operations over an `Expr`                    |
|  [06]   | `GeoSeriesNameSpace`    | accessor        | geometry operations over a `Series`                   |
|  [07]   | `GeoDataFrameNameSpace` | accessor        | geometry ops, `sjoin`, and IO over a `DataFrame`      |
|  [08]   | `GeoLazyFrameNameSpace` | accessor        | geometry ops and `sjoin` over a `LazyFrame`           |
|  [09]   | `GeometryType`          | literal alias   | OGC geometry kinds (`Point`, `LineString`, `Polygon`) |
|  [10]   | `DimensionType`         | literal alias   | topological dimension (`Point`, `Curve`, `Surface`)   |
|  [11]   | `CoordinateType`        | literal alias   | coordinate axes (`XY`, `XYZ`, `XYZM`, `XYM`)          |
|  [12]   | `PolarsGeometryType`    | enum projection | `pl.Enum` over `GeometryType`                         |
|  [13]   | `PolarsDimensionType`   | enum projection | `pl.Enum` over `DimensionType`                        |
|  [14]   | `PolarsCoordinateType`  | enum projection | `pl.Enum` over `CoordinateType`                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: geometry construction, parsing, and IO factories

Coordinate builders lift raw columns and text/bytes parsers lift encoded payloads into a `GeoExpr`, sharing the `srid` policy; `read_file`/`from_geopandas` materialize a `GeoDataFrame` from OGR sources or GeoPandas objects.

[BUILDERS] `(coords, srid=0)`, `rectangle(bounds, srid=0)`: `point` `multipoint` `linestring` `circularstring` `multilinestring` `polygon` `rectangle`
[PARSERS] `(expr) -> GeoExpr`: `from_wkb` `from_wkt` `from_ewkt` `from_geojson` `from_shapely`

| [INDEX] | [SURFACE]                            | [CAPABILITY]                                  |
| :-----: | :----------------------------------- | :-------------------------------------------- |
|  [01]   | `st(d)`                              | shape-discriminating geometry accessor        |
|  [02]   | `geom(name='geometry', *more_names)` | select geometry column(s) as a `GeoExpr`      |
|  [03]   | `element()`                          | geometry-typed alias for `polars.element`     |
|  [04]   | `read_file(path, /, ...)`            | read an OGR data source into a `GeoDataFrame` |
|  [05]   | `from_geopandas(data, *, ...)`       | lift a GeoPandas `GeoDataFrame`/`GeoSeries`   |

- `read_file`: `(path_or_buffer, /, layer, encoding, columns, read_geometry, force_2d, skip_features, max_features, where, bbox, fids, sql, sql_dialect, return_fids) -> GeoDataFrame`
- `from_geopandas`: `(data, *, schema_overrides, rechunk, nan_to_null, include_index) -> GeoDataFrame | GeoSeries`

[ENTRYPOINT_SCOPE]: `GeoDataFrame`/`GeoSeries` construction

`GeoSeries`/`GeoDataFrame` subclass the Polars container; `geometry_format` selects the input encoding (encoded text/bytes or raw coordinates per geometry kind) and `geometry_name` names the geometry column.

| [INDEX] | [SURFACE]                                                                 | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------ | :-------------------------------- |
|  [01]   | `GeoSeries(name, values, dtype, *, strict, nan_to_null, geometry_format)` | construct a WKB geometry `Series` |
|  [02]   | `GeoDataFrame(data, schema, *, geometry_name, geometry_format, ...)`      | construct a geometry `DataFrame`  |

[ENTRYPOINT_SCOPE]: `GeoExprNameSpace` operations

`geom(col).st.<op>(...)` is the canonical operation surface (the `.st.` prefix drops from the rows), and the `GeoSeriesNameSpace` mirrors each row returning `Series`/`GeoSeries`. Predicates, `project`, and `cast` return `pl.Expr`; overlay, constructive, topology, affine, referencing, ring/part access, `set_srid`/`to_srid`, and `collect` return `GeoExpr`. Top-level sugar covers the UNARY ops over the default `geometry` column; binary predicates and overlay require the second argument and stay on `.st`.

[PREDICATES] `(other) -> pl.Expr` bool: `intersects` `contains` `contains_properly` `within` `covers` `covered_by` `disjoint` `touches` `crosses` `overlaps` `equals` `equals_identical`
[OVERLAY] `(other, grid_size=None) -> GeoExpr`: `union` `intersection` `difference` `symmetric_difference`
[OVERLAY_ALL] `(grid_size=None) -> GeoExpr`: `union_all` `intersection_all` `difference_all` `symmetric_difference_all` `unary_union`

| [INDEX] | [SURFACE]                                                                       | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------ | :------------------------------------ |
|  [01]   | `coordinates(output_dimension=None)`                                            | coordinate array per geometry         |
|  [02]   | `distance(other)`                                                               | pairwise distance                     |
|  [03]   | `hausdorff_distance(other, densify=None)`                                       | Hausdorff distance                    |
|  [04]   | `frechet_distance(other, densify=None)`                                         | discrete Frechet distance             |
|  [05]   | `dwithin(other, distance)`                                                      | within-distance predicate             |
|  [06]   | `equals_exact(other, tolerance)`                                                | tolerant equality predicate           |
|  [07]   | `relate(other)`                                                                 | DE-9IM relation matrix                |
|  [08]   | `relate_pattern(other, pattern)`                                                | DE-9IM pattern test                   |
|  [09]   | `project(other, normalized=False)`                                              | linear-referencing distance           |
|  [10]   | `cast(into)`                                                                    | geometry-type cast                    |
|  [11]   | `shared_paths(other)`                                                           | shared edges                          |
|  [12]   | `shortest_line(other)`                                                          | nearest segment                       |
|  [13]   | `snap(other, tolerance)`                                                        | snap to a reference geometry          |
|  [14]   | `buffer(distance, quad_segs, cap_style, join_style, mitre_limit, single_sided)` | buffer each geometry                  |
|  [15]   | `offset_curve(distance, quad_segs, join_style, mitre_limit)`                    | offset line at a distance             |
|  [16]   | `concave_hull(ratio=0.0, allow_holes=False)`                                    | concave hull                          |
|  [17]   | `clip_by_rect(bounds)`                                                          | clip by a rectangle                   |
|  [18]   | `simplify(tolerance, preserve_topology=True)`                                   | Douglas-Peucker simplification        |
|  [19]   | `segmentize(max_segment_length)`                                                | densify edges to a max segment length |
|  [20]   | `make_valid(method='linework', keep_collapsed=True)`                            | repair invalid geometry               |
|  [21]   | `set_precision(grid_size, mode='valid_output')`                                 | snap to a precision grid              |
|  [22]   | `line_merge(directed=False)`                                                    | merge connected lines                 |
|  [23]   | `remove_repeated_points(tolerance=0.0)`                                         | drop consecutive duplicate vertices   |
|  [24]   | `force_3d(z=0.0)`                                                               | add a Z ordinate                      |
|  [25]   | `affine_transform(matrix)`                                                      | arbitrary affine transform            |
|  [26]   | `translate` / `rotate` / `scale` / `skew`                                       | named affine transforms               |
|  [27]   | `interpolate(distance, normalized=False)`                                       | linear-referencing point              |
|  [28]   | `substring(start, end)`                                                         | linear-referencing segment            |
|  [29]   | `voronoi_polygons` / `delaunay_triangles`                                       | Voronoi / Delaunay tessellation       |
|  [30]   | `minimum_rotated_rectangle`                                                     | minimum rotated bounding rectangle    |
|  [31]   | `maximum_inscribed_circle`                                                      | maximum inscribed circle              |
|  [32]   | `get_interior_ring(index)`                                                      | one interior ring by index            |
|  [33]   | `get_geometry(index)` / `get_point(index)`                                      | collection / vertex access            |
|  [34]   | `set_srid(srid)` / `to_srid(srid)`                                              | assign / reproject CRS                |
|  [35]   | `collect(into=None)`                                                            | aggregate into a collection           |

- `buffer`: `(distance, quad_segs=8, cap_style='round', join_style='round', mitre_limit=5.0, single_sided=False)`
- `offset_curve`: `(distance, quad_segs=8, join_style='round', mitre_limit=5.0)`

[ENTRYPOINT_SCOPE]: `GeoExprNameSpace` niladic operations

These take no positional geometry, and the `to_*` serializers accept format keyword arguments only. Measures, ordinate/count extractors, structural predicates, `srid`, `multi`, `parts`, and the `to_*` serializers return `pl.Expr`; the derived-geometry and topology-normalization ops return `GeoExpr`.

[MEASURES] `-> pl.Expr`: `geometry_type` `dimension` `coordinate_dimension` `coordinate_type` `area` `length` `bounds` `total_bounds` `x` `y` `z` `m` `srid` `minimum_clearance` `count_geometries` `count_points` `count_coordinates` `count_interior_rings`
[STRUCTURAL] `-> pl.Expr` bool: `is_valid` `is_valid_reason` `is_empty` `is_simple` `is_ring` `is_closed` `is_ccw` `has_z` `has_m`
[SERIALIZE] `-> pl.Expr`: `to_wkt` `to_ewkt` `to_wkb` `to_geojson` `to_shapely` `to_dict`
[DERIVED] `-> GeoExpr`: `convex_hull` `envelope` `boundary` `centroid` `center` `point_on_surface` `coverage_union` `coverage_union_all` `normalize` `reverse` `node` `build_area` `force_2d` `flip_coordinates` `polygonize` `extract_unique_points` `exterior_ring` `interior_rings` `multi` `parts`

[ENTRYPOINT_SCOPE]: `GeoDataFrameNameSpace` join and IO

`sjoin` is the spatial join keyed by a `predicate` row; the `to_*`/`write_*` surfaces serialize the whole frame, and `to_geopandas`/`write_file` bridge to the GeoPandas/OGR ecosystem.

| [INDEX] | [SURFACE]                                                                                 | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `sjoin(other, on='geometry', how='inner', predicate='intersects', distance=None, *, ...)` | spatial join by predicate                  |
|  [02]   | `to_wkt` / `to_ewkt` / `to_wkb` `(*geometry_columns, ...)`                                | serialize geometry columns to text/bytes   |
|  [03]   | `to_geojson(*geometry_columns, indent=None)`                                              | serialize geometry columns to GeoJSON      |
|  [04]   | `to_shapely` / `to_dict` `(*geometry_columns)`                                            | convert geometry columns to shapely / dict |
|  [05]   | `to_dicts(geometry_name='geometry')`                                                      | row dicts with decoded geometry            |
|  [06]   | `to_geopandas(*, geometry_name='geometry', ...)`                                          | export to a GeoPandas `GeoDataFrame`       |
|  [07]   | `write_file(path, layer=None, driver=None, ...)`                                          | write the frame to an OGR data source      |
|  [08]   | `write_geojson` / `write_ndgeojson`                                                       | write GeoJSON / newline-delimited GeoJSON  |
|  [09]   | `plot` / `explore`                                                                        | Altair chart / interactive map             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Geometry is a WKB-encoded Polars column, and each `.st.<op>` is a registered plugin expression node composing into the same `LazyFrame` query graph as ordinary `Expr` work, inheriting predicate/projection pushdown, never a post-collect Python pass.
- `st(d)` resolves the namespace by container shape; one named op owns each concept, with `grid_size`/`quad_segs`/`cap_style`/`tolerance`/`method`/`mode` as call rows on the op, never a builder type per variant nor a per-container accessor function.
- `sjoin` selects its predicate by the `predicate` row, never a join method per predicate; DGGS cell snapping routes through `set_precision` on the shared grid, never a hand-rolled vertex-snap loop; the geometry column stays WKB internally, decoded only at a `to_*` serialization boundary.
- Each spatial transform captures geometry type, coordinate dimension, SRID, validity flag, bounds, and output row count as a spatial receipt.

[STACKING]:
- `polars`(`.api/polars.md`): `.st` ops register as plugin expression nodes on `pl.Expr`, folding into the same `LazyFrame` graph and inheriting pushdown; `polars_st.selectors` re-exports `geom`/`element`/`cast` for `select`/`with_columns`, composing with `polars.selectors` for dtype/name column addressing.
- `h3ronpy`(`.api/h3ronpy.md`): the WKB geometry column feeds the `GRID_DGGS` index — `.st` output geometries cross to `wkb_to_cells`/`geometry_to_cells` at the Arrow buffer, no byte round-trip.
- `geoarrow-rust-compute`(`.api/geoarrow-rust-compute.md`): GeoRust compute over the shared GeoArrow memory of the same frame without a copy.
- `pyarrow`(`.api/pyarrow.md`): egress through the parent Polars `to_arrow`/C-stream interface.
- `pyproj`(`.api/pyproj.md`): `set_srid` tags and `to_srid` reprojects CRS; the forge-scientific-env wrapper supplies `PROJ_DATA`/`GDAL_DATA`, so a bare-invocation PROJ/GDAL warning is harmless.
- `pyogrio`(`.api/pyogrio.md`)/`geopandas`(`.api/geopandas.md`): `read_file`/`write_file`/`from_geopandas`/`to_geopandas` bridge OGR data sources and GeoPandas objects at the edge.
- `data` GRID_DGGS owner: a spatial pipeline is one `GeoLazyFrame` graph — `scan_* -> geom(col).st.<predicate>/<overlay> -> sjoin -> collect/sink_parquet`.

[LOCAL_ADMISSION]:
- polars-st is the sole vectorized-GEOS-over-Polars surface for `data`; its `.st` expressions admit for predicates, measures, overlay, constructive, transform, projection, and spatial joins feeding the GRID_DGGS and geospatial owners.

[RAIL_LAW]:
- Package: `polars-st`
- Owns: GeoArrow/WKB geometry columns over Polars, GEOS-backed predicate/measure/overlay/constructive/transform operations, SRID projection, spatial joins, and WKB/WKT/EWKT/GeoJSON/shapely/OGR/GeoPandas serialization
- Accept: vectorized geometry pipelines feeding the GRID_DGGS and geospatial owners through `geom(...).st.<op>()` expressions and `GeoLazyFrame` pushdown
- Reject: a wrapper-rename of `st`/`geom`/`GeoExprNameSpace` ops; a per-row shapely loop where a vectorized `.st` expression exists; a hand-rolled GEOS overlay/buffer or PROJ reprojection; a parallel accessor per container shape; a builder type per operation variant; identity or CRS-catalog minting the runtime owns
