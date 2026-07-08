# [PY_ARTIFACTS_API_OCRMYPDF]

`ocrmypdf` supplies the whole-document OCR-to-PDF/A pipeline owner for the artifacts pdf/document rail: a single `ocr` entrypoint that rasterizes an input PDF or image, runs Tesseract page-by-page, grafts the recognized text layer back over the original raster, and emits a searchable PDF or a validated PDF/A — the `document/lens#LENS` `LensProvider.OCRMYPDF` arm of the `OCR` recovery op. The owner composes `ocr` + a pre-built pydantic `OcrOptions`, the `ExitCode`/`ExitCodeException` rail, and the four exported in-package owners — `pdfinfo` (the `PdfInfo`/`PageInfo` layout/structure analyzer), `pdfa` (the Ghostscript PDF/A generation + `file_claims_pdfa` claim-check surface), `hocrtransform` (the `HocrParser` hOCR tree reader the interpose path mutates), and `helpers` (`check_pdf`/`Resolution`/`available_cpu_count` boundary utilities); a campaign that must interpose between OCR and grafting drives the `ocrmypdf.api` two-phase split (`run_hocr_pipeline` then `run_hocr_to_ocr_pdf_pipeline`) rather than two `ocr` calls. It removes any hand-rolled rasterize-OCR-graft loop because the pipeline, Ghostscript PDF/A conversion, hOCR transform, and pdfinfo layout analysis are all in-package, and it never re-implements the Tesseract subprocess orchestration, the `pypdfium2` rasterizer, or the PDF/A color-conversion strategy ocrmypdf already owns. The catalog drives the boundary where ocrmypdf owns OCR-to-searchable-PDF/A, `pymupdf` owns native render/extract, `pypdf`/`pypdfium2` own structural editing/PDFium render, and `pikepdf` owns AES-256/content-stream tokens — every owner meeting the others at PDF bytes; its `ExitCode` return and PDF/A output flow ON TOP OF the universal `expression` `Result`/`Option` rail (the consumer maps `ExitCode.ok` to its success arm), the `anyio` `to_process.run_sync` worker seam (OCR is GIL-heavy + subprocess-bound, never inline on the loop), and the `structlog`+OpenTelemetry `async_boundary` span the consumer wraps every recovery in.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ocrmypdf`
- package: `ocrmypdf`
- import: `ocrmypdf`
- owner: `artifacts`
- rail: pdf / document (OCR)
- installed: `17.8.0` (cp315-resolvable in `.venv`; `api resolve` returns the `pydist` asset — reflection-verified, not source-only)
- license: `MPL-2.0` (the `License-Expression` metadata field; per-file `SPDX-License-Identifier: MPL-2.0`). Copyleft is file-level (weak): linking from a closed consumer is permitted, modifications to ocrmypdf's own source must stay MPL — non-viral for the importing campaign, unlike the AGPL `pymupdf` constraint
- deps (reflection-verified `Requires-Dist`): `pikepdf>=10` (PDF model over QPDF), `pypdfium2>=5.0.0` (PDFium page rasterizer), `pdfminer-six>=20220319` (text extraction), `img2pdf>=0.5` (image->PDF), `Pillow>=10.0.1`+`pi-heif` (raster IO/HEIF), `pluggy>=1` (plugin manager), `pydantic>=2.12.5` (`OcrOptions` v2 model), `fpdf2>=2.8.0` (the `fpdf2` text-layer renderer arm), `uharfbuzz>=0.53.2` (glyph shaping for the text layer), `rich>=13` (progress), `packaging>=20`, `deprecation>=2.1.0`; the `watcher`/`webservice` extras (`cyclopts`/`watchdog`/`streamlit`) are unused by the library path
- entry points: console script `ocrmypdf` (CLI, declared in `entry_points.txt`); library use is import-only via `ocrmypdf.ocr()` or the `ocrmypdf.api` lower-level functions — no `[project.entry-points]` plugin group the campaign consumes
- external executables orchestrated (NOT pip deps; resolved on `PATH`): `tesseract` (OCR engine), `ghostscript` (PDF/A conversion + rasterize), `unpaper` (clean), `jbig2enc` (JBIG2 compression), `pngquant` (PNG optimization); `veraPDF` is the campaign's separate PDF/A validation oracle, not invoked by ocrmypdf
- capability: OCR-to-PDF/A conversion of a single PDF or image, Tesseract page OCR with language/PSM/OEM/threshold/timeout control, processing-mode control (default/skip/redo/force), deskew/clean/rotate/oversample/remove-background/remove-vectors image preprocessing, optimization levels with per-format quality knobs, PDF/A output-type and color-conversion control, document-metadata stamping, sidecar text emission, tagged-PDF policy, pre-OCR layout/structure analysis (`pdfinfo`), standalone PDF/A generation + claim-check (`pdfa`), hOCR tree parsing (`hocrtransform`), plugin-driven OCR/render extension, and a typed `ExitCode`/`ExitCodeException` failure rail

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pipeline result, context, and failure roots
- rail: ocr

`ocr` returns an `ExitCode` (an `IntEnum`); failures inside the pipeline raise `ExitCodeException` subclasses, each carrying the matching `ExitCode` as its `exit_code` attribute (reflection-verified: `ExitCodeException().exit_code == 15`/`other_error`). `OcrOptions` is the pydantic v2 `BaseModel` (57 model fields) the resolved options live in — `ocr` accepts a pre-built one as its positional. `PdfContext`/`PageContext` hold the per-run and per-page pipeline state (`PdfContext.__init__(options, work_folder, origin, pdfinfo, plugin_manager)` — the `pdfinfo` attribute is the live `PdfInfo`, the run's page-count + structure source). `Executor`/`OcrEngine` are abstract (`ABC`) extension points for concurrency and alternate OCR backends, registered through the `pluggy` plugin manager via the `hookimpl` marker. The structured-OCR hOCR tree is the `OcrElement` family — `OcrElement` itself plus `BoundingBox`/`Baseline`/`FontInfo`/`OcrClass` (all re-exported by `hocrtransform`) — while `OrientationConfidence` and `OcrEngine` are `pluginspec` types, the engine-contract side, not nodes of the tree. The `17.7.0` public exception family is a flat lattice: every exported error derives DIRECTLY from `ExitCodeException` and binds its own `exit_code`; `InputFileError`, `DpiError`, and `UnsupportedImageFormatError` are sibling `ExitCodeException` subclasses that all share `exit_code=input_file=2` (`DpiError`/`UnsupportedImageFormatError` are NOT nested under `InputFileError` — reflection confirms `__mro__[1] is ExitCodeException` for all three), so a `match` on `exit_code` groups them and `except ExitCodeException` catches the family. The boundary keys on the `exit_code` int, not on a deeper subclass tree.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| --- | --- | --- | --- |
| [01] | `ExitCode` | result enum | `IntEnum` exit codes (`ok=0`, `bad_args=1`, `input_file=2`, `missing_dependency=3`, `invalid_output_pdf=4`, `file_access_error=5`, `already_done_ocr=6`, `child_process_error=7`, `encrypted_pdf=8`, `invalid_config=9`, `pdfa_conversion_failed=10`, `other_error=15`, `ctrl_c=130`) |
| [02] | `Verbosity` | logging enum | `IntEnum` verbosity (`quiet=-1`, `default=0`, `debug=1`, `debug_all=2`) |
| [03] | `TaggedPdfMode` | policy enum | `StrEnum` tagged-PDF behavior (`default`/`ignore`) |
| [04] | `OcrOptions` | options model | pydantic v2 `BaseModel` of the resolved run options; accepted as the `ocr` positional |
| [05] | `PdfContext` | context | per-run pipeline state (`options`, `work_folder`, `origin`, `pdfinfo`, `plugin_manager`); `get_page_contexts`/`get_path` |
| [06] | `PageContext` | context | per-page pickle-able pipeline state |
| [07] | `Executor` | abstract service | `ABC` concurrent task executor extension point (`pbar_class`, `pool_lock`) |
| [08] | `OcrEngine` | abstract service | `ABC` OCR-engine plugin contract; required `@abstractmethod` set is `creator_tag`/`generate_hocr`/`generate_pdf`/`get_orientation`/`languages`/`version`/`str`. `generate_ocr`/`get_deskew`/`supports_generate_ocr` are concrete defaults a subclass may override (sandwich/`fpdf2` text-layer renderers use `generate_ocr`) |
| [09] | `OcrElement` | model | hOCR tree node (`bbox`/`baseline`/`poly`/`font`/`confidence`/`direction`/`textangle`/`dpi`/`language`/`page_number`/`logical_page_number`/`text`/`lines`/`words`/`paragraphs`; `find_by_class`/`iter_by_class`/`get_text_recursive`) |
| [10] | `BoundingBox` | model | axis-aligned pixel bounding box (`width`/`height`) |
| [11] | `Baseline` | model | text baseline `y = slope*x + intercept` |
| [12] | `FontInfo` | model | font `name`/`size` + `bold`/`italic`/`serif`/`monospace`/`underline`/`smallcaps` style |
| [13] | `OcrClass` | constants | hOCR element-class constants (`PAGE`/`CAREA`/`PARAGRAPH`/`LINE`/`WORD`/`CHAR`/`HEADER`/`FOOTER`/`CAPTION`/`TEXTFLOAT`) |
| [14] | `OrientationConfidence` | model | `NamedTuple(angle, confidence)` page rotation + confidence |
| [15] | `ExitCodeException` | error root | base failure carrying `exit_code` (default `other_error=15`) + `message`; `<- Exception` |
| [16] | `BadArgsError` | error | invalid argument combination; `<- ExitCodeException` (`exit_code=1`) |
| [17] | `InputFileError` | error | bad input file; `<- ExitCodeException` (`exit_code=2`) |
| [18] | `DpiError` | error | image DPI missing or invalid; `<- ExitCodeException` (`exit_code=2`) |
| [19] | `UnsupportedImageFormatError` | error | input image format unsupported; `<- ExitCodeException` (`exit_code=2`) |
| [20] | `MissingDependencyError` | error | third-party executable missing; `<- ExitCodeException` (`exit_code=3`) |
| [21] | `OutputFileAccessError` | error | output path inaccessible; `<- ExitCodeException` (`exit_code=5`) |
| [22] | `PriorOcrFoundError` | error | file already has OCR; `<- ExitCodeException` (`exit_code=6`) |
| [23] | `SubprocessOutputError` | error | a child subprocess failed; `<- ExitCodeException` (`exit_code=7`) |
| [24] | `EncryptedPdfError` | error | input PDF is encrypted; `<- ExitCodeException` (`exit_code=8`) |
| [25] | `TesseractConfigError` | error | invalid Tesseract configuration; `<- ExitCodeException` (`exit_code=9`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pipeline run
- rail: ocr

`ocr` is the single library entrypoint. Positional `input_file_or_options` accepts a path/IO or a pre-built `OcrOptions`; when a path is given, `output_file` is required. All processing knobs are keyword-only `| None` rows that default to the CLI defaults when omitted. `mode` (`'default'`/`'skip'`/`'redo'`/`'force'`) supersedes the legacy `force_ocr`/`skip_text`/`redo_ocr` booleans. The call returns an `ExitCode`; failures raise `ExitCodeException` subclasses.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `ocr` | `ocr(input_file_or_options, output_file=None, *, language=None, image_dpi=None, output_type=None, sidecar=None, jobs=None, use_threads=None, mode=None, force_ocr=None, skip_text=None, redo_ocr=None, ...) -> ExitCode` | run the OCR-to-PDF/A pipeline on one PDF or image |

[ENTRYPOINT_SCOPE]: `ocr` keyword axes
- rail: ocr

The keyword rows below are the campaign-consumed subset of the reflected `ocr` signature; every row is a keyword-only parameter typed `| None` (default applied when omitted). The full signature carries 57 keyword-only params plus `**kwargs` (reflection-verified count); the rows omitted here are jbig2/quality micro-tuning (`jpg_quality`/`png_quality`/`jbig2_lossy`/`jbig2_page_group_size`/`jbig2_threshold`), temp-file controls (`keep_temporary_files`), and `use_threads`/`no_overwrite`/`continue_on_soft_render_error`/`unpaper_args`/`max_image_mpixels`/`rasterizer` (`auto`/`ghostscript`/`pypdfium`)/`tesseract_non_ocr_timeout`/`tesseract_downsample_above`/`tesseract_downsample_large_images`/`rotate_pages_threshold`.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `ocr(language=...)` | `Iterable[str] \| None` | Tesseract language pack selection (e.g. `['eng']`) |
| [02] | `ocr(output_type=...)` | `str \| None` | output target `auto`/`pdfa`/`pdf`/`pdfa-1`/`pdfa-2`/`pdfa-3`/`none` |
| [03] | `ocr(mode=...)` | `str \| None` | processing mode `default`/`skip`/`redo`/`force` |
| [04] | `ocr(image_dpi=...)` | `int \| None` | DPI for image input lacking embedded resolution |
| [05] | `ocr(sidecar=...)` | `PathOrIO \| None` | write recognized text to a sidecar file |
| [06] | `ocr(rotate_pages=...)` | `bool \| None` | auto-rotate pages by OCR orientation detection |
| [07] | `ocr(deskew=...)` | `bool \| None` | deskew pages before OCR |
| [08] | `ocr(clean=...)` / `ocr(clean_final=...)` | `bool \| None` | clean with unpaper before OCR / clean the final output too |
| [09] | `ocr(remove_background=...)` | `bool \| None` | flatten page background before OCR |
| [10] | `ocr(remove_vectors=...)` | `bool \| None` | drop vector graphics before rasterize |
| [11] | `ocr(oversample=...)` | `int \| None` | rasterize at a higher DPI before OCR |
| [12] | `ocr(optimize=...)` | `int \| None` | optimization level (0-3) |
| [13] | `ocr(color_conversion_strategy=...)` | `str \| None` | Ghostscript PDF/A color-conversion strategy (e.g. `RGB`/`UseDeviceIndependentColor`) |
| [14] | `ocr(pdfa_image_compression=...)` | `str \| None` | PDF/A image codec selection (`auto`/`jpeg`/`lossless`) |
| [15] | `ocr(tagged_pdf_mode=...)` | `TaggedPdfMode \| str \| None` | tagged-PDF handling (`default`/`ignore`) |
| [16] | `ocr(tesseract_config=...)` | `Iterable[str] \| None` | extra Tesseract config flags |
| [17] | `ocr(tesseract_pagesegmode=...)` | `int \| None` | Tesseract `--psm` page-segmentation mode |
| [18] | `ocr(tesseract_oem=...)` | `int \| None` | Tesseract `--oem` OCR-engine mode |
| [19] | `ocr(tesseract_thresholding=...)` | `int \| None` | Tesseract thresholding method as the `ThresholdingMethod` int (CLI maps the `auto`/`otsu`/`sauvola`/`adaptive-otsu` names; the API row is the int) |
| [20] | `ocr(tesseract_timeout=...)` | `float \| None` | per-page Tesseract timeout in seconds |
| [21] | `ocr(user_words=...)` / `ocr(user_patterns=...)` | `Path \| None` | Tesseract user word list / pattern file |
| [22] | `ocr(pdf_renderer=...)` | `str \| None` | text-layer renderer `auto`/`hocr`/`sandwich`/`fpdf2` (plus debug-only `hocrdebug`) — `sandwich`/`fpdf2` use the `OcrEngine.generate_ocr` default |
| [23] | `ocr(title=, author=, subject=, keywords=...)` | `str \| None` | stamp output PDF document-info metadata |
| [24] | `ocr(fast_web_view=...)` | `float \| None` | linearize for fast web view above this size (MB) |
| [25] | `ocr(invalidate_digital_signatures=...)` | `bool \| None` | proceed on a signed PDF, invalidating the signature |
| [26] | `ocr(skip_big=...)` | `float \| None` | skip OCR on pages larger than this megapixel threshold |
| [27] | `ocr(jobs=...)` | `int \| None` | worker count for parallel page processing |
| [28] | `ocr(pages=...)` | `str \| None` | page range to OCR |
| [29] | `ocr(plugins=...)` | `Iterable[Path \| str] \| None` | load OCR/render plugins by import path |
| [30] | `ocr(plugin_manager=...)` | `OcrmypdfPluginManager \| None` | inject a pre-built plugin manager instead of a plugin list |
| [31] | `ocr(progress_bar=...)` | `bool \| None` | toggle the progress bar |

[ENTRYPOINT_SCOPE]: logging configuration
- rail: ocr

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `configure_logging` | `configure_logging(verbosity: Verbosity, *, progress_bar_friendly=True, manage_root_logger=False, plugin_manager=None)` | install ocrmypdf-style log handlers under `ocrmypdf` |
| [02] | `configure_debug_logging` | `configure_debug_logging(log_filename: Path, prefix='') -> tuple[FileHandler, Callable[[], None]]` | attach the debug-log file handler; returns the handler + a remover callable |

Integration: the campaign manages its own `structlog` root, so it SKIPS `configure_logging` (which installs CLI-style handlers) and lets ocrmypdf log under the `ocrmypdf` namespace — `structlog`'s `ProcessorFormatter` captures that stdlib-logger output, no second logging stack. `manage_root_logger=False` is load-bearing: it keeps ocrmypdf from seizing the root logger the universal `structlog` rail owns.

[ENTRYPOINT_SCOPE]: lower-level `ocrmypdf.api` pipeline and plugin manager
- rail: ocr

`ocr()` is a thin wrapper that builds an `OcrOptions` (via `create_options`), resolves a plugin manager (`get_plugin_manager`), and calls `run_pipeline`. A campaign that needs to interpose between OCR and grafting (custom hOCR post-processing, alternate text-layer source) uses the two-phase split: `run_hocr_pipeline` rasterizes and OCRs to hOCR/text only (returns `None`), then `run_hocr_to_ocr_pdf_pipeline` grafts the (possibly mutated) hOCR back into a PDF/A and returns the `ExitCode`. All `api` functions take a resolved `OcrOptions` plus a keyword-only `plugin_manager`.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `api.create_options` | `create_options(*, input_file, output_file, parser, **kwargs) -> OcrOptions` | build and validate the `OcrOptions` model from kwargs |
| [02] | `api.get_plugin_manager` | `get_plugin_manager(plugins=None, builtins=True) -> OcrmypdfPluginManager` | construct the pluggy manager with builtin + named plugins |
| [03] | `api.check_options` | `check_options(options, plugin_manager) -> None` | validate option combinations against the plugin set |
| [04] | `api.run_pipeline` | `run_pipeline(options, *, plugin_manager) -> ExitCode` | run the full pipeline from a resolved `OcrOptions` |
| [05] | `api.run_hocr_pipeline` | `run_hocr_pipeline(options, *, plugin_manager) -> None` | phase 1: rasterize + OCR to hOCR/text only |
| [06] | `api.run_hocr_to_ocr_pdf_pipeline` | `run_hocr_to_ocr_pdf_pipeline(options, *, plugin_manager) -> ExitCode` | phase 2: graft hOCR into the searchable PDF/A |
| [07] | `hookimpl` | `@hookimpl` (pluggy `HookimplMarker('ocrmypdf')`) | decorate an `OcrEngine`/`Executor`/render plugin hook |

## [04]-[INPACKAGE_OWNERS]

[INPACKAGE_OWNER_SCOPE]: the four `__all__`-exported submodule owners
- rail: pdf / document

`ocrmypdf.__all__` exports four in-package owners alongside `ocr` — `pdfinfo`, `pdfa`, `hocrtransform`, `helpers` — so the campaign reaches OCR-adjacent capability (pre-OCR layout analysis, standalone PDF/A conversion, hOCR parsing, PDF validation) WITHOUT shelling the CLI or re-importing Tesseract. These are the depth the rail composes when `ocr` alone is too coarse: `document/lens#LENS` reads `pdfinfo.PdfInfo` to decide whether a page even needs OCR (skip a page that already `has_text`), the print-production telos reaches `pdfa.speculative_pdfa_conversion`/`file_claims_pdfa` as the one Ghostscript PDF/A egress feeding the veraPDF preflight oracle, and the interpose path parses the emitted hOCR with `hocrtransform.HocrParser` before grafting.

| [INDEX] | [OWNER] | [SYMBOL_SIGNATURE] | [CAPABILITY_BOUNDARY] |
| --- | --- | --- | --- |
| [01] | `pdfinfo` | `PdfInfo(infile: Path, *, detailed_analysis=False, progbar=False, max_workers=None, use_threads=True, check_pages=None, executor=SerialExecutor())` | open a PDF and analyze its structure; the run's layout-analysis owner |
| [02] | `pdfinfo` | `PdfInfo.pages -> Sequence[PageInfo]` · `.is_tagged` · `.has_acroform` · `.has_signature` · `.has_structure_tree` · `.has_userunit` · `.needs_rendering` · `.min_version` · `.filename` | document-level structure facts; `needs_rendering`/`is_tagged` gate the OCR + tagged-PDF decisions, `has_acroform`/`has_signature` flag a form/signed PDF |
| [03] | `pdfinfo` | `PageInfo.has_text` · `.has_vector` · `.has_corrupt_text` · `.dpi` · `.mediabox`/`.cropbox`/`.bleedbox`/`.trimbox`/`.artbox` · `.images` · `.width_inches`/`.height_inches` · `.rotation` · `.get_textareas()` · `.page_dpi_profile` | per-page analysis: `has_text` is the skip-OCR predicate, `dpi`/`page_dpi_profile` size the rasterize, the box family + `rotation` recover page geometry |
| [04] | `pdfinfo` | `Colorspace` · `Encoding` · `Ink` · `FloatRect` | the value vocabulary `PageInfo` returns (colorspace/encoding enums, ink coverage, float rect) |
| [05] | `pdfa` | `speculative_pdfa_conversion(input_file: Path, output_file: Path, output_type: str) -> Path` | run the in-package Ghostscript PDF/A conversion on an already-OCR'd PDF; the one PDF/A egress, never a hand-stitched `gs` invocation |
| [06] | `pdfa` | `file_claims_pdfa(filename: Path) -> dict[str, str \| bool]` (keys `pass`/`output`/`conformance`, e.g. `{'pass': True, 'output': 'pdfa', 'conformance': 'PDF/A-2b'}`) | cheap pre-check reading the XMP whether a file already declares PDF/A conformance, the gate before re-converting; pairs with veraPDF for actual validation |
| [07] | `pdfa` | `add_pdfa_metadata(pdf: Pdf, part: str, conformance: str) -> None` · `add_srgb_output_intent(pdf: Pdf) -> None` · `generate_pdfa_ps(target_filename: Path, icc='sRGB') -> Path` · `SRGB_ICC_PROFILE_NAME` | low-level PDF/A assembly over a `pikepdf.Pdf`: stamp XMP PDF/A metadata, add the sRGB OutputIntent, build the Ghostscript PostScript prologue — the seam to `graphic/color/managed` ICC egress |
| [08] | `hocrtransform` | `HocrParser(hocr_file: str \| Path)` · `HocrParser.parse(...)` · `HocrParseError` | parse an emitted hOCR file into the `OcrElement` tree; the interpose path mutates the tree between `run_hocr_pipeline` and `run_hocr_to_ocr_pdf_pipeline` |
| [09] | `hocrtransform` | re-exports `OcrElement`/`BoundingBox`/`Baseline`/`FontInfo`/`OcrClass` | the hOCR node algebra; identical to the `ocrmypdf.*` exports, co-located with the parser that builds them |
| [10] | `helpers` | `check_pdf(input_file: Path) -> bool` · `Resolution(x, y)` · `available_cpu_count() -> int` · `is_file_writable(...)` · `IMG2PDF_KWARGS` | boundary utilities: `check_pdf` validates a PDF is openable before the pipeline, `Resolution` is the DPI value object, `available_cpu_count` sizes the `jobs` default |

[INPACKAGE_OWNER_SCOPE]: layout-analysis-gated OCR
- rail: document

The `pdfinfo.PdfInfo` owner is the cheap pre-flight the `document/lens#LENS` `OCR` arm runs BEFORE committing to a full rasterize-OCR-graft cycle: a page whose `PageInfo.has_text` is already true needs no OCR (`mode='skip'` territory), `has_corrupt_text` flags a redo candidate, `has_vector`/`images` distinguish a vector page from a scanned raster, and `is_tagged`/`has_structure_tree` route the tagged-PDF policy. This is read-only analysis sharing the same `Executor` abstraction the pipeline uses (`use_threads`/`max_workers`/`executor`), so a campaign that has already analyzed the document hands the facts forward rather than re-opening it.

## [05]-[IMPLEMENTATION_LAW]

[OCR_PIPELINE]:
- import: `lazy import ocrmypdf` (the `document/lens` import form) — never a module-level import; the manifest import policy bans top-level, and the consumer reifies the binding only inside the worker process. `ocr` itself takes path/IO arguments (its own `NamedTemporaryFile`-backed source/target), so the bytes-in/bytes-out boundary lives in the consuming arm, not in ocrmypdf.
- entry axis: one `ocr` owns the full rasterize-OCR-graft-PDF/A pipeline; `language`/`output_type`/`mode`/`optimize`/`deskew`/`clean`/`rotate_pages`/`color_conversion_strategy`/`tesseract_*` are keyword rows on that call, never a per-config builder type or a `run_force`/`run_skip`/`run_redo` family — `mode` discriminates the processing behavior. The `document/lens#LENS` `OCR` arm calls `ocrmypdf.ocr(source.name, target.name, sidecar=sidecar.name, language=spec.language, output_type='pdfa', mode='force', deskew=, clean=, rotate_pages=, optimize=, progress_bar=False)` and reads `sidecar.read()` only when `code is ocrmypdf.ExitCode.ok`. A campaign holding a validated `OcrOptions` pydantic model passes it as the positional instead of unpacking kwargs; a campaign needing to interpose between OCR and graft uses the `api.run_hocr_pipeline` -> `api.run_hocr_to_ocr_pdf_pipeline` split (mutating the `hocrtransform.HocrParser` tree between phases) rather than two separate `ocr` calls.
- result axis: `ocr` returns `ExitCode` (`ok=0`); the consumer compares `code is ocrmypdf.ExitCode.ok` to gate the sidecar feed and maps the `IntEnum` onto the universal `expression` `Result` rail — `ExitCode.ok` -> `Ok`, every other code carried as the receipt's `code.name` — never re-deriving exit semantics from string parsing. `output_type='none'` runs OCR for the sidecar text only and writes no PDF, the natural shape when only the recovered text feeds `DocumentNode`.
- concurrency axis: OCR is GIL-heavy AND subprocess-bound (Tesseract/Ghostscript children), so the consumer crosses the universal `anyio` `to_process.run_sync(_gated_recover, self, limiter=_OFFLOAD)` worker seam — ocrmypdf runs on the worker lane, never inline on the event loop, bounded by the shared `CapacityLimiter`. Inside ocrmypdf, page-level parallelism is the `jobs` row over the `Executor` `ABC` (default fan = `helpers.available_cpu_count()`); the two layers compose — outer anyio process offload, inner ocrmypdf `Executor` page fan — never a hand-rolled `multiprocessing` pool around `ocr`.
- failure axis: pipeline faults raise `ExitCodeException` subclasses, each binding its `ExitCode` (`MissingDependencyError`->`missing_dependency=3`, `InputFileError`/`DpiError`/`UnsupportedImageFormatError`->`input_file=2`, `EncryptedPdfError`->`encrypted_pdf=8`, `PriorOcrFoundError`->`already_done_ocr=6`, `SubprocessOutputError`->`child_process_error=7`, `TesseractConfigError`->`invalid_config=9`); the `17.7.0` public lattice is flat — every exported error is a direct `ExitCodeException` subclass, so the boundary discriminates on the `exit_code` int (a `match` over `ExitCode`), not on a deeper subclass tree, and `DpiError`/`UnsupportedImageFormatError` share `input_file=2` with `InputFileError` without being nested under it. PDF/A validation failure surfaces as `pdfa_conversion_failed=10`. The consumer lets the raise convert to the runtime `BoundaryFault` at the `async_boundary` capsule (the same span that traces every `document/lens` recovery), never catching bare `Exception` and never flattening the lattice in the interior `LensFault` vocabulary.
- output axis: `output_type` selects `auto`/`pdf`/`pdfa`/`pdfa-1`/`pdfa-2`/`pdfa-3`/`none` as a call row; PDF/A conversion is delegated to the in-package Ghostscript path with a `color_conversion_strategy` and `pdfa_image_compression` row, never a hand-stitched external `gs` invocation. A campaign that already has an OCR'd PDF and only needs the PDF/A step reaches `pdfa.speculative_pdfa_conversion(input, output, output_type)` directly, then `pdfa.file_claims_pdfa` as the cheap conformance gate and the veraPDF oracle for actual validation — the one PDF/A egress feeding the print-production `graphic/color/managed` plane.
- analysis axis: `pdfinfo.PdfInfo(infile, detailed_analysis=)` is the pre-OCR layout owner; `PageInfo.has_text` skips a page that needs no OCR, `has_corrupt_text` flags a redo, `PdfInfo.is_tagged`/`has_acroform`/`has_signature` route the tagged/form/signed-PDF policy — read-only analysis the consumer runs before committing to a rasterize-OCR-graft cycle, sharing the same `Executor`/`use_threads`/`max_workers` knobs.
- plugin axis: alternate OCR backends, executors, and renderers extend through the `pluggy` plugin manager (`api.get_plugin_manager`, `ocr(plugin_manager=...)`) implementing the `OcrEngine`/`Executor` `ABC` contract with `@hookimpl`-decorated hooks, never a fork of the pipeline; `OcrEngine`'s required `@abstractmethod` set is `creator_tag`/`generate_hocr`/`generate_pdf`/`get_orientation`/`languages`/`version`/`__str__`, with `generate_ocr`/`get_deskew`/`supports_generate_ocr` concrete defaults a backend overrides only when it sources its own text layer.
- logging axis: the campaign manages its own `structlog` root, so it SKIPS `configure_logging` (which installs CLI-style handlers) and lets ocrmypdf log under the `ocrmypdf` stdlib namespace, captured by the universal `structlog` `ProcessorFormatter`; `manage_root_logger=False` keeps ocrmypdf off the root logger the rail owns.
- evidence: each run captures the returned `ExitCode`, the resolved `output_type`, language set, processing `mode`, page count from `pdfinfo.PdfInfo.pages` (or `PdfContext.pdfinfo.pages`), sidecar emission, and any raised `ExitCodeException` `exit_code`/`message` as the OCR contribution to the `document/lens` `ArtifactReceipt.Introspection` case.
- boundary: ocrmypdf owns OCR, text-layer grafting, and PDF/A conversion of one document per call; Tesseract, Ghostscript, unpaper, jbig2enc, and pngquant are external executables the package orchestrates (resolved on `PATH`, not pip deps); concurrent execution and alternate OCR engines extend through the `Executor`/`OcrEngine` abstractions, never a parallel pipeline; the outer process offload, single-document scope, and multi-document batching stay in the `document/lens` consumer and the runtime concurrency lane, never re-implemented here.

[RAIL_LAW]:
- Package: `ocrmypdf`
- Owns: OCR-to-PDF/A conversion of a single PDF or image, Tesseract page OCR with language/PSM/OEM/mode control, image preprocessing, PDF/A output-type and color-conversion control (incl. the standalone `pdfa` egress + `file_claims_pdfa` claim-check), pre-OCR layout/structure analysis (`pdfinfo.PdfInfo`/`PageInfo`), hOCR parsing (`hocrtransform.HocrParser`), document-metadata stamping, sidecar text emission, plugin-driven extension, and a typed `ExitCode`/`ExitCodeException` rail
- Accept: single-document OCR-to-searchable-PDF/A runs feeding the `document/lens#LENS` `LensProvider.OCRMYPDF` arm (from `ocr` kwargs or a pre-built `OcrOptions`), gated on `code is ExitCode.ok` and mapped onto the universal `expression` `Result` rail; the `api.run_hocr_*` two-phase split (with a `hocrtransform.HocrParser` mutation) when interposing between OCR and graft; `pdfinfo.PdfInfo` as the standalone pre-flight; `pdfa.speculative_pdfa_conversion` as the standalone PDF/A step; HEIF input via the `pi-heif` boundary opener; execution on the `anyio` `to_process` worker lane under the shared `CapacityLimiter`
- Reject: wrapper-renames of `ocr`; a hand-rolled rasterize-OCR-graft loop or external `tesseract`/`gs` orchestration the package owns; a `run_force`/`run_skip`/`run_redo` entrypoint family where `mode` is a call row; bare-`Exception` handling that discards the `ExitCode` rail or flattens the `ExitCodeException` lattice; a hand-rolled `multiprocessing` pool around `ocr` instead of the `anyio` outer offload + `Executor` inner page fan; a forked OCR loop bypassing the `Executor`/`OcrEngine` plugin extension points; a separate `gs` PDF/A invocation where `pdfa.speculative_pdfa_conversion` is the in-package egress; phantom members — `HocrTransform` does NOT exist (the class is `HocrParser`); `pymupdf`/`pdfplumber` are NOT ocrmypdf deps (its deps are `pypdfium2`/`pdfminer-six`/`fpdf2`/`uharfbuzz`)
