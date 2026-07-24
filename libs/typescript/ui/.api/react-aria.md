# [TS_UI_API_REACT_ARIA]

`react-aria` mints the headless WAI-ARIA behavior spine — keyboard, focus, pointer/touch normalization, screen-reader semantics, and locale-aware interaction as unstyled hooks imposing zero DOM and zero styling. One pattern owns the whole surface: `use<Widget>(props, state, ref?)` folds the `Aria<Widget>Props` contract and the paired `react-stately` `use<Widget>State` into `DOMAttributes` bundles a `view` row spreads through `mergeProps`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-aria`
- package: `react-aria` (Apache-2.0)
- module: ESM+CJS, self-typed; barrels the scoped `@react-aria/*` hooks, selected `react-stately` state hooks, and the `@internationalized/date`/`/string` + `@react-types/shared` type packages
- runtime: client component — hooks touch `useLayoutEffect`/`document`, so `"use client"`; SSR gates behind `SSRProvider`/`useIsSSR`; peer `react`/`react-dom`
- asset: the type-level `Aria<Widget>Props`/`<Widget>Aria` prop contracts type every hook call, so `tsc` is the gate
- rail: the WAI-ARIA behavior spine the `view`/`act`/`intl` planes compose directly

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the per-widget contract — `Aria<Widget>Props`/`Aria<Widget>Options` the hook input, `<Widget>Aria` the spreadable-bundle record it returns (`AriaButtonProps`→`ButtonAria`, `AriaSelectOptions`→`SelectAria`), one pair per widget a `view` row types against.

| [INDEX] | [SYMBOL]                                                             | [TYPE_FAMILY]    | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------- | :--------------- | :---------------------------------------- |
|  [01]   | `Aria<Widget>Props` / `Aria<Widget>Options` / `<Widget>Aria`         | widget contract  | one input→output pair per widget          |
|  [02]   | `FocusManager` / `FocusManagerOptions`                               | focus control    | `FocusScope` imperative traversal         |
|  [03]   | `FocusScopeProps` / `FocusRingAria`                                  | focus control    | `useFocusRing` `isFocusVisible`           |
|  [04]   | `PositionAria` / `Placement` / `PlacementAxis` / `Axis` / `SizeAxis` | overlay geometry | `useOverlayPosition` resolved geometry    |
|  [05]   | `PressEvent` / `PressResult`                                         | interaction      | `usePress` return, cross-input-normalized |
|  [06]   | `HoverEvent` / `HoverResult`                                         | interaction      | `useHover` return                         |
|  [07]   | `MoveEvent` / `MoveResult`                                           | interaction      | `useMove` return                          |
|  [08]   | `LongPressEvent` / `KeyboardResult`                                  | interaction      | `useLongPress`/`useKeyboard` return       |
|  [09]   | `DragItem` / `DropTarget` / `DropOperation`                          | dnd value        | `DropOperation` = `move`/`copy`/`link`    |
|  [10]   | `DragTypes` / `DragResult` / `DropResult`                            | dnd value        | drag/drop payload + result                |

[PUBLIC_TYPE_SCOPE]: the shared + i18n vocabulary the hook signatures reference — `Key`/`RangeValue`/`Locale`/`DateFormatter`/`Filter` barrel-surface directly, the prop/collection/field vocab (`DOMAttributes`/`Selection`/`Node`/`ValidationResult`) resolves from `@react-types/shared`, and the calendar values (`CalendarDate`/`ZonedDateTime`) from `@internationalized/date`.

| [INDEX] | [SYMBOL]                                                             | [TYPE_FAMILY]     | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------- | :---------------- | :-------------------------------------------- |
|  [01]   | `Key` / `Orientation` / `RangeValue<T>` / `FocusableProps`           | shared primitive  | key/axis/`{start,end}`-range/focusable mixins |
|  [02]   | `DOMAttributes` / `DOMProps`                                         | prop/element base | the spread-bundle shapes                      |
|  [03]   | `AriaLabelingProps` / `FocusableElement`                             | prop/element base | labeling + focusable-element base             |
|  [04]   | `Selection` / `Node<T>` / `Collection<T>` / `LayoutDelegate`         | collection vocab  | `Set`-of-`Key` selection, node/tree, layout   |
|  [05]   | `ValidationResult` / `ValidationState` / `InputBase`                 | field vocab       | validity state + input base                   |
|  [06]   | `LabelableProps` / `HelpTextProps`                                   | field vocab       | label + help-text field mixins                |
|  [07]   | `Locale` / `DateFormatter` / `DateValue` / `DateRange` / `TimeValue` | i18n value        | date/time formatter values                    |
|  [08]   | `Filter` / `LocalizedStringFormatter`                                | i18n value        | `useFilter`/`useCollator` string helpers      |
|  [09]   | `Intl.NumberFormat` / `Intl.Collator` / `Intl.ListFormat`            | i18n value        | native returns of the formatter hooks         |
|  [10]   | `CalendarDate` / `CalendarDateTime` / `ZonedDateTime` / `Time`       | date value        | immutable calendar-aware `DateValue` targets  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one call shape, every widget — the families below group `use<Widget>(props, state, ref?)` rows by concern, and a new widget is a new `use<Widget>`/`use<Widget>State` row, never a new abstraction.

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `use<Widget>(props, state, ref?) → { <slot>Props }`                    | the pattern    | every hook `(props, state, ref)` → bundles    |
|  [02]   | `useButton` / `useToggleButton` / `useLink`                            | field/action   | button/link action rows                       |
|  [03]   | `useTextField` / `useSearchField` / `useNumberField`                   | field/action   | text/search/number field rows                 |
|  [04]   | `useCheckbox` / `useCheckboxGroup` / `useRadioGroup`                   | choice         | checkbox/radio over toggle/radio-group state  |
|  [05]   | `useSwitch` / `useSlider` / `useSliderThumb`                           | range          | switch + slider thumb over slider state       |
|  [06]   | `useSelect` / `useComboBox` / `useListBox` / `useOption`               | picker         | select/combobox/listbox rows                  |
|  [07]   | `HiddenSelect`                                                         | picker         | native-form-participation shadow input        |
|  [08]   | `useMenu` / `useMenuTrigger` / `useMenuItem` / `useSubmenuTrigger`     | menu           | menu + submenu over menu-trigger state        |
|  [09]   | `useToolbar` / `useTabList` / `useTab`                                 | command        | toolbar + tabs over tab-list state            |
|  [10]   | `useTable` / `useGridList` / `useTree`                                 | collection     | table/grid/tree over collection state         |
|  [11]   | `useListBox` / `useTagGroup` / `use*SelectionCheckbox`                 | collection     | listbox/tag-group + selection checkbox        |
|  [12]   | `useCalendar` / `useRangeCalendar` / `useDateField` / `useDateSegment` | date/time      | calendar + date-field + segment editing       |
|  [13]   | `useDatePicker` / `useDateRangePicker` / `useTimeField`                | date/time      | picker/range over `@internationalized/date`   |
|  [14]   | `useColorArea` / `useColorField` / `useColorSlider`                    | color          | color area/field/slider rows                  |
|  [15]   | `useColorWheel` / `useColorSwatch` / `useColorChannelField`            | color          | wheel/swatch/channel; composes `colorjs.io`   |
|  [16]   | `useOverlay` / `useOverlayTrigger` / `useOverlayPosition`              | overlay        | overlay trigger + positioning                 |
|  [17]   | `usePopover` / `useModalOverlay` / `useModal` / `usePreventScroll`     | overlay        | popover/modal + scroll lock                   |
|  [18]   | `DismissButton` / `Overlay` / `UNSAFE_PortalProvider`                  | overlay        | root through `react-dom` `createPortal`       |
|  [19]   | `useTooltip` / `useTooltipTrigger` / `useDialog` / `useDisclosure`     | disclosure     | tooltip/dialog/disclosure rows                |
|  [20]   | `useToast` / `useToastRegion`                                          | disclosure     | toast; via `@react-aria/live-announcer`       |
|  [21]   | `usePress` / `useHover` / `useMove` / `useLongPress`                   | interaction    | normalized pointer/press events               |
|  [22]   | `useKeyboard` / `useInteractOutside`                                   | interaction    | keyboard event hook + outside-press detection |
|  [23]   | `useFocus` / `useFocusVisible` / `useFocusWithin`                      | focus          | focus event hooks                             |
|  [24]   | `useFocusRing` / `useFocusManager` / `FocusScope`                      | focus          | `FocusScope` traps/restores focus             |
|  [25]   | `FocusRing` / `Focusable` / `Pressable`                                | focus          | focus-styling + focusable/pressable wrappers  |
|  [26]   | `useDrag` / `useDrop` / `useClipboard`                                 | dnd            | drag/drop + clipboard copy/paste              |
|  [27]   | `useDraggableCollection` / `useDroppableCollection`                    | dnd            | collection drag-drop reorder                  |
|  [28]   | `ListDropTargetDelegate` / `DragPreview`                               | dnd            | drop-target delegate + drag image             |
|  [29]   | `I18nProvider` / `useLocale`                                           | i18n           | sets + reads locale context                   |
|  [30]   | `useDateFormatter` / `useNumberFormatter` / `useListFormatter`         | i18n           | cached `Intl.*` formatters over `useLocale`   |
|  [31]   | `useCollator` / `useFilter` / `useLocalizedStringFormatter`            | i18n           | locale-aware collate/filter + string format   |
|  [32]   | `mergeProps` / `chain` / `useId`                                       | composition    | `mergeProps` folds N bundles per element      |
|  [33]   | `useObjectRef` / `mergeRefs`                                           | composition    | ref reconciliation                            |
|  [34]   | `VisuallyHidden` / `useVisuallyHidden`                                 | composition    | visually-hidden content                       |
|  [35]   | `RouterProvider` / `SSRProvider` / `useIsSSR`                          | context        | router `href` + SSR hydration gates           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each `use<Widget>` owns behavior, semantics, keyboard, and focus; the `view` row owns elements and styling — a new widget is a new `use<Widget>`/`use<Widget>State` pair, never a styled component or config flag.
- State lives in `react-stately` and the hook is stateless over it, so one hook serves controlled and uncontrolled modes: the modality is a state-construction argument, never a second hook.
- `mergeProps`/`chain` fold multiple hooks onto one element — chaining handlers, merging `className`/`id`/`style`, unioning aria attributes — so no element is owned by one hook; `useObjectRef`/`mergeRefs` reconcile the shared refs.
- `usePress`/`useHover`/`useMove` emit events unifying mouse, touch, pen, keyboard, and virtual cursors, resolving tap-vs-scroll and suppressing synthetic double-fires — why `act/gesture` binds no raw DOM listener.
- `useDateFormatter`/`useNumberFormatter`/`useCollator`/`useListFormatter` return memoized `Intl.*` instances keyed to the `I18nProvider` locale over immutable `@internationalized/date` values, so the surface carries zero i18n runtime dependency.

[STACKING]:
- `react-stately` (`.api/react-stately.md`): the state layer — `use<Widget>State` is the first-class `state` argument to `use<Widget>`, and selection (`useMultipleSelectionState`) and collections (`useListState`/`useTableState`/`useTreeState`) construct there and thread in. Date/time/tooltip state hooks re-export from the barrel, their state inseparable from the widget; every other `use*State` imports from the sibling. Field validity is the exception — a field hook is `use<Widget>(props, ref)` building validity internally via `@react-stately/form`'s `useFormValidationState` over the umbrella-exported `FormValidationContext`.
- `react-aria-components` (`.api/react-aria-components.md`): the styled layer above — it internalizes this barrel as its behavior/ARIA spine, wires the hooks into ready `<Select>`/`<Table>`/`<DatePicker>` render-prop components, and re-exports this barrel's infra (`I18nProvider`/`useLocale`/`isRTL`/`useFilter`/`VisuallyHidden`/`RouterProvider`/`SSRProvider`). `view` rows drop to the raw hooks only for a bespoke DOM structure or non-standard composition, styling the components' data-attribute states through `tailwindcss-react-aria-components`.
- `@react-aria/live-announcer` (`.api/react-aria-live-announcer.md`): this barrel OWNS the live-region singleton driving `useToast`/`useToastRegion` and collection announcements; the sibling re-exports its `announce`/`clearAnnouncer`/`destroyAnnouncer`. One visually-hidden `aria-live` element, `announce(message, assertiveness)` the single entry.
- `@floating-ui/react` (`.api/floating-ui-react.md`): the positioning split — `react-aria` owns overlay behavior (`useOverlayTrigger`/`usePopover`/`useModalOverlay`, dismiss, focus trap, scroll lock) and WAI-ARIA semantics; `@floating-ui` owns geometry (anchor tracking via `autoUpdate`, `offset`/`flip`/`shift`/`size` middleware) where `useOverlayPosition` is insufficient, with `safePolygon` hover-intent and `useListNavigation` virtual focus. One element takes react-aria's overlay hooks OR floating-ui's `useRole`+`useDismiss`+`FloatingFocusManager`, never both, always composing `useFloating` for position; `mergeProps` merges the aria and position bundles and floating-ui's `useMergeRefs` bridges the two ref systems onto the shared node.
- `react-dom` (`.api/react-dom.md`): overlays root through `createPortal` (via `Overlay`/`UNSAFE_PortalProvider`) to escape the `overflow`/`z-index` context; `flushSync` forces the synchronous commit `FocusScope` restoration and the `act/transition` View-Transition seam depend on.
- `effect` `Schema` (`libs/typescript/.api/effect.md`): the `FormBinding` boundary — a `Schema.standardSchemaV1` decoder validates field input and its `ParseError` projects into the hook `ValidationResult`/`validationErrors`, one `Schema` owning wire decode and live field validity.
- `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): the data seam — `useAtomValue` drives the `react-stately` controlled value (options, selected keys, rows) and a `react-aria` `onChange`/selection callback writes intent back via `useAtomSet`; the atom is the one binding, the hook stateless over its value.

[RAIL_LAW]:
- Package: `react-aria`
- Owns: the headless WAI-ARIA hook spine — one `use<Widget>(props, state, ref)` pattern across interactions, focus, collections, fields, pickers, overlays, date/time, color, and dnd; the per-widget `Aria<Widget>Props`/`<Widget>Aria` contracts; the `mergeProps`/`chain` composition mechanism; `I18nProvider` + the native-`Intl` formatter hooks over `@internationalized/date`/`/string`; and the curated re-export of the `@react-types/shared` interaction vocabulary
- Accept: `use<Widget>` over a `react-stately` state object, `mergeProps` for multi-hook elements, `useObjectRef`/`mergeRefs` for ref reconciliation, `Overlay`/`UNSAFE_PortalProvider` through `react-dom` `createPortal`, `useOverlayPosition` or `@floating-ui` for positioning, `ValidationResult` fed by a `Schema` decode, `I18nProvider` + formatter hooks for locale, `useIsSSR` for hydration safety
- Reject: hand-written `aria-*`/`role`/keyboard/focus-trap logic, ad-hoc `useState` beside a modeled widget state, manual handler concatenation instead of `mergeProps`, `Intl.*`/`new Date()` at a call site, `typeof window` SSR branches, a styled wrapper where `react-aria-components` already owns the composition
