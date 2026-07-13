# [PY_ARTIFACTS_API_SKIA_PATHOPS]

`skia-pathops` supplies the categorical-best planar boolean / offset / stroke-to-outline geometry surface the `graphic/vector` owner is missing: the abi3 Cython binding of Skia's `SkPathOps` + `SkStroke` + `SkPath` over a single mutable `Path` accumulator that ingests cubics/quads/conics/arcs, accumulates segments through a FontTools-pen-protocol `getPen()`/`draw(pen)` bridge, runs N-ary set operations (`union`/`difference`/`intersection`/`xor`/`reverse_difference`, the binary `op`, the N-way `OpBuilder`), self-intersection removal + winding repair through `simplify`, and converts an open contour into a filled closed outline through `Path.stroke` (width/cap/join/miter + dash) — the boolean/offset/outline algebra `svgelements`' `Path`/`Matrix` cannot express. The `Path` is one fontTools-pen-compatible owner: it round-trips an SVG `d`/`Shape` (drawn into `getPen()`), runs the Skia algebra, then re-emits geometry through `draw(outpen)` into any pen — the `svgelements` parse owner, a fontTools `SVGPathPen`, or a `uharfbuzz`/`fonttools` glyph pen — so the `graphic/vector` rail keeps one geometry spine across boolean ops, glyph outlines, and the typography plane. The package owner composes `op`/`OpBuilder`/`simplify`/`Path.stroke` plus the `Path.draw`/`getPen` pen bridge into the `Vector` boolean/offset/outline arms; it never re-implements set-ops, winding, or stroking algebra, and it never rasterizes (PNG/PDF egress stays `resvg_py`/`vl-convert`/`pyvips`).

## [01]-[PACKAGE_SURFACE]

- package: `skia-pathops`
- import: `pathops`
- owner: `artifacts`
- rail: figure (the `graphic/vector#VECTOR` boolean/offset/outline arms)
- installed: `0.9.2`
- distribution: abi3 wheel (`_pathops.abi3.so` + pure-Python `operations.py`); the wheel binds the Skia `pathops`/`core` subset, not full Skia — no canvas, no GPU, no raster, no SVG/font I/O
- gating: none — `pathops/_pathops.abi3.so` is the forward-compatible abi3 build; it imports and runs natively on this cp315 interpreter (the resolver shows a cp315-compatible abi3 wheel, so `pyproject` carries `skia-pathops` unpinned with no `python_version` marker)
- license: BSD-3-Clause (Skia upstream; commercial-safe, no Pantone/paid data)
- entry points: none (library only)
- version constant: `pathops.__version__` (`"0.9.2"`); falls back to `"0.0.0+unknown"` only when `_version.py` is absent
- capability: a single mutable `Path` accumulator over the `PathVerb` grammar (`MOVE`/`LINE`/`QUAD`/`CONIC`/`CUBIC`/`CLOSE`); planar set operations (`union`/`difference`/`intersection`/`xor`/`reverse_difference`, binary `op`, N-way `OpBuilder`); self-intersection removal + winding repair (`simplify`); stroke-to-outline / fixed-width offset (`Path.stroke` with cap/join/miter/dash); conic→quad flattening (`convertConicsToQuads`) for SVG/quad egress; geometric query (`area`/`bounds`/`controlPointBounds`/`contains`/`isConvex`/`clockwise`/`firstPoints`); the `points`/`verbs`/`segments`/`contours` introspection views; affine `transform`; in-place `reverse`/`reset`/`rewind`; `addPath`/`add` composition; the FontTools-pen-protocol `getPen()`/`draw(pen)`/`PathPen` interop bridge; the IEEE-754 round-trip helpers (`bits2float`/`float2bits`) and `decompose_quadratic_segment`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `Path` accumulator and the pen/builder owners
- rail: figure

The whole surface is three runtime owners plus the bounded enum vocabulary. `Path` is the one mutable geometry accumulator every operation reads and writes; `PathPen` is the pen-protocol adapter `Path.getPen()` returns (it is what bridges `svgelements`/fontTools outlines INTO a `Path`); `OpBuilder` is the N-way boolean accumulator for folding more than two operands. There is no second path type, no document type, no canvas — the boolean/offset/outline concern is exactly these.

| [INDEX] | [TYPE]      | [KIND]              | [ROLE]                                                                              |
| :-----: | :---------- | :------------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `Path`      | mutable accumulator | the one `SkPath` owner; ingest verbs, run boolean/simplify/stroke, draw/query       |
|  [02]   | `PathPen`   | pen adapter         | the pen `Path.getPen()` returns; a source outline draws its segments into this path |
|  [03]   | `OpBuilder` | N-way builder       | accumulate `add(path, operator)` operands, then `resolve()` to one `Path`           |

[PUBLIC_TYPE_SCOPE]: bounded enum vocabulary
- rail: figure

Six `IntFlag`/enum families (the `__init__` fixup forces full `_member_names_` so every alias member iterates on release). These are the closed vocabularies the `graphic/vector` owner keys its `VectorOp` boolean/stroke arms against — a boolean kind selects a `PathOp` member, a stroke style selects `LineCap`/`LineJoin`, a winding policy selects `FillType`. Each is a real catalogued enum, never a stringly knob.

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
- rail: figure

`PathOpsError` is the one provider exception root; the three leaves name the precise structural cause. The `graphic/vector` owner maps these onto its closed `VectorFault` `@tagged_union` at the arm that incurs them (a boolean/simplify raise onto a `singular`/`empty`-class case, an `OpenPathError` onto a stroke/boolean precondition fault), never a bare `except Exception`.

| [INDEX] | [TYPE]                 | [BASE]         | [RAISE_SITE]                                                                              |
| :-----: | :--------------------- | :------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `PathOpsError`         | `Exception`    | provider root for every `pathops` failure                                                 |
|  [02]   | `UnsupportedVerbError` | `PathOpsError` | a pen received a verb the `SkPath` grammar cannot represent                               |
|  [03]   | `OpenPathError`        | `PathOpsError` | a closed-path operation (or `draw` with `allow_open_paths=False`) met an unclosed contour |
|  [04]   | `NumberOfPointsError`  | `PathOpsError` | a verb received the wrong control-point count                                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: boolean set operations
- rail: figure

The boolean surface is one binary `op` plus the pure-Python contour-pen wrappers plus the N-way `OpBuilder`. `op(one, two, operator, …)` is the single polymorphic binary primitive over all five `PathOp` members and returns a new `Path`. The `operations.py` wrappers (`union`/`difference`/`intersection`/`xor`/`reverse_difference`) accept FontTools contour lists + an output pen (the pen-protocol form that drops straight into a fontTools/`svgelements` glyph pipeline). `OpBuilder` folds an arbitrary number of operands into one resolve — the right owner when the `graphic/vector` rail unions a whole sheet of marks rather than two. Import-path note: `union`/`difference`/`intersection`/`xor` are re-exported at the top-level `pathops` namespace; `reverse_difference` is in `pathops.operations.__all__` but is NOT re-exported at top level — reach it as `pathops.operations.reverse_difference` (or simply key the binary `op(one, two, PathOp.REVERSE_DIFFERENCE)` form, which is always top-level). Every `op`/wrapper/`OpBuilder` row carries the shared policy triple `fix_winding=True, keep_starting_points=True, clockwise=False`, elided as `…` below; the pen-form wrappers all take `(subject_contours, clip_contours, outpen, …)` (`union` takes `(contours, outpen, …)`).

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
- rail: figure

`simplify`/`Path.simplify` removes self-intersections and fixes winding (the canonicalizer every boolean result and every imported outline passes through before fill). `Path.stroke(width, cap, join, miter_limit, dash_array=None, dash_offset=0.0)` is the stroke-to-outline / fixed-width offset primitive — it replaces the contour with the filled outline of a `width`-wide stroke under the chosen cap/join/miter and optional dash, turning an open centerline into a closed fill (a true offset for closed input, the offsetting `svgelements` lacks). `convertConicsToQuads` flattens Skia's conic arcs to quads because SVG/most consumers have no conic verb. The `…` on `simplify`/`Path.simplify` is the shared policy triple `fix_winding=True, keep_starting_points=True, clockwise=False`.

| [INDEX] | [MEMBER]                                        | [KIND]   | [ROLE]                                                         |
| :-----: | :---------------------------------------------- | :------- | :------------------------------------------------------------- |
|  [01]   | `simplify(path, …) -> Path`                     | function | self-intersection removal + winding repair -> new `Path`       |
|  [02]   | `Path.simplify(…)`                              | in-place | the same canonicalization mutating the path                    |
|  [03]   | `Path.stroke(width, cap, join, miter_limit, …)` | offset   | stroke-to-outline: filled `width`-wide stroke (cap/join/dash)  |
|  [04]   | `Path.convertConicsToQuads(tolerance=0.25)`     | flatten  | replace every conic verb with quads (SVG/quad-consumer egress) |

[ENTRYPOINT_SCOPE]: `Path` build, ingest, draw, compose
- rail: figure

`Path` builds three ways — the SkPath-native cursor methods (`moveTo`/`lineTo`/`quadTo`/`conicTo`/`cubicTo`/`arcTo`/`close`), the generic `add(verb, *pts)`, or (the integration spine) a FontTools pen via `getPen()` that a source outline draws into. `draw(pen)` is the reverse: it replays the accumulated geometry into any output pen, so a `Path` round-trips back to `svgelements`/fontTools/a SVG-path pen. `addPath` composes another path's contours in.

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
- rail: figure

`Path.transform` is the full 3×3 affine (matches the `svgelements` `Matrix` 6-tuple plus perspective), signature `Path.transform(scaleX=1.0, skewY=0.0, skewX=0.0, scaleY=1.0, translateX=0.0, translateY=0.0, perspectiveX=0.0, perspectiveY=0.0, perspectiveBias=1.0)` — identity defaults on the diagonal and bias. The query properties answer the layout/hit-test/winding questions a placement or toolpath consumer needs without re-deriving geometry. The introspection views (`segments`/`verbs`/`points`/`contours`/`firstPoints`) are the read side a winding/hole/contour consumer keys per loop, and the pen-replay `draw` is the canonical serialization path.

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
- rail: figure

`PathPen` is the FontTools `AbstractPen` shape — what `getPen()` returns and what every drawable outline calls. The float helpers expose the exact IEEE-754 round-trip Skia uses internally; `decompose_quadratic_segment` splits an over-long quad for a consumer with a degree cap.

| [INDEX] | [MEMBER]                                          | [KIND] | [ROLE]                                                |
| :-----: | :------------------------------------------------ | :----- | :---------------------------------------------------- |
|  [01]   | `PathPen.moveTo(pt)` / `lineTo(pt)`               | pen    | line/move pen appends into the owning `Path`          |
|  [02]   | `PathPen.curveTo(*points)` / `qCurveTo(*points)`  | pen    | cubic / quad pen appends into the owning `Path`       |
|  [03]   | `PathPen.closePath()` / `endPath()`               | pen    | close (filled contour) / end-open (stroke centerline) |
|  [04]   | `PathPen.addComponent(glyphName, transformation)` | pen    | composite-glyph reference (fontTools component verb)  |
|  [05]   | `bits2float(float_as_bits)` / `float2bits(x)`     | helper | IEEE-754 int/float round-trip (exact fidelity)        |
|  [06]   | `decompose_quadratic_segment(points)`             | helper | split a quadratic chain into renderable quad segments |

## [04]-[IMPLEMENTATION_LAW]

- import: `import pathops` at boundary scope only; the distribution name is `skia-pathops`, the import name is `pathops`; the version is `pathops.__version__` (not a `_VERSION` constant). In the `graphic/vector` page this is a `lazy import pathops` beside the existing `lazy from svgelements import …` and `lazy import resvg_py`, deferring the abi3 `.so` load off the import-time path.
- boolean axis: `op(one, two, operator)` is the single binary boolean primitive over all five `PathOp` members; the `graphic/vector` owner keys a boolean `VectorOp` arm to a `PathOp` member, never five sibling boolean methods. For more than two operands `OpBuilder.add(path, op)` × N then `resolve()` is the one N-way fold — never a hand-rolled reduce of repeated `op` calls when the builder accumulates natively. The pure-Python `union`/`difference`/`intersection`/`xor` (top-level) and `pathops.operations.reverse_difference` (operations-module only) wrappers are the pen-protocol form (contour list + output pen) for a fontTools-shaped caller; the `Path`+`op` form is the in-rail form. Pick the form by whether the operands are already `Path`s (use `op`/`OpBuilder`) or fontTools contours (use the wrappers). Because the `graphic/vector` rail already holds `Path`s, the binary `op(one, two, PathOp.<MEMBER>)` / `OpBuilder` form is the canonical in-rail choice and sidesteps the `reverse_difference` import asymmetry entirely.
- offset / stroke-to-outline axis: `Path.stroke(width, cap, join, miter_limit, dash_array=None, dash_offset=0.0)` is the one fixed-width offset / stroke-to-outline owner — it converts an open centerline into a closed filled outline (and offsets a closed contour by half-width each side), the offsetting algebra `svgelements` has no member for. Cap is a `LineCap`, join a `LineJoin`, and the dash is `(dash_array, dash_offset)` — never a hand-tessellated parallel-curve offset. Because `stroke` emits conic arcs for round caps/joins, follow it with `convertConicsToQuads(tolerance)` before drawing into a SVG/quad pen.
- canonicalize axis: `simplify`/`Path.simplify` is the self-intersection-removal + winding-repair canonicalizer; every boolean result and every imported third-party outline passes through it before a fill/area/contains read is trusted. `fix_winding`/`keep_starting_points`/`clockwise` are the policy knobs shared by `op`/`simplify`/`OpBuilder`/the wrappers — one consistent policy triple, never a per-call re-derivation.
- ingest/egress axis: `Path.getPen()` returns a FontTools `PathPen`; a `svgelements` `Path`, a fontTools glyph, or a `uharfbuzz` outline draws INTO it, so geometry enters from any pen-speaking producer. `Path.draw(pen)` replays the accumulated geometry into any output pen, so the boolean/offset result round-trips back to a SVG-path pen (`fonttools.pens.svgPathPen.SVGPathPen`) or a glyph pen — the `graphic/vector` rail keeps ONE geometry spine: ingest the `svgelements`-parsed `Path` segments → `pathops.Path` → boolean/simplify/stroke → `draw` back to a SVG `d`. Never re-parse the `d` string between ops.
- conic axis: `arcTo`/`conicTo` and round-cap/round-join strokes emit `PathVerb.CONIC` segments; SVG (and most quad/cubic consumers) have no conic, so `convertConicsToQuads(tolerance)` flattens them before egress. This is the one conic→quad bridge — distinct from the `svgelements` arc-flatten rows (`approximate_arcs_with_cubics`), which operate on the `svgelements` side; pick the flattener that matches which owner currently holds the geometry.
- query axis: `area`/`bounds`/`controlPointBounds`/`contains`/`isConvex`/`clockwise`/`firstPoints` answer the area/extent/hit-test/winding/convexity question a placement, hole-detection, or toolpath consumer needs; `len(path)` is the contour count (not segment count), and `segments`/`verbs`/`points`/`contours` are the per-loop read side. The fill rule for `area`/`contains` is `Path.fillType` (`FillType`), set before the query, never assumed.
- transform axis: `Path.transform(...)` is the full 3×3 affine/perspective owner on the `pathops` side; for geometry already living in `svgelements`, the `svgelements` `Matrix` is the affine owner — transform on whichever side holds the geometry, never round-trip solely to transform.
- fault axis: `PathOpsError` is the provider root; `OpenPathError` (closed-op/`draw` met an open contour), `UnsupportedVerbError` (pen got an unrepresentable verb), and `NumberOfPointsError` (wrong control-point count) are the named leaves. The `graphic/vector` owner maps each onto its closed `VectorFault` `@tagged_union` at the incurring arm — a boolean/simplify degeneracy onto an `empty`/`singular`-class case, an `OpenPathError` onto a stroke/boolean precondition fault — never a bare `except Exception` and never trusting `async_boundary` to swallow an unclassified Skia raise.
- offload axis: every `op`/`simplify`/`stroke`/`OpBuilder.resolve` call is synchronous native CPU work; in the `graphic/vector` page it rides the same `WORKER_BAND`-bounded `to_process` seam the existing `resvg`/`svgelements` ops cross, never inline on the event loop.
- evidence: each boolean/offset/outline op captures operand count, `PathOp` member (or stroke width/cap/join/dash), input/output contour count (`len`), output `bounds`/`area`, and serialized `d`-string byte length as a figure receipt field on the consuming owner's receipt — `pathops` mints no receipt of its own.
- boundary: `pathops` owns boolean set-ops, simplify/winding, stroke-to-outline/offset, conic flatten, affine, and geometric query over `SkPath`; SVG parse/transform/measure/sample/value-objects stay `svgelements`; rasterization to PNG/PDF stays `resvg_py`/`vl-convert`/`pyvips`/`pillow`; glyph-table and shaping I/O stay `fonttools`/`uharfbuzz`; the live UI stays outside this package.

[STACKING]:
- The `graphic/vector#VECTOR` page admits this as the boolean/offset/outline arm: a new `VectorOp.Boolean(sources, op=PathOp.UNION)` case ingests each `svgelements`-parsed `Path` outline into a `pathops.Path` via `getPen()`, folds them through `OpBuilder.add(p, op)` + `resolve()` (or binary `op` for two), runs `simplify`, then `draw`s the result back to a fontTools `SVGPathPen` for one `path(...)` styled-egress fragment — closing the boolean gap `svgelements` cannot fill, with zero new geometry engine.
- A `VectorOp.Outline(source, width, cap, join, dash=None)` case ingests the centerline `Path`, calls `Path.stroke(width, cap, join, miter_limit, dash_array, dash_offset)`, `convertConicsToQuads(...)` for SVG round-trip, then `draw`s the filled outline back — the stroke-to-outline / fixed-width offset the `marks`/`diagram` plane needs for thick connectors and offset boundaries (and the offset a `graphic/marks` glyph or a `visualization/diagram/draw#DRAW` edge requires), routed through one arm not a per-mark stroker.
- The pen-protocol bridge unifies the geometry spine across the typography plane: `fonttools`/`uharfbuzz` produce glyph outlines through the SAME `AbstractPen` protocol `Path.getPen()` consumes, so a text-on-path boolean (glyph ∩ clip, glyph union for a knockout) or a glyph-outline offset composes `pathops` directly onto the `typography/shape`+`typography/font` outline producers without a serialization hop — one `Path`/pen spine from shaped text through boolean to SVG.
- Each boolean/offset op rails its provider raise into the page's `VectorFault` `@tagged_union` and returns through the `expression` `Result[VectorResult, VectorFault]` the rest of the rail already speaks (`Ok`/`Error`, `bind`/`map`, `Block`-collected over a multi-op batch) — the `op`/`simplify`/`stroke` outcome is a typed `VectorResult.document`/`contours`/`extent` case carrying the result `d`-string/`bounds`/`area`, structurally addressable, never an erased `bytes` a consumer re-parses.
- Geometric query (`area`/`bounds`/`contains`/`firstPoints`/`len`) and the `segments`/`contours` views feed a `msgspec`/`pydantic` figure-receipt model (operand count, `PathOp` member, output contour count, output `bounds`, `d` byte length) so the boolean/offset op's evidence is the same structured receipt shape every other `graphic/vector` op contributes — one receipt family, the boolean/offset arm as cases not a parallel rail.

## [05]-[LOCAL_ADMISSION]

- Package: `skia-pathops`
- Owns: planar boolean set operations (`union`/`difference`/`intersection`/`xor`/`reverse_difference`, binary `op`, N-way `OpBuilder`), self-intersection removal + winding repair (`simplify`), stroke-to-outline / fixed-width offset (`Path.stroke` with cap/join/miter/dash), conic→quad flattening (`convertConicsToQuads`), affine/perspective transform, geometric query (`area`/`bounds`/`contains`/`isConvex`/`clockwise`), and the FontTools-pen ingest/egress bridge (`getPen`/`draw`/`PathPen`) over one mutable `SkPath`
- Accept: the `graphic/vector#VECTOR` boolean/offset/outline arms — boolean composition of `svgelements`-parsed outlines, stroke-to-outline / offset for `marks`/`diagram` thick connectors and offset boundaries, glyph-outline booleans/offsets composed onto the `typography/shape`+`typography/font` pen producers, and the winding/area/hit-test query a hole/toolpath consumer keys per contour — every result round-tripped back through `draw` to a SVG `d` for the one `path(...)` styled egress
- Reject: a hand-rolled boolean/clip or parallel-curve offset where `op`/`OpBuilder`/`Path.stroke` exist; a re-parsed `d` string between ops where the `getPen`/`draw` pen spine round-trips; five sibling boolean methods where one `PathOp`-keyed arm discriminates; a manual reduce of repeated `op` where `OpBuilder` accumulates N operands; a conic-in-SVG emission where `convertConicsToQuads` flattens; SVG parse/transform/measure where `svgelements` owns it; a raster operation where `resvg_py`/`pyvips`/`pillow` covers it; glyph-table or shaping I/O where `fonttools`/`uharfbuzz` owns it; a receipt/content key the consuming owner and the runtime mint
