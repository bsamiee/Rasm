# [PY_DATA_API_PYPROJ]

`pyproj` supplies a Python binding to PROJ for coordinate reference system definition, coordinate transformation, and geodesic computation. It provides `CRS`, `Transformer`, `Geod`, and `Proj` as primary owners, with WKT/PROJ4/EPSG/JSON interchange on the CRS boundary and area-of-interest-aware operation selection on the transformer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyproj`
- package: `pyproj`
- import: `import pyproj`
- owner: `data`
- rail: geospatial
- capability: CRS construction and interchange, axis-aware coordinate transformation, geodesic forward/inverse and area computation, transformer-group enumeration, PROJ database queries, and CDN grid network control

## [02]-[CAPTURE]

[PUBLIC_TYPES]:
- `pyproj.CRS` — coordinate reference system; constructed from EPSG, WKT, PROJ4, JSON, dict, CF, or user input; exposes axis, datum, ellipsoid, and projection metadata.
- `pyproj.Transformer` — coordinate transformation between two CRS or from a pipeline; the supported high-performance transform owner.
- `pyproj.Geod` — geodesic calculator on an ellipsoid; forward, inverse, intermediate, length, and polygon area/perimeter.
- `pyproj.Proj` — single-CRS cartographic projection of lon/lat to projected x/y; a thin `Transformer` over a geographic-to-projected pair.
- `pyproj.transformer.TransformerGroup` — ranked set of candidate transformations between two CRS, ordered by accuracy and grid availability.
- `pyproj.aoi.AreaOfInterest`, `pyproj.aoi.BBox`, `pyproj.aoi.AreaOfUse` — bounding-box hints that bias operation selection.
- `pyproj.enums.TransformDirection` (`FORWARD`, `INVERSE`, `IDENT`), `pyproj.enums.WktVersion`, `pyproj.enums.ProjVersion` — transform and serialization vocabularies.

[ENTRYPOINTS]:
- CRS construction: `CRS.from_epsg(code)`, `CRS.from_wkt(wkt)`, `CRS.from_proj4(proj4)`, `CRS.from_string(s)`, `CRS.from_dict(d)`, `CRS.from_json(json)`, `CRS.from_authority(auth, code)`, `CRS.from_cf(d)`, `CRS.from_user_input(value)`.
- CRS interchange: `CRS.to_epsg(min_confidence=70)`, `CRS.to_wkt(version=WktVersion.WKT2_2019, ...)`, `CRS.to_proj4()`, `CRS.to_json()`, `CRS.to_dict()`, `CRS.to_authority()`, `CRS.to_2d()`, `CRS.to_3d()`, `CRS.get_geod()`.
- CRS predicates: `CRS.is_geographic`, `.is_projected`, `.is_geocentric`, `.is_bound`, `.is_compound`, `.is_vertical`, `.is_derived`, `.is_deprecated`, `.is_exact_same(other)`.
- transformer construction: `Transformer.from_crs(crs_from, crs_to, always_xy=False, area_of_interest=None, authority=None, accuracy=None, allow_ballpark=None, force_over=False, only_best=None)`, `Transformer.from_pipeline(proj_pipeline)`, `Transformer.from_proj(proj_from, proj_to, always_xy=False)`.
- transformation: `Transformer.transform(xx, yy, zz=None, tt=None, radians=False, errcheck=False, direction=TransformDirection.FORWARD, inplace=False)`, `Transformer.itransform(points, ...)`, `Transformer.transform_bounds(left, bottom, right, top, densify_pts=21, ...)`.
- transformer metadata: `Transformer.accuracy`, `.area_of_use`, `.operations`, `.has_inverse`, `.get_last_used_operation()`, `.is_network_enabled`, `.to_wkt()`, `.to_json()`, `.to_proj4()`.
- geodesic: `Geod(ellps='WGS84')` or `Geod(a=..., f=...)`, `Geod.fwd(lons, lats, az, dist)`, `Geod.inv(lons1, lats1, lons2, lats2)`, `Geod.npts(...)`, `Geod.fwd_intermediate(...)`, `Geod.inv_intermediate(...)`, `Geod.line_length(lons, lats)`, `Geod.polygon_area_perimeter(lons, lats)`, `Geod.geometry_area_perimeter(geom)`, `Geod.geometry_length(geom)`.
- database queries: `pyproj.database.query_crs_info(auth_name=None, pj_types=None, area_of_interest=None)`, `pyproj.database.query_utm_crs_info(datum_name, area_of_interest)`, `pyproj.get_authorities()`, `pyproj.get_codes(auth_name, pj_type)`, `pyproj.get_units_map()`, `pyproj.get_ellps_map()`.
- CDN grid network: `pyproj.network.set_network_enabled(active=None)`, `pyproj.sync.get_transform_grid_list(...)`, `pyproj.set_use_global_context(active=True)`.

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
- `set_network_enabled` and the CDN grid sync control transformation-grid download; enable the network when high-accuracy datum shifts require grids absent from the local cache.

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pyproj`
- Owns: CRS construction and interchange, axis-aware coordinate transformation, geodesic and ellipsoidal computation, transformer-group enumeration, PROJ database queries, CDN grid network control
- Accept: `Transformer.from_crs` with `always_xy` set explicitly and reused across calls, `CRS` interchange via `to_epsg`/`to_wkt`/`to_json`, `Geod` for distance and area on the ellipsoid, `area_of_interest` for operation selection
- Reject: per-point transformer reconstruction, implicit axis-order assumptions, planar-distance approximation where geodesic distance is required, and hand-rolled datum-shift math when PROJ grids exist
