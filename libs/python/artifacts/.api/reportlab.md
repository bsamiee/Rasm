# [PY_ARTIFACTS_API_REPORTLAB]

`reportlab` mints programmatic PDF synthesis across two layers: the imperative `pdfgen.canvas.Canvas` for absolute-coordinate drawing, text/path objects, AcroForm widgets, and navigation, and the declarative `platypus` model flowing a `Flowable` story across `Frame`/`PageTemplate` with multi-pass TOC and index resolution. `pdfbase.pdfmetrics` owns font registration and metrics, `graphics` the vector chart/barcode engine, `lib` the page sizes, units, colors, and stylesheets. Byte emission, a parallel layout engine, and post-synthesis editing stay off this synthesis-tier surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `reportlab`
- package: `reportlab` (`BSD-3-Clause`, ReportLab Europe)
- module: `reportlab`
- namespaces: `pdfgen`, `pdfbase`, `platypus`, `graphics`, `lib`
- rail: pdf — programmatic canvas drawing, flowable pagination, font registration, and the graphics/charts/barcode engine
- optional: `pillow` — `Image`/`drawImage` raster embedding and `renderPM` raster output

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: canvas and metrics family

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :-------------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `pdfgen.canvas.Canvas`            | class         | imperative low-level page drawing surface  |
|  [02]   | `pdfbase.pdfmetrics`              | module        | font registration and string-width metrics |
|  [03]   | `pdfbase.ttfonts.TTFont`          | class         | TrueType font registration handle          |
|  [04]   | `pdfbase.pdfmetrics.Font`         | class         | registered font descriptor                 |
|  [05]   | `pdfbase.cidfonts.UnicodeCIDFont` | class         | CJK/CID font face handle                   |

[PUBLIC_TYPE_SCOPE]: document template family

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :------------------ | :------------ | :------------------------------------------------- |
|  [01]   | `SimpleDocTemplate` | class         | one-frame document builder over flowables          |
|  [02]   | `BaseDocTemplate`   | class         | multi-template/multi-frame document builder        |
|  [03]   | `PageTemplate`      | class         | named page layout binding frames to draw callbacks |
|  [04]   | `Frame`             | class         | rectangular flow region on a page                  |

[PUBLIC_TYPE_SCOPE]: flowable content family

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :--------------------------------------------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `Flowable`                                                 | abstract base | wrap/draw protocol for flowed content                     |
|  [02]   | `Paragraph` / `XPreformatted`                              | flowable      | styled wrapped rich-text; whitespace-preserving variant   |
|  [03]   | `Table` / `LongTable`                                      | flowable      | grid of cells; `LongTable` streams a large table          |
|  [04]   | `TableStyle`                                               | style         | table command list (lines, spans, padding, color, valign) |
|  [05]   | `Image`                                                    | flowable      | embedded raster image (needs `pillow`)                    |
|  [06]   | `Spacer` / `PageBreak` / `CondPageBreak` / `FrameBreak`    | flowable      | vertical gap; page/conditional/frame break                |
|  [07]   | `ListFlowable` / `ListItem`                                | flowable      | bulleted or numbered list container and item              |
|  [08]   | `KeepTogether` / `KeepInFrame`                             | flowable      | keep children on one page; shrink-to-fit in a frame       |
|  [09]   | `Preformatted`                                             | flowable      | monospace, line-preserving text                           |
|  [10]   | `HRFlowable` / `Indenter` / `TopPadder`                    | flowable      | horizontal rule; left/right indent; bottom-align padder   |
|  [11]   | `BalancedColumns`                                          | flowable      | balance children across N columns                         |
|  [12]   | `NextPageTemplate`                                         | control       | switch the page template for the next page                |
|  [13]   | `tableofcontents.TableOfContents`                          | flowable      | multi-pass TOC (requires `multiBuild`)                    |
|  [14]   | `tableofcontents.SimpleIndex`                              | flowable      | multi-pass alphabetical index (requires `multiBuild`)     |
|  [15]   | `flowables.{DocAssign, DocExec, DocPara, DocIf, DocWhile}` | control       | document-time programming (compute values during build)   |
|  [16]   | `flowables.{AnchorFlowable, Macro, CallerMacro}`           | control       | named anchor target; inline canvas macro execution        |

[PUBLIC_TYPE_SCOPE]: canvas interactive and content-object family

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `pdfgen.textobject.PDFTextObject` | text builder  | batched text run via `beginText` (leading/spacing/rise/color) |
|  [02]   | `pdfgen.pathobject.PDFPathObject` | path builder  | composable path via `beginPath` (moveTo/lineTo/curveTo/...)   |
|  [03]   | `pdfbase.acroform.AcroForm`       | form root     | `canvas.acroForm`; interactive-widget factory root            |

[PUBLIC_TYPE_SCOPE]: vector graphics, charts, and barcode family

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :------------------------------------------------ | :------------ | :------------------------------------------------------------------ |
|  [01]   | `graphics.shapes.Drawing`                         | flowable      | vector canvas (shapes/groups) embeddable as a flowable              |
|  [02]   | `graphics.shapes.*`                               | shape         | primitives `Line`/`Rect`/`Circle`/`Polygon`/`String`/`Group`/`Path` |
|  [03]   | `graphics.charts.*`                               | chart         | data-driven `barcharts`/`linecharts`/`piecharts`/`lineplots`        |
|  [04]   | `graphics.barcode.createBarcodeDrawing(codeName)` | factory       | barcode/QR `Drawing` by symbology name                              |
|  [05]   | `graphics.renderPDF` / `renderPM` / `renderSVG`   | renderer      | render a `Drawing` to PDF / PNG (PM) / SVG bytes or file            |

[PUBLIC_TYPE_SCOPE]: style and lib family

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :------------------------------------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `lib.styles.ParagraphStyle` / `ListStyle`          | class         | named paragraph / list style attributes                    |
|  [02]   | `lib.styles.StyleSheet1` / `getSampleStyleSheet()` | registry      | named-style registry and the default sheet                 |
|  [03]   | `lib.colors`                                       | module        | `Color`/`CMYKColor`/`HexColor`/`toColor` + named constants |
|  [04]   | `lib.pagesizes`                                    | module        | `A4`/`A3`/`A5`/`LETTER`/`LEGAL`, `portrait`/`landscape`    |
|  [05]   | `lib.units`                                        | module        | `mm`/`cm`/`inch`/`pica`/`toLength`                         |
|  [06]   | `lib.enums`                                        | module        | `TA_LEFT`/`TA_CENTER`/`TA_RIGHT`/`TA_JUSTIFY` alignment    |
|  [07]   | `lib.utils.ImageReader`                            | class         | decode-once raster source reused across `drawImage` calls  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: canvas construction and lifecycle

`Canvas(filename, pagesize=, bottomup=, pageCompression=, encrypt=, cropMarks=, pdfVersion=, enforceColorSpace=, initialFontName=, cropBox=, artBox=, trimBox=, bleedBox=, lang=)` opens the page surface; `filename` is a path or a writable byte stream.

| [INDEX] | [SURFACE]                                               | [SHAPE]   | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------ | :-------- | :------------------------------------------ |
|  [01]   | `Canvas.showPage()`                                     | lifecycle | finish the current page                     |
|  [02]   | `Canvas.save()`                                         | lifecycle | write the PDF to the target                 |
|  [03]   | `Canvas.getpdfdata()`                                   | lifecycle | return PDF bytes instead of a file          |
|  [04]   | `Canvas.setPageSize(size)` / `setPageRotation(deg)`     | configure | set page dimensions / rotation              |
|  [05]   | `Canvas.setAuthor` / `setTitle` / `setSubject`          | metadata  | document info: author, title, subject       |
|  [06]   | `Canvas.setCreator` / `setKeywords` / `setProducer`     | metadata  | document info: creator, keywords, producer  |
|  [07]   | `Canvas.setEncrypt(encrypt)` / `setPageTransition(...)` | configure | encryption handler; presentation transition |

[ENTRYPOINT_SCOPE]: canvas drawing

| [INDEX] | [SURFACE]                                                                | [SHAPE]      | [CAPABILITY]                        |
| :-----: | :----------------------------------------------------------------------- | :----------- | :---------------------------------- |
|  [01]   | `Canvas.drawString(x, y, text, mode=None, charSpace=0, ...)`             | text         | left-aligned text                   |
|  [02]   | `Canvas.drawCentredString(x, y, text, ...)`                              | text         | centered text                       |
|  [03]   | `Canvas.drawRightString(x, y, text, ...)`                                | text         | right-aligned text                  |
|  [04]   | `Canvas.setFont(psfontname, size, leading=None)`                         | text         | select active font                  |
|  [05]   | `Canvas.drawImage(image, x, y, width=None, height=None, mask=None, ...)` | image        | place a raster image                |
|  [06]   | `Canvas.line(x1, y1, x2, y2)`                                            | path         | straight line                       |
|  [07]   | `Canvas.rect(x, y, width, height, stroke=1, fill=0)`                     | path         | rectangle                           |
|  [08]   | `Canvas.circle(x_cen, y_cen, r, stroke=1, fill=0)`                       | path         | circle                              |
|  [09]   | `Canvas.setFillColor(aColor, alpha=None)`                                | color        | fill color                          |
|  [10]   | `Canvas.setStrokeColor(aColor, alpha=None)`                              | color        | stroke color                        |
|  [11]   | `Canvas.translate(dx, dy)` / `scale(x, y)` / `rotate(theta)`             | transform    | translate / scale / rotate          |
|  [12]   | `Canvas.transform(a, b, c, d, e, f)`                                     | transform    | full affine matrix concatenation    |
|  [13]   | `Canvas.saveState()` / `restoreState()`                                  | state        | graphics-state stack                |
|  [14]   | `Canvas.setFillColorRGB` / `setFillColorCMYK` / `setStrokeColorRGB`      | color        | direct RGB/CMYK color-space setters |
|  [15]   | `Canvas.setFillAlpha` / `setStrokeAlpha`                                 | color        | fill / stroke transparency setters  |
|  [16]   | `Canvas.setDash(array, phase)` / `setLineWidth(w)`                       | stroke style | line dash pattern / width           |
|  [17]   | `Canvas.setLineCap(c)` / `setLineJoin(j)`                                | stroke style | line cap / join                     |
|  [18]   | `Canvas.arc`/`ellipse`/`roundRect`/`wedge`/`bezier`/`lines`/`grid`       | path         | extended primitive shapes           |

[ENTRYPOINT_SCOPE]: canvas text, path, interactive forms, and navigation

`beginText` builds the batched `PDFTextObject` and `beginPath` the composite `PDFPathObject`; `acroForm` roots interactive widgets, and bookmarks/outline/links own in-document and external navigation.

| [INDEX] | [SURFACE]                                                          | [SHAPE]    | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------------- | :--------- | :-------------------------------------------- |
|  [01]   | `Canvas.beginText(x, y) -> PDFTextObject` / `drawText(t)`          | text       | batched multi-line text run                   |
|  [02]   | `Canvas.beginPath() -> PDFPathObject` / `drawPath` / `clipPath`    | path       | composite path build and clip                 |
|  [03]   | `Canvas.acroForm.textfield(name, value, x, y, width, height, ...)` | form       | interactive text field widget                 |
|  [04]   | `Canvas.acroForm.checkbox` / `radio` / `choice` / `listbox`        | form       | checkbox/radio/dropdown/listbox widgets       |
|  [05]   | `Canvas.bookmarkPage(key)` / `bookmarkHorizontal(key, rx, ry)`     | navigation | define an in-document jump target             |
|  [06]   | `Canvas.addOutlineEntry(title, key, level=0, closed=None)`         | navigation | PDF bookmark/outline tree                     |
|  [07]   | `Canvas.linkAbsolute(...)` / `linkRect(...)` / `linkURL(...)`      | navigation | internal link to bookmark / external URL link |
|  [08]   | `Canvas.beginForm(name, ...)` / `endForm()` / `doForm(name)`       | reuse      | reusable XObject form                         |
|  [09]   | `Canvas.drawInlineImage(image, ...)`                               | reuse      | inline raster image                           |

[ENTRYPOINT_SCOPE]: vector graphics, charts, and barcodes

| [INDEX] | [SURFACE]                                                        | [SHAPE]   | [CAPABILITY]                                         |
| :-----: | :--------------------------------------------------------------- | :-------- | :--------------------------------------------------- |
|  [01]   | `graphics.shapes.Drawing(width, height)` + `.add(shape)`         | construct | a vector canvas embeddable as a flowable             |
|  [02]   | `graphics.barcode.createBarcodeDrawing(codeName, **options)`     | barcode   | barcode/QR `Drawing` by symbology name               |
|  [03]   | `graphics.renderPDF.draw(d, canvas, x, y)` / `drawToFile(...)`   | render    | place a `Drawing` on a canvas / write standalone PDF |
|  [04]   | `graphics.renderPM.drawToFile(d, fn, ...)` / `drawToString(...)` | render    | rasterize a `Drawing` to PNG/GIF (needs `_renderPM`) |
|  [05]   | `graphics.renderSVG.drawToString(d)` / `drawToFile(d, fn)`       | render    | serialize a `Drawing` to SVG                         |

[ENTRYPOINT_SCOPE]: document build and content

| [INDEX] | [SURFACE]                                                         | [SHAPE]   | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------------------- | :-------- | :--------------------------------------------- |
|  [01]   | `SimpleDocTemplate(filename, **kw)`                               | construct | one-frame template (page size, margins)        |
|  [02]   | `SimpleDocTemplate.build(flowables, onFirstPage=, onLaterPages=)` | build     | render a flowable story (`canvasmaker=Canvas`) |
|  [03]   | `BaseDocTemplate.addPageTemplates(pageTemplates)`                 | configure | register page templates                        |
|  [04]   | `BaseDocTemplate.multiBuild(story, ...)`                          | build     | multi-pass build for indexes/TOC               |
|  [05]   | `Paragraph(text, style=None, bulletText=None, ...)`               | content   | styled rich-text flowable                      |
|  [06]   | `Table(data, colWidths=None, rowHeights=None, style=None, ...)`   | content   | grid flowable; `repeatRows=` repeats headers   |
|  [07]   | `Image(filename, width=None, height=None, kind='direct', ...)`    | content   | image flowable                                 |
|  [08]   | `getSampleStyleSheet()`                                           | style     | default named-style sheet                      |
|  [09]   | `ParagraphStyle(name, parent=None, **kw)`                         | style     | custom paragraph style                         |
|  [10]   | `TableOfContents()` + `notify('TOCEntry', (level, text, page))`   | toc       | multi-pass TOC; resolved by `multiBuild`       |
|  [11]   | `Frame(x1, y1, width, height, leftPadding=..., id=...)`           | layout    | a flow region; N frames per `PageTemplate`     |
|  [12]   | `PageTemplate(id, frames, onPage=..., onPageEnd=...)`             | layout    | named page layout binding frames to callbacks  |
|  [13]   | `DocAssign(var, expr)` / `DocIf(cond, thenBlock, elseBlock)`      | control   | compute/branch values during the build pass    |

[ENTRYPOINT_SCOPE]: font registration and metrics

| [INDEX] | [SURFACE]                                                           | [SHAPE]   | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------ | :-------- | :----------------------------------------------- |
|  [01]   | `pdfmetrics.registerFont(font)`                                     | register  | register a `TTFont`/`UnicodeCIDFont` for use     |
|  [02]   | `TTFont(name, filename, subfontIndex=0, ...)`                       | construct | wrap a TrueType file as a registrable font       |
|  [03]   | `cidfonts.UnicodeCIDFont(face)`                                     | construct | register a CJK/CID font face                     |
|  [04]   | `pdfmetrics.registerFontFamily(family, normal, bold, italic, ...)`  | register  | bind bold/italic variants for `<b>`/`<i>` markup |
|  [05]   | `pdfmetrics.stringWidth(text, fontName, fontSize, encoding='utf8')` | metric    | width of a string in points                      |
|  [06]   | `pdfmetrics.getFont(name)` / `getRegisteredFontNames()`             | metric    | resolve a registered font; list names            |
|  [07]   | `units.toLength(value)` / `mm` / `cm` / `inch` / `pica`             | unit      | convert measurements to points                   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Two generation layers meet at the `canvasmaker`/`onPage` callbacks: `platypus` flows content while a `Canvas` callback paints fixed headers, footers, and watermarks on the same page.
- Cross-pass content — `TableOfContents`, `SimpleIndex`, back-references — resolves only under `BaseDocTemplate.multiBuild`, which re-runs layout until page numbers stabilize.
- Coordinate origin is bottom-left under `bottomup=1` and every `Canvas` coordinate is in points; a `Frame` flows top-down internally regardless.
- `TableStyle` is a command list of `('BACKGROUND', (col, row), (col, row), color)`-shaped tuples (`SPAN`/`LINEBELOW`/`VALIGN`/`*PADDING`) applied to cell ranges; `LongTable` streams large data with `repeatRows`.
- A `graphics.shapes.Drawing` is a `Flowable`, so charts and barcodes embed directly in a story or render standalone via `renderPDF`/`renderPM`/`renderSVG`.

[STACKING]:
- `pikepdf`(`.api/pikepdf.md`) / `pypdf`(`.api/pypdf.md`): `reportlab -> BytesIO -> Pdf.open(stream)` hands the synthesized bytes for merge/split/page-ops and re-encryption; reportlab synthesizes, pikepdf/pypdf restructure.
- `pyhanko`(`.api/pyhanko.md`): the `getpdfdata()` byte stream PAdES-signs downstream; synthesis precedes sign, never a hand-stamped signature dictionary.
- `pymupdf`(`.api/pymupdf.md`) / `pdfplumber`(`.api/pdfplumber.md`): post-synthesis render/extract and ruled-table extraction route here, never re-implemented on the canvas.
- `ocrmypdf`(`.api/ocrmypdf.md`): a scanned-source text layer routes here, never a hand-drawn invisible-text overlay.
- `pillow`(`.api/pillow.md`): `Image`/`drawImage` accept a `PIL.Image` or `lib.utils.ImageReader` for decode-once reuse across draws.
- `segno`(`.api/segno.md`) / `python-barcode`(`.api/python-barcode.md`) / `resvg-py`(`.api/resvg-py.md`): a sibling-drawn barcode SVG rasterizes through `resvg-py` when the built-in `graphics.barcode` symbology set does not carry the code.
- `great-tables`(`.api/great-tables.md`): standalone styled tables come from `great-tables`; reportlab's `Table` owns tables that must paginate inside a larger PDF.
- `weasyprint`(`.api/weasyprint.md`) / `typst`(`.api/typst.md`): HTML/CSS-to-PDF and markup-typeset PDF route by source shape; reportlab owns data-driven programmatic reports from Python objects.
- `expression`(`libs/python/.api/expression.md`): `getpdfdata()` lands as the success arm of the artifacts `Result` rail, synthesis faults mapped at the boundary.
- within-lib: `artifacts` composes `platypus` for pagination and a `Canvas` `onPage` callback for fixed decoration on the same page, `multiBuild` + `notify('TOCEntry', ...)` for TOC/index, and a `BytesIO` target for the in-memory rail.

[LOCAL_ADMISSION]:
- Paginating content builds through `platypus` (`SimpleDocTemplate`/`BaseDocTemplate` + flowables); `Canvas` serves fixed-coordinate overlays, interactive forms, and per-page decoration via `onPage`.
- Styles derive from `getSampleStyleSheet` through `ParagraphStyle(parent=...)`, never ad hoc style dicts.
- Geometry expresses in points scaled by `lib.units` with alignment via `lib.enums`; a raw point literal for a physical measurement is rejected.
- Custom fonts register once via `TTFont`/`UnicodeCIDFont` + `registerFont` (and `registerFontFamily` for markup variants) before first use; built-in Type 1 fonts need no registration.
- Multi-line text batches through `PDFTextObject` and composite geometry through `PDFPathObject`; `drawString`/`line`/`rect` serve one-off marks alone.
- In-memory output binds `getpdfdata()` or a `BytesIO` target instead of a temp file.
- TOC, index, and cross-references build under `multiBuild` with `notify('TOCEntry', ...)`, never hand-computed page numbers.

[RAIL_LAW]:
- Package: `reportlab`
- Owns: programmatic PDF synthesis via the imperative canvas (text/path objects, AcroForm widgets, bookmarks/outline/links, encryption, transitions) and the platypus flowable model (frames/templates, multi-pass TOC/index, doc-programming flowables), font registration/metrics, and the vector graphics/charts/barcode engine.
- Accept: flowable stories for paginated content; canvas drawing for fixed layout/forms/overlays; `TTFont`/`UnicodeCIDFont` for custom typography; `Drawing` charts/barcodes as flowables; a byte-stream target for in-memory output.
- Reject: hand-rolled PDF byte emission; a parallel layout engine where platypus applies; repeated `drawString` where a `PDFTextObject` batches; post-synthesis editing/signing/extraction that `pikepdf`/`pypdf`/`pymupdf`/`pyhanko` own; a wrapper-rename of `build`/`Canvas`.
