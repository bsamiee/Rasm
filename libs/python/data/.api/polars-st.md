# [PY_DATA_API_POLARS_ST]

`polars-st` supplies the GeoArrow spatial extension for the data spatial rail: a `st` namespace grafted onto `polars.Expr`/`Series`/`DataFrame`/`LazyFrame` that runs GEOS-backed geometry operations inside the Polars query engine over a WKB-encoded geometry column, plus parsing factories (`point`, `from_wkb`, `from_wkt`, `from_geojson`, `from_shapely`) that lift raw coordinates into geometry, OGR/GeoPandas IO (`read_file`, `from_geopandas`), and a `GeoExprNameSpace` that owns the full predicate/measure/overlay/transform/topology vocabulary as expression rows. The package owner composes `st.geom(...).st.<op>()` into the GRID_DGGS spatial pipeline; it pushes geometry work into vectorized Polars expressions instead of per-row Python shapely loops, and it never re-implements GEOS overlay, buffering, or coordinate-reference projection the extension already binds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `polars-st`
- package: `polars-st`
- import: `polars_st`
- owner: `data`
- rail: spatial
- version: `0.7.0`
- license: `LGPL-2.1` (dynamic-linkage extension; ships GEOS/GDAL-backed native code)
- runtime deps: `polars>=1.38.1`, `pyarrow>=23.0.1`, `pyogrio>=0.12.1` (OGR vector IO backend)
- entry points: library use is import-only; the surface activates as the `.st` accessor on Polars `Expr`/`Series`/`DataFrame`/`LazyFrame`, registered as a Polars expression plugin against the WKB-encoded geometry column
- capability: GeoArrow/WKB geometry columns, GEOS-backed predicates (`intersects`, `contains`, `contains_properly`, `within`, `dwithin`, `covers`, ...), measures (`area`, `length`, `distance`, `hausdorff_distance`, `frechet_distance`, `bounds`), overlay (`union`, `intersection`, `difference`, `symmetric_difference`, `shared_paths`, `shortest_line`), constructive ops (`buffer`, `offset_curve`, `convex_hull`, `concave_hull`, `voronoi_polygons`, `delaunay_triangles`), affine/topology transforms (`affine_transform`, `simplify`, `snap`, `make_valid`, `set_precision`, `force_2d`/`force_3d`), linear referencing (`interpolate`, `project`, `substring`), SRID projection (`set_srid`/`to_srid`), WKB/WKT/EWKT/GeoJSON/shapely serialization, OGR/GeoPandas IO, and spatial joins (`sjoin`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geo frame/expr roots, accessor namespaces, and geometry vocabulary
- rail: spatial

`GeoExpr`/`GeoSeries`/`GeoDataFrame`/`GeoLazyFrame` subclass the matching Polars type and add the `.st` accessor; each `.st` returns the parallel `*NameSpace` carrying the geometry operations. `st(d)` is the polymorphic accessor that returns the namespace matching the Polars container shape. `GeometryType`/`DimensionType`/`CoordinateType` are `Literal` aliases enumerating the OGC geometry, dimension, and coordinate vocabularies; the `Polars*` siblings are `pl.Enum` projections of those aliases.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                                                                |
| :-----: | :---------------------- | :-------------- | :-------------------------------------------------------------------- |
|  [01]   | `GeoExpr`               | geo container   | `polars.Expr` with the `.st` geometry accessor                        |
|  [02]   | `GeoSeries`             | geo container   | `polars.Series` of WKB geometry with the `.st` accessor               |
|  [03]   | `GeoDataFrame`          | geo container   | `polars.DataFrame` carrying a geometry column with the `.st` accessor |
|  [04]   | `GeoLazyFrame`          | geo container   | `polars.LazyFrame` carrying a geometry column with the `.st` accessor |
|  [05]   | `GeoExprNameSpace`      | accessor        | geometry operations over an `Expr`                                    |
|  [06]   | `GeoSeriesNameSpace`    | accessor        | geometry operations over a `Series`                                   |
|  [07]   | `GeoDataFrameNameSpace` | accessor        | geometry ops, `sjoin`, and IO over a `DataFrame`                      |
|  [08]   | `GeoLazyFrameNameSpace` | accessor        | geometry ops and `sjoin` over a `LazyFrame`                           |
|  [09]   | `GeometryType`          | literal alias   | OGC geometry kinds (`Point`, `LineString`, `Polygon`, ...)            |
|  [10]   | `DimensionType`         | literal alias   | topological dimension (`Point`, `Curve`, `Surface`)                   |
|  [11]   | `CoordinateType`        | literal alias   | coordinate axes (`XY`, `XYZ`, `XYZM`, `XYM`)                          |
|  [12]   | `PolarsGeometryType`    | enum projection | `pl.Enum` over `GeometryType`                                         |
|  [13]   | `PolarsDimensionType`   | enum projection | `pl.Enum` over `DimensionType`                                        |
|  [14]   | `PolarsCoordinateType`  | enum projection | `pl.Enum` over `CoordinateType`                                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: geometry construction, parsing, and IO factories
- rail: spatial
- call: coordinate builders `point`/`multipoint`/`linestring`/`circularstring`/`multilinestring`/`polygon` take `(coords, srid=0)` and `rectangle` takes `(bounds, srid=0)`; text/bytes parsers `from_wkb`/`from_wkt`/`from_ewkt`/`from_geojson`/`from_shapely` take `(expr)` — all returning `GeoExpr`; `st(d: Expr | Series | DataFrame | LazyFrame)` returns the shape-matched `*NameSpace`, and `geom(name="geometry", *more_names)`/`element()` open a `GeoExpr`
- call: `read_file(path_or_buffer, /, layer=None, encoding=None, columns=None, read_geometry=True, force_2d=False, skip_features=0, max_features=None, where=None, bbox=None, fids=None, sql=None, sql_dialect=None, return_fids=False) -> GeoDataFrame`; `from_geopandas(data, *, schema_overrides=None, rechunk=True, nan_to_null=True, include_index=False) -> GeoDataFrame | GeoSeries`

The parsing factories lift coordinate columns or encoded text/bytes into a `GeoExpr` and share the `srid` policy row; `read_file`/`from_geopandas` materialize a `GeoDataFrame` from OGR sources or GeoPandas objects.

| [INDEX] | [SURFACE]         | [CAPABILITY]                                     |
| :-----: | :---------------- | :----------------------------------------------- |
|  [01]   | `st`              | shape-discriminating geometry accessor           |
|  [02]   | `geom`            | select geometry column(s) as a geo expression    |
|  [03]   | `element`         | geometry-typed alias for `polars.element`        |
|  [04]   | `point`           | build `Point` geometries from coordinates        |
|  [05]   | `multipoint`      | build `MultiPoint` from a list of coordinates    |
|  [06]   | `linestring`      | build `LineString` from lists of coordinates     |
|  [07]   | `circularstring`  | build `CircularString` from lists of coordinates |
|  [08]   | `multilinestring` | build `MultiLineString` from nested coordinates  |
|  [09]   | `polygon`         | build `Polygon` from nested ring coordinates     |
|  [10]   | `rectangle`       | build `Polygon` from `(minx, miny, maxx, maxy)`  |
|  [11]   | `from_wkb`        | parse geometries from Well-Known Binary          |
|  [12]   | `from_wkt`        | parse geometries from Well-Known Text            |
|  [13]   | `from_ewkt`       | parse geometries from Extended WKT               |
|  [14]   | `from_geojson`    | parse geometries from GeoJSON                    |
|  [15]   | `from_shapely`    | parse geometries from shapely objects            |
|  [16]   | `read_file`       | read an OGR data source into a `GeoDataFrame`    |
|  [17]   | `from_geopandas`  | lift a GeoPandas `GeoDataFrame`/`GeoSeries`      |

[ENTRYPOINT_SCOPE]: `GeoDataFrame`/`GeoSeries` construction
- rail: spatial
- call: `GeoSeries(name=None, values=None, dtype=None, *, strict=True, nan_to_null=False, geometry_format=None)`; `GeoDataFrame(data=None, schema=None, *, geometry_name="geometry", geometry_format=None, schema_overrides=None, strict=True, orient=None, infer_schema_length=..., nan_to_null=False)`

The geo containers subclass the Polars container; `geometry_format` selects how the incoming column is interpreted (encoded text/bytes or raw coordinates per geometry kind), and `geometry_name` names the geometry column on the frame.

| [INDEX] | [SURFACE]      | [CAPABILITY]                             |
| :-----: | :------------- | :--------------------------------------- |
|  [01]   | `GeoSeries`    | construct a WKB geometry `Series`        |
|  [02]   | `GeoDataFrame` | construct a geometry-bearing `DataFrame` |

[ENTRYPOINT_SCOPE]: `GeoExprNameSpace` argument-taking operations
- rail: spatial
- returns: predicates and `project` return `pl.Expr`; overlay, constructive, topology, affine, geometry referencing, ring/part access, `set_srid`/`to_srid`, and `collect` return `GeoExpr`; `cast` returns `pl.Expr`
- call: `buffer(distance, quad_segs=8, cap_style="round", join_style="round", mitre_limit=5.0, single_sided=False)`, `offset_curve(distance, quad_segs=8, join_style="round", mitre_limit=5.0)`, `make_valid(method="linework"|"structure", keep_collapsed=True)`, and `set_precision(grid_size, mode="valid_output"|"no_topo"|"keep_collapsed")` carry the full constructive knobs

`expr.st.<op>(...)` is the canonical operation surface (the `.st.` prefix drops from the rows below); predicates take `other: IntoGeoExprColumn`, and the `GeoSeriesNameSpace` mirrors these row-for-row returning `Series`/`GeoSeries`. Top-level sugar covers only the UNARY measure/constructive ops over the default `geometry` column (`area`, `buffer`, `centroid`, `simplify`, `make_valid`, `set_precision`, ...); the BINARY predicates and overlay ops require a second geometry argument and live only on `.st` via `geom(col).st.<op>(other)`.

| [INDEX] | [SURFACE]                                       | [ARGS]                                | [CAPABILITY]                          |
| :-----: | :---------------------------------------------- | :------------------------------------ | :------------------------------------ |
|  [01]   | `coordinates`                                   | `(output_dimension=None)`             | coordinate array per geometry         |
|  [02]   | `distance`                                      | `(other)`                             | pairwise distance                     |
|  [03]   | `hausdorff_distance` / `frechet_distance`       | `(other, densify=None)`               | Hausdorff / discrete Frechet distance |
|  [04]   | `intersects`                                    | `(other)`                             | intersection predicate                |
|  [05]   | `contains` / `contains_properly`                | `(other)`                             | containment predicates                |
|  [06]   | `within`                                        | `(other)`                             | within predicate                      |
|  [07]   | `dwithin`                                       | `(other, distance)`                   | within-distance predicate             |
|  [08]   | `covers` / `covered_by`                         | `(other)`                             | coverage predicates                   |
|  [09]   | `disjoint` / `touches` / `crosses` / `overlaps` | `(other)`                             | topological predicates                |
|  [10]   | `equals` / `equals_identical`                   | `(other)`                             | equality predicates                   |
|  [11]   | `equals_exact`                                  | `(other, tolerance)`                  | tolerant equality predicate           |
|  [12]   | `relate`                                        | `(other)`                             | DE-9IM relation matrix                |
|  [13]   | `relate_pattern`                                | `(other, pattern)`                    | DE-9IM pattern test                   |
|  [14]   | `project`                                       | `(other, normalized=False)`           | linear-referencing distance           |
|  [15]   | `cast`                                          | `(into)`                              | geometry-type cast                    |
|  [16]   | `union`                                         | `(other, grid_size=None)`             | pairwise union                        |
|  [17]   | `intersection`                                  | `(other, grid_size=None)`             | pairwise intersection                 |
|  [18]   | `difference`                                    | `(other, grid_size=None)`             | pairwise difference                   |
|  [19]   | `symmetric_difference`                          | `(other, grid_size=None)`             | pairwise symmetric difference         |
|  [20]   | `union_all`                                     | `(grid_size=None)`                    | aggregate union                       |
|  [21]   | `intersection_all`                              | `(grid_size=None)`                    | aggregate intersection                |
|  [22]   | `difference_all`                                | `(grid_size=None)`                    | aggregate difference                  |
|  [23]   | `symmetric_difference_all`                      | `(grid_size=None)`                    | aggregate symmetric difference        |
|  [24]   | `unary_union`                                   | `(grid_size=None)`                    | self-union                            |
|  [25]   | `shared_paths` / `shortest_line`                | `(other)`                             | shared edges / nearest segment        |
|  [26]   | `snap`                                          | `(other, tolerance)`                  | snap to a reference geometry          |
|  [27]   | `buffer`                                        | see `- call:`                         | buffer each geometry                  |
|  [28]   | `offset_curve`                                  | see `- call:`                         | offset line at a distance             |
|  [29]   | `concave_hull`                                  | `(ratio=0.0, allow_holes=False)`      | concave hull                          |
|  [30]   | `clip_by_rect`                                  | `(bounds)`                            | clip by a rectangle                   |
|  [31]   | `simplify`                                      | `(tolerance, preserve_topology=True)` | Douglas-Peucker simplification        |
|  [32]   | `segmentize`                                    | `(max_segment_length)`                | densify edges to a max segment length |
|  [33]   | `make_valid`                                    | see `- call:`                         | repair invalid geometry               |
|  [34]   | `set_precision`                                 | see `- call:`                         | snap to a precision grid              |
|  [35]   | `line_merge`                                    | `(directed=False)`                    | merge connected lines                 |
|  [36]   | `remove_repeated_points`                        | `(tolerance=0.0)`                     | drop consecutive duplicate vertices   |
|  [37]   | `force_3d`                                      | `(z=0.0)`                             | add a Z ordinate                      |
|  [38]   | `affine_transform`                              | `(matrix)`                            | arbitrary affine transform            |
|  [39]   | `translate` / `rotate` / `scale` / `skew`       | `(...)`                               | named affine transforms               |
|  [40]   | `interpolate`                                   | `(distance, normalized=False)`        | linear-referencing point              |
|  [41]   | `substring`                                     | `(start, end)`                        | linear-referencing segment            |
|  [42]   | `voronoi_polygons` / `delaunay_triangles`       | `(...)`                               | Voronoi / Delaunay tessellation       |
|  [43]   | `minimum_rotated_rectangle`                     | `(...)`                               | minimum rotated bounding rectangle    |
|  [44]   | `maximum_inscribed_circle`                      | `(...)`                               | maximum inscribed circle              |
|  [45]   | `get_interior_ring`                             | `(index)`                             | one interior ring by index            |
|  [46]   | `get_geometry` / `get_point`                    | `(index)`                             | collection / vertex access            |
|  [47]   | `set_srid` / `to_srid`                          | `(srid)`                              | assign / reproject CRS                |
|  [48]   | `collect`                                       | `(into=None)`                         | aggregate into a collection           |

[ENTRYPOINT_SCOPE]: `GeoExprNameSpace` niladic operations
- rail: spatial
- returns: measures, ordinate/count extractors, structural predicates, `srid`, `multi`, `parts`, and the `to_*` serializers return `pl.Expr`; `convex_hull`, `envelope`/`boundary`/`centroid`/`center`/`point_on_surface`, `coverage_union`/`coverage_union_all`, `normalize`/`reverse`/`node`/`build_area`, `force_2d`/`flip_coordinates`, `polygonize`/`extract_unique_points`, and `exterior_ring`/`interior_rings` return `GeoExpr`

These take no positional geometry; the `to_*` serializers accept format keyword arguments only.

| [INDEX] | [SURFACE]                                                                          | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `geometry_type`                                                                    | per-geometry OGC type                   |
|  [02]   | `dimension`                                                                        | topological dimension                   |
|  [03]   | `coordinate_dimension`                                                             | coordinate dimension count              |
|  [04]   | `coordinate_type`                                                                  | `XY`/`XYZ`/`XYZM`/`XYM` axis kind       |
|  [05]   | `area`                                                                             | planar area                             |
|  [06]   | `length`                                                                           | geometry length                         |
|  [07]   | `bounds`                                                                           | per-geometry `(minx, miny, maxx, maxy)` |
|  [08]   | `total_bounds`                                                                     | aggregate bounding box                  |
|  [09]   | `x` / `y` / `z` / `m`                                                              | extract a single ordinate               |
|  [10]   | `is_valid` / `is_valid_reason`                                                     | OGC validity flag / reason              |
|  [11]   | `is_empty` / `is_simple` / `is_ring` / `is_closed` / `is_ccw` / `has_z` / `has_m`  | structural / dimensionality predicates  |
|  [12]   | `srid`                                                                             | read CRS                                |
|  [13]   | `multi`                                                                            | promote to multi-geometry               |
|  [14]   | `parts`                                                                            | explode collection parts                |
|  [15]   | `count_geometries` / `count_points` / `count_coordinates` / `count_interior_rings` | structural counts                       |
|  [16]   | `minimum_clearance`                                                                | minimum clearance distance              |
|  [17]   | `to_wkt` / `to_ewkt` / `to_wkb` / `to_geojson` / `to_shapely` / `to_dict`          | serialize geometry per row              |
|  [18]   | `convex_hull`                                                                      | convex hull                             |
|  [19]   | `envelope` / `boundary` / `centroid` / `center` / `point_on_surface`               | derived-geometry projections            |
|  [20]   | `coverage_union` / `coverage_union_all`                                            | coverage union                          |
|  [21]   | `normalize` / `reverse` / `node` / `build_area`                                    | topology normalization                  |
|  [22]   | `force_2d` / `flip_coordinates`                                                    | dimension / axis transforms             |
|  [23]   | `polygonize` / `extract_unique_points`                                             | construct polygons / unique vertices    |
|  [24]   | `exterior_ring` / `interior_rings`                                                 | ring access                             |

[ENTRYPOINT_SCOPE]: `GeoDataFrameNameSpace` join and IO
- rail: spatial
- call: `sjoin(other, on="geometry", how="inner", predicate="intersects", distance=None, *, left_on=None, right_on=None, suffix="_right", validate="m:m", coalesce=None) -> GeoDataFrame`; `write_file(path, layer=None, driver=None, geometry_name="geometry", crs=None, encoding=None, append=False, ...) -> None`; `to_geopandas(*, geometry_name="geometry", use_pyarrow_extension_array=False, **kwargs) -> gpd.GeoDataFrame`

`sjoin` is the spatial join keyed by a `predicate` row; the `to_*`/`write_*` surfaces serialize the whole frame, and `to_geopandas`/`write_file` bridge to the GeoPandas/OGR ecosystem.

| [INDEX] | [SURFACE]                           | [ARGS]                                 | [CAPABILITY]                               |
| :-----: | :---------------------------------- | :------------------------------------- | :----------------------------------------- |
|  [01]   | `sjoin`                             | see `- call:`                          | spatial join by predicate                  |
|  [02]   | `to_wkt` / `to_ewkt` / `to_wkb`     | `(*geometry_columns, ...)`             | serialize geometry columns to text/bytes   |
|  [03]   | `to_geojson`                        | `(*geometry_columns, indent=None)`     | serialize geometry columns to GeoJSON      |
|  [04]   | `to_shapely` / `to_dict`            | `(*geometry_columns)`                  | convert geometry columns to shapely / dict |
|  [05]   | `to_dicts`                          | `(geometry_name="geometry")`           | row dicts with decoded geometry            |
|  [06]   | `to_geopandas`                      | see `- call:`                          | export to a GeoPandas `GeoDataFrame`       |
|  [07]   | `write_file`                        | see `- call:`                          | write the frame to an OGR data source      |
|  [08]   | `write_geojson` / `write_ndgeojson` | `(...)`                                | write GeoJSON / newline-delimited GeoJSON  |
|  [09]   | `plot` / `explore`                  | `(geometry_name="geometry", **kwargs)` | Altair chart / interactive map             |

## [04]-[IMPLEMENTATION_LAW]

[SPATIAL_GEOMETRY]:
- import: `import polars_st as st` at boundary scope only; module-level import is banned by the manifest import policy.
- accessor axis: one `st(d)` resolves the namespace by container shape (`Expr`/`Series`/`DataFrame`/`LazyFrame`), never a per-container accessor function; expression pipelines route through `geom(columns).st.<op>(...)`, lazy frames through `GeoLazyFrame` for query-engine pushdown.
- construction axis: `point`/`linestring`/`polygon`/`rectangle` and `from_wkb`/`from_wkt`/`from_geojson`/`from_shapely` are the parsing rows that lift coordinates or encoded payloads into a `GeoExpr`; `geometry_format` is the `GeoSeries`/`GeoDataFrame` constructor row selecting the input encoding, never a parallel reader type per format.
- operation axis: `GeoExprNameSpace` (116 ops) owns the full predicate/measure/overlay/constructive/transform vocabulary as expression rows; `grid_size`, `quad_segs`, `cap_style`, `tolerance`, `preserve_topology`, and `mode` are call rows on the owning op, never a builder type per variant. Each `.st.<op>` is a registered Polars expression plugin node, so it composes into the SAME `LazyFrame` query graph as ordinary `Expr` work and inherits predicate/projection pushdown — never a post-collect Python pass.
- sugar/selector axis: top-level sugar is UNARY-only (`area`/`buffer`/`centroid`/`simplify`/...) over the default `geometry` column; binary predicates/overlay stay on `.st`. `polars_st.selectors` re-exports `geom`/`element`/`cast` as the geometry-column accessors used inside `select`/`with_columns`; column addressing by dtype/name composes with `polars.selectors` at the Polars layer.
- predicate axis: `intersects`/`contains`/`contains_properly`/`within`/`dwithin`/`covers`/`covered_by`/`disjoint`/`touches`/`crosses`/`overlaps`/`equals` are boolean `pl.Expr` rows over `other: IntoGeoExprColumn`; `relate`/`relate_pattern` carry the DE-9IM matrix; `sjoin` selects the predicate by its `predicate` row, never a join method per predicate.
- projection axis: `set_srid` assigns and `to_srid` reprojects the CRS; the forge-scientific-env wrapper supplies `PROJ_DATA`/`GDAL_DATA`, so any PROJ/GDAL warning under bare invocation is harmless.
- topology axis: `make_valid`/`set_precision`/`simplify`/`normalize`/`node`/`build_area` are repair/precision rows keyed by `method`/`mode`/`grid_size`; DGGS cell snapping routes through `set_precision` on the shared grid, never a hand-rolled vertex-snap loop.
- serialization axis: `to_wkb`/`to_wkt`/`to_ewkt`/`to_geojson`/`to_shapely` are encoding rows over the same geometry column; the geometry column stays WKB internally, decoded only at the serialization boundary.
- evidence: each spatial transform captures geometry type, coordinate dimension, SRID, validity flag, bounds, and output row count as a spatial receipt.
- integration axis: polars-st stacks ON the `polars` engine (geometry is a WKB-encoded column whose `.st` ops are plugin expression nodes), feeds the H3 DGGS rail (`h3ronpy` Arrow-native cell ops over the same frame) and `geoarrow-rust-compute` (GeoRust on shared GeoArrow memory) without a byte-roundtrip, and egresses to `pyarrow` via the parent Polars `to_arrow`/C-stream interface. A spatial pipeline is one `GeoLazyFrame` graph: `scan_* -> geom(col).st.<predicate>/<overlay> -> sjoin -> collect/sink_parquet`.
- boundary: polars-st owns vectorized GEOS geometry over Polars columns; raster/coverage post-processing and CRS catalog work route to their own owners; GeoPandas/OGR bridges (`from_geopandas`/`to_geopandas`/`read_file`/`write_file`, backed by `pyogrio`) stay at the edge; per-row shapely loops stay outside this package.

[RAIL_LAW]:
- Package: `polars-st`
- Owns: GeoArrow/WKB geometry columns over Polars, GEOS-backed predicates/measures/overlay/constructive/transform operations, SRID projection, spatial joins, and WKB/WKT/EWKT/GeoJSON/shapely/OGR/GeoPandas serialization
- Accept: vectorized geometry pipelines feeding the GRID_DGGS and geospatial owners through `geom(...).st.<op>()` expressions and `GeoLazyFrame` pushdown
- Reject: wrapper-renames of `st`/`geom`/`GeoExprNameSpace` ops; a per-row shapely loop where a vectorized `.st` expression exists; a hand-rolled GEOS overlay, buffer, or PROJ reprojection; a parallel accessor per container shape; a builder type per operation variant; identity or CRS-catalog minting the runtime owns
