# [TS_UI_API_REACT_STATELY]

`react-stately` is the framework-agnostic state layer beneath the react-aria spine — the meta-package re-exporting every `@react-stately/*` hook. Its shape is one parameterized pattern, not a heap of unrelated hooks: for each ARIA design pattern P there is a `use<P>State(props) → <P>State` hook that owns *only* P's interaction state (which item is focused/selected, whether an overlay is open, the segments of a date being edited, a slider's thumb positions), and the matching react-aria `use<P>` behavior hook (`.api/react-aria.md`) and react-aria-components `<P>` component (`.api/react-aria-components.md`) consume that state object. Every collection-backed pattern folds over the same substrate — `Collection<Node<T>>` (the normalized item tree) and `MultipleSelectionManager`/`MultipleSelectionState` (the selection model, exposed as the `.selectionManager` property) — so selection, focus, and disabled semantics are defined once and reused across list/tree/table/menu/combobox. The state it holds is *ephemeral and interaction-local*; domain/app state is the `@effect-atom` fold (`.api/effect-atom-atom-react.md`), and the seam between them is the mutable data hooks (`useListData`/`useAsyncList`/`useTreeData`), whose `load`/mutation surface is where an Effect runtime feeds a dynamic collection. Because react-aria-components bundles state+behavior+DOM, this package is used *directly* only when composing a custom primitive below RAC or driving a dynamic collection; the standalone hooks are the escape hatch, not the default.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-stately`
- package: `react-stately` (umbrella re-export of the `@react-stately/*` scope)
- license: `Apache-2.0` (© Adobe)
- module format: ESM/CJS dual (`import`/`require` conditions) with a per-hook `./*` subpath mirroring each `@react-stately/*` module; `sideEffects` clean, so the barrel tree-shakes to the used hooks
- runtime target: isomorphic (renderer-agnostic state — no DOM); pairs with react-aria for behavior and a renderer for DOM; peer `react`/`react-dom` `^19`
- asset: TypeScript library shipping `.d.ts`; the `<P>State`/`<P>Props`/`<P>StateOptions` type families are the load-bearing contract the behavior hooks type against, so `tsc`/`tsgo` is the gate
- peer surface: calendar/date-field/date-picker state consume `@internationalized/date` values (`DateValue`/`CalendarDate`, `DateValue` re-exported here) — a transitive dependency, latent until a date `view` lands
- rail: the `view` headless plane — `view/primitive` composes a state hook + its react-aria behavior hook; `view/compose` drives dynamic collections through the data hooks
- not-Effect: react-stately is React-hook-native; the `Effect` rail reaches it only through `useAsyncList.load` (or an atom-fed `useListData`), never inside the state hook

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the shared collection + selection substrate every pattern folds over
- rail: shapes
- These are defined once and reused across every collection-backed pattern — a `use<P>State` result exposes a selection manager (`MultipleSelectionManager`, the `.selectionManager` property) over a `Collection<Node<T>>`, so selection/focus/disabled semantics never fork per widget. `Node` carries `key`/`value`/`rendered`/`childNodes`.

| [INDEX] | [SYMBOL]                                                         | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                          |
| :-----: | :--------------------------------------------------------------- | :--------------- | :------------------------------------------- |
|  [01]   | `Collection<T>` / `Node<T>` / `Key`                              | item tree        | the normalized item tree every state exposes |
|  [02]   | `MultipleSelectionManager` (`state.selectionManager`)            | selection engine | imperative select/toggle/anchor interface    |
|  [03]   | `MultipleSelectionState` / `SingleSelectionState` / `FocusState` | selection state  | reactive selection/focus cells               |
|  [04]   | `SelectionMode`                                                  | selection vocab  | `"single"`/`"multiple"`/`"none"`             |
|  [05]   | `SelectionBehavior`                                              | selection vocab  | `"toggle"`/`"replace"`                       |
|  [06]   | `DisabledBehavior`                                               | selection vocab  | disabled `"selection"`/`"all"`               |
|  [07]   | `Selection`                                                      | selection vocab  | a `Set`-of-`Key`, or all                     |
|  [08]   | `SortDescriptor` / `Orientation`                                 | collection axis  | table sort direction, list/tab orientation   |

[PUBLIC_TYPE_SCOPE]: the per-pattern `<P>State` / `<P>Props` / `<P>StateOptions` families
- rail: shapes
- One triple per ARIA pattern: `<P>Props` is the caller input, `<P>StateOptions` the constructor input, `<P>State` the returned handle. They are a family indexed by pattern, not parallel designs — the table names the axes, not every member.

| [INDEX] | [SYMBOL_FAMILY]                                                       | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                          |
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

[ENTRYPOINT_SCOPE]: mutable client collection data — the Effect-plane bridge
- rail: surfaces-and-dispatch
- The one place react-stately touches app data: `useListData({ initialItems, getKey, initialSelectedKeys, filter })` and `useTreeData({ initialItems, getKey, getChildren })` hold a mutable collection with `insert`/`remove`/`move`/`update`/`append`/`prepend` fold operators; `useAsyncList({ load, sort, getKey, initialSelectedKeys })` adds paginated/sorted async loading where `load({ signal, cursor, filterText, sortDescriptor })` is the Effect-run boundary — never domain state re-modelled inside the hook.

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                   |
| :-----: | :------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `useListData(…)` → `ListData<T>`       | list store     | editable list/table; `.items`/`.selectedKeys` + folds |
|  [02]   | `useAsyncList(…)` → `AsyncListData<T>` | async store    | server-fed; `.loadingState`/`.reload`/`.loadMore`     |
|  [03]   | `useTreeData(…)` → `TreeData<T>`       | tree store     | hierarchical (BIM spatial tree); parent/child fold    |

[ENTRYPOINT_SCOPE]: collection builders and collection-backed state
- rail: surfaces-and-dispatch
- `Item`/`Section`/`useCollection` are the low-level collection builders (RAC ships its own JSX collection components over the same substrate); the `use*State` hooks build the reactive `Collection` + `SelectionManager` a behavior hook drives. `UNSTABLE_useFiltered*` layer a filter predicate over an existing collection (command-palette).

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY]   | [CONSUMER_BOUNDARY]                             |
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

[ENTRYPOINT_SCOPE]: field/value, overlay-trigger, and the specialized state hooks
- rail: surfaces-and-dispatch
- The rest of the family: value-owning field state, overlay open/close state, and the date/color/toast/dnd/virtualizer specializations. Each is `use<P>State(props) → <P>State` consumed by the matching react-aria behavior hook; the folder reaches them through RAC unless composing a custom primitive. `floating-ui` positions overlay surfaces; date state operates on `@internationalized/date` values with `DateSegment` editing (`intl/format` owns display); color state derives from `parseColor`/`getColorChannels` and pairs `colorjs.io` (`.api/colorjs.io.md`); `FormValidationContext` (built by `@react-stately/form`'s internal `useFormValidationState`) injects `Schema`→aria `FormBinding` errors.

| [INDEX] | [SURFACE_FAMILY]                                                      | [ENTRY_FAMILY]  | [CONSUMER_BOUNDARY]                     |
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

[STATELY_TOPOLOGY]:
- one pattern, one state hook: every ARIA pattern P is a `use<P>State(props) → <P>State` hook that owns *only* P's interaction state; there is no god-object and no per-P reinvention of selection — the table lists axes (`ListState`/`TableState`/`ComboBoxState`/…), and a new pattern is a new hook over the same substrate, never a fork of the selection engine.
- the collection + selection substrate is shared: collection-backed patterns expose a `Collection<Node<T>>` and a selection manager (`MultipleSelectionManager`, the `.selectionManager` property), so focus, selection mode/behavior, disabled behavior, and keyboard navigation are defined once in `@react-stately/selection`/`@react-stately/collections` and reused. A row references `SelectionMode`/`SelectionBehavior`/`DisabledBehavior` by value; it never re-encodes the semantics.
- state is ephemeral and interaction-local: a `<P>State` holds what the widget needs *this interaction* — the open overlay, the focused key, the uncommitted date segments — not domain data. Domain/app state is the `@effect-atom` fold; the only data seam is `useListData`/`useAsyncList`/`useTreeData`, whose mutation/`load` surface is where app data enters and leaves.
- RAC bundles state + behavior + DOM: react-aria-components composes each `use<P>State` with its `use<P>` behavior hook and a DOM element, and surfaces this package's state back to descendants through an `XxxStateContext` triple — `ListStateContext`/`TableStateContext`/`OverlayTriggerStateContext`/`SelectStateContext`/`TabListStateContext`/`TooltipTriggerStateContext`/`RootMenuTriggerStateContext` each expose the live `<P>State` a compound child reads instead of prop-drilling — and re-exports the mutable data hooks (`useListData`/`useTreeData`/`useAsyncList`). So the folder's default is the RAC component; standalone react-stately is the escape hatch — a custom primitive below RAC, a dynamic collection driven by the data hooks, or a standalone selection model / `ToastQueue`.

[INTEGRATION_LAW]:
- Stack with `react-aria` + `react-aria-components` (`.api/react-aria.md`, `.api/react-aria-components.md`): the state hook and the behavior hook are two halves of one pattern — the `use<P>State(props) → <P>State` machine is the first-class `state` argument to react-aria's `use<P>(props, state, ref)`. Selection (`useMultipleSelectionState`), collections (`useListState`/`useTableState`/`useTreeState`), and overlay/trigger (`useOverlayTriggerState`/`useMenuTriggerState`) states thread in directly: `const state = useListState(props); const { listBoxProps } = useListBox(props, state, ref)`. Field validity is the exception to the threaded-state shape — a react-aria field hook is `use<P>(props, ref)` and builds validity internally via `@react-stately/form`'s `useFormValidationState`, reading `FormValidationContext` for injected server/schema errors. RAC fuses both halves; a `view/primitive` row composes the pair only when authoring a primitive RAC does not ship.
- Stack with `@effect-atom` (`.api/effect-atom-atom-react.md`): domain state lives in an atom; `useAsyncList.load` runs an `Effect` (through the atom runtime or a direct `runPromise` at the boundary) to fetch a page, and an atom-derived array feeds `useListData({ initialItems })`. react-stately never becomes a second store — it holds interaction state, the atom holds truth.
- Stack with `@tanstack/react-virtual` (`.api/tanstack-react-virtual.md`) vs the built-in `useVirtualizerState`: `view/compose` picks per collection — the built-in virtualizer integrates the selection substrate natively; TanStack virtual is the row for a non-collection virtualized surface. One is chosen per row, never both on the same list.
- Stack with `Schema` validation (`.api/effect.md`): `FormValidationContext` is the sink for realtime + server validation — a `Schema` decode failure at the `FormBinding` seam projects into the context, and field state (`NumberFieldState`/`ComboBoxState`/…) reads it to surface `invalid`/`required` state, which the tw-rac `invalid:`/`required:` variants style.
- Stack with `@internationalized/date` + `colorjs.io`: calendar/date state operates on `@internationalized/date` values (`DateValue`/`CalendarDate`), and color state on `parseColor`'s `Color`; both are latent capability the folder wires only when a date/color `view` lands, with display owned by `intl/format` and `colorjs.io` respectively.

[LOCAL_ADMISSION]:
- Reach react-stately through react-aria-components by default; drop to a standalone `use<P>State` only to compose a primitive RAC does not ship, or to a standalone selection model / `ToastQueue`.
- Hold only interaction-local state in a `<P>State`; never mirror domain/app data into react-stately — that is the `@effect-atom` fold, bridged by `useListData`/`useAsyncList`/`useTreeData`.
- Drive dynamic/server collections through `useAsyncList.load` (the Effect-run boundary) or an atom-fed `useListData`; never re-fetch inside a `useEffect`+`useState` pair or re-model the async lifecycle by hand.
- Reference `SelectionMode`/`SelectionBehavior`/`DisabledBehavior`/`SortDescriptor` by value and let the shared selection manager (`.selectionManager`) own selection; never re-implement selection/focus/keyboard logic per widget.
- Feed validation through `FormValidationContext` from the `Schema` `FormBinding`; never thread field errors as ad-hoc props.

[RAIL_LAW]:
- Package: `react-stately`
- Owns: the framework-agnostic interaction-state layer — the shared `Collection`/`Node`/`MultipleSelectionManager` substrate, the mutable client data hooks (`useListData`/`useAsyncList`/`useTreeData`), the collection builders (`Item`/`Section`/`useCollection`), and the `use<P>State` family across list/tree/table/menu/combobox/select/tabs, toggle/checkbox/radio/number/search/slider fields, overlay/tooltip/disclosure triggers, calendar/date/time, color, toast (`ToastQueue`), dnd, virtualizer/layout, and `FormValidationContext`
- Accept: RAC as the default consumer, standalone `use<P>State` for custom primitives, interaction-local state only, `useAsyncList`/`useListData`/`useTreeData` as the Effect-plane data bridge, value-referenced selection vocabulary over the shared `SelectionManager`
- Reject: react-stately as a domain store, domain state mirrored into a `<P>State`, hand-rolled selection/focus/collection logic, a manual async-fetch lifecycle in place of `useAsyncList`, and both the built-in virtualizer and TanStack virtual on the same collection
