# [TS_UI_API_REACT_ARIA_COMPONENTS]

`react-aria-components` owns the headless accessible component spine — unstyled components owning keyboard, focus, ARIA, i18n, and interaction, all styling left to the consumer.

Every component instantiates one pattern — `AriaHook ∘ StateHook ∘ RenderProps<state> ∘ ContextValue ∘ SlotProps` — shipping the uniform `Xxx`/`XxxContext`/`XxxStateContext` triple; a design composes the pattern, never a per-component API.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-aria-components`
- package: `react-aria-components` (Apache-2.0)
- module: dual ESM/CJS (`dist/exports/index.mjs`, `.cjs`) with per-component subpaths (`react-aria-components/Button`) and `./i18n` + `./i18n/*` locale bundles via `exports["./*"]`.
- asset: `dist/types/exports/index.d.ts` with per-component `.d.ts` declarations.
- runtime: React render-time; internalizes `react-aria` (behavior/ARIA hooks), `react-stately` (collection/selection/form state), `@internationalized/date` (calendar/date values), `@react-types/shared` (shared vocab); peer `react`/`react-dom`.
- abi: headless core is pure — only the bundled `*.css` side-effects (`sideEffects: ["*.css"]`); a `client-only` import marks the client tier (RSC boundary).
- plane: `plane:runtime` (W4 `ui`), folder-local to `ui` — the headless spine every `view` row composes.
- rail: `ui/view` — the accessible component spine.

## [02]-[THE_ONE_PATTERN]

`StyleRenderProps<T>` carries `className`/`style`/`render` and `RenderProps<T>` carries `children`, each a value or a function of the component's render state surfaced as `data-*` selectors; `ContextValue<T,E>` injects props for compound composition and `SlotProps` names the slot. Every [03] row varies only the state type these owners carry, and the function form is reserved for state a `data-*` variant cannot express — `render` overrides the aria element where the non-aria plane uses the radix `asChild` slot.

[PATTERN_SURFACES]: the value substrate a styled wrapper composes.

| [INDEX] | [SURFACE]                          | [SHAPE]  | [CAPABILITY]                                                       |
| :-----: | :--------------------------------- | :------- | :----------------------------------------------------------------- |
|  [01]   | `composeRenderProps(value, wrap)`  | fold     | layer a wrapper over a render-prop value or function               |
|  [02]   | `useRenderProps(options)`          | fold     | resolve `className`/`style`/`children` against state               |
|  [03]   | `useContextProps(props, ref, Ctx)` | fold     | merge injected context props with local, return `[props, ref]`     |
|  [04]   | `useSlottedContext(Ctx, slot?)`    | fold     | read a slotted context value                                       |
|  [05]   | `Provider({values, children})`     | factory  | collapse nested `XxxContext.Provider` towers to one `values` array |
|  [06]   | `DEFAULT_SLOT`                     | property | the unnamed-slot key                                               |

## [03]-[COMPONENT_FAMILIES]

Each row is a family of the `Xxx`/`XxxContext`/`XxxStateContext` triple; every `XxxProps extends Aria<Xxx>Props, RenderProps<XxxRenderProps>, SlotProps`, and each `XxxRenderProps` exposes boolean state (`isHovered`, `isSelected`, `isDisabled`, `isPending`, `isOpen`) as `data-*` selectors.

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

Stateful families bind their react-stately state: fields carry `validationBehavior` + `ValidationResult`, date/time bind `@internationalized/date` values, color binds `parseColor`/`getColorChannels`, pickers add `useFilter` locale matching. Compound composition reads the `XxxStateContext` (`ListStateContext`, `TableStateContext`, `OverlayTriggerStateContext`, `SelectStateContext`, `TooltipTriggerStateContext`, `TabListStateContext`) rather than prop-drilling; `XxxContext` injects props via `Provider`.

## [04]-[COLLECTION_ENGINE]

Collections, selection, sorting, virtualization, drag-drop, and async data are one react-stately engine the collection and picker families share; a custom item is authored through `createLeafComponent`/`createBranchComponent`, never hand-parsed `children`.

[SURFACES]: `createLeafComponent` `createBranchComponent` `CollectionBuilder` `Collection` `Section` `Virtualizer` `Layout` `ResizableTableContainer` `useTableOptions` `Key` `Selection` `SelectionMode` `SortDescriptor` `SortDirection` `useDragAndDrop` `isFileDropItem` `useAsyncList`

`TableLayout`/`ListLayout`/`GridLayout`/`WaterfallLayout` own virtual geometry; `renderEmptyState` and the `*LoadMoreItem` sentinels (`ListBoxLoadMoreItem`, `TableLoadMoreItem`, `TreeLoadMoreItem`, `GridListLoadMoreItem`) own the empty/loading arms; `ResizableTableContainer` + `ColumnResizer` own resize.

## [05]-[OVERLAYS_FORMS_DND_TOAST_INFRA]

- Overlays: `DialogTrigger`/`Dialog`/`Modal`/`ModalOverlay`/`Popover`/`Tooltip`/`OverlayArrow` own focus-trap, dismiss, and positioning; `Placement` is the anchor axis; `OverlayTriggerStateContext`/`RootMenuTriggerStateContext`/`TooltipTriggerStateContext` expose open state.
- Forms: `Form` carries the `validationBehavior: 'native' | 'aria'` axis; `FieldError` renders a `ValidationResult`; `FormValidationContext` injects server or schema errors by field name.
- Toast (pre-stable): `UNSTABLE_Toast`/`UNSTABLE_ToastRegion`/`UNSTABLE_ToastList`/`UNSTABLE_ToastContent` render a `UNSTABLE_ToastQueue<T>` whose region carries a built-in ARIA live region; `QueuedToast`/`ToastOptions`/`ToastState`/`UNSTABLE_ToastStateContext` type the queue.
- Transitions (pre-stable): `SharedElementTransition`/`SharedElement` pair with the native View Transitions plane.
- Infra: `I18nProvider`/`useLocale`/`isRTL` (locale over native `Intl`), `RouterProvider` (client-nav integration, `RouterConfig`), `SSRProvider` (id stability), `useFilter` (locale-aware `contains`/`startsWith`/`endsWith`).
- Shared vocab (`@react-types/shared`): `Key`, `Selection`, `PressEvent`, `RangeValue`, `ValidationResult`, `RouterConfig`, and the drag-drop event union (`DroppableCollection*Event`, `DraggableCollection*Event`, `DropItem`/`FileDropItem`/`TextDropItem`/`DirectoryDropItem`).

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every component folds `AriaHook ∘ StateHook ∘ RenderProps ∘ ContextValue ∘ SlotProps`, so composing the pattern replaces every per-component API.
- State styling rides `data-*` attributes on every element, parents included, so `group-*`/`peer-*` variants target ancestor state; the function form is reserved for state no variant reaches.

[STACKING]:
- `tailwindcss-react-aria-components`(`.api/tailwindcss-react-aria-components.md`), `class-variance-authority`(`.api/class-variance-authority.md`), `clsx`(`.api/clsx.md`), `tailwind-merge`(`.api/tailwind-merge.md`): RAC `data-hovered`/`data-selected`/`data-focus-visible`/`data-pressed`/`data-disabled` map to tailwind variants keyed by the attribute short name, and `composeRenderProps(className, cn => twMerge(cva(base, variants)(state), cn))` layers cva variants over the user class deduped by tailwind-merge.
- `effect`(`libs/typescript/.api/effect.md`): `Schema.standardSchemaV1(FieldSchema)` validates a field into a `ValidationResult` fed to `FieldError` while `FormValidationContext` injects decode errors by field name and `validationBehavior:'aria'` marks fields invalid without blocking native submit; a `children`/`className` function dispatches render state through `Match.value`, and `renderEmptyState`/`*LoadMoreItem` arms dispatch async status through `Match.tagsExhaustive`.
- `effect-atom-atom-react`(`.api/effect-atom-atom-react.md`), `effect-atom-atom`(`.api/effect-atom-atom.md`): controlled props (`selectedKeys`/`value`/`isOpen`/`sortDescriptor`/`expandedKeys`) bind to atoms with RAC in controlled mode, and app-owned list or async state routes through the atom binding instead of `useListData`/`useAsyncList`.
- `react-aria`(`.api/react-aria.md`): `I18nProvider` over native `Intl` keyed by the kernel `Locale` brand supplies locale, `./i18n/*` bundles localize built-in strings, and `useFilter` supplies the `ComboBox`/`Autocomplete` matcher.
- within `ui/view`: `primitive.md` composes the component spine and toast/live-region; `compose.md` composes the Schema→aria form binding, picker, table/virtual, and floating-anchor/sheet rows.

[LOCAL_ADMISSION]:
- Accessible interactive collection routes to RAC; a heavy grid or faceting model routes to `@tanstack/react-table`/`@tanstack/react-virtual`, wrapped in the react-aria `grid`/`row`/`columnheader`/`gridcell` ARIA + roving-keyboard spine — `aria-rowcount`/`aria-rowindex` carry the full logical count while only the visible span mounts.
- `Placement` routes an aria overlay to RAC; bespoke non-aria anchoring routes to `@floating-ui/react`, one positioner per node.
- In-field filtering is RAC `Autocomplete`, a global command palette is `cmdk`, a touch-drag bottom sheet is `vaul`; aria `Label`/`Separator`/`VisuallyHidden`/`render` are RAC, the non-aria styling plane is the radix primitives — a radix `Label` never enters an RAC field.
- RAC `children` rendering decoded wire HTML sanitizes through `isomorphic-dompurify` first; an async collection wraps in `react-error-boundary` around `renderEmptyState`; `SharedElementTransition` composes the `act/transition` View Transitions owner.

[RAIL_LAW]:
- Package: `react-aria-components`
- Owns: the headless accessible component spine — the render-props/context/slot mechanism, the `Xxx`/`XxxContext`/`XxxStateContext` triple, the collection/overlay/form/date/color families, the shared collection engine, toast, and the i18n/router/SSR/filter infra.
- Accept: composing the [02] pattern; `data-*` tailwind variants for state, the function form only where a variant cannot reach; `composeRenderProps` for styled wrappers; `Provider` for context collapse; `validationBehavior:'aria'` + `FieldError` fed by `Schema.standardSchemaV1`; controlled props bound to the atom; `I18nProvider`/`useLocale` over the intl plane; `createLeafComponent`/`createBranchComponent` for custom items; `Virtualizer` + a layout for large collections.
- Reject: hand-rolling accessibility a component owns; a `className` string where a `data-*` variant expresses the state; nested `XxxContext.Provider` towers where `Provider` collapses them; `useListData` for state the atom binding owns; double-positioning an aria overlay with floating-ui; a radix `Label`/`Separator`/`VisuallyHidden` inside an aria field; importing the bundled `*.css`.
