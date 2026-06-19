# [PY_ARTIFACTS_API_FONTTOOLS]

`fontTools` supplies the OpenType/TrueType font manipulation surface for the artifacts font rail: `TTFont` as the primary binary-font owner, the `pens` protocol family for glyph outline authoring, `designspaceLib` descriptors for variable-font sources and instances, `feaLib` for OpenType Layout feature compilation, and `subset` for glyph-set and feature subsetting. The package owner drives all font IO, outline construction, and subsetting through these surfaces and never re-implements table parsing or glyph-pen primitives that `fontTools` owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fontTools`
- package: `fonttools`
- import: `fontTools`
- owner: `artifacts`
- rail: fonts
- asset: runtime library
- installed: `4.63.0` reflected via `/tmp/wfpy-artifacts315/bin/python`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core font container
- rail: fonts

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE] | [CAPABILITY]                                                   |
| :-----: | :----------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `fontTools.ttLib.TTFont` | font container | `(file, …, lazy, cfg)` — OTF/TTF/WOFF/TTC read-write container |

[PUBLIC_TYPE_SCOPE]: design space descriptors
- rail: fonts

| [INDEX] | [SYMBOL]                                       | [PACKAGE_ROLE]   | [CAPABILITY]                                                      |
| :-----: | :--------------------------------------------- | :--------------- | :---------------------------------------------------------------- |
|  [01]   | `fontTools.designspaceLib.DesignSpaceDocument` | varfont document | `(readerClass, writerClass)` — axes, sources, instances container |
|  [02]   | `fontTools.designspaceLib.SourceDescriptor`    | source record    | `(filename, path, font, name, location, designLocation, …)`       |
|  [03]   | `fontTools.designspaceLib.InstanceDescriptor`  | instance record  | `(filename, path, font, name, location, locationLabel, …)`        |
|  [04]   | `fontTools.designspaceLib.AxisDescriptor`      | axis record      | `(tag, name, minimum, default, maximum, map, …)`                  |

[PUBLIC_TYPE_SCOPE]: subsetting entry points
- rail: fonts

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]   | [CAPABILITY]                                               |
| :-----: | :--------------------------- | :--------------- | :--------------------------------------------------------- |
|  [01]   | `fontTools.subset.Subsetter` | subsetter engine | `(options)` — populate glyphs/unicodes then `subset(font)` |
|  [02]   | `fontTools.subset.Options`   | subset policy    | `(**kwargs)` — features, hints, name-IDs policy            |

[PUBLIC_TYPE_SCOPE]: glyph pen protocol
- rail: fonts

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]   | [CAPABILITY]                                                      |
| :-----: | :------------------------------- | :--------------- | :---------------------------------------------------------------- |
|  [01]   | `fontTools.pens.basePen.BasePen` | outline pen base | `moveTo`, `lineTo`, `qCurveTo`, `curveTo`, `closePath`, `endPath` |
|  [02]   | `fontTools.pens.recordingPen`    | recording pen    | capture and replay outline commands                               |
|  [03]   | `fontTools.pens.svgPathPen`      | SVG path pen     | emit SVG path data from outline commands                          |
|  [04]   | `fontTools.pens.transformPen`    | transform pen    | apply affine transform while forwarding outline commands          |
|  [05]   | `fontTools.pens.ttGlyphPen`      | TrueType pen     | emit TrueType quadratic glyph data to a `TTFont`                  |
|  [06]   | `fontTools.pens.t2CharStringPen` | CFF pen          | emit Type 2 charstring for CFF tables                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: font container operations
- rail: fonts

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]    | [CAPABILITY]                                |
| :-----: | :------------------------------------------------- | :---------------- | :------------------------------------------ |
|  [01]   | `TTFont(file, …, lazy, cfg)`                       | construction      | open or create a font container             |
|  [02]   | `TTFont.save(file, reorderTables)`                 | binary export     | write OTF/TTF/WOFF to file or buffer        |
|  [03]   | `TTFont.saveXML(fileOrPath, newlinestr, **kwargs)` | XML export        | write TTX XML representation                |
|  [04]   | `TTFont.keys()`                                    | table enumeration | list all table tags present in the font     |
|  [05]   | `TTFont.getGlyphOrder()`                           | glyph list        | ordered list of glyph names                 |
|  [06]   | `TTFont.getReverseGlyphMap(rebuild)`               | name-to-GID map   | `{name: GID}` mapping                       |
|  [07]   | `TTFont.newTable(tag)`                             | table creation    | allocate a new table object for a given tag |

[ENTRYPOINT_SCOPE]: subsetting
- rail: fonts

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]   | [CAPABILITY]                               |
| :-----: | :------------------------------------------------- | :--------------- | :----------------------------------------- |
|  [01]   | `Subsetter(options)`                               | subsetter init   | initialise with an `Options` policy        |
|  [02]   | `Subsetter.populate(glyphs, gids, unicodes, text)` | glyph selection  | declare the glyph set to retain            |
|  [03]   | `Subsetter.subset(font)`                           | subset execution | prune `font` in-place to the populated set |

[ENTRYPOINT_SCOPE]: design space and variable font
- rail: fonts

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY]  | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------ | :-------------- | :--------------------------------------------------- |
|  [01]   | `DesignSpaceDocument(readerClass, writerClass)`               | document owner  | axes, sources, instances for a variable font project |
|  [02]   | `SourceDescriptor(filename, path, font, name, location, …)`   | source record   | one master font plus its design-space location       |
|  [03]   | `InstanceDescriptor(filename, path, font, name, location, …)` | instance record | one static instance at a design-space position       |
|  [04]   | `AxisDescriptor(tag, name, minimum, default, maximum, …)`     | axis record     | one variation axis with range and optional mapping   |

[ENTRYPOINT_SCOPE]: glyph pen operations
- rail: fonts

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [CAPABILITY]                                      |
| :-----: | :-------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `BasePen.moveTo(pt)`        | contour start  | begin a new contour at point                      |
|  [02]   | `BasePen.lineTo(pt)`        | line segment   | straight segment to point                         |
|  [03]   | `BasePen.qCurveTo(*points)` | TrueType curve | quadratic spline through implicit on-curve points |
|  [04]   | `BasePen.curveTo(*points)`  | cubic curve    | cubic Bézier to final on-curve point              |
|  [05]   | `BasePen.closePath()`       | closed contour | close the current contour                         |
|  [06]   | `BasePen.endPath()`         | open contour   | end an open contour                               |

## [04]-[IMPLEMENTATION_LAW]

[FONT_TOPOLOGY]:
- namespace: `fontTools`; 336 submodules covering table IO, pens, subsetting, variation, feature authoring, and encoding
- core modules: `ttLib` (font container), `subset` (subsetting), `designspaceLib` (variable font sources), `feaLib` (OpenType Layout), `varLib` (variable font compilation), `pens` (27 pen modules)
- table access: `TTFont[tag]` returns a parsed table object; lazy-load defers decompilation to first access
- pen protocol: `BasePen` is the abstract outline receiver; concrete pens (`TTGlyphPen`, `T2CharStringPen`, `SVGPathPen`, `RecordingPen`, `TransformPen`) compose for construction and conversion pipelines
- subsetting: `Options(**kwargs)` configures feature/hint/name retention; `Subsetter.populate(…)` declares the glyph set; `Subsetter.subset(font)` prunes in-place and must run before `TTFont.save`

[LOCAL_ADMISSION]:
- font binary IO uses `TTFont(path, lazy=True)` for read; tables are accessed by tag key.
- subsetting uses `Subsetter(Options(…))` → `populate(unicodes=…)` → `subset(font)` → `TTFont.save`; never hand-prune table entries.
- outline authoring routes through a concrete `BasePen` subclass; raw glyph table mutations are boundary-only when a pen adapter is not available.
- design space uses `DesignSpaceDocument` with `SourceDescriptor`/`AxisDescriptor`/`InstanceDescriptor` records; `varLib.build` compiles the variable font from the document.

[RAIL_LAW]:
- Package: `fonttools`
- Owns: OpenType/TrueType font container IO, glyph outline pen protocol, variable-font design space management, subsetting, and OpenType Layout feature compilation
- Accept: `TTFont` for binary font work; `BasePen` subclasses for outline authoring; `Subsetter` + `Options` for subsetting; `DesignSpaceDocument` for variable font projects
- Reject: hand-parsed binary font data; raw outline coordinates without a pen adapter; duplicate subset logic where `Subsetter` operates
