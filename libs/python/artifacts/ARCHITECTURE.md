# [PY_ARTIFACTS_ARCHITECTURE]

The domain map of `artifacts` — the host-free artifact-production utility. Each sub-domain owns one polymorphic surface, every artifact keys by the runtime content key, and every receipt is one kind-discriminated `ArtifactReceipt` family.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
artifacts/
├── documents/          # DocumentNode tree and its emission/extraction inverses
│   ├── model.py        # DocumentNode semantic tree, StructureNode/StructEltKind PDF/UA family, DocumentDelta diff/merge algebra
│   ├── emit.py         # Emission axis: every backend lowers FROM DocumentNode tree
│   ├── egress.py       # PDF security/navigation/OCG finishing layer over pikepdf/pypdf/pymupdf/msoffcrypto
│   └── lens.py         # DocumentLens extraction/recovery half over pypdf/pdfplumber/odfpy/pymupdf/ocrmypdf/calamine/ruamel-yaml/tomlkit/lxml/python-docx
├── figures/            # Visual-figure plane regrouped by sub-domain
│   ├── chart/
│   │   └── chart.py    # 2D chart union over altair/lets-plot/matplotlib with host-free static export and the vegafusion data-transform pre-pass
│   ├── table/
│   │   └── table.py    # Great-tables publication-table owner exporting HTML/LaTeX/PDF
│   ├── scene/
│   │   └── scene.py    # Pyvista/VTK offscreen 3D render with glTF/VRML/OBJ/HTML/USD/USDZ export on gated floor
│   ├── raster/
│   │   └── raster.py   # Raster image-processing owner over pillow/scikit-image/pyvips/python-magic on the gated band
│   ├── marks/
│   │   └── marks.py    # In-process encoded-mark codec over segno/python-barcode/zxing-cpp with the read_barcodes decode inverse
│   ├── compose/
│   │   └── compose.py  # Post-render figure-placement owner over svgelements/resvg-py/pillow, FLAT-SVG egress
│   └── color/
│       ├── derive.py   # Colorimetry color-derivation owner over colour-science + ColorAide, the upstream palette source
│       └── managed.py  # ICC/LUT/CCTF color-managed raster egress over colour-science core + pillow ImageCms gated apply
├── typography/         # Font-binary, text-shaping, and PAdES-signing concerns
│   ├── font/
│   │   └── font.py     # FontEngineering subset/instance/axis-catalog/outline/embed-audit owner over fonttools
│   ├── shape/
│   │   └── shape.py    # Shaping uharfbuzz text-shaping + python-bidi reorder + blackrenderer COLRv1 glyph-render owner
│   └── sign/
│       └── sign.py     # Conformance pyhanko PAdES sign/audit close folding ConformanceVerdict
├── provenance/         # C2PA signed-artifact content-credential binding
│   └── credential.py   # c2pa-python content-credential manifest binding keyed by the content key
├── accessibility/      # Tagged-content PDF/UA accessibility structure
│   └── tagged.py       # Tagged-PDF structure-element authoring over pikepdf StructTreeRoot/StructElem from the model tree
├── media/              # Container/codec media encode
│   └── encode.py       # av container/codec encode consuming the figures/scene rgb24 frame sequence
├── pipeline/           # Content-keyed artifact-production planning and template binding
│   ├── plan.py         # ArtifactPipeline sub-graph-elision plan over the runtime session lane
│   └── template.py     # TemplatePipeline input->output template/format binding over docxtpl/typst/jinja2/python-pptx/odfpy
├── export/             # Named-layer editable export for Illustrator/InDesign
│   └── layered.py      # LayeredExport drawsvg named-layer SVG + pikepdf PDF OCG optional-content-group authoring
├── reports/            # Figure-binding report composition
│   └── report.py       # ReportPlan composition binding figures/sections over jinja2 and papermill
├── bundle/             # Content-addressed compression/bundling owner
│   └── bundle.py       # Algorithm-row compression/bundle owner over zstandard/lz4/brotli/py7zr/stream-zip/detools/zlib-ng with the pack/unpack inverse
└── receipt/            # Kind-discriminated ArtifactReceipt family
    └── receipt.py      # Shared kind-discriminated ArtifactReceipt family across every mode
```

Three concerns are bidirectional, not emit-only: `documents` owns one `DocumentNode` tree that emission lowers FROM and `documents/lens` recovers TO over one node algebra, with `DocumentDelta` diff/merge once and an extracted-tree corpus keyed into the runtime columnar lane; `typography` splits the font-binary transform (`typography/font`), text-shaping-and-glyph-render (`typography/shape`), and PAdES signing-and-audit (`typography/sign`) concerns across three coherent owners over one shared `PositionedGlyphRun`/`ConformanceVerdict` seam; `figures/color` splits palette derivation (`figures/color/derive`, the upstream color source every visual plane pulls from) from the ICC/LUT layer that color-manages the raster egress (`figures/color/managed`, the one async-forcing gated leg). Two sub-domains finish already-emitted graphics rather than author them: `documents/egress` seals and navigates the PDF (encryption, bookmarks, watermarks, attachments, n-up, redaction burn-in, OCG-layer strip, Office decrypt) over pikepdf/pypdf/pymupdf/msoffcrypto, and `figures/compose` places and annotates emitted SVG over svgelements, resvg-py, and pillow with a FLAT-SVG egress, the named-layer editable handoff routing to `export/layered`. `accessibility/tagged` authors the PDF/UA marked-content tree from the `documents/model` `StructureNode`/`StructEltKind` family and closes the structural verdict `typography/sign` discloses, and `media/encode` ingests the `figures/scene` orbit `rgb24` frame sequence into a muxed container — the only media encoder, `pyvips` being a `figures/raster` provider not a media owner.

`receipt` is the shared owner every visual and document sub-domain contributes one case to, never a parallel per-producer receipt rail, and the one receipt-fold edge the runtime `execution/lanes` reuse-fabric elision and the runtime `observability/metrics` `MeterProvider` signal stream both consume; outward figure handoff to a sibling package travels only as the `compute/graduation` `HandoffAxis` model-asset case keyed by `ContentIdentity`, never a parallel per-artifact handoff, and the artifacts sources re-mint no canonical concept so the runtime `evidence` `Structural.drift` query stays clean. `figures/color/derive` is the one upstream color source the visual sub-domains pull palettes from rather than each engine picking color ad hoc, while `figures/color/managed` is the downstream ICC/LUT/CCTF egress the raster and document outputs route through. The host-free posture is the structural axis cutting every sub-domain: `vl-convert` is the primary host-free chart export and `lets-plot` is the second host-free engine, and the great-tables Selenium path is the one remaining gated host path, never the default. The interpreter floor is the second structural axis: the cp315-core process imports no gated distribution, so every gated arm — `pillow`/`scikit-image`/`matplotlib`/`lxml`/`brotli`/`lz4`/`python-bidi` plus the `pillow` `ImageCms` ICC apply and `pikepdf` OCG authoring on the `python_version<'3.15'` band, `pyvista`/`vtk` on the sub-3.13 band with `usd-core` on the wider band sharing that worker — dispatches onto the runtime subprocess seam (`anyio.to_process.run_sync`), and the gated-band worker imports the package at module scope and renders offscreen on the software-GL floor.

## [02]-[SEAMS]

```text seams
*                       ←  python:runtime                    # [CONTENT_KEY]: ContentKey
documents/model         →  python:data/tabular               # [WIRE]: to_corpus_row flat record
documents/model         →  python:artifacts/accessibility    # [NODE]: StructureNode/StructEltKind structure tree + FigureNode.alt
figures/color/derive    →  python:data/tabular               # [WIRE]: color palette arrays / appearance correlates
figures/color/derive    →  python:artifacts/figures          # [DERIVE]: ColorReceipt.coords palette to chart/table/scene/marks
figures/color/derive    →  python:artifacts/figures/color    # [DERIVE]: tone-curve / space provenance to the managed egress
figures/color/managed   →  python:artifacts/figures/raster   # [MANAGED]: color-managed raster consumed by the Convert/Transform arms
figures/color/managed   →  python:artifacts/documents        # [MANAGED]: ICC-profiled raster consumed by document/PDF output
*                       →  python:runtime                    # [RECEIPT]: ArtifactReceipt contribution
receipt/receipt         ←  python:runtime/execution          # [RECEIPT]: reuse-fabric elision ContentKey hit/miss
receipt/receipt         ←  python:runtime/observability      # [RECEIPT]: MeterProvider signal stream
receipt/receipt         ←  python:artifacts/pipeline         # [SIGNALS]: per-artifact contribute folds walked into the elision plan
figures/chart           ←  python:data/tabular               # [TRANSPORT]: ColumnarEgress.ArrowIpc inline_datasets frame feed
figures/table           ←  python:data/tabular               # [SHAPE]: QualityProfile frame rendered by the great-tables tier
figures/raster          →  python:artifacts/receipt          # [RECEIPT]: ArtifactReceipt.Preview pixel-dimension facts
figures/marks           →  python:artifacts/export           # [LAYERED]: emitted mark SVG bound as a named layer
figures/scene           →  python:artifacts/media            # [MEDIA]: rgb24 frame sequence via VideoFrame.from_ndarray, ContentKey-keyed
figures/scene           ⇄  python:geometry/mesh              # [BOUNDARY]: visualization-scene export vs mesh-file codec, no shared owner
figures/compose         →  python:artifacts/export           # [LAYERED]: placed multi-source layout handed to named-layer egress
typography/font         →  python:artifacts/documents        # [FONT]: subsetted/instanced font bytes for FONT_EMBED
typography/font         →  python:artifacts/typography        # [SHAPE/SIGN]: face/variation location + embed-audit precondition
typography/shape        →  python:artifacts/documents        # [DOCUMENT]: PositionedGlyphRun text placement
typography/shape        →  python:artifacts/figures          # [COMPOSE]: PositionedGlyphRun annotation
typography/sign         ←  python:artifacts/documents        # [DOCUMENT]: emitted PDF input, contributing ArtifactReceipt.Verdict
typography/sign         ←  python:artifacts/accessibility    # [ACCESS]: structural-conformance result folded into the verdict
accessibility/tagged    ←  python:artifacts/documents        # [NODE]: StructureNode/StructEltKind tree + FigureNode.alt authored to marked content
accessibility/tagged    →  python:artifacts/typography        # [SIGN]: structural-conformance result folded into the verdict
accessibility/tagged    →  python:artifacts/receipt          # [RECEIPT]: ArtifactReceipt.Egress/Pdf structural evidence
provenance/credential   ←  python:runtime                    # [CONTENT_KEY]: ContentKey
provenance/credential   →  python:artifacts/receipt          # [RECEIPT]: ArtifactReceipt.Credential content-credential facts
provenance/credential   →  csharp:Rasm.Persistence           # [CONTENT_KEY]: signed-artifact content-key binding decodes the XxHash128 seed
media/encode            ←  python:artifacts/figures          # [SCENE]: rgb24 frame sequence ingested via VideoFrame.from_ndarray
media/encode            →  python:artifacts/receipt          # [RECEIPT]: ArtifactReceipt.Media container/codec encode facts
media/encode            →  python:runtime/observability      # [RECEIPT]: media-encode duration/codec ArtifactReceipt
export/layered          ←  python:artifacts/figures          # [COMPOSE]: placed multi-source layout + named-layer source graphics
export/layered          →  python:artifacts/receipt          # [RECEIPT]: ArtifactReceipt.Preview/Egress named-layer evidence
pipeline/plan           ←  python:runtime/execution          # [KEYED]: Keyed (ContentKey, Work) session-lane elision
pipeline/template       →  python:artifacts/documents        # [TEMPLATE]: structured context binding the DOCX_TEMPLATE/PDF_TYPST arms
pipeline/template       →  python:artifacts/reports          # [REPORT]: shared jinja2 ReportLoader/ParamSchema templating owner
pipeline/template       →  python:artifacts/receipt          # [RECEIPT]: emitted-artifact ArtifactReceipt
bundle/bundle           ←  python:runtime                    # [CONTENT_KEY]: ContentKey including the DELTA parent-key from-image
*                       ←  python:runtime                    # [BOUNDARY]: boundary sync
```
