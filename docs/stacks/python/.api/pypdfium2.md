# [PY_ARTIFACTS_API_PYPDFIUM2]

`pypdfium2` supplies the PDFium-backed PDF render and inspection surface for the artifacts pdf rail: a document root, a page unit, a render bitmap, and a text page that drive high-fidelity rasterization, text extraction/search, page-object inspection, and form access against Google's PDFium engine. The package owner composes `PdfDocument`, `PdfPage`, `PdfBitmap`, and `PdfTextPage` into the pdf owner; it never re-implements the PDFium render pipeline, the `PdfMatrix` affine, or the ctypes `raw` bindings the native core already owns. The catalog drives the BSD render arm of the pdf rail — pypdfium2 is the permissively-licensed rasterizer admissible where AGPL `pymupdf` is barred, `pypdf` does pure-Python structural editing, `pikepdf` owns AES-256-R6, `ocrmypdf` owns OCR-to-PDF/A — every owner meeting the others at PDF bytes or an in-memory `PdfBitmap`/`PIL.Image`/NumPy array.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pypdfium2`
- package: `pypdfium2`
- import: `pypdfium2`
- owner: `artifacts`
- rail: pdf
- installed: `5.10.1` (`pypdfium2.PYPDFIUM_INFO`); bundles PDFium binary `pypdfium2.PDFIUM_INFO`
- license: `BSD-3-Clause` (helpers) + `Apache-2.0`/`BSD-3-Clause` (bundled PDFium binary, see vendored licenses); permissive — the render arm admissible in a closed/distributed service where AGPL `pymupdf` is not
- abi: `py3-none-*` platform wheels (Tag `py3-none-<platform>`); PDFium ships as a prebuilt binary loaded via ctypes (`pypdfium2_raw`), so there is no Python-ABI floor; `Requires-Python >=3.6`; ungated in the manifest
- entry points: console script `pypdfium2` (CLI: render/toc/info/…); library use is import-only
- capability: PDFium document open/new/save, page rasterization to bitmap/PIL/NumPy (scale/rotation/crop/color-scheme/form-aware), text extraction/search/char-geometry, page-object + image inspection and editing, outline/destination walk, attachment read/write, AcroForm/XFA form environment, page-as-XObject reuse, raw ctypes escape hatch

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and page roots
- rail: pdf

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE] | [CAPABILITY]                                                        |
| :-----: | :------------------------ | :------------- | :------------------------------------------------------------------ |
|  [01]   | `PdfDocument`             | document root  | open/new/save a PDF; page access/insert, metadata, TOC, attachments, forms, page-as-XObject |
|  [02]   | `PdfPage`                 | page unit      | render, text page, object enumeration/insert, box geometry, rotation, flatten |
|  [03]   | `PdfBitmap`               | render buffer  | rasterized bitmap with PIL/NumPy bridges, fill ops, and `from_pil`/`from_raw` factories |
|  [04]   | `PdfTextPage`             | text model     | char/rect/range/bounded text extraction, char-geometry, and search  |
|  [05]   | `PdfTextSearcher`         | search cursor  | forward/backward text search state (`get_next`/`get_prev`)          |
|  [06]   | `PdfObject` / `PdfTextObj` | page object    | a single page content object (path/image/text); `PdfTextObj` is the text-object variant |
|  [07]   | `PdfImage`                | image object   | embedded image with extract/bitmap/metadata/filters/set-bitmap      |
|  [08]   | `PdfXObject`              | form xobject    | reusable page-as-XObject for stamping into another page             |
|  [09]   | `PdfFont`                 | font handle    | loaded font for inserted text objects                               |
|  [10]   | `PdfBookmark` / `PdfDest` | outline        | bookmark node (`get_title`/`get_count`/`get_dest`) and its destination (`get_index`/`get_view`) |
|  [11]   | `PdfFormEnv`              | form env       | interactive AcroForm/XFA environment handle                         |
|  [12]   | `PdfAttachment`           | attachment     | embedded-file attachment (`get_data`/`set_data`/`get_name`/params)  |
|  [13]   | `PdfMatrix`               | affine matrix  | transform value object for render/object placement                  |
|  [14]   | `PdfColorScheme`          | render colors  | render color override (`path_fill`/`path_stroke`/`text_fill`/`text_stroke`) |
|  [15]   | `PdfPosConv`              | coord converter | page<->bitmap coordinate conversion via `to_page(x, y)`/`to_bitmap(x, y)` (from `PdfBitmap.get_posconv(page)`) |
|  [16]   | `raw` (`pypdfium2_raw`)   | ctypes bindings | the full PDFium C API (`FPDF_*`) escape hatch for surfaces the helpers do not wrap |

[PUBLIC_TYPE_SCOPE]: faults
- rail: pdf

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE] | [CAPABILITY]                         |
| :-----: | :-------------- | :------------- | :----------------------------------- |
|  [01]   | `PdfiumError`   | engine fault   | a PDFium call returned an error code |
|  [02]   | `PdfiumWarning` | engine warning | a recoverable PDFium condition       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document open, build, and save
- rail: pdf

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                                  | [CAPABILITY]                                          |
| :-----: | :----------------------------- | :----------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `PdfDocument`                  | `PdfDocument(input, password=None, autoclose=False)`         | open from path/bytes/buffer/file-handle               |
|  [02]   | `PdfDocument.new`              | `new() -> PdfDocument`                                        | classmethod: build an empty document                  |
|  [03]   | `PdfDocument.save`             | `save(dest, version=None, flags=0) -> None`                  | serialize to a writable buffer (no `callback` param)  |
|  [04]   | `PdfDocument.get_page` / `new_page` / `del_page` | `get_page(index)` / `new_page(width, height, index=None)` / `del_page(index)` | page access / build / drop                            |
|  [05]   | `PdfDocument.import_pages`     | `import_pages(pdf, pages=None, index=None) -> None`          | splice pages from another document                    |
|  [06]   | `PdfDocument.page_as_xobject`  | `page_as_xobject(index, dest_pdf) -> PdfXObject`             | reuse a page as a stampable form XObject              |
|  [07]   | `PdfDocument.get_toc`          | `get_toc(max_depth=15) -> Iterator[PdfBookmark]`             | walk the outline                                      |
|  [08]   | `PdfDocument.get_metadata_dict` / `get_metadata_value` | `get_metadata_dict(skip_empty=False)` / `get_metadata_value(key)` | recover docinfo metadata (standard keys `Title`/`Author`/`Subject`/`Keywords`/`Creator`/`Producer`/`CreationDate`/`ModDate`) |
|  [09]   | `PdfDocument.get_identifier` / `get_version` / `get_pagemode` / `is_tagged` / `get_page_label` | accessors        | document identity/version/page-mode/tagged/page-label |
|  [10]   | `PdfDocument` attachments      | `count_attachments()` / `get_attachment(i)` / `new_attachment(name)` / `del_attachment(i)` | embedded-file attachment table                        |
|  [11]   | `PdfDocument.init_forms`       | `init_forms(config=None) -> None`                            | enable the AcroForm/XFA form environment              |

[ENTRYPOINT_SCOPE]: render, bitmap bridges, and page-object editing
- rail: pdf

Render rows share scale, rotation, crop, form-draw, color-scheme, fill-to-stroke, and bitmap-maker policy; `PdfBitmap` exposes both the bridge pair and the inbound factories; page-object rows enumerate and author content.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                                                                          | [CAPABILITY]                                          |
| :-----: | :----------------------------- | :---------------------------------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `PdfPage.render`               | `render(scale=1, rotation=0, crop=(0,0,0,0), may_draw_forms=True, bitmap_maker=PdfBitmap.new_native, color_scheme=None, fill_to_stroke=False) -> PdfBitmap` | rasterize a page (form-aware, color-overridable)      |
|  [02]   | `PdfBitmap.to_pil` / `to_numpy` | no-arg bridges                                                                                       | bridge the buffer to a Pillow `Image` / NumPy array (zero-copy where layout permits) |
|  [03]   | `PdfBitmap.from_pil` / `from_raw` | `from_pil(pil_image)` / `from_raw(raw, rev_byteorder=False, ex_buffer=None)`                        | wrap an existing PIL image / raw `FPDF_BITMAP` handle as a `PdfBitmap` |
|  [04]   | `PdfBitmap` attrs              | `.width` / `.height` / `.stride` / `.format` / `.rev_byteorder` / `.buffer` / `.fill_rect(...)` / `.get_posconv(page)` | bitmap geometry, buffer access, fill, coord converter |
|  [05]   | `PdfColorScheme`               | `PdfColorScheme(path_fill, path_stroke, text_fill, text_stroke)`                                     | per-render color override fed to `render(color_scheme=)` |
|  [06]   | `PdfPage.get_objects` / `insert_obj` / `gen_content` | `get_objects(filter=None, max_depth=15, form=None)` / `insert_obj(obj)` / `gen_content()` | enumerate / author page content objects               |
|  [07]   | `PdfPage` boxes / rotation     | `get_size` / `get_mediabox` / `get_cropbox` / `get_bbox` / `set_mediabox` / `get_rotation` / `set_rotation` / `flatten` | page box geometry, rotation, annotation flatten        |
|  [08]   | `PdfImage` / `PdfObject`       | `PdfImage.extract(dest)` / `get_bitmap(render=False)` / `get_metadata()` / `get_filters()` / `set_bitmap(bitmap)`; `PdfObject.get_matrix()` / `set_matrix()` / `get_bounds()` | extract/replace embedded images, read object geometry |

[ENTRYPOINT_SCOPE]: text extraction, char geometry, and search
- rail: pdf

Text rows share range, bounds, char-index, and search policy; char-geometry rows recover per-character bounding boxes for layout-faithful reconstruction.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                                            | [CAPABILITY]                                          |
| :-----: | :----------------------------- | :--------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `PdfPage.get_textpage`         | `get_textpage() -> PdfTextPage`                                         | build a text model for the page                       |
|  [02]   | `PdfTextPage.get_text_range`   | `get_text_range(index=0, count=-1, errors="ignore") -> str`            | extract a character range                             |
|  [03]   | `PdfTextPage.get_text_bounded` | `get_text_bounded(left=None, bottom=None, right=None, top=None, errors="ignore") -> str` | extract text within a rectangle                       |
|  [04]   | `PdfTextPage.count_chars` / `count_rects` | `count_chars() -> int` / `count_rects(index, count) -> int`         | char and rect counts for iteration                    |
|  [05]   | `PdfTextPage.get_charbox` / `get_rect` / `get_index` | `get_charbox(index, loose=False)` / `get_rect(index)` / `get_index(x, y, x_tol, y_tol)` | per-char bbox, rect geometry, hit-test a point        |
|  [06]   | `PdfTextPage.search`           | `search(text, index=0, match_case=False, match_whole_word=False, consecutive=False, flags=0) -> PdfTextSearcher` | build a search cursor (`flags` passes raw `FPDF_MATCHCASE`/`FPDF_MATCHWHOLEWORD`/`FPDF_CONSECUTIVE` bits) |
|  [07]   | `PdfTextSearcher`              | `get_next()` / `get_prev()` (return char index + count, or `None`)     | step matches forward/backward                         |

## [04]-[IMPLEMENTATION_LAW]

[PDF_PDFIUM]:
- import: `import pypdfium2` at boundary scope only; module-level import is banned by the manifest import policy.
- document axis: one `PdfDocument` owns open/new/save; `new` (classmethod), `new_page`, `import_pages`, and `page_as_xobject` are the build modalities, rows not parallel types. `save(dest, version=, flags=)` is the emission surface — there is no `callback` parameter.
- render axis: `PdfPage.render(scale=, rotation=, crop=, may_draw_forms=, color_scheme=, fill_to_stroke=)` returns a `PdfBitmap`; `to_pil`/`to_numpy` are the single outbound bridge pair to the image/compute owners and `from_pil`/`from_raw` are the inbound factories, never a re-minted raster type; `PdfColorScheme` drives color override and `PdfBitmap.get_posconv(page)` maps bitmap<->page coordinates for hit-testing rendered output.
- extraction axis: `PdfTextPage` with `get_text_range`/`get_text_bounded`/`search` is the text surface; `count_chars`/`get_charbox`/`get_rect`/`get_index` recover per-character geometry for layout-faithful reconstruction; results feed the document owner. PDFium text fidelity is the BSD alternative to pymupdf's `get_text` where the AGPL path is barred.
- object axis: `PdfPage.get_objects(filter=, max_depth=)` enumerates content objects; `PdfImage.extract`/`get_bitmap`/`get_filters` inspect embedded rasters and `set_bitmap` replaces them; `insert_obj`/`gen_content` author content — never a raw `raw.FPDF_*` call where a helper wraps it.
- escape hatch: `pypdfium2.raw` (`pypdfium2_raw`) is the full PDFium ctypes C API; reach for it only for surfaces the helpers do not wrap (e.g. signature/JS), and re-internalize any reused `FPDF_*` sequence into a local owner.
- evidence: each render captures page index, scale, rotation, color scheme, form-draw flag, and output dimensions/format as a pdf receipt.
- license posture: BSD-3-Clause helpers over an Apache-2.0/BSD PDFium binary — pypdfium2 is the permissive render arm; pair it with BSD `pypdf` structural editing to keep a closed/distributed pipeline free of the AGPL `pymupdf` obligation.
- boundary: pypdfium2 owns the PDFium render/inspect/object-edit path; native structural redaction/scrub routes to `pymupdf`, pure-Python structural assembly to `pypdf`, AES-256-R6 to `pikepdf`, OCR-to-PDF/A to `ocrmypdf`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pypdfium2`
- Owns: PDFium document open/build/save, page rasterization to bitmap/PIL/NumPy (scale/rotation/crop/color-scheme/form-aware), text extraction/search/char-geometry, page-object + image inspection/editing, outline/destination walk, attachment read/write, AcroForm/XFA forms, page-as-XObject reuse, raw ctypes escape hatch
- Accept: high-fidelity BSD render, char-geometry extraction, and inspection feeding the document/PDF, image (PIL/NumPy), and compute owners; the permissive render arm for closed/distributed services
- Reject: wrapper-renames of `render`/`get_textpage`; a second raster type where `PdfBitmap` already bridges PIL/NumPy; a phantom `save` callback; native structural scrub where `pymupdf` owns it; pure-Python assembly where `pypdf` owns it; a `raw.FPDF_*` call where a helper already wraps it; identity minting the runtime owns
