# [PY_ARTIFACTS_API_PDFPLUMBER]

`pdfplumber` supplies the ruled-table and word/char extraction surface for the artifacts table rail: a `pdfplumber.open` factory that parses a PDF stream into a `PDF`/`Page` container hierarchy carrying per-object char, line, rect, and edge geometry, plus a `TableFinder` engine that resolves ruled-line intersections into `Table` cell grids and a `WordExtractor` that clusters chars into positioned words. The package owner composes `open`, `Page.extract_words`, `Page.find_tables`, and `Table.extract` into the `LENS_TABLE_ARM` path; it removes any hand-rolled PDF tokenizer because pdfminer.six parsing is in-package, and it never re-implements the line-snap/intersection cell-detection algorithm pdfplumber already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pdfplumber`
- package: `pdfplumber`
- import: `pdfplumber`
- owner: `artifacts`
- rail: table
- installed: `0.11.10` reflected via `assay api` on cp315
- entry points: console script `pdfplumber` (CLI); library use is import-only via `pdfplumber.open`
- capability: PDF stream parsing into a `PDF`/`Page` container hierarchy, per-object char/line/rect/curve/edge geometry, ruled-line `TableFinder` cell detection (`lines`/`text`/`explicit` strategies), positioned word/char extraction with tolerance clustering, layout-preserving text rendering, bbox crop/filter views, and Ghostscript-backed repair

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: container and table roots
- rail: table

`open` returns a `PDF` whose `pages` property yields `Page` containers; `Page.find_tables` returns `Table` instances built by `TableFinder` from snapped/joined edges, and `TableSettings` carries the strategy and tolerance policy resolved from a settings dict. `PdfminerException` wraps a failed pdfminer.six parse.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [RAIL]                                                     |
| :-----: | :------------------ | :----------------- | :--------------------------------------------------------- |
|  [01]   | `PDF`               | container root     | parsed document owning `pages`, `metadata`, and `objects`  |
|  [02]   | `Page`              | container          | one page owning chars/lines/rects/edges and extraction ops |
|  [03]   | `Table`             | cell grid          | one detected table exposing `rows`/`columns`/`cells`       |
|  [04]   | `TableFinder`       | detection engine   | edge -> intersection -> cell -> `Table` resolver           |
|  [05]   | `TableSettings`     | settings dataclass | strategy and tolerance policy for table detection          |
|  [06]   | `CroppedPage`       | container view     | bbox-restricted `Page` from `crop`/`within_bbox`           |
|  [07]   | `FilteredPage`      | container view     | predicate-filtered `Page` from `filter`/`dedupe_chars`     |
|  [08]   | `PdfminerException` | error              | wrapped pdfminer.six parse failure                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open and document navigation
- rail: table

`open` accepts a path, `pathlib.Path`, or binary stream and returns a `PDF`; `pages` restricts to the 1-indexed `pages` argument when supplied. `repair` runs Ghostscript over a malformed file before parsing.

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                                                                                                                                                                                     | [CAPABILITY]                                |
| :-----: | :------------------ | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `pdfplumber.open`   | `open(path_or_fp, pages=None, laparams=None, password=None, strict_metadata=False, unicode_norm=None, repair=False, gs_path=None, repair_setting="default", raise_unicode_errors=True)` -> `PDF` | parse a PDF into a `PDF` container          |
|  [02]   | `pdfplumber.repair` | `repair(path_or_fp, outfile=None, password=None, gs_path=None, setting="default")` -> `Optional[BytesIO]`                                                                                        | Ghostscript-rewrite a malformed PDF         |
|  [03]   | `PDF.pages`         | property -> `List[Page]`                                                                                                                                                                         | 1-indexed page list (restricted by `pages`) |
|  [04]   | `PDF.metadata`      | attribute -> `Dict[str, Any]`                                                                                                                                                                    | resolved document info dictionary           |
|  [05]   | `PDF.objects`       | property -> `Dict[str, T_obj_list]`                                                                                                                                                              | all parsed objects keyed by kind            |

[ENTRYPOINT_SCOPE]: table detection and extraction
- rail: table

`find_tables`/`extract_tables` resolve every plausible table; `find_table`/`extract_table` return the largest by cell count. `table_settings` is a dict resolved into `TableSettings`; `vertical_strategy`/`horizontal_strategy` select `"lines"`, `"lines_strict"`, `"text"`, or `"explicit"`. `Table.extract` clusters chars within each cell bbox into row-major text.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                                  | [CAPABILITY]                                     |
| :-----: | :----------------------- | :---------------------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `Page.find_tables`       | `find_tables(table_settings=None)` -> `List[Table]`                           | every plausible table on the page                |
|  [02]   | `Page.find_table`        | `find_table(table_settings=None)` -> `Optional[Table]`                        | the largest table by cell count                  |
|  [03]   | `Page.extract_tables`    | `extract_tables(table_settings=None)` -> `List[List[List[Optional[str]]]]`    | row-major text for every table                   |
|  [04]   | `Page.extract_table`     | `extract_table(table_settings=None)` -> `Optional[List[List[Optional[str]]]]` | row-major text for the largest table             |
|  [05]   | `Page.debug_tablefinder` | `debug_tablefinder(table_settings=None)` -> `TableFinder`                     | the `TableFinder` with edges/intersections/cells |
|  [06]   | `Table.extract`          | `extract(**kwargs)` -> `List[List[Optional[str]]]`                            | per-cell clustered text grid                     |
|  [07]   | `Table.rows`             | property -> `List[CellGroup]`                                                 | row-grouped cells                                |
|  [08]   | `Table.columns`          | property -> `List[CellGroup]`                                                 | column-grouped cells                             |
|  [09]   | `Table.cells`            | attribute -> `List[T_bbox]`                                                   | detected cell bboxes                             |
|  [10]   | `Table.bbox`             | property -> `T_bbox`                                                          | bounding box of the table                        |
|  [11]   | `TableFinder.tables`     | attribute -> `List[Table]`                                                    | resolved tables from the engine                  |

[ENTRYPOINT_SCOPE]: word, char, and line geometry
- rail: table

`extract_words` clusters `chars` by `x_tolerance`/`y_tolerance` into positioned word dicts; `extract_text` renders running text and `layout=True` preserves positional spacing. Container geometry properties feed `vertical_strategy="text"` and explicit-line detection.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                                                                                                                                                                                    | [CAPABILITY]                                |
| :-----: | :------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `Page.extract_words`       | `extract_words(x_tolerance=3, y_tolerance=3, x_tolerance_ratio=None, y_tolerance_ratio=None, keep_blank_chars=False, use_text_flow=False, line_dir="ttb", char_dir="ltr", extra_attrs=None, split_at_punctuation=False, expand_ligatures=True)` -> `T_obj_list` | positioned word dicts from clustered chars  |
|  [02]   | `Page.extract_text`        | `extract_text(layout=False, x_tolerance=3, y_tolerance=3, line_dir_render=None, char_dir_render=None, **kwargs)` -> `str`                                                                                                                                       | running or layout-preserving page text      |
|  [03]   | `Page.extract_text_lines`  | `extract_text_lines(strip=True, return_chars=True, **kwargs)` -> `T_obj_list`                                                                                                                                                                                   | per-line text dicts with bbox/chars         |
|  [04]   | `Page.extract_text_simple` | `extract_text_simple(x_tolerance=3, y_tolerance=3)` -> `str`                                                                                                                                                                                                    | fast collated text without layout map       |
|  [05]   | `Page.search`              | `search(pattern, regex=True, case=True, main_group=0, return_chars=True, return_groups=True, **kwargs)` -> `List[Dict[str, Any]]`                                                                                                                               | regex match over the layout textmap         |
|  [06]   | `Page.crop`                | `crop(bbox, relative=False, strict=True)` -> `CroppedPage`                                                                                                                                                                                                      | bbox-restricted page view (objects overlap) |
|  [07]   | `Page.within_bbox`         | `within_bbox(bbox, relative=False, strict=True)` -> `CroppedPage`                                                                                                                                                                                               | bbox view of objects fully inside           |
|  [08]   | `Page.filter`              | `filter(test_function)` -> `FilteredPage`                                                                                                                                                                                                                       | predicate-filtered page view                |
|  [09]   | `Page.dedupe_chars`        | `dedupe_chars(**kwargs)` -> `FilteredPage`                                                                                                                                                                                                                      | drop duplicate overlapping chars            |
|  [10]   | `Page.chars`               | property -> `T_obj_list`                                                                                                                                                                                                                                        | per-char dicts with text and geometry       |
|  [11]   | `Page.lines`               | property -> `T_obj_list`                                                                                                                                                                                                                                        | vector line objects                         |
|  [12]   | `Page.rects`               | property -> `T_obj_list`                                                                                                                                                                                                                                        | rectangle objects                           |
|  [13]   | `Page.edges`               | property -> `T_obj_list`                                                                                                                                                                                                                                        | derived ruled edges (line + rect)           |
|  [14]   | `Page.to_image`            | `to_image(resolution=None, width=None, height=None, antialias=False, force_mediabox=False)` -> `PageImage`                                                                                                                                                      | raster render for visual debugging          |

## [04]-[IMPLEMENTATION_LAW]

[TABLE_EXTRACTION]:
- import: `import pdfplumber` at boundary scope only; module-level import is banned by the manifest import policy.
- open axis: one `pdfplumber.open` owns parsing; `pages`/`laparams`/`password`/`repair` are call rows, never a per-config loader type; `repair=True` routes through Ghostscript before parsing, never a separate parse path.
- detection axis: `find_tables` is the single ruled-table surface; `vertical_strategy`/`horizontal_strategy` select `"lines"`/`"lines_strict"`/`"text"`/`"explicit"` as `TableSettings` rows resolved from the `table_settings` dict, never a parallel finder type per strategy; snap/join/intersection tolerances are settings fields feeding one `TableFinder` pipeline (edges -> intersections -> cells -> tables).
- extraction axis: `Table.extract` owns per-cell char clustering keyed off the cell bbox; `extract_tables`/`extract_table` are the largest-vs-all rows over the same `find_tables` result, never a re-implementation of the cell text join.
- word axis: `extract_words` owns char-to-word clustering; `x_tolerance`/`y_tolerance`/`keep_blank_chars`/`split_at_punctuation`/`extra_attrs` are call rows on one `WordExtractor`, never a per-mode extractor type; `extract_text` with `layout=True` is the positional-render row, `layout=False` the running-text row.
- view axis: `crop`/`within_bbox`/`filter`/`dedupe_chars` return restricted `CroppedPage`/`FilteredPage` views sharing the container contract; a region scope is a view row, never a re-parsed page.
- evidence: each parse captures the reflected version, page count, per-page char/line/rect/edge counts, detected table count, cell-grid shape, and word-cluster tolerances as a table receipt.
- boundary: pdfplumber owns PDF parsing, geometry, ruled-table detection, and word/char extraction over pdfminer.six; Ghostscript repair routes through `repair`/`open(repair=True)` only when a malformed file requires it; raster `to_image` output feeds the visuals owner; the extracted cell grid and word geometry feed the document and table owners directly; live UI stays outside this package.

[RAIL_LAW]:
- Package: `pdfplumber`
- Owns: PDF parsing into a container hierarchy, ruled-line table detection and cell-grid extraction, positioned word/char extraction with tolerance clustering, layout-preserving text rendering, and bbox crop/filter views
- Accept: ruled-table and word/char extraction feeding the table, document, and visuals owners
- Reject: wrapper-renames of `open`/`find_tables`/`extract_words`; a hand-rolled PDF tokenizer where pdfminer.six parsing is in-package; a re-implemented line-snap/intersection cell detector; a parallel `Page` or finder type per strategy or region; identity/receipt shapes the runtime owner already owns
