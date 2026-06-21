# [PY_ARTIFACTS_API_PYTHON_PPTX]

`python-pptx` supplies the PowerPoint `.pptx` presentation surface for the artifacts office rail: a polymorphic `Presentation` factory that opens an existing file/stream or creates from the default template, returning a `presentation.Presentation` whose `Slides.add_slide(layout)` collection, the full `SlideShapes.add_*` shape family (textbox, picture, table, chart, autoshape, connector, group, movie, OLE object, freeform), the `TextFrame`/`_Paragraph`/`_Run`/`Font` text model, native `ChartData` series builders, and `Inches`/`Pt`/`Cm`/`Mm`/`Emu` value objects drive OOXML presentation construction and editing. The package owner composes `Presentation`, `add_slide`, the `add_*` family plus `build_freeform`, the text-frame model with `fit_text`/`auto_size`, the `ColorFormat` font surface, and the unit objects into the office owner; it removes any raw-EMU integer math because the `Length` value objects own conversion, and it never re-implements the OOXML presentation part graph, the lxml-backed element serialization, or the slide-layout/master inheritance python-pptx already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-pptx`
- package: `python-pptx`
- import: `pptx`
- owner: `artifacts`
- rail: office
- installed: `1.0.2` (uv.lock pin, ungated — runs on cp315-core; reflected on cp313 with matching version)
- license: MIT (Steve Canny) — permissive, no copyleft gate; aligns with the MIT/BSD sibling office owners
- abi: pure Python; runtime dependencies `lxml` (BSD, the only compiled/ABI surface), `Pillow` (image-embed sizing), `XlsxWriter` (chart-data workbook embed). Installs clean on cp315
- entry points: none (library only)
- capability: `.pptx` construction and editing — slides from layouts, layouts/masters, textboxes, runs/paragraphs/fonts with color and hyperlinks, pictures, tables, native category/xy/bubble charts, autoshapes, connectors, group shapes, movie and OLE-object embeds, freeform vector shapes, placeholders, speaker notes, core properties, and offscreen slide-shape geometry in EMU value objects

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: presentation, slide, and shape types
- rail: office

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]    | [CAPABILITY]                                       |
| :-----: | :----------------------------- | :---------------- | :------------------------------------------------- |
|  [01]   | `presentation.Presentation`    | presentation root | slides, layouts, masters, slide dimensions          |
|  [02]   | `slide.Slides`                 | slide collection  | `add_slide`/`get`/`index` over slides               |
|  [03]   | `slide.Slide`                  | slide unit        | shapes, placeholders, notes_slide, slide_layout      |
|  [04]   | `slide.SlideLayout`            | layout            | slide template layout (from `slide_layouts`)        |
|  [05]   | `slide.SlideMaster`            | master            | master slide source                                |
|  [06]   | `shapes.shapetree.SlideShapes` | shape tree        | the `add_*` shape collection plus `build_freeform`  |
|  [07]   | `shapes.autoshape.Shape`       | shape             | autoshape/textbox with fill/line/text frame         |
|  [08]   | `shapes.graphfrm.GraphicFrame` | graphic frame     | host returned for tables and charts                 |

[PUBLIC_TYPE_SCOPE]: text, chart, color, and unit types
- rail: office

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE] | [CAPABILITY]                                          |
| :-----: | :----------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `text.text.TextFrame`          | text frame     | paragraphs, anchor, autosize/fit_text, word_wrap, margins |
|  [02]   | `text.text._Paragraph`         | paragraph      | runs, alignment, level, line/space spacing, line break |
|  [03]   | `text.text._Run`               | run            | styled text run with font and hyperlink               |
|  [04]   | `text.text.Font`               | font           | size/bold/italic/underline/name/color/language        |
|  [05]   | `dml.color.ColorFormat`        | color          | `rgb` (RGBColor) or `theme_color` (MSO_THEME_COLOR)    |
|  [06]   | `chart.data.CategoryChartData` | chart data     | category-series chart data (`add_category`/`add_series`)|
|  [07]   | `chart.data.XyChartData`       | chart data     | xy / scatter series (`add_series` of point pairs)     |
|  [08]   | `util.Inches`                  | unit           | inch measurement (also `Pt`/`Cm`/`Mm`/`Emu`/`Centipoints`/`Length`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: presentation factory, slides, save
- rail: office

`Presentation(pptx=None)` is the single open-or-create factory; `Slides.add_slide(layout)` is the single slide-add surface, the layout sourced from `slide_layouts`.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                        | [CAPABILITY]                          |
| :-----: | :----------------------------- | :-------------------------------------------------- | :------------------------------------ |
|  [01]   | `Presentation`                 | `Presentation(pptx: str | IO[bytes] | None = None) -> presentation.Presentation` | open a file/stream or create from default |
|  [02]   | `Presentation.save`            | `save(file: str | IO[bytes])`                       | serialize to a path or stream         |
|  [03]   | `Presentation.slides`          | `slides -> Slides`                                  | the slide collection                  |
|  [04]   | `Slides.add_slide`             | `add_slide(slide_layout: SlideLayout) -> Slide`     | append a slide from a layout          |
|  [05]   | `Presentation.slide_layouts`   | `slide_layouts -> SlideLayouts`                     | indexable layouts of the default master|
|  [06]   | `Presentation.slide_width`     | `slide_width` / `slide_height -> Length`            | get/set slide dimensions              |
|  [07]   | `Presentation.core_properties` | `core_properties -> CoreProperties`                 | author/title/created/modified/keywords|
|  [08]   | `Slide.notes_slide`            | `notes_slide.notes_text_frame -> TextFrame`         | access/author speaker notes           |

[ENTRYPOINT_SCOPE]: shape and freeform authoring
- rail: office

Shape rows take `Inches`/`Pt`/`Emu` `Length` position/size values; the add family returns the created shape or graphic frame. `build_freeform` mints a vector-path builder.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                                                              | [CAPABILITY]                         |
| :-----: | :----------------------------- | :---------------------------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `SlideShapes.add_textbox`      | `add_textbox(left, top, width, height) -> Shape`                                          | add a text box                       |
|  [02]   | `SlideShapes.add_picture`      | `add_picture(image_file, left, top, width=None, height=None) -> Picture`                  | add an inline picture                |
|  [03]   | `SlideShapes.add_table`        | `add_table(rows, cols, left, top, width, height) -> GraphicFrame`                         | add a table graphic frame            |
|  [04]   | `SlideShapes.add_chart`        | `add_chart(chart_type: XL_CHART_TYPE, x, y, cx, cy, chart_data: ChartData) -> Chart`      | add a native chart                   |
|  [05]   | `SlideShapes.add_shape`        | `add_shape(autoshape_type_id: MSO_SHAPE, left, top, width, height) -> Shape`              | add an autoshape                     |
|  [06]   | `SlideShapes.add_connector`    | `add_connector(connector_type: MSO_CONNECTOR_TYPE, begin_x, begin_y, end_x, end_y) -> Connector` | add a connector             |
|  [07]   | `SlideShapes.add_group_shape`  | `add_group_shape(shapes: Iterable[BaseShape] = ()) -> GroupShape`                         | group shapes                         |
|  [08]   | `SlideShapes.add_movie`        | `add_movie(movie_file, left, top, width, height, poster_frame_image=None, mime_type='video/unknown') -> GraphicFrame` | embed a movie     |
|  [09]   | `SlideShapes.add_ole_object`   | `add_ole_object(object_file, prog_id, left, top, width=None, height=None, icon_file=None, ...) -> GraphicFrame` | embed an OLE object         |
|  [10]   | `SlideShapes.build_freeform`   | `build_freeform(start_x=0, start_y=0, scale=1.0) -> FreeformBuilder`                      | begin a freeform vector path         |
|  [11]   | `FreeformBuilder.add_line_segments`| `add_line_segments(vertices: Iterable[tuple[float, float]], close=True)`              | extend the freeform path             |
|  [12]   | `FreeformBuilder.convert_to_shape`| `convert_to_shape(origin_x: Length = 0, origin_y: Length = 0) -> FreeformShape`        | place the freeform on the slide      |

[ENTRYPOINT_SCOPE]: text and chart-data authoring
- rail: office

A shape's `TextFrame` owns paragraphs/runs and fit; `ChartData` builders feed `add_chart`. Categories precede series for the category chart.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                                          | [CAPABILITY]                            |
| :-----: | :----------------------------- | :-------------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `TextFrame.add_paragraph`      | `add_paragraph() -> _Paragraph`                                       | append a paragraph                      |
|  [02]   | `TextFrame.fit_text`           | `fit_text(font_family='Calibri', max_size=18, bold=False, italic=False, font_file=None)` | shrink text to fit the frame |
|  [03]   | `TextFrame.auto_size`          | `auto_size` / `word_wrap` / `vertical_anchor` / `margin_*`            | autosize, wrap, anchor, margins         |
|  [04]   | `_Paragraph.add_run`           | `add_run() -> _Run` (also `add_line_break()`)                         | append a run / soft line break          |
|  [05]   | `_Run.font` / `_Run.hyperlink` | `font -> Font` / `hyperlink -> _Hyperlink`                            | run typography and link target          |
|  [06]   | `Font.color`                   | `color -> ColorFormat` (`rgb=RGBColor(...)` or `theme_color=...`)     | run color (RGB or theme)                |
|  [07]   | `CategoryChartData.add_category`| `add_category(label)`                                                | add a category (before series)          |
|  [08]   | `CategoryChartData.add_series` | `add_series(name, values=(), number_format=None)`                    | add a chart data series                 |

## [04]-[IMPLEMENTATION_LAW]

[OFFICE_PPTX]:
- import: `from pptx import Presentation; from pptx.util import Inches, Pt, Emu` at boundary scope only; the distribution is `python-pptx`, the import name is `pptx`.
- presentation axis: `Presentation(pptx=None)` is the single polymorphic factory for both open and create (path/stream opens, `None` creates from the default template); it returns the `presentation.Presentation` object, never a parallel reader/writer split.
- slide axis: `Presentation.slides.add_slide(layout)` is the single slide-add surface; the layout comes from `slide_layouts`, so a slide is a layout-row instance, never a per-template slide type. `Slides.get(slide_id)`/`index(slide)` address by id/position.
- shape axis: `add_textbox`/`add_picture`/`add_table`/`add_chart`/`add_shape`/`add_connector`/`add_group_shape`/`add_movie`/`add_ole_object` is one shape-add surface on `slide.shapes`; each shape kind is a method row, never a parallel slide type. Vector geometry is `build_freeform` -> `FreeformBuilder.add_line_segments` -> `convert_to_shape`, never a hand-built path XML.
- text axis: a shape exposes a `TextFrame` whose `paragraphs`/`add_paragraph` and `_Run`/`add_run` carry text; `fit_text`/`auto_size`/`word_wrap` own sizing. `Font` is one font value per run with `color` as a `ColorFormat` (`rgb` or `theme_color`) and `_Run.hyperlink` the link target — never per-run color duplication.
- chart axis: build a `CategoryChartData` (categories first via `add_category`, then `add_series`) or `XyChartData`/`BubbleChartData`, then `add_chart(XL_CHART_TYPE, x, y, cx, cy, chart_data)`; chart type is an `XL_CHART_TYPE` enum row. The chart-data workbook is embedded via the `XlsxWriter` dependency, never a separate spreadsheet file.
- unit axis: positions and sizes use `Inches`/`Pt`/`Emu`/`Cm`/`Mm`/`Centipoints` value objects, never raw EMU ints; autoshape geometry is an `MSO_SHAPE` row, connector kind an `MSO_CONNECTOR_TYPE` row, text anchor an `MSO_ANCHOR` row, alignment a `PP_ALIGN` row.
- evidence: each presentation op captures slide count, shape count, image count, chart count, and output byte length as an office receipt.
- boundary: python-pptx owns `.pptx`. Integration: `python-magic` gates the reader at admission; `msoffcrypto-tool` decrypts an encrypted container to a stream `Presentation(stream)` reads and re-seals the saved bytes; chart imagery generated outside (`matplotlib`/`vl-convert-python`/`pyvista`) lands as a `Picture` via `add_picture`, and `Pillow` (a dependency) sizes embedded images. Word routes to `python-docx`, Excel to `openpyxl`, ODF presentation to `odfpy`. Live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `python-pptx`
- Owns: `.pptx` construction and editing — slides from layouts, textboxes/runs/paragraphs/fonts with color and hyperlinks, pictures, tables, native category/xy/bubble charts, autoshapes, connectors, groups, movie/OLE embeds, freeform vector shapes, placeholders, notes, core properties
- Accept: presentation authoring feeding the office and export-bundle owners, downstream of the `python-magic` admission gate and the `msoffcrypto-tool` confidentiality edge
- Reject: wrapper-renames of `add_slide`/`save`; a per-shape-kind slide type where the `add_*` family suffices; hand-built path XML where `build_freeform` exists; raw-EMU integers where the `Length` value objects exist; magic shape/anchor/alignment strings where the `MSO_*`/`PP_*`/`XL_*` enums exist; identity minting the runtime owns
