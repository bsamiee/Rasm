# [PY_ARTIFACTS_CHART_SPEC]

The chart-SPEC authoring half of the 2D chart axis. `ChartSpec` is ONE tagged union collapsing the host-free 2D engines — declarative-Vega (altair Vega-Lite), declarative-grammar (lets-plot grammar-of-graphics), and publication (matplotlib) — each case carrying its own palette-threaded payload, discriminated by one total `match` over the single `ChartEngineTag` literal with no parallel engine enum. The host-free posture is the decisive axis: vl-convert-python is the Vega/Vega-Lite engine (Rust-native, embedded V8/deno_runtime, inlined Vega sources, zero browser, SVG/PNG/PDF/HTML/JPEG/scenegraph), and lets-plot self-renders to bytes entirely in-process (no browser, no Node, no Vega binary), so two byte-identical content-keyable producers exist with zero Chrome dependency. The altair `Chart` builder owns the Vega-Lite grammar end-to-end — the `mark_*` geometry (including the statistical `mark_boxplot`/`mark_errorbar`/`mark_errorband` and the geographic `mark_geoshape`), `encode` binding the typed `Color`/`Size`/`Shape`/`Theta`/`Radius`/`Latitude`/`Longitude`/`Tooltip` channels, the `transform_*` data chains (including the server-side statistical `transform_regression`/`loess`/`density`/`quantile`/`window`), the `project` d3-geo projection with its `graticule`/`sphere`/`topo_feature` generators, the `param`/`selection_point`/`selection_interval` + `when`/`condition` + `binding_range`/`binding_select` interaction algebra, the `configure_*` publication config, and the `layer`/`hconcat`/`vconcat`/`concat`/`facet`/`repeat` composition — so the page never hand-builds a Vega-Lite dict where the builder is admitted, and `ChartSpec.of` themes and emits whatever grammar the consumer's builder carries without clobbering it. Color arrives from `graphic/color/derive#DERIVE` as the `ColorReceipt.coords` palette array carried on the one `ChartTheme` owner beside its typed `alt.theme.*Kwds` annotation-styling blocks, and threads through the ONE typed `config` binding into every engine — the palette owning `config.range` (the single color source, never picked ad hoc per engine and never overwriting a consumer's own color encoding) and the typed `axis`/`legend`/`view`/`title`/`mark` blocks owning the drawing-sheet/ISO-3098-lettering annotation styling a journal figure needs, each deep-merged over the consumer's own block rather than left as an untyped preserved dict. The themed result hands to `visualization/chart/export#EXPORT` `ChartExport.render`; the vegafusion data pre-pass and the narwhals-backed `transformed_data` local pre-execution both live at `visualization/chart/transform#TRANSFORM`, never on this authoring axis.

## [01]-[INDEX]

- [01]-[CHART]: the 2D `ChartSpec` union over the host-free charting engines, each case threading the `graphic/color/derive#DERIVE` palette array — carried on the one `ChartTheme` owner — through the one `ChartSpec.of` composer keyed on the admitted altair builder roots — the altair `Chart` builder mined to its mark/encode/transform/project/interaction/composition/resolve depth on the Vega arm, the theme bound through one typed `alt.theme.ConfigKwds` correspondence (the palette owning `config.range` via the typed `RangeConfigKwds` sub-dict, the typed `axis`/`legend`/`view`/`title`/`mark` `*Kwds` blocks owning the annotation styling), and the `to_dict` schema validation folded onto the `expression.Result` rail so `of` returns `Result[ChartSpec, SpecFault]`.

## [02]-[CHART]

- Cases: `ChartSpec` cases `Vega(spec)` (an altair `Chart`/`LayerChart`/`ConcatChart`/`FacetChart`/`RepeatChart` with its full `mark_*`/`encode`/`transform_*`/`project`/`param`/`selection_*`/`when`/`configure_*` grammar, or a raw Vega-Lite dict, folded to the palette-themed Vega-Lite spec dict the export axis consumes directly) · `LetsPlot(plot, palette)` (a lets-plot `PlotSpec` plus its `Palette` array self-rendering to bytes in-process on the worker lane) · `Matplotlib(figure, palette)` (a matplotlib `Figure` plus its `Palette` array rendered on the gated subprocess seam) — matched by one total `match`/`case` over `tag`, the plotly/kaleido Chrome-gated case deleted as the host-free posture forbids it as default and vl-convert renders only Vega/Vega-Lite, never plotly.js.
- Entry: `ChartSpec.of(engine, theme)` is the one composer returning `Result[ChartSpec, SpecFault]` — one total `match` over the input shape: the admitted altair builder roots fold the Vega arm through `_theme_vega` (whose `to_dict(validate=True)` SchemaValidationError rails onto `<invalid-spec>`), a raw `dict` (a consumer-materialized Vega-Lite spec with no `to_dict`) theme-threads directly through `_themed` onto the Vega arm, the `lets_plot`/`matplotlib` module-probe arms fold `LetsPlot`/`Matplotlib` as `Ok` carrying `theme.palette` (their annotation styling threads through their own export egress), and a non-chart object rails `<unknown-engine>` at admission rather than the prior catch-all that silently misrouted every unknown object to the matplotlib arm (where it would fail confusingly at render); the theme-styled result feeds `visualization/chart/export#EXPORT` `ChartExport.render`. No `ChartSpec.compose` forwarding hop and no parallel `ChartEngine.compose` fold survive — one composer, one discriminant. A palette-only caller constructs `ChartTheme(palette=coords)` (every sibling block `None`), so the color-only path is byte-identical to the prior `of(engine, palette)` form while a journal/ISO-3098 caller passes the typed `axis`/`legend`/`title` styling on the same owner.
- Growth: a new host-free 2D engine is one `ChartSpec` case plus one `ChartSpec.of` match arm carrying its palette-thread, plus one `visualization/chart/export#EXPORT` `_export_host_free` band-routing arm; zero new surface. A new altair channel, mark, transform, projection, selection, configure, or `resolve_*` block is a builder call inside the consumer's `Chart` the Vega arm preserves through `to_dict`, never a new owner or a new spec case. A new typed annotation-styling axis is one `ChartTheme` `alt.theme.*Kwds` field threaded through `_themed`'s `_merged` fold, never a per-block merge site.
- Boundary: no live dashboard, UI event state, or browser runtime; no Chrome-gated engine — the plotly `graph_objects` case and the kaleido headless-Chromium path are the deleted forms; 3D scientific scenes ride `scene#SCENE`, not this axis; color palettes arrive from `graphic/color/derive#DERIVE` as `ColorReceipt.coords`, never picked ad hoc per engine and never overwriting a consumer's color encoding (the `config.range` binding sets the discrete/continuous scale ranges globally, so it threads a geoshape choropleth, a polar arc, a statistical error band, and a layered composite alike without touching their per-view color fields); the emitted chart bytes are handed to the regrouped `composition/compose#COMPOSE` placement owner, never composited or re-rendered on this axis. The prior forced `Chart.interactive()` pan/zoom and forced `Color("category:N")` encode are the deleted lower-capability forms — the first suppressed the consumer's named `param`/`selection_*`/`binding_*` selections the HTML row renders, the second clobbered a consumer's own color encoding.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Literal

import altair as alt
import numpy as np
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.extra.result import catch
from msgspec import Struct
from numpy.typing import NDArray

from altair.utils.schemapi import SchemaValidationError

# --- [TYPES] ----------------------------------------------------------------------------
type Palette = NDArray[np.float64]
type ChartEngineTag = Literal["vega", "lets_plot", "matplotlib"]
type SpecFault = Literal["<invalid-spec>", "<unknown-engine>"]
type VegaBuilder = alt.Chart | alt.LayerChart | alt.HConcatChart | alt.VConcatChart | alt.ConcatChart | alt.FacetChart | alt.RepeatChart


# --- [MODELS] ---------------------------------------------------------------------------
class ChartTheme(Struct, frozen=True):
    # the ONE journal-grade-AND-ISO-3098 theme owner: the palette (the single color source) PLUS the typed
    # annotation-styling blocks a drawing-sheet figure and a book-typeset figure both need. `palette` threads to
    # every engine (config.range on Vega, scale_*_manual on lets-plot, the prop-cycle on matplotlib); the typed
    # `alt.theme.*Kwds` blocks thread ONLY the Vega arm (lets-plot/matplotlib theme through their own egress in
    # visualization/chart/export). A palette-only caller constructs `ChartTheme(palette=coords)` and the sibling
    # blocks stay `None`, so the color-only path is byte-identical to the prior palette-only form.
    palette: Palette
    axis: "alt.theme.AxisConfigKwds | None" = None    # ISO 3098 lettering / journal tick+label+title+grid axis styling — the typed sibling block the prior _themed left as an untyped preserved dict
    legend: "alt.theme.LegendConfigKwds | None" = None
    view: "alt.theme.ViewConfigKwds | None" = None
    title: "alt.theme.TitleConfigKwds | None" = None
    mark: "alt.theme.MarkConfigKwds | None" = None


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
    def of(engine: object, theme: "ChartTheme") -> "Result[ChartSpec, SpecFault]":
        match engine:
            case alt.Chart() | alt.LayerChart() | alt.HConcatChart() | alt.VConcatChart() | alt.ConcatChart() | alt.FacetChart() | alt.RepeatChart():
                return _theme_vega(engine, theme).map(ChartSpec.Vega)
            case dict() as raw:                                     # a consumer-materialized Vega-Lite dict — theme-thread it directly, no `to_dict`
                return Ok(ChartSpec.Vega(_themed(raw, theme)))
            case _ if type(engine).__module__.startswith("lets_plot"):
                return Ok(ChartSpec.LetsPlot(engine, theme.palette))  # lets-plot themes its own annotation styling; only the color source crosses
            case _ if type(engine).__module__.startswith("matplotlib"):
                return Ok(ChartSpec.Matplotlib(engine, theme.palette))
            case _:
                return Error("<unknown-engine>")                   # reject a non-chart object at admission rather than misroute it to the matplotlib arm


# --- [OPERATIONS] -----------------------------------------------------------------------
def _merged(prior_config: dict[str, object], key: str, block: object) -> dict[str, object]:
    # deep-merge ONE typed `alt.theme.*Kwds` sibling block over the consumer's own prior block so a journal/ISO-3098
    # axis/legend/view/title/mark styling threads WITHOUT wiping what the consumer already set; a None theme block
    # contributes nothing, so the palette-only path emits no sibling block at all.
    if block is None:
        return {}
    prior_block = pb if isinstance(pb := prior_config.get(key), dict) else {}
    return {key: {**prior_block, **block}}


def _themed(spec: dict[str, object], theme: ChartTheme) -> dict[str, object]:
    # thread the ChartTheme through one DEEP-merged typed `config` correspondence. The palette OWNS every color-scale
    # range via the typed `RangeConfigKwds` sub-dict (category/ordinal discrete, ramp/heatmap sequential, diverging
    # bipolar) so one source threads a nominal legend, a geoshape choropleth, a heatmap, a diverging map, a polar arc,
    # and a layer/concat/facet composite alike; the typed `axis`/`legend`/`view`/`title`/`mark` blocks the theme carries
    # thread the annotation styling a drawing-sheet/journal figure needs — each deep-merged over the consumer's own
    # block via `_merged` so the range slot AND every sibling block survive, never a per-view `Color` encode clobbering
    # a consumer's color field and never an untyped dict where the admitted `*Kwds` family types the theme.
    ramp = hex_ramp(theme.palette)
    prior_config = cfg if isinstance(cfg := spec.get("config"), dict) else {}
    prior_range = rng if isinstance(rng := prior_config.get("range"), dict) else {}
    range_block: alt.theme.RangeConfigKwds = {**prior_range, "category": ramp, "ordinal": ramp, "ramp": ramp, "heatmap": ramp, "diverging": ramp}
    typed: alt.theme.ConfigKwds = {
        "range": range_block,
        **_merged(prior_config, "axis", theme.axis), **_merged(prior_config, "legend", theme.legend),
        **_merged(prior_config, "view", theme.view), **_merged(prior_config, "title", theme.title),
        **_merged(prior_config, "mark", theme.mark),
    }
    return {**spec, "config": {**prior_config, **typed}}


def _theme_vega(builder: VegaBuilder, theme: ChartTheme) -> Result[dict[str, object], SpecFault]:
    return (
        catch(exception=SchemaValidationError)(builder.to_dict)(validate=True)
        .map_error(lambda _fault: "<invalid-spec>")
        .map(lambda resolved: _themed(resolved, theme))
    )


def hex_ramp(palette: Palette) -> list[str]:
    return [f"#{int(r * 255):02x}{int(g * 255):02x}{int(b * 255):02x}" for r, g, b in np.clip(palette, 0.0, 1.0)]
```

`ChartSpec.of` is the one composer keyed by the input shape through a total `match` — no parallel `ChartEngine` enum and no `ChartSpec.compose` forwarding hop — returning `Result[ChartSpec, SpecFault]` so the `to_dict` schema validation is a typed rail, never a raise into the composer, and a non-chart object rails `<unknown-engine>` rather than misrouting to matplotlib. Both the builder arm and the raw-`dict` arm converge on `_themed`, the ONE theme-thread that DEEP-merges a typed `alt.theme.ConfigKwds` correspondence into the resolved spec: the palette owns every color-scale range Vega-Lite applies through the typed `RangeConfigKwds` sub-dict — `category`/`ordinal` discrete, `ramp`/`heatmap` sequential, `diverging` bipolar — so the single-color-source invariant holds whether the chart is one view, a `mark_geoshape` choropleth, a `mark_rect` heatmap, a diverging anomaly map, a polar `mark_arc`, a statistical `mark_errorband`, or a `layer`/`concat`/`facet` composite; the typed `axis`/`legend`/`view`/`title`/`mark` `*Kwds` blocks the `ChartTheme` carries author the drawing-sheet/ISO-3098-lettering annotation styling — a journal-grade typed axis tick+label font and legend layout, not an untyped dict — each folded through `_merged` so the range slot AND every sibling block deep-merge over the consumer's own `symbol` shape and prior `axis`/`legend`/`view` rather than clobbering either, and WITHOUT overwriting a per-view `Color` field or suppressing named selections. The prior untyped-preserved sibling blocks are replaced by the typed `*Kwds` family the altair catalogue admits, the hand-merged raw `config.range` by the typed `RangeConfigKwds`, and the bare `builder.to_dict()` by the `catch(SchemaValidationError)`-railed form the catalogue `[04]` evidence row folds onto `expression.Result`. The gated `LetsPlot`/`Matplotlib` cases carry the raw engine object plus the `Palette` array (`theme.palette`) so the palette-thread runs on the worker lane beside its render — the matplotlib `Figure` and the lets-plot `PlotSpec` never construct on the runtime, and `hex_ramp` is the one shared RGB-to-hex projection both bands reach, declared once here and imported by `visualization/chart/export#EXPORT` for the gated lets-plot/matplotlib palette-thread and by `visualization/diagram/draw#DRAW` for the diagram palette.

The consumer builds the Vega arm's grammar to journal-and-AEC depth through the admitted altair builder, and `_theme_vega` preserves every fragment through `to_dict`: the statistical overlay (`Chart.transform_regression(method=...)`/`transform_loess`/`transform_density`/`transform_quantile`/`transform_window` server-side fits + `mark_errorbar`/`mark_errorband`/`mark_boxplot` uncertainty marks — never a hand-rolled numpy fit pre-baked into the data), the geographic plane (`Chart.project(type=...)` d3-geo + the `graticule`/`sphere`/`topo_feature` generators + the `Latitude`/`Longitude` channels driving `mark_geoshape` for a site plan or choropleth), the interaction algebra (`param`/`selection_point`/`selection_interval` registered through `add_params`, `when().then().otherwise()`/`condition` conditional encodings, and `binding_range`/`binding_select` UI controls the `vegalite_to_html` row renders live), the polar `Theta`/`Radius` channels for pie/arc, the composite-view guide resolution (`resolve_scale`/`resolve_axis`/`resolve_legend` — the shared-vs-independent scale/axis/legend conflict resolution a `layer`/`concat`/`facet`/`repeat` composite carries, preserved verbatim through `to_dict` alongside the composition operators themselves), the 55-arm `configure_*` publication config, and the `expr` Vega-expression algebra — all builder calls inside the consumer's `Chart`, never new spec cases and never re-implemented here. Data pre-execution — both the narwhals-backed `Chart.transformed_data` local frame and the vegafusion server-side reduction — is the `visualization/chart/transform#TRANSFORM` plane, never this authoring axis, so this page composes no `transformed_data` call and asserts no data-execution capability its fence does not implement.

## [03]-[RESEARCH]

- [ALTAIR_COMPOSE] [RESOLVED]: the altair `Chart`/`LayerChart`/`HConcatChart`/`VConcatChart`/`ConcatChart`/`FacetChart`/`RepeatChart` builder roots, the `mark_*` family (incl. `mark_errorbar`/`mark_errorband`/`mark_boxplot`/`mark_geoshape`, verified as `Chart` methods), the `transform_*` family (incl. `transform_regression`/`transform_loess`/`transform_density`/`transform_quantile`/`transform_window`, verified `Chart` methods), the `X`/`Y`/`Color`/`Theta`/`Radius`/`Latitude`/`Longitude` channels (verified module-level), the `project` d3-geo configurator + `graticule`/`sphere`/`topo_feature` generators (verified), the `param`/`selection_point`/`selection_interval`/`when`/`condition`/`binding_range`/`binding_select` interaction algebra (verified module-level), the `configure_axis`/`configure_legend`/`configure_view`/`configure_range` family (verified `Chart` methods), and the `resolve_scale`/`resolve_axis`/`resolve_legend` composite-view guide resolution (verified `Chart` methods, `.api/altair.md` entry [12]) compose against `.api/altair.md` — the `of` composer matches all seven roots so a `repeat`/`facet` composite folds into the Vega arm rather than the matplotlib catch-all, carrying its `resolve_*` shared-vs-independent guide policy through `to_dict`. Every one is a builder call the consumer makes and `to_dict` preserves; the page owns only the theme-thread and the emit rail, never a new spec case per grammar family (Growth: an altair channel/mark/transform/projection/selection/resolve is a builder call, never a new owner). `Chart.transformed_data` (verified, composition [06]) is the narwhals-backed in-process data pre-execution — the counterpart to the vegafusion server pre-pass — and is deliberately NOT composed here: data reduction is the `visualization/chart/transform#TRANSFORM` plane's, so the authoring axis asserts no data-execution capability, routing the local pre-pass there rather than minting a mention-without-composition.
- [TYPED_THEME] [RESOLVED]: the `ChartTheme` owner carries the palette AND the typed annotation-styling blocks through a typed `alt.theme.ConfigKwds` correspondence (verified `alt.theme.ConfigKwds`/`RangeConfigKwds`/`AxisConfigKwds`/`LegendConfigKwds`/`ViewConfigKwds`/`TitleConfigKwds`/`MarkConfigKwds`/`ThemeConfig` present, `.api/altair.md` type [12]) rather than the prior hand-merged raw `config.range` dict plus untyped-preserved sibling blocks the catalogue `[04]` rejects — the palette owns `config.range` via the typed `RangeConfigKwds` sub-dict (`category`/`ordinal` discrete, `ramp`/`heatmap` sequential, `diverging` bipolar) so the single-color-source invariant holds across nominal/geo/heatmap/diverging/polar/statistical/composite charts without a per-view `Color("category:N")` encode, while the typed `axis`/`legend`/`view`/`title`/`mark` `*Kwds` blocks author the drawing-sheet/ISO-3098 annotation styling the prior fence left as an untyped preserved dict. `_themed` DEEP-merges each block through `_merged` (`{**prior_block, **theme_block}`) so a consumer's own range slot (`symbol` shape) AND every sibling block survive rather than being wiped, and it is the ONE merge site both the builder arm (post-`to_dict`) and the raw-`dict` arm reach; a `None` theme block emits nothing, so the palette-only path threads only `config.range`. The `@alt.theme.register` global-registry path is the alternative for a process-wide named theme; the inline typed `config` merge is the per-theme form that carries the color source and the typed annotation styling without a stateful global registration.
- [VALIDATE_RAIL] [RESOLVED]: `_theme_vega` folds `builder.to_dict(validate=True)` through `catch(exception=SchemaValidationError)` (verified `altair.utils.schemapi.SchemaValidationError` raises under `validate=True`, and `catch` returns a `Result`) and `map_error`s onto `<invalid-spec>`, so a malformed spec yields a typed `Error` case rather than raising into the composer — the catalogue `[04]` evidence row's `SchemaValidationError`-onto-`expression.Result` fold the prior bare `builder.to_dict()` omitted. `ChartSpec.of` therefore returns `Result[ChartSpec, SpecFault]`; the Vega arm's `_theme_vega` result maps onto `ChartSpec.Vega`, the gated arms are `Ok`, and the `config.range.ordinal`/`ramp` keys are the one Vega-Lite-config item the altair catalogue does not enumerate (they are Vega-Lite config, settled against the Vega-Lite config schema, carried inside the typed `ConfigKwds`).
