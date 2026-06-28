# [PY_ARTIFACTS_CHART_EXPORT]

The host-free render/format dispatch half of the 2D chart axis. `ChartExport` folds the `visualization/chart/spec#CHART` chart case, the typed `RenderPolicy`, and the target `ExportFormat` to bytes, contributing one `core/receipt#RECEIPT` `ArtifactReceipt.Chart`. The host-free posture is the decisive axis: vl-convert-python is the Vega/Vega-Lite engine (Rust-native, embedded V8/deno_runtime, inlined Vega sources, zero browser, SVG/PNG/PDF/HTML/JPEG/scenegraph), and lets-plot self-renders to bytes in-process (SVG/HTML self-contained, PNG/PDF through pillow, no browser, no Node, no Vega binary), so two byte-identical content-keyable producers exist with zero Chrome dependency. The `visualization/chart/transform#TRANSFORM` `VegaTransform` pre-pass server-evaluates the spec's data transforms on the runtime subprocess seam and inlines the reduced result into ONE self-contained spec the `_vl_render` arm renders — vl-convert exposes no external-dataset feed, so the pre-computed data crosses inside the spec, and the `ChartState` self-contained spec serves the interactive HTML row with no live server. Every native render offloads off the event loop: vl-convert and lets-plot ride `to_thread` (GIL-releasing native, zero-copy of the spec the worker shares) under one `CapacityLimiter`, matplotlib and the vegafusion pre-pass ride `to_process` (matplotlib never resolves in the runtime process; vegafusion crosses the subprocess seam). The themed render bytes are a flat-SVG/raster handoff consumed by the regrouped `composition/compose#COMPOSE` placement owner, which lays the chart out beside its siblings and re-renders nothing.

## [01]-[INDEX]

- [01]-[EXPORT]: host-free static-and-interactive export rows over the typed `RenderPolicy`, the `visualization/chart/transform#TRANSFORM` server-side data pre-pass (reduce-and-inline into one self-contained spec, the `ChartState` self-contained spec for the interactive HTML row), and the per-engine `VL_RENDER`/`LP_RENDER` format tables each total over `ExportFormat`, every native render offloaded off the event loop, contributing the typed `core/receipt#RECEIPT` `ArtifactReceipt.Chart` facts.

## [02]-[EXPORT]

- Owner: `ChartExport` the static-and-interactive export dispatch over the `visualization/chart/spec#CHART` chart case, the typed `RenderPolicy`, and the target `ExportFormat`; `ExportFormat` the closed `StrEnum` whose `row` column resolves the `VlRow` converter member and whose `interactive` column marks the HTML row; `VlRow` the one frozen `Struct` row carrying each format's vl-convert `convert` member, its `text` `str`-return flag, and its admitted-policy `keys` tuple so the per-format render axis is one module-level `VL_RENDER` table never a per-format `match` arm; `RenderPolicy` the one frozen `Struct` carrying the `scale`/`ppi`/`theme`/`vl_version` render knobs plus the `fonts` directory tuple, projected onto each `VlRow.convert` call through `projected(row.keys)` so the row's own `keys` column selects exactly the parameters the converter admits — `scale`/`ppi`/`theme`/`vl_version` on the raster rows, `theme`/`vl_version` on the SVG/HTML rows — a knob being one field never a per-format render variant and never a boolean re-deriving the keep-set the row already names; the `_export_host_free` fold over the chart case IS the engine selection (the band-routing decision, not byte-emit sprawl — vega pre-transforms on the subprocess seam then renders on the worker thread, lets-plot renders on the worker thread, matplotlib renders on the subprocess seam), with vl-convert's per-format member on the `VL_RENDER` row table and lets-plot's per-format bytes producer on the `LP_RENDER` row table (each total over `ExportFormat`, the lets-plot JPEG row rasterizing its SVG through the shared vl-convert resvg core since lets-plot ships no `to_jpeg`) so each engine's format axis is one table not a per-format arm — no parallel engine enum, no Chrome path.
- Entry: `ChartExport.render` is `async` over the runtime `async_boundary`, returns a `RuntimeRail[ArtifactReceipt]`, and contributes the settled `core/receipt#RECEIPT` `ArtifactReceipt.Chart(key, engine, dialect, scale, theme, byte_len)` six-field fact — the engine the matched `ChartSpec.tag` (no parallel engine enum), the dialect the `ExportFormat` value, the scale and theme the `RenderPolicy` knobs, the byte length the rendered output — all flat scalars the receipt owner reads through its own `_facts` arm with no producer value object imported; `_compute` folds the chart case to its bytes through `_export_host_free` keyed by `ExportFormat` and threaded with `RenderPolicy`, keys the content through `ContentIdentity.of`, then projects the six-field `Chart` fact — vega through the gated `visualization/chart/transform#TRANSFORM` `VegaTransform.apply` pre-pass on the subprocess seam returning ONE self-contained spec, followed by the host-free `_vl_render` `VL_RENDER`-keyed vl-convert render on the worker thread, lets-plot through the `_letsplot_to_bytes` `LP_RENDER`-keyed acceptor on the worker thread, matplotlib through the `_matplotlib_savefig` worker awaited across the subprocess seam.
- Growth: a new export format is one `ExportFormat` row plus one `VL_RENDER` `VlRow` entry (vega) and one `LP_RENDER` entry (lets-plot); a new render knob is one `RenderPolicy` field named in the consuming row's `keys` tuple; a new transform pre-pass mode is one `visualization/chart/transform#TRANSFORM` `VegaTransform` case plus one `apply` arm; a new host-free engine is one `visualization/chart/spec#CHART` `ChartSpec` case plus one `_export_host_free` band-routing arm carrying its own format row table; zero new surface.

```python signature
import os
from collections.abc import Callable
from enum import StrEnum
from io import BytesIO
from types import MappingProxyType
from typing import Final, assert_never

import vl_convert as vlc
from anyio import CapacityLimiter, to_process, to_thread
from msgspec import Struct, structs

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.visualization.chart.spec import ChartSpec, Palette, hex_ramp
from artifacts.visualization.chart.transform import VegaTransform

lazy import matplotlib
lazy from lets_plot import scale_color_manual, scale_fill_manual
lazy from matplotlib import colormaps, colors

_RENDER_SLOTS: Final[int] = os.process_cpu_count() or 4
_RENDER_LIMITER: Final = CapacityLimiter(_RENDER_SLOTS)


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
    theme: str = "default"
    vl_version: str | None = None
    fonts: tuple[str, ...] = ()

    def projected(self, keys: tuple[str, ...]) -> dict[str, object]:
        return {key: value for key in keys if (value := getattr(self, key)) is not None}


class VlRow(Struct, frozen=True):
    convert: Callable[..., str | bytes]
    text: bool
    keys: tuple[str, ...]


VL_RENDER: Final[MappingProxyType[ExportFormat, VlRow]] = MappingProxyType({
    ExportFormat.SVG: VlRow(vlc.vegalite_to_svg, True, ("theme", "vl_version")),
    ExportFormat.PNG: VlRow(vlc.vegalite_to_png, False, ("scale", "ppi", "theme", "vl_version")),
    ExportFormat.PDF: VlRow(vlc.vegalite_to_pdf, False, ("scale", "theme", "vl_version")),
    ExportFormat.HTML: VlRow(vlc.vegalite_to_html, True, ("theme", "vl_version")),
    ExportFormat.JPEG: VlRow(vlc.vegalite_to_jpeg, False, ("scale", "quality", "theme", "vl_version")),
})


def _register_fonts(fonts: tuple[str, ...]) -> None:
    for directory in fonts:
        vlc.register_font_directory(directory)


def _pin_version(requested: str | None) -> str:
    available = vlc.get_vegalite_versions()
    return requested if requested in available else available[-1]


class ChartExport(Struct, frozen=True):
    chart: ChartSpec
    fmt: ExportFormat
    policy: RenderPolicy = RenderPolicy()

    async def render(self) -> RuntimeRail[ArtifactReceipt]:
        return await async_boundary(f"chart.export.{self.fmt}", self._compute)

    async def _compute(self) -> ArtifactReceipt:
        data = await _export_host_free(self.chart, self.fmt, self.policy)
        key = ContentIdentity.of(f"chart-{self.fmt}", data)
        return ArtifactReceipt.Chart(key, self.chart.tag, self.fmt.value, self.policy.scale, self.policy.theme, len(data))


async def _export_host_free(chart: ChartSpec, fmt: ExportFormat, policy: RenderPolicy) -> bytes:
    match chart:
        case ChartSpec(tag="vega", vega=spec):
            reduced = await to_process.run_sync(_pre_transform, spec, fmt.interactive, limiter=_RENDER_LIMITER)
            return await to_thread.run_sync(_vl_render, reduced, fmt, policy, limiter=_RENDER_LIMITER)
        case ChartSpec(tag="lets_plot", lets_plot=(plot, palette)):
            return await to_thread.run_sync(_letsplot_to_bytes, plot, palette, fmt, limiter=_RENDER_LIMITER)
        case ChartSpec(tag="matplotlib", matplotlib=(figure, palette)):
            return await to_process.run_sync(_matplotlib_savefig, figure, palette, fmt.value, policy.ppi, limiter=_RENDER_LIMITER)
        case _:
            assert_never(chart)


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


# total over `ExportFormat`: lets-plot ships no `to_jpeg`, so JPEG rasterizes the lets-plot SVG through
# the shared vl-convert `resvg` core — every format has a row, never a missing-key KeyError.
LP_RENDER: Final[MappingProxyType[ExportFormat, Callable[[object], bytes]]] = MappingProxyType({
    ExportFormat.SVG: _lp_native("to_svg"),
    ExportFormat.PNG: _lp_native("to_png"),
    ExportFormat.PDF: _lp_native("to_pdf"),
    ExportFormat.HTML: _lp_native("to_html"),
    ExportFormat.JPEG: lambda plot: vlc.svg_to_jpeg(_lp_native("to_svg")(plot).decode()),
})


def _letsplot_to_bytes(plot: object, palette: Palette, fmt: ExportFormat) -> bytes:
    ramp = hex_ramp(palette)
    themed = plot + scale_color_manual(values=ramp) + scale_fill_manual(values=ramp)
    return LP_RENDER[fmt](themed)


def _pre_transform(spec: dict[str, object], interactive: bool) -> dict[str, object]:
    return VegaTransform.of(spec, interactive).apply(spec)


def _matplotlib_savefig(figure: object, palette: Palette, fmt: str, ppi: float) -> bytes:
    matplotlib.use("Agg")
    colormaps.register(colors.ListedColormap(palette, name=f"chart-{id(figure):x}"), force=True)
    for axes in figure.axes:
        axes.set_prop_cycle(color=hex_ramp(palette))
    sink = BytesIO()
    figure.savefig(sink, format=fmt, dpi=ppi)
    return sink.getvalue()
```

`_export_host_free` IS the engine selection — the band-routing decision over the three chart cases, not byte-emit sprawl. The vega arm threads `_pre_transform` onto `anyio.to_process.run_sync` so the gated `visualization/chart/transform#TRANSFORM` `VegaTransform` pre-pass runs on the subprocess seam and returns ONE self-contained spec, then `_vl_render` renders that spec on the `to_thread` worker under the shared `CapacityLimiter`; lets-plot renders on the same worker lane; matplotlib renders on the subprocess seam. Each engine's format axis is one row table total over `ExportFormat` — `VL_RENDER` keying the converter member, its `text` flag, and its admitted-policy `keys` (each `keys` tuple naming only parameters the converter actually accepts, so `pdf`/`jpeg` never pass a `ppi` the converter rejects), `LP_RENDER` keying the per-format bytes producer (the JPEG row routing the lets-plot SVG through `vlc.svg_to_jpeg` since lets-plot ships no `to_jpeg`) — so a new output format is one row, never a per-format `match` arm, and `RenderPolicy.projected(row.keys)` spreads exactly the converter params the row names.

## [03]-[RESEARCH]

- [VL_SELF_CONTAINED_SPEC] [RESOLVED]: vl-convert exposes no external-dataset feed — the `vegalite_to_*` signatures (`.api/vl-convert-python.md` rows [01]-[05], confirmed by signature reflection of `vl_convert 1.9.0.post1`: `vegalite_to_svg(vl_spec, vl_version, config, theme, show_warnings, allowed_base_urls, format_locale, time_format_locale)` and siblings) carry NO `inline_datasets` parameter, so the prior `inline_datasets=` Arrow-IPC frame-map threading was a phantom kwarg that `TypeError`s. The server-side data reduction crosses INSIDE the spec: `visualization/chart/transform#TRANSFORM` `pre_transform_spec` evaluates the transforms and inlines the reduced result into one self-contained spec `_vl_render` renders, and `ChartState.get_transformed_spec` yields the self-contained interactive spec for the HTML row — the verified `altair -> vegafusion -> vl-convert` flow both `.api` catalogues document. Each `VL_RENDER` `keys` tuple names only the converter's real parameters: `pdf`/`jpeg` drop the `ppi` those signatures do not accept and `jpeg` carries `quality` (the `RenderPolicy.quality` knob), so `projected(row.keys)` never spreads a phantom kwarg.
- [VL_RENDER_OFFLOAD] [RESOLVED]: the vl-convert and lets-plot native renders ride `to_thread.run_sync(..., limiter=_RENDER_LIMITER)` (the Rust/V8 and pillow cores release the GIL, the worker shares the in-process spec with zero serialization), the vegafusion pre-pass and matplotlib ride `to_process.run_sync` (the gated subprocess seam), so no heavy native render runs inline on the event loop; one `CapacityLimiter` bounds the whole render subsystem.
- [CHART_RECEIPT_FACTS] [RESOLVED]: `ChartExport._compute` projects `ArtifactReceipt.Chart(key, self.chart.tag, self.fmt.value, self.policy.scale, self.policy.theme, len(data))` against the widened `core/receipt#RECEIPT` `Chart(key, engine, dialect, scale, theme, byte_len)` six-field constructor whose `_facts` arm projects `{"key", "engine", "dialect", "scale", "theme", "bytes"}`, so the receipt call is settled fence code with zero contradiction. The widening is all flat scalars — engine the matched `ChartSpec.tag` (no parallel engine enum), dialect the `ExportFormat` value, scale and theme the `RenderPolicy` knobs, byte length the rendered output — a field widening on the existing `chart` case, never a new receipt case, never a parallel chart-receipt owner, and never a producer value object the receipt owner would import. The `ChartExport.render -> RuntimeRail[ArtifactReceipt]` rail shape mirrors the settled `graphic/raster#RASTER` `Raster.of -> RuntimeRail[ArtifactReceipt]` pattern.
- [DROP_PLOTLY_KALEIDO]: the `ChartSpec.Plotly` case, the `_plotly_via_chrome` arm, and the `_export_host_free` plotly fold arm are deleted — vl-convert renders only Vega/Vega-Lite (never plotly.js), so every plotly static export is reachable only through kaleido's headless Chromium `get_chrome_sync`/`calc_fig_sync`, the degraded host-Chrome path the host-free posture forbids as default. The `ARCHITECTURE.md` `[02]-[SEAMS]` carries no cross-folder consumer of plotly JSON, so the deletion is seam-clean. The companion `README` `[Charts]` plotly/kaleido rows, the `pyproject.toml` plotly/kaleido manifest rows, and the `.api/plotly.md`/`.api/kaleido.md` catalogues are out of this page's edit scope and ripple to the `README`/manifest/`ARCHITECTURE` owners as a counterpart drop; the host-free interactivity the plotly removal drops is recovered by the `ExportFormat.HTML` `vegalite_to_html` row plus the lets-plot `to_html` arm, so no interactive capability is lost.
