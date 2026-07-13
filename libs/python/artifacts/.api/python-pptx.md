# [PY_ARTIFACTS_API_PYTHON_PPTX]

`python-pptx` supplies the PowerPoint `.pptx` presentation surface for the artifacts office rail: the polymorphic `Presentation(pptx=None)` factory opens an existing file/stream (the template-clone path) or creates from the default 16:9 template, returning a `presentation.Presentation` whose `Slides.add_slide(layout)` collection mints slides from `SlideLayouts` rows, the full `SlideShapes.add_*` shape family (textbox, picture, table, chart, autoshape, connector, group, movie, OLE object) plus `build_freeform` author the shape tree, the `SlidePlaceholders` `insert_picture`/`insert_chart`/`insert_table` family fills layout-defined placeholders in place, the `TextFrame`/`_Paragraph`/`_Run`/`Font` model carries paragraphs/runs with the full character-appearance axis (bold/italic/underline/size/name/`language_id`/`color`/`fill`/`hyperlink`), the `FillFormat`/`LineFormat`/`ShadowFormat`/`ColorFormat` DML surface owns fill (solid/gradient/pattern/picture), outline (width/dash/color), shadow, and RGB-or-theme color, native `CategoryChartData`/`XyChartData`/`BubbleChartData` series builders feed `add_chart` with the embedded `XlsxWriter` workbook and `Chart.replace_data` refresh, the `Table`/`_Cell` grid owns cell text/merge/split/banding, and `Inches`/`Pt`/`Cm`/`Mm`/`Emu`/`Centipoints`/`Length` value objects own EMU conversion in both directions. The package owner composes `Presentation`, `add_slide`, the `add_*` + placeholder-insert families, `build_freeform`, the text-frame model with `fit_text`/`auto_size`/`word_wrap`/`vertical_anchor`/margins, the `Font`+`ColorFormat`+`FillFormat`+`LineFormat`+`ShadowFormat` DML surface, the chart-data builders + `replace_data`, the `Table` grid, the `CoreProperties` metadata seal, and the unit objects into the `document/emit#DOCUMENT` `DocumentMode.PPTX` lowering arm and the `document/emit#DOCUMENT` `DocumentPlan.bound` `DocumentMode.PPTX` template-clone fan row; it removes any raw-EMU integer math because the `Length` value objects own conversion, and it never re-implements the OOXML presentation part graph, the lxml-backed element serialization, the embedded chart-data workbook, or the slide-layout/master inheritance python-pptx already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-pptx`
- package: `python-pptx`
- import: `pptx`
- owner: `artifacts`
- rail: office
- installed: `1.0.2`
- floor: pure-Python (no compiled extension); abi-agnostic, runs on cp315
- runtime deps: `lxml` (`libs/python/artifacts/.api/lxml.md`, the C14N OOXML element serialization backing every part), `Pillow` (`libs/python/artifacts/.api/pillow.md`, image inspection sizing embedded pictures and rendering the poster frame), `XlsxWriter` (`libs/python/artifacts/.api/xlsxwriter.md`, the embedded chart-data workbook writer), `typing_extensions`
- license: MIT (Steve Canny) — permissive, no copyleft gate; aligns with the MIT/BSD sibling office owners (`xlsxwriter`/`openpyxl`/`python-docx`), distinct from the triple-classified `odfpy`
- entry points: none (library only; the in-process `Presentation` surface composes directly — no CLI subprocess)
- capability: `.pptx` construction and editing — slides from layouts, layouts/masters/notes-master, placeholder fill-in-place, textboxes, runs/paragraphs/fonts with full character appearance (bold/italic/underline/size/name/`language_id`/color/fill/hyperlink), pictures (with crop) and image evidence, tables with merge/split/banding, native category/xy/bubble charts with axis/legend/data-label/series formatting and `replace_data` refresh, autoshapes (182 `MSO_SHAPE` rows) with adjustment handles, connectors (begin/end connect), group shapes, movie and OLE-object embeds, freeform vector shapes, the FillFormat/LineFormat/ShadowFormat DML surface, click-action hyperlinks, speaker notes, core-property metadata, slide background, and offscreen slide-shape geometry in EMU `Length` value objects

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: presentation root, slides, layouts
- rail: office — `pptx.presentation` / `pptx.slide`

`Presentation()` returns the document root; a slide is a `SlideLayout`-row instance via `Slides.add_slide`, never a per-template slide type. Layouts/masters/notes-master are read off the root; `Slides.get(slide_id)`/`index(slide)` address by id/position. `CoreProperties` seals the 15 OOXML core fields — `author`/`title`/`subject`/`keywords`/`category`/`comments`/`content_status`/`created`/`modified`/`last_modified_by`/`last_printed`/`identifier`/`language`/`revision`/`version`.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]     | [CAPABILITY]                                                                                     |
| :-----: | :--------------- | :---------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `Presentation`   | presentation root | `slides`/`slide_layouts`/`slide_master(s)`/`notes_master`/`core_properties`/`save`               |
|  [02]   | `Slides`         | slide collection  | `add_slide(layout)`/`get(slide_id)`/`index(slide)` + iteration                                   |
|  [03]   | `Slide`          | slide unit        | `shapes`/`placeholders`/`slide_layout`/`slide_id`/`name`/`background`/`follow_master_background` |
|  [04]   | `SlideLayout`    | layout            | `placeholders`/`shapes`/`slide_master`/`used_by_slides`/`iter_cloneable_placeholders`/`name`     |
|  [05]   | `SlideLayouts`   | layout collection | indexable master layouts (default master ships 11; index 6 is `Blank`)                           |
|  [06]   | `SlideMaster`    | master            | `slide_layouts`/`placeholders`/`shapes`/`background`/`name`                                      |
|  [07]   | `NotesSlide`     | notes slide       | `notes_text_frame`/`notes_placeholder` speaker notes; `clone_master_placeholders`                |
|  [08]   | `CoreProperties` | metadata seal     | the 15 OOXML core-property fields (lead); read via `presentation.core_properties`                |

[PUBLIC_TYPE_SCOPE]: shape tree, shapes, placeholders, freeform
- rail: office — `pptx.shapes`

`SlideShapes` (and the same surface on `GroupShapes`) is the one `add_*` shape-add collection plus `build_freeform`; each shape kind is a method row, never a parallel slide type. `SlidePlaceholders` fills layout-defined placeholders in place. Every concrete shape carries the `BaseShape` geometry/identity axis: `left`/`top`/`width`/`height`/`rotation`, `name`/`shape_id`/`shape_type`, `is_placeholder`/`placeholder_format`, `has_text_frame`/`has_chart`/`has_table`, `click_action`, `shadow`.

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

[PUBLIC_TYPE_SCOPE]: text model, DML format, image evidence
- rail: office — `pptx.text` / `pptx.dml` / `pptx.parts.image`

A shape's `TextFrame` owns paragraphs/runs and sizing; `Font` is one font value per run with `color`/`fill` and the full appearance axis. The shared `FillFormat`/`LineFormat`/`ShadowFormat`/`ColorFormat` DML surface grades the office plane as a journal-grade and ISO-3098 drawing-annotation engine: `FillFormat` carries `solid()`/`gradient()`/`patterned()`/`background()` mode setters with `fore_color`/`back_color`/`pattern`/`gradient_stops`/`gradient_angle`/`type` (`MSO_FILL`).

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

[PUBLIC_TYPE_SCOPE]: chart, table, and unit value objects
- rail: office — `pptx.chart` / `pptx.table` / `pptx.util`

`ChartData` builders feed `add_chart`; the resulting `Chart` exposes the axis/legend/series/data-label formatting surface and `replace_data` for in-place refresh. `Table`/`_Cell` is the schedule grid. The `Length` family owns bidirectional EMU conversion (`Inches(1) == 914400`, `Pt(1) == 12700`).

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
- rail: office

`Presentation(pptx=None)` is the single open-or-create factory (the `document/emit#DOCUMENT` `DocumentPlan.bound` template-clone path opens a `.pptx` template); `Slides.add_slide(layout)` is the single slide-add surface, the layout sourced from `slide_layouts`. `save` accepts a path OR a stream — the `document/emit#DOCUMENT` worker saves to `io.BytesIO` and only the bytes cross the seam. `core_properties` is the author/title/created/modified metadata seal.

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
- rail: office

Shape rows take `Inches`/`Pt`/`Emu` `Length` position/size values; the add family returns the created shape or graphic frame. `add_picture`/`add_movie`/`add_ole_object` accept a path OR an `IO[bytes]` stream, so the `document/emit#DOCUMENT` `DocumentPlan.bound` `assets` band embeds figures from `io.BytesIO` with no temp-file materialization. `build_freeform` mints a vector-path builder taking a per-axis `scale` tuple. `[SURFACE]` drops the `SlideShapes.`/`FreeformBuilder.`/`*Placeholder.` prefix; each `add_*` row carries the trailing `left, top, width[, height]` `Length` geometry shown as `…`. Autoshapes are one of 182 `MSO_SHAPE` rows, connectors an `MSO_CONNECTOR` (STRAIGHT/ELBOW/CURVE).

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
- rail: office

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

[OFFICE_PPTX]:
- import: `from pptx import Presentation; from pptx.util import Inches, Pt, Emu` at boundary scope only; module-level import is banned by the manifest import policy. The distribution is `python-pptx`, the import name is `pptx`.
- presentation axis: `Presentation(pptx=None)` is the single polymorphic factory for both open and create — `None` creates from the default 16:9 template, a path/stream opens (the `document/emit#DOCUMENT` `DocumentPlan.bound` `DocumentMode.PPTX` template-clone path that opens a corporate `.pptx` and appends slides into its layouts). It returns the `presentation.Presentation` object, never a parallel reader/writer split.
- slide axis: `Presentation.slides.add_slide(layout)` is the single slide-add surface; the layout comes from `slide_layouts` (index 6 == `Blank` on the default master), so a slide is a layout-row instance, never a per-template slide type. `Slides.get(slide_id)`/`index(slide)` address by id/position.
- shape axis: `add_textbox`/`add_picture`/`add_table`/`add_chart`/`add_shape`/`add_connector`/`add_group_shape`/`add_movie`/`add_ole_object` is one shape-add surface on `slide.shapes`; each shape kind is a method row, never a parallel slide type. The layout-driven mirror is `SlidePlaceholders` `insert_picture`/`insert_chart`/`insert_table` filling a layout-defined placeholder in place — prefer placeholder-insert when the layout defines the frame, reserve the explicit `add_*`+`Length` geometry for free positioning. Vector geometry is `build_freeform` -> `add_line_segments`/`move_to` -> `convert_to_shape`, never hand-built path XML.
- text axis: a shape exposes a `TextFrame` whose `paragraphs`/`add_paragraph` and `_Run`/`add_run`/`add_line_break` carry text; `fit_text`/`auto_size`/`word_wrap`/`vertical_anchor`/`margin_*` own sizing. `Font` is one font value per run carrying the FULL character appearance — `bold`/`italic`/`underline`/`size`/`name`/`language_id`, `color` as a `ColorFormat` (`rgb` or `theme_color`), `fill` as a `FillFormat`, and `_Run.hyperlink.address` the link target — so the `document/emit#DOCUMENT` RUN_FIDELITY projection (bold-italic-superscript-coloured-language-tagged runs) lands without per-run duplication.
- DML axis: shape/cell/run styling is the shared `FillFormat`/`LineFormat`/`ShadowFormat`/`ColorFormat` surface — `fill.solid()`/`gradient()`/`patterned()` with `fore_color`/`back_color`/`pattern` (MSO_PATTERN) on the closed `MSO_FILL` modes, `line.color`/`width`/`dash_style` (MSO_LINE_DASH_STYLE), `shadow.inherit`, and `color.rgb`/`theme_color`/`brightness`. Color is `RGBColor(r, g, b)` (or `RGBColor.from_string('RRGGBB')`) or an `MSO_THEME_COLOR` row, never a hex string literal.
- chart axis: build a `CategoryChartData` (categories first via `add_category`, then `add_series(name, values, number_format)`) or `XyChartData`/`BubbleChartData` (series then `add_data_point`), then `add_chart(XL_CHART_TYPE, x, y, cx, cy, chart_data)` (or `ChartPlaceholder.insert_chart`); `Chart.replace_data(chart_data)` refreshes in place. Chart type is one of 73 `XL_CHART_TYPE` rows; `value_axis`/`category_axis` carry `minimum_scale`/`maximum_scale`/`major_unit`/gridlines, `legend.position` is `XL_LEGEND_POSITION`, `DataLabels.position` is `XL_LABEL_POSITION`. The chart-data workbook is embedded via the `XlsxWriter` dependency, never a separate spreadsheet file.
- table axis: `add_table(rows, cols, ...)` returns a `GraphicFrame` whose `.table` is the `Table`; `table.cell(r, c).text`/`.text_frame` authors a cell, `cell(a).merge(cell(b))`/`.split()` spans/unspans, and `first_row`/`first_col`/`horz_banding`/`vert_banding` toggle the table-style banding — the AEC schedule grid, never a hand-built `a:tbl`.
- unit axis: positions and sizes use `Inches`/`Pt`/`Emu`/`Cm`/`Mm`/`Centipoints` value objects, never raw EMU ints; `Length` reads back as `.emu`/`.pt`/`.inches`/`.cm`/`.mm`. Autoshape geometry is an `MSO_SHAPE` row, connector kind an `MSO_CONNECTOR` row, text anchor an `MSO_ANCHOR` row, alignment a `PP_ALIGN` row, placeholder kind a `PP_PLACEHOLDER` row, underline an `MSO_TEXT_UNDERLINE_TYPE` row.
- evidence: each presentation op captures slide count (`len(presentation.slides)`), shape count, image count (with `Picture.image.sha1`/`size`/`content_type` per embed), chart count, and output byte length as a `msgspec.Struct` (`libs/python/.api/msgspec.md`) office receipt — the `core/receipt#RECEIPT` `ArtifactReceipt.Office` case — emitted under one `structlog` (`libs/python/.api/structlog.md`) event inside an OpenTelemetry (`libs/python/.api/opentelemetry-api.md`) span on the sub-3.15 worker; the build authors on the `anyio.to_process` (`libs/python/.api/anyio.md`) worker band and only the saved `.pptx` bytes (or a stream) plus the typed receipt cross the seam back, never a live `Presentation` object.
- boundary: python-pptx owns `.pptx`. Integration: `exchange/detect#DETECT` routes `MediaClass.PRESENTATION` -> python-pptx at the ingest gate (`puremagic`/`python-magic`, `libs/python/artifacts/.api/puremagic.md`); `msoffcrypto-tool` (`libs/python/artifacts/.api/msoffcrypto-tool.md`) decrypts an encrypted container to a plaintext stream `Presentation(stream)` reads and re-seals the saved bytes; chart imagery generated outside (`matplotlib`/`vl-convert-python`/`pyvista`, `libs/python/artifacts/.api/matplotlib.md`) and the `document/emit#DOCUMENT` `DocumentPlan.bound` `assets` band land as a `Picture` via `add_picture` from `io.BytesIO`, and `Pillow` (a dependency, co-resident on the worker) sizes embedded images and exposes `Picture.image` evidence. Word routes to `python-docx` (`libs/python/artifacts/.api/python-docx.md`), Excel to `openpyxl`/`xlsxwriter`, ODF presentation to `odfpy` (`DocumentMode.ODT`/`ODS`). Live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `python-pptx`
- Owns: `.pptx` construction and editing — slides from layouts, placeholder fill-in-place, textboxes/runs/paragraphs/fonts with the full character appearance (bold/italic/underline/size/name/language/color/fill/hyperlink), pictures with crop and image evidence, tables with merge/split/banding, native category/xy/bubble charts with axis/legend/data-label/series formatting and `replace_data` refresh, autoshapes/connectors/groups, movie/OLE embeds, freeform vector shapes, the FillFormat/LineFormat/ShadowFormat DML surface, slide background, notes, core-property metadata
- Accept: presentation authoring feeding the `document/emit#DOCUMENT` `DocumentMode.PPTX` lowering arm and the `document/emit#DOCUMENT` `DocumentPlan.bound` `DocumentMode.PPTX` template-clone fan row, downstream of the `exchange/detect#DETECT` `MediaClass.PRESENTATION` gate and the `msoffcrypto-tool` confidentiality edge, embedding the `document/emit#DOCUMENT` `DocumentPlan.bound` `assets` figure band via `add_picture` from a stream
- Reject: wrapper-renames of `add_slide`/`add_picture`/`save`; an `add_*`-per-shape parallel slide type where the one `SlideShapes` collection discriminates by method row; a per-run color/font rebuild where `Font` + `ColorFormat` carry the appearance; raw-EMU integers where `Length` value objects (and their `.emu`/`.pt`/`.inches` read-back) exist; hex-string color where `RGBColor`/`MSO_THEME_COLOR` exist; magic shape/anchor/alignment/dash strings where the `MSO_SHAPE`/`MSO_ANCHOR`/`PP_ALIGN`/`MSO_LINE_DASH_STYLE` enum rows exist; hand-built path or table XML where `build_freeform` and `add_table`/`Table.cell` exist; a separate chart-data spreadsheet where the embedded `XlsxWriter` workbook is in-package; identity minting the runtime owns
