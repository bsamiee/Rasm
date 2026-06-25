# [PY_DATA_API_PYPROJ]

`pyproj` supplies a Python binding to PROJ for coordinate reference system definition, coordinate transformation, and geodesic computation. It provides `CRS`, `Transformer`, `Geod`, and `Proj` as primary owners, with WKT/PROJ4/EPSG/JSON interchange on the CRS boundary and area-of-interest-aware operation selection on the transformer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyproj`
- package: `pyproj` `3.7.2`
- import: `import pyproj`
- license: MIT (bundled PROJ data under its own `LICENSE_proj`)
- python: `>=3.11`
- native: binds PROJ `9.8.1` (`pyproj.proj_version_str`); ships GDAL-independent PROJ; `pyproj.show_versions()` reports the linked native versions
- owner: `data`
- rail: geospatial
- capability: CRS construction and interchange, axis-aware coordinate transformation, geodesic forward/inverse and area computation, transformer-group enumeration with availability ranking, PROJ database queries, and CDN grid network control

## [02]-[CAPTURE]

[PUBLIC_TYPES]:
- `pyproj.CRS` — coordinate reference system; constructed from EPSG, WKT, PROJ4, JSON, dict, CF, or user input; exposes axis, datum, ellipsoid, and projection metadata.
- `pyproj.Transformer` — coordinate transformation between two CRS or from a pipeline; the supported high-performance transform owner.
- `pyproj.Geod` — geodesic calculator on an ellipsoid; forward, inverse, intermediate, length, and polygon area/perimeter.
- `pyproj.Proj` — single-CRS cartographic projection of lon/lat to projected x/y; a thin `Transformer` over a geographic-to-projected pair.
- `pyproj.transformer.TransformerGroup` — ranked set of candidate transformations between two CRS; `.transformers` (ordered list), `.best_available` (bool), `.unavailable_operations` (grid-missing candidates), `.download_grids(...)` to fetch the grids that would upgrade a ballpark candidate.
- `pyproj.aoi.AreaOfInterest(west_lon_degree, south_lat_degree, east_lon_degree, north_lat_degree)`, `pyproj.aoi.BBox`, `pyproj.aoi.AreaOfUse` — bounding-box hints that bias operation selection.
- `pyproj.enums.TransformDirection` (`FORWARD`, `INVERSE`, `IDENT`); `pyproj.enums.WktVersion` (`WKT2_2019`/`WKT2_2019_SIMPLIFIED`/`WKT2_2015`/`WKT1_GDAL`/`WKT1_ESRI`/…); `pyproj.enums.ProjVersion` (`PROJ_4`, `PROJ_5`) — transform and serialization vocabularies.
- `pyproj.database.CRSInfo`, `pyproj.database.Unit`, `pyproj.database.PJType` — typed query-result records and the CRS-type vocabulary for `query_crs_info`.

[ENTRYPOINTS]:
- CRS construction: `CRS.from_epsg(code)`, `CRS.from_wkt(wkt)`, `CRS.from_proj4(proj4)`, `CRS.from_string(s)`, `CRS.from_dict(d)`, `CRS.from_json(json)`, `CRS.from_json_dict(d)`, `CRS.from_authority(auth, code)`, `CRS.from_cf(d)`, `CRS.from_user_input(value)`; the bare `CRS(value)` constructor dispatches over the same input shapes (one polymorphic entry, the `from_*` family is the explicit-discriminant form).
- CRS interchange: `CRS.to_epsg(min_confidence=70)`, `CRS.to_wkt(version=WktVersion.WKT2_2019, pretty=False, output_axis_rule=None)`, `CRS.to_proj4()`, `CRS.to_json(pretty=False)`, `CRS.to_json_dict()`, `CRS.to_dict()`, `CRS.to_string()`, `CRS.to_authority(min_confidence=70)`, `CRS.to_cf()`, `CRS.cs_to_cf()`, `CRS.to_2d()`, `CRS.to_3d()`, `CRS.get_geod()`.
- CRS metadata: `CRS.axis_info`, `.datum`, `.ellipsoid`, `.prime_meridian`, `.coordinate_system`, `.coordinate_operation`, `.area_of_use`, `.utm_zone`, `.geodetic_crs`, `.source_crs`/`.target_crs` (bound CRS), `.sub_crs_list` (compound), `.name`, `.type_name`, `.scope`, `.remarks`, `.list_authority()`, `.get_non_deprecated()`.
- CRS predicates: `CRS.is_geographic`, `.is_projected`, `.is_geocentric`, `.is_bound`, `.is_compound`, `.is_vertical`, `.is_engineering`, `.is_derived`, `.is_deprecated`, `.is_exact_same(other)`, `.equals(other, ignore_axis_order=False)`.
- transformer construction: `Transformer.from_crs(crs_from, crs_to, always_xy=False, area_of_interest=None, authority=None, accuracy=None, allow_ballpark=None, force_over=False, only_best=None)`, `Transformer.from_pipeline(proj_pipeline)`, `Transformer.from_proj(proj_from, proj_to, always_xy=False)`; `TransformerGroup(crs_from, crs_to, always_xy=False, area_of_interest=None, authority=None, accuracy=None, allow_ballpark=True, allow_superseded=False)` enumerates candidates.
- transformation: `Transformer.transform(xx, yy, zz=None, tt=None, radians=False, errcheck=False, direction=TransformDirection.FORWARD, inplace=False)`, `Transformer.itransform(points, switch=False, time_3rd=False, ...)`, `Transformer.transform_bounds(left, bottom, right, top, densify_pts=21, radians=False, errcheck=False, direction=FORWARD) -> (l, b, r, t)`.
- transformer metadata: `Transformer.accuracy`, `.area_of_use`, `.operations`, `.has_inverse`, `.get_last_used_operation()`, `.is_network_enabled`, `.definition`, `.description`, `.source_crs`, `.target_crs`, `.to_wkt()`, `.to_json()`, `.to_json_dict()`, `.to_proj4()`, `.is_exact_same(other)`.
- geodesic: `Geod(ellps='WGS84')` or `Geod(a=..., f=...)`; `Geod.fwd(lons, lats, az, dist, radians=False, inplace=False, return_back_azimuth=True)`, `Geod.inv(lons1, lats1, lons2, lats2, radians=False, inplace=False, return_back_azimuth=True)`, `Geod.npts(...)`, `Geod.fwd_intermediate(...)`, `Geod.inv_intermediate(...)`, `Geod.line_length(lons, lats)`, `Geod.line_lengths(lons, lats)` (segment-wise), `Geod.polygon_area_perimeter(lons, lats) -> (area, perimeter)`, `Geod.geometry_area_perimeter(geom)`, `Geod.geometry_length(geom)`; read `.a`/`.b`/`.f`/`.es`/`.sphere`/`.initstring`.
- database queries: `pyproj.database.query_crs_info(auth_name=None, pj_types=None, area_of_interest=None, contains=False, allow_deprecated=False) -> list[CRSInfo]`, `pyproj.database.query_utm_crs_info(datum_name=None, area_of_interest=None, contains=False)`, `pyproj.database.get_database_metadata(key)`, `pyproj.get_authorities()`, `pyproj.get_codes(auth_name, pj_type, allow_deprecated=False)`, `pyproj.get_units_map()`, `pyproj.get_ellps_map()`, `pyproj.get_prime_meridians_map()`, `pyproj.get_proj_operations_map()`.
- CDN grid network: `pyproj.network.set_network_enabled(active=None)`, `pyproj.network.is_network_enabled()`, `pyproj.network.set_ca_bundle_path(ca_bundle_path)`, `pyproj.sync.get_transform_grid_list(...)`, `pyproj.sync.get_proj_endpoint()`, `pyproj.set_use_global_context(active=True)`; `pyproj.datadir.set_data_dir(path)`/`get_data_dir()` and `PROJ_DATA` env var locate the PROJ data directory.

[EXCEPTIONS]:
- `pyproj.exceptions.CRSError` — CRS parse, construction, or conversion failure.
- `pyproj.exceptions.ProjError` — transformation or projection failure.
- `pyproj.exceptions.GeodError` — geodesic computation failure.
- `pyproj.exceptions.DataDirError` — PROJ data directory not found.

[IMPLEMENTATION_LAW]:
- `always_xy=True` forces lon/lat (easting/northing) argument order regardless of the CRS-declared axis order; set it at construction to avoid silent axis swaps, since PROJ honors authority-declared axis order by default.
- `Transformer` instances are reusable; construct once per CRS pair and reuse across calls rather than rebuilding per point.
- Operation selection is data-driven: `area_of_interest`, `accuracy`, `allow_ballpark`, and `only_best` bias which `TransformerGroup` candidate PROJ picks; `get_last_used_operation()` reports the operation actually applied.
- `transform` accepts scalars, sequences, or NumPy arrays and returns the same shape; `inplace=True` mutates the input arrays and `radians=True` switches angular units.
- `CRS.to_epsg` returns `None` below `min_confidence`; never assume an EPSG code exists for an arbitrary WKT CRS.
- `Proj` is the projection special case of `Transformer` (geographic to or from one projected CRS); prefer `Transformer.from_crs` for general CRS-to-CRS work.
- `set_network_enabled` and the CDN grid sync control transformation-grid download; enable the network when high-accuracy datum shifts require grids absent from the local cache, and point `set_ca_bundle_path` at the trust store when the CDN sits behind a proxy. `set_use_global_context(True)` shares one PROJ context across threads — set it once at boundary init, never per-transform.
- `TransformerGroup` exposes the candidate ranking that `from_crs` hides: read `.transformers` for the ordered list, `.best_available` to detect a ballpark-only fallback, `.unavailable_operations` for grid-missing candidates, and call `.download_grids()` to promote a candidate before transforming. `get_last_used_operation()` on a plain `Transformer` reports which candidate PROJ actually applied.
- `CRS.to_cf()`/`from_cf()`/`cs_to_cf()` are the CF-convention bridge for NetCDF/Zarr grid_mapping attributes; `utm_zone`/`query_utm_crs_info(datum_name, AreaOfInterest)` pick the right UTM CRS for a bounding box without hard-coding EPSG codes.

[INTEGRATION_STACK]:
- the geospatial reprojection rail stacks `pyproj.CRS` as the canonical CRS owner across `geopandas` (`GeoDataFrame.crs` / `.to_crs(CRS)`), `shapely` (geometry is CRS-free; pyproj `Transformer.transform` reprojects coordinates), `rasterio`/`rioxarray` (raster CRS round-trips via `CRS.from_wkt`/`to_wkt`), and `pyogrio` (`read_dataframe` returns CRS as WKT that `CRS.from_user_input` ingests). One `CRS` instance is interchanged by WKT/EPSG/JSON, never re-parsed per library.
- coordinate transforms compose `Transformer.from_crs(src, dst, always_xy=True)` built once at boundary scope, then `transform(x_array, y_array)` over numpy arrays — the vectorized path that feeds `shapely.transform`/`shapely.ops.transform` and `geopandas` reprojection. For STAC/Arrow tables, `CRS.to_json_dict()` is the projjson the `stac-geoparquet`/`geoarrow` metadata carries.
- the CF bridge stacks `CRS.from_cf(ds.<var>.attrs)` / `CRS.to_cf()` against `xarray`/`netcdf4`/`zarr` grid_mapping attributes, so a tensor-cube CRS round-trips without a hand-rolled attribute map; `Geod.geometry_area_perimeter(shapely_geom)` gives ellipsoidal area beside the planar `shapely.area`.

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pyproj`
- Owns: CRS construction and interchange, axis-aware coordinate transformation, geodesic and ellipsoidal computation, transformer-group enumeration, PROJ database queries, CDN grid network control
- Accept: `Transformer.from_crs` with `always_xy` set explicitly and reused across calls, `CRS` interchange via `to_epsg`/`to_wkt`/`to_json`, `Geod` for distance and area on the ellipsoid, `area_of_interest` for operation selection
- Reject: per-point transformer reconstruction, implicit axis-order assumptions, planar-distance approximation where geodesic distance is required, and hand-rolled datum-shift math when PROJ grids exist
