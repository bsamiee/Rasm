# [PY_ARTIFACTS_API_SVGELEMENTS]

`svgelements` owns pure-Python SVG geometry for the figure rail: SVG-document parse, the `PathSegment` path algebra, spec-faithful affine `Matrix` transform, the document-tree node vocabulary, and the `Length`/`Color`/`Angle`/`Point` value objects that resolve units, colors, angles, and coordinates. It parses, transforms, bounds, and composes the SVG that `segno`/`great-tables`/`vl-convert-python` emit; PNG/PDF egress routes downstream to `resvg-py`/`vl-convert-python`/`pyvips`/`pillow`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `svgelements`
- package: `svgelements` (`MIT`)
- module: `svgelements`
- owner: `artifacts`
- rail: figure — the SVG geometry, parse, transform, and bounds owner
- entry points: none (library only)
- capability: SVG parse from filename/stream/`str`; the full `PathSegment` algebra (move/line/close/quadratic/cubic/arc with smooth and relative variants); spec-faithful affine transform with pre/post compose and inverse; `Length`/`Color`/`Angle`/`Point` value objects with unit resolution and color-channel/blend math; the document-tree node vocabulary; bounding-box query with a stroke option; viewport/viewBox resolution; and `reify` transform-baking

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, path, and transform roots

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :-------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `SVG`     | document-root | parse/build the tree; `elements(conditional=)`, resolve viewport   |
|  [02]   | `Path`    | segment-seq   | `Shape` + `MutableSequence` of `PathSegment`; `d()`, `*` transform |
|  [03]   | `Matrix`  | affine        | 2D affine; `pre_*`/`post_*` compose, `inverse`, apply by `*`       |
|  [04]   | `Point`   | coordinate    | 2D point, complex-compatible; distance/angle/transform/reflect     |
|  [05]   | `Length`  | unit-value    | CSS/SVG unit `mm`/`cm`/`in`/`px`/`pt`/`pc`/`em`/`%`; `value(...)`  |
|  [06]   | `Color`   | color-value   | SVG/CSS parse, channel access, blend/over/distance math            |
|  [07]   | `Angle`   | angle-value   | CSS angle `deg`/`rad`/`grad`/`turn`; degree/radian/turn projection |
|  [08]   | `Viewbox` | viewport      | viewBox + preserveAspectRatio to a fitting `Matrix`                |

[PUBLIC_TYPE_SCOPE]: path-segment vocabulary

`Path` sequences `PathSegment`; `Linear` (`Line`/`Close`) and `Curve` (`QuadraticBezier`/`CubicBezier`/`Arc`) are the two algebraic families.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :---------------- | :------------ | :-------------------------------------- |
|  [01]   | `PathSegment`     | segment-base  | abstract base; `point`/`bbox`/`reverse` |
|  [02]   | `Move`            | segment       | pen move (subpath start)                |
|  [03]   | `Line`            | linear        | straight segment                        |
|  [04]   | `Close`           | linear        | close-subpath segment                   |
|  [05]   | `QuadraticBezier` | curve         | quadratic Bezier                        |
|  [06]   | `CubicBezier`     | curve         | cubic Bezier                            |
|  [07]   | `Arc`             | curve         | elliptical arc                          |
|  [08]   | `Subpath`         | segment-view  | contiguous-subpath view into a `Path`   |

[PUBLIC_TYPE_SCOPE]: document-tree nodes

Every drawable node derives `Shape(SVGElement, GraphicObject, Transformable)`; `Group`/`Use` are the containers `SVG.elements()` resolves through.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :----------- | :------------ | :------------------------------------------- |
|  [01]   | `Shape`      | node-base     | transformable drawable base (bbox/reify/`*`) |
|  [02]   | `Rect`       | shape         | axis-aligned rectangle                       |
|  [03]   | `Circle`     | round-shape   | circle by center and radius                  |
|  [04]   | `Ellipse`    | round-shape   | ellipse by center and two radii              |
|  [05]   | `Polygon`    | poly-shape    | closed point sequence                        |
|  [06]   | `Polyline`   | poly-shape    | open point sequence                          |
|  [07]   | `SimpleLine` | shape         | single line segment                          |
|  [08]   | `Group`      | container     | `<g>` transform/style container              |
|  [09]   | `Use`        | container     | `<use>` instanced reference                  |
|  [10]   | `Text`       | text-node     | text element with font/anchor style          |
|  [11]   | `Image`      | image-node    | embedded/linked raster reference             |
|  [12]   | `Pattern`    | paint         | pattern-fill container                       |
|  [13]   | `ClipPath`   | clip          | clip-path container                          |
|  [14]   | `Desc`       | metadata      | `<desc>` annotation node                     |
|  [15]   | `Title`      | metadata      | `<title>` annotation node                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse and document iterate

`SVG.parse` seeds the viewport with `ppi`/`width`/`height` and controls malformed input with `on_error` (`'ignore'`/`'raise'`); `reify=True` bakes transforms so a consumer reads absolute coordinates.

| [INDEX] | [SURFACE]                        | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `SVG.parse(source, ...)`         | static   | parse filename/file/`str` into a document tree    |
|  [02]   | `SVG.elements(conditional=None)` | instance | iterate resolved nodes; predicate filters         |
|  [03]   | `SVG.select(conditional=None)`   | instance | iterate raw pre-resolution elements               |
|  [04]   | `SVG.reify()`                    | instance | bake the document transform into element geometry |

[ENTRYPOINT_SCOPE]: `Path` construct, build, serialize, query

`Path` reads bounds, transforms by `*`, and re-serializes through the same owner, never a re-parsed string; each fluent builder carries `relative=`.

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `Path(*args, **kwargs)` / `Path.parse(d)`                              | ctor     | build from `d` string, segment list, or empty       |
|  [02]   | `Path.d(relative, transformed, smooth)`                                | instance | emit the SVG `d` path-data string                   |
|  [03]   | `Path.svg_d(segments, relative, smooth)`                               | static   | serialize a segment list to a `d` string            |
|  [04]   | `Path.bbox(transformed, with_stroke)`                                  | instance | bounding box `(xmin, ymin, xmax, ymax)`             |
|  [05]   | `Path.point(position, error)`                                          | instance | point at parametric position `0..1`                 |
|  [06]   | `Path.npoint(positions, error)`                                        | instance | vectorized points at many positions                 |
|  [07]   | `Path.length(error, min_depth)`                                        | instance | arc length of the path                              |
|  [08]   | `Path.segments(transformed)`                                           | instance | the `PathSegment` list                              |
|  [09]   | `Path.as_subpaths()` / `subpath(index)` / `count_subpaths()`           | instance | split into / address `Subpath` views                |
|  [10]   | `Path.as_points()`                                                     | instance | the segment endpoint `Point` sequence               |
|  [11]   | `Path.reify()`                                                         | instance | bake the path transform into segment coordinates    |
|  [12]   | `Path.move/line/cubic/quad/arc/horizontal/vertical(*points, relative)` | instance | fluent segment-append builders                      |
|  [13]   | `Path.smooth_cubic/smooth_quad(*points, relative)`                     | instance | fluent smooth-segment builders                      |
|  [14]   | `Path.closed(relative)` / `Path.direct_close()`                        | instance | append a `Close`                                    |
|  [15]   | `Path.approximate_arcs_with_cubics(error)`                             | instance | replace each `Arc` with cubic Beziers               |
|  [16]   | `Path.approximate_arcs_with_quads(error)`                              | instance | replace each `Arc` with quad Beziers                |
|  [17]   | `Path.approximate_bezier_with_circular_arcs(error)`                    | instance | replace cubics with circular arcs (toolpath egress) |
|  [18]   | `Path.first_point/current_point/start/end/is_degenerate()`             | property | pen-state and degeneracy for incremental building   |
|  [19]   | `Path.reverse()`                                                       | instance | reverse segment order                               |

[ENTRYPOINT_SCOPE]: `Matrix` construct, compose, apply

`scale`/`translate`/`rotate`/`skew` build a matrix, `pre_*`/`post_*` compose in the requested order, and a shape or path transforms by `element * matrix` returning the same node type.

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `Matrix(*components)` / `Matrix.parse(transform_str)`             | ctor     | affine from `(a, b, c, d, e, f)` or string |
|  [02]   | `Matrix.scale(sx, sy)`                                            | factory  | uniform/non-uniform scale matrix           |
|  [03]   | `Matrix.translate(tx, ty)`                                        | factory  | translation matrix                         |
|  [04]   | `Matrix.rotate(angle)`                                            | factory  | rotation matrix (radians)                  |
|  [05]   | `Matrix.skew(angle_a, angle_b)` / `skew_x` / `skew_y`             | factory  | skew matrices                              |
|  [06]   | `Matrix.pre_scale/pre_translate/pre_rotate/pre_skew/pre_cat`      | instance | left-compose onto this matrix              |
|  [07]   | `Matrix.post_scale/post_translate/post_rotate/post_skew/post_cat` | instance | right-compose onto this matrix             |
|  [08]   | `Matrix.inverse()`                                                | instance | inverse transform                          |
|  [09]   | `Matrix.transform_point(v)` / `transform_vector(v)`               | instance | apply to a point/vector                    |
|  [10]   | `Matrix.point_in_matrix_space(v)`                                 | instance | map a point into matrix space              |
|  [11]   | `Matrix.is_identity()` / `determinant`                            | instance | identity test / determinant                |

[ENTRYPOINT_SCOPE]: value objects

`Length`/`Color`/`Angle`/`Point` own their parse and resolution; `Length.value` takes `ppi`/`relative_length`/`font_size`/`font_height`/`viewbox`, `value_in_units` is a property (not a unit-taking call), `Color.over`/`distance` are static, and `Color.blend` is bound.

| [INDEX] | [SURFACE]                                                                       | [SHAPE]     | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------------------ | :---------- | :------------------------------------- |
|  [01]   | `Length(value).value(...)`                                                      | instance    | resolve a length to absolute px        |
|  [02]   | `Length.to_mm/to_cm/to_inch/in_pixels`, `.amount`, `.units`                     | instance    | absolute-unit conversion to a `Length` |
|  [03]   | `Color.parse(color_string)` / `Color.parse_color_hex/hsl/rgb/rgbp/lookup`       | static      | color literal to `Color` or int        |
|  [04]   | `Color.red/green/blue/alpha/hex/hexa`                                           | property    | channel read/write                     |
|  [05]   | `Color.hue/saturation/lightness/luminance/rgb/rgba/hsl`                         | property    | derived-property read/write            |
|  [06]   | `Color.over(c1, c2)` / `Color.distance(c1, c2)` / `Color.blend(other, opacity)` | static      | alpha-over composite, distance, blend  |
|  [07]   | `Angle.parse(value)` / `Angle.normalized()`                                     | classmethod | CSS-angle parse and normalize          |
|  [08]   | `Angle.as_degrees/as_radians/as_turns/as_gradians`                              | property    | unit projection                        |
|  [09]   | `Point(x, y)` / `distance_to/angle_to/reflected_across(p)`                      | instance    | construct, distance, angle, reflect    |
|  [10]   | `Point.matrix_transform(m)` / `polar_to(angle, distance)`                       | instance    | matrix transform / polar build         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `SVG.parse` is the single ingestion factory across filename, stream, and `str`; `reify=True` bakes transforms into element geometry so a consumer reads absolute coordinates.
- `SVG.elements(conditional=)` is the single selection surface — a predicate discriminates which resolved nodes yield, foreclosing a `find`/`select_by_tag`/`filter` family.
- `Matrix` is the one affine owner; a shape or path transforms by `element * matrix` returning the same node type, never a hand-rolled coordinate-transform helper.
- `Path` is the one mutable `MutableSequence` of `PathSegment`; the `Move`/`Line`/`Close`/`QuadraticBezier`/`CubicBezier`/`Arc` grammar is bounded and query/serialize run through the owner, never a re-parsed `d` string. `approximate_arcs_with_*`/`approximate_bezier_with_circular_arcs` flatten curves through the same owner for a polyline-or-cubic-only consumer.
- Every drawable node is a `Shape(SVGElement, GraphicObject, Transformable)` subclass or a `Group`/`Use` container; n-up, scale-to-fit, and crop read each node's `bbox()` and transform by `*`, never raw XML.
- `Length`/`Color`/`Angle`/`Point` own unit, color, angle, and coordinate resolution; `Length.value(ppi=)` resolves to absolute px and `Color.parse` answers the color question. `SVGELEMENTS_VERSION` names the version constant.

[STACKING]:
- `segno` (`.api/segno.md`), `great-tables` (`.api/great-tables.md`), and `vl-convert-python` (`.api/vl-convert-python.md`) emit SVG that `SVG.parse` ingests into one typed tree; `Matrix.scale(...) * shape` + `bbox()` then composes each into an n-up sheet, so one figure rail consumes every admitted SVG producer.
- `resvg-py` (`.api/resvg-py.md`) `svg_to_bytes` or `vl-convert-python` rasterizes the composed SVG to PNG/PDF, and that PNG feeds `pillow` (`.api/pillow.md`) or `pyvips` (`.api/pyvips.md`); `svgelements` is the geometry/layout owner upstream of every raster owner.
- `Length.value(ppi=, viewbox=)` resolves CSS units against the same viewport the document target declares — a `weasyprint`/`pymupdf` page box — so scale-to-fit is spec-correct rather than a 96-dpi assumption.
- `Path.bbox()` and `SVG.elements(conditional=)` produce typed geometry a `msgspec`/`pydantic` figure receipt records — element count, bbox tuple, `Matrix` components — so the figure op's evidence is structured.
- `graphic/vector` folds `Path` as its metric substrate (point-at-distance, decimation) and `Matrix` as its transform, materializing one `Shape` per op before the `Rasterize` hand-off.

[LOCAL_ADMISSION]:
- Admitted as the sole SVG geometry, parse, transform, and bounds owner for the figure rail — ingestion, transform, scale-to-fit, n-up composition, crop, and bounds query over the SVG that `segno`/`great-tables`/`vl-convert-python` emit, feeding the document and figure owners; rasterization stays downstream at `resvg-py`/`vl-convert-python`.

[RAIL_LAW]:
- Package: `svgelements`
- Owns: SVG document parse, the full `PathSegment` algebra, spec-faithful affine transform with pre/post compose and inverse, the `Length`/`Color`/`Angle`/`Point` value objects, the document-tree node vocabulary, bounding-box query, and viewport/viewBox resolution
- Accept: SVG figure ingestion, transform, scale-to-fit, n-up composition, crop, and bounds query feeding the document and figure owners over the SVG that `segno`/`great-tables`/`vl-convert-python` emit
- Reject: a hand-rolled SVG path parser or affine helper where `Path`/`Matrix` exist; a raster operation `resvg-py`/`pyvips`/`pillow` owns; a second SVG renderer where `vl-convert-python` rasterizes; a `find`/`select`/`filter` node-query family where `elements(conditional=)` discriminates; identity or receipt minting the runtime owner holds
