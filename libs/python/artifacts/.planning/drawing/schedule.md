# [PY_ARTIFACTS_DRAWING_SCHEDULE]

The AEC scheduling plane: the owned schedule-template + legend vocabulary that LOWERS a settled quantity/schedule frame into `visualization/table#TABLE` and renders it as a standards-formatted door/window/room-finish/equipment/panel/structural schedule OR an ISO-drafting legend. `Schedule` is ONE owner over the closed `ScheduleContent` `expression.tagged_union` — a `tabular` case carrying a settled `polars.DataFrame` (the QTO/schedule rows arriving over the `data/tabular` wire from `csharp:Rasm.Bim`, styled here, never authored here) plus a `ScheduleKind` selecting the ISO/NCS column TEMPLATE, and a `legend` case carrying a `LegendKind` selecting which `drawing/standard#STANDARD` owned vocabulary to enumerate into a symbol→meaning table. The owner discriminates on the content case, ROLLS a per-element takeoff up to its bill-of-quantities display shape, DERIVES the `visualization/table#TABLE` `TableOp` sequence from the template (headers, spanners, per-column `FmtKind`, missing-value substitution, data-driven cost coloring, a division-sorted layout closing on a grand total, column-anchored footnotes), builds the styled table bytes through the one `TablePlan.build` render, and mints one `core/receipt#RECEIPT` `ArtifactReceipt.Schedule` case — the two public constructors `@beartype`-admitted at the ingress seam.

The AEC schedule is an OWNED vocabulary, not a package — the National CAD Standard / AIA / CSI schedule conventions are `frozendict` template rows authored to their real column cardinalities (a door schedule owns the full leaf/face/frame/fire/hardware/detail field set, not a two-column stub), exactly as `drawing/standard#STANDARD` owns the ISO drafting tables and `visualization/diagram/glyphset#GLYPHSET` owns the diagram primitives. `ScheduleKind` selects a `ScheduleTemplate` (the ordered `ColumnSpec` set with display label, `FmtKind`, unit, alignment, decimals, its `SpannerSpec` column groups, the sort key, the data-color column, and the standards note); `LegendKind` selects a `drawing/standard#STANDARD` vocabulary (`LineType` dash samples, `HatchMaterial` section indicators, `Discipline` layer-pen colors) or an authored `LegendEntry` set (abbreviations, keynotes, material-finish, general notes). Growth is one template row, one legend row, or one column on an existing row — never a per-schedule owner and never a re-implemented table renderer.

`Schedule` composes `visualization/table#TABLE` for the render (it holds no `great-tables` surface of its own — the boundary is law: `TablePlan` owns the `GT` builder, `Schedule` owns only the AEC template lowering), `drawing/standard#STANDARD` for the ISO `LineType`/`HatchMaterial`/`Discipline`/`LayerName`/`HatchSpec` codes the legends enumerate, `graphic/color/derive#DERIVE` (through the `visualization/chart/spec#CHART` `Palette`/`hex_ramp` seam) for the fire-rating data-color ramp and legend swatch palette, `polars` for the settled-frame shaping (`select`/`sort` at the boundary, never authoring data), and `drawsvg` for the structured-primitive legend swatches (never an f-string SVG splice). The synchronous `polars`/`great-tables`/`drawsvg` fold offloads off the event loop through one `CapacityLimiter`-bounded `anyio.to_thread.run_sync` — the shared-address-space thread lane the `msgspec`/`polars`/`great-tables` owners force, exactly as `visualization/diagram/draw#DRAW` and the sibling `drawing/symbol#SYMBOL` take — and rails through the runtime `RuntimeRail`/`async_boundary` `BoundaryFault`, never a decorative page-local fault union. The rendered table bytes are the FLAT handoff `composition/compose#COMPOSE` places beside its sibling graphics; `Schedule` computes no sheet placement (`composition/sheet#SHEET`), re-authors no IFC (`csharp:Rasm.Bim` owns the QTO/schedule rows), and re-renders nothing.

## [01]-[INDEX]

- [01]-[SCHEDULE]: the `Schedule` owner over the closed `ScheduleContent` `expression.tagged_union` (`tabular` a settled `polars.DataFrame` + `ScheduleKind` template, `legend` a `LegendKind` + authored `LegendEntry` set) lowering into `visualization/table#TABLE` — the `ScheduleKind` NCS/AIA schedule vocabulary (`DOOR`/`WINDOW`/`ROOM_FINISH`/`WALL_TYPE`/`PARTITION`/`EQUIPMENT`/`PLUMBING_FIXTURE`/`LIGHTING_FIXTURE`/`FINISH`/`HARDWARE_SET`/`PANEL`/`STRUCTURAL_COLUMN`/`STRUCTURAL_BEAM`/`FURNITURE`/`QUANTITY`) keyed in `_TEMPLATE` to its `ScheduleTemplate(title, key, columns, spanners, sort, color_by, totals, note, group, rollup, carry, footnotes)` ordered `ColumnSpec`/`SpannerSpec` layout, the `LegendKind` ISO-drafting legend vocabulary (`LINE_TYPE`/`HATCH_MATERIAL`/`DISCIPLINE_LAYER` derived from `drawing/standard#STANDARD` + `SYMBOL`/`ABBREVIATION`/`KEYNOTE`/`MATERIAL_FINISH`/`GENERAL_NOTE` authored), the `_schedule_ops`/`_legend_ops` derivations folding the template into the `TableOp` sequence (`Header`/`Stub`/`Stubhead`/`Label`/`Fmt`/`Align`/`Spanner`/`Color`/`SubMissing`/`GrandSummary`/`Footnote`/`SourceNote`, the `QUANTITY` takeoff rolled to its BOQ display shape, division-sorted, closing on the grand `GrandSummary` total, the amount column cost-shaded and the rate/amount columns footnoted), the `drawsvg`-authored legend swatches (`_dash_swatch` the ISO 128 line pattern, `_color_swatch` the discipline pen sRGB, `_hatch_swatch` the ISO 128-50 section indicator), and the one `TablePlan.build` render whose ONE bytes fact keys the artifact — palette-indexed to `graphic/color/derive#DERIVE` through `hex_ramp`, railed through `RuntimeRail`/`async_boundary`, offloaded via `anyio.to_thread`, contributing one `ArtifactReceipt.Schedule` case and one `core/plan#PLAN` `ArtifactWork` node.

## [02]-[SCHEDULE]

- Owner: `Schedule` the one AEC-scheduling owner holding `content: ScheduleContent`, the `visualization/chart/spec#CHART` `Palette`, the target `TableFormat`, the `visualization/table#TABLE` `Theme` publication identity, and the `drawing/standard#STANDARD` `Standard` profile (read only on the `legend` arm for the swatch codes) — discriminating render on the closed `ScheduleContent` `tagged_union` whose two cases carry ONLY their own payload (`tabular` a `(polars.DataFrame, ScheduleKind)`, `legend` a `(LegendKind, tuple[LegendEntry, ...])`), never a monolithic bag whose frame field is dead on the legend arm. It composes `visualization/table#TABLE` `TablePlan` for the render and holds NO `great-tables` `GT` surface of its own — the boundary is law: `TablePlan` owns the styled-table builder, `Schedule` owns only the AEC template-to-`TableOp` lowering. `ScheduleTemplate` is the ordered column layout (`ColumnSpec` display label / `FmtKind` / unit / alignment / decimals, `SpannerSpec` column groups, the sort key, the `color_by` data-color column, the standards `note`, the `rollup`/`carry` BOQ display-aggregation keys, the `group` division sort-lead, and the column-anchored `footnotes`); `LegendEntry` the authored `(code, description, swatch)` row; `LegendKind`/`ScheduleKind` the closed vocabularies. `visualization/table#TABLE` owns the `great-tables` render (`GT`, `TableOp`, `Theme`, `TableFormat`, `FmtKind`); `drawing/standard#STANDARD` owns the ISO `LineType`/`HatchMaterial`/`Discipline`/`LayerName`/`HatchSpec` codes the legends enumerate; `graphic/color/derive#DERIVE` (via the `visualization/chart/spec#CHART` `hex_ramp`) owns the palette; `polars` the settled-frame shaping; `drawsvg` the legend-swatch structured primitives. No sheet placement, drawing-symbol geometry, or IFC authoring crosses this owner — those are `composition/sheet#SHEET`, `drawing/symbol#SYMBOL`, and `csharp:Rasm.Bim`.
- Cases: the `ScheduleContent` `tagged_union` — `tabular(frame, kind)` a data-driven schedule over a settled `polars.DataFrame` styled by the `ScheduleKind` template, `legend(kind, entries)` an ISO-drafting or authored legend — matched by one total `match` in `_lower`, closed by `assert_never`. `ScheduleKind` the NCS/AIA schedule types authored to real column cardinality: `DOOR` (leaf/face/frame/fire/hardware/detail — 16 columns, fire-rating data-colored), `WINDOW` (opening/glazing/thermal-performance/detail with `u_value`/`shgc`), `ROOM_FINISH` (floor/base/four-wall/ceiling with ceiling height), `WALL_TYPE`/`PARTITION` (thickness/fire/STC assembly), `EQUIPMENT`/`PLUMBING_FIXTURE`/`LIGHTING_FIXTURE`/`FURNITURE` (product/utility/lamp), `FINISH` (material/product/color/location), `HARDWARE_SET` (set-grouped door hardware), `PANEL` (circuit/load/breaker), `STRUCTURAL_COLUMN`/`STRUCTURAL_BEAM` (size/material/grade/connection), `QUANTITY` (the BIM-derived bill of quantities — the settled per-element frame ROLLED to one row per `specification/classify#CLASSIFY` classification-division/description/material/unit, the count/length/area/volume/weight `MEASURE` spanner and the unit-rate/amount `COST` spanner, division-sorted, the six measure+cost columns SUMmed on the grand `totals` and the amount cost-shaded — the canonical `csharp:Rasm.Bim` QTO consumer the brief `[09]` names, the rollup DISPLAYED here and never authored here). `LegendKind` the drafting-legend types: `LINE_TYPE`/`HATCH_MATERIAL`/`DISCIPLINE_LAYER` DERIVED from the `drawing/standard#STANDARD` vocabularies (the swatch drawn from the real ISO code — the dash array, the section angle, the discipline pen sRGB), `SYMBOL`/`ABBREVIATION`/`KEYNOTE`/`MATERIAL_FINISH`/`GENERAL_NOTE` from an authored `LegendEntry` set — matched by one total `match` in `_legend_rows`, closed by `assert_never`.
- Entry: `Schedule.tabular(frame, kind, palette, *, fmt, theme, standard)` and `Schedule.legend(kind, palette, entries, *, fmt, theme, standard)` are the two-case constructors building the `ScheduleContent`, never a `mode`/`batch` knob — the content CASE is the discriminant the value carries. `resolve` is `async` over the runtime `async_boundary`, returns a `RuntimeRail[tuple[bytes, ArtifactReceipt]]`, and offloads the whole synchronous lower-shape-render fold onto `to_thread.run_sync(_render, self, limiter=_LANES)` — the shared-address-space thread arm (the `polars` frame shaping, the `great-tables` `GT` render, and the `drawsvg` swatch author touch the `numpy` palette and return the `msgspec`-backed `ArtifactReceipt` owners a `to_interpreter` isolate cannot load, the same lane `drawing/symbol#SYMBOL` and `visualization/diagram/draw#DRAW` take). `_render` is the ONE pure builder the thread runs: `_lower` dispatches the content into a shaped frame + a derived `TableOp` sequence + the kind/row/column counts, `TablePlan(frame, ops, fmt, theme).build()` renders the ONE bytes fact `visualization/table#TABLE` owns, `ContentIdentity.of(f"drawing-schedule-{kind}", data)` keys it, and `ArtifactReceipt.Schedule(key, kind, rows, columns, fmt, len(data))` carries the evidence — the bytes and the receipt both derive from the ONE `build` render, never a second re-render to mint the receipt.
- Auto: the `TableOp` sequence is DERIVED from the one `_TEMPLATE` row, never enumerated per schedule — `_schedule_ops` folds the present columns (filtered to the shaped frame's real columns so a QTO frame carrying a subset of the template fields never references an absent column in a `Spanner` or `Fmt`) into one `Header`, one `Stub`/`Stubhead` on the key column, one `Label` map (the unit folded into the header text `"WIDTH (mm)"`), one `Fmt` per numeric/integer column (`decimals` threaded only where `FmtKind.NUMBER` admits it), one `Align` per column, one `Spanner` per present column group, one `SubMissing("—")` for the blank cells, one `GrandSummary` totalling the whole sheet (`pl.col(*present_totals).sum()` the ONE multi-column aggregate great-tables lands as a single summary row, a design schedule with empty `totals` keeping the bare `Count`), one `Color` data-coloring the `color_by` column (a `QUANTITY` amount heat-scale or a fire-rating severity band) through `hex_ramp(palette)`, and one `Footnote(at=COLUMN_LABELS)` per `footnotes` row marked †/‡ by the theme's `opt_footnote_marks`. The frame arrives settled from `data/tabular`; `_schedule_frame` `select`s the template columns, ROLLS a per-element takeoff up to its BOQ display shape (`group_by(*rollup, maintain_order=True).agg` — measures SUMmed, the carried rate `first()`), and `sort`s the division column-adjacent so the BOQ reads by division — it aggregates existing values for display, authors no data, re-opens no lazy engine, and re-implements no transform the `data` owner owns. The great-tables `Stub(group=)` row-group + per-division `Summary(groups=)` subtotals are the intended enhancement, version-BLOCKED on the current polars great-tables build (RESEARCH). The `legend` arm authors its frame from the `drawing/standard#STANDARD` vocabulary: `_legend_rows` enumerates each `LineType` (its `.pattern` ISO 128 dash array → a `drawsvg` `Line` dash sample), each `HatchMaterial` (its `Standard.hatch` `HatchSpec.angle` → a `drawsvg` crosshatch indicator), or each `Discipline` (its `Standard.rgb` pen sRGB → a `drawsvg` filled swatch), the swatch column rendered inline through `TableOp.Fmt(FmtKind.MARKDOWN)` so the HTML egress carries the real ISO sample beside the code and the schedule-owned meaning. Every swatch is authored through `drawsvg` structured primitives (`Drawing`/`Rectangle`/`Line`/`as_svg`), never an f-string markup splice.
- Growth: a new AEC schedule type is one `ScheduleKind` member plus one `_TEMPLATE` `ScheduleTemplate` row (the `_schedule_ops` fold derives its whole `TableOp` sequence with zero edit); a new column on a schedule is one `ColumnSpec` in that row; a new column group is one `SpannerSpec`; a new summed measure/cost total is one column name on the template's `totals` tuple (the per-division `Summary` AND grand `GrandSummary` both derive with zero edit); a new BOQ display rollup is the `rollup`/`carry` pair on the template row; a new division sort-lead is one `group` field (the great-tables `Stub(group=)` row-group + per-division `Summary(groups=)` subtotal derive from it once the polars row-group render unblocks); a new column footnote is one `footnotes` `(column, text)` row; a new legend type is one `LegendKind` member plus one `_LEGEND_TITLE` row and (for a derived legend) one `_legend_rows` arm; a new legend meaning is one `_LINE_MEANING`/`_HATCH_MEANING`/`_DISCIPLINE_MEANING` row; a new format verb, theme axis, or column knob is the `visualization/table#TABLE` `TableOp`/`Theme` growth the owner composes, never re-declared here; a new data-color column is one `color_by` field on the template row; a new export format is one `TableFormat` reuse; a new receipt scalar is one slot on the shared `ArtifactReceipt.Schedule` case; a downstream that needs the composed `TablePlan` (to nest the schedule into a larger table) is one `plan` projection off the `_lower` fold; zero new surface for a new schedule, a new legend, or a new column.
- Boundary: the deleted forms are a per-schedule `DoorSchedule`/`WindowSchedule`/`RoomFinishSchedule` class family where one `ScheduleKind`-keyed `_TEMPLATE` states them; a re-implemented `great-tables` render where `visualization/table#TABLE` `TablePlan.build` owns it (the boundary — `Schedule` never touches a `GT`); a `polars` frame AUTHORED here where the settled QTO/schedule frame arrives over the `data/tabular` wire and is only `select`/`sort` shaped; a monolithic `ScheduleSpec` bag carrying a dead frame field on the legend arm where the per-mode `ScheduleContent` cases carry only their own payload; a hand-enumerated `TableOp` list per schedule where `_schedule_ops` derives it from the one template row; a `Spanner`/`Fmt` referencing a template column absent from a subset QTO frame where the present-column filter guards it; an f-string SVG swatch splice where `drawsvg` structured primitives author it (TEMPLATE-SAFETY); a re-derived ISO 128 dash array or discipline color where `drawing/standard#STANDARD` `LineType.pattern`/`Standard.rgb`/`Standard.hatch` own the real code; a per-cell fire-rating color literal where `TableOp.Color` over `hex_ramp(palette)` derives the severity band; a flat per-element takeoff dump where the `rollup` group-aggregates the settled frame to its canonical BOQ display shape; a per-column grand-summary bag fragmenting into one row per measure where `pl.col(*totals).sum()` lands one `Total` row; a blanket source-note-only caveat where a `Footnote(at=COLUMN_LABELS)` marks the exact rate/amount column; an un-contracted constructor where `@beartype(conf=_INGRESS)` refuses a malformed kind/frame at the seam; a `mode`/`batch` knob where the `ScheduleContent` case and the two constructors discriminate; a second re-render to mint the receipt where the ONE `build` bytes fact keys the artifact AND the receipt (SINGLE-FACT); a decorative page-local fault union where `RuntimeRail`/`async_boundary` carries the `BoundaryFault`; a synchronous render on the event loop where `to_thread.run_sync` offloads it; a parallel schedule receipt rail where the shared `ArtifactReceipt.Schedule` case carries the kind/row/column/byte facts; and any IFC authoring the `csharp:Rasm.Bim` owner holds. `visualization/table#TABLE` owns the render, `drawing/standard#STANDARD` the ISO legend codes, `graphic/color/derive#DERIVE` the palette, `polars` the frame shaping, `drawsvg` the swatch primitives, `composition/compose#COMPOSE` the placement, `specification/classify#CLASSIFY` the keynote classification codes, and `csharp:Rasm.Bim` the QTO/schedule rows; identity minting is the runtime's.
- Packages: `visualization/table#TABLE` (`TablePlan`/`TableOp`/`Theme`/`TableFormat`/`FmtKind` the styled-table render + the `build` bytes seam; `TableOp.Stub`/`GrandSummary` the stub + single-row grand-total ops — the `Stub(group=)`/`Summary(groups=)`/`RowGroupOrder` row-group subtotals composed but polars-render-blocked on great-tables 0.22, `TableOp.Footnote`/`StubLoc.COLUMN_LABELS`/`FootnoteMarks.STANDARD` the column-anchored †/‡ footnotes); `beartype` (`@beartype(conf=_INGRESS)` the two public constructors' ingress contract redirecting a malformed kind/frame to the boundary rail, the contract the sibling `standard`/`table`/`conformance` owners weave); `polars` (`pl.from_dicts` the authored legend frame, `DataFrame.select`/`.sort`/`.columns`/`.height` the settled-frame boundary shaping, `pl.col(...).count()`/`pl.col(...).sum()` the grand-summary count + `totals` measure/cost sum exprs — the `data/tabular` overlay, never the engine; `polars.exceptions.PolarsError` the `_FAULTS` frame-op base the boundary narrows `catch` to beside the great-tables `ValueError`/`KeyError` and the `_TEMPLATE`/`_*_MEANING`-miss `KeyError`, never the `Exception` catch-all); `drawing/standard#STANDARD` (`LineType`/`LineType.pattern`/`HatchMaterial`/`HatchSpec`/`Discipline`/`LayerName`/`Standard`/`Standard.hatch`/`Standard.rgb` the ISO 128/128-50/13567 legend codes); `visualization/chart/spec#CHART` (`Palette`/`hex_ramp` the `graphic/color/derive#DERIVE` ramp for the data-color band and legend palette); `drawsvg` (`Drawing`/`Rectangle`/`Line`/`as_svg` the structured legend swatch, never an f-string SVG); `expression` (`tagged_union`/`tag`/`case` the `ScheduleContent` vocabulary and rail); `msgspec` (`Struct(frozen=True)` the value objects); `builtins.frozendict` (the `_TEMPLATE`/`_LEGEND_TITLE`/`_*_MEANING` closed tables); `anyio` (`CapacityLimiter`/`to_thread` the offload); runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary`); `core/receipt#RECEIPT` (`ArtifactReceipt.Schedule`). No `ezdxf` (a schedule is a table, not a DXF drawing — the drawing-plane siblings own the CAD egress); no `great-tables` import (the render is `visualization/table#TABLE`'s, composed through `TablePlan`).

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import math
import os
from collections.abc import Iterable
from enum import StrEnum
from typing import Literal, Self, assert_never

import polars as pl
from anyio import CapacityLimiter, to_thread
from beartype import BeartypeConf, beartype
from builtins import frozendict
from polars.exceptions import PolarsError
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.drawing.standard import Discipline, HatchFill, HatchMaterial, HatchSpec, LayerName, LineType, Standard
from artifacts.visualization.chart.spec import Palette, hex_ramp
from artifacts.visualization.table import FmtKind, FootnoteMarks, StubLoc, TableFormat, TableOp, TablePlan, Theme

# the drawsvg swatch author reifies on first legend-arm use in the `to_thread` worker
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


class LegendKind(StrEnum):  # ISO-drafting + authored legend types — the DERIVED trio reads drawing/standard
    LINE_TYPE = "line-type"              # ISO 128 line-type samples (LineType.pattern)
    HATCH_MATERIAL = "hatch-material"    # ISO 128-50 section indicators (Standard.hatch)
    DISCIPLINE_LAYER = "discipline-layer"  # ISO 13567/AIA discipline pen colors (Standard.rgb)
    SYMBOL = "symbol"                    # authored drawing-symbol legend
    ABBREVIATION = "abbreviation"        # authored abbreviations legend
    KEYNOTE = "keynote"                  # authored keynote legend (specification/classify codes)
    MATERIAL_FINISH = "material-finish"  # authored material/finish legend
    GENERAL_NOTE = "general-note"        # authored general notes


# --- [CONSTANTS] ------------------------------------------------------------------------
_LANES: CapacityLimiter = CapacityLimiter(os.process_cpu_count() or 4)
# the real engine raise tuple the boundary narrows `catch` to — `PolarsError` the base every frame op throws,
# the great-tables render `ValueError`/`KeyError`/`NotImplementedError`, a `_TEMPLATE`/`_*_MEANING` miss `KeyError`,
# and the gated-PDF `OSError` — so a non-engine raise and cancellation cross as a defect rather than the
# `Exception` catch-all `boundaries.md` rejects, exactly as the composed `visualization/table#TABLE` narrows.
_FAULTS: tuple[type[Exception], ...] = (PolarsError, ValueError, KeyError, NotImplementedError, OSError)
# the ingress contract every sibling standard/table/conformance owner weaves — a malformed ScheduleKind/frame is
# refused at the two public constructors' seam rather than deep in the polars/great-tables fold (definition-time,
# rail-safe: violation_type redirects the refusal to the async_boundary catch, never a raw throw into the interior).
_INGRESS: BeartypeConf = BeartypeConf(violation_type=ValueError)
# the AEC schedule publication identity — a clean bordered grid, all-caps headers, centered title, ISO-drafting
# †/‡/§ footnote marks (opt_footnote_marks) rather than the numeric default a symbol legend reads as a callout.
_AEC_THEME: Theme = Theme(style=1, color="gray", all_caps=True, header_align="center", outline=("solid", "1px", "#333333"), footnote_marks=FootnoteMarks.STANDARD)


# --- [MODELS] ---------------------------------------------------------------------------
class ColumnSpec(Struct, frozen=True):
    # one schedule column — the source frame column name, the display header, its FmtKind (None = plain text),
    # the unit folded into the header, the alignment, and the decimals a NUMBER column formats to.
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
    # the ordered AEC schedule layout — the title, the stub key column, the ColumnSpec set, the SpannerSpec groups,
    # the sort key, the data-colored column, the standards note, the BOQ division row-group axis, the display
    # rollup keys, the first()-carried columns, and the column-anchored footnotes.
    title: str
    key: str
    columns: tuple[ColumnSpec, ...]
    spanners: tuple[SpannerSpec, ...] = ()
    sort: str | None = None
    color_by: str | None = None
    totals: tuple[str, ...] = ()  # numeric columns the group/grand-summary SUMs beside the key count (the QTO takeoff totals)
    note: str = ""
    group: str | None = None      # the division sort-lead column shown group-adjacent (a future great-tables row-group; the polars row-group render is version-blocked — see RESEARCH)
    rollup: tuple[str, ...] = ()   # the group_by keys that roll the settled element frame to its canonical BOQ display shape
    carry: tuple[str, ...] = ()    # non-key non-measure display columns the rollup carries by first() (a per-item unit rate)
    footnotes: tuple[Footnote, ...] = ()  # column-anchored footnotes marked †/‡ by the theme's opt_footnote_marks sequence


class LegendEntry(Struct, frozen=True):
    # one authored legend row — the code/symbol, its meaning, and an optional inline-SVG swatch.
    code: str
    description: str
    swatch: Swatch = ""


# --- [VOCABULARY] -----------------------------------------------------------------------
@tagged_union(frozen=True)
class ScheduleContent:
    # the two admission regimes as per-mode payloads — the tabular frame is dead on the legend arm and the
    # legend entries dead on the tabular arm, so each case carries ONLY its own payload, never a shared bag.
    tag: Literal["tabular", "legend"] = tag()
    tabular: tuple[pl.DataFrame, ScheduleKind] = case()          # a settled QTO/schedule frame + its template
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
    def tabular(cls, frame: pl.DataFrame, kind: ScheduleKind, palette: Palette, /, *, fmt: TableFormat = TableFormat.HTML, theme: Theme = _AEC_THEME, standard: Standard = Standard()) -> Self:
        return cls(content=ScheduleContent(tabular=(frame, kind)), palette=palette, fmt=fmt, theme=theme, standard=standard)

    @classmethod
    @beartype(conf=_INGRESS)
    def legend(cls, kind: LegendKind, palette: Palette, /, entries: Iterable[LegendEntry] = (), *, fmt: TableFormat = TableFormat.HTML, theme: Theme = _AEC_THEME, standard: Standard = Standard()) -> Self:
        return cls(content=ScheduleContent(legend=(kind, tuple(entries))), palette=palette, fmt=fmt, theme=theme, standard=standard)

    async def resolve(self) -> RuntimeRail[tuple[bytes, ArtifactReceipt]]:
        # the whole lower-shape-render fold is synchronous polars/great-tables/drawsvg CPU work, so it crosses
        # one to_thread seam off the event loop in the shared address space (the subinterpreter arm cannot load
        # the polars/numpy palette + msgspec receipt owners), never inline on the loop.
        return await async_boundary(f"drawing.schedule.{self.content.tag}", lambda: to_thread.run_sync(_render, self, limiter=_LANES), catch=_FAULTS)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _render(schedule: Schedule) -> tuple[bytes, ArtifactReceipt]:
    # the ONE builder the thread runs: lower the content, render the ONE bytes fact through visualization/table,
    # key the artifact off it, and mint the shared Schedule receipt — the bytes and the receipt both derive from
    # the single TablePlan.build render, never a second re-render (SINGLE-FACT).
    frame, ops, kind, rows, columns = _lower(schedule)
    data = TablePlan(frame=frame, ops=ops, fmt=schedule.fmt, theme=schedule.theme).build()
    key = ContentIdentity.of(f"drawing-schedule-{kind}", data)
    return data, ArtifactReceipt.Schedule(key, kind, rows, columns, schedule.fmt.value, len(data))


def _lower(schedule: Schedule) -> tuple[pl.DataFrame, tuple[TableOp, ...], str, int, int]:
    # the one total dispatch over the content — a tabular schedule shapes the settled frame and derives the ops
    # from the template, a legend authors its frame from the drawing/standard vocabulary; total by assert_never.
    match schedule.content:
        case ScheduleContent(tag="tabular", tabular=(frame, kind)):
            template = _TEMPLATE[kind]
            shaped = _schedule_frame(frame, template)
            # the settled per-element frame is rolled to its BOQ display shape by _schedule_frame, so `shaped`
            # already carries the division-sorted rolled rows; the ops derive from the shaped columns.
            ops = _schedule_ops(template, schedule.palette, frozenset(shaped.columns))
            return shaped, ops, kind.value, shaped.height, len(shaped.columns)
        case ScheduleContent(tag="legend", legend=(kind, entries)):
            frame = _legend_frame(kind, entries, schedule.standard)
            return frame, _legend_ops(kind), f"legend-{kind.value}", frame.height, len(frame.columns)
        case _ as unreachable:
            assert_never(unreachable)


def _schedule_frame(frame: pl.DataFrame, template: ScheduleTemplate) -> pl.DataFrame:
    # boundary DISPLAY shaping — select the template columns the settled frame carries, roll a per-element takeoff
    # up to its canonical BOQ display shape (group_by division+item, SUM each measure, FIRST the carried rate), and
    # sort [division, item] so the BOQ reads by division. The frame's VALUES are aggregated for display, never
    # authored — the data/tabular wire holds the per-element rows the C# Rasm.Bim graph emits; this owner only formats.
    present = [column.source for column in template.columns if column.source in frame.columns]
    shaped = frame.select(present) if present else frame
    if template.rollup and all(key in shaped.columns for key in template.rollup):
        measures = tuple(pl.col(column).sum() for column in template.totals if column in shaped.columns)
        carried = tuple(pl.col(column).first() for column in template.carry if column in shaped.columns and column not in template.rollup)
        shaped = shaped.group_by(*template.rollup, maintain_order=True).agg(*measures, *carried)
    keys = tuple(dict.fromkeys(key for key in (template.group, template.sort) if key and key in shaped.columns))
    return shaped.sort(keys) if keys else shaped


def _schedule_ops(template: ScheduleTemplate, palette: Palette, present: frozenset[str]) -> tuple[TableOp, ...]:
    # DERIVE the whole TableOp sequence from the ONE template row, filtered to the columns the shaped frame carries
    # (a subset/rolled frame never references an absent column). The BOQ division rides as a sorted regular column
    # (group-adjacent via the _schedule_frame [group, sort] sort); the great-tables `Stub(group=)` row-group +
    # per-division `Summary(groups=)` subtotals are BLOCKED on the current polars great-tables build — see RESEARCH.
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
    # the grand total is ONE summed row over the present measure set — `pl.col(*measures).sum()` the one multi-column
    # aggregate great-tables lands as a single row (a design schedule with no `totals` keeps the bare item count).
    present_totals = tuple(column for column in template.totals if column in present)
    grand_fns = {"Total": pl.col(*present_totals).sum()} if present_totals else {"Count": pl.col(template.key).count()}
    summary = (TableOp.GrandSummary(grand_fns, missing_text=""),) if template.key in present else ()
    footnotes = tuple(TableOp.Footnote(text, at=StubLoc.COLUMN_LABELS, columns=[column]) for column, text in template.footnotes if column in present)
    noted = (TableOp.SourceNote(template.note),) if template.note else ()
    return (TableOp.Header(template.title), *stub, TableOp.Label(labels), *fmts, *aligns, *spanners, *colored, TableOp.SubMissing(text="—"), *summary, *footnotes, *noted)


def _legend_frame(kind: LegendKind, entries: tuple[LegendEntry, ...], standard: Standard) -> pl.DataFrame:
    # author the legend frame from the drawing/standard vocabulary (the derived trio) or the authored entries;
    # pl.from_dicts builds the three-column (swatch, code, description) frame the legend ops style.
    rows = _legend_rows(kind, entries, standard)
    return pl.from_dicts([{"symbol": swatch, "code": code, "description": description} for swatch, code, description in rows])


def _legend_rows(kind: LegendKind, entries: tuple[LegendEntry, ...], standard: Standard) -> tuple[tuple[Swatch, str, str], ...]:
    # the DERIVED legends read the real ISO code (dash array / section angle / discipline sRGB) into a drawsvg
    # swatch beside the code and the schedule-owned meaning; the authored legends fold their entries. TOTAL.
    match kind:
        case LegendKind.LINE_TYPE:
            return tuple((_dash_swatch(lt.pattern), lt.name, _LINE_MEANING[lt]) for lt in LineType)
        case LegendKind.HATCH_MATERIAL:
            return tuple((_hatch_swatch(standard.hatch(material)), material.value, _HATCH_MEANING[material]) for material in HatchMaterial)
        case LegendKind.DISCIPLINE_LAYER:
            return tuple((_color_swatch(standard.rgb(LayerName.of(discipline, "XXXX"))), discipline.value, _DISCIPLINE_MEANING[discipline]) for discipline in Discipline)
        case LegendKind.SYMBOL | LegendKind.ABBREVIATION | LegendKind.KEYNOTE | LegendKind.MATERIAL_FINISH | LegendKind.GENERAL_NOTE:
            return tuple((entry.swatch, entry.code, entry.description) for entry in entries)
        case _ as unreachable:
            assert_never(unreachable)


def _legend_ops(kind: LegendKind) -> tuple[TableOp, ...]:
    # the legend TableOp sequence — the swatch column rendered inline through fmt_markdown so the HTML egress
    # carries the real ISO SVG sample; the swatch/code centered, the description left, the swatch column bounded.
    return (
        TableOp.Header(_LEGEND_TITLE[kind]),
        TableOp.Label({"symbol": "", "code": "SYMBOL", "description": "DESCRIPTION"}),
        TableOp.Fmt(FmtKind.MARKDOWN, columns=["symbol"]),
        TableOp.Align("center", columns=["symbol", "code"]),
        TableOp.Align("left", columns=["description"]),
        TableOp.Width({"symbol": "84px"}),
    )


def _dash_swatch(pattern: tuple[float, ...]) -> Swatch:
    # the ISO 128 line-type sample — a drawsvg Line carrying the real dash array (a solid line for CONTINUOUS's
    # empty pattern); the dash string is a value passed to the stroke_dasharray attribute, never a markup splice.
    canvas = drawsvg.Drawing(64, 14, origin=(0, 0))
    dash = " ".join(f"{abs(segment):g}" for segment in pattern if segment != 0.0)
    canvas.append(drawsvg.Line(2, 7, 62, 7, stroke="#111111", stroke_width=1.2, **({"stroke_dasharray": dash} if dash else {})))
    return canvas.as_svg()


def _color_swatch(rgb: tuple[int, int, int]) -> Swatch:
    # the ISO 13567/AIA discipline pen sample — a drawsvg Rectangle filled with the real Standard.rgb sRGB.
    canvas = drawsvg.Drawing(28, 14, origin=(0, 0))
    canvas.append(drawsvg.Rectangle(1, 1, 26, 12, fill=_hex(rgb), stroke="#111111", stroke_width=0.6))
    return canvas.as_svg()


def _hatch_swatch(spec: HatchSpec) -> Swatch:
    # the ISO 128-50 section indicator HONOURING the material's real HatchFill regime — a solid poche box, a graded
    # two-band box, or a bordered box crosshatched at the characteristic HatchSpec.angle; the legend SHOWS the real
    # fill regime, NOT the full ezdxf `spec.apply(hatch)` render the section producer owns. Total over HatchFill.
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
# the NCS/AIA schedule vocabulary -> its ordered ColumnSpec layout, authored to real column cardinality; the
# single edit site per schedule type, from which _schedule_ops derives the whole TableOp sequence.
_TEMPLATE: frozendict[ScheduleKind, ScheduleTemplate] = frozendict({
    ScheduleKind.DOOR: ScheduleTemplate(
        "DOOR SCHEDULE", "mark",
        (
            ColumnSpec("mark", "MARK", align="center"), ColumnSpec("type", "TYPE", align="center"),
            ColumnSpec("width", "W", FmtKind.NUMBER, "mm", "right"), ColumnSpec("height", "H", FmtKind.NUMBER, "mm", "right"),
            ColumnSpec("thickness", "THK", FmtKind.NUMBER, "mm", "right"), ColumnSpec("material", "MATERIAL"),
            ColumnSpec("finish", "FINISH"), ColumnSpec("glazing", "GLAZING"), ColumnSpec("fire_rating", "FIRE", align="center"),
            ColumnSpec("frame_type", "FR TYPE", align="center"), ColumnSpec("frame_material", "FR MATL"),
            ColumnSpec("hardware_set", "HW SET", align="center"), ColumnSpec("head", "HEAD", align="center"),
            ColumnSpec("jamb", "JAMB", align="center"), ColumnSpec("sill", "SILL", align="center"), ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("LEAF", ("type", "width", "height", "thickness")), SpannerSpec("FACE", ("material", "finish", "glazing")),
         SpannerSpec("FRAME", ("frame_type", "frame_material")), SpannerSpec("DETAILS", ("head", "jamb", "sill"))),
        sort="mark", color_by="fire_rating", note="Fire ratings per local code; verify hardware sets against door types.",
    ),
    ScheduleKind.WINDOW: ScheduleTemplate(
        "WINDOW SCHEDULE", "mark",
        (
            ColumnSpec("mark", "MARK", align="center"), ColumnSpec("type", "TYPE", align="center"),
            ColumnSpec("width", "W", FmtKind.NUMBER, "mm", "right"), ColumnSpec("height", "H", FmtKind.NUMBER, "mm", "right"),
            ColumnSpec("sill_height", "SILL HT", FmtKind.NUMBER, "mm", "right"), ColumnSpec("material", "MATERIAL"),
            ColumnSpec("glazing", "GLAZING"), ColumnSpec("operation", "OPERATION"),
            ColumnSpec("u_value", "U", FmtKind.NUMBER, "W/m²K", "right", 2), ColumnSpec("shgc", "SHGC", FmtKind.NUMBER, "", "right", 2),
            ColumnSpec("head", "HEAD", align="center"), ColumnSpec("jamb", "JAMB", align="center"),
            ColumnSpec("sill", "SILL", align="center"), ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("OPENING", ("type", "width", "height", "sill_height")), SpannerSpec("GLAZING", ("material", "glazing", "operation")),
         SpannerSpec("PERFORMANCE", ("u_value", "shgc")), SpannerSpec("DETAILS", ("head", "jamb", "sill"))),
        sort="mark",
    ),
    ScheduleKind.ROOM_FINISH: ScheduleTemplate(
        "ROOM FINISH SCHEDULE", "room_no",
        (
            ColumnSpec("room_no", "NO.", align="center"), ColumnSpec("room_name", "ROOM NAME"), ColumnSpec("floor", "FLOOR"),
            ColumnSpec("base", "BASE"), ColumnSpec("wall_n", "N"), ColumnSpec("wall_e", "E"), ColumnSpec("wall_s", "S"),
            ColumnSpec("wall_w", "W"), ColumnSpec("ceiling", "CEILING"),
            ColumnSpec("ceiling_height", "CLG HT", FmtKind.NUMBER, "mm", "right"), ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("WALLS", ("wall_n", "wall_e", "wall_s", "wall_w")), SpannerSpec("CEILING", ("ceiling", "ceiling_height"))),
        sort="room_no",
    ),
    ScheduleKind.WALL_TYPE: ScheduleTemplate(
        "WALL TYPE SCHEDULE", "type_mark",
        (
            ColumnSpec("type_mark", "TYPE", align="center"), ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("thickness", "THK", FmtKind.NUMBER, "mm", "right"), ColumnSpec("fire_rating", "FIRE", align="center"),
            ColumnSpec("stc", "STC", FmtKind.INTEGER, "", "right"), ColumnSpec("assembly", "ASSEMBLY"), ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("RATING", ("fire_rating", "stc")),), sort="type_mark", color_by="fire_rating",
    ),
    ScheduleKind.PARTITION: ScheduleTemplate(
        "PARTITION SCHEDULE", "type_mark",
        (
            ColumnSpec("type_mark", "TYPE", align="center"), ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("thickness", "THK", FmtKind.NUMBER, "mm", "right"), ColumnSpec("height", "HT"),
            ColumnSpec("fire_rating", "FIRE", align="center"), ColumnSpec("stc", "STC", FmtKind.INTEGER, "", "right"),
            ColumnSpec("head_condition", "HEAD"), ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("RATING", ("fire_rating", "stc")),), sort="type_mark", color_by="fire_rating",
    ),
    ScheduleKind.EQUIPMENT: ScheduleTemplate(
        "EQUIPMENT SCHEDULE", "mark",
        (
            ColumnSpec("mark", "MARK", align="center"), ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("manufacturer", "MANUFACTURER"), ColumnSpec("model", "MODEL"),
            ColumnSpec("quantity", "QTY", FmtKind.INTEGER, "", "right"), ColumnSpec("electrical", "ELEC"),
            ColumnSpec("plumbing", "PLBG"), ColumnSpec("weight", "WEIGHT", FmtKind.NUMBER, "kg", "right"), ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("PRODUCT", ("manufacturer", "model")), SpannerSpec("UTILITIES", ("electrical", "plumbing"))), sort="mark",
    ),
    ScheduleKind.PLUMBING_FIXTURE: ScheduleTemplate(
        "PLUMBING FIXTURE SCHEDULE", "mark",
        (
            ColumnSpec("mark", "MARK", align="center"), ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("manufacturer", "MANUFACTURER"), ColumnSpec("model", "MODEL"), ColumnSpec("hot_water", "HW"),
            ColumnSpec("cold_water", "CW"), ColumnSpec("waste", "WASTE"), ColumnSpec("vent", "VENT"), ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("PRODUCT", ("manufacturer", "model")), SpannerSpec("CONNECTIONS", ("hot_water", "cold_water", "waste", "vent"))), sort="mark",
    ),
    ScheduleKind.LIGHTING_FIXTURE: ScheduleTemplate(
        "LIGHTING FIXTURE SCHEDULE", "mark",
        (
            ColumnSpec("mark", "MARK", align="center"), ColumnSpec("type", "TYPE", align="center"),
            ColumnSpec("description", "DESCRIPTION"), ColumnSpec("lamp", "LAMP"),
            ColumnSpec("wattage", "W", FmtKind.INTEGER, "W", "right"), ColumnSpec("voltage", "V", FmtKind.INTEGER, "V", "right"),
            ColumnSpec("mounting", "MOUNTING"), ColumnSpec("manufacturer", "MANUFACTURER"), ColumnSpec("model", "MODEL"), ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("LAMP", ("lamp", "wattage", "voltage")), SpannerSpec("PRODUCT", ("manufacturer", "model"))), sort="mark",
    ),
    ScheduleKind.FINISH: ScheduleTemplate(
        "FINISH SCHEDULE", "code",
        (
            ColumnSpec("code", "CODE", align="center"), ColumnSpec("category", "CATEGORY"), ColumnSpec("material", "MATERIAL"),
            ColumnSpec("manufacturer", "MANUFACTURER"), ColumnSpec("product", "PRODUCT"), ColumnSpec("color", "COLOR"),
            ColumnSpec("finish", "FINISH"), ColumnSpec("size", "SIZE", align="right"), ColumnSpec("location", "LOCATION"), ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("PRODUCT", ("manufacturer", "product", "color", "finish")),), sort="code",
    ),
    ScheduleKind.HARDWARE_SET: ScheduleTemplate(
        "DOOR HARDWARE SET SCHEDULE", "set_no",
        (
            ColumnSpec("set_no", "SET", align="center"), ColumnSpec("quantity", "QTY", FmtKind.INTEGER, "", "right"),
            ColumnSpec("item", "ITEM"), ColumnSpec("manufacturer", "MANUFACTURER"), ColumnSpec("product", "PRODUCT"), ColumnSpec("finish", "FINISH", align="center"),
        ),
        sort="set_no",
    ),
    ScheduleKind.PANEL: ScheduleTemplate(
        "PANEL SCHEDULE", "circuit",
        (
            ColumnSpec("circuit", "CKT", align="center"), ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("load_va", "LOAD", FmtKind.INTEGER, "VA", "right"), ColumnSpec("poles", "P", FmtKind.INTEGER, "", "center"),
            ColumnSpec("breaker", "TRIP", FmtKind.INTEGER, "A", "right"), ColumnSpec("phase", "PH", align="center"), ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("BREAKER", ("poles", "breaker", "phase")),), sort="circuit",
    ),
    ScheduleKind.STRUCTURAL_COLUMN: ScheduleTemplate(
        "COLUMN SCHEDULE", "mark",
        (
            ColumnSpec("mark", "MARK", align="center"), ColumnSpec("size", "SIZE"), ColumnSpec("material", "MATERIAL"),
            ColumnSpec("grade", "GRADE", align="center"), ColumnSpec("base_plate", "BASE PL"), ColumnSpec("splice", "SPLICE"), ColumnSpec("remarks", "REMARKS"),
        ),
        sort="mark",
    ),
    ScheduleKind.STRUCTURAL_BEAM: ScheduleTemplate(
        "BEAM SCHEDULE", "mark",
        (
            ColumnSpec("mark", "MARK", align="center"), ColumnSpec("size", "SIZE"), ColumnSpec("material", "MATERIAL"),
            ColumnSpec("grade", "GRADE", align="center"), ColumnSpec("camber", "CAMBER", FmtKind.NUMBER, "mm", "right"),
            ColumnSpec("connection", "CONNECTION"), ColumnSpec("remarks", "REMARKS"),
        ),
        sort="mark",
    ),
    ScheduleKind.FURNITURE: ScheduleTemplate(
        "FURNITURE & EQUIPMENT SCHEDULE", "mark",
        (
            ColumnSpec("mark", "MARK", align="center"), ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("manufacturer", "MANUFACTURER"), ColumnSpec("model", "MODEL"),
            ColumnSpec("quantity", "QTY", FmtKind.INTEGER, "", "right"), ColumnSpec("location", "LOCATION"), ColumnSpec("remarks", "REMARKS"),
        ),
        (SpannerSpec("PRODUCT", ("manufacturer", "model")),), sort="mark",
    ),
    # the canonical Rasm.Bim QTO consumer: the settled per-element takeoff frame arriving over data/tabular is ROLLED
    # to its BOQ display shape (group_by division+item+material+unit, measures SUMmed, rate carried), rendered as
    # classification-DIVISION row-group blocks each closing on a per-division Subtotal, the whole sheet on the grand
    # Total, the amount column cost-shaded, and the rate/amount columns footnoted — the true bill-of-quantities form.
    ScheduleKind.QUANTITY: ScheduleTemplate(
        "QUANTITY TAKEOFF", "description",
        (
            ColumnSpec("classification", "DIVISION", align="center"), ColumnSpec("description", "DESCRIPTION"),
            ColumnSpec("material", "MATERIAL"), ColumnSpec("unit", "UNIT", align="center"),
            ColumnSpec("count", "QTY", FmtKind.INTEGER, "no.", "right"), ColumnSpec("length", "LENGTH", FmtKind.NUMBER, "m", "right", 2),
            ColumnSpec("area", "AREA", FmtKind.NUMBER, "m²", "right", 2), ColumnSpec("volume", "VOLUME", FmtKind.NUMBER, "m³", "right", 3),
            ColumnSpec("weight", "WEIGHT", FmtKind.NUMBER, "kg", "right", 1),
            ColumnSpec("unit_rate", "RATE", FmtKind.NUMBER, "", "right", 2), ColumnSpec("total_cost", "AMOUNT", FmtKind.NUMBER, "", "right", 2),
        ),
        (SpannerSpec("MEASURE", ("count", "length", "area", "volume", "weight")), SpannerSpec("COST", ("unit_rate", "total_cost"))),
        sort="description", color_by="total_cost", totals=("count", "length", "area", "volume", "weight", "total_cost"),
        group="classification", rollup=("classification", "description", "material", "unit"), carry=("unit_rate",),
        footnotes=(("unit_rate", "Rates exclude overheads, profit, and preliminaries."), ("total_cost", "BIM-derived estimate — verify before tender.")),
        note="Quantities BIM-derived and approximate; verify before pricing.",
    ),
})

# the legend kind -> its printed title; the single edit site per legend type.
_LEGEND_TITLE: frozendict[LegendKind, str] = frozendict({
    LegendKind.LINE_TYPE: "LINE TYPE LEGEND", LegendKind.HATCH_MATERIAL: "MATERIAL HATCH LEGEND",
    LegendKind.DISCIPLINE_LAYER: "DISCIPLINE LAYER LEGEND", LegendKind.SYMBOL: "SYMBOL LEGEND",
    LegendKind.ABBREVIATION: "ABBREVIATIONS", LegendKind.KEYNOTE: "KEYNOTE LEGEND",
    LegendKind.MATERIAL_FINISH: "MATERIAL LEGEND", LegendKind.GENERAL_NOTE: "GENERAL NOTES",
})

# the ISO 128 line-type -> its drafting meaning (the legend description beside the real dash swatch); TOTAL over
# the full ISO 128-2:2020 Table 1 fifteen-type LineType family so `_legend_rows` indexes every member (a 10-of-15
# slice KeyErrors the moment standard.md's LineType grows past the covered set).
_LINE_MEANING: frozendict[LineType, str] = frozendict({
    LineType.CONTINUOUS: "Visible edges and outlines", LineType.DASHED: "Hidden edges",
    LineType.DASHED_SPACED: "Hidden edges (alternate)", LineType.LONG_DASH_DOT: "Centre lines and axes of symmetry",
    LineType.LONG_DASH_DOUBLE_DOT: "Adjacent parts, alternate positions", LineType.LONG_DASH_TRIPLE_DOT: "Special surface treatment",
    LineType.DOTTED: "Hidden detail", LineType.LONG_DASH_SHORT_DASH: "Cutting and viewing planes",
    LineType.LONG_DASH_DOUBLE_SHORT_DASH: "Outlines of parts in front of a cutting plane",
    LineType.DASH_DOT: "Pitch lines and lines of symmetry", LineType.DOUBLE_DASH_DOT: "Outline of movable parts",
    LineType.DASH_DOUBLE_DOT: "Hinge lines and lines of weakness",
    LineType.DOUBLE_DASH_DOUBLE_DOT: "Outlines of adjacent parts, alternate positions",
    LineType.DASH_TRIPLE_DOT: "Boundaries of special surface treatment",
    LineType.DOUBLE_DASH_TRIPLE_DOT: "Boundaries of special requirements (alternate)",
})

# the ISO 128-50 section material -> its meaning (the legend description beside the angled section indicator).
_HATCH_MEANING: frozendict[HatchMaterial, str] = frozendict({
    HatchMaterial.STEEL: "Steel / metal in section", HatchMaterial.CONCRETE: "Cast-in-place concrete",
    HatchMaterial.CONCRETE_REINFORCED: "Reinforced concrete", HatchMaterial.MASONRY: "Masonry / brick",
    HatchMaterial.TIMBER_GRAIN: "Timber, grain direction", HatchMaterial.TIMBER_END: "Timber, end grain",
    HatchMaterial.INSULATION_THERMAL: "Thermal / acoustic insulation", HatchMaterial.EARTH: "Earth / subgrade",
    HatchMaterial.HARDCORE: "Hardcore / compacted fill", HatchMaterial.LIQUID: "Liquid", HatchMaterial.GLASS: "Glass / glazing",
})

# the ISO 13567/AIA discipline -> its name (the legend description beside the real discipline pen swatch).
_DISCIPLINE_MEANING: frozendict[Discipline, str] = frozendict({
    Discipline.ARCHITECTURAL: "Architectural", Discipline.CIVIL: "Civil", Discipline.ELECTRICAL: "Electrical",
    Discipline.FIRE: "Fire protection", Discipline.GENERAL: "General", Discipline.HAZMAT: "Hazardous materials",
    Discipline.INTERIORS: "Interiors", Discipline.LANDSCAPE: "Landscape", Discipline.MECHANICAL: "Mechanical",
    Discipline.PLUMBING: "Plumbing", Discipline.EQUIPMENT: "Equipment", Discipline.RESOURCE: "Resource",
    Discipline.STRUCTURAL: "Structural", Discipline.TELECOM: "Telecommunications", Discipline.SURVEY: "Survey / mapping",
    Discipline.PROCESS: "Process", Discipline.OTHER: "Other disciplines", Discipline.CONTRACTOR: "Contractor / shop",
})

# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "ColumnSpec", "LegendEntry", "LegendKind", "Schedule", "ScheduleContent", "ScheduleKind", "ScheduleTemplate", "SpannerSpec",
]
```

`Schedule` is the one AEC-scheduling owner every door/window/room-finish/equipment/panel/structural schedule, the BIM-derived quantity takeoff, and every ISO-drafting legend is built from: the `ScheduleContent` `tabular` case carries a settled `polars.DataFrame` (the QTO/schedule rows arriving over the `data/tabular` wire from `csharp:Rasm.Bim`, styled here, never authored here) plus a `ScheduleKind` selecting the NCS/AIA column template, and the `legend` case carries a `LegendKind` selecting which `drawing/standard#STANDARD` owned vocabulary to enumerate into a symbol→meaning table. `_schedule_ops` DERIVES the whole `visualization/table#TABLE` `TableOp` sequence from the one `_TEMPLATE` row — headers, spanners, per-column `FmtKind`, missing-value substitution, `hex_ramp`-derived cost coloring, a division-sorted layout, the grand `GrandSummary` total, and column-anchored `Footnote` marks — filtered to the columns the shaped frame carries so a subset/rolled QTO frame never references an absent column; `_schedule_frame` rolls a per-element takeoff up to its BOQ display shape (`group_by(*rollup).agg`, `@beartype`-admitted at the two constructors' seam), and `_legend_rows` reads the real ISO code (the `LineType.pattern` dash array, the `Standard.hatch` section angle, the `Standard.rgb` discipline sRGB) into a `drawsvg`-authored swatch. The one `TablePlan.build` render is the single bytes fact keying the artifact and the `ArtifactReceipt.Schedule` receipt, the synchronous fold offloads onto `to_thread` off the event loop, and the flat table bytes are the handoff `composition/compose#COMPOSE` places — computing no sheet placement, holding no `great-tables` surface, and re-authoring no IFC.

## [03]-[RESEARCH]

- [SCHEDULE_LOWERS_INTO_TABLE] [RESOLVED]: the AEC schedule is NOT a re-implemented table renderer — it LOWERS its template + frame into `visualization/table#TABLE` `TablePlan` and composes the `TablePlan.build` bytes render, holding NO `great-tables` `GT` surface (the boundary is law: `TablePlan` owns `GT(frame).style` + the `TableOp` fold + the HTML/LaTeX/PDF emit; `Schedule` owns only the `ScheduleKind` → `TableOp` derivation). `TablePlan.build` is the public bytes seam a composing owner renders through, so `Schedule` mints its own `ArtifactReceipt.Schedule` off the ONE bytes fact rather than reaching a private render (SINGLE-FACT: the bytes, the content key, and the receipt all derive from the single `build` call). Justified on CONSUMER (the brief `[04]` "lowering into visualization/table" + the `[07]` one-render seam) and PACKAGE (the verified `TablePlan`/`TableOp`/`Theme`/`TableFormat`/`FmtKind` surface). Close-condition: `visualization/table.md` `TablePlan._build` is exposed as the public `build` and `_emit` composes it — a `visualization/table.md` cross-file rename (the receipt still mints in `_emit`, so `TablePlan.render` is unchanged).
- [SETTLED_FRAME_NOT_AUTHORED] [RESOLVED]: the tabular schedule's `polars.DataFrame` is a SETTLED input arriving over the `data/tabular` wire (the QTO/schedule rows the C# `Rasm.Bim` graph emits, per the brief `[09]` boundary), so `_schedule_frame` only `select`s the template columns in template order and `sort`s by the key — it authors no data, re-opens no lazy engine, and re-implements no transform the `data` owner owns, exactly the `polars` artifacts-overlay law (`DataFrame.select`/`.sort`/`.columns`/`.height` are boundary shaping, `pl.col(...).count()` the summary expr). The legend arm is the one place a frame is AUTHORED, and it authors from the OWNED `drawing/standard#STANDARD` ISO vocabulary (not external data) through `pl.from_dicts`, legitimate exactly as `standard.md` authors its own ISO tables. Justified on DOMAIN (a QTO schedule is BIM-derived data the drawing plane formats, not authors) and PACKAGE (the verified `polars` overlay boundary surface).
- [OWNED_SCHEDULE_VOCABULARY] [RESOLVED]: the AEC schedule templates are an OWNED vocabulary, not a package — no library owns the NCS/AIA/CSI door/window/room-finish/equipment/panel/structural column conventions, so `_TEMPLATE` authors each to its real column cardinality (a door schedule owns the full 16-column leaf/face/frame/fire/hardware/detail set with its four column-group spanners, not a naive two-column stub), exactly as `drawing/standard#STANDARD` owns the ISO drafting tables and `visualization/diagram/glyphset#GLYPHSET` owns the diagram primitives. The `ScheduleTemplate`/`ColumnSpec`/`SpannerSpec` value objects and the `_schedule_ops` fold turn the one template row into the full `TableOp` sequence, so a new schedule is one `_TEMPLATE` row and a new column is one `ColumnSpec`. Justified on DOMAIN (15 real AEC schedule types, each a standards column set) and CONSUMER (`composition/sheet#SHEET` places the rendered schedule beside its drawing figures).
- [QUANTITY_TAKEOFF] [RESOLVED]: the fifteenth `ScheduleKind` is `QUANTITY` — the BIM-derived quantity takeoff, the canonical `csharp:Rasm.Bim` QTO consumer the brief `[01]`/`[09]` legislate ("the QTO/schedule rows arriving over the `data/tabular` wire from `csharp:Rasm.Bim`"). The prior fourteen were all DESIGN schedules (door/window/room-finish/equipment/panel/structural); a schedule owner whose telos is "the foundation of a high-end AEC documentation engine" that formats BIM data yet carries no takeoff template is under-built against its own consumer, so `QUANTITY` closes that gap in place — one `ScheduleKind` member plus one `_TEMPLATE` row modelling the real takeoff cardinality (mark/description/`specification/classify#CLASSIFY` classification code/material, the `MEASURE` count/length/area/volume/weight spanner, the `COST` unit-rate/amount spanner, classification-sorted). A takeoff MUST close on totals, so `ScheduleTemplate.totals` names the columns the `_schedule_ops` grand-summary SUMs (`pl.col(column).sum()`) beside the key count — a new capability the whole vocabulary shares (any schedule with a `totals` tuple gets summed totals, empty `totals` keeps the bare count), not a QTO-only branch. The frame stays SETTLED: `_schedule_frame` only `select`/`sort`s the takeoff columns the C# graph emits, authoring no quantity, so the C#-side IFC boundary the brief `[09]` fixes holds. Justified on CONSUMER (the `Rasm.Bim` QTO/`data/tabular` seam), DOMAIN (a quantity takeoff is the canonical BIM-derived schedule with summed measures and cost), and PACKAGE (the `pl.col(...).sum()` grand-summary expression the `visualization/table#TABLE` `GrandSummary` op composes).
- [LEGEND_SWATCH_FROM_STANDARD] [RESOLVED]: the derived legends read the REAL ISO code into a `drawsvg`-authored swatch — `LINE_TYPE` folds each `LineType.pattern` ISO 128 dash array into a `drawsvg.Line` `stroke_dasharray` sample (a new public `LineType.pattern` property on `drawing/standard#STANDARD` reading its private `_LINETYPE` dash table, idiomatic beside the existing `LineWeight.group`/`ScaleRatio.ratio`/`TextHeight.mm` enum properties), `DISCIPLINE_LAYER` folds each `Standard.rgb(LayerName.of(discipline, "XXXX"))` pen sRGB into a filled `drawsvg.Rectangle`, and `HATCH_MATERIAL` folds each `Standard.hatch(material).angle` into an angled crosshatch INDICATOR (the characteristic section angle, honestly NOT the full `ezdxf` `set_pattern_fill` render the section producer owns). The swatch column renders inline through `TableOp.Fmt(FmtKind.MARKDOWN)` so the HTML egress carries the sample; every swatch is authored through `drawsvg` structured primitives, never an f-string SVG splice (TEMPLATE-SAFETY). Justified on PACKAGE (the verified `drawsvg.Drawing`/`Rectangle`/`Line`/`as_svg` surface + the `drawing/standard#STANDARD` `LineType`/`HatchSpec`/`Standard.rgb`/`Standard.hatch` codes) and DOMAIN (a legend SHOWS the real ISO sample beside its meaning). Close-condition: `drawing/standard.md` `LineType` carries a `.pattern` property returning its `_LINETYPE` dash array.
- [RECEIPT_SCHEDULE_CASE] [RESOLVED]: the owner contributes a new shared `core/receipt#RECEIPT` `ArtifactReceipt.Schedule(key, kind, rows, columns, format, bytes)` case (the schedule/legend `kind`, the scheduled-item count, the column count, the `TableFormat`, and the rendered byte count) — the brief `[07]` legislates schedule evidence as a NEW case on the existing family (beside `drawing`/`spec`/`delivery`), NOT the thinner `visualization/table#TABLE` `ArtifactReceipt.Table(key, format, bytes)` case which loses the AEC schedule kind and the row/column counts. It rides the `ReceiptContributor` port through the shared `contribute` fold exactly as every producer does, never a parallel receipt rail. Justified on CONSUMER (the brief `[07]` AEC-case seam + a schedule index reader keying on the schedule kind and item count). Close-condition: `core/receipt.md` grows the `Schedule` case + mint + `_KEYS["schedule"]` row + the `slot`/`_facts` or-pattern arms, keeping the `assert_never` exhaustiveness gate.
- [OFFLOAD_LANE] [RESOLVED]: the whole lower-shape-render fold is synchronous CPU work (the `polars` frame `select`/`group_by`/`sort`, the `great-tables` `GT` fold + HTML/LaTeX render, the `drawsvg` swatch author), so `resolve` crosses ONE `to_thread.run_sync(_render, self, limiter=_LANES)` seam off the event loop in the shared address space — the same `CapacityLimiter`-bounded thread lane `drawing/symbol#SYMBOL` and `visualization/diagram/draw#DRAW` take, because the `polars`/`numpy` palette and the `msgspec`-backed `ArtifactReceipt` owners a `to_interpreter` isolate cannot load force the thread arm over the subinterpreter arm. A synchronous `great-tables` render inline on the loop is the event-loop-starvation defect this refuses. Justified on the `concurrency.md#OFFLOAD_LANE` law and the sibling drawing-plane lane precedent.
- [BOQ_ROLLUP_AND_GRAND_TOTAL] [RESOLVED]: the `QUANTITY` takeoff lowered a FLAT per-element dump — a bill of quantities is canonically the DIVISION-sorted, item-ROLLED, totalled form (SMM7/NRM2/CESMM), so the prior flat layout under-realized its own consumer. `ScheduleTemplate.rollup`/`carry` (`_schedule_frame` composes `group_by(*rollup, maintain_order=True).agg(pl.col(m).sum(), pl.col(c).first())` — the same `Reshape.GroupAgg` shape `visualization/table#TABLE` owns, applied at the schedule boundary exactly as the existing `select`/`sort` are, aggregating the settled values for DISPLAY and authoring none) rolls the settled element frame to one row per (classification, description, material, unit) with the measures SUMmed and the rate carried; the frame sorts division-adjacent, the division rides as a regular `DIVISION` column, and one `GrandSummary({"Total": pl.col(*present_totals).sum()})` closes the sheet — `pl.col(*cols).sum()` the ONE multi-column aggregate great-tables lands as a SINGLE summary row (replacing the prior per-column `{col: sum}` bag that fragments into one row per measure; verified rendering on a rowname-only polars GT). The settled per-element rows stay upstream at `csharp:Rasm.Bim` — the schedule DISPLAYS the rollup, never authoring quantity, so the brief `[09]` C# boundary holds. Justified on DOMAIN (a BOQ is a division-sorted item rollup closing on a total), CONSUMER (`visualization/table#TABLE` `Reshape.GroupAgg`/`GrandSummary` verified + folded), and PACKAGE (`polars` `group_by().agg`/`pl.col(*cols).sum` verified rendering).
- [GREAT_TABLES_POLARS_RENDER_BLOCK] [BLOCKED]: the per-division row-group subtotal form (`Stub(rowname=key, group=division)` + `RowGroupOrder` + per-division `Summary({"Subtotal": pl.col(*measures).sum()}, groups=[division])`) is the STRONGER BOQ layout the reading map named and is composed on the settled `visualization/table#TABLE` `TableOp` surface — but it does NOT RENDER on the pinned `great-tables 0.22.0` + `polars 1.42.0`: verified that `fmt_number`/`fmt_integer(columns=...)` AND `tab_stub(groupname_col=)` BOTH raise `polars.exceptions.OutOfBoundsError`/`IndexError` on ANY polars-backed `GT` (even the canonical gtcars example), while the identical calls RENDER on a pandas-backed `GT`. This is a CORPUS-WIDE `visualization/table#TABLE` render-path defect (every `TableOp.Fmt` consumer — `schedule`, `visualization/chart`, `document/report` — is blocked on polars), not a schedule-local one; 0.22.0 is the latest release, so no version bump fixes it. The fix is one line in `visualization/table#TABLE` `build()`: materialize the shaped frame to pandas for the `GT` construction (`GT(shaped.to_pandas(), ...)` / `shaped.to_pandas().style`), after which the full grouped BOQ (`fmt` + `groupname_col` + `summary_rows(groups=)`) renders (pandas verified). Recorded as a `visualization/table#TABLE` + manifest residual; the schedule keeps the render-safe division-sorted + grand-total form until the seam materializes to pandas.
- [COLUMN_FOOTNOTES] [RESOLVED]: a schedule carried only one blanket `SourceNote` — a standards caveat that qualifies a SPECIFIC column (a fire rating "verify with local code", a cost rate "excludes overheads") is a column-anchored footnote MARK, not a footer sentence. `ScheduleTemplate.footnotes: tuple[Footnote, ...]` derives one `TableOp.Footnote(text, at=StubLoc.COLUMN_LABELS, columns=[column])` per row, and `_AEC_THEME` carries `footnote_marks=FootnoteMarks.STANDARD` so the marks typeset as the ISO-drafting †/‡/§ symbol sequence (`opt_footnote_marks`) rather than the numeric default a symbol legend misreads as a callout number. The `SourceNote` stays for the general note; footnotes carry the column-specific caveats. Justified on DOMAIN (a drawing footnote qualifies a specific field) and PACKAGE (great-tables `tab_footnote(placement=)`/`opt_footnote_marks` + `visualization/table#TABLE` `TableOp.Footnote`/`FootnoteMarks`/`StubLoc.COLUMN_LABELS` verified).
- [INGRESS_CONTRACT] [RESOLVED]: the two public constructors admitted a raw `pl.DataFrame`/`ScheduleKind`/`Palette` with no boundary contract — the sibling `drawing/standard#STANDARD` (`@beartype` on `LayerName.of`/`SheetId.of`) and `visualization/table#TABLE`/`exchange/conformance` owners all weave a definition-time ingress contract, and its absence here left a malformed kind or a non-frame to fault deep in the `polars`/`great-tables` fold. `@beartype(conf=_INGRESS)` (a shared `BeartypeConf(violation_type=ValueError)`) now guards `Schedule.tabular`/`Schedule.legend`, redirecting a shape violation to a `ValueError` the `resolve` `async_boundary(catch=_FAULTS)` already narrows — the rail-safe admission form (never a raw throw into the interior), matching the `shapes.md` `BeartypeConf(violation_type=...)` admission-factory law. Justified on CONSUMER (the sibling owners' woven ingress contract) and PACKAGE (`beartype` `BeartypeConf`/`beartype` verified).
