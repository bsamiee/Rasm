# [PY_ARTIFACTS_API_PDF_OXIDE]

`pdf_oxide` supplies a Rust-core, dependency-free, commercial-safe (MIT OR Apache-2.0) PDF surface that owns one document root for extract/render/edit/redact/sanitize/sign and three creation entries for fluent/declarative/markup authoring: `PdfDocument` (open-and-edit an existing PDF — layout-aware text, word/char/line/span geometry, native tables, render-to-pixmap, ink separation plates for prepress, redaction-grade scrub, PDF/A-PDF/X-PDF/UA validate+convert, Office round-trip), `Pdf`/`DocumentBuilder`/`FluentPageBuilder` (create from text/HTML/Markdown/images or a tagged fluent page chain), the PAdES `sign_pdf_bytes_pades` + `Certificate`/`TsaClient` crypto family, and async mirrors (`AsyncPdfDocument`/`AsyncPdf`/`AsyncOfficeConverter`) that carry their own worker pool. The owner composes `PdfDocument`/`Pdf`/`DocumentBuilder` into the pdf rail across `document/lens#LENS` (extract), `document/emit#DOCUMENT`+`document/egress#FINISH` (author+finish), and `exchange/conformance#CONFORMANCE` (sign/validate); it never re-implements the Rust render/extract/redaction core, the `ExtractionProfile` layout heuristics, the `RenderedPixmap` premultiplied-RGBA buffer, or the PAdES signed-attribute model the native crate already owns. The catalog's load-bearing fact is license: where `pymupdf` (AGPL-3.0) bars a closed/distributed/network render-extract-redact path, `pdf_oxide` is the MIT/Apache categorical-best owner of that exact path — render (`render_page`/`render_pixmap`), layout-aware extract (`extract_words`/`to_markdown`), redaction (`add_redaction`/`apply_redactions_destructive`), and prepress separations (`render_separations`) all ship permissively, so the commercial-safe pdf rail routes through it and reserves the AGPL siblings for internal/permissive pipelines.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pdf_oxide`
- package: `pdf-oxide` (dist `pdf_oxide`)
- import: `pdf_oxide`
- owner: `artifacts`
- rail: pdf
- installed: `0.3.73`
- license: `MIT OR Apache-2.0` — permissive, no copyleft; this is the load-bearing differentiator from AGPL `pymupdf`. A closed-source distributed/SaaS render-extract-redact-separation pipeline routes here without source-disclosure obligation; `pdf_oxide` is the commercial-safe categorical-best PDF render/extract/redact/prepress owner and the supersession candidate for the AGPL-bound arms of that path
- build floor: Rust core, `cp38-abi3` wheel (`pdf_oxide.pdf_oxide.abi3.so` via maturin) — one abi3 wheel forward-compatible across all CPython including 3.15; `Requires-Python >=3.8`, no cp-gate, ungated in the manifest. Self-contained: no Python runtime deps (no Pillow/cryptography/lxml), the Rust core carries codecs, crypto providers, OCR, and Office conversion in-process
- target: `osx-arm64` (and every wheel platform); resolved/reflected on cp315 `osx-arm64`
- entry points: none (library only)
- typed surface: ships `pdf_oxide/pdf_oxide.pyi` (94 KB) — the authoritative signatures this catalog is verified against
- capability: open + authenticate + edit a PDF (`PdfDocument`), reading-order/layout-aware text extraction with pre-tuned `ExtractionProfile`s, word/char/line/span geometry, native ruled+text table extraction, region-restricted extraction, structured-element + tagged-structure recovery, document/page classification, full-text regex search, native outline + form-field + annotation + layer + image harvest, in-process render to PNG/JPEG/raw `RenderedPixmap` (premultiplied RGBA8888), ink `SeparationPlate` rendering for prepress/QC, `flatten_to_images`, redaction marking + redaction-grade destructive scrub + sanitize, annotation/form authoring and flatten, header/footer/artifact erase, image reposition/resize, page assembly/select/move/delete/crop/rotate, AcroForm read/fill/export (FDF/XFDF), Markdown/HTML/plain-text/DOCX/XLSX/PPTX conversion both directions, PDF/A + PDF/X + PDF/UA validation and PDF/A conversion, compressed/linearized/encrypted save, PAdES B-B…B-LTA digital signing with RFC-3161 timestamping + DSS/LTV + signature verification, pluggable crypto providers with FIPS mode + CBOM, fluent tagged-PDF/UA document authoring, embedded fonts, gradients/patterns/blend-modes, barcode/QR SVG, and bookmark-driven document split — with native async mirrors

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document roots and creation entries
- rail: pdf

`PdfDocument` is the single open-edit-extract-render root; `Pdf`/`DocumentBuilder` are the two creation modalities (declarative-from-source vs fluent-tagged-chain). `AsyncPdfDocument`/`AsyncPdf`/`AsyncOfficeConverter` mirror them on a built-in single-worker thread pool — `pdf_oxide` owns its async offload natively, so the rail composes `anyio.to_thread.run_sync` for the *sync* root and the native async mirror where the consumer is already on the loop.

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfDocument` | document root | open/authenticate/edit an existing PDF; extract, render, redact, sanitize, sign-read, validate, save |
| [02] | `Pdf` | creation root | build a PDF from text/HTML/Markdown/image(s)/merge via `from_*` static factories |
| [03] | `DocumentBuilder` | fluent builder | high-level fluent tagged-PDF/UA author; `register_embedded_font`, per-page `a4_page`/`letter_page`/`page`, `build` |
| [04] | `FluentPageBuilder` | page builder | buffered page-op chain returned by `DocumentBuilder.{a4_page,letter_page,page}`; single-use, sealed by `done()` |
| [05] | `PdfPage` | page edit view | per-page edit/annotate surface returned by `PdfDocument.page(index)`: `add_text`/`add_highlight`/`add_link`/`add_note`/`set_text`/`find_text_containing`/`find_images`/`get_element`/`children`/`annotations`/`remove_annotation`/`remove_element` + `width`/`height`/`index` |
| [06] | `Page` | page extract view | per-page extract/render surface yielded by iterating `PdfDocument.pages`: `text`/`words`/`chars`/`lines`/`spans`/`tables`/`images`/`paths`/`annotations`/`markdown`/`plain_text`/`html`/`render`/`render_pixmap`/`search`/`region` + `bbox`/`width`/`height`/`index` |
| [07] | `AsyncPdfDocument` | async root | full `PdfDocument` mirror over an owned single-worker pool; `open`/`from_bytes`/`close` + every op as a coroutine |
| [08] | `AsyncPdf` | async creation | `Pdf` mirror; `from_*` static coroutines returning `AsyncPdf`, `save`/`to_bytes` |
| [09] | `OfficeConverter` / `AsyncOfficeConverter` | office gate | DOCX/PPTX/XLSX → `Pdf`/`AsyncPdf` (`convert`/`from_docx`/`from_pptx`/`from_xlsx` + `_bytes` variants) |

[PUBLIC_TYPE_SCOPE]: extraction value objects
- rail: pdf

Geometry-bearing read models returned by the `PdfDocument.extract_*` / `Page` accessors; every `bbox` is a `(x0, y0, x1, y1)` float tuple in PDF user space. These feed the `document/lens#LENS` `RunNode`/`FigureNode`/`TableNode` lowering — `pdf_oxide` is the layout-aware extract engine the lens routes through, never re-clustering chars by hand.

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `TextChar` | glyph | one char: `char`/`bbox`/`font_name`/`font_size`/`font_weight`/`is_italic`/`is_monospace`/`color`/`rotation_degrees`/`origin_x`/`origin_y`/`advance_width`/`mcid` |
| [02] | `TextWord` | word | `text`/`bbox`/`font_name`/`font_size`/`is_bold`/`is_italic`/`chars: list[TextChar]` |
| [03] | `TextLine` | line | `text`/`bbox`/`words: list[TextWord]`/`chars: list[TextChar]` |
| [04] | `TextSpan` | styled span | `text`/`bbox`/`font_name`/`font_size`/`color`/`is_bold`/`is_italic`/`is_monospace`/`char_widths` (the styled-run unit feeding `RunNode`) |
| [05] | `FormField` | AcroForm field | `name`/`field_type`/`value`/`tooltip`/`bounds`/`flags`/`max_length`/`is_readonly`/`is_required` |
| [06] | `PdfPageRegion` | region view | bbox-restricted re-extraction: `extract_text`/`extract_words`/`extract_text_lines`/`extract_tables`/`extract_images`/`extract_paths` |
| [07] | `PdfElement` | structured node | `is_text`/`is_image`/`is_path`/`is_table`/`is_structure` discriminants + `as_text`/`as_image` + `bbox` |
| [08] | `PdfText` / `PdfImage` / `PdfAnnotation` | element leaves | recovered text (`value`/`font_name`/`is_bold`/`contains`/`starts_with`), image (`width`/`height`/`aspect_ratio`/`bbox`), annotation (`subtype`/`rect`/`contents`/`color`/`is_modified`/`is_new`) |
| [09] | `LayoutParams` | layout metrics | computed per-page `column_count`/`word_gap_threshold`/`line_gap_threshold`/`median_font_size`/`median_line_spacing`/`median_char_width` |

[PUBLIC_TYPE_SCOPE]: render buffers
- rail: pdf

Native render outputs. `RenderedPixmap` is a raw premultiplied-RGBA8888 buffer (PDF §11 transparency model) the raster owner wraps without re-decoding; `SeparationPlate` is the prepress/QC differentiator — a single-ink grayscale plate where pixel intensity == ink coverage percentage, the categorical-best Python entry into per-ink separation analysis.

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `RenderedPixmap` | raster buffer | `data: bytes` (row-major, top-left origin, 4 B premultiplied RGBA/pixel, `len == width*height*4`) + `width`/`height` |
| [02] | `SeparationPlate` | ink plate | one grayscale ink separation: `data` (0–255 == tint %)`/`width`/`height`/`ink_name`; prepress ink-coverage + ML/QC source |

[PUBLIC_TYPE_SCOPE]: authoring value objects — color, graphics, fonts, tables
- rail: pdf

Construct-then-pass value objects fed into the `Pdf`/`DocumentBuilder`/`FluentPageBuilder` creation chain. Colors are normalized 0..1 float RGB; the gradient/pattern/blend family is the full publication-grade graphics axis the create rail composes.

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `Color` | rgb value | `Color(r, g, b)` 0..1 floats; `from_hex(str)` + `black`/`white`/`red`/`green`/`blue` presets; `.r`/`.g`/`.b` |
| [02] | `LinearGradient` | linear fill | `start`/`end` + `add_stop(offset, color)`; `horizontal(width, start, end)`/`vertical(height, start, end)` helpers |
| [03] | `RadialGradient` | radial fill | `inner_circle`/`outer_circle` + `add_stop`; `centered(x, y, radius)` helper |
| [04] | `PatternPresets` | tiling patterns | `checkerboard`/`crosshatch`/`diagonal_lines`/`dots`/`horizontal_stripes`/`vertical_stripes` tiling-pattern factories |
| [05] | `BlendMode` | blend enum | `NORMAL`/`MULTIPLY`/`SCREEN`/`OVERLAY`/`DARKEN`/`LIGHTEN`/`COLOR_DODGE`/`COLOR_BURN`/`HARD_LIGHT`/`SOFT_LIGHT`/`DIFFERENCE`/`EXCLUSION` (PDF blend modes) |
| [06] | `ExtGState` | graphics state | `fill_alpha`/`stroke_alpha`/`alpha`/`blend_mode(mode)` + `semi_transparent()` transparency state |
| [07] | `LineCap` / `LineJoin` | stroke style | `BUTT`/`ROUND`/`SQUARE` caps; `MITER`/`ROUND`/`BEVEL` joins |
| [08] | `EmbeddedFont` | font handle | one-shot TTF/OTF handle: `from_file(path)`/`from_bytes(data, name=None)` + `.name`; moved into the builder by `register_embedded_font` (registering twice raises) |
| [09] | `Table` / `Column` / `Align` | buffered table | `Column(header, width=100.0, align=Align.LEFT)` + `Table` buffered grid for `FluentPageBuilder.table`; `Align.{LEFT,CENTER,RIGHT}` |
| [10] | `StreamingTable` | streaming table | row-by-row `push_row`/`push_row_span`/`flush`/`finish` for large grids; `batch_count`/`column_count`/`pending_row_count` |
| [11] | `PageTemplate` / `Header` / `Footer` / `Artifact` / `ArtifactStyle` | running content | running header/footer + tagged artifact templates (`Header.center`/`Footer.center`/`ArtifactStyle.bold`/`.font`) for `Pdf.from_markdown_with_template` |
| [12] | `ExtractionProfile` | extract tuning | pre-tuned profile: `conservative`/`aggressive`/`balanced`/`academic`/`policy`/`form`/`government`/`scanned_ocr`/`adaptive`; `available()` lists; tuning props (`word_margin_ratio`/`space_threshold_em_ratio`/…) |
| [13] | `OcrEngine` / `OcrConfig` | ocr config | OCR engine + config handles passed to `extract_text_ocr(engine=)` (opaque constructors; built via module OCR model functions) |

[PUBLIC_TYPE_SCOPE]: signing and crypto value objects
- rail: pdf

The PAdES digital-signature + RFC-3161 timestamp family feeding `exchange/conformance#CONFORMANCE`. `PadesLevel` is a frozen integer mapping (B_B=0/B_T=1/B_LT=2/B_LTA=3) shared with the C ABI — never renumbered. This is an in-process PAdES signer with TSA + DSS/LTV; it stacks beside `pyhanko` (the conformance owner) as the dependency-free signer where `pyhanko`'s cryptography stack is barred.

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `Certificate` | x509 + signer | `load(der)`/`load_pem(cert_pem, key_pem)`/`load_pkcs12(data, password)`; `subject`/`issuer`/`serial`/`validity: (int,int)`/`is_valid` |
| [02] | `Signature` | existing sig | one embedded `/Sig`: `signer_name`/`reason`/`location`/`contact_info`/`signing_time`/`covers_whole_document`/`pades_level`; `verify()`/`verify_detached(pdf_data)` |
| [03] | `PadesLevel` | level enum | frozen `B_B`/`B_T`/`B_LT`/`B_LTA` (+ `BB`/`BT`/`BLt`/`BLta` aliases); `int` → 0..3 ABI code |
| [04] | `TsaClient` | RFC-3161 client | `TsaClient(url, username=None, password=None, timeout_seconds=30, hash_algorithm=2, use_nonce=True, cert_req=True)`; `request_timestamp(data)`/`request_timestamp_hash(hash, hash_algorithm)` → `Timestamp` |
| [05] | `Timestamp` | timestamp token | `parse(der)`; `time`/`serial`/`policy_oid`/`tsa_name`/`hash_algorithm`/`message_imprint`/`verify()` |
| [06] | `RevocationMaterial` | LTV material | `RevocationMaterial(certs=, crls=, ocsps=)` offline B-LT validation material (DER certs/CRLs/OCSP responses) |
| [07] | `Dss` | security store | parsed `/DSS` (ISO 32000-2 §12.8.4.3): `certs`/`crls`/`ocsps` DER blobs + `vri` per-signature VRI keys |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open, authenticate, save
- rail: pdf

`PdfDocument(path, password=)` and `from_bytes(data, password=)` are the two intake forms; `save`/`to_bytes` and their `_encrypted` variants are the emission surface. `page_count`/`pages`/`page(i)` walk the page space; `version` reads the PDF version.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfDocument` | `PdfDocument(path: Path \| str, password: str \| None = None)` | open an existing PDF (decrypting if `password`) |
| [02] | `PdfDocument.from_bytes` | `from_bytes(data: bytes, password: str \| None = None) -> PdfDocument` | open from an in-memory byte stream |
| [03] | `PdfDocument.authenticate` | `authenticate(password) -> ...` | unlock an encrypted document post-open |
| [04] | `PdfDocument.page` / `pages` / `page_count` | `page(index) -> PdfPage` (edit view) / `pages -> PdfDocumentIter` yielding `Page` (extract view) / `page_count -> int` | per-page edit view, extract-view iterator, page count |
| [05] | `PdfDocument.save` | `save(path, compress=True, garbage_collect=True, linearize=False)` | save with compaction / GC / linearization |
| [06] | `PdfDocument.to_bytes` | `to_bytes(compress=True, garbage_collect=True, linearize=False) -> bytes` | serialize the edited document to bytes |
| [07] | `PdfDocument.save_encrypted` | `save_encrypted(path, user_password, owner_password=None, allow_print=True, allow_copy=True, allow_modify=True, allow_annotate=True)` | save with RC4/AES encryption + permission flags |
| [08] | `PdfDocument.to_bytes_encrypted` | `to_bytes_encrypted(user_password, owner_password=None, allow_print=, allow_copy=, allow_modify=, allow_annotate=) -> bytes` | encrypted serialize to bytes |
| [09] | `PdfDocument.permissions` / `version` / `save_page` | `permissions() -> ...` / `version() -> str` / `save_page(page) -> bytes` | read permission flags, PDF version, single-page bytes |

[ENTRYPOINT_SCOPE]: layout-aware text extraction and conversion
- rail: pdf

The reading-order/layout-aware extraction surface — `pdf_oxide`'s headline capability and the `document/lens#LENS` engine for the commercial-safe path. `to_*` convert whole-doc or per-page to Markdown/HTML/plain text; `extract_words`/`extract_text_lines`/`extract_chars`/`extract_spans` carry geometry; an `ExtractionProfile` tunes the layout heuristics per document class.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfDocument.extract_text` | `extract_text(page, region=None, exclude_layers=None, exclude_inks=None, extract_tables=True) -> str` | reading-order page text, layer/ink-excludable |
| [02] | `PdfDocument.extract_words` | `extract_words(page, *, include_artifacts=True, region=None, word_gap_threshold=None, profile: ExtractionProfile \| None = None) -> list[TextWord]` | word-geometry extraction → `RunNode` leaves |
| [03] | `PdfDocument.extract_text_lines` | `extract_text_lines(page, *, include_artifacts=True, region=None, word_gap_threshold=None, line_gap_threshold=None, profile=None) -> list[TextLine]` | line-grouped geometry extraction |
| [04] | `PdfDocument.extract_chars` | `extract_chars(page, region=None, exclude_layers=None, exclude_inks=None) -> list[TextChar]` | per-glyph extraction with full font/color/origin metrics |
| [05] | `PdfDocument.extract_spans` | `extract_spans(page, region=None, reading_order=None) -> list[TextSpan]` | styled-run extraction → `RunNode` styled spans |
| [06] | `PdfDocument.extract_tables` | `extract_tables(page, region=None, table_settings: dict \| None = None) -> Any` | native ruled+text table grid → `TableNode` |
| [07] | `PdfDocument.extract_structured` | `extract_structured(page) -> str` (+ `extract_text_auto`/`extract_page_auto(options_json=)`) | structured/auto-profile extraction (JSON-serialized) |
| [08] | `PdfDocument.extract_lines` / `extract_rects` / `extract_paths` | `extract_lines(page, region=)` / `extract_rects(...)` / `extract_paths(...)` | vector line/rect/path geometry harvest |
| [09] | `PdfDocument.extract_text_ocr` | `extract_text_ocr(page, engine: OcrEngine \| None = None) -> str` | OCR a scanned page in-process (Rust OCR engine) |
| [10] | `PdfDocument.to_markdown` / `to_markdown_all` | `to_markdown(page, preserve_layout=False, detect_headings=True, include_images=False, image_output_dir=None, embed_images=True, include_form_fields=True) -> str` | page/whole-doc Markdown conversion |
| [11] | `PdfDocument.to_html` / `to_html_all` | `to_html(page, preserve_layout=, detect_headings=, include_images=, image_output_dir=, embed_images=, include_form_fields=) -> str` | page/whole-doc HTML conversion |
| [12] | `PdfDocument.to_plain_text` / `to_plain_text_all` | `to_plain_text(page, preserve_layout=, detect_headings=, include_images=, image_output_dir=) -> str` | page/whole-doc plain-text conversion |
| [13] | `PdfDocument.search` / `search_page` | `search(pattern, case_insensitive=False, literal=False, whole_word=False, max_results=0)` / `search_page(page, pattern, ...)` | full-document / per-page regex (or literal) search |
| [14] | `PdfDocument.classify_document` / `classify_page` | `classify_document() -> str` / `classify_page(page) -> str` | document/page type classification (e.g. form/scanned) |
| [15] | `PdfDocument.page_layout_params` | `page_layout_params(page) -> LayoutParams` | computed adaptive layout metrics for a page |

[ENTRYPOINT_SCOPE]: render to raster and ink separations
- rail: pdf

The commercial-safe render surface. `render_page`/`render_page_fit` emit encoded PNG/JPEG bytes; `render_pixmap` returns the raw premultiplied-RGBA `RenderedPixmap`; `render_separation`/`render_separations` are the prepress differentiator (per-ink grayscale coverage plates); `flatten_to_images` rasterizes every page.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfDocument.render_page` | `render_page(page, dpi=None, format=None, background=None, transparent=False, render_annotations=None, jpeg_quality=None, excluded_layers=None) -> bytes` | render a page to encoded PNG/JPEG bytes at DPI |
| [02] | `PdfDocument.render_page_fit` | `render_page_fit(page, width, height, *, format=None, background=None, transparent=False, render_annotations=None, jpeg_quality=None, excluded_layers=None) -> bytes` | render fit to a target pixel box |
| [03] | `PdfDocument.render_pixmap` | `render_pixmap(page, dpi=None) -> RenderedPixmap` | raw premultiplied RGBA8888 buffer (no encode) |
| [04] | `PdfDocument.render_separation` | `render_separation(page, ink_name, dpi=None) -> SeparationPlate` | one named-ink coverage plate (prepress/QC) |
| [05] | `PdfDocument.render_separations` | `render_separations(page, dpi=None) -> list[SeparationPlate]` | every ink's coverage plate at once |
| [06] | `PdfDocument.flatten_to_images` | `flatten_to_images(dpi=150) -> ...` | rasterize all pages (flatten interactive layers to raster) |

[ENTRYPOINT_SCOPE]: redaction, sanitize, and content erase
- rail: pdf

Redaction-grade removal — the commercial-safe alternative to the AGPL pymupdf `scrub`/`apply_redactions` family for the `document/egress#FINISH` REDACT/SCRUB steps. `add_redaction` marks; `apply_*` burns; `apply_redactions_destructive` is the irreversible scrub; the `erase_*`/`remove_*` family strips headers/footers/artifacts by repeat-detection threshold.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfDocument.add_redaction` | `add_redaction(page, rect, fill=None)` | mark a region for redaction |
| [02] | `PdfDocument.apply_page_redactions` / `apply_all_redactions` | `apply_page_redactions(page)` / `apply_all_redactions()` | burn marked redactions on a page / whole document |
| [03] | `PdfDocument.apply_redactions_destructive` | `apply_redactions_destructive(scrub_metadata=True, remove_javascript=True, remove_embedded_files=True, fill=...)` | irreversible redaction + metadata/JS/attachment scrub |
| [04] | `PdfDocument.sanitize_document` | `sanitize_document(scrub_metadata=True, remove_javascript=True, remove_embedded_files=True)` | sanitize hidden/JS/attachment content (non-destructive of visible) |
| [05] | `PdfDocument.erase_region` / `erase_regions` | `erase_region(page, llx, lly, urx, ury)` / `erase_regions(page, rects)` | erase content within rect(s) |
| [06] | `PdfDocument.erase_header` / `erase_footer` / `erase_artifacts` | `erase_header(page)` / `erase_footer(page)` / `erase_artifacts(page)` | erase running header/footer/artifact content on a page |
| [07] | `PdfDocument.remove_headers` / `remove_footers` / `remove_artifacts` | `remove_headers(threshold=0.8)` / `remove_footers(...)` / `remove_artifacts(...)` | repeat-detection removal of headers/footers/artifacts across the doc |
| [08] | `PdfDocument.redaction_count` / `is_page_marked_for_redaction` / `clear_erase_regions` | `redaction_count(page)` / `is_page_marked_for_redaction(page)` / `clear_erase_regions(page)` | redaction-state queries + erase reset (`unmark_page_for_redaction` cancels a mark) |

[ENTRYPOINT_SCOPE]: page assembly, geometry, and image edit
- rail: pdf

In-place page-space editing and embedded-image manipulation — the structural-edit surface feeding `document/egress#FINISH` composition.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfDocument.merge_from` | `merge_from(source: PdfDocument)` | append another document's pages |
| [02] | `PdfDocument.select_pages` / `move_page` / `delete_page` | `select_pages(pages)` / `move_page(from_index, to_index)` / `delete_page(index)` | subset/reorder/drop pages |
| [03] | `PdfDocument.extract_pages` / `extract_pages_to_bytes` / `extract_page_ranges_to_bytes` | `extract_pages(pages, output)` / `extract_pages_to_bytes(pages) -> bytes` / `extract_page_ranges_to_bytes(ranges)` | split selected pages/ranges to a file or bytes |
| [04] | `PdfDocument.rotate_page` / `rotate_all_pages` / `set_page_rotation` | `rotate_page(page, degrees)` / `rotate_all_pages(degrees)` / `set_page_rotation(page, degrees)` | rotate one/all pages (`page_rotation(page)` reads) |
| [05] | `PdfDocument.crop_margins` / `set_page_crop_box` / `set_page_media_box` | `crop_margins(left, right, top, bottom)` / `set_page_crop_box(page, llx, lly, urx, ury)` / `set_page_media_box(...)` | crop margins / set crop+media boxes (`page_crop_box`/`page_media_box` read) |
| [06] | `PdfDocument.page_images` / `extract_images` / `extract_image_bytes` | `page_images(page)` / `extract_images(page, region=None)` / `extract_image_bytes(page)` | enumerate / harvest placed images + raw bytes |
| [07] | `PdfDocument.reposition_image` / `resize_image` / `set_image_bounds` | `reposition_image(page, image_name, x, y)` / `resize_image(page, image_name, width, height)` / `set_image_bounds(page, image_name, x, y, width, height)` | move/resize an embedded image in place (`has_image_modifications`/`clear_image_modifications` track) |
| [08] | `PdfDocument.within` | `within(page, bbox) -> PdfPageRegion` | a bbox-restricted region view for re-extraction |

[ENTRYPOINT_SCOPE]: forms, annotations, outline, layers, metadata
- rail: pdf

AcroForm read/fill/export, annotation + form flatten, native outline/layer/label harvest, and document-info + XMP metadata edit — the recovery + finishing surface feeding `document/lens#LENS` (read) and `document/egress#FINISH` (flatten/outline/metadata).

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfDocument.get_form_fields` | `get_form_fields() -> list[FormField]` | recover all AcroForm fields with type/value/bounds/flags |
| [02] | `PdfDocument.get_form_field_value` / `set_form_field_value` | `get_form_field_value(name)` / `set_form_field_value(name, value)` | read / fill a single form field |
| [03] | `PdfDocument.export_form_data` | `export_form_data(path, format='fdf')` | export filled form data as FDF/XFDF |
| [04] | `PdfDocument.flatten_forms` / `flatten_forms_on_page` | `flatten_forms()` / `flatten_forms_on_page(page)` | flatten interactive form fields into static content |
| [05] | `PdfDocument.get_annotations` | `get_annotations(page) -> Any` (list of `PdfAnnotation`) | recover page annotations (`subtype`/`rect`/`contents`/`color`) |
| [06] | `PdfDocument.flatten_all_annotations` / `flatten_page_annotations` | `flatten_all_annotations()` / `flatten_page_annotations(page)` | flatten annotations into content |
| [07] | `PdfDocument.get_outline` | `get_outline() -> Any \| None` | recover the native bookmark outline tree → `StructureNode` |
| [08] | `PdfDocument.get_layers` / `get_page_inks` / `get_page_inks_deep` | `get_layers() -> list[str]` / `get_page_inks(page)` / `get_page_inks_deep(page)` | enumerate OCG layers / page ink (separation) names |
| [09] | `PdfDocument.page_labels` | `page_labels() -> Any` | recover the page-label numbering tree |
| [10] | `PdfDocument.set_title` / `set_author` / `set_subject` / `set_keywords` | `set_title(title)` / `set_author(author)` / `set_subject(subject)` / `set_keywords(keywords)` | edit document-info metadata fields |
| [11] | `PdfDocument.xmp_metadata` | `xmp_metadata() -> Any` | recover the XMP metadata packet |
| [12] | `PdfDocument.embed_file` | `embed_file(name, data)` | attach an embedded file |
| [13] | `PdfDocument.edit_header` / `edit_footer` | `edit_header(page)` / `edit_footer(page)` | author/replace running header/footer |
| [14] | `PdfPage` (from `page(index)`) | `add_text(text, x, y, font_size=12.0)` / `add_highlight(x, y, width, height, color)` / `add_link(x, y, width, height, url)` / `add_note(x, y, text)` / `set_text(text_id, new_text)` / `find_text_containing(needle)` / `find_images()` / `get_element(id)` / `children()` / `annotations()` / `remove_annotation(index)` / `remove_element(id)` | per-page in-place edit: add text/annotations/links, retext an element, find/remove elements |

[ENTRYPOINT_SCOPE]: structure, conformance validation, and PDF/A conversion
- rail: pdf

Tagged-structure recovery + the standards-validation triple — the `exchange/conformance#CONFORMANCE` and `document/tagged#ACCESS` engines. `validate_pdf_a`/`validate_pdf_ua`/`validate_pdf_x` are the conformance oracle; `convert_to_pdf_a` is the archival upgrade.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfDocument.has_structure_tree` / `has_text_layer` / `has_xfa` | `has_structure_tree() -> bool` / `has_text_layer(page) -> bool` / `has_xfa() -> bool` | tagged-PDF / text-layer / XFA presence discriminants |
| [02] | `PdfDocument.structured_warnings` / `flatten_warnings` | `structured_warnings() -> ...` / `flatten_warnings() -> ...` | structure/flatten diagnostic warnings |
| [03] | `PdfDocument.validate_pdf_a` | `validate_pdf_a(level: str = "1b") -> Any` | PDF/A conformance validation (e.g. `"1b"`/`"2b"`/`"3b"`) |
| [04] | `PdfDocument.validate_pdf_ua` | `validate_pdf_ua() -> Any` | PDF/UA accessibility validation |
| [05] | `PdfDocument.validate_pdf_x` | `validate_pdf_x(level: str = "1a_2001") -> Any` | PDF/X print-production validation |
| [06] | `PdfDocument.convert_to_pdf_a` | `convert_to_pdf_a(level: str = "2b") -> Any` | upgrade the document to PDF/A archival conformance |

[ENTRYPOINT_SCOPE]: existing-signature read and verify
- rail: pdf

Recover and verify embedded signatures + the DSS store — the read half of `exchange/conformance#CONFORMANCE` `audit`.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfDocument.signatures` | `signatures() -> list[Signature]` | recover every embedded `/Sig` (then `.verify()`/`.verify_detached(data)`) |
| [02] | `PdfDocument.signature_count` | `signature_count() -> int` | embedded-signature count |
| [03] | `PdfDocument.dss` | `dss() -> Dss \| None` | recover the `/DSS` document-security store (certs/CRLs/OCSPs/VRI) |

[ENTRYPOINT_SCOPE]: Office round-trip conversion
- rail: pdf

Both directions of Office ↔ PDF in-process (no LibreOffice subprocess) — the `OfficeConverter` import path and the `PdfDocument.to_*` export path.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `OfficeConverter.from_docx` / `from_pptx` / `from_xlsx` | `from_docx(path) -> Pdf` / `from_pptx(path)` / `from_xlsx(path)` (+ `_bytes(data)` + `convert(path)`) | DOCX/PPTX/XLSX → `Pdf` in-process |
| [02] | `PdfDocument.to_docx` / `to_xlsx` / `to_pptx` | `to_docx(path)` / `to_xlsx(path)` / `to_pptx(path)` (+ `_bytes() -> bytes` variants) | PDF → DOCX/XLSX/PPTX export |

[ENTRYPOINT_SCOPE]: PDF creation — declarative `Pdf` factories
- rail: pdf

The declarative creation modality: one content source → a `Pdf`. `from_html_css`/`from_html_css_with_fonts` carry embedded-font bytes; `from_markdown_with_template` applies a running-header/footer `PageTemplate`. (Note: the `Pdf.create()` shown in the package docstring is not a real classmethod — the entries are the `from_*`/`merge` factories below plus `DocumentBuilder`.)

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `Pdf.from_markdown` | `from_markdown(content: str, title=None, author=None) -> Pdf` | author a PDF from Markdown |
| [02] | `Pdf.from_html` | `from_html(content: str, title=None, author=None) -> Pdf` | author a PDF from HTML |
| [03] | `Pdf.from_text` | `from_text(content: str, title=None, author=None) -> Pdf` | author a PDF from plain text |
| [04] | `Pdf.from_html_css` / `from_html_css_with_fonts` | `from_html_css(html, css, font_bytes: bytes) -> Pdf` / `from_html_css_with_fonts(html, css, fonts: list[tuple[str, bytes]])` | author from HTML+CSS with one / many embedded fonts |
| [05] | `Pdf.from_markdown_with_template` | `from_markdown_with_template(content, template: PageTemplate, title=None, author=None) -> Pdf` | Markdown author with running header/footer template |
| [06] | `Pdf.from_image` / `from_images` / `from_image_bytes` | `from_image(path) -> Pdf` / `from_images(paths: list[str])` / `from_image_bytes(data)` | wrap one/many raster images into a PDF |
| [07] | `Pdf.merge` | `merge(paths: list[str]) -> Pdf` | concatenate existing PDFs by path |
| [08] | `Pdf.from_bytes` / `save` / `to_bytes` | `from_bytes(data) -> Pdf` / `save(path)` / `to_bytes() -> bytes` (+ `len(pdf)` page count) | round-trip bytes, emit to path/bytes |

[ENTRYPOINT_SCOPE]: PDF creation — fluent tagged `DocumentBuilder` / `FluentPageBuilder`
- rail: pdf

The fluent tagged-PDF/UA authoring modality: each builder method mutates `self` and returns `self` for chaining. `DocumentBuilder` owns document-level metadata + font registration + page creation + `build`; `tagged_pdf_ua1()` makes the output PDF/UA-1; `a4_page`/`letter_page`/`page(w,h)` open a `FluentPageBuilder` sealed by `done()`. `FluentPageBuilder` carries the full publication content vocabulary — text/headings/paragraphs/footnotes, fields, annotations, vector primitives, images, tables, links, and watermarks.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `DocumentBuilder` | `DocumentBuilder()` → `.title`/`.author`/`.subject`/`.keywords`/`.creator`/`.language`/`.on_open(script)` | construct + set document metadata (fluent) |
| [02] | `DocumentBuilder.register_embedded_font` | `register_embedded_font(name: str, font: EmbeddedFont) -> DocumentBuilder` | register a TTF/OTF face for use by `font(name, size)` |
| [03] | `DocumentBuilder.tagged_pdf_ua1` / `role_map` | `tagged_pdf_ua1() -> DocumentBuilder` / `role_map(custom, standard)` | emit tagged PDF/UA-1 + custom→standard structure role map |
| [04] | `DocumentBuilder.a4_page` / `letter_page` / `page` | `a4_page() -> FluentPageBuilder` / `letter_page()` / `page(width, height)` | open a page builder (A4 / Letter / custom size) |
| [05] | `DocumentBuilder.build` / `save` / `save_encrypted` / `to_bytes_encrypted` | `build() -> bytes` / `save(path)` / `save_encrypted(path, user_password, owner_password)` / `to_bytes_encrypted(...)` | finalize to bytes / file / encrypted output |
| [06] | `FluentPageBuilder` text | `font(name, size)` / `at(x, y)` / `text(t)` / `paragraph(t)` / `heading(level, t)` / `columns(column_count, gap_pt, text)` / `text_in_rect(x, y, w, h, t, align=)` / `inline*`/`newline`/`space`/`measure(t)`/`remaining_space` | positioned + flowing text, headings, multi-column, inline runs |
| [07] | `FluentPageBuilder` graphics | `line` / `rect` / `filled_rect(x, y, w, h, r, g, b)` / `stroke_line(...)` / `stroke_rect(...)` / `stroke_line_dashed(...)` / `horizontal_rule` | vector primitives (stroked/filled/dashed) |
| [08] | `FluentPageBuilder` content | `image_artifact(bytes, x, y, w, h)` / `image_with_alt(..., alt_text)` / `table(table: Table)` / `streaming_table(columns, repeat_header=, mode=, sample_rows=, ...)` / `footnote(ref_mark, note_text)` | tagged image, buffered/streaming table, footnote |
| [09] | `FluentPageBuilder` annotations | `highlight(color)` / `underline` / `strikeout` / `squiggly` / `sticky_note(text)` / `sticky_note_at(x, y, text)` / `stamp(name)` / `freetext(x, y, w, h, text)` / `watermark(text)` / `watermark_confidential` / `watermark_draft` | annotation + watermark authoring |
| [10] | `FluentPageBuilder` forms | `text_field(name, x, y, w, h, default_value=)` / `checkbox(name, x, y, w, h, checked)` / `radio_group(name, buttons, selected=)` / `combo_box(name, x, y, w, h, options, selected=)` / `push_button(name, x, y, w, h, caption)` / `signature_field(name, x, y, w, h)` | AcroForm field authoring |
| [11] | `FluentPageBuilder` field scripts | `field_validate(script)` / `field_format(script)` / `field_calculate(script)` / `field_keystroke(script)` / `link_javascript(script)` / `on_open`/`on_close(script)` | JavaScript field-action authoring |
| [12] | `FluentPageBuilder` links + barcodes | `link_url(url)` / `link_page(page)` / `link_named(destination)` / `barcode_1d(barcode_type, data, x, y, w, h)` / `barcode_qr(data, x, y, size)` | hyperlinks + in-page 1D/QR barcodes |
| [13] | `FluentPageBuilder.done` / `new_page_same_size` | `done() -> DocumentBuilder` / `new_page_same_size() -> FluentPageBuilder` | seal the page (single-use) / open another same-size page |

[ENTRYPOINT_SCOPE]: module functions — signing, crypto policy, barcodes, split, logging
- rail: pdf

Free functions: byte-level PAdES signing (no `PdfDocument` round-trip), the pluggable crypto-provider + FIPS + CBOM surface, standalone barcode/QR SVG generation, bookmark-driven split, OCR model prefetch, and the stream-safety + logging controls.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `sign_pdf_bytes` | `sign_pdf_bytes(pdf_data: bytes, cert: Certificate, reason=None, location=None) -> bytes` | apply a basic (non-PAdES) signature to PDF bytes |
| [02] | `sign_pdf_bytes_pades` | `sign_pdf_bytes_pades(pdf_data: bytes, cert: Certificate, level: PadesLevel, tsa_url=None, reason=None, location=None, revocation: RevocationMaterial \| None = None) -> bytes` | PAdES B-B…B-LTA sign with TSA + DSS/LTV material |
| [03] | `has_document_timestamp` | `has_document_timestamp(pdf_data: bytes) -> bool` | detect a `/DocTimeStamp` proof-of-existence |
| [04] | `crypto_active_provider` / `crypto_available_providers` | `crypto_active_provider() -> str` / `crypto_available_providers() -> list[str]` | inspect the active / available crypto backends |
| [05] | `crypto_use_fips` / `crypto_policy` / `crypto_set_policy` | `crypto_use_fips()` / `crypto_policy() -> str` / `crypto_set_policy(spec: str)` | enable FIPS mode / read / set the crypto policy spec |
| [06] | `crypto_inventory` / `crypto_cbom` | `crypto_inventory() -> list[str]` / `crypto_cbom() -> str` | cryptographic inventory + CBOM (CycloneDX crypto BOM) |
| [07] | `generate_barcode_svg` / `generate_qr_svg` | `generate_barcode_svg(barcode_type: int, data: str) -> str` / `generate_qr_svg(data: str, error_correction: int, size: int) -> str` | standalone 1D-barcode / QR SVG strings (no PDF) |
| [08] | `plan_split_by_bookmarks` / `split_by_bookmarks` | `split_by_bookmarks(src_bytes: bytes, title_prefix=None, ignore_case=False, level=1, include_front_matter=True) -> list` (+ `plan_*` dry-run) | split a PDF into parts at bookmark boundaries |
| [09] | `pdf_oxide.pdf_oxide.prefetch_models` / `model_manifest` / `prefetch_available` | `prefetch_models(languages: list[str]) -> str` / `model_manifest() -> str` / `prefetch_available() -> bool` (inner-module access — not top-level re-exported) | pre-download / inspect OCR language models |
| [10] | `pdf_oxide.pdf_oxide.set_max_ops_per_stream` / `set_preserve_unmapped_glyphs` | `set_max_ops_per_stream(limit: int \| None) -> int \| None` / `set_preserve_unmapped_glyphs(preserve: bool) -> bool` (inner-module access — not top-level re-exported) | content-stream safety cap + unmapped-glyph policy (boundary hardening) |
| [11] | `setup_logging` / `set_log_level` / `get_log_level` / `disable_logging` | `setup_logging()` / `set_log_level(level: str)` / `get_log_level() -> str` / `disable_logging()` | route the Rust core's logs (bridge into `structlog`) |

[ENTRYPOINT_SCOPE]: async mirrors
- rail: pdf

`pdf_oxide` ships native async wrappers backed by an owned single-worker thread pool (the underlying `PdfDocument` is `Send + Sync`), so the event loop is never blocked by a render/extract/sign. Every `PdfDocument` op has an `AsyncPdfDocument` coroutine twin; `AsyncPdf`/`AsyncOfficeConverter` mirror the creation/convert factories.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `AsyncPdfDocument.open` / `from_bytes` / `close` | `await AsyncPdfDocument.open(path, password=None)` / `await AsyncPdfDocument.from_bytes(data, password=None)` / `await doc.close()` | async open + explicit pool teardown |
| [02] | `AsyncPdfDocument.<op>` | every `PdfDocument` op as a coroutine (`await doc.extract_words(...)`, `await doc.render_page(...)`, `await doc.apply_all_redactions()`, …) | non-blocking extract/render/redact/sanitize/save off the loop |
| [03] | `AsyncPdf.from_*` / `save` / `to_bytes` | `await AsyncPdf.from_markdown(...)` / `await pdf.save(path)` / `await pdf.to_bytes()` (+ all `Pdf.from_*` mirrored) | async PDF creation |
| [04] | `AsyncOfficeConverter.from_docx` / `from_pptx` / `from_xlsx` / `convert` | `await AsyncOfficeConverter.from_docx(path) -> AsyncPdf` (+ `_bytes` + `convert`) | async Office → PDF |

## [04]-[INTEGRATION_LAW]

[PDF_NATIVE]:
- import: `import pdf_oxide` at boundary scope only; the Rust core (`pdf_oxide.pdf_oxide.abi3.so`) loads on import — no Python-level deps, so it never drags Pillow/cryptography/lxml into the closure.
- import-surface reality: the top-level `pdf_oxide` package re-exports the roots, factories, value objects, and the signing/crypto/barcode/split/logging module functions (its `__all__`). The return-type classes (`PdfPage` from `page(index)`, `Page` from iterating `pages`, `TextChar`/`TextWord`/`TextLine`/`TextSpan`/`FormField`/`PdfPageRegion`/`PdfText`/`PdfImage`/`PdfAnnotation`/`PdfElement`/`Timestamp`) and the stream-safety + OCR-model functions (`set_max_ops_per_stream`/`set_preserve_unmapped_glyphs`/`prefetch_models`/`model_manifest`/`prefetch_available`) live on the inner `pdf_oxide.pdf_oxide` extension module and are not top-level re-exported — reach the return types through their owning method's return value (never `from pdf_oxide import Page`), and the inner functions as `pdf_oxide.pdf_oxide.<fn>`. The shipped `.pyi` declares some of these at package scope and exposes a `Pdf.create()`/`Page.words` shape the runtime does not — trust reflection: `page(index)` yields the edit-surface `PdfPage` (`add_text`/`set_text`/`add_highlight`, no `.words`), while the extract-surface `Page` (`.words`/`.markdown`/`.render`) is what `pages` iteration yields.
- license axis: this is the rail-defining fact. `pdf_oxide` is `MIT OR Apache-2.0`; `pymupdf` is AGPL-3.0. Every concern `pymupdf` owns AND `pdf_oxide` covers — render (`render_page`/`render_pixmap`), layout-aware extract (`extract_words`/`extract_text_lines`/`to_markdown`), search (`search`/`search_page`), redaction (`add_redaction`/`apply_redactions_destructive`), sanitize (`sanitize_document`/`flatten_*`), outline/embedded/widget/annotation recovery — routes through `pdf_oxide` on the commercial-safe (closed/distributed/SaaS) path, reserving the AGPL `pymupdf` arms in `document/lens#LENS`/`document/egress#FINISH` for permissively-licensed or internal pipelines or an Artifex seat. The supersession is per-concern: `pdf_oxide` is the replace-older candidate for the AGPL-bound render/extract/redact/separation rows, never a parallel second engine for the same band on the same license footing.
- extract axis (`document/lens#LENS`): `extract_words(profile=ExtractionProfile.academic())`/`extract_text_lines`/`extract_chars` carry full geometry → `RunNode` leaves; `extract_spans` → styled `RunNode` spans; `extract_tables(table_settings=)` → `TableNode`; `within(page, bbox)`/`PdfPageRegion` is the region arm; `to_markdown_all`/`to_html_all`/`to_plain_text_all` are the whole-document conversion arms. An `ExtractionProfile` (`form`/`government`/`scanned_ocr`/`academic`/…) is the layout-tuning knob — never a hand-rolled gap-threshold table. The `LensProvider` for the commercial-safe text/table/region rows binds here; the AGPL `MUPDF` rows stay for the permissive lane.
- render axis (`document/emit#DOCUMENT` raster + `document/egress#FINISH`): `render_page(dpi=, format=, jpeg_quality=, excluded_layers=)` emits encoded bytes; `render_pixmap` returns the raw premultiplied-RGBA `RenderedPixmap` the `graphic/raster/io#RASTER` owner wraps (premultiplied per PDF §11 — unpremultiply before handing to a straight-alpha consumer); `flatten_to_images(dpi=)` rasterizes the document. This is the commercial-safe render path beside the AGPL pymupdf/BSD pypdfium2 rows.
- separation axis (prepress, `graphic/color/managed#MANAGED`): `render_separations(page, dpi=)` → `list[SeparationPlate]` (and `render_separation(page, ink_name)`), each plate a grayscale ink-coverage map (value == tint %). `get_page_inks`/`get_page_inks_deep` enumerate the ink (spot/process) names first. This is the categorical-best Python entry into per-ink separation/QC — it composes the CMYK/separations plane (`colour-cxf` CxF3 spot exchange + `colour-science` CMYK math + Pillow `ImageCms` device-link) as the *measured* coverage source, never re-deriving separations by hand.
- redaction axis (`document/egress#FINISH` REDACT/SCRUB): `add_redaction(page, rect)` marks, `apply_all_redactions()`/`apply_page_redactions(page)` burns, `apply_redactions_destructive(scrub_metadata=, remove_javascript=, remove_embedded_files=)` is the irreversible scrub. `remove_headers`/`remove_footers`/`remove_artifacts(threshold=)` repeat-detect-and-strip running content; `erase_region(s)` erases by rect. The commercial-safe `Finisher` for the REDACT/SCRUB/SANITIZE steps binds here in place of the AGPL pymupdf `scrub`/`apply_redactions` arm.
- create axis (`document/emit#DOCUMENT`): two modalities under one rail — `Pdf.from_markdown`/`from_html`/`from_html_css_with_fonts`/`from_text`/`from_image(s)`/`merge` is the declarative-from-source `Backend` arm; `DocumentBuilder().tagged_pdf_ua1().register_embedded_font(name, EmbeddedFont.from_file(p)).a4_page().font(...).heading(...).table(t).done().build()` is the fluent tagged-PDF/UA arm. `EmbeddedFont` is one-shot (moved into the builder); `FluentPageBuilder` is single-use per page (`done()` seals it). Tables route through buffered `Table`/`Column`/`Align` or the row-streaming `StreamingTable` for large grids; colors/gradients/patterns/`BlendMode`/`ExtGState` are the publication-grade graphics axis. (`Pdf.create()` from the package docstring is not real — use the `from_*` factories or `DocumentBuilder`.)
- conformance axis (`exchange/conformance#CONFORMANCE`, `document/tagged#ACCESS`): `validate_pdf_a(level=)`/`validate_pdf_x(level=)`/`validate_pdf_ua()` are the in-process standards oracle; `convert_to_pdf_a(level=)` upgrades to archival; `has_structure_tree`/`has_text_layer`/`has_xfa` are the tagged-PDF discriminants. These complement the `veraPDF` JRE oracle — `pdf_oxide` is the dependency-free in-process validator, `veraPDF` the authoritative external cross-check.
- signing axis (`exchange/conformance#CONFORMANCE`): `sign_pdf_bytes_pades(pdf_data, cert, level=PadesLevel.B_LTA, tsa_url=, revocation=RevocationMaterial(certs=, crls=, ocsps=))` is the byte-level PAdES signer with TSA + DSS/LTV; `Certificate.load_pkcs12(data, password)` loads the signer; `TsaClient(url).request_timestamp(data)` is the RFC-3161 client; `signatures()`/`Signature.verify_detached(data)` + `dss()` are the audit-read half. This stacks beside the `pyhanko` conformance owner as the dependency-free signer where `pyhanko`'s cryptography stack is unavailable; `PadesLevel` is a frozen 0..3 ABI code, never renumbered. The pluggable provider surface (`crypto_set_policy`/`crypto_use_fips`/`crypto_cbom`) is FIPS/CBOM-grade.
- office axis: `OfficeConverter.from_docx(path)` → `Pdf` (and `from_pptx`/`from_xlsx` + `_bytes`) imports Office in-process (no LibreOffice subprocess); `PdfDocument.to_docx`/`to_xlsx`/`to_pptx` (+ `_bytes`) exports the reverse. The XLSX export composes the `visualization/table#TABLE` rail's tabular output, never a re-authored spreadsheet model.
- async axis: `pdf_oxide` owns its async offload — `AsyncPdfDocument`/`AsyncPdf`/`AsyncOfficeConverter` run every op on a built-in single-worker pool, so a consumer already on the loop awaits them directly. For the *sync* `PdfDocument`/`Pdf` root, the rail crosses `anyio.to_thread.run_sync` under the shared `_OFFLOAD`/`_THREAD_GATE` `CapacityLimiter` (the GIL-releasing Rust render/extract runs off the scheduler) — never inline on the event loop, mirroring how `document/egress#FINISH` and `exchange/conformance#CONFORMANCE` cross their native arms. Do not double-offload the async mirror through another thread.
- search axis: `search(pattern, case_insensitive=, literal=, whole_word=, max_results=)`/`search_page` is the native regex/literal full-text search → hit-region `AnnotationNode(annot=HIGHLIGHT)` leaves in the `SEARCH` `LensOp`; `classify_document`/`classify_page` route the auto-extraction profile.
- split axis (`package` / `composition/imposition#IMPOSE`): `split_by_bookmarks(src_bytes, level=, include_front_matter=)` (with `plan_split_by_bookmarks` dry-run) is the bookmark-driven document split; `extract_page_ranges_to_bytes(ranges)`/`extract_pages_to_bytes(pages)` are the explicit-range split arms feeding plan-set assembly.
- logging axis: `set_log_level("debug")`/`disable_logging()` route the Rust core's diagnostics; bridge them into the `structlog` rail at the boundary rather than letting the core log independently. `set_max_ops_per_stream(limit)`/`set_preserve_unmapped_glyphs(preserve)` are the content-stream-safety + glyph-fidelity boundary controls for untrusted-PDF intake.
- evidence: each operation captures page count, render dpi/format, separation ink names + plate count, extracted word/line/table counts, redaction count, PDF/A-X-UA verdict, PAdES level achieved + TSA time, signature valid/trusted/broken counts, and output byte length as a pdf receipt feeding `document/emit#DOCUMENT` `EmitFact` / `document/lens#LENS` `Introspection` / `exchange/conformance#CONFORMANCE` `ConformanceVerdict`.
- typed-model boundary (universal tier): the `extract_words`/`extract_tables`/`get_form_fields` returns are PyO3 value objects with `bbox` tuples — admit them at the boundary into the `msgspec`/`pydantic` discriminated `DocumentNode`/`RunNode`/`TableNode` models once; never forward the raw PyO3 objects into the interior. `@beartype` the boundary functions so a wrong-shaped `region`/`table_settings`/`ExtractionProfile` is caught before crossing into the Rust core. Wrap a `pdf_oxide` raise (parse/crypto/validate fault) into the runtime `BoundaryFault` rail via the `async_boundary` capsule + an `expression` `Result`, and weave `stamina.retry` over the `TsaClient`/PAdES network seam (transient TSA/OCSP), exactly as `exchange/conformance#CONFORMANCE` retries its TSA seam — never a bare `raise` surviving into domain flow.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pdf_oxide` (`pdf-oxide`)
- License: `MIT OR Apache-2.0` — the commercial-safe categorical-best PDF render/extract/redact/separation/create owner; supersession candidate for the AGPL `pymupdf` arms on the closed/distributed path
- Owns: commercial-safe PDF open-edit-extract-render-redact-sanitize-sign root (`PdfDocument`), reading-order/layout-aware text extraction with `ExtractionProfile` tuning + word/char/line/span geometry, native table extraction, region-restricted re-extraction, render to encoded bytes + raw `RenderedPixmap`, ink `SeparationPlate` prepress rendering, redaction-grade scrub + sanitize + header/footer/artifact strip, page assembly/crop/rotate + embedded-image edit, AcroForm read/fill/export + flatten, native outline/layer/label/annotation/XMP recovery, PDF/A-PDF/X-PDF/UA validate + PDF/A convert, PAdES B-B…B-LTA signing with TSA + DSS/LTV + signature verify, pluggable crypto providers + FIPS + CBOM, declarative (`Pdf.from_*`) and fluent tagged-PDF/UA (`DocumentBuilder`/`FluentPageBuilder`) creation with embedded fonts + gradients/patterns/blend-modes + barcodes, Office round-trip conversion, bookmark-driven split, and native async mirrors
- Accept: layout-aware extract → `document/lens#LENS` `RunNode`/`TableNode`; render/pixmap → `graphic/raster/io#RASTER`; separations → `graphic/color/managed#MANAGED` CMYK/separations plane; create → `document/emit#DOCUMENT` `Backend`; redact/sanitize/flatten/outline → `document/egress#FINISH` `Finisher`; validate + PAdES sign → `exchange/conformance#CONFORMANCE`; Office export → `visualization/table#TABLE` tabular feed; the async mirror where the consumer is on the loop
- Reject: wrapper-renames of `extract_words`/`render_page`/`render_separations`/`apply_redactions_destructive`/`sign_pdf_bytes_pades`/`validate_pdf_a`; a hand-rolled gap-threshold table where `ExtractionProfile` tunes layout; re-clustering chars into words/lines where `extract_words`/`extract_text_lines` carry geometry; re-deriving separations where `render_separations` measures ink coverage; an AGPL `pymupdf` render/extract/redact arm on the commercial-safe path where `pdf_oxide` covers it permissively (the supersession is per-concern, not a parallel second engine on equal license footing); double-offloading the native async mirror through another thread; a parallel barcode/QR generator where `generate_barcode_svg`/`generate_qr_svg` exist; forwarding raw PyO3 value objects into the interior instead of admitting into `msgspec`/`pydantic` models at the boundary; a bare provider `raise` surviving past the `async_boundary` into domain flow; the non-existent `Pdf.create()` from the package docstring (use `from_*`/`DocumentBuilder`); identity minting the runtime owns
