# [TS_UI_API_FLOATING_UI_REACT]

`@floating-ui/react` is the full floating-element surface: it re-exports the `@floating-ui/react-dom` positioning engine (and, through it, the whole `@floating-ui/dom` middleware set) and adds the interaction, focus, portal, and tree layers that turn raw geometry into a keyboard-navigable, dismissible, accessible overlay. `useFloating` returns a `FloatingContext` that threads through every interaction hook (`useClick`/`useHover`/`useFocus`/`useDismiss`/`useRole`/`useListNavigation`/`useTypeahead`), and `useInteractions` merges their `ElementProps` into three prop-getters spread onto the reference, floating, and item elements — the composition law that prevents handler collisions. `FloatingFocusManager` traps/guides focus, `FloatingPortal` escapes overflow/stacking contexts, `FloatingTree`+`FloatingNode` coordinate nested overlays, and `useTransitionStyles` drives enter/exit motion. It is the geometry-and-behavior provider under the `view/compose` floating-anchor, sheet, command-palette, and combobox rows; react-aria owns the ARIA semantics those rows also compose, floating-ui owns placement and the interaction primitives react-aria leaves headless.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@floating-ui/react`
- package: `@floating-ui/react` (MIT, © Floating UI contributors)
- module format: ESM + UMD (`dist/floating-ui.react.esm.js`, `.d.ts` at `dist/floating-ui.react.d.ts`), `sideEffects: false`; subpaths `.` and `./utils` (SSR/measurement helpers)
- runtime target: React DOM (browser); the interaction layer binds pointer/keyboard events and the DOM focus model
- peer: `react@>=catalog`, `react-dom@>=catalog`; deps `@floating-ui/react-dom@^catalog`, `@floating-ui/utils@^catalog`, `tabbable catalog` (focus-order enumeration for `FloatingFocusManager`)
- asset: React interaction + component library over the positioning engine; superset of `@floating-ui/react-dom`
- rail: position + interaction (the `view/compose` floating-anchor/sheet/palette rows; `view/primitive` overlay row)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: context and open-state family — the single thread through interaction hooks
- rail: interaction

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER] |
|:-----: |:------------------------------------------------------ |:--------------- |:--------------------------------------------------------------- |
| [01] | `FloatingContext<RT>` | context object | `view/compose` — `UseFloatingReturn` extended with open/event state; the value every interaction hook takes |
| [02] | `FloatingRootContext<RT>` | root context | `view/compose` — decoupled reference/floating root when the trigger and float are owned by different components |
| [03] | `ContextData` | extra context bag | `openEvent`, `floatingContext`, and arbitrary interaction keys (typeahead buffer, list index) |
| [04] | `OpenChangeReason` | reason union | `view/compose` — `'click' \| 'hover' \| 'focus' \| 'focus-out' \| 'escape-key' \| 'outside-press' \| 'reference-press' \| 'ancestor-scroll' \| 'list-navigation' \| 'safe-polygon'`; branch dismiss behavior on cause |
| [05] | `ElementProps` (`{ reference?, floating?, item? }`) | interaction props | the per-element HTML-prop map an interaction hook returns; merged by `useInteractions` |
| [06] | `FloatingTreeType<RT>` / `ReferenceType` / `Delay` / `HandleClose` | supporting types | tree events; `Element \| VirtualElement`; `number \| { open?, close? }`; the `safePolygon` return |

[PUBLIC_TYPE_SCOPE]: refs, elements, and hook config/return family
- rail: position + interaction

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER] |
|:-----: |:------------------------------------------------------ |:--------------- |:--------------------------------------------------------------- |
| [01] | `ExtendedRefs<RT>` / `ExtendedElements<RT>` / `NarrowedElement<T>` | refs / elements | `reference`/`floating`/`domReference` setters + resolved nodes; `domReference` differs from `reference` under virtual anchoring |
| [02] | `UseFloatingOptions<RT>` / `UseFloatingReturn<RT>` / `UseFloatingData` | position config / result | extends the react-dom options with `rootContext`/`onOpenChange`/`nodeId`; return adds `context: FloatingContext` |
| [03] | `UseFloatingRootContextOptions` (`{ open?, onOpenChange?, elements }`) | root config | `view/compose` — bind external open-state (an atom) to the floating root |
| [04] | `UseInteractionsReturn` (`getReferenceProps`/`getFloatingProps`/`getItemProps`) | merged getters | spread onto the three element roles; each getter chains all merged handlers |
| [05] | `UseClickProps` / `UseHoverProps` / `UseFocusProps` / `UseDismissProps` / `UseRoleProps` | interaction config | per-hook options: `event`/`toggle`; `delay`/`restMs`/`handleClose`; `visibleOnly`; `escapeKey`/`outsidePress`/`ancestorScroll`/`bubbles`; `role` |
| [06] | `UseListNavigationProps` / `UseTypeaheadProps` / `UseClientPointProps` / `UseInnerOffsetProps` | collection config | list arrow-nav (`listRef`/`activeIndex`/`virtual`/`loop`/`orientation`); character match; cursor-follow; scrollable-inner offset |
| [07] | `UseTransitionStatusProps` / `UseTransitionStylesProps` / `TransitionStatus` | transition config | enter/exit motion; `status` runs `unmounted → initial → open → close → unmounted` |

[PUBLIC_TYPE_SCOPE]: component props and re-exported geometry
- rail: render + position

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER] |
|:-----: |:------------------------------------------------------ |:--------------- |:--------------------------------------------------------------- |
| [01] | `FloatingArrowProps` / `FloatingFocusManagerProps` / `FloatingPortalProps` / `FloatingOverlayProps` | component props | arrow geometry (`context`/`width`/`height`/`tipRadius`); focus (`modal`/`order`/`initialFocus`/`returnFocus`/`guards`); portal (`root`/`preserveTabOrder`); overlay (`lockScroll`) |
| [02] | `FloatingListProps` / `FloatingDelayGroupProps` / `NextFloatingDelayGroupProps` | collection/group props | ordered-ref list provider; shared hover delay across siblings; the re-render-free next-gen delay group |
| [03] | `CompositeProps` / `CompositeItemProps` / `InnerProps` | composite props | single-tab-stop roving nav (`orientation`/`loop`/`cols`/`activeIndex`); scrollable-inner middleware config |
| [04] | `Placement` / `Side` / `Alignment` / `AlignedPlacement` / `Strategy` | placement geometry | re-exported from `@floating-ui/react-dom`; catalogued there — side+alignment vocabulary, `absolute \| fixed` |
| [05] | `Middleware` / `MiddlewareState` / `MiddlewareData` / `MiddlewareArguments` / `MiddlewareReturn` | middleware shape | re-exported; the pipeline contract each factory produces |
| [06] | `VirtualElement` / `ReferenceElement` / `FloatingElement` / `Coords` / `Rect` / `SideObject` / `Dimensions` / `Padding` / `Boundary` | element/box geometry | re-exported; `VirtualElement` anchors a float to a cursor/selection with no DOM node |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: positioning + interaction hooks — `useFloating` → `context` → interaction hooks → `useInteractions`
- rail: position + interaction

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:---------------------------------------------------------------------------------------------- |:------------- |:----------------------------------------------------------- |
| [01] | `useFloating<RT>(options?)` / `useFloatingRootContext(options)` | position root | `view/compose` — returns `UseFloatingReturn` with `context`; `useFloatingRootContext` splits reference/floating ownership and binds external open-state |
| [02] | `useInteractions(propsList)` | prop merger | `view/compose` — merges `ElementProps[]` into `getReferenceProps`/`getFloatingProps`/`getItemProps`; the collision-free composition boundary |
| [03] | `useClick(ctx, props?)` / `useHover(ctx, props?)` / `useFocus(ctx, props?)` / `useDismiss(ctx, props?)` / `useRole(ctx, props?)` | trigger hooks | popover/tooltip/menu open+dismiss+ARIA-role behavior; `useHover` takes `safePolygon` as `handleClose` |
| [04] | `useListNavigation(ctx, props)` / `useTypeahead(ctx, props)` / `useClientPoint(ctx, props?)` / `useInnerOffset(ctx, props)` | collection hooks | `view/compose` — combobox/menu/palette arrow-key + character nav; cursor anchoring; `useInnerOffset` positions a tall scrollable list at the active item |

[ENTRYPOINT_SCOPE]: components — focus, portal, overlay, tree, composite
- rail: render

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:---------------------------------------------------------------------------------------------- |:------------- |:----------------------------------------------------------- |
| [01] | `FloatingFocusManager(props)` / `FloatingPortal(props)` / `FloatingOverlay` | focus/portal/overlay | `view/compose` — trap/guide focus (modal or non-modal), render outside the app root escaping overflow, dim + scroll-lock behind a modal |
| [02] | `FloatingArrow` / `FloatingList(props)` | arrow / list | the pointing-arrow SVG bound to `arrow` middleware data; the ordered-ref provider `useListItem` registers into |
| [03] | `FloatingTree(props)` / `FloatingNode(props)` | nested overlays | `view/compose` — coordinate nested menus/submenus: bubble dismiss, parent-child open-state |
| [04] | `FloatingDelayGroup(props)` / `NextFloatingDelayGroup(props)` / `Composite` / `CompositeItem` | group / roving nav | shared tooltip hover-delay across siblings; single-tab-stop arrow-navigable container (toolbar, grid) |

[ENTRYPOINT_SCOPE]: tree, list, transition, and ref utility hooks
- rail: interaction

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:---------------------------------------------------------------------------------------------- |:------------- |:----------------------------------------------------------- |
| [01] | `useFloatingNodeId(parentId?)` / `useFloatingParentNodeId()` / `useFloatingTree()` / `useFloatingPortalNode(props?)` | tree/portal ids | register a node in `FloatingTree`, read the parent id, access the tree event bus, resolve the portal container |
| [02] | `useListItem(props?)` | list item | `view/compose` — register an item's ref+index into `FloatingList` for `useListNavigation` |
| [03] | `useTransitionStatus(ctx, props?)` / `useTransitionStyles<RT>(ctx, props?)` | transition | `view/compose` — `{ isMounted, status }` and `{ isMounted, styles }`; drive enter/exit motion tokens |
| [04] | `useDelayGroup(ctx, options?)` / `useDelayGroupContext()` / `useNextDelayGroup(ctx, options?)` | delay group | participate in a shared hover-delay group; the `Next` variant avoids extra re-renders |
| [05] | `useMergeRefs<I>(refs)` / `useId()` / `safePolygon(options?)` | ref/id/polygon | merge floating-ui + react-aria + local refs into one callback; SSR-stable id; the hover-intent safe-triangle `HandleClose` |

[ENTRYPOINT_SCOPE]: re-exported positioning engine — middleware + utilities from `@floating-ui/dom`
- rail: position

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:---------------------------------------------------------------------------------------------- |:------------- |:----------------------------------------------------------- |
| [01] | `offset` / `flip` / `shift` / `arrow` / `size` / `hide` / `inline` / `autoPlacement` / `limitShift` | middleware factory | re-exported; the canonical pipeline is `offset → flip → shift → arrow` (or `size`); catalogued in `floating-ui-react-dom.md` |
| [02] | `inner(props)` / `useInnerOffset` | inner middleware | `view/compose` — anchor a tall scrollable list (combobox) to the active item, keeping it in view |
| [03] | `autoUpdate` / `computePosition` / `detectOverflow` / `getOverflowAncestors` / `platform` | positioning util | `autoUpdate` is the required `whileElementsMounted` value; the rest are the imperative engine + overflow probes |

## [04]-[IMPLEMENTATION_LAW]

[INTERACTION_TOPOLOGY]:
- `useFloating` returns a `context: FloatingContext` that is the single value passed to every interaction hook — geometry, open-state, and event metadata travel as one object, so a popover, its dismiss behavior, its ARIA role, and its focus trap all read the same source. `useInteractions([useClick(ctx), useDismiss(ctx), useRole(ctx)])` merges the returned `ElementProps` into three prop-getters, and the getters chain every handler; spreading `getReferenceProps()`/`getFloatingProps()`/`getItemProps()` is the only correct way to apply interactions — hand-spreading the raw `ElementProps` drops the chaining and collides handlers.
- `useFloatingRootContext` decouples the reference from the floating element: when the trigger and the float live in different components (a toolbar button opening a portaled panel), the root context carries the shared open-state and both sides bind to it — this is also the seam where external open-state (an atom) drives `open`/`onOpenChange`.
- `FloatingFocusManager` requires a mounted floating element and manages the DOM focus order via `tabbable`; `modal` traps focus and adds an inert backdrop, non-modal guides focus while `preserveTabOrder` (with `FloatingPortal`) keeps the tab sequence matching the React tree despite the portal. `FloatingOverlay` adds scroll-lock and pointer-blocking behind a modal float.
- `FloatingTree` + `FloatingNode` + `useFloatingNodeId` are required only for nested floats that must communicate — a submenu bubbling an outside-press dismiss up to its parent menu, or nested virtual-list navigation. Flat overlays never need the tree.
- `TransitionStatus` sequences `unmounted → initial → open` on open and `open → close → unmounted` on close, and `useTransitionStyles` maps each phase to a style object — the mount lifecycle is a value the render folds, so enter/exit animation is declarative and the element unmounts only after the exit transition.

[STACKS_WITH]:
- `@floating-ui/react-dom` (`libs/typescript/ui/.api/floating-ui-react-dom.md`): the positioning engine underneath — this package re-exports its `useFloating`, every middleware factory, and the geometry types, then layers interaction/focus/portal/tree on top. A `view/compose` row imports `@floating-ui/react` and never `react-dom` directly unless it needs interaction-free anchoring.
- react-aria / react-stately (sibling rows `view/primitive`, `view/compose`, `libs/typescript/ui/.api/react-aria.md`): the reciprocal of react-aria's positioning split — react-aria's `useOverlayTrigger`/`usePopover`/`useModalOverlay` own overlay behavior (ARIA semantics, dismiss, focus trap, scroll lock, screen-reader), while `useFloating` + the `offset`/`flip`/`shift`/`size` middleware are the geometry that supplants react-aria's `useOverlayPosition` wherever live anchor-tracking (`autoUpdate`) or collision middleware is required — the react-aria positioner has no scroll-tracked flip/shift/clamp. The two meet at the overlay element: react-aria's aria/DOM prop bundle (`usePopover`) and floating-ui's `floatingStyles` position folded by react-aria's `mergeProps`, `useMergeRefs` bridging the two ref systems on that same node. floating-ui additionally owns the interaction primitives react-aria leaves out (`safePolygon` hover intent, `useListNavigation` virtual focus); a row picks react-aria's overlay hooks OR floating-ui's `useRole`+`useDismiss`+`FloatingFocusManager` as the semantic owner, never both, and always composes `useFloating` for position — the two positioners never both drive one element.
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
