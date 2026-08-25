# [TS_UI_API_REACT_ARIA_COMPONENTS]

`react-aria-components` owns the headless accessible component spine — unstyled components owning keyboard, focus, ARIA, i18n, and interaction, all styling left to the consumer.

Every component instantiates one pattern — `AriaHook ∘ StateHook ∘ RenderProps<state> ∘ ContextValue ∘ SlotProps` — shipping the uniform `Xxx`/`XxxContext`/`XxxStateContext` triple; a design composes the pattern, never a per-component API.

## [01]-[THE_ONE_PATTERN]

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

## [02]-[COMPONENT_FAMILIES]

Each row is a family of the `Xxx`/`XxxContext`/`XxxStateContext` triple; every `XxxProps extends Aria<Xxx>Props, RenderProps<XxxRenderProps>, SlotProps`, and each `XxxRenderProps` exposes boolean state (`isHovered`, `isSelected`, `isDisabled`, `isPending`, `isOpen`) as `data-*` selectors.

| [INDEX] | [FAMILY]    | [COMPONENTS]                                                                                                         |
| :-----: | :---------- | :------------------------------------------------------------------------------------------------------------------- |
|  [01]   | actions     | `Button` `ToggleButton` `ToggleButtonGroup` `Link` `FileTrigger`                                                     |
|  [02]   | collections | `ListBox` `GridList` `Menu` `Table` `Tree` `TagGroup` `Tabs` `Breadcrumbs` `Toolbar`                                 |
|  [03]   | pickers     | `Select` `ComboBox` `Autocomplete`                                                                                   |
|  [04]   | overlays    | `DialogTrigger` `Dialog` `Modal` `ModalOverlay` `Popover` `Tooltip`(`Trigger`) `PreviewTrigger` `OverlayArrow`       |
|  [05]   | fields      | `Form` `FieldError` `Label` `Input` `TextField` `TextArea` `SearchField` `NumberField` `TokenField`(`Input`/`Token`) |
|  [06]   | toggles     | `Checkbox`(`Group`) `RadioGroup` `Switch` `Slider` `Meter` `ProgressBar`                                             |
|  [07]   | date/time   | `Calendar` `RangeCalendar` `DateField` `TimeField` `DatePicker` `DateRangePicker`                                    |
|  [08]   | color       | `ColorPicker` `ColorArea` `ColorField` `ColorSlider` `ColorWheel` `ColorSwatch`(`Picker`) `ColorThumb`               |
|  [09]   | structure   | `Group` `Separator` `Heading` `Header` `Text` `Keyboard` `Disclosure`(`Group`)                                       |
|  [10]   | interaction | `Pressable` `Focusable` `VisuallyHidden`                                                                             |

Stateful families bind their react-stately state: fields carry `validationBehavior` + `ValidationResult`, date/time bind `@internationalized/date` values, color binds `parseColor`/`getColorChannels`, pickers add `useFilter` locale matching. Compound composition reads the `XxxStateContext` (`ListStateContext`, `TableStateContext`, `OverlayTriggerStateContext`, `SelectStateContext`, `TooltipTriggerStateContext`, `TabListStateContext`) rather than prop-drilling; `XxxContext` injects props via `Provider`.

## [03]-[COLLECTION_ENGINE]

Collections, selection, sorting, virtualization, drag-drop, and async data are one react-stately engine the collection and picker families share; a custom item is authored through `createLeafComponent`/`createBranchComponent`, never hand-parsed `children`.

[SURFACES]: `createLeafComponent` `createBranchComponent` `CollectionBuilder` `Collection` `Section` `Virtualizer` `Layout` `ResizableTableContainer` `useTableOptions` `Key` `Selection` `SelectionMode` `SortDescriptor` `SortDirection` `useDragAndDrop` `isFileDropItem` `useAsyncList`

`TableLayout`/`ListLayout`/`GridLayout`/`WaterfallLayout` own virtual geometry; `renderEmptyState` and the `*LoadMoreItem` sentinels (`ListBoxLoadMoreItem`, `TableLoadMoreItem`, `TreeLoadMoreItem`, `GridListLoadMoreItem`) own the empty/loading arms; `ResizableTableContainer` + `ColumnResizer` own resize. `keyboardNavigationBehavior` on the table and `focusMode`/`allowsArrowNavigation` on `Column`/`Cell`/`GridListItem`/`TreeItem` declare traversal per collection, and `Virtualizer`'s `shouldObserveItemSize` re-measures items whose content resizes in place.

## [04]-[OVERLAYS_FORMS_DND_TOAST_INFRA]

- Overlays: `DialogTrigger`/`Dialog`/`Modal`/`ModalOverlay`/`Popover`/`Tooltip`/`OverlayArrow` own focus-trap, dismiss, and positioning; `Placement` is the anchor axis; `OverlayTriggerStateContext`/`RootMenuTriggerStateContext`/`TooltipTriggerStateContext` expose open state. `PreviewTrigger` carries the tooltip's hover/focus/long-press opening with a dwell `delay`/`closeDelay` while admitting interactive popover content, and `PopoverProps.shouldSkipAnimation` suppresses the exit transition where a caller drives it.
- Context menus: `MenuTriggerProps.trigger` closes over exactly `'press' | 'longPress' | 'contextMenu'`, so a right-click menu is a trigger value on the one menu pattern; the trigger state carries the invocation `point` the popover anchors at. `contextMenu` alone forces `offset: 0` on the popover — every other trigger leaves the offset defaulted — and `usePopover` folds the point into geometry as `getTargetRect: () => new DOMRect(point.x, point.y, 0, 0)`, so the menu anchors to a zero-area rect at the cursor instead of to the trigger's own box.
- Token field: `TokenField`/`TokenInput`/`Token`/`TokenFieldContext` are STABLE, unprefixed exports on the main barrel, which also re-exports the `react-stately` `TokenFieldValue` class the field's value algebra subclasses.
- Forms: `Form` carries the `validationBehavior: 'native' | 'aria'` axis; `FieldError` renders a `ValidationResult`; `FormValidationContext` injects server or schema errors by field name.
- Toast (pre-stable): the main barrel ships the roster prefixed WITHOUT EXCEPTION — `UNSTABLE_Toast`, `UNSTABLE_ToastList`, `UNSTABLE_ToastRegion`, `UNSTABLE_ToastContent`, `UNSTABLE_ToastStateContext`, and the react-stately queue re-exported as `UNSTABLE_ToastQueue` — while the per-component `react-aria-components/Toast` subpath dual-exports each under both its bare and its prefixed name, so an unprefixed `ToastContent` compiles from the subpath and fails from the barrel. `QueuedToast`/`ToastOptions`/`ToastState` type the queue and carry no politeness member; the region element is a labelled landmark, and the live semantics sit on each note's content element.
- Transitions (pre-stable): `SharedElementTransition`/`SharedElement` pair with the native View Transitions plane.
- Infra: `I18nProvider`/`useLocale`/`isRTL` (locale over native `Intl`), `RouterProvider` (client-nav integration, `RouterConfig`), `SSRProvider` (id stability), `useFilter` (locale-aware `contains`/`startsWith`/`endsWith`).
- Shared vocab (`@react-types/shared`): `Key`, `Selection`, `PressEvent`, `RangeValue`, `ValidationResult`, `RouterConfig`, and the drag-drop event union (`DroppableCollection*Event`, `DraggableCollection*Event`, `DropItem`/`FileDropItem`/`TextDropItem`/`DirectoryDropItem`).

## [05]-[IMPLEMENTATION_LAW]

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
- In-field filtering is RAC `Autocomplete`, a global command palette is `cmdk`, a touch-drag bottom sheet is `vaul`; `Label`, `Separator`, `VisuallyHidden`, and the `render` element override are RAC's outright, and the radix plane survives only where no aria part answers — `createSlot` polymorphism on a non-aria node and an element-scoped SR-only label.
- RAC `children` rendering decoded wire HTML sanitizes through `isomorphic-dompurify` first; an async collection wraps in `react-error-boundary` around `renderEmptyState`; `SharedElementTransition` composes the `act/transition` View Transitions owner.
