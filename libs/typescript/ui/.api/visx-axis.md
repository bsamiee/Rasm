# [TS_UI_API_VISX_AXIS]

`@visx/axis` owns the axis face of the visx chart spine: one generic `Axis` renders `d3-axis` geometry as React SVG from any `AxisScale` and an `Orientation`, the presets fix that orientation as rows, and the override ladder graduates on the one component — format string, per-tick label styling, replacement tick component, then a whole-axis renderer that keeps the tick computation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@visx/axis`
- package: `@visx/axis` (MIT)
- module: dual ESM/CJS via conditional `exports`; peers `react` + `@types/react` 18||19
- runtime: browser SVG; deps `@visx/group` `@visx/point` `@visx/scale` `@visx/shape` `@visx/text` + `classnames`
- plane: `plane:runtime` (W4 `ui`)
- rail: the visx chart spine — the axis face over `.api/visx-scale.md` scales

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the orientation, tick-policy, and render-override vocabulary `Axis` reads.

[AXIS_TYPES]: `Orientation` `AxisScale` `AxisScaleOutput` `TickFormatter` `TickLabelProps` `TickRendererProps` `TicksRendererProps` `ComputedTick` `AxisRendererProps` `SharedAxisProps` `CommonProps`

- `Orientation` is the closed `top`/`right`/`bottom`/`left` vocabulary the presets fix; `AxisScale`/`AxisScaleOutput` gate any d3 scale with numeric output; `TickFormatter`/`TickLabelProps` type the value→label formatter and per-tick `TextProps` styling; the override ladder climbs `TickRendererProps` (per tick) to `TicksRendererProps` (all ticks) to `AxisRendererProps`/`SharedAxisProps`/`CommonProps` (whole axis), each keeping the scale's tick computation, and `ComputedTick` carries one resolved value and position.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the generic axis and its orientation-fixed presets.

| [INDEX] | [FAMILY] | [SURFACE]                                     | [CAPABILITY]                                  |
| :-----: | :------- | :-------------------------------------------- | :-------------------------------------------- |
|  [01]   | owner    | `Axis(AxisProps<Scale>)`                      | generic over any `AxisScale`; policy as props |
|  [02]   | presets  | `AxisTop` `AxisRight` `AxisBottom` `AxisLeft` | `Orientation`-fixed instances of `Axis`       |

[COMPOSITION]: `AxisBottom(scale, top, tickFormat, tickLabelProps)` `AxisLeft(scale, numTicks, label)`

- Tick values and format derive from the scale's own d3 vocabulary (`scale.ticks`, `format`/`timeFormat` specifiers, `.api/d3.md`); `Axis` renders them and never mints a second tick algebra.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Axis` computes ticks from the scale and emits line, ticks, and labels as React SVG elements, so orientation and the override ladder ride props on the one component and the presets add no second implementation.

[STACKING]:
- `@visx/scale`(`.api/visx-scale.md`): `Axis` reads the SAME scale instance the shapes read, so tick placement and shape geometry share one scale per chart, and ticks and format come from the scale's d3 vocabulary, never an axis-local algebra.
- `@visx/shape`(`.api/visx-shape.md`): `Axis` renders its tick marks through this package's `Line`, and its tick labels through `@visx/text`'s `Text` whose `TextProps` is the `TickLabelProps` vocabulary.
- `@visx/group`(`.api/visx-group.md`): `Axis` positions inside the chart `Group` by the margin convention, holding no axis-local geometry offset.
- `@visx/responsive`(`.api/visx-responsive.md`): `useParentSize` dimension changes flow through the scale rebuild and the axis follows, holding no axis-local geometry state.
- `react`(`.api/react.md`): line, ticks, and labels are React SVG nodes taking token classes through `tickLabelProps`/`labelProps`; the rendered text stays in the DOM — selectable, inspectable, styleable — the accessibility surface a canvas engine cannot give.
- within-lib: the `ui` chart rows pass the shared scale and data-shaped format and label policy into one `Axis` per axis.

[LOCAL_ADMISSION]:
- Plot renders its own axes (`axisX`/`axisY` marks, `.api/observablehq-plot.md`), so a visx `Axis` never decorates a Plot chart; `@visx/axis` is the React replacement for `d3-axis`, which never enters directly.

[RAIL_LAW]:
- Package: `@visx/axis`
- Owns: axis rendering for visx charts — the generic `Axis`, its orientation presets, and the typed tick-policy override ladder.
- Accept: the shared scale instance as the one input; format and label policy as data-shaped props; `tickComponent`/renderer overrides only when the prop ladder is exhausted; token classes through the props vocabulary.
- Reject: `d3-axis` anywhere; a second tick computation beside the scale's; axis-local copies of chart geometry; per-render `tickLabelProps` closures recreating identical objects; visx axes on Plot- or uplot-owned surfaces.
