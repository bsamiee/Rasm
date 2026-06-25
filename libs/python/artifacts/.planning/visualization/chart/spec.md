# [PY_ARTIFACTS_CHART_SPEC]

The chart-SPEC authoring half of the 2D chart axis. `ChartSpec` is ONE tagged union collapsing the host-free 2D engines — declarative-Vega (altair Vega-Lite), declarative-grammar (lets-plot grammar-of-graphics), and publication (matplotlib) — each case carrying its own palette-threaded payload, discriminated by one total `match` over the single `ChartEngineTag` literal with no parallel engine enum. The host-free posture is the decisive axis: vl-convert-python is the Vega/Vega-Lite engine (Rust-native, embedded V8/deno_runtime, inlined Vega sources, zero browser, SVG/PNG/PDF/HTML/JPEG/scenegraph), and lets-plot self-renders to bytes entirely in-process (bundled ImageMagick, no browser, no Node, no Vega binary), so two byte-identical content-keyable producers exist with zero Chrome dependency. The altair `Chart` builder owns the Vega-Lite grammar end-to-end — `mark_*` geometry, `encode` binding the typed `Color`/`Size`/`Shape`/`Tooltip` channels, `transform_*` data chains, `layer`/`hconcat`/`vconcat`/`concat`/`facet` composition into the `LayerChart`/`ConcatChart`/`FacetChart` roots, and `interactive`/`selection_point`/`selection_interval` selections — so the page never hand-builds a Vega-Lite dict where the builder is admitted. Color arrives from `graphic/color/derive#DERIVE` as the `ColorReceipt.coords` palette array, threaded into every engine theme — never picked ad hoc per engine. The themed result hands to `visualization/chart/export#EXPORT` `ChartExport.render`, which folds the chart case to bytes; the vegafusion data pre-pass lives at `visualization/chart/transform#TRANSFORM`.

## [01]-[INDEX]

- [01]-[CHART]: the 2D `ChartSpec` union over the host-free charting engines, each case threading the `graphic/color/derive#DERIVE` palette array through the one `ChartSpec.of` composer keyed on the admitted altair builder roots — the altair `Chart` builder mined to its mark/encode/transform/composition depth on the Vega arm.

## [02]-[CHART]

- Owner: `ChartSpec` the one chart axis, a frozen `tagged_union` whose `tag` carries the closed `ChartEngineTag` literal and whose every case carries its own palette-threaded payload — never a second `ChartEngine` enum re-describing the discriminant the union already owns. `ChartSpec.of` is the one composer threading the `graphic/color/derive#DERIVE` `ColorReceipt.coords` palette array into the case through one total `match` over `tag`, so the single-color-source invariant is real for charts — the host-free Vega arm folds the altair `Chart` to its Vega-Lite dict through `to_dict`, composes `Chart.interactive()` so the single-view spec carries pan/zoom selections the HTML row renders natively, binds the palette as the discrete `config.range.category` scale plus the `Color`-channel `Scale(range=...)` on the cp315 core, and the gated `lets_plot`/`matplotlib` cases carry the raw engine object plus the `Palette` array so the palette-thread runs on the gated band beside its render (`scale_color_manual`/`scale_fill_manual` for lets-plot, `colors.ListedColormap`/`colormaps.register` plus `Axes.set_prop_cycle` for matplotlib), never an ad-hoc per-engine color pick and never a gated import on the core.
- Cases: `ChartSpec` cases `Vega(spec)` (an altair `Chart`/`LayerChart`/`ConcatChart`/`FacetChart` with its full `mark_*`/`encode`/`transform_*`/`interactive`/`selection_*` grammar, or a raw Vega-Lite dict, folded to the palette-themed Vega-Lite spec dict the export axis consumes directly) · `LetsPlot(plot, palette)` (a lets-plot `PlotSpec` plus its `Palette` array self-rendering to bytes in-process on the gated band) · `Matplotlib(figure, palette)` (a matplotlib `Figure` plus its `Palette` array rendered on the gated subprocess seam) — matched by one total `match`/`case` over `tag`, the plotly/kaleido Chrome-gated case deleted as the host-free posture forbids it as default and vl-convert renders only Vega/Vega-Lite, never plotly.js.
- Entry: `ChartSpec.of(engine, palette)` is the one composer — `isinstance` over the admitted altair builder roots folds the Vega arm, the gated engine objects fold the `LetsPlot`/`Matplotlib` arms; the palette-themed result feeds `visualization/chart/export#EXPORT` `ChartExport.render`. No `ChartSpec.compose` forwarding hop and no parallel `ChartEngine.compose` fold survive — one composer, one discriminant.
- Packages: `altair` (`Chart`/`LayerChart`/`HConcatChart`/`VConcatChart`/`ConcatChart`/`FacetChart`; `encode`/`interactive`/`to_dict` the composed builder family, `mark_*`/`transform_*`/`properties` the growth-axis builder members; the `Color` channel, `Scale`/`Legend` guides composed in `_theme_vega`, the `X`/`Y`/`Size`/`Shape`/`Tooltip` channels and `selection_point`/`selection_interval`/`condition` selections and `layer`/`hconcat`/`vconcat`/`concat` composition operators on the caller-built grammar) on the cp315 core; `lets-plot` (`LetsPlot`/`ggplot`/`PlotSpec`, `scale_color_manual`/`scale_fill_manual`) and `matplotlib` (`Figure`/`colors.ListedColormap`/`colormaps.register`/`Axes.set_prop_cycle`) gated `python_version<'3.15'`; the palette-themed interactive Vega-Lite spec dict feeds `visualization/chart/export#EXPORT` where `visualization/chart/transform#TRANSFORM` pre-transforms it; runtime; `graphic/color/derive#DERIVE` (`ColorReceipt.coords`).
- Growth: a new host-free 2D engine is one `ChartSpec` case plus one `ChartSpec.of` match arm carrying its palette-thread, plus one `visualization/chart/export#EXPORT` `_export_host_free` band-routing arm carrying its own per-format row table; zero new surface. A new altair channel, mark, transform, composition operator, or selection is a builder call inside the Vega arm, never a new owner.
- Boundary: no live dashboard, UI event state, or browser runtime; no Chrome-gated engine — the plotly graph_objects case and the kaleido `get_chrome_sync`/`calc_fig_sync` figure-to-bytes path are the deleted forms, since every plotly static export is reachable only through kaleido's headless Chromium, the anti-axis the host-free posture forbids; 3D scientific scenes ride `scene#SCENE`, not this axis; color palettes arrive from `graphic/color/derive#DERIVE` as `ColorReceipt.coords`, never picked ad hoc per engine; the emitted chart bytes are handed to the regrouped `composition/compose#COMPOSE` placement owner, never composited or re-rendered on this axis.

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

`ChartSpec.of` is the one composer keyed by the admitted altair builder roots through `isinstance`-shaped `match` — no parallel `ChartEngine` enum and no `ChartSpec.compose` forwarding hop. The Vega arm folds the builder to its Vega-Lite dict through `to_dict`, composes `Chart.interactive()` on the single-view `Chart` so the emitted spec carries the pan/zoom selection the `vegalite_to_html` row renders without a browser, binds the palette through the `Color`-channel `Scale(range=...)` on that single view and as the `config.range.category`/`ordinal`/`ramp` discrete scales for every chart kind, so the single-color-source invariant holds whether the chart is one view or a `layer`/`concat`/`facet` composite. The gated `LetsPlot`/`Matplotlib` cases carry the raw engine object plus the `Palette` array so the palette-thread runs on the gated band beside its render — the matplotlib `Figure` and the lets-plot `PlotSpec` never construct on the cp315 core, and `hex_ramp` is the one shared RGB-to-hex projection both bands reach. The export half at `visualization/chart/export#EXPORT` reuses `hex_ramp` for the gated lets-plot/matplotlib palette-thread, so the RGB-to-hex projection is declared once on this spec page and imported there.

## [03]-[RESEARCH]

- [ALTAIR_COMPOSE]: the altair `Chart`/`LayerChart`/`HConcatChart`/`VConcatChart`/`ConcatChart`/`FacetChart` builder roots, the `Chart.mark_*`/`encode`/`transform_*`/`interactive`/`properties`/`to_dict` builder family, the `X`/`Y`/`Color`/`Size`/`Shape`/`Tooltip` channel constructors, the `Scale`/`Axis`/`Legend` guides, the `selection_point`/`selection_interval`/`condition` selections, and the `layer`/`hconcat`/`vconcat`/`concat` composition operators verify against the folder `.api/altair.md` catalogue (`[02]-[PUBLIC_TYPES]` chart/composition roots rows [01]-[06], encoding rows [01]-[08]; `[03]-[ENTRYPOINTS]` construction rows [01]-[06], composition rows [01]-[07]). The `_theme_vega` palette-thread binds the `Color("category:N", scale=Scale(range=ramp), legend=Legend(title=None))` channel and composes `Chart.interactive()` on the single-view `Chart` so the emitted spec carries the pan/zoom selection the HTML row renders, plus the `config.range.category`/`ordinal`/`ramp` discrete scales for every builder root, so the single-color-source invariant holds across one view, a `layer`/`concat`/`facet` composite, or a raw Vega-Lite dict — the catalogue settles `to_dict` as the spec-emit surface (`[03]-[ENTRYPOINTS]` composition row [01]), `interactive` as the pan/zoom enable (`[03]-[ENTRYPOINTS]` construction row [05]), and `encode` plus the `Color`/`Scale`/`Legend` value objects as the channel surface, so every `_theme_vega` spelling including the `Chart.interactive()` call is settled altair fence code other than the `config.range.ordinal`/`ramp` scale keys. The `config.range.ordinal`/`ramp` Vega-Lite scale-name keys are the one catalogue-independent Vega-Lite-config item the altair catalogue does not enumerate (they are Vega-Lite config, not an altair member), settled against the Vega-Lite config schema rather than a package member.
