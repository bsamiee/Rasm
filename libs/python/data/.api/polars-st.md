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

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| --- | --- | --- | --- |
| [01] | `GeoExpr` | geo container | `polars.Expr` with the `.st` geometry accessor |
| [02] | `GeoSeries` | geo container | `polars.Series` of WKB geometry with the `.st` accessor |
| [03] | `GeoDataFrame` | geo container | `polars.DataFrame` carrying a geometry column with the `.st` accessor |
| [04] | `GeoLazyFrame` | geo container | `polars.LazyFrame` carrying a geometry column with the `.st` accessor |
| [05] | `GeoExprNameSpace` | accessor | geometry operations over an `Expr` |
| [06] | `GeoSeriesNameSpace` | accessor | geometry operations over a `Series` |
| [07] | `GeoDataFrameNameSpace` | accessor | geometry ops, `sjoin`, and IO over a `DataFrame` |
| [08] | `GeoLazyFrameNameSpace` | accessor | geometry ops and `sjoin` over a `LazyFrame` |
| [09] | `GeometryType` | literal alias | OGC geometry kinds (`Point`, `LineString`, `Polygon`, ...) |
| [10] | `DimensionType` | literal alias | topological dimension (`Point`, `Curve`, `Surface`) |
| [11] | `CoordinateType` | literal alias | coordinate axes (`XY`, `XYZ`, `XYZM`, `XYM`) |
| [12] | `PolarsGeometryType` | enum projection | `pl.Enum` over `GeometryType` |
| [13] | `PolarsDimensionType` | enum projection | `pl.Enum` over `DimensionType` |
| [14] | `PolarsCoordinateType` | enum projection | `pl.Enum` over `CoordinateType` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: geometry construction, parsing, and IO factories
- rail: spatial

The parsing factories lift coordinate columns or encoded text/bytes into a `GeoExpr` and share the `srid` policy row; the `st` accessor selects the namespace by container shape; `read_file`/`from_geopandas` materialize a `GeoDataFrame` from OGR sources or GeoPandas objects; `geom`/`element` are the column selectors that open a geometry expression.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `st` | `st(d: Expr \| Series \| DataFrame \| LazyFrame)` -> matching `*NameSpace` | shape-discriminating geometry accessor |
| [02] | `geom` | `geom(name="geometry", *more_names)` -> `GeoExpr` | select geometry column(s) as a geo expression |
| [03] | `element` | `element()` -> `GeoExpr` | geometry-typed alias for `polars.element` |
| [04] | `point` | `point(coords, srid=0)` -> `GeoExpr` | build `Point` geometries from coordinates |
| [05] | `multipoint` | `multipoint(coords, srid=0)` -> `GeoExpr` | build `MultiPoint` from a list of coordinates |
| [06] | `linestring` | `linestring(coords, srid=0)` -> `GeoExpr` | build `LineString` from lists of coordinates |
| [07] | `circularstring` | `circularstring(coords, srid=0)` -> `GeoExpr` | build `CircularString` from lists of coordinates |
| [08] | `multilinestring` | `multilinestring(coords, srid=0)` -> `GeoExpr` | build `MultiLineString` from nested coordinates |
| [09] | `polygon` | `polygon(coords, srid=0)` -> `GeoExpr` | build `Polygon` from nested ring coordinates |
| [10] | `rectangle` | `rectangle(bounds, srid=0)` -> `GeoExpr` | build `Polygon` from `(minx, miny, maxx, maxy)` |
| [11] | `from_wkb` | `from_wkb(expr)` -> `GeoExpr` | parse geometries from Well-Known Binary |
| [12] | `from_wkt` | `from_wkt(expr)` -> `GeoExpr` | parse geometries from Well-Known Text |
| [13] | `from_ewkt` | `from_ewkt(expr)` -> `GeoExpr` | parse geometries from Extended WKT |
| [14] | `from_geojson` | `from_geojson(expr)` -> `GeoExpr` | parse geometries from GeoJSON |
| [15] | `from_shapely` | `from_shapely(expr)` -> `GeoExpr` | parse geometries from shapely objects |
| [16] | `read_file` | `read_file(path_or_buffer, /, layer=None, encoding=None, columns=None, read_geometry=True, force_2d=False, skip_features=0, max_features=None, where=None, bbox=None, fids=None, sql=None, sql_dialect=None, return_fids=False)` -> `GeoDataFrame` | read an OGR data source into a `GeoDataFrame` |
| [17] | `from_geopandas` | `from_geopandas(data, *, schema_overrides=None, rechunk=True, nan_to_null=True, include_index=False)` -> `GeoDataFrame \| GeoSeries` | lift a GeoPandas `GeoDataFrame`/`GeoSeries` |

[ENTRYPOINT_SCOPE]: `GeoDataFrame`/`GeoSeries` construction
- rail: spatial

The geo containers subclass the Polars container; `geometry_format` selects how the incoming column is interpreted (encoded text/bytes or raw coordinates per geometry kind), and `geometry_name` names the geometry column on the frame.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `GeoSeries` | `GeoSeries(name=None, values=None, dtype=None, *, strict=True, nan_to_null=False, geometry_format=None)` | construct a WKB geometry `Series` |
| [02] | `GeoDataFrame` | `GeoDataFrame(data=None, schema=None, *, geometry_name="geometry", geometry_format=None, schema_overrides=None, strict=True, orient=None, infer_schema_length=..., nan_to_null=False)` | construct a geometry-bearing `DataFrame` |

[ENTRYPOINT_SCOPE]: `GeoExprNameSpace` geometry operations
- rail: spatial

`expr.st.<op>()` is the canonical operation surface; predicates take `other: IntoGeoExprColumn` and return boolean `pl.Expr`; overlay/constructive ops return a `GeoExpr`; the `GeoSeriesNameSpace` mirrors these row-for-row returning `Series`/`GeoSeries`. Top-level sugar functions cover only the UNARY measure/constructive ops over the default `geometry` column (`area`, `length`, `bounds`, `buffer`, `centroid`, `convex_hull`, `simplify`, `make_valid`, `set_precision`, `force_2d`/`force_3d`, `to_wkb`/`to_wkt`/`to_geojson`, ...); the BINARY predicates and overlay ops (`intersects`, `contains`, `within`, `dwithin`, `distance`, `union`, `intersection`, `difference`, `relate`, ...) are NOT top-level — they require a second geometry argument and live only on the `.st` namespace via `geom(col).st.<op>(other)`.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `.st.geometry_type` | `geometry_type()` -> `Expr` | per-geometry OGC type |
| [02] | `.st.dimension` | `dimension()` -> `Expr` | topological dimension |
| [03] | `.st.coordinate_dimension` | `coordinate_dimension()` -> `Expr` | coordinate dimension count |
| [04] | `.st.coordinate_type` | `coordinate_type()` -> `Expr` | `XY`/`XYZ`/`XYZM`/`XYM` axis kind |
| [05] | `.st.area` | `area()` -> `Expr` | planar area |
| [06] | `.st.length` | `length()` -> `Expr` | geometry length |
| [07] | `.st.bounds` | `bounds()` -> `Expr` | per-geometry `(minx, miny, maxx, maxy)` |
| [08] | `.st.total_bounds` | `total_bounds()` -> `Expr` | aggregate bounding box |
| [09] | `.st.x` / `.st.y` / `.st.z` / `.st.m` | `x()` / `y()` / `z()` / `m()` -> `Expr` | extract a single ordinate |
| [10] | `.st.coordinates` | `coordinates(output_dimension=None)` -> `Expr` | coordinate array per geometry |
| [11] | `.st.distance` | `distance(other)` -> `Expr` | pairwise distance |
| [12] | `.st.hausdorff_distance` / `.st.frechet_distance` | `hausdorff_distance(other, densify=None)` / `frechet_distance(other, densify=None)` -> `Expr` | Hausdorff / discrete Frechet distance |
| [13] | `.st.intersects` | `intersects(other)` -> `Expr` | intersection predicate |
| [14] | `.st.contains` / `.st.contains_properly` | `contains(other)` / `contains_properly(other)` -> `Expr` | containment predicates |
| [15] | `.st.within` | `within(other)` -> `Expr` | within predicate |
| [16] | `.st.dwithin` | `dwithin(other, distance)` -> `Expr` | within-distance predicate |
| [17] | `.st.covers` / `.st.covered_by` | `covers(other)` / `covered_by(other)` -> `Expr` | coverage predicates |
| [18] | `.st.disjoint` / `.st.touches` / `.st.crosses` / `.st.overlaps` | `<pred>(other)` -> `Expr` | topological predicates |
| [19] | `.st.equals` / `.st.equals_exact` / `.st.equals_identical` | `equals(other)` / `equals_exact(other, tolerance)` / `equals_identical(other)` -> `Expr` | equality predicates |
| [20] | `.st.relate` / `.st.relate_pattern` | `relate(other)` / `relate_pattern(other, pattern)` -> `Expr` | DE-9IM relation matrix / pattern test |
| [21] | `.st.is_valid` / `.st.is_valid_reason` | `is_valid()` -> `Expr` / `is_valid_reason()` -> `Expr` | OGC validity flag / reason |
| [22] | `.st.is_empty` / `.st.is_simple` / `.st.is_ring` / `.st.is_closed` / `.st.is_ccw` / `.st.has_z` / `.st.has_m` | `<pred>()` -> `Expr` | structural / dimensionality predicates |
| [23] | `.st.union` / `.st.intersection` / `.st.difference` / `.st.symmetric_difference` | `<op>(other, grid_size=None)` -> `GeoExpr` | pairwise overlay |
| [24] | `.st.union_all` / `.st.intersection_all` / `.st.difference_all` / `.st.symmetric_difference_all` | `<op>(grid_size=None)` -> `GeoExpr` | aggregate overlay |
| [25] | `.st.unary_union` / `.st.coverage_union` / `.st.coverage_union_all` | `unary_union(grid_size=None)` / `coverage_union()` / `coverage_union_all()` -> `GeoExpr` | self-union / coverage union |
| [26] | `.st.shared_paths` / `.st.shortest_line` / `.st.snap` | `shared_paths(other)` / `shortest_line(other)` -> `GeoExpr` / `snap(other, tolerance)` -> `GeoExpr` | shared edges / nearest segment / snap |
| [27] | `.st.buffer` | `buffer(distance, quad_segs=8, cap_style="round", join_style="round", mitre_limit=5.0, single_sided=False)` -> `GeoExpr` | buffer each geometry |
| [28] | `.st.offset_curve` | `offset_curve(distance, quad_segs=8, join_style="round", mitre_limit=5.0)` -> `GeoExpr` | offset line at a distance |
| [29] | `.st.convex_hull` / `.st.concave_hull` | `convex_hull()` -> `GeoExpr` / `concave_hull(ratio=0.0, allow_holes=False)` -> `GeoExpr` | convex / concave hull |
| [30] | `.st.envelope` / `.st.boundary` / `.st.centroid` / `.st.center` / `.st.point_on_surface` | `<op>()` -> `GeoExpr` | derived-geometry projections |
| [31] | `.st.clip_by_rect` | `clip_by_rect(bounds)` -> `GeoExpr` | clip by a rectangle |
| [32] | `.st.simplify` | `simplify(tolerance, preserve_topology=True)` -> `GeoExpr` | Douglas-Peucker simplification |
| [33] | `.st.segmentize` | `segmentize(max_segment_length)` -> `GeoExpr` | densify edges to a max segment length |
| [34] | `.st.make_valid` | `make_valid(method="linework"\|"structure", keep_collapsed=True)` -> `GeoExpr` | repair invalid geometry |
| [35] | `.st.set_precision` | `set_precision(grid_size, mode="valid_output"\|"no_topo"\|"keep_collapsed")` -> `GeoExpr` | snap to a precision grid |
| [36] | `.st.normalize` / `.st.reverse` / `.st.node` / `.st.build_area` / `.st.line_merge` | `<op>()` / `line_merge(directed=False)` -> `GeoExpr` | topology normalization |
| [37] | `.st.remove_repeated_points` | `remove_repeated_points(tolerance=0.0)` -> `GeoExpr` | drop consecutive duplicate vertices |
| [38] | `.st.force_2d` / `.st.force_3d` / `.st.flip_coordinates` | `force_2d()` / `force_3d(z=0.0)` / `flip_coordinates()` -> `GeoExpr` | dimension / axis transforms |
| [39] | `.st.affine_transform` | `affine_transform(matrix)` -> `GeoExpr` | arbitrary affine transform |
| [40] | `.st.translate` / `.st.rotate` / `.st.scale` / `.st.skew` | `<op>(...)` -> `GeoExpr` | named affine transforms |
| [41] | `.st.interpolate` / `.st.project` / `.st.substring` | `interpolate(distance, normalized=False)` -> `GeoExpr` / `project(other, normalized=False)` -> `Expr` / `substring(start, end)` -> `GeoExpr` | linear referencing |
| [42] | `.st.voronoi_polygons` / `.st.delaunay_triangles` | `voronoi_polygons(...)` / `delaunay_triangles(...)` -> `GeoExpr` | Voronoi / Delaunay tessellation |
| [43] | `.st.polygonize` / `.st.extract_unique_points` | `polygonize()` / `extract_unique_points()` -> `GeoExpr` | construct polygons / unique vertices |
| [44] | `.st.minimum_rotated_rectangle` / `.st.maximum_inscribed_circle` / `.st.minimum_clearance` | `<op>(...)` -> `GeoExpr`/`Expr` | extremal geometry measures |
| [45] | `.st.exterior_ring` / `.st.interior_rings` / `.st.get_interior_ring` | `exterior_ring()` / `interior_rings()` / `get_interior_ring(index)` -> `GeoExpr`/`Expr` | ring access |
| [46] | `.st.get_geometry` / `.st.get_point` / `.st.parts` | `get_geometry(index)` / `get_point(index)` / `parts()` -> `GeoExpr`/`Expr` | collection / vertex access |
| [47] | `.st.count_geometries` / `.st.count_points` / `.st.count_coordinates` / `.st.count_interior_rings` | `<op>()` -> `Expr` | structural counts |
| [48] | `.st.srid` / `.st.set_srid` / `.st.to_srid` | `srid()` -> `Expr` / `set_srid(srid)` -> `GeoExpr` / `to_srid(srid)` -> `GeoExpr` | read / assign / reproject CRS |
| [49] | `.st.cast` / `.st.multi` / `.st.collect` | `cast(into)` -> `Expr` / `multi()` -> `Expr` / `collect(into=None)` -> `GeoExpr` | geometry-type casting / aggregation |
| [50] | `.st.to_wkt` / `.st.to_ewkt` / `.st.to_wkb` / `.st.to_geojson` / `.st.to_shapely` / `.st.to_dict` | `to_<fmt>(...)` -> `Expr` | serialize geometry per row |

[ENTRYPOINT_SCOPE]: `GeoDataFrameNameSpace` join and IO
- rail: spatial

`sjoin` is the spatial join keyed by a `predicate` row; the `to_*`/`write_*` surfaces serialize the whole frame, and `to_geopandas`/`write_file` bridge to the GeoPandas/OGR ecosystem.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `.st.sjoin` | `sjoin(other, on="geometry", how="inner", predicate="intersects", distance=None, *, left_on=None, right_on=None, suffix="_right", validate="m:m", coalesce=None)` -> `GeoDataFrame` | spatial join by predicate |
| [02] | `.st.to_wkt` / `.st.to_ewkt` / `.st.to_wkb` | `to_<fmt>(*geometry_columns, ...)` -> `DataFrame` | serialize geometry columns to text/bytes |
| [03] | `.st.to_geojson` | `to_geojson(*geometry_columns, indent=None)` -> `DataFrame` | serialize geometry columns to GeoJSON |
| [04] | `.st.to_shapely` / `.st.to_dict` | `to_shapely(*geometry_columns)` / `to_dict(*geometry_columns)` -> `DataFrame` | convert geometry columns to shapely / dict |
| [05] | `.st.to_dicts` | `to_dicts(geometry_name="geometry")` -> `list[dict]` | row dicts with decoded geometry |
| [06] | `.st.to_geopandas` | `to_geopandas(*, geometry_name="geometry", use_pyarrow_extension_array=False, **kwargs)` -> `gpd.GeoDataFrame` | export to a GeoPandas `GeoDataFrame` |
| [07] | `.st.write_file` | `write_file(path, layer=None, driver=None, geometry_name="geometry", crs=None, encoding=None, append=False, ...)` -> `None` | write the frame to an OGR data source |
| [08] | `.st.write_geojson` / `.st.write_ndgeojson` | `write_geojson(...)` / `write_ndgeojson(...)` | write GeoJSON / newline-delimited GeoJSON |
| [09] | `.st.plot` / `.st.explore` | `plot(geometry_name="geometry", **kwargs)` -> `alt.Chart` / `explore(...)` | Altair chart / interactive map |

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
