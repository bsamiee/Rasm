# [PY_ARTIFACTS_SHEET]

`Sheet` owns the single-sheet pipeline — placing already-emitted figures into titled, framed, field-bound architectural drawing sheets; `SheetSet` is the peer multi-sheet owner assembling N `SheetIssue` sheets into a numbered, audited, register-ready set. `Sheet` discriminates a closed `SheetOp` `expression.tagged_union` by one total `match` folded once into a `Composed` evidence struct — one typed payload per case, never a `StrEnum` over an erased `dict`; the async emission offloads that fold exactly once and mints its receipt from it, and the sync `folded()` successor lands the same fold on the frozen `composed` field so `contribute`/`layers` read evidence rather than re-rendering. It authors the frame over three PDF engines under one `PdfProfile` conformance value, places figures, fills and stamps the title block, but re-renders no figure and re-authors no register. `SheetSet` numbers each sheet, folds the ISO 7200 conformance verdict, and PROJECTS the set outward rather than re-authoring — no OSS sheet-set library exists, so the sheet algebra is this owner's composition over the admitted engines.

Frame authors are `reportlab` (the coordinate author of the border, the `_ZONES` ISO 5457 zone grid, the `TitleBlock.cells` grid, and the `Scale.bar` ISO 5455 graphic ruler), `typst`, and `weasyprint` (which owns its full PDF/A + PDF/UA close end to end), each driven by one `PdfProfile` — the compositional conformance owner whose `PdfA` archival level and `PdfUa` accessibility level derive every engine token (`tokens` the typst `pdf_standards` list, `variant` the weasyprint `pdf_variant`, `version` the reportlab `pdfVersion` pin, `pdfaid` the XMP part/conformance pair) from one declaration — never a per-engine `archival: bool`, a combination-named enum member, or a parallel token table; a level an engine cannot close (`weasyprint` closes no `a-4f`/`a-4e`) is an explicit boundary refusal, and `reportlab`'s PDF/A output-intent/structure-tree close routes to `exchange/conformance#CONFORMANCE`. `pymupdf.show_pdf_page` vector-copies each figure into its `FigurePlacement.target` window — a `Viewport`-fitted scaled model view, the bare cell, or a `FigurePlacement.arranged` constraint-solved cell off `composition/compose#COMPOSE`'s `Rule`/`arranged` Cassowary solve — bound to a minted `add_ocg` group or a `set_ocmd` membership dictionary, driven to its reader visibility/lock through one `set_layer` write, mirrored by `composition/imposition#IMPOSE` which imports this page's `Composed`/`ComposedKind`/`Orientation`/`PlacementPolicy` owners. `_FAULTS` discriminates each engine raise into its own case, with every opened handle `with`-bracketed; the `_composed` fold offloads through `self.lane.offload` as one `HOSTILE`-trait `Kernel` onto the warm process pool — every arm's engine holds the GIL (the `reportlab` pure-Python author, the `typst`/`weasyprint` native renders, the `pymupdf` native mutation), so the thread band a `RELEASING` row implies buys no parallelism, and the subinterpreter arm stays refused because the worker returns the `msgspec`-backed `Composed` the isolated interpreter cannot load the C-extension for — the PURE ban justifies nothing about the thread band, and a future GIL-releasing arm splits per arm as a trait row, never by re-collapsing the fold onto one band. Figures arrive already-emitted from `composition/compose#COMPOSE` and `visualization/chart/export#EXPORT`, the numbered set from `drawing/regime#REGIME` `SheetId`; it hands the titled sheet to `composition/imposition#IMPOSE`, projects `Sheet.layers` to `export/layered#LAYERED`, and hands the numbered set to `delivery/register#REGISTER` (register-ready tuples) and `visualization/table#TABLE` (a `TablePlan`). Single-sheet receipts thread `core/receipt#RECEIPT`'s named `ArtifactReceipt.Pdf`/`Egress`/`Preview` mints selected by `Composed.kind`; `SheetSet` is a synchronous projection owner exactly as `drawing/regime#REGIME`, minting no receipt, its evidence riding the register and table owners it projects to.

## [01]-[INDEX]

- [01]-[SHEET]: the single-sheet title-block/frame/field-binding owner over the closed `SheetOp` `tagged_union` (`Frame`/`Place`/`Fill`/`Stamp`/`Preview`) folded once into a `Composed` evidence struct the `folded()` successor threads, railed `RuntimeRail[ArtifactReceipt]` over `async_boundary(catch=_FAULTS)`, dispatched to the `reportlab`/`typst`/`weasyprint` frame authors under one `PdfProfile` and to `pymupdf` for figure placement, OCG binding, title-block fill, preview, and stamp.
- [02]-[SHEET_SET]: the multi-sheet assembly owner over `tuple[SheetIssue, ...]` — numbering each `drawing/regime#REGIME` `SheetId` across the set, auditing ISO 7200 conformance, and projecting the numbered set to `delivery/register#REGISTER` and `visualization/table#TABLE`; a pure synchronous projection owner minting no receipt, exactly as `drawing/regime#REGIME`.

## [02]-[SHEET]

- Owner: `Sheet` discriminates over the closed `SheetOp` `expression.tagged_union`, every arm folded ONCE into the `Composed` struct — the async emission derives its receipt from the one offloaded fold, and the sync `folded()` successor lands the fold on the `composed` field the `contribute`/`layers` projections read, so no projection re-invokes a frame author or the native PDF serializer. `Engine` binds each frame author through `_FRAME`; `PdfProfile` is the ONE conformance declaration — `archival: PdfA | None` spans the full ISO 19005 level matrix (`a-1a`..`a-4e`, the member value the typst token) and `accessible: PdfUa | None` the ISO 14289 axis, every engine projection (`tokens`/`variant`/`version`/`pdfaid`/`tags`/`pinned`) DERIVING from those two fields so a new archival level is one `PdfA` member and zero table edits; the four named profiles `DRAFT`/`ARCHIVAL`/`ACCESSIBLE`/`PRESERVED` are module-level values, never combination-named enum members. WeasyPrint closes one `pdf_variant`, B-level PDF/A or bare `a-4`, so an unsupported level or combined archival-accessible profile refuses at that author. `SheetSize`'s `_SIZES` correspondence spans ARCH/ANSI/JIS-B beyond the `reportlab`/`pymupdf` built-in tables, and `Orientation.of` projects it without a boolean swap knob. `_ZONES` is the EXACT ISO 5457 Table 2 field-count table — the ISO-A cardinalities load-bearing, non-ISO-A/A5 derived through `ZoneSpec.of` over the 50-mm field, I and O excluded from the letters and A4 fields on the top/right edges only — so a location is cited by its exact standard zone. `TitleBlock`'s `cells(dims)` is the ONE field-rect correspondence the frame author draws labels at AND the `Fill` arm binds values INTO, its `audit()` folding the `_ISO7200` table into a `TitleBlockAudit` and its `revised()` projecting the history as a `visualization/table#TABLE` `TablePlan` rather than hand-drawn rows; `FieldRow`'s `group` zone-key sorts the rows so identity and approval zones stay contiguous. `Viewport` binds an ISO 5455 `ScaleRatio` to a placement window as a `svgelements.Matrix` affine, and `FigurePlacement.target` discriminates that model view from a `(cell, clip)` payload by input shape. `PlacementPolicy` carries every `show_pdf_page` and OCG behavior axis as one value. `FigurePlacement.arranged` composes `composition/compose#COMPOSE`'s Cassowary `arranged` solve over the figures' PDF page extents, so a rule-declared sheet layout replaces hand-computed cells. `reportlab` draws the frame in `CMYKColor` process-black — press-faithful separations, not the sRGB default a drawing sheet must not ship; `typst` and `weasyprint` own their native PDF/A close, `pymupdf` the placement/OCG/fill/preview/stamp surface; no sheet-set library is admitted, so the sheet algebra is this owner's composition over those engines, never a re-implemented byte emitter.
- Cases: dispatched by one total `match` lowering to the one `Composed` fold — never a per-discipline sheet-builder sibling, a per-engine `_emit` method, or a per-figure draw call. `Frame(size, engine, title, profile, output_intent, orientation)` authors the framed titled sheet, the `PdfProfile` routing each engine's conformance tokens and the `OutputIntent` ICC into the engine that owns its PDF/A close. `Place(sheet, placements)` vector-copies each figure into its `placement.resolved()` window, minting an OCG or `set_ocmd` membership dictionary under one `PlacementPolicy` and driving one `set_layer` config. `Fill(sheet, title)` binds field VALUES into the EXACT rects their labels were drawn at, reading the emitted page's real `rect` and the SAME `TitleBlock.cells` correspondence through `insert_htmlbox` — a loose `fields=rows` reconstruction that offsets every value past the fixed head fields is the deleted illusory-alignment form. `Stamp(sheet, title, profile, output_intent, attachments)` runs archival hygiene (a metadata-preserving `subset_fonts` + `scrub`, plus the explicit per-page `set_trimbox` pin press tools read) THEN binds `set_metadata`/`set_toc`/`embfile_add`/`set_xml_metadata` with `tobytes(no_new_id=True)` pinning the stable `/ID`. `Preview(sheet, dpi)` rasterizes to PNG keyed by the same `ContentKey`.
- Auto: `_composed(op) -> Composed` is the ONE `_GUARD`-contracted total `match`, executed once per path — offloaded by the async emission, landed on the successor by the sync `folded()` — never re-entered per projection. `Frame` calls `_FRAME[engine]` and routes the profile; the `Place` arm mints one shared OCG per unique `membership` group, folds `_draw_one` over the placements (each minting an `add_ocg` leaf and, for a `membership`-bearing placement, a `set_ocmd` dictionary), keeps the rows that minted a real xref, and drives one `_configure_layers` write over the leaves AND the shared groups before `tobytes`, so `Composed.layers` carries `len(minted) + len(groups)`. `Frame` and `Place` carry their editable `Layer` rows in `Composed.layer_rows` beside those bytes; `Sheet.layers` only renames that evidence. `Fill`/`Stamp`/`Preview` bind their `pymupdf` surface. Each opened `pymupdf` document is `with`-bracketed so it closes on each exit — the native-handle mutation the platform-forced seam. Each arm returns `Composed` reading the REAL `Document.page_count`, so the body stays one `match`-shaped path — never an inline `try`/`except` ladder, a memo, or a second `match` re-rendering for the receipt.
- Receipt: each single-sheet op contributes `core/receipt#RECEIPT` off the one `Composed` fold — the `Frame`/`Fill`/`Stamp` arms `ArtifactReceipt.Pdf`, the OCG-bearing `Place` arm `Egress(key, bytes_, pages, 0, 0, layers)` carrying the minted-layer count (leaves plus shared groups) on the `overlays` slot, the `Preview` arm `Preview(key, width, height, bytes_, scores)`; the `Composed.kind` plus `Composed.layers` select the mint once, so the owner adds NO sibling type and NO new case. `SheetSet` contributes no receipt — its numbered set is register evidence the `delivery/register#REGISTER` `Register` case mints and drawing-list evidence the `visualization/table#TABLE` `Table` case mints, a parallel sheet-set rail the deleted form. `contribute` reads the successor's `composed` evidence and mints the key over the SAME fold the emission read, never a per-kind facts `Struct`, a phantom `ArtifactReceipt.of(key, facts)`, or a second render; the un-folded owner contributes nothing, so absence stays distinct from evidence.
- Growth: a new sheet format is one `SheetSize` member plus one `_SIZES` row (its zones one `_ZONES` row or the `ZoneSpec.of` derivation); an orientation is one `Orientation` member; a title-block field is one `FieldRow`, a revision one `Revision`, an ISO 7200 mandatory field one `Iso7200Field` member plus one `_ISO7200` predicate row; a frame-authoring engine is one `Engine` member plus one `_FRAME` row; a placement target is a `FigurePlacement.target` input shape and a layer behavior axis one `PlacementPolicy` field; a constraint-solved layout is one `Rule` handed to `FigurePlacement.arranged`; a graphic cell is one `TitleBlock` field; a border-grid change is the `_ZONES` table or `_zones` projection; a receipt shape is one `ComposedKind` arm breaking the `_receipt` `match`; an engine raise is one `_FAULTS` type; an archival level is one `PdfA` member (its typst token the member value, its XMP pdfaid derived from the spelling), an accessibility level one `PdfUa` member; a colour intent is the `OutputIntent` ICC threaded into the engine that owns its close; a set-level projection is one `SheetSet` method. Zero new surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import hashlib
import string
from collections import Counter
from collections.abc import Callable, Iterable
from enum import StrEnum
from html import escape
from io import BytesIO
from typing import TYPE_CHECKING, Annotated, Final, Literal, Self, assert_never
from xml.etree.ElementTree import Element, QName, SubElement, register_namespace, tostring

from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from builtins import frozendict
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, msgpack, structs

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.faults import RuntimeRail, async_boundary

from rasm.artifacts.composition.compose import Rule, arranged
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.drawing.regime import ScaleRatio, SheetId
from rasm.artifacts.export.layered import Layer
from rasm.artifacts.visualization.table import StubLoc, TableFormat, TableOp, TablePlan, Theme

lazy import polars
lazy import pymupdf
lazy import typst
lazy from reportlab.lib.colors import CMYKColor
lazy from reportlab.lib.utils import ImageReader
lazy from reportlab.pdfgen.canvas import Canvas
lazy from svgelements import Matrix, Point
lazy from weasyprint import HTML, Attachment
lazy from weasyprint.text.fonts import FontConfiguration

if TYPE_CHECKING:
    import polars as pl
    from pymupdf import Document

    from rasm.runtime.receipts import Receipt


# --- [TYPES] ----------------------------------------------------------------------------
type Box = tuple[float, float, float, float]
type Dimensions = tuple[float, float]
type Quarter = Annotated[int, Is[lambda d: d in (0, 90, 180, 270)]]  # the show_pdf_page `rotate` domain; imposition composes this owner
type FieldCell = tuple["FieldRow", Box]  # one title-block field bound to its derived top-left-origin rect
type OutputIntent = tuple[str, bytes] | None  # (icc-name, sRGB/CMYK ICC profile bytes) the PDF/A output-intent embeds
type Author = Callable[[Dimensions, "SheetSize", "TitleBlock", "PdfProfile", OutputIntent], bytes]


class Engine(StrEnum):
    REPORTLAB = "reportlab"
    TYPST = "typst"
    WEASYPRINT = "weasyprint"


class Orientation(StrEnum):
    PORTRAIT = "portrait"
    LANDSCAPE = "landscape"

    def of(self, dims: Dimensions, /) -> Dimensions:
        return dims if self is Orientation.PORTRAIT else (dims[1], dims[0])


class PdfA(StrEnum):
    # ISO 19005 archival level — part + conformance in one member whose value IS the typst `pdf_standards`
    # token; the XMP pdfaid pair and the reportlab version pin DERIVE from the spelling, never a second table.
    A1A = "a-1a"
    A1B = "a-1b"
    A2A = "a-2a"
    A2B = "a-2b"
    A2U = "a-2u"
    A3A = "a-3a"
    A3B = "a-3b"
    A3U = "a-3u"
    A4 = "a-4"
    A4E = "a-4e"
    A4F = "a-4f"


class PdfUa(StrEnum):  # ISO 14289 accessibility level; the member value IS the typst token — UA-2 lands as one member
    UA1 = "ua-1"


class SheetSize(StrEnum):  # the architectural / engineering sheet formats — a new size is one member plus one `_SIZES` row
    A0 = "A0"
    A1 = "A1"
    A2 = "A2"
    A3 = "A3"
    A4 = "A4"
    A5 = "A5"
    ANSI_A = "ANSI-A"
    ANSI_B = "ANSI-B"
    ANSI_C = "ANSI-C"
    ANSI_D = "ANSI-D"
    ANSI_E = "ANSI-E"
    ARCH_A = "ARCH-A"
    ARCH_B = "ARCH-B"
    ARCH_C = "ARCH-C"
    ARCH_D = "ARCH-D"
    ARCH_E = "ARCH-E"
    ARCH_E1 = "ARCH-E1"
    JIS_B0 = "JIS-B0"
    JIS_B1 = "JIS-B1"


class ComposedKind(StrEnum):  # the Composed receipt-shape discriminant — vector PDF vs raster preview; imposition imports it
    PDF = "pdf"
    PREVIEW = "preview"


class Membership(StrEnum):  # the ISO 32000 OCMD visibility policy — the member value IS the pymupdf `set_ocmd(policy=)` token
    ANY_ON = "AnyOn"  # visible when ANY member OCG is on (default nested-layer union)
    ALL_ON = "AllOn"  # visible only when EVERY member OCG is on (discipline AND detail)
    ANY_OFF = "AnyOff"
    ALL_OFF = "AllOff"


class Iso7200Field(
    StrEnum
):  # ISO 7200:2004 MANDATORY title-block data fields (Table 1 + §5.3); revision index (§5.1.4) is OPTIONAL and absent by design
    LEGAL_OWNER = "legal_owner"  # §5.1.2 M
    IDENTIFICATION_NUMBER = "identification_number"  # §5.1.3 M
    DATE_OF_ISSUE = "date_of_issue"  # §5.1.5 M
    SHEET_NUMBER = "sheet_number"  # §5.1.6 M
    TITLE = "title"  # §5.2 M
    APPROVAL_PERSON = "approval_person"  # §5.3 M
    CREATOR = "creator"  # §5.3.5 M
    DOCUMENT_TYPE = "document_type"  # §5.3.6 M


# --- [CONSTANTS] ------------------------------------------------------------------------
# Engine raise tuple: `RuntimeError` (pymupdf `FileDataError`/`EmptyFileError`, typst `TypstError`),
# `ValueError` (a reportlab/weasyprint author raise, an unsupported-profile refusal, an `ElementTree`
# malformed name, an unsatisfiable `arranged` solve), `KeyError` (`_SIZES`/`_FRAME` miss), `OSError` (an
# engine font/resource load), `BeartypeCallHintViolation` (a malformed scalar the `_GUARD` fold rejects).
_FAULTS: tuple[type[BaseException], ...] = (RuntimeError, ValueError, KeyError, OSError, BeartypeCallHintViolation)

_CANON: Final = msgpack.Encoder(order="deterministic")  # the stable preimage encoding the bare `ContentIdentity.key` mint addresses

_SIZES: frozendict[SheetSize, Dimensions] = frozendict({
    SheetSize.A0: (2383.94, 3370.39),
    SheetSize.A1: (1683.78, 2383.94),
    SheetSize.A2: (1190.55, 1683.78),
    SheetSize.A3: (841.89, 1190.55),
    SheetSize.A4: (595.28, 841.89),
    SheetSize.A5: (419.53, 595.28),
    SheetSize.ANSI_A: (612.0, 792.0),
    SheetSize.ANSI_B: (792.0, 1224.0),
    SheetSize.ANSI_C: (1224.0, 1584.0),
    SheetSize.ANSI_D: (1584.0, 2448.0),
    SheetSize.ANSI_E: (2448.0, 3168.0),
    SheetSize.ARCH_A: (648.0, 864.0),
    SheetSize.ARCH_B: (864.0, 1296.0),
    SheetSize.ARCH_C: (1296.0, 1728.0),
    SheetSize.ARCH_D: (1728.0, 2592.0),
    SheetSize.ARCH_E: (2592.0, 3456.0),
    SheetSize.ARCH_E1: (2160.0, 3024.0),
    SheetSize.JIS_B0: (2919.69, 4127.24),
    SheetSize.JIS_B1: (2063.62, 2919.69),
})

_BLOCK_W, _BLOCK_H, _MARGIN, _ROW_H = 510.0, 196.0, 28.35, 14.0  # title-block extent + margin + row pitch, points (top-left origin)
_PT_PER_MM: float = 72.0 / 25.4  # 2.834645669 — points per millimetre, the ISO 5457 field-length and Scale.bar anchor
_ZONE_FIELD_MM: float = 50.0  # ISO 5457 §4.4: the grid reference field length is 50 mm
# ISO 5457 §4.4: the letters I and O are excluded (confusable with 1 and 0), so the grid alphabet is 24 letters, not 26
_GRID_LETTERS: tuple[str, ...] = tuple(letter for letter in string.ascii_uppercase if letter not in "IO")

# weasyprint's `pdf_variant` closes the B-levels and bare `a-4` only; a level outside this set refuses at the
# author — an explicit boundary outcome, never a silent draft downgrade.
_WEASY: Final[frozenset[PdfA]] = frozenset({PdfA.A1B, PdfA.A2B, PdfA.A3B, PdfA.A4})
_PDF20: Final[frozenset[PdfA]] = frozenset({PdfA.A4, PdfA.A4E, PdfA.A4F})  # PDF/A-4 is a PDF 2.0 profile
_PDF14: Final[frozenset[PdfA]] = frozenset({PdfA.A1A, PdfA.A1B})  # PDF/A-1 is a PDF 1.4 profile

_CELL_CSS: str = "span.v{font-family:Helvetica,sans-serif;font-size:8pt;color:#000}"  # the insert_htmlbox Fill cell style
_INDEX_COLS: tuple[str, ...] = ("sheet", "title", "discipline", "revision", "suitability", "date")
_REVISION_COLS: tuple[str, ...] = ("rev", "date", "description", "by")


# --- [MODELS] ---------------------------------------------------------------------------
class PdfProfile(Struct, frozen=True):
    # ONE conformance declaration: two orthogonal axes, every engine token a DERIVED projection — the
    # typst tokens off the member values, the weasyprint variant off `_WEASY`, the reportlab version pin off the
    # archival part, the XMP pdfaid pair off the member spelling. Zero parallel token tables.
    archival: PdfA | None = None
    accessible: PdfUa | None = None

    @property
    def pinned(self) -> bool:  # any conformance claim — gates archival hygiene, box pinning, and the XMP packet
        return self.archival is not None or self.accessible is not None

    @property
    def tags(self) -> bool:  # PDF/UA or a level-A archival profile both force the structure tree
        return self.accessible is not None or (self.archival is not None and self.archival.value.endswith("a"))

    @property
    def tokens(self) -> tuple[str, ...]:  # the typst `pdf_standards` list — member values ARE the tokens
        return (*((self.archival.value,) if self.archival is not None else ()), *((self.accessible.value,) if self.accessible is not None else ()))

    @property
    def variant(self) -> str | None:  # the weasyprint `pdf_variant`; None = draft; an unsupported level refuses at the author
        if self.archival is not None:
            return f"pdf/{self.archival.value}" if self.archival in _WEASY else None
        return f"pdf/{self.accessible.value}" if self.accessible is not None else None

    @property
    def version(self) -> tuple[int, int] | None:  # the reportlab `pdfVersion` pin derived from the archival part
        match self.archival:
            case None:
                return (1, 7) if self.accessible is not None else None
            case level if level in _PDF14:
                return (1, 4)
            case level if level in _PDF20:
                return (2, 0)
            case _:
                return (1, 7)

    @property
    def pdfaid(self) -> tuple[str, str] | None:  # the XMP pdfaid (part, conformance) — derived from the member spelling
        return (self.archival.value[2], self.archival.value[3:].upper()) if self.archival is not None else None


DRAFT: Final[PdfProfile] = PdfProfile()
ARCHIVAL: Final[PdfProfile] = PdfProfile(archival=PdfA.A3B)
ACCESSIBLE: Final[PdfProfile] = PdfProfile(accessible=PdfUa.UA1)
PRESERVED: Final[PdfProfile] = PdfProfile(archival=PdfA.A3B, accessible=PdfUa.UA1)


class ZoneSpec(Struct, frozen=True):  # ISO 5457 Table 2 field counts — numerals along the long edge, letters along the short edge
    long_side: int  # "Long side" numeral fields (ISO 5457 Table 2)
    short_side: int  # "Short side" letter fields

    @staticmethod
    def of(dims: Dimensions) -> "ZoneSpec":
        # ISO 5457 §4.4 fixes the field length at 50 mm, so a size absent from Table 2 divides each edge by 50 mm.
        long_pt, short_pt = max(dims), min(dims)
        return ZoneSpec(
            long_side=max(round(long_pt / _PT_PER_MM / _ZONE_FIELD_MM), 1), short_side=max(round(short_pt / _PT_PER_MM / _ZONE_FIELD_MM), 1)
        )


class FieldRow(Struct, frozen=True):  # one title-block grid cell — a field is one row, never a parameter
    label: str
    value: str = ""
    span: int = 1
    group: str = "general"


class Revision(Struct, frozen=True):  # one row of the revision history table
    mark: str
    date: str = ""
    description: str = ""
    by: str = ""


class Scale(Struct, frozen=True):  # the drawing scale over an ISO 5455 ScaleRatio, drawn as a ratio AND a graphic divided bar
    ratio: ScaleRatio = ScaleRatio.FULL  # the drawing/regime#REGIME ISO 5455 vocabulary member, never a free string
    bar_length: float = 0.0  # graphic-bar total paper extent, points; zero suppresses the bar
    segments: int = 4  # equal divisions the ruler is ticked into
    units: str = "mm"

    @property
    def printed(self) -> str:  # the "1:100" string the grid prints and the caption carries, off the ScaleRatio owner
        return self.ratio.ratio

    def bar(self, x: float, y: float, /) -> tuple[tuple[Box, bool | str], ...]:
        # Divided graphic-scale ruler at (x, y): each division carries its fill state as a `bool`, the
        # caption the REAL ground distance (paper mm / ISO 5455 factor), so a 1:100 bar reads its true span.
        if self.bar_length <= 0.0 or self.segments <= 0:
            return ()
        step, height = self.bar_length / self.segments, _ROW_H * 0.4
        real = (self.bar_length / _PT_PER_MM) / self.ratio.factor
        divisions = tuple(((x + i * step, y, x + (i + 1) * step, y + height), bool(i % 2)) for i in range(self.segments))
        return (*divisions, ((x, y + height, x + self.bar_length, y + height + _ROW_H), f"0 — {real:g} {self.units} ({self.printed})"))


class NorthArrow(Struct, frozen=True):  # the north-arrow graphic cell
    bearing: float = 0.0
    glyph: bytes = b""


class KeyPlan(Struct, frozen=True):  # the key-plan reference graphic cell
    figure: bytes = b""
    highlight: str = ""


class Viewport(Struct, frozen=True):
    # a scaled model-space view: the ISO 5455 ScaleRatio maps the model box onto the sheet as one svgelements
    # affine, so `window()` IS the scaled model extent (a real affine-placed viewport, not a keep-proportion
    # fit); graphic/vector/path#PATH reifies the same Matrix for the SVG path, the pymupdf path draws into
    # `window()` with `model` as the show_pdf_page clip.
    scale: ScaleRatio
    model: Box  # the model-space region (drawing units) the view frames — the show_pdf_page clip
    anchor: tuple[float, float] = (0.0, 0.0)  # the sheet placement origin (points) the scaled model corner lands at

    def matrix(self) -> "Matrix":
        # scale by the ISO 5455 factor, translate the model origin onto the anchor — built as a DIRECT 6-value
        # affine because svgelements `Matrix.post_translate`/`inverse` MUTATE in place and return None.
        factor = self.scale.factor
        return Matrix(factor, 0.0, 0.0, factor, self.anchor[0] - self.model[0] * factor, self.anchor[1] - self.model[1] * factor)

    def window(self) -> Box:
        # Projects the model corners through the matrix via the non-mutating `Point * Matrix` — svgelements
        # `Matrix.transform_point` mutates a MUTABLE arg in place and raises on a tuple.
        transform = self.matrix()
        lo, hi = Point(self.model[0], self.model[1]) * transform, Point(self.model[2], self.model[3]) * transform
        return (min(lo.x, hi.x), min(lo.y, hi.y), max(lo.x, hi.x), max(lo.y, hi.y))


class TitleBlock(Struct, frozen=True):
    project: str = ""
    legal_owner: str = ""  # ISO 7200 §5.1.2 legal owner (firm/enterprise) — the mandatory field the audit reads
    sheet_number: str = ""  # ISO 7200 §5.1.3/§5.1.6 identification + sheet number (the drawing/regime SheetId "A-201" in AEC)
    sheet_total: str = ""  # the sheet-set count drawn as "Sheet {number} of {total}"
    sheet_title: str = ""  # ISO 7200 §5.2 title
    discipline: str = ""
    document_type: str = "drawing"  # ISO 7200 §5.3.6 document type (drawing / specification / schedule)
    status: str = ""  # the issue purpose ("FOR CONSTRUCTION" / "FOR TENDER" / "PRELIMINARY" / "AS-BUILT")
    date: str = ""  # ISO 7200 §5.1.5 date of issue
    drawn_by: str = ""  # ISO 7200 §5.3.5 creator
    checked_by: str = ""
    approved_by: str = ""  # ISO 7200 §5.3 approval person
    scale: Scale = Scale()
    fields: tuple[FieldRow, ...] = ()
    revisions: tuple[Revision, ...] = ()
    north: NorthArrow = NorthArrow()
    key_plan: KeyPlan = KeyPlan()

    def grid(self) -> Block[FieldRow]:
        # head fields carry their title-block ZONE in `group` so `cells` keeps each zone contiguous; the compact
        # `Rev` cell shows the latest index while the full history lowers to `revised()`.
        sheet = f"{self.sheet_number} of {self.sheet_total}" if self.sheet_total else self.sheet_number
        latest = self.revisions[-1].mark if self.revisions else ""
        head = (
            FieldRow("Project", self.project, group="identity"),
            FieldRow("Owner", self.legal_owner, group="identity"),
            FieldRow("Sheet", sheet, group="identity"),
            FieldRow("Title", self.sheet_title, span=2, group="identity"),
            FieldRow("Discipline", self.discipline, group="identity"),
            FieldRow("Type", self.document_type, group="identity"),
            FieldRow("Status", self.status, span=2, group="identity"),
            FieldRow("Scale", self.scale.printed, group="identity"),
            FieldRow("Date", self.date, group="identity"),
            FieldRow("Drawn", self.drawn_by, group="approval"),
            FieldRow("Checked", self.checked_by, group="approval"),
            FieldRow("Approved", self.approved_by, group="approval"),
            FieldRow("Rev", latest, group="approval"),
        )
        return Block.of_seq(head).append(Block.of_seq(self.fields))

    def cells(self, dims: Dimensions) -> tuple[FieldCell, ...]:
        # ONE field-rect correspondence the frame author draws and the `Fill` arm binds INTO (top-left
        # origin); `span` widens the value column leftward, `group` keeps each zone contiguous via a STABLE
        # sort, and the block anchors bottom-right clamped to the rows the `_BLOCK_H` extent admits.
        width, height = dims
        origin_y, col, right = height - _MARGIN - _BLOCK_H, _BLOCK_W * 0.5, width - _MARGIN
        zones: tuple[str, ...] = ("identity", "approval", "general")
        rank = frozendict({zone: index for index, zone in enumerate(zones)})
        ordered = self.grid().sort_with(lambda row: rank.get(row.group, len(zones)))
        return tuple(
            (row, (right - col * max(row.span, 1), origin_y + index * _ROW_H, right, origin_y + (index + 1) * _ROW_H))
            for index, row in enumerate(ordered.take(min(len(ordered), int(_BLOCK_H / _ROW_H))))
        )

    def history(self) -> Block[Revision]:
        return Block.of_seq(self.revisions)

    def outline(self) -> list[list[int | str]]:  # the pymupdf `set_toc` [level, title, page] row contract
        title = self.sheet_title or self.sheet_number
        revs: list[list[int | str]] = [[2, f"Rev {r.mark} {r.date}".strip(), 1] for r in self.revisions]
        return [[1, title, 1], *revs] if title else revs

    def metadata(self) -> dict[str, str]:
        return {
            "title": self.sheet_title or self.sheet_number,
            "subject": f"{self.discipline} {self.sheet_number}".strip(),
            "author": self.drawn_by,
            "keywords": self.project,
            "creator": "rasm.artifacts.sheet",
        }

    def audit(self) -> "TitleBlockAudit":
        # ISO 7200 mandatory-field conformance fold over the `_ISO7200` predicate table, mirroring
        # document/tagged#StructureAudit: present vs missing over the exact mandatory set (revision excluded).
        audited = tuple((field, probe(self)) for field, probe in _ISO7200.items())
        return TitleBlockAudit(present=tuple(field for field, ok in audited if ok), missing=tuple(field for field, ok in audited if not ok))

    def revised(self, fmt: TableFormat = TableFormat.HTML, theme: Theme = Theme()) -> TablePlan:
        # Revision history as a visualization/table#TABLE publication table; a bare frame projects an empty one.
        rows = [{"rev": r.mark, "date": r.date, "description": r.description, "by": r.by} for r in self.revisions]
        frame = polars.from_dicts(rows) if rows else polars.DataFrame(schema={column: polars.String for column in _REVISION_COLS})
        ops = (
            TableOp.Header("Revisions", subtitle=self.sheet_number or self.sheet_title or None),
            TableOp.Style((("text", {"weight": "bold"}),), at=StubLoc.COLUMN_LABELS),
        )
        return TablePlan(frame=frame, ops=ops, fmt=fmt, theme=theme)


class TitleBlockAudit(Struct, frozen=True, gc=False):  # the ISO 7200 mandatory-field conformance verdict, mirroring document/tagged#StructureAudit
    present: tuple[Iso7200Field, ...]
    missing: tuple[Iso7200Field, ...]

    @property
    def conformant(self) -> bool:  # every ISO 7200 mandatory field present
        return not self.missing

    @property
    def coverage(self) -> float:
        total = len(self.present) + len(self.missing)
        return 1.0 if total == 0 else len(self.present) / total

    def facts(self) -> dict[str, object]:  # the scalar projection a span/log consumer reads
        return {"present": len(self.present), "missing": ",".join(self.missing), "conformant": self.conformant}


class PlacementPolicy(Struct, frozen=True):
    keep_proportion: bool = True
    overlay: bool = True
    layered: bool = True
    visible: bool = True
    locked: bool = False
    groups: tuple[str, ...] = ()
    membership: Membership = Membership.ANY_ON


class FigurePlacement(Struct, frozen=True):
    figure: bytes
    target: tuple[Box, Box | None] | Viewport
    name: str = "figure"  # the OCG label the Place arm mints and binds this figure to
    page: int = 0
    rotate: Quarter = 0
    policy: PlacementPolicy = PlacementPolicy()

    def __post_init__(self) -> None:
        _admit_rotation(self.rotate)  # msgspec ignores `Is` at construction, so every mint rails an off-axis rotate before `show_pdf_page` sees it

    def resolved(self) -> tuple[Box, Box | None]:
        match self.target:
            case Viewport() as viewport:
                return viewport.window(), viewport.model
            case (cell, clip):
                return cell, clip

    @staticmethod
    def arranged(figures: tuple[bytes, ...], region: Box, rules: tuple[Rule, ...], /, *, name: str = "figure") -> tuple["FigurePlacement", ...]:
        # constraint-solved placement: each figure's PDF page extent feeds compose#COMPOSE's Cassowary
        # `arranged` solve, the solved boxes landing as cells — declared alignment/chain/pin/center rules
        # replace hand-computed sheet coordinates, one solver shared with the figure-arrange arm.
        extents = tuple(_figure_extent(figure) for figure in figures)
        return tuple(
            FigurePlacement(figure=figure, target=(box, None), name=f"{name}-{index}", policy=PlacementPolicy())
            for index, (figure, box) in enumerate(zip(figures, arranged(extents, region, rules), strict=True))
        )


class Composed(Struct, frozen=True):
    # one evidence struct every projection reads — no second render; imposition#IMPOSE imports this owner,
    # its imposed press form counting sheets on the same `pages` slot and its proof band riding `scores`.
    data: bytes
    pages: int
    kind: ComposedKind = ComposedKind.PDF
    extent: tuple[int, int] = (0, 0)  # the Preview pixmap width/height the raster receipt rides
    layers: int = 0  # the count of OCGs the placement fold minted (leaves + shared groups), riding the Egress overlays slot
    scores: frozendict[str, float | str] = frozendict()  # the Preview evidence band — empty when the arm measures nothing
    layer_rows: tuple[Layer, ...] = ()


class SheetIssue(Struct, frozen=True):
    # one member of a SheetSet — the drawing/regime#REGIME SheetId number, the sheet's TitleBlock, and the
    # raw ISO 19650 delivery codes delivery/register#REGISTER validates (plain strings, parsed at that
    # boundary); the register folds these into its OWN `SheetContext`, a distinct shape under its own name.
    sheet_id: SheetId
    title: TitleBlock
    suitability: str = "S2"  # ISO 19650 suitability code (register parses via Suitability.parse)
    revision: str = "P01"  # ISO 19650 revision code (register parses via RevisionCode.parse)
    container_id: str = ""  # the ISO 19650 container reference; defaults to the composed SheetId


class SheetSetAudit(Struct, frozen=True, gc=False):  # the set-level ISO 7200 + numbering coverage verdict
    sheets: int
    conformant_sheets: int
    duplicate_numbers: tuple[str, ...]  # sheet numbers claimed by more than one entry
    missing_fields: tuple[Iso7200Field, ...]  # the union of ISO 7200 fields missing across the set
    per_sheet: tuple[TitleBlockAudit, ...]

    @property
    def conformant(self) -> bool:
        return self.conformant_sheets == self.sheets and not self.duplicate_numbers


@tagged_union(frozen=True)
class SheetOp:  # the closed request vocabulary lowered once into Composed
    tag: Literal["frame", "place", "fill", "stamp", "preview"] = tag()
    frame: tuple[SheetSize, Engine, TitleBlock, PdfProfile, OutputIntent, Orientation] = case()
    place: tuple[bytes, tuple[FigurePlacement, ...]] = case()
    fill: tuple[bytes, TitleBlock] = case()
    stamp: tuple[bytes, TitleBlock, PdfProfile, OutputIntent, tuple[tuple[str, bytes], ...]] = case()
    preview: tuple[bytes, float] = case()

    @staticmethod
    def Frame(
        size: SheetSize,
        engine: Engine = Engine.REPORTLAB,
        title: TitleBlock = TitleBlock(),
        *,
        profile: PdfProfile = DRAFT,
        output_intent: OutputIntent = None,
        orientation: Orientation = Orientation.PORTRAIT,
    ) -> "SheetOp":
        return SheetOp(frame=(size, engine, title, profile, output_intent, orientation))

    @staticmethod
    def Place(sheet: bytes, placements: FigurePlacement | Iterable[FigurePlacement]) -> "SheetOp":
        match placements:
            case FigurePlacement():
                return SheetOp(place=(sheet, (placements,)))
            case stream:
                return SheetOp(place=(sheet, tuple(stream)))

    @staticmethod
    def Fill(sheet: bytes, title: TitleBlock = TitleBlock()) -> "SheetOp":
        return SheetOp(fill=(sheet, title))

    @staticmethod
    def Stamp(
        sheet: bytes,
        title: TitleBlock = TitleBlock(),
        *,
        profile: PdfProfile = DRAFT,
        output_intent: OutputIntent = None,
        attachments: Iterable[tuple[str, bytes]] = (),
    ) -> "SheetOp":
        return SheetOp(stamp=(sheet, title, profile, output_intent, tuple(attachments)))

    @staticmethod
    def Preview(sheet: bytes, dpi: float = 96.0) -> "SheetOp":
        return SheetOp(preview=(sheet, dpi))


# --- [TABLES] ---------------------------------------------------------------------------
# ISO 5457 Table 2 — Number of fields (Long side numerals, Short side letters). The ISO-A cardinalities are
# EXACT and load-bearing; A5 and the non-ISO-A ANSI/ARCH/JIS-B sizes derive through `ZoneSpec.of` (50 mm field).
_ZONES: frozendict[SheetSize, ZoneSpec] = frozendict({
    SheetSize.A0: ZoneSpec(long_side=24, short_side=16),
    SheetSize.A1: ZoneSpec(long_side=16, short_side=12),
    SheetSize.A2: ZoneSpec(long_side=12, short_side=8),
    SheetSize.A3: ZoneSpec(long_side=8, short_side=6),
    SheetSize.A4: ZoneSpec(long_side=6, short_side=4),
})

# ISO 7200 mandatory-field predicate table: one row per field mapping to the TitleBlock evidence that
# satisfies it, the single edit site the `audit()` fold derives present/missing from. IDENTIFICATION_NUMBER and
# SHEET_NUMBER both read `sheet_number` because in AEC the sheet number (the SheetId) IS the drawing identifier.
_ISO7200: frozendict[Iso7200Field, Callable[[TitleBlock], bool]] = frozendict({
    Iso7200Field.LEGAL_OWNER: lambda title: bool(title.legal_owner),
    Iso7200Field.IDENTIFICATION_NUMBER: lambda title: bool(title.sheet_number),
    Iso7200Field.DATE_OF_ISSUE: lambda title: bool(title.date),
    Iso7200Field.SHEET_NUMBER: lambda title: bool(title.sheet_number),
    Iso7200Field.TITLE: lambda title: bool(title.sheet_title),  # TITLE reads its OWN field — a sheet number never satisfies a blank title
    Iso7200Field.APPROVAL_PERSON: lambda title: bool(title.approved_by),
    Iso7200Field.CREATOR: lambda title: bool(title.drawn_by),
    Iso7200Field.DOCUMENT_TYPE: lambda title: bool(title.document_type),
})


# --- [SERVICES] -------------------------------------------------------------------------
class Sheet(Struct, frozen=True):
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    op: SheetOp
    lane: LanePolicy
    composed: Option[Composed] = Nothing

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: canonical op payload minted PRE-RUN through the bare `ContentIdentity.key`
        # (`of` returns the railed `RuntimeRail[ContentKey]`) — never a key over the composed sheet bytes;
        # placed-figure parent keys ride ArtifactWork.parents where the caller holds them (DATA edges).
        return ContentIdentity.key(f"sheet-{self.op.tag}", _CANON.encode(self.op))

    def folded(self) -> Self:
        # Sync evidence successor: ONE `_composed` render lands on `composed`, and contribute/layers read
        # it — never a per-projection re-render of the frame author or the native serializer.
        return structs.replace(self, composed=Some(_composed(self.op)))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        return await async_boundary(f"sheet.{self.op.tag}", self._folded, catch=_FAULTS)

    async def _folded(self) -> ArtifactReceipt:
        # Async execution: the `_composed` fold crosses as one HOSTILE process kernel — every arm's engine holds the
        # GIL — and the receipt derives from that one fold; the terminal threads the PRE-RUN key.
        rail = await self.lane.offload(Kernel.of(_composed, KernelTrait.HOSTILE), self.op)
        return self._receipt(self._key, rail.default_with(_sheet_raise))

    def _receipt(self, key: ContentKey, composed: Composed, /) -> ArtifactReceipt:
        match composed.kind:
            case ComposedKind.PDF if composed.layers:  # the OCG/OCMD-bearing placed sheet rides the layer count on the Egress overlays slot
                return ArtifactReceipt.Egress(key, len(composed.data), composed.pages, 0, 0, composed.layers)
            case ComposedKind.PDF:
                return ArtifactReceipt.Pdf(key, len(composed.data), composed.pages)
            case ComposedKind.PREVIEW:
                return ArtifactReceipt.Preview(key, composed.extent[0], composed.extent[1], len(composed.data), composed.scores)
            case _:
                assert_never(composed.kind)

    def contribute(self) -> "Iterable[Receipt]":
        # rows ride the folded successor alone; the un-folded owner contributes nothing, so absence stays
        # distinct from evidence and no projection re-enters the render.
        yield from self.composed.map(lambda live: tuple(self._receipt(self._key, live).contribute())).default_value(())

    def layers(self, names: tuple[str, ...] = ()) -> tuple[Layer, ...]:
        return _placed_layers(self.composed, names)


class SheetSet(Struct, frozen=True):
    # Multi-sheet projection owner — numbers, audits, and projects outward to delivery/register#REGISTER
    # and visualization/table#TABLE, no receipt; the register imports THIS owner's TitleBlock, so the register
    # projection returns plain tuples and no import cycle forms.
    entries: tuple[SheetIssue, ...]
    sheets: tuple[Sheet, ...] = ()  # the member producers, index-aligned with entries where composed

    def emit(self, /) -> "Iterable[ArtifactWork]":
        # ONE node per sheet with per-member PRE-RUN keys, so a re-issued set re-renders only changed sheets.
        return tuple(sheet.emit() for sheet in self.sheets)

    @property
    def total(self) -> int:
        return len(self.entries)

    def numbered(self) -> tuple[tuple[SheetId, TitleBlock], ...]:
        # stamp each entry's `SheetId.compose()` into `sheet_number` and the set count into `sheet_total`.
        total = str(self.total)
        return tuple(
            (entry.sheet_id, structs.replace(entry.title, sheet_number=entry.sheet_id.compose(), sheet_total=total)) for entry in self.entries
        )

    def audited(self) -> SheetSetAudit:
        # fold the per-sheet ISO 7200 audit, flag duplicate numbers (a Counter over composed SheetIds), and
        # union the missing fields — one pass, never a re-walk.
        numbered = self.numbered()
        per_sheet = tuple(title.audit() for _sheet_id, title in numbered)
        seen = Counter(sheet_id.compose() for sheet_id, _title in numbered)
        return SheetSetAudit(
            sheets=self.total,
            conformant_sheets=sum(1 for verdict in per_sheet if verdict.conformant),
            duplicate_numbers=tuple(number for number, count in seen.items() if count > 1),
            missing_fields=tuple(dict.fromkeys(field for verdict in per_sheet for field in verdict.missing)),
            per_sheet=per_sheet,
        )

    def frame(self) -> "pl.DataFrame":
        # one polars drawing-list frame `scheduled` renders; an empty set yields the typed empty frame so a
        # downstream consumer never hits a schema-less DataFrame.
        rows = [
            {
                "sheet": sheet_id.compose(),
                "title": title.sheet_title or title.sheet_number,
                "discipline": title.discipline,
                "revision": entry.revision,
                "suitability": entry.suitability,
                "date": title.date,
            }
            for entry, (sheet_id, title) in zip(self.entries, self.numbered(), strict=True)
        ]
        return polars.from_dicts(rows) if rows else polars.DataFrame(schema={column: polars.String for column in _INDEX_COLS})

    def registered(self) -> tuple[tuple[str, TitleBlock, str, str], ...]:
        # Projects the (container_id, numbered TitleBlock, suitability, revision) tuples delivery/register#REGISTER's
        # polymorphic `Register.admit` admits beside its OWN `SheetContext` — a pure projection with NO
        # register import, so no cycle forms.
        return tuple(
            (entry.container_id or sheet_id.compose(), title, entry.suitability, entry.revision)
            for entry, (sheet_id, title) in zip(self.entries, self.numbered(), strict=True)
        )

    def scheduled(self, fmt: TableFormat = TableFormat.HTML, theme: Theme = Theme()) -> TablePlan:
        # Drawing-list publication table projected to visualization/table#TABLE, that owner's render not this one's.
        ops = (
            TableOp.Header("Drawing List", subtitle=f"{self.total} sheets"),
            TableOp.Spanner("Identity", columns=["sheet", "title", "discipline"]),
            TableOp.Spanner("Issue", columns=["revision", "suitability", "date"]),
            TableOp.Style((("text", {"weight": "bold"}),), at=StubLoc.COLUMN_LABELS),
        )
        return TablePlan(frame=self.frame(), ops=ops, fmt=fmt, theme=theme)


# --- [OPERATIONS] -----------------------------------------------------------------------
# a malformed `TitleBlock`/`FigurePlacement`/`Box`/`Viewport` inside a well-tagged `SheetOp` raises
# `BeartypeCallHintViolation` from this guarded fold, the `_FAULTS` tuple admits.
_GUARD = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))


@_GUARD
def _admit_rotation(rotate: Quarter, /) -> None:
    return None  # the `@_GUARD` contract IS the work — `FigurePlacement.__post_init__` deep-checks the `Is`-refined scalar on every construction


def _zones(size: SheetSize, dims: Dimensions, /) -> tuple[tuple[float, float, str], ...]:
    # ISO 5457 grid at the EXACT Table 2 field count: numerals along the LONG edge, letters (I/O excluded)
    # along the SHORT edge, so the wider oriented dimension selects the axis; A4 draws top/right edges only.
    spec = _ZONES.get(size, ZoneSpec.of(dims))
    width, height = dims
    columns, rows = (spec.long_side, spec.short_side) if width >= height else (spec.short_side, spec.long_side)
    span_w, span_h, strip = width - 2.0 * _MARGIN, height - 2.0 * _MARGIN, _MARGIN * 0.5
    step_x, step_y = span_w / columns, span_h / rows
    num_edges = (height - strip - 3.0,) if size is SheetSize.A4 else (strip - 3.0, height - strip - 3.0)
    let_edges = (width - strip,) if size is SheetSize.A4 else (strip, width - strip)
    numerals = tuple((_MARGIN + (col + 0.5) * step_x, edge, str(col + 1)) for col in range(columns) for edge in num_edges)
    letters = tuple(
        (edge, height - _MARGIN - (row + 0.5) * step_y - 3.0, _GRID_LETTERS[row % len(_GRID_LETTERS)]) for row in range(rows) for edge in let_edges
    )
    return (*numerals, *letters)


def _sheet_raise(fault: object) -> Composed:
    # terminal collapse at the sheet boundary: an offload fault reconstructs the raise _FAULTS folds.
    raise ValueError(str(fault))


def _figure_extent(figure: bytes) -> tuple[float, float]:
    with pymupdf.open(stream=figure, filetype="pdf") as doc:  # only the first page rect is read; the handle then closes
        if not doc.page_count:
            raise ValueError("figure PDF carries zero pages")
        rect = doc[0].rect
        return (rect.width, rect.height)


@_GUARD
def _composed(op: SheetOp) -> Composed:  # the one pure render fold — offloaded by the emission, landed by `folded()`
    match op:
        case SheetOp(tag="frame", frame=(size, engine, title, profile, intent, orientation)):
            dims = orientation.of(_SIZES[size])
            data = _FRAME[engine](dims, size, title, profile, intent)
            with pymupdf.open(stream=data, filetype="pdf") as doc:  # the page count is read off the handle, which then closes
                return Composed(data, pages=doc.page_count, layer_rows=(Layer("frame", data, (0.0, 0.0, *dims)),))
        case SheetOp(tag="place", place=(sheet, placements)):
            with pymupdf.open(stream=sheet, filetype="pdf") as document:  # the live native handle closes once the placed bytes are serialized
                groups = _mint_groups(document, placements)  # one shared OCG per unique membership group name
                minted = Block.of_seq(_draw_one(document, placement, groups) for placement in placements).choose(
                    lambda drawn: Some(drawn) if drawn[0] else Nothing  # keep only the rows that minted a real OCG leaf xref
                )
                _configure_layers(document, minted, groups)  # one ui-config write over the minted leaves + shared groups
                data = document.tobytes(garbage=3, deflate=True, no_new_id=True)
                return Composed(
                    data,
                    pages=document.page_count,
                    layers=len(minted) + len(groups),
                    layer_rows=tuple(
                        Layer(
                            placement.name,
                            placement.figure,
                            placement.resolved()[0],
                            visible=placement.policy.visible,
                            locked=placement.policy.locked,
                        )
                        for placement in placements
                        if placement.policy.layered  # only rows that minted a real OCG leaf, aligned with `minted`/`Composed.layers`
                    ),
                )
        case SheetOp(tag="fill", fill=(sheet, title)):
            with pymupdf.open(stream=sheet, filetype="pdf") as document:  # the live native handle closes once the filled bytes are serialized
                if not document.page_count:
                    raise ValueError("sheet PDF carries zero pages")
                page = document[0]  # its real `rect` is the authoritative sheet geometry, oriented as drawn
                for row, rect in title.cells((page.rect.width, page.rect.height)):  # the SAME TitleBlock.cells geometry the frame drew
                    page.insert_htmlbox(pymupdf.Rect(*rect), _cell_html(row), css=_CELL_CSS)  # rich CSS-styled Fill over the flat insert_textbox
                return Composed(document.tobytes(garbage=3, deflate=True, no_new_id=True), pages=document.page_count)
        case SheetOp(tag="stamp", stamp=(sheet, title, profile, intent, attachments)):
            with pymupdf.open(stream=sheet, filetype="pdf") as document:  # the live native handle closes once the stamped bytes are serialized
                if profile.pinned:
                    # archival hygiene BEFORE the metadata write, the metadata/xml/attachment flags OFF so the
                    # pass never nukes what this arm then authors; the explicit TrimBox pin is the page-box
                    # floor press tools and PDF/X preflight read.
                    document.subset_fonts(fallback=True)
                    document.scrub(
                        hidden_text=True,
                        javascript=True,
                        clean_pages=True,
                        thumbnails=True,
                        metadata=False,
                        xml_metadata=False,
                        embedded_files=False,
                        attached_files=False,
                        redactions=False,
                        reset_fields=False,
                        reset_responses=False,
                        remove_links=False,
                    )
                    for page in document:
                        page.set_trimbox(page.rect)
                document.set_metadata(title.metadata())
                document.set_toc(title.outline())
                for name, payload in (
                    *attachments,
                    *(((f"{intent[0]}.icc", intent[1]),) if intent is not None else ()),
                ):  # source files + the PDF/A ICC ride as associated files; the ternary is parenthesized so `*` unpacks its result
                    document.embfile_add(name, payload, filename=name, desc=f"{title.discipline} source".strip())
                if profile.pinned:  # the archival/accessible XMP packet is gated on the preservation intent, not a per-engine variant column
                    document.set_xml_metadata(_xmp(title, profile))
                return Composed(document.tobytes(garbage=3, deflate=True, no_new_id=True), pages=document.page_count)
        case SheetOp(tag="preview", preview=(sheet, dpi)):
            with pymupdf.open(stream=sheet, filetype="pdf") as document:  # the handle closes once the preview pixmap is rendered off it
                if not document.page_count:
                    raise ValueError("sheet PDF carries zero pages")  # the `_figure_extent` refusal, lifted by the `_FAULTS` fold
                pixmap = document[0].get_pixmap(dpi=int(dpi))
                return Composed(pixmap.tobytes("png"), pages=1, kind=ComposedKind.PREVIEW, extent=(pixmap.width, pixmap.height))
        case _:
            assert_never(op)


def _mint_groups(document: "Document", placements: tuple[FigurePlacement, ...], /) -> "frozendict[str, int]":
    # ONE shared OCG per unique membership group, its N placements members via OCMD; only a layered placement's
    # membership contributes a group.
    names = tuple(dict.fromkeys(name for placement in placements if placement.policy.layered for name in placement.policy.groups))
    return frozendict({name: document.add_ocg(name, on=True, intent="View", usage="Artwork") for name in names})


def _draw_one(
    document: "Document", placement: FigurePlacement, groups: "frozendict[str, int]", /
) -> tuple[int, bool, bool]:  # (xref, visible, locked); xref 0 if unlayered
    page = document[0]
    policy = placement.policy
    leaf = document.add_ocg(placement.name, on=policy.visible, intent="View", usage="Artwork") if policy.layered else 0
    # a `membership`-bearing placement gates through an OCMD over its parent groups; the leaf rides the layer
    # config, the OCMD the draw.
    oc = (
        document.set_ocmd(ocgs=[leaf, *(groups[name] for name in policy.groups)], policy=policy.membership.value)
        if leaf and policy.groups
        else leaf
    )
    window, clip = placement.resolved()
    with pymupdf.open(
        stream=placement.figure, filetype="pdf"
    ) as docsrc:  # `show_pdf_page` copies the source page at call time, so the figure handle closes immediately after the draw
        page.show_pdf_page(
            pymupdf.Rect(*window),
            docsrc,
            pno=placement.page,
            keep_proportion=policy.keep_proportion,
            overlay=policy.overlay,
            rotate=placement.rotate,
            clip=pymupdf.Rect(*clip) if clip is not None else None,
            oc=oc,
        )
    return leaf, policy.visible, policy.locked


def _configure_layers(document: "Document", minted: Block[tuple[int, bool, bool]], groups: "frozendict[str, int]", /) -> None:
    # one `set_layer` ui-config write over the minted placement leaves AND the shared membership groups — the
    # reader's panel toggles `on`/`off` and honors `locked`, exactly as export/layered#LAYERED `PdfOcg` does.
    if minted.is_empty() and not groups:
        return
    document.set_layer(  # config -1 IS the default OC configuration; 0 addresses an alternate /Configs entry and raises on a fresh document
        -1,
        on=[xref for xref, visible, _locked in minted if visible] + list(groups.values()),
        off=[xref for xref, visible, _locked in minted if not visible],
        locked=[xref for xref, _visible, locked in minted if locked],
    )


def _cell_html(row: FieldRow) -> str:
    # Fill value as escaped CSS-styled HTML for `insert_htmlbox`; the dynamic value is neutralized by
    # `html.escape`, never an unescaped markup splice.
    return f'<span class="v">{escape(row.value)}</span>'


def _frame_reportlab(dims: Dimensions, size: SheetSize, title: TitleBlock, profile: PdfProfile, intent: OutputIntent) -> bytes:
    width, height = dims
    sink = BytesIO()
    # reportlab authors only the conformance REQUEST (the derived `pdfVersion` pin + `lang` tag for PDF/UA);
    # PDF/A output-intent ICC embed and the structure-tree close are `exchange/conformance#CONFORMANCE`'s.
    canvas = Canvas(sink, pagesize=dims, pdfVersion=profile.version, lang="en" if profile.tags else None)
    meta = title.metadata()  # the ISO 7200 info-dict the frame self-describes, so the sheet carries its metadata natively
    canvas.setTitle(meta["title"])
    canvas.setAuthor(meta["author"])
    canvas.setSubject(meta["subject"])
    canvas.setKeywords(meta["keywords"])
    canvas.setCreator(meta["creator"])
    ink = CMYKColor(0.0, 0.0, 0.0, 1.0)  # process-black (CMYK K=1) — press-faithful separations, not the sRGB/DeviceGray default
    canvas.setStrokeColor(ink)
    canvas.setFillColor(ink)
    border = canvas.beginPath()
    border.rect(_MARGIN, _MARGIN, width - 2 * _MARGIN, height - 2 * _MARGIN)
    canvas.setLineWidth(1.0)
    canvas.drawPath(border, stroke=1, fill=0)
    canvas.rect(width - _MARGIN - _BLOCK_W, _MARGIN, _BLOCK_W, _BLOCK_H)
    canvas.setFont("Helvetica", 7)
    for zx, zy, mark in _zones(size, dims):  # the exact ISO 5457 Table 2 reference grid (I/O excluded)
        canvas.drawCentredString(zx, zy, mark)
    for row, (rx, top, _x1, _y1) in title.cells(dims):  # reportlab is bottom-up; flip the top-left cell rect into the page frame
        text = canvas.beginText(rx, height - top - _ROW_H + 3.0)
        text.setFont("Helvetica-Bold", 8)
        text.textLine(f"{row.label}:")  # LABEL only — SheetOp.Fill is the one value-binding stage over the same cells geometry
        canvas.drawText(text)
    if title.north.glyph:  # the north arrow rotated to its `bearing` (CW-positive map convention -> CCW canvas)
        canvas.saveState()
        canvas.translate(width - _MARGIN - _BLOCK_W + _MARGIN, _MARGIN + _MARGIN)
        canvas.rotate(-title.north.bearing)
        canvas.drawImage(
            ImageReader(BytesIO(title.north.glyph)), -_MARGIN, -_MARGIN, width=_MARGIN * 2, height=_MARGIN * 2, preserveAspectRatio=True, mask="auto"
        )
        canvas.restoreState()
    if title.key_plan.figure:
        kx, ky = width - _MARGIN - _BLOCK_W, _MARGIN + _BLOCK_H + _MARGIN
        canvas.drawImage(
            ImageReader(BytesIO(title.key_plan.figure)), kx, ky, width=_MARGIN * 2, height=_MARGIN * 2, preserveAspectRatio=True, mask="auto"
        )
        if title.key_plan.highlight:  # the covered-region label annotating the key plan
            canvas.drawString(kx, ky - _ROW_H + 3.0, title.key_plan.highlight)
    for shape, mark in title.scale.bar(width - _MARGIN - _BLOCK_W, _MARGIN + _BLOCK_H + _ROW_H):  # the divided graphic scale bar above the block
        match mark:
            case bool():  # a division rect — fill state carried as the bool, never a magic "fill"/"clear" token
                canvas.rect(shape[0], shape[1], shape[2] - shape[0], shape[3] - shape[1], stroke=1, fill=int(mark))
            case str():  # the real-distance caption text
                canvas.drawString(shape[0], shape[1], mark)
    outline_key = title.sheet_number or "sheet"  # one navigable outline entry so the framed sheet has a reader bookmark
    canvas.bookmarkPage(outline_key)
    canvas.addOutlineEntry(meta["title"], outline_key, level=0)
    canvas.showPage()
    canvas.save()
    return sink.getvalue()


def _frame_typst(dims: Dimensions, _size: SheetSize, title: TitleBlock, profile: PdfProfile, _intent: OutputIntent) -> bytes:
    # typst's PDF/A output-intent is the compiler's own, the ICC not a frame-author input. Interpolate each field
    # value as a typst STRING (`[#"..."]`), never bracketed content, so `#`/`*`/`[`/`@` markup cannot inject.
    width, height = dims
    grid = ", ".join(f'[#"{_escape(row.label)}"], []' for row in title.grid())  # LABEL only — SheetOp.Fill binds the values
    source = (
        f'#set document(title: "{_escape(title.sheet_title or title.sheet_number)}", author: "{_escape(title.drawn_by)}")\n'
        f"#set page(width: {width}pt, height: {height}pt, margin: {_MARGIN}pt)\n"
        "#rect(width: 100%, height: 100%, stroke: 1pt)\n"
        "#place(bottom + right, dx: -4pt, dy: -4pt, rect(stroke: 1pt, inset: 6pt, "
        f"grid(columns: 2, gutter: 4pt, {grid})))\n"
    )
    return typst.compile(
        source.encode(), output=None, format="pdf", pdf_standards=list(profile.tokens), ignore_system_fonts=True, timestamp=0
    )


def _frame_weasyprint(dims: Dimensions, _size: SheetSize, title: TitleBlock, profile: PdfProfile, intent: OutputIntent) -> bytes:
    if profile.archival is not None and profile.accessible is not None:
        raise ValueError("weasyprint closes one pdf_variant, not a combined PDF/A + PDF/UA profile")
    if profile.archival is not None and profile.archival not in _WEASY:
        raise ValueError(f"weasyprint closes no {profile.archival.value} variant")  # explicit boundary refusal, never a silent draft downgrade
    width, height = dims
    cells = "".join(f"<tr><th>{escape(r.label)}</th><td></td></tr>" for r in title.grid())
    # <head> meta seeds weasyprint's OWN info-dict + variant XMP, so the ENGINE writes the conformant pdfaid
    # packet; the hand-built `_xmp` stays only for the pymupdf reportlab/typst Stamp arms with no engine XMP.
    metas = "".join(
        f'<meta name="{name}" content="{escape(value)}">'
        for name, value in (("author", title.drawn_by), ("description", title.project), ("keywords", title.discipline))
        if value
    )
    head = (
        f"<title>{escape(title.sheet_title or title.sheet_number)}</title>{metas}"
        f"<style>@page{{size:{width}pt {height}pt;margin:{_MARGIN}pt}}body{{border:1pt solid}}"
        f"table{{position:fixed;bottom:4pt;right:4pt;border:1pt solid}}</style>"
    )
    html = f"<head>{head}</head><body><table>{cells}</table></body>"
    # weasyprint owns the FULL archival close end to end: `pdf_variant`/`pdf_tags`/`output_intent`/`attachments`/
    # `custom_metadata`, and `pdf_identifier` pins a STABLE /ID so the framed bytes are byte-deterministic (the
    # content-key analogue of the pymupdf `no_new_id` and typst `timestamp=0`), never a random per-run identifier.
    return HTML(string=html, base_url=".").write_pdf(
        target=None,
        font_config=FontConfiguration(),
        pdf_variant=profile.variant,
        pdf_tags=profile.tags,
        output_intent=BytesIO(intent[1]) if intent is not None else None,
        attachments=[Attachment(file_obj=BytesIO(intent[1]), name=f"{intent[0]}.icc", description="output-intent ICC", relationship="Data")]
        if intent is not None
        else None,
        custom_metadata=True,
        pdf_identifier=_pdf_id(dims, title, profile, intent),
    )


def _pdf_id(dims: Dimensions, title: TitleBlock, profile: PdfProfile, intent: OutputIntent, /) -> bytes:
    # a stable 16-byte /ID over the COMPLETE frame identity — engine token, oriented dimensions, every TitleBlock
    # field (revisions and scale included) via one msgpack encode, both PdfProfile axes, and the output intent —
    # so re-framing identical inputs is byte-identical and any distinct frame input yields a distinct /ID.
    return hashlib.sha256(msgpack.encode((Engine.WEASYPRINT, dims, title, profile, intent))).digest()[:16]


_FRAME: frozendict[Engine, Author] = frozendict({
    Engine.REPORTLAB: _frame_reportlab,
    Engine.TYPST: _frame_typst,
    Engine.WEASYPRINT: _frame_weasyprint,
})


# --- [BOUNDARIES] -----------------------------------------------------------------------
def _placed_layers(composed: Option[Composed], names: tuple[str, ...]) -> tuple[Layer, ...]:
    return composed.map(
        lambda live: tuple(
            structs.replace(layer, name=names[index] if index < len(names) else layer.name)
            for index, layer in enumerate(live.layer_rows)
        )
    ).default_value(())


def _escape(value: str) -> str:  # typst markup escaping has no stdlib owner; HTML escaping rides `html.escape`
    return value.replace("\\", "\\\\").replace('"', '\\"')


_NS = frozendict({
    "x": "adobe:ns:meta/",
    "rdf": "http://www.w3.org/1999/02/22-rdf-syntax-ns#",
    "dc": "http://purl.org/dc/elements/1.1/",
    "pdfaid": "http://www.aiim.org/pdfa/ns/id/",
})


def _xmp(title: TitleBlock, profile: PdfProfile) -> str:
    # Fixed-schema XMP packet built as a structured ElementTree, never a concatenated string: the
    # Dublin-Core fields plus the derived `pdfaid` part/conformance make the metadata STATE the PDF/A claim.
    for prefix, uri in _NS.items():
        register_namespace(prefix, uri)
    rdf = Element(QName(_NS["rdf"], "RDF"))
    description = SubElement(rdf, QName(_NS["rdf"], "Description"), {QName(_NS["rdf"], "about"): ""})
    for ns, key, value in (
        ("dc", "title", title.sheet_title),
        ("dc", "creator", title.drawn_by),
        ("dc", "description", title.project),
        *(("pdfaid", slot, member) for slot, member in zip(("part", "conformance"), profile.pdfaid or (), strict=False)),
        # ISO 19005-4 identifies by part + rev (conformance only for the e/f subsets), so the A-4 family adds the year slot
        *((("pdfaid", "rev", "2020"),) if profile.pdfaid is not None and profile.pdfaid[0] == "4" else ()),
    ):
        if value:
            SubElement(description, QName(_NS[ns], key)).text = value
    meta = Element(QName(_NS["x"], "xmpmeta"))
    meta.append(rdf)
    return tostring(meta, encoding="unicode")


# --- [EXPORTS] ----------------------------------------------------------------------------
__all__ = [
    "ACCESSIBLE",
    "ARCHIVAL",
    "Composed",
    "ComposedKind",
    "DRAFT",
    "Engine",
    "FieldRow",
    "FigurePlacement",
    "Iso7200Field",
    "KeyPlan",
    "Membership",
    "NorthArrow",
    "Orientation",
    "PlacementPolicy",
    "PRESERVED",
    "PdfA",
    "PdfProfile",
    "PdfUa",
    "Revision",
    "Scale",
    "Sheet",
    "SheetIssue",
    "SheetOp",
    "SheetSet",
    "SheetSetAudit",
    "SheetSize",
    "TitleBlock",
    "TitleBlockAudit",
    "Viewport",
    "ZoneSpec",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
