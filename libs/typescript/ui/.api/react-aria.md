# [TS_UI_API_REACT_ARIA]

`react-aria` is the headless WAI-ARIA behavior spine: keyboard, focus, pointer/touch normalization, screen-reader semantics, and locale-aware interaction as unstyled hooks that impose zero DOM structure and zero styling. The whole surface is one pattern — `use<Widget>(props, state, ref?) → { <slot>Props }` — where `props` is the `Aria<Widget>Props` contract, `state` is the paired `react-stately` `use<Widget>State` machine, and the return is a record of DOM-attribute bundles (`labelProps`, `inputProps`, `triggerProps`, …) the view row spreads onto its own elements through `mergeProps`. It is a re-export barrel over the scoped `@react-aria/*` packages plus selected `react-stately` state hooks, `@internationalized/date`/`@internationalized/string` value types, and the `@react-types/shared` vocabulary. `react-aria-components` is the pre-styled batteries-included layer above it; `react-stately` is the state layer beneath; `@react-aria/live-announcer` voices its announcements; `@floating-ui/react` positions its overlays; `react-dom` `createPortal` roots them. The `view`, `act`, and `intl` planes compose these hooks directly — the component is the composition, never a wrapper.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-aria`
- package: `react-aria`
- license: `Apache-2.0`
- react-peer: `react catalog`, `react-dom catalog` (consumed against `catalog`; client component — hooks touch `useLayoutEffect`/`document`, so `"use client"`; SSR is `SSRProvider`/`useIsSSR`-gated)
- asset: self-typed ESM+CJS runtime library (`.js` + `.d.ts`, `types: ./dist/types/exports/index.d.ts`); the type-level `Aria*Props`/`*Aria` prop contracts are load-bearing, so `tsc`/`tsgo` is the real gate
- barrel-over: `@react-aria/*` (interactions, focus, i18n, overlays, collections, dnd, and every widget hook), re-exported `react-stately` state hooks (`useCalendarState`, `useDateRangePickerState`, `useRangeCalendarState`, `useTimeFieldState`, `useTooltipTriggerState`), `@internationalized/date`, `@internationalized/string`, `@react-types/shared`
- catalog-verdict: KEEP

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the per-widget aria contract — the barrel's bulk type surface
- rail: view/primitive
- The barrel's real type surface is the per-widget contract, one set per widget across every family: `Aria<Widget>Props`/`Aria<Widget>Options` is the input, `<Widget>Aria` is the record of spreadable DOM-prop bundles the hook returns (`AriaButtonProps`→`ButtonAria`, `AriaTextFieldProps`→`TextFieldAria`, `AriaSelectOptions`→`SelectAria`, `AriaComboBoxOptions`→`ComboBoxAria`). These paired contracts are what a `view` row types against.

| [INDEX] | [SYMBOL]                                                             | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                       |
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

[PUBLIC_TYPE_SCOPE]: the re-exported shared + i18n vocabulary the hook signatures reference
- rail: act/gesture + intl/format
- react-aria surfaces a curated slice directly (`Key`, `RangeValue`, `FocusableProps`, `Locale`, `DateFormatter`, `DateValue`, `Filter`, `LocalizedStringFormatter`); the deeper vocabulary the hook signatures reference is imported from the re-exported type packages `@react-types/shared` and `@internationalized/date`, not the `react-aria` barrel. Rows 02-06 resolve from `@react-types/shared`; row 01 and the i18n values (07-09) are barrel-surfaced; the date values (10) are `@internationalized/date`.

| [INDEX] | [SYMBOL]                                                             | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                           |
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

[ENTRYPOINT_SCOPE]: the aria-hook pattern — one shape, every widget
- rail: view/primitive
- The single mechanism: pass the widget props and the `react-stately` state, receive DOM-prop bundles. Widget growth is a new `use<Widget>` row over its `use<Widget>State`, never a new abstraction. The families below are the same call shape grouped by concern; each maps to a `ui` plane row.

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                          |
| :-----: | :--------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `use<Widget>(props, state, ref?) → { <slot>Props }`                    | the pattern    | every hook `(props, state, ref)` → bundles   |
|  [02]   | `useButton` / `useToggleButton` / `useLink`                            | field/action   | button/link action rows                      |
|  [03]   | `useTextField` / `useSearchField` / `useNumberField`                   | field/action   | text/search/number field rows                |
|  [04]   | `useCheckbox` / `useCheckboxGroup` / `useRadioGroup`                   | choice         | checkbox/radio over toggle/radio-group state |
|  [05]   | `useSwitch` / `useSlider` / `useSliderThumb`                           | range          | switch + slider thumb over slider state      |
|  [06]   | `useSelect` / `useComboBox` / `useListBox` / `useOption`               | picker         | select/combobox/listbox rows                 |
|  [07]   | `HiddenSelect`                                                         | picker         | native-form-participation shadow input       |
|  [08]   | `useMenu` / `useMenuTrigger` / `useMenuItem` / `useSubmenuTrigger`     | menu           | menu + submenu over menu-trigger state       |
|  [09]   | `useToolbar` / `useTabList` / `useTab`                                 | command        | toolbar + tabs over tab-list state           |
|  [10]   | `useTable` / `useGridList` / `useTree`                                 | collection     | table/grid/tree over collection state        |
|  [11]   | `useListBox` / `useTagGroup` / `use*SelectionCheckbox`                 | collection     | listbox/tag-group + selection checkbox       |
|  [12]   | `useCalendar` / `useRangeCalendar` / `useDateField` / `useDateSegment` | date/time      | calendar + date-field + segment editing      |
|  [13]   | `useDatePicker` / `useDateRangePicker` / `useTimeField`                | date/time      | picker/range over `@internationalized/date`  |
|  [14]   | `useColorArea` / `useColorField` / `useColorSlider`                    | color          | color area/field/slider rows                 |
|  [15]   | `useColorWheel` / `useColorSwatch` / `useColorChannelField`            | color          | wheel/swatch/channel; composes `colorjs.io`  |
|  [16]   | `useOverlay` / `useOverlayTrigger` / `useOverlayPosition`              | overlay        | overlay trigger + positioning                |
|  [17]   | `usePopover` / `useModalOverlay` / `useModal` / `usePreventScroll`     | overlay        | popover/modal + scroll lock                  |
|  [18]   | `DismissButton` / `Overlay` / `PortalProvider`                         | overlay        | root through `react-dom` `createPortal`      |
|  [19]   | `useTooltip` / `useTooltipTrigger` / `useDialog` / `useDisclosure`     | disclosure     | tooltip/dialog/disclosure rows               |
|  [20]   | `useToast` / `useToastRegion`                                          | disclosure     | toast; via `@react-aria/live-announcer`      |
|  [21]   | `usePress` / `useHover` / `useMove` / `useLongPress`                   | interaction    | normalized pointer/press events              |
|  [22]   | `useKeyboard` / `useInteractOutside` / `useScrollWheel`                | interaction    | keyboard + outside/scroll events             |
|  [23]   | `useFocus` / `useFocusVisible` / `useFocusWithin`                      | focus          | focus event hooks                            |
|  [24]   | `useFocusRing` / `useFocusManager` / `FocusScope`                      | focus          | `FocusScope` traps/restores focus            |
|  [25]   | `FocusRing` / `Focusable` / `Pressable`                                | focus          | focus-styling + focusable/pressable wrappers |
|  [26]   | `useDrag` / `useDrop` / `useClipboard`                                 | dnd            | drag/drop + clipboard copy/paste             |
|  [27]   | `useDraggableCollection` / `useDroppableCollection`                    | dnd            | collection drag-drop reorder                 |
|  [28]   | `ListDropTargetDelegate` / `DragPreview`                               | dnd            | drop-target delegate + drag image            |
|  [29]   | `I18nProvider` / `useLocale`                                           | i18n           | sets + reads locale context                  |
|  [30]   | `useDateFormatter` / `useNumberFormatter` / `useListFormatter`         | i18n           | cached `Intl.*` formatters over `useLocale`  |
|  [31]   | `useCollator` / `useFilter` / `useLocalizedStringFormatter`            | i18n           | locale-aware collate/filter + string format  |
|  [32]   | `mergeProps` / `chain` / `useId`                                       | composition    | `mergeProps` folds N bundles per element     |
|  [33]   | `useObjectRef` / `mergeRefs`                                           | composition    | ref reconciliation                           |
|  [34]   | `VisuallyHidden` / `useVisuallyHidden`                                 | composition    | visually-hidden content                      |
|  [35]   | `RouterProvider` / `SSRProvider` / `useIsSSR`                          | context        | router `href` + SSR hydration gates          |

## [04]-[IMPLEMENTATION_LAW]

[ARIA_HOOK_TOPOLOGY]:
- One pattern owns the whole surface: `use<Widget>(props, state, ref?)` takes the `Aria<Widget>Props` contract and the paired `react-stately` `use<Widget>State` machine and returns a record of `DOMAttributes` bundles. The view row owns the elements and the styling; the hook owns only behavior, semantics, keyboard, and focus. A new widget is a new `use<Widget>`/`use<Widget>State` pair, never a styled component or a config flag.
- State is external and lives in `react-stately`: `react-aria` hooks are stateless behavior over a state object the caller constructs (`useSelectState`, `useTableState`, `useOverlayTriggerState`). The barrel re-exports only the date/time/tooltip state hooks whose state is inseparable from the widget; every other `use*State` is imported from the `react-stately` sibling. This split is why one hook serves controlled and uncontrolled modes — the modality is a state-construction argument, never a second hook.
- Composition is `mergeProps` + `chain`: multiple hooks contribute to one element (`useButton` + `useHover` + `useFocusRing` on one `<button>`); `mergeProps` folds their bundles — chaining event handlers, merging `className`/`id`/`style`, unioning aria attributes — so no element is owned by one hook. `useObjectRef`/`mergeRefs` reconcile the refs the hooks and the caller both need.
- Interaction events are cross-input-normalized: `usePress`/`useHover`/`useMove` emit `PressEvent`/`HoverEvent`/`MoveEvent` that unify mouse, touch, pen, keyboard, and virtual (screen-reader) cursors, resolve tap-vs-scroll ambiguity, and suppress synthetic double-fires — the reason `act/gesture` never binds raw DOM listeners.
- i18n is native `Intl` under cached, locale-keyed formatters: `useDateFormatter`/`useNumberFormatter`/`useCollator`/`useListFormatter` return memoized `Intl.*` instances scoped to the `I18nProvider` locale; `@internationalized/date` supplies immutable, calendar-system-correct date values. Zero i18n runtime dependency — the `intl` plane's own message catalogs stay app data.

[STACKS_WITH]:
- `react-stately` (`.api/react-stately.md`): the state layer beneath every hook — `use<Widget>State` is the first-class argument to `use<Widget>`. Selection (`useMultipleSelectionState`) and collections (`useListState`/`useTableState`/`useTreeState`) are constructed there and threaded in as `state`; form validity is the exception — a field hook is `use<Widget>(props, ref)` and builds its validity internally via `@react-stately/form`'s `useFormValidationState`, reading the umbrella-exported `FormValidationContext` for injected errors. The two packages are one widget split across behavior and state.
- `react-aria-components` (`.api/react-aria-components.md`): the pre-composed styled layer above — it internalizes this package (`react-aria@catalog`) as its behavior/ARIA hook spine, wires the hooks into ready `<Select>`/`<Table>`/`<DatePicker>` render-prop components, and re-exports this barrel's infra directly (`I18nProvider`/`useLocale`/`isRTL`/`useFilter`/`VisuallyHidden`/`RouterProvider`/`SSRProvider`). A `view` row uses the components for standard widgets and drops to the raw hooks only when a bespoke DOM structure or a non-standard composition is required; `tailwindcss-react-aria-components` styles the components' data-attribute states.
- `@react-aria/live-announcer` (`.api/react-aria-live-announcer.md`): this package OWNS the live-region singleton — the private `react-aria/private/live-announcer/LiveAnnouncer` that `useToast`/`useToastRegion` and collection announcements drive internally; the sibling `@react-aria/live-announcer` merely re-exports its `announce`/`clearAnnouncer`/`destroyAnnouncer` from that private path. One visually-hidden `aria-live` element, `announce(message, assertiveness)` the single entry.
- `@floating-ui/react` / `@floating-ui/react-dom` (`.api/floating-ui-react.md`): the positioning split — `react-aria` owns the overlay behavior (`useOverlayTrigger`/`usePopover`/`useModalOverlay`, dismiss, focus trap, scroll lock) and the WAI-ARIA semantics; `@floating-ui` owns the geometry (anchor tracking via `autoUpdate`, `offset`/`flip`/`shift`/`size` middleware) where `useOverlayPosition` is insufficient, plus the interaction primitives react-aria leaves headless — `safePolygon` hover-intent and `useListNavigation` virtual focus. One element has ONE semantic owner: a row takes react-aria's overlay hooks OR floating-ui's `useRole`+`useDismiss`+`FloatingFocusManager`, never both, and always composes `useFloating` for position. They meet at the overlay element: aria props from one, position style from the other, merged by `mergeProps`; floating-ui's `useMergeRefs` bridges the two ref systems (react-aria's `useObjectRef`/`mergeRefs` ref and floating-ui's) onto the shared node.
- `react-dom` (`.api/react-dom.md`): overlays root through `createPortal` (via `Overlay`/`PortalProvider`) so they escape the DOM subtree and `overflow`/`z-index` context; `flushSync` forces the synchronous commit `FocusScope` restoration and the `act/transition` View-Transition seam depend on.
- `effect` `Schema` (`libs/typescript/.api/effect.md`): the `FormBinding` boundary — a `Schema.standardSchemaV1` decoder validates field input and its `ParseError` projects into the hook `ValidationResult`/`validationErrors`, so one `Schema` owns both wire decode and live field validity with no parallel validator.
- `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): the data seam — an atom's `Effect`-derived value feeds the `react-stately` state (options, selected keys, table rows): `useAtomValue` drives the stately hook's controlled value and a `react-aria` `onChange`/selection callback calls the atom setter (`useAtomSet`) to write the intent back. The atom is the one binding; the hook is stateless over its value.

[LOCAL_ADMISSION]:
- Compose the hooks directly in `view`/`act`/`intl` rows; never hand-write `role`/`aria-*`/`tabindex`/keyboard handlers or a focus trap — the hook owns WAI-ARIA semantics and the `mergeProps` bundle is the only interaction surface.
- Construct widget state through `react-stately` `use<Widget>State` and thread it in; never store selection, expansion, or overlay-open state in ad-hoc `useState` beside a hook that already models it.
- Merge multi-hook elements with `mergeProps`/`chain`; never manually concatenate handlers or spread bundles in declaration order — handler chaining and aria-union are `mergeProps`' contract.
- Localize through `I18nProvider` + the formatter hooks and `@internationalized/date` values; never instantiate `Intl.DateTimeFormat`/`new Date()` at a call site or store wall-clock dates as strings.
- Gate SSR-unsafe reads behind `useIsSSR`/`SSRProvider`; never branch on `typeof window` inside a row.

[RAIL_LAW]:
- Package: `react-aria`
- Owns: the headless WAI-ARIA hook spine — one `use<Widget>(props, state, ref)` pattern across interactions, focus, collections, fields, pickers, overlays, date/time, color, and dnd; the per-widget `Aria<Widget>Props`/`<Widget>Aria` contracts; the `mergeProps`/`chain` composition mechanism; `I18nProvider` + the native-`Intl` formatter hooks over `@internationalized/date`/`/string`; and the curated re-export of the `@react-types/shared` interaction vocabulary
- Accept: `use<Widget>` over a `react-stately` state object, `mergeProps` for multi-hook elements, `useObjectRef`/`mergeRefs` for ref reconciliation, `Overlay`/`PortalProvider` through `react-dom` `createPortal`, `useOverlayPosition` or `@floating-ui` for positioning, `ValidationResult` fed by a `Schema` decode, `I18nProvider` + formatter hooks for locale, `useIsSSR` for hydration safety
- Reject: hand-written `aria-*`/`role`/keyboard/focus-trap logic, ad-hoc `useState` beside a modeled widget state, manual handler concatenation instead of `mergeProps`, `Intl.*`/`new Date()` at a call site, `typeof window` SSR branches, a styled wrapper where `react-aria-components` already owns the composition
