# [PY_ARTIFACTS_IMPOSITION]

`Imposition` owns press imposition — reordering, scaling, rotating, and cropping an already-emitted multi-page PDF onto larger imposed sheets in a press-ready form. It discriminates a closed-payload `ImposeOp` `tagged_union` by one total `match` — `Impose` the drawing case (`source`, `Scheme`, `Geometry`, `Marks`), `Proof` the `ProofPolicy`-driven RGB-screen / CMYK-separations / GRAY-density raster proof — never a per-scheme `Nup`/`Booklet`/`Signature` draw family or a `StrEnum` over an erased `dict`; the compute-only pre-flight is `Imposition.planned` over the same `Impose` payload, never a parallel op case duplicating it. It is the dedicated booklet/signature engine computing the saddle-stitch creep, the folded-signature ordering, and the work-and-turn duplexing that the simpler `document/egress#FINISH` `IMPOSE` in-document n-up step over a finished PDF never reaches. It computes the imposition and places the pages but assembles no document — the imposed sheets hand onward to `document` assembly.

`Scheme` is a closed `StrEnum`: a locally-placeable scheme binds one `PLANS` `place(pages, geometry)` computation fusing page order, recto/verso rotation, and per-sheet creep, while a provider-native scheme (`wire`/`hardcover`/`cards`/`zine`) is a `_PDFIMPOSE_SCHEMAS` row whose fold geometry only `pdfimpose` owns (the inverse of local-only `perfect-bind`); the derived `_ENGINES` route table is the ONE capability declaration admission and execution share — `ImposeOp.Impose` returns `Result[ImposeOp, ImposeFault]` and refuses a scheme-engine pair with no route BEFORE any render, so the default `Scheme.NUP` can never reach a provider registry that lacks it — and the imposed-sheet count derives from the one placement `Block`, never a second formula. `Placement`, `Geometry`, and `Marks` carry the placement axis, the binding-and-gripper-aware cell grid `partition` projects (its divisor invariants `Is`-refined so a non-positive count or negative margin rails at admission), and the press-finishing policy; each placement binds to a shared signature-group OCG through `set_ocmd` and drives its reader toggle/lock through one `set_layer` write, over the `Composed`/`ComposedKind`/`Orientation`/`PlacementPolicy` owners imported from `composition/sheet#SHEET`, never N flat duplicate `add_ocg` groups. This owner's `_composed` fold crosses as one `HOSTILE`-trait kernel onto the warm process pool — the local `pymupdf.show_pdf_page` native mutation and the pure-Python `pdfimpose` provider both hold the GIL, so a `RELEASING` thread row serializes the loop behind them, and the subinterpreter arm stays refused because the worker returns the `msgspec`-backed `Composed` — with every opened handle `with`-bracketed; `pdfimpose` is provider-contained, accepting `(BytesIO(source),)`/`BytesIO()` and returning imposed bytes plus local facts. Local imposition folds editable `Layer` rows beside the imposed bytes into `Composed.layer_rows`; `Imposition.layers` renames that evidence without repeating placement math. `Proof` emits the device raster in the exact admitted `ProofInk`/`ProofRaster` pairing, and the ICC soft-proof / out-of-gamut audit is `graphic/color/managed#MANAGED`'s `ManageOp.Managed(..., transform=IccTransform(proof=...))`, chained as a downstream `core/plan#PLAN` producer node over the proof bytes — never a re-implemented lcms2 transform here; the LOCAL `press` printer's-mark set is distinct from the `Marks.overlay` figure-overlay route to `composition/compose#COMPOSE`. Receipts thread `core/receipt#RECEIPT`'s named `ArtifactReceipt.Egress`/`Pdf`/`Preview` mints selected by `Composed.kind`; every imposition routes through the `core/plan#PLAN` `ArtifactPipeline` as a producer node.

## [01]-[INDEX]

- [02]-[IMPOSE]: the press-imposition owner discriminating a closed `ImposeOp` `tagged_union` — `Impose` (locally-placeable n-up/booklet/signature/work-and-turn/cut-and-stack/come-and-go/perfect-bind/sheetwise plus provider-native wire/hardcover/cards/zine, route-admitted through the derived `_ENGINES` table) and `Proof` (the RGB/CMYK/GRAY raster proof) — folded once into the imported `Composed` evidence struct the `folded()` successor threads, railed `RuntimeRail[ArtifactReceipt]` over `async_boundary(catch=_FAULTS)`, dispatched to the local `pymupdf.show_pdf_page` engine or the `pdfimpose` schema wrappers; `planned` is the compute-only pre-flight over the `Impose` payload.

## [02]-[IMPOSE]

- Owner: `Imposition` discriminates over the closed `ImposeOp` `expression.tagged_union`, one typed payload per case, never a `StrEnum` over a shared erased `dict`; the verb family is two cases (`Impose`/`Proof`) plus the `planned` pre-flight projection — never a `Nup`/`Booklet`/`Signature` triple differing only by a literal scheme, and never a `Plan` case duplicating `Impose`'s payload to carry a receipt-less JSON blob. `Scheme` splits into the locally-placeable set (each a `PLANS` `place(pages, geometry)` row fusing page order, recto/verso rotation, and per-sheet creep) and the provider-native set (`WIRE`/`HARDCOVER`/`CARDS`/`ZINE`, a `_PDFIMPOSE_SCHEMAS` row whose fold geometry only `pdfimpose` owns), each row's `accepts` frozenset filtering the one candidate kwarg dict so a schema never sees a kwarg it rejects; `_ENGINES` DERIVES each scheme's capable engines from those two tables — one declaration site admission (`Impose`'s `Result` ingress) and execution (`_composed`'s route) both read — and the imposed-sheet count is the derived `_sheet_count` over the one placement list, never a second `sheets` callable. `Placement` projects every field straight onto the `show_pdf_page` keyword set and feeds the one `set_layer` reader-config write; `Geometry` projects once through `partition` to the binding-aware cell grid with every field live, and its `boxes()` derivation pins the imposed form's `TrimBox`/`BleedBox` so a press tool reads the finished-work area off the page geometry; `Marks` carries the press-finishing policy. `pymupdf` owns the cross-document `show_pdf_page` draw, the imposed-sheet construction, the `add_ocg`+`set_ocmd`+`set_layer` OCG binding, the press-finish surface, and the press-faithful RGB/CMYK/GRAY proof raster; the per-scheme page-order, creep, verso-rotation, and tumble arithmetic is this owner's fold over that one floor, never a re-implemented byte emitter.
- Cases: dispatched by one total `match` — `Impose(source, scheme, geometry, marks)` admits the scheme-engine pair against `_ENGINES` (an unroutable pair is the `<no-route>` refusal BEFORE any render), then the fold resolves `PLANS[scheme]` or the provider row, opens the source (raising the `EmptyFileError` the `_FAULTS` tuple admits on a zero-page source), computes the `Placement` list and derived count, mints one shared OCG per unique signature name, binds each member through `set_ocmd`, drives one `_configure_layers` reader write, runs the `Marks` finish, and pins the derived page boxes — the saddle order, head-to-head verso rotation, per-fold creep, duplex mirror, come-and-go duplicate, and sheetwise split all carried by the resolved `PLANS` row, the case unchanged. `Proof(source, dpi, sheet, policy)` returns `Result[ImposeOp, ImposeFault]`, refuses a non-finite dpi or one whose truncated rasterizer integer is zero with `<proof-dpi>` and a negative sheet selector with `<proof-sheet>` (the count-dependent overrun stays the boundary's trapped `IndexError`), refuses an ink/codec pair outside `_PROOF_RASTERS` with `<proof-route>`, and discriminates the `sheet` selector on input shape — an `int` proofs one imposed sheet, a tuple a contact strip (empty = every sheet) — rasterizing in the `ProofPolicy` ink model and encoding in the exact admitted `ProofRaster` codec. ICC-managed soft-proof and out-of-gamut audit chain outward through `graphic/color/managed#MANAGED` `ManageOp.Managed(..., transform=IccTransform(proof=...))` as a parent-keyed downstream producer, so the proof codec never silently changes and no lcms2 transform is re-owned here. A new scheme is one `Scheme` member plus one table row — `_ENGINES` re-derives — never a new case, a per-scheme imposer sibling, a per-page draw family, or a hard-coded `rotate=0`.
- Auto: `_composed(op) -> Composed` is the ONE total `match`, executed once per path — offloaded by the async emission, landed on the `folded()` successor the `contribute`/`layers` projections read — never re-entered per projection. `Impose`'s `_admit` guard has already railed a non-positive grid count or negative extent as `BeartypeCallHintViolation` (only a direct scalar parameter is deep-checked, never a `Struct`/`Block` field) BEFORE the placement resolution feeds the native draw; `_imposed` allocates the derived `_sheet_count` on the live `out` document, mints ONE shared OCG per unique placement `name`, folds its `Layer` rows beside the output bytes, and folds `_draw_one` — each binding a `name`-bearing placement to its shared group through `set_ocmd` under `PlacementPolicy.membership` and drawing `show_pdf_page` under the same policy on the live native page, never a held `Page` list that outlives `out` — then keeps the rows that bound a real group, drives one `_configure_layers` write over the deduped groups, runs the `Marks.finished` finish, the `_press_marks` LOCAL crop/fold/registration/colour-bar draw (distinct from the `Marks.overlay` route to `composition/compose#COMPOSE`), and the `Geometry.boxes()` TrimBox/BleedBox pin, and returns the deterministic `tobytes` reading the REAL `Document.page_count`. This native-handle `Block.map`/`choose` sweep is the platform-forced seam, not a `Result[Document, Never]` fold — the engine raise the boundary converts replaces a per-element `Result` thread that can never carry an `Error`. `_grid`/`_folded_plan`/`_duplexed`/`_stacked`/`_paired`/`_split` bodies are the `PLANS` rows (NUP and PERFECT_BIND both bind `_grid`, differing by a `Geometry.spine` field; WORK_AND_TURN and WORK_AND_TUMBLE both bind `_duplexed`, differing by the `on_across` mirror-axis value — two rows over one body), each `cell` from `partition`. `planned` projects the `ImposedPlan` without drawing, keying the placement model on the RESOLVED engine — a PDFIMPOSE-engined request carries empty placements because only `pdfimpose` owns that fold geometry, so a dual-routed scheme never fabricates a local stream the provider imposition diverges from; the `Proof` arm rasterizes through `_rasterized` in the `ProofPolicy` ink model and encodes native `tobytes` or the `pil_tobytes` bridge, the `_contact` montage celling on the LARGEST selected sheet so a mixed-size gang run never mis-cells.
- Receipt: each op contributes `core/receipt#RECEIPT` off the one `Composed` fold — the `Composed.kind` discriminant plus the `Composed.layers` count select the named flat-scalar mint once, so the owner adds NO sibling factory and NO new case. An OCG-bearing `Impose` op selects `ArtifactReceipt.Egress(key, len(data), pages, 0, 0, layers)` carrying the imposed byte count, REAL page count, and minted-OCG count on the `overlays` slot (zero encryption/outline depth — imposition is neither a security nor a navigation close), a degenerate no-mark imposition the `Pdf` form; `Proof` selects `Preview(key, extent[0], extent[1], bytes_, scores)` carrying byte count and pixmap extent. `contribute` reads the successor's `composed` evidence — the un-folded owner contributes nothing, so absence stays distinct from evidence and no projection re-enters the render. `planned` contributes no receipt — its `ImposedPlan` is a pre-flight payload, never a fake `0`-page `Pdf` over plan-JSON bytes.
- Growth: a new locally-placeable scheme is one `Scheme` member plus one `PLANS` row, a new provider-native scheme one `Scheme` member plus one `_PDFIMPOSE_SCHEMAS` row carrying its `impose` function and `accepts` kwarg frozenset — `_ENGINES` re-derives both, never a parallel imposer class, an `if scheme == ...` branch, or a new `ImposeOp` case; a geometry behavior is one `Orientation` or `CreepMode` policy value or one `Geometry` field read in `partition`, the fold, or the `_pdfimpose_kwargs` dict; a placement behavior axis extends `PlacementPolicy`; a deeper signature is the same `_folded_plan` fold over a larger `Geometry.leaves`; a press-finish concern is one `Marks` field; a LOCAL printer's mark is one `PressMark` member plus one `_press_marks` arm, while a figure overlay still routes through `Marks.overlay` to `composition/compose#COMPOSE` — two distinct seams; a proof axis is one `ProofInk`/`ProofRaster` member plus one `_PROOF_RASTERS` admission row or one `ProofPolicy` field, the ICC gamut audit an outward `graphic/color/managed#MANAGED` chain. Zero new surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import math
from collections.abc import Callable, Iterable
from enum import StrEnum
from functools import partial
from io import BytesIO
from typing import TYPE_CHECKING, Annotated, Final, Literal, Self, assert_never

from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, msgpack, structs

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.faults import FAULT_CONF, TRANSIENT, FaultRow, RuntimeRail, async_boundary, rostered

from rasm.artifacts.composition.sheet import Composed, ComposedKind, Orientation, PlacementPolicy, Quarter
from rasm.artifacts.core.hooks import ArtifactsLeg
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.export.layered import Layer

lazy import pymupdf
lazy from pdfimpose import UserError as PdfImposeUserError
lazy from pdfimpose.schema import cards as pdf_cards
lazy from pdfimpose.schema import copycutfold as pdf_copycutfold
lazy from pdfimpose.schema import cutstackfold as pdf_cutstackfold
lazy from pdfimpose.schema import hardcover as pdf_hardcover
lazy from pdfimpose.schema import onepagezine as pdf_onepagezine
lazy from pdfimpose.schema import saddle as pdf_saddle
lazy from pdfimpose.schema import wire as pdf_wire

if TYPE_CHECKING:
    from pymupdf import Colorspace, Document, Page, Pixmap, Shape

    from rasm.runtime.receipts import Receipt


# --- [TYPES] ----------------------------------------------------------------------------
type Box = tuple[float, float, float, float]
type Dimensions = tuple[float, float]
type Edge = Literal["left", "right", "top", "bottom"]
type Lay = Literal["short", "long"]
type Place = Callable[[int, "Geometry"], "Block[Placement]"]
type ImposeFault = Literal["<no-route>", "<proof-route>", "<proof-dpi>", "<proof-sheet>"]

type Across = Annotated[int, Is[lambda n: n >= 1]]
type Leaves = Annotated[int, Is[lambda n: n >= 1]]
type Span = Annotated[float, Is[lambda v: v >= 0.0]]


class Scheme(StrEnum):
    NUP = "nup"
    BOOKLET = "booklet"
    SIGNATURE = "signature"
    WORK_AND_TURN = "work-and-turn"
    WORK_AND_TUMBLE = "work-and-tumble"
    CUT_AND_STACK = "cut-and-stack"
    COME_AND_GO = "come-and-go"
    PERFECT_BIND = "perfect-bind"
    SHEETWISE = "sheetwise"
    WIRE = "wire"
    HARDCOVER = (
        "hardcover"
    )
    CARDS = "cards"
    ZINE = "zine"


class ImpositionEngine(StrEnum):
    LOCAL = "local"
    PDFIMPOSE = "pdfimpose"


class CreepMode(StrEnum):
    PUSH = "push"
    SHINGLE = "shingle"


class ProofInk(StrEnum):
    RGB = "rgb"
    CMYK = "cmyk"
    GRAY = "gray"


class ProofRaster(StrEnum):
    PNG = "png"
    JPEG = "jpg"
    WEBP = "webp"
    AVIF = "avif"
    TIFF = "tiff"


class PressMark(StrEnum):
    CROP = "crop"
    FOLD = "fold"
    REGISTRATION = "registration"
    COLOR_BAR = "color-bar"


# --- [CONSTANTS] ------------------------------------------------------------------------
_FAULTS: tuple[type[BaseException], ...] = (RuntimeError, ValueError, KeyError, IndexError, OSError, BeartypeCallHintViolation, PdfImposeUserError)

_CANON: Final = msgpack.Encoder(order="deterministic")

_PIL_RASTERS: Final[frozenset[ProofRaster]] = frozenset({ProofRaster.WEBP, ProofRaster.AVIF, ProofRaster.TIFF})

_PROOF_RASTERS: Final[frozendict[ProofInk, frozenset[ProofRaster]]] = frozendict({
    ProofInk.RGB: frozenset(ProofRaster),
    ProofInk.CMYK: frozenset({ProofRaster.JPEG, ProofRaster.TIFF}),
    ProofInk.GRAY: frozenset(ProofRaster),
})

_BAR_PATCHES: Final[tuple[tuple[float, float, float], ...]] = (
    (0.0, 0.0, 0.0),
    (0.0, 1.0, 1.0),
    (1.0, 0.0, 1.0),
    (1.0, 1.0, 0.0),
    (0.75, 0.75, 0.75),
    (0.5, 0.5, 0.5),
)


# --- [BOUNDARIES] -----------------------------------------------------------------------
_GUARD = beartype(conf=FAULT_CONF)


@_GUARD
def _admit(
    across: Across, down: Across, leaves: Leaves, gutter: Span, head_trim: Span, spine: Span, creep: Span, bleed: Span, omargin: Span, gripper: Span, /
) -> None:
    return None


@_GUARD
def _admit_rotation(rotate: Quarter, /) -> None:
    return None


# --- [MODELS] ---------------------------------------------------------------------------
class Geometry(Struct, frozen=True):
    sheet: Dimensions = (1190.55, 841.89)
    orientation: Orientation = Orientation.PORTRAIT
    engine: ImpositionEngine = ImpositionEngine.LOCAL
    across: Across = 2
    down: Across = 1
    leaves: Leaves = 1
    gutter: Span = 0.0
    omargin: Span = 0.0
    head_trim: Span = 0.0
    spine: Span = 0.0
    creep: Span = 0.0
    bleed: Span = 0.0
    binding: Edge = "left"
    lay: Lay = "long"
    gripper: Span = 0.0
    creep_mode: CreepMode = CreepMode.PUSH
    last: int = 0
    back: str = ""

    @property
    def slots(self) -> int:
        return self.across * self.down

    @property
    def oriented(self) -> Dimensions:
        return self.orientation.of(self.sheet)

    def partition(self, shift: float = 0.0) -> tuple[Box, ...]:
        _admit(self.across, self.down, self.leaves, self.gutter, self.head_trim, self.spine, self.creep, self.bleed, self.omargin, self.gripper)
        width, height = self.oriented
        grip_x, grip_y = (self.gripper, 0.0) if self.lay == "short" else (0.0, self.gripper)
        cell_w = (width - 2.0 * self.omargin - (self.across + 1) * self.gutter - self.spine - grip_x) / self.across
        cell_h = (height - 2.0 * self.omargin - (self.down + 1) * self.gutter - 2.0 * self.head_trim - grip_y) / self.down
        on_x = self.binding in ("left", "right")
        creep = -shift if self.creep_mode is CreepMode.SHINGLE else shift
        dx, dy = (creep, 0.0) if on_x else (0.0, creep)
        sign = -1.0 if self.binding in ("right", "bottom") else 1.0
        ox = self.omargin + self.gutter + grip_x + (self.spine if self.binding == "left" else 0.0)
        oy = self.omargin + self.head_trim + self.gutter + grip_y
        return tuple(
            (
                ox + col * (cell_w + self.gutter) + sign * dx - self.bleed,
                oy + row * (cell_h + self.gutter) + sign * dy - self.bleed,
                ox + col * (cell_w + self.gutter) + cell_w + sign * dx + self.bleed,
                oy + row * (cell_h + self.gutter) + cell_h + sign * dy + self.bleed,
            )
            for row in range(self.down)
            for col in range(self.across)
        )

    def boxes(self) -> tuple[Box, Box]:
        cells = self.partition()
        width, height = self.oriented
        x0, y0 = min(cell[0] for cell in cells), min(cell[1] for cell in cells)
        x1, y1 = max(cell[2] for cell in cells), max(cell[3] for cell in cells)
        trim = (x0 + self.bleed, y0 + self.bleed, x1 - self.bleed, y1 - self.bleed)
        return trim, (max(x0, 0.0), max(y0, 0.0), min(x1, width), min(y1, height))


class Placement(Struct, frozen=True):
    source: int
    sheet: int
    cell: Box
    rotate: Quarter = 0
    clip: Box | None = None
    name: str = ""
    policy: PlacementPolicy = PlacementPolicy()

    def __post_init__(self) -> None:
        _admit_rotation(self.rotate)


class Marks(Struct, frozen=True):
    overlay: bool = False
    press: tuple[PressMark, ...] = ()
    controls: tuple[str, ...] = ()
    imposition_map: bool = False
    cut_list: tuple[tuple[str, bytes], ...] = ()
    bake: bool = False
    subset: bool = True
    recompress: bool = False
    scrub: bool = False
    linearize: bool = True
    info: tuple[tuple[str, str], ...] = ()
    xmp: str | None = None

    def finished(self, document: "Document", geometry: Geometry, sheets: int) -> None:
        if self.imposition_map:
            document.set_toc([[1, f"Sheet {n + 1}", n + 1] for n in range(sheets)])
        for name, payload in self.cut_list:
            document.embfile_add(name, payload, filename=name, desc="imposition press file")
        if self.bake:
            document.bake(annots=True, widgets=True)
        if self.scrub:
            document.scrub(
                hidden_text=True,
                javascript=True,
                clean_pages=True,
                embedded_files=False,
                attached_files=False,
                metadata=False,
                xml_metadata=False,
                redactions=False,
            )
        if self.recompress:
            document.rewrite_images(dpi_threshold=300, dpi_target=300, lossy=True)
        if self.subset:
            document.subset_fonts(fallback=True)
        if self.info or self.controls or self.overlay:
            marks = (*self.controls, *(("overlay",) if self.overlay else ()), geometry.binding, geometry.lay)
            document.set_metadata({**dict(self.info), "keywords": ",".join(marks)})
        if self.xmp is not None:
            document.set_xml_metadata(self.xmp)

    def serialize(self, document: "Document") -> bytes:
        return (
            document.tobytes(garbage=4, deflate=True, linear=True, no_new_id=True)
            if self.linearize
            else document.tobytes(garbage=3, deflate=True, use_objstms=1, no_new_id=True)
        )


class ImposedPlan(Struct, frozen=True):
    scheme: Scheme
    sheet: Dimensions
    sheets: int | None
    pages: int
    leaves: int
    signatures: int | None
    padded: int
    creep: float
    placements: tuple[Placement, ...]
    engine: ImpositionEngine = ImpositionEngine.LOCAL


class ProofPolicy(Struct, frozen=True):
    ink: ProofInk = ProofInk.RGB
    raster: ProofRaster = ProofRaster.PNG
    clip: Box | None = None
    tint: tuple[int, int] | None = None
    gamma: float | None = None
    negative: bool = False


class PdfImposeSchema(Struct, frozen=True):
    impose: Callable[[], Callable[..., None]]
    accepts: frozenset[str]


# --- [TABLES] ---------------------------------------------------------------------------

IMPOSE_FOLD: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.IMPOSITION, point="fold", arm="boundary", defect="impose-fold", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([IMPOSE_FOLD]))

_FOLD_KW: Final[frozenset[str]] = frozenset({"signature", "imargin", "omargin", "mark", "bind", "creep", "group", "last"})

_PDFIMPOSE_SCHEMAS: Final[Map[Scheme, PdfImposeSchema]] = Map.of_seq([
    (Scheme.BOOKLET, PdfImposeSchema(lambda: pdf_saddle.impose, _FOLD_KW)),
    (Scheme.SIGNATURE, PdfImposeSchema(lambda: pdf_saddle.impose, _FOLD_KW)),
    (Scheme.CUT_AND_STACK, PdfImposeSchema(lambda: pdf_cutstackfold.impose, _FOLD_KW)),
    (Scheme.COME_AND_GO, PdfImposeSchema(lambda: pdf_copycutfold.impose, _FOLD_KW)),
    (Scheme.HARDCOVER, PdfImposeSchema(lambda: pdf_hardcover.impose, _FOLD_KW - {"creep"})),
    (Scheme.WIRE, PdfImposeSchema(
        lambda: pdf_wire.impose, frozenset({"signature", "imargin", "omargin", "mark", "last"})
    )),
    (Scheme.CARDS, PdfImposeSchema(
        lambda: pdf_cards.impose, frozenset({"signature", "imargin", "omargin", "mark", "bind", "back"})
    )),
    (Scheme.ZINE, PdfImposeSchema(
        lambda: pdf_onepagezine.impose, frozenset({"omargin", "mark", "bind", "last"})
    )),
])


def _sheet_count(placements: Block[Placement]) -> int:
    return placements.fold(lambda acc, p: max(acc, p.sheet), -1) + 1


# --- [OPERATIONS] -----------------------------------------------------------------------
def _saddle(slots: int) -> tuple[int, ...]:
    return tuple(leaf for i in range(slots // 2) for leaf in ((i, slots - 1 - i) if i % 2 == 0 else (slots - 1 - i, i)))


def _grid(pages: int, geometry: Geometry) -> Block[Placement]:
    slots, cells = geometry.slots, geometry.partition()
    return Block.of_seq(Placement(source=page, sheet=page // slots, cell=cells[page % slots]) for page in range(pages))


def _folded_plan(pages: int, geometry: Geometry) -> Block[Placement]:
    fold, slots, across = 4 * max(geometry.leaves, 1), geometry.slots, geometry.across
    padded = pages + (-pages % fold)
    signature = _saddle(fold)
    return Block.of_seq(
        Placement(
            source=position + base,
            sheet=base // slots + slot // slots,
            cell=geometry.partition(geometry.creep * (base // fold))[slot % slots],
            rotate=180 * ((slot % slots) // across) % 360,
            name=f"sig-{base // fold + 1}",
        )
        for base in range(0, padded, fold)
        for slot, position in enumerate(signature)
        if position + base < pages
    )


def _duplexed(on_across: bool, /) -> Place:
    def place(pages: int, geometry: Geometry) -> Block[Placement]:
        slots, across, down, cells = geometry.slots, geometry.across, geometry.down, geometry.partition()

        def cell(page: int) -> Box:
            slot, col, row = page % slots, (page % slots) % across, (page % slots) // across
            mirrored = (across - 1 - col) + across * row if on_across else col + across * (down - 1 - row)
            return cells[mirrored if (page // slots) % 2 else slot]

        return Block.of_seq(Placement(source=page, sheet=page // slots, cell=cell(page)) for page in range(pages))

    return place


def _paired(pages: int, geometry: Geometry) -> Block[Placement]:
    cells = geometry.partition()
    return Block.of_seq(Placement(source=page, sheet=page, cell=cells[slot]) for page in range(pages) for slot in range(geometry.slots))


def _stacked(pages: int, geometry: Geometry) -> Block[Placement]:
    slots, cells = geometry.slots, geometry.partition()
    stack = -(-pages // slots)
    return Block.of_seq(Placement(source=page, sheet=page % stack, cell=cells[page // stack]) for page in range(pages))


def _split(pages: int, geometry: Geometry) -> Block[Placement]:
    slots, cells = geometry.slots, geometry.partition()
    return Block.of_seq(Placement(source=page, sheet=2 * (page // 2 // slots) + page % 2, cell=cells[page // 2 % slots]) for page in range(pages))


PLANS: Final[Map[Scheme, Place]] = Map.of_seq([
    (Scheme.NUP, _grid),
    (Scheme.BOOKLET, lambda pages, geo: _folded_plan(pages, structs.replace(geo, leaves=1))),
    (Scheme.SIGNATURE, _folded_plan),
    (Scheme.WORK_AND_TURN, _duplexed(on_across=True)),
    (Scheme.WORK_AND_TUMBLE, _duplexed(on_across=False)),
    (Scheme.CUT_AND_STACK, _stacked),
    (Scheme.COME_AND_GO, _paired),
    (Scheme.PERFECT_BIND, _grid),
    (Scheme.SHEETWISE, _split),
])

_ENGINES: Final[frozendict[Scheme, frozenset[ImpositionEngine]]] = frozendict({
    scheme: frozenset(
        (
            *((ImpositionEngine.LOCAL,) if scheme in PLANS else ()),
            *((ImpositionEngine.PDFIMPOSE,) if scheme in _PDFIMPOSE_SCHEMAS else ()),
        )
    )
    for scheme in Scheme
})


@tagged_union(frozen=True)
class ImposeOp:
    tag: Literal["impose", "proof"] = tag()
    impose: tuple[bytes, Scheme, Geometry, Marks] = case()
    proof: tuple[bytes, float, int | tuple[int, ...], ProofPolicy] = case()

    @staticmethod
    def Impose(
        source: bytes, scheme: Scheme = Scheme.NUP, geometry: Geometry = Geometry(), marks: Marks = Marks()
    ) -> Result["ImposeOp", ImposeFault]:
        return (
            Ok(ImposeOp(impose=(source, scheme, geometry, marks)))
            if geometry.engine in _ENGINES[scheme]
            else Error("<no-route>")
        )

    @staticmethod
    def Proof(
        source: bytes, dpi: float = 96.0, sheet: int | tuple[int, ...] = 0, policy: ProofPolicy = ProofPolicy()
    ) -> Result["ImposeOp", ImposeFault]:
        indices = sheet if isinstance(sheet, tuple) else (sheet,)
        return (
            Error("<proof-dpi>")
            if not (math.isfinite(dpi) and int(dpi) > 0)
            else Error("<proof-sheet>")
            if any(index < 0 for index in indices)
            else Error("<proof-route>")
            if policy.raster not in _PROOF_RASTERS[policy.ink]
            else Ok(ImposeOp(proof=(source, dpi, sheet, policy)))
        )


# --- [COMPOSITION] ----------------------------------------------------------------------
class Imposition(Struct, frozen=True):
    op: ImposeOp
    lane: LanePolicy
    composed: Option[Composed] = Nothing

    def emit(self, /) -> ArtifactWork:
        key = self._key
        return ArtifactWork(key=key, work=partial(self._emit, key), parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key(f"impose-{self.op.tag}", _CANON.encode(self.op))

    def folded(self) -> Self:
        return structs.replace(self, composed=Some(_composed(self.op)))

    async def _emit(self, key: ContentKey, /) -> RuntimeRail[ArtifactReceipt]:
        return await async_boundary(IMPOSE_FOLD, partial(self._folded, key), catch=_FAULTS)

    async def _folded(self, key: ContentKey, /) -> ArtifactReceipt:
        crossed = await self.lane.offload(Kernel.of(_composed, KernelTrait.HOSTILE), self.op)
        return self._receipt(key, crossed.default_with(_impose_raise))

    def _receipt(self, key: ContentKey, composed: Composed, /) -> ArtifactReceipt:
        match composed.kind:
            case ComposedKind.PDF if composed.layers:
                return ArtifactReceipt.Egress(key, len(composed.data), composed.pages, 0, 0, composed.layers)
            case ComposedKind.PDF:
                return ArtifactReceipt.Pdf(key, len(composed.data), composed.pages)
            case ComposedKind.PREVIEW:
                return ArtifactReceipt.Preview(key, composed.extent[0], composed.extent[1], len(composed.data), composed.scores)
            case _:
                assert_never(composed.kind)

    def planned(self) -> Option[ImposedPlan]:
        match self.op:
            case ImposeOp(tag="impose", impose=(source, scheme, geometry, _)):
                return Some(_planned(source, scheme, geometry))
            case ImposeOp(tag="proof"):
                return Nothing
            case _ as unreachable:
                assert_never(unreachable)

    def contribute(self) -> "Iterable[Receipt]":
        yield from self.composed.map(lambda live: tuple(self._receipt(self._key, live).contribute())).default_value(())

    def layers(self, names: tuple[str, ...] = ()) -> tuple[Layer, ...]:
        return self.composed.map(lambda live: Layer.renamed(live.layer_rows, names)).default_value(())


def _impose_raise(fault: object) -> Composed:
    raise ValueError(str(fault))


def _composed(op: ImposeOp) -> Composed:
    match op:
        case ImposeOp(tag="impose", impose=(source, scheme, Geometry(engine=ImpositionEngine.PDFIMPOSE) as geometry, marks)):
            return _pdfimposed(source, scheme, geometry, marks)
        case ImposeOp(tag="impose", impose=(source, scheme, geometry, marks)):
            with pymupdf.open(
                stream=source, filetype="pdf"
            ) as src:
                return _imposed(src, source, geometry, marks, PLANS[scheme](src.page_count, geometry))
        case ImposeOp(tag="proof", proof=(source, dpi, sheet, policy)):
            with pymupdf.open(stream=source, filetype="pdf") as src:
                pixmap = _contact(src, sheet, int(dpi), policy) if isinstance(sheet, tuple) else _rasterized(src[sheet], int(dpi), policy)
                return Composed(_encoded(pixmap, policy.raster), pages=1, kind=ComposedKind.PREVIEW, extent=(pixmap.width, pixmap.height))
        case _:
            assert_never(op)


def _pdfimposed(source: bytes, scheme: Scheme, geometry: Geometry, marks: Marks, /) -> Composed:
    schema = _PDFIMPOSE_SCHEMAS[scheme]
    sink = BytesIO()
    schema.impose()((BytesIO(source),), sink, **_pdfimpose_kwargs(schema, scheme, geometry, marks))
    with pymupdf.open(stream=sink.getvalue(), filetype="pdf") as out:
        marks.finished(out, geometry, out.page_count)
        return Composed(data=marks.serialize(out), pages=out.page_count)


def _pdfimpose_kwargs(schema: PdfImposeSchema, scheme: Scheme, geometry: Geometry, marks: Marks, /) -> dict[str, object]:
    crop_marks = "crop" in marks.controls or marks.overlay
    candidate: dict[str, object] = {
        "signature": (geometry.across, geometry.down),
        "imargin": geometry.gutter,
        "omargin": geometry.omargin,
        "mark": ["crop"] if crop_marks else [],
        "bind": geometry.binding,
        "creep": lambda sheets: geometry.creep * max(sheets - 1, 0),
        "group": 1 if scheme is Scheme.BOOKLET else geometry.leaves,
        "last": geometry.last,
        "back": geometry.back,
    }
    return {name: value for name, value in candidate.items() if name in schema.accepts}


def _imposed(src: "Document", source: bytes, geometry: Geometry, marks: Marks, placements: Block[Placement], /) -> Composed:
    sheets = _sheet_count(placements)
    width, height = geometry.oriented
    with pymupdf.open() as out:
        for _ in range(sheets):
            out.new_page(width=width, height=height)
        groups = _mint_groups(out, placements)
        boxes = placements.fold(
            lambda acc, p: acc.add(p.name, _union(acc.try_find(p.name).default_value(p.cell), p.cell)) if p.name else acc, Map.empty()
        )
        ordered = tuple(dict.fromkeys(p.name for p in placements if p.name))
        layer_rows = tuple(Layer(name, source, boxes[name]) for name in ordered)
        minted = placements.map(lambda p: _draw_one(out, src, p, groups)).choose(
            lambda drawn: Some(drawn) if drawn[0] else Nothing
        )
        _configure_layers(out, minted)
        marks.finished(out, geometry, sheets)
        _press_marks(out, geometry, marks.press)
        trim, bleed_box = geometry.boxes()
        for page in out:
            page.set_trimbox(pymupdf.Rect(*trim))
            if geometry.bleed:
                page.set_bleedbox(pymupdf.Rect(*bleed_box))
        return Composed(data=marks.serialize(out), pages=out.page_count, layers=len(groups), layer_rows=layer_rows)


def _mint_groups(out: "Document", placements: Block[Placement], /) -> "frozendict[str, int]":
    names = tuple(dict.fromkeys(p.name for p in placements if p.name))
    return frozendict({name: out.add_ocg(name, on=True, intent="View", usage="Artwork") for name in names})


def _draw_one(
    out: "Document", src: "Document", p: Placement, groups: "frozendict[str, int]"
) -> tuple[int, bool, bool]:
    group = groups.get(p.name, 0) if p.name else 0
    oc = out.set_ocmd(ocgs=[group], policy=p.policy.membership.value) if group else 0
    out[p.sheet].show_pdf_page(
        pymupdf.Rect(*p.cell),
        src,
        pno=p.source,
        keep_proportion=p.policy.keep_proportion,
        overlay=p.policy.overlay,
        rotate=p.rotate,
        clip=pymupdf.Rect(*p.clip) if p.clip is not None else None,
        oc=oc,
    )
    return group, p.policy.visible, p.policy.locked


def _configure_layers(out: "Document", minted: Block[tuple[int, bool, bool]], /) -> None:
    if not minted.is_empty():
        groups = {xref for xref, _visible, _locked in minted}
        hidden = groups - {xref for xref, visible, _locked in minted if visible}
        out.set_layer(-1, on=list(groups - hidden), off=list(hidden), locked=list({xref for xref, _visible, locked in minted if locked}))


def _press_marks(out: "Document", geometry: Geometry, marks: tuple[PressMark, ...], /) -> None:
    if not marks:
        return
    width, height = geometry.oriented
    cells = geometry.partition()
    for page in out:
        shape = page.new_shape()
        for mark in marks:
            match mark:
                case PressMark.CROP:
                    _crop_marks(shape, cells)
                    shape.finish(color=(0.0, 0.0, 0.0), width=0.4)
                case PressMark.FOLD:
                    _fold_marks(shape, cells, height, geometry)
                    shape.finish(color=(0.0, 0.0, 0.0), width=0.3, dashes="[2 2] 0")
                case PressMark.REGISTRATION:
                    _registration_marks(shape, width, height, geometry.omargin or 12.0)
                    shape.finish(color=(0.0, 0.0, 0.0), width=0.4)
                case PressMark.COLOR_BAR:
                    _color_bar(shape, height, geometry.omargin or 12.0)
                case _ as unreachable:
                    assert_never(unreachable)
        shape.commit()


def _crop_marks(shape: "Shape", cells: tuple[Box, ...], /) -> None:
    gap, tick = 3.0, 9.0
    for x0, y0, x1, y1 in cells:
        for cx, cy, sx, sy in ((x0, y0, -1.0, -1.0), (x1, y0, 1.0, -1.0), (x0, y1, -1.0, 1.0), (x1, y1, 1.0, 1.0)):
            shape.draw_line(pymupdf.Point(cx + sx * gap, cy), pymupdf.Point(cx + sx * (gap + tick), cy))
            shape.draw_line(pymupdf.Point(cx, cy + sy * gap), pymupdf.Point(cx, cy + sy * (gap + tick)))


def _fold_marks(shape: "Shape", cells: tuple[Box, ...], height: float, geometry: Geometry, /) -> None:
    for col in range(1, geometry.across):
        x = (cells[col - 1][2] + cells[col][0]) / 2.0
        shape.draw_line(pymupdf.Point(x, 0.0), pymupdf.Point(x, height))


def _registration_marks(shape: "Shape", width: float, height: float, inset: float, /) -> None:
    radius, arm = 6.0, 9.0
    for cx, cy in ((width / 2.0, inset / 2.0), (width / 2.0, height - inset / 2.0), (inset / 2.0, height / 2.0), (width - inset / 2.0, height / 2.0)):
        shape.draw_circle(pymupdf.Point(cx, cy), radius)
        shape.draw_circle(pymupdf.Point(cx, cy), radius / 2.0)
        shape.draw_line(pymupdf.Point(cx - arm, cy), pymupdf.Point(cx + arm, cy))
        shape.draw_line(pymupdf.Point(cx, cy - arm), pymupdf.Point(cx, cy + arm))


def _color_bar(shape: "Shape", height: float, inset: float, /) -> None:
    size = 10.0
    for index, rgb in enumerate(_BAR_PATCHES):
        x = inset + index * size
        shape.draw_rect(pymupdf.Rect(x, height - inset - size, x + size, height - inset))
        shape.finish(color=(0.0, 0.0, 0.0), fill=rgb, width=0.2)


def _colorspace(ink: ProofInk, /) -> "Colorspace":
    match ink:
        case ProofInk.RGB:
            return pymupdf.csRGB
        case ProofInk.CMYK:
            return pymupdf.csCMYK
        case ProofInk.GRAY:
            return pymupdf.csGRAY
        case _ as unreachable:
            assert_never(unreachable)


def _rasterized(page: "Page", dpi: int, policy: ProofPolicy, /) -> "Pixmap":
    pixmap = page.get_pixmap(dpi=dpi, colorspace=_colorspace(policy.ink), clip=pymupdf.Rect(*policy.clip) if policy.clip is not None else None)
    if policy.tint is not None:
        pixmap.tint_with(policy.tint[0], policy.tint[1])
    if policy.gamma is not None:
        pixmap.gamma_with(policy.gamma)
    if policy.negative:
        pixmap.invert_irect()
    return pixmap


def _encoded(pixmap: "Pixmap", raster: ProofRaster, /) -> bytes:
    return pixmap.pil_tobytes(raster.value) if raster in _PIL_RASTERS else pixmap.tobytes(raster.value)


def _contact(src: "Document", sheets: tuple[int, ...], dpi: int, policy: ProofPolicy, /) -> "Pixmap":
    pages = sheets or tuple(range(src.page_count))
    columns = math.isqrt(len(pages) - 1) + 1
    cell_w, cell_h = max(src[index].rect.width for index in pages), max(src[index].rect.height for index in pages)
    with pymupdf.open() as montage:
        montage.new_page(width=cell_w * columns, height=cell_h * -(-len(pages) // columns))
        for slot, index in enumerate(pages):
            montage[0].show_pdf_page(
                pymupdf.Rect(slot % columns * cell_w, slot // columns * cell_h, (slot % columns + 1) * cell_w, (slot // columns + 1) * cell_h),
                src,
                pno=index,
            )
        return _rasterized(montage[0], dpi, policy)


def _planned(source: bytes, scheme: Scheme, geometry: Geometry) -> ImposedPlan:
    with pymupdf.open(stream=source, filetype="pdf") as src:
        pages = src.page_count
    local = geometry.engine is ImpositionEngine.LOCAL
    placements = PLANS[scheme](pages, geometry) if local else Block.empty()
    sheets, folded = (_sheet_count(placements) if local else None), scheme in (Scheme.BOOKLET, Scheme.SIGNATURE)
    leaves = 1 if scheme is Scheme.BOOKLET else geometry.leaves
    fold = 4 * max(leaves, 1)
    return ImposedPlan(
        scheme=scheme,
        sheet=geometry.oriented,
        sheets=sheets,
        pages=pages,
        leaves=leaves,
        signatures=-(-pages // fold) if folded else sheets,
        padded=(-pages % fold) if folded else 0,
        creep=geometry.creep * max(-(-pages // fold) - 1, 0) if folded else 0.0,
        placements=tuple(placements),
        engine=geometry.engine,
    )


def _union(left: Box, right: Box) -> Box:
    return (min(left[0], right[0]), min(left[1], right[1]), max(left[2], right[2]), max(left[3], right[3]))


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "CreepMode",
    "Geometry",
    "ImposeFault",
    "ImposeOp",
    "ImposedPlan",
    "Imposition",
    "ImpositionEngine",
    "Marks",
    "PLANS",
    "Placement",
    "PressMark",
    "ProofInk",
    "ProofPolicy",
    "ProofRaster",
    "Scheme",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
