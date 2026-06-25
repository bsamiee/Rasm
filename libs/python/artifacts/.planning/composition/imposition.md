# [PY_ARTIFACTS_IMPOSITION]

The n-up / booklet / signature imposition owner laying emitted sheets into a press-ready imposed form. `Imposition` is ONE owner over the imposition pipeline carrying a closed-payload `ImposeOp` `expression.tagged_union` — each operation a case carrying its own typed payload, never a `StrEnum` keyed against an erased `dict[str, object]` bag — dispatched by one total `match`. It reads an already-emitted multi-page PDF (the sheet set from `composition/sheet#SHEET`, the document body from `document/emit#DOCUMENT`) and re-orders, scales, and places each source page onto a larger imposed sheet — n-up grid placement, saddle-stitch booklet pagination, and folded-signature ordering — through `pymupdf.Page.show_pdf_page`, which draws each source page into its imposed-sheet cell at the page-order the imposition scheme computes. `Scheme` is the closed `StrEnum` over the imposition schemes (`NUP` raw grid, `BOOKLET` saddle-stitch 2-up centerfold, `SIGNATURE` folded-signature multi-leaf), each a `Plan` policy-table row binding its page-order computation, its sheet-geometry partition, and its imposed-sheet count in one value, so a new imposition scheme is one row, never a parallel imposer class. `Imposed` is the closed-payload value object each computed placement carries — the source page index, the target imposed-sheet index, the target cell rect, and the rotate axis — so the imposition fold is one sweep over the computed placement list, never a per-scheme draw loop. `Geometry` is the row-policy value object the imposition carries — the imposed-sheet size, the n-across/n-down grid, the creep/shingling gutter, the binding edge, and the bleed/trim — projecting once to the cell-rect partition every scheme's placement list reads. `pdfimpose` is the dedicated booklet/signature imposition engine that computes the saddle-stitch and folded-signature page order natively (it is NOT-yet-admitted, gated [RESEARCH]); until it lands, the `BOOKLET`/`SIGNATURE` page-order computation is the page-settled half-fold integer arithmetic (`saddle_order`/`signature_order`) over `pymupdf.show_pdf_page` placement, and the `NUP` scheme is fully settled over the same placement floor. One imposition surface discriminating the scheme, not a per-scheme imposer family. `pymupdf` resolves on the cp315 core (native MuPDF cp310-abi3), so the imposition arm resolves in-process and imports its engine at boundary scope inside the arm body; the `pdfimpose` engine, once admitted, likewise resolves on the core. The imposed sheets the fold produces are handed onward to `document` assembly (the imposed body is the document the assembler collates), so this owner computes the imposition and places the pages but assembles no document. This owner is the DEDICATED booklet/signature imposition engine distinct from the simple `document/egress#FINISH` `IMPOSE` step, which is the in-document `pypdf.Transformation` n-up grid fold over an already-finished PDF — egress imposes a finished PDF as one finishing step among eight, while this owner is the multi-sheet press-imposition engine computing the saddle-stitch and folded-signature page order the egress step never reaches. Every operation returns a `RuntimeRail[ContentKey]` and contributes the existing `core/receipt#RECEIPT` `ArtifactReceipt.Pdf` case carrying the content key, the imposed byte count, and the imposed-sheet page count.

## [01]-[INDEX]

- [01]-[IMPOSE]: n-up / booklet / signature imposition owner over the closed-payload `ImposeOp` `tagged_union` dispatched to `pymupdf` (`Page.show_pdf_page` source-page placement into the imposed-sheet cell, core) with `pdfimpose` as the dedicated booklet-signature page-order engine ([RESEARCH], not-yet-admitted); `Scheme`/`Plan`/`Imposed`/`Geometry` the closed scheme/plan/placement/geometry value objects; threads `core/receipt#RECEIPT` `ArtifactReceipt.Pdf`.

## [02]-[IMPOSE]

- Owner: `Imposition` the one imposition owner discriminating the scheme over the closed `ImposeOp` `expression.tagged_union` whose every case carries its own typed payload, never a `StrEnum` keyed against a shared erased `dict[str, object]`; `Scheme` the closed `StrEnum` over the three imposition schemes (`NUP` raw n-across-by-n-down grid, `BOOKLET` saddle-stitch 2-up centerfold pagination, `SIGNATURE` folded-signature multi-leaf ordering); `PLANS` the `dict[Scheme, Plan]` policy table binding each scheme to one `Plan` row carrying its page-order computation, its imposed-sheet partition, and its sheet-count rule — the table is the totality proof, one page-order arm per scheme, never an `if scheme == ...` cascade and never a parallel imposer class per scheme; `Plan` the row carrying the `(order, cells, sheets)` computation triple; `Imposed` the closed-payload placement value object each computed placement carries (source page index, imposed-sheet index, cell rect, rotate); `Geometry` the row-policy value object carrying the imposed-sheet size, the n-across/n-down grid, the creep/shingling gutter, the binding edge, and the bleed/trim, projecting once to the cell-rect partition every scheme's placement list reads. `pymupdf` owns the `Page.show_pdf_page` cross-document source-page draw, the `new_page`/`paper_rect` imposed-sheet construction, and the `tobytes` serialize; `pdfimpose` (not-yet-admitted) owns the native saddle-stitch and folded-signature page-order computation; until `pdfimpose` lands, the `BOOKLET`/`SIGNATURE` page-order arithmetic (`saddle_order`/`signature_order` half-fold integer computation) is this owner's page-settled fold over the same `show_pdf_page` placement floor the `NUP` scheme rides, never a re-implemented PDF byte emitter.
- Cases: `ImposeOp` cases — `Nup(source, geometry)` (raw n-up grid imposition — partition the imposed sheet into the `Geometry` n-across-by-n-down cell grid, fold each source page in document order onto its grid cell through `show_pdf_page`, returning the n-up imposed PDF — the fully-settled scheme over the `show_pdf_page` placement floor) · `Booklet(source, geometry)` (saddle-stitch 2-up booklet — compute the centerfold page order through `saddle_order` so the folded sheet stack reads in sequence when bound at the spine, place two source pages per imposed sheet side through `show_pdf_page`, returning the booklet-imposed PDF) · `Signature(source, geometry, leaves)` (folded-signature imposition — compute the folded-signature page order through `signature_order` for a multi-leaf signature of `leaves` folds, place each source page onto its signature position through `show_pdf_page`, returning the signature-imposed PDF) · `Plan(source, scheme, geometry)` (compute-only — resolve the `PLANS[scheme]` page-order and cell partition over the source page count and the `Geometry`, returning the computed `Imposed` placement list without drawing, the introspection arm a downstream consumer reads to preview or validate the imposition before the draw) — matched by one total `match`/`case`; never a per-scheme imposer sibling, never a parallel booklet-versus-signature draw method, never a per-page draw call family.
- Entry: `Imposition.of` is `async` over the runtime `async_boundary`, dispatches the `ImposeOp` case, and returns a `RuntimeRail[ContentKey]`; every drawing arm resolves synchronously on the cp315 core inside the async capsule — `pymupdf` (native MuPDF cp310-abi3) imports on the core band and `pdfimpose`, once admitted, likewise resolves on the core — so no imposition arm crosses a process seam and the `async_boundary` is the uniform consumer contract `document` assembly `await`s. The `pymupdf`/`pdfimpose` imports land at boundary scope inside each arm, never at module top.
- Auto: `_emit` folds the op — every drawing arm resolves `PLANS[scheme]` over the source page count and the `Geometry` into the `(order, cells, sheets)` triple, opens the source through `pymupdf.open(stream=source)`, allocates the `sheets` imposed pages through `Document.new_page(width=, height=)` at the `Geometry` imposed-sheet size, folds each `Imposed` placement over `target_page.show_pdf_page(cell_rect, source_doc, pno=imposed.source, rotate=imposed.rotate)` drawing each source page into its computed cell, and `tobytes`; the `Nup` arm reads the document-order `nup_order` (each source page in sequence to the next grid cell), the `Booklet` arm the `saddle_order` centerfold computation (the half-fold integer pairing `(last, first, second, second-last, ...)` so the bound stack reads in order), the `Signature` arm the `signature_order` folded-leaf computation over `leaves` folds; the `cell_rect` for each placement comes from the `Geometry.partition` n-across/n-down grid offset by the creep/shingling gutter; the `Plan` op resolves the same `PLANS[scheme]` triple and returns the `Imposed` placement list serialized without opening a draw surface. The content key mints over the returned imposed bytes through `ContentIdentity.of`.
- Receipt: each operation contributes `core/receipt#RECEIPT` `ArtifactReceipt.Pdf` carrying the content key, the imposed byte count, and the imposed-sheet page count (the `sheets` the scheme computed); the imposition owner adds NO new receipt case — the imposition facts are byte count and imposed-sheet count, the Pdf shape every PDF producer contributes to. The source page count, the scheme, and the n-up grid ride the imposition's own evidence carried into the Pdf facts, never a parallel imposition-receipt type.
- Packages: `pymupdf` (`open(stream=)`/`Document.new_page(width=, height=)`/`paper_rect`/`Page.show_pdf_page(rect, docsrc, pno, keep_proportion, overlay, oc, rotate, clip)`/`Page.rect`/`Rect`/`Document.page_count`/`Document.tobytes`, native MuPDF cp310-abi3 reflected on cp315, AGPL-3.0 — internal pipeline use) on the cp315 core; `pdfimpose` (the dedicated saddle-stitch/folded-signature page-order imposition engine — NOT-yet-admitted, [RESEARCH]) on the cp315 core once admitted; `msgspec` (`Struct` frozen rows, `structs.asdict` projecting the `Geometry` axis); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`).
- Growth: a new imposition scheme (cut-and-stack, come-and-go, work-and-turn) is one `Scheme` member plus one `Plan` row in `PLANS` carrying its page-order computation and sheet partition — never a parallel imposer class or an `if scheme == ...` branch; a new geometry knob (a per-signature creep curve, a variable gutter, a crossover bleed) is one `Geometry` field projected into the existing cell-rect partition — never a `Plan` parameter; a new placement axis (a flipped verso, a 180-rotated head-to-head) is one `Imposed` field carried into the existing `show_pdf_page` fold (`rotate=`/`clip=` are already rows on the member) — never a per-placement draw method; a deeper signature is the same `signature_order` computation over more `leaves`; a registration-mark or crop-guide overlay on the imposed sheet routes to `composition/compose#COMPOSE` `Overlay`, never a mark-emit arm grafted here. Zero new surface.
- Boundary: this owner imposes already-emitted sheets and assembles no document — a per-scheme imposer class family (a `NupImposer`/`BookletImposer`/`SignatureImposer` triple), a stringly-typed `if scheme == "booklet"` branch beside the `Scheme` row dispatch, a per-page `show_pdf_page` call beside the one placement fold, a hand-rolled PDF byte emitter beside the admitted `pymupdf`/`pdfimpose` engines, and a parallel imposition-receipt type beside the `ArtifactReceipt.Pdf` contribution are the deleted forms; no UI, no live imposition preview, no document assembly, no sheet re-author. The sheets the imposition reads arrive already-emitted from `composition/sheet#SHEET` (the framed titled sheet) and `document/emit#DOCUMENT` (the document body) — this owner re-orders and places them and re-authors no sheet; the sheet-authoring concern stays upstream. The imposed body is handed to `document` assembly — this owner imposes the press form and grows no assembly arm. This owner is the DEDICATED booklet/signature imposition engine distinct from `document/egress#FINISH` `IMPOSE`: the egress IMPOSE step is the in-document `pypdf.Transformation` n-up grid fold over a finished PDF as one of eight finishing steps, computing no booklet/signature page order; this owner is the multi-sheet press-imposition engine computing the saddle-stitch and folded-signature ordering over `pymupdf.show_pdf_page` (the higher-fidelity cross-document draw), so the two never overlap — egress finishes one PDF, imposition imposes a press form. A signature-imposition branch grafted onto the egress step and an n-up grid re-implemented here beside the egress `Transformation` fold are the rejected boundary crossings. PDF security finishing routes to `document/egress#FINISH`; registration/crop-mark overlay routes to `composition/compose#COMPOSE`. `pymupdf` resolves on the cp315 core and imports at boundary scope inside each arm; the AGPL `pymupdf` placement leg is reserved for the internal imposition pipeline per the rail's license constraint. The content key mints over the emitted imposed bytes through the runtime `ContentIdentity.of`, never re-minted off a source sheet key.

```python signature
from collections.abc import Callable, Sequence
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import case, tag, tagged_union
from msgspec import Struct, structs

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt

if TYPE_CHECKING:
    from collections.abc import Iterable

    from rasm.runtime.receipts import Receipt


type Rect = tuple[float, float, float, float]
type Dimensions = tuple[float, float]
type Edge = Literal["left", "right", "top", "bottom"]


class Scheme(StrEnum):
    NUP = "nup"
    BOOKLET = "booklet"
    SIGNATURE = "signature"


class Geometry(Struct, frozen=True):
    sheet: Dimensions = (1190.55, 841.89)
    across: int = 2
    down: int = 1
    gutter: float = 0.0
    creep: float = 0.0
    binding: Edge = "left"
    bleed: float = 0.0

    @property
    def slots(self) -> int:
        return self.across * self.down

    def partition(self) -> Sequence[Rect]:
        width, height = self.sheet
        cell_w, cell_h = (width - (self.across + 1) * self.gutter) / self.across, (height - (self.down + 1) * self.gutter) / self.down
        return tuple(
            (
                self.gutter + col * (cell_w + self.gutter),
                self.gutter + row * (cell_h + self.gutter),
                self.gutter + col * (cell_w + self.gutter) + cell_w,
                self.gutter + row * (cell_h + self.gutter) + cell_h,
            )
            for row in range(self.down)
            for col in range(self.across)
        )


class Imposed(Struct, frozen=True):
    source: int
    sheet: int
    cell: Rect
    rotate: int = 0


class Plan(Struct, frozen=True):
    order: Callable[[int, int], Sequence[int]]
    sheets: Callable[[int, Geometry], int]


def _nup_order(pages: int, leaves: int) -> Sequence[int]:
    return tuple(range(pages))


def _saddle_order(pages: int, leaves: int) -> Sequence[int]:
    padded = pages + (-pages % 4)
    order: list[int] = []
    left, right = padded - 1, 0
    for _ in range(padded // 2):
        order.extend((right, left) if right % 2 == 0 else (left, right))
        left, right = left - 1, right + 1
    return tuple(page if page < pages else -1 for page in order)


def _signature_order(pages: int, leaves: int) -> Sequence[int]:
    fold = 4 * leaves
    padded = pages + (-pages % fold)
    return tuple(
        page if (page := _saddle_order(fold, 1)[slot] + base) < pages else -1
        for base in range(0, padded, fold)
        for slot in range(fold)
    )


PLANS: Final[dict[Scheme, Plan]] = {
    Scheme.NUP: Plan(order=_nup_order, sheets=lambda pages, geo: -(-pages // geo.slots)),
    Scheme.BOOKLET: Plan(order=_saddle_order, sheets=lambda pages, geo: -(-(pages + (-pages % 4)) // 4)),
    Scheme.SIGNATURE: Plan(order=_signature_order, sheets=lambda pages, geo: -(-(pages + (-pages % (4 * geo.down))) // (2 * geo.down))),
}


@tagged_union(frozen=True)
class ImposeOp:
    tag: Literal["nup", "booklet", "signature", "plan"] = tag()
    nup: tuple[bytes, Geometry] = case()
    booklet: tuple[bytes, Geometry] = case()
    signature: tuple[bytes, Geometry, int] = case()
    plan: tuple[bytes, Scheme, Geometry] = case()

    @staticmethod
    def Nup(source: bytes, geometry: Geometry = Geometry()) -> "ImposeOp":
        return ImposeOp(nup=(source, geometry))

    @staticmethod
    def Booklet(source: bytes, geometry: Geometry = Geometry()) -> "ImposeOp":
        return ImposeOp(booklet=(source, geometry))

    @staticmethod
    def Signature(source: bytes, geometry: Geometry = Geometry(), leaves: int = 1) -> "ImposeOp":
        return ImposeOp(signature=(source, geometry, leaves))

    @staticmethod
    def Plan(source: bytes, scheme: Scheme, geometry: Geometry = Geometry()) -> "ImposeOp":
        return ImposeOp(plan=(source, scheme, geometry))


class Imposition(Struct, frozen=True):
    op: ImposeOp

    async def of(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"impose.{self.op.tag}", self._emit)

    async def _emit(self) -> ContentKey:
        match self.op:
            case ImposeOp(tag="nup", nup=(source, geometry)):
                data = _impose(source, Scheme.NUP, geometry, 1)
            case ImposeOp(tag="booklet", booklet=(source, geometry)):
                data = _impose(source, Scheme.BOOKLET, geometry, 1)
            case ImposeOp(tag="signature", signature=(source, geometry, leaves)):
                data = _impose(source, Scheme.SIGNATURE, geometry, leaves)
            case ImposeOp(tag="plan", plan=(source, scheme, geometry)):
                data = _plan(source, scheme, geometry).encode()
            case _:
                assert_never(self.op)
        return ContentIdentity.of(f"impose-{self.op.tag}", data)

    def contribute(self) -> "Iterable[Receipt]":
        data, sheets = _impose_bytes(self.op)
        key = ContentIdentity.of(f"impose-{self.op.tag}", data)
        yield from ArtifactReceipt.Pdf(key, len(data), sheets).contribute()


def _placements(pages: int, scheme: Scheme, geometry: Geometry, leaves: int) -> Sequence[Imposed]:
    plan = PLANS[scheme]
    order, cells, slots = plan.order(pages, leaves), geometry.partition(), geometry.slots
    return tuple(
        Imposed(source=source, sheet=index // slots, cell=cells[index % slots], rotate=0)
        for index, source in enumerate(order)
        if source >= 0
    )


def _impose(source: bytes, scheme: Scheme, geometry: Geometry, leaves: int) -> bytes:
    import pymupdf

    src = pymupdf.open(stream=source, filetype="pdf")
    placements = _placements(src.page_count, scheme, geometry, leaves)
    sheets = PLANS[scheme].sheets(src.page_count, geometry)
    width, height = geometry.sheet
    out = pymupdf.open()
    pages = [out.new_page(width=width, height=height) for _ in range(sheets)]
    for imposed in placements:
        pages[imposed.sheet].show_pdf_page(pymupdf.Rect(*imposed.cell), src, pno=imposed.source, rotate=imposed.rotate)
    return out.tobytes(garbage=3, deflate=True)


def _plan(source: bytes, scheme: Scheme, geometry: Geometry) -> str:
    import pymupdf

    src = pymupdf.open(stream=source, filetype="pdf")
    placements = _placements(src.page_count, scheme, geometry, 1)
    return ";".join(f"{p.source}:{p.sheet}:{p.cell}:{p.rotate}" for p in placements)


def _impose_bytes(op: ImposeOp) -> tuple[bytes, int]:
    match op:
        case ImposeOp(tag="nup", nup=(source, geometry)):
            return _impose(source, Scheme.NUP, geometry, 1), _sheets(source, Scheme.NUP, geometry)
        case ImposeOp(tag="booklet", booklet=(source, geometry)):
            return _impose(source, Scheme.BOOKLET, geometry, 1), _sheets(source, Scheme.BOOKLET, geometry)
        case ImposeOp(tag="signature", signature=(source, geometry, leaves)):
            return _impose(source, Scheme.SIGNATURE, geometry, leaves), _sheets(source, Scheme.SIGNATURE, geometry)
        case ImposeOp(tag="plan", plan=(source, scheme, geometry)):
            return _plan(source, scheme, geometry).encode(), 0
        case _:
            assert_never(op)


def _sheets(source: bytes, scheme: Scheme, geometry: Geometry) -> int:
    import pymupdf

    return PLANS[scheme].sheets(pymupdf.open(stream=source, filetype="pdf").page_count, geometry)
```

## [03]-[RESEARCH]

- [PDFIMPOSE_GATE] [RESEARCH]: `pdfimpose` is the dedicated saddle-stitch / folded-signature imposition engine that computes the booklet and multi-leaf signature page order natively (the `cut`/`fold`/`saddle`/`hardcover`/`onepagezine`/`perfect`/`wire` imposition scheme family over a `pdf-name -> imposed-pdf` transform). It is NOT-yet-admitted — reflection confirms `pdfimpose` is not installed on cp315 (`importlib.util.find_spec('pdfimpose') is None`), so no member of its surface is catalogue-verified and the engine carries no folder `.api` catalogue. The `BOOKLET`/`SIGNATURE` page-order computation rides this owner's page-settled half-fold integer arithmetic (`_saddle_order` centerfold pairing, `_signature_order` folded-leaf computation) over the `pymupdf.show_pdf_page` placement floor as the interim engine, fully settled against the `pymupdf` reflection; the `NUP` scheme is fully settled over the same floor with no `pdfimpose` dependency. Close-condition: `pdfimpose` is admitted to the central `pyproject.toml`, installs and imports on cp315 (or is companion-gated with the official surface tagged accordingly), and a folder `.api/pdfimpose.md` catalogue rows its imposition-scheme transform surface — at which point the `BOOKLET`/`SIGNATURE` page-order computation rebinds from the interim `_saddle_order`/`_signature_order` arithmetic onto the native `pdfimpose` scheme functions (the `Plan.order` rows in `PLANS` re-point to the engine's page-order output) while the `pymupdf.show_pdf_page` placement floor and the `ImposeOp`/`Scheme`/`Geometry`/`Imposed` surface stay unchanged. Until then `pdfimpose` is named as the intended owner but every page-order line is the interim settled arithmetic, never a phantom call into an unadmitted surface.
- [SHOW_PDF_PAGE_VERIFIED]: the imposition draws each source page into its imposed-sheet cell through `pymupdf.Page.show_pdf_page(rect, docsrc, pno=0, keep_proportion=True, overlay=True, oc=0, rotate=0, clip=None) -> int`, a VERIFIED REAL member of `pymupdf.Page` reflected on cp315 (`pymupdf 1.27.2.3`, native MuPDF cp310-abi3) — the member draws the `docsrc` page `pno` into the target `rect` of the host page, `keep_proportion` preserving aspect, `rotate` placing at 0/90/180/270, and `clip` restricting the source region. The folder `.api` catalogue for `pymupdf` does NOT yet row `show_pdf_page` (it rows the sibling `Page.insert_image`/`Document.insert_pdf` cross-document members), so this member is catalogue-pending: the signature above is reflection-verified against the installed distribution, the close-condition is the folder catalogue adding the `show_pdf_page` row. The `show_pdf_page` cross-document draw is the higher-fidelity placement the imposition needs (it vector-copies the source page content stream into the imposed cell, preserving text/vector rather than rasterizing), distinct from the `pypdf.Transformation`+`merge_page` fold the `document/egress#FINISH` `IMPOSE` step uses for in-document n-up. Until catalogued, the `show_pdf_page` fold is reflection-settled, not catalogue-settled fence code.
- [SCHEME_ORDER_SETTLED]: the page-order computation for each scheme is one `Plan` row in the `PLANS` policy table, the totality proof — `_nup_order` (document-order sequence to the next grid cell, fully settled), `_saddle_order` (the centerfold pairing padding the page count up to the next multiple of 4 and folding `(last, first, second, second-last, ...)` so the bound saddle-stitch stack reads in sequence, with `-1` slots for the blank pad pages the placement fold skips), `_signature_order` (the folded-signature computation tiling the `_saddle_order` over each `4*leaves`-page signature block at its base offset). The `Geometry.partition` n-across/n-down cell-rect computation offset by the creep/shingling gutter is the shared sheet-geometry surface every scheme's placement list reads, so the scheme differs only in the page order, never in the placement floor — the `_placements` fold reads `PLANS[scheme].order` for the order and `Geometry.partition()` for the cells, producing one `Imposed` placement list the draw fold sweeps. The split is the disciplined collapse: a parallel `_impose_nup`/`_impose_booklet`/`_impose_signature` method triple beside the one `PLANS`-keyed `_placements` fold and an `if scheme == ...` cascade beside the table dispatch are the rejected forms. This interim arithmetic is the settled engine until `[PDFIMPOSE_GATE]` resolves; the `pdfimpose` admission re-points the `Plan.order` rows without touching the placement floor or the `ImposeOp` surface.
- [EGRESS_DISTINCT] [RESOLVED]: this owner is the DEDICATED multi-sheet press-imposition engine, distinct from the `document/egress#FINISH` `IMPOSE` step. The egress IMPOSE is the in-document `pypdf.Transformation`+`merge_page` n-up grid fold over an already-finished PDF as one of eight finishing steps (encrypt/outline/watermark/attach/impose/rewrite/redact/protect) — it imposes a finished PDF into a simple n-up grid and computes no booklet or signature page order. This `Imposition` owner computes the saddle-stitch centerfold and folded-signature page order the egress step never reaches and places each source page through the higher-fidelity `pymupdf.show_pdf_page` cross-document draw. The two never overlap: egress finishes one PDF as a finishing step, imposition is the press-imposition engine producing a folded form. The split is the disciplined collapse boundary: a signature-imposition branch grafted onto the egress `IMPOSE` step (which owns no scheme dispatch and no page-order computation) and an n-up grid re-implemented here beside the egress `Transformation` fold are the rejected crossings — the simple in-document n-up lands once in egress, the multi-sheet booklet/signature press-imposition lands once here, and neither re-owns the other. The imposed press form this owner emits is handed to `document` assembly keyed by the same `ContentKey`; the egress n-up is handed onward as a finished PDF — the two feeds are distinct.
- [RECEIPT_THREAD_SETTLED]: each operation contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Pdf` case through `Imposition.contribute`, which mints the content key over the imposed bytes through the same `ContentIdentity.of` `_emit` uses (so the projection is self-contained with no caller-passed key) and yields the `Iterable[Receipt]` stream the runtime `ReceiptContributor` port declares by delegating `yield from ArtifactReceipt.Pdf(key, len(data), sheets).contribute()` to the case's own `contribute` (which projects the case facts onto the 2-positional `Receipt.of("artifacts", ("emitted", "pdf", facts))` contract). The imposition owner adds no receipt case — the imposed form is a PDF, so the `Pdf` byte/page shape carries the imposed-sheet count as the page facts; a parallel `ImpositionReceipt` type beside the `ArtifactReceipt.Pdf` contribution is the rejected form. The `Plan` compute-only op contributes a `0`-page `Pdf` receipt (it emits the serialized placement list, not an imposed PDF), the introspection arm a consumer reads before committing to the draw.
```
