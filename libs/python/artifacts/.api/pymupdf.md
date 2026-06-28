# [PY_ARTIFACTS_API_PYMUPDF]

`pymupdf` supplies the native MuPDF-backed PDF and document surface for the artifacts pdf rail: a document root, a page unit, a rasterizing pixmap, and a text/geometry extraction surface that drive rendering, text/image/table extraction, redaction, annotation, vector drawing, and page-level editing across PDF and the MuPDF document family. The package owner composes `Document`, `Page`, `Pixmap`, `TextPage`, and the `Shape`/`TextWriter`/`Story` authoring trio into the pdf owner; it never re-implements the MuPDF rendering pipeline, the `TEXTFLAGS_*` extraction modes, the redaction algebra, or the affine `Matrix` the native core already owns. The catalog drives the dense pdf rail where pymupdf renders/extracts/redacts, `pypdf` does pure-Python structural editing, `pypdfium2` does PDFium high-fidelity render, `pikepdf` owns AES-256-R6 + content-stream tokens, and `ocrmypdf` owns whole-document OCR-to-PDF/A — every owner meeting the others at PDF bytes or an in-memory `Pixmap`/`TextPage`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pymupdf`
- package: `pymupdf` (dist `PyMuPDF`)
- import: `pymupdf` (legacy alias `fitz`)
- owner: `artifacts`
- rail: pdf
- installed: `1.27.2.3`
- license: `AGPL-3.0-or-later OR Artifex-Commercial`; copyleft is whole-program — distributing a closed network service over an AGPL MuPDF link triggers source-disclosure obligations, so a commercial deployment routes structural editing to BSD `pypdf`/`pypdfium2` or licenses Artifex; this is the load-bearing licensing constraint of the pdf rail
- entry points: none (library only)
- capability: PDF/XPS/EPUB/CBZ/MOBI/FB2/SVG/image document open, page rasterization (`Pixmap` with matrix/dpi/colorspace/clip/alpha), per-page Tesseract OCR, text/image/table extraction (`TEXTFLAGS_*` modes, `find_tables` -> pandas/markdown), native outline and embedded-file recovery, vector drawing (`Shape`/`get_drawings`), positioned glyph authoring (`TextWriter`/`insert_htmlbox`), the full annotation-authoring family plus redaction, redaction-grade scrub/bake/font-subset/image-rewrite, lossless image embed, reflowable `Story` HTML layout, page assembly/reorder/copy, OCG layers, journalled undo/redo, AES-256/RC4 encrypted incremental save

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, page, and rendering roots
- rail: pdf

| [INDEX] | [SYMBOL]         | [PACKAGE_ROLE] | [CAPABILITY]                                                                         |
| :-----: | :--------------- | :------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `Document`       | document root  | open/save a document; page access, metadata, TOC, layers, embedded files, conversion |
|  [02]   | `Page`           | page unit      | render, extract, draw, annotate, redact, geometry boxes                              |
|  [03]   | `Pixmap`         | raster buffer  | rendered RGBA/CMYK bitmap with save/encode and color ops                             |
|  [04]   | `TextPage`       | text model     | structured text/word/dict extraction from a page                                     |
|  [05]   | `DocumentWriter` | author sink    | build a new document from drawn `Story`/`Shape` pages                                |
|  [06]   | `Annot`          | annotation     | a single page annotation with update/appearance/file/OC control                      |
|  [07]   | `Font`           | font handle    | font for inserted text, glyph metrics, and `has_glyph`/`valid_codepoints` probes     |
|  [08]   | `Story`          | flow layout    | reflowable HTML-to-PDF story layout (`Story.write` one-shot or `place`/`draw` loop)  |
|  [09]   | `Shape`          | vector canvas  | low-level vector draw canvas (`new_shape()`): line/rect/circle/bezier + finish/commit |
|  [10]   | `TextWriter`     | glyph writer   | font-bound positioned-glyph text accumulator, committed via `Page.write_text`        |
|  [11]   | `Widget`         | form widget    | interactive form-field widget for `add_widget`                                       |
|  [12]   | `Link`           | link object    | a single page link with `LINK_*` kind, URI/dest, and rect                            |
|  [13]   | `Outline`        | outline node   | a single TOC node (`title`/`dest`/`next`/`down`) for tree walks                      |
|  [14]   | `table.TableFinder` | table engine   | resolved-table container from `find_tables` (submodule `pymupdf.table`)              |
|  [15]   | `table.Table`    | cell grid      | one resolved table with `extract`/`to_pandas`/`to_markdown`/`bbox`/`rows`            |

[PUBLIC_TYPE_SCOPE]: geometry and color value objects
- rail: pdf

| [INDEX] | [SYMBOL]         | [PACKAGE_ROLE] | [CAPABILITY]                              |
| :-----: | :--------------- | :------------- | :---------------------------------------- |
|  [01]   | `Rect` / `IRect` | rectangle      | float/integer page rectangle algebra      |
|  [02]   | `Point`          | point          | 2D point                                  |
|  [03]   | `Matrix`         | affine matrix  | transform for render scale/rotate         |
|  [04]   | `Quad`           | quadrilateral  | rotated text/region bounds                |
|  [05]   | `Colorspace`     | color space    | `csRGB`/`csGRAY`/`csCMYK` singletons; `CS_RGB`/`CS_GRAY`/`CS_CMYK` int codes |

[PUBLIC_TYPE_SCOPE]: module-level discriminant constants
- rail: pdf

Bounded vocabularies the call rows discriminate on — module-level `int`/flag constants, never re-minted local enums. Combine the `TEXTFLAGS_*` and `PDF_REDACT_*` families by `|`.

| [INDEX] | [FAMILY]            | [MEMBERS]                                                                                                                  | [DRIVES]                                             |
| :-----: | :------------------ | :------------------------------------------------------------------------------------------------------------------------ | :--------------------------------------------------- |
|  [01]   | `TEXTFLAGS_*`       | `TEXT`/`WORDS`/`BLOCKS`/`DICT`/`RAWDICT`/`HTML`/`XHTML`/`XML`/`SEARCH` presets + `TEXT_PRESERVE_*`/`TEXT_DEHYPHENATE` bits | `get_text`/`get_textpage` flag composition           |
|  [02]   | `TEXT_OUTPUT_*`     | `TEXT`/`HTML`/`JSON`/`XHTML`/`XML`                                                                                         | `get_text` output kind                               |
|  [03]   | `PDF_REDACT_*`      | image `NONE`(0)/`REMOVE`(1)/`PIXELS`(2)/`REMOVE_UNLESS_INVISIBLE`; line-art `NONE`(0)/`REMOVE_IF_TOUCHED`(1)/`REMOVE_IF_COVERED`; text `REMOVE`(0)/`NONE`(1)/`REMOVE_INVISIBLE` (text family value order is INVERTED vs image — `0` is remove) | `apply_redactions(images=, graphics=, text=)` policy |
|  [04]   | `PDF_ENCRYPT_*`     | `KEEP`/`NONE`/`RC4_40`/`RC4_128`/`AES_128`/`AES_256`                                                                       | `save(encryption=)` strength                         |
|  [05]   | `PDF_PERM_*`        | `PRINT`/`MODIFY`/`COPY`/`ANNOTATE`/`FORM`/`ACCESSIBILITY`/`ASSEMBLE`/`PRINT_HQ`                                            | `save(permissions=)` bitmask                         |
|  [06]   | `PDF_WIDGET_TYPE_*` | `UNKNOWN`/`BUTTON`/`CHECKBOX`/`RADIOBUTTON`/`TEXT`/`LISTBOX`/`COMBOBOX`/`SIGNATURE`                                        | `Widget.field_type` discriminant                     |
|  [07]   | `PDF_ANNOT_*`       | `TEXT`/`FREE_TEXT`/`LINE`/`SQUARE`/`HIGHLIGHT`/`INK`/`STAMP`/`REDACT`/`WIDGET`/… kind codes + `PDF_ANNOT_IS_*` flag bits   | `Annot.type` discriminant / `set_flags`              |
|  [08]   | `STAMP_*`           | `Approved`/`Confidential`/`Draft`/`Final`/`TopSecret`/… 14 preset stamps                                                  | `add_stamp_annot(stamp=)`                            |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: pdf

`EmptyFileError` subclasses `FileDataError`; both are `RuntimeError`-derived. pymupdf rebinds the builtin name `FileNotFoundError` to its own resolution fault.

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE]   | [CAPABILITY]                                       |
| :-----: | :------------------ | :--------------- | :------------------------------------------------- |
|  [01]   | `FileDataError`     | data fault       | corrupt or invalid document data (`RuntimeError`)  |
|  [02]   | `FileNotFoundError` | resolution fault | document path absent                               |
|  [03]   | `EmptyFileError`    | empty fault      | zero-length document (subclass of `FileDataError`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document open and save
- rail: pdf

Document rows carry path/stream/filetype input, save compaction, encryption, conversion, password, and splice policy.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                       | [CAPABILITY]                            |
| :-----: | :------------------------ | :--------------------------------- | :-------------------------------------- |
|  [01]   | `open`                    | `open(filename=None, stream=None, filetype=None, ...)` (module fn, alias of `Document`) | open from path or in-memory stream      |
|  [02]   | `Document`                | `Document(filename=None, stream=None, filetype=None, rect=None, width=0, height=0, fontsize=11)` | the document constructor; `pymupdf.open` is the module-level alias |
|  [03]   | `Document.save`           | `save(filename, garbage=0, clean=0, deflate=0, deflate_images=0, deflate_fonts=0, incremental=0, ascii=0, expand=0, linear=0, no_new_id=0, appearance=0, pretty=0, encryption=1, permissions=4095, owner_pw=None, user_pw=None, preserve_metadata=1, use_objstms=0, compression_effort=0, raise_on_repair=False, ...)` | save with compaction/linearization/encryption knobs (`compression_effort` tunes brotli object-stream effort; `deflate_fonts` shrinks embedded fonts; `preserve_metadata` keeps the info dict) |
|  [04]   | `Document.ez_save`        | target plus defaulted kwargs       | save with deflate/garbage/use-objstms defaults on |
|  [05]   | `Document.convert_to_pdf` | page range plus rotation           | convert a non-PDF document to PDF bytes |
|  [06]   | `Document.authenticate`   | password                           | unlock an encrypted document            |
|  [07]   | `Document.insert_pdf`     | `insert_pdf(docsrc, *, from_page=-1, to_page=-1, start_at=-1, rotate=-1, links=1, annots=1, widgets=1, join_duplicates=0, show_progress=0)` | splice pages from another `Document`    |
|  [08]   | `Document.insert_file`    | source path/stream plus splice policy | splice pages from a file/stream of any supported format |
|  [09]   | `Document.select`         | `select(list[int])`                | reorder/subset the document to a page-index list (in place) |
|  [10]   | `Document.delete_pages`   | range or list of page indices      | drop pages (`delete_page` for one)      |
|  [11]   | `Document.copy_page` / `move_page` / `fullcopy_page` | source/target index | duplicate or relocate a page within the document |
|  [12]   | `Document.close` / context manager | `close() -> None`; `with pymupdf.open(...) as doc:` (`__enter__` returns the `Document`, `__exit__` calls `close`) | release the native MuPDF handle deterministically; the `with` bracket closes it on scope exit rather than GC-reaping the live handle |

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
|  [10]   | `Document.outline`       | `outline -> Outline` (then `.title`/`.dest`/`.next`/`.down`)                   | the outline as a linked-node tree for recursive walks |
|  [11]   | `Document.set_toc_item` / `del_toc_item` | `set_toc_item(idx, dest_dict=, ...)` / `del_toc_item(idx)`     | edit/drop a single outline entry in place           |

[ENTRYPOINT_SCOPE]: redaction-grade scrub, bake, and font subset
- rail: pdf

Document-level sanitization rows; `scrub` is the redaction-grade remover, `bake` flattens interactive layers, `subset_fonts` shrinks embedded fonts in place.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                                                                                                                                                                                                                                    | [CAPABILITY]                                                |
| :-----: | :---------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `Document.scrub`        | `scrub(attached_files=True, clean_pages=True, embedded_files=True, hidden_text=True, javascript=True, metadata=True, redactions=True, redact_images=0, remove_links=True, reset_fields=True, reset_responses=True, thumbnails=True, xml_metadata=True) -> None` | redaction-grade removal of hidden/attached/metadata content |
|  [02]   | `Document.bake`         | `bake(*, annots=True, widgets=True) -> None`                                                                                                                                                                                                                    | flatten annotations and form widgets into content           |
|  [03]   | `Document.subset_fonts` | `subset_fonts(verbose=False, fallback=False) -> int \| None`                                                                                                                                                                                                    | subset embedded fonts in place, returning byte delta        |

[ENTRYPOINT_SCOPE]: annotation recovery and image-document assembly
- rail: pdf

`Page.annots` iterates existing annotations whose `Annot.info`/`Annot.rect` accessors recover content and geometry; `Page.rect` reads the page box; the `Document.is_pdf`/`is_reflowable`/`is_stream`/`is_form_pdf` flags discriminate the opened format (a single-image input opens as a one-page non-PDF document — gate on `not is_pdf`); `new_page`/`tobytes` assemble a fresh single-page PDF around a raster stream for the OCR-intake path that `insert_image` embeds losslessly. The `Widget` instance accessors recover a form field's name, type, and value.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                                 | [CAPABILITY]                                          |
| :-----: | :----------------------- | :--------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `Page.annots`            | `annots(types=None) -> Iterator[Annot]`                                      | iterate existing annotations on the page              |
|  [02]   | `Annot.info`             | `info -> dict` (`content`/`title`/`subject`/`name`/`creationDate`/`modDate`) | annotation metadata dictionary                        |
|  [03]   | `Annot.rect`             | `rect -> Rect`                                                               | annotation bounding rectangle                         |
|  [04]   | `Annot.type`             | `type -> (int, str)`                                                         | annotation type number and name (`PDF_ANNOT_*`)       |
|  [05]   | `Annot.get_file` / `update_file` | `get_file() -> bytes` / `update_file(buffer=, filename=, ufilename=, desc=)` | read/replace a `FileAttachment` annotation's payload  |
|  [06]   | `Page.rect`              | `rect -> Rect`                                                               | the page rectangle (media/crop box)                   |
|  [07]   | `Widget.field_name`      | `field_name -> str` (instance attr)                                          | the form field's name                                 |
|  [08]   | `Widget.field_value`     | `field_value -> str \| bool \| None` (instance attr)                         | the form field's current value                        |
|  [09]   | `Widget.field_type`      | `field_type -> int` (instance attr)                                          | the form field's `PDF_WIDGET_TYPE_*` int discriminant |
|  [10]   | `Document.is_pdf`        | `is_pdf -> bool` (+ `is_reflowable`/`is_stream`/`is_form_pdf` siblings)       | opened-format discriminant; `not is_pdf` gates the single-image intake |
|  [11]   | `Document.new_page`      | `new_page(pno=-1, width=595, height=842) -> Page`                            | append a blank page and return it                     |
|  [12]   | `Document.tobytes`       | `tobytes(garbage=0, deflate=False, ...) -> bytes`                            | serialize the in-memory document to PDF bytes         |

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
|  [07]   | `Page.apply_redactions` | `apply_redactions(images=PDF_REDACT_IMAGE_PIXELS, graphics=PDF_REDACT_LINE_ART_REMOVE_IF_TOUCHED, text=PDF_REDACT_TEXT_REMOVE) -> bool` (literal defaults `2`/`1`/`0`; note `PDF_REDACT_TEXT_REMOVE==0` and `PDF_REDACT_TEXT_NONE==1`, inverted from the image family) | burn in redaction annotations (module `PDF_REDACT_*` int flags) |
|  [08]   | `Page.insert_image`     | `insert_image(rect, *, stream=None, filename=None, pixmap=None, xref=0, alpha=-1, keep_proportion=True, overlay=True, rotate=0, width=0, height=0, mask=None) -> int` | embed a raster losslessly from stream/file/pixmap               |
|  [09]   | `Pixmap`                | `Pixmap(*args)` (e.g. `Pixmap(doc, xref)`, `Pixmap(colorspace, irect, alpha)`)                                                                                        | construct a raster buffer from a doc xref or spec               |
|  [10]   | `Pixmap.save`           | target plus output policy                                                                                                                                             | encode raster to PNG/JPEG/etc.                                  |
|  [11]   | `Pixmap.tobytes`        | `tobytes(output="png", jpg_quality=95) -> bytes`                                                                                                                      | encode raster to PNG/PSD/PPM/PNM/PGM/PBM/PAM/PS/PSD/JPEG bytes   |
|  [12]   | `Pixmap.pil_tobytes`    | `pil_tobytes(format, **pil_kwargs) -> bytes`                                                                                                                          | bridge to Pillow's encoders (WEBP/AVIF/TIFF) via `Image.save`   |
|  [13]   | `Pixmap` color ops      | `set_alpha` / `invert_irect` / `gamma_with` / `tint_with` / `shrink` / `warp` / `color_count` / `digest`                                                              | in-place buffer color/geometry ops + dedup digest (`samples` mv) |
|  [14]   | `Document.rewrite_images` | `rewrite_images(dpi_threshold=None, dpi_target=0, quality=0, lossy=True, lossless=True, color=True, gray=True, set_to_gray=False) -> None`                          | recompress/downsample embedded images in place (size reduction) |
|  [15]   | `table.TableFinder.tables` | attribute -> `list[Table]`                                                                                                                                         | resolved tables from `find_tables`                              |
|  [16]   | `table.Table.extract`   | `extract(**kwargs) -> list[list[str \| None]]`                                                                                                                       | row-major cell text of one resolved table                       |
|  [17]   | `table.Table.to_pandas` | `to_pandas(**kwargs) -> pandas.DataFrame`                                                                                                                             | resolved table as a DataFrame (feeds the table/dataframe owner) |
|  [18]   | `table.Table.to_markdown` | `to_markdown(clean=True) -> str`                                                                                                                                   | resolved table as a GitHub-flavoured Markdown grid              |
|  [19]   | `table.Table` shape     | `bbox -> (x0,y0,x1,y1)` / `row_count` / `col_count` / `rows -> list[TableRow]` / `header -> TableHeader`                                                              | resolved-table geometry and row/header objects                  |

[ENTRYPOINT_SCOPE]: vector drawing, glyph authoring, and annotation/redaction authoring
- rail: pdf

`Page.new_shape()` returns a `Shape` vector canvas; `Page.write_text` commits a font-bound `TextWriter`; `Page.insert_htmlbox` flows styled HTML into a rect (the modern one-shot text-in-box). `Page.get_drawings`/`get_cdrawings` recover the page's vector path list. The `add_*_annot` family authors every annotation kind; `add_redact_annot` marks regions burned by `apply_redactions`. `Page.draw_*` are thin wrappers that auto-commit a one-shot `Shape`.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                                                       | [CAPABILITY]                                                |
| :-----: | :----------------------- | :------------------------------------------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `Page.new_shape`         | `new_shape() -> Shape`; then `draw_line`/`draw_rect`/`draw_circle`/`draw_bezier`/`draw_polyline`/`draw_oval`/`draw_sector`/`draw_curve` + `finish(...)` + `commit(overlay=)` | accumulate vector geometry then burn it onto the page       |
|  [02]   | `Page.draw_*`            | `draw_line` / `draw_rect` / `draw_circle` / `draw_bezier` / `draw_polyline` (+ all `Shape` verbs)  | one-shot vector primitive (auto-`Shape` + `commit`)         |
|  [03]   | `Page.get_drawings`      | `get_drawings(extended=False) -> list[dict]`                                                        | recover the page vector path list (fills/strokes/clips)     |
|  [04]   | `Page.insert_htmlbox`    | `insert_htmlbox(rect, text, *, css=None, scale_low=0, archive=None, rotate=0, oc=0, opacity=1, overlay=True) -> tuple` | flow styled HTML/CSS into a rect, returning fill spillover  |
|  [05]   | `Page.write_text`        | `write_text(rect=None, writers=, ...)` over a `TextWriter` (`append(pos, text, font=, fontsize=)`) | commit positioned-glyph text built by a `TextWriter`        |
|  [06]   | `Page.insert_text` / `insert_textbox` | point/rect + `fontname`/`fontsize`/`color`/`rotate`/`align`                            | simple text insert (single line / wrapped box)              |
|  [07]   | `Page.add_redact_annot`  | `add_redact_annot(quad, text=None, fontname=None, fontsize=11, align=0, fill=None, text_color=None, cross_out=True) -> Annot` | mark a region for redaction before `apply_redactions`       |
|  [08]   | `Page.add_*_annot`       | `add_highlight_annot` / `add_text_annot` / `add_freetext_annot` / `add_line_annot` / `add_rect_annot` / `add_circle_annot` / `add_ink_annot` / `add_stamp_annot` / `add_file_annot` / `add_squiggly_annot` / `add_strikeout_annot` / `add_underline_annot` / `add_polyline_annot` / `add_polygon_annot` | author every annotation kind; `Annot.update()` bakes appearance |
|  [09]   | `Page.find_tables`       | `find_tables(clip=None, vertical_strategy="lines", horizontal_strategy="lines", snap_tolerance=3, join_tolerance=3, edge_min_length=3, intersection_tolerance=3, text_tolerance=3, strategy=None, add_lines=None) -> table.TableFinder` | resolve ruled/text tables natively into a `TableFinder`     |
|  [10]   | `Page.widgets` / `add_widget` | `widgets(types=None) -> Iterator[Annot]` / `add_widget(widget: Widget) -> Annot`              | iterate / author interactive form-field widgets             |
|  [11]   | `Page.get_links` / `insert_link` / `delete_link` | `get_links() -> list[dict]` / `insert_link(dict)` / `delete_link(dict)`             | recover/author `LINK_*` link objects on the page            |

[ENTRYPOINT_SCOPE]: OCG layers and journalled editing
- rail: pdf

Optional-content groups gate visibility; the journal is the native undo/redo + snapshot rail for transactional document edits.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                              | [CAPABILITY]                                          |
| :-----: | :------------------------- | :----------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `Document.add_ocg`         | `add_ocg(name, config=-1, on=True, intent="View", usage="Artwork") -> int` | create an optional-content group, return its xref     |
|  [02]   | `Document.set_oc` / `get_oc` | xref + ocg-xref                                                        | bind/read an object's optional-content membership     |
|  [03]   | `Document.layer_ui_configs` / `set_layer` | config selectors                                          | enumerate/toggle layer UI configurations              |
|  [04]   | `Document.journal_enable`  | `journal_enable()` then `journal_start_op(name)` / `journal_stop_op()`   | open the undo/redo journal and bracket an operation   |
|  [05]   | `Document.journal_undo` / `journal_redo` | no-arg                                                     | step the journal backward/forward                     |
|  [06]   | `Document.save_snapshot`   | `save_snapshot(filename)`                                                | write an incremental snapshot tied to the journal     |

[ENTRYPOINT_SCOPE]: reflowable story layout
- rail: pdf

`Story.write` is the one-shot reflow entry (a `Story` + a `rectfn` callback drive a full `DocumentWriter` page sequence in one call); the `place`/`draw` loop is the manual modality, and `write_stabilized`/`add_pdf_links` add stabilized-HTML and live-link variants.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                                                         | [CAPABILITY]                                        |
| :-----: | :-------------------------- | :------------------------------------------------------------------ | :-------------------------------------------------- |
|  [01]   | `Story`                     | `Story(html="", user_css=None, em=12, archive=None)`                | build a reflowable HTML story                       |
|  [02]   | `Story.write`               | `write(writer, rectfn, positionfn=None, pagefn=None)`               | one-shot: lay the whole story across writer pages   |
|  [03]   | `Story.write_stabilized`    | `write_stabilized(writer, contentfn, rectfn, ...)`                  | iterative layout over regenerated HTML until stable |
|  [04]   | `Story.place` / `draw`      | `place(where, flags=0) -> (more, filled)` / `draw(device, matrix=None)` | manual slice-by-slice layout + render               |
|  [05]   | `Story.add_pdf_links`       | `add_pdf_links(document_or_stream, positions)`                      | inject live PDF links from placed element positions |
|  [06]   | `DocumentWriter`            | `DocumentWriter(path, options="")`                                  | author sink for drawn pages (path or `BytesIO`)     |
|  [07]   | `DocumentWriter.begin_page` | `begin_page(mediabox) -> Device`                                    | open a page and hand back its draw device           |
|  [08]   | `DocumentWriter.end_page`   | `end_page()`                                                        | close the current page                              |
|  [09]   | `DocumentWriter.close`      | `close()`                                            | finalize the written document                       |
|  [10]   | `paper_rect`                | `paper_rect(s: str) -> Rect`                         | named paper-size rect (e.g. `"a4"`)                 |

## [04]-[IMPLEMENTATION_LAW]

[PDF_NATIVE]:
- import: `import pymupdf` at boundary scope only; the `fitz` alias is not used.
- document axis: one `Document` owns every supported format (PDF/XPS/EPUB/CBZ/image); `filetype` is a row, never a per-format type.
- render axis: `Page.get_pixmap` with `matrix`/`dpi`/`colorspace` is the single rasterization entry; `Pixmap.save`/`tobytes` emit bytes the image/PDF owner consumes.
- extraction axis: `Page.get_text(flags=TEXTFLAGS_*)` with its mode row (plain/dict/json/words/html), `search_for`, and `find_tables` (the `pymupdf.table.TableFinder`) are the extraction surface; results feed the document owner, never a re-minted text model. Build a `TextPage` once via `get_textpage(flags=)` and replay `extractDICT`/`extractWORDS`/`search` against it instead of re-parsing per mode.
- table-stack axis: `find_tables().tables[i].to_pandas()`/`to_markdown()` is the native bridge into the dataframe/markdown owners — pymupdf's ruled-line `TableFinder` is the bulk path, `pdfplumber` owns the higher-recall `lines`/`text`/`explicit` strategy arm; never re-cluster cells by hand when `to_pandas` already shapes the grid.
- ocr axis: `Page.get_textpage_ocr(language=..., dpi=..., full=...)` grafts a Tesseract text layer onto a scanned page, returning a `TextPage` whose `extractWORDS`/`get_text` feed the same extraction owner; whole-document OCR-to-PDF/A stays with `ocrmypdf`, never a hand-stitched per-page graft.
- outline axis: `Document.get_toc(simple=...)`/`set_toc`/`set_toc_item`/`del_toc_item` is the native outline edit surface; `Document.outline` exposes the linked `Outline` tree for recursive walks; recovery reads `get_toc` level/title/page rows rather than re-deriving from links, and `pypdf.PdfReader.outline` is the pure-Python BSD alternative.
- embedded axis: `Document.embfile_names`/`embfile_get`/`embfile_info`/`embfile_count` recover the native attached-file table directly; `get_images` enumerates placed rasters, a distinct concern, never the embedded-file recovery path. `Annot.get_file`/`update_file` own per-annotation file-attachment payloads.
- annotation axis: `Page.annots` iterates existing annotations and `Annot.info`/`Annot.rect`/`Annot.type` recover content/geometry/kind (`PDF_ANNOT_*`); the `Page.add_*_annot` family + `Annot.update()` author and bake appearance; `Page.widgets` plus the `Widget.field_name`/`field_value`/`field_type` instance accessors recover form fields — each a native iterator/accessor, never an object-tree walk re-deriving the annotation table.
- drawing axis: `Page.new_shape()` -> `Shape.draw_*` + `finish()` + `commit()` is the accumulating vector-canvas entry; the `Page.draw_*` thin wrappers are one-shot; `get_drawings(extended=)` recovers the existing path list. `Page.insert_htmlbox`/`write_text` (over a `TextWriter`) own styled-HTML and positioned-glyph authoring respectively — never hand-emit content-stream operators.
- image-assembly axis: the `not Document.is_pdf` flag gates the single-image intake (an image input opens as a one-page non-PDF document), `new_page(width=, height=)` and `insert_image(rect, stream=)` wrap a raster stream into a fresh single-page PDF, and `tobytes` serializes it for the OCR feed; a separate raster-to-PDF library is the deleted form.
- sanitize axis: `Document.scrub` is the redaction-grade remover of hidden text, attachments, javascript, and metadata; `bake(annots=, widgets=)` flattens interactive layers into content; `subset_fonts` shrinks embedded fonts; `rewrite_images` recompresses/downsamples embedded rasters — each a document-level call row, never a hand-rolled object walker.
- image axis: `Page.insert_image(stream=...)`/`insert_image(filename=...)`/`insert_image(pixmap=...)` is the single lossless raster-embed entry, embedding the byte stream without recompression; `Pixmap.tobytes(output=)` covers native encoders, `Pixmap.pil_tobytes(format=)` stacks Pillow's WEBP/AVIF/TIFF encoders for formats MuPDF lacks; a separate raster-to-PDF library is the deleted form.
- save axis: `save`/`ez_save` with `incremental`/`garbage`/`deflate`/`linear`/`use_objstms` knobs is the single emission surface; `encryption=PDF_ENCRYPT_AES_256` + `permissions=PDF_PERM_*` ride the save call, never a parallel encryptor. The `journal_enable`/`journal_start_op`/`journal_undo`/`save_snapshot` rail is the native transactional editing modality.
- evidence: each operation captures page count, render dpi/matrix, color space, OCR language/dpi, redaction count, table cell-grid shape (`row_count`x`col_count`), embedded-file count, and output byte length as a pdf receipt.
- license boundary: MuPDF is AGPL-3.0 — any closed-source distributed/network use of pymupdf triggers source disclosure; route the network-service render/extract path through BSD `pypdfium2` (PDFium render/text) and `pypdf` (structural editing), reserve pymupdf for permissively-licensed/internal pipelines or an Artifex commercial seat. This is an architecture constraint, not a coding caveat.
- boundary: pymupdf owns native render/extract/ocr/redact/scrub/draw/table/embed; pure-Python structural assembly routes to BSD `pypdf`; high-fidelity BSD render routes to `pypdfium2`; AES-256-R6 + content-stream tokens route to `pikepdf`; ruled-table high-recall extraction routes to `pdfplumber`; whole-document OCR-to-PDF/A routes to `ocrmypdf`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pymupdf`
- Owns: native document open, page rasterization, per-page OCR, text/image/table extraction (`TEXTFLAGS_*`, `to_pandas`/`to_markdown`), native outline (`get_toc`/`set_toc`/`Outline`) and embedded-file recovery, vector drawing (`Shape`/`get_drawings`), positioned-glyph + HTML authoring (`TextWriter`/`insert_htmlbox`), the annotation-authoring family + redaction, redaction-grade `scrub`/`bake`/`subset_fonts`/`rewrite_images`, lossless `insert_image` raster embed, reflowable `Story` layout (`Story.write`), page assembly/reorder, OCG layers, journalled undo/redo, AES-256/RC4 encrypted incremental save
- Accept: render, OCR, table (`to_pandas`), outline, drawing, and embedded-file recovery feeding the document/PDF, table/dataframe, and image owners; `pil_tobytes` feeding Pillow encoders; lossless raster embed feeding the image-to-PDF intake
- Reject: wrapper-renames of `get_pixmap`/`get_text`/`find_tables`/`get_toc`/`embfile_get`; a second rasterizer where `pypdfium2` already covers a BSD render path; an embedded-file recovery re-derived from `get_images`; a hand-clustered table grid where `to_pandas` shapes it; a whole-document OCR-to-PDF/A pipeline where `ocrmypdf` owns it; AES-256-R6 where `pikepdf` owns it; a separate raster-to-PDF library where `insert_image` embeds losslessly; the AGPL render path inside a distributed closed service (route to BSD siblings); identity minting the runtime owns
