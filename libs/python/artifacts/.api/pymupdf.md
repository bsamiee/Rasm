# [PY_ARTIFACTS_API_PYMUPDF]

`pymupdf` supplies the native MuPDF-backed PDF and document surface for the artifacts pdf rail: a document root, a page unit, a rasterizing pixmap, and a text/geometry extraction surface that drive rendering, text/image extraction, redaction, annotation, and page-level editing across PDF and the MuPDF document family. The package owner composes `Document`, `Page`, `Pixmap`, and `TextPage` into the pdf owner; it never re-implements the MuPDF rendering pipeline the native core already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pymupdf`
- package: `pymupdf`
- import: `pymupdf` (legacy alias `fitz`)
- owner: `artifacts`
- rail: pdf
- installed: `1.27.2.3` reflected via `python -c "import pymupdf"` on cp315
- entry points: none (library only)
- capability: PDF/XPS/EPUB/CBZ/image document open, page rasterization, per-page OCR, text/image/table extraction, native outline and embedded-file recovery, drawing/annotation/redaction authoring, redaction-grade scrub/bake/font-subset, lossless image embed, reflowable story layout, page assembly, OCG layers, incremental save

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, page, and rendering roots
- rail: pdf

| [INDEX] | [SYMBOL]         | [PACKAGE_ROLE] | [CAPABILITY]                                                                         |
| :-----: | :--------------- | :------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `Document`       | document root  | open/save a document; page access, metadata, TOC, layers, embedded files, conversion |
|  [02]   | `Page`           | page unit      | render, extract, draw, annotate, redact, geometry boxes                              |
|  [03]   | `Pixmap`         | raster buffer  | rendered RGBA/CMYK bitmap with save/encode and color ops                             |
|  [04]   | `TextPage`       | text model     | structured text/word/dict extraction from a page                                     |
|  [05]   | `DocumentWriter` | author sink    | build a new document from drawn pages                                                |
|  [06]   | `Annot`          | annotation     | a single page annotation with update/appearance control                              |
|  [07]   | `Font`           | font handle    | font for inserted text and glyph metrics                                             |
|  [08]   | `Story`          | flow layout    | reflowable HTML-to-PDF story layout                                                  |
|  [09]   | `TableFinder`    | table engine   | resolved-table container from `find_tables`                                          |
|  [10]   | `Table`          | cell grid      | one resolved table with `extract`/`bbox`/`row_count`/`col_count`                     |
|  [11]   | `Widget`         | form widget    | interactive form-field widget for `add_widget`                                       |

[PUBLIC_TYPE_SCOPE]: geometry and color value objects
- rail: pdf

| [INDEX] | [SYMBOL]         | [PACKAGE_ROLE] | [CAPABILITY]                              |
| :-----: | :--------------- | :------------- | :---------------------------------------- |
|  [01]   | `Rect` / `IRect` | rectangle      | float/integer page rectangle algebra      |
|  [02]   | `Point`          | point          | 2D point                                  |
|  [03]   | `Matrix`         | affine matrix  | transform for render scale/rotate         |
|  [04]   | `Quad`           | quadrilateral  | rotated text/region bounds                |
|  [05]   | `Colorspace`     | color space    | CS_RGB/CS_GRAY/CS_CMYK pixmap color space |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: pdf

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE]   | [CAPABILITY]                     |
| :-----: | :------------------ | :--------------- | :------------------------------- |
|  [01]   | `FileDataError`     | data fault       | corrupt or invalid document data |
|  [02]   | `FileNotFoundError` | resolution fault | document path absent             |
|  [03]   | `EmptyFileError`    | empty fault      | zero-length document             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document open and save
- rail: pdf

Document rows carry path/stream/filetype input, save compaction, encryption, conversion, password, and splice policy.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                       | [CAPABILITY]                            |
| :-----: | :------------------------ | :--------------------------------- | :-------------------------------------- |
|  [01]   | `open`                    | path or stream plus filetype       | open from path or in-memory stream      |
|  [02]   | `Document`                | constructor alias policy           | document constructor (alias of `open`)  |
|  [03]   | `Document.save`           | target plus save policy            | save with compaction/encryption knobs   |
|  [04]   | `Document.ez_save`        | target plus defaulted kwargs       | save with deflate/garbage defaults on   |
|  [05]   | `Document.convert_to_pdf` | page range plus rotation           | convert a non-PDF document to PDF bytes |
|  [06]   | `Document.authenticate`   | password                           | unlock an encrypted document            |
|  [07]   | `Document.insert_pdf`     | source document plus splice policy | splice pages from another document      |

[ENTRYPOINT_SCOPE]: outline and embedded files
- rail: pdf

Native TOC and embedded-file rows recover and author the document outline and the attached-file table without re-deriving from `get_images`/`get_links`.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                                   | [CAPABILITY]                                        |
| :-----: | :----------------------- | :----------------------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `Document.get_toc`       | `get_toc(simple=True) -> list[[level, title, page, ...]]`                      | recover the nested outline as level/title/page rows |
|  [02]   | `Document.set_toc`       | `set_toc(toc, collapse=1) -> int`                                              | author/replace the outline from level rows          |
|  [03]   | `Document.embfile_names` | `embfile_names() -> list[str]`                                                 | list embedded-file entry names                      |
|  [04]   | `Document.embfile_get`   | `embfile_get(item: int \| str) -> bytes`                                       | read one embedded file's bytes by name/index        |
|  [05]   | `Document.embfile_info`  | `embfile_info(item: int \| str) -> dict`                                       | embedded-file metadata (filename, desc, size)       |
|  [06]   | `Document.embfile_count` | `embfile_count() -> int`                                                       | embedded-file entry count                           |
|  [07]   | `Document.embfile_add`   | `embfile_add(name, buffer_, filename=None, ufilename=None, desc=None) -> None` | attach a new embedded file                          |
|  [08]   | `Document.embfile_upd`   | `embfile_upd(item, buffer_=None, filename=None, ufilename=None, desc=None)`    | replace an embedded file's bytes/metadata           |
|  [09]   | `Document.embfile_del`   | `embfile_del(item: int \| str)`                                                | drop an embedded file                               |

[ENTRYPOINT_SCOPE]: redaction-grade scrub, bake, and font subset
- rail: pdf

Document-level sanitization rows; `scrub` is the redaction-grade remover, `bake` flattens interactive layers, `subset_fonts` shrinks embedded fonts in place.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                                                                                                                                                                                                                                    | [CAPABILITY]                                                |
| :-----: | :---------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `Document.scrub`        | `scrub(attached_files=True, clean_pages=True, embedded_files=True, hidden_text=True, javascript=True, metadata=True, redactions=True, redact_images=0, remove_links=True, reset_fields=True, reset_responses=True, thumbnails=True, xml_metadata=True) -> None` | redaction-grade removal of hidden/attached/metadata content |
|  [02]   | `Document.bake`         | `bake(*, annots=True, widgets=True) -> None`                                                                                                                                                                                                                    | flatten annotations and form widgets into content           |
|  [03]   | `Document.subset_fonts` | `subset_fonts(verbose=False, fallback=False) -> int \| None`                                                                                                                                                                                                    | subset embedded fonts in place, returning byte delta        |

[ENTRYPOINT_SCOPE]: redaction-mark, widget, and table authoring
- rail: pdf

Page-level authoring rows; `add_redact_annot` marks regions for `apply_redactions`, the widget rows recover/author form fields, `find_tables` resolves ruled tables natively.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                                                                                                                                                                                                      | [CAPABILITY]                                            |
| :-----: | :---------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `Page.add_redact_annot` | `add_redact_annot(quad, text=None, fontname=None, fontsize=11, align=0, fill=None, text_color=None, cross_out=True) -> Annot`                                                                                                     | mark a region for redaction before `apply_redactions`   |
|  [02]   | `Page.find_tables`      | `find_tables(clip=None, vertical_strategy="lines", horizontal_strategy="lines", snap_tolerance=3, join_tolerance=3, edge_min_length=3, intersection_tolerance=3, text_tolerance=3, strategy=None, add_lines=None) -> TableFinder` | resolve ruled/text tables natively into a `TableFinder` |
|  [03]   | `Page.widgets`          | `widgets(types=None) -> Iterator[Annot]`                                                                                                                                                                                          | iterate form-field widgets on the page                  |
|  [04]   | `Page.add_widget`       | `add_widget(widget: Widget) -> Annot`                                                                                                                                                                                             | author a new interactive form widget                    |

[ENTRYPOINT_SCOPE]: annotation recovery and image-document assembly
- rail: pdf

`Page.annots` iterates existing annotations whose `Annot.info`/`Annot.rect` accessors recover content and geometry; `Page.rect` reads the page box; `Document.is_image`/`new_page`/`tobytes` assemble a fresh single-page PDF around a raster stream for the OCR-intake path that `insert_image` embeds losslessly. The `Widget` accessors recover a form field's name, type, and value.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                                 | [CAPABILITY]                                          |
| :-----: | :------------------- | :--------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `Page.annots`        | `annots(types=None) -> Iterator[Annot]`                                      | iterate existing annotations on the page              |
|  [02]   | `Annot.info`         | `info -> dict` (`content`/`title`/`subject`/`name`/`creationDate`/`modDate`) | annotation metadata dictionary                        |
|  [03]   | `Annot.rect`         | `rect -> Rect`                                                               | annotation bounding rectangle                         |
|  [04]   | `Annot.type`         | `type -> (int, str)`                                                         | annotation type number and name                       |
|  [05]   | `Page.rect`          | `rect -> Rect`                                                               | the page rectangle (media/crop box)                   |
|  [06]   | `Widget.field_name`  | `field_name -> str`                                                          | the form field's name                                 |
|  [07]   | `Widget.field_value` | `field_value -> str \| bool \| None`                                         | the form field's current value                        |
|  [08]   | `Widget.field_type`  | `field_type -> int`                                                          | the form field's `PDF_WIDGET_TYPE_*` int discriminant |
|  [09]   | `Document.is_image`  | `is_image -> bool`                                                           | the document is a single-image format                 |
|  [10]   | `Document.new_page`  | `new_page(pno=-1, width=595, height=842) -> Page`                            | append a blank page and return it                     |
|  [11]   | `Document.tobytes`   | `tobytes(garbage=0, deflate=False, ...) -> bytes`                            | serialize the in-memory document to PDF bytes         |

[ENTRYPOINT_SCOPE]: render and extraction
- rail: pdf

Render and extraction rows share matrix/dpi, color, clip, text mode, textpage, search, image, redaction, and raster-output policy.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                                                                                                                                          | [CAPABILITY]                                                    |
| :-----: | :---------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `Page.get_pixmap`       | rasterization policy -> `Pixmap`                                                                                                                                      | rasterize a page                                                |
|  [02]   | `Page.get_text`         | text mode plus clip/sort policy                                                                                                                                       | extract text in plain/dict/json/words/html modes                |
|  [03]   | `Page.get_textpage`     | clip plus flags                                                                                                                                                       | build a reusable text model                                     |
|  [04]   | `Page.get_textpage_ocr` | `get_textpage_ocr(flags=0, language="eng", dpi=72, full=False, tessdata=None) -> TextPage`                                                                            | Tesseract-OCR a page into a searchable `TextPage`               |
|  [05]   | `Page.search_for`       | needle plus match policy                                                                                                                                              | locate text regions                                             |
|  [06]   | `Page.get_images`       | full-image flag                                                                                                                                                       | enumerate embedded images                                       |
|  [07]   | `Page.apply_redactions` | `apply_redactions(images=PDF_REDACT_IMAGE_REMOVE, graphics=PDF_REDACT_LINE_ART_REMOVE_IF_TOUCHED, text=PDF_REDACT_TEXT_REMOVE) -> bool`                               | burn in redaction annotations (module `PDF_REDACT_*` int flags) |
|  [08]   | `Page.insert_image`     | `insert_image(rect, *, stream=None, filename=None, pixmap=None, xref=0, alpha=-1, keep_proportion=True, overlay=True, rotate=0, width=0, height=0, mask=None) -> int` | embed a raster losslessly from stream/file/pixmap               |
|  [09]   | `Pixmap`                | `Pixmap(*args)` (e.g. `Pixmap(doc, xref)`, `Pixmap(colorspace, irect, alpha)`)                                                                                        | construct a raster buffer from a doc xref or spec               |
|  [10]   | `Pixmap.save`           | target plus output policy                                                                                                                                             | encode raster to PNG/JPEG/etc.                                  |
|  [11]   | `Pixmap.tobytes`        | `tobytes(output="png", jpg_quality=95) -> bytes`                                                                                                                      | encode raster to bytes                                          |
|  [12]   | `TableFinder.tables`    | attribute -> `list[Table]`                                                                                                                                            | resolved tables from `find_tables`                              |
|  [13]   | `Table.extract`         | `extract(**kwargs) -> list[list[str \| None]]`                                                                                                                        | row-major cell text of one resolved table                       |
|  [14]   | `Table.bbox`            | attribute -> `(x0, y0, x1, y1)`                                                                                                                                       | bounding box of a resolved table                                |
|  [15]   | `Table.row_count`       | attribute -> `int`                                                                                                                                                    | resolved table row count                                        |
|  [16]   | `Table.col_count`       | attribute -> `int`                                                                                                                                                    | resolved table column count                                     |

[ENTRYPOINT_SCOPE]: reflowable story layout
- rail: pdf

The `Story`/`DocumentWriter` reflow loop lays reflowable HTML into a fresh PDF page sequence.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                                         | [CAPABILITY]                                        |
| :-----: | :-------------------------- | :--------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `Story`                     | `Story(html="", user_css=None, em=12, archive=None)` | build a reflowable HTML story                       |
|  [02]   | `Story.place`               | `place(where, flags=0) -> (more, filled)`            | lay the next slice into a rect, signalling overflow |
|  [03]   | `Story.draw`                | `draw(device, matrix=None)`                          | render the placed slice onto a writer device        |
|  [04]   | `DocumentWriter`            | `DocumentWriter(path, options="")`                   | author sink for drawn pages (path or `BytesIO`)     |
|  [05]   | `DocumentWriter.begin_page` | `begin_page(mediabox) -> Device`                     | open a page and hand back its draw device           |
|  [06]   | `DocumentWriter.end_page`   | `end_page()`                                         | close the current page                              |
|  [07]   | `DocumentWriter.close`      | `close()`                                            | finalize the written document                       |
|  [08]   | `paper_rect`                | `paper_rect(s: str) -> Rect`                         | named paper-size rect (e.g. `"a4"`)                 |

## [04]-[IMPLEMENTATION_LAW]

[PDF_NATIVE]:
- import: `import pymupdf` at boundary scope only; the `fitz` alias is not used.
- document axis: one `Document` owns every supported format (PDF/XPS/EPUB/CBZ/image); `filetype` is a row, never a per-format type.
- render axis: `Page.get_pixmap` with `matrix`/`dpi`/`colorspace` is the single rasterization entry; `Pixmap.save`/`tobytes` emit bytes the image/PDF owner consumes.
- extraction axis: `Page.get_text` with its mode row (plain/dict/json/words/html), `search_for`, and `find_tables` (the native `TableFinder`) are the extraction surface; results feed the document owner, never a re-minted text model.
- ocr axis: `Page.get_textpage_ocr(language=..., dpi=..., full=...)` grafts a Tesseract text layer onto a scanned page, returning a `TextPage` whose `extractWORDS`/`get_text` feed the same extraction owner; whole-document OCR-to-PDF/A stays with `ocrmypdf`, never a hand-stitched per-page graft.
- outline axis: `Document.get_toc(simple=...)`/`set_toc` is the native outline surface; outline recovery reads `get_toc` level/title/page rows rather than re-deriving from links, and `pypdf.PdfReader.outline` is the pure-Python core alternative.
- embedded axis: `Document.embfile_names`/`embfile_get`/`embfile_info`/`embfile_count` recover the native attached-file table directly; `get_images` enumerates placed rasters, a distinct concern, never the embedded-file recovery path.
- annotation axis: `Page.annots` iterates existing annotations and `Annot.info`/`Annot.rect`/`Annot.type` recover content/geometry/kind; `Page.widgets` plus `Widget.field_name`/`field_value`/`field_type` recover interactive form fields — each a native iterator/accessor, never an object-tree walk re-deriving the annotation table.
- image-assembly axis: `Document.is_image` gates the single-image intake, `new_page(width=, height=)` and `insert_image(rect, stream=)` wrap a raster stream into a fresh single-page PDF, and `tobytes` serializes it for the OCR feed; a separate raster-to-PDF library is the deleted form.
- sanitize axis: `Document.scrub` is the redaction-grade remover of hidden text, attachments, javascript, and metadata; `bake` flattens annotations/widgets into content; `subset_fonts` shrinks embedded fonts — each a document-level call row, never a hand-rolled object walker.
- image axis: `Page.insert_image(stream=...)`/`insert_image(filename=...)`/`insert_image(pixmap=...)` is the single lossless raster-embed entry, embedding the byte stream without recompression; a separate raster-to-PDF library is the deleted form.
- save axis: `save`/`ez_save` with `incremental`/`garbage`/`deflate` knobs is the single emission surface; encryption rides the save call, never a parallel encryptor.
- evidence: each operation captures page count, render dpi/matrix, color space, OCR language/dpi, redaction count, table cell-grid shape, embedded-file count, and output byte length as a pdf receipt.
- boundary: pymupdf owns native render/extract/ocr/redact/scrub/table/embed; pure-Python structural assembly routes to `pypdf`; whole-document OCR-to-PDF/A routes to `ocrmypdf`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pymupdf`
- Owns: native document open, page rasterization, per-page OCR, text/image/table extraction, native outline (`get_toc`/`set_toc`) and embedded-file recovery, drawing/annotation/redaction authoring, redaction-grade `scrub`/`bake`/`subset_fonts`, lossless `insert_image` raster embed, reflowable `Story` layout, page assembly, layers, incremental save
- Accept: render, OCR, table, outline, and embedded-file recovery feeding the document/PDF and image owners; lossless raster embed feeding the image-to-PDF intake
- Reject: wrapper-renames of `get_pixmap`/`get_text`/`find_tables`/`get_toc`/`embfile_get`; a second rasterizer where `pypdfium2` already covers a render path; an embedded-file recovery re-derived from `get_images`; a whole-document OCR-to-PDF/A pipeline where `ocrmypdf` owns it; a separate raster-to-PDF library where `insert_image` embeds losslessly; identity minting the runtime owns
