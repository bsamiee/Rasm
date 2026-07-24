# [TS_UI_API_FLOATING_UI_REACT]

`@floating-ui/react` owns the React behavior layer over the `@floating-ui/react-dom` positioning engine it re-exports: every interaction and focus hook binds a `FloatingRootContext` — open-state, events, elements — needing no geometry, and `useInteractions` folds each hook's `ElementProps` into three chaining prop-getters.

It owns placement and the interaction primitives react-aria leaves headless; react-aria owns the ARIA semantics, and one element never binds both positioners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@floating-ui/react`
- package: `@floating-ui/react` (MIT)
- module: ESM + UMD, `sideEffects: false`; subpaths `.` and `./utils` (focus-order and environment probes — `getNextTabbable`, `getGridNavigatedIndex`, `isMac`)
- runtime: React DOM browser — the interaction layer binds pointer/keyboard events and the DOM focus model
- depends: `@floating-ui/react-dom`, `@floating-ui/utils`, `tabbable` (focus-order enumeration for `FloatingFocusManager`); peers `react`, `react-dom`
- rail: position + interaction — the `view/compose` floating-anchor/sheet/palette rows and the `view/primitive` overlay row

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: context and open-state family — the open-state thread through interaction hooks
- `OpenChangeReason` = `'click' \| 'hover' \| 'focus' \| 'focus-out' \| 'escape-key' \| 'outside-press' \| 'reference-press' \| 'ancestor-scroll' \| 'list-navigation' \| 'safe-polygon'`.

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]     | [CAPABILITY]                                                             |
| :-----: | :--------------------------------------- | :---------------- | :----------------------------------------------------------------------- |
|  [01]   | `FloatingRootContext<RT>`                | root context      | open-state/events/elements; every interaction + focus hook takes it      |
|  [02]   | `FloatingContext<RT>`                    | context object    | root + position data; `useTransitionStyles`/`FloatingArrow` consume it   |
|  [03]   | `ContextData`                            | extra context bag | `openEvent`, `floatingContext`, interaction keys (typeahead, list index) |
|  [04]   | `OpenChangeReason`                       | reason union      | branch dismiss on the open/close cause (values in lead)                  |
|  [05]   | `ElementProps`                           | interaction props | `{ reference?, floating?, item? }`; merged by `useInteractions`          |
|  [06]   | `FloatingTreeType<RT>` / `ReferenceType` | supporting types  | tree events; `Element \| VirtualElement`                                 |
|  [07]   | `Delay` / `HandleClose`                  | supporting types  | `number \| { open?, close? }`; the `safePolygon` return                  |

[PUBLIC_TYPE_SCOPE]: refs, elements, and hook config/return family

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]      | [CAPABILITY]                                                  |
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

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]      | [CAPABILITY]                                           |
| :-----: | :----------------------------------------------------- | :----------------- | :----------------------------------------------------- |
|  [01]   | `FloatingArrowProps`                                   | component props    | `context`/`width`/`height`/`tipRadius` — the arrow SVG |
|  [02]   | `FloatingFocusManagerProps`                            | component props    | `modal`/`order`/`initialFocus`/`returnFocus`/`guards`  |
|  [03]   | `FloatingPortalProps` / `FloatingOverlayProps`         | component props    | portal `root`/`preserveTabOrder`; overlay `lockScroll` |
|  [04]   | `FloatingListProps` / `FloatingDelayGroupProps`        | collection props   | ordered-ref list; shared hover delay                   |
|  [05]   | `NextFloatingDelayGroupProps`                          | group props        | the re-render-free next-gen delay group                |
|  [06]   | `CompositeProps` / `CompositeItemProps` / `InnerProps` | composite props    | roving nav `orientation`/`loop`/`activeIndex`          |
|  [07]   | `Placement` / `Side` / `Alignment`                     | placement geometry | re-exported placement geometry                         |
|  [08]   | `AlignedPlacement` / `Strategy`                        | placement geometry | re-exported; `absolute \| fixed`                       |
|  [09]   | `Middleware` / `MiddlewareState` / `MiddlewareData`    | middleware shape   | re-exported; the pipeline contract                     |
|  [10]   | `MiddlewareArguments` / `MiddlewareReturn`             | middleware shape   | re-exported; the `fn` args and return                  |
|  [11]   | `VirtualElement` / `ReferenceElement`                  | element geometry   | re-exported; `VirtualElement` anchors a cursor         |
|  [12]   | `FloatingElement`                                      | element geometry   | re-exported; the floating node type                    |
|  [13]   | `Coords` / `Rect` / `SideObject`                       | box geometry       | re-exported box types                                  |
|  [14]   | `Dimensions` / `Padding` / `Boundary`                  | box geometry       | re-exported box types                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: positioning + interaction hooks — every hook takes `(context, props?)` where `context` is the `FloatingRootContext`, the fuller `FloatingContext` from `useFloating` satisfying it as a superset

| [INDEX] | [SURFACE]                            | [SHAPE] | [CAPABILITY]                                                  |
| :-----: | :----------------------------------- | :------ | :------------------------------------------------------------ |
|  [01]   | `useFloating<RT>(options?)`          | hook    | returns `UseFloatingReturn` with `context`                    |
|  [02]   | `useFloatingRootContext(options)`    | hook    | splits ref/float ownership; binds external open-state         |
|  [03]   | `useInteractions(propsList)`         | hook    | merges `ElementProps[]` into three prop-getters               |
|  [04]   | `useClick` / `useHover` / `useFocus` | hook    | open+dismiss; `useHover` takes `safePolygon` as `handleClose` |
|  [05]   | `useDismiss` / `useRole`             | hook    | dismiss behavior; ARIA role wiring                            |
|  [06]   | `useListNavigation` / `useTypeahead` | hook    | arrow-key + character nav over a `listRef`                    |
|  [07]   | `useClientPoint` / `useInnerOffset`  | hook    | cursor anchoring; scrollable-inner offset                     |

[ENTRYPOINT_SCOPE]: components — focus, portal, overlay, tree, composite

| [INDEX] | [SURFACE]                                     | [SHAPE]   | [CAPABILITY]                                              |
| :-----: | :-------------------------------------------- | :-------- | :-------------------------------------------------------- |
|  [01]   | `FloatingFocusManager(props)`                 | component | trap/guide focus (modal or non-modal)                     |
|  [02]   | `FloatingPortal(props)` / `FloatingOverlay`   | component | render outside root; dim + scroll-lock                    |
|  [03]   | `FloatingArrow` / `FloatingList(props)`       | component | arrow SVG on `arrow` data; `useListItem` provider         |
|  [04]   | `FloatingTree(props)` / `FloatingNode(props)` | component | nested menus: bubble dismiss + open-state                 |
|  [05]   | `FloatingDelayGroup(props)`                   | component | shared tooltip hover-delay across siblings                |
|  [06]   | `NextFloatingDelayGroup(props)`               | component | the re-render-free next-gen delay group                   |
|  [07]   | `Composite` / `CompositeItem`                 | component | single-tab-stop arrow-navigable container (toolbar, grid) |

[ENTRYPOINT_SCOPE]: tree, list, transition, and ref utility hooks

| [INDEX] | [SURFACE]                                                     | [SHAPE]        | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------------ | :------------- | :------------------------------------------------------ |
|  [01]   | `useFloatingNodeId(parentId?)`                                | hook           | register a node in `FloatingTree`                       |
|  [02]   | `useFloatingParentNodeId()`                                   | hook           | read the parent node id                                 |
|  [03]   | `useFloatingTree()` / `useFloatingPortalNode(props?)`         | hook           | access the tree event bus; resolve the portal container |
|  [04]   | `useListItem(props?)`                                         | hook           | register a ref+index into `FloatingList`                |
|  [05]   | `useTransitionStatus(ctx, props?)`                            | hook           | `{ isMounted, status }` — the mount phase               |
|  [06]   | `useTransitionStyles<RT>(ctx, props?)`                        | hook           | `{ isMounted, styles }` — enter/exit motion styles      |
|  [07]   | `useDelayGroup(ctx, options?)`                                | hook           | join a shared hover-delay group                         |
|  [08]   | `useNextDelayGroup(ctx, options?)`                            | hook           | the `Next` group; avoids re-renders                     |
|  [09]   | `useDelayGroupContext()`                                      | hook           | read the active delay-group context                     |
|  [10]   | `useMergeRefs<I>(refs)` / `useId()` / `safePolygon(options?)` | hook / factory | merge refs; SSR-stable id; hover-intent safe-triangle   |

[ENTRYPOINT_SCOPE]: re-exported positioning engine — middleware + utilities from `@floating-ui/dom`

| [INDEX] | [SURFACE]                                           | [SHAPE]           | [CAPABILITY]                                                |
| :-----: | :-------------------------------------------------- | :---------------- | :---------------------------------------------------------- |
|  [01]   | `offset` / `flip` / `shift` / `arrow` / `size`      | factory           | re-exported core middleware — gap, collision, arrow, clamp  |
|  [02]   | `hide` / `inline` / `autoPlacement` / `limitShift`  | factory           | re-exported; canonical `offset → flip → shift → arrow`      |
|  [03]   | `inner(props)` / `useInnerOffset`                   | factory / hook    | anchor a tall scrollable list (combobox) to the active item |
|  [04]   | `autoUpdate` / `computePosition` / `detectOverflow` | static            | `autoUpdate` is the required `whileElementsMounted`         |
|  [05]   | `getOverflowAncestors` / `platform`                 | static / property | the imperative overflow probes + default platform           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Interaction and focus hooks read the `FloatingRootContext` — open-state, events, elements — so dismiss, ARIA role, and focus trap wire from that root with no geometry; `useFloating`'s `context` is that root extended with position and satisfies every hook as a superset.
- `useInteractions([useClick(context), useDismiss(context), useRole(context)])` folds each hook's `ElementProps` into three chaining prop-getters; spreading `getReferenceProps`/`getFloatingProps`/`getItemProps` is the sole application, and a raw `ElementProps` spread drops the chaining and collides handlers.
- `useFloatingRootContext` decouples the reference from the float across separate subtrees and is the seam where external open-state drives `open`/`onOpenChange`.
- `FloatingFocusManager` needs a mounted float and orders focus through `tabbable`: `modal` traps behind an inert backdrop, non-modal guides while `preserveTabOrder` holds the tab sequence to the React tree through a `FloatingPortal`; `FloatingOverlay` adds scroll-lock and pointer-blocking.
- `FloatingTree` + `FloatingNode` + `useFloatingNodeId` bind only nested floats that communicate — a submenu bubbling an outside-press dismiss to its parent menu; flat overlays stay tree-free.
- `TransitionStatus` sequences `unmounted → initial → open → close → unmounted` and `useTransitionStyles` maps each phase to a style object, so enter/exit is a value the render folds and the element unmounts only after the exit transition.

[STACKING]:
- `@floating-ui/react-dom`(`.api/floating-ui-react-dom.md`): the positioning substrate re-exported whole — `useFloating`, every middleware factory, and the geometry types — with interaction/focus/portal/tree layered on top; a `view/compose` row imports `@floating-ui/react` and drops to `react-dom` only for interaction-free anchoring.
- react-aria(`.api/react-aria.md`): the reciprocal positioning split — react-aria's `useOverlayTrigger`/`usePopover`/`useModalOverlay` own overlay ARIA, dismiss, focus trap, and scroll lock, while `useFloating` + `offset`/`flip`/`shift`/`size` supplant `useOverlayPosition` wherever scroll-tracked flip/shift/clamp is required; they meet at the overlay node where react-aria's `mergeProps` folds its aria bundle with `floatingStyles` and `useMergeRefs` bridges the ref systems.
- `@effect-atom/atom-react`(`.api/effect-atom-atom-react.md`): open-state is an atom — `useFloatingRootContext({ open: useAtomValue(openAtom), onOpenChange: useAtomSet(openAtom) })` binds visibility to the store, undoable and URL-syncable, never local `useState`.
- `cmdk`(`.api/cmdk.md`): floating-ui HOSTS it, never drives it — an anchored palette mounts a bare `Command` inside `useFloating` + `FloatingPortal` + `FloatingFocusManager` (non-modal), floating-ui owning position/portal/focus-return and cmdk owning the keyboard and filter; `useListNavigation`/`useTypeahead` are the ALTERNATIVE hand-built list, never layered over a cmdk list (the double-keyboard defect).
- `vaul`(`.api/vaul.md`): the overlay-class division — floating-ui owns ANCHORED overlays through its own portal/focus/overlay stack, vaul owns the DRAG-DISMISSABLE SHEET through its bundled Radix Dialog; a `view/compose` row picks the class by interaction and the two stacks never wrap one surface.
- `@tanstack/react-virtual`(`.api/tanstack-react-virtual.md`): a virtualized combobox binds `useListNavigation` with `virtual: true` (aria-activedescendant focus stays on the input, since windowed rows unmount) and the virtualizer's `activeIndex`/`onNavigate`, delegating row reveal to `scrollToIndex`; `inner`/`useInnerOffset` anchors the fixed-height scroll container at the reference while the virtualizer owns the total-size spacer and row offsets.
- within-lib: a `view/compose` floating-anchor, sheet, palette, or combobox row threads one `useFloating` `context` through its interaction hooks and merges with `useInteractions`; `useTransitionStyles` consumes the `token/theme` motion tokens for enter/exit and the `act/transition` View-Transitions row wraps the mount/unmount that `TransitionStatus` gates.

[LOCAL_ADMISSION]:
- Thread the `context` from `useFloating` through every interaction hook and merge with `useInteractions`; a hand-spread `ElementProps` or a raw handler where a hook covers the interaction drops handler chaining and ARIA wiring.
- Supply `whileElementsMounted: autoUpdate` on every persistent float so position tracks scroll, resize, and DOM mutation.
- Bind external open-state through an atom via `useFloatingRootContext` when the reference and float live in different subtrees, never a mirrored `useState`.
- Use `FloatingFocusManager` `modal` for dialogs and non-modal + `preserveTabOrder` for menus and comboboxes; reach for `FloatingTree` only for communicating nested floats.
- Pick the semantic owner once — react-aria overlay hooks OR floating-ui `useRole`/`useDismiss`/`FloatingFocusManager` — and when react-aria owns semantics let floating-ui own only geometry.

[RAIL_LAW]:
- Package: `@floating-ui/react`
- Owns: the React interaction, focus, portal, overlay, tree, composite-nav, delay-group, and transition layers over the re-exported `@floating-ui/react-dom` positioning engine, with `useInteractions` prop-getter merging and `safePolygon` hover intent
- Accept: `FloatingContext`/`FloatingRootContext` as the single interaction thread, external open-state via `useFloatingRootContext`, `autoUpdate` as `whileElementsMounted`, `useFloating` + `offset`/`flip`/`shift`/`size` supplanting `useOverlayPosition`, `useMergeRefs` and react-aria's `mergeProps` folding the aria bundle with `floatingStyles` at the overlay node
- Reject: a manual handler where an interaction hook exists, a hand-spread `ElementProps` bypassing `useInteractions`, a float without `autoUpdate`, both positioners on one element, local `useState` open-state where an atom is the store
