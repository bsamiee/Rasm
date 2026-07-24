# [TS_UI_API_OBSERVABLEHQ_PLOT]

`plot(options)` is the whole authoring surface: an options value carrying `marks` rows renders a detached `SVGSVGElement | HTMLElement` extended with `scale`/`legend` readbacks and an interactive `value` property.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@observablehq/plot`
- package: `@observablehq/plot` (ISC)
- module: `type: module`, ESM source at `src/index.js` typed by `src/index.d.ts`; `sideEffects` pins the entry, so tree-shaking stops at the barrel.
- runtime: builds a detached SVG/figure element headlessly from any DOM; no React coupling.
- plane: `plane:runtime` (W4 `ui`), the statistical-charting lane.
- rail: `ui` data-viz — the one-call exploratory/analytic chart owner.

## [02]-[ENTRY]

[SURFACES]: `plot(PlotOptions?) -> (SVGSVGElement|HTMLElement)&Plot` `marks(...Markish[]) -> CompoundMark` · readbacks `Plot.scale(ScaleName)` `Plot.legend(ScaleName,LegendOptions?)` `Plot.value`
[PLOT_OPTIONS]: `marks`, per-scale objects `x` `y` `r` `color` `opacity` `symbol` `length` `fx` `fy`, `facet` `projection`, `width` `height` `aspectRatio` `margin*`, `style` `className` `clip`, `title` `subtitle` `caption`, `grid`.
[PROJECTIONS]: `albers-usa` `albers` `azimuthal-equal-area` `azimuthal-equidistant` `conic-conformal` `conic-equal-area` `conic-equidistant` `equal-earth` `equirectangular` `gnomonic` `identity` `reflect-y` `mercator` `orthographic` `stereographic` `transverse-mercator`, or a d3 projection factory.
[MARK_OPTIONS]: base `MarkOptions` — `filter` `reverse` `sort` `transform` `initializer` `render`, `fx` `fy` `facet` `facetAnchor`, `fill`/`stroke` families, `opacity` `mixBlendMode` `imageFilter` `paintOrder` `shapeRendering`, `dx` `dy` `clip` `margin*`, `title` `tip` `href` `target`, `ariaLabel` `ariaDescription` `ariaHidden` `pointerEvents` `className`, `channels`; positional `x` `y` `z` `symbol` are per-mark, never base.

## [03]-[MARK_ROSTER]

Directional twins fix the axis (`barY` = vertical bars); several families ship no bare form — `barX`/`barY`, `ruleX`/`ruleY`, `tickX`/`tickY`, `linearRegressionX`/`Y`, `differenceX`/`Y`, `waffleX`/`Y` only.

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
|  [14]   | axes/grids      | `gridX` `gridY` `gridFx` `gridFy` `hexgrid`                             | explicit grid; `hexgrid` overlays `hexbin`  |
|  [15]   | interaction     | `tip` · `crosshair` `crosshairX` `crosshairY`                           | pointer tips + crosshair                    |
|  [16]   | interaction     | `pointer` `pointerX` `pointerY`                                         | nearest-datum → `value`/`input`             |
|  [17]   | auto            | `auto` `autoSpec`                                                       | heuristic mark/transform selection          |

`density`/`raster`/`contour` accept spatial interpolators `interpolateNone` `interpolateNearest` `interpolatorBarycentric` `interpolatorRandomWalk`.

## [04]-[TRANSFORM_ROSTER]

Transforms rewrite a mark's options — binning, grouping, and stacking happen in the options value, never in a pre-shaped data copy.

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

Helpers `valueof(data, value, type?)` `column(source?)` `identity` `indexOf`; formatters `formatNumber(locale?)` `formatMonth` `formatWeekday` `formatIsoDate`; intervals `timeInterval(period)` `utcInterval(period)` `numberInterval(period)` take plain periods (`"day"`, `"3 months"`), never d3's compound `"utcDay"`.

## [05]-[INTERACTION]

- `tip: true` on any mark, or an explicit `tip` mark, renders channel values at the pointed instant; extra `channels` rows surface in the tip.
- `pointer`/`pointerX`/`pointerY` filter a mark to the nearest datum, drive the root element's `value`, and emit `input` — the chart-as-input seam.
- `crosshair`/`crosshairX`/`crosshairY` compose pointer, rules, and axis text as one row.

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every mark is one channel-parameterized shape folding the shared `MarkOptions` vocabulary through a transform slot; scales, axes, legends, facets, and projections infer from channels, so the spec is data, never a render program.

[STACKING]:
- `react`(`.api/react.md`): `plot(...)` returns a detached element a component mounts in an effect bracket (`containerRef.current.replaceChildren(plot(...))`, cleanup removes) keyed on decoded inputs; the spec derives from atom state and the `pointer` `value`/`input` seam writes back through the atom binding, never component state.
- `apache-arrow`(`.api/apache-arrow.md`): marks bind an Arrow `Table` as `data` with column-name channel shorthand (`Plot.dot(table, {x: "Date", y: "Anomaly"})`) and Arrow date detection, so a wire-decoded frame plots with zero row materialization.
- `d3`(`.api/d3.md`): Plot embeds d3 wholesale as its one runtime dep; scale options, `format`/`timeFormat` specifiers, curve names, and projection factories pass through to d3 vocabularies, and `d3-array` folds prepare data beside the spec.
- `system/token` theming: Plot emits scoped classes under `className` and inherits CSS custom properties; categorical `color.range` and continuous `color.scheme` resolve from the token authority per the `.api/d3.md` chromatic boundary, `style` last-resort only.

[LOCAL_ADMISSION]:
- Plot owns the declared exploratory/statistical chart — distributions, facets, regressions, calendars, small multiples; bespoke interactive React-composed SVG routes to `@visx/*`, extreme-point streaming series to `uplot`(`.api/uplot.md`), pivot-grid analytics to `@perspective-dev/*`, geospatial GPU scale to deck.gl, and the `geo` mark serves statistical maps, never the live basemap.

[RAIL_LAW]:
- Package: `@observablehq/plot`
- Owns: grammar-of-graphics charting — the `plot(options)` entry, the mark roster as channel-parameterized rows, the transform algebra, inferred scales/axes/legends/facets, named projections, pointer/tip/crosshair interaction, `scale`/`legend` readbacks, Arrow columnar input.
- Accept: chart specs as atom-derived data mounted through the effect bracket; transforms in the options value over pre-shaped copies; `tip`/`pointer` inspection writing `value` back through the atom; Arrow tables with column-name shorthand; explicit `axisX`/`gridY` only where inference needs override; `marks(...)` composites for reusable layers.
- Reject: hand-rolled d3 SVG beside a Plot spec; React reconciling Plot's output; streaming/high-frequency series here; the live basemap here; pre-aggregating in JS what `bin`/`group`/`window` declare; d3 compound interval strings; a second grammar layer wrapping this one.
