# [PY_ARTIFACTS_CHART_EXPORT]

`ChartExport` is the render half of the 2D chart axis: format dispatch and the vegafusion data pre-pass on one page. It folds the `visualization/chart/spec#CHART` chart case, the typed `ChartRenderPolicy`, and the target `ExportFormat` to bytes, minting one `core/receipt#RECEIPT` `ArtifactReceipt.Chart`. Host-free posture is decisive: vl-convert stays the one chart-origin SVG rasterizer (Rust-native, embedded V8, zero browser), lets-plot self-renders in-process on its named `to_svg`/`to_png`/`to_pdf` rows, matplotlib serves the publication case. Both Vega dialects ride one row table — each `VlRow` binds the `vegalite_to_*` converter beside its `vega_to_*` twin, and the spec's own `$schema` selects the family, so a consumer-compiled full-Vega dict renders without a dialect flag beside the spec that already carries it. `VegaTransform` pre-pass lives in this page because vl-convert exposes no external-dataset feed — server-evaluated transforms inline into one self-contained spec the render arm consumes, so the reduced data crosses inside the spec and the interactive HTML row needs no live server.

`ChartRenderPolicy` carries the full converter axis as one typed policy value — raster `scale`/`ppi`/`quality`, the `VegaTheme` theme, the pinned `vl_version`, `register_font_directory` closing the font-identity loop, the `format_locale`/`time_format_locale` d3-locale pair, the `allowed_base_urls` SSRF fence (a non-empty fence refuses HTML export typed, because the browser-side render cannot enforce it), the `show_warnings` compile-diagnostic toggle the Vega-Lite raster/SVG rows admit, and the HTML `renderer`/`bundle` — and each `VlRow` stores two fully typed `(Spec, ChartRenderPolicy) -> str | bytes` calls with explicit provider keywords, so either a policy-field rename or a converter-signature change breaks its row at type-check. Every native render crosses the runtime lane through the owned `lane: LanePolicy` (vegafusion and matplotlib as `KernelTrait.HOSTILE` kernels on the warm process pool under the trait-row worker-death retry, the GIL-releasing vl-convert core and the lets-plot self-render as `KernelTrait.RELEASING` — lets-plot bundles a native core whose catalog fixes the thread arm, never a subinterpreter) inside one OpenTelemetry span, and every fault stays a typed `RuntimeRail` — a pre-pass refusal folds onto `BoundaryFault` at the seam, never a stringified raise mid-transform. Node contract is `emit()` minting the key PRE-RUN over the canonical input preimage and `_emit` threading that same key into the receipt; the Vega case admits `keyed` for warm elision while the live-figure cases admit `bare`, because a live `Figure`/`PlotSpec` has no cross-host canonical bytes. Themed bytes are a flat handoff `composition/compose#COMPOSE` places; `layered()` is the one editorial alternative — the rendered SVG decomposed into semantic `role-*` mark-group layers as the `tuple[Layer, ...]` that `export/layered#LAYERED` lands as a named-layer designer file.

## [01]-[INDEX]

- [01]-[EXPORT]: host-free static-and-interactive export over the typed `ChartRenderPolicy` and the per-engine `VL_RENDER`/`LP_RENDER` format tables, minting `ArtifactReceipt.Chart` across the runtime offload bound inside one span, with the `layered()` semantic-layer hand-off to `export/layered#LAYERED`.
- [02]-[PREPASS]: in-page `VegaTransform` pre-pass over the gated vegafusion runtime — `Passthrough`/`Inline`/`State` by transform presence and interactivity, each returning the self-contained spec with its `PrePassEvidence`.

## [02]-[EXPORT]

- Owner: `ChartExport` dispatches over the chart case, the typed `ChartRenderPolicy`, and `ExportFormat` resolving the `VlRow` converter pair; `VL_RENDER` rows the per-format axis as one `frozendict` table, never a per-format arm, and each row calls its provider with explicit typed keywords — no string field-name tuple, dynamic spread, or erased callable. `_rendered` is the engine selection over the three chart cases, each engine's format axis one table total over `ExportFormat`. No parallel engine enum, no Chrome path, no plotly/kaleido.
- Entry: `emit()` returns one `ArtifactWork` — `key` minted PRE-RUN through `ContentIdentity.key` over the length-framed canonical preimage: engine tag, the case's canonical bytes (the deterministic-encoded spec dict; protocol-5 pickle for a live figure, the picklability the matplotlib `PROCESS` offload already demands), the encoded `(fmt, policy, resolved transform, retention)` bundle, and each inline dataset's name and `hash_rows` digest. Vega's case lowers `Admission(keyed=None)` so keyed admission probes the warm seed; the lets-plot/matplotlib cases lower `Admission(bare=None)` — pickle bytes identify the node but are not cross-host canonical, so a live figure is forced-live rather than trusted to elide. `_emit` computes once inside the span — pre-pass evidence lands as one `chart.prepass` span event at the stage boundary and rides the structlog event — and mints `ArtifactReceipt.Chart(key, engine, format, scale, theme, byte_len)` as flat scalars, `receipt.slot == node.key`.
- Auto: `_resolved` pins the pre-pass `local_tz` to `vlc.get_local_tz()` when unset — vl-convert exposes no `local_tz=` render override, so the pin makes a time-axis chart's content key host-stable and one resolution feeds key and render; `_vl_render` registers every `ChartRenderPolicy.fonts` directory through `register_font_directory`, resolves `vl_version` against `get_vegalite_versions()`, and selects a converter whose typed row omits the Vega-Lite-only `theme`/`vl_version` knobs from the full-Vega twin; `_dialect` reads the admitted spec's `$schema` once.
- Layers: `layered()` is the editorial hand-off — the Vega case renders ONE SVG, `_split_layers` partitions the root graphics group's children by their Vega `role-*` class token through `lxml`, and each semantic group (axes, gridlines, legends, mark groups, titles) becomes one `export/layered#LAYERED` `Layer(name, svg_bytes, bbox, intent=OcgIntent.FIGURE, group="chart")` sharing the chart's viewBox, so a designer restyles axes and marks as separate named layers without re-plotting; its pre-pass and split crossings run inside one `chart.layers` span carrying the `chart.prepass` stage event and the charter Error fold; the live-figure cases rail `<vega-only>` — their engines own no semantic scenegraph to split.
- Growth: a new export format is one `ExportFormat` member, one `VL_RENDER` row, and one `LP_RENDER` row; a new render knob is one `ChartRenderPolicy` field read by the exact converter rows that admit it; a new pre-pass mode is one `VegaTransform` case, an `apply` arm, and an `of` branch; a new host-free engine is one `ChartSpec` case and one `_rendered` arm carrying its format table.
- Boundary: no chart authoring (`visualization/chart/spec#CHART` owns the grammar); no palette derivation (`graphic/color/derive#DERIVE` owns `Palette`/`hex_ramp`); no figure placement (`composition/compose#COMPOSE` consumes bytes as DATA parents); no Arrow-IPC extract side channel — vl-convert accepts no external dataset, so the reduction crosses inside the spec, and `get_column_usage`/`pre_transform_extract` stay the `data/tabular/columnar#COLUMNAR` egress owner's (the runtime already prunes inline frames internally through `get_inline_column_usage` on every call, so no chart-side column read exists); no folder-minted limiter or retry — the offload rides the owned `lane: LanePolicy`. A Chrome/kaleido path, a hand-written Vega-Lite dict downstream of the grammar, a per-format render variant, a string-keyed policy spread, and a raise standing in for a typed rail fault are the rejected forms.

## [03]-[PREPASS]

- Cases: `VegaTransform` cases — `Passthrough()` (no transform, or a full-Vega dialect whose transforms the embedded Vega runtime evaluates at render; spec passes through with empty evidence), `Inline(policy, retention)` (folds `pre_transform_spec` to one self-contained spec with transforms executed and the reduced result inlined, binding the `PreTransformWarning` tail as evidence), `State(policy)` (the interactive HTML row via `new_chart_state` then `get_transformed_spec`, reading `get_warnings`/`get_comm_plan` as dataset-dependency evidence). No Arrow-IPC extract case — server-side aggregation is the size reduction and inlines into the spec.
- Auto: `VegaTransform.of` discriminates once — `_has_transform` recurses the composition operators so a transform nested under a facet/repeat sub-view is never misread as `Passthrough`, and the full-Vega dialect short-circuits to `Passthrough`; `apply` never discards the warnings tail (`RowLimitExceeded`/`BrokenInteractivity`/`Unsupported` each on its own explicit data-loss flag); a malformed spec crosses `catch(ValueError)` to `<malformed-spec>`. `planned` prices the client/server split via `build_pre_transform_spec_plan` without executing. `_tuned` resolves EVERY cap from the policy or the once-captured pristine defaults and assigns the singleton reset-on-change — a None field never inherits worker history; `_occupancy` reads `size`/`total_memory` onto the evidence. `TransformPolicy.inline_datasets` is typed `frozendict[str, pl.DataFrame]` — the estate frame wire, each frame's `hash_rows` digest joining the content preimage so a changed input frame misses the cache.
- Growth: a new pre-pass mode is one case, one `apply` arm, and one `of` branch; a new runtime knob is one `TransformPolicy` field threaded into `_tuned`; a new retention rule is one `Retention` field; a new evidence fact is one `PrePassEvidence` field the arm fills.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import copy
import pickle
from collections.abc import Callable
from enum import StrEnum
from functools import cache, partial
from io import BytesIO
from typing import Final, Literal, assert_never, cast

import structlog
import vl_convert as vlc
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.extra.result import catch
from msgspec import Struct, json, structs
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode
from pydantic import JsonValue

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.export.layered import Layer, OcgIntent
from rasm.artifacts.graphic.color.derive import Palette, hex_ramp
from rasm.artifacts.visualization.chart.spec import ChartSpec
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

lazy import matplotlib
lazy import polars as pl
lazy import vegafusion
lazy import vegafusion.utils
lazy from lets_plot import scale_color_manual, scale_fill_manual
lazy from lets_plot.plot.core import PlotSpec
lazy from lets_plot.plot.subplots import SupPlotsSpec
lazy from lxml import etree
lazy from matplotlib import colors
lazy from matplotlib.figure import Figure
lazy from vegafusion.runtime import ChartState, PreTransformWarning
lazy from vegafusion.utils import PreTransformSpecPlan

# --- [TYPES] ----------------------------------------------------------------------------
type VegaTheme = Literal[
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
type VegaDialect = Literal["vega-lite", "vega"]
type Spec = dict[str, JsonValue]
type WarningKind = Literal["RowLimitExceeded", "BrokenInteractivity", "Unsupported"]
type TransformFault = Literal["<malformed-spec>"]
type KeepVar = str | tuple[str, tuple[int, ...]]
type VlRender = Callable[[Spec, ChartRenderPolicy], str | bytes]
type LpRender = Callable[[PlotSpec | SupPlotsSpec], bytes]

# --- [CONSTANTS] ------------------------------------------------------------------------
_LOG: Final = structlog.get_logger()
_TRACER: Final = trace.get_tracer(__name__)
_CANON: Final = json.Encoder(order="deterministic")


# --- [MODELS] ---------------------------------------------------------------------------
class ExportFormat(StrEnum):
    SVG = "svg"
    PNG = "png"
    PDF = "pdf"
    HTML = "html"
    JPEG = "jpeg"

    @property
    def interactive(self) -> bool:
        return self is ExportFormat.HTML


class ChartRenderPolicy(Struct, frozen=True):
    scale: float = 1.0
    ppi: float = 72.0
    quality: int | None = None
    theme: VegaTheme | None = None
    vl_version: str | None = None
    fonts: tuple[str, ...] = ()
    format_locale: str | None = None
    time_format_locale: str | None = None
    allowed_base_urls: tuple[str, ...] | None = None
    renderer: Renderer | None = None
    bundle: bool | None = None
    show_warnings: bool | None = None


class VlRow(Struct, frozen=True):
    vl: VlRender
    vega: VlRender
    text: bool


class Retention(Struct, frozen=True):
    preserve_interactivity: bool = False
    keep_signals: tuple[KeepVar, ...] = ()
    keep_datasets: tuple[KeepVar, ...] = ()


class TransformPolicy(Struct, frozen=True):
    row_limit: int | None = None
    local_tz: str | None = None
    default_input_tz: str | None = None
    inline_datasets: frozendict[str, pl.DataFrame] = frozendict()
    worker_threads: int | None = None
    cache_capacity: int | None = None
    memory_limit: int | None = None
    reclaim_cache: bool = False


class PrePassEvidence(Struct, frozen=True):
    mode: Literal["passthrough", "inline", "state"]
    row_limit: int | None
    local_tz: str
    transformed_datasets: int
    client_vars: int
    server_vars: int
    cache_entries: int
    resident_bytes: int
    warnings: tuple[tuple[WarningKind, str], ...]
    row_limit_exceeded: bool
    interactivity_broken: bool
    unsupported: bool


class PrePass(Struct, frozen=True):
    spec: Spec
    evidence: PrePassEvidence


class PrePassPlan(Struct, frozen=True):
    server_datasets: int
    client_datasets: int
    client_vars: int
    server_vars: int
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
    def of(spec: Spec, fmt: ExportFormat, policy: TransformPolicy = TransformPolicy(), retention: Retention = Retention()) -> "VegaTransform":
        # retention rides the inline case alone by LAW, not omission: the state arm's `new_chart_state` strips
        # nothing — every signal and dataset survives and interactivity is preserved by construction — so any
        # retention request is satisfied as a superset there, and only the inline fold consumes the keep/preserve
        # knobs it can actually violate.
        has_transform = _dialect(spec) == "vega-lite" and _has_transform(spec)
        if fmt.interactive and has_transform:
            return VegaTransform(state=policy)
        return VegaTransform(inline=(policy, retention)) if has_transform else VegaTransform(passthrough=None)

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
                return PrePassPlan(0, 0, 0, 0, ())
            case VegaTransform(tag="inline", inline=(_policy, retention)):
                plan = vegafusion.utils.build_pre_transform_spec_plan(
                    spec,
                    preserve_interactivity=retention.preserve_interactivity,
                    keep_signals=list(retention.keep_signals) or None,
                    keep_datasets=list(retention.keep_datasets) or None,
                )
                return _plan(plan)
            case VegaTransform(tag="state"):
                return _plan(vegafusion.utils.build_pre_transform_spec_plan(spec))
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------
_COMPOSITIONS: Final[tuple[str, ...]] = ("layer", "concat", "hconcat", "vconcat", "spec")


def _dialect(spec: Spec) -> VegaDialect:
    # spec admission proves a supported canonical $schema at the boundary; this read stays total for a direct
    # caller by classing an absent or foreign $schema as full Vega — the passthrough arm whose transforms the
    # embedded runtime evaluates at render — never a KeyError mid-discrimination.
    schema = spec.get("$schema")
    return "vega-lite" if isinstance(schema, str) and "/vega-lite/" in schema else "vega"


def _has_transform(node: Spec) -> bool:
    if isinstance(top := node.get("transform"), list) and top:
        return True
    return any(
        _has_transform(view)
        for key in _COMPOSITIONS
        for view in (raw if isinstance(raw := node.get(key), list) else (raw,))
        if isinstance(view, dict)
    )


@cache
def _runtime_defaults() -> tuple[int, int, int]:
    # pristine singleton caps captured ONCE before any policy mutation; a None-valued policy field resolves
    # HERE, so identical inputs see identical effective caps regardless of worker history or job order.
    return (vegafusion.runtime.worker_threads, vegafusion.runtime.cache_capacity, vegafusion.runtime.memory_limit)


def _tuned(policy: TransformPolicy) -> None:
    # every prepass observes a FULLY RESOLVED cap set — never process-history inheritance for a None field.
    # `vegafusion.runtime` is process-global, so this mutation is legal ONLY worker-side: `_pre_transform`
    # crosses as a HOSTILE kernel, whose process worker executes one job at a time, so the tune and its prepass
    # are serialized per process by construction and no concurrent prepass ever observes a foreign cap set.
    threads, cache_cap, memory = _runtime_defaults()
    vegafusion.runtime.worker_threads = policy.worker_threads if policy.worker_threads is not None else threads
    vegafusion.runtime.cache_capacity = policy.cache_capacity if policy.cache_capacity is not None else cache_cap
    vegafusion.runtime.memory_limit = policy.memory_limit if policy.memory_limit is not None else memory
    if policy.reclaim_cache:
        vegafusion.runtime.clear_cache()


def _occupancy() -> tuple[int, int]:
    return (vegafusion.runtime.size or 0, vegafusion.runtime.total_memory or 0)


def _warnings(raw: list[PreTransformWarning] | list[dict[str, object]]) -> tuple[tuple[WarningKind, str], ...]:
    return tuple((cast(WarningKind, entry["type"]), str(entry["message"])) for entry in raw)


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

    def _read(chart_state: ChartState) -> PrePass:
        transformed = chart_state.get_transformed_spec()
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


def _plan(plan: PreTransformSpecPlan) -> PrePassPlan:
    server = plan.get("server_spec", {})
    client = plan.get("client_spec", {})
    comm = plan.get("comm_plan", {})
    warns = plan.get("warnings", [])
    return PrePassPlan(
        server_datasets=len(data) if isinstance(data := server.get("data"), list) else 0,
        client_datasets=len(cdata) if isinstance(cdata := client.get("data"), list) else 0,
        client_vars=len(s2c) if isinstance(s2c := comm.get("server_to_client"), list) else 0,
        server_vars=len(c2s) if isinstance(c2s := comm.get("client_to_server"), list) else 0,
        warnings=_warnings(warns),
    )


def _pin_version(requested: str | None) -> str:
    # `vl_version` admits both `'6.4'` and `'v6.4'` spellings; the pin normalizes before the membership test.
    available = vlc.get_vegalite_versions()
    bare = (requested or "").removeprefix("v")
    return bare if bare in available else available[-1]


def _vl_render(spec: Spec, fmt: ExportFormat, policy: ChartRenderPolicy) -> bytes:
    tuple(map(vlc.register_font_directory, policy.fonts))
    row, dialect = VL_RENDER[fmt], _dialect(spec)
    pinned = structs.replace(policy, vl_version=_pin_version(policy.vl_version))
    output = (row.vl if dialect == "vega-lite" else row.vega)(spec, pinned)
    return output.encode() if row.text else output


def _lp_bytes(export: Callable[[BytesIO], str | None]) -> bytes:
    sink = BytesIO()
    export(sink)
    return sink.getvalue()


def _letsplot_to_bytes(plot: PlotSpec | SupPlotsSpec, palette: Palette, fmt: ExportFormat) -> bytes:
    # A composite root's subplots arrive palette-threaded from authoring; only a lone PlotSpec gains the manual scales.
    ramp = hex_ramp(palette)
    themed = plot + scale_color_manual(values=ramp) + scale_fill_manual(values=ramp) if isinstance(plot, PlotSpec) else plot
    return LP_RENDER[fmt](themed)


def _pre_transform(spec: Spec, fmt: ExportFormat, policy: TransformPolicy, retention: Retention) -> Result[PrePass, TransformFault]:
    return VegaTransform.of(spec, fmt, policy, retention).apply(spec)


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


def _faulted(span: trace.Span, event: str, fault: BoundaryFault, /, **fields: object) -> BoundaryFault:
    span.set_status(Status(StatusCode.ERROR, fault.tag))
    _LOG.error(event, **fields, **fault.facts())
    return fault


def _matplotlib_savefig(figure: Figure, palette: Palette, fmt: str, ppi: float) -> bytes:
    matplotlib.use("Agg")
    ramp = hex_ramp(palette)
    cmap = colors.ListedColormap(ramp, name="chart")
    for axes in figure.axes:
        axes.set_prop_cycle(color=ramp)
        tuple(scalar.set_cmap(cmap) for scalar in (*axes.images, *axes.collections))
    figure.set_layout_engine("constrained")
    sink = BytesIO()
    figure.savefig(
        sink, format="svg" if fmt == "html" else fmt, dpi=ppi, bbox_inches="tight", pad_inches=0.02, metadata={"Creator": "rasm.artifacts"}
    )
    payload = sink.getvalue()
    return b'<!doctype html><meta charset="utf-8"><figure>' + payload[payload.index(b"<svg") :] + b"</figure>" if fmt == "html" else payload


def _role(element: etree._Element) -> str:
    tokens = (element.get("class") or "").split()
    return next((token.removeprefix("role-") for token in tokens if token.startswith("role-")), "mark")


def _split_layers(spec: Spec, policy: ChartRenderPolicy) -> tuple[Layer, ...]:
    # ONE SVG render partitioned into semantic layers: Vega emits `class="mark-group role-*"` groups under one root
    # graphics group. Each layer is INDEPENDENTLY RENDERABLE: the shell carries the root `<defs>` closure (clip
    # paths, gradients) with the root/frame presentation attributes and transforms the detached child inherited,
    # so a recomposed layer stack is visually equivalent to the single source SVG.
    root = etree.fromstring(_vl_render(spec, ExportFormat.SVG, policy))
    view_box = root.get("viewBox") or f"0 0 {root.get('width', '0')} {root.get('height', '0')}"
    x0, y0, width, height = (float(part) for part in view_box.split())
    bbox = (x0, y0, x0 + width, y0 + height)  # Layer.bbox is (x0, y0, x1, y1); SVG viewBox is (x, y, w, h)
    svg_ns = "http://www.w3.org/2000/svg"
    frame = next(iter(root.findall(f"{{{svg_ns}}}g")), root)
    defs = tuple(root.findall(f"{{{svg_ns}}}defs"))
    inherited = {name: value for owner in (root, frame) for name, value in owner.attrib.items() if name not in ("viewBox", "width", "height", "class")}

    def wrapped(child: etree._Element) -> bytes:
        shell = etree.Element(f"{{{svg_ns}}}svg", attrib={"viewBox": view_box})
        for block in defs:
            shell.append(copy.deepcopy(block))
        carrier = etree.SubElement(shell, f"{{{svg_ns}}}g", attrib=inherited)
        carrier.append(child)
        return etree.tostring(shell)

    return tuple(
        Layer(f"{index:02d}-{_role(child)}", wrapped(child), bbox, intent=OcgIntent.FIGURE, group="chart")
        for index, child in enumerate(frame.findall(f"{{{svg_ns}}}g"))
    )


# --- [TABLES] ---------------------------------------------------------------------------
VL_RENDER: Final[frozendict[ExportFormat, VlRow]] = frozendict({
    ExportFormat.SVG: VlRow(
        lambda spec, p: vlc.vegalite_to_svg(
            spec,
            theme=p.theme,
            vl_version=p.vl_version,
            show_warnings=p.show_warnings,
            allowed_base_urls=list(p.allowed_base_urls) if p.allowed_base_urls else None,
            format_locale=p.format_locale,
            time_format_locale=p.time_format_locale,
        ),
        lambda spec, p: vlc.vega_to_svg(
            spec,
            allowed_base_urls=list(p.allowed_base_urls) if p.allowed_base_urls else None,
            format_locale=p.format_locale,
            time_format_locale=p.time_format_locale,
        ),
        True,
    ),
    ExportFormat.PNG: VlRow(
        lambda spec, p: vlc.vegalite_to_png(
            spec,
            scale=p.scale,
            ppi=p.ppi,
            theme=p.theme,
            vl_version=p.vl_version,
            show_warnings=p.show_warnings,
            allowed_base_urls=list(p.allowed_base_urls) if p.allowed_base_urls else None,
            format_locale=p.format_locale,
            time_format_locale=p.time_format_locale,
        ),
        lambda spec, p: vlc.vega_to_png(
            spec,
            scale=p.scale,
            ppi=p.ppi,
            allowed_base_urls=list(p.allowed_base_urls) if p.allowed_base_urls else None,
            format_locale=p.format_locale,
            time_format_locale=p.time_format_locale,
        ),
        False,
    ),
    ExportFormat.PDF: VlRow(
        lambda spec, p: vlc.vegalite_to_pdf(
            spec,
            scale=p.scale,
            theme=p.theme,
            vl_version=p.vl_version,
            allowed_base_urls=list(p.allowed_base_urls) if p.allowed_base_urls else None,
            format_locale=p.format_locale,
            time_format_locale=p.time_format_locale,
        ),
        lambda spec, p: vlc.vega_to_pdf(
            spec,
            scale=p.scale,
            allowed_base_urls=list(p.allowed_base_urls) if p.allowed_base_urls else None,
            format_locale=p.format_locale,
            time_format_locale=p.time_format_locale,
        ),
        False,
    ),
    # `*_to_html` accepts no `allowed_base_urls` — the browser renders the embedded spec, so the fence cannot
    # ride the call; `_rendered` refuses a fenced HTML export before this row is reached.
    ExportFormat.HTML: VlRow(
        lambda spec, p: vlc.vegalite_to_html(
            spec,
            theme=p.theme,
            vl_version=p.vl_version,
            bundle=p.bundle,
            renderer=p.renderer,
            format_locale=p.format_locale,
            time_format_locale=p.time_format_locale,
        ),
        lambda spec, p: vlc.vega_to_html(
            spec,
            bundle=p.bundle,
            renderer=p.renderer,
            format_locale=p.format_locale,
            time_format_locale=p.time_format_locale,
        ),
        True,
    ),
    ExportFormat.JPEG: VlRow(
        lambda spec, p: vlc.vegalite_to_jpeg(
            spec,
            scale=p.scale,
            quality=p.quality,
            theme=p.theme,
            vl_version=p.vl_version,
            show_warnings=p.show_warnings,
            allowed_base_urls=list(p.allowed_base_urls) if p.allowed_base_urls else None,
            format_locale=p.format_locale,
            time_format_locale=p.time_format_locale,
        ),
        lambda spec, p: vlc.vega_to_jpeg(
            spec,
            scale=p.scale,
            quality=p.quality,
            allowed_base_urls=list(p.allowed_base_urls) if p.allowed_base_urls else None,
            format_locale=p.format_locale,
            time_format_locale=p.time_format_locale,
        ),
        False,
    ),
})
LP_RENDER: Final[frozendict[ExportFormat, LpRender]] = frozendict({
    ExportFormat.SVG: lambda plot: _lp_bytes(plot.to_svg),
    ExportFormat.PNG: lambda plot: _lp_bytes(plot.to_png),
    ExportFormat.PDF: lambda plot: _lp_bytes(plot.to_pdf),
    ExportFormat.HTML: lambda plot: _lp_bytes(plot.to_html),
    ExportFormat.JPEG: lambda plot: vlc.svg_to_jpeg(_lp_bytes(plot.to_svg).decode()),
})


# --- [COMPOSITION] ----------------------------------------------------------------------
class ChartExport(Struct, frozen=True):
    chart: ChartSpec
    fmt: ExportFormat
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    lane: LanePolicy
    policy: ChartRenderPolicy = ChartRenderPolicy()
    transform: TransformPolicy = TransformPolicy()
    retention: Retention = Retention()

    def emit(self, /) -> ArtifactWork:
        admission = Admission(keyed=None) if self.chart.tag == "vega" else Admission(bare=None)
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=admission, cost=1.0)

    @property
    def _resolved(self) -> TransformPolicy:
        return self.transform if self.transform.local_tz is not None else structs.replace(self.transform, local_tz=vlc.get_local_tz() or "UTC")

    @property
    def _seed(self) -> tuple[bytes, ...]:
        framed = lambda chunk: len(chunk).to_bytes(8, "little") + chunk
        resolved = self._resolved
        match self.chart:
            case ChartSpec(tag="vega", vega=spec):
                case_bytes = _CANON.encode(spec)
            case ChartSpec(tag="lets_plot", lets_plot=(plot, palette)):
                # live palette threads into the render, so it joins the preimage — charts differing only in
                # palette key distinctly.
                case_bytes = framed(pickle.dumps(plot, protocol=5)) + framed(_CANON.encode(palette))
            case ChartSpec(tag="matplotlib", matplotlib=(figure, palette)):
                case_bytes = framed(pickle.dumps(figure, protocol=5)) + framed(_CANON.encode(palette))
            case _ as unreachable:
                assert_never(unreachable)
        # `structs.asdict` projection keeps the preimage canonical-encodable; the frames hash separately below.
        bundle = _CANON.encode((self.fmt.value, self.policy, {**structs.asdict(resolved), "inline_datasets": None}, self.retention))
        datasets = tuple(
            piece
            for name in sorted(resolved.inline_datasets)
            for piece in (framed(name.encode()), framed(resolved.inline_datasets[name].hash_rows(seed=0).to_numpy().tobytes()))
        )
        return (framed(self.chart.tag.encode()), framed(case_bytes), framed(bundle), *datasets)

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key(f"chart-{self.fmt}", self._seed)

    async def _prepared(self, spec: Spec, fmt: ExportFormat) -> RuntimeRail[PrePass]:
        return (
            await self.lane.offload(Kernel.of(_pre_transform, KernelTrait.HOSTILE), spec, fmt, self._resolved, self.retention)
        ).bind(lambda inner: inner.map_error(lambda fault: BoundaryFault(boundary=(f"chart.prepass.{fmt}", fault))))

    async def layered(self) -> RuntimeRail[tuple[Layer, ...]]:
        match self.chart:
            case ChartSpec(tag="vega", vega=spec):
                with _TRACER.start_as_current_span("chart.layers") as span:
                    span.set_attributes({"engine": self.chart.tag, "format": ExportFormat.SVG.value})
                    staged = await self._prepared(spec, ExportFormat.SVG)
                    match staged:
                        case Result(tag="ok", ok=prepass):
                            span.add_event("chart.prepass", _evidence_attrs(prepass.evidence))
                            split = await self.lane.offload(Kernel.of(_split_layers, KernelTrait.RELEASING), prepass.spec, self.policy)
                            return split.map_error(partial(_faulted, span, "chart.layers", engine=self.chart.tag))
                        case Result(tag="error", error=fault):
                            return Error(_faulted(span, "chart.layers", fault, engine=self.chart.tag))
                        case _ as unreachable:
                            assert_never(unreachable)
            case _:
                return Error(BoundaryFault(boundary=("chart.layers", "<vega-only>")))

    async def _rendered(self) -> RuntimeRail[tuple[bytes, PrePassEvidence | None]]:
        match self.chart:
            case ChartSpec(tag="vega", vega=spec):
                if self.fmt is ExportFormat.HTML and self.policy.allowed_base_urls:
                    # vl-convert's `*_to_html` defers rendering to the browser, so the SSRF fence cannot ride the
                    # call — a fenced policy refuses HTML export loudly rather than shipping an unfenced document.
                    return Error(BoundaryFault(config=("chart.export.html", "allowed-base-urls-unenforceable")))
                staged = await self._prepared(spec, self.fmt)
                match staged:
                    case Result(tag="ok", ok=prepass):
                        # pre-pass -> render is the one two-crossing stage boundary; the event timestamps it with its evidence.
                        trace.get_current_span().add_event("chart.prepass", _evidence_attrs(prepass.evidence))
                        drawn = await self.lane.offload(Kernel.of(_vl_render, KernelTrait.RELEASING), prepass.spec, self.fmt, self.policy)
                        return drawn.map(lambda data: (data, prepass.evidence))
                    case Result(tag="error", error=fault):
                        return Error(fault)
                    case _ as unreachable:
                        assert_never(unreachable)
            case ChartSpec(tag="lets_plot", lets_plot=(plot, palette)):
                drawn = await self.lane.offload(Kernel.of(_letsplot_to_bytes, KernelTrait.RELEASING), plot, palette, self.fmt)
                return drawn.map(lambda data: (data, None))
            case ChartSpec(tag="matplotlib", matplotlib=(figure, palette)):
                drawn = await self.lane.offload(Kernel.of(_matplotlib_savefig, KernelTrait.HOSTILE), figure, palette, self.fmt.value, self.policy.ppi)
                return drawn.map(lambda data: (data, None))
            case _ as unreachable:
                assert_never(unreachable)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        with _TRACER.start_as_current_span(f"chart.export.{self.fmt}") as span:
            transform = self._resolved
            span.set_attributes({
                "engine": self.chart.tag,
                "format": self.fmt.value,
                "theme": self.policy.theme or "default",
                "vega": vlc.get_vega_version(),
                "tz": transform.local_tz or "unknown",
            })
            outcome = await self._rendered()
            match outcome:
                case Result(tag="ok", ok=(data, evidence)):
                    _LOG.info(
                        "chart.export",
                        engine=self.chart.tag,
                        format=self.fmt.value,
                        bytes=len(data),
                        **(_evidence_attrs(evidence) if evidence is not None else {}),
                    )
                    return Ok(ArtifactReceipt.Chart(self._key, self.chart.tag, self.fmt.value, self.policy.scale, self.policy.theme or "default", len(data)))
                case Result(tag="error", error=fault):
                    return Error(_faulted(span, "chart.export", fault, engine=self.chart.tag, format=self.fmt.value))
                case _ as unreachable:
                    assert_never(unreachable)
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
