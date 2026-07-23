# [PY_ARTIFACTS_API_FONTTOOLS]

`fontTools` owns the OpenType/TrueType binary-font model for the artifacts font rail: font IO, the pen outline algebra, variable-font compile and instancing, glyph/feature/table subsetting with WOFF re-flavoring, OpenType Layout feature compilation, from-scratch synthesis, multi-font merge, and cubic-quadratic outline conversion, all through one `TTFont` container. HarfBuzz owns itemisation and layout and `blackrenderer` owns COLRv1 rasterization; this surface never re-parses sfnt tables, hand-prunes glyph entries, or hand-codes the script-to-OT-tag map.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fontTools`
- package: `fonttools` (MIT)
- module: `fontTools`
- owner: `artifacts`
- rail: fonts
- entry points: `fonttools`/`ttx`/`pyftsubset`/`pyftmerge` console scripts; import-only as a library

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: binary font container

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]   | [CAPABILITY]                                                                      |
| :-----: | :----------------------------- | :-------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `fontTools.ttLib.TTFont`       | font container  | sfnt/WOFF/WOFF2 container; `font[tag]` table access, `lazy`/`cfg`/`flavor` policy |
|  [02]   | `fontTools.ttLib.TTCollection` | font collection | `(file, shareTables=False)` — TTC/OTC collection of `TTFont` members              |
|  [03]   | `fontTools.ttLib.TTLibError`   | font fault      | binary-font IO/parse error rail                                                   |

[PUBLIC_TYPE_SCOPE]: glyph pen protocol (`fontTools.pens`)

Pens are the outline read/write algebra: `getGlyphSet()[name].draw(pen)` replays a glyph through the segment protocol `moveTo`/`lineTo`/`qCurveTo`/`curveTo`/`closePath`/`endPath`/`addComponent`, `.drawPoints(pen)` through the point protocol, and every pen composes by forwarding. Cells drop the `fontTools.pens.` prefix each symbol carries.

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY]       | [CAPABILITY]                                                  |
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

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]    | [CAPABILITY]                                                         |
| :-----: | :--------------------------------------------- | :--------------- | :------------------------------------------------------------------- |
|  [01]   | `fontTools.designspaceLib.DesignSpaceDocument` | varfont document | axes/sources/instances container; IO + `add*` builders               |
|  [02]   | `fontTools.designspaceLib.AxisDescriptor`      | axis record      | variation axis: `tag`/`name`/`minimum`/`default`/`maximum`/`map`     |
|  [03]   | `fontTools.designspaceLib.SourceDescriptor`    | source record    | `filename`/`path`/`font`/`name`/`location`/`designLocation`          |
|  [04]   | `fontTools.designspaceLib.InstanceDescriptor`  | instance record  | `filename`/`path`/`font`/`name`/`location`/`locationLabel`           |
|  [05]   | `fontTools.subset.Subsetter`                   | subsetter engine | `(options)` — `populate(...)` then `subset(font)`; in-place pruning  |
|  [06]   | `fontTools.subset.Options`                     | subset policy    | `(**kwargs)` — feature/hint/name/table retention (keys below)        |
|  [07]   | `fontTools.fontBuilder.FontBuilder`            | synthesis owner  | `(unitsPerEm, isTTF, glyphDataFormat)`; `setup*` family then `.font` |
|  [08]   | `fontTools.merge.Merger`                       | font combiner    | `(options)` — `merge(fontfiles)` combining tables across fonts       |

- `Options` keys: `layout_features` `name_IDs` `hinting` `flavor` `retain_gids` `glyph_names` `desubroutinize` `drop_tables` `harfbuzz_repacker` `with_zopfli`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: font container IO and glyph access

| [INDEX] | [SURFACE]                                                  | [CAPABILITY]                                           |
| :-----: | :--------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `TTFont(...)`                                              | open/create a font; `flavor`/`lazy`/`cfg` policy       |
|  [02]   | `save(file, reorderTables=True)`                           | write OTF/TTF/WOFF/WOFF2 to path or buffer             |
|  [03]   | `saveXML(...)` / `importXML(...)`                          | write/read the TTX XML representation                  |
|  [04]   | `getGlyphSet(...)`                                         | glyph set; `location` cuts a variable instance         |
|  [05]   | `getBestCmap(cmapPreferences=…)`                           | `{codepoint: glyphName}` from the best `cmap` subtable |
|  [06]   | `getGlyphOrder()` / `setGlyphOrder(order)`                 | ordered glyph names                                    |
|  [07]   | `getReverseGlyphMap(rebuild=False)`                        | `{name: GID}` reverse map                              |
|  [08]   | `getGlyphName(glyphID)` / `getGlyphID(glyphName)`          | bidirectional glyph-name/GID lookup                    |
|  [09]   | `keys()` / `TTFont[tag]` / `fontTools.ttLib.newTable(tag)` | enumerate tags, fetch a table, allocate a new one      |

[ENTRYPOINT_SCOPE]: variable-font compile and instance

| [INDEX] | [SURFACE]                                   | [CAPABILITY]                                           |
| :-----: | :------------------------------------------ | :----------------------------------------------------- |
|  [01]   | `varLib.build(...)`                         | build a variable `TTFont` (+ model) from a designspace |
|  [02]   | `instantiateVariableFont(...)`              | pin/limit axes to a static or reduced-axis instance    |
|  [03]   | `DesignSpaceDocument` read/build IO         | read/build the axes-sources-instances project          |
|  [04]   | `AxisLimits({tag: float\|(min,max)\|None})` | typed `axisLimits`: per-axis pin/range/drop            |

[ENTRYPOINT_SCOPE]: subsetting and feature compilation

| [INDEX] | [SURFACE]                    | [CAPABILITY]                                              |
| :-----: | :--------------------------- | :-------------------------------------------------------- |
|  [01]   | `Options(**kwargs)`          | configure feature/hint/name/table retention and `flavor`  |
|  [02]   | `Subsetter.populate(...)`    | declare the retained set by name, GID, codepoint, or text |
|  [03]   | `Subsetter.subset(font)`     | prune `font` in place; must precede `TTFont.save`         |
|  [04]   | `feaLib.addOpenTypeFeatures` | compile `.fea` Layout features into GSUB/GPOS/GDEF        |

[ENTRYPOINT_SCOPE]: synthesis, merge, outline conversion, and Unicode resolution

| [INDEX] | [SURFACE]                              | [CAPABILITY]                                                              |
| :-----: | :------------------------------------- | :------------------------------------------------------------------------ |
|  [01]   | `FontBuilder.setup*(...)` → `.font`    | build a complete font from scratch through the `setup*` family            |
|  [02]   | `addOpenTypeFeaturesFromString(...)`   | compile `.fea` features supplied as a string (no temp file)               |
|  [03]   | `addFeatureVariations(...)`            | add rvrn-style conditional GSUB substitutions to a variable font          |
|  [04]   | `colorLib.builder.buildCOLR/buildCPAL` | build COLRv0/COLRv1 + CPAL color-glyph tables for `setupCOLR`/`setupCPAL` |
|  [05]   | `Merger(options).merge(fontfiles)`     | combine multiple fonts' tables into one `TTFont`                          |
|  [06]   | `cu2qu`/`qu2cu` convert                | convert cubic⇄quadratic per-curve or whole-font for CFF↔glyf moves        |
|  [07]   | `unicodedata` script/OT-tag            | resolve script/block and map OpenType script tags bidirectionally         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every op folds through one `TTFont` container; `font[tag]` returns a parsed table object and `lazy=True` defers decompilation to first access.
- Pen protocol is the single glyph read path — `getGlyphSet()[name].draw(pen)`/`.drawPoints(pen)` replays a glyph into any pen, concrete pens composing by forwarding through `FilterPen`/`FilterPointPen`, and `PointToSegmentPen`/`SegmentToPointPen` bridging the segment and point protocols.
- Instancing precedes subsetting precedes `save`: `instantiateVariableFont` pins or limits axes (`OverlapMode` ∈ `KEEP_AND_SET_FLAGS`/`KEEP_AND_DONT_SET_FLAGS`/`REMOVE`/`REMOVE_AND_IGNORE_ERRORS`), `Subsetter` prunes glyphs/features/tables, `save` re-flavors to WOFF/WOFF2, and `varLib.featureVars.addFeatureVariations` adds conditional GSUB before instancing pins it.

[STACKING]:
- `uharfbuzz`(`.api/uharfbuzz.md`): fontTools owns the binary `TTFont` model and Unicode metadata (`unicodedata.script`/`ot_tags_from_script`, `getBestCmap` codepoint→glyph) HarfBuzz consumes for itemisation and layout; the two subsetters split — `Subsetter`+`Options` for Python-native pruning policy, HarfBuzz subset only when its repacker wires in via `Options(harfbuzz_repacker=True)`.
- `blackrenderer`(`.api/blackrenderer.md`): `colorLib.builder.buildCOLR`/`buildCPAL` (via `FontBuilder.setupCOLR`/`setupCPAL`) author the COLR/CPAL tables `blackrenderer` rasterizes over the loaded `TTFont`, and the renderer never re-decodes COLR.
- `resvg-py`(`.api/resvg-py.md`)/`svgelements`(`.api/svgelements.md`): `getGlyphSet()[name].draw(SVGPathPen(glyphSet))` then `.getCommands()` emits the glyph `d` path the SVG/figure rail consumes, a `TransformPen` applying the units-per-em-to-target affine inline.
- within-lib: an instanced + subset + WOFF2-flavored `TTFont.save(buf)` produces the embeddable web-font bytes the document/PDF/HTML owners reference; fontTools writes only the font binary, never a layout.

[LOCAL_ADMISSION]:
- Outlines read through `getGlyphSet()[name].draw(pen)` and write through `TTGlyphPen`/`T2CharStringPen`; raw `glyf`/`CFF` coordinate mutation is boundary-only where no pen adapter fits.
- Subsetting runs `Subsetter(Options(...))` → `populate(unicodes=…|text=…)` → `subset(font)` → `save`, instancing first when the font is also pinned.

[RAIL_LAW]:
- Package: `fonttools`
- Owns: OpenType/TrueType binary font IO, the glyph-outline pen algebra, variable-font compilation and instancing, glyph/feature/table subsetting with WOFF re-flavoring, OpenType Layout feature compilation, from-scratch synthesis, multi-font merge, cubic-quadratic conversion, and Unicode script/OT-tag resolution
- Accept: `TTFont` for binary work; pen subclasses for outline read/write; `varLib.build`/`instantiateVariableFont` for variable fonts; `Subsetter`+`Options` for pruning; `FontBuilder` for synthesis; `feaLib` for features
- Reject: hand-parsed sfnt tables; raw outline coordinates where a `FilterPen` subclass fits; a `TTFont.newTable` method call where the `ttLib.newTable` module function allocates; a `setupSTAT` call where the method is `setupStat`; hand-built COLR/CPAL where `colorLib.builder` operates; a hand-coded script-to-OT-tag map where `unicodedata` resolves it
