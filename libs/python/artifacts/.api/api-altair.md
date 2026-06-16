# [PY_ARTIFACTS_API_ALTAIR]

`altair` supplies the declarative Vega-Lite chart-specification surface for the artifacts visuals rail: a `Chart` builder with mark/encode/transform method families, composition operators, and a multi-format save path that drive grammar-of-graphics chart construction from tabular data into Vega-Lite JSON. The package owner composes `Chart`, the mark/encode/transform families, and `to_dict`/`save` into the visuals owner; it never re-implements the Vega-Lite grammar altair already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `altair`
- package: `altair`
- import: `altair` (lint alias `alt`)
- owner: `artifacts`
- rail: visuals
- installed: `6.2.1` reflected via `python -c "import altair"` on cp315
- entry points: none (library only)
- capability: declarative Vega-Lite chart construction (marks, encodings, transforms), layered/concat/facet composition, interactivity, multi-format export via the renderer/save path

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: chart and composition roots
- rail: visuals

| [INDEX] | [SYMBOL]                                        | [PACKAGE_ROLE]       | [CAPABILITY]                                               |
| :-----: | :---------------------------------------------- | :------------------- | :--------------------------------------------------------- |
|   [1]   | `Chart`                                         | chart builder        | single-view chart with mark/encode/transform/interactivity |
|   [2]   | `LayerChart`                                    | layered chart        | overlaid views                                             |
|   [3]   | `HConcatChart` / `VConcatChart` / `ConcatChart` | concat chart         | side-by-side/stacked composition                           |
|   [4]   | `FacetChart`                                    | faceted chart        | small-multiples by field                                   |
|   [5]   | `data_transformers`                             | data plugin registry | dataset handling plugin axis (default/json/csv)            |
|   [6]   | `renderers`                                     | renderer registry    | output renderer plugin axis                                |
|   [7]   | `theme`                                         | theme registry       | named chart theme axis                                     |

[PUBLIC_TYPE_SCOPE]: encoding and selection value objects
- rail: visuals

| [INDEX] | [SYMBOL]                                           | [PACKAGE_ROLE] | [CAPABILITY]                     |
| :-----: | :------------------------------------------------- | :------------- | :------------------------------- |
|   [1]   | `X` / `Y` / `Color` / `Size` / `Shape` / `Tooltip` | channel        | encoding channel constructors    |
|   [2]   | `Scale` / `Axis` / `Legend`                        | guide          | scale/axis/legend configuration  |
|   [3]   | `selection_point` / `selection_interval`           | selection      | interactive selection params     |
|   [4]   | `condition`                                        | conditional    | data-driven conditional encoding |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: chart construction, marks, and encoding
- rail: visuals

Mark rows cover the chart mark family; transform rows cover aggregate, filter, calculate, bin, fold, pivot, window, regression, and density transforms.

| [INDEX] | [SURFACE]           | [CALL_SHAPE]              | [CAPABILITY]                 |
| :-----: | :------------------ | :------------------------ | :--------------------------- |
|   [1]   | `Chart`             | data/encoding/mark policy | build a chart over a dataset |
|   [2]   | `Chart.mark_*`      | mark-family selector      | set the geometric mark       |
|   [3]   | `Chart.encode`      | channel encoding policy   | bind data fields to channels |
|   [4]   | `Chart.transform_*` | transform-family selector | data transform pipeline      |
|   [5]   | `Chart.interactive` | selection binding policy  | enable pan/zoom              |
|   [6]   | `Chart.properties`  | property kwargs           | set width/height/title       |

[ENTRYPOINT_SCOPE]: composition and export
- rail: visuals

Export rows share validation, format, data-transformer, scale, Vega/Vega-Lite/embed versions, engine, inline, and embed-option policy. Composition helpers are `layer`, `hconcat`, `vconcat`, and `concat`.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                | [CAPABILITY]            |
| :-----: | :-------------------- | :-------------------------- | :---------------------- |
|   [1]   | `Chart.to_dict`       | validation plus spec format | emit the Vega-Lite spec |
|   [2]   | `Chart.to_json`       | validation plus JSON policy | emit spec JSON          |
|   [3]   | `Chart.save`          | target plus export policy   | export to file          |
|   [4]   | composition functions | chart sequence composition  | compose multiple charts |

## [4]-[IMPLEMENTATION_LAW]

[VISUALS_VEGALITE]:
- import: `import altair as alt` at boundary scope only; module-level import is banned by the manifest import policy.
- chart axis: one `Chart` builder owns the single-view grammar; `mark_*`/`encode`/`transform_*` are method-family rows on the builder, never parallel chart types per mark.
- composition axis: `layer`/`hconcat`/`vconcat`/facet compose `Chart` instances into the composite roots; composition is an operator, never a duplicated chart definition.
- export axis: `to_dict`/`to_json` emit the spec; `save` with `format` is the single export entry — the static PNG/SVG/PDF path delegates to `vl-convert-python`, never a re-minted rasterizer.
- evidence: each chart captures mark kind, encoded channel set, transform chain length, and output spec/byte size as a visuals receipt.
- boundary: altair owns spec construction; static rendering routes to `vl-convert-python`; interactive Plotly figures route to `plotly`; raster matplotlib output to `matplotlib`; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `altair`
- Owns: declarative Vega-Lite chart construction, composition, interactivity, and spec/multi-format export
- Accept: grammar-of-graphics chart specs feeding the visuals owner and the `vl-convert` render path
- Reject: wrapper-renames of `mark_*`/`encode`; a hand-built Vega-Lite dict where the builder is admitted; a re-minted rasterizer where `vl-convert` renders; identity minting the runtime owns
