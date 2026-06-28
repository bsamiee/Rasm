# [PY_DATA_API_GEOPANDAS]

`geopandas` supplies geospatial tabular structures that extend pandas with a geometry column, CRS handling, vectorized Shapely operations, spatial joins, and multi-format IO. `GeoDataFrame` owns the table with an active geometry; `GeoSeries` owns a vectorized geometry column; module-level functions perform spatial joins, overlays, and file reads. It is the tabular layer over the geospatial stack: the geometry column is a `shapely` `GeometryArray` (vectorized GEOS), CRS metadata is a `pyproj.CRS`, the GeoArrow/GeoParquet wire is `pyarrow`, file IO routes through `pyogrio` (GDAL), and the resulting `GeoSeries.to_wkb()`/`points_from_xy` arrays feed `h3ronpy.vector.wkb_to_cells` for the DGGS rail and `shapely` STRtree for indexing — geopandas never re-implements geometry, projection, codec, or driver logic those owners hold.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geopandas`
- package: `geopandas`
- import: `import geopandas as gpd`
- owner: `data`
- rail: geospatial
- version: `1.1.4`
- asset: pure Python; geometry kernel is the `shapely` 2.x GEOS extension, CRS is `pyproj`, default IO engine is `pyogrio` (GDAL), Arrow interchange is `pyarrow`
- entry points: library use is import-only; no console script
- capability: geometry-aware DataFrame and Series, CRS assignment and reprojection, vectorized binary predicates and constructive operations, spatial/nearest joins, overlay set algebra, dissolve aggregation, sampling/coordinate extraction, geocoding, spatial indexing via shapely STRtree, and file/Parquet/Feather/PostGIS/Arrow IO

## [02]-[CAPTURE]

[PUBLIC_TYPES]:
- `geopandas.GeoDataFrame` — pandas `DataFrame` with one or more geometry columns and a designated active geometry; owns CRS, spatial joins, overlays, dissolve, and geospatial IO; geometry accessor `.geometry`, name accessor `.active_geometry_name`.
- `geopandas.GeoSeries` — vectorized geometry column; exposes constructive operations, predicates, measurements, and per-geometry coordinate accessors `.x`/`.y`/`.z`/`.m`.

[ENTRYPOINTS]:
- file reads: `read_file(filename, bbox=None, mask=None, columns=None, rows=None, engine=None, **kwargs) -> GeoDataFrame`, `read_parquet(path, columns=None, storage_options=None, bbox=None, to_pandas_kwargs=None, **kwargs) -> GeoDataFrame`, `read_feather(path, columns=None, to_pandas_kwargs=None, **kwargs) -> GeoDataFrame`, `read_postgis(sql, con, geom_col='geom', crs=None, index_col=None, coerce_float=True, parse_dates=None, params=None, chunksize=None) -> GeoDataFrame`, `list_layers(filename) -> pd.DataFrame`.
- frame construction: `GeoDataFrame.from_file(filename, **kwargs)`, `GeoDataFrame.from_features(features, crs=None, columns=None)`, `GeoDataFrame.from_postgis(sql, con, geom_col='geom', ...)`, `GeoDataFrame.from_arrow(table, geometry=None, to_pandas_kwargs=None)`.
- series construction: `points_from_xy(x, y, z=None, crs=None) -> GeometryArray`, `GeoSeries.from_xy(x, y, z=None, ...)`, `GeoSeries.from_wkb(data, ...)`, `GeoSeries.from_wkt(data, ...)`, `GeoSeries.from_arrow(arr, ...)`, `GeoSeries.from_file(filename, ...)`.
- geometry/CRS management: `GeoDataFrame.set_geometry(col, drop=None, inplace=False, crs=None)`, `.rename_geometry(col, inplace=False)`, `.set_crs(crs=None, epsg=None, inplace=False, allow_override=False)`, `.to_crs(crs=None, epsg=None, inplace=False)`, `.estimate_utm_crs(datum_name='WGS 84')`.
- spatial joins: `sjoin(left_df, right_df, how='inner', predicate='intersects', lsuffix='left', rsuffix='right', distance=None, on_attribute=None, **kwargs) -> GeoDataFrame`, `sjoin_nearest(left_df, right_df, how='inner', max_distance=None, lsuffix='left', rsuffix='right', distance_col=None, exclusive=False) -> GeoDataFrame`.
- overlay and clip: `overlay(df1, df2, how='intersection', keep_geom_type=None, make_valid=True) -> GeoDataFrame`, `clip(gdf, mask, keep_geom_type=False, sort=False) -> GeoDataFrame`, `GeoDataFrame.clip_by_rect(xmin, ymin, xmax, ymax) -> GeoDataFrame`.
- aggregation and explode: `GeoDataFrame.dissolve(by=None, aggfunc='first', as_index=True, level=None, sort=True, ...) -> GeoDataFrame`, `.explode(column=None, ignore_index=False, index_parts=False)`.
- sampling and coordinate extraction: `GeoDataFrame.sample_points(size, method='uniform', seed=None, rng=None, **kwargs) -> GeoSeries`, `.get_coordinates(include_z=False, ignore_index=False, index_parts=False, *, include_m=False) -> pd.DataFrame`, `GeoSeries.count_coordinates()`, `.count_geometries()`, `.count_interior_rings()`, `.get_geometry(index)`.
- geocoding (`geopandas.tools`): `tools.geocode(strings, provider=None, **kwargs) -> GeoDataFrame`, `tools.reverse_geocode(points, provider=None, **kwargs) -> GeoDataFrame` over a `geopy` provider; `tools.collect(x, multi=False)` rolls geometries into a single multipart geometry.
- predicates (vectorized): `contains`, `contains_properly`, `within`, `covers`, `covered_by`, `intersects`, `disjoint`, `touches`, `crosses`, `overlaps`, `dwithin(other, distance)`, `geom_equals`, `geom_equals_exact`, `relate`, `relate_pattern(other, pattern)`, plus boolean state `is_valid`, `is_valid_reason()`, `is_empty`, `is_simple`, `is_ring`, `is_ccw`, `is_closed`, `has_z`.
- constructive operations: `buffer(distance, ...)`, `intersection`, `union`, `difference`, `symmetric_difference`, `union_all(method='unary', grid_size=None)`, `intersection_all`, `clip_by_rect`, `convex_hull`, `concave_hull(ratio=0.0, allow_holes=False)`, `envelope`, `minimum_bounding_circle`, `minimum_rotated_rectangle`, `minimum_bounding_radius`, `simplify`, `segmentize(max_segment_length)`, `offset_curve(distance, ...)`, `shared_paths(other)`, `shortest_line(other)`, `snap(other, tolerance)`, `polygonize`, `build_area`, `line_merge`, `voronoi_polygons`, `delaunay_triangles`, `constrained_delaunay_triangles`, `make_valid`, `boundary`, `centroid`, `representative_point`, `extract_unique_points`, `remove_repeated_points(tolerance)`, `set_precision(grid_size, mode)`, `get_precision()`.
- measurements and transforms: `area`, `length`, `distance(other)`, `hausdorff_distance`, `frechet_distance`, `bounds`, `total_bounds`, `interpolate(distance, normalized=False)`, `project(other, normalized=False)`, `affine_transform(matrix)`, `translate`, `rotate`, `scale`, `skew`, `transform(func)`, `force_2d`, `force_3d`, `normalize`, `reverse`.
- spatial index: `GeoDataFrame.sindex` (a `shapely` `STRtree`-backed `SpatialIndex` with `query`/`nearest`/`query_bulk`), `.has_sindex`, `.cx[xmin:xmax, ymin:ymax]` coordinate-slice indexer.
- egress: `GeoDataFrame.to_file(filename, driver=None, schema=None, index=None, ...)`, `.to_parquet(path, index=None, compression='snappy', geometry_encoding='WKB', write_covering_bbox=False, ...)`, `.to_feather(path, ...)`, `.to_postgis(name, con, ...)`, `.to_arrow(*, index=None, geometry_encoding='WKB', interleaved=True, include_z=None) -> ArrowTable`, `.to_wkb(hex=False, ...)`, `.to_wkt(...)`, `.to_json(na='null', show_bbox=False, drop_id=False, ...)`, `.to_geo_dict(na='null', show_bbox=False, drop_id=False) -> dict`, `.iterfeatures(na='null', show_bbox=False, drop_id=False)`, `.explore(...)` (interactive folium map).

[IMPLEMENTATION_LAW]:
- A `GeoDataFrame` has one active geometry column accessed by `.geometry`; predicates and constructive operations dispatch on the active geometry, set elsewhere via `set_geometry`/`rename_geometry`. Multiple geometry columns coexist; only the active one drives the geospatial methods.
- CRS is metadata: `set_crs` assigns the declared `pyproj.CRS` without transforming coordinates, while `to_crs` reprojects geometry values; mixing them silently corrupts coordinates. `estimate_utm_crs` derives a metric projection from the data extent for area/length in meters.
- Predicates and constructive operations are vectorized over the whole `GeoSeries` via Shapely 2 GEOS and return aligned `GeoSeries`/`Series`; per-row Python iteration is never the geopandas path. The same operation names exist on `GeoSeries` and on `GeoDataFrame.geometry` — one polymorphic surface, no per-shape variant.
- Spatial joins are index-accelerated; `sjoin` uses a binary predicate (`intersects`, `within`, `contains`, `dwithin`) and `sjoin_nearest` uses distance with an optional `distance_col`; the spatial index `sindex` (a `shapely` `STRtree`) backs both and is reused for repeated queries.
- `read_file`/`to_file` select the GDAL driver through `pyogrio` (default) or `fiona`; `read_parquet`/`to_parquet` and `read_feather`/`to_feather` use the GeoParquet/GeoArrow encoding for lossless round-trips, and `to_arrow`/`from_arrow` hand a zero-copy `pyarrow` table across the wire to the columnar rail.
- `dissolve` is a spatial groupby-aggregate: it unions geometries per group and aggregates attributes with `aggfunc`, replacing manual group-then-union loops.

[INTEGRATION_LAW]:
- shapely seam: every constructive/predicate/measurement method delegates to the vectorized `shapely` 2 top-level functions over the backing `GeometryArray`; do not import `shapely` per-row inside geopandas pipelines — the `GeoSeries` already is the vectorized surface, and `GeoSeries.values` exposes the raw `GeometryArray` for direct shapely calls when needed.
- pyproj seam: CRS objects are `pyproj.CRS`; pass an EPSG int, a CRS, or a WKT string and let geopandas hold the metadata — never reproject coordinates by hand when `to_crs` owns the `pyproj.Transformer`.
- pyarrow/GeoParquet seam: `to_arrow`/`to_parquet` emit GeoArrow/GeoParquet (`geometry_encoding='WKB'` or `'geoarrow'`, `write_covering_bbox=True` for predicate pushdown); the resulting `pyarrow` table feeds `polars`/`datafusion`/`duckdb` spatial readers in one columnar hand-off with no intermediate file.
- h3ronpy DGGS seam: `GeoSeries.to_wkb()` produces the WKB array that `h3ronpy.vector.wkb_to_cells(arr, resolution, containment_mode=...)` consumes to index geometry onto the H3 grid; `points_from_xy` builds the centroid geometry that `h3ronpy.vector.coordinates_to_cells` mirrors at the array level. The geometry leaves geopandas as WKB/coordinate arrays, never as per-row Python geometries.
- postgis seam: `read_postgis(sql, con, ...)`/`to_postgis(name, con, ...)` round-trip through a SQLAlchemy/GeoAlchemy connection; pair with the query rail's connection rather than minting a parallel DB client.

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `geopandas`
- Owns: geometry-aware tabular structures, CRS assignment and reprojection, vectorized predicates/constructive/measurement operations, spatial/nearest joins, overlay set algebra, dissolve aggregation, sampling/coordinate extraction, geocoding, and geospatial IO
- Accept: `GeoDataFrame`/`GeoSeries` as the owners, `.geometry` active-column dispatch, `set_crs` versus `to_crs` discipline, `sjoin`/`sjoin_nearest`/`overlay` for spatial relations, GeoParquet/GeoArrow for round-trips, `to_arrow`/`to_wkb` as the zero-copy hand-off to the columnar and DGGS rails
- Reject: per-row geometry iteration where the vectorized `GeoSeries` method exists, CRS reassignment via `set_crs` where reprojection is required, hand-rolled spatial-index nested loops when `sindex` is the STRtree, re-implementing shapely/pyproj/pyogrio capability geopandas already composes, and duplicate per-format read/write wrappers outside the `read_*`/`to_*` family
