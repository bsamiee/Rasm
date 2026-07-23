# [PY_DATA_API_GEOARROW_RUST_COMPUTE]

`geoarrow-rust-compute` owns the native GeoArrow geometry-compute surface for the geospatial-ingress rail: vectorized GeoRust algorithms compiled to a static Rust package operating directly over Arrow-backed geometry arrays through the Arrow PyCapsule interface. Every operation consumes and returns Arrow capsules in-process, so a geometry crosses the ingress path once as a capsule and never round-trips through a Shapely/GEOS scalar loop; the package owns the algorithm kernels and never re-implements them.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geoarrow-rust-compute`
- package: `geoarrow-rust-compute`
- import: `geoarrow.rust.compute`
- owner: `data`
- rail: geospatial-ingress
- entry points: import-only; namespace-packaged under `geoarrow.rust`; no console script
- capability: vectorized GeoRust compute over Arrow GeoArrow arrays and chunked arrays — measurement, construction, morphology, affine transforms, linear referencing, and broadcast predicates, each metric selecting its method through one keyword enum row

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: result carriers

Geometry results carry `geoarrow.rust.core` `GeometryArray`/`ChunkedGeometryArray`; scalar results carry `arro3.core` `Array`/`ChunkedArray`, and `explode` yields a `Table`; chunking mirrors the input.

| [INDEX] | [SYMBOL]               | [ROLE]                                      |
| :-----: | :--------------------- | :------------------------------------------ |
|  [01]   | `GeometryArray`        | geometry-typed Arrow array                  |
|  [02]   | `ChunkedGeometryArray` | chunked geometry-typed Arrow array          |
|  [03]   | `Array`                | scalar/primitive Arrow array result         |
|  [04]   | `ChunkedArray`         | chunked scalar/primitive Arrow array result |
|  [05]   | `Table`                | Arrow table result for `explode`            |

[PUBLIC_TYPE_SCOPE]: method-selection enums

Each is a `StrEnum` paired with a lowercase `Literal` alias (`AreaMethodT`, `LengthMethodT`, `SimplifyMethodT`, `RotateOriginT`) accepted interchangeably; the alias is the member name lowercased.

| [INDEX] | [SYMBOL]         | [MEMBERS]                                                                                |
| :-----: | :--------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `AreaMethod`     | `Euclidean` planar, `Ellipsoidal` Karney geodesic (m²), `Spherical` Chamberlain-Duquette |
|  [02]   | `LengthMethod`   | `Euclidean`, `Ellipsoidal` Karney, `Haversine` (mean radius 6371.088 km), `Vincenty`     |
|  [03]   | `SimplifyMethod` | `RDP` Ramer-Douglas-Peucker, `VW` Visvalingam-Whyatt, `VW_Preserve` topology-preserving  |
|  [04]   | `RotateOrigin`   | `Center` bbox center, `Centroid`                                                         |

[PUBLIC_TYPE_SCOPE]: input protocols and parameter types

| [INDEX] | [SYMBOL]                | [ROLE]                                                                                 |
| :-----: | :---------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `GeoInterfaceProtocol`  | scalar geometry implementing `__geo_interface__` (e.g. a `shapely` geometry)           |
|  [02]   | `NumpyArrayProtocolf64` | object exposing the numpy `__array__` f64 interface                                    |
|  [03]   | `ScalarGeometry`        | `Union[GeoInterfaceProtocol, geoarrow.rust.core.Geometry]` — one geometry input        |
|  [04]   | `AffineTransform`       | `Union[tuple[float, ...]]` — 6- or 9-float affine; pairs with the `affine` library     |
|  [05]   | `BroadcastGeometry`     | `Union[ScalarGeometry, ArrowArrayExportable, ArrowStreamExportable]` broadcast operand |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: measurement and predicate operations

Scalar-returning ops return `Array`/`ChunkedArray` matching input chunking; `total_bounds` collapses to a 4-tuple, and a `method` row selects its metric via the enum or its lowercase alias.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                               | [CAPABILITY]                                     |
| :-----: | :------------------- | :--------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `area`               | `area(input, *, method=Euclidean)`                         | unsigned area per geometry                       |
|  [02]   | `signed_area`        | `signed_area(input, *, method=Euclidean)`                  | signed area (orientation-aware)                  |
|  [03]   | `length`             | `length(input, *, method=Euclidean)`                       | line length per metric                           |
|  [04]   | `geodesic_perimeter` | `geodesic_perimeter(input)`                                | ellipsoidal perimeter in meters                  |
|  [05]   | `is_empty`           | `is_empty(input)`                                          | boolean empty test per geometry                  |
|  [06]   | `frechet_distance`   | `frechet_distance(input, other)`                           | Fréchet distance between LineStrings             |
|  [07]   | `line_locate_point`  | `line_locate_point(input, point)`                          | fractional location of nearest point on line     |
|  [08]   | `total_bounds`       | `total_bounds(input) -> tuple[float, float, float, float]` | extent `(xmin, ymin, xmax, ymax)` over all geoms |

[ENTRYPOINT_SCOPE]: geometry-constructing and morphology operations

Geometry-returning ops return `GeometryArray`/`ChunkedGeometryArray` matching input chunking; `explode` consumes a stream and returns a `Table`.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                              | [CAPABILITY]                                  |
| :-----: | :----------------------- | :---------------------------------------- | :-------------------------------------------- |
|  [01]   | `center`                 | `center(input)`                           | bounding-box center point                     |
|  [02]   | `centroid`               | `centroid(input)`                         | arithmetic-mean centroid point                |
|  [03]   | `convex_hull`            | `convex_hull(input)`                      | QuickHull convex hull polygon                 |
|  [04]   | `envelope`               | `envelope(input)`                         | axis-aligned bounding-box polygon             |
|  [05]   | `polylabel`              | `polylabel(input, tolerance)`             | pole-of-inaccessibility label point           |
|  [06]   | `simplify`               | `simplify(input, epsilon, *, method=RDP)` | tolerance simplification                      |
|  [07]   | `densify`                | `densify(input, max_distance)`            | interpolate coordinates at max spacing        |
|  [08]   | `chaikin_smoothing`      | `chaikin_smoothing(input, n_iterations)`  | Chaikin corner-cutting smoothing              |
|  [09]   | `line_interpolate_point` | `line_interpolate_point(input, fraction)` | point at fractional distance along line       |
|  [10]   | `explode`                | `explode(input) -> Table`                 | split multi-part geometries into one row each |

[ENTRYPOINT_SCOPE]: affine transformations

Affine ops return `GeometryArray`/`ChunkedGeometryArray`; `affine_transform` applies a full matrix and `rotate`/`scale`/`skew`/`translate` are named-component rows over the same kernel.

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                         | [CAPABILITY]                                     |
| :-----: | :----------------- | :----------------------------------- | :----------------------------------------------- |
|  [01]   | `affine_transform` | `affine_transform(input, transform)` | apply full affine matrix                         |
|  [02]   | `rotate`           | `rotate(geom, angle, *, origin)`     | rotate by angle about origin                     |
|  [03]   | `scale`            | `scale(geom, xfact, yfact)`          | scale by per-axis factors                        |
|  [04]   | `skew`             | `skew(geom, xs, ys)`                 | skew from bounding-box center by per-axis angles |
|  [05]   | `translate`        | `translate(geom, xoff, yoff)`        | translate by `(xoff, yoff)` offsets              |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every op consumes the Arrow PyCapsule interface: an `ArrowArrayExportable` input selects the `GeometryArray`/`Array` overload, an `ArrowStreamExportable` input the `ChunkedGeometryArray`/`ChunkedArray` overload, so any capsule producer feeds compute zero-copy and chunking mirrors the input; `total_bounds` collapses to a 4-tuple and `explode` to a `Table`.
- `import geoarrow.rust.compute` binds at boundary scope; module-level import violates the manifest import policy.
- One `method` keyword row discriminates each metric — `area`/`signed_area` on `AreaMethod`, `length` on `LengthMethod`, `simplify` on `SimplifyMethod`, `rotate` on `RotateOrigin` — taking the enum or its lowercase alias; `affine_transform` owns the full matrix and `rotate`/`scale`/`skew`/`translate` fold the same kernel.
- `frechet_distance(input, other)` and `line_locate_point(input, point)` broadcast one scalar geometry through `__geo_interface__` or `geoarrow.rust.core.Geometry` against the array — the sole non-Arrow admission, a native broadcast rather than a Python scalar loop.
- `line_interpolate_point` matches `shapely.line_interpolate_point` at `normalized=True`: fraction in `[0,1]`, out-of-range clamps to the endpoints, a non-finite fraction or coordinate yields `POINT EMPTY`; `line_locate_point` is its inverse.
- Each call captures operation name, selected method, input geometry-type and chunk count, and output carrier kind as an ingress receipt.

[STACKING]:
- `geoarrow-rust-core`(`.api/geoarrow-rust-core.md`): consumes and returns its `GeometryArray`/`ChunkedGeometryArray` carriers over one shared memory model with no intermediate copy; `frechet_distance`/`line_locate_point` take its `Geometry` scalar as a broadcast operand.
- `arro3-core`(`.api/arro3-core.md`): scalar-returning ops emit its `Array`/`ChunkedArray` and `explode` its `Table`.
- geospatial `VectorOp` path: folds every op into the geospatial-ingress compute leg, capturing the ingress receipt per call.

[LOCAL_ADMISSION]:
- Admitted for Arrow-capsule geometry compute on the geospatial-ingress path; geometry-array construction routes to `geoarrow.rust.core` and file IO to `geoarrow.rust.io`.

[RAIL_LAW]:
- Package: `geoarrow-rust-compute`
- Owns: vectorized GeoRust geometry compute over Arrow GeoArrow arrays and chunked arrays — measurement, construction, morphology, affine transforms with `affine`/`shapely`-equivalent semantics, linear referencing, broadcast predicates, and method-selected metrics; emits `arro3.core` carriers for scalar and table results
- Accept: Arrow-capsule geometry compute feeding the geospatial-ingress and downstream geometry owners; scalar `__geo_interface__`/`Geometry` broadcast operands for `frechet_distance`/`line_locate_point`
- Reject: a wrapper-rename of a compute function; a hand-rolled QuickHull/RDP/VW/Chaikin/polylabel/Fréchet kernel; a Shapely/GEOS scalar loop where a vectorized Arrow op exists; a parallel per-metric function family where one `method` keyword row discriminates; geometry-array construction (`geoarrow.rust.core`) or file IO (`geoarrow.rust.io`) this package does not own
