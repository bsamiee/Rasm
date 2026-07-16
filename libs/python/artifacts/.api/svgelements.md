# [PY_ARTIFACTS_API_SVGELEMENTS]

`svgelements` supplies the pure-Python SVG geometry, transform, and parse surface for the artifacts figure rail: an `SVG` document root, a `Path` mutable segment sequence over a `PathSegment` vocabulary (`Move`/`Line`/`Close`/`QuadraticBezier`/`CubicBezier`/`Arc`), a spec-faithful affine `Matrix` with `pre_*`/`post_*` compose families, the document-tree node types (`Group`/`Use`/`Text`/`Image`/`Shape`/`Rect`/`Circle`/`Ellipse`/`Polygon`/`Polyline`/`SimpleLine`), and the unit/color/angle/point value objects (`Length`/`Color`/`Angle`/`Point`) that drive SVG ingestion, geometric transform, bounding-box query, and shape composition over the SVG that the `segno`/`great-tables`/`vl-convert` owners already emit. The package owner composes `SVG.parse`, `Path`, and `Matrix` into the figure-composition owner; it never re-implements the SVG path grammar or the affine algebra `svgelements` already owns, and it never rasterizes (rasterization routes to `resvg-py`/`vl-convert`/`pyvips`/`pillow`).

## [01]-[PACKAGE_SURFACE]

- package: `svgelements`
- import: `svgelements`
- owner: `artifacts`
- rail: figure
- installed: `1.9.6`
- entry points: none (library only)
- capability: SVG document parse from path/stream/string, full `PathSegment` algebra (move/line/close/quadratic/cubic/arc with smooth/relative variants), spec-faithful affine transform with pre/post compose and inverse, length/color/angle/point value objects with unit resolution and color-channel/blend math, the document-tree node vocabulary (group/use/text/image/shape primitives), bounding-box query with stroke option, viewport/viewBox resolution, and `reify` transform-baking

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, path, and transform roots
- rail: figure

| [INDEX] | [TYPE]    | [KIND]        | [ROLE]                                                                        |
| :-----: | :-------- | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `SVG`     | document root | parse/build an SVG tree; `elements(conditional=)` iterate, resolve viewport   |
|  [02]   | `Path`    | segment seq   | `Shape` + `MutableSequence` of `PathSegment`; `d()` serialize, `*` transform  |
|  [03]   | `Matrix`  | affine        | spec-faithful 2D affine; `pre_*`/`post_*` compose, `inverse`, apply by `*`    |
|  [04]   | `Point`   | coordinate    | 2D point with complex-number compatibility; distance/angle/transform/reflect  |
|  [05]   | `Length`  | unit value    | CSS/SVG unit (`mm`/`cm`/`in`/`px`/`pt`/`pc`/`em`/`%`) with `value(...)`       |
|  [06]   | `Color`   | color value   | SVG/CSS color parse, channel access, blend/over/distance math                 |
|  [07]   | `Angle`   | angle value   | CSS angle (`deg`/`rad`/`grad`/`turn`); `as_degrees`/`as_radians`/`normalized` |
|  [08]   | `Viewbox` | viewport      | viewBox + preserveAspectRatio -> a fitting `Matrix`                           |

[PUBLIC_TYPE_SCOPE]: path-segment vocabulary
- rail: figure

`Path` is a `MutableSequence` of `PathSegment`; the segment subclasses are the bounded grammar. `Linear` (`Line`/`Close`) and `Curve` (`QuadraticBezier`/`CubicBezier`/`Arc`) are the two algebraic families; a figure transform applies to the segment, never to a re-parsed `d` string.

| [INDEX] | [TYPE]            | [BASE]      | [ROLE]                                          |
| :-----: | :---------------- | :---------- | :---------------------------------------------- |
|  [01]   | `PathSegment`     | (root)      | abstract segment base; `point`/`bbox`/`reverse` |
|  [02]   | `Move`            | PathSegment | pen move (subpath start)                        |
|  [03]   | `Line`            | Linear      | straight segment                                |
|  [04]   | `Close`           | Linear      | close-subpath segment                           |
|  [05]   | `QuadraticBezier` | Curve       | quadratic Bezier                                |
|  [06]   | `CubicBezier`     | Curve       | cubic Bezier                                    |
|  [07]   | `Arc`             | Curve       | elliptical arc                                  |
|  [08]   | `Subpath`         | (view)      | a contiguous-subpath view into a `Path`         |

[PUBLIC_TYPE_SCOPE]: document-tree nodes
- rail: figure

Every drawable node derives `Shape(SVGElement, GraphicObject, Transformable)` (or `SVGElement` for containers); they share `bbox()`, `* matrix` transform, `reify()`, and `values` style access. `Group`/`Use` are the structural containers `SVG.elements()` resolves through.

| [INDEX] | [TYPE]                 | [KIND]     | [ROLE]                                          |
| :-----: | :--------------------- | :--------- | :---------------------------------------------- |
|  [01]   | `Shape`                | base       | transformable drawable base (bbox/reify/`*`)    |
|  [02]   | `Rect`                 | shape      | axis-aligned rectangle                          |
|  [03]   | `Circle`               | shape      | circle by center and radius (`_RoundShape`)     |
|  [04]   | `Ellipse`              | shape      | ellipse by center and two radii (`_RoundShape`) |
|  [05]   | `Polygon`              | shape      | closed point sequence (`_Polyshape`)            |
|  [06]   | `Polyline`             | shape      | open point sequence (`_Polyshape`)              |
|  [07]   | `SimpleLine`           | shape      | single line segment                             |
|  [08]   | `Group`                | container  | `<g>` transform/style container                 |
|  [09]   | `Use`                  | container  | `<use>` instanced reference                     |
|  [10]   | `Text` / `SVGText`     | text node  | text element with font/anchor style             |
|  [11]   | `Image` / `SVGImage`   | image      | embedded/linked raster reference                |
|  [12]   | `Pattern` / `ClipPath` | paint/clip | pattern fill and clip-path container            |
|  [13]   | `Desc` / `Title`       | metadata   | `<desc>`/`<title>` annotation nodes             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse and document iterate
- rail: figure

`SVG.parse(source, reify=True, ppi=96.0, width=None, height=None, color='black', transform=None, context=None, parse_display_none=False, on_error='ignore')` is the single polymorphic ingestion factory across filename/file-object/`str` source; `reify=True` bakes transforms into element geometry so a downstream consumer reads absolute coordinates. `elements(conditional=)` is the single selection surface — the optional predicate discriminates which resolved nodes the iterator yields, so there is no `select`/`find`/`filter` family.

| [INDEX] | [MEMBER]                         | [KIND]  | [ROLE]                                                  |
| :-----: | :------------------------------- | :------ | :------------------------------------------------------ |
|  [01]   | `SVG.parse(source, ...)`         | parse   | parse SVG from filename/file/`str` into a document tree |
|  [02]   | `SVG.elements(conditional=None)` | iterate | iterate resolved nodes; optional predicate filters      |
|  [03]   | `SVG.select(conditional=None)`   | iterate | iterate raw (pre-resolution) elements                   |
|  [04]   | `SVG.reify()`                    | bake    | bake the document transform into element geometry       |

[ENTRYPOINT_SCOPE]: `Path` construct, build, serialize, query
- rail: figure

`Path("M0,0 …")` parses an SVG `d` string; the builder rows (`move`/`line`/`cubic`/`quad`/`arc`/`horizontal`/`vertical`/`smooth_*`/`closed`) append segments fluently, each accepting `relative=`; `d()`/`bbox()`/`length()`/`point()` query and serialize. The path is one mutable owner — figure composition reads bounds, transforms by `*`, and re-serializes through the same `Path`, never a re-parsed string. The arc/bezier-approximation rows flatten curves for a downstream consumer that only accepts polylines or cubics.

| [INDEX] | [MEMBER]                                                     | [KIND]    | [ROLE]                                                     |
| :-----: | :----------------------------------------------------------- | :-------- | :--------------------------------------------------------- |
|  [01]   | `Path(*args, **kwargs)` / `Path.parse(d)`                    | construct | build from `d` string, segment list, or empty              |
|  [02]   | `Path.d(relative=None, transformed=True, smooth=None)`       | serialize | emit the SVG `d` path-data string                          |
|  [03]   | `Path.svg_d(...)`                                            | serialize | the cached `d` alias                                       |
|  [04]   | `Path.bbox(transformed=True, with_stroke=False)`             | query     | bounding box `(xmin, ymin, xmax, ymax)`                    |
|  [05]   | `Path.point(position, error=1e-12)`                          | query     | point at parametric position `0..1`                        |
|  [06]   | `Path.npoint(positions, error=1e-12)`                        | query     | vectorized points at multiple positions                    |
|  [07]   | `Path.length(error=1e-12, min_depth=5)`                      | query     | arc length of the path                                     |
|  [08]   | `Path.segments(transformed=True)`                            | iterate   | the `PathSegment` list (optionally transformed)            |
|  [09]   | `Path.as_subpaths()` / `subpath(index)` / `count_subpaths()` | iterate   | split into / address `Subpath` views                       |
|  [10]   | `Path.as_points()`                                           | iterate   | the segment endpoint `Point` sequence                      |
|  [11]   | `Path.reify()`                                               | bake      | bake the path's own transform into segment coordinates     |
|  [12]   | `Path.move/line/cubic/quad/arc/horizontal/vertical`          | build     | fluent `(*points, relative=False)` segment-append builders |
|  [13]   | `Path.smooth_cubic/smooth_quad`                              | build     | fluent smooth-segment builders (each takes `relative=`)    |
|  [14]   | `Path.closed(relative=False)` / `Path.direct_close()`        | build     | append a `Close` (`closed` honors the relative flag)       |
|  [15]   | `Path.approximate_arcs_with_cubics(error=0.1)`               | flatten   | replace each `Arc` with cubic Beziers                      |
|  [16]   | `Path.approximate_arcs_with_quads(error=0.1)`                | flatten   | replace each `Arc` with quad Beziers                       |
|  [17]   | `Path.approximate_bezier_with_circular_arcs(error=0.01)`     | flatten   | replace cubics with circular arcs (toolpath egress)        |
|  [18]   | `Path.first_point/current_point/start/end/is_degenerate()`   | state     | pen-state and degeneracy for incremental building          |
|  [19]   | `Path.reverse()`                                             | transform | reverse segment order                                      |

[ENTRYPOINT_SCOPE]: `Matrix` construct, compose, apply
- rail: figure

`Matrix` is the one affine owner. The bare `scale`/`translate`/`rotate`/`skew` factories build a matrix; `pre_*`/`post_*` rows compose onto an existing matrix in the requested order; `inverse` and `transform_point`/`transform_vector` apply. A shape or path transforms by `element * matrix` (returns the same node type).

| [INDEX] | [MEMBER]                                                          | [KIND]    | [ROLE]                                     |
| :-----: | :---------------------------------------------------------------- | :-------- | :----------------------------------------- |
|  [01]   | `Matrix(*components)` / `Matrix.parse(transform_str)`             | construct | affine from `(a, b, c, d, e, f)` or string |
|  [02]   | `Matrix.scale(sx=1.0, sy=None)`                                   | factory   | uniform/non-uniform scale matrix           |
|  [03]   | `Matrix.translate(tx=0.0, ty=0.0)`                                | factory   | translation matrix                         |
|  [04]   | `Matrix.rotate(angle=0.0)`                                        | factory   | rotation matrix (radians)                  |
|  [05]   | `Matrix.skew(angle_a=0.0, angle_b=0.0)` / `skew_x` / `skew_y`     | factory   | skew matrices                              |
|  [06]   | `Matrix.pre_scale/pre_translate/pre_rotate/pre_skew/pre_cat`      | compose   | left-compose onto this matrix              |
|  [07]   | `Matrix.post_scale/post_translate/post_rotate/post_skew/post_cat` | compose   | right-compose onto this matrix             |
|  [08]   | `Matrix.inverse()`                                                | invert    | inverse transform                          |
|  [09]   | `Matrix.transform_point(v)` / `transform_vector(v)`               | apply     | apply to a point/vector                    |
|  [10]   | `Matrix.point_in_matrix_space(v)`                                 | apply     | map a point into matrix space              |
|  [11]   | `Matrix.is_identity()` / `determinant`                            | query     | identity test / determinant                |

[ENTRYPOINT_SCOPE]: value objects
- rail: figure

`Length`/`Color`/`Angle`/`Point` are spec-faithful value objects; each owns its parse and resolution so figure egress never hand-multiplies a float or hand-parses a color string. `Length(value).value(ppi=None, relative_length=None, font_size=None, font_height=None, viewbox=None)` resolves to absolute px; `to_mm()`/`to_cm()`/`to_inch()` return a converted `Length` whose `.amount` carries the numeric value and `.units` the token (`value_in_units` is a property, not a unit-taking call); the static `Color.parse(color_string)` and `Color.parse_color_hex/hsl/rgb/rgbp/lookup` parse a literal; `Color.over(c1, c2)`/`distance(c1, c2)` are static and `instance.blend(other, opacity=None)` bound; `Point.polar_to(angle, dist)` builds by polar coordinates.

| [INDEX] | [MEMBER]                                                                 | [KIND]  | [ROLE]                                         |
| :-----: | :----------------------------------------------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `Length(value).value(...)`                                               | resolve | resolve a length to absolute px                |
|  [02]   | `Length.to_mm/to_cm/to_inch/in_pixels` / `.amount` / `.units`            | convert | absolute-unit conversions returning a `Length` |
|  [03]   | `Color(*args)` / `Color.parse` / `Color.parse_color_*`                   | parse   | color literal -> `Color` or int (static)       |
|  [04]   | `Color.red/green/blue/alpha/hex/hexa/hue/saturation/lightness/luminance` | channel | channel + derived-property read/write access   |
|  [05]   | `Color.rgb/rgba/hsl` / `Color.over(c1, c2)` / `distance(c1, c2)`         | math    | construct, alpha-over composite, distance      |
|  [06]   | `Angle.parse(value)` / `normalized()`                                    | angle   | CSS-angle parse and normalize                  |
|  [07]   | `Angle.as_degrees/as_radians/as_turns/as_gradians`                       | angle   | unit projection                                |
|  [08]   | `Point(x, y)` / `distance_to/angle_to/reflected_across`                  | point   | construct, distance, angle, reflect            |
|  [09]   | `Point.matrix_transform(m)` / `polar_to(angle, dist)`                    | point   | matrix transform / polar build                 |

## [04]-[IMPLEMENTATION_LAW]

- import: `import svgelements` at boundary scope only; the distribution and import name are both `svgelements`; the version constant is `SVGELEMENTS_VERSION`, not `__version__`.
- parse axis: `SVG.parse` is the single ingestion factory across filename/stream/string; `reify=True` resolves transforms into element geometry so a downstream consumer reads absolute coordinates; `ppi`/`width`/`height` seed the viewport for unit resolution and `on_error` controls malformed-input policy (`'ignore'`/`'raise'`), never a per-source parser type.
- iterate axis: `elements(conditional=)` is the single polymorphic selection surface — a predicate discriminates which resolved nodes the iterator yields; there is no `find`/`select_by_tag`/`filter` family, the predicate carries the discrimination.
- transform axis: `Matrix` is the one affine owner; `scale`/`translate`/`rotate`/`skew` are bare factory rows, `pre_*`/`post_*` compose in the requested order, and a shape or path transforms by `element * matrix` (returns the same node type), never a hand-rolled coordinate-transform helper.
- path axis: `Path` is the single mutable `MutableSequence` of `PathSegment`; the segment vocabulary (`Move`/`Line`/`Close`/`QuadraticBezier`/`CubicBezier`/`Arc`) is the bounded grammar, fluent `move`/`line`/`cubic`/`quad`/`arc`/`smooth_*`/`closed` builders (each taking `relative=`) append, and `d()`/`bbox()`/`length()`/`point()` query and serialize through one owner, never a re-parsed path string. The arc-flattening rows (`approximate_arcs_with_cubics`/`approximate_arcs_with_quads`, `approximate_bezier_with_circular_arcs`) are the native pipeline for a consumer that cannot render arcs/cubics (a toolpath or polyline egress) — flatten through the owner, never re-sample the `d` string.
- node axis: the document tree is `Shape(SVGElement, GraphicObject, Transformable)` subclasses plus `Group`/`Use` containers; figure composition reads each node's `bbox()` and transforms by `*`, so n-up/scale-to-fit/crop operates on typed nodes, never on raw XML.
- unit axis: `Length`/`Color`/`Angle`/`Point` are spec-faithful value objects; `Length.value(ppi=...)` resolves to absolute px for document egress and `Color.parse`/channel access answers the color question, never a raw float multiply or string slice.
- query axis: `bbox(transformed=, with_stroke=)` on shapes and paths and `elements()` iteration answer the layout-and-bounds question that figure composition (n-up, scale-to-fit, crop) needs, never a re-implemented SVG geometry engine.
- evidence: each figure op captures element count, source/target viewport (viewBox), applied transform (the composed `Matrix`), resolved bbox, and output byte length as a figure receipt.
- boundary: svgelements owns SVG geometry, transform, parse, and bounds; rasterization to PNG/PDF routes to `resvg-py`/`vl-convert`/`pyvips`/`pillow`; QR/table/chart SVG sources arrive from `segno`/`great-tables`/`vl-convert`; live UI stays outside this package.

[STACKING]:
- `SVG.parse(segno_qr.svg_inline())` ingests a `segno` QR SVG into a typed tree, then `Matrix.scale(...) * shape` + `bbox()` composes it into an n-up sheet alongside a `vl-convert` chart SVG and a `great-tables` table SVG — one figure rail consumes every admitted SVG producer.
- After composition, the assembled SVG string feeds `resvg-py`/`vl-convert` for PNG/PDF rasterization, so svgelements is the geometry/layout owner and the raster owner is downstream, never re-implemented here.
- `Length.value(ppi=, viewbox=)` resolves CSS units against the same viewport the document target declares (e.g. a `weasyprint`/`pymupdf` page box), so scale-to-fit math is spec-correct rather than a hard-coded 96-dpi assumption.
- `Path.bbox()`/`SVG.elements(conditional=)` produce typed geometry a `msgspec`/`pydantic` figure receipt model records (element count, bbox tuple, transform components), so the figure op's evidence is structured, not stringly.

## [05]-[LOCAL_ADMISSION]

- Package: `svgelements`
- Owns: SVG document parse, the full `PathSegment` algebra, spec-faithful affine transform with pre/post compose and inverse, length/color/angle/point value objects, the document-tree node vocabulary, bounding-box query, and viewport/viewBox resolution
- Accept: SVG figure ingestion, transform, scale-to-fit, n-up composition, crop, and bounds query feeding the document and figure owners over the SVG that the `segno`/`great-tables`/`vl-convert` owners emit
- Reject: a hand-rolled SVG path parser or affine helper where `Path`/`Matrix` exist; a raster operation where `resvg-py`/`pyvips`/`pillow` covers it; a second SVG renderer where `vl-convert` rasterizes; a `find`/`select`/`filter` node-query family where `elements(conditional=)` discriminates; identity minting the runtime owns
