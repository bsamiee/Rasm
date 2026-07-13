# [PY_ARTIFACTS_API_PIKEPDF]

`pikepdf` is the qpdf-backed PDF structure owner for the artifacts pdf rail. It binds libqpdf through nanobind and exposes the full PDF object model (`Object`/`Dictionary`/`Array`/`Name`/`Stream`/`Operator`), a `Pdf` document root, a `Page` unit with box/resource/overlay control, an `Encryption`/`Permissions` AES policy pair, a streaming content-stream tokenizer (`parse_content_stream`/`TokenFilter`/`unparse_content_stream`), a content-authoring surface (`pikepdf.canvas`), image extraction (`PdfImage.as_pil_image`), XMP/docinfo metadata, AcroForm, outlines, attachments, and a declarative qpdf `Job` runner. The package owner composes `Pdf`, `open`/`new`/`save`, `Encryption`, the object model, and the token-filter fold; it never re-implements the PDF parser, the affine `Matrix`, or the qpdf object model the package already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pikepdf`
- package: `pikepdf`
- import: `pikepdf`
- owner: `artifacts`
- rail: pdf
- version: `10.9.1`
- license: `MPL-2.0` (libqpdf is Apache-2.0); copyleft is file-scoped, so static linkage of the bundled C++ does not infect the consumer
- entry points: none (library only)
- capability: qpdf-backed open/repair, linearization, AES-R6 encryption with granular permission flags, page assembly/reorder/overlay, content-stream tokenization and streaming filter, content authoring, object-model editing, XMP/docinfo metadata, image extraction, attachments, outlines, AcroForm fields and annotation flatten, declarative qpdf jobs (raw JSON or the `JobBuilder` fluent assembler), inline-image externalization, and document sanitization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, page, and policy types
- rail: pdf

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]   | [CAPABILITY]                                                                |
| :-----: | :---------------------- | :--------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Pdf`                   | document root    | open/new/save, pages, root/trailer/docinfo, encryption, jobs                |
|  [02]   | `Page`                  | page unit        | media/crop/art/bleed/trim boxes, contents, resources, overlay               |
|  [03]   | `Encryption`            | encryption spec  | `owner`/`user` passwords, `R` level, `aes`, `allow=Permissions`             |
|  [04]   | `Permissions`           | permission flags | `accessibility`/`extract`/`modify_*`/`print_highres`/`print_lowres`         |
|  [05]   | `Job`                   | qpdf job         | run a job-JSON pipeline; exit/warnings/encryption/output receipt            |
|  [06]   | `JobBuilder`            | job assembler    | fluent job-JSON assembler → `.build()` dict                                 |
|  [07]   | `AcroForm`              | form surface     | `add_field`/`fields`/`remove_fields`/`disable_digital_signatures`           |
|  [08]   | `AcroFormField`         | form field       | one interactive field; `FormFieldFlag` bit policy                           |
|  [09]   | `Annotation`            | page annotation  | annotation object; `AnnotationFlag` bit policy + appearance source          |
|  [10]   | `Outline`               | bookmark tree    | `root`/`add` document outline editing                                       |
|  [11]   | `OutlineItem`           | bookmark node    | one outline node; `to`/`destination`/`title`/`is_closed`/`children`         |
|  [12]   | `AttachedFileSpec`      | attachment       | embedded file specification                                                 |
|  [13]   | `Matrix`                | affine transform | `scaled`/`rotated`/`translated`/`inverse`/`transform`/`as_array`/`identity` |
|  [14]   | `Rectangle`             | bbox object      | page-box rectangle with corner/extent accessors                             |
|  [15]   | `PageLocation`          | dest enum        | typed page-view kind (`XYZ`/`Fit`/`FitH`/…)                                 |
|  [16]   | `make_page_destination` | dest mint        | outline/link destination minting                                            |

[PUBLIC_TYPE_SCOPE]: object model, tokens, and enums
- rail: pdf

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE]     | [CAPABILITY]                                                                 |
| :-----: | :---------------------------------- | :----------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Object`                            | pdf object         | base value: `as_dict`/`to_json`/`read_bytes`/`parse`/`unparse`               |
|  [02]   | `Dictionary`                        | pdf dict           | name-keyed PDF dictionary                                                    |
|  [03]   | `Array`                             | pdf array          | ordered PDF array                                                            |
|  [04]   | `Name`                              | pdf name           | `/Name` token; `Name.random()` mints a unique resource name                  |
|  [05]   | `Stream`                            | pdf stream         | dictionary plus encoded byte payload                                         |
|  [06]   | `Operator`                          | content operator   | content-stream operator token                                                |
|  [07]   | `Boolean`/`Integer`/`Real`/`String` | typed scalars      | explicit PDF scalar wrappers (vs Python-coerced literals)                    |
|  [08]   | `ObjectType`                        | object-kind enum   | discriminant for `Object` (`dictionary`/`array`/`stream`/`name`/`integer`/…) |
|  [09]   | `ObjectHelper`                      | helper base        | base of `Page`/`Annotation`/`AcroFormField`; `.obj` → backing `Dictionary`   |
|  [10]   | `Token` / `TokenType`               | token + kind       | lexer token and the lexical-kind enum                                        |
|  [11]   | `TokenFilter`                       | streaming filter   | `handle_token` hook for `Page.add_content_token_filter`                      |
|  [12]   | `ContentStreamInstruction`          | parsed instruction | operand+operator pair from `parse_content_stream`                            |
|  [13]   | `ContentStreamInlineImage`          | inline image       | inline-image (`BI…EI`) row from `parse_content_stream`                       |
|  [14]   | `NameTree` / `NumberTree`           | name/number tree   | mapping view over a PDF name/number tree                                     |
|  [15]   | `StreamDecodeLevel`                 | decode-level enum  | `none`/`generalized`/`specialized`/`all`                                     |
|  [16]   | `ObjectStreamMode`                  | objstm-mode enum   | `disable`/`preserve`/`generate`                                              |
|  [17]   | `AccessMode`                        | open-access enum   | `default`/`stream`/`mmap`/`mmap_only`                                        |

[PUBLIC_TYPE_SCOPE]: model helpers and fault family
- rail: pdf

| [INDEX] | [SYMBOL]                             | [PACKAGE_ROLE]  | [CAPABILITY]                                            |
| :-----: | :----------------------------------- | :-------------- | :------------------------------------------------------ |
|  [01]   | `models.PdfImage`                    | image view      | `as_pil_image`/`extract_to`/`mode`/`colorspace`/`icc`   |
|  [02]   | `models.PdfMetadata`                 | XMP metadata    | `load_from_docinfo`/`pdfa_status`/`pdfx_status` mapping |
|  [03]   | `canvas.Canvas`                      | content author  | `add_font`/`do`/`to_pdf` page-content builder           |
|  [04]   | `canvas.ContentStreamBuilder`        | op emitter      | `begin_text`/`show_text`/`set_fill_color`/`cm`/`fill`   |
|  [05]   | `PdfError`                           | base fault      | qpdf operation failure (root of the fault family)       |
|  [06]   | `PasswordError`                      | auth fault      | wrong open password                                     |
|  [07]   | `DataDecodingError`                  | decode fault    | stream filter decode failure                            |
|  [08]   | `ForeignObjectError`                 | ownership fault | object used across `Pdf` owners without `copy_foreign`  |
|  [09]   | `DeletedObjectError`                 | lifetime fault  | access to an object whose owning `Pdf` was closed       |
|  [10]   | `ReferenceCycleError`                | cycle fault     | refusal to materialize a self-referential object graph  |
|  [11]   | `DependencyError`                    | codec fault     | a stream filter needs an unavailable native codec       |
|  [12]   | `OutlineStructureError`              | outline fault   | malformed outline tree during `open_outline` edit       |
|  [13]   | `UnsupportedImageTypeError`          | image fault     | `PdfImage` decode refusal by image class                |
|  [14]   | `InvalidPdfImageError`               | image fault     | malformed embedded image data                           |
|  [15]   | `HifiPrintImageNotTranscodableError` | image fault     | hi-fi print image not transcodable                      |
|  [16]   | `JobUsageError`                      | job fault       | invalid `Job`/`JobBuilder` configuration                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open, create, and save
- rail: pdf

`pikepdf.open(src, *, password='', attempt_recovery=True, access_mode=AccessMode.default, allow_overwriting_input=False, ignore_xref_streams=False)` is the single open factory; `Pdf.save(target, *, linearize=False, encryption=None, compress_streams=True, object_stream_mode=ObjectStreamMode.preserve, stream_decode_level=None, normalize_content=False, qdf=False, deterministic_id=False, min_version='', force_version='', preserve_pdfa=True, recompress_flate=False)` the single emission surface; `Pdf.open_metadata(set_pikepdf_as_editor=True, update_docinfo=True)` yields the XMP editor.

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]                                          | [CAPABILITY]                              |
| :-----: | :---------------------------------- | :---------------------------------------------------- | :---------------------------------------- |
|  [01]   | `pikepdf.open`                      | `open(src, *, …)`                                     | open path/stream with mmap + repair       |
|  [02]   | `pikepdf.new`                       | no-arg new document                                   | create an empty PDF                       |
|  [03]   | `Pdf.save`                          | `save(target, *, …)`                                  | save with linearization/encryption/objstm |
|  [04]   | `Pdf.close` / `lock`                | no-arg close / context lock                           | release or guard the document             |
|  [05]   | `Pdf.add_blank_page`                | `add_blank_page(*, page_size=(612.0, 792.0)) -> Page` | append a blank page                       |
|  [06]   | `Pdf.copy_foreign`                  | `copy_foreign(obj) -> Object`                         | import an object from another `Pdf`       |
|  [07]   | `Pdf.check_pdf_syntax`              | `check_pdf_syntax(progress=None) -> list[str]`        | validate PDF syntax, return warnings      |
|  [08]   | `Pdf.open_metadata`                 | `open_metadata(…) -> PdfMetadata`                     | context-manager XMP edit                  |
|  [09]   | `Pdf.open_outline`                  | `open_outline(max_depth=15) -> Outline`               | context-manager outline edit              |
|  [10]   | `Pdf.remove_unreferenced_resources` | no-arg sweep                                          | garbage-collect unused objects            |

[ENTRYPOINT_SCOPE]: page assembly and resources
- rail: pdf

`Page.add_overlay(other, rect=None, *, push_stack=True, shrink=True, expand=True)` and `add_underlay(other, rect=None, *, shrink=True, expand=True)` stamp or place a page; `add_resource(res, res_type, name=None, *, prefix='', replace_existing=True)` registers under `/Resources`; all three return a `Name`.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                   | [CAPABILITY]                                        |
| :-----: | :--------------------- | :--------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `Pdf.pages`            | list-like page collection                      | index/insert/delete/reorder pages                   |
|  [02]   | `Page.add_overlay`     | `add_overlay(other, rect=None, …)`             | stamp another page atop this one                    |
|  [03]   | `Page.add_underlay`    | `add_underlay(other, rect=None, …)`            | place a page beneath this one                       |
|  [04]   | `Page.add_resource`    | `add_resource(res, res_type, name=None, …)`    | register a resource under `/Resources`              |
|  [05]   | `Page.as_form_xobject` | `as_form_xobject(handle_transformations=True)` | convert a page to a form XObject                    |
|  [06]   | `Page.contents_add`    | `contents_add(stream, *, prepend=False)`       | append or merge streams; no-arg `contents_coalesce` |

- [07]-[Page maps + boxes]: `Page.images`/`Page.resources` maps + the `mediabox`/`cropbox`/`trimbox`/`bleedbox`/`artbox` `Rectangle` boxes — per-page image map, resource dict, box geometry.

[ENTRYPOINT_SCOPE]: content-stream tokenize and edit
- rail: pdf

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                    | [CAPABILITY]                                |
| :-----: | :------------------------------- | :---------------------------------------------- | :------------------------------------------ |
|  [01]   | `Page.add_content_token_filter`  | `add_content_token_filter(TokenFilter)`         | streaming operand/operator rewrite          |
|  [02]   | `Page.get_filtered_contents`     | `get_filtered_contents(TokenFilter) -> bytes`   | materialize token-filtered bytes            |
|  [03]   | `Page.externalize_inline_images` | `externalize_inline_images(min_size=0, …)`      | promote `BI…EI` inline images to XObjects   |
|  [04]   | `Page.parse_contents`            | `parse_contents(StreamParser)`                  | drive a custom `StreamParser`               |
|  [05]   | `pikepdf.parse_content_stream`   | `parse_content_stream(page_or_stream)`          | tokenize to `ContentStreamInstruction` rows |
|  [06]   | `pikepdf.unparse_content_stream` | `unparse_content_stream(instructions) -> bytes` | re-encode content instructions              |
|  [07]   | `pikepdf.get_objects_with_ctm`   | `get_objects_with_ctm(page)`                    | walk content; each drawn object + its CTM   |
|  [08]   | `Pdf.make_stream`                | `make_stream(data: bytes, d=None) -> Stream`    | mint an indirect content `Stream`           |
|  [09]   | `Pdf.make_indirect`              | `make_indirect(obj: Object) -> Object`          | promote a direct object to an indirect ref  |

[ENTRYPOINT_SCOPE]: native content authoring
- rail: pdf

`canvas.Canvas(page_size=(w, h))` then `.add_font(DimensionedFont)`/`.do(ContentStreamBuilder)`/`.to_pdf()` authors native page content (no reportlab leak); `models.PdfImage` extracts an embedded image to the Pillow rail. The builder op family, the `PdfImage` color surface, and the marked-content operators carry as keyed rows below.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                            | [CAPABILITY]                                 |
| :-----: | :----------------------------- | :------------------------------------------------------ | :------------------------------------------- |
|  [01]   | `canvas.Canvas`                | `Canvas(page_size=(w, h))`                              | author native page content                   |
|  [02]   | `models.PdfImage.as_pil_image` | `PdfImage(images[name]).as_pil_image()` / `.extract_to` | extract an embedded image to the Pillow rail |

- [03]-[CONTENTSTREAMBUILDER_OPS]: `begin_text`/`set_text_font`/`show_text`/`show_text_with_kerning`/`set_text_leading`/`move_cursor_new_line`/`set_fill_color`/`set_stroke_color`/`set_line_width`/`set_dashes`/`cm`/`append_rectangle`/`fill`/`stroke_and_close`/`draw_xobject`/`begin_marked_content` → `.build()` — full text+vector op emitter (no `draw_image`; raster rides `draw_xobject`).
- [04]-[PDFIMAGE_COLOR]: `.colorspace`/`.icc`/`.mode`/`.bits_per_component`/`.palette`/`.indexed`/`.image_mask`/`.is_device_n`/`.is_separation`/`.filters`/`.decode_parms` — full color/codec evidence for the extracted image.
- [05]-[MARKED_CONTENT]: `begin_marked_content(mctype: Name)` / `begin_marked_content_proplist(mctype: Name, mcid: int)` / `end_marked_content()` — emit the `/Tag BDC … EMC` operators binding a canvas-drawn region to the `document/tagged#ACCESS` `/StructTreeRoot`; the `_proplist` variant carries the explicit `/MCID` (`<</MCID n>>` property list) so a drawn region deterministically resolves to the MCID the structure element assigns rather than agreeing by document-order convention alone.

[ENTRYPOINT_SCOPE]: encryption, metadata, attachments, jobs
- rail: pdf

`Encryption(owner=…, user=…, R=6, aes=True, metadata=True, allow=Permissions(…))` and `Permissions(extract=…, modify_other=…, print_highres=…, …)` are one AES policy pair; `sanitize` composes the scrub helpers; `Job`/`JobBuilder` run or assemble a qpdf job-JSON pipeline (receipt via `exit_code`/`has_warnings`/`encryption_status`/`creates_output`).

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                        | [CAPABILITY]                                       |
| :-----: | :--------------------------- | :-------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `Encryption`                 | `Encryption(owner, user, R=6, aes=True, allow=…)`   | one AES encryption policy object                   |
|  [02]   | `Permissions`                | `Permissions(extract=False, modify_other=False, …)` | granular per-capability permission flags           |
|  [03]   | `Pdf.attachments`            | `attachments[name] = AttachedFileSpec(pdf, data)`   | embedded-file mapping in the catalog               |
|  [04]   | `Pdf.acroform`               | `acroform.add_field(…)` / `.fields`                 | interactive form editing (+ signature disable)     |
|  [05]   | `Pdf.flatten_annotations`    | `flatten_annotations(mode='all')`                   | bake annotations + appearance streams into content |
|  [06]   | `pikepdf.sanitize`           | `Sanitizer(pdf)` + scrub helpers                    | redaction/scrub of active/privacy content          |
|  [07]   | `Job.run` / `Job.create_pdf` | `Job(job_json_spec).run()` / `.create_pdf()`        | execute a declarative qpdf job-JSON pipeline       |
|  [08]   | `JobBuilder`                 | `JobBuilder().add_pages(…).encrypt(…).build()`      | assemble the job spec fluently (vs hand JSON)      |
|  [09]   | `settings.*`                 | `set_flate_compression_level`                       | global flate level; `set/get_decimal_precision`    |

- [06]-[SANITIZE]: `sanitize.remove_javascript(pdf)` / `remove_external_access` / `remove_multimedia` / `remove_attachments` / `remove_private_app_data` / `Sanitizer(pdf)` — helpers stripping active or privacy-bearing content.

[ENTRYPOINT_SCOPE]: Separation / DeviceN plate authoring (raw object model)
- rail: pdf — the WRITE-side complement to the read-side `PdfImage.is_separation`/`is_device_n` flags: authoring a spot/plate colorspace over the raw object model (`Object`/`Array`/`Name`/`Dictionary`/`Stream` + `make_stream`/`make_indirect`), the surface `graphic/color/managed#MANAGED`'s V4 plate authoring composes. A Separation/DeviceN colorspace is a direct PDF `Array`; its tint transform is a PDF Function object (Type 2 exponential dict, or a Type 4 PostScript-calculator `Stream`); the colorspace registers into `page.Resources.ColorSpace` and content selects it with the `cs`/`scn` operators.

| [INDEX] | [SURFACE]               | [ROLE]                     |
| :-----: | :---------------------- | :------------------------- |
|  [01]   | Separation array        | spot-ink plate colorspace  |
|  [02]   | DeviceN array           | N-colorant plate set       |
|  [03]   | tint transform (Type 2) | exponential spot→CMYK ramp |
|  [04]   | tint transform (Type 4) | PostScript-calculator tint |
|  [05]   | register + reference    | attach + select the plate  |

- [01]-[SEPARATION]: `Array([Name('/Separation'), Name('/PANTONE 185 C'), altspace, tint_fn])` (`altspace` = `Name('/DeviceCMYK')` or an ICC-based `Array`) — one spot-ink plate colorspace; the second element is the colorant name, the fourth the tint-transform function.
- [02]-[DEVICEN]: `Array([Name('/DeviceN'), Array([Name('/Cyan'), Name('/Spot1'), …]), altspace, tint_fn, attrs])` (`attrs` = `Dictionary(Subtype=Name('/NChannel'), Colorants=Dictionary(...), Process=Dictionary(...))`) — an N-colorant plate set; the `/NChannel` `attrs` carry per-colorant Separation colorspaces + the ICC process link.
- [03]-[TYPE_2]: `Dictionary(FunctionType=2, Domain=[0, 1], C0=[0, 0, 0, 0], C1=[c, m, y, k], N=1)` — exponential-interpolation tint from 0 (`C0`) to full ink (`C1`), the simple spot→CMYK ramp, a direct dict, no stream.
- [04]-[TYPE_4]: `pdf.make_stream(b'{ … PostScript calculator … }', Dictionary(FunctionType=4, Domain=[0, 1], Range=[0, 1, 0, 1, 0, 1, 0, 1]))` — PostScript-calculator tint for a non-linear or multi-colorant transform; the body is a content `Stream` minted via `make_stream`.
- [05]-[REGISTER]: `page.Resources.ColorSpace[Name('/PlateCS')] = pdf.make_indirect(sep_cs)` then content `/PlateCS cs` + `1.0 scn` (via `ContentStreamBuilder.set_fill_color`/raw operator) — attach the colorspace as an indirect resource and select it in the content stream; the plate ink is now paintable.

## [04]-[IMPLEMENTATION_LAW]

[PDF_STRUCTURE]:
- import: `import pikepdf` at boundary scope only; the distribution and import name are both `pikepdf`.
- document axis: `pikepdf.open` is the single open factory across path/stream; `attempt_recovery` repairs damaged files, `access_mode` selects mmap vs stream IO, `allow_overwriting_input` permits in-place save; `pikepdf.new` creates an empty document; PDF is the only format, never a per-source reader type.
- save axis: `Pdf.save` is the single emission surface; `linearize=True` produces fast-web-view output, `encryption=Encryption(...)` rides the same call, `object_stream_mode`/`stream_decode_level`/`compress_streams`/`recompress_flate` tune the xref/compression strategy, `qdf=True`+`normalize_content=True` emit a diffable debug PDF, `deterministic_id=True` yields reproducible output; never a parallel linearizer or encryptor.
- encryption axis: `Encryption(owner=..., user=..., R=6, aes=True, allow=Permissions(...))` is one policy object; permission capability is `Permissions` field rows (`accessibility`, `extract`, `modify_annotation`, `modify_assembly`, `modify_form`, `modify_other`, `print_highres`, `print_lowres`), never parallel boolean knobs scattered across the save signature.
- page axis: `Pdf.pages` is a mutable list-like collection for insert/delete/reorder; `add_overlay`/`add_underlay`/`as_form_xobject`/`add_resource`/`emplace` compose page content; box geometry is `Rectangle`-valued `mediabox`/`cropbox`/`trimbox`/`bleedbox`/`artbox`, never a parallel page builder.
- object axis: `Object`/`Dictionary`/`Array`/`Name`/`Stream`/`Operator` mirror the qpdf object model; `ObjectType` is the discriminant and the typed scalar wrappers (`Boolean`/`Integer`/`Real`/`String`/`Name`) force explicit PDF scalars where Python coercion does be ambiguous; content-stream editing uses `parse_content_stream` to tokenize into `ContentStreamInstruction` rows (and `ContentStreamInlineImage` for `BI…EI`), folds the operand/operator pairs, then writes `unparse_content_stream` bytes back through `Pdf.make_stream` into `page.obj[Name.Contents]`; streaming-scale rewrites (resource `Name` rename, operator drop) subclass `TokenFilter.handle_token` and ride `Page.add_content_token_filter` (or `Page.get_filtered_contents` for a non-mutating materialization), while inline-image promotion uses `Page.externalize_inline_images` and CTM-aware traversal uses `get_objects_with_ctm` — never materialize the whole stream or hand-roll a tokenizer; `Object.to_json`/`as_dict`/`read_bytes`/`parse` are the inspection/round-trip surface.
- authoring axis: `canvas.Canvas(page_size=...)` with `.add_font(DimensionedFont)` plus `ContentStreamBuilder` author native page content and `to_pdf()` into a `Pdf`; the builder owns the full text op family (`begin_text`/`set_text_font`/`set_text_leading`/`set_text_matrix`/`move_cursor_new_line`/`show_text`/`show_text_with_kerning`/`show_text_line`) and the vector op family (`set_fill_color`/`set_stroke_color`/`set_line_width`/`set_dashes`/`cm`/`append_rectangle`/`fill`/`stroke_and_close`/`draw_xobject`/`begin_marked_content`/`begin_marked_content_proplist`/`end_marked_content`); the `begin_marked_content_proplist(mctype, mcid)` MCID variant is what a `document/tagged#ACCESS`-tagged region emits so a canvas-drawn span binds to its assigned structure-element MCID deterministically; raster placement is `draw_xobject` (there is no `draw_image`); `Matrix` (`scaled`/`rotated`/`translated`/`inverse`/`transform`) owns the affine math for placement; this is the in-package vector path — `reportlab`/`weasyprint` are only admitted where a full HTML/flowable layout engine is required, never as a substitute for placing text/graphics on an existing PDF.
- image axis: `models.PdfImage(page.images[name])` decodes an embedded image; `as_pil_image()` hands a `PIL.Image` to the image rail and `extract_to(fileprefix=...)` writes the native codec bytes; `colorspace`/`icc`/`mode`/`bits_per_component` carry the color evidence; pikepdf owns extraction, the Pillow rail owns re-encode/transform.
- metadata axis: `Pdf.open_metadata` yields a `PdfMetadata` mapping with `load_from_docinfo` (legacy `/Info` -> XMP migration), `pdfa_status`/`pdfx_status` conformance probes, and namespace registration; `Pdf.docinfo` is the raw `/Info` dictionary, never a parallel metadata model.
- attachment axis: `Pdf.attachments` is a name-keyed mapping; `attachments[name] = AttachedFileSpec(pdf, data, filename=name)` binds an embedded file into the catalog, never a manual `/EmbeddedFiles` name-tree edit.
- job axis: when the operation is already a qpdf CLI verb (split/merge/rotate/encrypt/collate/decrypt), build the spec with `JobBuilder` fluent calls and run `Job(builder.build()).run()` — or pass a hand-authored `job_json_spec` — never a bespoke re-implementation of a qpdf job step; the `Job` receipt fields (`exit_code`/`has_warnings`/`encryption_status`/`creates_output`) feed the pdf receipt directly.
- plate axis: a Separation/DeviceN spot colorspace is authored over the raw object model — a `Array([Name('/Separation'), Name('/<ink>'), altspace, tint_fn])` (or `[/DeviceN [colorants] altspace tint_fn attrs]` with an `/NChannel` `attrs` dict), the tint transform a Type 2 exponential `Dictionary` or a Type 4 PostScript-calculator `pdf.make_stream(...)` function, registered as `page.Resources.ColorSpace[Name('/<key>')] = pdf.make_indirect(cs)` and selected in content with the `cs`/`scn` operators. `graphic/color/managed#MANAGED`'s V4 plate authoring composes this write surface; the read-side `PdfImage.is_separation`/`is_device_n` flags are its inverse — never hand-concatenate the colorspace array as a byte string where the typed `Array`/`Dictionary`/`Name` object model builds it.
- sanitize axis: `pikepdf.sanitize` removes active or privacy-bearing content (`remove_javascript`/`remove_external_access`/`remove_multimedia`/`remove_attachments`/`remove_private_app_data`) or composes them via `Sanitizer`; this is the structure-level scrub before a deliverable leaves the boundary, distinct from `flatten_annotations` (appearance baking) and from `pyhanko` signing.
- evidence: each pdf op captures page count, linearization flag, encryption `R` level and AES state, object-stream mode, object count, `Job` exit/warning state where a job ran, and output byte length as a pdf receipt.
- boundary: pikepdf owns structure repair, linearization, encryption, content tokenization/authoring, redaction/sanitize, and image extraction; native render/text-extract routes to `pymupdf`; ruled-table text extraction routes to `pdfplumber`; pure-Python merge/split where structure repair is unneeded routes to `pypdf`; PDF digital signing/PAdES routes to `pyhanko`; full HTML/flowable layout routes to `weasyprint`/`reportlab`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pikepdf`
- Owns: qpdf-backed open/repair, linearization, AES-R6 encryption with permission flags, page assembly/overlay, content-stream tokenization + streaming `TokenFilter` + `externalize_inline_images` + `get_objects_with_ctm`, native content authoring (`canvas` + the full `ContentStreamBuilder` op family), object-model editing with typed scalars, Separation/DeviceN plate colorspace authoring (raw-object `Array`/`Dictionary` + Type 2/4 tint functions), XMP/docinfo metadata, image extraction with color/codec evidence (`PdfImage`), attachments, outlines, AcroForm + annotation flatten, redaction/`sanitize`, and declarative qpdf jobs via `JobBuilder`/`Job`
- Accept: structure repair, linearization, encryption, sanitize, and content authoring feeding the document and export-bundle owners; extracted `PdfImage.as_pil_image` feeding the image rail
- Reject: wrapper-renames of `open`/`save`; scattered permission booleans where `Permissions` field rows exist; a hand-rolled content tokenizer where `parse_content_stream`/`TokenFilter` exist; a hand-written job-JSON string where `JobBuilder` assembles it; a byte-string-concatenated Separation/DeviceN colorspace where the typed `Array`/`Dictionary`/`Name` object model builds it; a second renderer where `pymupdf` covers it; routing simple text/graphic placement to `reportlab` where `canvas.Canvas` suffices; a phantom `draw_image` call where raster placement is `draw_xobject`
