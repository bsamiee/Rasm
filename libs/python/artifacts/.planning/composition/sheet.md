# [PY_ARTIFACTS_SHEET]

`Sheet` owns the single-sheet pipeline — placing already-emitted figures into titled, framed, field-bound architectural drawing sheets; `SheetSet` is the peer multi-sheet owner assembling N `SheetIssue` sheets into a numbered, audited, register-ready set. `Sheet` discriminates a closed `SheetOp` `expression.tagged_union` by one total `match` folded once into a `Composed` evidence struct — one typed payload per case, never a `StrEnum` over an erased `dict`; the async emission offloads that fold exactly once and returns its `Composed` value, and the sync `folded()` successor lands the same fold on the frozen `composed` field so `layers` reads the landed value rather than re-rendering. It authors the frame over three PDF engines under one `PdfProfile` conformance value, places figures, fills and stamps the title block, but re-renders no figure and re-authors no register. `SheetSet` numbers each sheet, folds the ISO 7200 conformance verdict, and PROJECTS the set outward rather than re-authoring — no OSS sheet-set library exists, so the sheet algebra is this owner's composition over the admitted engines.


## [01]-[INDEX]


## [02]-[SHEET]

- Owner: `Sheet` discriminates over the closed `SheetOp` `expression.tagged_union`, every arm folded ONCE into the `Composed` struct — the async emission returns the one offloaded fold, and the sync `folded()` successor lands it on the `composed` field that `layers` reads, so no projection re-invokes a frame author or the native PDF serializer. `Engine` binds each frame author through `_FRAME`; `PdfProfile` is the ONE conformance declaration — `archival: PdfA | None` spans the full ISO 19005 level matrix (`a-1a`..`a-4e`, the member value the typst token) and `accessible: PdfUa | None` the ISO 14289 axis, every engine projection (`tokens`/`variant`/`version`/`pdfaid`/`tags`/`pinned`) DERIVING from those two fields so a new archival level is one `PdfA` member and zero table edits; the four named profiles `DRAFT`/`ARCHIVAL`/`ACCESSIBLE`/`PRESERVED` are module-level values, never combination-named enum members. WeasyPrint closes one `pdf_variant`, B-level PDF/A or bare `a-4`, so an unsupported level or combined archival-accessible profile refuses at that author. `SheetSize`'s `_SIZES` correspondence spans ARCH/ANSI/JIS-B beyond the `reportlab`/`pymupdf` built-in tables, and `Orientation.of` projects it without a boolean swap knob. `_ZONES` is the EXACT ISO 5457 Table 2 field-count table — the ISO-A cardinalities load-bearing, non-ISO-A/A5 derived through `ZoneSpec.of` over the 50-mm field, I and O excluded from the letters and A4 fields on the top/right edges only — so a location is cited by its exact standard zone. `TitleBlock`'s `cells(dims)` is the ONE field-rect correspondence the frame author draws labels at AND the `Fill` arm binds values INTO, its `audit()` folding the `_ISO7200` table into a `TitleBlockAudit` and its `revised()` projecting the history as a `visualization/table#TABLE` `TablePlan` rather than hand-drawn rows; `FieldRow`'s `group` zone-key sorts the rows so identity and approval zones stay contiguous. `Viewport` binds an ISO 5455 `ScaleRatio` to a placement window as a `svgelements.Matrix` affine, and `FigurePlacement.target` discriminates that model view from a `(cell, clip)` payload by input shape. `PlacementPolicy` carries every `show_pdf_page` and OCG behavior axis as one value. `FigurePlacement.arranged` composes `composition/compose#COMPOSE`'s Cassowary `arranged` solve over the figures' PDF page extents, so a rule-declared sheet layout replaces hand-computed cells. `reportlab` draws the frame in `CMYKColor` process-black — press-faithful separations, not the sRGB default a drawing sheet must not ship; `typst` and `weasyprint` own their native PDF/A close, `pymupdf` the placement/OCG/fill/preview/stamp surface; no sheet-set library is admitted, so the sheet algebra is this owner's composition over those engines, never a re-implemented byte emitter. `SheetSet` is the peer set-level owner in this same cluster — `tuple[SheetIssue, ...]` folded by `audited()` into a `SheetSetAudit` over the SAME `_ISO7200` predicate table the single-sheet `TitleBlock.audit()` reads, adding duplicate-number detection across the set; it shares this cluster because its whole vocabulary is the single-sheet vocabulary, so a second page or a hollow set-only cluster would fork one `Iso7200Field` table across two owners.
- Cases: dispatched by one total `match` lowering to the one `Composed` fold — never a per-discipline sheet-builder sibling, a per-engine `_emit` method, or a per-figure draw call. `Frame(size, engine, title, profile, output_intent, orientation)` authors the framed titled sheet, the `PdfProfile` routing each engine's conformance tokens and the `OutputIntent` ICC into the engine that owns its PDF/A close. `Place(sheet, placements)` vector-copies each figure into its `placement.resolved()` window, minting an OCG or `set_ocmd` membership dictionary under one `PlacementPolicy` and driving one `set_layer` config. `Fill(sheet, title)` binds field VALUES into the EXACT rects their labels were drawn at, reading the emitted page's real `rect` and the SAME `TitleBlock.cells` correspondence through `insert_htmlbox` — a loose `fields=rows` reconstruction that offsets every value past the fixed head fields is the deleted illusory-alignment form. `Stamp(sheet, title, profile, output_intent, attachments)` runs archival hygiene (a metadata-preserving `subset_fonts` + `scrub`, plus the explicit per-page `set_trimbox` pin press tools read) THEN binds `set_metadata`/`set_toc`/`embfile_add`/`set_xml_metadata` with `tobytes(no_new_id=True)` pinning the stable `/ID`. `Preview(sheet, dpi)` rasterizes to PNG keyed by the same `ContentKey`.
- Auto: `_composed(op) -> Composed` is the ONE `_GUARD`-contracted total `match`, executed once per path — offloaded by the async emission, landed on the successor by the sync `folded()` — never re-entered per projection. `Frame` calls `_FRAME[engine]` and routes the profile; the `Place` arm mints one shared OCG per unique `membership` group, folds `_draw_one` over the placements (each minting an `add_ocg` leaf and, for a `membership`-bearing placement, a `set_ocmd` dictionary), keeps the rows that minted a real xref, and drives one `_configure_layers` write over the leaves AND the shared groups before `tobytes`, so `Composed.layers` carries `len(minted) + len(groups)`. `Frame` and `Place` carry their editable `Layer` rows in `Composed.layer_rows` beside those bytes; `Sheet.layers` only renames that evidence. `Fill`/`Stamp`/`Preview` bind their `pymupdf` surface. Each opened `pymupdf` document is `with`-bracketed so it closes on each exit — the native-handle mutation the platform-forced seam. Each arm returns `Composed` reading the REAL `Document.page_count`, so the body stays one `match`-shaped path — never an inline `try`/`except` ladder, a memo, or a second `match` re-rendering for the output.
- Growth: a new sheet format is one `SheetSize` member plus one `_SIZES` row (its zones one `_ZONES` row or the `ZoneSpec.of` derivation); an orientation is one `Orientation` member; a title-block field is one `FieldRow`, a revision one `Revision`, an ISO 7200 mandatory field one `Iso7200Field` member plus one `_ISO7200` predicate row; a frame-authoring engine is one `Engine` member plus one `_FRAME` row; a placement target is a `FigurePlacement.target` input shape and a layer behavior axis one `PlacementPolicy` field; a constraint-solved layout is one `Rule` handed to `FigurePlacement.arranged`; a graphic cell is one `TitleBlock` field; a border-grid change is the `_ZONES` table or `_zones` projection; an engine raise is one `_FAULTS` type; an archival level is one `PdfA` member (its typst token the member value, its XMP pdfaid derived from the spelling), an accessibility level one `PdfUa` member; a colour intent is the `OutputIntent` ICC threaded into the engine that owns its close; a set-level projection is one `SheetSet` method. Zero new surface.

```python
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

from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, msgpack, structs

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.metrics import Metrics
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.faults import FAULT_CONF, TRANSIENT, FaultRow, RuntimeRail, async_boundary, rostered

from rasm.artifacts.composition.compose import Rule, arranged
from rasm.artifacts.core.hooks import BYTE_VOLUME, DOMAIN, ArtifactKind, ArtifactsLeg
from rasm.artifacts.core.plan import Admission, ArtifactWork
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

# --- [TYPES] ----------------------------------------------------------------------------
type Box = tuple[float, float, float, float]
type Dimensions = tuple[float, float]
type Quarter = Annotated[int, Is[lambda d: d in (0, 90, 180, 270)]]
type FieldCell = tuple["FieldRow", Box]
type OutputIntent = tuple[str, bytes] | None
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


class PdfUa(StrEnum):
    UA1 = "ua-1"


class SheetSize(StrEnum):
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


class ComposedKind(StrEnum):
    PDF = "pdf"
    PREVIEW = "preview"


class Membership(StrEnum):
    ANY_ON = "AnyOn"
    ALL_ON = "AllOn"
    ANY_OFF = "AnyOff"
    ALL_OFF = "AllOff"


class Iso7200Field(
    StrEnum
):
    LEGAL_OWNER = "legal_owner"
    IDENTIFICATION_NUMBER = "identification_number"
    DATE_OF_ISSUE = "date_of_issue"
    SHEET_NUMBER = "sheet_number"
    TITLE = "title"
    APPROVAL_PERSON = "approval_person"
    CREATOR = "creator"
    DOCUMENT_TYPE = "document_type"


# --- [CONSTANTS] ------------------------------------------------------------------------
_FAULTS: tuple[type[BaseException], ...] = (RuntimeError, ValueError, KeyError, OSError, BeartypeCallHintViolation)

_CANON: Final = msgpack.Encoder(order="deterministic")

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

_BLOCK_W, _BLOCK_H, _MARGIN, _ROW_H = 510.0, 196.0, 28.35, 14.0
_PT_PER_MM: float = 72.0 / 25.4
_ZONE_FIELD_MM: float = 50.0
_GRID_LETTERS: tuple[str, ...] = tuple(letter for letter in string.ascii_uppercase if letter not in "IO")

_WEASY: Final[frozenset[PdfA]] = frozenset({PdfA.A1B, PdfA.A2B, PdfA.A3B, PdfA.A4})
_PDF20: Final[frozenset[PdfA]] = frozenset({PdfA.A4, PdfA.A4E, PdfA.A4F})
_PDF14: Final[frozenset[PdfA]] = frozenset({PdfA.A1A, PdfA.A1B})

_CELL_CSS: str = "span.v{font-family:Helvetica,sans-serif;font-size:8pt;color:#000}"
_INDEX_COLS: tuple[str, ...] = ("sheet", "title", "discipline", "revision", "suitability", "date")
_REVISION_COLS: tuple[str, ...] = ("rev", "date", "description", "by")


# --- [MODELS] ---------------------------------------------------------------------------
class PdfProfile(Struct, frozen=True):
    archival: PdfA | None = None
    accessible: PdfUa | None = None

    @property
    def pinned(self) -> bool:
        return self.archival is not None or self.accessible is not None

    @property
    def tags(self) -> bool:
        return self.accessible is not None or (self.archival is not None and self.archival.value.endswith("a"))

    @property
    def tokens(self) -> tuple[str, ...]:
        return (*((self.archival.value,) if self.archival is not None else ()), *((self.accessible.value,) if self.accessible is not None else ()))

    @property
    def variant(self) -> str | None:
        if self.archival is not None:
            return f"pdf/{self.archival.value}" if self.archival in _WEASY else None
        return f"pdf/{self.accessible.value}" if self.accessible is not None else None

    @property
    def version(self) -> tuple[int, int] | None:
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
    def pdfaid(self) -> tuple[str, str] | None:
        return (self.archival.value[2], self.archival.value[3:].upper()) if self.archival is not None else None


DRAFT: Final[PdfProfile] = PdfProfile()
ARCHIVAL: Final[PdfProfile] = PdfProfile(archival=PdfA.A3B)
ACCESSIBLE: Final[PdfProfile] = PdfProfile(accessible=PdfUa.UA1)
PRESERVED: Final[PdfProfile] = PdfProfile(archival=PdfA.A3B, accessible=PdfUa.UA1)


class ZoneSpec(Struct, frozen=True):
    long_side: int
    short_side: int

    @staticmethod
    def of(dims: Dimensions) -> "ZoneSpec":
        long_pt, short_pt = max(dims), min(dims)
        return ZoneSpec(
            long_side=max(round(long_pt / _PT_PER_MM / _ZONE_FIELD_MM), 1), short_side=max(round(short_pt / _PT_PER_MM / _ZONE_FIELD_MM), 1)
        )


class FieldRow(Struct, frozen=True):
    label: str
    value: str = ""
    span: int = 1
    group: str = "general"


class Revision(Struct, frozen=True):
    mark: str
    date: str = ""
    description: str = ""
    by: str = ""


class Scale(Struct, frozen=True):
    ratio: ScaleRatio = ScaleRatio.FULL
    bar_length: float = 0.0
    segments: int = 4
    units: str = "mm"

    @property
    def printed(self) -> str:
        return self.ratio.ratio

    def bar(self, x: float, y: float, /) -> tuple[tuple[Box, bool | str], ...]:
        if self.bar_length <= 0.0 or self.segments <= 0:
            return ()
        step, height = self.bar_length / self.segments, _ROW_H * 0.4
        real = (self.bar_length / _PT_PER_MM) / self.ratio.factor
        divisions = tuple(((x + i * step, y, x + (i + 1) * step, y + height), bool(i % 2)) for i in range(self.segments))
        return (*divisions, ((x, y + height, x + self.bar_length, y + height + _ROW_H), f"0 — {real:g} {self.units} ({self.printed})"))


class NorthArrow(Struct, frozen=True):
    bearing: float = 0.0
    glyph: bytes = b""


class KeyPlan(Struct, frozen=True):
    figure: bytes = b""
    highlight: str = ""


class Viewport(Struct, frozen=True):
    scale: ScaleRatio
    model: Box
    anchor: tuple[float, float] = (0.0, 0.0)

    def matrix(self) -> "Matrix":
        factor = self.scale.factor
        return Matrix(factor, 0.0, 0.0, factor, self.anchor[0] - self.model[0] * factor, self.anchor[1] - self.model[1] * factor)

    def window(self) -> Box:
        transform = self.matrix()
        lo, hi = Point(self.model[0], self.model[1]) * transform, Point(self.model[2], self.model[3]) * transform
        return (min(lo.x, hi.x), min(lo.y, hi.y), max(lo.x, hi.x), max(lo.y, hi.y))


class TitleBlock(Struct, frozen=True):
    project: str = ""
    legal_owner: str = ""
    sheet_number: str = ""
    sheet_total: str = ""
    sheet_title: str = ""
    discipline: str = ""
    document_type: str = "drawing"
    status: str = ""
    date: str = ""
    drawn_by: str = ""
    checked_by: str = ""
    approved_by: str = ""
    scale: Scale = Scale()
    fields: tuple[FieldRow, ...] = ()
    revisions: tuple[Revision, ...] = ()
    north: NorthArrow = NorthArrow()
    key_plan: KeyPlan = KeyPlan()

    def grid(self) -> Block[FieldRow]:
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

    def outline(self) -> list[list[int | str]]:
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
        audited = tuple((field, probe(self)) for field, probe in _ISO7200.items())
        return TitleBlockAudit(present=tuple(field for field, ok in audited if ok), missing=tuple(field for field, ok in audited if not ok))

    def revised(self, fmt: TableFormat = TableFormat.HTML, theme: Theme = Theme()) -> TablePlan:
        rows = [{"rev": r.mark, "date": r.date, "description": r.description, "by": r.by} for r in self.revisions]
        frame = polars.from_dicts(rows) if rows else polars.DataFrame(schema={column: polars.String for column in _REVISION_COLS})
        ops = (
            TableOp.Header("Revisions", subtitle=self.sheet_number or self.sheet_title or None),
            TableOp.Style((("text", {"weight": "bold"}),), at=StubLoc.COLUMN_LABELS),
        )
        return TablePlan(frame=frame, ops=ops, fmt=fmt, theme=theme)


class TitleBlockAudit(Struct, frozen=True, gc=False):
    present: tuple[Iso7200Field, ...]
    missing: tuple[Iso7200Field, ...]

    @property
    def conformant(self) -> bool:
        return not self.missing

    @property
    def coverage(self) -> float:
        total = len(self.present) + len(self.missing)
        return 1.0 if total == 0 else len(self.present) / total

    def facts(self) -> dict[str, object]:
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
    name: str = "figure"
    page: int = 0
    rotate: Quarter = 0
    policy: PlacementPolicy = PlacementPolicy()

    def __post_init__(self) -> None:
        _admit_rotation(self.rotate)

    def resolved(self) -> tuple[Box, Box | None]:
        match self.target:
            case Viewport() as viewport:
                return viewport.window(), viewport.model
            case (cell, clip):
                return cell, clip

    @staticmethod
    def arranged(figures: tuple[bytes, ...], region: Box, rules: tuple[Rule, ...], /, *, name: str = "figure") -> tuple["FigurePlacement", ...]:
        extents = tuple(_figure_extent(figure) for figure in figures)
        return tuple(
            FigurePlacement(figure=figure, target=(box, None), name=f"{name}-{index}", policy=PlacementPolicy())
            for index, (figure, box) in enumerate(zip(figures, arranged(extents, region, rules), strict=True))
        )


class Composed(Struct, frozen=True):
    data: bytes
    pages: int
    kind: ComposedKind = ComposedKind.PDF
    extent: tuple[int, int] = (0, 0)
    layers: int = 0
    scores: frozendict[str, float | str] = frozendict()
    layer_rows: tuple[Layer, ...] = ()


class SheetIssue(Struct, frozen=True):
    sheet_id: SheetId
    title: TitleBlock
    suitability: str = "S2"
    revision: str = "P01"
    container_id: str = ""


class SheetSetAudit(Struct, frozen=True, gc=False):
    sheets: int
    conformant_sheets: int
    duplicate_numbers: tuple[str, ...]
    missing_fields: tuple[Iso7200Field, ...]
    per_sheet: tuple[TitleBlockAudit, ...]

    @property
    def conformant(self) -> bool:
        return self.conformant_sheets == self.sheets and not self.duplicate_numbers


@tagged_union(frozen=True)
class SheetOp:
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

SHEET_FOLD: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.SHEET, point="fold", arm="boundary", defect="sheet-fold", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([SHEET_FOLD]))

_ZONES: frozendict[SheetSize, ZoneSpec] = frozendict({
    SheetSize.A0: ZoneSpec(long_side=24, short_side=16),
    SheetSize.A1: ZoneSpec(long_side=16, short_side=12),
    SheetSize.A2: ZoneSpec(long_side=12, short_side=8),
    SheetSize.A3: ZoneSpec(long_side=8, short_side=6),
    SheetSize.A4: ZoneSpec(long_side=6, short_side=4),
})

_ISO7200: frozendict[Iso7200Field, Callable[[TitleBlock], bool]] = frozendict({
    Iso7200Field.LEGAL_OWNER: lambda title: bool(title.legal_owner),
    Iso7200Field.IDENTIFICATION_NUMBER: lambda title: bool(title.sheet_number),
    Iso7200Field.DATE_OF_ISSUE: lambda title: bool(title.date),
    Iso7200Field.SHEET_NUMBER: lambda title: bool(title.sheet_number),
    Iso7200Field.TITLE: lambda title: bool(title.sheet_title),
    Iso7200Field.APPROVAL_PERSON: lambda title: bool(title.approved_by),
    Iso7200Field.CREATOR: lambda title: bool(title.drawn_by),
    Iso7200Field.DOCUMENT_TYPE: lambda title: bool(title.document_type),
})


# --- [SERVICES] -------------------------------------------------------------------------
class Sheet(Struct, frozen=True):
    op: SheetOp
    lane: LanePolicy
    composed: Option[Composed] = Nothing

    def emit(self, /) -> ArtifactWork[Composed]:
        key = self._key
        return ArtifactWork(key=key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key(f"sheet-{self.op.tag}", _CANON.encode(self.op))

    def folded(self) -> Self:
        return structs.replace(self, composed=Some(_composed(self.op)))

    async def _emit(self) -> RuntimeRail[Composed]:
        match await async_boundary(SHEET_FOLD, self._folded, catch=_FAULTS):
            case Result(tag="ok", ok=composed):
                kind: ArtifactKind = "preview" if composed.kind is ComposedKind.PREVIEW else "document"
                Metrics.record({BYTE_VOLUME: float(len(composed.data))}, domain=DOMAIN, kind=kind, scope=self.lane.scope)
                return Ok(composed)
            case refused:
                return Error(refused.error)

    async def _folded(self) -> Composed:
        rail = await self.lane.offload(Kernel.of(_composed, KernelTrait.HOSTILE), self.op)
        return rail.default_with(_sheet_raise)

    def layers(self, names: tuple[str, ...] = ()) -> tuple[Layer, ...]:
        return self.composed.map(lambda live: Layer.renamed(live.layer_rows, names)).default_value(())


class SheetSet(Struct, frozen=True):
    entries: tuple[SheetIssue, ...]
    sheets: tuple[Sheet, ...] = ()

    def emit(self, /) -> "Iterable[ArtifactWork]":
        return tuple(sheet.emit() for sheet in self.sheets)

    @property
    def total(self) -> int:
        return len(self.entries)

    def numbered(self) -> tuple[tuple[SheetId, TitleBlock], ...]:
        total = str(self.total)
        return tuple(
            (entry.sheet_id, structs.replace(entry.title, sheet_number=entry.sheet_id.compose(), sheet_total=total)) for entry in self.entries
        )

    def audited(self) -> SheetSetAudit:
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
        return tuple(
            (entry.container_id or sheet_id.compose(), title, entry.suitability, entry.revision)
            for entry, (sheet_id, title) in zip(self.entries, self.numbered(), strict=True)
        )

    def scheduled(self, fmt: TableFormat = TableFormat.HTML, theme: Theme = Theme()) -> TablePlan:
        ops = (
            TableOp.Header("Drawing List", subtitle=f"{self.total} sheets"),
            TableOp.Spanner("Identity", columns=["sheet", "title", "discipline"]),
            TableOp.Spanner("Issue", columns=["revision", "suitability", "date"]),
            TableOp.Style((("text", {"weight": "bold"}),), at=StubLoc.COLUMN_LABELS),
        )
        return TablePlan(frame=self.frame(), ops=ops, fmt=fmt, theme=theme)


# --- [OPERATIONS] -----------------------------------------------------------------------
_GUARD = beartype(conf=FAULT_CONF)


@_GUARD
def _admit_rotation(rotate: Quarter, /) -> None:
    return None


def _zones(size: SheetSize, dims: Dimensions, /) -> tuple[tuple[float, float, str], ...]:
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
    raise ValueError(str(fault))


def _figure_extent(figure: bytes) -> tuple[float, float]:
    with pymupdf.open(stream=figure, filetype="pdf") as doc:
        if not doc.page_count:
            raise ValueError("figure PDF carries zero pages")
        rect = doc[0].rect
        return (rect.width, rect.height)


@_GUARD
def _composed(op: SheetOp) -> Composed:
    match op:
        case SheetOp(tag="frame", frame=(size, engine, title, profile, intent, orientation)):
            dims = orientation.of(_SIZES[size])
            data = _FRAME[engine](dims, size, title, profile, intent)
            with pymupdf.open(stream=data, filetype="pdf") as doc:
                return Composed(data, pages=doc.page_count, layer_rows=(Layer("frame", data, (0.0, 0.0, *dims)),))
        case SheetOp(tag="place", place=(sheet, placements)):
            with pymupdf.open(stream=sheet, filetype="pdf") as document:
                groups = _mint_groups(document, placements)
                minted = Block.of_seq(_draw_one(document, placement, groups) for placement in placements).choose(
                    lambda drawn: Some(drawn) if drawn[0] else Nothing
                )
                _configure_layers(document, minted, groups)
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
                        if placement.policy.layered
                    ),
                )
        case SheetOp(tag="fill", fill=(sheet, title)):
            with pymupdf.open(stream=sheet, filetype="pdf") as document:
                if not document.page_count:
                    raise ValueError("sheet PDF carries zero pages")
                page = document[0]
                for row, rect in title.cells((page.rect.width, page.rect.height)):
                    page.insert_htmlbox(pymupdf.Rect(*rect), _cell_html(row), css=_CELL_CSS)
                return Composed(document.tobytes(garbage=3, deflate=True, no_new_id=True), pages=document.page_count)
        case SheetOp(tag="stamp", stamp=(sheet, title, profile, intent, attachments)):
            with pymupdf.open(stream=sheet, filetype="pdf") as document:
                if profile.pinned:
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
                ):
                    document.embfile_add(name, payload, filename=name, desc=f"{title.discipline} source".strip())
                if profile.pinned:
                    document.set_xml_metadata(_xmp(title, profile))
                return Composed(document.tobytes(garbage=3, deflate=True, no_new_id=True), pages=document.page_count)
        case SheetOp(tag="preview", preview=(sheet, dpi)):
            with pymupdf.open(stream=sheet, filetype="pdf") as document:
                if not document.page_count:
                    raise ValueError("sheet PDF carries zero pages")
                pixmap = document[0].get_pixmap(dpi=int(dpi))
                return Composed(pixmap.tobytes("png"), pages=1, kind=ComposedKind.PREVIEW, extent=(pixmap.width, pixmap.height))
        case _:
            assert_never(op)


def _mint_groups(document: "Document", placements: tuple[FigurePlacement, ...], /) -> "frozendict[str, int]":
    names = tuple(dict.fromkeys(name for placement in placements if placement.policy.layered for name in placement.policy.groups))
    return frozendict({name: document.add_ocg(name, on=True, intent="View", usage="Artwork") for name in names})


def _draw_one(
    document: "Document", placement: FigurePlacement, groups: "frozendict[str, int]", /
) -> tuple[int, bool, bool]:
    page = document[0]
    policy = placement.policy
    leaf = document.add_ocg(placement.name, on=policy.visible, intent="View", usage="Artwork") if policy.layered else 0
    oc = (
        document.set_ocmd(ocgs=[leaf, *(groups[name] for name in policy.groups)], policy=policy.membership.value)
        if leaf and policy.groups
        else leaf
    )
    window, clip = placement.resolved()
    with pymupdf.open(
        stream=placement.figure, filetype="pdf"
    ) as docsrc:
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
    if minted.is_empty() and not groups:
        return
    document.set_layer(
        -1,
        on=[xref for xref, visible, _locked in minted if visible] + list(groups.values()),
        off=[xref for xref, visible, _locked in minted if not visible],
        locked=[xref for xref, _visible, locked in minted if locked],
    )


def _cell_html(row: FieldRow) -> str:
    return f'<span class="v">{escape(row.value)}</span>'


def _frame_reportlab(dims: Dimensions, size: SheetSize, title: TitleBlock, profile: PdfProfile, intent: OutputIntent) -> bytes:
    width, height = dims
    sink = BytesIO()
    canvas = Canvas(sink, pagesize=dims, pdfVersion=profile.version, lang="en" if profile.tags else None)
    meta = title.metadata()
    canvas.setTitle(meta["title"])
    canvas.setAuthor(meta["author"])
    canvas.setSubject(meta["subject"])
    canvas.setKeywords(meta["keywords"])
    canvas.setCreator(meta["creator"])
    ink = CMYKColor(0.0, 0.0, 0.0, 1.0)
    canvas.setStrokeColor(ink)
    canvas.setFillColor(ink)
    border = canvas.beginPath()
    border.rect(_MARGIN, _MARGIN, width - 2 * _MARGIN, height - 2 * _MARGIN)
    canvas.setLineWidth(1.0)
    canvas.drawPath(border, stroke=1, fill=0)
    canvas.rect(width - _MARGIN - _BLOCK_W, _MARGIN, _BLOCK_W, _BLOCK_H)
    canvas.setFont("Helvetica", 7)
    for zx, zy, mark in _zones(size, dims):
        canvas.drawCentredString(zx, zy, mark)
    for row, (rx, top, _x1, _y1) in title.cells(dims):
        text = canvas.beginText(rx, height - top - _ROW_H + 3.0)
        text.setFont("Helvetica-Bold", 8)
        text.textLine(f"{row.label}:")
        canvas.drawText(text)
    if title.north.glyph:
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
        if title.key_plan.highlight:
            canvas.drawString(kx, ky - _ROW_H + 3.0, title.key_plan.highlight)
    for shape, mark in title.scale.bar(width - _MARGIN - _BLOCK_W, _MARGIN + _BLOCK_H + _ROW_H):
        match mark:
            case bool():
                canvas.rect(shape[0], shape[1], shape[2] - shape[0], shape[3] - shape[1], stroke=1, fill=int(mark))
            case str():
                canvas.drawString(shape[0], shape[1], mark)
    outline_key = title.sheet_number or "sheet"
    canvas.bookmarkPage(outline_key)
    canvas.addOutlineEntry(meta["title"], outline_key, level=0)
    canvas.showPage()
    canvas.save()
    return sink.getvalue()


def _frame_typst(dims: Dimensions, _size: SheetSize, title: TitleBlock, profile: PdfProfile, _intent: OutputIntent) -> bytes:
    width, height = dims
    grid = ", ".join(f'[#"{_escape(row.label)}"], []' for row in title.grid())
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
        raise ValueError(f"weasyprint closes no {profile.archival.value} variant")
    width, height = dims
    cells = "".join(f"<tr><th>{escape(r.label)}</th><td></td></tr>" for r in title.grid())
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
    return hashlib.sha256(msgpack.encode((Engine.WEASYPRINT, dims, title, profile, intent))).digest()[:16]


_FRAME: frozendict[Engine, Author] = frozendict({
    Engine.REPORTLAB: _frame_reportlab,
    Engine.TYPST: _frame_typst,
    Engine.WEASYPRINT: _frame_weasyprint,
})


# --- [BOUNDARIES] -----------------------------------------------------------------------
def _escape(value: str) -> str:
    return value.replace("\\", "\\\\").replace('"', '\\"')


_NS = frozendict({
    "x": "adobe:ns:meta/",
    "rdf": "http://www.w3.org/1999/02/22-rdf-syntax-ns#",
    "dc": "http://purl.org/dc/elements/1.1/",
    "pdfaid": "http://www.aiim.org/pdfa/ns/id/",
})


def _xmp(title: TitleBlock, profile: PdfProfile) -> str:
    for prefix, uri in _NS.items():
        register_namespace(prefix, uri)
    rdf = Element(QName(_NS["rdf"], "RDF"))
    description = SubElement(rdf, QName(_NS["rdf"], "Description"), {QName(_NS["rdf"], "about"): ""})
    for ns, key, value in (
        ("dc", "title", title.sheet_title),
        ("dc", "creator", title.drawn_by),
        ("dc", "description", title.project),
        *(("pdfaid", slot, member) for slot, member in zip(("part", "conformance"), profile.pdfaid or (), strict=False)),
        *((("pdfaid", "rev", "2020"),) if profile.pdfaid is not None and profile.pdfaid[0] == "4" else ()),
    ):
        if value:
            SubElement(description, QName(_NS[ns], key)).text = value
    meta = Element(QName(_NS["x"], "xmpmeta"))
    meta.append(rdf)
    return tostring(meta, encoding="unicode")


# --- [EXPORTS] --------------------------------------------------------------------------
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

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
