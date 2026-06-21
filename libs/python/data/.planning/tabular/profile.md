# [PY_DATA_PROFILE]

The graded data-quality observability owner — the data analogue of the runtime receipt-sink sitting above the `tabular/contract#QUALITY` pass/fail gate: one `QualityProfile` owner over `pointblank.Validate` plus `pointblank.Thresholds(warning, error, critical)` and the per-severity `pointblank.Actions(warning, error, critical, default, highest_only)`/`pointblank.FinalActions` callback axis, folding a tuple of `ProbeStep` rows into one chained `pb.Validate` plan, interrogating it once, grading every step at warning/error/critical severity, firing the bound severity actions, and emitting one `great_tables.GT` frame the `python:artifacts/figures` tier renders. `ProbeStep` collapses the entire pointblank step family — the `col_vals_*` comparison surface, `rows_distinct`/`rows_complete`, `col_schema_match`, `col_count_match`/`row_count_match`/`col_pct_null`, the column-aggregate checks, `conjointly`, `tbl_match`, and `specially` — into one tagged union dispatched over the boundary-bound `ProbeTables` direct-method maps, never a per-step method family, never a parallel step type per comparison operator, and never a `lambda` forwarding a renamed step. `ProfileReport` collapses the nine pointblank plan-and-table surfaces — `get_tabular_report`/`get_step_report`/`get_json_report`/`get_dataframe_report`/`get_sundered_data` over an interrogated plan plus `DataScan`/`col_summary_tbl`/`missing_vals_tbl`/`preview` over a plan-free table — into one report axis discriminated by whether the case names the interrogated plan or the raw table, emitted through one `report` entrypoint whose `GT`-carrying cases ride the `[SHAPE]` frame to artifacts and whose `json`/`dataframe`/`sundered` cases ride the wire, never two `render`/`scan` sibling dispatchers. The plan rides the agnostic `tabular/interop#INTEROP` frame and the DuckDB/parquet `data` paths from `tabular/query#QUERY` and `tabular/columnar#SCAN` straight into pointblank's own Narwhals-backed `data` admission — never a second frame translator. One interrogated `Validate` is the shared artifact every grade rail, receipt fold, and plan-consuming report reads, interrogated exactly once and never re-run per report. One `ProfileReceipt` keyed by `ContentIdentity` over the plan-content fingerprint — the probe rows and the threshold policy, not a bare label string — records the grade and contributes through runtime `ReceiptContributor`, never a parallel sink and never an HTML re-render where the in-package `GT` frame needs none.

## [01]-[INDEX]

- [01]-[PROFILE]: the graded data-quality observability owner over `pointblank` — the `ProbeStep` plan axis, the `Thresholds`/`Actions`-graded single interrogation, the one `ProfileReport` `GT`/wire axis emitting the `[SHAPE]` frame to artifacts through one `report` entrypoint, and the plan-content-keyed `ProfileReceipt` contributing through `ReceiptContributor`.

## [02]-[PROFILE]

- Owner: `QualityProfile` — the one graded data-quality observability owner over `pointblank.Validate`; `ProbeStep` the row family modeling one validation step folded into the chained plan, `Grade` the closed warning/error/critical severity axis the `Thresholds` projection grades, `ProfileReport` the closed report axis discriminating which `GT`/wire frame the interrogated plan or raw table emits, and `ProfileReceipt` the typed grade receipt keyed by `ContentIdentity` over the plan-content fingerprint carrying the real frame shape. A new validation is one `ProbeStep` row, never a `check_gt`/`check_between`/`check_unique` method family; a new report kind is one `ProfileReport` case, never a parallel emitter; a new severity callback is one `pb.Actions` slot threaded through the existing `actions=` axis, never a parallel hook. The profile sits above the `tabular/contract#QUALITY` `DataQuality`/`ContractClaim` enforcement gate — the contract gate proves the schema contract and records its breach without raising, the profile grades the live data against warning/error/critical thresholds, fires the bound severity actions, and emits the publication frame; the two are distinct planes over the same agnostic frame, never one owner.
- Cases: `ProbeStep` is the one plan axis — every case appends exactly one step to the `pb.Validate` plan, so the axis never carries a non-step. Every `columns` slot resolves through one `_columns` fold admitting a name, a `Sequence[str]`, or a `(Selector, args)` pair that mints the matching `pb.starts_with`/`ends_with`/`contains`/`matches`/`everything`/`first_n`/`last_n` selector, so a column set is a row, never a hand-built name loop. `compare(columns, op, value, na_pass)` folds `gt`/`ge`/`lt`/`le`/`eq`/`ne` through the `ProbeTables.compare` map (the unbound `Validate.col_vals_gt`/`ge`/`lt`/`le`/`eq`/`ne` resolved off the class by name) so the six comparison arms collapse to one bound-method lookup, `value` accepting a literal, a `pb.col(...)` column reference, or a `pb.ref(...)` cross-column reference — the one comparison surface discriminated by operator and value shape, never six step types · `span(columns, left, right, inclusive, outside, na_pass)` reads the bound builder off `ProbeTables.span` keyed by the `outside` polarity (`Validate.col_vals_between`/`col_vals_outside`) and threads the `(bool, bool)` inclusive endpoint pair the `_INCLUSIVE` fold mints straight into `inclusive=`, so the in-range and out-of-range checks are one row, never two cases and never four inline endpoint tests · `member(columns, present, values)` folds `isin`/`notin` polarity through `ProbeTables.member` (`Validate.col_vals_in_set`/`col_vals_not_in_set`) into the `set=` collection · `nullity(columns, present)` folds the null/non-null polarity through `ProbeTables.nullity` (`Validate.col_vals_not_null`/`col_vals_null`) · `pattern(columns, regex, inverse, na_pass)` threads the `str` pattern and `inverse` polarity into `Validate.col_vals_regex(pattern=, inverse=)`, the one regex surface, never a second `not_regex` case · `spec(columns, named, na_pass)` threads the named-spec literal into `Validate.col_vals_within_spec(spec=)` for the email/url/postal conformance checks pointblank owns natively · `ordered(columns, increasing, allow_stationary, tol, na_pass)` dispatches the monotonic direction as two inline arms (`Validate.col_vals_increasing`/`col_vals_decreasing`) because only the decreasing arm owns `decreasing_tol=`, so the asymmetric kwarg never forwards through a wrapper · `aggregate(columns, stat, op, value, tol)` folds the `(stat, op)` pair — `avg`/`sum`/`sd` × the six comparison operators — through `ProbeTables.aggregate`, one comprehension keyed by the `(stat, op)` tuple resolving each `col_{stat}_{op}` method off the class, never an eighteen-method family · `shape(kind, count, tol, inverse)` dispatches the row/column shape assertion as two inline arms because only `Validate.row_count_match` owns `tol=` while `Validate.col_count_match` rejects it · `nullfrac(columns, p, tol)` threads the null-fraction bound into `Validate.col_pct_null(p=, tol=)` · `distinct(columns_subset, complete)` folds the row-uniqueness/no-null-row polarity through `ProbeTables.distinct` (`Validate.rows_distinct`/`rows_complete`) threading the optional `columns_subset=` · `present(columns)` threads `Validate.col_exists` · `schema(declared, complete, in_order)` threads the `tabular/contract#QUALITY` `FieldShape` tuple projected to a `pb.Schema` into `Validate.col_schema_match(schema=, complete=, in_order=)` so the structural declaration the contract owner already mints drives the schema-match step, never a re-declared column list · `expr(expression)` threads `Validate.col_vals_expr(expr=)` for the row-wise boolean expression · `joint(expressions)` threads `Validate.conjointly(*expressions)` so a row passes only when every expression passes · `twin(other)` threads `Validate.tbl_match(tbl_compare=)` for whole-table equality against a comparison table · `bespoke(callable)` threads `Validate.specially(expr=)` for the arbitrary table-level predicate pointblank's escape hatch owns — the AI-driven `prompt` step stays outside this axis (the LLM-graded per-row assertion is a runtime/host concern, never a data-plane probe). Each case is matched by `match`/`case` closed by `assert_never` into the bound `Validate` builder, so the pointblank step vocabulary is one closed switch over the `ProbeTables` maps, never a per-step builder method.
- Grade: `Grade` is the closed severity axis — `WARNING`/`ERROR`/`CRITICAL` the three pointblank threshold levels plus `PASSED` the no-breach floor, ordered by severity rank so the profile's overall grade is the maximum breached level across the interrogated plan. `Grade.LEVELS` is the ascending `(WARNING, ERROR, CRITICAL)` tuple every breach sweep and breach-set fold reads, so the descending grade sweep and the ascending breach-set projection are one ordered vocabulary, never two hand-spelled level lists. `Thresholds` rides the `QualityProfile.thresholds` field as one `pb.Thresholds(warning=, error=, critical=)` value threaded into `Validate(thresholds=)` at plan open, the per-step `ProbeStep.thresholds` override an optional `pb.Thresholds` on the row threading into each builder's `thresholds=` so a single step tightens its grade without a parallel plan. The threshold limit is a count or a fraction — an `int` failing-unit count or a `float ∈ [0, 1]` failing-unit fraction — the one `Thresholds` shape pointblank grades natively, never two threshold types. `Actions` rides the parallel `QualityProfile.actions` field as one `pb.Actions(warning=, error=, critical=, default=, highest_only=)` value threaded into `Validate(actions=)` at plan open and the per-step `ProbeStep.actions` override into each builder's `actions=` — the severity-callback axis pointblank fires when a step breaches its threshold, `highest_only=True` collapsing a multi-level breach to its top severity, plus the optional `pb.FinalActions(*args)` threaded into `Validate(final_actions=)` for the one post-interrogation summary callback — so the grade-and-react surface is one threshold-plus-action policy on the profile, never a parallel observer wired beside the plan. `Grade.breaches` projects the breached levels by sweeping `Grade.LEVELS` through `plan.above_threshold(level=, i=)` — `i=None` the plan-wide breach, an `int` the per-step breach — and `Grade.of` reads `plan.all_passed()` as the affirmative `PASSED` floor then returns the maximum breached level from that same projection or `PASSED`, so the overall grade and the per-severity breach set are one fold over the plan reading one `above_threshold` sweep, never a per-level boolean tail and never two transcriptions of the level order.
- Entry: `QualityProfile.of` folds a tuple of `ProbeStep` plus the `pb.Thresholds` grade policy, the optional `pb.Actions`/`pb.FinalActions` severity-callback policy, and the optional `label`/`tbl_name`/`brief` plan metadata into one profile; `QualityProfile.interrogate` opens `pb.Validate(data, thresholds=, actions=, final_actions=, label=, tbl_name=, brief=)` over the agnostic frame or the DuckDB/parquet `data` path, folds every `ProbeStep` onto the plan through `_plan` (one `reduce` over `ProbeStep.append`, never a mutable for-loop accumulator), runs `plan.interrogate(sample_n=, sample_frac=, get_first_n=, extract_limit=)` once under the optional sampling bounds, and returns a `RuntimeRail[ProfileReceipt]` carrying the interrogated `Validate` plan — the rail is `Ok` even when steps breach because the profile records and grades but never enforces, exactly as the sibling `ContractClaim` records without raising. The single `interrogate()` call is the one execution surface; sampling (`sample_n`/`sample_frac`/`get_first_n`) and the `extract_limit` failing-row cap are call rows on that one surface, never a separate runner. `QualityProfile.report` is the one report entrypoint folding the whole nine-case `ProfileReport` axis to a `ProfileFrame` through one `match` closed by `assert_never` — the plan-consuming `tabular`/`step`/`json`/`dataframe`/`sundered` cases build and interrogate the `Validate` plan exactly once at the head of the dispatch (the `_PLAN_REPORTS` membership the report axis recovers from the tag, never a second method), the plan-free `probe`/`summary`/`missing`/`preview` cases read `pb.DataScan`/`pb.col_summary_tbl`/`pb.missing_vals_tbl`/`pb.preview` over the raw `data` table — so the interrogated-plan-versus-raw-table boundary is recovered from the case tag inside one dispatch, never a `render`/`scan` sibling-method split sharing a `boundary` prefix. The `GT`-carrying cases ride the `[SHAPE]` value the artifacts renderer renders and the `json`/`dataframe`/`sundered` cases carry the `str`/native-frame wire value, every case leaving through the one `ProfileFrame` rail, so a new report kind is one `ProfileReport` case, never a parallel render or scan method.
- Auto: a passing interrogation yields `ProfileReceipt.of` with `grade=PASSED` and `all_passed()` true; a breaching interrogation grades through `Grade.of` over the severity sweep and folds the per-step test-unit evidence into the receipt — each step's `n_passed`/`n_failed` counts and `f_passed`/`f_failed` fractions read off `plan.n_passed(scalar=False)`/`plan.n_failed(scalar=False)`/`plan.f_passed(scalar=False)`/`plan.f_failed(scalar=False)` as one `dict[int, ...]` keyed by step index, the per-level breach set reads off the one `Grade.breaches` projection over `plan.above_threshold(level=)`, and the real frame shape reads off `pb.get_row_count(data)`/`pb.get_column_count(data)` so the receipt `shape` carries the graded frame's `(rows, columns)` rather than a degenerate step-count tuple — so the grade receipt carries frame shape, step count, per-step pass/fail counts and both fractions, the per-severity breach set, and the overall grade as one typed evidence stream, never re-derived from the raw frame and never a parallel record per level. The interrogated plan stays lazy where the backend admits it — the polars/DuckDB/ibis `data` path pushes the validation into the scan because pointblank's Narwhals engine never materializes the frame it grades.
- Receipt: `ProfileReceipt.contribute` emits an emitted-phase `Receipt.of` row through runtime `ReceiptContributor` keyed by `ContentIdentity` over the plan-content fingerprint — `QualityProfile.fingerprint` folds the `repr` of every `ProbeStep` row and the `repr` of the `thresholds` policy into one canonical byte stream through `ContentIdentity.of("profile", ...)` so an unchanged probe set and threshold policy reuses its key byte-stable while a single changed threshold, a tightened per-step override, or an added probe flips it — the plan identity the bare `(label, step-count, grade)` string could not carry, since a changed threshold leaves label, count, and a still-passing grade untouched. The receipt is the graded data-quality observability evidence, never replacing the typed `tabular/columnar#SCAN` `QueryReceipt` and never minting the identity the runtime owns. It is the data-plane sibling of the runtime receipt-sink: it records the grade above the gate, the gate's `ContractClaim` records the contract at the gate, and neither raises.
- Shape: `QualityProfile.report` emits one `ProfileFrame` carrying the `great_tables.GT` object on its `frame` slot as an opaque `Any` plus the `kind` discriminant and the `grade` — data never imports `great_tables`, never re-renders the `GT` to HTML, and never reaches into the `GT` internals; the `python:artifacts/figures` great-tables tier owns the render and reads the `ProfileFrame` `[SHAPE]` value, exactly as the `tabular/columnar#SCAN` corpus-row wire hands a flat record to the documents tier. The `json`/`dataframe`/`sundered` report cases carry the `str`/native-frame wire value on the same `frame` slot, so the publication report, the machine-readable JSON, the in-memory grade frame, and the passing/failing row split all leave through one `ProfileFrame` rail, never four emitters.
- Packages: `pointblank` (`Validate(data, thresholds=, actions=, final_actions=, label=, tbl_name=, brief=)`/`Validate.col_vals_gt`/`ge`/`lt`/`le`/`eq`/`ne`/`col_vals_between(left=, right=, inclusive=)`/`col_vals_outside(left=, right=, inclusive=)`/`col_vals_in_set(set=)`/`col_vals_not_in_set(set=)`/`col_vals_not_null`/`col_vals_null`/`col_vals_regex(pattern=, inverse=)`/`col_vals_within_spec(spec=)`/`col_vals_increasing(allow_stationary=)`/`col_vals_decreasing(allow_stationary=, decreasing_tol=)`/`col_avg_gt`/`col_sum_gt`/`col_sd_gt`/`col_vals_expr(expr=)`/`col_exists`/`col_schema_match(schema=, complete=, in_order=)`/`col_count_match(count=, inverse=)`/`row_count_match(count=, tol=, inverse=)`/`col_pct_null(p=, tol=)`/`rows_distinct(columns_subset=)`/`rows_complete(columns_subset=)`/`conjointly`/`tbl_match(tbl_compare=)`/`specially(expr=)`/`interrogate(sample_n=, sample_frac=, get_first_n=, extract_limit=)`/`all_passed()`/`above_threshold(level=, i=)`/`n_passed(i=, scalar=)`/`n_failed(i=, scalar=)`/`f_passed(i=, scalar=)`/`f_failed(i=, scalar=)`/`get_tabular_report(title=, incl_header=, incl_footer=) -> GT`/`get_step_report(i=, columns_subset=, limit=) -> GT`/`get_json_report(use_fields=, exclude_fields=) -> str`/`get_dataframe_report(tbl_type=)`/`get_sundered_data(type=)`/`Thresholds(warning=, error=, critical=)`/`Actions(warning=, error=, critical=, default=, highest_only=)`/`FinalActions(*args)`/`Schema(columns=)`/`col`/`ref`/`starts_with`/`ends_with`/`contains`/`matches`/`everything`/`first_n`/`last_n`/`get_row_count(data) -> int`/`get_column_count(data) -> int`/`DataScan(data, tbl_name=).get_tabular_report(show_sample_data=) -> GT`/`col_summary_tbl(data, tbl_name=) -> GT`/`missing_vals_tbl(data) -> GT`/`preview(data, columns_subset=, n_head=, n_tail=, limit=) -> GT`, import at boundary scope under `# noqa: PLC0415` since pointblank transitively loads the manifest-banned module-level `polars`/`great_tables`), `tabular/contract#QUALITY` (`FieldShape` projected to a `pb.Schema` for the `schema` probe, the structural declaration the contract owner already mints), `tabular/interop#INTEROP` (`FrameInterop.source` the backend tag for the receipt fingerprint; the agnostic frame passes through unmodified into pointblank's own Narwhals `data` admission, never lowered through `FrameInterop.translate` because pointblank consumes the agnostic frame directly), runtime (`RuntimeRail`/`boundary`/`ContentIdentity`/`ContentKey`/`ReceiptContributor`/`Receipt`).
- Growth: a new comparison/range/membership/null-fraction/uniqueness check is one `ProbeStep` row threading its `ProbeTables.compare`/`span`/`member`/`distinct` polarity map; a new column-aggregate stat is one row in the `_STATS` axis the `ProbeTables.aggregate` comprehension folds; a new column selector is one `Selector` literal the `_columns` fold resolves off the `pb` namespace; a new report kind is one `ProfileReport` case on the one `report` axis, a plan-consuming case naming its tag in `_PLAN_REPORTS` and a plan-free case omitting it; a new severity level is one `Grade.LEVELS` row plus one `Thresholds`/`Actions` field; a per-step threshold or action override is the existing `ProbeStep.thresholds`/`ProbeStep.actions` field threaded into the builder's `thresholds=`/`actions=`; a post-interrogation summary callback is the existing `QualityProfile.final_actions` field threaded into `Validate(final_actions=)`; a sampling or extract-limit knob is the existing `interrogate` call row; the AI-driven `prompt` step is admitted as a `ProbeStep` row only when an LLM model handle arrives through the runtime host seam, never a module-top dependency on this data-plane page; a second backend `data` path is admitted free by pointblank's own Narwhals engine with zero profile-cluster change.
- Boundary: pointblank owns the validation plan, the warning/error/critical threshold grading, the severity-action callbacks, and the `great_tables.GT` report emission; `great_tables` owns the renderable frame downstream and stays `python:artifacts/figures`-owned; Narwhals owns the frame normalization inside pointblank; runtime owns the identity, the receipt rail, and the LLM/host seam. No raising in domain logic (the profile records and grades, never enforces — `assert_below_threshold` is pointblank's raising gate and stays unbound on this page), no second frame translator beside pointblank's own Narwhals admission, no `great_tables` import on this page, no HTML re-render where the `GT` frame needs none, no re-interrogation of an already-interrogated plan, no `ContentIdentity` mint the runtime owns duplicated, no parallel receipt sink beside the runtime `ReceiptContributor`; a `check_gt`/`check_between`/`check_unique` per-step method family, a parallel step type per comparison operator, a `lambda` table forwarding a renamed `pb.Validate` step method where the unbound method binds directly, an eighteen-method column-aggregate family where the `ProbeTables.aggregate` `(stat, op)` comprehension folds them, a hand-built column-name loop where the `_columns` selector fold resolves it, a `render`/`scan` sibling-method split where one `report` axis discriminates the interrogated-plan-versus-raw-table boundary from the case tag, a parallel severity-callback observer where the `actions=` axis threads it, a per-severity boolean grade tail where one `Grade.breaches` sweep folds them, a label-string content key where the probe-and-threshold fingerprint flips on a changed threshold, a degenerate step-count receipt shape where `get_row_count`/`get_column_count` carry the real frame shape, a second claim type beside `ProfileReceipt`, and an exception-driven grade gate are the deleted forms.

```python signature
from __future__ import annotations

from builtins import frozendict
from collections.abc import Callable, Sequence
from enum import IntEnum
from functools import reduce
from typing import TYPE_CHECKING, Any, ClassVar, Final, Literal, assert_never

from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    import pointblank as pb
    from great_tables import GT

    from rasm.data.tabular.contract import FieldShape

type Selector = Literal["starts_with", "ends_with", "contains", "matches", "everything", "first_n", "last_n"]
type Columns = str | Sequence[str] | tuple[Selector, tuple[Any, ...]] | Any
type Comparand = float | int | str | Any
type Inclusive = Literal["both", "neither", "left", "right"]

_INCLUSIVE: Final[frozendict[Inclusive, tuple[bool, bool]]] = frozendict({
    "both": (True, True), "neither": (False, False), "left": (True, False), "right": (False, True),
})
_PLAN_REPORTS: Final[frozenset[str]] = frozenset({"tabular", "step", "json", "dataframe", "sundered"})


def _columns(spec: Columns) -> Any:
    import pointblank as pb  # noqa: PLC0415

    match spec:
        case (str() as name, tuple() as args) if hasattr(pb, name):
            return getattr(pb, name)(*args)
        case resolved:
            return resolved


class Grade(IntEnum):
    PASSED = 0
    WARNING = 1
    ERROR = 2
    CRITICAL = 3

    LEVELS: "ClassVar[tuple[Grade, ...]]"

    @classmethod
    def breaches(cls, plan: "pb.Validate", i: int | None = None) -> "tuple[Grade, ...]":
        return tuple(level for level in cls.LEVELS if plan.above_threshold(level=level.name.lower(), i=i))

    @classmethod
    def of(cls, plan: "pb.Validate") -> "Grade":
        return max(cls.breaches(plan), default=cls.PASSED) if not plan.all_passed() else cls.PASSED


Grade.LEVELS = (Grade.WARNING, Grade.ERROR, Grade.CRITICAL)


@tagged_union(frozen=True)
class ProbeStep:
    tag: Literal[
        "compare", "span", "member", "nullity", "pattern", "spec", "ordered", "aggregate",
        "shape", "nullfrac", "distinct", "present", "schema", "expr", "joint", "twin", "bespoke",
    ] = tag()
    compare: tuple[Columns, Literal["gt", "ge", "lt", "le", "eq", "ne"], Comparand, bool] = case()
    span: tuple[Columns, Comparand, Comparand, Inclusive, bool, bool] = case()
    member: tuple[Columns, bool, tuple[Any, ...]] = case()
    nullity: tuple[Columns, bool] = case()
    pattern: tuple[Columns, str, bool, bool] = case()
    spec: tuple[Columns, str, bool] = case()
    ordered: tuple[Columns, bool, bool, float | None, bool] = case()
    aggregate: tuple[Columns, Literal["avg", "sum", "sd"], Literal["gt", "ge", "lt", "le", "eq", "ne"], float, float] = case()
    shape: tuple[Literal["row", "col"], int, float, bool] = case()
    nullfrac: tuple[Columns, float, float] = case()
    distinct: tuple[tuple[str, ...] | None, bool] = case()
    present: Columns = case()
    schema: tuple[tuple[FieldShape, ...], bool, bool] = case()
    expr: Any = case()
    joint: tuple[Any, ...] = case()
    twin: Any = case()
    bespoke: Callable[[Any], Any] = case()
    thresholds: "pb.Thresholds | None" = None
    actions: "pb.Actions | None" = None

    def append(self, plan: "pb.Validate", tables: "ProbeTables") -> "pb.Validate":
        t, a = self.thresholds, self.actions
        match self:
            case ProbeStep(tag="compare", compare=(columns, op, value, na_pass)):
                return tables.compare[op](plan, _columns(columns), value=value, na_pass=na_pass, thresholds=t, actions=a)
            case ProbeStep(tag="span", span=(columns, left, right, inclusive, outside, na_pass)):
                return tables.span[outside](plan, _columns(columns), left=left, right=right, inclusive=_INCLUSIVE[inclusive], na_pass=na_pass, thresholds=t, actions=a)
            case ProbeStep(tag="member", member=(columns, present, values)):
                return tables.member[present](plan, _columns(columns), set=list(values), thresholds=t, actions=a)
            case ProbeStep(tag="nullity", nullity=(columns, present)):
                return tables.nullity[present](plan, _columns(columns), thresholds=t, actions=a)
            case ProbeStep(tag="pattern", pattern=(columns, regex, inverse, na_pass)):
                return plan.col_vals_regex(_columns(columns), pattern=regex, inverse=inverse, na_pass=na_pass, thresholds=t, actions=a)
            case ProbeStep(tag="spec", spec=(columns, named, na_pass)):
                return plan.col_vals_within_spec(_columns(columns), spec=named, na_pass=na_pass, thresholds=t, actions=a)
            case ProbeStep(tag="ordered", ordered=(columns, True, allow_stationary, _, na_pass)):
                return plan.col_vals_increasing(_columns(columns), allow_stationary=allow_stationary, na_pass=na_pass, thresholds=t, actions=a)
            case ProbeStep(tag="ordered", ordered=(columns, False, allow_stationary, tol, na_pass)):
                return plan.col_vals_decreasing(_columns(columns), allow_stationary=allow_stationary, decreasing_tol=tol, na_pass=na_pass, thresholds=t, actions=a)
            case ProbeStep(tag="aggregate", aggregate=(columns, stat, op, value, tol)):
                return tables.aggregate[(stat, op)](plan, _columns(columns), value=value, tol=tol, thresholds=t, actions=a)
            case ProbeStep(tag="shape", shape=("row", count, tol, inverse)):
                return plan.row_count_match(count=count, tol=tol, inverse=inverse, thresholds=t, actions=a)
            case ProbeStep(tag="shape", shape=("col", count, _, inverse)):
                return plan.col_count_match(count=count, inverse=inverse, thresholds=t, actions=a)
            case ProbeStep(tag="nullfrac", nullfrac=(columns, p, tol)):
                return plan.col_pct_null(_columns(columns), p=p, tol=tol, thresholds=t, actions=a)
            case ProbeStep(tag="distinct", distinct=(subset, complete)):
                return tables.distinct[complete](plan, columns_subset=subset, thresholds=t, actions=a)
            case ProbeStep(tag="present", present=columns):
                return plan.col_exists(_columns(columns), thresholds=t, actions=a)
            case ProbeStep(tag="schema", schema=(declared, complete, in_order)):
                return plan.col_schema_match(schema=_schema_of(declared), complete=complete, in_order=in_order, thresholds=t, actions=a)
            case ProbeStep(tag="expr", expr=expression):
                return plan.col_vals_expr(expr=expression, thresholds=t, actions=a)
            case ProbeStep(tag="joint", joint=expressions):
                return plan.conjointly(*expressions, thresholds=t, actions=a)
            case ProbeStep(tag="twin", twin=other):
                return plan.tbl_match(tbl_compare=other, thresholds=t, actions=a)
            case ProbeStep(tag="bespoke", bespoke=callable_):
                return plan.specially(expr=callable_, thresholds=t, actions=a)
            case unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class ProfileReport:
    tag: Literal["tabular", "step", "json", "dataframe", "sundered", "probe", "summary", "missing", "preview"] = tag()
    tabular: tuple[str, bool | None, bool | None] = case()
    step: tuple[int, tuple[str, ...] | None, int] = case()
    json: tuple[tuple[str, ...] | None, tuple[str, ...] | None] = case()
    dataframe: Literal["polars", "pandas", "duckdb"] = case()
    sundered: Literal["pass", "fail"] = case()
    probe: bool = case()
    summary: bool = case()
    missing: bool = case()
    preview: tuple[tuple[str, ...] | None, int, int, int] = case()


class ProfileFrame(Struct, frozen=True):
    kind: str
    grade: Grade
    frame: Any


class ProfileReceipt(Struct, frozen=True):
    label: str
    shape: tuple[int, int]
    steps: int
    grade: Grade
    passed: frozendict[int, int]
    failed: frozendict[int, int]
    passed_fraction: frozendict[int, float]
    failed_fraction: frozendict[int, float]
    breached: tuple[str, ...]
    content_key: ContentKey

    @classmethod
    def of(cls, label: str, plan: "pb.Validate", data: Any, steps: int, key: ContentKey) -> "ProfileReceipt":
        import pointblank as pb  # noqa: PLC0415

        return cls(
            label=label,
            shape=(pb.get_row_count(data), pb.get_column_count(data)),
            steps=steps,
            grade=Grade.of(plan),
            passed=frozendict(plan.n_passed(scalar=False)),
            failed=frozendict(plan.n_failed(scalar=False)),
            passed_fraction=frozendict(plan.f_passed(scalar=False)),
            failed_fraction=frozendict(plan.f_failed(scalar=False)),
            breached=tuple(level.name.lower() for level in Grade.breaches(plan)),
            content_key=key,
        )

    def contribute(self) -> Receipt:
        rows, cols = self.shape
        return Receipt.of(
            "emitted", "quality-profile", self.label,
            {"grade": self.grade.name, "shape": f"{rows}x{cols}", "steps": str(self.steps), "breached": "|".join(self.breached)},
        )


class QualityProfile(Struct, frozen=True):
    steps: tuple[ProbeStep, ...]
    thresholds: "pb.Thresholds | None" = None
    actions: "pb.Actions | None" = None
    final_actions: "pb.FinalActions | None" = None
    label: str = "profile"
    tbl_name: str | None = None
    brief: bool | str = False

    @classmethod
    def of(cls, *steps: ProbeStep, thresholds: "pb.Thresholds | None" = None, actions: "pb.Actions | None" = None, final_actions: "pb.FinalActions | None" = None, label: str = "profile", tbl_name: str | None = None) -> "QualityProfile":
        return cls(steps=steps, thresholds=thresholds, actions=actions, final_actions=final_actions, label=label, tbl_name=tbl_name)

    def interrogate(self, data: Any, *, sample_n: int | None = None, sample_frac: float | None = None, get_first_n: int | None = None, extract_limit: int = 500) -> "RuntimeRail[ProfileReceipt]":
        return boundary(f"profile.interrogate.{self.label}", lambda: self._interrogate(data, sample_n, sample_frac, get_first_n, extract_limit))

    def report(self, data: Any, report: ProfileReport) -> "RuntimeRail[ProfileFrame]":
        return boundary(f"profile.report.{report.tag}", lambda: self._report(data, report))

    def fingerprint(self) -> ContentKey:
        spine = (f"{self.label}|{self.thresholds!r}".encode(), *(repr(step).encode() for step in self.steps))
        return ContentIdentity.of("profile", spine)

    def _interrogate(self, data: Any, sample_n: int | None, sample_frac: float | None, get_first_n: int | None, extract_limit: int) -> ProfileReceipt:
        plan = self._plan(data).interrogate(sample_n=sample_n, sample_frac=sample_frac, get_first_n=get_first_n, extract_limit=extract_limit)
        return ProfileReceipt.of(self.label, plan, data, len(self.steps), self.fingerprint())

    def _report(self, data: Any, report: ProfileReport) -> ProfileFrame:
        import pointblank as pb  # noqa: PLC0415

        if report.tag in _PLAN_REPORTS:
            plan = self._plan(data).interrogate()
            grade = Grade.of(plan)
            match report:
                case ProfileReport(tag="tabular", tabular=(title, header, footer)):
                    return ProfileFrame(report.tag, grade, plan.get_tabular_report(title=title, incl_header=header, incl_footer=footer))
                case ProfileReport(tag="step", step=(i, subset, limit)):
                    return ProfileFrame(report.tag, grade, plan.get_step_report(i=i, columns_subset=subset, limit=limit))
                case ProfileReport(tag="json", json=(use_fields, exclude_fields)):
                    return ProfileFrame(report.tag, grade, plan.get_json_report(use_fields=use_fields, exclude_fields=exclude_fields))
                case ProfileReport(tag="dataframe", dataframe=tbl_type):
                    return ProfileFrame(report.tag, grade, plan.get_dataframe_report(tbl_type=tbl_type))
                case ProfileReport(tag="sundered", sundered=side):
                    return ProfileFrame(report.tag, grade, plan.get_sundered_data(type=side))
                case unreachable:
                    assert_never(unreachable)
        match report:
            case ProfileReport(tag="probe", probe=show_sample):
                return ProfileFrame(report.tag, Grade.PASSED, pb.DataScan(data, tbl_name=self.tbl_name).get_tabular_report(show_sample_data=show_sample))
            case ProfileReport(tag="summary"):
                return ProfileFrame(report.tag, Grade.PASSED, pb.col_summary_tbl(data, tbl_name=self.tbl_name))
            case ProfileReport(tag="missing"):
                return ProfileFrame(report.tag, Grade.PASSED, pb.missing_vals_tbl(data))
            case ProfileReport(tag="preview", preview=(subset, n_head, n_tail, limit)):
                return ProfileFrame(report.tag, Grade.PASSED, pb.preview(data, columns_subset=subset, n_head=n_head, n_tail=n_tail, limit=limit))
            case unreachable:
                assert_never(unreachable)

    def _plan(self, data: Any) -> "pb.Validate":
        import pointblank as pb  # noqa: PLC0415

        tables = ProbeTables.bind(pb)
        root = pb.Validate(data, thresholds=self.thresholds, actions=self.actions, final_actions=self.final_actions, label=self.label, tbl_name=self.tbl_name, brief=self.brief)
        return reduce(lambda plan, step: step.append(plan, tables), self.steps, root)


def _schema_of(declared: "tuple[FieldShape, ...]") -> "pb.Schema":
    import pointblank as pb  # noqa: PLC0415

    return pb.Schema(columns=[(shape.field, shape.logical_type) for shape in declared])
```

`ProbeTables` binds each closed polarity directly onto the unbound `pb.Validate` step method — `pb.Validate.col_vals_gt` invoked as `compare["gt"](plan, columns, value=..., thresholds=t, actions=a)` threads `plan` as `self` and the per-step `thresholds=`/`actions=` policy as the two kwargs every step row shares, exactly as the sibling `tabular/contract#QUALITY` `_CMP` table binds `Check.ge`, so no `lambda` forwards a rename. The bind is boundary-scoped because the manifest bans the module-level `pointblank` import, and the `aggregate` cross-product is one comprehension over the `(stat, op)` axes resolving each `col_{stat}_{op}` method off the class by name, never eighteen hand-written rows. The order and shape arms stay inline two-case dispatch because `col_vals_increasing`/`col_count_match` drop the `decreasing_tol`/`tol` kwarg their siblings own, so no table can absorb the asymmetric signature without a forwarding wrapper.

```python signature
_STATS: Final[tuple[str, ...]] = ("avg", "sum", "sd")
_OPS: Final[tuple[str, ...]] = ("gt", "ge", "lt", "le", "eq", "ne")


class ProbeTables(Struct, frozen=True):
    compare: frozendict[str, Callable[..., "pb.Validate"]]
    span: frozendict[bool, Callable[..., "pb.Validate"]]
    member: frozendict[bool, Callable[..., "pb.Validate"]]
    nullity: frozendict[bool, Callable[..., "pb.Validate"]]
    aggregate: frozendict[tuple[str, str], Callable[..., "pb.Validate"]]
    distinct: frozendict[bool, Callable[..., "pb.Validate"]]

    @classmethod
    def bind(cls, pb: Any) -> "ProbeTables":
        v = pb.Validate
        return cls(
            compare=frozendict({op: getattr(v, f"col_vals_{op}") for op in _OPS}),
            span=frozendict({False: v.col_vals_between, True: v.col_vals_outside}),
            member=frozendict({True: v.col_vals_in_set, False: v.col_vals_not_in_set}),
            nullity=frozendict({True: v.col_vals_not_null, False: v.col_vals_null}),
            aggregate=frozendict({(stat, op): getattr(v, f"col_{stat}_{op}") for stat in _STATS for op in _OPS}),
            distinct=frozendict({True: v.rows_complete, False: v.rows_distinct}),
        )
```

```mermaid
flowchart TD
    steps["tuple[ProbeStep]"] -->|reduce over ProbeStep.append| plan["pb.Validate"]
    tables["ProbeTables.bind(pb)"] -->|bound col_vals_* methods| plan
    thr["pb.Thresholds(warning,error,critical)"] -->|Validate(thresholds=)| plan
    acts["pb.Actions / pb.FinalActions"] -->|Validate(actions=,final_actions=)| plan
    data["agnostic frame / DuckDB·parquet path"] -->|Validate(data=)| plan
    plan -->|interrogate(sample_n,extract_limit) once| interr["interrogated Validate"]
    interr -->|all_passed then Grade.breaches sweep| grade["Grade: PASSED·WARNING·ERROR·CRITICAL"]
    interr -->|n_passed·n_failed·f_passed·f_failed·above_threshold| receipt["ProfileReceipt"]
    data -->|get_row_count·get_column_count| receipt
    fp["fingerprint: probe rows + thresholds repr"] -->|ContentIdentity.of profile| key["ContentKey"]
    grade --> receipt
    key --> receipt
    receipt -->|contribute| sink["runtime ReceiptContributor"]
    interr -->|report over plan-consuming ProfileReport| frame["ProfileFrame carrying GT"]
    scanned["DataScan·col_summary_tbl·missing_vals_tbl·preview"] -->|report over plan-free ProfileReport| frame
    frame -->|SHAPE| artifacts["python:artifacts/figures great-tables tier"]
```

## [03]-[RESEARCH]

- [POINTBLANK_PLAN_SURFACE]: the `pointblank` `Validate(data, thresholds=, actions=, final_actions=, label=, tbl_name=, brief=)` plan root, the `interrogate(sample_n=, sample_frac=, get_first_n=, extract_limit=)` single execution surface, and the grade rails `all_passed()`/`above_threshold(level=, i=)`/`n_passed(i=, scalar=)`/`n_failed(i=, scalar=)`/`f_passed(i=, scalar=)`/`f_failed(i=, scalar=)` the `Grade.breaches`/`Grade.of` sweep and `ProfileReceipt.of` evidence fold transcribe are catalogue-confirmed against the folder `pointblank` `.api` ([03]-[ENTRYPOINTS] [01]-[02]-[08]-[14]). `all_passed() -> bool` is the affirmative grade floor [08], and `above_threshold(level='warning'|'error'|'critical', i=None)` returns the breach `bool` for the named severity — `i=None` the plan-wide breach, an `int` the per-step breach — so the one `Grade.breaches` sweep over `Grade.LEVELS` reads the breach set for both the overall `Grade.of` maximum and the per-severity receipt set off one `above_threshold` projection, never two transcriptions of the level order; `n_passed`/`n_failed`/`f_passed`/`f_failed` with `scalar=False` return the `dict[int, int]`/`dict[int, float]` per-step map the receipt folds into its four `frozendict` slots. `get_row_count(data) -> int`/`get_column_count(data) -> int` ([03]-[ENTRYPOINTS] grading/profiling functions [08]) are the catalogue-confirmed frame-shape rails the receipt `shape` slot reads, so the receipt carries the graded frame's `(rows, columns)` rather than a degenerate step-count tuple. The `interrogate` sampling and `extract_limit` knobs are call rows on the one execution surface; the interrogated `Validate` is the one shared artifact every grade rail, receipt fold, and plan-consuming report reads, never re-interrogated per report. Settled fence code.
- [POINTBLANK_STEP_VOCABULARY]: the `ProbeStep` cases bind catalogue-confirmed `pb.Validate` step methods ([03]-[ENTRYPOINTS] step algebra [01]-[19]) through the `ProbeTables` boundary-bound maps and the inline asymmetric arms: `col_vals_gt(columns, value, na_pass=, thresholds=)` is the catalogue-named comparison row [01] with the `value` slot accepting a literal/`col(...)`/`ref(...)`, `col_vals_between`/`col_vals_outside(columns, left, right, inclusive=(bool, bool), na_pass=, thresholds=)` (the `ProbeTables.span` map, `inclusive` a two-tuple the `_INCLUSIVE` fold mints) are both catalogue-named [02], `col_vals_in_set`/`col_vals_not_in_set(columns, set, thresholds=)` (the `ProbeTables.member` map, `set=` the membership collection) [03], `col_vals_not_null`/`col_vals_null(columns, thresholds=)` (the `ProbeTables.nullity` map) [04], `col_vals_regex(columns, pattern, inverse=, na_pass=, thresholds=)` [05], `col_vals_within_spec(columns, spec, na_pass=, thresholds=)` [06], `col_vals_increasing(columns, allow_stationary=, na_pass=, thresholds=)`/`col_vals_decreasing(columns, allow_stationary=, decreasing_tol=, na_pass=, thresholds=)` (two inline arms, `decreasing_tol` legal only on the decreasing arm per [07]) [07], `col_vals_expr(expr, thresholds=)` [08], `col_exists(columns, thresholds=)` [09], `col_schema_match(schema, complete=, in_order=, thresholds=)` [10], `col_count_match(count, inverse=, thresholds=)`/`row_count_match(count, tol=, inverse=, thresholds=)` (two inline arms, `tol=` legal only on `row_count_match` per [11]-[12]) [11]-[12], `col_pct_null(columns, p, tol=, thresholds=)` [15], `rows_distinct`/`rows_complete(columns_subset=, thresholds=)` (the `ProbeTables.distinct` map) [13], `conjointly(*exprs, thresholds=)` [17], `tbl_match(tbl_compare, thresholds=)` (the `twin` case, whole-table equality) [16], and `specially(expr, thresholds=)` [18] are signature-confirmed against the `.api` step algebra rows. The `ProbeTables.bind(pb)` owner binds each map value to the unbound class method (`pb.Validate.col_vals_gt`) so the `ProbeStep.append` lookup invokes it with the threaded `plan` as `self` — the same direct-bind idiom the sibling `tabular/contract#QUALITY` `_CMP`/`Check.ge` table uses, never a `lambda` rename. Every step method returns `Validate` for chaining and carries the per-step `thresholds=`/`brief=`/`active=` policy, so the `_plan` `reduce` appends each step onto the threaded plan. The AI-driven `prompt` step ([19]) is deliberately excluded from the `ProbeStep` axis — the LLM-graded per-row assertion routes through a runtime/host model seam, never a data-plane probe. Settled fence code, except the `ProbeTables.compare` operator-sibling and `ProbeTables.aggregate` column-aggregate-sibling spellings below.
- [POINTBLANK_COMPARE_SIBLINGS]: the `.api` step algebra row [01] names the comparison surface as `col_vals_gt` and abbreviates the operator family as "(`gt`/`ge`/`lt`/`le`/`eq`/`ne`)" in the capability column, so `col_vals_gt` is catalogue-confirmed and the `ProbeTables.compare` map resolves `getattr(pb.Validate, f"col_vals_{op}")` over the `_OPS` axis into one keyed lookup. The exact spelling of the five sibling members the catalogue abbreviates — `col_vals_ge`/`col_vals_lt`/`col_vals_le`/`col_vals_eq`/`col_vals_ne` (whether each operator suffix is the literal two-letter form versus a `greater_than`/`equal_to` long form pointblank's docstring-graded API may use) is unverified against the live distribution, so the `getattr(pb.Validate, f"col_vals_{op}")` resolution over `_OPS` is the one marked seam and stays a RESEARCH item until the full `col_vals_*` operator member set is confirmed; the `col_vals_gt` row and the operator-keyed fold structure are settled.
- [POINTBLANK_AGGREGATE_SIBLINGS]: the `.api` step algebra row [14] enumerates the column-aggregate family as `col_avg_gt(columns, value=None, tol=0, thresholds=, brief=, actions=, active=)` and names the cross-product as "(avg/sum/sd) × comparison", so `col_avg_gt` is catalogue-confirmed and the `ProbeTables.aggregate` comprehension resolves `getattr(pb.Validate, f"col_{stat}_{op}")` over the `_STATS × _OPS` product into one `(stat, op)`-keyed lookup. The exact spelling of the fifteen sibling members the catalogue abbreviates — `col_avg_ge`/`col_avg_lt`/`col_avg_le`/`col_avg_eq`/`col_avg_ne`, the five `col_sum_*` siblings, and the five `col_sd_*` siblings (whether each comparison operator suffix is `gt`/`ge`/`lt`/`le`/`eq`/`ne` uniformly across the `avg`/`sum`/`sd` stats, versus a subset the catalogue does not enumerate) is unverified against the live distribution, so the `getattr(pb.Validate, f"col_{stat}_{op}")` resolution over `_STATS × _OPS` is the one marked seam and stays a RESEARCH item until the full column-aggregate member set is confirmed; the `col_avg_gt` row and the `(stat, op)`-keyed fold structure are settled.
- [POINTBLANK_REPORT_SURFACE]: the one `ProfileReport` axis the single `report` entrypoint dispatches binds catalogue-confirmed report emitters ([03]-[ENTRYPOINTS] [03]-[07] and the profiling functions [04]-[07]) — the plan-consuming cases named in `_PLAN_REPORTS` build and interrogate the `Validate` plan once at the dispatch head, the plan-free cases read the standalone profiling functions over the raw table, the interrogated-plan-versus-raw-table boundary recovered from the case tag inside one fold rather than a `render`/`scan` sibling-method split: `get_tabular_report(title=':default:', incl_header=None, incl_footer=None) -> GT` (the `tabular` case), `get_step_report(i, columns_subset=None, header=':default:', limit=10) -> GT` (the `step` case), `get_json_report(use_fields=None, exclude_fields=None) -> str` (the `json` wire case), `get_dataframe_report(tbl_type='polars') -> Any` (the `dataframe` wire case), `get_sundered_data(type='pass'|'fail') -> Any` (the `sundered` wire case [07], the passing/failing row split), `DataScan(data, tbl_name=None).get_tabular_report(show_sample_data=False) -> GT` (the `probe` case), `col_summary_tbl(data, tbl_name=None) -> GT` (the `summary` case), `missing_vals_tbl(data) -> GT` (the `missing` case), and `preview(data, columns_subset=None, n_head=5, n_tail=5, limit=50) -> GT` (the `preview` case) are signature-confirmed against the `.api`. The column-set resolution the `_columns` fold mints — `starts_with`/`ends_with`/`contains`/`matches`/`everything`/`first_n`/`last_n` ([03]-[ENTRYPOINTS] [15]) — is catalogue-confirmed as the selector family every `columns` slot admits. The `GT`-returning cases carry the `great_tables.GT` object on the `ProfileFrame.frame` slot as the `[SHAPE]` value the `python:artifacts/figures` tier renders, so data binds the `GT` opaquely and never imports `great_tables` (the `from great_tables import GT` line is `TYPE_CHECKING`-only); the `json`/`dataframe`/`sundered` cases carry the `str`/native-frame wire value on the same slot. Settled fence code.
- [POINTBLANK_GRADE_SURFACE]: the `pb.Thresholds(warning=None, error=None, critical=None)` grade policy threaded into `Validate(thresholds=)` and each step's `thresholds=`, and the `pb.Schema(columns=[(name, dtype), ...])` declaration the `schema` probe mints from the `tabular/contract#QUALITY` `FieldShape` tuple through `_schema_of`, are catalogue-confirmed against the folder `pointblank` `.api` ([03]-[ENTRYPOINTS] grading functions [01], [02]-[PUBLIC_TYPES] `Schema` [05]). The `Thresholds` limit is an `int` failing-unit count or a `float ∈ [0, 1]` failing-unit fraction, the one threshold shape pointblank grades natively. The `pb.col(exprs)`/`pb.ref(column_name)` column references the `compare` `value` slot admits ([03]-[ENTRYPOINTS] [14]) are catalogue-confirmed. The exact `Schema` constructor keyword — whether the column-name/dtype pairs bind through `Schema(columns=[...])` versus a positional `Schema([...])` or a `**kwargs` column map — is the one marked seam on `_schema_of` and stays a RESEARCH item until the `Schema` constructor signature is confirmed against the live distribution; the `Thresholds` constructor, the `col`/`ref` references, and the `FieldShape`-to-pairs projection are settled.
- [POINTBLANK_ACTION_SURFACE]: the `pb.Actions(warning=None, error=None, critical=None, default=None, highest_only=True)` per-severity callback policy and the `pb.FinalActions(*args)` post-interrogation callback the `QualityProfile.actions`/`final_actions` fields thread into `Validate(actions=, final_actions=)` at plan open, and the per-step `ProbeStep.actions` override threaded into each builder's `actions=`, are catalogue-confirmed against the folder `pointblank` `.api` ([02]-[PUBLIC_TYPES] `Actions` [03]/`FinalActions` [04], [03]-[ENTRYPOINTS] grading functions [02]-[03]); the catalogue confirms `actions=` is a per-step policy kwarg every step row carries ([03]-[ENTRYPOINTS] step algebra preamble, the `col_avg_gt(..., actions=, ...)` row [14] naming it explicitly) so the `ProbeTables`-bound maps and the inline arms thread `actions=a` beside `thresholds=t` with no signature widening, `highest_only=True` collapsing a multi-level breach to its top severity. The profile binds the callback axis but never fires it — pointblank fires the bound action on breach internally — and never threads `pb.assert_below_threshold`, the one raising rail this records-not-enforces page leaves unbound. Settled fence code.
- [POINTBLANK_PROVISION]: `pointblank` is absent from the data manifest at authoring; execution provisions it into the cp315 environment (`installed 0.24.0` reflected on cp315 per the `.api` package surface). The whole-package boundary-scoped import (`import pointblank as pb` under `# noqa: PLC0415`) holds because pointblank transitively loads the manifest-banned module-level `polars`/`great_tables`, exactly as the sibling `tabular/contract#COLLECTION` `dataframely` and `tabular/contract#QUALITY` `pandera` arms import at boundary scope. The `pb.Thresholds`/`pb.Actions`/`pb.FinalActions`/`pb.Schema` annotations resolve under `TYPE_CHECKING` and the runtime `pb` uses (`Validate`/`DataScan`/`col_summary_tbl`/`missing_vals_tbl`/`preview`/`Schema`/`get_row_count`/`get_column_count` plus the `starts_with`/`ends_with`/`contains`/`matches`/`everything`/`first_n`/`last_n` selectors the `_columns` fold resolves off the `pb` namespace) import boundary-scoped inside `_plan`/`_report`/`_schema_of`/`_columns`/`ProbeTables.bind`/`ProfileReceipt.of`. No fence member contradicts a sibling RESEARCH item: the `pointblank` step, grade, action, and report surfaces this page settles are disjoint from the `pandera`/`dataframely` surfaces the `tabular/contract` page settles.
