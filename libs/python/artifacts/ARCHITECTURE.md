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
│       ├── layout.py       # 5-engine coordinate assignment (rustworkx/fast-sugiyama/pyelk/kiwisolver/grandalf) over Force/Radial(circular|shell)/Layered/Projected/Constrained, emitting all 10 DiagramKind (AEC sun-path/circulation/stacking/program/site + general node-link/flowchart/entity-relation/sankey/section-callout), threading NodeShape/Port/weight
│       ├── draw.py         # drawsvg named-layer SVG (ziafont/ziamath <path> label/formula outlining) + drawpyo editable .drawio, over the DrawTarget egress selector
│       └── glyphset.py     # bounded data-driven diagram-primitive vocabulary (node/edge/swimlane/annotation/marker marks + NodeShape/Port/weight topology + TextAnchor), general/technical + AEC
├── drawing/                # AEC drawing-production plane: owned ISO/NCS drafting vocabularies + ISO 129-1 dimensions + drawing symbols + detail cross-references + schedules
│   ├── standard.py         # ISO 128/129-1/3098/5455/13567 + NCS/AIA owned-vocabulary substrate lowered onto ezdxf symbol tables
│   ├── dimension.py        # ISO 129-1 dimensioning producer (linear/aligned/angular/radial/diameter/ordinate/arc/chain/baseline + tolerance) lowered onto ezdxf add_*_dim + a layered decomposition
│   ├── symbol.py           # AEC drawing-symbol vocabulary (section/elevation/detail/grid/north/scale/revision) dual-lowered to drawsvg + ezdxf blocks
│   ├── annotate.py         # ISO 128-2 leaders/keynotes/flag-notes + Knuth-Plass general notes + ziamath formula notes + revision clouds, dual-lowered to drawsvg + ezdxf multileaders
│   ├── detail.py           # detail callouts + content-keyed ezdxf detail-library block store + rustworkx sheet cross-reference DAG
│   └── schedule.py         # AEC schedule templates (door/window/room-finish/equipment/panel/structural + BIM QTO takeoff) + ISO legends lowered into visualization/table
├── specification/          # CSI construction-specification plane: SectionFormat 3-part sections + MasterFormat/UniFormat/OmniClass classification
│   ├── section.py          # CSI SectionFormat 3-part (General/Products/Execution) + PageFormat numbering producer, authored INTO the DocumentNode tree; contributes ArtifactReceipt.Spec
│   └── classify.py         # MasterFormat/UniFormat/OmniClass owned classification vocabularies + crosswalk + drawing<->spec ReferenceIndex resolver (pure substrate, no receipt)
├── delivery/               # ISO 19650 delivery plane: information-container register + issue-for-construction transmittal
│   ├── register.py         # ISO 19650 drawing-register/sheet-index/container-metadata owner (S0-S7/A-B/P-C owned vocabularies) over great-tables/xlsxwriter/openpyxl/lxml/polars; contributes ArtifactReceipt.Register
│   └── transmittal.py      # ISO 19650 transmittal/issue-for-construction orchestrator composing imposition + archive + credential + conformance + register; contributes ArtifactReceipt.Transmittal
├── graphic/                # 2D graphic-primitive toolkit every visual + document plane composes
│   ├── raster/
│   │   ├── io.py           # pillow/pyvips IO/convert/thumbnail/montage working surface
│   │   ├── process.py      # scikit-image restoration/segmentation/morphology/transform engine
│   │   └── measure.py      # perceptual-quality metrics + region/feature/registration measurement
│   ├── vector.py           # svgelements/resvg-py SVG geometry primitive (parse/Path/Matrix/bbox), the layer-authoring substrate
│   ├── marks/
│   │   ├── encode.py       # segno/python-barcode/zxing-cpp machine-readable-mark generation
│   │   └── decode.py       # zxing-cpp read_barcodes decode inverse
│   └── color/
│       ├── derive.py       # colour-science + ColorAide palette/spectral/CAM16 derivation, the upstream color source
│       └── managed.py      # ICC/LUT/CCTF color-managed raster egress over colour-science core + pyvips/libvips ICC apply
├── typography/             # font binary + glyph shaping + line-layout
│   ├── font.py             # FontEngineering subset/instance/axis-catalog/outline/embed-audit owner over fonttools
│   ├── shape.py            # Shaping uharfbuzz text-shaping + python-bidi reorder + blackrenderer COLRv1 glyph-render
│   └── layout.py           # line-break (uniseg UAX#14) + hyphenation (pyphen) + Knuth-Plass paragraph layout
├── composition/            # assembling artifacts into pages/sheets
│   ├── compose.py          # post-render figure/section placement (scale-fit/n-up/crop/rotate/overlay) over svgelements/resvg/pillow
│   ├── sheet.py            # architectural single-sheet title-block/frame/field owner (exact ISO 5457 zones + ISO 7200 audit + ISO 5455 Viewport scale) + SheetSet multi-sheet numbering/register/drawing-list projection
│   └── imposition.py       # n-up / booklet / signature imposition over pymupdf show_pdf_page / pdfimpose
├── export/                 # editable layered hand-off for Illustrator/InDesign + DXF CAD exchange
│   ├── layered.py          # drawsvg named-layer SVG + pymupdf/pikepdf PDF OCG optional-content groups + psd-tools/PhotoshopAPI native PSD/PSB channel-stack + psdtags/tifffile layered TIFF + pyvips/lxml/stream-zip ORA
│   ├── indesign.py         # SimpleIDML IDML template-mutation hand-off
│   └── dxf.py              # ezdxf DXF New/Read/Recover/Render/Query/Bridge CAD-exchange owner (7-backend render lowering into composition/sheet + graphic/vector, DXF<->SVG<->GeoJSON<->glyph bridge), contributes ArtifactReceipt.Cad
├── exchange/               # metadata / provenance / format identification at the boundary
│   ├── metadata.py         # EXIF/IPTC/XMP/ICC descriptive metadata read/write over the MetaCarrier axis — pyexiftool (standing) + pyexiv2 (optional cp-gated) raster, pikepdf PDF, av MEDIA (iptcinfo3/libxmp superseded)
│   ├── credential.py       # c2pa-python content-credential Sign/Read/ReadFragment/Embed binding keyed by the content key
│   ├── conformance.py      # pyhanko PAdES sign/stamp/augment/reserve/audit close (signer-free /DocTimeStamp stamp, LTV augment, seed-value reserve, visible drawing-sheet seal) folding ConformanceVerdict
│   └── detect.py           # dual-engine format identification: puremagic in-process default + python-magic worker-band libmagic fallback
├── media/                  # temporal media: the 7-page container/codec/filter/timeline/subtitle/analysis/synthesis plane
│   ├── container.py        # av container spine: mux/demux capsule, video encode/mux, transcode/remux, HDR/color, HLS/DASH via io_open (absorbs the former video.py)
│   ├── filtergraph.py      # closed FilterNode owner + capability-detection native-vs-substitute routing over av.filter.filters_available
│   ├── audio.py            # av audio stream encode/mux/resample/master composing container + filtergraph
│   ├── timeline.py         # Trim/Concat/Segment/xfade non-linear editing over the container/filtergraph spine
│   ├── subtitle.py         # pysubs2 parse/convert/retime/restyle + passthrough-mux + typography/shape RGBA burn-in
│   ├── analysis.py         # waveform/spectrogram/loudness(ebur128)/silence/scene-detect/thumbnail, capability-routed native-vs-numpy/measure
│   └── synthesis.py        # numpy oscillator/noise/FM/AM/sweep/ADSR generation -> media/audio encode
├── scene/                  # 3D / spatial visualization
│   ├── render.py           # pyvista/vtk offscreen render + FieldFilter clip/slice/threshold/contour/warp pipeline + two-operand boolean CSG on the scene worker lane
│   ├── export.py           # glTF/VRML/OBJ/HTML scene-file export + orbit rgb24 frame seam
│   └── stage.py            # usd-core USD/USDZ stage authoring and composition
├── core/                   # production spine
│   ├── plan.py             # ArtifactPipeline content-keyed sub-graph-elision plan over the runtime session lane
│   ├── format.py           # TemplatePipeline one-context-many-format binding over docxtpl/typst/jinja2/python-pptx/odfpy/openpyxl/xlsxwriter (owns no engine, delegates to emit/report)
│   └── receipt.py          # shared kind-discriminated ArtifactReceipt family across every production mode
└── package/                # content-addressed compression / archive / delta
    ├── codec.py            # zstandard/lz4/brotli content-addressed compression
    ├── archive.py          # py7zr/stream-zip/stream-unzip archive containers
    └── delta.py            # detools binary diff/patch for incremental artifact delta bundles
```

The engine reads as fourteen high-order domains. `document` owns one `DocumentNode` tree that `document/emit` lowers FROM and `document/lens` recovers TO over one node algebra, with `DocumentDelta` diff/merge once and an extracted-tree corpus keyed into the runtime columnar lane; `document/egress` seals and navigates the emitted PDF, `document/tagged` authors the PD/UA marked-content tree from the `document/model` `StructureNode`/`StructEltKind` family, and `document/report` composes reproducible notebooks back into the tree. `visualization` is the data->visual axis: `visualization/chart` splits spec authoring from host-free export and the vegafusion data pre-pass, `visualization/table` owns publication tables, and `visualization/diagram` is the data-driven diagram engine that lays a graph out (`layout`, five engines — `rustworkx`/`fast-sugiyama`/`pyelk`/`kiwisolver`/`grandalf` — over the `Force`/`Radial`/`Layered`/`Projected`/`Constrained` policies, emitting all ten `DiagramKind`: the AEC sun-path/circulation/stacking/program/site plus the general node-link/flowchart/entity-relation/Sankey/section-callout, threading `NodeShape`/`Port`/`weight`) and emits it as named-layer SVG or an editable `drawpyo` `.drawio` (`draw`, the `DrawTarget` selector, with `ziafont`/`ziamath` `<path>` label/formula outlining) from a bounded five-mark primitive vocabulary (`glyphset`: node/edge/swimlane/annotation/marker). `drawing` is the AEC drawing-production plane that sits on top of the pub/print substrate: `standard` is the closed owned-vocabulary substrate (ISO 128 line types/weights/hatches, ISO 5455 scales, ISO 3098 lettering, ISO 129-1 dimension styles, the ISO 13567/AIA/NCS layer-name + NCS sheet-identification codecs) lowered onto the real `ezdxf` symbol tables and read by every drawing producer, minting no receipt of its own exactly as `glyphset` mints none, `dimension` is the ISO 129-1 dimensioning producer over the closed `DimOp` family (linear/aligned/angular/radial/diameter/ordinate/arc/chain/baseline + tolerance) dual-lowered over `DimTarget` onto the categorical-best `ezdxf` `add_*_dim().render()` native path (DXF/SVG/PDF, ISO tolerance as `dimtol`/`dimlim`/`MTextEditor.stack`) or a `LAYERED` decomposition (`ezdxf.math` geometry + self-contained filled/stroked ISO 129-1 terminator marks + `ziafont` ISO 3098 text + `ziamath` tolerance math into named `export/layered` layers, all penned by the discipline sRGB, the tapered-terminator `graphic/vector` `VectorOp.Outline` stroke-to-outline a pending growth axis), the `override=` DIM-variables from `standard` and the dimension-line offset stack solved by `kiwisolver`, `symbol` is the drawing-symbol vocabulary dual-lowered to `drawsvg` groups and `ezdxf` blocks that feeds the `composition/sheet` graphic cells, `annotate` is the ISO 128-2 leader/keynote/flag-note/general-note/revision-cloud producer dual-lowered to `drawsvg` named layers and `ezdxf` multileaders (composing `symbol`'s `SymbolStyle`, `typography`'s Knuth-Plass line break, and `kiwisolver` keynote-column routing), `detail` is the detail-management owner over detail callouts, a content-keyed `ezdxf` detail-library block store (one block authored once, placed N times), and the `rustworkx` sheet cross-reference DAG (cycle rejection, transitive reduction, revision-impact closure), and `schedule` is the AEC-scheduling owner that lowers a settled QTO/schedule frame (or an ISO legend enumerated from `standard`) into `visualization/table` as a standards-formatted door/window/room-finish/equipment/panel/structural schedule, the `csharp:Rasm.Bim` quantity-takeoff schedule (measure/cost columns closed on grand-summary `totals` sums), or a line-type/hatch/discipline legend, contributing the shared `ArtifactReceipt.Schedule` case. `specification` is the CSI construction-specification plane on that same substrate: `section` is the SectionFormat 3-part (General/Products/Execution) + PageFormat multi-level-numbering producer authored INTO the `document/model` `DocumentNode` tree, validating its MasterFormat number and article roster against the owned SectionFormat vocabularies and folding the specifier-note / unresolved-fill-in / reference-reconciliation `SpecVerdict`, contributing the `ArtifactReceipt.Spec` case, and `classify` is the pure MasterFormat/UniFormat/OmniClass classification substrate — exact-cardinality vocabularies, the `DERIVED_LOGIC` crosswalk, and the drawing<->spec `ReferenceIndex` resolver every spec/schedule/drawing reads — minting no receipt exactly as `standard` mints none. `delivery` is the ISO 19650 delivery plane that issues the set: `register` is the information-container register / sheet-index / container-metadata owner over the exact NA Table NA.1 suitability (`S0`-`S7`/`A`-`B`/documented) and revision (`P`/`C`) vocabularies, lowering to a `great-tables` sheet-index publication table, a structured `lxml` COBie/BS 1192 container XML, an `xlsxwriter`/`openpyxl` register spreadsheet, and the accumulating `RegisterEvidence` coverage audit, contributing `ArtifactReceipt.Register`, and `transmittal` is the issue-for-construction orchestrator composing `composition/imposition` + `package/archive` + `exchange/conformance` (PAdES) + `exchange/credential` (C2PA sheet-lineage) + `register` into the signed, content-addressed issue, contributing `ArtifactReceipt.Transmittal`. `graphic` is the 2D primitive toolkit every visual and document plane composes — `raster` (io/process/measure), the `vector` SVG-geometry primitive, the `marks` encode/decode codec, and `color` (the upstream `derive` palette source plus the `managed` pyvips/libvips ICC/LUT egress, its one async-forcing worker leg). `typography` owns the font-binary transform, glyph shaping, and line-layout over one shared `PositionedGlyphRun` seam. `composition` assembles placed figures, architectural sheet-sets, and imposition; the editable named-layer hand-off then routes to `export` (`layered` OCG/SVG/ORA/PSD/layered TIFF + `indesign` SimpleIDML + `dxf` ezdxf CAD exchange), while the descriptive and trust boundary routes to `exchange` (`metadata`, `credential`, `conformance`, `detect`). `media` muxes the temporal artifacts, `scene` renders and exports the 3D plane, `core` is the production spine (`plan`/`format`/`receipt`), and `package` is the content-addressed compression close.

`core/receipt` is the shared owner every visual and document sub-domain contributes one case to, never a parallel per-producer receipt rail, and the one receipt-fold edge the runtime `execution/lanes` reuse-fabric elision and the runtime `observability/metrics` `MeterProvider` signal stream both consume; outward figure handoff to a sibling package travels only as the `compute/graduation` `HandoffAxis` model-asset case keyed by `ContentIdentity`, never a parallel per-artifact handoff, and the artifacts sources re-mint no canonical concept so the runtime `evidence` `Structural.drift` query stays clean. `graphic/color/derive` is the one upstream color source the visual sub-domains pull palettes from rather than each engine picking color ad hoc, while `graphic/color/managed` is the downstream ICC/LUT/CCTF egress the raster and document outputs route through. The host-free posture is the structural axis cutting every sub-domain: `vl-convert` is the primary host-free chart export and `lets-plot` is the second host-free engine, and the great-tables Selenium path is the one remaining gated host path, never the default. The engine selection is the second structural axis: heavyweight render, raster, compression, text-layout, and 3D arms dispatch onto the runtime subprocess seam (`anyio.to_process.run_sync`) instead of importing provider-heavy modules into the core runtime path.

## [02]-[SEAMS]

```text seams
*                            ←  python:runtime                          # [CONTENT_KEY]: ContentKey — ContentIdentity.of over a whole/merkle byte source is infallible, so every producer binds the projected ContentKey directly; the RuntimeRail rails only the canonical Struct-encode path, never a byte/merkle derivation
document/model               →  python:data/tabular                     # [WIRE]: to_corpus_row flat record
document/model               →  python:artifacts/document/tagged        # [NODE]: StructureNode/StructEltKind structure tree + FigureNode.alt
graphic/color/derive         →  python:data/tabular                     # [WIRE]: color palette arrays / appearance correlates
graphic/color/derive         →  python:artifacts/visualization          # [DERIVE]: ColorReceipt.coords palette to chart/table/diagram
graphic/color/derive         →  python:artifacts/scene                  # [DERIVE]: palette to scene colormap
graphic/color/derive         →  python:artifacts/graphic/color/managed  # [DERIVE]: tone-curve / space provenance to the managed egress
graphic/color/managed        →  python:artifacts/graphic/raster         # [MANAGED]: color-managed raster consumed by the process arms
graphic/color/managed        →  python:artifacts/document               # [MANAGED]: ICC-profiled raster consumed by document/PDF output
*                            →  python:runtime                          # [RECEIPT]: ArtifactReceipt contribution
core/receipt                 ←  python:runtime/execution                # [RECEIPT]: reuse-fabric elision ContentKey hit/miss
core/receipt                 ←  python:runtime/observability            # [RECEIPT]: MeterProvider signal stream
core/receipt                 ←  python:artifacts/core/plan              # [SIGNALS]: per-artifact contribute folds walked into the elision plan
visualization/table          ←  python:data/tabular                     # [SHAPE]: QualityProfile frame rendered by the great-tables tier
visualization/diagram/layout ←  python:data/tabular                     # [GRAPH]: graph adjacency/attributes feed the layout
graphic/raster/measure       →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Preview pixel + perceptual-metric facts
graphic/vector               →  python:artifacts/composition            # [GEOMETRY]: SVG geometry primitive consumed by placement
graphic/marks/encode         →  python:artifacts/export                 # [LAYERED]: emitted mark SVG bound as a named layer
visualization/diagram/draw   →  python:artifacts/export                 # [LAYERED]: laid-out diagram SVG bound as named layers
drawing/standard             →  python:artifacts/composition            # [SCALE]: ScaleRatio.ratio + SheetId.compose read by the sheet title-block
drawing/standard             →  python:artifacts/graphic/color/derive   # [DERIVE]: discipline ACI-resolved sRGB routed to the color engine
drawing/dimension            ←  python:artifacts/drawing/standard       # [DIMSTYLE]: Standard.dimstyle DIM-variable override + seed scaled by the ISO 5455 factor
drawing/dimension            →  python:artifacts/composition            # [SHEET]: dimensioned SVG/PDF bytes feed the sheet FigurePlacement drawing region
drawing/dimension            ←  python:artifacts/graphic/vector         # [VECTOR]: VectorOp.Outline / outline() tapered-terminator stroke-to-outline composed for a non-default tapered terminator (base terminator self-contained, the landed outline the refinement)
drawing/dimension            →  python:artifacts/export                 # [LAYERED]: named dimension-line/terminator/text/tolerance layers bound as OCG/SVG layers
drawing/dimension            →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Drawing dimension/dimstyle/extent/byte facts
drawing/symbol               →  python:artifacts/composition            # [SHEET]: single-mark PNG raster feeds the sheet NorthArrow.glyph/KeyPlan.figure cells
drawing/symbol               ←  python:artifacts/graphic/vector         # [GEOMETRY]: resvg rasterize SVG->PNG for the sheet-cell seam (skia-pathops boolean/offset growth axis)
drawing/symbol               →  python:artifacts/export                 # [LAYERED]: named-layer symbol groups bound as OCG/SVG layers
drawing/symbol               →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Drawing kind/entity/extent/byte facts
drawing/detail               ←  python:artifacts/drawing/symbol         # [BUBBLE]: SymbolKind bubble vocabulary + SymbolStyle mark-style the callout projects
drawing/detail               ←  python:artifacts/drawing/standard       # [STANDARD]: Discipline/ScaleRatio ISO layer/scale codes on the DetailRef
drawing/detail               →  python:artifacts/composition            # [SHEET]: detail-library block store + callout boundary/leader placed on the sheet
drawing/detail               →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Drawing callout/entity/byte facts
drawing/annotate             ←  python:artifacts/drawing/symbol         # [STYLE]: SymbolStyle mark-style the leader/keynote/note marks compose
drawing/annotate             ←  python:artifacts/drawing/standard       # [STANDARD]: Terminator/LineWeight/TextHeight/LayerName owned ISO vocabulary
drawing/annotate             ←  python:artifacts/typography/layout      # [LAYOUT]: LineBrokenRun Knuth-Plass general-note line break
drawing/annotate             ←  python:artifacts/typography/shape       # [SHAPE]: PositionedGlyphRun general-note shaped run
drawing/annotate             ←  python:artifacts/graphic/vector         # [VECTOR]: skia-pathops stroke-to-outline for the revision-cloud filled band
drawing/annotate             →  python:artifacts/export                 # [LAYERED]: named-layer annotation groups bound as OCG/SVG layers
drawing/annotate             →  python:artifacts/composition            # [SHEET]: placed annotation layers onto the sheet
drawing/annotate             →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Drawing leader/keynote/note/cloud facts
drawing/schedule             ←  python:data/tabular                     # [SHAPE]: QTO/schedule rows frame from Rasm.Bim shaped by the ScheduleKind template
drawing/schedule             ←  python:artifacts/drawing/standard       # [STANDARD]: LineType.pattern/HatchMaterial/Discipline ISO codes for the legend swatches
drawing/schedule             →  python:artifacts/visualization/table    # [LOWER]: AEC schedule/legend frame+ops lowered into the TablePlan.build render
drawing/schedule             →  python:artifacts/composition            # [FLAT]: rendered schedule/legend table bytes placed on the sheet
drawing/schedule             →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Schedule kind/rows/columns/byte facts
specification/section        →  python:artifacts/document/model         # [MODEL]: Spec.to_document lowers the SectionFormat 3-part tree into the DocumentNode SectionNode/BlockNode/RunNode tree
specification/section        →  python:artifacts/specification/classify # [CLASSIFY]: ClassCode MasterFormat section-number identity + division admitted through classify, never re-parsed
specification/section        →  python:artifacts/visualization/table    # [TABLE]: QTO/schedule citation frame from csharp:Rasm.Bim rendered by the table owner, never re-authored here
specification/section        →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Spec section/division/part/article/byte facts
specification/classify       ←  python:artifacts/drawing/standard       # [DISCIPLINE]: ISO 13567/NCS Discipline vocabulary on the ReferenceIndex SheetRef, composed never re-declared
typography/font              →  python:artifacts/document               # [FONT]: subsetted/instanced font bytes for FONT_EMBED
typography/font              →  python:artifacts/typography/shape       # [SHAPE]: face/variation location + embed-audit precondition
typography/shape             →  python:artifacts/document               # [DOCUMENT]: PositionedGlyphRun text placement
typography/shape             →  python:artifacts/composition            # [COMPOSE]: PositionedGlyphRun annotation
typography/shape             →  python:artifacts/graphic/vector         # [SHAPE]: PositionedGlyphRun.on_path() per-glyph outlines threaded along a baseline by vector.text_path (text-on-path, no pathops import in shape)
typography/layout            →  python:artifacts/document               # [LAYOUT]: line-broken paragraph runs for emission (LineLayout.broken() LineBrokenRun projection driven from a shaped run, beside the lay() content-key rail)
composition/compose          →  python:artifacts/export                 # [LAYERED]: placed multi-source layout handed to named-layer egress
composition/sheet            ←  python:artifacts/drawing/standard       # [SCALE]: ScaleRatio + SheetId sheet-number assembly + Viewport ISO 5455 scale read by the sheet owner
composition/sheet            →  python:artifacts/visualization/table    # [TABLE]: TitleBlock.revised + SheetSet.scheduled revision/drawing-list TablePlan lowered into the great-tables render
composition/sheet            →  python:artifacts/delivery/register      # [REGISTER]: SheetSet.registered (container, TitleBlock, suitability, revision) tuples the register builds into its OWN SheetEntry (from_title_block + externally-supplied ISO 13567 naming context) then of_sheets aggregates — not a direct of_sheets(SheetEntry) input
composition/sheet            →  python:artifacts/export                 # [LAYERED]: frame + placed figures projected as named Layer rows
composition/imposition       →  python:artifacts/document               # [IMPOSE]: n-up/booklet sheet handed to the PDF assembler
export/layered               ←  python:artifacts/composition            # [COMPOSE]: placed multi-source layout + named-layer source graphics
export/layered               →  python:artifacts/core/receipt           # [RECEIPT]: LayeredExport.emit ArtifactReceipt.Preview/Egress named-layer evidence (the ArtifactWork.work the ArtifactPipeline schedules, no module-level batch entry)
export/indesign              ←  python:artifacts/composition            # [TEMPLATE]: placed layout bound into the IDML template
export/indesign              →  python:artifacts/core/receipt           # [RECEIPT]: Idml.emit ArtifactReceipt.Office IDML-package evidence (the ArtifactWork.work the ArtifactPipeline schedules, no module-level batch entry)
export/dxf                   →  python:artifacts/composition/sheet      # [PLACE]: PyMuPdfBackend one-page PDF placed via show_pdf_page
export/dxf                   ⇄  python:artifacts/graphic/vector         # [VECTOR]: make_path/flattening OUT ↔ from_vertices/render_lines IN + SVGBackend SVG lowering at the vertex/d-string wire
export/dxf                   →  python:geospatial                       # [GEO]: addons.geo GeoProxy GeoJSON georeferenced wire (aligned; CRS authority stays the geospatial owner)
export/dxf                   →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Cad dxfversion/units/counts/layers/blocks/errors/fixes/byte facts
exchange/metadata            →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt descriptive-metadata facts
exchange/credential          ←  python:runtime                          # [CONTENT_KEY]: ContentKey
exchange/credential          →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Credential content-credential facts
exchange/credential          →  csharp:Rasm.Persistence                 # [CONTENT_KEY]: signed-artifact content-key binding decodes the XxHash128 seed
exchange/conformance         ←  python:artifacts/document               # [DOCUMENT]: emitted PDF input, contributing ArtifactReceipt.Verdict
exchange/conformance         ←  python:artifacts/document/tagged        # [ACCESS]: structural-conformance result folded into the verdict
exchange/detect              →  python:artifacts/document               # [DETECT]: media-type gate at the ingest boundary
document/tagged              ←  python:artifacts/document/model         # [NODE]: StructureNode/StructEltKind tree authored to marked content
document/tagged              →  python:artifacts/exchange               # [SIGN]: structural-conformance result folded into the conformance verdict
document/tagged              →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Egress/Pdf structural evidence
scene/render                 →  python:artifacts/media                  # [MEDIA]: rgb24 frame sequence via VideoFrame.from_ndarray, ContentKey-keyed
scene/export                 ⇄  python:geometry/mesh                    # [BOUNDARY]: visualization-scene export vs mesh-file codec, no shared owner
media/container              ←  python:artifacts/scene                  # [SCENE]: rgb24 frame sequence ingested via VideoFrame.from_ndarray (the renamed video owner)
media/container              →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media container/codec encode facts + per-page av/pysubs2 evidence band
media/container              ←  python:artifacts/media/filtergraph      # [FILTER]: Transcode composes build_graph — native/substitute-routed av.filter.Graph + numpy composite passes
media/filtergraph            →  python:artifacts/core/receipt           # [RECEIPT]: filter-node count in the composing producer's ArtifactReceipt.Media band (mints no receipt itself)
media/audio                  →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media audio encode facts (EBU R128 LUFS/true-peak/LRA in the band)
media/timeline               ←  python:artifacts/media/container        # [CONTAINER]: _seek/_decode_window/_decode_video/_encode_video/_open_sink capsule composed, opens no container
media/timeline               ←  python:artifacts/media/filtergraph      # [FILTER]: cross_dissolve xfade substitute + link_clips concat/amix
media/timeline               ←  python:artifacts/media/audio            # [DECODE]: _decode_audio for the xfade acrossfade audio leg
media/timeline               →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media clip/segment counts + lossless-vs-reencode strategy facts
media/subtitle               ←  python:artifacts/media/container        # [CONTAINER]: MediaProfile/MediaFault/_encode_video for passthrough-mux + burn-in encode
media/subtitle               ←  python:artifacts/media/filtergraph      # [FILTER]: filters_available probe selects overlay/soft-sub native vs numpy alpha-composite/burn-in
media/subtitle               ←  python:artifacts/typography/shape       # [SHAPE]: styled-fragment -> RGBA raster (uharfbuzz+python-bidi RTL, not un-bundled Pillow RAQM)
media/subtitle               →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media subtitle event/style counts in the facts band
media/analysis               ←  python:artifacts/media/container        # [CONTAINER]: av.open read capsule + MediaFault/_media_fault/_deployment
media/analysis               ←  python:artifacts/media/audio            # [DECODE]: _decode_audio Pcm-block ingest
media/analysis               ←  python:artifacts/media/filtergraph      # [FILTER]: filters_available probe selects native filter vs numpy/measure substitute
media/analysis               ←  python:artifacts/graphic/raster/measure # [MEASURE]: structural_similarity frame-to-frame scene-detection substitute
media/analysis               →  python:artifacts/graphic/raster/io      # [THUMBNAIL]: render_png + montage waveform/spectrogram/contact-sheet
media/analysis               →  python:compute/analysis/signal          # [SPECTRAL]: SignalOp.Spectral scipy spectrogram substitute (cross-branch)
media/analysis               →  python:compute/analysis/transform       # [ENVELOPE]: analytic-signal centroid/envelope (cross-branch)
media/analysis               →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media scene-cut/silence-span/LUFS facts band
media/synthesis              →  python:artifacts/media/audio            # [ENCODE]: _encode_audio numpy buffer -> container/mux (Pcm/_INGEST/Master reuse)
media/synthesis              →  python:compute/analysis/signal          # [SHAPE]: SignalOp.Filter/Resample band-limit + transform spectral QA (cross-branch)
media/synthesis              →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media synthesis fundamental_hz/waveform/duration facts band
delivery/register            ←  python:artifacts/composition/sheet      # [SHEET]: SheetSet.registered TitleBlock/suitability/revision tuples the register builds into its SheetEntry via from_title_block (+ externally-supplied naming context) then of_sheets aggregates
delivery/register            →  python:artifacts/visualization/table    # [TABLE]: RegisterOp.Index lowers Register.frame into the great-tables sheet-index publication table
delivery/register            →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Register kind/container/suitability/revision/classification/byte facts
delivery/transmittal         →  python:artifacts/composition/imposition # [IMPOSE]: constituent sheets laid into the press-form plan-set
delivery/transmittal         →  python:artifacts/package/archive        # [ARCHIVE]: collated plan-set + register/receipts sealed into one content-addressed transmittal container
delivery/transmittal         →  python:artifacts/exchange/conformance   # [CONFORMANCE]: PAdES-LTA legal issue-for-construction signature over the plan-set PDF
delivery/transmittal         →  python:artifacts/exchange/credential    # [CREDENTIAL]: C2PA sheet-lineage provenance over the cover asset (each sheet an Ingredient)
delivery/transmittal         ←  python:artifacts/delivery/register      # [REGISTER]: the issued index emit() + audited() composed as the transmittal manifest
delivery/transmittal         →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Transmittal issue/sheet/suitability/container/signed-verdict facts
core/plan                    ←  python:runtime/execution                # [KEYED]: Keyed (ContentKey, Work) session-lane elision
core/format                  →  python:artifacts/document               # [TEMPLATE]: bound context lowering the DOCX_TEMPLATE/PDF_TYPST document + ODS/XLSX spreadsheet-schedule + XML/YAML/TOML arms
core/format                  →  python:artifacts/document/report        # [REPORT]: HTML via the report TEMPLATE-kind jinja ReportLoader owner (rendered entry)
package/codec                ←  python:runtime                          # [CONTENT_KEY]: ContentKey including the DELTA parent-key from-image
package/delta                ←  python:runtime                          # [CONTENT_KEY]: DELTA parent-key from-image
*                            ←  python:runtime                          # [BOUNDARY]: boundary sync
```
