# [PY_ARTIFACTS_API_FONTTOOLS]

`fontTools` supplies the OpenType/TrueType font manipulation surface for the artifacts font rail: `TTFont` as the primary binary-font owner, the `pens` protocol family for glyph outline authoring, `designspaceLib` descriptors for variable-font sources and instances, `feaLib` for OpenType Layout feature compilation, and `subset` for glyph-set and feature subsetting. The package owner drives all font IO, outline construction, and subsetting through these surfaces and never re-implements table parsing or glyph-pen primitives that `fontTools` owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fontTools`
- package: `fonttools`
- import: `fontTools`
- owner: `artifacts`
- rail: fonts
- asset: runtime library
- installed: `4.63.0` reflected via `/tmp/wfpy-artifacts315/bin/python`

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core font container
- rail: fonts

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE] | [CAPABILITY]                                                   |
| :-----: | :----------------------- | :------------- | :------------------------------------------------------------- |
|   [1]   | `fontTools.ttLib.TTFont` | font container | `(file, …, lazy, cfg)` — OTF/TTF/WOFF/TTC read-write container |

[PUBLIC_TYPE_SCOPE]: design space descriptors
- rail: fonts

| [INDEX] | [SYMBOL]                                       | [PACKAGE_ROLE]   | [CAPABILITY]                                                      |
| :-----: | :--------------------------------------------- | :--------------- | :---------------------------------------------------------------- |
|   [1]   | `fontTools.designspaceLib.DesignSpaceDocument` | varfont document | `(readerClass, writerClass)` — axes, sources, instances container |
|   [2]   | `fontTools.designspaceLib.SourceDescriptor`    | source record    | `(filename, path, font, name, location, designLocation, …)`       |
|   [3]   | `fontTools.designspaceLib.InstanceDescriptor`  | instance record  | `(filename, path, font, name, location, locationLabel, …)`        |
|   [4]   | `fontTools.designspaceLib.AxisDescriptor`      | axis record      | `(tag, name, minimum, default, maximum, map, …)`                  |

[PUBLIC_TYPE_SCOPE]: subsetting entry points
- rail: fonts

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]   | [CAPABILITY]                                               |
| :-----: | :--------------------------- | :--------------- | :--------------------------------------------------------- |
|   [1]   | `fontTools.subset.Subsetter` | subsetter engine | `(options)` — populate glyphs/unicodes then `subset(font)` |
|   [2]   | `fontTools.subset.Options`   | subset policy    | `(**kwargs)` — features, hints, name-IDs policy            |

[PUBLIC_TYPE_SCOPE]: glyph pen protocol
- rail: fonts

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]   | [CAPABILITY]                                                      |
| :-----: | :------------------------------- | :--------------- | :---------------------------------------------------------------- |
|   [1]   | `fontTools.pens.basePen.BasePen` | outline pen base | `moveTo`, `lineTo`, `qCurveTo`, `curveTo`, `closePath`, `endPath` |
|   [2]   | `fontTools.pens.recordingPen`    | recording pen    | capture and replay outline commands                               |
|   [3]   | `fontTools.pens.svgPathPen`      | SVG path pen     | emit SVG path data from outline commands                          |
|   [4]   | `fontTools.pens.transformPen`    | transform pen    | apply affine transform while forwarding outline commands          |
|   [5]   | `fontTools.pens.ttGlyphPen`      | TrueType pen     | emit TrueType quadratic glyph data to a `TTFont`                  |
|   [6]   | `fontTools.pens.t2CharStringPen` | CFF pen          | emit Type 2 charstring for CFF tables                             |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: font container operations
- rail: fonts

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]    | [CAPABILITY]                                |
| :-----: | :------------------------------------------------- | :---------------- | :------------------------------------------ |
|   [1]   | `TTFont(file, …, lazy, cfg)`                       | construction      | open or create a font container             |
|   [2]   | `TTFont.save(file, reorderTables)`                 | binary export     | write OTF/TTF/WOFF to file or buffer        |
|   [3]   | `TTFont.saveXML(fileOrPath, newlinestr, **kwargs)` | XML export        | write TTX XML representation                |
|   [4]   | `TTFont.keys()`                                    | table enumeration | list all table tags present in the font     |
|   [5]   | `TTFont.getGlyphOrder()`                           | glyph list        | ordered list of glyph names                 |
|   [6]   | `TTFont.getReverseGlyphMap(rebuild)`               | name-to-GID map   | `{name: GID}` mapping                       |
|   [7]   | `TTFont.newTable(tag)`                             | table creation    | allocate a new table object for a given tag |

[ENTRYPOINT_SCOPE]: subsetting
- rail: fonts

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]   | [CAPABILITY]                               |
| :-----: | :------------------------------------------------- | :--------------- | :----------------------------------------- |
|   [1]   | `Subsetter(options)`                               | subsetter init   | initialise with an `Options` policy        |
|   [2]   | `Subsetter.populate(glyphs, gids, unicodes, text)` | glyph selection  | declare the glyph set to retain            |
|   [3]   | `Subsetter.subset(font)`                           | subset execution | prune `font` in-place to the populated set |

[ENTRYPOINT_SCOPE]: design space and variable font
- rail: fonts

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY]  | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------ | :-------------- | :--------------------------------------------------- |
|   [1]   | `DesignSpaceDocument(readerClass, writerClass)`               | document owner  | axes, sources, instances for a variable font project |
|   [2]   | `SourceDescriptor(filename, path, font, name, location, …)`   | source record   | one master font plus its design-space location       |
|   [3]   | `InstanceDescriptor(filename, path, font, name, location, …)` | instance record | one static instance at a design-space position       |
|   [4]   | `AxisDescriptor(tag, name, minimum, default, maximum, …)`     | axis record     | one variation axis with range and optional mapping   |

[ENTRYPOINT_SCOPE]: glyph pen operations
- rail: fonts

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [CAPABILITY]                                      |
| :-----: | :-------------------------- | :------------- | :------------------------------------------------ |
|   [1]   | `BasePen.moveTo(pt)`        | contour start  | begin a new contour at point                      |
|   [2]   | `BasePen.lineTo(pt)`        | line segment   | straight segment to point                         |
|   [3]   | `BasePen.qCurveTo(*points)` | TrueType curve | quadratic spline through implicit on-curve points |
|   [4]   | `BasePen.curveTo(*points)`  | cubic curve    | cubic Bézier to final on-curve point              |
|   [5]   | `BasePen.closePath()`       | closed contour | close the current contour                         |
|   [6]   | `BasePen.endPath()`         | open contour   | end an open contour                               |

## [4]-[IMPLEMENTATION_LAW]

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
