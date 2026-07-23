# [PY_ARTIFACTS_API_ZIAFONT]

`ziafont` outlines text to SVG `<path>` geometry with zero runtime dependencies: it reads a `glyf`/`CFF`/`CFF2` sfnt through the font's own cmap, shapes with full GSUB and GPOS, and lowers each glyph to an `xml.etree.ElementTree` `<path>` — surviving where the font is absent, the gap `drawsvg.Text`'s font-dependent `<text>` leaves. `Font` owns load, shape, and per-glyph access; `Text` owns the shaped run that serializes standalone or composites into an SVG tree via `drawon`. Math layout is `ziamath` and color-glyph raster is `blackrenderer`; this catalog owns the sfnt-to-`<path>` fold alone.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ziafont`
- package: `ziafont` (MIT)
- module: `ziafont`
- namespaces: `ziafont`, `ziafont.glyph`, `ziafont.fonttypes`, `ziafont.gsub`, `ziafont.gpos`, `ziafont.inspect`
- owner: `artifacts`
- rail: glyphset
- depends: none — parses the sfnt binary itself, bundles `ziafont/fonts/DejaVuSans.ttf` as the no-argument fallback, and emits stdlib `xml.etree.ElementTree`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the two owners, the shared glyph family, and the value records

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]        | [CAPABILITY]                                                                      |
| :-----: | :---------------------------- | :------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `ziafont.Font`                | load+shape owner     | parse sfnt, own cmap/GSUB/GPOS, glyph access, run measure, script/language select |
|  [02]   | `ziafont.Text`                | shaped-run owner     | shaped multi-line run; `.svg()`/`.svgxml()`, `.drawon(svg, x, y)` composite       |
|  [03]   | `ziafont.glyph.SimpleGlyph`   | outline glyph        | one `glyf`/`CFF` outline on the shared glyph interface                            |
|  [04]   | `ziafont.glyph.CompoundGlyph` | composite glyph      | component glyphs and transforms on the shared interface                           |
|  [05]   | `ziafont.glyph.EmptyGlyph`    | missing glyph        | `.notdef`/unmapped fallback on the shared interface                               |
|  [06]   | `ziafont.fonttypes.BBox`      | bounds record        | `(xmin, xmax, ymin, ymax)` font-unit bounds `namedtuple`                          |
|  [07]   | `ziafont.fonttypes.Xform`     | transform record     | the affine a `CompoundGlyph` component carries, and a placed run's rotation       |
|  [08]   | `ziafont.fonttypes.FontInfo`  | font metrics         | parsed `head`/`hhea`/`OS/2` metrics — units-per-em, ascent/descent, advance       |
|  [09]   | `ziafont.config`              | global render policy | the `Config` render policy, set once and read by every serialize                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Font` load, shape, and glyph access
- run tail (shared, `Font.text` and `Text`): `size`, `linespacing`, `halign`, `valign`, `color`, `rotation`, `rotation_mode`

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `Font(name, searchpaths)`                    | ctor     | parse sfnt, or the bundled `DejaVuSans`  |
|  [02]   | `Font.text(s, ...) -> Text`                  | instance | shaped multi-line run                    |
|  [03]   | `Font.glyph(char) -> SimpleGlyph`            | instance | shared-interface glyph for one character |
|  [04]   | `Font.glyph_fromid(glyphid) -> SimpleGlyph`  | instance | glyph for a shaped gid (post-GSUB)       |
|  [05]   | `Font.glyphindex(char) -> int`               | instance | resolve a character to its cmap glyph id |
|  [06]   | `Font.advance(glyph, glyph2) -> int`         | instance | GPOS-kerned advance of `glyph`           |
|  [07]   | `Font.getsize(s) -> tuple[float, float]`     | instance | shaped `(width, height)` in font units   |
|  [08]   | `Font.scripts() -> list[str]`                | instance | OpenType script tags                     |
|  [09]   | `Font.languages(script='DFLT') -> list[str]` | instance | language-system tags under a script      |
|  [10]   | `Font.language(script, language)`            | instance | select the active script and language    |
|  [11]   | `Font.usecmap(cmapidx)`                      | instance | select a cmap subtable                   |
|  [12]   | `Font.verifychecksum()`                      | instance | validate the sfnt table checksums        |

- `Font.advance`: applies the GPOS kern here, un-kerned when `glyph2` is `None`.
- `Font.glyph_fromid`: returns a `CompoundGlyph` for a composite gid.

[ENTRYPOINT_SCOPE]: `Text` run serialization and compositing

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :-------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `Text.svg() -> str`                     | instance | standalone `<svg>` document string                     |
|  [02]   | `Text.svgxml() -> ET.Element`           | instance | run as an ET `<svg>` element for in-process build      |
|  [03]   | `Text.drawon(svg, x, y)`                | instance | composite `<symbol>` defs and a `<g>` into an SVG tree |
|  [04]   | `Text.getsize() -> tuple[float, float]` | instance | rendered `(width, height)` of the laid-out run         |
|  [05]   | `Text.getyofst() -> float`              | instance | baseline-to-top offset for vertical alignment          |
|  [06]   | `Text.str_to_gids()`                    | instance | the shaped post-GSUB glyph-id sequence                 |
|  [07]   | `Text(s, font, ...)`                    | ctor     | direct run construction; `s` accepts a gid sequence    |

- `Text.drawon`: appends into a caller `ET.Element` at `(x, y)` — the seam a diagram owner uses to place outlined text into the tree it is already building.

[ENTRYPOINT_SCOPE]: the shared glyph interface — `SimpleGlyph`, `CompoundGlyph`, `EmptyGlyph`

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                                               |
| :-----: | :---------------------------------------------------- | :------- | :--------------------------------------------------------- |
|  [01]   | `SimpleGlyph.svgpath(x0, y0, scale_factor)`           | instance | raw `<path>` (`d` = outline in user units) — the primitive |
|  [02]   | `SimpleGlyph.place(x, y, point_size)`                 | instance | positioned `<use>` for the `<symbol>` flow                 |
|  [03]   | `SimpleGlyph.svg(point_size) -> str`                  | instance | standalone single-glyph `<svg>` string                     |
|  [04]   | `SimpleGlyph.svgxml(point_size) -> ET.Element`        | instance | standalone single-glyph `<svg>` element                    |
|  [05]   | `SimpleGlyph.svgsymbol() -> ET.Element`               | instance | the reusable `<symbol>` def the `<use>` references         |
|  [06]   | `SimpleGlyph.advance(nextchr) -> int`                 | instance | GPOS-kerned advance to `nextchr`                           |
|  [07]   | `SimpleGlyph.funits_to_points(value, scale_factor)`   | instance | font-unit to point conversion at a scale                   |
|  [08]   | `SimpleGlyph.describe() -> DescribeGlyph`             | instance | glyph-metrics description                                  |
|  [09]   | `SimpleGlyph.test(pxwidth, pxheight) -> InspectGlyph` | instance | labeled point/contour inspection SVG                       |
|  [10]   | `SimpleGlyph.viewbox`                                 | property | the glyph's `(x, y, w, h)` SVG viewBox                     |

- `SimpleGlyph.svgpath`: returns `None` for an empty outline; the `d` is offset to `(x0, y0)`.
- `CompoundGlyph`, `EmptyGlyph`: the same interface as `SimpleGlyph` — a component-composite outline and the `.notdef`/unmapped fallback, never special-cased.

[ENTRYPOINT_SCOPE]: font discovery and feature inspection

| [INDEX] | [SURFACE]                                | [SHAPE] | [CAPABILITY]                                                             |
| :-----: | :--------------------------------------- | :------ | :----------------------------------------------------------------------- |
|  [01]   | `find_font(name, paths) -> Path`         | static  | resolve a family name to a font file path                                |
|  [02]   | `system_fonts(paths) -> dict[str, Path]` | static  | map every discovered family to its path                                  |
|  [03]   | `inspect.DescribeFont(font)`             | ctor    | font metrics + script/language table (`.table()`, `.format_languages()`) |
|  [04]   | `inspect.ShowGlyphs(font)`               | ctor    | a glyph-roster inspection grid (`.table()`)                              |
|  [05]   | `inspect.LookupDisplay(...)`             | ctor    | per-lookup before/after render (`.table()`, `.svg_for_gid(gid)`)         |

- `find_font`, `system_fonts`: return `None` and an empty map when nothing resolves.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- two owners: `Font` owns load, cmap, GSUB, GPOS, and glyph access; `Text` owns the laid-out run and its SVG egress. `Font.text` returns a `Text`, so one run type exists — a design constructs through `Font.text` and reads `.svg()`/`.svgxml()`/`.drawon(...)`.
- one glyph interface: `SimpleGlyph`, `CompoundGlyph`, and `EmptyGlyph` expose one surface, and `glyph.svgpath(...)` decodes the right outline (TrueType `glyf`, PostScript `CFF` charstring, composite, or empty `.notdef`) internally, so a caller never branches on outline kind.
- path primitive: `SimpleGlyph.svgpath(x0, y0, scale_factor)` returns a raw `xml.etree.ElementTree` `<path>` (`d` = outline in user units offset to `(x0, y0)`), the font-independent geometry a `drawsvg`/`svgelements`/`ezdxf` owner appends into its own tree, so outlined text survives where the font is absent.
- shaping is owned: `Font` carries its own `gsub.Gsub`/`gpos.Gpos` engines, so GSUB substitution and GPOS positioning apply through `Font.text`/`Font.advance` under `Font.language(script, language)` — the reason ziafont needs no external shaper.
- config is global: `ziafont.config` is a process-wide `Config` — `svg2` (default `True`) emits SVG2 `<path>`/CSS versus SVG1.x `<symbol>`/`<use>`, `precision` (default `3`) bounds the `d`-float places and is the content-key stability lever, `fontsize` (default `48`) sets the default point size, `debug` (default `False`) draws bbox/baseline overlays; a design sets it once at boundary scope, never per call.

[STACKING]:
- `visualization/diagram/draw#DRAW`: its `Annotation`/`Node`/`Swimlane`/`Edge` arms emit `drawsvg.Text`, a font-dependent `<text>`; a `FONT_OUTLINE` arm instead builds one `Font` and calls `Font(...).text(label, size=...).drawon(group, x, y)` to composite `<symbol>` defs and a `<g>` into the `GlyphStyle.layer` group's ET tree, so the label is outlined `<path>` inside the named SVG layer `export/layered#LAYERED` binds. `drawsvg` owns the mark shapes, `ziafont` the text-to-outline.
- `visualization/diagram/glyphset#GLYPHSET`: its label text mark resolves to a `ziafont` outline through this catalog — the glyph vocabulary stays geometry and style, and `ziafont` is the draw-side outline engine the label folds into, never a new glyph case.
- `typography/shape#SHAPE`: `SimpleGlyph.svgpath(x0, y0, scale_factor)` yields the per-glyph `<path>` positioned along a curve and `Text.drawon` yields the composited run — the path-only seam, composed exactly as `blackrenderer`'s path-only `saveImage`. `uharfbuzz` (`.api/uharfbuzz.md`) shapes the main full-Unicode/bidi/complex-script run; `ziafont` outlines the self-contained label or caption to a `<path>` with its own adequate GSUB/GPOS.
- `ziamath` (`.api/ziamath.md`): built on this `Font`/glyph surface for the non-math glyph runs inside an equation — a math annotation routes to `ziamath`, a plain label to `ziafont`, both sharing the `<path>`/`<svg>` egress so a diagram owner mixes them on one canvas.
- `typography/font#FONT`: no shared model with `fonttools` — a `FREEZE`/`SUBSET` transform stays on `fonttools`, and the subset font file feeds `ziafont.Font(path)` for outlining; the two are sequential, never a shared `TTFont`.
- boundary rails: `msgspec` (`libs/python/.api/msgspec.md`) frames the typed run/font policy struct and `beartype` (`libs/python/.api/beartype.md`) boundary-validates it; `find_font`/`system_fonts` and the `Font` parse ride the `expression` `Result` rail (`libs/python/.api/expression.md`), so a missing family or malformed sfnt surfaces as a typed failure. `ContentIdentity` (`libs/python/.api/xxhash.md`) content-keys the outlined bytes with `config.precision` fixed.
- notebook display: `Text`, `SimpleGlyph`, and `CompoundGlyph` carry `_repr_svg_` for inline `great-tables`/Jupyter cells.

[LOCAL_ADMISSION]:
- text-to-outline is `Font(...).text(s, ...).svg()`/`.svgxml()`/`.drawon(tree, x, y)`, or per-glyph `Font(...).glyph(ch).svgpath(x0, y0, scale_factor)`.
- shaping is `Font.text`/`Font.advance` under `Font.language(script, language)`; the owned `gsub.Gsub`/`gpos.Gpos` engines apply the lookup-type dispatch.
- run/font policy is a typed `msgspec.Struct` over `family`/`size`/`linespacing`/`halign`/`valign`/`color`/`rotation`/`script`/`language`; the render policy is `ziafont.config`, set once.
- discovery binds the family through `system_fonts`/`find_font` and answers font-feature coverage through `inspect.DescribeFont`/`LookupDisplay`; the parse and discovery ride the `RuntimeRail` so a missing family or malformed sfnt is a typed failure.

[RAIL_LAW]:
- Package: `ziafont`
- Owns: pure-Python text-to-SVG-`<path>` outlining — sfnt (`glyf`/`CFF`/`CFF2`) read through the font's own cmap, full GSUB and GPOS under a chosen script and language, per-glyph `<path>`/`<symbol>`/`<svg>` emission, and a shaped multi-line run serializing standalone or compositing into an SVG tree, with `system_fonts`/`find_font` discovery and the `inspect` feature-QA surface
- Accept: a `FONT_OUTLINE` arm on `visualization/diagram/draw#DRAW` outlining label text to `<path>` inside the named SVG layer; the text-mark outline source `visualization/diagram/glyphset#GLYPHSET` admits; the outlined-label / text-on-path seam `typography/shape#SHAPE` composes via `SimpleGlyph.svgpath`/`Text.drawon`; the `Font`/glyph surface `ziamath` builds equation glyph runs on; the typed run/font policy struct and the global `config`
- Reject: a font-dependent `<text>` element where outlined `<path>` geometry is the goal; a hand-walked `glyf`/`CFF` charstring or cmap lookup where `SimpleGlyph` decodes; a hand-rolled kern/ligature/alternate substitution where `gsub.Gsub`/`gpos.Gpos` apply it; a full-document complex-script run where `uharfbuzz` is the shaper, or a caption-to-outline routed to the heavier `uharfbuzz`+`blackrenderer` stack; a math-bearing annotation where the `ziamath` sibling owns it; `svg2`/`precision`/`fontsize` threaded per call where the global `ziafont.config` owns the render policy
