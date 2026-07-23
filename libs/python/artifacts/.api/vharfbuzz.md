# [PY_ARTIFACTS_API_VHARFBUZZ]

`vharfbuzz` owns shaping-QA over the artifacts text-shaping rail: the single `Vharfbuzz` class wraps a font path into a lazily-built `uharfbuzz` `Font`, shapes a run with an optional per-lookup `onchange` buffer-diff trace, serializes a shaped buffer to the `hb-shape`-style regression golden (`glyphname=cluster@dx,dy+adv|...`), parses that golden back into a buffer-shaped `FakeBuffer`, and renders a shaped buffer to a standalone COLRv0-layer SVG proof. `uharfbuzz` owns the layout engine and the `ot_color_*` palette/layer surface; `vharfbuzz` owns the QA fold the production `typography/shape#SHAPE` pipeline omits — the golden serialize/deserialize pair, the GSUB/GPOS per-lookup trace, and the buffer-to-SVG diff. `typography/shape#SHAPE` reaches it as the `ShapeOp.QA` arm alongside the live `hb.shape` path; `typography/font#FONT` as a post-`SUBSET`/`INSTANCE`/`FREEZE` regression oracle.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vharfbuzz`
- package: `vharfbuzz` (MIT)
- import: `vharfbuzz`
- owner: `artifacts`
- rail: text-shaping (shaping QA / buffer-diff / regression)
- depends: `uharfbuzz` — the HarfBuzz binding whose `Blob`/`Face`/`Font`/`Buffer`/`shape` and `ot_color_*` surface every method calls (folder-tier `.api/uharfbuzz.md`); `fontTools` — the binary-font model the wrapped face reads (folder-tier `.api/fonttools.md`)
- entry points: none (library only)
- capability: lazily build a `uharfbuzz` `Font` from a font-file path; shape a run with `script`/`direction`/`language`/`features`/`variations`/`shaper` and an optional per-lookup `onchange` buffer-diff callback; serialize a shaped `hb.Buffer` to the `hb-shape`-style regression golden (glyphs-only mode); parse that golden back into a `FakeBuffer` carrying `glyph_infos`/`glyph_positions`; extract one glyph's SVG `d`-path via `DrawFuncs`; render a shaped buffer to a standalone COLRv0-layered SVG; read the CPAL palette-0 colour list

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the shaping-QA owner and its buffer-shaped deserialization carriers

`Vharfbuzz` is the single owner, constructed with one font-file path and holding the lazily-built `Font` and QA state. `FakeBuffer`/`FakeItem` are the duck-typed carriers `buf_from_string` returns — structural stand-ins exposing `glyph_infos`/`glyph_positions` (and per-record `codepoint`/`cluster`, `x_offset`/`y_offset`/`x_advance`/`y_advance`/`position`) so a deserialized golden re-enters `buf_to_svg` or any `glyph_infos`/`glyph_positions` reader. Instance members read `.name` on `Vharfbuzz`.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]          | [CAPABILITY]                                                         |
| :-----: | :-------------------------------------- | :--------------------- | :------------------------------------------------------------------- |
|  [01]   | `vharfbuzz.Vharfbuzz`                   | class                  | `Vharfbuzz(filename)`; the wrap-shape-serialize QA fold owner        |
|  [02]   | `.hbfont`                               | lazy scaled font       | `@property` -> `uharfbuzz.Font`; lazy `Blob`->`Face`->`Font`, cached |
|  [03]   | `.drawfuncs`                            | lazy outline extractor | `@property` -> `uharfbuzz.DrawFuncs`; the SVG path-command emitter   |
|  [04]   | `.palette`                              | lazy CPAL palette      | `@property` -> `list[uharfbuzz.Color]`; CPAL palette-0, else `[]`    |
|  [05]   | `.shapers`                              | shaper backend pin     | `list[str]` HarfBuzz shaper names into `hb.shape(shapers=)`          |
|  [06]   | `.history` / `.stage` / `.lastLookupID` | trace cursor           | traced-`shape` per-lookup snapshots; the `onchange` diff cursor      |
|  [07]   | `vharfbuzz.FakeBuffer`                  | duck-typed buffer      | the `buf_from_string` return; the `hb.Buffer`-compatible carrier     |
|  [08]   | `vharfbuzz.FakeItem`                    | duck-typed record      | per-glyph stand-in; `codepoint`/`cluster` + offset/advance fields    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: shaping with an optional per-lookup buffer-diff trace

`shape` is the QA/regression shaping path, not the production one (`typography/shape#SHAPE`'s `hb.Face.create` -> `hb.Font.create` -> `hb.Buffer.create` pipeline); it adds the `onchange` per-lookup trace the production path omits. `parameters` mirrors the HarfBuzz shaping inputs; `onchange` installs `set_message_func` to capture the buffer at every GSUB/GPOS lookup boundary.

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                                           |
| :-----: | :---------------------------------------------- | :------- | :--------------------------------------------------------------------- |
|  [01]   | `shape(text, parameters, onchange) -> Buffer`   | instance | QA shaping with a per-lookup `onchange` diff callback                  |
|  [02]   | `parameters["script"/"direction"/"language"]`   | key      | override `guess_segment_properties()`; a falsy value keeps the guess   |
|  [03]   | `parameters["features"]`                        | key      | `features` dict forwarded to `hb.shape`; whole-buffer / cluster-ranged |
|  [04]   | `parameters["variations"]`                      | key      | save/apply/restore design coords via `set_variations`; no axis bleed   |
|  [05]   | `parameters["shaper"]`                          | key      | per-call override of `self.shapers`; pins the shaper list for QA       |
|  [06]   | `make_message_handling_function(buf, onchange)` | factory  | diff-gated `set_message_func` over GSUB/GPOS lookup-trace messages     |

[ENTRYPOINT_SCOPE]: regression-golden serialize / deserialize round-trip

`serialize_buf` produces the stable `hb-shape`-compatible golden a regression test stores; `buf_from_string` is the exact inverse, parsing that golden back into a buffer-shaped object so a stored golden re-renders without re-shaping. `uharfbuzz.Buffer.serialize` (a `TEXT`/`JSON` GID-keyed dump) has no parsing inverse.

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :-------------------------------------- | :------- | :---------------------------------------------------------- |
|  [01]   | `serialize_buf(buf, glyphsonly) -> str` | instance | the `hb-shape` golden string; `glyphsonly` = GID names only |
|  [02]   | `buf_from_string(s) -> FakeBuffer`      | instance | inverse parse to `FakeBuffer`; `ValueError` on a bad token  |

[ENTRYPOINT_SCOPE]: SVG visual-proof rendering

`buf_to_svg` renders a shaped or deserialized buffer to a standalone SVG document — the visual-diff proof a regression attaches — composing the cached `DrawFuncs` outline extractor and the COLRv0 layer/palette surface over a per-glyph viewBox from font and glyph extents. It is the dependency-free in-process SVG path; full COLRv1 raster routes through the `blackrenderer` rail (`.api/blackrenderer.md`).

| [INDEX] | [SURFACE]                       | [SHAPE]  | [CAPABILITY]                                                         |
| :-----: | :------------------------------ | :------- | :------------------------------------------------------------------- |
|  [01]   | `buf_to_svg(buf) -> str`        | instance | full SVG doc: `<defs>`/`<use>` layout, COLRv0 fills, y-flip          |
|  [02]   | `glyph_to_svg_path(gid) -> str` | instance | one glyph's SVG `d`-path via `draw_glyph` (`M`/`L`/`C`/`Q`/`Z`)      |
|  [03]   | `palette` fill source           | property | COLRv0 fill: `color_index` -> `palette` `rgb()`; `0xFFFF`=fg `<use>` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one class: `Vharfbuzz` is the whole library (with the `FakeBuffer`/`FakeItem` carriers), no module-level functions; compose it as "construct over a font path, `shape`, then `serialize_buf` / `buf_from_string` / `buf_to_svg`". Lazy `_hbfont`/`_drawfuncs`/`_palette` caches and `_saved_variations`/`history`/`stage` trace state mutate across a traced `shape`, so a QA owner holds one `Vharfbuzz` per font file and single-threads it.
- path-only construction: `Vharfbuzz(filename)` takes a font PATH via `hb.Blob.from_file_path`, no bytes-in entry; the `QA` arm writes shared `font` bytes to a `tempfile.NamedTemporaryFile`, constructs over the temp path, and `unlink`s in a `finally`, as the blackrenderer `saveImage` and freezer `openFont` arms do.
- round-trip invariant: `serialize_buf`/`buf_from_string` are exact inverses for the horizontal case; vertical advance is not preserved (`y_advance = 0`). `shape(onchange=fn)` diff-gates its per-lookup callback to fire only on a buffer-mutating lookup, and `buf_to_svg` folds the COLRv0 layer/palette surface into a `<defs>`-deduplicated SVG with a font-space `matrix(1 0 0 -1 0 0)` y-flip.

[STACKING]:
- uharfbuzz seam (folder-tier `.api/uharfbuzz.md`): `vharfbuzz` wraps `uharfbuzz` at the positional-ctor surface — `hb.Blob.from_file_path`, `hb.Face(blob)` / `hb.Font(face)` (positional aliases of the `Face.create`/`Font.create` factories), `hb.Buffer()` with `add_str`/`guess_segment_properties`, `hb.shape(font, buf, features, shapers=)`, `buf.set_message_func`, `hbfont.glyph_to_string`/`glyph_from_string`/`draw_glyph`/`get_font_extents`/`get_glyph_extents`, and the COLRv0 colour surface `ot_color_has_palettes`/`ot_color_palette_get_colors`/`ot_color_has_layers`/`ot_color_glyph_get_layers`. `Vharfbuzz.shape` returns a live `hb.Buffer` whose `glyph_infos`/`glyph_positions` the caller reads; the `FakeBuffer` mirror exposes the same two. `uharfbuzz` owns the layout engine and colour tables, `vharfbuzz` the QA fold — `typography/shape#SHAPE` calls `uharfbuzz` directly through `.create()` for production while the `QA` arm calls `vharfbuzz`, the same engine.
- fonttools seam (folder-tier `.api/fonttools.md`): `fontTools` is a declared dependency `vharfbuzz` does not import in the QA fold — its SVG path uses its own `DrawFuncs` emitter, not `fontTools.pens.svgPathPen.SVGPathPen` — so it stays the font-model substrate for the `typography/font#FONT` post-transform regression that round-trips a `fontTools`-subsetted `TTFont` through `vharfbuzz`. `SVGPathPen` stays the production outline bridge; `glyph_to_svg_path` is the QA-side standalone variant.
- blackrenderer seam (folder-tier `.api/blackrenderer.md`): `buf_to_svg` is the COLRv0-layer + monochrome-outline SVG proof, the zero-native-dependency in-process diff a regression attaches inline; the COLRv1 paint-graph raster (gradients/composites/transforms) routes through the `typography/shape#SHAPE` `RASTERIZE` arm over `blackrenderer`.
- universal-tier rails: the shaping-regression golden corpus is a `msgspec.Struct` row set (`libs/python/.api/msgspec.md`) — each row a frozen `(text, parameters, expected_serialization)`, `msgspec.json`/`msgpack` encoded, decoded and re-asserted against a fresh `serialize_buf`; `beartype` (`libs/python/.api/beartype.md`) validates the `parameters` dict at the QA boundary; the fold rides the `expression`-`Result`/`RuntimeRail[ContentKey]` rail (`libs/python/.api/expression.md`) every `ShapeOp` arm returns, so a `serialize_buf(actual) != golden` diff lifts into a typed rail FAILURE carrying both serializations; the native `shape` rides the shape owner's `lane.offload(Kernel.of(_shape_qa, KernelTrait.RELEASING), request)` seam under the shared `THREAD_BAND`; the `onchange` trace emits one `structlog` event (`libs/python/.api/structlog.md`) and one `opentelemetry` span (`libs/python/.api/opentelemetry-api.md`) per mutating lookup.

[LOCAL_ADMISSION]:
- `serialize_buf(buf)` and its inverse `buf_from_string(s)` own the regression golden — the `glyphname=cluster@dx,dy+adv` serialization and its regex inverse.
- `shape(text, onchange=fn)` over `make_message_handling_function` / `set_message_func` owns the per-lookup trace, diff-gated to fire only on a buffer-mutating lookup.
- `buf_to_svg(buf)` (and `glyph_to_svg_path` for one glyph) owns the visual diff over the cached `DrawFuncs` emitter and the `ot_color_glyph_get_layers` palette fold.
- construction is path-only (`Vharfbuzz(filename)`); the owning arm writes shared `font` bytes to a temp path and `unlink`s in a `finally`.

[RAIL_LAW]:
- Package: `vharfbuzz`
- Owns: shaping QA / buffer-diff / regression over the artifacts text-shaping rail — the `hb-shape`-style golden serialize (`serialize_buf`) and its parsing inverse (`buf_from_string`), the diff-gated per-lookup GSUB/GPOS trace (`shape(onchange=)` / `make_message_handling_function`), and the standalone COLRv0-layer SVG proof (`buf_to_svg` / `glyph_to_svg_path`)
- Accept: the `ShapeOp.QA` arm running the golden round-trip + lookup trace alongside the live `hb.shape` over shared temp-path font bytes; a post-`SUBSET`/`INSTANCE`/`FREEZE` regression oracle on `typography/font#FONT`; the `msgspec` golden-corpus record set, the `beartype`-validated `parameters` dict, the `expression`-`Result` diff verdict, and the `structlog`/`opentelemetry` per-lookup trace
- Reject: a hand-built `hb-shape` string or hand-parsed golden where `serialize_buf`/`buf_from_string` own the round-trip; `uharfbuzz.Buffer.serialize` for a human-readable round-trippable golden (no parsing inverse); a hand-installed `set_message_func` callback or manual GSUB/GPOS walk where `shape(onchange=)` owns the diff-gated trace; a hand-rolled glyph-to-SVG emitter or COLRv0 layer walk where `buf_to_svg` owns the proof; mistaking `vharfbuzz` for the production shaping engine, which calls `uharfbuzz` directly through the `.create()` factories
