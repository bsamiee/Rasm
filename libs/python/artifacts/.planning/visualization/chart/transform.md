# [PY_ARTIFACTS_CHART_TRANSFORM]

The vegafusion data pre-pass owner of the 2D chart axis. `VegaTransform` is the closed pre-pass policy whose `apply` fold returns ONE self-contained Vega-Lite spec over the gated `vegafusion` runtime — the spec's DataFusion-backed data transforms server-evaluated and the reduced result inlined, never the raw frame. It runs before the `visualization/chart/export#EXPORT` vl-convert render: `pre_transform_spec` evaluates the transforms and inlines the pre-computed (reduced) data into one self-contained spec, and `ChartState.get_transformed_spec` yields the fully-transformed self-contained spec the interactive HTML row needs with no live server. vl-convert exposes no external-dataset feed, so the reduced data crosses INSIDE the spec — the server-side aggregation IS the size reduction, not a separate Arrow-IPC channel the renderer cannot accept. The pre-pass decision is computed once in `VegaTransform.of` and applied once in `apply`, never re-derived inside the worker, and the whole pre-pass crosses the runtime subprocess seam.

## [01]-[INDEX]

- [01]-[TRANSFORM]: the closed `VegaTransform` pre-pass policy over the gated `vegafusion` runtime — `Passthrough`/`Inline`/`State` discriminating the data pre-pass by transform presence and interactivity, the `Inline` arm folding `runtime.pre_transform_spec` to one self-contained transform-bearing spec, the `State` arm folding `new_chart_state` then `ChartState.get_transformed_spec` for the host-free interactive HTML row, every arm returning one self-contained spec the `visualization/chart/export#EXPORT` `_vl_render` renders directly.

## [02]-[TRANSFORM]

- Cases: `VegaTransform` cases — `Passthrough()` (the spec carries no data transform, the pre-pass is skipped and the spec passes through unmodified) · `Inline()` (the spec carries a data transform, folding `runtime.pre_transform_spec` to one self-contained spec with the transforms executed and the reduced data inlined) · `State()` (the interactive HTML row, folding `runtime.new_chart_state` then `ChartState.get_transformed_spec` to one fully-transformed self-contained spec the host-free renderer needs with no live server) — matched by one total `match`/`case` over `tag`. There is no separate Arrow-IPC extract case: vl-convert accepts no external dataset, so a `pre_transform_extract` frame map would have no renderer consumer; the server-side aggregation `pre_transform_spec` runs IS the data reduction, and the reduced result inlines into the spec rather than crossing as a side channel.
- Auto: `VegaTransform.of` discriminates the pre-pass once — `has_transform` scanning the top-level `transform` plus every `layer` transform, so an interactive transform-bearing spec returns `State`, a transform-bearing spec returns `Inline`, and a plain spec returns `Passthrough`; `apply` folds the matched arm — `Inline` calls `runtime.pre_transform_spec(spec)` and returns the transformed self-contained spec (discarding the `PreTransformWarning` tail), `State` calls `runtime.new_chart_state(spec).get_transformed_spec()` and returns the self-contained interactive spec, `Passthrough` returns the spec unchanged. Each arm returns one spec dict the export render consumes directly.
- Growth: a new transform pre-pass mode is one `VegaTransform` case plus one `apply` arm and one `VegaTransform.of` branch; a new decision rule (a per-layer transform footprint, a `row_limit` policy threaded into `pre_transform_spec`) is one `VegaTransform.of` branch over the same transform-presence projection; zero new surface.

```python signature
from typing import Literal, assert_never

from expression import case, tag, tagged_union

lazy import vegafusion


@tagged_union(frozen=True)
class VegaTransform:
    tag: Literal["passthrough", "inline", "state"] = tag()
    passthrough: None = case()
    inline: None = case()
    state: None = case()

    @staticmethod
    def Passthrough() -> "VegaTransform":
        return VegaTransform(passthrough=None)

    @staticmethod
    def Inline() -> "VegaTransform":
        return VegaTransform(inline=None)

    @staticmethod
    def State() -> "VegaTransform":
        return VegaTransform(state=None)

    @staticmethod
    def of(spec: dict[str, object], interactive: bool) -> "VegaTransform":
        has_transform = "transform" in spec or any(
            isinstance(layer, dict) and "transform" in layer for layer in spec.get("layer", ())
        )
        if interactive and has_transform:
            return VegaTransform.State()
        return VegaTransform.Inline() if has_transform else VegaTransform.Passthrough()

    def apply(self, spec: dict[str, object]) -> dict[str, object]:
        match self:
            case VegaTransform(tag="passthrough"):
                return spec
            case VegaTransform(tag="inline"):
                transformed, _warnings = vegafusion.runtime.pre_transform_spec(spec)
                return transformed
            case VegaTransform(tag="state"):
                return vegafusion.runtime.new_chart_state(spec).get_transformed_spec()
            case _:
                assert_never(self)
```

`VegaTransform.of` decides the pre-pass once from the spec's transform presence and the interactive flag; `apply` runs the one matched arm and returns one self-contained spec. The `Inline` arm folds `pre_transform_spec` so the DataFusion-backed transforms (aggregations, joins, binning) execute server-side and the reduced result inlines into the spec — vl-convert then renders with no client-side transform work and no oversized embedded data, because the aggregation already collapsed the raw frame. The `State` arm recovers the host-free interactivity for the HTML row by folding `ChartState.get_transformed_spec` to a self-contained spec the renderer needs with no live server. vl-convert has no external-dataset feed, so an Arrow-IPC side channel cannot reach it; the reduction crosses inside the spec.

## [03]-[RESEARCH]

- [VL_NO_EXTERNAL_FEED] [RESOLVED]: signature reflection of `vl_convert 1.9.0.post1` (and `.api/vl-convert-python.md` rows [01]-[05]) confirms the `vegalite_to_*` family carries NO `inline_datasets` parameter, so the prior Arrow-IPC frame-map `Extract` arm had no renderer that could consume it — an `inline_datasets=` kwarg `TypeError`s. The owner therefore returns ONE self-contained spec: `pre_transform_spec` server-evaluates the transforms and inlines the reduced result (the aggregation IS the size reduction), `ChartState.get_transformed_spec` serves the interactive HTML row, and `Passthrough` skips a transform-free spec. The `pre_transform_extract`/`get_column_usage` and the `vegafusion.transformer` Arrow-IPC serializers (`arrow_table_to_ipc_bytes(pa.Table)`, the `(name, scope, pa.Table)` 3-tuple `pre_transform_extract` returns under `extracted_format='arro3'`) remain the `data/tabular/columnar#COLUMNAR` egress owner's surface for a genuine columnar data export, never a chart-render side channel.
- [PRE_TRANSFORM_SPEC] [RESOLVED]: `runtime.pre_transform_spec(spec) -> (dict, list[PreTransformWarning])` and `runtime.new_chart_state(spec).get_transformed_spec() -> dict` verify against `.api/vegafusion.md` runtime rows [01]/[04] and `ChartState` row [01]; `apply` binds the spec half and discards the warnings tail, returning the one self-contained dict. `vegafusion.runtime.pre_transform_spec` resolves on the singleton `runtime` instance the package binds at `__init__`, distinct from the shadowed `vegafusion.runtime` submodule attribute the `.api` flags.
