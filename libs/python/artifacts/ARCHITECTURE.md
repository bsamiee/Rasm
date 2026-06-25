# [PY_ARTIFACTS_ARCHITECTURE]

The domain map of `artifacts` — the host-free durable-output engine that turns ingress (data, compute, geometry) into controllable, layer-clean artifacts. Each sub-domain owns one polymorphic surface, every artifact keys by the runtime content key, and every receipt is one kind-discriminated `ArtifactReceipt` family.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
artifacts/
├── document/               # paginated structured documents: the DocumentNode tree, its emit/extract inverses, finishing, accessibility, reports
│   ├── model.py            # DocumentNode semantic tree + StructureNode/StructEltKind PDF/UA family + DocumentDelta diff/merge algebra
│   ├── emit.py             # Emission axis: every backend (reportlab/weasyprint/typst/office/lxml) lowers FROM the DocumentNode tree
│   ├── lens.py             # DocumentLens extraction/recovery half over pypdf/pdfplumber/odfpy/pymupdf/ocrmypdf/calamine/lxml
│   ├── egress.py           # PDF security/navigation finishing (encrypt/outline/watermark/attach/impose/redact) over pikepdf/pypdf/pymupdf/msoffcrypto
│   ├── tagged.py           # Tagged-PDF PD/UA marked-content authoring + structural audit over pikepdf StructTreeRoot from the model tree
│   └── report.py           # ReportPlan reproducible notebook/section composition into the DocumentNode tree over jinja2/papermill/nbclient
├── visualization/          # data -> visual artifact
│   ├── chart/
│   │   ├── spec.py         # ChartSpec engine union over altair/lets-plot/matplotlib, palette-threaded
│   │   ├── export.py       # ChartExport host-free render/format dispatch over vl-convert-python
│   │   └── transform.py    # VegaTransform vegafusion data pre-pass (inline / Arrow-IPC extract / chart-state)
│   ├── table.py            # great-tables publication-table owner exporting HTML/LaTeX/PDF
│   └── diagram/
│       ├── layout.py       # rustworkx/grandalf graph-layout coordinate assignment (sun-path/circulation/stacking/program/site)
│       ├── draw.py         # drawsvg named-layer SVG emission of the laid-out diagram
│       └── glyphset.py     # bounded AEC diagram-primitive vocabulary (node/edge/swimlane/annotation marks)
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
│       └── managed.py      # ICC/LUT/CCTF color-managed raster egress over colour-science core + pillow ImageCms gated apply
├── typography/             # font binary + glyph shaping + line-layout
│   ├── font.py             # FontEngineering subset/instance/axis-catalog/outline/embed-audit owner over fonttools
│   ├── shape.py            # Shaping uharfbuzz text-shaping + python-bidi reorder + blackrenderer COLRv1 glyph-render
│   └── layout.py           # line-break (uniseg UAX#14) + hyphenation (pyphen) + Knuth-Plass paragraph layout
├── composition/            # assembling artifacts into pages/sheets
│   ├── compose.py          # post-render figure/section placement (scale-fit/n-up/crop/rotate/overlay) over svgelements/resvg/pillow
│   ├── sheet.py            # architectural sheet-set / title-block / drawing-frame composition
│   └── imposition.py       # n-up / booklet / signature imposition over pymupdf show_pdf_page / pdfimpose
├── export/                 # editable layered hand-off for Illustrator/InDesign
│   ├── layered.py          # drawsvg named-layer SVG + pymupdf/pikepdf PDF OCG optional-content groups + psdtags layered TIFF
│   └── indesign.py         # SimpleIDML IDML template-mutation hand-off
├── exchange/               # metadata / provenance / format identification at the boundary
│   ├── metadata.py         # EXIF/XMP/IPTC descriptive metadata read/write over exif/iptcinfo3/pikepdf XMP
│   ├── credential.py       # c2pa-python content-credential manifest binding keyed by the content key
│   ├── conformance.py      # pyhanko PAdES sign/audit close folding ConformanceVerdict
│   └── detect.py           # python-magic media-type / file-info / format identification
├── media/                  # temporal media
│   ├── video.py            # av container/codec video encode/mux consuming the scene rgb24 frame sequence
│   └── audio.py            # av audio stream encode/mux
├── scene/                  # 3D / spatial visualization
│   ├── render.py           # pyvista/vtk offscreen render + FieldFilter clip/slice/threshold/contour/warp pipeline on the gated floor
│   ├── export.py           # glTF/VRML/OBJ/HTML scene-file export + orbit rgb24 frame seam
│   └── stage.py            # usd-core USD/USDZ stage authoring and composition
├── core/                   # production spine
│   ├── plan.py             # ArtifactPipeline content-keyed sub-graph-elision plan over the runtime session lane
│   ├── format.py           # TemplatePipeline input->output template/format binding over docxtpl/typst/jinja2/python-pptx/odfpy
│   └── receipt.py          # shared kind-discriminated ArtifactReceipt family across every production mode
└── package/                # content-addressed compression / archive / delta
    ├── codec.py            # zstandard/lz4/brotli content-addressed compression
    ├── archive.py          # py7zr/stream-zip/stream-unzip archive containers
    └── delta.py            # detools binary diff/patch for incremental artifact delta bundles
```

The engine reads as eleven high-order domains. `document` owns one `DocumentNode` tree that `document/emit` lowers FROM and `document/lens` recovers TO over one node algebra, with `DocumentDelta` diff/merge once and an extracted-tree corpus keyed into the runtime columnar lane; `document/egress` seals and navigates the emitted PDF, `document/tagged` authors the PD/UA marked-content tree from the `document/model` `StructureNode`/`StructEltKind` family, and `document/report` composes reproducible notebooks back into the tree. `visualization` is the data->visual axis: `visualization/chart` splits spec authoring from host-free export and the vegafusion data pre-pass, `visualization/table` owns publication tables, and `visualization/diagram` is the data-driven AEC-diagram engine that lays a graph out (`layout`) and emits it as named-layer SVG (`draw`) from a bounded primitive vocabulary (`glyphset`). `graphic` is the 2D primitive toolkit every visual and document plane composes — `raster` (io/process/measure), the `vector` SVG-geometry primitive, the `marks` encode/decode codec, and `color` (the upstream `derive` palette source plus the `managed` ICC/LUT egress, its one async-forcing gated leg). `typography` owns the font-binary transform, glyph shaping, and line-layout over one shared `PositionedGlyphRun` seam. `composition` assembles placed figures, architectural sheet-sets, and imposition; the editable named-layer hand-off then routes to `export` (`layered` OCG/SVG/TIFF + `indesign` IDML), while the descriptive and trust boundary routes to `exchange` (`metadata`, `credential`, `conformance`, `detect`). `media` muxes the temporal artifacts, `scene` renders and exports the 3D plane, `core` is the production spine (`plan`/`format`/`receipt`), and `package` is the content-addressed compression close.

`core/receipt` is the shared owner every visual and document sub-domain contributes one case to, never a parallel per-producer receipt rail, and the one receipt-fold edge the runtime `execution/lanes` reuse-fabric elision and the runtime `observability/metrics` `MeterProvider` signal stream both consume; outward figure handoff to a sibling package travels only as the `compute/graduation` `HandoffAxis` model-asset case keyed by `ContentIdentity`, never a parallel per-artifact handoff, and the artifacts sources re-mint no canonical concept so the runtime `evidence` `Structural.drift` query stays clean. `graphic/color/derive` is the one upstream color source the visual sub-domains pull palettes from rather than each engine picking color ad hoc, while `graphic/color/managed` is the downstream ICC/LUT/CCTF egress the raster and document outputs route through. The host-free posture is the structural axis cutting every sub-domain: `vl-convert` is the primary host-free chart export and `lets-plot` is the second host-free engine, and the great-tables Selenium path is the one remaining gated host path, never the default. The interpreter floor is the second structural axis: the cp315-core process imports no gated distribution, so every gated arm — `pillow`/`scikit-image`/`matplotlib`/`lxml`/`brotli`/`lz4`/`python-bidi` plus the `pillow` `ImageCms` ICC apply and `pikepdf` OCG authoring on the `python_version<'3.15'` band, `pyvista`/`vtk` on the sub-3.13 band with `usd-core` on the wider band sharing that worker — dispatches onto the runtime subprocess seam (`anyio.to_process.run_sync`), and the gated-band worker imports the package at module scope and renders offscreen on the software-GL floor.

## [02]-[SEAMS]

```text seams
*                            ←  python:runtime                          # [CONTENT_KEY]: ContentKey
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
visualization/chart/export   ←  python:data/tabular                     # [TRANSPORT]: ColumnarEgress.ArrowIpc inline_datasets frame feed
visualization/table          ←  python:data/tabular                     # [SHAPE]: QualityProfile frame rendered by the great-tables tier
visualization/diagram/layout ←  python:data/tabular                     # [GRAPH]: graph adjacency/attributes feed the layout
graphic/raster/measure       →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Preview pixel + perceptual-metric facts
graphic/vector               →  python:artifacts/composition            # [GEOMETRY]: SVG geometry primitive consumed by placement
graphic/marks/encode         →  python:artifacts/export                 # [LAYERED]: emitted mark SVG bound as a named layer
visualization/diagram/draw   →  python:artifacts/export                 # [LAYERED]: laid-out diagram SVG bound as named layers
typography/font              →  python:artifacts/document               # [FONT]: subsetted/instanced font bytes for FONT_EMBED
typography/font              →  python:artifacts/typography/shape       # [SHAPE]: face/variation location + embed-audit precondition
typography/shape             →  python:artifacts/document               # [DOCUMENT]: PositionedGlyphRun text placement
typography/shape             →  python:artifacts/composition            # [COMPOSE]: PositionedGlyphRun annotation
typography/layout            →  python:artifacts/document               # [LAYOUT]: line-broken paragraph runs for emission
composition/compose          →  python:artifacts/export                 # [LAYERED]: placed multi-source layout handed to named-layer egress
composition/imposition       →  python:artifacts/document               # [IMPOSE]: n-up/booklet sheet handed to the PDF assembler
export/layered               ←  python:artifacts/composition            # [COMPOSE]: placed multi-source layout + named-layer source graphics
export/layered               →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Preview/Egress named-layer evidence
export/indesign              ←  python:artifacts/composition            # [TEMPLATE]: placed layout bound into the IDML template
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
media/video                  ←  python:artifacts/scene                  # [SCENE]: rgb24 frame sequence ingested via VideoFrame.from_ndarray
media/video                  →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media container/codec encode facts
media/audio                  →  python:artifacts/core/receipt           # [RECEIPT]: ArtifactReceipt.Media audio encode facts
core/plan                    ←  python:runtime/execution                # [KEYED]: Keyed (ContentKey, Work) session-lane elision
core/format                  →  python:artifacts/document               # [TEMPLATE]: structured context binding the DOCX_TEMPLATE/PDF_TYPST arms
core/format                  →  python:artifacts/document/report        # [REPORT]: shared jinja2 ReportLoader/ParamSchema templating owner
core/format                  →  python:artifacts/core/receipt           # [RECEIPT]: emitted-artifact ArtifactReceipt
package/codec                ←  python:runtime                          # [CONTENT_KEY]: ContentKey including the DELTA parent-key from-image
package/delta                ←  python:runtime                          # [CONTENT_KEY]: DELTA parent-key from-image
*                            ←  python:runtime                          # [BOUNDARY]: boundary sync
```
