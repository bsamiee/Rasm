# [PY_ARTIFACTS_API_PYPDFIUM2]

`pypdfium2` supplies the PDFium-backed PDF render and inspection surface for the artifacts pdf rail: a document root, a page unit, a render bitmap, and a text page that drive high-fidelity rasterization, text extraction/search, page-object inspection, and form access against Google's PDFium engine. The package owner composes `PdfDocument`, `PdfPage`, `PdfBitmap`, and `PdfTextPage` into the pdf owner; it never re-implements the PDFium render pipeline the native core already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pypdfium2`
- package: `pypdfium2`
- import: `pypdfium2`
- owner: `artifacts`
- rail: pdf
- installed: `5.10.1` reflected via `python -c "import pypdfium2"` on cp315
- entry points: none (library only)
- capability: PDFium document open, page rasterization to bitmap/PIL/NumPy, text extraction and search, page-object and image inspection, bookmarks/destinations, form environment, attachments

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and page roots
- rail: pdf

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE] | [CAPABILITY]                                                        |
| :-----: | :------------------------ | :------------- | :------------------------------------------------------------------ |
|  [01]   | `PdfDocument`             | document root  | open/new/save a PDF; page access, metadata, TOC, attachments, forms |
|  [02]   | `PdfPage`                 | page unit      | render, text page, object enumeration, box geometry, rotation       |
|  [03]   | `PdfBitmap`               | render buffer  | rasterized bitmap with PIL/NumPy bridges and fill ops               |
|  [04]   | `PdfTextPage`             | text model     | char/rect/range text extraction and search                          |
|  [05]   | `PdfTextSearcher`         | search cursor  | forward/backward text search state                                  |
|  [06]   | `PdfObject`               | page object    | a single page content object (text/image/path)                      |
|  [07]   | `PdfImage`                | image object   | embedded image with extract/render/metadata                         |
|  [08]   | `PdfFont`                 | font handle    | loaded font for inserted text                                       |
|  [09]   | `PdfBookmark` / `PdfDest` | outline        | bookmark node and its destination                                   |
|  [10]   | `PdfFormEnv`              | form env       | interactive form environment handle                                 |
|  [11]   | `PdfAttachment`           | attachment     | embedded-file attachment                                            |
|  [12]   | `PdfMatrix`               | affine matrix  | transform value object for render/object placement                  |

[PUBLIC_TYPE_SCOPE]: faults
- rail: pdf

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE] | [CAPABILITY]                         |
| :-----: | :-------------- | :------------- | :----------------------------------- |
|  [01]   | `PdfiumError`   | engine fault   | a PDFium call returned an error code |
|  [02]   | `PdfiumWarning` | engine warning | a recoverable PDFium condition       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document open, build, and save
- rail: pdf

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                 | [CAPABILITY]                         |
| :-----: | :------------------------- | :----------------------------------------------------------- | :----------------------------------- |
|  [01]   | `PdfDocument`              | `PdfDocument(input, password=None, autoclose=False)`         | open from path/bytes/buffer          |
|  [02]   | `PdfDocument.new`          | `new() -> PdfDocument`                                       | classmethod: build an empty document |
|  [03]   | `PdfDocument.save`         | `save(dest, version=None, flags=..., callback=None) -> None` | serialize to a writable buffer       |
|  [04]   | `PdfDocument.get_page`     | `get_page(index: int) -> PdfPage`                            | page accessor                        |
|  [05]   | `PdfDocument.import_pages` | `import_pages(pdf, pages=None, index=None) -> None`          | splice pages from another document   |
|  [06]   | `PdfDocument.get_toc`      | `get_toc(max_depth=15) -> typing.Iterator[PdfBookmark]`      | walk the outline                     |
|  [07]   | `PdfDocument.init_forms`   | `init_forms(config=None) -> None`                            | enable the form environment          |

[ENTRYPOINT_SCOPE]: render and text extraction
- rail: pdf

Render rows share scale, rotation, crop, form, color, annotation, and bitmap-maker policy; text rows share range, bounds, and search policy.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                 | [CAPABILITY]              |
| :-----: | :----------------------------- | :--------------------------- | :------------------------ |
|  [01]   | `PdfPage.render`               | render policy -> `PdfBitmap` | rasterize a page          |
|  [02]   | `PdfBitmap.to_pil`             | no-arg Pillow bridge         | bridge to a Pillow image  |
|  [03]   | `PdfBitmap.to_numpy`           | no-arg NumPy bridge          | bridge to a NumPy array   |
|  [04]   | `PdfPage.get_textpage`         | no-arg text-page build       | build a text model        |
|  [05]   | `PdfTextPage.get_text_range`   | index/count text range       | extract a character range |
|  [06]   | `PdfTextPage.get_text_bounded` | rectangle bounds plus errors | extract text in a region  |
|  [07]   | `PdfTextPage.search`           | text plus match policy       | build a search cursor     |
|  [08]   | `PdfPage.get_objects`          | filter plus depth policy     | enumerate page objects    |

## [04]-[IMPLEMENTATION_LAW]

[PDF_PDFIUM]:
- import: `import pypdfium2` at boundary scope only; module-level import is banned by the manifest import policy.
- document axis: one `PdfDocument` owns open/new/save; `new` (classmethod) and `import_pages` are the build modalities, rows not parallel types.
- render axis: `PdfPage.render` returns a `PdfBitmap`; `to_pil`/`to_numpy` are the single bridge pair to the image/compute owners, never a re-minted raster type.
- extraction axis: `PdfTextPage` with `get_text_range`/`get_text_bounded`/`search` is the extraction surface; results feed the document owner.
- evidence: each render captures page index, scale, rotation, color scheme, and output dimensions as a pdf receipt.
- boundary: pypdfium2 owns the PDFium render/inspect path; native structural redaction routes to `pymupdf`, pure-Python assembly to `pypdf`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pypdfium2`
- Owns: PDFium document open/build/save, page rasterization to bitmap/PIL/NumPy, text extraction and search, page-object inspection, outline, forms, attachments
- Accept: high-fidelity render and inspection feeding the document/PDF and image owners
- Reject: wrapper-renames of `render`/`get_textpage`; a second raster type where `PdfBitmap` already bridges; identity minting the runtime owns
