# [PY_DATA_PROFILE]

The graded data-quality observability owner — the data analogue of the runtime receipt-sink sitting above the `tabular/contract#QUALITY` pass/fail gate: one `QualityProfile` owner over `pointblank.Validate` plus `pointblank.Thresholds(warning, error, critical)` and the per-severity `pointblank.Actions(warning, error, critical, default, highest_only)`/`pointblank.FinalActions` callback axis, folding a tuple of `ProbeStep` rows into one chained `pb.Validate` plan, interrogating it once, grading every step at warning/error/critical severity, firing the bound severity actions, and emitting one `great_tables.GT` frame the `python:artifacts/figures` tier renders. `StepKind` collapses the entire pointblank step family — the `col_vals_*` comparison surface, `rows_distinct`/`rows_complete`, `col_schema_match`, `col_count_match`/`row_count_match`/`col_pct_null`, the column-aggregate checks, `conjointly`, `tbl_match`, and `specially` — into one tagged union dispatched over the boundary-bound `ProbeTables` direct-method maps, the thin `ProbeStep` `Struct` pairing one `StepKind` with its optional per-step `thresholds`/`actions` override, never a per-step method family, never a parallel step type per comparison operator, and never a `lambda` forwarding a renamed step. `ProfileReport` collapses the nine pointblank plan-and-table surfaces — `get_tabular_report`/`get_step_report`/`get_json_report`/`get_dataframe_report`/`get_sundered_data` over an interrogated plan plus `DataScan`/`col_summary_tbl`/`missing_vals_tbl`/`preview` over a plan-free table — into one report axis discriminated by whether the case names the interrogated plan or the raw table, emitted through one `report` entrypoint whose `GT`-carrying cases ride the `[SHAPE]` frame to artifacts and whose `json`/`dataframe`/`sundered` cases ride the wire, never two `render`/`scan` sibling dispatchers. The plan rides the agnostic `tabular/interop#INTEROP` frame and the DuckDB/parquet `data` paths from `tabular/query#QUERY` and `tabular/columnar#SCAN` straight into pointblank's own Narwhals-backed `data` admission — never a second frame translator. One interrogated `Validate` is the shared artifact every grade rail, receipt fold, and plan-consuming report reads, interrogated exactly once and never re-run per report. One `ProfileReceipt` keyed by `ContentIdentity` over the plan-content fingerprint — the probe rows and the threshold policy, not a bare label string — records the grade and contributes through runtime `ReceiptContributor`, never a parallel sink and never an HTML re-render where the in-package `GT` frame needs none.

## [01]-[INDEX]

- [01]-[PROFILE]: the graded data-quality observability owner over `pointblank` — the `ProbeStep` plan axis, the `Thresholds`/`Actions`-graded single interrogation, the one `ProfileReport` `GT`/wire axis emitting the `[SHAPE]` frame to artifacts through one `report` entrypoint, and the plan-content-keyed `ProfileReceipt` contributing through `ReceiptContributor`.

## [02]-[PROFILE]

- Owner: `QualityProfile` — the one graded data-quality observability owner over `pointblank.Validate`; `StepKind` the closed step-payload union and `ProbeStep` the `Struct` row pairing one `StepKind` with its optional `thresholds`/`actions` override, modeling one validation step folded into the chained plan, `Grade` the closed warning/error/critical severity axis the `Thresholds` projection grades, `ProfileReport` the closed report axis discriminating which `GT`/wire frame the interrogated plan or raw table emits, and `ProfileReceipt` the typed grade receipt keyed by `ContentIdentity` over the plan-content fingerprint carrying the real frame shape. A new validation is one `StepKind` case wrapped in one `ProbeStep` row, never a `check_gt`/`check_between`/`check_unique` method family; a new report kind is one `ProfileReport` case, never a parallel emitter; a new severity callback is one `pb.Actions` slot threaded through the existing `actions=` axis, never a parallel hook. The profile sits above the `tabular/contract#QUALITY` `DataQuality`/`ContractClaim` enforcement gate — the contract gate proves the schema contract and records its breach without raising, the profile grades the live data against warning/error/critical thresholds, fires the bound severity actions, and emits the publication frame; the two are distinct planes over the same agnostic frame, never one owner.
- Cases: `StepKind` is the one closed step-payload union and `ProbeStep` the thin `Struct` wrapper pairing one `StepKind` with its optional per-step `thresholds`/`actions` override — the policy rides the wrapper, never a default field on the union, because the `expression` `@tagged_union.__init__` treats every keyword as a case candidate and raises `TypeError("One and only one case can be specified")` the moment a construction passes a case plus a policy field, so a per-step override is unconstructible on the union and must live on the `ProbeStep` `Struct` the case is wrapped in; `ProbeStep.append` matches `self.kind` so every case appends exactly one step to the `pb.Validate` plan, so the axis never carries a non-step. Every `columns` slot resolves through one `ProbeTables.cols` fold over the already-bound `pb` namespace, admitting a name, a `Sequence[str]`, or a `(Selector, args)` pair that mints the matching `pb.starts_with`/`ends_with`/`contains`/`matches`/`everything`/`first_n`/`last_n` selector, so a column set is a row resolved off the table the plan already holds, never a hand-built name loop and never a per-step `import pointblank`. `compare(columns, op, value, na_pass)` folds `gt`/`ge`/`lt`/`le`/`eq`/`ne` through the `ProbeTables.compare` map (the unbound `Validate.col_vals_gt`/`ge`/`lt`/`le`/`eq`/`ne` resolved off the class by name) so the six comparison arms collapse to one bound-method lookup, `value` accepting a literal, a `pb.col(...)` column reference, or a `pb.ref(...)` cross-column reference — the one comparison surface discriminated by operator and value shape, never six step types · `span(columns, left, right, inclusive, outside, na_pass)` reads the bound builder off `ProbeTables.span` keyed by the `outside` polarity (`Validate.col_vals_between`/`col_vals_outside`) and threads the `(bool, bool)` inclusive endpoint pair the `_INCLUSIVE` fold mints straight into `inclusive=`, so the in-range and out-of-range checks are one row, never two cases and never four inline endpoint tests · `member(columns, present, values)` folds `isin`/`notin` polarity through `ProbeTables.member` (`Validate.col_vals_in_set`/`col_vals_not_in_set`) into the `set=` collection · `nullity(columns, present)` folds the null/non-null polarity through `ProbeTables.nullity` (`Validate.col_vals_not_null`/`col_vals_null`) · `pattern(columns, regex, inverse, na_pass)` threads the `str` pattern and `inverse` polarity into `Validate.col_vals_regex(pattern=, inverse=)`, the one regex surface, never a second `not_regex` case · `spec(columns, named, na_pass)` threads the named-spec literal into `Validate.col_vals_within_spec(spec=)` for the email/url/postal conformance checks pointblank owns natively · `ordered(columns, increasing, allow_stationary, tol, na_pass)` dispatches the monotonic direction as two inline arms because the tolerance kwarg is the mirror of the direction — `Validate.col_vals_increasing` owns `decreasing_tol=` (the permitted backward slack) and `Validate.col_vals_decreasing` owns `increasing_tol=` (the permitted forward slack), so each arm threads the single `tol` slot into its own asymmetric kwarg and neither forwards through a wrapper · `aggregate(columns, stat, op, value, tol)` folds the `(stat, op)` pair — `avg`/`sum`/`sd` × the five `gt`/`ge`/`lt`/`le`/`eq` aggregate comparators pointblank exposes (no `col_{stat}_ne`, so `AggOp` is the comparison vocabulary minus `ne`, distinct from the full six-member `Operator` the per-cell `compare` axis carries) — through `ProbeTables.aggregate`, one comprehension over `_STATS × _AGG_OPS` keyed by the `(stat, op)` tuple resolving each `col_{stat}_{op}` method off the class, never a fifteen-method family · `shape(kind, count, tol, inverse)` dispatches the row/column shape assertion as two inline arms because only `Validate.row_count_match` owns `tol=` while `Validate.col_count_match` rejects it · `nullfrac(columns, p, tol)` threads the null-fraction bound into `Validate.col_pct_null(p=, tol=)` · `distinct(columns_subset, complete)` folds the row-uniqueness/no-null-row polarity through `ProbeTables.distinct` (`Validate.rows_distinct`/`rows_complete`) threading the optional `columns_subset=` · `present(columns)` threads `Validate.col_exists` · `schema(declared, complete, in_order)` threads the `tabular/interop#INTEROP` `FieldShape` tuple (the minter-declared structural value the contract gate also reads) projected to a `pb.Schema` into `Validate.col_schema_match(schema=, complete=, in_order=)` so the structural declaration the contract owner already mints drives the schema-match step, never a re-declared column list · `expr(expression)` threads `Validate.col_vals_expr(expr=)` for the row-wise boolean expression · `joint(expressions)` threads `Validate.conjointly(*expressions)` so a row passes only when every expression passes · `twin(other)` threads `Validate.tbl_match(tbl_compare=)` for whole-table equality against a comparison table · `bespoke(callable)` threads `Validate.specially(expr=)` for the arbitrary table-level predicate pointblank's escape hatch owns — the AI-driven `prompt` step stays outside this axis (the LLM-graded per-row assertion is a runtime/host concern, never a data-plane probe). Each `StepKind` case is matched by `match`/`case` closed by `assert_never` into the bound `Validate` builder, so the pointblank step vocabulary is one closed switch over the `ProbeTables` maps, never a per-step builder method.
- Grade: `Grade` is the closed severity axis — `WARNING`/`ERROR`/`CRITICAL` the three pointblank threshold levels plus `PASSED` the no-breach floor, ordered by severity rank so the profile's overall grade is the maximum breached level across the interrogated plan. `Grade.LEVELS` is the ascending `(WARNING, ERROR, CRITICAL)` tuple every breach sweep and breach-set fold reads, so the descending grade sweep and the ascending breach-set projection are one ordered vocabulary, never two hand-spelled level lists. `Thresholds` rides the `QualityProfile.thresholds` field as one `pb.Thresholds(warning=, error=, critical=)` value threaded into `Validate(thresholds=)` at plan open, the per-step `ProbeStep.thresholds` override an optional `pb.Thresholds` on the row threading into each builder's `thresholds=` so a single step tightens its grade without a parallel plan. The threshold limit is a count or a fraction — an `int` failing-unit count or a `float ∈ [0, 1]` failing-unit fraction — the one `Thresholds` shape pointblank grades natively, never two threshold types. `Actions` rides the parallel `QualityProfile.actions` field as one `pb.Actions(warning=, error=, critical=, default=, highest_only=)` value threaded into `Validate(actions=)` at plan open and the per-step `ProbeStep.actions` override into each builder's `actions=` — the severity-callback axis pointblank fires when a step breaches its threshold, `highest_only=True` collapsing a multi-level breach to its top severity, plus the optional `pb.FinalActions(*args)` threaded into `Validate(final_actions=)` for the one post-interrogation summary callback — so the grade-and-react surface is one threshold-plus-action policy on the profile, never a parallel observer wired beside the plan. `Grade.breaches` projects the breached levels by sweeping `Grade.LEVELS` through `plan.above_threshold(level=, i=)` — `i=None` the plan-wide breach, an `int` the per-step breach — and `Grade.of` reads `plan.all_passed()` as the affirmative `PASSED` floor then returns the maximum breached level from that same projection or `PASSED`, so the overall grade and the per-severity breach set are one fold over the plan reading one `above_threshold` sweep, never a per-level boolean tail and never two transcriptions of the level order.
- Entry: `QualityProfile.of` folds a tuple of `ProbeStep` plus the `pb.Thresholds` grade policy, the optional `pb.Actions`/`pb.FinalActions` severity-callback policy, and the optional `label`/`tbl_name`/`brief` plan metadata into one profile; `QualityProfile.interrogate` opens `pb.Validate(data, thresholds=, actions=, final_actions=, label=, tbl_name=, brief=)` over the agnostic frame or the DuckDB/parquet `data` path, folds every `ProbeStep` onto the plan through `_plan` (one `reduce` over `ProbeStep.append`, never a mutable for-loop accumulator), runs `plan.interrogate(sample_n=, sample_frac=, get_first_n=, extract_limit=)` once inside one `boundary(...)` fence, then `.bind`s the railed `fingerprint(sampling)` content-key derivation — the same sampling bound folded into the key so a re-sampled interrogation flips it — and `.map`s the resolved `ContentKey` into `ProfileReceipt.of`, returning a `RuntimeRail[ProfileReceipt]` — the plan-build-and-interrogate and the key derivation are two railed hops threaded once, never a bare `ContentKey` assignment collapsing the railed `ContentIdentity.of`, and the rail is `Ok` even when steps breach because the profile records and grades but never enforces, exactly as the sibling `ContractClaim` records without raising. The single `interrogate()` call is the one execution surface; sampling (`sample_n`/`sample_frac`/`get_first_n`) and the `extract_limit` failing-row cap are call rows on that one surface, never a separate runner. `QualityProfile.report` is the one report entrypoint folding the whole nine-case `ProfileReport` axis to a `ProfileFrame` through one total `match` closed by `assert_never` — the plan-consuming `tabular`/`step`/`json`/`dataframe`/`sundered` cases read the interrogated `Validate` plan through one `cache`-memoized `graded()` closure so the plan is built and interrogated at most once per call and only when a plan-consuming arm runs, the plan-free `probe`/`summary`/`missing`/`preview` cases read `pb.DataScan`/`pb.col_summary_tbl`/`pb.missing_vals_tbl`/`pb.preview` over the raw `data` table and never touch `graded()` — so the interrogated-plan-versus-raw-table boundary is recovered structurally from the matched arm inside one total dispatch, never a second partial `match` that would leave `assert_never` unsound and never a `render`/`scan` sibling-method split sharing a `boundary` prefix. The `GT`-carrying cases ride the `[SHAPE]` value the artifacts renderer renders and the `json`/`dataframe`/`sundered` cases carry the `str`/native-frame wire value, every case leaving through the one `ProfileFrame` rail, so a new report kind is one `ProfileReport` case, never a parallel render or scan method.
- Auto: a passing interrogation yields `ProfileReceipt.of` with `grade=PASSED` and `all_passed()` true; a breaching interrogation grades through `Grade.of` over the severity sweep and folds the per-step test-unit evidence into the receipt — each step's `n_passed`/`n_failed` counts and `f_passed`/`f_failed` fractions read off `plan.n_passed(scalar=False)`/`plan.n_failed(scalar=False)`/`plan.f_passed(scalar=False)`/`plan.f_failed(scalar=False)` as one `dict[int, ...]` keyed by step index, the per-level breach set reads off the one `Grade.breaches` projection over `plan.above_threshold(level=)`, and the real frame shape reads off `pb.get_row_count(data)`/`pb.get_column_count(data)` so the receipt `shape` carries the graded frame's `(rows, columns)` rather than a degenerate step-count tuple — so the grade receipt carries frame shape, step count, per-step pass/fail counts and both fractions, the per-severity breach set, and the overall grade as one typed evidence stream, never re-derived from the raw frame and never a parallel record per level. The interrogated plan stays lazy where the backend admits it — the polars/DuckDB/ibis `data` path pushes the validation into the scan because pointblank's Narwhals engine never materializes the frame it grades.
- Receipt: `ProfileReceipt.contribute` yields one emitted-phase row through the runtime two-argument `Receipt.of(owner, evidence)` factory — `Receipt.of("quality-profile", ("emitted", label, facts))` routing the `(phase, subject, facts)` evidence triple the receipts owner's `of` match decomposes, never the four-positional `Receipt.of(phase, owner, subject, facts)` shape the owner does not expose — satisfying the `ReceiptContributor.contribute -> Iterable[Receipt]` streaming Protocol the sibling `egress#EGRESS`/`interop#INTEROP` receipts yield through, never a bare single `Receipt` against the iterable Protocol, the `rows`/`columns`/`steps` counts riding as native `int` scalars the receipts `Encoder(enc_hook=repr)` serializes without a `str()` coerce. The receipt is keyed by `ContentIdentity` over the plan-content fingerprint — `QualityProfile.fingerprint` folds one deterministic msgspec-JSON row per `ProbeStep` (tag, payload, override presence — a callable projected to its stable code identity `(module, qualname, marshalled bytecode)` so two distinct `<lambda>` predicates never share a key, policy values to their deterministic rendering) plus the plan-level row over the label, the `interrogate` sampling bound (`sample_n`/`sample_frac`/`get_first_n`/`extract_limit`), and the threshold/action policy into one canonical byte stream through `ContentIdentity.of("profile", ...)`, returning the railed `RuntimeRail[ContentKey]` the `interrogate` rail threads through `.bind`/`.map` into the receipt rather than collapsing the rail into a field, so an unchanged probe set, threshold policy, and sampling bound reuses its key byte-stable while a single changed threshold, a tightened per-step override, an added probe, or a widened sampling bound flips it — the graded-evidence identity the bare `(label, step-count, grade)` string could not carry, since a changed threshold or a re-sampled interrogation leaves label, count, and a still-passing grade untouched while the receipt's counts and grade shift. The receipt is the graded data-quality observability evidence, never replacing the typed `tabular/columnar#SCAN` `QueryReceipt` and never minting the identity the runtime owns. It is the data-plane sibling of the runtime receipt-sink: it records the grade above the gate, the gate's `ContractClaim` records the contract at the gate, and neither raises.
- Shape: `QualityProfile.report` emits one `ProfileFrame` carrying the `great_tables.GT` object on its `frame` slot as an opaque `Any` plus the `kind` discriminant and the `grade` — data never imports `great_tables`, never re-renders the `GT` to HTML, and never reaches into the `GT` internals; the `python:artifacts/figures` great-tables tier owns the render and reads the `ProfileFrame` `[SHAPE]` value, exactly as the `tabular/columnar#SCAN` corpus-row wire hands a flat record to the documents tier. The `json`/`dataframe`/`sundered` report cases carry the `str`/native-frame wire value on the same `frame` slot, so the publication report, the machine-readable JSON, the in-memory grade frame, and the passing/failing row split all leave through one `ProfileFrame` rail, never four emitters.
- Packages: `pointblank` (`Validate(data, thresholds=, actions=, final_actions=, label=, tbl_name=, brief=)`/`Validate.col_vals_gt`/`ge`/`lt`/`le`/`eq`/`ne`/`col_vals_between(left=, right=, inclusive=)`/`col_vals_outside(left=, right=, inclusive=)`/`col_vals_in_set(set=)`/`col_vals_not_in_set(set=)`/`col_vals_not_null`/`col_vals_null`/`col_vals_regex(pattern=, inverse=)`/`col_vals_within_spec(spec=)`/`col_vals_increasing(allow_stationary=, decreasing_tol=)`/`col_vals_decreasing(allow_stationary=, increasing_tol=)`/`col_{avg,sum,sd}_{gt,ge,lt,le,eq}`/`col_vals_expr(expr=)`/`col_exists`/`col_schema_match(schema=, complete=, in_order=)`/`col_count_match(count=, inverse=)`/`row_count_match(count=, tol=, inverse=)`/`col_pct_null(p=, tol=)`/`rows_distinct(columns_subset=)`/`rows_complete(columns_subset=)`/`conjointly`/`tbl_match(tbl_compare=)`/`specially(expr=)`/`interrogate(sample_n=, sample_frac=, get_first_n=, extract_limit=)`/`all_passed()`/`above_threshold(level=, i=)`/`n_passed(i=, scalar=)`/`n_failed(i=, scalar=)`/`f_passed(i=, scalar=)`/`f_failed(i=, scalar=)`/`get_tabular_report(title=, incl_header=, incl_footer=) -> GT`/`get_step_report(i=, columns_subset=, limit=) -> GT`/`get_json_report(use_fields=, exclude_fields=) -> str`/`get_dataframe_report(tbl_type=)`/`get_sundered_data(type=)`/`Thresholds(warning=, error=, critical=)`/`Actions(warning=, error=, critical=, default=, highest_only=)`/`FinalActions(*args)`/`Schema(columns=)`/`col`/`ref`/`starts_with`/`ends_with`/`contains`/`matches`/`everything`/`first_n`/`last_n`/`get_row_count(data) -> int`/`get_column_count(data) -> int`/`DataScan(data, tbl_name=).get_tabular_report(show_sample_data=) -> GT`/`col_summary_tbl(data, tbl_name=) -> GT`/`missing_vals_tbl(data) -> GT`/`preview(data, columns_subset=, n_head=, n_tail=, limit=) -> GT`, import at boundary scope under `# noqa: PLC0415` since the manifest-banned `pointblank` transitively loads the heavy `polars`/`great_tables` engines), `tabular/interop#INTEROP` (`FieldShape` projected to a `pb.Schema` for the `schema` probe, imported downward from its minter), `tabular/interop#INTEROP` (the agnostic `nw.DataFrame`/`nw.LazyFrame` passes through unmodified into pointblank's own Narwhals `data` admission as the `Any` `data` payload, never lowered through `FrameInterop.translate` and never re-tagged through a consumed `FrameInterop` member, because pointblank consumes the agnostic frame directly through its own Narwhals engine), `beartype` (`@beartype(conf=FAULT_CONF)` the public domain-admission contract on the `of` factory and the caller-facing `interrogate`/`report` submissions so a non-`ProbeStep` or a malformed `ProfileReport`/`data` argument that violates the in-process annotation raises the canonical `BeartypeCallHintViolation` root the `reliability/faults#FAULT` `CLASSIFY` `api` row folds onto the rail at the enclosing `boundary` fence, the shared `FAULT_CONF` the sibling data admission seams bind; the `ProfileReceipt.of`/`Grade.of`/`Grade.breaches`/`ProbeStep.append`/`ProbeTables` folds and the `_plan`/`_report` kernels over the owner's own already-admitted values carry no decorator), runtime (`RuntimeRail`/`boundary`/`FAULT_CONF` the shared beartype violation-redirect config/`ContentIdentity`/`ContentKey`/`ReceiptContributor`/`Receipt`).
- Growth: a new comparison/range/membership/null-fraction/uniqueness check is one `ProbeStep` row threading its `ProbeTables.compare`/`span`/`member`/`distinct` polarity map; a new column-aggregate stat is one row in the `_STATS` axis the `ProbeTables.aggregate` comprehension folds; a new column selector is one `Selector` literal the `ProbeTables.cols` fold resolves off the bound `pb` namespace; a new report kind is one `ProfileReport` case on the one `report` axis, a plan-consuming case reading the `graded()` closure and a plan-free case reading the raw `data` table; a new severity level is one `Grade.LEVELS` row plus one `Thresholds`/`Actions` field; a per-step threshold or action override is the existing `ProbeStep.thresholds`/`ProbeStep.actions` field threaded into the builder's `thresholds=`/`actions=`; a post-interrogation summary callback is the existing `QualityProfile.final_actions` field threaded into `Validate(final_actions=)`; a sampling or extract-limit knob is the existing `interrogate` call row; the AI-driven `prompt` step is admitted as a `ProbeStep` row only when an LLM model handle arrives through the runtime host seam, never a module-top dependency on this data-plane page; a second backend `data` path is admitted free by pointblank's own Narwhals engine with zero profile-cluster change.
- Boundary: pointblank owns the validation plan, the warning/error/critical threshold grading, the severity-action callbacks, and the `great_tables.GT` report emission; `great_tables` owns the renderable frame downstream and stays `python:artifacts/figures`-owned; Narwhals owns the frame normalization inside pointblank; runtime owns the identity, the receipt rail, and the LLM/host seam. No raising in domain logic (the profile records and grades, never enforces — `assert_below_threshold` is pointblank's raising gate and stays unbound on this page), no second frame translator beside pointblank's own Narwhals admission, no `great_tables` import on this page, no HTML re-render where the `GT` frame needs none, no re-interrogation of an already-interrogated plan, no `ContentIdentity` mint the runtime owns duplicated, no parallel receipt sink beside the runtime `ReceiptContributor`; a `check_gt`/`check_between`/`check_unique` per-step method family, a parallel step type per comparison operator, a `lambda` table forwarding a renamed `pb.Validate` step method where the unbound method binds directly, a fifteen-method column-aggregate family where the `ProbeTables.aggregate` `(stat, op)` comprehension folds the `_STATS × _AGG_OPS` product, a hand-built column-name loop where the `ProbeTables.cols` selector fold resolves it off the bound `pb` namespace, a `render`/`scan` sibling-method split where one `report` axis discriminates the interrogated-plan-versus-raw-table boundary from the case tag, a parallel severity-callback observer where the `actions=` axis threads it, a per-severity boolean grade tail where one `Grade.breaches` sweep folds them, a label-string content key where the probe-threshold-and-sampling fingerprint flips on a changed threshold or a re-sampled interrogation, a bare-qualname bespoke-callable key where the marshalled code identity separates two distinct `<lambda>` predicates, a degenerate step-count receipt shape where `get_row_count`/`get_column_count` carry the real frame shape, a `thresholds`/`actions` field on the `StepKind` `@tagged_union` where the `@tagged_union.__init__` rejects a case-plus-field construction with `TypeError("One and only one case can be specified")` and the `ProbeStep` `Struct` wrapper holds the per-step policy, a four-positional `Receipt.of(phase, owner, subject, facts)` against the two-argument `Receipt.of(owner, evidence)` evidence-tuple form, a `contribute` returning a single `Receipt` against the `Iterable[Receipt]` Protocol, a bare `ContentKey` `fingerprint` assignment collapsing the railed `ContentIdentity.of` where the rail threads through `.bind`/`.map`, a second claim type beside `ProfileReceipt`, an exception-driven grade gate, and an undecorated `of`/`interrogate`/`report` admitting a caller `ProbeStep`/`ProfileReport`/`data` argument without the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling data admission entrypoints share are the deleted forms.

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
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
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
    def interrogate(
        self, data: Any, *, sample_n: int | None = None, sample_frac: float | None = None, get_first_n: int | None = None, extract_limit: int = 500
    ) -> "RuntimeRail[ProfileReceipt]":
        # the plan build+interrogate fences once, then `.bind`s the railed content-key derivation
        # and `.map`s the resolved `ContentKey` into the receipt — never collapsing the railed
        # `ContentIdentity.of` into a field, exactly as the sibling `columnar`/`interop` receipts.
        # the interrogation sampling bound folds into the fingerprint so a re-sampled run — same
        # probes, different grade and counts — never reuses a byte-stable key.
        sampling = (sample_n, sample_frac, get_first_n, extract_limit)
        return boundary(
            f"profile.interrogate.{self.label}",
            lambda: self._plan(data).interrogate(sample_n=sample_n, sample_frac=sample_frac, get_first_n=get_first_n, extract_limit=extract_limit),
        ).bind(lambda plan: self.fingerprint(sampling).map(lambda key: ProfileReceipt.of(self.label, plan, data, len(self.steps), key)))

    @beartype(conf=FAULT_CONF)
    def report(self, data: Any, report: ProfileReport) -> "RuntimeRail[ProfileFrame]":
        return boundary(f"profile.report.{report.tag}", lambda: self._report(data, report))

    def fingerprint(self, sampling: tuple[int | None, float | None, int | None, int]) -> "RuntimeRail[ContentKey]":
        # the canonical plan wire: the plan-level row folds label, the interrogation sampling bound,
        # and the threshold/action policy; one deterministic msgspec-JSON row per probe folds (tag,
        # payload, per-step override presence). the leaf projection renders a callable to its stable
        # code identity (module, qualname, marshalled bytecode) so two distinct `<lambda>` predicates
        # never collide on a bare qualname, a provider policy to its deterministic rendering; the
        # `repr(step)` whole-object stream is the folder key-law deleted form (a bare callable repr
        # carries a memory address, non-deterministic by run).
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

`ProbeTables` binds each closed polarity directly onto the unbound `pb.Validate` step method — `pb.Validate.col_vals_gt` invoked as `compare["gt"](plan, columns, value=..., thresholds=t, actions=a)` threads `plan` as `self` and the per-step `thresholds=`/`actions=` policy as the two kwargs every step row shares, exactly as the sibling `tabular/contract#QUALITY` `_CMP` table binds `Check.ge`, so no `lambda` forwards a rename. The bind is boundary-scoped because the manifest bans the module-level `pointblank` import, and the `aggregate` cross-product is one comprehension over the `_STATS × _AGG_OPS` axes resolving each `col_{stat}_{op}` method off the class by name, never fifteen hand-written rows. `ProbeTables.bind` also captures the `pb` module handle on the `ns` field once per plan, so `ProbeTables.cols` (the `(Selector, args)` selector-or-passthrough fold every `columns` slot reads) and `ProbeTables.schema` (the `FieldShape`-tuple-to-`pb.Schema` projection the `schema` probe reads) resolve off that one captured namespace — the selector and schema resolution own no second `import pointblank`, collapsing the thirteen per-step selector imports the standalone fold otherwise paid into the single `bind` capture. The order and shape arms stay inline two-case dispatch because each side names a different kwarg: the order arms carry mirror tolerance names (`col_vals_increasing` owns `decreasing_tol=`, `col_vals_decreasing` owns `increasing_tol=`), and the shape arms carry an asymmetric tolerance (`row_count_match` owns `tol=` while `col_count_match` drops it), so no single table row can bind both members of a pair without a forwarding wrapper that renames the kwarg.

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
    frame -->|SHAPE| artifacts["python:artifacts/figures great-tables tier"]
```
