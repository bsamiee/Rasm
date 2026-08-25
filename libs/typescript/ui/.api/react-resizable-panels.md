# [TS_UI_API_REACT_RESIZABLE_PANELS]

`react-resizable-panels` owns the split-pane constraint solver: a `Group` holds N `Panel` children with per-panel min, max, collapse, and unit-mixed size constraints, and every drag, keypress, or programmatic resize solves the whole row at once so the sum stays exact. `Separator` ships the complete window-splitter pattern — the full `role="separator"` aria property set, arrow, Home, End, Enter, and F6 keys, and coarse-pointer hit-target widening.

Layout crosses the boundary as a plain `{ [panelId]: percentage }` map through `defaultLayout` and `onLayoutChanged`, so persistence, restore, and workspace tokens ride the estate's own storage rail.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the layout value model — the data crossing the persistence boundary

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :----------------------------------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `Layout` (`{ [panelId: string]: number }`)             | struct        | panel id to percentage of the group; the whole persisted value  |
|  [02]   | `LayoutChangedMeta` (`{ isUserInteraction: boolean }`) | struct        | discriminates a pointer or key resize from every other trigger  |
|  [03]   | `PanelSize` (`{ asPercentage, inPixels }`)             | struct        | both readings of one panel's size, reported together            |
|  [04]   | `Orientation` (`'horizontal' \| 'vertical'`)           | union         | the group's resize axis; binds `aria-orientation`               |
|  [05]   | `SizeUnit`                                             | union         | `px` `%` `em` `rem` `vh` `vw` — the units a size string carries |
|  [06]   | `LayoutStorage`                                        | interface     | `Pick<Storage, 'getItem' \| 'setItem'>` — a SYNCHRONOUS port    |

[PUBLIC_TYPE_SCOPE]: the component props and imperative handles

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                                                   |
| :-----: | :-------------------------------------- | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `GroupProps`                            | struct        | the solver's whole configuration surface over `HTMLAttributes<HTMLDivElement>` |
|  [02]   | `PanelProps`                            | struct        | per-panel constraints and callbacks; omits DOM `onResize`                      |
|  [03]   | `SeparatorProps`                        | struct        | the handle's own props; `role` and `tabIndex` stay library-owned               |
|  [04]   | `GroupImperativeHandle`                 | interface     | `getLayout()` and `setLayout(layout) -> Layout`                                |
|  [05]   | `PanelImperativeHandle`                 | interface     | `collapse()` `expand()` `getSize()` `isCollapsed()` `resize(size)`             |
|  [06]   | `OnGroupLayoutChange` / `OnPanelResize` | delegate      | the extracted callback types for a typed consumer signature                    |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the three components — every element must be a direct DOM child of its group

| [INDEX] | [SURFACE]                   | [SHAPE] | [CAPABILITY]                                                            |
| :-----: | :-------------------------- | :------ | :---------------------------------------------------------------------- |
|  [01]   | `Group(GroupProps)`         | ctor    | the constraint solver and flex container; renders `data-group`          |
|  [02]   | `Panel(PanelProps)`         | ctor    | one constrained region; renders `data-panel` with a nested styled child |
|  [03]   | `Separator(SeparatorProps)` | ctor    | the focusable splitter; renders `data-separator` and `role="separator"` |

- [01]-[GROUP]: `orientation` `defaultLayout` `onLayoutChange` `onLayoutChanged` `disabled` `disableCursor` `resizeTargetMinimumSize` `id` `elementRef` `groupRef`.
- [02]-[PANEL]: `defaultSize` `minSize` `maxSize` `collapsible` `collapsedSize` `groupResizeBehavior` `disabled` `onResize` `id` `elementRef` `panelRef`.
- [03]-[SEPARATOR]: `disabled` `disableDoubleClick` `id` `elementRef` `className` `style` `children`.
- `Panel`: `className` and `style` land on a NESTED `div`, never the `data-panel` root, so flex sizing survives any consumer rule.
- `Separator`: `flexGrow`, `flexShrink`, and `touchAction` stay library-owned; `Group` reserves `display`, `flex-direction`, `flex-wrap`, and `overflow`.

[ENTRYPOINT_SCOPE]: hooks — persistence and the typed ref pairs

| [INDEX] | [SURFACE]                                           | [SHAPE] | [CAPABILITY]                                                        |
| :-----: | :-------------------------------------------------- | :------ | :------------------------------------------------------------------ |
|  [01]   | `useDefaultLayout(options)`                         | static  | returns `{ defaultLayout, onLayoutChanged }` to spread on a `Group` |
|  [02]   | `useGroupRef() -> RefObject<GroupImperativeHandle>` | static  | the typed ref for `groupRef`                                        |
|  [03]   | `useGroupCallbackRef() -> [handle, setHandle]`      | static  | the callback-ref form when the handle is shared with another hook   |
|  [04]   | `usePanelRef() -> RefObject<PanelImperativeHandle>` | static  | the typed ref for `panelRef`                                        |
|  [05]   | `usePanelCallbackRef() -> [handle, setHandle]`      | static  | the callback-ref form for a shared panel handle                     |
|  [06]   | `isCoarsePointer() -> boolean`                      | static  | the cached `matchMedia('pointer:coarse')` reading                   |

- `useDefaultLayout`: takes `id`, `storage`, `panelIds`, and `onlySaveAfterUserInteractions`; `storage` defaults to `localStorage` and reads through `useSyncExternalStore`, so both port members return and commit synchronously.
- `useDefaultLayout`: `panelIds` keys a second layout under the conditionally-rendered panel set, so a collapsed-away panel restores its own arrangement.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One solve per interaction covers the whole group: a drag, key step, double-click reset, or `setLayout` call re-derives every panel's flex-grow against the full constraint set — per-panel `minSize`, `maxSize`, `collapsedSize`, `disabled`, and each panel's `groupResizeBehavior` — so the row always sums exactly and no panel escapes its bounds through an intermediate state.
- Sizes are unit-mixed by design and resolve against live pixel measurements: a bare number reads as pixels, a bare string as a percentage, and `px`/`%`/`em`/`rem`/`vh`/`vw` spell the rest — one group holds a pixel-pinned sidebar beside percentage-flexible content without the consumer converting anything.
- Group resize distributes by policy, not by rule: `groupResizeBehavior: 'preserve-pixel-size'` pins a panel through a window resize while `'preserve-relative-size'` scales with the group, and at least one panel in every group must carry the relative behavior for the remainder to land somewhere.
- `Separator` ships the whole window-splitter pattern: `role="separator"` with `aria-orientation`, `aria-valuenow`, `aria-valuemin`, `aria-valuemax`, `aria-controls`, and `aria-disabled` computed from the live layout, `tabIndex={0}` when enabled, arrow keys stepping 5% along the group's axis, `Home`/`End` driving to either extreme, `Enter` toggling a collapsible neighbor, and `F6` (with `Shift`) cycling focus across the group's separators.
- Hit targets widen by pointer class rather than by media query: `resizeTargetMinimumSize` carries a `{ coarse, fine }` pair the group resolves through `isCoarsePointer()`, and a panel edge is draggable even where no `Separator` is rendered.
- Persistence is a props pair, never internal state: `defaultLayout` seeds the group and `onLayoutChanged(layout, meta)` fires once per settled change, with `meta.isUserInteraction` true only for a released pointer drag or a resize keypress — every programmatic `setLayout`, constraint recompute, and initial mount reads false, so a restore never echoes back as a user edit.
- `onLayoutChange` fires continuously through a drag and `onLayoutChanged` only on settle; live-tracking chrome reads the former, storage the latter.
- `useDefaultLayout` owns its own storage and its port is SYNCHRONOUS — `getItem` feeds a `useSyncExternalStore` snapshot and `setItem` commits inline, so the hook admits `localStorage`, `sessionStorage`, and a synchronous in-memory shim alone.

[STACKING]:
- `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): an async or server-backed layout store rides the props pair rather than the hook — `useAtomValue` supplies `defaultLayout` from a workspace atom that suspends until hydrated, and `onLayoutChanged` dispatches through `useAtomSet`, branching on `meta.isUserInteraction` so a hydration write never round-trips to the server. `Atom.kvs` serves the local-only case with the same shape.
- `react-aria-components` (`.api/react-aria-components.md`): the panes host RAC surfaces and never wrap them — this package owns `role="separator"` and its key handling outright, so a RAC focus scope or pressable layered onto a `Separator` doubles the keyboard model; `F6` separator cycling composes beside RAC's own landmark navigation.
- `class-variance-authority` (`.api/class-variance-authority.md`): `cva()` selectors folded through the one `cn()` rail style off the library's own state attributes — `data-separator` resolves to `focus`, `active`, `disabled`, or the hover state — with `disableCursor` handing cursor styling to the token plane.
- `@tanstack/react-virtual` (`.api/tanstack-react-virtual.md`): a windowed list inside a panel reads its own scroll container, and the panel's `onResize` (`PanelSize.inPixels`) drives `virtualizer.measure()`, so a pane drag settles the window without a second `ResizeObserver`.
- `@xyflow/react` (`.api/xyflow-react.md`): a canvas hosted in a panel takes `width`/`height` from the panel rather than the window, and the pane's settled `onLayoutChanged` drives `fitView` from `useReactFlow`, so the graph reframes once per drag instead of once per frame.
- `@use-gesture/react` (`.api/use-gesture-react.md`): pane dragging stays library-owned — `Separator` binds pointer capture and `touch-action: none` itself, so a gesture hook on the handle double-applies every delta.

[LOCAL_ADMISSION]:
- Give every `Panel` and `Group` an explicit stable `id`; the `useId` fallback re-keys across renders and orphans a persisted layout.
- Persist through `defaultLayout` + `onLayoutChanged` and branch on `meta.isUserInteraction`; reach for `useDefaultLayout` only where a synchronous browser store is the whole persistence story.
- Keep `Panel` and `Separator` as direct DOM children of their `Group` — an intervening wrapper breaks the flex solve.
- Style through the `data-group`/`data-panel`/`data-separator` attributes and let `Panel` apply `className`/`style` to its nested child.
- Render a `Separator` between every adjacent panel pair; edge dragging alone leaves the split unreachable by keyboard.
