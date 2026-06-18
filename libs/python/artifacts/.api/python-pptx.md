# [PY_ARTIFACTS_API_PYTHON_PPTX]

`python-pptx` supplies the PowerPoint `.pptx` presentation surface for the artifacts office rail: a `Presentation` factory returning a presentation object whose slide collection, shape-add family (textbox, picture, table, chart, autoshape), text frame model, and unit value objects drive OOXML presentation construction and editing. The package owner composes `Presentation`, `Slides.add_slide`, the `SlideShapes.add_*` family, and the `Inches`/`Pt`/`Emu` unit objects into the office owner; it never re-implements OOXML parsing python-pptx already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-pptx`
- package: `python-pptx`
- import: `pptx`
- owner: `artifacts`
- rail: office
- installed: `1.0.2` reflected via `python -c "import pptx"` on the gated `python_version<'3.15'` band (cp313)
- entry points: none (library only)
- capability: `.pptx` presentation construction and editing — slides, layouts, masters, textboxes, runs/paragraphs/fonts, pictures, tables, native charts, autoshapes, connectors, placeholders, notes, core properties

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: presentation, slide, and shape types
- rail: office

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]    | [CAPABILITY]                         |
| :-----: | :----------------------------- | :---------------- | :----------------------------------- |
|   [1]   | `presentation.Presentation`    | presentation root | slides, layouts, masters, dimensions |
|   [2]   | `slide.Slide`                  | slide unit        | shapes, placeholders, notes, layout  |
|   [3]   | `slide.SlideLayout`            | layout            | slide template layout                |
|   [4]   | `slide.SlideMaster`            | master            | master slide source                  |
|   [5]   | `shapes.shapetree.SlideShapes` | shape tree        | the `add_*` shape collection         |
|   [6]   | `shapes.autoshape.Shape`       | shape             | autoshape with fill/line/text frame  |
|   [7]   | `shapes.picture.Picture`       | picture           | inline image shape                   |
|   [8]   | `shapes.graphfrm.GraphicFrame` | graphic frame     | host for tables and charts           |

[PUBLIC_TYPE_SCOPE]: text, chart, and unit types
- rail: office

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE] | [CAPABILITY]                           |
| :-----: | :----------------------------- | :------------- | :------------------------------------- |
|   [1]   | `text.text.TextFrame`          | text frame     | paragraphs, anchor, autosize, margins  |
|   [2]   | `text.text._Paragraph`         | paragraph      | runs, alignment, level, spacing        |
|   [3]   | `text.text._Run`               | run            | styled text run with font              |
|   [4]   | `text.text.Font`               | font           | size/bold/italic/color font properties |
|   [5]   | `chart.data.CategoryChartData` | chart data     | category-series chart data             |
|   [6]   | `table.Table`                  | table          | rows/columns/cells grid                |
|   [7]   | `util.Inches`                  | unit           | inch measurement (EMU value object)    |
|   [8]   | `enum.shapes.MSO_SHAPE`        | shape enum     | autoshape geometry preset              |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: presentation open, slides, and save
- rail: office

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                       | [CAPABILITY]                      |
| :-----: | :----------------------------- | :--------------------------------- | :-------------------------------- |
|   [1]   | `Presentation`                 | `Presentation(pptx=None)`          | open or create (factory function) |
|   [2]   | `Presentation.save`            | path or stream target              | serialize the presentation        |
|   [3]   | `Presentation.slides`          | slide collection                   | enumerate slides                  |
|   [4]   | `Slides.add_slide`             | `add_slide(slide_layout) -> Slide` | append a slide from a layout      |
|   [5]   | `Presentation.slide_layouts`   | indexable layout collection        | access master layouts             |
|   [6]   | `Presentation.slide_width`     | EMU width property                 | get/set slide width               |
|   [7]   | `Presentation.core_properties` | core property object               | author/title/created metadata     |
|   [8]   | `Slide.notes_slide`            | notes slide accessor               | access speaker notes              |

[ENTRYPOINT_SCOPE]: shape and content authoring
- rail: office

Shape rows take `Inches`/`Pt`/`Emu` position and size values; the add family returns the created shape.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                                  | [CAPABILITY]                    |
| :-----: | :----------------------------- | :------------------------------------------------------------ | :------------------------------ |
|   [1]   | `SlideShapes.add_textbox`      | `add_textbox(left, top, width, height) -> Shape`              | add a text box                  |
|   [2]   | `SlideShapes.add_picture`      | `add_picture(image_file, left, top, width=None, height=None)` | add an inline picture           |
|   [3]   | `SlideShapes.add_table`        | `add_table(rows, cols, left, top, width, height)`             | add a table graphic frame       |
|   [4]   | `SlideShapes.add_chart`        | `add_chart(chart_type, x, y, cx, cy, chart_data)`             | add a native chart              |
|   [5]   | `SlideShapes.add_shape`        | `add_shape(MSO_SHAPE, left, top, width, height)`              | add an autoshape                |
|   [6]   | `TextFrame.add_paragraph`      | no-arg append                                                 | add a paragraph to a text frame |
|   [7]   | `_Paragraph.add_run`           | no-arg append                                                 | add a styled run to a paragraph |
|   [8]   | `CategoryChartData.add_series` | name plus values                                              | add a chart data series         |

## [4]-[IMPLEMENTATION_LAW]

[OFFICE_PPTX]:
- import: `from pptx import Presentation; from pptx.util import Inches, Pt, Emu` at boundary scope only; the distribution is `python-pptx`, the import name is `pptx`.
- presentation axis: `Presentation(...)` is the single factory for both open and create (the `pptx` argument is `None` to create from the default template); it returns the `presentation.Presentation` object, never a parallel reader/writer split.
- slide axis: `Presentation.slides.add_slide(layout)` is the single slide-add surface; the layout comes from `slide_layouts`, so a slide is a layout-row instance, never a per-template slide type.
- shape axis: `SlideShapes.add_textbox`/`add_picture`/`add_table`/`add_chart`/`add_shape` is one shape-add surface on `slide.shapes`; each shape kind is a method row, never a parallel slide type.
- text axis: a shape exposes a `TextFrame` whose `paragraphs`/`add_paragraph` and `_Run`/`add_run` carry text; `Font` is one font value applied per run, never per-run duplication.
- unit axis: positions and sizes use `Inches`/`Pt`/`Emu`/`Cm`/`Mm` value objects, never raw EMU ints; chart type is an `XL_CHART_TYPE` enum row, autoshape geometry an `MSO_SHAPE` row.
- evidence: each presentation op captures slide count, shape count, image count, chart count, and output byte length as an office receipt.
- boundary: python-pptx owns `.pptx`; Word routes to `python-docx`, Excel to `openpyxl`; chart imagery routes to the chart owner; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `python-pptx`
- Owns: `.pptx` construction and editing — slides, layouts, textboxes, runs/paragraphs/fonts, pictures, tables, native charts, autoshapes, placeholders, notes, core properties
- Accept: presentation authoring feeding the office and export-bundle owners
- Reject: wrapper-renames of `add_slide`/`save`; a per-shape-kind slide type where the `add_*` family suffices; raw-EMU measurements where unit value objects exist; identity minting the runtime owns
