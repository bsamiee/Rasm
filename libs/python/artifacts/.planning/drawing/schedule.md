# [PY_ARTIFACTS_DRAWING_SCHEDULE]

AEC scheduling lowers settled quantity and schedule frames into standards-formatted publication tables and lowers drafting vocabularies into legends. `ScheduleContent.tabular` owns a settled `polars.DataFrame` plus its `ScheduleKind`; `regime_legend` owns a line or hatch `LegendKind`; `discipline_legend` owns the `Standard` needed for discipline colors; `authored_legend` owns an authored legend kind plus nonempty `LegendEntry` rows. `Schedule.of` discriminates these payload shapes at one ingress, derives `TableOp` values from template data, builds bytes through `TablePlan.build`, and mints `ArtifactReceipt.Schedule`.

AEC schedule vocabulary lives in `frozendict` template rows with domain-scale column cardinality. `Schedule` composes `visualization/table#TABLE` for rendering, `drawing/regime#REGIME` for ISO legend codes and `HATCH_BIND`, `drawing/standard#STANDARD` for discipline colors, `graphic/color/derive#DERIVE` for palette ramps, `polars` for settled-frame shaping, and `drawsvg` for swatches. `LanePolicy` offloads the synchronous fold; `RuntimeRail` and `async_boundary` carry failures; `built()` supplies table bytes to `composition/compose#COMPOSE`; `ArtifactReceipt.Schedule` and `ArtifactWork` supply evidence and scheduling.

## [01]-[INDEX]

- [02]-[SCHEDULE]: the `Schedule` owner over tabular, regime-legend, discipline-legend, and authored-legend `ScheduleContent` cases lowering into `visualization/table#TABLE`.

## [02]-[SCHEDULE]

- Owner: `Schedule` holds the closed `ScheduleContent`, `Palette`, `LanePolicy`, `TableFormat`, and `Theme`. Mode-specific `Standard` and authored rows live only inside their cases. `TablePlan` owns the styled-table builder; `Schedule` owns AEC template-to-`TableOp` lowering. `ScheduleTemplate` carries the ordered columns, spanners, sorting, data color, totals, BOQ rollup, and column footnotes. `LegendEntry` carries authored code, meaning, and optional swatch.
- Cases: `tabular(frame, kind)` owns settled tabular data, `regime_legend(kind)` owns line or hatch enumeration, `discipline_legend(standard)` owns discipline enumeration and its color standard, and `authored_legend(kind, entries)` owns nonempty authored rows. `_lower` matches all cases. `ScheduleKind.QUANTITY` rolls element rows to classification/description/material/unit/rate groups, sums measures and cost, and preserves differing rates as distinct BOQ rows. `ScheduleKind.REVISION` folds the sheet set's `composition/sheet#SHEET` `Revision` rows into the issue-ordered set-wide log. Derived line, hatch, and discipline legends enumerate their owning regime vocabularies; authored symbol, abbreviation, keynote, material-finish, and general-note legends consume admitted entries.
- Entry: `Schedule.of` is the one ingress over tabular, regime-legend, discipline-legend, and authored-legend payload shapes, admitting under the folder's `INGRESS` conf so a malformed kind or frame refuses as the `ValueError` the `_FAULTS` boundary already admits. `folded()` lands the ONE render on `product` and `_emit`/`built` project receipt and bytes from that landed fact — `TablePlan.build()` executes once per product, never once per projection; `built` reads only the landed successor and refuses typed when unfolded, so a bytes+evidence consumer cannot fork two renders.
- Auto: `_schedule_ops` derives labels, formats, alignment, spanners, colors, totals, footnotes, and notes from `_TEMPLATE`, filtered to present columns. `_schedule_frame` requires the template key, selects owned columns, rolls quantity rows by the full price discriminant, and sorts the result. `pl.col(*present_totals).sum()` fills one grand-total row across present measures. `_derived_legend_rows` and `_discipline_legend_rows` enumerate owned vocabularies, while authored rows lower directly and `drawsvg` builds structured swatches. `_key` includes the kind's canonical `_TEMPLATE` row, frame schema and row hashes, mode payload, palette, format, and theme; mode-dead values never perturb identity.
- Packages: `visualization/table#TABLE` owns styled rendering and the `build` bytes seam; `beartype` guards `Schedule.of` under `drawing/regime#REGIME` `INGRESS`; `polars` owns selection, sorting, aggregation, and row hashing; `PolarsError`, `ValueError`, `KeyError`, `NotImplementedError`, and `OSError` form the narrowed engine catch; drawing regime and standard owners supply ISO codes and colors; `hex_ramp` supplies palettes; `HatchFill` supplies fill cases; `drawsvg` supplies structured swatches. `Schedule` imports neither `ezdxf` nor `great-tables`.
- Receipt: `_render` mints `core/receipt#RECEIPT` `ArtifactReceipt.Schedule` off the one build, and `_emit` awaits `Journal.record` over `receipt.evidence()` at that fold — a published schedule is production trail, so the fact is `OPERATIONAL` and its byte volume charges `STORAGE`. Recording suspends, so the seat is the awaitable `_emit`; `built` reads the SAME landed render for its bytes consumer and records nothing beside it.
- Growth: a new schedule type adds one `ScheduleKind` member and `_TEMPLATE` row; columns, spanners, totals, rollup keys, sorting, footnotes, notes, and colors remain template data. A new regime-derived legend adds one `LegendKind`, `_REGIME_LEGENDS` value, title, and total vocabulary arm; discipline remains the `Standard`-carrying case. A new authored legend adds one kind and title. A new fill regime adds one `HatchFill` case and `_hatch_swatch` arm.
- Boundary: no sheet placement (`composition/sheet#SHEET`), no drawing-symbol geometry (`drawing/symbol#SYMBOL`), no IFC authoring (`dotnet:Rasm.Bim` owns the QTO/schedule rows). `visualization/table#TABLE` owns the render, `drawing/regime#REGIME` and `drawing/standard#STANDARD` the ISO legend codes and pens, `graphic/color/derive#DERIVE` the palette, `polars` the frame shaping, `drawsvg` the swatch primitives, `composition/compose#COMPOSE` the placement, `specification/classify#CODE` the keynote classification codes; identity minting is the runtime's.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import math
from builtins import frozendict
from collections.abc import Iterable
from enum import StrEnum
from typing import Final, Literal, Self, assert_never

import polars as pl
from beartype import beartype
from polars.exceptions import PolarsError
from expression import Error, Nothing, Ok, Option, Result, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, msgpack, structs

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.journal import Journal
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.faults import TERMINAL, TRANSIENT, FaultRow, RuntimeRail, async_boundary, rostered

from rasm.artifacts.core.hooks import ArtifactsLeg
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.drawing.regime import Discipline, HATCH_BIND, HatchMaterial, INGRESS, LayerName, LineType
from rasm.artifacts.drawing.standard import Standard
from rasm.artifacts.graphic.vector.pattern import HatchFill, Motif
from rasm.artifacts.graphic.color.derive import Palette, hex_ramp
from rasm.artifacts.visualization.table import FmtKind, FootnoteMarks, StubLoc, TableFormat, TableOp, TablePlan, Theme

lazy import drawsvg

# --- [TYPES] ----------------------------------------------------------------------------
type Swatch = str
type Align = Literal["left", "center", "right"]
type Footnote = tuple[str, str]


class ScheduleKind(StrEnum):
    DOOR = "door"
    WINDOW = "window"
    ROOM_FINISH = "room-finish"
    WALL_TYPE = "wall-type"
    PARTITION = "partition"
    EQUIPMENT = "equipment"
    PLUMBING_FIXTURE = "plumbing-fixture"
    LIGHTING_FIXTURE = "lighting-fixture"
    FINISH = "finish"
    HARDWARE_SET = "hardware-set"
    PANEL = "panel"
    AIR_TERMINAL = "air-terminal"
    MECHANICAL_EQUIPMENT = "mechanical-equipment"
    ELECTRICAL_EQUIPMENT = "electrical-equipment"
    FIRE_PROTECTION = "fire-protection"
    SIGNAGE = "signage"
    ROOM_DATA = "room-data"
    STRUCTURAL_COLUMN = "structural-column"
    STRUCTURAL_BEAM = "structural-beam"
    FURNITURE = "furniture"
    REVISION = "revision"
    QUANTITY = "quantity"


class LegendKind(StrEnum):
    LINE_TYPE = "line-type"
    HATCH_MATERIAL = "hatch-material"
    DISCIPLINE_LAYER = "discipline-layer"
    SYMBOL = "symbol"
    ABBREVIATION = "abbreviation"
    KEYNOTE = "keynote"
    MATERIAL_FINISH = "material-finish"
    GENERAL_NOTE = "general-note"


_REGIME_LEGENDS: frozenset[LegendKind] = frozenset({LegendKind.LINE_TYPE, LegendKind.HATCH_MATERIAL})
_DERIVED_LEGENDS: frozenset[LegendKind] = _REGIME_LEGENDS | {LegendKind.DISCIPLINE_LAYER}


# --- [CONSTANTS] ------------------------------------------------------------------------
_FAULTS: tuple[type[Exception], ...] = (PolarsError, ValueError, KeyError, NotImplementedError, OSError)
_CANON: Final = msgpack.Encoder(order="deterministic")
_AEC_THEME: Theme = Theme(
    style=1, color="gray", all_caps=True, header_align="center", outline=("solid", "1px", "#333333"), footnote_marks=FootnoteMarks.STANDARD
)



# --- [TABLES] ---------------------------------------------------------------------------

SCHEDULE_UNBUILT: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.SCHEDULE, point="product", arm="config", defect="built-before-folded", retriability=TERMINAL, slots=("content",)
)
SCHEDULE_RENDER: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.SCHEDULE, point="render", arm="boundary", defect="render-fold", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([SCHEDULE_UNBUILT, SCHEDULE_RENDER]))

# --- [MODELS] ---------------------------------------------------------------------------
class ColumnSpec(Struct, frozen=True):
    source: str
    label: str
    fmt: FmtKind | None = None
    unit: str = ""
    align: Align = "left"
    decimals: int = 0

    def __post_init__(self) -> None:
        if not self.source.strip() or not self.label.strip() or self.decimals < 0:
            raise ValueError("column source, label, and decimals are invalid")
        if self.unit and self.fmt not in (FmtKind.INTEGER, FmtKind.NUMBER):
            raise ValueError("units require a numeric format")
        if self.decimals and self.fmt is not FmtKind.NUMBER:
            raise ValueError("decimals require number format")


class SpannerSpec(Struct, frozen=True):
    label: str
    columns: tuple[str, ...]

    def __post_init__(self) -> None:
        if not self.label.strip() or not self.columns or len(self.columns) != len(set(self.columns)):
            raise ValueError("spanner requires a label and unique columns")


class ScheduleTemplate(Struct, frozen=True):
    title: str
    key: str
    columns: tuple[ColumnSpec, ...]
    spanners: tuple[SpannerSpec, ...] = ()
    sort: tuple[str, ...] = ()
    colors: tuple[str, ...] = ()
    totals: tuple[str, ...] = ()
    notes: tuple[str, ...] = ()
    rollup: tuple[str, ...] = ()
    footnotes: tuple[Footnote, ...] = ()

    def __post_init__(self) -> None:
        sources = tuple(column.source for column in self.columns)
        references = (
            *self.totals,
            *self.rollup,
            *self.sort,
            *self.colors,
            *(column for spanner in self.spanners for column in spanner.columns),
            *(column for column, _ in self.footnotes),
        )
        if not self.title.strip() or not self.columns or self.key not in sources or len(sources) != len(set(sources)):
            raise ValueError("schedule template requires a title, unique columns, and a present key")
        if any(reference not in sources for reference in references):
            raise ValueError("schedule template references an unknown column")
        if len(self.sort) != len(set(self.sort)) or len(self.colors) != len(set(self.colors)):
            raise ValueError("schedule sort and color columns must be unique")
        if any(not note.strip() for note in self.notes) or any(not text.strip() for _, text in self.footnotes):
            raise ValueError("schedule notes must not be empty")


class LegendEntry(Struct, frozen=True):
    code: str
    description: str
    swatch: Option[Swatch] = Nothing

    def __post_init__(self) -> None:
        match self.swatch:
            case _ if not self.code.strip() or not self.description.strip():
                raise ValueError("legend entry requires code and description")
            case Option(tag="some", some=swatch) if not swatch.strip():
                raise ValueError("legend swatch must not be empty")
            case _:
                return


# --- [VOCABULARY] -----------------------------------------------------------------------
@tagged_union(frozen=True)
class ScheduleContent:
    tag: Literal["tabular", "regime_legend", "discipline_legend", "authored_legend"] = tag()
    tabular: tuple[pl.DataFrame, ScheduleKind] = case()
    regime_legend: LegendKind = case()
    discipline_legend: Standard = case()
    authored_legend: tuple[LegendKind, tuple[LegendEntry, ...]] = case()


# --- [SERVICES] -------------------------------------------------------------------------
class Schedule(Struct, frozen=True):
    content: ScheduleContent
    palette: Palette
    lane: LanePolicy
    fmt: TableFormat = TableFormat.HTML
    theme: Theme = _AEC_THEME
    product: tuple[bytes, ArtifactReceipt] | None = None

    def __post_init__(self) -> None:
        match self.content:
            case ScheduleContent(tag="tabular", tabular=(frame, kind)) if _TEMPLATE[kind].key not in frame.columns:
                raise ValueError(f"schedule frame omits key column {_TEMPLATE[kind].key!r}")
            case ScheduleContent(tag="tabular", tabular=(frame, kind)) if (
                missing := tuple(key for key in _TEMPLATE[kind].rollup if key not in frame.columns)
            ):
                raise ValueError(f"schedule frame omits rollup columns {missing!r}")
            case ScheduleContent(tag="regime_legend", regime_legend=kind) if kind not in _REGIME_LEGENDS:
                raise ValueError("regime legend kind must enumerate line or hatch vocabulary")
            case ScheduleContent(tag="authored_legend", authored_legend=(kind, entries)) if kind in _DERIVED_LEGENDS or not entries:
                raise ValueError("authored legend requires an authored kind and nonempty entries")
            case _:
                return

    @classmethod
    @beartype(conf=INGRESS)
    def of(
        cls,
        source: tuple[pl.DataFrame, ScheduleKind] | LegendKind | Standard | tuple[LegendKind, Iterable[LegendEntry]],
        palette: Palette,
        lane: LanePolicy,
        /,
        *,
        fmt: TableFormat = TableFormat.HTML,
        theme: Theme = _AEC_THEME,
    ) -> Self:
        match source:
            case (pl.DataFrame() as frame, ScheduleKind() as kind):
                content = ScheduleContent(tabular=(frame, kind))
            case Standard() as standard:
                content = ScheduleContent(discipline_legend=standard)
            case LegendKind() as kind:
                content = ScheduleContent(regime_legend=kind)
            case (LegendKind() as kind, entries):
                content = ScheduleContent(authored_legend=(kind, tuple(entries)))
            case _ as unreachable:
                assert_never(unreachable)
        return cls(content=content, palette=palette, lane=lane, fmt=fmt, theme=theme)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key(f"drawing-schedule-{self.content.tag}", _CANON.encode((self._token(), self.palette, self.fmt, self.theme)))

    def _token(self) -> tuple[object, ...]:
        match self.content:
            case ScheduleContent(tag="tabular", tabular=(frame, kind)):
                template = _TEMPLATE[kind]
                owned = frame.select(tuple(column.source for column in template.columns if column.source in frame.columns))
                return (kind.value, template, tuple((name, str(dtype)) for name, dtype in owned.schema.items()), tuple(owned.hash_rows(seed=0).to_list()))
            case ScheduleContent(tag="regime_legend", regime_legend=kind):
                return (kind.value,)
            case ScheduleContent(tag="discipline_legend", discipline_legend=standard):
                return (LegendKind.DISCIPLINE_LAYER.value, standard)
            case ScheduleContent(tag="authored_legend", authored_legend=(kind, entries)):
                return (kind.value, entries)
            case _ as unreachable:
                assert_never(unreachable)

    async def folded(self) -> RuntimeRail["Schedule"]:
        return (await self._crossed()).map(lambda pair: structs.replace(self, product=pair))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        settled = Ok(self.product[1]) if self.product is not None else (await self._crossed()).map(lambda pair: pair[1])
        match settled:
            case Result(tag="ok", ok=receipt):
                return (await Journal.record(receipt.evidence())).map(lambda _landed: receipt)
            case refused:
                return Error(refused.error)

    async def built(self) -> RuntimeRail[bytes]:
        return (
            Ok(self.product[0])
            if self.product is not None
            else Error(SCHEDULE_UNBUILT.raised(self.content.tag))
        )

    async def _crossed(self) -> RuntimeRail[tuple[bytes, ArtifactReceipt]]:
        crossed = await async_boundary(SCHEDULE_RENDER, lambda: self.lane.offload(Kernel.of(_render, KernelTrait.RELEASING), self), catch=_FAULTS)
        return crossed.bind(lambda rail: rail)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _render(schedule: Schedule) -> tuple[bytes, ArtifactReceipt]:
    frame, ops, kind, rows, columns = _lower(schedule)
    data = TablePlan(frame=frame, ops=ops, fmt=schedule.fmt, theme=schedule.theme).build()
    return data, ArtifactReceipt.Schedule(schedule._key, kind, rows, columns, schedule.fmt.value, len(data))


def _lower(schedule: Schedule) -> tuple[pl.DataFrame, tuple[TableOp, ...], str, int, int]:
    match schedule.content:
        case ScheduleContent(tag="tabular", tabular=(frame, kind)):
            template = _TEMPLATE[kind]
            shaped = _schedule_frame(frame, template)
            ops = _schedule_ops(template, schedule.palette, frozenset(shaped.columns))
            return shaped, ops, kind.value, shaped.height, len(shaped.columns)
        case ScheduleContent(tag="regime_legend", regime_legend=kind):
            frame = _legend_frame(_derived_legend_rows(kind, schedule.palette))
            return frame, _legend_ops(kind), f"legend-{kind.value}", frame.height, len(frame.columns)
        case ScheduleContent(tag="discipline_legend", discipline_legend=standard):
            kind = LegendKind.DISCIPLINE_LAYER
            frame = _legend_frame(_discipline_legend_rows(standard, schedule.palette))
            return frame, _legend_ops(kind), f"legend-{kind.value}", frame.height, len(frame.columns)
        case ScheduleContent(tag="authored_legend", authored_legend=(kind, entries)):
            frame = _legend_frame(tuple((entry.swatch.default_value(""), entry.code, entry.description) for entry in entries))
            return frame, _legend_ops(kind), f"legend-{kind.value}", frame.height, len(frame.columns)
        case _ as unreachable:
            assert_never(unreachable)


def _schedule_frame(frame: pl.DataFrame, template: ScheduleTemplate) -> pl.DataFrame:
    present = tuple(column.source for column in template.columns if column.source in frame.columns)
    if template.key not in present:
        raise ValueError(f"schedule frame omits key column {template.key!r}")
    shaped = frame.select(present)
    missing_rollup = tuple(key for key in template.rollup if key not in shaped.columns)
    if missing_rollup:
        raise ValueError(f"schedule frame omits rollup columns {missing_rollup!r}")
    if template.rollup:
        measures = tuple(pl.col(column).sum() for column in template.totals if column in shaped.columns)
        shaped = shaped.group_by(*template.rollup, maintain_order=True).agg(*measures)
    keys = tuple(key for key in template.sort if key in shaped.columns)
    return shaped.sort(keys) if keys else shaped


def _schedule_ops(template: ScheduleTemplate, palette: Palette, present: frozenset[str]) -> tuple[TableOp, ...]:
    cols = tuple(column for column in template.columns if column.source in present)
    labels = {column.source: (f"{column.label} ({column.unit})" if column.unit else column.label) for column in cols}
    key_label = next((column.label for column in cols if column.source == template.key), template.key)
    stub = (TableOp.Stub(template.key), TableOp.Stubhead(key_label)) if template.key in present else ()
    fmts = tuple(
        TableOp.Fmt(fmt, columns=[column.source], **({"decimals": column.decimals} if fmt is FmtKind.NUMBER else {}))
        for column in cols
        if (fmt := column.fmt) is not None
    )
    aligns = tuple(TableOp.Align(column.align, columns=[column.source]) for column in cols)
    spanners = tuple(
        TableOp.Spanner(spanner.label, columns=[c for c in spanner.columns if c in present])
        for spanner in template.spanners
        if any(c in present for c in spanner.columns)
    )
    colored = tuple(TableOp.Color(columns=[color], palette=list(hex_ramp(palette))) for color in template.colors if color in present)
    present_totals = tuple(column for column in template.totals if column in present)
    grand_fns = {"TOTAL": pl.col(*present_totals).sum()} if present_totals else {"Count": pl.col(template.key).count()}
    summary = (TableOp.GrandSummary(grand_fns, missing_text=""),) if template.key in present else ()
    footnotes = tuple(TableOp.Footnote(text, at=StubLoc.COLUMN_LABELS, columns=[column]) for column, text in template.footnotes if column in present)
    noted = tuple(TableOp.SourceNote(note) for note in template.notes)
    return (
        TableOp.Header(template.title),
        *stub,
        TableOp.Label(labels),
        *fmts,
        *aligns,
        *spanners,
        *colored,
        TableOp.SubMissing(text="—"),
        *summary,
        *footnotes,
        *noted,
    )


def _legend_frame(rows: tuple[tuple[Swatch, str, str], ...], /) -> pl.DataFrame:
    return pl.from_dicts([{"symbol": swatch, "code": code, "description": description} for swatch, code, description in rows])


def _derived_legend_rows(kind: LegendKind, palette: Palette) -> tuple[tuple[Swatch, str, str], ...]:
    ink = hex_ramp(palette)[0]
    match kind:
        case LegendKind.LINE_TYPE:
            return tuple((_dash_swatch(lt.pattern, ink), lt.name, _LINE_MEANING[lt]) for lt in LineType)
        case LegendKind.HATCH_MATERIAL:
            return tuple((_hatch_swatch(HATCH_BIND[material], ink), material.value, _HATCH_MEANING[material]) for material in HatchMaterial)
        case LegendKind.DISCIPLINE_LAYER | LegendKind.SYMBOL | LegendKind.ABBREVIATION | LegendKind.KEYNOTE | LegendKind.MATERIAL_FINISH | LegendKind.GENERAL_NOTE:
            raise ValueError("non-regime legend kind cannot enter regime derivation")
        case _ as unreachable:
            assert_never(unreachable)


def _discipline_legend_rows(standard: Standard, palette: Palette) -> tuple[tuple[Swatch, str, str], ...]:
    ink = hex_ramp(palette)[0]
    return tuple(
        (_color_swatch(standard.rgb(LayerName.of(discipline, "XXXX")), ink), discipline.value, _DISCIPLINE_MEANING[discipline])
        for discipline in Discipline
    )


def _legend_ops(kind: LegendKind) -> tuple[TableOp, ...]:
    return (
        TableOp.Header(_LEGEND_TITLE[kind]),
        TableOp.Label({"symbol": "", "code": "SYMBOL", "description": "DESCRIPTION"}),
        TableOp.Fmt(FmtKind.MARKDOWN, columns=["symbol"]),
        TableOp.Align("center", columns=["symbol", "code"]),
        TableOp.Align("left", columns=["description"]),
        TableOp.Width({"symbol": "84px"}),
    )


def _dash_swatch(pattern: tuple[float, ...], ink: str, /) -> Swatch:
    canvas = drawsvg.Drawing(64, 14, origin=(0, 0))
    dash = " ".join("0.5" if segment == 0.0 else f"{abs(segment):g}" for segment in pattern[1:])
    canvas.append(drawsvg.Line(2, 7, 62, 7, stroke=ink, stroke_width=1.2, **({"stroke_dasharray": dash} if dash else {})))
    return canvas.as_svg()


def _color_swatch(rgb: tuple[int, int, int], ink: str, /) -> Swatch:
    canvas = drawsvg.Drawing(28, 14, origin=(0, 0))
    canvas.append(drawsvg.Rectangle(1, 1, 26, 12, fill=_hex(rgb), stroke=ink, stroke_width=0.6))
    return canvas.as_svg()


def _hatch_swatch(fill: HatchFill, ink: str, /) -> Swatch:
    canvas = drawsvg.Drawing(28, 14, origin=(0, 0))
    match fill:
        case HatchFill(tag="solid", solid=color):
            canvas.append(drawsvg.Rectangle(1, 1, 26, 12, fill=color, stroke=ink, stroke_width=0.6))
        case HatchFill(tag="gradient", gradient=(stops, _angle)):
            band = 12.0 / max(len(stops), 1)
            for order, (_offset, color) in enumerate(stops):
                canvas.append(drawsvg.Rectangle(1, 1 + order * band, 26, band, fill=color, stroke=ink, stroke_width=0.3))
        case HatchFill(tag="pattern", pattern=spec):
            canvas.append(drawsvg.Rectangle(1, 1, 26, 12, fill="none", stroke=ink, stroke_width=0.6))
            for family in spec.families:
                dx, dy = math.cos(math.radians(family.angle)), math.sin(math.radians(family.angle))
                for offset in (7.0, 14.0, 21.0):
                    canvas.append(_family_stroke(family.motif, offset, dx, dy, ink))
        case _ as unreachable:
            assert_never(unreachable)
    return canvas.as_svg()


def _family_stroke(motif: Motif, offset: float, dx: float, dy: float, ink: str, /) -> "drawsvg.DrawingElement":
    x0, y0, x1, y1 = offset - dx * 6, 7 - dy * 5, offset + dx * 6, 7 + dy * 5
    match motif:
        case Motif(tag="line", line=segments):
            dash = " ".join(f"{abs(segment):g}" for segment in segments if segment != 0.0)
            return drawsvg.Line(x0, y0, x1, y1, stroke=ink, stroke_width=0.5, **({"stroke_dasharray": dash} if dash else {}))
        case Motif(tag="loop", loop=(amplitude, _chord)):
            span, (nx, ny) = max(min(amplitude, 3.0), 0.8), (-dy, dx)
            path = drawsvg.Path(stroke=ink, stroke_width=0.5, fill="none")
            path.M(x0, y0)
            for step in range(1, 7):
                t, side = step / 6.0, 1.0 if step % 2 else -1.0
                path.Q(
                    x0 + (x1 - x0) * (t - 1 / 12) + nx * span * side,
                    y0 + (y1 - y0) * (t - 1 / 12) + ny * span * side,
                    x0 + (x1 - x0) * t,
                    y0 + (y1 - y0) * t,
                )
            return path
        case _ as unreachable:
            assert_never(unreachable)


def _hex(rgb: tuple[int, int, int]) -> str:
    red, green, blue = rgb
    return f"#{red:02x}{green:02x}{blue:02x}"


# --- [TABLES] ---------------------------------------------------------------------------
_TEMPLATE: frozendict[ScheduleKind, ScheduleTemplate] = frozendict({
    ScheduleKind.DOOR: ScheduleTemplate(
        "DOOR SCHEDULE",
        "mark",
        (
            ColumnSpec("mark", "MARK", align="center"),
            ColumnSpec("type", "TYPE", align="center"),
            ColumnSpec("width", "W", FmtKind.NUMBER, "mm", "right"),
            ColumnSpec("height", "H", FmtKind.NUMBER, "mm", "right"),
            ColumnSpec("thickness", "THK", FmtKind.NUMBER, "mm", "right"),
            ColumnSpec("material", "MATERIAL"),
            ColumnSpec("finish", "FINISH"),
            ColumnSpec("glazing", "GLAZING"),
            ColumnSpec("fire_rating", "FIRE", align="center"),
            ColumnSpec("frame_type", "FR TYPE", align="center"),
            ColumnSpec("frame_material", "FR MATL"),
            ColumnSpec("hardware_set", "HW SET", align="center"),
            ColumnSpec("head", "HEAD", align="center"),
            ColumnSpec("jamb", "JAMB", align="center"),
            ColumnSpec("sill", "SILL", align="center"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (
            SpannerSpec("LEAF", ("type", "width", "height", "thickness")),
            SpannerSpec("FACE", ("material", "finish", "glazing")),
            SpannerSpec("FRAME", ("frame_type", "frame_material")),
            SpannerSpec("DETAILS", ("head", "jamb", "sill")),
        ),
        sort=("mark",),
        colors=("fire_rating",),
        notes=("Fire ratings per local code; verify hardware sets against door types.",),
    ),
    ScheduleKind.WINDOW: ScheduleTemplate(
        "WINDOW SCHEDULE",
        "mark",
        (
            ColumnSpec("mark", "MARK", align="center"),
            ColumnSpec("type", "TYPE", align="center"),
            ColumnSpec("width", "W", FmtKind.NUMBER, "mm", "right"),
            ColumnSpec("height", "H", FmtKind.NUMBER, "mm", "right"),
            ColumnSpec("sill_height", "SILL HT", FmtKind.NUMBER, "mm", "right"),
            ColumnSpec("material", "MATERIAL"),
            ColumnSpec("glazing", "GLAZING"),
            ColumnSpec("operation", "OPERATION"),
            ColumnSpec("u_value", "U", FmtKind.NUMBER, "W/m²K", "right", 2),
            ColumnSpec("shgc", "SHGC", FmtKind.NUMBER, "", "right", 2),
            ColumnSpec("head", "HEAD", align="center"),
            ColumnSpec("jamb", "JAMB", align="center"),
            ColumnSpec("sill", "SILL", align="center"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (
            SpannerSpec("OPENING", ("type", "width", "height", "sill_height")),
            SpannerSpec("GLAZING", ("material", "glazing", "operation")),
            SpannerSpec("PERFORMANCE", ("u_value", "shgc")),
            SpannerSpec("DETAILS", ("head", "jamb", "sill")),
        ),
        sort=("mark",),
    ),
    ScheduleKind.ROOM_FINISH: ScheduleTemplate(
        "ROOM FINISH SCHEDULE",
        "room_no",
        (
            ColumnSpec("room_no", "NO.", align="center"),
            ColumnSpec("room_name", "ROOM NAME"),
            ColumnSpec("floor", "FLOOR"),
            ColumnSpec("base", "BASE"),
            ColumnSpec("wall_n", "N"),
            ColumnSpec("wall_e", "E"),
            ColumnSpec("wall_s", "S"),
            ColumnSpec("wall_w", "W"),
            ColumnSpec("ceiling", "CEILING"),
            ColumnSpec("ceiling_height", "CLG HT", FmtKind.NUMBER, "mm", "right"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("WALLS", ("wall_n", "wall_e", "wall_s", "wall_w")), SpannerSpec("CEILING", ("ceiling", "ceiling_height"))),
        sort=("room_no",),
    ),
    ScheduleKind.WALL_TYPE: ScheduleTemplate(
        "WALL TYPE SCHEDULE",
        "type_mark",
        (
            ColumnSpec("type_mark", "TYPE", align="center"),
            ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("thickness", "THK", FmtKind.NUMBER, "mm", "right"),
            ColumnSpec("fire_rating", "FIRE", align="center"),
            ColumnSpec("stc", "STC", FmtKind.INTEGER, "", "right"),
            ColumnSpec("assembly", "ASSEMBLY"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("RATING", ("fire_rating", "stc")),),
        sort=("type_mark",),
        colors=("fire_rating",),
    ),
    ScheduleKind.PARTITION: ScheduleTemplate(
        "PARTITION SCHEDULE",
        "type_mark",
        (
            ColumnSpec("type_mark", "TYPE", align="center"),
            ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("thickness", "THK", FmtKind.NUMBER, "mm", "right"),
            ColumnSpec("height", "HT"),
            ColumnSpec("fire_rating", "FIRE", align="center"),
            ColumnSpec("stc", "STC", FmtKind.INTEGER, "", "right"),
            ColumnSpec("head_condition", "HEAD"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("RATING", ("fire_rating", "stc")),),
        sort=("type_mark",),
        colors=("fire_rating",),
    ),
    ScheduleKind.EQUIPMENT: ScheduleTemplate(
        "EQUIPMENT SCHEDULE",
        "mark",
        (
            ColumnSpec("mark", "MARK", align="center"),
            ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("manufacturer", "MANUFACTURER"),
            ColumnSpec("model", "MODEL"),
            ColumnSpec("quantity", "QTY", FmtKind.INTEGER, "", "right"),
            ColumnSpec("electrical", "ELEC"),
            ColumnSpec("plumbing", "PLBG"),
            ColumnSpec("weight", "WEIGHT", FmtKind.NUMBER, "kg", "right"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("PRODUCT", ("manufacturer", "model")), SpannerSpec("UTILITIES", ("electrical", "plumbing"))),
        sort=("mark",),
    ),
    ScheduleKind.PLUMBING_FIXTURE: ScheduleTemplate(
        "PLUMBING FIXTURE SCHEDULE",
        "mark",
        (
            ColumnSpec("mark", "MARK", align="center"),
            ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("manufacturer", "MANUFACTURER"),
            ColumnSpec("model", "MODEL"),
            ColumnSpec("hot_water", "HW"),
            ColumnSpec("cold_water", "CW"),
            ColumnSpec("waste", "WASTE"),
            ColumnSpec("vent", "VENT"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("PRODUCT", ("manufacturer", "model")), SpannerSpec("CONNECTIONS", ("hot_water", "cold_water", "waste", "vent"))),
        sort=("mark",),
    ),
    ScheduleKind.LIGHTING_FIXTURE: ScheduleTemplate(
        "LIGHTING FIXTURE SCHEDULE",
        "mark",
        (
            ColumnSpec("mark", "MARK", align="center"),
            ColumnSpec("type", "TYPE", align="center"),
            ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("lamp", "LAMP"),
            ColumnSpec("wattage", "W", FmtKind.INTEGER, "W", "right"),
            ColumnSpec("voltage", "V", FmtKind.INTEGER, "V", "right"),
            ColumnSpec("mounting", "MOUNTING"),
            ColumnSpec("manufacturer", "MANUFACTURER"),
            ColumnSpec("model", "MODEL"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("LAMP", ("lamp", "wattage", "voltage")), SpannerSpec("PRODUCT", ("manufacturer", "model"))),
        sort=("mark",),
    ),
    ScheduleKind.FINISH: ScheduleTemplate(
        "FINISH SCHEDULE",
        "code",
        (
            ColumnSpec("code", "CODE", align="center"),
            ColumnSpec("category", "CATEGORY"),
            ColumnSpec("material", "MATERIAL"),
            ColumnSpec("manufacturer", "MANUFACTURER"),
            ColumnSpec("product", "PRODUCT"),
            ColumnSpec("color", "COLOR"),
            ColumnSpec("finish", "FINISH"),
            ColumnSpec("size", "SIZE", align="right"),
            ColumnSpec("location", "LOCATION"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("PRODUCT", ("manufacturer", "product", "color", "finish")),),
        sort=("code",),
    ),
    ScheduleKind.HARDWARE_SET: ScheduleTemplate(
        "DOOR HARDWARE SET SCHEDULE",
        "set_no",
        (
            ColumnSpec("set_no", "SET", align="center"),
            ColumnSpec("quantity", "QTY", FmtKind.INTEGER, "", "right"),
            ColumnSpec("item", "ITEM"),
            ColumnSpec("manufacturer", "MANUFACTURER"),
            ColumnSpec("product", "PRODUCT"),
            ColumnSpec("finish", "FINISH", align="center"),
        ),
        sort=("set_no",),
    ),
    ScheduleKind.PANEL: ScheduleTemplate(
        "PANEL SCHEDULE",
        "panel_circuit",
        (
            ColumnSpec("panel_circuit", "PANEL/CKT", align="center"),
            ColumnSpec("panel", "PANEL", align="center"),
            ColumnSpec("circuit", "CKT", align="center"),
            ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("location", "LOCATION"),
            ColumnSpec("voltage", "V", FmtKind.INTEGER, "V", "right"),
            ColumnSpec("phase", "PH", align="center"),
            ColumnSpec("load_va", "LOAD", FmtKind.INTEGER, "VA", "right"),
            ColumnSpec("load_category", "CATEGORY"),
            ColumnSpec("demand_factor", "DF", FmtKind.NUMBER, "", "right", 2),
            ColumnSpec("demand_va", "DEMAND", FmtKind.INTEGER, "VA", "right"),
            ColumnSpec("poles", "P", FmtKind.INTEGER, "", "center"),
            ColumnSpec("breaker", "TRIP", FmtKind.INTEGER, "A", "right"),
            ColumnSpec("phase_a_va", "A", FmtKind.INTEGER, "VA", "right"),
            ColumnSpec("phase_b_va", "B", FmtKind.INTEGER, "VA", "right"),
            ColumnSpec("phase_c_va", "C", FmtKind.INTEGER, "VA", "right"),
            ColumnSpec("feed_from", "FED FROM"),
            ColumnSpec("feeder", "FEEDER"),
            ColumnSpec("neutral", "NEUTRAL"),
            ColumnSpec("ground", "GROUND"),
            ColumnSpec("bus_rating", "BUS", FmtKind.INTEGER, "A", "right"),
            ColumnSpec("main_rating", "MAIN", FmtKind.INTEGER, "A", "right"),
            ColumnSpec("aic_rating", "AIC", FmtKind.INTEGER, "A", "right"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (
            SpannerSpec("CIRCUIT", ("panel", "circuit", "description", "location")),
            SpannerSpec("LOAD", ("voltage", "phase", "load_va", "load_category", "demand_factor", "demand_va")),
            SpannerSpec("BREAKER", ("poles", "breaker")),
            SpannerSpec("PHASE LOAD", ("phase_a_va", "phase_b_va", "phase_c_va")),
            SpannerSpec("FEEDER", ("feed_from", "feeder", "neutral", "ground")),
            SpannerSpec("RATINGS", ("bus_rating", "main_rating", "aic_rating")),
        ),
        sort=("panel", "circuit"),
    ),
    ScheduleKind.AIR_TERMINAL: ScheduleTemplate(
        "AIR TERMINAL SCHEDULE",
        "mark",
        (
            ColumnSpec("mark", "MARK", align="center"),
            ColumnSpec("type", "TYPE", align="center"),
            ColumnSpec("service", "SERVICE"),
            ColumnSpec("neck_size", "NECK"),
            ColumnSpec("face_size", "FACE"),
            ColumnSpec("airflow", "AIRFLOW", FmtKind.NUMBER, "L/s", "right", 1),
            ColumnSpec("throw", "THROW", FmtKind.NUMBER, "m", "right", 1),
            ColumnSpec("nc", "NC", FmtKind.INTEGER, "", "right"),
            ColumnSpec("pressure_drop", "ΔP", FmtKind.NUMBER, "Pa", "right", 1),
            ColumnSpec("mounting", "MOUNTING"),
            ColumnSpec("manufacturer", "MANUFACTURER"),
            ColumnSpec("model", "MODEL"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("PERFORMANCE", ("airflow", "throw", "nc", "pressure_drop")), SpannerSpec("PRODUCT", ("manufacturer", "model"))),
        sort=("mark",),
    ),
    ScheduleKind.MECHANICAL_EQUIPMENT: ScheduleTemplate(
        "MECHANICAL EQUIPMENT SCHEDULE",
        "mark",
        (
            ColumnSpec("mark", "MARK", align="center"),
            ColumnSpec("service", "SERVICE"),
            ColumnSpec("location", "LOCATION"),
            ColumnSpec("quantity", "QTY", FmtKind.INTEGER, "", "right"),
            ColumnSpec("capacity", "CAPACITY"),
            ColumnSpec("airflow", "AIRFLOW", FmtKind.NUMBER, "L/s", "right", 1),
            ColumnSpec("esp", "ESP", FmtKind.NUMBER, "Pa", "right", 1),
            ColumnSpec("heating_power", "HEAT", FmtKind.NUMBER, "kW", "right", 1),
            ColumnSpec("cooling_power", "COOL", FmtKind.NUMBER, "kW", "right", 1),
            ColumnSpec("electrical", "ELECTRICAL"),
            ColumnSpec("mca", "MCA", FmtKind.INTEGER, "A", "right"),
            ColumnSpec("mocp", "MOCP", FmtKind.INTEGER, "A", "right"),
            ColumnSpec("weight", "WEIGHT", FmtKind.NUMBER, "kg", "right", 1),
            ColumnSpec("manufacturer", "MANUFACTURER"),
            ColumnSpec("model", "MODEL"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (
            SpannerSpec("PERFORMANCE", ("capacity", "airflow", "esp", "heating_power", "cooling_power")),
            SpannerSpec("POWER", ("electrical", "mca", "mocp")),
            SpannerSpec("PRODUCT", ("manufacturer", "model")),
        ),
        sort=("mark",),
    ),
    ScheduleKind.ELECTRICAL_EQUIPMENT: ScheduleTemplate(
        "ELECTRICAL EQUIPMENT SCHEDULE",
        "mark",
        (
            ColumnSpec("mark", "MARK", align="center"),
            ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("location", "LOCATION"),
            ColumnSpec("voltage", "V", FmtKind.INTEGER, "V", "right"),
            ColumnSpec("phase", "PH", align="center"),
            ColumnSpec("wires", "WIRE", align="center"),
            ColumnSpec("frequency", "HZ", FmtKind.INTEGER, "Hz", "right"),
            ColumnSpec("connected_load", "CONNECTED", FmtKind.NUMBER, "kVA", "right", 1),
            ColumnSpec("demand_load", "DEMAND", FmtKind.NUMBER, "kVA", "right", 1),
            ColumnSpec("feed_from", "FED FROM"),
            ColumnSpec("feeder", "FEEDER"),
            ColumnSpec("ocpd", "OCPD", FmtKind.INTEGER, "A", "right"),
            ColumnSpec("aic", "AIC", FmtKind.INTEGER, "A", "right"),
            ColumnSpec("manufacturer", "MANUFACTURER"),
            ColumnSpec("model", "MODEL"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (
            SpannerSpec("SYSTEM", ("voltage", "phase", "wires", "frequency")),
            SpannerSpec("LOAD", ("connected_load", "demand_load")),
            SpannerSpec("SUPPLY", ("feed_from", "feeder", "ocpd", "aic")),
            SpannerSpec("PRODUCT", ("manufacturer", "model")),
        ),
        sort=("mark",),
    ),
    ScheduleKind.FIRE_PROTECTION: ScheduleTemplate(
        "FIRE PROTECTION SCHEDULE",
        "mark",
        (
            ColumnSpec("mark", "MARK", align="center"),
            ColumnSpec("type", "TYPE"),
            ColumnSpec("location", "LOCATION"),
            ColumnSpec("hazard", "HAZARD"),
            ColumnSpec("coverage", "COVERAGE", FmtKind.NUMBER, "m²", "right", 1),
            ColumnSpec("k_factor", "K", FmtKind.NUMBER, "", "right", 1),
            ColumnSpec("orifice", "ORIFICE"),
            ColumnSpec("temperature", "TEMP", FmtKind.NUMBER, "°C", "right", 0),
            ColumnSpec("response", "RESPONSE"),
            ColumnSpec("finish", "FINISH"),
            ColumnSpec("manufacturer", "MANUFACTURER"),
            ColumnSpec("model", "MODEL"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("DESIGN", ("hazard", "coverage", "k_factor", "orifice", "temperature", "response")), SpannerSpec("PRODUCT", ("manufacturer", "model"))),
        sort=("mark",),
    ),
    ScheduleKind.SIGNAGE: ScheduleTemplate(
        "SIGNAGE SCHEDULE",
        "mark",
        (
            ColumnSpec("mark", "MARK", align="center"),
            ColumnSpec("type", "TYPE"),
            ColumnSpec("message", "MESSAGE"),
            ColumnSpec("location", "LOCATION"),
            ColumnSpec("mounting", "MOUNTING"),
            ColumnSpec("size", "SIZE"),
            ColumnSpec("material", "MATERIAL"),
            ColumnSpec("finish", "FINISH"),
            ColumnSpec("illumination", "ILLUMINATION"),
            ColumnSpec("tactile", "TACTILE", align="center"),
            ColumnSpec("braille", "BRAILLE", align="center"),
            ColumnSpec("contrast", "CONTRAST"),
            ColumnSpec("detail", "DETAIL", align="center"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("ACCESSIBILITY", ("tactile", "braille", "contrast")), SpannerSpec("FABRICATION", ("material", "finish", "illumination"))),
        sort=("mark",),
    ),
    ScheduleKind.ROOM_DATA: ScheduleTemplate(
        "ROOM DATA SCHEDULE",
        "room_no",
        (
            ColumnSpec("room_no", "NO.", align="center"),
            ColumnSpec("room_name", "ROOM NAME"),
            ColumnSpec("department", "DEPARTMENT"),
            ColumnSpec("occupancy", "OCCUPANCY"),
            ColumnSpec("area", "AREA", FmtKind.NUMBER, "m²", "right", 1),
            ColumnSpec("ceiling_height", "CLG HT", FmtKind.NUMBER, "mm", "right", 0),
            ColumnSpec("floor", "FLOOR"),
            ColumnSpec("base", "BASE"),
            ColumnSpec("walls", "WALLS"),
            ColumnSpec("ceiling", "CEILING"),
            ColumnSpec("doors", "DOORS"),
            ColumnSpec("glazing", "GLAZING"),
            ColumnSpec("casework", "CASEWORK"),
            ColumnSpec("equipment", "EQUIPMENT"),
            ColumnSpec("lighting", "LIGHTING"),
            ColumnSpec("power", "POWER"),
            ColumnSpec("data", "DATA"),
            ColumnSpec("plumbing", "PLUMBING"),
            ColumnSpec("hvac", "HVAC"),
            ColumnSpec("fire_protection", "FIRE"),
            ColumnSpec("acoustics", "ACOUSTICS"),
            ColumnSpec("security", "SECURITY"),
            ColumnSpec("accessibility", "ACCESSIBILITY"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (
            SpannerSpec("FINISHES", ("floor", "base", "walls", "ceiling")),
            SpannerSpec("OPENINGS", ("doors", "glazing")),
            SpannerSpec("CONTENTS", ("casework", "equipment")),
            SpannerSpec("SERVICES", ("lighting", "power", "data", "plumbing", "hvac", "fire_protection")),
            SpannerSpec("CRITERIA", ("acoustics", "security", "accessibility")),
        ),
        sort=("room_no",),
    ),
    ScheduleKind.STRUCTURAL_COLUMN: ScheduleTemplate(
        "COLUMN SCHEDULE",
        "mark",
        (
            ColumnSpec("mark", "MARK", align="center"),
            ColumnSpec("size", "SIZE"),
            ColumnSpec("material", "MATERIAL"),
            ColumnSpec("grade", "GRADE", align="center"),
            ColumnSpec("base_plate", "BASE PL"),
            ColumnSpec("splice", "SPLICE"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        sort=("mark",),
    ),
    ScheduleKind.STRUCTURAL_BEAM: ScheduleTemplate(
        "BEAM SCHEDULE",
        "mark",
        (
            ColumnSpec("mark", "MARK", align="center"),
            ColumnSpec("size", "SIZE"),
            ColumnSpec("material", "MATERIAL"),
            ColumnSpec("grade", "GRADE", align="center"),
            ColumnSpec("camber", "CAMBER", FmtKind.NUMBER, "mm", "right"),
            ColumnSpec("connection", "CONNECTION"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        sort=("mark",),
    ),
    ScheduleKind.FURNITURE: ScheduleTemplate(
        "FURNITURE & EQUIPMENT SCHEDULE",
        "mark",
        (
            ColumnSpec("mark", "MARK", align="center"),
            ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("manufacturer", "MANUFACTURER"),
            ColumnSpec("model", "MODEL"),
            ColumnSpec("quantity", "QTY", FmtKind.INTEGER, "", "right"),
            ColumnSpec("location", "LOCATION"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("PRODUCT", ("manufacturer", "model")),),
        sort=("mark",),
    ),
    ScheduleKind.REVISION: ScheduleTemplate(
        "REVISION HISTORY",
        "mark",
        (
            ColumnSpec("mark", "REV", align="center"),
            ColumnSpec("date", "DATE", align="center"),
            ColumnSpec("sheet", "SHEET", align="center"),
            ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("by", "BY", align="center"),
        ),
        sort=("date", "mark"),
    ),
    ScheduleKind.QUANTITY: ScheduleTemplate(
        "QUANTITY TAKEOFF",
        "description",
        (
            ColumnSpec("classification", "DIVISION", align="center"),
            ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("material", "MATERIAL"),
            ColumnSpec("unit", "UNIT", align="center"),
            ColumnSpec("count", "QTY", FmtKind.INTEGER, "no.", "right"),
            ColumnSpec("length", "LENGTH", FmtKind.NUMBER, "m", "right", 2),
            ColumnSpec("area", "AREA", FmtKind.NUMBER, "m²", "right", 2),
            ColumnSpec("volume", "VOLUME", FmtKind.NUMBER, "m³", "right", 3),
            ColumnSpec("weight", "WEIGHT", FmtKind.NUMBER, "kg", "right", 1),
            ColumnSpec("unit_rate", "RATE", FmtKind.NUMBER, "", "right", 2),
            ColumnSpec("total_cost", "AMOUNT", FmtKind.NUMBER, "", "right", 2),
        ),
        (SpannerSpec("MEASURE", ("count", "length", "area", "volume", "weight")), SpannerSpec("COST", ("unit_rate", "total_cost"))),
        sort=("classification", "description"),
        colors=("total_cost",),
        totals=("count", "length", "area", "volume", "weight", "total_cost"),
        rollup=("classification", "description", "material", "unit", "unit_rate"),
        footnotes=(
            ("unit_rate", "Rates exclude overheads, profit, and preliminaries."),
            ("total_cost", "BIM-derived estimate — verify before tender."),
        ),
        notes=("Quantities BIM-derived and approximate; verify before pricing.",),
    ),
})

_LEGEND_TITLE: frozendict[LegendKind, str] = frozendict({
    LegendKind.LINE_TYPE: "LINE TYPE LEGEND",
    LegendKind.HATCH_MATERIAL: "MATERIAL HATCH LEGEND",
    LegendKind.DISCIPLINE_LAYER: "DISCIPLINE LAYER LEGEND",
    LegendKind.SYMBOL: "SYMBOL LEGEND",
    LegendKind.ABBREVIATION: "ABBREVIATIONS",
    LegendKind.KEYNOTE: "KEYNOTE LEGEND",
    LegendKind.MATERIAL_FINISH: "MATERIAL LEGEND",
    LegendKind.GENERAL_NOTE: "GENERAL NOTES",
})

_LINE_MEANING: frozendict[LineType, str] = frozendict({
    LineType.CONTINUOUS: "Visible edges and outlines",
    LineType.DASHED: "Hidden edges",
    LineType.DASHED_SPACED: "Hidden edges (alternate)",
    LineType.LONG_DASH_DOT: "Centre lines and axes of symmetry",
    LineType.LONG_DASH_DOUBLE_DOT: "Adjacent parts, alternate positions",
    LineType.LONG_DASH_TRIPLE_DOT: "Special surface treatment",
    LineType.DOTTED: "Hidden detail",
    LineType.LONG_DASH_SHORT_DASH: "Cutting and viewing planes",
    LineType.LONG_DASH_DOUBLE_SHORT_DASH: "Outlines of parts in front of a cutting plane",
    LineType.DASH_DOT: "Pitch lines and lines of symmetry",
    LineType.DOUBLE_DASH_DOT: "Outline of movable parts",
    LineType.DASH_DOUBLE_DOT: "Hinge lines and lines of weakness",
    LineType.DOUBLE_DASH_DOUBLE_DOT: "Outlines of adjacent parts, alternate positions",
    LineType.DASH_TRIPLE_DOT: "Boundaries of special surface treatment",
    LineType.DOUBLE_DASH_TRIPLE_DOT: "Boundaries of special requirements (alternate)",
})

_HATCH_MEANING: frozendict[HatchMaterial, str] = frozendict({
    HatchMaterial.STEEL: "Steel / metal in section",
    HatchMaterial.ALUMINIUM: "Aluminium / light alloy in section",
    HatchMaterial.CONCRETE: "Cast-in-place concrete",
    HatchMaterial.CONCRETE_REINFORCED: "Reinforced concrete",
    HatchMaterial.MASONRY: "Masonry / brick",
    HatchMaterial.TIMBER_GRAIN: "Timber, grain direction",
    HatchMaterial.TIMBER_END: "Timber, end grain",
    HatchMaterial.INSULATION_THERMAL: "Thermal / acoustic insulation",
    HatchMaterial.PLASTIC: "Plastics / polymers in section",
    HatchMaterial.EARTH: "Earth / subgrade",
    HatchMaterial.HARDCORE: "Hardcore / compacted fill",
    HatchMaterial.LIQUID: "Liquid",
    HatchMaterial.GLASS: "Glass / glazing",
})

_DISCIPLINE_MEANING: frozendict[Discipline, str] = frozendict({
    Discipline.ARCHITECTURAL: "Architectural",
    Discipline.CIVIL: "Civil",
    Discipline.ELECTRICAL: "Electrical",
    Discipline.FIRE: "Fire protection",
    Discipline.GENERAL: "General",
    Discipline.HAZMAT: "Hazardous materials",
    Discipline.INTERIORS: "Interiors",
    Discipline.LANDSCAPE: "Landscape",
    Discipline.MECHANICAL: "Mechanical",
    Discipline.PLUMBING: "Plumbing",
    Discipline.EQUIPMENT: "Equipment",
    Discipline.RESOURCE: "Resource",
    Discipline.STRUCTURAL: "Structural",
    Discipline.TELECOM: "Telecommunications",
    Discipline.SURVEY: "Survey / mapping",
    Discipline.PROCESS: "Process",
    Discipline.OTHER: "Other disciplines",
    Discipline.CONTRACTOR: "Contractor / shop",
})

# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["ColumnSpec", "LegendEntry", "LegendKind", "Schedule", "ScheduleContent", "ScheduleKind", "ScheduleTemplate", "SpannerSpec"]
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
