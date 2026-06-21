# [PY_ARTIFACTS_API_PIKEPDF]

`pikepdf` is the qpdf-backed PDF structure owner for the artifacts pdf rail. It binds libqpdf through nanobind and exposes the full PDF object model (`Object`/`Dictionary`/`Array`/`Name`/`Stream`/`Operator`), a `Pdf` document root, a `Page` unit with box/resource/overlay control, an `Encryption`/`Permissions` AES policy pair, a streaming content-stream tokenizer (`parse_content_stream`/`TokenFilter`/`unparse_content_stream`), a content-authoring surface (`pikepdf.canvas`), image extraction (`PdfImage.as_pil_image`), XMP/docinfo metadata, AcroForm, outlines, attachments, and a declarative qpdf `Job` runner. The package owner composes `Pdf`, `open`/`new`/`save`, `Encryption`, the object model, and the token-filter fold; it never re-implements the PDF parser, the affine `Matrix`, or the qpdf object model the wheel already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pikepdf`
- package: `pikepdf`
- import: `pikepdf`
- owner: `artifacts`
- rail: pdf
- version: `10.8.0`; bundles libqpdf `12.3.2`
- license: `MPL-2.0` (libqpdf is Apache-2.0); copyleft is file-scoped, so static linkage of the bundled C++ does not infect the consumer
- abi: `cp314-abi3` stable-ABI wheels; ABI3 floor cp314 with forward compatibility to cp315+; `Requires-Python >=3.10`; ungated in the manifest
- entry points: none (library only)
- capability: qpdf-backed open/repair, linearization, AES-R6 encryption with granular permission flags, page assembly/reorder/overlay, content-stream tokenization and streaming filter, content authoring, object-model editing, XMP/docinfo metadata, image extraction, attachments, outlines, AcroForm fields, declarative qpdf jobs

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, page, and policy types
- rail: pdf

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]    | [CAPABILITY]                                                    |
| :-----: | :----------------- | :---------------- | :------------------------------------------------------------- |
|  [01]   | `Pdf`              | document root     | open/new/save, pages, root/trailer/docinfo, encryption, jobs   |
|  [02]   | `Page`             | page unit         | media/crop/art/bleed/trim boxes, contents, resources, overlay  |
|  [03]   | `Encryption`       | encryption spec   | `owner`/`user` passwords, `R` level, `aes`, `allow=Permissions` |
|  [04]   | `Permissions`      | permission flags  | `accessibility`/`extract`/`modify_*`/`print_highres`/`print_lowres` |
|  [05]   | `Job`              | qpdf job          | run a declarative qpdf job-JSON pipeline                       |
|  [06]   | `AcroForm`         | form surface      | `add_field`/`fields`/`remove_fields`/`disable_digital_signatures` |
|  [07]   | `Outline`          | bookmark tree     | `root`/`add` document outline editing                          |
|  [08]   | `AttachedFileSpec` | attachment        | embedded file specification                                    |
|  [09]   | `Matrix`           | affine transform  | content-stream affine matrix with full algebra                 |
|  [10]   | `Rectangle`        | bbox value object | page-box rectangle with corner/extent accessors                |

[PUBLIC_TYPE_SCOPE]: object model, tokens, and enums
- rail: pdf

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE]   | [CAPABILITY]                                                |
| :-----: | :------------------ | :--------------- | :--------------------------------------------------------- |
|  [01]   | `Object`            | pdf object       | base value: `as_dict`/`to_json`/`read_bytes`/`parse`/`unparse` |
|  [02]   | `Dictionary`        | pdf dict         | name-keyed PDF dictionary                                  |
|  [03]   | `Array`             | pdf array        | ordered PDF array                                          |
|  [04]   | `Name`              | pdf name         | `/Name` token; `Name.random()` mints a unique resource name |
|  [05]   | `Stream`            | pdf stream       | dictionary plus encoded byte payload                       |
|  [06]   | `Operator`          | content operator | content-stream operator token                             |
|  [07]   | `Token` / `TokenType` | token + kind   | lexer token and the lexical-kind enum                     |
|  [08]   | `TokenFilter`       | streaming filter | `handle_token` hook for `Page.add_content_token_filter`    |
|  [09]   | `NameTree` / `NumberTree` | name/number tree | mapping view over a PDF name/number tree            |
|  [10]   | `StreamDecodeLevel` | decode-level enum | `none`/`generalized`/`specialized`/`all`                  |
|  [11]   | `ObjectStreamMode`  | objstm-mode enum | `disable`/`preserve`/`generate`                            |
|  [12]   | `AccessMode`        | open-access enum | `default`/`stream`/`mmap`/`mmap_only`                      |

[PUBLIC_TYPE_SCOPE]: model helpers and fault family
- rail: pdf

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]  | [CAPABILITY]                                            |
| :-----: | :------------------------ | :-------------- | :----------------------------------------------------- |
|  [01]   | `models.PdfImage`         | image view      | `as_pil_image`/`extract_to`/`mode`/`colorspace`/`icc`  |
|  [02]   | `models.PdfMetadata`      | XMP metadata    | `load_from_docinfo`/`pdfa_status`/`pdfx_status` mapping |
|  [03]   | `canvas.Canvas`           | content author  | `add_font`/`do`/`to_pdf` page-content builder          |
|  [04]   | `canvas.ContentStreamBuilder` | op emitter  | `begin_text`/`show_text`/`set_fill_color`/`cm`/`fill`  |
|  [05]   | `PdfError`                | base fault      | qpdf operation failure                                 |
|  [06]   | `PasswordError`           | auth fault      | wrong open password                                    |
|  [07]   | `DataDecodingError`       | decode fault    | stream filter decode failure                           |
|  [08]   | `ForeignObjectError`      | ownership fault | object used across `Pdf` owners without `copy_foreign` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open, create, and save
- rail: pdf

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                                              | [CAPABILITY]                            |
| :-----: | :--------------------- | :-------------------------------------------------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `pikepdf.open`         | `open(src, *, password='', attempt_recovery=True, access_mode=AccessMode.default, allow_overwriting_input=False, ignore_xref_streams=False)` | open path/stream with mmap + repair     |
|  [02]   | `pikepdf.new`          | no-arg new document                                                                                       | create an empty PDF                     |
|  [03]   | `Pdf.save`             | `save(target, *, linearize=False, encryption=None, compress_streams=True, object_stream_mode=ObjectStreamMode.preserve, stream_decode_level=None, normalize_content=False, qdf=False, deterministic_id=False, min_version='', force_version='', preserve_pdfa=True, recompress_flate=False)` | save with linearization/encryption/objstm |
|  [04]   | `Pdf.close` / `lock`   | no-arg close / context lock                                                                               | release or guard the document           |
|  [05]   | `Pdf.add_blank_page`   | `add_blank_page(*, page_size=(612.0, 792.0)) -> Page`                                                     | append a blank page                     |
|  [06]   | `Pdf.copy_foreign`     | `copy_foreign(obj) -> Object`                                                                             | import an object from another `Pdf`     |
|  [07]   | `Pdf.check_pdf_syntax` | `check_pdf_syntax(progress=None) -> list[str]`                                                            | validate PDF syntax, return warnings    |
|  [08]   | `Pdf.open_metadata`    | `open_metadata(set_pikepdf_as_editor=True, update_docinfo=True) -> PdfMetadata`                           | context-manager XMP edit                |
|  [09]   | `Pdf.open_outline`     | `open_outline(max_depth=15) -> Outline`                                                                   | context-manager outline edit            |
|  [10]   | `Pdf.remove_unreferenced_resources` | no-arg sweep                                                                                 | garbage-collect unused objects          |

[ENTRYPOINT_SCOPE]: page assembly, content edit, and authoring
- rail: pdf

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                                                  | [CAPABILITY]                              |
| :-----: | :------------------------------- | :---------------------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `Pdf.pages`                      | list-like page collection                                                     | index/insert/delete/reorder pages         |
|  [02]   | `Page.add_overlay`               | `add_overlay(other, rect=None, *, push_stack=True, shrink=True, expand=True) -> Name` | stamp another page atop this one    |
|  [03]   | `Page.add_underlay`              | `add_underlay(other, rect=None, *, shrink=True, expand=True) -> Name`         | place a page beneath this one             |
|  [04]   | `Page.add_resource`              | `add_resource(res, res_type, name=None, *, prefix='', replace_existing=True) -> Name` | register a resource under `/Resources` |
|  [05]   | `Page.as_form_xobject`           | `as_form_xobject(handle_transformations=True) -> Object`                      | convert a page to a form XObject          |
|  [06]   | `Page.contents_add` / `contents_coalesce` | `contents_add(stream, *, prepend=False)` / no-arg coalesce           | append or merge content streams           |
|  [07]   | `Page.images` / `Page.resources` / `Page.mediabox` / `cropbox` / `trimbox` / `bleedbox` / `artbox` | mappings + box `Rectangle`s | per-page image map, resource dict, box geometry |
|  [08]   | `Page.add_content_token_filter`  | `add_content_token_filter(TokenFilter)`                                       | streaming operand/operator rewrite        |
|  [09]   | `pikepdf.parse_content_stream`   | `parse_content_stream(page_or_stream, operators='') -> list[ContentStreamInstructions]` | tokenize content instructions   |
|  [10]   | `pikepdf.unparse_content_stream` | `unparse_content_stream(instructions) -> bytes`                               | re-encode content instructions            |
|  [11]   | `Pdf.make_stream`                | `make_stream(data: bytes, d=None) -> Stream`                                  | mint an indirect content `Stream`         |
|  [12]   | `canvas.Canvas`                  | `Canvas(page_size=(w, h))` then `.add_font`/`.do(ContentStreamBuilder)`/`.to_pdf()` | author native page content (no reportlab leak) |
|  [13]   | `models.PdfImage.as_pil_image`   | `PdfImage(page.images[name]).as_pil_image()` / `.extract_to(fileprefix=...)`  | extract an embedded image to the Pillow rail |

[ENTRYPOINT_SCOPE]: encryption, metadata, attachments, jobs
- rail: pdf

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                  | [CAPABILITY]                            |
| :-----: | :------------------------- | :---------------------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `Encryption`               | `Encryption(owner=..., user=..., R=6, aes=True, metadata=True, allow=Permissions(...))` | one AES encryption policy object  |
|  [02]   | `Permissions`              | `Permissions(extract=False, modify_other=False, print_highres=True, ...)`     | granular per-capability permission flags |
|  [03]   | `Pdf.attachments`          | `attachments[name] = AttachedFileSpec(pdf, data, filename=name)` mapping       | embedded-file mapping in the catalog    |
|  [04]   | `Pdf.acroform`             | `acroform.add_field(...)` / `acroform.fields` / `acroform.disable_digital_signatures()` | interactive form editing       |
|  [05]   | `Pdf.flatten_annotations`  | `flatten_annotations(mode='all')` + `generate_appearance_streams()`           | bake annotations into page content      |
|  [06]   | `Job.run` / `Job.create_pdf` | `Job(job_json_spec).run()` / `.create_pdf()`                                | execute a declarative qpdf job pipeline |
|  [07]   | `settings.set_flate_compression_level` / `set_decimal_precision` | level / precision int                              | global flate level and real precision   |

## [04]-[IMPLEMENTATION_LAW]

[PDF_STRUCTURE]:
- import: `import pikepdf` at boundary scope only; the distribution and import name are both `pikepdf`.
- document axis: `pikepdf.open` is the single open factory across path/stream; `attempt_recovery` repairs damaged files, `access_mode` selects mmap vs stream IO, `allow_overwriting_input` permits in-place save; `pikepdf.new` creates an empty document; PDF is the only format, never a per-source reader type.
- save axis: `Pdf.save` is the single emission surface; `linearize=True` produces fast-web-view output, `encryption=Encryption(...)` rides the same call, `object_stream_mode`/`stream_decode_level`/`compress_streams`/`recompress_flate` tune the xref/compression strategy, `qdf=True`+`normalize_content=True` emit a diffable debug PDF, `deterministic_id=True` yields reproducible output; never a parallel linearizer or encryptor.
- encryption axis: `Encryption(owner=..., user=..., R=6, aes=True, allow=Permissions(...))` is one policy object; permission capability is `Permissions` field rows (`accessibility`, `extract`, `modify_annotation`, `modify_assembly`, `modify_form`, `modify_other`, `print_highres`, `print_lowres`), never parallel boolean knobs scattered across the save signature.
- page axis: `Pdf.pages` is a mutable list-like collection for insert/delete/reorder; `add_overlay`/`add_underlay`/`as_form_xobject`/`add_resource`/`emplace` compose page content; box geometry is `Rectangle`-valued `mediabox`/`cropbox`/`trimbox`/`bleedbox`/`artbox`, never a parallel page builder.
- object axis: `Object`/`Dictionary`/`Array`/`Name`/`Stream`/`Operator` mirror the qpdf object model; content-stream editing uses `parse_content_stream` to tokenize into `ContentStreamInstructions`, folds the operand/operator pairs, then writes `unparse_content_stream` bytes back through `Pdf.make_stream` into `page.obj[Name.Contents]`; streaming-scale rewrites (resource `Name` rename, operator drop, inline-image externalize) subclass `TokenFilter.handle_token` and ride `Page.add_content_token_filter` instead of materializing the whole stream; `Object.to_json`/`as_dict`/`read_bytes`/`parse` are the inspection/round-trip surface; never a hand-rolled tokenizer.
- authoring axis: `canvas.Canvas(page_size=...)` plus `ContentStreamBuilder` (`begin_text`/`set_text_font`/`show_text`/`set_fill_color`/`cm`/`fill`/`draw_xobject`) author native page content and `to_pdf()` into a `Pdf`; `Matrix` (`scaled`/`rotated`/`translated`/`inverse`/`transform`) owns the affine math for placement; this is the in-package vector path — `reportlab`/`weasyprint` are only admitted where a full HTML/flowable layout engine is required, never as a substitute for placing text/graphics on an existing PDF.
- image axis: `models.PdfImage(page.images[name])` decodes an embedded image; `as_pil_image()` hands a `PIL.Image` to the image rail and `extract_to(fileprefix=...)` writes the native codec bytes; `colorspace`/`icc`/`mode`/`bits_per_component` carry the color evidence; pikepdf owns extraction, the Pillow rail owns re-encode/transform.
- metadata axis: `Pdf.open_metadata` yields a `PdfMetadata` mapping with `load_from_docinfo` (legacy `/Info` -> XMP migration), `pdfa_status`/`pdfx_status` conformance probes, and namespace registration; `Pdf.docinfo` is the raw `/Info` dictionary, never a parallel metadata model.
- attachment axis: `Pdf.attachments` is a name-keyed mapping; `attachments[name] = AttachedFileSpec(pdf, data, filename=name)` binds an embedded file into the catalog, never a manual `/EmbeddedFiles` name-tree edit.
- job axis: `Job(job_json_spec).run()` executes the declarative qpdf job-JSON pipeline (split/merge/rotate/encrypt) when the operation is already a qpdf CLI verb, never a bespoke re-implementation of a qpdf job step.
- evidence: each pdf op captures page count, linearization flag, encryption `R` level and AES state, object-stream mode, object count, and output byte length as a pdf receipt.
- boundary: pikepdf owns structure repair, linearization, encryption, content tokenization/authoring, and image extraction; native render/text-extract routes to `pymupdf`; pure-Python merge/split where structure repair is unneeded routes to `pypdf`; PDF digital signing/PAdES routes to `pyhanko`; full HTML/flowable layout routes to `weasyprint`/`reportlab`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pikepdf`
- Owns: qpdf-backed open/repair, linearization, AES-R6 encryption with permission flags, page assembly/overlay, content-stream tokenization + streaming `TokenFilter`, native content authoring (`canvas`), object-model editing, XMP/docinfo metadata, image extraction (`PdfImage`), attachments, outlines, AcroForm, declarative qpdf jobs
- Accept: structure repair, linearization, encryption, and content authoring feeding the document and export-bundle owners; extracted `PdfImage.as_pil_image` feeding the image rail
- Reject: wrapper-renames of `open`/`save`; scattered permission booleans where `Permissions` field rows exist; a hand-rolled content tokenizer where `parse_content_stream`/`TokenFilter` exist; a second renderer where `pymupdf` covers it; routing simple text/graphic placement to `reportlab` where `canvas.Canvas` suffices
