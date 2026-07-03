# [API_CATALOGUE] @floating-ui/react-dom

`@floating-ui/react-dom` supplies the React DOM binding for the Floating UI positioning engine. It wraps core `@floating-ui/dom` middleware factories with React-aware `deps` arrays, exposes `useFloating` for anchor-based positioning, re-exports all `@floating-ui/dom` geometry types, and is the direct dependency of `@floating-ui/react` for the positioning layer consumed by every tooltip, popover, dropdown, and overlay owner in the ui stack.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@floating-ui/react-dom`
- package: `@floating-ui/react-dom`
- module: `@floating-ui/react-dom`
- namespace: named exports from `dist/floating-ui.react-dom.d.ts`
- asset: React DOM positioning hook and middleware factories
- rail: position

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: position result family
- rail: position

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [RAIL]                                                                               |
| :-----: | :----------------------- | :---------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `UseFloatingOptions<RT>` | hook config       | `Partial<ComputePositionConfig>` + React extensions                                  |
|  [02]   | `UseFloatingReturn<RT>`  | hook result       | position data + refs + styles + `update`                                             |
|  [03]   | `UseFloatingData`        | position snapshot | `ComputePositionReturn & { isPositioned: boolean }`                                  |
|  [04]   | `ReferenceType`          | reference union   | `Element \| VirtualElement`                                                          |
|  [05]   | `ArrowOptions`           | arrow config      | `{ element: MutableRefObject<Element\|null> \| Element \| null; padding?: Padding }` |

[PUBLIC_TYPE_SCOPE]: re-exported geometry family
- rail: position

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]    | [RAIL]                                         |
| :-----: | :----------------- | :--------------- | :--------------------------------------------- |
|  [01]   | `Placement`        | placement union  | side + alignment string literal                |
|  [02]   | `AlignedPlacement` | aligned subtype  | placements with explicit alignment             |
|  [03]   | `Side`             | side union       | `"top" \| "right" \| "bottom" \| "left"`       |
|  [04]   | `Alignment`        | alignment union  | `"start" \| "end"`                             |
|  [05]   | `Strategy`         | position mode    | `"absolute" \| "fixed"`                        |
|  [06]   | `Middleware`       | middleware shape | `{ name; fn(state) => MiddlewareReturn }`      |
|  [07]   | `MiddlewareData`   | middleware bag   | keyed middleware output per name               |
|  [08]   | `MiddlewareState`  | middleware input | position state passed to each middleware       |
|  [09]   | `Coords`           | x/y record       | `{ x: number; y: number }`                     |
|  [10]   | `SideObject`       | side record      | `{ top; right; bottom; left: number }`         |
|  [11]   | `Dimensions`       | size record      | `{ width: number; height: number }`            |
|  [12]   | `VirtualElement`   | virtual ref      | `{ getBoundingClientRect(); contextElement? }` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: positioning hook
- rail: position

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [RAIL]                                                    |
| :-----: | :-------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `useFloating<RT>(options?)` | position hook  | returns `UseFloatingReturn<RT>` with refs, styles, update |

[ENTRYPOINT_SCOPE]: React-aware middleware factories
- rail: position

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY]        | [RAIL]                                          |
| :-----: | :------------------------------- | :-------------------- | :---------------------------------------------- |
|  [01]   | `arrow(options, deps?)`          | arrow middleware      | centers arrow element, accepts React ref        |
|  [02]   | `autoPlacement(options?, deps?)` | placement middleware  | auto-selects best placement                     |
|  [03]   | `flip(options?, deps?)`          | placement middleware  | flips to opposite side on overflow              |
|  [04]   | `shift(options?, deps?)`         | overflow middleware   | shifts along axis to stay in view               |
|  [05]   | `offset(options?, deps?)`        | offset middleware     | translates floating element                     |
|  [06]   | `hide(options?, deps?)`          | visibility middleware | hides when reference is out of clipping context |
|  [07]   | `size(options?, deps?)`          | size middleware       | resizes floating element to available space     |
|  [08]   | `inline(options?, deps?)`        | inline middleware     | improves positioning for inline references      |
|  [09]   | `limitShift(options?, deps?)`    | shift limiter         | stops shift at a configurable threshold         |

[ENTRYPOINT_SCOPE]: utility functions
- rail: position

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY]  | [RAIL]                                         |
| :-----: | :------------------------------------------------------- | :-------------- | :--------------------------------------------- |
|  [01]   | `autoUpdate(reference, floating, update, options?)`      | auto-updater    | runs `update` on scroll, resize, mutations     |
|  [02]   | `computePosition(reference, floating, config)`           | static query    | returns `ComputePositionReturn` promise        |
|  [03]   | `detectOverflow(state, options?)`                        | overflow probe  | returns `SideObject` overflow amounts          |
|  [04]   | `getOverflowAncestors(element, list?, traverseIframes?)` | DOM util        | returns scrollable ancestor list               |
|  [05]   | `platform`                                               | platform object | default DOM platform used by `computePosition` |

## [04]-[IMPLEMENTATION_LAW]

[POSITION_TOPOLOGY]:
- `useFloating` returns `refs.setReference` and `refs.setFloating` callback setters plus `floatingStyles: React.CSSProperties` for direct style application
- middleware factories accept an optional `deps?: React.DependencyList` second argument; pass dependencies when options are computed from reactive state to avoid stale closures
- `UseFloatingOptions.whileElementsMounted` receives `autoUpdate` to re-run positioning on DOM mutations, scroll, and resize; returns a cleanup function
- `isPositioned` in `UseFloatingData` is `false` until the first position calculation completes; use it to suppress flicker

[LOCAL_ADMISSION]:
- Spread `floatingStyles` directly onto the floating element's `style` prop; do not manually apply `top`/`left`/`transform` outside this return value.
- Pass `open` in `UseFloatingOptions` so `isPositioned` resets correctly when the floating element is hidden.
- Use `@floating-ui/react` (not this package directly) when interaction hooks (`useClick`, `useHover`, `useFocus`, `useDismiss`) are needed.

[RAIL_LAW]:
- Package: `@floating-ui/react-dom`
- Owns: React DOM positioning hook and React-aware middleware wrappers
- Accept: refs via `refs.setReference` / `refs.setFloating`; middleware via `UseFloatingOptions.middleware`
- Reject: manual `computePosition` calls when `useFloating` covers the same use case
