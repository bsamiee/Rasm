# [PY_ARTIFACTS_CHART_TRANSFORM]

The vegafusion data pre-pass owner of the 2D chart axis. `VegaTransform` is the closed pre-pass policy whose `apply` fold returns the reduced Vega-Lite spec plus the Arrow-IPC `inline_datasets` frame map over the gated `vegafusion` runtime, never re-inlining megabytes into the JSON spec. It is the server-side Vega transform pre-pass executing a spec's DataFusion-backed data transforms before the `visualization/chart/export#EXPORT` vl-convert render — inline-JSON for small specs, Arrow-IPC `inline_datasets` for large polars frames so megabytes never inline as JSON, `ChartState.get_transformed_spec` yielding one fully-transformed self-contained spec for the interactive HTML row so no live server is needed — gated `python_version<'3.15'` and crossing the runtime subprocess seam. The pre-pass decision is computed once in `VegaTransform.of` and applied once in `apply`, never re-derived inside the worker. The Arrow-IPC `Extract` arm is the `data/tabular/columnar#COLUMNAR` chart-to-data seam: it serializes each extracted frame to Arrow-IPC `bytes` the export `_vl_render` arm threads through the converter `inline_datasets` keyword, the same Arrow path the `ColumnarEgress.ArrowIpc` egress owner reaches, so a large frame crosses as Arrow IPC rather than inlined or hex-stuffed JSON.

## [01]-[INDEX]

- [01]-[TRANSFORM]: the closed `VegaTransform` pre-pass policy over the gated `vegafusion` runtime — `Passthrough`/`Inline`/`Extract`/`State` discriminating the data pre-pass by transform presence and frame size, the `Extract` arm folding `pre_transform_extract(extracted_format="arrow-ipc")` with `get_column_usage` column pruning then `to_arrow_table`+`to_arrow_ipc_bytes` Arrow-IPC serialization, the `State` arm folding `new_chart_state` then `ChartState.get_transformed_spec` for the host-free interactive HTML row, composing the `data/tabular/columnar#COLUMNAR` Arrow-IPC frame source.

## [02]-[TRANSFORM]

- Owner: `VegaTransform` the closed pre-pass policy whose `tag` carries the `passthrough`/`inline`/`extract`/`state` literal and whose `apply` fold returns the reduced spec plus the Arrow-IPC `inline_datasets` frame map over the gated `vegafusion` runtime — never re-inlining megabytes into the JSON spec, never re-deriving the pre-pass decision inside the worker. `VegaTransform.of` computes the decision once from the spec's transform presence and column usage; `apply` dispatches the one matched arm. The pre-pass runs on the gated subprocess lane (`anyio.to_process.run_sync`) carried by the export owner, returns the reduced spec plus a `name -> Arrow-IPC bytes` frame map, and the `visualization/chart/export#EXPORT` `_vl_render` arm consumes that map on the cp315 core.
- Cases: `VegaTransform` cases — `Passthrough()` (the spec carries no transform and no large frame, the pre-pass is skipped and the spec passes through unmodified with an empty frame map) · `Inline()` (the spec carries a data transform with inlined small data, folding `runtime.pre_transform_spec` to one transform-bearing spec with the transforms executed and data inlined) · `Extract(frame_keys)` (a large-frame spec, folding `runtime.pre_transform_extract(extracted_format="arrow-ipc")` with `get_column_usage` column pruning then `transformer.to_arrow_table`+`to_arrow_ipc_bytes` serializing each extracted frame to Arrow-IPC `bytes` so a large polars frame crosses as Arrow IPC, the `data/tabular/columnar#COLUMNAR` seam) · `State()` (the interactive HTML row, folding `runtime.new_chart_state` then `ChartState.get_transformed_spec` to one fully-transformed self-contained spec the host-free renderer needs with no live server) — matched by one total `match`/`case` over `tag`.
- Entry: `VegaTransform.of(spec, interactive)` is the one decision composer — `get_column_usage` resolves the per-dataset column footprint, a frame exceeding `_ARROW_THRESHOLD` or carrying unbounded usage selecting the `Extract` keys, transform presence selecting `Inline` versus `Passthrough`, and the `interactive` flag with any transform or large frame escalating to `State` so the HTML row recovers a self-contained spec; `VegaTransform.apply(spec)` runs the matched arm on the gated subprocess lane and returns `tuple[dict[str, object], dict[str, bytes]]` — the reduced spec plus the `name -> Arrow-IPC bytes` frame map. The export owner threads `_pre_transform(spec_json, interactive)` (one `VegaTransform.of(...).apply(...)` fold over the JSON-round-tripped spec) onto `anyio.to_process.run_sync` so the gated `vegafusion` import never resolves on the cp315 core.
- Auto: `VegaTransform.of` discriminates the pre-pass once — `get_column_usage(spec)` yields the per-dataset column map, `keys` collecting the dataset names whose usage is unbounded (`None`) or exceeds `_ARROW_THRESHOLD` columns, `has_transform` scanning the top-level `transform` plus every `layer` transform, so an interactive transform-or-large spec returns `State`, a large-frame spec returns `Extract(keys)`, a transform-only spec returns `Inline`, and a plain spec returns `Passthrough`; `apply` folds the matched arm — `Inline` calls `runtime.pre_transform_spec` and returns the transformed spec with an empty frame map, `Extract` calls `runtime.pre_transform_extract(spec, extracted_format="arrow-ipc")` then serializes each `(name, frame)` pair the requested `frame_keys` names through `transformer.to_arrow_ipc_bytes(transformer.to_arrow_table(frame))` into the `name -> bytes` map, `State` calls `runtime.new_chart_state(spec).get_transformed_spec()` and returns the self-contained spec with an empty frame map, `Passthrough` returns the spec unchanged with an empty map.
- Packages: `vegafusion` (`runtime.pre_transform_spec`/`pre_transform_extract`/`new_chart_state`, `ChartState.get_transformed_spec`, `transformer.to_arrow_table`/`to_arrow_ipc_bytes`, `get_column_usage`) gated `python_version<'3.15'` and imported only inside `of`/`apply` so the abi3 wheel never resolves on the cp315 core where the pre-pass dispatches across the runtime subprocess lane; `data/tabular/columnar#COLUMNAR` (the `ColumnarEgress.ArrowIpc` Arrow-IPC frame source the `Extract` arm's `inline_datasets` feed consumes).
- Growth: a new transform pre-pass mode is one `VegaTransform` case plus one `apply` arm; a new extraction format is one `pre_transform_extract` `extracted_format` literal on the `Extract` arm; a new decision rule (a larger threshold, a per-layer footprint) is one `VegaTransform.of` branch over the same column-usage projection; zero new surface.
- Boundary: no render and no byte-emit — the pre-pass returns the reduced spec plus the Arrow-IPC frame map and `visualization/chart/export#EXPORT` `_vl_render` renders it; no spec authoring (that is `visualization/chart/spec#CHART`'s); the prior hex-into-`datasets` round-trip is the deleted double-encode, the `Extract` arm serializing to Arrow-IPC `bytes` once and the export arm threading that map through the converter `inline_datasets` keyword rather than re-inlining or hex-stuffing into the JSON spec; vegafusion carries the `python_version<'3.15'` band marker and imports inside `of`/`apply` only, so by band policy the transform dispatches onto the runtime subprocess lane (`anyio.to_process.run_sync`) carried by the export owner and never resolves on the cp315 core.

```python signature
from typing import Literal, assert_never

from expression import case, tag, tagged_union


_ARROW_THRESHOLD: int = 32


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
```

`VegaTransform.of` decides the pre-pass once from the spec's transform presence and per-dataset column usage; `apply` runs the one matched arm. The `Extract` arm is the only one producing a non-empty frame map: it prunes columns through `get_column_usage`, extracts the data through `pre_transform_extract(extracted_format="arrow-ipc")`, and serializes each frame the `frame_keys` names through `to_arrow_table`+`to_arrow_ipc_bytes` so a large polars frame crosses to the renderer as Arrow IPC — the same Arrow-IPC path `data/tabular/columnar#COLUMNAR` `ColumnarEgress.ArrowIpc` emits. The `State` arm recovers the host-free interactivity for the HTML row by folding `ChartState.get_transformed_spec` to a self-contained spec the renderer needs with no live server.

## [03]-[RESEARCH]

- [VEGAFUSION_ARROW]: the `vegafusion` `runtime.pre_transform_spec`, `runtime.pre_transform_extract`, `runtime.new_chart_state`, `ChartState.get_transformed_spec`, `transformer.to_arrow_table`, `transformer.to_arrow_ipc_bytes`, and `get_column_usage` spellings verify against the folder `.api/vegafusion.md` catalogue (`2.0.3` reflected, band `python_version<'3.15'`, abi3 on cp315). The `pre_transform_extract(spec, ..., extract_threshold=20, extracted_format='arro3', ...) -> (spec, datasets, warnings)` split-and-extract surface, the `transformer.to_arrow_table(data) -> pa.Table` and `transformer.to_arrow_ipc_bytes(data, stream=False) -> bytes` Arrow conversion pair, the `new_chart_state(spec, ...) -> ChartState` interactive root, and the `ChartState.get_transformed_spec() -> dict` fully-transformed self-contained spec are catalogue-confirmed (`[03]-[ENTRYPOINTS]` rows [03]/[04], `[ChartState and transformer]` rows [01]/[06]/[07], `get_column_usage` row [08]), so the `Extract` arm's `to_arrow_table`+`to_arrow_ipc_bytes` Arrow-IPC serialization, the `State` arm's `ChartState.get_transformed_spec` self-contained spec, and the `VegaTransform.of` column-usage pruning are all settled fence code. The catalogue documents `extracted_format` with the `'arro3'` default; the `'arrow-ipc'` explicit value the `Extract` arm passes and the `datasets` element shape it iterates (a `(name, frame)` pairing `to_arrow_table` then `to_arrow_ipc_bytes` serializes) are the one [VEGAFUSION_EXTRACT_SHAPE] catalogue-deepen item until a `pre_transform_extract` return-element reflection pass enumerates the `extracted_format` literal set and the `datasets` element tuple. The `vl-convert` render arm consumes the reduced spec plus the Arrow-IPC frame map on the cp315 core; the `vegafusion` pre-pass runs on the gated subprocess lane.
- [VL_INLINE_DATASETS] [RESEARCH]: the export `_vl_render` arm threads this owner's pre-pass Arrow-IPC frame map through the converter `inline_datasets=` keyword, but the folder `.api/vl-convert-python.md` catalogue documents the `vegalite_to_*` call shape as `spec plus render/raster/bundle policy` without enumerating an `inline_datasets` parameter. The Arrow-IPC inline-dataset feed (so a `pre_transform_extract` extracted frame crosses to the renderer as Arrow IPC `bytes` rather than inlined or hex-stuffed JSON) stays a marked RESEARCH item until a `vegalite_to_*` signature reflection pass enumerates the inline-dataset keyword and its `name -> bytes` map shape; the `VegaTransform.Extract` Arrow-IPC serialization and the `name -> bytes` map shape this owner emits are settled, only the consumer-side `inline_datasets` keyword name is the RESEARCH leg, tracked on `visualization/chart/export#EXPORT`. Close-condition: `.api/vl-convert-python.md` carries the `vegalite_to_*` `inline_datasets` parameter with its Arrow-IPC `bytes` map contract.
