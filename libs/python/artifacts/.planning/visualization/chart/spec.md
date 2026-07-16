# [PY_ARTIFACTS_CHART_SPEC]

`ChartSpec` is the chart-authoring half of the 2D chart axis: one tagged union collapsing the host-free 2D engines — declarative-Vega (altair Vega-Lite), declarative-grammar (lets-plot), and publication (matplotlib) — onto a single palette-threaded surface, discriminated by one total `match` over `ChartEngineTag` with no parallel engine enum. Host-free posture is the decisive axis: vl-convert renders Vega/Vega-Lite Rust-native under an embedded V8 with zero browser, and lets-plot self-renders to bytes in-process, so two content-keyable producers exist with no Chrome dependency. `ChartSpec.of` themes and emits whatever grammar the consumer's admitted altair `Chart` builder carries — mark, encode, transform, projection, selection, composition, resolve — without clobbering it, then hands the themed spec dict to the export half.

Color arrives from `graphic/color/derive#DERIVE` as the `Derivation.coords` array carried on the one `ChartTheme` owner, threading through one typed `config` binding into every engine: palette owns `config.range` as the single color source, while the typed `axis`/`legend`/`view`/`title`/`mark` blocks own the drawing-sheet/ISO-3098 annotation styling a journal figure needs, each deep-merged over the consumer's own block. `hex_ramp` is the one shared RGB-to-hex projection, declared here and imported by `visualization/chart/export#EXPORT` and `visualization/diagram/draw#DRAW`. Themed result hands to `visualization/chart/export#EXPORT`; the vegafusion data pre-pass and the narwhals-backed `transformed_data` pre-execution both live at `visualization/chart/export#PREPASS`, never on this authoring axis.

## [01]-[INDEX]

- [01]-[CHART]: the `ChartSpec` union over host-free charting engines, each case threading the derived palette through one `ChartSpec.of` composer keyed on the admitted altair builder roots.

## [02]-[CHART]

- Owner: `ChartSpec` collapses `Vega`/`LetsPlot`/`Matplotlib` under one total `match` over `ChartEngineTag`; `ChartSpec.of(engine, theme)` is the one composer keyed on input shape, returning `Result[ChartSpec, SpecFault]` so `to_dict` schema validation is a typed rail, not a raise. No `ChartSpec.compose` forwarding hop, no parallel `ChartEngine.compose` fold.
- Cases: `Vega` folds an altair builder or a raw Vega-Lite dict to the palette-themed spec dict the export axis consumes directly; `LetsPlot`/`Matplotlib` carry the raw engine object plus its `Palette`, rendering on the worker lane beside their own annotation egress. No plotly/kaleido case — host-free posture forbids a Chrome-gated engine, and vl-convert renders no plotly.js.
- Entry: builder roots fold the Vega arm through `_theme_vega`, whose `to_dict(validate=True)` `SchemaValidationError` rails to `<invalid-spec>`; a raw `dict` theme-threads directly through `_themed`; a `lets_plot`/`matplotlib` module-probe folds its case carrying `theme.palette`; a non-chart object rails `<unknown-engine>` at admission, never misrouted to the matplotlib arm. A palette-only caller constructs `ChartTheme(palette=coords)` with sibling blocks `None`, byte-identical to a color-only path, while a journal/ISO-3098 caller passes the typed `axis`/`legend`/`title` styling on the same owner.
- Growth: a new host-free 2D engine is one `ChartSpec` case plus one `ChartSpec.of` arm plus one `visualization/chart/export#EXPORT` `_export_host_free` arm; a new altair channel, mark, transform, projection, selection, or `resolve_*` block is a builder call the Vega arm preserves through `to_dict`, never a new case; a new annotation-styling axis is one `ChartTheme` `*Kwds` field threaded through `_themed`'s `_merged` fold.
- Boundary: no live dashboard, UI event state, or browser runtime; no Chrome-gated engine, so plotly `graph_objects` and the kaleido headless-Chromium path are rejected; 3D scientific scenes ride `scene#SCENE`, not this axis; palettes arrive as `Derivation.coords`, never picked per engine and never overwriting a consumer's own `Color` encoding, since `config.range` sets scale ranges globally rather than per view; a forced `Chart.interactive()` or a forced `Color("category:N")` encode are rejected lower-capability forms, the first suppressing the consumer's named selections the HTML row renders, the second clobbering its color field. Emitted bytes hand to `composition/compose#COMPOSE`, re-rendered nowhere.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Literal

import altair as alt
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.extra.result import catch
from msgspec import Struct

from altair.utils.schemapi import SchemaValidationError
from rasm.artifacts.graphic.color.derive import Palette, hex_ramp

# --- [TYPES] ----------------------------------------------------------------------------
type ChartEngineTag = Literal["vega", "lets_plot", "matplotlib"]
type SpecFault = Literal["<invalid-spec>", "<unknown-engine>"]
type VegaBuilder = alt.Chart | alt.LayerChart | alt.HConcatChart | alt.VConcatChart | alt.ConcatChart | alt.FacetChart | alt.RepeatChart


# --- [MODELS] ---------------------------------------------------------------------------
class ChartTheme(Struct, frozen=True):
    # the ONE journal-and-ISO-3098 theme owner: palette (single color source) PLUS the typed annotation-styling blocks.
    # `palette` threads every engine (config.range on Vega, scale_*_manual on lets-plot, prop-cycle on matplotlib); the
    # `alt.theme.*Kwds` blocks thread ONLY the Vega arm — lets-plot/matplotlib theme through their own export egress.
    palette: Palette
    axis: "alt.theme.AxisConfigKwds | None" = None  # ISO 3098 / journal axis styling: a typed block, never an untyped dict
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
            case dict() as raw:  # a consumer-materialized Vega-Lite dict; theme-thread directly, no `to_dict`
                return Ok(ChartSpec.Vega(_themed(raw, theme)))
            case _ if type(engine).__module__.startswith("lets_plot"):
                return Ok(ChartSpec.LetsPlot(engine, theme.palette))  # lets-plot themes its own styling; only the color source crosses
            case _ if type(engine).__module__.startswith("matplotlib"):
                return Ok(ChartSpec.Matplotlib(engine, theme.palette))
            case _:
                return Error("<unknown-engine>")  # reject at admission, never misroute to matplotlib


# --- [OPERATIONS] -----------------------------------------------------------------------
def _merged(prior_config: dict[str, object], key: str, block: object) -> dict[str, object]:
    # deep-merge ONE typed `*Kwds` sibling block over the consumer's own prior block so journal/ISO-3098 styling
    # threads WITHOUT wiping what the consumer already set; a None block contributes nothing.
    if block is None:
        return {}
    prior_block = pb if isinstance(pb := prior_config.get(key), dict) else {}
    return {key: {**prior_block, **block}}


def _themed(spec: dict[str, object], theme: ChartTheme) -> dict[str, object]:
    # thread the ChartTheme through one deep-merged typed `config`: palette OWNS every color-scale range via the typed
    # `RangeConfigKwds` sub-dict (discrete/sequential/diverging), and each `axis`/`legend`/`view`/`title`/`mark` block
    # deep-merges over the consumer's own via `_merged`, never clobbering a per-view `Color` field.
    ramp = hex_ramp(theme.palette)
    prior_config = cfg if isinstance(cfg := spec.get("config"), dict) else {}
    prior_range = rng if isinstance(rng := prior_config.get("range"), dict) else {}
    range_block: alt.theme.RangeConfigKwds = {**prior_range, "category": ramp, "ordinal": ramp, "ramp": ramp, "heatmap": ramp, "diverging": ramp}
    typed: alt.theme.ConfigKwds = {
        "range": range_block,
        **_merged(prior_config, "axis", theme.axis),
        **_merged(prior_config, "legend", theme.legend),
        **_merged(prior_config, "view", theme.view),
        **_merged(prior_config, "title", theme.title),
        **_merged(prior_config, "mark", theme.mark),
    }
    return {**spec, "config": {**prior_config, **typed}}


def _theme_vega(builder: VegaBuilder, theme: ChartTheme) -> Result[dict[str, object], SpecFault]:
    return (
        catch(exception=SchemaValidationError)(builder.to_dict)(validate=True)
        .map_error(lambda _fault: "<invalid-spec>")
        .map(lambda resolved: _themed(resolved, theme))
    )

```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
