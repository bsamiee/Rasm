# [API_CATALOGUE] react-aria-components

`react-aria-components` (RAC) is the styled-ready component layer that pre-composes the `react-aria` behavior hooks and `react-stately` state hooks under one uniform contract. Every component is a `ForwardRefExoticComponent<XProps & RefAttributes<XElement>>` whose props extend the `react-aria` Aria props plus a render-prop styling surface (`className`/`style`/`children` each accept a value OR a `(state: XRenderProps) => value` function) plus a `slot` prop; every component ships an `XContext` for prop injection, and stateful widgets ship an `XStateContext` exposing the raw `react-stately` state for custom subcomponents. Every root DOM node carries `data-rac` and `data-*` interaction-state attributes, which are the exact target of `tailwindcss-react-aria-components`. This is the primary AppUi component surface: form controls, overlays, collections, the full colour-picker family, date/calendar, drag-and-drop, tree/table, toasts, and view transitions arrive from one barrel import.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-aria-components`
- package: `react-aria-components`
- version: `1.19.0`
- license: `Apache-2.0`
- module: `react-aria-components` (barrel); `react-aria-components/i18n` (locale sub-path)
- namespace: ~180 component/context/hook value exports + ~200 `*Props`/`*RenderProps`/state type exports
- asset: dual CJS/ESM (`dist/exports/index.cjs` / `.js`), `sideEffects: ['*.css']`; the barrel opens with `import 'client-only'`
- runtime: client React ONLY (peer `react ^19`, `react-dom ^19`); a server import throws via the `client-only` guard
- contract: `<X>` component + `<X>Props` (Aria props + `RenderProps`/`StyleRenderProps` + `SlotProps`) + `<X>RenderProps` state + `<X>Context` + (stateful) `<X>StateContext`; every root emits `data-rac` + `data-*` state attrs
- rail: ui-components

## [02]-[PUBLIC_TYPES]

[TYPE_NAMING_LAW]: one uniform quad per component, so a component name derives its whole type family
- rail: ui-components

| [INDEX] | [PATTERN]           | [ROLE]                                                              | [BARREL_EXPORTED] |
| :-----: | :------------------ | :----------------------------------------------------------------- | :---------------- |
|  [01]   | `<X>Props`          | props: Aria props + `RenderProps`/`StyleRenderProps` + `SlotProps` (+ `RACValidation` for form controls) | yes |
|  [02]   | `<X>RenderProps`    | interaction-state shape passed to the `className`/`style`/`children` functions | yes |
|  [03]   | `<X>Context`        | `React.Context<ContextValue<XProps, XElement>>` for slot/prop injection via `Provider` | value export |
|  [04]   | `<X>StateContext`   | `React.Context<<X>State \| null>` exposing the raw `react-stately` state | value export |

[PUBLIC_TYPE_SCOPE]: render-prop + slot substrate (the barrel exports exactly these four types + `DEFAULT_SLOT`)
- rail: ui-components

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]     | [RAIL]                                                          |
| :-----: | :------------------------ | :---------------- | :------------------------------------------------------------- |
|  [01]   | `RenderProps<T, E>`       | interface         | `className` / `style` / `children` value-or-`(state)=>value`    |
|  [02]   | `StyleRenderProps<T, E>`  | interface         | `className` / `style` value-or-`(state)=>value` (no children)   |
|  [03]   | `SlotProps`               | interface         | `slot?: string \| null`                                        |
|  [04]   | `ContextValue<T, E>`      | type alias        | `SlottedContextValue<WithRef<T, E>>` — slotted value + optional ref |
|  [05]   | `DEFAULT_SLOT`            | `unique symbol`   | default slot key when no `slot` prop is set                     |

[PUBLIC_TYPE_SCOPE]: render-prop STATE flag vocabulary — one union across families, a component carries the subset it earns
- rail: ui-components

| [INDEX] | [FLAG_GROUP]     | [FLAGS]                                                                                     | [CARRIED_BY]                          |
| :-----: | :--------------- | :----------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | interaction      | `isHovered`, `isPressed`, `isFocused`, `isFocusVisible`, `isDisabled`, `isPending`          | button, link, item, thumb families    |
|  [02]   | selection        | `isSelected`, `isIndeterminate`, `selectionMode`, `selectionBehavior`                       | checkbox, radio, switch, collection items |
|  [03]   | validity         | `isInvalid`, `isRequired`, `isReadOnly`, `validationErrors`, `validationDetails`            | field controls, `FieldError`          |
|  [04]   | disclosure/overlay | `isOpen`, `isEntering`, `isExiting`, `placement`, `trigger`, `isExpanded`, `close()`      | select/combobox, modal/popover/tooltip, disclosure, dialog |
|  [05]   | collection       | `isEmpty`, `isDropTarget`, `layout`, `orientation`, `level`, `state`                        | listbox, gridlist, table, tree, tabs  |

[PUBLIC_TYPE_SCOPE]: validation, drag-and-drop, data, colour, and collection-key types
- rail: cross-cutting

| [INDEX] | [SYMBOL]                                                                                  | [TYPE_FAMILY]     | [RAIL]                                            |
| :-----: | :---------------------------------------------------------------------------------------- | :---------------- | :------------------------------------------------ |
|  [01]   | `ValidationResult`                                                                        | interface         | `{ isInvalid: boolean; validationErrors: string[]; validationDetails: ValidityState }` — the `FieldError` render-prop |
|  [02]   | `DragAndDropHooks<T>`, `DragAndDropOptions<T>`, `DropIndicatorProps`, `DropIndicatorRenderProps`, `DragOptions`, `DragResult` | dnd family | `useDragAndDrop` return + the `dragAndDropHooks` prop |
|  [03]   | `ListData<T>`, `ListDataOptions`, `TreeData<T>`, `TreeDataOptions`, `AsyncListData<T>`, `AsyncListOptions`, `AsyncListLoadFunction`, `AsyncListLoadOptions`, `AsyncListStateUpdate` | data family | `useListData`/`useTreeData`/`useAsyncList` results |
|  [04]   | `Color`, `ColorSpace`, `ColorChannel`, `ColorFormat`, `ColorAxes`, `ColorChannelRange`    | colour family     | `parseColor`/`getColorChannels`/`ColorPickerState` |
|  [05]   | `Key`, `Selection`, `SelectionMode`, `SortDescriptor`, `SortDirection`, `RangeValue<T>`, `RouterConfig` | collection primitives | keys, sort, selection, ranged values      |
|  [06]   | `ContextValue`, `RenderProps`, `SlotProps`, `StyleRenderProps`, `VirtualizerProps`, `CollectionProps`, `CollectionRenderer` | composition | slot/render/virtualizer/collection typing |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: component roster — one contract, one row per family (root + sub-parts, render-state, state context)
- rail: ui-components

| [INDEX] | [FAMILY]            | [COMPONENTS (root + sub-parts)]                                                                                                          | [STATE_CONTEXT]                                   |
| :-----: | :------------------ | :-------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | button / toggle     | `Button`, `ToggleButton`, `ToggleButtonGroup`                                                                                            | `ToggleGroupStateContext`                         |
|  [02]   | fields              | `TextField`, `TextArea`, `Input`, `NumberField`, `SearchField`, `Label`, `Text`, `FieldError`, `Group`                                   | `NumberFieldStateContext`                         |
|  [03]   | form                | `Form` (`validationBehavior: 'aria' \| 'native'`)                                                                                        | `FormContext`, `FormValidationContext`            |
|  [04]   | checkbox / radio / switch | `Checkbox`, `CheckboxGroup`, `CheckboxField`, `CheckboxButton`, `RadioGroup`, `Radio`, `RadioField`, `RadioButton`, `Switch`, `SwitchField`, `SwitchButton` | `CheckboxGroupStateContext`, `RadioGroupStateContext` |
|  [05]   | slider              | `Slider`, `SliderOutput`, `SliderTrack`, `SliderThumb`, `SliderFill`                                                                     | `SliderStateContext`                              |
|  [06]   | select / combobox / autocomplete | `Select`, `SelectValue`, `ComboBox`, `ComboBoxValue`, `Autocomplete`                                                        | `SelectStateContext`, `ComboBoxStateContext`, `AutocompleteStateContext` |
|  [07]   | colour              | `ColorPicker`, `ColorArea`, `ColorField`, `ColorSlider`, `ColorWheel`, `ColorWheelTrack`, `ColorThumb`, `ColorSwatch`, `ColorSwatchPicker`, `ColorSwatchPickerItem` | `ColorPickerStateContext`, `ColorAreaStateContext`, `ColorWheelStateContext` |
|  [08]   | date / calendar     | `Calendar`, `RangeCalendar`, `CalendarGrid`, `CalendarGridHeader`, `CalendarGridBody`, `CalendarHeaderCell`, `CalendarCell`, `CalendarHeading`, `CalendarMonthPicker`, `CalendarYearPicker`, `DateField`, `TimeField`, `DateInput`, `DateSegment`, `DatePicker`, `DateRangePicker` | `CalendarStateContext`, `DateFieldStateContext`, `DatePickerStateContext` |
|  [09]   | overlays            | `Modal`, `ModalOverlay`, `Popover`, `OverlayArrow`, `Dialog`, `DialogTrigger`, `Tooltip`, `TooltipTrigger`                               | `OverlayTriggerStateContext`, `TooltipTriggerStateContext` |
|  [10]   | toast (UNSTABLE)    | `UNSTABLE_ToastRegion`, `UNSTABLE_ToastList`, `UNSTABLE_Toast`, `UNSTABLE_ToastContent`, `UNSTABLE_ToastQueue`                           | `UNSTABLE_ToastStateContext`                      |
|  [11]   | listbox / gridlist  | `ListBox`, `ListBoxItem`, `ListBoxSection`, `ListBoxLoadMoreItem`, `GridList`, `GridListItem`, `GridListSection`, `GridListHeader`, `GridListLoadMoreItem` | `ListStateContext`                       |
|  [12]   | menu                | `Menu`, `MenuItem`, `MenuTrigger`, `MenuSection`, `SubmenuTrigger`, `Keyboard`                                                           | `MenuStateContext`, `RootMenuTriggerStateContext` |
|  [13]   | table               | `Table`, `TableHeader`, `TableBody`, `Column`, `Row`, `Cell`, `ColumnResizer`, `ResizableTableContainer`, `TableFooter`, `TableLoadMoreItem`, `useTableOptions` | `TableStateContext`, `TableColumnResizeStateContext` |
|  [14]   | tree                | `Tree`, `TreeItem`, `TreeItemContent`, `TreeHeader`, `TreeSection`, `TreeLoadMoreItem`                                                   | `TreeStateContext`                                |
|  [15]   | tabs / disclosure   | `Tabs`, `TabList`, `Tab`, `TabPanel`, `TabPanels`, `Disclosure`, `DisclosureGroup`, `DisclosurePanel`                                    | `TabListStateContext`, `DisclosureStateContext`, `DisclosureGroupStateContext` |
|  [16]   | tag / breadcrumbs / link | `TagGroup`, `TagList`, `Tag`, `Breadcrumbs`, `Breadcrumb`, `Link`                                                                   | —                                                 |
|  [17]   | status / structure  | `ProgressBar`, `Meter`, `Separator`, `Toolbar`, `Header`, `Heading`, `Section`, `SelectionIndicator`                                     | —                                                 |
|  [18]   | file / drop         | `FileTrigger`, `DropZone`, `DropIndicator`                                                                                               | —                                                 |
|  [19]   | transitions         | `SharedElementTransition`, `SharedElement`                                                                                               | —                                                 |
|  [20]   | virtualization      | `Virtualizer`, `GridLayout`, `ListLayout`, `WaterfallLayout`, `TableLayout`, `Layout`, `LayoutInfo`, `Size`, `Rect`, `Point`            | —                                                 |

[ENTRYPOINT_SCOPE]: composition, slot, and collection-building utilities
- rail: ui-components

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [RAIL]                                                     |
| :-----: | :-------------------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `composeRenderProps(value, wrap)`                                     | utility fn     | chains render-prop fns preserving `defaultClassName`/`defaultChildren` |
|  [02]   | `useContextProps(props, ref, Context)`                               | hook           | `[mergedProps, ref]` — slot-forwarded prop merge at a subcomponent boundary |
|  [03]   | `useSlottedContext(Context, slot?)`                                  | hook           | reads the slotted context value (or `null`)                |
|  [04]   | `useRenderProps(options)`                                            | hook           | resolves className/style/children functions to concrete values |
|  [05]   | `Provider`                                                           | component      | composes up to 12 `[Context, value]` pairs in one node     |
|  [06]   | `Collection`, `CollectionBuilder`, `createLeafComponent`, `createBranchComponent` | builders | declarative collection subtree construction         |

[ENTRYPOINT_SCOPE]: drag-and-drop, data, colour, and validation seams
- rail: dnd / data / colour / validation

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [RAIL]                                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `useDragAndDrop(options: DragAndDropOptions<T>) => DragAndDropHooks<T>`                            | dnd hook       | passed as the `dragAndDropHooks` prop on `ListBox`/`GridList`/`Table`/`Tree` |
|  [02]   | `useDrag`, `useDrop`, `DIRECTORY_DRAG_TYPE`, `isDirectoryDropItem`, `isFileDropItem`, `isTextDropItem` | dnd helpers | element-level drag/drop + drop-item type guards          |
|  [03]   | `useListData(options)`, `useTreeData(options)`, `useAsyncList(options)`                            | data hooks     | local CRUD list/tree + server-sorted async list (re-exported from `react-stately`) |
|  [04]   | `parseColor(value) => Color`, `getColorChannels(colorSpace) => [ColorChannel, ColorChannel, ColorChannel]` | colour helpers | perceptual colour parse + channel set (re-exported from `react-stately/Color`) |
|  [05]   | `FormValidationContext` (`React.Context<Record<string, string \| string[]>>`)                     | validation seam | injects external per-field errors into every `FieldError` |

[ENTRYPOINT_SCOPE]: providers and behavior re-exports
- rail: composition

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :--------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `I18nProvider`, `useLocale`, `isRTL`, `useFilter`               | i18n           | locale context + locale-aware substring filter  |
|  [02]   | `RouterProvider`, `SSRProvider`                                 | providers      | client link-navigation + SSR id stability       |
|  [03]   | `Pressable`, `Focusable`, `VisuallyHidden`                      | behavior re-exports | `react-aria` interaction/focus/SR primitives |

## [04]-[IMPLEMENTATION_LAW]

[COMPONENT_TOPOLOGY]:
- `import 'client-only'` at the barrel root — the whole surface is client-React; a server import throws
- every component root places `data-rac` plus boolean/enum `data-*` state attributes on its DOM element; `tailwindcss-react-aria-components` variants target exactly those attributes, so styling has two co-equal paths — the render-prop function (typed `XRenderProps`) and the `data-*` Tailwind variant
- `className`/`style`/`children` each accept a static value OR a `(state: XRenderProps) => value` function; `composeRenderProps` composes two such functions without dropping the `defaultClassName`/`defaultChildren` passthrough
- `slot?: string | null` routes context-injected props; `DEFAULT_SLOT` is the fallback key; `useContextProps(props, ref, XContext)` merges slot-forwarded props at a custom subcomponent boundary, `useSlottedContext` reads them
- the internal render-prop mixins (`ClassNameOrFunction`, `RACValidation`, `DOMRenderProps`, `DOMRenderFunction`) are structural — reachable through each `XProps` but NOT barrel-exported; only `RenderProps`/`StyleRenderProps`/`SlotProps`/`ContextValue` and `DEFAULT_SLOT` are importable substrate
- stateful widgets expose `XStateContext` carrying the raw `react-stately` state (`ListState`, `SelectState`, `TableState`, `ColorPickerState`, …) so a bespoke subcomponent reads state without re-deriving it

[LOCAL_ADMISSION]:
- import every component from the `react-aria-components` barrel; `useTableOptions`, `parseColor`, `getColorChannels`, and the data hooks arrive from the same barrel
- `useDragAndDrop(options)` returns the `DragAndDropHooks` object to pass as `dragAndDropHooks` on `ListBox`/`GridList`/`Table`/`Tree`; the drop-item guards discriminate a `DropItem` into text/file/directory
- `Virtualizer` wraps a collection with a `Layout` (`ListLayout`/`GridLayout`/`WaterfallLayout`/`TableLayout`) for windowed rendering; the `*LoadMoreItem` components drive `useAsyncList` pagination
- `Text slot="description"` / `Text slot="errorMessage"` and `Label` are the a11y description/label building blocks a field composes, not raw markup

[STACKING]:
- universal tier `effect` (validation): the `interaction/form.md#FORM_BINDING` `FormBinding` folds one `Schema` through `Schema.decodeUnknownEither`, projects the `ParseResult.ParseError` via `ParseResult.ArrayFormatter.formatErrorSync` into a per-field `Record<string, string>`, and feeds it to `<FormValidationContext.Provider value={errorMap}>` — that `Record` IS the `ValidationErrors` shape (`Record<string, string | string[]>`) `FormValidationContext` consumes, and each field's `FieldError` render-prop exposes the resulting `ValidationResult` (`{ isInvalid, validationErrors, validationDetails }`); `Form validationBehavior="aria"` is the ARIA projection, the effect `Schema` the sole validity authority
- sibling `react-stately`: RAC consumes state internally and re-exports the state types as `XStateContext` values; `useListData`/`useTreeData`/`useAsyncList` feed the `items` prop; `UNSTABLE_ToastQueue` (the `ToastQueue` alias) or a `react-stately` `ToastQueue` drives `UNSTABLE_ToastRegion` for the `announce.md` toast surface with F6 landmark navigation
- sibling `react-aria`: RAC pre-composes the `react-aria` hooks; `Pressable`/`Focusable`/`VisuallyHidden`/`useFilter` re-export directly, so a consumer never re-imports from `react-aria` for those
- sibling `tailwindcss-react-aria-components`: the `data-rac` + `data-*` attributes are the plugin's selector target — `open:`, `disabled:`, `selected:`, `placement-left:` variants pair with the same state the render-prop function receives
- sibling `@radix-ui/react-slot`: RAC's slot system (`slot` prop + `DEFAULT_SLOT` + `useContextProps`) is its OWN mechanism, distinct from Radix `asChild`; `command.md` uses Radix `Slot` for the `cn` recipe, RAC uses `composeRenderProps` for render-prop composition — never conflate the two
- `interaction/picker.md#PICKER_BEHAVIOR`: `Calendar`/`RangeCalendar`/`DatePicker` (date arms) and `FileTrigger` (ingest arms) are the widgets the `$match` selects; `parseColor`/`getColorChannels`/`Color`/`ColorSpace` are the colour-state surface `colorPick` projects into the OKLCH token space; `Autocomplete` is the RAC-native filtered-collection primitive (an alternative to the `cmdk` palette) wrapping `useFilter`

[RAIL_LAW]:
- package: `react-aria-components`
- owns: fully composed accessible components combining ARIA behavior, `react-stately` state, and render-prop styling; the `data-rac` styling contract; the `FormValidationContext` external-error seam
- accept: `className`/`style`/`children` render fns, a `slot` prop, `Provider` context injection, `dragAndDropHooks`, a `Layout` for virtualization, `validationBehavior` + `FormValidationContext` for validity
- reject: re-implementing component ARIA behavior, duplicating `react-stately` state a `StateContext` already exposes, importing `ClassNameOrFunction`/`RACValidation`/`DOMRenderFunction` as public types, conflating the RAC `slot` system with Radix `asChild`, a server-side import of the `client-only` barrel
