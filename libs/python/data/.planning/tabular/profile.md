# [PY_DATA_PROFILE]

The graded data-quality observability owner — the data-plane analogue of the runtime receipt-sink, sitting above the `tabular/contract#QUALITY` pass/fail gate: `QualityProfile` folds `ProbeStep` rows into one chained `pointblank.Validate` plan, interrogates it once, grades every step at warning/error/critical severity, fires the bound severity actions, and emits one `great_tables.GT` frame the `python:artifacts/visualization/table` tier renders through its `TablePlan.rendered` opaque-GT egress. The contract gate and the profile are two planes over one agnostic frame — the gate proves the schema contract and records its breach, the profile grades the live data and fires actions above that gate — never one owner, and neither raises.

The plan rides the agnostic `tabular/interop#INTEROP` frame and the DuckDB/parquet paths from `tabular/query#QUERY` and `tabular/columnar#SCAN` straight into pointblank's own Narwhals `data` admission, never a second frame translator. `FieldShape` imports downward from its `interop#INTEROP` minter for the `schema` probe's `pb.Schema` projection — the same structural declaration the contract gate reads. One interrogated `Validate` is the shared artifact every grade rail, receipt fold, and plan-consuming report reads, interrogated once and never re-run per report. `ProfileReceipt` keys by runtime `ContentIdentity` over the plan-content fingerprint and contributes through `ReceiptContributor`; the identity, the receipt rail, and the LLM/host seam are runtime-owned.

## [01]-[INDEX]

- [01]-[PROFILE]: the graded data-quality observability owner over `pointblank` — the `ProbeStep` plan axis, the `Thresholds`/`Actions`-graded single interrogation, the `ProfileReport` `GT`/wire axis through one `report` entrypoint, and the plan-content-keyed `ProfileReceipt`.

## [02]-[PROFILE]

- Owner: `QualityProfile` over `pointblank.Validate` — the one graded data-quality observability owner. Closed types: `StepKind` the step-payload union; `ProbeStep` the `Struct` row pairing one `StepKind` with an optional per-step `thresholds`/`actions` override; `Grade` the ordered `PASSED`/`WARNING`/`ERROR`/`CRITICAL` severity axis; `ProfileReport` the report axis over the `GT`/wire frames; `ProfileReceipt` the grade receipt keyed by `ContentIdentity` over the plan-content fingerprint.
- Cases: `StepKind` collapses the entire pointblank step family into one tagged union dispatched over the boundary-bound `ProbeTables` method maps and matched by `match`/`case` closed by `assert_never`, never a per-comparison step type and never a `lambda` forwarding a renamed step. The per-step `thresholds`/`actions` override rides the `ProbeStep` `Struct` wrapper, not a union field: `@tagged_union.__init__` treats every keyword as a case candidate and raises `TypeError("One and only one case can be specified")` the moment a construction passes a case plus a policy field, so the override is unconstructible on the union. Every `columns` slot resolves through one `ProbeTables.cols` fold over the bound `pb` namespace — a name, a `Sequence[str]`, or a `(Selector, args)` pair minting the matching `pb.starts_with`/`ends_with`/… selector — so a column set is a row, never a name loop and never a per-step `import pointblank`.
  - `compare` folds `gt`/`ge`/`lt`/`le`/`eq`/`ne` through `ProbeTables.compare` (the unbound `Validate.col_vals_*` off the class), `value` a literal, a `pb.col(...)`, or a `pb.ref(...)` cross-column reference — one comparison surface, never six step types.
  - `span`/`member`/`nullity`/`distinct` each fold a two-way polarity through their `ProbeTables` map (`col_vals_between`/`outside`, `in_set`/`not_in_set`, `not_null`/`null`, `rows_distinct`/`rows_complete`); `span` threads the `_INCLUSIVE` `(bool, bool)` endpoint pair and `distinct` the optional `columns_subset`.
  - `pattern`/`spec`/`present`/`schema`/`expr`/`joint`/`twin`/`bespoke` are one surface each — `col_vals_regex(inverse=)` (no second `not_regex`), `col_vals_within_spec`, `col_exists`, `col_schema_match` fed the `interop#INTEROP` `FieldShape` tuple projected to `pb.Schema` (the contract's structural declaration, never re-listed), `col_vals_expr`, `conjointly`, `tbl_match`, and `specially` the escape hatch. The AI-driven `prompt` step stays outside this axis — an LLM-graded per-row assertion is a runtime/host concern, never a data-plane probe.
  - `ordered` dispatches two inline arms, not a table row, because the tolerance kwarg mirrors the direction: `col_vals_increasing` owns `decreasing_tol=` (permitted backward slack), `col_vals_decreasing` owns `increasing_tol=`, so each arm threads the single `tol` into its own asymmetric kwarg.
  - `aggregate` folds one comprehension over `_STATS × _AGG_OPS` keyed by `(stat, op)`, resolving each `col_{stat}_{op}` off the class, never a fifteen-method family; `AggOp` is `Operator` minus `ne` because pointblank exposes no `col_{stat}_ne`.
  - `shape`/`nullfrac` — `shape` dispatches two inline arms because only `row_count_match` owns `tol=` while `col_count_match` rejects it; `nullfrac` threads the null-fraction bound into `col_pct_null(p=, tol=)`.
- Entry: `QualityProfile.of` folds the `ProbeStep` tuple plus the `pb.Thresholds` grade policy, the optional `pb.Actions`/`pb.FinalActions` severity-callback policy, and the `label`/`tbl_name`/`brief` plan metadata into one profile. `interrogate` opens `pb.Validate(data, thresholds=, actions=, final_actions=, …)` over the agnostic frame or the DuckDB/parquet path, folds every `ProbeStep` onto the plan through one `reduce` over `ProbeStep.append` (never a mutable loop), runs `plan.interrogate(sample_n=, sample_frac=, get_first_n=, extract_limit=)` once inside one `async_boundary` over the banded `on_thread` hop — interrogation materializes the whole backend frame, a blocking leg that never rides the loop — then `.bind`s the railed `fingerprint(sampling)` key and `.map`s the resolved `ContentKey` into `ProfileReceipt.of`, returning a `RuntimeRail[ProfileReceipt]`. The rail is `Ok` even when steps breach because the profile records and grades but never enforces, exactly as the sibling `ContractClaim`. The single `interrogate()` is the one execution surface; sampling and the `extract_limit` failing-row cap are call rows, never a separate runner. `Thresholds`/`Actions` ride owner fields threaded into `Validate` at plan open, the per-step `ProbeStep.thresholds`/`actions` override threaded into each builder so a step tightens its grade without a parallel plan; `highest_only=True` collapses a multi-level breach to its top severity, and the threshold limit is an `int` failing-unit count or a `float ∈ [0, 1]` fraction, the one shape pointblank grades. `report` is the one report entrypoint folding the nine-case `ProfileReport` to a `ProfileFrame` through one total `match` closed by `assert_never`: the plan-consuming `tabular`/`step`/`json`/`dataframe`/`sundered` cases read a `cache`-memoized `graded()` closure so the plan builds and interrogates at most once and only when a plan-consuming arm runs, while the plan-free `probe`/`summary`/`missing`/`preview` cases read `pb.DataScan`/`col_summary_tbl`/`missing_vals_tbl`/`preview` over the raw table and never touch `graded()` — the interrogated-plan-versus-raw-table boundary recovered structurally from the matched arm, never a `render`/`scan` sibling split.
- Auto: a passing interrogation yields `ProfileReceipt.of` at `grade=PASSED` with `all_passed()` true; a breach grades through `Grade.of` and folds per-step evidence into the receipt. `Grade` is ordered by severity rank so the overall grade is the maximum breached level; `Grade.LEVELS` is the ascending `(WARNING, ERROR, CRITICAL)` tuple both the breach sweep and the breach-set projection read, never two hand-spelled lists. `Grade.breaches` sweeps `LEVELS` through `plan.above_threshold(level=, i=)` (`i=None` plan-wide, an `int` per-step) and `Grade.of` reads `plan.all_passed()` then returns the max breached level or `PASSED` — one fold, never a per-level boolean tail. The receipt carries the graded `(rows, columns)` off `pb.get_row_count`/`get_column_count` (never a degenerate step-count tuple), the step count, per-step `n_passed`/`n_failed`/`f_passed`/`f_failed` off `plan.*(scalar=False)` as a `dict` keyed by step index, and the per-severity breach set — one typed evidence stream, never re-derived from the raw frame. The plan stays lazy where the backend admits it: the polars/DuckDB/ibis path pushes validation into the scan because pointblank's Narwhals engine never materializes the frame it grades.
- Output: `report` emits one `ProfileFrame` carrying the `great_tables.GT` on its `frame` slot as opaque `Any` plus the `kind` discriminant and the `grade` — data never imports `great_tables`, never re-renders to HTML, never reaches into `GT` internals; the `python:artifacts/visualization/table` tier renders it through its `TablePlan.rendered` opaque-GT egress and reads the `[SHAPE]` value, exactly as the `tabular/columnar#SCAN` corpus wire hands a flat record to the documents tier. The `json`/`dataframe`/`sundered` cases carry the `str`/native-frame wire value on the same slot, so the publication report, the machine-readable JSON, the grade frame, and the passing/failing row split all leave through one `ProfileFrame` rail, never four emitters.
- Receipt: `ProfileReceipt.contribute` yields one emitted-phase row through the two-argument `Receipt.of(owner, evidence)` factory decomposing the `(phase, subject, facts)` triple — never the four-positional form the owner does not expose — satisfying the `ReceiptContributor.contribute -> Iterable[Receipt]` Protocol, never a bare single `Receipt`; the `rows`/`columns`/`steps` counts ride as native `int` scalars. The receipt keys by `ContentIdentity` over the plan-content fingerprint: `QualityProfile.fingerprint` folds one deterministic msgspec-JSON row per `ProbeStep` (tag, payload, override presence — a callable projected to its stable code identity `(module, qualname, marshalled bytecode)` so two distinct `<lambda>` predicates never collide) plus the plan-level row over the label, the sampling bound, and the threshold/action policy, returning the railed `RuntimeRail[ContentKey]` the `interrogate` rail threads through `.bind`/`.map` rather than collapsing into a field. An unchanged probe set, threshold policy, and sampling bound reuses its key byte-stable; a changed threshold, a tightened override, an added probe, or a widened sampling bound flips it — the graded-evidence identity a bare `(label, step-count, grade)` string cannot carry, since a changed threshold leaves all three untouched while the counts and grade shift.
- Packages: `pointblank` (`Validate(data, thresholds=, actions=, final_actions=, label=, tbl_name=, brief=)`/`col_vals_gt`/`ge`/`lt`/`le`/`eq`/`ne`/`col_vals_between(left=, right=, inclusive=)`/`col_vals_outside`/`col_vals_in_set(set=)`/`col_vals_not_in_set`/`col_vals_not_null`/`col_vals_null`/`col_vals_regex(pattern=, inverse=)`/`col_vals_within_spec(spec=)`/`col_vals_increasing(allow_stationary=, decreasing_tol=)`/`col_vals_decreasing(allow_stationary=, increasing_tol=)`/`col_{avg,sum,sd}_{gt,ge,lt,le,eq}`/`col_vals_expr(expr=)`/`col_exists`/`col_schema_match(schema=, complete=, in_order=)`/`col_count_match(count=, inverse=)`/`row_count_match(count=, tol=, inverse=)`/`col_pct_null(p=, tol=)`/`rows_distinct(columns_subset=)`/`rows_complete`/`conjointly`/`tbl_match(tbl_compare=)`/`specially(expr=)`/`interrogate(sample_n=, sample_frac=, get_first_n=, extract_limit=)`/`all_passed()`/`above_threshold(level=, i=)`/`n_passed`/`n_failed`/`f_passed`/`f_failed(i=, scalar=)`/`get_tabular_report(title=, incl_header=, incl_footer=) -> GT`/`get_step_report(i=, columns_subset=, limit=) -> GT`/`get_json_report(use_fields=, exclude_fields=) -> str`/`get_dataframe_report(tbl_type=)`/`get_sundered_data(type=)`/`Thresholds(warning=, error=, critical=)`/`Actions(warning=, error=, critical=, default=, highest_only=)`/`FinalActions`/`Schema`/`col`/`ref`/`starts_with`/`ends_with`/`contains`/`matches`/`everything`/`first_n`/`last_n`/`get_row_count`/`get_column_count`/`DataScan(data, tbl_name=)`/`col_summary_tbl`/`missing_vals_tbl`/`preview(data, columns_subset=, n_head=, n_tail=, limit=)`, imported at boundary scope under `# noqa: PLC0415` since the manifest-banned import transitively loads the heavy `polars`/`great_tables` engines), `tabular/interop#INTEROP` (`FieldShape` projected to a `pb.Schema` for the `schema` probe, imported downward; the agnostic `nw.DataFrame`/`nw.LazyFrame` passes through unmodified into pointblank's own Narwhals `data` admission, never lowered through `FrameInterop.translate`), `beartype` (`@beartype(conf=FAULT_CONF)` on the public `of`/`interrogate`/`report` seams so a malformed `ProbeStep`/`ProfileReport`/`data` argument raises the `BeartypeCallHintViolation` root the `reliability/faults#FAULT` `api` row folds onto the rail; the internal folds and kernels over already-admitted values carry none), runtime (`RuntimeRail`/`boundary`/`async_boundary`/`on_thread`/`FAULT_CONF`/`ContentIdentity`/`ContentKey`/`ReceiptContributor`/`Receipt`).
- Growth: a new comparison/range/membership/uniqueness check is one `ProbeStep` threading its `ProbeTables` polarity map; a new column-aggregate stat is one `_STATS` row the `aggregate` comprehension folds; a new column selector is one `Selector` literal the `cols` fold resolves; a new report kind is one `ProfileReport` case (plan-consuming reads `graded()`, plan-free reads the raw table); a new severity level is one `Grade.LEVELS` row plus one `Thresholds`/`Actions` field; a per-step threshold or action override is the existing `ProbeStep` field; a post-interrogation summary callback is the existing `final_actions` field; a sampling or extract-limit knob is a call row on `interrogate`; the AI-driven `prompt` step is admitted as a `ProbeStep` only when an LLM handle arrives through the runtime host seam, never a module-top dependency; a second backend `data` path is admitted free by pointblank's Narwhals engine.
- Boundary: pointblank owns the validation plan, the warning/error/critical threshold grading, the severity-action callbacks, and the `great_tables.GT` emission; `great_tables` owns the renderable frame downstream and stays `python:artifacts/visualization/table`-owned; Narwhals owns the frame normalization inside pointblank; runtime owns the identity, the receipt rail, and the LLM/host seam. No raising in domain logic — the profile records and grades, never enforces; `assert_below_threshold` is pointblank's raising gate and stays unbound on this page. No second frame translator beside pointblank's Narwhals admission, no `great_tables` import here, no HTML re-render where the `GT` frame needs none, and no re-interrogation of an already-interrogated plan.

```python signature
import marshal
from collections.abc import Callable, Iterable, Sequence
from enum import IntEnum
from functools import cache, reduce
from typing import TYPE_CHECKING, Any, ClassVar, Final, Literal, assert_never, cast

from beartype import beartype
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from msgspec import json as msgjson

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, async_boundary, boundary
from rasm.runtime.lanes import on_thread
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    import pointblank as pb
    from pointblank.column import Column, ReferenceColumn

    from rasm.data.tabular.interop import FieldShape

type Selector = Literal["starts_with", "ends_with", "contains", "matches", "everything", "first_n", "last_n"]
type Columns = str | Sequence[str] | tuple[Selector, tuple[Any, ...]] | "Column"
type Comparand = float | int | str | "Column" | "ReferenceColumn"
type Grades = Literal["warning", "error", "critical"]
type Operator = Literal["gt", "ge", "lt", "le", "eq", "ne"]
type AggOp = Literal["gt", "ge", "lt", "le", "eq"]
type Stat = Literal["avg", "sum", "sd"]
type ReportKind = Literal["tabular", "step", "json", "dataframe", "sundered", "probe", "summary", "missing", "preview"]
type Inclusive = Literal["both", "neither", "left", "right"]

_INCLUSIVE: Final[Map[Inclusive, tuple[bool, bool]]] = Map.of_seq([
    ("both", (True, True)),
    ("neither", (False, False)),
    ("left", (True, False)),
    ("right", (False, True)),
])


class Grade(IntEnum):
    PASSED = 0
    WARNING = 1
    ERROR = 2
    CRITICAL = 3

    LEVELS: "ClassVar[tuple[Grade, ...]]"

    @property
    def label(self) -> Grades:
        return cast(Grades, self.name.lower())

    @classmethod
    def breaches(cls, plan: "pb.Validate", i: int | None = None) -> "tuple[Grade, ...]":
        return tuple(level for level in cls.LEVELS if plan.above_threshold(level=level.label, i=i))

    @classmethod
    def of(cls, plan: "pb.Validate") -> "Grade":
        return max(cls.breaches(plan), default=cls.PASSED) if not plan.all_passed() else cls.PASSED


Grade.LEVELS = (Grade.WARNING, Grade.ERROR, Grade.CRITICAL)


@tagged_union(frozen=True)
class StepKind:
    tag: Literal[
        "compare",
        "span",
        "member",
        "nullity",
        "pattern",
        "spec",
        "ordered",
        "aggregate",
        "shape",
        "nullfrac",
        "distinct",
        "present",
        "schema",
        "expr",
        "joint",
        "twin",
        "bespoke",
    ] = tag()
    compare: tuple[Columns, Operator, Comparand, bool] = case()
    span: tuple[Columns, Comparand, Comparand, Inclusive, bool, bool] = case()
    member: tuple[Columns, bool, tuple[Any, ...]] = case()
    nullity: tuple[Columns, bool] = case()
    pattern: tuple[Columns, str, bool, bool] = case()
    spec: tuple[Columns, str, bool] = case()
    ordered: tuple[Columns, bool, bool, float | None, bool] = case()
    aggregate: tuple[Columns, Stat, AggOp, float, float] = case()
    shape: tuple[Literal["row", "col"], int, float, bool] = case()
    nullfrac: tuple[Columns, float, float] = case()
    distinct: tuple[tuple[str, ...] | None, bool] = case()
    present: Columns = case()
    schema: tuple[tuple[FieldShape, ...], bool, bool] = case()
    expr: Any = case()
    joint: tuple[Any, ...] = case()
    twin: Any = case()
    bespoke: Callable[[Any], Any] = case()


class ProbeStep(Struct, frozen=True):
    kind: StepKind
    thresholds: "pb.Thresholds | None" = None
    actions: "pb.Actions | None" = None

    def append(self, plan: "pb.Validate", tables: "ProbeTables") -> "pb.Validate":
        t, a = self.thresholds, self.actions
        match self.kind:
            case StepKind(tag="compare", compare=(columns, op, value, na_pass)):
                return tables.compare[op](plan, tables.cols(columns), value=value, na_pass=na_pass, thresholds=t, actions=a)
            case StepKind(tag="span", span=(columns, left, right, inclusive, outside, na_pass)):
                return tables.span[outside](
                    plan, tables.cols(columns), left=left, right=right, inclusive=_INCLUSIVE[inclusive], na_pass=na_pass, thresholds=t, actions=a
                )
            case StepKind(tag="member", member=(columns, present, values)):
                return tables.member[present](plan, tables.cols(columns), set=list(values), thresholds=t, actions=a)
            case StepKind(tag="nullity", nullity=(columns, present)):
                return tables.nullity[present](plan, tables.cols(columns), thresholds=t, actions=a)
            case StepKind(tag="pattern", pattern=(columns, regex, inverse, na_pass)):
                return plan.col_vals_regex(tables.cols(columns), pattern=regex, inverse=inverse, na_pass=na_pass, thresholds=t, actions=a)
            case StepKind(tag="spec", spec=(columns, named, na_pass)):
                return plan.col_vals_within_spec(tables.cols(columns), spec=named, na_pass=na_pass, thresholds=t, actions=a)
            case StepKind(tag="ordered", ordered=(columns, True, allow_stationary, tol, na_pass)):
                return plan.col_vals_increasing(
                    tables.cols(columns), allow_stationary=allow_stationary, decreasing_tol=tol, na_pass=na_pass, thresholds=t, actions=a
                )
            case StepKind(tag="ordered", ordered=(columns, False, allow_stationary, tol, na_pass)):
                return plan.col_vals_decreasing(
                    tables.cols(columns), allow_stationary=allow_stationary, increasing_tol=tol, na_pass=na_pass, thresholds=t, actions=a
                )
            case StepKind(tag="aggregate", aggregate=(columns, stat, op, value, tol)):
                return tables.aggregate[(stat, op)](plan, tables.cols(columns), value=value, tol=tol, thresholds=t, actions=a)
            case StepKind(tag="shape", shape=("row", count, tol, inverse)):
                return plan.row_count_match(count=count, tol=tol, inverse=inverse, thresholds=t, actions=a)
            case StepKind(tag="shape", shape=("col", count, _, inverse)):
                return plan.col_count_match(count=count, inverse=inverse, thresholds=t, actions=a)
            case StepKind(tag="nullfrac", nullfrac=(columns, p, tol)):
                return plan.col_pct_null(tables.cols(columns), p=p, tol=tol, thresholds=t, actions=a)
            case StepKind(tag="distinct", distinct=(subset, complete)):
                return tables.distinct[complete](plan, columns_subset=subset, thresholds=t, actions=a)
            case StepKind(tag="present", present=columns):
                return plan.col_exists(tables.cols(columns), thresholds=t, actions=a)
            case StepKind(tag="schema", schema=(declared, complete, in_order)):
                return plan.col_schema_match(schema=tables.schema(declared), complete=complete, in_order=in_order, thresholds=t, actions=a)
            case StepKind(tag="expr", expr=expression):
                return plan.col_vals_expr(expr=expression, thresholds=t, actions=a)
            case StepKind(tag="joint", joint=expressions):
                return plan.conjointly(*expressions, thresholds=t, actions=a)
            case StepKind(tag="twin", twin=other):
                return plan.tbl_match(tbl_compare=other, thresholds=t, actions=a)
            case StepKind(tag="bespoke", bespoke=callable_):
                return plan.specially(expr=callable_, thresholds=t, actions=a)
            case unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class ProfileReport:
    tag: ReportKind = tag()
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
    kind: ReportKind
    grade: Grade
    frame: Any


class ProfileReceipt(Struct, frozen=True):
    label: str
    shape: tuple[int, int]
    steps: int
    grade: Grade
    passed: Map[int, int]
    failed: Map[int, int]
    passed_fraction: Map[int, float]
    failed_fraction: Map[int, float]
    breached: tuple[Grades, ...]
    content_key: ContentKey

    @classmethod
    def of(cls, label: str, plan: "pb.Validate", data: Any, steps: int, key: ContentKey) -> "ProfileReceipt":
        import pointblank as pb  # noqa: PLC0415

        return cls(
            label=label,
            shape=(pb.get_row_count(data), pb.get_column_count(data)),
            steps=steps,
            grade=Grade.of(plan),
            passed=Map.of_seq(plan.n_passed(scalar=False).items()),
            failed=Map.of_seq(plan.n_failed(scalar=False).items()),
            passed_fraction=Map.of_seq(plan.f_passed(scalar=False).items()),
            failed_fraction=Map.of_seq(plan.f_failed(scalar=False).items()),
            breached=tuple(level.label for level in Grade.breaches(plan)),
            content_key=key,
        )

    def contribute(self) -> Iterable[Receipt]:
        rows, cols = self.shape
        yield Receipt.of(
            "quality-profile",
            (
                "emitted",
                self.label,
                {"grade": self.grade.name, "rows": rows, "columns": cols, "steps": self.steps, "breached": "|".join(self.breached)},
            ),
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
    @beartype(conf=FAULT_CONF)
    def of(
        cls,
        *steps: ProbeStep,
        thresholds: "pb.Thresholds | None" = None,
        actions: "pb.Actions | None" = None,
        final_actions: "pb.FinalActions | None" = None,
        label: str = "profile",
        tbl_name: str | None = None,
        brief: bool | str = False,
    ) -> "QualityProfile":
        return cls(steps=steps, thresholds=thresholds, actions=actions, final_actions=final_actions, label=label, tbl_name=tbl_name, brief=brief)

    @beartype(conf=FAULT_CONF)
    async def interrogate(
        self, data: Any, *, sample_n: int | None = None, sample_frac: float | None = None, get_first_n: int | None = None, extract_limit: int = 500
    ) -> "RuntimeRail[ProfileReceipt]":
        # sampling folds into the fingerprint, so a re-sampled run — same probes, different grade and counts — never reuses a byte-stable key.
        # pointblank interrogation materializes the whole backend frame — a blocking leg riding the banded thread hop, never the loop.
        sampling = (sample_n, sample_frac, get_first_n, extract_limit)
        interrogated = await async_boundary(
            f"profile.interrogate.{self.label}",
            lambda: on_thread(
                lambda: self._plan(data).interrogate(sample_n=sample_n, sample_frac=sample_frac, get_first_n=get_first_n, extract_limit=extract_limit)
            ),
        )
        return interrogated.bind(lambda plan: self.fingerprint(sampling).map(lambda key: ProfileReceipt.of(self.label, plan, data, len(self.steps), key)))

    @beartype(conf=FAULT_CONF)
    async def report(self, data: Any, report: ProfileReport) -> "RuntimeRail[ProfileFrame]":
        # report extraction re-walks the interrogated backend — the same blocking materialization, the same banded hop.
        return await async_boundary(f"profile.report.{report.tag}", lambda: on_thread(self._report, data, report))

    def fingerprint(self, sampling: tuple[int | None, float | None, int | None, int]) -> "RuntimeRail[ContentKey]":
        # `leaf` renders a callable to its stable code identity (module, qualname, marshalled bytecode) so two `<lambda>`
        # predicates never collide on a bare qualname; a `repr`-of-callable carries a run-varying memory address.
        def leaf(value: object) -> object:
            code = getattr(value, "__code__", None)
            if code is not None:
                return (getattr(value, "__module__", ""), getattr(value, "__qualname__", ""), marshal.dumps(code))
            return getattr(value, "__qualname__", None) or str(value)

        spine = (
            msgjson.encode((self.label, sampling, leaf(self.thresholds), leaf(self.actions)), enc_hook=leaf, order="deterministic"),
            *(
                msgjson.encode(
                    (step.kind.tag, getattr(step.kind, step.kind.tag), leaf(step.thresholds), leaf(step.actions)),
                    enc_hook=leaf,
                    order="deterministic",
                )
                for step in self.steps
            ),
        )
        return ContentIdentity.of("profile", spine)

    def _report(self, data: Any, report: ProfileReport) -> ProfileFrame:
        import pointblank as pb  # noqa: PLC0415

        graded = cache(lambda: self._plan(data).interrogate())
        match report:
            case ProfileReport(tag="tabular", tabular=(title, header, footer)):
                return ProfileFrame(report.tag, Grade.of(graded()), graded().get_tabular_report(title=title, incl_header=header, incl_footer=footer))
            case ProfileReport(tag="step", step=(i, subset, limit)):
                return ProfileFrame(report.tag, Grade.of(graded()), graded().get_step_report(i=i, columns_subset=subset, limit=limit))
            case ProfileReport(tag="json", json=(use_fields, exclude_fields)):
                return ProfileFrame(report.tag, Grade.of(graded()), graded().get_json_report(use_fields=use_fields, exclude_fields=exclude_fields))
            case ProfileReport(tag="dataframe", dataframe=tbl_type):
                return ProfileFrame(report.tag, Grade.of(graded()), graded().get_dataframe_report(tbl_type=tbl_type))
            case ProfileReport(tag="sundered", sundered=side):
                return ProfileFrame(report.tag, Grade.of(graded()), graded().get_sundered_data(type=side))
            case ProfileReport(tag="probe", probe=show_sample):
                return ProfileFrame(
                    report.tag, Grade.PASSED, pb.DataScan(data, tbl_name=self.tbl_name).get_tabular_report(show_sample_data=show_sample)
                )
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
        root = pb.Validate(
            data,
            thresholds=self.thresholds,
            actions=self.actions,
            final_actions=self.final_actions,
            label=self.label,
            tbl_name=self.tbl_name,
            brief=self.brief,
        )
        return reduce(lambda plan, step: step.append(plan, tables), self.steps, root)
```

`ProbeTables.bind` captures the `pb` handle on `ns` once per plan and resolves each closed polarity onto the unbound `pb.Validate` step method — `compare["gt"]` is `Validate.col_vals_gt` invoked as `(plan, columns, value=, thresholds=t, actions=a)`, threading `plan` as `self` and the shared per-step policy kwargs, exactly as the sibling `tabular/contract#QUALITY` `_CMP` binds `Check.ge`, so no `lambda` forwards a rename. The bind is boundary-scoped because the manifest bans the module-level `pointblank` import; `cols` (the `(Selector, args)` selector-or-passthrough fold) and `schema` (the `FieldShape`-tuple-to-`pb.Schema` projection) resolve off that one captured `ns`, collapsing the per-step selector imports a standalone fold pays, and the `aggregate` cross-product is one comprehension over `_STATS × _AGG_OPS` resolving each `col_{stat}_{op}` off the class.

```python signature
_STATS: Final[tuple[Stat, ...]] = ("avg", "sum", "sd")
_OPS: Final[tuple[Operator, ...]] = ("gt", "ge", "lt", "le", "eq", "ne")
_AGG_OPS: Final[tuple[AggOp, ...]] = ("gt", "ge", "lt", "le", "eq")


class ProbeTables(Struct, frozen=True):
    ns: Any
    compare: Map[Operator, Callable[..., "pb.Validate"]]
    span: Map[bool, Callable[..., "pb.Validate"]]
    member: Map[bool, Callable[..., "pb.Validate"]]
    nullity: Map[bool, Callable[..., "pb.Validate"]]
    aggregate: Map[tuple[Stat, AggOp], Callable[..., "pb.Validate"]]
    distinct: Map[bool, Callable[..., "pb.Validate"]]

    def cols(self, spec: Columns) -> Any:
        match spec:
            case (str() as name, tuple() as args) if hasattr(self.ns, name):
                return getattr(self.ns, name)(*args)
            case resolved:
                return resolved

    def schema(self, declared: "tuple[FieldShape, ...]") -> "pb.Schema":
        return self.ns.Schema(columns=[(shape.field, shape.logical_type) for shape in declared])

    @classmethod
    def bind(cls, pb: Any) -> "ProbeTables":
        v = pb.Validate
        return cls(
            ns=pb,
            compare=Map.of_seq((op, getattr(v, f"col_vals_{op}")) for op in _OPS),
            span=Map.of_seq([(False, v.col_vals_between), (True, v.col_vals_outside)]),
            member=Map.of_seq([(True, v.col_vals_in_set), (False, v.col_vals_not_in_set)]),
            nullity=Map.of_seq([(True, v.col_vals_not_null), (False, v.col_vals_null)]),
            aggregate=Map.of_seq(((stat, op), getattr(v, f"col_{stat}_{op}")) for stat in _STATS for op in _AGG_OPS),
            distinct=Map.of_seq([(True, v.rows_complete), (False, v.rows_distinct)]),
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
    fp["fingerprint: canonical msgspec-JSON probe rows"] -->|ContentIdentity.of profile| key["ContentKey"]
    grade --> receipt
    key --> receipt
    receipt -->|contribute| sink["runtime ReceiptContributor"]
    interr -->|report over plan-consuming ProfileReport| frame["ProfileFrame carrying GT"]
    scanned["DataScan·col_summary_tbl·missing_vals_tbl·preview"] -->|report over plan-free ProfileReport| frame
    frame -->|SHAPE| artifacts["python:artifacts/visualization/table great-tables tier"]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
