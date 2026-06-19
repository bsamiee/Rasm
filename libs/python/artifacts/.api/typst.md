# [PY_ARTIFACTS_API_TYPST]

`typst` supplies the Rust-backed Typst typesetting surface for the artifacts documents rail: a free-function family that compiles, evaluates, and queries Typst markup into PDF/PNG/SVG/HTML output plus a reusable `Compiler` that caches fonts and world state across renders, with no LaTeX toolchain or external process. The package owner composes `compile`, `query`, and the `Compiler` class into the `DocumentMode.PDF_TYPST` path; it never re-implements the Typst layout engine the embedded compiler already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `typst`
- package: `typst`
- import: `typst`
- owner: `artifacts`
- rail: documents
- installed: `0.15.0` reflected via `python -c "import typst"` on cp315
- entry points: none (library only)
- capability: Typst markup compilation to PDF/PNG/SVG/HTML, expression evaluation, document querying, PDF/A standard selection, system-input injection, font-path control, and a font-caching reusable `Compiler`

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: compiler and font roots
- rail: documents

The free functions construct a single-shot `Compiler` internally; the owner holds a `Compiler` when amortizing font loading across many renders. `Fonts`/`FontInfo` expose the resolved font set; `TypstError`/`TypstWarning` are the diagnostic carriers raised and surfaced by the compile path.

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]   | [RAIL]                                        |
| :-----: | :------------- | :-------------- | :-------------------------------------------- |
|   [1]   | `Compiler`     | compiler        | reusable font-cached document compiler        |
|   [2]   | `Fonts`        | font set        | resolved font collection (`families`/`fonts`) |
|   [3]   | `FontInfo`     | font descriptor | one resolved font face                        |
|   [4]   | `TypstError`   | diagnostic      | compile failure carrier                       |
|   [5]   | `TypstWarning` | diagnostic      | compile warning carrier                       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: module free functions
- rail: documents

The free functions take `input` (source path or `bytes`) and share `root`, `font_paths`, `ignore_system_fonts`, `sys_inputs`, `pdf_standards`, `package_path`, and `package_cache_path` policy; `output` of `None` returns rendered `bytes`.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                                                                                                                                                                                            | [CAPABILITY]                                       |
| :-----: | :---------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------- |
|   [1]   | `compile`               | `compile(input, output=None, root=None, font_paths=..., ignore_system_fonts=False, format=None, ppi=None, sys_inputs=..., pdf_standards=..., package_path=None, timestamp=None, pretty=False, package_cache_path=None)` | compile markup to PDF/PNG/SVG/HTML `bytes` or file |
|   [2]   | `compile_with_warnings` | same signature as `compile` -> `(output, warnings)`                                                                                                                                                                     | compile and return collected warnings              |
|   [3]   | `eval`                  | `eval(input, expression, format=None, pretty=False, root=None, font_paths=..., ignore_system_fonts=False, sys_inputs=..., package_path=None, package_cache_path=None)`                                                  | evaluate a Typst expression against a document     |
|   [4]   | `query`                 | `query(input, selector, field=None, one=False, format=None, root=None, font_paths=..., ignore_system_fonts=False, sys_inputs=..., package_path=None, package_cache_path=None)`                                          | query document elements by selector                |

[ENTRYPOINT_SCOPE]: `Compiler` methods
- rail: documents

The `Compiler` constructor carries `input`, `root`, `font_paths`, `ignore_system_fonts`, `sys_inputs`, `package_path`, and `package_cache_path`; methods amortize font loading across repeated renders of one world.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                                                                                                          | [CAPABILITY]                          |
| :-----: | :------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------ | :------------------------------------ |
|   [1]   | `Compiler.compile`               | `compile(input=None, output=None, format=None, ppi=None, sys_inputs=..., pdf_standards=..., root=None, timestamp=None, pretty=False)` | compile against the cached world      |
|   [2]   | `Compiler.compile_with_warnings` | same signature -> `(output, warnings)`                                                                                                | compile and return collected warnings |
|   [3]   | `Compiler.eval`                  | `eval(expression, format=None, pretty=False, root=None)`                                                                              | evaluate an expression                |
|   [4]   | `Compiler.query`                 | `query(selector, field=None, one=False, format=None, root=None)`                                                                      | query elements by selector            |

## [4]-[IMPLEMENTATION_LAW]

[DOCUMENT_PDF_TYPST]:
- import: `import typst` at boundary scope only; module-level import is banned by the manifest import policy.
- compile axis: `compile`/`compile_with_warnings` is one render surface keyed by `format` and `output`; PDF/PNG/SVG/HTML is the `format` row, not a per-format function, and `output=None` returns `bytes` for the receipt path.
- compiler axis: `Compiler` is the reusable world; the owner holds one `Compiler` to amortize `font_paths`/`sys_inputs` across batched renders, never a fresh compiler per document.
- standard axis: `pdf_standards` selects the archival PDF/A target; archival conformance is a render row, never a parallel signer path — PAdES signing routes to `pyhanko`.
- query axis: `query`/`eval` answer the document-introspection question over `selector`/`field`; structured extraction is a row, never a re-parsed AST.
- evidence: each render captures source identity, output format, page/byte count, PDF standard, and collected warnings as a document receipt.
- boundary: typst owns Typst markup typesetting; reportlab/weasyprint own their own document models; raster post-processing routes to `pillow`; PAdES signing routes to `pyhanko`; live UI stays outside this package.

[RAIL_LAW]:
- Package: `typst`
- Owns: Typst markup compilation to PDF/PNG/SVG/HTML, expression evaluation, document querying, PDF/A standard selection, and font/world caching via `Compiler`
- Accept: Typst-source document production feeding the `DocumentMode.PDF_TYPST` and document owners
- Reject: wrapper-renames of `compile`/`query`; a per-format render function where `format` is a row; a fresh `Compiler` per render in a batch; a re-minted signer where `pyhanko` owns PAdES; identity minting the runtime owns
