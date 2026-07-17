# [PY_ARTIFACTS_CHART_SPEC]

`ChartSpec` is the chart-authoring half of the 2D chart axis: one tagged union collapsing the host-free 2D engines — declarative-Vega (altair Vega-Lite), declarative-grammar (lets-plot), and publication (matplotlib) — onto a single palette-threaded surface, discriminated by one total `match` over `ChartEngineTag` with no parallel engine enum. Host-free posture is the decisive axis: vl-convert renders Vega/Vega-Lite Rust-native under an embedded V8 with zero browser, and lets-plot self-renders to bytes in-process, so two content-keyable producers exist with no Chrome dependency. `ChartSpec.of` themes and emits whatever grammar the consumer's admitted altair `Chart` builder carries — mark, encode, transform, projection, selection, composition, resolve — without clobbering it, then hands the themed spec dict to the export half. Every case payload is typed at admission: `VegaInput` validates a raw Vega-family mapping as recursive `JsonValue` data with a recognized `$schema` and disappears at promotion, while builder roots prove themselves through Altair's classes; `PlotSpec`/`SupPlotsSpec` and `Figure` prove the other cases by `isinstance`, so a lookalike stays on the `<unknown-engine>` rail and `visualization/chart/export#EXPORT` receives no erased payload.

Color arrives from `graphic/color/derive#DERIVE` as the `Derivation.coords` array carried on the one `ChartTheme` owner, threading through one typed `config` binding into every engine: palette owns `config.range` as the single color source, while the typed `axis`/`legend`/`view`/`title`/`mark` blocks plus the `background`/`font` scalars own the drawing-sheet/ISO-3098 annotation styling a journal figure needs, each deep-merged over the consumer's own block. `hex_ramp` is `graphic/color/derive#DERIVE`'s one shared RGB-to-hex projection, imported here and by `visualization/chart/export#EXPORT` and `visualization/diagram/draw#DRAW`. Themed result hands to `visualization/chart/export#EXPORT`; the vegafusion data pre-pass lives at `visualization/chart/export#PREPASS`, never on this authoring axis, and altair's builder-side `Chart.transformed_data` is the same vegafusion evaluation reached through a second path — rejected so one pre-pass owner exists.

## [01]-[INDEX]

- [01]-[CHART]: the `ChartSpec` union over host-free charting engines, each case threading the derived palette through one `ChartSpec.of` composer keyed on the admitted altair builder roots and the typed engine classes.

## [02]-[CHART]

- Owner: `ChartSpec` collapses the `vega`/`lets_plot`/`matplotlib` cases under one total `match` over `ChartEngineTag`; `ChartSpec.of(engine, theme)` is the one composer keyed on input shape, returning `Result[ChartSpec, SpecFault]` so `to_dict` schema validation is a typed rail, not a raise. Construction is `of` alone — no per-case forwarding staticmethod, no `ChartSpec.compose` hop, no parallel `ChartEngine.compose` fold.
- Cases: `vega` folds an altair builder or a raw Vega-family dict to the palette-themed spec dict the export axis consumes directly — a raw dict's `$schema` may name either dialect and either supported major generation (`_SCHEMAS` closes Vega-Lite v5/v6 and Vega v5/v6, the exact band the bundled converter renders), and the export half derives the converter family from that same `$schema`, so no dialect flag rides beside the spec that already carries it; `lets_plot` carries a single `PlotSpec` or a `gggrid`/`ggbunch`/`ggdeck` `SupPlotsSpec` composite root (both own the same `to_*` serializers) plus its `Palette`; `matplotlib` carries the typed `Figure` plus its `Palette` — both rendering on the worker lane beside their own annotation egress. No plotly/kaleido case — host-free posture forbids a Chrome-gated engine, and vl-convert renders no plotly.js.
- Entry: builder roots fold through `to_dict(validate=True)` and map `SchemaValidationError` to `<invalid-spec>`; a raw `dict` crosses `VegaInput.model_validate` and maps `ValidationError` to the same closed fault before `ChartTheme.apply` promotes its root; `PlotSpec()`/`SupPlotsSpec()`/`Figure()` class patterns prove the grammar/publication cases by type, carrying `theme.palette` — a composite root's subplots arrive palette-threaded from authoring, so export passes the composite through untouched while a lone `PlotSpec` gains the manual scales at render; a non-chart object rails `<unknown-engine>` at admission. A palette-only caller constructs `ChartTheme(palette=coords)` with sibling blocks `None`, while a journal/ISO-3098 caller passes the typed `axis`/`legend`/`title` styling on the same owner.
- Growth: a new host-free 2D engine is one `ChartSpec` case plus one `of` class-pattern arm plus one `visualization/chart/export#EXPORT` `_export_host_free` arm; a new altair channel, mark, transform, projection, selection, or `resolve_*` block is a builder call the Vega arm preserves through `to_dict`, never a new case; a new annotation-styling axis is one `ChartTheme` field plus one row in `ChartTheme.apply`'s block fold, and a new whole-canvas scalar is one `ChartTheme` field beside `background`/`font`.
- Boundary: no live dashboard, UI event state, or browser runtime; no Chrome-gated engine, so plotly `graph_objects` and the kaleido headless-Chromium path are rejected; 3D scientific scenes ride `scene/render#SCENE`, not this axis; palettes arrive as `Derivation.coords`, never picked per engine and never overwriting a consumer's own `Color` encoding, since `config.range` sets scale ranges globally rather than per view; a forced `Chart.interactive()` or a forced `Color("category:N")` encode are rejected lower-capability forms, the first suppressing the consumer's named selections the HTML row renders, the second clobbering its color field. `alt.theme.register` is rejected on this axis: theming is per-spec data threaded through `ChartTheme.apply`, and the process-global theme registry would leak one caller's styling into every concurrent producer — only the registry's typed `*Kwds` vocabulary crosses in. The lets-plot/matplotlib arms carry the palette alone by design: each engine themes annotation styling through its own export egress, so the `*Kwds` blocks are Vega-only and that asymmetry is the modeled contract, not a gap. Emitted bytes hand to `composition/compose#COMPOSE`, re-rendered nowhere.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Final, Literal

from expression import Error, Ok, Result, case, tag, tagged_union
from expression.extra.result import catch
from msgspec import Struct
from pydantic import ConfigDict, JsonValue, RootModel, ValidationError, model_validator

from rasm.artifacts.graphic.color.derive import Palette, hex_ramp

lazy import altair as alt
lazy from altair.utils.schemapi import SchemaValidationError
lazy from lets_plot.plot.core import PlotSpec
lazy from lets_plot.plot.subplots import SupPlotsSpec
lazy from matplotlib.figure import Figure

# --- [TYPES] ----------------------------------------------------------------------------
type ChartEngineTag = Literal["vega", "lets_plot", "matplotlib"]
type SpecFault = Literal["<invalid-spec>", "<unknown-engine>"]

# --- [CONSTANTS] ------------------------------------------------------------------------
# vl-convert bundles Vega 6.x plus the 5.8-6.4 Vega-Lite band, so both major schema generations of both dialects render.
# canonical URLs close the allowlist whole — a suffix test would admit any authority or lookalike path ahead of the tail.
_SCHEMAS: Final[frozenset[str]] = frozenset(
    f"https://vega.github.io/schema{path}" for path in ("/vega-lite/v6.json", "/vega-lite/v5.json", "/vega/v6.json", "/vega/v5.json")
)


# --- [MODELS] ---------------------------------------------------------------------------
class VegaInput(RootModel[dict[str, JsonValue]]):
    model_config = ConfigDict(strict=True, frozen=True)

    @model_validator(mode="after")
    def _known_schema(self) -> "VegaInput":
        schema = self.root.get("$schema")
        # Provider validation cannot cover full Vega, so admission closes the supported schema families here.
        if not isinstance(schema, str) or schema not in _SCHEMAS:
            raise ValueError("Vega input requires a supported $schema")
        return self


def _merged(base: dict[str, JsonValue], override: dict[str, object]) -> dict[str, JsonValue]:
    # the depth-recursive config weave every theme block rides: a theme value wins its key, a nested consumer mapping
    # survives beneath it, so an `axis.title`- or `legend`-nested consumer setting outlives the themed override.
    return {
        **base,
        **{
            key: _merged(prior, value) if isinstance(prior := base.get(key), dict) and isinstance(value, dict) else value
            for key, value in override.items()
        },
    }


class ChartTheme(Struct, frozen=True):
    palette: Palette
    axis: alt.theme.AxisConfigKwds | None = None
    legend: alt.theme.LegendConfigKwds | None = None
    view: alt.theme.ViewConfigKwds | None = None
    title: alt.theme.TitleConfigKwds | None = None
    mark: alt.theme.MarkConfigKwds | None = None
    background: str | None = None
    font: str | None = None

    def apply(self, spec: dict[str, JsonValue]) -> dict[str, JsonValue]:
        config = prior if isinstance(prior := spec.get("config"), dict) else {}
        ramp = hex_ramp(self.palette)
        blocks = (
            ("axis", self.axis),
            ("legend", self.legend),
            ("view", self.view),
            ("title", self.title),
            ("mark", self.mark),
        )
        styled = {
            key: _merged(value if isinstance(value := config.get(key), dict) else {}, block)
            for key, block in blocks
            if block is not None
        }
        ranges: alt.theme.RangeConfigKwds = {
            **(value if isinstance(value := config.get("range"), dict) else {}),
            **dict.fromkeys(("category", "ordinal", "ramp", "heatmap", "diverging"), ramp),
        }
        themed: alt.theme.ConfigKwds = {
            **config,
            "range": ranges,
            **styled,
            **({"background": self.background} if self.background is not None else {}),
            **({"font": self.font} if self.font is not None else {}),
        }
        return {**spec, "config": themed}


@tagged_union(frozen=True)
class ChartSpec:
    tag: ChartEngineTag = tag()
    vega: dict[str, JsonValue] = case()
    lets_plot: tuple[PlotSpec | SupPlotsSpec, Palette] = case()
    matplotlib: tuple[Figure, Palette] = case()

    @staticmethod
    def of(engine: object, theme: "ChartTheme") -> "Result[ChartSpec, SpecFault]":
        match engine:
            case alt.Chart() | alt.LayerChart() | alt.HConcatChart() | alt.VConcatChart() | alt.ConcatChart() | alt.FacetChart() | alt.RepeatChart():
                return catch(exception=SchemaValidationError)(engine.to_dict)(validate=True).map_error(lambda _fault: "<invalid-spec>").map(
                    lambda resolved: ChartSpec(vega=theme.apply(resolved))
                )
            case dict() as raw:
                return catch(exception=ValidationError)(VegaInput.model_validate)(raw).map_error(lambda _fault: "<invalid-spec>").map(
                    lambda admitted: ChartSpec(vega=theme.apply(admitted.root))
                )
            case PlotSpec() | SupPlotsSpec() as plot:
                return Ok(ChartSpec(lets_plot=(plot, theme.palette)))
            case Figure() as figure:
                return Ok(ChartSpec(matplotlib=(figure, theme.palette)))
            case _:
                return Error("<unknown-engine>")
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [SUPPLOTS_MODULE]-[BLOCKED]: confirm `SupPlotsSpec` imports from `lets_plot.plot.subplots` (the class is catalog-attested at `.api/lets-plot.md:32`; the module path is unreflected); source-route reflection on the cp314 wheel, re-run `api resolve lets-plot` when the cp315 gate drops.
