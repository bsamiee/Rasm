# [PY_ARTIFACTS]

`artifacts` is the host-free, self-contained artifact-production companion: it turns data, compute, and geometry outputs (and any structured payload) into durable files — documents, reproducible reports, publication tables, 2D charts and 3D scientific visuals, archival signed PDFs, color-managed assets, image previews, and compressed bundles — each keyed by the runtime content key and carrying one kind-discriminated `ArtifactReceipt`. It is a relevant science/data utility valid wherever the monorepo needs a file emitted, not coupled to the AEC pipelines; it owns no UI, no durable store, no IFC/GLB geometry, and no columnar/mesh interchange. This file routes the design pages and registers the external packages.

## [1]-[ROUTER]

The design pages under `.planning/`, one sub-domain folder per eventual source sub-tree.

- `documents/document-plan.md` — the document-mode dispatch axis: PDF authoring/render/raster/assemble/repair, Office, and structured-text on one backend-per-mode table.
- `reporting/report-plan.md` — the report-composition axis binding figures and sections into a document tree over jinja2 templating and papermill/nbclient notebook execution.
- `charts/chart-spec.md` — the 2D chart union over altair/plotly/matplotlib with host-free static export (vl-convert-python primary, kaleido gated).
- `scene3d/scene.md` — the pyvista/VTK offscreen 3D scientific render and glTF/VRML scene export on the gated native floor.
- `tables/table-plan.md` — the great-tables publication-table owner exporting HTML/LaTeX/PDF.
- `imaging/preview.md` — the raster/preview owner over pillow, scikit-image, qrcode, and python-magic.
- `color-management/colorimetry.md` — the colour-science colorimetry, spectral, CAM16, and palette owner feeding the visual sub-domains.
- `typography/conformance.md` — the fonttools subset/instance and pyhanko PAdES-signing PDF conformance close.
- `compression/bundle.md` — the algorithm-row compression and bundle owner over zstandard/lz4/brotli/py7zr.
- `receipt/artifact-receipt.md` — the shared kind-discriminated `ArtifactReceipt` family across every production mode.

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list. Versions live in the one Python manifest; new admissions land here from the folder's ideas and tasks. The cp314-bound rows (`pillow`, `matplotlib`, `scikit-image`, `lxml`, `brotli`, `lz4`) are gated `python_version<'3.15'`; `vtk`/`pyvista` are gated `python_version<'3.13'`. All are Rasm-owned and manifest-declared, distinct from the Forge companion lane. A gated row never resolves in the cp315-core process: its owner dispatches the gated arm onto the runtime subprocess lane (`anyio.to_process.run_sync`), and the gated-band worker imports the package at module scope.

- Documents: `reportlab`, `weasyprint`, `pymupdf`, `pypdfium2`, `pypdf`, `pikepdf`, `python-docx`, `python-pptx`, `openpyxl`, `lxml`, `ruamel-yaml`, `tomlkit`.
- Reporting: `jinja2`, `papermill`, `nbclient`.
- Charts: `altair`, `plotly`, `matplotlib`, `vl-convert-python`, `kaleido`.
- Scene3d: `pyvista`, `vtk`.
- Visualization: `lonboard`.
- Tables: `great-tables` (manifest, cp315-clean).
- Imaging: `pillow`, `scikit-image`, `qrcode`, `python-magic`.
- Color management: `colour-science`.
- Typography: `fonttools`, `pyhanko`, `uharfbuzz`.
- Compression: `zstandard`, `lz4`, `brotli`, `py7zr`.
- Runtime ports (consumed, never re-minted): `content_identity.ContentKey`/`ContentIdentity`, `faults.RuntimeRail`/`boundary`, `receipts.Receipt`/`ReceiptContributor`, the `anyio.to_process.run_sync` subprocess lane the gated-band workers cross.

## [3]-[CROSS_CUTTING]

Branch-wide infrastructure packages this folder consumes; canonical registry lives at `libs/python/.api/`.

- anyio
- expression
- msgspec
- numpy
