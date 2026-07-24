# [TS_UI_API_FLOATING_UI_REACT_DOM]

`@floating-ui/react-dom` binds the `@floating-ui/dom` positioning engine to React: one `useFloating` hook projects live geometry into a spread-ready `floatingStyles`, and React-aware middleware factories take `Derivable` options with a `deps` array so reactive positioning recomputes without stale closures.

`view/compose` reaches this package directly only for interaction-free anchoring — a static positioned badge, a measured non-dismissible label; anything dismissible, focus-trapped, or portaled composes `@floating-ui/react`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@floating-ui/react-dom`
- package: `@floating-ui/react-dom` (MIT)
- module: ESM + UMD, single `.` entry, `sideEffects: false`
- runtime: React DOM browser — binds real DOM nodes and emits inline `position`/`transform` styles
- depends: `@floating-ui/dom` (the framework-agnostic engine wrapped here)
- rail: position — the geometry layer under the `view/compose` floating-anchor and sheet rows

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: position result family — the hook config and return

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [CAPABILITY]                                                                            |
| :-----: | :----------------------- | :---------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `UseFloatingOptions<RT>` | hook config       | `Partial<ComputePositionConfig>` + `whileElementsMounted`/`open`/`elements`/`transform` |
|  [02]   | `UseFloatingReturn<RT>`  | hook result       | `refs`, `elements`, `floatingStyles`, `update`, `UseFloatingData`; no `context`         |
|  [03]   | `UseFloatingData`        | position snapshot | `ComputePositionReturn & { isPositioned }` — `x`/`y`/`placement`/`strategy`             |
|  [04]   | `ReferenceType`          | reference union   | `Element \| VirtualElement` — real node or virtual rect (cursor/selection)              |
|  [05]   | `ArrowOptions`           | arrow config      | `{ element, padding? }`; `element` targets the `arrow` ref (React ref or node)          |

[PUBLIC_TYPE_SCOPE]: re-exported geometry — the `@floating-ui/dom` vocabulary

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]       | [CAPABILITY]                                                 |
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
- `useFloating<RT>(options?)` returns `{ refs, elements, floatingStyles, update, placement, middlewareData, isPositioned }`; interaction `context` is added only by `@floating-ui/react`

| [INDEX] | [SURFACE]                   | [SHAPE] | [CAPABILITY]                                                       |
| :-----: | :-------------------------- | :------ | :----------------------------------------------------------------- |
|  [01]   | `useFloating<RT>(options?)` | hook    | spread `floatingStyles`; `isPositioned` guards first-paint flicker |

[ENTRYPOINT_SCOPE]: React-aware middleware factories — `(options, deps?)`, options `Derivable` except `offset`

| [INDEX] | [SURFACE]                   | [SHAPE] | [CAPABILITY]                                                                   |
| :-----: | :-------------------------- | :------ | :----------------------------------------------------------------------------- |
|  [01]   | `offset` / `flip` / `shift` | factory | canonical `offset → flip → shift`: gap, flip cross-axis, slide on overflow     |
|  [02]   | `arrow` / `size`            | factory | center the arrow on the reference; clamp to `availableWidth`/`availableHeight` |
|  [03]   | `autoPlacement` / `hide`    | factory | auto-pick the best side; hide when the reference is clipped                    |
|  [04]   | `inline` / `limitShift`     | factory | anchor wrapped-text references; bound `shift` displacement                     |

[ENTRYPOINT_SCOPE]: re-exported imperative engine and DOM utilities

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `autoUpdate(reference, floating, update, options?)`      | static   | the required `whileElementsMounted` value        |
|  [02]   | `computePosition(reference, floating, config)`           | static   | one-shot position query outside the hook         |
|  [03]   | `detectOverflow(state, options?)`                        | static   | one-shot overflow query for measurement          |
|  [04]   | `getOverflowAncestors(element, list?, traverseIframes?)` | static   | scrollable-ancestor set `autoUpdate` listens on  |
|  [05]   | `platform`                                               | property | the default DOM platform `computePosition` binds |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `useFloating` owns `position`/`top`/`left`/`transform` inside `floatingStyles`; a float applies position by spreading `floatingStyles` onto its `style`, and `isPositioned` stays `false` until the first calculation resolves so the float renders hidden until placed.
- Middleware runs as an ordered pipeline — `offset` before `flip` before `shift` before `arrow`/`size` — each stage consuming the prior stage's coordinates; `MiddlewareData` keys each stage's output by name (`middlewareData.arrow.x`, `middlewareData.hide.referenceHidden`).
- `Derivable` options and the `deps` array are the React-reactivity seam: an option that depends on live placement is a `Derivable`, an option derived from reactive state carries `deps`; a plain object option computed from state without `deps` reads a stale closure.
- `whileElementsMounted: autoUpdate` re-runs positioning on scroll, resize, DOM mutation, and layout shift and returns a cleanup the hook calls on unmount; a mounted float without it positions once and drifts.
- `Strategy` `'fixed'` escapes `overflow`/`transform`/`contain` clipping ancestors; `'absolute'` positions within the nearest positioned ancestor.

[STACKING]:
- `@floating-ui/react`(`.api/floating-ui-react.md`): the superset re-exporting this whole surface and adding interaction/focus/portal/tree; a `view/compose` row composes it and drops to `@floating-ui/react-dom` only for interaction-free anchoring where `useClick`/`useDismiss`/`FloatingFocusManager` are dead weight.
- within-lib: `view/compose` spreads `floatingStyles` onto the float `style` beside the design-token classes — floating-ui owns only `position`/`top`/`left`/`transform`, the token layer owns the visual box; a row also composing react-aria gives react-aria ARIA and dismiss and this engine placement, so one positioner drives each element.

[LOCAL_ADMISSION]:
- `view/compose` composes `@floating-ui/react`, importing `@floating-ui/react-dom` directly only when the float needs no interaction, focus, portal, or dismiss.
- Every persistent float supplies `whileElementsMounted: autoUpdate`; a one-shot `computePosition` serves measurement alone, and `open` in options resets `isPositioned` when the float hides.

[RAIL_LAW]:
- Package: `@floating-ui/react-dom`
- Owns: the React `useFloating` positioning hook with `floatingStyles`/`isPositioned`/`update`, the React-aware middleware factories, and the re-exported `@floating-ui/dom` engine and geometry vocabulary
- Accept: refs via `refs.setReference`/`refs.setFloating`, middleware in canonical order through `UseFloatingOptions.middleware`, `autoUpdate` as `whileElementsMounted`, `Derivable` + `deps` for reactive options
- Reject: hand-applied `top`/`left`/`transform`, a mounted float without `autoUpdate`, a direct import where interaction/focus/dismiss is needed, a middleware option derived from state without `deps`, two positioners on one element
