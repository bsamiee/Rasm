# [PY_ARTIFACTS_IMPOSITION]

`Imposition` owns press imposition — reordering, scaling, rotating, and cropping an already-emitted multi-page PDF onto larger imposed sheets in a press-ready form. It discriminates a closed-payload `ImposeOp` `tagged_union` by one total `match` — `Impose` the drawing case (`source`, `Scheme`, `Geometry`, `Marks`), `Plan` the compute-only pre-flight, `Proof` the `ProofPolicy`-driven RGB-screen / CMYK-separations / GRAY-density raster proof — never a per-scheme `Nup`/`Booklet`/`Signature` draw family or a `StrEnum` over an erased `dict`. It is the dedicated booklet/signature engine computing the saddle-stitch creep, the folded-signature ordering, and the work-and-turn duplexing that the simpler `document/egress#FINISH` `IMPOSE` in-document n-up step over a finished PDF never reaches. It computes the imposition and places the pages but assembles no document — the imposed sheets hand onward to `document` assembly.

`Scheme` is a closed `StrEnum`: a locally-placeable scheme binds one `PLANS` `place(pages, geometry)` computation fusing page order, recto/verso rotation, and per-sheet creep, while a provider-native scheme (`wire`/`hardcover`/`cards`/`zine`) is a `_PDFIMPOSE_SCHEMAS` row whose fold geometry only `pdfimpose` owns (the inverse of local-only `perfect-bind`); `scheme in PLANS` is the locally-placeable discriminant every consumer reads, and the imposed-sheet count derives from that one placement `Block`, never a second formula. `Placement`, `Geometry`, and `Marks` carry the placement axis, the binding-and-gripper-aware cell grid `partition` projects (its divisor invariants `Is`-refined so a non-positive count or negative margin rails as `api` at admission), and the press-finishing policy; each placement binds to a shared signature-group OCG through `set_ocmd` and drives its reader toggle/lock through one `set_layer` write, mirroring `composition/sheet#SHEET`, never N flat duplicate `add_ocg` groups. This owner's local `pymupdf.show_pdf_page` engine offloads the `_composed` fold to a THREAD lane under `RetryClass.OCCT` with every opened handle `with`-bracketed; `pdfimpose` is provider-contained, accepting `(BytesIO(source),)`/`BytesIO()` and returning imposed bytes plus local facts. It projects `Imposition.layers -> tuple[Layer, ...]` (one row per OCG-bound placement) to `export/layered#LAYERED`; the `Proof` output-intent ICC and the CxF3 separations arrive via `graphic/color/managed#MANAGED`, never a direct `colour-cxf` import; the LOCAL `press` printer's-mark set is distinct from the `Marks.overlay` figure-overlay route to `composition/compose#COMPOSE`. Receipts thread `core/receipt#RECEIPT`'s named `ArtifactReceipt.Egress`/`Pdf`/`Preview` mints selected by `Composed.kind`; every imposition routes through the `core/plan#PLAN` `ArtifactPipeline` as a producer node.

## [01]-[INDEX]

- [01]-[IMPOSE]: the press-imposition owner discriminating a closed `ImposeOp` `tagged_union` — `Impose` (locally-placeable n-up/booklet/signature/work-and-turn/cut-and-stack/come-and-go/perfect-bind/sheetwise plus provider-native wire/hardcover/cards/zine), `Plan` the pre-flight, `Proof` the RGB/CMYK/GRAY raster proof — folded once into a `Composed` evidence struct the `of`/`contribute`/`layers` projections share, railed `RuntimeRail[ArtifactReceipt]` over `async_boundary(catch=_FAULTS)`, dispatched to the local `pymupdf.show_pdf_page` engine or the `pdfimpose` schema wrappers.

## [02]-[IMPOSE]

- Owner: `Imposition` discriminates over the closed `ImposeOp` `expression.tagged_union`, one typed payload per case, never a `StrEnum` over a shared erased `dict`; the verb family is three cases (`Impose`/`Plan`/`Proof`), never a `Nup`/`Booklet`/`Signature` triple differing only by a literal scheme and a `leaves` knob the `Geometry` already carries. `Scheme` splits into the locally-placeable set (each a `PLANS` `place(pages, geometry)` row fusing page order, recto/verso rotation, and per-sheet creep) and the provider-native set (`WIRE`/`HARDCOVER`/`CARDS`/`ZINE`, a `_PDFIMPOSE_SCHEMAS` row only whose fold geometry only `pdfimpose` owns), each row's `accepts` frozenset filtering the one candidate kwarg dict so a schema never sees a kwarg it rejects; `scheme in PLANS` is the totality-proof discriminant, never an `if scheme == ...` cascade, and the imposed-sheet count is the derived `_sheet_count` over the one placement list, never a second `sheets` callable. `Placement` projects every field straight onto the `show_pdf_page` keyword set and feeds the one `set_layer` reader-config write; `Geometry` projects once through `partition` to the binding-aware cell grid with every field live; `Marks` carries the press-finishing policy. `pymupdf` owns the cross-document `show_pdf_page` draw, the imposed-sheet construction, the `add_ocg`+`set_ocmd`+`set_layer` OCG binding, the press-finish surface, and the press-faithful RGB/CMYK/GRAY proof raster; the per-scheme page-order, creep, verso-rotation, and tumble arithmetic is this owner's fold over that one floor, never a re-implemented byte emitter.
- Cases: dispatched by one total `match` — `Impose(source, scheme, geometry, marks)` resolves `PLANS[scheme]`, opens the source (raising the `EmptyFileError` the `_FAULTS` tuple admits on a zero-page source), computes the `Placement` list and derived count, mints one shared OCG per unique signature name, binds each member through `set_ocmd`, drives one `_configure_layers` reader write, then runs the `Marks` finish — the saddle order, head-to-head verso rotation, per-fold creep, duplex mirror, come-and-go duplicate, and sheetwise split all carried by the resolved `PLANS` row, the case unchanged. `Plan(source, scheme, geometry)` projects the `ImposedPlan` metrics without drawing, the pre-flight a press operator validates before committing. `Proof(source, dpi, sheet, policy)` discriminates the `sheet` selector on input shape — an `int` proofs one imposed sheet, a tuple a contact strip (empty = every sheet) — rasterizing in the `ProofPolicy` ink model and, when an output-intent profile is set, ICC-soft-proofing under `SOFTPROOFING|GAMUTCHECK` with the out-of-gamut count on the `Preview` `scores` band (the separations preflight the raw device conversion cannot warn on), never only the first sheet and never a re-imposition. A new scheme is one `Scheme` member plus one table row, never a new case, a per-scheme imposer sibling, a per-page draw family, or a hard-coded `rotate=0`.
- Auto: `_composed(op) -> Composed` is the ONE total `match` both `of` and `contribute` read — no second render recomputes the bytes. `Impose`'s `_admit` guard has already railed a non-positive grid count or negative extent as `api` (only a direct scalar parameter is deep-checked, never a `Struct`/`Block` field) BEFORE the placement resolution feeds the native draw; `_imposed` allocates the derived `_sheet_count` on the live `out` document, mints ONE shared OCG per unique placement `name`, and folds `_draw_one` — each binding a `name`-bearing placement to its shared group through `set_ocmd` (a reader toggles the whole signature, never N flat duplicate `sig-N` groups) and drawing `show_pdf_page` on the live native page, never a held `Page` list that outlives `out` — then keeps the rows that bound a real group, drives one `_configure_layers` write over the deduped groups, runs the `Marks.finished` finish and the `_press_marks` LOCAL crop/fold/registration/colour-bar draw (distinct from the `Marks.overlay` route to `composition/compose#COMPOSE`), and returns the deterministic `tobytes` reading the REAL `Document.page_count`. This native-handle `Block.map`/`choose` sweep is the platform-forced seam, not a `Result[Document, Never]` fold — the engine raise the boundary converts replaces a per-element `Result` thread that can never carry an `Error`. `_grid`/`_folded`/`_duplexed`/`_stacked`/`_paired`/`_split` bodies are the `PLANS` rows (NUP and PERFECT_BIND both bind `_grid`, differing by a `Geometry.spine` field; WORK_AND_TURN and WORK_AND_TUMBLE both bind `_duplexed`, differing by the `on_across` mirror-axis value — two rows over one body), each `cell` from `partition`. `Plan` projects the `ImposedPlan` without drawing (an empty local model for a provider-native scheme absent from `PLANS`); the `Proof` arm rasterizes through `_rasterized` in the `ProofPolicy` ink model and encodes native `tobytes` or the `pil_tobytes` bridge. `of` maps the `Composed` to the key; `contribute` reads the same fold and routes on `Composed.kind`, so byte count, page count, and OCG count are computed facts, never a recompute or a fabricated zero-sheet receipt.
- Receipt: each op contributes `core/receipt#RECEIPT` off the one `Composed` fold — the `Composed.kind` `Artifact` discriminant plus the `Composed.layers` count select the named flat-scalar mint once, so the owner adds NO sibling factory and NO new case. An OCG-bearing `Impose` op selects `ArtifactReceipt.Egress(key, len(data), sheets, 0, 0, layers)` carrying the imposed byte count, REAL page count, and minted-OCG count on the `overlays` slot (zero encryption/outline depth — imposition is neither a security nor a navigation close), a degenerate no-mark imposition the `Pdf` form; `Proof` selects `Preview(key, extent[0], extent[1], scores)` carrying the pixmap extent plus the soft-proof gamut band (empty when no output-intent profile is set). `contribute` reads the SAME fold, mints the key over `Composed.data`, and yields `receipt.contribute()` — never a per-kind facts `Struct` re-wrapping scalars the named mint takes positionally. `Plan` contributes no receipt — its `ImposedPlan` is a pre-flight payload read through `Imposition.planned`, never a fake `0`-page `Pdf` over plan-JSON bytes.
- Growth: a new locally-placeable scheme is one `Scheme` member plus one `PLANS` row, a new provider-native scheme one `Scheme` member plus one `_PDFIMPOSE_SCHEMAS` row carrying its `impose` function and `accepts` kwarg frozenset — never a parallel imposer class, an `if scheme == ...` branch, or a new `ImposeOp` case; a geometry knob (variable gutter, crossover bleed, creep curve, lay jog, outer trim, back-cover pin) is one `Geometry` field read in `partition`, the fold, or the `_pdfimpose_kwargs` dict; a placement axis is one `Placement` field on the `show_pdf_page` keyword set; a deeper signature is the same `_folded` fold over a larger `Geometry.leaves`; a press-finish concern is one `Marks` field; a LOCAL printer's mark is one `PressMark` member plus one `_press_marks` arm, while a figure overlay still routes through `Marks.overlay` to `composition/compose#COMPOSE` — two distinct seams; a nested reader-layer axis is one `Placement.membership`/`policy` field the `set_ocmd` fold nests; a proof axis (a new ink model, raster codec, per-sheet caption) is one `ProofInk`/`ProofRaster` member or `ProofPolicy` field, the ICC gamut warning realized by `ProofPolicy.proof` through `_softproof`. Zero new surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import math
from collections.abc import Callable, Iterable
from enum import StrEnum
from io import BytesIO
from tempfile import NamedTemporaryFile
from typing import TYPE_CHECKING, Annotated, Final, Literal, assert_never

from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, json, structs

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.export.layered import Layer

lazy import pymupdf

# the pillow ImageCms lcms2 soft-proof lane; the OUTPUT-INTENT/proof ICC profile arrives via the
# graphic/color/managed#MANAGED seam (colour-cxf CxF3 + ImageCms), NEVER a direct colour-cxf import here.
lazy from PIL import Image as PilImage, ImageChops, ImageCms
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

# the geometry invariants the `partition`/`_folded` arithmetic divides by: `Across`/`Leaves`/`Span` are the
# DIRECT scalar parameters `_admit` deep-checks (msgspec ignores `Is` at construction, so the scalar seam is
# the only enforcement site). `Quarter` bounds the 0/90/180/270 the `show_pdf_page` `rotate` keyword admits.
type Across = Annotated[int, Is[lambda n: n >= 1]]
type Leaves = Annotated[int, Is[lambda n: n >= 1]]
type Span = Annotated[float, Is[lambda v: v >= 0.0]]  # a non-negative gutter/trim/spine/bleed extent
type Quarter = Annotated[int, Is[lambda d: d in (0, 90, 180, 270)]]


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
        "hardcover"  # provider-native: sewn folded-signature book, distinct from the local `_folded` saddle block (pdfimpose.schema.hardcover)
    )
    CARDS = "cards"  # provider-native: front/back sample/swatch/keynote cards, `Geometry.back` orders the verso sources (pdfimpose.schema.cards)
    ZINE = "zine"  # provider-native: single-sheet 8-page fold-zine, fixed 2x4 fold (pdfimpose.schema.onepagezine)


class Artifact(StrEnum):  # the Composed receipt-shape discriminant — imposed vector PDF vs raster proof
    PDF = "pdf"
    PREVIEW = "preview"


class ImpositionEngine(StrEnum):
    LOCAL = "local"  # local placement facts over pymupdf show_pdf_page
    PDFIMPOSE = "pdfimpose"  # admitted pdfimpose schema wrapper, normalized back to local facts/receipts


class ProofInk(StrEnum):  # the Proof colorspace axis — pymupdf `get_pixmap(colorspace=)` selects the ink model
    RGB = "rgb"  # csRGB screen proof (default); pairs with a PNG/WEBP/AVIF raster
    CMYK = "cmyk"  # csCMYK press-separations proof a bindery reads; pairs with a CMYK-capable PSD/TIFF raster
    GRAY = "gray"  # csGRAY single-ink density proof


class ProofRaster(StrEnum):  # the Proof egress codec — native `Pixmap.tobytes` vs the `pil_tobytes` bridge
    PNG = "png"  # native tobytes RGB/GRAY raster (default)
    PSD = "psd"  # native tobytes CMYK-capable separations raster
    WEBP = "WEBP"  # pil_tobytes bridge — a format MuPDF's native tobytes lacks
    AVIF = "AVIF"  # pil_tobytes bridge
    TIFF = "TIFF"  # pil_tobytes bridge — CMYK-capable container


class Membership(StrEnum):  # the ISO 32000 OCMD visibility policy — the member value IS the pymupdf `set_ocmd(policy=)` token
    ANY_ON = "AnyOn"  # a placement is visible when ANY member OCG is on (default nested signature-layer union)
    ALL_ON = "AllOn"  # visible only when EVERY member OCG is on (signature AND side)


class PressMark(StrEnum):  # the local press-form printer's-mark set drawn at the imposed-cell/sheet boundaries
    CROP = "crop"  # L-shaped trim ticks at each imposed-cell corner — the guillotine cut guides
    FOLD = "fold"  # dashed fold lines down the inter-column gutters — the signature fold axes
    REGISTRATION = "registration"  # concentric target + crosshair at each sheet-edge midpoint — multi-plate alignment
    COLOR_BAR = "color-bar"  # a CMYK + gray control-patch row along the foot margin — the densitometer strip


# --- [CONSTANTS] ------------------------------------------------------------------------
# pymupdf raises `FileDataError`/`EmptyFileError` (both `RuntimeError`-derived) on a corrupt or
# zero-page source, a `PLANS`/`_PDFIMPOSE_SCHEMAS` miss raises `KeyError`, a malformed `Rect(*cell)`
# extent raises `ValueError`, an out-of-range `Proof` sheet index raises `IndexError`, the source
# stream raises `OSError`, pdfimpose raises `PdfImposeUserError`, and `_GUARD` raises
# `BeartypeCallHintViolation` on a non-positive grid count or off-axis verso rotation.
_FAULTS: tuple[type[BaseException], ...] = (RuntimeError, ValueError, KeyError, IndexError, OSError, BeartypeCallHintViolation, PdfImposeUserError)

# the Proof raster codecs MuPDF's native `Pixmap.tobytes` lacks — routed through the `Pixmap.pil_tobytes`
# Pillow bridge; every other `ProofRaster` (PNG, PSD) rides the native encoder.
_PIL_RASTERS: Final[frozenset[ProofRaster]] = frozenset({ProofRaster.WEBP, ProofRaster.AVIF, ProofRaster.TIFF})

# the colour control-bar patches (registration black, C, M, Y, K, quarter/half gray) as pymupdf RGB float-triple
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
    across: Across, down: Across, leaves: Leaves, gutter: Span, head_trim: Span, spine: Span, creep: Span, bleed: Span, omargin: Span, /
) -> None:
    return None  # the `@_GUARD` beartype contract IS the work — every `Is`-refined scalar is deep-checked on call


# --- [MODELS] ---------------------------------------------------------------------------
class Geometry(Struct, frozen=True):
    sheet: Dimensions = (1190.55, 841.89)
    landscape: bool = False
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
    shingle: bool = False  # creep direction: push-out (default) vs shingle-in for a thick bound block
    last: int = 0  # trailing pages pinned to the document end (back-cover), blank-filled before them (pdfimpose `last`)
    back: str = ""  # (CARDS only) the verso-source ordering token pdfimpose `cards.impose(back=)` reads

    @property
    def slots(self) -> int:
        return self.across * self.down

    @property
    def oriented(self) -> Dimensions:
        width, height = self.sheet
        return (height, width) if self.landscape else (width, height)

    def partition(self, shift: float = 0.0) -> tuple[Box, ...]:
        _admit(self.across, self.down, self.leaves, self.gutter, self.head_trim, self.spine, self.creep, self.bleed, self.omargin)
        # `shift` is the per-signature creep the fold passes and `shingle` flips its direction (push-out vs
        # shingle-in); `omargin` insets the whole grid, `gripper` reserves the claw margin on the `lay` feed edge.
        width, height = self.oriented
        grip_x, grip_y = (self.gripper, 0.0) if self.lay == "short" else (0.0, self.gripper)
        cell_w = (width - 2.0 * self.omargin - (self.across + 1) * self.gutter - self.spine - grip_x) / self.across
        cell_h = (height - 2.0 * self.omargin - (self.down + 1) * self.gutter - 2.0 * self.head_trim - grip_y) / self.down
        on_x = self.binding in ("left", "right")
        creep = -shift if self.shingle else shift
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


class Placement(Struct, frozen=True):
    source: int
    sheet: int
    cell: Box
    rotate: Quarter = 0  # verso head-to-head flip — the `show_pdf_page` `rotate` admits 0/90/180/270 only
    fit: bool = True  # keep_proportion — scale-to-fit the cell preserving aspect
    overlay: bool = True  # draw above (True) or below the imposed-sheet content already on the page
    clip: Box | None = None
    name: str = ""  # the SHARED signature-group label — one OCG per unique name, the OCMD nests each member (never N flat `sig-N` groups)
    visible: bool = True  # the group default-on state the `set_layer` ui config writes
    locked: bool = False  # the reader-side layer-lock hint `set_layer(locked=)` honors
    policy: Membership = Membership.ANY_ON  # the `set_ocmd(policy=)` visibility over the signature-group membership


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
    linearize: bool = True  # deterministic web-optimized save with garbage collection and deflate
    info: tuple[tuple[str, str], ...] = ()
    xmp: str | None = None

    def finished(self, document: "Document", geometry: Geometry, sheets: int) -> None:
        # the live-native-handle press-finish seam: a `for`/`if` mutation chain over the one document, never a fold.
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
            # the `overlay` route marker rides the press keywords so the downstream compose owner reading
            # the imposed PDF knows the registration/crop/colour-bar overlay was requested for this lay.
            marks = (*self.controls, *(("overlay",) if self.overlay else ()), geometry.binding, geometry.lay)
            document.set_metadata({**dict(self.info), "keywords": ",".join(marks)})
        if self.xmp is not None:
            document.set_xml_metadata(self.xmp)

    def serialize(self, document: "Document") -> bytes:
        # `no_new_id=True` suppresses the randomized `/ID` so the imposition is byte-identical run-to-run (one
        # stable `ContentKey`); the web-optimized form rides `garbage=4`/`use_objstms=1`, the plain form `garbage=3`.
        return (
            document.tobytes(garbage=4, deflate=True, use_objstms=1, no_new_id=True)
            if self.linearize
            else document.tobytes(garbage=3, deflate=True, no_new_id=True)
        )


class ImposedPlan(Struct, frozen=True):
    scheme: Scheme
    sheet: Dimensions  # the oriented imposed press-sheet size a press operator validates the plan against before committing the draw
    sheets: int  # imposed press sheets the placement fold yields
    pages: int  # source pages read off the live document
    leaves: int  # signature leaf count the fold depth reads
    signatures: int  # bound folding units (ceil(pages / 4·leaves) for a folded scheme, sheets otherwise)
    padded: int  # blank pages the fold pads the count to the next signature multiple
    creep: float  # outward creep extent applied at the outermost signature
    placements: tuple[Placement, ...]
    engine: ImpositionEngine = ImpositionEngine.LOCAL


class Composed(Struct, frozen=True):  # the one evidence struct of/contribute/layers read — no second render
    data: bytes
    sheets: int
    kind: Artifact = Artifact.PDF
    extent: tuple[int, int] = (0, 0)  # the Proof pixmap width/height the raster receipt rides
    layers: int = 0  # the count of shared signature-group OCGs the draw fold minted, on the Egress overlays slot
    scores: frozendict[str, float | str] = (
        frozendict()
    )  # the Proof `Preview` band — the soft-proof out-of-gamut count when an output-intent profile is set, empty otherwise


class ProofPolicy(Struct, frozen=True):
    # one behavior-carrying policy value, never a flag tail the raster body re-derives; the RGB+PNG default is
    # the screen proof, a CMYK separations proof the caller-stated `ink=CMYK` + `raster=PSD`/`TIFF` pairing.
    ink: ProofInk = ProofInk.RGB
    raster: ProofRaster = ProofRaster.PNG
    clip: Box | None = None  # proof only this trim/bleed sub-region — the `get_pixmap(clip=)` extent
    tint: tuple[int, int] | None = None  # `Pixmap.tint_with(black, white)` registration-tint
    gamma: float | None = None  # `Pixmap.gamma_with(gamma)` dot-gain / tone-curve finish
    negative: bool = False  # `Pixmap.invert_irect()` film-negative proof
    proof: bytes | None = (
        None  # the OUTPUT-INTENT ICC profile; when set the proof is ICC soft-proofed with an out-of-gamut count, the profile arriving via graphic/color/managed#MANAGED
    )


class PdfImposeSchema(Struct, frozen=True):
    # one provider schema row: its `impose` function plus the `accepts` set of optional kwargs its verified
    # signature honors, so `_pdfimpose_kwargs` filters the one candidate dict and drops a kwarg a schema
    # rejects (onepagezine takes no `signature`/`imargin`, cards no `last`, wire no `bind`) rather than raising.
    impose: Callable[..., None]
    accepts: frozenset[str]


# --- [TABLES] ---------------------------------------------------------------------------
# the fold-scheme kwarg set the three creep-bearing schemas share (`hardcover` drops `creep`); each row's
# `accepts` is the exact optional-kwarg set that schema's verified `impose(...)` honors.
_FOLD_KW: Final[frozenset[str]] = frozenset({"signature", "imargin", "omargin", "mark", "bind", "creep", "group", "last"})

_PDFIMPOSE_SCHEMAS: Final[Map[Scheme, PdfImposeSchema]] = Map.of_seq([
    (Scheme.BOOKLET, PdfImposeSchema(pdf_saddle.impose, _FOLD_KW)),  # saddle, group=1 (single-leaf centerfold)
    (Scheme.SIGNATURE, PdfImposeSchema(pdf_saddle.impose, _FOLD_KW)),  # saddle, group=leaves (multi-leaf signatures)
    (Scheme.CUT_AND_STACK, PdfImposeSchema(pdf_cutstackfold.impose, _FOLD_KW)),
    (Scheme.COME_AND_GO, PdfImposeSchema(pdf_copycutfold.impose, _FOLD_KW)),
    (Scheme.HARDCOVER, PdfImposeSchema(pdf_hardcover.impose, _FOLD_KW - {"creep"})),  # sewn signatures — no creep kwarg
    (Scheme.WIRE, PdfImposeSchema(
        pdf_wire.impose, frozenset({"signature", "imargin", "omargin", "mark", "last"})
    )),  # cards-derived — no bind/creep/group
    (Scheme.CARDS, PdfImposeSchema(
        pdf_cards.impose, frozenset({"signature", "imargin", "omargin", "mark", "bind", "back"})
    )),  # front/back — no creep/group/last
    (Scheme.ZINE, PdfImposeSchema(pdf_onepagezine.impose, frozenset({"omargin", "mark", "bind", "last"}))),  # fixed 2x4 fold — no signature/imargin
])


def _sheet_count(placements: Block[Placement]) -> int:
    return placements.fold(lambda acc, p: max(acc, p.sheet), -1) + 1


# --- [OPERATIONS] -----------------------------------------------------------------------
def _saddle(slots: int) -> tuple[int, ...]:
    # the centerfold pairing (last, first, second, second-last, …) as one flat comprehension — at fold
    # position `i` the outer leaf is `slots - 1 - i`, the inner `i`, alternating by parity, no mutable index.
    return tuple(leaf for i in range(slots // 2) for leaf in ((i, slots - 1 - i) if i % 2 == 0 else (slots - 1 - i, i)))


def _grid(pages: int, geometry: Geometry) -> Block[Placement]:
    slots, cells = geometry.slots, geometry.partition()
    return Block.of_seq(Placement(source=page, sheet=page // slots, cell=cells[page % slots]) for page in range(pages))


def _folded(pages: int, geometry: Geometry) -> Block[Placement]:
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
    # one plate, both sides: the back side flips, so its cells mirror across the row (work-and-turn) or down
    # the column (work-and-tumble) — one body carries both rows, the mirror axis the only difference.
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
    (Scheme.BOOKLET, lambda pages, geo: _folded(pages, structs.replace(geo, leaves=1))),
    (Scheme.SIGNATURE, _folded),
    (Scheme.WORK_AND_TURN, _duplexed(on_across=True)),
    (Scheme.WORK_AND_TUMBLE, _duplexed(on_across=False)),
    (Scheme.CUT_AND_STACK, _stacked),
    (Scheme.COME_AND_GO, _paired),
    (Scheme.PERFECT_BIND, _grid),
    (Scheme.SHEETWISE, _split),
])


@tagged_union(frozen=True)
class ImposeOp:  # the closed request vocabulary; the fault rail is BoundaryFault at the boundary
    tag: Literal["impose", "plan", "proof"] = tag()
    impose: tuple[bytes, Scheme, Geometry, Marks] = case()
    plan: tuple[bytes, Scheme, Geometry] = case()
    proof: tuple[bytes, float, int | tuple[int, ...], ProofPolicy] = case()

    @staticmethod
    def Impose(source: bytes, scheme: Scheme = Scheme.NUP, geometry: Geometry = Geometry(), marks: Marks = Marks()) -> "ImposeOp":
        return ImposeOp(impose=(source, scheme, geometry, marks))

    @staticmethod
    def Plan(source: bytes, scheme: Scheme, geometry: Geometry = Geometry()) -> "ImposeOp":
        return ImposeOp(plan=(source, scheme, geometry))

    @staticmethod
    def Proof(
        source: bytes, dpi: float = 96.0, sheet: int | tuple[int, ...] = 0, policy: ProofPolicy = ProofPolicy()
    ) -> "ImposeOp":  # an `int` proofs one imposed sheet, a tuple a contact strip (empty = every sheet)
        return ImposeOp(proof=(source, dpi, sheet, policy))


# --- [COMPOSITION] ----------------------------------------------------------------------
class Imposition(Struct, frozen=True):
    op: ImposeOp

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: canonical op payload minted PRE-RUN — never a key over the imposed press-form bytes.
        return ContentIdentity.of(f"impose-{self.op.tag}", self.op, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        return await async_boundary(f"impose.{self.op.tag}", self._folded, catch=_FAULTS)

    async def _folded(self) -> ArtifactReceipt:
        # the GIL-releasing `_composed` MuPDF fold crosses the THREAD lane; the receipt threads the PRE-RUN key.
        crossed = await LanePolicy.offload(_composed, self.op, modality=Modality.THREAD, retry=RetryClass.OCCT)
        return self._receipt(self._key, crossed.default_with(_impose_raise))

    def _receipt(self, key: ContentKey, composed: "Composed", /) -> ArtifactReceipt:
        match composed.kind:
            case Artifact.PDF if composed.layers:  # the OCG-bearing imposed press form rides the minted-layer count on the Egress overlays slot
                return ArtifactReceipt.Egress(key, len(composed.data), composed.sheets, 0, 0, composed.layers)
            case Artifact.PDF:
                return ArtifactReceipt.Pdf(key, len(composed.data), composed.sheets)
            case Artifact.PREVIEW:
                return ArtifactReceipt.Preview(key, composed.extent[0], composed.extent[1], composed.scores)
            case _:
                assert_never(composed.kind)

    def planned(self) -> Option[ImposedPlan]:
        match self.op:
            # both OR-alternatives must bind the SAME names — `marks` is irrelevant to the plan, so it is
            # the `_` wildcard (a `_marks` capture in only one alternative is a `SyntaxError`).
            case ImposeOp(tag="impose", impose=(source, scheme, geometry, _)) | ImposeOp(tag="plan", plan=(source, scheme, geometry)):
                return Some(_planned(source, scheme, geometry))
            case ImposeOp(tag="proof"):  # a proof op imposes no sheets — its pre-flight plan is a non-failing absence
                return Nothing
            case _ as unreachable:
                assert_never(unreachable)

    def contribute(self) -> "Iterable[Receipt]":
        if self.op.tag in ("impose", "proof"):
            # contribute re-enters the deterministic fold off the loop; the slot is the PRE-RUN input key.
            yield from self._receipt(self._key, _composed(self.op)).contribute()

    def layers(self, names: tuple[str, ...] = ()) -> tuple[Layer, ...]:
        return _placed_layers(self.op, names)


def _impose_raise(fault: object) -> "Composed":
    # terminal collapse at the press boundary: an offload fault reconstructs the raise _FAULTS folds.
    raise ValueError(str(fault))


def _composed(op: ImposeOp) -> Composed:  # the one pure render fold both `of` and `contribute` read
    match op:
        case ImposeOp(tag="impose", impose=(source, scheme, geometry, marks)):
            # a provider-native scheme (no `PLANS` row: wire/hardcover/cards/zine) OR an explicit PDFIMPOSE
            # engine routes to the provider fold; every locally-placeable scheme draws through show_pdf_page.
            if geometry.engine is ImpositionEngine.PDFIMPOSE or scheme not in PLANS:
                return _pdfimposed(source, scheme, geometry, marks)
            with pymupdf.open(
                stream=source, filetype="pdf"
            ) as src:  # the native source handle closes deterministically once the imposed bytes are folded
                return _imposed(src, geometry, marks, PLANS[scheme](src.page_count, geometry))
        case ImposeOp(tag="plan", plan=(source, scheme, geometry)):
            plan = _planned(source, scheme, geometry)
            return Composed(data=json.encode(plan), sheets=plan.sheets)
        case ImposeOp(tag="proof", proof=(source, dpi, sheet, policy)):
            with pymupdf.open(stream=source, filetype="pdf") as src:  # the native source handle closes once the proof pixmap bytes are read
                pixmap = _contact(src, sheet, int(dpi), policy) if isinstance(sheet, tuple) else _rasterized(src[sheet], int(dpi), policy)
                blob, gamut = _proofed(pixmap, policy)  # ICC soft-proof when policy.proof is set (out-of-gamut count), else the raw device raster
                scores = frozendict({"gamut": float(gamut)}) if policy.proof is not None else frozendict()
                return Composed(blob, sheets=1, kind=Artifact.PREVIEW, extent=(pixmap.width, pixmap.height), scores=scores)
        case _:
            assert_never(op)


def _pdfimposed(source: bytes, scheme: Scheme, geometry: Geometry, marks: Marks, /) -> Composed:
    schema = _PDFIMPOSE_SCHEMAS[scheme]
    sink = BytesIO()
    schema.impose((BytesIO(source),), sink, **_pdfimpose_kwargs(schema, scheme, geometry, marks))
    data = sink.getvalue()
    return Composed(data=data, sheets=_page_count(data))


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


def _imposed(src: "Document", geometry: Geometry, marks: Marks, placements: Block[Placement], /) -> Composed:
    # the live native document the engine grows in place is the platform-forced seam, `with`-bracketed so it
    # closes on every exit; `partition`'s `_admit` guard has already railed a malformed grid count before here.
    sheets = _sheet_count(placements)
    width, height = geometry.oriented
    with pymupdf.open() as out:
        for _ in range(sheets):
            out.new_page(width=width, height=height)
        groups = _mint_groups(out, placements)  # one shared OCG per unique signature-group name (dedupes the flat `sig-N` duplicates)
        minted = placements.map(lambda p: _draw_one(out, src, p, groups)).choose(
            _oc_state
        )  # live handle; each placement OCMD-nested under its signature group
        _configure_layers(out, minted)  # one ui-config write driving reader visibility/lock over the deduped shared groups
        marks.finished(out, geometry, sheets)
        _press_marks(out, geometry, marks.press)  # draw the LOCAL crop/fold/registration/colour-bar marks at the imposed-cell/sheet boundaries
        return Composed(data=marks.serialize(out), sheets=out.page_count, layers=len(groups))


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
    oc = out.set_ocmd(ocgs=[group], policy=p.policy.value) if group else 0
    out[p.sheet].show_pdf_page(  # index the live doc; a held Page list outliving `out` faults on draw
        pymupdf.Rect(*p.cell),
        src,
        pno=p.source,
        keep_proportion=p.fit,
        overlay=p.overlay,
        rotate=p.rotate,
        clip=pymupdf.Rect(*p.clip) if p.clip is not None else None,
        oc=oc,
    )
    return group, p.visible, p.locked


def _oc_state(drawn: tuple[int, bool, bool], /) -> Option[tuple[int, bool, bool]]:  # keep only the rows that bound a real OCG group
    return Some(drawn) if drawn[0] else Nothing


def _configure_layers(out: "Document", minted: Block[tuple[int, bool, bool]], /) -> None:
    # one `set_layer` write over the DEDUPED shared groups — the reader toggles the signature `on`/`off` and
    # honors `locked`; a group is off only when every member placement is hidden.
    if not minted.is_empty():
        groups = {xref for xref, _visible, _locked in minted}
        hidden = groups - {xref for xref, visible, _locked in minted if visible}
        out.set_layer(0, on=list(groups - hidden), off=list(hidden), locked=list({xref for xref, _visible, locked in minted if locked}))


def _press_marks(out: "Document", geometry: Geometry, marks: tuple[PressMark, ...], /) -> None:
    # the LOCAL crop/fold/registration/colour-bar marks drawn at the imposed-cell rects and sheet margins,
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
    # the densitometer control-patch row — each `_BAR_PATCHES` patch its OWN `draw_rect` + `finish(fill=)`, a
    # shared finish cannot carry per-patch fills.
    size = 10.0
    for index, rgb in enumerate(_BAR_PATCHES):
        x = inset + index * size
        shape.draw_rect(pymupdf.Rect(x, height - inset - size, x + size, height - inset))
        shape.finish(color=(0.0, 0.0, 0.0), fill=rgb, width=0.2)


def _colorspace(ink: ProofInk, /) -> "Colorspace":
    # the pymupdf colorspace singleton resolves at call time, never a module-level table that would reify the
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
    # the press-faithful proof rasterizer in the `ProofPolicy` ink model, then the in-place tint/gamma/negative
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
    # native `tobytes` covers PNG (RGB/GRAY) and PSD (CMYK-capable separations); the `pil_tobytes` bridge
    # covers the WEBP/AVIF/TIFF formats MuPDF's native encoder lacks — one codec split keyed by the member.
    return pixmap.pil_tobytes(raster.value) if raster in _PIL_RASTERS else pixmap.tobytes(raster.value)


def _proofed(pixmap: "Pixmap", policy: ProofPolicy, /) -> tuple[bytes, int]:
    # no output-intent profile -> the raw device raster (`_encoded`), zero out-of-gamut; a profile -> the ICC soft-proof.
    return _softproof(pixmap, policy.proof, policy.raster) if policy.proof is not None else (_encoded(pixmap, policy.raster), 0)


def _softproof(pixmap: "Pixmap", profile: bytes, raster: ProofRaster, /) -> tuple[bytes, int]:
    # the ICC-managed separations proof the raw device conversion cannot warn on: apply the proof transform
    # (`profile` the press OUTPUT-INTENT) once under `SOFTPROOFING` and once under `SOFTPROOFING|GAMUTCHECK`,
    # counting the differing out-of-gamut pixels. The profile arrives via graphic/color/managed#MANAGED.
    origin = PilImage.open(BytesIO(pixmap.pil_tobytes("png"))).convert("RGB")
    with NamedTemporaryFile(suffix=".icc", delete_on_close=False) as handle:  # the press profile must outlive the lazy transform build
        handle.write(profile)
        handle.close()
        reference = ImageCms.createProfile("sRGB")
        keys = {"renderingIntent": ImageCms.Intent.RELATIVE_COLORIMETRIC, "proofRenderingIntent": ImageCms.Intent.ABSOLUTE_COLORIMETRIC}
        proofed = ImageCms.applyTransform(
            origin, ImageCms.buildProofTransform(reference, reference, handle.name, "RGB", "RGB", flags=ImageCms.Flags.SOFTPROOFING, **keys)
        )
        alarmed = ImageCms.applyTransform(
            origin,
            ImageCms.buildProofTransform(
                reference, reference, handle.name, "RGB", "RGB", flags=ImageCms.Flags.SOFTPROOFING | ImageCms.Flags.GAMUTCHECK, **keys
            ),
        )
    gamut = ImageChops.difference(proofed, alarmed).convert("L").point(lambda level: 255 if level else 0).histogram()[255]
    sink = BytesIO()
    alarmed.save(
        sink, format=raster.value if raster in _PIL_RASTERS else "PNG"
    )  # the soft-proof simulation is RGB, so PSD/native ProofRaster falls to PNG
    return sink.getvalue(), gamut


def _contact(src: "Document", sheets: tuple[int, ...], dpi: int, policy: ProofPolicy, /) -> "Pixmap":
    # the contact strip: lay each selected sheet (every sheet when empty) row-major into one montage page
    # through the same `show_pdf_page` draw, then rasterize once through the same `_rasterized` press path.
    pages = sheets or tuple(range(src.page_count))
    columns, cell = math.isqrt(len(pages) - 1) + 1, src[pages[0]].rect
    with pymupdf.open() as montage:  # the montage handle closes once the strip pixmap is rendered off it
        montage.new_page(width=cell.width * columns, height=cell.height * -(-len(pages) // columns))
        for slot, index in enumerate(pages):  # index the live montage page; a held Page outliving `montage` faults on draw
            montage[0].show_pdf_page(
                pymupdf.Rect(
                    slot % columns * cell.width, slot // columns * cell.height, (slot % columns + 1) * cell.width, (slot // columns + 1) * cell.height
                ),
                src,
                pno=index,
            )
        return _rasterized(montage[0], dpi, policy)


def _planned(source: bytes, scheme: Scheme, geometry: Geometry) -> ImposedPlan:
    with pymupdf.open(stream=source, filetype="pdf") as src:  # only the page count is read off the source handle, which then closes
        pages = src.page_count
    # a provider-native scheme has no local placement model, so its pre-flight carries scheme/pages/sheet-size/
    # engine with empty placements rather than a fabricated stream that would diverge from the real imposition.
    local = scheme in PLANS
    placements = PLANS[scheme](pages, geometry) if local else Block.empty()
    sheets, folded = _sheet_count(placements), scheme in (Scheme.BOOKLET, Scheme.SIGNATURE)
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
        engine=geometry.engine if local else ImpositionEngine.PDFIMPOSE,
    )


# --- [BOUNDARIES] -----------------------------------------------------------------------
def _placed_layers(op: ImposeOp, names: tuple[str, ...]) -> tuple[Layer, ...]:
    # one named row per OCG-bound signature group for `export/layered#LAYERED`, each carrying the union cell
    # box over its placements; only the LOCAL show_pdf_page arm mints OCGs, so a provider imposition projects nothing.
    match op:
        case ImposeOp(tag="impose", impose=(source, scheme, geometry, _)) if scheme in PLANS and geometry.engine is not ImpositionEngine.PDFIMPOSE:
            placements = PLANS[scheme](_page_count(source), geometry)
            boxes = placements.fold(
                lambda acc, p: acc.add(p.name, _union(acc.try_find(p.name).default_value(p.cell), p.cell)) if p.name else acc, Map.empty()
            )
            # signature order is the placement order, never `Map.items()` AVL key-sort (which orders `sig-10`
            # before `sig-2`); `dict.fromkeys` dedupes the ordered name stream so the `names` index aligns.
            ordered = tuple(dict.fromkeys(p.name for p in placements if p.name))
            return tuple(Layer(names[index] if index < len(names) else name, source, boxes[name]) for index, name in enumerate(ordered))
        case _:
            return ()


def _union(left: Box, right: Box) -> Box:
    return (min(left[0], right[0]), min(left[1], right[1]), max(left[2], right[2]), max(left[3], right[3]))


def _page_count(source: bytes) -> int:
    with pymupdf.open(stream=source, filetype="pdf") as doc:  # the count is read off the handle, which then closes
        return doc.page_count
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
