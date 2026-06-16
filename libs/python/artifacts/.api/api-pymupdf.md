# [PY_ARTIFACTS_API_PYMUPDF]

`pymupdf` supplies the native MuPDF-backed PDF and document surface for the artifacts pdf rail: a document root, a page unit, a rasterizing pixmap, and a text/geometry extraction surface that drive rendering, text/image extraction, redaction, annotation, and page-level editing across PDF and the MuPDF document family. The package owner composes `Document`, `Page`, `Pixmap`, and `TextPage` into the pdf owner; it never re-implements the MuPDF rendering pipeline the native core already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pymupdf`
- package: `pymupdf`
- import: `pymupdf` (legacy alias `fitz`)
- owner: `artifacts`
- rail: pdf
- installed: `1.27.2.3` reflected via `python -c "import pymupdf"` on cp315
- entry points: none (library only)
- capability: PDF/XPS/EPUB/CBZ/image document open, page rasterization, text and image extraction, drawing/annotation/redaction authoring, page assembly, OCG layers, embedded files, incremental save

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, page, and rendering roots
- rail: pdf

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `Document` | document root | open/save a document; page access, metadata, TOC, layers, embedded files, conversion |
| [2] | `Page` | page unit | render, extract, draw, annotate, redact, geometry boxes |
| [3] | `Pixmap` | raster buffer | rendered RGBA/CMYK bitmap with save/encode and color ops |
| [4] | `TextPage` | text model | structured text/word/dict extraction from a page |
| [5] | `DocumentWriter` | author sink | build a new document from drawn pages |
| [6] | `Annot` | annotation | a single page annotation with update/appearance control |
| [7] | `Font` | font handle | font for inserted text and glyph metrics |
| [8] | `Story` | flow layout | reflowable HTML-to-PDF story layout |

[PUBLIC_TYPE_SCOPE]: geometry and color value objects
- rail: pdf

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `Rect` / `IRect` | rectangle | float/integer page rectangle algebra |
| [2] | `Point` | point | 2D point |
| [3] | `Matrix` | affine matrix | transform for render scale/rotate |
| [4] | `Quad` | quadrilateral | rotated text/region bounds |
| [5] | `Colorspace` | color space | CS_RGB/CS_GRAY/CS_CMYK pixmap color space |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: pdf

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `FileDataError` | data fault | corrupt or invalid document data |
| [2] | `FileNotFoundError` | resolution fault | document path absent |
| [3] | `EmptyFileError` | empty fault | zero-length document |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document open and save
- rail: pdf

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `open` | `open(filename=None, stream=None, filetype=None, rect=None, width=0, height=0, fontsize=11) -> Document` | open from path or in-memory stream |
| [2] | `Document` | `Document(filename=None, stream=None, filetype=None, rect=None, width=0, height=0, fontsize=11)` | document constructor (alias of `open`) |
| [3] | `Document.save` | `save(filename, garbage=0, clean=0, deflate=0, deflate_images=0, deflate_fonts=0, incremental=0, ascii=0, expand=0, linear=0, pretty=0, encryption=1, permissions=-1, owner_pw=None, user_pw=None) -> None` | save with compaction/encryption knobs |
| [4] | `Document.ez_save` | `ez_save(filename, **kwargs) -> None` | save with deflate/garbage defaults on |
| [5] | `Document.convert_to_pdf` | `convert_to_pdf(from_page=0, to_page=-1, rotate=0) -> bytes` | convert a non-PDF document to PDF bytes |
| [6] | `Document.authenticate` | `authenticate(password) -> int` | unlock an encrypted document |
| [7] | `Document.insert_pdf` | `insert_pdf(docsrc, from_page=-1, to_page=-1, start_at=-1, rotate=-1, links=True, annots=True, show_progress=0, final=1) -> None` | splice pages from another document |

[ENTRYPOINT_SCOPE]: render and extraction
- rail: pdf

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `Page.get_pixmap` | `get_pixmap(*, matrix=Identity, dpi=None, colorspace=csRGB, clip=None, alpha=False, annots=True) -> Pixmap` | rasterize a page |
| [2] | `Page.get_text` | `get_text(option='text', *, clip=None, flags=None, textpage=None, sort=False, delimiters=None) -> str | list | dict` | extract text in plain/dict/json/words/html modes |
| [3] | `Page.get_textpage` | `get_textpage(clip=None, flags=3) -> TextPage` | build a reusable text model |
| [4] | `Page.search_for` | `search_for(needle, *, clip=None, quads=False, flags=..., textpage=None) -> list[Rect | Quad]` | locate text regions |
| [5] | `Page.get_images` | `get_images(full=False) -> list[tuple]` | enumerate embedded images |
| [6] | `Page.apply_redactions` | `apply_redactions(images=2, graphics=1, text=0) -> bool` | burn in redaction annotations |
| [7] | `Pixmap.save` | `save(filename, output=None, jpg_quality=95) -> None` | encode raster to PNG/JPEG/etc. |
| [8] | `Pixmap.tobytes` | `tobytes(output='png', jpg_quality=95) -> bytes` | encode raster to bytes |

## [4]-[IMPLEMENTATION_LAW]

[PDF_NATIVE]:
- import: `import pymupdf` at boundary scope only; the `fitz` alias is not used.
- document axis: one `Document` owns every supported format (PDF/XPS/EPUB/CBZ/image); `filetype` is a row, never a per-format type.
- render axis: `Page.get_pixmap` with `matrix`/`dpi`/`colorspace` is the single rasterization entry; `Pixmap.save`/`tobytes` emit bytes the image/PDF owner consumes.
- extraction axis: `Page.get_text` with its mode row (plain/dict/json/words/html) and `search_for` are the extraction surface; results feed the document owner, never a re-minted text model.
- save axis: `save`/`ez_save` with `incremental`/`garbage`/`deflate` knobs is the single emission surface; encryption rides the save call, never a parallel encryptor.
- evidence: each operation captures page count, render dpi/matrix, color space, redaction count, and output byte length as a pdf receipt.
- boundary: pymupdf owns native render/extract/redact; pure-Python structural assembly routes to `pypdf`; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pymupdf`
- Owns: native document open, page rasterization, text/image extraction, drawing/annotation/redaction authoring, page assembly, layers, embedded files, incremental save
- Accept: render and extraction feeding the document/PDF and image owners
- Reject: wrapper-renames of `get_pixmap`/`get_text`; a second rasterizer where `pypdfium2` already covers a render path; identity minting the runtime owns
