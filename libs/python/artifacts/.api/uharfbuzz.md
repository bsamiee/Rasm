# [PY_ARTIFACTS_API_UHARFBUZZ]

`uharfbuzz` owns OpenType text shaping and live font-table introspection for the artifacts text-shaping rail: it loads font data through `Blob`/`Face`/`Font`, shapes a `Buffer` into positioned glyphs, extracts outlines through `DrawFuncs` or a fontTools pen, walks the COLRv1 paint graph through `PaintFuncs`/`RasterPaint`, and drives the HarfBuzz subsetter and GSUB/GPOS repacker. Unicode itemisation, OpenType layout, COLRv1 paint composition, and subtable-overflow repacking stay in-package; full COLRv1 SVG composition routes to `blackrenderer` over this same paint surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `uharfbuzz`
- package: `uharfbuzz` (Apache-2.0)
- module: `uharfbuzz` — Cython binding exposing the `hb_*` API as classes and top-level functions over the bundled libharfbuzz (subsetter and GSUB/GPOS repacker included)
- owner: `artifacts`
- rail: text-shaping
- entry points: none (library only)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: font data, metric, and shaping-record objects

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                                         |
| :-----: | :---------------- | :------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `Blob`            | class         | `Blob.from_file_path(filename)`; `.data` bytes; `Face.reference_table` -> `Blob`     |
|  [02]   | `Face`            | class         | `Face.create(blob, index)`; introspection + subset source (`upem`/`glyph_count`/...) |
|  [03]   | `Font`            | class         | `Font.create(face)`; metric/glyph/outline/paint query surface                        |
|  [04]   | `FontExtents`     | struct        | `ascender`, `descender`, `line_gap`                                                  |
|  [05]   | `GlyphExtents`    | struct        | `x_bearing`, `y_bearing`, `width`, `height`                                          |
|  [06]   | `GlyphInfo`       | struct        | `codepoint` (GID), `cluster`, `flags` (`GlyphFlags`) from a shaped `Buffer`          |
|  [07]   | `GlyphPosition`   | struct        | `x_advance`, `y_advance`, `x_offset`, `y_offset`                                     |
|  [08]   | `GlyphFlags`      | flag enum     | `UNSAFE_TO_BREAK`, `UNSAFE_TO_CONCAT`, `SAFE_TO_INSERT_TATWEEL`                      |
|  [09]   | `HarfBuzzError`   | error         | raised by shaping                                                                    |
|  [10]   | `RepackerError`   | error         | raised by table repacking                                                            |
|  [11]   | `SerializerError` | error         | raised by serialization                                                              |
|  [12]   | `Map`             | container     | `Map.get/keys/values/items`                                                          |
|  [13]   | `Set`             | container     | `Set.add/add_range/update/intersection_update/invert/issubset`                       |

[PUBLIC_TYPE_SCOPE]: shaping buffer and feature objects

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :---------------------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `Buffer`                | class         | `Buffer.create()` Unicode container with direction/script/language/cluster_level    |
|  [02]   | `BufferClusterLevel`    | enum          | `MONOTONE_GRAPHEMES` (default) / `MONOTONE_CHARACTERS` / `GRAPHEMES` / `CHARACTERS` |
|  [03]   | `BufferContentType`     | enum          | `INVALID`, `UNICODE`, `GLYPHS`                                                      |
|  [04]   | `BufferFlags`           | flag enum     | `BOT`/`EOT`/`PRESERVE_DEFAULT_IGNORABLES`/`PRODUCE_UNSAFE_TO_CONCAT`/`VERIFY`/...   |
|  [05]   | `BufferSerializeFormat` | enum          | `TEXT`, `JSON`, `INVALID` output format for `Buffer.serialize`                      |
|  [06]   | `BufferSerializeFlags`  | flag enum     | `NO_CLUSTERS`/`NO_POSITIONS`/`NO_GLYPH_NAMES`/`GLYPH_EXTENTS`/`GLYPH_FLAGS`/...     |

[PUBLIC_TYPE_SCOPE]: outline, colour-paint, and raster objects

`DrawFuncs.set_{move_to,line_to,quadratic_to,cubic_to,close_path}_func` install the outline callbacks; `PaintFuncs.set_{color,color_glyph,linear_gradient,radial_gradient,sweep_gradient,image,push_clip_glyph,push_clip_rectangle,push_group,pop_group,push_transform,pop_transform,custom_palette_color,pop_clip}_func` install the COLRv1 paint callbacks; `RasterPaint` carries palette/foreground/background/transform/scale_factor.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                                             |
| :-----: | :------------------- | :------------ | :--------------------------------------------------------------------------------------- |
|  [01]   | `DrawFuncs`          | callback set  | `DrawFuncs.create()`; setter callbacks (lead); `draw_glyph`/`get_glyph_shape`            |
|  [02]   | `PaintFuncs`         | callback set  | `PaintFuncs.create()`; COLRv1 paint setter callbacks (in the lead)                       |
|  [03]   | `RasterPaint`        | class         | drives `PaintFuncs` to a `RasterImage`; `paint_glyph`/`render`                           |
|  [04]   | `RasterImage`        | struct        | `buffer`, `format` (`RasterFormat.BGRA32`/`A8`), `extents`                               |
|  [05]   | `RasterDraw`         | class         | renders a monochrome glyph outline to a `RasterImage` (A8)                               |
|  [06]   | `Color`              | value object  | RGBA colour value                                                                        |
|  [07]   | `ColorLine`          | struct        | gradient color-line                                                                      |
|  [08]   | `ColorStop`          | struct        | gradient stop (`offset`/`color`/`is_foreground`)                                         |
|  [09]   | `PaintExtend`        | enum          | gradient extend (`PAD`/`REPEAT`/`REFLECT`)                                               |
|  [10]   | `PaintCompositeMode` | enum          | 27-mode Porter-Duff/blend compositing                                                    |
|  [11]   | `FontFuncs`          | callback set  | override nominal-glyph/advance/name/var-glyph/extents callbacks via `FontFuncs.create()` |

[PUBLIC_TYPE_SCOPE]: OpenType introspection and subsetting objects

`SubsetInput` exposes the retention `Set`s `glyph_set`/`unicode_set`/`layout_feature_tag_set`/`layout_script_tag_set`/`name_id_set`/`name_lang_id_set`/`drop_table_tag_set`/`no_subset_table_tag_set` (or the generic `sets(SubsetInputSets)` selector), per-axis `pin_axis_to_default`/`pin_axis_location`/`set_axis_range`/`get_axis_range`/`pin_all_axes_to_default`, `keep_everything()`, and `.subset(face)`.

`SubsetInputSets` selects `UNICODE`/`GLYPH_INDEX`/`LAYOUT_FEATURE_TAG`/`LAYOUT_SCRIPT_TAG`/`NAME_ID`/`NAME_LANG_ID`/`NO_SUBSET_TABLE_TAG`/`DROP_TABLE_TAG`; `SubsetFlags` carries `RETAIN_GIDS`/`NO_HINTING`/`DESUBROUTINIZE`/`NAME_LEGACY`/`SET_OVERLAPS_FLAG`/`PASSTHROUGH_UNRECOGNIZED`/`NOTDEF_OUTLINE`/`GLYPH_NAMES`/`NO_PRUNE_UNICODE_RANGES`/`NO_LAYOUT_CLOSURE`.

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------------------- | :------------ | :------------------------------------ |
|  [01]   | `OTVarAxisInfo` / `OTVarNamedInstance` / `OTVarAxisFlags`             | struct / enum | axis info + `HIDDEN`; instance coords |
|  [02]   | `OTNameEntry` / `OTNameIdPredefined`                                  | struct / enum | name records; predefined name IDs     |
|  [03]   | `OTColorPalette` / `OTColorPaletteFlags` / `OTColorLayer` / `OTColor` | struct / enum | CPAL palette; COLRv0 layer            |
|  [04]   | `OTMathConstant` / `OTMathKern` / `OTMathKernEntry`                   | enum / struct | MATH constants + cut-in kerning       |
|  [05]   | `OTMathGlyphPart` / `OTMathGlyphPartFlags` / `OTMathGlyphVariant`     | struct / enum | assemblies + stretch variants         |
|  [06]   | `OTLayoutGlyphClass` / `OTMetricsTag` / `StyleTag`                    | enum          | GDEF class; metric + style tags       |

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :-------------------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `SubsetInput`                     | class         | retention `Set`s, axis pin/range, `keep_everything()`, `.subset(face)` |
|  [02]   | `SubsetInputSets` / `SubsetFlags` | enum          | set selector + retention flags (values above)                          |
|  [03]   | `SubsetPlan`                      | class         | `execute() -> Face`; new/old GID + codepoint remap maps                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: font loading

| [INDEX] | [SURFACE]                                  | [SHAPE] | [CAPABILITY]                                     |
| :-----: | :----------------------------------------- | :------ | :----------------------------------------------- |
|  [01]   | `Blob.from_file_path(filename)`            | factory | read font file bytes as a `Blob`                 |
|  [02]   | `Face.create(blob: bytes, index: int = 0)` | factory | construct a `Face` from raw bytes or TTC index   |
|  [03]   | `Face.create_for_tables(func, user_data)`  | factory | create a `Face` backed by a table-fetch callback |
|  [04]   | `Font.create(face: Face)`                  | factory | create a scaled `Font` from a `Face`             |
|  [05]   | `version_string()`                         | static  | linked libharfbuzz version string                |

[ENTRYPOINT_SCOPE]: text input and shaping

`Buffer.serialize(font, format=BufferSerializeFormat.TEXT, flags=BufferSerializeFlags.DEFAULT) -> str` emits the shaped glyph sequence; the `add_*` inputs share `item_offset=0, item_length=-1`.

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Buffer.create()`                                                      | factory  | allocate an empty shaping buffer              |
|  [02]   | `Buffer.add_str(text: str, ...)`                                       | instance | add Python `str` text to the buffer           |
|  [03]   | `Buffer.add_codepoints(codepoints: list[int], ...)`                    | instance | add a list of Unicode codepoints              |
|  [04]   | `Buffer.add_utf8(text: bytes, ...)`                                    | instance | add `bytes` UTF-8 text                        |
|  [05]   | `Buffer.guess_segment_properties()`                                    | instance | infer direction/script/language from content  |
|  [06]   | `Buffer.set_script_from_ot_tag(tag)` / `set_language_from_ot_tag(tag)` | instance | set script/language from an OT tag            |
|  [07]   | `Buffer.set_message_func(fn)`                                          | instance | install a shaping-trace message callback      |
|  [08]   | `Buffer.reset()` / `clear_contents()`                                  | instance | reset / clear buffer contents                 |
|  [09]   | `Buffer.not_found_glyph` / `invisible_glyph` / `replacement_codepoint` | property | override .notdef/invisible/replacement glyph  |
|  [10]   | `shape(font, buffer, features=None, shapers=None)`                     | static   | run OpenType shaping; mutates buffer in place |
|  [11]   | `Buffer.serialize(font, format=, flags=) -> str`                       | instance | human/JSON-readable shaped glyph sequence     |
|  [12]   | `Buffer.glyph_infos` / `Buffer.glyph_positions`                        | property | `list[GlyphInfo]` / `list[GlyphPosition]`     |

[ENTRYPOINT_SCOPE]: glyph metric, outline, and colour queries

`get_glyph_h_advance`/`get_glyph_v_advance` and the `*_h_origin`/`*_v_origin` pair give both writing directions; `paint_glyph` is the COLRv1 entry; `get_glyph_color_png`/`Face.get_glyph_color_svg` / `ot_color_glyph_get_png`/`ot_color_glyph_get_svg` are the bitmap/SVG colour-glyph extractors. Full signatures: `Font.draw_glyph(gid, draw_funcs: DrawFuncs, draw_state=None)`, `Font.paint_glyph(gid, paint_funcs: PaintFuncs, paint_state=None, palette_index=0, foreground=None)`, `Font.set_variations(variations: dict[str, float])`.

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :---------------------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `Font.get_glyph_h_advance(gid)` / `get_glyph_v_advance(gid)`      | instance | horizontal/vertical advance in font units           |
|  [02]   | `Font.get_glyph_extents(gid)`                                     | instance | `GlyphExtents` bounding box                         |
|  [03]   | `Font.get_font_extents(direction)`                                | instance | `FontExtents` ascender/descender/line-gap           |
|  [04]   | `Font.get_metric_position(OTMetricsTag)`                          | instance | named OS/2 metric position                          |
|  [05]   | `Font.get_metric_position_with_fallback(tag)`                     | instance | metric position with fallback                       |
|  [06]   | `Font.get_metric_variation(tag)`                                  | instance | per-axis metric variation delta                     |
|  [07]   | `Font.get_metric_x_variation` / `get_metric_y_variation`          | instance | x/y metric variation deltas                         |
|  [08]   | `Font.get_style_value(StyleTag)`                                  | instance | style-attribute value by `StyleTag`                 |
|  [09]   | `Font.get_nominal_glyph(unicode)` / `get_variation_glyph(u, vs)`  | instance | GID for a codepoint (and UVS form)                  |
|  [10]   | `Font.get_glyph_h_origin(gid)` / `get_glyph_v_origin(gid)`        | instance | per-direction glyph origin                          |
|  [11]   | `Font.get_glyph_name(gid)` / `get_glyph_from_name(name)`          | instance | GID<->name                                          |
|  [12]   | `Font.glyph_to_string(gid)` / `glyph_from_string(s)`              | instance | GID<->`gidNNN`/name string                          |
|  [13]   | `Font.draw_glyph_with_pen(gid, pen)`                              | instance | drive a fontTools-compatible pen                    |
|  [14]   | `Font.draw_glyph(gid, draw_funcs, ...)`                           | instance | dispatch outline to `DrawFuncs` (signature in lead) |
|  [15]   | `Font.paint_glyph(gid, paint_funcs, ...)`                         | instance | drive COLRv1 paint callbacks (signature in lead)    |
|  [16]   | `Font.get_glyph_color_png(glyph)` (free `ot_color_glyph_get_png`) | instance | CBDT/sbix PNG `Blob`                                |
|  [17]   | `Face.get_glyph_color_svg(glyph)` (free `..._get_svg`)            | instance | SVG `Blob`                                          |
|  [18]   | `Face.get_glyph_color_layers(glyph)` (free `..._get_layers`)      | instance | COLRv0 layer list                                   |
|  [19]   | `Font.set_variation(name, value)` / `set_variations(...)`         | instance | set axis positions (tag-keyed / design-space)       |
|  [20]   | `Font.set_var_coords_design(coords)`                              | instance | set design-space coords                             |
|  [21]   | `Font.set_var_coords_normalized(coords)`                          | instance | set normalised coords                               |
|  [22]   | `Font.get_var_coords_design()` / `get_var_coords_normalized()`    | instance | read design / normalised coords                     |
|  [23]   | `Font.var_named_instance`                                         | property | pin to a named instance by index                    |
|  [24]   | `Font.synthetic_bold` / `synthetic_slant`                         | property | synthetic emboldening / slant                       |
|  [25]   | `Font.ptem` / `ppem` / `scale`                                    | property | point size, ppem, integer scale                     |

[ENTRYPOINT_SCOPE]: OpenType introspection

Each `ot_*(face|font, ...)` free function mirrors a bound method on the object it queries — `ot_layout_table_get_script_tags(face, ...)` == `face.get_table_script_tags(...)`, `ot_math_get_constant(font, c)` == `font.get_math_constant(c)`, `ot_color_has_paint(face)` == `face.has_color_paint` — the bound form the canonical owner-side call, the free function the boundary form. Both read the live layout/variation/math/colour tables instead of re-parsing GSUB/GPOS/MATH/fvar/CPAL.

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `Face.axis_infos`                                                | property | `list[OTVarAxisInfo]` axes                  |
|  [02]   | `Face.named_instances`                                           | property | `list[OTVarNamedInstance]` named instances  |
|  [03]   | `Face.variation_selectors`                                       | property | UVS selectors                               |
|  [04]   | `Face.unicodes` / `Face.table_tags`                              | property | covered codepoint `Set`; present table tags |
|  [05]   | `Face.list_names()` / `Face.get_name(name_id, ...)`              | instance | enumerate/read OpenType name records        |
|  [06]   | `Face.color_palettes`                                            | property | CPAL palettes                               |
|  [07]   | `Face.has_color_paint` / `has_color_png` / `has_color_svg`       | property | COLRv1/bitmap/SVG probes                    |
|  [08]   | `Face.has_color_layers` / `has_color_palettes`                   | property | COLRv0/CPAL probes                          |
|  [09]   | `Face.get_table_script_tags` / `get_script_language_tags`        | instance | GSUB/GPOS script + language tags            |
|  [10]   | `Face.get_language_feature_tags` / `get_lookup_glyph_alternates` | instance | feature tags; per-lookup glyph alternates   |
|  [11]   | `Face.get_layout_glyph_class(glyph)`                             | instance | GDEF glyph class                            |
|  [12]   | `Face.has_layout_positioning` / `has_layout_substitution`        | property | GSUB/GPOS presence probes                   |
|  [13]   | `Face.has_layout_glyph_classes`                                  | property | GDEF glyph-class presence                   |
|  [14]   | `Font.get_layout_baseline(...)`                                  | instance | baseline position by tag/direction/script   |
|  [15]   | `Face.has_math_data`                                             | property | MATH-table presence                         |
|  [16]   | `Font.get_math_constant(OTMathConstant)`                         | instance | MATH-table constants                        |
|  [17]   | `Font.get_math_glyph_variants` / `get_math_glyph_assembly`       | instance | stretch variants and assemblies             |
|  [18]   | `Font.get_math_glyph_italics_correction`                         | instance | italics correction                          |
|  [19]   | `Font.get_math_glyph_top_accent_attachment`                      | instance | top-accent attachment                       |
|  [20]   | `Font.get_math_glyph_kernings`                                   | instance | cut-in kerning                              |
|  [21]   | `Font.get_math_min_connector_overlap`                            | instance | min connector overlap                       |
|  [22]   | `Face.is_glyph_extended_math_shape(glyph)`                       | instance | extended math-shape flag                    |
|  [23]   | `ot_tag_to_script(tag)` / `ot_tag_to_language(tag)`              | static   | OT tag to BCP-47/ISO resolution             |

[ENTRYPOINT_SCOPE]: subsetting and table repacking

`SubsetInput` is the closure surface: mutate the `glyph_set`/`unicode_set` (`Set` containers), select retention via `flags`/`SubsetInputSets`, pin or range-restrict variation axes, then `subset(face, input)` or `input.subset(face)`; `SubsetPlan` exposes the GID remapping. `SubsetInput.set_axis_range(face, tag, min_value=None, max_value=None, def_value=None)` range-restricts an axis; `repack_with_tag`/`serialize_with_tag` are the tagged repacker variants of the GSUB/GPOS overflow-resolving repacker.

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `SubsetInput()`                                                      | ctor     | allocate subsetting parameter object            |
|  [02]   | `SubsetInput.glyph_set` / `unicode_set`                              | property | GID / codepoint `Set`                           |
|  [03]   | `SubsetInput.layout_feature_tag_set` / `name_id_set`                 | property | layout-feature-tag / name-ID `Set`              |
|  [04]   | `SubsetInput.sets(SubsetInputSets)`                                  | instance | generic `Set` accessor by selector              |
|  [05]   | `SubsetInput.flags` (`SubsetFlags`)                                  | property | retention flags (see `SubsetFlags`)             |
|  [06]   | `SubsetInput.keep_everything()`                                      | instance | retain all glyphs and tables                    |
|  [07]   | `SubsetInput.pin_axis_to_default(face, tag)`                         | instance | instance out an axis at its default             |
|  [08]   | `SubsetInput.pin_all_axes_to_default(face)`                          | instance | instance out every axis at default              |
|  [09]   | `SubsetInput.pin_axis_location(face, tag, value)`                    | instance | pin an axis to a value                          |
|  [10]   | `SubsetInput.set_axis_range(face, tag, ...)`                         | instance | range-restrict an axis (partial instancing)     |
|  [11]   | `subset_preprocess(face)`                                            | static   | prepare a `Face` for faster repeated subsetting |
|  [12]   | `subset(face, input)` / `SubsetInput.subset(face)`                   | static   | produce a subsetted `Face`                      |
|  [13]   | `SubsetPlan.execute()`                                               | instance | run a plan -> `Face`                            |
|  [14]   | `SubsetPlan.new_to_old_glyph_mapping`                                | property | new -> old GID map                              |
|  [15]   | `SubsetPlan.old_to_new_glyph_mapping`                                | property | old -> new GID map                              |
|  [16]   | `SubsetPlan.unicode_to_old_glyph_mapping`                            | property | codepoint -> old-GID map                        |
|  [17]   | `repack(subtables, graphnodes)` / `serialize(subtables, graphnodes)` | static   | resolve GSUB/GPOS overflows; serialize bytes    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- shaping pipeline: `Blob.from_file_path` -> `Face.create` -> `Font.create` -> `Buffer.create` -> `Buffer.add_str`/`add_codepoints` -> `Buffer.guess_segment_properties` -> `shape(font, buffer, features)` -> read `buffer.glyph_infos` / `buffer.glyph_positions`.
- properties: `Buffer.direction`/`script`/`language`/`cluster_level`/`flags` are settable; `guess_segment_properties()` runs before `shape` only when direction/script/language is unset.
- features: the `features` argument to `shape` takes `dict[str, int | bool | Sequence[tuple[int, int, int | bool]]]` for whole-buffer or cluster-ranged toggles; `shapers` pins the shaper backend list.
- break safety: `GlyphInfo.flags & GlyphFlags.UNSAFE_TO_BREAK`/`UNSAFE_TO_CONCAT` is the signal a layout owner reads to decide where a shaped run splits or reshapes.
- outline extraction: `Font.draw_glyph_with_pen(gid, pen)` drives a fontTools pen; `Font.draw_glyph(gid, DrawFuncs, state)` drives raw callbacks at boundary scope only.
- colour extraction: `Font.paint_glyph(gid, PaintFuncs, ...)` walks the COLRv1 paint graph (gradients via `ColorLine`/`PaintExtend`, compositing via `PaintCompositeMode`, clips/transforms/groups); `RasterPaint`/`RasterImage` rasterizes to a CPU BGRA32/A8 bitmap; `get_glyph_color_png`/`ot_color_glyph_get_svg`/`ot_color_glyph_get_layers` cover CBDT/sbix-PNG, SVG, and COLRv0-layer formats, and `ot_color_has_paint`/`has_png`/`has_svg`/`has_layers` probe which a face carries.
- variation: `Font.set_variation(name, value)` / `set_variations({tag: value})` / `set_var_coords_normalized(coords)` set axis positions; `Face.axis_infos`/`named_instances` enumerate the design space.

[STACKING]:
- `fontTools`(`.api/fonttools.md`): `shape` emits GIDs + positions and `Font.draw_glyph_with_pen` feeds a fontTools pen, meeting at the pen protocol and the `Face`/`TTFont` byte boundary; `fontTools` owns the binary font model and designspace compile, this surface owns the layout engine, live-table introspection (`ot_layout_*`/`axis_infos`), and the production subsetter/repacker.
- `blackrenderer`(`.api/blackrenderer.md`): the shaped GIDs and loaded `Font` hand to `blackrenderer`, which drives `Font.paint_glyph` + `PaintFuncs` to compose a COLRv1 glyph to SVG/raster rather than walking the paint graph twice.
- `pymupdf`/`reportlab`(`.api/pymupdf.md`): the document's used `glyph_set` subsets through `subset` (with `RETAIN_GIDS` when shaped GIDs must stay stable) before the subsetted `Face` bytes embed into the PDF, so the font carries only glyphs the shaped runs reference.
- within-lib: `typography/shape` runs the five-step pipeline, `typography/font` drives `subset`/`SubsetInput` for document embedding, and COLRv1 rasterization composes `blackrenderer` over `paint_glyph`.

[LOCAL_ADMISSION]:
- `Font.draw_glyph_with_pen` bridges to `fontTools.pens` for outline extraction; raw `DrawFuncs` callbacks are boundary use only.
- COLRv1 extraction routes through `Font.paint_glyph` + `PaintFuncs`; the in-process `RasterPaint`/`RasterImage` rasterizer serves only a CPU bitmap inside the shaping owner, and full colour-glyph SVG composition belongs to `blackrenderer` over this same paint surface.
- subsetting runs through `uharfbuzz.subset` (set `flags`/`glyph_set`/`unicode_set`, pin/range axes, then `subset`); `SubsetPlan` is taken when the GID remapping must be read.
- `repack`/`serialize` resolves overflows for GSUB/GPOS subtable graphs synthesized by a higher tier; it is boundary use only.

[RAIL_LAW]:
- Package: `uharfbuzz`
- Owns: OpenType text shaping; glyph advance/extents/origin/name queries in both axes; outline extraction; COLRv1 paint extraction and CPU rasterization; bitmap/SVG colour-glyph extraction; OpenType layout/math/variation/name-table introspection; font subsetting with closure/instancing; GSUB/GPOS table repacking with overflow resolution
- Accept: the five-step shaping pipeline; `Font.draw_glyph_with_pen` for pen-protocol outline export; `Font.paint_glyph` for COLRv1; `subset`/`SubsetInput` for document font subsetting; `ot_*` introspection over a live face
- Reject: hand-rolled Unicode itemisation or OpenType feature application; re-implementing the pipeline where `shape()` applies; a hand-rolled COLRv1 paint walker or subtable-overflow repacker where `paint_glyph`/`repack` own it; re-parsing layout/variation/name tables the `ot_*`/`Face` introspection answers
