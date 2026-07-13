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

`ocr` returns an `ExitCode` (`IntEnum`) and raises `ExitCodeException` subclasses on failure — both are the lattice at the end of this section. `OcrOptions` is the pydantic v2 `BaseModel` (57 model fields) the resolved options live in; `ocr` accepts a pre-built one as its positional. `PdfContext`/`PageContext` hold the per-run and per-page pipeline state (`PdfContext.__init__(options, work_folder, origin, pdfinfo, plugin_manager)` — `pdfinfo` is the live `PdfInfo`, the run's page-count + structure source). `Executor`/`OcrEngine` are `ABC` extension points for concurrency and alternate OCR backends, `pluggy`-registered via `hookimpl`. The structured-OCR hOCR tree is the `OcrElement` family — `OcrElement` plus `BoundingBox`/`Baseline`/`FontInfo`/`OcrClass` (all re-exported by `hocrtransform`) — while `OrientationConfidence` and `OcrEngine` are `pluginspec` types on the engine-contract side, not tree nodes. `OcrElement` carries `bbox`/`baseline`/`poly`/`font`/`confidence`/`direction`/`textangle`/`dpi`/`language`/`page_number`/`logical_page_number`/`text`/`lines`/`words`/`paragraphs`.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]    | [RAIL]                                                                                  |
| :-----: | :---------------------- | :--------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `Verbosity`             | logging enum     | `IntEnum` verbosity (`quiet=-1`, `default=0`, `debug=1`, `debug_all=2`)                 |
|  [02]   | `TaggedPdfMode`         | policy enum      | `StrEnum` tagged-PDF behavior (`default`/`ignore`)                                      |
|  [03]   | `OcrOptions`            | options model    | pydantic v2 `BaseModel` of the resolved run options; accepted as the `ocr` positional   |
|  [04]   | `PdfContext`            | context          | per-run pipeline state; `get_page_contexts`/`get_path`                                  |
|  [05]   | `PageContext`           | context          | per-page pickle-able pipeline state                                                     |
|  [06]   | `Executor`              | abstract service | `ABC` concurrent task executor extension point (`pbar_class`, `pool_lock`)              |
|  [07]   | `OcrEngine`             | abstract service | `ABC` OCR-engine plugin contract, `pluggy`-registered                                   |
|  [08]   | `OcrElement`            | model            | hOCR tree node; `find_by_class`/`iter_by_class`/`get_text_recursive` walk the attr tree |
|  [09]   | `BoundingBox`           | model            | axis-aligned pixel bounding box (`width`/`height`)                                      |
|  [10]   | `Baseline`              | model            | text baseline `y = slope*x + intercept`                                                 |
|  [11]   | `FontInfo`              | model            | font `name`/`size` + `bold`/`italic`/`serif`/`monospace`/`underline`/`smallcaps` style  |
|  [12]   | `OcrClass`              | constants        | `PAGE`/`CAREA`/`PARAGRAPH`/`LINE`/`WORD`/`CHAR`/`HEADER`/`FOOTER`/`CAPTION`/`TEXTFLOAT` |
|  [13]   | `OrientationConfidence` | model            | `NamedTuple(angle, confidence)` page rotation + confidence                              |

[PUBLIC_TYPE_SCOPE]: exit-code / error lattice
- rail: ocr

`ExitCode` is the `IntEnum` `ocr` returns; every exported error derives DIRECTLY from `ExitCodeException` (default `other_error=15`, carrying `message`, `<- Exception`) and binds one code. The lattice is flat — `DpiError`/`UnsupportedImageFormatError` share `input_file=2` with `InputFileError` without nesting under it, and the boundary matches on the `exit_code` int (a `match` over `ExitCode`), not a subclass tree. Codes `ok=0`/`invalid_output_pdf=4`/`pdfa_conversion_failed=10`/`ctrl_c=130` carry no exported error.

| [INDEX] | [CODE] | [NAME]                   | [BOUND_ERROR]                                                 |
| :-----: | :----: | :----------------------- | :------------------------------------------------------------ |
|  [01]   |   0    | `ok`                     | —                                                             |
|  [02]   |   1    | `bad_args`               | `BadArgsError`                                                |
|  [03]   |   2    | `input_file`             | `InputFileError` / `DpiError` / `UnsupportedImageFormatError` |
|  [04]   |   3    | `missing_dependency`     | `MissingDependencyError`                                      |
|  [05]   |   4    | `invalid_output_pdf`     | —                                                             |
|  [06]   |   5    | `file_access_error`      | `OutputFileAccessError`                                       |
|  [07]   |   6    | `already_done_ocr`       | `PriorOcrFoundError`                                          |
|  [08]   |   7    | `child_process_error`    | `SubprocessOutputError`                                       |
|  [09]   |   8    | `encrypted_pdf`          | `EncryptedPdfError`                                           |
|  [10]   |   9    | `invalid_config`         | `TesseractConfigError`                                        |
|  [11]   |   10   | `pdfa_conversion_failed` | —                                                             |
|  [12]   |   15   | `other_error`            | `ExitCodeException` (base default)                            |
|  [13]   |  130   | `ctrl_c`                 | —                                                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pipeline run
- rail: ocr

`ocr` is the single library entrypoint, running the OCR-to-PDF/A pipeline on one PDF or image. Positional `input_file_or_options` accepts a path/IO or a pre-built `OcrOptions`; when a path is given, `output_file` is required. All processing knobs are keyword-only `| None` rows that default to the CLI defaults when omitted. `mode` (`'default'`/`'skip'`/`'redo'`/`'force'`) supersedes the legacy `force_ocr`/`skip_text`/`redo_ocr` booleans. The call returns an `ExitCode`; failures raise `ExitCodeException` subclasses.
- call: `ocr(input_file_or_options, output_file=None, *, language=None, image_dpi=None, output_type=None, sidecar=None, jobs=None, use_threads=None, mode=None, force_ocr=None, skip_text=None, redo_ocr=None, ...) -> ExitCode`

[ENTRYPOINT_SCOPE]: `ocr` keyword axes
- rail: ocr

The keyword rows below are the campaign-consumed subset of the reflected `ocr` signature; every `[SURFACE]` is `ocr(<param>=...)` and every row is a keyword-only parameter typed `| None` (default applied when omitted). The full signature carries 57 keyword-only params plus `**kwargs` (reflection-verified count); the rows omitted here are jbig2/quality micro-tuning (`jpg_quality`/`png_quality`/`jbig2_lossy`/`jbig2_page_group_size`/`jbig2_threshold`), temp-file controls (`keep_temporary_files`), and `use_threads`/`no_overwrite`/`continue_on_soft_render_error`/`unpaper_args`/`max_image_mpixels`/`rasterizer` (`auto`/`ghostscript`/`pypdfium`)/`tesseract_non_ocr_timeout`/`tesseract_downsample_above`/`tesseract_downsample_large_images`/`rotate_pages_threshold`. `tesseract_thresholding` takes the `ThresholdingMethod` int the CLI `auto`/`otsu`/`sauvola`/`adaptive-otsu` names map to; `pdf_renderer`'s `hocrdebug` value is debug-only, and its `sandwich`/`fpdf2` renderers use the `OcrEngine.generate_ocr` default.

| [INDEX] | [PARAM]                               | [TYPE]                          | [CAPABILITY]                                                 |
| :-----: | :------------------------------------ | :------------------------------ | :----------------------------------------------------------- |
|  [01]   | `language`                            | `Iterable[str] \| None`         | Tesseract language pack selection (e.g. `['eng']`)           |
|  [02]   | `output_type`                         | `str \| None`                   | target `auto`/`pdfa`/`pdf`/`pdfa-1`/`pdfa-2`/`pdfa-3`/`none` |
|  [03]   | `mode`                                | `str \| None`                   | processing mode `default`/`skip`/`redo`/`force`              |
|  [04]   | `image_dpi`                           | `int \| None`                   | DPI for image input lacking embedded resolution              |
|  [05]   | `sidecar`                             | `PathOrIO \| None`              | write recognized text to a sidecar file                      |
|  [06]   | `rotate_pages`                        | `bool \| None`                  | auto-rotate pages by OCR orientation detection               |
|  [07]   | `deskew`                              | `bool \| None`                  | deskew pages before OCR                                      |
|  [08]   | `clean`/`clean_final`                 | `bool \| None`                  | clean with unpaper before OCR / clean the final output too   |
|  [09]   | `remove_background`                   | `bool \| None`                  | flatten page background before OCR                           |
|  [10]   | `remove_vectors`                      | `bool \| None`                  | drop vector graphics before rasterize                        |
|  [11]   | `oversample`                          | `int \| None`                   | rasterize at a higher DPI before OCR                         |
|  [12]   | `optimize`                            | `int \| None`                   | optimization level (0-3)                                     |
|  [13]   | `color_conversion_strategy`           | `str \| None`                   | PDF/A color-conversion (`RGB`/`UseDeviceIndependentColor`)   |
|  [14]   | `pdfa_image_compression`              | `str \| None`                   | PDF/A image codec selection (`auto`/`jpeg`/`lossless`)       |
|  [15]   | `tagged_pdf_mode`                     | `TaggedPdfMode \| str \| None`  | tagged-PDF handling (`default`/`ignore`)                     |
|  [16]   | `tesseract_config`                    | `Iterable[str] \| None`         | extra Tesseract config flags                                 |
|  [17]   | `tesseract_pagesegmode`               | `int \| None`                   | Tesseract `--psm` page-segmentation mode                     |
|  [18]   | `tesseract_oem`                       | `int \| None`                   | Tesseract `--oem` OCR-engine mode                            |
|  [19]   | `tesseract_thresholding`              | `int \| None`                   | Tesseract thresholding method (`ThresholdingMethod` int)     |
|  [20]   | `tesseract_timeout`                   | `float \| None`                 | per-page Tesseract timeout in seconds                        |
|  [21]   | `user_words`/`user_patterns`          | `Path \| None`                  | Tesseract user word list / pattern file                      |
|  [22]   | `pdf_renderer`                        | `str \| None`                   | text-layer renderer `auto`/`hocr`/`sandwich`/`fpdf2`         |
|  [23]   | `title`/`author`/`subject`/`keywords` | `str \| None`                   | stamp output PDF document-info metadata                      |
|  [24]   | `fast_web_view`                       | `float \| None`                 | linearize for fast web view above this size (MB)             |
|  [25]   | `invalidate_digital_signatures`       | `bool \| None`                  | proceed on a signed PDF, invalidating the signature          |
|  [26]   | `skip_big`                            | `float \| None`                 | skip OCR on pages larger than this megapixel threshold       |
|  [27]   | `jobs`                                | `int \| None`                   | worker count for parallel page processing                    |
|  [28]   | `pages`                               | `str \| None`                   | page range to OCR                                            |
|  [29]   | `plugins`                             | `Iterable[Path \| str] \| None` | load OCR/render plugins by import path                       |
|  [30]   | `plugin_manager`                      | `OcrmypdfPluginManager \| None` | inject a pre-built plugin manager instead of a plugin list   |
|  [31]   | `progress_bar`                        | `bool \| None`                  | toggle the progress bar                                      |

[ENTRYPOINT_SCOPE]: logging configuration
- rail: ocr
- call: `configure_logging(verbosity: Verbosity, *, progress_bar_friendly=True, manage_root_logger=False, plugin_manager=None)`
- call: `configure_debug_logging(log_filename: Path, prefix='') -> tuple[FileHandler, Callable[[], None]]`

| [INDEX] | [SURFACE]                 | [CAPABILITY]                                                                |
| :-----: | :------------------------ | :-------------------------------------------------------------------------- |
|  [01]   | `configure_logging`       | install ocrmypdf-style log handlers under `ocrmypdf`                        |
|  [02]   | `configure_debug_logging` | attach the debug-log file handler; returns the handler + a remover callable |

Integration: the campaign manages its own `structlog` root, so it SKIPS `configure_logging` (which installs CLI-style handlers) and lets ocrmypdf log under the `ocrmypdf` namespace — `structlog`'s `ProcessorFormatter` captures that stdlib-logger output, no second logging stack. `manage_root_logger=False` is load-bearing: it keeps ocrmypdf from seizing the root logger the universal `structlog` rail owns.

[ENTRYPOINT_SCOPE]: lower-level `ocrmypdf.api` pipeline and plugin manager
- rail: ocr

`ocr()` is a thin wrapper that builds an `OcrOptions` (via `create_options`), resolves a plugin manager (`get_plugin_manager`), and calls `run_pipeline`. A campaign that needs to interpose between OCR and grafting (custom hOCR post-processing, alternate text-layer source) uses the two-phase split: `run_hocr_pipeline` rasterizes and OCRs to hOCR/text only (returns `None`), then `run_hocr_to_ocr_pdf_pipeline` grafts the (possibly mutated) hOCR back into a PDF/A and returns the `ExitCode`. All `api` functions take a resolved `OcrOptions` plus a keyword-only `plugin_manager`; `check_options` validates option/plugin combinations, and `hookimpl` decorates an `OcrEngine`/`Executor`/render plugin hook. Every `[SURFACE]` is `api.*` unless it is `hookimpl`.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                                                 |
| :-----: | :----------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `create_options`               | `create_options(*, input_file, output_file, parser, **kwargs) -> OcrOptions` |
|  [02]   | `get_plugin_manager`           | `get_plugin_manager(plugins=None, builtins=True) -> OcrmypdfPluginManager`   |
|  [03]   | `check_options`                | `check_options(options, plugin_manager) -> None`                             |
|  [04]   | `run_pipeline`                 | `run_pipeline(options, *, plugin_manager) -> ExitCode`                       |
|  [05]   | `run_hocr_pipeline`            | `run_hocr_pipeline(options, *, plugin_manager) -> None`                      |
|  [06]   | `run_hocr_to_ocr_pdf_pipeline` | `run_hocr_to_ocr_pdf_pipeline(options, *, plugin_manager) -> ExitCode`       |
|  [07]   | `hookimpl`                     | `@hookimpl` (pluggy `HookimplMarker('ocrmypdf')`)                            |

## [04]-[INPACKAGE_OWNERS]

[INPACKAGE_OWNER_SCOPE]: the four `__all__`-exported submodule owners
- rail: pdf / document

`ocrmypdf.__all__` exports four in-package owners alongside `ocr` — `pdfinfo`, `pdfa`, `hocrtransform`, `helpers` — so the campaign reaches OCR-adjacent capability (pre-OCR layout analysis, standalone PDF/A conversion, hOCR parsing, PDF validation) WITHOUT shelling the CLI or re-importing Tesseract. These are the depth the rail composes when `ocr` alone is too coarse: `document/lens#LENS` reads `pdfinfo.PdfInfo` to decide whether a page even needs OCR (skip a page that already `has_text`), the print-production telos reaches `pdfa.speculative_pdfa_conversion`/`file_claims_pdfa` as the one Ghostscript PDF/A egress feeding the veraPDF preflight oracle, and the interpose path parses the emitted hOCR with `hocrtransform.HocrParser` before grafting.

[OWNER]: `pdfinfo` — layout/structure analyzer. `PdfInfo(infile: Path, *, detailed_analysis=False, progbar=False, max_workers=None, use_threads=True, check_pages=None, executor=SerialExecutor())` opens and analyzes a PDF as the run's layout-analysis owner, sharing the pipeline's `Executor`/`use_threads`/`max_workers` knobs so an already-analyzed document hands its facts forward; `PdfInfo.pages -> Sequence[PageInfo]` is the per-page sequence the rows below read.

| [INDEX] | [MEMBERS]                                                                    | [ROLE]                                                |
| :-----: | :--------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `PdfInfo.is_tagged`·`.has_structure_tree`·`.has_userunit`·`.needs_rendering` | `needs_rendering`/`is_tagged` gate OCR + tagged-PDF   |
|  [02]   | `PdfInfo.has_acroform`·`.has_signature`·`.min_version`·`.filename`           | flag a form/signed PDF; version floor + source path   |
|  [03]   | `PageInfo.has_text`·`.has_corrupt_text`                                      | `has_text` skips OCR; `has_corrupt_text` flags a redo |
|  [04]   | `PageInfo.has_vector`·`.images`·`.get_textareas()`                           | vector-vs-scan split, image + text-area inventory     |
|  [05]   | `PageInfo.dpi`·`.page_dpi_profile`                                           | size the rasterize DPI                                |
|  [06]   | `PageInfo.mediabox`/`.cropbox`/`.bleedbox`/`.trimbox`/`.artbox`              | the media/crop/bleed/trim/art box family              |
|  [07]   | `PageInfo.width_inches`/`.height_inches`·`.rotation`                         | physical page dimensions + `rotation`                 |
|  [08]   | `Colorspace`·`Encoding`·`Ink`·`FloatRect`                                    | the value vocabulary `PageInfo` returns               |

[OWNER]: `pdfa` — PDF/A generation + claim-check. `speculative_pdfa_conversion(input_file: Path, output_file: Path, output_type: str) -> Path` is the one Ghostscript PDF/A egress on an already-OCR'd PDF, never a hand-stitched `gs` invocation; `file_claims_pdfa` returns the XMP conformance claim `{'pass': True, 'output': 'pdfa', 'conformance': 'PDF/A-2b'}`, the cheap gate before re-converting, paired with veraPDF for actual validation.

| [INDEX] | [SIGNATURE]                                                        | [ROLE]                                                           |
| :-----: | :----------------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `file_claims_pdfa(filename: Path) -> dict[str, str \| bool]`       | XMP conformance claim-check (keys `pass`/`output`/`conformance`) |
|  [02]   | `add_pdfa_metadata(pdf: Pdf, part: str, conformance: str) -> None` | stamp XMP PDF/A metadata onto a `pikepdf.Pdf`                    |
|  [03]   | `add_srgb_output_intent(pdf: Pdf) -> None`                         | add the sRGB OutputIntent                                        |
|  [04]   | `generate_pdfa_ps(target_filename: Path, icc='sRGB') -> Path`      | build the Ghostscript PostScript prologue                        |
|  [05]   | `SRGB_ICC_PROFILE_NAME`                                            | sRGB profile-name constant; the `graphic/color/managed` ICC seam |

[OWNER]: `hocrtransform` — hOCR tree parser. The interpose path parses the emitted hOCR into the `OcrElement` tree and mutates it between `run_hocr_pipeline` and `run_hocr_to_ocr_pdf_pipeline` before grafting.

| [INDEX] | [SIGNATURE]                                                                   | [ROLE]                                                |
| :-----: | :---------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `HocrParser(hocr_file: str \| Path)`·`HocrParser.parse(...)`·`HocrParseError` | parse an emitted hOCR file into the `OcrElement` tree |
|  [02]   | re-exports `OcrElement`/`BoundingBox`/`Baseline`/`FontInfo`/`OcrClass`        | the hOCR node algebra, co-located with the parser     |

[OWNER]: `helpers` — boundary utilities. Pipeline-adjacent validation and value objects the campaign reaches without re-importing the pipeline.

| [INDEX] | [SIGNATURE]                           | [ROLE]                                         |
| :-----: | :------------------------------------ | :--------------------------------------------- |
|  [01]   | `check_pdf(input_file: Path) -> bool` | validate a PDF is openable before the pipeline |
|  [02]   | `Resolution(x, y)`                    | the DPI value object                           |
|  [03]   | `available_cpu_count() -> int`        | sizes the `jobs` default                       |
|  [04]   | `is_file_writable(...)`               | output-path writability check                  |
|  [05]   | `IMG2PDF_KWARGS`                      | the `img2pdf` conversion-kwargs constant       |

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
