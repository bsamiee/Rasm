# [TS_UI_API_REACT_ARIA_COMPONENTS]

[PACKAGE_SURFACE]:
- package: `react-aria-components` (Apache-2.0)
- module: dual — `dist/exports/index.mjs` (ESM) + `dist/exports/index.cjs` (CJS); per-component subpaths (`react-aria-components/Button`, …) via `exports["./*"]`; `./i18n` + `./i18n/*` locale bundles. `sideEffects: ["*.css"]` — the headless core is pure; only the optional bundled stylesheet side-effects. `client-only` import ⇒ client-tier (RSC boundary).
- asset: `dist/types/exports/index.d.ts` + ~80 per-component `.d.ts` (`restore: restored`, 79 declaration assets).
- runtime: React 19 render-time; internalizes `react-aria@catalog` (behavior/ARIA hooks) + `react-stately@catalog` (collection/selection/form state) + `@internationalized/date catalog` (calendar/date values) + `@react-types/shared` (the shared vocab). Peer react/react-dom 19.
- plane: `plane:runtime` (W4 `ui`); folder-local to `ui`, the headless spine every `view` row composes.
- rail: `ui/view` — the accessible component spine.
- role: `view/primitive.md` (the component spine + toast/live-region) and `view/compose.md` (Schema→aria FormBinding, picker, table/virtual, floating-anchor/sheet rows).

`react-aria-components` is the headless component spine — ~65 fully-accessible, unstyled components that own keyboard, focus, ARIA, i18n, and interaction, leaving all styling to the consumer. The load-bearing fact for a dense design: it is NOT 65 bespoke APIs but ONE parameterized pattern instantiated 65 times. Every component is `AriaHook(react-aria) ∘ StateHook(react-stately) ∘ RenderProps<state> ∘ ContextValue injection ∘ SlotProps`, and ships a uniform triple — `Xxx` (the element), `XxxContext` (prop injection for compound composition), `XxxStateContext` (the react-stately state exposed to descendants). Styling is state-driven: `className`/`style`/`children` each accept a function of the component's render state, surfaced as `data-*` selectors. The component roster is SEED DATA on the render-props + context + slot mechanism; a new component is a new instance of the pattern, and the folder composes the pattern — not a per-component memorized API. The mechanism owners are documented first because they, not the component list, are what a design reasons against.

## [01]-[THE_ONE_PATTERN]

Every component styles through `RenderProps`, injects props through a `ContextValue`, and names its slot through `SlotProps`. These owners are the mechanism; the roster in [02] varies only the state type `T`. The value exports here (`composeRenderProps`, `Provider`, `useRenderProps`, `useContextProps`, `useSlottedContext`, `DEFAULT_SLOT`) and type exports (`RenderProps`, `StyleRenderProps`, `ContextValue`, `SlotProps`) are the entire compositional substrate.

[CLASS_NAME_OR_FUNCTION]: `ClassNameOrFunction = string|((s:T&{defaultClassName:string|undefined})=>string)`
[STYLE_RENDER_PROPS]: `StyleRenderProps.className: ClassNameOrFunction<T>` `StyleRenderProps.style: CSSProperties|((s:T&{defaultStyle:CSSProperties})=>CSSProperties|undefined)` `StyleRenderProps.render: (props:JSX.IntrinsicElements[E],renderProps:T)=>ReactElement`
[RENDER_PROPS]: `RenderProps.children: ReactNode|((s:T&{defaultChildren:ReactNode|undefined})=>ReactNode)`
[CONTEXT_VALUE]: `ContextValue = SlottedContextValue<T&{ref?:ForwardedRef<E>}>`
[SLOT_PROPS]: `SlotProps.slot: string|null`
[SURFACES]: `composeRenderProps(T|((r:U)=>V),(prev:T,r:U)=>V) -> (r:U)=>V` `useRenderProps(RenderPropsHookOptions<T,E>) -> {…}` `DEFAULT_SLOT: unique symbol` `useContextProps(T&SlotProps,ForwardedRef<E>|undefined,Context<ContextValue<U,E>>) -> [T,RefObject<E|null>]` `useSlottedContext(Context<SlottedContextValue<T>>,string|null?) -> T|null|undefined` `Provider({values:readonly[Context<A>,A][];children:ReactNode}) -> JSX.Element`

Consumer note: styling rarely needs the function form — the `data-*` selectors ([`tailwindcss-react-aria-components`]) express state as Tailwind variants; reach for the `className`/`children` function only for state a variant cannot express. `render` is the element override for the aria spine (radix `asChild` serves the non-aria plane). `Provider` replaces nested `<XContext.Provider>` towers with one `values` array.

## [02]-[COMPONENT_FAMILIES]

The roster — each row a family of the `Xxx` / `XxxContext` / `XxxStateContext` triple. Every `XxxProps extends Aria<Xxx>Props, RenderProps<XxxRenderProps>, SlotProps` plus a shared DOM-attributes base; each `XxxRenderProps` exposes the boolean/data state (`isHovered`, `isSelected`, `isDisabled`, `isPending`, `isOpen`, …) as `data-*` selectors. This is SEED DATA on [01], not a distinct API per row.

| [INDEX] | [FAMILY]    | [COMPONENTS]                                                                                           |
| :-----: | :---------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | actions     | `Button` `ToggleButton` `ToggleButtonGroup` `Link` `FileTrigger`                                       |
|  [02]   | collections | `ListBox` `GridList` `Menu` `Table` `Tree` `TagGroup` `Tabs` `Breadcrumbs` `Toolbar`                   |
|  [03]   | pickers     | `Select` `ComboBox` `Autocomplete`                                                                     |
|  [04]   | overlays    | `DialogTrigger` `Dialog` `Modal` `ModalOverlay` `Popover` `Tooltip`(`Trigger`) `OverlayArrow`          |
|  [05]   | fields      | `Form` `FieldError` `Label` `Input` `TextField` `TextArea` `SearchField` `NumberField`                 |
|  [06]   | toggles     | `Checkbox`(`Group`) `RadioGroup` `Switch` `Slider` `Meter` `ProgressBar`                               |
|  [07]   | date/time   | `Calendar` `RangeCalendar` `DateField` `TimeField` `DatePicker` `DateRangePicker`                      |
|  [08]   | color       | `ColorPicker` `ColorArea` `ColorField` `ColorSlider` `ColorWheel` `ColorSwatch`(`Picker`) `ColorThumb` |
|  [09]   | structure   | `Group` `Separator` `Heading` `Header` `Text` `Keyboard` `Disclosure`(`Group`)                         |
|  [10]   | interaction | `Pressable` `Focusable` `VisuallyHidden`                                                               |

State axis per family:
- [01]-[ACTIONS]: press/toggle/upload; `ButtonRenderProps.isPending`/`isDisabled`.
- [02]-[COLLECTIONS]: react-stately collection state; selection/sort/drag on the [03] engine.
- [03]-[PICKERS]: collection + overlay + `useFilter` locale matching.
- [04]-[OVERLAYS]: focus-trap + dismiss + positioning (`Placement`).
- [05]-[FIELDS]: `validationBehavior` + `ValidationResult`.
- [06]-[TOGGLES]: react-stately toggle/slider/number state.
- [07]-[DATE_TIME]: `@internationalized/date` (`DateValue`/`TimeValue`/`DateRange`).
- [08]-[COLOR]: `parseColor`/`getColorChannels`; `Color`/`ColorSpace`/`ColorChannel`.
- [09]-[STRUCTURE]: labeling/structure primitives.
- [10]-[INTERACTION]: raw press/focus/SR-only on the aria spine.

Consumer note: each component also exports `XxxContext` (inject props via `Provider`) and, where stateful, `XxxStateContext` (read the react-stately state — `ListStateContext`, `TableStateContext`, `OverlayTriggerStateContext`, `SelectStateContext`, `RootMenuTriggerStateContext`, `TooltipTriggerStateContext`, `TabListStateContext`, …). Compound composition reads the state context rather than prop-drilling.

## [03]-[COLLECTION_ENGINE]

Collections, selection, sorting, virtualization, drag-drop, and async data are ONE engine the collection/picker families share — react-stately state surfaced through RAC. A custom item is authored through the factory pair, never hand-parsed `children`.

[SURFACES]: `createLeafComponent` `createBranchComponent` `CollectionBuilder` `Collection` `Section` `Virtualizer` `Layout` `ResizableTableContainer` `useTableOptions` `Key` `Selection` `SelectionMode` `SortDescriptor` `SortDirection` `useDragAndDrop` `isFileDropItem` `useAsyncList`

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
