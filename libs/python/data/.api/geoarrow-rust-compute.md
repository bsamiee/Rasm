# [PY_DATA_API_GEOARROW_RUST_COMPUTE]

`geoarrow-rust-compute` supplies the native GeoArrow geometry-compute surface for the data ingress rail: vectorized GeoRust algorithms compiled to a static Rust package that operate directly over Arrow-backed geometry arrays through the Arrow PyCapsule interface. The package owner composes the array-level operations (`area`, `centroid`, `convex_hull`, `simplify`, `envelope`, affine `rotate`/`scale`/`skew`/`translate`, `total_bounds`) and the table-level `explode` into the `GEOSPATIAL_INGRESS_DEEPEN` compute path; it removes any Shapely/GEOS scalar loop because every operation consumes and returns Arrow capsules in-process, and it never re-implements the GeoRust algorithm kernels the package already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geoarrow-rust-compute`
- package: `geoarrow-rust-compute`
- import: `geoarrow.rust.compute`
- owner: `data`
- rail: geospatial-ingress
- installed: `0.6.3`
- entry points: library use is import-only; no console script; namespace-packaged under `geoarrow.rust`; `__version__` resolves via the native `___version()` export
- capability: vectorized geometry compute over Arrow GeoArrow arrays and chunked arrays — measurement (`area`, `signed_area`, `length`, `geodesic_perimeter`), construction (`center`, `centroid`, `convex_hull`, `envelope`, `polylabel`), morphology (`simplify`, `densify`, `chaikin_smoothing`, `explode`), affine transforms (`affine_transform`, `rotate`, `scale`, `skew`, `translate`), linear referencing (`line_interpolate_point`, `line_locate_point`), predicates (`is_empty`, `frechet_distance`, `total_bounds`), with method selection via `AreaMethod`/`LengthMethod`/`SimplifyMethod`/`RotateOrigin` and broadcast/scalar interop with shapely `__geo_interface__` and the `affine` library

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: result carriers, method enums, and parameter aliases
- rail: geospatial-ingress

Geometry-returning operations yield `GeoArray` or `GeoChunkedArray`; scalar-returning operations yield `Array` or `ChunkedArray`; `explode` returns a `Table`. The scalar/primitive carriers (`Array`/`ChunkedArray`/`Table`) are owned by `arro3.core` (the lightweight Arrow runtime); the geometry carriers (`GeoArray`/`GeoChunkedArray`/`Geometry`) are owned by `geoarrow.rust.core` and re-consumed here. Method enums are `StrEnum` members (`enums.py`); each pairs with a lowercase `Literal` alias (`types.py`) accepted interchangeably at call sites.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                                                                                                                        |
| :-----: | :---------------------- | :-------------- | :---------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `GeoArray`              | result carrier  | geometry-typed Arrow array (from `geoarrow.rust.core`)                                                                        |
|  [02]   | `GeoChunkedArray`       | result carrier  | chunked geometry-typed Arrow array (from `geoarrow.rust.core`)                                                                |
|  [03]   | `Array`                 | result carrier  | scalar/primitive Arrow array result (from `arro3.core`)                                                                       |
|  [04]   | `ChunkedArray`          | result carrier  | chunked scalar/primitive Arrow array result (from `arro3.core`)                                                               |
|  [05]   | `Table`                 | result carrier  | Arrow table result for `explode` (from `arro3.core`)                                                                          |
|  [06]   | `AreaMethod`            | method enum     | `StrEnum`: `Euclidean` (planar), `Ellipsoidal` (Karney geodesic, meter²), `Spherical` (Chamberlain-Duquette)                  |
|  [07]   | `LengthMethod`          | method enum     | `StrEnum`: `Euclidean`, `Ellipsoidal` (Karney), `Haversine` (mean radius 6371.088 km), `Vincenty`                             |
|  [08]   | `SimplifyMethod`        | method enum     | `StrEnum`: `RDP` (Ramer-Douglas-Peucker), `VW` (Visvalingam-Whyatt), `VW_Preserve` (topology-preserving VW)                   |
|  [09]   | `RotateOrigin`          | method enum     | `StrEnum`: `Center` (bbox center), `Centroid`                                                                                 |
|  [10]   | `AreaMethodT`           | parameter alias | `Literal['ellipsoidal', 'euclidean', 'spherical']`                                                                            |
|  [11]   | `LengthMethodT`         | parameter alias | `Literal['ellipsoidal', 'euclidean', 'haversine', 'vincenty']`                                                                |
|  [12]   | `SimplifyMethodT`       | parameter alias | `Literal['rdp', 'vw', 'vw_preserve']`                                                                                         |
|  [13]   | `RotateOriginT`         | parameter alias | `Literal['center', 'centroid']`                                                                                               |
|  [14]   | `GeoInterfaceProtocol`  | input protocol  | scalar geometry implementing `__geo_interface__` (e.g. a `shapely` geometry)                                                  |
|  [15]   | `NumpyArrayProtocolf64` | input protocol  | object exposing the numpy `__array__` f64 interface                                                                           |
|  [16]   | `ScalarGeometry`        | parameter alias | `Union[GeoInterfaceProtocol, geoarrow.rust.core.Geometry]` — a single geometry input                                          |
|  [17]   | `AffineTransform`       | parameter type  | `Union[tuple[6 floats], tuple[9 floats], tuple[float, ...]]` — a 6- or 9-element affine; integrates with the `affine` library |
|  [18]   | `BroadcastGeometry`     | parameter type  | `Union[ScalarGeometry, ArrowArrayExportable, ArrowStreamExportable]` — scalar-or-array geometry broadcast against `input`     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: measurement and predicate operations
- rail: geospatial-ingress

Scalar-returning operations consume one geometry array (`ArrowArrayExportable | ArrowStreamExportable`) and return `Array | ChunkedArray` matching the input chunking. Method-bearing rows select the metric via the enum or its lowercase `Literal` alias.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                                                                                      | [CAPABILITY]                                          |
| :-----: | :------------------- | :-------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `area`               | `area(input, *, method: AreaMethod \| AreaMethodT = Euclidean) -> Array \| ChunkedArray`                                          | unsigned area per geometry                            |
|  [02]   | `signed_area`        | `signed_area(input, *, method: AreaMethod \| AreaMethodT = Euclidean) -> Array \| ChunkedArray`                                   | signed area (orientation-aware)                       |
|  [03]   | `length`             | `length(input, *, method: LengthMethod \| LengthMethodT = Euclidean) -> Array \| ChunkedArray`                                    | line length per metric                                |
|  [04]   | `geodesic_perimeter` | `geodesic_perimeter(input) -> Array \| ChunkedArray`                                                                              | ellipsoidal perimeter in meters                       |
|  [05]   | `is_empty`           | `is_empty(input) -> Array \| ChunkedArray`                                                                                        | boolean empty test per geometry                       |
|  [06]   | `frechet_distance`   | `frechet_distance(input, other: BroadcastGeometry) -> Array \| ChunkedArray`                                                      | Fréchet distance between LineStrings                  |
|  [07]   | `line_locate_point`  | `line_locate_point(input, point: GeoInterfaceProtocol \| ArrowArrayExportable \| ArrowStreamExportable) -> Array \| ChunkedArray` | fractional location of nearest point on line          |
|  [08]   | `total_bounds`       | `total_bounds(input) -> Tuple[float, float, float, float]`                                                                        | extent `(xmin, ymin, xmax, ymax)` over all geometries |

[ENTRYPOINT_SCOPE]: geometry-constructing and morphology operations
- rail: geospatial-ingress

Geometry-returning operations consume one geometry array and return `GeoArray | GeoChunkedArray` matching the input chunking; `explode` consumes a stream and returns a `Table`.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                                                                                            | [CAPABILITY]                                  |
| :-----: | :----------------------- | :-------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `center`                 | `center(input) -> GeoArray \| GeoChunkedArray`                                                                                          | bounding-box center point                     |
|  [02]   | `centroid`               | `centroid(input) -> GeoArray \| GeoChunkedArray`                                                                                        | arithmetic-mean centroid point                |
|  [03]   | `convex_hull`            | `convex_hull(input) -> GeoArray \| GeoChunkedArray`                                                                                     | QuickHull convex hull polygon                 |
|  [04]   | `envelope`               | `envelope(input) -> GeoArray \| GeoChunkedArray`                                                                                        | axis-aligned bounding-box polygon             |
|  [05]   | `polylabel`              | `polylabel(input, tolerance: float) -> GeoArray \| GeoChunkedArray`                                                                     | pole-of-inaccessibility label point           |
|  [06]   | `simplify`               | `simplify(input, epsilon: float, *, method: SimplifyMethod \| SimplifyMethodT = RDP) -> GeoArray \| GeoChunkedArray`                    | tolerance simplification (RDP/VW/VW_Preserve) |
|  [07]   | `densify`                | `densify(input, max_distance: float) -> GeoArray \| GeoChunkedArray`                                                                    | interpolate coordinates at max spacing        |
|  [08]   | `chaikin_smoothing`      | `chaikin_smoothing(input, n_iterations: int) -> GeoArray \| GeoChunkedArray`                                                            | Chaikin corner-cutting smoothing              |
|  [09]   | `line_interpolate_point` | `line_interpolate_point(input, fraction: float \| int \| ArrowArrayExportable \| NumpyArrayProtocolf64) -> GeoArray \| GeoChunkedArray` | point at fractional distance along line       |
|  [10]   | `explode`                | `explode(input: ArrowStreamExportable) -> Table`                                                                                        | split multi-part geometries into one row each |

[ENTRYPOINT_SCOPE]: affine transformations
- rail: geospatial-ingress

Affine operations consume a geometry array (`geom`) and return `GeoArray | GeoChunkedArray`. `affine_transform` applies a full matrix; `rotate`/`scale`/`skew`/`translate` are the named-component rows. `rotate` selects its pivot via `RotateOrigin`, its lowercase alias, or an explicit `(x, y)` tuple.

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                                                                                                                 | [CAPABILITY]                                     |
| :-----: | :----------------- | :--------------------------------------------------------------------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `affine_transform` | `affine_transform(input, transform: AffineTransform) -> GeoArray \| GeoChunkedArray`                                         | apply full affine matrix                         |
|  [02]   | `rotate`           | `rotate(geom, angle: float, *, origin: RotateOrigin \| RotateOriginT \| tuple[float, float]) -> GeoArray \| GeoChunkedArray` | rotate by angle about origin                     |
|  [03]   | `scale`            | `scale(geom, xfact: float, yfact: float) -> GeoArray \| GeoChunkedArray`                                                     | scale by per-axis factors                        |
|  [04]   | `skew`             | `skew(geom, xs: float, ys: float) -> GeoArray \| GeoChunkedArray`                                                            | skew from bounding-box center by per-axis angles |
|  [05]   | `translate`        | `translate(geom, xoff: float, yoff: float) -> GeoArray \| GeoChunkedArray`                                                   | translate by `(xoff, yoff)` offsets              |

## [04]-[IMPLEMENTATION_LAW]

[GEOSPATIAL_INGRESS_COMPUTE]:
- import: `from geoarrow.rust import compute` (or `import geoarrow.rust.compute`) at boundary scope only; module-level import is banned by the manifest import policy.
- input axis: every operation accepts the Arrow PyCapsule interface (`ArrowArrayExportable | ArrowStreamExportable`), so any Arrow-backed geometry array — built by `geoarrow.rust.core` or any capsule producer — feeds compute with zero copy; an `ArrowArrayExportable` input selects the `GeoArray`/`Array` overload, an `ArrowStreamExportable` input selects the `GeoChunkedArray`/`ChunkedArray` overload. Never materialize Shapely scalars to bridge an array call.
- scalar/broadcast axis: `frechet_distance(input, other: BroadcastGeometry)` and `line_locate_point(input, point)` accept a single scalar geometry through `__geo_interface__` (a `shapely` geometry) or `geoarrow.rust.core.Geometry`, broadcast against the array input — the only place a non-Arrow scalar is admitted, and it is the native broadcast, not a Python scalar loop.
- result axis: geometry-returning operations yield `GeoArray`/`GeoChunkedArray` and scalar-returning operations yield `Array`/`ChunkedArray`; chunking mirrors the input. `total_bounds` collapses to a 4-tuple `(xmin, ymin, xmax, ymax)` and `explode` to a `Table` — these are the only non-array result rows.
- method axis: `area`/`signed_area` select `AreaMethod` (default `Euclidean`), `length` selects `LengthMethod` (default `Euclidean`), `simplify` selects `SimplifyMethod` (default `RDP`), `rotate` selects `RotateOrigin`; each is one keyword row taking the enum or its lowercase `Literal` alias, never a parallel per-metric function.
- affine axis: `affine_transform(input, transform: AffineTransform)` owns the full matrix path — `transform` is a 6- or 9-float tuple equivalent to `shapely.affinity.affine_transform` and integrates directly with the `affine` library; `rotate`/`scale`/`skew`/`translate` are named-component rows over the same kernel (`rotate` pivots on `RotateOrigin`/`(x,y)`, `skew` distorts from the bbox center), never a hand-rolled coordinate loop.
- linear-referencing axis: `line_interpolate_point(input, fraction)` is equivalent to `shapely.line_interpolate_point` with `normalized=True` (fraction in [0,1], out-of-range clamps to endpoints, NaN/non-finite -> `POINT EMPTY`); `fraction` broadcasts as a Python scalar, a numpy f64 array, or an Arrow array. `line_locate_point` is the inverse, returning the normalized fractional location of the nearest point.
- evidence: each call captures operation name, selected method, input geometry-type and chunk count, and output carrier kind (`GeoArray`/`Array`/`Table`) as an ingress receipt.

[RAIL_LAW]:
- Package: `geoarrow-rust-compute`
- Owns: vectorized GeoRust geometry compute over Arrow GeoArrow arrays/chunked arrays — measurement, construction, morphology, affine transforms (with `affine`/`shapely`-equivalent semantics), linear referencing, broadcast predicates, and method-selected metrics; emits `arro3.core` carriers for scalar/table results
- Accept: Arrow-capsule geometry compute feeding the geospatial-ingress, data, and downstream geometry owners; scalar `__geo_interface__`/`Geometry` broadcast operands for `frechet_distance`/`line_locate_point`
- Reject: wrapper-renames of the compute functions; a hand-rolled QuickHull/RDP/VW/Chaikin/polylabel/Fréchet kernel; a Shapely/GEOS scalar loop where a vectorized Arrow operation exists; a parallel per-metric function family where one `method` keyword row discriminates; geometry-array construction (`geoarrow.rust.core`) or file IO (`geoarrow.rust.io`) this package does not own
