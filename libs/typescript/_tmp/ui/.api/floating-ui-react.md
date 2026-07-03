# [API_CATALOGUE] @floating-ui/react

`@floating-ui/react` is the full floating-element surface: it re-exports the `@floating-ui/react-dom` positioning engine (and through it the entire `@floating-ui/dom` middleware set + geometry vocabulary) and layers interaction, focus, portal, tree, composite, and transition primitives on top. `useFloating` returns a `context: FloatingContext` that threads through every interaction hook (`useClick`/`useHover`/`useFocus`/`useDismiss`/`useRole`/`useListNavigation`/`useTypeahead`/`useClientPoint`/`useInnerOffset`), and `useInteractions` merges their `ElementProps` into three prop-getters spread onto the reference, floating, and item elements — the composition law that prevents handler collisions and drives ARIA wiring. It is the placement-and-behavior provider under the `overlay/floating.md` `useFloatingAnchor` owner; react-aria owns the ARIA semantics an anchored surface also composes, floating-ui owns geometry and the interaction primitives react-aria leaves headless (`safePolygon` hover intent, virtual-focus list navigation).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@floating-ui/react`
- package: `@floating-ui/react`
- version: `0.27.19`
- license: `MIT`
- module: ESM + `.d.ts` at `dist/floating-ui.react.d.ts`; `sideEffects: false`; subpaths `.` and `./utils` (`dist/floating-ui.react.utils.d.ts` — focus/tabbable/tree/grid navigation helpers)
- peer: `react >=17`, `react-dom >=17` (the interaction layer binds pointer/keyboard events and the DOM focus model)
- deps: `@floating-ui/react-dom ^2.1.8` (`floating-ui-react-dom.md` — the positioning engine this re-exports), `@floating-ui/utils ^0.2.11`, `tabbable ^6` (focus-order enumeration for `FloatingFocusManager`)
- runtime: React DOM (browser); superset of `@floating-ui/react-dom`
- rail: position + interaction (the `overlay/floating.md` `useFloatingAnchor` owner over tooltip/popover/menu/dialog/select/combobox)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: context + open-state family — the single thread through interaction hooks
- rail: interaction
- `useFloating().context` is a `FloatingContext`; interaction hooks accept it as `FloatingRootContext` (a widening — `FloatingContext` is assignable), transition hooks require the full `FloatingContext`. One `context` value carries geometry, open-state, and event metadata to every hook.

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                                        |
| :-----: | :------------------------------------------------------ | :---------------- | :-------------------------------------------------------------------------- |
|  [01]   | `FloatingContext<RT>`                                   | context object    | `overlay/floating.md` — `Omit<UseFloatingReturn, 'refs'\|'elements'> & { open, onOpenChange, events, dataRef, nodeId, floatingId, refs, elements }`; the value every hook takes |
|  [02]   | `FloatingRootContext<RT>`                               | root context      | the reference/floating/open-state root; interaction-hook param type, produced by `useFloatingRootContext` for split ownership |
|  [03]   | `OpenChangeReason`                                      | reason union      | `'outside-press'\|'escape-key'\|'ancestor-scroll'\|'reference-press'\|'click'\|'hover'\|'focus'\|'focus-out'\|'list-navigation'\|'safe-polygon'` — branch dismiss/announce behavior on cause |
|  [04]   | `ElementProps` (`{ reference?, floating?, item? }`)     | interaction props | the per-element HTML-prop map each interaction hook returns; merged by `useInteractions` |
|  [05]   | `ContextData` / `FloatingEvents` / `HandleClose` / `HandleCloseContext` | context bag / bus | `openEvent` + arbitrary interaction keys; the `emit`/`on` tree event bus; the `safePolygon` hover-intent return and its context |
|  [06]   | `FloatingNodeType<RT>` / `FloatingTreeType<RT>` / `ReferenceType` / `Delay` | tree + supporting | node/tree shapes for nested overlays; `Element \| VirtualElement`; `number \| Partial<{ open, close }>` |

[PUBLIC_TYPE_SCOPE]: refs, elements, and hook config/return family
- rail: position + interaction

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                                        |
| :-----: | :------------------------------------------------------ | :---------------- | :-------------------------------------------------------------------------- |
|  [01]   | `ExtendedRefs<RT>` / `ExtendedElements<RT>` / `NarrowedElement<T>` | refs / elements | `setReference`/`setFloating`/`setPositionReference` setters + resolved nodes; `domReference` differs from `reference` under virtual anchoring |
|  [02]   | `UseFloatingOptions<RT>` / `UseFloatingReturn<RT>` / `UseFloatingData` | position config / result | extends react-dom options with `rootContext`/`onOpenChange`/`nodeId`/`open`; return adds `context`/`refs`/`elements`; `UseFloatingData = Prettify<UseFloatingReturn>` |
|  [03]   | `UseFloatingRootContextOptions` (`{ open?, onOpenChange?(open, event?, reason?), elements: { reference, floating } }`) | root config | `binding/atom.md` — bind external open-state (an atom) to the floating root; `onOpenChange` carries the `OpenChangeReason` |
|  [04]   | `UseInteractionsReturn` (`getReferenceProps`/`getFloatingProps`/`getItemProps`) | merged getters | spread onto the three element roles; each getter chains every merged handler |
|  [05]   | `UseClickProps` / `UseHoverProps` / `UseFocusProps` / `UseDismissProps` / `UseRoleProps` | interaction config | `event`/`toggle`; `delay`/`restMs`/`handleClose`; `visibleOnly`; `escapeKey`/`outsidePress`/`referencePress`/`ancestorScroll`/`bubbles`; `role` |
|  [06]   | `UseListNavigationProps` / `UseTypeaheadProps` / `UseClientPointProps` / `UseInnerOffsetProps` | collection config | `listRef`/`activeIndex`/`virtual`/`loop`/`orientation`/`cols`; character-match `onMatch`; cursor-follow `axis`; scrollable-inner `overflowRef`/`onChange` |
|  [07]   | `UseTransitionStatusProps` / `UseTransitionStylesProps` / `SafePolygonOptions` | transition + polygon | enter/exit motion (`duration`/`initial`/`open`/`close`/`common`); `buffer`/`blockPointerEvents`/`requireIntent` for `safePolygon` |

[PUBLIC_TYPE_SCOPE]: component props and re-exported positioning geometry
- rail: render + position

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                                        |
| :-----: | :------------------------------------------------------ | :---------------- | :-------------------------------------------------------------------------- |
|  [01]   | `FloatingArrowProps` / `FloatingFocusManagerProps` / `FloatingPortalProps` / `FloatingOverlayProps` | component props | arrow (`context`/`width`/`height`/`tipRadius`/`d`); focus (`modal`/`order`/`initialFocus`/`returnFocus`/`guards`/`restoreFocus`/`closeOnFocusOut`); portal (`root`/`id`/`preserveTabOrder`); overlay (`lockScroll`) |
|  [02]   | `FloatingDelayGroupProps` / `NextFloatingDelayGroupProps` / `CompositeProps` / `CompositeItemProps` / `InnerProps` | group/composite props | shared hover delay; the re-render-free `Next` group; roving nav (`orientation`/`loop`/`cols`/`activeIndex`/`onNavigate`); scrollable-inner middleware config |
|  [03]   | `Placement` / `Side` / `Alignment` / `AlignedPlacement` / `Strategy` | placement geometry | re-exported from react-dom (`floating-ui-react-dom.md`) — side+alignment vocabulary, `absolute \| fixed` |
|  [04]   | `Middleware` / `MiddlewareState` / `MiddlewareData` / `MiddlewareArguments` / `MiddlewareReturn` / `Derivable<T>` | middleware shape | re-exported; the pipeline contract each factory produces |
|  [05]   | `VirtualElement` / `ReferenceElement` / `FloatingElement` / `Coords` / `Rect` / `SideObject` / `Dimensions` / `Padding` / `Boundary` / `RootBoundary` / `ArrowOptions` | element/box geometry | re-exported; `VirtualElement` anchors a float to a cursor/selection with no DOM node |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: positioning + interaction hooks — `useFloating` → `context` → interaction hooks → `useInteractions`
- rail: position + interaction
- The composition topology: `useFloating(opts)` returns `context`; each interaction hook takes it and returns `ElementProps`; `useInteractions([...])` merges them into three chained prop-getters. Hand-spreading raw `ElementProps` drops chaining and collides handlers — the named defect.

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                         |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `useFloating<RT>(options?): UseFloatingReturn<RT>` / `useFloatingRootContext(options): FloatingRootContext` | position root  | `overlay/floating.md` — `useFloatingAnchor` calls `useFloating({ open, onOpenChange, placement, whileElementsMounted: autoUpdate, middleware })`; `useFloatingRootContext` splits reference/floating ownership + binds external open-state |
|  [02]   | `useInteractions(propsList?: Array<ElementProps \| void>): UseInteractionsReturn`                | prop merger    | `overlay/floating.md` — merges `[useClick, useFocus, useRole, useDismiss]` into `getReferenceProps`/`getFloatingProps`/`getItemProps`; the collision-free composition boundary |
|  [03]   | `useClick(ctx, props?)` / `useHover(ctx, props?)` / `useFocus(ctx, props?)` / `useDismiss(ctx, props?)` / `useRole(ctx, props?)` | trigger hooks  | open/dismiss/ARIA-role behavior keyed by `overlay/floating.md` `ROLE_OF[kind]`; `useHover` takes `safePolygon()` as `handleClose`; `useDismiss` closes on outside-press/escape/ancestor-scroll |
|  [04]   | `useListNavigation(ctx, props)` / `useTypeahead(ctx, props)` / `useClientPoint(ctx, props?)` / `useInnerOffset(ctx, props)` | collection hooks | combobox/menu/palette arrow-key + character nav (real or virtual focus); cursor anchoring; `useInnerOffset` positions a tall scrollable list at the active item |

[ENTRYPOINT_SCOPE]: components — focus, portal, overlay, tree, arrow, list, composite, delay
- rail: render

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                         |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `FloatingFocusManager(props)` / `FloatingPortal(props)` / `FloatingOverlay`                      | focus/portal/overlay | `overlay/floating.md` — trap/guide focus (modal or non-modal + `preserveTabOrder`), render outside the app root escaping overflow/stacking, dim + scroll-lock behind a modal |
|  [02]   | `FloatingArrow` / `FloatingList(props)`                                                          | arrow / list   | the pointing-arrow SVG bound to `arrow` middleware data (the `overlay/floating.md` `arrowRef`); the ordered-ref provider `useListItem` registers into |
|  [03]   | `FloatingTree(props)` / `FloatingNode(props)`                                                    | nested overlays | coordinate nested menus/submenus — bubble outside-press dismiss and parent-child open-state up the tree |
|  [04]   | `FloatingDelayGroup(props)` / `NextFloatingDelayGroup(props)` / `Composite` / `CompositeItem`    | group / roving nav | shared tooltip hover-delay across siblings; single-tab-stop arrow-navigable container (toolbar, grid) |

[ENTRYPOINT_SCOPE]: tree, list, transition, delay, and ref/id utility hooks
- rail: interaction

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                         |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `useFloatingNodeId(parentId?)` / `useFloatingParentNodeId()` / `useFloatingTree()` / `useFloatingPortalNode(props?)` | tree/portal ids | register a node in `FloatingTree`, read the parent id, access the tree event bus, resolve the portal container `HTMLElement` |
|  [02]   | `useListItem(props?): { ref, index }`                                                            | list item      | register an item's ref+index into `FloatingList` for `useListNavigation` |
|  [03]   | `useTransitionStatus(ctx, props?)` / `useTransitionStyles<RT>(ctx, props?)`                      | transition     | `interaction/transition.md` — `{ isMounted, status }` and `{ isMounted, styles }`; drive enter/exit motion (`status`: `unmounted → initial → open → close → unmounted`) |
|  [04]   | `useDelayGroup(ctx, options?)` / `useDelayGroupContext()` / `useNextDelayGroup(ctx, options?)`   | delay group    | participate in a shared hover-delay group; the `Next` variant avoids extra re-renders |
|  [05]   | `useMergeRefs<I>(refs)` / `useId()` / `safePolygon(options?): HandleClose`                       | ref/id/polygon | merge floating-ui + react-aria + local refs into one callback; SSR-stable id; the hover-intent safe-triangle `handleClose` |

[ENTRYPOINT_SCOPE]: re-exported positioning engine + `./utils` DOM helpers
- rail: position

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                         |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `offset` / `flip` / `shift` / `arrow` / `size` / `hide` / `inline` / `autoPlacement` / `limitShift` | middleware factory | re-exported; `overlay/floating.md` runs `[offset(8), flip(), shift({ padding: 8 }), arrow({ element: arrowRef })]`; catalogued in `floating-ui-react-dom.md` |
|  [02]   | `inner(props): Middleware` / `useInnerOffset`                                                    | inner middleware | anchor a tall scrollable list (combobox) to the active item, keeping it in view |
|  [03]   | `autoUpdate` / `computePosition` / `detectOverflow` / `getOverflowAncestors` / `platform`        | positioning util | `autoUpdate` is the required `whileElementsMounted` value; the rest are the imperative engine + overflow probes |
|  [04]   | `./utils`: `getNextTabbable` / `getPreviousTabbable` / `enableFocusInside` / `disableFocusInside` / `getTabbableOptions` | focus util | tab-order enumeration for custom focus managers over `tabbable` |
|  [05]   | `./utils`: `getNodeChildren` / `getNodeAncestors` / `getDeepestNode` / `getFloatingFocusElement` | tree traversal | walk the `FloatingTree` node graph for bubble-dismiss and nested open-state; resolve a float's focusable element |
|  [06]   | `./utils`: `getGridNavigatedIndex` / `createGridCellMap` / `getGridCellIndices` / `isDifferentGridRow` / `isMac` / `isSafari` / `isTypeableCombobox` / `isVirtualClick` | grid + env probe | 2D roving-nav math for `Composite` grids; platform/pointer probes list nav branches on |

## [04]-[IMPLEMENTATION_LAW]

[INTERACTION_TOPOLOGY]:
- `useFloating` returns a `context: FloatingContext` that is the single value passed to every interaction hook — geometry, open-state, and event metadata travel as one object, so a popover, its dismiss behavior, its ARIA role, and its focus trap all read the same source. `useInteractions([useClick(ctx), useDismiss(ctx), useRole(ctx)])` merges the returned `ElementProps` into three prop-getters, and the getters chain every handler; spreading `getReferenceProps()`/`getFloatingProps()`/`getItemProps()` is the only correct way to apply interactions.
- interaction hooks are typed against `FloatingRootContext`, transition hooks against the full `FloatingContext` — `useFloating().context` satisfies both, so the design threads one `context` everywhere. `useFloatingRootContext` decouples the reference from the floating element (a trigger in one component opening a portaled panel in another) and is the seam where external open-state (an atom) drives `open`/`onOpenChange`.
- `FloatingFocusManager` manages DOM focus order via `tabbable`; `modal` traps focus and adds an inert backdrop, non-modal guides focus while `preserveTabOrder` (with `FloatingPortal`) keeps the tab sequence matching the React tree despite the portal. `FloatingOverlay` adds scroll-lock and pointer-blocking behind a modal float.
- `FloatingTree` + `FloatingNode` + `useFloatingNodeId` are required only for nested floats that must communicate — a submenu bubbling an outside-press dismiss up to its parent menu. Flat overlays never need the tree.
- `TransitionStatus` sequences `unmounted → initial → open` on open and `open → close → unmounted` on close; `useTransitionStyles` maps each phase to a style object, so the element unmounts only after the exit transition — enter/exit motion is a value the render folds.

[STACKING_LAW]:
- `overlay/floating.md` `useFloatingAnchor` is the one placement owner: it calls `useFloating({ open, onOpenChange, placement, whileElementsMounted: autoUpdate, middleware: [offset, flip, shift, arrow] })` and merges `[useClick(ctx), useFocus(ctx), useRole(ctx, { role: ROLE_OF[kind] }), useDismiss(ctx, { outsidePress, escapeKey, ancestorScroll })]` through one `useInteractions`, keying the ARIA `role` off the `ROLE_OF` overlay-kind table; a hand-rolled `getBoundingClientRect` placement or a per-overlay outside-press handler is the named defect. `interaction/role.md` overlay/navigation `RoleBehavior` rows compose one `useFloatingAnchor` by reference.
- `floating-ui-react-dom.md` is the positioning engine underneath — this package re-exports its `useFloating`, every middleware factory, and the geometry types, then layers interaction/focus/portal/tree on top. A `overlay/floating.md` row imports `@floating-ui/react` and never `react-dom` directly unless it needs interaction-free anchoring; the CSS Anchor Positioning bridge maps the same `Placement` onto the native `anchor-name`/`position-anchor` layer where the browser admits it.
- `react-aria.md` / `react-stately.md` are a division of labor — react-aria's overlay hooks own ARIA semantics, dismiss, and screen-reader behavior, floating-ui owns placement geometry and the interaction primitives react-aria leaves out (`safePolygon` hover intent, `useListNavigation` virtual focus, `useTypeahead`). `useMergeRefs` bridges the two ref systems on the shared element; a row picks react-aria's overlay hooks OR floating-ui's `useRole`+`useDismiss`+`FloatingFocusManager` as the semantic owner, never both, and always composes `useFloating` for position.
- `binding/atom.md` `AtomBinding`: an anchored surface reads its `open` state and drives `onOpenChange` through the atom store (via `useFloatingRootContext` for decoupled trigger/float), so overlay visibility is undoable and URL-syncable like any other atom, never local `useState`.
- `interaction/transition.md`: `useTransitionStyles` consumes the `tw-animate-css` motion tokens (`tw-animate-css.md`, `theming/tokens.md`) for enter/exit, and the `<ViewTransition>`/`<Activity>` `SurfaceTransition` wraps the mount/unmount `TransitionStatus` gates. `interaction/command.md` composes `useListNavigation`+`useTypeahead` for the `cmdk` palette (`cmdk.md`) hosted by `FloatingPortal`; a `@tanstack/react-virtual` combobox binds `useListNavigation({ virtual: true })` with the virtualizer's active-index and `inner`/`useInnerOffset` for scroll.
- universal `libs/typescript/.api/effect.md`: open-state and dismiss-reason are `Schema`-decoded values; the anchored surface's lifecycle is an `Effect.acquireRelease` scope, and `OpenChangeReason` branches through `Match.value` at the dismiss seam.

[LOCAL_ADMISSION]:
- Thread the `context` from `useFloating` through every interaction hook and merge with `useInteractions`; never hand-spread `ElementProps` or attach raw event handlers where a hook covers the interaction.
- Pass `whileElementsMounted: autoUpdate` so position tracks scroll, resize, and DOM mutation; a static float without it drifts.
- Use `useFloatingRootContext` when the reference and floating elements are in different subtrees, and bind external open-state (an atom) through its `open`/`onOpenChange` rather than mirroring state.
- Reach for `FloatingTree` only for nested-float communication; keep flat overlays tree-free. Use `FloatingFocusManager` `modal` for dialogs, non-modal (+`preserveTabOrder`) for menus/comboboxes.
- Decide the semantic owner once — react-aria overlay hooks OR floating-ui `useRole`/`useDismiss`/`FloatingFocusManager` — and bridge refs with `useMergeRefs`.

[RAIL_LAW]:
- Package: `@floating-ui/react`
- Owns: the React interaction layer (`useClick`/`useHover`/`useFocus`/`useDismiss`/`useRole`/`useListNavigation`/`useTypeahead`/`useClientPoint`/`useInnerOffset`), `useInteractions` prop merging, focus (`FloatingFocusManager`), portal (`FloatingPortal`/`useFloatingPortalNode`), overlay, tree (`FloatingTree`/`FloatingNode`), composite roving nav, delay grouping, transition hooks, `safePolygon`, the `./utils` focus/tree/grid helpers, and the re-exported positioning engine
- Accept: `FloatingContext`/`FloatingRootContext` from `useFloating` as the single interaction thread, external open-state via `useFloatingRootContext`, `autoUpdate` as `whileElementsMounted`, `useMergeRefs` to bridge ref systems
- Reject: manual event handlers where an interaction hook exists, hand-spread `ElementProps` bypassing `useInteractions`, a float without `autoUpdate`, floating-ui and react-aria both owning overlay semantics on one element, local `useState` open-state where an atom is the store, a per-overlay `getBoundingClientRect` placement beside `useFloatingAnchor`
