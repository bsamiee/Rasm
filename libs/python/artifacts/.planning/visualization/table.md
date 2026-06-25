# [PY_ARTIFACTS_TABLE]

The publication-quality tabular-artifact owner. `TablePlan` is ONE owner over great-tables (Bring-Your-Own-DataFrame: polars first-class through the `DataFrame.style` seam, pandas optional) producing journal-quality tables — spanners, scientific value formatting, uncertainty merges, missing-value substitution, configured inline nanoplots, data-driven coloring, and `opt_*` theme identity — exported to HTML, LaTeX, and PDF. It is the missing third artifact pillar beside documents and charts, so measured study results render as formatted tables rather than raw dumps, feeding `document/report#REPORT` and `document/emit#DOCUMENT` and handing its flat HTML/SVG egress to the regrouped `composition/compose#COMPOSE` placement owner for layout beside its sibling graphics. great-tables is admitted in the manifest (cp315-clean, pure-Python); this page closes the `PUBLICATION_TABLE_OWNER` idea.

## [01]-[INDEX]

- [01]-[TABLE]: great-tables styled-table export owner over HTML/LaTeX/PDF, carrying the full publication surface as one closed `TableOp` tagged-union family — every `fmt_*` scientific format, `cols_*` column control, `cols_merge_*` uncertainty merge, `sub_*` substitution, `text_*` transform, `opt_*` theme, `tab_*` structure, `fmt_nanoplot` configured-sparkline, and `data_color(autocolor_text)` row folded by one total `match` over an ordered op sequence in the one `reduce`-over-spec pipeline; `loc.*` targeting collapses into one `Loc` data row whose arg-arity is policy, never a parallel selector catalogue or a branch on location kind.

## [02]-[TABLE]

- Owner: `TablePlan` the one table owner discriminating export format over the closed `TableFormat` `StrEnum` (`HTML`/`LATEX`/`PDF`); `TableOp` an `expression.tagged_union` whose every case carries its own typed payload — never a parallel `dict[str, Callable]` format/style/location catalogue and never a per-feature `reduce` pass — dispatched by one total `match`/`case` folding each op onto the great-tables `GT` builder; the dataframe is the BYODF input the `GT(frame)` builder styles, the polars `DataFrame.style` accessor the in-process `GT`-construction seam returning a real `GT`. `Theme` is the closed `opt_*`-row stylize vocabulary carried as the table's named publication identity, never a per-call `tab_options` keyword scatter.
- Cases: `TableOp` rows fold the full publication surface — `Header`/`Stub`/`Stubhead`/`SourceNote`/`Footnote` structure · `Label`/`LabelWith`/`LabelRotate`/`Align`/`Width`/`Hide`/`Unhide`/`Reorder`/`Move`/`MoveEnds` column control · `Spanner`/`SpannerDelim` column-group nesting · `Fmt` value format dispatched over the `FmtKind` data table (`Number`/`Integer`/`Currency`/`Percent`/`Scientific`/`Engineering`/`PartsPer`/`Units`/`Duration`/`Bytes`/`Roman`/`Date`/`Datetime`/`Time`/`Tf`/`Flag`/`Icon`/`Image`/`Markdown`/`Custom`) · `Nanoplot` configured-sparkline · `MergeRange`/`MergeUncert`/`MergeNPct`/`Merge` column merge · `SubMissing`/`SubZero`/`SubSmall`/`SubLarge`/`SubValues` substitution · `TextTransform`/`TextCaseWhen`/`TextCaseMatch`/`TextReplace` cell transform · `Summary`/`GrandSummary`/`RowGroupOrder` aggregation · `Style`/`Color`/`Css` cell-style, data-driven fill, and raw-CSS injection — matched by one total `match`/`case`, each arm composing the verified `GT` member directly. The one `Fmt` case carrying a `FmtKind` discriminant collapses the twenty format verbs — the nineteen `fmt_*` members plus the arbitrary `fmt(fns, is_substitution)` custom-function verb at the `Custom` row — onto one fold over the `FMT_TABLE` row catalogue, since every member shares the `(columns, rows, **opts)` shape; the `FmtCustom` factory threads `fns` and `is_substitution` through `opts` so an arbitrary value-to-string callable lands as one row, not a parallel surface.
- Entry: `TablePlan.render` seeds the `GT` from the frame and the `Theme` publication identity, folds the `ops` sequence through the one `reduce`-over-`TableOp` pipeline, dispatches the `TableFormat`, and returns a `RuntimeRail[ContentKey]`; the styling is one ordered closed-family fold, never a per-column imperative loop, a per-feature `gt.tab_header(...).fmt_number(...)` method chain, or a parallel-catalogue scatter.
- Auto: the build is one ordered fold over `Theme` then `ops` — `Theme.apply` seeds the `opt_stylize`/`opt_row_striping`/`opt_table_font`/`opt_align_table_header`/`opt_all_caps`/`opt_vertical_padding`/`opt_horizontal_padding`/`opt_table_outline`/`opt_footnote_marks` identity, and `_font` discriminates the `FontFace` over the closed `("google", "system", "family")` axis so `google_font(name)` rides `opt_table_font(font=...)`, the named `system_fonts(name)` stack rides `opt_table_font(stack=..., font=...)`, and a raw family string rides `font=` — never a bare `font_stack` string. Each `TableOp` case folds its one verified `GT` member; the `Fmt` arm indexes the `FMT_TABLE` row by `FmtKind` and applies it with the case `opts`; the `Color` arm folds the domain-to-palette map (from `graphic/color/derive#DERIVE`) through `data_color(na_color, alpha, reverse, autocolor_text=True, truncate)` so the text color derives from the background luminance, the `reverse` axis flips the scale, `na_color` paints the missing band, and `truncate` bounds the palette interpolation range; the `Style` arm folds `tab_style(style.*, loc.*)` where the `Loc` row carries the polars-predicate `mask` for value-targeted cells and a `from_column(column)` bound threads a per-row data-driven value into `style.fill`/`style.text`, never a constant color scatter; `Nanoplot` folds the `NanoSpec` value object's `NanoSpec.fold` over `fmt_nanoplot` carrying the full `plot_type`/`plot_height`/`missing_vals`/`autoscale` plot grammar and the `reference_line`/`reference_area`/`expand_x`/`expand_y` axis-framing args with the `nanoplot_options(...)` configured dict, never the flat default sparkline; `Summary`/`GrandSummary` carry the optional `fmt` standalone formatter so each aggregate band formats with the same `vals.fmt_*` member the cells use, and the `side`/`missing_text` placement so each band names its empty-cell glyph; `Pipe` threads an escape-hatch `GT -> GT` through `GT.pipe` so an unrowed package call lands as one case, never a method-chain break-out. `_place` is one `LOC_TABLE` data-row projection from `StubLoc` to the verified per-selector arg-arity through the `LocArity` discriminant — the `IDS` arity binds the `spanner_labels` `ids=` selector and the `GROUPED`/`GROUP_ROWS` arities thread the `groups` axis into `summary`/`summary_stub` — never a branch on which location admits `columns`, `rows`, `ids`, or `groups`. `TablePlan.Series` indexes the `VALS_TABLE` row by `FmtKind` over the standalone `vals.fmt_*` Series formatters and `TablePlan.Units` pre-parses a units string through `define_units` for out-of-table rich-text rendering, both `staticmethod` projections on the owner off the `GT`-chain seam, never module-level rename wrappers.
- Receipt: each render contributes `core/receipt#RECEIPT` `ArtifactReceipt.Table` carrying the content key and the format.
- Packages: `great-tables` (`GT`; `tab_header`/`tab_stub`/`tab_stubhead`/`tab_source_note`/`tab_footnote`/`tab_spanner`/`tab_spanner_delim`/`tab_style`/`tab_options`; `cols_label`/`cols_label_with`/`cols_label_rotate`/`cols_align`/`cols_width`/`cols_hide`/`cols_unhide`/`cols_reorder`/`cols_move`/`cols_move_to_start`/`cols_move_to_end`/`cols_merge`/`cols_merge_range`/`cols_merge_uncert`/`cols_merge_n_pct`; `fmt`/`fmt_number`/`fmt_integer`/`fmt_currency`/`fmt_percent`/`fmt_scientific`/`fmt_engineering`/`fmt_partsper`/`fmt_units`/`fmt_duration`/`fmt_bytes`/`fmt_roman`/`fmt_date`/`fmt_datetime`/`fmt_time`/`fmt_tf`/`fmt_flag`/`fmt_icon`/`fmt_image`/`fmt_markdown`/`fmt_nanoplot`; `sub_missing`/`sub_zero`/`sub_small_vals`/`sub_large_vals`/`sub_values`; `text_transform`/`text_case_when`/`text_case_match`/`text_replace`; `summary_rows`/`grand_summary_rows`/`row_group_order`; `data_color`; `opt_stylize`/`opt_row_striping`/`opt_table_font`/`opt_align_table_header`/`opt_all_caps`/`opt_vertical_padding`/`opt_horizontal_padding`/`opt_table_outline`/`opt_footnote_marks`/`opt_css`; the `loc.body`/`loc.column_labels`/`loc.column_header`/`loc.spanner_labels`/`loc.stub`/`loc.stubhead`/`loc.row_group`/`loc.row_groups`/`loc.header`/`loc.footer`/`loc.title`/`loc.subtitle`/`loc.source_notes`/`loc.summary`/`loc.summary_stub`/`loc.grand_summary`/`loc.grand_summary_stub` selectors with `loc.body(mask=...)`; the `style.text`/`style.fill`/`style.borders`/`style.css` constructors; the `nanoplot_options`/`define_units`/`from_column`/`google_font`/`system_fonts`/`md`/`html`/`pct`/`px` helpers; the standalone `vals.fmt_number`/`vals.fmt_integer`/`vals.fmt_currency`/`vals.fmt_percent`/`vals.fmt_scientific`/`vals.fmt_engineering`/`vals.fmt_bytes`/`vals.fmt_roman`/`vals.fmt_date`/`vals.fmt_time`/`vals.fmt_markdown` Series formatters; `GT(auto_align, id)` construction knobs and `pipe`; `as_raw_html(inline_css, make_page, all_important)`/`as_latex(use_longtable, tbl_pos)`/`save(scale)`), `polars` (`DataFrame.style` → `GT` seam, `pl.Series` into `vals.fmt_*`), runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`boundary`).
- Growth: a new export format is one `TableFormat` row; a new format verb is one `FmtKind` row in `FMT_TABLE` (and one `VALS_TABLE` row when the standalone Series formatter exists); a new structural, merge, substitution, transform, or aggregation feature is one `TableOp` case with its typed payload and one `match` arm; a new column-op knob (`nest`/`spanner_id`/`gather`/`replace` on `Spanner`, `limit`/`reverse` on `SpannerDelim`, `align`/`padding` on `LabelRotate`, `rows`/`autohide` on a merge, `fmt` on a summary, `truncate` on `Color`) is one widened case-tuple slot and one factory parameter, never a parallel case; a new emit knob is one `TablePlan` policy field folded in `_build`; a new location target is one `StubLoc` row in `LOC_TABLE`; a new theme axis is one `Theme` field folded in `Theme.apply`; a new font source is one `FontFace` row in `_font`; a new nanoplot knob is one `NanoSpec` field folded in `NanoSpec.fold`; an unrowed package call is one `Pipe` `GT -> GT`; zero new catalogue, zero new `reduce` pass, zero parallel surface.
- Boundary: no raw data interchange (that stays at `data`); the dataframe arrives as a settled polars/pandas input or the polars `DataFrame.style` `GT` seam, and `_seam` keeps that accessor as the default-construction path, falling to the explicit `GT(...)` constructor only when `rowname_col`/`groupname_col`/`locale`/`table_id`/`auto_align` diverges from default so the in-process `[WIRE]` edge survives. The HTML and LaTeX emit knobs ride `TablePlan` policy fields — `inline_css`/`make_page`/`all_important` on `as_raw_html` and `use_longtable`/`tbl_pos` on `as_latex` — so every emit literal traces to a field default, not a hardcoded call argument; both are the no-driver host-free byte paths, and the emitted HTML/SVG is the flat handoff the regrouped `composition/compose#COMPOSE` placement owner reads to lay the table out beside its sibling graphics, re-rendering nothing on this axis. The PDF `save` path rides a host-coupled Chrome/Selenium WebDriver scaled by the `pdf_scale` field, writes a suffix-typed file (`save` infers format from the extension and rejects a file-like object), so `_build` rounds it through a `NamedTemporaryFile(suffix=".pdf")` and reads the bytes back — gated optional and never the default, the one remaining gated host path the package admits. A per-feature method chain (`gt.tab_header(...).tab_source_note(...).fmt_number(...)`), a per-cell imperative loop, a parallel format/style/location catalogue, and a branch on location kind are the deleted forms — every feature folds through the one `reduce`-over-`TableOp` pipeline; the `Theme` `opt_*` rows are the table's one named publication identity, never a per-call style scatter.

```python signature
from collections.abc import Callable
from enum import Enum, StrEnum, auto
from functools import reduce
from pathlib import Path
from tempfile import NamedTemporaryFile
from typing import Any, Literal, assert_never

import polars as pl
from expression import case, tag, tagged_union
from great_tables import GT, define_units, from_column, google_font, loc, md, nanoplot_options, style, system_fonts, vals
from great_tables._locations import Loc
from great_tables._styles import CellStyle, ColumnExpr
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary

type Cols = str | list[str] | None
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


FMT_TABLE: dict[FmtKind, FmtMember] = {
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
}

LOC_TABLE: dict[StubLoc, tuple[Callable[..., Loc], LocArity]] = {
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
}


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


VALS_TABLE: dict[FmtKind, Callable[[pl.Series, dict[str, Any]], list[str]]] = {
    FmtKind.NUMBER: lambda v, k: vals.fmt_number(v, **k),
    FmtKind.INTEGER: lambda v, k: vals.fmt_integer(v, **k),
    FmtKind.CURRENCY: lambda v, k: vals.fmt_currency(v, **k),
    FmtKind.PERCENT: lambda v, k: vals.fmt_percent(v, **k),
    FmtKind.SCIENTIFIC: lambda v, k: vals.fmt_scientific(v, **k),
    FmtKind.ENGINEERING: lambda v, k: vals.fmt_engineering(v, **k),
    FmtKind.BYTES: lambda v, k: vals.fmt_bytes(v, **k),
    FmtKind.ROMAN: lambda v, k: vals.fmt_roman(v, **k),
    FmtKind.DATE: lambda v, k: vals.fmt_date(v, **k),
    FmtKind.TIME: lambda v, k: vals.fmt_time(v, **k),
    FmtKind.MARKDOWN: lambda v, k: vals.fmt_markdown(v, **k),
}


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
    footnote_marks: str = "numbers"
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

    def apply(self, gt: GT) -> GT:
        themed = gt.opt_stylize(style=self.style, color=self.color, add_row_striping=self.striping)
        fonted = _font(themed, self.font) if self.font else themed
        outlined = fonted.opt_table_outline(style=self.outline[0], width=self.outline[1], color=self.outline[2]) if self.outline else fonted
        cssed = outlined.opt_css(css=self.css) if self.css else outlined
        return (
            cssed.opt_row_striping(row_striping=self.striping)
            .opt_align_table_header(align=self.header_align)
            .opt_all_caps(all_caps=self.all_caps)
            .opt_vertical_padding(scale=self.vertical_scale)
            .opt_horizontal_padding(scale=self.horizontal_scale)
            .opt_footnote_marks(marks=self.footnote_marks)
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
            columns=columns, rows=rows, plot_type=self.plot_type, plot_height=self.plot_height,
            missing_vals=self.missing_vals, autoscale=self.autoscale, reference_line=self.reference_line,
            reference_area=self.reference_area, expand_x=self.expand_x, expand_y=self.expand_y,
            options=nanoplot_options(**self.options) if self.options else None,
        )


@tagged_union(frozen=True)
class TableOp:
    tag: Literal[
        "header", "stub", "stubhead", "source_note", "footnote",
        "label", "label_with", "label_rotate", "align", "width",
        "hide", "unhide", "reorder", "move", "move_ends",
        "spanner", "spanner_delim",
        "fmt", "nanoplot",
        "merge_range", "merge_uncert", "merge_n_pct", "merge",
        "sub_missing", "sub_zero", "sub_small", "sub_large", "sub_values",
        "text_transform", "text_case_when", "text_case_match", "text_replace",
        "summary", "grand_summary", "row_group_order",
        "style", "color", "css", "pipe",
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
    color: tuple[Cols, Rows, str | list[str], list[float] | None, str | None, float, bool, bool] = case()
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
    def Footnote(text: str, at: StubLoc = StubLoc.BODY, columns: Cols = None, rows: Rows = None, mask: Mask = None, groups: Groups = None, placement: Literal["auto", "left", "right"] = "auto") -> "TableOp":
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
    def Spanner(label: str, columns: Cols = None, nest: str | list[str] | None = None, level: int | None = None, spanner_id: str | None = None, gather: bool = True, replace: bool = False) -> "TableOp":
        return TableOp(spanner=(label, columns, nest, level, spanner_id, gather, replace))

    @staticmethod
    def SpannerDelim(delim: str = ".", columns: Cols = None, split: Literal["first", "last"] = "last", limit: int = -1, reverse: bool = False) -> "TableOp":
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
    def TextTransform(fn: Callable[[str], str], at: StubLoc = StubLoc.BODY, columns: Cols = None, rows: Rows = None, mask: Mask = None, groups: Groups = None) -> "TableOp":
        return TableOp(text_transform=((at, columns, rows, mask, groups), fn))

    @staticmethod
    def TextCaseWhen(cases: tuple[tuple[Predicate, str], ...], default: str | None = None, at: StubLoc = StubLoc.BODY, columns: Cols = None, rows: Rows = None, mask: Mask = None, groups: Groups = None) -> "TableOp":
        return TableOp(text_case_when=((at, columns, rows, mask, groups), cases, default))

    @staticmethod
    def TextCaseMatch(cases: tuple[tuple[str | list[str], str], ...], default: str | None = None, replace: Literal["all", "partial"] = "all", at: StubLoc = StubLoc.BODY, columns: Cols = None, rows: Rows = None, mask: Mask = None, groups: Groups = None) -> "TableOp":
        return TableOp(text_case_match=((at, columns, rows, mask, groups), cases, default, replace))

    @staticmethod
    def TextReplace(pattern: str, replacement: str, at: StubLoc = StubLoc.BODY, columns: Cols = None, rows: Rows = None, mask: Mask = None, groups: Groups = None) -> "TableOp":
        return TableOp(text_replace=((at, columns, rows, mask, groups), pattern, replacement))

    @staticmethod
    def Summary(fns: dict[str, pl.Expr], fmt: FmtFn | None = None, groups: list[str] | None = None, side: Literal["bottom", "top"] = "bottom", missing_text: str = "---") -> "TableOp":
        return TableOp(summary=(fns, fmt, groups, side, missing_text))

    @staticmethod
    def GrandSummary(fns: dict[str, pl.Expr], fmt: FmtFn | None = None, side: Literal["bottom", "top"] = "bottom", missing_text: str = "---") -> "TableOp":
        return TableOp(grand_summary=(fns, fmt, side, missing_text))

    @staticmethod
    def RowGroupOrder(groups: list[str]) -> "TableOp":
        return TableOp(row_group_order=groups)

    @staticmethod
    def Style(specs: tuple[StyleSpec, ...], at: StubLoc = StubLoc.BODY, columns: Cols = None, rows: Rows = None, mask: Mask = None, groups: Groups = None) -> "TableOp":
        return TableOp(style=((at, columns, rows, mask, groups), specs))

    @staticmethod
    def Color(columns: Cols = None, palette: str | list[str] = "viridis", domain: list[float] | None = None, rows: Rows = None, na_color: str | None = None, alpha: float = 1.0, reverse: bool = False, truncate: bool = True) -> "TableOp":
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
    theme: Theme = Theme()
    rowname_col: str | None = None
    groupname_col: str | None = None
    locale: str | None = None
    auto_align: bool = True
    table_id: str | None = None
    inline_css: bool = True
    make_page: bool = False
    all_important: bool = False
    use_longtable: bool = True
    tbl_pos: str | None = None
    pdf_scale: float = 2.0

    @staticmethod
    def Series(values: pl.Series, kind: FmtKind = FmtKind.NUMBER, **opts: Any) -> list[str]:
        return VALS_TABLE[kind](values, opts)

    @staticmethod
    def Units(notation: str) -> Any:
        return define_units(units_notation=notation)

    def render(self) -> RuntimeRail[ContentKey]:
        return boundary(f"table.{self.fmt}", self._emit)

    def _emit(self) -> ContentKey:
        return ContentIdentity.of(f"table-{self.fmt}", self._build())

    @property
    def _seam(self) -> bool:
        return self.rowname_col is None and self.groupname_col is None and self.locale is None and self.table_id is None and self.auto_align

    def _build(self) -> bytes:
        base = self.frame.style if self._seam else GT(self.frame, rowname_col=self.rowname_col, groupname_col=self.groupname_col, auto_align=self.auto_align, id=self.table_id, locale=self.locale)
        built = reduce(_fold, self.ops, self.theme.apply(base))
        match self.fmt:
            case TableFormat.HTML:
                return built.as_raw_html(inline_css=self.inline_css, make_page=self.make_page, all_important=self.all_important).encode()
            case TableFormat.LATEX:
                return built.as_latex(use_longtable=self.use_longtable, tbl_pos=self.tbl_pos).encode()
            case TableFormat.PDF:
                with NamedTemporaryFile(suffix=".pdf") as sink:
                    built.save(file=sink.name, scale=self.pdf_scale)
                    return Path(sink.name).read_bytes()
            case _:
                assert_never(self.fmt)


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
        case TableOp(tag="move_ends", move_ends=(columns, "start")):
            return gt.cols_move_to_start(columns=columns)
        case TableOp(tag="move_ends", move_ends=(columns, "end")):
            return gt.cols_move_to_end(columns=columns)
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
            return gt.data_color(columns=columns, rows=rows, palette=palette, domain=domain, na_color=na_color, alpha=alpha, reverse=reverse, autocolor_text=True, truncate=truncate)
        case TableOp(tag="css", css=rule):
            return gt.opt_css(css=rule)
        case TableOp(tag="pipe", pipe=fn):
            return gt.pipe(fn)
        case _:
            assert_never(op)
```

## [03]-[RESEARCH]

No open items. Every `great_tables` member below verifies against the folder `.api` catalogue for `great-tables` at the `0.22.0` surface (cp315-clean, pure-Python). Two type imports resolve only at the private module path the package exposes: `CellStyle` and the `ColumnExpr` alias (`FromColumn | PlExpr | FromValues`) live at `great_tables._styles`, not `great_tables.style`, so the `_cell` return annotation and the data-driven `Bound = str | ColumnExpr` import from `great_tables._styles`; `Loc` lives at `great_tables._locations`. The public `style`/`loc`/`vals` submodules and the top-level `GT`/`define_units`/`from_column`/`google_font`/`system_fonts`/`nanoplot_options`/`md`/`html` helpers resolve from the package root. The `loc.spanner_labels(ids)` selector binds the `ids=` keyword (never `columns=`), `loc.summary(groups, columns, rows)` and `loc.summary_stub(groups, rows)` carry the `groups` axis, `text_case_match` admits `replace ∈ {"all","partial"}` over `(str | list[str], str)` match cases, and `summary_rows`/`grand_summary_rows` reject a non-`None` `columns` with `NotImplementedError` so each `pl.Expr` carries its own column.

- `GT(data, rowname_col, groupname_col, auto_align, id, locale)` builder, the terminal emitters `as_raw_html(inline_css, make_page, all_important)` / `as_latex(use_longtable, tbl_pos)` / `write_raw_html(filename, encoding, inline_css, ...)` / `save(file, selector, scale, expand, web_driver, window_size, ...)`; `with_id(id)` / `with_locale(locale)` / `pipe(func, *args, **kwargs)` post-construction.
- structure: `tab_header(title, subtitle, preheader)` (`title` accepts `str | Text`) · `tab_stub(rowname_col, groupname_col)` · `tab_stubhead(label)` · `tab_source_note(source_note)` · `tab_footnote(footnote, locations, placement)` · `tab_spanner(label, columns, spanners, level, id, gather, replace)` · `tab_spanner_delim(delim, columns, split, limit, reverse)` (`split` ∈ `{"first","last"}`).
- column control: `cols_label(cases, **kwargs)` · `cols_label_with(columns, fn)` · `cols_label_rotate(columns, dir, align, padding)` (`dir` ∈ `{"sideways-lr","sideways-rl","vertical-lr"}`) · `cols_align(align, columns)` · `cols_width(cases, **kwargs)` · `cols_hide(columns)` · `cols_unhide(columns)` · `cols_reorder(columns)` · `cols_move(columns, after)` · `cols_move_to_start(columns)` · `cols_move_to_end(columns)`.
- merge: `cols_merge(columns, hide_columns, rows, pattern)` · `cols_merge_range(col_begin, col_end, rows, sep, autohide, locale)` (`sep` defaults to `None`) · `cols_merge_uncert(col_val, col_uncert, rows, sep, autohide)` (`sep` defaults to `" +/- "`) · `cols_merge_n_pct(col_n, col_pct, rows, autohide)`.
- value format: `fmt_number` · `fmt_integer(..., use_seps, compact)` · `fmt_currency(..., currency, use_subunits, placement)` · `fmt_percent(..., scale_values, placement)` · `fmt_scientific(..., exp_style, locale)` · `fmt_engineering(..., n_sigfig, scale_by, exp_style)` · `fmt_partsper(columns, rows, to_units, symbol, decimals, scale_values, ...)` (`to_units` ∈ `{"per-mille","per-myriad","pcm","ppm","ppb","ppt"}`) · `fmt_units(columns, rows, pattern)` · `fmt_duration(columns, rows, input_units, output_units, duration_style, trim_zero_units, max_output_units, ...)` · `fmt_bytes(..., standard, decimals, incl_space)` · `fmt_roman(columns, rows, case, pattern)` · `fmt_date` · `fmt_datetime` · `fmt_time` · `fmt_tf(..., tf_style, true_val, false_val, na_val, colors)` · `fmt_flag(columns, rows, height, sep, use_title)` · `fmt_icon(..., stroke_color, fill_color)` · `fmt_image(columns, rows, height, width, sep, path, file_pattern, encode)` · `fmt_markdown(columns, rows)` · `fmt(fns, columns, rows, is_substitution)` (the arbitrary value-to-string callable, `is_substitution` defaults to `False`) — all twenty share the `(columns, rows, **opts)` shape that `FMT_TABLE` rows fold, `fmt_markdown` taking no extra `opts` and the `Custom` row threading `fns`/`is_substitution` through `opts`.
- nanoplot: `fmt_nanoplot(columns, rows, plot_type, plot_height, missing_vals, autoscale, reference_line, reference_area, expand_x, expand_y, options)` carries the full plot grammar on the `NanoSpec` value object — `plot_type ∈ {"line","bar"}` (default `"line"`), `plot_height` (default `"2em"`), `missing_vals ∈ {"marker","gap","zero","remove"}` (default `"gap"`), `autoscale` (default `False`), `reference_line`, `reference_area` (`list[Any] | None`), `expand_x`/`expand_y` (`list[float] | None`) — configured by the top-level `nanoplot_options(...)` callable returning the `options` dict (`data_point_radius`/`data_point_stroke_color`/`data_point_fill_color`/`data_line_type`/`data_line_stroke_color`/`data_bar_fill_color`/`data_area_fill_color`/`reference_line_color`/`reference_area_fill_color`/`show_data_points`/`show_data_line`/`show_data_area`/`show_reference_line`/`y_val_fmt_fn`/`y_axis_fmt_fn`/`y_ref_line_fmt_fn`/`currency` and the full plot-styling keyword surface), never the flat default sparkline. The `NanoSpec.missing_vals` default tracks the `.api` catalogue `"gap"`.
- substitution: `sub_missing(columns, rows, missing_text)` · `sub_zero(columns, rows, zero_text)` (`zero_text` defaults to `"nil"`) · `sub_small_vals(columns, rows, threshold, small_pattern, sign)` (`threshold` defaults to `0.01`, `sign` to `"+"`) · `sub_large_vals(columns, rows, threshold, large_pattern, sign)` (`threshold` defaults to `1e12`, `large_pattern` to `">={x}"`, `sign` to `"+"`) · `sub_values(columns, rows, values, pattern, fn, replacement)`.
- cell transform: `text_transform(locations, fn)` · `text_case_when(*cases, default, locations)` (`cases` are `(predicate, replacement)` tuples) · `text_case_match(*cases, default, replace, locations)` (`cases` are `(match, replacement)` tuples where `match` is `str | list[str]`, `replace` ∈ `{"all","partial"}`) · `text_replace(pattern, replacement, locations)`.
- aggregation: `summary_rows(*, fns, fmt, columns, groups, side, missing_text)` and `grand_summary_rows(*, fns, fmt, columns, side, missing_text)` take `fns` keyword-only as `dict[str, PlExpr]` whose polars expression names its own target column; a non-`None` `columns` raises `NotImplementedError`, so the `Summary`/`GrandSummary` cases omit the `columns` slot, let each `pl.Expr` carry its own column, and thread the optional `fmt` standalone formatter (`vals.fmt_*`) instead · `row_group_order(groups)` (`groups` is `RowGroups`).
- style and color: `tab_style(style, locations)` over the `style.text(color, font, size, align, v_align, style, weight, stretch, decorate, transform, whitespace)` / `style.fill(color)` / `style.borders(sides, color, style, weight)` / `style.css(rule)` constructors (`style.text` and `style.fill` accept a `ColumnExpr` from `from_column` for data-driven values). The `loc.*` selectors carry a per-selector argument arity that `LOC_TABLE` encodes as `LocArity`: `loc.body(columns, rows, mask)` and `loc.grand_summary(columns, rows, mask)` are `MASKED`, `loc.summary(groups, columns, rows)` is `GROUPED`, `loc.summary_stub(groups, rows)` is `GROUP_ROWS`, `loc.column_labels(columns)` is `COLUMNS`, `loc.spanner_labels(ids)` is `IDS` (the `ids=` keyword binds the `columns` slot, never `columns=`), `loc.stub(rows)` / `loc.row_group(rows)` / `loc.row_groups(rows)` / `loc.grand_summary_stub(rows)` are `ROWS`, and `loc.column_header()` / `loc.stubhead()` / `loc.header()` / `loc.footer()` / `loc.title()` / `loc.subtitle()` / `loc.source_notes()` are `NONE`. `loc.body(mask=<polars predicate>)` is mutually exclusive with `columns`/`rows`, so a masked `Place` passes the polars `mask` alone · `data_color(columns, rows, palette, domain, na_color, alpha, reverse, autocolor_text, truncate)` accepts `palette` as a matplotlib palette-name `str` or an explicit hex `list[str]` (the `graphic/color/derive#DERIVE` `ColorReceipt.coords` palette projected to a hex list) and `domain` as `list[str] | list[int] | list[float]`, deriving cell text color from background luminance when `autocolor_text=True`.
- theme identity: `opt_stylize(style, color, add_row_striping)` · `opt_row_striping(row_striping)` · `opt_table_font(font, stack, weight, style, add)` (`stack` is a named `FontStackName`) · `opt_align_table_header(align)` · `opt_all_caps(all_caps, locations)` · `opt_vertical_padding(scale)` · `opt_horizontal_padding(scale)` · `opt_table_outline(style, width, color)` · `opt_footnote_marks(marks)` · `opt_css(css, add, allow_duplicates)` — the `Theme` value object's `opt_*` rows are the table's one named publication identity, replacing the per-call `tab_options(**kwargs)` keyword scatter (`tab_options` remains the low-level theme surface but is not the authoring path).
- helpers: the top-level `md`/`html` rich-text markers, `from_column(column, na_value, fn)` returning the data-driven `ColumnExpr` that `style.fill`/`style.text` accept (composed directly at the `Style`/`Color` call sites, never a rename wrapper), `define_units(units_notation)` returning the standalone `UnitDefinitionList` for rich-text rendering (the `TablePlan.Units` owner projection; `fmt_units(columns, rows, pattern)` itself takes only `pattern`, never a units object), `google_font(name)` returning a `GoogleFont` for `opt_table_font(font=...)`, `system_fonts(name="system-ui")` returning the `list[str]` font list for `opt_table_font(font=...)` over the named `FontStackName` `stack=`, the `pct`/`px` dimension helpers, and the standalone `vals.fmt_number`/`vals.fmt_integer`/`vals.fmt_currency`/`vals.fmt_percent`/`vals.fmt_scientific`/`vals.fmt_engineering`/`vals.fmt_bytes`/`vals.fmt_roman`/`vals.fmt_date`/`vals.fmt_time`/`vals.fmt_markdown` formatters, each taking a `pl.Series` and returning `list[str]` (the `TablePlan.Series`/`VALS_TABLE` owner projection; `vals` carries no `fmt_units`/`fmt_duration`/`fmt_partsper`/`fmt_tf`/`fmt_flag`/`fmt_icon`, so those `FmtKind` rows stay `GT`-chain only).
- seam: the polars `DataFrame.style` accessor returns a real `GT` (`polars.DataFrame.style -> GT`, verified at the polars `frame.py` source and `isinstance(df.style, GT)`), so the `visualization/table` ← `data/tabular` `[WIRE]` edge constructs the styled table in-process from a polars frame with no intermediate interchange. HTML (`as_raw_html(inline_css=True)`) and LaTeX (`as_latex`) are the host-free no-driver paths; PDF (`save`) rides a host-coupled Chrome/Selenium WebDriver, gated optional and never the default. The emitted HTML/SVG bytes are the flat egress the regrouped `composition/compose#COMPOSE` owner places beside its sibling graphics, the same outbound handoff `visualization/chart/spec#CHART` makes, so the table is a content-keyed source of the composition plane and never composites or re-renders downstream.
