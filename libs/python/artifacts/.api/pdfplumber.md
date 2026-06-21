# [PY_ARTIFACTS_API_PDFPLUMBER]

`pdfplumber` supplies the ruled-table and word/char extraction surface for the artifacts table rail: a `pdfplumber.open` factory that parses a PDF stream into a `PDF`/`Page` container hierarchy carrying per-object char, line, rect, and edge geometry, plus a `TableFinder` engine that resolves ruled-line intersections into `Table` cell grids and a `WordExtractor` that clusters chars into positioned words. The package owner composes `open`, `Page.extract_words`, `Page.find_tables`, and `Table.extract` into the `LENS_TABLE_ARM` path; it removes any hand-rolled PDF tokenizer because pdfminer.six parsing is in-package, and it never re-implements the line-snap/intersection cell-detection algorithm pdfplumber already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pdfplumber`
- package: `pdfplumber`
- import: `pdfplumber`
- owner: `artifacts`
- rail: table
- installed: `0.11.10` reflected via reflection on cp315 (Python 3.15)
- license: MIT; pure Python (`pdfplumber-0.11.10-py3-none-any.whl`) layered over `pdfminer.six` (MIT, the parse engine), `pypdfium2` (BSD-3/Apache-2.0, the PDFium raster bindings used by `to_image`), and `pillow` (MIT-CMU, image backing); no compiled extension of its own, installs clean on cp315; `to_image` requires the `pypdfium2` raster path, `repair` requires a system Ghostscript binary
- entry points: console script `pdfplumber` (CLI; `--format csv/json/text` over `to_csv`/`to_json`); library use is import-only via `pdfplumber.open`
- capability: PDF stream parsing into a `PDF`/`Page` container hierarchy, per-object char/line/rect/curve/image/annot/hyperlink/edge geometry, ruled-line `TableFinder` cell detection (`lines`/`lines_strict`/`text`/`explicit` strategies with full snap/join/intersection tolerance fields), positioned word/char extraction with tolerance + direction clustering, layout-preserving text rendering and regex `search`, tagged-PDF logical `structure_tree`, bbox crop/within/outside/filter/dedupe views, `to_csv`/`to_json`/`to_dict` object export, `PageImage` raster render with a draw/outline debug surface, and Ghostscript-backed repair

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: container and table roots
- rail: table

`open` returns a `PDF` whose `pages` property yields `Page` containers; `Page.find_tables` returns `Table` instances built by `TableFinder` from snapped/joined edges, and `TableSettings` carries the strategy and tolerance policy resolved from a settings dict. `PdfminerException` wraps a failed pdfminer.six parse.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [RAIL]                                                     |
| :-----: | :------------------ | :----------------- | :--------------------------------------------------------- |
|  [01]   | `PDF`               | container root     | parsed document owning `pages`, `metadata`, `objects`, `structure_tree`, and `to_csv`/`to_json` export |
|  [02]   | `Page`              | container          | one page owning chars/lines/rects/curves/images/edges/annots and extraction ops |
|  [03]   | `Table`             | cell grid          | one detected table exposing `rows`/`columns`/`cells`/`bbox` |
|  [04]   | `TableFinder`       | detection engine   | edge -> intersection -> cell -> `Table` resolver (`.edges`/`.intersections`/`.cells`/`.tables`) |
|  [05]   | `TableSettings`     | settings dataclass | frozen strategy + snap/join/intersection tolerance policy; `TableSettings.resolve(dict)` |
|  [06]   | `CroppedPage`       | container view     | bbox-restricted `Page` from `crop`/`within_bbox`/`outside_bbox` |
|  [07]   | `FilteredPage`      | container view     | predicate-filtered `Page` from `filter`/`dedupe_chars`     |
|  [08]   | `display.PageImage` | raster view        | PDFium-rendered raster with the `draw_*`/`outline_*` debug overlay surface |
|  [09]   | `utils.text.WordExtractor` | clustering engine | the single char-to-word clusterer behind `extract_words`/`extract_text` |
|  [10]   | `PdfminerException` | error              | wrapped pdfminer.six parse failure                         |

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
|  [11]   | `TableFinder.tables`     | attribute -> `List[Table]`                                                    | resolved tables (also `.edges`/`.intersections`/`.cells`) |

[ENTRYPOINT_SCOPE]: table-settings tolerance fields
- rail: table

`TableSettings` is a frozen dataclass resolved from the `table_settings` dict by `TableSettings.resolve`; the strategy and tolerance fields are the single tuning surface feeding one `TableFinder` pipeline (edges -> intersections -> cells -> tables). `text_settings` nests the `WordExtractor` kwargs used when a `"text"` strategy infers lines from word positions.

| [INDEX] | [FIELD]                                            | [DEFAULT]    | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------- | :----------- | :---------------------------------------------------- |
|  [01]   | `vertical_strategy` / `horizontal_strategy`        | `"lines"`    | `"lines"`/`"lines_strict"`/`"text"`/`"explicit"` edge source |
|  [02]   | `explicit_vertical_lines` / `explicit_horizontal_lines` | `None`  | caller-supplied line coordinates for `"explicit"`     |
|  [03]   | `snap_tolerance` / `snap_x_tolerance` / `snap_y_tolerance` | `3`/`0`/`0` | collapse near-collinear edges before joining     |
|  [04]   | `join_tolerance` / `join_x_tolerance` / `join_y_tolerance` | `3`/`0`/`0` | merge co-linear segments into one edge           |
|  [05]   | `edge_min_length` / `edge_min_length_prefilter`    | `3`/`1`      | drop short edges (prefilter is the cheap pre-pass)    |
|  [06]   | `min_words_vertical` / `min_words_horizontal`      | `3`/`1`      | word count needed to infer a line under `"text"`      |
|  [07]   | `intersection_tolerance` / `intersection_x_tolerance` / `intersection_y_tolerance` | `3`/`0`/`0` | edge-crossing tolerance forming cell corners |
|  [08]   | `text_settings`                                    | `None`       | nested `WordExtractor` kwargs for `"text"` strategy   |

[ENTRYPOINT_SCOPE]: word, char, and line geometry
- rail: table

`extract_words` clusters `chars` by `x_tolerance`/`y_tolerance` into positioned word dicts; `extract_text` renders running text and `layout=True` preserves positional spacing. Container geometry properties feed `vertical_strategy="text"` and explicit-line detection.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                                                                                                                                                                                    | [CAPABILITY]                                |
| :-----: | :------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `Page.extract_words`       | `extract_words(**kwargs)` -> `List[Dict]` — kwargs pass to `utils.text.WordExtractor`: `x_tolerance=3, y_tolerance=3, x_tolerance_ratio=None, y_tolerance_ratio=None, keep_blank_chars=False, use_text_flow=False, line_dir="ttb", char_dir="ltr", line_dir_rotated=None, char_dir_rotated=None, extra_attrs=None, split_at_punctuation=False, expand_ligatures=True` | positioned word dicts from clustered chars  |
|  [02]   | `Page.extract_text`        | `extract_text(**kwargs)` -> `str` — kwargs forward `layout`, `x_tolerance`/`y_tolerance`, `line_dir_render`/`char_dir_render`, and the `WordExtractor` clustering fields to `utils.text.extract_text`                                                            | running or layout-preserving page text      |
|  [03]   | `Page.extract_text_lines`  | `extract_text_lines(strip=True, return_chars=True, **kwargs)` -> `T_obj_list`                                                                                                                                                                                   | per-line text dicts with bbox/chars         |
|  [04]   | `Page.extract_text_simple` | `extract_text_simple(x_tolerance=3, y_tolerance=3)` -> `str`                                                                                                                                                                                                    | fast collated text without layout map       |
|  [05]   | `Page.search`              | `search(pattern, regex=True, case=True, main_group=0, return_chars=True, return_groups=True, **kwargs)` -> `List[Dict[str, Any]]`                                                                                                                               | regex match over the layout textmap         |
|  [06]   | `Page.crop`                | `crop(bbox, relative=False, strict=True)` -> `CroppedPage`                                                                                                                                                                                                      | bbox-restricted page view (objects overlap) |
|  [07]   | `Page.within_bbox`         | `within_bbox(bbox, relative=False, strict=True)` -> `CroppedPage`                                                                                                                                                                                               | bbox view of objects fully inside           |
|  [08]   | `Page.outside_bbox`        | `outside_bbox(bbox, relative=False, strict=True)` -> `CroppedPage`                                                                                                                                                                                              | bbox view of objects fully outside (inverse) |
|  [09]   | `Page.filter`              | `filter(test_function)` -> `FilteredPage`                                                                                                                                                                                                                       | predicate-filtered page view                |
|  [10]   | `Page.dedupe_chars`        | `dedupe_chars(**kwargs)` -> `FilteredPage`                                                                                                                                                                                                                      | drop duplicate overlapping chars            |
|  [11]   | `Page.chars`               | property -> `T_obj_list`                                                                                                                                                                                                                                        | per-char dicts with text and geometry       |
|  [12]   | `Page.lines`               | property -> `T_obj_list`                                                                                                                                                                                                                                        | vector line objects                         |
|  [13]   | `Page.rects`               | property -> `T_obj_list`                                                                                                                                                                                                                                        | rectangle objects                           |
|  [14]   | `Page.curves`              | property -> `T_obj_list`                                                                                                                                                                                                                                        | bezier/curve path objects                   |
|  [15]   | `Page.images`             | property -> `T_obj_list`                                                                                                                                                                                                                                        | raster image XObjects with bbox + stream    |
|  [16]   | `Page.edges`               | property -> `T_obj_list`                                                                                                                                                                                                                                        | derived ruled edges (line + rect + curve)   |
|  [17]   | `Page.annots`              | property -> `T_obj_list`                                                                                                                                                                                                                                        | PDF annotations (notes/widgets)             |
|  [18]   | `Page.hyperlinks`          | property -> `T_obj_list`                                                                                                                                                                                                                                        | URI-link annotations with target + bbox     |
|  [19]   | `Page.structure_tree`      | property -> `List[Dict]`                                                                                                                                                                                                                                        | tagged-PDF logical structure subtree        |
|  [20]   | `Page.to_image`            | `to_image(resolution=None, width=None, height=None, antialias=False, force_mediabox=False)` -> `PageImage`                                                                                                                                                      | raster render (PDFium via pypdfium2) for debug |
|  [21]   | `Page.to_dict` / `to_csv` / `to_json` | `to_csv(types=None)` -> `str` / `to_json(...)` -> `str` / `to_dict(object_types=None)` -> `Dict`                                                                                                                                                | flatten parsed objects to tabular/JSON/dict export |

[ENTRYPOINT_SCOPE]: raster render and visual debug
- rail: table

`to_image` renders the page through PDFium (`pypdfium2`) into a `PageImage` whose `draw_*`/`outline_*` methods overlay geometry for chained, fluent debugging (each returns `self`); `debug_tablefinder` overlays the detected edges/intersections/cells. The image backs onto `pillow`; `save` writes PNG (or any Pillow format).

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                        | [CAPABILITY]                                |
| :-----: | :------------------------- | :-------------------------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `PageImage.draw_rect(s)`   | `draw_rect(bbox_or_obj, fill=..., stroke=..., stroke_width=1)` / `draw_rects(list)` -> `PageImage`   | overlay bbox/object rectangles               |
|  [02]   | `PageImage.draw_line(s)`   | `draw_line(points_or_obj, stroke=..., stroke_width=1)` / `draw_lines(list)` -> `PageImage`           | overlay line segments                        |
|  [03]   | `PageImage.draw_circle(s)` | `draw_circle(center_or_obj, radius=5, fill=..., stroke=...)` / `draw_circles(list)` -> `PageImage`   | overlay point markers                        |
|  [04]   | `PageImage.draw_vline(s)` / `draw_hline(s)` | `draw_vline(location, stroke=..., stroke_width=1)` / `draw_hline(...)` -> `PageImage`               | overlay full-height/width guide lines        |
|  [05]   | `PageImage.outline_words` / `outline_chars` | `outline_words(stroke=..., fill=..., stroke_width=1, x_tolerance=3, y_tolerance=3)` -> `PageImage`  | overlay clustered word/char boxes            |
|  [06]   | `PageImage.debug_tablefinder` | `debug_tablefinder(table_settings=None)` -> `PageImage`                                          | overlay the `TableFinder` edges/cells        |
|  [07]   | `PageImage.reset` / `copy` | `reset()` -> `PageImage` / `copy()` -> `PageImage`                                                   | clear overlays / fork the image              |
|  [08]   | `PageImage.save`           | `save(dest, format="PNG", quantize=True, colors=256, bits=8, **kwargs)` -> `None`                   | write the rendered/annotated raster          |

## [04]-[IMPLEMENTATION_LAW]

[TABLE_EXTRACTION]:
- import: `import pdfplumber` at boundary scope only; module-level import is banned by the manifest import policy.
- open axis: one `pdfplumber.open` owns parsing; `pages`/`laparams`/`password`/`repair` are call rows, never a per-config loader type; `repair=True` routes through Ghostscript before parsing, never a separate parse path.
- detection axis: `find_tables` is the single ruled-table surface; `vertical_strategy`/`horizontal_strategy` select `"lines"`/`"lines_strict"`/`"text"`/`"explicit"` as `TableSettings` rows resolved from the `table_settings` dict, never a parallel finder type per strategy; snap/join/intersection tolerances are settings fields feeding one `TableFinder` pipeline (edges -> intersections -> cells -> tables).
- extraction axis: `Table.extract` owns per-cell char clustering keyed off the cell bbox; `extract_tables`/`extract_table` are the largest-vs-all rows over the same `find_tables` result, never a re-implementation of the cell text join.
- word axis: `extract_words`/`extract_text` forward `**kwargs` to one `utils.text.WordExtractor`; `x_tolerance`/`y_tolerance`/`x_tolerance_ratio`/`keep_blank_chars`/`use_text_flow`/`line_dir`/`char_dir`/`split_at_punctuation`/`extra_attrs`/`expand_ligatures` are call rows on that single extractor, never a per-mode extractor type; `extract_text` with `layout=True` is the positional-render row, `layout=False` the running-text row; `search(pattern)` runs a regex over the same layout textmap.
- export axis: `to_csv`/`to_json`/`to_dict` flatten the parsed object stream (chars/lines/rects/curves/images/edges) for a one-shot tabular handoff; `structure_tree` exposes the tagged-PDF logical hierarchy; these are projection rows over the already-parsed container, never a re-parse.
- view axis: `crop`/`within_bbox`/`outside_bbox`/`filter`/`dedupe_chars` return restricted `CroppedPage`/`FilteredPage` views sharing the container contract; a region scope is a view row, never a re-parsed page.
- debug axis: `to_image` renders through `pypdfium2`/`pillow` into a `PageImage`; the fluent `draw_*`/`outline_*`/`debug_tablefinder` chain (each returns `self`) overlays geometry for inspection — a debugging projection, never part of the extraction result.
- evidence: each parse captures the reflected version, page count, per-page char/line/rect/curve/image/edge counts, detected table count, cell-grid shape, word-cluster tolerances, and resolved `TableSettings` strategy as a table receipt.
- boundary: pdfplumber owns ruled-table detection, per-object geometry, and tolerance-clustered word/char extraction over `pdfminer.six`; bulk page text and native rasterization route to `pymupdf` when MuPDF's speed beats the pdfminer geometry pass; `to_image` rasterization runs through `pypdfium2`/`pillow`, never an open-coded renderer; Ghostscript repair routes through `repair`/`open(repair=True)` only when a malformed file requires it; the extracted cell grid and word geometry feed the document and table owners directly; live UI stays outside this package.

[RAIL_LAW]:
- Package: `pdfplumber`
- Owns: PDF parsing into a container hierarchy over `pdfminer.six`, ruled-line table detection with full snap/join/intersection tolerance fields and cell-grid extraction, positioned word/char extraction with tolerance + direction clustering, layout-preserving text rendering and regex `search`, tagged-PDF `structure_tree`, object `to_csv`/`to_json`/`to_dict` export, bbox crop/within/outside/filter/dedupe views, and `PageImage` raster debug overlays via `pypdfium2`/`pillow`
- Accept: ruled-table and word/char extraction feeding the table, document, and visuals owners
- Reject: wrapper-renames of `open`/`find_tables`/`extract_words`; a hand-rolled PDF tokenizer where pdfminer.six parsing is in-package; a re-implemented line-snap/intersection cell detector; an open-coded column-26/coordinate or raster renderer where `TableSettings`/`pypdfium2` own it; a parallel `Page`, `WordExtractor`, or finder type per strategy or region; reaching here for bulk page text where `pymupdf`'s MuPDF pass is faster; identity/receipt shapes the runtime owner already owns
