# [TS_UI_API_REACT_STATELY]

`react-stately` owns the framework-agnostic interaction-state layer beneath react-aria: one `use<P>State(props) -> <P>State` hook per ARIA pattern holds only that pattern's ephemeral interaction state over one shared `Collection<Node<T>>` + `MultipleSelectionManager` substrate, so selection, focus, and disabled semantics never fork per widget.

Domain state is the `@effect-atom` fold reached only through the mutable data hooks; react-aria-components is the default consumer, a standalone hook the escape hatch.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-stately`
- package: `react-stately` (Apache-2.0)
- module: ESM/CJS dual with a per-hook `./*` subpath mirroring each `@react-stately/*` module; `sideEffects`-clean, so the barrel tree-shakes to the used hooks
- runtime: isomorphic — renderer-agnostic state, no DOM; pairs with react-aria for behavior and a renderer for DOM
- rail: the `view` headless plane — `view/primitive` composes a state hook with its react-aria behavior hook, `view/compose` drives dynamic collections through the data hooks

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the shared collection + selection substrate every collection-backed pattern folds over, defined once and reused across list/tree/table/menu/combobox

| [INDEX] | [SYMBOL]                                                         | [TYPE_FAMILY]    | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------- | :--------------- | :------------------------------------------- |
|  [01]   | `Collection<T>` / `Node<T>` / `Key`                              | item tree        | the normalized item tree every state exposes |
|  [02]   | `MultipleSelectionManager` (`state.selectionManager`)            | selection engine | imperative select/toggle/anchor interface    |
|  [03]   | `MultipleSelectionState` / `SingleSelectionState` / `FocusState` | selection state  | reactive selection/focus cells               |
|  [04]   | `SelectionMode`                                                  | selection vocab  | `"single"`/`"multiple"`/`"none"`             |
|  [05]   | `SelectionBehavior`                                              | selection vocab  | `"toggle"`/`"replace"`                       |
|  [06]   | `DisabledBehavior`                                               | selection vocab  | disabled `"selection"`/`"all"`               |
|  [07]   | `Selection`                                                      | selection vocab  | a `Set`-of-`Key`, or all                     |
|  [08]   | `SortDescriptor` / `Orientation`                                 | collection axis  | table sort direction, list/tab orientation   |

[PUBLIC_TYPE_SCOPE]: the per-pattern `<P>Props` (caller input) / `<P>StateOptions` (constructor input) / `<P>State` (returned handle) triple, one family indexed by ARIA pattern

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY]     | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------------- | :---------------- | :------------------------------------------- |
|  [01]   | `ListState` / `TreeState` / `TableState` / `SingleSelectListState`    | collection state  | list/tree/table/virtual collection rows      |
|  [02]   | `ComboBoxState` / `SelectState` / `MenuTriggerState` / `TabListState` | picker/menu state | collection + selection + open state          |
|  [03]   | `ToggleState` / `CheckboxGroupState` / `RadioGroupState`              | boolean field     | form toggles; group owns value + validation  |
|  [04]   | `NumberFieldState` / `SearchFieldState` / `SliderState`               | value field       | committed value + `Schema` validation        |
|  [05]   | `OverlayTriggerState` / `TooltipTriggerState`                         | overlay state     | popover/tooltip; `floating-ui` positions     |
|  [06]   | `DisclosureState` / `DisclosureGroupState`                            | overlay state     | accordion open-close                         |
|  [07]   | `CalendarState` / `RangeCalendarState` / `DatePickerState`            | date/time state   | latent date-picker `view` rows               |
|  [08]   | `DateFieldState` / `TimeFieldState`                                   | date/time state   | segment editing (`DateSegment`)              |
|  [09]   | `ColorPickerState` / `ColorAreaState` / `ColorFieldState`             | color state       | color-picker `view` rows                     |
|  [10]   | `ColorSliderState` / `ColorWheelState`                                | color state       | `parseColor` types; `colorjs.io` space math  |
|  [11]   | `ToastState<T>` / `QueuedToast<T>` / `ToastStateProps`                | toast queue       | toast; queue backs the live-region announce  |
|  [12]   | `ListData<T>` / `AsyncListData<T>` / `TreeData<T>`                    | data store        | client collection rows mutate; Effect bridge |
|  [13]   | `AsyncListLoadOptions`                                                | load options      | `useAsyncList` load params                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the mutable client-collection data hooks — react-stately's one seam to app data

| [INDEX] | [SURFACE]                              | [SHAPE]     | [CAPABILITY]                                          |
| :-----: | :------------------------------------- | :---------- | :---------------------------------------------------- |
|  [01]   | `useListData(…)` → `ListData<T>`       | list store  | editable list/table; `.items`/`.selectedKeys` + folds |
|  [02]   | `useAsyncList(…)` → `AsyncListData<T>` | async store | server-fed; `.loadingState`/`.reload`/`.loadMore`     |
|  [03]   | `useTreeData(…)` → `TreeData<T>`       | tree store  | hierarchical (BIM spatial tree); parent/child fold    |

- `useListData`/`useTreeData` → `ListData`/`TreeData`: `insert`/`remove`/`move`/`update`/`append`/`prepend` folds over `.items`/`.selectedKeys`; `useAsyncList.load({ signal, cursor, filterText, sortDescriptor })` is the Effect-run boundary, never a domain lifecycle re-modelled inside the hook.

[ENTRYPOINT_SCOPE]: collection builders and collection-backed `use<P>State`; `UNSTABLE_useFiltered*` layers a filter predicate over a built collection

| [INDEX] | [SURFACE]                                                      | [SHAPE]          | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `Item` / `Section` / `useCollection(props, factory, context?)` | collection build | static/dynamic collection authoring             |
|  [02]   | `useListState(props)` / `useSingleSelectListState(props)`      | list state       | single-select `.collection`+`.selectionManager` |
|  [03]   | `useTreeState(props)`                                          | tree state       | a tree's `.collection`+`.selectionManager`      |
|  [04]   | `useTableState(props)`                                         | table state      | column collection, sort, row selection          |
|  [05]   | `useTableColumnResizeState(props, tableState)`                 | table state      | column resize over a table state                |
|  [06]   | `TableHeader` / `TableBody` / `Column` / `Row` / `Cell`        | table build      | the table collection elements                   |
|  [07]   | `UNSTABLE_useFilteredListState(state, filterFn)`               | filtered view    | type-ahead filter over a built list             |
|  [08]   | `UNSTABLE_useFilteredTableState(...)`                          | filtered view    | the filtered-table variant                      |
|  [09]   | `useMultipleSelectionState(props)` → `MultipleSelectionState`  | selection state  | standalone selection model below RAC            |

[ENTRYPOINT_SCOPE]: field/value, trigger, and the date/color/toast/dnd/virtualizer state hooks — each `use<P>State(props) -> <P>State` paired with a react-aria behavior hook

| [INDEX] | [SURFACE]                                                             | [SHAPE]         | [CAPABILITY]                            |
| :-----: | :-------------------------------------------------------------------- | :-------------- | :-------------------------------------- |
|  [01]   | `useToggleState` / `useToggleGroupState`                              | boolean field   | switch/toggle-group boolean value       |
|  [02]   | `useCheckboxGroupState` / `useRadioGroupState`                        | group field     | shared value + validation               |
|  [03]   | `useNumberFieldState` / `useSearchFieldState` / `useSliderState`      | value field     | numeric/search/slider committed value   |
|  [04]   | `useSelectState` / `useComboBoxState`                                 | value field     | select/combobox committed value         |
|  [05]   | `useOverlayTriggerState` / `useMenuTriggerState`                      | open state      | popover/menu open-close                 |
|  [06]   | `useSubmenuTriggerState` / `useTooltipTriggerState`                   | open state      | submenu/tooltip open-close              |
|  [07]   | `useDisclosureState` / `useDisclosureGroupState` / `useTabListState`  | open state      | accordion/tabs open-close               |
|  [08]   | `useCalendarState` / `useRangeCalendarState` / `useDatePickerState`   | date state      | latent calendar/date-picker rows        |
|  [09]   | `useDateRangePickerState` / `useDateFieldState` / `useTimeFieldState` | date state      | range + segment editing                 |
|  [10]   | `parseColor` / `getColorChannels`                                     | color util      | `parseColor(str) → Color`; channel list |
|  [11]   | `useColorPickerState` / `useColorAreaState`                           | color state     | picker/area color state                 |
|  [12]   | `useColorFieldState` / `useColorChannelFieldState`                    | color state     | field/channel color state               |
|  [13]   | `useColorSliderState` / `useColorWheelState`                          | color state     | slider/wheel color state                |
|  [14]   | `useToastState` / `useToastQueue` / `ToastQueue`                      | toast queue     | standalone imperative queue → announce  |
|  [15]   | `useDraggableCollectionState` / `useDroppableCollectionState`         | dnd state       | drag-drop reorder over a collection     |
|  [16]   | `useVirtualizerState`                                                 | virtualizer     | built-in; or `@tanstack/react-virtual`  |
|  [17]   | `Layout` / `ListLayout` / `GridLayout`                                | layout          | base + list/grid layout strategies      |
|  [18]   | `TableLayout` / `WaterfallLayout`                                     | layout          | table + waterfall layout strategies     |
|  [19]   | `Rect` / `Size` / `Point` / `LayoutInfo` / `ReusableView`             | layout geometry | virtualizer geometry + recycled view    |
|  [20]   | `FormValidationContext`                                               | validation ctx  | `Schema`→aria `FormBinding` error sink  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `use<P>State` owns one ARIA pattern's interaction state and nothing else; a new pattern is a new hook over the shared substrate, never a fork of the selection engine or a god-object.
- Collection-backed patterns expose one `Collection<Node<T>>` and one `MultipleSelectionManager` (the `.selectionManager` property), so focus, selection mode/behavior, disabled behavior, and keyboard navigation resolve once in the shared selection/collection modules and a row references `SelectionMode`/`SelectionBehavior`/`DisabledBehavior` by value.
- `<P>State` holds only interaction-local data — the open overlay, the focused key, the uncommitted date segments — and defers domain state to the `@effect-atom` fold reached through the `useListData`/`useAsyncList`/`useTreeData` seam.
- `react-aria-components` composes each `use<P>State` with its `use<P>` behavior hook and a DOM element and re-surfaces the live state to descendants through per-pattern state-context providers; standalone react-stately is the escape hatch — a custom primitive below RAC, a data-hook-driven dynamic collection, or a standalone selection model / `ToastQueue`.

[STACKING]:
- `react-aria`(`.api/react-aria.md`): the state hook is the first-class `state` argument to `use<P>(props, state, ref)` — `const state = useListState(props); const { listBoxProps } = useListBox(props, state, ref)`; field validity is the exception, built internally by `@react-stately/form`'s `useFormValidationState` reading `FormValidationContext`.
- `react-aria-components`(`.api/react-aria-components.md`): fuses the state hook and behavior hook into ready components and re-exports the data hooks; a `view/primitive` row composes the raw `use<P>State`+`use<P>` pair only when authoring a primitive RAC does not ship.
- `@effect-atom`(`.api/effect-atom-atom-react.md`): domain state lives in an atom, `useAsyncList.load` runs an `Effect` to fetch a page, and an atom-derived array feeds `useListData({ initialItems })` — react-stately never becomes a second store.
- `effect` `Schema`(`libs/typescript/.api/effect.md`): a `Schema` decode failure at the `FormBinding` seam projects into `FormValidationContext`, and field state (`NumberFieldState`/`ComboBoxState`/…) reads it to surface `invalid`/`required` for the tw-rac `invalid:`/`required:` variants.
- `@tanstack/react-virtual`(`.api/tanstack-react-virtual.md`): `view/compose` picks per collection — the built-in `useVirtualizerState` integrates the selection substrate natively, TanStack virtual owns a non-collection virtualized surface; one per list, never both.
- `@internationalized/date` + `colorjs.io`(`.api/colorjs.io.md`): calendar/date state operates on `DateValue`/`CalendarDate` and color state on `parseColor`'s `Color`, latent until a date/color `view` lands, display owned by `intl/format` and `colorjs.io`.
- within-lib: `view/primitive` composes a state hook with its behavior hook; `view/compose` drives a dynamic collection through the data hooks and the built-in virtualizer.

[LOCAL_ADMISSION]:
- Reach react-stately through react-aria-components by default; drop to a standalone `use<P>State` only for a custom primitive RAC does not ship or a standalone selection model / `ToastQueue`.
- Hold only interaction-local state in a `<P>State`; domain data is the `@effect-atom` fold, bridged by `useListData`/`useAsyncList`/`useTreeData`.
- Drive dynamic/server collections through `useAsyncList.load` (the Effect-run boundary) or an atom-fed `useListData`, never a hand-rolled `useEffect`+`useState` fetch lifecycle.
- Reference `SelectionMode`/`SelectionBehavior`/`DisabledBehavior`/`SortDescriptor` by value and let the shared `.selectionManager` own selection; feed field validity through `FormValidationContext` from the `Schema` `FormBinding`.

[RAIL_LAW]:
- Package: `react-stately`
- Owns: the framework-agnostic interaction-state layer — the shared `Collection`/`Node`/`MultipleSelectionManager` substrate, the mutable client data hooks, the collection builders (`Item`/`Section`/`useCollection`), and one `use<P>State` hook per ARIA pattern across collections, fields, triggers, date/color/toast, dnd, and virtualizer/layout
- Accept: react-aria-components as the default consumer, standalone `use<P>State` for custom primitives, interaction-local state only, `useAsyncList`/`useListData`/`useTreeData` as the Effect-plane data bridge, value-referenced selection vocabulary over the shared `SelectionManager`
- Reject: react-stately as a domain store, domain state mirrored into a `<P>State`, hand-rolled selection/focus/collection logic, a manual async-fetch lifecycle replacing `useAsyncList`, and both the built-in virtualizer and TanStack virtual on one collection
