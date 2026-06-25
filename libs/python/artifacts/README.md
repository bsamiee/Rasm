# [PY_ARTIFACTS]

`artifacts` is the host-free, self-contained artifact-production library: it turns data, compute, and geometry outputs — and any structured payload — into durable files keyed by the runtime content key and carrying one kind-discriminated `ArtifactReceipt`. Emitted forms include documents, reproducible reports, publication tables, 2D charts and 3D scientific visuals, archival signed PDFs, color-managed assets, image previews, and compressed bundles. The library is a general science/data surface valid wherever the monorepo needs a file emitted; it owns no UI, no durable store, no IFC/GLB geometry, and no columnar/mesh interchange. This file routes the design pages and registers the external packages.

## [01]-[ROUTER]

- [01]-[MODEL](.planning/documents/model.md): The semantic `DocumentNode` tagged-union tree (page/section/block/run/table/figure/field/annotation/structure-element) every backend lowers FROM and every extractor recovers TO, plus the `DocumentDelta` diff/merge algebra keyed by content key — the single interior representation that makes emission and extraction inverses.
- [02]-[EMIT](.planning/documents/emit.md): Document-mode emission axis — PDF authoring/render/raster/assemble/repair (`reportlab`/`weasyprint`/`typst`), Office, and structured-text on one backend-per-mode table; each backend is a lowering arm folding from `DocumentNode`, with `TYPST_QUERY`/`TYPST_DATA` introspection rows on the held `Compiler` font-cached world for batched data-bound renders.
- [03]-[EGRESS](.planning/documents/egress.md): `DocumentEgress` security-and-navigation finishing layer over an emitted PDF or Office container — RC4/AES encryption with a permission policy, outline/bookmark-tree authoring, overlay/underlay watermarking and stamping, embedded-attachment binding, n-up/booklet imposition, qpdf content-stream rewriting, MuPDF redaction burn-in, and Office confidentiality decrypt over `pikepdf`, `pypdf`, `pymupdf`, and `msoffcrypto`; finishes an emitted artifact, never authors one.
- [04]-[LENS](.planning/documents/lens.md): Recover-TO half of the bidirectional seam — text/image/word/region/table extraction, full-text search, scanned-page OCR, embedded-file and outline harvest, form-widget recovery, redaction burn-in, annotation authoring, reflowable `Story` layout, and spreadsheet/ODF ingest over `pypdf`/`pdfplumber`/`odfpy` (core) and `pymupdf`/`ocrmypdf`/`python-calamine` (gated) selected by one `LensProvider` band, recovering `DocumentNode` trees into the runtime columnar corpus lane.
- [05]-[CHART](.planning/figures/chart/chart.md): 2D chart union over `altair`/`matplotlib`/`lets-plot` with host-free static export (`vl-convert-python` primary, `lets-plot` as the second host-free engine), the `VegaTransform` `vegafusion` DataFusion data-transform pre-pass sub-owner feeding `inline_datasets`, and the palette threaded from `figures/color/derive`.
- [06]-[TABLE](.planning/figures/table/table.md): `great-tables` publication-table owner exporting HTML/LaTeX/PDF over one `reduce`-over-spec build — `tab_options` theme, header/footnote/source-note structure, summary-row algebra, `loc`-targeted cell styling, column-control family, and `data_color(autocolor_text)` data-driven fill.
- [07]-[SCENE](.planning/figures/scene/scene.md): `pyvista`/VTK offscreen 3D scientific render with glTF/VRML/OBJ/HTML export on the gated `python_version<'3.13'` floor and `usd-core` USD/USDZ packaging on the wider gated band, the `FieldFilter` chain, `CameraPose` viewpoint, and the orbit `rgb24` frame sequence handed to `media/encode`.
- [08]-[RASTER](.planning/figures/raster/raster.md): `Raster` raster-image-processing owner over `pillow` (working surface), the `scikit-image` 52-row `Transform` scientific engine, `pyvips` (the fused-libvips decode/downscale/ICC/smartcrop high-throughput provider), and the `python-magic` MIME ingress gate — one `RasterOp` surface on the gated `python_version<'3.15'` subprocess band, no per-media-type class family.
- [09]-[MARKS](.planning/figures/marks/marks.md): `Mark` in-process encoded-mark codec over the `Symbology` axis — `segno` QR/Micro-QR/sequence, the `python-barcode` linear-1D `get_barcode_class` registry, and `zxing-cpp` 2D-matrix DataMatrix/PDF417/Aztec/MaxiCode `create_barcode`/`to_svg` — plus the `Decode` `read_barcodes` round-trip inverse the generation arms cannot express, dependency-free SVG on the cp315 core.
- [10]-[COMPOSE](.planning/figures/compose/compose.md): `Figure` post-render composition owner placing and annotating already-emitted chart/marks/table SVG — SVG scale-to-fit, n-up tiling, crop, rotate-place, registration-overlay, and bounds query over `svgelements`, own-SVG rasterization over `resvg-py`, plus draw/text/filter annotation and EXIF/XMP metadata over `pillow`; FLAT-SVG egress, the named-layer editable handoff routing to `export/layered`.
- [11]-[DERIVE](.planning/figures/color/derive.md): `Colorimetry` color-derivation owner — `colour-science` (CIE convert/spectral/CAM16/`delta_E`/chromatic-adaptation/CCT) + `ColorAide` (gamut-map/CVD/perceptual-palette/harmony/WCAG contrast) over one `ColorModel` dual-name vocabulary keying both engines, the upstream color source every visual plane pulls from.
- [12]-[MANAGED](.planning/figures/color/managed.md): `ColorManaged` ICC/LUT/CCTF color-managed raster-egress owner — the `colour-science` `cctf_encoding`/`read_LUT`/`LUTSequence.apply` tone chain on the core then the `pillow` `ImageCms` ICC apply under `RenderingIntent` on the gated `python_version<'3.15'` subprocess seam (the one async-forcing leg).
- [13]-[FONT](.planning/typography/font/font.md): `FontEngineering` font-binary owner over `fonttools` — `subset.Subsetter` footprint reduction, `varLib.instancer` partial-axis instancing under the `AxisLimit` policy, `fvar`/`STAT` axis introspection, the `SVGPathPen` outline bridge, and embed-completeness audit; the subset->instance->embed-precondition chain consumed by `documents/emit` `FONT_EMBED`.
- [14]-[SHAPE](.planning/typography/shape/shape.md): `Shaping` text-shaping-and-glyph-render owner — `uharfbuzz` `Face`/`Font`/`Buffer`/`shape` producing `PositionedGlyphRun` on the cp315 core, the `python-bidi` UAX#9 reorder pass on the gated band, and `blackrenderer` COLRv1/COLRv0 `drawGlyph` color-glyph raster onto a `getSurfaceClass` surface; the run consumed by `documents/emit` and `figures/compose`.
- [15]-[SIGN](.planning/typography/sign/sign.md): `Conformance` PAdES-signing-and-audit owner — `pyhanko` `SimpleSigner`/`ExternalSigner` + `PdfSignatureMetadata` + `HTTPTimeStamper` + `update_archival_timestamp_chain` under `PadesLevel`/`CertifyPerm`, folding `pyhanko`/`pikepdf` audit plus the `accessibility/tagged` structural result into one `ConformanceVerdict`.
- [16]-[REPORT](.planning/reports/report.md): Report-composition axis binding figures and sections into a document tree over `jinja2` templating and `papermill`/`nbclient` notebook execution, the `TEMPLATE` kind growing the structured-office output row (DOCX/`docxtpl`, PPTX/`python-pptx`, ODF/`odfpy`).
- [17]-[BUNDLE](.planning/bundle/bundle.md): `Bundle` content-addressed compression/bundle owner over the `CompressionAlgo` algorithm-row table — `zstandard`/`lz4`/`brotli`/`py7zr`/`stream-zip`/`detools` delta/`zlib-ng` gzip — plus the pack/unpack modal inverse over a `BundleManifest` of `(name, ContentKey, algo, size)` rows.
- [18]-[RECEIPT](.planning/receipt/receipt.md): Shared kind-discriminated `ArtifactReceipt` family across every production mode, including the `Credential` and `Media` cases and the `Verdict` case carrying the `typography/sign` `ConformanceVerdict`.
- [19]-[CREDENTIAL](.planning/provenance/credential.md): `c2pa-python` content-credential manifest sign/read/verify/ingredient keyed by the content key, binding the signed-artifact content key into the `csharp:Rasm.Persistence` store, contributing `ArtifactReceipt.Credential`.
- [20]-[TAGGED](.planning/accessibility/tagged.md): `Access` tagged-PDF structure owner folding the `documents/model` `StructureNode`/`StructEltKind` tree and `FigureNode.alt` into PDF marked-content over `pikepdf` `StructTreeRoot`/`StructElem`, the structural `AUDIT` closing the verdict `typography/sign` disclaims, reusing the `ArtifactReceipt.Egress`/`Pdf` case.
- [21]-[ENCODE](.planning/media/encode.md): `Media` container/codec encode owner over `av` `open`/`add_stream`/`VideoFrame.from_ndarray`(`rgb24`)/`mux` consuming the `figures/scene` orbit frame sequence in-process, contributing `ArtifactReceipt.Media`.
- [22]-[PLAN](.planning/pipeline/plan.md): `ArtifactPipeline` content-keyed sub-graph-elision plan folding each producer's `ContentIdentity` into `Keyed` `(ContentKey, Work)` pairs over `graphlib.TopologicalSorter`, consuming the runtime `execution` session-lane `Keyed` port and owning no cache/store/scheduler/DAG.
- [23]-[TEMPLATE](.planning/pipeline/template.md): `TemplatePipeline` input->output template/format owner binding a structured context through the `docxtpl`/`typst` `sys_inputs`/`jinja2`/`python-pptx`/`odfpy` surfaces into a chosen `TemplateTarget` output format, composing the emit/report binding owners for spec-sheet/sheet-set production.
- [24]-[LAYERED](.planning/export/layered.md): `LayeredExport` editable-export owner over the `ExportOp` family — `drawsvg` `Drawing`/`Group(id=...)`/`as_svg` named-layer SVG authoring on the cp315 core and `pikepdf` PDF OCG (`/OCProperties`/`/OCGs`/`/OCMD`) optional-content-group authoring on the gated band — binding placed `figures/compose` layouts as NAMED editable layers for Illustrator/InDesign hand-off.

## [02]-[DOMAIN_PACKAGES]

Every domain rendering library this folder uses. Versions are centralized in the one Python manifest; substrate packages live in `[3]-[SUBSTRATE_PACKAGES]` below. The cp314-bound rows (`pillow`, `matplotlib`, `scikit-image`, `lxml`, `brotli`, `lz4`, `python-bidi`) are gated `python_version<'3.15'`; `vtk`/`pyvista` are gated `python_version<'3.13'` and `usd-core` carries the wider `python_version<'3.15'` band, both resolving on the same sub-3.13 vtk worker; `vegafusion` carries the `python_version<'3.15'` band marker yet ships an abi3 wheel that imports on the cp315 core, so its chart-transform pre-pass arm runs on the subprocess lane by band policy, not by wheel limitation. `typst` ships a cp38-abi3 wheel installing on the cp315 core; `segno`, `great-tables`, and `drawsvg` are pure-Python, cp315-clean. A gated row never resolves in the cp315-core process: its owner dispatches the gated arm onto the runtime subprocess lane (`anyio.to_process.run_sync`), and the gated-band worker imports the package at module scope. Four native libraries arrive from the Forge environment rather than a wheel: `libvips` backs `pyvips`, `leptonica` and `tesseract` back `ocrmypdf` OCR, and `ghostscript` backs the `ocrmypdf` PDF/A render path.

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
- `zxing-cpp` - 2D-matrix symbology owner — DataMatrix/PDF417/Aztec/MaxiCode encode and `read_barcodes` decode round-trip — beside the python-barcode linear arm
- `pyvips` - Fast libvips raster pipeline — resize/thumbnail/format-convert/composite — on the libvips Forge native floor
- `resvg-py` - Resvg SVG-to-raster render with accurate font/filter support
- `svgelements` - Pure-Python SVG geometry/transform/parse — `SVG.parse`/`Path`/`Matrix`/`bbox` — owning the `figures/compose` SVG scale-to-fit/n-up/crop/bounds composition over the SVG the chart/QR/nanoplot owners emit, cp315-clean
- `drawsvg` - Hierarchical named-layer SVG authoring — `Drawing`/`Group(id=...)`/`as_svg` — for the `export/layered` Illustrator/InDesign editable hand-off, pure `py3-none-any`, cp315-clean
- `python-magic`

[Color]:
- `colour-science`
- `coloraide` - CSS-space color parse/interpolate/gamut-map for web-facing palette egress

[Typography]:
- `fonttools`
- `pyhanko`
- `uharfbuzz` - OpenType text shaping — the `Blob`/`Face`/`Font`/`Buffer`/`shape`/`GlyphInfo`/`GlyphPosition` pipeline, COLRv1 `PaintFuncs`, `Font.set_variations`, and the `draw_glyph_with_pen` fontTools outline bridge
- `blackrenderer` - COLRv1 color-glyph raster/SVG rendering over the uharfbuzz/fontTools paint chain
- `python-bidi` - UAX#9 bidirectional reorder pass before HarfBuzz shaping for mixed-direction Arabic/Hebrew runs, feeding `typography/shape`; gated `python_version<'3.15'` (Rust pyo3 lacks a cp315 wheel)

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
