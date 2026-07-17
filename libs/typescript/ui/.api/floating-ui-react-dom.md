# [TS_UI_API_FLOATING_UI_REACT_DOM]

`@floating-ui/react-dom` is the React DOM binding for the `@floating-ui/dom` positioning engine — the substrate `@floating-ui/react` builds its interaction layer on. It exposes one `useFloating` hook that returns callback ref-setters (`refs.setReference`/`refs.setFloating`), a ready-to-spread `floatingStyles: React.CSSProperties`, an `isPositioned` flicker-guard, and an `update` function, plus React-aware wrappers of every `@floating-ui/dom` middleware factory (`offset`/`flip`/`shift`/`arrow`/`size`/`hide`/`inline`/`autoPlacement`/`limitShift`) that each accept a `Derivable` option (a function of the live `MiddlewareState`) and a `deps: React.DependencyList` second argument so reactive options recompute without stale closures. It re-exports the full `@floating-ui/dom` geometry vocabulary and the imperative engine (`computePosition`/`autoUpdate`/`detectOverflow`). A `view/compose` row imports `@floating-ui/react` for anything interactive; this package is imported directly only for interaction-free anchoring — a static positioned badge, a measured non-dismissible label.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@floating-ui/react-dom`
- package: `@floating-ui/react-dom` (MIT, © Floating UI contributors)
- module format: ESM + UMD (`dist/floating-ui.react-dom.esm.js`, `.d.ts` at `dist/floating-ui.react-dom.d.ts`), `sideEffects: false`; single `.` entry
- runtime target: React DOM (browser) — the hook binds to real DOM nodes and produces inline `position`/`transform` styles
- peer: `react@>=catalog`, `react-dom@>=catalog`; dep `@floating-ui/dom@^catalog` (the framework-agnostic engine this package wraps)
- asset: React positioning hook + React-aware middleware factories; the substrate of `@floating-ui/react`
- rail: position (the geometry layer under the `view/compose` floating-anchor/sheet rows)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: position result family — the hook config and return
- rail: position

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [CONSUMER]                                                                              |
| :-----: | :----------------------- | :---------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `UseFloatingOptions<RT>` | hook config       | `Partial<ComputePositionConfig>` + `whileElementsMounted`/`open`/`elements`/`transform` |
|  [02]   | `UseFloatingReturn<RT>`  | hook result       | `refs`/`elements` + `floatingStyles` + `update()` + `UseFloatingData`; no `context` here |
|  [03]   | `UseFloatingData`        | position snapshot | `ComputePositionReturn & { isPositioned: boolean }` — `x`/`y`/`placement`/`strategy`    |
|  [04]   | `ReferenceType`          | reference union   | `Element \| VirtualElement` — real node or virtual rect (cursor/selection)              |
|  [05]   | `ArrowOptions`           | arrow config      | `{ element, padding? }`; `element` is the `arrow` ref target (React ref or node)        |

[PUBLIC_TYPE_SCOPE]: re-exported geometry — the `@floating-ui/dom` vocabulary
- rail: position

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]       | [CONSUMER]                                                   |
| :-----: | :---------------------------------------------- | :------------------ | :----------------------------------------------------------- |
|  [01]   | `Placement` / `AlignedPlacement`                | placement union     | `'top'\|'right'\|'bottom'\|'left'` × `'start'\|'end'`        |
|  [02]   | `Side` / `Alignment`                            | placement axis      | the side and alignment halves a `Placement` composes         |
|  [03]   | `Strategy`                                      | position mode       | `'absolute' \| 'fixed'`; `fixed` escapes clip/transform      |
|  [04]   | `Middleware` / `MiddlewareState`                | middleware contract | `{ name; fn(state) => MiddlewareReturn }` — the stage shape  |
|  [05]   | `MiddlewareArguments` / `MiddlewareReturn`      | middleware fn io    | the `fn(state)` argument bag and its return                  |
|  [06]   | `MiddlewareData`                                | middleware output   | keyed by stage name (`arrow`/`hide`/`offset`)                |
|  [07]   | `Derivable<T>`                                  | reactive option     | `(state: MiddlewareState) => T` from the live position state |
|  [08]   | `Coords` / `Rect` / `Dimensions` / `SideObject` | box geometry        | `{ x, y }`; a rect; `{ width, height }`; per-side numbers    |
|  [09]   | `VirtualElement` / `Padding` / `Boundary`       | anchor + overflow   | virtual-anchor shape; overflow padding; clip boundary        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: positioning hook
- rail: position
- `useFloating<RT>(options?)` returns `{ refs, elements, floatingStyles, update, placement, middlewareData, isPositioned }`; the interaction-layer `context` is added only by `@floating-ui/react`'s `useFloating`, never this hook

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [CONSUMER]                                                                          |
| :-----: | :-------------------------- | :------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `useFloating<RT>(options?)` | position hook  | `view/compose` — spread `floatingStyles`; `isPositioned` guards first-paint flicker |

[ENTRYPOINT_SCOPE]: React-aware middleware factories — `(options | Derivable, deps?)`
- rail: position

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [CONSUMER]                                                                           |
| :-----: | :-------------------------- | :------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `offset` / `flip` / `shift` | core pipeline  | canonical `offset → flip → shift`: gap, flip cross-axis, slide main-axis on overflow |
|  [02]   | `arrow` / `size`            | measurement    | center the arrow on the reference; clamp to `availableWidth`/`availableHeight`       |
|  [03]   | `autoPlacement` / `hide`    | placement      | auto-pick the best side; hide when the reference is clipped                          |
|  [04]   | `inline` / `limitShift`     | visibility     | anchor inline (wrapped-text) references; bound `shift` displacement                  |

[ENTRYPOINT_SCOPE]: imperative engine and DOM utilities
- rail: position

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [CONSUMER]                                       |
| :-----: | :------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `autoUpdate(reference, floating, update, options?)`      | auto-updater   | the required `whileElementsMounted` value        |
|  [02]   | `computePosition(reference, floating, config)`           | imperative     | one-shot position query outside the hook         |
|  [03]   | `detectOverflow(state, options?)`                        | imperative     | one-shot overflow query (measurement, tests)     |
|  [04]   | `getOverflowAncestors(element, list?, traverseIframes?)` | DOM util       | scrollable-ancestor set `autoUpdate` listens on  |
|  [05]   | `platform`                                               | DOM platform   | the default DOM platform `computePosition` binds |

## [04]-[IMPLEMENTATION_LAW]

[POSITION_TOPOLOGY]:
- `useFloating` returns `refs.setReference`/`refs.setFloating` callback setters and a `floatingStyles: React.CSSProperties` object; spreading `floatingStyles` onto the float's `style` prop is the only correct application — the hook owns `position`/`top`/`left`/`transform`, and hand-writing them fights the engine. `isPositioned` is `false` until the first calculation resolves, so a float renders hidden until placed, killing first-paint flicker.
- Middleware is an ordered pipeline: each factory returns a `Middleware` whose `fn` reads and mutates `MiddlewareState`, and order is semantic — `offset` before `flip` before `shift` before `arrow`/`size`, because each stage consumes the prior stage's coordinates. `MiddlewareData` keys each stage's output by name so the consumer reads `middlewareData.arrow.x` or `middlewareData.hide.referenceHidden`.
- `deps` second argument and `Derivable` options are the React-reactivity seam: pass `deps` when a middleware's options derive from reactive state (a dynamic `offset` from a prop), and use a `Derivable` (`(state) => options`) when the option depends on the live placement — both prevent the stale-closure bug a plain object option does cause.
- `whileElementsMounted: autoUpdate` is mandatory for a persistent float: it re-runs positioning on scroll, resize, DOM mutation, and layout shift, and returns a cleanup the hook calls on unmount. Without it a float positions once and drifts.
- `Strategy` `'fixed'` escapes `overflow`/`transform`/`contain` clipping ancestors; `'absolute'` (default) positions within the nearest positioned ancestor — a portaled float uses `fixed` to break out of a scroll container.

[STACKS_WITH]:
- `@floating-ui/dom` (dep): the framework-agnostic engine this package wraps — `computePosition`, the middleware algorithms, and the geometry types are `dom`'s; react-dom adds the `useFloating` hook, `floatingStyles`, the `deps` arrays, and React ref integration. No positioning logic is re-implemented here.
- `@floating-ui/react` (dependent, `libs/typescript/ui/.api/floating-ui-react.md`): the full surface that re-exports everything here and adds interaction/focus/portal/tree. A `view/compose` row imports `@floating-ui/react`; it drops to `@floating-ui/react-dom` directly only for interaction-free anchoring (a static badge, a measured decorative pointer) where `useClick`/`useDismiss`/`FloatingFocusManager` are dead weight.
- `react` (peer): `floatingStyles` is `React.CSSProperties`, the ref setters are React callback refs, and `deps` is a `React.DependencyList` — the hook is the React-native form of the imperative `computePosition` + `autoUpdate` pair.
- `token/theme` (sibling row): the float element carries the design-token classes/styles beside the spread `floatingStyles`; the two never conflict because floating-ui owns only `position`/`top`/`left`/`transform` and the token layer owns the visual box.
- react-aria overlay hooks (sibling rows, via `@floating-ui/react`): react-aria's `useOverlayPosition` is the react-aria-native positioner; when a row commits to floating-ui for geometry it uses this engine and lets react-aria own only ARIA/dismiss — the two positioners never both drive one element.

[LOCAL_ADMISSION]:
- Prefer `@floating-ui/react`; import `@floating-ui/react-dom` directly only when the float needs no interaction, focus, portal, or dismiss behavior.
- Spread `floatingStyles` onto the float's `style`; never hand-apply `top`/`left`/`transform`. Pass `open` in options so `isPositioned` resets when the float hides.
- Supply `whileElementsMounted: autoUpdate` for any persistent float; a one-shot `computePosition` is for measurement, not a mounted element.
- Order middleware `offset → flip → shift → arrow`/`size`; pass `deps` for reactive options and a `Derivable` for placement-dependent options — never a plain object option computed from state without `deps`.
- Use `Strategy` `'fixed'` for portaled/overflow-escaping floats; `'absolute'` within a positioned container.

[RAIL_LAW]:
- Package: `@floating-ui/react-dom`
- Owns: the React `useFloating` positioning hook, `floatingStyles`/`isPositioned`/`update`, the React-aware middleware factories (`offset`/`flip`/`shift`/`arrow`/`size`/`hide`/`inline`/`autoPlacement`/`limitShift`), and the re-exported `@floating-ui/dom` engine (`autoUpdate`/`computePosition`/`detectOverflow`/`getOverflowAncestors`/`platform`) and geometry vocabulary
- Accept: refs via `refs.setReference`/`refs.setFloating`, middleware via `UseFloatingOptions.middleware` in canonical order, `autoUpdate` as `whileElementsMounted`, `Derivable` + `deps` for reactive options
- Reject: manual `top`/`left`/`transform` application, a mounted float without `autoUpdate`, direct import when interaction/focus/dismiss is needed (use `@floating-ui/react`), a middleware option derived from state without `deps`, two positioners on one element
