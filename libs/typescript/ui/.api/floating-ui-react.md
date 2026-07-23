# [TS_UI_API_FLOATING_UI_REACT]

`@floating-ui/react` is the full floating-element surface: it re-exports the `@floating-ui/react-dom` positioning engine (and, through it, the whole `@floating-ui/dom` middleware set) and adds the interaction, focus, portal, and tree layers that turn raw geometry into a keyboard-navigable, dismissible, accessible overlay. Interaction and focus hooks (`useClick`/`useHover`/`useFocus`/`useDismiss`/`useRole`/`useListNavigation`/`useTypeahead`) bind to a `FloatingRootContext` — open-state, events, and elements — so they need no position data and `useFloatingRootContext` can drive them without a positioned float; `useFloating` extends that root with geometry into the fuller `FloatingContext` that `useTransitionStyles` and `FloatingArrow` consume. `useInteractions` merges the hooks' `ElementProps` into three prop-getters spread onto the reference, floating, and item elements — the composition law that prevents handler collisions. `FloatingFocusManager` traps/guides focus, `FloatingPortal` escapes overflow/stacking contexts, `FloatingTree`+`FloatingNode` coordinate nested overlays, and `useTransitionStyles` drives enter/exit motion. It is the geometry-and-behavior provider under the `view/compose` floating-anchor, sheet, command-palette, and combobox rows; react-aria owns the ARIA semantics those rows also compose, floating-ui owns placement and the interaction primitives react-aria leaves headless.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@floating-ui/react`
- package: `@floating-ui/react` (MIT)
- module format: ESM + UMD (`dist/floating-ui.react.esm.js`, `.d.ts` at `dist/floating-ui.react.d.ts`), `sideEffects: false`; subpaths `.` and `./utils` (focus-order, list/grid-navigation, and environment probes — `getNextTabbable`, `getGridNavigatedIndex`, `isMac`)
- runtime target: React DOM (browser); the interaction layer binds pointer/keyboard events and the DOM focus model
- peer: `react@>=catalog`, `react-dom@>=catalog`; deps `@floating-ui/react-dom@^catalog`, `@floating-ui/utils@^catalog`, `tabbable catalog` (focus-order enumeration for `FloatingFocusManager`)
- asset: React interaction + component library over the positioning engine; superset of `@floating-ui/react-dom`
- rail: position + interaction (the `view/compose` floating-anchor/sheet/palette rows; `view/primitive` overlay row)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: context and open-state family — the open-state thread through interaction hooks
- rail: interaction
- `OpenChangeReason` = `'click' \| 'hover' \| 'focus' \| 'focus-out' \| 'escape-key' \| 'outside-press' \| 'reference-press' \| 'ancestor-scroll' \| 'list-navigation' \| 'safe-polygon'`.

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]     | [CONSUMER]                                                               |
| :-----: | :--------------------------------------- | :---------------- | :----------------------------------------------------------------------- |
|  [01]   | `FloatingRootContext<RT>`                | root context      | open-state/events/elements; every interaction + focus hook takes it      |
|  [02]   | `FloatingContext<RT>`                    | context object    | root + position data; `useTransitionStyles`/`FloatingArrow` consume it   |
|  [03]   | `ContextData`                            | extra context bag | `openEvent`, `floatingContext`, interaction keys (typeahead, list index) |
|  [04]   | `OpenChangeReason`                       | reason union      | branch dismiss on the open/close cause (values in lead)                  |
|  [05]   | `ElementProps`                           | interaction props | `{ reference?, floating?, item? }`; merged by `useInteractions`          |
|  [06]   | `FloatingTreeType<RT>` / `ReferenceType` | supporting types  | tree events; `Element \| VirtualElement`                                 |
|  [07]   | `Delay` / `HandleClose`                  | supporting types  | `number \| { open?, close? }`; the `safePolygon` return                  |

[PUBLIC_TYPE_SCOPE]: refs, elements, and hook config/return family
- rail: position + interaction

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]      | [CONSUMER]                                                    |
| :-----: | :-------------------------------------------------- | :----------------- | :------------------------------------------------------------ |
|  [01]   | `ExtendedRefs<RT>` / `ExtendedElements<RT>`         | refs / elements    | `reference`/`floating`/`domReference` setters + nodes         |
|  [02]   | `NarrowedElement<T>`                                | element narrow     | `domReference` ≠ `reference` under virtual anchoring          |
|  [03]   | `UseFloatingOptions<RT>` / `UseFloatingReturn<RT>`  | config / result    | adds `rootContext`/`onOpenChange`/`nodeId`                    |
|  [04]   | `UseFloatingData`                                   | position snapshot  | `Prettify<UseFloatingReturn>` — the full `useFloating` result |
|  [05]   | `UseFloatingRootContextOptions`                     | root config        | `{ open?, onOpenChange?, elements }` binds open-state         |
|  [06]   | `UseInteractionsReturn`                             | merged getters     | `getReferenceProps`/`getFloatingProps`/`getItemProps`         |
|  [07]   | `UseClickProps` / `UseHoverProps` / `UseFocusProps` | interaction config | `event`/`toggle`; `delay`/`restMs`/`handleClose`              |
|  [08]   | `UseDismissProps` / `UseRoleProps`                  | interaction config | `escapeKey`/`outsidePress`/`ancestorScroll`; `role`           |
|  [09]   | `UseListNavigationProps` / `UseTypeaheadProps`      | collection config  | `listRef`/`activeIndex`/`virtual`/`loop`; typeahead match     |
|  [10]   | `UseClientPointProps` / `UseInnerOffsetProps`       | collection config  | cursor-follow; scrollable-inner offset                        |
|  [11]   | `UseTransitionStatusProps`                          | transition config  | `{ isMounted, status }` mount phases                          |
|  [12]   | `UseTransitionStylesProps`                          | transition config  | `{ isMounted, styles }` enter/exit motion                     |
|  [13]   | `TransitionStatus`                                  | transition status  | `unmounted → initial → open → close → unmounted`              |

[PUBLIC_TYPE_SCOPE]: component props and re-exported geometry
- rail: render + position

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]      | [CONSUMER]                                             |
| :-----: | :----------------------------------------------------- | :----------------- | :----------------------------------------------------- |
|  [01]   | `FloatingArrowProps`                                   | component props    | `context`/`width`/`height`/`tipRadius` — the arrow SVG |
|  [02]   | `FloatingFocusManagerProps`                            | component props    | `modal`/`order`/`initialFocus`/`returnFocus`/`guards`  |
|  [03]   | `FloatingPortalProps` / `FloatingOverlayProps`         | component props    | portal `root`/`preserveTabOrder`; overlay `lockScroll` |
|  [04]   | `FloatingListProps` / `FloatingDelayGroupProps`        | collection props   | ordered-ref list; shared hover delay                   |
|  [05]   | `NextFloatingDelayGroupProps`                          | group props        | the re-render-free next-gen delay group                |
|  [06]   | `CompositeProps` / `CompositeItemProps` / `InnerProps` | composite props    | roving nav `orientation`/`loop`/`activeIndex`          |
|  [07]   | `Placement` / `Side` / `Alignment`                     | placement geometry | re-exported; catalogued in `floating-ui-react-dom.md`  |
|  [08]   | `AlignedPlacement` / `Strategy`                        | placement geometry | re-exported; `absolute \| fixed`                       |
|  [09]   | `Middleware` / `MiddlewareState` / `MiddlewareData`    | middleware shape   | re-exported; the pipeline contract                     |
|  [10]   | `MiddlewareArguments` / `MiddlewareReturn`             | middleware shape   | re-exported; the `fn` args and return                  |
|  [11]   | `VirtualElement` / `ReferenceElement`                  | element geometry   | re-exported; `VirtualElement` anchors a cursor         |
|  [12]   | `FloatingElement`                                      | element geometry   | re-exported; the floating node type                    |
|  [13]   | `Coords` / `Rect` / `SideObject`                       | box geometry       | re-exported box types                                  |
|  [14]   | `Dimensions` / `Padding` / `Boundary`                  | box geometry       | re-exported box types                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: positioning + interaction hooks — `useFloating` → `context` → interaction hooks → `useInteractions`
- rail: position + interaction
- Every interaction hook takes `(context, props?)` where `context` is the `FloatingRootContext`; the fuller `FloatingContext` from `useFloating` satisfies it as a superset.

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY]   | [CONSUMER]                                                    |
| :-----: | :----------------------------------- | :--------------- | :------------------------------------------------------------ |
|  [01]   | `useFloating<RT>(options?)`          | position root    | returns `UseFloatingReturn` with `context`                    |
|  [02]   | `useFloatingRootContext(options)`    | position root    | splits ref/float ownership; binds external open-state         |
|  [03]   | `useInteractions(propsList)`         | prop merger      | merges `ElementProps[]` into three prop-getters               |
|  [04]   | `useClick` / `useHover` / `useFocus` | trigger hooks    | open+dismiss; `useHover` takes `safePolygon` as `handleClose` |
|  [05]   | `useDismiss` / `useRole`             | trigger hooks    | dismiss behavior; ARIA role wiring                            |
|  [06]   | `useListNavigation` / `useTypeahead` | collection hooks | arrow-key + character nav over a `listRef`                    |
|  [07]   | `useClientPoint` / `useInnerOffset`  | collection hooks | cursor anchoring; scrollable-inner offset                     |

[ENTRYPOINT_SCOPE]: components — focus, portal, overlay, tree, composite
- rail: render

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]  | [CONSUMER]                                                |
| :-----: | :-------------------------------------------- | :-------------- | :-------------------------------------------------------- |
|  [01]   | `FloatingFocusManager(props)`                 | focus           | trap/guide focus (modal or non-modal)                     |
|  [02]   | `FloatingPortal(props)` / `FloatingOverlay`   | portal/overlay  | render outside root; dim + scroll-lock                    |
|  [03]   | `FloatingArrow` / `FloatingList(props)`       | arrow / list    | arrow SVG on `arrow` data; `useListItem` provider         |
|  [04]   | `FloatingTree(props)` / `FloatingNode(props)` | nested overlays | nested menus: bubble dismiss + open-state                 |
|  [05]   | `FloatingDelayGroup(props)`                   | group           | shared tooltip hover-delay across siblings                |
|  [06]   | `NextFloatingDelayGroup(props)`               | group           | the re-render-free next-gen delay group                   |
|  [07]   | `Composite` / `CompositeItem`                 | roving nav      | single-tab-stop arrow-navigable container (toolbar, grid) |

[ENTRYPOINT_SCOPE]: tree, list, transition, and ref utility hooks
- rail: interaction

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CONSUMER]                                              |
| :-----: | :------------------------------------------------------------ | :------------- | :------------------------------------------------------ |
|  [01]   | `useFloatingNodeId(parentId?)`                                | tree ids       | register a node in `FloatingTree`                       |
|  [02]   | `useFloatingParentNodeId()`                                   | tree ids       | read the parent node id                                 |
|  [03]   | `useFloatingTree()` / `useFloatingPortalNode(props?)`         | tree/portal    | access the tree event bus; resolve the portal container |
|  [04]   | `useListItem(props?)`                                         | list item      | register a ref+index into `FloatingList`                |
|  [05]   | `useTransitionStatus(ctx, props?)`                            | transition     | `{ isMounted, status }` — the mount phase               |
|  [06]   | `useTransitionStyles<RT>(ctx, props?)`                        | transition     | `{ isMounted, styles }` — enter/exit motion styles      |
|  [07]   | `useDelayGroup(ctx, options?)`                                | delay group    | join a shared hover-delay group                         |
|  [08]   | `useNextDelayGroup(ctx, options?)`                            | delay group    | the `Next` group; avoids re-renders                     |
|  [09]   | `useDelayGroupContext()`                                      | delay group    | read the active delay-group context                     |
|  [10]   | `useMergeRefs<I>(refs)` / `useId()` / `safePolygon(options?)` | ref/id/polygon | merge refs; SSR-stable id; hover-intent safe-triangle   |

[ENTRYPOINT_SCOPE]: re-exported positioning engine — middleware + utilities from `@floating-ui/dom`
- rail: position

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY]     | [CONSUMER]                                                  |
| :-----: | :-------------------------------------------------- | :----------------- | :---------------------------------------------------------- |
|  [01]   | `offset` / `flip` / `shift` / `arrow` / `size`      | middleware factory | re-exported; catalogued in `floating-ui-react-dom.md`       |
|  [02]   | `hide` / `inline` / `autoPlacement` / `limitShift`  | middleware factory | re-exported; canonical `offset → flip → shift → arrow`      |
|  [03]   | `inner(props)` / `useInnerOffset`                   | inner middleware   | anchor a tall scrollable list (combobox) to the active item |
|  [04]   | `autoUpdate` / `computePosition` / `detectOverflow` | positioning util   | `autoUpdate` is the required `whileElementsMounted`         |
|  [05]   | `getOverflowAncestors` / `platform`                 | positioning util   | the imperative overflow probes + default platform           |

## [04]-[IMPLEMENTATION_LAW]

[INTERACTION_TOPOLOGY]:
- Interaction and focus hooks read the `FloatingRootContext` — open-state, events, and elements — so dismiss behavior, ARIA role, and focus trap wire from that root alone, and `useFloatingRootContext` drives them with no positioned float; `useFloating`'s `context` is that root extended with geometry, and it satisfies every hook as a superset. `useInteractions([useClick(context), useDismiss(context), useRole(context)])` merges the returned `ElementProps` into three prop-getters, and the getters chain every handler; spreading `getReferenceProps()`/`getFloatingProps()`/`getItemProps()` is the only correct way to apply interactions — hand-spreading the raw `ElementProps` drops the chaining and collides handlers.
- `useFloatingRootContext` decouples the reference from the floating element: when the trigger and the float live in different components (a toolbar button opening a portaled panel), the root context carries the shared open-state and both sides bind to it — this is also the seam where external open-state (an atom) drives `open`/`onOpenChange`.
- `FloatingFocusManager` requires a mounted floating element and manages the DOM focus order via `tabbable`; `modal` traps focus and adds an inert backdrop, non-modal guides focus while `preserveTabOrder` (with `FloatingPortal`) keeps the tab sequence matching the React tree despite the portal. `FloatingOverlay` adds scroll-lock and pointer-blocking behind a modal float.
- `FloatingTree` + `FloatingNode` + `useFloatingNodeId` are required only for nested floats that must communicate — a submenu bubbling an outside-press dismiss up to its parent menu, or nested virtual-list navigation. Flat overlays never need the tree.
- `TransitionStatus` sequences `unmounted → initial → open` on open and `open → close → unmounted` on close, and `useTransitionStyles` maps each phase to a style object — the mount lifecycle is a value the render folds, so enter/exit animation is declarative and the element unmounts only after the exit transition.

[STACKS_WITH]:
- `@floating-ui/react-dom` (`libs/typescript/ui/.api/floating-ui-react-dom.md`): the positioning engine underneath — this package re-exports its `useFloating`, every middleware factory, and the geometry types, then layers interaction/focus/portal/tree on top. A `view/compose` row imports `@floating-ui/react` and never `react-dom` directly unless it needs interaction-free anchoring.
- react-aria / react-stately (sibling rows `view/primitive`, `view/compose`, `libs/typescript/ui/.api/react-aria.md`): the reciprocal of react-aria's positioning split — react-aria's `useOverlayTrigger`/`usePopover`/`useModalOverlay` own overlay behavior (ARIA semantics, dismiss, focus trap, scroll lock, screen-reader), while `useFloating` + the `offset`/`flip`/`shift`/`size` middleware are the geometry that supplants react-aria's `useOverlayPosition` wherever live anchor-tracking (`autoUpdate`) or collision middleware is required — the react-aria positioner has no scroll-tracked flip/shift/clamp. Both meet at the overlay element: react-aria's aria/DOM prop bundle (`usePopover`) and floating-ui's `floatingStyles` position folded by react-aria's `mergeProps`, `useMergeRefs` bridging the two ref systems on that same node. floating-ui additionally owns the interaction primitives react-aria leaves out (`safePolygon` hover intent, `useListNavigation` virtual focus); a row picks react-aria's overlay hooks OR floating-ui's `useRole`+`useDismiss`+`FloatingFocusManager` as the semantic owner, never both, and always composes `useFloating` for position — the two positioners never both drive one element.
- `@effect-atom/atom-react` (sibling row, `libs/typescript/ui/.api/effect-atom-atom-react.md`): open-state is an atom — `useFloatingRootContext({ open: useAtomValue(openAtom), onOpenChange: useAtomSet(openAtom) })` binds the float's visibility to the store, so a popover's open state is undoable, URL-syncable, and shared across components like any other atom, never local `useState`.
- `cmdk` (sibling row, `libs/typescript/ui/.api/cmdk.md`, command palette): cmdk is a self-contained list+filter+keyboard machine, so floating-ui HOSTS it, never drives it — an anchored/combobox palette mounts a bare `Command` inside `useFloating` + `FloatingPortal` + `FloatingFocusManager` (non-modal), floating-ui owning position/portal/focus-return and cmdk owning the whole keyboard+filter. `useListNavigation`/`useTypeahead` are the ALTERNATIVE hand-built (non-cmdk) list — they drive a caller-owned `listRef`/`activeIndex` and `useTypeahead` targets menu buttons, not a searchable combobox input — so they are never layered over a cmdk list (the double-keyboard defect).
- `vaul` (sibling row, `libs/typescript/ui/.api/vaul.md`, drag sheet/drawer): the overlay-class division, not a composition — floating-ui owns ANCHORED overlays through its own portal/focus/overlay stack, while vaul owns the DRAG-DISMISSABLE SHEET and takes portal/focus-trap/scroll-lock from its bundled Radix Dialog. A `view/compose` row picks the class by interaction; the two overlay stacks never wrap one surface.
- `@tanstack/react-virtual` (sibling row, `libs/typescript/ui/.api/tanstack-react-virtual.md`): a virtualized combobox binds `useListNavigation` with `virtual: true` (aria-activedescendant focus stays on the input — off-screen windowed rows are unmounted and cannot take real DOM focus) and the virtualizer's single `activeIndex`/`onNavigate`, delegating row reveal to the virtualizer's `scrollToIndex` (native `scrollItemIntoView` is off — it only scrolls a mounted node). `inner`/`useInnerOffset` anchors the fixed-height scroll container at the reference while the virtualizer owns the inner `getTotalSize()` spacer and row offsets.
- `token/theme` + `act/transition` (sibling rows): `useTransitionStyles` consumes the motion tokens (tw-animate) for enter/exit, and the `act/transition` View-Transitions row wraps the mount/unmount that `TransitionStatus` gates.

[LOCAL_ADMISSION]:
- Thread the `context` from `useFloating` through every interaction hook and merge with `useInteractions`; never hand-spread `ElementProps` or attach raw event handlers where a hook covers the interaction — that drops handler chaining and ARIA wiring.
- Pass `whileElementsMounted: autoUpdate` in `useFloating` options so position tracks scroll, resize, and DOM mutation; a static float without it drifts.
- Use `useFloatingRootContext` when the reference and floating elements are in different subtrees, and bind external open-state (an atom) through its `open`/`onOpenChange` rather than mirroring state.
- Reach for `FloatingTree` only for nested-float communication; keep flat overlays tree-free. Use `FloatingFocusManager` `modal` for dialogs and non-modal (+`preserveTabOrder`) for menus/comboboxes.
- Decide the semantic owner once — react-aria overlay hooks OR floating-ui `useRole`/`useDismiss`/`FloatingFocusManager` — and when react-aria owns semantics let floating-ui own only geometry: `useFloating` + `offset`/`flip`/`shift`/`size` supplant `useOverlayPosition` for anchor-tracking/collision, `useMergeRefs` bridges the refs, and react-aria's `mergeProps` folds the aria bundle with `floatingStyles` at the overlay element.

[RAIL_LAW]:
- Package: `@floating-ui/react`
- Owns: the React interaction layer (`useClick`/`useHover`/`useFocus`/`useDismiss`/`useRole`/`useListNavigation`/`useTypeahead`/`useClientPoint`/`useInnerOffset`), `useInteractions` prop merging, focus (`FloatingFocusManager`), portal (`FloatingPortal`/`useFloatingPortalNode`), overlay, tree (`FloatingTree`/`FloatingNode`), composite roving nav, delay grouping, transition hooks, `safePolygon`, and the re-exported positioning engine
- Accept: `FloatingContext`/`FloatingRootContext` from `useFloating` as the single interaction thread, external open-state via `useFloatingRootContext`, `autoUpdate` as `whileElementsMounted`, `useFloating` + `offset`/`flip`/`shift`/`size` supplanting react-aria's `useOverlayPosition`, `useMergeRefs` to bridge ref systems and react-aria's `mergeProps` to fold the aria bundle with `floatingStyles` at the overlay element
- Reject: manual event handlers where an interaction hook exists, hand-spread `ElementProps` bypassing `useInteractions`, a float without `autoUpdate`, floating-ui and react-aria both owning overlay semantics on one element, local `useState` open-state where an atom is the store
