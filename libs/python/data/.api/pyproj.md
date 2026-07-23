# [PY_DATA_API_PYPROJ]

`pyproj` binds PROJ as the data branch's coordinate-reference-system and transformation owner: `CRS` construction with WKT/PROJ4/EPSG/JSON/CF interchange, axis-aware `Transformer` reprojection under area-of-interest operation selection, and `Geod` ellipsoidal distance and area. Geometry stays CRS-free at `shapely` and tabular CRS rides `geopandas`/`rasterio`/`pyogrio` as one `pyproj.CRS` interchanged by WKT/EPSG/JSON, so pyproj owns the projection math the geospatial rail never re-implements.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyproj`
- package: `pyproj` (MIT; bundled PROJ data under `LICENSE_proj`)
- module: `import pyproj`
- owner: `data`
- rail: geospatial
- asset: native PROJ binding, GDAL-independent; `proj_version_str` / `show_versions()` report the linked native versions

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: CRS, transformer, geodesic owners with their operation vocabularies

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :----------------------------------------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `CRS`                                                        | class         | CRS from EPSG/WKT/PROJ4/JSON/CF with axis-datum metadata |
|  [02]   | `Transformer`                                                | class         | axis-aware coordinate transformation owner               |
|  [03]   | `Geod`                                                       | class         | geodesic and ellipsoidal calculator                      |
|  [04]   | `Proj`                                                       | class         | single-CRS lon/lat projection; thin `Transformer`        |
|  [05]   | `transformer.TransformerGroup`                               | class         | ranked candidate transformations with availability       |
|  [06]   | `aoi.AreaOfInterest` `aoi.BBox` `aoi.AreaOfUse`              | class         | bounding-box hints biasing operation selection           |
|  [07]   | `enums.TransformDirection`                                   | enum          | `FORWARD` / `INVERSE` / `IDENT` transform direction      |
|  [08]   | `enums.WktVersion` `enums.ProjVersion`                       | enum          | WKT2/WKT1 and PROJ_4/PROJ_5 serialization vocabularies   |
|  [09]   | `enums.GeodIntermediateFlag`                                 | enum          | intermediate-point recompute and count-rounding flags    |
|  [10]   | `database.CRSInfo` `database.Unit` `database.PJType`         | record        | database query-result records and CRS-type vocabulary    |
|  [11]   | `exceptions.CRSError` `ProjError` `GeodError` `DataDirError` | exception     | typed CRS, transform, geodesic, and data-dir failures    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction, transformation, geodesic, database, and network surfaces

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `CRS(value)` `CRS.from_epsg` `from_wkt` `from_user_input`        | factory  | polymorphic CRS construction over every input shape |
|  [02]   | `Transformer.from_crs` `from_pipeline` `from_proj`               | factory  | transformer construction; AOI/accuracy selection    |
|  [03]   | `transform` `itransform` `transform_bounds`                      | instance | scalar, array, and bounds coordinate transform      |
|  [04]   | `TransformerGroup(...)`                                          | ctor     | ranked candidate enumeration with grid availability |
|  [05]   | `Geod.fwd` `inv` `fwd_intermediate` `inv_intermediate` `npts`    | instance | geodesic forward, inverse, and intermediate points  |
|  [06]   | `polygon_area_perimeter` `geometry_area_perimeter` `line_length` | instance | ellipsoidal polygon area and length                 |
|  [07]   | `database.query_crs_info` `query_utm_crs_info`                   | static   | PROJ database CRS and UTM queries                   |
|  [08]   | `network.set_network_enabled` `set_ca_bundle_path`               | static   | CDN grid network toggle and CA trust store          |

[CRS_INTERCHANGE]: `to_epsg` `to_wkt` `to_proj4` `to_json` `to_json_dict` `to_dict` `to_string` `to_authority` `to_cf` `cs_to_cf` `to_2d` `to_3d` `get_geod`
[CRS_METADATA]: `axis_info` `datum` `ellipsoid` `prime_meridian` `coordinate_system` `coordinate_operation` `area_of_use` `utm_zone` `geodetic_crs` `source_crs` `target_crs` `sub_crs_list` `name` `type_name` `scope` `remarks` `list_authority` `get_non_deprecated`
[CRS_PREDICATES]: `is_geographic` `is_projected` `is_geocentric` `is_bound` `is_compound` `is_vertical` `is_engineering` `is_derived` `is_deprecated` `is_exact_same` `equals`
[TRANSFORMER_READOUT]: `accuracy` `area_of_use` `operations` `has_inverse` `get_last_used_operation` `is_network_enabled` `definition` `description` `source_crs` `target_crs` `to_wkt` `to_json` `to_json_dict` `to_proj4`
[PROJ_REGISTRY]: `get_authorities` `get_codes` `get_units_map` `get_ellps_map` `get_prime_meridians_map` `get_proj_operations_map` `database.get_database_metadata` `set_use_global_context` `datadir.set_data_dir` `datadir.get_data_dir` `sync.get_transform_grid_list` `sync.get_proj_endpoint`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `always_xy=True` forces lon/lat argument order regardless of authority-declared axis order; set it at construction or PROJ silently swaps axes.
- One `Transformer` per CRS pair is reusable across every call; per-point reconstruction is the naive form.
- Operation selection is data-driven: `area_of_interest`, `accuracy`, `allow_ballpark`, and `only_best` bias which `TransformerGroup` candidate PROJ applies, and `get_last_used_operation()` reports the one used.
- `transform` accepts scalars, sequences, or NumPy arrays and returns the same shape; `inplace=True` mutates the inputs and `radians=True` switches angular units.
- `to_epsg` returns `None` below `min_confidence`, so an arbitrary WKT CRS carries no guaranteed EPSG code.
- `TransformerGroup.download_grids()` promotes a grid-missing candidate before transforming, and `set_use_global_context(True)` shares one PROJ context across threads at boundary init.

[STACKING]:
- `geopandas`(`.api/geopandas.md`): `GeoDataFrame.crs` holds a `pyproj.CRS` and `.to_crs(CRS)` owns the `Transformer`; pass an EPSG int, `CRS`, or WKT string, never reproject by hand.
- `shapely`(`.api/shapely.md`): geometry is CRS-free, so `Transformer.transform` reprojects the coordinate arrays feeding `shapely.ops.transform`; `Geod.geometry_area_perimeter(geom)` gives ellipsoidal area beside planar `shapely.area`.
- `rasterio`(`.api/rasterio.md`): raster CRS round-trips through `CRS.from_wkt` / `to_wkt`.
- `pyogrio`(`.api/pyogrio.md`): `read_dataframe` returns CRS as WKT that `CRS.from_user_input` ingests.
- `pyarrow`(`.api/pyarrow.md`): `CRS.to_json_dict()` is the projjson that STAC/GeoParquet/GeoArrow metadata carries.
- `rioxarray`(`.api/rioxarray.md`): `CRS.from_cf` / `to_cf` / `cs_to_cf` round-trip the NetCDF/Zarr `grid_mapping` attributes without a hand-rolled attribute map.
- within-lib: the data geospatial owner builds `Transformer.from_crs(src, dst, always_xy=True)` once at boundary scope and drives the vectorized numpy transform across the `VectorOp`/`RasterOp` axes.

[LOCAL_ADMISSION]:
- Admit `pyproj` as the canonical CRS and coordinate-transformation owner on the data geospatial rail, composed by geopandas/shapely/rasterio/pyogrio rather than re-parsed per library.

[RAIL_LAW]:
- Package: `pyproj`
- Owns: CRS construction and interchange, axis-aware coordinate transformation, geodesic and ellipsoidal computation, transformer-group enumeration, PROJ database queries, CDN grid network control
- Accept: `Transformer.from_crs` with `always_xy` explicit and reused, `CRS` interchange via `to_epsg`/`to_wkt`/`to_json`, `Geod` for ellipsoidal distance and area, `area_of_interest` for operation selection
- Reject: per-point transformer reconstruction, implicit axis-order assumptions, planar-distance approximation where geodesic distance is required, and hand-rolled datum-shift math when PROJ grids exist
