# [TS_UI_API_VISX_SHAPE]

`@visx/shape` owns the SVG geometry roster of the visx chart spine: each component renders a vendored `d3-shape` generator as one addressable React `<svg>` element — accessor props in, path out, token classes and handlers and per-element a11y on every node. This per-element addressability earns visx its surface over Plot; headless config-object factories cover geometry when no element must render.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@visx/shape`
- package: `@visx/shape` (MIT)
- module: dual ESM/CJS via conditional `exports`; peers `react` + `@types/react` 18||19
- runtime: browser SVG; deps `@visx/curve` `@visx/group` `@visx/scale` `@visx/vendor` (the pinned d3 bundle) + `classnames` — curves are consumed internally, so no `curve*` member re-exports from this entry
- plane: `plane:runtime` (W4 `ui`)
- rail: the visx SVG geometry roster — line/area/bar/radial/link/region components rendering against `@visx/scale` scales inside a `@visx/group` frame

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the vocabulary generic chart wrappers constrain against.

[SHAPE_TYPES]: `AddSVGProps` `PositionScale` `Accessor` `AccessorForArrayItem` `StackOffset` `StackOrder` `PathType` `SeriesPoint`

- `AddSVGProps<OwnProps, Element>` merges a component's own props with `SVGProps<Element>` minus the keys the component owns, so every shape accepts native SVG attributes with no prop collision; `PositionScale` gates the scale a shape's accessors read, `StackOffset`/`StackOrder` the offset and order selector unions, and `Accessor`/`SeriesPoint` the datum-in and stack-point-out shapes.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: every component emits one addressable React SVG element; accessors, stack policy, and the config-object factories are the headless helpers.

| [INDEX] | [FAMILY] | [SURFACE]                                                                            | [CAPABILITY]               |
| :-----: | :------- | :----------------------------------------------------------------------------------- | :------------------------- |
|  [01]   | line     | `LinePath` `Line` `LineRadial` `SplitLinePath`                                       | accessor-driven line paths |
|  [02]   | area     | `Area` `AreaClosed` `AreaStack` `Stack`                                              | filled and stacked bands   |
|  [03]   | bar      | `Bar` `BarRounded` `BarGroup` `BarGroupHorizontal` `BarStack` `BarStackHorizontal`   | rect and grouped layout    |
|  [04]   | radial   | `Arc` `Pie`                                                                          | arc and pie geometry       |
|  [05]   | link     | `LinkHorizontal` `LinkVertical` `LinkRadial` + `{Curve,Line,Step,Diagonal}` variants | hierarchy edge paths       |
|  [06]   | region   | `Circle` `Polygon`                                                                   | point and polygon marks    |

- [01]-[LINE]: `LinePath` folds `data` through `x`/`y` accessors with `curve` and `defined`; `Line` draws a two-point segment; `LineRadial` takes `angle`/`radius`; `SplitLinePath` styles each segment.
- [02]-[AREA]: `AreaClosed` closes the fill to a scale's zero; `AreaStack`/`Stack` take `keys` with offset and order policy, exposing laid-out bands through children-as-function.
- [03]-[BAR]: `Bar` emits one rect; group and stack variants fold a band scale and `keys` into laid-out rects via children-as-function.
- [04]-[RADIAL]: `Arc` takes `innerRadius`/`outerRadius`/`padAngle`; `Pie` exposes each arc datum through children-as-function for custom slice rendering.
- [05]-[LINK]: source and target accessors drive hierarchy and graph edges; the path shape selects `Diagonal` (the base default), `Curve`, `Line`, or `Step`, each with a matching `path*` factory.
- [06]-[REGION]: `Circle` marks a point; `Polygon` builds an N-gon, with `getPoints`/`getPoint` computing its vertices.
- [ACCESSORS]: `getX` `getY` `getSource` `getTarget` `getFirstItem` `getSecondItem` `getBandwidth` `degreesToRadians` — the default datum and trig accessors a component reads unless a prop overrides one.
- [STACK_POLICY]: `stackOffset` `STACK_OFFSETS` `STACK_OFFSET_NAMES` `stackOrder` `STACK_ORDERS` `STACK_ORDER_NAMES` — offset and order vocabularies as named lookup tables, so `offset="wiggle"` is a data row, never a hand-computed baseline.
- [GENERATORS]: `arc` `area` `line` `pie` `radialLine` `stack` — raw `d3-shape` generators for headless geometry when no element renders.

[COMPOSITION]: `LinePath(data, x, y)` `AreaClosed(data, x, y0, y1)` `BarStack(data, keys, x, xScale, yScale, color)`

`curve` binds a d3 `CurveFactory` value, never a `@visx/curve` re-export — that dep stays internal here, so curve values import from the d3 substrate (`.api/d3.md` `curveMonotoneX`/`curveNatural`), structurally identical to the vendored factories.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every component computes its path through the vendored `d3-shape` generator and emits a plain SVG element, so one accessor fold per chart feeds line, area, bar, radial, link, and region alike and each path stays an addressable React node rather than a canvas draw.

[STACKING]:
- `@visx/scale`(`.api/visx-scale.md`): a shape's `x`/`y` accessor props close over the scale object `createScale` returns, the same instance an `Axis` reads, so shapes and axes share one scale per chart.
- `@visx/responsive`(`.api/visx-responsive.md`): `useParentSize` width and height set the scale `range`, and shape geometry re-derives on resize with no path arithmetic re-authored.
- `@visx/group`(`.api/visx-group.md`): shapes render margin-blind inside one `Group` translate, so no component folds the margin offset into its own geometry.
- `@visx/axis`(`.api/visx-axis.md`): `Axis` consumes this package's `Line` for its tick marks and shares the shape's scale instance for tick placement.
- `@visx/vendor` d3 bundle (`.api/d3.md`): the `curve` prop binds a d3 `CurveFactory` (`curveMonotoneX`), and `STACK_OFFSETS`/`STACK_ORDERS` mirror d3's `stackOffset*`/`stackOrder*` families as named tables.
- `react`(`.api/react.md`): every emitted node is React-owned — token classes via `cn`, handlers and `data-*` on the element, path transitions through the `system/act` rows.
- within-lib: the `ui` chart rows fold accessors once per chart and thread the shared scales into every shape and axis, never inlining a divergent accessor per shape.

[LOCAL_ADMISSION]:
- Admit `@visx/shape` as the sole SVG geometry owner of the visx spine; every path renders through a component or a headless factory, never a hand-built `d` string.
- Geometry at DOM-hostile point counts leaves SVG — `uplot`(`.api/uplot.md`) owns dense time series and a GPU engine owns spatial maps — and a declared chart needing no per-element addressability routes to `observablehq-plot`(`.api/observablehq-plot.md`), whose `stackY`/`differenceX` marks own the stacked-band shading `AreaStack` covers here.

[RAIL_LAW]:
- Package: `@visx/shape`
- Owns: the SVG geometry roster — accessor-driven line, area, bar, radial, link, and region components, stack layout with named offset and order tables, children-as-function layout escapes, and the headless config-object factories.
- Accept: accessors folded once per chart; scales from `@visx/scale` shared with axes; curve values from the d3 substrate; children-as-function for custom bar, pie, and stack rendering; token classes on every element.
- Reject: a hand-built `d` string where a component or factory exists; per-shape scale reconstruction; expecting `curve*` or a `Threshold` shading member from this entry, where curves ride the d3 substrate and threshold shading is its own package; SVG at `uplot` or GPU-engine point counts; visx layers mixed into a Plot-owned chart.
