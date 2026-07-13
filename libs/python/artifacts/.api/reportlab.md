# [PY_ARTIFACTS_API_REPORTLAB]

`reportlab` supplies the programmatic PDF generation surface for the artifacts pdf rail across two composable layers: the low-level `pdfgen.canvas.Canvas` (direct coordinate drawing, text objects, path objects, interactive `acroForm` widgets, bookmarks/outlines/links) and the high-level `platypus` document model (`SimpleDocTemplate`/`BaseDocTemplate`, `Frame`, `PageTemplate`, and the `Flowable` content family flowed across pages with multi-pass TOC/index resolution); `pdfbase.pdfmetrics`/`TTFont` own font registration and metrics, `reportlab.graphics` owns vector drawings/charts/barcodes that embed as flowables, and `reportlab.lib` owns page sizes, units, colors, enums, and stylesheets. The package owner composes platypus for paginated content and the canvas for fixed-coordinate overlays/forms; it never hand-rolls PDF byte emission or a parallel layout engine.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `reportlab`
- package: `reportlab`
- import: `reportlab`
- owner: `artifacts`
- rail: pdf
- installed: `5.0.0`
- entry points: none (library only); optional `pillow` enables `Image`/`drawImage` raster embedding and `renderPM` raster output
- capability: imperative canvas drawing (text/path/image/transform/state, interactive AcroForm fields, bookmarks/outline/internal+external links, page transitions, encryption), declarative platypus flowable pagination (paragraphs/tables/lists/frames/templates, multi-pass TOC/index, doc-programming flowables), TrueType/Type1/CID font registration and metrics, and a vector graphics/charts/barcode engine that embeds as flowables or renders to PDF/PNG/SVG

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

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY] | [ROLE]                                                    |
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
- rail: pdf — `reportlab.pdfgen`, `reportlab.pdfbase.acroform`

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY] | [ROLE]                                                        |
| :-----: | :------------------------------------------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `pdfgen.textobject.PDFTextObject`                        | text builder  | batched text run via `beginText` (leading/spacing/rise/color) |
|  [02]   | `pdfgen.pathobject.PDFPathObject`                        | path builder  | composable path via `beginPath` (moveTo/lineTo/curveTo/...)   |
|  [03]   | `pdfbase.acroform.AcroForm`                              | form root     | `canvas.acroForm`; interactive-widget factory root            |
|  [04]   | `pdfmetrics.Font` / `TTFont` / `cidfonts.UnicodeCIDFont` | font handle   | registered Type1/TrueType/CID font descriptors                |

[PUBLIC_TYPE_SCOPE]: vector graphics, charts, and barcode family
- rail: pdf — `reportlab.graphics`

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [ROLE]                                                              |
| :-----: | :------------------------------------------------ | :------------ | :------------------------------------------------------------------ |
|  [01]   | `graphics.shapes.Drawing`                         | flowable      | vector canvas (shapes/groups) embeddable as a flowable              |
|  [02]   | `graphics.shapes.*`                               | shape         | primitives `Line`/`Rect`/`Circle`/`Polygon`/`String`/`Group`/`Path` |
|  [03]   | `graphics.charts.*`                               | chart         | data-driven `barcharts`/`linecharts`/`piecharts`/`lineplots`        |
|  [04]   | `graphics.barcode.createBarcodeDrawing(codeName)` | factory       | barcode/QR `Drawing` by symbology name                              |
|  [05]   | `graphics.renderPDF` / `renderPM` / `renderSVG`   | renderer      | render a `Drawing` to PDF / PNG (PM) / SVG bytes or file            |

[PUBLIC_TYPE_SCOPE]: style and lib family
- rail: pdf — `reportlab.lib`

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [ROLE]                                                     |
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
- rail: pdf — `reportlab.pdfgen.canvas.Canvas`

`Canvas(...)` opens the page surface; `filename` may be a path or a writable byte stream. The lifecycle/configure/metadata methods below drive it.

```python signature
Canvas(
    filename, pagesize=None, bottomup=1, pageCompression=None, encrypt=None,
    cropMarks=None, pdfVersion=None, enforceColorSpace=None, initialFontName=None,
    cropBox=None, artBox=None, trimBox=None, bleedBox=None, lang=None,
)
```

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [ROLE]                                      |
| :-----: | :------------------------------------------------------ | :------------- | :------------------------------------------ |
|  [01]   | `Canvas.showPage()`                                     | lifecycle      | finish the current page                     |
|  [02]   | `Canvas.save()`                                         | lifecycle      | write the PDF to the target                 |
|  [03]   | `Canvas.getpdfdata()`                                   | lifecycle      | return PDF bytes instead of a file          |
|  [04]   | `Canvas.setPageSize(size)` / `setPageRotation(deg)`     | configure      | set page dimensions / rotation              |
|  [05]   | `Canvas.setAuthor` / `setTitle` / `setSubject`          | metadata       | document info: author, title, subject       |
|  [06]   | `Canvas.setCreator` / `setKeywords` / `setProducer`     | metadata       | document info: creator, keywords, producer  |
|  [07]   | `Canvas.setEncrypt(encrypt)` / `setPageTransition(...)` | configure      | encryption handler; presentation transition |

[ENTRYPOINT_SCOPE]: canvas drawing
- rail: pdf — `reportlab.pdfgen.canvas.Canvas`

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY] | [ROLE]                              |
| :-----: | :----------------------------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `Canvas.drawString(x, y, text, mode=None, charSpace=0, ...)`             | text           | left-aligned text                   |
|  [02]   | `Canvas.drawCentredString(x, y, text, ...)`                              | text           | centered text                       |
|  [03]   | `Canvas.drawRightString(x, y, text, ...)`                                | text           | right-aligned text                  |
|  [04]   | `Canvas.setFont(psfontname, size, leading=None)`                         | text           | select active font                  |
|  [05]   | `Canvas.drawImage(image, x, y, width=None, height=None, mask=None, ...)` | image          | place a raster image                |
|  [06]   | `Canvas.line(x1, y1, x2, y2)`                                            | path           | straight line                       |
|  [07]   | `Canvas.rect(x, y, width, height, stroke=1, fill=0)`                     | path           | rectangle                           |
|  [08]   | `Canvas.circle(x_cen, y_cen, r, stroke=1, fill=0)`                       | path           | circle                              |
|  [09]   | `Canvas.setFillColor(aColor, alpha=None)`                                | color          | fill color                          |
|  [10]   | `Canvas.setStrokeColor(aColor, alpha=None)`                              | color          | stroke color                        |
|  [11]   | `Canvas.translate(dx, dy)` / `scale(x, y)` / `rotate(theta)`             | transform      | translate / scale / rotate          |
|  [12]   | `Canvas.transform(a, b, c, d, e, f)`                                     | transform      | full affine matrix concatenation    |
|  [13]   | `Canvas.saveState()` / `restoreState()`                                  | state          | graphics-state stack                |
|  [14]   | `Canvas.setFillColorRGB` / `setFillColorCMYK` / `setStrokeColorRGB`      | color          | direct RGB/CMYK color-space setters |
|  [15]   | `Canvas.setFillAlpha` / `setStrokeAlpha`                                 | color          | fill / stroke transparency setters  |
|  [16]   | `Canvas.setDash(array, phase)` / `setLineWidth(w)`                       | stroke style   | line dash pattern / width           |
|  [17]   | `Canvas.setLineCap(c)` / `setLineJoin(j)`                                | stroke style   | line cap / join                     |
|  [18]   | `Canvas.arc`/`ellipse`/`roundRect`/`wedge`/`bezier`/`lines`/`grid`       | path           | extended primitive shapes           |

[ENTRYPOINT_SCOPE]: canvas text, path, interactive forms, and navigation
- rail: pdf — `reportlab.pdfgen.canvas.Canvas`, `reportlab.pdfbase.acroform.AcroForm`

The batched `PDFTextObject` (`beginText(x, y)` then `textLine`/`textLines`/`setLeading`/`setRise`/`setFont`, drawn by `drawText`) and `PDFPathObject` (`beginPath()` then `moveTo`/`lineTo`/`curveTo`/`arc`/`rect`/`close`, drawn/clipped by `drawPath`/`clipPath`) builders are the dense form for multi-line text and composite paths; `acroForm` owns interactive widgets; bookmarks/outline/links own in-document and external navigation.

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [ROLE]                                        |
| :-----: | :------------------------------------------------------------------ | :------------- | :-------------------------------------------- |
|  [01]   | `Canvas.beginText(x, y) -> PDFTextObject` / `drawText(t)`           | text           | batched multi-line text run                   |
|  [02]   | `Canvas.beginPath() -> PDFPathObject` / `drawPath` / `clipPath`     | path           | composite path build and clip                 |
|  [03]   | `Canvas.acroForm.textfield(name, value, x, y, width, height, ...)`  | form           | interactive text field widget                 |
|  [04]   | `Canvas.acroForm.checkbox` / `radio` / `choice` / `listbox`         | form           | checkbox/radio/dropdown/listbox widgets       |
|  [05]   | `Canvas.bookmarkPage(key)` / `bookmarkHorizontal(key, left, top)`   | navigation     | define an in-document jump target             |
|  [06]   | `Canvas.addOutlineEntry(title, key, ...)` / `setOutlineNames0(...)` | navigation     | PDF bookmark/outline tree                     |
|  [07]   | `Canvas.linkAbsolute(...)` / `linkRect(...)` / `linkURL(...)`       | navigation     | internal link to bookmark / external URL link |
|  [08]   | `Canvas.beginForm(name, ...)` / `endForm()` / `doForm(name)`        | reuse          | reusable XObject form                         |
|  [09]   | `Canvas.drawInlineImage(image, ...)`                                | reuse          | inline raster image                           |

[ENTRYPOINT_SCOPE]: vector graphics, charts, and barcodes
- rail: pdf — `reportlab.graphics`

A `Drawing` is itself a `Flowable`, so charts/barcodes embed directly into a platypus story; the `render*` modules also rasterize/vectorize a standalone `Drawing`.

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [ROLE]                                               |
| :-----: | :--------------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `graphics.shapes.Drawing(width, height)` + `.add(shape)`         | construct      | a vector canvas embeddable as a flowable             |
|  [02]   | `graphics.barcode.createBarcodeDrawing(codeName, **options)`     | barcode        | barcode/QR `Drawing` by symbology name               |
|  [03]   | `graphics.renderPDF.draw(d, canvas, x, y)` / `drawToFile(...)`   | render         | place a `Drawing` on a canvas / write standalone PDF |
|  [04]   | `graphics.renderPM.drawToFile(d, fn, ...)` / `drawToString(...)` | render         | rasterize a `Drawing` to PNG/GIF (needs `_renderPM`) |
|  [05]   | `graphics.renderSVG.drawToString(d)` / `drawToFile(d, fn)`       | render         | serialize a `Drawing` to SVG                         |

[ENTRYPOINT_SCOPE]: document build and content
- rail: pdf — `reportlab.platypus`, `reportlab.lib.styles`

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [ROLE]                                         |
| :-----: | :----------------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `SimpleDocTemplate(filename, **kw)`                                | construct      | one-frame template (page size, margins)        |
|  [02]   | `SimpleDocTemplate.build(flowables, onFirstPage=, onLaterPages=)`  | build          | render a flowable story (`canvasmaker=Canvas`) |
|  [03]   | `BaseDocTemplate.addPageTemplates(pageTemplates)`                  | configure      | register page templates                        |
|  [04]   | `BaseDocTemplate.multiBuild(story, ...)`                           | build          | multi-pass build for indexes/TOC               |
|  [05]   | `Paragraph(text, style=None, bulletText=None, ...)`                | content        | styled rich-text flowable                      |
|  [06]   | `Table(data, colWidths=None, rowHeights=None, style=None, ...)`    | content        | grid flowable; `repeatRows=` repeats headers   |
|  [07]   | `Image(filename, width=None, height=None, kind='direct', ...)`     | content        | image flowable                                 |
|  [08]   | `getSampleStyleSheet()`                                            | style          | default named-style sheet                      |
|  [09]   | `ParagraphStyle(name, parent=None, **kw)`                          | style          | custom paragraph style                         |
|  [10]   | `TableOfContents()` + `notify('TOCEntry', (level, text, pageNum))` | toc            | multi-pass TOC; resolved by `multiBuild`       |
|  [11]   | `Frame(x1, y1, width, height, leftPadding=..., id=...)`            | layout         | a flow region; N frames per `PageTemplate`     |
|  [12]   | `PageTemplate(id, frames, onPage=..., onPageEnd=...)`              | layout         | named page layout binding frames to callbacks  |
|  [13]   | `DocAssign(var, expr)` / `DocIf(cond, thenBlock, elseBlock)`       | control        | compute/branch values during the build pass    |

[ENTRYPOINT_SCOPE]: font registration and metrics
- rail: pdf — `reportlab.pdfbase`

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [ROLE]                                           |
| :-----: | :------------------------------------------------------------------ | :------------- | :----------------------------------------------- |
|  [01]   | `pdfmetrics.registerFont(font)`                                     | register       | register a `TTFont`/`UnicodeCIDFont` for use     |
|  [02]   | `TTFont(name, filename, subfontIndex=0, ...)`                       | construct      | wrap a TrueType file as a registrable font       |
|  [03]   | `cidfonts.UnicodeCIDFont(face)`                                     | construct      | register a CJK/CID font face                     |
|  [04]   | `pdfmetrics.registerFontFamily(family, normal, bold, italic, ...)`  | register       | bind bold/italic variants for `<b>`/`<i>` markup |
|  [05]   | `pdfmetrics.stringWidth(text, fontName, fontSize, encoding='utf8')` | metric         | width of a string in points                      |
|  [06]   | `pdfmetrics.getFont(name)` / `getRegisteredFontNames()`             | metric         | resolve a registered font; list names            |
|  [07]   | `units.toLength(value)` / `mm` / `cm` / `inch` / `pica`             | unit           | convert measurements to points                   |

## [04]-[IMPLEMENTATION_LAW]

[REPORTLAB_TOPOLOGY]:
- two composable generation layers: imperative `Canvas` (absolute coordinates, text/path objects, forms, navigation, `showPage`/`save`) and declarative `platypus` (`build` a list of `Flowable` over `Frame`/`PageTemplate`). They meet at the `canvasmaker`/`onPage` callbacks — platypus flows content while a canvas callback paints fixed headers/footers/watermarks on the same page.
- platypus story: a list of flowables flowed by `SimpleDocTemplate.build(story, onFirstPage=, onLaterPages=)`; content that resolves across passes (TOC, `SimpleIndex`, back-references) requires `BaseDocTemplate.multiBuild`, which re-runs the layout until page numbers stabilize.
- canvas batching: multi-line text uses `beginText -> PDFTextObject -> drawText`, not repeated `drawString`; composite geometry uses `beginPath -> PDFPathObject -> drawPath/clipPath`. These cut PDF operator count and are the dense form.
- coordinate origin: bottom-left when `bottomup=1` (default); all `Canvas` coordinates are in points. `Frame` flows top-down internally regardless.
- `TableStyle` is a command list of `('BACKGROUND', (col, row), (col, row), color)`-style tuples (`SPAN`/`LINEBELOW`/`VALIGN`/`*PADDING`) applied to cell ranges; `LongTable` streams large data with `repeatRows`.
- fonts: built-in Type 1 fonts need no registration; TrueType needs `TTFont` + `pdfmetrics.registerFont`; CJK needs `UnicodeCIDFont`; bold/italic `<b>`/`<i>` markup in paragraphs needs `registerFontFamily` to bind the variants.
- graphics: a `graphics.shapes.Drawing` is a `Flowable`, so charts (`graphics.charts.*`) and barcodes (`createBarcodeDrawing`) embed directly in a story or render standalone via `renderPDF`/`renderPM`/`renderSVG`.
- units: all geometry is in points; multiply by `mm`/`cm`/`inch` from `reportlab.lib.units`; alignment is `lib.enums` constants.

[LOCAL_ADMISSION]:
- Use platypus (`SimpleDocTemplate`/`BaseDocTemplate` + flowables) for content that paginates; reserve `Canvas` for fixed-coordinate overlays, interactive forms, and per-page decoration via the `onPage` callback.
- Build styles from `getSampleStyleSheet` and derive with `ParagraphStyle(parent=...)`; do not construct ad hoc style dicts.
- Express all geometry in points scaled by `reportlab.lib.units` and alignment via `lib.enums`; never bake raw point literals for physical measurements.
- Register custom fonts once via `TTFont`/`UnicodeCIDFont` + `registerFont` (and `registerFontFamily` for markup variants) before first use in any flowable or canvas text.
- Batch multi-line text through `PDFTextObject` and composite geometry through `PDFPathObject`; reach for `drawString`/`line`/`rect` only for one-off marks.
- Use `Canvas.getpdfdata()` or construct the canvas/`SimpleDocTemplate` over a `BytesIO` to obtain bytes for an in-memory pipeline instead of writing a temp file.
- For TOC/index/cross-references, build with `multiBuild` and `notify('TOCEntry', ...)`; do not hand-compute page numbers.

[INTEGRATION]:
- reportlab is the PDF synthesis tier; post-synthesis editing/inspection routes to siblings, never re-implemented here: `pikepdf`/`pypdf` for merge/split/page-ops, `pymupdf` for render/extract, `pdfplumber` for table extraction, `pyhanko` for PAdES digital signing, `ocrmypdf` for OCR. The canonical rail is `reportlab -> BytesIO -> pikepdf/pyhanko` for downstream stamping/signing.
- it is one of three PDF producers by content shape: reportlab owns programmatic data-driven reports (tables/charts/forms from Python objects); `weasyprint` owns HTML/CSS-to-PDF (templated documents, paired with `jinja2`); `typst` owns markup-typeset PDF. Route by source shape, never duplicate a layout across engines.
- raster images embed via `pillow` (`Image`/`drawImage` accept a PIL image or `lib.utils.ImageReader` for decode-once reuse); barcodes/QR can come from the built-in `graphics.barcode` engine or be drawn from a sibling `segno`/`python-barcode` SVG rasterized through `resvg_py`.
- tabular data flows in as Python row lists for `Table`; for publication tables the `great-tables` owner is the alternative high-level surface — pick reportlab's `Table` when the output must compose into a larger paginated PDF, `great-tables` for standalone styled tables.

[RAIL_LAW]:
- Package: `reportlab`
- Owns: programmatic PDF synthesis via the imperative canvas (text/path objects, AcroForm widgets, bookmarks/outline/links, encryption, transitions) and the platypus flowable document model (frames/templates, multi-pass TOC/index, doc-programming flowables), plus font registration/metrics and the vector graphics/charts/barcode engine
- Accept: flowable stories for paginated content; canvas drawing for fixed layout/forms/overlays; `TTFont`/`UnicodeCIDFont` for custom typography; `Drawing` charts/barcodes as flowables; a byte stream target for in-memory output
- Reject: hand-rolled PDF byte emission; parallel layout engines where platypus applies; repeated `drawString` where a `PDFTextObject` batches; post-synthesis PDF editing/signing/extraction that `pikepdf`/`pypdf`/`pymupdf`/`pyhanko` own; wrapper-renames of `build`/`Canvas`
