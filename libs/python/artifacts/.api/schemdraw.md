# [PY_ARTIFACTS_API_SCHEMDRAW]

`schemdraw` mints symbol-anchored technical-schematic authoring: a `Drawing` context-manager canvas over the 226-symbol closed `elements` vocabulary and the `flow`/`logic`/`dsp` domain modules, a fluent relative-connection placement algebra over named anchors, the `Segment*` primitive grammar, and a backend-selected `get_imagedata` egress (pure-SVG or matplotlib). schemdraw owns the diagram class whose marks are NAMED symbols with bound anchor terminals — a resistor, an op-amp, a NAND gate — never the node-link routing `rustworkx`/`pyelk`/`fast-sugiyama` hold nor the rasterization `resvg-py`/`vl-convert`/`pyvips` own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `schemdraw`
- package: `schemdraw` (MIT)
- import: `schemdraw`
- owner: `artifacts`
- rail: diagram
- entry points: none (library only)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document canvas, render context, geometry

`Drawing` is the one document canvas and context manager: `with Drawing() as d:` opens it, `d += element` / `d.add(element)` is the single polymorphic insertion surface, `__exit__` finalizes layout. `Transform` carries the per-element placement frame the fluent algebra threads; `BBox`/`Point` are the geometry value objects anchor and bbox queries return. No per-shape `add_resistor` method exists — every symbol is a constructed `Element` handed to `+=`.

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]   | [CAPABILITY]                                                                      |
| :-----: | :------------ | :-------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `Drawing`     | document canvas | `Drawing(canvas=None, file=None, show=True, transparent=False, dpi=72, **kwargs)` |
|  [02]   | `Transform`   | placement frame | `Transform(theta, globalshift, localshift=(0,0), zoom=Point(1,1))`                |
|  [03]   | `ImageFormat` | enum            | `str`-Enum `SVG`/`PNG`/`PDF`/`EPS`/`PS`/`PGF`/`JPG`/`TIF`/`RAW`/`RGBA`            |

[PUBLIC_TYPE_SCOPE]: element base hierarchy

Every drawable symbol derives `Element`; the hierarchy splits into `Element2Term` (a two-terminal in-line symbol adding the `.length`/`.to`/`.tox`/`.toy`/`.endpoints`/`.dot`/`.idot` surface), `ElementImage` (an embedded raster), `ElementDrawing` (a `Drawing` reused as a symbol), `ElementCompound` (the base a custom multi-`Segment` symbol composes from), and `Container`/`Encircle` (a box around a set of elements). Placement, styling, and labelling read off the base, never a per-symbol special case.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                                                         |
| :-----: | :---------------- | :------------ | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `Element`         | `object` base | abstract symbol root; fluent placement/style/label algebra, `.anchors`, `.segments`, `.get_bbox()`   |
|  [02]   | `Element2Term`    | `Element`     | two-terminal in-line symbol; adds `.length`/`.to`/`.tox`/`.toy`/`.endpoints`/`.shift`/`.dot`/`.idot` |
|  [03]   | `ElementImage`    | `Element`     | `ElementImage(image, width, height, xy=Point(0,0), imgfmt=None, **kwargs)` embedded raster           |
|  [04]   | `ElementDrawing`  | `Element`     | `ElementDrawing(drawing, **kwargs)` one `Drawing` reused as a single symbol                          |
|  [05]   | `ElementCompound` | `Element`     | base for a custom symbol assembled from `Segment*` primitives in `init`                              |
|  [06]   | `Container`       | `Element`     | `Container(drawing, *, cornerradius=None, padx=None, pady=None)` box around a sub-drawing            |

[PUBLIC_TYPE_SCOPE]: low-level segment primitive vocabulary

`Segment*` is the bounded primitive grammar every `Element` is built from and a custom symbol composes through `ElementCompound`: a placed element's `.segments` is a `list[SegmentType]`, and `Drawing.get_segments()` flattens the whole schematic's stream for geometry inspection or a custom render. Every `Segment*` carries the shared style tail `(color, lw, ls, fill, capstyle, joinstyle, clip, zorder, visible)`; `SegmentText` adds `(align, rotation, rotation_mode, rotation_global, bgcolor, fontsize, font, mathfont, href, decoration)`. Each row shows only its distinguishing geometry head.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]        | [CAPABILITY]                                                                               |
| :-----: | :-------------- | :------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `Segment`       | path primitive       | `Segment(path, arrow=None, arrowwidth=0.15, arrowlength=0.25)` a polyline path             |
|  [02]   | `SegmentCircle` | circle primitive     | `SegmentCircle(center, radius, ref=None)`                                                  |
|  [03]   | `SegmentArc`    | arc primitive        | `SegmentArc(center, width, height, theta1=35, theta2=-35, arrow=None, angle=0)` elliptical |
|  [04]   | `SegmentText`   | text primitive       | `SegmentText(pos, label, …)` (text params hoisted to the lead)                             |
|  [05]   | `SegmentPath`   | mixed-path primitive | `SegmentPath(path)` a path mixing `XY` points and SVG command strings                      |
|  [06]   | `SegmentPoly`   | polygon primitive    | `SegmentPoly(verts, closed=True, cornerradius=0, hatch=False)`                             |
|  [07]   | `SegmentBezier` | bezier primitive     | `SegmentBezier(p, arrow=None, arrowlength=0.25, arrowwidth=0.15)`                          |

[PUBLIC_TYPE_SCOPE]: `elements` symbol vocabulary (`schemdraw.elements`)

`schemdraw.elements` is the closed electrical/electronic symbol vocabulary — 226 named `Element`/`Element2Term` subclasses placed by construction and discriminated by type (a standards variant a sibling class, `ResistorIEEE`/`ResistorIEC`). Its owning page imports `from schemdraw import elements as elm`, selecting the symbol by name; `elements.__all__` is the exhaustive verifiable closed set, mapped by family below.

| [INDEX] | [FAMILY]               | [REPRESENTATIVE_SYMBOLS]                                                                                       |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | passive two-term       | `Resistor`/`ResistorIEEE`/`ResistorIEC`/`ResistorVar`/`Capacitor`/`Inductor`/`Crystal`/`Fuse`/`Memristor`      |
|  [02]   | diodes/semiconductor   | `Diode`/`Schottky`/`Zener`/`DiodeTVS`/`Varactor`/`LED`/`Photodiode`/`Diac`/`Triac`/`SCR`/`Josephson`           |
|  [03]   | sources/grounds        | `Source`/`SourceV`/`SourceI`/`SourceSin`/`SourcePulse`/`Battery`/`Ground`/`GroundSignal`/`Vdd`/`Vss`/`Antenna` |
|  [04]   | meters/indicators      | `MeterV`/`MeterI`/`MeterA`/`MeterOhm`/`Lamp`/`Solar`/`Neon`/`Oscilloscope`                                     |
|  [05]   | switches/buttons       | `Switch`/`SwitchSpdt`/`SwitchDpst`/`SwitchDpdt`/`Button`/`SwitchReed`/`SwitchRotary`/`SwitchDIP`/`Breaker`     |
|  [06]   | transistors            | `NFet`/`PFet`/`JFet`/`JFetN`/`JFetP`/`Bjt`/`BjtNpn`/`BjtPnp`/`IgbtN`/`IgbtP`/`NMos`/`PMos`/`Hemt`              |
|  [07]   | transducers/machines   | `Speaker`/`Mic`/`Motor`/`AudioJack`/`Transformer`/`Coax`/`Triax`                                               |
|  [08]   | integrated circuits    | `Ic`/`IcPin`/`IcDIP`/`Multiplexer`/`VoltageRegulator`/`DFlipFlop`/`JKFlipFlop`/`Ic555`/`SevenSegment`/`Opamp`  |
|  [09]   | lines/connectors/wires | `Line`/`Wire`/`DataBusLine`/`Dot`/`Arrow`/`Arrowhead`/`DotDotDot`/`Gap`/`OrthoLines`/`RightLines`/`BusLine`    |
|  [10]   | annotation/markup      | `Label`/`Tag`/`Annotate`/`ZLabel`/`CurrentLabel`/`VoltageLabelArc`/`LoopCurrent`/`LoopArrow`/`Rect`/`Encircle` |
|  [11]   | arcs                   | `Arc2`/`Arc3`/`ArcZ`/`ArcN`/`ArcLoop`                                                                          |
|  [12]   | connectors/headers     | `Header`/`Jumper`/`DB25`/`DB9`/`DE9`/`DA15`/`Plug`/`Jack`/`Terminal`/`CoaxConnect`                             |
|  [13]   | compound/two-port      | `Optocoupler`/`Relay`/`Rectifier`/`TwoPort`/`CurrentTransactor`/`Nullor`/`VMCMPair`                            |
|  [14]   | mains outlets/tubes    | `OutletA`..`OutletL` (IEC/NEMA) · `VacuumTube`/`Triode`/`Tetrode`/`Pentode`/`NixieTube`/`TubeDiode`            |

[PUBLIC_TYPE_SCOPE]: domain element modules (`flow`, `logic`, `dsp`)

`flow`, `logic`, and `dsp` carry the non-electrical vocabularies on the same `Drawing`/`Element`/placement spine: `flow` the flowchart owner (terminal/process/decision/data boxes), `logic` the digital-gate vocabulary with the `Kmap`/`Table`/`TimingDiagram`/`BitField` structured owners, `dsp` the signal-flow/DSP blocks. Each module's elements place and connect through the identical fluent algebra.

| [INDEX] | [MODULE] | [OWNS]                          | [KEY_SYMBOLS]                                                                              |
| :-----: | :------- | :------------------------------ | :----------------------------------------------------------------------------------------- |
|  [01]   | `flow`   | flowchart boxes + connectors    | `Box`/`RoundBox`/`Process`/`Decision`/`Data`/`Subroutine`/`Terminal`/`Start`               |
|  [02]   | `logic`  | logic gates + structured owners | `And`/`Nand`/`Or`/`Nor`/`Xor`/`Xnor`/`Schmitt` · `Kmap`/`Table`/`TimingDiagram`/`BitField` |
|  [03]   | `dsp`    | signal-flow/DSP blocks          | `Square`/`Sum`/`SumSigma`/`Mixer`/`Amp`/`Oscillator`/`Filter`/`Adc`/`Dac`                  |

[PUBLIC_TYPE_SCOPE]: structured logic owners (`schemdraw.logic`)

`Kmap`/`Table`/`TimingDiagram`/`BitField` each fold a `dict`/string into a complete figure in one constructor, not a per-cell placement loop — the data-driven entrypoints a downstream truth-table, register-map, or waveform figure lowers onto.

| [INDEX] | [SYMBOL]        | [CAPABILITY]                                                                                |
| :-----: | :-------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `Kmap`          | `Kmap(names='ABCD', truthtable=None, groups=None, default='0', **kwargs)` grouping ellipses |
|  [02]   | `Table`         | `Table(table, colfmt=None, fontsize=12, font='sans', **kwargs)` markdown-pipe table figure  |
|  [03]   | `TimingDiagram` | `TimingDiagram(waved, **kwargs)` WaveJSON digital-timing diagram from a `dict`              |
|  [04]   | `BitField`      | `BitField(reg, **kwargs)` register bit-field map from a `dict`                              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Drawing` — open, insert, render, egress

`Drawing(canvas=None, file=None, show=True, transparent=False, dpi=72, **kwargs)` is the canvas, entered as a context manager so layout finalizes on `__exit__`. `d += element` (`__iadd__`) and `d.add(element)` place a constructed `Element` and RETURN it; `add_elements(*elements)` batches. `get_imagedata(fmt)` over `ImageFormat` is the durable egress (`"svg"` under the standalone backend), beside `save`, `get_segments`, and the `get_bbox`/`here`/`theta` geometry and cursor queries.

| [INDEX] | [SURFACE]                                                     | [SHAPE]   | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------------ | :-------- | :-------------------------------------------------------- |
|  [01]   | `Drawing(canvas=None, …, dpi=72, **kwargs)`                   | construct | the schematic canvas; context-manager or explicit         |
|  [02]   | `add(element)` / `iadd(element)` (`d += e`)                   | build     | place one constructed `Element`, return it (anchors live) |
|  [03]   | `add_elements(*elements)`                                     | build     | batch-place several elements                              |
|  [04]   | `config(unit, …, mathfont)`                                   | style     | per-drawing appearance override                           |
|  [05]   | `add_svgdef(svgdef)`                                          | build     | inject a raw `<defs>` SVG fragment                        |
|  [06]   | `get_imagedata(fmt='svg')`                                    | egress    | rendered bytes over `ImageFormat` (`'svg'` = durable)     |
|  [07]   | `save(fname, transparent=True, dpi=72)`                       | egress    | write the rendered image to disk (format from extension)  |
|  [08]   | `get_segments()` / `get_bbox()` / `elements`                  | query     | `Segment*` stream / bounding box / placed `list[Element]` |
|  [09]   | `move(dx, dy)` / `move_from(ref, …)` / `here` / `theta`       | cursor    | move / re-anchor / read the placement cursor              |
|  [10]   | `push()` / `pop()` / `undo()`                                 | cursor    | save / restore / remove the last cursor state             |
|  [11]   | `draw(show=True, canvas=None)`                                | render    | force a (re)render onto a backend (explicit)              |
|  [12]   | `container(cornerradius=0.3, …)` / `hold()` / `interactive()` | build     | open a `Container` box / hold cursor / interactive redraw |

[ENTRYPOINT_SCOPE]: fluent placement algebra (`Element`)

Every `Element` carries the chainable placement/style/label algebra — each method returns `self`, so a symbol is constructed, positioned relative to the prior element's named anchor, styled, and labelled in one expression: `elm.Resistor().right().at(prev.end).label('R1').color('blue')`. This relative-connection algebra IS the schematic-authoring model — coordinates are derived, rarely typed; the table carries the per-method roster.

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                                               |
| :-----: | :-------------------------------------------------------------- | :------- | :--------------------------------------------------------- |
|  [01]   | `at(xy \| (element, anchor), dx=0, dy=0)`                       | place    | pin the start to a point or another element's named anchor |
|  [02]   | `right()` / `left()` / `up()` / `down()` / `theta(deg)`         | place    | set the placement direction                                |
|  [03]   | `anchor(name)` / `anchors` (attr) / `set_anchor(name)`          | place    | choose the anchor that lands at the point                  |
|  [04]   | `label(label, loc=None, …, decoration=None)`                    | annotate | attach positioned math/hyperlinked text (sig in the lead)  |
|  [05]   | `color(c)` / `fill(c=True)` / `gradient_fill(c1, c2, …)`        | style    | stroke color / fill / two-stop gradient fill               |
|  [06]   | `linewidth(lw)` / `linestyle(ls)` / `style(color=, fill=, …)`   | style    | line width / dash style / bundled style                    |
|  [07]   | `scale/scalex/scaley(s)` / `flip()` / `reverse()` / `zorder(z)` | style    | scale / mirror / reverse direction / z-order               |
|  [08]   | `drop(drop)` / `hold()` / `get_bbox(...)`                       | place    | set the exit anchor / hold cursor / bbox                   |

[ENTRYPOINT_SCOPE]: `Element2Term` extra placement (in-line two-terminal symbols)

A two-terminal symbol (resistor, capacitor, wire) adds the in-line placement surface: `.to(xy)` stretches so the END lands at a point, `.tox`/`.toy` stretch to an x/y keeping the other axis, `.length(L)` fixes the span, `.endpoints(start, end)` pins both ends, and `.dot`/`.idot` add a connection dot at the end/start — how a wire run reaches a target node by coordinate-free stretching.

| [INDEX] | [SURFACE]                                                          | [CAPABILITY]                                       |
| :-----: | :----------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `Element2Term.to(xy, dx=0, dy=0)`                                  | stretch so the end lands at a point                |
|  [02]   | `Element2Term.tox(x \| XY \| Element)` / `toy(y \| XY \| Element)` | stretch to an x / y, keeping the other axis        |
|  [03]   | `Element2Term.length(L)` / `endpoints(start, end)` / `shift(s)`    | fix the span / pin both ends / shift along the run |
|  [04]   | `Element2Term.dot(open=False)` / `idot(open=False)`                | add a (open) connection dot at the end / start     |

[ENTRYPOINT_SCOPE]: integrated-circuit & compound builders (`elements`)

`Ic(size=None, pins=None, slant=0)` builds a named-pin IC box from a `Sequence[IcPin]`, each pin a named, sided (`L`/`R`/`T`/`B`), positioned terminal that becomes an addressable anchor on the placed `Ic` (a wire connects to `ic.IN`/`ic.OUT` by name). `Multiplexer` is the slanted-IC specialization, `Encircle`/`Container` box a set of elements, and `ElementCompound` is the base a custom symbol composes from.

| [INDEX] | [SURFACE]                                                             | [CAPABILITY]                                        |
| :-----: | :-------------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `Ic(size=None, pins=None, slant=0, **kwargs)`                         | named-pin IC box; each `IcPin.name` is an anchor    |
|  [02]   | `IcPin(name, side='L', pos=None, invert=False, invertradius=0.15, …)` | one named/sided/positioned IC pin (`invert` bubble) |
|  [03]   | `Multiplexer(demux=False, size=None, pins=None, slant=25, **kwargs)`  | a slanted-IC mux/demux from `IcPin`s                |
|  [04]   | `Encircle(…)` / `EncircleBox(…)`                                      | rounded/rectangular box around placed elements      |
|  [05]   | `Opamp(*, sign=None, leads=None, **kwargs)`                           | op-amp with `in1`/`in2`/`out`/`vd`/`vs` anchors     |
|  [06]   | `ElementImage(…)` / `ElementDrawing(…)`                               | embed a raster / embed a `Drawing` as one symbol    |

[ENTRYPOINT_SCOPE]: structured logic & boolean-expression parse (`logic`, `parsing`)

`logic.Kmap`/`Table`/`TimingDiagram`/`BitField` fold a `dict`/string into a complete structured figure in one constructor. `parsing.logicparse(expr, ...)` parses an expression string (`'a and (b or c)'`), runs the bundled Buchheim tree placement, and RETURNS a fully-placed gate `Drawing` — the logic-layout fallback, not the generic routing engine.

| [INDEX] | [SURFACE]                                                      | [CAPABILITY]                                               |
| :-----: | :------------------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `logic.Kmap(names='ABCD', truthtable=None, groups=None, …)`    | a Karnaugh map with grouping ellipses                      |
|  [02]   | `logic.Table(table, colfmt=None, fontsize=12, font='sans')`    | a markdown-pipe truth/data table as a schematic figure     |
|  [03]   | `logic.TimingDiagram(waved)` / `logic.BitField(reg)`           | a WaveJSON timing diagram / register bit-field from `dict` |
|  [04]   | `parsing.logicparse(expr, gateW=2, gateH=0.75, outlabel=None)` | parse a boolean expression to a placed gate `Drawing`      |

[ENTRYPOINT_SCOPE]: global appearance & backend selection (`config`, `theme`, `use`, `svgconfig`, `style`)

`use(backend)` selects the render engine ONCE per process — `'svg'` the standalone pure-SVG backend (no matplotlib, no native lib), `'matplotlib'` the raster/EPS/PGF path. `config`/`theme(name)` set the global default appearance; `svgconfig` carries the SVG-backend render policy (`text='path'` emits font-independent `<path>` text with ziamath math rendering, `text='text'` emits native `<text>` using system fonts, `svg2=True` uses SVG 2.0 features, `precision` the coordinate decimal precision). `style.color_rgb`/`color_hex`/`validate_color` parse/validate color input.

- `theme` names: `default` `dark` `solarizedd` `solarizedl` `onedork` `oceans16` `monokai` `gruvboxl` `gruvboxd` `grade3` `chesterish`.
- `config` carry: `unit`, `inches_per_unit`, `lblofst`, `fontsize`, `font`, `color`, `lw`, `ls`, `fill`, `bgcolor`, `margin`, `mathfont` — the global default; `Drawing.config` takes the same fields with `None` meaning inherit.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `use(backend='matplotlib')`                                | backend  | select engine: `'svg'` (standalone) or `'matplotlib'` |
|  [02]   | `config(**appearance)`                                     | style    | global default appearance (fields in the lead)        |
|  [03]   | `theme(theme='default')`                                   | style    | global theme (names in the lead)                      |
|  [04]   | `svgconfig.text` / `svg2` / `precision` / `useBatik`       | style    | SVG render policy (path/native text, SVG2, precision) |
|  [05]   | `color_rgb` / `color_hex` / `color_hsl` / `validate_color` | validate | color-input parse/validate helpers (`style.*`)        |
|  [06]   | `debug(dwgbbox=True, elmbbox=True)`                        | debug    | overlay drawing/element bboxes (layout debugging)     |

[ENTRYPOINT_SCOPE]: custom symbol composition (`Segment*` + `ElementCompound`)

A symbol the `elements` vocabulary does not carry is composed by subclassing `ElementCompound` and appending typed `Segment*` primitives to `self.segments` in `__init__`, declaring named anchors in `self.anchors` — so a custom AEC fixture symbol (a sprinkler head, a damper, a building-system glyph) is one `Segment*`-built class, never a hand-emitted SVG path, and participates in the identical fluent placement algebra.

| [INDEX] | [SURFACE]                                                          | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `class MySymbol(ElementCompound)`; append `Segment*` in `__init__` | a custom symbol from typed primitives                |
|  [02]   | `self.anchors['name'] = (x, y)` in the symbol `init`               | declare a named terminal anchor on the custom symbol |
|  [03]   | `self.segments.append(Segment/SegmentPoly/SegmentArc/…)`           | append a typed path/poly/arc/text primitive          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `import schemdraw` and `from schemdraw import elements as elm` (add `flow`, `logic`, `dsp` where the domain vocabulary is needed) at boundary scope only; distribution and import name are both `schemdraw`. Domain code holds the schematic content — the component graph, flowchart structure, or logic network — and lowers it here through the fluent algebra, never letting the `Drawing`/`Element` object model leak inward.
- backend axis: `use('svg')` selects the standalone pure-SVG backend once — in-process, no matplotlib, no native library — so the design fixes it and treats `get_imagedata("svg")` as the durable artifact; `use('matplotlib')` is the raster/EPS/PGF path, engaged only where the SVG-then-`resvg-py`/`vl-convert` raster route cannot reach. Backend selection is a process-global, set once at the rail boundary, never per-element.
- canvas axis: `Drawing` is the one spine, entered as a context manager so layout finalizes on `__exit__`; `d += element` / `d.add(element)` places a constructed `Element` and returns it. A new symbol is one `elements` class or one `ElementCompound`, never a new insertion method.
- placement axis: the fluent algebra (`.at(prev.anchor)`/`.right`/`.up`/`.to`/`.tox`/`.toy`/`.length`/`.anchor(name)`) chains each method returning `self`; coordinates derive from the prior element's named anchor, a wire reaches a target by `.to`/`.tox`/`.toy` stretch, an IC pin is addressed by name (`ic.IN`) — the diagram structure is the source of truth, not a coordinate table.
- symbol axis: `elements` is the 226-symbol closed vocabulary (a standards variant a sibling class — `ResistorIEEE`/`ResistorIEC`), `flow`/`logic`/`dsp` the domain modules on the same spine, `Segment`/`SegmentCircle`/`SegmentArc`/`SegmentText`/`SegmentPath`/`SegmentPoly`/`SegmentBezier` the primitive grammar; a custom symbol subclasses `ElementCompound` and appends typed `Segment*` to `self.segments` with named anchors in `self.anchors`.
- structured-figure axis: `logic.Kmap`/`Table`/`TimingDiagram`/`BitField` fold a `dict`/string into a complete figure in one constructor, and `parsing.logicparse(expr)` parses a boolean expression to a fully-placed gate `Drawing` through the bundled Buchheim placement — the logic-layout fallback; generic graph routing stays with `rustworkx`/`pyelk`/`fast-sugiyama`.
- appearance axis: `config`/`theme` set the global default and `Drawing.config` the per-drawing override; `svgconfig.text='path'` is the design default so emitted SVG is font-independent (`'text'` only where live-editable native-SVG text is required), and color binds through `style.color_rgb`/`color_hex`/`validate_color` from the `graphic/color/derive#DERIVE` palette so a schematic's color traces to one palette index.
- egress axis: `get_imagedata(fmt)` over the `ImageFormat` vocabulary is the one render family — `get_imagedata("svg")` the recorded bytes, `save(fname)` the disk write, `get_segments()`/`get_bbox()` the geometry query; the SVG bytes are the artifact of record under the runtime content-key, never `save`'s disk path.
- evidence: each schematic egress captures the placed-element count, the connector tally, and the emitted SVG byte length on the shared `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` row — the same case the `drawsvg`/`drawpyo` arms mint — while `get_segments()`/`get_bbox()` geometry rides the observability span.
- boundary: schemdraw owns symbol-anchored schematic authoring and the `Segment*`/`ElementCompound` custom-symbol grammar; the generic data-driven `DiagramGlyph` diagram is the `drawsvg` arm in `visualization/diagram/draw#DRAW`, the editable `.drawio` egress the `drawpyo` arm, graph routing `rustworkx`/`pyelk`/`fast-sugiyama`/`libavoid`, rasterization `resvg-py`/`vl-convert`/`pyvips`, content identity `rasm.runtime.identity#ContentIdentity`.

[STACKING]:
- `expression` rail: the `get_imagedata`/`save` egress is wrapped by the owning page's `RuntimeRail`/`async_boundary` (`rasm.runtime.faults`) so a render fault (an unknown backend, an off-domain `validate_color`/`validate_linestyle` raise, a bad-format `ElementImage`) lands as a typed `Result` failure, never a raw exception crossing the domain.
- `msgspec`/`pydantic` rail: the placed-element count, connector tally, and SVG byte length populate a `msgspec.Struct` `ArtifactReceipt.Diagram(key, "diagram-schematic", nodes, edges, "schemdraw", bytes)` row on the shared `core/receipt#RECEIPT` family — `nodes` the placed-element tally, `edges` the wire/connector tally, `algorithm` the `"schemdraw"` engine descriptor.
- admission rail: the owning page validates symbol-name selection and placement-anchor wiring as accumulated `SchematicFault.admission` evidence before any `Drawing` mutation — a stale symbol name, a duplicate reference, or a bad anchor refuses typed at the seam, never a deep `AttributeError`.
- `ContentIdentity` rail: the owning page mints the node key PRE-RUN through `rasm.runtime.identity#ContentIdentity.key("schematic", ...)` over the length-framed canonical spec⊕theme chunks — never over rendered bytes — so keyed elision probes the warm seed before the render runs and `receipt.slot == node.key`; schemdraw never mints identity.
- `anyio` rail: schemdraw is pure-Python and synchronous; the owning page offloads the `Drawing.__exit__`/`get_imagedata` render through `lane.offload(Kernel.of(..., KernelTrait.RELEASING))` — the shared-address-space thread arm the sibling `drawsvg` `_render` and GIL-releasing `rustworkx` layout take, since the `to_interpreter` arm cannot load schemdraw's `ziafont`/`ziamath` render path — so a large render never blocks the loop.
- `structlog`/`opentelemetry` rail: the render is bracketed by the runtime observability seam, emitting a span carrying the backend, the element/segment counts, and the byte length as attributes — the same diagram-rail telemetry the `drawsvg`/`drawpyo` arms emit.
- `draw#DRAW` sibling seam: schemdraw is owned by `visualization/diagram/schematic#SCHEMATIC`, disjoint from `draw#DRAW`'s `drawsvg` general-diagram arm and `drawpyo` `.drawio` arm by diagram CLASS — the seven-mark `DiagramGlyph` grammar cannot express a named symbol with bound anchor terminals, so schematic content lowers onto the `elements`/`flow`/`logic`/`dsp` vocabulary here while the data-driven AEC diagram lowers onto `DiagramGlyph` there.
- `export/layered#LAYERED` seam: a schematic emitting as named SVG layers buckets its elements with `zorder`/`add_svgdef` and lowers the rendered SVG to the `export/layered#LAYERED` `Layer(name, source, bbox)` row — the same named-layer contract the `drawsvg` arm uses.

[RAIL_LAW]:
- Package: `schemdraw`
- Owns: symbol-anchored technical-schematic authoring — the `Drawing` context-manager canvas over a standalone pure-SVG or matplotlib backend, the 226-symbol closed `elements` vocabulary with the `flow`/`logic`/`dsp` domain modules, the fluent relative-connection placement algebra over named anchors, the `Segment*` primitive grammar with `ElementCompound` custom-symbol composition, the `Ic`/`IcPin` named-pin builder, the `logic.Kmap`/`Table`/`TimingDiagram`/`BitField` structured owners and `parsing.logicparse` gate-network builder, the `config`/`theme`/`svgconfig`/`style` appearance owners, and the `get_imagedata`/`save`/`get_segments`/`get_bbox` egress over `ImageFormat`.
- Accept: diagrams whose marks are named symbols with bound anchor terminals — electrical/electronic schematics, signal-flow/DSP blocks, digital-logic gate networks, flowcharts, state diagrams, Karnaugh maps, register bit-fields, digital-timing diagrams — emitted as font-independent SVG (`svgconfig.text='path'`, `use('svg')`); custom AEC fixture symbols from `Segment*`/`ElementCompound`; feeding the `ArtifactReceipt.Diagram` case and the runtime content-key; lowering to `export/layered#LAYERED` rows when multi-discipline layering is required.
- Reject: a hand-emitted SVG tag or `<path d>` string where `elements` and `Segment*` exist; an `add_resistor`/`add_capacitor` family where `d +=`/`add` discriminates on the constructed symbol; an absolute-coordinate placement table where the fluent algebra derives coordinates; the generic `DiagramGlyph` node-link/ER/Sankey diagram the `drawsvg` arm renders; an editable `.drawio` egress the `drawpyo` arm owns; re-implemented graph routing where `rustworkx`/`pyelk`/`fast-sugiyama`/`libavoid` route; in-page matplotlib raster where `resvg-py`/`vl-convert`/`pyvips` cover it; a per-symbol color literal where the `graphic/color/derive#DERIVE` palette binds; identity minting the runtime owns.
