# [PY_ARTIFACTS_CHART_EXPORT]

The host-free render/format dispatch half of the 2D chart axis. `ChartExport` folds the `visualization/chart/spec#CHART` chart case, the typed `RenderPolicy`, and the target `ExportFormat` to bytes, contributing one `core/receipt#RECEIPT` `ArtifactReceipt.Chart`. The host-free posture is the decisive axis: vl-convert-python is the Vega/Vega-Lite engine (Rust-native, embedded V8/deno_runtime, inlined Vega sources, zero browser, SVG/PNG/PDF/HTML/JPEG/scenegraph), and lets-plot self-renders to bytes in-process (SVG/HTML self-contained, PNG/PDF through pillow, no browser, no Node, no Vega binary), so two byte-identical content-keyable producers exist with zero Chrome dependency. The `visualization/chart/transform#TRANSFORM` `VegaTransform` pre-pass server-evaluates the spec's data transforms on the runtime subprocess seam and inlines the reduced result into ONE self-contained spec the `_vl_render` arm renders — vl-convert exposes no external-dataset feed, so the pre-computed data crosses inside the spec, and the `ChartState` self-contained spec serves the interactive HTML row with no live server. `RenderPolicy` carries the full converter axis as typed policy — the raster `scale`/`ppi`/`quality`, the `VegaThemes`-typed `theme`, the pinned `vl_version`, the `format_locale`/`time_format_locale` d3-locale pair journal figures and ISO 129 drawing annotation both need, the `allowed_base_urls` SSRF fence, the HTML-row `renderer`/`bundle` — each projected onto its converter through the row's own `keys` tuple. Every native render offloads off the event loop under one `CapacityLimiter`, each offload wrapped in a `stamina` worker-death retry inside an OpenTelemetry span carrying engine/dialect/backend + the bundled `get_vega_version`/`get_vegalite_versions` actuals; the themed render bytes are a flat-SVG/raster handoff consumed by the regrouped `composition/compose#COMPOSE` placement owner, which re-renders nothing.

## [01]-[INDEX]

- [01]-[EXPORT]: host-free static-and-interactive export rows over the typed `RenderPolicy` (the full locale/SSRF/theme/renderer/bundle converter axis), the `visualization/chart/transform#TRANSFORM` server-side data pre-pass (reduce-and-inline into one self-contained spec, the `ChartState` self-contained spec for the interactive HTML row), and the per-engine `VL_RENDER`/`LP_RENDER` format tables each total over `ExportFormat`, every native render offloaded off the event loop under one `CapacityLimiter` and one `stamina` worker-death retry inside an OpenTelemetry span, contributing the typed `core/receipt#RECEIPT` `ArtifactReceipt.Chart` facts.

## [02]-[EXPORT]

- Owner: `ChartExport` the static-and-interactive export dispatch over the `visualization/chart/spec#CHART` chart case, the typed `RenderPolicy`, and the target `ExportFormat`; `ExportFormat` the closed `StrEnum` whose `row` column resolves the `VlRow` converter member and whose `interactive` column marks the HTML row; `VlRow` the one frozen `Struct` row carrying each format's vl-convert `convert` member, its `text` `str`-return flag, and its admitted-policy `keys` tuple so the per-format render axis is one module-level `VL_RENDER` table never a per-format `match` arm; `RenderPolicy` the one frozen `Struct` carrying every render knob — `scale`/`ppi`/`quality`/`theme`/`vl_version`/`fonts` plus the `format_locale`/`time_format_locale` locale pair, the `allowed_base_urls` SSRF fence, and the HTML `renderer`/`bundle` — projected onto each `VlRow.convert` call through `projected(row.keys)` so the row's own `keys` column selects exactly the parameters that converter admits (`scale`/`ppi` only on the raster rows, `quality` only on JPEG, `allowed_base_urls` only on the non-HTML rows, `renderer`/`bundle` only on HTML, the locale pair on every row), a knob being one field never a per-format render variant and never a boolean re-deriving the keep-set the row already names; the `_export_host_free` fold over the chart case IS the engine selection (the band-routing decision, not byte-emit sprawl — vega pre-transforms on the subprocess seam then renders on the worker thread, lets-plot renders on the worker thread, matplotlib renders on the subprocess seam), with vl-convert's per-format member on the `VL_RENDER` row table and lets-plot's per-format bytes producer on the `LP_RENDER` row table (each total over `ExportFormat`, the lets-plot JPEG row rasterizing its SVG through the shared vl-convert `resvg` core since lets-plot ships no `to_jpeg`) so each engine's format axis is one table not a per-format arm — no parallel engine enum, no Chrome path.
- Entry: `ChartExport.render` is `async` over the runtime `async_boundary`, returns a `RuntimeRail[ArtifactReceipt]`, and contributes the settled `core/receipt#RECEIPT` `ArtifactReceipt.Chart(key, engine, dialect, scale, theme, byte_len)` six-field fact — the engine the matched `ChartSpec.tag`, the dialect the `ExportFormat` value, the scale and theme the `RenderPolicy` knobs, the byte length the rendered output — all flat scalars the receipt owner reads through its own `_facts` arm; `_compute` pins the pre-pass `TransformPolicy.local_tz` to the render host's `vlc.get_local_tz()` when unset (so a time-axis chart is host-tz-stable), folds the chart case to its `(bytes, PrePassEvidence | None)` through `_export_host_free` keyed by `ExportFormat` and threaded with `RenderPolicy`/`TransformPolicy`/`Retention` inside one OpenTelemetry span, folds any vega `PrePassEvidence` onto the span+event, keys the content through `ContentIdentity.of`, emits one `structlog` event carrying engine/dialect/bytes+evidence, then projects the six-field `Chart` fact. Each engine offload rides one `stamina.AsyncRetryingCaller` bound to `BrokenWorkerProcess`/`BrokenWorkerInterpreter` so a transient worker death recovers before the rail surfaces an exhausted failure — the mandated resilience the vl-convert `[04]` concurrency row names.
- Growth: a new export format is one `ExportFormat` row plus one `VL_RENDER` `VlRow` entry (vega) and one `LP_RENDER` entry (lets-plot); a new render knob is one `RenderPolicy` field named in the consuming row's `keys` tuple; a new transform pre-pass mode is one `visualization/chart/transform#TRANSFORM` `VegaTransform` case plus one `apply` arm; a new host-free engine is one `visualization/chart/spec#CHART` `ChartSpec` case plus one `_export_host_free` band-routing arm carrying its own format row table; zero new surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import os
from collections.abc import Callable
from enum import StrEnum
from io import BytesIO
from typing import Final, Literal, NoReturn, assert_never

import structlog
import vl_convert as vlc
from anyio import BrokenWorkerInterpreter, BrokenWorkerProcess, CapacityLimiter, to_process, to_thread
from builtins import frozendict
from expression import Result
from msgspec import Struct, structs
from opentelemetry import trace
from stamina import AsyncRetryingCaller

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.visualization.chart.spec import ChartSpec, Palette, hex_ramp
from artifacts.visualization.chart.transform import PrePass, PrePassEvidence, Retention, TransformFault, TransformPolicy, VegaTransform

lazy import matplotlib
lazy from lets_plot import scale_color_manual, scale_fill_manual
lazy from matplotlib import colormaps, colors

# --- [TYPES] ----------------------------------------------------------------------------
# closed render domains as policy, never free strings: the 14 bundled themes (`.api/vl-convert-python.md` type [02]) + `default`, and the HTML renderer (type [03]).
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

# --- [CONSTANTS] ------------------------------------------------------------------------
_RENDER_SLOTS: Final[int] = os.process_cpu_count() or 4
_RENDER_LIMITER: Final = CapacityLimiter(_RENDER_SLOTS)
_LOG: Final = structlog.get_logger()
_TRACER: Final = trace.get_tracer(__name__)
_TRANSIENT: Final = AsyncRetryingCaller(attempts=3, timeout=30.0).on((BrokenWorkerProcess, BrokenWorkerInterpreter))


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
    fonts: tuple[str, ...] = ()
    format_locale: str | None = (
        None  # d3-format named locale (`.api/vl-convert-python.md` type [04]); localized number formatting for journal + ISO 129 annotation
    )
    time_format_locale: str | None = None  # d3-time-format named locale (type [05]); localized date/time axis formatting
    allowed_base_urls: tuple[str, ...] | None = None  # external-data SSRF fence; None allows any
    renderer: Renderer | None = None  # HTML-row renderer (svg/canvas/hybrid); None uses the converter default
    bundle: bool | None = None  # HTML-row: True inlines every dep, None uses the CDN default

    def projected(self, keys: tuple[str, ...]) -> dict[str, object]:
        # the tuple-valued `allowed_base_urls` coerces to the converter's `list[str]`; a `None` knob is dropped so the row spreads only what its converter admits
        return {key: (list(value) if isinstance(value, tuple) else value) for key in keys if (value := getattr(self, key)) is not None}


class VlRow(Struct, frozen=True):
    convert: Callable[..., str | bytes]
    text: bool
    keys: tuple[str, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------
def _register_fonts(fonts: tuple[str, ...]) -> None:
    for directory in fonts:
        vlc.register_font_directory(directory)


def _pin_version(requested: str | None) -> str:
    available = vlc.get_vegalite_versions()
    return requested if requested in available else available[-1]


def _vl_render(spec: dict[str, object], fmt: ExportFormat, policy: RenderPolicy) -> bytes:
    _register_fonts(policy.fonts)
    row = fmt.row
    pinned = structs.replace(policy, vl_version=_pin_version(policy.vl_version))
    output = row.convert(spec, **pinned.projected(row.keys))
    return output.encode() if row.text else output


def _lp_native(method: str) -> Callable[[object], bytes]:
    def render(plot: object) -> bytes:
        sink = BytesIO()
        getattr(plot, method)(sink)  # PlotSpec.to_*(file-like) writes bytes in-process and returns None
        return sink.getvalue()

    return render


def _letsplot_to_bytes(plot: object, palette: Palette, fmt: ExportFormat) -> bytes:
    ramp = hex_ramp(palette)
    themed = plot + scale_color_manual(values=ramp) + scale_fill_manual(values=ramp)
    return LP_RENDER[fmt](themed)


def _pre_transform(spec: dict[str, object], interactive: bool, policy: TransformPolicy, retention: Retention) -> Result[PrePass, TransformFault]:
    # the vegafusion pre-pass subprocess kernel: `of` decides the arm, `apply` returns the self-contained spec
    # PAIRED with the `PrePassEvidence` — the whole `Result` pickles back across the `to_process` seam.
    return VegaTransform.of(spec, interactive, policy, retention).apply(spec)


def _raise_transform(fault: TransformFault) -> NoReturn:
    # terminal collapse at the render boundary: the `<malformed-spec>` fault reconstructs the raise the
    # `async_boundary` maps into a `BoundaryFault` on the export `RuntimeRail`, never a `Result` fed to `_vl_render`.
    raise ValueError(fault)


def _evidence_attrs(evidence: PrePassEvidence) -> dict[str, object]:
    # the pre-pass evidence folded onto the export span + structlog event: the three data-loss flags made explicit,
    # the pinned tz, the server-reduced dataset count, and the `State` comm-plan client/server cardinality.
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
    # matplotlib's Agg writer covers png/pdf/svg/jpeg but NOT html, so the static figure serves the HTML slot as prolog-stripped inline SVG in a standalone shell (trusted-bytes assembly, never an f-string markup splice) — keeping the arm total over ExportFormat
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
            outcome = await _TRANSIENT(to_process.run_sync, _pre_transform, spec, fmt.interactive, transform, retention, limiter=_RENDER_LIMITER)
            prepass = outcome.default_with(
                _raise_transform
            )  # Ok -> the self-contained spec + evidence; Error("<malformed-spec>") -> the boundary raise
            data = await _TRANSIENT(to_thread.run_sync, _vl_render, prepass.spec, fmt, policy, limiter=_RENDER_LIMITER)
            return data, prepass.evidence
        case ChartSpec(tag="lets_plot", lets_plot=(plot, palette)):
            return await _TRANSIENT(to_thread.run_sync, _letsplot_to_bytes, plot, palette, fmt, limiter=_RENDER_LIMITER), None
        case ChartSpec(tag="matplotlib", matplotlib=(figure, palette)):
            return await _TRANSIENT(to_process.run_sync, _matplotlib_savefig, figure, palette, fmt.value, policy.ppi, limiter=_RENDER_LIMITER), None
        case _:
            assert_never(chart)


# --- [TABLES] ---------------------------------------------------------------------------
# each `keys` names ONLY the converter's real parameters: `pdf`/`svg`/`html` drop `ppi`, only `jpeg` carries `quality`, HTML alone carries `renderer`/`bundle` and drops `allowed_base_urls`, the locale pair rides every row (`.api/vl-convert-python.md` rows [01]-[05]).
VL_RENDER: Final[frozendict[ExportFormat, VlRow]] = frozendict({
    ExportFormat.SVG: VlRow(vlc.vegalite_to_svg, True, ("theme", "vl_version", "allowed_base_urls", "format_locale", "time_format_locale")),
    ExportFormat.PNG: VlRow(
        vlc.vegalite_to_png, False, ("scale", "ppi", "theme", "vl_version", "allowed_base_urls", "format_locale", "time_format_locale")
    ),
    ExportFormat.PDF: VlRow(vlc.vegalite_to_pdf, False, ("scale", "theme", "vl_version", "allowed_base_urls", "format_locale", "time_format_locale")),
    ExportFormat.HTML: VlRow(vlc.vegalite_to_html, True, ("theme", "vl_version", "bundle", "renderer", "format_locale", "time_format_locale")),
    ExportFormat.JPEG: VlRow(
        vlc.vegalite_to_jpeg, False, ("scale", "quality", "theme", "vl_version", "allowed_base_urls", "format_locale", "time_format_locale")
    ),
})


# total over `ExportFormat`: lets-plot ships no `to_jpeg`, so JPEG rasterizes the lets-plot SVG through the shared
# vl-convert `resvg` core (`svg_to_jpeg`) — every format has a row, never a missing-key KeyError.
LP_RENDER: Final[frozendict[ExportFormat, Callable[[object], bytes]]] = frozendict({
    ExportFormat.SVG: _lp_native("to_svg"),
    ExportFormat.PNG: _lp_native("to_png"),
    ExportFormat.PDF: _lp_native("to_pdf"),
    ExportFormat.HTML: _lp_native("to_html"),
    ExportFormat.JPEG: lambda plot: vlc.svg_to_jpeg(_lp_native("to_svg")(plot).decode()),
})


# --- [COMPOSITION] ----------------------------------------------------------------------
class ChartExport(Struct, frozen=True):
    chart: ChartSpec
    fmt: ExportFormat
    policy: RenderPolicy = RenderPolicy()
    transform: TransformPolicy = (
        TransformPolicy()
    )  # the vegafusion pre-pass policy threaded into the subprocess kernel (row_limit/local_tz/inline_datasets/caps)
    retention: Retention = Retention()  # the Inline interactivity-retention policy (preserve_interactivity/keep_signals/keep_datasets)

    async def render(self) -> RuntimeRail[ArtifactReceipt]:
        return await async_boundary(f"chart.export.{self.fmt}", self._compute)

    async def _compute(self) -> ArtifactReceipt:
        with _TRACER.start_as_current_span(f"chart.export.{self.fmt}") as span:
            # vl-convert exposes NO `local_tz=` render override, so a time-axis chart content-keys per host UNLESS the
            # vegafusion pre-pass evaluates its time transforms under a PINNED tz: default the pre-pass `local_tz` to the
            # render host's `vlc.get_local_tz()` when the caller left it unset, so the reduced spec is host-tz-stable.
            transform = (
                self.transform if self.transform.local_tz is not None else structs.replace(self.transform, local_tz=vlc.get_local_tz() or "UTC")
            )
            span.set_attributes({
                "engine": self.chart.tag,
                "dialect": self.fmt.value,
                "theme": self.policy.theme,
                "vega": vlc.get_vega_version(),
                "tz": transform.local_tz or "unknown",
            })
            data, evidence = await _export_host_free(self.chart, self.fmt, self.policy, transform, self.retention)
            if evidence is not None:  # the vega arm's pre-pass evidence: data-loss flags + tz + dataset/comm-plan cardinality onto the span
                span.set_attributes(_evidence_attrs(evidence))
        key = ContentIdentity.of(f"chart-{self.fmt}", data)
        _LOG.info(
            "chart.export",
            engine=self.chart.tag,
            dialect=self.fmt.value,
            bytes=len(data),
            **(_evidence_attrs(evidence) if evidence is not None else {}),
        )
        return ArtifactReceipt.Chart(key, self.chart.tag, self.fmt.value, self.policy.scale, self.policy.theme, len(data))
```

`_export_host_free` IS the engine selection — the band-routing decision over the three chart cases, not byte-emit sprawl. The vega arm threads `_pre_transform` (with the tz-pinned `TransformPolicy` + the `Retention`) onto `anyio.to_process.run_sync` so the gated `visualization/chart/transform#TRANSFORM` `VegaTransform` pre-pass runs on the subprocess seam and returns ONE `Result[PrePass, TransformFault]` — the self-contained spec PAIRED with the `PrePassEvidence`, pickled back across the seam; `outcome.default_with(_raise_transform)` collapses a `<malformed-spec>` fault to the boundary raise the `async_boundary` rails, `_vl_render` renders `PrePass.spec` on the `to_thread` worker under the shared `CapacityLimiter`, and `_evidence_attrs` folds the data-loss flags / pinned tz / dataset+comm-plan cardinality onto the span and structlog event; lets-plot renders on the same worker lane; matplotlib renders on the subprocess seam. Each offload rides the module-level `_TRANSIENT` `stamina.AsyncRetryingCaller` bound to the two worker-death types, so a transient `BrokenWorkerProcess`/`BrokenWorkerInterpreter` re-drives the render before the rail surfaces an exhausted failure. Each engine's format axis is one row table total over `ExportFormat` — `VL_RENDER` keying the converter member, its `text` flag, and its admitted-policy `keys` (each `keys` tuple naming only parameters the converter actually accepts, so `pdf`/`jpeg` never pass a `ppi` the converter rejects and HTML alone carries `renderer`/`bundle`), `LP_RENDER` keying the per-format bytes producer (the JPEG row routing the lets-plot SVG through `vlc.svg_to_jpeg`) — so a new output format is one row, never a per-format `match` arm, and `RenderPolicy.projected(row.keys)` spreads exactly the converter params the row names, coercing the tuple-valued `allowed_base_urls` to the converter's `list[str]` and dropping every `None` knob.

## [03]-[RESEARCH]

- [VL_SELF_CONTAINED_SPEC] [RESOLVED]: vl-convert exposes no external-dataset feed — the `vegalite_to_*` signatures (`.api/vl-convert-python.md` rows [01]-[05], confirmed by reflection of `vl_convert 1.9.0.post1`) carry NO `inline_datasets` parameter, so the prior Arrow-IPC frame-map threading was a phantom kwarg that `TypeError`s. The server-side data reduction crosses INSIDE the spec: `visualization/chart/transform#TRANSFORM` `pre_transform_spec` evaluates the transforms and inlines the reduced result into one self-contained spec `_vl_render` renders, and `ChartState.get_transformed_spec` yields the self-contained interactive spec for the HTML row. Each `VL_RENDER` `keys` tuple names only the converter's real parameters, so `projected(row.keys)` never spreads a phantom kwarg.
- [RENDER_KNOBS] [RESOLVED]: the render domains are typed policy, never free strings — `theme` is the `VegaThemes` 14-name + `default` `Literal` (verified `alt`-independent against `.api/vl-convert-python.md` type [02] and the live `get_themes()` key set), `renderer` the `svg`/`canvas`/`hybrid` `Literal` (type [03]), and `format_locale`/`time_format_locale` the d3-format/d3-time-format named-locale strings (types [04]/[05], verified `get_format_locale`/`get_time_format_locale` present) journal figures and ISO 129 drawing annotation localize through; `allowed_base_urls` is the SSRF fence (render rows) and `bundle` the HTML inline-vs-CDN switch. Each rides ONLY the rows whose converter admits it — the SVG/PNG/PDF/JPEG rows carry `allowed_base_urls` and the locale pair, PNG/JPEG add `scale`+`ppi`/`quality`, HTML carries `bundle`/`renderer` and drops `allowed_base_urls` — verified against the per-function signatures, so the `pdf`/`jpeg`-drops-`ppi` discipline holds and no row spreads a parameter its converter rejects.
- [VL_RENDER_OFFLOAD] [RESOLVED]: the vl-convert and lets-plot native renders ride `to_thread.run_sync(..., limiter=_RENDER_LIMITER)` (the Rust/V8 and pillow cores release the GIL, the worker shares the in-process spec zero-copy), the vegafusion pre-pass and matplotlib ride `to_process.run_sync` (the gated subprocess seam), so no heavy native render runs inline on the event loop; one `CapacityLimiter` bounds the whole render subsystem and one `stamina.AsyncRetryingCaller` (`.api/stamina.md` caller row [02], verified) wraps every offload — the mandated bounded worker-death retry the vl-convert `[04]` concurrency row names — inside one OpenTelemetry span (`.api/opentelemetry-api.md` `start_as_current_span`) and one `structlog` event (`.api/structlog.md`) carrying engine/dialect/backend + the bundled `get_vega_version()` actual + the tz the pre-pass PINS: vl-convert exposes NO `local_tz=` render override (entry [10]), so a time-axis chart content-keys per host UNLESS the `visualization/chart/transform#TRANSFORM` pre-pass evaluates its time transforms under a fixed tz — `_compute` defaults the pre-pass `TransformPolicy.local_tz` to the render host's `vlc.get_local_tz()` when the caller left it unset, so the reduced spec is host-tz-stable and the pinned tz rides the span. The vega arm's `_pre_transform` returns `Result[PrePass, TransformFault]` (the self-contained spec + the `PrePassEvidence` `warnings`/`row_limit_exceeded`/`interactivity_broken`/`unsupported`/dataset+comm-plan cardinality) that pickles back across the `to_process` seam: `default_with(_raise_transform)` rails a `<malformed-spec>` and `_evidence_attrs` folds the evidence onto the span+event — the evidence weave the vl-convert/altair `[04]` rows mandate, and the prior fence, which fed the pre-pass through as a bare dict into `_vl_render`, both omitted the evidence AND crashed once `apply` returned a `Result`.
- [MATPLOTLIB_SAVE] [RESOLVED]: the `_matplotlib_savefig` arm deepens beyond `use('Agg')` + `ListedColormap` + `set_prop_cycle` + `savefig` — `Figure.set_layout_engine('constrained')` auto-fits axes spacing as a figure policy (`.api/matplotlib.md` entry [05], verified), and `savefig(metadata=, bbox_inches='tight', pad_inches=)` seals the document metadata and tight crop (entry [06]) — while `FigureCanvasAgg.buffer_rgba()` (type [07]) is the numpy zero-copy RGBA handoff to `graphic/raster`, `backend_pdf.PdfPages` (type [08]) the multi-page PDF sink `package/archive` consumes for a figure set, and `ticker.EngFormatter` (type [03]) the SI-prefix AEC-drawing-scale label an authored `Figure` axis carries — the save-richness the prior thin arm ignored, verified present on `matplotlib 3.11.0`. The Agg writer's supported set (`png`/`pdf`/`svg`/`jpeg`/`webp`/... but NOT `html`, `.api/matplotlib.md` save axis) is why the arm renders SVG for the interactive HTML slot and byte-assembles the prolog-stripped `<svg>` into a standalone HTML shell (trusted matplotlib output, never an f-string markup splice), keeping `_matplotlib_savefig` total over `ExportFormat` rather than raising an opaque `savefig(format="html")` `ValueError` on the reachable matplotlib×HTML combination.
- [CHART_RECEIPT_FACTS] [RESOLVED]: `ChartExport._compute` projects `ArtifactReceipt.Chart(key, self.chart.tag, self.fmt.value, self.policy.scale, self.policy.theme, len(data))` against the settled `core/receipt#RECEIPT` `Chart(key, engine, dialect, scale, theme, byte_len)` six-field constructor (verified against `receipt.md` `_KEYS['chart'] == ("engine", "dialect", "scale", "theme", "bytes")`), so the receipt call is settled fence code with zero contradiction — engine the matched `ChartSpec.tag`, dialect the `ExportFormat` value, scale/theme the `RenderPolicy` knobs, byte length the rendered output, all flat scalars, never a new receipt case or a producer value object the receipt owner would import.
- [DROP_PLOTLY_KALEIDO] [RESOLVED]: the `ChartSpec.Plotly` case, the `_plotly_via_chrome` arm, and the plotly fold arm are deleted — vl-convert renders only Vega/Vega-Lite (never plotly.js), so every plotly static export is reachable only through kaleido's headless Chromium the host-free posture forbids as default. The `ARCHITECTURE.md` `[02]-[SEAMS]` carries no cross-folder plotly-JSON consumer, so the deletion is seam-clean; the host-free interactivity the plotly removal drops is recovered by the `ExportFormat.HTML` `vegalite_to_html` row (with the `renderer`/`bundle` knobs) plus the lets-plot `to_html` arm, so no interactive capability is lost. The companion `README`/`pyproject`/`.api` plotly/kaleido rows ripple to those owners as a counterpart drop.
