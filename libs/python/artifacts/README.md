# [PY_ARTIFACTS]

`artifacts` is the host-free, self-contained artifact-production library: it turns data, compute, and geometry outputs — and any structured payload — into durable files keyed by the runtime content key and carrying one kind-discriminated `ArtifactReceipt`. Emitted forms include documents, reproducible reports, publication tables, 2D charts and 3D scientific visuals, archival signed PDFs, color-managed assets, image previews, and compressed bundles. The library is a general science/data surface valid wherever the monorepo needs a file emitted; it owns no UI, no durable store, no IFC/GLB geometry, and no columnar/mesh interchange. This file routes the design pages and registers the external packages.

## [01]-[ROUTER]

- [01]-[MODEL](.planning/documents/model.md): The semantic `DocumentNode` tagged-union tree (page/section/block/run/table/figure/field/annotation/structure-element) every backend lowers FROM and every extractor recovers TO, plus the `DocumentDelta` diff/merge algebra keyed by content key — the single interior representation that makes emission and extraction inverses.
- [02]-[EMIT](.planning/documents/emit.md): Document-mode emission axis — PDF authoring/render/raster/assemble/repair (`reportlab`/`weasyprint`/`typst`), Office, and structured-text on one backend-per-mode table; each backend is a lowering arm folding from `DocumentNode`, with `TYPST_QUERY`/`TYPST_DATA` introspection rows on the held `Compiler` font-cached world for batched data-bound renders.
- [03]-[EGRESS](.planning/documents/egress.md): `DocumentEgress` security-and-navigation finishing layer over an emitted PDF or Office container — RC4/AES encryption with a permission policy, outline/bookmark-tree authoring, overlay/underlay watermarking and stamping, embedded-attachment binding, n-up/booklet imposition, qpdf content-stream rewriting, MuPDF redaction burn-in, and Office confidentiality decrypt over `pikepdf`, `pypdf`, `pymupdf`, and `msoffcrypto`; finishes an emitted artifact, never authors one.
- [04]-[LENS](.planning/documents/lens.md): Recover-TO half of the bidirectional seam — text/image/word/region/table extraction, full-text search, scanned-page OCR, embedded-file and outline harvest, form-widget recovery, redaction burn-in, annotation authoring, reflowable `Story` layout, and spreadsheet/ODF ingest over `pypdf`/`pdfplumber`/`odfpy` (core) and `pymupdf`/`ocrmypdf`/`python-calamine` (gated) selected by one `LensProvider` band, recovering `DocumentNode` trees into the runtime columnar corpus lane.
- [05]-[CHART](.planning/figures/chart.md): 2D chart union over `altair`/`matplotlib`/`lets-plot` with host-free static export (`vl-convert-python` primary, `lets-plot` as the second host-free engine) and server-side Vega transform via `vegafusion`.
- [06]-[TABLE](.planning/figures/table.md): `great-tables` publication-table owner exporting HTML/LaTeX/PDF over one `reduce`-over-spec build — `tab_options` theme, header/footnote/source-note structure, summary-row algebra, `loc`-targeted cell styling, column-control family, and `data_color(autocolor_text)` data-driven fill.
- [07]-[SCENE](.planning/figures/scene.md): `pyvista`/VTK offscreen 3D scientific render and glTF/VRML scene export on the gated native floor.
- [08]-[PREVIEW](.planning/figures/preview.md): Raster/preview owner over `pillow`, `scikit-image`, `segno`, and `python-magic`; the `MARK` encoded-mark arm carries a `Symbology` sub-axis over `segno` QR/Micro-QR/sequence and the `python-barcode` linear 1D registry, with the `zxing-cpp` 2D-matrix owner (DataMatrix/PDF417/Aztec encode-decode) the planned separate-owner deepen the page routes to, not a present preview arm.
- [09]-[COMPOSE](.planning/figures/compose.md): `Figure` composition owner turning emitted graphics into placed, annotated, color-correct figures — SVG scale-to-fit, n-up tiling, crop, rotate-place, registration-overlay, and bounds query over `svgelements`, own-SVG rasterization over `resvg-py`, plus draw/text/filter annotation and EXIF/XMP metadata over `pillow`.
- [10]-[COLOR](.planning/figures/color.md): `colour-science` color, spectral, CAM16, and palette owner feeding the visual sub-domains, plus the `MANAGED` ICC/LUT/CCTF color-managed raster egress (`colour-science` tone-curve chain settled on the core, `pillow` `ImageCms` apply RESEARCH-pending on the gated band, forcing the async owner).
- [11]-[CONFORMANCE](.planning/typography/conformance.md): `fonttools` subset/instance, `uharfbuzz` OpenType text shaping (`SHAPE` -> `PositionedGlyphRun`), `pyhanko` PAdES-signing, and `pyhanko`/`pikepdf` conformance-audit close (`AUDIT` -> `ConformanceVerdict`: signature validity, coverage, DSS/LTV material, archival metadata).
- [12]-[REPORT](.planning/reports/report.md): Report-composition axis binding figures and sections into a document tree over `jinja2` templating and `papermill`/`nbclient` notebook execution.
- [13]-[BUNDLE](.planning/bundle/bundle.md): Algorithm-row compression and bundle owner over `zstandard`/`lz4`/`brotli`/`py7zr`.
- [14]-[RECEIPT](.planning/receipt/receipt.md): Shared kind-discriminated `ArtifactReceipt` family across every production mode.
- [15]-[CREDENTIAL](.planning/provenance/credential.md): `c2pa-python` content-credential manifest sign/verify keyed by the content key, binding the signed-artifact content key into the `csharp:Rasm.Persistence` store (planned).
- [16]-[TAGGED](.planning/accessibility/tagged.md): Tagged-PDF structure-element and accessibility-metadata authoring over the `documents/model` structure-element tree (planned).
- [17]-[ENCODE](.planning/media/encode.md): `av` container/codec media encode emitting one duration/codec `ArtifactReceipt` on the FFmpeg floor (planned).
- [18]-[PLAN](.planning/pipeline/plan.md): `ArtifactPipeline` sub-graph-elision plan consuming the runtime `execution` session-lane `Keyed` port (planned).

## [02]-[DOMAIN_PACKAGES]

Every domain rendering library this folder uses, planned or implemented. Versions are centralized in the one Python manifest; substrate packages live in `[3]-[SUBSTRATE_PACKAGES]` below. The cp314-bound rows (`pillow`, `matplotlib`, `scikit-image`, `lxml`, `brotli`, `lz4`) are gated `python_version<'3.15'`; `vtk`/`pyvista` are gated `python_version<'3.13'`; `vegafusion` carries the `python_version<'3.15'` band marker yet ships an abi3 wheel that imports on the cp315 core, so its chart-transform pre-pass arm runs on the subprocess lane by band policy, not by wheel limitation. `typst` ships a cp38-abi3 wheel installing on the cp315 core; `segno` and `great-tables` are pure-Python, cp315-clean. A gated row never resolves in the cp315-core process: its owner dispatches the gated arm onto the runtime subprocess lane (`anyio.to_process.run_sync`), and the gated-band worker imports the package at module scope. Four native libraries arrive from the Forge environment rather than a wheel: `libvips` backs `pyvips`, `leptonica` and `tesseract` back `ocrmypdf` OCR, and `ghostscript` backs the `ocrmypdf` PDF/A render path.

[Documents]:
- `reportlab`
- `weasyprint` - `HTML.write_pdf`, `Document.make_bookmark_tree` outline
- `typst` - `PDF_TYPST` mode with PDF/A via `pdf_standards`, the reusable `Compiler` world, `query`/`eval` introspection, `sys_inputs` data binding
- `pymupdf`
- `pypdfium2`
- `pypdf` - assembly plus `PdfWriter.encrypt`/outline/`Transformation`/`merge_page` egress finishing
- `pikepdf` - repair/linearize plus `Encryption`/`Permissions`/`Outline`/`add_overlay`/`add_underlay`/`as_form_xobject`/`AttachedFileSpec` egress finishing
- `python-docx`
- `python-pptx`
- `openpyxl`
- `xlsxwriter` - Write-only XLSX emit with charts/formats/conditional formatting
- `python-calamine` - Fast read-only XLSX/XLS/ODS ingest into the corpus lane
- `odfpy` - OpenDocument ODT/ODS/ODP read/write
- `docxtpl` - jinja2 DOCX template render over python-docx
- `msoffcrypto-tool` - Encrypted Office document decrypt at the ingest boundary
- `pdfplumber` - Page text/table/word geometry extraction over pdfminer
- `ocrmypdf` - OCR text layer over a scanned PDF on the tesseract/ghostscript Forge native floor
- `lxml`
- `ruamel-yaml`
- `tomlkit`

[Reporting]:
- `jinja2`
- `papermill`
- `nbclient`
- `nbconvert` - Notebook export to HTML/PDF/script over the executed notebook tree
- `jupytext` - Notebook/text round-trip for diffable report sources

[Charts]:
- `altair`
- `matplotlib`
- `lets-plot` - Second host-free chart engine beside the `vl-convert` Vega export path
- `vl-convert-python` - Primary host-free static chart export
- `vegafusion` - Chart `EXPORT` transform pre-pass

[Scene3d]:
- `pyvista`
- `vtk`
- `usd-core` - USD/USDA/USDC scene authoring and stage composition for 3D scene export

[Tables]:
- `great-tables` - Pure-Python, cp315-clean

[Imaging]:
- `pillow` - Raster I/O/transform/`ImageCms` ICC profiles, plus `ImageDraw`/`ImageFont`/`ImageOps`/`ImageFilter`/`Image.Exif` figure annotation and metadata
- `scikit-image`
- `segno` - QR/Micro-QR, dependency-free serializers, replaces qrcode
- `python-barcode` - Linear 1D symbologies only — Code128/EAN/UPC/ITF/Code39/Codabar — beside the segno QR arm
- `zxing-cpp` - 2D-matrix symbology owner — DataMatrix/PDF417/Aztec encode and decode — beside the python-barcode linear arm
- `pyvips` - Fast libvips raster pipeline — resize/thumbnail/format-convert/composite — on the libvips Forge native floor
- `resvg-py` - Resvg SVG-to-raster render with accurate font/filter support
- `svgelements` - Pure-Python SVG geometry/transform/parse — `SVG.parse`/`Path`/`Matrix`/`bbox` — owning the `figures/compose` SVG scale-to-fit/n-up/crop/bounds composition over the SVG the chart/QR/nanoplot owners emit, cp315-clean
- `python-magic`

[Color]:
- `colour-science`
- `coloraide` - CSS-space color parse/interpolate/gamut-map for web-facing palette egress

[Typography]:
- `fonttools`
- `pyhanko`
- `uharfbuzz` - OpenType text shaping — the `Blob`/`Face`/`Font`/`Buffer`/`shape`/`GlyphInfo`/`GlyphPosition` pipeline, COLRv1 `PaintFuncs`, `Font.set_variations`, and the `draw_glyph_with_pen` fontTools outline bridge
- `blackrenderer` - COLRv1 color-glyph raster/SVG rendering over the uharfbuzz/fontTools paint chain

[Provenance]:
- `c2pa-python` - C2PA content-credential manifest sign/verify keyed by the content key, feeding `provenance/credential`

[Compression]:
- `zstandard`
- `lz4`
- `brotli`
- `py7zr`
- `stream-zip` - Streaming ZIP archive emit without buffering the whole archive
- `stream-unzip` - Streaming ZIP archive ingest without buffering the whole archive
- `detools` - Binary-diff/patch generation for incremental artifact delta bundles

[Media]:
- `av` (PyAV container/codec read/write for `media/encode` duration/codec receipts on the FFmpeg floor)

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting substrate libraries this folder consumes; canonical registry lives at `libs/python/.planning/README.md` and branch `libs/python/.api/`.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`
- `pydantic`

[CONCURRENCY]:
- `anyio`

[NUMERIC_SUBSTRATE]:
- `numpy`
