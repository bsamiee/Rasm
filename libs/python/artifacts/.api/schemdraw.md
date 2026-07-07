# [PY_ARTIFACTS_API_SCHEMDRAW]

`schemdraw` is the categorical-best owner of the symbol-rich technical-schematic egress concern for the artifacts diagramming rail — the diagram class the generic seven-mark `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` grammar (`Node`/`Edge`/`Swimlane`/`Annotation`/`Marker`) cannot express: electrical/electronic schematics, signal-flow/DSP block diagrams, digital-logic gate networks, flowcharts, state diagrams, Karnaugh maps, and timing diagrams whose marks are NAMED SYMBOLS (a resistor, an op-amp, a NAND gate, an ADC) with bound anchor terminals, not anonymous boxes. The spine is a `Drawing` context-manager canvas whose backend is selected once (`schemdraw.use('svg')` for the standalone in-process pure-SVG egress that needs no matplotlib, `'matplotlib'` for the richer raster/EPS/PGF render path), a 226-symbol closed `elements` vocabulary plus the `flow`/`logic`/`dsp` domain element modules, a fluent placement algebra (`.at`/`.right`/`.up`/`.to`/`.tox`/`.toy`/`.anchor`/`.label`/`.color`/`.theta`) that chains each symbol off the prior element's named anchor so the diagram is authored by RELATIVE connection rather than absolute coordinates, the low-level `Segment*` primitive vocabulary (`Segment`/`SegmentCircle`/`SegmentArc`/`SegmentText`/`SegmentPath`/`SegmentPoly`/`SegmentBezier`) every element is built from and a custom symbol composes through `ElementCompound`, the `config`/`theme`/`svgconfig`/`style` global appearance owners, and the `get_imagedata(fmt)`/`save`/`get_segments`/`get_bbox` egress family over the `ImageFormat` raster vocabulary. The owning design page (`visualization/diagram/schematic#SCHEMATIC`, beside draw's `drawsvg` general-diagram arm and `drawpyo` `.drawio` arm) authors the schematic through the `Drawing` `+=` insertion surface and the fluent placement algebra, drives the standalone SVG backend with `text='path'` so the emitted SVG is font-independent, encodes `get_imagedata("svg")` bytes, content-keys them through `rasm.runtime.identity#ContentIdentity`, offloads the synchronous render onto the runtime `to_thread` seam, and contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` case; schemdraw owns symbol-anchored schematic authoring and never re-implements the generic graph-layout routing the `rustworkx`/`pyelk`/`fast-sugiyama` owners hold (its `parsing.logicparse` Buchheim tree-placement is the built-in logic-gate fallback, not the routing engine), never rasterizes through its own matplotlib path when `resvg-py`/`vl-convert`/`pyvips` own raster, and never mints the content identity the runtime owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `schemdraw`
- package: `schemdraw`
- import: `schemdraw`
- owner: `artifacts`
- rail: diagram
- license: MIT (`License :: OSI Approved :: MIT License`)
- installed: `0.23`
- build-floor: `Requires-Python >=3.10`; pure-Python `py3-none-any` wheel (no compiled extension, no abi gate) — resolves on cp315 with no `; python_version` marker. The base import closure is `typing-extensions` + `ziafont`/`ziamath` (bundled math/font-path render); `matplotlib` is an OPTIONAL render backend pulled only when `use('matplotlib')` is active, NOT a base dependency — the standalone `'svg'` backend (the design's egress) needs neither matplotlib nor any native library.
- entry points: none (library only)
- capability: author a symbol-rich technical schematic through a `Drawing` context-manager canvas with a backend-selected render engine (standalone pure-SVG or matplotlib); place from a 226-symbol closed `elements` vocabulary plus the `flow` flowchart, `logic` digital-gate, and `dsp` signal-flow domain modules; chain each symbol off the prior element's named anchor through the fluent `.at`/`.right`/`.up`/`.to`/`.anchor`/`.label`/`.color`/`.theta` placement algebra (relative connection, not absolute coordinates); compose a custom symbol from the low-level `Segment`/`SegmentCircle`/`SegmentArc`/`SegmentText`/`SegmentPath`/`SegmentPoly`/`SegmentBezier` primitive vocabulary through `ElementCompound`; build a logic network from a boolean expression string (`parsing.logicparse`); render a Karnaugh map / truth table / timing diagram from a `dict` (`logic.Kmap`/`Table`/`TimingDiagram`/`BitField`); set global appearance through `config`/`theme`/`svgconfig`/`style`; and serialize to an SVG string, a raster/vector byte payload over the `ImageFormat` vocabulary, or a `get_segments`/`get_bbox` geometry query — all without hand-emitting an SVG tag or a `<path d>` string.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document canvas, render context, geometry
- rail: diagram

`Drawing` is the one document canvas and a context-manager — `with Drawing() as d:` opens the canvas, `d += element` / `d.add(element)` is the single polymorphic insertion surface, and `__exit__` finalizes the layout and (if `show=True`) displays. `Transform` carries the per-element placement frame (theta + global/local shift + zoom) the fluent algebra threads; `BBox`/`Point` are the geometry value objects the bounding-box and anchor queries return. There is no per-shape `add_resistor`/`add_capacitor` method — every symbol is a constructed `Element` handed to `+=`.

| [INDEX] | TYPE          | KIND               | ROLE                                                                                   |
| :-----: | ------------- | ------------------ | -------------------------------------------------------------------------------------- |
|  [01]   | `Drawing`     | document canvas     | `Drawing(canvas=None, file=None, show=True, transparent=False, dpi=72, **kwargs)`; context-manager, `+=`/`add` insertion, `get_imagedata`/`save`/`get_segments`/`get_bbox` egress |
|  [02]   | `Transform`   | placement frame     | `Transform(theta, globalshift, localshift=(0,0), zoom=Point(1,1))`; `transform(xy)`/`transform_array(arr)` map a local point to the canvas |
|  [03]   | `ImageFormat` | egress format enum  | `str`-Enum `SVG`/`PNG`/`PDF`/`EPS`/`PS`/`PGF`/`JPG`/`TIF`/`RAW`/`RGBA` — the `get_imagedata(fmt)` discriminant |

[PUBLIC_TYPE_SCOPE]: element base hierarchy
- rail: diagram

Every drawable symbol derives `Element`; the hierarchy splits into `Element2Term` (a two-terminal in-line symbol like a resistor — carries the `.length`/`.to`/`.tox`/`.toy`/`.endpoints`/`.dot`/`.idot` extra placement surface), `ElementImage` (an embedded raster), `ElementDrawing` (one `Drawing` embedded as a reusable symbol), `ElementCompound` (the base a custom multi-`Segment` symbol composes from), and `Container`/`Encircle` (a box drawn around a set of elements). The base classes are the bounded algebra the schematic author dispatches over; placement, styling, and labelling read off the base, never off a per-symbol special case.

| [INDEX] | TYPE              | BASE          | ROLE                                                                          |
| :-----: | ----------------- | ------------- | ----------------------------------------------------------------------------- |
|  [01]   | `Element`         | `object`      | abstract symbol root; fluent placement/style/label algebra, `.anchors` dict, `.segments` list, `.get_bbox()` |
|  [02]   | `Element2Term`    | `Element`     | two-terminal in-line symbol; adds `.length`/`.to`/`.tox`/`.toy`/`.endpoints`/`.shift`/`.dot`/`.idot` |
|  [03]   | `ElementImage`    | `Element`     | `ElementImage(image, width, height, xy=Point(0,0), imgfmt=None, **kwargs)` embedded raster |
|  [04]   | `ElementDrawing`  | `Element`     | `ElementDrawing(drawing, **kwargs)` one `Drawing` reused as a single symbol  |
|  [05]   | `ElementCompound` | `Element`     | base for a custom symbol assembled from `Segment*` primitives in `__init__`  |
|  [06]   | `Container`       | `Element`     | `Container(drawing, *, cornerradius=None, padx=None, pady=None)` box around a sub-drawing |

[PUBLIC_TYPE_SCOPE]: low-level segment primitive vocabulary
- rail: diagram

`Segment*` is the bounded primitive grammar every `Element` is built from and a custom symbol composes through `ElementCompound`: a placed element's `.segments` is a `list[SegmentType]` of these, and `Drawing.get_segments()` returns the flattened segment stream of the whole schematic for geometry inspection or a custom render. A custom symbol appends `Segment*` instances to `self.segments` in its `__init__`; there is no hand-emitted path — the segment IS the typed primitive.

| [INDEX] | TYPE             | KIND          | ROLE                                                                          |
| :-----: | ---------------- | ------------- | ----------------------------------------------------------------------------- |
|  [01]   | `Segment`        | path primitive | `Segment(path, color=None, lw=None, ls=None, capstyle=None, joinstyle=None, fill=None, arrow=None, arrowwidth=0.15, arrowlength=0.25, clip=None, zorder=None, visible=True)` a polyline path |
|  [02]   | `SegmentCircle`  | circle primitive | `SegmentCircle(center, radius, color=None, lw=None, ls=None, fill=None, clip=None, zorder=None, ref=None, visible=True)` |
|  [03]   | `SegmentArc`     | arc primitive | `SegmentArc(center, width, height, theta1=35, theta2=-35, arrow=None, angle=0, ...)` elliptical arc |
|  [04]   | `SegmentText`    | text primitive | `SegmentText(pos, label, align=None, rotation=None, rotation_mode=None, rotation_global=True, color=None, bgcolor=None, fontsize=14, font=None, mathfont=None, clip=None, zorder=None, visible=True, href=None, decoration=None)` |
|  [05]   | `SegmentPath`    | mixed-path primitive | `SegmentPath(path, ...)` a path mixing `XY` points and SVG command strings  |
|  [06]   | `SegmentPoly`    | polygon primitive | `SegmentPoly(verts, closed=True, cornerradius=0, color=None, fill=None, lw=None, ls=None, hatch=False, joinstyle=None, capstyle=None, ...)` |
|  [07]   | `SegmentBezier`  | bezier primitive | `SegmentBezier(p, color=None, lw=None, ls=None, capstyle=None, arrow=None, arrowlength=0.25, arrowwidth=0.15, ...)` |

[PUBLIC_TYPE_SCOPE]: `elements` symbol vocabulary (`schemdraw.elements`, 226 symbols)
- rail: diagram

`schemdraw.elements` is the closed electrical/electronic schematic-symbol vocabulary — 226 named symbols in `__all__`, every one an `Element`/`Element2Term` subclass placed by construction and discrimination-by-type (the SYMBOL is a class, with a standards variant as a sibling, e.g. `ResistorIEEE`/`ResistorIEC`). The owning page imports `from schemdraw import elements as elm` and selects the symbol by name; below is the family map (the exhaustive 226-name roster is the package `elements.__all__`, the verifiable closed set).

| [INDEX] | FAMILY                | REPRESENTATIVE SYMBOLS                                                              |
| :-----: | --------------------- | ---------------------------------------------------------------------------------- |
|  [01]   | passive two-term      | `Resistor`/`ResistorIEEE`/`ResistorIEC`/`ResistorVar`/`Rshunt`/`Thermistor`/`Photoresistor`/`Capacitor`/`Capacitor2`/`CapacitorVar`/`Inductor`/`Inductor2`/`Crystal`/`Fuse`/`CPE`/`Memristor` |
|  [02]   | diodes/semiconductor  | `Diode`/`Schottky`/`Zener`/`DiodeTVS`/`Varactor`/`LED`/`LED2`/`Photodiode`/`Diac`/`Triac`/`SCR`/`Josephson` |
|  [03]   | sources/grounds       | `Source`/`SourceV`/`SourceI`/`SourceSin`/`SourcePulse`/`SourceControlled`/`Battery`/`BatteryCell`/`Ground`/`GroundSignal`/`GroundChassis`/`Vdd`/`Vss`/`Antenna`/`NoConnect` |
|  [04]   | meters/indicators     | `MeterV`/`MeterI`/`MeterA`/`MeterOhm`/`Lamp`/`Solar`/`Neon`/`Oscilloscope`                |
|  [05]   | switches/buttons      | `Switch`/`SwitchSpdt`/`SwitchDpst`/`SwitchDpdt`/`Button`/`SwitchReed`/`SwitchRotary`/`SwitchDIP`/`Breaker` |
|  [06]   | transistors           | `NFet`/`PFet`/`JFet`/`JFetN`/`JFetP`/`Bjt`/`BjtNpn`/`BjtPnp`/`IgbtN`/`IgbtP`/`NMos`/`PMos`/`Hemt`/`AnalogNFet` |
|  [07]   | transducers/machines  | `Speaker`/`Mic`/`Motor`/`AudioJack`/`Transformer`/`Coax`/`Triax`                          |
|  [08]   | integrated circuits   | `Ic`/`IcPin`/`IcDIP`/`Multiplexer`/`VoltageRegulator`/`DFlipFlop`/`JKFlipFlop`/`Ic555`/`SevenSegment`/`Opamp` |
|  [09]   | lines/connectors/wires| `Line`/`Wire`/`DataBusLine`/`Dot`/`Arrow`/`Arrowhead`/`DotDotDot`/`Gap`/`OrthoLines`/`RightLines`/`Jumper`/`BusConnect`/`BusLine` |
|  [10]   | annotation/markup     | `Label`/`Tag`/`Annotate`/`ZLabel`/`CurrentLabel`/`CurrentLabelInline`/`VoltageLabelArc`/`LoopCurrent`/`LoopArrow`/`Rect`/`Encircle`/`EncircleBox` |
|  [11]   | arcs                  | `Arc2`/`Arc3`/`ArcZ`/`ArcN`/`ArcLoop`                                                     |
|  [12]   | connectors/headers    | `Header`/`Jumper`/`DB25`/`DB9`/`DE9`/`DA15`/`Plug`/`Jack`/`Terminal`/`CoaxConnect`        |
|  [13]   | compound/two-port     | `Optocoupler`/`Relay`/`Rectifier`/`Wheatstone`/`TwoPort`/`VoltageTransactor`/`CurrentTransactor`/`Nullor`/`VMCMPair` |
|  [14]   | mains outlets/tubes   | `OutletA`..`OutletL` (IEC/NEMA outlets) · `VacuumTube`/`Triode`/`Tetrode`/`Pentode`/`NixieTube`/`TubeDiode` |

[PUBLIC_TYPE_SCOPE]: domain element modules (`flow`, `logic`, `dsp`)
- rail: diagram

The three domain modules carry the non-electrical diagram vocabularies on the SAME `Drawing`/`Element`/placement spine: `schemdraw.flow` is the flowchart owner (terminal/process/decision/data boxes the AEC `schedule`/`detail` flowchart figures lower onto), `schemdraw.logic` the digital-logic gate vocabulary plus the `Kmap`/`Table`/`TimingDiagram`/`BitField` structured owners, `schemdraw.dsp` the signal-flow/DSP block vocabulary. Each module's elements are placed and connected through the identical fluent algebra.

| [INDEX] | MODULE   | OWNS                                                                                                | KEY SYMBOLS                                                                  |
| :-----: | -------- | --------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
|  [01]   | `flow`   | flowchart boxes + flow connectors                                                                   | `Box`/`RoundBox`/`Process`/`RoundProcess`/`Decision`/`Data`/`Subroutine`/`Terminal`/`Start`/`State`/`StateEnd`/`Connect`/`Circle`/`Ellipse`/`Line`/`Arrow`/`Wire`/`Arc2`/`Arc3`/`ArcZ`/`ArcN`/`ArcLoop` |
|  [02]   | `logic`  | digital-logic gates + structured logic owners                                                       | `And`/`Nand`/`Or`/`Nor`/`Xor`/`Xnor`/`Buf`/`Not`/`NotNot`/`Tristate`/`Tgate`/`Schmitt`/`SchmittAnd` · `Kmap`/`Table`/`TimingDiagram`/`BitField` |
|  [03]   | `dsp`    | signal-flow/DSP blocks                                                                               | `Square`/`Circle`/`Sum`/`SumSigma`/`Mixer`/`Amp`/`Oscillator`/`OscillatorBox`/`Filter`/`Adc`/`Dac`/`Demod`/`Circulator`/`Isolator`/`VGA`/`Speaker`/`Antenna`/`Ic`/`Multiplexer` |

[PUBLIC_TYPE_SCOPE]: structured logic owners (`schemdraw.logic`)
- rail: diagram

`Kmap`/`Table`/`TimingDiagram`/`BitField` are the four structured diagram owners that fold a `dict`/string into a complete figure in one constructor — not a per-cell placement loop. They are the data-driven entrypoints a downstream truth-table / register-map / waveform figure lowers onto.

| [INDEX] | TYPE             | KIND          | ROLE                                                                          |
| :-----: | ---------------- | ------------- | ----------------------------------------------------------------------------- |
|  [01]   | `Kmap`           | structured    | `Kmap(names='ABCD', truthtable=None, groups=None, default='0', **kwargs)` a Karnaugh map with grouping ellipses |
|  [02]   | `Table`          | structured    | `Table(table, colfmt=None, fontsize=12, font='sans', **kwargs)` a markdown-pipe table rendered as a schematic figure |
|  [03]   | `TimingDiagram`  | structured    | `TimingDiagram(waved, **kwargs)` a WaveJSON-style digital-timing diagram from a `dict` |
|  [04]   | `BitField`       | structured    | `BitField(reg, **kwargs)` a register bit-field map from a `dict`              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Drawing` — open, insert, render, egress
- rail: diagram

`Drawing(canvas=None, file=None, show=True, transparent=False, dpi=72, **kwargs)` is the canvas; entered as a context manager (`with Drawing() as d:`) so the layout finalizes on `__exit__`, or constructed and rendered explicitly. `d += element` (`__iadd__`) and `d.add(element)` are the one polymorphic insertion surface — both place a constructed `Element` and RETURN it (so its bound anchors are addressable), and `add_elements(*elements)` batches. `config(...)` overrides this drawing's appearance (unit length, font, color, line width). The egress family is one surface: `get_imagedata(fmt)` returns the rendered bytes over the `ImageFormat` vocabulary (the design's `get_imagedata("svg")` is the durable artifact under the standalone backend), `save(fname)` writes to disk, `get_segments()` returns the flattened primitive stream, and `get_bbox()`/`here`/`theta` expose the geometry/cursor state.

| [INDEX] | MEMBER                                                                          | KIND      | ROLE                                                       |
| :-----: | ------------------------------------------------------------------------------- | --------- | ---------------------------------------------------------- |
|  [01]   | `Drawing(canvas=None, file=None, show=True, transparent=False, dpi=72, **kwargs)` | construct | the schematic canvas; context-manager or explicit          |
|  [02]   | `Drawing.add(element)` / `Drawing.__iadd__(element)` (`d += e`)                  | build     | place one constructed `Element`, return it (anchors addressable) |
|  [03]   | `Drawing.add_elements(*elements)`                                               | build     | batch-place several elements                                |
|  [04]   | `Drawing.config(unit=None, inches_per_unit=None, fontsize=None, font=None, color=None, lw=None, ls=None, fill=None, bgcolor=None, margin=None, mathfont=None)` | style | per-drawing appearance override |
|  [05]   | `Drawing.add_svgdef(svgdef)`                                                     | build     | inject a raw `<defs>` SVG fragment (gradient/pattern/marker) into the SVG document |
|  [06]   | `Drawing.get_imagedata(fmt='svg')`                                              | egress    | rendered bytes over `ImageFormat` (`'svg'` = the durable artifact under `use('svg')`) |
|  [07]   | `Drawing.save(fname, transparent=True, dpi=72)`                                 | egress    | write the rendered image to disk (format inferred from extension) |
|  [08]   | `Drawing.get_segments()` / `Drawing.get_bbox()`                                 | query     | the flattened `Segment*` stream / the schematic bounding box |
|  [09]   | `Drawing.move(dx, dy)` / `move_from(ref, dx, dy, theta=None)` / `here` / `theta` | cursor    | move / re-anchor / read the placement cursor (the next element's start) |
|  [10]   | `Drawing.push()` / `pop()` / `undo()`                                           | cursor    | save / restore / remove the last placement cursor state     |
|  [11]   | `Drawing.draw(show=True, canvas=None)`                                          | render    | force a (re)render onto a backend (explicit, non-context use) |
|  [12]   | `Drawing.container(cornerradius=0.3, padx=0.75, pady=0.75)` / `hold()` / `interactive(...)` | build | open a `Container` box scope / hold cursor / toggle interactive redraw |

[ENTRYPOINT_SCOPE]: fluent placement algebra (`Element`)
- rail: diagram

Every `Element` carries the chainable placement/style/label algebra — each method returns `self`, so a symbol is constructed, positioned RELATIVE to the prior element's named anchor, styled, and labelled in one expression: `elm.Resistor().right().at(prev.end).label('R1').color('blue')`. `.at(xy | (element, anchor))` pins the start to an absolute point or another element's named anchor; `.right`/`.left`/`.up`/`.down`/`.theta(deg)` set direction; `.anchor(name)` chooses WHICH of the element's anchors lands at the placement point; `.label(text, loc, ...)` attaches positioned text; `.color`/`.fill`/`.linewidth`/`.linestyle`/`.style`/`.zorder`/`.scale`/`.flip`/`.reverse` style. This relative-connection algebra IS the schematic-authoring model — coordinates are derived, rarely typed.

| [INDEX] | MEMBER                                                                          | KIND      | ROLE                                                       |
| :-----: | ------------------------------------------------------------------------------- | --------- | ---------------------------------------------------------- |
|  [01]   | `Element.at(xy | (element, anchor), dx=0, dy=0)`                                 | place     | pin the start to a point or another element's named anchor  |
|  [02]   | `Element.right()` / `left()` / `up()` / `down()` / `theta(deg)`                 | place     | set the placement direction                                 |
|  [03]   | `Element.anchor(name)` / `Element.anchors` (attr) / `set_anchor(name)`          | place     | choose the anchor that lands at the placement point; the named-anchor dict |
|  [04]   | `Element.label(label, loc=None, ofst=None, halign=None, valign=None, rotate=False, fontsize=None, font=None, mathfont=None, color=None, href=None, decoration=None)` | annotate | attach positioned (optionally math/hyperlinked) text |
|  [05]   | `Element.color(c)` / `fill(c=True)` / `gradient_fill(c1, c2, vertical=True)`    | style     | stroke color / fill / two-stop gradient fill                |
|  [06]   | `Element.linewidth(lw)` / `linestyle(ls)` / `style(color=, fill=, ls=, lw=)`    | style     | line width / dash style / bundled style                     |
|  [07]   | `Element.scale(s)` / `scalex(s)` / `scaley(s)` / `flip()` / `reverse()` / `zorder(z)` | style | scale / mirror / reverse direction / z-order            |
|  [08]   | `Element.drop(drop)` / `hold()` / `get_bbox(transform=False, includetext=True)` | place     | set the exit anchor for the next element / hold cursor / bbox |

[ENTRYPOINT_SCOPE]: `Element2Term` extra placement (in-line two-terminal symbols)
- rail: diagram

A two-terminal symbol (resistor, capacitor, wire) adds the in-line placement surface: `.to(xy)` stretches the element so its END lands at a point, `.tox(x)`/`.toy(y)` stretch to an x/y while keeping the other axis, `.length(L)` fixes the span, `.endpoints(start, end)` pins both ends, and `.dot`/`.idot` add a connection dot at the end/start. This is how a wire run reaches a target node by COORDINATE-FREE stretching.

| [INDEX] | MEMBER                                          | KIND    | ROLE                                                        |
| :-----: | ----------------------------------------------- | ------- | ----------------------------------------------------------- |
|  [01]   | `Element2Term.to(xy, dx=0, dy=0)`               | place   | stretch so the end lands at a point                          |
|  [02]   | `Element2Term.tox(x | XY | Element)` / `toy(y | XY | Element)` | place | stretch to an x / y, keeping the other axis           |
|  [03]   | `Element2Term.length(L)` / `endpoints(start, end)` / `shift(s)` | place | fix the span / pin both ends / shift along the run    |
|  [04]   | `Element2Term.dot(open=False)` / `idot(open=False)` | place | add a (open) connection dot at the end / start             |

[ENTRYPOINT_SCOPE]: integrated-circuit & compound builders (`elements`)
- rail: diagram

`Ic(size=None, pins=None, slant=0)` builds a named-pin integrated-circuit box from a `Sequence[IcPin]`, each pin a named, sided (`L`/`R`/`T`/`B`), positioned terminal that becomes an addressable anchor on the placed `Ic` (so a wire connects to `ic.IN`/`ic.OUT` by name). `Multiplexer` is the slanted-IC specialization, `Encircle`/`Container` draw a box around a set of elements, and `ElementCompound` is the base a custom symbol composes from by appending `Segment*` to `self.segments`.

| [INDEX] | MEMBER                                                                          | KIND      | ROLE                                                       |
| :-----: | ------------------------------------------------------------------------------- | --------- | ---------------------------------------------------------- |
|  [01]   | `Ic(size=None, pins=None, slant=0, **kwargs)`                                    | construct | a named-pin IC box; each `IcPin.name` becomes an addressable anchor |
|  [02]   | `IcPin(name=None, pin=None, side='L', pos=None, slot=None, invert=False, invertradius=0.15, anchorname=None, rotation=0, ...)` | construct | one named/sided/positioned IC pin (the bubble for `invert=True`) |
|  [03]   | `Multiplexer(demux=False, size=None, pins=None, slant=25, **kwargs)`             | construct | a slanted-IC mux/demux from `IcPin`s                        |
|  [04]   | `Encircle(elm_list=None, *, padx=None, pady=None, includelabels=True, **kwargs)` / `EncircleBox(...)` | construct | a rounded / rectangular box drawn around a set of placed elements |
|  [05]   | `Opamp(*, sign=None, leads=None, **kwargs)`                                      | construct | op-amp with `in1`/`in2`/`out`/`vd`/`vs` named anchors        |
|  [06]   | `ElementImage(image, width, height, xy=Point(0,0), imgfmt=None, **kwargs)` / `ElementDrawing(drawing, **kwargs)` | construct | embed a raster / embed a `Drawing` as one reusable symbol |

[ENTRYPOINT_SCOPE]: structured logic & boolean-expression parse (`logic`, `parsing`)
- rail: diagram

`logic.Kmap`/`Table`/`TimingDiagram`/`BitField` fold a `dict`/string into a complete structured figure in one constructor. `parsing.logicparse(expr, ...)` is the boolean-expression-to-gate-network builder — it parses an expression string (`'a and (b or c)'`), runs the bundled Buchheim tree placement, and RETURNS a fully-placed `Drawing` of logic gates (the built-in logic-layout fallback, not the generic routing engine).

| [INDEX] | MEMBER                                                                          | KIND      | ROLE                                                       |
| :-----: | ------------------------------------------------------------------------------- | --------- | ---------------------------------------------------------- |
|  [01]   | `logic.Kmap(names='ABCD', truthtable=None, groups=None, default='0', **kwargs)` | build     | a Karnaugh map with grouping ellipses                       |
|  [02]   | `logic.Table(table, colfmt=None, fontsize=12, font='sans', **kwargs)`           | build     | a markdown-pipe truth/data table as a schematic figure      |
|  [03]   | `logic.TimingDiagram(waved, **kwargs)` / `logic.BitField(reg, **kwargs)`        | build     | a WaveJSON timing diagram / a register bit-field map from a `dict` |
|  [04]   | `parsing.logicparse(expr, gateW=2, gateH=0.75, outlabel=None)`                   | build     | parse a boolean expression to a fully-placed gate `Drawing` (returns `Drawing`) |

[ENTRYPOINT_SCOPE]: global appearance & backend selection (`config`, `theme`, `use`, `svgconfig`, `style`)
- rail: diagram

`use(backend)` selects the render engine ONCE per process — `'svg'` is the standalone pure-SVG backend (no matplotlib, no native lib; the design's egress) and `'matplotlib'` the richer raster/EPS/PGF path. `config(...)` and `theme(name)` set the global default appearance; `svgconfig` carries the SVG-backend render policy (`text='path'` emits font-independent SVG `<path>` text and enables ziamath math rendering, `text='text'` emits native SVG `<text>` elements using system fonts; `svg2=True` uses SVG 2.0 features; `precision=3` the coordinate decimal precision). `style.color_rgb`/`color_hex`/`validate_color` are the color-input helpers.

| [INDEX] | MEMBER                                                                          | KIND      | ROLE                                                       |
| :-----: | ------------------------------------------------------------------------------- | --------- | ---------------------------------------------------------- |
|  [01]   | `use(backend='matplotlib')`                                                      | backend   | select the render engine: `'svg'` (standalone) or `'matplotlib'` |
|  [02]   | `config(unit=3.0, inches_per_unit=0.5, lblofst=0.1, fontsize=14.0, font='sans-serif', color='black', lw=2.0, ls='-', fill=None, bgcolor=None, margin=0.1, mathfont=None)` | style | global default appearance |
|  [03]   | `theme(theme='default')`                                                        | style     | global theme — `default`/`dark`/`solarizedd`/`solarizedl`/`onedork`/`oceans16`/`monokai`/`gruvboxl`/`gruvboxd`/`grade3`/`chesterish` |
|  [04]   | `svgconfig.text` (`'path'`/`'text'`) / `svgconfig.svg2` / `svgconfig.precision` / `svgconfig.useBatik` | style | SVG-backend render policy (path-vs-native text, SVG2, precision) |
|  [05]   | `style.color_rgb(c)` / `color_hex(c)` / `color_hsl(c)` / `validate_color(c)`    | validate  | color-input parsing/validation helpers                      |
|  [06]   | `debug(dwgbbox=True, elmbbox=True)`                                             | debug     | overlay the drawing/element bounding boxes (layout debugging) |

[ENTRYPOINT_SCOPE]: custom symbol composition (`Segment*` + `ElementCompound`)
- rail: diagram

A symbol the 226-element vocabulary does not carry is composed by subclassing `ElementCompound` and appending typed `Segment*` primitives to `self.segments` in `__init__`, plus declaring named anchors in `self.anchors` — so a custom AEC fixture symbol (a sprinkler head, a damper, a building-system glyph) is one `Segment*`-built class, never a hand-emitted SVG path. The placed element then participates in the identical fluent placement algebra.

| [INDEX] | MEMBER                                                          | KIND    | ROLE                                                       |
| :-----: | -------------------------------------------------------------- | ------- | ---------------------------------------------------------- |
|  [01]   | `class MySymbol(ElementCompound): ...` appending `Segment*` to `self.segments` | compose | a custom symbol from typed primitives (no hand-emitted path) |
|  [02]   | `self.anchors['name'] = (x, y)` in the symbol `__init__`        | compose | declare a named terminal anchor on the custom symbol         |
|  [03]   | `self.segments.append(Segment(...) | SegmentPoly(...) | SegmentArc(...) | SegmentText(...))` | compose | append a typed path/poly/arc/text primitive |

## [04]-[IMPLEMENTATION_LAW]

- import: `import schemdraw` and `from schemdraw import elements as elm` (plus `from schemdraw import flow, logic, dsp` where the domain vocabulary is needed) at boundary scope only; the distribution and import name are both `schemdraw`; the version is `importlib.metadata.version("schemdraw")` (`"0.23"`; `schemdraw.__all__` is the verifiable top-level surface). schemdraw is the schematic-egress boundary owner — keep the import at the egress edge, never let the `Drawing`/`Element` object model leak into domain code; the domain holds the schematic content (the component graph / flowchart structure / logic network) and lowers it here through the fluent placement algebra.
- backend axis: `use('svg')` selects the standalone pure-SVG backend ONCE — it renders in-process with no matplotlib and no native library, so the design fixes `use('svg')` and treats `get_imagedata("svg")` as the durable artifact; `use('matplotlib')` is the richer raster/EPS/PGF render path engaged ONLY when a raster the SVG backend cannot reach is required (and even then the SVG-then-`resvg-py`/`vl-convert` raster route is preferred over the matplotlib backend). The backend is a process-global, set once at the diagram-rail boundary, never per-element.
- canvas axis: `Drawing` is the one document spine, entered as a context manager so the layout finalizes on `__exit__`; `d += element` / `d.add(element)` is the single polymorphic insertion surface that places a constructed `Element` and returns it — there is no `add_resistor`/`add_capacitor` family, the SYMBOL is a constructed class handed to one entrypoint, and a new symbol is one `elements` class (or one `ElementCompound`), never a new insertion method.
- placement axis: the schematic is authored by RELATIVE connection through the fluent algebra (`.at(prev.anchor)`/`.right`/`.up`/`.to`/`.tox`/`.toy`/`.length`/`.anchor(name)`), each method returning `self` for chaining — coordinates are DERIVED from the prior element's named anchor, rarely typed; a wire reaches a target by `.to`/`.tox`/`.toy` stretching, an IC pin is addressed by name (`ic.IN`), so the placement is coordinate-free and the diagram structure (not a coordinate table) is the source of truth.
- symbol axis: `elements` is the 226-symbol closed vocabulary (the standards variant a sibling class — `ResistorIEEE`/`ResistorIEC`), `flow`/`logic`/`dsp` the domain element modules on the same spine, and `Segment`/`SegmentCircle`/`SegmentArc`/`SegmentText`/`SegmentPath`/`SegmentPoly`/`SegmentBezier` the low-level primitive grammar every element is built from; a custom symbol subclasses `ElementCompound` and appends typed `Segment*` to `self.segments` with named anchors in `self.anchors` — never a hand-emitted SVG path, never a free `<path d>` string.
- structured-figure axis: `logic.Kmap`/`Table`/`TimingDiagram`/`BitField` fold a `dict`/string into a complete figure in one constructor, and `parsing.logicparse(expr)` parses a boolean expression to a fully-placed gate `Drawing` through the bundled Buchheim tree placement — these are the built-in data-driven entrypoints and the logic-layout FALLBACK; the generic graph-layout routing stays with the `rustworkx`/`pyelk`/`fast-sugiyama` owners, and schemdraw never re-implements that routing.
- appearance axis: `config`/`theme` set the global default and `Drawing.config` the per-drawing override; `svgconfig.text='path'` is the design default so the emitted SVG is font-independent (`'text'` only when live editable native-SVG text is required), `svgconfig.svg2`/`precision` the SVG-render policy; color binds through `style.color_rgb`/`color_hex`/`validate_color` from the `graphic/color/derive#DERIVE` palette, so a schematic's color traces to one palette index, never a per-symbol literal chosen at the egress.
- egress axis: `get_imagedata(fmt)` over the `ImageFormat` vocabulary is the one render family — `get_imagedata("svg")` the SVG bytes the design records, `save(fname)` the disk write, `get_segments()`/`get_bbox()` the geometry query; the SVG bytes are the artifact this owner produces, recorded under the runtime content-key, never trusting `save`'s disk path as the artifact of record.
- boundary: schemdraw owns symbol-anchored technical-schematic authoring (electrical/electronic schematics, signal-flow/DSP, digital logic, flowcharts, state diagrams, K-maps, timing diagrams) and the custom `Segment*`/`ElementCompound` symbol grammar; the generic data-driven node-link/ER/Sankey/section-callout AEC diagram over the seven-mark `DiagramGlyph` grammar is the sibling `drawsvg` arm in `visualization/diagram/draw#DRAW`, the editable-`.drawio` egress is the `drawpyo` arm, graph layout/routing is `rustworkx`/`pyelk`/`fast-sugiyama`/`libavoid`, rasterization is `resvg-py`/`vl-convert`/`pyvips`, the content identity is `rasm.runtime.identity#ContentIdentity`, and live UI is out of scope.
- evidence: each schematic egress captures the placed-element count, the `get_segments()` primitive count, the `get_bbox()` extent, and the emitted SVG byte length as a `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` row — the SAME case the `drawsvg`/`drawpyo` diagram arms and the `visualization/diagram/layout#LAYOUT` owner contribute, never a parallel schematic-only receipt shape.

[STACKING]:
- `expression` rail: the `Drawing.get_imagedata`/`save` egress is wrapped by the owning page's `RuntimeRail`/`async_boundary` (`rasm.runtime.faults`) so a render fault (an unknown backend, an off-domain `validate_color`/`validate_linestyle` raise, an `ElementImage` bad-format) lands as a typed `Result` failure, never a raw exception crossing the domain — the schematic rail never lets a `ValueError` from a color/linestyle validator or a backend mismatch escape.
- `msgspec`/`pydantic` rail: the placed-element count, the `get_segments()` primitive count, the `get_bbox()` extent, and the SVG byte length populate a `msgspec.Struct` `ArtifactReceipt.Diagram(key, "diagram-schematic", nodes, edges, "schemdraw")` row on the one shared `core/receipt#RECEIPT` family — the schematic facts are structured, not stringly, and share the case the generic-diagram arms emit (`nodes` the placed-element tally, `edges` the wire/connector tally, `algorithm` the `"schemdraw"` engine descriptor).
- `beartype` rail: the boundary lowering helper (the function turning a domain component graph / flowchart / logic network into the placed `Drawing`) is `@beartype`-decorated so the symbol-name selection and the placement-anchor wiring are validated at the egress edge before any `Drawing` mutation, catching a stale symbol name or a bad anchor as a typed contract violation, not a deep `AttributeError`.
- `ContentIdentity` rail: the owning page encodes `get_imagedata("svg")` to bytes and derives the content key through `rasm.runtime.identity#ContentIdentity.of("diagram-schematic", svg_bytes)` over the rendered bytes (never a second render), so the schematic artifact is content-addressed in the same artifact index the cache-check law uses — schemdraw never mints identity.
- `anyio` rail: schemdraw is pure-Python and fully synchronous; the owning page offloads the `Drawing.__exit__`/`get_imagedata` render onto `to_thread.run_sync(..., limiter=_DIAGRAM_LANES)` off the event loop (the same shared-address-space thread arm the sibling `drawsvg` `_render` and the GIL-releasing `rustworkx` layout take — the subinterpreter `to_interpreter` arm cannot load schemdraw's `ziafont`/`ziamath` render path or the `msgspec`/`numpy` receipt owners), so a large schematic render never blocks the loop while the `msgspec` receipt owner stays event-loop-side.
- `structlog`/`opentelemetry` rail: the render is bracketed by the runtime observability seam so the schematic op emits a structured span carrying the backend, the element/segment counts, and the byte length as span attributes, the same diagram-rail telemetry the `drawsvg`/`drawpyo` arms emit — one observability shape across the three diagram egress arms.
- `draw#DRAW` sibling seam: schemdraw is owned by `visualization/diagram/schematic#SCHEMATIC`, disjoint from `visualization/diagram/draw#DRAW`'s `drawsvg` general-diagram arm and the `drawpyo` `.drawio` arm by concern — the seven-mark `DiagramGlyph` grammar (anonymous `Node`/`Edge`/`Swimlane`/`Annotation`/`Marker`/`Area`/`Fragment`) the `drawsvg` arm consumes cannot express a named symbol with bound anchor terminals (a resistor, an op-amp, a NAND gate, an ADC), so the schematic content lowers directly onto the `elements`/`flow`/`logic`/`dsp` vocabulary here while the data-driven AEC diagram lowers onto `DiagramGlyph` there — one diagram rail, three egress arms partitioned by diagram CLASS, never duplicated logic.
- `export/layered#LAYERED` seam: a schematic that must emit as named SVG layers (e.g. a multi-discipline schematic where each system is its own layer) buckets its elements with `zorder`/`add_svgdef` and lowers the rendered SVG to the `export/layered#LAYERED` `Layer(name, source, bbox)` row the OCG/SVG-layer owner binds — the same named-layer egress contract the `drawsvg` arm uses, schemdraw contributing the schematic SVG source.

## [05]-[LOCAL_ADMISSION]

- Package: `schemdraw`
- Owns: symbol-anchored technical-schematic authoring through the `Drawing` context-manager canvas with a standalone pure-SVG (or matplotlib) backend; the 226-symbol closed `elements` electrical/electronic vocabulary plus the `flow` flowchart, `logic` digital-gate, and `dsp` signal-flow domain modules; the fluent relative-connection placement algebra (`.at`/`.right`/`.up`/`.to`/`.tox`/`.toy`/`.length`/`.anchor`/`.label`/`.color`/`.theta`) over named element anchors; the low-level `Segment`/`SegmentCircle`/`SegmentArc`/`SegmentText`/`SegmentPath`/`SegmentPoly`/`SegmentBezier` primitive grammar and `ElementCompound` custom-symbol composition; the `Ic`/`IcPin` named-pin IC builder; the `logic.Kmap`/`Table`/`TimingDiagram`/`BitField` structured owners and the `parsing.logicparse` boolean-expression-to-gate-network builder; the `config`/`theme`/`svgconfig`/`style` appearance owners; and the `get_imagedata`/`save`/`get_segments`/`get_bbox` egress family over the `ImageFormat` vocabulary.
- Accept: authoring schematic and engineering diagrams whose marks are named symbols with bound anchor terminals — electrical/electronic schematics, signal-flow/DSP block diagrams, digital-logic gate networks, flowcharts, state diagrams, Karnaugh maps, register bit-fields, and digital-timing diagrams — emitted as a font-independent SVG (`svgconfig.text='path'`, `use('svg')`) the downstream consumer composes; custom AEC fixture/system symbols composed from `Segment*`/`ElementCompound`; feeding the one `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` case and the runtime content-key index; lowering to the `export/layered#LAYERED` named-layer rows when multi-discipline layering is required.
- Reject: a hand-emitted SVG tag or a free `<path d>` string where the `elements` vocabulary and `Segment*` primitives exist; an `add_resistor`/`add_capacitor` insertion family where `d +=`/`add` discriminates on the constructed symbol; an absolute-coordinate placement table where the fluent relative-connection algebra (`.at(anchor)`/`.to`/`.anchor`) derives coordinates from the diagram structure; the generic data-driven node-link/ER/Sankey/section-callout AEC diagram over the seven-mark `DiagramGlyph` grammar where the sibling `drawsvg` arm renders it; an editable-`.drawio` egress where the `drawpyo` arm owns it; a re-implemented graph-layout routing where `rustworkx`/`pyelk`/`fast-sugiyama`/`libavoid` route; in-page matplotlib rasterization where `resvg-py`/`vl-convert`/`pyvips` cover raster; a per-symbol color literal where the `graphic/color/derive#DERIVE` palette index binds through `style.color_rgb`; identity minting the runtime owns.
