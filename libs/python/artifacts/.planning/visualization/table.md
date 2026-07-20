# [PY_ARTIFACTS_TABLE]

`TablePlan` is the publication-and-schedule tabular-artifact owner — the missing third artifact pillar beside documents and charts — graded at once as a journal-grade publication primitive and an ISO-drafting schedule/QTO primitive. One owner over great-tables (bring-your-own-DataFrame, polars first-class through `DataFrame.style`) admits a settled frame or the C# `Rasm.Bim` QTO/schedule wire over `data/tabular`, shapes it to its display cross-tab through the closed `Reshape` polars pre-pass, then styles it into journal output — spanners, scientific formatting, uncertainty merges, substitution, configured nanoplots, data-driven coloring, footnote marks, and `opt_*` theme identity — exported host-free to HTML/LaTeX and gated PDF. Every option axis is typed: the format verbs share one closed `FmtOptions` payload, cell styling rides the per-kind `TextStyleKwds`/`FillStyleKwds`/`BorderStyleKwds`/`CssStyleKwds` vocabulary, nanoplots the closed `NanoOptions` roster, and data coloring the `DataColorKwds` bundle — no case carries an open `Any` bag, so a misspelled provider argument fails at type-check, never inside the render.

Frame arrives settled; `TablePlan` reshapes-to-display, styles, and content-keys it, never authoring the data, re-opening the lazy engine, or re-implementing a `data`-owned transform. `data/tabular/profile#QUALITY`'s `QualityProfile` frame enters through `TablePlan.of` exactly like any settled frame — the profile owner authors quality facts, this owner styles them. Measured results and BIM quantity takeoffs both render as formatted tables feeding `document/report#REPORT` and `document/emit#DOCUMENT`; `drawing/schedule#SCHEDULE` lowers its AEC templates into this owner and composes the same `build()` bytes fact to mint its own `ArtifactReceipt.Schedule`, so a schedule lowers in with no private-method reach. Flat HTML/SVG egress hands to `composition/compose#COMPOSE` for placement beside sibling graphics.

## [01]-[INDEX]

- [01]-[TABLE]: great-tables styled-table owner over HTML/LaTeX/PDF, one closed `TableOp` family folded over an ordered op sequence, preceded by the closed `Reshape` polars display-shaping pre-pass and content-keyed for a byte-stable render.

## [02]-[TABLE]

- Owner: `TablePlan` discriminates export format over the closed `TableFormat` (`HTML`/`LATEX`/`PDF`); `Reshape` and `TableOp` are `expression.tagged_union` families each carrying one typed payload per case — `Reshape` folded `pl.DataFrame -> pl.DataFrame` before `.style`, `TableOp` folded onto the great-tables `GT` builder — dispatched by one total `match`, never a parallel `dict[str, Callable]` catalogue or a per-feature `reduce`. `Theme` is the closed `opt_*` stylize vocabulary carried as the table's named publication identity, its `FootnoteMarks` axis selecting numeric, alphabetic, or symbol mark sequences.
- Cases: `Reshape` rows land the display cross-tab a raw QTO/schedule frame is not yet in — `Select`/`Filter`/`Sort`/`Rename`/`Cast`/`Head`/`Slice`/`With`, `GroupAgg` quantity rollup, `Pivot` (the door/window/room-finish cross-tab), `Unpivot`, `TopK`, and `Derive` (a display column the frame did not carry) — each a `pl.Expr`-carrying case folded by one total `_shape` match, so a schedule rollup is one row and never a Python loop over cells. `TableOp` folds the full publication surface — structure, column control, spanners, `Fmt` value format over the `FmtKind` table, `Nanoplot`, column merges, substitutions, cell transforms, aggregation, data-driven `Color`, and raw `Css` — each arm composing the verified `GT` member directly. One `Fmt` case carrying a `FmtKind` discriminant collapses every `fmt_*` verb plus the arbitrary `fmt(fns, is_substitution)` custom verb onto one fold over `FMT_TABLE`, since every member shares the `(columns, rows, FmtOptions)` boundary shape and `FmtOptions` is the closed union of every verb's admitted keywords, `_frozen` sealing each payload to an immutable band at storage.
- Entry: `TablePlan.of` admits raw material exactly once — a settled `pl.DataFrame` passes through, an Arrow-C-stream / interchange capsule (the C# `Rasm.Bim` QTO/schedule egress over `data/tabular`, or any `data` producer) normalizes through `pl.from_dataframe` zero-copy where the layout permits — so the interior sees only a settled frame, never re-validated inward, and one parameterized `[WIRE]` edge sources across providers without touching the interior. `emit()` returns the `ArtifactWork` node with a PRE-RUN input key; `build()` is the synchronous bytes seam a composing AEC owner renders through directly, so `drawing/schedule#SCHEDULE` mints its own receipt off the same single render with no private-method reach.
- Auto: `build` is one `pl.Config` scope over an ordered fold — `_shape` reshapes the frame to its cross-tab, `Theme.apply` seeds the `opt_*` identity, each `TableOp` case folds its one `GT` member; `_bridged` detects a group-scoped `summary` op and enters the `GT` through `shaped.to_pandas()` because great-tables' group resolver faults on a polars frame (`IndexError`), the one verified provider gap the bridge closes. `maintain_order=True`/`sort_columns=True` on the group/pivot rows fix row/column order and the fold runs inside the render scope, so a float-to-display `Derive` renders at fixed precision and the display frame is byte-reproducible for the content key. CSS-scope ids derive from the content key — great-tables mints `random_id()` when `id` is `None`, drifting the rendered bytes every render, so `build` stamps `gt{key:032x}` unless the caller pinned `table_id`. `_place` is one `LOC_TABLE` projection from `StubLoc` to the verified per-selector arg-arity through the `LocArity` discriminant, never a branch on which location admits `columns`/`rows`/`ids`/`groups`; `_font` discriminates the closed `FontFace` payloads and passes the bounded `FontStackName` straight to `opt_table_font(stack=)`; the `Color` arm derives text color from background luminance via `autocolor_text=True`; `NanoSpec.fold` owns `fmt_nanoplot`, while standalone value formatting and unit grammar use `vals.fmt_*` and `define_units` directly.
- Identity: ONE primary correspondence keys the render: `_seed` emits length-framed canonical chunks for every render-affecting input — the frame schema plus `hash_rows` digest, each `Reshape` and `TableOp` through the `_sealed` per-value canon (a `pl.Expr` through `Expr.meta.serialize(format="binary")`, a callable through its `module:qualname`, marshalled code, defaults, keyword defaults, closure cells, bound-instance state, and one-hop referenced-global facets — an opaque `__call__` instance refuses), the format, theme, seam knobs, and emit knobs — and `_key = ContentIdentity.key(...)` over those chunks feeds the `ArtifactWork` node, the receipt (`receipt.slot == node.key`), and the derived CSS id, so no second identity mint exists and equal keys imply equal render behavior.
- Receipt: each render contributes the shared `core/receipt#RECEIPT` `ArtifactReceipt.Table(key, format, bytes)` — the node's own content key, `TableFormat` value, and byte count off the one `build()` render — through the runtime `ReceiptContributor` port, the LEAF-producer pattern where the receipt IS the return, mirroring the sibling chart and raster producers rather than the `composition/compose#COMPOSE` placement-owner's `of()` plus separate `contribute()`. `drawing/schedule#SCHEDULE` composes the same `build` bytes fact to mint its own `ArtifactReceipt.Schedule` (single-fact: one render, one content key).
- Packages: `great-tables` (the `GT` builder and its full `tab_*`/`cols_*`/`fmt_*`/`sub_*`/`text_*`/`opt_*`/`data_color`/`vals.fmt_*` surface plus the `loc.*` selectors, `style.*` constructors, and `nanoplot_options`/`define_units`/`from_column`/`google_font` helpers), `polars` (the `DataFrame.style -> GT` seam, the `Reshape` frame ops, `pl.from_dataframe` interchange ingress, `polars.selectors` for the `Cols` dtype-class selector, `pl.Config` render scope, `hash_rows` and `Expr.meta.serialize` the identity seeds, `PolarsError` the `_FAULTS` base), runtime (`ContentIdentity`/`ContentKey`, `RuntimeRail`/`boundary`, `LanePolicy`/`Kernel`/`KernelTrait`), `core/receipt#RECEIPT` (`ArtifactReceipt.Table`).
- Growth: a new frame-shaping transform is one `Reshape` case plus one `_shape` arm; a new export format is one `TableFormat` row; a new format verb is one `FmtKind` row in `FMT_TABLE` plus its `FmtOptions` keys (and one `VALS_TABLE` row where the standalone Series formatter exists); a new structural, merge, substitution, transform, or aggregation feature is one `TableOp` case plus one `match` arm; a new emit knob is one `TablePlan` policy field; a new location target is one `StubLoc` row in `LOC_TABLE`; a new theme axis is one `Theme` field; a new footnote-mark family is one `FootnoteMarks` member; a new font source is one `FontFace` row; a new provider ingress is one `FrameSource` arm in `of`; a new provider keyword is one `NotRequired` key on the owning options TypedDict; an unrowed package call is one `Pipe` `GT -> GT`.
- Boundary: no raw data interchange authoring (that stays at `data`) — the frame arrives settled over the `data/tabular` wire and `_shape` only re-projects/rolls-up/cross-tabs it for display, `_seam` keeping the `DataFrame.style` accessor as the default path and falling to the explicit `GT(...)` constructor only when `rowname_col`/`groupname_col`/`locale`/`auto_align` diverges. Summary fns split by path: `grand_summary` on the polars-native path evaluates `pl.Expr` fns, while a bridged plan (any group-scoped `summary` op) evaluates only the per-column callable form — a `pl.Expr` fn under the bridge raises `TypeError`, folded onto the boundary rail — so a plan mixing both keeps every summary fn callable. HTML and LaTeX emit knobs ride `TablePlan` policy fields so every emit literal traces to a field default, both host-free no-driver byte paths; the emitted HTML/SVG is the flat handoff `composition/compose#COMPOSE` reads, re-rendering nothing. PDF `save` rides a host-coupled Chrome/Selenium WebDriver rounded through a temp file — gated optional, never the default, the one remaining gated host path, a host-free PDF instead routing this owner's HTML through `document/emit#DOCUMENT`; the driver lifecycle owns the wedge kill through its own page-load timeout, because a worker-level `Enforcement.TERMINAL` orphans chromedriver. great-tables' `random_id()` default, a per-feature method chain, a per-cell imperative loop, a Python row loop where a `Reshape` expr shapes the frame, a re-opened lazy engine, a parallel format/style/location catalogue, a branch on location kind, an `Any`-typed option bag, and a second identity mint beside `_key` are the rejected forms.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import marshal
from collections.abc import Callable, Iterable
from enum import Enum, StrEnum, auto
from functools import reduce
from types import BuiltinFunctionType, MethodDescriptorType, ModuleType, WrapperDescriptorType
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Literal, NotRequired, Protocol, Self, TypedDict, Unpack, assert_never, runtime_checkable

import polars as pl
import polars.selectors as cs
from builtins import frozendict
from expression import case, tag, tagged_union
from great_tables import GT, google_font, loc, md, nanoplot_options, style, vals
from great_tables._helpers import FontStackName
from great_tables._locations import Loc
from great_tables._styles import CellStyle, ColumnExpr
from msgspec import Struct, json
from polars.exceptions import PolarsError

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

lazy from selenium.common.exceptions import WebDriverException
lazy from selenium.webdriver import Chrome, ChromeOptions

# --- [TYPES] ----------------------------------------------------------------------------
type Cols = str | list[str] | cs.Selector | None
type Rows = int | list[int] | None
type Mask = pl.Expr | None
type Groups = list[str] | str | None
type Predicate = Callable[[str], bool]
type FmtFn = Callable[[object], str]
type Bound = str | ColumnExpr
type Place = tuple["StubLoc", Cols, Rows, Mask, Groups]
type FmtMember = Callable[[GT, Cols, Rows, frozendict[str, object]], GT]
type Aggregate = str | pl.Expr | None
type SummaryFn = pl.Expr | Callable[[object], object]


@runtime_checkable
class ArrowStream(Protocol):
    def __arrow_c_stream__(self, requested_schema: object = None, /) -> object: ...


type FrameSource = pl.DataFrame | ArrowStream
type RenderedGt = object  # an upstream-styled great_tables.GT crossing opaque (the data-plane QualityProfile wire); `rendered` is its one egress


class TableFormat(StrEnum):
    HTML = "html"
    LATEX = "latex"
    PDF = "pdf"


class LocArity(Enum):
    NONE = auto()
    COLUMNS = auto()
    IDS = auto()
    ROWS = auto()
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


class FootnoteMarks(StrEnum):
    NUMBERS = "numbers"
    LETTERS = "letters"
    UPPER_LETTERS = "LETTERS"
    STANDARD = "standard"
    EXTENDED = "extended"


class FmtOptions(TypedDict, closed=True):
    decimals: NotRequired[int]
    n_sigfig: NotRequired[int | None]
    drop_trailing_zeros: NotRequired[bool]
    drop_trailing_dec_mark: NotRequired[bool]
    use_seps: NotRequired[bool]
    accounting: NotRequired[bool]
    scale_by: NotRequired[float]
    compact: NotRequired[bool]
    pattern: NotRequired[str]
    sep_mark: NotRequired[str]
    dec_mark: NotRequired[str]
    force_sign: NotRequired[bool]
    locale: NotRequired[str | None]
    currency: NotRequired[str | None]
    use_subunits: NotRequired[bool]
    placement: NotRequired[Literal["left", "right"]]
    incl_space: NotRequired[str | bool]
    scale_values: NotRequired[bool]
    to_units: NotRequired[str]
    symbol: NotRequired[str]
    exp_style: NotRequired[str]
    force_sign_m: NotRequired[bool]
    force_sign_n: NotRequired[bool]
    standard: NotRequired[Literal["decimal", "binary"]]
    date_style: NotRequired[str]
    time_style: NotRequired[str]
    format_str: NotRequired[str]
    sep: NotRequired[str]
    input_units: NotRequired[str | list[str]]
    output_units: NotRequired[str | list[str]]
    duration_style: NotRequired[str]
    trim_zero_units: NotRequired[bool]
    max_output_units: NotRequired[int]
    tf_style: NotRequired[str]
    true_val: NotRequired[str]
    false_val: NotRequired[str]
    na_val: NotRequired[str]
    colors: NotRequired[list[str]]
    height: NotRequired[str | int]
    width: NotRequired[str | int]
    use_title: NotRequired[bool]
    stroke_color: NotRequired[str]
    stroke_width: NotRequired[str | int]
    stroke_alpha: NotRequired[float]
    fill_color: NotRequired[str | dict[str, str]]
    fill_alpha: NotRequired[float]
    margin_left: NotRequired[str | int]
    margin_right: NotRequired[str | int]
    path: NotRequired[str]
    file_pattern: NotRequired[str]
    encode: NotRequired[bool]
    case: NotRequired[Literal["upper", "lower"]]
    fns: NotRequired[FmtFn | dict[str, FmtFn]]
    is_substitution: NotRequired[bool]


class NanoOptions(TypedDict, closed=True):
    data_point_radius: NotRequired[int | list[int]]
    data_point_stroke_color: NotRequired[str | list[str]]
    data_point_stroke_width: NotRequired[int | list[int]]
    data_point_fill_color: NotRequired[str | list[str]]
    data_line_type: NotRequired[str]
    data_line_stroke_color: NotRequired[str]
    data_line_stroke_width: NotRequired[int]
    data_area_fill_color: NotRequired[str]
    data_bar_stroke_color: NotRequired[str | list[str]]
    data_bar_stroke_width: NotRequired[int | list[int]]
    data_bar_fill_color: NotRequired[str | list[str]]
    data_bar_negative_stroke_color: NotRequired[str]
    data_bar_negative_stroke_width: NotRequired[int]
    data_bar_negative_fill_color: NotRequired[str]
    reference_line_color: NotRequired[str]
    reference_area_fill_color: NotRequired[str]
    vertical_guide_stroke_color: NotRequired[str]
    vertical_guide_stroke_width: NotRequired[int]
    show_data_points: NotRequired[bool]
    show_data_line: NotRequired[bool]
    show_data_area: NotRequired[bool]
    show_reference_line: NotRequired[bool]
    show_reference_area: NotRequired[bool]
    show_vertical_guides: NotRequired[bool]
    show_y_axis_guide: NotRequired[bool]
    interactive_data_values: NotRequired[bool]
    y_val_fmt_fn: NotRequired[FmtFn]
    y_axis_fmt_fn: NotRequired[FmtFn]
    y_ref_line_fmt_fn: NotRequired[FmtFn]
    currency: NotRequired[str]


class DataColorKwds(TypedDict, closed=True):
    palette: NotRequired[str | list[str]]
    domain: NotRequired[list[float] | list[str] | None]
    na_color: NotRequired[str | None]
    alpha: NotRequired[float]
    reverse: NotRequired[bool]
    truncate: NotRequired[bool]


class SpannerKwds(TypedDict, closed=True):
    spanners: NotRequired[str | list[str] | None]
    level: NotRequired[int | None]
    id: NotRequired[str | None]
    gather: NotRequired[bool]
    replace: NotRequired[bool]


class TextStyleKwds(TypedDict, closed=True):
    color: NotRequired[Bound]
    font: NotRequired[Bound]
    size: NotRequired[Bound]
    align: NotRequired[Literal["center", "left", "right", "justify"]]
    v_align: NotRequired[Literal["middle", "top", "bottom"]]
    style: NotRequired[Literal["normal", "italic", "oblique"]]
    weight: NotRequired[Bound]
    stretch: NotRequired[str]
    decorate: NotRequired[str]
    transform: NotRequired[str]
    whitespace: NotRequired[str]


class FillStyleKwds(TypedDict, closed=True):
    color: Bound


class BorderStyleKwds(TypedDict, closed=True):
    sides: NotRequired[str | list[str]]
    color: NotRequired[Bound]
    style: NotRequired[str]
    weight: NotRequired[Bound]


class CssStyleKwds(TypedDict, closed=True):
    rule: str


type StyleSpec = tuple[Literal["text", "fill", "borders", "css"], TextStyleKwds | FillStyleKwds | BorderStyleKwds | CssStyleKwds]


# --- [CONSTANTS] ------------------------------------------------------------------------
_FAULTS: tuple[type[Exception], ...] = (PolarsError, ValueError, TypeError, KeyError, IndexError, NotImplementedError, OSError)
_CANON = json.Encoder(order="deterministic")
_PDF_LOAD_TIMEOUT: float = 30.0  # driver-owned page-load bound; the render deadline never joins the content preimage


# --- [MODELS] ---------------------------------------------------------------------------
def _frozen(value: object) -> object:
    # storage-side freeze every constructor runs over a caller payload: lists and sets become tuples, dict bands
    # become frozendict rows, and tuples re-freeze member-wise so a list nested inside one cannot ride through —
    # the freeze is transitive; scalars and callables pass through untouched.
    match value:
        case dict() as mapping:
            return frozendict({key: _frozen(item) for key, item in mapping.items()})
        case list() | set() | tuple() as items:
            return tuple(_frozen(item) for item in items)
        case _:
            return value


@tagged_union(frozen=True)
class FontFace:
    tag: Literal["google", "system", "family"] = tag()
    google: str = case()
    system: FontStackName = case()
    family: str = case()


class Theme(Struct, frozen=True):
    style: Literal[1, 2, 3, 4, 5, 6] = 1
    color: Literal["blue", "cyan", "pink", "green", "red", "gray"] = "blue"
    striping: bool = True
    font: FontFace | None = None
    header_align: Literal["left", "center", "right"] = "center"
    all_caps: bool = False
    vertical_scale: float = 1.0
    horizontal_scale: float = 1.0
    outline: tuple[str, str, str] | None = None
    footnote_marks: FootnoteMarks | tuple[str, ...] = FootnoteMarks.NUMBERS
    css: str | None = None

    def _marks(self) -> str | list[str]:
        return list(self.footnote_marks) if isinstance(self.footnote_marks, tuple) else self.footnote_marks.value

    def apply(self, gt: GT) -> GT:
        themed = gt.opt_stylize(style=self.style, color=self.color, add_row_striping=self.striping)
        fonted = _font(themed, self.font) if self.font else themed
        outlined = fonted.opt_table_outline(style=self.outline[0], width=self.outline[1], color=self.outline[2]) if self.outline else fonted
        cssed = outlined.opt_css(css=self.css) if self.css else outlined
        return (
            cssed
            .opt_align_table_header(align=self.header_align)
            .opt_all_caps(all_caps=self.all_caps)
            .opt_vertical_padding(scale=self.vertical_scale)
            .opt_horizontal_padding(scale=self.horizontal_scale)
            .opt_footnote_marks(marks=self._marks())
        )


class NanoSpec(Struct, frozen=True):
    # every payload is immutable at rest — tuple sequences and a frozen options band — so a caller-held reference
    # mutated after emit() can shift neither the scheduled content key nor the rendered plot; `fold` thaws to the
    # provider's list shapes at the one call boundary.
    plot_type: Literal["line", "bar"] = "line"
    plot_height: str = "2em"
    missing_vals: Literal["marker", "gap", "zero", "remove"] = "gap"
    autoscale: bool = False
    reference_line: str | int | float | None = None
    reference_area: tuple[str | int | float, ...] | None = None
    expand_x: tuple[float, ...] | None = None
    expand_y: tuple[float, ...] | None = None
    options: frozendict[str, object] | None = None

    def fold(self, gt: GT, columns: Cols, rows: Rows) -> GT:
        return gt.fmt_nanoplot(
            columns=columns,
            rows=rows,
            plot_type=self.plot_type,
            plot_height=self.plot_height,
            missing_vals=self.missing_vals,
            autoscale=self.autoscale,
            reference_line=self.reference_line,
            reference_area=list(self.reference_area) if self.reference_area is not None else None,
            expand_x=list(self.expand_x) if self.expand_x is not None else None,
            expand_y=list(self.expand_y) if self.expand_y is not None else None,
            options=nanoplot_options(**self.options) if self.options else None,
        )


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
    header: tuple[str, str | None, tuple[str, ...] | None] = case()
    stub: tuple[str | None, str | None] = case()
    stubhead: str = case()
    source_note: str = case()
    footnote: tuple[str, Place, Literal["auto", "left", "right"]] = case()
    label: frozendict[str, str] = case()
    label_with: tuple[Cols, Callable[[str], str]] = case()
    label_rotate: tuple[Cols, str, str, int] = case()
    align: tuple[str, Cols] = case()
    width: frozendict[str, str] = case()
    hide: Cols = case()
    unhide: Cols = case()
    reorder: tuple[str, ...] = case()
    move: tuple[Cols, str] = case()
    move_ends: tuple[Cols, Literal["start", "end"]] = case()
    spanner: tuple[str, Cols, frozendict[str, object]] = case()  # the SpannerKwds roster proves at the Spanner boundary; storage is frozen evidence
    spanner_delim: tuple[str, Cols, Literal["first", "last"], int, bool] = case()
    fmt: tuple[FmtKind, Cols, Rows, frozendict[str, object]] = case()  # the FmtOptions roster proves at the Fmt boundary; storage is frozen evidence
    nanoplot: tuple[str | None, Rows, "NanoSpec"] = case()
    merge_range: tuple[str, str, str | None, Rows, bool] = case()
    merge_uncert: tuple[str, str, str, Rows, bool] = case()
    merge_n_pct: tuple[str, str, Rows, bool] = case()
    merge: tuple[Cols, str, Cols, Rows] = case()
    sub_missing: tuple[Cols, Rows, str | None] = case()
    sub_zero: tuple[Cols, Rows, str] = case()
    sub_small: tuple[Cols, Rows, float, str | None, str] = case()
    sub_large: tuple[Cols, Rows, float, str, str] = case()
    sub_values: tuple[Cols, Rows, Predicate | None, tuple[object, ...] | None, str | None, str] = case()
    text_transform: tuple[Place, Callable[[str], str]] = case()
    text_case_when: tuple[Place, tuple[tuple[Predicate, str], ...], str | None] = case()
    text_case_match: tuple[Place, tuple[tuple[str | list[str], str], ...], str | None, Literal["all", "partial"]] = case()
    text_replace: tuple[Place, str, str] = case()
    summary: tuple[frozendict[str, SummaryFn], FmtFn | None, tuple[str, ...] | None, Literal["bottom", "top"], str] = case()
    grand_summary: tuple[frozendict[str, SummaryFn], FmtFn | None, Literal["bottom", "top"], str] = case()
    row_group_order: tuple[str, ...] = case()
    style: tuple[Place, tuple[StyleSpec, ...]] = case()
    color: tuple[Cols, Rows, frozendict[str, object]] = case()  # the DataColorKwds roster proves at the Color boundary; storage is frozen evidence
    css: str = case()
    pipe: Callable[[GT], GT] = case()

    @staticmethod
    def Header(title: str, subtitle: str | None = None, preheader: list[str] | None = None) -> "TableOp":
        return TableOp(header=(title, subtitle, tuple(preheader) if preheader is not None else None))

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
    def Label(labels: dict[str, str] | frozendict[str, str]) -> "TableOp":
        return TableOp(label=frozendict(labels))

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
    def Width(widths: dict[str, str] | frozendict[str, str]) -> "TableOp":
        return TableOp(width=frozendict(widths))

    @staticmethod
    def Hide(columns: Cols) -> "TableOp":
        return TableOp(hide=columns)

    @staticmethod
    def Unhide(columns: Cols) -> "TableOp":
        return TableOp(unhide=columns)

    @staticmethod
    def Reorder(columns: list[str]) -> "TableOp":
        return TableOp(reorder=tuple(columns))

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
        # the SpannerKwds mint proves the static key roster, `_frozen` then seals it — a list-valued `nest` lands as
        # a tuple inside a frozendict row, so a caller can never mutate a scheduled op or its content key.
        return TableOp(spanner=(label, columns, _frozen(SpannerKwds(spanners=nest, level=level, id=spanner_id, gather=gather, replace=replace))))

    @staticmethod
    def SpannerDelim(
        delim: str = ".", columns: Cols = None, split: Literal["first", "last"] = "last", limit: int = -1, reverse: bool = False
    ) -> "TableOp":
        return TableOp(spanner_delim=(delim, columns, split, limit, reverse))

    @staticmethod
    def Fmt(kind: FmtKind, columns: Cols = None, rows: Rows = None, **opts: Unpack[FmtOptions]) -> "TableOp":
        return TableOp(fmt=(kind, columns, rows, _frozen(opts)))

    @staticmethod
    def FmtCustom(fns: FmtFn | dict[str, FmtFn], columns: Cols = None, rows: Rows = None, is_substitution: bool = False) -> "TableOp":
        return TableOp(fmt=(FmtKind.CUSTOM, columns, rows, frozendict({"fns": _frozen(fns), "is_substitution": is_substitution})))

    @staticmethod
    def Nanoplot(columns: str | None, spec: "NanoSpec | None" = None, rows: Rows = None, **opts: Unpack[NanoOptions]) -> "TableOp":
        return TableOp(nanoplot=(columns, rows, spec if spec is not None else NanoSpec(options=_frozen(opts) if opts else None)))

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
    def SubMissing(columns: Cols = None, rows: Rows = None, text: str | None = None) -> "TableOp":
        return TableOp(sub_missing=(columns, rows, text))

    @staticmethod
    def SubZero(columns: Cols = None, rows: Rows = None, text: str = "nil") -> "TableOp":
        return TableOp(sub_zero=(columns, rows, text))

    @staticmethod
    def SubSmall(columns: Cols = None, rows: Rows = None, threshold: float = 0.01, pattern: str | None = None, sign: str = "+") -> "TableOp":
        return TableOp(sub_small=(columns, rows, threshold, pattern, sign))

    @staticmethod
    def SubLarge(columns: Cols = None, rows: Rows = None, threshold: float = 1e12, pattern: str = ">={x}", sign: str = "+") -> "TableOp":
        return TableOp(sub_large=(columns, rows, threshold, pattern, sign))

    @staticmethod
    def SubValues(
        replacement: str,
        fn: Predicate | None = None,
        values: tuple[object, ...] | None = None,
        pattern: str | None = None,
        columns: Cols = None,
        rows: Rows = None,
    ) -> "TableOp":
        return TableOp(sub_values=(columns, rows, fn, values, pattern, replacement))

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
        fns: dict[str, SummaryFn] | frozendict[str, SummaryFn],
        fmt: FmtFn | None = None,
        groups: list[str] | None = None,
        side: Literal["bottom", "top"] = "bottom",
        missing_text: str = "---",
    ) -> "TableOp":
        return TableOp(summary=(frozendict(fns), fmt, tuple(groups) if groups is not None else None, side, missing_text))

    @staticmethod
    def GrandSummary(
        fns: dict[str, SummaryFn] | frozendict[str, SummaryFn],
        fmt: FmtFn | None = None,
        side: Literal["bottom", "top"] = "bottom",
        missing_text: str = "---",
    ) -> "TableOp":
        return TableOp(grand_summary=(frozendict(fns), fmt, side, missing_text))

    @staticmethod
    def RowGroupOrder(groups: list[str]) -> "TableOp":
        return TableOp(row_group_order=tuple(groups))

    @staticmethod
    def Style(
        specs: tuple[StyleSpec, ...], at: StubLoc = StubLoc.BODY, columns: Cols = None, rows: Rows = None, mask: Mask = None, groups: Groups = None
    ) -> "TableOp":
        return TableOp(style=((at, columns, rows, mask, groups), specs))

    @staticmethod
    def Color(columns: Cols = None, rows: Rows = None, **kwds: Unpack[DataColorKwds]) -> "TableOp":
        return TableOp(color=(columns, rows, _frozen(kwds)))

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
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    lane: LanePolicy
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
    pdf_load_timeout: float = _PDF_LOAD_TIMEOUT

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

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _seed(self) -> tuple[bytes, ...]:
        framed = lambda chunk: len(chunk).to_bytes(8, "little") + chunk
        knobs = _CANON.encode((
            self.fmt.value,
            self.theme,
            self.rowname_col,
            self.groupname_col,
            self.locale,
            self.auto_align,
            self.table_id,
            dict(self.config),
            self.inline_css,
            self.make_page,
            self.all_important,
            self.use_longtable,
            self.tbl_pos,
            self.pdf_scale,
        ))
        return (
            framed(_sealed(tuple(self.frame.schema.items()))),
            framed(self.frame.hash_rows(seed=0).to_numpy().tobytes()),
            *(framed(_sealed(op)) for op in self.shape),
            *(framed(_sealed(op)) for op in self.ops),
            framed(knobs),
        )

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key(f"table-{self.fmt}", self._seed)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        return (await self.lane.offload(Kernel.of(self._railed, KernelTrait.RELEASING))).bind(lambda inner: inner)

    def _railed(self) -> RuntimeRail[ArtifactReceipt]:
        # WebDriverException's tree joins the catch set only on the gated PDF arm, so the lazy selenium import stays cold elsewhere
        return boundary(f"table.{self.fmt}", self._rendered, catch=(*_FAULTS, WebDriverException) if self.fmt is TableFormat.PDF else _FAULTS)

    def _rendered(self) -> ArtifactReceipt:
        data = self.build()
        return ArtifactReceipt.Table(self._key, self.fmt.value, len(data))

    @property
    def _seam(self) -> bool:
        return self.rowname_col is None and self.groupname_col is None and self.locale is None and self.auto_align

    @property
    def _bridged(self) -> bool:
        # great-tables' group resolver faults on a polars frame (IndexError), so a group-scoped summary forces the pandas bridge.
        return any(op.tag == "summary" for op in self.ops)

    def build(self) -> bytes:
        with pl.Config(**self.config):
            shaped = reduce(_shape, self.shape, self.frame)
            ident = self.table_id if self.table_id is not None else f"gt{self._key.value:032x}"
            base = (
                GT(
                    shaped.to_pandas(),
                    rowname_col=self.rowname_col,
                    groupname_col=self.groupname_col,
                    auto_align=self.auto_align,
                    id=ident,
                    locale=self.locale,
                )
                if self._bridged
                else shaped.style.with_id(ident)
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
                    return _pdf_bytes(built, self.pdf_scale, self.pdf_load_timeout)
                case _:
                    assert_never(self.fmt)

    @staticmethod
    def rendered(gt: RenderedGt, fmt: TableFormat = TableFormat.HTML, /) -> bytes:
        # upstream-styled GT egress: the data-plane `QualityProfile` report crosses as an opaque already-built
        # `great_tables.GT` this tier alone renders — never re-shaped through `of` (a GT is no `FrameSource`) and
        # never rendered on the data side, which imports no great_tables.
        match fmt:
            case TableFormat.HTML:
                return gt.as_raw_html().encode()
            case TableFormat.LATEX:
                return gt.as_latex().encode()
            case TableFormat.PDF:
                return _pdf_bytes(gt)
            case _:
                assert_never(fmt)


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

VALS_TABLE: frozendict[FmtKind, Callable[[pl.Series, frozendict[str, object]], list[str]]] = frozendict({
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
def _global_facet(target: object) -> object:
    # one-hop projection of a referenced module global: a module pins by name, a callable by its own code identity
    # (cycle-free — mutual recursion never re-enters `_sealed`), and plain data seals whole.
    match target:
        case ModuleType():
            return f"mod:{target.__name__}"
        case _ if callable(target):
            code = getattr(target, "__code__", None)
            name = f"{getattr(target, '__module__', '')}:{getattr(target, '__qualname__', type(target).__qualname__)}"
            return (name, marshal.dumps(code) if code is not None else b"")
        case _:
            return target


def _sealed(value: object) -> bytes:
    # Canonicalization reads each case tag without dispatching behavior and length-frames every recursive projection.
    match value:
        case pl.Expr():
            return b"expr:" + value.meta.serialize(format="binary")
        case Reshape() | TableOp():
            return b"case:" + value.tag.encode() + b":" + _sealed(getattr(value, value.tag))
        case NanoSpec():
            fields = (value.plot_type, value.plot_height, value.missing_vals, value.autoscale, value.reference_line)
            return b"nanospec:" + _sealed((*fields, value.reference_area, value.expand_x, value.expand_y, value.options))
        case tuple() | list():
            return b"seq:" + b"".join(len(part := _sealed(item)).to_bytes(8, "little") + part for item in value)
        case dict() | frozendict():
            return b"map:" + b"".join(
                len(part := _sealed((key, value[key]))).to_bytes(8, "little") + part for key in sorted(value, key=repr)
            )
        case _ if callable(value):
            # content identity must determine the table bytes: code + defaults + closure + bound-instance state +
            # the one-hop facet of every referenced module global. A C callable carries no code but is runtime-
            # pinned by qualname; a stateful `__call__` instance is opaque and refuses (TypeError rides `_FAULTS`)
            # rather than aliasing distinct behaviors onto one key.
            code = getattr(value, "__code__", None)
            if code is None:
                if isinstance(value, BuiltinFunctionType | MethodDescriptorType | WrapperDescriptorType):
                    return b"fn:" + _sealed(f"{getattr(value, '__module__', '')}:{value.__qualname__}")
                raise TypeError(f"content key cannot seal opaque callable {type(value).__qualname__}")
            closure = tuple(cell.cell_contents for cell in (getattr(value, "__closure__", None) or ()))
            globals_read = getattr(value, "__globals__", {})
            referenced = tuple((name, _global_facet(globals_read[name])) for name in code.co_names if name in globals_read)
            return b"fn:" + _sealed((
                f"{getattr(value, '__module__', '')}:{getattr(value, '__qualname__', type(value).__qualname__)}",
                marshal.dumps(code),
                getattr(value, "__defaults__", None),
                getattr(value, "__kwdefaults__", None),
                closure,
                getattr(value, "__self__", None),
                referenced,
            ))
        case pl.DataType():
            return b"dtype:" + repr(value).encode()
        case _:
            return _CANON.encode(value)


def _pdf_bytes(gt: RenderedGt, scale: float = 1.0, load_timeout: float = _PDF_LOAD_TIMEOUT, /) -> bytes:
    # host-coupled boundary law: a worker-level Enforcement.TERMINAL kill would orphan chromedriver, so the DRIVER
    # owns the wedge bound over BOTH of its lanes — page-load bounds navigation and script bounds the print/snapshot
    # CDP leg gt.save drives after it — each raising into the boundary rail with the context exit quitting the
    # browser; the offload stays a cooperative RELEASING crossing whose lane deadline abandons the settle loop-side.
    options = ChromeOptions()
    options.add_argument("--headless=new")
    with TemporaryDirectory() as directory, Chrome(options=options) as driver:
        driver.set_page_load_timeout(load_timeout)
        driver.set_script_timeout(load_timeout)
        sink = Path(directory) / "table.pdf"
        gt.save(file=sink, scale=scale, web_driver=driver)
        return sink.read_bytes()


def _font(gt: GT, face: FontFace) -> GT:
    match face:
        case FontFace(tag="google", google=name):
            return gt.opt_table_font(font=google_font(name=name))
        case FontFace(tag="system", system=name):
            return gt.opt_table_font(stack=name)
        case FontFace(tag="family", family=name):
            return gt.opt_table_font(font=name)
        case _ as unreachable:
            assert_never(unreachable)


def _place(at: StubLoc, columns: Cols = None, rows: Rows = None, mask: Mask = None, groups: Groups = None) -> Loc:
    selector, arity = LOC_TABLE[at]
    match arity:
        case LocArity.MASKED if mask is not None:
            return selector(mask=mask)
        case LocArity.MASKED:
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


def _shape(frame: pl.DataFrame, op: Reshape) -> pl.DataFrame:
    # `maintain_order=True` and `sort_columns=True` fix display order without a process-global category cache.
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


def _fold(gt: GT, op: TableOp) -> GT:
    match op:
        case TableOp(tag="header", header=(title, subtitle, preheader)):
            return gt.tab_header(title=md(title), subtitle=subtitle, preheader=list(preheader) if preheader is not None else None)
        case TableOp(tag="stub", stub=(rowname, group)):
            return gt.tab_stub(rowname_col=rowname, groupname_col=group)
        case TableOp(tag="stubhead", stubhead=label):
            return gt.tab_stubhead(label=label)
        case TableOp(tag="source_note", source_note=note):
            return gt.tab_source_note(source_note=md(note))
        case TableOp(tag="footnote", footnote=(text, (at, columns, rows, mask, groups), placement)):
            return gt.tab_footnote(footnote=md(text), locations=_place(at, columns, rows, mask, groups), placement=placement)
        case TableOp(tag="label", label=labels):
            return gt.cols_label(cases=dict(labels))
        case TableOp(tag="label_with", label_with=(columns, fn)):
            return gt.cols_label_with(columns=columns, fn=fn)
        case TableOp(tag="label_rotate", label_rotate=(columns, direction, align, padding)):
            return gt.cols_label_rotate(columns=columns, dir=direction, align=align, padding=padding)
        case TableOp(tag="align", align=(align, columns)):
            return gt.cols_align(align=align, columns=columns)
        case TableOp(tag="width", width=widths):
            return gt.cols_width(cases=dict(widths))
        case TableOp(tag="hide", hide=columns):
            return gt.cols_hide(columns=columns)
        case TableOp(tag="unhide", unhide=columns):
            return gt.cols_unhide(columns=columns)
        case TableOp(tag="reorder", reorder=columns):
            return gt.cols_reorder(columns=list(columns))
        case TableOp(tag="move", move=(columns, after)):
            return gt.cols_move(columns=columns, after=after)
        case TableOp(tag="move_ends", move_ends=(columns, end)):
            return (gt.cols_move_to_end if end == "end" else gt.cols_move_to_start)(columns=columns)
        case TableOp(tag="spanner", spanner=(label, columns, kwds)):
            return gt.tab_spanner(label=label, columns=columns, **kwds)
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
        case TableOp(tag="sub_missing", sub_missing=(columns, rows, text)):
            return gt.sub_missing(columns=columns, rows=rows, missing_text=text)
        case TableOp(tag="sub_zero", sub_zero=(columns, rows, text)):
            return gt.sub_zero(columns=columns, rows=rows, zero_text=text)
        case TableOp(tag="sub_small", sub_small=(columns, rows, threshold, pattern, sign)):
            return gt.sub_small_vals(columns=columns, rows=rows, threshold=threshold, small_pattern=pattern, sign=sign)
        case TableOp(tag="sub_large", sub_large=(columns, rows, threshold, pattern, sign)):
            return gt.sub_large_vals(columns=columns, rows=rows, threshold=threshold, large_pattern=pattern, sign=sign)
        case TableOp(tag="sub_values", sub_values=(columns, rows, fn, values, pattern, replacement)):
            return gt.sub_values(
                columns=columns, rows=rows, values=list(values) if values is not None else None, pattern=pattern, fn=fn, replacement=replacement
            )
        case TableOp(tag="text_transform", text_transform=((at, columns, rows, mask, groups), fn)):
            return gt.text_transform(locations=_place(at, columns, rows, mask, groups), fn=fn)
        case TableOp(tag="text_case_when", text_case_when=((at, columns, rows, mask, groups), cases, default)):
            return gt.text_case_when(*cases, default=default, locations=_place(at, columns, rows, mask, groups))
        case TableOp(tag="text_case_match", text_case_match=((at, columns, rows, mask, groups), cases, default, replace)):
            return gt.text_case_match(*cases, default=default, replace=replace, locations=_place(at, columns, rows, mask, groups))
        case TableOp(tag="text_replace", text_replace=((at, columns, rows, mask, groups), pattern, replacement)):
            return gt.text_replace(pattern=pattern, replacement=replacement, locations=_place(at, columns, rows, mask, groups))
        case TableOp(tag="summary", summary=(fns, fmt, groups, side, missing_text)):
            return gt.summary_rows(fns=dict(fns), fmt=fmt, groups=list(groups) if groups is not None else None, side=side, missing_text=missing_text)
        case TableOp(tag="grand_summary", grand_summary=(fns, fmt, side, missing_text)):
            return gt.grand_summary_rows(fns=dict(fns), fmt=fmt, side=side, missing_text=missing_text)
        case TableOp(tag="row_group_order", row_group_order=groups):
            return gt.row_group_order(groups=list(groups))
        case TableOp(tag="style", style=((at, columns, rows, mask, groups), specs)):
            return gt.tab_style(style=[_cell(spec) for spec in specs], locations=_place(at, columns, rows, mask, groups))
        case TableOp(tag="color", color=(columns, rows, kwds)):
            return gt.data_color(columns=columns, rows=rows, autocolor_text=True, **kwds)
        case TableOp(tag="css", css=rule):
            return gt.opt_css(css=rule)
        case TableOp(tag="pipe", pipe=fn):
            return gt.pipe(fn)
        case _:
            assert_never(op)


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "BorderStyleKwds",
    "CssStyleKwds",
    "DataColorKwds",
    "FillStyleKwds",
    "FmtKind",
    "FmtOptions",
    "FontFace",
    "FootnoteMarks",
    "NanoOptions",
    "NanoSpec",
    "Reshape",
    "SpannerKwds",
    "StubLoc",
    "StyleSpec",
    "SummaryFn",
    "TableFormat",
    "TableOp",
    "TablePlan",
    "TextStyleKwds",
    "Theme",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
