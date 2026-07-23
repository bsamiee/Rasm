# [PY_ARTIFACTS_API_PYMUPDF]

`pymupdf` owns the native MuPDF-backed document surface for the artifacts pdf rail: one `Document` root spanning the PDF/XPS/EPUB/CBZ/image family drives render, text/image/table extraction, per-page OCR, redaction, annotation, vector drawing, and page assembly, all through the bundled MuPDF C core. Whole-program AGPL copyleft binds that path to internal or permissively-licensed deployments and routes a distributed closed service to the BSD render and structural siblings on the same rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pymupdf`
- package: `pymupdf` (`AGPL-3.0-or-later OR Artifex-Commercial`, Artifex; dist `PyMuPDF`)
- module: `pymupdf`; bundles the native MuPDF C core, no system library
- rail: pdf — native render/extract/ocr/redact/scrub/draw/table/embed/author owner

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, page, and rendering roots

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                                   |
| :-----: | :------------------ | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `Document`          | document root | open/save; page access, metadata, TOC, layers, embedded files, conversion      |
|  [02]   | `Page`              | page unit     | render, extract, draw, annotate, redact, geometry boxes                        |
|  [03]   | `Pixmap`            | raster buffer | rendered RGBA/CMYK bitmap with save/encode and color ops                       |
|  [04]   | `TextPage`          | text model    | structured text/word/dict extraction from a page                               |
|  [05]   | `DocumentWriter`    | author sink   | build a new document from drawn `Story`/`Shape` pages                          |
|  [06]   | `Annot`             | annotation    | a single page annotation with update/appearance/file/OC control                |
|  [07]   | `Font`              | font handle   | inserted-text font, glyph metrics, `has_glyph`/`valid_codepoints` probes       |
|  [08]   | `Story`             | flow layout   | reflowable HTML-to-PDF story (`Story.write` one-shot or `place`/`draw` loop)   |
|  [09]   | `Shape`             | vector canvas | low-level draw canvas (`new_shape()`): line/rect/circle/bezier + finish/commit |
|  [10]   | `TextWriter`        | glyph writer  | font-bound positioned-glyph accumulator, committed via `Page.write_text`       |
|  [11]   | `Widget`            | form widget   | interactive form-field widget for `add_widget`                                 |
|  [12]   | `Link`              | link object   | a single page link with `LINK_*` kind, URI/dest, and rect                      |
|  [13]   | `Outline`           | outline node  | a single TOC node (`title`/`dest`/`next`/`down`) for tree walks                |
|  [14]   | `table.TableFinder` | table engine  | resolved-table container from `find_tables` (submodule `pymupdf.table`)        |
|  [15]   | `table.Table`       | cell grid     | one resolved table with `extract`/`to_pandas`/`to_markdown`/`bbox`/`rows`      |

[PUBLIC_TYPE_SCOPE]: geometry and color value objects

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :--------------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `Rect` / `IRect` | rectangle     | float/integer page rectangle algebra                                         |
|  [02]   | `Point`          | point         | 2D point                                                                     |
|  [03]   | `Matrix`         | affine matrix | transform for render scale/rotate                                            |
|  [04]   | `Quad`           | quadrilateral | rotated text/region bounds                                                   |
|  [05]   | `Colorspace`     | color space   | `csRGB`/`csGRAY`/`csCMYK` singletons; `CS_RGB`/`CS_GRAY`/`CS_CMYK` int codes |

[PUBLIC_TYPE_SCOPE]: module-level discriminant constants

Call rows discriminate on module-level `int`/flag constants, never re-minted local enums; `TEXTFLAGS_*` and `PDF_REDACT_*` families combine by `|`.

- [01]-[TEXTFLAGS]: `get_text`/`get_textpage` flag composition — `TEXT`/`WORDS`/`BLOCKS`/`DICT`/`RAWDICT`/`HTML`/`XHTML`/`XML`/`SEARCH` presets + `TEXT_PRESERVE_*`/`TEXT_DEHYPHENATE` bits.
- [02]-[TEXT_OUTPUT]: `get_text` output kind — `TEXT`/`HTML`/`JSON`/`XHTML`/`XML`.
- [03]-[PDF_REDACT]: `apply_redactions(images=, graphics=, text=)` policy — image `NONE`(0)/`REMOVE`(1)/`PIXELS`(2)/`REMOVE_UNLESS_INVISIBLE`; line-art `NONE`(0)/`REMOVE_IF_TOUCHED`(1)/`REMOVE_IF_COVERED`; text `REMOVE`(0)/`NONE`(1)/`REMOVE_INVISIBLE` — text order is INVERTED vs image, `0` removes.
- [04]-[PDF_ENCRYPT]: `save(encryption=)` strength — `KEEP`/`NONE`/`RC4_40`/`RC4_128`/`AES_128`/`AES_256`.
- [05]-[PDF_PERM]: `save(permissions=)` bitmask — `PRINT`/`MODIFY`/`COPY`/`ANNOTATE`/`FORM`/`ACCESSIBILITY`/`ASSEMBLE`/`PRINT_HQ`.
- [06]-[PDF_WIDGET_TYPE]: `Widget.field_type` discriminant — `UNKNOWN`/`BUTTON`/`CHECKBOX`/`RADIOBUTTON`/`TEXT`/`LISTBOX`/`COMBOBOX`/`SIGNATURE`.
- [07]-[PDF_ANNOT]: `Annot.type` discriminant / `set_flags` — `TEXT`/`FREE_TEXT`/`LINE`/`SQUARE`/`HIGHLIGHT`/`INK`/`STAMP`/`REDACT`/`WIDGET`/… kind codes + `PDF_ANNOT_IS_*` flag bits.
- [08]-[STAMP]: `add_stamp_annot(stamp=)` — `Approved`/`Confidential`/`Draft`/`Final`/`TopSecret`/… preset stamps.

[PUBLIC_TYPE_SCOPE]: fault family

`EmptyFileError` subclasses `FileDataError`; both derive from `RuntimeError`. pymupdf rebinds the builtin name `FileNotFoundError` to its own resolution fault.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]    | [CAPABILITY]                                       |
| :-----: | :------------------ | :--------------- | :------------------------------------------------- |
|  [01]   | `FileDataError`     | data fault       | corrupt or invalid document data (`RuntimeError`)  |
|  [02]   | `FileNotFoundError` | resolution fault | document path absent                               |
|  [03]   | `EmptyFileError`    | empty fault      | zero-length document (subclass of `FileDataError`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document open and save
- `save`/`ez_save` carry: `garbage, clean, deflate, deflate_fonts, incremental, linear, use_objstms, encryption, permissions, owner_pw, user_pw, preserve_metadata, compression_effort`

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `open(filename, stream, filetype)`                  | static   | open from path or in-memory stream (`Document` alias) |
|  [02]   | `Document(filename, stream, filetype, *, width)`    | ctor     | constructor; `pymupdf.open` is the module alias       |
|  [03]   | `Document.save(filename, *, ...)`                   | instance | compaction, linearization, and encryption knobs       |
|  [04]   | `Document.ez_save(filename)`                        | instance | deflate/garbage/use-objstms defaults on               |
|  [05]   | `Document.convert_to_pdf(*, from_page, to_page)`    | instance | convert a non-PDF document to PDF bytes               |
|  [06]   | `Document.authenticate(password)`                   | instance | unlock an encrypted document                          |
|  [07]   | `Document.insert_pdf(docsrc, *, from_page, rotate)` | instance | splice pages from another `Document`                  |
|  [08]   | `Document.insert_file(src, *, ...)`                 | instance | splice pages from any supported file/stream           |
|  [09]   | `Document.select(list[int])`                        | instance | reorder/subset to a page-index list in place          |
|  [10]   | `Document.delete_pages(*, from_page, to_page)`      | instance | drop pages (`delete_page` for one)                    |
|  [11]   | `Document.copy_page / move_page / fullcopy_page`    | instance | duplicate or relocate a page internally               |
|  [12]   | `Document.close()` / context manager                | instance | deterministic native-handle release                   |
|  [13]   | `Document.set_metadata(dict)`                       | instance | write info dict (title/author/subject/…); `{}` clears |
|  [14]   | `Document.set_xml_metadata(str)`                    | instance | write archival PDF/A XMP metadata                     |

- `Document` context manager (`with pymupdf.open(...) as doc`): `__enter__` returns the `Document`, `__exit__` closes the native handle at scope exit rather than GC-reaping it.

[ENTRYPOINT_SCOPE]: outline and embedded files

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :-------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `Document.get_toc(simple)`              | instance | recover the nested outline as level/title/page rows |
|  [02]   | `Document.set_toc(toc, collapse)`       | instance | author/replace the outline from level rows          |
|  [03]   | `Document.embfile_names()`              | instance | list embedded-file entry names                      |
|  [04]   | `Document.embfile_get(item)`            | instance | read one embedded file's bytes by name/index        |
|  [05]   | `Document.embfile_info(item)`           | instance | embedded-file metadata (filename, desc, size)       |
|  [06]   | `Document.embfile_count()`              | instance | embedded-file entry count                           |
|  [07]   | `Document.embfile_add(name, buffer, *)` | instance | attach a new embedded file                          |
|  [08]   | `Document.embfile_upd(item, *, buffer)` | instance | replace an embedded file's bytes/metadata           |
|  [09]   | `Document.embfile_del(item)`            | instance | drop an embedded file                               |
|  [10]   | `Document.outline`                      | property | linked-node outline tree for recursive walks        |
|  [11]   | `Document.set_toc_item / del_toc_item`  | instance | edit/drop a single outline entry in place           |

[ENTRYPOINT_SCOPE]: redaction-grade scrub, bake, and font subset

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :------------------------------------ | :------- | :---------------------------------------------------------- |
|  [01]   | `Document.scrub(*, hidden_text, ...)` | instance | redaction-grade removal of hidden/attached/metadata content |
|  [02]   | `Document.bake(*, annots, widgets)`   | instance | flatten annotations and form widgets into content           |
|  [03]   | `Document.subset_fonts(*, fallback)`  | instance | subset embedded fonts in place, returning byte delta        |

[ENTRYPOINT_SCOPE]: annotation recovery and image-document assembly

| [INDEX] | [SURFACE]                      | [SHAPE]  | [CAPABILITY]                                                       |
| :-----: | :----------------------------- | :------- | :----------------------------------------------------------------- |
|  [01]   | `Page.annots(types)`           | instance | iterate existing annotations on the page                           |
|  [02]   | `Annot.info`                   | property | annotation metadata dictionary                                     |
|  [03]   | `Annot.rect`                   | property | annotation bounding rectangle                                      |
|  [04]   | `Annot.type`                   | property | annotation type number and name (`PDF_ANNOT_*`)                    |
|  [05]   | `Annot.get_file / update_file` | instance | read/replace a `FileAttachment` annotation's payload               |
|  [06]   | `Page.rect`                    | property | the page rectangle (media/crop box)                                |
|  [07]   | `Widget.field_name`            | property | the form field's name                                              |
|  [08]   | `Widget.field_value`           | property | the form field's current value                                     |
|  [09]   | `Widget.field_type`            | property | the form field's `PDF_WIDGET_TYPE_*` int discriminant              |
|  [10]   | `Document.is_pdf`              | property | opened-format discriminant; `not is_pdf` gates single-image intake |
|  [11]   | `Document.new_page(*, width)`  | instance | append a blank page and return it                                  |
|  [12]   | `Document.tobytes(*, garbage)` | instance | serialize the in-memory document to PDF bytes                      |

- `Document.is_pdf` siblings `is_reflowable`/`is_stream`/`is_form_pdf` discriminate the opened format; a single-image input opens as a one-page non-PDF document, so `new_page`/`insert_image`/`tobytes` wrap a raster stream into a fresh single-page PDF for the OCR-intake path.

[ENTRYPOINT_SCOPE]: render and extraction

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                                 |
| :-----: | :---------------------------------------------- | :------- | :----------------------------------------------------------- |
|  [01]   | `Page.get_pixmap(*, matrix, dpi, colorspace)`   | instance | rasterize a page to a `Pixmap`                               |
|  [02]   | `Page.get_text(option, *, clip, sort, flags)`   | instance | extract text in plain/dict/json/words/html modes             |
|  [03]   | `Page.get_textpage(*, clip, flags)`             | instance | build a reusable text model                                  |
|  [04]   | `Page.get_textpage_ocr(*, language, dpi, full)` | instance | Tesseract-OCR a page into a searchable `TextPage`            |
|  [05]   | `Page.search_for(needle, *, clip, quads)`       | instance | locate text regions                                          |
|  [06]   | `Page.get_images(full)`                         | instance | enumerate embedded images                                    |
|  [07]   | `Page.apply_redactions(*, images, graphics)`    | instance | burn in redactions with module `PDF_REDACT_*` int flags      |
|  [08]   | `Page.insert_image(rect, *, stream, pixmap)`    | instance | embed a raster losslessly from stream/file/pixmap            |
|  [09]   | `Pixmap(*args)`                                 | ctor     | construct a raster buffer from a doc xref or colorspace spec |
|  [10]   | `Pixmap.save(filename)`                         | instance | encode raster to PNG/JPEG/etc.                               |
|  [11]   | `Pixmap.tobytes(output, jpg_quality)`           | instance | encode PNG/PPM/PNM/PGM/PBM/PAM/PS/JPEG bytes (CMYK: JPEG)    |
|  [12]   | `Pixmap.pil_tobytes(format, **kw)`              | instance | bridge to Pillow's WEBP/AVIF/TIFF encoders via `Image.save`  |
|  [13]   | `Pixmap` color ops                              | instance | in-place color/geometry ops + dedup `digest` (`samples` mv)  |
|  [14]   | `Document.rewrite_images(*, dpi_threshold)`     | instance | recompress/downsample embedded images in place               |
|  [15]   | `table.TableFinder.tables`                      | property | resolved tables from `find_tables`                           |
|  [16]   | `table.Table.extract()`                         | instance | row-major cell text of one resolved table                    |
|  [17]   | `table.Table.to_pandas()`                       | instance | resolved `DataFrame` feeding the table/dataframe owner       |
|  [18]   | `table.Table.to_markdown(clean)`                | instance | resolved table as a GitHub-flavoured Markdown grid           |
|  [19]   | `table.Table` shape                             | property | resolved-table geometry (`bbox`/`row_count`/`rows`/`header`) |

- `Pixmap` color ops are `set_alpha`/`invert_irect`/`gamma_with`/`tint_with`/`shrink`/`warp`/`color_count`/`digest`.
- `Pixmap.tobytes`: `"psd"` faults `FzErrorArgument: cannot seek in buffer` on the in-memory path — PSD writes only through the seekable-file `Pixmap.save`.

[ENTRYPOINT_SCOPE]: vector drawing, glyph authoring, and annotation/redaction authoring

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :---------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `Page.new_shape()`                              | factory  | accumulate vector geometry then burn it onto the page   |
|  [02]   | `Page.draw_line / draw_rect / draw_circle / …`  | instance | one-shot vector primitive (auto-`Shape` + `commit`)     |
|  [03]   | `Page.get_drawings(extended)`                   | instance | recover page vector paths (fills/strokes/clips)         |
|  [04]   | `Page.insert_htmlbox(rect, text, *, css, oc)`   | instance | flow styled HTML/CSS into a rect; return fill spillover |
|  [05]   | `Page.write_text(*, rect, writers)`             | instance | commit positioned-glyph text built by a `TextWriter`    |
|  [06]   | `Page.insert_text / insert_textbox(pos, text)`  | instance | simple single-line or wrapped-box text insert           |
|  [07]   | `Page.add_redact_annot(quad, *, text, fill)`    | instance | mark a region before `apply_redactions`                 |
|  [08]   | `Page.add_*_annot(...)`                         | instance | author every kind; `Annot.update()` bakes appearance    |
|  [09]   | `Page.find_tables(*, vertical_strategy, …)`     | instance | resolve ruled/text tables natively into a `TableFinder` |
|  [10]   | `Page.widgets(types)` / `add_widget(widget)`    | instance | iterate/author interactive form-field widgets           |
|  [11]   | `Page.get_links / insert_link / delete_link`    | instance | recover/author page `LINK_*` objects                    |
|  [12]   | `Page.show_pdf_page(rect, docsrc, *, clip, oc)` | instance | vector-copy a clipped source PDF region into the page   |

- `Page.show_pdf_page`: `clip` selects the source region, `rect` receives it, `oc` binds an OCG/OCMD xref; the native figure-placement/imposition primitive never rasterizes before embedding.
- `TextWriter.append(pos, text, *, font, fontsize)` accumulates glyphs a `Page.write_text` then commits.

[ENTRYPOINT_SCOPE]: OCG layers and journalled editing

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :--------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `Document.add_ocg(name, *, on, intent)`  | instance | create an optional-content group; return its xref |
|  [02]   | `Document.set_ocmd(*, ocgs, policy, ve)` | instance | mint an OCMD over member OCG xrefs                |
|  [03]   | `Document.set_oc / get_oc / get_ocmd`    | instance | bind/read an object's optional-content membership |
|  [04]   | `Document.layer_ui_configs / set_layer`  | instance | enumerate/toggle layer UI configurations          |
|  [05]   | `Document.journal_enable()`              | instance | open journal and bracket an operation             |
|  [06]   | `Document.journal_undo / journal_redo`   | instance | step the journal backward/forward                 |
|  [07]   | `Document.save_snapshot(filename)`       | instance | write an incremental snapshot tied to the journal |

- `Document.set_ocmd`: `policy` is `AnyOn`/`AllOn`/`AnyOff`/`AllOff` and `ve` a visibility expression; OCMD expresses the nested-layer/radio-group hierarchy `add_ocg` alone cannot. `get_ocmd(xref)` recovers a minted OCMD.
- `Document.set_layer`: `config=-1` addresses the default OC configuration, `0..n` an alternate `/Configs` entry; `set_layer(0, ...)` with no alternate configs raises `ValueError: bad config number`.

[ENTRYPOINT_SCOPE]: page-box geometry

Five PDF page boxes: MediaBox (physical sheet), CropBox (visible region), TrimBox (finished trim), BleedBox (bleed extent), ArtBox (content). Boxes are inherited and clamped — CropBox clamps to MediaBox.

| [INDEX] | [SURFACE]             | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :-------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `Page` box read attrs | property | read five unrotated boxes in PDF coordinates        |
|  [02]   | `Page.set_mediabox`   | instance | set physical sheet; resets other boxes — call FIRST |
|  [03]   | `Page.set_cropbox`    | instance | set visible region clamped to MediaBox              |
|  [04]   | `Page.set_trimbox`    | instance | set finished-trim cut line inside bleed             |
|  [05]   | `Page.set_bleedbox`   | instance | set printed-past-trim extent the press fold reads   |
|  [06]   | `Page.set_artbox`     | instance | set the meaningful-content box                      |

- `Page` box read attrs are `mediabox`/`cropbox`/`trimbox`/`bleedbox`/`artbox -> Rect`; `mediabox_size`/`cropbox_position -> Point` give sheet size and crop origin.

[ENTRYPOINT_SCOPE]: reflowable story layout

`Story.write` is the one-shot reflow entry — a `Story` and a `rectfn` callback drive a full `DocumentWriter` page sequence in one call; `place`/`draw` is the manual slice modality and `write_stabilized`/`add_pdf_links` add stabilized-HTML and live-link variants.

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :------------------------------------ | :------- | :---------------------------------------------- |
|  [01]   | `Story(*, html, user_css, em)`        | ctor     | build a reflowable HTML story                   |
|  [02]   | `Story.write(writer, rectfn, *)`      | instance | one-shot layout across writer pages             |
|  [03]   | `Story.write_stabilized(writer, ...)` | instance | regenerate HTML and iterate until stable        |
|  [04]   | `Story.place(where, flags) / draw`    | instance | manual slice-by-slice layout + render           |
|  [05]   | `Story.add_pdf_links(doc, positions)` | instance | inject live links from placed element positions |
|  [06]   | `DocumentWriter(path, options)`       | ctor     | drawn-page sink for path or `BytesIO`           |
|  [07]   | `DocumentWriter.begin_page(mediabox)` | instance | open a page and return its draw device          |
|  [08]   | `DocumentWriter.end_page()`           | instance | close the current page                          |
|  [09]   | `DocumentWriter.close()`              | instance | finalize the written document                   |
|  [10]   | `paper_rect(str)`                     | static   | named paper-size rect (e.g. `"a4"`)             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `Document` owns every supported format, `filetype` a row and never a per-format type; `Page.get_pixmap(matrix/dpi/colorspace)` is the single render entry and `Page.get_text(flags=TEXTFLAGS_*)`/`find_tables` the single extraction surface, both replayed over one `get_textpage(flags=)` (`extractDICT`/`extractWORDS`/`search`) rather than re-parsed per mode.
- Each operation captures page count, render dpi/matrix, colorspace, OCR language/dpi, redaction count, table grid shape (`row_count`×`col_count`), embedded-file count, and output byte length as one pdf receipt.

[STACKING]:
- `pypdf`(`.api/pypdf.md`): pure-Python BSD structural editing (merge/split/prune/AcroForm) for a distributed closed service where AGPL bars pymupdf, meeting at PDF bytes / `tobytes`.
- `pypdfium2`(`.api/pypdfium2.md`): BSD high-fidelity PDFium render/text for the network-service render path, meeting at PDF bytes and the `Pixmap`.
- `pikepdf`(`.api/pikepdf.md`): AES-256-R6 encryption and content-stream token editing beyond the `save(encryption=)` strengths, meeting at PDF bytes.
- `pdfplumber`(`.api/pdfplumber.md`): higher-recall `lines`/`text`/`explicit` table strategy where the ruled-line `find_tables` path misses, meeting at the page.
- `ocrmypdf`(`.api/ocrmypdf.md`): whole-document OCR-to-PDF/A against the per-page `get_textpage_ocr` graft, meeting at PDF bytes.
- `pillow`(`.api/pillow.md`): `Pixmap.pil_tobytes` bridges Pillow's WEBP/AVIF/TIFF encoders MuPDF lacks, off the `Pixmap.samples` buffer.
- within-lib: `find_tables().tables[i].to_pandas()`/`to_markdown()` feeds the dataframe/markdown owner directly; `not is_pdf` + `new_page`/`insert_image`/`tobytes` wraps a raster into a one-page PDF for OCR intake; `scrub`/`bake`/`subset_fonts`/`rewrite_images` chain the document-level sanitize path.

[LOCAL_ADMISSION]:
- `import pymupdf` at boundary scope only.
- Native calls own render, extract, OCR, redact, scrub, draw, table, and embed; a hand-rolled object walker, a second rasterizer, a re-clustered table grid, or a raster-to-PDF library beside `insert_image` is the deleted form.
- AGPL routing is the rail's architecture constraint: reserve pymupdf for internal or permissively-licensed pipelines or an Artifex commercial seat, and route a distributed closed service's render/extract path to BSD `pypdfium2` and `pypdf`.

[RAIL_LAW]:
- Package: `pymupdf`
- Owns: native document open, page rasterization, per-page OCR, text/image/table extraction (`TEXTFLAGS_*`, `to_pandas`/`to_markdown`), native outline (`get_toc`/`set_toc`/`Outline`) and embedded-file recovery, the five page boxes (read + `set_*` write), vector drawing (`Shape`/`get_drawings`), PDF-page vector placement (`show_pdf_page`), positioned-glyph + HTML authoring (`TextWriter`/`insert_htmlbox`), the annotation-authoring family + redaction, `scrub`/`bake`/`subset_fonts`/`rewrite_images`, lossless `insert_image` embed, reflowable `Story` layout, page assembly/reorder, OCG/OCMD layers, info-dict + XMP metadata authoring, journalled undo/redo, AES-256/RC4 encrypted incremental save
- Accept: render, OCR, table (`to_pandas`), outline, drawing, and embedded-file recovery feeding the document/PDF, table/dataframe, and image owners; `pil_tobytes` feeding Pillow encoders; lossless raster embed feeding the image-to-PDF intake
- Reject: a wrapper-rename of `get_pixmap`/`get_text`/`find_tables`/`get_toc`/`embfile_get`; a second rasterizer where `pypdfium2` covers the BSD render path; an embedded-file recovery re-derived from `get_images`; a hand-clustered table grid where `to_pandas` shapes it; a whole-document OCR-to-PDF/A pipeline where `ocrmypdf` owns it; AES-256-R6 where `pikepdf` owns it; a raster-to-PDF library where `insert_image` embeds losslessly; the AGPL render path inside a distributed closed service; identity minting the runtime owns
