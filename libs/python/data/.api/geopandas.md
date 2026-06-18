# [PY_DATA_API_GEOPANDAS]

`geopandas` supplies geospatial tabular structures that extend pandas with a geometry column, CRS handling, vectorized Shapely operations, spatial joins, and multi-format IO. `GeoDataFrame` owns the table with an active geometry; `GeoSeries` owns a vectorized geometry column; module-level functions perform spatial joins, overlays, and file reads.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geopandas`
- package: `geopandas`
- import: `import geopandas as gpd`
- owner: `data`
- rail: geospatial
- capability: geometry-aware DataFrame and Series, CRS assignment and reprojection, vectorized predicates and constructive operations, spatial and nearest joins, overlay set algebra, dissolve aggregation, spatial indexing, and file/Parquet/Feather/PostGIS/Arrow IO

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- `geopandas.GeoDataFrame` — pandas `DataFrame` with one or more geometry columns and a designated active geometry; owns CRS, spatial joins, overlays, dissolve, and geospatial IO; geometry accessor `.geometry`, name accessor `.active_geometry_name`.
- `geopandas.GeoSeries` — vectorized geometry column; exposes constructive operations, predicates, measurements, and per-geometry coordinate accessors `.x`/`.y`/`.z`/`.m`.

[ENTRYPOINTS]:
- file reads: `read_file(filename, bbox=None, mask=None, columns=None, rows=None, engine=None, **kwargs) -> GeoDataFrame`, `read_parquet(path, columns=None, storage_options=None, bbox=None, to_pandas_kwargs=None, **kwargs) -> GeoDataFrame`, `read_feather(path, columns=None, to_pandas_kwargs=None, **kwargs) -> GeoDataFrame`, `read_postgis(sql, con, geom_col='geom', crs=None, index_col=None, coerce_float=True, parse_dates=None, params=None, chunksize=None) -> GeoDataFrame`, `list_layers(filename) -> pd.DataFrame`.
- frame construction: `GeoDataFrame.from_file(filename, **kwargs)`, `GeoDataFrame.from_features(features, crs=None, columns=None)`, `GeoDataFrame.from_postgis(sql, con, geom_col='geom', ...)`, `GeoDataFrame.from_arrow(table, geometry=None, to_pandas_kwargs=None)`.
- series construction: `points_from_xy(x, y, z=None, crs=None) -> GeometryArray`, `GeoSeries.from_xy(x, y, z=None, ...)`, `GeoSeries.from_wkb(data, ...)`, `GeoSeries.from_wkt(data, ...)`, `GeoSeries.from_file(filename, ...)`.
- geometry/CRS management: `GeoDataFrame.set_geometry(col, drop=None, inplace=False, crs=None)`, `.rename_geometry(col, inplace=False)`, `.set_crs(crs=None, epsg=None, inplace=False, allow_override=False)`, `.to_crs(crs=None, epsg=None, inplace=False)`, `.estimate_utm_crs(datum_name='WGS 84')`.
- spatial joins: `sjoin(left_df, right_df, how='inner', predicate='intersects', lsuffix='left', rsuffix='right', distance=None, on_attribute=None, **kwargs) -> GeoDataFrame`, `sjoin_nearest(left_df, right_df, how='inner', max_distance=None, lsuffix='left', rsuffix='right', distance_col=None, exclusive=False) -> GeoDataFrame`.
- overlay and clip: `overlay(df1, df2, how='intersection', keep_geom_type=None, make_valid=True) -> GeoDataFrame`, `clip(gdf, mask, keep_geom_type=False, sort=False) -> GeoDataFrame`.
- aggregation and explode: `GeoDataFrame.dissolve(by=None, aggfunc='first', as_index=True, level=None, sort=True, ...) -> GeoDataFrame`, `.explode(column=None, ignore_index=False, index_parts=False)`.
- predicates (vectorized): `contains`, `contains_properly`, `within`, `covers`, `covered_by`, `intersects`, `disjoint`, `touches`, `crosses`, `overlaps`, `dwithin(other, distance)`, `geom_equals`, `geom_equals_exact`, `relate`.
- constructive operations: `buffer(distance, ...)`, `intersection`, `union`, `difference`, `symmetric_difference`, `union_all`, `intersection_all`, `convex_hull`, `concave_hull`, `envelope`, `simplify`, `segmentize`, `offset_curve`, `voronoi_polygons`, `delaunay_triangles`, `make_valid`, `boundary`, `centroid`, `representative_point`.
- measurements and transforms: `area`, `length`, `distance(other)`, `hausdorff_distance`, `frechet_distance`, `bounds`, `total_bounds`, `affine_transform(matrix)`, `translate`, `rotate`, `scale`, `force_2d`, `force_3d`, `normalize`, `reverse`.
- spatial index: `GeoDataFrame.sindex`, `.has_sindex`, `.cx[xmin:xmax, ymin:ymax]` coordinate-slice indexer.
- egress: `GeoDataFrame.to_file(filename, driver=None, schema=None, index=None, ...)`, `.to_parquet(path, index=None, compression='snappy', geometry_encoding='WKB', ...)`, `.to_feather(path, ...)`, `.to_postgis(name, con, ...)`, `.to_arrow(*, index=None, geometry_encoding='WKB', interleaved=True, ...)`, `.to_wkb(hex=False, ...)`, `.to_wkt(...)`, `.to_json(na='null', show_bbox=False, drop_id=False, ...)`, `.to_geo_dict(...)`, `.iterfeatures(...)`.

[IMPLEMENTATION_LAW]:
- A `GeoDataFrame` has one active geometry column accessed by `.geometry`; predicates and constructive operations dispatch on the active geometry, set elsewhere via `set_geometry`/`rename_geometry`.
- CRS is metadata: `set_crs` assigns the declared CRS without transforming coordinates, while `to_crs` reprojects geometry values; mixing them silently corrupts coordinates.
- Predicates and constructive operations are vectorized over the whole `GeoSeries` via Shapely 2 and return aligned `GeoSeries`/`Series`; per-row Python iteration is never the geopandas path.
- Spatial joins are index-accelerated; `sjoin` uses a binary predicate (`intersects`, `within`, `contains`, `dwithin`) and `sjoin_nearest` uses distance with an optional `distance_col`; the spatial index `sindex` backs both.
- `read_file`/`to_file` select the GDAL driver through pyogrio (default) or fiona; `read_parquet`/`to_parquet` and `read_feather`/`to_feather` use the GeoParquet/GeoArrow encoding for lossless round-trips.
- `dissolve` is a spatial groupby-aggregate: it unions geometries per group and aggregates attributes with `aggfunc`, replacing manual group-then-union loops.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `geopandas`
- Owns: geometry-aware tabular structures, CRS assignment and reprojection, vectorized predicates and constructive operations, spatial/nearest joins, overlay set algebra, dissolve aggregation, and geospatial IO
- Accept: `GeoDataFrame`/`GeoSeries` as the owners, `.geometry` active-column dispatch, `set_crs` versus `to_crs` discipline, `sjoin`/`sjoin_nearest`/`overlay` for spatial relations, GeoParquet/GeoArrow for round-trips
- Reject: per-row geometry iteration, CRS reassignment via `set_crs` where reprojection is required, hand-rolled spatial-index nested loops, and duplicate per-format read/write wrappers outside the `read_*`/`to_*` family
