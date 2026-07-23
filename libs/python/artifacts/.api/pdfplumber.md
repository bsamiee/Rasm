# [PY_ARTIFACTS_API_PDFPLUMBER]

`pdfplumber` owns ruled-table, word-geometry, region, and tagged-structure recovery over `pdfminer.six`: `pdfplumber.open` parses a PDF stream into a `PDF`/`Page` hierarchy carrying per-object geometry as plain dicts, a `TableFinder` resolves ruled-line intersections into `Table` cell grids, and a `WordExtractor` clusters chars into positioned word dicts. It feeds the `document` lens `LensProvider.PLUMBER` arm as the higher-recall `lines`/`lines_strict`/`text`/`explicit` ruled-table arm beside the native `pymupdf` bulk arm, meeting it at PDF bytes and the shared `DocumentNode` grid.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pdfplumber`
- package: `pdfplumber`
- import: `pdfplumber`
- owner: `artifacts`
- rail: table
- license: `MIT` — permissive, unrestricted in a closed or distributed pipeline
- build: pure-Python wheel, no native build, runs in-process on the cp315 runtime; `pdfminer.six`/`pypdfium2`/`Pillow` are the transitive parse/raster deps
- entry points: console script `pdfplumber` dumps CSV/JSON/text; library use is import-only via `pdfplumber.open`
- capability: PDF parsing into a `PDF`/`Page` hierarchy; per-object char/line/rect/curve/image/annot/hyperlink/edge geometry; ruled-line `TableFinder` cell detection over four strategies with full snap/join/intersection tolerance; tolerance- and direction-clustered word/char extraction; layout-preserving text and regex `search`; tagged-PDF `structure_tree`; bbox crop/within/outside/filter/dedupe views; `to_csv`/`to_json`/`to_dict` export; `PageImage` raster debug overlays; Ghostscript repair

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: container roots, table grid, and faults

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]        | [CAPABILITY]                                                        |
| :-----: | :--------------------------------------- | :------------------- | :------------------------------------------------------------------ |
|  [01]   | `PDF`                                    | container root       | owns `pages`/`metadata`/`objects`/`structure_tree` + export         |
|  [02]   | `Page`                                   | container            | owns chars/lines/rects/curves/images/edges/annots + extraction ops  |
|  [03]   | `Table`                                  | cell grid            | one detected table exposing `rows`/`columns`/`cells`/`bbox`         |
|  [04]   | `TableFinder`                            | detection engine     | edge->cell->`Table` resolver; `.edges`/`.cells`/`.tables`           |
|  [05]   | `TableSettings`                          | settings dataclass   | frozen strategy + tolerance policy; `TableSettings.resolve(dict)`   |
|  [06]   | `CroppedPage`                            | container view       | bbox-restricted `Page` from `crop`/`within_bbox`/`outside_bbox`     |
|  [07]   | `FilteredPage`                           | container view       | predicate-filtered `Page` from `filter`/`dedupe_chars`              |
|  [08]   | `display.PageImage`                      | raster view          | PDFium raster with `draw_*`/`outline_*` debug overlay               |
|  [09]   | `utils.text.WordExtractor`               | clustering engine    | the char-to-word clusterer behind `extract_words`/`extract_text`    |
|  [10]   | `utils.text.TextMap` / `WordMap`         | layout textmap       | char->layout map behind `extract_text(layout=True)`/`search`        |
|  [11]   | `utils.clustering.cluster_objects`       | clustering primitive | the `cluster_objects`/`cluster_list`/`make_cluster_dict` primitives |
|  [12]   | `utils.exceptions.PdfminerException`     | error                | pdfminer.six parse fault -> `LensFault`/`Result.Error`              |
|  [13]   | `utils.exceptions.MalformedPDFException` | error                | malformed-PDF fault; routes to `open(repair=True)`                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open and document navigation
- `pages` restricts to the 1-indexed page list; `repair=True` runs Ghostscript over a malformed file before parsing.

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :----------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `open(path_or_fp, pages, password, repair, gs_path) -> PDF`  | factory  | parse a path or stream into a `PDF` |
|  [02]   | `repair(path_or_fp, outfile, password, gs_path) -> BytesIO?` | factory  | Ghostscript-rewrite a malformed PDF |
|  [03]   | `PDF.pages -> list[Page]`                                    | property | 1-indexed page list                 |
|  [04]   | `PDF.metadata -> dict`                                       | property | resolved document info dictionary   |
|  [05]   | `PDF.objects -> dict`                                        | property | all parsed objects keyed by kind    |

[ENTRYPOINT_SCOPE]: table detection and extraction
- `find_tables`/`extract_tables` resolve every plausible table; `find_table`/`extract_table` return the largest by cell count. `table_settings` is a dict resolved into `TableSettings`; `vertical_strategy`/`horizontal_strategy` select the edge source.

| [INDEX] | [SURFACE]                                               | [SHAPE]   | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------ | :-------- | :--------------------------------------------------- |
|  [01]   | `Page.find_tables(table_settings) -> list[Table]`       | instance  | every plausible table on the page                    |
|  [02]   | `Page.find_table(table_settings) -> Table?`             | instance  | the largest table by cell count                      |
|  [03]   | `Page.extract_tables(table_settings) -> list[grid]`     | instance  | row-major text for every table                       |
|  [04]   | `Page.extract_table(table_settings) -> grid?`           | instance  | row-major text for the largest table                 |
|  [05]   | `Page.debug_tablefinder(table_settings) -> TableFinder` | instance  | the `TableFinder` with edges/cells                   |
|  [06]   | `Table.extract(**kwargs) -> list[list[str?]]`           | instance  | per-cell clustered text grid                         |
|  [07]   | `Table.rows -> list[CellGroup]`                         | property  | row-grouped cells                                    |
|  [08]   | `Table.columns -> list[CellGroup]`                      | property  | column-grouped cells                                 |
|  [09]   | `Table.cells -> list[bbox]`                             | attribute | detected cell bboxes                                 |
|  [10]   | `Table.bbox -> bbox`                                    | property  | bounding box of the table                            |
|  [11]   | `TableFinder.tables`                                    | attribute | resolved tables (`.edges`/`.intersections`/`.cells`) |

[ENTRYPOINT_SCOPE]: table-settings tolerance fields
- `TableSettings.resolve` builds the frozen dataclass from the `table_settings` dict; strategy and tolerance are the single tuning surface feeding one `TableFinder` pipeline (edges -> intersections -> cells -> tables). Each snap/join/intersection tolerance carries `_x_tolerance`/`_y_tolerance` axis overrides defaulting to the `UNSET` (`0.0`) sentinel, which inherits the base.

| [INDEX] | [FIELD]                                                 | [DEFAULT] | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------------------ | :-------- | :----------------------------------------------------------- |
|  [01]   | `vertical_strategy` / `horizontal_strategy`             | `"lines"` | `"lines"`/`"lines_strict"`/`"text"`/`"explicit"` edge source |
|  [02]   | `explicit_vertical_lines` / `explicit_horizontal_lines` | `None`    | caller-supplied line coordinates for `"explicit"`            |
|  [03]   | `snap_tolerance`                                        | `3`       | collapse near-collinear edges before joining                 |
|  [04]   | `join_tolerance`                                        | `3`       | merge co-linear segments into one edge                       |
|  [05]   | `edge_min_length` / `edge_min_length_prefilter`         | `3`/`1`   | drop short edges (prefilter is the cheap pre-pass)           |
|  [06]   | `min_words_vertical` / `min_words_horizontal`           | `3`/`1`   | word count needed to infer a line under `"text"`             |
|  [07]   | `intersection_tolerance`                                | `3`       | edge-crossing tolerance forming cell corners                 |
|  [08]   | `text_settings`                                         | `None`    | nested `WordExtractor` kwargs consumed under `"text"`        |

- [08]-[TEXT_SETTINGS]: `TableSettings.resolve` strips the `text_` prefix off any settings-dict key (`text_x_tolerance` -> `text_settings["x_tolerance"]`) and consumes it only under a `"text"` strategy; a non-`WordExtractor` key raises `TypeError` at `WordExtractor(**text_settings)`.

[ENTRYPOINT_SCOPE]: word, char, and line geometry
- `extract_words`/`extract_text` forward `**kwargs` to one `WordExtractor`: `x_tolerance, y_tolerance, x_tolerance_ratio, y_tolerance_ratio, keep_blank_chars, use_text_flow, line_dir, char_dir, extra_attrs, split_at_punctuation, expand_ligatures`.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                                       |
| :-----: | :------------------------------------------------------- | :------- | :----------------------------------------------------------------- |
|  [01]   | `Page.extract_words(**kwargs) -> list[dict]`             | instance | positioned word dicts from clustered chars                         |
|  [02]   | `Page.extract_text(**kwargs) -> str`                     | instance | running or `layout=True` positional page text                      |
|  [03]   | `Page.extract_text_lines(strip, return_chars, **kwargs)` | instance | per-line text dicts with bbox/chars                                |
|  [04]   | `Page.extract_text_simple(**kwargs) -> str`              | instance | fast collated text without the layout textmap                      |
|  [05]   | `Page.search(pattern, regex, case, return_chars)`        | instance | regex match over the layout textmap                                |
|  [06]   | `Page.crop(bbox, relative, strict) -> CroppedPage`       | instance | bbox-restricted view; objects overlap                              |
|  [07]   | `Page.within_bbox(bbox, ...) -> CroppedPage`             | instance | bbox view of objects fully inside                                  |
|  [08]   | `Page.outside_bbox(bbox, ...) -> CroppedPage`            | instance | inverse bbox view of objects fully outside                         |
|  [09]   | `Page.filter(test_function) -> FilteredPage`             | instance | predicate-filtered page view                                       |
|  [10]   | `Page.dedupe_chars(**kwargs) -> FilteredPage`            | instance | drop duplicate overlapping chars                                   |
|  [11]   | `Page.chars`                                             | property | per-char dicts with text and geometry                              |
|  [12]   | `Page.lines` / `rects` / `curves`                        | property | vector line / rectangle / bezier-path objects                      |
|  [13]   | `Page.images`                                            | property | raster image XObjects with bbox + stream                           |
|  [14]   | `Page.edges`                                             | property | derived ruled edges (line + rect + curve), each with `orientation` |
|  [15]   | `Page.horizontal_edges` / `vertical_edges`               | property | edges by `orientation`; the explicit-line strategy reads these     |
|  [16]   | `Page.rect_edges` / `curve_edges`                        | property | edges derived only from rects / only from curves                   |
|  [17]   | `Page.annots` / `hyperlinks`                             | property | PDF annotations / URI-link annotations with target + bbox          |
|  [18]   | `Page.structure_tree`                                    | property | tagged-PDF logical structure subtree                               |
|  [19]   | `Page.to_image(resolution, width, height) -> PageImage`  | instance | raster render via `pypdfium2` for debug                            |
|  [20]   | `Page.to_dict` / `to_csv` / `to_json`                    | instance | flatten parsed objects to dict/tabular/JSON export                 |

[ENTRYPOINT_SCOPE]: object-dict key schema (the recovery contract)
- Every `T_obj` is a plain `dict[str, Any]` keyed by a fixed object-kind schema, so the `LensProvider.PLUMBER` arms index keys directly (`word["x0"]`, `line["text"]`, `cell[0]`) and the key contract is the load-bearing boundary. Coordinates are top-left origin (`top`/`bottom` grow downward); `doctop` is the document-cumulative top. Base geometry keys `text`/`x0`/`x1`/`top`/`bottom`/`doctop`/`upright`/`height`/`width` ride word and char dicts; each row lists only its additions.

| [INDEX] | [SHAPE]                          | [KEYS]                                                                                      |
| :-----: | :------------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | word dict (`extract_words`)      | base + `direction`, + each `extra_attrs` key off the first char (e.g. `fontname`/`size`)    |
|  [02]   | line dict (`extract_text_lines`) | `text`, `x0`/`x1`/`top`/`bottom` bbox, `chars` (when `return_chars=True`)                   |
|  [03]   | char dict (`Page.chars`)         | base + `size`/`fontname`/`adv`/`matrix`/`stroking_color`/`non_stroking_color`/`object_type` |
|  [04]   | line/rect/curve dict             | `x0`/`x1`/`top`/`bottom`/`width`/`height`/`linewidth`/`stroke`/`fill`/`pts`/`object_type`   |
|  [05]   | edge dict (`Page.edges`)         | line/rect/curve keys + `orientation` (`"v"`/`"h"`)                                          |
|  [06]   | `Table.cells` element            | `(x0, top, x1, bottom)` 4-tuple (or `None` for an absent cell in a `CellGroup`)             |
|  [07]   | search hit (`Page.search`)       | `text`/`x0`/`x1`/`top`/`bottom` + `groups`/`chars` (each conditional)                       |

- [01]-[WORD]: `fontname`/`size` need `extra_attrs=("fontname","size")`; they are not default word keys but live natively on the char dict.
- [05]-[EDGE]: `orientation` is the axis discriminant the `TableFinder` partitions on.
- [06]-[CELL]: cell corners fold into merged-cell `(row, col, col_span, row_span)` spans by deduping rounded x/y edges.

[ENTRYPOINT_SCOPE]: raster render and visual debug
- `to_image` renders through PDFium into a `PageImage` whose `draw_*`/`outline_*` methods overlay geometry and each return `self` for chaining; the raster backs onto `pillow`, and `save` writes PNG or any Pillow format.

| [INDEX] | [SURFACE]                                   | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `PageImage.draw_rect(s)`                    | instance | overlay bbox/object rectangles        |
|  [02]   | `PageImage.draw_line(s)`                    | instance | overlay line segments                 |
|  [03]   | `PageImage.draw_circle(s)`                  | instance | overlay point markers                 |
|  [04]   | `PageImage.draw_vline(s)` / `draw_hline(s)` | instance | overlay full-height/width guide lines |
|  [05]   | `PageImage.outline_words` / `outline_chars` | instance | overlay clustered word/char boxes     |
|  [06]   | `PageImage.debug_tablefinder`               | instance | overlay the `TableFinder` edges/cells |
|  [07]   | `PageImage.reset` / `copy`                  | instance | clear overlays / fork the image       |
|  [08]   | `PageImage.save(dest, format, quantize)`    | instance | write the rendered/annotated raster   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one `pdfplumber.open` owns parsing; `pages`/`laparams`/`password`/`repair` are call rows, and `repair=True` routes through Ghostscript before the same parse path, never a per-config loader type.
- `find_tables` is the single ruled-table surface; `vertical_strategy`/`horizontal_strategy` select the edge source as `TableSettings` rows feeding one `TableFinder` pipeline (edges -> intersections -> cells -> tables), never a finder type per strategy; `extract_tables`/`extract_table` are largest-vs-all rows over that result and `Table.extract` owns per-cell char clustering off the cell bbox.
- `extract_words`/`extract_text` forward `**kwargs` to one `WordExtractor`; `layout=True` is the positional-render row and `search(pattern)` runs a regex over the same layout textmap, never a per-mode extractor type.
- `crop`/`within_bbox`/`outside_bbox`/`filter`/`dedupe_chars` return `CroppedPage`/`FilteredPage` views sharing the container contract, and `to_csv`/`to_json`/`to_dict`/`structure_tree` project the already-parsed object stream; a region scope or export is a view row, never a re-parse.
- `to_image` renders through `pypdfium2`/`pillow`; the fluent `draw_*`/`outline_*`/`debug_tablefinder` chain overlays geometry as a debugging projection, never part of the extraction result.
- each parse folds a table receipt (reflected version, page count, per-page char/line/rect/curve/image/edge counts, detected-table count, cell-grid shape, word-cluster tolerances, resolved `TableSettings` strategy) into the one `ArtifactReceipt` family.

[STACKING]:
- `expression`(`.api/expression.md`): the parse boundary mints a `Result` — wrap `open`/`find_tables`/`extract_words` so `utils.exceptions.PdfminerException`/`MalformedPDFException` becomes `Result.Error`/`LensFault`; downstream code stays `Result`/`Option`-native and the `_node` fold is a pure transform over the `Ok` tuple.
- `anyio`(`.api/anyio.md`): pdfminer.six releases the GIL on the C inflate/parse, so `open(BytesIO(payload))` crosses `to_thread.run_sync(_plumber_recover, payload, limiter=_OFFLOAD)` under the shared `CapacityLimiter`, NOT the `to_process` pickle hop the worker-band `ocrmypdf`/`python-calamine` arms take and never inline on the loop; a `fail_after` `TimeoutError` maps to a typed `Error` at the boundary.
- `msgspec`(`.api/msgspec.md`): each recovered word/line/char/cell dict folds into the canonical `DocumentNode` (`RunNode`/`TableNode` over `_node(NodeKind, …)`) whose `msgpack` `node_digest` merkle is the only content codec; the raw `T_obj` dict is never re-encoded by a second codec.
- `beartype`(`.api/beartype.md`): narrow the untyped `T_obj`/`Table.cells` dict/tuple shapes at the seam where they enter typed `_node`/`_table_node` calls (`door.is_bearable` for optional `extra_attrs` keys), so a missing `fontname`/`size` degrades to a default rather than a `KeyError`.
- `structlog` + `opentelemetry`(`.api/structlog.md`, `.api/opentelemetry-api.md`): recovery rides the consumer's `async_boundary` span under the `@receipted` weave; a parse-fault `Result.Error` folds into one structured egress event and the `Ok` path stays silent.
- `pymupdf`(`.api/pymupdf.md`): the sibling native PDF owner — `pdfplumber` is the higher-recall ruled-table + word-geometry arm, `pymupdf` the native bulk `TableFinder` + render/extract/redact arm; they meet at PDF bytes and the shared `DocumentNode`, never re-implementing each other.

[LOCAL_ADMISSION]:
- import at boundary scope only; the module-level import is barred by the manifest import policy.
- each op's `LensSpec`/`bbox`/`needle` precondition is a `Result[Self, LensFault]` gate before any `open`, so an empty-payload or missing-bbox request fails total without parsing.

[RAIL_LAW]:
- Package: `pdfplumber`
- Owns: PDF parsing into a container hierarchy over `pdfminer.six`, ruled-line table detection with full snap/join/intersection tolerance and cell-grid extraction, tolerance- and direction-clustered word/char extraction, layout-preserving text and regex `search`, tagged-PDF `structure_tree`, `to_csv`/`to_json`/`to_dict` export, bbox crop/within/outside/filter/dedupe views, and `PageImage` raster debug overlays via `pypdfium2`/`pillow`
- Accept: ruled-table and word/char extraction feeding the table, document, and visuals owners
- Reject: wrapper-renames of `open`/`find_tables`/`extract_words`; a hand-rolled PDF tokenizer where pdfminer.six parsing is in-package; a re-implemented line-snap/intersection cell detector; an open-coded coordinate or raster renderer where `TableSettings`/`pypdfium2` own it; a parallel `Page`, `WordExtractor`, or finder type per strategy or region; reaching here for bulk page text where `pymupdf` is faster; identity or receipt shapes the runtime owner already owns
