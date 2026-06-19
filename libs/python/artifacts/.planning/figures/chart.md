# [PY_ARTIFACTS_CHART]

The 2D chart-to-render axis. `ChartSpec` is ONE owner collapsing the three 2D engines — declarative (altair Vega-Lite), imperative (plotly graph_objects), and publication (matplotlib) — into one tagged union; `ChartExport` carries the host-free static-export rows. The host-free posture is the decisive axis: vl-convert-python is the primary Vega/Vega-Lite engine (Rust-native, embedded V8/deno_runtime, inlined Vega sources, zero browser, SVG-first then resvg/svg2pdf to PNG/PDF) on the cp315 core, and kaleido is a host-Chrome-gated degraded path, never the default. vegafusion is the server-side Vega transform pre-pass executing a spec's DataFusion-backed data transforms before vl-convert render, gated `python_version<'3.15'` and crossing the runtime subprocess seam. matplotlib is gated `python_version<'3.15'`: it never resolves in the cp315-core process, so its offscreen Agg/PDF/SVG render runs on the runtime subprocess seam. Every render returns a `RuntimeRail[ContentKey]`.

## [01]-[INDEX]

- [01]-[CHART]: the 2D `ChartSpec` union over the three charting engines.
- [02]-[EXPORT]: host-free static-export backend rows with the vegafusion transform pre-pass.

## [02]-[CHART]

- Owner: `ChartSpec` the one chart axis discriminating engine; the declarative, imperative, and publication engines are cases on one owner, never a parallel per-engine chart class.
- Cases: `ChartSpec` cases `Vega(spec)` (altair Vega-Lite grammar / raw Vega-Lite dict) · `Plotly(figure)` (plotly graph_objects) · `Matplotlib(figure)` (matplotlib `Figure`) — matched by `match`/`case`; altair compiles to the Vega-Lite spec the export axis consumes directly.
- Entry: `ChartSpec.compose` builds the engine object; the result feeds `EXPORT` `ChartExport.render`.
- Packages: `altair` (`Chart`/`mark_*`/`encode`/`to_dict`), `plotly` (`graph_objects.Figure`) on the cp315 core; `matplotlib` (`Figure`/Agg backend) gated `python_version<'3.15'`; the Vega-Lite spec dict feeds `EXPORT`, where `vegafusion` pre-transforms it; runtime.
- Growth: a new 2D engine is one `ChartSpec` case; zero new surface.
- Boundary: no live dashboard, UI event state, or browser runtime; 3D scientific scenes ride `figures/scene#SCENE`, not this axis; color palettes arrive from `figures/color#COLOR`, never picked ad hoc per engine.

```python signature
from typing import Literal

from expression import case, tag, tagged_union


@tagged_union(frozen=True)
class ChartSpec:
    tag: Literal["vega", "plotly", "matplotlib"] = tag()
    vega: dict[str, object] = case()
    plotly: object = case()
    matplotlib: object = case()

    @staticmethod
    def Vega(spec: dict[str, object]) -> "ChartSpec":
        return ChartSpec(vega=spec)

    @staticmethod
    def Plotly(figure: object) -> "ChartSpec":
        return ChartSpec(plotly=figure)

    @staticmethod
    def Matplotlib(figure: object) -> "ChartSpec":
        return ChartSpec(matplotlib=figure)
```

## [03]-[EXPORT]

- Owner: `ChartExport` the static-export dispatch over the chart case and target format; `ExportFormat` the closed `StrEnum`; the `_export_host_free` fold over the chart case IS the engine selection, vl-convert primary and kaleido host-Chrome-gated — no parallel engine enum.
- Cases: vega-via-`vl-convert-python` `vegalite_to_svg`/`vegalite_to_png`/`vegalite_to_pdf` (primary, host-free, cp315-core) preceded by the gated `vegafusion` `runtime.pre_transform_spec` server-side transform pre-pass when the spec carries data transforms · plotly-via-`kaleido` `calc_fig_sync` figure-to-bytes (cp315-core, gated behind detected host Chrome via `get_chrome_sync`, never the default) · matplotlib-via-`Figure.savefig` on the Agg/PDF/SVG backends (gated `python_version<'3.15'`, run on the subprocess seam) — the engine is the chart-case fold arm, not a knob.
- Entry: `ChartExport.render` is `async` over the runtime `async_boundary`, returns a `RuntimeRail[ContentKey]`, and contributes `receipt/receipt#RECEIPT` `ArtifactReceipt.Chart`; `_export_host_free` folds the chart case to its engine — vega through `_vega_to_bytes` (the gated `vegafusion` pre-transform on the subprocess seam followed by host-free vl-convert render in-process), plotly through `_plotly_via_chrome` (the gated-on-host-Chrome kaleido path, in-process, never the default), matplotlib through the gated-band worker `_matplotlib_savefig` awaited across the subprocess seam.
- Packages: `vl-convert-python` (`vegalite_to_svg`/`vegalite_to_png`/`vegalite_to_pdf`), `kaleido` (`calc_fig_sync`/`get_chrome_sync`, host-Chrome-gated) on the cp315 core; `vegafusion` (`runtime.pre_transform_spec`) and `matplotlib` (`Figure.savefig`) gated `python_version<'3.15'`; `plotly` (`graph_objects.Figure`) on the cp315 core; runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` (the runtime subprocess lane)).
- Growth: a new export format is one `ExportFormat` row; a new host-free engine is one acceptor arm; zero new surface.
- Boundary: a per-backend export class family is the deleted form; kaleido v1 locates a host Chrome via its Choreographer driver, so it is the degraded optional path gated on host presence, never the primary; vegafusion carries the `python_version<'3.15'` band marker and its abi3 wheel imports on the cp315 core, yet by band policy the transform arm dispatches onto the runtime subprocess lane; matplotlib rides the gated `python_version<'3.15'` band and never resolves in the cp315-core process, so its arm dispatches onto the runtime subprocess lane (`anyio.to_process.run_sync`); the export keys by one runtime content owner.

```python signature
import json
from enum import StrEnum
from io import BytesIO
from typing import assert_never

from anyio import to_process
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary


class ExportFormat(StrEnum):
    SVG = "svg"
    PNG = "png"
    PDF = "pdf"


class ChartExport(Struct, frozen=True):
    chart: ChartSpec
    fmt: ExportFormat

    async def render(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"chart.export.{self.fmt}", self._emit)

    async def _emit(self) -> ContentKey:
        data = await _export_host_free(self.chart, self.fmt)
        return ContentIdentity.of(f"chart-{self.fmt}", data)


async def _export_host_free(chart: ChartSpec, fmt: ExportFormat) -> bytes:
    match chart:
        case ChartSpec(tag="vega", vega=spec):
            return await _vega_to_bytes(spec, fmt)
        case ChartSpec(tag="plotly", plotly=figure):
            return _plotly_via_chrome(figure, fmt)
        case ChartSpec(tag="matplotlib", matplotlib=figure):
            return await to_process.run_sync(_matplotlib_savefig, figure, fmt.value)
        case _:
            assert_never(chart)


async def _vega_to_bytes(spec: dict[str, object], fmt: ExportFormat) -> bytes:
    settled = json.loads(await to_process.run_sync(_vegafusion_pre_transform, json.dumps(spec))) if _has_transforms(spec) else spec
    import vl_convert as vlc

    match fmt:
        case ExportFormat.SVG:
            return vlc.vegalite_to_svg(settled).encode()
        case ExportFormat.PNG:
            return vlc.vegalite_to_png(settled)
        case ExportFormat.PDF:
            return vlc.vegalite_to_pdf(settled)
        case _:
            assert_never(fmt)


def _plotly_via_chrome(figure: object, fmt: ExportFormat) -> bytes:
    import kaleido

    kaleido.get_chrome_sync()
    return kaleido.calc_fig_sync(figure, opts={"format": fmt.value})


def _vegafusion_pre_transform(spec_json: str) -> str:
    import vegafusion

    transformed, _warnings = vegafusion.runtime.pre_transform_spec(json.loads(spec_json))
    return json.dumps(transformed)


def _matplotlib_savefig(figure: object, fmt: str) -> bytes:
    import matplotlib

    matplotlib.use("Agg")
    sink = BytesIO()
    figure.savefig(sink, format=fmt)
    return sink.getvalue()


def _has_transforms(spec: dict[str, object]) -> bool:
    return "transform" in spec or any("transform" in layer for layer in spec.get("layer", ()) if isinstance(layer, dict))
```

## [04]-[RESEARCH]

No open items. The `vl-convert-python` `vegalite_to_svg`/`vegalite_to_png`/`vegalite_to_pdf`, the `kaleido` `get_chrome_sync`/`calc_fig_sync`, the `vegafusion` `runtime.pre_transform_spec`, and the `matplotlib` `Figure.savefig` Agg spellings each verify against the folder `.api` catalogue for the owning package. The `vegafusion` transform arm and the `matplotlib` render arm cross `anyio.to_process.run_sync` on the `python_version<'3.15'` band, importing the package at boundary scope inside the gated-band worker; the `vl-convert`/`kaleido` arms resolve on the cp315 core.
