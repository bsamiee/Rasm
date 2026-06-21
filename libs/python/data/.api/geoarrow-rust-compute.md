# [PY_DATA_API_GEOARROW_RUST_COMPUTE]

`geoarrow-rust-compute` supplies the native GeoArrow geometry-compute surface for the data ingress rail: vectorized GeoRust algorithms compiled to a static Rust wheel that operate directly over Arrow-backed geometry arrays through the Arrow PyCapsule interface. The package owner composes the array-level operations (`area`, `centroid`, `convex_hull`, `simplify`, `envelope`, affine `rotate`/`scale`/`skew`/`translate`, `total_bounds`) and the table-level `explode` into the `GEOSPATIAL_INGRESS_DEEPEN` compute path; it removes any Shapely/GEOS scalar loop because every operation consumes and returns Arrow capsules in-process, and it never re-implements the GeoRust algorithm kernels the wheel already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geoarrow-rust-compute`
- package: `geoarrow-rust-compute`
- import: `geoarrow.rust.compute`
- owner: `data`
- rail: geospatial-ingress
- installed: `0.6.3` docs-derived (companion <3.15); live reflection pending env provisioning
- entry points: library use is import-only; no console script; namespace-packaged under `geoarrow.rust`
- capability: vectorized geometry compute over Arrow GeoArrow arrays and chunked arrays — measurement (`area`, `signed_area`, `length`, `geodesic_perimeter`), construction (`center`, `centroid`, `convex_hull`, `envelope`, `polylabel`), morphology (`simplify`, `densify`, `chaikin_smoothing`, `explode`), affine transforms (`affine_transform`, `rotate`, `scale`, `skew`, `translate`), linear referencing (`line_interpolate_point`, `line_locate_point`), predicates (`is_empty`, `frechet_distance`, `total_bounds`), with method selection via `AreaMethod`/`LengthMethod`/`SimplifyMethod`/`RotateOrigin`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: result carriers, method enums, and parameter aliases
- rail: geospatial-ingress

Geometry-returning operations yield `GeoArray` or `GeoChunkedArray`; scalar-returning operations yield `Array` or `ChunkedArray`; `explode` returns a `Table`. The array/chunked carriers are owned by `geoarrow.rust.core` and re-consumed here. Method enums are `StrEnum` members; each pairs with a lowercase `Literal` alias accepted interchangeably at call sites.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                                                                                                         |
| :-----: | :---------------------- | :-------------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | `GeoArray`              | result carrier  | geometry-typed Arrow array (from `geoarrow.rust.core`)                                                         |
|  [02]   | `GeoChunkedArray`       | result carrier  | chunked geometry-typed Arrow array (from `geoarrow.rust.core`)                                                 |
|  [03]   | `Array`                 | result carrier  | scalar/primitive Arrow array result                                                                            |
|  [04]   | `ChunkedArray`          | result carrier  | chunked scalar/primitive Arrow array result                                                                    |
|  [05]   | `Table`                 | result carrier  | Arrow table result for `explode`                                                                               |
|  [06]   | `AreaMethod`            | method enum     | `StrEnum`: `Euclidean`, `Ellipsoidal`, `Spherical`                                                             |
|  [07]   | `LengthMethod`          | method enum     | `StrEnum`: `Euclidean`, `Ellipsoidal`, `Haversine`, `Vincenty`                                                 |
|  [08]   | `SimplifyMethod`        | method enum     | `StrEnum`: `RDP`, `VW`, `VW_Preserve`                                                                          |
|  [09]   | `RotateOrigin`          | method enum     | `StrEnum`: `Center`, `Centroid`                                                                                |
|  [10]   | `AreaMethodT`           | parameter alias | `Literal['ellipsoidal', 'euclidean', 'spherical']`                                                             |
|  [11]   | `LengthMethodT`         | parameter alias | `Literal['ellipsoidal', 'euclidean', 'haversine', 'vincenty']`                                                 |
|  [12]   | `SimplifyMethodT`       | parameter alias | `Literal['rdp', 'vw', 'vw_preserve']`                                                                          |
|  [13]   | `RotateOriginT`         | parameter alias | `Literal['center', 'centroid']`                                                                                |
|  [14]   | `GeoInterfaceProtocol`  | input protocol  | scalar geometry implementing `__geo_interface__`                                                               |
|  [15]   | `NumpyArrayProtocolf64` | input protocol  | object exposing the numpy `__array__` f64 interface                                                            |
|  [16]   | `AffineTransform`       | parameter type  | affine matrix input for `affine_transform` (RESEARCH: exact tuple/matrix shape not on a resolvable docs page)  |
|  [17]   | `BroadcastGeometry`     | parameter type  | scalar-or-array geometry input broadcast against `input` (RESEARCH: exact union not on a resolvable docs page) |

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
- input axis: every operation accepts the Arrow PyCapsule interface (`ArrowArrayExportable | ArrowStreamExportable`), so any Arrow-backed geometry array — built by `geoarrow.rust.core` or any capsule producer — feeds compute with zero copy; never materialize Shapely scalars to bridge the call.
- result axis: geometry-returning operations yield `GeoArray`/`GeoChunkedArray` and scalar-returning operations yield `Array`/`ChunkedArray`; chunking mirrors the input. `total_bounds` collapses to a 4-tuple and `explode` to a `Table` — these are the only non-array result rows.
- method axis: `area`/`signed_area` select `AreaMethod`, `length` selects `LengthMethod`, `simplify` selects `SimplifyMethod`, `rotate` selects `RotateOrigin`; each is one keyword row taking the enum or its lowercase `Literal` alias, never a parallel per-metric function.
- affine axis: `affine_transform` owns the full matrix path; `rotate`/`scale`/`skew`/`translate` are named-component rows over the same kernel, never a hand-rolled coordinate loop.
- algorithm axis: the GeoRust kernels (QuickHull hull, RDP/VW simplification, Chaikin smoothing, polylabel pole-of-inaccessibility, Fréchet distance) are owned by the wheel; compose them — never re-derive a Python geometry algorithm the native surface already exposes.
- evidence: each call captures operation name, selected method, input geometry-type and chunk count, and output carrier kind (`GeoArray`/`Array`/`Table`) as an ingress receipt.
- boundary: `geoarrow-rust-compute` owns native geometry compute over Arrow capsules; geometry-array construction and type metadata route to `geoarrow.rust.core`; file IO (GeoParquet/FlatGeobuf/Arrow IPC) routes to `geoarrow.rust.io`; predicate/overlay operations the wheel does not expose route to the GEOS-backed owner, not a local reimplementation.

[RAIL_LAW]:
- Package: `geoarrow-rust-compute`
- Owns: vectorized GeoRust geometry compute over Arrow GeoArrow arrays/chunked arrays — measurement, construction, morphology, affine transforms, linear referencing, predicates, and method-selected metrics
- Accept: Arrow-capsule geometry compute feeding the geospatial-ingress, data, and downstream geometry owners
- Reject: wrapper-renames of the compute functions; a hand-rolled QuickHull/RDP/VW/Chaikin/polylabel/Fréchet kernel; a Shapely/GEOS scalar loop where a vectorized Arrow operation exists; a parallel per-metric function family where one `method` keyword row discriminates; geometry-array construction or file IO this package does not own
