# [TS_UI_API_REACT_ARIA_COMPONENTS]

[PACKAGE_SURFACE]:
- package: `react-aria-components` · version `` · license `Apache-2.0`
- module: dual — `dist/exports/index.mjs` (ESM) + `dist/exports/index.cjs` (CJS); per-component subpaths (`react-aria-components/Button`, …) via `exports["./*"]`; `./i18n` + `./i18n/*` locale bundles. `sideEffects: ["*.css"]` — the headless core is pure; only the optional bundled stylesheet side-effects. `client-only` import ⇒ client-tier (RSC boundary).
- asset: `dist/types/exports/index.d.ts` + ~80 per-component `.d.ts` (`restore: restored`, 79 declaration assets).
- runtime: React 19 render-time; internalizes `react-aria@catalog` (behavior/ARIA hooks) + `react-stately@catalog` (collection/selection/form state) + `@internationalized/date catalog` (calendar/date values) + `@react-types/shared` (the shared vocab). Peer react/react-dom 19.
- plane: `plane:runtime` (W4 `ui`); folder-local to `ui`, the headless spine every `view` row composes.
- rail: `ui/view` — the accessible component spine.
- role: `view/primitive.md` (the component spine + toast/live-region) and `view/compose.md` (Schema→aria FormBinding, picker, table/virtual, floating-anchor/sheet rows).

`react-aria-components` is the headless component spine — ~65 fully-accessible, unstyled components that own keyboard, focus, ARIA, i18n, and interaction, leaving all styling to the consumer. The load-bearing fact for a dense design: it is NOT 65 bespoke APIs but ONE parameterized pattern instantiated 65 times. Every component is `AriaHook(react-aria) ∘ StateHook(react-stately) ∘ RenderProps<state> ∘ ContextValue injection ∘ SlotProps`, and ships a uniform triple — `Xxx` (the element), `XxxContext` (prop injection for compound composition), `XxxStateContext` (the react-stately state exposed to descendants). Styling is state-driven: `className`/`style`/`children` each accept a function of the component's render state, surfaced as `data-*` selectors. The component roster is SEED DATA on the render-props + context + slot mechanism; a new component is a new instance of the pattern, and the folder composes the pattern — not a per-component memorized API. The mechanism owners are documented first because they, not the component list, are what a design reasons against.

## [01]-[THE_ONE_PATTERN]

Every component styles through `RenderProps`, injects props through a `ContextValue`, and names its slot through `SlotProps`. These owners are the mechanism; the roster in [02] varies only the state type `T`. The value exports here (`composeRenderProps`, `Provider`, `useRenderProps`, `useContextProps`, `useSlottedContext`, `DEFAULT_SLOT`) and type exports (`RenderProps`, `StyleRenderProps`, `ContextValue`, `SlotProps`) are the entire compositional substrate.

```ts contract
// className/style/children each accept a plain value OR a function of the component's render state (the data-* selectors). `render` overrides the DOM element.
type ClassNameOrFunction<T> = string | ((s: T & { defaultClassName: string | undefined }) => string)
interface StyleRenderProps<T, E extends keyof JSX.IntrinsicElements = 'div'> {
  className?: ClassNameOrFunction<T>
  style?: CSSProperties | ((s: T & { defaultStyle: CSSProperties }) => CSSProperties | undefined)
  render?: (props: JSX.IntrinsicElements[E], renderProps: T) => ReactElement   // DOM-override: custom element / router link (aria-spine counterpart to radix asChild)
}
interface RenderProps<T, E = 'div'> extends StyleRenderProps<T, E> {
  children?: ReactNode | ((s: T & { defaultChildren: ReactNode | undefined }) => ReactNode)
}

// composeRenderProps layers a styled wrapper's class/children over the user's render prop; useRenderProps is the hook every component runs internally.
declare function composeRenderProps<T, U, V extends T>(value: T | ((r: U) => V), wrap: (prev: T, r: U) => V): (r: U) => V
declare function useRenderProps<T, E>(opts: RenderPropsHookOptions<T, E>): { className?: string; style?: CSSProperties; children?: ReactNode; 'data-rac': string; render?: (p: JSX.IntrinsicElements[E], r: T) => ReactElement }

// Context injection: a parent supplies props/state to a slotted child; SlotProps names the slot; Provider collapses up to 12 contexts into ONE values array (the React-context analogue of a Layer merge).
type ContextValue<T, E> = SlottedContextValue<T & { ref?: ForwardedRef<E> }>   // SlottedContextValue<T> = { slots: Record<string|symbol, T> } | T | null | undefined
interface SlotProps { slot?: string | null }   // null ⇒ local props fully override the injected context
declare const DEFAULT_SLOT: unique symbol
declare function useContextProps<T, U extends SlotProps, E extends Element>(props: T & SlotProps, ref: ForwardedRef<E> | undefined, ctx: Context<ContextValue<U, E>>): [T, RefObject<E | null>]
declare function useSlottedContext<T>(ctx: Context<SlottedContextValue<T>>, slot?: string | null): T | null | undefined
declare function Provider<A /* …up to L */>(props: { values: readonly [Context<A>, A][]; children: ReactNode }): JSX.Element
```

Consumer note: styling rarely needs the function form — the `data-*` selectors ([`tailwindcss-react-aria-components`]) express state as Tailwind variants; reach for the `className`/`children` function only for state a variant cannot express. `render` is the element override for the aria spine (radix `asChild` serves the non-aria plane). `Provider` replaces nested `<XContext.Provider>` towers with one `values` array.

## [02]-[COMPONENT_FAMILIES]

The roster — each row a family of the `Xxx` / `XxxContext` / `XxxStateContext` triple. Every `XxxProps extends Aria<Xxx>Props, RenderProps<XxxRenderProps>, SlotProps` plus a shared DOM-attributes base; each `XxxRenderProps` exposes the boolean/data state (`isHovered`, `isSelected`, `isDisabled`, `isPending`, `isOpen`, …) as `data-*` selectors. This is SEED DATA on [01], not a distinct API per row.

| [INDEX] | [FAMILY]    | [COMPONENTS]                                                                                           | [STATE_AXIS]                                                           |
| :-----: | :---------- | :----------------------------------------------------------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | actions     | `Button` `ToggleButton` `ToggleButtonGroup` `Link` `FileTrigger`                                       | press/toggle/upload; `ButtonRenderProps.isPending`/`isDisabled`        |
|  [02]   | collections | `ListBox` `GridList` `Menu` `Table` `Tree` `TagGroup` `Tabs` `Breadcrumbs` `Toolbar`                   | react-stately collection state; selection/sort/drag on the [03] engine |
|  [03]   | pickers     | `Select` `ComboBox` `Autocomplete`                                                                     | collection + overlay + `useFilter` locale matching                     |
|  [04]   | overlays    | `DialogTrigger` `Dialog` `Modal` `ModalOverlay` `Popover` `Tooltip`(`Trigger`) `OverlayArrow`          | focus-trap + dismiss + positioning (`Placement`)                       |
|  [05]   | fields      | `Form` `FieldError` `Label` `Input` `TextField` `TextArea` `SearchField` `NumberField`                 | `validationBehavior` + `ValidationResult`                              |
|  [06]   | toggles     | `Checkbox`(`Group`) `RadioGroup` `Switch` `Slider` `Meter` `ProgressBar`                               | react-stately toggle/slider/number state                               |
|  [07]   | date/time   | `Calendar` `RangeCalendar` `DateField` `TimeField` `DatePicker` `DateRangePicker`                      | `@internationalized/date` (`DateValue`/`TimeValue`/`DateRange`)        |
|  [08]   | color       | `ColorPicker` `ColorArea` `ColorField` `ColorSlider` `ColorWheel` `ColorSwatch`(`Picker`) `ColorThumb` | `parseColor`/`getColorChannels`; `Color`/`ColorSpace`/`ColorChannel`   |
|  [09]   | structure   | `Group` `Separator` `Heading` `Header` `Text` `Keyboard` `Disclosure`(`Group`)                         | labeling/structure primitives                                          |
|  [10]   | interaction | `Pressable` `Focusable` `VisuallyHidden`                                                               | raw press/focus/SR-only on the aria spine                              |

Consumer note: each component also exports `XxxContext` (inject props via `Provider`) and, where stateful, `XxxStateContext` (read the react-stately state — `ListStateContext`, `TableStateContext`, `OverlayTriggerStateContext`, `SelectStateContext`, `RootMenuTriggerStateContext`, `TooltipTriggerStateContext`, `TabListStateContext`, …). Compound composition reads the state context rather than prop-drilling.

## [03]-[COLLECTION_ENGINE]

Collections, selection, sorting, virtualization, drag-drop, and async data are ONE engine the collection/picker families share — react-stately state surfaced through RAC. A custom item is authored through the factory pair, never hand-parsed `children`.

```ts contract
// Custom collection items: two factories, not bespoke children parsing. Collection/Section/CollectionBuilder are the render substrate.
declare function createLeafComponent<T, P>(type: string, render: (props: P, ref, item) => ReactElement): (props: P) => ReactElement   // a selectable item
declare function createBranchComponent<T, P>(type: string, render, useChildren?): (props: P) => ReactElement                          // an item owning child items (Tree/Section)
declare const CollectionBuilder: FC; declare const Collection: <T>(props: CollectionProps<T>) => ReactNode; declare const Section: FC

// Virtualization: a Virtualizer wraps a collection with a Layout; the layout roster is seed data on one Layout shape + geometry primitives.
declare const Virtualizer: <T>(props: { layout: Layout } & CollectionProps<T>) => ReactNode
type Layout = ListLayout | GridLayout | WaterfallLayout | TableLayout          // + LayoutInfo / Size / Rect / Point geometry
declare const ResizableTableContainer, ColumnResizer                          // column resize; TableLayout owns table virtual geometry
declare function useTableOptions(): { selectionMode: SelectionMode; selectionBehavior: SelectionBehavior | null; allowsDragging: boolean }  // read inside a custom TableHeader

// Selection/sort vocabulary (@react-types/shared) — the closed axes every collection reads.
type Key; type Selection; type SelectionMode = 'none' | 'single' | 'multiple'; type SortDescriptor; type SortDirection

// Drag-drop is one hook returning the DragAndDropHooks a collection consumes; the item guards discriminate the drop payload.
declare function useDragAndDrop(options: DragAndDropOptions): DragAndDropHooks   // + useDrag/useDrop/DropZone/DropIndicator
declare const isFileDropItem, isTextDropItem, isDirectoryDropItem, DIRECTORY_DRAG_TYPE

// Async collection data (react-stately re-exports): loading/pagination/sort folded into collection state.
declare function useAsyncList<T>(options: AsyncListOptions<T, C>): AsyncListData<T>   // + useListData / useTreeData
```

Consumer note: `TableLayout`/`ListLayout`/`GridLayout`/`WaterfallLayout` own virtual geometry; `renderEmptyState` and the `*LoadMoreItem` sentinels (`ListBoxLoadMoreItem`, `TableLoadMoreItem`, `TreeLoadMoreItem`, `GridListLoadMoreItem`) own the empty/loading arms; `ResizableTableContainer` + `ColumnResizer` own resize. A custom item type is `createLeafComponent`/`createBranchComponent`.

## [04]-[OVERLAYS_FORMS_DND_TOAST_INFRA]

The advanced surfaces beyond the roster — the pieces a dense compose/primitive page wires directly.

- Overlays: `DialogTrigger`/`Dialog`/`Modal`/`ModalOverlay`/`Popover`/`Tooltip`/`OverlayArrow` own focus-trap, dismiss, and positioning; `Placement` is the anchor axis; `OverlayTriggerStateContext`/`RootMenuTriggerStateContext`/`TooltipTriggerStateContext` expose open state.
- Forms: `Form` carries the `validationBehavior?: 'native' | 'aria'` axis; `FieldError` renders a `ValidationResult`; `FormValidationContext` injects server/schema errors by field name — the seam for Schema-driven validation.
- Toast (pre-stable): `UNSTABLE_Toast`/`UNSTABLE_ToastRegion`/`UNSTABLE_ToastList`/`UNSTABLE_ToastContent` render a `queue: UNSTABLE_ToastQueue<T>` (react-stately `ToastQueue`); the region carries a built-in ARIA live region. `QueuedToast`/`ToastOptions`/`ToastState`/`UNSTABLE_ToastStateContext` type the queue.
- Transitions (pre-stable, 1.19): `SharedElementTransition`/`SharedElement` — the shared-element animation surface pairing with the native View Transitions plane.
- Infra: `I18nProvider`/`useLocale`/`isRTL` (locale over native `Intl`), `RouterProvider` (client-nav integration, `RouterConfig`), `SSRProvider` (id stability), `useFilter` (locale-aware `contains`/`startsWith`/`endsWith`).
- Re-exported shared vocab (`@react-types/shared`): `Key`, `Selection`, `PressEvent`, `RangeValue`, `ValidationResult`, `RouterConfig`, and the full drag-drop event union (`DroppableCollection*Event`, `DraggableCollection*Event`, `DropItem`/`FileDropItem`/`TextDropItem`/`DirectoryDropItem`).

## [05]-[INTEGRATION]

[STACK: `RenderProps` `data-*` + `tailwindcss-react-aria-components` + `cva`/`clsx`/`tailwind-merge` (`.api/tailwindcss-react-aria-components.md`, `.api/class-variance-authority.md`, `.api/clsx.md`, `.api/tailwind-merge.md`)] — the styling rail: RAC emits `data-hovered`/`data-selected`/`data-focus-visible`/`data-pressed`/`data-disabled` on its elements — including parent components, so `group-*`/`peer-*` variants target ancestor RAC state; the tailwind plugin maps each to its variant (`data-hovered`→`hover:`, `data-selected`→`selected:`, `data-focus-visible`→`focus-visible:`, `data-pressed`→`pressed:`, `data-disabled`→`disabled:` — the variant is the attribute's short name, not the `data-*` suffix), so state styling is a class string, not a render function. Where variants compose, `composeRenderProps(className, (cn) => twMerge(cva(base, variants)(state), cn))` layers cva variants over the user class deduped by tailwind-merge — the exact `composeRenderProps` idiom.

[STACK: `Form`/`FieldError` `validationBehavior:'aria'` + `Schema.standardSchemaV1` (`.api/effect.md`)] — the Schema→aria FormBinding seam: a kernel `Schema` projected via `Schema.standardSchemaV1(FieldSchema)` validates a field; its `ValidationResult` feeds `FieldError`, and `FormValidationContext` injects decode errors by field name. `validationBehavior:'aria'` marks fields invalid via ARIA without blocking native submit — the design's form rows decode once at the boundary and render the same error set.

[STACK: controlled props + `@effect-atom` (`.api/effect-atom-atom-react.md`, `.api/effect-atom-atom.md`)] — the one state binding: `selectedKeys`/`value`/`isOpen`/`sortDescriptor`/`expandedKeys` bind to atoms (`ONE_FOLD_ONE_BINDING`), RAC running controlled. Boundary: `useListData`/`useAsyncList` are RAC-native, but list/async state the app owns routes through the atom binding and RAC's controlled mode — never a second state store.

[STACK: render dispatch + `effect/Match` (`.api/effect.md`)] — a `children`/`className` function dispatches the render state with `Match.value(renderProps)` for closed-arm styling; `renderEmptyState`/the `*LoadMoreItem` arms dispatch the `AsyncListData` status through `Match.tagsExhaustive`.

[STACK: `I1 catalognProvider`/`useLocale` + intl plane (`.api/react-aria.md`)] — the intl/format rows compose `I1 catalognProvider` over native `Intl` keyed by the kernel `Locale` brand; RAC reads locale from it, `./i18n/*` bundles localize built-in component strings, and `useFilter` supplies the locale-aware ComboBox/Autocomplete matcher.

[BOUNDARY: RAC vs sibling owners] — `Table`/`Virtualizer` (RAC: accessible interactive collection, selection/sort/resize/drag) vs `@tanstack/react-table`/`@tanstack/react-virtual` (`.api/tanstack-react-table.md`, `.api/tanstack-react-virtual.md`: headless data-grid modeling, arbitrary DOM virtualization) — accessible collection = RAC, heavy grid/faceting model = TanStack; where a heavy TanStack-modeled grid needs accessibility, the react-aria `grid`/`row`/`columnheader`/`gridcell` ARIA + roving-keyboard spine wraps the headless TanStack rows (react-aria semantics over the TanStack model), and `aria-rowcount`/`aria-rowindex` stay on the full logical count while `@tanstack/react-virtual` mounts only the visible span. `Popover`/`Tooltip` (RAC aria overlay + `Placement`) vs `@floating-ui/react` (`.api/floating-ui-react.md`: bespoke non-aria anchoring, presence-cursor cohort) — one positioner per node. `Autocomplete` (in-field filtering) vs `cmdk` (`.api/cmdk.md`: the global command palette). `Modal` vs `vaul` (`.api/vaul.md`: touch-drag bottom sheet). `Label`/`Separator`/`VisuallyHidden`/`render` (aria spine) vs the radix `.api/radix-ui-*` primitives (non-aria styling plane) — never a radix `Label` inside an RAC field. `SharedElementTransition` composes the `act/transition` native View Transitions owner; RAC `children` rendering decoded wire HTML sanitizes through `isomorphic-dompurify` (`.api/isomorphic-dompurify.md`) first; async collections wrap in `react-error-boundary` (`.api/react-error-boundary.md`) around `renderEmptyState`.

## [06]-[RAIL_LAW]

- Owns: the headless accessible component spine — the render-props + context + slot mechanism, the `Xxx`/`XxxContext`/`XxxStateContext` triple, the collection/overlay/form/date/color families, the shared collection engine (`Collection`/`CollectionBuilder`/`Virtualizer`/layouts/drag-drop), toast, and the i18n/router/SSR/filter infra.
- Accept: composing the [01] pattern (not memorizing per-component APIs); `data-*` tailwind variants for state styling, the `className`/`children` function only where a variant cannot reach; `composeRenderProps` for styled wrappers; `Provider` for context collapse; `validationBehavior:'aria'` + `FieldError` fed by `Schema.standardSchemaV1`; controlled props bound to the atom; `I18nProvider`/`useLocale` over the intl plane; `createLeafComponent`/`createBranchComponent` for custom items; `Virtualizer` + a layout for large collections.
- Reject: hand-rolling accessibility a component owns; a `className` string where a `data-*` variant expresses the state; nested `XContext.Provider` towers where `Provider` collapses them; `useListData` for state the atom binding owns; double-positioning an aria overlay with floating-ui; a radix `Label`/`Separator`/`VisuallyHidden` inside an aria field; importing the bundled `*.css` (style via tailwind); shipping `UNSTABLE_*` toast / `SharedElementTransition` without noting the pre-stable marker.
- Boundary: `sideEffects: ["*.css"]` — the headless core is pure, only the optional stylesheet side-effects; `client-only` ⇒ client-tier (RSC boundary). Internalizes `react-aria@catalog` + `react-stately@catalog` + `@internationalized/date`; peer react/react-dom 19. Per-component subpaths + `./i18n/*` locale bundles. `UNSTABLE_`/`SharedElement*` are pre-stable API.
