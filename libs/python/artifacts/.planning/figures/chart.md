# [PY_ARTIFACTS_CHART]

The 2D chart-to-render axis. `ChartSpec` is ONE tagged union collapsing the host-free 2D engines — declarative-Vega (altair Vega-Lite), declarative-grammar (lets-plot grammar-of-graphics), and publication (matplotlib) — each case carrying its own palette-threaded payload, discriminated by one total `match` over the single `ChartEngineTag` literal with no parallel engine enum; `ChartExport` folds the chart case, the typed `RenderPolicy`, and the target `ExportFormat` to bytes, contributing one `receipt/receipt#RECEIPT` `ArtifactReceipt.Chart`. The host-free posture is the decisive axis: vl-convert-python is the Vega/Vega-Lite engine (Rust-native, embedded V8/deno_runtime, inlined Vega sources, zero browser, SVG/PNG/PDF/HTML/JPEG/scenegraph), and lets-plot self-renders to bytes entirely in-process (bundled ImageMagick, no browser, no Node, no Vega binary), so two byte-identical content-keyable producers exist with zero Chrome dependency. The altair `Chart` builder owns the Vega-Lite grammar end-to-end — `mark_*` geometry, `encode` binding the typed `Color`/`Size`/`Shape`/`Tooltip` channels, `transform_*` data chains, `layer`/`hconcat`/`vconcat`/`concat`/`facet` composition into the `LayerChart`/`ConcatChart`/`FacetChart` roots, and `interactive`/`selection_point`/`selection_interval` selections — so the page never hand-builds a Vega-Lite dict where the builder is admitted. vegafusion is the server-side Vega transform pre-pass executing a spec's DataFusion-backed data transforms before vl-convert render — inline-JSON for small specs, Arrow-IPC `inline_datasets` for large polars frames so megabytes never inline as JSON, `ChartState.get_transformed_spec` yielding one fully-transformed self-contained spec for the interactive HTML row so no live server is needed — gated `python_version<'3.15'` and crossing the runtime subprocess seam. matplotlib and lets-plot are gated `python_version<'3.15'`: matplotlib never resolves in the cp315-core process and renders on the runtime subprocess seam, while lets-plot rides the gated band in-process because `PlotSpec.to_*` is pure-Python over bundled ImageMagick. Color arrives from `figures/color#COLOR` as the `ColorReceipt.coords` palette array, threaded into every engine theme — never picked ad hoc per engine.

## [01]-[INDEX]

- [01]-[CHART]: the 2D `ChartSpec` union over the host-free charting engines, each case threading the `figures/color#COLOR` palette array through the one `ChartSpec.of` composer keyed on the admitted altair builder roots — the altair `Chart` builder mined to its mark/encode/transform/composition depth on the Vega arm.
- [02]-[EXPORT]: host-free static-and-interactive export rows over the typed `RenderPolicy`, the vegafusion inline-JSON-or-Arrow-IPC transform pre-pass, and the `ChartState` interactive-HTML server/client split, contributing the typed `ArtifactReceipt.Chart` facts.

## [02]-[CHART]

- Owner: `ChartSpec` the one chart axis, a frozen `tagged_union` whose `tag` carries the closed `ChartEngineTag` literal and whose every case carries its own palette-threaded payload — never a second `ChartEngine` enum re-describing the discriminant the union already owns. `ChartSpec.of` is the one composer threading the `figures/color#COLOR` `ColorReceipt.coords` palette array into the case through one total `match` over `tag`, so the single-color-source invariant is real for charts — the host-free Vega arm folds the altair `Chart` to its Vega-Lite dict through `to_dict`, composes `Chart.interactive()` so the single-view spec carries pan/zoom selections the HTML row renders natively, binds the palette as the discrete `config.range.category` scale plus the `Color`-channel `Scale(range=...)` on the cp315 core, and the gated `lets_plot`/`matplotlib` cases carry the raw engine object plus the `Palette` array so the palette-thread runs on the gated band beside its render (`scale_color_manual`/`scale_fill_manual` for lets-plot, `colors.ListedColormap`/`colormaps.register` plus `Axes.set_prop_cycle` for matplotlib), never an ad-hoc per-engine color pick and never a gated import on the core.
- Cases: `ChartSpec` cases `Vega(spec)` (an altair `Chart`/`LayerChart`/`ConcatChart`/`FacetChart` with its full `mark_*`/`encode`/`transform_*`/`interactive`/`selection_*` grammar, or a raw Vega-Lite dict, folded to the palette-themed Vega-Lite spec dict the export axis consumes directly) · `LetsPlot(plot, palette)` (a lets-plot `PlotSpec` plus its `Palette` array self-rendering to bytes in-process on the gated band) · `Matplotlib(figure, palette)` (a matplotlib `Figure` plus its `Palette` array rendered on the gated subprocess seam) — matched by one total `match`/`case` over `tag`, the plotly/kaleido Chrome-gated case deleted as the host-free posture forbids it as default and vl-convert renders only Vega/Vega-Lite, never plotly.js.
- Entry: `ChartSpec.of(engine, palette)` is the one composer — `isinstance` over the admitted altair builder roots folds the Vega arm, the gated engine objects fold the `LetsPlot`/`Matplotlib` arms; the palette-themed result feeds `EXPORT` `ChartExport.render`. No `ChartSpec.compose` forwarding hop and no parallel `ChartEngine.compose` fold survive — one composer, one discriminant.
- Packages: `altair` (`Chart`/`LayerChart`/`HConcatChart`/`VConcatChart`/`ConcatChart`/`FacetChart`; `encode`/`interactive`/`to_dict` the composed builder family, `mark_*`/`transform_*`/`properties` the growth-axis builder members; the `Color` channel, `Scale`/`Legend` guides composed in `_theme_vega`, the `X`/`Y`/`Size`/`Shape`/`Tooltip` channels and `selection_point`/`selection_interval`/`condition` selections and `layer`/`hconcat`/`vconcat`/`concat` composition operators on the caller-built grammar) on the cp315 core; `lets-plot` (`LetsPlot`/`ggplot`/`PlotSpec`, `scale_color_manual`/`scale_fill_manual`) and `matplotlib` (`Figure`/`colors.ListedColormap`/`colormaps.register`/`Axes.set_prop_cycle`) gated `python_version<'3.15'`; the palette-themed interactive Vega-Lite spec dict feeds `EXPORT` where `vegafusion` pre-transforms it; runtime; `figures/color#COLOR` (`ColorReceipt.coords`).
- Growth: a new host-free 2D engine is one `ChartSpec` case plus one `ChartSpec.of` match arm carrying its palette-thread, plus one `_export_host_free` band-routing arm carrying its own per-format row table; zero new surface. A new altair channel, mark, transform, composition operator, or selection is a builder call inside the Vega arm, never a new owner.
- Boundary: no live dashboard, UI event state, or browser runtime; no Chrome-gated engine — the plotly graph_objects case and the kaleido `get_chrome_sync`/`calc_fig_sync` figure-to-bytes path are the deleted forms, since every plotly static export is reachable only through kaleido's headless Chromium, the anti-axis the host-free posture forbids; 3D scientific scenes ride `figures/scene#SCENE`, not this axis; color palettes arrive from `figures/color#COLOR` as `ColorReceipt.coords`, never picked ad hoc per engine.

```python signature
from typing import Literal, assert_never

import altair as alt
import numpy as np
from expression import case, tag, tagged_union
from numpy.typing import NDArray

type Palette = NDArray[np.float64]
type ChartEngineTag = Literal["vega", "lets_plot", "matplotlib"]
type VegaBuilder = alt.Chart | alt.LayerChart | alt.HConcatChart | alt.VConcatChart | alt.ConcatChart | alt.FacetChart


@tagged_union(frozen=True)
class ChartSpec:
    tag: ChartEngineTag = tag()
    vega: dict[str, object] = case()
    lets_plot: tuple[object, Palette] = case()
    matplotlib: tuple[object, Palette] = case()

    @staticmethod
    def Vega(spec: dict[str, object]) -> "ChartSpec":
        return ChartSpec(vega=spec)

    @staticmethod
    def LetsPlot(plot: object, palette: Palette) -> "ChartSpec":
        return ChartSpec(lets_plot=(plot, palette))

    @staticmethod
    def Matplotlib(figure: object, palette: Palette) -> "ChartSpec":
        return ChartSpec(matplotlib=(figure, palette))

    @staticmethod
    def of(engine: object, palette: Palette) -> "ChartSpec":
        match engine:
            case alt.Chart() | alt.LayerChart() | alt.HConcatChart() | alt.VConcatChart() | alt.ConcatChart() | alt.FacetChart():
                return ChartSpec.Vega(_theme_vega(engine, palette))
            case _ if type(engine).__module__.startswith("lets_plot"):
                return ChartSpec.LetsPlot(engine, palette)
            case _:
                return ChartSpec.Matplotlib(engine, palette)


def _theme_vega(builder: VegaBuilder, palette: Palette) -> dict[str, object]:
    ramp = hex_ramp(palette)
    themed = (
        builder.encode(color=alt.Color("category:N", scale=alt.Scale(range=ramp), legend=alt.Legend(title=None))).interactive()
        if isinstance(builder, alt.Chart)
        else builder
    )
    resolved = themed.to_dict()
    config = dict(resolved.get("config", {}))
    config["range"] = {**config.get("range", {}), "category": ramp, "ordinal": ramp, "ramp": ramp}
    return {**resolved, "config": config}


def hex_ramp(palette: Palette) -> list[str]:
    return [f"#{int(r * 255):02x}{int(g * 255):02x}{int(b * 255):02x}" for r, g, b in np.clip(palette, 0.0, 1.0)]
```

`ChartSpec.of` is the one composer keyed by the admitted altair builder roots through `isinstance`-shaped `match` — no parallel `ChartEngine` enum and no `ChartSpec.compose` forwarding hop. The Vega arm folds the builder to its Vega-Lite dict through `to_dict`, composes `Chart.interactive()` on the single-view `Chart` so the emitted spec carries the pan/zoom selection the `vegalite_to_html` row renders without a browser, binds the palette through the `Color`-channel `Scale(range=...)` on that single view and as the `config.range.category`/`ordinal`/`ramp` discrete scales for every chart kind, so the single-color-source invariant holds whether the chart is one view or a `layer`/`concat`/`facet` composite. The gated `LetsPlot`/`Matplotlib` cases carry the raw engine object plus the `Palette` array so the palette-thread runs on the gated band beside its render — the matplotlib `Figure` and the lets-plot `PlotSpec` never construct on the cp315 core, and `hex_ramp` is the one shared RGB-to-hex projection both bands reach.

## [03]-[EXPORT]

- Owner: `ChartExport` the static-and-interactive export dispatch over the chart case, the typed `RenderPolicy`, and the target `ExportFormat`; `ExportFormat` the closed `StrEnum` whose `row` column resolves the `VlRow` converter member and whose `interactive` column marks the HTML row; `VlRow` the one frozen `Struct` row carrying each format's vl-convert `convert` member, its `text` `str`-return flag, and its admitted-policy `keys` tuple so the per-format render axis is one module-level `VL_RENDER` table never a per-format `match` arm; `RenderPolicy` the one frozen `Struct` carrying the `scale`/`ppi`/`theme`/`vl_version` render knobs plus the `fonts` directory tuple, projected onto each `VlRow.convert` call through `projected(row.keys)` so the row's own `keys` column selects exactly the parameters the converter admits — `scale`/`ppi`/`theme`/`vl_version` on the raster rows, `theme`/`vl_version` on the SVG/HTML rows — a knob being one field never a per-format render variant and never a boolean re-deriving the keep-set the row already names; `VegaTransform` the closed pre-pass policy whose `apply` fold returns the reduced spec plus the Arrow-IPC `inline_datasets` frame map over the gated `vegafusion` runtime, never re-inlining megabytes into the JSON spec; the `_export_host_free` fold over the chart case IS the engine selection (the band-routing decision, not byte-emit sprawl — vega pre-transforms on the subprocess seam then renders in-process, lets-plot renders in-process, matplotlib renders on the subprocess seam), with vl-convert's per-format member on the `VL_RENDER` row table and lets-plot's per-format `PlotSpec` member on the `LP_RENDER` row table so each engine's format axis is one table not a per-format arm — no parallel engine enum, no Chrome path.
- Cases: the export engine is the chart-case fold arm, never a knob — vega-via-`vl-convert-python` (the `VL_RENDER` `VlRow` table keying `vegalite_to_svg`/`vegalite_to_png`/`vegalite_to_pdf`/`vegalite_to_html`/`vegalite_to_jpeg` by `ExportFormat`, each call projected with the `RenderPolicy` parameters the row's `keys` column admits, the `RenderPolicy.fonts` directories pre-registered once through `vl-convert` `register_font_directory` and the `vl_version` pinned against `get_vegalite_versions` so an unsupported version downgrades to the newest supported rather than crashing, the Arrow-IPC frames the pre-pass extracted threaded through the converter's `inline_datasets` keyword so a large frame never inlines as JSON, preceded by the gated `vegafusion` pre-pass `VegaTransform.apply` when the spec carries data transforms or a large frame, the interactive HTML row additionally folding `ChartState.get_transformed_spec` to recover one fully-transformed self-contained spec the host-free renderer needs with no live server) · lets-plot-via-the `LP_RENDER` row table keying `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html`(`BytesIO()`) by `ExportFormat` and returning bytes entirely in-process on the gated band with no subprocess hop (bundled ImageMagick) · matplotlib-via-`Figure.savefig` on the Agg/PDF/SVG backends gated `python_version<'3.15'` on the subprocess seam, the `RenderPolicy.ppi` riding `savefig` `dpi`.
- Entry: `ChartExport.render` is `async` over the runtime `async_boundary`, returns a `RuntimeRail[ArtifactReceipt]`, and contributes the settled `receipt/receipt#RECEIPT` `ArtifactReceipt.Chart(key, engine, dialect, scale, theme, byte_len)` six-field fact — the engine the matched `ChartSpec.tag` (no parallel engine enum), the dialect the `ExportFormat` value, the scale and theme the `RenderPolicy` knobs, the byte length the rendered output — all flat scalars the receipt owner reads through its own `_facts` arm with no producer value object imported; `_compute` folds the chart case to its bytes through `_export_host_free` keyed by `ExportFormat` and threaded with `RenderPolicy`, keys the content through `ContentIdentity.of`, then projects the six-field `Chart` fact — vega through the gated `VegaTransform.apply` pre-pass on the subprocess seam returning the reduced spec plus the Arrow-IPC frame map, followed by the host-free `_vl_render` `VL_RENDER`-keyed vl-convert render in-process threading those frames through `inline_datasets`, lets-plot through the in-process `_letsplot_to_bytes` `LP_RENDER`-keyed acceptor, matplotlib through the gated-band worker `_matplotlib_savefig` awaited across the subprocess seam.
- Auto: the vl-convert render axis collapses to one `VL_RENDER` frozen `VlRow` table mapping each `ExportFormat` to its converter `convert` member, its `text` `str`-return flag (SVG/HTML return `str` and `.encode()`, PNG/PDF/JPEG return `bytes`), and its admitted-policy `keys` tuple, so a new vl-convert output format is one `ExportFormat` row plus one `VL_RENDER` entry, never a per-format `match` arm, and `RenderPolicy.projected(row.keys)` spreads exactly the policy axis the row's own `keys` column names — `scale`/`ppi`/`theme`/`vl_version` on the raster rows, `theme`/`vl_version` on the SVG/HTML rows — never a boolean re-deriving the keep-set from the `text` flag and never a per-format kwarg branch; `_register_fonts` calls `register_font_directory` once per `RenderPolicy.fonts` directory before render so a custom typeface is one directory row never a second render path, and `_pin_version` resolves `vl_version` against `get_vegalite_versions` so an unset or unsupported version downgrades to the newest supported rather than crashing the converter; `VegaTransform.apply` discriminates the pre-pass returning the reduced spec plus the Arrow-IPC frame map — `Inline` folds `runtime.pre_transform_spec` for a transform-bearing spec with inlined data, `Extract` folds `runtime.pre_transform_extract(extracted_format="arrow-ipc")` with `get_column_usage` column pruning then `transformer.to_arrow_table`+`to_arrow_ipc_bytes` serializing each extracted frame to Arrow-IPC `bytes` the `_vl_render` arm threads through the converter `inline_datasets` keyword so a large polars frame crosses as Arrow IPC rather than inlined or hex-stuffed JSON, `State` folds `runtime.new_chart_state` then `ChartState.get_transformed_spec` to one fully-transformed self-contained spec the host-free HTML renderer needs with no live server, and `Passthrough` skips the pre-pass when the spec carries neither — the pre-pass decision is computed once in `VegaTransform.of` and applied once in `apply`, never re-derived inside the worker; the lets-plot arm folds the `LP_RENDER` `to_svg`/`to_png`/`to_pdf`/`to_html` member over `BytesIO()` keyed by `ExportFormat`; the matplotlib arm folds `savefig` over the Agg/PDF/SVG backend keyed by the format value.
- Packages: `vl-convert-python` (`vegalite_to_svg`/`vegalite_to_png`/`vegalite_to_pdf`/`vegalite_to_html`/`vegalite_to_jpeg` the converter family, `register_font_directory` the font-provisioning hook, `get_vegalite_versions` the version-pin query) on the cp315 core; `vegafusion` (`runtime.pre_transform_spec`/`pre_transform_extract`/`new_chart_state`, `ChartState.get_transformed_spec`, `transformer.to_arrow_table`/`to_arrow_ipc_bytes`, `get_column_usage`), `lets-plot` (`PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html`), and `matplotlib` (`Figure.savefig`) gated `python_version<'3.15'`; runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` the subprocess lane), `receipt/receipt#RECEIPT` (`ArtifactReceipt`), `data/tabular/columnar#COLUMNAR` (the `ColumnarEgress.ArrowIpc` Arrow-IPC frame source the `Extract` arm's `inline_datasets` feed consumes).
- Growth: a new export format is one `ExportFormat` row plus one `VL_RENDER` `VlRow` entry (vega) and one `LP_RENDER` entry (lets-plot); a new render knob is one `RenderPolicy` field named in the consuming row's `keys` tuple; a new transform pre-pass mode is one `VegaTransform` case plus one `apply` arm; a new host-free engine is one `ChartSpec` case plus one `_export_host_free` band-routing arm carrying its own format row table; zero new surface.
- Boundary: a per-backend export class family and a per-format byte-emit branch are the deleted forms — each engine's format axis is one row table (`VL_RENDER`/`LP_RENDER`) and `_export_host_free` carries only the three band-routing arms; no kaleido and no host Chrome — the degraded `_plotly_via_chrome` path is removed entirely; vegafusion carries the `python_version<'3.15'` band marker and its abi3 wheel imports on the cp315 core, yet by band policy the transform arm dispatches onto the runtime subprocess lane (`anyio.to_process.run_sync`), the Arrow-IPC `Extract` arm is the `data/tabular/columnar#COLUMNAR` chart-to-data seam carrying a large frame as Arrow IPC through the converter `inline_datasets` keyword rather than inlined or hex-stuffed JSON, and the `State` arm folds `ChartState.get_transformed_spec` to recover one fully-transformed self-contained render spec for the HTML row so the host-free interactivity the plotly drop removed is recovered without a browser or a live server; the single-view Vega arm composes `Chart.interactive()` so the HTML row carries pan/zoom selections natively; lets-plot rides the gated band IN-PROCESS because `PlotSpec.to_*` is pure-Python with bundled ImageMagick, never the subprocess seam; matplotlib rides the gated band and never resolves in the cp315-core process, so its arm dispatches onto the subprocess lane; the export keys by one runtime content owner and contributes the settled six-field `ArtifactReceipt.Chart` carrying the engine/dialect/scale/theme/byte-len render facts.

```python signature
import json
from collections.abc import Callable
from enum import StrEnum
from io import BytesIO
from typing import Literal, assert_never

import vl_convert as vlc
from anyio import to_process
from expression import case, tag, tagged_union
from msgspec import Struct, structs

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.receipt.receipt import ArtifactReceipt


_ARROW_THRESHOLD: int = 32


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
    theme: str = "default"
    vl_version: str | None = None
    fonts: tuple[str, ...] = ()

    def projected(self, keys: tuple[str, ...]) -> dict[str, object]:
        return {key: value for key in keys if (value := getattr(self, key)) is not None}


class VlRow(Struct, frozen=True):
    convert: Callable[..., str | bytes]
    text: bool
    keys: tuple[str, ...]


VL_RENDER: dict[ExportFormat, VlRow] = {
    ExportFormat.SVG: VlRow(vlc.vegalite_to_svg, True, ("theme", "vl_version")),
    ExportFormat.PNG: VlRow(vlc.vegalite_to_png, False, ("scale", "ppi", "theme", "vl_version")),
    ExportFormat.PDF: VlRow(vlc.vegalite_to_pdf, False, ("scale", "ppi", "theme", "vl_version")),
    ExportFormat.HTML: VlRow(vlc.vegalite_to_html, True, ("theme", "vl_version")),
    ExportFormat.JPEG: VlRow(vlc.vegalite_to_jpeg, False, ("scale", "ppi", "theme", "vl_version")),
}


def _register_fonts(fonts: tuple[str, ...]) -> None:
    for directory in fonts:
        vlc.register_font_directory(directory)


def _pin_version(requested: str | None) -> str:
    available = vlc.get_vegalite_versions()
    return requested if requested in available else available[-1]


@tagged_union(frozen=True)
class VegaTransform:
    tag: Literal["passthrough", "inline", "extract", "state"] = tag()
    passthrough: None = case()
    inline: None = case()
    extract: tuple[str, ...] = case()
    state: None = case()

    @staticmethod
    def Passthrough() -> "VegaTransform":
        return VegaTransform(passthrough=None)

    @staticmethod
    def Inline() -> "VegaTransform":
        return VegaTransform(inline=None)

    @staticmethod
    def Extract(frame_keys: tuple[str, ...]) -> "VegaTransform":
        return VegaTransform(extract=frame_keys)

    @staticmethod
    def State() -> "VegaTransform":
        return VegaTransform(state=None)

    @staticmethod
    def of(spec: dict[str, object], interactive: bool) -> "VegaTransform":
        import vegafusion

        usage = vegafusion.get_column_usage(spec)
        keys = tuple(name for name, columns in usage.items() if columns is None or len(columns) > _ARROW_THRESHOLD)
        has_transform = "transform" in spec or any("transform" in layer for layer in spec.get("layer", ()) if isinstance(layer, dict))
        if interactive and (has_transform or keys):
            return VegaTransform.State()
        if keys:
            return VegaTransform.Extract(keys)
        return VegaTransform.Inline() if has_transform else VegaTransform.Passthrough()

    def apply(self, spec: dict[str, object]) -> tuple[dict[str, object], dict[str, bytes]]:
        import vegafusion

        match self:
            case VegaTransform(tag="passthrough"):
                return spec, {}
            case VegaTransform(tag="inline"):
                transformed, _warnings = vegafusion.runtime.pre_transform_spec(spec)
                return transformed, {}
            case VegaTransform(tag="extract", extract=frame_keys):
                reduced, datasets, _warnings = vegafusion.runtime.pre_transform_extract(spec, extracted_format="arrow-ipc")
                return reduced, {name: vegafusion.transformer.to_arrow_ipc_bytes(vegafusion.transformer.to_arrow_table(frame)) for name, frame in datasets if name in frame_keys}
            case VegaTransform(tag="state"):
                return vegafusion.runtime.new_chart_state(spec).get_transformed_spec(), {}
            case _:
                assert_never(self)


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
            reduced, frames = await to_process.run_sync(_pre_transform, json.dumps(spec), fmt.interactive)
            return _vl_render(reduced, frames, fmt, policy)
        case ChartSpec(tag="lets_plot", lets_plot=(plot, palette)):
            return _letsplot_to_bytes(plot, palette, fmt)
        case ChartSpec(tag="matplotlib", matplotlib=(figure, palette)):
            return await to_process.run_sync(_matplotlib_savefig, figure, palette, fmt.value, policy.ppi)
        case _:
            assert_never(chart)


def _vl_render(spec: dict[str, object], frames: dict[str, bytes], fmt: ExportFormat, policy: RenderPolicy) -> bytes:
    _register_fonts(policy.fonts)
    row = fmt.row
    pinned = structs.replace(policy, vl_version=_pin_version(policy.vl_version))
    output = row.convert(spec, inline_datasets=frames or None, **pinned.projected(row.keys))
    return output.encode() if row.text else output


LP_RENDER: dict[ExportFormat, str] = {
    ExportFormat.SVG: "to_svg",
    ExportFormat.PNG: "to_png",
    ExportFormat.PDF: "to_pdf",
    ExportFormat.HTML: "to_html",
}


def _letsplot_to_bytes(plot: object, palette: Palette, fmt: ExportFormat) -> bytes:
    from lets_plot import scale_color_manual, scale_fill_manual

    ramp = hex_ramp(palette)
    themed = plot + scale_color_manual(values=ramp) + scale_fill_manual(values=ramp)
    sink = BytesIO()
    getattr(themed, LP_RENDER[fmt])(sink)
    return sink.getvalue()


def _pre_transform(spec_json: str, interactive: bool) -> tuple[dict[str, object], dict[str, bytes]]:
    spec = json.loads(spec_json)
    return VegaTransform.of(spec, interactive).apply(spec)


def _matplotlib_savefig(figure: object, palette: Palette, fmt: str, ppi: float) -> bytes:
    import matplotlib

    matplotlib.use("Agg")
    from matplotlib import colormaps, colors

    colormaps.register(colors.ListedColormap(palette, name=f"chart-{id(figure):x}"), force=True)
    for axes in figure.axes:
        axes.set_prop_cycle(color=hex_ramp(palette))
    sink = BytesIO()
    figure.savefig(sink, format=fmt, dpi=ppi)
    return sink.getvalue()
```

## [04]-[RESEARCH]

- [LETSPLOT_ENGINE] [RESEARCH]: `lets-plot` is declared in the manifest gated `python_version<'3.15'` (`artifacts: host-free grammar-of-graphics self-render to svg/png/pdf bytes`), but NO folder `.api/lets-plot.md` catalogue exists. The `LetsPlot`/`ggplot`/`PlotSpec` construction surface, the `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html(file)` in-process byte serializers over a file-like sink (the `LP_RENDER` row members the `getattr` fold resolves), the bundled-ImageMagick raster path, and the `scale_color_manual`/`scale_fill_manual` palette-thread members are all marked RESEARCH and never appear as settled fence code until a `lets-plot` `.api` reflection pass lands. Close-condition: `.api/lets-plot.md` carries `PlotSpec`, the four `to_*` byte serializers with their sink-argument contract, the in-process (no subprocess) render guarantee, and the `scale_*_manual` palette surface; until then the `_letsplot_to_bytes` acceptor's palette-thread plus byte serialization and the `ChartSpec.LetsPlot` case body are RESEARCH-gated. The `ChartSpec.LetsPlot(plot, palette)` `(object, Palette)` case shape, the `LP_RENDER` `ExportFormat -> method-name` row table (mirroring the settled `VL_RENDER` `VlRow` table so a new lets-plot output format is one row not a per-format arm), and the gated-band in-process dispatch are the settled structure; the `lets_plot` member spellings the `LP_RENDER` rows name are the RESEARCH leg.
- [ALTAIR_COMPOSE]: the altair `Chart`/`LayerChart`/`HConcatChart`/`VConcatChart`/`ConcatChart`/`FacetChart` builder roots, the `Chart.mark_*`/`encode`/`transform_*`/`interactive`/`properties`/`to_dict` builder family, the `X`/`Y`/`Color`/`Size`/`Shape`/`Tooltip` channel constructors, the `Scale`/`Axis`/`Legend` guides, the `selection_point`/`selection_interval`/`condition` selections, and the `layer`/`hconcat`/`vconcat`/`concat` composition operators verify against the folder `.api/altair.md` catalogue (`[02]-[PUBLIC_TYPES]` chart/composition roots rows [01]-[06], encoding rows [01]-[08]; `[03]-[ENTRYPOINTS]` construction rows [01]-[06], composition rows [01]-[07]). The `_theme_vega` palette-thread binds the `Color("category:N", scale=Scale(range=ramp), legend=Legend(title=None))` channel and composes `Chart.interactive()` on the single-view `Chart` so the emitted spec carries the pan/zoom selection the HTML row renders, plus the `config.range.category`/`ordinal`/`ramp` discrete scales for every builder root, so the single-color-source invariant holds across one view, a `layer`/`concat`/`facet` composite, or a raw Vega-Lite dict — the catalogue settles `to_dict` as the spec-emit surface (`[03]-[ENTRYPOINTS]` composition row [01]), `interactive` as the pan/zoom enable (`[03]-[ENTRYPOINTS]` construction row [05]), and `encode` plus the `Color`/`Scale`/`Legend` value objects as the channel surface, so every `_theme_vega` spelling including the `Chart.interactive()` call is settled altair fence code other than the `config.range.ordinal`/`ramp` scale keys. The `config.range.ordinal`/`ramp` Vega-Lite scale-name keys are the one catalogue-independent Vega-Lite-config item the altair catalogue does not enumerate (they are Vega-Lite config, not an altair member), settled against the Vega-Lite config schema rather than a package member.
- [VL_CONVERT]: the `vl-convert-python` `vegalite_to_svg`/`vegalite_to_png`/`vegalite_to_pdf`/`vegalite_to_html`/`vegalite_to_jpeg` converter rows, the `register_font_directory` font-provisioning hook, and the `get_vegalite_versions` version query verify against the folder `.api/vl-convert-python.md` catalogue (`1.9.0.post1` reflected on cp315, Vega-Lite converter `[03]-[ENTRYPOINTS]` rows [01]/[02]/[04]/[05]/[03], Vega-converter-and-configuration `register_font_directory` row [04] and `get_vegalite_versions` row [06]); the `vegalite_to_svg`/`vegalite_to_html` `str` return and the `vegalite_to_png`/`vegalite_to_pdf`/`vegalite_to_jpeg` `bytes` return are the `VlRow.text` flag, so the converter family is the one render axis keyed by `ExportFormat` and the HTML row recovers the host-free interactivity the plotly drop removed. The catalogue `[03]-[ENTRYPOINTS]` documents the raster rows (`vegalite_to_png`/`vegalite_to_pdf`/`vegalite_to_jpeg`) as `spec plus raster policy` carrying `scale`/`ppi`/`quality`, the SVG row as `spec plus render policy`, and the HTML row as `spec plus bundle policy`, and `[04]-[IMPLEMENTATION_LAW]` fixes `theme`/`vl_version` as the version/theme axis spanning every row — so the `VlRow.keys` column carries `scale`/`ppi`/`theme`/`vl_version` on the `bytes`-returning raster rows and `theme`/`vl_version` on the `str`-returning SVG/HTML rows, and `RenderPolicy.projected(row.keys)` spreads exactly that admitted set, the row's own `keys` column being the raster-policy applicability key so no boolean re-derives the keep-set, no per-format kwarg branch exists, and no `scale`/`ppi` reaches the SVG/HTML converters that reject them. `_register_fonts` folds `register_font_directory` once per `RenderPolicy.fonts` directory and `_pin_version` resolves `vl_version` against `get_vegalite_versions()` so an unset or unsupported version downgrades to the newest supported (the `[04]-[IMPLEMENTATION_LAW]` version-axis law "the chart owner pins `vl_version` from this list") rather than crashing the converter. The exact per-converter render-policy kwarg name (`ppi` versus a dpi alias, `theme` versus a config-theme name) and the `get_vegalite_versions()` element ordering the `_pin_version` `available[-1]` newest-fallback assumes (the `.api` row [06] documents `-> list[str]` without an ordering contract) are the [VL_POLICY_KWARGS] catalogue-deepen item until a `vegalite_to_*` signature plus a `get_vegalite_versions` ordering reflection pass land; every `VL_RENDER` `VlRow` member spelling plus `register_font_directory`/`get_vegalite_versions` is settled fence code.
- [VL_INLINE_DATASETS] [RESEARCH]: the `_vl_render` arm threads the pre-pass Arrow-IPC frame map through the converter `inline_datasets=` keyword, but the folder `.api/vl-convert-python.md` catalogue documents the `vegalite_to_*` call shape as `spec plus render/raster/bundle policy` without enumerating an `inline_datasets` parameter. The Arrow-IPC inline-dataset feed (so a `pre_transform_extract` extracted frame crosses to the renderer as Arrow IPC `bytes` rather than inlined or hex-stuffed JSON) stays a marked RESEARCH item until a `vegalite_to_*` signature reflection pass enumerates the inline-dataset keyword and its `name -> bytes` map shape; the `VL_RENDER` converter members, the `VlRow.keys` policy projection, and the `frames or None` empty-elision are settled, only the `inline_datasets` keyword name and frame-map element type are the RESEARCH leg. Close-condition: `.api/vl-convert-python.md` carries the `vegalite_to_*` `inline_datasets` parameter with its Arrow-IPC `bytes` map contract.
- [VEGAFUSION_ARROW]: the `vegafusion` `runtime.pre_transform_spec`, `runtime.pre_transform_extract`, `runtime.new_chart_state`, `ChartState.get_transformed_spec`, `transformer.to_arrow_table`, `transformer.to_arrow_ipc_bytes`, and `get_column_usage` spellings verify against the folder `.api/vegafusion.md` catalogue (`2.0.3` reflected, band `python_version<'3.15'`, abi3 on cp315). The `pre_transform_extract(spec, ..., extract_threshold=20, extracted_format='arro3', ...) -> (spec, datasets, warnings)` split-and-extract surface, the `transformer.to_arrow_table(data) -> pa.Table` and `transformer.to_arrow_ipc_bytes(data, stream=False) -> bytes` Arrow conversion pair, the `new_chart_state(spec, ...) -> ChartState` interactive root, and the `ChartState.get_transformed_spec() -> dict` fully-transformed self-contained spec are catalogue-confirmed (`[03]-[ENTRYPOINTS]` rows [03]/[04], `[ChartState and transformer]` rows [01]/[06]/[07], `get_column_usage` row [08]), so the `Extract` arm's `to_arrow_table`+`to_arrow_ipc_bytes` Arrow-IPC serialization, the `State` arm's `ChartState.get_transformed_spec` self-contained spec, and the `VegaTransform.of` column-usage pruning are all settled fence code. The pre-pass runs on the gated subprocess lane and returns the reduced spec plus a `name -> Arrow-IPC bytes` frame map; the `_vl_render` arm threads that map through the converter `inline_datasets` keyword (the [VL_INLINE_DATASETS] research leg) so a large frame crosses to the renderer as Arrow IPC, never re-inlined or hex-stuffed into the JSON spec — the prior hex-into-`datasets` round-trip is the deleted double-encode. The catalogue documents `extracted_format` with the `'arro3'` default; the `'arrow-ipc'` explicit value the `Extract` arm passes and the `datasets` element shape it iterates (a `(name, frame)` pairing `to_arrow_table` then `to_arrow_ipc_bytes` serializes) are the one [VEGAFUSION_EXTRACT_SHAPE] catalogue-deepen item until a `pre_transform_extract` return-element reflection pass enumerates the `extracted_format` literal set and the `datasets` element tuple. The `vl-convert` render arm consumes the reduced spec plus the Arrow-IPC frame map on the cp315 core, the `vegafusion` pre-pass runs on the gated subprocess lane.
- [CHART_RECEIPT_FACTS] [RESOLVED]: `ChartExport._compute` projects `ArtifactReceipt.Chart(key, self.chart.tag, self.fmt.value, self.policy.scale, self.policy.theme, len(data))` against the widened `receipt/receipt#RECEIPT` `Chart(key, engine, dialect, scale, theme, byte_len)` six-field constructor whose `_facts` arm projects `{"key", "engine", "dialect", "scale", "theme", "bytes"}`, so the receipt call is settled fence code with zero contradiction. The widening is all flat scalars — engine the matched `ChartSpec.tag` (no parallel engine enum), dialect the `ExportFormat` value, scale and theme the `RenderPolicy` knobs, byte length the rendered output — a field widening on the existing `chart` case, never a new receipt case, never a parallel chart-receipt owner, and never a producer value object the receipt owner would import. The `ChartExport.render -> RuntimeRail[ArtifactReceipt]` rail shape mirrors the settled `figures/preview#PREVIEW` `Preview.of -> RuntimeRail[ArtifactReceipt]` pattern.
- [DROP_PLOTLY_KALEIDO]: the `ChartSpec.Plotly` case, the `_plotly_via_chrome` arm, and the `_export_host_free` plotly fold arm are deleted — vl-convert renders only Vega/Vega-Lite (never plotly.js), so every plotly static export is reachable only through kaleido's headless Chromium `get_chrome_sync`/`calc_fig_sync`, the degraded host-Chrome path the host-free posture forbids as default. The `ARCHITECTURE.md` `[2]-[SEAMS]` carries no cross-folder consumer of plotly JSON, so the deletion is seam-clean. The companion `README` `[Charts]` plotly/kaleido rows, the `pyproject.toml` plotly/kaleido manifest rows, and the `.api/plotly.md`/`.api/kaleido.md` catalogues are out of this page's edit scope and ripple to the `README`/manifest/`ARCHITECTURE` owners as a counterpart drop; the host-free interactivity the plotly removal drops is recovered by the `ExportFormat.HTML` `vegalite_to_html` row plus the lets-plot `to_html` arm, so no interactive capability is lost.
