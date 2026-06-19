# [PY_ARTIFACTS_ARCHITECTURE]

The domain map of `artifacts` — the host-free artifact-production utility. Each sub-domain owns one polymorphic surface, every artifact keys by the runtime content key, and every receipt is one kind-discriminated `ArtifactReceipt` family.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
artifacts/
├── documents/          # DocumentNode tree and its emission/extraction inverses
│   ├── model.py        # DocumentNode semantic tree and DocumentDelta diff/merge algebra
│   ├── emit.py         # Emission axis: every backend lowers FROM DocumentNode tree
│   ├── egress.py       # PDF security/navigation finishing layer over pikepdf/pypdf/weasyprint
│   └── lens.py         # DocumentLens extraction/recovery half over pymupdf/pypdf
├── figures/            # Visual-figure plane: charts, tables, scenes, preview, compose, color
│   ├── chart.py        # 2D chart union over altair/plotly/matplotlib with host-free static export
│   ├── table.py        # Great-tables publication-table owner exporting HTML/LaTeX/PDF
│   ├── scene.py        # Pyvista/VTK offscreen 3D render and glTF/VRML export on gated floor
│   ├── preview.py      # Raster/preview owner over pillow/scikit-image/segno
│   ├── compose.py      # Figure-composition owner over svgelements and pillow
│   └── color.py        # Colour-science color/spectral/palette owner and ICC/LUT egress
├── typography/         # Font-shaping/PAdES-signing/PDF-A conformance close
│   └── conformance.py  # Font subset/instance, uharfbuzz shaping, and pyhanko PAdES conformance
├── reports/            # Figure-binding report composition
│   └── report.py       # ReportPlan composition binding figures/sections over jinja2 and papermill
├── bundle/             # Compression/bundling owner
│   └── bundle.py       # Algorithm-row compression/bundle owner over zstandard/lz4/brotli/py7zr
└── receipt/            # Kind-discriminated ArtifactReceipt family
    └── receipt.py      # Shared kind-discriminated ArtifactReceipt family across every mode
```

Three concerns are bidirectional, not emit-only: `documents` owns one `DocumentNode` tree that emission lowers FROM and `documents/lens` recovers TO over one node algebra, with `DocumentDelta` diff/merge once and an extracted-tree corpus keyed into the runtime columnar lane; `typography` owns PAdES signing AND conformance verification plus uharfbuzz OpenType shaping; `figures/color` owns palette derivation AND the ICC/LUT layer that color-manages the raster egress. Two sub-domains finish already-emitted graphics rather than author them: `documents/egress` seals and navigates the PDF (encryption, bookmarks, watermarks, attachments, n-up) over pikepdf/pypdf/weasyprint, and `figures/compose` places and annotates emitted SVG over svgelements and pillow.

`receipt` is the shared owner every visual and document sub-domain contributes one case to, never a parallel per-producer receipt rail, and the one receipt-fold edge the runtime `execution/lanes` reuse-fabric elision and the runtime `observability/metrics` `MeterProvider` signal stream both consume; outward figure handoff to a sibling package travels only as the `compute/graduation` `HandoffAxis` model-asset case keyed by `ContentIdentity`, never a parallel per-artifact handoff, and the artifacts sources re-mint no canonical concept so the runtime `evidence` `Structural.drift` query stays clean. `figures/color` is the one upstream color source the visual sub-domains pull palettes from rather than each engine picking color ad hoc. The host-free posture is the structural axis cutting every sub-domain: vl-convert-python is the primary chart engine, and the kaleido host-Chrome and great-tables Selenium paths are gated optional, never the default. The interpreter floor is the second structural axis: the cp315-core process imports no gated distribution, so every gated arm — `pillow`/`scikit-image`/`matplotlib`/`lxml`/`brotli`/`lz4` on the `python_version<'3.15'` band, `pyvista`/`vtk` on the sub-3.13 band — dispatches onto the runtime subprocess seam (`anyio.to_process.run_sync`), and the gated-band worker imports the package at module scope and renders offscreen on the software-GL floor.

## [02]-[SEAMS]

```text seams
*                       ←  python:runtime                # [CONTENT_KEY]: ContentKey
documents/model         →  python:data/tabular           # [WIRE]: to_corpus_row flat record
figures/color           →  python:data/tabular           # [WIRE]: color palette arrays / appearance correlates
documents/emit          ←  python:runtime/evidence       # [TRANSPORT]: gRPC ServerHost / CRDT MessagePack
*                       →  python:runtime                # [RECEIPT]: Chart receipt
receipt/receipt         ←  python:runtime/execution      # [RECEIPT]: reuse-fabric elision ContentKey hit/miss
receipt/receipt         ←  python:runtime/observability  # [RECEIPT]: MeterProvider signal stream
figures/color           →  python:runtime/evidence       # [SHAPE]: Derived color arrays/palettes
typography/conformance  →  python:data/tabular           # [SHAPE]: PositionedGlyphRun shape/run facts
*                       ←  python:runtime                # [BOUNDARY]: boundary sync
```
