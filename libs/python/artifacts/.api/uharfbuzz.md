# [PY_ARTIFACTS_API_UHARFBUZZ]

`uharfbuzz` supplies the Python binding to HarfBuzz for the artifacts text-shaping rail: `Blob`/`Face`/`Font` for font-data loading and metric/glyph queries, `Buffer` + `shape` for OpenType layout shaping, `DrawFuncs`/`Font.draw_glyph*` for outline extraction, the COLRv1 colour stack (`PaintFuncs`/`RasterPaint`/`RasterImage` + `Font.paint_glyph` + `ot_color_*`), the OpenType introspection family (`ot_layout_*`/`ot_math_*`/`ot_var_*`/name table), and the subsetting engine (`SubsetInput`/`SubsetPlan`/`subset` + the GSUB/GPOS table repacker). The package owner composes these surfaces into a shaping/colour/subset pipeline and never re-implements Unicode itemisation, OpenType layout, COLRv1 paint composition, or the table-repacking overflow resolver that HarfBuzz owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `uharfbuzz`
- package: `uharfbuzz`
- import: `uharfbuzz`
- owner: `artifacts`
- rail: text-shaping
- license: Apache-2.0 (Cython binding over the bundled libharfbuzz, which includes the subsetter and the GSUB/GPOS repacker)
- asset: runtime library (Cython extension over libharfbuzz); `Requires-Python>=3.10`, cp315-clean
- installed: `0.55.0` reflected via `import uharfbuzz` on cp315 (`uharfbuzz.version_string()` returns the linked libharfbuzz version)
- entry points: none (library only)
- capability: OpenType text shaping; glyph advance/extents/origin/name queries in both axes; glyph outline extraction via draw callbacks or a fontTools pen; COLRv1 paint extraction and CPU rasterization to a BGRA32/A8 image; bitmap (CBDT/sbix PNG) and SVG colour-glyph extraction; OpenType layout/math/variation/name-table introspection; named-instance and variation-axis enumeration; font subsetting (glyph/unicode/feature/name/axis closure with retain-GID and instancing); GSUB/GPOS subtable repacking with overflow resolution

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: font data, metric, and shaping-record objects
- rail: text-shaping

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE]       | [CAPABILITY]                                                                                  |
| :-----: | :-------------- | :------------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `Blob`          | raw font data        | `Blob.from_file_path(filename)` load; `.data` bytes; `Face.reference_table` returns a `Blob`  |
|  [02]   | `Face`          | font face            | `Face.create(blob, index)`; introspection + subset-source surface (`upem`/`glyph_count`/...)  |
|  [03]   | `Font`          | scaled font          | `Font.create(face)`; metric/glyph/outline/paint query surface                                 |
|  [04]   | `FontExtents`   | font-level metrics   | `ascender`, `descender`, `line_gap`                                                           |
|  [05]   | `GlyphExtents`  | glyph bounding box   | `x_bearing`, `y_bearing`, `width`, `height`                                                   |
|  [06]   | `GlyphInfo`     | shaping glyph record | `codepoint` (GID), `cluster`, `flags` (`GlyphFlags`) from a shaped `Buffer`                    |
|  [07]   | `GlyphPosition` | shaping position     | `x_advance`, `y_advance`, `x_offset`, `y_offset`                                               |
|  [08]   | `GlyphFlags`    | glyph-flag enum      | `UNSAFE_TO_BREAK`, `UNSAFE_TO_CONCAT`, `SAFE_TO_INSERT_TATWEEL` (line-break safety)            |
|  [09]   | `HarfBuzzError` / `RepackerError` / `SerializerError` | error rails | exception carriers raised by shaping, repacking, and serialization      |
|  [10]   | `Map` / `Set`   | hb containers        | `Map.get/keys/values/items`; `Set.add/add_range/update/intersection_update/invert/issubset` (subset glyph/unicode sets) |

[PUBLIC_TYPE_SCOPE]: shaping buffer and feature objects
- rail: text-shaping

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE] | [CAPABILITY]                                                                       |
| :-----: | :---------------------- | :------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `Buffer`                | shaping buffer | `Buffer.create()` Unicode container with direction/script/language/cluster_level   |
|  [02]   | `BufferClusterLevel`    | cluster enum   | `DEFAULT` / `MONOTONE_GRAPHEMES` / `MONOTONE_CHARACTERS` / `GRAPHEMES` / `CHARACTERS` |
|  [03]   | `BufferContentType`     | content enum   | `INVALID`, `UNICODE`, `GLYPHS`                                                      |
|  [04]   | `BufferFlags`           | flags enum     | `BOT`/`EOT`/`PRESERVE_DEFAULT_IGNORABLES`/`PRODUCE_UNSAFE_TO_CONCAT`/`VERIFY`/...   |
|  [05]   | `BufferSerializeFormat` | serialize enum | `TEXT`, `JSON`, `INVALID` output format for `Buffer.serialize`                      |
|  [06]   | `BufferSerializeFlags`  | serialize flags | `NO_CLUSTERS`/`NO_POSITIONS`/`NO_GLYPH_NAMES`/`GLYPH_EXTENTS`/`GLYPH_FLAGS`/...     |

[PUBLIC_TYPE_SCOPE]: outline, colour-paint, and raster objects
- rail: text-shaping

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]       | [CAPABILITY]                                                                          |
| :-----: | :-------------------- | :------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `DrawFuncs`           | outline extractor    | `DrawFuncs.create()`; `set_{move_to,line_to,quadratic_to,cubic_to,close_path}_func`; `draw_glyph`/`get_glyph_shape` |
|  [02]   | `PaintFuncs`          | COLRv1 paint funcs   | `set_{color,color_glyph,linear_gradient,radial_gradient,sweep_gradient,image,push_clip_glyph,push_clip_rectangle,push_group,pop_group,push_transform,pop_transform,custom_palette_color,pop_clip}_func` |
|  [03]   | `RasterPaint`         | CPU colour renderer  | drives `PaintFuncs` to render a COLRv1 glyph to a `RasterImage`; palette/foreground/background/transform/scale_factor + `paint_glyph`/`render` |
|  [04]   | `RasterImage`         | rendered bitmap      | `buffer`, `format` (`RasterFormat.BGRA32`/`A8`), `extents` (`RasterExtents`)           |
|  [05]   | `RasterDraw`          | CPU outline renderer | renders a monochrome glyph outline to a `RasterImage` (A8) via draw callbacks         |
|  [06]   | `Color`/`ColorLine`/`ColorStop` | paint colour | RGBA colour, gradient color-line, and gradient stop (`offset`/`color`/`is_foreground`) |
|  [07]   | `PaintExtend`/`PaintCompositeMode` | paint enums | gradient extend (`PAD`/`REPEAT`/`REFLECT`); 27-mode Porter-Duff/blend compositing      |
|  [08]   | `FontFuncs`           | custom font funcs    | `FontFuncs.create()`; override nominal-glyph/advance/name/var-glyph/extents callbacks  |

[PUBLIC_TYPE_SCOPE]: OpenType introspection and subsetting objects
- rail: text-shaping

| [INDEX] | [SYMBOL]                                    | [PACKAGE_ROLE]      | [CAPABILITY]                                                                      |
| :-----: | :------------------------------------------ | :------------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `OTVarAxisInfo` / `OTVarNamedInstance`      | variation introspect | axis `tag`/`min_value`/`default_value`/`max_value`/`flags`; named-instance coords |
|  [02]   | `OTNameEntry` / `OTNameIdPredefined`        | name table          | name-record `name_id`/`language`; predefined IDs (`FONT_FAMILY`/`LICENSE`/`POSTSCRIPT_NAME`/...) |
|  [03]   | `OTColorPalette` / `OTColorPaletteFlags` / `OTColorLayer` / `OTColor` | colour palette | CPAL palette `colors`/`flags`/`name_id`; COLRv0 layer `glyph`/`color_index` |
|  [04]   | `OTMathConstant` / `OTMathKern` / `OTMathGlyphPart` / `OTMathGlyphVariant` / `OTMathKernEntry` | math layout | MATH-table constants, kerning, assemblies, and stretch variants |
|  [05]   | `OTLayoutGlyphClass` / `OTMetricsTag` / `StyleTag` | layout tags  | GDEF glyph class; metric tags; style-attribute tags (`ITALIC`/`SLANT_ANGLE`/`WEIGHT`/`WIDTH`/`OPTICAL_SIZE`) |
|  [06]   | `SubsetInput`                               | subset parameters   | flags + glyph/unicode/feature/name/axis closure sets; per-axis pin/range; `.subset(face)` one-shot |
|  [07]   | `SubsetInputSets` / `SubsetFlags`           | subset enums        | set selector (`UNICODE`/`GLYPH_INDEX`/`LAYOUT_FEATURE_TAG`/`NAME_ID`/...); flags (`RETAIN_GIDS`/`NO_HINTING`/`DESUBROUTINIZE`/`GLYPH_NAMES`/`NO_LAYOUT_CLOSURE`/...) |
|  [08]   | `SubsetPlan`                                | subset plan         | `execute()` -> `Face`; `new_to_old_glyph_mapping`/`old_to_new_glyph_mapping` GID maps |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: font loading
- rail: text-shaping

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `Blob.from_file_path(filename)`                            | file load      | read font file bytes as a `Blob`                   |
|  [02]   | `Face.create(blob: bytes, index: int = 0)`                 | face creation  | construct a `Face` from raw bytes or TTC index     |
|  [03]   | `Face.create_for_tables(func, user_data)`                  | virtual face   | create a `Face` backed by a table-fetch callback   |
|  [04]   | `Font.create(face: Face)`                                  | font creation  | create a scaled `Font` from a `Face`               |
|  [05]   | `version_string()`                                         | version query  | linked libharfbuzz version string                  |

[ENTRYPOINT_SCOPE]: text input and shaping
- rail: text-shaping

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY]  | [CAPABILITY]                                                  |
| :-----: | :----------------------------------------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `Buffer.create()`                                                        | buffer init     | allocate an empty shaping buffer                             |
|  [02]   | `Buffer.add_str(text: str, item_offset=0, item_length=-1)`               | Unicode input   | add Python `str` text to the buffer                          |
|  [03]   | `Buffer.add_codepoints(codepoints: list[int], item_offset=0, item_length=-1)` | codepoint input | add a list of Unicode codepoints                       |
|  [04]   | `Buffer.add_utf8(text: bytes, item_offset=0, item_length=-1)`            | UTF-8 input     | add `bytes` UTF-8 text                                       |
|  [05]   | `Buffer.guess_segment_properties()`                                      | auto properties | infer direction, script, language from buffer content        |
|  [06]   | `shape(font, buffer, features=None, shapers=None)`                       | shaping entry   | run OpenType layout shaping; mutates buffer in-place          |
|  [07]   | `Buffer.serialize(font, format=BufferSerializeFormat.TEXT, flags=BufferSerializeFlags.DEFAULT)` -> `str` | result text | human/JSON-readable shaped glyph sequence |
|  [08]   | `Buffer.glyph_infos` / `Buffer.glyph_positions`                          | shaped result   | `list[GlyphInfo]` / `list[GlyphPosition]` read after `shape` |

[ENTRYPOINT_SCOPE]: glyph metric, outline, and colour queries
- rail: text-shaping

`get_glyph_h_advance`/`get_glyph_v_advance` and the `*_h_origin`/`*_v_origin` pair give both writing directions; `paint_glyph` is the COLRv1 entry; `get_glyph_color_png`/`Face.get_glyph_color_svg` / `ot_color_glyph_get_png`/`ot_color_glyph_get_svg` are the bitmap/SVG colour-glyph extractors.

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------------------ | :------------- | :----------------------------------------------------------- |
|  [01]   | `Font.get_glyph_h_advance(gid)` / `get_glyph_v_advance(gid)`  | advance query  | horizontal/vertical advance in font units                    |
|  [02]   | `Font.get_glyph_extents(gid)`                                 | bbox query     | `GlyphExtents` bounding box                                   |
|  [03]   | `Font.get_font_extents(direction)`                            | font metrics   | `FontExtents` ascender/descender/line-gap                    |
|  [04]   | `Font.get_nominal_glyph(unicode)` / `get_variation_glyph(u, vs)` | codepoint map | GID for a codepoint (and Unicode-variation-selector form)   |
|  [05]   | `Font.get_glyph_name(gid)` / `get_glyph_from_name(name)` / `glyph_to_string(gid)` / `glyph_from_string(s)` | name map | GID<->name and GID<->`gidNNN`/name string |
|  [06]   | `Font.draw_glyph_with_pen(gid, pen)`                          | outline export | drive a fontTools-compatible pen with the glyph outline      |
|  [07]   | `Font.draw_glyph(gid, draw_funcs: DrawFuncs, draw_state=None)` | raw draw       | dispatch outline to `DrawFuncs` callbacks                    |
|  [08]   | `Font.paint_glyph(gid, paint_funcs: PaintFuncs, paint_state=None, palette_index=0, foreground=None)` | colour paint | drive COLRv1 paint callbacks |
|  [09]   | `Font.get_glyph_color_png(glyph)` / `ot_color_glyph_get_svg(face, glyph)` / `ot_color_glyph_get_layers(face, glyph)` | colour extract | CBDT/sbix PNG `Blob`, SVG `Blob`, COLRv0 layer list |
|  [10]   | `Font.set_variation(name, value)` / `set_variations(variations: dict[str, float])` / `set_var_coords_normalized(coords: list[float])` | axis setting | set variation axis positions (tag-keyed, or normalised coords) |
|  [11]   | `Font.synthetic_bold` / `synthetic_slant` / `ptem` / `ppem` / `scale` | synth/render state | synthetic emboldening/slant, point size, ppem, integer scale |

[ENTRYPOINT_SCOPE]: OpenType introspection
- rail: text-shaping

These read the font's layout/variation/math/colour tables directly — the owner queries feature/script/baseline/math metrics from the live `Face`/`Font` instead of re-parsing GSUB/GPOS/MATH/fvar/CPAL.

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY]    | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------------------------ | :---------------- | :------------------------------------------------------- |
|  [01]   | `Face.axis_infos` / `Face.named_instances`                                | variation introspect | `list[OTVarAxisInfo]` axes; `list[OTVarNamedInstance]` |
|  [02]   | `Face.list_names()` / `Face.get_name(name_id, ...)`                       | name table        | enumerate/read OpenType name records                     |
|  [03]   | `Face.color_palettes` / `ot_color_has_paint(face)` / `ot_color_has_png(face)` / `ot_color_has_svg(face)` | colour caps | CPAL palettes; COLRv1/bitmap/SVG colour-format probes |
|  [04]   | `ot_layout_table_get_script_tags` / `ot_layout_script_get_language_tags` / `ot_layout_language_get_feature_tags` | feature introspect | GSUB/GPOS script/language/feature tag enumeration |
|  [05]   | `ot_layout_get_glyph_class(face, glyph)` / `ot_layout_get_baseline(font, ...)` | layout query | GDEF glyph class; baseline position by tag/direction/script |
|  [06]   | `ot_math_has_data(face)` / `ot_math_get_constant(font, OTMathConstant)` / `ot_math_get_glyph_variants` / `ot_math_get_glyph_assembly` | math layout | MATH-table constants, stretch variants, and assemblies |
|  [07]   | `ot_tag_to_script(tag)` / `ot_tag_to_language(tag)`                       | tag resolve       | OT tag to BCP-47/ISO resolution                          |

[ENTRYPOINT_SCOPE]: subsetting and table repacking
- rail: text-shaping

`SubsetInput` is the rich closure surface: mutate `glyph_set`/`unicode_set` (`Set` containers), select retention via `flags`/`SubsetInputSets`, pin or range-restrict variation axes, then `subset(face, input)` or `input.subset(face)`. `SubsetPlan` exposes the GID remapping. The `repack`/`serialize` family is HarfBuzz's GSUB/GPOS overflow-resolving table repacker for synthesized layout tables.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY]  | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `SubsetInput()`                                                            | parameters init | allocate subsetting parameter object                         |
|  [02]   | `SubsetInput.glyph_set` / `unicode_set` / `sets(SubsetInputSets)`          | retention sets  | mutable `Set` of GIDs / codepoints / feature-name-script tags |
|  [03]   | `SubsetInput.flags` (`SubsetFlags`)                                        | retention flags | `RETAIN_GIDS`/`NO_HINTING`/`DESUBROUTINIZE`/`GLYPH_NAMES`/`NO_LAYOUT_CLOSURE`/... |
|  [04]   | `SubsetInput.keep_everything()`                                           | full retain     | configure to retain all glyphs and tables                    |
|  [05]   | `SubsetInput.pin_axis_to_default(face, tag)` / `pin_all_axes_to_default(face)` | axis pin    | instance out a variation axis at its default                 |
|  [06]   | `SubsetInput.pin_axis_location(face, tag, value)` / `set_axis_range(face, tag, min_value=None, max_value=None, def_value=None)` | axis instance | pin/range-restrict a variation axis (partial instancing) |
|  [07]   | `subset_preprocess(face)`                                                  | preprocessing   | prepare a `Face` for faster repeated subsetting              |
|  [08]   | `subset(face, input)` / `SubsetInput.subset(face)`                         | subset execute  | produce a subsetted `Face`                                   |
|  [09]   | `SubsetPlan.execute()` / `.new_to_old_glyph_mapping` / `.old_to_new_glyph_mapping` | plan execute | run a plan and read the GID remapping                  |
|  [10]   | `repack(subtables, graphnodes)` / `serialize(subtables, graphnodes)` (+`*_with_tag`) | repacker | resolve GSUB/GPOS subtable-graph offset overflows; serialize to table bytes |

## [04]-[IMPLEMENTATION_LAW]

[SHAPING_TOPOLOGY]:
- namespace: `uharfbuzz`; Cython binding exposing the `hb_*` API as Python classes and top-level functions; `version_string()` reports the linked libharfbuzz.
- shaping pipeline: `Blob.from_file_path` -> `Face.create` -> `Font.create` -> `Buffer.create` -> `Buffer.add_str`/`add_codepoints` -> `Buffer.guess_segment_properties` -> `shape(font, buffer, features)` -> read `buffer.glyph_infos` / `buffer.glyph_positions`.
- properties: `Buffer.direction`, `Buffer.script`, `Buffer.language`, `Buffer.cluster_level`, `Buffer.flags` are settable; `guess_segment_properties()` is called before `shape` only when explicit direction/script/language is unset.
- features: the `features` parameter to `shape` accepts `dict[str, int | bool | Sequence[tuple[int, int, int | bool]]]` for whole-buffer or cluster-ranged feature toggles; `shapers` pins the shaper backend list.
- break safety: `GlyphInfo.flags & GlyphFlags.UNSAFE_TO_BREAK`/`UNSAFE_TO_CONCAT` is the line-break/run-concatenation safety signal a layout owner reads to decide where a shaped run may be split or reshaped.
- outline extraction: `Font.draw_glyph_with_pen(gid, pen)` drives a fontTools-compatible pen; `Font.draw_glyph(gid, DrawFuncs, state)` drives raw callbacks; raw `DrawFuncs` callbacks are boundary use only.
- colour extraction: `Font.paint_glyph(gid, PaintFuncs, ...)` walks the COLRv1 paint graph (gradients via `ColorLine`/`PaintExtend`, compositing via `PaintCompositeMode`, clips/transforms/groups); `RasterPaint`/`RasterImage` is the CPU rasterizer when an in-process BGRA32/A8 bitmap is wanted without a separate engine. `get_glyph_color_png`/`ot_color_glyph_get_svg`/`ot_color_glyph_get_layers` cover the CBDT/sbix-PNG, SVG, and COLRv0-layer colour formats. `ot_color_has_paint`/`has_png`/`has_svg`/`has_layers` probe which format a face carries.
- variation: `Font.set_variation(name, value)` / `set_variations({tag: value})` / `set_var_coords_normalized(coords)` set axis positions; `Face.axis_infos`/`named_instances` enumerate the design space.

[LOCAL_ADMISSION]:
- shaping uses the five-step pipeline above; `Font.draw_glyph_with_pen` bridges to `fontTools.pens` for outline extraction.
- COLRv1 paint extraction routes through `Font.paint_glyph` + `PaintFuncs`; the in-process `RasterPaint`/`RasterImage` rasterizer is used only where a CPU bitmap is needed inside the shaping owner — full colour-glyph SVG composition belongs to the `blackrenderer` rail, which itself drives this same HarfBuzz paint surface.
- subsetting via `uharfbuzz.subset` is the closure-and-instancing path (set `flags`/`glyph_set`/`unicode_set`, pin/range axes, then `subset`); `SubsetPlan` is taken when the GID remapping must be read; the `fonttools.subset.Subsetter` path remains available for fontTools-side feature-policy control.
- `repack`/`serialize` is the HarfBuzz overflow-resolving repacker for GSUB/GPOS subtable graphs synthesized by a higher tier; it is boundary use only and never re-implemented.
- `buffer.glyph_infos`/`buffer.glyph_positions` are read after `shape`; they are lists of `GlyphInfo`/`GlyphPosition`.

[STACK_INTEGRATION]:
- `uharfbuzz` + `fonttools`: HarfBuzz shapes (`shape` -> GIDs + positions) and extracts outlines via `Font.draw_glyph_with_pen` into a fontTools pen; `fonttools` owns the binary font model, designspace compilation, and the fontTools-side subsetter — HarfBuzz owns the layout engine, the live-table introspection (`ot_layout_*`/`axis_infos`), and the production subsetter/repacker. The two meet at the pen protocol and the `Face`/`TTFont` byte boundary.
- `uharfbuzz` -> `blackrenderer`: `blackrenderer` drives `Font.paint_glyph` + `PaintFuncs` to compose a COLRv1 colour glyph to SVG/raster; the shaping owner hands the shaped GIDs and the loaded `Font` to that rail rather than walking the paint graph twice.
- document embedding rail: the shaping owner subsets the document's used `glyph_set` via `subset` (with `RETAIN_GIDS` when the shaped GIDs must stay stable) before the subsetted `Face` bytes embed into the `pymupdf`/`reportlab` PDF — so the embedded font carries only the glyphs the shaped runs reference.

[RAIL_LAW]:
- Package: `uharfbuzz`
- Owns: OpenType text shaping; glyph advance/extents/origin/name queries; outline extraction; COLRv1 paint extraction and CPU rasterization; bitmap/SVG colour-glyph extraction; OpenType layout/math/variation/name-table introspection; font subsetting with closure/instancing; GSUB/GPOS table repacking
- Accept: the five-step shaping pipeline; `Font.draw_glyph_with_pen` for pen-protocol outline export; `Font.paint_glyph` for COLRv1; `subset`/`SubsetInput` for document font subsetting; `ot_*` introspection over a live face
- Reject: hand-rolled Unicode itemisation or OpenType feature application; re-implementing the shaping pipeline where `shape()` applies; a hand-rolled COLRv1 paint walker or subtable-overflow repacker where `paint_glyph`/`repack` own it; re-parsing layout/variation/name tables that the `ot_*`/`Face` introspection answers
