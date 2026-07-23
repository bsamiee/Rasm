# [PY_DATA_API_GEOARROW_RUST_CORE]

`geoarrow-rust-core` owns the GeoArrow-native geometry memory model for the geospatial-ingress rail: immutable Arrow-backed carriers exposing the Arrow PyCapsule interface, so a geometry crosses the path once as a capsule and never round-trips through a Shapely scalar. Encoded-bytes and external-container construction feed the same carriers the `geoarrow-rust-compute` kernels consume and return, and `get_crs` reads the GeoArrow field CRS as a `pyproj.CRS`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geoarrow-rust-core`
- package: `geoarrow-rust-core`
- import: `geoarrow.rust.core`
- owner: `data`
- rail: geospatial-ingress
- entry points: import-only, no console script; namespace-packaged under `geoarrow.rust`
- capability: GeoArrow-native geometry memory — immutable carriers over the Arrow PyCapsule interface, encoded-bytes (`EWKB`/`WKB`/`WKT`) and external-container (Shapely/GeoPandas) construction, WKB/Shapely/GeoPandas egress, OGR ingest through `read_pyogrio`, geometry-column extraction, and field-CRS retrieval, with `CoordType`/`Dimension` layout selection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry memory carriers

| [INDEX] | [SYMBOL]               | [ROLE]                                                                                               |
| :-----: | :--------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `Geometry`             | immutable single-geometry scalar; `__geo_interface__`, `__arrow_c_array__`, `_repr_svg_`             |
|  [02]   | `GeometryArray`        | immutable Arrow geometry array; `__getitem__ -> Geometry`, `__len__`, `.type`, `from_arrow`          |
|  [03]   | `ChunkedGeometryArray` | immutable chunked geometry array; `chunk(i)`, `chunks()`, `num_chunks()`, `.type`, `from_arrow`      |
|  [04]   | `GeometryType`         | GeoArrow extension-type carrier; `__arrow_c_schema__`, `.coord_type`, `.dimension`, type-string ctor |

[PUBLIC_TYPE_SCOPE]: carrier members

`from_arrow` adopts existing Arrow data and `from_arrow_pycapsule` raw capsules; `GeometryType` constructs from one type string (`point`, `linestring`, `polygon`, `multipoint`, `multilinestring`, `multipolygon`, `geometry`, `geometrycollection`, `wkb`, `box`) with a `Dimension` and `CoordType`, `wkb` taking neither and `box` a dimension only.

| [INDEX] | [SYMBOL]                             | [CALL_SHAPE]                                     | [CAPABILITY]                              |
| :-----: | :----------------------------------- | :----------------------------------------------- | :---------------------------------------- |
|  [01]   | `GeometryArray.from_arrow`           | `from_arrow(data: ArrowArrayExportable) -> Self` | adopt an existing Arrow array zero-copy   |
|  [02]   | `GeometryArray.from_arrow_pycapsule` | `from_arrow_pycapsule(schema, array) -> Self`    | adopt raw schema and array capsules       |
|  [03]   | `ChunkedGeometryArray.chunk`         | `chunk(i: int) -> GeometryArray`                 | access one underlying chunk               |
|  [04]   | `ChunkedGeometryArray.chunks`        | `chunks() -> list[GeometryArray]`                | list single-chunked arrays                |
|  [05]   | `ChunkedGeometryArray.num_chunks`    | `num_chunks() -> int`                            | chunk count                               |
|  [06]   | `GeometryType.coord_type`            | `coord_type -> CoordType \| None`                | coordinate layout of the geometry type    |
|  [07]   | `GeometryType.dimension`             | `dimension -> Dimension \| None`                 | coordinate dimension of the geometry type |

[PUBLIC_TYPE_SCOPE]: layout vocabulary

`CoordType` and `Dimension` are `StrEnum` members (`enums`) paired with lowercase `Literal` aliases (`CoordTypeT`, `DimensionT` in `types`) accepted interchangeably; `DimensionT` also admits the uppercase `XY`/`XYZ` spellings.

| [INDEX] | [SYMBOL]    | [MEMBERS]                                                                                |
| :-----: | :---------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `CoordType` | `Interleaved` single `XYXY` buffer, `Separated` one buffer per dimension (`XXXX`/`YYYY`) |
|  [02]   | `Dimension` | `XY` two-dimensional, `XYZ` three-dimensional                                            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: encoded-to-GeoArrow construction

`from_wkb`/`from_wkt` return a `ChunkedGeometryArray` on an `ArrowStreamExportable` input and a `GeometryArray` otherwise, selecting layout through `coord_type` (default `Interleaved`).

| [INDEX] | [SURFACE]        | [CALL_SHAPE]                                        | [CAPABILITY]                                        |
| :-----: | :--------------- | :-------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `from_ewkb`      | `from_ewkb(input) -> GeometryArray`                 | parse an Arrow binary array from EWKB               |
|  [02]   | `from_wkb`       | `from_wkb(input, *, coord_type=Interleaved)`        | parse ISO WKB to a GeoArrow array/chunked array     |
|  [03]   | `from_wkt`       | `from_wkt(input, *, coord_type=Interleaved)`        | parse WKT strings to a GeoArrow array/chunked array |
|  [04]   | `from_shapely`   | `from_shapely(input, *, crs=None) -> GeometryArray` | build from a Shapely/GeoSeries object array         |
|  [05]   | `from_geopandas` | `from_geopandas(input: GeoDataFrame) -> Table`      | build a GeoArrow Table from a GeoDataFrame          |

[ENTRYPOINT_SCOPE]: GeoArrow-to-encoded egress and column access

| [INDEX] | [SURFACE]      | [CALL_SHAPE]                                                                  | [CAPABILITY]                   |
| :-----: | :------------- | :---------------------------------------------------------------------------- | :----------------------------- |
|  [01]   | `geometry_col` | `geometry_col(input) -> GeometryArray \| ChunkedGeometryArray`                | column of a Table/RecordBatch  |
|  [02]   | `to_wkb`       | `to_wkb(input) -> GeometryArray \| ChunkedGeometryArray`                      | encode to ISO WKB              |
|  [03]   | `to_shapely`   | `to_shapely(input) -> NDArray[np.object_]`                                    | numpy array of Shapely objects |
|  [04]   | `to_geopandas` | `to_geopandas(input: ArrowStreamExportable) -> GeoDataFrame`                  | lower to a GeoDataFrame        |
|  [05]   | `read_pyogrio` | `read_pyogrio(path_or_buffer, /, layer, bbox, sql, ...) -> RecordBatchReader` | stream an OGR source to Arrow  |
|  [06]   | `get_crs`      | `get_crs(data, /, column=None) -> pyproj.CRS \| None`                         | field CRS as a pyproj CRS      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- carrier axis: `GeometryArray`/`ChunkedGeometryArray`/`Geometry`/`GeometryType` are the sole geometry memory carriers; each exposes the Arrow PyCapsule interface, so any capsule consumer reads them zero-copy without a materialized Shapely scalar.
- construction axis: `from_wkb`/`from_wkt`/`from_ewkb` own the encoded-bytes entry and `from_shapely`/`from_geopandas` the external-container entry; `coord_type` selects `Interleaved` versus `Separated` at construction, and one polymorphic entry per source discriminates array from chunked on input shape, never a per-shape function family.
- egress axis: `to_wkb` stays GeoArrow-native; `to_shapely`/`to_geopandas` cross to the Shapely/GeoPandas boundary, admitted only where a consumer requires those objects.
- crs axis: `get_crs` reads the CRS from GeoArrow field metadata through `pyproj`, inferring the single geometry column or taking an explicit `column`, never re-derived from coordinates.
- evidence: each call captures operation name, input encoding, selected `CoordType`, geometry-type, and chunk count as an ingress receipt.

[STACKING]:
- `geoarrow-rust-compute`(`.api/geoarrow-rust-compute.md`): its kernels consume these carriers' capsules and its geometry-returning operations yield the same `GeometryArray`/`ChunkedGeometryArray` carriers back, so construction here and compute there share one memory model with no intermediate copy — core owns the carriers, compute owns the algorithms.
- `geoarrow-rust-io`(`.api/geoarrow-rust-io.md`): its readers yield GeoArrow-native `Table` memory whose `geometry_col` extraction is a carrier here, and `to_wkb` output feeds its writers.
- `pyarrow`(`.api/pyarrow.md`)/`arro3-core`(`.api/arro3-core.md`): consume `__arrow_c_array__`/`__arrow_c_stream__`/`__arrow_c_schema__` zero-copy, and `pyproj`(`.api/pyproj.md`) receives the `get_crs` field CRS as a `pyproj.CRS`.

[LOCAL_ADMISSION]:
- import at boundary scope only (`from geoarrow.rust import core`); a module-level import is banned by the manifest import policy.

[RAIL_LAW]:
- Package: `geoarrow-rust-core`
- Owns: the GeoArrow-native geometry memory model — immutable geometry carriers and the `GeometryType` extension carrier over the Arrow PyCapsule interface, encoded-bytes and external-container construction, WKB/Shapely/GeoPandas egress, OGR ingest, geometry-column extraction, and field-CRS retrieval
- Accept: GeoArrow construction and lowering feeding the geospatial-ingress path and the `geoarrow.rust.compute` kernels; Shapely/GeoPandas boundary crossings where a consumer requires them
- Reject: a hand-rolled GeoArrow layout or WKB/WKT parser; a Shapely-scalar bridge where a capsule hand-off exists; a per-source construction family where one `coord_type`-keyed entry discriminates; geometry compute or file IO beyond `read_pyogrio` this package does not own
