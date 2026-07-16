# [PY_ARTIFACTS_SHEET]

`Sheet` owns the single-sheet pipeline — placing already-emitted figures into titled, framed, field-bound architectural drawing sheets; `SheetSet` is the peer multi-sheet owner assembling N `SheetEntry` sheets into a numbered, audited, register-ready set. `Sheet` discriminates a closed `SheetOp` `expression.tagged_union` by one total `match` folded once into a `Composed` evidence struct the `of`/`contribute`/`layers` projections share — one typed payload per case, never a `StrEnum` over an erased `dict`; it authors the frame over three PDF engines under one `Standard` profile, places figures, fills and stamps the title block, but re-renders no figure and re-authors no register. `SheetSet` numbers each sheet, folds the ISO 7200 conformance verdict, and PROJECTS the set outward rather than re-authoring — no OSS sheet-set library exists, so the sheet algebra is this owner's composition over the admitted engines.

Frame authors are `reportlab` (the coordinate author of the border, the `_ZONES` ISO 5457 zone grid, the `TitleBlock.cells` grid, and the `Scale.bar` ISO 5455 graphic ruler), `typst`, and `weasyprint` (which owns its full PDF/A + PDF/UA close end to end), each driven by one `Standard` profile resolved through `_STANDARD`, never a per-engine `archival: bool`; `reportlab` carries only the version pin, its PDF/A output-intent/structure-tree close routing to `exchange/conformance#CONFORMANCE`. `pymupdf.show_pdf_page` vector-copies each figure into its `FigurePlacement.target` window — a `Viewport`-fitted scaled model view or the bare cell — bound to a minted `add_ocg` group or a `set_ocmd` membership dictionary, driven to its reader visibility/lock through one `set_layer` write, mirroring `composition/imposition#IMPOSE`; a sheet is one frame plus N OCG-bound placed figures, never a per-figure draw family. `_FAULTS` discriminates each engine raise into its own `BoundaryFault`, matching the imposition tuple one-for-one, with every opened handle `with`-bracketed; the `_composed` fold offloads to a THREAD lane under `RetryClass.OCCT`, forced whole because it returns the `msgspec`-backed `Composed` the subinterpreter lane cannot load the C-extension for. Figures arrive already-emitted from `composition/compose#COMPOSE` and `visualization/chart#EXPORT`, the numbered set from `drawing/regime#REGIME` `SheetId`; it hands the titled sheet to `composition/imposition#IMPOSE`, projects `Sheet.layers` to `export/layered#LAYERED`, and hands the numbered set to `delivery/register#REGISTER` (register-ready tuples) and `visualization/table#TABLE` (a `TablePlan`). Single-sheet receipts thread `core/receipt#RECEIPT`'s named `ArtifactReceipt.Pdf`/`Egress`/`Preview` mints selected by `Composed.kind`; `SheetSet` is a synchronous projection owner exactly as `drawing/regime#REGIME`, minting no receipt, its evidence riding the register and table owners it projects to.

## [01]-[INDEX]

- [01]-[SHEET]: the single-sheet title-block/frame/field-binding owner over the closed `SheetOp` `tagged_union` (`Frame`/`Place`/`Fill`/`Stamp`/`Preview`) folded once into a `Composed` evidence struct, railed `RuntimeRail[ArtifactReceipt]` over `async_boundary(catch=_FAULTS)`, dispatched to the `reportlab`/`typst`/`weasyprint` frame authors under one `Standard` profile and to `pymupdf` for figure placement, OCG binding, title-block fill, preview, and stamp.
- [02]-[SHEET_SET]: the multi-sheet assembly owner over `tuple[SheetEntry, ...]` — numbering each `drawing/regime#REGIME` `SheetId` across the set, auditing ISO 7200 conformance, and projecting the numbered set to `delivery/register#REGISTER` and `visualization/table#TABLE`; a pure synchronous projection owner minting no receipt, exactly as `drawing/regime#REGIME`.

## [02]-[SHEET]

- Owner: `Sheet` discriminates over the closed `SheetOp` `expression.tagged_union`, every arm folded ONCE into the `Composed` struct the `of`/`contribute`/`layers` projections share — no second `match` re-renders and no memo stands in for the one fold. `Engine` binds each frame author through `_FRAME`; `Standard` resolves through the ONE `_STANDARD` profile-to-conformance-token table plus the `_PDFAID` PDF/A part table, never a per-arm archival literal, a boolean `archival` flag, or an `output_intent=None` hardcoded against a profile the variant requires an ICC for. `SheetSize`'s `_SIZES` correspondence spans ARCH/ANSI/JIS-B beyond the `reportlab`/`pymupdf` built-in tables, so this owner carries the full set, projected landscape through `_oriented`. `_ZONES` is the EXACT ISO 5457 Table 2 field-count table — the ISO-A cardinalities load-bearing, non-ISO-A/A5 derived through `ZoneSpec.of` over the 50-mm field, I and O excluded from the letters and A4 fields on the top/right edges only — so a location is cited by its exact standard zone. `TitleBlock`'s `cells(dims)` is the ONE field-rect correspondence the frame author draws labels at AND the `Fill` arm binds values INTO, its `audit()` folding the `_ISO7200` table into a `TitleBlockAudit` and its `revised()` projecting the history as a `visualization/table#TABLE` `TablePlan` rather than hand-drawn rows; `FieldRow`'s `group` zone-key sorts the rows so identity and approval zones stay contiguous. `Viewport` binds an ISO 5455 `ScaleRatio` to a placement window as a `svgelements.Matrix` affine, so a scaled model view is a real affine-placed window rather than a bare keep-proportion fit. `reportlab` draws the frame in `CMYKColor` process-black — press-faithful separations, not the sRGB default a drawing sheet must not ship; `typst` and `weasyprint` own their native PDF/A close, `pymupdf` the placement/OCG/fill/preview/stamp surface; no sheet-set library is admitted, so the sheet algebra is this owner's composition over those engines, never a re-implemented byte emitter.
- Cases: dispatched by one total `match` lowering to the one `Composed` fold — never a per-discipline sheet-builder sibling, a per-engine `_emit` method, or a per-figure draw call. `Frame(size, engine, title, standard, output_intent, landscape)` authors the framed titled sheet, the `Standard` member routing the engine's conformance token and the `OutputIntent` ICC into the engine that owns its PDF/A close (the reportlab author drawing the `_zones` grid, the templated `typst`/`weasyprint` authors carrying the title block alone). `Place(sheet, placements)` vector-copies each figure into its `placement.target()` window, minting an OCG (or a `set_ocmd` membership dictionary over the shared parent groups) and driving one `set_layer` config. `Fill(sheet, title)` binds field VALUES into the EXACT rects their labels were drawn at, reading the emitted page's real `rect` and the SAME `TitleBlock.cells` correspondence through `insert_htmlbox` — a loose `fields=rows` reconstruction that offsets every value past the fixed head fields is the deleted illusory-alignment form. `Stamp(sheet, title, standard, output_intent, attachments)` runs archival hygiene (a metadata-preserving `subset_fonts` + `scrub`) THEN binds `set_metadata`/`set_toc`/`embfile_add`/`set_xml_metadata` with `tobytes(no_new_id=True)` pinning the stable `/ID`. `Preview(sheet, dpi)` rasterizes to PNG keyed by the same `ContentKey`.
- Auto: `_composed(op) -> Composed` is the ONE `_GUARD`-contracted total `match` both `of` and `contribute` read — no second render. `Frame` calls `_FRAME[engine]` and routes the `_STANDARD` token; the `Place` arm mints one shared OCG per unique `membership` group, folds `_draw_one` over the placements (each minting an `add_ocg` leaf and, for a `membership`-bearing placement, a `set_ocmd` dictionary), keeps the rows that minted a real xref, and drives one `_configure_layers` write over the leaves AND the shared groups before `tobytes`, so `Composed.layers` carries `len(minted) + len(groups)`; the `Fill`/`Stamp`/`Preview` arms bind their pymupdf surface. Each opened `pymupdf` document is `with`-bracketed so it closes on each exit — the native-handle mutation the platform-forced seam. Each arm returns `Composed` reading the REAL `Document.page_count`, so the body stays one `match`-shaped path — never an inline `try`/`except` ladder, a memo, or a second `match` re-rendering for the receipt.
- Receipt: each single-sheet op contributes `core/receipt#RECEIPT` off the one `Composed` fold — the `Frame`/`Fill`/`Stamp` arms `ArtifactReceipt.Pdf`, the OCG-bearing `Place` arm `Egress(key, bytes, pages, 0, 0, layers)` carrying the minted-layer count (leaves plus shared groups) on the `overlays` slot, the `Preview` arm `Preview(key, w, h)`; the `Composed.kind` plus `Composed.layers` select the mint once, so the owner adds NO sibling type and NO new case. `SheetSet` contributes no receipt — its numbered set is register evidence the `delivery/register#REGISTER` `Register` case mints and drawing-list evidence the `visualization/table#TABLE` `Table` case mints, a parallel sheet-set rail the deleted form. `contribute` reads the SAME fold and mints the key over `composed.data`, never a per-kind facts `Struct`, a phantom `ArtifactReceipt.of(key, facts)`, or a second render.
- Growth: a new sheet format is one `SheetSize` member plus one `_SIZES` row (its zones one `_ZONES` row or the `ZoneSpec.of` derivation); an orientation is the `Frame` `landscape` axis; a title-block field is one `FieldRow`, a revision one `Revision`, an ISO 7200 mandatory field one `Iso7200Field` member plus one `_ISO7200` predicate row; a frame-authoring engine is one `Engine` member plus one `_FRAME` row; a placement axis (a clipped callout, a scaled `Viewport`, an OCMD hierarchy, a locked layer) is one `FigurePlacement` field on the `target`/`show_pdf_page`/`set_layer` fold; a graphic cell is one `TitleBlock` field; a border-grid change is the `_ZONES` table or `_zones` projection; a receipt shape is one `Artifact` `Composed.kind` arm breaking the `contribute` `match`; an engine raise is one `_FAULTS` type; an archival profile is one `Standard` member plus one `_STANDARD` plus one `_PDFAID` row; a colour intent is the `OutputIntent` ICC threaded into the engine that owns its close; a set-level projection is one `SheetSet` method. Zero new surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import hashlib
import string
from collections import Counter
from collections.abc import Callable, Iterable
from enum import StrEnum
from html import escape
from io import BytesIO
from typing import TYPE_CHECKING, Literal, assert_never
from xml.etree.ElementTree import Element, QName, SubElement, register_namespace, tostring

from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, structs

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.drawing.standard import ScaleRatio, SheetId
from artifacts.export.layered import Layer
from artifacts.visualization.table import StubLoc, TableFormat, TableOp, TablePlan, Theme

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
type FieldCell = tuple["FieldRow", Box]  # one title-block field bound to its derived top-left-origin rect
type OutputIntent = tuple[str, bytes] | None  # (icc-name, sRGB/CMYK ICC profile bytes) the PDF/A output-intent embeds
type Author = Callable[[Dimensions, "SheetSize", "TitleBlock", "Standard", OutputIntent], bytes]


class Engine(StrEnum):
    REPORTLAB = "reportlab"
    TYPST = "typst"
    WEASYPRINT = "weasyprint"


class Standard(StrEnum):  # the closed archival/accessibility profile — a vocabulary member, never an `archival: bool`
    DRAFT = "draft"  # no conformance pin
    ARCHIVAL = "archival"  # PDF/A-3b long-term-preservation profile
    ACCESSIBLE = "accessible"  # PDF/UA-1 tagged-structure profile (forces a `document(title:)`)
    PRESERVED = "preserved"  # PDF/A-3b + PDF/UA-1 archival AND accessible


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


class Artifact(StrEnum):  # the Composed receipt-shape discriminant — vector PDF vs raster preview
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
# the engine raise tuple: `RuntimeError` (pymupdf `FileDataError`/`EmptyFileError`, typst `TypstError`),
# `ValueError` (a reportlab/weasyprint author or `ElementTree` malformed-name raise), `KeyError`
# (`_SIZES`/`_FRAME`/`_STANDARD` miss), `OSError` (an engine font/resource load), `BeartypeCallHintViolation`
# (a malformed `TitleBlock`/`FigurePlacement`/`Box`/`Viewport` the `_GUARD` fold rejects).
_FAULTS: tuple[type[BaseException], ...] = (RuntimeError, ValueError, KeyError, OSError, BeartypeCallHintViolation)

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

_CELL_CSS: str = "span.v{font-family:Helvetica,sans-serif;font-size:8pt;color:#000}"  # the insert_htmlbox Fill cell style
_INDEX_COLS: tuple[str, ...] = ("sheet", "title", "discipline", "revision", "suitability", "date")
_REVISION_COLS: tuple[str, ...] = ("rev", "date", "description", "by")


# --- [MODELS] ---------------------------------------------------------------------------
# the one profile->per-engine conformance-REQUEST row every author reads by field; the reportlab `version`
# pin's PDF/A close is `exchange/conformance#CONFORMANCE`'s, the other fields the typst/weasyprint close.
class StandardRow(Struct, frozen=True):
    version: tuple[int, int] | None  # reportlab `pdfVersion` pin
    standards: tuple[str, ...]  # typst `pdf_standards` tokens
    variant: str | None  # weasyprint `pdf_variant` profile
    tags: bool  # PDF/UA structure-tree + `lang` flag


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
        # the divided graphic-scale ruler at (x, y): each division carries its fill state as a `bool`, the
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
        # the model corners through the matrix via the non-mutating `Point * Matrix` — svgelements
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
        # the ONE field-rect correspondence the frame author draws and the `Fill` arm binds INTO (top-left
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
        # the ISO 7200 mandatory-field conformance fold over the `_ISO7200` predicate table, mirroring
        # document/tagged#StructureAudit: present vs missing over the exact mandatory set (revision excluded).
        audited = tuple((field, probe(self)) for field, probe in _ISO7200.items())
        return TitleBlockAudit(present=tuple(field for field, ok in audited if ok), missing=tuple(field for field, ok in audited if not ok))

    def revised(self, fmt: TableFormat = TableFormat.HTML, theme: Theme = Theme()) -> TablePlan:
        # the revision history as a visualization/table#TABLE publication table; a bare frame projects an empty one.
        rows = [{"rev": r.mark, "date": r.date, "description": r.description, "by": r.by} for r in self.revisions]
        frame = polars.from_dicts(rows) if rows else polars.DataFrame(schema={column: polars.String for column in _REVISION_COLS})
        ops = (
            TableOp.Header("Revisions", subtitle=self.sheet_number or self.sheet_title or None),
            TableOp.Style(((("text", {"weight": "bold"}),)), at=StubLoc.COLUMN_LABELS),
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


class FigurePlacement(Struct, frozen=True):
    figure: bytes
    cell: Box  # the drawing-region rect (bare tuple)
    name: str = "figure"  # the OCG label the Place arm mints and binds this figure to
    page: int = 0
    keep_proportion: bool = True
    rotate: int = 0
    overlay: bool = True  # draw above (True) or below the sheet content
    clip: Box | None = None
    layered: bool = True  # mint+bind an in-PDF OCG so the placed sheet is layer-separable in a reader
    visible: bool = True  # the OCG default-on state the `set_layer` ui config writes
    locked: bool = False  # the reader-side layer-lock hint `set_layer(locked=)` honors
    viewport: Viewport | None = None  # a scale-bound model-space view; overrides cell/clip with the ISO 5455 fitted window
    membership: tuple[str, ...] = ()  # parent OCG group names an OCMD gates this placement over (a nested/radio layer hierarchy)
    policy: Membership = Membership.ANY_ON  # the set_ocmd visibility policy over the membership groups

    def target(self) -> tuple[Box, Box | None]:
        # the (window, clip) the show_pdf_page draw uses: a Viewport binds the ISO 5455 scale so the placed
        # figure is an affine-scaled model view (window = the matrix-derived scaled extent, clip = the model
        # region); a bare placement keeps its cell + optional clip. One resolution site, never a per-arm branch.
        return (self.viewport.window(), self.viewport.model) if self.viewport is not None else (self.cell, self.clip)


class Composed(Struct, frozen=True):  # the one evidence struct of/contribute/layers read — no second render
    data: bytes
    pages: int
    kind: Artifact = Artifact.PDF
    extent: tuple[int, int] = (0, 0)  # the Preview pixmap width/height the raster receipt rides
    layers: int = 0  # the count of OCGs the Place arm minted (placement leaves + shared membership groups), riding the Egress overlays slot


class SheetEntry(Struct, frozen=True):
    # one member of a SheetSet — the drawing/regime#REGIME SheetId number, the sheet's TitleBlock, and the
    # raw ISO 19650 delivery codes delivery/register#REGISTER validates (plain strings, parsed at that boundary).
    sheet_id: SheetId
    title: TitleBlock
    suitability: str = "S2"  # ISO 19650 suitability code (register parses via _Suitability.of)
    revision: str = "P01"  # ISO 19650 revision code (register parses via Revision.of)
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
    frame: tuple[SheetSize, Engine, TitleBlock, Standard, OutputIntent, bool] = case()
    place: tuple[bytes, tuple[FigurePlacement, ...]] = case()
    fill: tuple[bytes, TitleBlock] = case()
    stamp: tuple[bytes, TitleBlock, Standard, OutputIntent, tuple[tuple[str, bytes], ...]] = case()
    preview: tuple[bytes, float] = case()

    @staticmethod
    def Frame(
        size: SheetSize,
        engine: Engine = Engine.REPORTLAB,
        title: TitleBlock = TitleBlock(),
        *,
        standard: Standard = Standard.DRAFT,
        output_intent: OutputIntent = None,
        landscape: bool = False,
    ) -> "SheetOp":
        return SheetOp(frame=(size, engine, title, standard, output_intent, landscape))

    @staticmethod
    def Place(sheet: bytes, placements: FigurePlacement | Iterable[FigurePlacement]) -> "SheetOp":
        return SheetOp(place=(sheet, _tupled(placements)))

    @staticmethod
    def Fill(sheet: bytes, title: TitleBlock = TitleBlock()) -> "SheetOp":
        return SheetOp(fill=(sheet, title))

    @staticmethod
    def Stamp(
        sheet: bytes,
        title: TitleBlock = TitleBlock(),
        *,
        standard: Standard = Standard.DRAFT,
        output_intent: OutputIntent = None,
        attachments: Iterable[tuple[str, bytes]] = (),
    ) -> "SheetOp":
        return SheetOp(stamp=(sheet, title, standard, output_intent, tuple(attachments)))

    @staticmethod
    def Preview(sheet: bytes, dpi: float = 96.0) -> "SheetOp":
        return SheetOp(preview=(sheet, dpi))


# --- [TABLES] ---------------------------------------------------------------------------
_STANDARD: frozendict[Standard, StandardRow] = frozendict({
    Standard.DRAFT: StandardRow(version=None, standards=(), variant=None, tags=False),
    Standard.ARCHIVAL: StandardRow(version=(1, 7), standards=("a-3b",), variant="pdf/a-3b", tags=False),
    Standard.ACCESSIBLE: StandardRow(version=(1, 7), standards=("ua-1",), variant="pdf/ua-1", tags=True),
    Standard.PRESERVED: StandardRow(version=(1, 7), standards=("a-3b", "ua-1"), variant="pdf/a-3b", tags=True),
})

# ISO 5457 Table 2 — Number of fields (Long side numerals, Short side letters). The ISO-A cardinalities are
# EXACT and load-bearing; A5 and the non-ISO-A ANSI/ARCH/JIS-B sizes derive through `ZoneSpec.of` (50 mm field).
_ZONES: frozendict[SheetSize, ZoneSpec] = frozendict({
    SheetSize.A0: ZoneSpec(long_side=24, short_side=16),
    SheetSize.A1: ZoneSpec(long_side=16, short_side=12),
    SheetSize.A2: ZoneSpec(long_side=12, short_side=8),
    SheetSize.A3: ZoneSpec(long_side=8, short_side=6),
    SheetSize.A4: ZoneSpec(long_side=6, short_side=4),
})

# the ISO 7200 mandatory-field predicate table: one row per field mapping to the TitleBlock evidence that
# satisfies it, the single edit site the `audit()` fold derives present/missing from. IDENTIFICATION_NUMBER and
# SHEET_NUMBER both read `sheet_number` because in AEC the sheet number (the SheetId) IS the drawing identifier.
_ISO7200: frozendict[Iso7200Field, Callable[[TitleBlock], bool]] = frozendict({
    Iso7200Field.LEGAL_OWNER: lambda title: bool(title.legal_owner),
    Iso7200Field.IDENTIFICATION_NUMBER: lambda title: bool(title.sheet_number),
    Iso7200Field.DATE_OF_ISSUE: lambda title: bool(title.date),
    Iso7200Field.SHEET_NUMBER: lambda title: bool(title.sheet_number),
    Iso7200Field.TITLE: lambda title: bool(title.sheet_title or title.sheet_number),
    Iso7200Field.APPROVAL_PERSON: lambda title: bool(title.approved_by),
    Iso7200Field.CREATOR: lambda title: bool(title.drawn_by),
    Iso7200Field.DOCUMENT_TYPE: lambda title: bool(title.document_type),
})


# --- [SERVICES] -------------------------------------------------------------------------
class Sheet(Struct, frozen=True):
    op: SheetOp

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: canonical op payload minted PRE-RUN — never a key over the composed sheet bytes;
        # placed-figure parent keys ride ArtifactWork.parents where the caller holds them (DATA edges).
        return ContentIdentity.of(f"sheet-{self.op.tag}", self.op, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the `_composed` fold crosses the THREAD lane; the terminal receipt threads the PRE-RUN key.
        crossed = await async_boundary(f"sheet.{self.op.tag}", self._folded, catch=_FAULTS)
        return crossed

    async def _folded(self) -> ArtifactReceipt:
        rail = await LanePolicy.offload(_composed, self.op, modality=Modality.THREAD, retry=RetryClass.OCCT)
        composed = rail.default_with(_sheet_raise)
        return self._receipt(self._key, composed)

    def _receipt(self, key: ContentKey, composed: "Composed", /) -> ArtifactReceipt:
        match composed.kind:
            case Artifact.PDF if composed.layers:  # the OCG/OCMD-bearing placed sheet rides the layer count on the Egress overlays slot
                return ArtifactReceipt.Egress(key, len(composed.data), composed.pages, 0, 0, composed.layers)
            case Artifact.PDF:
                return ArtifactReceipt.Pdf(key, len(composed.data), composed.pages)
            case Artifact.PREVIEW:
                return ArtifactReceipt.Preview(key, composed.extent[0], composed.extent[1])
            case _:
                assert_never(composed.kind)

    def contribute(self) -> "Iterable[Receipt]":
        # contribute re-enters the deterministic fold off the loop; the slot is the PRE-RUN input key.
        yield from self._receipt(self._key, _composed(self.op)).contribute()

    def layers(self, names: tuple[str, ...] = ()) -> tuple[Layer, ...]:
        return _placed_layers(self.op, names)


class SheetSet(Struct, frozen=True):
    # the multi-sheet projection owner — numbers, audits, and projects outward to delivery/register#REGISTER
    # and visualization/table#TABLE, no receipt; the register imports THIS owner's TitleBlock, so the register
    # projection returns plain tuples and no import cycle forms.
    entries: tuple[SheetEntry, ...]
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
        # the one polars drawing-list frame `scheduled` renders; an empty set yields the typed empty frame so a
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
        # the (container_id, numbered TitleBlock, suitability, revision) tuples delivery/register#REGISTER folds
        # into its OWN `SheetEntry` — a pure projection with NO register import, so no cycle forms.
        return tuple(
            (entry.container_id or sheet_id.compose(), title, entry.suitability, entry.revision)
            for entry, (sheet_id, title) in zip(self.entries, self.numbered(), strict=True)
        )

    def scheduled(self, fmt: TableFormat = TableFormat.HTML, theme: Theme = Theme()) -> TablePlan:
        # the drawing-list publication table projected to visualization/table#TABLE, that owner's render not this one's.
        ops = (
            TableOp.Header("Drawing List", subtitle=f"{self.total} sheets"),
            TableOp.Spanner("Identity", columns=["sheet", "title", "discipline"]),
            TableOp.Spanner("Issue", columns=["revision", "suitability", "date"]),
            TableOp.Style(((("text", {"weight": "bold"}),)), at=StubLoc.COLUMN_LABELS),
        )
        return TablePlan(frame=self.frame(), ops=ops, fmt=fmt, theme=theme)


# --- [OPERATIONS] -----------------------------------------------------------------------
# a malformed `TitleBlock`/`FigurePlacement`/`Box`/`Viewport` inside a well-tagged `SheetOp` raises
# `BeartypeCallHintViolation` from this guarded fold, the `_FAULTS` tuple admits.
_GUARD = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))


def _tupled[T](items: T | Iterable[T], /) -> tuple[T, ...]:
    match items:
        case str() | bytes():  # the named platform-forced seam: str/bytes are themselves Iterable
            return (items,)  # type: ignore[return-value]
        case Iterable() as stream:
            return tuple(stream)
        case lone:
            return (lone,)


def _oriented(dims: Dimensions, landscape: bool, /) -> Dimensions:  # swap to landscape exactly as imposition.Geometry.oriented does
    return (dims[1], dims[0]) if landscape else dims


def _zones(size: SheetSize, dims: Dimensions, /) -> tuple[tuple[float, float, str], ...]:
    # the ISO 5457 grid at the EXACT Table 2 field count: numerals along the LONG edge, letters (I/O excluded)
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


@_GUARD
def _sheet_raise(fault: object) -> "Composed":
    # terminal collapse at the sheet boundary: an offload fault reconstructs the raise _FAULTS folds.
    raise ValueError(str(fault))


def _composed(op: SheetOp) -> Composed:  # the one pure render fold both `of` and `contribute` read
    match op:
        case SheetOp(tag="frame", frame=(size, engine, title, standard, intent, landscape)):
            data = _FRAME[engine](_oriented(_SIZES[size], landscape), size, title, standard, intent)
            with pymupdf.open(stream=data, filetype="pdf") as doc:  # the page count is read off the handle, which then closes
                return Composed(data, pages=doc.page_count)
        case SheetOp(tag="place", place=(sheet, placements)):
            with pymupdf.open(stream=sheet, filetype="pdf") as document:  # the live native handle closes once the placed bytes are serialized
                groups = _mint_groups(document, placements)  # one shared OCG per unique membership group name
                minted = Block.of_seq(_draw_one(document, placement, groups) for placement in placements).choose(_oc_state)
                _configure_layers(document, minted, groups)  # one ui-config write over the minted leaves + shared groups
                return Composed(
                    document.tobytes(garbage=3, deflate=True, no_new_id=True), pages=document.page_count, layers=len(minted) + len(groups)
                )
        case SheetOp(tag="fill", fill=(sheet, title)):
            with pymupdf.open(stream=sheet, filetype="pdf") as document:  # the live native handle closes once the filled bytes are serialized
                page = document[0]  # its real `rect` is the authoritative sheet geometry, oriented as drawn
                for row, rect in title.cells((page.rect.width, page.rect.height)):  # the SAME TitleBlock.cells geometry the frame drew
                    page.insert_htmlbox(pymupdf.Rect(*rect), _cell_html(row), css=_CELL_CSS)  # rich CSS-styled Fill over the flat insert_textbox
                return Composed(document.tobytes(garbage=3, deflate=True, no_new_id=True), pages=document.page_count)
        case SheetOp(tag="stamp", stamp=(sheet, title, standard, intent, attachments)):
            with pymupdf.open(stream=sheet, filetype="pdf") as document:  # the live native handle closes once the stamped bytes are serialized
                if (
                    standard is not Standard.DRAFT
                ):  # archival hygiene BEFORE the metadata write, the metadata/xml/attachment flags OFF so the pass never nukes what this arm then authors
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
                document.set_metadata(title.metadata())
                document.set_toc(title.outline())
                for name, payload in (
                    *attachments,
                    *(((f"{intent[0]}.icc", intent[1]),) if intent is not None else ()),
                ):  # source files + the PDF/A ICC ride as associated files; the ternary is parenthesized so `*` unpacks its result
                    document.embfile_add(name, payload, filename=name, desc=f"{title.discipline} source".strip())
                if (
                    standard is not Standard.DRAFT
                ):  # the archival/accessible XMP packet is gated on the preservation intent, not a per-engine variant column
                    document.set_xml_metadata(_xmp(title, standard))
                return Composed(document.tobytes(garbage=3, deflate=True, no_new_id=True), pages=document.page_count)
        case SheetOp(tag="preview", preview=(sheet, dpi)):
            with pymupdf.open(stream=sheet, filetype="pdf") as document:  # the handle closes once the preview pixmap is rendered off it
                pixmap = document[0].get_pixmap(dpi=int(dpi))
                return Composed(pixmap.tobytes("png"), pages=1, kind=Artifact.PREVIEW, extent=(pixmap.width, pixmap.height))
        case _:
            assert_never(op)


def _mint_groups(document: "Document", placements: tuple[FigurePlacement, ...], /) -> "frozendict[str, int]":
    # ONE shared OCG per unique membership group, its N placements members via OCMD; only a layered placement's
    # membership contributes a group.
    names = tuple(dict.fromkeys(name for placement in placements if placement.layered for name in placement.membership))
    return frozendict({name: document.add_ocg(name, on=True, intent="View", usage="Artwork") for name in names})


def _draw_one(
    document: "Document", placement: FigurePlacement, groups: "frozendict[str, int]", /
) -> tuple[int, bool, bool]:  # (xref, visible, locked); xref 0 if unlayered
    page = document[0]
    leaf = document.add_ocg(placement.name, on=placement.visible, intent="View", usage="Artwork") if placement.layered else 0
    # a `membership`-bearing placement gates through an OCMD over its parent groups; the leaf rides the layer
    # config, the OCMD the draw.
    oc = (
        document.set_ocmd(ocgs=[leaf, *(groups[name] for name in placement.membership)], policy=placement.policy.value)
        if leaf and placement.membership
        else leaf
    )
    window, clip = placement.target()
    with pymupdf.open(
        stream=placement.figure, filetype="pdf"
    ) as docsrc:  # `show_pdf_page` copies the source page at call time, so the figure handle closes immediately after the draw
        page.show_pdf_page(
            pymupdf.Rect(*window),
            docsrc,
            pno=placement.page,
            keep_proportion=placement.keep_proportion,
            overlay=placement.overlay,
            rotate=placement.rotate,
            clip=pymupdf.Rect(*clip) if clip is not None else None,
            oc=oc,
        )
    return leaf, placement.visible, placement.locked


def _oc_state(drawn: tuple[int, bool, bool], /) -> Option[tuple[int, bool, bool]]:  # keep only the rows that minted a real OCG leaf xref
    return Some(drawn) if drawn[0] else Nothing


def _configure_layers(document: "Document", minted: Block[tuple[int, bool, bool]], groups: "frozendict[str, int]", /) -> None:
    # one `set_layer` ui-config write over the minted placement leaves AND the shared membership groups — the
    # reader's panel toggles `on`/`off` and honors `locked`, exactly as export/layered#LAYERED `PdfOcg` does.
    if minted.is_empty() and not groups:
        return
    document.set_layer(
        0,
        on=[xref for xref, visible, _locked in minted if visible] + list(groups.values()),
        off=[xref for xref, visible, _locked in minted if not visible],
        locked=[xref for xref, _visible, locked in minted if locked],
    )


def _cell_html(row: FieldRow) -> str:
    # the Fill value as escaped CSS-styled HTML for `insert_htmlbox`; the dynamic value is neutralized by
    # `html.escape`, never an unescaped markup splice.
    return f'<span class="v">{escape(row.value)}</span>'


def _frame_reportlab(dims: Dimensions, size: SheetSize, title: TitleBlock, standard: Standard, intent: OutputIntent) -> bytes:
    width, height = dims
    sink = BytesIO()
    # reportlab authors only the conformance REQUEST (the `pdfVersion` pin + `lang` tag for PDF/UA); the PDF/A
    # output-intent ICC embed and the structure-tree close are `exchange/conformance#CONFORMANCE`'s.
    profile = _STANDARD[standard]
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
        text.textLine(f"{row.label}: {row.value}")
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


def _frame_typst(dims: Dimensions, _size: SheetSize, title: TitleBlock, standard: Standard, _intent: OutputIntent) -> bytes:
    # typst's PDF/A output-intent is the compiler's own, the ICC not a frame-author input. Interpolate each field
    # value as a typst STRING (`[#"..."]`), never bracketed content, so `#`/`*`/`[`/`@` markup cannot inject.
    width, height = dims
    grid = ", ".join(f'[#"{_escape(row.label)}"], [#"{_escape(row.value)}"]' for row in title.grid())
    source = (
        f'#set document(title: "{_escape(title.sheet_title or title.sheet_number)}", author: "{_escape(title.drawn_by)}")\n'
        f"#set page(width: {width}pt, height: {height}pt, margin: {_MARGIN}pt)\n"
        "#rect(width: 100%, height: 100%, stroke: 1pt)\n"
        "#place(bottom + right, dx: -4pt, dy: -4pt, rect(stroke: 1pt, inset: 6pt, "
        f"grid(columns: 2, gutter: 4pt, {grid})))\n"
    )
    return typst.compile(
        source.encode(), output=None, format="pdf", pdf_standards=_STANDARD[standard].standards, ignore_system_fonts=True, timestamp=0
    )


def _frame_weasyprint(dims: Dimensions, _size: SheetSize, title: TitleBlock, standard: Standard, intent: OutputIntent) -> bytes:
    width, height = dims
    profile = _STANDARD[standard]
    variant, tags = profile.variant, profile.tags
    cells = "".join(f"<tr><th>{escape(r.label)}</th><td>{escape(r.value)}</td></tr>" for r in title.grid())
    # the <head> meta seeds weasyprint's OWN info-dict + variant XMP, so the ENGINE writes the conformant pdfaid
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
        pdf_variant=variant,
        pdf_tags=tags,
        output_intent=BytesIO(intent[1]) if intent is not None else None,
        attachments=[Attachment(file_obj=BytesIO(intent[1]), name=f"{intent[0]}.icc", description="output-intent ICC", relationship="Data")]
        if intent is not None
        else None,
        custom_metadata=True,
        pdf_identifier=_pdf_id(title, standard),
    )


def _pdf_id(title: TitleBlock, standard: Standard, /) -> bytes:
    # a stable 16-byte /ID from the sheet identity so re-framing the same titled sheet yields byte-identical
    # output; a /ID collision across content-distinct sheets is harmless (the content key is over the whole bytes).
    return hashlib.sha256(f"{title.sheet_number}|{title.sheet_title}|{title.project}|{standard}".encode()).digest()[:16]


_FRAME: frozendict[Engine, Author] = frozendict({
    Engine.REPORTLAB: _frame_reportlab,
    Engine.TYPST: _frame_typst,
    Engine.WEASYPRINT: _frame_weasyprint,
})


# --- [BOUNDARIES] -----------------------------------------------------------------------
def _placed_layers(op: SheetOp, names: tuple[str, ...]) -> tuple[Layer, ...]:
    match op:
        case SheetOp(tag="frame", frame=(size, _engine, _title, _standard, _intent, landscape)):
            w, h = _oriented(_SIZES[size], landscape)
            return (Layer(_name(names, 0, "frame"), _composed(op).data, (0.0, 0.0, w, h)),)
        case SheetOp(tag="place", place=(_sheet, placements)):
            return tuple(
                Layer(
                    _name(names, index, placement.name), placement.figure, placement.target()[0], visible=placement.visible, locked=placement.locked
                )
                for index, placement in enumerate(placements)
            )
        case _:
            return ()


def _name(names: tuple[str, ...], index: int, fallback: str) -> str:
    return names[index] if index < len(names) else fallback


def _escape(value: str) -> str:  # typst markup escaping has no stdlib owner; HTML escaping rides `html.escape`
    return value.replace("\\", "\\\\").replace('"', '\\"')


_NS = frozendict({
    "x": "adobe:ns:meta/",
    "rdf": "http://www.w3.org/1999/02/22-rdf-syntax-ns#",
    "dc": "http://purl.org/dc/elements/1.1/",
    "pdfaid": "http://www.aiim.org/pdfa/ns/id/",
})
# the `Standard` -> PDF/A (part, conformance) the `pdfaid` XMP packet declares; `None` for a profile carrying no PDF/A part.
_PDFAID: frozendict[Standard, tuple[str, str] | None] = frozendict({
    Standard.DRAFT: None,
    Standard.ARCHIVAL: ("3", "B"),
    Standard.ACCESSIBLE: None,
    Standard.PRESERVED: ("3", "B"),
})


def _xmp(title: TitleBlock, standard: Standard) -> str:
    # the fixed-schema XMP packet built as a structured ElementTree, never a concatenated string: the
    # Dublin-Core fields plus the `_PDFAID` part/conformance make the metadata STATE the PDF/A claim.
    for prefix, uri in _NS.items():
        register_namespace(prefix, uri)
    rdf = Element(QName(_NS["rdf"], "RDF"))
    description = SubElement(rdf, QName(_NS["rdf"], "Description"), {QName(_NS["rdf"], "about"): ""})
    for ns, key, value in (
        ("dc", "title", title.sheet_title),
        ("dc", "creator", title.drawn_by),
        ("dc", "description", title.project),
        *(("pdfaid", slot, member) for slot, member in zip(("part", "conformance"), _PDFAID[standard] or (), strict=False)),
    ):
        if value:
            SubElement(description, QName(_NS[ns], key)).text = value
    meta = Element(QName(_NS["x"], "xmpmeta"))
    meta.append(rdf)
    return tostring(meta, encoding="unicode")
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
