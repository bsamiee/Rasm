# [PY_ARTIFACTS_API_OCRMYPDF]

`ocrmypdf` supplies the OCR-to-PDF/A pipeline surface for the artifacts LENS_OCR_ARM rail: a single `ocr` entrypoint that rasterizes an input PDF or image, runs Tesseract page-by-page, grafts the recognized text layer back over the original raster, and emits a searchable PDF or a validated PDF/A. The package owner composes `ocr`, `OcrOptions`, `ExitCode`, `Verbosity`, and the `ExitCodeException` rail into the LENS_OCR_ARM path; for a campaign that needs to interpose between OCR and grafting, the lower-level `ocrmypdf.api` two-phase pipeline (`run_hocr_pipeline` then `run_hocr_to_ocr_pdf_pipeline`) and the `run_pipeline(options, *, plugin_manager)` entry are available. It removes any hand-rolled rasterize-OCR-graft loop because the pipeline, Ghostscript PDF/A conversion, hOCR transform, and pdfinfo layout analysis are all in-package, and it never re-implements the Tesseract subprocess orchestration or PDF/A color-conversion strategy ocrmypdf already owns. Integration: `ocr` accepts a pre-built pydantic `OcrOptions` model as its positional, so the campaign validates its OCR configuration as a typed pydantic model and hands it straight in; alternate OCR backends and concurrency wire through the `pluggy`-based `OcrEngine`/`Executor` plugin contract (`hookimpl` marker), not a fork of the pipeline.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ocrmypdf`
- package: `ocrmypdf`
- import: `ocrmypdf`
- owner: `artifacts`
- rail: ocr
- installed: `17.7.0` reflected via isolated `uv pip` install on cp313 + reflection (assay finds no source in the project venv; cp315 build blocked only by `pi-heif` -> `libheif`)
- license: MPL-2.0 (pikepdf dep also MPL-2.0; pi-heif BSD-3-Clause; pdfminer.six MIT)
- abi: pure Python package, but a hard runtime floor on native executables (Tesseract, Ghostscript, jbig2enc, unpaper, pngquant) it orchestrates as subprocesses, plus `pikepdf`/`pdfminer.six`/`img2pdf`/`Pillow` Python deps. `pi-heif` (BSD-3-Clause) is a hard dependency that needs `libheif` headers to build from source; cp315 has no prebuilt `pi-heif` wheel, so the cp315 install is blocked on Forge `libheif` — cp313 installs from a prebuilt wheel.
- deps: `pikepdf` (PDF model over QPDF), `pdfminer.six` (text extraction), `img2pdf` (image->PDF), `Pillow`+`pi-heif` (raster IO/HEIF), `pluggy` (plugin manager), `pydantic` (`OcrOptions` model), `rich` (progress)
- entry points: console script `ocrmypdf` (CLI); library use is import-only via `ocrmypdf.ocr()` or the `ocrmypdf.api` lower-level functions
- capability: OCR-to-PDF/A conversion of a single PDF or image, Tesseract page OCR with language/PSM/OEM/threshold/timeout control, processing-mode control (default/skip/redo/force), deskew/clean/rotate/oversample/remove-background/remove-vectors image preprocessing, optimization levels with per-format quality knobs, PDF/A output-type and color-conversion control, document-metadata stamping, sidecar text emission, tagged-PDF policy, plugin-driven OCR/render extension, and a typed `ExitCode`/`ExitCodeException` failure rail

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pipeline result, context, and failure roots
- rail: ocr

`ocr` returns an `ExitCode` (an `IntEnum`); failures inside the pipeline raise `ExitCodeException` subclasses, each carrying the matching `ExitCode` as its `exit_code` attribute. `OcrOptions` is the pydantic v2 model the resolved options live in — `ocr` accepts a pre-built one as its positional. `PdfContext`/`PageContext` hold the per-run and per-page pipeline state; `Executor`/`OcrEngine` are abstract (`ABC`) extension points for concurrency and alternate OCR backends, registered through the `pluggy` plugin manager via the `hookimpl` marker. The `OcrElement` family (`BoundingBox`, `Baseline`, `FontInfo`, `OcrClass`, `OrientationConfidence`) models structured OCR output as an hOCR tree. The exception family is a small inheritance lattice: most failures derive directly from `ExitCodeException`, but `ColorConversionNeededError <- BadArgsError`, and `DigitalSignatureError`/`TaggedPDFError <- InputFileError`.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]    | [RAIL]                                                            |
| :-----: | :---------------------------- | :--------------- | :--------------------------------------------------------------- |
|  [01]   | `ExitCode`                    | result enum      | `IntEnum` exit codes (`ok=0`, `bad_args=1`, `input_file=2`, `missing_dependency=3`, `invalid_output_pdf=4`, `file_access_error=5`, `already_done_ocr=6`, `child_process_error=7`, `encrypted_pdf=8`, `invalid_config=9`, `pdfa_conversion_failed=10`, `other_error=15`, `ctrl_c=130`) |
|  [02]   | `Verbosity`                   | logging enum     | `IntEnum` verbosity (`quiet=-1`, `default=0`, `debug=1`, `debug_all=2`) |
|  [03]   | `TaggedPdfMode`               | policy enum      | `StrEnum` tagged-PDF behavior (`default`/`ignore`)               |
|  [04]   | `OcrOptions`                  | options model    | pydantic v2 `BaseModel` of the resolved run options; accepted as the `ocr` positional |
|  [05]   | `PdfContext`                  | context          | per-run pipeline state (`options`, `work_folder`, `origin`, `pdfinfo`, `plugin_manager`); `get_page_contexts`/`get_path` |
|  [06]   | `PageContext`                 | context          | per-page pickle-able pipeline state                              |
|  [07]   | `Executor`                    | abstract service | `ABC` concurrent task executor extension point (`pbar_class`, `pool_lock`) |
|  [08]   | `OcrEngine`                   | abstract service | `ABC` OCR-engine plugin contract (`generate_hocr`/`generate_ocr`/`generate_pdf`/`get_orientation`/`get_deskew`/`languages`/`version`) |
|  [09]   | `OcrElement`                  | model            | hOCR tree node (`bbox`/`baseline`/`font`/`confidence`/`text`/`lines`/`words`/`paragraphs`; `find_by_class`/`iter_by_class`/`get_text_recursive`) |
|  [10]   | `BoundingBox`                 | model            | axis-aligned pixel bounding box (`width`/`height`)               |
|  [11]   | `Baseline`                    | model            | text baseline `y = slope*x + intercept`                          |
|  [12]   | `FontInfo`                    | model            | font `name`/`size` + `bold`/`italic`/`serif`/`monospace`/`underline`/`smallcaps` style |
|  [13]   | `OcrClass`                    | constants        | hOCR element-class constants (`PAGE`/`CAREA`/`PARAGRAPH`/`LINE`/`WORD`/`CHAR`/`HEADER`/`FOOTER`/`CAPTION`/`TEXTFLOAT`) |
|  [14]   | `OrientationConfidence`       | model            | `NamedTuple(angle, confidence)` page rotation + confidence       |
|  [15]   | `ExitCodeException`           | error root       | base failure carrying `exit_code` (default `other_error=15`) + `message` |
|  [16]   | `BadArgsError`                | error            | invalid argument combination (`exit_code=1`)                     |
|  [17]   | `ColorConversionNeededError`  | error            | `<- BadArgsError`; color-conversion strategy required (`exit_code=1`) |
|  [18]   | `InputFileError`              | error            | bad input file (`exit_code=2`)                                   |
|  [19]   | `DpiError`                    | error            | image DPI missing or invalid (`exit_code=2`)                     |
|  [20]   | `UnsupportedImageFormatError` | error            | input image format unsupported (`exit_code=2`)                   |
|  [21]   | `DigitalSignatureError`       | error            | `<- InputFileError`; input has a digital signature (`exit_code=2`) |
|  [22]   | `TaggedPDFError`              | error            | `<- InputFileError`; input is a tagged PDF (`exit_code=2`)       |
|  [23]   | `MissingDependencyError`      | error            | third-party executable missing (`exit_code=3`)                   |
|  [24]   | `OutputFileAccessError`       | error            | output path inaccessible (`exit_code=5`)                         |
|  [25]   | `PriorOcrFoundError`          | error            | file already has OCR (`exit_code=6`)                             |
|  [26]   | `SubprocessOutputError`       | error            | a child subprocess failed (`exit_code=7`)                        |
|  [27]   | `EncryptedPdfError`           | error            | input PDF is encrypted (`exit_code=8`)                           |
|  [28]   | `TesseractConfigError`        | error            | invalid Tesseract configuration (`exit_code=9`)                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pipeline run
- rail: ocr

`ocr` is the single library entrypoint. Positional `input_file_or_options` accepts a path/IO or a pre-built `OcrOptions`; when a path is given, `output_file` is required. All processing knobs are keyword-only `| None` rows that default to the CLI defaults when omitted. `mode` (`'default'`/`'skip'`/`'redo'`/`'force'`) supersedes the legacy `force_ocr`/`skip_text`/`redo_ocr` booleans. The call returns an `ExitCode`; failures raise `ExitCodeException` subclasses.

| [INDEX] | [SURFACE] | [CALL_SHAPE]                                                                                                                                                                                                             | [CAPABILITY]                                      |
| :-----: | :-------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `ocr`     | `ocr(input_file_or_options, output_file=None, *, language=None, image_dpi=None, output_type=None, sidecar=None, jobs=None, use_threads=None, mode=None, force_ocr=None, skip_text=None, redo_ocr=None, ...) -> ExitCode` | run the OCR-to-PDF/A pipeline on one PDF or image |

[ENTRYPOINT_SCOPE]: `ocr` keyword axes
- rail: ocr

The keyword rows below are the campaign-consumed subset of the reflected `ocr` signature; every row is a keyword-only parameter typed `| None` (default applied when omitted). The full signature carries ~55 keyword-only params; the rows omitted here are jbig2/quality micro-tuning (`jpg_quality`/`png_quality`/`jbig2_lossy`/`jbig2_page_group_size`/`jbig2_threshold`), temp-file controls (`keep_temporary_files`), and `use_threads`/`no_overwrite`/`continue_on_soft_render_error`/`unpaper_args`/`max_image_mpixels`/`tesseract_downsample_*`/`rotate_pages_threshold`.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                    | [CAPABILITY]                                                  |
| :-----: | :--------------------------------- | :------------------------------ | :----------------------------------------------------------- |
|  [01]   | `ocr(language=...)`                | `Iterable[str] \| None`         | Tesseract language pack selection (e.g. `['eng']`)           |
|  [02]   | `ocr(output_type=...)`             | `str \| None`                   | output target `auto`/`pdfa`/`pdf`/`pdfa-1`/`pdfa-2`/`pdfa-3`/`none` |
|  [03]   | `ocr(mode=...)`                    | `str \| None`                   | processing mode `default`/`skip`/`redo`/`force`              |
|  [04]   | `ocr(image_dpi=...)`               | `int \| None`                   | DPI for image input lacking embedded resolution              |
|  [05]   | `ocr(sidecar=...)`                 | `PathOrIO \| None`              | write recognized text to a sidecar file                      |
|  [06]   | `ocr(rotate_pages=...)`            | `bool \| None`                  | auto-rotate pages by OCR orientation detection               |
|  [07]   | `ocr(deskew=...)`                  | `bool \| None`                  | deskew pages before OCR                                      |
|  [08]   | `ocr(clean=...)` / `ocr(clean_final=...)` | `bool \| None`           | clean with unpaper before OCR / clean the final output too   |
|  [09]   | `ocr(remove_background=...)`       | `bool \| None`                  | flatten page background before OCR                           |
|  [10]   | `ocr(remove_vectors=...)`          | `bool \| None`                  | drop vector graphics before rasterize                        |
|  [11]   | `ocr(oversample=...)`              | `int \| None`                   | rasterize at a higher DPI before OCR                         |
|  [12]   | `ocr(optimize=...)`                | `int \| None`                   | optimization level (0-3)                                     |
|  [13]   | `ocr(color_conversion_strategy=...)` | `str \| None`                 | Ghostscript PDF/A color-conversion strategy (e.g. `RGB`/`UseDeviceIndependentColor`) |
|  [14]   | `ocr(pdfa_image_compression=...)`  | `str \| None`                   | PDF/A image codec selection (`auto`/`jpeg`/`lossless`)       |
|  [15]   | `ocr(tagged_pdf_mode=...)`         | `TaggedPdfMode \| str \| None`  | tagged-PDF handling (`default`/`ignore`)                     |
|  [16]   | `ocr(tesseract_config=...)`        | `Iterable[str] \| None`         | extra Tesseract config flags                                 |
|  [17]   | `ocr(tesseract_pagesegmode=...)`   | `int \| None`                   | Tesseract `--psm` page-segmentation mode                     |
|  [18]   | `ocr(tesseract_oem=...)`           | `int \| None`                   | Tesseract `--oem` OCR-engine mode                            |
|  [19]   | `ocr(tesseract_thresholding=...)`  | `str \| None`                   | Tesseract thresholding method (`auto`/`sauvola`/`adaptive-otsu`) |
|  [20]   | `ocr(tesseract_timeout=...)`       | `float \| None`                 | per-page Tesseract timeout in seconds                        |
|  [21]   | `ocr(user_words=...)` / `ocr(user_patterns=...)` | `Path \| None`    | Tesseract user word list / pattern file                      |
|  [22]   | `ocr(pdf_renderer=...)`            | `str \| None`                   | text-layer renderer `auto`/`hocr`/`sandwich`/`fpdf2`         |
|  [23]   | `ocr(title=, author=, subject=, keywords=...)` | `str \| None`       | stamp output PDF document-info metadata                      |
|  [24]   | `ocr(fast_web_view=...)`           | `float \| None`                 | linearize for fast web view above this size (MB)             |
|  [25]   | `ocr(invalidate_digital_signatures=...)` | `bool \| None`            | proceed on a signed PDF, invalidating the signature          |
|  [26]   | `ocr(skip_big=...)`                | `float \| None`                 | skip OCR on pages larger than this megapixel threshold       |
|  [27]   | `ocr(jobs=...)`                    | `int \| None`                   | worker count for parallel page processing                    |
|  [28]   | `ocr(pages=...)`                   | `str \| None`                   | page range to OCR                                            |
|  [29]   | `ocr(plugins=...)`                 | `Iterable[Path \| str] \| None` | load OCR/render plugins by import path                       |
|  [30]   | `ocr(plugin_manager=...)`          | `OcrmypdfPluginManager \| None` | inject a pre-built plugin manager instead of a plugin list   |
|  [31]   | `ocr(progress_bar=...)`            | `bool \| None`                  | toggle the progress bar                                      |

[ENTRYPOINT_SCOPE]: logging configuration
- rail: ocr

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                                                                                 | [CAPABILITY]                                         |
| :-----: | :------------------------ | :----------------------------------------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `configure_logging`       | `configure_logging(verbosity: Verbosity, *, progress_bar_friendly=True, manage_root_logger=False, plugin_manager=None)` | install ocrmypdf-style log handlers under `ocrmypdf` |
|  [02]   | `configure_debug_logging` | `configure_debug_logging(log_filename, prefix='ocrmypdf')`                                                   | attach the debug-log file handler                    |

[ENTRYPOINT_SCOPE]: lower-level `ocrmypdf.api` pipeline and plugin manager
- rail: ocr

`ocr()` is a thin wrapper that builds an `OcrOptions` (via `create_options`), resolves a plugin manager (`get_plugin_manager`), and calls `run_pipeline`. A campaign that needs to interpose between OCR and grafting (custom hOCR post-processing, alternate text-layer source) uses the two-phase split: `run_hocr_pipeline` rasterizes and OCRs to hOCR/text only (returns `None`), then `run_hocr_to_ocr_pdf_pipeline` grafts the (possibly mutated) hOCR back into a PDF/A and returns the `ExitCode`. All `api` functions take a resolved `OcrOptions` plus a keyword-only `plugin_manager`.

| [INDEX] | [SURFACE]                                    | [CALL_SHAPE]                                                                          | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------- | :------------------------------------------------------------------------------------ | :----------------------------------------------------------- |
|  [01]   | `api.create_options`                         | `create_options(*, input_file, output_file, parser, **kwargs) -> OcrOptions`          | build and validate the `OcrOptions` model from kwargs        |
|  [02]   | `api.get_plugin_manager`                     | `get_plugin_manager(plugins=None, builtins=True) -> OcrmypdfPluginManager`            | construct the pluggy manager with builtin + named plugins    |
|  [03]   | `api.check_options`                          | `check_options(options, plugin_manager) -> None`                                      | validate option combinations against the plugin set          |
|  [04]   | `api.run_pipeline`                           | `run_pipeline(options, *, plugin_manager) -> ExitCode`                                 | run the full pipeline from a resolved `OcrOptions`           |
|  [05]   | `api.run_hocr_pipeline`                      | `run_hocr_pipeline(options, *, plugin_manager) -> None`                               | phase 1: rasterize + OCR to hOCR/text only                   |
|  [06]   | `api.run_hocr_to_ocr_pdf_pipeline`           | `run_hocr_to_ocr_pdf_pipeline(options, *, plugin_manager) -> ExitCode`                | phase 2: graft hOCR into the searchable PDF/A                |
|  [07]   | `hookimpl`                                   | `@hookimpl` (pluggy `HookimplMarker('ocrmypdf')`)                                      | decorate an `OcrEngine`/`Executor`/render plugin hook        |

## [04]-[IMPLEMENTATION_LAW]

[OCR_PIPELINE]:
- import: `import ocrmypdf` at boundary scope only; module-level import is banned by the manifest import policy.
- entry axis: one `ocr` owns the full rasterize-OCR-graft-PDF/A pipeline; `language`/`output_type`/`mode`/`optimize`/`deskew`/`clean`/`rotate_pages`/`color_conversion_strategy`/`tesseract_*` are keyword rows on that call, never a per-config builder type or a `run_force`/`run_skip`/`run_redo` family — `mode` discriminates the processing behavior. A campaign holding a validated `OcrOptions` pydantic model passes it as the positional instead of unpacking kwargs; a campaign needing to interpose between OCR and graft uses the `api.run_hocr_pipeline` -> `api.run_hocr_to_ocr_pdf_pipeline` split rather than two separate `ocr` calls.
- result axis: `ocr` returns `ExitCode` (`ok=0`); the campaign maps the `IntEnum` to its success/failure rail and never re-derives exit semantics from string parsing. `output_type='none'` runs OCR for the sidecar text only and writes no PDF.
- failure axis: pipeline faults raise `ExitCodeException` subclasses, each binding its `ExitCode` (`MissingDependencyError`->`missing_dependency=3`, `InputFileError`->`input_file=2`, `EncryptedPdfError`->`encrypted_pdf=8`, `PriorOcrFoundError`->`already_done_ocr=6`, `SubprocessOutputError`->`child_process_error=7`, `TesseractConfigError`->`invalid_config=9`); the lattice nests `ColorConversionNeededError <- BadArgsError` and `DigitalSignatureError`/`TaggedPDFError <- InputFileError`, so catching the parent catches the refinement; PDF/A validation failure surfaces as `pdfa_conversion_failed=10`. The boundary catches `ExitCodeException` and reads `exit_code`/`message`, never catches bare `Exception`.
- output axis: `output_type` selects `auto`/`pdf`/`pdfa`/`pdfa-1`/`pdfa-2`/`pdfa-3`/`none` as a call row; PDF/A conversion is delegated to the in-package Ghostscript path with a `color_conversion_strategy` and `pdfa_image_compression` row, never a hand-stitched external `gs` invocation.
- input axis: HEIF input is admitted only through `pi-heif` registering its Pillow opener at the boundary, pending `libheif` on Forge (which also unblocks the cp315 `pi-heif` build); the floor for LENS_OCR_ARM is core OCR/PDF-A on PDF and standard raster formats, which is intact without HEIF.
- plugin axis: alternate OCR backends, executors, and renderers extend through the `pluggy` plugin manager (`api.get_plugin_manager`, `ocr(plugin_manager=...)`) implementing the `OcrEngine`/`Executor` `ABC` contract with `@hookimpl`-decorated hooks, never a fork of the pipeline; `OcrEngine` requires `generate_hocr`/`generate_ocr`/`generate_pdf`/`get_orientation`/`languages`/`version`.
- logging axis: `configure_logging(Verbosity.x)` installs ocrmypdf-style handlers only when the application wants CLI-equivalent output; an application managing its own logging skips it, and ocrmypdf logs under the `ocrmypdf` namespace regardless.
- evidence: each run captures the returned `ExitCode`, the resolved `output_type`, language set, processing `mode`, page count from `PdfContext.pdfinfo`, sidecar emission, and any raised `ExitCodeException` `exit_code`/`message` as an OCR receipt.
- boundary: ocrmypdf owns OCR, text-layer grafting, and PDF/A conversion of one document per call; Tesseract, Ghostscript, unpaper, jbig2enc, and pngquant are external executables the package orchestrates; concurrent execution and alternate OCR engines extend through the `Executor`/`OcrEngine` abstractions, never a parallel pipeline; live UI and multi-document batching stay outside this package.

[RAIL_LAW]:
- Package: `ocrmypdf`
- Owns: OCR-to-PDF/A conversion of a single PDF or image, Tesseract page OCR with language/PSM/OEM/mode control, image preprocessing, PDF/A output-type and color-conversion control, document-metadata stamping, sidecar text emission, plugin-driven extension, and a typed `ExitCode`/`ExitCodeException` rail
- Accept: single-document OCR-to-searchable-PDF/A runs feeding the artifacts LENS_OCR_ARM path (from `ocr` kwargs or a pre-built `OcrOptions`); the `api.run_hocr_*` two-phase split when interposing between OCR and graft; HEIF input via the `pi-heif` boundary opener
- Reject: wrapper-renames of `ocr`; a hand-rolled rasterize-OCR-graft loop or external `tesseract`/`gs` orchestration the package owns; a `run_force`/`run_skip`/`run_redo` entrypoint family where `mode` is a call row; bare-`Exception` handling that discards the `ExitCode` rail or flattens the `ExitCodeException` lattice; a parallel pipeline or forked OCR loop bypassing the `Executor`/`OcrEngine` plugin extension points
