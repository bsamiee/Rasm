# [PY_ARTIFACTS_DRAWING_SCHEDULE]

The AEC scheduling plane — the owned schedule-template and legend vocabulary lowering a settled quantity/schedule frame into `visualization/table#TABLE` and rendering it as a standards-formatted door/window/room-finish/equipment/panel/structural schedule OR an ISO-drafting legend. `Schedule` is ONE owner over the closed `ScheduleContent` union: a `tabular` case carrying a settled `polars.DataFrame` (the QTO/schedule rows arriving over the `data/tabular` wire from `csharp:Rasm.Bim`, styled here, never authored here) plus a `ScheduleKind` selecting the NCS/AIA column TEMPLATE, and a `legend` case carrying a `LegendKind` selecting which `drawing/regime#REGIME` vocabulary to enumerate into a symbol→meaning table. The owner derives the `TableOp` sequence from the template, builds the styled bytes through the one `TablePlan.build`, and mints one `ArtifactReceipt.Schedule`.

The AEC schedule is an OWNED vocabulary, not a package — the NCS/AIA/CSI conventions are `frozendict` template rows authored to their real column cardinalities (a door schedule owns the full leaf/face/frame/fire/hardware/detail field set, not a two-column stub), exactly as `drawing/regime#REGIME` owns the ISO drafting tables. `Schedule` composes `visualization/table#TABLE` for the render (holding no `great-tables` surface of its own — `TablePlan` owns the `GT` builder), `drawing/regime#REGIME` and `drawing/standard#STANDARD` for the ISO legend codes the legends enumerate, `graphic/color/derive#DERIVE` `hex_ramp` for the cost/legend palette, `polars` for settled-frame `select`/`sort` shaping, and `drawsvg` for the legend swatches. The synchronous fold offloads on the runtime thread lane and rails through `RuntimeRail`/`async_boundary`; the flat table bytes are the handoff `composition/compose#COMPOSE` places, and the owner contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Schedule` and one `core/plan#PLAN` `ArtifactWork` node. This is the publication-table lowering anchor: `Schedule` lowers into the `visualization/table` builder, never a second table renderer.

## [01]-[INDEX]

- [01]-[SCHEDULE]: the `Schedule` owner over the `ScheduleContent` union (a `tabular` frame + `ScheduleKind` template, or a `LegendKind` legend) lowering into `visualization/table#TABLE`.

## [02]-[SCHEDULE]

- Owner: `Schedule` holds `content: ScheduleContent`, the `Palette`, the target `TableFormat`, the `Theme` publication identity, and the `Standard` profile (read only on the `legend` arm for the swatch codes); it discriminates render on the closed `ScheduleContent` union whose two cases carry ONLY their own payload — never a monolithic bag whose frame field is dead on the legend arm. It composes `visualization/table#TABLE` `TablePlan` and holds NO `great-tables` `GT` surface: `TablePlan` owns the styled-table builder, `Schedule` owns only the AEC template-to-`TableOp` lowering. `ScheduleTemplate` is the ordered column layout (`ColumnSpec` label/`FmtKind`/unit/align/decimals, `SpannerSpec` groups, sort key, `color_by`, `note`, the `rollup`/`carry` BOQ keys, the `group` division lead, and column-anchored `footnotes`); `LegendEntry` the authored `(code, description, swatch)` row.
- Cases: the `ScheduleContent` union — `tabular(frame, kind)` a data-driven schedule over a settled `polars.DataFrame` styled by the `ScheduleKind` template, `legend(kind, entries)` an ISO-drafting or authored legend — one total `match` in `_lower` closed by `assert_never`. `ScheduleKind` is the NCS/AIA schedule types authored to real column cardinality, with `QUANTITY` the BIM-derived bill of quantities: the settled per-element frame ROLLED to one row per `specification/classify#CLASSIFY` division/description/material/unit, the `MEASURE` and `COST` spanners SUMmed on the grand `totals`, division-sorted, the amount cost-shaded — the canonical `csharp:Rasm.Bim` QTO consumer, the rollup DISPLAYED here and never authored here. `LegendKind` is the drafting-legend types: `LINE_TYPE`/`HATCH_MATERIAL`/`DISCIPLINE_LAYER` DERIVED from the `drawing/regime#REGIME` vocabularies (the swatch drawn from the real ISO code), the rest from an authored `LegendEntry` set.
- Entry: `Schedule.tabular` and `Schedule.legend` are the two-case constructors, `@beartype(conf=_INGRESS)`-admitted at the seam, never a `mode`/`batch` knob — the content CASE is the discriminant. `_emit` is `async` over `async_boundary`, offloading the whole synchronous lower-shape-render fold onto the runtime thread lane. `_render` is the ONE pure builder: `_lower` dispatches the content into a shaped frame plus a derived `TableOp` sequence, `TablePlan(...).build()` renders the ONE bytes fact, `ContentIdentity.of(...)` keys it, and `ArtifactReceipt.Schedule(...)` carries the evidence — the bytes and the receipt both derive from the ONE `build`, never a second re-render.
- Auto: the `TableOp` sequence is DERIVED from the one `_TEMPLATE` row, filtered to the shaped frame's real columns so a subset QTO frame never references an absent column in a `Spanner`/`Fmt`. The grand total is ONE summed row (`pl.col(*present_totals).sum()`), a design schedule with empty `totals` keeping the bare count; the `color_by` column data-colors through `hex_ramp(palette)`; a `Footnote(at=COLUMN_LABELS)` marks the exact rate/amount column rather than a blanket source note. `_schedule_frame` `select`s the template columns, ROLLS a per-element takeoff to its BOQ display shape (`group_by(*rollup).agg` — measures SUMmed, the carried rate `first()`), and `sort`s division-adjacent — it aggregates existing values for display, authors no data, and re-implements no transform the `data` owner owns. The `legend` arm authors its frame from the regime vocabulary through `drawsvg` structured primitives (never an f-string SVG splice), the swatch rendered inline through `TableOp.Fmt(FmtKind.MARKDOWN)` so the HTML egress carries the real ISO sample beside the code and the schedule-owned meaning.
- Packages: `visualization/table#TABLE` the styled-table render and the `build` bytes seam (`TableOp`/`Theme`/`TableFormat`/`FmtKind`, the column-anchored `Footnote` marks); `beartype(conf=_INGRESS)` the two constructors' ingress contract redirecting a malformed kind/frame to the boundary rail; `polars` the settled-frame `select`/`sort` boundary shaping and the summary aggregates — the `data/tabular` overlay, never the engine, `PolarsError` the `_FAULTS` base the boundary narrows `catch` to beside the great-tables `ValueError`/`KeyError` and the table-miss `KeyError`, never the `Exception` catch-all; `drawing/standard#STANDARD` the ISO 128/128-50/13567 legend codes (`LineType.pattern`/`Standard.hatch`/`Standard.rgb`); `graphic/color/derive#DERIVE` `hex_ramp` the data-color and legend palette; `drawsvg` the structured legend swatch. No `ezdxf` (a schedule is a table, the drawing-plane siblings own the CAD egress); no `great-tables` import (the render is `visualization/table`'s, composed through `TablePlan`).
- Growth: a new AEC schedule type is one `ScheduleKind` member plus one `_TEMPLATE` row; a new column one `ColumnSpec`; a new column group one `SpannerSpec`; a new summed measure/cost total one column name on `totals`; a new BOQ display rollup the `rollup`/`carry` pair; a new division sort-lead one `group` field; a new column footnote one `footnotes` `(column, text)` row; a new legend type one `LegendKind` member plus one `_LEGEND_TITLE` row (and for a derived legend one `_legend_rows` arm); a new legend meaning one `_*_MEANING` row; a new data-color column one `color_by` field; a new receipt scalar one slot on the shared `ArtifactReceipt.Schedule` case.
- Boundary: no sheet placement (`composition/sheet#SHEET`), no drawing-symbol geometry (`drawing/symbol#SYMBOL`), no IFC authoring (`csharp:Rasm.Bim` owns the QTO/schedule rows). `visualization/table#TABLE` owns the render, `drawing/regime#REGIME` and `drawing/standard#STANDARD` the ISO legend codes, `graphic/color/derive#DERIVE` the palette, `polars` the frame shaping, `drawsvg` the swatch primitives, `composition/compose#COMPOSE` the placement, `specification/classify#CLASSIFY` the keynote classification codes; identity minting is the runtime's.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import math
import os
from collections.abc import Iterable
from enum import StrEnum
from typing import Literal, Self, assert_never

import polars as pl
from beartype import BeartypeConf, beartype
from polars.exceptions import PolarsError
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.drawing.regime import Discipline, HatchMaterial, LayerName, LineType
from artifacts.drawing.standard import Standard
from artifacts.graphic.vector.pattern import HatchFill, PatternSpec
from rasm.artifacts.graphic.color.derive import Palette, hex_ramp
from artifacts.visualization.table import FmtKind, FootnoteMarks, StubLoc, TableFormat, TableOp, TablePlan, Theme

# the drawsvg swatch author reifies on first legend-arm use in the offloaded worker
lazy import drawsvg

# --- [TYPES] ----------------------------------------------------------------------------
type Swatch = str  # an inline SVG legend sample rendered through TableOp.Fmt(FmtKind.MARKDOWN)
type Align = Literal["left", "center", "right"]
type Footnote = tuple[str, str]  # (source column, footnote text) — a column-label-anchored tab_footnote mark


class ScheduleKind(StrEnum):  # NCS/AIA/CSI AEC schedule types — each keys one _TEMPLATE column layout
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
    STRUCTURAL_COLUMN = "structural-column"
    STRUCTURAL_BEAM = "structural-beam"
    FURNITURE = "furniture"
    QUANTITY = "quantity"  # the BIM-derived quantity takeoff — count/length/area/volume/weight/cost, totalled


class LegendKind(StrEnum):  # ISO-drafting + authored legend types — the DERIVED trio reads drawing/regime
    LINE_TYPE = "line-type"  # ISO 128 line-type samples (LineType.pattern)
    HATCH_MATERIAL = "hatch-material"  # ISO 128-50 section indicators (Standard.hatch)
    DISCIPLINE_LAYER = "discipline-layer"  # ISO 13567/AIA discipline pen colors (Standard.rgb)
    SYMBOL = "symbol"  # authored drawing-symbol legend
    ABBREVIATION = "abbreviation"  # authored abbreviations legend
    KEYNOTE = "keynote"  # authored keynote legend (specification/classify codes)
    MATERIAL_FINISH = "material-finish"  # authored material/finish legend
    GENERAL_NOTE = "general-note"  # authored general notes


# --- [CONSTANTS] ------------------------------------------------------------------------
# the real engine raise tuple the boundary narrows catch to — PolarsError base, great-tables ValueError/KeyError/
# NotImplementedError, a table-miss KeyError, the gated-PDF OSError — so a non-engine raise crosses as a defect, never the Exception catch-all.
_FAULTS: tuple[type[Exception], ...] = (PolarsError, ValueError, KeyError, NotImplementedError, OSError)
# the ingress contract — a malformed ScheduleKind/frame refused at the constructors' seam; violation_type
# redirects the refusal to the async_boundary catch, never a raw throw into the interior.
_INGRESS: BeartypeConf = BeartypeConf(violation_type=ValueError)
# the AEC schedule publication identity — bordered grid, all-caps headers, ISO-drafting †/‡ footnote marks,
# not the numeric default a legend reads as a callout.
_AEC_THEME: Theme = Theme(
    style=1, color="gray", all_caps=True, header_align="center", outline=("solid", "1px", "#333333"), footnote_marks=FootnoteMarks.STANDARD
)


# --- [MODELS] ---------------------------------------------------------------------------
class ColumnSpec(Struct, frozen=True):
    # one schedule column; FmtKind None = plain text, the unit folds into the header, decimals apply to a NUMBER column.
    source: str
    label: str
    fmt: FmtKind | None = None
    unit: str = ""
    align: Align = "left"
    decimals: int = 0


class SpannerSpec(Struct, frozen=True):
    # one column-group header spanning its member source columns (a great-tables tab_spanner).
    label: str
    columns: tuple[str, ...]


class ScheduleTemplate(Struct, frozen=True):
    # the ordered AEC schedule layout — one row per ScheduleKind, the single edit site _schedule_ops folds.
    title: str
    key: str
    columns: tuple[ColumnSpec, ...]
    spanners: tuple[SpannerSpec, ...] = ()
    sort: str | None = None
    color_by: str | None = None
    totals: tuple[str, ...] = ()  # numeric columns the grand-summary SUMs (the QTO takeoff totals)
    note: str = ""
    group: str | None = (
        None  # the division sort-lead; the great-tables row-group render is version-blocked — see RESEARCH
    )
    rollup: tuple[str, ...] = ()  # group_by keys rolling the element frame to its BOQ display shape
    carry: tuple[str, ...] = ()  # display columns the rollup carries by first() (a per-item rate)
    footnotes: tuple[Footnote, ...] = ()  # column-anchored footnotes marked †/‡ by the theme


class LegendEntry(Struct, frozen=True):
    # one authored legend row — the code/symbol, its meaning, and an optional inline-SVG swatch.
    code: str
    description: str
    swatch: Swatch = ""


# --- [VOCABULARY] -----------------------------------------------------------------------
@tagged_union(frozen=True)
class ScheduleContent:
    # per-mode payloads — each case carries ONLY its own payload, never a shared bag with a dead field.
    tag: Literal["tabular", "legend"] = tag()
    tabular: tuple[pl.DataFrame, ScheduleKind] = case()  # a settled QTO/schedule frame + its template
    legend: tuple[LegendKind, tuple[LegendEntry, ...]] = case()  # a legend kind + its authored entries


# --- [SERVICES] -------------------------------------------------------------------------
class Schedule(Struct, frozen=True):
    content: ScheduleContent
    palette: Palette
    fmt: TableFormat = TableFormat.HTML
    theme: Theme = _AEC_THEME
    standard: Standard = Standard()  # read only on the legend arm — the ISO legend-swatch codes

    @classmethod
    @beartype(conf=_INGRESS)
    def tabular(
        cls,
        frame: pl.DataFrame,
        kind: ScheduleKind,
        palette: Palette,
        /,
        *,
        fmt: TableFormat = TableFormat.HTML,
        theme: Theme = _AEC_THEME,
        standard: Standard = Standard(),
    ) -> Self:
        return cls(content=ScheduleContent(tabular=(frame, kind)), palette=palette, fmt=fmt, theme=theme, standard=standard)

    @classmethod
    @beartype(conf=_INGRESS)
    def legend(
        cls,
        kind: LegendKind,
        palette: Palette,
        /,
        entries: Iterable[LegendEntry] = (),
        *,
        fmt: TableFormat = TableFormat.HTML,
        theme: Theme = _AEC_THEME,
        standard: Standard = Standard(),
    ) -> Self:
        return cls(content=ScheduleContent(legend=(kind, tuple(entries))), palette=palette, fmt=fmt, theme=theme, standard=standard)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: minted PRE-RUN, never over rendered bytes.
        return ContentIdentity.of(f"drawing-schedule-{self.content.tag}", (self.content, self.fmt, self.theme, self.standard), policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the rendered bytes are content-addressed on the receipt FACTS, the slot the PRE-RUN input key; the fold crosses the runtime thread lane.
        crossed = await async_boundary(
            f"drawing.schedule.{self.content.tag}",
            lambda: LanePolicy.offload(_render, self, modality=Modality.THREAD, retry=RetryClass.OCCT),
            catch=_FAULTS,
        )
        return crossed.bind(lambda rail: rail.map(lambda pair: pair[1]))


# --- [OPERATIONS] -----------------------------------------------------------------------
def _render(schedule: Schedule) -> tuple[bytes, ArtifactReceipt]:
    # the ONE builder: lower the content, render the ONE bytes fact, key it, mint the receipt — bytes and receipt from the single build, never a second re-render (SINGLE-FACT).
    frame, ops, kind, rows, columns = _lower(schedule)
    data = TablePlan(frame=frame, ops=ops, fmt=schedule.fmt, theme=schedule.theme).build()
    key = schedule._key
    return data, ArtifactReceipt.Schedule(key, kind, rows, columns, schedule.fmt.value, len(data))


def _lower(schedule: Schedule) -> tuple[pl.DataFrame, tuple[TableOp, ...], str, int, int]:
    # the one total dispatch over the content, closed by assert_never.
    match schedule.content:
        case ScheduleContent(tag="tabular", tabular=(frame, kind)):
            template = _TEMPLATE[kind]
            shaped = _schedule_frame(frame, template)
            ops = _schedule_ops(template, schedule.palette, frozenset(shaped.columns))
            return shaped, ops, kind.value, shaped.height, len(shaped.columns)
        case ScheduleContent(tag="legend", legend=(kind, entries)):
            frame = _legend_frame(kind, entries, schedule.standard)
            return frame, _legend_ops(kind), f"legend-{kind.value}", frame.height, len(frame.columns)
        case _ as unreachable:
            assert_never(unreachable)


def _schedule_frame(frame: pl.DataFrame, template: ScheduleTemplate) -> pl.DataFrame:
    # boundary DISPLAY shaping — select the template columns, roll the takeoff to its BOQ shape (group_by division+item,
    # SUM measures, FIRST the rate), sort [division, item]. Values aggregated for display, never authored.
    present = [column.source for column in template.columns if column.source in frame.columns]
    shaped = frame.select(present) if present else frame
    if template.rollup and all(key in shaped.columns for key in template.rollup):
        measures = tuple(pl.col(column).sum() for column in template.totals if column in shaped.columns)
        carried = tuple(pl.col(column).first() for column in template.carry if column in shaped.columns and column not in template.rollup)
        shaped = shaped.group_by(*template.rollup, maintain_order=True).agg(*measures, *carried)
    keys = tuple(dict.fromkeys(key for key in (template.group, template.sort) if key and key in shaped.columns))
    return shaped.sort(keys) if keys else shaped


def _schedule_ops(template: ScheduleTemplate, palette: Palette, present: frozenset[str]) -> tuple[TableOp, ...]:
    # DERIVE the whole TableOp sequence from the ONE template row, filtered to the shaped frame's columns. The great-tables
    # Stub(group=) row-group + per-division Summary(groups=) subtotals are BLOCKED on the current polars great-tables build — see RESEARCH.
    cols = tuple(column for column in template.columns if column.source in present)
    labels = {column.source: (f"{column.label} ({column.unit})" if column.unit else column.label) for column in cols}
    key_label = next((column.label for column in cols if column.source == template.key), template.key)
    stub = (TableOp.Stub(template.key), TableOp.Stubhead(key_label)) if template.key in present else ()
    # the walrus narrows FmtKind|None -> FmtKind for the op; decimals ride only where fmt_number admits them.
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
    colored = (TableOp.Color(columns=[color], palette=hex_ramp(palette)),) if (color := template.color_by) is not None and color in present else ()
    # the grand total is ONE summed row over the present measures (no totals -> the bare item count).
    present_totals = tuple(column for column in template.totals if column in present)
    grand_fns = {"Total": pl.col(*present_totals).sum()} if present_totals else {"Count": pl.col(template.key).count()}
    summary = (TableOp.GrandSummary(grand_fns, missing_text=""),) if template.key in present else ()
    footnotes = tuple(TableOp.Footnote(text, at=StubLoc.COLUMN_LABELS, columns=[column]) for column, text in template.footnotes if column in present)
    noted = (TableOp.SourceNote(template.note),) if template.note else ()
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


def _legend_frame(kind: LegendKind, entries: tuple[LegendEntry, ...], standard: Standard) -> pl.DataFrame:
    # author the legend frame — the derived trio or the authored entries; pl.from_dicts builds the (swatch, code, description) frame.
    rows = _legend_rows(kind, entries, standard)
    return pl.from_dicts([{"symbol": swatch, "code": code, "description": description} for swatch, code, description in rows])


def _legend_rows(kind: LegendKind, entries: tuple[LegendEntry, ...], standard: Standard) -> tuple[tuple[Swatch, str, str], ...]:
    # the DERIVED legends read the real ISO code into a drawsvg swatch; the authored legends fold their entries. TOTAL.
    match kind:
        case LegendKind.LINE_TYPE:
            return tuple((_dash_swatch(lt.pattern), lt.name, _LINE_MEANING[lt]) for lt in LineType)
        case LegendKind.HATCH_MATERIAL:
            return tuple((_hatch_swatch(standard.hatch(material)), material.value, _HATCH_MEANING[material]) for material in HatchMaterial)
        case LegendKind.DISCIPLINE_LAYER:
            return tuple(
                (_color_swatch(standard.rgb(LayerName.of(discipline, "XXXX"))), discipline.value, _DISCIPLINE_MEANING[discipline])
                for discipline in Discipline
            )
        case LegendKind.SYMBOL | LegendKind.ABBREVIATION | LegendKind.KEYNOTE | LegendKind.MATERIAL_FINISH | LegendKind.GENERAL_NOTE:
            return tuple((entry.swatch, entry.code, entry.description) for entry in entries)
        case _ as unreachable:
            assert_never(unreachable)


def _legend_ops(kind: LegendKind) -> tuple[TableOp, ...]:
    # the legend TableOp sequence — the swatch column inline via fmt_markdown carrying the real ISO SVG sample.
    return (
        TableOp.Header(_LEGEND_TITLE[kind]),
        TableOp.Label({"symbol": "", "code": "SYMBOL", "description": "DESCRIPTION"}),
        TableOp.Fmt(FmtKind.MARKDOWN, columns=["symbol"]),
        TableOp.Align("center", columns=["symbol", "code"]),
        TableOp.Align("left", columns=["description"]),
        TableOp.Width({"symbol": "84px"}),
    )


def _dash_swatch(pattern: tuple[float, ...]) -> Swatch:
    # the ISO 128 line-type sample — a drawsvg Line carrying the real dash array (solid for CONTINUOUS); a value to stroke_dasharray, never a splice.
    canvas = drawsvg.Drawing(64, 14, origin=(0, 0))
    dash = " ".join(f"{abs(segment):g}" for segment in pattern if segment != 0.0)
    canvas.append(drawsvg.Line(2, 7, 62, 7, stroke="#111111", stroke_width=1.2, **({"stroke_dasharray": dash} if dash else {})))
    return canvas.as_svg()


def _color_swatch(rgb: tuple[int, int, int]) -> Swatch:
    # the discipline pen sample — a drawsvg Rectangle filled with the real Standard.rgb sRGB.
    canvas = drawsvg.Drawing(28, 14, origin=(0, 0))
    canvas.append(drawsvg.Rectangle(1, 1, 26, 12, fill=_hex(rgb), stroke="#111111", stroke_width=0.6))
    return canvas.as_svg()


def _hatch_swatch(spec: PatternSpec) -> Swatch:
    # the ISO 128-50 section indicator — a solid, graded, or crosshatched box at the PatternSpec.angle; SHOWS the fill regime, not the full ezdxf render. Total over HatchFill.
    canvas = drawsvg.Drawing(28, 14, origin=(0, 0))
    match spec.fill:
        case HatchFill.SOLID:
            canvas.append(drawsvg.Rectangle(1, 1, 26, 12, fill="#111111", stroke="#111111", stroke_width=0.6))
        case HatchFill.GRADIENT:
            canvas.append(drawsvg.Rectangle(1, 7, 26, 6, fill="#7a5c33", stroke="#111111", stroke_width=0.6))
            canvas.append(drawsvg.Rectangle(1, 1, 26, 6, fill="#c6a86a", stroke="#111111", stroke_width=0.6))
        case HatchFill.PATTERN:
            canvas.append(drawsvg.Rectangle(1, 1, 26, 12, fill="none", stroke="#111111", stroke_width=0.6))
            dx, dy = math.cos(math.radians(spec.angle)), math.sin(math.radians(spec.angle))
            for offset in (7.0, 14.0, 21.0):
                canvas.append(drawsvg.Line(offset - dx * 6, 7 - dy * 5, offset + dx * 6, 7 + dy * 5, stroke="#111111", stroke_width=0.5))
        case _ as unreachable:
            assert_never(unreachable)
    return canvas.as_svg()


def _hex(rgb: tuple[int, int, int]) -> str:
    red, green, blue = rgb
    return f"#{red:02x}{green:02x}{blue:02x}"


# --- [TABLES] ---------------------------------------------------------------------------
# the NCS/AIA schedule vocabulary -> its ordered ColumnSpec layout; the single edit site per schedule type.
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
        sort="mark",
        color_by="fire_rating",
        note="Fire ratings per local code; verify hardware sets against door types.",
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
        sort="mark",
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
        sort="room_no",
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
        sort="type_mark",
        color_by="fire_rating",
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
        sort="type_mark",
        color_by="fire_rating",
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
        sort="mark",
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
        sort="mark",
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
        sort="mark",
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
        sort="code",
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
        sort="set_no",
    ),
    ScheduleKind.PANEL: ScheduleTemplate(
        "PANEL SCHEDULE",
        "circuit",
        (
            ColumnSpec("circuit", "CKT", align="center"),
            ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("load_va", "LOAD", FmtKind.INTEGER, "VA", "right"),
            ColumnSpec("poles", "P", FmtKind.INTEGER, "", "center"),
            ColumnSpec("breaker", "TRIP", FmtKind.INTEGER, "A", "right"),
            ColumnSpec("phase", "PH", align="center"),
            ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("BREAKER", ("poles", "breaker", "phase")),),
        sort="circuit",
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
        sort="mark",
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
        sort="mark",
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
        sort="mark",
    ),
    # the canonical Rasm.Bim QTO consumer: the takeoff ROLLED to its BOQ display shape (group_by division+item+material+unit,
    # measures SUMmed, rate carried), division-sorted, cost-shaded, the rate/amount columns footnoted.
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
        sort="description",
        color_by="total_cost",
        totals=("count", "length", "area", "volume", "weight", "total_cost"),
        group="classification",
        rollup=("classification", "description", "material", "unit"),
        carry=("unit_rate",),
        footnotes=(
            ("unit_rate", "Rates exclude overheads, profit, and preliminaries."),
            ("total_cost", "BIM-derived estimate — verify before tender."),
        ),
        note="Quantities BIM-derived and approximate; verify before pricing.",
    ),
})

# the legend kind -> its printed title; the single edit site per legend type.
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

# the ISO 128 line-type -> its drafting meaning (beside the dash swatch); TOTAL over the LineType family so
# _legend_rows indexes every member (a partial slice KeyErrors when regime's LineType grows).
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

# the ISO 128-50 section material -> its meaning (the legend description beside the angled section indicator).
_HATCH_MEANING: frozendict[HatchMaterial, str] = frozendict({
    HatchMaterial.STEEL: "Steel / metal in section",
    HatchMaterial.CONCRETE: "Cast-in-place concrete",
    HatchMaterial.CONCRETE_REINFORCED: "Reinforced concrete",
    HatchMaterial.MASONRY: "Masonry / brick",
    HatchMaterial.TIMBER_GRAIN: "Timber, grain direction",
    HatchMaterial.TIMBER_END: "Timber, end grain",
    HatchMaterial.INSULATION_THERMAL: "Thermal / acoustic insulation",
    HatchMaterial.EARTH: "Earth / subgrade",
    HatchMaterial.HARDCORE: "Hardcore / compacted fill",
    HatchMaterial.LIQUID: "Liquid",
    HatchMaterial.GLASS: "Glass / glazing",
})

# the ISO 13567/AIA discipline -> its name (the legend description beside the real discipline pen swatch).
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

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [ROWGROUP]-[BLOCKED]: does the current polars great-tables build render `Stub(group=)` row-groups with per-division `Summary(groups=)` subtotals (the SMM7/NRM2 BOQ layout with a `Subtotal` closing each classification division)?; probe `visualization/table#TABLE`'s `GT` render against the polars-native `.style` path, falling back to the `GT(frame.to_pandas())` floor while it fails.
