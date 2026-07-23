# [PY_ARTIFACTS_API_PYPDF]

`pypdf` owns the pure-Python PDF read/write surface for the artifacts pdf rail: `PdfReader`/`PdfWriter` roots, `PageObject`, the `pypdf.generic` object model, and a fluent `Transformation` algebra driving merge, overlay/transform, text and image extraction, AcroForm fill, outline/annotation/attachment authoring, `ObjectDeletionFlag` pruning, and RC4/AES encryption — no native runtime. BSD-3-Clause with no native link, it is the permissive structural editor admissible where AGPL `pymupdf` is barred; rendering routes to `pymupdf`/`pypdfium2`, AES-256-R6 to `pikepdf`, OCR-to-PDF/A to `ocrmypdf`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pypdf`
- package: `pypdf` (BSD-3-Clause)
- module: `pypdf`
- namespaces: `pypdf`, `pypdf.generic`, `pypdf.annotations`, `pypdf.errors`, `pypdf.constants`
- rail: pdf — pure-Python structural read/write, merge/transform, layout-aware extraction, form fill, authoring, `ObjectDeletionFlag` pruning, RC4/AES encryption

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document roots and geometry

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                                              |
| :-----: | :-------------------- | :------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `PdfReader`           | class         | open/decrypt/read ingest; pages, metadata, outline, XMP, fields, attachments              |
|  [02]   | `PdfWriter`           | class         | build/clone/incremental emit; append, encrypt, form-fill, authoring, pruning              |
|  [03]   | `PageObject`          | class         | one page; text/image extract, transform, `merge_*` family, rotate, scale, boxes, compress |
|  [04]   | `Transformation`      | value-object  | fluent CTM; `translate`/`scale`/`rotate`/`transform` compose, `apply_on` maps coords      |
|  [05]   | `PageRange`           | class         | a parsed slice expression over a page index space                                         |
|  [06]   | `PaperSize`           | class         | named physical page dimensions                                                            |
|  [07]   | `DocumentInformation` | class         | the document info dictionary projection                                                   |

[PUBLIC_TYPE_SCOPE]: `pypdf.generic` object model and `pypdf.annotations`

`pypdf.generic` is the PDF object algebra every editing op composes; `Fit` is the destination-zoom value object for outline/link authoring.

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `generic.DictionaryObject` / `ArrayObject`   | class         | PDF dict/array primitives for object-tree edits              |
|  [02]   | `generic.NameObject` / `NumberObject`        | class         | PDF name/number leaves                                       |
|  [03]   | `generic.TextStringObject` / `BooleanObject` | class         | PDF string/bool leaves                                       |
|  [04]   | `generic.IndirectObject`                     | class         | a resolvable cross-reference into the object table           |
|  [05]   | `generic.StreamObject` / `ContentStream`     | class         | content/data stream objects with filter decode               |
|  [06]   | `generic.Fit`                                | value-object  | dest fit mode (`/Fit`/`/XYZ`/`/FitH`) for outline/link dests |
|  [07]   | `annotations.*`                              | class         | typed annotation constructors for `PdfWriter.add_annotation` |

- `annotations`: `Link` `Text` `FreeText` `Highlight` `Line` `Rectangle` `Ellipse` `Polygon` `PolyLine` `Popup` — fed to `PdfWriter.add_annotation`.

[PUBLIC_TYPE_SCOPE]: enums and faults

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :---------------------------------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `ImageType`                                           | enum          | `XOBJECT_IMAGES`/`INLINE_IMAGES`/`DRAWING_IMAGES` filter |
|  [02]   | `PasswordType`                                        | enum          | `NOT_DECRYPTED`/`OWNER_PASSWORD`/`USER_PASSWORD` result  |
|  [03]   | `ObjectDeletionFlag`                                  | enum          | object-removal `IntFlag`; members below                  |
|  [04]   | `constants.UserAccessPermissions`                     | enum          | `IntFlag` permission bitmask for `PdfWriter.encrypt`     |
|  [05]   | `PdfReadError` / `PdfStreamError` / `DependencyError` | exception     | parse/stream and missing-extra (`[image]`/`[crypto]`)    |

- `ObjectDeletionFlag`: `NONE` `TEXT` `LINKS` `ATTACHMENTS` `OBJECTS_3D` `ALL_ANNOTATIONS` `XOBJECT_IMAGES` `INLINE_IMAGES` `DRAWING_IMAGES` `IMAGES` — compose by `|`; `IMAGES` OR's the three image kinds.
- `UserAccessPermissions`: `PRINT` `MODIFY` `EXTRACT` `ADD_OR_MODIFY` `FILL_FORM_FIELDS` `EXTRACT_TEXT_AND_GRAPHICS` `ASSEMBLE_DOC` `PRINT_TO_REPRESENTATION` — compose by `|`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read, write, and clone construction

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :-------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `PdfReader(stream, strict, password)`         | ctor     | open and decrypt an existing PDF      |
|  [02]   | `PdfWriter(fileobj, clone_from, incremental)` | ctor     | build/clone/incremental-edit a writer |
|  [03]   | `PdfReader.decrypt(password) -> PasswordType` | instance | decrypt; discriminates owner vs user  |
|  [04]   | `PdfReader.is_encrypted`                      | property | encryption probe                      |
|  [05]   | `PdfReader.decode_permissions()`              | instance | permission readout                    |
|  [06]   | `PdfReader.metadata`                          | property | `DocumentInformation` docinfo view    |
|  [07]   | `PdfReader.xmp_metadata`                      | property | `XmpInformation` XMP view             |
|  [08]   | `PdfReader.pages`                             | property | indexable/iterable pages              |
|  [09]   | `PdfReader.outline`                           | property | outline tree                          |
|  [10]   | `PdfReader.named_destinations`                | property | named destinations                    |
|  [11]   | `PdfReader.threads`                           | property | article threads                       |
|  [12]   | `PdfReader.attachments`                       | property | embedded attachments                  |
|  [13]   | `PdfReader.get_fields() -> dict[str, Field]`  | instance | AcroForm field tree                   |
|  [14]   | `PdfReader.get_form_text_fields()`            | instance | text-field name→value map             |
|  [15]   | `PdfReader.viewer_preferences`                | property | viewer preferences                    |
|  [16]   | `PdfReader.page_layout` / `page_mode`         | property | layout / mode view-state              |

[ENTRYPOINT_SCOPE]: page assembly, merge, transform, and extraction

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `PdfWriter.add_page(page, excluded_keys)`                          | instance | append one page                                 |
|  [02]   | `PdfWriter.insert_page(page, index)`                               | instance | insert a page at an index                       |
|  [03]   | `PdfWriter.append(fileobj, outline_item, pages)`                   | instance | append source pages with outline                |
|  [04]   | `PdfWriter.merge(position, fileobj, outline_item, pages)`          | instance | splice source pages at a position               |
|  [05]   | `PdfWriter.clone_document_from_reader(reader)`                     | instance | full clone into the writer                      |
|  [06]   | `PdfWriter.add_blank_page(width, height)`                          | instance | append a blank page                             |
|  [07]   | `PdfWriter.insert_blank_page(width, height, index)`                | instance | insert a blank page                             |
|  [08]   | `PdfWriter.encrypt(user_password, owner_password, *, algorithm)`   | instance | RC4/AES encrypt (algorithms in the caveat)      |
|  [09]   | `PdfWriter.add_metadata(infos)`                                    | instance | set the document info dict                      |
|  [10]   | `PdfWriter.xmp_metadata`                                           | property | set XMP metadata                                |
|  [11]   | `PdfWriter.write(stream) -> tuple[bool, IO]`                       | instance | serialize to a path or stream                   |
|  [12]   | `PageObject.extract_text(*, extraction_mode, visitor_text) -> str` | instance | text; `layout` mode with operator/text visitors |
|  [13]   | `PageObject.images -> list[ImageFile]`                             | property | embedded images (needs `[image]` extra)         |
|  [14]   | `PageObject.add_transformation(ctm, expand)`                       | instance | apply an affine CTM                             |
|  [15]   | `PageObject.merge_page(page2, over)`                               | instance | overlay a page (watermark/stamp)                |
|  [16]   | `PageObject.merge_transformed_page(page2, ctm)`                    | instance | built-in-transform overlay                      |
|  [17]   | `PageObject.rotate(angle)`                                         | instance | rotate a page                                   |
|  [18]   | `PageObject.scale(sx, sy)`                                         | instance | scale a page                                    |
|  [19]   | `PageObject.scale_by(factor)` / `scale_to(width, height)`          | instance | scale by factor or to a target size             |
|  [20]   | `PageObject.compress_content_streams(level)`                       | instance | FlateDecode-compress the content stream         |
|  [21]   | `Transformation(ctm)`                                              | ctor     | fluent affine CTM value object                  |

- `PdfWriter.encrypt`: `algorithm` (keyword-only) in `RC4-40` `RC4-128` `AES-128` `AES-256-R5` `AES-256` selects strength (needs the `[crypto]` extra); `permissions_flag` is a `UserAccessPermissions` `IntFlag`.
- [22]-[MERGE_TRANSFORMS]: `PageObject.merge_scaled_page` `merge_rotated_page` `merge_translated_page` are the built-in-transform siblings of `merge_transformed_page` (source page with transform args).
- [23]-[TRANSFORMATION_CHAIN]: `Transformation().translate(tx, ty)` `.scale(sx, sy)` `.rotate(deg)` `.transform(other)` compose the CTM; `.matrix` reads the tuple and `.apply_on(pt)` maps coordinates.
- [24]-[PAGE_BOXES]: `PageObject.mediabox` `.cropbox` `.trimbox` `.bleedbox` `.artbox` are the five `RectangleObject` page boxes, read/set.

[ENTRYPOINT_SCOPE]: form fill, outline/destination/annotation authoring, attachments, and object pruning

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `PdfWriter.update_page_form_field_values(page, fields, flatten)`        | instance | fill fields; `flatten` bakes in, `page=None`  |
|  [02]   | `PdfWriter.set_need_appearances_writer(state)`                          | instance | force viewer field-appearance regeneration    |
|  [03]   | `PdfWriter.add_outline_item(title, page_number, fit) -> IndirectObject` | instance | author an outline (bookmark) node             |
|  [04]   | `PdfWriter.add_outline_item_destination(dest)`                          | instance | author an outline from a destination          |
|  [05]   | `PdfWriter.find_outline_item(query)`                                    | instance | locate an existing outline item               |
|  [06]   | `PdfWriter.add_named_destination(title, page_number)`                   | instance | author a named destination                    |
|  [07]   | `PdfWriter.add_annotation(page_number, annotation)`                     | instance | author a typed `pypdf.annotations` annotation |
|  [08]   | `PdfWriter.add_uri(page_number, uri, rect)`                             | instance | author a URI link                             |
|  [09]   | `PdfWriter.add_js(javascript)`                                          | instance | author document JavaScript                    |
|  [10]   | `PdfWriter.add_attachment(filename, data) -> EmbeddedFile`              | instance | embed a file attachment                       |
|  [11]   | `PdfWriter.remove_objects_from_page(page, to_delete, text_filters)`     | instance | prune text/links/images/annotations on a page |
|  [12]   | `PdfWriter.remove_images(to_delete)`                                    | instance | bulk-remove images document-wide              |
|  [13]   | `PdfWriter.remove_text()`                                               | instance | bulk-remove text document-wide                |
|  [14]   | `PdfWriter.remove_links()` / `remove_annotations(subtypes)`             | instance | bulk-remove links / annotations document-wide |
|  [15]   | `PdfWriter.compress_identical_objects()`                                | instance | dedup/GC the object table                     |
|  [16]   | `PdfWriter.create_viewer_preferences()`                                 | instance | author viewer preferences                     |
|  [17]   | `PdfWriter.set_page_layout(layout)` / `page_mode`                       | instance | author viewer view-state                      |
|  [18]   | `PdfWriter.generate_file_identifiers()`                                 | instance | (re)generate the `/ID` file identifiers       |

- `PdfWriter.compress_identical_objects`: `remove_duplicates` and `remove_unreferenced` (default true) select dedup and unreferenced-GC.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `PdfReader` owns ingestion and `PdfWriter` emission; `clone_from=` is clone-then-edit and `(reader, incremental=True)` is append-only signature-preserving — two rows on the writer axis, never parallel writer types.
- `PageObject` is the single page owner; the `merge_*` family overlays and `add_transformation`/`rotate`/`scale` compose the fluent `Transformation` chain, never a per-operation page subtype.
- `extract_text(extraction_mode="layout", visitor_text=, visitor_operand_before=)` with `images` is the extraction surface; layout mode and operator visitors capture positioned glyph runs feeding the document owner, where `pymupdf`/`pypdfium2` own rendered-text and table extraction that needs a render.
- `pypdf.generic` is the editing algebra; deep edits compose `DictionaryObject`/`ArrayObject`/`NameObject`/`IndirectObject`/`StreamObject` directly rather than mutating raw dicts, and `Fit` is the dest-zoom value object for outline/link authoring.
- `PdfReader.get_fields` reads the AcroForm tree and `PdfWriter.update_page_form_field_values(..., flatten=)` is the single fill/flatten entry; `set_need_appearances_writer` forces viewer regeneration, never a hand-built `/AP` appearance stream.
- `remove_objects_from_page`/`remove_images`/`remove_text`/`remove_links`/`remove_annotations` discriminate on the `ObjectDeletionFlag` `IntFlag`, and `compress_identical_objects` dedups/GCs the object table — the pure-Python counterpart to pymupdf's native `scrub`, never a hand walk of the xref table.
- `PdfWriter.encrypt` is the single encrypt surface; `algorithm` selects strength (needs `[crypto]`) and `permissions_flag` is a `UserAccessPermissions` `IntFlag`, never scattered boolean print/extract/modify knobs; qpdf-native R6 and linearization route to `pikepdf`.
- Each op captures source page count, output page count, encryption state (`PasswordType`), permission flags, filled-field count, pruned-object count, and output byte length as a pdf receipt.

[STACKING]:
- `expression`(`libs/python/.api/expression.md`): the `errors` subtree maps at the boundary to `Result[PdfReceipt, PdfError]` — `DependencyError` the missing-`[image]`/`[crypto]`-extra arm, `PdfReadError`/`PdfStreamError` the malformed-input arm; `decrypt` returns `PasswordType` lifted to `Ok(OWNER|USER)`/`Error(NOT_DECRYPTED)`, and `extract_text` returns `""` on undecodable spans so the empty string is `Ok` data.
- `numpy`(`libs/python/.api/numpy.md`)/`msgspec`(`libs/python/.api/msgspec.md`): the `visitor_text(text, cm, tm, font_dict, font_size)` and `visitor_operand_before/after` callbacks on `extract_text(extraction_mode="layout")` append `(text, x, y, font, size)` to a buffer the owner folds into a `numpy` coordinate array or `msgspec.Struct` run — one streaming visitor, never a regex over raw operators.
- `anyio`(`libs/python/.api/anyio.md`): a per-page read op (`extract_text`/`images`/`merge_page`/`remove_objects_from_page`) over a large `reader.pages` fans across `anyio.to_thread.run_sync` under a `CapacityLimiter`; `PdfWriter` mutation stays single-owner, only read-side page work parallelizes.
- `pillow`(`.api/pillow.md`): `PageObject.images` yields `ImageFile` whose `.image` is a `PIL.Image` and `.data` the codec bytes, handed to the pillow owner or `av.VideoFrame.from_image`, never a hand-decoded XObject stream.
- document-rail: `pypdf.append`/`merge` assembles the multi-source document, `vl_convert`/`typst`/`reportlab`/`weasyprint` PDFs enter as appended source files, `pymupdf`/`pypdfium2` render a page to raster, `pikepdf` re-encrypts at AES-256-R6 and linearizes, `ocrmypdf` adds the OCR layer, and `pyhanko` PAdES-signs the finished bytes.
- within-lib: `document/pdf` composes `PdfReader`/`PdfWriter`/`PageObject`/`Transformation`/`generic` into the pdf owner; the extraction visitor feeds the document text model and the `merge_*` family the overlay/watermark op.

[LOCAL_ADMISSION]:
- `import pypdf` at boundary scope only; PDF is the one format, admitted as one document owner under the reader/writer split, never a per-source reader type.

[RAIL_LAW]:
- Package: `pypdf`
- Owns: pure-Python PDF read/write, clone and incremental edit, merge/append/split/reorder, the `merge_*` overlay family, rotate/scale, layout-aware text and image extraction, AcroForm read/fill/flatten, outline/named-destination/annotation/attachment authoring, `ObjectDeletionFlag` object pruning, content-stream compression, and RC4/AES encryption with `UserAccessPermissions` flags
- Accept: structural assembly, form fill, object pruning, and extraction feeding the document/pdf owner; the BSD-permissive editing arm for closed/distributed services
- Reject: wrapper-renames of `add_page`/`extract_text`/`update_page_form_field_values`; a native rasterizer where `pymupdf`/`pypdfium2` render; AES-256-R6 where `pikepdf` owns it; a hand walk of the xref table where `remove_objects_from_page`/`compress_identical_objects` prune; scattered boolean permission knobs where `UserAccessPermissions` composes
