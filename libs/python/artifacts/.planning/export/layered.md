# [PY_ARTIFACTS_LAYERED]

The editable named-layer export close: `LayeredExport` authors the separable, toggleable, lockable layer structure an external editor re-orders and re-colors, the inverse shape of the `document/egress#FINISH` `FINISHERS` table that STRIPS the layers this owner authors. ONE owner discriminates the editor family over the closed `ExportTarget` `StrEnum` — `SVG` (`drawsvg` named-layer `<g inkscape:groupmode=layer>` groups read as layers by both the Illustrator and the Inkscape panel), `PDF` (`pymupdf` optional-content-group placement enriched by the `pikepdf` `/OCProperties` object model for Acrobat), `ORA` (an OpenRaster `pyvips`/`lxml`/`stream-zip` layered container for GIMP/Krita/raster editors) — each target a `LayerEngine` row in the `ENGINES` policy table binding its single `LayerFact`-returning arm, its `preview`-versus-egress receipt discriminant, and its execution `Band`, the table the totality proof and the only dispatch.

Admission is trusted layer material plus one optional untrusted blob. The `tuple[Layer, ...]` rows arrive from the visual producers (`composition/compose#COMPOSE`, `graphic/marks/encode#MARK`, `visualization/diagram/draw#DRAW`, `graphic/raster/io#RASTER`), each carrying its placed `bbox`, beside the one trusted `LayerPolicy` save bundle; the untrusted external `base` PDF the `PDF` arm optionally grafts onto is admitted exactly once at `LayeredExport.of` through the closed `ExportPayload` `TypedDict` and its module-level `TypeAdapter`. `of` rejects an empty layer set into `ExportFault.empty` and a malformed payload into `ExportFault.payload` before the fold runs, so the interior is total over admitted owners and never re-validates a stringly-keyed bag. Every richer `Layer` attribute defaults after `bbox`, so the `Layer(name, source, bbox)` positional contract the five producers construct stays a three-argument call while `visible`/`locked`/`opacity`/`blend`/`intent`/`group`/`color` carry the full editor-panel axis.

Each arm returns a `LayerFact` carrying the authored bytes beside its evidence — the viewport for the `SVG`/`ORA` named documents, the page and authored-layer count for the `PDF` arm — and `_emit` threads that fact onto the frozen owner through `copy.replace` so `contribute` projects the receipt without a second parse. Placement is a `Band` policy value the `_PLACE` table resolves: the `SVG` and `PDF` arms run in-process on the cp315 core (`drawsvg` pure-Python, `pymupdf` `cp310-abi3` and `pikepdf` `cp314-abi3` stable-ABI wheels forward-compatible to cp315), the `ORA` arm crosses one `anyio.to_process.run_sync` worker seam threading the module-level `_GATE` `CapacityLimiter` because libvips and the libxml2-backed `lxml` are off the cp315-core loader path — the same bounded gated-band crossing `graphic/raster/io#RASTER` and `export/indesign#INDESIGN`'s IDML worker ride, never a separate band per call and never the per-loop process-limiter default an unbounded `to_process` would fall to. Cross-cutting receipt emission is the definition-time `@receipted(Redaction.STRUCTURAL)` aspect over the thin pure `_emit`, and the runtime `async_boundary` default `Exception` capture converts a worker death or any engine provider raise (the cp315-core `pymupdf.FileDataError`/`drawsvg` `ValueError`, the gated worker's `pyvips.Error`/`lxml.etree.LxmlError`, a `BrokenWorkerProcess`) into the runtime `BoundaryFault` rail through its `CLASSIFY` table; the owner reads as the receipt weave over a band-dispatched fold. Placement, scaling, and rasterization stay upstream: this owner re-renders nothing and re-lays-out nothing — it receives already-placed sources (vector `SVG` fragments and one-page `PDF`s from the visual producers, encoded raster from `graphic/raster/io#RASTER` for the OpenRaster arm) and authors the layer structure the flat egress deliberately omits. The one modal-arity `exported(LayeredExport | Iterable[LayeredExport])` entry — the shape-discriminating mirror of `export/indesign#INDESIGN`'s `produced` — returns a `RuntimeRail[Block[ContentKey]]`, and each plan contributes the existing `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case (the `SVG`/`ORA` named-layer documents) or `ArtifactReceipt.Egress` case (the OCG-layered `PDF`) carrying the authored-layer count on the `overlays` slot — the same slot the `document/egress#FINISH` REWRITE arm reports a STRIPPED-layer count on, so the layer-count fact rides one settled receipt field across the author/subtract inverse pair, never a new receipt case.

## [01]-[INDEX]

- [01]-[LAYERED]: `LayeredExport` + `ExportTarget` (`SVG`/`PDF`/`ORA`) policy-table dispatch over `drawsvg`/`pymupdf`+`pikepdf`/`pyvips`+`lxml`+`stream-zip`; the `LayerEngine` row binds each target to its `(arm, preview, band)` triple and `ENGINES` is the totality proof; `Band` keys the `_PLACE` placement table so the `SVG`/`PDF` arms run in-process on the cp315 core and the `ORA` arm crosses one `to_process` worker seam threading the module-level `_GATE` `CapacityLimiter` gated-band bound, never an arm-side `if gated` branch and never the per-loop process-limiter default; the `Layer` row carries the full editor-panel attribute axis (`visible`/`locked`/`opacity`/`blend`/`intent`/`group`/`color`) projected per-format — `blend`/`opacity` through the SVG `mix-blend-mode`/`opacity` style, the ORA `composite-op` plus the libvips `_vips_blend` merge nickname and the alpha-scaled opacity, `color` through the preserved SVG `<g>` `data-color` swatch, `visible`/`locked`/`intent`/`group` through the PDF OCG model — the `LayerPolicy` bundle carries the trusted save knobs, and the optional untrusted `base` is admitted once through `LayeredExport.of` over the `ExportPayload` `TypedDict` + `TypeAdapter`; every arm returns a `LayerFact` carrying the authored bytes beside its viewport/page/layer evidence, `_emit` threads the fact onto the frozen owner through `copy.replace`, and `LayeredExport.contribute` folds BOTH the `SVG`/`ORA` `ArtifactReceipt.Preview` and the `PDF` `ArtifactReceipt.Egress` case off the `LayerEngine.preview` discriminant; `ExportFault` is the closed `@tagged_union` over the `payload`/`empty`/`duplicate` admission causes `of` produces (the `duplicate` arm rejecting colliding layer names the by-`name` interior keying would silently drop), the arm-level provider exception families converting to the runtime `BoundaryFault` at the `async_boundary` capsule; the `SVG` arm folds each `Layer` into one Inkscape-recognized `drawsvg.Group(**layer.svg_attrs())` (the `id`/`inkscape:groupmode="layer"`/`inkscape:label`/folded-`style`/`data-color` group dict) nested under one `<g inkscape:groupmode=layer id=group>` folder per `group` label and serializes through `Drawing.as_svg`; the `PDF` arm mints one `pymupdf` `add_ocg`/`show_pdf_page(oc=)` per layer then enriches the `pikepdf` `/OCProperties` catalog with the per-layer `/Usage` view-application or `/PageElement` structural marking and the nested `/D/Order` folder tree the `add_ocg` placement cannot author; the `ORA` arm composites the visible stack through the native per-layer-`BlendMode` `pyvips` `composite` (opacity alpha-scaled, blend via `_vips_blend`) into a faithful `mergedimage.png`, authors `stack.xml` through `lxml`, and frames the OpenRaster ZIP through `stream-zip` with the `mimetype` stored first; the module-level modal-arity `exported`/`_exported` owns the async entry threading each fact-bearing plan through the `@receipted` weave the `async_boundary` capsule converts faults at.

## [02]-[LAYERED]

- Owner: `LayeredExport` the one editable-export owner discriminating the editor family over the closed `ExportTarget` `StrEnum`; `ENGINES` the `frozendict[ExportTarget, LayerEngine]` policy table binding each target to one `LayerEngine` row carrying its single `LayerFact`-returning `arm`, its `preview` receipt discriminant, and its `band` placement value — the table is the totality proof, one arm per target, never an `if target == ...` cascade and never a per-producer layer-emit family. `Band` the closed `{CORE, GATED}` placement vocabulary keying the `_PLACE` table to its execution callable, so the cp315-core arms and the gated-worker arm share one `_emit` dispatch and a new band is one row, never an arm-side offload branch. `Layer` the one frozen `msgspec.Struct` binding a named placed source over the full editor-panel attribute axis — `name` the editor label, `source` the placed bytes the producer emits (an SVG fragment for `SVG`, a one-page PDF for `PDF`, an encoded raster for `ORA`), `bbox` the REQUIRED placed extent, `visible`/`locked`/`opacity`/`blend`/`intent`/`group`/`color` the panel attributes — so the canonical row stays `Layer(name, source, bbox, …)` with `name`/`source`/`bbox` the positional contract `composition/compose#COMPOSE`/`graphic/marks/encode#MARK`/`visualization/diagram/draw#DRAW`/`composition/sheet#SHEET`/`composition/imposition#IMPOSE` construct, never four parallel `names`/`sources`/`flags`/`boxes` lists zipped at the call site; the row derives its own SVG `<g>` attribute dict (`svg_attrs` emitting `inkscape:groupmode="layer"`+`inkscape:label` so Inkscape reads the group AS a layer rather than a plain group, the `id` Illustrator layer name, the folded `display:none`/`opacity:`/`mix-blend-mode:` `style` from `blend.value` — the StrEnum value already the CSS spelling — and the `color` as a preserved `data-color` data attribute, no portable SVG layer-colour slot existing) and projects no provider object so the canonical owner stays codec-free. `LayerPolicy` the one trusted save-knob bundle (`usage` OCG category label, `garbage`/`deflate` pymupdf serialization), `LayerFact` the bytes-plus-evidence carrier every arm returns, `ExportFault` the closed admission vocabulary `of` produces, `BlendMode` the closed 16-mode CSS `mix-blend-mode` vocabulary whose value IS the SVG spelling, whose `ORA` `composite-op` derives through `_ora_op` (`svg:src-over` for `NORMAL`, `svg:<value>` otherwise), and whose libvips merge nickname derives through `_vips_blend` (the British `colour-*` spellings, the `_VIPS_UNMAPPED` NORMAL+HSL set falling to `over` in the flattened preview while `stack.xml` keeps the full `svg:` op); `LayerIntent` the closed ISO 32000 OCG vocabulary spanning the visibility applications (`VIEW`/`PRINT`/`EXPORT`), the `DESIGN` `/Intent` hint, and the `BACKGROUND`/`HEADER_FOOTER`/`FOREGROUND`/`LOGO` `/PageElement` structural roles, driving the `_INTENT` `add_ocg` `/Intent` hint and the uniform `_USAGE` `category -> {stateKey: cell}` sub-dict (`_STATE_KEY` naming the inner key). `drawsvg` owns the named-`Group` SVG authoring, `pymupdf` the native OCG placement (`add_ocg`/`show_pdf_page(oc=)`/`set_layer`), `pikepdf` the `/OCProperties` catalog enrichment (`/Usage`/nested `/Order`), `pyvips` the OpenRaster native per-layer-blend raster `composite` and opacity alpha-scale, `lxml` the `stack.xml` author, `stream-zip` the OpenRaster ZIP container.
- Cases: `ExportTarget` rows · `SVG` (named-layer SVG authoring on the cp315 core — fold each `Layer` into one `drawsvg.Group(**layer.svg_attrs())` carrying the `inkscape:groupmode="layer"`/`inkscape:label` layer idiom (so Inkscape reads it as a LAYER, not a plain group), the `id` Illustrator layer name, the folded `style`, and the `data-color` swatch, wrapping the placed source as a `Raw` verbatim-markup child, nest each leaf under one `<g inkscape:groupmode=layer id=group>` folder per distinct `group` label — the SVG counterpart to the ORA `<stack name=>` and PDF `/Order` folder, NOT a parent-layer-name reference — append every folder and ungrouped leaf to one `Drawing` (the `xmlns:inkscape` namespace declared on the root `<svg>`) sized to the `_viewport` union, serialize through `Drawing.as_svg` into a layered SVG both the Illustrator and the Inkscape layer panel read one-to-one — `preview` receipt) · `PDF` (the Acrobat OCG-layered PDF on the cp315 core — `pymupdf.open` the optional `base` placed layout or a fresh `new_page` sized to the `_viewport`, mint one OCG xref per `Layer` through `Document.add_ocg(name, on=visible, intent=_INTENT[layer.intent], usage=policy.usage)`, place each placed source bound to its layer through `Page.show_pdf_page(rect, src, 0, oc=xref)` which brackets the drawn content in the `/OC … BDC … EMC` marked-content span natively, drive visibility-and-lock through `Document.set_layer(0, on=, off=, locked=)`, serialize through `tobytes`, then `_enriched` re-opens the bytes through `pikepdf` to author the per-layer `/Usage` sub-dict — the print/export view-application or the `/PageElement /Subtype` structural role — and the nested `/D/Order` folder tree `add_ocg` cannot express — `egress` receipt) · `ORA` (the OpenRaster layered container on the gated `to_process` worker — `pyvips.Image.new_from_buffer` decodes each placed raster source, `addalpha`-then-alpha-scale applies per-layer `opacity` and the native `composite(placed[1:], modes[1:])` flattens the visible stack under each layer's own `_vips_blend` mode into a faithful `mergedimage.png` and `thumbnail_buffer` derives the preview, `lxml.etree` authors `stack.xml` nesting each `group`'s layers under one organizational `<stack name=>` folder (the ORA counterpart to the SVG nested `<g>` and the PDF `/Order` tree) and listing each layer top-to-bottom with its `x`/`y` offset, `opacity`, `visibility`, and `composite-op="svg:<mode>"`, and `stream_zip` frames the ZIP with the `mimetype` stored first so GIMP/Krita/MyPaint open each region as a named layer — `preview` receipt) — each one `LayerEngine.arm` resolved off `ENGINES` and placed by `_PLACE[engine.band]`, never re-enumerated by a worker-side `match` and never an `if svg`/`if pdf` branch re-deriving the target the row already names.
- Entry: `exported(plans: LayeredExport | Iterable[LayeredExport])` is the ONE modal-arity production entrypoint — a lone export normalizes to `Block.singleton`, an iterable to `Block.of_seq` at the head — running `_exported` inside one runtime `async_boundary("export.layered", …)` whose default `Exception` capture the runtime `CLASSIFY` routes, awaiting each plan's `@receipted` `_emit` and mints `ContentIdentity.of(f"export-{plan.target}", plan.output)` per plan, returning a `RuntimeRail[Block[ContentKey]]`, the exact `produced` shape `export/indesign#INDESIGN` carries — never a per-instance `author` method nor a `batch`/`mode` knob. Each `_emit` dispatches the one `LayerEngine.arm` off `ENGINES[self.target]` through `_PLACE[engine.band]`: the `SVG`/`PDF` arms resolve in-process on the cp315 core (`drawsvg` pure-Python, `pymupdf`/`pikepdf` stable-ABI wheels ungated in the manifest) while the `ORA` arm crosses the one `to_process` worker seam onto the gated band where libvips and `lxml` live, threading the module-level `_GATE` `CapacityLimiter` — the placement difference is the `band` policy value, never a per-arm offload branch and never the per-loop process-limiter default. `LayeredExport.of` is the admission classmethod returning `Result[Self, ExportFault]`: it rejects an empty layer set into `ExportFault.empty` and a colliding layer-name set into `ExportFault.duplicate` (the interior keys every arm by `name`, so a duplicate would silently drop an OCG, an ORA `data/<name>.png`, or an SVG leaf), admits the optional untrusted `base` through the `ExportPayload` `TypeAdapter` (a malformed payload into `ExportFault.payload`), and constructs the owner the in-process or offloaded fold then runs.
- Auto: `_emit` is the thin pure core the `@receipted` weave wraps — it resolves `engine = ENGINES[self.target]`, awaits `_PLACE[engine.band](engine.arm, self)` so the band table runs the arm in-process (`_in_process`) or across the gated worker (`_offloaded` over `to_process.run_sync`), and threads the returned `LayerFact` onto the frozen owner through `copy.replace(self, fact=fact)`, the seed never mutated; `_exported` mints the content key over each finished plan's `output` (the `fact.data` bytes) through `ContentIdentity.of`, never a re-minted seed. The `SVG` arm builds one `drawsvg.Drawing` over the `_viewport` union (declaring `xmlns:inkscape`), folds each `Layer` into one leaf `Group(**layer.svg_attrs())` whose `Raw(source.decode())` child carries the placed markup, nests each leaf under `folders.get(layer.group, drawing)` — one `<g inkscape:groupmode=layer id=group>` folder minted per distinct `group` label and appended to the drawing once — and serializes through `as_svg`. The `PDF` arm opens the `base`-or-fresh page, mints `xref = add_ocg(name, on=visible, intent=_INTENT[intent], usage=policy.usage)` per layer, places each source — opened in a `with` that closes it once `show_pdf_page(Rect(bbox), src, 0, oc=xref)` copies it — drives the visibility/lock partitions through `set_layer`, serializes through `tobytes`, and `_enriched` folds the per-layer `_usage` `/Usage` sub-dict and the `_order` nested folder tree onto the `pikepdf` `/OCProperties` catalog. The `ORA` arm decodes each source, applies `opacity` by scaling alpha on the `addalpha`-normalized image, stacks the visible layers through the native `composite(placed[1:], modes[1:])` under their `_vips_blend` modes for a faithful `mergedimage.png`, authors `stack.xml` through `lxml`, and frames the OpenRaster ZIP through `stream_zip`. Every arm keys its result through the `LayerFact.data` bytes, never a re-minted identity seed.
- Receipt: each authoring contributes one `core/receipt#RECEIPT` case through the `@receipted(Redaction.STRUCTURAL)` weave draining `LayeredExport.contribute` off the fact-bearing owner `_emit` returns. `contribute` reads the threaded `LayerFact` off `self.fact` (never a re-run of an arm), re-mints the content key over `fact.data`, and folds the case off the `LayerEngine.preview` discriminant in one expression — the `SVG`/`ORA` named-document arms emit `ArtifactReceipt.Preview(key, fact.width, fact.height)` carrying the named-layer document's viewport (the same `Preview` shape `composition/compose#COMPOSE`/`graphic/marks/encode#MARK`/`graphic/raster/io#RASTER` contribute, the perceptual `scores` band defaulting empty), and the `PDF` arm emits `ArtifactReceipt.Egress(key, len(fact.data), fact.pages, 0, 0, fact.layers)` carrying the authored-layer count on the `overlays` slot (the natural counterpart to the `document/egress#FINISH` REWRITE arm reporting a STRIPPED-layer count on the same slot — author and subtract are inverses over one receipt field), the `encryption_r`/`outline_depth` slots zero because layering is neither a security nor a navigation close. Layered export adds NO new receipt case: the named-document facts are the `Preview` width/height shape and the OCG-PDF facts are the `Egress` byte/page/layer shape, both settled, the producer importing `ArtifactReceipt` and projecting flat scalars so the receipt owner imports no producer value object.
- Packages: `drawsvg` (`Drawing(width, height, origin, **svg_args)` carrying `xmlns:inkscape` on the root through `**svg_args`/`Drawing.append`/`Drawing.as_svg`, `Group(children, ordered_children, **args)` carrying `id`/`inkscape:groupmode`/`inkscape:label`/`style`/`data-color` through `**args`, `Raw(content, defs)` the verbatim-markup escape, MIT pure-Python `py3-none-any` `2.4.1` on the cp315 core, version via `importlib.metadata.version("drawsvg")`, the `cairoSVG` raster extra absent so this owner uses the SVG-string egress only); `pymupdf` (`open(stream=)`/`Document.new_page(width=, height=)`/`Document.add_ocg(name, config=-1, on=1, intent=None, usage=None) -> int`/`Document.set_layer(config, basestate=None, on=, off=, locked=)`/`Page.show_pdf_page(rect, docsrc, pno=0, oc=0) -> int`/`Document.tobytes(garbage=, deflate=)`/`Rect`, the `cp310-abi3` stable-ABI wheel `1.27.2.3` on the cp315 core, `AGPL-3.0-or-later OR Artifex-Commercial` the load-bearing pdf-rail licensing constraint recorded once here); `pikepdf` (`open`/`Pdf.Root`/`Pdf.save`/`Pdf.pages`/`Object.get`/`Dictionary`/`Array`/`Name`/`String` plus the `/OCProperties`/`/OCGs`/`/D`/`/Order`/`/Usage`/`/Name` tokens the enrichment names, the `cp314-abi3` stable-ABI wheel `10.9.1` (libqpdf `12.3.2`) on the cp315 core, `MPL-2.0` file-scoped copyleft); `pyvips` (`Image.new_from_buffer`/`hasalpha`/`addalpha`/`embed`/`composite(overlays, modes)`/`thumbnail_buffer`/`write_to_buffer` plus the `image * [1.0, 1.0, 1.0, opacity]` alpha-scale arithmetic, the `Extend.BACKGROUND` enum row and the 25-case `BlendMode` set reached as the `_vips_blend` nickname strings, libvips-backed `cffi` binding gated off the cp315 loader path on the host-native `libvips`, run on the gated `to_process` worker); `lxml` (`etree.Element`/`SubElement`/`tostring` the `stack.xml` author, libxml2-backed `python_version<'3.15'` gated, run on the gated worker); `stream-zip` (`stream_zip`/`ZIP_AUTO`/`NO_COMPRESSION_32` the OpenRaster ZIP framing, pure-Python `0.0.84` cp315-clean but run on the gated worker beside the raster arm); `msgspec` (`Struct` frozen `Layer`/`LayerPolicy`/`LayerFact`/`LayerEngine`/`LayeredExport`, `field` the policy default); `expression` (`tagged_union`/`tag`/`case` the `ExportFault` family, `Result`/`Ok`/`Error` the admission rail, `Block` the modal-arity carrier); `pydantic` (`TypeAdapter(ExportPayload)`/`ValidationError` the untrusted-`base` admission); `frozendict` (the `ENGINES`/`_PLACE`/`_INTENT`/`_USAGE`/`_STATE_KEY` tables and the `_VIPS_UNMAPPED` blend set); stdlib `copy.replace` (the fact thread), `collections.Counter` (the `of` layer-name uniqueness gate), `io.BytesIO` (the pikepdf in-memory re-open), `zlib.crc32` (the OpenRaster `mimetype` CRC); `anyio` (`to_process.run_sync`/`CapacityLimiter` — the gated-band worker seam threading the module-level `_GATE` bound, the same gated-band bound `export/indesign#INDESIGN`'s worker carries); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `receipts.Receipt`/`Redaction`/`receipted`).
- Growth: a new editable-export target (a `.psd`-native channel stack, a Scribus `.sla` profile) is one `ExportTarget` member plus one `LayerEngine` row plus one arm over the existing engine algebra — never a re-implemented SVG serializer, PDF object model, or ZIP container; a new layer attribute (an alpha-mask key, a clip-to-below flag, a per-layer color profile) is one field on the `Layer` row threaded into the SVG `Group` style, the OCG enrichment, and the `stack.xml` author, never a parallel attribute list; a new compositing mode is one `BlendMode` member, its value the SVG spelling, the `_ora_op` derivation the OpenRaster `svg:` cell, and the `_vips_blend` derivation the libvips merge nickname (or one `_VIPS_UNMAPPED` entry when `VipsBlendMode` lacks it); the four `/PageElement` roles and the print/export view-applications already span the OCG `category -> {stateKey: cell}` shape, so a further usage application (a CMYK-only print view) is one `LayerIntent` member plus one `_USAGE`/`_STATE_KEY` row — `_INTENT` is DERIVED over `LayerIntent` (only `DESIGN` is non-`View`), so the new member auto-derives its hint with no cell and `_usage`/`_enriched` untouched — only a Real-valued `/Zoom` min/max band, the one entry the uniform `Name`-emitting `_usage` cannot carry, would add a single `_usage` arm beside the `Layer` field holding the range; a new execution band (a free-threaded lane, a subinterpreter isolate) is one `Band` member plus one `_PLACE` row, every arm untouched; a new save knob is one `LayerPolicy` field; a new admission invariant (a name-uniqueness, a bbox-bounds check) is one `ExportFault` case plus one `of` guard; a new untrusted external blob is one `ExportPayload` band line plus one `of` admission read. Zero new surface.
- Boundary: a per-producer layer-export class family, a parallel `_ocg`/`_surgery` pair beside the one `PDF` arm, a `names`/`sources`/`flags` triple-list zipped at the call site beside the one `Layer` row, a hand-emitted `<g id="...">` string beside the `drawsvg` `Group`, a hand-written `/OC … BDC … EMC` content-stream string beside the `pymupdf` `oc=` placement that brackets the span natively, a multi-column `_BLEND` per-format blend table whose SVG column duplicates `blend.value` and whose PDF `/BM` column has no consumer (the OCG model gates visibility, not compositing) — distinct from the live `_vips_blend` derivation the faithful merge consumes, a `reduce(composite2, BlendMode.OVER)` flatten discarding every per-layer blend and `opacity` into an unfaithful `mergedimage.png`, a phantom `psdtags`/`tifffile` Photoshop-TIFF arm citing un-admitted packages, a stub `_psd_layer`/`_composite` body, an `if engine.gated` offload branch beside the `_PLACE` band table, a bare unbounded `to_process` offload trusting the per-loop process-limiter default beside the `_GATE`-bounded crossing, a per-instance `author` method beside the one modal-arity `exported` entrypoint, a `Layer.svg_style` returning a bare CSS string where `svg_attrs` carries the whole `id`/`inkscape:groupmode`/`inkscape:label`/`style`/`data-color` group dict, a plain `<g id=>` group Inkscape reads as a GROUP not a layer where the `inkscape:groupmode="layer"` idiom makes it a real layer, a `groups[layer.name]`-keyed parent lookup where `group` references a layer NAME instead of the `folders[group]` label folder, a silent duplicate-layer-name collapse where the `of` `Counter` gate rejects it into `ExportFault.duplicate`, an `_INTENT` row hand-enumerated per intent where the comprehension derives it, a manual `_emit`-side `Receipt.of` where the `@receipted` weave harvests `contribute`, and a `StrEnum`-plus-`dict[str, object]` erased-bag dispatch are the deleted forms; no UI, no live editor, no re-render, no re-layout. `drawsvg` owns programmatic named-`Group` SVG authoring; `pymupdf` owns the native OCG placement and `pikepdf` the `/OCProperties` catalog enrichment, meeting at PDF bytes; `pyvips`/`lxml`/`stream-zip` own the OpenRaster composite/manifest/container. Rasterization stays `graphic/raster/io#RASTER`/`graphic/vector#VECTOR`, placement stays the compose `Tile`/`ScaleFit`/`Overlay` arms, and the encoded raster layer sources for `ORA` arrive pre-rendered keyed by the same `ContentKey` — layered export binds each as a named layer and authors no placement, scaling, or rasterization. The inverse OCG-layer STRIP/FLATTEN stays `document/egress#FINISH`'s, the PDF/A archival close stays `document/emit#EMIT`'s, the PAdES cryptographic close stays `exchange/conformance#CONFORMANCE`'s, and the InDesign template-mutation hand-off stays `export/indesign#INDESIGN`'s. There is NO layer-faithful `.ai` writer: Illustrator's `.ai` is a PDF-compatible private container whose OCG layers do NOT map to the Illustrator layer panel, so an OCG-layered PDF renamed `.ai` opens FLAT — the only durable layer-faithful Illustrator hand-off is the `SVG` named-layer `<g>` document (its `id` the Illustrator layer name, its `inkscape:groupmode="layer"` the Inkscape recognition), so the `.ai` target is deliberately absent rather than mis-promised. The content key is consumed from runtime over the authored bytes, never re-minted off the source key.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections import Counter
from collections.abc import Awaitable, Callable, Iterable
from copy import replace
from enum import StrEnum
from io import BytesIO
from typing import TYPE_CHECKING, Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack

from anyio import CapacityLimiter, to_process
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, field
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.receipts import Receipt, Redaction, receipted

from artifacts.core.receipt import ArtifactReceipt

if TYPE_CHECKING:
    import pikepdf


# --- [TYPES] ----------------------------------------------------------------------------
class ExportTarget(StrEnum):
    SVG = "svg"  # drawsvg named-layer <g inkscape:groupmode=layer> document — Illustrator/Inkscape (core)
    PDF = "pdf"  # pymupdf OCG placement + pikepdf /Usage+/Order catalog enrichment — Acrobat (core)
    ORA = "ora"  # OpenRaster layered container (pyvips + lxml + stream-zip) — GIMP/Krita (gated)


class Band(StrEnum):
    CORE = "core"    # in-process on the cp315 core — drawsvg / pymupdf+pikepdf
    GATED = "gated"  # the to_process gated-band worker — pyvips + lxml + stream_zip


class BlendMode(StrEnum):  # the 16 CSS `mix-blend-mode` modes; the value IS the SVG token (`svg_attrs`), `_ora_op` derives the OpenRaster `svg:` op, `_vips_blend` the libvips composite nickname
    NORMAL = "normal"
    MULTIPLY = "multiply"
    SCREEN = "screen"
    OVERLAY = "overlay"
    DARKEN = "darken"
    LIGHTEN = "lighten"
    COLOR_DODGE = "color-dodge"
    COLOR_BURN = "color-burn"
    HARD_LIGHT = "hard-light"
    SOFT_LIGHT = "soft-light"
    DIFFERENCE = "difference"
    EXCLUSION = "exclusion"
    HUE = "hue"
    SATURATION = "saturation"
    COLOR = "color"
    LUMINOSITY = "luminosity"


class LayerIntent(StrEnum):  # the ISO 32000 OCG `/Usage` application + `/Intent` hint; `_INTENT` -> add_ocg /Intent, `_USAGE` -> the `_enriched` /Usage sub-dict
    VIEW = "view"      # always visible; default, no explicit /Usage
    PRINT = "print"    # print-only — /Print /PrintState /ON, /View /ViewState /OFF
    EXPORT = "export"  # export-only — /Export /ExportState /ON, /View /ViewState /OFF
    DESIGN = "design"  # design-time /Intent /Design processing hint
    BACKGROUND = "background"        # /PageElement /Subtype /BG — structural background plate
    HEADER_FOOTER = "header_footer"  # /PageElement /Subtype /HF — running header/footer furniture
    FOREGROUND = "foreground"        # /PageElement /Subtype /FG — foreground overlay plate
    LOGO = "logo"                    # /PageElement /Subtype /L — brand/logo mark


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class ExportFault:
    # the closed ADMISSION vocabulary `of` produces; arm-level provider raises (the cp315-core `pymupdf.FileDataError`/
    # drawsvg `ValueError`, the gated worker's `pyvips.Error`/`lxml.etree.LxmlError`, a `BrokenWorkerProcess`) convert
    # to the runtime `BoundaryFault` at the `async_boundary` capsule's `CLASSIFY` table, never into this interior vocabulary.
    tag: Literal["payload", "empty", "duplicate"] = tag()
    payload: tuple[str, ...] = case()    # the rejected `ExportPayload` key paths
    empty: None = case()                 # an empty layer set
    duplicate: tuple[str, ...] = case()  # layer names colliding across the by-`name` OCG match, the ORA `data/<name>.png` path, and the SVG group keying


# --- [CONSTANTS] ------------------------------------------------------------------------
_ORA_MIME: Final[bytes] = b"image/openraster"  # the OpenRaster magic, stored first and uncompressed in the ZIP
_THUMB: Final[int] = 256                        # the OpenRaster `Thumbnails/thumbnail.png` 256x256 long-edge bound
_INKSCAPE_NS: Final = "http://www.inkscape.org/namespaces/inkscape"  # the `inkscape:` namespace the `<g inkscape:groupmode=layer>` layer idiom declares on the root `<svg>`
# NORMAL plus the four non-separable HSL modes are absent from `VipsBlendMode`, so `_vips_blend` falls them
# to `over` in the FLATTENED preview while the layer `stack.xml` carries their full `svg:` op for the editor.
_VIPS_UNMAPPED: Final[frozenset[BlendMode]] = frozenset(
    {BlendMode.NORMAL, BlendMode.HUE, BlendMode.SATURATION, BlendMode.COLOR, BlendMode.LUMINOSITY}
)
# the pymupdf `add_ocg(intent=)` /Intent processing hint, DERIVED over the closed `LayerIntent`: only `DESIGN`
# is the design-time-only hint and every other intent is the default `View`, so a new intent auto-derives its
# hint with no row. The richer per-layer view-application AND the /PageElement structural marking are the
# `_enriched` /Usage sub-dict concern (`_USAGE`/`_STATE_KEY`), never this binary hint.
_INTENT: Final[frozendict[LayerIntent, str]] = frozendict(
    {intent: "Design" if intent is LayerIntent.DESIGN else "View" for intent in LayerIntent}
)
# the /Usage sub-dict policy keyed `category -> cell`; VIEW omits its dict (default visible), the PRINT/EXPORT
# rows ride the `state` cell and the /PageElement rows ride the `Subtype` cell, `_STATE_KEY` naming each
# category's inner key so `_usage` emits `/<Category> << /<StateKey> /<Cell> >>` uniformly with no per-kind arm.
_USAGE: Final[frozendict[LayerIntent, frozendict[str, str]]] = frozendict({
    LayerIntent.PRINT: frozendict({"Print": "ON", "View": "OFF"}),
    LayerIntent.EXPORT: frozendict({"Export": "ON", "View": "OFF"}),
    LayerIntent.DESIGN: frozendict({"View": "ON"}),
    LayerIntent.BACKGROUND: frozendict({"PageElement": "BG"}),
    LayerIntent.HEADER_FOOTER: frozendict({"PageElement": "HF"}),
    LayerIntent.FOREGROUND: frozendict({"PageElement": "FG"}),
    LayerIntent.LOGO: frozendict({"PageElement": "L"}),
})
_STATE_KEY: Final[frozendict[str, str]] = frozendict(
    {"View": "ViewState", "Print": "PrintState", "Export": "ExportState", "PageElement": "Subtype"}
)


# --- [MODELS] ---------------------------------------------------------------------------
class Layer(Struct, frozen=True):
    # `name`/`source`/`bbox` are the positional contract compose/encode/draw/sheet/imposition construct;
    # every richer editor-panel attribute defaults after `bbox` so a 3-arg construction stays valid.
    name: str
    source: bytes
    bbox: tuple[float, float, float, float]
    visible: bool = True
    locked: bool = False
    opacity: float = 1.0
    blend: BlendMode = BlendMode.NORMAL
    intent: LayerIntent = LayerIntent.VIEW
    group: str = ""    # folder label projected to all three editors: the SVG parent <g> the arm nests under, the OCG `/Order` folder title, and the ORA `<stack name=>` organizational folder; "" roots the layer
    color: str = ""    # editor layer-panel swatch, projected to the SVG <g> `data-color` (OCG/ORA carry no standard color slot); "" omits it

    def svg_attrs(self) -> dict[str, str]:
        # the SVG <g> attribute dict the `_svg` arm splats: `inkscape:groupmode="layer"`+`inkscape:label` make
        # Inkscape read the group AS a layer (a bare `<g id=>` is a GROUP, not a layer, in Inkscape) while `id`
        # carries the Illustrator layer name, the folded CSS `style` (`blend.value` IS the `mix-blend-mode` token,
        # so no per-format blend table), and the panel swatch rides a PRESERVED `data-color` data attribute — no
        # portable SVG layer-colour slot exists, so the swatch is machine-readable evidence, not an editor-panel
        # field a viewer renders. Each cell omitted when default so a plain layer writes the minimal group.
        style = ";".join(
            ([] if self.visible else ["display:none"])
            + ([f"opacity:{self.opacity:g}"] if self.opacity < 1.0 else [])
            + ([f"mix-blend-mode:{self.blend.value}"] if self.blend is not BlendMode.NORMAL else [])
        )
        return (
            {"id": self.name, "inkscape:groupmode": "layer", "inkscape:label": self.name}
            | ({"style": style} if style else {})
            | ({"data-color": self.color} if self.color else {})
        )


class LayerPolicy(Struct, frozen=True):
    # the trusted save-knob bundle (POLICY_VALUES: never a `garbage`/`deflate` flag tail on the signature)
    usage: str = "Artwork"  # the OCG /Usage category label pymupdf `add_ocg(usage=)` carries
    garbage: int = 3        # pymupdf `tobytes(garbage=)` xref compaction level
    deflate: bool = True


class LayerFact(Struct, frozen=True):
    # the bytes-plus-evidence carrier every arm returns; `contribute` reads it off the threaded owner
    data: bytes
    width: int = 0   # the SVG/ORA named-document viewport (the `Preview` facts)
    height: int = 0
    pages: int = 0   # the PDF page count (the `Egress` facts)
    layers: int = 0  # the authored-layer count, riding the `Egress` `overlays` slot


class LayerEngine(Struct, frozen=True):
    arm: Callable[["LayeredExport"], LayerFact]
    preview: bool = False    # True -> `ArtifactReceipt.Preview`; False -> `ArtifactReceipt.Egress`
    band: Band = Band.CORE   # the `_PLACE` placement lane: in-process core, or the gated `to_process` worker


# --- [BOUNDARIES] -----------------------------------------------------------------------
class ExportPayload(TypedDict, closed=True):
    base: NotRequired[ReadOnly[bytes]]  # the optional untrusted placed-layout PDF the `PDF` arm grafts onto


_PAYLOAD: Final = TypeAdapter(ExportPayload)


# --- [SERVICES] -------------------------------------------------------------------------
class LayeredExport(Struct, frozen=True):
    target: ExportTarget
    layers: tuple[Layer, ...]
    base: bytes = b""
    policy: LayerPolicy = field(default_factory=LayerPolicy)
    fact: LayerFact | None = None

    @property
    def output(self) -> bytes:
        return self.fact.data if self.fact is not None else b""

    @classmethod
    def of(
        cls,
        target: ExportTarget,
        layers: tuple[Layer, ...],
        /,
        *,
        policy: LayerPolicy = LayerPolicy(),
        **raw: Unpack[ExportPayload],
    ) -> Result[Self, "ExportFault"]:
        if not layers:
            return Error(ExportFault(empty=None))
        if collisions := tuple(name for name, n in Counter(layer.name for layer in layers).items() if n > 1):
            return Error(ExportFault(duplicate=collisions))  # the interior keys layers by `name`; a collision silently drops an OCG/ORA-file/SVG leaf
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(ExportFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        return Ok(cls(target=target, layers=layers, base=payload.get("base", b""), policy=policy))

    @receipted(Redaction.STRUCTURAL)  # the harvest weave drains `contribute` off the returned fact-bearing owner and emits via `Signals.emit_async`
    async def _emit(self) -> Self:
        engine = ENGINES[self.target]  # the thin pure core: one band-placed arm, one threaded fact
        return replace(self, fact=await _PLACE[engine.band](engine.arm, self))

    def contribute(self) -> Iterable[Receipt]:
        # the canonical `ReceiptContributor.contribute(self)` port — phase is the constant "emitted" the
        # `ArtifactReceipt` fixes by construction (KNOB_TEST); the `preview` discriminant folds the case.
        if (fact := self.fact) is None:
            return
        key = ContentIdentity.of(f"export-{self.target}", fact.data)
        recorded = (
            ArtifactReceipt.Preview(key, fact.width, fact.height)
            if ENGINES[self.target].preview
            else ArtifactReceipt.Egress(key, len(fact.data), fact.pages, 0, 0, fact.layers)
        )
        yield from recorded.contribute()


# --- [OPERATIONS] -----------------------------------------------------------------------
async def exported(plans: "LayeredExport | Iterable[LayeredExport]", /) -> RuntimeRail[Block[ContentKey]]:
    # the ONE modal-arity production entrypoint discriminating on the INPUT SHAPE — a lone export is
    # `Block.singleton`, an iterable `Block.of_seq`, normalized once at the head — over one runtime
    # `async_boundary` whose default `Exception` capture the runtime `CLASSIFY` routes, exactly as
    # `export/indesign#INDESIGN`'s `produced`, never a per-instance `author` method nor a `batch`/`mode` knob.
    block = Block.singleton(plans) if isinstance(plans, LayeredExport) else Block.of_seq(plans)
    return await async_boundary("export.layered", lambda: _exported(block))


async def _exported(block: "Block[LayeredExport]", /) -> Block[ContentKey]:
    authored = [await plan._emit() for plan in block]  # each `_emit` is `@receipted`, harvesting its own contribute
    return Block.of_seq([ContentIdentity.of(f"export-{plan.target}", plan.output) for plan in authored])


def _viewport(layers: tuple[Layer, ...], /) -> tuple[float, float]:
    return (
        max((layer.bbox[2] for layer in layers), default=0.0),
        max((layer.bbox[3] for layer in layers), default=0.0),
    )


def _ora_op(blend: BlendMode, /) -> str:
    # the OpenRaster `composite-op` for `stack.xml`: SVG-namespaced, `normal` mapping to the spec's `svg:src-over`.
    return "svg:src-over" if blend is BlendMode.NORMAL else f"svg:{blend.value}"


def _vips_blend(blend: BlendMode, /) -> str:
    # the libvips composite nickname for the FLATTENED `mergedimage.png`, derived off `blend.value`: the 11
    # separable CSS modes map by value (libvips spells the British `colour-dodge`/`colour-burn`), while the
    # `_VIPS_UNMAPPED` set (NORMAL + the 4 non-separable HSL modes absent from `VipsBlendMode`) falls to `over`.
    return "over" if blend in _VIPS_UNMAPPED else blend.value.replace("color", "colour")


def _svg(export: LayeredExport) -> LayerFact:
    import drawsvg

    width, height = _viewport(export.layers)
    drawing = drawsvg.Drawing(width, height, origin=(0.0, 0.0), **{"xmlns:inkscape": _INKSCAPE_NS})
    folders: dict[str, drawsvg.Group] = {}  # one `<g inkscape:groupmode=layer id=group>` sublayer-folder per distinct `group` label — the ORA `<stack name=>`/PDF `/Order` counterpart, NOT a parent-layer-name reference
    for layer in export.layers:
        leaf = drawsvg.Group(**layer.svg_attrs())
        leaf.append(drawsvg.Raw(layer.source.decode()))
        if layer.group and layer.group not in folders:
            folders[layer.group] = drawsvg.Group(**{"id": layer.group, "inkscape:groupmode": "layer", "inkscape:label": layer.group})
            drawing.append(folders[layer.group])
        folders.get(layer.group, drawing).append(leaf)  # nest under the group folder, else the root
    return LayerFact(drawing.as_svg().encode(), width=int(width), height=int(height), layers=len(export.layers))


def _pdf(export: LayeredExport) -> LayerFact:
    import pymupdf

    width, height = _viewport(export.layers)
    doc = pymupdf.open(stream=export.base, filetype="pdf") if export.base else pymupdf.open()
    page = doc[0] if export.base else doc.new_page(width=width, height=height)
    placed = []  # one `(layer, xref)` evidence stream; the visibility/lock partitions derive from it, never three co-mutated lists
    for layer in export.layers:
        xref = doc.add_ocg(layer.name, on=layer.visible, intent=_INTENT[layer.intent], usage=export.policy.usage)
        with pymupdf.open(stream=layer.source, filetype="pdf") as src:  # close each placed source once `show_pdf_page` copies it
            page.show_pdf_page(pymupdf.Rect(layer.bbox), src, 0, oc=xref)
        placed.append((layer, xref))
    doc.set_layer(
        0,
        on=[xref for layer, xref in placed if layer.visible],
        off=[xref for layer, xref in placed if not layer.visible],
        locked=[xref for layer, xref in placed if layer.locked],
    )
    return _enriched(doc.tobytes(garbage=export.policy.garbage, deflate=export.policy.deflate), export)


def _enriched(placed: bytes, export: LayeredExport) -> LayerFact:
    # the pikepdf catalog enrichment the pymupdf `add_ocg` placement cannot author: the per-layer `/Usage`
    # print/export view-application and the nested `/D/Order` folder tree, matched onto the placed OCGs by /Name.
    import pikepdf
    from pikepdf import Array, Name

    pdf = pikepdf.open(BytesIO(placed))
    ocprops = pdf.Root[Name.OCProperties]
    by_name = {str(ocg.get(Name.Name, "")): ocg for ocg in ocprops.get(Name.OCGs, Array())}
    for layer in export.layers:
        if layer.intent is not LayerIntent.VIEW and (ocg := by_name.get(layer.name)) is not None:
            ocg[Name.Usage] = _usage(layer.intent)
    ocprops[Name.D][Name("/Order")] = _order(export.layers, by_name)
    sink = BytesIO()
    pdf.save(sink)
    return LayerFact(sink.getvalue(), pages=len(pdf.pages), layers=len(export.layers))


def _usage(intent: LayerIntent) -> "pikepdf.Object":
    # the /Usage sub-dict — PRINT/EXPORT/DESIGN view-application OR /PageElement structural marking, both
    # `category -> {stateKey: cell}` rows — emitted uniformly; the nanobind `Dictionary` constructor coerces
    # keys to strings and rejects a `Name` key (`std::bad_cast`), so each `/Category /StateKey /Cell` rides subscript.
    from pikepdf import Dictionary, Name

    usage = Dictionary()
    for category, state in _USAGE[intent].items():
        entry = Dictionary()
        entry[Name("/" + _STATE_KEY[category])] = Name("/" + state)
        usage[Name("/" + category)] = entry
    return usage


def _order(layers: tuple[Layer, ...], ocgs: "dict[str, pikepdf.Object]") -> "pikepdf.Array":
    # the nested /Order: top-level layers as direct OCG refs, grouped layers folded into `[/GroupTitle, …]`.
    from pikepdf import Array, String

    grouped: dict[str, list[pikepdf.Object]] = {}
    direct: list[pikepdf.Object] = []
    for layer in layers:
        if (ref := ocgs.get(layer.name)) is not None:
            (grouped.setdefault(layer.group, []) if layer.group else direct).append(ref)
    return Array([*direct, *(Array([String(title), *members]) for title, members in grouped.items())])


def _ora(export: LayeredExport) -> LayerFact:
    # the OpenRaster layered container on the gated `to_process` worker: `pyvips` decodes each placed raster,
    # scales its alpha by `opacity`, and stacks the visible layers through the native `composite` (ONE call,
    # per-layer `BlendMode` via `_vips_blend`) so the flattened `mergedimage.png` is FAITHFUL to the layer
    # stack the editor re-composites from — never `reduce(composite2, OVER)` discarding every blend and
    # opacity; `lxml` authors `stack.xml` (top-to-bottom, each `composite-op` from `blend.value`); `stream_zip`
    # frames the ZIP with the `mimetype` stored first. Layer sources are RGB/RGBA per the raster-producer contract.
    import zlib
    from datetime import UTC, datetime

    import pyvips
    from lxml import etree
    from stream_zip import NO_COMPRESSION_32, ZIP_AUTO, stream_zip

    width, height = (int(extent) for extent in _viewport(export.layers))
    loaded = [(layer, pyvips.Image.new_from_buffer(layer.source, "")) for layer in export.layers]
    pngs = frozendict({layer.name: image.write_to_buffer(".png") for layer, image in loaded})
    visible = [(layer, image) for layer, image in loaded if layer.visible]
    placed = [
        (rgba * [1.0, 1.0, 1.0, layer.opacity] if layer.opacity < 1.0 else rgba).embed(
            int(layer.bbox[0]), int(layer.bbox[1]), width, height, extend=pyvips.Extend.BACKGROUND
        )
        for layer, image in visible
        if (rgba := image if image.hasalpha() else image.addalpha()) is not None  # the alpha-scale base bound once per layer
    ]
    modes = [_vips_blend(layer.blend) for layer, _ in visible]
    flattened = (
        placed[0].composite(placed[1:], modes[1:]) if len(placed) > 1  # bottom layer is the OVER base; the rest carry their mode
        else placed[0] if placed
        else None
    )
    merged = flattened.write_to_buffer(".png") if flattened is not None else b""
    thumb = pyvips.Image.thumbnail_buffer(merged, _THUMB, height=_THUMB).write_to_buffer(".png") if merged else b""
    root = etree.Element("image", version="0.0.3", w=str(width), h=str(height))
    stack = etree.SubElement(root, "stack")
    folders: dict[str, etree._Element] = {}  # one organizational `<stack name=group>` per distinct `group` — the ORA folder counterpart to the SVG nested `<g>` and the PDF `/Order` tree
    for layer in reversed(export.layers):  # OpenRaster lists the topmost layer first; the tuple is bottom-up paint order
        if layer.group and layer.group not in folders:
            folders[layer.group] = etree.SubElement(stack, "stack", name=layer.group)
        etree.SubElement(
            folders.get(layer.group, stack), "layer", name=layer.name, src=f"data/{layer.name}.png",
            x=str(int(layer.bbox[0])), y=str(int(layer.bbox[1])), opacity=f"{layer.opacity:g}",
            visibility="visible" if layer.visible else "hidden", **{"composite-op": _ora_op(layer.blend)},
        )
    manifest = etree.tostring(root, xml_declaration=True, encoding="UTF-8")
    now = datetime.now(UTC)
    members = (
        ("mimetype", now, 0o644, NO_COMPRESSION_32(len(_ORA_MIME), zlib.crc32(_ORA_MIME)), (_ORA_MIME,)),
        ("stack.xml", now, 0o644, ZIP_AUTO(len(manifest)), (manifest,)),
        *((f"data/{name}.png", now, 0o644, ZIP_AUTO(len(png)), (png,)) for name, png in pngs.items()),
        ("mergedimage.png", now, 0o644, ZIP_AUTO(len(merged)), (merged,)),
        ("Thumbnails/thumbnail.png", now, 0o644, ZIP_AUTO(len(thumb)), (thumb,)),
    )
    return LayerFact(b"".join(stream_zip(members)), width=width, height=height, layers=len(export.layers))


# --- [COMPOSITION] ----------------------------------------------------------------------
# the gated `ORA` offload threads the module-level `_GATE` `CapacityLimiter` so N concurrent `exported` calls
# share a fixed gated-band subprocess pool instead of fanning out at the per-loop process-limiter default — the
# bounded crossing `export/indesign#INDESIGN`'s IDML worker rides — and the runtime `async_boundary` default
# `Exception` capture plus its `CLASSIFY` table routes a worker death to `resource` and every other engine raise
# to `boundary`: the cp315 core can neither import nor name the gated worker's `pyvips.Error`/`lxml.etree.LxmlError`
# (neither a `ValueError`, and `import pyvips` itself raises with native libvips unprovisioned), so a hand-named
# `catch` tuple would silently miss the dominant `ORA` failure.
_GATE: Final[CapacityLimiter] = CapacityLimiter(4)  # the gated-band offload bound; the heavy libvips+lxml+zip `ORA` worker keeps it small, the same bound `export/indesign#INDESIGN`'s worker carries


async def _in_process(arm: Callable[["LayeredExport"], LayerFact], export: "LayeredExport", /) -> LayerFact:
    return arm(export)


async def _offloaded(arm: Callable[["LayeredExport"], LayerFact], export: "LayeredExport", /) -> LayerFact:
    return await to_process.run_sync(arm, export, limiter=_GATE)


_PLACE: Final[frozendict[Band, Callable[..., Awaitable[LayerFact]]]] = frozendict({
    Band.CORE: _in_process,
    Band.GATED: _offloaded,
})
ENGINES: Final[frozendict[ExportTarget, LayerEngine]] = frozendict({
    ExportTarget.SVG: LayerEngine(_svg, preview=True),
    ExportTarget.PDF: LayerEngine(_pdf),
    ExportTarget.ORA: LayerEngine(_ora, preview=True, band=Band.GATED),
})
```

## [03]-[RESEARCH]

- [POLICY_TABLE_DISPATCH] [RESOLVED]: the editor families collapse to one `ExportTarget` `StrEnum` keying the `ENGINES` `frozendict[ExportTarget, LayerEngine]` policy table — the inverse shape of the `document/egress#FINISH` `FINISHERS` table that STRIPS these layers. The prior page split Acrobat into a `PDF_OCG` placement arm and a `PDF_SURGERY` catalog arm; these collapse into ONE `PDF` arm where `pymupdf` places and gates the content (its `show_pdf_page(oc=)` brackets the `/OC … BDC … EMC` marked-content span natively) and `pikepdf` enriches the `/OCProperties` catalog with the `/Usage`/nested-`/Order` authoring `add_ocg` cannot reach — the two providers meeting at PDF bytes exactly as the pdf-rail catalogs prescribe, removing the prior `_surgery` arm's empty-`BDC … EMC` bracket (it marked nothing, the load-bearing illusory defect) and its un-confirmable `make_indirect` dependency.
- [BAND_AS_POLICY] [RESOLVED]: placement is a `Band` policy value keying the `_PLACE` table, so the cp315-core `SVG`/`PDF` arms and the gated-worker `ORA` arm share one `_emit` dispatch — the same band-as-policy law the runtime `STRATEGY`, concurrency `ARM`, and `graphic/raster/io#RASTER` `RasterEngine` tables carry, never an arm-side `if engine.gated` branch. The `ORA` arm crosses one `anyio.to_process.run_sync(arm, export, limiter=_GATE)` worker seam because libvips and the libxml2-backed `lxml` are off the cp315-core loader path, verified against the `pyvips` (`sdist-only, native libvips not provisioned on the cp315 band`) and `lxml` (`python_version<'3.15'`, no cp315 wheel) catalogues. The offload threads the module-level `_GATE` `CapacityLimiter` exactly as `export/indesign#INDESIGN`'s IDML worker threads ITS `_GATE` — `concurrency.md`'s `OFFLOAD_LANE` law mandates an explicit `CapacityLimiter` on every offload arm and REJECTS the unbounded `to_process` that falls to the per-loop process-limiter default, so a bare `to_process.run_sync(arm, export)` here is the deleted form, not the discipline; a worker death rides the runtime `async_boundary` `CLASSIFY` `resource` arm onto `BoundaryFault`.
- [ORA_LAYERS] [RESOLVED]: the OpenRaster `ORA` arm replaces the prior phantom `TIFF` arm that cited `psdtags`+`tifffile`, NEITHER admitted in `pyproject.toml` nor catalogued in any `.api` tier (a phantom-package illusion with stub `_psd_layer`/`_composite` bodies). OpenRaster is the admitted-backed raster-editor layered format: `pyvips.Image.new_from_buffer`/`hasalpha`/`addalpha`/`embed`/`composite(overlays, modes)`/`thumbnail_buffer`/`write_to_buffer` (catalogue rows [02]/[08]/[05]/[09]/[01], the `Extend.BACKGROUND` `[09]` enum row) plus the `image * [1.0, 1.0, 1.0, opacity]` alpha-scale arithmetic apply per-layer `opacity` and flatten the visible stack under each layer's own `_vips_blend` mode into a FAITHFUL `mergedimage.png` and the thumbnail — never the prior `reduce(composite2, BlendMode.OVER)` that discarded every per-layer blend and opacity into an unfaithful preview while the design carried both; `lxml.etree.Element`/`SubElement`/`tostring` (catalogue `[03]` rows) author `stack.xml`, and `stream_zip` with `NO_COMPRESSION_32`/`ZIP_AUTO` (catalogue `[03]` rows) frame the ZIP with the `mimetype` stored first as the format requires. GIMP, Krita, and MyPaint open the result with layers intact; Photoshop's native layered TIFF is deliberately absent because no admitted package authors the Photoshop image-resource block.
- [BLEND_DERIVED] [RESOLVED]: the prior `_BLEND` `frozendict[BlendMode, tuple[str, str, str]]` table is deleted. Its SVG column duplicated `blend.value` (the `BlendMode` StrEnum value IS the CSS `mix-blend-mode` token by construction), its PSD column served the phantom TIFF arm, and its PDF `/BM` column had no consumer because the OCG model gates VISIBILITY rather than compositing. `svg_attrs` reads `blend.value` directly as the `mix-blend-mode` token, `_ora_op` derives the OpenRaster `composite-op` (`svg:src-over` for `NORMAL`, `svg:<value>` otherwise), and the `PDF` arm honors visibility/intent/group through the OCG model — the PDF `/BM` PascalCase correspondence has no live consumer, so no per-format table survives. The libvips merge correspondence, by contrast, DOES have a live consumer — the faithful `mergedimage.png` — so `_vips_blend` derives the nickname off `blend.value` (the British `colour-dodge`/`colour-burn` spellings, the `_VIPS_UNMAPPED` NORMAL+HSL set absent from `VipsBlendMode` falling to `over`): a single derived column with a real consumer, not the multi-column duplicate table that was deleted.
- [ADMISSION_GATE] [RESOLVED]: admission is trusted layer material plus one OPTIONAL untrusted `base`. The `Layer` rows and `LayerPolicy` bundle are trusted (constructed by the visual-producer siblings); the untrusted external `base` PDF the `PDF` arm optionally grafts onto is admitted once at `LayeredExport.of` through the closed `ExportPayload` `TypedDict` + module-level `TypeAdapter`, a malformed payload rejected into `ExportFault.payload`, an empty layer set into `ExportFault.empty`, and a colliding layer-name set into `ExportFault.duplicate` before the fold runs. The `duplicate` gate is load-bearing, not defensive ceremony: every arm keys its layers by `name` — the `PDF` `_enriched` matches OCGs into a `by_name` dict, the `ORA` arm writes one `data/<name>.png` per name through a `frozendict` comprehension, and the SVG/ORA folders key by name — so a duplicate name silently collapses a layer's OCG `/Usage`, overwrites its raster, or drops its leaf; the `of` `Counter` gate rejects it once at admission so the interior fold is total over a unique-named set. Making the `base` optional (the `PDF` arm creates a fresh `new_page` sized to the viewport when none is supplied) removes the prior `_PREREQ` predicate table and the `ExportFault.incomplete` case: no arm REQUIRES the base, so all three arms are self-contained from their layer rows and the fault vocabulary is exactly the three causes `of` genuinely produces.
- [LAYER_ATTRIBUTES] [RESOLVED]: the `Layer` row owns the full editor-panel attribute axis projected per-format — `visible` (SVG `display:none`, PDF OCG on/off partition, ORA `visibility`), `locked` (PDF `set_layer(locked=)` and ORA editor hint), `opacity` (SVG `opacity:`, ORA layer `opacity` AND the alpha-scaled merge), `blend` over the 16-mode `BlendMode` vocabulary (SVG `mix-blend-mode`, ORA `composite-op` and the `_vips_blend` merge nickname), `intent` over the eight-member `LayerIntent` OCG vocabulary — the `VIEW`/`PRINT`/`EXPORT` view-applications, the `DESIGN` `/Intent` hint, and the `BACKGROUND`/`HEADER_FOOTER`/`FOREGROUND`/`LOGO` `/PageElement` structural roles (ISO 32000 OCG `/Usage` `/PageElement /Subtype /BG`/`/HF`/`/FG`/`/L`), all driving the `_INTENT`/`_USAGE` application (PDF only) — `group` for the SVG `<g inkscape:groupmode=layer id=group>` folder / OCG `/Order` folder / ORA `<stack name=>` folder (one folder per distinct `group` LABEL across all three arms — the SVG arm minting a folder per label like the ORA/PDF arms, NOT nesting under a layer that happens to share the label's name), and `color` projected to the SVG `<g>` `data-color` data attribute (the OCG and ORA formats carry no standard layer-color slot, and SVG has none either). Two SVG fidelity defects are closed here: a bare `<g id=>` is a GROUP in Inkscape, not a layer, so `svg_attrs` now emits the `inkscape:groupmode="layer"`+`inkscape:label` idiom (verified to serialize through `drawsvg.Group(**args)` with `xmlns:inkscape` on the root `Drawing`) so BOTH the Illustrator `id`-keyed and the Inkscape `groupmode`-keyed layer panels read the layers; and `data-color` is a PRESERVED machine-readable swatch, not an editor-panel field a viewer renders, because no portable SVG layer-colour standard exists — the prose no longer over-claims a panel reads it. The four `/PageElement` roles extend the prior four-member intent slice to the structural-classification half of the OCG usage domain the Acrobat editor reads, landing as pure `_USAGE`/`_STATE_KEY` rows (`_INTENT` is DERIVED over `LayerIntent`, so a new role needs no `_INTENT` cell) with `_usage`/`_enriched` untouched. `name`/`source`/`bbox` stay the positional construction contract `composition/compose#COMPOSE` (`Figure.layers`), `composition/sheet#SHEET`, `composition/imposition#IMPOSE`, `graphic/marks/encode#MARK`, and `visualization/diagram/draw#DRAW` use, every richer field defaulting after `bbox`.
- [RECEIPT_WEAVE] [RESOLVED]: receipt emission is the definition-time `@receipted(Redaction.STRUCTURAL)` aspect over the thin pure `_emit` (the same weave the egress/finish close carries), draining `LayeredExport.contribute` off the fact-bearing owner `_emit` returns. `_emit` threads the arm's `LayerFact` onto the frozen owner through `copy.replace` so `contribute` reads `self.fact` and folds the case off the `LayerEngine.preview` discriminant — the `SVG`/`ORA` arms `ArtifactReceipt.Preview(key, fact.width, fact.height)`, the `PDF` arm `ArtifactReceipt.Egress(key, len(fact.data), fact.pages, 0, 0, fact.layers)`, the authored-layer count riding the `overlays` slot the egress REWRITE arm reports a STRIPPED count on. No separate telemetry-span aspect rides the stack: the runtime `async_boundary` owns the span exactly as the `document/egress#FINISH` sibling composes it, so `@receipted` is the one definition-time weave.
- [BOUNDARY_CATCH] [RESOLVED]: `exported` rides the runtime `async_boundary("export.layered", …)` default `Exception` capture, exactly as the `export/indesign#INDESIGN`/`document/emit#EMIT` sibling producers do, NOT a hand-named `catch` tuple — because this owner is mixed-band and the gated `ORA` arm's provider raises are un-nameable on the core: `pyvips.Error` and `lxml.etree.LxmlError` are direct `Exception` subclasses (NEITHER a `ValueError`), and `import pyvips` itself raises on the cp315 core where native libvips is unprovisioned, so the worker's pickled raise cannot be re-imported and re-typed in the host — a `(ValueError, RuntimeError, BrokenWorkerProcess)` tuple would silently MISS the dominant `ORA` decode failure, the illusory-discipline trap of narrowing a `catch` to providers it cannot actually name. The runtime `reliability/faults#FAULT` owner carries the one conversion: `async_boundary(subject, thunk, *, catch=Exception)` defaults to the full `Exception` capture and its ordered `CLASSIFY` table routes `BrokenWorkerProcess` to `resource` and every other engine raise (the cp315-core `drawsvg`/`pymupdf`/`pikepdf` throws and the surfaced gated-worker raise alike) to the `boundary` catch-all onto `BoundaryFault`, where the runtime owns the cancellation re-raise and the non-engine-defect propagation a per-producer tuple cannot soundly partition across the band split.
- [MODAL_ARITY] [RESOLVED]: the prior per-instance `LayeredExport.author` returning `RuntimeRail[ContentKey]` violated MODAL_ARITY (one entrypoint owns every modality) and diverged from the freshest export sibling `export/indesign#INDESIGN`, whose module-level `produced(Idml | Iterable[Idml])` is the established artifacts-producer entry. It collapses into the module-level `exported(plans: LayeredExport | Iterable[LayeredExport])` discriminating on the INPUT SHAPE — `Block.singleton` for a lone export, `Block.of_seq` for an iterable, normalized once at the head — running `_exported` inside one `async_boundary` that awaits each plan's `@receipted` `_emit` (partial-progress receipts harvested per plan) and mints `ContentIdentity.of(f"export-{plan.target}", plan.output)`, returning `RuntimeRail[Block[ContentKey]]`. The `_emit`/`output`/`contribute` owner surface is unchanged; only the singular entry folds into the modal one, so a single export is `exported(one)` and a batch `exported(many)` over one boundary, never a `batch`/`mode` knob nor a per-plan boundary.
