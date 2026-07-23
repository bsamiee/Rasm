# [PY_ARTIFACTS_API_PIKEPDF]

`pikepdf` binds libqpdf through nanobind and owns qpdf-grade PDF structure for the artifacts pdf rail: open/repair, linearization, AES-R6 encryption with granular permissions, page assembly and overlay, content-stream tokenization and authoring, object-model editing, image extraction, XMP/docinfo metadata, and declarative qpdf jobs. It never re-implements the PDF parser, the qpdf object model, or the affine `Matrix` the package already binds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pikepdf`
- package: `pikepdf` (MPL-2.0)
- module: `pikepdf`
- namespaces: `pikepdf`, `pikepdf.models`, `pikepdf.canvas`, `pikepdf.settings`, `pikepdf.sanitize`
- rail: pdf — qpdf-backed structure repair, encryption, content tokenize/author, object-model edit, image extract, qpdf jobs

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, page, and policy types

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                                |
| :-----: | :----------------- | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `Pdf`              | class         | open/new/save, pages, root/trailer/docinfo, encryption, jobs                |
|  [02]   | `Page`             | class         | media/crop/art/bleed/trim boxes, contents, resources, overlay               |
|  [03]   | `Encryption`       | class         | `owner`/`user` passwords, `R` level, `aes`, `allow=Permissions`             |
|  [04]   | `Permissions`      | class         | `accessibility`/`extract`/`modify_*`/`print_highres`/`print_lowres`         |
|  [05]   | `Job`              | class         | run a job-JSON pipeline; exit/warnings/encryption/output receipt            |
|  [06]   | `JobBuilder`       | class         | fluent job-JSON assembler to `.build()` dict                                |
|  [07]   | `AcroForm`         | class         | `add_field`/`fields`/`remove_fields`/`disable_digital_signatures`           |
|  [08]   | `AcroFormField`    | class         | one interactive field; `FormFieldFlag` bit policy                           |
|  [09]   | `Annotation`       | class         | annotation object; `AnnotationFlag` bit policy + appearance source          |
|  [10]   | `Outline`          | class         | `root`/`add` document outline editing                                       |
|  [11]   | `OutlineItem`      | class         | one node; `to`/`destination`/`title`/`is_closed`/`children`                 |
|  [12]   | `AttachedFileSpec` | class         | embedded file specification                                                 |
|  [13]   | `Matrix`           | value-object  | `scaled`/`rotated`/`translated`/`inverse`/`transform`/`as_array`/`identity` |
|  [14]   | `Rectangle`        | value-object  | page-box rectangle with corner/extent accessors                             |
|  [15]   | `PageLocation`     | enum          | typed page-view kind (`XYZ`/`Fit`/`FitH`/…)                                 |

[PUBLIC_TYPE_SCOPE]: object model, tokens, and enums

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :---------------------------------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `Object`                            | class         | base value: `as_dict`/`to_json`/`read_bytes`/`parse`/`unparse`               |
|  [02]   | `Dictionary`                        | class         | name-keyed PDF dictionary                                                    |
|  [03]   | `Array`                             | class         | ordered PDF array; full `list` interface — slice + insert/pop/remove/reverse |
|  [04]   | `Name`                              | class         | `/Name` token; `Name.random()` mints a unique resource name                  |
|  [05]   | `Stream`                            | class         | dictionary plus encoded byte payload                                         |
|  [06]   | `Operator`                          | class         | content-stream operator token                                                |
|  [07]   | `Boolean`/`Integer`/`Real`/`String` | class         | explicit PDF scalar wrappers vs Python-coerced literals                      |
|  [08]   | `ObjectType`                        | enum          | discriminant for `Object` (`dictionary`/`array`/`stream`/`name`/`integer`/…) |
|  [09]   | `ObjectHelper`                      | class         | base of `Page`/`Annotation`/`AcroFormField`; `.obj` to backing `Dictionary`  |
|  [10]   | `Token`                             | class         | lexer token from the content-stream lexer                                    |
|  [11]   | `TokenType`                         | enum          | lexical-kind discriminant of a `Token`                                       |
|  [12]   | `TokenFilter`                       | class         | `handle_token` hook for `Page.add_content_token_filter`                      |
|  [13]   | `ContentStreamInstruction`          | class         | operand+operator pair from `parse_content_stream`                            |
|  [14]   | `ContentStreamInlineImage`          | class         | inline image (`BI…EI`) row from `parse_content_stream`                       |
|  [15]   | `NameTree`/`NumberTree`             | class         | mapping view over a PDF name/number tree                                     |
|  [16]   | `StreamDecodeLevel`                 | enum          | `none`/`generalized`/`specialized`/`all`                                     |
|  [17]   | `ObjectStreamMode`                  | enum          | `disable`/`preserve`/`generate`                                              |
|  [18]   | `AccessMode`                        | enum          | `default`/`stream`/`mmap`/`mmap_only`                                        |

[PUBLIC_TYPE_SCOPE]: model helpers and fault family

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :----------------------------------- | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `models.PdfImage`                    | class         | `as_pil_image`/`extract_to`/`mode`/`colorspace`/`icc`; `MAX_IMAGE_PIXELS` cap |
|  [02]   | `models.PdfMetadata`                 | class         | `load_from_docinfo`/`pdfa_status`/`pdfx_status` mapping                       |
|  [03]   | `canvas.Canvas`                      | class         | `add_font`/`do`/`to_pdf` page-content builder                                 |
|  [04]   | `canvas.ContentStreamBuilder`        | class         | `begin_text`/`show_text`/`set_fill_color`/`cm`/`fill`                         |
|  [05]   | `PdfError`                           | exception     | qpdf operation failure, root of the fault family                              |
|  [06]   | `PasswordError`                      | exception     | wrong open password                                                           |
|  [07]   | `DataDecodingError`                  | exception     | stream filter decode failure                                                  |
|  [08]   | `ForeignObjectError`                 | exception     | object used across `Pdf` owners without `copy_foreign`                        |
|  [09]   | `DeletedObjectError`                 | exception     | access to an object whose owning `Pdf` was closed                             |
|  [10]   | `ReferenceCycleError`                | exception     | refusal to materialize a self-referential object graph                        |
|  [11]   | `DependencyError`                    | exception     | a stream filter needs an unavailable native codec                             |
|  [12]   | `OutlineStructureError`              | exception     | malformed outline tree during `open_outline` edit                             |
|  [13]   | `UnsupportedImageTypeError`          | exception     | `PdfImage` decode refusal by image class                                      |
|  [14]   | `InvalidPdfImageError`               | exception     | malformed embedded image data                                                 |
|  [15]   | `HifiPrintImageNotTranscodableError` | exception     | hi-fi print image not transcodable                                            |
|  [16]   | `JobUsageError`                      | exception     | invalid `Job`/`JobBuilder` configuration                                      |
|  [17]   | `DecompressionBombError`             | exception     | image pixel count exceeds `PdfImage.MAX_IMAGE_PIXELS`                         |
|  [18]   | `DecompressionBombWarning`           | warning       | image pixel count nears the decode cap                                        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open, create, and save

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `pikepdf.open(src, *, password, attempt_recovery, …)` | factory  | open path/stream with mmap + repair       |
|  [02]   | `pikepdf.new()`                                       | factory  | create an empty PDF                       |
|  [03]   | `Pdf.save(target, *, linearize, encryption, …)`       | instance | save with linearization/encryption/objstm |
|  [04]   | `Pdf.close()` / `Pdf.lock`                            | instance | release or guard the document             |
|  [05]   | `Pdf.add_blank_page(*, page_size) -> Page`            | instance | append a blank page                       |
|  [06]   | `Pdf.copy_foreign(obj) -> Object`                     | instance | import an object from another `Pdf`       |
|  [07]   | `Pdf.check_pdf_syntax(progress=None) -> list[str]`    | instance | validate syntax, return warnings          |
|  [08]   | `Pdf.open_metadata(...) -> PdfMetadata`               | instance | context-manager XMP edit                  |
|  [09]   | `Pdf.open_outline(max_depth=15) -> Outline`           | instance | context-manager outline edit              |
|  [10]   | `Pdf.remove_unreferenced_resources()`                 | instance | garbage-collect unused objects            |
|  [11]   | `pikepdf.make_page_destination(...)`                  | factory  | mint an outline/link destination          |

[ENTRYPOINT_SCOPE]: page assembly and resources

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Pdf.pages`                                         | property | index/insert/delete/reorder pages         |
|  [02]   | `Page.add_overlay(other, rect=None, …) -> Name`     | instance | stamp another page atop this one          |
|  [03]   | `Page.add_underlay(other, rect=None, …) -> Name`    | instance | place a page beneath this one             |
|  [04]   | `Page.add_resource(res, res_type, …) -> Name`       | instance | register a resource under `/Resources`    |
|  [05]   | `Page.as_form_xobject(handle_transformations=True)` | instance | convert a page to a form XObject          |
|  [06]   | `Page.contents_add(stream, *, prepend=False)`       | instance | append/merge streams; `contents_coalesce` |

- [07]-[PAGE_MAPS_BOXES]: `Page.get_images(recursive=True)` (the name→image map that also reaches images nested in form XObjects), `Page.resources`, and the `mediabox`/`cropbox`/`trimbox`/`bleedbox`/`artbox` `Rectangle` boxes carry the per-page image inventory, resource dict, and box geometry.

[ENTRYPOINT_SCOPE]: content-stream tokenize and edit

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `Page.add_content_token_filter(TokenFilter)`       | instance | streaming operand/operator rewrite          |
|  [02]   | `Page.get_filtered_contents(TokenFilter) -> bytes` | instance | materialize token-filtered bytes            |
|  [03]   | `Page.externalize_inline_images(min_size=0, …)`    | instance | promote `BI…EI` inline images to XObjects   |
|  [04]   | `Page.parse_contents(StreamParser)`                | instance | drive a custom `StreamParser`               |
|  [05]   | `parse_content_stream(page_or_stream)`             | static   | tokenize to `ContentStreamInstruction` rows |
|  [06]   | `unparse_content_stream(instructions) -> bytes`    | static   | re-encode content instructions              |
|  [07]   | `get_objects_with_ctm(page)`                       | static   | walk content; each drawn object + its CTM   |
|  [08]   | `Pdf.make_stream(data, d=None) -> Stream`          | instance | mint an indirect content `Stream`           |
|  [09]   | `Pdf.make_indirect(obj) -> Object`                 | instance | promote a direct object to an indirect ref  |

[ENTRYPOINT_SCOPE]: native content authoring

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `canvas.Canvas(page_size=(w, h))`                            | ctor     | author native page content                   |
|  [02]   | `models.PdfImage(...).as_pil_image(*, apply_mask=True)`      | instance | extract an embedded image to the Pillow rail |
|  [03]   | `models.PdfImage(...).extract_to(*, apply_mask=True) -> str` | instance | write native codec bytes, mask-composited    |

- `PdfImage.as_pil_image`/`extract_to`: `apply_mask=True` composites the `/SMask` soft mask or `/Mask` stencil/colour-key mask into an alpha channel (`LA`/`RGBA`, transparency-capable format), `apply_mask=False` returns the opaque base; `apply_decode_array=True` (default) folds a non-default `/Decode` per-channel map, `False` yields raw stored samples; `extract_to` writes to a `stream` or a `fileprefix`+ext path and returns that path. A pixel count over `PdfImage.MAX_IMAGE_PIXELS` raises `DecompressionBombError` (set the class attribute to `None` to disable the guard).
- [04]-[CONTENTSTREAMBUILDER_OPS]: `begin_text` `set_text_font` `show_text` `show_text_with_kerning` `set_text_leading` `move_cursor_new_line` `set_fill_color` `set_stroke_color` `set_line_width` `set_dashes` `cm` `append_rectangle` `fill` `stroke_and_close` `draw_xobject` `begin_marked_content` to `.build()` — text and vector op emitter; raster rides `draw_xobject`, there is no `draw_image`.
- [05]-[PDFIMAGE_COLOR]: `.colorspace` `.icc` `.mode` `.bits_per_component` `.palette` `.indexed` `.image_mask` `.is_device_n` `.is_separation` `.filters` `.decode_parms` — color and codec evidence for the extracted image.
- [06]-[MARKED_CONTENT]: `begin_marked_content(mctype)` `begin_marked_content_proplist(mctype, mcid)` `end_marked_content()` — emit the `/Tag BDC … EMC` operators; the `_proplist` MCID variant binds a canvas-drawn region to the `document/tagged` structure element.

[ENTRYPOINT_SCOPE]: encryption, metadata, attachments, jobs

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Encryption(owner, user, R=6, aes=True, allow=…)`     | ctor     | one AES encryption policy object              |
|  [02]   | `Permissions(extract=…, modify_other=…, …)`           | ctor     | granular per-capability permission flags      |
|  [03]   | `Pdf.attachments[name] = AttachedFileSpec(pdf, data)` | property | embedded-file mapping in the catalog          |
|  [04]   | `Pdf.acroform.add_field(…)` / `.fields`               | property | interactive form editing + signature disable  |
|  [05]   | `Pdf.flatten_annotations(mode='all')`                 | instance | bake annotations + appearance into content    |
|  [06]   | `Sanitizer().remove_*().apply(pdf)`                   | instance | redaction/scrub of active/privacy content     |
|  [07]   | `Job(spec).run()` / `.create_pdf()`                   | instance | execute a declarative qpdf job-JSON pipeline  |
|  [08]   | `JobBuilder().add_pages(…).encrypt(…).build()`        | instance | assemble the job spec fluently                |
|  [09]   | `settings.set_flate_compression_level(…)`             | static   | global flate level; decimal precision get/set |

- [06]-[SANITIZE]: `Sanitizer` chains `remove_javascript` `remove_external_access` `remove_multimedia` `remove_attachments` `remove_private_app_data`, applied by `.apply(pdf)` — strip active or privacy-bearing content.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `pikepdf.open` is the single open factory across path and stream and `Pdf.save` the single emission surface; `attempt_recovery` repairs damaged files, `access_mode` selects mmap vs stream IO, and linearization and encryption ride `save` keyword arguments, never a parallel linearizer or encryptor.
- `Encryption` is one policy object carrying an `allow=Permissions(...)` field set; permission capability lives in `Permissions` rows, never boolean knobs scattered across the `save` signature.
- `Object`/`Dictionary`/`Array`/`Name`/`Stream`/`Operator` mirror the qpdf object model, `ObjectType` discriminates, and the typed scalar wrappers force explicit PDF scalars where Python coercion is ambiguous.
- Content editing tokenizes through `parse_content_stream` into `ContentStreamInstruction` rows, folds the operand/operator pairs, and writes `unparse_content_stream` bytes back through `Pdf.make_stream` into `page.obj[Name.Contents]`; streaming-scale rewrites subclass `TokenFilter.handle_token` on `Page.add_content_token_filter` (or `Page.get_filtered_contents` for a non-mutating read), never materializing the whole stream or hand-rolling a tokenizer.
- `canvas.Canvas` with `ContentStreamBuilder` authors native page content and `Matrix` owns the affine placement math; raster placement is `draw_xobject` and there is no `draw_image`; `reportlab`/`weasyprint` enter only for full HTML/flowable layout, never to place text or graphics on an existing PDF.
- `begin_marked_content_proplist(mctype, mcid)` binds a canvas-drawn region to its assigned structure-element MCID deterministically, where the bare `begin_marked_content` agrees only by document order.
- A Separation/DeviceN spot colorspace authors over the typed object model — a `Array`/`Dictionary`/`Name` colorspace with a Type 2 exponential or Type 4 PostScript-calculator tint function registered into `page.Resources.ColorSpace` and selected by the `cs`/`scn` operators — never a byte-string-concatenated colorspace array.
- `Pdf.open_metadata` yields the read-or-edit XMP editor and `Pdf.docinfo` the raw `/Info` dictionary; an XMP `parseType="Resource"` struct bag, the `pdfd:declarations` PDF Declarations bag, reads whitespace through the `PdfMetadata` mapping view, so its member URIs read off the raw `/Metadata` stream bytes via an element-tree parse.
- `pikepdf.sanitize` is the structure-level active/privacy scrub, distinct from `flatten_annotations` appearance baking and from `pyhanko` signing.
- Each op captures page count, linearization flag, encryption `R` level and AES state, object-stream mode, object count, `Job` exit/warning state, and output byte length as one `ArtifactReceipt` case.

[STACKING]:
- `pillow`(`.api/pillow.md`): `models.PdfImage(page.get_images()[name]).as_pil_image()` hands a `PIL.Image` to the `graphic/raster/io#` owner — an `/SMask`/`/Mask`-bearing image arrives alpha-composited (`LA`/`RGBA`) under the `apply_mask=True` default — and `extract_to` writes native codec bytes; pikepdf extracts, pillow re-encodes and ICC-converts.
- `pypdf`(`.api/pypdf.md`) / `pymupdf`(`.api/pymupdf.md`): `Pdf.save(encryption=Encryption(R=6), linearize=True)` re-encrypts at AES-R6 and linearizes the finished bytes that `pypdf` assembled or `pymupdf` rendered; ruled-table extraction routes to `pdfplumber`(`.api/pdfplumber.md`).
- `pyhanko`(`.api/pyhanko.md`): `sanitize`/`flatten_annotations` finish the bytes before pyhanko PAdES-signs them; sanitize precedes sign.
- `ocrmypdf`(`.api/ocrmypdf.md`): whole-document OCR-to-PDF/A routes here, never a hand-stitched per-page text layer.
- `expression`(`libs/python/.api/expression.md`): the `PdfError` fault family maps at the boundary to `Result[ArtifactReceipt, PdfError]`, `PasswordError` the auth arm and `DataDecodingError`/`DependencyError` the codec arm.
- within-lib: `document/egress` composes `Encryption`/`Permissions`/`flatten_annotations`/`sanitize`; `document/tagged` emits the `begin_marked_content_proplist` MCID binding; `graphic/color/managed` authors the Separation/DeviceN plate over this object model.

[LOCAL_ADMISSION]:
- `import pikepdf` at boundary scope only; PDF is the only format, admitted as one document owner, never a per-source reader type.

[RAIL_LAW]:
- Package: `pikepdf`
- Owns: qpdf-backed open/repair, linearization, AES-R6 encryption with permission flags, page assembly and overlay, content-stream tokenization + `TokenFilter` + `externalize_inline_images` + `get_objects_with_ctm`, native authoring (`canvas` + `ContentStreamBuilder`), object-model editing with typed scalars, Separation/DeviceN plate authoring, XMP/docinfo metadata, image extraction with color/codec evidence and `/SMask`·`/Mask`·`/Decode` compositing under a `MAX_IMAGE_PIXELS` decompression-bomb cap, attachments, outlines, AcroForm and annotation flatten, `sanitize`, and declarative qpdf jobs via `JobBuilder`/`Job`
- Accept: structure repair, linearization, encryption, sanitize, and content authoring feeding the document and export-bundle owners; `PdfImage.as_pil_image` feeding the image rail
- Reject: wrapper-renames of `open`/`save`; scattered permission booleans where `Permissions` rows exist; a hand-rolled tokenizer where `parse_content_stream`/`TokenFilter` exist; a hand-written job-JSON string where `JobBuilder` assembles it; a byte-concatenated colorspace where the typed `Array`/`Dictionary`/`Name` object model builds it; a second renderer where `pymupdf` covers it; simple text/graphic placement routed to `reportlab` where `canvas.Canvas` suffices; a `draw_image` call where raster placement is `draw_xobject`
