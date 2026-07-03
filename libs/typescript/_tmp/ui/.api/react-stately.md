# [API_CATALOGUE] react-stately

`react-stately` is the headless state layer under every React Aria widget family. The package is one parameterized family: each `use<Widget>State(props) => <Widget>State` hook returns a typed state object passed verbatim as the `state` argument to the matching `react-aria` `use<Widget>` hook, or read from the matching `react-aria-components` `<Widget>StateContext`. Beside the widget hooks it owns the collection model (`Item`, `Section`, `useCollection`, the `Table*`/`Column`/`Row`/`Cell` collection components), the selection model (`useMultipleSelectionState`, `SelectionManager`), the drag-and-drop collection state, local/async data managers (`useListData`, `useTreeData`, `useAsyncList`), the `ToastQueue` external store, the `FormValidationContext` server-error seam, and the virtualiser layout primitives the AppUi collection surfaces consume.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-stately`
- package: `react-stately`
- version: `3.48.0`
- license: `Apache-2.0`
- module: dual ESM (`dist/exports/index.mjs`) / CJS (`.cjs`); barrel `react-stately` plus per-hook deep sub-paths `react-stately/use*State` (each `exports["./*"]`)
- runtime: peer `react`; deps `@internationalized/{date,number,string}`, `@react-types/shared` (re-exports `Key`/`Node`/`Collection`/`Selection`/`SortDescriptor`), `use-sync-external-store`
- rail: state

## [02]-[PUBLIC_TYPES]

Every widget hook carries a `<Widget>State` result, a `<Widget>StateOptions` (or `<Widget>Props`) input, and shared `@react-types/shared` primitives; the tables below name the load-bearing families and the cross-cutting types the design pages compose.

[PUBLIC_TYPE_SCOPE]: overlay, trigger, toggle, and selection state
- rail: state

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]   | [SHAPE]                                                              |
| :-----: | :-------------------- | :-------------- | :------------------------------------------------------------------- |
|  [01]   | `OverlayTriggerState` | state interface | `isOpen`, `open()`, `close()`, `toggle()`, `setOpen(v)`             |
|  [02]   | `MenuTriggerState`    | state interface | `OverlayTriggerState` + focus-strategy; `RootMenuTriggerState`      |
|  [03]   | `ToggleState`         | state interface | `isSelected`, `setSelected`, `toggle`                              |
|  [04]   | `ToggleStateOptions`  | options interface | `isSelected?`, `defaultSelected?`, `onChange?`                   |
|  [05]   | `MultipleSelectionManager` | manager interface | `selectionMode`, `selectedKeys`, `select`, `toggleSelection`, `replaceSelection`, `extendSelection` |
|  [06]   | `Selection`           | model type      | `'all' \| Set<Key>` (from `@react-types/shared`)                   |
|  [07]   | `SelectionMode`       | union           | `'none' \| 'single' \| 'multiple'`                                |
|  [08]   | `SelectionBehavior`   | union           | `'toggle' \| 'replace'`                                            |

[PUBLIC_TYPE_SCOPE]: list, collection, and table state
- rail: state

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [SHAPE]                                                                       |
| :-----: | :------------------ | :-------------- | :--------------------------------------------------------------------------- |
|  [01]   | `ListState<T>`      | state interface | `collection: Collection<Node<T>>`, `selectionManager: SelectionManager`     |
|  [02]   | `SelectState<T, M>` | state interface | `ListState<T>` + `OverlayTriggerState`; `value`, `setValue`, `selectedItems` |
|  [03]   | `ComboBoxState<T>`  | state interface | `ListState<T>` + input value, open state, `commit`                          |
|  [04]   | `TableState<T>`     | state interface | `collection`, `selectionManager`, `sortDescriptor`, `sort`                  |
|  [05]   | `Node<T>`           | collection node | `type`, `key`, `value`, `rendered`, `childNodes`, `level`, `index`         |
|  [06]   | `Collection<T>`     | model interface | `size`, `getKeys`, `getItem`, `at`, iterable node graph                    |
|  [07]   | `Key`               | id type         | `string \| number` (from `@react-types/shared`)                            |
|  [08]   | `SortDescriptor`    | model interface | `{ column: Key; direction: SortDirection }` — drives `useAsyncList` server sort |

[PUBLIC_TYPE_SCOPE]: color state and value model
- rail: color

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [SHAPE]                                                                       |
| :-----: | :--------------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `Color`          | value interface | immutable color; `getChannelValue`, `withChannelValue`, `toString`, `getColorChannels` (see [COLOR_VALUE]) |
|  [02]   | `ColorSpace`     | union         | `'rgb' \| 'hsl' \| 'hsb'`                                                   |
|  [03]   | `ColorChannel`   | union         | `'hue' \| 'saturation' \| 'brightness' \| 'lightness' \| 'red' \| 'green' \| 'blue' \| 'alpha'` |
|  [04]   | `ColorFormat`    | union         | `'hex' \| 'hexa' \| 'rgb' \| 'rgba' \| 'hsl' \| 'hsla' \| 'hsb' \| 'hsba'` |
|  [05]   | `ColorChannelRange` | interface  | `minValue`, `maxValue`, `step`, `pageSize`                                 |
|  [06]   | `ColorPickerState` | state interface | `color: Color`, `setColor(color: Color \| null): void`                   |

[PUBLIC_TYPE_SCOPE]: data-management and toast types
- rail: state

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [SHAPE]                                                                              |
| :-----: | :-------------------- | :---------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `ListData<T>`         | result interface  | `items`, `selectedKeys`, `filterText`, `getItem`, `insert`/`append`/`remove`/`move`/`update` CRUD, `setSelectedKeys`, `setFilterText` |
|  [02]   | `ListOptions<T>`      | options interface | `initialItems?`, `initialSelectedKeys?`, `initialFilterText?`, `getKey?`, `filter?` |
|  [03]   | `TreeData<T>`         | result interface  | hierarchical `ListData<T>` with node-scoped mutation                                |
|  [04]   | `AsyncListData<T>`    | result interface  | `items`, `loadingState`, `loadMore`, `sort`, `reload`, `sortDescriptor`, `error`   |
|  [05]   | `AsyncListOptions<T>` | options interface | `load: AsyncListLoadFunction<T>`, `initialSortDescriptor?`, `getKey?`               |
|  [06]   | `ToastState<T>`       | state interface   | `add(content, options?): string`, `close(key)`, `pauseAll()`, `resumeAll()`, `visibleToasts` |
|  [07]   | `QueuedToast<T>`      | node interface    | `content: T`, `key: string`, `timeout?`, `onClose?`                                 |
|  [08]   | `ToastStateProps`     | options interface | `maxVisibleToasts?: number`, `wrapUpdate?: (fn, action) => void`                    |
|  [09]   | `ToastOptions`        | options interface | `timeout?: number`, `onClose?: () => void`                                          |

[PUBLIC_TYPE_SCOPE]: layout and virtualiser types
- rail: layout

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [NOTE]                                        |
| :-----: | :------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `Layout`             | base class    | abstract virtualiser layout                    |
|  [02]   | `ListLayout`         | class         | single-axis list layout; `ListLayoutOptions`  |
|  [03]   | `GridLayout`         | class         | two-axis grid layout; `GridLayoutOptions`     |
|  [04]   | `TableLayout`        | class         | table layout; `TableLayoutProps`              |
|  [05]   | `WaterfallLayout`    | class         | variable-height waterfall; `WaterfallLayoutOptions` |
|  [06]   | `LayoutInfo`         | class         | single-item layout descriptor                  |
|  [07]   | `Rect` / `Size` / `Point` | classes  | geometry value objects                         |
|  [08]   | `ReusableView`       | class         | pooled reusable virtual view                   |
|  [09]   | `VirtualizerState`   | state interface | visible-rect + reusable-view virtualiser state |

## [03]-[COLOR_VALUE]

[COLOR_VALUE_SCOPE]: the `Color` interface the picker composes
- rail: color

`parseColor(value)` returns a `Color`; `render/` and `interaction/picker.md` read these members to project a pick into the OKLCH token space.

| [INDEX] | [MEMBER]                              | [SIGNATURE]                                        | [NOTE]                                    |
| :-----: | :------------------------------------ | :------------------------------------------------- | :----------------------------------------- |
|  [01]   | `getChannelValue(channel)`            | `(channel: ColorChannel) => number`               | numeric channel read for token projection  |
|  [02]   | `withChannelValue(channel, value)`    | `(channel, value: number) => Color`               | immutable channel update                    |
|  [03]   | `toString(format?)`                   | `(format?: ColorFormat \| 'css') => string`       | `'css'` yields the `color(...)` string      |
|  [04]   | `toFormat(format)`                    | `(format: ColorFormat) => Color`                  | reinterpret into another space              |
|  [05]   | `getChannelRange(channel)`            | `(channel) => ColorChannelRange`                  | min/max/step for slider bounds              |
|  [06]   | `getColorSpace()` / `getColorChannels()` | `() => ColorSpace` / `() => [ColorChannel, ColorChannel, ColorChannel]` | space + ordered channel triple |

## [04]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: overlay, menu, and form-control state hooks
- rail: state

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [RETURNS]              |
| :-----: | :------------------------------ | :------------- | :--------------------- |
|  [01]   | `useOverlayTriggerState(props)` | state hook     | `OverlayTriggerState`  |
|  [02]   | `useMenuTriggerState(props)`    | state hook     | `MenuTriggerState`     |
|  [03]   | `useSubmenuTriggerState(props, state)` | state hook | `SubmenuTriggerState` (`state: RootMenuTriggerState`) |
|  [04]   | `useTooltipTriggerState(props)` | state hook     | `TooltipTriggerState`  |
|  [05]   | `useToggleState(props?)`        | state hook     | `ToggleState`          |
|  [06]   | `useToggleGroupState(props)`    | state hook     | `ToggleGroupState`     |
|  [07]   | `useCheckboxGroupState(props)`  | state hook     | `CheckboxGroupState`   |
|  [08]   | `useRadioGroupState(props)`     | state hook     | `RadioGroupState`      |
|  [09]   | `useSelectState(props)`         | state hook     | `SelectState<T, M>`    |
|  [10]   | `useComboBoxState(props)`       | state hook     | `ComboBoxState<T>`     |
|  [11]   | `useNumberFieldState(props)`    | state hook     | `NumberFieldState`     |
|  [12]   | `useSearchFieldState(props)`    | state hook     | `SearchFieldState`     |
|  [13]   | `useSliderState(props)`         | state hook     | `SliderState`          |

[ENTRYPOINT_SCOPE]: collection, list, selection, and DnD state hooks
- rail: state

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [RETURNS]                          |
| :-----: | :------------------------------------------ | :------------- | :--------------------------------- |
|  [01]   | `useListState(props)`                       | state hook     | `ListState<T>`                     |
|  [02]   | `useSingleSelectListState(props)`           | state hook     | `SingleSelectListState<T>`         |
|  [03]   | `UNSTABLE_useFilteredListState(state, filterFn)` | state hook | filtered `ListState<T>` — `filterFn: (nodeValue: string, node: Node<T>) => boolean` |
|  [04]   | `useTableState(props)`                      | state hook     | `TableState<T>`                    |
|  [05]   | `UNSTABLE_useFilteredTableState(state, fn)` | state hook     | filtered `TableState<T>` view      |
|  [06]   | `useTableColumnResizeState(props, state)`   | state hook     | `TableColumnResizeState<T>`        |
|  [07]   | `useTabListState(props)`                    | state hook     | `TabListState<T>`                  |
|  [08]   | `useTreeState(props)`                       | state hook     | `TreeState<T>`                     |
|  [09]   | `useDisclosureState(props?)`                | state hook     | `DisclosureState`                  |
|  [10]   | `useDisclosureGroupState(props?)`           | state hook     | `DisclosureGroupState`             |
|  [11]   | `useMultipleSelectionState(props)`          | state hook     | `MultipleSelectionState` (raw selection model) |
|  [12]   | `useDraggableCollectionState(props)`        | state hook     | `DraggableCollectionState`         |
|  [13]   | `useDroppableCollectionState(props)`        | state hook     | `DroppableCollectionState`         |

[ENTRYPOINT_SCOPE]: calendar, date, and colour state hooks
- rail: date-time / color

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RETURNS]                |
| :-----: | :--------------------------------- | :------------- | :----------------------- |
|  [01]   | `useCalendarState(props)`          | state hook     | `CalendarState`          |
|  [02]   | `useRangeCalendarState(props)`     | state hook     | `RangeCalendarState`     |
|  [03]   | `useDateFieldState(props)`         | state hook     | `DateFieldState`         |
|  [04]   | `useDatePickerState(props)`        | state hook     | `DatePickerState`        |
|  [05]   | `useDateRangePickerState(props)`   | state hook     | `DateRangePickerState`   |
|  [06]   | `useTimeFieldState(props)`         | state hook     | `TimeFieldState`         |
|  [07]   | `useColorAreaState(props)`         | state hook     | `ColorAreaState`         |
|  [08]   | `useColorChannelFieldState(props)` | state hook     | `ColorChannelFieldState` |
|  [09]   | `useColorFieldState(props)`        | state hook     | `ColorFieldState`        |
|  [10]   | `useColorPickerState(props)`       | state hook     | `ColorPickerState`       |
|  [11]   | `useColorSliderState(props)`       | state hook     | `ColorSliderState`       |
|  [12]   | `useColorWheelState(props)`        | state hook     | `ColorWheelState`        |
|  [13]   | `parseColor(value)`                | utility fn     | `Color`                  |
|  [14]   | `getColorChannels(colorSpace)`     | utility fn     | `[ColorChannel, ColorChannel, ColorChannel]` (ordered triple) |

[ENTRYPOINT_SCOPE]: collection primitives, data managers, table components
- rail: state

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY]  | [RETURNS / NOTE]                                          |
| :-----: | :---------------------------------------- | :-------------- | :-------------------------------------------------------- |
|  [01]   | `Item` / `Section`                        | collection nodes | JSX collection descriptors for `CollectionStateBase`     |
|  [02]   | `useCollection(props, factory, context?)` | hook            | `Collection<Node<T>>` built from JSX children            |
|  [03]   | `TableHeader` / `TableBody`               | collection comp | table row-group descriptors                              |
|  [04]   | `Column` / `Row` / `Cell`                 | collection comp | table structure descriptors feeding `useTableState`      |
|  [05]   | `useListData(options)`                    | data hook       | `ListData<T>` — local mutable list                       |
|  [06]   | `useTreeData(options)`                    | data hook       | `TreeData<T>` — local mutable tree                       |
|  [07]   | `useAsyncList(options)`                   | data hook       | `AsyncListData<T>` — pagination + server sort via `load` |

[ENTRYPOINT_SCOPE]: toast queue, form validation, virtualiser
- rail: state / layout

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [SIGNATURE / NOTE]                                                         |
| :-----: | :------------------------------ | :------------- | :------------------------------------------------------------------------- |
|  [01]   | `new ToastQueue<T>(options?)`   | constructor    | `options?: ToastStateProps` = `{ maxVisibleToasts?, wrapUpdate? }` — no `hasExitAnimation` |
|  [02]   | `ToastQueue.add(content, options?)` | method     | `(content: T, options?: ToastOptions) => string` (queue key); `ToastOptions` = `{ timeout?, onClose? }` — no `priority` |
|  [03]   | `ToastQueue.close(key)`         | method         | `(key: string) => void`                                                     |
|  [04]   | `ToastQueue.subscribe(fn)`      | method         | `(fn: () => void) => () => void` — external-store subscription             |
|  [05]   | `ToastQueue.pauseAll()` / `resumeAll()` / `clear()` | methods | timer pause/resume; drop all toasts                             |
|  [06]   | `useToastState(props?)`         | state hook     | `(props?: ToastStateProps) => ToastState<T>`                              |
|  [07]   | `useToastQueue(queue)`          | hook           | `(queue: ToastQueue<T>) => ToastState<T>` — subscribes a component to the queue |
|  [08]   | `FormValidationContext`         | context        | `Context<Record<string, string \| string[]>>` — inject server/submit errors into descendant fields |
|  [09]   | `useVirtualizerState(props)`    | state hook     | `VirtualizerState` — over a `Layout` subclass                            |

## [05]-[IMPLEMENTATION_LAW]

[STATE_TOPOLOGY]:
- one law: `use<Widget>State(props) => <Widget>State`, and that state object is passed verbatim as `state` to the matching `react-aria` `use<Widget>` hook or read from the `react-aria-components` `<Widget>StateContext` — never re-derived.
- `ListState<T>` owns `collection: Collection<Node<T>>` and `selectionManager: SelectionManager`; all selection mutation goes through `selectionManager`, and `useMultipleSelectionState` is the raw selection model the collection hooks build on.
- `SelectState<T, M>` extends `ListState<T>` and `OverlayTriggerState`; `value`/`setValue`/`selectedItems` are the v2 API — `selectedKey`/`setSelectedKey` are deprecated.
- `useListData`/`useTreeData` produce local mutable snapshots (not backed by `ListState`), feeding `items` props on `ListBox`/`ComboBox`/`Select`; a mutation yields a new snapshot via React state.
- `useAsyncList` handles pagination and server sort by re-calling `load` on `loadMore()`/`sort()`; the returned `sortDescriptor` is a `SortDescriptor`.
- layout classes (`ListLayout`/`GridLayout`/`TableLayout`/`WaterfallLayout`) are `Layout` subclasses consumed by `useVirtualizerState` and the `react-aria-components` `Virtualizer`.

[STACKING]:
- `interaction/command.md`: `UNSTABLE_useFilteredListState(state, fn)` and `useSelectState`/`ListState<T>` compose the `react-aria` `useFilter` locale-aware `contains` predicate into one scoring algebra shared by the `cmdk` palette `filter` prop and every collection surface; the state feeds the `react-aria-components` `ListStateContext`.
- `interaction/picker.md`: `parseColor` + `getColorChannels` + the `Color` interface project a pick through the `theming/tokens.md` `colorjs.io` OKLCH space; the live `useColorPickerState` `color`/`setColor` cell is the consumer's stateful component read by reference.
- `interaction/announce.md`: the `ToastQueue` class is the external store fired from any leaf/effect/fold; `useToastQueue(queue)` subscribes the `react-aria-components` `UNSTABLE_ToastRegion`/`UNSTABLE_Toast` landmark.
- `interaction/form.md`: `FormValidationContext` is the seam a `Schema.decodeUnknownEither` fold writes the `Record<string, string|string[]>` error map into, surfacing on the `react-aria-components` `Form` `validationBehavior: "aria"` fields.
- `render/dashboard.md`: the layout classes + `useVirtualizerState` drive the `@tanstack/react-virtual` health table; a `SortDescriptor` from `useAsyncList` server-sort binds the `@tanstack/react-table` column sort.

[LOCAL_ADMISSION]:
- import from the `react-stately` barrel or a deep sub-path (`react-stately/useOverlayTriggerState`).
- `OverlayTriggerState.isOpen` is read-only; mutate via `setOpen`/`open`/`close`/`toggle`.
- `ToastQueue` is a class instance (per-region or app-singleton); construct with `{ maxVisibleToasts?, wrapUpdate? }` only — `wrapUpdate(fn, action)` wraps updates (e.g. `document.startViewTransition`); `add` returns the queue key `close` dismisses.
- `UNSTABLE_useFilteredListState`/`UNSTABLE_useFilteredTableState` are preview APIs subject to change.

[RAIL_LAW]:
- package: `react-stately`
- owns: controlled/uncontrolled state for every widget family, the collection/selection/DnD models, local/async data managers, the toast external store, the form-validation seam, virtualiser layouts
- accept: props objects conforming to each widget's `*StateOptions`/`*Props` type
- reject: direct DOM manipulation or ARIA attribute assignment (owned by `react-aria`), component rendering (owned by `react-aria-components`), a re-minted selection/collection model beside `SelectionManager`/`Collection`, naming `hasExitAnimation`/toast `priority` (neither exists)
