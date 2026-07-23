# [PY_ARTIFACTS_API_DOCXTPL]

`docxtpl` owns DOCX templating for the artifacts `document` rail: one `DocxTemplate` loads a `.docx` as a jinja2 template, renders a context dict into the body, header, and footer XML, and saves the result. Styled content enters the render context as typed carriers that own the run XML, so the owner never re-implements the Office Open XML serialization `python-docx` already carries. Its `document/emit#DOCUMENT` consumer lowers the `DocumentNode` tree into that context through the `DocumentMode.DOCX_TEMPLATE` arm and mints the `ArtifactReceipt.Office` receipt case.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `docxtpl`
- package: `docxtpl` (LGPL-2.1-only, Eric Lapouyade)
- import: `docxtpl`
- owner: `artifacts`
- rail: document — load a `.docx` jinja2 template, render a context into body/header/footer XML, emit the rendered document

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: template root and content carriers

`RichText`/`RichTextParagraph`/`Listing`/`InlineImage` are content carriers placed in the render context, each stringifying to OOXML via `__str__`/`__html__`; `R` aliases `RichText` and `RP` aliases `RichTextParagraph`.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [CAPABILITY]                                                |
| :-----: | :------------------ | :-------------- | :---------------------------------------------------------- |
|  [01]   | `DocxTemplate`      | template root   | load, render, and save a `.docx` jinja2 template            |
|  [02]   | `RichText`          | content carrier | inline styled run sequence inside an existing paragraph     |
|  [03]   | `RichTextParagraph` | content carrier | styled paragraph sequence outside an existing paragraph     |
|  [04]   | `Listing`           | content carrier | newline/page-break-preserving text keeping template styling |
|  [05]   | `InlineImage`       | content carrier | inline image bound to a template part, optional hyperlink   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `DocxTemplate` render and emit

`render` mutates the loaded document in place; `render_footnotes` and `render_properties` run after it on the same context, `render_properties` writing the string core properties `author`, `comments`, `identifier`, `language`, `subject`, and `title`.
- render family carry: `context`, `jinja_env`

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `DocxTemplate(template_file)`                             | ctor     | load a `.docx` as a jinja2 template (path/stream)     |
|  [02]   | `render(context, jinja_env=None, autoescape=False)`       | instance | substitute context into body + header/footer in place |
|  [03]   | `render_footnotes(context, jinja_env=None)`               | instance | substitute into footnote-part XML                     |
|  [04]   | `render_properties(context, jinja_env=None)`              | instance | render context through `core_properties`              |
|  [05]   | `save(filename, *args, **kwargs)`                         | instance | write the rendered doc to path or stream              |
|  [06]   | `get_undeclared_template_variables(jinja_env=, context=)` | instance | list jinja2 variables the template references         |
|  [07]   | `new_subdoc(docpath=None) -> Subdoc`                      | factory  | mint a sub-document carrier (needs `docxcompose`)     |
|  [08]   | `build_url_id(url) -> str`                                | instance | register a hyperlink relationship, return `url_id`    |
|  [09]   | `get_docx() -> Document`                                  | property | reach the underlying `python-docx` `Document`         |
|  [10]   | `replace_media(src_file, dst_file)`                       | instance | swap a media part before render                       |
|  [11]   | `replace_embedded(src_file, dst_file)`                    | instance | swap an embedded OLE/object part before render        |
|  [12]   | `replace_zipname(zipname, dst_file)`                      | instance | swap a zip-entry part (the `EmitSpec.replace` band)   |
|  [13]   | `replace_pic(embedded_file, dst_file)`                    | instance | swap a picture part by embedded filename              |

[ENTRYPOINT_SCOPE]: content carriers

Carriers serialize to OOXML and stringify into the render context. `RichText`/`R` accumulate styled runs via `add`; a `RichText` passed as the `text` of another `add` concatenates by XML, so carriers nest. `RichTextParagraph`/`RP` accumulate styled paragraphs, each wrapping a `RichText` under an optional `parastyle`. `Listing` escapes text while preserving `\n` (soft line break) and `\a` (page break) to keep the surrounding paragraph style.
- `RichText.add` appearance axis: `style`, `color`, `highlight`, `size`, `subscript`, `superscript`, `bold`, `italic`, `underline`, `strike`, `font`, `url_id`, `rtl`, `lang`

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `RichText(text=None, **text_prop)`                          | ctor     | start an inline run sequence                     |
|  [02]   | `RichText.add(text, ...)`                                   | instance | append a styled run over the appearance axis     |
|  [03]   | `RichTextParagraph(text=None, **text_prop)`                 | ctor     | start a styled paragraph sequence                |
|  [04]   | `RichTextParagraph.add(text, parastyle=None)`               | instance | append a paragraph wrapping a `RichText`         |
|  [05]   | `Listing(text)`                                             | ctor     | escape text keeping `\n`/`\a` and para style     |
|  [06]   | `InlineImage(tpl, image_descriptor, width, height, anchor)` | ctor     | bind an inline image to `tpl` with `Length` size |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `DocxTemplate` owns load, render, and save; `template_file` (path, `PathLike`, or stream) is the constructor argument, never a per-source builder type, and `jinja_env`/`autoescape` are call rows on `render`, never parallel render entrypoints.
- Styled content is a carrier placed in the context, never a hand-built `python-docx` run: `RichText`/`R` own inline runs, `RichTextParagraph`/`RP` styled paragraphs keyed by `parastyle`, `Listing` newline/page-break-preserving text, `InlineImage` inline images bound via `tpl`.
- A bare `save` (no prior `render`) reloads the template and applies only the `replace_*` swaps, which are keyed by part kind, never a parallel template type per media class.
- `build_url_id(url)` returns the `url_id` a `RichText.add(url_id=)` or `InlineImage(anchor=)` references, never an inline hyperlink string.
- `DocxTemplate.docx` (via `get_docx`) is the underlying `python-docx` `Document`; structural work the template cannot express routes through it, never a hand-built OOXML string.

[STACKING]:
- `document/emit#DOCUMENT`(`emit.md`): `DocumentMode.DOCX_TEMPLATE` resolves to the `_docxtpl_emit` arm, which lowers the `document/model#NODE` `DocumentNode` tree into the role-keyed `render` context, calls `render` → `render_footnotes` → `save`, and mints the `core/receipt#RECEIPT` `ArtifactReceipt.Office(key, bytes)` case through the `@receipted` weave; the emit owner holds the engine.
- `jinja2`(`.api/jinja2.md`): `render`/`render_footnotes`/`render_properties` accept a shared `jinja2.Environment` — pass the engine used for the HTML/PDF report body so filters, globals, and the autoescape/undefined policy match across branches; `get_undeclared_template_variables(jinja_env=)` gates the context before binding.
- `python-docx`(`.api/python-docx.md`): `DocxTemplate.docx` exposes the `Document` for residual structural work, and the emit arm's `_docx_run_props` mirrors the `RichText.add` axis onto `python-docx` `Run.font` for the non-template `DOCX` mode, single-sourcing the appearance contract across both office arms.
- `pyvips`(`.api/pyvips.md`) / `pillow`(`.api/pillow.md`): `InlineImage(width, height)` takes a `python-docx` `Length`; a pyvips/pillow-prepared raster feeds the `image_descriptor`, never a re-encoded blob.
- `msoffcrypto-tool`(`.api/msoffcrypto-tool.md`): the saved `.docx` routes here for ECMA-376 encryption when a protected deliverable is required.
- universal rail: the emit op threads the shared `libs/python/.api` rails on top of this package — `EmitSpec`/`EmitFact` are `msgspec.Struct` admitted through a `pydantic` `TypeAdapter`, the boundary is `@beartype`-validated, a provider raise (`jinja2.TemplateError`, `lxml.etree.XMLSyntaxError`) converts to `BoundaryFault` at the `async_boundary` capsule, and the `@receipted` weave carries the `structlog` event and `opentelemetry` span; `docxtpl` is pure-sync and contributes no rail.

[LOCAL_ADMISSION]:
- Pure-Python wheel, LGPL-2.1 weak copyleft, dynamic-link-safe for an internalized owner; admitted for the `document` rail.
- Import lazily at boundary scope (`from docxtpl import DocxTemplate, InlineImage, Listing, RichText, RichTextParagraph`); `R`/`RP` stay aliases, never imported as distinct names.
- `new_subdoc`/`Subdoc` and its optional `docxcompose` dependency stay out of admission; the `DocumentNode` carriers cover styled content without sub-document composition.

[RAIL_LAW]:
- Package: `docxtpl`
- Owns: DOCX jinja2 templating; context render into body/header/footer XML, footnote-part (`render_footnotes`), and core-property-part (`render_properties`) substitution; styled inline/paragraph/listing/image carriers; external-hyperlink registration (`build_url_id`); pre-render part swaps (`replace_*`); undeclared-variable inspection; rendered-document emit.
- Accept: template render and save feeding the `document/emit#DOCUMENT` owner with carriers lowered from the `DocumentNode` tree; the `ArtifactReceipt.Office` case as the receipt.
- Reject: wrapper-renames of `render`/`save`; hand-rolled `python-docx` run/paragraph stitching where a carrier owns the styling; a parallel template type per source or media class; aliasing `R`/`RP` into new types; a `Subdoc`/`docxcompose` dependency the carrier path does not need; identity minting the runtime owns.
