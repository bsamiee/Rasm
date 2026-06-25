# [PY_ARTIFACTS_API_TYPST]

`typst` supplies the Rust-backed Typst typesetting surface for the artifacts documents rail: a free-function family that compiles, evaluates, and queries Typst markup into PDF/PNG/SVG/HTML output plus a reusable `Compiler` that caches fonts and world state across renders, with no LaTeX toolchain or external process. The package owner composes `compile`, `query`, and the `Compiler` class into the `DocumentMode.PDF_TYPST` path; it never re-implements the Typst layout engine the embedded compiler already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `typst`
- package: `typst`
- import: `typst`
- owner: `artifacts`
- rail: documents
- license: Apache-2.0 (PyO3 binding over the bundled Rust Typst compiler + `comemo` world cache; no LaTeX/Node/external process)
- asset: runtime library (Rust extension); `cp38-abi3` wheel, `Requires-Python>=3.8`, cp315-clean (no `python_version` marker)
- version: `0.15.0` (lockfile-resolved; imports and runs on cp315; the wheel bundles the Typst `0.13`-series compiler)
- entry points: none (library only)
- capability: Typst markup compilation to PDF/PNG/SVG/HTML, expression evaluation, document querying, PDF/A and PDF/UA standard selection, system-input (`sys.inputs`) injection, font-path/system-font control, deterministic-timestamp control, registry package-path/cache control, resolved-font enumeration, and a font/world-caching reusable `Compiler`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: compiler and font roots
- rail: documents

The free functions construct a single-shot `Compiler` internally; the owner holds a `Compiler` when amortizing font loading across many renders. `Fonts`/`FontInfo` expose the resolved font set; `TypstError`/`TypstWarning` are the diagnostic carriers raised and surfaced by the compile path. The module surface is exactly `{compile, compile_with_warnings, eval, query, Compiler, Fonts, FontInfo, TypstError, TypstWarning}` — there is no per-format render function; `format`/`output`/`pdf_standards` are rows on the one compile surface.

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]   | [RAIL]                                                                                       |
| :-----: | :------------- | :-------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `Compiler`     | compiler        | reusable font/world-cached document compiler                                                 |
|  [02]   | `Fonts`        | font set        | resolved font collection: `.families` (family-name list), `.fonts` (`FontInfo` list)          |
|  [03]   | `FontInfo`     | font descriptor | one resolved font face: `.family`, `.index`, `.path`, `.stretch`, `.style`, `.weight`         |
|  [04]   | `TypstError`   | diagnostic      | compile failure carrier (`RuntimeError` subclass; carries the rendered Typst diagnostic message) — raised by the compile path |
|  [05]   | `TypstWarning` | diagnostic      | compile warning carrier (`UserWarning` subclass, a warnings category — collected, never raised; element of the `compile_with_warnings` list) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: module free functions
- rail: documents

The free functions take `input` (source path or `bytes`) and share `root`, `font_paths`, `ignore_system_fonts`, `sys_inputs`, `pdf_standards`, `package_path`, and `package_cache_path` policy; `output` of `None` returns rendered `bytes`. `format` is `"pdf"`/`"png"`/`"svg"`/`"html"` (inferred from the `output` extension when `None`); `ppi` only affects `png`; `pdf_standards` is a string/sequence of PDF-conformance tokens — the bundled compiler accepts `"1.7"`, `"2.0"`, `"a-1b"`, `"a-2b"`, `"a-3b"`, `"a-3u"`, and `"ua-1"` (combinable, e.g. `["a-3b", "ua-1"]` for an archival+accessible render); a `"ua-1"` render hard-errors `missing document title` unless the source sets `document(title: ..)`, so the lowering always emits a title for the PDF/UA path; `sys_inputs` is a `dict[str, str]` exposed to the document as `sys.inputs`; `timestamp` is an epoch int that pins the document's creation time for reproducible byte output.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                                                                                                                                                                                            | [CAPABILITY]                                       |
| :-----: | :---------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `compile`               | `compile(input, output=None, root=None, font_paths=..., ignore_system_fonts=False, format=None, ppi=None, sys_inputs=..., pdf_standards=..., package_path=None, timestamp=None, pretty=False, package_cache_path=None)` | compile markup to PDF/PNG/SVG/HTML `bytes` or file |
|  [02]   | `compile_with_warnings` | same signature as `compile` -> `(output, warnings)`                                                                                                                                                                     | compile and return collected warnings              |
|  [03]   | `eval`                  | `eval(input, expression, format=None, pretty=False, root=None, font_paths=..., ignore_system_fonts=False, sys_inputs=..., package_path=None, package_cache_path=None)`                                                  | evaluate a Typst expression against a document     |
|  [04]   | `query`                 | `query(input, selector, field=None, one=False, format=None, root=None, font_paths=..., ignore_system_fonts=False, sys_inputs=..., package_path=None, package_cache_path=None)`                                          | query document elements by selector                |

[ENTRYPOINT_SCOPE]: `Compiler` methods
- rail: documents

The `Compiler` constructor carries `input`, `root`, `font_paths`, `ignore_system_fonts`, `sys_inputs`, `package_path`, and `package_cache_path`; methods amortize font loading across repeated renders of one world.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                                                                                                          | [CAPABILITY]                          |
| :-----: | :------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------ | :------------------------------------ |
|  [01]   | `Compiler.compile`               | `compile(input=None, output=None, format=None, ppi=None, sys_inputs=..., pdf_standards=..., root=None, timestamp=None, pretty=False)` | compile against the cached world      |
|  [02]   | `Compiler.compile_with_warnings` | same signature -> `(output, warnings)`                                                                                                | compile and return collected warnings |
|  [03]   | `Compiler.eval`                  | `eval(expression, format=None, pretty=False, root=None)`                                                                              | evaluate an expression                |
|  [04]   | `Compiler.query`                 | `query(selector, field=None, one=False, format=None, root=None)`                                                                      | query elements by selector            |

[MARKUP_ELEMENT_SCOPE]: tagged-PDF accessibility elements (bundled-compiler markup vocabulary the lowering emits)
- rail: documents

The compiled Typst source the owner emits drives these built-in markup functions; the lowering authors the `alt` text equivalent so the `format="pdf"` render with a PDF/UA `pdf_standards` row writes the marked-content structure element. Typst guidance pins `alt` to the inner `image` when the figure body is an image with its own description; the figure-level `alt` is reserved for figures whose body is custom content, never doubled with an image `alt`. The bundled `pdf` module exposes exactly `attach` and `artifact` (there is no `pdf.embed` member in this compiler) — `pdf.attach` is the associated-file embed and `pdf.artifact` wraps decorative content so the structure tree skips it.

| [INDEX] | [MARKUP]    | [CALL_SHAPE]                                                                                                     | [CAPABILITY]                                      |
| :-----: | :---------- | :--------------------------------------------------------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `image`     | `image(source: str\|bytes\|path, format=auto, width=auto, height=auto, alt: none\|str = none, fit="cover", ...)` | embed graphic with screen-reader `alt` equivalent |
|  [02]   | `figure`    | `figure(body: content, alt: none\|str = none, caption: none\|content = none, kind=auto, supplement=auto, ...)`   | numbered figure block carrying caption + role     |
|  [03]   | `heading`   | `heading(body: content, level: int = 1, ...)`                                                                    | outline node lowering the `H1`-`H6` structure     |
|  [04]   | `table`      | `table(columns, ..., ..children: content)`                                                                      | row-major cell grid lowering the table structure  |
|  [05]   | `pdf.attach` | `pdf.attach(path: str, relationship: auto\|str = auto, mime-type: none\|str = none, description: none\|str = none)` | associated-file embed for tagged-PDF attachments (path/string source, not `bytes`) |
|  [06]   | `pdf.artifact` | `pdf.artifact(kind: str = "other", body: content)`                                                            | mark decorative/non-semantic content as a PDF artifact, excluded from the tagged structure tree so AT skips it |

## [04]-[IMPLEMENTATION_LAW]

[DOCUMENT_PDF_TYPST]:
- import: `import typst` at boundary scope only; module-level import is banned by the manifest import policy.
- compile axis: `compile`/`compile_with_warnings` is one render surface keyed by `format` and `output`; PDF/PNG/SVG/HTML is the `format` row, not a per-format function, and `output=None` returns `bytes` for the receipt path.
- compiler axis: `Compiler` is the reusable world; the owner holds one `Compiler` to amortize `font_paths`/`sys_inputs` across batched renders, never a fresh compiler per document.
- standard axis: `pdf_standards` selects the archival PDF/A target; archival conformance is a render row, never a parallel signer path — PAdES signing routes to `pyhanko`.
- query axis: `query`/`eval` answer the document-introspection question over `selector`/`field`; structured extraction is a row, never a re-parsed AST.
- markup axis: the lowering authors `image(.., alt=..)` for embedded graphics and `figure(.., caption: ..)` for numbered blocks; the `alt` equivalent rides the inner `image`, never doubled onto the enclosing `figure`, so a PDF/UA `pdf_standards` render writes one marked-content structure element per figure. Decorative content (rules, background fills, repeated ornaments) is wrapped in `pdf.artifact[..]` so it is excluded from the tagged structure tree, and source/data files ride `pdf.attach(path, relationship: .., description: ..)` as associated files rather than a phantom `pdf.embed`; a `pdf.attach` under a PDF/A `pdf_standards` row hard-errors `the file mime type is missing` unless `mime-type:` is supplied, so the archival lowering always sets it. Interpolated `alt`/source strings are Typst-string-escaped (`\` and `"`) before emission; raw interpolation of an `alt` carrying a quote yields invalid markup.
- reproducibility axis: `timestamp` pins the document creation date and `ignore_system_fonts=True` + explicit `font_paths` pins the font set, so a PDF/A render is byte-deterministic across machines; the owner sets both for archival output.
- evidence: each render captures source identity, output format, page/byte count, PDF standard, resolved font set (`Fonts.fonts`), and collected warnings as a document receipt.
- boundary: typst owns Typst markup typesetting; reportlab/weasyprint own their own document models; raster post-processing routes to `pillow`; PAdES signing routes to `pyhanko`; live UI stays outside this package.

[STACK_INTEGRATION]:
- `typst` -> `vl_convert` figure rail: a chart rendered by `vl_convert.vegalite_to_svg` is embedded into the Typst source via `image(<svg-bytes>, alt: ..)`; Typst lays out and paginates the figure, and the one `format="pdf"` render writes the chart plus the surrounding tagged document — no second PDF merge step.
- `typst` font set vs the shaping rail: `Fonts.fonts` (resolved `FontInfo` faces) is the same OTF/TTF the `fonttools`/`uharfbuzz` rails own; the owner registers the document's fonts via `font_paths` and reads back `Fonts` to confirm the resolved face matches the shaped/subsetted font before an archival render.
- `typst` -> `pyhanko` signing: the archival PDF/A bytes from a `pdf_standards` render are handed to the `pyhanko` PAdES signer as input; signing is never minted here — `typst` produces the conformant unsigned PDF, `pyhanko` owns the signature.

[RAIL_LAW]:
- Package: `typst`
- Owns: Typst markup compilation to PDF/PNG/SVG/HTML, expression evaluation, document querying, PDF/A standard selection, and font/world caching via `Compiler`
- Accept: Typst-source document production feeding the `DocumentMode.PDF_TYPST` and document owners
- Reject: wrapper-renames of `compile`/`query`; a per-format render function where `format` is a row; a fresh `Compiler` per render in a batch; a re-minted signer where `pyhanko` owns PAdES; identity minting the runtime owns
