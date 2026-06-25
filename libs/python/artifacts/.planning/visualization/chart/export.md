# [PY_ARTIFACTS_CHART_EXPORT]

The host-free render/format dispatch half of the 2D chart axis. `ChartExport` folds the `visualization/chart/spec#CHART` chart case, the typed `RenderPolicy`, and the target `ExportFormat` to bytes, contributing one `core/receipt#RECEIPT` `ArtifactReceipt.Chart`. The host-free posture is the decisive axis: vl-convert-python is the Vega/Vega-Lite engine (Rust-native, embedded V8/deno_runtime, inlined Vega sources, zero browser, SVG/PNG/PDF/HTML/JPEG/scenegraph), and lets-plot self-renders to bytes entirely in-process (bundled ImageMagick, no browser, no Node, no Vega binary), so two byte-identical content-keyable producers exist with zero Chrome dependency. The `visualization/chart/transform#TRANSFORM` `VegaTransform` pre-pass is the server-side Vega transform stage executed before the vl-convert render — gated `python_version<'3.15'` and crossing the runtime subprocess seam — returning the reduced spec plus the Arrow-IPC `inline_datasets` frame map this owner's `_vl_render` arm threads through the converter so megabytes never inline as JSON. matplotlib and lets-plot are gated `python_version<'3.15'`: matplotlib never resolves in the cp315-core process and renders on the runtime subprocess seam, while lets-plot rides the gated band in-process because `PlotSpec.to_*` is pure-Python over bundled ImageMagick. The themed render bytes are a flat-SVG/raster handoff consumed by the regrouped `composition/compose#COMPOSE` placement owner, which lays the chart out beside its siblings and re-renders nothing.

## [01]-[INDEX]

- [01]-[EXPORT]: host-free static-and-interactive export rows over the typed `RenderPolicy`, the `visualization/chart/transform#TRANSFORM` inline-JSON-or-Arrow-IPC transform pre-pass, and the `ChartState` interactive-HTML server/client split, contributing the typed `core/receipt#RECEIPT` `ArtifactReceipt.Chart` facts.

## [02]-[EXPORT]

- Owner: `ChartExport` the static-and-interactive export dispatch over the `visualization/chart/spec#CHART` chart case, the typed `RenderPolicy`, and the target `ExportFormat`; `ExportFormat` the closed `StrEnum` whose `row` column resolves the `VlRow` converter member and whose `interactive` column marks the HTML row; `VlRow` the one frozen `Struct` row carrying each format's vl-convert `convert` member, its `text` `str`-return flag, and its admitted-policy `keys` tuple so the per-format render axis is one module-level `VL_RENDER` table never a per-format `match` arm; `RenderPolicy` the one frozen `Struct` carrying the `scale`/`ppi`/`theme`/`vl_version` render knobs plus the `fonts` directory tuple, projected onto each `VlRow.convert` call through `projected(row.keys)` so the row's own `keys` column selects exactly the parameters the converter admits — `scale`/`ppi`/`theme`/`vl_version` on the raster rows, `theme`/`vl_version` on the SVG/HTML rows — a knob being one field never a per-format render variant and never a boolean re-deriving the keep-set the row already names; the `_export_host_free` fold over the chart case IS the engine selection (the band-routing decision, not byte-emit sprawl — vega pre-transforms on the subprocess seam then renders in-process, lets-plot renders in-process, matplotlib renders on the subprocess seam), with vl-convert's per-format member on the `VL_RENDER` row table and lets-plot's per-format `PlotSpec` member on the `LP_RENDER` row table so each engine's format axis is one table not a per-format arm — no parallel engine enum, no Chrome path.
- Cases: the export engine is the chart-case fold arm, never a knob — vega-via-`vl-convert-python` (the `VL_RENDER` `VlRow` table keying `vegalite_to_svg`/`vegalite_to_png`/`vegalite_to_pdf`/`vegalite_to_html`/`vegalite_to_jpeg` by `ExportFormat`, each call projected with the `RenderPolicy` parameters the row's `keys` column admits, the `RenderPolicy.fonts` directories pre-registered once through `vl-convert` `register_font_directory` and the `vl_version` pinned against `get_vegalite_versions` so an unsupported version downgrades to the newest supported rather than crashing, the Arrow-IPC frames the `visualization/chart/transform#TRANSFORM` pre-pass extracted threaded through the converter's `inline_datasets` keyword so a large frame never inlines as JSON, preceded by the gated `VegaTransform.apply` pre-pass when the spec carries data transforms or a large frame, the interactive HTML row additionally folding `ChartState.get_transformed_spec` to recover one fully-transformed self-contained spec the host-free renderer needs with no live server) · lets-plot-via-the `LP_RENDER` row table keying `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html`(`BytesIO()`) by `ExportFormat` and returning bytes entirely in-process on the gated band with no subprocess hop (bundled ImageMagick) · matplotlib-via-`Figure.savefig` on the Agg/PDF/SVG backends gated `python_version<'3.15'` on the subprocess seam, the `RenderPolicy.ppi` riding `savefig` `dpi`.
- Entry: `ChartExport.render` is `async` over the runtime `async_boundary`, returns a `RuntimeRail[ArtifactReceipt]`, and contributes the settled `core/receipt#RECEIPT` `ArtifactReceipt.Chart(key, engine, dialect, scale, theme, byte_len)` six-field fact — the engine the matched `ChartSpec.tag` (no parallel engine enum), the dialect the `ExportFormat` value, the scale and theme the `RenderPolicy` knobs, the byte length the rendered output — all flat scalars the receipt owner reads through its own `_facts` arm with no producer value object imported; `_compute` folds the chart case to its bytes through `_export_host_free` keyed by `ExportFormat` and threaded with `RenderPolicy`, keys the content through `ContentIdentity.of`, then projects the six-field `Chart` fact — vega through the gated `visualization/chart/transform#TRANSFORM` `VegaTransform.apply` pre-pass on the subprocess seam returning the reduced spec plus the Arrow-IPC frame map, followed by the host-free `_vl_render` `VL_RENDER`-keyed vl-convert render in-process threading those frames through `inline_datasets`, lets-plot through the in-process `_letsplot_to_bytes` `LP_RENDER`-keyed acceptor, matplotlib through the gated-band worker `_matplotlib_savefig` awaited across the subprocess seam.
- Auto: the vl-convert render axis collapses to one `VL_RENDER` frozen `VlRow` table mapping each `ExportFormat` to its converter `convert` member, its `text` `str`-return flag (SVG/HTML return `str` and `.encode()`, PNG/PDF/JPEG return `bytes`), and its admitted-policy `keys` tuple, so a new vl-convert output format is one `ExportFormat` row plus one `VL_RENDER` entry, never a per-format `match` arm, and `RenderPolicy.projected(row.keys)` spreads exactly the policy axis the row's own `keys` column names — `scale`/`ppi`/`theme`/`vl_version` on the raster rows, `theme`/`vl_version` on the SVG/HTML rows — never a boolean re-deriving the keep-set from the `text` flag and never a per-format kwarg branch; `_register_fonts` calls `register_font_directory` once per `RenderPolicy.fonts` directory before render so a custom typeface is one directory row never a second render path, and `_pin_version` resolves `vl_version` against `get_vegalite_versions` so an unset or unsupported version downgrades to the newest supported rather than crashing the converter; the `visualization/chart/transform#TRANSFORM` `VegaTransform.of`/`apply` owns the pre-pass decision and the reduced-spec-plus-frame-map return, this owner threading `_pre_transform` onto `anyio.to_process.run_sync` so the gated `vegafusion` import never resolves on the cp315 core; the lets-plot arm folds the `LP_RENDER` `to_svg`/`to_png`/`to_pdf`/`to_html` member over `BytesIO()` keyed by `ExportFormat`; the matplotlib arm folds `savefig` over the Agg/PDF/SVG backend keyed by the format value.
- Packages: `vl-convert-python` (`vegalite_to_svg`/`vegalite_to_png`/`vegalite_to_pdf`/`vegalite_to_html`/`vegalite_to_jpeg` the converter family, `register_font_directory` the font-provisioning hook, `get_vegalite_versions` the version-pin query) on the cp315 core; `lets-plot` (`PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html`) and `matplotlib` (`Figure.savefig`) gated `python_version<'3.15'`; runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` the subprocess lane), `core/receipt#RECEIPT` (`ArtifactReceipt`), `visualization/chart/spec#CHART` (`ChartSpec`/`Palette`/`hex_ramp`), `visualization/chart/transform#TRANSFORM` (`VegaTransform`), `data/tabular/columnar#COLUMNAR` (the `ColumnarEgress.ArrowIpc` Arrow-IPC frame source the `Extract` arm's `inline_datasets` feed consumes).
- Growth: a new export format is one `ExportFormat` row plus one `VL_RENDER` `VlRow` entry (vega) and one `LP_RENDER` entry (lets-plot); a new render knob is one `RenderPolicy` field named in the consuming row's `keys` tuple; a new transform pre-pass mode is one `visualization/chart/transform#TRANSFORM` `VegaTransform` case plus one `apply` arm; a new host-free engine is one `visualization/chart/spec#CHART` `ChartSpec` case plus one `_export_host_free` band-routing arm carrying its own format row table; zero new surface.
- Boundary: a per-backend export class family and a per-format byte-emit branch are the deleted forms — each engine's format axis is one row table (`VL_RENDER`/`LP_RENDER`) and `_export_host_free` carries only the three band-routing arms; no kaleido and no host Chrome — the degraded `_plotly_via_chrome` path is removed entirely; the `vegafusion` pre-pass carries the `python_version<'3.15'` band marker and dispatches onto the runtime subprocess lane (`anyio.to_process.run_sync`) via `visualization/chart/transform#TRANSFORM`, the Arrow-IPC `Extract` arm is the `data/tabular/columnar#COLUMNAR` chart-to-data seam carrying a large frame as Arrow IPC through the converter `inline_datasets` keyword rather than inlined or hex-stuffed JSON, and the `State` arm folds `ChartState.get_transformed_spec` to recover one fully-transformed self-contained render spec for the HTML row so the host-free interactivity the plotly drop removed is recovered without a browser or a live server; the single-view Vega arm composes `Chart.interactive()` (in `visualization/chart/spec#CHART`) so the HTML row carries pan/zoom selections natively; lets-plot rides the gated band IN-PROCESS because `PlotSpec.to_*` is pure-Python with bundled ImageMagick, never the subprocess seam; matplotlib rides the gated band and never resolves in the cp315-core process, so its arm dispatches onto the subprocess lane; the export keys by one runtime content owner and contributes the settled six-field `ArtifactReceipt.Chart` carrying the engine/dialect/scale/theme/byte-len render facts.

```python signature
import json
from collections.abc import Callable
from enum import StrEnum
from io import BytesIO
from typing import assert_never

import vl_convert as vlc
from anyio import to_process
from msgspec import Struct, structs

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.visualization.chart.spec import ChartSpec, Palette, hex_ramp
from artifacts.visualization.chart.transform import VegaTransform


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

`_export_host_free` IS the engine selection — the band-routing decision over the three chart cases, not byte-emit sprawl. The vega arm threads `_pre_transform` onto `anyio.to_process.run_sync` so the gated `visualization/chart/transform#TRANSFORM` `VegaTransform` pre-pass runs on the subprocess seam returning the reduced spec plus the Arrow-IPC frame map, then `_vl_render` renders in-process and threads that map through the converter `inline_datasets` keyword; lets-plot renders in-process on the gated band; matplotlib renders on the subprocess seam. Each engine's format axis is one row table — `VL_RENDER` keying the converter member, its `text` flag, and its admitted-policy `keys`, `LP_RENDER` keying the `PlotSpec` method name — so a new output format is one row, never a per-format `match` arm, and `RenderPolicy.projected(row.keys)` spreads exactly the policy axis the row admits.

## [03]-[RESEARCH]

- [LETSPLOT_ENGINE] [RESEARCH]: `lets-plot` is declared in the manifest gated `python_version<'3.15'` (`artifacts: host-free grammar-of-graphics self-render to svg/png/pdf bytes`). The `LetsPlot`/`ggplot`/`PlotSpec` construction surface, the `PlotSpec.to_svg`/`to_png`/`to_pdf`/`to_html(file)` in-process byte serializers over a file-like sink (the `LP_RENDER` row members the `getattr` fold resolves), the bundled-ImageMagick raster path, and the `scale_color_manual`/`scale_fill_manual` palette-thread members verify against the folder `.api/lets-plot.md` catalogue; the `_letsplot_to_bytes` acceptor's palette-thread plus byte serialization and the `visualization/chart/spec#CHART` `ChartSpec.LetsPlot` case body remain RESEARCH-gated until the `lets-plot` `.api` `PlotSpec`/`to_*` sink-argument contract and the in-process (no subprocess) render guarantee are confirmed against the reflected surface. The `ChartSpec.LetsPlot(plot, palette)` `(object, Palette)` case shape, the `LP_RENDER` `ExportFormat -> method-name` row table (mirroring the settled `VL_RENDER` `VlRow` table so a new lets-plot output format is one row not a per-format arm), and the gated-band in-process dispatch are the settled structure; the `lets_plot` member spellings the `LP_RENDER` rows name are the RESEARCH leg.
- [VL_CONVERT]: the `vl-convert-python` `vegalite_to_svg`/`vegalite_to_png`/`vegalite_to_pdf`/`vegalite_to_html`/`vegalite_to_jpeg` converter rows, the `register_font_directory` font-provisioning hook, and the `get_vegalite_versions` version query verify against the folder `.api/vl-convert-python.md` catalogue (`1.9.0.post1` reflected on cp315, Vega-Lite converter `[03]-[ENTRYPOINTS]` rows [01]/[02]/[04]/[05]/[03], Vega-converter-and-configuration `register_font_directory` row [04] and `get_vegalite_versions` row [06]); the `vegalite_to_svg`/`vegalite_to_html` `str` return and the `vegalite_to_png`/`vegalite_to_pdf`/`vegalite_to_jpeg` `bytes` return are the `VlRow.text` flag, so the converter family is the one render axis keyed by `ExportFormat` and the HTML row recovers the host-free interactivity the plotly drop removed. The catalogue `[03]-[ENTRYPOINTS]` documents the raster rows (`vegalite_to_png`/`vegalite_to_pdf`/`vegalite_to_jpeg`) as `spec plus raster policy` carrying `scale`/`ppi`/`quality`, the SVG row as `spec plus render policy`, and the HTML row as `spec plus bundle policy`, and `[04]-[IMPLEMENTATION_LAW]` fixes `theme`/`vl_version` as the version/theme axis spanning every row — so the `VlRow.keys` column carries `scale`/`ppi`/`theme`/`vl_version` on the `bytes`-returning raster rows and `theme`/`vl_version` on the `str`-returning SVG/HTML rows, and `RenderPolicy.projected(row.keys)` spreads exactly that admitted set, the row's own `keys` column being the raster-policy applicability key so no boolean re-derives the keep-set, no per-format kwarg branch exists, and no `scale`/`ppi` reaches the SVG/HTML converters that reject them. `_register_fonts` folds `register_font_directory` once per `RenderPolicy.fonts` directory and `_pin_version` resolves `vl_version` against `get_vegalite_versions()` so an unset or unsupported version downgrades to the newest supported (the `[04]-[IMPLEMENTATION_LAW]` version-axis law "the chart owner pins `vl_version` from this list") rather than crashing the converter. The exact per-converter render-policy kwarg name (`ppi` versus a dpi alias, `theme` versus a config-theme name) and the `get_vegalite_versions()` element ordering the `_pin_version` `available[-1]` newest-fallback assumes (the `.api` row [06] documents `-> list[str]` without an ordering contract) are the [VL_POLICY_KWARGS] catalogue-deepen item until a `vegalite_to_*` signature plus a `get_vegalite_versions` ordering reflection pass land; every `VL_RENDER` `VlRow` member spelling plus `register_font_directory`/`get_vegalite_versions` is settled fence code.
- [VL_INLINE_DATASETS] [RESEARCH]: the `_vl_render` arm threads the `visualization/chart/transform#TRANSFORM` pre-pass Arrow-IPC frame map through the converter `inline_datasets=` keyword, but the folder `.api/vl-convert-python.md` catalogue documents the `vegalite_to_*` call shape as `spec plus render/raster/bundle policy` without enumerating an `inline_datasets` parameter. The Arrow-IPC inline-dataset feed (so a `pre_transform_extract` extracted frame crosses to the renderer as Arrow IPC `bytes` rather than inlined or hex-stuffed JSON) stays a marked RESEARCH item until a `vegalite_to_*` signature reflection pass enumerates the inline-dataset keyword and its `name -> bytes` map shape; the `VL_RENDER` converter members, the `VlRow.keys` policy projection, and the `frames or None` empty-elision are settled, only the `inline_datasets` keyword name and frame-map element type are the RESEARCH leg. Close-condition: `.api/vl-convert-python.md` carries the `vegalite_to_*` `inline_datasets` parameter with its Arrow-IPC `bytes` map contract.
- [CHART_RECEIPT_FACTS] [RESOLVED]: `ChartExport._compute` projects `ArtifactReceipt.Chart(key, self.chart.tag, self.fmt.value, self.policy.scale, self.policy.theme, len(data))` against the widened `core/receipt#RECEIPT` `Chart(key, engine, dialect, scale, theme, byte_len)` six-field constructor whose `_facts` arm projects `{"key", "engine", "dialect", "scale", "theme", "bytes"}`, so the receipt call is settled fence code with zero contradiction. The widening is all flat scalars — engine the matched `ChartSpec.tag` (no parallel engine enum), dialect the `ExportFormat` value, scale and theme the `RenderPolicy` knobs, byte length the rendered output — a field widening on the existing `chart` case, never a new receipt case, never a parallel chart-receipt owner, and never a producer value object the receipt owner would import. The `ChartExport.render -> RuntimeRail[ArtifactReceipt]` rail shape mirrors the settled `graphic/raster#RASTER` `Raster.of -> RuntimeRail[ArtifactReceipt]` pattern.
- [DROP_PLOTLY_KALEIDO]: the `ChartSpec.Plotly` case, the `_plotly_via_chrome` arm, and the `_export_host_free` plotly fold arm are deleted — vl-convert renders only Vega/Vega-Lite (never plotly.js), so every plotly static export is reachable only through kaleido's headless Chromium `get_chrome_sync`/`calc_fig_sync`, the degraded host-Chrome path the host-free posture forbids as default. The `ARCHITECTURE.md` `[02]-[SEAMS]` carries no cross-folder consumer of plotly JSON, so the deletion is seam-clean. The companion `README` `[Charts]` plotly/kaleido rows, the `pyproject.toml` plotly/kaleido manifest rows, and the `.api/plotly.md`/`.api/kaleido.md` catalogues are out of this page's edit scope and ripple to the `README`/manifest/`ARCHITECTURE` owners as a counterpart drop; the host-free interactivity the plotly removal drops is recovered by the `ExportFormat.HTML` `vegalite_to_html` row plus the lets-plot `to_html` arm, so no interactive capability is lost.
