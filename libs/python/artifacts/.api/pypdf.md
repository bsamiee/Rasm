# [PY_ARTIFACTS_API_PYPDF]

`pypdf` supplies the pure-Python PDF read/write surface for the artifacts pdf rail: a reader root, a writer root, a page object, and a transformation algebra that drive merge, split, rotate, scale, text/image extraction, encryption, and metadata edits without a native runtime. The package owner composes `PdfReader`, `PdfWriter`, `PageObject`, and `Transformation` into the pdf owner; it never re-implements PDF object parsing pypdf already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pypdf`
- package: `pypdf`
- import: `pypdf`
- owner: `artifacts`
- rail: pdf
- installed: `6.13.2` reflected via `python -c "import pypdf"` on cp315
- entry points: none (library only)
- capability: pure-Python PDF read/write, merge, append, split, rotate, scale, text and image extraction, encryption/decryption, outline and annotation authoring, metadata editing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document roots and geometry
- rail: pdf

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE] | [CAPABILITY]                                                                       |
| :-----: | :-------------------- | :------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `PdfReader`           | reader root    | open, decrypt, and read an existing PDF; exposes pages, metadata, outline, XMP     |
|  [02]   | `PdfWriter`           | writer root    | build or clone a PDF; append pages, encrypt, write outline/annotations/attachments |
|  [03]   | `PageObject`          | page unit      | one page; text/image extraction, transform, merge, rotate, scale, box geometry     |
|  [04]   | `Transformation`      | affine algebra | composable CTM for translate/rotate/scale applied to a page                        |
|  [05]   | `PageRange`           | page selector  | a parsed slice expression over a page index space                                  |
|  [06]   | `PaperSize`           | size table     | named physical page dimensions                                                     |
|  [07]   | `DocumentInformation` | metadata view  | the document info dictionary projection                                            |

[PUBLIC_TYPE_SCOPE]: enums and faults
- rail: pdf

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]  | [CAPABILITY]                                           |
| :-----: | :-------------------------------- | :-------------- | :----------------------------------------------------- |
|  [01]   | `ImageType`                       | image enum      | image extraction output kind                           |
|  [02]   | `PasswordType`                    | auth enum       | decrypt result discriminant (owner/user/not-decrypted) |
|  [03]   | `ObjectDeletionFlag`              | cleanup flag    | object-removal selector for compaction                 |
|  [04]   | `constants.UserAccessPermissions` | permission flag | `IntFlag` permission bitmask for `PdfWriter.encrypt`   |

`UserAccessPermissions` members: `PRINT`, `MODIFY`, `EXTRACT`, `ADD_OR_MODIFY`, `FILL_FORM_FIELDS`, `EXTRACT_TEXT_AND_GRAPHICS`, `ASSEMBLE_DOC`, `PRINT_TO_REPRESENTATION` (the `IntFlag` composes by `|`).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read, write, and clone construction
- rail: pdf

Constructor rows carry stream/path input, password, strictness, cloning, and recovery-limit policy.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                   | [CAPABILITY]                     |
| :-----: | :----------------------- | :----------------------------- | :------------------------------- |
|  [01]   | `PdfReader`              | source plus reader policy      | open and decrypt an existing PDF |
|  [02]   | `PdfWriter`              | target plus clone/write policy | build or clone a writer          |
|  [03]   | `PdfReader.decrypt`      | text or bytes password         | decrypt with the given password  |
|  [04]   | `PdfReader.is_encrypted` | encryption-state property      | encryption-state probe           |
|  [05]   | `PdfReader.xmp_metadata` | optional XMP metadata property | XMP metadata view                |

[ENTRYPOINT_SCOPE]: page assembly, transform, and extraction
- rail: pdf

Assembly rows share source-document, page-range, metadata, encryption, stream-target, text-mode, and transform policy.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                                            | [CAPABILITY]                                                                           |
| :-----: | :------------------------------ | :------------------------------------------------------------------------------------------------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `PdfWriter.add_page`            | page plus excluded-key policy                                                                           | append one page                                                                        |
|  [02]   | `PdfWriter.append`              | source plus page range                                                                                  | append source pages                                                                    |
|  [03]   | `PdfWriter.add_blank_page`      | optional width/height                                                                                   | append a blank page                                                                    |
|  [04]   | `PdfWriter.encrypt`             | `encrypt(user_password, owner_password=None, use_128bit=True, *, algorithm=None, permissions_flag=...)` | encrypt the output (`algorithm` ∈ `RC4-40`/`RC4-128`/`AES-128`/`AES-256`/`AES-256-R5`) |
|  [05]   | `PdfWriter.add_metadata`        | info dictionary                                                                                         | set document info                                                                      |
|  [06]   | `PdfWriter.write`               | path or stream target                                                                                   | serialize to a stream                                                                  |
|  [07]   | `PageObject.extract_text`       | extraction mode plus visitors                                                                           | extract page text                                                                      |
|  [08]   | `PageObject.images`             | images property                                                                                         | extract embedded images                                                                |
|  [09]   | `PageObject.add_transformation` | transform plus expand flag                                                                              | apply affine transform                                                                 |
|  [10]   | `PageObject.merge_page`         | source page plus overlay                                                                                | overlay another page                                                                   |
|  [11]   | `PageObject.rotate`             | clockwise angle                                                                                         | rotate clockwise                                                                       |
|  [12]   | `PageObject.scale`              | x/y scale factors                                                                                       | scale the page                                                                         |
|  [13]   | `Transformation`                | affine matrix seed                                                                                      | compose affine transforms                                                              |

## [04]-[IMPLEMENTATION_LAW]

[PDF_PURE]:
- import: `import pypdf` at boundary scope only; module-level import is banned by the manifest import policy.
- reader/writer split: `PdfReader` owns ingestion and `PdfWriter` owns emission; clone-then-edit (`clone_from`) and incremental write are the two write modalities, a row on the writer axis, never parallel writer types.
- page axis: `PageObject` is the single page owner; transform/merge/rotate/scale compose `Transformation`, never a per-operation page subtype.
- encryption axis: `PdfWriter.encrypt` is the single in-process encrypt surface; `algorithm` (`RC4-40`/`RC4-128`/`AES-128`/`AES-256`/`AES-256-R5`) selects strength and `permissions_flag` is a `UserAccessPermissions` `IntFlag` composed by `|`, never scattered boolean print/extract/modify knobs; R6/AES-256 (qpdf-native strength) routes to `pikepdf`.
- extraction axis: `extract_text` with its `extraction_mode` row and `images` are the extraction surface; the result feeds the document owner, never a re-minted text model.
- evidence: each operation captures source page count, output page count, encryption state (`PasswordType`), and output byte length as a pdf receipt.
- boundary: pypdf owns pure-Python structural editing; rasterization and rendering route to `pymupdf`/`pypdfium2`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pypdf`
- Owns: pure-Python PDF read/write, merge, append, split, rotate, scale, text/image extraction, encryption, outline/annotation/attachment authoring, metadata editing
- Accept: structural PDF assembly and extraction feeding the document/PDF owner
- Reject: wrapper-renames of `add_page`/`extract_text`; a native rasterizer reimplementation; identity minting the runtime owns
