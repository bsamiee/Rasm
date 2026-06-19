# [PY_ARTIFACTS_API_UHARFBUZZ]

`uharfbuzz` supplies the Python binding to HarfBuzz for the artifacts text-shaping rail: `Blob` and `Face` for font data loading, `Font` for metric and glyph queries, `Buffer` for Unicode text input, `shape` for OpenType layout shaping, `DrawFuncs` and `PaintFuncs` for glyph outline and colour extraction, and `SubsetInput` + `subset` for font subsetting. The package owner composes these surfaces into a shaping pipeline and never re-implements Unicode itemisation or OpenType layout logic that HarfBuzz owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `uharfbuzz`
- package: `uharfbuzz`
- import: `uharfbuzz`
- owner: `artifacts`
- rail: text-shaping
- asset: runtime library (Cython extension over libharfbuzz)
- installed: `0.55.0` reflected via `python -c "import uharfbuzz"` on cp315
- entry points: none (library only)
- capability: OpenType text shaping, glyph advance/extents/name queries, glyph outline extraction, COLRv1 paint callbacks, font subsetting via HarfBuzz

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: font data and metric objects
- rail: text-shaping

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE]       | [CAPABILITY]                                                                              |
| :-----: | :-------------- | :------------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `Blob`          | raw font data        | `Blob.from_file_path(filename)` — load raw bytes; `Face.reference_table` returns a `Blob` |
|  [02]   | `Face`          | font face            | `Face.create(blob, index)` — create from `bytes` or index into a TTC                      |
|  [03]   | `Font`          | scaled font          | `Font.create(face)` — metric and glyph query surface over a `Face`                        |
|  [04]   | `FontExtents`   | font-level metrics   | named tuple carrying ascender, descender, line-gap fields                                 |
|  [05]   | `GlyphExtents`  | glyph bounding box   | x\_bearing, y\_bearing, width, height fields                                              |
|  [06]   | `GlyphInfo`     | shaping glyph record | codepoint (GID) and cluster index from a shaped `Buffer`                                  |
|  [07]   | `GlyphPosition` | shaping position     | x\_advance, y\_advance, x\_offset, y\_offset from a shaped `Buffer`                       |

[PUBLIC_TYPE_SCOPE]: shaping buffer and feature objects
- rail: text-shaping

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE] | [CAPABILITY]                                                              |
| :-----: | :---------------------- | :------------- | :------------------------------------------------------------------------ |
|  [01]   | `Buffer`                | shaping buffer | `Buffer.create()` — Unicode text container with direction/script/language |
|  [02]   | `BufferClusterLevel`    | cluster enum   | `DEFAULT` / `MONOTONE_GRAPHEMES` / `MONOTONE_CHARACTERS` / `CHARACTERS`   |
|  [03]   | `BufferContentType`     | content enum   | `INVALID`, `UNICODE`, `GLYPHS`                                            |
|  [04]   | `BufferFlags`           | flags enum     | shaping buffer behaviour flags                                            |
|  [05]   | `BufferSerializeFormat` | serialize enum | `TEXT`, `JSON` output format for `Buffer.serialize`                       |

[PUBLIC_TYPE_SCOPE]: outline, paint, and subset objects
- rail: text-shaping

| [INDEX] | [SYMBOL]      | [PACKAGE_ROLE]     | [CAPABILITY]                                                            |
| :-----: | :------------ | :----------------- | :---------------------------------------------------------------------- |
|  [01]   | `DrawFuncs`   | outline extractor  | `DrawFuncs.create()` — set callback functions for move/line/curve/close |
|  [02]   | `PaintFuncs`  | colour paint funcs | COLRv1 paint callback surface                                           |
|  [03]   | `FontFuncs`   | custom font funcs  | `FontFuncs.create()` — override glyph advance/name/nominal-glyph funcs  |
|  [04]   | `SubsetInput` | subset parameters  | `SubsetInput()` — unicode sets, glyph sets, axis ranges for subsetting  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: font loading
- rail: text-shaping

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [CAPABILITY]                                     |
| :-----: | :----------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `Blob.from_file_path(filename)`            | file load      | read font file bytes as a `Blob`                 |
|  [02]   | `Face.create(blob: bytes, index: int = 0)` | face creation  | construct a `Face` from raw bytes or TTC index   |
|  [03]   | `Face.create_for_tables(func, user_data)`  | virtual face   | create a `Face` backed by a table-fetch callback |
|  [04]   | `Font.create(face: Face)`                  | font creation  | create a scaled `Font` from a `Face`             |

[ENTRYPOINT_SCOPE]: text input and shaping
- rail: text-shaping

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY]  | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------ | :-------------- | :---------------------------------------------------- |
|  [01]   | `Buffer.create()`                                             | buffer init     | allocate an empty shaping buffer                      |
|  [02]   | `Buffer.add_str(text, item_offset, item_length)`              | Unicode input   | add Python `str` text to the buffer                   |
|  [03]   | `Buffer.add_codepoints(codepoints, item_offset, item_length)` | codepoint input | add a list of Unicode codepoints                      |
|  [04]   | `Buffer.add_utf8(text, item_offset, item_length)`             | UTF-8 input     | add `bytes` UTF-8 text                                |
|  [05]   | `Buffer.guess_segment_properties()`                           | auto properties | infer direction, script, language from buffer content |
|  [06]   | `shape(font, buffer, features, shapers)`                      | shaping entry   | run OpenType Layout shaping; mutates buffer in-place  |
|  [07]   | `Buffer.serialize(font, format, flags)`                       | result text     | human/JSON-readable shaped glyph sequence             |

[ENTRYPOINT_SCOPE]: glyph metric and outline queries
- rail: text-shaping

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------ | :------------- | :-------------------------------------------------- |
|  [01]   | `Font.get_glyph_h_advance(gid)`                               | advance query  | horizontal advance in font units                    |
|  [02]   | `Font.get_glyph_extents(gid)`                                 | bbox query     | `GlyphExtents` bounding box                         |
|  [03]   | `Font.get_font_extents(direction)`                            | font metrics   | `FontExtents` ascender/descender/line-gap           |
|  [04]   | `Font.get_nominal_glyph(unicode)`                             | codepoint map  | GID for a Unicode codepoint                         |
|  [05]   | `Font.get_glyph_name(gid)` / `Font.get_glyph_from_name(name)` | name map       | GID↔name bidirectional lookup                       |
|  [06]   | `Font.draw_glyph_with_pen(gid, pen)`                          | outline export | drive a fontTools-compatible pen with glyph outline |
|  [07]   | `Font.draw_glyph(gid, draw_funcs, draw_state)`                | raw draw       | dispatch outline to `DrawFuncs` callbacks           |
|  [08]   | `Font.set_variations(variations: dict[str, float])`           | axis setting   | set variation axis values by tag                    |

[ENTRYPOINT_SCOPE]: subsetting
- rail: text-shaping

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY]  | [CAPABILITY]                                    |
| :-----: | :------------------------------------------ | :-------------- | :---------------------------------------------- |
|  [01]   | `SubsetInput()`                             | parameters init | allocate subsetting parameter object            |
|  [02]   | `SubsetInput.keep_everything()`             | full retain     | configure to retain all glyphs and tables       |
|  [03]   | `SubsetInput.pin_all_axes_to_default(face)` | axis pin        | pin all variation axes to their default values  |
|  [04]   | `subset_preprocess(face)`                   | preprocessing   | prepare a `Face` for faster repeated subsetting |
|  [05]   | `subset(face, input)`                       | subset execute  | produce a subsetted `Face` from `SubsetInput`   |

## [04]-[IMPLEMENTATION_LAW]

[SHAPING_TOPOLOGY]:
- namespace: `uharfbuzz`; Cython binding exposing `hb_*` API as Python classes and top-level functions
- shaping pipeline: `Blob.from_file_path` → `Face.create` → `Font.create` → `Buffer.create` → `Buffer.add_str` / `add_codepoints` → `Buffer.guess_segment_properties` → `shape(font, buffer, features)` → read `buffer.glyph_infos` / `buffer.glyph_positions`
- properties: `Buffer.direction`, `Buffer.script`, `Buffer.language`, `Buffer.cluster_level`, `Buffer.flags` are settable string/enum properties
- features: the `features` parameter to `shape` accepts `dict[str, int | bool | Sequence[tuple[int, int, int | bool]]]` for feature ranges
- outline extraction: `Font.draw_glyph_with_pen(gid, pen)` drives a fontTools-compatible pen; `Font.draw_glyph(gid, DrawFuncs, state)` drives raw callbacks
- variation: `Font.set_variation(name, value)` or `Font.set_variations({tag: value})` set axis positions; `Font.set_var_coords_normalized(coords)` sets normalised coords

[LOCAL_ADMISSION]:
- shaping uses the five-step pipeline above; `Buffer.guess_segment_properties()` is called before `shape` when explicit script/language/direction is not set.
- `Font.draw_glyph_with_pen` bridges to `fontTools.pens` for outline extraction; raw `DrawFuncs` callbacks are boundary use only.
- subsetting via `uharfbuzz.subset` is an alternative path; the `fonttools.subset.Subsetter` path remains available for feature-policy control.
- `buffer.glyph_infos` and `buffer.glyph_positions` are read after `shape`; they are lists of `GlyphInfo` and `GlyphPosition` named tuples.

[RAIL_LAW]:
- Package: `uharfbuzz`
- Owns: OpenType text shaping, glyph advance/extents/name queries, glyph outline extraction, and font-level subsetting via HarfBuzz
- Accept: the five-step shaping pipeline; `Font.draw_glyph_with_pen` for pen-protocol outline export
- Reject: hand-rolled Unicode itemisation or OpenType feature application; re-implementing the shaping pipeline where `shape()` applies
