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
- capability: PDF/XPS/EPUB/CBZ/image document open, page rasterization, text and image extraction, drawing/annotation/redaction authoring, page assembly, OCG layers, embedded files, incremental save

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

[ENTRYPOINT_SCOPE]: render and extraction
- rail: pdf

Render and extraction rows share matrix/dpi, color, clip, text mode, textpage, search, image, redaction, and raster-output policy.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                     | [CAPABILITY]                                     |
| :-----: | :---------------------- | :------------------------------- | :----------------------------------------------- |
|  [01]   | `Page.get_pixmap`       | rasterization policy -> `Pixmap` | rasterize a page                                 |
|  [02]   | `Page.get_text`         | text mode plus clip/sort policy  | extract text in plain/dict/json/words/html modes |
|  [03]   | `Page.get_textpage`     | clip plus flags                  | build a reusable text model                      |
|  [04]   | `Page.search_for`       | needle plus match policy         | locate text regions                              |
|  [05]   | `Page.get_images`       | full-image flag                  | enumerate embedded images                        |
|  [06]   | `Page.apply_redactions` | image/graphics/text policy       | burn in redaction annotations                    |
|  [07]   | `Pixmap.save`           | target plus output policy        | encode raster to PNG/JPEG/etc.                   |
|  [08]   | `Pixmap.tobytes`        | output format plus quality       | encode raster to bytes                           |

## [04]-[IMPLEMENTATION_LAW]

[PDF_NATIVE]:
- import: `import pymupdf` at boundary scope only; the `fitz` alias is not used.
- document axis: one `Document` owns every supported format (PDF/XPS/EPUB/CBZ/image); `filetype` is a row, never a per-format type.
- render axis: `Page.get_pixmap` with `matrix`/`dpi`/`colorspace` is the single rasterization entry; `Pixmap.save`/`tobytes` emit bytes the image/PDF owner consumes.
- extraction axis: `Page.get_text` with its mode row (plain/dict/json/words/html) and `search_for` are the extraction surface; results feed the document owner, never a re-minted text model.
- save axis: `save`/`ez_save` with `incremental`/`garbage`/`deflate` knobs is the single emission surface; encryption rides the save call, never a parallel encryptor.
- evidence: each operation captures page count, render dpi/matrix, color space, redaction count, and output byte length as a pdf receipt.
- boundary: pymupdf owns native render/extract/redact; pure-Python structural assembly routes to `pypdf`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pymupdf`
- Owns: native document open, page rasterization, text/image extraction, drawing/annotation/redaction authoring, page assembly, layers, embedded files, incremental save
- Accept: render and extraction feeding the document/PDF and image owners
- Reject: wrapper-renames of `get_pixmap`/`get_text`; a second rasterizer where `pypdfium2` already covers a render path; identity minting the runtime owns
