# [PY_ARTIFACTS_TABLE]

The publication-and-schedule tabular-artifact owner — graded at once as a journal-grade publication primitive AND an ISO-drafting schedule/QTO primitive, its bar the union of both planes. `TablePlan` is ONE owner over great-tables (Bring-Your-Own-DataFrame: polars first-class through the `DataFrame.style` seam) that admits a settled frame OR the C# `Rasm.Bim` QTO/schedule wire over `data/tabular`, SHAPES it to its exact display cross-tab through the closed `Reshape` polars pre-pass, then styles it into journal-quality output — spanners, scientific value formatting, uncertainty merges, missing-value substitution, configured inline nanoplots, data-driven coloring, lettered/symbol footnote marks, and `opt_*` theme identity — exported host-free to HTML/LaTeX and (gated) PDF. The frame arrives settled; `TablePlan` reshapes-to-display, styles, and content-keys it, never authoring the data, re-opening the lazy engine, or re-implementing a transform the `data` owner owns. It is the missing third artifact pillar beside documents and charts, so measured results AND BIM quantity takeoffs render as formatted tables rather than raw dumps, feeding `document/report#REPORT` and `document/emit#DOCUMENT`, lowering the `drawing/schedule#SCHEDULE` AEC templates INTO this owner, and handing its flat HTML/SVG egress to `composition/compose#COMPOSE` for placement beside its sibling graphics.

## [01]-[INDEX]

- [01]-[TABLE]: great-tables styled-table export owner over HTML/LaTeX/PDF carrying the full publication surface as one closed `TableOp` tagged-union family — every `fmt_*` scientific format, `cols_*` column control, `cols_merge_*` uncertainty merge, `sub_*` substitution, `text_*` transform, `opt_*` theme, `tab_*` structure, `fmt_nanoplot` configured-sparkline, and `data_color(autocolor_text)` row folded by one total `match` over an ordered op sequence — PRECEDED by the closed `Reshape` polars frame-shaping pre-pass (`pivot`/`unpivot`/`group_agg`/`top_k`/`select`/`sort`/`derive`) that lands a QTO/schedule display cross-tab before the `.style` seam, admitted from a settled frame OR an Arrow-C-stream interchange object, rendered under a `pl.Config` deterministic scope keyed by a content-stable id so the one bytes fact never drifts, offloaded off the event loop, and contributing the `core/receipt#RECEIPT` `ArtifactReceipt.Table` case. `loc.*` targeting collapses into one `Loc` data row whose arg-arity is policy, never a parallel selector catalogue or a branch on location kind; `Cols` admits a `polars.selectors` class so a format/align/color op addresses a dtype class without a hardcoded name list.

## [02]-[TABLE]

- Owner: `TablePlan` the one table owner discriminating export format over the closed `TableFormat` `StrEnum` (`HTML`/`LATEX`/`PDF`); `Reshape` an `expression.tagged_union` whose every case carries one polars frame-shaping transform folded `pl.DataFrame -> pl.DataFrame` BEFORE `.style`; `TableOp` an `expression.tagged_union` whose every case carries its own typed payload — never a parallel `dict[str, Callable]` catalogue and never a per-feature `reduce` pass — dispatched by one total `match`/`case` folding each op onto the great-tables `GT` builder. The dataframe is the BYODF input `GT(frame)`/`frame.style` styles; the polars `DataFrame.style` accessor is the in-process `GT`-construction seam returning a real `GT`. `Theme` is the closed `opt_*`-row stylize vocabulary carried as the table's named publication identity, never a per-call `tab_options` keyword scatter, its `FootnoteMarks` axis selecting numeric, `letters()`/`LETTERS()` alphabetic, or `standard`/`extended` symbol mark sequences.
- Ingress: `TablePlan.of` admits raw material EXACTLY ONCE — a settled `pl.DataFrame` passes through, and an Arrow-C-stream / interchange object (the `__arrow_c_stream__`-exporting capsule the C# `Rasm.Bim` QTO/schedule egress crosses over `data/tabular`, or any `data` producer) normalizes through `pl.from_dataframe(source)` into the canonical `pl.DataFrame` owner (zero-copy where the Arrow layout permits). The interior sees only a settled frame; the `data/tabular` `[WIRE]` edge is parameterized so the same owner sources across many providers without touching its interior, and the frame field is never re-validated inward.
- Shape: `Reshape` rows land the display cross-tab the raw QTO/schedule frame is not yet in — `Select`/`Filter`/`Sort`/`Rename`/`Cast`/`Head`/`Slice`/`With` (re-project, gate, order, re-type, bound), `GroupAgg` (`group_by(*keys, maintain_order=True).agg(*exprs)` quantity rollup), `Pivot` (`pivot(on, index, values, aggregate_function, maintain_order=True, sort_columns=True)` the door/window/room-finish cross-tab), `Unpivot` (wide→long), `TopK` (`top_k(k, by)`), and `Derive` (a `pl.when`/`format`/`concat_str`/`coalesce` display column the frame did not carry) — each a `pl.Expr`/name-carrying case folded by the one total `_shape` `match` over the `shape` sequence, so a schedule rollup is one `Reshape` row and never a Python loop over rows, never a per-cell callable, and never a re-opened lazy engine. `maintain_order=True`/`sort_columns=True` on the `group_agg`/`pivot` rows fix row/column order (modern polars per-column `Categories` make a categorical sort/group deterministic by content, so no global string-cache scope is needed), and the fold runs inside the `pl.Config` render scope so a float→display-string `Derive` renders at fixed precision — the display frame byte-reproducible for the content key.
- Cases: `TableOp` rows fold the full publication surface — `Header`/`Stub`/`Stubhead`/`SourceNote`/`Footnote` structure · `Label`/`LabelWith`/`LabelRotate`/`Align`/`Width`/`Hide`/`Unhide`/`Reorder`/`Move`/`MoveEnds` column control · `Spanner`/`SpannerDelim` column-group nesting · `Fmt` value format dispatched over the `FmtKind` data table (`Number`/`Integer`/`Currency`/`Percent`/`Scientific`/`Engineering`/`PartsPer`/`Units`/`Duration`/`Bytes`/`Roman`/`Date`/`Datetime`/`Time`/`Tf`/`Flag`/`Icon`/`Image`/`Markdown`/`Custom`) · `Nanoplot` configured-sparkline · `MergeRange`/`MergeUncert`/`MergeNPct`/`Merge` column merge · `SubMissing`/`SubZero`/`SubSmall`/`SubLarge`/`SubValues` substitution · `TextTransform`/`TextCaseWhen`/`TextCaseMatch`/`TextReplace` cell transform · `Summary`/`GrandSummary`/`RowGroupOrder` aggregation · `Style`/`Color`/`Css` cell-style, data-driven fill, and raw-CSS injection — matched by one total `match`/`case`, each arm composing the verified `GT` member directly. The one `Fmt` case carrying a `FmtKind` discriminant collapses the twenty format verbs — the nineteen `fmt_*` members plus the arbitrary `fmt(fns, is_substitution)` custom-function verb at the `Custom` row — onto one fold over the `FMT_TABLE` row catalogue, since every member shares the `(columns, rows, **opts)` shape; the `FmtCustom` factory threads `fns` and `is_substitution` through `opts` so an arbitrary value-to-string callable lands as one row, not a parallel surface.
- Entry: `TablePlan.emit()` returns the `ArtifactWork` node (PRE-RUN input key) and `_emit` resolves `RuntimeRail[ArtifactReceipt]`, offloading the whole synchronous shape-style-emit fold onto one `CapacityLimiter`-bounded `to_thread.run_sync(self._emit, ...)` — the shared-address-space thread lane the `polars`/`great-tables` GIL-releasing native render forces, exactly as the sibling leaf producers `visualization/chart/export#EXPORT` and `drawing/schedule#SCHEDULE` take — so no heavy render evaluates on the event loop, the boundary `catch=_FAULTS` narrowed to the real `(PolarsError, ValueError, KeyError, NotImplementedError, OSError)` engine raise tuple (`PolarsError` the base every `_shape` frame op throws, great-tables' render `ValueError`/`KeyError`/`NotImplementedError`, the gated PDF `OSError`) so a non-engine raise and cancellation cross as a defect rather than railing through the `Exception` catch-all `boundaries.md` rejects. `_emit` renders the ONE bytes fact through `build`, content-keys it through the synchronous bare `ContentIdentity.key`, and mints the `ArtifactReceipt.Table(key, format, bytes)` fact off that single render; the public `build()` is the synchronous bytes seam a composing AEC owner (`drawing/schedule#SCHEDULE`) renders through directly to mint its own `ArtifactReceipt.Schedule` case (SINGLE-FACT — one render, one content key, no private-method reach). The styling is one ordered closed-family fold, never a per-column imperative loop, a per-feature method chain, or a parallel-catalogue scatter.
- Auto: `build` is one `pl.Config` scope over an ordered fold — `_shape` reshapes the frame to its display cross-tab, `Theme.apply` seeds the `opt_stylize`/`opt_row_striping`/`opt_table_font`/`opt_align_table_header`/`opt_all_caps`/`opt_vertical_padding`/`opt_horizontal_padding`/`opt_table_outline`/`opt_footnote_marks` identity (`_marks` projecting the `FootnoteMarks` member to its `opt_footnote_marks` keyword great-tables expands to `letters()`/`LETTERS()`/symbol sequences, or an explicit `tuple[str, ...]` custom mark list), and each `TableOp` case folds its one verified `GT` member. The content-stable `table_id` is `_stable_id(shaped, fmt, ops)` — the frame row-hash (`hash_rows(seed=0)`) plus the format plus the op-tag digest through `blake2b`, set through `frame.style.with_id(...)` on the seam path or `GT(id=...)` on the explicit path — because great-tables mints `random_id()` when `id` is `None`, drifting the rendered bytes and the ContentKey every render, so the deterministic id is the byte-reproducibility gate the content-addressing contract needs. `_font` discriminates the `FontFace` over the closed `("google", "system", "family")` axis so `google_font(name)` rides `opt_table_font(font=...)`, the named `system_fonts(name)` stack rides `opt_table_font(stack=..., font=...)`, and a raw family string rides `font=`. The `Fmt` arm indexes `FMT_TABLE` by `FmtKind`; the `Color` arm folds the domain-to-palette map (the hex list `graphic/color/derive#DERIVE` projects through the `graphic/color/derive#DERIVE` `hex_ramp` seam) through `data_color(na_color, alpha, reverse, autocolor_text=True, truncate)` so text color derives from background luminance; the `Style` arm folds `tab_style(style.*, loc.*)` where the `Loc` row carries the polars-predicate `mask` and a `from_column(column)` bound threads a per-row data-driven value into `style.fill`/`style.text`; `Nanoplot` folds the `NanoSpec` value object over `fmt_nanoplot` with the full `plot_type`/`plot_height`/`missing_vals`/`autoscale` grammar and the `reference_line`/`reference_area`/`expand_x`/`expand_y` axis-framing args plus the `nanoplot_options(...)` configured dict; `Summary`/`GrandSummary` carry the optional `fmt` standalone formatter and the `side`/`missing_text` placement; `Pipe` threads an escape-hatch `GT -> GT` through `GT.pipe`. `_place` is one `LOC_TABLE` data-row projection from `StubLoc` to the verified per-selector arg-arity through the `LocArity` discriminant — the `IDS` arity binds the `spanner_labels` `ids=` selector and the `GROUPED`/`GROUP_ROWS` arities thread the `groups` axis — never a branch on which location admits `columns`, `rows`, `ids`, or `groups`. `TablePlan.Series` indexes `VALS_TABLE` by `FmtKind` over the standalone `vals.fmt_*` Series formatters and `TablePlan.Units` pre-parses a units string through `define_units`, both `staticmethod` projections off the `GT`-chain seam.
- Receipt: each render contributes the shared `core/receipt#RECEIPT` `ArtifactReceipt.Table(key, format, bytes)` case — the content key, the `TableFormat` value, and the rendered byte count read off the one `build()` render — through the runtime `ReceiptContributor` port exactly as the sibling `visualization/chart/export#EXPORT` `ArtifactReceipt.Chart` contribution does (the LEAF-producer pattern: `render -> RuntimeRail[ArtifactReceipt]`, the receipt IS the return, mirroring `graphic/raster#RASTER` `Raster.of`), never the `composition/compose#COMPOSE` placement-owner `of() -> RuntimeRail[ArtifactReceipt]` + separate `contribute()` pattern. The public `build()` is the bytes seam a composing AEC owner renders through: `render` composes `build` for the `Table` receipt, while `drawing/schedule#SCHEDULE` composes the same `build` bytes fact to mint its own `ArtifactReceipt.Schedule` case (SINGLE-FACT), so a schedule lowers into this owner without a private-method reach.
- Packages: `great-tables` (`GT(data, rowname_col, groupname_col, auto_align, id, locale)`/`with_id`; `tab_header`/`tab_stub`/`tab_stubhead`/`tab_source_note`/`tab_footnote`/`tab_spanner`/`tab_spanner_delim`/`tab_style`; `cols_label`/`cols_label_with`/`cols_label_rotate`/`cols_align`/`cols_width`/`cols_hide`/`cols_unhide`/`cols_reorder`/`cols_move`/`cols_move_to_start`/`cols_move_to_end`/`cols_merge`/`cols_merge_range`/`cols_merge_uncert`/`cols_merge_n_pct`; `fmt`/`fmt_number`/`fmt_integer`/`fmt_currency`/`fmt_percent`/`fmt_scientific`/`fmt_engineering`/`fmt_partsper`/`fmt_units`/`fmt_duration`/`fmt_bytes`/`fmt_roman`/`fmt_date`/`fmt_datetime`/`fmt_time`/`fmt_tf`/`fmt_flag`/`fmt_icon`/`fmt_image`/`fmt_markdown`/`fmt_nanoplot`; `sub_missing`/`sub_zero`/`sub_small_vals`/`sub_large_vals`/`sub_values`; `text_transform`/`text_case_when`/`text_case_match`/`text_replace`; `summary_rows`/`grand_summary_rows`/`row_group_order`; `data_color`; `opt_stylize`/`opt_row_striping`/`opt_table_font`/`opt_align_table_header`/`opt_all_caps`/`opt_vertical_padding`/`opt_horizontal_padding`/`opt_table_outline`/`opt_footnote_marks`/`opt_css`; the `loc.*` selectors with `loc.body(mask=...)`; the `style.text`/`style.fill`/`style.borders`/`style.css` constructors; the `nanoplot_options`/`define_units`/`from_column`/`google_font`/`system_fonts`/`md`/`html`/`pct`/`px` helpers; the standalone `vals.fmt_*` Series formatters; `pipe`; `as_raw_html(inline_css, make_page, all_important)`/`as_latex(use_longtable, tbl_pos)`/`save(scale)`), `polars` (`DataFrame.style -> GT` seam; the `Reshape` frame-shaping surface `select`/`filter`/`sort`/`rename`/`cast`/`head`/`slice`/`with_columns`/`group_by().agg()`/`pivot`/`unpivot`/`top_k`; `pl.from_dataframe` the interchange ingress; `polars.selectors` `cs.numeric`/`cs.by_dtype`/`cs.matches`/`cs.exclude` the `Cols` selector; `pl.Config` the deterministic display/precision render scope; `DataFrame.hash_rows` the id seed; `pl.Series` into the standalone `vals.fmt_*` formatters; `polars.exceptions.PolarsError` the `_FAULTS` frame-op base), `anyio` (`CapacityLimiter`/the runtime thread lane the offload), `hashlib` (`blake2b` the content-stable id digest), runtime (`identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary(catch=_FAULTS)`), `core/receipt#RECEIPT` (`ArtifactReceipt.Table`).
- Growth: a new frame-shaping transform is one `Reshape` case with its typed payload and one `_shape` arm; a new export format is one `TableFormat` row; a new format verb is one `FmtKind` row in `FMT_TABLE` (and one `VALS_TABLE` row when the standalone Series formatter exists); a new structural, merge, substitution, transform, or aggregation feature is one `TableOp` case with its typed payload and one `match` arm; a new column-op knob is one widened case-tuple slot and one factory parameter, never a parallel case; a new emit knob is one `TablePlan` policy field; a new location target is one `StubLoc` row in `LOC_TABLE`; a new theme axis is one `Theme` field folded in `Theme.apply`; a new footnote-mark family is one `FootnoteMarks` member; a new font source is one `FontFace` row; a new nanoplot knob is one `NanoSpec` field; a new provider ingress is one `FrameSource` arm in `of`; an unrowed package call is one `Pipe` `GT -> GT`; zero new catalogue, zero new `reduce` pass, zero parallel surface.
- Boundary: no raw data interchange authoring (that stays at `data`) — the frame arrives settled over the `data/tabular` wire (a `pl.DataFrame` or an Arrow-C-stream capsule `of` admits once), `_shape` only re-projects/rolls-up/cross-tabs it to display, and `_seam` keeps the `DataFrame.style` accessor as the default-construction path, falling to the explicit `GT(...)` constructor only when `rowname_col`/`groupname_col`/`locale`/`auto_align` diverges from default so the in-process `[WIRE]` edge survives. The HTML and LaTeX emit knobs ride `TablePlan` policy fields — `inline_css`/`make_page`/`all_important` on `as_raw_html` and `use_longtable`/`tbl_pos` on `as_latex` — so every emit literal traces to a field default, not a hardcoded call argument; both are the host-free no-driver byte paths, and the emitted HTML/SVG is the flat handoff `composition/compose#COMPOSE` reads to lay the table out beside its sibling graphics, re-rendering nothing. The PDF `save` path rides a host-coupled Chrome/Selenium WebDriver scaled by `pdf_scale`, writes a suffix-typed file (`save` infers format from the extension and rejects a file-like object), so `build` rounds it through a `NamedTemporaryFile(suffix=".pdf", delete_on_close=False)` — gated optional and never the default, the one remaining gated host path the package admits, a host-free PDF instead routing this owner's HTML through the `document/emit#DOCUMENT` weasyprint printer rather than re-owning it here. `great-tables`' `random_id()` id-is-`None` default (rendered-byte nondeterminism), a per-feature method chain, a per-cell imperative loop, a Python row loop where a `Reshape` `pl.Expr` shapes the frame, a re-opened lazy engine or a source `read_*`/`scan_*` where the settled frame arrives over the wire, a parallel format/style/location catalogue, and a branch on location kind are the deleted forms — every feature folds through the one `reduce`-over-`TableOp` pipeline over the one `reduce`-over-`Reshape` display frame; the `Theme` `opt_*` rows are the table's one named publication identity, never a per-call style scatter.

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
# addresses a dtype/name class without a hardcoded name list; great-tables' `SelectExpr` accepts it.
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
# `pl.from_dataframe` admits zero-copy-where-possible; the settled `pl.DataFrame` is the other ingress arm.
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


# the footnote-mark vocabulary — each member projects to the `opt_footnote_marks(marks=...)` keyword
# great-tables expands: `LETTERS`/`UPPER_LETTERS` to `letters()`/`LETTERS()`, `STANDARD`/`EXTENDED` to the
# 4-/6-symbol sets; a custom `tuple[str, ...]` rides the explicit `list[str]` form (Unicode marks).
class FootnoteMarks(StrEnum):
    NUMBERS = "numbers"
    LETTERS = "letters"
    UPPER_LETTERS = "LETTERS"
    STANDARD = "standard"
    EXTENDED = "extended"


# --- [CONSTANTS] ------------------------------------------------------------------------
# the shared-address-space render lane the polars/great-tables GIL-releasing native fold offloads onto.

# the real engine raise tuple the leaf boundary narrows `catch` to: `PolarsError` the base every `_shape`
# frame op throws (`ColumnNotFoundError`/`ComputeError`/`SchemaError`/`InvalidOperationError` all subclass it),
# the great-tables render `ValueError`/`KeyError`/`NotImplementedError` (a non-`None` summary `columns`), and the
# gated PDF `NamedTemporaryFile` `OSError` — so a non-engine raise and cancellation propagate as a defect rather
# than railing through the `Exception` catch-all `boundaries.md` rejects, exactly as the sibling leaf producers narrow.
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
# BEFORE `.style` so a raw QTO/schedule frame lands its display cross-tab (rollup/pivot/top-k) columnar,
# never a Python row loop; the frame arrives settled from `data/tabular`, this only re-shapes for display.
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
        # admit raw material ONCE: a settled frame passes through, an Arrow-C-stream interchange capsule
        # (the C# `Rasm.Bim` QTO/schedule wire) normalizes into the canonical frame (zero-copy where the layout permits).
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
        # key-over-INPUT: canonical (frame ⊕ ops ⊕ fmt ⊕ theme) minted PRE-RUN — never a key over rendered
        # bytes; the frame enters as its `_stable_id` row-hash digest, so distinct data never shares a key.
        return ContentIdentity.of(
            f"table-{self.fmt}", (_stable_id(self.frame, self.fmt, self.ops), self.ops, self.fmt, self.theme), policy=CANONICAL_POLICY
        )

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the whole shape-style-emit fold is synchronous polars/great-tables CPU work — it crosses the runtime
        # thread lane, `catch=_FAULTS` narrowing the boundary to the real engine raise tuple.
        return await async_boundary(f"table.{self.fmt}", self._offloaded, catch=_FAULTS)

    async def _offloaded(self) -> ArtifactReceipt:
        crossed = await LanePolicy.offload(self._rendered, modality=Modality.THREAD, retry=RetryClass.OCCT)
        return crossed.default_with(_table_raise)

    def _rendered(self) -> ArtifactReceipt:
        data = self.build()
        return ArtifactReceipt.Table(
            ContentIdentity.key(f"table-{self.fmt}", data), self.fmt.value, len(data)
        )  # `key` is the synchronous bare accessor: the rendered bytes are an infallible whole-byte source, so `_emit` mints the receipt off a bare `ContentKey`, never the railed `of`

    @property
    def _seam(self) -> bool:
        # the id is always set (via `with_id`/`id=`), so the fast `DataFrame.style` seam survives whenever the
        # non-default GT knobs stay default; a diverging rowname/group/locale/auto_align forces the constructor.
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
    # renders byte-reproducibly (modern per-column `Categories`, no global cache); total by `assert_never`.
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
    # a content-stable CSS-scope id: great-tables mints `random_id()` when `id` is None, so the rendered
    # bytes and the ContentKey drift every render — the frame row-hash + format + op-tag digest keys the
    # render deterministically so the one bytes fact is reproducible (the id must lead with a letter).
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
