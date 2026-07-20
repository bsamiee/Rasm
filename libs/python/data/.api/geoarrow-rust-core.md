# [PY_DATA_API_GEOARROW_RUST_CORE]

`geoarrow-rust-core` supplies the native GeoArrow geometry memory model for the data ingress rail: an immutable Arrow-backed geometry array (`GeometryArray`), its chunked form (`ChunkedGeometryArray`), the single-geometry scalar (`Geometry`), and the geometry extension-type carrier (`GeometryType`), each exposing the Arrow PyCapsule interface for zero-copy hand-off. Encoding functions parse WKB/EWKB/WKT and Shapely/GeoPandas inputs into GeoArrow-native arrays and lower them back out, `read_pyogrio` streams an OGR source to Arrow, and `get_crs` reads the GeoArrow field CRS as a `pyproj.CRS`. Package owner composes these carriers as the typed memory the `geoarrow-rust-compute` kernels consume and return, so a geometry crosses the geospatial-ingress path once as a capsule and never round-trips through a Shapely scalar; it never re-implements the GeoArrow layout the underlying GeoRust crate already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geoarrow-rust-core`
- package: `geoarrow-rust-core`
- import: `geoarrow.rust.core`
- owner: `data`
- rail: geospatial-ingress
- entry points: library use is import-only; no console script; namespace-packaged under `geoarrow.rust`
- capability: GeoArrow-native geometry memory — immutable `GeometryArray`/`ChunkedGeometryArray`/`Geometry`/`GeometryType` carriers over the Arrow PyCapsule interface (`__arrow_c_array__`/`__arrow_c_stream__`/`__arrow_c_schema__`), array construction from EWKB/WKB/WKT (`from_ewkb`, `from_wkb`, `from_wkt`), Shapely/GeoPandas interop both directions (`from_shapely`/`to_shapely`, `from_geopandas`/`to_geopandas`), OGR ingest to an Arrow `RecordBatchReader` (`read_pyogrio`), geometry-column extraction from a Table/RecordBatch (`geometry_col`), WKB egress (`to_wkb`), and CRS retrieval as a `pyproj.CRS` (`get_crs`), with `CoordType`/`Dimension` layout selection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry memory carriers
- rail: geospatial-ingress

Each carrier is immutable and exposes the Arrow PyCapsule interface for zero-copy transfer; `pyarrow.array`/`pyarrow.chunked_array`/`pyarrow.field` consume them without a copy. `GeometryArray` constructs from an `ArrowArrayExportable` and indexes to a `Geometry` scalar; `ChunkedGeometryArray` reads its chunks; both expose their `GeometryType`.

| [INDEX] | [SYMBOL]               | [ROLE]                                                                                               |
| :-----: | :--------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `Geometry`             | immutable single-geometry scalar; `__geo_interface__`, `__arrow_c_array__`, `_repr_svg_`             |
|  [02]   | `GeometryArray`        | immutable Arrow geometry array; `__getitem__ -> Geometry`, `__len__`, `.type`, `from_arrow`          |
|  [03]   | `ChunkedGeometryArray` | immutable chunked geometry array; `chunk(i)`, `chunks()`, `num_chunks()`, `.type`, `from_arrow`      |
|  [04]   | `GeometryType`         | GeoArrow extension-type carrier; `__arrow_c_schema__`, `.coord_type`, `.dimension`, type-string ctor |

[PUBLIC_TYPE_SCOPE]: carrier members
- rail: geospatial-ingress

`from_arrow` adopts existing Arrow data and `from_arrow_pycapsule` adopts raw capsules; `GeometryType` constructs from one of the type strings (`point`, `linestring`, `polygon`, `multipoint`, `multilinestring`, `multipolygon`, `geometry`, `geometrycollection`, `wkb`, `box`) with a `Dimension` and `CoordType`, `wkb` taking neither and `box` taking a dimension only.

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
- rail: geospatial-ingress

`CoordType` and `Dimension` are `StrEnum` members (`enums` module) paired with lowercase `Literal` aliases (`CoordTypeT`, `DimensionT` in `types.py`) accepted interchangeably; `DimensionT` additionally admits the uppercase `"XY"`/`"XYZ"` spellings.

| [INDEX] | [SYMBOL]    | [MEMBERS]                                                                                |
| :-----: | :---------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `CoordType` | `Interleaved` single `XYXY` buffer, `Separated` one buffer per dimension (`XXXX`/`YYYY`) |
|  [02]   | `Dimension` | `XY` two-dimensional, `XYZ` three-dimensional                                            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: encoded-to-GeoArrow construction
- rail: geospatial-ingress

Construction consumes an Arrow binary/string array and returns a GeoArrow-native `GeometryArray`; `from_wkb`/`from_wkt` return a `ChunkedGeometryArray` for an `ArrowStreamExportable` input and select the output layout through `coord_type` (default `Interleaved`). `from_shapely` and `from_geopandas` bridge external geometry containers into the same memory model.

| [INDEX] | [SURFACE]        | [CALL_SHAPE]                                        | [CAPABILITY]                                        |
| :-----: | :--------------- | :-------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `from_ewkb`      | `from_ewkb(input) -> GeometryArray`                 | parse an Arrow binary array from EWKB               |
|  [02]   | `from_wkb`       | `from_wkb(input, *, coord_type=Interleaved)`        | parse ISO WKB to a GeoArrow array/chunked array     |
|  [03]   | `from_wkt`       | `from_wkt(input, *, coord_type=Interleaved)`        | parse WKT strings to a GeoArrow array/chunked array |
|  [04]   | `from_shapely`   | `from_shapely(input, *, crs=None) -> GeometryArray` | build from a Shapely/GeoSeries object array         |
|  [05]   | `from_geopandas` | `from_geopandas(input: GeoDataFrame) -> Table`      | build a GeoArrow Table from a GeoDataFrame          |

[ENTRYPOINT_SCOPE]: GeoArrow-to-encoded egress and column access
- rail: geospatial-ingress

Egress lowers a GeoArrow-native array back to a WKB array, a numpy object array of Shapely geometries, or a GeoDataFrame; `geometry_col` extracts the geometry column from a Table/RecordBatch, `read_pyogrio` streams an OGR source, and `get_crs` reads the GeoArrow field CRS.

| [INDEX] | [SURFACE]      | [CALL_SHAPE]                                                                     | [CAPABILITY]                   |
| :-----: | :------------- | :------------------------------------------------------------------------------- | :----------------------------- |
|  [01]   | `geometry_col` | `geometry_col(input) -> GeometryArray \| ChunkedGeometryArray`                   | column of a Table/RecordBatch  |
|  [02]   | `to_wkb`       | `to_wkb(input) -> GeometryArray \| ChunkedGeometryArray`                         | encode to ISO WKB              |
|  [03]   | `to_shapely`   | `to_shapely(input) -> NDArray[np.object_]`                                       | numpy array of Shapely objects |
|  [04]   | `to_geopandas` | `to_geopandas(input: ArrowStreamExportable) -> GeoDataFrame`                     | lower to a GeoDataFrame        |
|  [05]   | `read_pyogrio` | `read_pyogrio(path_or_buffer, /, *, layer, bbox, sql, ...) -> RecordBatchReader` | stream an OGR source to Arrow  |
|  [06]   | `get_crs`      | `get_crs(data, /, column=None) -> pyproj.CRS \| None`                            | field CRS as a pyproj CRS      |

## [04]-[IMPLEMENTATION_LAW]

[GEOSPATIAL_INGRESS_MEMORY]:
- import: `from geoarrow.rust import core` (or `import geoarrow.rust.core`) at boundary scope only; module-level import is banned by the manifest import policy.
- carrier axis: `GeometryArray`/`ChunkedGeometryArray`/`Geometry`/`GeometryType` are the sole geometry memory carriers of the geospatial-ingress path; each exposes the Arrow PyCapsule interface, so any capsule consumer (`geoarrow.rust.compute`, `pyarrow`, `arro3.core`) reads them zero-copy — never materialize a Shapely scalar to bridge an array hand-off.
- construction axis: `from_wkb`/`from_wkt`/`from_ewkb` own the encoded-bytes entry and `from_shapely`/`from_geopandas` own the external-container entry; `coord_type` selects `Interleaved` versus `Separated` layout at construction, never a post-construction re-pack. An `ArrowArrayExportable` input yields a `GeometryArray`, an `ArrowStreamExportable` input a `ChunkedGeometryArray` — one polymorphic entry per source, never a per-shape function family.
- egress axis: `to_wkb`/`to_shapely`/`to_geopandas` are the only lowering rows; `to_wkb` stays GeoArrow-native, `to_shapely`/`to_geopandas` cross to the Shapely/GeoPandas boundary and are admitted only where a consumer requires those objects.
- crs axis: `get_crs` reads the GeoArrow extension metadata CRS through `pyproj`, inferring the single geometry column or taking an explicit `column`; CRS lives in the field metadata and is never re-derived from coordinates.
- compute seam: geometry-returning `geoarrow.rust.compute` operations yield these same `GeometryArray`/`ChunkedGeometryArray` carriers, so construction here and compute there share one memory model with no intermediate copy; `geoarrow.rust.core` owns the carriers and `geoarrow.rust.compute` owns the algorithms.
- evidence: each call captures operation name, input encoding (EWKB/WKB/WKT/Shapely/GeoPandas/OGR), selected `CoordType`, geometry-type, and chunk count as an ingress receipt.

[RAIL_LAW]:
- Package: `geoarrow-rust-core`
- Owns: the GeoArrow-native geometry memory model — immutable `GeometryArray`/`ChunkedGeometryArray`/`Geometry` carriers and the `GeometryType` extension carrier over the Arrow PyCapsule interface, encoded-bytes and external-container construction, WKB/Shapely/GeoPandas egress, OGR ingest to Arrow, geometry-column extraction, and field-CRS retrieval
- Accept: GeoArrow geometry construction and lowering feeding the geospatial-ingress path and the `geoarrow.rust.compute` kernels; boundary crossings to Shapely/GeoPandas objects where a consumer requires them
- Reject: a hand-rolled GeoArrow buffer layout or WKB/WKT parser; a Shapely-scalar bridge where a capsule hand-off exists; a per-source construction family where one `coord_type`-keyed entry discriminates; geometry compute (`geoarrow.rust.compute`) or file IO beyond `read_pyogrio` this package does not own
