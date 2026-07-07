# [PY_ARTIFACTS_CHART_EXPORT]

The host-free render half of the 2D chart axis — format dispatch AND the vegafusion data pre-pass, one page. `ChartExport` folds the `visualization/chart/spec#CHART` chart case, the typed `RenderPolicy`, and the target `ExportFormat` to bytes and mints one `core/receipt#RECEIPT` `ArtifactReceipt.Chart`. The host-free posture is the decisive axis: vl-convert-python is the Vega/Vega-Lite engine (Rust-native, embedded V8/deno_runtime, inlined Vega sources, zero browser, SVG/PNG/PDF/HTML/JPEG) and stays the ONE chart-origin SVG rasterizer; lets-plot self-renders to bytes in-process (SVG/HTML self-contained, PNG/PDF through pillow) as the second engine on its named consumed rows — `PlotSpec.to_svg`/`to_png`/`to_pdf` self-render, `scale_color_manual`/`scale_fill_manual` threading `graphic/color/derive#DERIVE`'s `Palette`; matplotlib serves the publication case. The `VegaTransform` pre-pass lives IN this page: it server-evaluates the spec's data transforms on the process seam and inlines the reduced result into ONE self-contained spec the `_vl_render` arm renders — vl-convert exposes no external-dataset feed, so the pre-computed data crosses inside the spec, and the `ChartState` self-contained spec serves the interactive HTML row with no live server.

`RenderPolicy` carries the full converter axis as typed policy — raster `scale`/`ppi`/`quality`, the `VegaTheme`-typed `theme`, the pinned `vl_version`, `register_font_directory` closing the font-identity loop with the fonttools/uharfbuzz rail, the `format_locale`/`time_format_locale` d3-locale pair pinning deterministic time axes, the `allowed_base_urls` SSRF fence, and the HTML `renderer`/`bundle` — each projected onto its converter through the row's own `keys` tuple. Every native render crosses the runtime-owned offload bound (`LanePolicy.offload` — the vegafusion and matplotlib kernels on `Modality.PROCESS` with `retry=RetryClass.OCCT` for worker death, the GIL-releasing vl-convert core on `Modality.THREAD`, lets-plot on `Modality.INTERPRETER`), inside an OpenTelemetry span carrying engine/dialect/theme plus the pre-pass evidence. The producer lands the one node contract: `emit()` mints the key PRE-RUN over the canonical `(chart, fmt, policy)` input, `_emit` renders once and threads the same key into the receipt; the themed bytes are a flat handoff `composition/compose#COMPOSE` places as parent content keys, re-rendering nothing.

## [01]-[INDEX]

- [01]-[EXPORT]: host-free static-and-interactive export rows over the typed `RenderPolicy` and the per-engine `VL_RENDER`/`LP_RENDER` format tables (each total over `ExportFormat`), the `emit()`/`_emit` node contract minting `ArtifactReceipt.Chart`, every native render across the runtime offload bound inside one span — consuming spec's chart case and derive's `Palette`/`hex_ramp`; consumed by compose as DATA parent keys.
- [02]-[PREPASS]: the in-page `VegaTransform` pre-pass policy over the gated vegafusion runtime — `Passthrough`/`Inline`/`State` discriminating by transform presence and interactivity, each arm returning `Result[PrePass, TransformFault]` carrying the self-contained spec AND the `PrePassEvidence` (warnings, the three explicit data-loss flags, tz, dataset and comm-plan cardinality, worker-pool occupancy) — plus `planned` the pre-execution cost gate over `build_pre_transform_spec_plan` and the `get_column_usage` input-prune map.

## [02]-[EXPORT]

- Owner: `ChartExport` the export dispatch over the chart case, the typed `RenderPolicy`, and the `ExportFormat` whose `row` column resolves the `VlRow` converter member; `VlRow` the frozen row carrying each format's vl-convert `convert` member, its `text` flag, and its admitted-policy `keys` tuple so the per-format axis is one `VL_RENDER` table, never a per-format `match` arm; `RenderPolicy.projected(row.keys)` spreads exactly the parameters that converter admits (`scale`/`ppi` only on raster rows, `quality` only on JPEG, `allowed_base_urls` only on non-HTML rows, `renderer`/`bundle` only on HTML, the locale pair on every row). `_export_host_free` over the chart case IS the engine selection — vega pre-transforms on the process seam then renders on the thread lane, lets-plot renders in-process, matplotlib renders on the process seam — with each engine's format axis one table total over `ExportFormat` (the lets-plot JPEG row rasterizes its SVG through the shared vl-convert `resvg` core since lets-plot ships no `to_jpeg`). No parallel engine enum, no Chrome path, no plotly/kaleido.
- Entry: `emit()` returns ONE `ArtifactWork` — `key` minted PRE-RUN over the canonical `(chart, fmt, policy, transform, retention)` bytes so keyed admission probes the warm seed before any render; `_emit` computes once inside the span, folds any pre-pass evidence onto the span and the one structlog event, and mints `ArtifactReceipt.Chart(key, engine, dialect, scale, theme, byte_len)` — engine the matched `ChartSpec.tag`, dialect the `ExportFormat` value, all flat scalars, `receipt.slot == node.key`.
- Auto: `_emit` pins the pre-pass `TransformPolicy.local_tz` to the render host's `vlc.get_local_tz()` when unset — vl-convert exposes no `local_tz=` render override, so the pre-pass tz pin is what makes a time-axis chart's content key host-stable; `_register_fonts` routes `RenderPolicy.fonts` through `register_font_directory` so the engineered faces the typography rail produces are the faces the chart renders; `_pin_version` resolves `vl_version` against `get_vegalite_versions()`.
- Growth: a new export format is one `ExportFormat` member plus one `VL_RENDER` row and one `LP_RENDER` row; a new render knob is one `RenderPolicy` field named in the consuming rows' `keys`; a new pre-pass mode is one `VegaTransform` case plus one `apply` arm plus one `of` branch; a new host-free engine is one `ChartSpec` case plus one `_export_host_free` arm carrying its own format table; zero new surface.
- Boundary: no chart authoring (`visualization/chart/spec#CHART` owns the typed grammar); no palette derivation (`graphic/color/derive#DERIVE` owns `Palette`/`hex_ramp` — imported from there, never a spec alias); no figure placement (`composition/compose#COMPOSE` consumes the bytes as DATA parents); no Arrow-IPC extract side channel (the `pre_transform_extract`/`transformer` surface stays the `data/tabular` egress owner's — vl-convert accepts no external dataset, so the reduction crosses inside the spec); no folder-minted limiter or retry (the runtime lane owns both). A Chrome/kaleido path, a hand-written Vega-Lite dict downstream of the typed grammar, and a per-format render variant are the deleted forms.

## [03]-[PREPASS]

- Cases: `VegaTransform` cases — `Passthrough()` (no data transform; the spec passes through with empty evidence) · `Inline(policy, retention)` (folds `runtime.pre_transform_spec` to one self-contained spec with transforms executed and the reduced result inlined, threading `TransformPolicy` and the `Retention` interactivity policy, binding the returned `PreTransformWarning` tail as evidence) · `State(policy)` (the interactive HTML row — `runtime.new_chart_state` then `ChartState.get_transformed_spec`, reading `get_warnings`/`get_comm_plan` as the signal/dataset dependency evidence a host-free renderer needs). There is no Arrow-IPC extract case: the server-side aggregation IS the size reduction and inlines into the spec.
- Auto: `VegaTransform.of` discriminates once — `_has_transform` RECURSES the composition operators (`layer`/`concat`/`hconcat`/`vconcat`/`spec`) so a transform nested under a facet/repeat sub-view is never misread as `Passthrough`; `apply` folds the matched arm and never discards the warnings tail (`RowLimitExceeded`/`BrokenInteractivity`/`Unsupported` each derived onto its own explicit flag); a malformed spec crosses the substrate `catch(exception=ValueError)` trap and re-spells to `<malformed-spec>`. `planned` is the pre-execution cost gate — `build_pre_transform_spec_plan` predicts the client/server dataset split and the `comm_plan` variable cardinality without executing, and `get_column_usage(spec)` reports the referenced-column map a caller prunes a wide `inline_datasets` frame by BEFORE it crosses the seam. `_tuned` sets the singleton runtime's `worker_threads`/`cache_capacity`/`memory_limit` reset-on-set and `clear_cache()` reclaims pool-preserving; `_occupancy` reads `runtime.size`/`total_memory` onto the evidence at mint time.
- Growth: a new pre-pass mode is one case + one `apply` arm + one `of` branch; a new runtime knob is one `TransformPolicy` field threaded into `_tuned`; a new retention rule is one `Retention` field; a new evidence fact is one `PrePassEvidence` field the arm fills.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from io import BytesIO
from typing import Final, Literal, NoReturn, assert_never

import structlog
import vl_convert as vlc
from expression import Ok, Result, case, tag, tagged_union
from expression.collections import Map
from expression.extra.result import catch
from msgspec import Struct, structs
from opentelemetry import trace

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.graphic.color.derive import Palette, hex_ramp
from rasm.artifacts.visualization.chart.spec import ChartSpec
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass

lazy import matplotlib
lazy import vegafusion
lazy import vegafusion.utils
lazy from lets_plot import scale_color_manual, scale_fill_manual
lazy from matplotlib import colormaps, colors

# --- [TYPES] ----------------------------------------------------------------------------
# closed render domains as policy, never free strings: the 14 bundled themes + `default`, the HTML renderer.
type VegaTheme = Literal[
    "default",
    "carbong10",
    "carbong100",
    "carbong90",
    "carbonwhite",
    "dark",
    "excel",
    "fivethirtyeight",
    "ggplot2",
    "googlecharts",
    "latimes",
    "powerbi",
    "quartz",
    "urbaninstitute",
    "vox",
]
type Renderer = Literal["svg", "canvas", "hybrid"]
type Spec = dict[str, object]
type WarningKind = Literal["RowLimitExceeded", "BrokenInteractivity", "Unsupported"]
type TransformFault = Literal["<malformed-spec>"]
type KeepVar = str | tuple[str, tuple[int, ...]]  # a variable `name`, or `(name, scope)` retained client-side

# --- [CONSTANTS] ------------------------------------------------------------------------
_LOG: Final = structlog.get_logger()
_TRACER: Final = trace.get_tracer(__name__)


# --- [MODELS] ---------------------------------------------------------------------------
class ExportFormat(StrEnum):
    SVG = "svg"
    PNG = "png"
    PDF = "pdf"
    HTML = "html"
    JPEG = "jpeg"

    @property
    def row(self) -> "VlRow":
        return VL_RENDER[self]

    @property
    def interactive(self) -> bool:
        return self is ExportFormat.HTML


class RenderPolicy(Struct, frozen=True):
    scale: float = 1.0
    ppi: float = 72.0
    quality: int | None = None
    theme: VegaTheme = "default"
    vl_version: str | None = None
    fonts: tuple[str, ...] = ()  # register_font_directory roots — the typography-rail faces the chart renders
    format_locale: str | None = None  # d3-format named locale — localized numbers for journal + ISO 129 annotation
    time_format_locale: str | None = None  # d3-time-format named locale — localized date/time axes
    allowed_base_urls: tuple[str, ...] | None = None  # external-data SSRF fence; None allows any
    renderer: Renderer | None = None  # HTML-row renderer; None uses the converter default
    bundle: bool | None = None  # HTML-row: True inlines every dep, None uses the CDN default

    def projected(self, keys: tuple[str, ...]) -> dict[str, object]:
        # tuple-valued knobs coerce to the converter's list[str]; a None knob is dropped so the row
        # spreads only what its converter admits.
        return {key: (list(value) if isinstance(value, tuple) else value) for key in keys if (value := getattr(self, key)) is not None}


class VlRow(Struct, frozen=True):
    convert: Callable[..., str | bytes]
    text: bool
    keys: tuple[str, ...]


class Retention(Struct, frozen=True):
    preserve_interactivity: bool = False  # Inline default: maximal static reduction; True keeps client-side selections
    keep_signals: tuple[KeepVar, ...] = ()
    keep_datasets: tuple[KeepVar, ...] = ()


class TransformPolicy(Struct, frozen=True):
    row_limit: int | None = None  # cap the inlined rows so a large server aggregation cannot bloat the spec
    local_tz: str | None = None  # default get_local_tz(); pinned so server-evaluated time transforms match the render tz
    default_input_tz: str | None = None
    inline_datasets: frozendict[str, object] = frozendict()  # named data/tabular frames the spec references by url (INPUT seam)
    worker_threads: int | None = None  # singleton runtime caps; setters reset the pool only on a changed value
    cache_capacity: int | None = None
    memory_limit: int | None = None
    reclaim_cache: bool = False  # runtime.clear_cache() — pool-preserving reclaim beside the pool-resetting cap setters


class PrePassEvidence(Struct, frozen=True):
    mode: Literal["passthrough", "inline", "state"]
    row_limit: int | None
    local_tz: str
    transformed_datasets: int  # server-reduced inlined datasets in the returned spec
    client_vars: int  # State: comm-plan server->client variable cardinality (retained interactivity)
    server_vars: int  # State: comm-plan client->server cardinality
    cache_entries: int  # runtime.size after the pre-pass
    resident_bytes: int  # runtime.total_memory after the pre-pass
    warnings: tuple[tuple[WarningKind, str], ...]  # the collected PreTransformWarning list — never discarded
    row_limit_exceeded: bool  # derived explicit data-loss flag
    interactivity_broken: bool
    unsupported: bool


class PrePass(Struct, frozen=True):
    spec: Spec  # the self-contained spec _vl_render renders directly
    evidence: PrePassEvidence


class PrePassPlan(Struct, frozen=True):
    server_datasets: int  # 0 => no server work; the caller skips the round-trip
    client_datasets: int
    client_vars: int  # predicted retained-interactivity cardinality, read off the plan without executing
    server_vars: int
    column_usage: frozendict[str, tuple[str, ...]]  # referenced columns per root dataset — the INPUT-prune fact
    warnings: tuple[tuple[WarningKind, str], ...]


_PASSTHROUGH = PrePassEvidence(
    mode="passthrough",
    row_limit=None,
    local_tz="",
    transformed_datasets=0,
    client_vars=0,
    server_vars=0,
    cache_entries=0,
    resident_bytes=0,
    warnings=(),
    row_limit_exceeded=False,
    interactivity_broken=False,
    unsupported=False,
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
                return PrePassPlan(0, 0, 0, 0, frozendict(), ())
            case VegaTransform(tag="inline", inline=(_policy, retention)):
                plan = vegafusion.utils.build_pre_transform_spec_plan(
                    spec,
                    preserve_interactivity=retention.preserve_interactivity,
                    keep_signals=list(retention.keep_signals) or None,
                    keep_datasets=list(retention.keep_datasets) or None,
                )
                return _plan(plan, vegafusion.get_column_usage(spec))
            case VegaTransform(tag="state"):
                return _plan(vegafusion.utils.build_pre_transform_spec_plan(spec), vegafusion.get_column_usage(spec))
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------
_COMPOSITIONS: Final[tuple[str, ...]] = ("layer", "concat", "hconcat", "vconcat", "spec")


def _has_transform(node: Spec) -> bool:
    # recurses the composition operators so a transform nested under a facet/repeat sub-view is never
    # misread as Passthrough — which would skip the pre-pass and ship raw data client-side.
    if isinstance(top := node.get("transform"), list) and top:
        return True
    return any(
        _has_transform(view)
        for key in _COMPOSITIONS
        for view in (raw if isinstance(raw := node.get(key), list) else (raw,))
        if isinstance(view, dict)
    )


def _tuned(policy: TransformPolicy) -> None:
    # Exemption: the vegafusion runtime singleton exposes its caps as properties whose setter resets the
    # pool only on a changed value — an idempotent per-worker tune, never a fresh runtime per transform.
    if policy.worker_threads is not None:
        vegafusion.runtime.worker_threads = policy.worker_threads
    if policy.cache_capacity is not None:
        vegafusion.runtime.cache_capacity = policy.cache_capacity
    if policy.memory_limit is not None:
        vegafusion.runtime.memory_limit = policy.memory_limit
    if policy.reclaim_cache:
        vegafusion.runtime.clear_cache()


def _occupancy() -> tuple[int, int]:
    return (vegafusion.runtime.size or 0, vegafusion.runtime.total_memory or 0)


def _warnings(raw: object) -> tuple[tuple[WarningKind, str], ...]:
    return tuple((entry["type"], entry["message"]) for entry in raw) if isinstance(raw, list) else ()


def _dataset_count(spec: Spec) -> int:
    return len(datasets) if isinstance(datasets := spec.get("datasets"), dict) else 0


def _evidence(
    mode: Literal["inline", "state"],
    tz: str,
    policy: TransformPolicy,
    spec: Spec,
    warnings: tuple[tuple[WarningKind, str], ...],
    /,
    *,
    client: int = 0,
    server: int = 0,
) -> PrePassEvidence:
    entries, resident = _occupancy()
    return PrePassEvidence(
        mode=mode,
        row_limit=policy.row_limit,
        local_tz=tz,
        transformed_datasets=_dataset_count(spec),
        client_vars=client,
        server_vars=server,
        cache_entries=entries,
        resident_bytes=resident,
        warnings=warnings,
        row_limit_exceeded=any(kind == "RowLimitExceeded" for kind, _ in warnings),
        interactivity_broken=any(kind == "BrokenInteractivity" for kind, _ in warnings),
        unsupported=any(kind == "Unsupported" for kind, _ in warnings),
    )


def _run_inline(spec: Spec, policy: TransformPolicy, retention: Retention) -> Result[PrePass, TransformFault]:
    tz = policy.local_tz or vegafusion.get_local_tz() or "UTC"
    return (
        catch(exception=ValueError)(vegafusion.runtime.pre_transform_spec)(
            spec,
            row_limit=policy.row_limit,
            local_tz=tz,
            default_input_tz=policy.default_input_tz,
            preserve_interactivity=retention.preserve_interactivity,
            inline_datasets=dict(policy.inline_datasets) or None,
            keep_signals=list(retention.keep_signals) or None,
            keep_datasets=list(retention.keep_datasets) or None,
        )
        .map_error(lambda _raised: "<malformed-spec>")
        .map(lambda pair: PrePass(pair[0], _evidence("inline", tz, policy, pair[0], _warnings(pair[1]))))
    )


def _run_state(spec: Spec, policy: TransformPolicy) -> Result[PrePass, TransformFault]:
    tz = policy.local_tz or vegafusion.get_local_tz() or "UTC"

    def _read(chart_state: object) -> PrePass:
        transformed = chart_state.get_transformed_spec()  # the ONE computed fact — spec AND evidence read off it once
        comm = chart_state.get_comm_plan()
        return PrePass(
            transformed,
            _evidence(
                "state",
                tz,
                policy,
                transformed,
                _warnings(chart_state.get_warnings()),
                client=len(comm.get("server_to_client", ())),
                server=len(comm.get("client_to_server", ())),
            ),
        )

    return (
        catch(exception=ValueError)(vegafusion.runtime.new_chart_state)(
            spec,
            local_tz=tz,
            default_input_tz=policy.default_input_tz,
            row_limit=policy.row_limit,
            inline_datasets=dict(policy.inline_datasets) or None,
        )
        .map_error(lambda _raised: "<malformed-spec>")
        .map(_read)
    )


def _columns(usage: object) -> frozendict[str, tuple[str, ...]]:
    # {root_dataset: [col, ...] | None}; a None entry (not statically determinable) folds to () — a total map.
    return frozendict({name: tuple(cols) for name, cols in usage.items() if isinstance(cols, list)}) if isinstance(usage, dict) else frozendict()


def _plan(plan: object, usage: object) -> PrePassPlan:
    server = plan.get("server_spec", {}) if isinstance(plan, dict) else {}
    client = plan.get("client_spec", {}) if isinstance(plan, dict) else {}
    comm = plan.get("comm_plan", {}) if isinstance(plan, dict) else {}
    warns = plan.get("warnings", ()) if isinstance(plan, dict) else ()
    return PrePassPlan(
        server_datasets=len(data) if isinstance(data := server.get("data"), list) else 0,
        client_datasets=len(cdata) if isinstance(cdata := client.get("data"), list) else 0,
        client_vars=len(s2c) if isinstance(s2c := comm.get("server_to_client"), list) else 0,
        server_vars=len(c2s) if isinstance(c2s := comm.get("client_to_server"), list) else 0,
        column_usage=_columns(usage),
        warnings=_warnings(warns),
    )


def _register_fonts(fonts: tuple[str, ...]) -> None:
    for directory in fonts:
        vlc.register_font_directory(directory)


def _pin_version(requested: str | None) -> str:
    available = vlc.get_vegalite_versions()
    return requested if requested in available else available[-1]


def _vl_render(spec: Spec, fmt: ExportFormat, policy: RenderPolicy) -> bytes:
    _register_fonts(policy.fonts)
    row = fmt.row
    pinned = structs.replace(policy, vl_version=_pin_version(policy.vl_version))
    output = row.convert(spec, **pinned.projected(row.keys))
    return output.encode() if row.text else output


def _lp_native(method: str) -> Callable[[object], bytes]:
    def render(plot: object) -> bytes:
        sink = BytesIO()
        getattr(plot, method)(sink)  # PlotSpec.to_*(file-like) writes bytes in-process
        return sink.getvalue()

    return render


def _letsplot_to_bytes(plot: object, palette: Palette, fmt: ExportFormat) -> bytes:
    ramp = hex_ramp(palette)
    themed = plot + scale_color_manual(values=ramp) + scale_fill_manual(values=ramp)
    return LP_RENDER[fmt](themed)


def _pre_transform(spec: Spec, interactive: bool, policy: TransformPolicy, retention: Retention) -> Result[PrePass, TransformFault]:
    # the process-seam kernel: `of` decides the arm, `apply` returns the self-contained spec paired
    # with the evidence — the whole Result pickles back across the seam.
    return VegaTransform.of(spec, interactive, policy, retention).apply(spec)


def _raise_transform(fault: object) -> NoReturn:
    # terminal collapse at the render boundary: a pre-pass fault or an offload fault reconstructs the raise
    # the drain's fault capsule folds onto the node's rail — never a Result fed to _vl_render.
    raise ValueError(str(fault))


def _evidence_attrs(evidence: PrePassEvidence) -> dict[str, object]:
    return {
        "prepass.mode": evidence.mode,
        "prepass.local_tz": evidence.local_tz,
        "prepass.transformed_datasets": evidence.transformed_datasets,
        "prepass.warnings": len(evidence.warnings),
        "prepass.row_limit_exceeded": evidence.row_limit_exceeded,
        "prepass.interactivity_broken": evidence.interactivity_broken,
        "prepass.unsupported": evidence.unsupported,
        "prepass.client_vars": evidence.client_vars,
        "prepass.server_vars": evidence.server_vars,
    }


def _matplotlib_savefig(figure: object, palette: Palette, fmt: str, ppi: float) -> bytes:
    matplotlib.use("Agg")
    colormaps.register(colors.ListedColormap(palette, name=f"chart-{id(figure):x}"), force=True)
    for axes in figure.axes:
        axes.set_prop_cycle(color=hex_ramp(palette))
    figure.set_layout_engine("constrained")  # auto-fit spacing as a figure policy, never a per-call tight_layout
    sink = BytesIO()
    # the Agg writer covers png/pdf/svg/jpeg but NOT html: the static figure serves the HTML slot as
    # prolog-stripped inline SVG in a standalone shell — keeping the arm total over ExportFormat.
    figure.savefig(
        sink, format="svg" if fmt == "html" else fmt, dpi=ppi, bbox_inches="tight", pad_inches=0.02, metadata={"Creator": "rasm.artifacts"}
    )
    payload = sink.getvalue()
    return b'<!doctype html><meta charset="utf-8"><figure>' + payload[payload.index(b"<svg") :] + b"</figure>" if fmt == "html" else payload


async def _export_host_free(
    chart: ChartSpec, fmt: ExportFormat, policy: RenderPolicy, transform: TransformPolicy, retention: Retention
) -> tuple[bytes, PrePassEvidence | None]:
    match chart:
        case ChartSpec(tag="vega", vega=spec):
            railed = await LanePolicy.offload(
                _pre_transform, spec, fmt.interactive, transform, retention, modality=Modality.PROCESS, retry=RetryClass.OCCT
            )
            prepass = railed.default_with(_raise_transform).default_with(_raise_transform)  # offload fault, then <malformed-spec>
            data = await LanePolicy.offload(_vl_render, prepass.spec, fmt, policy, modality=Modality.THREAD)
            return data.default_with(_raise_transform), prepass.evidence
        case ChartSpec(tag="lets_plot", lets_plot=(plot, palette)):
            rendered = await LanePolicy.offload(_letsplot_to_bytes, plot, palette, fmt, modality=Modality.INTERPRETER)
            return rendered.default_with(_raise_transform), None
        case ChartSpec(tag="matplotlib", matplotlib=(figure, palette)):
            rendered = await LanePolicy.offload(
                _matplotlib_savefig, figure, palette, fmt.value, policy.ppi, modality=Modality.PROCESS, retry=RetryClass.OCCT
            )
            return rendered.default_with(_raise_transform), None
        case _:
            assert_never(chart)


# --- [TABLES] ---------------------------------------------------------------------------
# each `keys` names ONLY the converter's real parameters: pdf/svg/html drop ppi, only jpeg carries quality,
# HTML alone carries renderer/bundle and drops allowed_base_urls, the locale pair rides every row.
VL_RENDER: Final[Map[ExportFormat, VlRow]] = Map.of_seq([
    (ExportFormat.SVG, VlRow(vlc.vegalite_to_svg, True, ("theme", "vl_version", "allowed_base_urls", "format_locale", "time_format_locale"))),
    (
        ExportFormat.PNG,
        VlRow(vlc.vegalite_to_png, False, ("scale", "ppi", "theme", "vl_version", "allowed_base_urls", "format_locale", "time_format_locale")),
    ),
    (
        ExportFormat.PDF,
        VlRow(vlc.vegalite_to_pdf, False, ("scale", "theme", "vl_version", "allowed_base_urls", "format_locale", "time_format_locale")),
    ),
    (ExportFormat.HTML, VlRow(vlc.vegalite_to_html, True, ("theme", "vl_version", "bundle", "renderer", "format_locale", "time_format_locale"))),
    (
        ExportFormat.JPEG,
        VlRow(vlc.vegalite_to_jpeg, False, ("scale", "quality", "theme", "vl_version", "allowed_base_urls", "format_locale", "time_format_locale")),
    ),
])
# total over ExportFormat: lets-plot ships no to_jpeg, so JPEG rasterizes the lets-plot SVG through the
# shared vl-convert resvg core — every format has a row, never a missing-key fault.
LP_RENDER: Final[Map[ExportFormat, Callable[[object], bytes]]] = Map.of_seq([
    (ExportFormat.SVG, _lp_native("to_svg")),
    (ExportFormat.PNG, _lp_native("to_png")),
    (ExportFormat.PDF, _lp_native("to_pdf")),
    (ExportFormat.HTML, _lp_native("to_html")),
    (ExportFormat.JPEG, lambda plot: vlc.svg_to_jpeg(_lp_native("to_svg")(plot).decode())),
])


# --- [COMPOSITION] ----------------------------------------------------------------------
class ChartExport(Struct, frozen=True):
    chart: ChartSpec
    fmt: ExportFormat
    policy: RenderPolicy = RenderPolicy()
    transform: TransformPolicy = TransformPolicy()
    retention: Retention = Retention()

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _resolved(self) -> TransformPolicy:
        # vl-convert exposes NO local_tz render override, so the pre-pass tz pin is what makes a
        # time-axis chart's reduced spec host-tz-stable; ONE resolution feeds both key and render.
        return self.transform if self.transform.local_tz is not None else structs.replace(self.transform, local_tz=vlc.get_local_tz() or "UTC")

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: the canonical (chart, format, policies) bytes, minted PRE-RUN over the SAME
        # tz-resolved transform `_emit` renders with, so two hosts resolving different local zones never
        # share a key — a re-issued unchanged chart elides on its warm key before any pre-pass or render runs.
        return ContentIdentity.of(f"chart-{self.fmt}", (self.chart, self.fmt, self.policy, self._resolved, self.retention), policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        with _TRACER.start_as_current_span(f"chart.export.{self.fmt}") as span:
            transform = self._resolved
            span.set_attributes({
                "engine": self.chart.tag,
                "dialect": self.fmt.value,
                "theme": self.policy.theme,
                "vega": vlc.get_vega_version(),
                "tz": transform.local_tz or "unknown",
            })
            data, evidence = await _export_host_free(self.chart, self.fmt, self.policy, transform, self.retention)
            if evidence is not None:
                span.set_attributes(_evidence_attrs(evidence))
        _LOG.info(
            "chart.export",
            engine=self.chart.tag,
            dialect=self.fmt.value,
            bytes=len(data),
            **(_evidence_attrs(evidence) if evidence is not None else {}),
        )
        return Ok(ArtifactReceipt.Chart(self._key, self.chart.tag, self.fmt.value, self.policy.scale, self.policy.theme, len(data)))
```

`_export_host_free` IS the engine selection — the band-routing decision over the three chart cases, not byte-emit sprawl. The vega arm threads `_pre_transform` across the process lane with the OCCT worker-death retry, collapses a `<malformed-spec>` fault to the boundary raise, renders `PrePass.spec` on the thread lane, and folds the data-loss flags, pinned tz, and dataset/comm-plan cardinality onto the span and structlog event; lets-plot renders in-process; matplotlib renders on the process lane. Each engine's format axis is one row table total over `ExportFormat`, so a new output format is one row per table and `RenderPolicy.projected(row.keys)` spreads exactly the converter parameters the row names. The pre-pass lives beside the render it feeds: `VegaTransform.of` decides once from the recursive transform-presence scan and the interactive flag, `apply` never discards the warnings tail, `planned` prices the split before paying for execution, and the `inline_datasets` INPUT seam stays distinct from the no-external-OUTPUT constraint the renderer imposes — the reduction crosses inside the spec, and the `pre_transform_extract` Arrow-IPC surface stays the `data/tabular` egress owner's. The node key mints over the chart INPUT, so the warm seed elides an unchanged chart before any engine spins.
