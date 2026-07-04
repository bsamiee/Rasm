# DRAFT — lens: OUTPUT-FIRST (RASM-PY-ARTIFACTS)

Work BACKWARD from the three flagship deliverables to the structure that produces them: a full issued sheet SET, a portfolio AEC diagram SUITE (all six AEC `DiagramKind`s at published grade), and an editorial document PACKAGE (a spec-book section). The flagship-review clause (`[05]`) closes the campaign on a VISUAL judgment the seam ledger cannot substitute, so a library-default flagship reopens V13/V15 as residuals — which makes the output the design pressure, not the incidental result.

## THESIS

The corpus already holds the two hardest engines (`core/plan` = a realized `ArtifactWork`/CPM scheduler; `core/receipt` = the finished 22-case evidence family with every flagship case — `Pdf`/`Drawing`/`Schedule`/`Spec`/`Cad`/`Register`/`Transmittal`/`Diagram` — already present), but NOTHING FLOWS THROUGH THEM. The output-first inversion names the one structural fact every other lens under-weights: `ArtifactPipeline` is constructed by ZERO pages, so no flagship is CONSTRUCTIBLE. Every flagship is a graph of producer `emit()` nodes that a constructing owner folds into `ArtifactPipeline.of(...)` and drives through the runtime lane; that owner does not exist. The blueprint's spine is therefore (1) the corpus-wide `emit() -> ArtifactWork | Iterable[ArtifactWork]` convergence collapsing the six live carriers, (2) a NEW sole constructing owner `core/issue` whose fence builds the three flagship pipelines, and (3) the multi-artifact arity that lets a SET/SUITE emit per-member nodes while the composite (Transmittal/Register/the document package) is one aggregate node over member keys. On top of that spine the flagships demand V13 theme-as-data (no library default reaches print), V15 diagram machinery (solar/area/plan-geometry, not node-link wearing AEC labels), V9 scene drawing-production (massing/section linework), and the V3 typography seam (every glyph any flagship draws shaped once) — the output cannot read published-grade without all four, so they are structure, not polish.

## [A] THE THREE FLAGSHIP CONSTRUCTION CHAINS (backward derivation)

Each flagship is a producer-`emit()` graph; the terminal is the aggregate the composition root targets. This is the derivation the rest of the blueprint serves.

### [A1] Flagship 1 — the issued sheet SET

```text
graphic/style#REGIME (ScaleRatio/SheetId/Discipline/pen/linetype rows)  ─┐  wave 1
graphic/style#THEME (sheet-family title-block grid variant, type system) ─┤
graphic/vector/pattern#PATTERN (legend hatch geometry)                    ─┤
typography/shape + layout (title-block + general-note shaped runs)        ─┤
        │                                                                  │
        ▼ (composed, wave-1 substrate, no upward edge)                     │
drawing/{standard,dimension,annotate,symbol,detail,schedule}.emit()  ──────┤  wave 3
  → per-drawing ArtifactWork nodes (ArtifactReceipt.Drawing/Schedule)      │
        │                                                                  │
        ▼                                                                  │
composition/sheet.SheetSet.emit() → Iterable[ArtifactWork]  ───────────────┤  wave 2 struct / wave-1 emit
  ONE node per SHEET (cover, drawing-index, general-notes, drawing sheets) │
  each sheet node parents = its placed drawing/figure content keys         │
  (title-block grid = a graphic/style#THEME sheet-family row, NOT engine)  │
        │                                                                  │
        ▼                                                                  │
delivery/register.emit() → ArtifactWork (ArtifactReceipt.Register)  ───────┤  wave 3
  the drawing-index/COBie/spreadsheet, parents = the sheet keys            │
        │                                                                  │
        ▼                                                                  │
delivery/transmittal.emit() → Iterable[ArtifactWork]  ─────────────────────┘  wave 3
  member nodes (sheets + register + imposition + conformance + credential + archive)
  + ONE aggregate node: ArtifactReceipt.Transmittal, parents = member sheet keys
        │
        ▼
core/issue.issue(SheetSetIssue(...))  →  ArtifactPipeline.of(transmittal.emit(), targets={transmittal.key})
  .plan()  →  runtime LanePolicy.drain per front  →  RuntimeRail[ArtifactReceipt.Transmittal]
```

The load-bearing outputs this chain forces into structure: the `scheduled()`→compose→table BRIDGE (drawing-index sheet), the sheet←emit TYPESETTING seam (general-notes sheet), the cover-sheet PRODUCER (zero corpus concept today), the page-box law (MediaBox/TrimBox/BleedBox/CropBox), the `[05](b)` regime re-home (sheet reads `ScaleRatio`/`SheetId` from wave-1 `graphic/style`), and per-member elision (a re-issued set re-renders only the changed sheet — the arity law's whole point).

### [A2] Flagship 2 — the portfolio AEC diagram SUITE (six kinds)

```text
visualization/diagram/solar#SOLAR (pvlib SPA azimuth/altitude, sun-path furniture)  ─┐ wave 2
graphic/style#THEME (diagram aesthetics: gradient fills, blended transparency,        │
   silhouette entourage, art-directed annotation type)                                │
graphic/color/derive (CAM16 ramps, contrast/CVD/gamut — the ONLY palette source)      │
typography/{shape,math} (annotation type + formula, shaped once)                      │
scene/render#SCENE (V9 silhouette + GL2PS vectors → massing/section linework)  ───────┤
        │                                                                              │
        ▼ (data/tabular self-describing frames: plan polygons, adjacency, areas)       │
visualization/diagram/layout.emit() → ArtifactWork per kind (ArtifactReceipt.Diagram) │
  SUN_PATH: solar furniture over ProjectionKind.SOLAR_ARC (real sun geometry)         │
  STACKING/PROGRAM/SITE: AREA-proportional (bands/bubbles/parcels by true area)       │
  CIRCULATION/SECTION_CALLOUT: plan-ANCHORED (ingress coords as Constrained), no spring│
        │                                                                              │
        ▼                                                                              │
visualization/diagram/draw.emit() → Iterable[ArtifactWork]  ──────────────────────────┘
  ONE node per DiagramKind member (19 NodeShapes rendered, glyphs shaped via typography,
  layers projected into graphic/layer#LAYERPLAN, ink = derive contrast, theme = style row)
        │
        ▼
core/issue.issue(DiagramSuiteIssue(kinds={SUN_PATH,CIRCULATION,STACKING,PROGRAM,SITE,SECTION_CALLOUT}))
  →  ArtifactPipeline.of(draw.emit(), targets=<per-kind keys>)  →  lane drive  →  Block[ArtifactReceipt.Diagram]
```

The suite is the campaign's hardest VISUAL bar. It forces: the solar-ephemeris owner (real sun geometry, not caller-supplied rows), the AREA-law glyph case (`glyphset` sixth polygon-bearing mark), plan-geometry ingress (typed coordinate columns), the massing arm (V9 scene silhouette/GL2PS composed with diagram annotation), and V13 theme-as-data (switch office style = switch one row set). A node-link render wearing an AEC label fails the flagship review — this chain is why V15 is machinery, not labels.

### [A3] Flagship 3 — the editorial document PACKAGE (spec-book section)

```text
graphic/style#THEME (type system: type-scale ratios, per-role leading/tracking,        ─┐ wave 1
   variable-axis coords, OT-feature presets — tabular figures/small caps)               │
typography/{font,shape,layout,math} (shaped runs, Knuth-Plass, math notes, pyphen)      │
specification/classify (MasterFormat/OmniClass/Uniclass owned vocabularies)             │ wave 3
        │                                                                                │
        ▼                                                                                │
specification/section.emit() → ArtifactWork  (authors INTO document/model tree)  ───────┤  wave 3
        │  ArtifactReceipt.Spec (section/division/parts/articles/bytes)                  │
        ▼                                                                                │
document/model (the 11-variant DocumentNode tree — SectionNode/BlockNode/RunNode/Formula)│  wave 2 substrate
        │                                                                                │
        ▼                                                                                │
document/emit.emit() → ArtifactWork | Iterable[ArtifactWork]  ───────────────────────────┤  wave 2
  composes typography seams IN its arms (shaped placement, bidi, kerning — not reportlab │
  re-derived metrics); weasyprint @page/string-set section-aware running heads;          │
  cross-arm page-numbered TOC; FORMS first-class; absorbs core/format (TemplatePayload)  │
        │  ArtifactReceipt.Pdf/Office/Document                                           │
        ▼                                                                                │
document/egress.emit() → ArtifactWork (finishing: outline, page-box, PDF/A)  ────────────┘  wave 2
  composes imposition's press fold (not a clone)   ArtifactReceipt.Egress
        │
        ▼
core/issue.issue(DocumentPackageIssue(section_specs=...))
  →  ArtifactPipeline.of(<section + emit + egress nodes>, targets={egress.key})  →  lane drive
  →  RuntimeRail[ArtifactReceipt.Egress]   (a paginated, running-head, TOC'd, forms-capable spec book)
```

Forces: the V3 typography seam COMPOSED (emit's `ARCH:153/156` shape/layout edges are live in arms — the E7 defect), section-aware running heads (weasyprint `@page`/`string-set`), cross-arm page-numbered TOC, the pyphen orthographic-substitution emission (layout carries the channel, emit applies it at the break), and the `core/format` dissolution (its `TemplatePayload`/`DELEGATES` re-home into emit — the flagship needs the office/typst template-clone arms emit already holds).

## [B] THE `ArtifactWork` ENTRY CONTRACT AS LAW (the spine the flagships ride)

Verified on disk (`core/plan.md:89-108`), the node the corpus converges on — unchanged, already realized:

```python
class ArtifactWork(Struct, frozen=True):
    key: ContentKey                         # ContentIdentity.of(...) the producer's _emit already minted
    work: Work[ArtifactReceipt]             # the producer coroutine the plan schedules, never invokes
    parents: tuple[ContentKey, ...] = ()    # upstream content keys (a composite's parents = its member keys)
    admission: Admission = Admission(keyed=None)   # keyed | bare | retried(RetryClass)
    cost: float = 1.0                       # CPM forward-pass weight
    def lowered(self, /) -> Admit[ArtifactReceipt]: ...   # to the runtime lane Admit case
```

- ONE PRODUCER CONTRACT. Every producer page exposes `emit() -> ArtifactWork | Iterable[ArtifactWork]` (`ArtifactPipeline.of` already normalizes a lone node or any iterable, `plan.md:13`). The six live carriers (`derive() -> RuntimeRail[ColorReceipt]`, `managed/marks/io of() -> RuntimeRail[Block[Result[...]]]`, `font engineer() -> RuntimeRail[tuple[ContentKey, ArtifactReceipt]]`, `shape/layout run()/lay() -> RuntimeRail[ContentKey]`, `emit produced()`/`report rendered() -> RuntimeRail[Block[ContentKey]]`, `sheet/imposition/section of() -> RuntimeRail[ContentKey]`, `layout assign()`/`draw render()`/`scene render() -> RuntimeRail[tuple[..., ArtifactReceipt]]`) COLLAPSE to this one. The `export/layered.emit()`/`export/indesign.emit()` per-instance shape (`layered.md:256`, carrying no module-level batch entry) is the LANDED convergence proof; V6 generalizes it. The receipt carries its own key (`ArtifactReceipt.slot`).
- MULTI-ARTIFACT ARITY IS LAW. A sheet SET or diagram SUITE emits one node per member with per-member keys so elision stays per-member (a re-issued set re-renders only changed members — the reuse-fabric hit is per node). A COMPOSITE (Transmittal, Register, the document package) is ONE aggregate node whose `parents` are its member keys and whose receipt case carries the aggregate facts. `Register`/`Transmittal` are the standing proof: both already contribute aggregate cases (`receipt.md:14`). A composite producer's `emit()` returns the FULL node set (members + aggregate as an `Iterable[ArtifactWork]`, the aggregate's `parents` referencing the member keys), so `ArtifactPipeline.of(producer.emit(), targets=...)` schedules members-first-then-aggregate by topological order.
- ONE RECEIPT FAMILY. `ArtifactReceipt` (22 cases, verified complete `receipt.md:37-60`) stays the only rail. `derive`'s `ColorReceipt`/`ColorReceiptWire` collapses into it (derive becomes a wave-1 substrate consumed by every visual plane, minting no receipt of its own; `managed` contributes `Preview`). `ArtifactKind` derives from the case roster (the hand-synced `_KEYS` stays the DERIVED table `_facts` reads, single-edit-site per `DERIVED_LOGIC`; the stringly `Literal` is replaced by a roster-derived owner). Leg 1 fixes the roster; later legs consume, never re-open.
- THE CONSTRUCTING OWNER IS `core/issue` (NEW). Named explicitly per `[SEAM_AND_ENTRY_LAW]`. It is the SOLE fence that builds `ArtifactPipeline` from producer `emit()` nodes. `core/plan` stays the pure scheduler (it "owns no producer execution", `plan.md:18`); `core/issue` is the composition root that calls `terminal.emit()`, folds into `ArtifactPipeline.of(...)`, `.plan()`s it, drives the fronts through the runtime `LanePolicy.drain`, and returns the terminal aggregate receipt. Its polymorphic `issue(request)` discriminates the flagship shapes on a closed `IssueRequest` union — the arity law made a single entry.

```python
# core/issue.md skeleton (constructing owner, wave-1 / leg-1b spine)
@tagged_union(frozen=True)
class IssueRequest:                          # the flagship discriminant — arity law as one closed vocabulary
    tag: Literal["sheet_set", "diagram_suite", "document_package", "single"] = tag()
    sheet_set: "TransmittalRecord" = case()          # terminal = delivery/transmittal
    diagram_suite: tuple["DiagramLayout", ...] = case()   # terminal = visualization/diagram/draw, one per kind
    document_package: tuple["SpecSection", ...] = case()  # terminal chain = section -> emit -> egress
    single: "ArtifactWork" = case()                  # any lone producer node

class ArtifactIssue(Struct, frozen=True):
    lane: LanePolicy = _DEFAULT_LANE
    async def issue(self, request: IssueRequest, /) -> RuntimeRail[Block[ArtifactReceipt]]:
        # ONE entry: dispatch to the terminal producer's emit(), build the pipeline, drive it.
        nodes, targets = self._nodes(request)                    # terminal.emit() -> Iterable[ArtifactWork]
        plan = yield from ArtifactPipeline.of(nodes, targets=targets).plan   # core/plan schedules
        return (yield from self.lane.driven(plan.fronts, plan.cache_seed))   # runtime LanePolicy drains fronts
```

## [C] FINAL PAGE-SET TABLE

Schema per row: `path | action + engine-lowering | owner charter (tier: generate/bind/select or N/A) + skeleton | entry signature + receipt case | in/out seam edges | wave·leg`. `emit() -> ArtifactWork | Iterable[ArtifactWork]` abbreviated `emit()->Work`; substrate/vocabulary pages state `NONE`.

### FOUNDATIONS — leg 1 (1a = owner structure, 1b = entry contract + core/ + corpus sweeps)

| path | action + lowering | charter (tier) + skeleton | entry + receipt | in/out seams | wave·leg |
|---|---|---|---|---|---|
| `graphic/vector.md` | **SPLIT** → delete-with-absorb {into: `vector/algebra`, from: `vector.md`} | dissolves into the `vector/` folder (V1) | — | — | 1·1a |
| `graphic/vector/algebra.md` | **NEW** (kind new) | 2D path-algebra KERNEL (GENERATE): svgelements parse/query/affine/measure + metric arc-length point-at-distance (dimension ticks), polyline decimation/simplify, area-weighted centroid, `Length.to_mm` unit egress, tolerances lifted to a policy row. Owner `Vector`, `VectorOp`/`VectorResult`/`VectorFault` | NONE (substrate) | out: →composition, →drawing, →marks, →typography | 1·1a |
| `graphic/vector/boolean.md` | **NEW** (kind new) | pathops boolean/offset/stroke/warp/wind/region/contains spine (GENERATE). skia-pathops `addPath`/verb-level build for pattern clip paths | NONE | in: ←`vector/algebra`; out: →`vector/pattern`, →drawing/annotate (stroke-to-outline) | 1·1a |
| `graphic/vector/emit.md` | **NEW** (kind new) | serialization + raster egress (GENERATE): `drawsvg` `Drawing`/`Group`/`Raw` (kills every f-string SVG), resvg raster, `RenderPolicy`, text-on-path composing typography `on_path()` | NONE | in: ←`vector/algebra`,←typography/shape; out: →drawing, →export | 1·1a |
| `graphic/vector/pattern.md` | **NEW** (kind new) | V2 parametric fill generator (GENERATE): `StrokeFamily`/`PatternSpec`/`DensityLaw`; THREE lowerings — ezdxf `set_pattern_fill(definition=)`, `drawsvg.Pattern` tiles, pathops-clipped geometry. `HatchMaterial→PatternSpec` seed rows per ISO 128-50/ANSI/BS regime | NONE | in: ←`vector/boolean`,←`graphic/style`(regime); out: →drawing/schedule(legend), →export/dxf(hatch), →export/layered(fills) | 1·1a |
| `graphic/color/derive.md` | **REBUILD** (kind rebuild) | GENERATE: colour-science+ColorAide `Colorimetry`/`ColorOp`/`ColorModel`/`Ramp`; NOW owns `Palette`/`hex_ramp` (rehomed from `chart/spec`, V4); `ColorReceipt`/`ColorReceiptWire` rail DELETED (collapses into `ArtifactReceipt`); owns `AdaptMethod` (managed composes); CAM16/harmony/contrast/CVD/gamut the ONLY palette source | NONE (substrate — rail dies) | out: →visualization, →scene, →drawing, →`graphic/style`, →`color/managed` | 1·1a |
| `graphic/color/measure.md` | **NEW** (kind new; SPLIT from derive) | the 28-member `Metric` measurement concern + `_METRIC` policy table (V4 three-concern split); colour-science spectral-resample surfaces | NONE (substrate) | in: ←`color/derive` | 1·1a |
| `graphic/color/exchange.md` | **NEW** (kind new; SPLIT from derive) | CxF3 intake owner (V4): colour-cxf `read_cxf`/`write_cxf` over `cxf3.CxF`; `Spot` device measurement | NONE (substrate) | in: ←`color/derive`; out: →`color/managed`(SpotChannel) | 1·1a |
| `graphic/color/managed.md` | **REBUILD** (kind rebuild) | SELECT/egress: ICC/LUT/CCTF egress (pyvips `icc_transform` on lane); composes derive `AdaptMethod` (E6 dedupe); **AUTHORS Separation/DeviceN plates** (V4 plate lane over pikepdf raw object model, consuming `color/exchange` SpotChannel); TAC policy gate + overprint/rich-black rows | `emit()->Work` · `Preview` | in: ←`color/derive`,←`color/exchange`; out: →graphic/raster, →document | 1·1a |
| `graphic/raster/io.md` | **REBUILD** (kind rebuild) | V8 re-partition: `Raster` owner + decode/codec-policy + working-surface ops; **exports bare `render_png`/`montage`** (E5 media home); pyvips `block_untrusted_set` hardening + `Source`/`Target` streaming; consolidates `RasterFact` (canonical here); tifffile GeoTIFF/`aszarr`/`memmap`/`TiffSequence`/`validate_jhove`. `scikit-image` census blocker named | `emit()->Work` · `Preview` | in: ←`raster/process`,←`raster/measure`,←`exchange/detect`(re-homed, [05]d); out: →media, →composition | 1·1a |
| `graphic/raster/process.md` | **KEEP** (kind improve) | 97-member produced-raster engine (award-grade); pillow ImageChops/ImageMath/gradients; V16 rebind only | NONE (substrate) | in: — ; out: →`raster/io` | 1·1a |
| `graphic/raster/measure.md` | **KEEP** (kind improve) | 42-member measurement engine; **exports `frame_similarity`** (E5); V16 rebind, `[RESEARCH]` fold | NONE (substrate) | in: — ; out: →`raster/io`, →media/analysis, →core/receipt(`Preview`) | 1·1a |
| `graphic/marks/vocabulary.md` | **NEW** (kind new) | V8 cycle-break neutral site: `Symbology`/`DecodeSource`/`MarkFault` the shared vocabulary encode+decode both import | NONE (vocabulary) | out: →`marks/encode`,→`marks/decode` | 1·1a |
| `graphic/marks/encode.md` | **REBUILD** (kind rebuild) | GENERATE (mark geometry over vector): segno/barcode/zxing; imports `marks/vocabulary` (NOT decode) — cycle broken; `RasterFact` consolidated to `raster/io` | `emit()->Work` · `Preview` | in: ←`marks/vocabulary`,←`raster/io`; out: →export | 1·1a |
| `graphic/marks/decode.md` | **REBUILD** (kind rebuild) | decode inverse (zxing `read_barcodes`); imports `marks/vocabulary` (NOT encode) — cycle broken | NONE (substrate) | in: ←`marks/vocabulary`,←`raster/io` | 1·1a |
| `graphic/style.md` | **NEW** (kind new) | V13 design-language + `[05](b)` regime re-home. TWO row families in ONE deep owner: **BIND** (the regime vocabulary re-homed from `drawing/standard` — `ScaleRatio`/`SheetId`/`SheetType`/`Discipline`/`LineType`/`LineWeight`/`TextHeight`/`Terminator`/`HatchMaterial`/`LetteringStyle`/`LayerName` + material→pattern, discipline→pen bind rows) and **SELECT** (theme rows: color system via derive, stroke hierarchies, composition grids, the TYPE SYSTEM — type-scale ratios/leading/tracking/variable-axis coords/OT-feature presets, the SHEET FAMILY — title-block grid variants/margin-zone/page-master rows). A theme row references regime rows by key; a regime row references geometry/color shapes by value | NONE (substrate) | in: ←`color/derive`,←`vector/pattern`,←typography; out: →composition/sheet([05]b resolved),→document/emit,→drawing/standard,→visualization | 1·1a |
| `graphic/layer.md` | **NEW** (kind new) | V14 `LayerPlan` owner (foundational, `[05](c)` re-home): the semantic layer TREE (bounded top-level roster, discipline/kind/z-order rows, ISO 13567 names on AEC / editorial names elsewhere, nested groups); projection targets PSD group-folders / PDF OCG `/Order` / SVG `<g>` / IDML / layered TIFF/ORA | NONE (vocabulary) | out: →composition/{compose,imposition,sheet},→visualization/diagram/draw,→marks/encode,→drawing,→export/layered(composes, never owns) | 1·1a |
| `typography/font.md` | **KEEP** (kind improve) | the receipt-law exemplar (font subset/instance/axis/outline/freeze); V16 rebind; `engineer()`→`emit()` convergence | `emit()->Work` · `Document` | out: →typography/shape,→document | 1·1a |
| `typography/shape.md` | **REBUILD** (kind rebuild) | GENERATE (sole text engine): uharfbuzz shaping/bidi/itemize/fallback + COLRv1; **exports `shaped_rgba`** (subtitle phantom fixed); fallback-chain RESOLVER (fontTools `unicodedata.script()` itemization); font-METRICS surface (cap-height/x-height via `hb_ot_metrics`) | `emit()->Work` · `Document` | in: ←font; out: →document,→composition,→`vector/emit`,→drawing,→diagram,→media/subtitle,→typography/math | 1·1a |
| `typography/layout.md` | **KEEP** (kind improve) | Knuth-Plass break correctness; carries the pyphen `(change,index,cut)` orthographic channel (emit applies it); vertical (ttb) stacking; V16 rebind | `emit()->Work` · `Document` | in: ←shape; out: →document,→drawing/annotate | 1·1a |
| `typography/math.md` | **NEW** (kind new) | V3 math owner (GENERATE): the ziamath concern the 7 dual-engine pages hand-build; composes shape's outline surface + uharfbuzz math (`Face.has_math_data`/`get_math_constant`/`get_math_glyph_variants`/`get_math_glyph_assembly`); ziafont/ziamath enter typography manifest | NONE (substrate) | in: ←shape; out: →drawing/{annotate,dimension},→document/model,→diagram/draw | 1·1a |
| `core/plan.md` | **KEEP** (kind improve) | the realized `ArtifactPipeline`/CPM scheduler (9/10). Rename `content_identity`→`identity`; purge `[RESEARCH]`; stays the pure scheduler | NONE (scheduler) | in: ←every producer(`Work` thunk),←runtime; out: →runtime lane | 1·1b |
| `core/receipt.md` | **KEEP** (kind improve) | the 22-case `ArtifactReceipt` family (complete). Derive `ArtifactKind` from the case roster; RULE the band-vs-table frozendict split (see [D]); freeze the roster for leg 1 | NONE (substrate) | in: ←`exchange/conformance`(`Verdict`, [05]a frozen); out: →core/plan,→runtime | 1·1b |
| `core/issue.md` | **NEW** (kind new) | **the sole constructing owner** (V6/E1): polymorphic `issue(IssueRequest)` builds `ArtifactPipeline.of(terminal.emit(), targets=...)`, drives fronts via runtime `LanePolicy`. Flagship constructions: sheet-set / diagram-suite / document-package / single | `issue(request) -> RuntimeRail[Block[ArtifactReceipt]]` (composition root, not a producer) | in: ←delivery/transmittal,←visualization/diagram/draw,←document/egress,←core/plan,←runtime lane | 1·1b |
| `core/format.md` | **DELETE** → delete-with-absorb {into: `document/emit`, from: `core/format`} | V6 dissolution: `TemplatePayload` admission + `bound()` fan-out + `DELEGATES` re-home into `document/emit`; `_fanned` second scheduler dies. Condemned-intact until the leg-2 absorb | — | — | (absorb executes 2·2) |

### MID PLANE — leg 2

| path | action + lowering | charter (tier) + skeleton | entry + receipt | in/out seams | wave·leg |
|---|---|---|---|---|---|
| `visualization/chart/spec.md` | **REBUILD** (kind rebuild) | typed MARKS/CHANNELS/TRANSFORMS/COMPOSITION algebra over altair (raw `Vega(dict)` case DIES); imports `graphic/color/derive` for palette (V4); `@theme.register` chart-theme rows are `graphic/style` LOWERINGs | NONE (substrate) | in: ←`color/derive`,←`graphic/style`; out: →chart/export | 2·2 |
| `visualization/chart/export.md` | **REBUILD** (kind rebuild; MERGE transform in) | host-free render dispatch (vl-convert/lets-plot); ABSORBS `transform.md` (vegafusion pre-pass); vl-convert `register_font_directory` font-identity loop (chart glyphs match document shaped text); lets-plot charter narrows to a consumed-capability row (census blocker named) | `emit()->Work` · `Chart` | in: ←chart/spec,←`color/derive`; out: →core/plan | 2·2 |
| `visualization/chart/transform.md` | **MERGE** → delete-with-absorb {into: `chart/export`, from: `chart/transform`} | V10; delete the stray `</content>` EOF tag on absorb | — | — | 2·2 |
| `visualization/table.md` | **REBUILD** (kind rebuild) | `GT(frame.to_pandas())` DEFAULT floor (E5 fix; polars `.style` demoted to probed); pagination/continuation-across-sheets (AEC schedule pressure); kiwisolver column-width solving; units-sub-rows; self-describing `data/tabular` ingress (source/unit/identifier/content-key columns decode by name) | `emit()->Work` · `Table` | in: ←`data/tabular`,←`color/derive`; out: →drawing/schedule,→delivery/register,→composition/sheet | 2·2 |
| `visualization/diagram/glyphset.md` | **REBUILD** (kind rebuild) | dead-carrier purge (consume/delete `EndCap`/`SubLayout`/`TextRun`/`Port.at`/`corner`); **ADD the AREA/polygon glyph case** (V15 — the sixth mark room/parcel/footprint the five-mark closure cannot express) | NONE (vocabulary) | out: →diagram/{layout,draw} | 2·2 |
| `visualization/diagram/solar.md` | **NEW** (kind new) | V15 solar-ephemeris owner (GENERATE): **pvlib** SPA azimuth/altitude from date/latitude, sunrise/transit/solstice, numpy-vectorized sampling + sun-path FURNITURE (horizon circle, altitude rings, compass ticks, labeled date arcs) over `ProjectionKind.SOLAR_ARC`; owned closed-form kernel as fallback. Distinct from geometry `Sunpath` (AGPL, energy plane) | NONE (substrate) | in: — ; out: →diagram/layout | 2·2 |
| `visualization/diagram/layout.md` | **REBUILD** (kind rebuild) | V15 machinery: plan-geometry INGRESS (typed coordinate columns, not `attrs`); AREA law (STACKING/PROGRAM/SITE area-proportional; CIRCULATION/SECTION_CALLOUT plan-anchored `Constrained`, spring defaults DIE); typed ELK layout-option vocabulary; composes `solar` for SUN_PATH. grandalf kept (parity + sole spline router) until fast-sugiyama parity | `emit()->Work` · `Diagram` | in: ←`data/tabular`(GRAPH),←diagram/solar,←diagram/glyphset; out: →diagram/draw | 2·2 |
| `visualization/diagram/draw.md` | **REBUILD** (kind rebuild) | the diagram-suite TERMINAL: renders ALL 19 `NodeShape`s (12 dead/type-error fixed); routes label outlining + math through `typography/{shape,math}` (V3, no local ziafont/ziamath); `_INK`→derive contrast (V4); reads `GlyphStyle.text` via `graphic/style` type system (V13); projects layers into `graphic/layer`; `.drawio` round-trip (drawpyo `load_diagram`). SUITE emits one node per kind | `emit() -> Iterable[ArtifactWork]` · `Diagram` | in: ←diagram/layout,←typography,←`color/derive`,←`graphic/style`,←`graphic/layer`,←scene(massing egress); out: →export,→core/issue(suite) | 2·2 |
| `visualization/diagram/schematic.md` | **NEW** (kind new) | V10 schematic owner: the 226-element `schemdraw.elements` catalog (logic/flow/dsp, Kmap/Timing/BitField) the five marks cannot express (`svgconfig.text='path'`). `drawing/symbol` keeps only the `Segment*`/`ElementCompound` spine | `emit()->Work` · `Diagram` | in: ←typography; out: →export,→core/plan | 2·2 |
| `scene/render.md` | **REBUILD** (kind rebuild) | V9 drawing-production: standard-view family (plan/elevation/section/axo/iso — `view_xy`/`view_isometric`); silhouette/feature-edge/hidden-line `FieldFilter` case (`vtkPolyDataSilhouette`/`vtkFeatureEdges`/`enable_hidden_line_removal`); directional SUN policy (date/latitude light, not parameterless `enable_shadows`); GL2PS 3D→vector; typed mesh ingress (geometry `mesh`/data `MeshPayload`, not `grid: object`). `vtk`/`usd-core` census blockers named; catalogs authored | `emit()->Work` · `Scene` | in: ←geometry/mesh(typed),←`color/derive`; out: →media,→scene/export,→diagram/draw(massing),→drawing/detail(section linework) | 2·2 |
| `scene/stage.md` | **KEEP** (kind improve) | USD/USDZ authoring owner (collapse premise REFUTED — 488-LOC deep owner); V16 rebind | NONE (substrate; contributes via render Export) | in: ←render | 2·2 |
| `scene/export.md` | **REBUILD** (kind rebuild) | V9 cycle break: HOIST `SceneTarget` (breaks render⇄export cycle); ADD GL2PS vector target; composes `package/archive` reproducible-ZIP (zlib-ng re-entry — no `zlib_ng.compressobj` hand-reach) | `emit()->Work` · `Scene` | in: ←render,←stage,←package/archive; out: →geometry/mesh(boundary) | 2·2 |
| `composition/compose.md` | **KEEP** (kind improve) | post-render figure placement; limiter→runtime `lanes.offload`; entry convergence; imports `graphic/layer` (LayerPlan) | `emit()->Work` · `Preview` | in: ←`vector/*`,←`graphic/layer`; out: →export | 2·2 |
| `composition/imposition.md` | **REBUILD** (kind rebuild) | E6/V7 press-fold OWNER: the OCG-bind mechanism (`_mint_groups`/`_draw_one`/`_configure_layers`) becomes the composable owner sheet+egress import; pdfimpose schemes; complete the Map migration | `emit()->Work` · `Egress`/`Pdf` | in: ←`graphic/layer`; out: →document,→sheet,→egress(compose),→delivery/transmittal | 2·2 |
| `composition/sheet.md` | **REBUILD** (kind rebuild) | flagship-1 sheet producer (V3+V7+V13+[05]b): composes `typography` shaped runs (no Helvetica magic); ISO 216-derived `_SIZES`; 2D ISO 7200 title-block GRID; **cover-sheet producer** (new concept); drawing-index sheet via `scheduled()`→compose→table BRIDGE; general-notes sheet via sheet←emit TYPESETTING seam; page-box law (MediaBox/TrimBox/BleedBox/CropBox); title-block grid VARIANTS = `graphic/style` sheet-family rows; imports `graphic/style` regime (ScaleRatio/SheetId — [05]b resolved); composes imposition's press fold. **`SheetSet` emits ONE node per sheet** | `emit() -> Iterable[ArtifactWork]` · `Pdf`/`Egress`/`Preview` | in: ←`graphic/style`(regime+theme),←typography,←`graphic/layer`,←drawing,←imposition,←visualization/table; out: →export,→delivery/register,→delivery/transmittal | 2·2 |
| `document/model.md` | **KEEP** (kind improve) | the 11-variant `DocumentNode` tree (excellent substrate); fix `:15` "ten-variant" stale count; V16 rebind | NONE (substrate) | in: ←specification/section; out: →emit,→tagged,→data/tabular | 2·2 |
| `document/emit.md` | **REBUILD** (kind rebuild; ABSORB core/format) | flagship-3 lowering axis (E1+E7+E8+V12): composes `typography/{shape,layout,math}` seams IN its arms (ARCH:153/156 live — no reportlab re-derived metrics/dropped bidi); ~48-field `EmitSpec`→grouped value objects; weasyprint `@page`/`string-set` running heads; cross-arm page-numbered TOC; FORMS first-class (`FieldNode` per-target lowering); applies pyphen orthographic substitution at breaks; **absorbs `core/format`** (`TemplatePayload`/`DELEGATES`/docxtpl/PPTX template-clone); `produced` batch rail dies; dedup `Pdf.from_html` | `emit() -> ArtifactWork \| Iterable[ArtifactWork]` · `Pdf`/`Office`/`Document` | in: ←model,←typography,←`graphic/style`(type system),←`color/managed`; out: →egress,→conformance,→core/issue(package) | 2·2 |
| `document/report.md` | **REBUILD** (kind rebuild) | E1+E6+V12: `rendered` batch rail dies; terminal PDF routes through emit (duplicate `Pdf.from_html` root dies); page-numbered TOC; jinja/papermill/nbclient compose arms | `emit()->Work` · `Report` | in: ←model,←emit; out: →core/plan | 2·2 |
| `document/egress.md` | **KEEP** (kind improve) | E6/V7: composes imposition's saddle-stitch algebra (NOT the clone; `[EGRESS_DISTINCT]` false claim removed); finishing close (encrypt/outline/watermark/page-box); complete Map migration | `emit()->Work` · `Egress` | in: ←imposition(press fold),←emit; out: →core/issue(package) | 2·2 |
| `document/lens.md` | **KEEP** (kind improve) | V12: OCR pre-flight gate (`ocrmypdf` `pdfinfo.PdfInfo.has_text` skip-OCR) + in-package PDF/A egress (`pdfa.speculative_pdfa_conversion`→veraPDF oracle); `recover()`→`emit()` convergence | `emit()->Work` · `Introspection` | in: ←model; out: →data/tabular | 2·2 |
| `document/tagged.md` | **KEEP** (kind improve) | PDF/UA marked-content (sound, grade 9); V16 rebind; entry convergence | `emit()->Work` · `Egress`/`Pdf` | in: ←model; out: →exchange/conformance | 2·2 |

### AEC / EGRESS — leg 3 (media/exchange/package COLD — emit + limiter/stamina landed in leg-1 reconcile)

| path | action + lowering | charter (tier) + skeleton | entry + receipt | in/out seams | wave·leg |
|---|---|---|---|---|---|
| `drawing/standard.md` | **REBUILD** (kind rebuild) | BIND lowering only ([05]b): the ezdxf symbol-table lowering (`Standard.of`/`seed`/`dimstyle`) composing `graphic/style` regime vocabulary (the closed enums MOVED to style); binds regime→ezdxf resources | NONE (substrate) | in: ←`graphic/style`(regime),←`vector/pattern`; out: →drawing/*,→export/dxf(seed) | 3·3 |
| `drawing/dimension.md` | **REBUILD** (kind rebuild) | V3+V5+E3: text via `typography` shaped positions (no ziafont); ISO 129-1 terminators via `symbol` mark owner; ISO 286 fits + ISO 1101 GD&T frames/datums + dual-unit DIMALT | `emit()->Work` · `Drawing` | in: ←standard(dimstyle),←`vector/algebra`,←symbol,←typography; out: →composition,→export,→core/receipt | 3·3 |
| `drawing/annotate.md` | **REBUILD** (kind rebuild) | V3+V5+E3: consumes `PositionedGlyphRun` POSITIONS (no re-shape); welding (ISO 2553)+surface-texture (ISO 1302)+datum symbols; revision clouds via `vector/boolean` stroke-to-outline; palette via derive (E2/E7 fixed) | `emit()->Work` · `Drawing` | in: ←standard,←symbol,←typography/{shape,layout},←`vector/boolean`,←`color/derive`; out: →export,→composition | 3·3 |
| `drawing/symbol.md` | **REBUILD** (kind rebuild) | V5 mark-geometry OWNER (GENERATE): ONE parametric owner for ISO 129-1 terminators + north-arrow/scale-bar/grid-bubble/section-marker + revision triangles/clouds; proportion rows feed BOTH SVG+DXF lowerings (kills the three-mechanism triad + SVG/DXF magic duplication); composes `vector` Matrix/point algebra (no hand trig) | `emit()->Work` · `Drawing` | in: ←`vector/{algebra,boolean}`; out: →drawing/{annotate,detail},→composition/sheet | 3·3 |
| `drawing/detail.md` | **REBUILD** (kind rebuild) | E9+V9: `SheetId`-typed refs (no stringly `sheet: str`); build-once rustworkx DAG (cross-reference closure); section/elevation callouts re-scoped to V9 scene-extracted figures (`block: bytes` origin stated) | `emit()->Work` · `Drawing` | in: ←symbol,←`graphic/style`(SheetId),←scene/render(linework); out: →composition | 3·3 |
| `drawing/schedule.md` | **REBUILD** (kind rebuild) | E5+V2: `Reshape`-lowered schedules; legend swatches via `vector/pattern` (no invented hex/crosshatch trig); the polars render fixed (table pandas floor, leg-2 landed); QTO frames self-describing | `emit()->Work` · `Schedule` | in: ←standard,←`vector/pattern`,←visualization/table,←`data/tabular`; out: →composition,→core/receipt | 3·3 |
| `specification/section.md` | **KEEP** (kind improve) | CSI SectionFormat 3-part into `document/model`; full §5.1.7 Schematron; composes `classify` `ClassCode` | `emit()->Work` · `Spec` | in: ←classify; out: →document/model,→visualization/table,→core/issue(package) | 3·3 |
| `specification/classify.md` | **KEEP** (kind improve) | MasterFormat/UniFormat/OmniClass + Uniclass 2015 element rows (kills register's parallel `ClassificationSystem`); crosswalk + ReferenceIndex | NONE (substrate) | in: ←`graphic/style`(Discipline); out: →section,→register | 3·3 |
| `delivery/register.md` | **REBUILD** (kind rebuild) | E5+E6+E9: composes `classify` `ClassCode` + `graphic/style` `Discipline` (parallel `ClassificationSystem` DIES); blocked `Stub(group=)` fixed (table pandas floor); rustworkx DAG assembly-ordering + cross-reference closure; `code`/`discipline` typed | `emit()->Work` · `Register` | in: ←composition/sheet(SheetSet.registered),←classify,←visualization/table; out: →delivery/transmittal | 3·3 |
| `delivery/transmittal.md` | **REBUILD** (kind rebuild) | flagship-1 TERMINAL (V6 aggregate): the ISO 19650 issue orchestrator composing imposition+archive+conformance+credential+register; `emit()` returns the FULL node set (member sheets + register + the aggregate `Transmittal` node whose `parents` = member sheet keys); ledger the `package/codec` edge (E7) | `emit() -> Iterable[ArtifactWork]` · `Transmittal` | in: ←register,←composition/imposition,←package/archive,←exchange/{conformance,credential}; out: →core/issue(sheet_set) | 3·3 |
| `export/dxf.md` | **REBUILD** (kind rebuild) | V11: paperspace/viewport authoring (sheets without layouts are not CAD deliverables); standard→dxf seam REAL (composes `Standard.seed`, not a fresh doc); entity completion (ATTRIB/Image/MPolygon/underlay); `import_blocks`; by-NAME version/units (E9); `of()+contribute()`→`emit()` convergence | `emit()->Work` · `Cad` | in: ←drawing/standard(seed),←`vector/*`,←composition/sheet; out: →geospatial(GeoJSON) | 3·3 |
| `export/layered.md` | **REBUILD** (kind rebuild) | V14: composes `graphic/layer` `LayerPlan` tree (flat `Layer` rows widen to the tree; layered COMPOSES, never owns); PSD group-folders/OCG `/Order`/SVG `<g>`/IDML/ORA; 12 native PSD blend modes (psd-tools author; PhotoshopAPI census blocker named); explicit Illustrator lane = layered-PDF + organized-SVG pair | `emit()->Work` · `Preview`/`Egress` | in: ←`graphic/layer`,←composition; out: →core/plan | 3·3 |
| `export/indesign.md` | **KEEP** (kind improve) | SimpleIDML IDML template-mutation; composes `graphic/layer`; the E1 convergence proof holds | `emit()->Work` · `Office` | in: ←`graphic/layer`,←composition; out: →core/plan | 3·3 |
| `exchange/conformance.md` | **KEEP** (kind improve) | PAdES sign/stamp/reserve/audit; the `ConformanceVerdict` FROZEN as a wave-1-consumed contract (`[05]a` — `core/receipt` imports it acyclically); transient `@stamina.retry`→runtime `guarded` ORACLE row (veraPDF/JHOVE) | `emit()->Work` · `Verdict` | in: ←document; out: →core/receipt(`Verdict`),→delivery/transmittal | 3·3 |
| `exchange/credential.md` | **KEEP** (kind improve) | c2pa Sign/Read; sanctioned transient retry→runtime `guarded` | `emit()->Work` · `Credential` | in: ←runtime; out: →core/receipt,→Rasm.Persistence(content-key),→delivery/transmittal | 3·3 |
| `exchange/detect.md` | **KEEP** (kind improve) | `[05]d`: format-ID substrate (puremagic default + python-magic fallback); the `Detect`/`DetectEngine`/`DetectIdentity`/`Source` vocabulary re-homed BELOW raster (raster/io consumes it within-wave — inversion resolved); `_WORKER_RETRY`→runtime `offload(retry=OCCT)` | NONE (substrate) | out: →graphic/raster/io,→document | 3·3 |
| `exchange/metadata.md` | **KEEP** (kind improve) | EXIF/IPTC/XMP/ICC over the `MetaCarrier` axis; **pyexiv2 in-process arm RE-SITED behind the process boundary** (`pyexiftool` out-of-process owns the carrier; GPL-3.0-in-process removed) or dropped; iptcinfo3/xmp-toolkit stay superseded (zero consumers) | `emit()->Work` · `Metadata` | in: — ; out: →core/receipt | 3·3 |
| `media/container.md` | **KEEP** (cold; kind improve) | av container spine; `_WORKER_RETRY`→runtime `offload(retry=OCCT)` (leg-1 reconcile); entry convergence | `emit()->Work` · `Media` | in: ←scene,←filtergraph; out: →core/receipt | 3·3 |
| `media/analysis.md` | **REBUILD** (kind rebuild) | E5: the three phantom imports fixed (`render_png`/`montage` from `raster/io`, `frame_similarity` from `raster/measure` — now real exports); waveform/spectrogram/loudness | `emit()->Work` · `Media` | in: ←container,←audio,←filtergraph,←`raster/{io,measure}`; out: →compute/analysis | 3·3 |
| `media/audio.md` | **KEEP** (cold) | av audio encode/mux/resample; entry convergence | `emit()->Work` · `Media` | in: ←container,←filtergraph | 3·3 |
| `media/subtitle.md` | **REBUILD** (kind rebuild) | E3: `shaped_rgba` phantom fixed (now real `typography/shape` export); pysubs2 parse/convert/burn-in | `emit()->Work` · `Media` | in: ←container,←filtergraph,←typography/shape | 3·3 |
| `media/filtergraph.md` | **KEEP** (cold) | closed FilterNode owner (E7 REFUTED — no derive coupling); av capability routing | NONE (substrate) | out: →media/* | 3·3 |
| `media/synthesis.md` | **KEEP** (cold) | numpy oscillator/noise→audio encode; entry convergence | `emit()->Work` · `Media` | in: ←audio; out: →compute/analysis | 3·3 |
| `media/timeline.md` | **KEEP** (cold) | non-linear editing (multi-parent clip DAG — the strongest CPM exemplar); entry convergence | `emit()->Work` · `Media` | in: ←container,←filtergraph,←audio | 3·3 |
| `package/vocabulary.md` | **NEW** (kind new) | E10 cycle-break neutral site: `Bundle`/`CompressionAlgo`/`CodecProfile` the shared vocabulary archive+delta import (breaking codec⇄archive⇄delta) | NONE (vocabulary) | out: →package/{codec,archive,delta} | 3·3 |
| `package/codec.md` | **REBUILD** (kind rebuild) | E10 cycle break: imports `package/vocabulary` (stops eager-importing archive/delta privates; codec→archive/delta one-directional); zlib-ng RE-ENTRY ([DATA]→[ARTIFACTS] re-tag + `.api` overlay, beside lz4/brotli/zstandard) | `emit()->Work` · `Bundle` | in: ←`package/vocabulary`,←runtime; out: →core/receipt | 3·3 |
| `package/archive.md` | **KEEP** (cold + growth) | py7zr/stream-zip; the multi-entry `unpack`/`list`/`BundleManifest` inverse (carried growth); imports `package/vocabulary` | `emit()->Work` · `Bundle` | in: ←`package/vocabulary`,←codec | 3·3 |
| `package/delta.md` | **KEEP** (cold + growth) | detools; the parent-keyed delta row (carried growth); imports `package/vocabulary` | `emit()->Work` · `Bundle` | in: ←`package/vocabulary`,←codec,←runtime | 3·3 |

**Page count:** 71 live pages (60 start − 3 deleted {`vector.md`, `core/format.md`, `chart/transform.md`} + 14 new) + 3 delete-with-absorb rows = **74 rows**. New pages (14): `vector/{algebra,boolean,emit,pattern}`, `color/{measure,exchange}`, `marks/vocabulary`, `graphic/style`, `graphic/layer`, `typography/math`, `core/issue`, `diagram/{schematic,solar}`, `package/vocabulary`.

## [D] THE V16(b) FROZENDICT RULING (band-vs-table — the hinge every draft must rule)

`api-census` proves on the repo's own interpreter (CPython 3.15.0b3, PEP 814) that `from builtins import frozendict` SUCCEEDS — the doctrine (`language.md` FROZENDICT_TABLE_SITE, `shapes.md` OWNER_INDEX[08]) canonicalizes `frozendict` as THE immutable-map form and uses it in every snippet. So V16(b)'s stated rationale ("not a builtin / every import raises") is REFUTED; the migration ACTION survives ONLY on `[04]` SHARED-TIER-LAW consistency (the `expression.Map` ADT/dispatch spine the sibling campaigns forced). Ruling, split by role:

- **DISPATCH/POLICY TABLES** (`Final[frozendict[K, V]]` the code READS by key — `_KEYS`, `_SIZES`, `_HATCH`, `_KIND_POLICY`, the many `Final[frozendict[...]]` tables) migrate to `Final[Map[...]]` (`Map.of_seq`). This is the `rg`-zero acceptance surface, re-grounded as "builtin-dict table vs `expression.Map` ADT rail," never "broken vs working import." `core/plan.md` already exemplifies the target state (prose-only frozendict mention, no import).
- **MSGSPEC `case()` PAYLOAD BANDS STAY `frozendict`** — the ruled EXCEPTION. `ArtifactReceipt` carries `frozendict[str, float|str]` INSIDE case tuples (`preview`/`media`/`scene` `receipt.md:78-86`) and `frozendict[str, int]` (`cad`); these are msgspec-encodable hashable payloads the encoder serializes as maps. `expression.Map` is NOT msgspec-native, so a band rebind would break the wire. The bands stay `frozendict` (the codec-native immutable map, doctrine-sanctioned per `shapes.md` OWNER_INDEX[08] "tuple, frozendict, Map"). `receipt`/`managed`/`measure`/`io` carry these bands and are exempt on the band fields only; their dispatch tables still migrate.

Acceptance is therefore mechanical BUT scoped: `rg 'from builtins import frozendict'` returns zero (every table migrated to `Map`), while `frozendict[...]` type annotations survive ONLY as msgspec `case()` payload band fields (documented in the receipt/managed/measure/io Boundary prose as the codec-native exception). `content_identity`→`identity` (69/45/44) and `[RESEARCH]` purge (54 pages) are unconditional, landed in leg-1's reconcile.

## [E] CORRECTED SEAM LEDGER (every cross-domain edge; acyclic; four inversions disposed)

Delta from `ARCHITECTURE.md:98-222` (the full ledger is re-emitted at build; the load-bearing corrections):

- **[05](a) receipt←conformance INVERSION → FREEZE.** `core/receipt.md:33` imports `exchange/conformance.ConformanceVerdict` (wave-1←wave-3). RULING: FREEZE `ConformanceVerdict` as a wave-1-consumed contract the wave-3 cold pass may not alter (the single acyclic value-object edge; conformance imports nothing back). The ledger records `core/receipt ← exchange/conformance [VERDICT frozen]`; leg-1 consumes the frozen shape, leg-3 conformance verifies it unchanged.
- **[05](b) sheet←standard INVERSION → RE-HOME.** `composition/sheet.md:48` imports `drawing/standard.ScaleRatio`/`SheetId` (wave-2←wave-3), and V3/V13 pull more regime rows earlier. RULING: the regime VOCABULARY (closed enums + identity value objects + bind rows) RE-HOMES into the foundational `graphic/style.md` (wave 1, BIND row family). `drawing/standard` keeps only the ezdxf LOWERING (wave 3, composing `graphic/style` regime). New edges: `composition/sheet ← graphic/style [REGIME]`, `drawing/standard ← graphic/style [REGIME]`, `specification/classify ← graphic/style [DISCIPLINE]` — all within-wave-or-earlier. Deleted: the `drawing/standard → graphic/color/derive [DERIVE]` edge (ARCH:119) is subsumed by regime→derive within `graphic/style`.
- **[05](c) export/layered.Layer upward fan → RE-HOME.** Six pre-egress projectors import `export/layered.Layer` (wave-2/3←wave-3). RULING: `LayerPlan` re-homes to foundational `graphic/layer.md` (wave 1). Edges flip: `composition/{compose,imposition,sheet} ← graphic/layer`, `visualization/diagram/draw ← graphic/layer`, `graphic/marks/encode ← graphic/layer`, `drawing/* ← graphic/layer`, and `export/layered ← graphic/layer` (composes, never owns). All within-wave-or-earlier.
- **[05](d) raster/io←exchange/detect INVERSION → RE-HOME.** `graphic/raster/io.md:49` imports `exchange/detect` (wave-1←wave-3). RULING: the detect VOCABULARY (`Detect`/`DetectEngine`/`DetectIdentity`/`Source`) is ingress substrate; RE-HOME it below raster so `graphic/raster/io ← exchange/detect` becomes within-wave (detect vocabulary moves to a wave-1-consumable position; the ezdxf/libmagic ENGINE arm stays in `exchange/detect` wave-3). Edge: `graphic/raster/io ← exchange/detect [DETECT vocab, wave-1 frozen]`.
- **CYCLE breaks (3 census + confirmed):** marks encode⇄decode → both import `graphic/marks/vocabulary` (`Symbology`/`DecodeSource`); `RasterFact`→`raster/io` canonical (dup carrier collapsed). scene render⇄export → hoist `SceneTarget` out of render (export imports it, render does not). package codec⇄archive⇄delta → `Bundle`/`CompressionAlgo`/`CodecProfile`→`package/vocabulary`; codec→archive/delta one-directional.
- **Unledgered edges LEDGERED:** `visualization/diagram/draw ← visualization/chart/spec` DELETED (palette rehomes to derive; `draw ← graphic/color/derive [PALETTE]` replaces it); `drawing/{annotate,detail,schedule,symbol} ← visualization/chart/spec` all DELETED (→`← graphic/color/derive`); `delivery/transmittal ← package/codec [CODEC]` LEDGERED (was omitted); the ARCH:103-105 derive `[DERIVE]` edges become REAL (derive is now imported). E7 `filtergraph.md:14` derive-coupling DROPPED (refuted — no such import exists).
- **New flagship edges:** `core/issue ← delivery/transmittal`, `core/issue ← visualization/diagram/draw`, `core/issue ← document/egress`, `core/issue ← core/plan` (the constructing owner's fan-in to terminal producers + the scheduler); `composition/sheet → document/emit [NOTES]` (the general-notes typesetting seam, was prose-only `:851`); `composition/sheet → visualization/table [INDEX]` (the drawing-index bridge, was prose-only); `visualization/diagram/draw ← scene/render [MASSING]` (V15 massing egress); `drawing/detail ← scene/render [LINEWORK]` (V9 section/elevation).

The whole page-level import graph is acyclic; every ledger edge points within-wave or to an earlier wave (proof: leg-1 owners import only leg-1/runtime; leg-2 imports leg-1/leg-2/runtime; leg-3 imports any earlier + runtime; `core/issue` in leg-1b imports terminal `emit()` shapes whose NODE contract is leg-1-landed even where the terminal's internal render deepens in leg-2/3).

## [F] PACKAGE ROSTER DELTA + RECONCILIATION (integration-first; every disposition ruled)

Closure is INTEGRATION at the owning surface, never removal; the only sanctioned removals are the reconciliation rows, each ruled by its brief-stated condition on evidence.

| package | disposition | owning surface + `.api` obligation |
|---|---|---|
| `pvlib` | **ADMIT** (feed-verified: BSD-3, NREL, pure-Python wheel, no census exposure) | `visualization/diagram/solar.md` (V15); `.api/pvlib.md` authored with the `solarposition` member set at admission; discharges the recorded proof burden (owned closed-form kernel stands as fallback) |
| `zlib-ng` | **RE-ENTER** ([DATA]→[ARTIFACTS] re-tag) | `package/codec.md` compression band beside lz4/brotli/zstandard (3 live consumers); author `libs/python/artifacts/.api/zlib-ng.md` folder overlay (shared-tier catalog exists); `scene/export` composes archive's ZIP path (no hand-reach) |
| `drawsvg` | INTEGRATE (only `.Pattern` unmined) | `graphic/vector/pattern.md` (V2 SVG lowering) + `graphic/vector/emit.md` (`Drawing`/`Group`/`Raw` replacing f-string XML) |
| `schemdraw` | INTEGRATE (1 consumer, catalog unmined) | NEW `visualization/diagram/schematic.md` (226-element catalog); `drawing/symbol` keeps the `Segment*`/`ElementCompound` spine |
| `tifffile` | INTEGRATE (1 consumer) | `graphic/raster/io.md` GeoTIFF + `aszarr`/`memmap`/`TiffSequence`; `validate_jhove`→`exchange/conformance` oracle family (V8) |
| `ocrmypdf` | INTEGRATE (only `ocr()` mined) | `document/lens.md` pre-flight (`pdfinfo.PdfInfo.has_text`) + PDF/A egress (`pdfa.speculative_pdfa_conversion`→veraPDF) |
| `rustworkx` | INTEGRATE (DAG surface under-consumed) | `delivery/register`+`transmittal`+`specification/classify` fold `TopologicalSorter`/`transitive_reduction`/`ancestors`/`descendants` for assembly ordering + cross-reference closure |
| `vl-convert-python` | INTEGRATE (`register_font_directory` unwired) | `visualization/chart/export.md` font-identity loop (chart glyphs match document shaped text) |
| `drawpyo` | INTEGRATE (`load_diagram` round-trip unconsumed) | `visualization/diagram/draw.md` `.drawio` template round-trip |
| `pikepdf` | INTEGRATE (only read-side plate flags cataloged) | `graphic/color/managed.md` Separation/DeviceN plate AUTHORING (V4); `.api/pikepdf.md` authoring surface authored with the verdict; `composition/sheet` page-box attrs |
| `weasyprint` | INTEGRATE (running-content absent from catalog) | `document/emit.md` `@page`/`string-set`/`target-counter`; `.api/weasyprint.md` authored with the verdict (V12) |
| `vtk`/`pyvista` | INTEGRATE (silhouette/GL2PS absent both catalogs) | `scene/render.md` (V9); author `Silhouette`/`FeatureEdges`/`GL2PS` into `.api/vtk.md`+`.api/pyvista.md` with the verdict |
| `pymupdf` | INTEGRATE (trimbox/bleedbox absent) | `composition/sheet.md` page-box law; `.api/pymupdf.md` `trimbox`/`bleedbox` authored with the verdict (V7) |
| `uharfbuzz` | INTEGRATE (math surface unmined) | `typography/math.md` (`has_math_data`/`get_math_constant`/glyph-variants/assembly) + `typography/shape.md` metrics (`hb_ot_metrics`) |
| `pyphen` | INTEGRATE (channel carried, unapplied) | `document/emit.md` applies the `(change,index,cut)` orthographic substitution at the break (`.api/pyphen.md:79`) |
| `pyexiv2` | **RE-SITE or REMOVE** (survival UNMET) | `exchange/metadata.md`: in-process GPL-3.0 arm (`metadata.md:46`) re-sited behind the process boundary (pyexiftool owns the out-of-process carrier) or dropped — no capability pyexiftool cannot reach; census substantiates removal-default |
| `iptcinfo3` | **REMOVE** (SUPERSEDED, zero consumers) | prose-mentioned, not imported; the `pyexiftool`/`pyexiv2` fold owns the carrier |
| `python-xmp-toolkit` (`libxmp`) | **REMOVE** (SUPERSEDED, zero consumers) | prose-mentioned, not imported |
| `grandalf` | **KEEP until parity** (copyleft GPL-2.0/EPL-1.0, 2023-stale) | LIVE parity oracle + SOLE spline router (`layout.md:_grandalf_router`); removal gated on `fast-sugiyama` parity AND the SPLINES route re-home — a real routing ripple, never a zero-consumer strike |
| `scikit-image` | KEEP (census blocker) | `graphic/raster/{process,measure}` (V8); marker-drop blocker named on every process/measure claim (cp315 wheel pending) |
| `PhotoshopAPI` | KEEP-gated (structurally dead: no sdist) | native-writer lane rides the Forge source-build follow-up; `psd-tools` standing author for V14 blend modes until it lands |
| `PyICU` | KEEP-gated (structurally dead: sdist + native ICU) | `typography/{shape,layout}`; `uniseg`+`python-bidi` the standing charter while the wheel gate holds |
| `usd-core`/`lets-plot` | KEEP (census blockers) | `scene/stage` / `visualization/chart/export`; marker-drop blockers named (cp315 wheel-lag) |
| `pdf_oxide` vs `pdfplumber`/`pypdf`/`pypdfium2` | RATIONALIZE per concern | `document/{emit,lens,egress,tagged}`; `pdf_oxide` cp315-abi3-viable; keep per-concern parity |

Central manifest: all changes in root `pyproject.toml`; `.api` stubs per admission (`pvlib`, `zlib-ng` overlay); both `.api` tiers stack.

## [G] VERDICT DISPOSITION (V1–V16, all ruled)

- **V1** vector→folder: `graphic/vector/{algebra,boolean,emit,pattern}` (4 pages); metric point-at-distance/decimation/centroid/unit egress land in `algebra`; tolerances→policy row; `drawsvg` emitter in `emit`. [1a]
- **V2** pattern plane: `graphic/vector/pattern.md` `StrokeFamily`/`PatternSpec`/`DensityLaw`, three lowerings; consumed by schedule/dxf/layered. [1a]
- **V3** typography completion: `typography/math.md` NEW (ziamath owner); shape exports `shaped_rgba` + metrics surface + fallback resolver; consumers rewire (dimension/annotate/symbol/draw/emit/sheet); SHAPE_QA golden oracle proves equivalence. [1a→consumers 2/3]
- **V4** color rehoming: Palette/hex_ramp→derive; ColorReceipt collapsed; derive split into derive/measure/exchange (3 pages); AdaptMethod deduped; plate authoring in managed; no literal hex corpus-wide. [1a]
- **V5** drafting-mark geometry: `drawing/symbol` the ONE parametric owner; proportion rows feed SVG+DXF; composes vector algebra. [3]
- **V6** entry realization: `emit()->ArtifactWork` corpus-wide (leg-1 reconcile); `core/issue` the constructing owner; `core/format` dissolves into emit; batch rails die. [1b]
- **V7** press-fold ownership: `composition/imposition` OWNS the fold; sheet+egress compose; ISO 216 sizes; ISO 7200 grid; SheetSet ISSUE completion (cover/index/notes); page-box law. [2]
- **V8** raster/media seams: `raster/io` re-partition; render_png/montage/frame_similarity real exports; pyvips hardening/streaming; marks cycle→`marks/vocabulary`. [1a]
- **V9** scene drawing bridge: **REALIZED** (flagship-2 massing + flagship-1 detail linework demand it) — standard views/silhouette/GL2PS/sun policy/typed mesh ingress; catalogs authored; render⇄export cycle broken; detail callouts + massing consume scene egress. [2]
- **V10** visualization repairs: table pandas floor + pagination + kiwisolver + units; chart typed grammar (Vega dict dies) + transform merge; diagram 19-shape parity + dead carriers + `schematic.md` + typed ELK; lets-plot charter narrowed. [2]
- **V11** dxf depth: paperspace/viewports; standard→dxf seam real; entity completion; import_blocks; by-NAME derivation. [3]
- **V12** document plane: emit composes typography seams + grouped value objects + weasyprint parity + cross-arm TOC + forms; model folds re-home; report routes through emit; lens OCR pre-flight + PDF/A. [2]
- **V13** design language: `graphic/style.md` NEW (theme+regime, foundational); type system + sheet family; zero library-default output; six AEC DiagramKinds reach published grade as theme DATA. [1a→consumers 2/3]
- **V14** layer taxonomy: `graphic/layer.md` NEW `LayerPlan` tree (foundational); every layered exporter projects INTO it; layered composes, never owns; PSD blend modes. [1a]
- **V15** diagram machinery: `visualization/diagram/solar.md` NEW (pvlib); glyphset area case; layout plan-geometry ingress + area law + plan-anchored placement; massing = V9 scene egress. [2]
- **V16** track rebind: (a) content_identity→identity (69/45/44); (b) frozendict→Map for TABLES, msgspec bands stay frozendict (see [D]); (c) [RESEARCH] purge (54 pages); + limiter/stamina collapse (43 mints + 55 stamina, ~10x the named census). All in leg-1 reconcile. [1b]

## [H] EVIDENCE DISPOSITION (E1–E13, all disposed)

- **E1** entry fiction → `emit()->ArtifactWork` corpus-wide + `core/issue` constructs `ArtifactPipeline` (V6). The six carriers named in [B]. layered/indesign the landed proof.
- **E2** color orphan → Palette/hex_ramp→derive; 4 drawing importers + chart/spec + scene rewire; ColorReceipt collapsed; ARCH:103-105 edges become real (V4).
- **E3** dual text engine → `typography/math.md` owner; shaped positions consumed (not re-shaped); `shaped_rgba` exported; standard fontTools leak→shape metrics surface; 7 pages rewire (V3).
- **E4** hatch instances → `graphic/vector/pattern.md` generator; `HatchMaterial→PatternSpec` seed rows; no ACAD magic names; schedule swatches via pattern (V2).
- **E5** broken paths → table `GT(to_pandas())` floor (schedule/register verified); `render_png`/`montage`/`frame_similarity` real exports (media/analysis fixed) (V8/V10).
- **E6** duplication → imposition OWNS the press fold; emit dedups `Pdf.from_html`; egress composes imposition's saddle algebra; `RasterFact`/`AdaptMethod` collapsed; register composes classify (V7/V4).
- **E7** unwired seams → emit composes shape/layout in arms; annotate imports ledgered (→derive); transmittal→codec ledgered; filtergraph derive-coupling DROPPED (refuted); ARCH:122 vector-outline phantom removed.
- **E8** hardcoding → tolerances→policy rows (vector); Helvetica magic→typography (sheet); EmitSpec→value objects; diagram dims→theme rows; `_INK`→derive contrast; ELK strategies→typed vocabulary.
- **E9** dead/stringly → glyphset dead carriers consumed/deleted + area case; detail `SheetId`-typed; register `ClassCode`/`Discipline` typed; receipt `ArtifactKind` derived from cases; dxf by-NAME.
- **E10** ledger drift → the corrected ledger [E]; model "ten-variant"→eleven; package cycle ledgered+broken; derive edges real.
- **E11** scene seams → render⇄export cycle broken (SceneTarget hoist); stage KEEP (collapse premise refuted); silhouette/feature-edge/GL2PS catalogs authored (V9).
- **E12** output-grade gaps → V13 themes (no library default) + V15 machinery (solar/area/plan-geometry) + V7 sheet completion + V12 running heads/TOC/forms + page-box law — the flagship-review bar.
- **E13** track-law debt → the V16 rebind + limiter/stamina collapse (leg-1 reconcile); the api-census REFUTATION re-grounds V16(b) (see [D]); counts land at rg-zero for the migrated surfaces.

## [I] CAPABILITY-TARGET DISPOSITION ([03], all 16 planes → target grade)

graphic/vector(+pattern) 9.5 → [V1]+[V2]; typography 9.5 → [V3]; graphic/color 9 → [V4]+plate; graphic/raster 9 → [V8]; visualization/table 9 → [V10]/[E5]; visualization/diagram 9.5 → [V15]+[V13]+schematic; design language 9.5 → [V13] `graphic/style`; export/layered+indesign 9.5 → [V14]; visualization/chart 9 → [V10] typed grammar; scene 9 → [V9] REALIZED; core 9.5 → [V6] `core/issue` REAL + `ArtifactKind` derived + format dissolved; drawing 9.5 → V2+V3+V5 composed + ISO 286/1101/13567; composition 9.5 → [V7] one fold + ISO 7200 grid + ISSUE completion + sheet←emit; document 9.5 → [V12] typography seams + weasyprint + TOC + forms; export/dxf 9.5 → [V11]; specification/delivery 9.5 → OmniClass/Uniclass + rustworkx DAG + register↔classify.

## [J] GATED-OBLIGATION RULINGS ([06], all re-ruled — never silent)

- **Gate #1 measured-signals → REALIZE.** Runtime `[V5]` landed the table-keyed domain-histogram recorder (`record_artifact(kind, byte_volume, ratio)` on `INSTRUMENTS`, verified `runtime metrics.md:9`; runtime is track 1/5). Realize: `ArtifactReceipt.contribute` carries byte-volume/compression-ratio as NATIVE facts (`Bundle.ratio`, `Preview`/`Media` bands, byte counts) onto the one metric stream keyed by the carried `artifact` kind (`receipt.md:338` [METRIC_SIGNALS] RESOLVED). Render duration stays the runtime `Metrics.measured` aspect, not an ArtifactReceipt fact.
- **Gate #2 outward figure hand-off → REALIZE via a NEW artifacts-origin `HandoffAxis` case.** Compute `[V2]` (track 4/5, landed) requires a NEW artifacts-origin case shipped WITH its self-wired producer — NOT `model_asset` (which is compute-OWN; `handoff.md:20`). Realize: mint the artifacts-origin `HandoffAxis` case + its self-wired producer (geometry-ripple-at-axis-scale discipline), keyed by `ContentIdentity`. **CORRECT** the "model-asset case" wording in `[06]` and `ARCHITECTURE.md:94`.
- **Gate #3 content-keyed output elision → KEEP GATED.** Blocker: the C# `Rasm.Persistence` reuse fabric (not a py upstream). The runtime-side seam is landed (`ARCH:216` keyed session-lane elision, `core/plan` the consumer); the durable cache hit/miss is C#-owned. Ruled explicitly gated, blocker named.

## [K] LEG PARTITION (dependency barriers; each leg self-contained on the rebuild engine)

- **Leg 1a — foundational owner STRUCTURE.** `graphic/vector/{algebra,boolean,emit,pattern}` (+ delete `vector.md`), `graphic/color/{derive,measure,exchange,managed}`, `graphic/raster/{io,process,measure}`, `graphic/marks/{vocabulary,encode,decode}`, `graphic/style`, `graphic/layer`, `typography/{font,shape,layout,math}`. Targets carry repo-relative `libs/python/artifacts/.planning/graphic/...` paths. In-run reconcile closes the [05]b regime re-home + [05]c/d/cycle breaks + V4 rehome across these owners with whole-repo write authority.
- **Leg 1b — entry contract + core/ + corpus sweeps.** `core/{plan,receipt,issue}` (+ `core/format` condemned-intact). Reconcile obligations (whole-repo): the corpus-wide `emit()->ArtifactWork` convergence (every producer across all waves), the limiter/stamina collapse (43 `CapacityLimiter` mints + 55 `stamina` refs → `lanes.offload`/`guarded`), the full V16 rebind (content_identity/frozendict-tables/[RESEARCH]). Gated on 1a residual-clean (the emit rewire binds against stable owner structure). Rationale for the split: 1b's three corpus-wide sweeps rewire every page including 1a's — running them AFTER 1a's restructure stabilizes avoids re-rewiring emits against owners mid-flux.
- **Leg 2 — MID PLANE.** `visualization/{table,chart/{spec,export},diagram/{glyphset,solar,layout,draw,schematic}}` (+ delete `chart/transform`), `scene/{render,stage,export}`, `composition/{compose,imposition,sheet}`, `document/{model,emit,report,egress,lens,tagged}` (+ the `core/format` absorb executes here). V15 machinery + V9 scene + V12 typography seams land; placed figures arrive as parent content keys on the work graph (data edge, never an import).
- **Leg 3 — AEC/EGRESS.** `drawing/{standard,dimension,annotate,symbol,detail,schedule}`, `specification/{section,classify}`, `delivery/{register,transmittal}`, `export/{dxf,layered,indesign}`, `exchange/{conformance,credential,detect,metadata}` (COLD — emit + limiter/stamina landed in leg-1b; cold pass VERIFIES), `media/*` (COLD — same), `package/{vocabulary,codec,archive,delta}` (COLD except the two carried growth realizations). The three flagships render for visual review after leg 3.

Every leg's in-run reconcile closes every confirmed residual before it returns; no post-leg residual channel, no between-legs cleanup. The DECISION contains no `hard_residual` step. `README.md`/`ARCHITECTURE.md` close in the same leg that changes their facts; the pyproject reconciliation + `.api` stubs land with the leg that admits/removes the package.
