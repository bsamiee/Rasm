# [TS_UI_API_VISX_GROUP]

[PACKAGE_SURFACE]:
- package: `@visx/group` (MIT)
- module: dual ESM/CJS via conditional `exports`; peers `react` + `@types/react` 18||19; sole dep `classnames` — a leaf package, zero d3.
- plane: `plane:runtime` (W4 `ui`); rail: the visx chart spine — the coordinate frame under `.api/visx-shape.md` / `.api/visx-axis.md`.

`@visx/group` is one component: `Group`, an `<g>` with `top`/`left` shorthand for the translate transform (or a raw `transform` string when the frame is not a pure offset), `className`, `innerRef`, and full `SVGProps<SVGGElement>` spread. It exists to make the margin convention STRUCTURAL — the chart body renders in inner coordinates inside `<Group top={margin.top} left={margin.left}>`, scales range over inner dimensions, and no shape or axis ever adds margin arithmetic to its own geometry. Nested `Group`s compose frames (facet cells, small-multiple grids, legend blocks) the same way.

## [01]-[SURFACE]

[GROUP_PROPS]: `GroupProps.top: number` `GroupProps.left: number` `GroupProps.transform: string` `GroupProps.className: string` `GroupProps.innerRef: React.Ref<SVGGElement>` `GroupProps.children: React.ReactNode`
[SURFACES]: `Group: React.FC<GroupProps>`

[COMPOSITION]: `svg(width,height)` `Group(top,left)`

## [02]-[INTEGRATION]

[STACK: the visx set (`.api/visx-responsive.md`, `.api/visx-scale.md`, `.api/visx-shape.md`, `.api/visx-axis.md`)] — `useParentSize` → margin fold → inner dimensions → scale ranges → one `Group` frame; every shape and axis renders margin-blind. Facets/small multiples are a mapped `Group` per cell over shared or per-cell scales.

[STACK: React ownership (`.api/react.md`)] — the frame is a real element: `data-*` state, handlers, and token classes attach at the group level, and `innerRef` is the measurement/portal anchor where an overlay (a floating tooltip via the `.api/floating-ui-react.md` positioner) anchors to a chart region.

## [03]-[RAIL_LAW]

- Owns: the SVG coordinate frame — the margin-convention translate, nested frame composition, and the group-level attach point for classes/handlers/refs.
- Accept: one root `Group` per chart owning the margin offset; nested `Group`s for facet/legend frames; raw `transform` only for non-offset frames.
- Reject: margin arithmetic inside shape/axis geometry; bare `<g transform>` strings where `top`/`left` express the offset; per-child translate spam a single frame collapses.
