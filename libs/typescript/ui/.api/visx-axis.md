# [TS_UI_API_VISX_AXIS]

[PACKAGE_SURFACE]:
- package: `@visx/axis` · license `MIT`
- module: dual ESM/CJS via conditional `exports`; peers `react` + `@types/react` 18||19.
- asset: deps `@visx/group` `@visx/point` `@visx/scale` `@visx/shape` `@visx/text` + `classnames` — tick labels render through `@visx/text`'s SVG `Text` (its `TextProps` is the `TickLabelProps` vocabulary), tick marks through `@visx/shape`'s `Line`.
- plane: `plane:runtime` (W4 `ui`); rail: the visx chart spine — the axis face over `.api/visx-scale.md` scales.

`@visx/axis` renders what `d3-axis` has painted into a selection — as React elements. ONE generic `Axis` component takes any `AxisScale` plus an `Orientation` value; `AxisTop`/`AxisRight`/`AxisBottom`/`AxisLeft` are the four orientation-preset rows on it, not four implementations. Ticks are computed from the scale (`ComputedTick` values), and every stage is an override point: `TickFormatter<T>` for value→label, `TickLabelProps<T>` (per-tick `TextProps`) for label styling, `TickRendererProps`/`TicksRendererProps` components for full tick replacement, and `AxisRendererProps` for replacing the entire axis rendering while keeping the tick computation — policy layers, from a format string up to a bespoke renderer, all on one component.

## [01]-[SURFACE]

| [INDEX] | [SURFACE]                                                                                                                                                        | [FAMILY]     | [CAPABILITY]                                                                             |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `Axis` (`AxisProps<Scale>`)                                                                                                                                      | the owner    | generic over `AxisScale`; orientation, tick policy, label, line/tick visibility as props |
|  [02]   | `AxisTop` `AxisRight` `AxisBottom` `AxisLeft`                                                                                                                    | preset rows  | `Orientation`-fixed instances of [01]                                                    |
|  [03]   | `Orientation`                                                                                                                                                    | vocabulary   | the closed orientation axis (`top`/`right`/`bottom`/`left`)                              |
|  [04]   | `AxisScale` / `AxisScaleOutput`                                                                                                                                  | type gate    | any d3 scale with a numeric-output range — the `@visx/scale` results pass directly       |
|  [05]   | `TickFormatter<T>` · `TickLabelProps<T>` · `TickRendererProps` / `TicksRendererProps` · `ComputedTick` · `AxisRendererProps` · `SharedAxisProps` / `CommonProps` | policy types | the override ladder: format → label props → tick component → whole-axis renderer         |

```ts contract
// One scale object feeds shapes AND its axis; policy is props, labels are @visx/text TextProps per tick.
<AxisBottom scale={x} top={innerHeight} tickFormat={fmt} tickLabelProps={() => ({ className: "fill-muted text-2xs", textAnchor: "middle" })} />
<AxisLeft scale={y} numTicks={density.rows} label={axisLabel} />
```

Tick VALUES/format derive from the d3 substrate where the scale's own vocabulary rules (`scale.ticks`, `format`/`timeFormat` specifiers per `.api/d3.md`); the axis renders them, it never invents a second tick algebra.

## [02]-[INTEGRATION]

[STACK: the visx set (`.api/visx-scale.md`, `.api/visx-shape.md`, `.api/visx-group.md`, `.api/visx-responsive.md`)] — the axis consumes the SAME scale instance the shapes read, positioned by the margin convention inside the chart `Group`; `useParentSize` dimension changes flow through the scale rebuild, and the axis follows — no axis-local geometry state.

[STACK: token + a11y (`.api/react.md`)] — axis line, ticks, and labels are React SVG elements: token classes via `tickLabelProps`/`labelProps`, and the rendered text participates in the DOM (selectable, inspectable, styleable) — the accessibility surface canvas engines cannot give, and the reason axes render here rather than as uplot-style painted ticks in SVG charts.

[BOUNDARY] — Plot infers and renders its own axes (`axisX`/`axisY` marks, `.api/observablehq-plot.md`); a visx `Axis` never decorates a Plot chart. The rejected `d3-axis` module never appears — this package IS its React replacement.

## [03]-[RAIL_LAW]

- Owns: axis rendering for visx charts — the generic `Axis`, the four orientation presets, the `Orientation` vocabulary, and the typed tick-policy override ladder.
- Accept: the shared scale instance as the one input; format/label policy as data-shaped props; `tickComponent`/renderer overrides only when the prop ladder is exhausted; token classes through the props vocabulary.
- Reject: `d3-axis` anywhere; a second tick computation beside the scale's; axis-local copies of chart geometry; per-render `tickLabelProps` closures recreating identical objects; visx axes on Plot- or uplot-owned surfaces.
