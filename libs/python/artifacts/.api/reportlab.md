# [PY_ARTIFACTS_API_REPORTLAB]

`reportlab` supplies two PDF generation surfaces for the artifacts pdf rail: the low-level `Canvas` for direct coordinate drawing, and the high-level platypus document model (`SimpleDocTemplate`, `Paragraph`, `Table`, `Frame`) that flows `Flowable` content across pages; `pdfmetrics` and `TTFont` own font registration, and `reportlab.lib` owns page sizes, units, colors, and stylesheets.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `reportlab`
- package: `reportlab`
- module: `reportlab`
- asset: pure-Python runtime library (optional `pillow` for raster images)
- rail: pdf

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: canvas and metrics family
- rail: pdf — `reportlab.pdfgen.canvas`, `reportlab.pdfbase`

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [ROLE]                                     |
| :-----: | :------------------------ | :------------ | :----------------------------------------- |
|  [01]   | `pdfgen.canvas.Canvas`    | class         | imperative low-level page drawing surface  |
|  [02]   | `pdfbase.pdfmetrics`      | module        | font registration and string-width metrics |
|  [03]   | `pdfbase.ttfonts.TTFont`  | class         | TrueType font registration handle          |
|  [04]   | `pdfbase.pdfmetrics.Font` | class         | registered font descriptor                 |

[PUBLIC_TYPE_SCOPE]: document template family
- rail: pdf — `reportlab.platypus`
- kind: class

| [INDEX] | [SYMBOL]            | [ROLE]                                             |
| :-----: | :------------------ | :------------------------------------------------- |
|  [01]   | `SimpleDocTemplate` | one-frame document builder over flowables          |
|  [02]   | `BaseDocTemplate`   | multi-template/multi-frame document builder        |
|  [03]   | `PageTemplate`      | named page layout binding frames to draw callbacks |
|  [04]   | `Frame`             | rectangular flow region on a page                  |

[PUBLIC_TYPE_SCOPE]: flowable content family
- rail: pdf — `reportlab.platypus`

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [ROLE]                                            |
| :-----: | :------------- | :------------ | :------------------------------------------------ |
|  [01]   | `Flowable`     | abstract base | wrap/draw protocol for flowed content             |
|  [02]   | `Paragraph`    | flowable      | styled, wrapped rich-text block                   |
|  [03]   | `Table`        | flowable      | grid of cells with per-cell styling               |
|  [04]   | `LongTable`    | flowable      | streaming variant for very large tables           |
|  [05]   | `TableStyle`   | style         | table command list (lines, spans, padding, color) |
|  [06]   | `Image`        | flowable      | embedded raster image                             |
|  [07]   | `Spacer`       | flowable      | vertical gap                                      |
|  [08]   | `PageBreak`    | flowable      | force a new page                                  |
|  [09]   | `ListFlowable` | flowable      | bulleted or numbered list container               |
|  [10]   | `KeepTogether` | flowable      | keep child flowables on one page                  |
|  [11]   | `Preformatted` | flowable      | monospace, line-preserving text                   |

[PUBLIC_TYPE_SCOPE]: style and lib family
- rail: pdf — `reportlab.lib`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [ROLE]                                        |
| :-----: | :-------------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `lib.styles.ParagraphStyle` | class         | named paragraph style attributes              |
|  [02]   | `lib.styles.StyleSheet1`    | class         | named-style registry                          |
|  [03]   | `lib.colors`                | module        | named colors and color constructors           |
|  [04]   | `lib.pagesizes`             | module        | `A4`, `A3`, `LETTER`, `portrait`, `landscape` |
|  [05]   | `lib.units`                 | module        | `mm`, `cm`, `inch`, `pica`, `toLength`        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: canvas construction and lifecycle
- rail: pdf — `reportlab.pdfgen.canvas.Canvas`

| [INDEX] | [SURFACE]                                                                                               | [ENTRY_FAMILY] | [ROLE]                             |
| :-----: | :------------------------------------------------------------------------------------------------------ | :------------- | :--------------------------------- |
|  [01]   | `Canvas(filename, pagesize=None, bottomup=1, pageCompression=None, encrypt=None, pdfVersion=None, ...)` | construct      | open a canvas for one PDF          |
|  [02]   | `Canvas.showPage()`                                                                                     | lifecycle      | finish the current page            |
|  [03]   | `Canvas.save()`                                                                                         | lifecycle      | write the PDF to the target        |
|  [04]   | `Canvas.getpdfdata()`                                                                                   | lifecycle      | return PDF bytes instead of a file |
|  [05]   | `Canvas.setPageSize(size)`                                                                              | configure      | set page dimensions                |
|  [06]   | `Canvas.setAuthor` / `setTitle` / `setSubject`                                                          | metadata       | document info dictionary fields    |

[ENTRYPOINT_SCOPE]: canvas drawing
- rail: pdf — `reportlab.pdfgen.canvas.Canvas`

| [INDEX] | [SURFACE]                                                                                           | [ENTRY_FAMILY] | [ROLE]               |
| :-----: | :-------------------------------------------------------------------------------------------------- | :------------- | :------------------- |
|  [01]   | `Canvas.drawString(x, y, text, mode=None, charSpace=0, ...)`                                        | text           | left-aligned text    |
|  [02]   | `Canvas.drawCentredString(x, y, text, ...)`                                                         | text           | centered text        |
|  [03]   | `Canvas.drawRightString(x, y, text, ...)`                                                           | text           | right-aligned text   |
|  [04]   | `Canvas.setFont(psfontname, size, leading=None)`                                                    | text           | select active font   |
|  [05]   | `Canvas.drawImage(image, x, y, width=None, height=None, mask=None, preserveAspectRatio=False, ...)` | image          | place a raster image |
|  [06]   | `Canvas.line(x1, y1, x2, y2)`                                                                       | path           | straight line        |
|  [07]   | `Canvas.rect(x, y, width, height, stroke=1, fill=0)`                                                | path           | rectangle            |
|  [08]   | `Canvas.circle(x_cen, y_cen, r, stroke=1, fill=0)`                                                  | path           | circle               |
|  [09]   | `Canvas.setFillColor(aColor, alpha=None)`                                                           | color          | fill color           |
|  [10]   | `Canvas.setStrokeColor(aColor, alpha=None)`                                                         | color          | stroke color         |
|  [11]   | `Canvas.translate(dx, dy)` / `scale(x, y)` / `rotate(theta)`                                        | transform      | coordinate transform |
|  [12]   | `Canvas.saveState()` / `restoreState()`                                                             | state          | graphics-state stack |

[ENTRYPOINT_SCOPE]: document build and content
- rail: pdf — `reportlab.platypus`, `reportlab.lib.styles`

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY] | [ROLE]                                  |
| :-----: | :------------------------------------------------------------------------------------------ | :------------- | :-------------------------------------- |
|  [01]   | `SimpleDocTemplate(filename, **kw)`                                                         | construct      | one-frame template (page size, margins) |
|  [02]   | `SimpleDocTemplate.build(flowables, onFirstPage=..., onLaterPages=..., canvasmaker=Canvas)` | build          | render a flowable story                 |
|  [03]   | `BaseDocTemplate.addPageTemplates(pageTemplates)`                                           | configure      | register page templates                 |
|  [04]   | `BaseDocTemplate.multiBuild(story, ...)`                                                    | build          | multi-pass build for indexes/TOC        |
|  [05]   | `Paragraph(text, style=None, bulletText=None, ...)`                                         | content        | styled rich-text flowable               |
|  [06]   | `Table(data, colWidths=None, rowHeights=None, style=None, repeatRows=0, ...)`               | content        | grid flowable from row data             |
|  [07]   | `Image(filename, width=None, height=None, kind='direct', ...)`                              | content        | image flowable                          |
|  [08]   | `getSampleStyleSheet()`                                                                     | style          | default named-style sheet               |
|  [09]   | `ParagraphStyle(name, parent=None, **kw)`                                                   | style          | custom paragraph style                  |

[ENTRYPOINT_SCOPE]: font registration and metrics
- rail: pdf — `reportlab.pdfbase`

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [ROLE]                                     |
| :-----: | :------------------------------------------------------------------ | :------------- | :----------------------------------------- |
|  [01]   | `pdfmetrics.registerFont(font)`                                     | register       | register a `TTFont` for use                |
|  [02]   | `TTFont(name, filename, subfontIndex=0, ...)`                       | construct      | wrap a TrueType file as a registrable font |
|  [03]   | `pdfmetrics.stringWidth(text, fontName, fontSize, encoding='utf8')` | metric         | width of a string in points                |
|  [04]   | `units.toLength(value)` / `mm` / `cm` / `inch`                      | unit           | convert measurements to points             |

## [04]-[IMPLEMENTATION_LAW]

[REPORTLAB_TOPOLOGY]:
- two generation paths: imperative `Canvas` (absolute coordinates, `showPage`/`save`) and declarative platypus (`build` a list of `Flowable` over a template)
- platypus story: a list of flowables (`Paragraph`, `Table`, `Image`, `Spacer`, `PageBreak`) flowed by `SimpleDocTemplate.build`
- coordinate origin: bottom-left when `bottomup=1` (default); all `Canvas` coordinates are in points
- `TableStyle` is a command list of `('BACKGROUND', (col, row), (col, row), color)`-style tuples applied to cell ranges
- fonts: built-in Type 1 fonts are available without registration; TrueType fonts require `TTFont` plus `pdfmetrics.registerFont`
- units: all geometry is in points; multiply by `mm`, `cm`, or `inch` from `reportlab.lib.units`

[LOCAL_ADMISSION]:
- Use platypus (`SimpleDocTemplate` + flowables) for content that paginates; reserve `Canvas` for fixed-coordinate drawing and overlays.
- Build styles from `getSampleStyleSheet` and derive with `ParagraphStyle(parent=...)`; do not construct ad hoc style dicts.
- Express all geometry in points scaled by `reportlab.lib.units`; never bake raw point literals for physical measurements.
- Register custom fonts once via `TTFont` + `registerFont` before first use in any flowable or canvas text.
- Use `Canvas.getpdfdata()` to obtain bytes for an in-memory pipeline instead of writing a temp file.

[RAIL_LAW]:
- Package: `reportlab`
- Owns: programmatic PDF generation via the imperative canvas and the platypus flowable document model
- Accept: flowable stories for paginated content; canvas drawing for fixed layout; `TTFont` for custom typography
- Reject: hand-rolled PDF byte emission; parallel layout engines where platypus applies; wrapper-renames of `build`/`Canvas`
