# [PY_ARTIFACTS_API_DOCXTPL]

`docxtpl` supplies the DOCX templating surface for the artifacts document rail: a `DocxTemplate` that loads a `.docx` as a jinja2 template, `render` substitutes a context dict into the document body, headers, and footers, `render_footnotes`/`render_properties` extend the same substitution into the footnote part and the core-property part, and `save` emits the rendered `.docx`. The `document/emit#DOCUMENT` `DocumentMode.DOCX_TEMPLATE` arm (`_docxtpl_emit`) lowers the one `document/model#NODE` `DocumentNode` tree into a role-keyed `render` context of `RichText`/`RichTextParagraph`/`Listing`/`InlineImage` carriers, threads `replace_zipname` pre-render part swaps off the `EmitSpec.replace` band, harvests `get_undeclared_template_variables` onto the `EmitFact`, and mints the `core/receipt#RECEIPT` `ArtifactReceipt.Office(key, bytes)` case; the owner removes ad-hoc `python-docx` paragraph-stitching because the template carries the styled layout and the carriers own the run XML, and it never re-implements the Office Open XML run/paragraph serialization that `RichText`, `RichTextParagraph`, `Listing`, and `InlineImage` already own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `docxtpl`
- package: `docxtpl`
- import: `docxtpl`
- owner: `artifacts`
- rail: document
- license: LGPL-2.1-only (Eric Lapouyade) — weak copyleft, dynamic-link safe for an internalized owner; runtime deps `python-docx` (MIT), `jinja2` (BSD), `lxml` (BSD); optional `docxcompose` (MPL-2.0) only for `Subdoc`
- installed: `0.20.2`
- build floor: pure-Python wheel (`py3-none-any`), no native build, no cp-gate — installs on cp315 unconditionally; no `; python_version` marker needed
- entry points: library use is import-only; `python -m docxtpl <template_path> <json_path> <output_filename> [--overwrite] [--quiet]` runs the `__main__` CLI (the artifacts owner uses the import path, never the CLI)
- capability: load a `.docx` as a jinja2 template, render a context dict into body/header/footer XML, separately substitute into footnote-part XML via `render_footnotes` and into the core-document-property part via `render_properties`, inject styled inline rich text via `RichText`, styled paragraphs via `RichTextParagraph`, newline/page-break-preserving listings via `Listing`, and inline images via `InlineImage`, register external hyperlinks via `build_url_id`, compose full sub-documents via `new_subdoc`, swap embedded media/parts before render via the `replace_*` family, reach the underlying `python-docx` `Document` via `.docx`/`get_docx` for structural pre/post work, then save the rendered document or list its undeclared template variables

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: template root and content carriers
- rail: document

`DocxTemplate` is the template root; `RichText`/`RichTextParagraph`/`Listing`/`InlineImage` are content carriers placed in the render context, each stringifying to OOXML via `__str__`/`__html__`. `R` is an exact alias of `RichText` and `RP` an exact alias of `RichTextParagraph` (both real package exports from `docxtpl.__init__`, not new types). `new_subdoc` returns a `Subdoc` that composes a full sub-document into the context; `Subdoc` is a conditional export — `docxtpl.__init__` imports it under a `try/except ImportError`, so it is present only when the optional `docxcompose` dependency is installed, and the artifacts owner does not depend on it (the `DocumentNode` carriers cover the styled-content concern without sub-document composition).

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

`DocxTemplate(template_file)` accepts a path, `PathLike`, or binary stream and lazily materializes the `python-docx` `Document` (`init_docx`) on first need. `render` mutates the loaded document in place (body + header/footer parts); `render_footnotes` and `render_properties` are companion substitution passes for the footnote and core-property parts, both called after `render` on the same `context`/`jinja_env`. `save` flushes the rendered document to the target. `get_undeclared_template_variables` runs jinja2 meta analysis (over a throwaway `Document` so it is non-mutating) to list variables the template references. `new_subdoc` mints a `Subdoc` for full sub-document composition. `build_url_id` registers an external hyperlink relationship and returns the `url_id` the carriers reference. The `replace_*` rows swap embedded media/files inside the package before render.

Every surface is a `DocxTemplate` method; `render`/`render_footnotes`/`render_properties` share `(context, jinja_env=None)` (`render` adds `autoescape=False`).

| [INDEX] | [SURFACE]                           | [CAPABILITY]                                                                |
| :-----: | :---------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `__init__`                          | load a `.docx` as a jinja2 template (path/`PathLike`/stream)                |
|  [02]   | `render`                            | substitute context into body + header/footer XML in place                   |
|  [03]   | `render_footnotes`                  | substitute into footnote-part XML (call after `render`)                     |
|  [04]   | `render_properties`                 | render context through `core_properties` and write it back                  |
|  [05]   | `save`                              | write the rendered doc to path/stream; a bare save applies only `replace_*` |
|  [06]   | `get_undeclared_template_variables` | list jinja2 variables body+header+footer reference (non-mutating)           |
|  [07]   | `new_subdoc`                        | mint a sub-document carrier (requires `docxcompose`)                        |
|  [08]   | `build_url_id`                      | register an external hyperlink relationship, return its `url_id`            |
|  [09]   | `docx` / `get_docx`                 | reach the underlying `python-docx` `Document` for structural work           |
|  [10]   | `replace_media`                     | swap a media part by source file before render                              |
|  [11]   | `replace_embedded`                  | swap an embedded (OLE/object) part before render                            |
|  [12]   | `replace_zipname`                   | swap a zip-entry part by archive name (the `EmitSpec.replace` band)         |
|  [13]   | `replace_pic`                       | swap a picture part by its embedded filename before render                  |

Call shapes:

- [01]-[__init__]: `DocxTemplate(template_file: Union[IO[bytes], str, PathLike]) -> None`
- [02]-[RENDER]: `render(context, jinja_env=None, autoescape=False) -> None`
- [03]-[RENDER_FOOTNOTES]: `render_footnotes(context, jinja_env=None) -> None`
- [04]-[RENDER_PROPERTIES]: `render_properties(context, jinja_env=None) -> None` — writes `author`/`comments`/`identifier`/`language`/`subject`/`title`
- [05]-[SAVE]: `save(filename: Union[IO[bytes], str, PathLike], *args, **kwargs) -> None` — `*args`/`**kwargs` forward to `python-docx` `Document.save`
- [06]-[GET_UNDECLARED_TEMPLATE_VARIABLES]: `get_undeclared_template_variables(jinja_env=None, context=None) -> Set[str]`
- [07]-[NEW_SUBDOC]: `new_subdoc(docpath=None) -> Subdoc`
- [08]-[BUILD_URL_ID]: `build_url_id(url) -> str`
- [09]-[DOCX]: `docx` attribute (the `python-docx` `Document`); `get_docx() -> Document` forces `init_docx()` then returns it
- [10]-[REPLACE_MEDIA]: `replace_media(src_file, dst_file)`
- [11]-[REPLACE_EMBEDDED]: `replace_embedded(src_file, dst_file)`
- [12]-[REPLACE_ZIPNAME]: `replace_zipname(zipname, dst_file)`
- [13]-[REPLACE_PIC]: `replace_pic(embedded_file, dst_file)`

[ENTRYPOINT_SCOPE]: content carriers
- rail: document

Carriers serialize to OOXML and stringify into the render context (`__str__`/`__html__` both return the accumulated `.xml`). `RichText`/`R` accumulate styled runs via `add` (the full character-appearance axis below); a `RichText` passed as the `text` of another `add` is concatenated by XML, so carriers nest. `RichTextParagraph`/`RP` accumulate styled paragraphs via `add`, each paragraph wrapping a `RichText` under an optional `parastyle`. `Listing` escapes text while preserving `\n` (soft line break) and `\a` (page break) so the template's surrounding paragraph style is kept. `InlineImage` binds an image descriptor to the template via `tpl` and stringifies to the inline-drawing XML.

| [INDEX] | [SURFACE]                    | [CAPABILITY]                                                           |
| :-----: | :--------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `RichText.__init__`          | start an inline run sequence; `**text_prop` forwards to a first `add`  |
|  [02]   | `RichText.add`               | append a styled run over the full character-appearance axis            |
|  [03]   | `RichTextParagraph.__init__` | start a styled paragraph sequence                                      |
|  [04]   | `RichTextParagraph.add`      | append a paragraph wrapping a `RichText` under an optional `parastyle` |
|  [05]   | `Listing.__init__`           | escape text while keeping `\n`/`\a` and the template paragraph style   |
|  [06]   | `InlineImage.__init__`       | bind an inline image to `tpl` with optional `Length` size and `anchor` |

Call shapes — `RichText.add` projects the full run-appearance axis the emit `_docx_run_props` mirrors (a `RichText` argument is concatenated, a non-str is `str()`-coerced then HTML-escaped):

- [01]-[RichText.__init__]: `RichText(text=None, **text_prop)`
- [02]-[RichText.add]: `add(text, style=None, color=None, highlight=None, size=None, subscript=None, superscript=None, bold=False, italic=False, underline=False, strike=False, font=None, url_id=None, rtl=False, lang=None)`
- [03]-[RichTextParagraph.__init__]: `RichTextParagraph(text=None, **text_prop)`
- [04]-[RichTextParagraph.add]: `add(text, parastyle=None)`
- [05]-[Listing.__init__]: `Listing(text)`
- [06]-[InlineImage.__init__]: `InlineImage(tpl, image_descriptor, width=None, height=None, anchor=None)`

## [04]-[IMPLEMENTATION_LAW]

[DOCUMENT_DOCX_TEMPLATE]:
- import: `lazy from docxtpl import DocxTemplate, InlineImage, Listing, RichText, RichTextParagraph` at boundary scope only (the `document/emit#DOCUMENT` lazy import block); module-level import is banned by the manifest import policy. `R`/`RP` are aliases — never import them as distinct names.
- template axis: one `DocxTemplate` owns load, render, and emit; `template_file` accepts path/`PathLike`/binary stream as a constructor argument, never a per-source builder type. `render(context, jinja_env=, autoescape=)` is the single body+header/footer substitution surface keyed by the `context` dict; `jinja_env` and `autoescape` are call rows, never parallel render entrypoints.
- content axis: styled content is a carrier placed in the context, never a hand-built `python-docx` run. `RichText`/`R` own inline runs through `add` rows projecting the full appearance axis; `RichTextParagraph`/`RP` own styled paragraphs through `add` keyed by `parastyle`; `Listing` owns newline/page-break-preserving text; `InlineImage` owns inline images bound to the template via `tpl`. The emit arm builds the role-keyed context by lowering the `DocumentNode` tree: a run node `→ context.setdefault(role, RichText()).add(child.text, **_docx_run_props(...))`, a listing block `→ Listing("".join(run.text ...))`, a styled paragraph block `→ RichTextParagraph().add(RichText(run.text, ...), parastyle=block.value)`, a figure `→ InlineImage(...)` — one `walk`+`match` pass, never a parallel `python-docx` stitch.
- emit axis: `save(filename, *args, **kwargs)` is the single serializer keyed by `filename` (path or stream); a `save` without a prior `render` reloads the template and applies only the `replace_*` swaps. `render_footnotes(context, jinja_env=)` is the companion footnote-part substitution and `render_properties(context, jinja_env=)` the companion core-property substitution — both called after `render` on the same `context`/`jinja_env` (the consumer threads `render_footnotes` after `render`; `render_properties` is the docProps-templating row for context-driven `author`/`title`/`subject`/`comments`/`identifier`/`language`). `get_undeclared_template_variables(jinja_env=)` is the inspection row harvested onto the `EmitFact` before binding.
- hyperlink axis: `build_url_id(url)` registers one external hyperlink relationship and returns the `url_id` a `RichText.add(url_id=...)` or `InlineImage(anchor=...)` references — the run/image is the carrier, the URL relationship is a template-level registration, never an inline `<a>` string.
- replacement axis: `replace_media`/`replace_embedded`/`replace_zipname`/`replace_pic` are pre-render part-swap rows keyed by part kind, never a parallel template type per media class. The emit owner wires `replace_zipname(name, dst)` off the `EmitSpec.replace: frozendict[str, str]` band before `render`.
- structural axis: `DocxTemplate.docx` is the underlying `python-docx` `Document` (lazily built by `init_docx`/`get_docx`) — structural pre/post work (section/style/numbering edits the template cannot express) goes through this `python-docx` surface (`.api/python-docx.md`), never a hand-built OOXML string.
- evidence: each render captures the resolved template path, the context key set, the `get_undeclared_template_variables` set, the carrier kinds injected, the output path, and the output byte length onto the `EmitFact`; the `core/receipt#RECEIPT` `ArtifactReceipt.Office(key, bytes)` case carries only the content-keyed byte scalar, the rich set riding `EmitFact`.
- boundary: `docxtpl` owns DOCX templating and OOXML run/paragraph/image serialization over `python-docx`; `new_subdoc`/`Subdoc` full sub-document composition requires the optional `docxcompose` dependency (absent in the artifacts admission); raster image preparation routes to `pyvips`/`pillow` only when the `InlineImage` descriptor needs it; the rendered `.docx` feeds the `document/emit#DOCUMENT` owner directly; live UI stays outside this package.

[STACKING]:
- emit-seam (the real consumer): `document/emit#DOCUMENT` `DocumentMode.DOCX_TEMPLATE` resolves to the `_docxtpl_emit` arm in the `BACKENDS` table as `Backend(Band.CORE, _docxtpl_emit, ReceiptKind.OFFICE)`; the arm lowers the `document/model#NODE` `DocumentNode` tree to the role-keyed `render` context, calls `render` → `render_footnotes` → `save`, and the `@receipted(_REDACTION)` harvest weave drains `DocumentPlan.contribute` into the `ArtifactReceipt.Office` case. `document/emit#DOCUMENT` `DocumentPlan.bound` reaches this same arm through its per-mode row `(DocumentMode.DOCX_TEMPLATE, _office_spec)` row — it mints no `DocxTemplate` of its own, the emit owner holds the engine.
- jinja2 seam: `render(context, jinja_env=...)`, `render_footnotes(context, jinja_env=...)`, and `render_properties(context, jinja_env=...)` accept a shared `jinja2.Environment` (`.api/jinja2.md`) — pass the same engine used for the HTML/PDF report body so custom filters, globals, and the autoescape/undefined policy are identical across the DOCX and HTML branches; `get_undeclared_template_variables(jinja_env=...)` runs the same engine's meta analysis to gate the context before binding (omitting `jinja_env` falls back to a default `Environment()`).
- python-docx seam: the template carries the styled layout; `DocxTemplate.docx` exposes the `python-docx` `Document` (`.api/python-docx.md`) for the residual structural work, and the content carriers (`RichText`/`RichTextParagraph`/`Listing`/`InlineImage`) replace any hand-built `python-docx` run/paragraph stitching inside the context values. The emit arm's `_docx_run_props` mirrors the `RichText.add` axis onto `python-docx` `Run.font` for the non-template `DOCX` mode, so the appearance contract is single-sourced across both office arms.
- image seam: `InlineImage(tpl, image_descriptor, width=Mm(...)/Pt(...), height=...)` takes a `python-docx` `Length`-typed `width`/`height`; the `image_descriptor` is a path or stream that `python-docx` (over `pillow`) decodes — a `pyvips` (`.api/pyvips.md`)/`pillow` (`.api/pillow.md`)-prepared raster feeds the descriptor, never a re-encoded blob.
- confidentiality seam: the saved `.docx` bytes route to `msoffcrypto-tool` (`.api/msoffcrypto-tool.md`) for ECMA-376 encryption when the document owner requires a protected deliverable.
- universal-rail seam: the emit op threads the shared `libs/python/.api` rails ON TOP of this folder package — the `EmitSpec`/`EmitFact` payload/evidence carriers are `msgspec.Struct(frozen=True)` (`libs/python/.api/msgspec.md`) admitted once through a `pydantic` `TypeAdapter` (`libs/python/.api/pydantic.md`), the boundary is `@beartype`-validated (`libs/python/.api/beartype.md`), a `docxtpl` provider raise (`jinja2.TemplateError` from a malformed template, `lxml.etree.XMLSyntaxError` from a corrupt part) converts to the runtime `BoundaryFault` at the `async_boundary` capsule rather than escaping, and the `@receipted` weave's `Signals.emit_async` carries the `structlog` event (`libs/python/.api/structlog.md`) and the `opentelemetry` span (`libs/python/.api/opentelemetry-api.md`) over the in-process render — `docxtpl` itself is pure-sync and contributes no rail, the universal tier wraps it.

[RAIL_LAW]:
- Package: `docxtpl`
- Owns: DOCX jinja2 templating, context render into body/header/footer XML plus footnote-part (`render_footnotes`) and core-property-part (`render_properties`) substitution, styled inline/paragraph/listing/image carriers, external-hyperlink registration (`build_url_id`), pre-render part swaps (`replace_*`), undeclared-variable inspection, and rendered-document emit
- Accept: template render and save feeding the `document/emit#DOCUMENT` owner with styled content carriers lowered from the `DocumentNode` tree, the `ArtifactReceipt.Office` case the receipt
- Reject: wrapper-renames of `render`/`save`; hand-rolled `python-docx` run/paragraph stitching where a carrier owns the styling; a parallel template type per source kind or per media class; aliasing `R`/`RP` into new types; a phantom bind name where `DocumentMode.DOCX_TEMPLATE`/`_docxtpl_emit` is the real consumer; a `Subdoc`/`docxcompose` dependency the carrier path does not need; identity minting the runtime owns
