# [PY_ARTIFACTS_API_OCRMYPDF]

`ocrmypdf` supplies the OCR-to-PDF/A pipeline surface for the artifacts LENS_OCR_ARM rail: a single `ocr` entrypoint that rasterizes an input PDF or image, runs Tesseract page-by-page, grafts the recognized text layer back over the original raster, and emits a searchable PDF or a validated PDF/A. The package owner composes `ocr`, `ExitCode`, `Verbosity`, and the `ExitCodeException` rail into the LENS_OCR_ARM path; it removes any hand-rolled rasterize-OCR-graft loop because the pipeline, Ghostscript PDF/A conversion, hOCR transform, and pdfinfo layout analysis are all in-package, and it never re-implements the Tesseract subprocess orchestration or PDF/A color-conversion strategy ocrmypdf already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ocrmypdf`
- package: `ocrmypdf`
- import: `ocrmypdf`
- owner: `artifacts`
- rail: ocr
- installed: `17.7.0` reflected via `assay api` on cp315
- entry points: console script `ocrmypdf` (CLI); library use is import-only via `ocrmypdf.ocr()`
- capability: OCR-to-PDF/A conversion of a single PDF or image, Tesseract page OCR with language selection, processing-mode control (default/skip/redo/force), deskew/clean/rotate image preprocessing, optimization levels, PDF/A output-type and color-conversion control, sidecar text emission, and a typed `ExitCode`/`ExitCodeException` failure rail

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pipeline result, context, and failure roots
- rail: ocr

`ocr` returns an `ExitCode` (an `IntEnum`); failures inside the pipeline raise `ExitCodeException` subclasses, each carrying the matching `ExitCode`. `PdfContext`/`PageContext` hold the per-run and per-page pipeline state; `Executor`/`OcrEngine` are abstract extension points for concurrency and alternate OCR backends. The `OcrElement` family (`BoundingBox`, `Baseline`, `FontInfo`, `OcrClass`, `OrientationConfidence`) models structured OCR output.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]    | [RAIL]                                                 |
| :-----: | :---------------------------- | :--------------- | :----------------------------------------------------- |
|  [01]   | `ExitCode`                    | result enum      | `IntEnum` pipeline exit codes (`ok=0` .. `ctrl_c=130`) |
|  [02]   | `Verbosity`                   | logging enum     | `IntEnum` verbosity (`quiet=-1` .. `debug_all=2`)      |
|  [03]   | `TaggedPdfMode`               | policy enum      | `StrEnum` tagged-PDF behavior (`default`/`ignore`)     |
|  [04]   | `PdfContext`                  | context          | per-run pipeline state (options, origin, pdfinfo)      |
|  [05]   | `PageContext`                 | context          | per-page pickle-able pipeline state                    |
|  [06]   | `Executor`                    | abstract service | concurrent task executor extension point               |
|  [07]   | `OcrEngine`                   | abstract service | OCR-engine plugin extension point                      |
|  [08]   | `OcrElement`                  | model            | tree node of structured OCR output (hOCR classes)      |
|  [09]   | `BoundingBox`                 | model            | axis-aligned pixel bounding box                        |
|  [10]   | `Baseline`                    | model            | text baseline `y = slope*x + intercept`                |
|  [11]   | `FontInfo`                    | model            | font family/size/style for rendered text               |
|  [12]   | `OcrClass`                    | constants        | hOCR element-class string constants                    |
|  [13]   | `OrientationConfidence`       | model            | `NamedTuple` page rotation angle + confidence          |
|  [14]   | `ExitCodeException`           | error root       | base failure carrying `exit_code` + `message`          |
|  [15]   | `MissingDependencyError`      | error            | third-party dependency missing (`exit_code=3`)         |
|  [16]   | `InputFileError`              | error            | bad input file (`exit_code=2`)                         |
|  [17]   | `OutputFileAccessError`       | error            | output path inaccessible (`exit_code=5`)               |
|  [18]   | `PriorOcrFoundError`          | error            | file already has OCR (`exit_code=6`)                   |
|  [19]   | `EncryptedPdfError`           | error            | input PDF is encrypted (`exit_code=8`)                 |
|  [20]   | `DpiError`                    | error            | image DPI is missing or invalid                        |
|  [21]   | `BadArgsError`                | error            | invalid argument combination (`exit_code=1`)           |
|  [22]   | `SubprocessOutputError`       | error            | a child subprocess failed (`exit_code=7`)              |
|  [23]   | `TesseractConfigError`        | error            | invalid Tesseract configuration                        |
|  [24]   | `UnsupportedImageFormatError` | error            | input image format is unsupported                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pipeline run
- rail: ocr

`ocr` is the single library entrypoint. Positional `input_file_or_options` accepts a path/IO or a pre-built `OcrOptions`; when a path is given, `output_file` is required. All processing knobs are keyword-only `| None` rows that default to the CLI defaults when omitted. `mode` (`'default'`/`'skip'`/`'redo'`/`'force'`) supersedes the legacy `force_ocr`/`skip_text`/`redo_ocr` booleans. The call returns an `ExitCode`; failures raise `ExitCodeException` subclasses.

| [INDEX] | [SURFACE] | [CALL_SHAPE]                                                                                                                                                                                                             | [CAPABILITY]                                      |
| :-----: | :-------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `ocr`     | `ocr(input_file_or_options, output_file=None, *, language=None, image_dpi=None, output_type=None, sidecar=None, jobs=None, use_threads=None, mode=None, force_ocr=None, skip_text=None, redo_ocr=None, ...) -> ExitCode` | run the OCR-to-PDF/A pipeline on one PDF or image |

[ENTRYPOINT_SCOPE]: `ocr` keyword axes
- rail: ocr

The keyword rows below are the campaign-consumed subset of the reflected `ocr` signature; every row is a keyword-only parameter typed `| None` (default applied when omitted).

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                    | [CAPABILITY]                                       |
| :-----: | :---------------------- | :------------------------------ | :------------------------------------------------- |
|  [01]   | `ocr(language=...)`     | `Iterable[str] \| None`         | Tesseract language pack selection (e.g. `['eng']`) |
|  [02]   | `ocr(output_type=...)`  | `str \| None`                   | output target (`pdfa`/`pdf`/`pdfa-1..3`/`auto`)    |
|  [03]   | `ocr(mode=...)`         | `str \| None`                   | processing mode `default`/`skip`/`redo`/`force`    |
|  [04]   | `ocr(image_dpi=...)`    | `int \| None`                   | DPI for image input lacking embedded resolution    |
|  [05]   | `ocr(sidecar=...)`      | `PathOrIO \| None`              | write recognized text to a sidecar file            |
|  [06]   | `ocr(rotate_pages=...)` | `bool \| None`                  | auto-rotate pages by OCR orientation detection     |
|  [07]   | `ocr(deskew=...)`       | `bool \| None`                  | deskew pages before OCR                            |
|  [08]   | `ocr(clean=...)`        | `bool \| None`                  | clean pages with unpaper before OCR                |
|  [09]   | `ocr(optimize=...)`     | `int \| None`                   | optimization level (0-3)                           |
|  [10]   | `ocr(jobs=...)`         | `int \| None`                   | worker count for parallel page processing          |
|  [11]   | `ocr(pages=...)`        | `str \| None`                   | page range to OCR                                  |
|  [12]   | `ocr(plugins=...)`      | `Iterable[Path \| str] \| None` | load OCR/render plugins by import path             |
|  [13]   | `ocr(progress_bar=...)` | `bool \| None`                  | toggle the progress bar                            |

[ENTRYPOINT_SCOPE]: logging configuration
- rail: ocr

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                                                                                 | [CAPABILITY]                                         |
| :-----: | :------------------------ | :----------------------------------------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `configure_logging`       | `configure_logging(verbosity, *, progress_bar_friendly=True, manage_root_logger=False, plugin_manager=None)` | install ocrmypdf-style log handlers under `ocrmypdf` |
|  [02]   | `configure_debug_logging` | `configure_debug_logging(...)`                                                                               | attach the debug-log file handler                    |

## [04]-[IMPLEMENTATION_LAW]

[OCR_PIPELINE]:
- import: `import ocrmypdf` at boundary scope only; module-level import is banned by the manifest import policy.
- entry axis: one `ocr` owns the full rasterize-OCR-graft-PDF/A pipeline; `language`/`output_type`/`mode`/`optimize`/`deskew`/`clean`/`rotate_pages` are keyword rows on that call, never a per-config builder type or a `run_force`/`run_skip`/`run_redo` family — `mode` discriminates the processing behavior.
- result axis: `ocr` returns `ExitCode` (`ok=0`); the campaign maps the `IntEnum` to its success/failure rail and never re-derives exit semantics from string parsing.
- failure axis: pipeline faults raise `ExitCodeException` subclasses, each binding its `ExitCode` (`MissingDependencyError`->`missing_dependency=3`, `InputFileError`->`input_file=2`, `EncryptedPdfError`->`encrypted_pdf=8`, `PriorOcrFoundError`->`already_done_ocr=6`); the boundary catches `ExitCodeException` and reads `exit_code`/`message`, never catches bare `Exception`.
- output axis: `output_type` selects the PDF vs PDF/A-1/2/3 target as a call row; PDF/A conversion is delegated to the in-package Ghostscript path with a chosen color-conversion strategy, never a hand-stitched external `gs` invocation.
- input axis: HEIF input is admitted only through `pi-heif` registering its Pillow opener at the boundary, pending `libheif` on Forge; the floor for LENS_OCR_ARM is core OCR/PDF-A on PDF and standard raster formats, which is intact without HEIF.
- logging axis: `configure_logging(Verbosity.x)` installs ocrmypdf-style handlers only when the application wants CLI-equivalent output; an application managing its own logging skips it, and ocrmypdf logs under the `ocrmypdf` namespace regardless.
- evidence: each run captures the returned `ExitCode`, the resolved `output_type`, language set, processing `mode`, page count from `PdfContext.pdfinfo`, sidecar emission, and any raised `ExitCodeException` `exit_code`/`message` as an OCR receipt.
- boundary: ocrmypdf owns OCR, text-layer grafting, and PDF/A conversion of one document per call; Tesseract, Ghostscript, unpaper, and jbig2enc are external executables the package orchestrates; concurrent execution and alternate OCR engines extend through the `Executor`/`OcrEngine` abstractions, never a parallel pipeline; live UI and multi-document batching stay outside this package.

[RAIL_LAW]:
- Package: `ocrmypdf`
- Owns: OCR-to-PDF/A conversion of a single PDF or image, Tesseract page OCR with language/mode control, image preprocessing, PDF/A output-type and color-conversion control, sidecar text emission, and a typed `ExitCode`/`ExitCodeException` rail
- Accept: single-document OCR-to-searchable-PDF/A runs feeding the artifacts LENS_OCR_ARM path; HEIF input via the `pi-heif` boundary opener
- Reject: wrapper-renames of `ocr`; a hand-rolled rasterize-OCR-graft loop or external `tesseract`/`gs` orchestration the package owns; a `run_force`/`run_skip`/`run_redo` entrypoint family where `mode` is a call row; bare-`Exception` handling that discards the `ExitCode` rail; a parallel pipeline bypassing the `Executor`/`OcrEngine` extension points
