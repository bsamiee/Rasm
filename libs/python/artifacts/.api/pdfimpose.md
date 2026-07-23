# [PY_ARTIFACTS_API_PDFIMPOSE]

`pdfimpose` is the press-imposition schema engine for the artifacts composition rail: it re-orders, rotates, scales, and grids an emitted PDF onto larger press-ready sheets for the bindery, one `AbstractImpositor` subclass per binding schema. It composes the native MuPDF page-placement core the `pymupdf` owner already drives and never re-implements the affine `Matrix`, the page-copy primitive, or the PDF write. Behind `composition/imposition#IMPOSE` it is the provider-contained `ImpositionEngine.PDFIMPOSE` arm, selected against the local `pymupdf.Page.show_pdf_page` placement arm.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pdfimpose`
- package: `pdfimpose`
- import: `pdfimpose` (schemas under `pdfimpose.schema.<name>`)
- owner: `composition`
- rail: imposition
- license: `AGPL-3.0-or-later`; whole-program copyleft co-extensive with the `pymupdf` MuPDF core it links
- runtime deps: `pymupdf` (page-placement + PDF-write core), `papersize` (`parse_papersize`/`parse_length` sizing), `pyxdg` + `argdispatch` (CLI-only, unused in-process)
- entry points: console script `pdfimpose` (CLI); the in-process owner composes the library `impose` functions, never the console script or its `apply` config rail
- capability: press imposition of a source PDF onto larger imposed sheets across the binding-schema family — per-schema destination page-order matrix, recto/verso bind-edge rotation, per-sheet `creep` compensation, n-across×n-down `signature` layout, inner/outer margins, crop-mark overlay, `last`-page tail preservation, `group`-before-fold batching, and `BytesIO`-to-`BytesIO` in-memory operation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the impositor base and the schema family

`AbstractImpositor` is the closed imposition base every schema subclasses; each `pdfimpose.schema.<name>` submodule pairs one concrete impositor with a module-level `impose(...)` the owner composes, never the impositor class directly. `perfect` is a doc-only placeholder exposing no `impose` and no impositor, so `composition/imposition#IMPOSE` keeps `PERFECT_BIND` on its local `show_pdf_page` arm.

| [INDEX] | [SYMBOL]                                    | [PACKAGE_ROLE]  | [CAPABILITY]                                                         |
| :-----: | :------------------------------------------ | :-------------- | :------------------------------------------------------------------- |
|  [01]   | `schema.AbstractImpositor`                  | imposition base | abstract base owning the matrix/creep/bind fold over a `pymupdf` doc |
|  [02]   | `schema.saddle.SaddleImpositor`             | saddle-stitch   | magazine inserted-sheet booklet; `HardcoverImpositor` subclass       |
|  [03]   | `schema.hardcover.HardcoverImpositor`       | sewn signatures | folded-and-cut multi-page signatures, hardcover-bound                |
|  [04]   | `schema.cutstackfold.CutStackFoldImpositor` | cut-stack-fold  | one book printed, cut, stacked, folded, bound                        |
|  [05]   | `schema.copycutfold.CopyCutFoldImpositor`   | copy-cut-fold   | many cut-and-folded copies; `CutStackFoldImpositor` subclass         |
|  [06]   | `schema.wire.WireImpositor`                 | wire-bound      | pages cut, stacked, wire-bound; `CardsImpositor`-derived             |
|  [07]   | `schema.cards.CardsImpositor`               | cut flash-cards | front/back card sheets; `back=` orders the verso source              |
|  [08]   | `schema.onepagezine.OnePageZineImpositor`   | one-page zine   | single-sheet 8-page fold-zine, poster on the unfolded back           |
|  [09]   | `schema.perfect`                            | placeholder     | doc-only, no `impose`/impositor; perfect-bind has no schema          |

[PUBLIC_TYPE_SCOPE]: the geometry value object and binding vocabulary

`Margins` is the four-edge margin value object the `imargin`/`omargin` arguments accept — a lone positional broadcasts to all four edges. `BIND2ANGLE` is the closed binding-edge→rotation table the owner mirrors, mapping `Geometry.binding` to a key and `Geometry.gutter` to `imargin`, never re-deriving the rotation.

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]      | [CAPABILITY]                                                                        |
| :-----: | :------------------- | :------------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `schema.Margins`     | margin value object | `Margins(left, right, top, bottom)`; lone positional → all four, none → all-zero    |
|  [02]   | `schema.BIND2ANGLE`  | `dict[str, int]`    | binding-edge → page-rotation map: `{'left':0, 'top':90, 'right':180, 'bottom':270}` |
|  [03]   | `DEFAULT_PAPER_SIZE` | `str`               | module default destination paper name (`'A4'`) for the CLI/`size=` path             |
|  [04]   | `VERSION`            | `str`               | package version string                                                              |

[PUBLIC_TYPE_SCOPE]: the creep model

Creep is a `Callable[[int], float]` returning the per-sheet inward shift in pt from the sheet count; `nocreep` is the zero-creep default. `RE_CREEP` is the CLI creep-expression grammar the in-process owner never touches — it supplies its own sheet-count closure to `creep=`.

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE]           | [CAPABILITY]                                                                         |
| :-----: | :---------------- | :----------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `schema.nocreep`  | `Callable[[int], float]` | zero-creep default (`nocreep(s) -> 0`); the `creep=` argument's default              |
|  [02]   | `schema.RE_CREEP` | `re.Pattern`             | CLI creep-expression grammar `(?P<slope>…)s(?P<yintercept>…)?(?P<unit>…)?`; CLI-only |

[PUBLIC_TYPE_SCOPE]: signature-sizing helpers

`compute_signature` and `size2signature` compute the n-across×n-down layout maximally filling a destination sheet — `compute_signature(source, dest) -> (across, down)` from explicit sizes, `size2signature(destsize, *, sourcesize, imargin, omargin) -> tuple` from a destination size and margins. Both are the provider's auto-signature substrate behind the `size=` path; the owner authors its own `(across, down)` and never calls them.

[PUBLIC_TYPE_SCOPE]: fault family

`UserError` is the package's sole error type and derives directly from `BaseException`, so a bare `except Exception` never catches it — the `composition/imposition#IMPOSE` boundary names it explicitly in its `_FAULTS` catch tuple. `pymupdf`'s corrupt/zero-page faults (`FileDataError`/`EmptyFileError`, `RuntimeError`-derived) ride the same tuple.

| [INDEX] | [SYMBOL]    | [PACKAGE_ROLE] | [CAPABILITY]                                                                     |
| :-----: | :---------- | :------------- | :------------------------------------------------------------------------------- |
|  [01]   | `UserError` | user fault     | invalid imposition request: bad page count, unparseable size, conflicting sizing |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the per-schema `impose` convenience functions

One module-level `impose(files, output, *, ...)` per schema submodule is the owner-composed surface: `files` is a `Sequence[str | Path | BytesIO]` and `output` a `str | Path | BytesIO`, so the owner passes `(BytesIO(source),)` and a `BytesIO()` sink for fully in-memory operation. Each schema's keyword set differs, with no single uniform shape. `mark` honors only `['crop']`; the `signature`/`folds`/`size` sizing modes are mutually exclusive and a double-spec raises `UserError`.

Per-schema `impose` call shapes:
- [01]: `schema.saddle.impose(files, output, *, folds=None, signature=None, size=None, imargin=0, omargin=0, mark=None, last=0, bind='left', creep=nocreep, group=1)`
- [02]: `schema.hardcover.impose(files, output, *, folds=None, signature=None, size=None, imargin=0, omargin=0, mark=None, last=0, bind='left', group=1)`
- [03]: `schema.cutstackfold.impose(files, output, *, imargin=0, omargin=0, last=0, mark=None, signature=None, size=None, bind='left', creep=nocreep, group=0)`
- [04]: `schema.copycutfold.impose(files, output, *, imargin=0, omargin=0, last=0, mark=None, signature=None, size=None, bind='left', creep=nocreep, group=0)`
- [05]: `schema.wire.impose(files, output, *, imargin=0, omargin=0, last=0, mark=None, signature=None, size=None)`
- [06]: `schema.cards.impose(files, output, *, imargin=0, omargin=0, mark=None, signature=None, size=None, back='', bind='left')`
- [07]: `schema.onepagezine.impose(files, output, *, omargin=0, last=0, mark=None, bind='left')`
- [08]: `schema.perfect.impose` — absent (module exposes nothing)

| [INDEX] | [SURFACE]                    | [CAPABILITY]                                                                    |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `schema.saddle.impose`       | saddle-stitch booklet; owner's `BOOKLET` (group=1) + `SIGNATURE` (group>1) arms |
|  [02]   | `schema.hardcover.impose`    | sewn folded-signature book; no `creep` kwarg (creep via `folds`/`group`)        |
|  [03]   | `schema.cutstackfold.impose` | one cut-stack-fold book; owner's `CUT_AND_STACK` arm                            |
|  [04]   | `schema.copycutfold.impose`  | many cut-and-folded copies; owner's `COME_AND_GO` arm                           |
|  [05]   | `schema.wire.impose`         | wire-bound stacked pages; no `bind`/`creep`/`group` (cards-derived layout)      |
|  [06]   | `schema.cards.impose`        | front/back flash-cards; `back=` orders verso sources, no `creep`/`group`/`last` |
|  [07]   | `schema.onepagezine.impose`  | single-sheet 8-page fold-zine; no `imargin`/`signature`/`creep`/`group`         |
|  [08]   | `schema.perfect.impose`      | perfect-bind has no schema; local placement arm owns `PERFECT_BIND`             |

[ENTRYPOINT_SCOPE]: shared `impose` keyword vocabulary

Keyword semantics shared across the schemas that expose them; the owner sources each from its `Geometry`/`Marks` value objects.

| [INDEX] | [KEYWORD]   | [TYPE]                               | [CAPABILITY]                                                                      |
| :-----: | :---------- | :----------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `signature` | `tuple[int, int]`                    | `(n_across, n_down)` source-page matrix per destination sheet                     |
|  [02]   | `folds`     | `str`                                | fold sequence as `'h'`/`'v'` characters                                           |
|  [03]   | `size`      | `str`                                | destination paper name parsed by `papersize.parse_papersize`                      |
|  [04]   | `imargin`   | `float \| Margins \| Decimal`        | inner (gutter) margin between adjacent imposed pages, pt                          |
|  [05]   | `omargin`   | `float \| Margins \| str \| Decimal` | outer margin around the imposed sheet, pt (or `Margins` per-edge)                 |
|  [06]   | `bind`      | `str`                                | binding edge → recto/verso rotation via `BIND2ANGLE`                              |
|  [07]   | `creep`     | `Callable[[int], float]`             | per-sheet gutter shingle: sheet-count → inward shift pt (`nocreep` default)       |
|  [08]   | `group`     | `int`                                | sheets per fold group before binding (`0`/`1` ungrouped, `>1` batched signatures) |
|  [09]   | `last`      | `int`                                | trailing source pages pinned to the doc end (blank-fill inserted before them)     |
|  [10]   | `mark`      | `list[str]`                          | overlay marks; only `['crop']` is honored, every other token silently ignored     |
|  [11]   | `back`      | `str`                                | (cards only) verso-source ordering control                                        |

[ENTRYPOINT_SCOPE]: the impositor methods and the placement core

`AbstractImpositor` methods are the substrate the module `impose` functions drive over a `pymupdf` reader/writer — the matrix fold, sheet assembly, and mark overlay the owner depends on but never instantiates. `pdf.readpdf`/`Reader`/`Writer` wrap `pymupdf.Document` at the I/O boundary.

Impositor method call shapes:
- [01]: `AbstractImpositor.init(self, last=0, omargin=<Margins>, mark=[])`
- [02]: `AbstractImpositor.impose(self, files, output)` — the 2-arg instance method
- [03]: `AbstractImpositor.matrixes(self, pages: int) -> Iterator[Matrix]`
- [04]: `AbstractImpositor.stack_matrixes(self, matrixes, step, repeat)`
- [05]: `AbstractImpositor.insert_sheets(self, matrixes, sheets, pages, pagespersheet)`
- [06]: `AbstractImpositor.crop_marks` / `bind_marks(*, number, total, matrix, outputsize, inputsize)`
- [07]: `AbstractImpositor.blank_page_number(self, source) -> int`
- [08]: `pdf.readpdf(file: str | Document | Path | BytesIO)`; `pdf.Reader(files)`; `pdf.Writer(output)`

| [INDEX] | [SURFACE]                                     | [CAPABILITY]                                                                           |
| :-----: | :-------------------------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `AbstractImpositor.init`                      | base impositor state; subclasses add `bind`/`signature`/`creep`/`group`/`folds`/`back` |
|  [02]   | `AbstractImpositor.impose`                    | run the full imposition fold (module `impose(...)` builds the impositor first)         |
|  [03]   | `AbstractImpositor.matrixes`                  | per-page affine `pymupdf.Matrix` placement sequence                                    |
|  [04]   | `AbstractImpositor.stack_matrixes`            | tile a matrix block across stacked destination sheets                                  |
|  [05]   | `AbstractImpositor.insert_sheets`             | materialize destination sheets from the matrix plan                                    |
|  [06]   | `AbstractImpositor.crop_marks` / `bind_marks` | overlay crop / binding marks at the sheet edges                                        |
|  [07]   | `AbstractImpositor.blank_page_number`         | count of blank pages padded to fill the final signature                                |
|  [08]   | `pdf.readpdf` / `pdf.Reader` / `pdf.Writer`   | the `pymupdf.Document` read/write boundary the impositor uses                          |

## [04]-[IMPLEMENTATION_LAW]

[IMPOSITION_PROVIDER]:
- import: `lazy from pdfimpose.schema import saddle, cutstackfold, copycutfold` (with `hardcover`/`cards`/`wire`/`onepagezine` where admitted) and `lazy from pdfimpose import UserError as PdfImposeUserError`, at boundary scope through a module-scope lazy import so the native MuPDF dependency loads on first impose.
- engine-row axis: one provider row behind `ImpositionEngine.PDFIMPOSE`, never a second imposition owner; a `_PDFIMPOSE_SCHEMAS` `frozendict[Scheme, …]` binds each local `Scheme` to its module `impose` with the bind/creep/group flags valid for that schema, and `Geometry.engine is ImpositionEngine.PDFIMPOSE` switches the arm off the default local `show_pdf_page` placement.
- call axis: the arm wraps source bytes in `io.BytesIO`, calls `schema.impose((BytesIO(source),), BytesIO(), **kwargs)` with `kwargs` assembled per schema (`signature=(across, down)`, `imargin=gutter`, `omargin=0`, `mark=['crop'] if marks else []`, `bind=binding`/`creep=<closure>` where the schema exposes them), then reads `sink.getvalue()`; the module function constructs the impositor, so the owner never instantiates the class.
- sizing axis: the owner uses `signature=(across, down)` exclusively — `folds`/`size` are mutually exclusive and a double-spec raises `UserError`. `compute_signature`/`size2signature` are the provider's auto-fill substrate, not called.
- creep axis: the owner supplies `lambda sheets: geometry.creep * max(sheets - 1, 0)` so the shingle scales with the live sheet count; schemas exposing no `creep` kwarg carry no creep arm, gated by the `_PDFIMPOSE_SCHEMAS` flag.
- mark axis: the provider honors only `mark=['crop']`; richer registration/colour-bar/imposition-map marks land on the local placement arm.
- perfect-bind axis: `perfect` exposes no `impose`, so `PERFECT_BIND` stays on the local `show_pdf_page` arm; an unsupported scheme requested under `PDFIMPOSE` raises the `_PDFIMPOSE_SCHEMAS` `KeyError` the boundary catches rather than silently falling back.
- offload axis: every provider impose runs inside `anyio.to_thread.run_sync` under a `CapacityLimiter`-bounded band — the MuPDF page-copy fold is CPU-bound and never runs on the event loop.
- fault axis: `_FAULTS = (RuntimeError, ValueError, KeyError, IndexError, OSError, BeartypeCallHintViolation, PdfImposeUserError)` admits the `BaseException`-derived `UserError`, the `RuntimeError`-derived `pymupdf` corrupt/zero-page faults, and the `_PDFIMPOSE_SCHEMAS` `KeyError`; `reliability/faults#FAULT`'s `async_boundary(subject, thunk, catch=_FAULTS)` folds each caught cause through `BoundaryFault.of(...)`, and the interior raises nothing of its own.
- boundary axis: only imposed PDF bytes with local `Composed`/`ImposedPlan` facts cross the owner boundary — `Matrix`, `Page`, `Margins`, impositor instances, and schema names never do; the owner projects the layout as `Imposition.layers -> tuple[Layer, ...]` to `export/layered#LAYERED` and contributes one `core/receipt#RECEIPT` case selected by `Composed.kind`.
- license boundary: AGPL-3.0, co-extensive with `pymupdf` — a closed distributed/network imposition service triggers source disclosure, so the rail admits only an internal/permissively-licensed pipeline or an AGPL-compliant deployment; no permissive imposition substitute exists in the roster.
- evidence: each impose captures the imposed byte length, the re-read `pymupdf.Document.page_count`, the scheme, the `(across, down)` signature, the binding edge, the creep extent, and the crop-mark flag as the imposition receipt.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pdfimpose`
- Owns: press imposition of a source PDF onto larger imposed sheets across the binding-schema family — per-schema destination page-order matrix, recto/verso rotation, and creep over a `pymupdf` document, behind `composition/imposition#IMPOSE`'s `ImpositionEngine.PDFIMPOSE` provider row
- Accept: the module-level `<schema>.impose((BytesIO(source),), BytesIO(), *, signature=(across,down), imargin=gutter, omargin=0, mark=['crop']|[], bind=, creep=)` call returning imposed PDF bytes; `UserError` (named explicitly — `BaseException`-derived) plus `pymupdf` corrupt/zero-page faults folded into `reliability/faults#FAULT`; the imposed bytes re-read for page count and projected as `Layer` rows and one `ArtifactReceipt` case
- Reject: a second imposition owner where `composition/imposition#IMPOSE` owns the concern; instantiating `AbstractImpositor` subclasses directly; letting `Matrix`/`Page`/`Margins`/impositor handles cross the boundary; mixing `folds`/`size` with `signature`; expecting any mark beyond `['crop']` from the provider; routing `PERFECT_BIND` through `pdfimpose`; the CLI console-script/`apply` rail in-process; a second page-count formula drifting from the imposed `Document.page_count`; the MuPDF fold on the event loop; the AGPL path inside a distributed closed service
