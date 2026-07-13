# [TS_UI_API_VISX_SHAPE]

[PACKAGE_SURFACE]:
- package: `@visx/shape` · license `MIT`
- module: dual ESM/CJS via conditional `exports`; peers `react` + `@types/react` 18||19 (React 19 native at catalog-bound, `prop-types` dropped).
- asset: deps `@visx/curve` `@visx/group` `@visx/scale` `@visx/vendor` (the pinned d3 bundle) + `classnames`; curves are consumed INTERNALLY — no `curve*` member re-exports from this entry.
- plane: `plane:runtime` (W4 `ui`); rail: the visx chart spine — geometry components under `.api/visx-axis.md` / over `.api/visx-scale.md`.

`@visx/shape` renders `d3-shape` generators as components: each takes `data` plus accessor props (`x={(d) => xScale(getDate(d))}`), computes the path via the vendored generator, and emits a plain SVG element that participates fully in React — token classes, RAC-adjacent handlers, per-element a11y. This is exactly the capability split against Plot: every path here is an addressable React element. Components pair with children-as-function escape hatches (receive the computed path/generator for custom rendering), and the `D3ShapeFactories` re-exports (`arc` `area` `line` `pie` `radialLine` `stack` as config-object wrappers) cover headless geometry when no element must render.

## [01]-[COMPONENT_ROSTER]

Per-family behaviour notes carry to the keyed list below; every component emits an addressable React SVG element.

| [INDEX] | [FAMILY]     | [COMPONENTS]                                                                                           |
| :-----: | :----------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | line         | `LinePath` `Line` `LineRadial` `SplitLinePath`                                                         |
|  [02]   | area         | `Area` `AreaClosed` `AreaStack` `Stack`                                                                |
|  [03]   | bar          | `Bar` `BarRounded` `BarGroup` `BarGroupHorizontal` `BarStack` `BarStackHorizontal`                     |
|  [04]   | radial       | `Arc` `Pie`                                                                                            |
|  [05]   | link         | `LinkHorizontal` `LinkVertical` `LinkRadial` + `Link{Horizontal,Vertical,Radial}{Curve,Line,Step}`     |
|  [06]   | point/region | `Circle` `Polygon` (+`getPoints`/`getPoint`) `Threshold`                                               |
|  [07]   | stack policy | `stackOffset`/`STACK_OFFSETS`/`STACK_OFFSET_NAMES` `stackOrder`/`STACK_ORDERS`/`STACK_ORDER_NAMES`     |
|  [08]   | accessors    | `getX` `getY` `getSource` `getTarget` `getFirstItem` `getSecondItem` `getBandwidth` `degreesToRadians` |
|  [09]   | generators   | `D3ShapeFactories` (`arc` `area` `line` `pie` `radialLine` `stack`)                                    |

- [01]-[LINE]: `LinePath` the data line (accessors + `curve` + `defined`); `Line` a two-point segment; `SplitLinePath` per-segment styling.
- [02]-[AREA]: `AreaClosed` closes to a scale's zero; stacks take `keys` + offset/order policy.
- [03]-[BAR]: `Bar` is one rect; group/stack variants fold a band scale + keys into laid-out rects via children-as-function.
- [04]-[RADIAL]: generator props (`innerRadius`/`padAngle`/…); `Pie` children-as-function exposes arcs for custom slice rendering.
- [05]-[LINK]: hierarchy/graph edges; 12 typed variants on one source/target accessor shape, with matching `path*` factories.
- [06]-[POINT_REGION]: `Threshold` shades above/below-crossing regions between two series.
- [07]-[STACK_POLICY]: offset/order vocabularies as named lookup tables — `offset="wiggle"` is a data row, never a hand-computed baseline.
- [09]-[GENERATORS]: the headless generator escape when no element must render.

```ts signature
// Accessor-driven: scales close over the accessors, the component emits one styleable SVG element.
<LinePath<Point> data={series} x={(d) => x(d.t)} y={(d) => y(d.v)} curve={curveMonotoneX} className={cn("stroke-accent", lineClass)} />
<AreaClosed<Point> data={series} x={(d) => x(d.t)} y={(d) => y(d.v)} yScale={y} curve={curveMonotoneX} fill="url(#ramp)" />
<BarStack data={rows} keys={keys} x={getKey} xScale={x} yScale={y} color={colorFor}>
  {(stacks) => stacks.map((s) => s.bars.map((b) => <rect key={b.key + b.index} x={b.x} y={b.y} width={b.width} height={b.height} fill={b.color} />))}
</BarStack>
```

Curve law: the `curve` prop takes a d3 `CurveFactory` VALUE — `@visx/curve` is an internal dep with no re-export here, so curve values import from the admitted d3 substrate (`.api/d3.md` `curveMonotoneX`/`curveNatural`/…), structurally identical to the vendored factories.

## [02]-[INTEGRATION]

[STACK: the visx set (`.api/visx-scale.md`, `.api/visx-group.md`, `.api/visx-axis.md`, `.api/visx-responsive.md`)] — one chart = `useParentSize` dimensions → scale configs → shapes + axes inside a margin-translated `Group`; shapes and axes read the SAME scale instances, and accessors are named once per chart fold, never inlined divergently per shape.

[STACK: React ownership (`.api/react.md`)] — every emitted element is React-owned: token classes via `cn`, handlers/`data-*` state directly on shapes, transitions on paths through the `system/act` rows. This per-element addressability is what earns visx a surface over Plot; a chart not needing it belongs to Plot.

[STACK: stacks + the d3 substrate (`.api/d3.md`)] — `STACK_OFFSETS`/`STACK_ORDERS` are the same `stackOffset*`/`stackOrder*` vocabulary d3 documents, exposed as named lookup tables — stack policy is a data row (`offset="wiggle"`), never a hand-computed baseline.

[BOUNDARY] — geometry at DOM-hostile scale (10k+ points) leaves SVG: uplot (`.api/uplot.md`) for time series, deck.gl for spatial. `Threshold`/`AreaStack` cover the analytic shading Plot's `difference`/`stack` marks own in the declared-chart regime — the surface is chosen once per chart, not per layer.

## [03]-[RAIL_LAW]

- Owns: the SVG geometry roster — accessor-driven line/area/bar/radial/link/region components, stack layout with named offset/order policy tables, children-as-function layout escapes, and the headless `D3ShapeFactories`.
- Accept: accessors defined once per chart fold; scales from `@visx/scale` shared with axes; curve values from the d3 substrate; children-as-function for custom bar/pie/stack rendering; token classes on every element.
- Reject: hand-built `d` strings where a component/generator exists; per-shape scale reconstruction; expecting `curve*`/`@visx/point`/`@visx/text` members from this entry (not re-exported — d3 substrate or the owning package); SVG at uplot/deck.gl scale; mixing visx layers into a Plot-owned chart.
