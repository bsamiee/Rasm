# [PY_ARTIFACTS_API_PDF_OXIDE]

`pdf_oxide` supplies a Rust-core, dependency-free PDF surface for the artifacts pdf rail: the `PdfDocument` root for open/extract/render/edit/redact/sanitize/sign-read, the `Pdf`/`DocumentBuilder`/`FluentPageBuilder` creation trio, the `sign_pdf_bytes_pades` + `Certificate`/`TsaClient` PAdES family, and native `Async*` mirrors over an owned worker pool. Its owner composes these into the pdf rail across extract, author/finish, and sign/validate, never re-implementing the Rust render/extract/redaction core or the `ExtractionProfile`, `RenderedPixmap`, and PAdES models the crate owns. License is the rail-defining fact: `MIT OR Apache-2.0` where `pymupdf` is AGPL-3.0, so the commercial-safe closed/distributed/SaaS render-extract-redact-separation path routes here and reserves the AGPL siblings for permissive or internal pipelines.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pdf_oxide`
- package: `pdf-oxide` (dist `pdf_oxide`)
- import: `pdf_oxide`
- owner: `artifacts`
- rail: pdf
- license: `MIT OR Apache-2.0` — permissive, no copyleft; the load-bearing differentiator from AGPL `pymupdf`, routing the closed/distributed/SaaS render-extract-redact-separation path here without source-disclosure obligation
- asset: Rust core in one `cp38-abi3` wheel (`pdf_oxide.pdf_oxide.abi3.so`), forward-compatible across every CPython; the crate carries codecs, crypto providers, OCR, and Office conversion in-process, so no Python runtime dep enters the closure
- entry points: none (library only)
- capability: open/authenticate/edit (`PdfDocument`), layout-aware reading-order extraction with `ExtractionProfile` tuning + word/char/line/span geometry, native table extraction, region-restricted re-extraction, structured-element + tagged-structure recovery, document/page classification, full-text regex search, native outline/form-field/annotation/layer/image harvest, in-process render to PNG/JPEG/raw `RenderedPixmap`, ink `SeparationPlate` prepress plates, redaction marking + destructive scrub + sanitize, header/footer/artifact erase, image reposition/resize, page assembly/select/move/delete/crop/rotate, AcroForm read/fill/export (FDF/XFDF), Markdown/HTML/text/DOCX/XLSX/PPTX conversion both directions, PDF/A + PDF/X + PDF/UA validate + PDF/A convert, PAdES B-B…B-LTA signing with RFC-3161 timestamp + DSS/LTV + verify, pluggable crypto providers with FIPS + CBOM, fluent tagged-PDF/UA authoring with embedded fonts + gradients/patterns/blend-modes + barcode/QR, bookmark-driven split, and native async mirrors

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document roots and creation entries
- rail: pdf

`PdfDocument` is the single open-edit-extract-render root; `Pdf`/`DocumentBuilder` are the two creation modalities (declarative-from-source vs fluent-tagged-chain); the `Async*` trio mirrors them on a built-in single-worker pool. `page(index)` yields the edit-surface `PdfPage`; iterating `pages` yields the extract-surface `Page` — distinct views, never interchanged.

| [INDEX] | [SYMBOL]                                   | [PACKAGE_ROLE]    | [CAPABILITY]                                                           |
| :-----: | :----------------------------------------- | :---------------- | :--------------------------------------------------------------------- |
|  [01]   | `PdfDocument`                              | document root     | open-edit root; extract/render/redact/sanitize/validate/sign-read/save |
|  [02]   | `Pdf`                                      | creation root     | build from text/HTML/Markdown/image(s)/merge via `from_*`              |
|  [03]   | `DocumentBuilder`                          | fluent builder    | tagged-PDF/UA author; font register, per-page open, `build`            |
|  [04]   | `FluentPageBuilder`                        | page builder      | buffered page-op chain; single-use, sealed by `done()`                 |
|  [05]   | `PdfPage`                                  | page edit view    | per-page edit/annotate view from `page(index)`                         |
|  [06]   | `Page`                                     | page extract view | per-page extract/render view from `pages` iteration                    |
|  [07]   | `AsyncPdfDocument`                         | async root        | full `PdfDocument` coroutine mirror over an owned pool                 |
|  [08]   | `AsyncPdf`                                 | async creation    | `Pdf` mirror; `from_*` coroutines, `save`/`to_bytes`                   |
|  [09]   | `OfficeConverter` / `AsyncOfficeConverter` | office gate       | DOCX/PPTX/XLSX → `Pdf`/`AsyncPdf` (`from_*` + `_bytes` + `convert`)    |

[PUBLIC_TYPE_SCOPE]: extraction value objects
- rail: pdf

Geometry-bearing read models the `PdfDocument.extract_*` / `Page` accessors return; every `bbox` is a `(x0, y0, x1, y1)` float tuple in PDF user space. These feed the `document/lens#LENS` `RunNode`/`FigureNode`/`TableNode` lowering as the layout-aware extract source, never a hand char-cluster.

| [INDEX] | [SYMBOL]                                 | [PACKAGE_ROLE]  | [CAPABILITY]                                                          |
| :-----: | :--------------------------------------- | :-------------- | :-------------------------------------------------------------------- |
|  [01]   | `TextChar`                               | glyph           | one char with full font/color/origin/advance/mcid metrics             |
|  [02]   | `TextWord`                               | word            | word text + geometry + font weight/style + `chars`                    |
|  [03]   | `TextLine`                               | line            | line text + geometry + `words`/`chars`                                |
|  [04]   | `TextSpan`                               | styled span     | styled-run unit feeding `RunNode`; font/color/style/`char_widths`     |
|  [05]   | `FormField`                              | AcroForm field  | name/type/value/bounds/flags/length/readonly/required recovery        |
|  [06]   | `PdfPageRegion`                          | region view     | bbox-restricted re-extraction of text/words/lines/tables/images/paths |
|  [07]   | `PdfElement`                             | structured node | text/image/path/table/structure discriminants + `as_*` + `bbox`       |
|  [08]   | `PdfText` / `PdfImage` / `PdfAnnotation` | element leaves  | recovered text run / placed image / annotation leaf                   |
|  [09]   | `LayoutParams`                           | layout metrics  | computed per-page column count + gap/spacing/font/char medians        |

- `FormField`: `field_type` is a lowercase token; a raw `/Btn` (checkbox included) reads `"button"` with a `bool` `value`, and `tooltip`/`max_length`/`flags` reflect `None` when unset.

[PUBLIC_TYPE_SCOPE]: render buffers
- rail: pdf

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE] | [CAPABILITY]                                                  |
| :-----: | :---------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `RenderedPixmap`  | raster buffer  | raw pixel buffer the raster owner wraps without re-decode     |
|  [02]   | `SeparationPlate` | ink plate      | one grayscale ink plate; prepress ink-coverage + ML/QC source |

- `RenderedPixmap`: `data` is row-major, top-left origin, 4-byte premultiplied RGBA per pixel (PDF §11), `len == width*height*4`; unpremultiply before a straight-alpha consumer.
- `SeparationPlate`: `data` runs 0–255 where pixel intensity == ink coverage percent, keyed by `ink_name`.

[PUBLIC_TYPE_SCOPE]: authoring value objects — color, graphics, fonts, tables
- rail: pdf

Construct-then-pass value objects fed into the `Pdf`/`DocumentBuilder`/`FluentPageBuilder` create chain; colors are normalized 0..1 float RGB, and the gradient/pattern/blend family is the publication-grade graphics axis.

| [INDEX] | [SYMBOL]                                                            | [PACKAGE_ROLE]  | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------------ | :-------------- | :------------------------------------------- |
|  [01]   | `Color`                                                             | rgb value       | 0..1 float RGB; `from_hex` + named presets   |
|  [02]   | `LinearGradient`                                                    | linear fill     | start/end stops; `horizontal`/`vertical`     |
|  [03]   | `RadialGradient`                                                    | radial fill     | inner/outer-circle stops; `centered` helper  |
|  [04]   | `PatternPresets`                                                    | tiling patterns | tiling-pattern factories                     |
|  [05]   | `BlendMode`                                                         | blend enum      | PDF blend-mode vocabulary                    |
|  [06]   | `ExtGState`                                                         | graphics state  | alpha + `blend_mode` + `semi_transparent`    |
|  [07]   | `LineCap` / `LineJoin`                                              | stroke style    | cap and join vocabularies                    |
|  [08]   | `EmbeddedFont`                                                      | font handle     | one-shot TTF/OTF handle from file/bytes      |
|  [09]   | `Table` / `Column` / `Align`                                        | buffered table  | buffered grid for `FluentPageBuilder.table`  |
|  [10]   | `StreamingTable`                                                    | streaming table | row-by-row push/flush/finish for large grids |
|  [11]   | `PageTemplate` / `Header` / `Footer` / `Artifact` / `ArtifactStyle` | running content | running header/footer + artifact templates   |
|  [12]   | `ExtractionProfile`                                                 | extract tuning  | pre-tuned layout profile + tuning props      |
|  [13]   | `OcrEngine` / `OcrConfig`                                           | ocr config      | OCR engine/config for `extract_text_ocr`     |

- [05]-[BLEND_MODE]: `NORMAL` `MULTIPLY` `SCREEN` `OVERLAY` `DARKEN` `LIGHTEN` `COLOR_DODGE` `COLOR_BURN` `HARD_LIGHT` `SOFT_LIGHT` `DIFFERENCE` `EXCLUSION`.
- [07]-[LINE_CAP]: `BUTT` `ROUND` `SQUARE`; [LINE_JOIN]: `MITER` `ROUND` `BEVEL`.
- [12]-[EXTRACTION_PROFILE]: `conservative` `aggressive` `balanced` `academic` `policy` `form` `government` `scanned_ocr` `adaptive`; `available()` lists them.
- `EmbeddedFont`: moved into the builder by `register_embedded_font`, so registering the same handle twice raises.

[PUBLIC_TYPE_SCOPE]: signing and crypto value objects
- rail: pdf

`Certificate`/`TsaClient`/`Signature` form the PAdES + RFC-3161 timestamp family feeding `exchange/conformance#CONFORMANCE`; the in-process signer stacks beside `pyhanko` where its cryptography stack is barred.

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]  | [CAPABILITY]                                               |
| :-----: | :------------------- | :-------------- | :--------------------------------------------------------- |
|  [01]   | `Certificate`        | x509 + signer   | load DER/PEM/PKCS12; subject/issuer/serial/validity/valid  |
|  [02]   | `Signature`          | existing sig    | one embedded `/Sig`; signer/reason/time/coverage + verify  |
|  [03]   | `PadesLevel`         | level enum      | B-B/B-T/B-LT/B-LTA level                                   |
|  [04]   | `TsaClient`          | RFC-3161 client | timestamp request over a TSA URL → `Timestamp`             |
|  [05]   | `Timestamp`          | timestamp token | parse + time/serial/policy/tsa/imprint + verify            |
|  [06]   | `RevocationMaterial` | LTV material    | offline B-LT DER certs/CRLs/OCSP responses                 |
|  [07]   | `Dss`                | security store  | parsed `/DSS`: certs/CRLs/OCSPs + per-signature `vri` keys |

- `PadesLevel`: `int` maps to a frozen 0..3 ABI code (`B_B`=0/`B_T`=1/`B_LT`=2/`B_LTA`=3) shared with the C ABI, never renumbered.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open, authenticate, save
- rail: pdf
- call: `PdfDocument(path, password=None)`; `PdfDocument.from_bytes(data, password=None)`; `authenticate(password)`; `page(index) -> PdfPage`; `pages -> PdfDocumentIter` yielding `Page`; `page_count`; `save(path, compress=True, garbage_collect=True, linearize=False)`; `to_bytes(...) -> bytes`; `save_encrypted(path, user_password, owner_password=None, allow_print=True, allow_copy=True, allow_modify=True, allow_annotate=True)`; `to_bytes_encrypted(...) -> bytes`; `permissions()`; `version() -> tuple[int, int]`; `save_page(page) -> bytes`.

`PdfDocument` is a context manager (`__enter__`/`__exit__`), so `with PdfDocument.from_bytes(data) as doc:` closes the Rust handle deterministically; `version()` returns the `(major, minor)` pair.

| [INDEX] | [SURFACE]                                           | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `PdfDocument`                                       | open an existing PDF, decrypting if `password`   |
|  [02]   | `PdfDocument.from_bytes`                            | open from an in-memory byte stream               |
|  [03]   | `PdfDocument.authenticate`                          | unlock an encrypted document post-open           |
|  [04]   | `PdfDocument.page` / `pages` / `page_count`         | edit view, extract-view iterator, page count     |
|  [05]   | `PdfDocument.save`                                  | save with compaction / GC / linearization        |
|  [06]   | `PdfDocument.to_bytes`                              | serialize the edited document to bytes           |
|  [07]   | `PdfDocument.save_encrypted`                        | save with RC4/AES encryption + permission flags  |
|  [08]   | `PdfDocument.to_bytes_encrypted`                    | encrypted serialize to bytes                     |
|  [09]   | `PdfDocument.permissions` / `version` / `save_page` | permission flags, PDF version, single-page bytes |

[ENTRYPOINT_SCOPE]: layout-aware text extraction and conversion
- rail: pdf
- call: `extract_text(page, region=None, exclude_layers=None, exclude_inks=None, extract_tables=True) -> str`; `extract_words(page, *, include_artifacts=True, region=None, word_gap_threshold=None, profile=None) -> list[TextWord]`; `extract_text_lines(page, *, include_artifacts=True, region=None, word_gap_threshold=None, line_gap_threshold=None, profile=None) -> list[TextLine]`; `extract_chars(page, region=None, exclude_layers=None, exclude_inks=None) -> list[TextChar]`; `extract_spans(page, region=None, reading_order=None) -> list[TextSpan]`; `extract_tables(page, region=None, table_settings=None)`; `extract_structured(page) -> str` (+ `extract_text_auto`/`extract_page_auto(options_json=)`); `extract_lines`/`extract_rects`/`extract_paths(page, region=)`; `extract_text_ocr(page, engine=None) -> str`; `to_markdown(page, preserve_layout=False, detect_headings=True, include_images=False, image_output_dir=None, embed_images=True, include_form_fields=True) -> str` (+ `to_markdown_all`); `to_html(...)`/`to_html_all`; `to_plain_text(...)`/`to_plain_text_all`; `search(pattern, case_insensitive=False, literal=False, whole_word=False, max_results=0)`/`search_page(page, pattern, ...)`; `classify_document() -> str`/`classify_page(page) -> str`; `page_layout_params(page) -> LayoutParams`.

`pdf_oxide` extraction is the `document/lens#LENS` engine for the commercial-safe path; an `ExtractionProfile` tunes the heuristics per document class, and `classify_document`/`classify_page` return JSON (document keys `pages`/`pages_needing_ocr`/`summary`; page keys `page`/`kind`/`confidence`/`reason`/`signals`).

| [INDEX] | [SURFACE]                                                       | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `PdfDocument.extract_text`                                      | reading-order page text, layer/ink-excludable    |
|  [02]   | `PdfDocument.extract_words`                                     | word-geometry extraction → `RunNode` leaves      |
|  [03]   | `PdfDocument.extract_text_lines`                                | line-grouped geometry extraction                 |
|  [04]   | `PdfDocument.extract_chars`                                     | per-glyph extraction with full metrics           |
|  [05]   | `PdfDocument.extract_spans`                                     | styled-run extraction → `RunNode` spans          |
|  [06]   | `PdfDocument.extract_tables`                                    | native ruled+text grid → `TableNode`             |
|  [07]   | `PdfDocument.extract_structured`                                | structured/auto-profile extraction (JSON)        |
|  [08]   | `PdfDocument.extract_lines` / `extract_rects` / `extract_paths` | vector line/rect/path geometry harvest           |
|  [09]   | `PdfDocument.extract_text_ocr`                                  | OCR a scanned page in-process                    |
|  [10]   | `PdfDocument.to_markdown` / `to_markdown_all`                   | page/whole-doc Markdown conversion               |
|  [11]   | `PdfDocument.to_html` / `to_html_all`                           | page/whole-doc HTML conversion                   |
|  [12]   | `PdfDocument.to_plain_text` / `to_plain_text_all`               | page/whole-doc plain-text conversion             |
|  [13]   | `PdfDocument.search` / `search_page`                            | full-document / per-page regex or literal search |
|  [14]   | `PdfDocument.classify_document` / `classify_page`               | document/page type classification                |
|  [15]   | `PdfDocument.page_layout_params`                                | computed adaptive layout metrics for a page      |

[ENTRYPOINT_SCOPE]: render to raster and ink separations
- rail: pdf
- call: `render_page(page, dpi=None, format=None, background=None, transparent=False, render_annotations=None, jpeg_quality=None, excluded_layers=None) -> bytes`; `render_page_fit(page, width, height, *, format=None, background=None, transparent=False, render_annotations=None, jpeg_quality=None, excluded_layers=None) -> bytes`; `render_pixmap(page, dpi=None) -> RenderedPixmap`; `render_separation(page, ink_name, dpi=None) -> SeparationPlate`; `render_separations(page, dpi=None) -> list[SeparationPlate]`; `flatten_to_images(dpi=150)`.

`render_separation(s)` is the prepress differentiator on this commercial-safe render surface, emitting per-ink grayscale coverage plates.

| [INDEX] | [SURFACE]                        | [CAPABILITY]                                       |
| :-----: | :------------------------------- | :------------------------------------------------- |
|  [01]   | `PdfDocument.render_page`        | render a page to encoded PNG/JPEG bytes at DPI     |
|  [02]   | `PdfDocument.render_page_fit`    | render fit to a target pixel box                   |
|  [03]   | `PdfDocument.render_pixmap`      | raw premultiplied RGBA8888 buffer (no encode)      |
|  [04]   | `PdfDocument.render_separation`  | one named-ink coverage plate (prepress/QC)         |
|  [05]   | `PdfDocument.render_separations` | every ink's coverage plate at once                 |
|  [06]   | `PdfDocument.flatten_to_images`  | rasterize all pages, flattening interactive layers |

[ENTRYPOINT_SCOPE]: redaction, sanitize, and content erase
- rail: pdf
- call: `add_redaction(page, rect, fill=None)`; `apply_page_redactions(page)`/`apply_all_redactions()`; `apply_redactions_destructive(scrub_metadata=True, remove_javascript=True, remove_embedded_files=True, fill=...)`; `sanitize_document(scrub_metadata=True, remove_javascript=True, remove_embedded_files=True)`; `erase_region(page, llx, lly, urx, ury)`/`erase_regions(page, rects)`; `erase_header(page)`/`erase_footer(page)`/`erase_artifacts(page)`; `remove_headers(threshold=0.8) -> int`/`remove_footers(...)`/`remove_artifacts(...)`; `redaction_count(page)`/`is_page_marked_for_redaction(page)`/`clear_erase_regions(page)`/`unmark_page_for_redaction(page)`.

`apply_redactions_destructive` is the irreversible scrub of this commercial-safe removal surface for the `document/egress#FINISH` REDACT/SCRUB steps; the `remove_*` family strips repeat-detected running content by threshold.

| [INDEX] | [SURFACE]                                                                              | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `PdfDocument.add_redaction`                                                            | mark a region for redaction                     |
|  [02]   | `PdfDocument.apply_page_redactions` / `apply_all_redactions`                           | burn marked redactions (page / whole doc)       |
|  [03]   | `PdfDocument.apply_redactions_destructive`                                             | irreversible redaction + metadata/JS/file scrub |
|  [04]   | `PdfDocument.sanitize_document`                                                        | sanitize hidden/JS/attachment content           |
|  [05]   | `PdfDocument.erase_region` / `erase_regions`                                           | erase content within rect(s)                    |
|  [06]   | `PdfDocument.erase_header` / `erase_footer` / `erase_artifacts`                        | erase running header/footer/artifact content    |
|  [07]   | `PdfDocument.remove_headers` / `remove_footers` / `remove_artifacts`                   | repeat-detect + strip headers/footers/artifacts |
|  [08]   | `PdfDocument.redaction_count` / `is_page_marked_for_redaction` / `clear_erase_regions` | redaction-state queries + erase reset           |

[ENTRYPOINT_SCOPE]: page assembly, geometry, and image edit
- rail: pdf
- call: `merge_from(source)`; `select_pages(pages)`/`move_page(from_index, to_index)`/`delete_page(index)`; `extract_pages(pages, output)`/`extract_pages_to_bytes(pages) -> bytes`/`extract_page_ranges_to_bytes(ranges)`; `rotate_page(page, degrees)`/`rotate_all_pages(degrees)`/`set_page_rotation(page, degrees)`/`page_rotation(page)`; `crop_margins(left, right, top, bottom)`/`set_page_crop_box(page, llx, lly, urx, ury)`/`set_page_media_box(...)`/`page_crop_box`/`page_media_box`; `page_images(page)`/`extract_images(page, region=None)`/`extract_image_bytes(page)`; `reposition_image(page, image_name, x, y)`/`resize_image(page, image_name, width, height)`/`set_image_bounds(page, image_name, x, y, width, height)`/`has_image_modifications`/`clear_image_modifications`; `within(page, bbox) -> PdfPageRegion`.

This structural-edit surface feeds `document/egress#FINISH` composition.

| [INDEX] | [SURFACE]                                                                               | [CAPABILITY]                                  |
| :-----: | :-------------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `PdfDocument.merge_from`                                                                | append another document's pages               |
|  [02]   | `PdfDocument.select_pages` / `move_page` / `delete_page`                                | subset/reorder/drop pages                     |
|  [03]   | `PdfDocument.extract_pages` / `extract_pages_to_bytes` / `extract_page_ranges_to_bytes` | split selected pages/ranges to file or bytes  |
|  [04]   | `PdfDocument.rotate_page` / `rotate_all_pages` / `set_page_rotation`                    | rotate one/all pages                          |
|  [05]   | `PdfDocument.crop_margins` / `set_page_crop_box` / `set_page_media_box`                 | crop margins / set crop+media boxes           |
|  [06]   | `PdfDocument.page_images` / `extract_images` / `extract_image_bytes`                    | enumerate / harvest placed images + raw bytes |
|  [07]   | `PdfDocument.reposition_image` / `resize_image` / `set_image_bounds`                    | move/resize an embedded image in place        |
|  [08]   | `PdfDocument.within`                                                                    | bbox-restricted region view for re-extraction |

[ENTRYPOINT_SCOPE]: forms, annotations, outline, layers, metadata
- rail: pdf
- call: `get_form_fields() -> list[FormField]`; `get_form_field_value(name)`/`set_form_field_value(name, value)`; `export_form_data(path, format='fdf')`; `flatten_forms()`/`flatten_forms_on_page(page)`; `get_annotations(page)`; `flatten_all_annotations()`/`flatten_page_annotations(page)`; `get_outline()`; `get_layers() -> list[str]`/`get_page_inks(page)`/`get_page_inks_deep(page)`; `page_labels()`; `set_title(title)`/`set_author(author)`/`set_subject(subject)`/`set_keywords(keywords)`; `xmp_metadata()`; `embed_file(name, data)`; `edit_header(page)`/`edit_footer(page)`; `PdfPage`: `add_text(text, x, y, font_size=12.0)`/`add_highlight(x, y, width, height, color)`/`add_link(x, y, width, height, url)`/`add_note(x, y, text)`/`set_text(text_id, new_text)`/`find_text_containing(needle)`/`find_images()`/`get_element(id)`/`children()`/`annotations()`/`remove_annotation(index)`/`remove_element(id)`.

This recovery + finishing surface feeds `document/lens#LENS` (read) and `document/egress#FINISH` (flatten/outline/metadata).

| [INDEX] | [SURFACE]                                                               | [CAPABILITY]                                               |
| :-----: | :---------------------------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `PdfDocument.get_form_fields`                                           | recover AcroForm fields (type/value/bounds/flags)          |
|  [02]   | `PdfDocument.get_form_field_value` / `set_form_field_value`             | read / fill a single form field                            |
|  [03]   | `PdfDocument.export_form_data`                                          | export filled form data as FDF/XFDF                        |
|  [04]   | `PdfDocument.flatten_forms` / `flatten_forms_on_page`                   | flatten interactive form fields into static content        |
|  [05]   | `PdfDocument.get_annotations`                                           | recover page annotations (subtype/rect/contents/color)     |
|  [06]   | `PdfDocument.flatten_all_annotations` / `flatten_page_annotations`      | flatten annotations into content                           |
|  [07]   | `PdfDocument.get_outline`                                               | recover the native bookmark outline → `StructureNode`      |
|  [08]   | `PdfDocument.get_layers` / `get_page_inks` / `get_page_inks_deep`       | enumerate OCG layers / page ink (separation) names         |
|  [09]   | `PdfDocument.page_labels`                                               | recover the page-label numbering tree                      |
|  [10]   | `PdfDocument.set_title` / `set_author` / `set_subject` / `set_keywords` | edit document-info metadata fields                         |
|  [11]   | `PdfDocument.xmp_metadata`                                              | recover the XMP metadata packet                            |
|  [12]   | `PdfDocument.embed_file`                                                | attach an embedded file                                    |
|  [13]   | `PdfDocument.edit_header` / `edit_footer`                               | author/replace running header/footer                       |
|  [14]   | `PdfPage` (from `page(index)`)                                          | per-page edit: text/annotations/links; retext; find/remove |

[ENTRYPOINT_SCOPE]: structure, conformance validation, and PDF/A conversion
- rail: pdf
- call: `has_structure_tree() -> bool`/`has_text_layer(page) -> bool`/`has_xfa() -> bool`; `structured_warnings()`/`flatten_warnings()`; `validate_pdf_a(level='1b') -> ConformanceReport`; `validate_pdf_ua() -> AccessibilityReport`; `validate_pdf_x(level='1a_2001') -> ConformanceReport`; `convert_to_pdf_a(level='2b') -> ConversionReport`.

Tagged-structure recovery + the standards-validation triple, the `exchange/conformance#CONFORMANCE` and `document/tagged#ACCESS` engines; the reports are `TypedDict`s (`valid`/`level`/`errors`/`warnings`; conversion `success`/`actions`/`errors`).

| [INDEX] | [SURFACE]                                                       | [CAPABILITY]                                         |
| :-----: | :-------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `PdfDocument.has_structure_tree` / `has_text_layer` / `has_xfa` | tagged-PDF / text-layer / XFA presence discriminants |
|  [02]   | `PdfDocument.structured_warnings` / `flatten_warnings`          | structure/flatten diagnostic warnings                |
|  [03]   | `PdfDocument.validate_pdf_a`                                    | PDF/A conformance validation                         |
|  [04]   | `PdfDocument.validate_pdf_ua`                                   | PDF/UA accessibility validation                      |
|  [05]   | `PdfDocument.validate_pdf_x`                                    | PDF/X print-production validation                    |
|  [06]   | `PdfDocument.convert_to_pdf_a`                                  | upgrade the document to PDF/A archival conformance   |

[ENTRYPOINT_SCOPE]: existing-signature read and verify
- rail: pdf
- call: `signatures() -> list[Signature]` (then `Signature.verify()`/`verify_detached(pdf_data)`); `signature_count() -> int`; `dss() -> Dss | None`.

This read half feeds `exchange/conformance#CONFORMANCE` audit.

| [INDEX] | [SURFACE]                     | [CAPABILITY]                                    |
| :-----: | :---------------------------- | :---------------------------------------------- |
|  [01]   | `PdfDocument.signatures`      | recover every embedded `/Sig` for verify        |
|  [02]   | `PdfDocument.signature_count` | embedded-signature count                        |
|  [03]   | `PdfDocument.dss`             | recover the `/DSS` store (certs/CRLs/OCSPs/VRI) |

[ENTRYPOINT_SCOPE]: Office round-trip conversion
- rail: pdf
- call: `OfficeConverter.from_docx(path) -> Pdf`/`from_pptx(path)`/`from_xlsx(path)` (+ `_bytes(data)` + `convert(path)`); `PdfDocument.to_docx(path)`/`to_xlsx(path)`/`to_pptx(path)` (+ `_bytes() -> bytes`).

Both directions of Office ↔ PDF in-process, no LibreOffice subprocess.

| [INDEX] | [SURFACE]                                               | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------ | :-------------------------------- |
|  [01]   | `OfficeConverter.from_docx` / `from_pptx` / `from_xlsx` | DOCX/PPTX/XLSX → `Pdf` in-process |
|  [02]   | `PdfDocument.to_docx` / `to_xlsx` / `to_pptx`           | PDF → DOCX/XLSX/PPTX export       |

[ENTRYPOINT_SCOPE]: PDF creation — declarative `Pdf` factories
- rail: pdf
- call: `Pdf.from_markdown(content, title=None, author=None) -> Pdf`; `from_html(content, title=None, author=None)`; `from_text(content, title=None, author=None)`; `from_html_css(html, css, font_bytes) -> Pdf`/`from_html_css_with_fonts(html, css, fonts)`; `from_markdown_with_template(content, template, title=None, author=None)`; `from_image(path) -> Pdf`/`from_images(paths)`/`from_image_bytes(data)`; `merge(paths) -> Pdf`; `from_bytes(data) -> Pdf`/`save(path)`/`to_bytes() -> bytes` (+ `len(pdf)` page count).

One content source → a `Pdf`; `from_html_css_with_fonts` carries embedded-font bytes and `from_markdown_with_template` applies a running-header/footer `PageTemplate`.

| [INDEX] | [SURFACE]                                             | [CAPABILITY]                                        |
| :-----: | :---------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `Pdf.from_markdown`                                   | author a PDF from Markdown                          |
|  [02]   | `Pdf.from_html`                                       | author a PDF from HTML                              |
|  [03]   | `Pdf.from_text`                                       | author a PDF from plain text                        |
|  [04]   | `Pdf.from_html_css` / `from_html_css_with_fonts`      | author from HTML+CSS with one / many embedded fonts |
|  [05]   | `Pdf.from_markdown_with_template`                     | Markdown author with running header/footer template |
|  [06]   | `Pdf.from_image` / `from_images` / `from_image_bytes` | wrap one/many raster images into a PDF              |
|  [07]   | `Pdf.merge`                                           | concatenate existing PDFs by path                   |
|  [08]   | `Pdf.from_bytes` / `save` / `to_bytes`                | round-trip bytes, emit to path/bytes                |

[ENTRYPOINT_SCOPE]: PDF creation — fluent tagged `DocumentBuilder` / `FluentPageBuilder`
- rail: pdf
- call: `DocumentBuilder()` → `.title`/`.author`/`.subject`/`.keywords`/`.creator`/`.language`/`.on_open(script)`; `register_embedded_font(name, font) -> DocumentBuilder`; `tagged_pdf_ua1() -> DocumentBuilder`/`role_map(custom, standard)`; `a4_page() -> FluentPageBuilder`/`letter_page()`/`page(width, height)`; `build() -> bytes`/`save(path)`/`save_encrypted(path, user_password, owner_password)`/`to_bytes_encrypted(...)`; `FluentPageBuilder` text `font`/`at`/`text`/`paragraph`/`heading`/`columns`/`text_in_rect`/`inline*`/`newline`/`space`/`measure`/`remaining_space`; graphics `line`/`rect`/`filled_rect`/`stroke_line`/`stroke_rect`/`stroke_line_dashed`/`horizontal_rule`; content `image_artifact`/`image_with_alt`/`table`/`streaming_table(columns, repeat_header=False, mode='fixed', sample_rows=50, min_col_width_pt=20.0, max_col_width_pt=400.0, max_rowspan=1, batch_size=256)`/`footnote`; annotations `highlight`/`underline`/`strikeout`/`squiggly`/`sticky_note`/`sticky_note_at`/`stamp`/`freetext`/`watermark`/`watermark_confidential`/`watermark_draft`; forms `text_field`/`checkbox`/`radio_group`/`combo_box`/`push_button`/`signature_field`; field scripts `field_validate`/`field_format`/`field_calculate`/`field_keystroke`/`link_javascript`/`on_open`/`on_close`; links `link_url`/`link_page`/`link_named`/`barcode_1d`/`barcode_qr`; `done() -> DocumentBuilder`/`new_page_same_size() -> FluentPageBuilder`.

Each builder method mutates and returns `self` in this fluent tagged-PDF/UA modality; `tagged_pdf_ua1()` makes the output PDF/UA-1, and every page builder is single-use, sealed by `done()`.

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `DocumentBuilder`                                                          | construct + set document metadata (fluent)                |
|  [02]   | `DocumentBuilder.register_embedded_font`                                   | register a TTF/OTF face for use by `font(name, size)`     |
|  [03]   | `DocumentBuilder.tagged_pdf_ua1` / `role_map`                              | emit tagged PDF/UA-1 + custom→standard role map           |
|  [04]   | `DocumentBuilder.a4_page` / `letter_page` / `page`                         | open a page builder (A4 / Letter / custom size)           |
|  [05]   | `DocumentBuilder.build` / `save` / `save_encrypted` / `to_bytes_encrypted` | finalize to bytes / file / encrypted output               |
|  [06]   | `FluentPageBuilder` text                                                   | positioned + flowing text, headings, multi-column, inline |
|  [07]   | `FluentPageBuilder` graphics                                               | vector primitives (stroked/filled/dashed)                 |
|  [08]   | `FluentPageBuilder` content                                                | tagged image, buffered/streaming table, footnote          |
|  [09]   | `FluentPageBuilder` annotations                                            | annotation + watermark authoring                          |
|  [10]   | `FluentPageBuilder` forms                                                  | AcroForm field authoring                                  |
|  [11]   | `FluentPageBuilder` field scripts                                          | JavaScript field-action authoring                         |
|  [12]   | `FluentPageBuilder` links + barcodes                                       | hyperlinks + in-page 1D/QR barcodes                       |
|  [13]   | `FluentPageBuilder.done` / `new_page_same_size`                            | seal the page / open another same-size page               |

[ENTRYPOINT_SCOPE]: module functions — signing, crypto policy, barcodes, split, logging
- rail: pdf
- call: `sign_pdf_bytes(pdf_data, cert, reason=None, location=None) -> bytes`; `sign_pdf_bytes_pades(pdf_data, cert, level, tsa_url=None, reason=None, location=None, revocation=None) -> bytes`; `has_document_timestamp(pdf_data) -> bool`; `crypto_active_provider() -> str`/`crypto_available_providers() -> list[str]`; `crypto_use_fips()`/`crypto_policy() -> str`/`crypto_set_policy(spec)`; `crypto_inventory() -> list[str]`/`crypto_cbom() -> str`; `generate_barcode_svg(barcode_type, data) -> str`/`generate_qr_svg(data, error_correction, size) -> str`; `split_by_bookmarks(src_bytes, title_prefix=None, ignore_case=False, level=1, include_front_matter=True) -> list` (+ `plan_split_by_bookmarks` dry-run); `pdf_oxide.pdf_oxide.prefetch_models(languages) -> str`/`model_manifest() -> str`/`prefetch_available() -> bool`; `pdf_oxide.pdf_oxide.set_max_ops_per_stream(limit) -> int | None`/`set_preserve_unmapped_glyphs(preserve) -> bool`; `setup_logging()`/`set_log_level(level)`/`get_log_level() -> str`/`disable_logging()`.

Free functions: byte-level PAdES signing with no `PdfDocument` round-trip, the pluggable crypto-provider + FIPS + CBOM surface, standalone barcode/QR SVG, bookmark-driven split, OCR-model prefetch, and the stream-safety + logging controls; the `prefetch_*`/`set_max_ops_per_stream`/`set_preserve_unmapped_glyphs` functions live on the inner `pdf_oxide.pdf_oxide` module, not top-level re-exported.

| [INDEX] | [SURFACE]                                                                       | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------------------------ | :--------------------------------------------------- |
|  [01]   | `sign_pdf_bytes`                                                                | apply a basic (non-PAdES) signature to PDF bytes     |
|  [02]   | `sign_pdf_bytes_pades`                                                          | PAdES B-B…B-LTA sign with TSA + DSS/LTV material     |
|  [03]   | `has_document_timestamp`                                                        | detect a `/DocTimeStamp` proof-of-existence          |
|  [04]   | `crypto_active_provider` / `crypto_available_providers`                         | inspect the active / available crypto backends       |
|  [05]   | `crypto_use_fips` / `crypto_policy` / `crypto_set_policy`                       | enable FIPS mode / read / set the crypto policy spec |
|  [06]   | `crypto_inventory` / `crypto_cbom`                                              | cryptographic inventory + CycloneDX crypto BOM       |
|  [07]   | `generate_barcode_svg` / `generate_qr_svg`                                      | standalone 1D-barcode / QR SVG strings (no PDF)      |
|  [08]   | `plan_split_by_bookmarks` / `split_by_bookmarks`                                | split a PDF into parts at bookmark boundaries        |
|  [09]   | `pdf_oxide.pdf_oxide.prefetch_models` / `model_manifest` / `prefetch_available` | pre-download / inspect OCR language models           |
|  [10]   | `pdf_oxide.pdf_oxide.set_max_ops_per_stream` / `set_preserve_unmapped_glyphs`   | content-stream safety cap + unmapped-glyph policy    |
|  [11]   | `setup_logging` / `set_log_level` / `get_log_level` / `disable_logging`         | route the Rust core's logs into `structlog`          |

[ENTRYPOINT_SCOPE]: async mirrors
- rail: pdf
- call: `await AsyncPdfDocument.open(path, password=None)`/`await AsyncPdfDocument.from_bytes(data, password=None)`/`await doc.close()`; `await doc.<op>(...)` (every `PdfDocument` op as a coroutine); `await AsyncPdf.from_*(...)`/`await pdf.save(path)`/`await pdf.to_bytes()`; `await AsyncOfficeConverter.from_docx(path) -> AsyncPdf` (+ `_bytes` + `convert`).

Native async wrappers run every op on an owned single-worker pool (the `PdfDocument` core is `Send + Sync`), so a consumer already on the loop awaits them directly; `close()` tears the pool down explicitly.

| [INDEX] | [SURFACE]                                                                | [CAPABILITY]                                                  |
| :-----: | :----------------------------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `AsyncPdfDocument.open` / `from_bytes` / `close`                         | async open + explicit pool teardown                           |
|  [02]   | `AsyncPdfDocument.<op>`                                                  | non-blocking extract/render/redact/sanitize/save off the loop |
|  [03]   | `AsyncPdf.from_*` / `save` / `to_bytes`                                  | async PDF creation                                            |
|  [04]   | `AsyncOfficeConverter.from_docx` / `from_pptx` / `from_xlsx` / `convert` | async Office → PDF                                            |

## [04]-[IMPLEMENTATION_LAW]

[PDF_NATIVE]:
- import: `import pdf_oxide` at boundary scope only; the Rust core loads on import with no Python-level deps, never dragging Pillow/cryptography/lxml into the closure.
- import-surface reality: the top-level `pdf_oxide` package re-exports the roots, factories, value objects, and signing/crypto/barcode/split/logging functions; the return-type classes (`PdfPage`, `Page`, `TextChar`/`TextWord`/`TextLine`/`TextSpan`/`FormField`/`PdfPageRegion`/`PdfText`/`PdfImage`/`PdfAnnotation`/`PdfElement`/`Timestamp`) and the stream-safety + OCR-model functions live on the inner `pdf_oxide.pdf_oxide` module. Reach a return type through its owning method's return value, never `from pdf_oxide import Page`; reach the inner functions through `pdf_oxide.pdf_oxide.<fn>`. Reflection is authoritative over the shipped `.pyi`: `page(index)` yields the edit-surface `PdfPage` (`add_text`/`set_text`, no `.words`), while `pages` iteration yields the extract-surface `Page` (`.words`/`.markdown`/`.render`).
- license axis: the rail-defining fact. Every concern `pymupdf` owns and `pdf_oxide` covers — render (`render_page`/`render_pixmap`), layout-aware extract (`extract_words`/`extract_text_lines`/`to_markdown`), search, redaction (`add_redaction`/`apply_redactions_destructive`), sanitize, outline/embedded/widget/annotation recovery — routes through `pdf_oxide` on the closed/distributed/SaaS path, reserving the AGPL `pymupdf` arms in `document/lens#LENS`/`document/egress#FINISH` for permissive or internal pipelines. Supersession is per-concern: `pdf_oxide` replaces the AGPL-bound render/extract/redact/separation rows, never a parallel second engine on equal license footing.
- extract axis (`document/lens#LENS`): `extract_words(profile=ExtractionProfile.academic())`/`extract_text_lines`/`extract_chars` carry geometry → `RunNode` leaves; `extract_spans` → styled `RunNode` spans; `extract_tables(table_settings=)` → `TableNode`; `within(page, bbox)`/`PdfPageRegion` is the region arm; the `to_*_all` family is whole-document conversion. An `ExtractionProfile` is the layout-tuning knob, never a hand-rolled gap-threshold table; the commercial-safe `LensProvider` binds here and the AGPL `MUPDF` rows stay for the permissive lane.
- render axis (`document/emit#DOCUMENT` raster + `document/egress#FINISH`): `render_page(dpi=, format=, jpeg_quality=, excluded_layers=)` emits encoded bytes; `render_pixmap` returns the premultiplied-RGBA `RenderedPixmap` the `graphic/raster/io#RASTER` owner wraps (unpremultiply before a straight-alpha consumer); `flatten_to_images(dpi=)` rasterizes the document.
- separation axis (`graphic/color/managed#MANAGED`): `render_separations(page, dpi=)` → `list[SeparationPlate]` (and `render_separation(page, ink_name)`), each plate a grayscale coverage map where value == tint percent; `get_page_inks`/`get_page_inks_deep` enumerate the spot/process ink names first. This is the measured coverage source composing the CMYK/separations plane (`colour-cxf` CxF3 exchange + `colour-science` CMYK math + Pillow `ImageCms` device-link), never a hand-derived separation.
- redaction axis (`document/egress#FINISH` REDACT/SCRUB): `add_redaction(page, rect)` marks, `apply_all_redactions()`/`apply_page_redactions(page)` burns, `apply_redactions_destructive(...)` is the irreversible scrub; `remove_headers`/`remove_footers`/`remove_artifacts(threshold=)` repeat-detect-and-strip running content, `erase_region(s)` erases by rect; the commercial-safe `Finisher` for REDACT/SCRUB/SANITIZE binds here.
- create axis (`document/emit#DOCUMENT`): two modalities under one rail — `Pdf.from_markdown`/`from_html`/`from_html_css_with_fonts`/`from_text`/`from_image(s)`/`merge` is the declarative `Backend` arm; `DocumentBuilder().tagged_pdf_ua1().register_embedded_font(...).a4_page().font(...).heading(...).table(t).done().build()` is the fluent tagged-PDF/UA arm. `EmbeddedFont` is one-shot and `FluentPageBuilder` single-use per page; tables route through buffered `Table`/`Column`/`Align` or row-streaming `StreamingTable`; colors/gradients/patterns/`BlendMode`/`ExtGState` are the graphics axis.
- conformance axis (`exchange/conformance#CONFORMANCE`, `document/tagged#ACCESS`): `validate_pdf_a(level=)`/`validate_pdf_x(level=)`/`validate_pdf_ua()` are the in-process standards oracle, `convert_to_pdf_a(level=)` upgrades to archival, and `has_structure_tree`/`has_text_layer`/`has_xfa` are the tagged-PDF discriminants — the dependency-free complement to the `veraPDF` JRE cross-check.
- signing axis (`exchange/conformance#CONFORMANCE`): `sign_pdf_bytes_pades(pdf_data, cert, level=PadesLevel.B_LTA, tsa_url=, revocation=RevocationMaterial(...))` is the byte-level PAdES signer with TSA + DSS/LTV; `Certificate.load_pkcs12(data, password)` loads the signer; `TsaClient(url).request_timestamp(data)` is the RFC-3161 client; `signatures()`/`Signature.verify_detached(data)` + `dss()` are the audit-read half. This stacks beside the `pyhanko` conformance owner as the dependency-free signer where `pyhanko`'s cryptography stack is unavailable; the pluggable provider surface (`crypto_set_policy`/`crypto_use_fips`/`crypto_cbom`) is FIPS/CBOM-grade.
- office axis: `OfficeConverter.from_docx(path)` → `Pdf` (+ `from_pptx`/`from_xlsx` + `_bytes`) imports Office in-process with no LibreOffice subprocess; `PdfDocument.to_docx`/`to_xlsx`/`to_pptx` (+ `_bytes`) exports the reverse, the XLSX export composing the `visualization/table#TABLE` tabular output.
- async axis: `AsyncPdfDocument`/`AsyncPdf`/`AsyncOfficeConverter` run every op on the built-in single-worker pool, so a consumer on the loop awaits them directly, while the sync `PdfDocument`/`Pdf` root crosses the runtime `LanePolicy.offload` thread arm (`self.lane.offload(Kernel.of(..., KernelTrait.RELEASING))` — the GIL-releasing Rust render/extract runs off the scheduler under the runtime band), never inline on the loop and never double-offloading the async mirror through a second thread.
- search axis: `search(pattern, case_insensitive=, literal=, whole_word=, max_results=)`/`search_page` is native regex/literal full-text search → hit-region `AnnotationNode(annot=HIGHLIGHT)` leaves in the `SEARCH` `LensOp`; `classify_document`/`classify_page` route the auto-extraction profile.
- split axis (`composition/imposition#IMPOSE`): `split_by_bookmarks(src_bytes, level=, include_front_matter=)` (with `plan_split_by_bookmarks` dry-run) is the bookmark-driven split; `extract_page_ranges_to_bytes(ranges)`/`extract_pages_to_bytes(pages)` are the explicit-range arms feeding plan-set assembly.
- logging axis: `set_log_level("debug")`/`disable_logging()` route the Rust core's diagnostics into the `structlog` rail at the boundary; `set_max_ops_per_stream(limit)`/`set_preserve_unmapped_glyphs(preserve)` are the content-stream-safety + glyph-fidelity controls for untrusted-PDF intake.
- evidence: each operation captures page count, render dpi/format, separation ink names + plate count, extracted word/line/table counts, redaction count, PDF/A-X-UA verdict, PAdES level achieved + TSA time, signature valid/trusted/broken counts, and output byte length as a pdf receipt feeding `document/emit#DOCUMENT` `EmitFact` / `document/lens#LENS` `Introspection` / `exchange/conformance#CONFORMANCE` `ConformanceVerdict`.
- typed-model boundary: boundary code admits the `extract_words`/`extract_tables`/`get_form_fields` PyO3 returns into the `msgspec`/`pydantic` discriminated `DocumentNode`/`RunNode`/`TableNode` models once, never forwarding raw PyO3 objects into the interior. `@beartype` guards the boundary functions so a wrong-shaped `region`/`table_settings`/`ExtractionProfile` is caught before crossing into the Rust core; a `pdf_oxide` raise wraps into the runtime `BoundaryFault` rail via the `async_boundary` capsule + an `expression` `Result`, and `stamina.retry` weaves over the `TsaClient`/PAdES network seam.
- boundary: `Pdf.create()` from the package docstring is not a real classmethod — create through the `from_*` factories or `DocumentBuilder`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pdf_oxide` (`pdf-oxide`)
- License: `MIT OR Apache-2.0` — the commercial-safe render/extract/redact/separation/create owner and per-concern supersession candidate for the AGPL `pymupdf` arms on the closed/distributed path
- Owns: commercial-safe PDF open-edit-extract-render-redact-sanitize-sign root (`PdfDocument`), reading-order/layout-aware extraction with `ExtractionProfile` tuning + word/char/line/span geometry, native table extraction, region-restricted re-extraction, render to encoded bytes + raw `RenderedPixmap`, ink `SeparationPlate` prepress rendering, redaction-grade scrub + sanitize + header/footer/artifact strip, page assembly/crop/rotate + embedded-image edit, AcroForm read/fill/export + flatten, native outline/layer/label/annotation/XMP recovery, PDF/A-PDF/X-PDF/UA validate + PDF/A convert, PAdES B-B…B-LTA signing with TSA + DSS/LTV + verify, pluggable crypto providers + FIPS + CBOM, declarative (`Pdf.from_*`) and fluent tagged-PDF/UA (`DocumentBuilder`/`FluentPageBuilder`) creation with embedded fonts + gradients/patterns/blend-modes + barcodes, Office round-trip conversion, bookmark-driven split, and native async mirrors
- Accept: layout-aware extract → `document/lens#LENS` `RunNode`/`TableNode`; render/pixmap → `graphic/raster/io#RASTER`; separations → `graphic/color/managed#MANAGED` CMYK/separations plane; create → `document/emit#DOCUMENT` `Backend`; redact/sanitize/flatten/outline → `document/egress#FINISH` `Finisher`; validate + PAdES sign → `exchange/conformance#CONFORMANCE`; Office export → `visualization/table#TABLE` tabular feed; the async mirror where the consumer is on the loop
- Reject: wrapper-renames of `extract_words`/`render_page`/`render_separations`/`apply_redactions_destructive`/`sign_pdf_bytes_pades`/`validate_pdf_a`; a hand-rolled gap-threshold table where `ExtractionProfile` tunes layout; re-clustering chars where `extract_words`/`extract_text_lines` carry geometry; re-deriving separations where `render_separations` measures ink coverage; an AGPL `pymupdf` render/extract/redact arm on the commercial-safe path where `pdf_oxide` covers it permissively; double-offloading the native async mirror through another thread; a parallel barcode/QR generator where `generate_barcode_svg`/`generate_qr_svg` exist; forwarding raw PyO3 value objects into the interior instead of admitting into `msgspec`/`pydantic` models at the boundary; a bare provider `raise` surviving past the `async_boundary` into domain flow; the non-existent `Pdf.create()`; identity minting the runtime owns
