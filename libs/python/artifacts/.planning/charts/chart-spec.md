# [PY_ARTIFACTS_CHART_SPEC]

The 2D chart-to-render axis. `ChartSpec` is ONE owner collapsing the three 2D engines — declarative (altair Vega-Lite), imperative (plotly graph_objects), and publication (matplotlib) — into one tagged union; `ChartExport` carries the host-free static-export rows. The host-free posture is the decisive axis: vl-convert-python is the primary Vega/Vega-Lite engine (Rust-native, embedded V8/deno_runtime, inlined Vega sources, zero browser, SVG-first then resvg/svg2pdf to PNG/PDF), matplotlib renders fully offscreen on the Agg/PDF/SVG backends, and kaleido is a host-Chrome-gated degraded path, never the default. Every render returns a `RuntimeRail[ContentKey]`.

## [1]-[INDEX]

[CLUSTERS]: two clusters — `[2]-[CHART]`, the 2D chart-spec union over the three engines; `[3]-[EXPORT]`, the host-free static-export backend rows.

## [2]-[CHART]

- Owner: `ChartSpec` the one chart axis discriminating engine; the declarative, imperative, and publication engines are cases on one owner, never a parallel per-engine chart class.
- Cases: `ChartSpec` cases `Vega(spec)` (altair Vega-Lite grammar / raw Vega-Lite dict) · `Plotly(figure)` (plotly graph_objects) · `Matplotlib(figure)` (matplotlib `Figure`) — matched by `match`/`case`; altair compiles to the Vega-Lite spec the export axis consumes directly.
- Entry: `ChartSpec.compose` builds the engine object; the result feeds `EXPORT` `ChartExport.render`.
- Packages: `altair` (`Chart`/`mark_*`/`encode`/`to_dict`), `plotly` (`graph_objects.Figure`), `matplotlib` (`Figure`/Agg backend), runtime.
- Growth: a new 2D engine is one `ChartSpec` case; zero new surface.
- Boundary: no live dashboard, UI event state, or browser runtime; 3D scientific scenes ride `scene3d/scene#SCENE`, not this axis; color palettes arrive from `color-management/colorimetry#COLOR`, never picked ad hoc per engine.

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

## [3]-[EXPORT]

- Owner: `ChartExport` the static-export dispatch over the chart case and target format; `ExportFormat` the closed `StrEnum`; the `_export_host_free` fold over the chart case IS the engine selection, vl-convert primary and kaleido host-Chrome-gated — no parallel engine enum.
- Cases: vega-via-`vl-convert-python` `vegalite_to_svg`/`vegalite_to_png`/`vegalite_to_pdf` (primary, host-free) · matplotlib-via-`Figure.savefig` on the Agg/PDF/SVG backends (host-free) · plotly-via-`kaleido` figure-to-image (gated behind detected host Chrome, never the default) — the engine is the chart-case fold arm, not a knob.
- Entry: `ChartExport.render` returns a `RuntimeRail[ContentKey]` and contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Chart`; `_export_host_free` folds the chart case to its engine — vega through vl-convert (host-free primary), matplotlib through `Figure.savefig` (host-free), plotly through `_plotly_via_chrome` (the gated kaleido path engaged only when host Chrome is detected, never the default).
- Packages: `vl-convert-python` (`vegalite_to_svg`/`vegalite_to_png`/`vegalite_to_pdf`), `kaleido` (figure-to-image, gated), `matplotlib` (`Figure.savefig`), runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`boundary`).
- Growth: a new export format is one `ExportFormat` row; a new host-free engine is one acceptor arm; zero new surface.
- Boundary: a per-backend export class family is the deleted form; kaleido v1 locates a host Chrome via its Choreographer driver, so it is the degraded optional path gated on host presence, never the primary; the export keys by one runtime content owner.

```python signature
from enum import StrEnum
from io import BytesIO
from typing import assert_never

import vl_convert as vlc
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary


class ExportFormat(StrEnum):
    SVG = "svg"
    PNG = "png"
    PDF = "pdf"


class ChartExport(Struct, frozen=True):
    chart: ChartSpec
    fmt: ExportFormat

    def render(self) -> RuntimeRail[ContentKey]:
        return boundary(f"chart.export.{self.fmt}", self._emit)

    def _emit(self) -> ContentKey:
        data = _export_host_free(self.chart, self.fmt)
        return ContentIdentity.key(f"chart-{self.fmt}", data)


def _export_host_free(chart: ChartSpec, fmt: ExportFormat) -> bytes:
    match chart:
        case ChartSpec(tag="vega", vega=spec):
            return _vega_to_bytes(spec, fmt)
        case ChartSpec(tag="matplotlib", matplotlib=figure):
            sink = BytesIO()
            figure.savefig(sink, format=fmt.value)
            return sink.getvalue()
        case ChartSpec(tag="plotly", plotly=figure):
            return _plotly_via_chrome(figure, fmt)
        case _:
            assert_never(chart)


def _vega_to_bytes(spec: dict[str, object], fmt: ExportFormat) -> bytes:
    match fmt:
        case ExportFormat.SVG:
            return vlc.vegalite_to_svg(spec).encode()
        case ExportFormat.PNG:
            return vlc.vegalite_to_png(spec)
        case ExportFormat.PDF:
            return vlc.vegalite_to_pdf(spec)
        case _:
            assert_never(fmt)
```

## [4]-[RESEARCH]

- [EXPORT_ENGINES]: the `vl-convert-python` `vegalite_to_svg`/`vegalite_to_png`/`vegalite_to_pdf` spellings verify against the branch `.api` catalogue for `vl-convert-python`; the `_plotly_via_chrome` gated body — the plotly figure-to-image call and the host-Chrome detection gate — resolves against `.api` for `plotly`/`kaleido`, requires plotly.py >=6.1.1 for the kaleido v1 Choreographer driver, and is the only export arm whose body is not transcription-complete until the catalogue lands; the matplotlib `Figure.savefig` Agg/PDF/SVG spellings confirm once the pillow image toolchain installs.
