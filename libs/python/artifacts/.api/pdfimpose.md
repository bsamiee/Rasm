# [PY_ARTIFACTS_API_PDFIMPOSE]

`pdfimpose` is the press-imposition schema engine for the artifacts composition rail — the dedicated booklet/signature impositor that re-orders, rotates, scales, and grids an already-emitted multi-page PDF onto larger press-ready imposed sheets for the bindery: saddle-stitch (magazine inserted-sheet), hardcover (sewn folded-signature book), cut-stack-fold (one cut-and-stacked book), copy-cut-fold (many cut-and-folded copies), wire-bound, cut flash-cards, and the one-page-zine fold. Each schema is an `AbstractImpositor` subclass that computes the destination-sheet page-order matrix, the recto/verso binding-edge rotation, and the per-sheet creep shift over a `pymupdf.Document`, then writes imposed PDF bytes — it composes the same native MuPDF page-placement core the `pymupdf` owner already drives (`pdfimpose` is built directly on `pymupdf>=1.26.1`) and never re-implements the affine `Matrix`, the page-copy primitive, or the PDF write the native core owns. In the `composition/imposition#IMPOSE` owner it is the provider-contained engine arm (`ImpositionEngine.PDFIMPOSE`) selected against the local `pymupdf.Page.show_pdf_page` placement arm: the owner maps a local `Scheme` value to one module-level `<schema>.impose(files, output, *, signature=, imargin=, omargin=, mark=, bind=, creep=, group=, last=)` call, hands it `(io.BytesIO(source),)` and an `io.BytesIO()` sink, and reads back only imposed PDF bytes — provider objects, `Matrix`, `Page`, `Margins`, and schema names never cross the owner boundary; local `Composed`/`ImposedPlan`/`Placement`/`Layer`/`ArtifactReceipt` remain the only downstream facts.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pdfimpose`
- package: `pdfimpose`
- import: `pdfimpose` (schemas under `pdfimpose.schema.<name>`)
- owner: `composition`
- rail: imposition
- version: `2.9.0`
- license: `AGPL-3.0-or-later`; copyleft is whole-program and matches the `pymupdf` arm it links — distributing a closed network service that imposes through `pdfimpose` triggers the same source-disclosure obligation the AGPL MuPDF core already imposes, so the imposition rail's commercial constraint is identical to the pdf rail's: an internal/permissively-licensed pipeline or an Artifex/AGPL-compliant deployment, never a closed distributed service. This is the load-bearing licensing fact, co-extensive with `pymupdf`'s
- build-floor: `Requires-Python >=3.10`; pure-Python (no native extension, the native work is MuPDF via the `pymupdf>=1.26.1` dependency), wheels resolve on cp315 directly — runs on the same `execution/lanes#LANE` worker band as the rest of the pdf cluster, the `_composed` fold offloaded off the event loop through an `anyio.to_thread.run_sync` bounded by a `CapacityLimiter`
- runtime deps: `pymupdf>=1.26.1` (the page-placement + PDF write core), `papersize` (the `parse_papersize` string→`(width,height)` pt parser behind the `size=` sizing mode), `pyxdg` (XDG config lookup for the CLI `apply` rail — unused in-process), `argdispatch` (CLI argument dispatch — unused in-process)
- entry points: console script `pdfimpose` (CLI; the in-process owner composes the library `impose` functions directly, never the console script or its `apply` config rail)
- capability: press imposition of a source PDF onto larger imposed sheets across seven schemas — saddle-stitch, hardcover sewn-signature, cut-stack-fold, copy-cut-fold, wire-bound, cut-cards (front/back), one-page-zine; per-schema destination page-order matrix, recto/verso `bind`-edge rotation (left/right/top/bottom → 0/90/180/270°), per-sheet `creep` (gutter-shingle) compensation via a `Callable[[int], float]` sheet-count function, n-across×n-down `signature` layout (or `folds` h/v fold-string or `size=` paper-name sizing), inner/outer margins (`imargin`/`omargin`, scalar pt or a four-edge `Margins`), crop-mark overlay (`mark=['crop']` only — every other mark token is silently ignored), `last`-page tail preservation, `group`-before-fold sheet batching, and `BytesIO`-to-`BytesIO` in-memory operation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the impositor base and the schema family
- rail: imposition

`AbstractImpositor` is the closed imposition base every schema subclasses; the seven concrete impositors live one-per-`pdfimpose.schema.<name>` submodule, each paired with a module-level `impose(...)` convenience function that constructs the impositor from keyword args and runs it. The owner composes the module-level `impose` functions, not the impositor classes directly — the class hierarchy is the substrate whose semantics (the matrix fold, the creep, the bind-edge rotation) the owner depends on but never instantiates. `perfect` is a documentation-only placeholder submodule that exposes no `impose` and no impositor — perfect-binding has no provider schema, so the `composition/imposition#IMPOSE` owner correctly keeps `PERFECT_BIND` on its local `show_pdf_page` placement arm.

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `schema.AbstractImpositor` | imposition base | abstract base: owns `impose`/`matrixes`/`stack_matrixes`/`insert_sheets`/`crop_marks`/`bind_marks` over a `pymupdf` doc |
| [02] | `schema.saddle.SaddleImpositor` | saddle-stitch | magazine/newspaper inserted-sheet booklet (subclass of `HardcoverImpositor`); `mark=['crop']` only |
| [03] | `schema.hardcover.HardcoverImpositor` | sewn signatures | real-book folded-and-cut multi-page signatures bound as a hardcover block |
| [04] | `schema.cutstackfold.CutStackFoldImpositor` | cut-stack-fold | one book: pages printed, cut, stacked, folded, bound (subclass of `AbstractImpositor`) |
| [05] | `schema.copycutfold.CopyCutFoldImpositor` | copy-cut-fold | many copies of one book: cut and folded per copy (subclass of `CutStackFoldImpositor`) |
| [06] | `schema.wire.WireImpositor` | wire-bound | individual pages cut, stacked, wire-bound (subclass of `CardsImpositor`) |
| [07] | `schema.cards.CardsImpositor` | cut flash-cards | front/back card sheets (question/answer), `back=` controls the verso source order |
| [08] | `schema.onepagezine.OnePageZineImpositor` | one-page zine | single-sheet 8-page fold-zine with a poster on the unfolded back |
| [09] | `schema.perfect` | placeholder | doc-only submodule, no `impose`/impositor — perfect-bind has no provider schema (local arm owns it) |

[PUBLIC_TYPE_SCOPE]: the geometry value object and binding vocabulary
- rail: imposition

`Margins` is the four-edge margin value object the `imargin`/`omargin` arguments accept (a single positional broadcasts to all four edges, no argument is all-zero); `BIND2ANGLE` is the closed binding-edge→rotation-angle table the owner mirrors when it derives recto/verso rotation. The owner maps its local `Geometry.binding` to a `BIND2ANGLE` key string and its `Geometry.gutter` to `imargin`, never re-deriving the rotation.

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `schema.Margins` | margin value object | `Margins(left, right, top, bottom)`; one positional broadcasts to all four, none → all-zero (`dataclass`) |
| [02] | `schema.BIND2ANGLE` | `dict[str, int]` | binding-edge → page-rotation map: `{'left':0, 'top':90, 'right':180, 'bottom':270}` |
| [03] | `DEFAULT_PAPER_SIZE` | `str` | module default destination paper name (`'A4'`) for the CLI/`size=` path |
| [04] | `VERSION` | `str` | package version string (`'2.9.0'`) |

[PUBLIC_TYPE_SCOPE]: the creep model
- rail: imposition

Creep (gutter shingling) is a `Callable[[int], float]`: given the sheet count, return the per-sheet inward shift in pt. `nocreep` is the zero-creep default; `RE_CREEP` is the CLI creep-expression grammar (`<slope>s<yintercept><unit>`, e.g. `2sin`). The owner supplies its own `lambda sheets: geometry.creep * max(sheets - 1, 0)` closure for the `creep=` argument rather than the CLI string, so the in-process arm never touches `RE_CREEP`.

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `schema.nocreep` | `Callable[[int], float]` | the zero-creep default function (`nocreep(s) -> 0`); the `creep=` argument's default |
| [02] | `schema.RE_CREEP` | `re.Pattern` | CLI creep-expression grammar `(?P<slope>…)s(?P<yintercept>…)?(?P<unit>…)?`; CLI-only |

[PUBLIC_TYPE_SCOPE]: signature-sizing helpers
- rail: imposition

The two signature helpers compute the n-across×n-down layout that maximally fills a destination sheet — `compute_signature` from explicit source/dest sizes, `size2signature` from a destination size plus inner/outer margins. The owner authors its own `(geometry.across, geometry.down)` signature, so these are documented as the substrate behind the `size=`/auto-signature path rather than a member the owner calls — they are the proof the provider's signature math exists and need not be re-implemented locally.

| [INDEX] | [SYMBOL] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `schema.compute_signature` | `compute_signature(source, dest) -> tuple[int, int]` | maximal `(across, down)` fitting `source`-sized pages into `dest` |
| [02] | `schema.size2signature` | `size2signature(destsize, *, sourcesize, imargin, omargin) -> tuple` | signature + margins for a destination paper size |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: imposition

`UserError` is the package's sole error type and — critically — derives directly from `BaseException`, not `Exception` (MRO: `UserError → BaseException → object`). A bare `except Exception` therefore does not catch it; the `composition/imposition#IMPOSE` owner must name it explicitly in its `_FAULTS` raise tuple (as `PdfImposeUserError`), which is exactly what the page's `runtime/faults#FAULTS` `async_boundary(..., catch=_FAULTS)` does. The underlying `pymupdf.FileDataError`/`EmptyFileError` faults (corrupt or zero-page source) are `RuntimeError`-derived and ride the same tuple through the `pymupdf` dependency.

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `UserError` | user fault | invalid imposition request (bad page count, unparseable size, conflicting sizing); subclasses `BaseException` directly, so `except Exception` misses it — name it in the boundary `catch` tuple |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the per-schema `impose` convenience functions
- rail: imposition

The owner-composed surface: one module-level `impose(files, output, *, ...)` per schema submodule. `files` is a `Sequence[str | pathlib.Path | io.BytesIO]` and `output` is `str | pathlib.Path | io.BytesIO`, so the owner passes `(io.BytesIO(source),)` and an `io.BytesIO()` sink for fully in-memory operation. The keyword set differs per schema — there is no single uniform `impose` shape; the owner selects the kwargs valid for the chosen schema. `mark` accepts only `['crop']` (every other token silently ignored). The three sizing modes `signature` / `folds` / `size` are mutually exclusive — supplying more than one raises `UserError`; the owner uses `signature=(across, down)` exclusively.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `schema.saddle.impose` | `impose(files, output, *, folds=None, signature=None, size=None, imargin=0, omargin=0, mark=None, last=0, bind='left', creep=nocreep, group=1)` | saddle-stitch booklet; the owner's `BOOKLET` (group=1) and `SIGNATURE` (group>1) arms |
| [02] | `schema.hardcover.impose` | `impose(files, output, *, folds=None, signature=None, size=None, imargin=0, omargin=0, mark=None, last=0, bind='left', group=1)` | sewn folded-signature book; no `creep` kwarg (signature creep handled by `folds`/`group`) |
| [03] | `schema.cutstackfold.impose` | `impose(files, output, *, imargin=0, omargin=0, last=0, mark=None, signature=None, size=None, bind='left', creep=nocreep, group=0)` | one cut-stack-fold book; the owner's `CUT_AND_STACK` arm |
| [04] | `schema.copycutfold.impose` | `impose(files, output, *, imargin=0, omargin=0, last=0, mark=None, signature=None, size=None, bind='left', creep=nocreep, group=0)` | many cut-and-folded copies; the owner's `COME_AND_GO` arm |
| [05] | `schema.wire.impose` | `impose(files, output, *, imargin=0, omargin=0, last=0, mark=None, signature=None, size=None)` | wire-bound stacked pages; no `bind`/`creep`/`group` (cards-derived layout) |
| [06] | `schema.cards.impose` | `impose(files, output, *, imargin=0, omargin=0, mark=None, signature=None, size=None, back='', bind='left')` | front/back flash-cards; `back=` orders verso sources, no `creep`/`group`/`last` |
| [07] | `schema.onepagezine.impose` | `impose(files, output, *, omargin=0, last=0, mark=None, bind='left')` | single-sheet 8-page fold-zine; no `imargin`/`signature`/`creep`/`group` (fixed 2×4 fold) |
| [08] | `schema.perfect.impose` | _(absent — module exposes nothing)_ | perfect-bind has no provider schema; the local placement arm owns `PERFECT_BIND` |

[ENTRYPOINT_SCOPE]: shared `impose` keyword vocabulary
- rail: imposition

The keyword semantics shared across the schemas that expose them. The owner reads each from its `Geometry`/`Marks` value objects: `signature ← (geometry.across, geometry.down)`, `imargin ← geometry.gutter`, `bind ← geometry.binding`, `creep ← geometry.creep` closure, `mark ← ['crop'] if marks else []`.

| [INDEX] | [KEYWORD] | [TYPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `signature` | `tuple[int, int]` | `(n_across, n_down)` source-page matrix per destination sheet; mutually exclusive with `folds`/`size` |
| [02] | `folds` | `str` | fold sequence as `'h'`/`'v'` characters; mutually exclusive with `signature`/`size` |
| [03] | `size` | `str` | destination paper name parsed by `papersize.parse_papersize`; mutually exclusive with `signature`/`folds` |
| [04] | `imargin` | `float \| Margins \| Decimal` | inner (gutter) margin between adjacent imposed pages, pt |
| [05] | `omargin` | `float \| Margins \| str \| Decimal` | outer margin around the imposed sheet, pt (or a `Margins` for per-edge control) |
| [06] | `bind` | `str` | binding edge `'left'`/`'right'`/`'top'`/`'bottom'` → recto/verso rotation via `BIND2ANGLE` |
| [07] | `creep` | `Callable[[int], float]` | per-sheet gutter shingle: sheet-count → inward shift pt (`nocreep` default) |
| [08] | `group` | `int` | sheets per fold group before binding (`0`/`1` ungrouped, `>1` batched signatures) |
| [09] | `last` | `int` | number of trailing source pages pinned to the document end (blank-fill pages inserted before them) |
| [10] | `mark` | `list[str]` | overlay marks; only `['crop']` is honored, every other token silently ignored |
| [11] | `back` | `str` | (cards only) verso-source ordering control |

[ENTRYPOINT_SCOPE]: the impositor methods and the placement core
- rail: imposition

The substrate the module `impose` functions drive — documented so the owner understands the provider's page-placement model (and confirms it is the same MuPDF core `pymupdf` owns) rather than re-deriving it. `AbstractImpositor.matrixes(pages)` yields the per-page affine `Matrix` sequence, `stack_matrixes`/`insert_sheets` assemble destination sheets, `crop_marks`/`bind_marks` overlay the marks, and `impose(files, output)` (the 2-arg instance method, distinct from the keyword-rich module function that constructs the impositor first) runs the whole fold over a `pymupdf` reader/writer. `open_pdf`/`read`/`write` are the I/O boundary (`pdfimpose.pdf.Reader`/`Writer`/`readpdf` wrap `pymupdf.Document`).

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `AbstractImpositor.init` | `init(self, last=0, omargin=<Margins>, mark=[])` | base impositor state; subclasses add `bind`/`signature`/`creep`/`group`/`folds`/`back` |
| [02] | `AbstractImpositor.impose` | `impose(self, files, output)` (2-arg instance method) | run the full imposition fold; the module `impose(...)` constructs the impositor then calls this |
| [03] | `AbstractImpositor.matrixes` | `matrixes(self, pages: int) -> Iterator[Matrix]` | per-page affine `pymupdf.Matrix` placement sequence |
| [04] | `AbstractImpositor.stack_matrixes` | `stack_matrixes(self, matrixes, step, repeat)` | tile a matrix block across stacked destination sheets |
| [05] | `AbstractImpositor.insert_sheets` | `insert_sheets(self, matrixes, sheets, pages, pagespersheet)` | materialize destination sheets from the matrix plan |
| [06] | `AbstractImpositor.crop_marks` / `bind_marks` | `(*, number, total, matrix, outputsize, inputsize)` | overlay crop / binding marks at the sheet edges |
| [07] | `AbstractImpositor.blank_page_number` | `blank_page_number(self, source) -> int` | count of blank pages padded to fill the final signature |
| [08] | `pdf.readpdf` / `pdf.Reader` / `pdf.Writer` | `readpdf(file: str \| Document \| Path \| BytesIO)`; `Reader(files)`; `Writer(output)` | the `pymupdf.Document` read/write boundary the impositor uses |

## [04]-[IMPLEMENTATION_LAW]

[IMPOSITION_PROVIDER]:
- import: `lazy from pdfimpose.schema import saddle, cutstackfold, copycutfold` (and `hardcover`/`cards`/`wire`/`onepagezine` if those schemes are admitted) plus `lazy from pdfimpose import UserError as PdfImposeUserError` — at boundary scope only, deferred through a module-scope lazy import so the native MuPDF dependency loads on first impose, never at module import.
- engine-row axis: `pdfimpose` is one provider row behind the `composition/imposition#IMPOSE` owner's `ImpositionEngine.PDFIMPOSE`, never a second imposition owner. A `_PDFIMPOSE_SCHEMAS` `frozendict[Scheme, …]` table binds each local `Scheme` to its module `impose` function plus the bind/creep/group capability flags valid for that schema; `Geometry.engine is ImpositionEngine.PDFIMPOSE` switches the `Impose` arm to the provider path, the default local `pymupdf.Page.show_pdf_page` placement arm otherwise.
- call axis: the provider arm wraps the source bytes in `io.BytesIO`, calls `schema.impose((BytesIO(source),), BytesIO(), kwargs)` where `kwargs` is assembled per-schema (`signature=(across, down)`, `imargin=gutter`, `omargin=0`, `mark=['crop'] if marks else []`, `bind=binding` where the schema exposes it, `creep=<closure>` where the schema exposes it), then reads `sink.getvalue()` back. The module function constructs the impositor and runs its 2-arg `impose`; the owner never instantiates the impositor class.
- sizing axis: the owner uses `signature=(across, down)` exclusively — the n-across×n-down matrix — never mixing in `folds` or `size` (mutually exclusive; a double-spec raises `UserError`). `compute_signature`/`size2signature` are the provider's auto-fill substrate, documented but not called.
- creep axis: creep is a `Callable[[int], float]`; the owner supplies `lambda sheets: geometry.creep * max(sheets - 1, 0)` rather than the CLI `RE_CREEP` string, so the gutter shingle scales with the live sheet count. Schemas without a `creep` kwarg (`hardcover`/`cards`/`wire`/`onepagezine`) carry no creep arm; the `_PDFIMPOSE_SCHEMAS` flag gates it.
- mark axis: only `mark=['crop']` is honored by the provider — every other mark token is silently dropped. The owner's richer registration/colour-bar/imposition-map marks therefore land on the local placement arm (the `composition/sheet#SHEET`-style overlay), never expecting `pdfimpose` to draw them; the provider arm requests only crop marks.
- perfect-bind axis: `perfect` exposes no `impose` (doc-only placeholder) — `PERFECT_BIND` stays on the local `show_pdf_page` arm. An unsupported scheme requested under `PDFIMPOSE` raises the `_PDFIMPOSE_SCHEMAS` `KeyError` the boundary catches, which is preferable to silently falling back to local placement and hiding the requested provider.
- offload axis: every provider impose runs inside `anyio.to_thread.run_sync` under a `CapacityLimiter`-bounded band — the MuPDF page-copy fold is CPU-bound and must never run on the event loop, exactly as the local `pymupdf` arm offloads its `_composed` fold.
- fault axis: `pdfimpose.UserError` derives directly from `BaseException` (not `Exception`), so it MUST be named explicitly in the boundary `catch` tuple — `_FAULTS = (RuntimeError, ValueError, KeyError, IndexError, OSError, BeartypeCallHintViolation, PdfImposeUserError)` admits it alongside the `pymupdf.FileDataError`/`EmptyFileError` (`RuntimeError`-derived) corrupt/zero-page faults and the `_PDFIMPOSE_SCHEMAS` `KeyError`. The `runtime/faults#FAULTS` `async_boundary(subject, thunk, catch=_FAULTS)` folds each caught cause through `BoundaryFault.of(...)` into a structurally-addressable case; the interior raises nothing of its own and carries no second rail.
- boundary axis: provider objects, `Matrix`, `Page`, `Margins`, the impositor instances, and the schema module names never cross the owner boundary — only imposed PDF bytes plus local `Composed`/`ImposedPlan` facts. The owner projects the imposed layout outward as `Imposition.layers -> tuple[Layer, ...]` to `export/layered#LAYERED` and contributes one `core/receipt#RECEIPT` case (`ArtifactReceipt.Egress`/`Pdf`/`Preview` selected by `Composed.kind`), never a parallel receipt shape.
- license boundary: AGPL-3.0 — co-extensive with `pymupdf`. A closed distributed/network imposition service triggers source disclosure; the constraint is identical to the pdf rail's and is an architecture fact, not a coding caveat. There is no permissively-licensed imposition substitute in the roster, so the AGPL boundary is the imposition rail's boundary.
- evidence: each provider impose captures the imposed byte length, the real imposed `pymupdf.Document.page_count` (re-read from the sink bytes, never a second parallel formula), the scheme, the `(across, down)` signature, the binding edge, the creep extent, and the crop-mark flag as the imposition receipt.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pdfimpose`
- Owns: the press-imposition schema engine — saddle-stitch, hardcover sewn-signature, cut-stack-fold, copy-cut-fold, wire-bound, cut-cards, and one-page-zine destination page-order/rotation/creep computation over a `pymupdf` document, behind `composition/imposition#IMPOSE`'s `ImpositionEngine.PDFIMPOSE` provider row
- Accept: the module-level `<schema>.impose((BytesIO(source),), BytesIO(), *, signature=(across,down), imargin=gutter, omargin=0, mark=['crop']|[], bind=, creep=)` call returning imposed PDF bytes; the `UserError` (named explicitly — it is `BaseException`-derived) plus `pymupdf` corrupt/zero-page faults folded into the `runtime/faults#FAULTS` boundary; the imposed bytes re-read for page count and projected as `Layer` rows + a single `ArtifactReceipt` case
- Reject: a second imposition owner where `composition/imposition#IMPOSE` already owns the concern; instantiating `AbstractImpositor` subclasses directly where the module `impose` functions construct them; letting `Matrix`/`Page`/`Margins`/impositor handles cross the owner boundary; mixing `folds`/`size` with `signature` (mutually exclusive — raises `UserError`); expecting any mark beyond `['crop']` to be drawn by the provider (richer marks belong to the local placement arm); routing `PERFECT_BIND` through `pdfimpose` (no `perfect.impose` exists — local arm owns it); the CLI console-script/`apply` config rail in-process; a second parallel page-count formula that drifts from the imposed `Document.page_count`; running the MuPDF fold on the event loop instead of the bounded `to_thread` band; the AGPL imposition path inside a distributed closed service (co-extensive with the `pymupdf` license boundary)
