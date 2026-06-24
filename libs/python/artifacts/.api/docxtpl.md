# [PY_ARTIFACTS_API_DOCXTPL]

`docxtpl` supplies the DOCX templating surface for the artifacts document rail: a `DocxTemplate` that loads a `.docx` as a jinja2 template, `render` substitutes a context dict into the document body, headers, and footers, and `save` emits the rendered `.docx`. The package owner composes `DocxTemplate.render` and `DocxTemplate.save` into the `DOCX_TEMPLATE_BIND` path; it removes ad-hoc `python-docx` paragraph-stitching because the template carries the styled layout, and it never re-implements the Office Open XML run/paragraph serialization that `RichText`, `RichTextParagraph`, `Listing`, and `InlineImage` already own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `docxtpl`
- package: `docxtpl`
- import: `docxtpl`
- owner: `artifacts`
- rail: document
- license: LGPL-2.1 (runtime deps `python-docx`, `jinja2`; optional `docxcompose` for `Subdoc`)
- asset: runtime library; pure Python (`py3-none-any`), no ABI gate, cp315-clean (manifest unpinned, no `python_version` marker)
- installed: `0.20.2` reflected via `assay api resolve docxtpl`
- entry points: library use is import-only; `python -m docxtpl <template> <json_context> <output>` runs the module CLI in `__main__`
- capability: load a `.docx` as a jinja2 template, render a context dict into body/header/footer XML and (separately) footnote-part XML via `render_footnotes`, inject styled inline rich text via `RichText`, styled paragraphs via `RichTextParagraph`, newline-preserving listings via `Listing`, and inline images via `InlineImage`, compose full sub-documents via `new_subdoc`, swap embedded media/parts before render, reach the underlying `python-docx` `Document` via `.docx` for structural pre/post work, then save the rendered document or list its undeclared template variables

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: template root and content carriers
- rail: document

`DocxTemplate` is the template root; `RichText`/`RichTextParagraph`/`Listing`/`InlineImage` are content carriers passed through the render context. `R` is an exact alias of `RichText` and `RP` an exact alias of `RichTextParagraph`. `new_subdoc` returns a `Subdoc` that composes a full sub-document into the render context; `Subdoc` requires the optional `docxcompose` dependency and is not a package export.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [RAIL]                                                        |
| :-----: | :------------------ | :-------------- | :------------------------------------------------------------ |
|  [01]   | `DocxTemplate`      | template root   | load, render, and save a `.docx` jinja2 template              |
|  [02]   | `RichText`          | content carrier | inline styled run sequence inside an existing paragraph       |
|  [03]   | `R`                 | content carrier | alias of `RichText`                                           |
|  [04]   | `RichTextParagraph` | content carrier | styled paragraph sequence outside an existing paragraph       |
|  [05]   | `RP`                | content carrier | alias of `RichTextParagraph`                                  |
|  [06]   | `Listing`           | content carrier | newline/page-break-preserving text keeping template styling   |
|  [07]   | `InlineImage`       | content carrier | inline image bound to a template part with optional hyperlink |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `DocxTemplate` render and emit
- rail: document

`DocxTemplate(template_file)` accepts a path, `PathLike`, or binary stream. `render` mutates the loaded document in place; `save` flushes the rendered document to the target. `get_undeclared_template_variables` runs jinja2 meta analysis to list variables the template references. `new_subdoc` mints a `Subdoc` for full sub-document composition. The `replace_*` rows swap embedded media/files inside the package before render.

| [INDEX] | [SURFACE]                                        | [CALL_SHAPE]                                                                                                                       | [CAPABILITY]                                        |
| :-----: | :----------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `DocxTemplate.__init__`                          | `DocxTemplate(template_file: Union[IO[bytes], str, PathLike]) -> None`                                                             | load a `.docx` as a jinja2 template                 |
|  [02]   | `DocxTemplate.render`                            | `render(context: Dict[str, Any], jinja_env: Optional[Environment] = None, autoescape: bool = False) -> None`                       | substitute context into body/header/footer XML      |
|  [03]   | `DocxTemplate.render_footnotes`                  | `render_footnotes(context: Dict[str, Any], jinja_env: Optional[Environment] = None) -> None`                                       | substitute context into footnote-part XML (call after `render`) |
|  [04]   | `DocxTemplate.save`                              | `save(filename: Union[IO[bytes], str, PathLike], *args, **kwargs) -> None`                                                         | write the rendered document to path or stream       |
|  [05]   | `DocxTemplate.get_undeclared_template_variables` | `get_undeclared_template_variables(jinja_env: Optional[Environment] = None, context: Optional[Dict[str, Any]] = None) -> Set[str]` | list jinja2 variables the template references       |
|  [06]   | `DocxTemplate.new_subdoc`                        | `new_subdoc(docpath=None) -> Subdoc`                                                                                               | mint a sub-document carrier for the context         |
|  [07]   | `DocxTemplate.build_url_id`                      | `build_url_id(url)`                                                                                                                | register a hyperlink and return its `url_id`        |
|  [08]   | `DocxTemplate.docx` / `get_docx`                 | `docx` attribute (the `python-docx` `Document`); `get_docx() -> Document` forces `init_docx()` then returns it                    | reach the underlying `python-docx` `Document` for structural pre/post work |
|  [09]   | `DocxTemplate.replace_media`                     | `replace_media(src_file, dst_file)`                                                                                                | swap a media part by source file before render      |
|  [10]   | `DocxTemplate.replace_embedded`                  | `replace_embedded(src_file, dst_file)`                                                                                             | swap an embedded part before render                 |
|  [11]   | `DocxTemplate.replace_zipname`                   | `replace_zipname(zipname, dst_file)`                                                                                               | swap a zip-entry part by archive name before render |
|  [12]   | `DocxTemplate.replace_pic`                       | `replace_pic(embedded_file, dst_file)`                                                                                             | swap a picture part before render                   |

[ENTRYPOINT_SCOPE]: content carriers
- rail: document

Carriers serialize to OOXML and stringify into the render context. `RichText`/`R` accumulate styled runs via `add`; `RichTextParagraph`/`RP` accumulate styled paragraphs via `add`; `Listing` escapes text while preserving `\n` and `\a`; `InlineImage` binds an image descriptor to the template and stringifies to the inline-drawing XML.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                                                                                                                                                                            | [CAPABILITY]                                           |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :----------------------------------------------------- |
|  [01]   | `RichText.__init__`          | `RichText(text=None, **text_prop)`                                                                                                                                                                      | start an inline run sequence with optional first run   |
|  [02]   | `RichText.add`               | `add(text, style=None, color=None, highlight=None, size=None, subscript=None, superscript=None, bold=False, italic=False, underline=False, strike=False, font=None, url_id=None, rtl=False, lang=None)` | append a styled run                                    |
|  [03]   | `RichTextParagraph.__init__` | `RichTextParagraph(text=None, **text_prop)`                                                                                                                                                             | start a styled paragraph sequence                      |
|  [04]   | `RichTextParagraph.add`      | `add(text, parastyle=None)`                                                                                                                                                                             | append a paragraph wrapping a `RichText`               |
|  [05]   | `Listing.__init__`           | `Listing(text)`                                                                                                                                                                                         | escape text while keeping `\n`/`\a` and template style |
|  [06]   | `InlineImage.__init__`       | `InlineImage(tpl, image_descriptor, width=None, height=None, anchor=None)`                                                                                                                              | bind an inline image with optional size and hyperlink  |

## [04]-[IMPLEMENTATION_LAW]

[DOCUMENT_DOCX_TEMPLATE]:
- import: `from docxtpl import DocxTemplate, RichText, RichTextParagraph, Listing, InlineImage` at boundary scope only; module-level import is banned by the manifest import policy.
- template axis: one `DocxTemplate` owns load, render, and emit; `template_file` accepts path/`PathLike`/binary stream as a constructor argument, never a per-source builder type. `render` is the single substitution surface keyed by the `context` dict; `jinja_env` and `autoescape` are call rows, never parallel render entrypoints.
- content axis: styled content is a carrier placed in the context, never a hand-built `python-docx` run. `RichText`/`R` own inline runs through `add` rows; `RichTextParagraph`/`RP` own styled paragraphs through `add`; `Listing` owns newline/page-break-preserving text; `InlineImage` owns inline images bound to the template via `tpl`. `R`/`RP` are aliases, not parallel types.
- emit axis: `save` is the single serializer keyed by `filename` (path or stream); `render_footnotes` is the companion substitution surface for footnote-part XML (called after `render` on the same context/`jinja_env`); `get_undeclared_template_variables` is the inspection row that lists referenced variables before binding.
- replacement axis: `replace_media`/`replace_embedded`/`replace_zipname`/`replace_pic` are pre-render part-swap rows keyed by part kind, never a parallel template type per media class.
- structural axis: `DocxTemplate.docx` is the underlying `python-docx` `Document` (lazily built by `init_docx`/`get_docx`) — structural pre/post work (section/style/numbering edits the template cannot express) goes through this `python-docx` surface (`.api/python-docx.md`), never a hand-built OOXML string.
- evidence: each render captures the resolved template path, the context key set, the undeclared-variable set, the carrier kinds injected, the output path, and the output byte length as a document receipt.
- boundary: `docxtpl` owns DOCX templating and OOXML run/paragraph/image serialization over `python-docx`; `new_subdoc`/`Subdoc` full sub-document composition requires the optional `docxcompose` dependency; raster image preparation routes to `pillow` only when the descriptor needs it; the rendered `.docx` feeds the document owner directly; live UI stays outside this package.

[STACKING]:
- jinja2 seam: `render(context, jinja_env=...)` accepts a shared `jinja2.Environment` (`.api/jinja2.md`) — pass the same engine used for the HTML/PDF report body so custom filters, globals, and the autoescape/undefined policy are identical across the DOCX and HTML branches; `get_undeclared_template_variables(jinja_env=...)` runs the same engine's meta analysis to gate the context before binding.
- python-docx seam: the template carries the styled layout; `DocxTemplate.docx` exposes the `python-docx` `Document` for the residual structural work, and the content carriers (`RichText`/`RichTextParagraph`/`Listing`/`InlineImage`) replace any hand-built `python-docx` run/paragraph stitching inside the context values.
- image seam: `InlineImage(tpl, image_descriptor, width=Mm(...)/Pt(...), height=...)` takes a `python-docx` `Length`-typed `width`/`height`; the `image_descriptor` is a path or stream that `python-docx` (over `pillow`) decodes — a `pyvips`/`pillow`-prepared raster feeds the descriptor, never a re-encoded blob.
- confidentiality seam: the saved `.docx` bytes route to `msoffcrypto-tool` (`.api/msoffcrypto-tool.md`) for ECMA-376 encryption when the document owner requires a protected deliverable.

[RAIL_LAW]:
- Package: `docxtpl`
- Owns: DOCX jinja2 templating, context render into body/header/footer XML, styled inline/paragraph/listing/image carriers, undeclared-variable inspection, and rendered-document emit
- Accept: template render and save feeding the document owner with styled content carriers
- Reject: wrapper-renames of `render`/`save`; hand-rolled `python-docx` run/paragraph stitching where a carrier owns the styling; a parallel template type per source kind or per media class; aliasing `R`/`RP` into new types; identity minting the runtime owns
