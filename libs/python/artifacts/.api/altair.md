# [PY_ARTIFACTS_API_ALTAIR]

`altair` supplies the declarative Vega-Lite chart-specification surface for the artifacts visuals rail: a `Chart` builder with mark/encode/transform method families, composition operators, and a multi-format save path that drive grammar-of-graphics chart construction from tabular data into Vega-Lite JSON. The package owner composes `Chart`, the mark/encode/transform families, and `to_dict`/`save` into the visuals owner; it never re-implements the Vega-Lite grammar altair already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `altair`
- package: `altair`
- module: `altair`
- asset: runtime library
- rail: visuals

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: chart and composition roots
- rail: visuals

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [RAIL]                                          |
| :-----: | :------------------ | :-------------- | :---------------------------------------------- |
|   [1]   | `Chart`             | chart builder   | single-view chart with mark/encode/transform    |
|   [2]   | `LayerChart`        | layered chart   | overlaid views                                  |
|   [3]   | `HConcatChart`      | concat chart    | horizontal side-by-side composition             |
|   [4]   | `VConcatChart`      | concat chart    | vertical stacked composition                    |
|   [5]   | `ConcatChart`       | concat chart    | wrappable grid composition                      |
|   [6]   | `FacetChart`        | faceted chart   | small-multiples by field                        |
|   [7]   | `data_transformers` | plugin registry | dataset handling plugin axis (default/json/csv) |
|   [8]   | `renderers`         | plugin registry | output renderer plugin axis                     |
|   [9]   | `theme`             | plugin registry | named chart theme axis                          |

[PUBLIC_TYPE_SCOPE]: encoding and selection value objects
- rail: visuals

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :------------------- | :------------ | :------------------------------- |
|   [1]   | `X` / `Y`            | channel       | positional encoding constructors |
|   [2]   | `Color` / `Size`     | channel       | color and size encoding          |
|   [3]   | `Shape` / `Tooltip`  | channel       | shape and tooltip encoding       |
|   [4]   | `Scale` / `Axis`     | guide         | scale and axis configuration     |
|   [5]   | `Legend`             | guide         | legend configuration             |
|   [6]   | `selection_point`    | selection     | point interactive selection      |
|   [7]   | `selection_interval` | selection     | interval interactive selection   |
|   [8]   | `condition`          | conditional   | data-driven conditional encoding |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: chart construction, marks, and encoding
- rail: visuals

| [INDEX] | [SURFACE]           | [ENTRY_FAMILY]   | [RAIL]                       |
| :-----: | :------------------ | :--------------- | :--------------------------- |
|   [1]   | `Chart(data)`       | construction     | build a chart over a dataset |
|   [2]   | `Chart.mark_*`      | mark family      | set the geometric mark       |
|   [3]   | `Chart.encode`      | encoding         | bind data fields to channels |
|   [4]   | `Chart.transform_*` | transform family | data transform pipeline      |
|   [5]   | `Chart.interactive` | interactivity    | enable pan/zoom selection    |
|   [6]   | `Chart.properties`  | config           | set width/height/title       |

[ENTRYPOINT_SCOPE]: composition and export
- rail: visuals

| [INDEX] | [SURFACE]       | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------- | :------------- | :------------------------------ |
|   [1]   | `Chart.to_dict` | spec emit      | emit the Vega-Lite spec as dict |
|   [2]   | `Chart.to_json` | spec emit      | emit spec as JSON string        |
|   [3]   | `Chart.save`    | export         | export to file (html/png/svg)   |
|   [4]   | `layer`         | composition    | overlay multiple chart views    |
|   [5]   | `hconcat`       | composition    | horizontal chart concatenation  |
|   [6]   | `vconcat`       | composition    | vertical chart concatenation    |
|   [7]   | `concat`        | composition    | wrappable grid concatenation    |

## [4]-[IMPLEMENTATION_LAW]

[VISUALS_VEGALITE]:
- import: `import altair as alt` at boundary scope only; module-level import is banned by the manifest import policy.
- chart axis: one `Chart` builder owns the single-view grammar; `mark_*`/`encode`/`transform_*` are method-family rows on the builder, never parallel chart types per mark.
- composition axis: `layer`/`hconcat`/`vconcat`/facet compose `Chart` instances into the composite roots; composition is an operator, never a duplicated chart definition.
- export axis: `to_dict`/`to_json` emit the spec; `save` with `format` is the single export entry — the static PNG/SVG/PDF path delegates to `vl-convert-python`, never a re-minted rasterizer.
- evidence: each chart captures mark kind, encoded channel set, transform chain length, and output spec/byte size as a visuals receipt.
- boundary: altair owns spec construction; static rendering routes to `vl-convert-python`; interactive Plotly figures route to `plotly`; raster matplotlib output to `matplotlib`; live UI stays outside this package.

[RAIL_LAW]:
- Package: `altair`
- Owns: declarative Vega-Lite chart construction, composition, interactivity, and spec/multi-format export
- Accept: grammar-of-graphics chart specs feeding the visuals owner and the `vl-convert` render path
- Reject: wrapper-renames of `mark_*`/`encode`; a hand-built Vega-Lite dict where the builder is admitted; a re-minted rasterizer where `vl-convert` renders; identity minting the runtime owns
