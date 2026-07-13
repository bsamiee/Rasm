# [PY_ARTIFACTS_API_PYPDF]

`pypdf` supplies the pure-Python PDF read/write surface for the artifacts pdf rail: a reader root, a writer root, a page object, the `pypdf.generic` PDF object model, and a fluent transformation algebra that drive merge, split, rotate, scale, text/image extraction, AcroForm read/fill, attachment and outline authoring, object pruning, encryption, and metadata edits with no native runtime. The package owner composes `PdfReader`, `PdfWriter`, `PageObject`, `Transformation`, and the `generic`/`annotations` model into the pdf owner; it never re-implements the PDF object parser, the affine CTM, the `ObjectDeletionFlag` pruner, or the encryption codecs pypdf already owns. The catalog drives the dense pdf rail where pypdf is the zero-dependency structural editor (BSD, fine for distributed closed services where AGPL pymupdf is barred), pymupdf/pypdfium2 do rendering, `pikepdf` owns AES-256-R6, and `ocrmypdf` owns OCR-to-PDF/A.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pypdf`
- package: `pypdf`
- import: `pypdf`
- owner: `artifacts`
- rail: pdf
- installed: `6.14.2`
- extras: image extraction needs `pypdf[image]` (Pillow); AES needs `pypdf[crypto]` (`cryptography`, or `pycryptodome` legacy); both resolve through the manifest, never a per-package pin
- entry points: none (library only)
- capability: pure-Python PDF read/write, clone-then-edit and incremental write, merge/append/split/reorder, the `merge_*` page-overlay/transform family, rotate/scale, layout-aware text extraction with operator visitors, image extraction, AcroForm field read/fill/flatten, outline + named-destination + annotation + attachment authoring, `ObjectDeletionFlag` object pruning, content-stream compression, RC4/AES encryption + permission flags, viewer-preference and XMP/docinfo metadata editing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document roots and geometry
- rail: pdf

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE] | [CAPABILITY]                                                                              |
| :-----: | :-------------------- | :------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `PdfReader`           | reader root    | open/decrypt/read ingest; pages, metadata, outline, XMP, fields, attachments              |
|  [02]   | `PdfWriter`           | writer root    | build/clone/incremental emit; append, encrypt, form-fill, authoring, pruning              |
|  [03]   | `PageObject`          | page unit      | one page; text/image extract, transform, `merge_*` family, rotate, scale, boxes, compress |
|  [04]   | `Transformation`      | affine algebra | fluent CTM; `translate`/`scale`/`rotate`/`transform` compose, `apply_on` maps coords      |
|  [05]   | `PageRange`           | page selector  | a parsed slice expression over a page index space                                         |
|  [06]   | `PaperSize`           | size table     | named physical page dimensions                                                            |
|  [07]   | `DocumentInformation` | metadata view  | the document info dictionary projection                                                   |

[PUBLIC_TYPE_SCOPE]: `pypdf.generic` object model and `pypdf.annotations`
- rail: pdf

`pypdf.generic` is the PDF object algebra every editing op composes; `Fit` is the destination-zoom value object for outline/link authoring; `pypdf.annotations` is the typed annotation constructor family (`Link`/`Text`/`FreeText`/`Highlight`/`Line`/`Rectangle`/`Ellipse`/`Polygon`/`PolyLine`/`Popup`) fed to `PdfWriter.add_annotation`.

| [INDEX] | [SYMBOL]                                     | [PACKAGE_ROLE]    | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------- | :---------------- | :----------------------------------------------------------- |
|  [01]   | `generic.DictionaryObject` / `ArrayObject`   | container nodes   | PDF dict/array primitives for object-tree edits              |
|  [02]   | `generic.NameObject` / `NumberObject`        | scalar nodes      | PDF name/number leaves                                       |
|  [03]   | `generic.TextStringObject` / `BooleanObject` | scalar nodes      | PDF string/bool leaves                                       |
|  [04]   | `generic.IndirectObject`                     | reference         | a resolvable cross-reference into the object table           |
|  [05]   | `generic.StreamObject` / `ContentStream`     | stream nodes      | content/data stream objects with filter decode               |
|  [06]   | `generic.Fit`                                | dest zoom         | dest fit mode (`/Fit`/`/XYZ`/`/FitH`) for outline/link dests |
|  [07]   | `annotations.*`                              | typed annotations | typed annotation constructors for `PdfWriter.add_annotation` |

[PUBLIC_TYPE_SCOPE]: enums and faults
- rail: pdf

`errors` is the typed failure rail; the `errors.PdfReadError`/`PdfStreamError`/`DependencyError` (image/crypto extra missing) rows are the principal faults. `ObjectDeletionFlag` composes by `|` over `NONE`/`TEXT`/`LINKS`/`ATTACHMENTS`/`OBJECTS_3D`/`ALL_ANNOTATIONS`/`XOBJECT_IMAGES`/`INLINE_IMAGES`/`DRAWING_IMAGES`/`IMAGES` (`IMAGES` OR's the three image kinds). `extract_text` is exception-light (returns `""` on undecodable spans).

| [INDEX] | [SYMBOL]                                              | [PACKAGE_ROLE]  | [CAPABILITY]                                               |
| :-----: | :---------------------------------------------------- | :-------------- | :--------------------------------------------------------- |
|  [01]   | `ImageType`                                           | image enum      | `XOBJECT_IMAGES`/`INLINE_IMAGES`/`DRAWING_IMAGES` filter   |
|  [02]   | `PasswordType`                                        | auth enum       | `NOT_DECRYPTED`/`OWNER_PASSWORD`/`USER_PASSWORD` result    |
|  [03]   | `ObjectDeletionFlag`                                  | cleanup flag    | object-removal `IntFlag`; values in the lead               |
|  [04]   | `constants.UserAccessPermissions`                     | permission flag | `IntFlag` permission bitmask for `PdfWriter.encrypt`       |
|  [05]   | `PdfReadError` / `PdfStreamError` / `DependencyError` | fault rail      | parse/stream + missing-extra (`[image]`/`[crypto]`) faults |

`UserAccessPermissions` members: `PRINT`, `MODIFY`, `EXTRACT`, `ADD_OR_MODIFY`, `FILL_FORM_FIELDS`, `EXTRACT_TEXT_AND_GRAPHICS`, `ASSEMBLE_DOC`, `PRINT_TO_REPRESENTATION` (the `IntFlag` composes by `|`; reserved bits `R1`/`R2`/`R7`/`R8`/`R13`-`R32` round out the 32-bit field).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read, write, and clone construction
- rail: pdf

Constructor rows carry stream/path input, password, strictness, cloning, incremental-edit, and recovery-limit policy. `PdfWriter(clone_from=)` is the clone-then-edit modality; `PdfWriter(reader, incremental=True)` is the append-only incremental modality (preserves signatures).
- call: `PdfReader(stream, strict=False, password=None, *, root_object_recovery_limit=10000)`
- call: `PdfWriter(fileobj="", clone_from=None, incremental=False, full=False, strict=False, *, incremental_clone_object_count_limit=500000, incremental_clone_object_id_limit=1000000)`
- call: `decrypt(password: str | bytes) -> PasswordType`

| [INDEX] | [SURFACE]                                            | [CALL_SHAPE]                           | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------- | :------------------------------------- | :------------------------------------ |
|  [01]   | `PdfReader`                                          | `PdfReader(stream, …)`                 | open and decrypt an existing PDF      |
|  [02]   | `PdfWriter`                                          | `PdfWriter(fileobj="", …)`             | build/clone/incremental-edit a writer |
|  [03]   | `PdfReader.decrypt`                                  | `decrypt(password) -> PasswordType`    | decrypt; discriminates owner/user     |
|  [04]   | `PdfReader.is_encrypted` / `decode_permissions`      | property / `decode_permissions()`      | encryption probe + permission readout |
|  [05]   | `PdfReader.metadata` / `xmp_metadata`                | `DocumentInformation`/`XmpInformation` | docinfo and XMP views                 |
|  [06]   | `PdfReader.pages` / `outline` / `named_destinations` | indexable/iterable props               | pages, outline tree, named dests      |
|  [07]   | `PdfReader.threads` / `attachments`                  | iterable props                         | articles, embedded attachments        |
|  [08]   | `PdfReader.get_fields` / `get_form_text_fields`      | `get_fields() -> dict[str,Field]`      | AcroForm field tree / text-field map  |
|  [09]   | `PdfReader.viewer_preferences`                       | property                               | viewer preferences                    |
|  [10]   | `PdfReader.page_layout` / `page_mode`                | properties                             | layout / mode view-state              |

[ENTRYPOINT_SCOPE]: page assembly, merge, transform, and extraction
- rail: pdf

Assembly rows share source-document, page-range, metadata, encryption, stream-target, text-mode, visitor, and transform policy. The `merge_*` family is the page-overlay/composite arm; `add_transformation` applies a raw CTM.
- call: `append(fileobj, outline_item=None, pages=None, import_outline=True, excluded_fields=None)`; `merge(position, fileobj, outline_item=None, pages=None, import_outline=True, excluded_fields=())`
- call: `encrypt(user_password, owner_password=None, use_128bit=True, permissions_flag=UserAccessPermissions.all(), *, algorithm=None)` — `algorithm` in `RC4-40`/`RC4-128`/`AES-128`/`AES-256-R5`/`AES-256`, keyword-only; `permissions_flag` positional-or-keyword
- call: `extract_text(*, orientations=(0,90,180,270), space_width=200.0, extraction_mode="plain"|"layout", visitor_operand_before=None, visitor_operand_after=None, visitor_text=None)`
- call: `Transformation(ctm=(1,0,0,1,0,0))` then `.translate(tx,ty)` / `.scale(sx,sy)` / `.rotate(deg)` / `.transform(other)`
- call: `merge_scaled_page` / `merge_rotated_page` / `merge_translated_page` are the built-in-transform siblings of `merge_transformed_page` (source page + transform args)

| [INDEX] | [SURFACE]                                     | [CALL_SHAPE]                            | [CAPABILITY]                                  |
| :-----: | :-------------------------------------------- | :-------------------------------------- | :-------------------------------------------- |
|  [01]   | `PdfWriter.add_page`                          | `add_page(page, excluded_keys=())`      | append one page                               |
|  [02]   | `PdfWriter.insert_page`                       | page + index                            | insert a page at an index                     |
|  [03]   | `PdfWriter.append`                            | `append(fileobj, …)`                    | append source pages + outline                 |
|  [04]   | `PdfWriter.merge`                             | `merge(position, fileobj, …)`           | splice source pages at a position             |
|  [05]   | `PdfWriter.clone_document_from_reader`        | reader source                           | full clone into the writer                    |
|  [06]   | `PdfWriter.add_blank_page`                    | optional width/height (+ index)         | append a blank page                           |
|  [07]   | `PdfWriter.insert_blank_page`                 | optional width/height + index           | insert a blank page                           |
|  [08]   | `PdfWriter.encrypt`                           | `encrypt(user_password, …)`             | RC4/AES encrypt (algorithms in the call note) |
|  [09]   | `PdfWriter.add_metadata` / `xmp_metadata`     | info dict / `XmpInformation` setter     | set document info / XMP                       |
|  [10]   | `PdfWriter.write`                             | `write(stream) -> tuple[bool, IO]`      | serialize to a path/stream                    |
|  [11]   | `PageObject.extract_text`                     | `extract_text(*, extraction_mode=…)`    | text; `layout` mode + operator/text visitors  |
|  [12]   | `PageObject.images`                           | `images -> list[ImageFile]`             | embedded images (needs `[image]` extra)       |
|  [13]   | `PageObject.add_transformation`               | `add_transformation(ctm, expand=False)` | apply an affine CTM                           |
|  [14]   | `PageObject.merge_page`                       | `merge_page(page2, over=True)`          | overlay a page (watermark/stamp)              |
|  [15]   | `PageObject.merge_transformed_page`           | source page + transform args            | built-in-transform overlay (see call note)    |
|  [16]   | `PageObject.rotate` / `scale`                 | angle / factors                         | rotate / scale a page                         |
|  [17]   | `PageObject.scale_by` / `scale_to`            | factors / target size                   | scale a page by factor or to size             |
|  [18]   | `PageObject.compress_content_streams`         | `compress_content_streams(level=-1)`    | FlateDecode-compress the content stream       |
|  [19]   | `PageObject.mediabox` / `cropbox` / `trimbox` | `RectangleObject` properties            | read/set three of the five page boxes         |
|  [20]   | `PageObject.bleedbox` / `artbox`              | `RectangleObject` properties            | read/set the bleed and art boxes              |
|  [21]   | `Transformation`                              | `Transformation(ctm=…)`                 | fluent CTM; `.matrix` reads the tuple         |

[ENTRYPOINT_SCOPE]: form fill, outline/destination/annotation authoring, attachments, and object pruning
- rail: pdf

The writer owns the full document-authoring surface beyond page assembly: AcroForm fill with optional flatten, outline + named-destination + typed-annotation authoring, attachment embedding, and `ObjectDeletionFlag`-driven object removal (the structural counterpart of pymupdf's `scrub`).
- call: `update_page_form_field_values(page: PageObject | list[PageObject] | None, fields: Mapping[str, str | list[str] | tuple[str,str,float]], flags=FieldDictionaryAttributes.FfBits(0), auto_regenerate=True, flatten=False)`
- call: `add_outline_item(title, page_number, parent=None, before=None, color=None, bold=False, italic=False, fit=Fit.fit(), is_open=True) -> IndirectObject`
- call: `compress_identical_objects(*, remove_duplicates=True, remove_unreferenced=True)` — both flags keyword-only
- call: `remove_objects_from_page(page, to_delete: ObjectDeletionFlag | Iterable, text_filters=None)`; `add_attachment(filename, data: str | bytes) -> EmbeddedFile`
- call: table surfaces are `PdfWriter` members

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]                                     | [CAPABILITY]                                  |
| :-----: | :------------------------------------ | :----------------------------------------------- | :-------------------------------------------- |
|  [01]   | `update_page_form_field_values`       | `update_page_form_field_values(page, fields)`    | fill fields; `flatten` bakes in, `page=None`  |
|  [02]   | `set_need_appearances_writer`         | bool                                             | force viewer field-appearance regeneration    |
|  [03]   | `add_outline_item`                    | `add_outline_item(title, page_number, …)`        | author an outline (bookmark) node             |
|  [04]   | `add_outline_item_destination`        | destination                                      | author from a destination                     |
|  [05]   | `find_outline_item`                   | search                                           | locate an existing outline item               |
|  [06]   | `add_named_destination`               | title + page number                              | author a named destination                    |
|  [07]   | `add_annotation`                      | `add_annotation(page_number, annotation)`        | author a typed `pypdf.annotations` annotation |
|  [08]   | `add_uri` / `add_js`                  | page + rect + uri / js string                    | author a URI link / document JavaScript       |
|  [09]   | `add_attachment`                      | `add_attachment(filename, data) -> EmbeddedFile` | embed a file attachment                       |
|  [10]   | `remove_objects_from_page`            | `remove_objects_from_page(page, to_delete, …)`   | prune text/links/images/annotations           |
|  [11]   | `remove_images` / `remove_text`       | flag / subtype selectors                         | bulk-remove images/text document-wide         |
|  [12]   | `remove_links` / `remove_annotations` | flag / subtype selectors                         | bulk-remove links/annotations document-wide   |
|  [13]   | `compress_identical_objects`          | `compress_identical_objects(*, …)`               | dedup/GC the object table                     |
|  [14]   | `create_viewer_preferences`           | preference object                                | author viewer preferences                     |
|  [15]   | `set_page_layout` / `page_mode`       | layout / mode                                    | author viewer view-state                      |
|  [16]   | `generate_file_identifiers`           | no-arg                                           | (re)generate the `/ID` file identifiers       |

## [04]-[IMPLEMENTATION_LAW]

[PDF_PURE]:
- import: `import pypdf` at boundary scope only; module-level import is banned by the manifest import policy.
- reader/writer split: `PdfReader` owns ingestion and `PdfWriter` owns emission; `PdfWriter(clone_from=)` (clone-then-edit) and `PdfWriter(reader, incremental=True)` (append-only, signature-preserving) are the two write modalities, a row on the writer axis, never parallel writer types.
- page axis: `PageObject` is the single page owner; the `merge_*` family is the page-overlay arm and `add_transformation`/`rotate`/`scale` compose the fluent `Transformation` (`translate`/`scale`/`rotate`/`transform` chain), never a per-operation page subtype.
- extraction axis: `extract_text(extraction_mode="layout", visitor_text=, visitor_operand_before=)` plus `images` is the extraction surface; the `layout` mode and operator visitors capture positioned glyph runs, the result feeds the document owner, never a re-minted text model. pymupdf/pypdfium2 own rendered-text and table extraction where layout fidelity needs a render.
- object-model axis: `pypdf.generic` is the editing algebra — `DictionaryObject`/`ArrayObject`/`NameObject`/`IndirectObject`/`StreamObject`; deep edits compose these directly rather than mutating raw dicts; `Fit` is the dest-zoom value object for outline/link authoring.
- form axis: `PdfReader.get_fields` reads the AcroForm tree and `PdfWriter.update_page_form_field_values(..., flatten=)` is the single fill/flatten entry; `set_need_appearances_writer` is the viewer-regeneration flag, never a hand-built `/AP` appearance stream.
- pruning axis: `PdfWriter.remove_objects_from_page`/`remove_images`/`remove_text`/`remove_links`/`remove_annotations` discriminate on the `ObjectDeletionFlag` `IntFlag` (composed by `|`); `compress_identical_objects` dedups/GCs the object table — this is the pure-Python structural counterpart to pymupdf's native `scrub`, never a hand walk of the xref table.
- encryption axis: `PdfWriter.encrypt` is the single in-process encrypt surface; `algorithm` (`RC4-40`/`RC4-128`/`AES-128`/`AES-256-R5`/`AES-256`) selects strength (needs the `[crypto]` extra) and `permissions_flag` is a `UserAccessPermissions` `IntFlag` composed by `|`, never scattered boolean print/extract/modify knobs; `permissions_flag` is positional-or-keyword and `algorithm` is keyword-only; the harder qpdf-native R6/linearization strength routes to `pikepdf`.
- evidence: each operation captures source page count, output page count, encryption state (`PasswordType`), permission flags, filled-field count, pruned-object count, and output byte length as a pdf receipt.
- license posture: BSD-3-Clause, no native link — pypdf is the structural-editing arm admissible inside a closed/distributed network service where AGPL `pymupdf` rendering is barred; pair pypdf editing with `pypdfium2` render to keep the whole pipeline permissive.
- boundary: pypdf owns pure-Python structural read/write, merge/transform, form fill, outline/annotation/attachment authoring, object pruning, and encryption; rasterization and rendered-text extraction route to `pymupdf`/`pypdfium2`; AES-256-R6 and content-stream tokenization route to `pikepdf`; OCR-to-PDF/A routes to `ocrmypdf`; live UI stays outside this package.

[STACK_INTEGRATION]:
- universal `expression` tier (`libs/python/.api/expression.md`): the `errors` subtree (`PdfReadError`/`PdfStreamError`/`DependencyError`) maps at the boundary to `Result[PdfReceipt, PdfError]` — `DependencyError` is the missing-`[image]`/`[crypto]`-extra arm, `PdfReadError`/`PdfStreamError` the malformed-input arm; `decrypt` returns a `PasswordType` that the owner lifts to an `Ok(OWNER|USER)` / `Error(NOT_DECRYPTED)` split rather than a boolean. `extract_text` is exception-light (returns `""` on undecodable spans), so it stays inside the `Ok` arm and the empty string is data, not a failure.
- universal `numpy`/`msgspec` tier: the `visitor_text(text, cm, tm, font_dict, font_size)` and `visitor_operand_before/after` callbacks fed to `extract_text(extraction_mode="layout", ...)` are the positioned-glyph capture hook — each call appends `(text, x, y, font, size)` to a canonical buffer the owner folds into a `numpy` coordinate array or an `msgspec.Struct` run, so layout-aware extraction is one streaming visitor, never a re-minted text model or a regex over raw operators.
- universal `anyio` tier (`libs/python/.api/anyio.md`): a per-page op (`extract_text`/`images`/`merge_page`/`remove_objects_from_page`) over a large `reader.pages` is CPU-bound pure Python, so a bulk extract/prune over a multi-hundred-page document fans across `anyio.to_thread.run_sync` workers under a `CapacityLimiter`; the `PdfWriter` mutation stays single-owner (the object table is not thread-safe), only the read-side page work parallelizes.
- `[image]`/`pillow` seam: `PageObject.images` yields `ImageFile` whose `.image` is a `PIL.Image` and `.data` the raw codec bytes — the extracted image hands directly to the `pillow` artifacts owner (downscale/ICC/format-convert) or to `av.VideoFrame.from_image` for a page-to-frame poster, never a hand-decoded XObject stream.
- document-rail STACK: `pypdf` is the structural spine — `PdfWriter.append`/`merge` assembles the multi-source document, `vl_convert`/`typst`/`reportlab`/`weasyprint`-emitted PDF pages are appended source files, `pymupdf`/`pypdfium2` render a page to a raster when layout fidelity needs a render, `pikepdf` re-encrypts at AES-256-R6 / linearizes for web, `ocrmypdf` adds the OCR text layer, and `pyhanko` digitally signs (PAdES) the finished bytes; each sibling owns one stage and `pypdf` owns the page-tree edits between them.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pypdf`
- Owns: pure-Python PDF read/write, clone/incremental edit, merge/append/split/reorder, the `merge_*` overlay family, rotate/scale, layout-aware text + image extraction, AcroForm read/fill/flatten, outline/named-destination/annotation/attachment authoring, `ObjectDeletionFlag` object pruning, content-stream compression, RC4/AES encryption + permission flags, viewer-preference/XMP/docinfo metadata editing
- Accept: structural PDF assembly, form fill, object pruning, and extraction feeding the document/PDF owner; the BSD permissive editing arm for closed/distributed services
- Reject: wrapper-renames of `add_page`/`extract_text`/`update_page_form_field_values`; a native rasterizer reimplementation where `pymupdf`/`pypdfium2` render; AES-256-R6 where `pikepdf` owns it; a hand walk of the xref table where `remove_objects_from_page`/`compress_identical_objects` prune; scattered boolean permission knobs where the `UserAccessPermissions` `IntFlag` composes; identity minting the runtime owns
