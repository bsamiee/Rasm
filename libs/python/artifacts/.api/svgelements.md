# [PY_ARTIFACTS_API_SVGELEMENTS]

`svgelements` supplies the pure-Python SVG geometry, transform, and parse surface for the artifacts figure rail: an `SVG` document root, a `Path` mutable segment sequence, a spec-faithful affine `Matrix`, and the unit/color/angle value objects (`Length`, `Color`, `Angle`, `Point`) that drive SVG ingestion, geometric transform, bounding-box query, and shape composition over the SVG that the chart/QR/nanoplot owners already emit. The package owner composes `SVG.parse`, `Path`, and `Matrix` into the figure-composition owner; it never re-implements the SVG path grammar or the affine algebra `svgelements` already owns, and it never rasterizes (rasterization routes to `vl-convert`/`pillow`).

## [01]-[PACKAGE_SURFACE]

- package: `svgelements`
- import: `svgelements`
- owner: `artifacts`
- rail: figure
- asset: pure-Python runtime library (no native build; `py3-none-any` wheel)
- installed: `1.9.6` reflected on cp315 (pure-Python, imports on the cp315 core)
- entry points: none (library only)
- capability: SVG document parse from path/stream/string, path-segment algebra, spec-faithful affine transform, length/color/angle unit value objects, point geometry, shape primitives (`Rect`/`Circle`/`Ellipse`/`Polygon`/`Polyline`/`SimpleLine`), bounding-box query, viewport/viewBox resolution

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, path, and transform roots

- rail: figure

| [INDEX] | TYPE     | KIND          | ROLE                                                                |
| :-----: | -------- | ------------- | ------------------------------------------------------------------- |
|  [01]   | `SVG`    | document root | parse/build an SVG tree; iterate elements, resolve viewport/viewBox |
|  [02]   | `Path`   | segment seq   | mutable ordered path segments; `d()` serialize, `*` transform       |
|  [03]   | `Matrix` | affine        | spec-faithful 2D affine transform; compose, invert, apply           |
|  [04]   | `Point`  | coordinate    | 2D point with complex-number compatibility                          |
|  [05]   | `Length` | unit value    | CSS/SVG unit (`mm`/`cm`/`in`/`px`/`pt`/`pc`/`%`) with `value(...)`  |
|  [06]   | `Color`  | color value   | SVG/CSS color parse and channel access                              |
|  [07]   | `Angle`  | angle value   | CSS angle (`deg`/`rad`/`grad`/`turn`)                               |

- rail: figure

| [INDEX] | TYPE         | KIND  | ROLE                            |
| :-----: | ------------ | ----- | ------------------------------- |
|  [01]   | `Rect`       | shape | axis-aligned rectangle          |
|  [02]   | `Circle`     | shape | circle by center and radius     |
|  [03]   | `Ellipse`    | shape | ellipse by center and two radii |
|  [04]   | `Polygon`    | shape | closed point sequence           |
|  [05]   | `Polyline`   | shape | open point sequence             |
|  [06]   | `SimpleLine` | shape | single line segment             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse, transform, and query

- rail: figure

Parse rows take path/stream/string sources; the `Matrix`/`Path` rows compose transforms; the geometry rows query bounds.

| [INDEX] | MEMBER                               | KIND      | ROLE                                                       |
| :-----: | ------------------------------------ | --------- | ---------------------------------------------------------- |
|  [01]   | `SVG.parse(source, reify=True, ...)` | parse     | parse SVG from filename/file/`str` into a document tree    |
|  [02]   | `SVG.elements()`                     | iterate   | iterate resolved shape/path elements of the document       |
|  [03]   | `Path(d)`                            | construct | build a path from SVG `d` path-data string                 |
|  [04]   | `Path.d()`                           | serialize | emit the SVG `d` path-data string                          |
|  [05]   | `Path.bbox()`                        | query     | bounding box `(xmin, ymin, xmax, ymax)` of the path        |
|  [06]   | `Matrix(*components)`                | construct | build affine from `(a, b, c, d, e, f)` or transform string |
|  [07]   | `Matrix.scale(sx, sy)`               | factory   | uniform/non-uniform scale matrix                           |
|  [08]   | `Matrix.translate(tx, ty)`           | factory   | translation matrix                                         |
|  [09]   | `Matrix.rotate(angle)`               | factory   | rotation matrix                                            |
|  [10]   | `Length(value).value(ppi, ...)`      | resolve   | resolve a length to absolute units                         |
|  [11]   | `Color(value)`                       | parse     | parse an SVG/CSS color literal                             |

## [04]-[IMPLEMENTATION_LAW]

- import: `import svgelements` at boundary scope only; the distribution and import name are both `svgelements`.
- parse axis: `SVG.parse` is the single ingestion factory across filename/stream/string; `reify=True` resolves transforms into element geometry so a downstream consumer reads absolute coordinates, never a per-source parser type.
- transform axis: `Matrix` is the one affine owner; `scale`/`translate`/`rotate` are factory rows and matrices compose by `*`, never a hand-rolled coordinate-transform helper. A shape or path transforms by `element * matrix`.
- path axis: `Path` is the single mutable segment sequence; `d()` serializes and `bbox()` queries, so figure composition reads bounds and re-serializes through one owner, never a re-parsed path string.
- unit axis: `Length`/`Color`/`Angle`/`Point` are spec-faithful value objects; `Length.value(ppi=...)` resolves to absolute units for the document egress, never a raw float multiply.
- query axis: `bbox()` on shapes and paths and `SVG.elements()` iteration answer the layout-and-bounds question that figure composition (n-up, scale-to-fit, crop) needs, never a re-implemented SVG geometry engine.
- evidence: each figure op captures element count, source/target viewport, applied transform, and output byte length as a figure receipt.
- boundary: svgelements owns SVG geometry, transform, parse, and bounds; rasterization to PNG/PDF routes to `vl-convert`/`pillow`; QR/nanoplot SVG sources arrive from `segno`/`great-tables`; chart SVG arrives from `vl-convert`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

- Package: `svgelements`
- Owns: SVG document parse, path-segment algebra, spec-faithful affine transform, length/color/angle unit value objects, shape primitives, and bounding-box query
- Accept: SVG figure ingestion, transform, scale-to-fit, n-up composition, and bounds query feeding the document and figure owners over the SVG that the chart/QR/nanoplot owners emit
- Reject: a hand-rolled SVG path parser or affine helper where `Path`/`Matrix` exist; a raster operation where `pillow` covers it; a second SVG renderer where `vl-convert` rasterizes; identity minting the runtime owns
