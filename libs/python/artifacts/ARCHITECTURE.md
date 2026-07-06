# [PY_ARTIFACTS_ARCHITECTURE]

The domain map of `artifacts` — the host-free durable-output engine that turns ingress (data, compute, geometry) into controllable, layer-clean artifacts. Each sub-domain owns one polymorphic surface, every artifact keys by the runtime content key, and every receipt is one kind-discriminated `ArtifactReceipt` family.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
artifacts/
├── document/               # paginated structured documents: the DocumentNode tree, its emit/extract inverses, finishing, accessibility, reports
│   ├── model.py            # DocumentNode semantic tree + StructureNode/StructEltKind PDF/UA family + DocumentDelta diff/merge algebra
│   ├── emit.py             # Emission axis: every backend (reportlab/weasyprint/typst/office/lxml) lowers FROM the DocumentNode tree
│   ├── lens.py             # DocumentLens extraction/recovery half over pdf_oxide/pypdf/pdfplumber/odfpy/pymupdf/ocrmypdf/calamine/lxml
│   ├── egress.py           # PDF security/navigation finishing (encrypt/outline/watermark/attach/impose/redact) over pikepdf/pypdf/pymupdf/msoffcrypto
│   ├── tagged.py           # Tagged-PDF PD/UA marked-content authoring + structural audit over pikepdf StructTreeRoot from the model tree
│   └── report.py           # ReportPlan reproducible notebook/section composition into the DocumentNode tree over jinja2/papermill/nbclient
├── visualization/          # data -> visual artifact
│   ├── chart/
│   │   ├── spec.py         # ChartSpec engine union over altair/lets-plot/matplotlib, palette-threaded
│   │   ├── export.py       # ChartExport host-free render/format dispatch over vl-convert-python
│   │   └── transform.py    # VegaTransform vegafusion data pre-pass (passthrough / inline self-contained spec / chart-state)
│   ├── table.py            # great-tables publication-table owner exporting HTML/LaTeX/PDF
│   └── diagram/
│       ├── layout.py       # diagram coordinate assignment over the 5-engine Force/Radial/Layered/Projected/Constrained policy, emitting all 10 DiagramKind
│       ├── draw.py         # drawsvg named-layer SVG + drawpyo editable .drawio over the DrawTarget selector (ziafont/ziamath <path> outlining)
│       └── glyphset.py     # bounded diagram-primitive vocabulary: node/edge/swimlane/annotation/marker marks + NodeShape/Port/weight topology
├── drawing/                # AEC drawing-production plane: owned ISO/NCS drafting vocabularies, ISO 129-1 dimensions, symbols, detail xrefs, schedules
│   ├── standard.py         # ISO 128/129-1/3098/5455/13567 + NCS/AIA owned-vocabulary substrate lowered onto ezdxf symbol tables
│   ├── dimension.py        # ISO 129-1 dimensioning producer over the closed DimOp family, lowered onto ezdxf add_*_dim() or a layered decomposition
│   ├── symbol.py           # AEC drawing-symbol vocabulary (section/elevation/detail/grid/north/scale/revision) dual-lowered to drawsvg + ezdxf blocks
│   ├── annotate.py         # ISO 128-2 leaders/keynotes/flag-notes + Knuth-Plass general notes + revision clouds, dual-lowered to drawsvg + ezdxf multileaders
│   ├── detail.py           # detail callouts + content-keyed ezdxf detail-library block store + rustworkx sheet cross-reference DAG
│   └── schedule.py         # AEC schedule templates + BIM QTO takeoff + ISO legends lowered into visualization/table over the closed ScheduleContent family
├── specification/          # CSI construction-specification plane: SectionFormat 3-part sections + MasterFormat/UniFormat/OmniClass classification
│   ├── section.py          # CSI SectionFormat 3-part + PageFormat numbering producer authored INTO the DocumentNode tree; contributes ArtifactReceipt.Spec
│   └── classify.py         # MasterFormat/UniFormat/OmniClass vocabularies + crosswalk + drawing<->spec ReferenceIndex resolver; pure substrate, no receipt
├── delivery/               # ISO 19650 delivery plane: information-container register + issue-for-construction transmittal
│   ├── register.py         # ISO 19650 drawing-register/sheet-index/container-metadata owner over the NA Table NA.1 vocabularies; ArtifactReceipt.Register
│   └── transmittal.py      # ISO 19650 issue-for-construction orchestrator over imposition + archive + credential + conformance; ArtifactReceipt.Transmittal
├── graphic/                # 2D graphic-primitive toolkit every visual + document plane composes
│   ├── raster/
│   │   ├── io.py           # pillow/pyvips IO/convert/thumbnail/montage working surface
│   │   ├── process.py      # raster vocabulary owner (Transform/RasterFact/ConvertFormat/Frame + TRANSFORMS acceptors) + the pillow/scikit-image produced-raster engine
│   │   └── measure.py      # perceptual-quality metrics + region/feature/registration measurement
│   ├── vector/
│   │   ├── path.py         # svgelements metric substrate: memoized parse, point-at-distance, decimation, area-weighted centroid, Tolerance policy rows
│   │   ├── region.py       # skia-pathops boolean/offset/stroke-to-outline + drawsvg document emission + resvg raster + metric text-on-path over shaped glyph outlines
│   │   └── pattern.py      # StrokeFamily/PatternSpec/DensityLaw repeating-fill generator; HatchFill pattern/solid/gradient; ezdxf + drawsvg.Pattern + pathops-clip lowerings
│   ├── marks/
│   │   ├── mark.py         # shared Symbology/DecodeSource/MarkFault/MarkPayload vocabulary + TAXONOMY class/carrier table both codec halves import
│   │   ├── encode.py       # segno/python-barcode/zxing-cpp machine-readable-mark generation
│   │   └── decode.py       # zxing-cpp read_barcodes decode inverse
│   └── color/
│       ├── derive.py       # colour-science + coloraide derivation VALUES (CIE/CAM16/spectral, gamut/CVD/harmony/WCAG, Metric block, Palette/hex_ramp, AdaptMethod), receipt-free
│       └── managed.py      # ICC/LUT/CCTF color-managed raster egress over colour-science core + pyvips/libvips ICC apply
├── typography/             # font binary + glyph shaping + line-layout
│   ├── font.py             # FontEngineering subset/instance/axis-catalog/outline/embed-audit owner over fonttools
│   ├── shape.py            # Shaping uharfbuzz text-shaping + python-bidi reorder + blackrenderer COLRv1 glyph-render
│   └── layout.py           # line-break (uniseg UAX#14) + hyphenation (pyphen) + Knuth-Plass paragraph layout
├── composition/            # assembling artifacts into pages/sheets
│   ├── compose.py          # post-render figure/section placement (scale-fit/n-up/crop/rotate/overlay) over svgelements/resvg/pillow
│   ├── sheet.py            # single-sheet title-block/frame/field owner (ISO 5457/7200/5455) + SheetSet multi-sheet numbering/register
│   └── imposition.py       # n-up / booklet / signature imposition over pymupdf show_pdf_page / pdfimpose
├── export/                 # editable layered hand-off for Illustrator/InDesign + DXF CAD exchange
│   ├── layered.py          # named-layer SVG + PDF OCG groups + psd-tools/PhotoshopAPI PSD/PSB + psdtags/tifffile layered TIFF + pyvips/lxml/stream-zip ORA
│   ├── indesign.py         # SimpleIDML IDML template-mutation hand-off
│   └── dxf.py              # ezdxf DXF CAD-exchange owner over the DxfOp family (7-backend render, DXF<->SVG<->GeoJSON<->glyph bridge); ArtifactReceipt.Cad
├── exchange/               # metadata / provenance / format identification at the boundary
│   ├── metadata.py         # EXIF/IPTC/XMP/ICC descriptive-metadata read/write over the MetaCarrier axis — pyexiftool raster, pikepdf PDF, av MEDIA
│   ├── credential.py       # c2pa-python content-credential Sign/Read/ReadFragment/Embed binding keyed by the content key
│   ├── conformance.py      # pyhanko PAdES sign/stamp/augment/reserve/audit close folding ConformanceVerdict (signer-free stamp, LTV augment, sheet seal)
│   └── detect.py           # s1 format-ID substrate: puremagic default + python-magic libmagic fallback on the runtime lane offload; imports nothing artifacts-internal
├── media/                  # temporal media: the 7-page container/codec/filter/timeline/subtitle/analysis/synthesis plane
│   ├── container.py        # av container spine: mux/demux capsule, encode/mux, transcode/remux, HDR/color, HLS/DASH via io_open (absorbs former video.py)
│   ├── filtergraph.py      # closed FilterNode owner + capability-detection native-vs-substitute routing over av.filter.filters_available
│   ├── audio.py            # av audio stream encode/mux/resample/master composing container + filtergraph
│   ├── timeline.py         # Trim/Concat/Segment/xfade non-linear editing over the container/filtergraph spine
│   ├── subtitle.py         # pysubs2 parse/convert/retime/restyle + passthrough-mux + typography/shape RGBA burn-in
│   ├── analysis.py         # waveform/spectrogram/loudness(ebur128)/silence/scene-detect/thumbnail, capability-routed native-vs-numpy/measure
│   └── synthesis.py        # numpy oscillator/noise/FM/AM/sweep/ADSR generation -> media/audio encode
├── scene/                  # 3D / spatial visualization
│   ├── render.py           # pyvista/vtk offscreen render + FieldFilter clip/slice/threshold/contour/warp pipeline + two-operand boolean CSG on the worker lane
│   ├── export.py           # glTF/VRML/OBJ/HTML scene-file export + orbit rgb24 frame seam
│   └── stage.py            # usd-core USD/USDZ stage authoring and composition
├── core/                   # production spine
│   ├── plan.py             # ArtifactPipeline content-keyed sub-graph-elision plan over the runtime session lane
│   ├── format.py           # TemplatePipeline one-context-many-format binding delegating to emit/report; owns no engine
│   └── receipt.py          # the ONE 23-case ArtifactReceipt union + re-homed ConformanceVerdict + roster-derived kind/keys + the Metrics.record seam + the graduates rail
└── package/                # content-addressed compression / archive / delta over one shared bundle vocabulary
    ├── bundle.py           # shared Bundle/CompressionAlgo/CodecProfile/BundleManifest vocabulary + BundleEvidence receipt projection + the PackWorker port
    ├── codec.py            # zstandard/lz4/brotli/zlib-ng single-blob compression composing bundle; the gzip_ng block-fan band with crc32_combine recombination
    ├── archive.py          # py7zr/stream-zip/stream-unzip archive containers; THE reproducible-ZIP owner (scene bundles arrive as DATA parents)
    └── delta.py            # detools binary diff/patch; parent-keyed delta nodes against the base bundle key
```

The engine reads as high-order domains. `document` owns one `DocumentNode` tree that `document/emit` lowers FROM and `document/lens` recovers TO over one node algebra, with `DocumentDelta` diff/merge once and an extracted-tree corpus keyed into the runtime columnar lane; `document/egress` seals and navigates the emitted PDF, `document/tagged` authors the PD/UA marked-content tree from the `document/model` `StructureNode`/`StructEltKind` family, and `document/report` composes reproducible notebooks back into the tree. `visualization` is the data->visual axis: `visualization/chart` splits spec authoring from host-free export and the vegafusion data pre-pass, `visualization/table` owns publication tables, and `visualization/diagram` is the data-driven diagram engine that lays a graph out (`layout`, five engines — `rustworkx`/`fast-sugiyama`/`pyelk`/`kiwisolver`/`grandalf` — over the `Force`/`Radial`/`Layered`/`Projected`/`Constrained` policies, emitting all ten `DiagramKind`: the AEC sun-path/circulation/stacking/program/site plus the general node-link/flowchart/entity-relation/Sankey/section-callout, threading `NodeShape`/`Port`/`weight`) and emits it as named-layer SVG or an editable `drawpyo` `.drawio` (`draw`, the `DrawTarget` selector, with `ziafont`/`ziamath` `<path>` label/formula outlining) from a bounded five-mark primitive vocabulary (`glyphset`: node/edge/swimlane/annotation/marker). `drawing` is the AEC drawing-production plane that sits on top of the pub/print substrate: `standard` is the closed owned-vocabulary substrate (ISO 128 line types/weights/hatches, ISO 5455 scales, ISO 3098 lettering, ISO 129-1 dimension styles, the ISO 13567/AIA/NCS layer-name + NCS sheet-identification codecs) lowered onto the real `ezdxf` symbol tables and read by every drawing producer, minting no receipt of its own exactly as `glyphset` mints none, `dimension` is the ISO 129-1 dimensioning producer over the closed `DimOp` family (linear/aligned/angular/radial/diameter/ordinate/arc/chain/baseline + tolerance) dual-lowered over `DimTarget` onto the categorical-best `ezdxf` `add_*_dim().render()` native path (DXF/SVG/PDF, ISO tolerance as `dimtol`/`dimlim`/`MTextEditor.stack`) or a `LAYERED` decomposition (`ezdxf.math` geometry + self-contained filled/stroked ISO 129-1 terminator marks + `ziafont` ISO 3098 text + `ziamath` tolerance math into named `export/layered` layers, all penned by the discipline sRGB, the tapered-terminator `graphic/vector/region` stroke-to-outline, composed via `symbol`, a pending growth axis), the `override=` DIM-variables from `standard` and the dimension-line offset stack solved by `kiwisolver`, `symbol` is the drawing-symbol vocabulary dual-lowered to `drawsvg` groups and `ezdxf` blocks that feeds the `composition/sheet` graphic cells, `annotate` is the ISO 128-2 leader/keynote/flag-note/general-note/revision-cloud producer dual-lowered to `drawsvg` named layers and `ezdxf` multileaders (composing `symbol`'s `SymbolStyle`, `typography`'s Knuth-Plass line break, and `kiwisolver` keynote-column routing), `detail` is the detail-management owner over detail callouts, a content-keyed `ezdxf` detail-library block store (one block authored once, placed N times), and the `rustworkx` sheet cross-reference DAG (cycle rejection, transitive reduction, revision-impact closure), and `schedule` is the AEC-scheduling owner that lowers a settled QTO/schedule frame (or an ISO legend enumerated from `standard`) into `visualization/table` as a standards-formatted door/window/room-finish/equipment/panel/structural schedule, the `csharp:Rasm.Bim` quantity-takeoff schedule (measure/cost columns closed on grand-summary `totals` sums), or a line-type/hatch/discipline legend, contributing the shared `ArtifactReceipt.Schedule` case. `specification` is the CSI construction-specification plane on that same substrate: `section` is the SectionFormat 3-part (General/Products/Execution) + PageFormat multi-level-numbering producer authored INTO the `document/model` `DocumentNode` tree, validating its MasterFormat number and article roster against the owned SectionFormat vocabularies and folding the specifier-note / unresolved-fill-in / reference-reconciliation `SpecVerdict`, contributing the `ArtifactReceipt.Spec` case, and `classify` is the pure MasterFormat/UniFormat/OmniClass classification substrate — exact-cardinality vocabularies, the `DERIVED_LOGIC` crosswalk, and the drawing<->spec `ReferenceIndex` resolver every spec/schedule/drawing reads — minting no receipt exactly as `standard` mints none. `delivery` is the ISO 19650 delivery plane that issues the set: `register` is the information-container register / sheet-index / container-metadata owner over the exact NA Table NA.1 suitability (`S0`-`S7`/`A`-`B`/documented) and revision (`P`/`C`) vocabularies, lowering to a `great-tables` sheet-index publication table, a structured `lxml` COBie/BS 1192 container XML, an `xlsxwriter`/`openpyxl` register spreadsheet, and the accumulating `RegisterEvidence` coverage audit, contributing `ArtifactReceipt.Register`, and `transmittal` is the issue-for-construction orchestrator composing `composition/imposition` + `package/archive` + `exchange/conformance` (PAdES) + `exchange/credential` (C2PA sheet-lineage) + `register` into the signed, content-addressed issue, contributing `ArtifactReceipt.Transmittal`. `graphic` is the 2D primitive toolkit every visual and document plane composes — `raster` (io/measure over the `process` vocabulary-and-behavior owner), the `vector` sub-plane (`path` the svgelements metric substrate, `region` the pathops/drawsvg/resvg emission owner with metric text-on-path, `pattern` the repeating-fill generator), the `marks` codec (the shared `mark` vocabulary plus the encode/decode halves), and `color` (the receipt-free `derive` value source plus the `managed` pyvips/libvips ICC/LUT egress, its one async-forcing worker leg). `typography` owns the font-binary transform, glyph shaping, and line-layout over one shared `PositionedGlyphRun` seam. `composition` assembles placed figures, architectural sheet-sets, and imposition; the editable named-layer hand-off then routes to `export` (`layered` OCG/SVG/ORA/PSD/layered TIFF + `indesign` SimpleIDML + `dxf` ezdxf CAD exchange), while the descriptive and trust boundary routes to `exchange` (`metadata`, `credential`, `conformance`, `detect`). `media` muxes the temporal artifacts, `scene` renders and exports the 3D plane (`render` owns the `SceneTarget` file-target vocabulary its `SceneOp.Export` keys; `export` composes it downward), `core` is the production spine (`plan`/`format`/`receipt`), and `package` is the content-addressed compression close over the shared `bundle` vocabulary.

`core/receipt` is the shared owner every visual and document sub-domain contributes one case to, never a parallel per-producer receipt rail, and the one receipt-fold edge the runtime `execution/lanes` reuse-fabric elision and the runtime `observability/metrics` recorder both consume — `contribute` records the numeric `_facts` scalars through the one polymorphic `Metrics.record(measure, domain="artifact", kind=...)` arm onto the landed `artifact.byte_volume`/`artifact.compression_ratio` distribution rows, render DURATION staying the runtime `Metrics.measured` aspect's fact, never a receipt's; outward figure handoff is LANDED: the `compute/graduation` `HandoffAxis` union carries the artifacts-origin `artifact` case and `core/receipt.graduates` projects any `ArtifactReceipt` into it keyed by `ContentIdentity` under the governed residual-ceiling policy row (a caller's tighter ceiling the override), `model_asset` staying a compute-own subject figures never ride; the artifacts sources re-mint no canonical concept so the runtime `evidence` `Structural.drift` query stays clean. `graphic/color/derive` is the one upstream color source the visual sub-domains pull palettes from rather than each engine picking color ad hoc, while `graphic/color/managed` is the downstream ICC/LUT/CCTF egress the raster and document outputs route through. The host-free posture is the structural axis cutting every sub-domain: `vl-convert` is the primary host-free chart export and `lets-plot` is the second host-free engine, and the great-tables Selenium path is the one remaining gated host path, never the default. The engine selection is the second structural axis: heavyweight render, raster, compression, text-layout, and 3D arms dispatch onto the runtime subprocess seam (`anyio.to_process.run_sync`) instead of importing provider-heavy modules into the core runtime path.

## [02]-[SEAMS]

```text seams
*                            ←  python:runtime                          # [CONTENT_KEY]: infallible ContentIdentity.of; producers bind the projected ContentKey
*                            →  python:runtime                          # [RECEIPT]: ArtifactReceipt contribution
document/model               →  python:data/tabular                     # [WIRE]: to_corpus_row flat record
document/model               →  python:artifacts/document/tagged        # [SHAPE]: StructureNode/StructEltKind structure tree + FigureNode.alt
graphic/color/derive         →  python:artifacts/visualization          # [PROJECTION]: Derivation.coords/notation palette VALUES to chart/table/diagram (receipt-free)
graphic/color/derive         →  python:artifacts/scene                  # [PROJECTION]: palette to scene colormap
graphic/color/derive         →  python:artifacts/graphic/color/managed  # [PROJECTION]: tone-curve / space provenance to the managed egress
graphic/color/managed        →  python:artifacts/graphic/raster         # [PROJECTION]: color-managed raster consumed by the process arms
graphic/color/managed        →  python:artifacts/document               # [PROJECTION]: ICC-profiled raster consumed by document/PDF output
core/receipt                 ←  python:runtime/execution                # [RECEIPT]: reuse-fabric elision ContentKey hit/miss
core/receipt                 ←  python:runtime/observability            # [RECEIPT]: Metrics.record(domain="artifact", kind) distribution rows off _facts at contribute
core/receipt                 →  python:compute/graduation               # [WIRE]: graduates rail projects ArtifactReceipt into HandoffAxis(artifact=) keyed by ContentIdentity
core/receipt                 ←  python:artifacts/core/plan              # [RECEIPT]: per-artifact contribute folds walked into the elision plan
visualization/table          ←  python:data/tabular                     # [SHAPE]: QualityProfile frame rendered by the great-tables tier
visualization/diagram/layout ←  python:data/tabular                     # [SHAPE]: graph adjacency/attributes feed the layout
graphic/raster/measure       →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Preview pixel + perceptual-metric facts
graphic/vector/path          →  python:artifacts/composition            # [SHAPE]: svgelements metric/affine substrate consumed by placement
graphic/vector/region        ←  python:artifacts/typography/shape       # [SHAPE]: text-on-path lays PositionedGlyphRun outlines via region.text_path at metric arc-length
graphic/marks/encode         →  python:artifacts/export                 # [PROJECTION]: emitted mark SVG bound as a named layer
visualization/diagram/draw   →  python:artifacts/export                 # [PROJECTION]: laid-out diagram SVG bound as named layers
drawing/standard             →  python:artifacts/composition            # [SHAPE]: ScaleRatio.ratio + SheetId.compose read by the sheet title-block
drawing/standard             →  python:artifacts/graphic/color/derive   # [PROJECTION]: discipline ACI-resolved sRGB routed to the color engine
drawing/dimension            ←  python:artifacts/drawing/standard       # [SHAPE]: Standard.dimstyle DIM-variable override + seed scaled by the ISO 5455 factor
drawing/dimension            →  python:artifacts/composition            # [PROJECTION]: dimensioned SVG/PDF bytes feed the sheet FigurePlacement drawing region
drawing/dimension            ←  python:artifacts/graphic/vector/path    # [SHAPE]: metric point-at-distance tick spacing; region stroke-to-outline composed via symbol
drawing/dimension            →  python:artifacts/export                 # [PROJECTION]: dimension-line/terminator/text/tolerance layers bound as OCG/SVG
drawing/dimension            →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Drawing dimension/dimstyle/extent/byte facts
drawing/symbol               →  python:artifacts/composition            # [PROJECTION]: single-mark PNG feeds the sheet NorthArrow.glyph/KeyPlan.figure cells
drawing/symbol               ←  python:artifacts/graphic/vector/region  # [SHAPE]: resvg rasterize SVG->PNG for the sheet-cell seam (skia-pathops offset growth)
drawing/symbol               →  python:artifacts/export                 # [PROJECTION]: named-layer symbol groups bound as OCG/SVG layers
drawing/symbol               →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Drawing kind/entity/extent/byte facts
drawing/detail               ←  python:artifacts/drawing/symbol         # [SHAPE]: SymbolKind bubble vocabulary + SymbolStyle mark-style the callout projects
drawing/detail               ←  python:artifacts/drawing/standard       # [SHAPE]: Discipline/ScaleRatio ISO layer/scale codes on the DetailRef
drawing/detail               →  python:artifacts/composition            # [PROJECTION]: detail-library block store + callout boundary/leader placed on the sheet
drawing/detail               →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Drawing callout/entity/byte facts
drawing/annotate             ←  python:artifacts/drawing/symbol         # [SHAPE]: SymbolStyle mark-style the leader/keynote/note marks compose
drawing/annotate             ←  python:artifacts/drawing/standard       # [SHAPE]: Terminator/LineWeight/TextHeight/LayerName owned ISO vocabulary
drawing/annotate             ←  python:artifacts/typography/layout      # [SHAPE]: LineBrokenRun Knuth-Plass general-note line break
drawing/annotate             ←  python:artifacts/typography/shape       # [SHAPE]: PositionedGlyphRun general-note shaped run
drawing/annotate             ←  python:artifacts/graphic/vector/region  # [SHAPE]: skia-pathops outline/boolean for the revision-cloud filled band
drawing/annotate             →  python:artifacts/export                 # [PROJECTION]: named-layer annotation groups bound as OCG/SVG layers
drawing/annotate             →  python:artifacts/composition            # [PROJECTION]: placed annotation layers onto the sheet
drawing/annotate             →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Drawing leader/keynote/note/cloud facts
drawing/schedule             ←  python:data/tabular                     # [SHAPE]: QTO/schedule rows frame from Rasm.Bim shaped by the ScheduleKind template
drawing/schedule             ←  python:artifacts/drawing/standard       # [SHAPE]: LineType.pattern/HatchMaterial/Discipline ISO codes for the legend swatches
drawing/schedule             →  python:artifacts/visualization/table    # [PROJECTION]: AEC schedule/legend frame+ops lowered into the TablePlan.build render
drawing/schedule             →  python:artifacts/composition            # [PROJECTION]: rendered schedule/legend table bytes placed on the sheet
drawing/schedule             →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Schedule kind/rows/columns/byte facts
specification/section        →  python:artifacts/document/model         # [PROJECTION]: Spec.to_document lowers the SectionFormat 3-part tree into DocumentNode
specification/section        →  python:artifacts/specification/classify # [SHAPE]: ClassCode MasterFormat section-number + division admitted through classify
specification/section        →  python:artifacts/visualization/table    # [PROJECTION]: QTO/schedule citation frame from csharp:Rasm.Bim rendered by table
specification/section        →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Spec section/division/part/article/byte facts
specification/classify       ←  python:artifacts/drawing/standard       # [SHAPE]: ISO 13567/NCS Discipline vocabulary on the ReferenceIndex SheetRef, composed
typography/font              →  python:artifacts/document               # [PROJECTION]: subsetted/instanced font bytes for FONT_EMBED
typography/font              →  python:artifacts/typography/shape       # [SHAPE]: face/variation location + embed-audit precondition
typography/shape             →  python:artifacts/document               # [PROJECTION]: PositionedGlyphRun text placement
typography/shape             →  python:artifacts/composition            # [PROJECTION]: PositionedGlyphRun annotation
typography/shape             →  python:artifacts/graphic/vector/region  # [SHAPE]: PositionedGlyphRun.on_path() per-glyph outlines consumed by region.text_path
typography/layout            →  python:artifacts/document               # [SHAPE]: LineLayout.broken() LineBrokenRun from a shaped run, beside the lay() rail
composition/compose          →  python:artifacts/export                 # [PROJECTION]: placed multi-source layout handed to named-layer egress
composition/sheet            ←  python:artifacts/drawing/standard       # [SHAPE]: ScaleRatio + SheetId sheet-number + Viewport ISO 5455 scale read by sheet
composition/sheet            →  python:artifacts/visualization/table    # [PROJECTION]: TitleBlock.revised + SheetSet.scheduled revision/drawing-list TablePlan
composition/sheet            →  python:artifacts/delivery/register      # [PROJECTION]: SheetSet.registered tuples build the SheetEntry via from_title_block
composition/sheet            →  python:artifacts/export                 # [PROJECTION]: frame + placed figures projected as named Layer rows
composition/imposition       →  python:artifacts/document               # [PROJECTION]: n-up/booklet sheet handed to the PDF assembler
export/layered               ←  python:artifacts/composition            # [PROJECTION]: placed multi-source layout + named-layer source graphics
export/layered               →  python:artifacts/core/receipt           # [RECEIPT]: LayeredExport.emit ArtifactReceipt.Preview/Egress named-layer evidence
export/indesign              ←  python:artifacts/composition            # [PROJECTION]: placed layout bound into the IDML template
export/indesign              →  python:artifacts/core/receipt           # [RECEIPT]: Idml.emit ArtifactReceipt.Office IDML-package evidence
export/dxf                   →  python:artifacts/composition/sheet      # [PROJECTION]: PyMuPdfBackend one-page PDF placed via show_pdf_page
export/dxf                   →  python:artifacts/graphic/vector/path    # [SHAPE]: make_path/flatten/fragment bridge at the vertex/d wire, one direction
export/dxf                   →  python:data/spatial/geospatial          # [WIRE]: addons.geo GeoProxy GeoJSON georeferenced wire; CRS authority stays with data
export/dxf                   →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Cad dxfversion/units/counts/layers/blocks/errors/byte facts
exchange/metadata            →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt descriptive-metadata facts
exchange/credential          ←  python:runtime                          # [CONTENT_KEY]: ContentKey
exchange/credential          →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Credential content-credential facts
exchange/credential          →  csharp:Rasm.Persistence                 # [CONTENT_KEY]: signed-artifact content-key binding decodes the XxHash128 seed
exchange/conformance         ←  python:artifacts/document               # [PROJECTION]: emitted PDF input, contributing ArtifactReceipt.Verdict
exchange/conformance         ←  python:artifacts/document/tagged        # [RECEIPT]: structural-conformance result folded into the verdict
graphic/raster/io            →  python:artifacts/exchange/detect        # [BOUNDARY]: s1 format-ID substrate imported downward at the profiled ingress
document/lens                →  python:artifacts/exchange/detect        # [BOUNDARY]: format-ID pre-flight at the ingest boundary (detect s1)
document/tagged              ←  python:artifacts/document/model         # [SHAPE]: StructureNode/StructEltKind tree authored to marked content
document/tagged              →  python:artifacts/exchange               # [RECEIPT]: structural-conformance result folded into the conformance verdict
document/tagged              →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Egress/Pdf structural evidence
scene/render                 →  python:artifacts/media                  # [SHAPE]: rgb24 frame sequence via VideoFrame.from_ndarray, ContentKey-keyed
scene/export                 ⇄  python:geometry/mesh                    # [BOUNDARY]: visualization-scene export vs mesh-file codec, no shared owner
media/container              ←  python:artifacts/scene                  # [SHAPE]: rgb24 frames ingested via VideoFrame.from_ndarray, renamed video owner
media/container              →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media container/codec encode + av/pysubs2 evidence band
media/container              ←  python:artifacts/media/filtergraph      # [SHAPE]: Transcode build_graph — native/substitute av.filter.Graph + numpy passes
media/filtergraph            →  python:artifacts/core/receipt           # [RECEIPT]: filter-node count in the composing producer's Media band; mints no receipt
media/audio                  →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media audio encode facts (EBU R128 LUFS/true-peak/LRA)
media/timeline               ←  python:artifacts/media/container        # [SHAPE]: _seek/_decode_window/_encode_video/_open_sink capsule, opens no container
media/timeline               ←  python:artifacts/media/filtergraph      # [SHAPE]: cross_dissolve xfade substitute + link_clips concat/amix
media/timeline               ←  python:artifacts/media/audio            # [SHAPE]: _decode_audio for the xfade acrossfade audio leg
media/timeline               →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media clip/segment counts + lossless-vs-reencode facts
media/subtitle               ←  python:artifacts/media/container        # [SHAPE]: MediaProfile/MediaFault/_encode_video for passthrough-mux + burn-in encode
media/subtitle               ←  python:artifacts/media/filtergraph      # [SHAPE]: filters_available selects overlay/soft-sub native vs numpy composite/burn-in
media/subtitle               ←  python:artifacts/typography/shape       # [SHAPE]: styled-fragment -> RGBA raster (uharfbuzz+python-bidi RTL, not Pillow RAQM)
media/subtitle               →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media subtitle event/style counts in the facts band
media/analysis               ←  python:artifacts/media/container        # [SHAPE]: av.open read capsule + MediaFault/_media_fault/_deployment
media/analysis               ←  python:artifacts/media/audio            # [SHAPE]: _decode_audio Pcm-block ingest
media/analysis               ←  python:artifacts/media/filtergraph      # [SHAPE]: filters_available probe selects native filter vs numpy/measure substitute
media/analysis               ←  python:artifacts/graphic/raster/measure # [SHAPE]: structural_similarity frame-to-frame scene-detection substitute
media/analysis               →  python:artifacts/graphic/raster/io      # [PROJECTION]: render_png + montage waveform/spectrogram/contact-sheet
media/analysis               →  python:compute/analysis/signal          # [SHAPE]: SignalOp.Spectral scipy spectrogram substitute (cross-branch)
media/analysis               →  python:compute/analysis/transform       # [SHAPE]: analytic-signal centroid/envelope (cross-branch)
media/analysis               →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media scene-cut/silence-span/LUFS facts band
media/synthesis              →  python:artifacts/media/audio            # [SHAPE]: _encode_audio numpy buffer -> container/mux (Pcm/_INGEST/Master reuse)
media/synthesis              →  python:compute/analysis/signal          # [SHAPE]: SignalOp.Filter/Resample band-limit + transform spectral QA (cross-branch)
media/synthesis              →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media synthesis fundamental_hz/waveform/duration facts band
delivery/register            ←  python:artifacts/composition/sheet      # [PROJECTION]: SheetSet.registered tuples build the SheetEntry via from_title_block
delivery/register            →  python:artifacts/visualization/table    # [PROJECTION]: RegisterOp.Index lowers Register.frame into the great-tables sheet-index
delivery/register            →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Register kind/container/suitability/revision/byte facts
delivery/transmittal         →  python:artifacts/composition/imposition # [PROJECTION]: constituent sheets laid into the press-form plan-set
delivery/transmittal         →  python:artifacts/package/archive        # [PROJECTION]: plan-set + register/receipts sealed into a content-addressed container
delivery/transmittal         →  python:artifacts/exchange/conformance   # [PROJECTION]: PAdES-LTA legal issue-for-construction signature over the plan-set PDF
delivery/transmittal         →  python:artifacts/exchange/credential    # [PROJECTION]: C2PA sheet-lineage provenance over the cover, sheets as Ingredients
delivery/transmittal         ←  python:artifacts/delivery/register      # [PROJECTION]: the issued index emit() + audited() composed as the transmittal manifest
delivery/transmittal         →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Transmittal issue/sheet/suitability/container/verdict facts
core/plan                    ←  python:runtime/execution                # [CONTENT_KEY]: (ContentKey, Work) session-lane elision keyed on the Admit.keyed unit
core/format                  →  python:artifacts/document               # [PROJECTION]: bound lowering DOCX_TEMPLATE/PDF_TYPST + ODS/XLSX/XML/YAML/TOML arms
core/format                  →  python:artifacts/document/report        # [PROJECTION]: HTML via the report TEMPLATE-kind jinja ReportLoader rendered entry
package/codec                ←  python:runtime                          # [CONTENT_KEY]: ContentKey including the DELTA parent-key from-image
package/delta                ←  python:runtime                          # [CONTENT_KEY]: DELTA parent-key from-image
```
