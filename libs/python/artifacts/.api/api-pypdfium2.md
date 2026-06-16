# [PY_ARTIFACTS_API_PYPDFIUM2]

`pypdfium2` supplies the PDFium-backed PDF render and inspection surface for the artifacts pdf rail: a document root, a page unit, a render bitmap, and a text page that drive high-fidelity rasterization, text extraction/search, page-object inspection, and form access against Google's PDFium engine. The package owner composes `PdfDocument`, `PdfPage`, `PdfBitmap`, and `PdfTextPage` into the pdf owner; it never re-implements the PDFium render pipeline the native core already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pypdfium2`
- package: `pypdfium2`
- import: `pypdfium2`
- owner: `artifacts`
- rail: pdf
- installed: `5.10.1` reflected via `python -c "import pypdfium2"` on cp315
- entry points: none (library only)
- capability: PDFium document open, page rasterization to bitmap/PIL/NumPy, text extraction and search, page-object and image inspection, bookmarks/destinations, form environment, attachments

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and page roots
- rail: pdf

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `PdfDocument` | document root | open/new/save a PDF; page access, metadata, TOC, attachments, forms |
| [2] | `PdfPage` | page unit | render, text page, object enumeration, box geometry, rotation |
| [3] | `PdfBitmap` | render buffer | rasterized bitmap with PIL/NumPy bridges and fill ops |
| [4] | `PdfTextPage` | text model | char/rect/range text extraction and search |
| [5] | `PdfTextSearcher` | search cursor | forward/backward text search state |
| [6] | `PdfObject` | page object | a single page content object (text/image/path) |
| [7] | `PdfImage` | image object | embedded image with extract/render/metadata |
| [8] | `PdfFont` | font handle | loaded font for inserted text |
| [9] | `PdfBookmark` / `PdfDest` | outline | bookmark node and its destination |
| [10] | `PdfFormEnv` | form env | interactive form environment handle |
| [11] | `PdfAttachment` | attachment | embedded-file attachment |
| [12] | `PdfMatrix` | affine matrix | transform value object for render/object placement |

[PUBLIC_TYPE_SCOPE]: faults
- rail: pdf

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `PdfiumError` | engine fault | a PDFium call returned an error code |
| [2] | `PdfiumWarning` | engine warning | a recoverable PDFium condition |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document open, build, and save
- rail: pdf

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `PdfDocument` | `PdfDocument(input, password=None, autoclose=False)` | open from path/bytes/buffer |
| [2] | `PdfDocument.new` | `new() -> PdfDocument` | classmethod: build an empty document |
| [3] | `PdfDocument.save` | `save(dest, version=None, flags=..., callback=None) -> None` | serialize to a writable buffer |
| [4] | `PdfDocument.get_page` | `get_page(index: int) -> PdfPage` | page accessor |
| [5] | `PdfDocument.import_pages` | `import_pages(pdf, pages=None, index=None) -> None` | splice pages from another document |
| [6] | `PdfDocument.get_toc` | `get_toc(max_depth=15) -> typing.Iterator[PdfBookmark]` | walk the outline |
| [7] | `PdfDocument.init_forms` | `init_forms(config=None) -> None` | enable the form environment |

[ENTRYPOINT_SCOPE]: render and text extraction
- rail: pdf

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `PdfPage.render` | `render(scale=1, rotation=0, crop=(0,0,0,0), may_draw_forms=True, fill_color=(255,255,255,255), color_scheme=None, draw_annots=True, draw_forms=True, bitmap_maker=PdfBitmap.new_native, **kwargs) -> PdfBitmap` | rasterize a page |
| [2] | `PdfBitmap.to_pil` | `to_pil() -> PIL.Image.Image` | bridge to a Pillow image |
| [3] | `PdfBitmap.to_numpy` | `to_numpy() -> numpy.ndarray` | bridge to a NumPy array |
| [4] | `PdfPage.get_textpage` | `get_textpage() -> PdfTextPage` | build a text model |
| [5] | `PdfTextPage.get_text_range` | `get_text_range(index=0, count=-1, errors='ignore') -> str` | extract a character range |
| [6] | `PdfTextPage.get_text_bounded` | `get_text_bounded(left=None, bottom=None, right=None, top=None, errors='ignore') -> str` | extract text in a region |
| [7] | `PdfTextPage.search` | `search(text, index=0, count=-1, match_case=False, match_whole_word=False, consecutive=False) -> PdfTextSearcher` | build a search cursor |
| [8] | `PdfPage.get_objects` | `get_objects(filter=None, max_depth=15) -> typing.Iterator[PdfObject]` | enumerate page objects |

## [4]-[IMPLEMENTATION_LAW]

[PDF_PDFIUM]:
- import: `import pypdfium2` at boundary scope only; module-level import is banned by the manifest import policy.
- document axis: one `PdfDocument` owns open/new/save; `new` (classmethod) and `import_pages` are the build modalities, rows not parallel types.
- render axis: `PdfPage.render` returns a `PdfBitmap`; `to_pil`/`to_numpy` are the single bridge pair to the image/compute owners, never a re-minted raster type.
- extraction axis: `PdfTextPage` with `get_text_range`/`get_text_bounded`/`search` is the extraction surface; results feed the document owner.
- evidence: each render captures page index, scale, rotation, color scheme, and output dimensions as a pdf receipt.
- boundary: pypdfium2 owns the PDFium render/inspect path; native structural redaction routes to `pymupdf`, pure-Python assembly to `pypdf`; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pypdfium2`
- Owns: PDFium document open/build/save, page rasterization to bitmap/PIL/NumPy, text extraction and search, page-object inspection, outline, forms, attachments
- Accept: high-fidelity render and inspection feeding the document/PDF and image owners
- Reject: wrapper-renames of `render`/`get_textpage`; a second raster type where `PdfBitmap` already bridges; identity minting the runtime owns
