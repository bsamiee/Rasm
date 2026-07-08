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
│   ├── egress.py           # PDF security/navigation finishing over pikepdf/pypdf/pymupdf/msoffcrypto
│   ├── tagged.py           # Tagged-PDF PD/UA marked-content authoring + structural audit over pikepdf StructTreeRoot from the model tree
│   └── report.py           # ReportPlan reproducible notebook/section composition into the DocumentNode tree over jinja2/papermill/nbclient
├── visualization/          # data -> visual artifact
│   ├── chart/
│   │   ├── spec.py         # ChartSpec engine union over altair/lets-plot/matplotlib, derive-palette-threaded
│   │   └── export.py       # ChartExport host-free render/format dispatch over vl-convert-python + the in-page VegaTransform vegafusion pre-pass
│   ├── table.py            # great-tables publication-table owner exporting HTML/LaTeX/PDF
│   └── diagram/
│       ├── layout.py       # diagram coordinate assignment over Force/Radial/Layered/Projected/Constrained, emitting ten DiagramKind rows
│       ├── draw.py         # drawsvg named-layer SVG + drawpyo editable .drawio over DrawTarget (labels via typography/shape, formulas via math)
│       ├── schematic.py    # schemdraw named-symbol schematic producer: 226-element catalog + flow/logic/dsp + Kmap/Timing/BitField, path-text SVG
│       ├── solar.py        # pvlib SPA solar-ephemeris owner: positions/day arcs/analemmas/solstices + sun-path furniture over SolarProjection
│       └── glyphset.py     # bounded diagram-primitive vocabulary: node/edge/swimlane/annotation/marker/area/fragment
├── drawing/                # AEC drawing-production plane: owned ISO/NCS drafting vocabularies, ISO 129-1 dimensions, symbols, detail xrefs
│   ├── regime.py           # s1 drafting vocabulary + BIND substrate: ISO 128/3098/5455/13567 + NCS/AIA codes, LayerName/SheetId codecs
│   ├── standard.py         # the ezdxf symbol-table LOWERING of the regime: seed/graphics/dimstyle/hatch/rgb/paper_factor + ACI/fill/DIMSTYLE rows
│   ├── dimension.py        # ISO 129-1 dimensioning producer over the closed DimOp family, lowered onto ezdxf add_*_dim() or a layered decomposition
│   ├── symbol.py           # AEC drawing-symbol vocabulary (section/elevation/detail/grid/north/scale/revision) dual-lowered to drawsvg + ezdxf
│   ├── annotate.py         # ISO 128-2 leaders/keynotes/flag-notes + Knuth-Plass general notes + revision clouds, dual-lowered to drawsvg + ezdxf
│   ├── detail.py           # detail callouts + content-keyed ezdxf detail-library block store + rustworkx sheet cross-reference DAG
│   └── schedule.py         # AEC schedule templates + BIM QTO takeoff + ISO legends lowered into visualization/table over the closed ScheduleContent
├── specification/          # CSI construction-specification plane: SectionFormat 3-part sections + MasterFormat/UniFormat/OmniClass classification
│   ├── section.py          # CSI SectionFormat 3-part + PageFormat numbering authored INTO DocumentNode; contributes Spec receipt
│   └── classify.py         # MasterFormat/UniFormat/OmniClass vocabularies + drawing<->spec ReferenceIndex resolver; receipt-free substrate
├── delivery/               # ISO 19650 delivery plane: information-container register + issue-for-construction transmittal
│   ├── register.py         # ISO 19650 drawing-register/sheet-index/container-metadata owner over the NA Table NA.1 vocabularies
│   └── transmittal.py      # ISO 19650 issue-for-construction orchestrator over imposition + archive + credential + conformance
├── graphic/                # 2D graphic-primitive toolkit every visual + document plane composes
│   ├── raster/
│   │   ├── io.py           # pillow/pyvips IO/convert/thumbnail/montage working surface
│   │   ├── process.py      # raster vocabulary owner plus pillow/scikit-image produced-raster engine
│   │   └── measure.py      # perceptual-quality metrics + region/feature/registration measurement
│   ├── vector/
│   │   ├── path.py         # svgelements metric substrate: parse, point-at-distance, decimation, centroid, tolerance rows
│   │   ├── region.py       # skia-pathops boolean/offset/stroke-to-outline, drawsvg emission, resvg raster, metric text-on-path
│   │   └── pattern.py      # StrokeFamily/PatternSpec/DensityLaw repeating-fill generator; HatchFill pattern/solid/gradient; ezdxf + drawsvg.Pattern
│   ├── marks/
│   │   ├── mark.py         # shared Symbology/DecodeSource/MarkFault/MarkPayload vocabulary + TAXONOMY class/carrier table both codec halves import
│   │   ├── encode.py       # segno/python-barcode/zxing-cpp machine-readable-mark generation
│   │   └── decode.py       # zxing-cpp read_barcodes decode inverse
│   ├── color/
│   │   ├── derive.py       # colour-science + coloraide derivation values: CIE/CAM16/spectral, gamut, CVD, harmony, WCAG
│   │   └── managed.py      # ICC/LUT/CCTF color-managed raster egress + Separation/DeviceN plate, LUT-bake, CxF3 swatch Color terminals
│   ├── style.py            # theme-as-DATA SELECT owner: type-system rows, strokes, palette seeds, ground, entourage, sheet families
│   └── layer.py            # LayerPlan semantic layer tree: LayerIntent, BlendMode, ISO/editorial names, flattened/zsorted folds
├── typography/             # font binary + glyph shaping + line-layout
│   ├── font.py             # FontEngineering subset/instance/axis-catalog/outline/embed-audit owner over fonttools + the FaceMetrics OT-metrics value
│   ├── shape.py            # Shaping uharfbuzz text-shaping, python-bidi reorder, blackrenderer COLRv1 glyph render, SVG path export
│   ├── math.py             # THE ziamath mathematical-typesetting owner: MathML/LaTeX/mixed formulas, MATH constants, baseline seating
│   └── layout.py           # line-break (uniseg UAX#14) + hyphenation (pyphen) + Knuth-Plass paragraph layout
├── composition/            # assembling artifacts into pages/sheets
│   ├── compose.py          # post-render figure/section placement (scale-fit/n-up/crop/rotate/overlay) over svgelements/resvg/pillow
│   ├── sheet.py            # single-sheet title-block/frame/field owner (ISO 5457/7200/5455) + SheetSet multi-sheet numbering/register
│   └── imposition.py       # n-up / booklet / signature imposition over pymupdf show_pdf_page / pdfimpose
├── export/                 # editable layered hand-off for Illustrator/InDesign + DXF CAD exchange
│   ├── layered.py          # named-layer SVG, PDF OCG groups, PSD/PSB, layered TIFF, and ORA export
│   ├── indesign.py         # SimpleIDML IDML template-mutation hand-off
│   └── dxf.py              # ezdxf DXF CAD-exchange owner over the DxfOp family (7-backend render, DXF<->SVG<->GeoJSON<->glyph bridge)
├── exchange/               # metadata / provenance / format identification at the boundary
│   ├── metadata.py         # EXIF/IPTC/XMP/ICC descriptive-metadata read/write over the MetaCarrier axis — pyexiftool raster, pikepdf PDF, av MEDIA
│   ├── credential.py       # c2pa-python content-credential Sign/Read/ReadFragment/Embed binding keyed by the content key
│   ├── conformance.py      # pyhanko PAdES sign/stamp/augment/reserve/audit close folding ConformanceVerdict and sheet seal
│   └── detect.py           # s1 format-ID substrate: puremagic default plus python-magic fallback on runtime lane
├── media/                  # temporal media: the 7-page container/codec/filter/timeline/subtitle/analysis/synthesis plane
│   ├── container.py        # av container spine: mux/demux, encode, transcode, HDR/color, HLS/DASH via io_open
│   ├── filtergraph.py      # closed FilterNode owner + capability-detection native-vs-substitute routing over av.filter.filters_available
│   ├── audio.py            # av audio stream encode/mux/resample/master composing container + filtergraph
│   ├── timeline.py         # Trim/Concat/Segment/xfade non-linear editing over the container/filtergraph spine
│   ├── subtitle.py         # pysubs2 parse/convert/retime/restyle + passthrough-mux + typography/shape RGBA burn-in
│   ├── analysis.py         # waveform/spectrogram/loudness(ebur128)/silence/scene-detect/thumbnail, capability-routed native-vs-numpy/measure
│   └── synthesis.py        # numpy oscillator/noise/FM/AM/sweep/ADSR generation -> media/audio encode
├── scene/                  # 3D / spatial visualization
│   ├── render.py           # pyvista/vtk offscreen render, FieldFilter pipeline, and two-operand boolean CSG on the worker lane
│   ├── export.py           # glTF/VRML/OBJ/HTML scene-file export + orbit rgb24 frame seam
│   └── stage.py            # usd-core USD/USDZ stage authoring and composition
├── core/                   # production spine
│   ├── plan.py             # ArtifactPipeline content-keyed sub-graph-elision plan over the runtime session lane
│   ├── issue.py            # THE constructing owner: issue(IssueRequest) over sheet_set|diagram_suite|document_package|single -> pipeline + drain
│   └── receipt.py          # the ONE ArtifactReceipt union, ConformanceVerdict, roster-derived kind/keys, and Metrics.record seam
└── package/                # content-addressed compression / archive / delta over one shared bundle vocabulary
    ├── bundle.py           # shared Bundle/CompressionAlgo/CodecProfile/BundleManifest vocabulary plus BundleEvidence projection
    ├── codec.py            # zstandard/lz4/brotli/zlib-ng single-blob compression composing bundle; the gzip_ng block-fan band with crc32_combine
    ├── archive.py          # py7zr/stream-zip/stream-unzip archive containers; THE reproducible-ZIP owner (scene bundles arrive as DATA parents)
    └── delta.py            # detools binary diff/patch; parent-keyed delta nodes against the base bundle key
```

The engine reads as high-order domains. `document` owns one `DocumentNode` tree that `document/emit` lowers FROM and `document/lens` recovers TO over one node algebra, with `DocumentDelta` diff/merge once and an extracted-tree corpus keyed into the runtime columnar lane; `document/egress` seals and navigates the emitted PDF, `document/tagged` authors the PD/UA marked-content tree from the `document/model` `StructureNode`/`StructEltKind` family, and `document/report` composes reproducible notebooks back into the tree. `visualization` is the data->visual axis: `visualization/chart` splits spec authoring from host-free export (the vegafusion data pre-pass riding the export page beside the render it feeds), `visualization/table` owns publication tables, and `visualization/diagram` is the data-driven diagram engine that lays a graph out (`layout`, five engines — `rustworkx`/`fast-sugiyama`/`pyelk`/`kiwisolver`/`grandalf` — over the `Force`/`Radial`/`Layered`/`Projected`/`Constrained` policies, emitting all ten `DiagramKind`: the AEC sun-path/circulation/stacking/program/site plus the general node-link/flowchart/entity-relation/Sankey/section-callout, threading `NodeShape`/`Port`/`weight`/`EndCap`) and emits it as named-layer SVG or an editable `drawpyo` `.drawio` (`draw`, the `DrawTarget` selector, labels shaped through `typography/shape` and formulas through `typography/math`) from a bounded primitive vocabulary (`glyphset`: node/edge/swimlane/annotation/marker/area/fragment — the true-polygon area mark and the solar-furniture fragment beside the five topology marks); `schematic` is the named-symbol producer over the schemdraw catalog for the diagram class the marks cannot express, and `solar` is the pvlib SPA ephemeris-and-furniture owner the `SUN_PATH` layouts compose. `drawing` is the AEC drawing-production plane that sits on top of the pub/print substrate: `regime` is the s1 closed owned-vocabulary + BIND substrate (ISO 128 line types/weights, ISO 128-50 materials bound onto pattern-plane fills, ISO 5455 scales and ISO 216 paper by derivation, ISO 3098 lettering folded over the font-metrics value, the three-schema ISO 13567/AIA/NCS `LayerName` codec and the NCS `SheetId` codec, LCh discipline pens) read by every drawing, composition, specification, and delivery consumer, minting no receipt exactly as `glyphset` mints none; `standard` is its `ezdxf` symbol-table LOWERING (seed/graphics/dimstyle/hatch plus the DXF-native ACI and fill-code correspondences), `dimension` is the ISO 129-1 dimensioning producer over the closed `DimOp` family (linear/aligned/angular/radial/diameter/ordinate/arc/chain/baseline + tolerance) dual-lowered over `DimTarget` onto the categorical-best `ezdxf` `add_*_dim().render()` native path (DXF/SVG/PDF, ISO tolerance as `dimtol`/`dimlim`/`MTextEditor.stack`) or a `LAYERED` decomposition (`ezdxf.math` geometry + self-contained filled/stroked ISO 129-1 terminator marks + `ziafont` ISO 3098 text + `ziamath` tolerance math into named `export/layered` layers, all penned by the discipline sRGB, the tapered-terminator `graphic/vector/region` stroke-to-outline, composed via `symbol`, a pending growth axis), the `override=` DIM-variables from `standard` and the dimension-line offset stack solved by `kiwisolver`, `symbol` is the drawing-symbol vocabulary dual-lowered to `drawsvg` groups and `ezdxf` blocks that feeds the `composition/sheet` graphic cells, `annotate` is the ISO 128-2 leader/keynote/flag-note/general-note/revision-cloud producer dual-lowered to `drawsvg` named layers and `ezdxf` multileaders (composing `symbol`'s `SymbolStyle`, `typography`'s Knuth-Plass line break, and `kiwisolver` keynote-column routing), `detail` is the detail-management owner over detail callouts, a content-keyed `ezdxf` detail-library block store (one block authored once, placed N times), and the `rustworkx` sheet cross-reference DAG (cycle rejection, transitive reduction, revision-impact closure), and `schedule` is the AEC-scheduling owner that lowers a settled QTO/schedule frame (or an ISO legend enumerated from `standard`) into `visualization/table` as a standards-formatted door/window/room-finish/equipment/panel/structural schedule, the `csharp:Rasm.Bim` quantity-takeoff schedule (measure/cost columns closed on grand-summary `totals` sums), or a line-type/hatch/discipline legend, contributing the shared `ArtifactReceipt.Schedule` case. `specification` is the CSI construction-specification plane on that same substrate: `section` is the SectionFormat 3-part (General/Products/Execution) + PageFormat multi-level-numbering producer authored INTO the `document/model` `DocumentNode` tree, validating its MasterFormat number and article roster against the owned SectionFormat vocabularies and folding the specifier-note / unresolved-fill-in / reference-reconciliation `SpecVerdict`, contributing the `ArtifactReceipt.Spec` case, and `classify` is the pure MasterFormat/UniFormat/OmniClass classification substrate — exact-cardinality vocabularies, the `DERIVED_LOGIC` crosswalk, and the drawing<->spec `ReferenceIndex` resolver every spec/schedule/drawing reads — minting no receipt exactly as `standard` mints none. `delivery` is the ISO 19650 delivery plane that issues the set: `register` is the information-container register / sheet-index / container-metadata owner over the exact NA Table NA.1 suitability (`S0`-`S7`/`A`-`B`/documented) and revision (`P`/`C`) vocabularies, lowering to a `great-tables` sheet-index publication table, a structured `lxml` COBie/BS 1192 container XML, an `xlsxwriter`/`openpyxl` register spreadsheet, and the accumulating `RegisterEvidence` coverage audit, contributing `ArtifactReceipt.Register`, and `transmittal` is the issue-for-construction orchestrator composing `composition/imposition` + `package/archive` + `exchange/conformance` (PAdES) + `exchange/credential` (C2PA sheet-lineage) + `register` into the signed, content-addressed issue, contributing `ArtifactReceipt.Transmittal`. `graphic` is the 2D primitive toolkit every visual and document plane composes — `raster` (io/measure over the `process` vocabulary-and-behavior owner), the `vector` sub-plane (`path` the svgelements metric substrate, `region` the pathops/drawsvg/resvg emission owner with metric text-on-path, `pattern` the repeating-fill generator), the `marks` codec (the shared `mark` vocabulary plus the encode/decode halves), `color` (the receipt-free `derive` value source plus the `managed` pyvips/libvips ICC/LUT egress, its one async-forcing worker leg), `style` (the theme-as-DATA SELECT owner whose one row set carries an office's type system, stroke hierarchy, palette seeds, ground/entourage conventions, and sheet family — switching styles is switching one `Theme`), and `layer` (the `LayerPlan` semantic tree every layered producer projects INTO and every layered exporter composes OUT of). `typography` owns the font-binary transform, glyph shaping, mathematical typesetting (`math`, the one ziamath owner every formula consumer routes through), and line-layout over one shared `PositionedGlyphRun` seam. `composition` assembles placed figures, architectural sheet-sets, and imposition; the editable named-layer hand-off then routes to `export` (`layered` OCG/SVG/ORA/PSD/layered TIFF + `indesign` SimpleIDML + `dxf` ezdxf CAD exchange), while the descriptive and trust boundary routes to `exchange` (`metadata`, `credential`, `conformance`, `detect`). `media` muxes the temporal artifacts, `scene` renders and exports the 3D plane (`render` owns the `SceneTarget` file-target vocabulary its `SceneOp.Export` keys; `export` composes it downward), `core` is the production spine (`receipt`+`plan` the s1 law, `issue` the s3 constructing owner every host invokes; `format` dissolved into `document/emit`'s absorbed one-context fan), and `package` is the content-addressed compression close over the shared `bundle` vocabulary.

`core/receipt` is the shared owner every visual and document sub-domain contributes one case to, never a parallel per-producer receipt rail, and the one receipt-fold edge the runtime `execution/lanes` reuse-fabric elision and the runtime `observability/metrics` recorder both consume — `contribute` records the numeric `_facts` scalars through the one polymorphic `Metrics.record(measure, domain="artifact", kind=...)` arm onto the landed `artifact.byte_volume`/`artifact.compression_ratio` distribution rows, render DURATION staying the runtime `Metrics.measured` aspect's fact, never a receipt's; outward figure handoff is LANDED: the `compute/graduation` `HandoffAxis` union carries the artifacts-origin `artifact` case and `core/receipt.graduates` projects any `ArtifactReceipt` into it keyed by `ContentIdentity` under the governed residual-ceiling policy row (a caller's tighter ceiling the override), `model_asset` staying a compute-own subject figures never ride; the artifacts sources re-mint no canonical concept so the runtime `evidence` `Structural.drift` query stays clean. `graphic/color/derive` is the one upstream color source the visual sub-domains pull palettes from rather than each engine picking color ad hoc, while `graphic/color/managed` is the downstream ICC/LUT/CCTF egress the raster and document outputs route through. The host-free posture is the structural axis cutting every sub-domain: `vl-convert` is the primary host-free chart export and `lets-plot` is the second host-free engine, and the great-tables Selenium path is the one remaining gated host path, never the default. The engine selection is the second structural axis: heavyweight render, raster, compression, text-layout, and 3D arms dispatch onto the runtime subprocess seam (`anyio.to_process.run_sync`) instead of importing provider-heavy modules into the core runtime path.

## [02]-[SEAMS]

```text seams
*                            ←  python:runtime                          # [CONTENT_KEY]: ContentIdentity.of projects the ContentKey
*                            →  python:runtime                          # [RECEIPT]: ArtifactReceipt contribution
document/model               →  python:data/tabular                     # [WIRE]: to_corpus_row flat record
document/model               →  python:artifacts/document/tagged        # [SHAPE]: StructureNode/StructEltKind structure tree + FigureNode.alt
graphic/color/derive         →  python:artifacts/visualization          # [PROJECTION]: palette values feed chart/table/diagram
graphic/color/derive         →  python:artifacts/scene                  # [PROJECTION]: palette to scene colormap
graphic/color/derive         →  python:artifacts/graphic/color/managed  # [PROJECTION]: tone-curve / space provenance to the managed egress
graphic/color/managed        →  python:artifacts/graphic/raster         # [PROJECTION]: color-managed raster consumed by the process arms
graphic/color/managed        →  python:artifacts/document               # [PROJECTION]: ICC-profiled raster consumed by document/PDF output
core/receipt                 ←  python:runtime/execution                # [RECEIPT]: reuse-fabric elision ContentKey hit/miss
core/receipt                 ←  python:runtime/observability            # [RECEIPT]: Metrics.record(domain="artifact", kind) rows
core/receipt                 →  python:compute/graduation               # [GRADUATION]: graduates projects ArtifactReceipt into HandoffAxis(artifact=)
core/receipt                 ←  python:artifacts/core/plan              # [RECEIPT]: per-artifact contribute folds walked into the elision plan
visualization/table          ←  python:data/tabular                     # [SHAPE]: QualityProfile frame rendered by the great-tables tier
visualization/diagram/layout ←  python:data/tabular                     # [SHAPE]: graph adjacency/attributes feed the layout
graphic/raster/measure       →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Preview pixel + perceptual-metric facts
graphic/vector/path          →  python:artifacts/composition            # [SHAPE]: svgelements metric/affine substrate consumed by placement
graphic/vector/region        ←  python:artifacts/typography/shape       # [SHAPE]: text-on-path lays PositionedGlyphRun outlines via region.text_path
graphic/marks/encode         →  python:artifacts/export                 # [PROJECTION]: emitted mark SVG bound as a named layer
visualization/diagram/draw   →  python:artifacts/export                 # [PROJECTION]: laid-out diagram SVG bound as named layers
drawing/regime               →  python:artifacts/composition            # [SHAPE]: ScaleRatio.ratio + SheetId.compose read by the sheet title-block
drawing/regime               →  python:artifacts/graphic/color/derive   # [SHAPE]: discipline LCh pen VALUES resolved to any target model by derive
drawing/dimension            ←  python:artifacts/drawing/standard       # [SHAPE]: Standard.dimstyle override and ISO scale seed
drawing/dimension            →  python:artifacts/composition            # [PROJECTION]: dimensioned SVG/PDF bytes feed the sheet FigurePlacement
drawing/dimension            ←  python:artifacts/graphic/vector/path    # [SHAPE]: point-at-distance tick spacing plus stroke outline
drawing/dimension            →  python:artifacts/export                 # [PROJECTION]: dimension layers bind as OCG/SVG
drawing/dimension            →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Drawing dimension/dimstyle/extent/byte facts
drawing/symbol               →  python:artifacts/composition            # [PROJECTION]: single-mark PNG feeds sheet cells
drawing/symbol               ←  python:artifacts/graphic/vector/region  # [SHAPE]: resvg rasterizes SVG to PNG after pathops growth
drawing/symbol               →  python:artifacts/export                 # [PROJECTION]: named-layer symbol groups bound as OCG/SVG layers
drawing/symbol               →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Drawing kind/entity/extent/byte facts
drawing/detail               ←  python:artifacts/drawing/symbol         # [SHAPE]: SymbolKind bubble vocabulary + SymbolStyle mark-style the callout
drawing/detail               ←  python:artifacts/drawing/regime         # [SHAPE]: Discipline/ScaleRatio ISO layer/scale codes on the DetailRef
drawing/detail               →  python:artifacts/composition            # [PROJECTION]: detail blocks and callout leaders place on sheets
drawing/detail               →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Drawing callout/entity/byte facts
drawing/annotate             ←  python:artifacts/drawing/symbol         # [SHAPE]: SymbolStyle mark-style the leader/keynote/note marks compose
drawing/annotate             ←  python:artifacts/drawing/regime         # [SHAPE]: Terminator/LineWeight/TextHeight/LayerName owned ISO vocabulary
drawing/annotate             ←  python:artifacts/typography/layout      # [SHAPE]: LineBrokenRun Knuth-Plass general-note line break
drawing/annotate             ←  python:artifacts/typography/shape       # [SHAPE]: PositionedGlyphRun general-note shaped run
drawing/annotate             ←  python:artifacts/graphic/vector/region  # [SHAPE]: skia-pathops outline/boolean for the revision-cloud filled band
drawing/annotate             →  python:artifacts/export                 # [PROJECTION]: named-layer annotation groups bound as OCG/SVG layers
drawing/annotate             →  python:artifacts/composition            # [PROJECTION]: placed annotation layers onto the sheet
drawing/annotate             →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Drawing leader/keynote/note/cloud facts
drawing/annotate             ←  csharp:Rasm.Fabrication/Spec/tolerance  # [DEMAND]: GD&T rendering consumes Fabrication tolerance vocabulary
drawing/schedule             ←  python:data/tabular                     # [SHAPE]: QTO/schedule frame shaped by ScheduleKind
drawing/schedule             ←  python:artifacts/drawing/regime         # [SHAPE]: LineType/HATCH_BIND/Discipline rows drive legends
drawing/schedule             →  python:artifacts/visualization/table    # [PROJECTION]: AEC schedule and legend frames lower into TablePlan
drawing/schedule             →  python:artifacts/composition            # [PROJECTION]: rendered schedule/legend table bytes placed on the sheet
drawing/schedule             →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Schedule kind/rows/columns/byte facts
specification/section        →  python:artifacts/document/model         # [PROJECTION]: Spec.to_document lowers SectionFormat into DocumentNode
specification/section        →  python:artifacts/specification/classify # [SHAPE]: ClassCode admits MasterFormat section and division
specification/section        →  python:artifacts/visualization/table    # [PROJECTION]: BIM citation frames render through TablePlan
specification/section        →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Spec section/division/part/article/byte facts
specification/classify       ←  python:artifacts/drawing/regime         # [SHAPE]: ISO/NCS discipline vocabulary on ReferenceIndex
typography/font              →  python:artifacts/document               # [PROJECTION]: subsetted/instanced font bytes for FONT_EMBED
typography/font              →  python:artifacts/typography/shape       # [SHAPE]: face/variation location + embed-audit precondition
typography/shape             →  python:artifacts/document               # [PROJECTION]: PositionedGlyphRun text placement
typography/shape             →  python:artifacts/composition            # [PROJECTION]: PositionedGlyphRun annotation
typography/shape             →  python:artifacts/graphic/vector/region  # [SHAPE]: PositionedGlyphRun.on_path outlines feed text_path
typography/layout            →  python:artifacts/document               # [SHAPE]: LineLayout.broken() feeds document lay rail
composition/compose          →  python:artifacts/export                 # [PROJECTION]: placed multi-source layout handed to named-layer egress
composition/sheet            ←  python:artifacts/drawing/regime         # [SHAPE]: ScaleRatio, SheetId, and Viewport scale feed sheets
composition/sheet            →  python:artifacts/visualization/table    # [PROJECTION]: revision and drawing-list rows feed TablePlan
composition/sheet            →  python:artifacts/delivery/register      # [PROJECTION]: SheetSet.registered builds SheetEntry rows
composition/sheet            →  python:artifacts/export                 # [PROJECTION]: frame + placed figures projected as named Layer rows
composition/imposition       →  python:artifacts/document               # [PROJECTION]: n-up/booklet sheet handed to the PDF assembler
export/layered               ←  python:artifacts/composition            # [PROJECTION]: placed multi-source layout + named-layer source graphics
export/layered               →  python:artifacts/core/receipt           # [RECEIPT]: LayeredExport emits named-layer evidence
export/indesign              ←  python:artifacts/composition            # [PROJECTION]: placed layout bound into the IDML template
export/indesign              →  python:artifacts/core/receipt           # [RECEIPT]: Idml.emit ArtifactReceipt.Office IDML-package evidence
export/dxf                   →  python:artifacts/composition/sheet      # [PROJECTION]: PyMuPdfBackend one-page PDF placed via show_pdf_page
export/dxf                   →  python:artifacts/graphic/vector/path    # [SHAPE]: make_path/flatten/fragment bridge at vertex/d wire
export/dxf                   →  python:data/spatial/geospatial          # [WIRE]: GeoProxy GeoJSON wire; CRS authority stays with data
export/dxf                   →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Cad
exchange/metadata            →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt descriptive-metadata facts
exchange/credential          ←  python:runtime                          # [CONTENT_KEY]: ContentKey
exchange/credential          →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Credential content-credential facts
exchange/credential          →  csharp:Rasm.Persistence                 # [CONTENT_KEY]: signed-artifact binding decodes XxHash128 seed
exchange/conformance         ←  python:artifacts/document               # [PROJECTION]: emitted PDF input, contributing ArtifactReceipt.Verdict
exchange/conformance         ←  python:artifacts/document/tagged        # [RECEIPT]: structural-conformance result folded into the verdict
graphic/raster/io            →  python:artifacts/exchange/detect        # [BOUNDARY]: s1 format-ID substrate imported downward at the profiled ingress
document/lens                →  python:artifacts/exchange/detect        # [BOUNDARY]: format-ID pre-flight at the ingest boundary (detect s1)
document/tagged              ←  python:artifacts/document/model         # [SHAPE]: StructureNode/StructEltKind tree authored to marked content
document/tagged              →  python:artifacts/exchange               # [RECEIPT]: structural-conformance result folded into the conformance verdict
document/tagged              →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Egress/Pdf structural evidence
scene/render                 →  python:artifacts/media                  # [SHAPE]: rgb24 frame sequence via VideoFrame.from_ndarray, ContentKey-keyed
scene/export                 ⇄  python:geometry/mesh                    # [BOUNDARY]: scene export and mesh-file codec stay separate
media/container              ←  python:artifacts/scene                  # [SHAPE]: rgb24 frames ingest through VideoFrame.from_ndarray
media/container              →  python:artifacts/core/receipt           # [RECEIPT]: Media container/codec encode facts
media/container              ←  python:artifacts/media/filtergraph      # [SHAPE]: Transcode build_graph over native/substitute filters
media/filtergraph            →  python:artifacts/core/receipt           # [RECEIPT]: filter-node count rides Media band
media/audio                  →  python:artifacts/core/receipt           # [RECEIPT]: Media audio encode and EBU R128 facts
media/timeline               ←  python:artifacts/media/container        # [SHAPE]: seek/decode/encode/open-sink capsule
media/timeline               ←  python:artifacts/media/filtergraph      # [SHAPE]: cross_dissolve xfade substitute + link_clips concat/amix
media/timeline               ←  python:artifacts/media/audio            # [SHAPE]: _decode_audio for the xfade acrossfade audio leg
media/timeline               →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media clip/segment counts + lossless-vs-reencode
media/subtitle               ←  python:artifacts/media/container        # [SHAPE]: passthrough mux and burn-in compose media profile
media/subtitle               ←  python:artifacts/media/filtergraph      # [SHAPE]: filters_available selects overlay/soft-sub native vs numpy
media/subtitle               ←  python:artifacts/typography/shape       # [SHAPE]: styled fragments rasterize through shaped runs
media/subtitle               →  python:artifacts/core/receipt           # [RECEIPT]: Media subtitle event/style counts
media/analysis               ←  python:artifacts/media/container        # [SHAPE]: av.open read capsule plus MediaFault deployment
media/analysis               ←  python:artifacts/media/audio            # [SHAPE]: _decode_audio Pcm-block ingest
media/analysis               ←  python:artifacts/media/filtergraph      # [SHAPE]: filters_available probe selects native filter vs numpy/measure
media/analysis               ←  python:artifacts/graphic/raster/measure # [SHAPE]: structural_similarity frame-to-frame scene-detection substitute
media/analysis               →  python:artifacts/graphic/raster/io      # [PROJECTION]: render_png + montage waveform/spectrogram/contact-sheet
media/analysis               →  python:compute/analysis/signal          # [SHAPE]: SignalOp.Spectral scipy spectrogram substitute (cross-branch)
media/analysis               →  python:compute/analysis/transform       # [SHAPE]: analytic-signal centroid/envelope (cross-branch)
media/analysis               →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media scene-cut/silence-span/LUFS facts band
media/synthesis              →  python:artifacts/media/audio            # [SHAPE]: _encode_audio numpy buffer enters Pcm/Master mux
media/synthesis              →  python:compute/analysis/signal          # [SHAPE]: SignalOp.Filter/Resample band-limit + transform spectral QA
media/synthesis              →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media synthesis fundamental_hz/waveform/duration
delivery/register            ←  python:artifacts/composition/sheet      # [PROJECTION]: SheetSet.registered builds SheetEntry rows
delivery/register            →  python:artifacts/visualization/table    # [PROJECTION]: RegisterOp.Index lowers Register.frame
delivery/register            →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Register
delivery/transmittal         →  python:artifacts/composition/imposition # [PROJECTION]: constituent sheets laid into the press-form plan-set
delivery/transmittal         →  python:artifacts/package/archive        # [PROJECTION]: plan-set, register, and receipts seal into archive
delivery/transmittal         →  python:artifacts/exchange/conformance   # [PROJECTION]: PAdES-LTA signs issue-for-construction PDF
delivery/transmittal         →  python:artifacts/exchange/credential    # [PROJECTION]: C2PA sheet-lineage provenance binds cover and sheets
delivery/transmittal         ←  python:artifacts/delivery/register      # [PROJECTION]: issued index and audit compose into transmittal
delivery/transmittal         →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Transmittal
core/plan                    ←  python:runtime/execution                # [CONTENT_KEY]: session-lane elision keyed by ContentKey
package/codec                ←  python:runtime                          # [CONTENT_KEY]: ContentKey including the DELTA parent-key from-image
package/delta                ←  python:runtime                          # [CONTENT_KEY]: DELTA parent-key from-image
drawing/regime               ←  python:artifacts/graphic/vector/pattern # [SHAPE]: HATCH_BIND rows compose PatternSpec presets + HatchFill kinds
drawing/regime               ←  python:artifacts/typography/font        # [SHAPE]: FaceMetrics cap/x-height value folded by regime.lettering()
drawing/standard             ←  python:artifacts/drawing/regime         # [SHAPE]: vocabulary + bind rows lowered onto the ezdxf symbol tables
drawing/standard             ←  python:artifacts/graphic/vector/pattern # [SHAPE]: to_dxf definition rows rendered by Hatch.set_pattern_fill
graphic/layer                ←  python:artifacts/drawing/regime         # [SHAPE]: ISO 13567 LayerName fields compose AEC names
graphic/layer                →  python:artifacts/export                 # [PROJECTION]: LayerPlan tree composed by layered/indesign writers
graphic/style                ←  python:artifacts/drawing/regime         # [SHAPE]: stroke hierarchy selects LineWeight; lettering snaps to metrics
graphic/style                ←  python:artifacts/graphic/color/derive   # [PROJECTION]: PaletteSpec seeds resolve through the derive Palette factory
graphic/style                →  python:artifacts/composition/sheet      # [SHAPE]: SheetFamily variant + PageMaster grid rows read by the sheet plane
graphic/style                →  python:artifacts/document/emit          # [SHAPE]: page-master + running-head type roles read by the @page lowering
typography/math              ←  python:artifacts/typography/shape       # [SHAPE]: plain runs shape through text engine; math owns formulas
typography/math              →  python:artifacts/drawing                # [PROJECTION]: Fragment SVG + seat() baseline consumed by dimension/annotate
typography/math              →  python:artifacts/visualization/diagram  # [PROJECTION]: formula fragments consumed by draw labels
visualization/diagram/layout ←  python:artifacts/visualization/diagram/solar # [SHAPE]: project() positions SUN_PATH marks; furniture() draws chart
visualization/diagram/schematic → python:artifacts/core/receipt         # [RECEIPT]: ArtifactReceipt.Diagram schematic kind/symbol/edge facts
document/emit                →  python:artifacts/exchange/detect        # [BOUNDARY]: TemplatePayload template band format-IDs before any engine
core/issue                   →  python:artifacts/delivery/transmittal   # [PROJECTION]: sheet_set modality folds the transmittal emit() node set
core/issue                   →  python:artifacts/document/emit          # [PROJECTION]: document_package folds bound() format nodes + egress terminal
core/issue                   →  python:artifacts/visualization/diagram  # [PROJECTION]: diagram_suite folds draw.emit() per kind
core/issue                   →  python:artifacts/core/plan              # [SHAPE]: ArtifactPipeline.of construction + front-by-front lane drain
```
