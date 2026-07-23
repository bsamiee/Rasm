# [PY_ARTIFACTS_API_SKIA_PATHOPS]

`skia-pathops` mints the planar boolean, offset, and stroke-to-outline geometry the `graphic/vector` rail lacks: an abi3 binding of Skia `SkPathOps`/`SkStroke`/`SkPath` over one mutable `Path` ingesting cubics/quads/conics/arcs, running N-ary set operations, self-intersection repair, and stroke-to-outline fill. A FontTools-pen bridge (`getPen`/`draw`) makes `Path` one geometry spine any pen producer draws into and re-emits from. Its owner composes `op`/`OpBuilder`/`simplify`/`Path.stroke` into the `Vector` arms, never re-implementing set-ops or stroking, and never rasterizing.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `skia-pathops`
- package: `skia-pathops` (BSD-3-Clause)
- import: `pathops`
- owner: `artifacts`
- rail: figure (the `graphic/vector#VECTOR` boolean/offset/outline arms)
- abi: forward-compatible abi3 wheel (`_pathops.abi3.so`) binding the Skia `pathops`/`core` subset
- entry points: none (library only)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `Path` accumulator and the pen/builder owners

Three runtime owners carry the whole concern: `Path` is the one mutable geometry accumulator every operation reads and writes, `PathPen` the pen-protocol adapter that bridges a source outline INTO a `Path`, `OpBuilder` the N-way boolean accumulator. No second path type, no document, no canvas.

| [INDEX] | [TYPE]      | [KIND]              | [ROLE]                                                                              |
| :-----: | :---------- | :------------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `Path`      | mutable accumulator | the one `SkPath` owner; ingest verbs, run boolean/simplify/stroke, draw/query       |
|  [02]   | `PathPen`   | pen adapter         | the pen `Path.getPen()` returns; a source outline draws its segments into this path |
|  [03]   | `OpBuilder` | N-way builder       | accumulate `add(path, operator)` operands, then `resolve()` to one `Path`           |

[PUBLIC_TYPE_SCOPE]: bounded enum vocabulary

`graphic/vector` keys its `VectorOp` arms against these closed enums — a boolean kind selects a `PathOp`, a stroke style `LineCap`/`LineJoin`, a winding policy `FillType` — each a catalogued enum, never a stringly knob.

| [INDEX] | [ENUM]      | [MEMBERS]                                                                | [ROLE]                                          |
| :-----: | :---------- | :----------------------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `PathOp`    | `DIFFERENCE=0` `INTERSECTION=1` `UNION=2` `XOR=3` `REVERSE_DIFFERENCE=4` | boolean selector for `op`/`OpBuilder.add`       |
|  [02]   | `FillType`  | `WINDING=0` `EVEN_ODD=1` `INVERSE_WINDING=2` `INVERSE_EVEN_ODD=3`        | fill rule ("inside" for booleans/area)          |
|  [03]   | `LineCap`   | `BUTT_CAP=0` `ROUND_CAP=1` `SQUARE_CAP=2`                                | open-end cap style for `Path.stroke`            |
|  [04]   | `LineJoin`  | `MITER_JOIN=0` `ROUND_JOIN=1` `BEVEL_JOIN=2`                             | corner join style for `Path.stroke`             |
|  [05]   | `ArcSize`   | `SMALL=0` `LARGE=1`                                                      | arc-sweep selector for `Path.arcTo` (SVG flag)  |
|  [06]   | `Direction` | `CW=0` `CCW=1`                                                           | contour winding (paired with `Path.clockwise`)  |
|  [07]   | `PathVerb`  | `MOVE=0` `LINE=1` `QUAD=2` `CONIC=3` `CUBIC=4` `CLOSE=5`                 | segment grammar `Path.verbs`/`segments` reports |

[PUBLIC_TYPE_SCOPE]: typed faults

`PathOpsError` is the provider root; each leaf names the precise structural cause. `graphic/vector` maps every leaf onto its closed `VectorFault` `@tagged_union` at the incurring arm, never a bare `except Exception`.

| [INDEX] | [TYPE]                 | [BASE]         | [RAISE_SITE]                                                                              |
| :-----: | :--------------------- | :------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `PathOpsError`         | `Exception`    | provider root for every `pathops` failure                                                 |
|  [02]   | `UnsupportedVerbError` | `PathOpsError` | a pen received a verb the `SkPath` grammar cannot represent                               |
|  [03]   | `OpenPathError`        | `PathOpsError` | a closed-path operation (or `draw` with `allow_open_paths=False`) met an unclosed contour |
|  [04]   | `NumberOfPointsError`  | `PathOpsError` | a verb received the wrong control-point count                                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: boolean set operations
- policy carry: `fix_winding`, `keep_starting_points`, `clockwise`

`op(one, two, operator)` is the one binary primitive over all five `PathOp` members; `OpBuilder` folds more than two operands into one `resolve()`. Pen-form wrappers take a contour list with an output pen for a fontTools-shaped caller — `(subject_contours, clip_contours, outpen, …)`, `union` taking `(contours, outpen, …)`; `reverse_difference` lives only in `pathops.operations`, reached at the top level as `op(one, two, PathOp.REVERSE_DIFFERENCE)`.

| [INDEX] | [MEMBER]                                   | [KIND]     | [ROLE]                                                                       |
| :-----: | :----------------------------------------- | :--------- | :--------------------------------------------------------------------------- |
|  [01]   | `op(one, two, operator, …) -> Path`        | binary     | the one binary boolean primitive over all five `PathOp` members              |
|  [02]   | `pathops.union(contours, outpen, …)`       | pen-form   | self-union a contour list into `outpen` (merges overlaps, repairs winding)   |
|  [03]   | `pathops.difference(…)`                    | pen-form   | subject − clip into `outpen`                                                 |
|  [04]   | `pathops.intersection(…)`                  | pen-form   | subject ∩ clip into `outpen`                                                 |
|  [05]   | `pathops.xor(…)`                           | pen-form   | symmetric difference into `outpen`                                           |
|  [06]   | `pathops.operations.reverse_difference(…)` | pen-form   | clip − subject into `outpen` (top-level: `op(…, PathOp.REVERSE_DIFFERENCE)`) |
|  [07]   | `OpBuilder(…)`                             | construct  | N-way boolean accumulator                                                    |
|  [08]   | `OpBuilder.add(path, operator)`            | accumulate | stage one `Path` operand under a `PathOp` (first add seeds the base)         |
|  [09]   | `OpBuilder.resolve() -> Path`              | resolve    | fold every staged operand into one result `Path`                             |

[ENTRYPOINT_SCOPE]: simplify, stroke-to-outline, conic flatten
- policy carry: `fix_winding`, `keep_starting_points`, `clockwise` (on `simplify`/`Path.simplify`)

`Path.stroke` converts an open centerline into a closed filled outline (a true offset for closed input, the offset `svgelements` lacks); `convertConicsToQuads` flattens Skia conic arcs to quads for SVG/quad consumers that have no conic verb.

| [INDEX] | [MEMBER]                                        | [KIND]   | [ROLE]                                                         |
| :-----: | :---------------------------------------------- | :------- | :------------------------------------------------------------- |
|  [01]   | `simplify(path, …) -> Path`                     | function | self-intersection removal + winding repair -> new `Path`       |
|  [02]   | `Path.simplify(…)`                              | in-place | the same canonicalization mutating the path                    |
|  [03]   | `Path.stroke(width, cap, join, miter_limit, …)` | offset   | stroke-to-outline: filled `width`-wide stroke (cap/join/dash)  |
|  [04]   | `Path.convertConicsToQuads(tolerance=0.25)`     | flatten  | replace every conic verb with quads (SVG/quad-consumer egress) |

[ENTRYPOINT_SCOPE]: `Path` build, ingest, draw, compose

`Path` builds three ways — the SkPath cursor methods, the generic `add(verb, *pts)`, or a FontTools pen from `getPen()` that a source outline draws into. `draw(pen)` replays the accumulated geometry into any output pen, so a `Path` round-trips back to `svgelements`/fontTools.

| [INDEX] | [MEMBER]                                                          | [KIND]    | [ROLE]                                                |
| :-----: | :---------------------------------------------------------------- | :-------- | :---------------------------------------------------- |
|  [01]   | `Path(verbs=None, points=None, …)` / `Path()`                     | construct | empty path, or seed from `verbs`/`points` arrays      |
|  [02]   | `Path.getPen(glyphSet=None, allow_open_paths=True) -> PathPen`    | ingest    | FontTools pen a source outline draws into this path   |
|  [03]   | `Path.draw(pen)`                                                  | egress    | replay geometry into any output pen (SVG/glyph)       |
|  [04]   | `Path.moveTo(x, y)` / `lineTo(x, y)`                              | build     | move-to start / straight-segment cursor appends       |
|  [05]   | `Path.quadTo(x1, y1, x2, y2)` / `cubicTo(x1, y1, x2, y2, x3, y3)` | build     | quadratic / cubic cursor appends                      |
|  [06]   | `Path.conicTo(x1, y1, x2, y2, w)`                                 | build     | rational conic append (emits a `CONIC` verb)          |
|  [07]   | `Path.arcTo(rx, ry, xAxisRotate, largeArc, sweep, x, y)`          | build     | SVG-style elliptical arc append (emits `CONIC` verbs) |
|  [08]   | `Path.close()`                                                    | build     | close the current contour                             |
|  [09]   | `Path.add(verb, *pts)`                                            | build     | generic verb append keyed by `PathVerb`               |
|  [10]   | `Path.addPath(path)`                                              | compose   | append every contour of another `Path`                |
|  [11]   | `Path.reset()` / `Path.rewind()`                                  | clear     | empty the path (`rewind` keeps allocated capacity)    |

[ENTRYPOINT_SCOPE]: transform, reverse, geometric query, introspection

`Path.transform` is the full 3×3 affine (`scaleX`, `skewY`, `skewX`, `scaleY`, `translateX`, `translateY`, `perspectiveX`, `perspectiveY`, `perspectiveBias`, identity on the diagonal and bias). Query properties answer layout/hit-test/winding without re-deriving geometry; introspection views are the read side a winding/hole/contour consumer keys per loop.

| [INDEX] | [MEMBER]                                               | [KIND]     | [ROLE]                                                     |
| :-----: | :----------------------------------------------------- | :--------- | :--------------------------------------------------------- |
|  [01]   | `Path.transform(…)`                                    | transform  | apply a 3×3 affine/perspective in place                    |
|  [02]   | `Path.reverse()`                                       | transform  | reverse contour direction (flip winding)                   |
|  [03]   | `Path.area` (property)                                 | query      | signed/absolute enclosed area (sign encodes winding)       |
|  [04]   | `Path.bounds` / `Path.controlPointBounds` (properties) | query      | tight bbox / control-hull bbox `(xmin, ymin, xmax, ymax)`  |
|  [05]   | `Path.contains(pt)`                                    | query      | point-in-path hit test under the fill rule                 |
|  [06]   | `Path.isConvex` / `Path.clockwise` (properties)        | query      | convexity test / dominant winding (settable)               |
|  [07]   | `Path.fillType` (property)                             | policy     | the `FillType` governing inside/area/contains (read/write) |
|  [08]   | `Path.firstPoints` (property)                          | query      | the start point of each contour                            |
|  [09]   | `Path.segments` / `Path.verbs` (properties)            | introspect | `(verb-name, points)` tuples / `PathVerb` list             |
|  [10]   | `Path.points` / `Path.contours` (properties)           | introspect | flat point list / per-contour `Path` views                 |
|  [11]   | `Path.dump(cpp=False, as_hex=False)`                   | debug      | print as SkPath C++ / hex literal (diagnostic, not egress) |

[ENTRYPOINT_SCOPE]: `PathPen` pen protocol + float helpers

`PathPen` is the FontTools `AbstractPen` shape `getPen()` returns; the float helpers expose the IEEE-754 round-trip Skia uses internally, and `decompose_quadratic_segment` splits an over-long quad for a consumer with a degree cap.

| [INDEX] | [MEMBER]                                          | [KIND] | [ROLE]                                                |
| :-----: | :------------------------------------------------ | :----- | :---------------------------------------------------- |
|  [01]   | `PathPen.moveTo(pt)` / `lineTo(pt)`               | pen    | line/move pen appends into the owning `Path`          |
|  [02]   | `PathPen.curveTo(*points)` / `qCurveTo(*points)`  | pen    | cubic / quad pen appends into the owning `Path`       |
|  [03]   | `PathPen.closePath()` / `endPath()`               | pen    | close (filled contour) / end-open (stroke centerline) |
|  [04]   | `PathPen.addComponent(glyphName, transformation)` | pen    | composite-glyph reference (fontTools component verb)  |
|  [05]   | `bits2float(float_as_bits)` / `float2bits(x)`     | helper | IEEE-754 int/float round-trip (exact fidelity)        |
|  [06]   | `decompose_quadratic_segment(points)`             | helper | split a quadratic chain into renderable quad segments |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `import pathops` at boundary scope only, a `lazy import` beside the `svgelements`/`resvg_py` lazies so the abi3 `.so` load stays off the import-time path; the distribution is `skia-pathops`, the import name `pathops`.
- boolean axis: `op(one, two, operator)` keys one boolean `VectorOp` arm to a `PathOp` member, never five sibling methods; `OpBuilder.add ×N` then `resolve()` is the one N-way fold, never a hand-rolled reduce of repeated `op`. Because the rail already holds `Path`s, `op`/`OpBuilder` is the in-rail form and the binary `op(…, PathOp.REVERSE_DIFFERENCE)` sidesteps the operations-module import asymmetry; the pen-form wrappers serve a fontTools contour caller.
- offset axis: `Path.stroke(width, cap, join, miter_limit, dash_array, dash_offset)` is the one fixed-width offset / stroke-to-outline owner, never a hand-tessellated parallel-curve offset; round caps/joins emit conics, so follow with `convertConicsToQuads(tolerance)` before an SVG/quad pen.
- canonicalize axis: `simplify`/`Path.simplify` removes self-intersections and repairs winding; every boolean result and every imported outline passes through it before a fill/area/contains read is trusted, under one shared policy triple never re-derived per call.
- ingest/egress axis: `getPen()` ingests any pen-speaking producer (an `svgelements` `Path`, a fontTools glyph, a `uharfbuzz` outline), `draw(pen)` replays out to a SVG-path or glyph pen — ONE geometry spine, never a `d`-string re-parse between ops.
- conic axis: `arcTo`/`conicTo` and round strokes emit `PathVerb.CONIC`; `convertConicsToQuads(tolerance)` is the one conic→quad bridge for SVG egress, distinct from the `svgelements` arc-flatten — flatten on whichever owner holds the geometry.
- query axis: `area`/`bounds`/`controlPointBounds`/`contains`/`isConvex`/`clockwise`/`firstPoints` answer the layout/hit-test/winding question a placement, hole-detection, or toolpath consumer needs; `len(path)` is the contour count, and the fill rule is `Path.fillType`, set before the query.
- transform axis: `Path.transform(...)` is the 3×3 affine/perspective owner on the `pathops` side; transform on whichever side holds the geometry, never round-trip solely to transform.
- fault axis: `PathOpsError` is the provider root; the leaves map onto the closed `VectorFault` `@tagged_union` at the incurring arm, never a bare `except Exception` and never trusting `async_boundary` to swallow an unclassified Skia raise.
- offload axis: every `op`/`simplify`/`stroke`/`OpBuilder.resolve` is synchronous native CPU work, riding the `WORKER_BAND`-bounded `to_process` seam the `resvg`/`svgelements` ops cross, never inline on the event loop.
- evidence: each op captures operand count, `PathOp` member (or stroke width/cap/join/dash), in/out contour count, output `bounds`/`area`, and serialized `d` byte length as a figure-receipt field on the consuming owner; `pathops` mints no receipt of its own.
- boundary: `pathops` owns boolean set-ops, simplify/winding, stroke-to-outline/offset, conic flatten, affine, and geometric query over `SkPath`; SVG parse/transform/measure stays `svgelements`, rasterization stays `resvg_py`/`vl-convert`/`pyvips`/`pillow`, glyph-table and shaping I/O stay `fonttools`/`uharfbuzz`.

[STACKING]:
- `graphic/vector#VECTOR` admits this as the boolean/offset arm: a `VectorOp.Boolean(sources, op=PathOp.UNION)` case ingests each `svgelements`-parsed outline into a `pathops.Path` via `getPen()`, folds through `OpBuilder.add`+`resolve()` (or binary `op` for two), runs `simplify`, then `draw`s back to a fontTools `SVGPathPen` for one styled-egress fragment — closing the boolean gap with zero new geometry engine.
- `VectorOp.Outline(source, width, cap, join, dash=None)` ingests the centerline, calls `Path.stroke`, `convertConicsToQuads(...)` for SVG round-trip, then `draw`s the filled outline back — the stroke-to-outline / offset the `marks`/`diagram` plane needs for thick connectors and offset boundaries, routed through one arm.
- `getPen`/`draw` unifies the spine across typography: `fonttools`/`uharfbuzz` glyph outlines flow through the same `AbstractPen` protocol `getPen()` consumes, so a glyph ∩ clip knockout or a glyph-outline offset composes `pathops` directly onto the `typography/shape`+`typography/font` producers without a serialization hop.
- Each op rails its provider raise into the page's `VectorFault` `@tagged_union` and returns through the `expression` `Result[VectorResult, VectorFault]` the rail speaks (`Ok`/`Error`, `bind`/`map`, `Block`-collected over a batch) — a typed `VectorResult.document`/`contours`/`extent` case carrying the result `d`/`bounds`/`area`, never an erased `bytes` a consumer re-parses.
- Geometric query and the `segments`/`contours` views feed a `msgspec`/`pydantic` figure-receipt model (operand count, `PathOp` member, output contour count, `bounds`, `d` byte length), so the boolean/offset arm contributes the same structured receipt shape every other `graphic/vector` op does.

[RAIL_LAW]:
- Package: `skia-pathops`
- Owns: planar boolean set operations (`op`/`OpBuilder`, the pen-form wrappers), self-intersection removal + winding repair (`simplify`), stroke-to-outline / fixed-width offset (`Path.stroke`), conic→quad flattening, affine/perspective transform, geometric query, and the FontTools-pen ingest/egress bridge over one mutable `SkPath`
- Accept: boolean composition of `svgelements`-parsed outlines, stroke-to-outline / offset for `marks`/`diagram` thick connectors and boundaries, glyph-outline booleans/offsets over the `typography/shape`+`typography/font` producers, and the winding/area/hit-test query a hole/toolpath consumer keys per contour — every result `draw`n back to a SVG `d` for one styled egress
- Reject: a hand-rolled boolean/clip or parallel-curve offset where `op`/`OpBuilder`/`Path.stroke` exist; a re-parsed `d` between ops where the `getPen`/`draw` spine round-trips; five sibling boolean methods where one `PathOp`-keyed arm discriminates; a manual reduce where `OpBuilder` accumulates; a conic-in-SVG emission where `convertConicsToQuads` flattens; SVG parse/transform where `svgelements` owns it; a raster op where `resvg_py`/`pyvips`/`pillow` covers it; glyph-table or shaping I/O where `fonttools`/`uharfbuzz` owns it; a receipt key the consuming owner and runtime mint
