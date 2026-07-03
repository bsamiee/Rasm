# [API_CATALOGUE] react-aria

`react-aria` is the headless ARIA behavior layer: one uniform hook contract — `use<Widget>(props, state?, ...refs) => { <slot>Props, ...interactionState }` — produces DOM attribute bags a consumer spreads onto its own markup, keeping styling fully decoupled from accessibility. Every stateful widget hook consumes the matching `react-stately` `use<Widget>State` object as its `state` argument, so behavior and state are two halves of one seam. Beyond the widget roster it owns the cross-cutting rails the whole interaction spine reuses: normalized pointer/keyboard interactions, focus containment and rings, `Intl`-backed i18n, overlay positioning, drag-and-drop coordination, landmark F6 navigation, collection building, and prop-merge utilities. `react-aria-components` is these hooks pre-composed; the `interaction/role.md` `RoleBehavior` discards per-component `.tsx` and keeps the headless hook behavior directly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-aria`
- package: `react-aria`
- version: `3.50.0`
- license: `Apache-2.0`
- module: `react-aria` (barrel); `react-aria/i18n` (locale sub-path); `react-aria/private/live-announcer/LiveAnnouncer` (the `@react-aria/live-announcer` source)
- namespace: ~120 hook/component/util value exports + ~200 `Aria*`/`*Aria`/`*Props` type exports
- asset: dual CJS/ESM (`dist/exports/index.cjs` / `.js`), `sideEffects: false`, fully tree-shakeable per-hook
- runtime: client React (peer `react ^19`, `react-dom ^19`); SSR-safe via `SSRProvider`/`useIsSSR`
- contract: `use<Widget>(props, state?, ...refs)` returns a `{ <slot>Props }` prop bag plus interaction flags; hooks are stateless — `react-stately` owns state, refs come from the consumer
- rail: interaction / accessibility

## [02]-[PUBLIC_TYPES]

[TYPE_NAMING_LAW]: the per-widget type family follows one uniform triple, so a widget name derives its types
- rail: interaction

| [INDEX] | [PATTERN]            | [ROLE]                                                    | [EXAMPLE]                                            |
| :-----: | :------------------- | :------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `Aria<Widget>Props`  | full input props (ARIA + behavior + `children`)          | `AriaButtonProps<E>`, `AriaListBoxProps<T>`         |
|  [02]   | `Aria<Widget>Options`| input props minus `children`, element-typed for the hook | `AriaButtonOptions<E>`, `AriaColorAreaOptions`      |
|  [03]   | `<Widget>Aria`       | result bag of DOM prop objects + interaction flags        | `ButtonAria<T>`, `ListBoxAria`, `DateFieldAria`     |

[PUBLIC_TYPE_SCOPE]: interaction event + result types (shared across every widget, from `@react-types/shared`)
- rail: interaction

| [INDEX] | [SYMBOL]                                                                              | [TYPE_FAMILY]     | [RAIL]                                            |
| :-----: | :------------------------------------------------------------------------------------ | :---------------- | :------------------------------------------------ |
|  [01]   | `PressEvent`, `PressEvents`, `PressResult`, `PressProps`, `PressHookProps`             | press family      | normalized pointer/key/touch press               |
|  [02]   | `HoverEvent`, `HoverEvents`, `HoverResult`, `HoverProps`                               | hover family      | hover sans touch emulation                        |
|  [03]   | `MoveStartEvent`, `MoveMoveEvent`, `MoveEndEvent`, `MoveEvent`, `MoveEvents`, `MoveResult` | move family    | pointer/keyboard drag deltas (sliders, color)     |
|  [04]   | `LongPressEvent`, `LongPressProps`, `LongPressResult`                                  | long-press family | held-press detection                             |
|  [05]   | `KeyboardEvents`, `KeyboardProps`, `KeyboardResult`, `ScrollWheelProps`                | keyboard family   | key/wheel event delegation                        |
|  [06]   | `FocusEvents`, `FocusProps`, `FocusResult`, `FocusWithinProps`, `FocusWithinResult`, `FocusVisibleProps`, `FocusVisibleResult` | focus family | focus normalization + within/visible splits |
|  [07]   | `ButtonAria<T>`, `AriaButtonProps<E>`, `AriaButtonOptions<E>`, `AriaBaseButtonProps`, `LinkButtonProps` | button family | element-typed button props (`button`/`a`/`div`/`input`/`span`) |

[PUBLIC_TYPE_SCOPE]: focus management + overlay positioning types
- rail: focus / overlay

| [INDEX] | [SYMBOL]                                                             | [TYPE_FAMILY]     | [RAIL]                                            |
| :-----: | :------------------------------------------------------------------- | :---------------- | :------------------------------------------------ |
|  [01]   | `FocusScopeProps`                                                    | props interface   | `contain`, `restoreFocus`, `autoFocus`            |
|  [02]   | `FocusManager`, `FocusManagerOptions`                               | interface         | `focusNext/Previous/First/Last`; `from/tabbable/wrap/accept` |
|  [03]   | `AriaFocusRingProps`, `FocusRingAria`, `FocusRingProps`             | focus-ring family | `within`, `isTextInput`, keyboard-visible ring    |
|  [04]   | `FocusableOptions`, `FocusableAria`, `FocusableProps`               | focusable family  | imperative `.focus()` wrapper props               |
|  [05]   | `AriaPositionProps`, `PositionProps`, `PositionAria`, `Placement`, `PlacementAxis`, `Axis`, `SizeAxis` | overlay position | `overlayProps`, `arrowProps`, resolved placement  |

[PUBLIC_TYPE_SCOPE]: drag-and-drop + landmark + i18n types
- rail: dnd / landmark / i18n

| [INDEX] | [SYMBOL]                                                                                 | [TYPE_FAMILY]     | [RAIL]                                            |
| :-----: | :--------------------------------------------------------------------------------------- | :---------------- | :------------------------------------------------ |
|  [01]   | `DragItem`, `DropItem`, `TextDropItem`, `FileDropItem`, `DirectoryDropItem`, `DragTypes` | drop-item family  | typed clipboard/drag payloads                     |
|  [02]   | `DropOperation`, `DropPosition`, `DropTarget`, `ItemDropTarget`, `RootDropTarget`, `DropTargetDelegate` | drop-target family | drop geometry + operation kind                |
|  [03]   | `DragStartEvent`, `DragMoveEvent`, `DragEndEvent`, `DropEvent`, `DropEnterEvent`, `DropExitEvent`, `DropMoveEvent`, `DroppableCollection*Event` | dnd event family | drag/drop lifecycle events                 |
|  [04]   | `AriaLandmarkRole`, `AriaLandmarkProps`, `LandmarkAria`, `LandmarkController`, `LandmarkControllerOptions` | landmark family | F6 region navigation (`navigate('forward'\|'backward')`) |
|  [05]   | `Filter`, `Locale`, `I18nProviderProps`, `DateFormatterOptions`                          | i18n family       | `startsWith`/`endsWith`/`contains`; `{locale, direction}` |
|  [06]   | `Key`, `Orientation`, `RangeValue<T>`, `DateValue`, `TimeValue`, `DateRange`             | shared primitives | collection keys, axes, ranged values              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: widget hook roster — one contract, one row per family; `state` is the matching `react-stately` object
- rail: per-widget

| [INDEX] | [WIDGET_FAMILY]      | [HOOKS]                                                                                                       | [STATE_ARG (react-stately)]              |
| :-----: | :------------------- | :----------------------------------------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | button / toggle      | `useButton`, `useToggleButton`, `useToggleButtonGroup`, `useToggleButtonGroupItem`                            | `ToggleState` / `ToggleGroupState`       |
|  [02]   | listbox              | `useListBox`, `useOption`, `useListBoxSection`                                                                | `ListState<T>`                           |
|  [03]   | gridlist             | `useGridList`, `useGridListItem`, `useGridListSection`, `useGridListSelectionCheckbox`                        | `ListState<T>`                           |
|  [04]   | menu                 | `useMenu`, `useMenuItem`, `useMenuSection`, `useMenuTrigger`, `useSubmenuTrigger`                             | `TreeState<T>` / `MenuTriggerState`      |
|  [05]   | select / combobox    | `useSelect`, `useHiddenSelect`, `HiddenSelect`, `useComboBox`, `useAutocomplete`                              | `SelectState<T,M>` / `ComboBoxState<T>`  |
|  [06]   | text / number / search | `useTextField`, `useNumberField`, `useSearchField`, `useField`, `useLabel`                                  | `NumberFieldState` / `SearchFieldState`  |
|  [07]   | checkbox / radio / switch | `useCheckbox`, `useCheckboxGroup`, `useCheckboxGroupItem`, `useRadio`, `useRadioGroup`, `useSwitch`      | `CheckboxGroupState` / `RadioGroupState` / `ToggleState` |
|  [08]   | slider               | `useSlider`, `useSliderThumb`                                                                                 | `SliderState`                            |
|  [09]   | calendar / date / time | `useCalendar`, `useRangeCalendar`, `useCalendarGrid`, `useCalendarCell`, `useCalendarHeading`, `useCalendarMonthPicker`, `useCalendarYearPicker`, `useDateField`, `useTimeField`, `useDatePicker`, `useDateRangePicker`, `useDateSegment` | `CalendarState` / `DateFieldState` / `DatePickerState` |
|  [10]   | color                | `useColorArea`, `useColorField`, `useColorChannelField`, `useColorSlider`, `useColorSwatch`, `useColorWheel` | `ColorAreaState` / `ColorFieldState` / `ColorWheelState` |
|  [11]   | table                | `useTable`, `useTableRow`, `useTableCell`, `useTableColumnHeader`, `useTableColumnResize`, `useTableHeaderRow`, `useTableRowGroup`, `useTableSelectAllCheckbox`, `useTableSelectionCheckbox` | `TableState<T>` |
|  [12]   | tree                 | `useTree`, `useTreeItem`                                                                                      | `TreeState<T>`                           |
|  [13]   | tabs                 | `useTabList`, `useTab`, `useTabPanel`                                                                         | `TabListState<T>`                        |
|  [14]   | tag group            | `useTagGroup`, `useTag`                                                                                       | `ListState<T>`                           |
|  [15]   | disclosure           | `useDisclosure`                                                                                              | `DisclosureState`                        |
|  [16]   | breadcrumbs / link   | `useBreadcrumbs`, `useBreadcrumbItem`, `useLink`                                                              | —                                        |
|  [17]   | progress / meter     | `useProgressBar`, `useMeter`                                                                                  | —                                        |
|  [18]   | separator / toolbar  | `useSeparator`, `useToolbar`                                                                                  | —                                        |
|  [19]   | dialog / tooltip     | `useDialog`, `useTooltip`, `useTooltipTrigger`                                                                | `TooltipTriggerState`                    |
|  [20]   | toast                | `useToast`, `useToastRegion`                                                                                  | `ToastState<T>`                          |

[ENTRYPOINT_SCOPE]: interaction hooks — cross-cutting, not per-widget; return an event-normalized prop bag
- rail: interaction

| [INDEX] | [SURFACE]                        | [RETURNS]                          | [RAIL]                                    |
| :-----: | :------------------------------- | :--------------------------------- | :---------------------------------------- |
|  [01]   | `usePress(props)`                | `{ pressProps, isPressed }`        | normalized pointer/key/touch press        |
|  [02]   | `useHover(props)`                | `{ hoverProps, isHovered }`        | hover sans touch emulation                |
|  [03]   | `useMove(props)`                 | `{ moveProps }`                    | pointer/keyboard move deltas              |
|  [04]   | `useKeyboard(props)`             | `{ keyboardProps }`                | key event delegation                      |
|  [05]   | `useLongPress(props)`            | `{ longPressProps }`               | held-press detection                      |
|  [06]   | `useFocus(props)`                | `{ focusProps }`                   | focus/blur normalization                  |
|  [07]   | `useFocusVisible(props?)`        | `{ isFocusVisible }`               | keyboard-only focus visibility            |
|  [08]   | `useFocusWithin(props)`          | `{ focusWithinProps }`             | focus-within containment                  |
|  [09]   | `useInteractOutside(props, ref)` | —                                  | outside pointer detection                 |
|  [10]   | `Pressable`                      | component                          | press-behavior wrapper for a single child |

[ENTRYPOINT_SCOPE]: focus management + collection building
- rail: focus / collection

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :------------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `FocusScope(props)`                                                  | component      | `contain` / `restoreFocus` / `autoFocus`   |
|  [02]   | `useFocusManager()`                                                  | hook           | `FocusManager` from nearest scope          |
|  [03]   | `FocusRing(props)`, `useFocusRing(props?)`                          | component/hook | `{ isFocused, isFocusVisible, focusProps }`|
|  [04]   | `useFocusable(props, ref)`, `Focusable`                             | hook/component | spread focusable props / imperative focus  |
|  [05]   | `Collection`, `CollectionBuilder`, `createLeafComponent`, `createBranchComponent` | builders | declarative collection tree construction |
|  [06]   | `ListKeyboardDelegate`                                              | class          | arrow-key navigation delegate              |

[ENTRYPOINT_SCOPE]: i18n hooks (main barrel + the `react-aria/i18n` sub-path)
- rail: i18n

| [INDEX] | [SURFACE]                                            | [RETURNS]                          | [RAIL]                            |
| :-----: | :--------------------------------------------------- | :--------------------------------- | :-------------------------------- |
|  [01]   | `useLocale()`                                        | `Locale` `{ locale, direction }`   | nearest provider locale           |
|  [02]   | `useFilter(options?: Intl.CollatorOptions)`          | `Filter` `{ startsWith, endsWith, contains }` | locale-aware substring search |
|  [03]   | `useCollator(options?)`                              | `Intl.Collator`                    | locale comparison                 |
|  [04]   | `useDateFormatter(options?)`                         | `DateFormatter`                    | `Intl.DateTimeFormat` wrapper     |
|  [05]   | `useNumberFormatter(options?)`, `useListFormatter(options?)` | `Intl.NumberFormat` / `Intl.ListFormat` | number / list formatting    |
|  [06]   | `useLocalizedStringFormatter(strings, packageName?)`, `useLocalizedStringDictionary(strings, packageName?)` | localized-string surface | translated string maps    |
|  [07]   | `I18nProvider`, `isRTL(locale)`                     | component / boolean                | locale context, RTL test          |

[ENTRYPOINT_SCOPE]: overlays, landmark, drag-and-drop
- rail: overlay / landmark / dnd

| [INDEX] | [SURFACE]                                                                                              | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :---------------------------------------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `useOverlay`, `useOverlayTrigger`, `useOverlayPosition`, `useModalOverlay`, `usePopover`, `usePreventScroll` | overlay hooks | ARIA overlay attrs + `{ overlayProps, arrowProps, placement }` |
|  [02]   | `Overlay`, `DismissButton`, `ModalProvider`, `OverlayProvider`, `OverlayContainer`, `useModal`, `useModalProvider` | overlay components | portal host, SR dismiss target, modal context |
|  [03]   | `UNSAFE_PortalProvider`, `useUNSAFE_PortalContext`                                                     | portal escape  | custom portal container                         |
|  [04]   | `useLandmark(props, ref)`                                                                              | landmark hook  | F6 region nav; `LandmarkController.navigate('forward'\|'backward')` |
|  [05]   | `useDrag`, `useDrop`, `useDraggableCollection`, `useDroppableCollection`, `useDraggableItem`, `useDroppableItem`, `useDropIndicator`, `useClipboard` | dnd hooks | drag/drop/clipboard coordination |
|  [06]   | `DragPreview`, `ListDropTargetDelegate`, `DIRECTORY_DRAG_TYPE`, `isDirectoryDropItem`, `isFileDropItem`, `isTextDropItem` | dnd helpers | preview render, drop delegate, drop-item type guards |

[ENTRYPOINT_SCOPE]: SSR + prop-merge utilities
- rail: utility

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :--------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `mergeProps(...args)`        | utility        | chains handlers, combines classNames, dedupes ids, merges refs |
|  [02]   | `chain(...fns)`              | utility        | composes multiple event handlers into one       |
|  [03]   | `mergeRefs(...refs)`         | utility        | merges multiple refs                            |
|  [04]   | `useId(defaultId?)`, `useObjectRef(forwardedRef)` | utility hooks | stable id, `RefObject` from `ForwardedRef` |
|  [05]   | `VisuallyHidden`, `useVisuallyHidden(props?)` | component/hook | SR-only wrapper; `isFocusable` reveals on focus (skip links) |
|  [06]   | `RouterProvider`, `SSRProvider`, `useIsSSR()` | providers/hook | client router context, SSR id stability, SSR detection |

## [04]-[IMPLEMENTATION_LAW]

[HOOK_TOPOLOGY]:
- every widget hook returns a bag of DOM prop objects (`buttonProps`, `listBoxProps`, `optionProps`) plus interaction flags; the consumer spreads each bag onto the matching element and reads the flags for styling
- stateful widget hooks are pure functions of `(props, state, ref)` — they hold no state; the `state` argument is the `react-stately` `use<Widget>State` object, so a widget is exactly one behavior hook plus one state hook
- `useButton` is overloaded on `elementType` (`'button' | 'a' | 'div' | 'input' | 'span' | ElementType`) so a non-`button` element still receives correct role/keyboard/press semantics
- interaction hooks (`usePress`/`useHover`/`useMove`/`useKeyboard`/`useLongPress`/`useFocus*`) are the atoms every widget hook composes internally and a consumer composes for a bespoke role; they normalize pointer, touch, and keyboard into one event shape
- `FocusScope` contains/restores focus for its subtree; `useFocusManager()` resolves the nearest scope; `useFocusRing` distinguishes keyboard focus from pointer focus for the visible ring
- `useLandmark` registers a landmark region and wires F6 cross-region navigation through `LandmarkController.navigate`

[LOCAL_ADMISSION]:
- import from the `react-aria` barrel or the `react-aria/i18n` sub-path; the private `react-aria/private/live-announcer/LiveAnnouncer` path is the `@react-aria/live-announcer` source
- `mergeProps` chains handlers rather than overwriting — use it at every composition boundary where a role behavior, an interaction hook, and consumer props meet; `chain` is the lower-level handler-only compose
- `I18nProvider` wraps the app root; `useLocale`/`useFilter` read it; `useFilter` produces locale-aware predicates with `Intl.CollatorOptions` sensitivity
- `UNSAFE_PortalProvider`/`useUNSAFE_PortalContext` override the portal container only where a custom stacking context demands it

[STACKING]:
- universal tier `effect`: the `interaction/command.md#COMMAND_SURFACE` `useCommandFilter` lifts `useFilter({ sensitivity: "base" }).contains` into the one scoring primitive that drives BOTH the `cmdk` `filter` prop and the `react-stately` `UNSTABLE_useFilteredListState` view, so the palette and every collection score identically; the sync hook bags need no effect wrapping, while the imperative `announce` seam is folded into `Effect.sync` under a `Match` at `announce.md`
- sibling `react-stately`: the load-bearing pairing — every stateful widget hook takes a `use<Widget>State` object as `state`; `mergeProps` chains the `role.md` `RoleBehavior` props onto the widget bag; `useToast`/`useToastRegion` consume `ToastState<T>`/the `ToastQueue`
- sibling `react-aria-components`: RAC is these hooks pre-composed with render-prop styling; consume raw `react-aria` only for a headless surface RAC does not cover — the `role.md` pattern that owns the behavior and leaves the markup to the consumer
- sibling `@react-aria/live-announcer`: `useToastRegion`/`useLandmark` build managed, widget-owned `aria-live`/landmark regions with F6 navigation; the standalone `announce` (re-exported here at the private path) is the region-less imperative broadcast
- sibling `@radix-ui/react-visually-hidden`: `react-aria`'s `VisuallyHidden`/`useVisuallyHidden` adds `isFocusable` for skip-link reveal; the Radix primitive is the `asChild`-mergeable render primitive; `picker.md` composes `useColorArea`/`useColorWheel` state through `react-stately` color into the OKLCH token space

[RAIL_LAW]:
- package: `react-aria`
- owns: DOM attribute generation, normalized interaction events, ARIA application, focus containment, overlay positioning, dnd coordination, landmark navigation, i18n formatting, prop merge
- accept: state from `react-stately` hooks, refs from the consuming component, an `Intl.CollatorOptions`/locale for i18n, a `FocusScope` boundary for focus
- reject: hand-rolling ARIA attributes, re-implementing pointer/touch normalization, focus trapping outside `FocusScope`, a bespoke substring predicate beside `useFilter`, duplicating `react-stately` state inside a hook call
