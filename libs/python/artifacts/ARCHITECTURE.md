# [PY_ARTIFACTS_ARCHITECTURE]

`artifacts` owns the host-free durable-output engine turning data, compute, and geometry ingress into layer-clean files. Each sub-domain owns one polymorphic surface, every artifact keys by the runtime content key, and every receipt is one case of the kind-discriminated `ArtifactReceipt` union. Alignment with the Persistence, Fabrication, and Python peers travels the content-keyed wire, never a reference.

## [01]-[DOMAIN_MAP]

```text codemap
artifacts/
├── document/            # paginated structured documents: the DocumentNode tree and its emit/extract inverses
│   ├── model.py         # DocumentNode semantic tree, DocumentDelta diff/merge algebra, PDF/UA StructureNode family, lowering projections
│   ├── emit.py          # emission axis: every backend lowers FROM the DocumentNode tree
│   ├── lens.py          # DocumentLens extraction and recovery half over the reader backends
│   ├── egress.py        # PDF security and navigation finishing
│   ├── tagged.py        # Tagged-PDF PDF/UA marked-content authoring and structural audit from the model tree
│   └── report.py        # reproducible notebook and section composition into the DocumentNode tree
├── visualization/       # data to visual artifact
│   ├── chart/
│   │   ├── spec.py      # ChartSpec engine union, derive-palette-threaded
│   │   └── export.py    # host-free chart render and format dispatch with the in-page VegaTransform pre-pass
│   ├── table.py         # great-tables publication-table owner exporting HTML/LaTeX/PDF
│   └── diagram/
│       ├── layout.py    # diagram coordinate assignment over the layout engines, emitting the DiagramKind rows
│       ├── draw.py      # named-layer SVG and editable .drawio emission over the DrawTarget selector
│       ├── schematic.py # named-symbol schematic producer for the diagram class the marks cannot express
│       ├── solar.py     # pvlib SPA solar-ephemeris and sun-path furniture owner
│       └── glyphset.py  # bounded diagram-primitive vocabulary the marks draw from
├── drawing/             # AEC drawing-production plane: owned ISO/NCS drafting vocabularies, dimensions, symbols, xrefs
│   ├── regime.py        # the owned drafting vocabulary and BIND substrate every drawing consumer reads; mints no receipt
│   ├── standard.py      # the ezdxf symbol-table lowering of the regime
│   ├── dimension.py     # ISO 129-1 + ISO 1101 GD&T dimensioning producer over the closed DimOp family, dual-lowered per DimTarget
│   ├── symbol.py        # AEC drawing-symbol vocabulary dual-lowered to drawsvg and ezdxf
│   ├── annotate.py      # ISO 128-2 leaders, keynotes, notes, and revision clouds, dual-lowered to drawsvg and ezdxf
│   ├── detail.py        # detail callouts, a content-keyed ezdxf block store, and the sheet cross-reference DAG
│   └── schedule.py      # AEC schedule and BIM QTO takeoff lowered into visualization/table; contributes the Schedule receipt
├── specification/       # CSI construction-specification plane on the pub/print substrate
│   ├── section.py       # CSI SectionFormat 3-part sections authored INTO DocumentNode; contributes the Spec receipt
│   └── classify.py      # MasterFormat/UniFormat/OmniClass vocabularies and the drawing<->spec resolver; mints no receipt
├── delivery/            # ISO 19650 delivery plane: container register, issue-for-construction transmittal, and wire notice
│   ├── register.py      # ISO 19650 register, sheet-index, and container-metadata owner; contributes the Register receipt
│   ├── transmittal.py   # issue-for-construction orchestrator over imposition, archive, credential, and conformance
│   └── notice.py        # TransmittalNotice trace-continuous CloudEvents envelope sealing the settled issue close
├── graphic/             # 2D graphic-primitive toolkit every visual and document plane composes
│   ├── raster/
│   │   ├── io.py        # pillow/pyvips IO, convert, thumbnail, montage working surface
│   │   ├── process.py   # raster vocabulary owner and produced-raster engine
│   │   └── measure.py   # perceptual-quality metrics and region/feature/registration measurement
│   ├── vector/
│   │   ├── path.py      # svgelements metric substrate: parse, point-at-distance, decimation, tolerance
│   │   ├── region.py    # skia-pathops boolean/offset/stroke-to-outline with metric text-on-path
│   │   └── pattern.py   # repeating-fill and hatch generator emitting to ezdxf and drawsvg
│   ├── marks/
│   │   ├── mark.py      # shared machine-readable-mark vocabulary both codec halves import
│   │   ├── encode.py    # Mark operation owner: segno/python-barcode/zxing-cpp generation with composed decode/verify
│   │   └── decode.py    # zxing-cpp scan substrate on the shared MarkFault rail
│   ├── color/
│   │   ├── derive.py    # the one upstream color-derivation source: CIE/CAM16/spectral, gamut, CVD, harmony, WCAG
│   │   └── managed.py   # the downstream ICC/LUT/CCTF color-managed raster egress
│   ├── style.py         # theme-as-data owner: one Theme row set carries type, stroke, palette, ground, sheet family
│   └── layer.py         # LayerPlan semantic layer tree every layered producer projects into and exporter composes out of
├── typography/          # font binary, glyph shaping, math typesetting, and line-layout over one PositionedGlyphRun seam
│   ├── font.py          # FontEngineering subset/instance/synthesize/axis/outline/embed-audit owner and the FaceMetrics value
│   ├── shape.py         # uharfbuzz shaping, bidi reorder, COLRv1 glyph render, SVG path export
│   ├── math.py          # the one ziamath mathematical-typesetting owner every formula consumer routes through
│   └── layout.py        # line-break, hyphenation, and Knuth-Plass paragraph layout
├── composition/         # assembling placed figures, sheets, and imposition
│   ├── compose.py       # post-render figure and section placement
│   ├── sheet.py         # single-sheet title-block/frame owner and the SheetSet register-ready set owner
│   └── imposition.py    # n-up, booklet, and signature imposition
├── export/              # editable layered hand-off for Illustrator/InDesign and DXF CAD exchange
│   ├── layered.py       # named-layer SVG, PDF OCG, PSD/PSB, layered TIFF, and ORA export
│   ├── indesign.py      # SimpleIDML template-mutation hand-off
│   └── dxf.py           # ezdxf DXF CAD-exchange owner over the DxfOp family and the geospatial bridge
├── exchange/            # metadata, provenance, and format identification at the boundary
│   ├── metadata.py      # EXIF/IPTC/XMP/ICC descriptive-metadata read/write over the MetaCarrier axis
│   ├── credential.py    # c2pa-python content-credential sign/read/embed/ingredient-archive keyed by the content key
│   ├── conformance.py   # pyhanko PAdES sign/stamp/augment/reserve/audit folding ConformanceVerdict
│   └── detect.py        # format-ID substrate over puremagic with a python-magic fallback
├── media/               # temporal media: container, codec, filter, timeline, subtitle, analysis, synthesis
│   ├── container.py     # av container spine: mux, demux, encode, transcode, HDR/color, HLS/DASH
│   ├── filtergraph.py   # closed FilterNode owner with capability-detected native-vs-substitute routing
│   ├── audio.py         # av audio stream encode, mux, resample, master
│   ├── timeline.py      # non-linear editing over the container and filtergraph spine
│   ├── subtitle.py      # pysubs2 parse/convert/retime/restyle, passthrough-mux, and burn-in
│   ├── analysis.py      # waveform, spectrogram, loudness, silence, black/scene detect, thumbnail; capability-routed
│   └── synthesis.py     # numpy oscillator/noise/FM/AM/sweep/ADSR audio and bars/ramp/grid/countdown test-signal generation
├── scene/               # 3D and spatial visualization
│   ├── spec.py          # parse-floor seam vocabulary both interpreters import, framed/CANON identity preimage, and the WORKER_MODULE floor anchor
│   ├── render.py        # Scene3d SceneOp producer: content-key mint, process-lane offload, receipt fold, and the rgb24 frame egress
│   ├── render_worker.py # worker-only pyvista/matplotlib render bodies the runtime shipped gate resolves inside the process lane
│   ├── export.py        # per-target ExportRow law: plotter writes, bundle capture, and USD delegation over every SceneTarget
│   └── stage.py         # usd-core USD/USDZ stage authoring and composition
├── core/                # production spine
│   ├── plan.py          # ArtifactPipeline content-keyed sub-graph-elision plan over the runtime session lane
│   ├── issue.py         # the constructing owner: issue(IssueRequest) over the modality union into pipeline and drain
│   ├── receipt.py       # the one ArtifactReceipt union, ConformanceVerdict, and the Metrics.record + hook-tap seam
│   ├── hooks.py         # ArtifactHook production-fact point table, payload projections, and the latched Production registrar
│   └── bench.py         # CORPUS producer benchmark subjects, seeded recipes, and threshold policy over the runtime bench tier
└── package/             # content-addressed compression, archive, and delta over one shared bundle vocabulary
    ├── bundle.py        # shared Bundle/CodecProfile/BundleManifest vocabulary and the BundleEvidence projection
    ├── codec.py         # single-blob compression composing bundle, with the parallel block-fan band
    ├── archive.py       # archive containers and the reproducible-ZIP owner
    └── delta.py         # detools binary diff/patch; parent-keyed delta nodes against the base bundle key
```

## [02]-[STRATA]

Strata rank the artifacts interior; seating rows carry only the law the fence cannot show.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TB
    accTitle: Artifacts interior import strata
    accDescr: Six import strata from the issue conductor down to the plan-receipt floor; each labeled edge names one sourced type.
    subgraph S5["S5 CONDUCTOR"]
        Issue[core/issue]
        Bench[core/bench]
    end
    subgraph S4["S4 DELIVERY"]
        Delivery[delivery]
    end
    subgraph S3["S3 COMPOSERS"]
        Media[media]
        Document[document]
        Composition[composition]
        Specification[specification]
    end
    subgraph S2["S2 VISUAL"]
        Graphic[graphic]
        Visualization[visualization]
        Drawing[drawing]
        Export[export]
    end
    subgraph S1["S1 SUBSTRATE"]
        Scene[scene]
        Typography[typography]
        Exchange[exchange]
        Package[package]
    end
    subgraph S0["S0 FLOOR"]
        Plan[core/plan]
        Receipt[core/receipt]
        Hooks[core/hooks]
    end
    Issue s1@-->|"[IMPORT]: Transmittal"| Delivery
    Issue s2@-->|"[IMPORT]: DocumentPlan"| Document
    Issue s3@-->|"[IMPORT]: Spec"| Specification
    Issue s4@-->|"[IMPORT]: DiagramDraw"| Visualization
    Issue s5@-->|"[IMPORT]: Palette"| Graphic
    Issue s6@-->|"[IMPORT]: PipelinePlan"| Plan
    Delivery s7@-->|"[IMPORT]: ImposedPlan"| Composition
    Delivery s8@-->|"[IMPORT]: SignerSource"| Exchange
    Delivery s9@-->|"[IMPORT]: Archive"| Package
    Delivery s10@-->|"[IMPORT]: TablePlan"| Visualization
    Composition s11@-->|"[IMPORT]: Layer"| Export
    Composition s12@-->|"[IMPORT]: SheetId"| Drawing
    Composition s13@-->|"[IMPORT]: PathFault"| Graphic
    Composition s14@-->|"[IMPORT]: TablePlan"| Visualization
    Specification s15@-->|"[IMPORT]: Discipline"| Drawing
    Document s16@-->|"[IMPORT]: MediaClass"| Exchange
    Media s17@-->|"[IMPORT]: framed"| Scene
    Media s18@-->|"[IMPORT]: _save_array"| Graphic
    Graphic s19@-->|"[IMPORT]: PositionedGlyphRun"| Typography
    Graphic s20@-->|"[IMPORT]: DetectEngine"| Exchange
    Drawing s21@-->|"[IMPORT]: PositionedGlyphRun"| Typography
    Visualization s22@-->|"[IMPORT]: Formula"| Typography
    Plan s23@-->|"[IMPORT]: ArtifactReceipt"| Receipt
    Issue s24@-->|"[IMPORT]: Production"| Hooks
    Bench s25@-->|"[IMPORT]: Codec"| Package
    Bench s26@-->|"[IMPORT]: ArtifactKind"| Receipt
    Bench s27@-->|"[IMPORT]: SynthOp"| Media
    Delivery s28@-->|"[IMPORT]: Production"| Hooks
    Typography ~~~ Plan
    Receipt f1@-->|"forbidden: upward import"| S5
```

- S0 `core/plan` + `core/receipt` + `core/hooks` — the spine floor imports no artifacts sibling above it.
- S0 seats `PipelinePlan`/`ArtifactWork`, the `ArtifactReceipt` union, and the `ARTIFACT_POINTS` hook rows; `hooks` composes the runtime registry.
- S0 `receipt` composes runtime, the compute `HandoffAxis`, and the hooks `Production` fire — the one same-stratum interleave.
- S0–S5 every plane composes the floor (`ArtifactWork`, `ArtifactReceipt`); the fence draws only each plane's discriminating imports.
- S1 `typography`, `exchange`, `package`, `scene` — substrate planes composing the floor alone.
- S1 seats `PositionedGlyphRun`, the metadata/credential/conformance boundary, the `Bundle` vocabulary, and the `SceneGrid` parse floor.
- S2 `graphic` + `drawing` + `visualization` + `export` — one visual stratum, module-acyclic.
- S2 `drawing/regime` composes `graphic/color/derive` and `vector/pattern`; `graphic/layer` and `style` compose the regime back.
- S2 `drawing/schedule` lowers into `visualization/table`; `visualization/chart/export` composes `export/layered`, the DXF owner hopping back.
- S3 `document`, `media`, `composition`, `specification` — composer planes over the visual stratum.
- S3 `specification/section` composes the document `BlockKind` tree in-stratum; `media` rides the scene `framed` parse floor and raster save hop.
- S4 `delivery` then S5 `core/issue` — `issue` alone imports upward-named producers, so the spine is floor and conductor, never one stratum.
- S4 `transmittal` composes the `notice` seal downward; `notice` fires the floor `Production` row, so the plane stays acyclic.
- S5 `core/bench` rides the conductor stratum without conducting — no producer imports it or cycles through it.
- S5 `bench` composes the package recipes, the receipt `ArtifactKind`, and `media/synthesis` replay; native-offload kernels arrive as caller values.

## [03]-[SEAMS]

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Artifacts package seam registry
    accDescr: Artifacts sub-domain owners exchanging content keys, receipts, wires, and shapes with Python and C# peers.
    subgraph artifacts[PY:ARTIFACTS]
        Core[Core spine]
        Document[Document]
        Visualization[Visualization]
        Drawing[Drawing]
        Media[Media]
        Scene[Scene]
        Export[Export]
        Exchange[Exchange]
        Package[Package]
    end
    Geometry{{python:geometry}}
    Runtime{{python:runtime}}
    Data{{python:data}}
    Compute([python:compute])
    Persistence[(csharp:Rasm.Persistence)]
    Fabrication([csharp:Rasm.Fabrication])
    Runtime e1@-->|"[CONTENT_KEY]: ContentKey"| Core
    Core e2@-->|"[RECEIPT]: ArtifactReceipt"| Runtime
    Runtime e3@-->|"[CONTENT_KEY]: ContentKey"| Exchange
    Runtime e4@-->|"[CONTENT_KEY]: ContentKey"| Package
    Core e5@-->|"[GRADUATION]: HandoffAxis"| Compute
    Media e6@-->|"[SHAPE]: SignalOp"| Compute
    Document e7@-->|"[WIRE]: CorpusRow"| Data
    Export e8@-->|"[WIRE]: GeoJSON"| Data
    Data e9@-->|"[SHAPE]: QualityProfile"| Visualization
    Fabrication e11@-->|"[WIRE]: IToleranceEncoder bytes"| Drawing
    Geometry e12@-->|"[BOUNDARY]: SceneGrid"| Scene
    Exchange e13@-->|"[CONTENT_KEY]: SignedArtifact"| Persistence
    Runtime e14@-->|"[PORT]: Kernel"| Scene
    Core e15@-->|"[HOOK]: HookPoint"| Runtime
    Core e16@-->|"[BENCH]: BenchmarkReceipt"| Runtime
```

Frozen names spell from the owner's endpoint page: `SignedArtifact` from Rasm.Persistence with the runtime `ContentKey` minting beneath it, `IToleranceEncoder` bytes from Rasm.Fabrication admitted into `GdtFrame` at dimensioning, and the graduation hub as `HandoffAxis`, C#-owned as `GraduationEvidence`.

Production-fact points register onto the runtime `Hooks` registry under the `rasm.artifacts.<domain>.<point>` grammar, and the bench corpus consumes the runtime `Bench` tier, minting no timing. `TransmittalNotice` seals the settled close as a CloudEvents envelope whose transport stays the composing app's, so no broker edge joins this registry.

## [04]-[INTERNAL]

One production spine composes the primitive substrate, the producer planes, and the finishing tiers; per-stage guards, conditioning, and rails live on the owning implementation pages.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Artifacts production spine
    accDescr: Issue request through plan, producer engines, composition, and finishing into the one receipt fold and the package close.
    Issue([Issue request]) --> Plan[[Pipeline plan]]
    Substrate[(Graphic + type substrate)] --> Engines
    Plan --> Engines[[Producer engines]]
    Engines --> Compose[[Composition]]
    Compose --> Finish[[Export + exchange]]
    Finish --> Fold[(Contribution fold)]
    Fold --> Package[[Package close]]
    Package --> Deliver([Transmittal])
```

High-order producer planes sit on a shared primitive substrate. `graphic` and `typography` own the raster, vector, marks, color, style, layer, font, shaping, math, and line-layout primitives every plane composes over one `PositionedGlyphRun` seam; the producer planes lower onto them; `composition` places the outputs, `export` and `exchange` finish them, `core` is the production spine, and `package` is the content-addressed close.

- `core/receipt` is the one shared receipt owner every producer contributes one case to.
- Composite owners drive sibling producers only through the uniform `emit()`/`work()` contract; a sibling convenience entry is a phantom.
- A receipt's `slot` threads the producer's pre-run input key; a produced-output content address lands only as a facts-band scalar.
- Producer sync projections read the landed evidence successor; re-invoking the fold or a frame author is a split-execution defect.
- An un-folded owner projects nothing, so absence stays distinct from evidence.
- Dual-license provider pairs split by import reachability: no copyleft module is reachable from the permissive footing.
- Derivable constants land as policy tables on the owner, and each footing's closure audits from its imports alone.
- `contribute` records numeric facts through the runtime metrics arm; render duration stays a runtime fact, never a receipt's.
- Production facts fire on the `core/hooks` point rows at the issue seams and the contribute fold.
- Observability subscribes through `Production.subscribed` at the app root, never in producer code.
- Issue-scope baggage the issue bracket binds attributes every signal; tenant promotion stays runtime-owned.
- `core/bench` grades producer kernels against threshold policy rows through the runtime bench tier.
- Bench timing, quantiles, and instruments stay runtime-owned.
- Each bench row's deterministic input is a typed `BenchFeed` edge; a regression is a graded verdict, never a fault.
- `delivery/notice` seals the settled transmittal close as one trace-continuous CloudEvents envelope fired on the `NOTICE_ISSUED` hook row.
- Notice envelope bytes end at the wire value.
- Outward figure handoff is landed, not re-minted: `core/receipt.graduates` projects any `ArtifactReceipt` into the compute graduation hub.
- Projection keys by `ContentIdentity` under the governed residual-ceiling policy, a caller's tighter ceiling overriding.
- Sources re-mint no canonical concept, so the runtime structural-drift query stays clean.
- `graphic/color/derive` is the one upstream color source every visual plane pulls palettes from.
- `graphic/color/managed` is the downstream ICC/LUT/CCTF egress the raster and document outputs route through.
- Host-free rendering cuts every sub-domain: chart export dispatches onto host-free engines only, ranked by the owner's policy row.
- One gated host-render path exists behind explicit opt-in, never the default.
- Engine selection is the second structural axis: heavy render, raster, compression, text-layout, and 3D arms cross as runtime `Kernel` values.
- A `KernelTrait` row derives each kernel's thread, subinterpreter, or process arm.
- Provider-heavy modules never import into the core runtime path.
