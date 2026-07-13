# [PY_ARTIFACTS_API_FONTTOOLS]

`fontTools` supplies the OpenType/TrueType font model for the artifacts font rail: `TTFont` as the binary-font owner, the `pens` protocol family as the outline read/write algebra, `varLib`/`varLib.instancer` as the variable-font compile-and-instance engine, `subset` as the glyph-set/feature/table pruning engine, `feaLib` as the OpenType Layout feature compiler, `fontBuilder.FontBuilder` as the high-level synthesis owner, `merge.Merger` as the multi-font combiner, `cu2qu`/`qu2cu` as the cubic-quadratic outline converter, and `unicodedata` as the script/OT-tag resolver. The package owner drives all font IO, outline construction, instancing, subsetting, and feature compilation through these surfaces and never re-parses sfnt tables, hand-prunes glyph entries, or hand-codes the script-to-OT-tag map.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fontTools`
- package: `fonttools`
- import: `fontTools`
- owner: `artifacts`
- rail: fonts
- license: MIT
- installed: `4.63.0`
- entry points: `fonttools`, `ttx`, `pyftsubset`, `pyftmerge` console scripts; library use is import-only
- capability: read/write OTF/TTF/WOFF/WOFF2/TTC binary fonts; lazy table decompilation by tag; outline read/write through a pen protocol; variable-font compilation from a designspace and partial/full instancing; glyph-set/feature/name/table subsetting with WOFF re-flavoring; OpenType Layout feature compilation from `.fea`; from-scratch font synthesis; multi-font merge; cubic-quadratic outline conversion; Unicode script/block/OT-tag resolution

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: binary font container
- rail: fonts

`TTFont` is the single binary-font owner: `font[tag]` returns the parsed table object (lazy-decompiled on first access when `lazy=True`), `font.keys()` enumerates present tags, `font.flavor` is `None`/`"woff"`/`"woff2"`. `ttLib.newTable(tag)` is a module function (not a `TTFont` method) that allocates an empty table; `TTCollection` owns `.ttc`/`.otc` font collections.

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]  | [CAPABILITY]                                                                      |
| :-----: | :----------------------------- | :-------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `fontTools.ttLib.TTFont`       | font container  | sfnt/WOFF/WOFF2 container; `font[tag]` table access, `lazy`/`cfg`/`flavor` policy |
|  [02]   | `fontTools.ttLib.TTCollection` | font collection | `(file, shareTables=False)` — TTC/OTC collection of `TTFont` members              |
|  [03]   | `fontTools.ttLib.TTLibError`   | font fault      | binary-font IO/parse error rail                                                   |

[PUBLIC_TYPE_SCOPE]: glyph pen protocol (`fontTools.pens`)
- rail: fonts

Pens are the outline read/write algebra: `TTFont.getGlyphSet()[name].draw(pen)` (segment protocol) or `.drawPoints(pen)` (point protocol) replays a glyph into any pen; pens compose by forwarding. The segment protocol is `moveTo`/`lineTo`/`qCurveTo`/`curveTo`/`closePath`/`endPath`/`addComponent`. Every pen is `fontTools.pens.<module>.<Name>`; the cell drops the `fontTools.pens.` prefix.
- call: `svgPathPen.SVGPathPen(glyphSet, ntos=str)` — emit SVG `d` path via `.getCommands()`
- call: `transformPen.TransformPen(outPen, transformation)` — apply affine while forwarding
- call: `ttGlyphPen.TTGlyphPen(glyphSet, handleOverflowingTransforms, outputImpliedClosingLine)` — build a `glyf` `Glyph` via `.glyph(...)`
- call: `t2CharStringPen.T2CharStringPen(width, glyphSet, roundTolerance=0.5, CFF2=False)` — emit Type 2 charstring
- call: `recordingPen.lerpRecordings(rec1, rec2, factor)` — interpolate two `RecordingPen` captures

| [INDEX] | [SYMBOL]                                           | [PACKAGE_ROLE]      | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------- | :------------------ | :------------------------------------------------------------ |
|  [01]   | `basePen.BasePen`                                  | outline pen base    | segment receiver base; `qCurveTo` decomposes to `_curveToOne` |
|  [02]   | `pointPen.AbstractPointPen`                        | point pen base      | `beginPath`/`addPoint`/`endPath`/`addComponent`               |
|  [03]   | `recordingPen.RecordingPen` / `RecordingPointPen`  | recording pen       | capture segment/point calls into `.value`; `.replay(pen)`     |
|  [04]   | `recordingPen.DecomposingRecordingPen`             | flattening recorder | record with component references decomposed to outlines       |
|  [05]   | `recordingPen.lerpRecordings`                      | interpolation       | interpolate two `RecordingPen` captures by `factor`           |
|  [06]   | `pointPen.PointToSegmentPen` / `SegmentToPointPen` | protocol adapter    | convert between point and segment pen protocols               |
|  [07]   | `svgPathPen.SVGPathPen`                            | SVG path pen        | emit SVG `d` path via `.getCommands()`                        |
|  [08]   | `transformPen.TransformPen` / `TransformPointPen`  | transform pen       | apply affine while forwarding                                 |
|  [09]   | `boundsPen.BoundsPen` / `ControlBoundsPen`         | bounds pen          | exact / control-point glyph bounding box                      |
|  [10]   | `statisticsPen.StatisticsPen`                      | moments pen         | glyph area/moments/mean (exact) for outline metrics           |
|  [11]   | `statisticsPen.StatisticsControlPen`               | moments pen         | glyph area/moments/mean (control bbox) for outline metrics    |
|  [12]   | `statisticsPen.MomentsPen`                         | moments pen         | raw glyph moments base for the statistics pens                |
|  [13]   | `areaPen.AreaPen`                                  | area pen            | signed contour area                                           |
|  [14]   | `ttGlyphPen.TTGlyphPen` / `TTGlyphPointPen`        | TrueType pen        | build a `glyf` `Glyph` via `.glyph(...)`                      |
|  [15]   | `t2CharStringPen.T2CharStringPen`                  | CFF pen             | emit Type 2 charstring                                        |
|  [16]   | `reverseContourPen.ReverseContourPen`              | winding flip        | reverse contour direction (CFF↔glyf winding)                  |
|  [17]   | `filterPen.FilterPen` / `FilterPointPen`           | filter base         | pass-through base for outline transforms by subclassing       |
|  [18]   | `roundingPen.RoundingPen` / `RoundingPointPen`     | rounding pen        | round coordinates to integers while forwarding                |
|  [19]   | `hashPointPen.HashPointPen`                        | hash pen            | content hash of a glyph outline (compat checks)               |
|  [20]   | `cairoPen.CairoPen`                                | render pen          | draw a glyph into a cairo surface (optional backend)          |
|  [21]   | `qtPen.QtPen`                                      | render pen          | draw a glyph into a Qt surface (optional backend)             |
|  [22]   | `freetypePen.FreeTypePen`                          | render pen          | draw a glyph into a FreeType surface (optional backend)       |

[PUBLIC_TYPE_SCOPE]: variable-font, subset, synthesis, and merge engines
- rail: fonts

| [INDEX] | [SYMBOL]                                       | [PACKAGE_ROLE]   | [CAPABILITY]                                                         |
| :-----: | :--------------------------------------------- | :--------------- | :------------------------------------------------------------------- |
|  [01]   | `fontTools.designspaceLib.DesignSpaceDocument` | varfont document | axes/sources/instances container; IO + `add*` builders               |
|  [02]   | `fontTools.designspaceLib.AxisDescriptor`      | axis record      | `(tag, name, minimum, default, maximum, map, …)` — a variation axis  |
|  [03]   | `fontTools.designspaceLib.SourceDescriptor`    | source record    | `(filename, path, font, name, location, designLocation, …)`          |
|  [04]   | `fontTools.designspaceLib.InstanceDescriptor`  | instance record  | `(filename, path, font, name, location, locationLabel, …)`           |
|  [05]   | `fontTools.subset.Subsetter`                   | subsetter engine | `(options)` — `populate(...)` then `subset(font)`; in-place pruning  |
|  [06]   | `fontTools.subset.Options`                     | subset policy    | `(**kwargs)` — feature/hint/name/table retention (keys below)        |
|  [07]   | `fontTools.fontBuilder.FontBuilder`            | synthesis owner  | `(unitsPerEm, isTTF, glyphDataFormat)`; `setup*` family then `.font` |
|  [08]   | `fontTools.merge.Merger`                       | font combiner    | `(options)` — `merge(fontfiles)` combining tables across fonts       |

- [06]-[OPTIONS]: `layout_features`/`name_IDs`/`hinting`/`flavor`/`retain_gids`/`glyph_names`/`desubroutinize`/`drop_tables`/`harfbuzz_repacker`/`with_zopfli`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: font container IO and glyph access
- rail: fonts

`TTFont(path, lazy=True)` opens read-mostly; `flavor="woff2"` selects WOFF2 output before `save`. `getGlyphSet()` is the read-path glyph factory whose members `.draw(pen)`/`.drawPoints(pen)` feed the pen family; `getBestCmap()` resolves the Unicode-to-glyph map for shaping/subset input. Cells drop the `TTFont.` method prefix.
- call: `TTFont(file=None, sfntVersion='\x00\x01\x00\x00', flavor=None, recalcBBoxes=True, recalcTimestamp=True, lazy=None, fontNumber=-1, cfg={})`
- call: `TTFont.saveXML(fileOrPath, newlinestr='\n', **XMLSavingOptions)` / `TTFont.importXML(fileOrPath)`
- call: `TTFont.getGlyphSet(preferCFF=True, location=None, normalized=False, recalcBounds=True)`

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY]   | [CAPABILITY]                                           |
| :-----: | :--------------------------------------------------------- | :--------------- | :----------------------------------------------------- |
|  [01]   | `TTFont(...)`                                              | construction     | open/create a font; `flavor`/`lazy`/`cfg` policy       |
|  [02]   | `save(file, reorderTables=True)`                           | binary export    | write OTF/TTF/WOFF/WOFF2 to path or buffer             |
|  [03]   | `saveXML(...)` / `importXML(...)`                          | TTX round-trip   | write/read the TTX XML representation                  |
|  [04]   | `getGlyphSet(...)`                                         | glyph factory    | glyph set; `location` cuts a variable instance         |
|  [05]   | `getBestCmap(cmapPreferences=…)`                           | unicode map      | `{codepoint: glyphName}` from the best `cmap` subtable |
|  [06]   | `getGlyphOrder()` / `setGlyphOrder(order)`                 | glyph order      | ordered glyph names                                    |
|  [07]   | `getReverseGlyphMap(rebuild=False)`                        | glyph order      | `{name: GID}` reverse map                              |
|  [08]   | `getGlyphName(glyphID)` / `getGlyphID(glyphName)`          | name-gid resolve | bidirectional glyph-name/GID lookup                    |
|  [09]   | `keys()` / `TTFont[tag]` / `fontTools.ttLib.newTable(tag)` | table access     | enumerate tags, fetch a table, allocate a new one      |

[ENTRYPOINT_SCOPE]: variable-font compile and instance
- rail: fonts

`varLib.build` compiles a variable font from a designspace; `varLib.instancer.instantiateVariableFont` is the partial/full instancing engine — limit a subset of axes to pin a static or reduced-axis cut that downstream shaping/subsetting consumes.
- call: `varLib.build(designspace, master_finder=…, exclude=[], optimize=True, colr_layer_reuse=True, drop_implied_oncurves=False)`
- call: `varLib.instancer.instantiateVariableFont(varfont, axisLimits, inplace=False, optimize=True, overlap=OverlapMode.KEEP_AND_SET_FLAGS, updateFontNames=False, *, downgradeCFF2=False, static=False)`
- call: `DesignSpaceDocument.read(path)` / `.write(path)` / `.addAxis(a)` / `.addSource(s)` / `.addInstance(i)`

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]  | [CAPABILITY]                                           |
| :-----: | :---------------------------------- | :-------------- | :----------------------------------------------------- |
|  [01]   | `varLib.build(...)`                 | varfont compile | build a variable `TTFont` (+ model) from a designspace |
|  [02]   | `instantiateVariableFont(...)`      | instancing      | pin/limit axes to a static or reduced-axis instance    |
|  [03]   | `DesignSpaceDocument` read/build IO | designspace IO  | read/build the axes-sources-instances project          |

[ENTRYPOINT_SCOPE]: subsetting and feature compilation
- rail: fonts

`Subsetter` prunes a `TTFont` in place to the populated glyph/Unicode set before `save`; `Options.flavor` re-flavors the output to WOFF/WOFF2 in the same pass. `feaLib.addOpenTypeFeatures` compiles a `.fea` source into the font's GSUB/GPOS/GDEF tables.
- call: `Subsetter(options).populate(glyphs=[], gids=[], unicodes=[], text='')`
- call: `feaLib.builder.addOpenTypeFeatures(font, featurefile, tables=None, debug=False)`

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY]   | [CAPABILITY]                                              |
| :-----: | :--------------------------- | :--------------- | :-------------------------------------------------------- |
|  [01]   | `Options(**kwargs)`          | subset policy    | configure feature/hint/name/table retention and `flavor`  |
|  [02]   | `Subsetter.populate(...)`    | glyph selection  | declare the retained set by name, GID, codepoint, or text |
|  [03]   | `Subsetter.subset(font)`     | subset execution | prune `font` in place; must precede `TTFont.save`         |
|  [04]   | `feaLib.addOpenTypeFeatures` | feature compile  | compile `.fea` Layout features into GSUB/GPOS/GDEF        |

[ENTRYPOINT_SCOPE]: synthesis, merge, outline conversion, and Unicode resolution
- rail: fonts

`FontBuilder` synthesizes a whole font through its `setup*` family (table casing follows the method — `setupStat`, not `setupSTAT`); `Merger` combines fonts, `cu2qu`/`qu2cu` convert outlines per-curve or whole-font, and `unicodedata` resolves script/block and the OpenType script tags.
- call: `FontBuilder(unitsPerEm, isTTF=True).setupGlyphOrder/setupCharacterMap/setupHead/setupMaxp/setupGlyf/setupCFF/setupCFF2/setupHorizontalHeader/setupHorizontalMetrics/setupVerticalHeader/setupVerticalMetrics/setupNameTable/setupOS2/setupPost/setupFvar/setupGvar/setupAvar/setupStat/setupCOLR/setupCPAL/setupDummyDSIG(...)` then `.font`
- call: `feaLib.builder.addOpenTypeFeaturesFromString(font, features, …)`
- call: `varLib.featureVars.addFeatureVariations(font, conditionalSubstitutions, …)`
- call: `colorLib.builder.buildCOLR(colorGlyphs, …)` / `buildCPAL(palettes, …)`
- call: `cu2qu.curve_to_quadratic(curve, max_err, all_quadratic=True)` / `cu2qu.ufo.fonts_to_quadratic(fonts, …)` / `qu2cu.quadratic_to_curves(...)`
- call: `unicodedata.script(char)` / `script_extension` / `block` / `ot_tags_from_script(script)` / `ot_tag_to_script(tag)` / `script_horizontal_direction(script)` / `script_name`/`script_code`

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY]     | [CAPABILITY]                                                              |
| :-----: | :------------------------------------- | :----------------- | :------------------------------------------------------------------------ |
|  [01]   | `FontBuilder.setup*(...)` → `.font`    | font synthesis     | build a complete font from scratch through the `setup*` family            |
|  [02]   | `addOpenTypeFeaturesFromString(...)`   | inline feature     | compile `.fea` features supplied as a string (no temp file)               |
|  [03]   | `addFeatureVariations(...)`            | feature variations | add rvrn-style conditional GSUB substitutions to a variable font          |
|  [04]   | `colorLib.builder.buildCOLR/buildCPAL` | color tables       | build COLRv0/COLRv1 + CPAL color-glyph tables for `setupCOLR`/`setupCPAL` |
|  [05]   | `Merger(options).merge(fontfiles)`     | font merge         | combine multiple fonts' tables into one `TTFont`                          |
|  [06]   | `cu2qu`/`qu2cu` convert                | outline convert    | convert cubic⇄quadratic per-curve or whole-font for CFF↔glyf moves        |
|  [07]   | `unicodedata` script/OT-tag            | unicode resolve    | resolve script/block and map OpenType script tags bidirectionally         |

## [04]-[IMPLEMENTATION_LAW]

[FONT_TOPOLOGY]:
- namespace: `fontTools`; 336 submodules covering table IO, pens, subsetting, variation, feature authoring, synthesis, merge, outline conversion, and Unicode data
- core owners: `ttLib` (font container + `newTable` factory + `TTCollection`), `pens` (outline read/write algebra incl. `lerpRecordings`/`HashPointPen`/filter pens), `varLib` + `varLib.instancer` (variable-font compile/instance) + `varLib.featureVars` (conditional substitutions), `subset` (pruning), `feaLib` (Layout compile, file or string), `fontBuilder` (synthesis), `colorLib.builder` (COLR/CPAL color-glyph tables), `merge` (combine), `cu2qu`/`qu2cu` (outline conversion, per-curve or whole-font via `cu2qu.ufo`), `designspaceLib` (varfont project), `unicodedata` (script/OT-tag map)
- table access: `font[tag]` returns a parsed table object; `lazy=True` defers decompilation to first access; `ttLib.newTable(tag)` allocates an empty one — `TTFont` has no `newTable` method
- pen protocol: `getGlyphSet()[name].draw(pen)`/`.drawPoints(pen)` is the single read path; concrete pens (`RecordingPen`/`RecordingPointPen`, `SVGPathPen`, `TransformPen`, `BoundsPen`, `StatisticsPen`, `TTGlyphPen`, `T2CharStringPen`, `ReverseContourPen`, `RoundingPen`) compose by forwarding through `FilterPen`/`FilterPointPen`; `PointToSegmentPen`/`SegmentToPointPen` bridge the two protocols; `lerpRecordings` interpolates two `RecordingPen` captures and `HashPointPen` hashes an outline for interpolation-compatibility checks
- instancing vs subsetting: `instantiateVariableFont(font, {axis: value}, overlap=OverlapMode.KEEP_AND_SET_FLAGS)` pins axes (output is a static or reduced-axis font; `OverlapMode` ∈ `KEEP_AND_SET_FLAGS`/`KEEP_AND_DONT_SET_FLAGS`/`REMOVE`/`REMOVE_AND_IGNORE_ERRORS`); `Subsetter` prunes glyphs/features/tables — both mutate toward a smaller deliverable, instancing first, subset second, `save` last; `varLib.featureVars.addFeatureVariations` adds conditional GSUB before instancing pins them

[STACKING]:
- text-shaping seam: `unicodedata.script`/`ot_tags_from_script` resolve the script and OpenType script tags; `getBestCmap()` resolves codepoint→glyph; `uharfbuzz` (`.api/uharfbuzz.md`) consumes the same `TTFont` bytes for OpenType shaping — fontTools owns the binary model and Unicode metadata, HarfBuzz owns itemisation/layout. The HarfBuzz `subset` and the fontTools `Subsetter` are two subsetters for one concern: prefer `Subsetter` for the Python-native pruning policy (`Options`), HarfBuzz subset only when its repacker is wired via `Options(harfbuzz_repacker=True)`.
- colour-glyph seam: fontTools both builds and provides the COLR/CPAL tables — `colorLib.builder.buildCOLR`/`buildCPAL` (via `FontBuilder.setupCOLR`/`setupCPAL`) author the color-glyph tables, and `blackrenderer` (`.api/blackrenderer.md`) rasterizes COLRv1 over the loaded fontTools `TTFont` COLR/CPAL tables and HarfBuzz paint callbacks — the font owner provides/authors the `TTFont`, the renderer never re-decodes COLR.
- outline-to-SVG seam: `getGlyphSet()[name].draw(SVGPathPen(glyphSet))` then `.getCommands()` emits the glyph `d` path that feeds the SVG/figure rail (`resvg-py`/`svgelements`); a `TransformPen` wrapper applies the units-per-em-to-target affine inline.
- deliverable seam: an instanced+subset+WOFF2-flavored `TTFont.save(buf)` produces the embeddable web-font bytes the document/PDF/HTML owners reference; fontTools never writes a layout, only the font binary.

[LOCAL_ADMISSION]:
- read uses `TTFont(path, lazy=True)`; tables are accessed by tag key, never hand-parsed
- glyph outlines are read through `getGlyphSet()[name].draw(pen)` and written through `TTGlyphPen`/`T2CharStringPen`; raw `glyf`/`CFF` coordinate mutation is boundary-only when no pen adapter fits
- variable fonts compile from `DesignSpaceDocument` via `varLib.build`; static cuts use `instantiateVariableFont`, never per-instance hand assembly
- subsetting is `Subsetter(Options(...))` → `populate(unicodes=…|text=…)` → `subset(font)` → `save`; never hand-prune table entries
- from-scratch synthesis uses `FontBuilder.setup*` (the table-name casing follows the method, e.g. `setupStat`/`setupGvar`/`setupCOLR`, never `setupSTAT`); feature work uses `feaLib.addOpenTypeFeatures`/`addOpenTypeFeaturesFromString`, never hand-built GSUB/GPOS; color glyphs use `colorLib.builder.buildCOLR`/`buildCPAL`

[RAIL_LAW]:
- Package: `fonttools`
- Owns: OpenType/TrueType binary font IO, the glyph-outline pen algebra, variable-font compilation and instancing, glyph/feature/table subsetting with WOFF re-flavoring, OpenType Layout feature compilation, from-scratch font synthesis, multi-font merge, cubic-quadratic conversion, and Unicode script/OT-tag resolution
- Accept: `TTFont` for binary font work; pen subclasses for outline read/write; `instantiateVariableFont`/`varLib.build` for variable fonts; `Subsetter`+`Options` for pruning; `FontBuilder` for synthesis; `feaLib` for features
- Reject: hand-parsed sfnt tables; raw outline coordinates without a pen adapter where a `FilterPen` subclass fits; a `TTFont.newTable` call (use the `ttLib.newTable` module function); a `setupSTAT` call (the method is `setupStat`); duplicate subset/instance logic where `Subsetter`/`instancer` operates; hand-built COLR/CPAL where `colorLib.builder` operates; a hand-coded script-to-OT-tag map where `unicodedata` resolves it
