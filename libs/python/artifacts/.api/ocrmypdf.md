# [PY_ARTIFACTS_API_OCRMYPDF]

`ocrmypdf` owns whole-document OCR-to-PDF/A for the artifacts pdf/document rail: one `ocr` entrypoint rasterizes a PDF or image, Tesseract-OCRs each page, grafts the text layer over the raster, and emits a searchable PDF or validated PDF/A — the `document/lens#LENS` `LensProvider.OCRMYPDF` arm of the `OCR` recovery op. It orchestrates the external `tesseract`/`ghostscript`/`unpaper`/`jbig2enc`/`pngquant` executables; `ExitCode` folds onto the `expression` `Result` rail off the `anyio` `to_process` worker lane.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ocrmypdf`
- package: `ocrmypdf` (MPL-2.0)
- module: `ocrmypdf`
- rail: pdf / document (OCR)
- depends: external `PATH` executables `tesseract` `ghostscript` `unpaper` `jbig2enc` `pngquant`, orchestrated and never pip deps; `veraPDF` is the campaign's separate PDF/A validation oracle, not invoked here

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pipeline result, context, and hOCR-tree roots

`ocr` returns an `ExitCode` and raises `ExitCodeException` subclasses — the lattice below. `OcrOptions` is the pydantic v2 `BaseModel` `ocr` accepts as its positional; `PdfContext`/`PageContext` hold per-run and per-page pipeline state. `Executor` and `OcrEngine` are the `pluggy`-registered `ABC` extension points for concurrency and alternate OCR backends; `OcrElement` is the hOCR tree node family (`BoundingBox`/`Baseline`/`FontInfo`/`OcrClass`, re-exported by `hocrtransform`), `OrientationConfidence` riding the engine contract, not the tree.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]    | [CAPABILITY]                                                                            |
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

[PUBLIC_TYPE_SCOPE]: exit-code and error lattice

Every exported error derives directly from `ExitCodeException` (base `other_error=15`, carrying `message`) and binds one `ExitCode`; the lattice is flat — `DpiError`/`UnsupportedImageFormatError` share `input_file=2` with `InputFileError` without nesting, so the boundary matches on the `exit_code` int, never a subclass tree. Codes `ok=0`/`invalid_output_pdf=4`/`pdfa_conversion_failed=10`/`ctrl_c=130` bind no exported error.

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

`ocr` is the single library entrypoint over one PDF or image. Positional `input_file_or_options` takes a path/IO or a pre-built `OcrOptions`, and a path requires `output_file`. Processing knobs are keyword-only `| None` rows defaulting to the CLI defaults when omitted, and `mode` (`default`/`skip`/`redo`/`force`) discriminates the processing behavior; `ocr` returns an `ExitCode` and raises `ExitCodeException` subclasses on failure.
- call: `ocr(input_file_or_options, output_file=None, *, language=None, output_type=None, mode=None, ...) -> ExitCode`

[ENTRYPOINT_SCOPE]: `ocr` keyword axes

Rows below are the campaign-consumed subset of a broader keyword-only signature; each is keyword-only `| None` with its default applied when omitted.

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

- `tesseract_thresholding` takes the `ThresholdingMethod` int the CLI `auto`/`otsu`/`sauvola`/`adaptive-otsu` names map to; `pdf_renderer=hocrdebug` is debug-only.

[ENTRYPOINT_SCOPE]: logging and stdout configuration
- call: `configure_logging(verbosity: Verbosity, *, progress_bar_friendly=True, manage_root_logger=False, plugin_manager=None)`
- call: `configure_debug_logging(log_filename: Path, prefix='') -> tuple[FileHandler, Callable[[], None]]`
- call: `configure_stdout_protection() -> bool`

| [INDEX] | [SURFACE]                     | [CAPABILITY]                                                                     |
| :-----: | :---------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `configure_logging`           | install ocrmypdf-style log handlers under `ocrmypdf`                             |
|  [02]   | `configure_debug_logging`     | attach the debug-log file handler; returns the handler + a remover callable      |
|  [03]   | `configure_stdout_protection` | redirect fd 1 to stderr, reserve a private stdout for the PDF; returns installed |

- `configure_stdout_protection`: mutates process-global file descriptors (guards `output_file='-'` PDF-to-stdout against stray `print`), returns `False` when stdout is not a real OS descriptor and changes nothing; call once early or never.

Campaign code owns its `structlog` root and its own stdout, so it skips both `configure_logging` (which installs CLI-style handlers) and `configure_stdout_protection` (fd-level, CLI-only), letting ocrmypdf log under the `ocrmypdf` stdlib namespace; `manage_root_logger=False` keeps ocrmypdf off the root logger the rail owns.

[ENTRYPOINT_SCOPE]: lower-level `ocrmypdf.api` pipeline and plugin manager

`ocr` builds an `OcrOptions` (`create_options`), resolves a plugin manager (`get_plugin_manager`), and runs `run_pipeline`. To interpose between OCR and grafting, the two-phase split runs `run_hocr_pipeline` (rasterize and OCR to hOCR only, returns `None`) then `run_hocr_to_ocr_pdf_pipeline` (graft the mutated hOCR into a PDF/A, returns `ExitCode`); every `api.*` takes a resolved `OcrOptions` with a keyword-only `plugin_manager`, and `check_options` validates the option/plugin combination. `hookimpl` is `ocrmypdf.hookimpl`, the `pluggy` `HookimplMarker('ocrmypdf')`.

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

[INPACKAGE_OWNER_SCOPE]: the `__all__`-exported submodule owners

`ocrmypdf.__all__` exports `ocr` alongside `pdfinfo`, `pdfa`, `hocrtransform`, and `helpers`, so the campaign reaches OCR-adjacent capability — pre-OCR layout analysis, standalone PDF/A conversion, hOCR parsing, PDF validation — without shelling the CLI or re-importing Tesseract. `document/lens#LENS` reads `pdfinfo.PdfInfo` to decide whether a page needs OCR, reaches `pdfa.speculative_pdfa_conversion`/`file_claims_pdfa` for the Ghostscript PDF/A egress feeding the veraPDF preflight, and parses the emitted hOCR with `hocrtransform.HocrParser` before grafting.

[OWNER]: `pdfinfo` — layout/structure analyzer. `PdfInfo(infile: Path, *, detailed_analysis=False, progbar=False, max_workers=None, use_threads=True, check_pages=None, executor=SerialExecutor())` opens and analyzes a PDF, sharing the pipeline's `Executor`/`use_threads`/`max_workers` knobs so an already-analyzed document hands its facts forward; `PdfInfo.pages -> Sequence[PageInfo]` is the per-page sequence the rows read.

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

[OWNER]: `pdfa` — PDF/A generation + claim-check. `speculative_pdfa_conversion(input_file, output_file, output_type) -> Path` is the one Ghostscript PDF/A egress on an already-OCR'd PDF, never a hand-stitched `gs`; `file_claims_pdfa` returns the XMP conformance claim (keys `pass`/`output`/`conformance`), the cheap gate before re-converting, paired with veraPDF for actual validation.

| [INDEX] | [SIGNATURE]                                                        | [ROLE]                                                           |
| :-----: | :----------------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `file_claims_pdfa(filename: Path) -> dict[str, str \| bool]`       | XMP conformance claim-check (keys `pass`/`output`/`conformance`) |
|  [02]   | `add_pdfa_metadata(pdf: Pdf, part: str, conformance: str) -> None` | stamp XMP PDF/A metadata onto a `pikepdf.Pdf`                    |
|  [03]   | `add_srgb_output_intent(pdf: Pdf) -> None`                         | add the sRGB OutputIntent                                        |
|  [04]   | `generate_pdfa_ps(target_filename: Path, icc='sRGB') -> Path`      | build the Ghostscript PostScript prologue                        |
|  [05]   | `SRGB_ICC_PROFILE_NAME`                                            | sRGB profile-name constant; the `graphic/color/managed` ICC seam |

[OWNER]: `hocrtransform` — hOCR tree parser. Interpose parsing reads the emitted hOCR into the `OcrElement` tree and mutates it between `run_hocr_pipeline` and `run_hocr_to_ocr_pdf_pipeline` before grafting.

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

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one `ocr` owns the full rasterize-OCR-graft-PDF/A pipeline; `language`/`output_type`/`mode`/`optimize`/`deskew`/`clean`/`rotate_pages`/`color_conversion_strategy`/`tesseract_*` are keyword rows on that call, never a per-config builder or a `run_force`/`run_skip`/`run_redo` family — `mode` discriminates. `document/lens#LENS`'s `OCR` arm calls `ocrmypdf.ocr(source.name, target.name, sidecar=sidecar.name, language=spec.language, output_type='pdfa', mode='force', deskew=, clean=, rotate_pages=, optimize=, progress_bar=False)`; a holder of a validated `OcrOptions` passes it as the positional instead.
- `output_type` selects `auto`/`pdf`/`pdfa`/`pdfa-1`/`pdfa-2`/`pdfa-3`/`none` as a call row; PDF/A conversion runs the in-package Ghostscript path with `color_conversion_strategy`/`pdfa_image_compression` rows, never a hand-stitched external `gs`. `auto` targets PDF/A whenever achievable — the fast Ghostscript-free path validated by veraPDF first, Ghostscript fallback next — and emits a plain PDF preserving the existing text layer only when even Ghostscript cannot safely convert (a non-embedded CID/CJK font), where an explicit `pdfa*` raises instead. `output_type='none'` runs OCR for the sidecar text only, writing no PDF — the shape when only the recovered text feeds `DocumentNode`.
- `pdfinfo.PdfInfo(infile, detailed_analysis=)` is the cheap pre-flight before a full cycle: `PageInfo.has_text` skips a page, `has_corrupt_text` flags a redo, `is_tagged`/`has_acroform`/`has_signature` route the tagged/form/signed policy — read-only, sharing the pipeline `Executor`/`use_threads`/`max_workers`.
- pipeline faults raise `ExitCodeException` subclasses, each binding its `ExitCode`; the flat lattice means the boundary discriminates on the `exit_code` int, never a subclass tree, and PDF/A validation failure surfaces as `pdfa_conversion_failed=10`.
- alternate OCR backends, executors, and renderers extend through the `pluggy` manager (`get_plugin_manager`, `ocr(plugin_manager=...)`) implementing the `OcrEngine`/`Executor` `ABC` with `@hookimpl` hooks, never a pipeline fork; `OcrEngine`'s required `@abstractmethod` set is `creator_tag`/`generate_hocr`/`generate_pdf`/`get_orientation`/`languages`/`version`/`__str__`, with `generate_ocr`/`get_deskew`/`supports_generate_ocr` concrete defaults.
- each run's evidence — returned `ExitCode`, resolved `output_type`, language set, processing `mode`, `pdfinfo.PdfInfo.pages` page count, sidecar emission, any raised `exit_code`/`message` — is the OCR contribution to the `document/lens` `ArtifactReceipt.Introspection` case.

[STACKING]:
- `expression`(`.api/expression.md`): the consumer maps `ExitCode.ok` to `Ok` and carries every other code as the receipt's `code.name` on the `Result` rail, never re-deriving exit semantics from string parsing; a raised `ExitCodeException` converts to the runtime `BoundaryFault` at the `async_boundary` capsule, never caught as bare `Exception` nor flattened into the interior `LensFault` vocabulary.
- `anyio`(`.api/anyio.md`): OCR is GIL- and subprocess-bound, so the consumer crosses `to_process.run_sync(_gated_recover, self, limiter=_OFFLOAD)` onto the worker lane under the shared `CapacityLimiter`, never inline on the event loop; ocrmypdf's inner `jobs`/`Executor` page fan (default `helpers.available_cpu_count()`) composes beneath, never a hand-rolled `multiprocessing` pool around `ocr`.
- `structlog`(`.api/structlog.md`): ocrmypdf's `ocrmypdf`-namespace stdlib output is captured by the universal `ProcessorFormatter` and stamped onto the recovery span, no second logging stack.
- `document/lens#LENS`: its `LensProvider.OCRMYPDF` arm reads `sidecar.read()` only when `code is ExitCode.ok`; `pdfa.speculative_pdfa_conversion` is the one Ghostscript PDF/A egress feeding the print-production `graphic/color/managed` plane and the veraPDF oracle; the interpose path mutates the `hocrtransform.HocrParser` tree between `api.run_hocr_pipeline` and `api.run_hocr_to_ocr_pdf_pipeline`.
- `pikepdf`(`.api/pikepdf.md`) / `pymupdf`(`.api/pymupdf.md`) / `pypdf`(`.api/pypdf.md`) / `pypdfium2`(`.api/pypdfium2.md`): each PDF owner meets ocrmypdf at PDF bytes — ocrmypdf owns OCR-to-searchable-PDF/A, pikepdf owns AES-256 and content-stream tokens, pymupdf owns native render/extract, pypdf and pypdfium2 own structural editing and PDFium render.

[LOCAL_ADMISSION]:
- `lazy import ocrmypdf` inside the worker process (the `document/lens` import form), never module-level; `ocr` takes path/IO arguments over its own `NamedTemporaryFile` source/target, so the bytes-in/bytes-out boundary lives in the consuming arm, not in ocrmypdf.

[RAIL_LAW]:
- Package: `ocrmypdf`
- Owns: OCR-to-PDF/A conversion of a single PDF or image, Tesseract page OCR with language/PSM/OEM/mode control, image preprocessing, PDF/A output-type and color-conversion control (including the standalone `pdfa` egress and `file_claims_pdfa` claim-check), pre-OCR layout/structure analysis (`pdfinfo.PdfInfo`/`PageInfo`), hOCR parsing (`hocrtransform.HocrParser`), document-metadata stamping, sidecar text emission, plugin-driven extension, and a typed `ExitCode`/`ExitCodeException` rail
- Accept: single-document OCR-to-searchable-PDF/A runs feeding the `document/lens#LENS` `LensProvider.OCRMYPDF` arm (from `ocr` kwargs or a pre-built `OcrOptions`), gated on `code is ExitCode.ok` and mapped onto the `expression` `Result` rail; the `api.run_hocr_*` two-phase split with a `hocrtransform.HocrParser` mutation when interposing between OCR and graft; `pdfinfo.PdfInfo` as the standalone pre-flight; `pdfa.speculative_pdfa_conversion` as the standalone PDF/A step; HEIF input via the `pi-heif` boundary opener; execution on the `anyio` `to_process` worker lane under the shared `CapacityLimiter`
- Reject: wrapper-renames of `ocr`; a hand-rolled rasterize-OCR-graft loop or external `tesseract`/`gs` orchestration the package owns; a `run_force`/`run_skip`/`run_redo` entrypoint family where `mode` is a call row; bare-`Exception` handling that discards the `ExitCode` rail or flattens the `ExitCodeException` lattice; a hand-rolled `multiprocessing` pool around `ocr` instead of the `anyio` outer offload + `Executor` inner page fan; a forked OCR loop bypassing the `Executor`/`OcrEngine` plugin points; a separate `gs` PDF/A invocation where `pdfa.speculative_pdfa_conversion` is the in-package egress; a phantom `HocrTransform` (the class is `HocrParser`); treating `pymupdf`/`pdfplumber` as ocrmypdf deps (its deps are `pypdfium2`/`pdfminer-six`/`fpdf2`/`uharfbuzz`)
