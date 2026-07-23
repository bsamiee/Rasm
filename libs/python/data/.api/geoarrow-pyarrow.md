# [PY_DATA_API_GEOARROW_PYARROW]

`geoarrow-pyarrow` registers GeoArrow geometry as a first-class pyarrow `ExtensionType`/`ExtensionArray`, so every pyarrow-native rail — Parquet, DuckDB, Delta/Lance, the pandas/GeoPandas boundary — carries geometry as a typed column decoded by extension name, never re-parsed from a WKB `binary` column. It is the pyarrow-registration face of the GeoArrow family, binding the WKB/WKT codecs and coordinate kernels the shared `geoarrow-c`/`geoarrow-types` core owns rather than re-implementing them.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geoarrow-pyarrow`
- package: `geoarrow-pyarrow`
- import: `geoarrow.pyarrow`
- owner: `data`
- rail: geospatial-ingress
- entry points: import-only, no console script; namespace-packaged under `geoarrow`
- capability: pyarrow-native GeoArrow — extension-type registration, the `GeometryExtensionType`/`GeometryExtensionArray` carrier hierarchy over typed Arrow storage, polymorphic construction and re-encoding across Shapely/GeoPandas/WKB/WKT/Arrow, CRS/edge/layout refinement, aggregate and coordinate ops, and the pandas `ExtensionDtype`/GeoPandas boundary over the `CoordType`/`Dimensions`/`EdgeType`/`Encoding`/`GeometryType` vocabulary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pyarrow extension type and array

`GeometryExtensionType` subclasses `pyarrow.ExtensionType`, so a registered geometry column is a native pyarrow `DataType` that Parquet, IPC, and the C Data Interface round-trip by extension name; `GeometryExtensionArray` subclasses `pyarrow.ExtensionArray` as the storage-backed carrier crossing to and from the backing Arrow array.

| [INDEX] | [SYMBOL]                 | [ROLE]                                                                                 |
| :-----: | :----------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `GeometryExtensionType`  | `pyarrow.ExtensionType` base; `wrap_array`/`from_geobuffers`/`to_pandas_dtype`/`field` |
|  [02]   | `GeometryExtensionArray` | `pyarrow.ExtensionArray` base; `storage`/`from_storage`/`geobuffers`/`from_pandas`     |

[PUBLIC_TYPE_SCOPE]: per-encoding geometry types

Each subtype is a `GeometryExtensionType` naming one encoding; a zero-arg constructor (`ga.point()`, `ga.wkb()`) yields the unspecified-CRS type and the metadata setters refine it. `WkbType`/`WktType` cover the serialized ISO WKB and WKT encodings including the large and view Arrow storage variants (`large_wkb`, `wkb_view`, `large_wkt`, `wkt_view`).

| [INDEX] | [SYMBOL]              | [ENCODING]                                                       |
| :-----: | :-------------------- | :--------------------------------------------------------------- |
|  [01]   | `PointType`           | GeoArrow-native point                                            |
|  [02]   | `LinestringType`      | GeoArrow-native linestring                                       |
|  [03]   | `PolygonType`         | GeoArrow-native polygon                                          |
|  [04]   | `MultiPointType`      | GeoArrow-native multipoint                                       |
|  [05]   | `MultiLinestringType` | GeoArrow-native multilinestring                                  |
|  [06]   | `MultiPolygonType`    | GeoArrow-native multipolygon                                     |
|  [07]   | `WkbType`             | serialized WKB — `wkb`, `large_wkb`, `wkb_view` storage variants |
|  [08]   | `WktType`             | serialized WKT — `wkt`, `large_wkt`, `wkt_view` storage variants |

[PUBLIC_TYPE_SCOPE]: encoding and layout vocabulary

Vocabulary enums drive type selection and metadata; `OGC_CRS84` is the explicit PROJJSON longitude-latitude CRS value.

| [INDEX] | [SYMBOL]       | [MEMBERS]                                                                                 |
| :-----: | :------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `Encoding`     | `GEOARROW`, `WKB`, `LARGE_WKB`, `WKB_VIEW`, `WKT`, `LARGE_WKT`, `WKT_VIEW`, `UNSPECIFIED` |
|  [02]   | `GeometryType` | `POINT`…`MULTIPOLYGON`, `GEOMETRYCOLLECTION`, `BOX`, `GEOMETRY`, `UNSPECIFIED`            |
|  [03]   | `CoordType`    | `INTERLEAVED` single `XYXY` buffer, `SEPARATED` one buffer per dimension, `UNSPECIFIED`   |
|  [04]   | `Dimensions`   | `XY`, `XYZ`, `XYM`, `XYZM`, `UNKNOWN`, `UNSPECIFIED`                                      |
|  [05]   | `EdgeType`     | `PLANAR`, `SPHERICAL`, `VINCENTY`, `THOMAS`, `ANDOYER`, `KARNEY`, `UNSPECIFIED`           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration and construction

One polymorphic `array` entry builds a `GeometryExtensionArray` from Shapely, GeoPandas, WKB/WKT, or any Arrow-exportable input; `register_extension_types` installs the `geoarrow.*` names once per process so pyarrow round-trips geometry by extension name, and `extension_type` resolves a `TypeSpec` to its concrete type.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                   | [CAPABILITY]                                 |
| :-----: | :--------------------------- | :--------------------------------------------- | :------------------------------------------- |
|  [01]   | `register_extension_types`   | `register_extension_types(lazy=True)`          | install `geoarrow.*` types into the registry |
|  [02]   | `unregister_extension_types` | `unregister_extension_types(lazy=True)`        | remove the registered geometry types         |
|  [03]   | `array`                      | `array(obj, type_=None)`                       | build a geometry array from any input        |
|  [04]   | `as_geoarrow`                | `as_geoarrow(obj, type=None, ...)`             | re-encode to a GeoArrow-native layout        |
|  [05]   | `as_wkb`                     | `as_wkb(obj, strict_iso_wkb=False)`            | re-encode to serialized WKB                  |
|  [06]   | `as_wkt`                     | `as_wkt(obj)`                                  | re-encode to serialized WKT                  |
|  [07]   | `make_point`                 | `make_point(x, y, z=None, m=None, crs=None)`   | build a point array from coordinate arrays   |
|  [08]   | `extension_type`             | `extension_type(spec, storage_type=None, ...)` | resolve a `TypeSpec` to its type             |

[ENTRYPOINT_SCOPE]: metadata, layout conversion, and typed ops

Metadata setters (`with_crs`, `with_edge_type`) replace field attribution without repacking storage; layout setters (`with_coord_type`, `with_dimensions`, `with_geometry_type`) transform the encoded layout or coordinates through the GeoArrow kernel.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                   | [CAPABILITY]                             |
| :-----: | :------------------- | :--------------------------------------------- | :--------------------------------------- |
|  [01]   | `with_crs`           | `with_crs(obj, crs)`                           | set an explicit field CRS                |
|  [02]   | `with_coord_type`    | `with_coord_type(obj, coord_type)`             | set `INTERLEAVED`/`SEPARATED` layout     |
|  [03]   | `with_dimensions`    | `with_dimensions(obj, dimensions)`             | set `XY`/`XYZ`/`XYM`/`XYZM`              |
|  [04]   | `with_edge_type`     | `with_edge_type(obj, edge_type)`               | set planar versus geodesic edges         |
|  [05]   | `with_geometry_type` | `with_geometry_type(obj, geometry_type)`       | set the declared geometry type           |
|  [06]   | `box` / `box_agg`    | `box(obj)` / `box_agg(obj)`                    | per-row and aggregate bounding box       |
|  [07]   | `point_coords`       | `point_coords(obj, dimensions=None)`           | lower a point array to coordinate arrays |
|  [08]   | `format_wkt`         | `format_wkt(obj, precision=None, ...)`         | render bounded-precision WKT strings     |
|  [09]   | `rechunk`            | `rechunk(obj, max_bytes)`                      | re-partition a chunked geometry array    |
|  [10]   | `infer_type_common`  | `infer_type_common(obj, coord_type=None, ...)` | infer the common type over mixed input   |

[ENTRYPOINT_SCOPE]: pandas and GeoPandas boundary

`to_pandas_dtype` yields the pandas `ExtensionDtype` carrying a geometry column through a pandas frame, and `to_geopandas` lowers a geometry array or table to a `GeoDataFrame`; both cross only where a consumer requires the pandas/GeoPandas objects.

| [INDEX] | [SURFACE]         | [CALL_SHAPE]                                       | [CAPABILITY]                                   |
| :-----: | :---------------- | :------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `to_pandas_dtype` | `GeometryExtensionType.to_pandas_dtype() -> dtype` | pandas `ExtensionDtype` for a geometry column  |
|  [02]   | `to_geopandas`    | `to_geopandas(obj) -> GeoDataFrame`                | lower a geometry array/table to a GeoDataFrame |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `register_extension_types` runs once at a process boundary so pyarrow round-trips geometry by extension name across Parquet, IPC, and the C Data Interface; a rail registers before its first `array` call and never re-registers per operation.
- `GeometryExtensionType`/`GeometryExtensionArray` are pyarrow-native subclasses, so a geometry column is a typed pyarrow `Array`/`Field` any consumer reads directly, never a WKB `binary` column re-parsed downstream.
- `array`/`as_geoarrow` own input entry and `as_wkb`/`as_wkt` own serialized re-encoding; one polymorphic `array` entry discriminates on input, and `coord_type` selects `INTERLEAVED` versus `SEPARATED` at construction, never a post-construction re-pack.
- Zero-argument geometry-type constructors carry unspecified CRS; `with_crs(obj, OGC_CRS84)` or an explicit pyproj CRS writes field attribution, and CRS is never derived from coordinates.
- `geoarrow-c`/`geoarrow-types` own the WKB/WKT codecs, coordinate kernels, and type specs; this package binds them to pyarrow and re-implements no codec or layout.
- Each call captures operation name, input encoding, selected `CoordType`, geometry type, CRS presence, and chunk count as an ingress receipt.

[STACKING]:
- `geoarrow-rust-core`(`.api/geoarrow-rust-core.md`): a geometry crosses to the immutable Rust `GeometryArray`/`ChunkedGeometryArray` carriers over the shared Arrow C Data Interface PyCapsule hand-off — one on-wire GeoArrow layout, no intermediate Shapely scalar.
- `geoarrow-rust-compute`(`.api/geoarrow-rust-compute.md`): the compute kernels consume that same capsule layout, so registration here and compute there share one memory model.
- `pyarrow`(`.api/pyarrow.md`): `GeometryExtensionType` is a native pyarrow `DataType`, so a geometry column rides every pyarrow reader — Parquet, DuckDB, Delta/Lance — by extension name.
- data folder: geometry stays a typed extension column across the Parquet, DuckDB, and Delta/Lance planes of the geospatial-ingress rail, decoded by name and never re-parsed.

[LOCAL_ADMISSION]:
- `to_pandas_dtype` and `to_geopandas` are the sole pandas/GeoPandas crossings, admitted only where a consumer requires those objects; a pyarrow-typed geometry column otherwise stays pyarrow-native through the interchange planes.

[RAIL_LAW]:
- Package: `geoarrow-pyarrow`
- Owns: pyarrow-native GeoArrow — extension-type registration, the `GeometryExtensionType`/`GeometryExtensionArray` subclass hierarchy, construction and re-encoding from Shapely/GeoPandas/WKB/WKT/Arrow, CRS/edge attribution, layout conversion, aggregate and coordinate ops, and the pandas `ExtensionDtype`/GeoPandas boundary
- Accept: geometry carried as a typed pyarrow column across the Parquet, DuckDB, Delta/Lance, and pandas/GeoPandas planes; construction and re-encoding feeding the geospatial-ingress path; the Arrow C Data Interface seam to the `geoarrow-rust-*` carriers
- Reject: a WKB `binary` column re-parsed downstream where a registered extension type carries the geometry; a hand-rolled GeoArrow layout or WKB/WKT codec `geoarrow-c` owns; a Shapely-scalar bridge where a capsule or extension-array hand-off exists; geometry compute (`geoarrow-rust-compute`) or file IO (`geoarrow-rust-io`) this package does not own
