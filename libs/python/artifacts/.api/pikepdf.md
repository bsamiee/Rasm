# [PY_ARTIFACTS_API_PIKEPDF]

`pikepdf` supplies the qpdf-backed PDF structure surface for the artifacts pdf rail: a `Pdf` document root, a `Page` unit, an `Encryption`/`Permissions` policy pair, and the PDF object model (`Object`, `Dictionary`, `Array`, `Name`, `Stream`) that drive structure repair, linearization, encryption, page assembly, and content-stream editing. The package owner composes `Pdf`, the `open`/`new` factory, `Pdf.save(linearize=...)`, and the `Encryption` policy into the pdf owner; it never re-implements the qpdf object model or the PDF parser pikepdf already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pikepdf`
- package: `pikepdf`
- import: `pikepdf`
- owner: `artifacts`
- rail: pdf
- installed: `10.8.0` reflected via `python -c "import pikepdf"` on the gated `python_version<'3.15'` band (cp313)
- entry points: none (library only)
- capability: qpdf-backed PDF open/repair, linearization, AES encryption with permission flags, page assembly and reordering, content-stream tokenization and filtering, object-model editing, metadata, attachments, outlines, AcroForm fields, qpdf job execution

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, page, and policy types
- rail: pdf

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]   | [CAPABILITY]                                    |
| :-----: | :----------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `Pdf`              | document root    | open/new/save, pages, metadata, encryption      |
|  [02]   | `Page`             | page unit        | media/crop boxes, contents, resources, overlay  |
|  [03]   | `Encryption`       | encryption spec  | owner/user passwords, R level, AES, permissions |
|  [04]   | `Permissions`      | permission flags | print/extract/modify capability flags           |
|  [05]   | `Job`              | qpdf job         | run a qpdf job-JSON pipeline                    |
|  [06]   | `AcroForm`         | form surface     | interactive form fields and widgets             |
|  [07]   | `Outline`          | bookmark tree    | document outline editing                        |
|  [08]   | `AttachedFileSpec` | attachment       | embedded file specification                     |

[PUBLIC_TYPE_SCOPE]: object model and fault family
- rail: pdf

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE] | [CAPABILITY]                         |
| :-----: | :-------------- | :------------- | :----------------------------------- |
|  [01]   | `Object`        | pdf object     | base PDF object value                |
|  [02]   | `Dictionary`    | pdf dict       | name-keyed PDF dictionary            |
|  [03]   | `Array`         | pdf array      | ordered PDF array                    |
|  [04]   | `Name`          | pdf name       | `/Name` token                        |
|  [05]   | `Stream`        | pdf stream     | dictionary plus encoded byte payload |
|  [06]   | `Matrix`        | transform      | content-stream affine matrix         |
|  [07]   | `PdfError`      | base fault     | qpdf operation failure               |
|  [08]   | `PasswordError` | auth fault     | wrong open password                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open, create, and save
- rail: pdf

Open/save rows carry path/stream input, password, recovery, linearization, and encryption policy.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                             | [CAPABILITY]                           |
| :-----: | :--------------------- | :------------------------------------------------------- | :------------------------------------- |
|  [01]   | `pikepdf.open`         | `open(src, *, password='', attempt_recovery=True, ...)`  | open from path or stream (with repair) |
|  [02]   | `pikepdf.new`          | no-arg new document                                      | create an empty PDF                    |
|  [03]   | `Pdf.save`             | `save(target, *, linearize=False, encryption=None, ...)` | save with linearization/encryption     |
|  [04]   | `Pdf.close`            | no-arg close                                             | release the document                   |
|  [05]   | `Pdf.add_blank_page`   | `add_blank_page(*, page_size=(612.0, 792.0)) -> Page`    | append a blank page                    |
|  [06]   | `Pdf.copy_foreign`     | foreign object                                           | import an object from another `Pdf`    |
|  [07]   | `Pdf.check_pdf_syntax` | no-arg syntax check                                      | validate PDF syntax                    |
|  [08]   | `Pdf.open_metadata`    | context manager                                          | edit XMP metadata                      |

[ENTRYPOINT_SCOPE]: page assembly and content editing
- rail: pdf

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]              | [CAPABILITY]                       |
| :-----: | :------------------------------- | :------------------------ | :--------------------------------- |
|  [01]   | `Pdf.pages`                      | list-like page collection | index/insert/delete/reorder pages  |
|  [02]   | `Page.add_overlay`               | source page plus rect     | overlay another page atop this one |
|  [03]   | `Page.add_underlay`              | source page plus rect     | place a page beneath this one      |
|  [04]   | `Page.as_form_xobject`           | optional handle policy    | convert a page to a form XObject   |
|  [05]   | `Page.contents_coalesce`         | no-arg coalesce           | merge content streams into one     |
|  [06]   | `pikepdf.parse_content_stream`   | page or object            | tokenize page content instructions |
|  [07]   | `pikepdf.unparse_content_stream` | instruction list          | re-encode content instructions     |
|  [08]   | `Job.run`                        | qpdf job-JSON spec        | execute a declarative qpdf job     |

## [04]-[IMPLEMENTATION_LAW]

[PDF_STRUCTURE]:
- import: `import pikepdf` at boundary scope only; the distribution and import name are both `pikepdf`.
- document axis: `pikepdf.open` is the single open factory across path/stream with `attempt_recovery` for repair; `pikepdf.new` creates an empty document; format is implicit (PDF only), never a per-source reader type.
- save axis: `Pdf.save` is the single emission surface; `linearize=True` produces a web-optimized fast-web-view PDF, `encryption=Encryption(...)` rides the save call, never a parallel linearizer or encryptor.
- encryption axis: `Encryption(owner=..., user=..., R=6, aes=True, allow=Permissions(...))` is one policy object; permission flags are `Permissions` rows, never parallel boolean knobs scattered across the save signature.
- page axis: `Pdf.pages` is a mutable list-like collection for insert/delete/reorder; `add_overlay`/`add_underlay`/`as_form_xobject` compose page content, never a parallel page builder.
- object axis: `Object`/`Dictionary`/`Array`/`Name`/`Stream` mirror the qpdf object model; content-stream editing uses `parse_content_stream`/`unparse_content_stream`, never a hand-rolled tokenizer.
- evidence: each pdf op captures page count, linearization flag, encryption R level, object count, and output byte length as a pdf receipt.
- boundary: pikepdf owns structure repair, linearization, and encryption; native render/extract routes to `pymupdf`; pure-Python merge/split routes to `pypdf`; vector content authoring routes to `reportlab`/`weasyprint`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pikepdf`
- Owns: qpdf-backed PDF open/repair, linearization, AES encryption with permission flags, page assembly, content-stream editing, object-model editing, metadata, attachments, outlines, forms
- Accept: structure repair, linearization, and encryption feeding the document and export-bundle owners
- Reject: wrapper-renames of `open`/`save`; scattered permission booleans where `Permissions` exists; a hand-rolled content tokenizer where `parse_content_stream` exists; a second renderer where `pymupdf` covers it
