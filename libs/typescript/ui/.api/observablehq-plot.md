# [TS_UI_API_OBSERVABLEHQ_PLOT]

[PACKAGE_SURFACE]:
- package: `@observablehq/plot` (ISC)
- module: `type: module`, ships ESM SOURCE directly (`main`/`module`/`default` → `src/index.js`, `umd` recovery build); `sideEffects: ["./src/index.js"]` — the entry is not tree-shakable past itself.
- asset: `types: src/index.d.ts`; deps: the whole `d catalog` metapackage (`.api/d3.md`), `interval-tree- catalogd`, `isoformat`; no peers.
- runtime: builds a detached SVG/figure element headlessly from data — browser or any DOM implementation; no React coupling.
- plane: `plane:runtime` (W4 `ui`); folder-local to `ui`, the statistical-charting lane.
- rail: `ui` data-viz — the one-call exploratory/analytical chart owner.

`Plot.plot(options)` is the whole authoring surface: an options value carrying `marks` rows returns a rendered `SVGSVGElement | HTMLElement` (a `<figure>` when legends/captions attach) extended with `scale(name)` and `legend(name, options?)` readbacks and an interactive `value` property. A mark is ONE parameterized shape — data + channel options + a transform slot — and the ~60-name roster is seed data on it: `dot` and `waffleY` differ only in geometry, every mark accepts the same shared option vocabulary, and every transform is an options→options rewriter composed into the mark row. Scales, axes, legends, facets, and projections are INFERRED from channels and overridden by scale options — the grammar means a statistical chart is a data declaration, not a composition program. That is the boundary against visx: Plot for the declared exploratory/analytic chart, visx for the bespoke interactive React-composed one.

## [01]-[ENTRY]

[SURFACES]: `plot(PlotOptions?) -> (SVGSVGElement|HTMLElement)&{scale(name:ScaleName):Scale|…` `marks(...Markish[]) -> CompoundMark`

- `PlotOptions`: `marks`, per-scale option objects (`x` `y` `r` `color` `opacity` `symbol` `length` `fx` `fy`), `facet`, `projection`, `width` `height` `aspectRatio` `margin*`, `style` `className` `clip`, `title` `subtitle` `caption`, `grid` (scale default).
- Named projections: `albers-usa` `albers` `azimuthal-equal-area` `azimuthal-equidistant` `conic-conformal` `conic-equal-area` `conic-equidistant` `equal-earth` `equirectangular` `gnomonic` `identity` `reflect-y` `mercator` `orthographic` `stereographic` `transverse-mercator` — or a d3 projection factory/implementation.
- Shared mark options (base `MarkOptions`): `filter` `reverse` `sort` `transform` `initializer` `render`, `fx` `fy` `facet` `facetAnchor`, `fill`/`stroke` families, `opacity` `mixBlendMode` `imageFilter` `paintOrder` `shapeRendering`, `dx` `dy` `clip` `margin*`, `title` `tip` `href` `target`, `ariaLabel`/`ariaDescription`/`ariaHidden` `pointerEvents` `className`, `channels` (extra named channels feeding tips). Positional `x`/`y`/`z`/`symbol` are per-mark, not base.

## [02]-[MARK_ROSTER]

Directional twins encode the fixed axis (`barY` = vertical bars); several families ship NO bare form — `barX/barY`, `ruleX/ruleY`, `tickX/tickY`, `linearRegressionX/Y`, `differenceX/Y`, `waffleX/Y` only.

| [INDEX] | [FAMILY]        | [MARKS]                                                                 | [AXIS_NOTE]                                 |
| :-----: | :-------------- | :---------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | point           | `dot` `dotX` `dotY` `circle` `hexagon`                                  | `circle`/`hexagon` = `dot` + fixed `symbol` |
|  [02]   | line/area       | `line` `lineX` `lineY` · `area` `areaX` `areaY`                         | series via `z`; curves via `curve`          |
|  [03]   | line/area       | `differenceX` `differenceY` · `bollingerX` `bollingerY` (+ `bollinger`) | difference shading; bollinger-band helper   |
|  [04]   | bar/rect/cell   | `barX` `barY` · `rect` `rectX` `rectY`                                  | bars = band scale; rects = intervals        |
|  [05]   | bar/rect/cell   | `cell` `cellX` `cellY` · `waffleX` `waffleY`                            | categorical cells; unit/waffle              |
|  [06]   | annotation      | `text` `textX` `textY` · `ruleX` `ruleY` · `tickX` `tickY`              | text, axis-spanning rules, 1-D ticks        |
|  [07]   | annotation      | `frame` `image` `link` `arrow` `vector` `vectorX` `vectorY` `spike`     | `spike` = `vector` shape variant            |
|  [08]   | statistical     | `boxX` `boxY` · `linearRegressionX` `linearRegressionY`                 | box = composite dot+bar+tick+rule           |
|  [09]   | statistical     | `density` `raster` `contour`                                            | spatial density/raster/contour              |
|  [10]   | hierarchy/graph | `tree` `cluster`                                                        | ride `treeNode`/`treeLink` transforms       |
|  [11]   | hierarchy/graph | `delaunayLink` `delaunayMesh` `hull` `voronoi` `voronoiMesh`            | Delaunay/Voronoi geometry                   |
|  [12]   | geo             | `geo` `sphere` `graticule`                                              | GeoJSON under the `projection` option       |
|  [13]   | axes/grids      | `axisX` `axisY` `axisFx` `axisFy`                                       | explicit only when inference needs override |
|  [14]   | axes/grids      | `gridX` `gridY` `gridFx` `gridFy`                                       | explicit grid override                      |
|  [15]   | interaction     | `tip` · `crosshair` `crosshairX` `crosshairY`                           | pointer tips + crosshair (see [04])         |
|  [16]   | interaction     | `pointer` `pointerX` `pointerY`                                         | nearest-datum → `value`/`input`             |
|  [17]   | auto            | `auto` `autoSpec`                                                       | heuristic mark/transform selection          |

The `density`/`raster`/`contour` marks accept spatial interpolators `interpolateNone`/`interpolateNearest`/`interpolatorBarycentric`/`interpolatorRandomWalk`.

## [03]-[TRANSFORM_ROSTER]

Transforms rewrite a mark's options — grouping/binning/stacking happen in the options value, never in a pre-shaped copy of the data.

| [INDEX] | [FAMILY] | [TRANSFORMS]                                                         | [ROLE]                                       |
| :-----: | :------- | :------------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | reduce   | `bin` `binX` `binY` · `group` `groupX` `groupY` `groupZ` · `find`    | histogram/categorical aggregation + reducers |
|  [02]   | layout   | `stackX` `stackY` (+`stackX1/X2/Y1/Y2`) · `dodgeX` `dodgeY`          | stacking + beeswarm dodge                    |
|  [03]   | layout   | `hexbin` · `shiftX` `shiftY`                                         | hex aggregation; lag/lead shift              |
|  [04]   | window   | `windowX` `windowY` `window` · `normalizeX` `normalizeY` `normalize` | moving aggregates; index-relative normalize  |
|  [05]   | select   | `select` `selectFirst` `selectLast`                                  | endpoint labeling                            |
|  [06]   | select   | `selectMinX` `selectMinY` `selectMaxX` `selectMaxY`                  | extreme labeling                             |
|  [07]   | basic    | `map` `mapX` `mapY` · `filter` `reverse` `sort` `shuffle`            | per-datum map; filter/order primitives       |
|  [08]   | basic    | `transform` `initializer`                                            | the low-level compose primitives             |
|  [09]   | spatial  | `centroid` `geoCentroid` · `treeNode` `treeLink`                     | geometry→point projection; hierarchy edges   |

Helpers: `valueof(data, value, type?)` `column(source?)` `identity` `indexOf`; formatters `formatNumber(locale?)` `formatMonth` `formatWeekday` `formatIsoDate`; intervals `timeInterval(period)` `utcInterval(period)` `numberInterval(period)` — Plot interval names are plain periods (`"day"`, `"3 months"`), never d3's compound `"utcDay"` spellings.

## [04]-[INTERACTION]

- `tip: true` on any mark (or an explicit `tip` mark) renders channel values at the pointed instant; extra `channels` rows surface in the tip.
- `pointer`/`pointerX`/`pointerY` filter a mark to the nearest datum, drive the root element's `value` property, and emit `input` events — the seam for chart-as-input.
- `crosshair`/`crosshairX`/`crosshairY` compose pointer + rules + axis text as one row.

## [05]-[INTEGRATION]

[STACK: the React seam (`.api/react.md`)] — Plot returns a DETACHED element: a component mounts it in an effect bracket (`containerRef.current.replaceChildren(plot(...))`, remove on cleanup) keyed on the decoded inputs; rebuild-per-change is the model (the grammar rebuilds cheaply — it is not an incremental engine, which is exactly why streaming/high-frequency series live on uplot). The chart spec derives from atom state; the `pointer` `value`/`input` seam writes back through the atom binding, never component state.

[STACK: `apache-arrow` columnar input (`.api/apache-arrow.md`)] — marks accept an Arrow `Table` as `data` with COLUMN-NAME channel shorthand (`Plot.dot(table, { x: "Date", y: "Anomaly" })`) and Arrow date-type detection — a wire-decoded Arrow frame plots with zero row materialization, the same zero-copy law the GeoArrow deck layers follow.

[STACK: the d3 substrate (`.api/d3.md`)] — Plot embeds d3 wholesale (its one runtime dep): scale options, `format`/`timeFormat` specifier strings, curve names, and projection factories pass through to d3 vocabularies. Data preparation beyond the transform roster uses `d3-array` folds beside the spec; a d3-rendered chart beside Plot is the split the substrate law forbids.

[STACK: `system/token` theming] — Plot emits scoped classes under `className` and inherits CSS custom properties; categorical `color.range`/continuous `color.scheme` resolve from the token authority per the `.api/d3.md` chromatic boundary (data-value colormaps free, UI palettes token-owned). `style` stays a last-resort override.

[BOUNDARY: the viz owners] — Plot owns declared exploratory/statistical charts (distributions, facets, regressions, calendars, small multiples). `@visx/*` owns bespoke interactive React-composed SVG (per-element handlers, RAC-integrated a11y, custom hit logic); `uplot` (`.api/uplot.md`) owns extreme-point-count streaming time series; `@perspective-dev/*` owns pivot-grid analytics; deck.gl owns geospatial GPU scale — Plot's `geo` mark serves statistical maps, never the live basemap.

## [06]-[RAIL_LAW]

- Owns: grammar-of-graphics charting — the `plot(options)` entry, the mark roster as channel-parameterized rows, the transform algebra (bin/group/stack/window/normalize/select/map/dodge/hexbin), inferred scales/axes/legends/facets, named projections, pointer/tip/crosshair interaction, `scale`/`legend` readbacks, and Arrow-native columnar input.
- Accept: chart specs as data derived from atom state, mounted through the effect bracket; transforms in the options value over pre-shaped data copies; `tip`/`pointer` for inspection with `value` written back through the atom; Arrow tables with column-name shorthand; explicit `axisX`/`gridY` marks only where inference needs override; `marks(...)` composites for reusable layers.
- Reject: hand-rolled d3 SVG beside a Plot spec; React reconciling Plot's output (the bracket replaces wholesale); streaming/high-frequency series here (uplot's regime); the live basemap here (maplibre/deck.gl); pre-aggregating in JS what `bin`/`group`/`window` declare; d3 compound interval strings in Plot options; a second grammar layer wrapping this one.
