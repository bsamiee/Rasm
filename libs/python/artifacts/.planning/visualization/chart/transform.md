# [PY_ARTIFACTS_CHART_TRANSFORM]

The `vegafusion` data pre-pass owner of the 2D chart axis. `VegaTransform` is the closed pre-pass policy whose `apply` fold returns ONE self-contained Vega-Lite spec over the gated `vegafusion` runtime PAIRED with the typed `PrePassEvidence` the pre-pass produced — the spec's DataFusion-backed data transforms server-evaluated and the reduced result inlined, never the raw frame, and the collected `PreTransformWarning` list carried as evidence rather than discarded. It runs before the `visualization/chart/export#EXPORT` `vl-convert` render on the `anyio.to_process` subprocess seam (the gated DataFusion core the export owner offloads): the `Inline` arm folds `runtime.pre_transform_spec` to reduce-and-inline one self-contained spec, the `State` arm folds `runtime.new_chart_state` then `ChartState.get_transformed_spec` for the host-free interactive HTML row and reads the `get_comm_plan` signal/dataset dependency graph as evidence, and the `Passthrough` arm skips a transform-free spec. `vl-convert` exposes no external-dataset feed, so the reduced data crosses INSIDE the spec — the server-side aggregation IS the size reduction, not a separate Arrow-IPC channel the renderer cannot accept; the `inline_datasets` INPUT seam (a `data/tabular` frame entering the spec by name for server reduction) is distinct from that no-external-OUTPUT constraint. The pre-pass decision is computed once in `VegaTransform.of` from the spec's transform presence and interactivity, and `build_pre_transform_spec_plan` is the cost-gate projection that predicts the client/server split WITHOUT executing. A malformed spec folds onto the `Result` rail; the singleton runtime's worker-pool caps tune reset-on-set, never a fresh runtime per transform.

## [01]-[INDEX]

- [01]-[TRANSFORM]: the closed `VegaTransform` pre-pass policy over the gated `vegafusion` runtime — `Passthrough`/`Inline`/`State` discriminating the data pre-pass by transform presence and interactivity, each executing arm threading the shared `TransformPolicy` (`row_limit`/`local_tz`/`default_input_tz`/`inline_datasets` + the reset-on-set runtime caps) and the `Inline` arm additionally the `Retention` interactivity policy (`preserve_interactivity`/`keep_signals`/`keep_datasets`), every arm returning one `Result[PrePass, TransformFault]` carrying the self-contained spec the `visualization/chart/export#EXPORT` `_vl_render` renders AND the `PrePassEvidence` (the collected `PreTransformWarning` list, the `RowLimitExceeded`/`BrokenInteractivity`/`Unsupported` data-loss flags each derived explicit, the timezone, the transformed-dataset count, the `State` comm-plan cardinality) the export owner folds onto its `structlog`/`opentelemetry` evidence — plus the `planned` cost-gate projection over `build_pre_transform_spec_plan`.

## [02]-[TRANSFORM]

- Cases: `VegaTransform` cases — `Passthrough()` (the spec carries no data transform, the pre-pass is skipped and the spec passes through unmodified with empty evidence) · `Inline(policy, retention)` (the spec carries a data transform, folding `runtime.pre_transform_spec` to one self-contained spec with the transforms executed and the reduced data inlined, threading the `TransformPolicy` and the `Retention` interactivity policy, capturing the returned `PreTransformWarning` tail as evidence) · `State(policy)` (the interactive HTML row, folding `runtime.new_chart_state` then `ChartState.get_transformed_spec` to one fully-transformed self-contained spec, reading `ChartState.get_warnings` and `ChartState.get_comm_plan` as the interactivity evidence the host-free renderer needs with no live server) — matched by one total `match`/`case` over `tag`. There is no separate Arrow-IPC extract case: `vl-convert` accepts no external dataset, so a `pre_transform_extract` frame map would have no renderer consumer; the server-side aggregation `pre_transform_spec` runs IS the data reduction, and the reduced result inlines into the spec rather than crossing as a side channel (the `pre_transform_extract`/`get_column_usage`/`transformer` Arrow-IPC surface stays the `data/tabular/columnar#COLUMNAR` egress owner's).
- Auto: `VegaTransform.of` discriminates the pre-pass once — `_has_transform` RECURSES the composition operators (`layer`/`concat`/`hconcat`/`vconcat`/`spec`), so a transform nested under a concat/facet/repeat sub-view is detected rather than a shallow top-level+`layer` scan misreading it as `Passthrough` (which would skip the whole reduce-and-inline pre-pass and ship the raw data client-side); an interactive transform-bearing spec returns `State`, a transform-bearing spec returns `Inline`, and a genuinely plain spec returns `Passthrough`; the caller hands in the `TransformPolicy` and (for the `Inline` path) the `Retention`. `apply` folds the matched arm and NEVER discards the warnings tail: `Inline` binds the `(spec, warnings)` pair and returns `Ok(PrePass(spec, evidence))`, `State` reads `get_transformed_spec`/`get_warnings`/`get_comm_plan` into the evidence off ONE `get_transformed_spec` fact, `Passthrough` returns the spec unchanged with empty evidence, and a malformed spec crosses the substrate `catch(exception=ValueError)` trap at the arm and re-spells to `<malformed-spec>` in one expression rather than a hand-rolled try/except. `planned` is the pre-execution cost gate — `build_pre_transform_spec_plan` predicts the `client_spec`/`server_spec`/`comm_plan` split so a caller reads `server_datasets == 0` to skip the whole pre-pass round-trip before paying for transform execution.
- Growth: a new pre-pass mode is one `VegaTransform` case plus one `apply` arm plus one `VegaTransform.of` branch; a new decision rule (a per-layer transform footprint, a plan-cardinality gate) is one `of`/`planned` branch over the same transform-presence projection; a new runtime knob is one `TransformPolicy` field threaded into `_tuned`; a new interactivity retention rule is one `Retention` field; a new evidence fact is one `PrePassEvidence` field the arm fills; zero new surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Literal, assert_never

from builtins import frozendict
from expression import Ok, Result, case, tag, tagged_union
from expression.extra.result import catch
from msgspec import Struct

lazy import vegafusion
lazy import vegafusion.utils

# --- [TYPES] ----------------------------------------------------------------------------
type Spec = dict[str, object]
type WarningKind = Literal["RowLimitExceeded", "BrokenInteractivity", "Unsupported"]
type TransformFault = Literal["<malformed-spec>"]
type KeepVar = str | tuple[str, tuple[int, ...]]     # a variable `name`, or `(name, scope)` retained client-side


# --- [MODELS] ---------------------------------------------------------------------------
class Retention(Struct, frozen=True):
    preserve_interactivity: bool = False              # Inline default: maximal static reduction; True keeps client-side selections
    keep_signals: tuple[KeepVar, ...] = ()
    keep_datasets: tuple[KeepVar, ...] = ()


class TransformPolicy(Struct, frozen=True):
    row_limit: int | None = None                      # cap the inlined rows so a large server aggregation cannot bloat the self-contained spec
    local_tz: str | None = None                       # default runtime.get_local_tz(); pin to the vl-convert render tz so time transforms match
    default_input_tz: str | None = None
    inline_datasets: frozendict[str, object] = frozendict()  # named data/tabular frames the spec references by url, server-reduced (INPUT, not the no-external-OUTPUT constraint)
    worker_threads: int | None = None                 # singleton runtime caps; the property setter resets the pool only on a changed value
    cache_capacity: int | None = None
    memory_limit: int | None = None


class PrePassEvidence(Struct, frozen=True):
    mode: Literal["passthrough", "inline", "state"]
    row_limit: int | None
    local_tz: str
    transformed_datasets: int                         # count of server-reduced inlined datasets in the returned spec
    client_vars: int                                  # State: variables the comm plan pushes server->client (retained interactivity)
    server_vars: int                                  # State: variables the comm plan pushes client->server
    warnings: tuple[tuple[WarningKind, str], ...]     # the collected PreTransformWarning list — NEVER discarded
    row_limit_exceeded: bool                          # derived: a truncated aggregation is a silent data-loss signal made explicit
    interactivity_broken: bool                        # derived: a lost client-side selection
    unsupported: bool                                 # derived: a transform the engine could not evaluate (silently dropped) — the third data-loss signal made explicit


class PrePass(Struct, frozen=True):
    spec: Spec                                         # the self-contained spec the _vl_render arm renders directly
    evidence: PrePassEvidence


class PrePassPlan(Struct, frozen=True):
    server_datasets: int                              # 0 => the pre-pass has no server work; the caller skips the round-trip
    client_datasets: int
    warnings: tuple[tuple[WarningKind, str], ...]


_PASSTHROUGH = PrePassEvidence(
    mode="passthrough", row_limit=None, local_tz="", transformed_datasets=0, client_vars=0, server_vars=0,
    warnings=(), row_limit_exceeded=False, interactivity_broken=False, unsupported=False,
)


@tagged_union(frozen=True)
class VegaTransform:
    tag: Literal["passthrough", "inline", "state"] = tag()
    passthrough: None = case()
    inline: tuple[TransformPolicy, Retention] = case()
    state: TransformPolicy = case()

    @staticmethod
    def Passthrough() -> "VegaTransform":
        return VegaTransform(passthrough=None)

    @staticmethod
    def Inline(policy: TransformPolicy, retention: Retention) -> "VegaTransform":
        return VegaTransform(inline=(policy, retention))

    @staticmethod
    def State(policy: TransformPolicy) -> "VegaTransform":
        return VegaTransform(state=policy)

    @staticmethod
    def of(spec: Spec, interactive: bool, policy: TransformPolicy = TransformPolicy(), retention: Retention = Retention()) -> "VegaTransform":
        has_transform = _has_transform(spec)
        if interactive and has_transform:
            return VegaTransform.State(policy)
        return VegaTransform.Inline(policy, retention) if has_transform else VegaTransform.Passthrough()

    def apply(self, spec: Spec) -> Result[PrePass, TransformFault]:
        match self:
            case VegaTransform(tag="passthrough"):
                return Ok(PrePass(spec, _PASSTHROUGH))
            case VegaTransform(tag="inline", inline=(policy, retention)):
                _tuned(policy)
                return _run_inline(spec, policy, retention)
            case VegaTransform(tag="state", state=policy):
                _tuned(policy)
                return _run_state(spec, policy)
            case _ as unreachable:
                assert_never(unreachable)

    def planned(self, spec: Spec) -> PrePassPlan:
        match self:
            case VegaTransform(tag="passthrough"):
                return PrePassPlan(0, 0, ())
            case VegaTransform(tag="inline", inline=(_policy, retention)):
                plan = vegafusion.utils.build_pre_transform_spec_plan(
                    spec, preserve_interactivity=retention.preserve_interactivity,
                    keep_signals=list(retention.keep_signals) or None, keep_datasets=list(retention.keep_datasets) or None,
                )
                return _plan(plan)
            case VegaTransform(tag="state"):
                return _plan(vegafusion.utils.build_pre_transform_spec_plan(spec))
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------
_COMPOSITIONS: tuple[str, ...] = ("layer", "concat", "hconcat", "vconcat", "spec")


def _has_transform(node: Spec) -> bool:
    # a Vega-Lite transform nests under any composition operator; the scan recurses through `spec` (facet/repeat)
    # and the layer/concat operators, so a composite whose only transforms live in a sub-view is not misread as
    # Passthrough — which would skip the whole reduce-and-inline pre-pass and ship the raw data client-side.
    if isinstance(top := node.get("transform"), list) and top:
        return True
    return any(
        _has_transform(view)
        for key in _COMPOSITIONS
        for view in (raw if isinstance(raw := node.get(key), list) else (raw,))
        if isinstance(view, dict)
    )


def _tuned(policy: TransformPolicy) -> None:  # Exemption: the vegafusion runtime singleton exposes its caps as properties whose setter resets the pool only on a changed value — an idempotent per-worker tune, never a fresh runtime per transform
    if policy.worker_threads is not None:
        vegafusion.runtime.worker_threads = policy.worker_threads
    if policy.cache_capacity is not None:
        vegafusion.runtime.cache_capacity = policy.cache_capacity
    if policy.memory_limit is not None:
        vegafusion.runtime.memory_limit = policy.memory_limit


def _warnings(raw: object) -> tuple[tuple[WarningKind, str], ...]:
    return tuple((entry["type"], entry["message"]) for entry in raw) if isinstance(raw, list) else ()


def _dataset_count(spec: Spec) -> int:
    return len(datasets) if isinstance(datasets := spec.get("datasets"), dict) else 0


def _evidence(mode: Literal["inline", "state"], tz: str, policy: TransformPolicy, spec: Spec, warnings: tuple[tuple[WarningKind, str], ...], /, *, client: int = 0, server: int = 0) -> PrePassEvidence:
    return PrePassEvidence(
        mode=mode, row_limit=policy.row_limit, local_tz=tz, transformed_datasets=_dataset_count(spec),
        client_vars=client, server_vars=server, warnings=warnings,
        row_limit_exceeded=any(kind == "RowLimitExceeded" for kind, _ in warnings),
        interactivity_broken=any(kind == "BrokenInteractivity" for kind, _ in warnings),
        unsupported=any(kind == "Unsupported" for kind, _ in warnings),
    )


def _run_inline(spec: Spec, policy: TransformPolicy, retention: Retention) -> Result[PrePass, TransformFault]:
    # the single-exception `catch` trap the substrate ships mints the transient from the provider ValueError a
    # malformed spec raises and re-spells it in one expression — never a hand-rolled try/except for one exception;
    # the interior is total over the admitted (spec, warnings) pair past it.
    tz = policy.local_tz or vegafusion.get_local_tz() or "UTC"
    return (
        catch(exception=ValueError)(vegafusion.runtime.pre_transform_spec)(
            spec, row_limit=policy.row_limit, local_tz=tz, default_input_tz=policy.default_input_tz,
            preserve_interactivity=retention.preserve_interactivity, inline_datasets=dict(policy.inline_datasets) or None,
            keep_signals=list(retention.keep_signals) or None, keep_datasets=list(retention.keep_datasets) or None,
        )
        .map_error(lambda _raised: "<malformed-spec>")
        .map(lambda pair: PrePass(pair[0], _evidence("inline", tz, policy, pair[0], _warnings(pair[1]))))
    )


def _run_state(spec: Spec, policy: TransformPolicy) -> Result[PrePass, TransformFault]:
    tz = policy.local_tz or vegafusion.get_local_tz() or "UTC"

    def _read(chart_state: object) -> PrePass:
        transformed = chart_state.get_transformed_spec()          # the ONE computed fact — spec AND evidence read off it once, never re-derived
        comm = chart_state.get_comm_plan()                        # {client_to_server, server_to_client: list[Variable]}
        return PrePass(transformed, _evidence(
            "state", tz, policy, transformed, _warnings(chart_state.get_warnings()),
            client=len(comm.get("server_to_client", ())), server=len(comm.get("client_to_server", ())),
        ))

    return (
        catch(exception=ValueError)(vegafusion.runtime.new_chart_state)(
            spec, local_tz=tz, default_input_tz=policy.default_input_tz, row_limit=policy.row_limit,
            inline_datasets=dict(policy.inline_datasets) or None,
        )
        .map_error(lambda _raised: "<malformed-spec>")
        .map(_read)
    )


def _plan(plan: object) -> PrePassPlan:
    server = plan.get("server_spec", {}) if isinstance(plan, dict) else {}
    client = plan.get("client_spec", {}) if isinstance(plan, dict) else {}
    warns = plan.get("warnings", ()) if isinstance(plan, dict) else ()
    return PrePassPlan(
        server_datasets=len(data) if isinstance(data := server.get("data"), list) else 0,
        client_datasets=len(cdata) if isinstance(cdata := client.get("data"), list) else 0,
        warnings=_warnings(warns),
    )
```

`VegaTransform.of` decides the pre-pass once from the recursive transform-presence scan and the interactive flag; `apply` runs the one matched arm and returns `Result[PrePass, TransformFault]` — the `PrePass` carrying both the self-contained spec the export render consumes AND the `PrePassEvidence` the export owner folds onto its `structlog`/`opentelemetry` evidence, so a `RowLimitExceeded` truncation, a `BrokenInteractivity` loss, or an `Unsupported` dropped transform is NEVER silent (each derived onto its own explicit flag), and a malformed spec re-spells through the substrate `catch(ValueError)` trap rather than raising. The `Inline` arm folds `pre_transform_spec` so the DataFusion-backed transforms execute server-side and the reduced result inlines into the spec — `vl-convert` then renders with no client-side transform work and no oversized embedded data — threading the `row_limit` cap, the `local_tz`/`default_input_tz` pin (so the server-evaluated time transforms match the render tz), the `inline_datasets` frame-input map, and the `Retention` interactivity policy, and binding the returned `PreTransformWarning` list. The `State` arm recovers the host-free interactivity for the HTML row by folding `ChartState.get_transformed_spec` and reads `get_comm_plan`/`get_warnings` as the signal/dataset dependency evidence the interactive row needs. `_tuned` sets the singleton runtime's `worker_threads`/`cache_capacity`/`memory_limit` reset-on-set, never a fresh runtime per transform. `apply` and `planned` are subprocess-side kernels the `visualization/chart/export#EXPORT` owner offloads onto its `anyio.to_process` seam under one `CapacityLimiter`, guarded by the export owner's bounded `stamina` retry on a worker death — this owner never itself crosses the process seam.

## [03]-[RESEARCH]

- [VL_NO_EXTERNAL_FEED] [RESOLVED]: signature reflection of `vl_convert 1.9.0.post1` (`.api/vl-convert-python.md` rows [01]-[05]) confirms the `vegalite_to_*` family carries NO `inline_datasets` parameter, so the prior Arrow-IPC frame-map `Extract` arm had no renderer that could consume it — an `inline_datasets=` kwarg `TypeError`s. The owner therefore returns ONE self-contained spec: `pre_transform_spec` server-evaluates the transforms and inlines the reduced result (the aggregation IS the size reduction), `ChartState.get_transformed_spec` serves the interactive HTML row, and `Passthrough` skips a transform-free spec. The `TransformPolicy.inline_datasets` field is the INPUT seam (`.api/vegafusion.md` entry [01]) — a `data/tabular` frame entering the spec by name (`vegafusion+dataset://{name}`/`table://{name}`) for the runtime to server-reduce through its internal narwhals interchange, DISTINCT from the no-external-OUTPUT constraint the renderer imposes. The `pre_transform_extract`/`get_column_usage`/`transformer` Arrow-IPC serializers remain the `data/tabular/columnar#COLUMNAR` egress owner's surface for a genuine columnar data export, never a chart-render side channel.
- [PRE_TRANSFORM_SPEC] [RESOLVED]: `runtime.pre_transform_spec(spec, row_limit=, local_tz=, default_input_tz=, preserve_interactivity=, inline_datasets=, keep_signals=, keep_datasets=) -> (dict, list[PreTransformWarning])`, `runtime.new_chart_state(spec, local_tz=, default_input_tz=, row_limit=, inline_datasets=) -> ChartState`, and `ChartState.get_transformed_spec()`/`get_warnings()`/`get_comm_plan()` verify against `.api/vegafusion.md` runtime rows [01]/[04] and `ChartState` rows [01]/[04]/[05]. `apply` binds BOTH halves of the `pre_transform_spec` return — the spec AND the `PreTransformWarning` tail (`{type: Literal['RowLimitExceeded','BrokenInteractivity','Unsupported'], message: str}`, catalog type [05]) folded onto `PrePassEvidence` so a truncated aggregation or a broken interactivity is a carried data-loss fact, never the discarded tail the prior fold dropped. `vegafusion.runtime.pre_transform_spec`/`new_chart_state` resolve on the singleton `runtime` INSTANCE the package binds at `__init__` (distinct from the shadowed `vegafusion.runtime` submodule attribute the `.api` flags), while `build_pre_transform_spec_plan` resolves on the `vegafusion.utils` submodule (reached directly, not shadowed) and `get_local_tz` on the top-level module.
- [RESOURCE_POLICY] [RESOLVED]: `runtime.worker_threads`/`cache_capacity`/`memory_limit` are read/write properties on the lazily-initialized multi-threaded worker-pool singleton whose SETTERS `reset()` the pool only on a changed value (`.api/vegafusion.md` entry [07]), so `_tuned` assigns the `TransformPolicy` caps idempotently once per worker process rather than constructing a fresh runtime per transform. `build_pre_transform_spec_plan(spec, preserve_interactivity=, keep_signals=, keep_datasets=) -> PreTransformSpecPlan {client_spec, server_spec, comm_plan, warnings}` (entry [06]) is the diagnostic that returns the SAME planned split `pre_transform_spec` would produce WITHOUT executing — `planned` reads its `server_spec.data` cardinality as the cost gate a caller checks (`server_datasets == 0` skips the pre-pass) and its `warnings` as the pre-execution data-loss prediction. The GIL-releasing DataFusion `apply`/`planned` kernels cross the `visualization/chart/export#EXPORT` `anyio.to_process` subprocess seam under one `CapacityLimiter`, recovered by that owner's bounded `stamina` retry on a `BrokenWorkerProcess` death; the `PrePassEvidence`/`PrePass`/`Result` return pickles back across the seam for the export owner to fold onto its charts receipt under one `structlog` event inside an `opentelemetry` span.
</content>
