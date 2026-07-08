# [PY_ARTIFACTS_API_PYPDFIUM2]

`pypdfium2` supplies the PDFium-backed PDF render and inspection surface that `document/emit#DOCUMENT` routes the `DocumentMode.PDF_RASTER` `Backend(Band.CORE, …)` arm through: a document root, a page unit, a render bitmap, a text page, an affine-matrix algebra, and the page-object/image/font/outline/attachment models that drive high-fidelity rasterization, layout-faithful text extraction/search, page-object inspection and authoring, and interactive-form access against Google's PDFium engine. The owner composes `PdfDocument`, `PdfPage`, `PdfBitmap`, `PdfTextPage`, `PdfMatrix`, and the object/font/outline models into the pdf owner; it never re-implements the PDFium render pipeline, the `PdfMatrix` affine, the font-substitution `FPDF_SYSFONTINFO` shim, or the `pypdfium2.raw` ctypes bindings the native core already owns. The catalog's load-bearing axis is distinct render footing, not the commercial-safe slot: `pdf_oxide` (MIT/Apache) is the categorical-best commercial-safe render/extract/redact/separation owner, so pypdfium2 does NOT carry that headline — pypdfium2 is the BSD, Pillow-free, CORE-band PDFium rasterizer + native outline-harvest arm (`render(**kw) -> PdfBitmap -> to_numpy` straight onto the runtime band with zero Pillow runtime dependency, plus `get_toc -> PdfBookmark -> PdfDest` → `OutlineRow` triples), sitting beside `pdf_oxide`'s Rust render and the AGPL `pymupdf` native render as the third render arm separated on the license-and-band axis the brief admits. `pypdf` does pure-Python structural editing, `pikepdf` owns AES-256-R6, `pdfplumber` owns ruled-table geometry (and itself rasterizes through pypdfium2 for debug overlays), `ocrmypdf` owns OCR-to-PDF/A (and itself depends on pypdfium2 as its rasterizer) — every owner meeting the others at PDF bytes or an in-memory `PdfBitmap`/`PIL.Image`/NumPy array.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pypdfium2`
- package: `pypdfium2`
- import: `pypdfium2`
- owner: `artifacts`
- rail: pdf
- installed: `5.10.1` (`PYPDFIUM_INFO == 5.10.1`; bundled `PDFIUM_INFO == 151.0.7891.0`)
- license: `BSD-3-Clause` (helpers) over an `Apache-2.0`/`BSD-3-Clause` bundled PDFium binary (`METADATA: License: BSD-3-Clause, Apache-2.0, dependency licenses`); permissive — no copyleft, so the render arm is admissible in a closed/distributed/SaaS pipeline where AGPL `pymupdf` is barred. This permissiveness is shared with `pdf_oxide`/`pypdf`, not unique to pypdfium2 — its distinct slot is the Pillow-free CORE-band render footing, not the license alone
- build floor: prebuilt PDFium binary + ctypes helper wheel, `py3-none-<platform>` (no Python ABI tag — the wheel is pure-Python helpers over a vendored shared library), `Requires-Python >= 3.6`; runs on the cp315 runtime in-process, no cp-gate, ungated in the manifest. Zero hard Python runtime deps — Pillow/NumPy are optional bridge targets resolved only when `to_pil`/`to_numpy`/`from_pil` are called, so the CORE-band raster arm never drags Pillow onto the runtime
- target: `osx-arm64` (and every PDFium-binary platform); resolved/reflected on cp315 `osx-arm64`
- entry points: console script `pypdfium2` (CLI: `render`/`toc`/`info`/`pdfinfo`/…); library use is import-only
- capability: PDFium document open/new/save (path/bytes/buffer/handle), page rasterization to bitmap/PIL/NumPy (scale/rotation/crop/color-scheme/form-aware/fill-to-stroke, native+foreign bitmap allocators), layout-faithful text extraction/search with per-char geometry, page-object + image inspection and authoring (path/image/text objects, JPEG embed, simple-filter decode), a full `PdfMatrix` affine algebra for object placement, embedded + standard-font handles, outline/destination walk, attachment table read/write with parameter dict, AcroForm/XFA form environment, page-as-XObject reuse, headless font-substitution + unsupported-feature handlers, the full PDFium box family (media/crop/bleed/trim/art), and a raw ctypes escape hatch (`pypdfium2.raw`, all `FPDF_*`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, page, render, and matrix roots
- rail: pdf

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfDocument` | document root | open/new/save a PDF; page access/insert/delete, metadata, TOC, attachments, forms, page-as-XObject |
| [02] | `PdfPage` | page unit | render, text page, object enumeration/insert/remove, the full box family (media/crop/bleed/trim/art), rotation, flatten |
| [03] | `PdfBitmap` | render buffer | rasterized bitmap with PIL/NumPy bridges, fill, posconv, and `new_native`/`new_foreign`/`from_pil`/`from_raw` allocators |
| [04] | `PdfTextPage` | text model | char/rect/range/bounded text extraction, per-char geometry, text-object lookup, and search |
| [05] | `PdfTextSearcher` | search cursor | forward/backward text search state (`get_next`/`get_prev`) |
| [06] | `PdfMatrix` | affine algebra | transform value object with `scale`/`rotate`/`translate`/`skew`/`mirror`/`multiply` builders + `on_point`/`on_rect` apply |
| [07] | `PdfColorScheme` | render colors | render color override (`path_fill`/`path_stroke`/`text_fill`/`text_stroke`) + `convert(rev_byteorder)` |
| [08] | `PdfPosConv` | coord converter | page<->bitmap coordinate conversion via `to_page(bitmap_x, bitmap_y)`/`to_bitmap(page_x, page_y)` (from `PdfBitmap.get_posconv(page)`) |

[PUBLIC_TYPE_SCOPE]: page-content objects, fonts, outline, and attachments
- rail: pdf

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfObject` | page object | base content object (path/image/text); `get_matrix`/`set_matrix`/`transform`/`get_bounds`/`get_quad_points` |
| [02] | `PdfImage` | image object | embedded image: `extract`/`get_bitmap`/`get_data`/`get_px_size`/`get_metadata`/`get_filters`/`set_bitmap`/`load_jpeg`/`new`; `SIMPLE_FILTERS` |
| [03] | `PdfTextObj` | text object | a text page-object: `get_font`/`get_font_size`/`extract` + the `PdfObject` geometry surface |
| [04] | `PdfFont` | font handle | inserted-text font: `load_standard`/`get_base_name`/`get_family_name`/`get_weight`/`is_embedded`; `STANDARD_FONTS` |
| [05] | `PdfXObject` | form xobject | reusable page-as-XObject; `as_pageobject()` mints the stampable `PdfObject` |
| [06] | `PdfBookmark` / `PdfDest` | outline | bookmark node (`get_title`/`get_count`/`get_dest`) and its destination (`get_index`/`get_view`) |
| [07] | `PdfFormEnv` | form env | interactive AcroForm/XFA environment handle (`close()`) |
| [08] | `PdfAttachment` | attachment | embedded-file attachment (`get_data`/`set_data`/`get_name`/`has_key`/`get_value_type`/`get_str_value`/`set_str_value`) |

[PUBLIC_TYPE_SCOPE]: headless font substitution and unsupported-feature handling
- rail: pdf

Process-singleton handlers for deterministic headless rendering — installed once at boundary setup so a missing embedded font or an unsupported PDF feature resolves predictably instead of failing the raster.

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfSysfontBase` | font-substitution | `FPDF_SYSFONTINFO` subclass base (`MapFont`/`GetFont`/`GetFontData`/…); `setup(reusable=False)` installs, `SINGLETON` holds the active handler |
| [02] | `PdfDefaultTTFMap` | font-name map | the default font-name → system-TTF mapping table consulted by the substitution handler |
| [03] | `PdfUnspHandler` | unsupported handler | `setup(add_default=True)` installs the unsupported-feature callback (XFA/portfolio/etc.); `SINGLETON` holds it |

[PUBLIC_TYPE_SCOPE]: faults and the escape hatch
- rail: pdf

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfiumError` | engine fault | a PDFium call returned an error code (the boundary maps it to the runtime fault) |
| [02] | `PdfiumWarning` | engine warning | a recoverable PDFium condition |
| [03] | `raw` (`pypdfium2.raw`) | ctypes bindings | the full PDFium C API (`FPDF_*` functions, `FPDFBitmap_*`/`FPDF_ANNOT_*`/`FPDF_TEXTRENDERMODE_*` constants) for surfaces the helpers do not wrap |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document open, build, metadata, attachments, and save
- rail: pdf

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfDocument` | `PdfDocument(input, password=None, autoclose=False)` | open from path/bytes/buffer/file-handle |
| [02] | `PdfDocument.new` | `new() -> PdfDocument` | staticmethod: build an empty document |
| [03] | `PdfDocument.save` | `save(dest, version=None, flags=0) -> None` | serialize to a writable buffer (`flags` passes raw `FPDF_INCREMENTAL`/`FPDF_NO_INCREMENTAL`/`FPDF_REMOVE_SECURITY` bits; NO `callback` param) |
| [04] | `PdfDocument.get_page` / `new_page` / `del_page` | `get_page(index)` / `new_page(width, height, index=None)` / `del_page(index)` | page access / build / drop |
| [05] | `PdfDocument.import_pages` | `import_pages(pdf, pages=None, index=None) -> None` | splice pages from another document |
| [06] | `PdfDocument.page_as_xobject` | `page_as_xobject(index, dest_pdf) -> PdfXObject` | reuse a page as a stampable form XObject |
| [07] | `PdfDocument.get_toc` | `get_toc(max_depth=15, parent=None, level=0, seen=None) -> Iterator[PdfBookmark]` | walk the outline (depth-bounded, cycle-guarded) |
| [08] | `PdfDocument.get_metadata_dict` / `get_metadata_value` | `get_metadata_dict(skip_empty=False)` / `get_metadata_value(key)` | recover docinfo (keys from `PdfDocument.METADATA_KEYS`: `Title`/`Author`/`Subject`/`Keywords`/`Creator`/`Producer`/`CreationDate`/`ModDate`) |
| [09] | `PdfDocument` identity | `get_identifier(type=0)` / `get_version()` / `get_pagemode()` / `get_page_label(index)` / `get_page_size(index)` / `is_tagged()` | document identity / version / page-mode / page-label / page-size / tagged |
| [10] | `PdfDocument` attachments | `count_attachments()` / `get_attachment(i)` / `new_attachment(name)` / `del_attachment(i)` | embedded-file attachment table |
| [11] | `PdfDocument` forms | `init_forms(config=None)` / `get_formtype()` / `close_forms()` / `.formenv` | enable/inspect/tear-down the AcroForm/XFA form environment |

[ENTRYPOINT_SCOPE]: render, bitmap allocators, and page-object editing
- rail: pdf

Render rows share scale, rotation, crop, form-draw, color-scheme, fill-to-stroke, and bitmap-maker policy; `PdfBitmap` exposes the bridge pair, the inbound factories, and the native/foreign allocators (`bitmap_maker`); page-object rows enumerate, author, and remove content; `PdfObject`/`PdfImage`/`PdfTextObj` carry geometry + matrix transform.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfPage.render` | `render(scale=1, rotation=0, crop=(0,0,0,0), may_draw_forms=True, bitmap_maker=PdfBitmap.new_native, color_scheme=None, fill_to_stroke=False) -> PdfBitmap` | rasterize a page (form-aware, color-overridable, fill-to-stroke) |
| [02] | `PdfBitmap.to_pil` / `to_numpy` | no-arg bridges | bridge the buffer to a Pillow `Image` / NumPy array (zero-copy where layout permits) — the CORE-band raster exit |
| [03] | `PdfBitmap.new_native` / `new_foreign` / `new_foreign_simple` | `new_native(width, height, format, rev_byteorder=False, buffer=None, stride=None)` / `new_foreign(width, height, format, rev_byteorder=False, force_packed=False)` / `new_foreign_simple(width, height, use_alpha, rev_byteorder=False)` | the `bitmap_maker` allocators: PDFium-owned (native) vs host-buffer (foreign) backing |
| [04] | `PdfBitmap.from_pil` / `from_raw` | `from_pil(pil_image)` / `from_raw(raw, rev_byteorder=False, ex_buffer=None)` | wrap an existing PIL image / raw `FPDF_BITMAP` handle as a `PdfBitmap` |
| [05] | `PdfBitmap` attrs | `.width` / `.height` / `.stride` / `.format` / `.rev_byteorder` / `.buffer` / `.fill_rect(color, left, top, width, height)` / `.get_posconv(page)` | bitmap geometry, buffer access, fill, coord converter (`.format` ∈ `raw.FPDFBitmap_BGR`/`BGRA`/`BGRx`/`BGRA_Premul`/`Gray`) |
| [06] | `PdfColorScheme` | `PdfColorScheme(path_fill, path_stroke, text_fill, text_stroke)` | per-render color override fed to `render(color_scheme=)` |
| [07] | `PdfPage.get_objects` / `insert_obj` / `remove_obj` / `gen_content` | `get_objects(filter=None, max_depth=15, form=None)` / `insert_obj(pageobj)` / `remove_obj(pageobj)` / `gen_content()` | enumerate / author / remove page content objects (regenerate the content stream after edits) |
| [08] | `PdfPage` boxes / rotation | `get_size` / `get_width` / `get_height` / `get_mediabox` / `get_cropbox` / `get_bleedbox` / `get_trimbox` / `get_artbox` / `set_mediabox(l,b,r,t)` / `set_cropbox` / `set_bleedbox` / `set_trimbox` / `set_artbox` / `get_bbox` / `get_rotation` / `set_rotation` / `flatten(flag=0)` | the full PDF box family (media/crop/bleed/trim/art — bleed/trim/art are load-bearing for PDF/X print production), rotation, annotation flatten |
| [09] | `PdfObject` transform | `get_matrix()` / `set_matrix(matrix)` / `transform(matrix)` / `get_bounds()` / `get_quad_points()` | read/replace the object CTM, compose a transform, recover axis-aligned bounds + rotated quad |
| [10] | `PdfImage` | `PdfImage.new(pdf)` / `extract(dest)` / `get_bitmap(render=False, scale_to_original=True)` / `get_data(decode_simple=False)` / `get_px_size()` / `get_metadata()` / `get_filters(skip_simple=False)` / `set_bitmap(bitmap, pages=None)` / `load_jpeg(source, pages=None, inline=False, autoclose=True)` | author/extract/replace embedded rasters, read pixel size + filter chain, embed a JPEG directly (`SIMPLE_FILTERS` lists the inline-decodable codecs) |
| [11] | `PdfTextObj` / `PdfFont` | `PdfTextObj.get_font()` / `get_font_size()` / `extract()`; `PdfFont.load_standard(pdf, name)` / `get_base_name()` / `get_family_name()` / `get_weight()` / `is_embedded` | inspect a text object's font + size + string; load one of the 14 `STANDARD_FONTS` for inserted text |

[ENTRYPOINT_SCOPE]: text extraction, char geometry, and search
- rail: pdf

Text rows share range, bounds, char-index, and search policy; char-geometry rows recover per-character bounding boxes for layout-faithful reconstruction; `get_textobj` bridges a char index to its owning `PdfTextObj`.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfPage.get_textpage` | `get_textpage() -> PdfTextPage` | build a text model for the page |
| [02] | `PdfTextPage.get_text_range` | `get_text_range(index=0, count=-1, errors="ignore") -> str` | extract a character range |
| [03] | `PdfTextPage.get_text_bounded` | `get_text_bounded(left=None, bottom=None, right=None, top=None, errors="ignore") -> str` | extract text within a rectangle |
| [04] | `PdfTextPage.count_chars` / `count_rects` | `count_chars() -> int` / `count_rects(index=0, count=-1) -> int` | char and rect counts for iteration |
| [05] | `PdfTextPage` char geometry | `get_charbox(index, loose=False)` / `get_rect(index)` / `get_index(x, y, x_tol, y_tol)` / `get_textobj(index)` | per-char bbox, rect geometry, hit-test a point, bridge a char index to its `PdfTextObj` |
| [06] | `PdfTextPage.search` | `search(text, index=0, match_case=False, match_whole_word=False, consecutive=False, flags=0) -> PdfTextSearcher` | build a search cursor (`flags` passes raw `FPDF_MATCHCASE`/`FPDF_MATCHWHOLEWORD`/`FPDF_CONSECUTIVE` bits) |
| [07] | `PdfTextSearcher` | `get_next()` / `get_prev()` (return `(char_index, count)`, or `None`) | step matches forward/backward |

[ENTRYPOINT_SCOPE]: outline walk, affine matrix, and headless handlers
- rail: pdf

The outline-harvest the `PDF_RASTER` arm folds into `OutlineRow` triples; the `PdfMatrix` affine algebra for object placement/stamping; and the process-singleton handlers installed once at boundary setup.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `PdfBookmark` | `get_title()` / `get_count()` / `get_dest() -> PdfDest` | bookmark title, signed sub-item count (>0 open, <0 closed), destination |
| [02] | `PdfDest` | `get_index()` / `get_view(seqtype=list)` | destination page index + view (fit-mode + params) |
| [03] | `PdfMatrix` builders | `PdfMatrix(a=1,b=0,c=0,d=1,e=0,f=0)` / `scale(x, y)` / `rotate(angle, ccw=False, rad=False)` / `translate(x, y)` / `skew(x_angle, y_angle, rad=False)` / `mirror(invert_x, invert_y)` / `multiply(other)` | compose an affine transform (each builder returns the composed `PdfMatrix`) for object placement |
| [04] | `PdfMatrix` apply / convert | `on_point(x, y)` / `on_rect(left, bottom, right, top)` / `get()` / `to_raw()` / `from_raw(raw)` | apply the transform to a point/rect; bridge to/from the raw `FS_MATRIX` |
| [05] | `PdfSysfontBase.setup` | `PdfSysfontBase().setup(reusable=False)` (read `PdfDefaultTTFMap` / `PdfSysfontBase.SINGLETON`) | install the headless font-substitution handler for deterministic missing-font fallback |
| [06] | `PdfUnspHandler.setup` | `PdfUnspHandler().setup(add_default=True)` (read `PdfUnspHandler.SINGLETON`) | install the unsupported-feature callback (XFA/portfolio/3D) so an unhandled feature is a logged event, not a crash |

## [04]-[INTEGRATION_LAW]

[PDF_PDFIUM]:
- import: `import pypdfium2` at boundary scope only; module-level import is banned by the manifest import policy. PDFium is a process-global library — the `PdfDocument`/`PdfPage`/`PdfTextPage` tree owns a strict parent->child close order, so the rail holds each handle inside one `async_boundary` capsule and closes leaf-first (PDFium closes children when the parent closes, but explicit leaf-first close keeps the buffer lifetime deterministic).
- render-arm position (`document/emit#DOCUMENT` `PDF_RASTER`): `PdfPage.render(scale=, rotation=, crop=, may_draw_forms=, color_scheme=, fill_to_stroke=) -> PdfBitmap` then `to_numpy()` is the CORE-band raster fold — it lands straight onto the runtime band with no Pillow dependency, which is exactly why `PDF_RASTER` is a `Band.CORE` row while AGPL `pymupdf` (`PDF_RENDER`) and the heavier arms sit elsewhere. This is the distinct axis: `pdf_oxide` is the commercial-safe Rust render owner and `pymupdf` the AGPL native owner, but pypdfium2 is the BSD PDFium rasterizer that the Pillow-free runtime band routes through. The `bitmap_maker` selects PDFium-owned (`new_native`) vs host-buffer (`new_foreign`) backing; `PdfColorScheme` overrides path/text fill/stroke; `get_posconv(page)` maps bitmap<->page coordinates for hit-testing rendered output.
- outline axis (`document/emit#DOCUMENT` `PDF_RASTER` harvest): `get_toc(max_depth)` yields `PdfBookmark`s; the arm reads `mark.get_count()` (the signed sub-item count — NOT a `.level` attribute, which `PdfBookmark` does not expose), `mark.get_title()`, and `dest.get_index()` off `mark.get_dest()` into one `OutlineRow` triple. `document/emit#DOCUMENT` `[OUTLINE_VERIFIED]` is correctness-bound to these exact rows — `get_count`/`get_title`/`get_dest`/`PdfDest.get_index` are the verified surface, `.level` is the phantom that page must never reintroduce.
- extraction axis (`document/lens#LENS` permissive lane): `PdfTextPage` with `get_text_range`/`get_text_bounded`/`search` is the text surface; `count_chars`/`get_charbox`/`get_rect`/`get_index`/`get_textobj` recover per-character geometry + the owning text object for layout-faithful reconstruction → `RunNode` leaves. This is the BSD char-geometry alternative to pymupdf's `get_text` where the AGPL path is barred and to pdf_oxide's Rust `extract_chars` on the permissive lane.
- object + matrix axis (`document/egress#FINISH`): `PdfPage.get_objects(filter=, max_depth=)` enumerates content objects; `insert_obj`/`remove_obj` author + drop, `gen_content()` regenerates the stream after edits. Object placement composes the `PdfMatrix` algebra — `PdfMatrix.scale(sx, sy).rotate(deg).translate(dx, dy)` builds the CTM, `obj.set_matrix(m)`/`obj.transform(m)` applies it, `obj.get_quad_points()` recovers the rotated bounds — never a hand-rolled 2x3 affine where `PdfMatrix` owns the algebra. Page-as-XObject stamping is `page_as_xobject(index, dest)` → `xobj.as_pageobject()` → `insert_obj`.
- image axis: `PdfImage.new(pdf)` + `set_bitmap(bitmap, pages=)` or `load_jpeg(source, inline=)` author embedded rasters; `extract(dest)`/`get_bitmap(render=)`/`get_data(decode_simple=)`/`get_px_size()`/`get_filters()` inspect + recover them — `load_jpeg` embeds JPEG bytes directly (no re-encode), `SIMPLE_FILTERS` names the inline-decodable codecs, never a raw `raw.FPDFImageObj_*` call where the helper wraps it.
- font axis: `PdfFont.load_standard(pdf, name)` (from the 14 `STANDARD_FONTS`) loads a base font for inserted text; `PdfSysfontBase().setup()` + `PdfDefaultTTFMap` install the headless substitution handler so a missing embedded font resolves to a deterministic system TTF instead of a blank glyph — load this once at boundary setup for reproducible headless rendering, alongside `PdfUnspHandler().setup()` for unsupported-feature events.
- form axis: `init_forms(config=)` enables the AcroForm/XFA environment, `render(may_draw_forms=True)` draws field appearances into the raster, `get_formtype()` discriminates AcroForm vs XFA, `close_forms()` tears the environment down before document close.
- escape hatch: `pypdfium2.raw` (the in-package ctypes module) is the full PDFium C API; reach for it only for surfaces the helpers do not wrap (signature dictionaries, JavaScript actions, page-object marks), and re-internalize any reused `FPDF_*` sequence into a local owner. The `raw` constant families (`FPDFBitmap_*` formats, `FPDF_ANNOT_*` subtypes/flags, `FPDF_TEXTRENDERMODE_*`, `FPDF_RENDER_*` flags, `FPDF_*` save flags) are the typed bit sources for the helper `flags=`/`format=` parameters.
- evidence: each render captures page index, scale, rotation, color scheme, form-draw flag, fill-to-stroke, bitmap format, and output dimensions; each raster fold captures the `OutlineRow` count + per-bookmark title/count/dest as the `PDF_RASTER` `EmitFact` pdf receipt.
- license posture: BSD-3-Clause helpers over an Apache-2.0/BSD PDFium binary — permissive, so the render arm is free of the AGPL `pymupdf` obligation. But the permissiveness is shared with `pdf_oxide`/`pypdf`; pypdfium2's claim to the slot is the Pillow-free CORE-band footing + the native outline harvest, not the license alone. `pdf_oxide` is the commercial-safe categorical-best render/extract/redact/separation owner — pypdfium2 does NOT contest the separations/PDF-X/signing rows it lacks.
- boundary: pypdfium2 owns the PDFium CORE-band render + outline-harvest + char-geometry-extract + page-object/image/font-edit path; commercial-safe Rust render/extract/redact/separations route to `pdf_oxide`; native AGPL render/scrub route to `pymupdf`; pure-Python structural assembly to `pypdf`; AES-256-R6 to `pikepdf`; ruled-table geometry to `pdfplumber` (which rasterizes through pypdfium2 for debug); OCR-to-PDF/A to `ocrmypdf` (which depends on pypdfium2 as its rasterizer); live UI stays outside this package.

## [05]-[STACK_INTEGRATION]

[STACK_LAW]:
- universal `expression` tier (`libs/python/.api/expression.md`): `PdfiumError` (and `PdfiumWarning`) map at the boundary to `Result[PdfReceipt, PdfError]` — a PDFium call returning an error code raises `PdfiumError`, which the `async_boundary` capsule converts to the runtime `BoundaryFault` and the owner lifts to an `Error` arm; a `PdfiumWarning` (recoverable, e.g. a partially-loaded page) stays inside the `Ok` arm as a receipt note. `get_text_range`/`get_text_bounded` are exception-light (an undecodable span yields the `errors="ignore"` substitution string), so empty/partial text is `Ok` data, never a failure — exactly the `document/emit#DOCUMENT` pattern where the provider raise becomes `BoundaryFault` but missing content is data.
- universal `numpy` tier (`libs/python/.api/numpy.md`): `PdfBitmap.to_numpy()` returns a `(height, width, channels)` `uint8` array viewing the PDFium buffer (channel layout per `.format`: BGR/BGRA/BGRx/Gray, byte-order per `.rev_byteorder`) — the CORE-band raster fold stacks per-page arrays into one `numpy` frame stack the `graphic/raster/io#RASTER` owner consumes without re-decoding. The array is a view over the live `.buffer`; the rail copies (or keeps the `PdfBitmap` alive) before the page closes, since closing the page frees the backing buffer.
- universal `anyio` tier (`libs/python/.api/anyio.md`): PDFium render/extract is GIL-bound C work, so a multi-page raster or extract fans across `anyio.to_thread.run_sync` under the shared `_OFFLOAD`/`_THREAD_GATE` `CapacityLimiter` — never inline on the event loop. But PDFium's per-document handle is NOT thread-safe across pages concurrently: the rail keeps one document's page work serialized within a worker (one page rendered at a time per `PdfDocument`) and parallelizes across distinct documents, mirroring how `document/emit#DOCUMENT` crosses its CORE-band arms off the scheduler.
- universal `structlog`/OpenTelemetry tier (`libs/python/.api/structlog.md`, `libs/python/.api/opentelemetry.md`): each render/extract is wrapped in the `async_boundary` span the consumer opens for every emit/lens op; the span carries page index + scale + output dims, and a `PdfiumWarning` is a structured span event rather than a swallowed condition. PDFium has no Python-side logger to bridge (unlike `pdf_oxide`'s `set_log_level`) — its diagnostics surface only as `PdfiumError`/`PdfiumWarning`, so the boundary is the sole observability seam.
- universal `msgspec`/`pydantic` tier (`libs/python/.api/msgspec.md`, `libs/python/.api/pydantic.md`): the `get_charbox`/`get_rect` geometry tuples and `OutlineRow` triples are admitted at the boundary into the `msgspec`-discriminated `RunNode`/`StructureNode`/`OutlineRow` models once; never forward a live `PdfTextPage`/`PdfBookmark` handle into the interior (its validity is tied to the open document). `@beartype` the boundary functions so a wrong-shaped `crop`/`color_scheme`/`PdfMatrix` is caught before it reaches the ctypes layer.
- `pillow` seam (optional, off the CORE band): `PdfBitmap.to_pil()`/`from_pil(img)` bridge to the `pillow` artifacts owner for ICC/format-convert/downscale on the WORKER band only — the default `PDF_RASTER` path stays `to_numpy` to keep the CORE band Pillow-free, and the rail reaches `to_pil` solely where a downstream Pillow operation (ICC transform, format encode) is genuinely required.
- pdf-rail STACK: pypdfium2 is the render-and-inspect node — `pypdf`/`pdf_oxide` assemble + author the document, `pypdfium2` renders a page to a numpy raster (CORE band, Pillow-free) and harvests the outline, `pdfplumber` extracts ruled tables (itself rasterizing through pypdfium2), `ocrmypdf` adds the OCR text layer (itself depending on pypdfium2 as rasterizer), `pikepdf` re-encrypts at AES-256-R6, and `pyhanko`/`pdf_oxide` sign; pypdfium2 owns the render + outline + char-geometry + object-edit stage between them, meeting each sibling at PDF bytes or an in-memory `PdfBitmap`/numpy array.

## [06]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pypdfium2`
- License: `BSD-3-Clause` over an Apache-2.0/BSD PDFium binary — permissive; the Pillow-free CORE-band PDFium render arm, distinct from `pdf_oxide` (commercial-safe Rust render/extract/redact/separations) and `pymupdf` (AGPL native render) on the band-and-engine axis, NOT a competing commercial-safe owner
- Owns: PDFium document open/build/save, CORE-band page rasterization to bitmap/PIL/NumPy (scale/rotation/crop/color-scheme/form-aware/fill-to-stroke, native+foreign allocators), native outline harvest (`get_toc` → `PdfBookmark`/`PdfDest` → `OutlineRow`), layout-faithful text extraction/search with per-char geometry, page-object + image inspection/authoring (path/image/text, JPEG embed, simple-filter decode), the `PdfMatrix` affine algebra for object placement, embedded + standard fonts, the full PDF box family (media/crop/bleed/trim/art), attachment read/write with parameter dict, AcroForm/XFA forms, page-as-XObject reuse, headless font-substitution + unsupported-feature handlers, and a raw ctypes escape hatch
- Accept: the BSD CORE-band raster + outline harvest feeding `document/emit#DOCUMENT` `PDF_RASTER`; char-geometry extraction feeding `document/lens#LENS` `RunNode` on the permissive lane; render/numpy → `graphic/raster/io#RASTER`; object + matrix edit + page-as-XObject stamp → `document/egress#FINISH`; the optional `to_pil` bridge to `graphic/raster/io#RASTER` only on the WORKER band where Pillow is required
- Reject: wrapper-renames of `render`/`get_textpage`/`get_toc`; a second raster type where `PdfBitmap` already bridges PIL/NumPy; a phantom `save` callback param (`save` is `(dest, version=, flags=)`); a `mark.level` outline read (`PdfBookmark` exposes `get_count`/`get_title`/`get_dest`, never `.level`); a hand-rolled 2x3 affine where `PdfMatrix` owns the algebra; commercial-safe separations/PDF-X/PAdES rows where `pdf_oxide` owns them; native structural scrub where `pymupdf` owns it; pure-Python assembly where `pypdf` owns it; a `raw.FPDF_*` call where a helper already wraps it; forwarding a live `PdfTextPage`/`PdfBookmark` handle into the interior past the document's lifetime; eagerly reaching `to_pil` on the CORE band where `to_numpy` keeps it Pillow-free; identity minting the runtime owns
