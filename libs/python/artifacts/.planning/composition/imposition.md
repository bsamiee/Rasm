# [PY_ARTIFACTS_IMPOSITION]

`Imposition` owns press imposition — reordering, scaling, rotating, and cropping an already-emitted multi-page PDF onto larger imposed sheets in a press-ready form. It discriminates a closed-payload `ImposeOp` `tagged_union` by one total `match` — `Impose` the drawing case (`source`, `Scheme`, `Geometry`, `Marks`), `Proof` the `ProofPolicy`-driven RGB-screen / CMYK-separations / GRAY-density raster proof — never a per-scheme `Nup`/`Booklet`/`Signature` draw family or a `StrEnum` over an erased `dict`; the compute-only pre-flight is `Imposition.planned` over the same `Impose` payload, never a parallel op case duplicating it. It is the dedicated booklet/signature engine computing the saddle-stitch creep, the folded-signature ordering, and the work-and-turn duplexing that the simpler `document/egress#FINISH` `IMPOSE` in-document n-up step over a finished PDF never reaches. It computes the imposition and places the pages but assembles no document — the imposed sheets hand onward to `document` assembly.

`Scheme` is a closed `StrEnum`: a locally-placeable scheme binds one `PLANS` `place(pages, geometry)` computation fusing page order, recto/verso rotation, and per-sheet creep, while a provider-native scheme (`wire`/`hardcover`/`cards`/`zine`) is a `_PDFIMPOSE_SCHEMAS` row whose fold geometry only `pdfimpose` owns (the inverse of local-only `perfect-bind`); the derived `_ENGINES` route table is the ONE capability declaration admission and execution share — `ImposeOp.Impose` returns `Result[ImposeOp, ImposeFault]` and refuses a scheme-engine pair with no route BEFORE any render, so the default `Scheme.NUP` can never reach a provider registry that lacks it — and the imposed-sheet count derives from the one placement `Block`, never a second formula. `Placement`, `Geometry`, and `Marks` carry the placement axis, the binding-and-gripper-aware cell grid `partition` projects (its divisor invariants `Is`-refined so a non-positive count or negative margin rails at admission), and the press-finishing policy; each placement binds to a shared signature-group OCG through `set_ocmd` and drives its reader toggle/lock through one `set_layer` write, over the `Composed`/`ComposedKind`/`Orientation`/`PlacementPolicy` owners imported from `composition/sheet#SHEET`, never N flat duplicate `add_ocg` groups. This owner's `_composed` fold crosses as one `HOSTILE`-trait kernel onto the warm process pool — the local `pymupdf.show_pdf_page` native mutation and the pure-Python `pdfimpose` provider both hold the GIL, so a `RELEASING` thread row serializes the loop behind them, and the subinterpreter arm stays refused because the worker returns the `msgspec`-backed `Composed` — with every opened handle `with`-bracketed; `pdfimpose` is provider-contained, accepting `(BytesIO(source),)`/`BytesIO()` and returning imposed bytes plus local facts. Local imposition folds editable `Layer` rows beside the imposed bytes into `Composed.layer_rows`; `Imposition.layers` renames that evidence without repeating placement math. `Proof` emits the device raster in the exact admitted `ProofInk`/`ProofRaster` pairing, and the ICC soft-proof / out-of-gamut audit is `graphic/color/managed#MANAGED`'s `ManageOp.Managed(..., transform=IccTransform(proof=...))`, chained as a downstream `core/plan#PLAN` producer node over the proof bytes — never a re-implemented lcms2 transform here; the LOCAL `press` printer's-mark set is distinct from the `Marks.overlay` figure-overlay route to `composition/compose#COMPOSE`. Receipts thread `core/receipt#RECEIPT`'s named `ArtifactReceipt.Egress`/`Pdf`/`Preview` mints selected by `Composed.kind`; every imposition routes through the `core/plan#PLAN` `ArtifactPipeline` as a producer node.

## [01]-[INDEX]

- [01]-[IMPOSE]: the press-imposition owner discriminating a closed `ImposeOp` `tagged_union` — `Impose` (locally-placeable n-up/booklet/signature/work-and-turn/cut-and-stack/come-and-go/perfect-bind/sheetwise plus provider-native wire/hardcover/cards/zine, route-admitted through the derived `_ENGINES` table) and `Proof` (the RGB/CMYK/GRAY raster proof) — folded once into the imported `Composed` evidence struct the `folded()` successor threads, railed `RuntimeRail[ArtifactReceipt]` over `async_boundary(catch=_FAULTS)`, dispatched to the local `pymupdf.show_pdf_page` engine or the `pdfimpose` schema wrappers; `planned` is the compute-only pre-flight over the `Impose` payload.

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
from io import BytesIO
from typing import TYPE_CHECKING, Annotated, Final, Literal, Self, assert_never

from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, msgpack, structs

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.faults import RuntimeRail, async_boundary

from rasm.artifacts.composition.sheet import Composed, ComposedKind, Orientation, PlacementPolicy, Quarter
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
type Lay = Literal["short", "long"]  # the press lay/gripper edge — feed direction, distinct from the binding edge
type Place = Callable[[int, "Geometry"], "Block[Placement]"]
type ImposeFault = Literal["<no-route>", "<proof-route>", "<proof-dpi>", "<proof-sheet>"]

# Geometry invariants the `partition`/`_folded_plan` arithmetic divides by: `Across`/`Leaves`/`Span` are the
# DIRECT scalar parameters `_admit` deep-checks (msgspec ignores `Is` at construction, so the scalar seam is the
# sole enforcement site). `Quarter` — the 0/90/180/270 the `show_pdf_page` `rotate` keyword admits — is sheet's owner, composed here.
type Across = Annotated[int, Is[lambda n: n >= 1]]
type Leaves = Annotated[int, Is[lambda n: n >= 1]]
type Span = Annotated[float, Is[lambda v: v >= 0.0]]  # a non-negative gutter/trim/spine/bleed extent


class Scheme(StrEnum):
    # locally-placeable schemes carry a `PLANS` row; provider-native schemes carry a `_PDFIMPOSE_SCHEMAS` row
    # ONLY — their fold geometry has no local show_pdf_page equivalent, `PERFECT_BIND` the inverse (local-only).
    NUP = "nup"
    BOOKLET = "booklet"
    SIGNATURE = "signature"
    WORK_AND_TURN = "work-and-turn"
    WORK_AND_TUMBLE = "work-and-tumble"
    CUT_AND_STACK = "cut-and-stack"
    COME_AND_GO = "come-and-go"
    PERFECT_BIND = "perfect-bind"
    SHEETWISE = "sheetwise"
    WIRE = "wire"  # provider-native: individual pages cut, stacked, wire/spiral-bound — the AEC drawing-set / spec-book bindery form (pdfimpose.schema.wire)
    HARDCOVER = (
        "hardcover"  # provider-native: sewn folded-signature book, distinct from the local `_folded_plan` saddle block (pdfimpose.schema.hardcover)
    )
    CARDS = "cards"  # provider-native: front/back sample/swatch/keynote cards, `Geometry.back` orders the verso sources (pdfimpose.schema.cards)
    ZINE = "zine"  # provider-native: single-sheet 8-page fold-zine, fixed 2x4 fold (pdfimpose.schema.onepagezine)


class ImpositionEngine(StrEnum):
    LOCAL = "local"  # local placement facts over pymupdf show_pdf_page
    PDFIMPOSE = "pdfimpose"  # admitted pdfimpose schema wrapper, normalized back to local facts/receipts


class CreepMode(StrEnum):
    PUSH = "push"
    SHINGLE = "shingle"


class ProofInk(StrEnum):  # the Proof colorspace axis — pymupdf `get_pixmap(colorspace=)` selects the ink model
    RGB = "rgb"  # csRGB screen proof (default); pairs with a PNG/WEBP/AVIF raster
    CMYK = "cmyk"  # csCMYK press-separations proof a bindery reads; pairs with a CMYK-capable JPEG/TIFF raster
    GRAY = "gray"  # csGRAY single-ink density proof


class ProofRaster(StrEnum):
    # Proof egress codec — native `Pixmap.tobytes` vs the `pil_tobytes` bridge; PSD is DELETED capability:
    # MuPDF's native writer faults `cannot seek in buffer` on the in-memory tobytes path (file-only `Pixmap.save`)
    # and Pillow reads PSD without writing it, so no in-memory route exists on either engine.
    PNG = "png"  # native tobytes RGB/GRAY raster (default)
    JPEG = "jpg"  # native tobytes CMYK-capable separations raster — the press-proof interchange codec
    WEBP = "webp"  # pil_tobytes bridge — a format MuPDF's native tobytes lacks
    AVIF = "avif"  # pil_tobytes bridge
    TIFF = "tiff"  # pil_tobytes bridge — the lossless CMYK-capable container


class PressMark(StrEnum):  # the local press-form printer's-mark set drawn at the imposed-cell/sheet boundaries
    CROP = "crop"  # L-shaped trim ticks at each imposed-cell corner — the guillotine cut guides
    FOLD = "fold"  # dashed fold lines down the inter-column gutters — the signature fold axes
    REGISTRATION = "registration"  # concentric target + crosshair at each sheet-edge midpoint — multi-plate alignment
    COLOR_BAR = "color-bar"  # a CMYK + gray control-patch row along the foot margin — the densitometer strip


# --- [CONSTANTS] ------------------------------------------------------------------------
# pymupdf raises `FileDataError`/`EmptyFileError` (both `RuntimeError`-derived) on a corrupt or
# zero-page source, a `PLANS`/`_PDFIMPOSE_SCHEMAS` miss raises `KeyError`, a malformed `Rect(*cell)`
# extent raises `ValueError`, an out-of-range `Proof` sheet index raises `IndexError`, the source
# stream raises `OSError`, pdfimpose raises `PdfImposeUserError` (a direct `BaseException` subclass a
# bare `except Exception` never catches), and `_GUARD` raises `BeartypeCallHintViolation` on a
# non-positive grid count or off-axis verso rotation.
_FAULTS: tuple[type[BaseException], ...] = (RuntimeError, ValueError, KeyError, IndexError, OSError, BeartypeCallHintViolation, PdfImposeUserError)

_CANON: Final = msgpack.Encoder(order="deterministic")  # the stable preimage encoding the bare `ContentIdentity.key` mint addresses

# Proof raster codecs MuPDF's native `Pixmap.tobytes` lacks — routed through the `Pixmap.pil_tobytes`
# Pillow bridge; every other `ProofRaster` (PNG, JPEG) rides the native encoder.
_PIL_RASTERS: Final[frozenset[ProofRaster]] = frozenset({ProofRaster.WEBP, ProofRaster.AVIF, ProofRaster.TIFF})

_PROOF_RASTERS: Final[frozendict[ProofInk, frozenset[ProofRaster]]] = frozendict({
    ProofInk.RGB: frozenset(ProofRaster),
    ProofInk.CMYK: frozenset({ProofRaster.JPEG, ProofRaster.TIFF}),
    ProofInk.GRAY: frozenset(ProofRaster),
})

# Colour control-bar patches (registration black, C, M, Y, K, quarter/half gray) as pymupdf RGB float-triple
# `draw_rect` fills — the densitometer strip a press operator reads against a proof; a new patch is one row.
_BAR_PATCHES: Final[tuple[tuple[float, float, float], ...]] = (
    (0.0, 0.0, 0.0),
    (0.0, 1.0, 1.0),
    (1.0, 0.0, 1.0),
    (1.0, 1.0, 0.0),
    (0.75, 0.75, 0.75),
    (0.5, 0.5, 0.5),
)


# --- [BOUNDARIES] -----------------------------------------------------------------------
# `_admit` takes the grid counts, leaf count, and span extents as DIRECT `Is`-refined scalar parameters that
# beartype deep-checks (a `Struct` field or a `Block`/`tuple` element is NOT deep-checked — only a direct
# scalar is), raising `BeartypeCallHintViolation` before the divide; `Geometry.partition` calls it at the site.
_GUARD = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))


@_GUARD
def _admit(
    across: Across, down: Across, leaves: Leaves, gutter: Span, head_trim: Span, spine: Span, creep: Span, bleed: Span, omargin: Span, gripper: Span, /
) -> None:
    return None  # the `@_GUARD` beartype contract IS the work — every `Is`-refined scalar is deep-checked on call


@_GUARD
def _admit_rotation(rotate: Quarter, /) -> None:
    return None  # `Placement.__post_init__` calls this on EVERY construction, so an off-axis rotate rails before `show_pdf_page` sees it


# --- [MODELS] ---------------------------------------------------------------------------
class Geometry(Struct, frozen=True):
    sheet: Dimensions = (1190.55, 841.89)
    orientation: Orientation = Orientation.PORTRAIT
    engine: ImpositionEngine = ImpositionEngine.LOCAL
    across: Across = 2
    down: Across = 1
    leaves: Leaves = 1  # signature leaf count — the fold depth a saddle/signature block reads
    gutter: Span = 0.0  # inner binding-edge gutter (imargin) between facing cells
    omargin: Span = 0.0  # outer/trim margin insetting the cell grid on every edge (pdfimpose `omargin`)
    head_trim: Span = 0.0  # top/bottom finished-trim allowance
    spine: Span = 0.0  # perfect-bind glue/spine allowance at the binding edge
    creep: Span = 0.0  # per-fold creep compensating fold-thickness drift
    bleed: Span = 0.0  # cell expansion past the trim box
    binding: Edge = "left"
    lay: Lay = "long"  # the press feed edge the grippers seize, reserving the `gripper` margin there
    gripper: Span = 0.0  # the unprintable claw margin reserved on the `lay` feed edge
    creep_mode: CreepMode = CreepMode.PUSH
    last: int = 0  # trailing pages pinned to the document end (back-cover), blank-filled before them (pdfimpose `last`)
    back: str = ""  # (CARDS only) the verso-source ordering token pdfimpose `cards.impose(back=)` reads

    @property
    def slots(self) -> int:
        return self.across * self.down

    @property
    def oriented(self) -> Dimensions:
        return self.orientation.of(self.sheet)

    def partition(self, shift: float = 0.0) -> tuple[Box, ...]:
        _admit(self.across, self.down, self.leaves, self.gutter, self.head_trim, self.spine, self.creep, self.bleed, self.omargin, self.gripper)
        # `shift` is the per-signature creep the fold passes and `shingle` flips its direction (push-out vs
        # shingle-in); `omargin` insets the whole grid, `gripper` reserves the claw margin on the `lay` feed edge.
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
        # Derives the (TrimBox, BleedBox) pair off the base cell grid: trim is the finished-work union with the
        # bleed expansion removed, bleed the raw cell union clamped to the sheet — the page-box pin a press tool
        # and PDF/X preflight read off the imposed form.
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
    rotate: Quarter = 0  # verso head-to-head flip — the `show_pdf_page` `rotate` admits 0/90/180/270 only
    clip: Box | None = None
    name: str = ""  # the SHARED signature-group label — one OCG per unique name, the OCMD nests each member (never N flat `sig-N` groups)
    policy: PlacementPolicy = PlacementPolicy()

    def __post_init__(self) -> None:
        # msgspec ignores `Is` at construction, so the public boundary re-enters the `_GUARD` scalar seam here —
        # plan builders and foreign constructors alike rail an off-axis rotate at mint, never inside `_draw_one`.
        _admit_rotation(self.rotate)


class Marks(Struct, frozen=True):
    overlay: bool = False  # route a FIGURE-placement overlay SHAPE to composition/compose#COMPOSE, not the press-form cell grid
    press: tuple[PressMark, ...] = ()  # the LOCAL crop/fold/registration/colour-bar marks the `overlay` seam never reaches
    controls: tuple[str, ...] = ()  # print-control mark names the press info dict records
    imposition_map: bool = False  # author a per-signature outline so a reader/press navigates each fold section
    cut_list: tuple[tuple[str, bytes], ...] = ()  # cut-list / fold-map / job-ticket files embedded as PDF associated files
    bake: bool = False  # flatten interactive annotations/widgets into content before press
    subset: bool = True
    recompress: bool = False
    scrub: bool = False  # redaction-grade hidden-content/metadata removal before press
    linearize: bool = True  # deterministic fast-web-view save (pymupdf `linear=`) with garbage collection and deflate
    info: tuple[tuple[str, str], ...] = ()
    xmp: str | None = None

    def finished(self, document: "Document", geometry: Geometry, sheets: int) -> None:
        # Live-native-handle press-finish seam: a `for`/`if` mutation chain over the one document, never a fold.
        if self.imposition_map:  # set_toc page numbers are 1-based
            document.set_toc([[1, f"Sheet {n + 1}", n + 1] for n in range(sheets)])
        for name, payload in self.cut_list:  # cut-list/fold-map/job-ticket rides as a PDF associated file
            document.embfile_add(name, payload, filename=name, desc="imposition press file")
        if self.bake:
            document.bake(annots=True, widgets=True)
        if self.scrub:  # strip hostile/hidden content but PRESERVE the press files just attached and the explicit info/XMP writes below
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
            # `overlay` route marker rides the press keywords so the downstream compose owner reading the
            # imposed PDF knows the registration/crop/colour-bar overlay was requested for this lay.
            marks = (*self.controls, *(("overlay",) if self.overlay else ()), geometry.binding, geometry.lay)
            document.set_metadata({**dict(self.info), "keywords": ",".join(marks)})
        if self.xmp is not None:
            document.set_xml_metadata(self.xmp)

    def serialize(self, document: "Document") -> bytes:
        # `no_new_id=True` suppresses the randomized `/ID` so the imposition is byte-identical run-to-run (one
        # stable `ContentKey`); `linearize` selects pymupdf's DISTINCT `linear` fast-web-view save — object-stream
        # compression (`use_objstms`) stays an independent storage concern, never relabeled as linearization.
        return (
            document.tobytes(garbage=4, deflate=True, linear=True, no_new_id=True)
            if self.linearize
            else document.tobytes(garbage=3, deflate=True, use_objstms=1, no_new_id=True)
        )


class ImposedPlan(Struct, frozen=True):
    scheme: Scheme
    sheet: Dimensions  # the oriented imposed press-sheet size a press operator validates the plan against before committing the draw
    sheets: int | None  # imposed press sheets the placement fold yields; None when only the provider owns the fold geometry
    pages: int  # source pages read off the live document
    leaves: int  # signature leaf count the fold depth reads
    signatures: int | None  # bound folding units (ceil(pages / 4·leaves) for a folded scheme, sheets otherwise — None with them)
    padded: int  # blank pages the fold pads the count to the next signature multiple
    creep: float  # outward creep extent applied at the outermost signature
    placements: tuple[Placement, ...]
    engine: ImpositionEngine = ImpositionEngine.LOCAL


class ProofPolicy(Struct, frozen=True):
    # one behavior-carrying policy value, never a flag tail the raster body re-derives; the RGB+PNG default is the
    # screen proof, a CMYK separations proof the caller-stated `ink=CMYK` + `raster=JPEG`/`TIFF` pairing.
    # ICC-managed soft-proofing and its out-of-gamut audit are graphic/color/managed#MANAGED's
    # `ManageOp.Managed(..., transform=IccTransform(proof=...))`, chained downstream over these proof bytes — never a policy field here.
    ink: ProofInk = ProofInk.RGB
    raster: ProofRaster = ProofRaster.PNG
    clip: Box | None = None  # proof only this trim/bleed sub-region — the `get_pixmap(clip=)` extent
    tint: tuple[int, int] | None = None  # `Pixmap.tint_with(black, white)` registration-tint
    gamma: float | None = None  # `Pixmap.gamma_with(gamma)` dot-gain / tone-curve finish
    negative: bool = False  # `Pixmap.invert_irect()` film-negative proof


class PdfImposeSchema(Struct, frozen=True):
    # one provider schema row: an `impose` THUNK (a bare `pdf_saddle.impose` reference at module scope would
    # reify the lazy proxy at import — the thunk body defers it to first call inside the offload worker) plus the
    # `accepts` set of optional kwargs the verified signature honors, so `_pdfimpose_kwargs` filters the
    # one candidate dict and drops a kwarg a schema rejects (onepagezine takes no `signature`/`imargin`,
    # cards no `last`, wire no `bind`) rather than raising.
    impose: Callable[[], Callable[..., None]]
    accepts: frozenset[str]


# --- [TABLES] ---------------------------------------------------------------------------
# Fold-scheme kwarg set the three creep-bearing schemas share (`hardcover` drops `creep`); each row's
# `accepts` is the exact optional-kwarg set that schema's verified `impose(...)` honors.
_FOLD_KW: Final[frozenset[str]] = frozenset({"signature", "imargin", "omargin", "mark", "bind", "creep", "group", "last"})

_PDFIMPOSE_SCHEMAS: Final[Map[Scheme, PdfImposeSchema]] = Map.of_seq([
    (Scheme.BOOKLET, PdfImposeSchema(lambda: pdf_saddle.impose, _FOLD_KW)),  # saddle, group=1 (single-leaf centerfold)
    (Scheme.SIGNATURE, PdfImposeSchema(lambda: pdf_saddle.impose, _FOLD_KW)),  # saddle, group=leaves (multi-leaf signatures)
    (Scheme.CUT_AND_STACK, PdfImposeSchema(lambda: pdf_cutstackfold.impose, _FOLD_KW)),
    (Scheme.COME_AND_GO, PdfImposeSchema(lambda: pdf_copycutfold.impose, _FOLD_KW)),
    (Scheme.HARDCOVER, PdfImposeSchema(lambda: pdf_hardcover.impose, _FOLD_KW - {"creep"})),  # sewn signatures — no creep kwarg
    (Scheme.WIRE, PdfImposeSchema(
        lambda: pdf_wire.impose, frozenset({"signature", "imargin", "omargin", "mark", "last"})
    )),  # cards-derived — no bind/creep/group
    (Scheme.CARDS, PdfImposeSchema(
        lambda: pdf_cards.impose, frozenset({"signature", "imargin", "omargin", "mark", "bind", "back"})
    )),  # front/back — no creep/group/last
    (Scheme.ZINE, PdfImposeSchema(
        lambda: pdf_onepagezine.impose, frozenset({"omargin", "mark", "bind", "last"})
    )),  # fixed 2x4 fold — no signature/imargin
])


def _sheet_count(placements: Block[Placement]) -> int:
    return placements.fold(lambda acc, p: max(acc, p.sheet), -1) + 1


# --- [OPERATIONS] -----------------------------------------------------------------------
def _saddle(slots: int) -> tuple[int, ...]:
    # Centerfold pairing (last, first, second, second-last, …) as one flat comprehension — at fold
    # position `i` the outer leaf is `slots - 1 - i`, the inner `i`, alternating by parity, no mutable index.
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
    # one plate, both sides: the back side flips, so its cells mirror across the row (work-and-turn) or down the
    # column (work-and-tumble) — one body carries both rows, the mirror axis the only difference.
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
    # sheetwise: recto/verso pages stream to SEPARATE plates by parity, each packed n-up and printed
    # independently, so no cell mirror — distinct from the mirrored `_duplexed` and the sequential `_grid`.
    slots, cells = geometry.slots, geometry.partition()
    return Block.of_seq(Placement(source=page, sheet=2 * (page // 2 // slots) + page % 2, cell=cells[page // 2 % slots]) for page in range(pages))


# perfect-bind is `_grid` over a spine-carrying geometry — `Geometry.spine` already widens the bind-edge
# origin in `partition`, so NUP and PERFECT_BIND differ by a field, never a parallel function.
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

# ONE scheme-engine capability declaration, DERIVED from the two route tables so admission and execution
# can never disagree: a pair outside its row is the `<no-route>` refusal at `ImposeOp.Impose`, before any render.
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
class ImposeOp:  # the closed request vocabulary; route admission rides the `Impose` ingress, engine raises the boundary
    tag: Literal["impose", "proof"] = tag()
    impose: tuple[bytes, Scheme, Geometry, Marks] = case()
    proof: tuple[bytes, float, int | tuple[int, ...], ProofPolicy] = case()

    @staticmethod
    def Impose(
        source: bytes, scheme: Scheme = Scheme.NUP, geometry: Geometry = Geometry(), marks: Marks = Marks()
    ) -> Result["ImposeOp", ImposeFault]:
        # Validated ingress: the scheme-engine pair resolves against the derived `_ENGINES` route table, so
        # an unroutable request (NUP under PDFIMPOSE, WIRE under LOCAL) refuses HERE, never a mid-render KeyError.
        return (
            Ok(ImposeOp(impose=(source, scheme, geometry, marks)))
            if geometry.engine in _ENGINES[scheme]
            else Error("<no-route>")
        )

    @staticmethod
    def Proof(
        source: bytes, dpi: float = 96.0, sheet: int | tuple[int, ...] = 0, policy: ProofPolicy = ProofPolicy()
    ) -> Result["ImposeOp", ImposeFault]:  # an `int` proofs one imposed sheet, a tuple a contact strip (empty = every sheet)
        # dpi refuses HERE on the EFFECTIVE rasterizer integer — the render truncates `int(dpi)`, so a finite
        # positive 0.5 would reach `get_pixmap(dpi=0)` — and a negative selector, which would silently index
        # from the sheet tail, refuses with it; only the count-dependent overrun stays the boundary's trapped `IndexError`.
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
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    op: ImposeOp
    lane: LanePolicy
    composed: Option[Composed] = Nothing

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: canonical op payload minted PRE-RUN through the bare `ContentIdentity.key`
        # (`of` returns the railed `RuntimeRail[ContentKey]`) — never a key over the imposed press-form bytes.
        return ContentIdentity.key(f"impose-{self.op.tag}", _CANON.encode(self.op))

    def folded(self) -> Self:
        # Sync evidence successor: ONE `_composed` render lands on `composed`, and contribute/layers read
        # it — never a per-projection re-render of the imposition fold.
        return structs.replace(self, composed=Some(_composed(self.op)))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        return await async_boundary(f"impose.{self.op.tag}", self._folded, catch=_FAULTS)

    async def _folded(self) -> ArtifactReceipt:
        # Async execution: the `_composed` fold crosses as one HOSTILE process kernel — MuPDF mutation and the
        # pdfimpose provider both hold the GIL — and the receipt derives from that one fold; the terminal
        # threads the PRE-RUN key.
        crossed = await self.lane.offload(Kernel.of(_composed, KernelTrait.HOSTILE), self.op)
        return self._receipt(self._key, crossed.default_with(_impose_raise))

    def _receipt(self, key: ContentKey, composed: Composed, /) -> ArtifactReceipt:
        match composed.kind:
            case ComposedKind.PDF if composed.layers:  # the OCG-bearing imposed press form rides the minted-layer count on the Egress overlays slot
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
            case ImposeOp(tag="proof"):  # a proof op imposes no sheets — its pre-flight plan is a non-failing absence
                return Nothing
            case _ as unreachable:
                assert_never(unreachable)

    def contribute(self) -> "Iterable[Receipt]":
        # rows ride the folded successor alone; the un-folded owner contributes nothing, so absence stays
        # distinct from evidence and no projection re-enters the render.
        yield from self.composed.map(lambda live: tuple(self._receipt(self._key, live).contribute())).default_value(())

    def layers(self, names: tuple[str, ...] = ()) -> tuple[Layer, ...]:
        return _placed_layers(self.composed, names)


def _impose_raise(fault: object) -> Composed:
    # terminal collapse at the press boundary: an offload fault reconstructs the raise _FAULTS folds.
    raise ValueError(str(fault))


def _composed(op: ImposeOp) -> Composed:  # the one pure render fold — offloaded by the emission, landed by `folded()`
    match op:
        case ImposeOp(tag="impose", impose=(source, scheme, Geometry(engine=ImpositionEngine.PDFIMPOSE) as geometry, marks)):
            return _pdfimposed(source, scheme, geometry, marks)
        case ImposeOp(tag="impose", impose=(source, scheme, geometry, marks)):
            with pymupdf.open(
                stream=source, filetype="pdf"
            ) as src:  # the native source handle closes deterministically once the imposed bytes are folded
                return _imposed(src, source, geometry, marks, PLANS[scheme](src.page_count, geometry))
        case ImposeOp(tag="proof", proof=(source, dpi, sheet, policy)):
            with pymupdf.open(stream=source, filetype="pdf") as src:  # the native source handle closes once the proof pixmap bytes are read
                pixmap = _contact(src, sheet, int(dpi), policy) if isinstance(sheet, tuple) else _rasterized(src[sheet], int(dpi), policy)
                # Device raster in the EXACT ProofRaster codec; the ICC soft-proof/gamut audit chains
                # outward through graphic/color/managed#MANAGED over these bytes, never re-encoded here.
                return Composed(_encoded(pixmap, policy.raster), pages=1, kind=ComposedKind.PREVIEW, extent=(pixmap.width, pixmap.height))
        case _:
            assert_never(op)


def _pdfimposed(source: bytes, scheme: Scheme, geometry: Geometry, marks: Marks, /) -> Composed:
    schema = _PDFIMPOSE_SCHEMAS[scheme]
    sink = BytesIO()
    schema.impose()((BytesIO(source),), sink, **_pdfimpose_kwargs(schema, scheme, geometry, marks))
    with pymupdf.open(stream=sink.getvalue(), filetype="pdf") as out:
        # provider-native render re-enters the ONE press-finish/serialize seam the local engine rides, so every
        # Marks field the kwargs filter cannot express — imposition_map, cut_list, bake, scrub, recompress,
        # subset, info, xmp, linearize — lands identically regardless of which engine imposed the sheets.
        marks.finished(out, geometry, out.page_count)
        return Composed(data=marks.serialize(out), pages=out.page_count)


def _pdfimpose_kwargs(schema: PdfImposeSchema, scheme: Scheme, geometry: Geometry, marks: Marks, /) -> dict[str, object]:
    # one candidate dict over `Geometry`/`Marks`, filtered to the kwargs `schema.accepts` honors so no schema
    # sees a kwarg its `impose(...)` would reject.
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
    # Live native document the engine grows in place is the platform-forced seam, `with`-bracketed so it
    # closes on every exit; `partition`'s `_admit` guard has already railed a malformed grid count before here.
    sheets = _sheet_count(placements)
    width, height = geometry.oriented
    with pymupdf.open() as out:
        for _ in range(sheets):
            out.new_page(width=width, height=height)
        groups = _mint_groups(out, placements)  # one shared OCG per unique signature-group name (dedupes the flat `sig-N` duplicates)
        boxes = placements.fold(
            lambda acc, p: acc.add(p.name, _union(acc.try_find(p.name).default_value(p.cell), p.cell)) if p.name else acc, Map.empty()
        )
        ordered = tuple(dict.fromkeys(p.name for p in placements if p.name))
        layer_rows = tuple(Layer(name, source, boxes[name]) for name in ordered)
        minted = placements.map(lambda p: _draw_one(out, src, p, groups)).choose(
            lambda drawn: Some(drawn) if drawn[0] else Nothing  # keep only the rows that bound a real OCG group
        )  # live handle; each placement OCMD-nested under its signature group
        _configure_layers(out, minted)  # one ui-config write driving reader visibility/lock over the deduped shared groups
        marks.finished(out, geometry, sheets)
        _press_marks(out, geometry, marks.press)  # draw the LOCAL crop/fold/registration/colour-bar marks at the imposed-cell/sheet boundaries
        trim, bleed_box = geometry.boxes()
        for page in out:  # pin the derived TrimBox/BleedBox so the press form declares its finished-work area
            page.set_trimbox(pymupdf.Rect(*trim))
            if geometry.bleed:
                page.set_bleedbox(pymupdf.Rect(*bleed_box))
        return Composed(data=marks.serialize(out), pages=out.page_count, layers=len(groups), layer_rows=layer_rows)


def _mint_groups(out: "Document", placements: Block[Placement], /) -> "frozendict[str, int]":
    # ONE shared OCG per unique signature name, its N placements members via OCMD; a nameless (NUP/duplex)
    # placement contributes no group, so the mint empties and the degenerate `Pdf` receipt form selects.
    names = tuple(dict.fromkeys(p.name for p in placements if p.name))
    return frozendict({name: out.add_ocg(name, on=True, intent="View", usage="Artwork") for name in names})


def _draw_one(
    out: "Document", src: "Document", p: Placement, groups: "frozendict[str, int]"
) -> tuple[int, bool, bool]:  # (shared group xref, visible, locked); 0 if unlayered
    # each name-bearing placement binds to its shared signature-group OCG through `set_ocmd`, never N flat
    # duplicate `sig-N` groups; the shared group xref rides `set_layer`.
    group = groups.get(p.name, 0) if p.name else 0
    oc = out.set_ocmd(ocgs=[group], policy=p.policy.membership.value) if group else 0
    out[p.sheet].show_pdf_page(  # index the live doc; a held Page list outliving `out` faults on draw
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
    # one `set_layer` write over the DEDUPED shared groups — the reader toggles the signature `on`/`off` and
    # honors `locked`; a group is off only when every member placement is hidden.
    if not minted.is_empty():
        groups = {xref for xref, _visible, _locked in minted}
        hidden = groups - {xref for xref, visible, _locked in minted if visible}
        # config -1 IS the default OC configuration; 0 addresses an alternate /Configs entry and raises on a fresh document
        out.set_layer(-1, on=list(groups - hidden), off=list(hidden), locked=list({xref for xref, _visible, locked in minted if locked}))


def _press_marks(out: "Document", geometry: Geometry, marks: tuple[PressMark, ...], /) -> None:
    # LOCAL crop/fold/registration/colour-bar marks drawn at the imposed-cell rects and sheet margins,
    # distinct from the `Marks.overlay` figure-overlay route to composition/compose#COMPOSE; `pdfimpose`
    # honors only `mark=['crop']`, so a provider-native scheme stays crop-only.
    if not marks:
        return
    width, height = geometry.oriented
    cells = geometry.partition()
    for page in out:  # each imposed sheet — the live native page the shape commits onto
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
                    _color_bar(shape, height, geometry.omargin or 12.0)  # each patch its OWN fill finish inside — no outer finish
                case _ as unreachable:
                    assert_never(unreachable)
        shape.commit()


def _crop_marks(shape: "Shape", cells: tuple[Box, ...], /) -> None:
    # L-shaped trim ticks just outside each imposed-cell corner — the bindery's guillotine cut guides.
    gap, tick = 3.0, 9.0
    for x0, y0, x1, y1 in cells:
        for cx, cy, sx, sy in ((x0, y0, -1.0, -1.0), (x1, y0, 1.0, -1.0), (x0, y1, -1.0, 1.0), (x1, y1, 1.0, 1.0)):
            shape.draw_line(pymupdf.Point(cx + sx * gap, cy), pymupdf.Point(cx + sx * (gap + tick), cy))
            shape.draw_line(pymupdf.Point(cx, cy + sy * gap), pymupdf.Point(cx, cy + sy * (gap + tick)))


def _fold_marks(shape: "Shape", cells: tuple[Box, ...], height: float, geometry: Geometry, /) -> None:
    # dashed fold lines down the inter-column gutter midpoints — the sheet fold axes a signature folds along.
    for col in range(1, geometry.across):
        x = (cells[col - 1][2] + cells[col][0]) / 2.0
        shape.draw_line(pymupdf.Point(x, 0.0), pymupdf.Point(x, height))


def _registration_marks(shape: "Shape", width: float, height: float, inset: float, /) -> None:
    # concentric registration targets + crosshairs at the four sheet-edge midpoints — the multi-plate press alignment.
    radius, arm = 6.0, 9.0
    for cx, cy in ((width / 2.0, inset / 2.0), (width / 2.0, height - inset / 2.0), (inset / 2.0, height / 2.0), (width - inset / 2.0, height / 2.0)):
        shape.draw_circle(pymupdf.Point(cx, cy), radius)
        shape.draw_circle(pymupdf.Point(cx, cy), radius / 2.0)
        shape.draw_line(pymupdf.Point(cx - arm, cy), pymupdf.Point(cx + arm, cy))
        shape.draw_line(pymupdf.Point(cx, cy - arm), pymupdf.Point(cx, cy + arm))


def _color_bar(shape: "Shape", height: float, inset: float, /) -> None:
    # Densitometer control-patch row — each `_BAR_PATCHES` patch its OWN `draw_rect` + `finish(fill=)`, a
    # shared finish cannot carry per-patch fills.
    size = 10.0
    for index, rgb in enumerate(_BAR_PATCHES):
        x = inset + index * size
        shape.draw_rect(pymupdf.Rect(x, height - inset - size, x + size, height - inset))
        shape.finish(color=(0.0, 0.0, 0.0), fill=rgb, width=0.2)


def _colorspace(ink: ProofInk, /) -> "Colorspace":
    # pymupdf's colorspace singleton resolves at call time, never a module-level table that would reify the
    # native import early.
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
    # Press-faithful proof rasterizer in the `ProofPolicy` ink model, then the in-place tint/gamma/negative
    # finish — gamma BEFORE the invert so the tone shift lands on the positive separations the negative mirrors.
    pixmap = page.get_pixmap(dpi=dpi, colorspace=_colorspace(policy.ink), clip=pymupdf.Rect(*policy.clip) if policy.clip is not None else None)
    if policy.tint is not None:
        pixmap.tint_with(policy.tint[0], policy.tint[1])
    if policy.gamma is not None:
        pixmap.gamma_with(policy.gamma)
    if policy.negative:
        pixmap.invert_irect()
    return pixmap


def _encoded(pixmap: "Pixmap", raster: ProofRaster, /) -> bytes:
    # native `tobytes` covers PNG (RGB/GRAY) and JPEG (the CMYK-capable separations codec); the `pil_tobytes`
    # bridge covers the WEBP/AVIF/TIFF formats MuPDF's native encoder lacks — one codec split keyed by the
    # member, and the emitted bytes ALWAYS honor the requested member.
    return pixmap.pil_tobytes(raster.value) if raster in _PIL_RASTERS else pixmap.tobytes(raster.value)


def _contact(src: "Document", sheets: tuple[int, ...], dpi: int, policy: ProofPolicy, /) -> "Pixmap":
    # Contact strip: lay each selected sheet (every sheet when empty) row-major into one montage page
    # through the same `show_pdf_page` draw, then rasterize once through the same `_rasterized` press path;
    # each cell is the LARGEST selected sheet rect, so a mixed-size gang run never mis-cells its members.
    pages = sheets or tuple(range(src.page_count))
    columns = math.isqrt(len(pages) - 1) + 1
    cell_w, cell_h = max(src[index].rect.width for index in pages), max(src[index].rect.height for index in pages)
    with pymupdf.open() as montage:  # the montage handle closes once the strip pixmap is rendered off it
        montage.new_page(width=cell_w * columns, height=cell_h * -(-len(pages) // columns))
        for slot, index in enumerate(pages):  # index the live montage page; a held Page outliving `montage` faults on draw
            montage[0].show_pdf_page(
                pymupdf.Rect(slot % columns * cell_w, slot // columns * cell_h, (slot % columns + 1) * cell_w, (slot // columns + 1) * cell_h),
                src,
                pno=index,
            )
        return _rasterized(montage[0], dpi, policy)


def _planned(source: bytes, scheme: Scheme, geometry: Geometry) -> ImposedPlan:
    with pymupdf.open(stream=source, filetype="pdf") as src:  # only the page count is read off the source handle, which then closes
        pages = src.page_count
    # RESOLVED engine keys the placement model: a PDFIMPOSE-engined request — a provider-only scheme OR a
    # dual-routed one like BOOKLET — carries empty placements AND a None sheet count, because only pdfimpose owns
    # that fold geometry; unavailable provider geometry is never published as a measured zero, while the folded
    # signature arithmetic stays engine-free over the true page count.
    local = geometry.engine is ImpositionEngine.LOCAL
    placements = PLANS[scheme](pages, geometry) if local else Block.empty()
    sheets, folded = (_sheet_count(placements) if local else None), scheme in (Scheme.BOOKLET, Scheme.SIGNATURE)
    leaves = 1 if scheme is Scheme.BOOKLET else geometry.leaves  # the booklet row forces leaves=1
    fold = 4 * max(leaves, 1)  # the effective signature size the placement folded against
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


# --- [BOUNDARIES] -----------------------------------------------------------------------
def _placed_layers(composed: Option[Composed], names: tuple[str, ...]) -> tuple[Layer, ...]:
    return composed.map(
        lambda live: tuple(
            structs.replace(layer, name=names[index] if index < len(names) else layer.name)
            for index, layer in enumerate(live.layer_rows)
        )
    ).default_value(())


def _union(left: Box, right: Box) -> Box:
    return (min(left[0], right[0]), min(left[1], right[1]), max(left[2], right[2]), max(left[3], right[3]))


# --- [EXPORTS] ----------------------------------------------------------------------------
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

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
