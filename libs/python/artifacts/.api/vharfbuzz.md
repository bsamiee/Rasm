# [PY_ARTIFACTS_API_VHARFBUZZ]

`vharfbuzz` supplies the shaping-QA / buffer-diff / regression companion over the artifacts text-shaping rail: the single `Vharfbuzz` class wraps a font path into a lazily-built `uharfbuzz` `Font`, shapes a text run with an optional per-lookup `onchange` buffer-diff trace, serializes a shaped `hb.Buffer` into the canonical `hb-shape`-style string golden (`glyphname=cluster@dx,dy+adv|...`), parses that golden back into a buffer-shaped `FakeBuffer` (the inverse round-trip), and renders a shaped buffer to a standalone COLRv0-layer SVG proof. It owns no shaping engine, no Unicode itemisation, and no font model — `uharfbuzz` owns the layout engine and the `ot_color_*` palette/layer surface, `fontTools` is the declared dependency for the binary-font model the wrapped face reads, and `vharfbuzz` owns exactly the QA fold the production `typography/shape#SHAPE` pipeline does not provide: the regression-golden serialize/deserialize pair, the GSUB/GPOS per-lookup buffer-evolution trace, and the buffer-to-SVG visual diff. This is the categorical-best shaping-regression owner; `typography/shape#SHAPE` reaches it as a `SHAPE_QA` `ShapeOp` arm (the golden round-trip + lookup trace running ALONGSIDE the live `hb.shape` production path on the same shared font bytes), and `typography/font#FONT` reaches it as a post-`SUBSET`/`INSTANCE`/`FREEZE` regression oracle (proving a transform did not perturb the shaped output). The owner never hand-builds the `hb-shape` serialization, never hand-parses the golden grammar, and never hand-walks the COLRv0 layer list — `serialize_buf`/`buf_from_string`/`buf_to_svg` own the whole QA fold.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vharfbuzz`
- package: `vharfbuzz`
- import: `vharfbuzz`
- owner: `artifacts`
- rail: text-shaping (shaping QA / buffer-diff / regression)
- license: MIT
- installed: `0.3.1` (pure-Python; abi-agnostic, present on cp315). The in-source `vharfbuzz.__init__.__version__` literal reads `"0.1.0"` and is stale — the dist-info / `vharfbuzz._version.__version__` truth is `0.3.1`; the version anchor is the dist-info, never the module `__version__` literal.
- depends: `uharfbuzz (>=0.34.0)` — the HarfBuzz binding whose `Blob`/`Face`/`Font`/`Buffer`/`shape` and `ot_color_*` surface every method calls (installed `0.55.0`, folder-tier `.api/uharfbuzz.md`); `fontTools` — declared dependency for the binary-font model (folder-tier `.api/fonttools.md`)
- entry points: none (library only; no console script)
- capability: lazily build a `uharfbuzz` `Font` from a font-file path; shape a text run with `script`/`direction`/`language`/`features`/`variations`/`shaper` parameters and an optional per-lookup `onchange` buffer-diff callback; serialize a shaped `hb.Buffer` to the canonical `hb-shape`-style regression-golden string (with a glyphs-only mode); parse that golden string back into a buffer-shaped `FakeBuffer` carrying `glyph_infos`/`glyph_positions` (the inverse round-trip); extract a single glyph's SVG `d`-path via `DrawFuncs`; render a shaped buffer to a standalone COLRv0-layered SVG document; read the CPAL palette-0 colour list

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the shaping-QA owner and its buffer-shaped deserialization carriers
- rail: text-shaping

`Vharfbuzz` is the single owner; it is constructed with one font-file path and holds the lazily-built `uharfbuzz` `Font` plus QA state (`shapers`, the cached `DrawFuncs`, saved variation coords, the cached palette, and — populated during a traced `shape` — the `history`/`stage`/`lastLookupID` trace cursor). `FakeBuffer`/`FakeItem` are the duck-typed buffer/record carriers `buf_from_string` returns: not real `hb.Buffer`/`hb.GlyphInfo`/`hb.GlyphPosition` objects, but structurally-compatible stand-ins exposing `glyph_infos`/`glyph_positions` (and per-record `codepoint`/`cluster` and `x_offset`/`y_offset`/`x_advance`/`y_advance`/`position`) so a deserialized golden re-enters `buf_to_svg` or any `glyph_infos`/`glyph_positions` reader. Instance members below are written `.name` on `Vharfbuzz`.

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]         | [CAPABILITY]                                                         |
| :-----: | :-------------------------------------- | :--------------------- | :------------------------------------------------------------------- |
|  [01]   | `vharfbuzz.Vharfbuzz`                   | shaping-QA engine      | `Vharfbuzz(filename)`; the wrap-shape-serialize QA fold owner        |
|  [02]   | `.hbfont`                               | lazy scaled font       | `@property` -> `uharfbuzz.Font`; lazy `Blob`->`Face`->`Font`, cached |
|  [03]   | `.drawfuncs`                            | lazy outline extractor | `@property` -> `uharfbuzz.DrawFuncs`; the SVG path-command emitter   |
|  [04]   | `.palette`                              | lazy CPAL palette      | `@property` -> `list[uharfbuzz.Color]`; CPAL palette-0, else `[]`    |
|  [05]   | `.shapers`                              | shaper backend pin     | `list[str]` HarfBuzz shaper names into `hb.shape(shapers=)`          |
|  [06]   | `.history` / `.stage` / `.lastLookupID` | trace cursor           | traced-`shape` per-lookup snapshots; the `onchange` diff cursor      |
|  [07]   | `vharfbuzz.FakeBuffer`                  | duck-typed buffer      | the `buf_from_string` return; the `hb.Buffer`-compatible carrier     |
|  [08]   | `vharfbuzz.FakeItem`                    | duck-typed record      | per-glyph stand-in; `codepoint`/`cluster` + offset/advance fields    |

[PUBLIC_TYPE_SCOPE]: the wrapped `uharfbuzz` surface vharfbuzz composes (folder-tier `.api/uharfbuzz.md`)
- rail: text-shaping

vharfbuzz constructs and reads these `uharfbuzz` objects directly; the catalog rows below are the wrapped surface so the consuming design page knows what `vharfbuzz` returns and what it leaves to the `uharfbuzz` owner. `Vharfbuzz.shape` returns a raw `hb.Buffer`, so a caller reads `buffer.glyph_infos`/`glyph_positions` (rows `[06]`/`[07]` of `uharfbuzz.md`) on the live result; the `FakeBuffer` mirror exposes the same two attributes. Rows below drop the `uharfbuzz.` qualifier.

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]                | [CAPABILITY]                                                          |
| :-----: | :----------------------------- | :---------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `Buffer`                       | shaping result                | `Vharfbuzz.shape`'s live return; read `glyph_infos`/`glyph_positions` |
|  [02]   | `GlyphInfo`                    | shaping glyph record          | `codepoint` (GID), `cluster`; serialized `glyphname=cluster`          |
|  [03]   | `GlyphPosition`                | shaping position              | `position` 4-tuple; `@dx,dy+adv` tail, `buf_to_svg` cursor            |
|  [04]   | `Color` / `OTColorLayer`       | palette colour / COLRv0 layer | `Color` rgba; `OTColorLayer.glyph`/`color_index` fill, `0xFFFF`=fg    |
|  [05]   | `FontExtents` / `GlyphExtents` | metrics                       | `FontExtents`/`GlyphExtents` — the `buf_to_svg` viewBox bounds        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: shaping with an optional per-lookup buffer-diff trace
- rail: text-shaping

`shape` is the QA shaping entry. It is NOT the production shaping path (that is `typography/shape#SHAPE`'s `hb.Face.create` -> `hb.Font.create` -> `hb.Buffer.create` pipeline); it is the regression/debug shaping path that adds the `onchange` per-lookup trace the production path omits. The `parameters` dict mirrors the HarfBuzz shaping inputs; `onchange` installs `hb.Buffer.set_message_func` so the buffer's evolution is captured at every GSUB/GPOS lookup boundary.

| [INDEX] | [SURFACE]                                     | [CAPABILITY]                                                                        |
| :-----: | :-------------------------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `Vharfbuzz.shape`                             | `shape(text, parameters=None, onchange=None) -> hb.Buffer`; per-lookup `onchange`   |
|  [02]   | `parameters["script"/"direction"/"language"]` | override the `guess_segment_properties()` defaults; a falsy value keeps the guess   |
|  [03]   | `parameters["features"]`                      | the `features` dict forwarded to `hb.shape` (whole-buffer / cluster-ranged toggles) |
|  [04]   | `parameters["variations"]`                    | save/apply/restore design coords via `set_variations`; prevents axis bleed          |
|  [05]   | `parameters["shaper"]`                        | per-call override of `self.shapers` pinning the shaper list for divergence QA       |
|  [06]   | `Vharfbuzz.make_message_handling_function`    | the diff-gated `set_message_func` factory over the GSUB/GPOS lookup-trace messages  |

[ENTRYPOINT_SCOPE]: regression-golden serialize / deserialize round-trip
- rail: text-shaping

This pair IS the categorical reason to admit `vharfbuzz`: `serialize_buf` produces the stable `hb-shape`-compatible string a shaping regression test stores as a golden, and `buf_from_string` is the exact inverse — it parses that golden back into a buffer-shaped object so a stored golden re-renders without re-shaping. `uharfbuzz`'s own `Buffer.serialize` (a `TEXT`/`JSON` GID-keyed dump) has no parsing inverse; `vharfbuzz` owns the human-glyphname round-trip.

| [INDEX] | [SURFACE]         | [CALL_SHAPE]                                          | [CAPABILITY]                                                |
| :-----: | :---------------- | :---------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `serialize_buf`   | `serialize_buf(buf, glyphsonly: bool = False) -> str` | the `hb-shape` golden string; `glyphsonly` = GID names only |
|  [02]   | `buf_from_string` | `buf_from_string(s: str) -> FakeBuffer`               | inverse parse to `FakeBuffer`; `ValueError` on a bad token  |

[ENTRYPOINT_SCOPE]: SVG visual-proof rendering
- rail: text-shaping

`buf_to_svg` renders a shaped (or deserialized) buffer to a standalone SVG document — the visual-diff proof a shaping regression attaches. It composes the cached `DrawFuncs` outline extractor and the COLRv0 layer/palette surface, accumulating a per-glyph viewBox from the font and glyph extents. This is the COLRv0-layer SVG path; it is complementary to `blackrenderer`'s COLRv1 paint-graph raster (`.api/blackrenderer.md`) and to the `typography/shape#SHAPE` `RASTERIZE` arm — `buf_to_svg` is the dependency-free, in-process SVG proof; full COLRv1 raster routes through the blackrenderer rail.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                    | [CAPABILITY]                                                         |
| :-----: | :-------------------------- | :------------------------------ | :------------------------------------------------------------------- |
|  [01]   | `buf_to_svg`                | `buf_to_svg(buf) -> str`        | full SVG doc: `<defs>`/`<use>` layout, COLRv0 fills, y-flip          |
|  [02]   | `glyph_to_svg_path`         | `glyph_to_svg_path(gid) -> str` | one glyph's SVG `d`-path via `draw_glyph` (`M`/`L`/`C`/`Q`/`Z`)      |
|  [03]   | `palette` (in `buf_to_svg`) | CPAL fill source                | COLRv0 fill: `color_index` -> `palette` `rgb()`; `0xFFFF`=fg `<use>` |

## [04]-[IMPLEMENTATION_LAW]

[QA_TOPOLOGY]:
- one class: `Vharfbuzz` is the entire library (plus the `FakeBuffer`/`FakeItem` deserialization carriers); no module-level functions. The unit of composition is "construct over a font path, `shape` a run, then `serialize_buf` / `buf_from_string` / `buf_to_svg` it". The engine is stateful — `_hbfont`/`_drawfuncs`/`_palette` are lazily cached and `_saved_variations`/`history`/`stage` mutate across a traced `shape` — so a QA owner holds one `Vharfbuzz` per font file and treats it as single-threaded.
- path-only construction: `Vharfbuzz(filename)` takes a font-file PATH, not bytes — the lazy `hbfont` property calls `hb.Blob.from_file_path(filename)`. There is no bytes-in entry, so the owning `SHAPE_QA` arm composes it exactly as `typography/shape#SHAPE` composes blackrenderer's path-only `saveImage` and as `typography/font#FONT` composes the freezer's path-only `openFont`: write the shared `font` bytes to a `tempfile.NamedTemporaryFile`, point `Vharfbuzz(path)` at the temp path, run the QA fold, and `unlink` in a `finally`.
- the regression mechanism: `serialize_buf(buf)` resolves each glyph's name via `hbfont.glyph_to_string(info.codepoint)` and emits `glyphname=cluster`, appending `@x_offset,y_offset` only when an offset is non-zero and `+x_advance` in the GPOS stage — a stable, human-readable, GID-name-keyed string. `buf_from_string(s)` parses that grammar back per `|`-token into a `FakeBuffer`, so a stored golden re-enters `buf_to_svg` or a `glyph_infos`/`glyph_positions` comparison without re-shaping. The two are exact inverses for the horizontal case; vertical advance is intentionally not preserved (`y_advance = 0`).
- the trace mechanism: a traced `shape(text, onchange=fn)` installs `make_message_handling_function` as the buffer's `set_message_func`. HarfBuzz emits `start lookup N` / `end lookup N` / `start GPOS stage` messages during shaping; the handler snapshots `serialize_buf(buf2)` at each `start lookup`, and at the matching `end lookup` compares the serialization — firing `onchange(self, stage, lookupid, self._copy_buf(buf2))` ONLY when the lookup mutated the buffer (a diff-gated per-lookup trace). `_copy_buf` snapshots `[glyph_name, cluster, position-or-None]` per glyph (position only in the GPOS stage). This is the GSUB/GPOS step-debugging surface for diagnosing WHICH lookup produced a shaping regression.
- the SVG mechanism: `buf_to_svg` reuses the cached `drawfuncs` (the `DrawFuncs` whose callbacks emit SVG path commands) via `glyph_to_svg_path`, deduplicates glyph paths into an SVG `<defs>` block keyed `gNNN`, and lays each glyph with a `translate(x_cursor+dx, y_cursor+dy)` `<g>` referencing the def. COLRv0-layered glyphs (`hb.ot_color_has_layers(face)` + `hb.ot_color_glyph_get_layers(face, gid)`) emit one `<use>` per layer with a palette-indexed fill. The viewBox accumulates from the font ascender/descender and per-glyph extents, with a final `matrix(1 0 0 -1 0 0)` y-flip from font space to SVG space.

[STACKING]:
- uharfbuzz seam (folder-tier `.api/uharfbuzz.md`): `vharfbuzz` wraps `uharfbuzz` at the positional-ctor surface — `hb.Blob.from_file_path` (row `[01]`), `hb.Face(blob)` / `hb.Font(face)` (the positional aliases of the `Face.create`/`Font.create` factory rows `[02]`/`[04]`), `hb.Buffer()` + `add_str`/`guess_segment_properties` (rows `[01]`/`[02]`/`[05]`), `hb.shape(font, buf, features, shapers=)` (row `[06]`), `buf.set_message_func` (row `[05b]`), `hbfont.glyph_to_string`/`glyph_from_string` (row `[05] of glyph queries`), `hbfont.draw_glyph(gid, DrawFuncs, state)` (row `[07] of glyph queries`), `hbfont.get_font_extents`/`get_glyph_extents` (rows `[03]`/`[02]`), and the COLRv0 colour surface `hb.ot_color_has_palettes`/`ot_color_palette_get_colors`/`ot_color_has_layers`/`ot_color_glyph_get_layers` (introspection rows `[03]`/`[03] of glyph extract`). `uharfbuzz` owns the layout engine and the colour tables; `vharfbuzz` owns the QA fold over them. The production `typography/shape#SHAPE` owner calls `uharfbuzz` directly through the `.create()` factories; the `SHAPE_QA` arm calls `vharfbuzz` for the golden/trace/SVG QA — the two reach the SAME engine, never two engines.
- fonttools seam (folder-tier `.api/fonttools.md`): `fontTools` is a declared `Requires-Dist`; `vharfbuzz` does not import it in the QA fold (its SVG path uses its own `DrawFuncs` emitter, NOT `fontTools.pens.svgPathPen.SVGPathPen`), so the dependency is the font-model substrate the wrapped face reads, available for the `typography/font#FONT` post-transform regression that round-trips a `fontTools`-subsetted `TTFont` through `vharfbuzz` to prove the subset preserved shaping. The `SVGPathPen` outline bridge stays the `typography/shape#SHAPE`/`typography/font#FONT` production path; `vharfbuzz.glyph_to_svg_path` is the QA-side standalone variant.
- blackrenderer seam (folder-tier `.api/blackrenderer.md`): `vharfbuzz.buf_to_svg` is the COLRv0-layer + monochrome-outline SVG proof; `blackrenderer` is the COLRv1 paint-graph (gradients/composites/transforms) raster. They are complementary QA surfaces — `buf_to_svg` is the zero-native-dependency in-process SVG diff a regression attaches inline; the COLRv1-correct raster proof routes through the `typography/shape#SHAPE` `RASTERIZE` arm over blackrenderer. A `buf_to_svg` call is not a substitute for COLRv1 raster, and a blackrenderer raster is not a substitute for the lightweight golden-SVG diff.
- universal-tier rails: the shaping-regression GOLDEN CORPUS is a `msgspec.Struct` row set (`libs/python/.api/msgspec.md`) — each row a `(text, parameters, expected_serialization)` frozen record, `msgspec.json`/`msgpack` encoded into the test fixture store, decoded and re-asserted against a fresh `serialize_buf`; `beartype` (`libs/python/.api/beartype.md`) validates the shaping-`parameters` dict (`script`/`direction`/`language`/`features`/`variations`/`shaper`) at the QA boundary so a malformed parameter set fails typed, not deep in `hb.shape`; the QA fold rides the same `expression`-`Result`/`RuntimeRail[ContentKey]` rail (`libs/python/.api/expression.md`) every `ShapeOp` arm returns — a `serialize_buf(actual) != golden` diff lifts into a typed rail FAILURE carrying the two serializations, never an assertion or a raised `ValueError`; the native `shape` rides `anyio.to_thread.run_sync(..., limiter=_SHAPE_LIMITER)` (`libs/python/.api/anyio.md`) under the SAME `CapacityLimiter` the `typography/shape#SHAPE` production shaping uses (GIL-releasing native, the worker shares the temp-path font); the per-lookup `onchange` trace emits one `structlog` event (`libs/python/.api/structlog.md`) and one `opentelemetry` span (`libs/python/.api/opentelemetry-api.md`) per mutating lookup, so a shaping regression's WHICH-lookup evidence is a structured trace, not a print.

[LOCAL_ADMISSION]:
- the regression golden is `serialize_buf(buf)` and its inverse `buf_from_string(s)`, never a hand-built `hb-shape` string nor a hand-parsed golden grammar — `vharfbuzz` owns the `glyphname=cluster@dx,dy+adv` serialization and its regex inverse, and `uharfbuzz.Buffer.serialize` (the GID-keyed `TEXT`/`JSON` dump with no parsing inverse) is the rejected lower-capability form for a human-readable round-trippable golden.
- the per-lookup shaping trace is `shape(text, onchange=fn)` driving the `make_message_handling_function` / `set_message_func` machinery, never a hand-installed message callback nor a manual GSUB/GPOS lookup walk — the diff-gated `onchange` (firing only on a buffer-mutating lookup) is the owned trace surface.
- the visual diff is `buf_to_svg(buf)` (and `glyph_to_svg_path` for a single glyph), never a hand-rolled glyph-outline-to-SVG emitter nor a hand-walked COLRv0 layer list — the cached `DrawFuncs` SVG emitter and the `ot_color_glyph_get_layers` palette fold own the proof.
- construction is path-only (`Vharfbuzz(filename)`); the owning arm writes the shared `font` bytes to a temp path and `unlink`s in a `finally`, exactly as the blackrenderer `saveImage` and freezer `openFont` arms do — there is no bytes-in entry to bypass it.
- the version anchor is the dist-info / `vharfbuzz._version.__version__` (`0.3.1`), never the stale `vharfbuzz.__init__.__version__` literal (`"0.1.0"`); a design that pins the version reads the dist-info, not the module attribute.

[RAIL_LAW]:
- Package: `vharfbuzz`
- Owns: shaping QA / buffer-diff / regression over the artifacts text-shaping rail — the `hb-shape`-style regression-golden serialize (`serialize_buf`) and its parsing inverse (`buf_from_string`), the diff-gated per-lookup GSUB/GPOS buffer-evolution trace (`shape(onchange=)` / `make_message_handling_function`), and the standalone COLRv0-layer SVG visual proof (`buf_to_svg` / `glyph_to_svg_path`)
- Accept: a `SHAPE_QA` `ShapeOp` arm on `typography/shape#SHAPE` running the golden round-trip + lookup trace ALONGSIDE the live production `hb.shape` over the same shared font bytes (temp-path constructed); a post-`SUBSET`/`INSTANCE`/`FREEZE` shaping-regression oracle on `typography/font#FONT`; the `msgspec` golden-corpus record set, the `beartype`-validated `parameters` dict, the `expression`-`Result` diff verdict, and the `structlog`/`opentelemetry` per-lookup trace
- Reject: a hand-built `hb-shape` string or hand-parsed golden where `serialize_buf`/`buf_from_string` own the round-trip; `uharfbuzz.Buffer.serialize` for a human-readable round-trippable golden (no parsing inverse); a hand-installed `set_message_func` callback or manual GSUB/GPOS lookup walk where `shape(onchange=)` owns the diff-gated trace; a hand-rolled glyph-to-SVG emitter or COLRv0 layer walk where `buf_to_svg` owns the proof; mistaking `vharfbuzz` for the production shaping engine — `typography/shape#SHAPE` calls `uharfbuzz` directly through the `.create()` factories, `vharfbuzz` is the QA companion over that same engine; the `__init__.__version__` literal as the version anchor
