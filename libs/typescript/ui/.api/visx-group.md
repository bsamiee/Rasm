# [TS_UI_API_VISX_GROUP]

`@visx/group` owns the SVG coordinate frame of the visx chart spine: one `Group` component renders a `<g>` whose `top`/`left` shorthand carries the margin-convention translate, so the margin lands structurally and no shape or axis folds offset arithmetic into its own geometry.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@visx/group`
- package: `@visx/group` (MIT)
- module: dual ESM/CJS via conditional `exports`; peers `react` + `@types/react` 18||19
- runtime: browser SVG; sole dep `classnames`, zero d3 — a leaf of the visx spine
- plane: `plane:runtime` (W4 `ui`)
- rail: the SVG coordinate frame every visx chart body renders inside

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: one prop contract, every field optional.

- `GroupProps`: `top`/`left` are numeric offsets folding to the `<g>` translate; `transform` overrides both with a raw string; `className` and `innerRef` (`Ref<SVGGElement>`) attach at the element; `children` is `ReactNode`; every remaining `SVGProps<SVGGElement>` attribute spreads through.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the single component.

[COMPOSITION]: `Group(top, left)` inside `svg(width, height)`

- `Group(GroupProps & SVGProps<SVGGElement>) -> JSX.Element`: emits one `<g>` as a default-export function component (no `forwardRef`); `top`/`left` render the margin translate, and the `SVGProps` remainder attaches classes, handlers, and `data-*` at the frame.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One root `Group` owns the chart margin offset, so shapes and axes render in inner coordinates and no geometry carries margin arithmetic; nested `Group`s compose facet cells, small-multiple grids, and legend blocks by the same translate.

[STACKING]:
- `@visx/responsive`(`.api/visx-responsive.md`): `useParentSize` width/height feeds the margin fold to inner dimensions, then the one `Group` translate the chart body renders inside.
- `@visx/shape`(`.api/visx-shape.md`) + `@visx/axis`(`.api/visx-axis.md`): shapes and axes render margin-blind inside the frame against scales ranged over inner dimensions; a facet grid maps one `Group` per cell over shared or per-cell scales.
- `react`(`.api/react.md`): the `<g>` is a real element — token classes, handlers, and `data-*` state attach at the group, and `innerRef` is the measurement and portal anchor.
- `floating-ui-react`(`.api/floating-ui-react.md`): `Group.innerRef` feeds the `ExtendedRefs` `reference` setter, anchoring a floating overlay to a chart region's `<g>`.
- within-lib: the `ui` chart rows wrap every body in one `Group`, so a panel measures once and each chart's shapes and axes stay margin-blind.

[RAIL_LAW]:
- Package: `@visx/group`
- Owns: the SVG coordinate frame — the margin-convention translate, nested frame composition, and the group-level attach point for classes, handlers, and refs.
- Accept: one root `Group` per chart owning the margin offset; nested `Group`s for facet and legend frames; a raw `transform` only for a non-offset frame.
- Reject: margin arithmetic inside shape or axis geometry; a bare `<g transform>` string where `top`/`left` express the offset; per-child translate spam a single frame collapses.
