# [PY_ARTIFACTS_API_PYTHON_PPTX]

`python-pptx` mints the PowerPoint `.pptx` presentation surface for the artifacts office rail: `Presentation(pptx=None)` opens a template/stream or creates from the default 16:9 template, `Slides.add_slide(layout)` mints layout-row slides, the one `SlideShapes.add_*` family and `build_freeform` author the shape tree, `SlidePlaceholders` fills layout placeholders in place, the `TextFrame`/`Font` model carries the run-appearance axis, the shared `FillFormat`/`LineFormat`/`ShadowFormat`/`ColorFormat` DML surface owns fill/outline/shadow/color, the `CategoryChartData`/`XyChartData`/`BubbleChartData` builders feed `add_chart` with `Chart.replace_data` refresh, the `Table`/`_Cell` grid owns cell merge/banding, and `Length` value objects own bidirectional EMU conversion. It anchors the `document/emit#DOCUMENT` `DocumentMode.PPTX` lowering arm and the `DocumentPlan.bound` template-clone fan, never re-implementing the OOXML part graph, lxml serialization, the embedded chart-data workbook, or the slide-layout/master inheritance python-pptx already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-pptx`
- package: `python-pptx`
- import: `pptx`
- owner: `artifacts`
- rail: office
- floor: pure-Python, abi-agnostic; runs on cp315, offloadable to the `anyio.to_process` worker band
- runtime deps: `lxml`, `Pillow`, `XlsxWriter`, `typing_extensions`
- license: MIT (Steve Canny)
- entry points: none (library only; the in-process `Presentation` surface composes directly)
- capability: `.pptx` construction and editing — slides from layouts, masters/notes-master, placeholder fill-in-place, textboxes, runs/paragraphs/fonts with full character appearance, cropped pictures with image evidence, tables with merge/split/banding, native category/xy/bubble charts with axis/legend/data-label/series formatting and `replace_data` refresh, autoshapes with adjustment handles, connectors, groups, movie/OLE embeds, freeform vector shapes, the DML fill/line/shadow/color surface, click-action hyperlinks, notes, core-property metadata, slide background, EMU `Length` geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: presentation root, slides, layouts (`pptx.presentation`, `pptx.slide`)

`Presentation()` returns the document root; a slide is a `SlideLayout`-row instance via `Slides.add_slide`, never a per-template type. `Slides.get(slide_id)`/`index(slide)` address by id/position, and layouts, masters, and notes-master read off the root. `CoreProperties` seals the OOXML core fields — `author`/`title`/`subject`/`keywords`/`category`/`comments`/`content_status`/`created`/`modified`/`last_modified_by`/`last_printed`/`identifier`/`language`/`revision`/`version`.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]     | [CAPABILITY]                                                                                     |
| :-----: | :--------------- | :---------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `Presentation`   | presentation root | `slides`/`slide_layouts`/`slide_master(s)`/`notes_master`/`core_properties`/`save`               |
|  [02]   | `Slides`         | slide collection  | `add_slide(layout)`/`get(slide_id)`/`index(slide)` + iteration                                   |
|  [03]   | `Slide`          | slide unit        | `shapes`/`placeholders`/`slide_layout`/`slide_id`/`name`/`background`/`follow_master_background` |
|  [04]   | `SlideLayout`    | layout            | `placeholders`/`shapes`/`slide_master`/`used_by_slides`/`iter_cloneable_placeholders`/`name`     |
|  [05]   | `SlideLayouts`   | layout collection | indexable master layouts (index 6 is `Blank` on the default master)                              |
|  [06]   | `SlideMaster`    | master            | `slide_layouts`/`placeholders`/`shapes`/`background`/`name`                                      |
|  [07]   | `NotesSlide`     | notes slide       | `notes_text_frame`/`notes_placeholder` speaker notes; `clone_master_placeholders`                |
|  [08]   | `CoreProperties` | metadata seal     | the OOXML core-property fields (lead); read via `presentation.core_properties`                   |

[PUBLIC_TYPE_SCOPE]: shape tree, shapes, placeholders, freeform (`pptx.shapes`)

`SlideShapes` (and the same surface on `GroupShapes`) is the one `add_*` shape-add collection with `build_freeform`; each shape kind is a method row, never a parallel slide type. `SlidePlaceholders` fills layout-defined placeholders in place. Every concrete shape carries the `BaseShape` geometry/identity axis: `left`/`top`/`width`/`height`/`rotation`, `name`/`shape_id`/`shape_type`, `is_placeholder`/`placeholder_format`, `has_text_frame`/`has_chart`/`has_table`, `click_action`, `shadow`.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]    | [CAPABILITY]                                                                           |
| :-----: | :------------------- | :--------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `SlideShapes`        | shape tree       | `placeholders`/`title`/`index`/`turbo_add_enabled`/`clone_layout_placeholders`         |
|  [02]   | `GroupShapes`        | group shape tree | same `add_*`/`build_freeform` inside a group (no `add_table`; tables are slide-level)  |
|  [03]   | `BaseShape`          | shape base       | the geometry/identity axis every shape carries (enumerated in the lead)                |
|  [04]   | `Shape`              | autoshape        | `text_frame`/`text`/`fill`/`line`/`shadow`/`adjustments`/`auto_shape_type`             |
|  [05]   | `Picture`            | picture          | `image`/`crop_left`/`crop_top`/`crop_right`/`crop_bottom`/`line`/`auto_shape_type`     |
|  [06]   | `Movie`              | movie            | `media_type`/`media_format`/`poster_frame`; embedded video graphic frame               |
|  [07]   | `Connector`          | connector        | `begin_x`/`begin_y`/`end_x`/`end_y`, `begin_connect(shape, idx)`/`end_connect`, `line` |
|  [08]   | `GraphicFrame`       | graphic frame    | `table`/`has_table`, `chart`/`chart_part`/`has_chart`, `ole_format`                    |
|  [09]   | `GroupShape`         | group shape      | `shapes` (nested `GroupShapes`) + the `BaseShape` bounding-box geometry                |
|  [10]   | `FreeformBuilder`    | freeform builder | `add_line_segments`/`move_to`/`convert_to_shape`, `shape_offset_x`/`shape_offset_y`    |
|  [11]   | `PicturePlaceholder` | placeholder      | `insert_picture` fills a layout picture placeholder in place                           |
|  [12]   | `ChartPlaceholder`   | placeholder      | `insert_chart` fills a layout chart placeholder                                        |
|  [13]   | `TablePlaceholder`   | placeholder      | `insert_table` fills a layout table placeholder                                        |
|  [14]   | `_PlaceholderFormat` | placeholder id   | `idx`/`type` (`PP_PLACEHOLDER`) — the layout-placeholder discriminant                  |

[PUBLIC_TYPE_SCOPE]: text model, DML format, image evidence (`pptx.text`, `pptx.dml`, `pptx.parts.image`)

A shape's `TextFrame` owns paragraphs/runs and sizing; `Font` is one font value per run with `color`/`fill` and the full appearance axis. One shared `FillFormat`/`LineFormat`/`ShadowFormat`/`ColorFormat` DML surface grades every fill (solid/gradient/pattern/picture), outline, shadow, and RGB-or-theme color: `FillFormat` carries `solid()`/`gradient()`/`patterned()`/`background()` mode setters with `fore_color`/`back_color`/`pattern`/`gradient_stops`/`gradient_angle`/`type` (`MSO_FILL`).

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]  | [CAPABILITY]                                                                                     |
| :-----: | :------------- | :------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `TextFrame`    | text frame     | `paragraphs`/`add_paragraph`/`text`/`clear`; the sizing axis + `fit_text` in entrypoints         |
|  [02]   | `_Paragraph`   | paragraph      | `runs`/`text`/`font`, `line_spacing`/`space_before`/`space_after`, `clear`                       |
|  [03]   | `_Run`         | run            | `text`, `font` (Font), `hyperlink` (_Hyperlink)                                                  |
|  [04]   | `Font`         | font           | `bold`/`italic`/`underline`/`size`/`name`/`language_id`/`color`/`fill`                           |
|  [05]   | `_Hyperlink`   | run hyperlink  | `address` (the external URL or internal target the run links to)                                 |
|  [06]   | `ColorFormat`  | color          | `rgb` (RGBColor) / `theme_color` (MSO_THEME_COLOR) / `brightness` / `type` (MSO_COLOR_TYPE)      |
|  [07]   | `RGBColor`     | rgb value      | `RGBColor(r, g, b)` int-triple subclass; `RGBColor.from_string('RRGGBB')`; `str()` -> 6-hex      |
|  [08]   | `FillFormat`   | fill           | `solid`/`gradient`/`patterned`/`background` fill modes; members in the DML lead                  |
|  [09]   | `LineFormat`   | outline        | `color` (ColorFormat), `fill` (FillFormat), `width` (Length), `dash_style` (MSO_LINE_DASH_STYLE) |
|  [10]   | `ShadowFormat` | shadow         | `inherit` (toggle inheriting the theme shadow vs. an explicit no-shadow)                         |
|  [11]   | `Image`        | image evidence | `blob`/`content_type`/`ext`/`dpi`/`size`/`sha1`/`filename`; `Image.from_blob`/`from_file`        |

[PUBLIC_TYPE_SCOPE]: chart, table, and unit value objects (`pptx.chart`, `pptx.table`, `pptx.util`)

`ChartData` builders feed `add_chart`; the resulting `Chart` exposes the axis/legend/series/data-label formatting surface and `replace_data` for in-place refresh. `Table`/`_Cell` is the schedule grid. `Length` owns bidirectional EMU conversion (`Inches(1) == 914400`, `Pt(1) == 12700`).

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                                          |
| :-----: | :------------------ | :------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `CategoryChartData` | chart data    | `add_category`/`add_series`; `categories`/`number_format`/`xlsx_blob`                 |
|  [02]   | `XyChartData`       | chart data    | `add_series` -> `.add_data_point(x, y)`; scatter/xy                                   |
|  [03]   | `BubbleChartData`   | chart data    | `add_series` -> `.add_data_point(x, y, size)`; bubble                                 |
|  [04]   | `Chart`             | chart         | `chart_type` (`XL_CHART_TYPE`), `replace_data`, axes/legend/title/font (roster below) |
|  [05]   | `ValueAxis`         | axis          | scale/unit/gridline/tick-label surface (roster below); `CategoryAxis` is the sibling  |
|  [06]   | `DataLabels`        | data labels   | `number_format`, `position`, per-field `show_*` toggles, `font` (roster below)        |
|  [07]   | `Legend`            | legend        | `position` (`XL_LEGEND_POSITION`)/`include_in_layout`/`horz_offset`/`font`            |
|  [08]   | `Table`             | table         | `cell(row, col)`, `rows`/`columns`, banding toggles (roster below)                    |
|  [09]   | `_Cell`             | table cell    | `text`/`text_frame`, `fill`, `merge`/`split`, anchor/margins (roster below)           |
|  [10]   | `Length`            | unit base     | `Inches`/`Pt`/`Cm`/`Mm`/`Emu`/`Centipoints` ctors; `.emu`/`.pt`/`.inches`/… read-back |

- [04]-[CHART]: `chart_type` (`XL_CHART_TYPE`), `plots`, `series`, `category_axis`/`value_axis`, `has_legend`/`legend`, `has_title`/`chart_title`, `font`, `chart_style`, `replace_data(chart_data)`.
- [05]-[VALUEAXIS]: `minimum_scale`/`maximum_scale`, `major_unit`/`minor_unit`, `major_gridlines`/`has_major_gridlines`, `tick_labels`, `axis_title`, `format` (`ChartFormat`), `visible`.
- [06]-[DATALABELS]: `number_format`/`number_format_is_linked`, `position` (`XL_LABEL_POSITION`), `show_value`/`show_category_name`/`show_series_name`/`show_percentage`/`show_legend_key`, `font`.
- [08]-[TABLE]: `cell(row, col)`, `rows`/`columns`, `iter_cells`, `first_row`/`last_row`/`first_col`/`last_col` banding toggles, `horz_banding`/`vert_banding`.
- [09]-[_Cell]: `text`/`text_frame`, `fill`, `merge(other_cell)`/`split()`/`is_merge_origin`/`is_spanned`/`span_height`/`span_width`, `vertical_anchor`, `margin_*`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: presentation factory, slides, metadata, save

`Presentation(pptx=None)` is the single open-or-create factory (the `DocumentPlan.bound` template-clone path opens a `.pptx` template). `Slides.add_slide(layout)` is the single slide-add surface, the layout sourced from `slide_layouts`. `save` accepts a path OR a stream — the `document/emit#DOCUMENT` worker saves to `io.BytesIO` and only the bytes cross the seam. `core_properties` is the author/title/created/modified metadata seal.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                                                       |
| :-----: | :----------------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `Presentation`                 | `Presentation(pptx: str \| IO[bytes] \| None = None) -> presentation.Presentation` |
|  [02]   | `Presentation.save`            | `save(file: str \| IO[bytes])`                                                     |
|  [03]   | `Presentation.slides`          | `slides -> Slides`                                                                 |
|  [04]   | `Slides.add_slide`             | `add_slide(slide_layout: SlideLayout) -> Slide`                                    |
|  [05]   | `Slides.get` / `index`         | `get(slide_id, default=None) -> Slide \| None` / `index(slide) -> int`             |
|  [06]   | `Presentation.slide_layouts`   | `slide_layouts -> SlideLayouts` (`[idx]`; index 6 == `Blank`)                      |
|  [07]   | `Presentation.slide_width`     | `slide_width` / `slide_height -> Length` (settable)                                |
|  [08]   | `Presentation.core_properties` | `core_properties -> CoreProperties`                                                |
|  [09]   | `Slide.notes_slide`            | `notes_slide.notes_text_frame -> TextFrame` (guard with `has_notes_slide`)         |

[ENTRYPOINT_SCOPE]: shape and freeform authoring

Shape rows take `Inches`/`Pt`/`Emu` `Length` position/size and return the created shape or graphic frame. `add_picture`/`add_movie`/`add_ole_object` accept a path OR an `IO[bytes]` stream, so the `DocumentPlan.bound` `assets` band embeds figures from `io.BytesIO` with no temp file. `[SURFACE]` drops the `SlideShapes.`/`FreeformBuilder.`/`*Placeholder.` prefix, and each `add_*` row's trailing `left, top, width[, height]` `Length` geometry shows as `…`. Autoshape kind is an `MSO_SHAPE` row, connector an `MSO_CONNECTOR` (STRAIGHT/ELBOW/CURVE).

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                                                                                                 |
| :-----: | :------------------ | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | `add_textbox`       | `add_textbox(…) -> Shape`                                                                                    |
|  [02]   | `add_picture`       | `add_picture(image_file: str \| IO[bytes], …, width=None, height=None) -> Picture`                           |
|  [03]   | `add_table`         | `add_table(rows, cols, …) -> GraphicFrame`                                                                   |
|  [04]   | `add_chart`         | `add_chart(chart_type: XL_CHART_TYPE, x, y, cx, cy, chart_data: ChartData) -> Chart`                         |
|  [05]   | `add_shape`         | `add_shape(autoshape_type_id: MSO_SHAPE, …) -> Shape`                                                        |
|  [06]   | `add_connector`     | `add_connector(connector_type: MSO_CONNECTOR, begin_x, begin_y, end_x, end_y) -> Connector`                  |
|  [07]   | `add_group_shape`   | `add_group_shape(shapes: Iterable[BaseShape] = ()) -> GroupShape`                                            |
|  [08]   | `add_movie`         | `add_movie(movie_file, …, poster_frame_image=None, mime_type='video/unknown') -> GraphicFrame`               |
|  [09]   | `add_ole_object`    | `add_ole_object(object_file, prog_id, …, icon_file=None, icon_width=None, icon_height=None) -> GraphicFrame` |
|  [10]   | `build_freeform`    | `build_freeform(start_x=0, start_y=0, scale: tuple[float, float] \| float = 1.0) -> FreeformBuilder`         |
|  [11]   | `add_line_segments` | `add_line_segments(vertices: Iterable[tuple[float, float]], close=True)`                                     |
|  [12]   | `move_to`           | `move_to(x: float, y: float)`                                                                                |
|  [13]   | `convert_to_shape`  | `convert_to_shape(origin_x: Length = 0, origin_y: Length = 0) -> FreeformShape`                              |
|  [14]   | `insert_picture`    | `insert_picture(image_file) -> PlaceholderPicture`                                                           |
|  [15]   | `insert_chart`      | `insert_chart(chart_type: XL_CHART_TYPE, chart_data: ChartData) -> PlaceholderGraphicFrame`                  |
|  [16]   | `insert_table`      | `insert_table(rows, cols) -> PlaceholderGraphicFrame`                                                        |

[ENTRYPOINT_SCOPE]: text, style, chart-data, and table authoring

A shape's `TextFrame` owns paragraphs/runs and fit; `Font` carries the full character appearance the `document/emit#DOCUMENT` RUN_FIDELITY law projects. `ChartData` builders feed `add_chart`/`insert_chart` and `Chart.replace_data` refreshes in place; categories precede series on `CategoryChartData`. `Table.cell` addresses the schedule grid.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                                                             |
| :-----: | :------------------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `TextFrame.add_paragraph`        | `add_paragraph() -> _Paragraph`                                                          |
|  [02]   | `TextFrame.fit_text`             | `fit_text(font_family='Calibri', max_size=18, bold=False, italic=False, font_file=None)` |
|  [03]   | `TextFrame` sizing               | `auto_size` (MSO_AUTO_SIZE) / `word_wrap` / `vertical_anchor` (MSO_ANCHOR) / `margin_*`  |
|  [04]   | `_Paragraph.add_run`             | `add_run() -> _Run` (also `add_line_break()`, `alignment=PP_ALIGN.*`, `level=`)          |
|  [05]   | `_Run.font` / `_Run.hyperlink`   | `font -> Font` / `hyperlink.address = url`                                               |
|  [06]   | `Font` appearance                | `bold`/`italic`/`underline`/`size`/`name`/`language_id`, `color.rgb`, `fill.solid()`     |
|  [07]   | `CategoryChartData.add_category` | `add_category(label)`                                                                    |
|  [08]   | `CategoryChartData.add_series`   | `add_series(name, values=(), number_format=None)`                                        |
|  [09]   | `Chart.replace_data`             | `replace_data(chart_data: ChartData)`                                                    |
|  [10]   | `Table.cell` / `_Cell.merge`     | `cell(row, col).text = ...` / `cell(a).merge(cell(b))` / `.split()`                      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- presentation: `Presentation(pptx=None)` is the single polymorphic open-or-create factory — `None` builds from the default 16:9 template, a path/stream opens the `DocumentPlan.bound` `DocumentMode.PPTX` template-clone path that appends slides into a corporate `.pptx`'s layouts — returning the `presentation.Presentation` object, never a reader/writer split.
- slide: `slides.add_slide(layout)` is the single slide-add surface, the layout sourced from `slide_layouts` (index 6 == `Blank` on the default master), so a slide is a layout-row instance, never a per-template type; `Slides.get(slide_id)`/`index(slide)` address by id/position.
- shape: `add_textbox`/`add_picture`/`add_table`/`add_chart`/`add_shape`/`add_connector`/`add_group_shape`/`add_movie`/`add_ole_object` is one shape-add surface, each kind a method row; `SlidePlaceholders` `insert_picture`/`insert_chart`/`insert_table` fills a layout-defined frame in place, while explicit `add_*`+`Length` geometry serves free positioning. Vector geometry is `build_freeform` -> `add_line_segments`/`move_to` -> `convert_to_shape`.
- text: a shape's `TextFrame` `paragraphs`/`add_paragraph` and `_Run`/`add_run`/`add_line_break` carry text, `fit_text`/`auto_size`/`word_wrap`/`vertical_anchor`/`margin_*` own sizing, and `Font` is one font value per run carrying the full appearance — `bold`/`italic`/`underline`/`size`/`name`/`language_id`, `color` (`ColorFormat` `rgb`/`theme_color`), `fill` (`FillFormat`), `_Run.hyperlink.address` — so the RUN_FIDELITY projection lands without per-run duplication.
- DML: shape/cell/run styling is the shared `FillFormat`/`LineFormat`/`ShadowFormat`/`ColorFormat` surface — `fill.solid()`/`gradient()`/`patterned()` with `fore_color`/`back_color`/`pattern` (MSO_PATTERN) on the `MSO_FILL` modes, `line.color`/`width`/`dash_style` (MSO_LINE_DASH_STYLE), `shadow.inherit`, `color.rgb`/`theme_color`/`brightness`. Color is `RGBColor(r, g, b)` / `RGBColor.from_string('RRGGBB')` or an `MSO_THEME_COLOR` row.
- chart: build a `CategoryChartData` (categories first via `add_category`, then `add_series(name, values, number_format)`) or `XyChartData`/`BubbleChartData` (series then `add_data_point`), then `add_chart(XL_CHART_TYPE, x, y, cx, cy, chart_data)` (or `ChartPlaceholder.insert_chart`); `Chart.replace_data(chart_data)` refreshes in place. `value_axis`/`category_axis` carry `minimum_scale`/`maximum_scale`/`major_unit`/gridlines, `legend.position` is `XL_LEGEND_POSITION`, `DataLabels.position` is `XL_LABEL_POSITION`.
- table: `add_table(rows, cols, ...)` returns a `GraphicFrame` whose `.table` is the `Table`; `table.cell(r, c).text`/`.text_frame` authors a cell, `cell(a).merge(cell(b))`/`.split()` spans/unspans, and `first_row`/`first_col`/`horz_banding`/`vert_banding` toggle style banding — the AEC schedule grid.
- unit: positions and sizes use `Inches`/`Pt`/`Emu`/`Cm`/`Mm`/`Centipoints` value objects reading back as `.emu`/`.pt`/`.inches`/`.cm`/`.mm`; autoshape geometry is `MSO_SHAPE`, connector `MSO_CONNECTOR`, text anchor `MSO_ANCHOR`, alignment `PP_ALIGN`, placeholder `PP_PLACEHOLDER`, underline `MSO_TEXT_UNDERLINE_TYPE`.

[STACKING]:
- `msgspec`(`.api/msgspec.md`): each presentation op captures slide/shape/image/chart counts, per-embed `Picture.image.sha1`/`size`/`content_type`, and output byte length as a `msgspec.Struct` office receipt — the `core/receipt#RECEIPT` `ArtifactReceipt.Office` case.
- `structlog`(`.api/structlog.md`) + `opentelemetry-api`(`.api/opentelemetry-api.md`): the build emits one `structlog` event inside one OpenTelemetry span stamping the receipt counts, never re-derived off the bytes by a second reader.
- `anyio`(`.api/anyio.md`): the build authors on the `anyio.to_process` sub-3.15 worker band; only the saved `.pptx` bytes (or a stream) with the typed receipt cross the seam, never a live `Presentation`.
- `XlsxWriter`(`.api/xlsxwriter.md`): the chart-data workbook embeds through the in-package `XlsxWriter` writer.
- `Pillow`(`.api/pillow.md`): sizes embedded images and exposes `Picture.image` evidence.
- `puremagic`/`python-magic`(`.api/puremagic.md`): `exchange/detect#DETECT` routes `MediaClass.PRESENTATION` here at the ingest gate.
- `msoffcrypto-tool`(`.api/msoffcrypto-tool.md`): decrypts an encrypted container to a plaintext stream `Presentation(stream)` reads and re-seals on save.
- `matplotlib`/`vl-convert-python`/`pyvista`(`.api/matplotlib.md`): externally-rendered chart imagery and the `DocumentPlan.bound` `assets` band land as a `Picture` via `add_picture` from `io.BytesIO`.
- `document/emit#DOCUMENT` `PPTX` arm: the `DocumentMode.PPTX` lowering arm and the `DocumentPlan.bound` template-clone fan compose `Presentation`, `add_slide`, the add+placeholder-insert families, `build_freeform`, the `Font`+DML surface, the chart-data builders + `replace_data`, the `Table` grid, `CoreProperties`, and the unit objects, saving to `io.BytesIO`.

[LOCAL_ADMISSION]:
- python-pptx is the owner for `.pptx` construction and editing; Word routes to `python-docx`, Excel to `openpyxl`/`xlsxwriter`, ODF presentation to `odfpy` (`DocumentMode.ODT`/`ODS`), and live UI stays outside this package.

[RAIL_LAW]:
- Package: `python-pptx`
- Owns: `.pptx` construction and editing — slides from layouts, placeholder fill-in-place, textboxes/runs/paragraphs/fonts with full character appearance, pictures with crop and image evidence, tables with merge/split/banding, native category/xy/bubble charts with axis/legend/data-label/series formatting and `replace_data` refresh, autoshapes/connectors/groups, movie/OLE embeds, freeform vector shapes, the FillFormat/LineFormat/ShadowFormat DML surface, slide background, notes, core-property metadata
- Accept: presentation authoring feeding the `document/emit#DOCUMENT` `DocumentMode.PPTX` lowering arm and the `DocumentPlan.bound` template-clone fan row, downstream of the `exchange/detect#DETECT` `MediaClass.PRESENTATION` gate and the `msoffcrypto-tool` confidentiality edge, embedding the `DocumentPlan.bound` `assets` figure band via `add_picture` from a stream
- Reject: wrapper-renames of `add_slide`/`add_picture`/`save`; an `add_*`-per-shape parallel slide type where the one `SlideShapes` collection discriminates by method row; a per-run color/font rebuild where `Font` + `ColorFormat` carry the appearance; raw-EMU integers where `Length` value objects and their read-back exist; hex-string color where `RGBColor`/`MSO_THEME_COLOR` exist; magic shape/anchor/alignment/dash strings where the `MSO_SHAPE`/`MSO_ANCHOR`/`PP_ALIGN`/`MSO_LINE_DASH_STYLE` rows exist; hand-built path or table XML where `build_freeform` and `add_table`/`Table.cell` exist; a separate chart-data spreadsheet where the embedded `XlsxWriter` workbook is in-package; identity minting the runtime owns
