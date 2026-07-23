# [PY_DATA_API_GEOPANDAS]

`geopandas` extends pandas with an active geometry column, holding CRS as a `pyproj.CRS` and dispatching vectorized Shapely GEOS ops, spatial and nearest joins, overlay, and dissolve over `GeoDataFrame`/`GeoSeries`. Geometry, projection, codec, and driver concerns delegate to their owners — the column is a `shapely` `GeometryArray`, IO routes through `pyogrio` (GDAL) and the GeoArrow/GeoParquet `pyarrow` wire, `GeoSeries.to_wkb()`/`points_from_xy` arrays feed the `h3ronpy` DGGS rail — so geopandas composes the geospatial stack, never re-implements it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geopandas`
- package: `geopandas` (BSD-3-Clause)
- module: `import geopandas as gpd`
- owner: `data`
- rail: geospatial
- asset: pure Python; geometry kernel is `shapely` 2 GEOS, CRS is `pyproj`, file IO is `pyogrio` (GDAL), Arrow interchange is `pyarrow`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry-aware frame, column, and index

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]   | [CAPABILITY]                                                                                    |
| :-----: | :-------------- | :-------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `GeoDataFrame`  | tabular frame   | pandas frame with an active `.geometry` column, CRS, joins, overlay, dissolve, IO               |
|  [02]   | `GeoSeries`     | geometry column | vectorized geometry column; `.x`/`.y`/`.z`/`.m` accessors; `.values` is the raw `GeometryArray` |
|  [03]   | `GeometryArray` | extension array | shapely-backed geometry buffer under a column (`geopandas.array`)                               |
|  [04]   | `SpatialIndex`  | spatial index   | `.sindex` STRtree with `query`/`nearest`/`query_bulk`                                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction, IO, CRS, and spatial operations

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `read_file` `read_parquet` `read_feather`                          | static   | GDAL and GeoParquet/GeoArrow readers             |
|  [02]   | `read_postgis` `list_layers`                                       | static   | PostGIS read; layer enumeration                  |
|  [03]   | `GeoDataFrame.from_features / from_postgis / from_arrow`           | factory  | frame from features, PostGIS, or an Arrow table  |
|  [04]   | `points_from_xy` `GeoSeries.from_xy / from_wkb / from_wkt`         | factory  | geometry column from coordinates or WKB/WKT      |
|  [05]   | `GeoSeries.from_arrow / from_file`                                 | factory  | geometry column from an Arrow array or a file    |
|  [06]   | `set_geometry` `rename_geometry`                                   | instance | set or rename the active geometry column         |
|  [07]   | `set_crs` `to_crs` `estimate_utm_crs`                              | instance | assign CRS, reproject, derive a metric UTM CRS   |
|  [08]   | `sjoin` `sjoin_nearest`                                            | static   | predicate join; nearest join by distance         |
|  [09]   | `overlay` `clip`                                                   | static   | overlay set algebra; rectangular or mask clip    |
|  [10]   | `dissolve` `explode`                                               | instance | groupby-union aggregate; multipart explode       |
|  [11]   | `sample_points` `get_coordinates` `get_geometry`                   | instance | point sampling; coordinate and part extraction   |
|  [12]   | `tools.geocode` `tools.reverse_geocode` `tools.collect`            | static   | geopy forward/reverse geocode; collect multipart |
|  [13]   | `sindex` `has_sindex` `cx[xslice, yslice]`                         | property | STRtree index; coordinate-slice `cx`             |
|  [14]   | `to_file` `to_postgis`                                             | instance | GDAL driver and PostGIS writers                  |
|  [15]   | `to_parquet` `to_feather` `to_arrow`                               | instance | GeoParquet/GeoArrow zero-copy egress             |
|  [16]   | `to_wkb` `to_wkt` `to_json` `to_geo_dict` `iterfeatures` `explore` | instance | WKB/WKT/GeoJSON/dict/folium serialization        |

[VECTORIZED_OPS]: GEOS ufuncs on the active geometry, delegating to `shapely` top-level functions — one polymorphic surface on `GeoSeries` and `GeoDataFrame.geometry`, returning aligned `GeoSeries`/`Series`
- predicates: `contains` `contains_properly` `within` `covers` `covered_by` `intersects` `disjoint` `touches` `crosses` `overlaps` `dwithin(other, distance)` `geom_equals` `geom_equals_exact` `relate` `relate_pattern(other, pattern)`; state `is_valid` `is_valid_reason` `is_empty` `is_simple` `is_ring` `is_ccw` `is_closed` `has_z`
- constructive: `buffer` `intersection` `union` `union_all` `intersection_all` `difference` `symmetric_difference` `clip_by_rect` `convex_hull` `concave_hull` `envelope` `minimum_bounding_circle` `minimum_rotated_rectangle` `minimum_bounding_radius` `simplify` `segmentize` `offset_curve` `shared_paths` `shortest_line` `snap` `polygonize` `build_area` `line_merge` `voronoi_polygons` `delaunay_triangles` `constrained_delaunay_triangles` `make_valid` `boundary` `centroid` `representative_point` `extract_unique_points` `remove_repeated_points` `set_precision` `get_precision`
- measurement: `area` `length` `distance` `hausdorff_distance` `frechet_distance` `bounds` `total_bounds` `interpolate` `project` `affine_transform` `translate` `rotate` `scale` `skew` `transform` `force_2d` `force_3d` `normalize` `reverse` `count_coordinates` `count_geometries` `count_interior_rings`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One active geometry column drives every predicate, constructive, and measurement op; `set_geometry`/`rename_geometry` reassign it and additional geometry columns coexist inert.
- `set_crs` writes CRS metadata without moving coordinates while `to_crs` reprojects — mixing them corrupts coordinates; `estimate_utm_crs` derives a metric CRS from the data extent for area and length in meters.
- Operations vectorize over the whole `GeoSeries` through Shapely 2 GEOS; the same op names on `GeoSeries` and `GeoDataFrame.geometry` are one surface, never a per-row Python loop.
- `sjoin`/`sjoin_nearest` ride the `sindex` STRtree, reused across repeated queries; `dissolve` unions geometries per group and aggregates attributes with `aggfunc`.

[STACKING]:
- `shapely`(`.api/shapely.md`): every predicate/constructive/measurement delegates to shapely 2 top-level ufuncs over the backing `GeometryArray`; `GeoSeries.values` exposes the raw array for a direct shapely call, and `STRtree` backs `sindex` — never a per-row shapely import.
- `pyproj`(`.api/pyproj.md`): CRS objects are `pyproj.CRS` and `to_crs` owns the `pyproj.Transformer`; pass an EPSG int, a CRS, or a WKT string and never reproject coordinates by hand.
- `pyarrow`(`.api/pyarrow.md`): `to_arrow`/`to_parquet` emit GeoArrow/GeoParquet (`geometry_encoding='WKB'` or `'geoarrow'`, `write_covering_bbox=True` for predicate pushdown); the resulting table feeds `polars-st`/`datafusion`/`duckdb` in one columnar hand-off with no intermediate file.
- `pyogrio`(`.api/pyogrio.md`): `read_file`/`to_file` select the GDAL driver through pyogrio; `list_layers` enumerates layers before a scoped read.
- `h3ronpy`(`.api/h3ronpy.md`): `GeoSeries.to_wkb()` feeds `wkb_to_cells(arr, resolution, containment_mode)` and `points_from_xy` feeds `coordinates_to_cells`; geometry leaves as WKB or coordinate arrays, never per-row Python geometries.
- within-lib: the data geospatial owner drives the vectorized array form and the `sindex` STRtree join primitive directly; `read_postgis`/`to_postgis` round-trip through the query rail's SQLAlchemy/GeoAlchemy connection rather than a parallel DB client.

[LOCAL_ADMISSION]:
- Admit `geopandas` as the geometry-aware tabular owner on the data geospatial rail, composing shapely/pyproj/pyogrio/pyarrow rather than re-importing them per row.

[RAIL_LAW]:
- Package: `geopandas`
- Owns: geometry-aware tabular structures, CRS assignment and reprojection, vectorized predicate/constructive/measurement ops, spatial and nearest joins, overlay set algebra, dissolve aggregation, sampling and coordinate extraction, geocoding, and geospatial IO
- Accept: `GeoDataFrame`/`GeoSeries` owners, `.geometry` active-column dispatch, `set_crs` versus `to_crs` discipline, `sjoin`/`sjoin_nearest`/`overlay` for spatial relations, GeoParquet/GeoArrow round-trips, `to_arrow`/`to_wkb` as the zero-copy hand-off to the columnar and DGGS rails
- Reject: per-row geometry iteration where a vectorized `GeoSeries` method exists, CRS reassignment via `set_crs` where reprojection is required, hand-rolled spatial-index loops when `sindex` is the STRtree, re-implementing shapely/pyproj/pyogrio capability, and duplicate per-format read/write wrappers outside the `read_*`/`to_*` family
