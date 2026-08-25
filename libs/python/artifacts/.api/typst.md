# [PY_ARTIFACTS_API_TYPST]

`typst` compiles Typst markup to PDF/PNG/SVG/HTML through a statically-linked Rust typesetting engine — no LaTeX, Node, or external process — with expression `eval`, document `query`, a font/world-cached `Compiler`, and a `Fonts`/`FontInfo` resolved-face pair. That engine owns the markup vocabulary, the PDF/A + PDF/UA export profiles, and the tagged-PDF structure the lowering emits. `document/emit#DOCUMENT` composes the surface through its `PDF_TYPST`/`TYPST_DATA`/`TYPST_QUERY`/`TYPST_EVAL` arms; layout stays in the engine and PAdES signing routes to `pyhanko`.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: compiler, font set, diagnostics

Free functions construct a single-shot `Compiler` internally; the owner holds a `Compiler` to amortize font loading across many renders. `Fonts(include_system_fonts=True, include_embedded_fonts=True, font_paths=[])` constructs the resolved set, and `Fonts.fonts()`/`Fonts.families()` are methods.

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]   | [CAPABILITY]                                                                               |
| :-----: | :------------- | :-------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `Compiler`     | compiler        | reusable font/world-cached compiler; amortizes font discovery across batched renders       |
|  [02]   | `Fonts`        | font set        | resolved font set; `.fonts() -> list[FontInfo]`, `.families() -> list[str]` (methods)      |
|  [03]   | `FontInfo`     | font descriptor | one resolved face (immutable): `.family`/`.style`/`.weight`/`.stretch`/`.path`/`.index`    |
|  [04]   | `TypstError`   | diagnostic      | compile/query failure (`RuntimeError`): `.message`/`.diagnostic`/`.hints`/`.trace`; RAISED |
|  [05]   | `TypstWarning` | diagnostic      | compile warning (`UserWarning`): same fields; COLLECTED by `compile_with_warnings`         |

[PUBLIC_TYPE_SCOPE]: typed `__init__.pyi` domain aliases

Stub aliases bind the input, format, and standard domains as closed vocabularies. `pdf_standards` accepts the full ISO matrix, combinable — `["a-3b", "ua-1"]` renders archival and accessible.

| [INDEX] | [ALIAS]             | [DEFINITION]                               | [CAPABILITY]                                           |
| :-----: | :------------------ | :----------------------------------------- | :----------------------------------------------------- |
|  [01]   | `Input`             | `TypeVar(str, pathlib.Path, bytes)`        | a source path, `Path`, or in-memory `.typ` bytes       |
|  [02]   | `OutputFormat`      | `Literal["pdf", "svg", "png", "html"]`     | the `format` row; inferred from the `output` extension |
|  [03]   | `PathLike`          | `TypeVar(str, pathlib.Path)`               | `root`/`package_path`/`package_cache_path` anchors     |
|  [04]   | `CreationTimestamp` | `int \| datetime.datetime`                 | `timestamp`: UNIX seconds or tz-aware `datetime`       |
|  [05]   | `PDFStandard`       | `Literal[...]` (token set below)           | PDF version + PDF/A + PDF/UA conformance matrix        |
|  [06]   | `PDFStandards`      | `PDFStandard \| list[PDFStandard] \| None` | one token, a combinable list, or `None`                |

[PDF_STANDARD]: `"1.4"` `"1.5"` `"1.6"` `"1.7"` `"2.0"` `"a-1a"` `"a-1b"` `"a-2a"` `"a-2b"` `"a-2u"` `"a-3a"` `"a-3b"` `"a-3u"` `"a-4"` `"a-4e"` `"a-4f"` `"ua-1"`

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: module free functions

Free functions carry `input` (path, `Path`, or `.typ` bytes) and share `root`, `font_paths` (a `Fonts` or folder `list`), `ignore_system_fonts`, `sys_inputs`, and the `package_path`/`package_cache_path` anchors. `output=None` returns `bytes` (per-page `list[bytes]` for multi-page PNG); an `output` path writes the file and returns `None`. `format` infers from the `output` extension, `ppi` affects `png` alone (default 144), `timestamp` pins creation time for byte-reproducible output, a `"ua-1"` render errors without `document(title: ..)`, and `pretty` pretty-prints PDF/SVG/HTML.

| [INDEX] | [SURFACE]               | [SHAPE] | [CAPABILITY]                                                                          |
| :-----: | :---------------------- | :------ | :------------------------------------------------------------------------------------ |
|  [01]   | `compile`               | static  | render markup to PDF/PNG/SVG/HTML -> `bytes` \| per-page `list[bytes]` \| file `None` |
|  [02]   | `compile_with_warnings` | static  | render and return `tuple[bytes \| list[bytes] \| None, list[TypstWarning]]`           |
|  [03]   | `query`                 | static  | query elements by `<label>` selector + `field`/`one` -> `str` (`json`/`yaml`)         |
|  [04]   | `eval`                  | static  | evaluate a source expression -> `str` (`json`/`yaml`)                                 |

[ENTRYPOINT_SCOPE]: `Compiler` methods

`Compiler(input=None, ..)` holds the world identity and shares the free-function `root`/`font_paths`/`ignore_system_fonts`/`sys_inputs`/`package` policy; `input=None` seeds an empty in-memory document, and each method's `input=` overrides the per-compile source while discovered fonts persist, so a batch pays font discovery once. `Compiler.compile.sys_inputs` is tri-state: `...` keeps the world's inputs, `None` clears them, a `dict` replaces them, mutating the `comemo`-cached world in place.

| [INDEX] | [SURFACE]                        | [SHAPE]  | [CAPABILITY]                                                      |
| :-----: | :------------------------------- | :------- | :---------------------------------------------------------------- |
|  [01]   | `Compiler.compile`               | instance | compile against the cached world; `sys_inputs` keep/clear/replace |
|  [02]   | `Compiler.compile_with_warnings` | instance | compile against the cached world and return warnings              |
|  [03]   | `Compiler.query`                 | instance | query elements by selector against the cached world               |
|  [04]   | `Compiler.eval`                  | instance | evaluate an expression against the cached world                   |

[MARKUP_ELEMENT_SCOPE]: tagged-PDF markup the lowering emits

Compiled source drives these built-in markup functions. `alt` rides the inner `image`, never doubled onto the enclosing `figure`; the `pdf` module exposes `attach` and `artifact` only; a `pdf.attach` under a PDF/A row errors without `mime-type:`.

| [INDEX] | [MARKUP]       | [CALL_SHAPE]                                                   | [CAPABILITY]                                           |
| :-----: | :------------- | :------------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `image`        | `image(source, alt: none\|str, width, height, fit)`            | embed graphic with screen-reader `alt` equivalent      |
|  [02]   | `figure`       | `figure(body, alt: none\|str, caption, kind, supplement)`      | numbered figure block carrying caption + role          |
|  [03]   | `heading`      | `heading(body, level: int = 1)`                                | outline node lowering the `H1`-`H6` structure          |
|  [04]   | `table`        | `table(columns, ..children)`                                   | row-major cell grid lowering the table structure       |
|  [05]   | `pdf.attach`   | `pdf.attach(path, data, relationship, mime-type, description)` | associated-file embed for tagged-PDF attachments       |
|  [06]   | `pdf.artifact` | `pdf.artifact(kind: str = "other", content)`                   | mark decorative content as a PDF artifact; AT skips it |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `compile`/`compile_with_warnings` is one render surface keyed by `format` and `output`; the owner takes `compile_with_warnings` and returns its warning count in `EmitFact.warnings`.
- One `Compiler` mints per plan through `DocumentPlan.world(title=)`; the compile, query, and eval arms share the one mint so the tree lowers to Typst source once per emission.
- `query`/`eval` run over THIS plan's own single-shot world, never a held head world that introspects the wrong document in a batch.
- `pdf_standards` selects the archival PDF/A + accessible PDF/UA target; `UA_1` projects to `("ua-1",)` alone, an archival-plus-tagged deliverable projects the combined tuple, and archival conformance is a render row, never a signer path.
- Lowering authors `image(.., alt: ..)` and `figure(.., caption: ..)`, Typst-string-escapes interpolated `alt`/`title`, wraps decorative content in `pdf.artifact`, and sets `mime-type:` on every `pdf.attach` under a PDF/A row.
- `TypstError` converts to `BoundaryFault` at `async_boundary`; `compile_with_warnings` returns `TypstWarning` values and `_typst_emit` records their count in `EmitFact`.
- `_typst_emit` returns compiled bytes and warnings in `EmitFact`; `query`/`eval` return bytes with `EmitFact.queried`.

[STACKING]:
- `expression`(`libs/python/.api/expression.md`) / `msgspec`(`libs/python/.api/msgspec.md`): a Typst arm is a pure `Arm = Callable[[DocumentPlan], EmitFact]` returning one frozen `EmitFact(data, warnings=..)`; `DocumentPlan` admits its `EmitSpec` through the closed `EmitPayload` `TypedDict` + `pydantic.TypeAdapter`, `DocumentPlan.of` returns `Result[Self, EmitFault]`, and the `TypstError` raise converts to `BoundaryFault` at the capsule rather than folding into the `EmitFault` admission vocabulary.
- `anyio`(`libs/python/.api/anyio.md`): the `Compiler` mints inside the offloaded arm (the font-discovery scan is GIL-releasing native), and each `CORE`-band render crosses the runtime `LanePolicy.offload` seam as a `KernelTrait.RELEASING` kernel — in-process, never inline on the loop.
- `vl-convert-python`(`vl-convert-python.md`): a `vl_convert.vegalite_to_svg` chart embeds into the source via `image(<svg-bytes>, alt: ..)` and Typst paginates it into one `format="pdf"` render; inversely a `format="svg"` render rasterizes through `vl_convert.svg_to_pdf`/`svg_to_png`, keeping one SVG-to-raster owner.
- `fonttools`(`fonttools.md`) / `uharfbuzz`(`uharfbuzz.md`): `Fonts(font_paths=[..]).fonts()` reads back the same OTF/TTF the shaping rails own and `typography/font` subset; the owner registers via `font_paths` and confirms `Fonts.families()`/`FontInfo.path` matches the shaped face before an `ignore_system_fonts=True` archival render.
- `document/tagged#ACCESS` (`pikepdf`): the emitted `figure`/`image`/`heading`/`table` + `pdf.artifact`/`pdf.attach` structure produces marked content `pikepdf` reads when authoring the `/StructTreeRoot`; typst draws the taggable content, `pikepdf` closes the tree.
- `pyhanko`(`pyhanko.md`): the archival PDF/A bytes from a `pdf_standards` render hand to the `pyhanko` PAdES signer; typst produces the conformant unsigned PDF, `pyhanko` owns the signature.

[LOCAL_ADMISSION]:
- `import typst` at boundary scope only, lazy so the native `.so` load defers off the import-time path; compile through one plan-minted `Compiler` offloaded as a `KernelTrait.RELEASING` kernel, never a fresh compiler per render in a batch.
