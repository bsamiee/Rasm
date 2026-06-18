# [API_CATALOGUE] react-aria-components

`react-aria-components` delivers styled-ready, accessible React component wrappers over the `react-aria` hook layer and `react-stately` state layer. Each component accepts a `render` prop override, a `className` or `style` render-prop function receiving component interaction state, and a `slot` prop for context-driven prop forwarding. The package is the primary AppUi component surface: form controls, overlays, collections, navigation, disclosure, drag-and-drop, date pickers, and colour pickers are all covered by one barrel import.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-aria-components`
- package: `react-aria-components`
- module: `react-aria-components`
- namespace: `react-aria-components`
- asset: runtime component library (`client-only`)
- rail: ui-components

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: shared render-prop and slot types
- rail: ui-components

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                                   |
| :-----: | :------------------------ | :------------ | :--------------------------------------- |
|   [1]   | `RenderProps<T, E>`       | interface     | `className`, `style`, `children` fns     |
|   [2]   | `StyleRenderProps<T, E>`  | interface     | `className` + `style` render fns         |
|   [3]   | `ClassNameOrFunction<T>`  | type alias    | `string \| (state) => string`            |
|   [4]   | `SlotProps`               | interface     | `slot?: string \| null`                  |
|   [5]   | `ContextValue<T, E>`      | type alias    | slotted context value with optional ref  |
|   [6]   | `DOMRenderFunction<E, T>` | type alias    | custom DOM render override signature     |
|   [7]   | `RACValidation`           | interface     | `validationBehavior: 'native' \| 'aria'` |
|   [8]   | `DEFAULT_SLOT`            | unique symbol | default slot key                         |

[PUBLIC_TYPE_SCOPE]: form control render-prop state types
- rail: ui-components

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                                                           |
| :-----: | :---------------------- | :-------------- | :--------------------------------------------------------------- |
|   [1]   | `ButtonRenderProps`     | state interface | `isHovered`, `isPressed`, `isFocused`, `isDisabled`, `isPending` |
|   [2]   | `TextFieldRenderProps`  | state interface | `isDisabled`, `isInvalid`, `isReadOnly`                          |
|   [3]   | `SelectRenderProps`     | state interface | `isOpen`, `isDisabled`, `isInvalid`                              |
|   [4]   | `ComboBoxRenderProps`   | state interface | `isOpen`, `isDisabled`, `isInvalid`                              |
|   [5]   | `CheckboxRenderProps`   | state interface | `isSelected`, `isIndeterminate`, `isHovered`, `isDisabled`       |
|   [6]   | `RadioGroupRenderProps` | state interface | `isDisabled`, `isReadOnly`, `isInvalid`, `orientation`           |
|   [7]   | `SliderRenderProps`     | state interface | `orientation`, `isDisabled`                                      |
|   [8]   | `SwitchRenderProps`     | state interface | `isSelected`, `isHovered`, `isDisabled`                          |

[PUBLIC_TYPE_SCOPE]: overlay and dialog render-prop state types
- rail: overlay

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [RAIL]                                            |
| :-----: | :------------------- | :-------------- | :------------------------------------------------ |
|   [1]   | `ModalRenderProps`   | state interface | `isEntering`, `isExiting`, `state`                |
|   [2]   | `PopoverRenderProps` | state interface | `trigger`, `placement`, `isEntering`, `isExiting` |
|   [3]   | `DialogRenderProps`  | state interface | `close(): void`                                   |
|   [4]   | `TooltipRenderProps` | state interface | `isEntering`, `isExiting`, `placement`            |

[PUBLIC_TYPE_SCOPE]: collection render-prop state types
- rail: collection

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]   | [RAIL]                                                             |
| :-----: | :----------------------- | :-------------- | :----------------------------------------------------------------- |
|   [1]   | `ListBoxRenderProps`     | state interface | `isEmpty`, `isFocusVisible`, `layout`, `orientation`               |
|   [2]   | `ListBoxItemRenderProps` | state interface | `isSelected`, `isHovered`, `isPressed`, `isFocused`, `isDisabled`  |
|   [3]   | `GridListRenderProps`    | state interface | `isEmpty`, `isFocusVisible`                                        |
|   [4]   | `TableRenderProps`       | state interface | `isDropTarget`, `isFocusVisible`                                   |
|   [5]   | `RowRenderProps`         | state interface | `isSelected`, `isHovered`, `isFocused`, `isDisabled`, `isExpanded` |
|   [6]   | `TreeRenderProps`        | state interface | `isEmpty`, `isFocusVisible`                                        |
|   [7]   | `TreeItemRenderProps`    | state interface | `isSelected`, `isExpanded`, `isDisabled`, `level`                  |
|   [8]   | `TabsRenderProps`        | state interface | `orientation`                                                      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: form control components
- rail: ui-components

| [INDEX] | [SURFACE]       | [ENTRY_FAMILY]      | [PROPS_TYPE]                                           |
| :-----: | :-------------- | :------------------ | :----------------------------------------------------- |
|   [1]   | `Button`        | component           | `ButtonProps & RefAttributes<HTMLButtonElement>`       |
|   [2]   | `ToggleButton`  | component           | `ToggleButtonProps & RefAttributes<HTMLButtonElement>` |
|   [3]   | `TextField`     | component           | `TextFieldProps & RefAttributes<HTMLDivElement>`       |
|   [4]   | `NumberField`   | component           | `NumberFieldProps & RefAttributes<HTMLDivElement>`     |
|   [5]   | `SearchField`   | component           | `SearchFieldProps & RefAttributes<HTMLDivElement>`     |
|   [6]   | `TextArea`      | component           | `TextAreaProps & RefAttributes<HTMLTextAreaElement>`   |
|   [7]   | `Input`         | component           | `InputProps & RefAttributes<HTMLInputElement>`         |
|   [8]   | `Checkbox`      | component           | `CheckboxProps & RefAttributes<HTMLLabelElement>`      |
|   [9]   | `CheckboxGroup` | component           | `CheckboxGroupProps & RefAttributes<HTMLDivElement>`   |
|  [10]   | `RadioGroup`    | component           | `RadioGroupProps & RefAttributes<HTMLDivElement>`      |
|  [11]   | `Radio`         | component           | `RadioProps & RefAttributes<HTMLLabelElement>`         |
|  [12]   | `Switch`        | component           | `SwitchProps & RefAttributes<HTMLLabelElement>`        |
|  [13]   | `Slider`        | component           | `SliderProps & RefAttributes<HTMLDivElement>`          |
|  [14]   | `Select`        | component (generic) | `SelectProps<T, M> & RefAttributes<HTMLDivElement>`    |
|  [15]   | `SelectValue`   | component (generic) | `SelectValueProps<T> & RefAttributes<HTMLSpanElement>` |
|  [16]   | `ComboBox`      | component (generic) | `ComboBoxProps<T, M> & RefAttributes<HTMLDivElement>`  |
|  [17]   | `Form`          | component           | `FormProps & RefAttributes<HTMLFormElement>`           |
|  [18]   | `FieldError`    | component           | `FieldErrorProps`                                      |

[ENTRYPOINT_SCOPE]: date and time picker components
- rail: date-time

| [INDEX] | [SURFACE]         | [ENTRY_FAMILY] | [RAIL]                   |
| :-----: | :---------------- | :------------- | :----------------------- |
|   [1]   | `DateField`       | component      | date segments input      |
|   [2]   | `DateInput`       | component      | inner segment container  |
|   [3]   | `DateSegment`     | component      | individual date segment  |
|   [4]   | `TimeField`       | component      | time segments input      |
|   [5]   | `DatePicker`      | component      | date picker with popover |
|   [6]   | `DateRangePicker` | component      | date range picker        |
|   [7]   | `Calendar`        | component      | monthly calendar grid    |
|   [8]   | `RangeCalendar`   | component      | range selection calendar |
|   [9]   | `CalendarGrid`    | component      | calendar grid layout     |
|  [10]   | `CalendarCell`    | component      | individual calendar cell |

[ENTRYPOINT_SCOPE]: overlay and dialog components
- rail: overlay

| [INDEX] | [SURFACE]              | [ENTRY_FAMILY] | [PROPS_TYPE]                                        |
| :-----: | :--------------------- | :------------- | :-------------------------------------------------- |
|   [1]   | `Modal`                | component      | `ModalOverlayProps & RefAttributes<HTMLDivElement>` |
|   [2]   | `ModalOverlay`         | component      | `ModalOverlayProps & RefAttributes<HTMLDivElement>` |
|   [3]   | `Dialog`               | component      | `DialogProps & RefAttributes<HTMLElement>`          |
|   [4]   | `DialogTrigger`        | component      | `{ children: ReactNode } & OverlayTriggerProps`     |
|   [5]   | `Popover`              | component      | `PopoverProps & RefAttributes<HTMLElement>`         |
|   [6]   | `OverlayArrow`         | component      | `OverlayArrowProps & RefAttributes<SVGSVGElement>`  |
|   [7]   | `Tooltip`              | component      | `TooltipProps & RefAttributes<HTMLElement>`         |
|   [8]   | `TooltipTrigger`       | component      | `TooltipTriggerComponentProps`                      |
|   [9]   | `UNSTABLE_Toast`       | component      | toast item (unstable API)                           |
|  [10]   | `UNSTABLE_ToastRegion` | component      | toast region container (unstable API)               |

[ENTRYPOINT_SCOPE]: collection components
- rail: collection

| [INDEX] | [SURFACE]        | [ENTRY_FAMILY]      | [RAIL]                       |
| :-----: | :--------------- | :------------------ | :--------------------------- |
|   [1]   | `ListBox`        | component (generic) | option list                  |
|   [2]   | `ListBoxItem`    | component (generic) | single option                |
|   [3]   | `ListBoxSection` | component (generic) | option group                 |
|   [4]   | `GridList`       | component (generic) | keyboard-navigable grid list |
|   [5]   | `GridListItem`   | component (generic) | grid list row                |
|   [6]   | `Menu`           | component (generic) | dropdown menu                |
|   [7]   | `MenuItem`       | component (generic) | menu item                    |
|   [8]   | `MenuTrigger`    | component           | menu trigger button          |
|   [9]   | `MenuSection`    | component (generic) | menu section group           |
|  [10]   | `SubmenuTrigger` | component           | nested submenu trigger       |
|  [11]   | `Table`          | component           | data table                   |
|  [12]   | `TableHeader`    | component (generic) | table header row group       |
|  [13]   | `TableBody`      | component (generic) | table body row group         |
|  [14]   | `Column`         | component           | table column definition      |
|  [15]   | `Row`            | component (generic) | table data row               |
|  [16]   | `Cell`           | component           | table data cell              |

[ENTRYPOINT_SCOPE]: navigation and tabs components
- rail: navigation

| [INDEX] | [SURFACE]     | [ENTRY_FAMILY]      | [RAIL]                          |
| :-----: | :------------ | :------------------ | :------------------------------ |
|   [1]   | `Tabs`        | component           | tab container with orientation  |
|   [2]   | `TabList`     | component (generic) | list of tab triggers            |
|   [3]   | `Tab`         | component           | individual tab trigger          |
|   [4]   | `TabPanel`    | component           | tab content panel               |
|   [5]   | `TabPanels`   | component (generic) | collection-driven tab panels    |
|   [6]   | `Breadcrumbs` | component (generic) | breadcrumb navigation           |
|   [7]   | `Breadcrumb`  | component           | single breadcrumb item          |
|   [8]   | `Link`        | component           | accessible anchor / router link |

[ENTRYPOINT_SCOPE]: disclosure, toolbar, tag, tree, file components
- rail: ui-components

| [INDEX] | [SURFACE]         | [ENTRY_FAMILY]      | [RAIL]                        |
| :-----: | :---------------- | :------------------ | :---------------------------- |
|   [1]   | `Disclosure`      | component           | expand/collapse panel         |
|   [2]   | `DisclosurePanel` | component           | disclosure content region     |
|   [3]   | `DisclosureGroup` | component           | accordion group               |
|   [4]   | `Toolbar`         | component           | tool region landmark          |
|   [5]   | `TagGroup`        | component (generic) | removable tag collection      |
|   [6]   | `TagList`         | component (generic) | tag list container            |
|   [7]   | `Tag`             | component           | individual tag                |
|   [8]   | `Tree`            | component (generic) | hierarchical tree view        |
|   [9]   | `TreeItem`        | component (generic) | tree row with expand/collapse |
|  [10]   | `TreeItemContent` | component           | tree item content region      |
|  [11]   | `FileTrigger`     | component           | file input trigger            |
|  [12]   | `DropZone`        | component           | drag-and-drop drop target     |

[ENTRYPOINT_SCOPE]: progress, meter, separator, visually hidden
- rail: ui-components

| [INDEX] | [SURFACE]        | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :--------------- | :------------- | :-------------------------------- |
|   [1]   | `ProgressBar`    | component      | deterministic / indeterminate bar |
|   [2]   | `Meter`          | component      | status meter                      |
|   [3]   | `Separator`      | component      | horizontal / vertical rule        |
|   [4]   | `VisuallyHidden` | component      | SR-only wrapper                   |

[ENTRYPOINT_SCOPE]: context and composition utilities
- rail: ui-components

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :------------------------------------- | :------------- | :-------------------------------- |
|   [1]   | `composeRenderProps(value, wrap)`      | utility fn     | chain render-prop functions       |
|   [2]   | `useContextProps(props, ref, context)` | hook           | slot-based prop forwarding        |
|   [3]   | `useSlottedContext(context, slot?)`    | hook           | read slotted context value        |
|   [4]   | `Provider`                             | component      | compose multiple context values   |
|   [5]   | `I18nProvider`                         | component      | locale context provider           |
|   [6]   | `RouterProvider`                       | component      | link-navigation context           |
|   [7]   | `useListData(options)`                 | data hook      | CRUD list state manager           |
|   [8]   | `useTreeData(options)`                 | data hook      | hierarchical CRUD state manager   |
|   [9]   | `useAsyncList(options)`                | data hook      | server-sorted/filtered async list |
|  [10]   | `useDragAndDrop(options)`              | dnd hook       | returns `DragAndDropHooks<T>`     |

## [4]-[IMPLEMENTATION_LAW]

[COMPONENT_TOPOLOGY]:
- `import 'client-only'` at barrel root; components run in a client React environment only
- every component places `data-rac` on its root DOM element; `tailwindcss-react-aria-components` targets this attribute
- `className` and `style` accept either a static value or a function `(state) => value` where `state` carries interaction flags
- `children` on interactive components accepts either `ReactNode` or `(state) => ReactNode`
- `slot` accepts a string slot name or `null` to opt out of context-driven props
- context exports (e.g. `ButtonContext`, `ListBoxContext`) follow the `Context<ContextValue<Props, Element>>` shape
- state contexts (e.g. `ListStateContext`, `TabListStateContext`) expose the raw `react-stately` state for custom subcomponents
- `DEFAULT_SLOT` is the unique symbol used when no explicit `slot` prop is provided

[LOCAL_ADMISSION]:
- import all components from the `react-aria-components` barrel
- `composeRenderProps` composes two render-prop functions without losing the `defaultClassName` / `defaultChildren` passthrough
- `useContextProps(props, ref, SomeContext)` merges slot-forwarded props with local props at a custom subcomponent boundary
- `useListData` / `useTreeData` / `useAsyncList` are re-exported from `react-stately` for convenience
- `useDragAndDrop` returns the `DragAndDropHooks` object to pass as the `dragAndDropHooks` prop on `ListBox`, `GridList`, or `Table`

[RAIL_LAW]:
- package: `react-aria-components`
- owns: fully composed accessible UI components that combine ARIA behaviour, state, and render-prop styling
- accept: `className` / `style` render fns, `slot` prop, `render` prop for DOM override, context injection via `Provider`
- reject: re-implementing component ARIA behaviour manually, duplicating state management already in `react-stately`
