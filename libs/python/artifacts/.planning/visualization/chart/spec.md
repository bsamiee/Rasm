# [PY_ARTIFACTS_CHART_SPEC]

The chart-SPEC authoring half of the 2D chart axis. `ChartSpec` is ONE tagged union collapsing the host-free 2D engines — declarative-Vega (altair Vega-Lite), declarative-grammar (lets-plot grammar-of-graphics), and publication (matplotlib) — each case carrying its own palette-threaded payload, discriminated by one total `match` over the single `ChartEngineTag` literal with no parallel engine enum. The host-free posture is the decisive axis: vl-convert-python is the Vega/Vega-Lite engine (Rust-native, embedded V8/deno_runtime, inlined Vega sources, zero browser, SVG/PNG/PDF/HTML/JPEG/scenegraph), and lets-plot self-renders to bytes entirely in-process (no browser, no Node, no Vega binary), so two byte-identical content-keyable producers exist with zero Chrome dependency. The altair `Chart` builder owns the Vega-Lite grammar end-to-end — the `mark_*` geometry (including the statistical `mark_boxplot`/`mark_errorbar`/`mark_errorband` and the geographic `mark_geoshape`), `encode` binding the typed `Color`/`Size`/`Shape`/`Theta`/`Radius`/`Latitude`/`Longitude`/`Tooltip` channels, the `transform_*` data chains (including the server-side statistical `transform_regression`/`loess`/`density`/`quantile`/`window`), the `project` d3-geo projection with its `graticule`/`sphere`/`topo_feature` generators, the `param`/`selection_point`/`selection_interval` + `when`/`condition` + `binding_range`/`binding_select` interaction algebra, the `configure_*` publication config, and the `layer`/`hconcat`/`vconcat`/`concat`/`facet`/`repeat` composition — so the page never hand-builds a Vega-Lite dict where the builder is admitted, and `ChartSpec.of` themes and emits whatever grammar the consumer's builder carries without clobbering it. Color arrives from `graphic/color/derive#DERIVE` as the `ColorReceipt.coords` palette array, threaded through the ONE typed theme `config.range` binding into every engine — never picked ad hoc per engine and never overwriting a consumer's own color encoding. The themed result hands to `visualization/chart/export#EXPORT` `ChartExport.render`; the vegafusion data pre-pass lives at `visualization/chart/transform#TRANSFORM`.

## [01]-[INDEX]

- [01]-[CHART]: the 2D `ChartSpec` union over the host-free charting engines, each case threading the `graphic/color/derive#DERIVE` palette array through the one `ChartSpec.of` composer keyed on the admitted altair builder roots — the altair `Chart` builder mined to its mark/encode/transform/project/interaction/composition depth on the Vega arm, the palette bound through one typed `alt.theme.ConfigKwds` `config.range` correspondence, and the `to_dict` schema validation folded onto the `expression.Result` rail so `of` returns `Result[ChartSpec, SpecFault]`.

## [02]-[CHART]

- Cases: `ChartSpec` cases `Vega(spec)` (an altair `Chart`/`LayerChart`/`ConcatChart`/`FacetChart`/`RepeatChart` with its full `mark_*`/`encode`/`transform_*`/`project`/`param`/`selection_*`/`when`/`configure_*` grammar, or a raw Vega-Lite dict, folded to the palette-themed Vega-Lite spec dict the export axis consumes directly) · `LetsPlot(plot, palette)` (a lets-plot `PlotSpec` plus its `Palette` array self-rendering to bytes in-process on the worker lane) · `Matplotlib(figure, palette)` (a matplotlib `Figure` plus its `Palette` array rendered on the gated subprocess seam) — matched by one total `match`/`case` over `tag`, the plotly/kaleido Chrome-gated case deleted as the host-free posture forbids it as default and vl-convert renders only Vega/Vega-Lite, never plotly.js.
- Entry: `ChartSpec.of(engine, palette)` is the one composer returning `Result[ChartSpec, SpecFault]` — one total `match` over the input shape: the admitted altair builder roots fold the Vega arm through `_theme_vega` (whose `to_dict(validate=True)` SchemaValidationError rails onto `<invalid-spec>`), a raw `dict` (a consumer-materialized Vega-Lite spec with no `to_dict`) palette-threads directly through `_themed` onto the Vega arm, the `lets_plot`/`matplotlib` module-probe arms fold `LetsPlot`/`Matplotlib` as `Ok`, and a non-chart object rails `<unknown-engine>` at admission rather than the prior catch-all that silently misrouted every unknown object to the matplotlib arm (where it would fail confusingly at render); the palette-themed result feeds `visualization/chart/export#EXPORT` `ChartExport.render`. No `ChartSpec.compose` forwarding hop and no parallel `ChartEngine.compose` fold survive — one composer, one discriminant.
- Growth: a new host-free 2D engine is one `ChartSpec` case plus one `ChartSpec.of` match arm carrying its palette-thread, plus one `visualization/chart/export#EXPORT` `_export_host_free` band-routing arm; zero new surface. A new altair channel, mark, transform, projection, selection, or configure block is a builder call inside the consumer's `Chart` the Vega arm preserves through `to_dict`, never a new owner or a new spec case.
- Boundary: no live dashboard, UI event state, or browser runtime; no Chrome-gated engine — the plotly `graph_objects` case and the kaleido headless-Chromium path are the deleted forms; 3D scientific scenes ride `scene#SCENE`, not this axis; color palettes arrive from `graphic/color/derive#DERIVE` as `ColorReceipt.coords`, never picked ad hoc per engine and never overwriting a consumer's color encoding (the `config.range` binding sets the discrete/continuous scale ranges globally, so it threads a geoshape choropleth, a polar arc, a statistical error band, and a layered composite alike without touching their per-view color fields); the emitted chart bytes are handed to the regrouped `composition/compose#COMPOSE` placement owner, never composited or re-rendered on this axis. The prior forced `Chart.interactive()` pan/zoom and forced `Color("category:N")` encode are the deleted lower-capability forms — the first suppressed the consumer's named `param`/`selection_*`/`binding_*` selections the HTML row renders, the second clobbered a consumer's own color encoding.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Literal

import altair as alt
import numpy as np
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.extra.result import catch
from numpy.typing import NDArray

from altair.utils.schemapi import SchemaValidationError

# --- [TYPES] ----------------------------------------------------------------------------
type Palette = NDArray[np.float64]
type ChartEngineTag = Literal["vega", "lets_plot", "matplotlib"]
type SpecFault = Literal["<invalid-spec>", "<unknown-engine>"]
type VegaBuilder = alt.Chart | alt.LayerChart | alt.HConcatChart | alt.VConcatChart | alt.ConcatChart | alt.FacetChart | alt.RepeatChart


# --- [MODELS] ---------------------------------------------------------------------------
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
    def of(engine: object, palette: Palette) -> "Result[ChartSpec, SpecFault]":
        match engine:
            case alt.Chart() | alt.LayerChart() | alt.HConcatChart() | alt.VConcatChart() | alt.ConcatChart() | alt.FacetChart() | alt.RepeatChart():
                return _theme_vega(engine, palette).map(ChartSpec.Vega)
            case dict() as raw:                                     # a consumer-materialized Vega-Lite dict — palette-thread it directly, no `to_dict`
                return Ok(ChartSpec.Vega(_themed(raw, palette)))
            case _ if type(engine).__module__.startswith("lets_plot"):
                return Ok(ChartSpec.LetsPlot(engine, palette))
            case _ if type(engine).__module__.startswith("matplotlib"):
                return Ok(ChartSpec.Matplotlib(engine, palette))
            case _:
                return Error("<unknown-engine>")                   # reject a non-chart object at admission rather than misroute it to the matplotlib arm


# --- [OPERATIONS] -----------------------------------------------------------------------
def _themed(spec: dict[str, object], palette: Palette) -> dict[str, object]:
    # thread the ONE palette through a DEEP-merged typed `config.range` correspondence — the palette OWNS every
    # color-scale range (category/ordinal discrete, ramp/heatmap sequential, diverging bipolar) so one source threads
    # a nominal legend, a geoshape choropleth, a heatmap, a diverging map, a polar arc, and a layer/concat/facet
    # composite alike; the deep merge preserves a consumer's own non-color range slot (`symbol` shape) and every
    # sibling config block (axis/legend/view), never a per-view `Color` encode that clobbers a consumer's color field.
    ramp = hex_ramp(palette)
    prior_config = cfg if isinstance(cfg := spec.get("config"), dict) else {}
    prior_range = rng if isinstance(rng := prior_config.get("range"), dict) else {}
    config: alt.theme.ConfigKwds = {"range": {**prior_range, "category": ramp, "ordinal": ramp, "ramp": ramp, "heatmap": ramp, "diverging": ramp}}
    return {**spec, "config": {**prior_config, **config}}


def _theme_vega(builder: VegaBuilder, palette: Palette) -> Result[dict[str, object], SpecFault]:
    return (
        catch(exception=SchemaValidationError)(builder.to_dict)(validate=True)
        .map_error(lambda _fault: "<invalid-spec>")
        .map(lambda resolved: _themed(resolved, palette))
    )


def hex_ramp(palette: Palette) -> list[str]:
    return [f"#{int(r * 255):02x}{int(g * 255):02x}{int(b * 255):02x}" for r, g, b in np.clip(palette, 0.0, 1.0)]
```

`ChartSpec.of` is the one composer keyed by the input shape through a total `match` — no parallel `ChartEngine` enum and no `ChartSpec.compose` forwarding hop — returning `Result[ChartSpec, SpecFault]` so the `to_dict` schema validation is a typed rail, never a raise into the composer, and a non-chart object rails `<unknown-engine>` rather than misrouting to matplotlib. Both the builder arm and the raw-`dict` arm converge on `_themed`, the ONE palette-thread that DEEP-merges a typed `alt.theme.ConfigKwds` `config.range` correspondence into the resolved spec: the palette owns every color-scale range Vega-Lite applies — `category`/`ordinal` discrete, `ramp`/`heatmap` sequential, `diverging` bipolar — so the single-color-source invariant holds whether the chart is one view, a `mark_geoshape` choropleth, a `mark_rect` heatmap, a diverging anomaly map, a polar `mark_arc`, a statistical `mark_errorband`, or a `layer`/`concat`/`facet` composite, while the deep merge preserves the consumer's own non-color range slot (`symbol` shape) and every sibling `config` block (axis/legend/view) rather than clobbering the whole `config.range` — and WITHOUT overwriting a per-view `Color` field or suppressing named selections. The hand-merged raw `config.range` dict is replaced by the typed `ConfigKwds` the altair catalogue admits, and the bare `builder.to_dict()` by the `catch(SchemaValidationError)`-railed form the catalogue `[04]` evidence row folds onto `expression.Result`. The gated `LetsPlot`/`Matplotlib` cases carry the raw engine object plus the `Palette` array so the palette-thread runs on the worker lane beside its render — the matplotlib `Figure` and the lets-plot `PlotSpec` never construct on the runtime, and `hex_ramp` is the one shared RGB-to-hex projection both bands reach, declared once here and imported by `visualization/chart/export#EXPORT` for the gated lets-plot/matplotlib palette-thread.

The consumer builds the Vega arm's grammar to journal-and-AEC depth through the admitted altair builder, and `_theme_vega` preserves every fragment through `to_dict`: the statistical overlay (`Chart.transform_regression(method=...)`/`transform_loess`/`transform_density`/`transform_quantile`/`transform_window` server-side fits + `mark_errorbar`/`mark_errorband`/`mark_boxplot` uncertainty marks — never a hand-rolled numpy fit pre-baked into the data), the geographic plane (`Chart.project(type=...)` d3-geo + the `graticule`/`sphere`/`topo_feature` generators + the `Latitude`/`Longitude` channels driving `mark_geoshape` for a site plan or choropleth), the interaction algebra (`param`/`selection_point`/`selection_interval` registered through `add_params`, `when().then().otherwise()`/`condition` conditional encodings, and `binding_range`/`binding_select` UI controls the `vegalite_to_html` row renders live), the polar `Theta`/`Radius` channels for pie/arc, the 55-arm `configure_*` publication config, and the `expr` Vega-expression algebra + the narwhals-backed `transformed_data` local pre-execution — all builder calls inside the consumer's `Chart`, never new spec cases and never re-implemented here.

## [03]-[RESEARCH]

- [ALTAIR_COMPOSE] [RESOLVED]: the altair `Chart`/`LayerChart`/`HConcatChart`/`VConcatChart`/`ConcatChart`/`FacetChart`/`RepeatChart` builder roots, the `mark_*` family (incl. `mark_errorbar`/`mark_errorband`/`mark_boxplot`/`mark_geoshape`, verified as `Chart` methods), the `transform_*` family (incl. `transform_regression`/`transform_loess`/`transform_density`/`transform_quantile`/`transform_window`, verified `Chart` methods), the `X`/`Y`/`Color`/`Theta`/`Radius`/`Latitude`/`Longitude` channels (verified module-level), the `project` d3-geo configurator + `graticule`/`sphere`/`topo_feature` generators (verified), the `param`/`selection_point`/`selection_interval`/`when`/`condition`/`binding_range`/`binding_select` interaction algebra (verified module-level), the `configure_axis`/`configure_legend`/`configure_view`/`configure_range` family (verified `Chart` methods), and `expr`/`transformed_data` (verified) compose against `.api/altair.md` — the `of` composer matches all seven roots so a `repeat`/`facet` composite folds into the Vega arm rather than the matplotlib catch-all. Every one is a builder call the consumer makes and `to_dict` preserves; the page owns only the palette-thread and the emit rail, never a new spec case per grammar family (Growth: an altair channel/mark/transform/projection/selection is a builder call, never a new owner).
- [TYPED_THEME] [RESOLVED]: the palette threads through a typed `alt.theme.ConfigKwds` `config.range` correspondence (verified `alt.theme.ConfigKwds`/`RangeConfigKwds`/`ThemeConfig` present) rather than the prior hand-merged raw `config.range` dict the catalogue `[04]` rejects — the `category`/`ordinal` discrete, `ramp`/`heatmap` sequential, and `diverging` bipolar ranges bind EVERY color-scale kind Vega-Lite resolves, so the single-color-source invariant holds across nominal/geo/heatmap/diverging/polar/statistical/composite charts without a per-view `Color("category:N")` encode that would clobber a consumer's own color field. `_themed` DEEP-merges the range sub-dict (`{**prior_range, ...palette slots}`) so a consumer's own non-color range slot (`symbol` shape) and sibling `config` blocks survive rather than being wiped by a wholesale `config.range` replacement, and it is the ONE merge site both the builder arm (post-`to_dict`) and the raw-`dict` arm reach. The `@alt.theme.register` global-registry path is the alternative for a process-wide named theme; the inline typed `config` merge is the per-palette form that carries the color source without a stateful global registration.
- [VALIDATE_RAIL] [RESOLVED]: `_theme_vega` folds `builder.to_dict(validate=True)` through `catch(exception=SchemaValidationError)` (verified `altair.utils.schemapi.SchemaValidationError` raises under `validate=True`, and `catch` returns a `Result`) and `map_error`s onto `<invalid-spec>`, so a malformed spec yields a typed `Error` case rather than raising into the composer — the catalogue `[04]` evidence row's `SchemaValidationError`-onto-`expression.Result` fold the prior bare `builder.to_dict()` omitted. `ChartSpec.of` therefore returns `Result[ChartSpec, SpecFault]`; the Vega arm's `_theme_vega` result maps onto `ChartSpec.Vega`, the gated arms are `Ok`, and the `config.range.ordinal`/`ramp` keys are the one Vega-Lite-config item the altair catalogue does not enumerate (they are Vega-Lite config, settled against the Vega-Lite config schema, carried inside the typed `ConfigKwds`).
