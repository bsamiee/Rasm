# [PY_ARTIFACTS_TABLE]

`TablePlan` is the publication-and-schedule tabular-artifact owner — the missing third artifact pillar beside documents and charts — graded at once as a journal-grade publication primitive and an ISO-drafting schedule/QTO primitive. One owner over great-tables (bring-your-own-DataFrame, polars first-class through `DataFrame.style`) admits a settled frame or the C# `Rasm.Bim` QTO/schedule wire over `data/tabular`, shapes it to its display cross-tab through the closed `Reshape` polars pre-pass, then styles it into journal output — spanners, scientific formatting, uncertainty merges, substitution, configured nanoplots, data-driven coloring, footnote marks, and `opt_*` theme identity — exported host-free to HTML/LaTeX and gated PDF.

Frame arrives settled; `TablePlan` reshapes-to-display, styles, and content-keys it, never authoring the data, re-opening the lazy engine, or re-implementing a `data`-owned transform. Measured results and BIM quantity takeoffs both render as formatted tables feeding `document/report#REPORT` and `document/emit#DOCUMENT`; `drawing/schedule#SCHEDULE` lowers its AEC templates into this owner and composes the same `build()` bytes fact to mint its own `ArtifactReceipt.Schedule`, so a schedule lowers in with no private-method reach. Flat HTML/SVG egress hands to `composition/compose#COMPOSE` for placement beside sibling graphics.

## [01]-[INDEX]

- [01]-[TABLE]: great-tables styled-table owner over HTML/LaTeX/PDF, one closed `TableOp` family folded over an ordered op sequence, preceded by the closed `Reshape` polars display-shaping pre-pass and content-keyed for a byte-stable render.

## [02]-[TABLE]

- Owner: `TablePlan` discriminates export format over the closed `TableFormat` (`HTML`/`LATEX`/`PDF`); `Reshape` and `TableOp` are `expression.tagged_union` families each carrying one typed payload per case — `Reshape` folded `pl.DataFrame -> pl.DataFrame` before `.style`, `TableOp` folded onto the great-tables `GT` builder — dispatched by one total `match`, never a parallel `dict[str, Callable]` catalogue or a per-feature `reduce`. `Theme` is the closed `opt_*` stylize vocabulary carried as the table's named publication identity, its `FootnoteMarks` axis selecting numeric, alphabetic, or symbol mark sequences.
- Cases: `Reshape` rows land the display cross-tab a raw QTO/schedule frame is not yet in — `Select`/`Filter`/`Sort`/`Rename`/`Cast`/`Head`/`Slice`/`With`, `GroupAgg` quantity rollup, `Pivot` (the door/window/room-finish cross-tab), `Unpivot`, `TopK`, and `Derive` (a display column the frame did not carry) — each a `pl.Expr`-carrying case folded by one total `_shape` match, so a schedule rollup is one row and never a Python loop over cells. `TableOp` folds the full publication surface — structure, column control, spanners, `Fmt` value format over the `FmtKind` table, `Nanoplot`, column merges, substitutions, cell transforms, aggregation, data-driven `Color`, and raw `Css` — each arm composing the verified `GT` member directly. One `Fmt` case carrying a `FmtKind` discriminant collapses every `fmt_*` verb plus the arbitrary `fmt(fns, is_substitution)` custom verb onto one fold over `FMT_TABLE`, since every member shares the `(columns, rows, **opts)` shape.
- Entry: `TablePlan.of` admits raw material exactly once — a settled `pl.DataFrame` passes through, an Arrow-C-stream / interchange capsule (the C# `Rasm.Bim` QTO/schedule egress over `data/tabular`, or any `data` producer) normalizes through `pl.from_dataframe` zero-copy where the layout permits — so the interior sees only a settled frame, never re-validated inward, and one parameterized `[WIRE]` edge sources across providers without touching the interior. `emit()` returns the `ArtifactWork` node with a PRE-RUN input key; `build()` is the synchronous bytes seam a composing AEC owner renders through directly, so `drawing/schedule#SCHEDULE` mints its own receipt off the same single render with no private-method reach.
- Auto: `build` is one `pl.Config` scope over an ordered fold — `_shape` reshapes the frame to its cross-tab, `Theme.apply` seeds the `opt_*` identity, each `TableOp` case folds its one `GT` member. `maintain_order=True`/`sort_columns=True` on the group/pivot rows fix row/column order and the fold runs inside the render scope, so a float-to-display `Derive` renders at fixed precision and the display frame is byte-reproducible for the content key. `_stable_id` keys the render off the frame row-hash plus format plus op-tag digest through `blake2b`, because great-tables mints `random_id()` when `id` is `None`, drifting the rendered bytes and the `ContentKey` every render. `_place` is one `LOC_TABLE` projection from `StubLoc` to the verified per-selector arg-arity through the `LocArity` discriminant, never a branch on which location admits `columns`/`rows`/`ids`/`groups`; `_font` discriminates `FontFace` over the closed google/system/family axis; the `Color` arm derives text color from background luminance via `autocolor_text=True`; `Nanoplot` folds the `NanoSpec` value object over `fmt_nanoplot`; `TablePlan.Series`/`Units` are `staticmethod` projections off the `GT`-chain seam.
- Receipt: each render contributes the shared `core/receipt#RECEIPT` `ArtifactReceipt.Table(key, format, bytes)` — content key, `TableFormat` value, and byte count off the one `build()` render — through the runtime `ReceiptContributor` port, the LEAF-producer pattern where the receipt IS the return, mirroring the sibling chart and raster producers rather than the `composition/compose#COMPOSE` placement-owner's `of()` plus separate `contribute()`. `drawing/schedule#SCHEDULE` composes the same `build` bytes fact to mint its own `ArtifactReceipt.Schedule` (single-fact: one render, one content key).
- Packages: `great-tables` (the `GT` builder and its full `tab_*`/`cols_*`/`fmt_*`/`sub_*`/`text_*`/`opt_*`/`data_color`/`vals.fmt_*` surface plus the `loc.*` selectors, `style.*` constructors, and `nanoplot_options`/`define_units`/`from_column`/`google_font`/`system_fonts` helpers), `polars` (the `DataFrame.style -> GT` seam, the `Reshape` frame ops, `pl.from_dataframe` interchange ingress, `polars.selectors` for the `Cols` dtype-class selector, `pl.Config` render scope, `hash_rows` id seed, `PolarsError` the `_FAULTS` base), `anyio` (`CapacityLimiter` thread lane), `hashlib` (`blake2b` id digest), runtime (`ContentIdentity`/`ContentKey`, `RuntimeRail`/`async_boundary(catch=_FAULTS)`), `core/receipt#RECEIPT` (`ArtifactReceipt.Table`).
- Growth: a new frame-shaping transform is one `Reshape` case plus one `_shape` arm; a new export format is one `TableFormat` row; a new format verb is one `FmtKind` row in `FMT_TABLE` (and one `VALS_TABLE` row where the standalone Series formatter exists); a new structural, merge, substitution, transform, or aggregation feature is one `TableOp` case plus one `match` arm; a new emit knob is one `TablePlan` policy field; a new location target is one `StubLoc` row in `LOC_TABLE`; a new theme axis is one `Theme` field; a new footnote-mark family is one `FootnoteMarks` member; a new font source is one `FontFace` row; a new provider ingress is one `FrameSource` arm in `of`; an unrowed package call is one `Pipe` `GT -> GT`.
- Boundary: no raw data interchange authoring (that stays at `data`) — the frame arrives settled over the `data/tabular` wire and `_shape` only re-projects/rolls-up/cross-tabs it for display, `_seam` keeping the `DataFrame.style` accessor as the default path and falling to the explicit `GT(...)` constructor only when `rowname_col`/`groupname_col`/`locale`/`auto_align` diverges. HTML and LaTeX emit knobs ride `TablePlan` policy fields so every emit literal traces to a field default, both host-free no-driver byte paths; the emitted HTML/SVG is the flat handoff `composition/compose#COMPOSE` reads, re-rendering nothing. PDF `save` rides a host-coupled Chrome/Selenium WebDriver rounded through a temp file — gated optional, never the default, the one remaining gated host path, a host-free PDF instead routing this owner's HTML through `document/emit#DOCUMENT`. great-tables' `random_id()` default, a per-feature method chain, a per-cell imperative loop, a Python row loop where a `Reshape` expr shapes the frame, a re-opened lazy engine, a parallel format/style/location catalogue, and a branch on location kind are the rejected forms.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import os
from collections.abc import Callable, Iterable
from enum import Enum, StrEnum, auto
from functools import reduce
from hashlib import blake2b
from pathlib import Path
from tempfile import NamedTemporaryFile
from typing import Any, Literal, Protocol, Self, assert_never, runtime_checkable

import polars as pl
import polars.selectors as cs
from polars.exceptions import PolarsError
from expression import case, tag, tagged_union
from great_tables import GT, define_units, from_column, google_font, loc, md, nanoplot_options, style, system_fonts, vals
from great_tables._locations import Loc
from great_tables._styles import CellStyle, ColumnExpr
from msgspec import Struct

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt

# --- [TYPES] ----------------------------------------------------------------------------
# `Cols` admits a `polars.selectors` class (`cs.numeric()` IS a `pl.Expr`) so a Fmt/Align/Color/Style op
# addresses a dtype class without a hardcoded name list.
type Cols = str | list[str] | cs.Selector | None
type Rows = int | list[int] | None
type Mask = pl.Expr | None
type Groups = list[str] | str | None
type Predicate = Callable[[str], bool]
type FmtFn = Callable[[Any], str]
type Bound = str | ColumnExpr
type StyleSpec = tuple[Literal["text", "fill", "borders", "css"], dict[str, Bound]]
type Place = tuple["StubLoc", Cols, Rows, Mask, Groups]
type FmtMember = Callable[[GT, Cols, Rows, dict[str, Any]], GT]
type FontFace = tuple[Literal["google", "system", "family"], str]
type Aggregate = str | pl.Expr | None


# the C# `Rasm.Bim` QTO/schedule egress (or any `data` producer) crosses as an Arrow-C-stream capsule
# `pl.from_dataframe` admits zero-copy where possible; a settled `pl.DataFrame` is the other ingress arm.
@runtime_checkable
class ArrowStream(Protocol):
    def __arrow_c_stream__(self, requested_schema: object = None, /) -> object: ...


type FrameSource = pl.DataFrame | ArrowStream


class TableFormat(StrEnum):
    HTML = "html"
    LATEX = "latex"
    PDF = "pdf"


class LocArity(Enum):
    NONE = auto()
    COLUMNS = auto()
    IDS = auto()
    ROWS = auto()
    CELLS = auto()
    GROUPED = auto()
    GROUP_ROWS = auto()
    MASKED = auto()


class FmtKind(StrEnum):
    NUMBER = "number"
    INTEGER = "integer"
    CURRENCY = "currency"
    PERCENT = "percent"
    SCIENTIFIC = "scientific"
    ENGINEERING = "engineering"
    PARTSPER = "partsper"
    UNITS = "units"
    DURATION = "duration"
    BYTES = "bytes"
    ROMAN = "roman"
    DATE = "date"
    DATETIME = "datetime"
    TIME = "time"
    TF = "tf"
    FLAG = "flag"
    ICON = "icon"
    IMAGE = "image"
    MARKDOWN = "markdown"
    CUSTOM = "custom"


class StubLoc(StrEnum):
    BODY = "body"
    COLUMN_LABELS = "column_labels"
    COLUMN_HEADER = "column_header"
    SPANNER_LABELS = "spanner_labels"
    STUB = "stub"
    STUBHEAD = "stubhead"
    ROW_GROUP = "row_group"
    ROW_GROUPS = "row_groups"
    HEADER = "header"
    FOOTER = "footer"
    TITLE = "title"
    SUBTITLE = "subtitle"
    SOURCE_NOTES = "source_notes"
    SUMMARY = "summary"
    SUMMARY_STUB = "summary_stub"
    GRAND_SUMMARY = "grand_summary"
    GRAND_SUMMARY_STUB = "grand_summary_stub"


# each member projects to the `opt_footnote_marks(marks=...)` keyword great-tables expands: alphabetic to
# `letters()`/`LETTERS()`, `STANDARD`/`EXTENDED` to the symbol sets; a custom `tuple[str, ...]` rides the `list[str]` form.
class FootnoteMarks(StrEnum):
    NUMBERS = "numbers"
    LETTERS = "letters"
    UPPER_LETTERS = "LETTERS"
    STANDARD = "standard"
    EXTENDED = "extended"


# --- [CONSTANTS] ------------------------------------------------------------------------
# the real engine raise tuple the leaf boundary narrows `catch` to: `PolarsError` (base of every `_shape` frame op),
# great-tables' render `ValueError`/`KeyError`/`NotImplementedError`, and the gated PDF `OSError` — so a non-engine
# raise and cancellation propagate as a defect, never railing through the `Exception` catch-all.
_FAULTS: tuple[type[Exception], ...] = (PolarsError, ValueError, KeyError, NotImplementedError, OSError)


# --- [MODELS] ---------------------------------------------------------------------------
class Theme(Struct, frozen=True):
    style: int = 1
    color: str = "blue"
    striping: bool = True
    font: FontFace | None = None
    header_align: str = "center"
    all_caps: bool = False
    vertical_scale: float = 1.0
    horizontal_scale: float = 1.0
    outline: tuple[str, str, str] | None = None
    footnote_marks: FootnoteMarks | tuple[str, ...] = FootnoteMarks.NUMBERS
    css: str | None = None

    @staticmethod
    def google(name: str) -> FontFace:
        return ("google", name)

    @staticmethod
    def system(name: str) -> FontFace:
        return ("system", name)

    @staticmethod
    def family(name: str) -> FontFace:
        return ("family", name)

    def _marks(self) -> str | list[str]:
        return list(self.footnote_marks) if isinstance(self.footnote_marks, tuple) else self.footnote_marks.value

    def apply(self, gt: GT) -> GT:
        themed = gt.opt_stylize(style=self.style, color=self.color, add_row_striping=self.striping)
        fonted = _font(themed, self.font) if self.font else themed
        outlined = fonted.opt_table_outline(style=self.outline[0], width=self.outline[1], color=self.outline[2]) if self.outline else fonted
        cssed = outlined.opt_css(css=self.css) if self.css else outlined
        return (
            cssed
            .opt_row_striping(row_striping=self.striping)
            .opt_align_table_header(align=self.header_align)
            .opt_all_caps(all_caps=self.all_caps)
            .opt_vertical_padding(scale=self.vertical_scale)
            .opt_horizontal_padding(scale=self.horizontal_scale)
            .opt_footnote_marks(marks=self._marks())
        )


class NanoSpec(Struct, frozen=True):
    plot_type: Literal["line", "bar"] = "line"
    plot_height: str = "2em"
    missing_vals: Literal["marker", "gap", "zero", "remove"] = "gap"
    autoscale: bool = False
    reference_line: str | int | float | None = None
    reference_area: list[Any] | None = None
    expand_x: list[float] | None = None
    expand_y: list[float] | None = None
    options: dict[str, Any] | None = None

    def fold(self, gt: GT, columns: Cols, rows: Rows) -> GT:
        return gt.fmt_nanoplot(
            columns=columns,
            rows=rows,
            plot_type=self.plot_type,
            plot_height=self.plot_height,
            missing_vals=self.missing_vals,
            autoscale=self.autoscale,
            reference_line=self.reference_line,
            reference_area=self.reference_area,
            expand_x=self.expand_x,
            expand_y=self.expand_y,
            options=nanoplot_options(**self.options) if self.options else None,
        )


# the closed frame-shaping pre-pass — one polars transform per case, folded `pl.DataFrame -> pl.DataFrame`
# BEFORE `.style` so a raw QTO/schedule frame lands its display cross-tab columnar, never a Python row loop.
@tagged_union(frozen=True)
class Reshape:
    tag: Literal[
        "select", "filter", "sort", "rename", "cast", "head", "slice", "with_columns", "derive", "group_agg", "pivot", "unpivot", "top_k"
    ] = tag()
    select: tuple[pl.Expr | str, ...] = case()
    filter: tuple[pl.Expr, ...] = case()
    sort: tuple[str | list[str], bool] = case()
    rename: frozendict[str, str] = case()
    cast: frozendict[str, pl.DataType] = case()
    head: int = case()
    slice: tuple[int, int] = case()
    with_columns: tuple[pl.Expr, ...] = case()
    derive: tuple[pl.Expr, ...] = case()
    group_agg: tuple[tuple[str, ...], tuple[pl.Expr, ...]] = case()
    pivot: tuple[str | list[str], str | list[str] | None, str | list[str], Aggregate] = case()
    unpivot: tuple[str | list[str], str | list[str] | None, str, str] = case()
    top_k: tuple[int, str | list[str]] = case()

    @staticmethod
    def Select(*columns: pl.Expr | str) -> "Reshape":
        return Reshape(select=columns)

    @staticmethod
    def Filter(*predicates: pl.Expr) -> "Reshape":
        return Reshape(filter=predicates)

    @staticmethod
    def Sort(by: str | list[str], descending: bool = False) -> "Reshape":
        return Reshape(sort=(by, descending))

    @staticmethod
    def Rename(mapping: frozendict[str, str]) -> "Reshape":
        return Reshape(rename=mapping)

    @staticmethod
    def Cast(dtypes: frozendict[str, pl.DataType]) -> "Reshape":
        return Reshape(cast=dtypes)

    @staticmethod
    def Head(n: int) -> "Reshape":
        return Reshape(head=n)

    @staticmethod
    def Slice(offset: int, length: int) -> "Reshape":
        return Reshape(slice=(offset, length))

    @staticmethod
    def With(*exprs: pl.Expr) -> "Reshape":
        return Reshape(with_columns=exprs)

    @staticmethod
    def Derive(*exprs: pl.Expr) -> "Reshape":
        return Reshape(derive=exprs)

    @staticmethod
    def GroupAgg(keys: tuple[str, ...], aggs: tuple[pl.Expr, ...]) -> "Reshape":
        return Reshape(group_agg=(keys, aggs))

    @staticmethod
    def Pivot(on: str | list[str], values: str | list[str], index: str | list[str] | None = None, aggregate: Aggregate = "first") -> "Reshape":
        return Reshape(pivot=(on, index, values, aggregate))

    @staticmethod
    def Unpivot(on: str | list[str], index: str | list[str] | None = None, variable: str = "variable", value: str = "value") -> "Reshape":
        return Reshape(unpivot=(on, index, variable, value))

    @staticmethod
    def TopK(k: int, by: str | list[str]) -> "Reshape":
        return Reshape(top_k=(k, by))


@tagged_union(frozen=True)
class TableOp:
    tag: Literal[
        "header",
        "stub",
        "stubhead",
        "source_note",
        "footnote",
        "label",
        "label_with",
        "label_rotate",
        "align",
        "width",
        "hide",
        "unhide",
        "reorder",
        "move",
        "move_ends",
        "spanner",
        "spanner_delim",
        "fmt",
        "nanoplot",
        "merge_range",
        "merge_uncert",
        "merge_n_pct",
        "merge",
        "sub_missing",
        "sub_zero",
        "sub_small",
        "sub_large",
        "sub_values",
        "text_transform",
        "text_case_when",
        "text_case_match",
        "text_replace",
        "summary",
        "grand_summary",
        "row_group_order",
        "style",
        "color",
        "css",
        "pipe",
    ] = tag()
    header: tuple[str, str | None, list[str] | None] = case()
    stub: tuple[str | None, str | None] = case()
    stubhead: str = case()
    source_note: str = case()
    footnote: tuple[str, Place, Literal["auto", "left", "right"]] = case()
    label: dict[str, str] = case()
    label_with: tuple[Cols, Callable[[str], str]] = case()
    label_rotate: tuple[Cols, str, str, int] = case()
    align: tuple[str, Cols] = case()
    width: dict[str, str] = case()
    hide: Cols = case()
    unhide: Cols = case()
    reorder: list[str] = case()
    move: tuple[Cols, str] = case()
    move_ends: tuple[Cols, Literal["start", "end"]] = case()
    spanner: tuple[str, Cols, str | list[str] | None, int | None, str | None, bool, bool] = case()
    spanner_delim: tuple[str, Cols, Literal["first", "last"], int, bool] = case()
    fmt: tuple[FmtKind, Cols, Rows, dict[str, Any]] = case()
    nanoplot: tuple[Cols, Rows, "NanoSpec"] = case()
    merge_range: tuple[str, str, str | None, Rows, bool] = case()
    merge_uncert: tuple[str, str, str, Rows, bool] = case()
    merge_n_pct: tuple[str, str, Rows, bool] = case()
    merge: tuple[Cols, str, Cols, Rows] = case()
    sub_missing: tuple[Cols, str | None] = case()
    sub_zero: tuple[Cols, str] = case()
    sub_small: tuple[Cols, float, str | None, str] = case()
    sub_large: tuple[Cols, float, str, str] = case()
    sub_values: tuple[Cols, Predicate, str] = case()
    text_transform: tuple[Place, Callable[[str], str]] = case()
    text_case_when: tuple[Place, tuple[tuple[Predicate, str], ...], str | None] = case()
    text_case_match: tuple[Place, tuple[tuple[str | list[str], str], ...], str | None, Literal["all", "partial"]] = case()
    text_replace: tuple[Place, str, str] = case()
    summary: tuple[dict[str, pl.Expr], FmtFn | None, list[str] | None, Literal["bottom", "top"], str] = case()
    grand_summary: tuple[dict[str, pl.Expr], FmtFn | None, Literal["bottom", "top"], str] = case()
    row_group_order: list[str] = case()
    style: tuple[Place, tuple[StyleSpec, ...]] = case()
    color: tuple[Cols, Rows, str | list[str], list[float] | list[str] | None, str | None, float, bool, bool] = case()
    css: str = case()
    pipe: Callable[[GT], GT] = case()

    @staticmethod
    def Header(title: str, subtitle: str | None = None, preheader: list[str] | None = None) -> "TableOp":
        return TableOp(header=(title, subtitle, preheader))

    @staticmethod
    def Stub(rowname: str | None = None, group: str | None = None) -> "TableOp":
        return TableOp(stub=(rowname, group))

    @staticmethod
    def Stubhead(label: str) -> "TableOp":
        return TableOp(stubhead=label)

    @staticmethod
    def SourceNote(note: str) -> "TableOp":
        return TableOp(source_note=note)

    @staticmethod
    def Footnote(
        text: str,
        at: StubLoc = StubLoc.BODY,
        columns: Cols = None,
        rows: Rows = None,
        mask: Mask = None,
        groups: Groups = None,
        placement: Literal["auto", "left", "right"] = "auto",
    ) -> "TableOp":
        return TableOp(footnote=(text, (at, columns, rows, mask, groups), placement))

    @staticmethod
    def Label(labels: dict[str, str]) -> "TableOp":
        return TableOp(label=labels)

    @staticmethod
    def LabelWith(fn: Callable[[str], str], columns: Cols = None) -> "TableOp":
        return TableOp(label_with=(columns, fn))

    @staticmethod
    def LabelRotate(columns: Cols = None, direction: str = "sideways-lr", align: str = "center", padding: int = 8) -> "TableOp":
        return TableOp(label_rotate=(columns, direction, align, padding))

    @staticmethod
    def Align(align: str, columns: Cols = None) -> "TableOp":
        return TableOp(align=(align, columns))

    @staticmethod
    def Width(widths: dict[str, str]) -> "TableOp":
        return TableOp(width=widths)

    @staticmethod
    def Hide(columns: Cols) -> "TableOp":
        return TableOp(hide=columns)

    @staticmethod
    def Unhide(columns: Cols) -> "TableOp":
        return TableOp(unhide=columns)

    @staticmethod
    def Reorder(columns: list[str]) -> "TableOp":
        return TableOp(reorder=columns)

    @staticmethod
    def Move(columns: Cols, after: str) -> "TableOp":
        return TableOp(move=(columns, after))

    @staticmethod
    def MoveEnds(columns: Cols, end: Literal["start", "end"] = "start") -> "TableOp":
        return TableOp(move_ends=(columns, end))

    @staticmethod
    def Spanner(
        label: str,
        columns: Cols = None,
        nest: str | list[str] | None = None,
        level: int | None = None,
        spanner_id: str | None = None,
        gather: bool = True,
        replace: bool = False,
    ) -> "TableOp":
        return TableOp(spanner=(label, columns, nest, level, spanner_id, gather, replace))

    @staticmethod
    def SpannerDelim(
        delim: str = ".", columns: Cols = None, split: Literal["first", "last"] = "last", limit: int = -1, reverse: bool = False
    ) -> "TableOp":
        return TableOp(spanner_delim=(delim, columns, split, limit, reverse))

    @staticmethod
    def Fmt(kind: FmtKind, columns: Cols = None, rows: Rows = None, **opts: Any) -> "TableOp":
        return TableOp(fmt=(kind, columns, rows, opts))

    @staticmethod
    def FmtCustom(fns: FmtFn | dict[str, FmtFn], columns: Cols = None, rows: Rows = None, is_substitution: bool = False) -> "TableOp":
        return TableOp(fmt=(FmtKind.CUSTOM, columns, rows, {"fns": fns, "is_substitution": is_substitution}))

    @staticmethod
    def Nanoplot(columns: Cols, spec: "NanoSpec | None" = None, rows: Rows = None, **opts: Any) -> "TableOp":
        return TableOp(nanoplot=(columns, rows, spec if spec is not None else NanoSpec(options=opts or None)))

    @staticmethod
    def MergeRange(begin: str, end: str, sep: str | None = None, rows: Rows = None, autohide: bool = True) -> "TableOp":
        return TableOp(merge_range=(begin, end, sep, rows, autohide))

    @staticmethod
    def MergeUncert(value: str, uncert: str, sep: str = " +/- ", rows: Rows = None, autohide: bool = True) -> "TableOp":
        return TableOp(merge_uncert=(value, uncert, sep, rows, autohide))

    @staticmethod
    def MergeNPct(n: str, pct: str, rows: Rows = None, autohide: bool = True) -> "TableOp":
        return TableOp(merge_n_pct=(n, pct, rows, autohide))

    @staticmethod
    def Merge(columns: Cols, pattern: str, hide_columns: Cols = None, rows: Rows = None) -> "TableOp":
        return TableOp(merge=(columns, pattern, hide_columns, rows))

    @staticmethod
    def SubMissing(columns: Cols = None, text: str | None = None) -> "TableOp":
        return TableOp(sub_missing=(columns, text))

    @staticmethod
    def SubZero(columns: Cols = None, text: str = "nil") -> "TableOp":
        return TableOp(sub_zero=(columns, text))

    @staticmethod
    def SubSmall(columns: Cols = None, threshold: float = 0.01, pattern: str | None = None, sign: str = "+") -> "TableOp":
        return TableOp(sub_small=(columns, threshold, pattern, sign))

    @staticmethod
    def SubLarge(columns: Cols = None, threshold: float = 1e12, pattern: str = ">={x}", sign: str = "+") -> "TableOp":
        return TableOp(sub_large=(columns, threshold, pattern, sign))

    @staticmethod
    def SubValues(fn: Predicate, replacement: str, columns: Cols = None) -> "TableOp":
        return TableOp(sub_values=(columns, fn, replacement))

    @staticmethod
    def TextTransform(
        fn: Callable[[str], str], at: StubLoc = StubLoc.BODY, columns: Cols = None, rows: Rows = None, mask: Mask = None, groups: Groups = None
    ) -> "TableOp":
        return TableOp(text_transform=((at, columns, rows, mask, groups), fn))

    @staticmethod
    def TextCaseWhen(
        cases: tuple[tuple[Predicate, str], ...],
        default: str | None = None,
        at: StubLoc = StubLoc.BODY,
        columns: Cols = None,
        rows: Rows = None,
        mask: Mask = None,
        groups: Groups = None,
    ) -> "TableOp":
        return TableOp(text_case_when=((at, columns, rows, mask, groups), cases, default))

    @staticmethod
    def TextCaseMatch(
        cases: tuple[tuple[str | list[str], str], ...],
        default: str | None = None,
        replace: Literal["all", "partial"] = "all",
        at: StubLoc = StubLoc.BODY,
        columns: Cols = None,
        rows: Rows = None,
        mask: Mask = None,
        groups: Groups = None,
    ) -> "TableOp":
        return TableOp(text_case_match=((at, columns, rows, mask, groups), cases, default, replace))

    @staticmethod
    def TextReplace(
        pattern: str, replacement: str, at: StubLoc = StubLoc.BODY, columns: Cols = None, rows: Rows = None, mask: Mask = None, groups: Groups = None
    ) -> "TableOp":
        return TableOp(text_replace=((at, columns, rows, mask, groups), pattern, replacement))

    @staticmethod
    def Summary(
        fns: dict[str, pl.Expr],
        fmt: FmtFn | None = None,
        groups: list[str] | None = None,
        side: Literal["bottom", "top"] = "bottom",
        missing_text: str = "---",
    ) -> "TableOp":
        return TableOp(summary=(fns, fmt, groups, side, missing_text))

    @staticmethod
    def GrandSummary(
        fns: dict[str, pl.Expr], fmt: FmtFn | None = None, side: Literal["bottom", "top"] = "bottom", missing_text: str = "---"
    ) -> "TableOp":
        return TableOp(grand_summary=(fns, fmt, side, missing_text))

    @staticmethod
    def RowGroupOrder(groups: list[str]) -> "TableOp":
        return TableOp(row_group_order=groups)

    @staticmethod
    def Style(
        specs: tuple[StyleSpec, ...], at: StubLoc = StubLoc.BODY, columns: Cols = None, rows: Rows = None, mask: Mask = None, groups: Groups = None
    ) -> "TableOp":
        return TableOp(style=((at, columns, rows, mask, groups), specs))

    @staticmethod
    def Color(
        columns: Cols = None,
        palette: str | list[str] = "viridis",
        domain: list[float] | list[str] | None = None,
        rows: Rows = None,
        na_color: str | None = None,
        alpha: float = 1.0,
        reverse: bool = False,
        truncate: bool = True,
    ) -> "TableOp":
        return TableOp(color=(columns, rows, palette, domain, na_color, alpha, reverse, truncate))

    @staticmethod
    def Css(rule: str) -> "TableOp":
        return TableOp(css=rule)

    @staticmethod
    def Pipe(fn: Callable[[GT], GT]) -> "TableOp":
        return TableOp(pipe=fn)


class TablePlan(Struct, frozen=True):
    frame: pl.DataFrame
    ops: tuple[TableOp, ...]
    fmt: TableFormat
    shape: tuple[Reshape, ...] = ()
    theme: Theme = Theme()
    rowname_col: str | None = None
    groupname_col: str | None = None
    locale: str | None = None
    auto_align: bool = True
    table_id: str | None = None
    config: frozendict[str, object] = frozendict()
    inline_css: bool = True
    make_page: bool = False
    all_important: bool = False
    use_longtable: bool = True
    tbl_pos: str | None = None
    pdf_scale: float = 2.0

    @classmethod
    def of(
        cls,
        source: FrameSource,
        ops: Iterable[TableOp],
        fmt: TableFormat,
        *,
        shape: Iterable[Reshape] = (),
        theme: Theme = Theme(),
        rowname_col: str | None = None,
        groupname_col: str | None = None,
        locale: str | None = None,
        config: frozendict[str, object] = frozendict(),
    ) -> Self:
        # admit raw material ONCE: a settled frame passes through; an Arrow-C-stream capsule normalizes via `pl.from_dataframe` (zero-copy where possible).
        frame = source if isinstance(source, pl.DataFrame) else pl.from_dataframe(source)
        return cls(
            frame=frame,
            ops=tuple(ops),
            fmt=fmt,
            shape=tuple(shape),
            theme=theme,
            rowname_col=rowname_col,
            groupname_col=groupname_col,
            locale=locale,
            config=config,
        )

    @staticmethod
    def Series(values: pl.Series, kind: FmtKind = FmtKind.NUMBER, **opts: Any) -> list[str]:
        return VALS_TABLE[kind](values, opts)

    @staticmethod
    def Units(notation: str) -> Any:
        return define_units(units_notation=notation)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: canonical (frame ⊕ ops ⊕ fmt ⊕ theme) minted PRE-RUN, never over rendered bytes; the
        # frame enters as its `_stable_id` row-hash digest, so distinct data never shares a key.
        return ContentIdentity.of(
            f"table-{self.fmt}", (_stable_id(self.frame, self.fmt, self.ops), self.ops, self.fmt, self.theme), policy=CANONICAL_POLICY
        )

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the whole shape-style-emit fold is synchronous CPU work; it crosses the runtime thread lane, `catch=_FAULTS` narrowing to the engine raise tuple.
        return await async_boundary(f"table.{self.fmt}", self._offloaded, catch=_FAULTS)

    async def _offloaded(self) -> ArtifactReceipt:
        crossed = await LanePolicy.offload(self._rendered, modality=Modality.THREAD, retry=RetryClass.OCCT)
        return crossed.default_with(_table_raise)

    def _rendered(self) -> ArtifactReceipt:
        data = self.build()
        return ArtifactReceipt.Table(
            ContentIdentity.key(f"table-{self.fmt}", data), self.fmt.value, len(data)
        )  # rendered bytes are an infallible whole-byte source, so the receipt mints off a bare `ContentKey`, never the railed `of`

    @property
    def _seam(self) -> bool:
        # the id is always set, so the fast `DataFrame.style` seam survives whenever the non-default GT knobs
        # stay default; a diverging rowname/group/locale/auto_align forces the explicit constructor.
        return self.rowname_col is None and self.groupname_col is None and self.locale is None and self.auto_align

    def build(self) -> bytes:
        with pl.Config(**self.config):
            shaped = reduce(_shape, self.shape, self.frame)
            ident = self.table_id if self.table_id is not None else _stable_id(shaped, self.fmt, self.ops)
            base = (
                shaped.style.with_id(ident)
                if self._seam
                else GT(
                    shaped, rowname_col=self.rowname_col, groupname_col=self.groupname_col, auto_align=self.auto_align, id=ident, locale=self.locale
                )
            )
            built = reduce(_fold, self.ops, self.theme.apply(base))
            match self.fmt:
                case TableFormat.HTML:
                    return built.as_raw_html(inline_css=self.inline_css, make_page=self.make_page, all_important=self.all_important).encode()
                case TableFormat.LATEX:
                    return built.as_latex(use_longtable=self.use_longtable, tbl_pos=self.tbl_pos).encode()
                case TableFormat.PDF:
                    with NamedTemporaryFile(suffix=".pdf", delete_on_close=False) as sink:
                        built.save(file=sink.name, scale=self.pdf_scale)
                        return Path(sink.name).read_bytes()
                case _:
                    assert_never(self.fmt)


# --- [TABLES] ---------------------------------------------------------------------------
FMT_TABLE: frozendict[FmtKind, FmtMember] = frozendict({
    FmtKind.NUMBER: lambda g, c, r, k: g.fmt_number(columns=c, rows=r, **k),
    FmtKind.INTEGER: lambda g, c, r, k: g.fmt_integer(columns=c, rows=r, **k),
    FmtKind.CURRENCY: lambda g, c, r, k: g.fmt_currency(columns=c, rows=r, **k),
    FmtKind.PERCENT: lambda g, c, r, k: g.fmt_percent(columns=c, rows=r, **k),
    FmtKind.SCIENTIFIC: lambda g, c, r, k: g.fmt_scientific(columns=c, rows=r, **k),
    FmtKind.ENGINEERING: lambda g, c, r, k: g.fmt_engineering(columns=c, rows=r, **k),
    FmtKind.PARTSPER: lambda g, c, r, k: g.fmt_partsper(columns=c, rows=r, **k),
    FmtKind.UNITS: lambda g, c, r, k: g.fmt_units(columns=c, rows=r, **k),
    FmtKind.DURATION: lambda g, c, r, k: g.fmt_duration(columns=c, rows=r, **k),
    FmtKind.BYTES: lambda g, c, r, k: g.fmt_bytes(columns=c, rows=r, **k),
    FmtKind.ROMAN: lambda g, c, r, k: g.fmt_roman(columns=c, rows=r, **k),
    FmtKind.DATE: lambda g, c, r, k: g.fmt_date(columns=c, rows=r, **k),
    FmtKind.DATETIME: lambda g, c, r, k: g.fmt_datetime(columns=c, rows=r, **k),
    FmtKind.TIME: lambda g, c, r, k: g.fmt_time(columns=c, rows=r, **k),
    FmtKind.TF: lambda g, c, r, k: g.fmt_tf(columns=c, rows=r, **k),
    FmtKind.FLAG: lambda g, c, r, k: g.fmt_flag(columns=c, rows=r, **k),
    FmtKind.ICON: lambda g, c, r, k: g.fmt_icon(columns=c, rows=r, **k),
    FmtKind.IMAGE: lambda g, c, r, k: g.fmt_image(columns=c, rows=r, **k),
    FmtKind.MARKDOWN: lambda g, c, r, k: g.fmt_markdown(columns=c, rows=r),
    FmtKind.CUSTOM: lambda g, c, r, k: g.fmt(columns=c, rows=r, **k),
})

LOC_TABLE: frozendict[StubLoc, tuple[Callable[..., Loc], LocArity]] = frozendict({
    StubLoc.BODY: (loc.body, LocArity.MASKED),
    StubLoc.GRAND_SUMMARY: (loc.grand_summary, LocArity.MASKED),
    StubLoc.SUMMARY: (loc.summary, LocArity.GROUPED),
    StubLoc.COLUMN_LABELS: (loc.column_labels, LocArity.COLUMNS),
    StubLoc.SPANNER_LABELS: (loc.spanner_labels, LocArity.IDS),
    StubLoc.STUB: (loc.stub, LocArity.ROWS),
    StubLoc.ROW_GROUP: (loc.row_group, LocArity.ROWS),
    StubLoc.ROW_GROUPS: (loc.row_groups, LocArity.ROWS),
    StubLoc.SUMMARY_STUB: (loc.summary_stub, LocArity.GROUP_ROWS),
    StubLoc.GRAND_SUMMARY_STUB: (loc.grand_summary_stub, LocArity.ROWS),
    StubLoc.COLUMN_HEADER: (loc.column_header, LocArity.NONE),
    StubLoc.STUBHEAD: (loc.stubhead, LocArity.NONE),
    StubLoc.HEADER: (loc.header, LocArity.NONE),
    StubLoc.FOOTER: (loc.footer, LocArity.NONE),
    StubLoc.TITLE: (loc.title, LocArity.NONE),
    StubLoc.SUBTITLE: (loc.subtitle, LocArity.NONE),
    StubLoc.SOURCE_NOTES: (loc.source_notes, LocArity.NONE),
})

VALS_TABLE: frozendict[FmtKind, Callable[[pl.Series, dict[str, Any]], list[str]]] = frozendict({
    FmtKind.NUMBER: lambda v, k: vals.fmt_number(v, **k),
    FmtKind.INTEGER: lambda v, k: vals.fmt_integer(v, **k),
    FmtKind.CURRENCY: lambda v, k: vals.fmt_currency(v, **k),
    FmtKind.PERCENT: lambda v, k: vals.fmt_percent(v, **k),
    FmtKind.SCIENTIFIC: lambda v, k: vals.fmt_scientific(v, **k),
    FmtKind.ENGINEERING: lambda v, k: vals.fmt_engineering(v, **k),
    FmtKind.PARTSPER: lambda v, k: vals.fmt_partsper(v, **k),
    FmtKind.DURATION: lambda v, k: vals.fmt_duration(v, **k),
    FmtKind.BYTES: lambda v, k: vals.fmt_bytes(v, **k),
    FmtKind.ROMAN: lambda v, k: vals.fmt_roman(v, **k),
    FmtKind.DATE: lambda v, k: vals.fmt_date(v, **k),
    FmtKind.TIME: lambda v, k: vals.fmt_time(v, **k),
    FmtKind.IMAGE: lambda v, k: vals.fmt_image(v, **k),
    FmtKind.MARKDOWN: lambda v, k: vals.fmt_markdown(v, **k),
})


# --- [OPERATIONS] -----------------------------------------------------------------------
def _font(gt: GT, face: FontFace) -> GT:
    kind, name = face
    match kind:
        case "google":
            return gt.opt_table_font(font=google_font(name=name))
        case "system":
            return gt.opt_table_font(stack=name, font=system_fonts(name=name))
        case "family":
            return gt.opt_table_font(font=name)
        case _:
            assert_never(kind)


def _place(at: StubLoc, columns: Cols = None, rows: Rows = None, mask: Mask = None, groups: Groups = None) -> Loc:
    selector, arity = LOC_TABLE[at]
    match arity:
        case LocArity.MASKED if mask is not None:
            return selector(mask=mask)
        case LocArity.MASKED | LocArity.CELLS:
            return selector(columns=columns, rows=rows)
        case LocArity.GROUPED:
            return selector(groups=groups, columns=columns, rows=rows)
        case LocArity.GROUP_ROWS:
            return selector(groups=groups, rows=rows)
        case LocArity.COLUMNS:
            return selector(columns=columns)
        case LocArity.IDS:
            return selector(ids=columns)
        case LocArity.ROWS:
            return selector(rows=rows)
        case LocArity.NONE:
            return selector()
        case _:
            assert_never(arity)


def _cell(spec: StyleSpec) -> CellStyle:
    kind, opts = spec
    match kind:
        case "text":
            return style.text(**opts)
        case "fill":
            return style.fill(**opts)
        case "borders":
            return style.borders(**opts)
        case "css":
            return style.css(**opts)
        case _:
            assert_never(kind)


def _table_raise(fault: object) -> "ArtifactReceipt":
    raise ValueError(str(fault))


def _shape(frame: pl.DataFrame, op: Reshape) -> pl.DataFrame:
    # `maintain_order=True`/`sort_columns=True` on group/pivot fix row/column order so the display cross-tab
    # renders byte-reproducibly (per-column `Categories`, no global cache); total by `assert_never`.
    match op:
        case Reshape(tag="select", select=columns):
            return frame.select(*columns)
        case Reshape(tag="filter", filter=predicates):
            return frame.filter(*predicates)
        case Reshape(tag="sort", sort=(by, descending)):
            return frame.sort(by, descending=descending)
        case Reshape(tag="rename", rename=mapping):
            return frame.rename(dict(mapping))
        case Reshape(tag="cast", cast=dtypes):
            return frame.cast(dict(dtypes))
        case Reshape(tag="head", head=n):
            return frame.head(n)
        case Reshape(tag="slice", slice=(offset, length)):
            return frame.slice(offset, length)
        case Reshape(tag="with_columns", with_columns=exprs) | Reshape(tag="derive", derive=exprs):
            return frame.with_columns(*exprs)
        case Reshape(tag="group_agg", group_agg=(keys, aggs)):
            return frame.group_by(*keys, maintain_order=True).agg(*aggs)
        case Reshape(tag="pivot", pivot=(on, index, values, aggregate)):
            return frame.pivot(on, index=index, values=values, aggregate_function=aggregate, maintain_order=True, sort_columns=True)
        case Reshape(tag="unpivot", unpivot=(on, index, variable, value)):
            return frame.unpivot(on, index=index, variable_name=variable, value_name=value)
        case Reshape(tag="top_k", top_k=(k, by)):
            return frame.top_k(k, by=by)
        case _ as unreachable:
            assert_never(unreachable)


def _stable_id(frame: pl.DataFrame, fmt: TableFormat, ops: tuple[TableOp, ...]) -> str:
    # a content-stable CSS-scope id: great-tables mints `random_id()` when `id` is None, drifting the rendered
    # bytes and the ContentKey every render — the row-hash + format + op-tag digest keys it (id must lead with a letter).
    seed = frame.hash_rows(seed=0).to_numpy().tobytes() + fmt.value.encode() + b"".join(op.tag.encode() for op in ops)
    return f"gt{blake2b(seed, digest_size=8).hexdigest()}"


def _fold(gt: GT, op: TableOp) -> GT:
    match op:
        case TableOp(tag="header", header=(title, subtitle, preheader)):
            return gt.tab_header(title=md(title), subtitle=subtitle, preheader=preheader)
        case TableOp(tag="stub", stub=(rowname, group)):
            return gt.tab_stub(rowname_col=rowname, groupname_col=group)
        case TableOp(tag="stubhead", stubhead=label):
            return gt.tab_stubhead(label=label)
        case TableOp(tag="source_note", source_note=note):
            return gt.tab_source_note(source_note=md(note))
        case TableOp(tag="footnote", footnote=(text, (at, columns, rows, mask, groups), placement)):
            return gt.tab_footnote(footnote=md(text), locations=_place(at, columns, rows, mask, groups), placement=placement)
        case TableOp(tag="label", label=labels):
            return gt.cols_label(cases=labels)
        case TableOp(tag="label_with", label_with=(columns, fn)):
            return gt.cols_label_with(columns=columns, fn=fn)
        case TableOp(tag="label_rotate", label_rotate=(columns, direction, align, padding)):
            return gt.cols_label_rotate(columns=columns, dir=direction, align=align, padding=padding)
        case TableOp(tag="align", align=(align, columns)):
            return gt.cols_align(align=align, columns=columns)
        case TableOp(tag="width", width=widths):
            return gt.cols_width(cases=widths)
        case TableOp(tag="hide", hide=columns):
            return gt.cols_hide(columns=columns)
        case TableOp(tag="unhide", unhide=columns):
            return gt.cols_unhide(columns=columns)
        case TableOp(tag="reorder", reorder=columns):
            return gt.cols_reorder(columns=columns)
        case TableOp(tag="move", move=(columns, after)):
            return gt.cols_move(columns=columns, after=after)
        case TableOp(tag="move_ends", move_ends=(columns, end)):
            return (gt.cols_move_to_end if end == "end" else gt.cols_move_to_start)(columns=columns)
        case TableOp(tag="spanner", spanner=(label, columns, nest, level, spanner_id, gather, replace)):
            return gt.tab_spanner(label=label, columns=columns, spanners=nest, level=level, id=spanner_id, gather=gather, replace=replace)
        case TableOp(tag="spanner_delim", spanner_delim=(delim, columns, split, limit, reverse)):
            return gt.tab_spanner_delim(delim=delim, columns=columns, split=split, limit=limit, reverse=reverse)
        case TableOp(tag="fmt", fmt=(kind, columns, rows, opts)):
            return FMT_TABLE[kind](gt, columns, rows, opts)
        case TableOp(tag="nanoplot", nanoplot=(columns, rows, spec)):
            return spec.fold(gt, columns, rows)
        case TableOp(tag="merge_range", merge_range=(begin, end, sep, rows, autohide)):
            return gt.cols_merge_range(col_begin=begin, col_end=end, sep=sep, rows=rows, autohide=autohide)
        case TableOp(tag="merge_uncert", merge_uncert=(value, uncert, sep, rows, autohide)):
            return gt.cols_merge_uncert(col_val=value, col_uncert=uncert, sep=sep, rows=rows, autohide=autohide)
        case TableOp(tag="merge_n_pct", merge_n_pct=(n, pct, rows, autohide)):
            return gt.cols_merge_n_pct(col_n=n, col_pct=pct, rows=rows, autohide=autohide)
        case TableOp(tag="merge", merge=(columns, pattern, hide_columns, rows)):
            return gt.cols_merge(columns=columns, pattern=pattern, hide_columns=hide_columns, rows=rows)
        case TableOp(tag="sub_missing", sub_missing=(columns, text)):
            return gt.sub_missing(columns=columns, missing_text=text)
        case TableOp(tag="sub_zero", sub_zero=(columns, text)):
            return gt.sub_zero(columns=columns, zero_text=text)
        case TableOp(tag="sub_small", sub_small=(columns, threshold, pattern, sign)):
            return gt.sub_small_vals(columns=columns, threshold=threshold, small_pattern=pattern, sign=sign)
        case TableOp(tag="sub_large", sub_large=(columns, threshold, pattern, sign)):
            return gt.sub_large_vals(columns=columns, threshold=threshold, large_pattern=pattern, sign=sign)
        case TableOp(tag="sub_values", sub_values=(columns, fn, replacement)):
            return gt.sub_values(columns=columns, fn=fn, replacement=replacement)
        case TableOp(tag="text_transform", text_transform=((at, columns, rows, mask, groups), fn)):
            return gt.text_transform(locations=_place(at, columns, rows, mask, groups), fn=fn)
        case TableOp(tag="text_case_when", text_case_when=((at, columns, rows, mask, groups), cases, default)):
            return gt.text_case_when(*cases, default=default, locations=_place(at, columns, rows, mask, groups))
        case TableOp(tag="text_case_match", text_case_match=((at, columns, rows, mask, groups), cases, default, replace)):
            return gt.text_case_match(*cases, default=default, replace=replace, locations=_place(at, columns, rows, mask, groups))
        case TableOp(tag="text_replace", text_replace=((at, columns, rows, mask, groups), pattern, replacement)):
            return gt.text_replace(pattern=pattern, replacement=replacement, locations=_place(at, columns, rows, mask, groups))
        case TableOp(tag="summary", summary=(fns, fmt, groups, side, missing_text)):
            return gt.summary_rows(fns=fns, fmt=fmt, groups=groups, side=side, missing_text=missing_text)
        case TableOp(tag="grand_summary", grand_summary=(fns, fmt, side, missing_text)):
            return gt.grand_summary_rows(fns=fns, fmt=fmt, side=side, missing_text=missing_text)
        case TableOp(tag="row_group_order", row_group_order=groups):
            return gt.row_group_order(groups=groups)
        case TableOp(tag="style", style=((at, columns, rows, mask, groups), specs)):
            return gt.tab_style(style=[_cell(spec) for spec in specs], locations=_place(at, columns, rows, mask, groups))
        case TableOp(tag="color", color=(columns, rows, palette, domain, na_color, alpha, reverse, truncate)):
            return gt.data_color(
                columns=columns,
                rows=rows,
                palette=palette,
                domain=domain,
                na_color=na_color,
                alpha=alpha,
                reverse=reverse,
                autocolor_text=True,
                truncate=truncate,
            )
        case TableOp(tag="css", css=rule):
            return gt.opt_css(css=rule)
        case TableOp(tag="pipe", pipe=fn):
            return gt.pipe(fn)
        case _:
            assert_never(op)


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["FmtKind", "FootnoteMarks", "NanoSpec", "Reshape", "StubLoc", "TableFormat", "TableOp", "TablePlan", "Theme"]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
