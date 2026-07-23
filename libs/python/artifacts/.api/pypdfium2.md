# [PY_ARTIFACTS_API_PYPDFIUM2]

`pypdfium2` binds Google's PDFium through ctypes helpers and owns the BSD, Pillow-free PDF render and inspection surface for the artifacts pdf rail: page rasterization to bitmap/PIL/NumPy, layout-faithful text extraction and search with per-char geometry, native outline harvest, page-object/image/font authoring, the `PdfMatrix` affine algebra, and interactive-form access. It never re-implements the PDFium render pipeline, the affine algebra, the headless font-substitution shim, or the `pypdfium2.raw` ctypes bindings the native core already binds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pypdfium2`
- package: `pypdfium2` (BSD-3-Clause)
- module: `pypdfium2` (prebuilt PDFium shared library and ctypes helpers; `pypdfium2.raw` binds the full `FPDF_*` C ABI)
- namespaces: `pypdfium2`, `pypdfium2.raw`
- rail: pdf — Pillow-free PDFium render, outline harvest, char-geometry extract, page-object/image/font edit
- depends: zero hard Python runtime deps; Pillow and NumPy are optional bridges resolved only at `to_pil`/`to_numpy`/`from_pil`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, page, render, and matrix roots

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :---------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `PdfDocument`     | class         | open/new/save, page access, metadata, TOC, attachments, forms       |
|  [02]   | `PdfPage`         | class         | render/text, object edits, all PDF boxes, rotation, flatten         |
|  [03]   | `PdfBitmap`       | class         | raster buffer, PIL/NumPy bridges, fill, posconv, allocators         |
|  [04]   | `PdfTextPage`     | class         | char/rect/range/bounded extraction, geometry, object lookup, search |
|  [05]   | `PdfTextSearcher` | class         | forward/backward search cursor state                                |
|  [06]   | `PdfMatrix`       | value-object  | affine transform builders and point/rect application                |
|  [07]   | `PdfColorScheme`  | value-object  | path/text fill/stroke override with byte-order conversion           |
|  [08]   | `PdfPosConv`      | class         | page-to-bitmap and bitmap-to-page coordinate conversion             |

[PUBLIC_TYPE_SCOPE]: page-content objects, fonts, outline, and attachments

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :-------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `PdfObject`     | class         | path/image/text geometry and matrix transforms          |
|  [02]   | `PdfImage`      | class         | embedded-image authoring, extraction, metadata, filters |
|  [03]   | `PdfTextObj`    | class         | text/font inspection with inherited object geometry     |
|  [04]   | `PdfFont`       | class         | inserted-text standard-font loading and metadata        |
|  [05]   | `PdfXObject`    | class         | page reuse; `as_pageobject()` mints a stampable object  |
|  [06]   | `PdfBookmark`   | class         | title, signed child count, destination                  |
|  [07]   | `PdfDest`       | class         | page index and view fit-mode                            |
|  [08]   | `PdfFormEnv`    | class         | interactive AcroForm/XFA handle                         |
|  [09]   | `PdfAttachment` | class         | embedded-file data, name, typed parameters, mutation    |

[PUBLIC_TYPE_SCOPE]: headless font substitution and unsupported-feature handling

Process-singleton handlers installed once at boundary setup so a missing embedded font or an unsupported feature resolves deterministically instead of failing the raster.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :----------------- | :------------ | :----------------------------------------------- |
|  [01]   | `PdfSysfontBase`   | class         | active PDFium system-font substitution singleton |
|  [02]   | `PdfDefaultTTFMap` | singleton     | default font-name to system-TTF mapping          |
|  [03]   | `PdfUnspHandler`   | class         | XFA/portfolio unsupported-feature callback       |

[PUBLIC_TYPE_SCOPE]: faults and the escape hatch

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :-------------- | :------------ | :-------------------------------------------- |
|  [01]   | `PdfiumError`   | exception     | PDFium error code mapped to the runtime fault |
|  [02]   | `PdfiumWarning` | exception     | recoverable PDFium condition                  |
|  [03]   | `raw`           | module        | full PDFium C API for helper gaps             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document open, build, metadata, attachments, and save

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :---------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `PdfDocument(input, password=None, autoclose=False)`              | ctor     | open path/bytes/buffer/handle            |
|  [02]   | `PdfDocument.new()`                                               | factory  | build an empty document                  |
|  [03]   | `save(dest, version=None, flags=0)`                               | instance | serialize via raw save-flag bits         |
|  [04]   | `get_page(i)` / `new_page(w, h, index=None)` / `del_page(i)`      | instance | page access, build, drop                 |
|  [05]   | `import_pages(pdf, pages=None, index=None)`                       | instance | splice pages from another document       |
|  [06]   | `page_as_xobject(index, dest_pdf)`                                | instance | reuse a page as a form XObject           |
|  [07]   | `get_toc(max_depth=15, parent=None, level=0, seen=None)`          | instance | depth-bounded cycle-guarded outline walk |
|  [08]   | `get_metadata_dict(skip_empty=False)` / `get_metadata_value(key)` | instance | docinfo via `METADATA_KEYS`              |
|  [09]   | `init_forms(config=None)` / `get_formtype()` / `close_forms()`    | instance | AcroForm/XFA environment lifecycle       |

- [10]-[ATTACHMENTS]: `count_attachments()` `get_attachment(i)` `new_attachment(name)` `del_attachment(i)` walk the embedded-file table; `PdfAttachment` reads and writes data, name, and typed parameters.
- [11]-[METADATA_KEYS]: `Title` `Author` `Subject` `Keywords` `Creator` `Producer` `CreationDate` `ModDate`.
- [12]-[IDENTITY]: `get_identifier(type=0)` `get_version()` `get_pagemode()` `get_page_label(index)` `get_page_size(index)` `is_tagged()` recover document identity and structural facts.

[ENTRYPOINT_SCOPE]: render, bitmap allocators, and page-object editing
- render carry: `scale`, `rotation`, `crop`, `may_draw_forms`, `bitmap_maker`, `color_scheme`, `fill_to_stroke`

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :--------------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `PdfPage.render(...) -> PdfBitmap`                               | instance | form-aware page raster                           |
|  [02]   | `to_pil()` / `to_numpy()`                                        | instance | Pillow/NumPy buffer bridge, zero-copy when valid |
|  [03]   | `PdfBitmap.new_native` / `new_foreign` / `new_foreign_simple`    | factory  | PDFium-owned or host-buffer backing              |
|  [04]   | `PdfBitmap.from_pil(img)` / `from_raw(raw)`                      | factory  | wrap a PIL image or raw `FPDF_BITMAP`            |
|  [05]   | `.fill_rect(color, l, t, w, h)` / `.get_posconv(page)`           | instance | fill a region, bitmap-to-page coords             |
|  [06]   | `PdfColorScheme(path_fill, path_stroke, text_fill, text_stroke)` | ctor     | per-render fill/stroke override                  |
|  [07]   | `get_objects(filter=None, max_depth=15, form=None)`              | instance | enumerate page-content objects                   |
|  [08]   | `insert_obj(obj)` / `remove_obj(obj)` / `gen_content()`          | instance | author/drop objects, regenerate stream           |
|  [09]   | `PdfFont.load_standard(pdf, name)`                               | factory  | load one of 14 `STANDARD_FONTS`                  |

- [10]-[OBJECT_TRANSFORM]: `get_matrix()` `set_matrix(m)` `transform(m)` `get_bounds()` `get_quad_points()` read or replace the object CTM and recover axis-aligned bounds and the rotated quad.
- [11]-[PDFIMAGE]: `PdfImage.new(pdf)` mints an image object; `set_bitmap(bitmap, pages=None)` and `load_jpeg(source, inline=False)` author it, JPEG embedded without re-encode; `extract(dest)` `get_bitmap(render=False)` `get_data(decode_simple=False)` `get_filters()` `get_px_size()` recover data, filters, and size, and `SIMPLE_FILTERS` names the inline-decodable codecs.
- [12]-[TEXTOBJ]: `PdfTextObj.get_font()` `get_font_size()` `extract()` inspect the object's font, size, and string.
- [13]-[PAGE_BOXES]: `get_mediabox`/`cropbox`/`bleedbox`/`trimbox`/`artbox` read and `set_*box(l, b, r, t)` write the full box family; `get_rotation`/`set_rotation`/`flatten(flag=0)`/`get_bbox` cover rotation and annotation flatten — bleed/trim/art are load-bearing for PDF/X print production.

[ENTRYPOINT_SCOPE]: text extraction, char geometry, and search
- search carry: `text`, `index`, `match_case`, `match_whole_word`, `consecutive`, `flags`

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------ | :------- | :------------------------------ |
|  [01]   | `PdfPage.get_textpage() -> PdfTextPage`                       | instance | construct the page text model   |
|  [02]   | `get_text_range(index=0, count=-1, errors="ignore")`          | instance | extract a character range       |
|  [03]   | `get_text_bounded(left, bottom, right, top, errors="ignore")` | instance | extract text inside a rectangle |
|  [04]   | `count_chars()` / `count_rects(index=0, count=-1)`            | instance | character and rectangle counts  |
|  [05]   | `search(...) -> PdfTextSearcher`                              | instance | text search cursor              |
|  [06]   | `get_next()` / `get_prev()`                                   | instance | forward/backward match stepping |

- [07]-[CHAR_GEOMETRY]: `get_charbox(index, loose=False)` `get_rect(index)` `get_index(x, y, x_tol, y_tol)` `get_textobj(index)` recover per-character geometry, hit-test a point, and bridge a char index to its owning `PdfTextObj`.

[ENTRYPOINT_SCOPE]: outline walk, affine matrix, and headless handlers

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `PdfBookmark.get_title()`·`get_count()`·`get_dest()`·`get_color()` | instance | title, signed sub-item count, destination, RGB color |
|  [02]   | `PdfDest.get_index()` / `get_view(seqtype=list)`                   | instance | destination page index and fit-mode view             |
|  [03]   | `PdfMatrix(a, b, c, d, e, f)` / `PdfMatrix.from_raw(raw)`          | ctor     | affine transform, `FS_MATRIX` inbound                |
|  [04]   | `PdfSysfontBase().setup(reusable=False)`                           | instance | install deterministic missing-font fallback          |
|  [05]   | `PdfUnspHandler().setup(add_default=True)`                         | instance | install unsupported-feature callback                 |

- `PdfBookmark.get_color()` returns the outline entry's `(r, g, b)` floats in `0..1` or `None` when unset; it binds a recent PDFium (`FPDFBookmark_GetColor`), so a lagging conda/system-search build falls back to the `raw` seam.
- [06]-[MATRIX_BUILDERS]: `scale(x, y)` `rotate(angle, ccw=False, rad=False)` `translate(x, y)` `skew(x_angle, y_angle, rad=False)` `mirror(invert_x, invert_y)` `multiply(other)` compose the affine, each returning the composed `PdfMatrix`; `on_point(x, y)` `on_rect(left, bottom, right, top)` `get()` `to_raw()` apply it and bridge outbound.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `PdfPage.render(...) -> PdfBitmap` then `to_numpy()` is the CORE-band raster fold, landing straight onto the runtime band with no Pillow dependency: `bitmap_maker` selects PDFium-owned (`new_native`) versus host-buffer (`new_foreign`) backing, `PdfColorScheme` overrides path/text fill/stroke, and `get_posconv(page)` maps bitmap-to-page coordinates for hit-testing rendered output. This is the distinct band-and-engine axis — the Pillow-free BSD PDFium rasterizer beside Rust `pdf_oxide` and AGPL `pymupdf`.
- `get_toc(max_depth)` yields `PdfBookmark`s read as one outline triple through `get_title()`, `get_count()` (the signed sub-item count, positive open and negative closed), and `get_dest().get_index()`, with `get_color()` reading the optional RGB label color when the bundled PDFium carries it; `PdfBookmark` exposes no `.level`, so any level derives from walk depth.
- `PdfTextPage` with `get_text_range`/`get_text_bounded`/`search` is the text surface; `count_chars`/`get_charbox`/`get_rect`/`get_index`/`get_textobj` recover per-character geometry and the owning text object for layout-faithful reconstruction. Undecodable spans yield the `errors="ignore"` substitution, so empty or partial text is data, never a fault.
- `get_objects` enumerates content objects, `insert_obj`/`remove_obj` author and drop, and `gen_content()` regenerates the content stream after edits; object placement composes the `PdfMatrix` algebra (`scale`/`rotate`/`translate` builders to the CTM, `set_matrix`/`transform`, `get_quad_points` for the rotated bounds), never a hand-rolled 2x3 affine. Page-as-XObject stamping is `page_as_xobject(index, dest)` to `as_pageobject()` to `insert_obj`.
- `PdfImage.new` with `set_bitmap`/`load_jpeg` authors embedded rasters and `extract`/`get_bitmap`/`get_data`/`get_filters` recovers them; `load_jpeg` embeds bytes with no re-encode and `SIMPLE_FILTERS` names the inline-decodable codecs, never a raw `raw.FPDFImageObj_*` call where the helper wraps it.
- `PdfFont.load_standard(pdf, name)` loads a base font for inserted text; `PdfSysfontBase().setup()` with `PdfDefaultTTFMap` installs headless missing-font substitution and `PdfUnspHandler().setup()` logs unsupported features — both installed once at boundary setup for reproducible headless rendering.
- `init_forms(config)` opens the AcroForm/XFA environment, `render(may_draw_forms=True)` draws field appearances into the raster, `get_formtype()` discriminates AcroForm versus XFA, and `close_forms()` tears the environment down before document close.
- `pypdfium2.raw` is the full PDFium C API for surfaces the helpers do not wrap (signature dictionaries, JavaScript actions, page-object marks); the `FPDFBitmap_*` formats, `FPDF_ANNOT_*`, `FPDF_TEXTRENDERMODE_*`, `FPDF_RENDER_*`, and `FPDF_*` save-flag families are the typed bit sources for helper `flags=`/`format=` parameters, and any reused `FPDF_*` sequence re-internalizes into a local owner.
- Each render captures page index, scale, rotation, color scheme, form-draw flag, fill-to-stroke, bitmap format, and output dimensions; each raster fold captures the outline-triple count and per-bookmark title/count/dest as the `PDF_RASTER` pdf receipt.

[STACKING]:
- `expression`(`libs/python/.api/expression.md`): `PdfiumError` maps at the boundary to the `Result[PdfReceipt, PdfError]` error arm, while `PdfiumWarning` stays a receipt note on the `Ok` arm; `errors="ignore"` text is `Ok` data.
- `numpy`(`libs/python/.api/numpy.md`): `PdfBitmap.to_numpy()` returns a `(height, width, channels)` `uint8` view over the live buffer (layout per `.format`, order per `.rev_byteorder`); the rail copies before the page closes and stacks per-page arrays into one frame stack the `graphic/raster/io` owner consumes without re-decoding.
- `anyio`(`libs/python/.api/anyio.md`): render and extract are GIL-bound C work fanned across `anyio.to_thread.run_sync` under the shared `CapacityLimiter`; one `PdfDocument`'s page work stays serialized within a worker, and parallelism runs across distinct documents.
- `structlog`(`libs/python/.api/structlog.md`) / `opentelemetry`(`libs/python/.api/opentelemetry-api.md`): each op rides the boundary span carrying page index, scale, and dimensions, and a `PdfiumWarning` is a structured span event; PDFium exposes no Python-side logger, so the boundary is the sole observability seam.
- `msgspec`(`libs/python/.api/msgspec.md`) / `pydantic`(`libs/python/.api/pydantic.md`): `get_charbox`/`get_rect` geometry tuples and outline triples admit at the boundary into the discriminated node models once; a live `PdfTextPage`/`PdfBookmark` handle never crosses into the interior, and `@beartype` on the boundary catches a wrong-shaped `crop`/`color_scheme`/`PdfMatrix` before ctypes.
- `pillow`(`.api/pillow.md`): `to_pil()`/`from_pil(img)` bridge to the `graphic/raster/io` owner for ICC or format-convert work on the WORKER band only; the CORE-band raster path stays `to_numpy` to keep it Pillow-free.
- within-lib: `document/emit` `PDF_RASTER` folds render-to-`to_numpy` and the outline harvest, `document/lens` folds char-geometry extraction into `RunNode`, and `document/egress` folds object-and-matrix edits and page-as-XObject stamping.
- pdf-rail: the rail meets `pypdf`/`pdf_oxide` (assemble), `pdfplumber` (ruled tables, itself rasterizing through pypdfium2), `ocrmypdf` (OCR layer, itself depending on pypdfium2), and `pikepdf` (AES-256-R6) at PDF bytes or an in-memory `PdfBitmap`/array.

[LOCAL_ADMISSION]:
- `import pypdfium2` at boundary scope only; module-level import is banned by the manifest import policy. PDFium is a process-global library and the `PdfDocument`/`PdfPage`/`PdfTextPage` tree owns a strict parent-to-child close order, so the rail holds each handle inside one `async_boundary` capsule and closes leaf-first for deterministic buffer lifetime.

[RAIL_LAW]:
- Package: `pypdfium2`
- Owns: PDFium document open/build/save, CORE-band page rasterization to bitmap/PIL/NumPy with scale/rotation/crop/color-scheme/form-aware/fill-to-stroke and native and foreign allocators, native outline harvest (`get_toc` to `PdfBookmark`/`PdfDest`), layout-faithful text extraction and search with per-char geometry, page-object and image inspection/authoring, the `PdfMatrix` affine algebra, embedded and standard fonts, the full media/crop/bleed/trim/art box family, attachment read/write with parameter dict, AcroForm/XFA forms, page-as-XObject reuse, headless font-substitution and unsupported-feature handlers, and the `pypdfium2.raw` ctypes escape hatch
- Accept: the BSD CORE-band raster and outline harvest feeding `document/emit` `PDF_RASTER`; char-geometry extraction feeding `document/lens` `RunNode`; render-to-numpy feeding `graphic/raster/io`; object-and-matrix edit and page-as-XObject stamp feeding `document/egress`; the optional `to_pil` bridge on the WORKER band only
- Reject: wrapper-renames of `render`/`get_textpage`/`get_toc`; a second raster type where `PdfBitmap` bridges PIL/NumPy; a `save` callback parameter it does not carry; a `mark.level` outline read; a hand-rolled 2x3 affine where `PdfMatrix` owns the algebra; commercial-safe separations/PDF-X/PAdES where `pdf_oxide` owns them; native structural scrub where `pymupdf` owns it; pure-Python assembly where `pypdf` owns it; a `raw.FPDF_*` call where a helper wraps it; forwarding a live `PdfTextPage`/`PdfBookmark` handle past the document lifetime; eager `to_pil` on the CORE band; identity minting the runtime owns
