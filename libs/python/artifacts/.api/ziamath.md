# [PY_ARTIFACTS_API_ZIAMATH]

`ziamath` is the categorical-best pure-Python math-to-SVG typesetter for the artifacts figure/diagram/document rail: it parses a MathML or LaTeX expression, lays it out against an OpenType **MATH**-table font (bundled STIX Two Math, or any caller-supplied `.ttf`/`.otf` with a MATH table), and emits a standalone SVG document OR draws the laid-out equation directly onto an existing `xml.etree.ElementTree` SVG tree at an aligned anchor — no MathJax/Node/browser, no LaTeX binary, no rasterizer. The render trio `Math`/`Latex`/`Text` is the bounded entry vocabulary; `Math` renders presentation MathML, `Latex` renders a single LaTeX expression (via `latex2mathml`), and `Text` renders a mixed text-and-`$math$` paragraph with multi-line/rotation/alignment. The package owner composes the equation SVG INTO the `drawsvg`-authored diagram canvas (a math glyph SVG embedded as a `drawsvg.Raw` fragment or `svg_as_utf8_data_uri` `Image`, or — when both sides share one `ET.Element` tree — drawn in place via `Math.drawon`), into the `document/model` rich-text equation flow, and into the `typography` annotation plane wherever a mathematical/engineering label exceeds plain shaped text; it never rasterizes (that routes to `resvg-py`/`vl-convert`/`pyvips` over the emitted SVG string), never re-implements glyph outline extraction (that is the `ziafont` `Font`/`SimpleGlyph` substrate it extends), and runs its XML+font-layout kernel off the event loop through the runtime `to_thread.run_sync` seam (the lxml-free `xml.etree` parse, the font-table walk, and the global `config` mutation are all GIL-bound, shared-address-space work — never a subinterpreter arm). The emitted SVG string is the durable artifact, recorded under a `ContentIdentity` content-key as one `ArtifactReceipt` case.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ziamath`
- package: `ziamath`
- import: `ziamath`
- owner: `artifacts`
- rail: figure
- license: MIT (`License :: OSI Approved :: MIT License`)
- installed: `0.13`
- build-floor: `Requires-Python >=3.8`; pure-Python wheel (`py3-none-any`), abi-agnostic — cp315 wheel resolves and installs unconditionally, NO `python_version` gate
- requires: `ziafont>=0.10` (OpenType glyph/outline substrate — the local `ziafont` catalog), `latex2mathml` (LaTeX->MathML front-end; required for the `Latex`/`fromlatex`/`Text` paths, optional for the pure-MathML `Math` path)
- bundled asset: `ziamath/fonts/STIXTwoMath-Regular.ttf` (the default MATH font; no system font needed)
- entry points: none (library only); a `__main__` CLI exists (`python -m ziamath <latex>`) but the design composes the in-process API
- capability: typeset presentation MathML or LaTeX to a standalone SVG document tree, draw a laid-out equation onto an existing `ET.Element` SVG canvas at a horizontal/vertical anchor, render mixed text-plus-`$math$` multi-line paragraphs with rotation and alignment, drive equation auto-numbering with a caller format/callback, read and exploit the OpenType MATH table (italic-correction kerning, vertical/horizontal glyph-variant selection, stretchy-delimiter assembly, math constants) through `MathFont`/`MathTable`, register custom LaTeX operator names, and apply unicode math-variant styling (bold/italic/script/fraktur/double-struck/mono/sans) — all without a browser, a LaTeX install, or a rasterizer

## [02]-[RENDER_VOCABULARY]

[RENDER_TYPE_SCOPE]: the bounded `Math`/`Latex`/`Text` render trio
- rail: figure

The three render owners are the closed entry algebra a figure/diagram/document consumer dispatches over: `Math` takes presentation MathML (`str` or `ET.Element`), `Latex` IS-A `Math` specialized on a single LaTeX expression, and `Text` is the standalone mixed-content paragraph owner (text interleaved with `$inline$`/`$$display$$` math, multi-line, rotatable). A consumer constructs one of the three with its source string, then reads the same egress surface (`svg`/`svgxml`/`drawon`/`save`/`getsize`) off the result — there is no per-output-format render family. `Latex` subclasses `Math`, so a LaTeX equation IS a `Math` and shares its entire egress; `Text` is independent (it owns its own line-breaking layout) but mirrors the egress names.

| [INDEX] | TYPE    | KIND               | ROLE                                                                                  |
| :-----: | ------- | ------------------ | ------------------------------------------------------------------------------------- |
|  [01]   | `Math`  | MathML renderer    | `Math(mathml, size=None, font=None, title=None, number=None, margin=1.0)` — presentation-MathML -> laid-out node tree -> SVG |
|  [02]   | `Latex` | LaTeX renderer     | `Latex(latex, size=None, mathstyle=None, font=None, color=None, inline=False, title=None, number=None, margin=1.0)` — IS-A `Math`; converts via `latex2mathml` |
|  [03]   | `Text`  | mixed paragraph    | `Text(s, textfont=None, mathfont=None, mathstyle=None, size=None, linespacing=None, color=None, halign='left', valign='base', rotation=0, rotation_mode='anchor', title=None)` — text + `$..$`/`$$..$$` math, multi-line |

[RENDER_TYPE_SCOPE]: layout-geometry value types
- rail: figure

`Halign`/`Valign` are the closed alignment literals threaded through `drawon`; `getsize`/`getyofst` expose the laid-out bounding-box geometry the diagram/document owner needs to position the equation (the bbox is a `ziafont.fonttypes.BBox` over the node tree). The vertical-alignment vocabulary is richer than plain text — `axis` aligns to the math axis (minus-sign height above baseline), `base` to the first text element's baseline — which the AEC annotation plane needs to seat an inline equation correctly against a leader or dimension line.

| [INDEX] | TYPE / MEMBER                                          | KIND     | ROLE                                                              |
| :-----: | ------------------------------------------------------ | -------- | ---------------------------------------------------------------- |
|  [01]   | `Halign = Literal['left', 'center', 'right']`          | align    | horizontal anchor for `drawon`                                   |
|  [02]   | `Valign = Literal['top', 'center', 'base', 'axis', 'bottom']` | align | vertical anchor; `axis` = math-axis, `base` = first-text baseline |
|  [03]   | `<renderer>.getsize() -> tuple[float, float]`          | geometry | laid-out `(width, height)` in SVG px (bbox extent)               |
|  [04]   | `<renderer>.getyofst() -> float`                       | geometry | y-shift from bbox bottom to baseline (`bbox.ymin`) for seating   |

## [03]-[EGRESS]

[ENTRYPOINT_SCOPE]: SVG document and in-place draw egress
- rail: figure

Every renderer exposes one egress family: `svg()` returns the standalone SVG **string** (the durable artifact), `svgxml()` returns the same as an `ET.Element` tree (for in-process composition without a parse round-trip), `drawon(svg, x, y, halign, valign)` draws the laid-out equation onto a CALLER-PROVIDED `ET.Element` at an aligned anchor and returns the inserted `<g>` group, and `save(fname)` writes the SVG file. `drawon` is the load-bearing composition seam: a diagram or document owner that builds its own `xml.etree` SVG tree threads the equation in directly, no string concatenation, no temp file. `Math.mathml2svg(mathml, size, font)` is the one-shot classmethod shortcut (construct + `svg()` in one call). The string egress is what the figure owner records under the content-key and what downstream rasterizers consume.

| [INDEX] | MEMBER                                                                          | KIND       | ROLE                                                       |
| :-----: | ------------------------------------------------------------------------------- | ---------- | ---------------------------------------------------------- |
|  [01]   | `<renderer>.svg() -> str`                                                        | serialize  | standalone SVG string — the durable figure artifact        |
|  [02]   | `<renderer>.svgxml() -> ET.Element`                                              | serialize  | standalone SVG as `ElementTree` (in-process compose, no reparse) |
|  [03]   | `<renderer>.drawon(svg, x=0, y=0, halign='left', valign='base') -> ET.Element`   | compose    | draw onto a caller `ET.Element`; returns the inserted `<g>` |
|  [04]   | `<renderer>.save(fname)`                                                         | serialize  | write the SVG string to a file path                        |
|  [05]   | `Math.mathml2svg(mathml, size=None, font=None) -> str`                           | serialize  | classmethod one-shot: construct + `svg()`                  |
|  [06]   | `Math.mathmlstr() -> str`                                                        | introspect | the (possibly LaTeX-derived) MathML source as a string     |
|  [07]   | `<renderer>._repr_svg_() -> str`                                                 | notebook   | Jupyter inline SVG (notebook boundary only)                |

[ENTRYPOINT_SCOPE]: LaTeX and mixed-content constructors
- rail: figure

`Latex(latex, ...)` is the primary LaTeX path (block mode by default, `inline=True` for inline mode); it also extracts a `\tag{...}` equation label inline. `Math.fromlatex` is the equivalent classmethod on `Math`. `Text` parses `$inline$`/`$$display$$` delimiters out of a multi-line string and renders each math run through `Math.fromlatex` and each text run through the math font's `\text{}` mode (or a caller `textfont` via `ziafont.Text`), composing one SVG with line spacing, alignment, and optional rotation. `Math.fromlatextext` is DEPRECATED (it warns) — the brief-correct mixed-content owner is `Text`.

| [INDEX] | MEMBER                                                                          | KIND      | ROLE                                                       |
| :-----: | ------------------------------------------------------------------------------- | --------- | ---------------------------------------------------------- |
|  [01]   | `Latex(latex, size=None, mathstyle=None, font=None, color=None, inline=False, title=None, number=None, margin=1.0)` | construct | single LaTeX expression -> `Math`; `\tag{...}` -> eq number |
|  [02]   | `Math.fromlatex(latex, size=None, mathstyle=None, font=None, color=None, inline=False, margin=1.0)` | construct | classmethod twin of `Latex`                                |
|  [03]   | `Text(s, textfont=None, mathfont=None, mathstyle=None, size=None, linespacing=None, color=None, halign='left', valign='base', rotation=0, rotation_mode='anchor', title=None)` | construct | mixed multi-line text + `$..$`/`$$..$$` math               |
|  [04]   | `Math.fromlatextext(latex, ...)`                                                 | construct | [DEPRECATED] warns; use `Text` for mixed content           |

## [04]-[CONFIG_AND_STYLE]

[CONFIG_SCOPE]: the global `config` singleton
- rail: figure

`config` is a module-level `Config` singleton (one process-wide owner) — NOT a per-call argument bag. It carries default math/text style, equation-numbering policy, debug overlays, and SVG-version/precision settings. The render trio reads `config.math.fontsize`/`config.math.mathfont`/`config.text.*`/`config.numbering.*` when the corresponding constructor argument is `None`, so a document sets the house defaults ONCE on `config` and every subsequent render inherits them. `config.svg2` is the load-bearing egress control: it toggles SVG 2.0 output (compact, uses `<symbol>` glyph reuse) versus SVG 1.1 (larger, maximal browser/consumer compatibility) — it delegates to the underlying `ziafont.config.svg2`, mirroring the SVG-version concern the `drawsvg` owner also exposes, so the diagram and the embedded equation emit at one consistent SVG profile. Because `config` is global mutable state, any render that overrides it must do so under the serialized `to_thread` offload lane, never concurrently across the event loop.

| [INDEX] | MEMBER                                                          | KIND   | ROLE                                                       |
| :-----: | -------------------------------------------------------------- | ------ | ---------------------------------------------------------- |
|  [01]   | `config` (`Config` singleton)                                  | config | the one process-wide style/numbering/render-policy owner   |
|  [02]   | `config.math` (`MathStyle`)                                    | config | `mathfont`/`variant`/`fontsize`/`color`/`background`/`bold_font`/`italic_font`/`bolditalic_font` defaults |
|  [03]   | `config.text` (`TextStyle`)                                    | config | `textfont`/`variant`/`fontsize`/`color`/`linespacing` defaults |
|  [04]   | `config.svg2 -> bool` (delegates `ziafont.config.svg2`)        | egress | SVG 2.0 (compact, `<symbol>` reuse) vs SVG 1.1 (max compat) |
|  [05]   | `config.precision -> float` (delegates `ziafont.config`)       | egress | SVG coordinate decimal precision                           |
|  [06]   | `config.minsizefraction` (`float = 0.3`)                       | layout | floor for nested script sizes (sub/superscript)            |
|  [07]   | `config.decimal_separator` (`'.'` / `','`)                     | layout | LaTeX decimal separator (affects number parsing)           |
|  [08]   | `config.debug` (`DebugConfig`)                                 | debug  | `.baseline`/`.bbox` overlays; `.on()`/`.off()` toggles     |
|  [09]   | `config.numbering` (`NumberingStyle`)                          | number | `autonumber`/`format='({0})'`/`format_func`/`columnwidth`; `.getlabel(i)` |

[NUMBERING_SCOPE]: equation numbering
- rail: figure

Equation auto-numbering is a global counter driven by `config.numbering`: set `config.numbering.autonumber = True` and every `Math`/`Latex` gets the next number formatted by `config.numbering.format` (or a `format_func` callback for roman/section-relative labels), right-aligned to `columnwidth`. A `Latex` `\tag{label}` overrides with an explicit label; the `number=` constructor argument sets one explicitly. `reset_numbering(n)` resets the counter — the document owner calls it at each numbered-equation section boundary.

| [INDEX] | MEMBER                                          | KIND   | ROLE                                                       |
| :-----: | ----------------------------------------------- | ------ | ---------------------------------------------------------- |
|  [01]   | `reset_numbering(number=1)`                     | number | reset the global equation counter                          |
|  [02]   | `config.numbering.autonumber` (`bool`)          | number | enable automatic numbering of every equation               |
|  [03]   | `config.numbering.format` / `format_func`       | number | label format string / callback (e.g. roman, `§N.M`)        |
|  [04]   | `Latex(..., number='3a')` / `\tag{3a}` in source | number | explicit per-equation label override                       |

[STYLE_SCOPE]: unicode math-variant styling
- rail: figure

`styledchr(char, variant)` maps an ASCII `[A-Za-z0-9]` to its unicode math-variant codepoint (bold/italic/script/fraktur/double-struck/mono/sans) per the MathML `mathvariant` algebra — the mechanism behind `\mathbb`/`\mathfrak`/`\mathcal`. `styledstr` applies it across a string. `MathVariant` is the style descriptor (`style`/`italic`/`bold`/`normal`); the `mathstyle=` constructor argument and the MathML `mathvariant` attribute both resolve through it. These are the styling hooks a custom symbol owner reaches when composing math glyph runs directly.

| [INDEX] | MEMBER                                                          | KIND  | ROLE                                                       |
| :-----: | -------------------------------------------------------------- | ----- | ---------------------------------------------------------- |
|  [01]   | `styledchr(char, variant: MathVariant) -> str`                 | style | ASCII -> unicode math-variant codepoint                    |
|  [02]   | `styledstr(st, variant: MathVariant) -> str`                   | style | `styledchr` across a string                                |
|  [03]   | `MathVariant(style='serif', italic=False, bold=False, normal=False)` | style | the math-variant descriptor `mathstyle`/`mathvariant` resolve to |
|  [04]   | `auto_italic(char) -> bool`                                    | style | whether a single-char identifier auto-italicizes (MathML rule) |

## [05]-[FONT_AND_MATH_TABLE]

[FONT_SCOPE]: `MathFont` and the OpenType MATH table
- rail: figure

`MathFont(fname, basesize=24)` extends the `ziafont` `Font` with the OpenType MATH-table machinery; pass `font=<path>` to any renderer to typeset against a custom MATH font (the bundled STIX Two Math is the default). `MathTable` is the parsed MATH table — it exposes the italic-correction superscript/subscript kerning (`kernsuper`/`kernsub`), the glyph-variant ladders for stretchy delimiters and big operators (`listvariants`/`variant`/`variant_minmax`), the stretchy-delimiter assembly from glyph parts (`MathAssembly.assemble`), and the `MathConstants` record (axis height, script scale-down, limit gaps, fraction/stack rules) that governs every layout decision. A consumer rarely touches `MathTable` directly — `Math` walks it internally — but the math constants and variant selection are the deep substrate that makes the output true math typesetting rather than naive glyph placement, and the API is here for a font-engineering or QA owner that needs to inspect or validate a MATH font's coverage.

| [INDEX] | MEMBER                                                          | KIND  | ROLE                                                       |
| :-----: | -------------------------------------------------------------- | ----- | ---------------------------------------------------------- |
|  [01]   | `MathFont(fname, basesize=24)`                                 | font  | `ziafont.Font` + MATH table; the renderer font owner       |
|  [02]   | `MathFont.findglyph(char, variant: MathVariant) -> SimpleGlyph`| font  | resolve a styled char to a glyph (self or alt bold/italic font) |
|  [03]   | `MathFont.language(script, language)`                          | font  | enable `ssty` math script variants for a script/lang       |
|  [04]   | `MathTable` (`font.math`)                                      | table | parsed OpenType MATH table                                 |
|  [05]   | `MathTable.consts` (`MathConstants`)                           | table | axis height, script scale, limit/fraction gaps — layout law |
|  [06]   | `MathTable.kernsuper / kernsub(g1, g2) -> tuple[int, int]`     | table | italic-correction script kerning                           |
|  [07]   | `MathTable.variant / variant_minmax(glyphid, height/ymin, ymax, vert=True)` | table | stretchy glyph-variant selection by target size            |
|  [08]   | `MathTable.listvariants(glyphid, vert=True) -> dict[int, int]` | table | the available variant ladder for a glyph                   |

[TEX_HOOK_SCOPE]: LaTeX operator extension and conversion
- rail: figure

`declareoperator(name)` registers a custom LaTeX operator name (the equivalent of `\DeclareMathOperator`) so a domain expression can use `\myfunc` and have it typeset upright as an operator — the package pre-declares the Russian/extended trig set (`\tg`/`\ctg`/`\sh`/`\ch`/`\th`/...). `tex2mml(tex, inline)` is the LaTeX->MathML conversion the LaTeX path uses (with ziamath's preprocessing workarounds for `\binom`/`\mathrm`/`||`/`aligned`); a consumer that wants the MathML intermediate (to cache, diff, or hand to a MathML consumer) calls it directly.

| [INDEX] | MEMBER                                          | KIND  | ROLE                                                       |
| :-----: | ----------------------------------------------- | ----- | ---------------------------------------------------------- |
|  [01]   | `declareoperator(name: str)`                    | tex   | register a custom upright LaTeX operator (`\DeclareMathOperator`) |
|  [02]   | `tex2mml(tex, inline=False) -> str`             | tex   | LaTeX -> MathML string (with ziamath preprocessing)        |

## [06]-[NODE_TREE]

[NODE_SCOPE]: the laid-out math node tree (advanced/internal)
- rail: figure

`Math` builds a `Drawable` node tree (`nodes.Mnode.fromelement`) mirroring the MathML element structure — `Mrow`/`Mfrac`/`Msqrt`/`Mroot`/`Msub`/`Msup`/`Msubsup`/`Mmultiscripts`/`Munder`/`Mover`/`Munderover`/`Mfenced`/`Menclose`/`Mtable`/`Moperator`/`Midentifier`/`Mnumber`/`Mtext`/`Mspace`/`Mpadded`/`Mphantom` — each a `Drawable` with a `bbox` and a `draw(x, y, svg)` method. The `drawable` primitive shapes (`Glyph`/`HLine`/`VLine`/`Box`/`Diagonal`/`Ellipse`) are the leaf marks the node tree composes (fraction bars, radical lines, `menclose` boxes/strikes). This tree is internal layout machinery — a consumer reads `Math.node.bbox` for geometry and otherwise never constructs nodes — but it is the bounded grammar that makes the layout a true MathML renderer, documented here so a font/layout QA owner can introspect it.

| [INDEX] | MEMBER                                                          | KIND  | ROLE                                                       |
| :-----: | -------------------------------------------------------------- | ----- | ---------------------------------------------------------- |
|  [01]   | `Math.node` (`nodes.Mnode`)                                    | tree  | the root laid-out math node (carries `.bbox`)              |
|  [02]   | `nodes.Mnode.fromelement(element, parent) -> Mnode`            | tree  | build the node tree from a MathML `ET.Element`             |
|  [03]   | `nodes.{Mrow,Mfrac,Msqrt,Mroot,Msub,Msup,Msubsup,Mmultiscripts,Munder,Mover,Munderover,Mfenced,Menclose,Mtable,Moperator,Midentifier,Mnumber,Mtext,Mspace,Mpadded,Mphantom}` | tree | the closed MathML-element node family                      |
|  [04]   | `drawable.{Glyph,HLine,VLine,Box,Diagonal,Ellipse}`           | leaf  | primitive marks the node tree composes (bars, radicals, enclosures) |

## [07]-[IMPLEMENTATION_LAW]

- import: `import ziamath as zm` at boundary scope only; the distribution and import name are both `ziamath`; the version is `importlib.metadata.version("ziamath")` (the module also exposes `__version__ = '0.13'`). The `Latex`/`Text` paths require `latex2mathml` (a declared dependency, always present); the pure-`Math` MathML path does not.
- floor: pure-Python `py3-none-any` wheel, `Requires-Python >=3.8`; it installs on cp315 with NO `python_version` gate — the resolver (`uv lock`) carries no marker for it, and none must be added.
- render axis: `Math`/`Latex`/`Text` is the one render trio — `Math` for MathML, `Latex` (IS-A `Math`) for a single LaTeX expression, `Text` for mixed multi-line text+`$math$`; a consumer constructs one with its source and reads the shared egress, never a per-output-format render class. `Latex` subclasses `Math`, so a LaTeX equation shares the entire `Math` egress surface.
- egress axis: `svg()` (string) is the durable artifact; `svgxml()` (`ET.Element`) composes in-process without a reparse; `drawon(svg, x, y, halign, valign)` draws onto a caller `ET.Element` at an aligned anchor and returns the inserted `<g>`; `save(fname)` writes the file. `drawon` is the in-place composition seam — a diagram/document owner that owns its own `xml.etree` SVG tree threads the equation in directly, never via string concatenation or a temp file.
- config axis: `config` is the one global `Config` singleton — house math/text style, numbering policy, debug overlays, and `config.svg2`/`config.precision` egress settings are set ONCE on it and inherited by every render whose constructor argument is `None`; `config.svg2` toggles the SVG 2.0/1.1 profile (delegating to `ziafont.config.svg2`) so the embedded equation and the host `drawsvg` diagram emit at one consistent SVG version. Global mutation is serialized under the `to_thread` lane, never raced across the event loop.
- font axis: the bundled STIX Two Math is the default; `font=<path>` typesets against any MATH-table font through `MathFont` (a `ziafont.Font` subclass). `MathTable`/`MathConstants`/the variant ladders are the deep OpenType-MATH substrate that makes the output true typesetting (italic correction, stretchy delimiters, big-operator variants) — the renderer walks it internally; the API is exposed for font/QA introspection.
- offload axis: the render kernel (the `xml.etree` MathML parse, the MATH-table font walk, the global `config` read/mutate, the SVG serialization) is GIL-bound, lxml-free, shared-address-space work — it runs through the runtime worker `to_thread.run_sync(..., limiter=...)` seam, the SAME thread arm the `drawsvg` diagram emitter and the GIL-releasing-native `rustworkx` layout take, NEVER a `to_interpreter` subinterpreter arm (the global mutable `config` and the C-extension `ziafont`/`numpy` palette neighbors are subinterpreter-hostile).
- evidence: each math-render op captures the laid-out `getsize()` `(width, height)`, the `getyofst()` baseline shift, the source kind (`mathml`/`latex`), the eq-number label if any, the `config.svg2` profile, and the output SVG byte length as a figure `ArtifactReceipt`, keyed by `ContentIdentity` over the serialized SVG bytes — never a second render to measure.
- boundary: ziamath owns math/LaTeX -> SVG typesetting, the OpenType-MATH layout, equation numbering, and unicode math-variant styling; glyph outline extraction and the base `Font`/`Text` substrate are `ziafont`'s (the local `ziafont` catalog); LaTeX->MathML front-end parsing is `latex2mathml`'s; SVG-tree layout/composition of the emitted fragment alongside other marks is `drawsvg`'s (`Raw`/`Image`) or `svgelements`' (parse + transform + bbox); rasterization of the emitted SVG string routes to `resvg-py`/`vl-convert`/`pyvips`; plain (non-math) shaped text stays at `typography/shape` (`uharfbuzz`); Jupyter display stays at the notebook boundary.

[STACKING]:
- A diagram annotation that carries a formula renders `zm.Latex(formula).svg()` to an SVG string the `visualization/diagram/draw#DRAW` `_lower` `Annotation` arm wraps as a `drawsvg.Raw(math_svg)` fragment (or `drawsvg.Image(..., data=svg_as_utf8_data_uri(math_svg), embed=True)`), bucketed into the same `GlyphStyle.layer` named `Group` as every other mark — so a math callout composes into the named-layer SVG egress the `export/layered#LAYERED` owner binds, with zero new diagram surface (the equation is one more `Annotation` glyph, not a parallel renderer). The `Annotation` `GlyphStyle.fill` palette index threads to the equation via the `color=` constructor argument, keeping every mark's color on the one `graphic/color/derive#DERIVE` palette.
- When the diagram owner and ziamath share one `xml.etree` SVG tree, `zm.Math(...).drawon(svg, x, y, halign, valign)` draws the equation in place at the laid-out anchor and returns the inserted `<g>` — no string round-trip — and `getsize()`/`getyofst()` feed the layout owner the bbox extent and baseline so the equation seats correctly against a leader, dimension line, or grid bubble (the `valign='axis'`/`'base'` vocabulary the AEC annotation plane needs).
- A `document/model` `DocumentNode` equation block renders `zm.Latex(latex, number=...).svg()` (or with `config.numbering.autonumber` driving section-relative labels), threading the SVG into the rich-text flow as an inline figure; `reset_numbering` is called at each numbered-section boundary, and `config.svg2` is set to match the document's target PDF/print SVG profile so `weasyprint`/`pikepdf` embed a consistent vector.
- The render kernel runs through the runtime `to_thread.run_sync` seam (the GIL-bound XML/font/`config` work), the result `svg` string is wrapped in an expression `Result`/`Try` rail at the boundary (a malformed LaTeX or absent MATH-table font is a typed failure, not a raised exception crossing the async edge), and the emitted bytes are content-keyed via `ContentIdentity.of` into one `ArtifactReceipt` case — the same offload + rail + receipt spine every figure owner shares, with structlog/OTel spans over the render boundary.
- A custom engineering-operator vocabulary registers once via `zm.declareoperator(r'\Rd')` at module init (alongside the pre-declared `\tg`/`\sh`/...), and a domain symbol owner that composes math glyph runs directly reaches `styledchr`/`MathVariant` for `\mathbb`/`\mathfrak` styling and `tex2mml` to cache the MathML intermediate — the deep hooks the surface-level `Latex(...).svg()` path never needs but the typography/symbol plane does.

## [08]-[LOCAL_ADMISSION]

- Package: `ziamath`
- Owns: math/LaTeX -> SVG typesetting, the `Math`/`Latex`/`Text` render trio, the OpenType-MATH-table layout (italic correction, stretchy delimiters, big-operator variants, math constants via `MathFont`/`MathTable`), equation auto-numbering (`config.numbering`/`reset_numbering`), unicode math-variant styling (`styledchr`/`MathVariant`), custom LaTeX operator registration (`declareoperator`), and the SVG-string/`ET.Element`/in-place-`drawon` egress with SVG 2.0/1.1 profile control (`config.svg2`)
- Accept: rendering mathematical/engineering formulas as durable SVG composited into the `drawsvg` diagram canvas (as `Raw`/`Image` or in-place `drawon`), into the `document/model` rich-text equation flow with numbering, and into the `typography`/symbol annotation plane wherever a label exceeds plain shaped text — all under the `to_thread` offload + `Result` rail + `ContentIdentity` receipt spine
- Reject: glyph outline extraction and the base `Font`/`Text` substrate where `ziafont` owns it; LaTeX->MathML front-end parsing where `latex2mathml` owns it; SVG-fragment layout/transform/bbox alongside other marks where `drawsvg`/`svgelements` own it; rasterization of the emitted SVG where `resvg-py`/`vl-convert`/`pyvips` own it; plain non-math shaped text where `typography/shape`/`uharfbuzz` owns it; a per-output-format render-class family where the one trio + shared egress discriminates; a raised exception crossing the async edge where the boundary `Result` rail owns failure; a subinterpreter offload arm where the global mutable `config` and C-extension neighbors force the `to_thread` arm; identity minting the runtime owns
