# [PY_ARTIFACTS_API_ZIAMATH]

`ziamath` typesets presentation MathML or a LaTeX expression to a standalone SVG against an OpenType MATH-table font, with no MathJax, LaTeX binary, or rasterizer. It feeds the artifacts figure rail: the emitted SVG string composes into the `drawsvg` diagram canvas, the `document/model` equation flow, and the `typography` annotation plane, recorded under a `ContentIdentity` content-key as one `ArtifactReceipt` case.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ziamath`
- package: `ziamath` (MIT)
- module: `ziamath`
- owner: `artifacts`
- rail: figure
- bundled asset: `ziamath/fonts/STIXTwoMath-Regular.ttf` — the default MATH font, so no system font is bound
- depends: `ziafont` (glyph/outline substrate — the local `ziafont` catalog); `latex2mathml` (LaTeX->MathML front-end the `Latex`/`Text` paths bind, unused by the pure-MathML `Math` path)
- entry points: library only; a `python -m ziamath <latex>` CLI exists, the design composes the in-process API
- capability: MathML/LaTeX -> SVG typesetting, in-place `drawon` onto a caller `ET.Element`, mixed text+`$math$` multi-line paragraphs, equation auto-numbering, OpenType-MATH exploitation (italic-correction kerning, glyph-variant selection, stretchy-delimiter assembly, math constants), custom LaTeX operators, and unicode math-variant styling

## [02]-[RENDER_VOCABULARY]

[RENDER_TYPE_SCOPE]: the bounded `Math`/`Latex`/`Text` render trio

`Math` renders presentation MathML, `Latex` IS-A `Math` on a single LaTeX expression, `Text` owns the mixed multi-line text+`$math$` paragraph; a consumer constructs one with its source and reads the shared `svg`/`svgxml`/`drawon`/`save`/`getsize` egress, never a per-output-format render class.

| [INDEX] | [TYPE]  | [KIND]          | [ROLE]                                                       |
| :-----: | :------ | :-------------- | :----------------------------------------------------------- |
|  [01]   | `Math`  | MathML renderer | `Math(mathml, …)` — presentation-MathML -> node tree -> SVG  |
|  [02]   | `Latex` | LaTeX renderer  | `Latex(latex, …)` — IS-A `Math`; converts via `latex2mathml` |
|  [03]   | `Text`  | mixed paragraph | `Text(s, …)` — text + `$..$`/`$$..$$` math, multi-line       |

[RENDER_TYPE_SCOPE]: layout-geometry value types

`Halign`/`Valign` are the alignment literals threaded through `drawon`; `getsize`/`getyofst` expose the laid-out bbox (a `ziafont.fonttypes.BBox`) the diagram/document owner positions against — `axis` seats an inline equation to the math axis, `base` to the first-text baseline, the vocabulary the AEC annotation plane needs against a leader or dimension line.

| [INDEX] | [TYPE_MEMBER]                                                 | [KIND]   | [ROLE]                                                        |
| :-----: | :------------------------------------------------------------ | :------- | :------------------------------------------------------------ |
|  [01]   | `Halign = Literal['left', 'center', 'right']`                 | align    | horizontal anchor for `drawon`                                |
|  [02]   | `Valign = Literal['top', 'center', 'base', 'axis', 'bottom']` | align    | vertical anchor; `axis`=math-axis, `base`=first-text baseline |
|  [03]   | `<renderer>.getsize() -> tuple[float, float]`                 | geometry | laid-out `(width, height)` in SVG px (bbox extent)            |
|  [04]   | `<renderer>.getyofst() -> float`                              | geometry | y-shift from bbox bottom to baseline (`bbox.ymin`)            |

## [03]-[EGRESS]

[ENTRYPOINT_SCOPE]: SVG document and in-place draw egress

`drawon` is the composition seam: a diagram or document owner threading its own `xml.etree` tree gets the equation drawn in place and the inserted `<g>` back, no string concatenation or temp file; `svg()` is the durable string artifact the content-key records and downstream rasterizers consume.

| [INDEX] | [MEMBER]                                               | [KIND]     | [ROLE]                                                           |
| :-----: | :----------------------------------------------------- | :--------- | :--------------------------------------------------------------- |
|  [01]   | `<renderer>.svg() -> str`                              | serialize  | standalone SVG string — the durable figure artifact              |
|  [02]   | `<renderer>.svgxml() -> ET.Element`                    | serialize  | standalone SVG as `ElementTree` (in-process compose, no reparse) |
|  [03]   | `<renderer>.drawon(svg, x, y, halign, valign)`         | compose    | draw onto a caller `ET.Element`; returns the inserted `<g>`      |
|  [04]   | `<renderer>.save(fname)`                               | serialize  | write the SVG string to a file path                              |
|  [05]   | `Math.mathml2svg(mathml, size=None, font=None) -> str` | serialize  | classmethod one-shot: construct + `svg()`                        |
|  [06]   | `Math.mathmlstr() -> str`                              | introspect | the (possibly LaTeX-derived) MathML source as a string           |
|  [07]   | `<renderer>._repr_svg_() -> str`                       | notebook   | Jupyter inline SVG (notebook boundary only)                      |

[ENTRYPOINT_SCOPE]: LaTeX and mixed-content constructors

`Latex` renders block by default (`inline=True` for inline mode), extracting a `\tag{...}` label; `Math.fromlatex` is its classmethod twin. `Text` parses `$inline$`/`$$display$$` out of a multi-line string, rendering each math run through `Math.fromlatex` and each text run through the math font's `\text{}` mode (or a caller `textfont` via `ziafont.Text`).

| [INDEX] | [MEMBER]                   | [KIND]    | [ROLE]                                                |
| :-----: | :------------------------- | :-------- | :---------------------------------------------------- |
|  [01]   | `Latex(latex, …)`          | construct | single LaTeX expr -> `Math`; `\tag{...}` -> eq number |
|  [02]   | `Math.fromlatex(latex, …)` | construct | classmethod twin of `Latex`                           |
|  [03]   | `Text(s, …)`               | construct | mixed multi-line text + `$..$`/`$$..$$` math          |

## [04]-[CONFIG_AND_STYLE]

[CONFIG_SCOPE]: the global `config` singleton

`config` is a module-level `Config` singleton, not a per-call argument bag: house math/text style, numbering policy, debug overlays, and `svg2`/`precision` egress settings are set once and inherited by every render whose constructor argument is `None`. `config.svg2` toggles SVG 2.0 (compact, `<symbol>` glyph reuse) versus SVG 1.1 (max consumer compatibility), delegating to `ziafont.config.svg2` so the diagram and its embedded equation emit one consistent profile; global mutation runs serialized under the `to_thread` lane.

| [INDEX] | [MEMBER]                                   | [KIND] | [ROLE]                                                                    |
| :-----: | :----------------------------------------- | :----- | :------------------------------------------------------------------------ |
|  [01]   | `config` (`Config` singleton)              | config | the one process-wide style/numbering/render-policy owner                  |
|  [02]   | `config.math` (`MathStyle`)                | config | math font/variant/size/color/background/weight defaults                   |
|  [03]   | `config.text` (`TextStyle`)                | config | text font/variant/size/color/linespacing defaults                         |
|  [04]   | `config.svg2 -> bool`                      | egress | SVG 2.0 (compact, `<symbol>` reuse) vs SVG 1.1 (max compat)               |
|  [05]   | `config.precision -> float`                | egress | SVG coordinate decimal precision                                          |
|  [06]   | `config.minsizefraction` (`float = 0.3`)   | layout | floor for nested script sizes (sub/superscript)                           |
|  [07]   | `config.decimal_separator` (`'.'` / `','`) | layout | LaTeX decimal separator (affects number parsing)                          |
|  [08]   | `config.debug` (`DebugConfig`)             | debug  | `.baseline`/`.bbox` overlays; `.on()`/`.off()` toggles                    |
|  [09]   | `config.numbering` (`NumberingStyle`)      | number | `autonumber`/`format='({0})'`/`format_func`/`columnwidth`; `.getlabel(i)` |

[NUMBERING_SCOPE]: equation numbering

`config.numbering.autonumber` drives a global counter: every `Math`/`Latex` takes the next number formatted by `format` (or a `format_func` callback for roman or section-relative labels), right-aligned to `columnwidth`. A `\tag{label}` or the `number=` constructor argument overrides one equation; `reset_numbering` resets the counter at each numbered-section boundary.

| [INDEX] | [MEMBER]                                        | [KIND] | [ROLE]                                              |
| :-----: | :---------------------------------------------- | :----- | :-------------------------------------------------- |
|  [01]   | `reset_numbering(number=1)`                     | number | reset the global equation counter                   |
|  [02]   | `config.numbering.autonumber` (`bool`)          | number | enable automatic numbering of every equation        |
|  [03]   | `config.numbering.format` / `format_func`       | number | label format string / callback (e.g. roman, `§N.M`) |
|  [04]   | `Latex(..., number='3a')` / `tag{3a}` in source | number | explicit per-equation label override                |

[STYLE_SCOPE]: unicode math-variant styling

`styledchr`/`styledstr` map ASCII `[A-Za-z0-9]` to its unicode math-variant codepoint (the `\mathbb`/`\mathfrak`/`\mathcal` mechanism) through a `MathVariant` descriptor the `mathstyle=` argument and the MathML `mathvariant` attribute both resolve through — the hooks a custom symbol owner reaches when composing math glyph runs directly.

| [INDEX] | [MEMBER]                                       | [KIND] | [ROLE]                                                           |
| :-----: | :--------------------------------------------- | :----- | :--------------------------------------------------------------- |
|  [01]   | `styledchr(char, variant: MathVariant) -> str` | style  | ASCII -> unicode math-variant codepoint                          |
|  [02]   | `styledstr(st, variant: MathVariant) -> str`   | style  | `styledchr` across a string                                      |
|  [03]   | `MathVariant(style='serif', …)`                | style  | the math-variant descriptor `mathstyle`/`mathvariant` resolve to |
|  [04]   | `auto_italic(char) -> bool`                    | style  | whether a single-char identifier auto-italicizes (MathML rule)   |

## [05]-[FONT_AND_MATH_TABLE]

[FONT_SCOPE]: `MathFont` and the OpenType MATH table

`MathFont` extends `ziafont`'s `Font` with the OpenType MATH-table machinery; `font=<path>` typesets against any MATH-table font. `MathTable` exposes the italic-correction script kerning, the glyph-variant ladders for stretchy delimiters and big operators, the `MathAssembly` part assembly, and the `MathConstants` record (axis height, script scale, limit/fraction gaps) governing every layout decision — the renderer walks it internally, and the API is here for a font-engineering or QA owner validating a MATH font's coverage.

| [INDEX] | [MEMBER]                                                        | [KIND] | [ROLE]                                                      |
| :-----: | :-------------------------------------------------------------- | :----- | :---------------------------------------------------------- |
|  [01]   | `MathFont(fname, basesize=24)`                                  | font   | `ziafont.Font` + MATH table; the renderer font owner        |
|  [02]   | `MathFont.findglyph(char, variant: MathVariant) -> SimpleGlyph` | font   | styled char -> glyph (self or alt bold/italic font)         |
|  [03]   | `MathFont.language(script, language)`                           | font   | enable `ssty` math script variants for a script/lang        |
|  [04]   | `MathTable` (`font.math`)                                       | table  | parsed OpenType MATH table                                  |
|  [05]   | `MathTable.consts` (`MathConstants`)                            | table  | axis height, script scale, limit/fraction gaps — layout law |
|  [06]   | `MathTable.kernsuper / kernsub(g1, g2) -> tuple[int, int]`      | table  | italic-correction script kerning                            |
|  [07]   | `MathTable.variant / variant_minmax(glyphid, …)`                | table  | stretchy glyph-variant selection by target size             |
|  [08]   | `MathTable.listvariants(glyphid, vert=True) -> dict[int, int]`  | table  | the available variant ladder for a glyph                    |

[TEX_HOOK_SCOPE]: LaTeX operator extension and conversion

`declareoperator` (at `ziamath.tex`, re-exported at package scope) registers a custom upright LaTeX operator, the `\DeclareMathOperator` equivalent — the Russian/extended trig set (`\tg`/`\ctg`/`\sh`/`\ch`/`\th`) is pre-declared. `tex2mml` exposes the LaTeX->MathML intermediate (with ziamath's `\binom`/`\mathrm`/`||`/`aligned` preprocessing) a consumer caches, diffs, or hands to a MathML consumer.

| [INDEX] | [MEMBER]                            | [KIND] | [ROLE]                                                           |
| :-----: | :---------------------------------- | :----- | :--------------------------------------------------------------- |
|  [01]   | `declareoperator(name: str)`        | tex    | register a custom upright LaTeX operator (`DeclareMathOperator`) |
|  [02]   | `tex2mml(tex, inline=False) -> str` | tex    | LaTeX -> MathML string (with ziamath preprocessing)              |

## [06]-[NODE_TREE]

[NODE_SCOPE]: the laid-out math node tree (advanced/internal)

`Math.node` is the root `Drawable` node tree mirroring the MathML element structure, each node carrying a `bbox` and a `draw(x, y, svg)` method; the `drawable` primitive shapes are the leaf marks it composes (fraction bars, radical lines, `menclose` boxes). Internal layout machinery — a consumer reads `Math.node.bbox` for geometry and never constructs nodes — exposed so a font/layout QA owner can introspect the bounded MathML grammar.

| [INDEX] | [MEMBER]                                                     | [KIND] | [ROLE]                                                         |
| :-----: | :----------------------------------------------------------- | :----- | :------------------------------------------------------------- |
|  [01]   | `Math.node` (`nodes.Mnode`)                                  | tree   | the root laid-out math node (carries `.bbox`)                  |
|  [02]   | `nodes.Mnode.fromelement(element, parent) -> Mnode`          | tree   | build the node tree from a MathML `ET.Element`                 |
|  [03]   | `nodes.{Mrow, Mfrac, Msqrt, …, Mphantom}` (lead rosters all) | tree   | the closed MathML-element node family                          |
|  [04]   | `drawable.{Glyph,HLine,VLine,Box,Diagonal,Ellipse}`          | leaf   | primitive marks the tree composes (bars, radicals, enclosures) |

## [07]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- render trio: `Math` (MathML), `Latex` IS-A `Math` (one LaTeX expression), `Text` (mixed multi-line); a consumer constructs one with its source and reads the shared egress, never a per-output-format render class.
- egress: `svg()` (string) is the durable artifact; `svgxml()` (`ET.Element`) composes in-process without a reparse; `drawon(svg, x, y, halign, valign)` draws onto a caller `ET.Element` and returns the inserted `<g>`; `save(fname)` writes the file.
- config is global: one `Config` singleton owns house math/text style, numbering, debug overlays, and `svg2`/`precision` egress, set once and inherited where a constructor argument is `None`; `config.svg2` matches the embedded equation to the host `drawsvg`/PDF SVG profile via `ziafont.config.svg2`.
- font: bundled STIX Two Math is the default; `font=<path>` typesets any MATH-table font through `MathFont` (a `ziafont.Font` subclass), and `MathTable`/`MathConstants`/the variant ladders are the deep substrate (italic correction, stretchy delimiters, big-operator variants) the renderer walks internally.
- offload: the GIL-bound lxml-free kernel — `xml.etree` parse, MATH-table font walk, global `config` read/mutate, SVG serialization — runs through the runtime `to_thread.run_sync(..., limiter=...)` seam, the same arm the `drawsvg` emitter and GIL-releasing `rustworkx` layout take, never a `to_interpreter` subinterpreter arm (the global mutable `config` and C-extension `ziafont`/`numpy` neighbors are subinterpreter-hostile).
- evidence: each render captures `getsize()`, `getyofst()`, source kind (`mathml`/`latex`), eq-number label, `config.svg2` profile, and SVG byte length as a figure `ArtifactReceipt`, keyed by `ContentIdentity` over the `(spec ⊕ font ⊕ operators ⊕ effective-config)` bytes minted pre-run — `effective-config` the render-affecting set resolved from each constructor argument or its inherited `config` default, so two renders differing only in an inherited default still key distinctly.

[STACKING]:
- `drawsvg`(`.api/drawsvg.md`): a formula-bearing annotation renders `zm.Latex(formula, color=<palette>).svg()` to a string the `visualization/diagram/draw#DRAW` `_lower` `Annotation` arm wraps as a `drawsvg.Raw` fragment (or `drawsvg.Image(..., data=svg_as_utf8_data_uri(...), embed=True)`), bucketed into the same `GlyphStyle.layer` named `Group` as every mark, so the callout composes into the named-layer SVG the `export/layered#LAYERED` owner binds — one more `Annotation` glyph, not a parallel renderer, its `color=` threaded from the `graphic/color/derive#DERIVE` palette index.
- `drawsvg`(`.api/drawsvg.md`) shared-tree path: when the diagram owner and ziamath share one `xml.etree` tree, `zm.Math(...).drawon(svg, x, y, halign, valign)` draws the equation at the laid-out anchor and returns the inserted `<g>`, and `getsize()`/`getyofst()` feed the layout owner the bbox and baseline so the equation seats against a leader, dimension line, or grid bubble (`valign='axis'`/`'base'`).
- `document/model`(`.planning/document/model.md`): a `DocumentNode` equation block renders `zm.Latex(latex, number=...).svg()` (or `config.numbering.autonumber` for section-relative labels) into the rich-text flow as an inline figure, `reset_numbering` called at each section boundary and `config.svg2` set to the document's PDF/print profile so `weasyprint`/`pikepdf` embed a consistent vector.
- `ziafont`(`.api/ziafont.md`): ziamath IS BUILT ON ziafont — it composes the `Font`/glyph surface for non-math glyph runs inside an equation, so the two share the `<path>`/`<svg>` egress and a diagram owner mixes a plain `ziafont` label and a `ziamath` equation on one SVG canvas.
- runtime rails: the kernel runs on the `to_thread.run_sync` seam, the `svg` string is wrapped in an `expression`-`Result`/`Try` rail at the boundary (malformed LaTeX or an absent MATH font is a typed failure, never a raised exception crossing the async edge), and the bytes are content-keyed via `ContentIdentity.of` into one `ArtifactReceipt` case with structlog/OTel spans over the render boundary.

[LOCAL_ADMISSION]:
- a formula renders `zm.Latex(formula).svg()`, or `zm.Math(...).drawon(tree, x, y, halign, valign)` when sharing one `ET.Element` tree — never a hand-built MathML->SVG layout where the render trio owns it.
- mixed text-and-math is `zm.Text(...)` and a single expression is `zm.Latex(...)`; the render/font/numbering policy is set once on the global `config`, never threaded per call, and a custom engineering operator registers once via `zm.declareoperator(r'\Rd')` at module init.
- `styledchr`/`MathVariant`/`tex2mml` serve a typography/symbol owner composing math glyph runs directly, hooks the surface-level `Latex(...).svg()` path never reaches.

[RAIL_LAW]:
- Package: `ziamath`
- Owns: math/LaTeX -> SVG typesetting, the `Math`/`Latex`/`Text` render trio, the OpenType-MATH-table layout (italic correction, stretchy delimiters, big-operator variants, math constants via `MathFont`/`MathTable`), equation auto-numbering (`config.numbering`/`reset_numbering`), unicode math-variant styling (`styledchr`/`MathVariant`), custom LaTeX operator registration (`declareoperator`), and the SVG-string/`ET.Element`/in-place-`drawon` egress with SVG 2.0/1.1 profile control (`config.svg2`)
- Accept: mathematical/engineering formulas rendered as durable SVG composited into the `drawsvg` diagram canvas (`Raw`/`Image` or in-place `drawon`), the `document/model` rich-text equation flow with numbering, and the `typography`/symbol annotation plane wherever a label exceeds plain shaped text — all under the `to_thread` offload + `Result` rail + `ContentIdentity` receipt spine
- Reject: glyph outline extraction and the base `Font`/`Text` substrate where `ziafont` owns it; LaTeX->MathML front-end parsing where `latex2mathml` owns it; SVG-fragment layout/transform/bbox alongside other marks where `drawsvg`/`svgelements` own it; rasterization of the emitted SVG where `resvg-py`/`vl-convert`/`pyvips` own it; plain non-math shaped text where `typography/shape`/`uharfbuzz` owns it; a per-output-format render-class family where the one trio + shared egress discriminates; a raised exception crossing the async edge where the boundary `Result` rail owns failure; a subinterpreter offload arm where the global mutable `config` and C-extension neighbors force `to_thread`; identity minting the runtime owns
