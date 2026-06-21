# [PY_ARTIFACTS_API_FONTTOOLS]

`fontTools` supplies the OpenType/TrueType font model for the artifacts font rail: `TTFont` as the binary-font owner, the `pens` protocol family as the outline read/write algebra, `varLib`/`varLib.instancer` as the variable-font compile-and-instance engine, `subset` as the glyph-set/feature/table pruning engine, `feaLib` as the OpenType Layout feature compiler, `fontBuilder.FontBuilder` as the high-level synthesis owner, `merge.Merger` as the multi-font combiner, `cu2qu`/`qu2cu` as the cubic-quadratic outline converter, and `unicodedata` as the script/OT-tag resolver. The package owner drives all font IO, outline construction, instancing, subsetting, and feature compilation through these surfaces and never re-parses sfnt tables, hand-prunes glyph entries, or hand-codes the script-to-OT-tag map.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fontTools`
- package: `fonttools`
- import: `fontTools`
- owner: `artifacts`
- rail: fonts
- license: MIT
- asset: runtime library; pure Python (`py3-none-any`), no ABI gate, cp315-clean (manifest floor `fonttools>=4.63.0`, no `python_version` marker)
- installed: `4.63.0` reflected via `assay api resolve fonttools`
- entry points: `fonttools`, `ttx`, `pyftsubset`, `pyftmerge` console scripts; library use is import-only
- capability: read/write OTF/TTF/WOFF/WOFF2/TTC binary fonts; lazy table decompilation by tag; outline read/write through a pen protocol; variable-font compilation from a designspace and partial/full instancing; glyph-set/feature/name/table subsetting with WOFF re-flavoring; OpenType Layout feature compilation from `.fea`; from-scratch font synthesis; multi-font merge; cubic-quadratic outline conversion; Unicode script/block/OT-tag resolution

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: binary font container
- rail: fonts

`TTFont` is the single binary-font owner: `font[tag]` returns the parsed table object (lazy-decompiled on first access when `lazy=True`), `font.keys()` enumerates present tags, `font.flavor` is `None`/`"woff"`/`"woff2"`. `ttLib.newTable(tag)` is a module function (not a `TTFont` method) that allocates an empty table; `TTCollection` owns `.ttc`/`.otc` font collections.

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE] | [CAPABILITY]                                                                            |
| :-----: | :---------------------------------- | :------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `fontTools.ttLib.TTFont`            | font container | sfnt/WOFF/WOFF2 read-write container; `font[tag]` table access, `lazy`/`cfg`/`flavor` policy |
|  [02]   | `fontTools.ttLib.TTCollection`      | font collection | `(file, shareTables=False)` — TTC/OTC collection of `TTFont` members                    |
|  [03]   | `fontTools.ttLib.TTLibError`        | font fault     | binary-font IO/parse error rail                                                          |

[PUBLIC_TYPE_SCOPE]: glyph pen protocol (`fontTools.pens`)
- rail: fonts

Pens are the outline read/write algebra: `TTFont.getGlyphSet()[name].draw(pen)` (segment protocol) or `.drawPoints(pen)` (point protocol) replays a glyph into any pen; pens compose by forwarding. The segment protocol is `moveTo`/`lineTo`/`qCurveTo`/`curveTo`/`closePath`/`endPath`/`addComponent`.

| [INDEX] | [SYMBOL]                                      | [PACKAGE_ROLE]    | [CAPABILITY]                                                                  |
| :-----: | :-------------------------------------------- | :---------------- | :---------------------------------------------------------------------------- |
|  [01]   | `fontTools.pens.basePen.BasePen`              | outline pen base  | abstract segment receiver; `qCurveTo` decomposes to `_curveToOne` for cubic-only pens |
|  [02]   | `fontTools.pens.pointPen.AbstractPointPen`    | point pen base    | `beginPath`/`addPoint`/`endPath`/`addComponent` point protocol                |
|  [03]   | `fontTools.pens.recordingPen.RecordingPen`    | recording pen     | capture segment calls into `.value`, replay via `.replay(pen)`                |
|  [04]   | `fontTools.pens.recordingPen.DecomposingRecordingPen` | flattening recorder | record with component references decomposed to outlines                 |
|  [05]   | `fontTools.pens.pointPen.PointToSegmentPen` / `SegmentToPointPen` | protocol adapter | convert between point and segment pen protocols                      |
|  [06]   | `fontTools.pens.svgPathPen.SVGPathPen`        | SVG path pen      | `(glyphSet, ntos=str)` — emit SVG `d` path via `.getCommands()`               |
|  [07]   | `fontTools.pens.transformPen.TransformPen`    | transform pen     | `(outPen, transformation)` — apply affine while forwarding segments           |
|  [08]   | `fontTools.pens.boundsPen.BoundsPen` / `ControlBoundsPen` | bounds pen | compute exact / control-point glyph bounding box                              |
|  [09]   | `fontTools.pens.statisticsPen.StatisticsPen`  | moments pen       | glyph area/moments/mean for outline-quality metrics                           |
|  [10]   | `fontTools.pens.areaPen.AreaPen`              | area pen          | signed contour area                                                           |
|  [11]   | `fontTools.pens.ttGlyphPen.TTGlyphPen`        | TrueType pen      | `(glyphSet, handleOverflowingTransforms, outputImpliedClosingLine)` — build a `glyf` `Glyph` via `.glyph(...)` |
|  [12]   | `fontTools.pens.t2CharStringPen.T2CharStringPen` | CFF pen        | `(width, glyphSet, roundTolerance=0.5, CFF2=False)` — emit Type 2 charstring  |
|  [13]   | `fontTools.pens.cairoPen.CairoPen`            | cairo pen         | draw glyph into a cairo context (optional cairocffi)                          |

[PUBLIC_TYPE_SCOPE]: variable-font, subset, synthesis, and merge engines
- rail: fonts

| [INDEX] | [SYMBOL]                                       | [PACKAGE_ROLE]    | [CAPABILITY]                                                          |
| :-----: | :--------------------------------------------- | :---------------- | :-------------------------------------------------------------------- |
|  [01]   | `fontTools.designspaceLib.DesignSpaceDocument` | varfont document  | axes/sources/instances container; `.read(path)`/`.write(path)`/`.addAxis`/`.addSource`/`.addInstance` |
|  [02]   | `fontTools.designspaceLib.AxisDescriptor`      | axis record       | `(tag, name, minimum, default, maximum, map, …)` — one variation axis  |
|  [03]   | `fontTools.designspaceLib.SourceDescriptor`    | source record     | `(filename, path, font, name, location, designLocation, …)` — one master |
|  [04]   | `fontTools.designspaceLib.InstanceDescriptor`  | instance record   | `(filename, path, font, name, location, locationLabel, …)` — one static cut |
|  [05]   | `fontTools.subset.Subsetter`                   | subsetter engine  | `(options)` — `populate(...)` then `subset(font)`; in-place table pruning |
|  [06]   | `fontTools.subset.Options`                     | subset policy     | `(**kwargs)` — `layout_features`/`name_IDs`/`hinting`/`flavor`/`retain_gids`/`glyph_names`/`desubroutinize`/`drop_tables`/`harfbuzz_repacker`/`with_zopfli` |
|  [07]   | `fontTools.fontBuilder.FontBuilder`            | synthesis owner   | `(unitsPerEm, isTTF, glyphDataFormat)` — high-level `setup*` table family then `.font` |
|  [08]   | `fontTools.merge.Merger`                       | font combiner     | `(options)` — `merge(fontfiles)` combining tables across fonts        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: font container IO and glyph access
- rail: fonts

`TTFont(path, lazy=True)` opens read-mostly; `flavor="woff2"` selects WOFF2 output before `save`. `getGlyphSet()` is the read-path glyph factory whose members `.draw(pen)`/`.drawPoints(pen)` feed the pen family; `getBestCmap()` resolves the Unicode-to-glyph map for shaping/subset input.

| [INDEX] | [SURFACE]                                                                                                 | [ENTRY_FAMILY]    | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------------------------------------------------------------- | :---------------- | :---------------------------------------------------- |
|  [01]   | `TTFont(file=None, sfntVersion='\x00\x01\x00\x00', flavor=None, recalcBBoxes=True, recalcTimestamp=True, lazy=None, fontNumber=-1, cfg={})` | construction      | open or create a font; `flavor`/`lazy`/`cfg` policy   |
|  [02]   | `TTFont.save(file, reorderTables=True)`                                                                   | binary export     | write OTF/TTF/WOFF/WOFF2 to path or buffer            |
|  [03]   | `TTFont.saveXML(fileOrPath, newlinestr='\n', **XMLSavingOptions)` / `TTFont.importXML(fileOrPath)`        | TTX round-trip    | write/read the TTX XML representation                 |
|  [04]   | `TTFont.getGlyphSet(preferCFF=True, location=None, normalized=False, recalcBounds=True)`                  | glyph factory     | glyph set whose members expose `.draw(pen)`/`.drawPoints(pen)`; `location` cuts a variable instance |
|  [05]   | `TTFont.getBestCmap(cmapPreferences=…)`                                                                   | unicode map       | `{codepoint: glyphName}` from the best `cmap` subtable |
|  [06]   | `TTFont.getGlyphOrder()` / `setGlyphOrder(order)` / `getReverseGlyphMap(rebuild=False)`                   | glyph order       | ordered glyph names and `{name: GID}` map             |
|  [07]   | `TTFont.getGlyphName(glyphID)` / `getGlyphID(glyphName)`                                                  | name-gid resolve  | bidirectional glyph-name/GID lookup                   |
|  [08]   | `TTFont.keys()` / `TTFont[tag]` / `fontTools.ttLib.newTable(tag)`                                          | table access      | enumerate tags, fetch a parsed table, allocate a new one |

[ENTRYPOINT_SCOPE]: variable-font compile and instance
- rail: fonts

`varLib.build` compiles a variable font from a designspace; `varLib.instancer.instantiateVariableFont` is the partial/full instancing engine — limit a subset of axes to pin a static or reduced-axis cut that downstream shaping/subsetting consumes.

| [INDEX] | [SURFACE]                                                                                              | [ENTRY_FAMILY]    | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------------------------------------------------------------- | :---------------- | :--------------------------------------------------- |
|  [01]   | `varLib.build(designspace, master_finder=…, exclude=[], optimize=True, colr_layer_reuse=True, drop_implied_oncurves=False)` | varfont compile   | build a variable `TTFont` (+ model) from a designspace |
|  [02]   | `varLib.instancer.instantiateVariableFont(varfont, axisLimits, inplace=False, optimize=True, overlap=OverlapMode.KEEP_AND_SET_FLAGS, updateFontNames=False, *, downgradeCFF2=False, static=False)` | instancing        | pin/limit axes to a static or reduced-axis instance  |
|  [03]   | `DesignSpaceDocument.read(path)` / `.write(path)` / `.addAxis(a)` / `.addSource(s)` / `.addInstance(i)` | designspace IO    | read/build the axes-sources-instances project        |

[ENTRYPOINT_SCOPE]: subsetting and feature compilation
- rail: fonts

`Subsetter` prunes a `TTFont` in place to the populated glyph/Unicode set before `save`; `Options.flavor` re-flavors the output to WOFF/WOFF2 in the same pass. `feaLib.addOpenTypeFeatures` compiles a `.fea` source into the font's GSUB/GPOS/GDEF tables.

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]    | [CAPABILITY]                                              |
| :-----: | :--------------------------------------------------------------------- | :---------------- | :-------------------------------------------------------- |
|  [01]   | `Options(**kwargs)`                                                    | subset policy     | configure feature/hint/name/table retention and `flavor`  |
|  [02]   | `Subsetter(options).populate(glyphs=[], gids=[], unicodes=[], text='')` | glyph selection   | declare the retained set by name, GID, codepoint, or text |
|  [03]   | `Subsetter.subset(font)`                                              | subset execution  | prune `font` in place; must precede `TTFont.save`         |
|  [04]   | `feaLib.builder.addOpenTypeFeatures(font, featurefile, tables=None, debug=False)` | feature compile   | compile `.fea` Layout features into GSUB/GPOS/GDEF        |

[ENTRYPOINT_SCOPE]: synthesis, merge, outline conversion, and Unicode resolution
- rail: fonts

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY]    | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------------------------- | :---------------- | :-------------------------------------------------------- |
|  [01]   | `FontBuilder(unitsPerEm, isTTF=True).setupGlyphOrder/setupCharacterMap/setupGlyf/setupCFF/setupHorizontalMetrics/setupNameTable/setupOS2/setupPost/setupFvar/setupGvar/setupSTAT/setupCOLR/setupCPAL(...)` then `.font` | font synthesis    | build a complete font from scratch through the `setup*` family |
|  [02]   | `Merger(options).merge(fontfiles)`                                         | font merge        | combine multiple fonts' tables into one `TTFont`          |
|  [03]   | `cu2qu.curve_to_quadratic(curve, max_err, all_quadratic=True)` / `qu2cu.quadratic_to_curves(...)` | outline convert | convert cubic⇄quadratic outlines for CFF↔glyf moves       |
|  [04]   | `unicodedata.script(char)` / `script_extension` / `block` / `ot_tags_from_script(script)` / `script_horizontal_direction(script)` | unicode resolve   | resolve script/block and the OpenType script tags shaping needs |

## [04]-[IMPLEMENTATION_LAW]

[FONT_TOPOLOGY]:
- namespace: `fontTools`; 336 submodules covering table IO, pens, subsetting, variation, feature authoring, synthesis, merge, outline conversion, and Unicode data
- core owners: `ttLib` (font container + `newTable` factory + `TTCollection`), `pens` (outline read/write algebra), `varLib` + `varLib.instancer` (variable-font compile/instance), `subset` (pruning), `feaLib` (Layout compile), `fontBuilder` (synthesis), `merge` (combine), `cu2qu`/`qu2cu` (outline conversion), `designspaceLib` (varfont project), `unicodedata` (script/OT-tag map)
- table access: `font[tag]` returns a parsed table object; `lazy=True` defers decompilation to first access; `ttLib.newTable(tag)` allocates an empty one — `TTFont` has no `newTable` method
- pen protocol: `getGlyphSet()[name].draw(pen)`/`.drawPoints(pen)` is the single read path; concrete pens (`RecordingPen`, `SVGPathPen`, `TransformPen`, `BoundsPen`, `StatisticsPen`, `TTGlyphPen`, `T2CharStringPen`) compose by forwarding; `PointToSegmentPen`/`SegmentToPointPen` bridge the two protocols
- instancing vs subsetting: `instantiateVariableFont(font, {axis: value})` pins axes (output is a static or reduced-axis font); `Subsetter` prunes glyphs/features/tables — both mutate toward a smaller deliverable, instancing first, subset second, `save` last

[STACKING]:
- text-shaping seam: `unicodedata.script`/`ot_tags_from_script` resolve the script and OpenType script tags; `getBestCmap()` resolves codepoint→glyph; `uharfbuzz` (`.api/uharfbuzz.md`) consumes the same `TTFont` bytes for OpenType shaping — fontTools owns the binary model and Unicode metadata, HarfBuzz owns itemisation/layout. The HarfBuzz `subset` and the fontTools `Subsetter` are two subsetters for one concern: prefer `Subsetter` for the Python-native pruning policy (`Options`), HarfBuzz subset only when its repacker is wired via `Options(harfbuzz_repacker=True)`.
- colour-glyph seam: `blackrenderer` (`.api/blackrenderer.md`) rasterizes COLRv1 over the fontTools `TTFont` COLR/CPAL tables and HarfBuzz paint callbacks — the font owner provides the loaded `TTFont`, never re-decodes COLR.
- outline-to-SVG seam: `getGlyphSet()[name].draw(SVGPathPen(glyphSet))` then `.getCommands()` emits the glyph `d` path that feeds the SVG/figure rail (`resvg-py`/`svgelements`); a `TransformPen` wrapper applies the units-per-em-to-target affine inline.
- deliverable seam: an instanced+subset+WOFF2-flavored `TTFont.save(buf)` produces the embeddable web-font bytes the document/PDF/HTML owners reference; fontTools never writes a layout, only the font binary.

[LOCAL_ADMISSION]:
- read uses `TTFont(path, lazy=True)`; tables are accessed by tag key, never hand-parsed
- glyph outlines are read through `getGlyphSet()[name].draw(pen)` and written through `TTGlyphPen`/`T2CharStringPen`; raw `glyf`/`CFF` coordinate mutation is boundary-only when no pen adapter fits
- variable fonts compile from `DesignSpaceDocument` via `varLib.build`; static cuts use `instantiateVariableFont`, never per-instance hand assembly
- subsetting is `Subsetter(Options(...))` → `populate(unicodes=…|text=…)` → `subset(font)` → `save`; never hand-prune table entries
- from-scratch synthesis uses `FontBuilder.setup*`; feature work uses `feaLib.addOpenTypeFeatures`, never hand-built GSUB/GPOS

[RAIL_LAW]:
- Package: `fonttools`
- Owns: OpenType/TrueType binary font IO, the glyph-outline pen algebra, variable-font compilation and instancing, glyph/feature/table subsetting with WOFF re-flavoring, OpenType Layout feature compilation, from-scratch font synthesis, multi-font merge, cubic-quadratic conversion, and Unicode script/OT-tag resolution
- Accept: `TTFont` for binary font work; pen subclasses for outline read/write; `instantiateVariableFont`/`varLib.build` for variable fonts; `Subsetter`+`Options` for pruning; `FontBuilder` for synthesis; `feaLib` for features
- Reject: hand-parsed sfnt tables; raw outline coordinates without a pen adapter; a `TTFont.newTable` call (use the `ttLib.newTable` module function); duplicate subset/instance logic where `Subsetter`/`instancer` operates; a hand-coded script-to-OT-tag map where `unicodedata` resolves it
