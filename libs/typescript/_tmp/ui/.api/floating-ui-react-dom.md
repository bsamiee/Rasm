# [API_CATALOGUE] @floating-ui/react-dom

`@floating-ui/react-dom` is the React DOM positioning base of Floating UI: it wraps every `@floating-ui/dom` middleware factory with a React `deps` array, exposes `useFloating` for anchor-based positioning, and re-exports the full `@floating-ui/core`/`@floating-ui/dom` geometry and middleware-options vocabulary. It carries **no interaction layer** — open state, `useClick`/`useHover`/`useDismiss`, focus management, and portals live in `@floating-ui/react` (`floating-ui-react.md`), the superset that re-exports this package's positioner and middleware and layers behavior, focus, portal, and tree on top. The `overlay/floating.md` `useFloatingAnchor` owner imports from `@floating-ui/react`, consuming these positioning primitives (`useFloating`, `offset`/`flip`/`shift`/`arrow`, `autoUpdate`, `Placement`) transitively; this catalogue documents that positioning substrate.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@floating-ui/react-dom`
- package: `@floating-ui/react-dom`
- version: `2.1.8`
- license: `MIT`
- module: ESM + `.d.ts` at `dist/floating-ui.react-dom.d.ts`; `sideEffects: false`; UMD `main` + ESM `module`; single entry `.` (no subpaths, unlike `@floating-ui/react`'s `./utils`)
- peer: `react >=16.8.0`, `react-dom >=16.8.0` (the hook binds React refs, `floatingStyles`, and isomorphic layout effects)
- deps: `@floating-ui/dom ^1.7.6` (`MIT`; the framework-agnostic positioning engine — `computePosition`, `autoUpdate`, `detectOverflow`, every middleware, and the whole geometry vocabulary this package re-exports)
- runtime: React DOM (browser); the positioning base — `@floating-ui/react` (`floating-ui-react.md`) is the interaction superset over this
- rail: position (the positioning substrate under the `overlay/floating.md` `useFloatingAnchor` owner, consumed transitively via `@floating-ui/react`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: React positioning hook types (declared here, not re-exported)
- rail: position

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                                                |
| :-----: | :----------------------- | :---------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `UseFloatingOptions<RT>` | hook config       | `overlay/floating.md` — `Partial<ComputePositionConfig> & { whileElementsMounted?, elements?, open?, transform? }`; the `useFloatingAnchor` options bag (adds `open`/`placement`/`middleware`/`whileElementsMounted`) |
|  [02]   | `UseFloatingReturn<RT>`  | hook result       | `UseFloatingData & { update(), floatingStyles, refs, elements }` — `refs.setReference`/`refs.setFloating` bind the anchor/float in `overlay/floating.md` |
|  [03]   | `UseFloatingData`        | position snapshot | `Prettify<ComputePositionReturn & { isPositioned: boolean }>` (`= { x, y, placement, strategy, middlewareData, isPositioned }`); gate first paint on `isPositioned` |
|  [04]   | `ArrowOptions`           | arrow config      | `{ element: MutableRefObject<Element\|null> \| Element \| null; padding?: Padding }` — `overlay/floating.md` passes `{ element: arrowRef }` |
|  [05]   | `ReferenceType`          | reference union   | `Element \| VirtualElement` (declared here; the `RT` hook generic) — a `VirtualElement` anchors to a cursor/selection with no DOM node |

[PUBLIC_TYPE_SCOPE]: middleware-options and derivable family — one parameterized options-per-middleware pattern
- rail: position
- law: every middleware factory takes a single options bag; each bag is `T | Derivable<T>` so options may be a pure function of live `MiddlewareState`. `Derivable<T> = (state: MiddlewareState) => T` is the one parameterization — a factory does not gain a variant per reactive input, it gains a derivable options function.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]      | [CONSUMER / BOUNDARY]                                                       |
| :-----: | :---------------------- | :----------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Derivable<T>`          | derivable options  | `(state: MiddlewareState) => T` — options as a function of live state; the one parameterization |
|  [02]   | `DetectOverflowOptions` | overflow config    | `{ boundary?, rootBoundary?, elementContext?, altBoundary?, padding? }` — the shared spine of flip/shift/hide/size/autoPlacement |
|  [03]   | `OffsetOptions`         | offset config      | `number \| { mainAxis?, crossAxis?, alignmentAxis? } \| Derivable<…>`        |
|  [04]   | `FlipOptions`           | flip config        | `DetectOverflowOptions & { mainAxis?, crossAxis?, fallbackPlacements?, fallbackStrategy?, flipAlignment? }` |
|  [05]   | `ShiftOptions`          | shift config       | `DetectOverflowOptions & { mainAxis?, crossAxis?, limiter? }`               |
|  [06]   | `LimitShiftOptions`     | shift limiter      | `{ offset?, mainAxis?, crossAxis? }` — the `limitShift` parameter type; sourced from `@floating-ui/dom`, NOT a named `react-dom` re-export (only the `limitShift` factory ships here) |
|  [07]   | `AutoPlacementOptions`  | placement config   | `DetectOverflowOptions & { crossAxis?, alignment?, autoAlignment?, allowedPlacements? }` |
|  [08]   | `SizeOptions`           | size config        | `DetectOverflowOptions & { apply(args): void \| Promise<void> }`           |
|  [09]   | `HideOptions`           | hide config        | `DetectOverflowOptions & { strategy?: 'referenceHidden' \| 'escaped' }`     |
|  [10]   | `InlineOptions`         | inline config      | `{ x?, y?, padding? }` — cursor/segment hint for wrapped inline references  |
|  [11]   | `AutoUpdateOptions`     | auto-update config | `{ ancestorScroll?, ancestorResize?, elementResize?, layoutShift?, animationFrame? }` — the `whileElementsMounted: autoUpdate` config |

[PUBLIC_TYPE_SCOPE]: re-exported core geometry and middleware vocabulary (from `@floating-ui/dom`/`@floating-ui/core`)
- rail: position
- boundary: DOM overlay pixel-space geometry, disjoint by construction from the geographic/world-space geometry of `maplibre-gl.md` (`LngLat`/`MercatorCoordinate`/`LngLatBounds`) and `deck.gl-core.md` (`Viewport`/`Position`/`CoordinateSystem`); `@floating-ui/react` re-exports this same vocabulary one level up.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                        |
| :-----: | :---------------------------------------------- | :---------------- | :---------------------------------------------------------- |
|  [01]   | `Placement`                                     | placement union   | `Side` or `${Side}-${Alignment}` string literal            |
|  [02]   | `AlignedPlacement`                              | aligned subtype   | placements carrying an explicit alignment                  |
|  [03]   | `Side`, `Alignment`                             | axis unions       | `'top'\|'right'\|'bottom'\|'left'` / `'start'\|'end'`       |
|  [04]   | `Axis`, `Length`                                | axis primitives   | `'x'\|'y'` / `'width'\|'height'`                            |
|  [05]   | `Strategy`                                      | position mode     | `'absolute' \| 'fixed'`                                     |
|  [06]   | `Coords`, `Dimensions`, `Rect`, `SideObject`    | geometry records  | `{x,y}` / `{width,height}` / `{x,y,width,height}` / `{top,right,bottom,left}` |
|  [07]   | `ClientRectObject`, `ElementRects`              | rect records      | DOM-rect shape / `{ reference: Rect; floating: Rect }`     |
|  [08]   | `Boundary`, `RootBoundary`, `ElementContext`    | clipping config   | overflow clipping boundary + `'reference'\|'floating'` context |
|  [09]   | `Padding`, `NodeScroll`                         | scalar configs    | `number \| Partial<SideObject>` / scroll offsets           |
|  [10]   | `VirtualElement`, `ReferenceElement`, `FloatingElement`, `Elements` | element shapes | `{ getBoundingClientRect(); contextElement? }` and the resolved element bag |
|  [11]   | `Middleware`, `MiddlewareState`, `MiddlewareData`, `MiddlewareReturn`, `MiddlewareArguments` | middleware protocol | `{ name; options?; fn(state) => MiddlewareReturn }`, the state in, the keyed data out (`MiddlewareArguments` = `MiddlewareState`) |
|  [12]   | `ComputePositionConfig`, `ComputePositionReturn` | compute contract  | `{ placement?, strategy?, middleware?, platform? }` / `{ x, y, placement, strategy, middlewareData }` |
|  [13]   | `Platform`, `platform`                          | platform surface  | the DOM platform interface + the default DOM `platform` object |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: positioning hook
- rail: position

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :-------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `useFloating<RT>(options?)` | position hook  | `overlay/floating.md` — `useFloatingAnchor` calls `useFloating({ open, onOpenChange, placement, whileElementsMounted: autoUpdate, middleware })`; returns `refs.setReference`/`refs.setFloating`, `floatingStyles`, `update`, `x`/`y`/`placement`/`strategy`/`middlewareData`/`isPositioned` |

[ENTRYPOINT_SCOPE]: React-aware middleware factories — one signature `(options?, deps?) => Middleware`
- rail: position
- law: the nine share the shape `(options?: T | Derivable<T>, deps?: React.DependencyList) => Middleware`; the `deps` second argument is the only React addition over `@floating-ui/dom`. Pass `deps` when the options close over reactive state so a stale closure never freezes the option. Three break the uniform shape: `offset` takes `OffsetOptions` directly (the union already embeds the derivable/number/object forms), `arrow`'s options argument is required (not optional), and `limitShift` returns a limiter object (`{ fn, options }`), not a standalone `Middleware`.

| [INDEX] | [SURFACE]                        | [OPTION BAG]          | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------- | :-------------------- | :-------------------------------------------------------- |
|  [01]   | `offset(options?, deps?)`        | `OffsetOptions`       | `overlay/floating.md` — `offset(8)`; translates along the main/cross/alignment axis |
|  [02]   | `flip(options?, deps?)`          | `FlipOptions`         | `overlay/floating.md` — `flip()`; flips to the opposite/fallback placement on overflow |
|  [03]   | `shift(options?, deps?)`         | `ShiftOptions`        | `overlay/floating.md` — `shift({ padding: 8 })`; shifts along the axis to keep the float in view |
|  [04]   | `limitShift(options?, deps?)`    | `LimitShiftOptions`   | limiter for `shift`'s `limiter` option; returns `{ fn, options }`, not a standalone `Middleware` |
|  [05]   | `arrow(options, deps?)`          | `ArrowOptions`        | `overlay/floating.md` — `arrow({ element: arrowRef })`; centers an arrow element against a React ref (options required) |
|  [06]   | `autoPlacement(options?, deps?)` | `AutoPlacementOptions`| auto-selects the best placement (alternative to `flip`)   |
|  [07]   | `size(options?, deps?)`          | `SizeOptions`         | resizes the float to available space via `apply`          |
|  [08]   | `hide(options?, deps?)`          | `HideOptions`         | flags `referenceHidden`/`escaped` into `middlewareData`   |
|  [09]   | `inline(options?, deps?)`        | `InlineOptions`       | improves placement over wrapped/inline references         |

[ENTRYPOINT_SCOPE]: positioning utilities and DOM primitives
- rail: position

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY]  | [CONSUMER / BOUNDARY]                                       |
| :-----: | :------------------------------------------------------- | :-------------- | :---------------------------------------------------------- |
|  [01]   | `autoUpdate(reference, floating, update, options?)`      | auto-updater    | `overlay/floating.md` — the required `whileElementsMounted` value; re-runs `update` on scroll/resize/mutation/`animationFrame`; returns a cleanup fn |
|  [02]   | `computePosition(reference, floating, config?)`          | static query    | one-shot `Promise<ComputePositionReturn>` (the imperative form under `useFloating`) |
|  [03]   | `detectOverflow(state, options?)`                        | overflow probe  | `Promise<SideObject>` overflow amounts — the primitive `flip`/`shift`/`hide`/`size`/`autoPlacement` compose |
|  [04]   | `getOverflowAncestors(element, list?, traverseIframes?)` | DOM util        | scrollable-ancestor list feeding `autoUpdate`               |
|  [05]   | `platform`                                               | platform object | default DOM `Platform` used by `computePosition`            |

## [04]-[IMPLEMENTATION_LAW]

[POSITION_TOPOLOGY]:
- `useFloating` returns `refs.setReference`/`refs.setFloating` callback setters plus `floatingStyles: React.CSSProperties`; spread `floatingStyles` directly onto the floating element's `style` and never hand-apply `top`/`left`/`transform`. `transform: true` (default) positions via `transform`; `transform: false` uses layout `top`/`left`.
- The middleware pipeline is ordered: `offset` → `flip`/`autoPlacement` → `shift` → `arrow`/`size`/`hide`. Each middleware reads `MiddlewareState` and writes a keyed entry into `middlewareData` (e.g. `middlewareData.arrow.{x,y}`, `middlewareData.hide.referenceHidden`); read the arrow coordinates back from `middlewareData.arrow` to place the arrow element.
- `detectOverflow` is the shared spine: `flip`, `shift`, `hide`, `size`, and `autoPlacement` all call it, so their `DetectOverflowOptions` (`boundary`/`rootBoundary`/`padding`/`altBoundary`) are one vocabulary, not per-middleware knobs.
- `deps?: React.DependencyList` is the React seam: when a factory's options derive from reactive state, either pass a `Derivable` options function (recomputed each position pass) or list the state in `deps` (rebuilds the middleware). Omitting both freezes the first closure.
- `UseFloatingOptions.whileElementsMounted` receives `autoUpdate` to re-run positioning on DOM mutation/scroll/resize; it is called once both elements mount and its cleanup runs on unmount. `isPositioned` is `false` until the first computation completes — gate first paint on it to suppress flicker, and pass `open` so it resets when the float hides.

[STACKING_LAW]:
- This is the positioning base; `@floating-ui/react` (`floating-ui-react.md`) is the symmetric superset — it re-exports `useFloating`, every middleware factory, `autoUpdate`, and the geometry types, then layers interaction/focus/portal/tree on top. The `overlay/floating.md` `useFloatingAnchor` owner imports from `@floating-ui/react` and composes exactly these primitives — `useFloating({ whileElementsMounted: autoUpdate, middleware: [offset(8), flip(), shift({ padding: 8 }), arrow({ element: arrowRef })] })` with a `Placement` argument — so the ui stack consumes react-dom transitively, never directly.
- Open state is not owned here: `overlay/floating.md` reads/writes it through the `binding/atom.md` `AtomBinding` (`effect` universal tier) and drives `useFloating`'s `open`/the interaction `onOpenChange`; the `isPositioned` flag synchronizes to that `open`.
- The `deps` array and `Derivable` options are the integration point with the React 19 compiler (`babel-plugin-react-compiler.md`): middleware whose options bind atom-derived state pass those values in `deps` so the compiler's memoization and the middleware rebuild agree. The CSS Anchor Positioning bridge in `overlay/floating.md` maps `floatingStyles`/`placement` onto the native `anchor-name`/`position-anchor` layer where supported, falling back to this computed position otherwise.
- The re-exported geometry vocabulary (`Coords`/`Rect`/`Dimensions`/`SideObject`/`Padding`/`Boundary`) is DOM overlay pixel-space, disjoint by construction from the geographic/world-space geometry of `maplibre-gl.md` (`LngLat`/`Point`/`MercatorCoordinate`/`LngLatBounds`/`EdgeInsets`) and `deck.gl-core.md` (`Viewport`/`WebMercatorViewport`/`Position`/`CoordinateSystem`) — no catalog crosses the two, so an overlay anchored over a map reads `floatingStyles` in CSS pixels while the map layer stays in its projection. The two positioning stacks are symmetric and independent: react-dom is the base of the overlay-geometry stack (`floating-ui-react.md` its interaction superset) exactly as maplibre/deck are the base of the map-geometry stack.

[LOCAL_ADMISSION]:
- Spread `floatingStyles` onto the floating element; bind elements through `refs.setReference`/`refs.setFloating`, never a leaked free React ref.
- Pass `open` so `isPositioned` resets when the float hides; supply `whileElementsMounted: autoUpdate` to keep position correct during scroll/resize.
- Use `@floating-ui/react` (not this package directly) when interaction hooks (`useClick`/`useHover`/`useFocus`/`useDismiss`) or portals/focus management are needed — that is the `overlay/floating.md` import path.
- Compute options from state via a `Derivable` options function or a `deps` list; never inline reactive state into a factory without one.
- Import `LimitShiftOptions` from `@floating-ui/dom` (or `@floating-ui/react`) when typing a `shift` limiter — `react-dom` re-exports the `limitShift` factory, not the type name.

[RAIL_LAW]:
- Package: `@floating-ui/react-dom`
- Owns: the React DOM positioning hook and React-aware (`deps`-carrying) middleware wrappers over `@floating-ui/dom`
- Accept: `useFloating` for position; `refs.setReference`/`refs.setFloating` for element binding; the `(options?, deps?) => Middleware` factories via `UseFloatingOptions.middleware`; `autoUpdate` via `whileElementsMounted`
- Reject: manual `computePosition` calls when `useFloating` covers the case; importing this package directly for interaction/portal/focus behavior (that is `@floating-ui/react`, `floating-ui-react.md`); hand-applied `top`/`left`/`transform` in place of `floatingStyles`
