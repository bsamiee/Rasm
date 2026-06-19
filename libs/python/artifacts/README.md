# [PY_ARTIFACTS]

`artifacts` is the host-free, self-contained artifact-production companion: it turns data, compute, and geometry outputs (and any structured payload) into durable files — documents, reproducible reports, publication tables, 2D charts and 3D scientific visuals, archival signed PDFs, color-managed assets, image previews, and compressed bundles — each keyed by the runtime content key and carrying one kind-discriminated `ArtifactReceipt`. It is a relevant science/data utility valid wherever the monorepo needs a file emitted, not coupled to the AEC pipelines; it owns no UI, no durable store, no IFC/GLB geometry, and no columnar/mesh interchange. This file routes the design pages and registers the external packages.

## [1]-[ROUTER]

The design pages under `.planning/`, one sub-domain folder per eventual source sub-tree.

- `documents/model.md` — the semantic `DocumentNode` tagged-union tree (page/section/block/run/table/figure/field/annotation/structure-element) every backend lowers FROM and every extractor recovers TO, plus the `DocumentDelta` diff/merge algebra keyed by content key; the single interior representation that makes emission and extraction inverses.
- `documents/document-plan.md` — the document-mode emission axis: PDF authoring/render/raster/assemble/repair (reportlab/weasyprint/typst), Office, and structured-text on one backend-per-mode table, each backend a lowering arm folding from `DocumentNode`.
- `documents/inspection/lens.md` — the recover-TO half of the bidirectional seam: text/image/word/region extraction, full-text search, embedded-file and outline harvest, redaction burn-in, annotation authoring, and reflowable `Story` layout over pymupdf and pypdf, recovering `DocumentNode` trees into the runtime columnar corpus lane.
- `documents/egress/finish.md` — the `DocumentEgress` security-and-navigation finishing layer over an emitted PDF: AES encryption with a permission policy, outline/bookmark-tree authoring, overlay/underlay watermarking and stamping, embedded-attachment binding, and n-up/booklet imposition over pikepdf, pypdf, and weasyprint; finishes an emitted PDF, never authors one.
- `reporting/report-plan.md` — the report-composition axis binding figures and sections into a document tree over jinja2 templating and papermill/nbclient notebook execution.
- `charts/chart-spec.md` — the 2D chart union over altair/plotly/matplotlib with host-free static export (vl-convert-python primary, kaleido gated) and server-side Vega transform via vegafusion.
- `scene3d/scene.md` — the pyvista/VTK offscreen 3D scientific render and glTF/VRML scene export on the gated native floor.
- `tables/table-plan.md` — the great-tables publication-table owner exporting HTML/LaTeX/PDF.
- `imaging/preview.md` — the raster/preview owner over pillow, scikit-image, segno, and python-magic.
- `imaging/figure/compose.md` — the `Figure` composition owner turning emitted graphics into placed, annotated, color-correct figures: SVG scale-to-fit, n-up tiling, crop, and bounds query over svgelements (the SVG the chart/QR/nanoplot owners emit), plus draw/text/filter annotation and EXIF/XMP metadata over pillow.
- `color-management/colorimetry.md` — the colour-science colorimetry, spectral, CAM16, and palette owner feeding the visual sub-domains, plus the ICC/LUT color-managed raster egress.
- `typography/conformance.md` — the fonttools subset/instance, uharfbuzz OpenType text shaping, pyhanko PAdES-signing, and pyhanko/pikepdf conformance-verification close (signature validity, coverage, DSS/LTV material, archival metadata).
- `compression/bundle.md` — the algorithm-row compression and bundle owner over zstandard/lz4/brotli/py7zr.
- `receipt/artifact-receipt.md` — the shared kind-discriminated `ArtifactReceipt` family across every production mode.

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list. Versions live in the one Python manifest; new admissions land here from the folder's ideas and tasks. The cp314-bound rows (`pillow`, `matplotlib`, `scikit-image`, `lxml`, `brotli`, `lz4`) are gated `python_version<'3.15'`; `vtk`/`pyvista` are gated `python_version<'3.13'`; `vegafusion` carries the `python_version<'3.15'` band marker yet ships an abi3 wheel that imports on the cp315 core, so its chart-transform pre-pass arm runs on the subprocess lane by band policy, not by wheel limitation. `typst` ships a cp38-abi3 wheel installing on the cp315 core; `segno` and `great-tables` are pure-Python, cp315-clean. All are Rasm-owned and manifest-declared, distinct from the Forge companion lane. A gated row never resolves in the cp315-core process: its owner dispatches the gated arm onto the runtime subprocess lane (`anyio.to_process.run_sync`), and the gated-band worker imports the package at module scope.

- Documents: `reportlab`, `weasyprint` (`HTML.write_pdf`, `Document.make_bookmark_tree` outline), `typst` (`PDF_TYPST` mode with PDF/A via `pdf_standards`, the reusable `Compiler` world, `query`/`eval` introspection, `sys_inputs` data binding), `pymupdf`, `pypdfium2`, `pypdf` (assembly plus `PdfWriter.encrypt`/outline/`Transformation`/`merge_page` egress finishing), `pikepdf` (repair/linearize plus `Encryption`/`Permissions`/`Outline`/`add_overlay`/`add_underlay`/`as_form_xobject`/`AttachedFileSpec` egress finishing), `python-docx`, `python-pptx`, `openpyxl`, `lxml`, `ruamel-yaml`, `tomlkit`.
- Reporting: `jinja2`, `papermill`, `nbclient`.
- Charts: `altair`, `plotly`, `matplotlib`, `vl-convert-python`, `kaleido`, `vegafusion` (chart `EXPORT` transform pre-pass).
- Scene3d: `pyvista`, `vtk`.
- Tables: `great-tables` (manifest, cp315-clean).
- Imaging: `pillow` (raster I/O/transform/`ImageCms` ICC profiles, plus `ImageDraw`/`ImageFont`/`ImageOps`/`ImageFilter`/`Image.Exif` figure annotation and metadata), `scikit-image`, `segno` (QR/Micro-QR, dependency-free serializers, replaces qrcode), `python-barcode` (1D/linear symbologies only — Code128/EAN/UPC/ITF/Code39/Codabar — beside the segno QR arm; 2D matrix codes such as DataMatrix/PDF417 are NOT covered by python-barcode and route to a separate owner; catalogue RESEARCH-capture-pending behind the manifest admission), `svgelements` (pure-Python SVG geometry/transform/parse — `SVG.parse`/`Path`/`Matrix`/`bbox` — owning the `imaging/figure` SVG scale-to-fit/n-up/crop/bounds composition over the SVG the chart/QR/nanoplot owners emit, cp315-clean), `python-magic`.
- Color management: `colour-science`.
- Typography: `fonttools`, `pyhanko`, `uharfbuzz` (OpenType text shaping — the `Blob`/`Face`/`Font`/`Buffer`/`shape`/`GlyphInfo`/`GlyphPosition` pipeline, COLRv1 `PaintFuncs`, `Font.set_variations`, and the `draw_glyph_with_pen` fontTools outline bridge).
- Compression: `zstandard`, `lz4`, `brotli`, `py7zr`.
- Runtime ports (consumed, never re-minted): `content_identity.ContentKey`/`ContentIdentity`, `faults.RuntimeRail`/`boundary`, `receipts.Receipt`/`ReceiptContributor`, the `anyio.to_process.run_sync` subprocess lane the gated-band workers cross.

## [3]-[CROSS_CUTTING]

Branch-wide infrastructure packages this folder consumes; canonical registry lives at `libs/python/.api/`.

- anyio
- expression
- msgspec
- numpy
