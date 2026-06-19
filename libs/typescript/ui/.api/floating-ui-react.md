# [API_CATALOGUE] @floating-ui/react

`@floating-ui/react` extends `@floating-ui/react-dom` with the full interaction layer: open-state management, interaction hooks (`useClick`, `useHover`, `useFocus`, `useDismiss`, `useRole`, `useListNavigation`, `useTypeahead`), prop merger (`useInteractions`), transition utilities, focus management (`FloatingFocusManager`), portal (`FloatingPortal`), overlay (`FloatingOverlay`), arrow (`FloatingArrow`), tree context (`FloatingTree`/`FloatingNode`), delay grouping, and composite keyboard navigation. It is the primary floating-UI surface consumed by tooltip, popover, dropdown, combobox, and menu owners in the ui stack.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@floating-ui/react`
- package: `@floating-ui/react`
- module: `@floating-ui/react`
- namespace: named exports from `dist/floating-ui.react.d.ts`
- asset: React interaction layer, components, and hooks for floating elements
- rail: position + interaction

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: context and state family
- rail: interaction

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]     | [RAIL]                                                                    |
| :-----: | :------------------------ | :---------------- | :------------------------------------------------------------------------ |
|   [1]   | `FloatingContext<RT>`     | context object    | `UseFloatingReturn` extended with open/event state                        |
|   [2]   | `FloatingRootContext<RT>` | root context      | decoupled reference + floating context root                               |
|   [3]   | `ContextData`             | extra context bag | `openEvent`, `floatingContext`, arbitrary keys                            |
|   [4]   | `OpenChangeReason`        | reason union      | `'outside-press' \| 'escape-key' \| 'click' \| 'hover' \| 'focus' \| ...` |
|   [5]   | `ReferenceType`           | reference union   | `Element \| VirtualElement`                                               |
|   [6]   | `Delay`                   | delay type        | `number \| { open?: number; close?: number }`                             |
|   [7]   | `ElementProps`            | interaction props | `{ reference?; floating?; item? }` HTML props map                         |

[PUBLIC_TYPE_SCOPE]: refs and elements family
- rail: interaction

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [RAIL]                                            |
| :-----: | :--------------------- | :--------------- | :------------------------------------------------ |
|   [1]   | `ExtendedRefs<RT>`     | extended refs    | `reference`, `floating`, `domReference` + setters |
|   [2]   | `ExtendedElements<RT>` | element bag      | `reference`, `floating`, `domReference`           |
|   [3]   | `NarrowedElement<T>`   | element narrower | `T extends Element ? T : Element`                 |

[PUBLIC_TYPE_SCOPE]: floating UI hook options family
- rail: position + interaction

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]   | [RAIL]                                                                                                                                      |
| :-----: | :------------------------------ | :-------------- | :------------------------------------------------------------------------------------------------------------------------------------------ |
|   [1]   | `UseFloatingOptions<RT>`        | hook config     | extends `@floating-ui/react-dom` options + `rootContext`, `onOpenChange`, `nodeId`                                                          |
|   [2]   | `UseFloatingReturn<RT>`         | hook result     | extends react-dom return + `context: FloatingContext<RT>`                                                                                   |
|   [3]   | `UseFloatingRootContextOptions` | root config     | `open?`, `onOpenChange?`, `elements`                                                                                                        |
|   [4]   | `UseFloatingData`               | position data   | `Prettify<UseFloatingReturn>`                                                                                                               |
|   [5]   | `UseInteractionsReturn`         | merged prop fns | `getReferenceProps`, `getFloatingProps`, `getItemProps`                                                                                     |
|   [6]   | `UseClickProps`                 | hook config     | `enabled?`, `event?: 'click' \| 'mousedown'`, `toggle?`, `ignoreMouse?`, `keyboardHandlers?`                                                |
|   [7]   | `UseFocusProps`                 | hook config     | `enabled?`, `visibleOnly?`                                                                                                                  |
|   [8]   | `UseRoleProps`                  | hook config     | `enabled?`, `role?: 'dialog' \| 'alertdialog' \| 'tooltip' \| 'menu' \| 'listbox' \| 'grid' \| 'tree' \| 'select' \| 'label' \| 'combobox'` |
|   [9]   | `UseDismissProps`               | hook config     | `enabled?`, `escapeKey?`, `outsidePress?`, `referencePress?`, `ancestorScroll?`, `bubbles?`                                                 |

[PUBLIC_TYPE_SCOPE]: component props family
- rail: render

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]     | [RAIL]                                                                                                      |
| :-----: | :-------------------------- | :---------------- | :---------------------------------------------------------------------------------------------------------- |
|   [1]   | `FloatingArrowProps`        | arrow props       | extends `ComponentPropsWithRef<'svg'>` + `context`, `width`, `height`, `tipRadius`, `stroke`, `strokeWidth` |
|   [2]   | `FloatingFocusManagerProps` | focus mgr props   | `context`, `modal`, `order`, `initialFocus`, `returnFocus`, `guards`, `restoreFocus`                        |
|   [3]   | `FloatingPortalProps`       | portal props      | `id?`, `root?`, `preserveTabOrder?`                                                                         |
|   [4]   | `FloatingOverlayProps`      | overlay props     | `lockScroll?: boolean`                                                                                      |
|   [5]   | `FloatingDelayGroupProps`   | delay group props | `delay: Delay`, `timeoutMs?`                                                                                |
|   [6]   | `CompositeProps`            | composite props   | `orientation?`, `loop?`, `rtl?`, `cols?`, `activeIndex?`, `onNavigate?`                                     |
|   [7]   | `CompositeItemProps`        | item props        | `render?: RenderProp`                                                                                       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: core positioning hooks
- rail: position

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY]    | [RAIL]                                                                 |
| :-----: | :-------------------------------- | :---------------- | :--------------------------------------------------------------------- |
|   [1]   | `useFloating<RT>(options?)`       | position hook     | returns `UseFloatingReturn<RT>` with `context`                         |
|   [2]   | `useFloatingRootContext(options)` | root context hook | creates a `FloatingRootContext` for split reference/floating ownership |

[ENTRYPOINT_SCOPE]: interaction hooks
- rail: interaction

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]    | [RAIL]                                             |
| :-----: | :---------------------------------- | :---------------- | :------------------------------------------------- |
|   [1]   | `useInteractions(propsList?)`       | prop merger       | merges `ElementProps[]` into three prop-getter fns |
|   [2]   | `useClick(context, props?)`         | click handler     | toggles open on click/mousedown                    |
|   [3]   | `useHover(context, props?)`         | hover handler     | opens on mouseover, closes on mouseleave           |
|   [4]   | `useFocus(context, props?)`         | focus handler     | opens on `:focus` / `:focus-visible`               |
|   [5]   | `useDismiss(context, props?)`       | dismiss handler   | closes on Escape, outside press, ancestor scroll   |
|   [6]   | `useRole(context, props?)`          | ARIA role handler | adds `role` and `aria-*` props                     |
|   [7]   | `useClientPoint(context, props?)`   | cursor handler    | positions relative to mouse/pointer coordinates    |
|   [8]   | `useListNavigation(context, props)` | list nav hook     | arrow-key navigation with real or virtual focus    |
|   [9]   | `useTypeahead(context, props)`      | typeahead hook    | character-matching list navigation                 |

[ENTRYPOINT_SCOPE]: floating UI components
- rail: render

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]   | [RAIL]                                            |
| :-----: | :------------------------------ | :--------------- | :------------------------------------------------ |
|   [1]   | `FloatingArrow`                 | SVG component    | renders pointing arrow triangle                   |
|   [2]   | `FloatingFocusManager(props)`   | focus manager    | traps or guides focus inside floating element     |
|   [3]   | `FloatingPortal(props)`         | portal component | renders floating element outside app root         |
|   [4]   | `FloatingOverlay`               | overlay div      | dims content / blocks pointer events behind float |
|   [5]   | `FloatingTree(props)`           | tree provider    | enables nested floating element communication     |
|   [6]   | `FloatingNode(props)`           | tree node        | registers a node into `FloatingTree`              |
|   [7]   | `FloatingList(props)`           | list provider    | provides ordered element refs to list hooks       |
|   [8]   | `FloatingDelayGroup(props)`     | delay group      | shared hover delay across sibling floats          |
|   [9]   | `NextFloatingDelayGroup(props)` | delay group v2   | experimental delay group without extra re-renders |
|  [10]   | `Composite`                     | composite nav    | single-tab-stop arrow-key navigable container     |
|  [11]   | `CompositeItem`                 | composite item   | item inside a `Composite`                         |

[ENTRYPOINT_SCOPE]: tree and portal hooks
- rail: interaction

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY]   | [RAIL]                                           |
| :-----: | :----------------------------------- | :--------------- | :----------------------------------------------- |
|   [1]   | `useFloatingNodeId(customParentId?)` | node id hook     | registers node in `FloatingTree`, returns id     |
|   [2]   | `useFloatingParentNodeId()`          | parent id hook   | returns parent node id or `null`                 |
|   [3]   | `useFloatingTree<RT>()`              | tree hook        | returns `FloatingTreeType<RT>` or `null`         |
|   [4]   | `useFloatingPortalNode(props?)`      | portal node hook | creates or selects the portal container DOM node |

[ENTRYPOINT_SCOPE]: list and transition utilities
- rail: interaction

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]   | [RAIL]                                             |
| :-----: | :----------------------------------------- | :--------------- | :------------------------------------------------- |
|   [1]   | `useListItem(props?)`                      | list item hook   | registers item ref and index in `FloatingList`     |
|   [2]   | `useTransitionStatus(context, props?)`     | transition hook  | `{ isMounted: boolean; status: TransitionStatus }` |
|   [3]   | `useTransitionStyles<RT>(context, props?)` | transition hook  | `{ isMounted: boolean; styles: CSSProperties }`    |
|   [4]   | `useDelayGroup(context, options?)`         | delay group hook | participates in a `FloatingDelayGroup`             |
|   [5]   | `useNextDelayGroup(context, options?)`     | delay group hook | participates in `NextFloatingDelayGroup`           |
|   [6]   | `useMergeRefs<Instance>(refs)`             | ref merger       | combines multiple refs into one callback ref       |
|   [7]   | `useId()`                                  | id hook          | React 18 `useId` with earlier-version fallback     |
|   [8]   | `safePolygon(options?)`                    | safe-polygon fn  | returns `HandleClose` for safe hover polygon       |

## [4]-[IMPLEMENTATION_LAW]

[INTERACTION_TOPOLOGY]:
- `useFloating` returns `context: FloatingContext` which is the single value passed to all interaction hooks
- `useInteractions` merges an array of `ElementProps` objects; spread `getReferenceProps()` onto the reference element and `getFloatingProps()` onto the floating element
- `FloatingFocusManager` requires a `FloatingRootContext` (from `useFloatingRootContext`) rather than the plain `FloatingContext` when reference and floating are owned by different components
- `FloatingPortal` preserves tab order in the React tree when `preserveTabOrder` is `true` with non-modal `FloatingFocusManager`
- `FloatingTree` + `FloatingNode` + `useFloatingNodeId` are required only when nested floats need explicit parent-child communication (bubble dismiss, nested virtual list navigation)
- `TransitionStatus` runs through `"unmounted" → "initial" → "open"` on open and `"open" → "close" → "unmounted"` on close

[LOCAL_ADMISSION]:
- Pass every interaction hook result to `useInteractions`; do not spread `ElementProps` objects manually to avoid handler collisions.
- Supply `whileElementsMounted: autoUpdate` in `useFloating` options to keep position correct during scroll and resize.
- Use `useFloatingRootContext` when the reference element is in a different subtree from the floating element.

[RAIL_LAW]:
- Package: `@floating-ui/react`
- Owns: React interaction layer and full component set for floating elements
- Accept: `FloatingContext` from `useFloating` as the single thread through all interaction hooks
- Reject: manual event handlers on reference/floating elements when interaction hooks cover the same interaction
