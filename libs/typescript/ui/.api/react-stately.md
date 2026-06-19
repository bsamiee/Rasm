# [API_CATALOGUE] react-stately

`react-stately` supplies the headless state layer for every React Aria widget family. Each `use*State` hook returns a typed state object consumed by the matching `react-aria` hook or passed as `state` to `react-aria-components` context. The package also owns layout primitives (`ListLayout`, `GridLayout`, `TableLayout`, `WaterfallLayout`), collection models (`Item`, `Section`), and local data management hooks (`useListData`, `useTreeData`, `useAsyncList`) shared by the AppUi collection surfaces.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-stately`
- package: `react-stately`
- assembly: —
- namespace: `react-stately` (barrel); `react-stately/use*State` sub-paths (direct import)
- asset: runtime state library
- rail: state

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: overlay and trigger state types
- rail: state

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]   | [RAIL]                                                 |
| :-----: | :-------------------- | :-------------- | :----------------------------------------------------- |
|   [1]   | `OverlayTriggerState` | state interface | `isOpen`, `open()`, `close()`, `toggle()`, `setOpen()` |
|   [2]   | `OverlayTriggerProps` | props interface | `isOpen?`, `defaultOpen?`, `onOpenChange?`             |

[PUBLIC_TYPE_SCOPE]: toggle and selection state types
- rail: state

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]     | [RAIL]                                                               |
| :-----: | :------------------- | :---------------- | :------------------------------------------------------------------- |
|   [1]   | `ToggleState`        | state interface   | `isSelected`, `defaultSelected`, `setSelected`, `toggle`             |
|   [2]   | `ToggleStateOptions` | options interface | `isSelected?`, `defaultSelected?`, `onChange?`                       |
|   [3]   | `ToggleProps`        | props interface   | `ToggleStateOptions` + `Validation<boolean>` + `children?`, `value?` |

[PUBLIC_TYPE_SCOPE]: list and collection state types
- rail: state

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [RAIL]                                                                       |
| :-----: | :------------------ | :-------------- | :--------------------------------------------------------------------------- |
|   [1]   | `ListState<T>`      | state interface | `collection`, `disabledKeys`, `selectionManager`                             |
|   [2]   | `ListProps<T>`      | props interface | `CollectionStateBase<T>`, `filter?`, `layoutDelegate?`                       |
|   [3]   | `SelectState<T, M>` | state interface | `ListState<T>` + `OverlayTriggerState`, `value`, `setValue`, `selectedItems` |
|   [4]   | `SelectProps<T, M>` | props interface | `CollectionBase<T>`, `ValueBase`, selection mode, open state                 |

[PUBLIC_TYPE_SCOPE]: data management types
- rail: state

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [RAIL]                                                |
| :-----: | :-------------------- | :---------------- | :---------------------------------------------------- |
|   [1]   | `ListData<T>`         | result interface  | `items`, `selectedKeys`, CRUD operations, filter      |
|   [2]   | `ListOptions<T>`      | options interface | `initialItems?`, `getKey?`, `filter?`                 |
|   [3]   | `TreeData<T>`         | result interface  | hierarchical equivalent of `ListData<T>`              |
|   [4]   | `AsyncListData<T>`    | result interface  | `items`, `loadingState`, `loadMore`, `sort`, `reload` |
|   [5]   | `AsyncListOptions<T>` | options interface | `load`, `initialSortDescriptor?`, `getKey?`           |

[PUBLIC_TYPE_SCOPE]: layout types
- rail: layout

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                              |
| :-----: | :---------------- | :------------ | :---------------------------------- |
|   [1]   | `ListLayout`      | class         | single-axis list virtualiser layout |
|   [2]   | `GridLayout`      | class         | two-axis grid virtualiser layout    |
|   [3]   | `TableLayout`     | class         | table virtualiser layout            |
|   [4]   | `WaterfallLayout` | class         | variable-height waterfall layout    |
|   [5]   | `Layout`          | base class    | abstract layout base                |
|   [6]   | `LayoutInfo`      | class         | single item layout descriptor       |
|   [7]   | `Rect`            | class         | bounding rectangle                  |
|   [8]   | `Size`            | class         | width × height value                |
|   [9]   | `Point`           | class         | x, y coordinate value               |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: overlay state hooks
- rail: state

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [RETURNS]             |
| :-----: | :------------------------------ | :------------- | :-------------------- |
|   [1]   | `useOverlayTriggerState(props)` | state hook     | `OverlayTriggerState` |
|   [2]   | `useMenuTriggerState(props)`    | state hook     | `MenuTriggerState`    |
|   [3]   | `useTooltipTriggerState(props)` | state hook     | `TooltipTriggerState` |

[ENTRYPOINT_SCOPE]: form control state hooks
- rail: state

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RETURNS]            |
| :-----: | :----------------------------- | :------------- | :------------------- |
|   [1]   | `useToggleState(props?)`       | state hook     | `ToggleState`        |
|   [2]   | `useToggleGroupState(props)`   | state hook     | `ToggleGroupState`   |
|   [3]   | `useCheckboxGroupState(props)` | state hook     | `CheckboxGroupState` |
|   [4]   | `useRadioGroupState(props)`    | state hook     | `RadioGroupState`    |
|   [5]   | `useSelectState(props)`        | state hook     | `SelectState<T, M>`  |
|   [6]   | `useComboBoxState(props)`      | state hook     | `ComboBoxState<T>`   |
|   [7]   | `useNumberFieldState(props)`   | state hook     | `NumberFieldState`   |
|   [8]   | `useSearchFieldState(props)`   | state hook     | `SearchFieldState`   |
|   [9]   | `useSliderState(props)`        | state hook     | `SliderState`        |

[ENTRYPOINT_SCOPE]: collection and list state hooks
- rail: state

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [RETURNS]                     |
| :-----: | :------------------------------------------ | :------------- | :---------------------------- |
|   [1]   | `useListState(props)`                       | state hook     | `ListState<T>`                |
|   [2]   | `useSingleSelectListState(props)`           | state hook     | `SingleSelectListState<T>`    |
|   [3]   | `UNSTABLE_useFilteredListState(state, fn)`  | state hook     | filtered `ListState<T>` view  |
|   [4]   | `useTableState(props)`                      | state hook     | `TableState<T>`               |
|   [5]   | `UNSTABLE_useFilteredTableState(state, fn)` | state hook     | filtered `TableState<T>` view |
|   [6]   | `useTabListState(props)`                    | state hook     | `TabListState<T>`             |
|   [7]   | `useTreeState(props)`                       | state hook     | `TreeState<T>`                |
|   [8]   | `useDisclosureState(props?)`                | state hook     | `DisclosureState`             |
|   [9]   | `useDisclosureGroupState(props?)`           | state hook     | `DisclosureGroupState`        |

[ENTRYPOINT_SCOPE]: calendar and date state hooks
- rail: date-time

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [RETURNS]              |
| :-----: | :------------------------------- | :------------- | :--------------------- |
|   [1]   | `useCalendarState(props)`        | state hook     | `CalendarState`        |
|   [2]   | `useRangeCalendarState(props)`   | state hook     | `RangeCalendarState`   |
|   [3]   | `useDateFieldState(props)`       | state hook     | `DateFieldState`       |
|   [4]   | `useDatePickerState(props)`      | state hook     | `DatePickerState`      |
|   [5]   | `useDateRangePickerState(props)` | state hook     | `DateRangePickerState` |
|   [6]   | `useTimeFieldState(props)`       | state hook     | `TimeFieldState`       |

[ENTRYPOINT_SCOPE]: colour state hooks
- rail: color

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RETURNS]                |
| :-----: | :--------------------------------- | :------------- | :----------------------- |
|   [1]   | `useColorAreaState(props)`         | state hook     | `ColorAreaState`         |
|   [2]   | `useColorChannelFieldState(props)` | state hook     | `ColorChannelFieldState` |
|   [3]   | `useColorFieldState(props)`        | state hook     | `ColorFieldState`        |
|   [4]   | `useColorPickerState(props)`       | state hook     | `ColorPickerState`       |
|   [5]   | `useColorSliderState(props)`       | state hook     | `ColorSliderState`       |
|   [6]   | `useColorWheelState(props)`        | state hook     | `ColorWheelState`        |
|   [7]   | `parseColor(value)`                | utility fn     | `Color`                  |
|   [8]   | `getColorChannels(colorSpace)`     | utility fn     | `ColorChannel[]`         |

[ENTRYPOINT_SCOPE]: data management hooks
- rail: state

| [INDEX] | [SURFACE]               | [ENTRY_FAMILY] | [RETURNS]          |
| :-----: | :---------------------- | :------------- | :----------------- |
|   [1]   | `useListData(options)`  | data hook      | `ListData<T>`      |
|   [2]   | `useTreeData(options)`  | data hook      | `TreeData<T>`      |
|   [3]   | `useAsyncList(options)` | data hook      | `AsyncListData<T>` |

[ENTRYPOINT_SCOPE]: collection primitives and toast queue
- rail: state

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY]  | [RETURNS]                                                                                                     |
| :-----: | :---------------------------------------- | :-------------- | :------------------------------------------------------------------------------------------------------------ |
|   [1]   | `Item`                                    | collection node | item descriptor for `CollectionStateBase` consumers                                                           |
|   [2]   | `Section`                                 | collection node | section descriptor                                                                                            |
|   [3]   | `useCollection(props, factory, context?)` | hook            | `Collection<Node<T>>`                                                                                         |
|   [4]   | `useToastState(props?)`                   | state hook      | `ToastState<T>`                                                                                               |
|   [5]   | `ToastQueue`                              | class           | global singleton toast queue                                                                                  |
|   [6]   | `new ToastQueue<T>(options?)`             | constructor     | `{ maxVisibleToasts?: number; hasExitAnimation?: boolean; wrapUpdate?: (fn) => void }`                        |
|   [7]   | `ToastQueue.add(content, options?)`       | method          | `(content: T, options?: { timeout?: number; onClose?: () => void; priority?: number }) => string` (queue key) |
|   [8]   | `ToastQueue.close(key)`                   | method          | `(key: string) => void` — dismiss a queued toast by key                                                       |
|   [9]   | `useToastQueue(queue)`                    | hook            | subscribes to a `ToastQueue`                                                                                  |
|  [10]   | `useVirtualizerState(props)`              | state hook      | `VirtualizerState`                                                                                            |

## [4]-[IMPLEMENTATION_LAW]

[STATE_TOPOLOGY]:
- every `use*State` hook returns a state object that is passed verbatim as the `state` argument to the matching `react-aria` hook
- `ListState<T>` owns `collection: Collection<Node<T>>` and `selectionManager: SelectionManager`; all mutation goes through `selectionManager`
- `SelectState<T, M>` extends `ListState<T>` and `OverlayTriggerState`; `value` / `setValue` are the v2 multi-select API; `selectedKey` / `setSelectedKey` are deprecated
- `useListData` / `useTreeData` produce local mutable lists that are not backed by `ListState`; they feed items props on `ListBox` / `ComboBox` / `Select`
- `useAsyncList` handles pagination and server-side sort by calling `load` again on `loadMore()` or `sort()`
- layout classes (`ListLayout`, `GridLayout`, etc.) are virtualiser adapters consumed by `useVirtualizerState` and the `Virtualizer` component in `react-aria-components`

[LOCAL_ADMISSION]:
- import state hooks from the `react-stately` barrel or deep paths (e.g. `react-stately/useOverlayTriggerState`)
- `OverlayTriggerState.isOpen` is read-only; `setOpen`, `open`, `close`, `toggle` are the mutation surface
- `ListData<T>.items` is the snapshot array; mutations produce a new snapshot via React state
- `ToastQueue` is a class instance (singleton or per-component); `useToastQueue(queue)` subscribes the component to its changes
- `UNSTABLE_useFilteredListState` and `UNSTABLE_useFilteredTableState` are preview APIs and subject to change

[RAIL_LAW]:
- package: `react-stately`
- owns: controlled and uncontrolled state for every widget family, collection and data models, layout primitives
- accept: props objects conforming to the widget's `*StateOptions` type
- reject: direct DOM manipulation, ARIA attribute assignment (owned by `react-aria`), component rendering (owned by `react-aria-components`)
