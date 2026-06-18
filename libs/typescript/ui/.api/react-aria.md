# [API_CATALOGUE] react-aria

`react-aria` supplies headless ARIA-compliant interaction hooks that produce `buttonProps`, `pressProps`, `hoverProps`, `focusProps`, and DOM attribute bags for every standard widget family. Consuming owners spread the returned props onto their own markup, keeping styling and accessibility behaviour fully decoupled. The package also re-exports `FocusScope`, `FocusRing`, collection primitives, overlay position utilities, i18n helpers, and DnD coordination hooks for the AppUi interaction spine.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-aria`
- package: `react-aria`
- module: `react-aria` (main); `react-aria/i18n`, `react-aria/private/live-announcer/LiveAnnouncer` (sub-paths)
- namespace: `react-aria`
- asset: runtime hook library
- rail: interaction / accessibility

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: interaction result types
- rail: interaction

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]    | [RAIL]                         |
| :-----: | :-------------- | :--------------- | :----------------------------- |
|   [1]   | `ButtonAria<T>` | result interface | `buttonProps: T`, `isPressed`  |
|   [2]   | `PressResult`   | result interface | `pressProps`, `isPressed`      |
|   [3]   | `HoverResult`   | result interface | `hoverProps`, `isHovered`      |
|   [4]   | `FocusRingAria` | result interface | `focusProps`, focus visibility |

[PUBLIC_TYPE_SCOPE]: interaction input props
- rail: interaction

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]     | [RAIL]                             |
| :-----: | :--------------------- | :---------------- | :--------------------------------- |
|   [1]   | `ButtonProps`          | props interface   | press, disabled, children          |
|   [2]   | `AriaButtonProps<T>`   | props interface   | element-typed overload             |
|   [3]   | `AriaButtonOptions<E>` | options interface | omits children, element type       |
|   [4]   | `AriaBaseButtonProps`  | props interface   | aria-* attributes + form attrs     |
|   [5]   | `PressProps`           | props interface   | `PressEvents`, cancel-on-exit      |
|   [6]   | `PressHookProps`       | props interface   | `PressProps` + `ref`               |
|   [7]   | `HoverProps`           | props interface   | `HoverEvents`, `isDisabled`        |
|   [8]   | `AriaFocusRingProps`   | props interface   | `within`, `isTextInput`, autoFocus |

[PUBLIC_TYPE_SCOPE]: focus management types
- rail: focus

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [RAIL]                                 |
| :-----: | :-------------------- | :---------------- | :------------------------------------- |
|   [1]   | `FocusScopeProps`     | props interface   | `contain`, `restoreFocus`, `autoFocus` |
|   [2]   | `FocusManager`        | interface         | `focusNext/Previous/First/Last`        |
|   [3]   | `FocusManagerOptions` | options interface | `from`, `tabbable`, `wrap`, `accept`   |

[PUBLIC_TYPE_SCOPE]: i18n types
- rail: i18n

| [INDEX] | [SYMBOL] | [TYPE_FAMILY]    | [RAIL]                               |
| :-----: | :------- | :--------------- | :----------------------------------- |
|   [1]   | `Filter` | result interface | `startsWith`, `endsWith`, `contains` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: button and toggle hooks
- rail: interaction

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :-------------------------------------------- | :------------- | :------------------------- |
|   [1]   | `useButton(props, ref)`                       | ARIA hook      | element-typed button props |
|   [2]   | `useToggleButton(props, state, ref)`          | ARIA hook      | aria-pressed toggle props  |
|   [3]   | `useToggleButtonGroup(props, ref)`            | ARIA hook      | group ARIA props           |
|   [4]   | `useToggleButtonGroupItem(props, state, ref)` | ARIA hook      | item within toggle group   |

[ENTRYPOINT_SCOPE]: press, hover, keyboard, move interaction hooks
- rail: interaction

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY]   | [RAIL]                       |
| :-----: | :------------------------------- | :--------------- | :--------------------------- |
|   [1]   | `usePress(props)`                | interaction hook | normalised pointer/key press |
|   [2]   | `useHover(props)`                | interaction hook | hover sans touch emulation   |
|   [3]   | `useKeyboard(props)`             | interaction hook | keyboard event delegation    |
|   [4]   | `useLongPress(props)`            | interaction hook | long-press detection         |
|   [5]   | `useMove(props)`                 | interaction hook | pointer/keyboard move events |
|   [6]   | `useFocus(props)`                | interaction hook | focus event normalisation    |
|   [7]   | `useFocusVisible(props?)`        | interaction hook | keyboard-focus visibility    |
|   [8]   | `useFocusWithin(props)`          | interaction hook | focus within containment     |
|   [9]   | `useInteractOutside(props, ref)` | interaction hook | outside-click detection      |

[ENTRYPOINT_SCOPE]: focus management hooks and components
- rail: focus

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :------------------------- | :------------- | :-------------------------------- |
|   [1]   | `FocusScope(props)`        | component      | contain / restore focus scope     |
|   [2]   | `useFocusManager()`        | hook           | returns `FocusManager` from scope |
|   [3]   | `FocusRing(props)`         | component      | visible focus ring wrapper        |
|   [4]   | `useFocusRing(props?)`     | hook           | `isFocused`, `isFocusVisible`     |
|   [5]   | `useFocusable(props, ref)` | hook           | spread focusable props            |
|   [6]   | `Focusable`                | component      | imperative `.focus()` wrapper     |

[ENTRYPOINT_SCOPE]: listbox, gridlist, menu hooks
- rail: collection

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :------------------------------------- | :------------- | :------------------------- |
|   [1]   | `useListBox(props, state, ref)`        | ARIA hook      | listbox ARIA attrs         |
|   [2]   | `useOption(props, state, ref)`         | ARIA hook      | option ARIA attrs          |
|   [3]   | `useListBoxSection(props)`             | ARIA hook      | section ARIA attrs         |
|   [4]   | `useGridList(props, state, ref)`       | ARIA hook      | gridlist ARIA attrs        |
|   [5]   | `useGridListItem(props, state, ref)`   | ARIA hook      | row/item ARIA attrs        |
|   [6]   | `useMenu(props, state, ref)`           | ARIA hook      | menu ARIA attrs            |
|   [7]   | `useMenuItem(props, state, ref)`       | ARIA hook      | menuitem ARIA attrs        |
|   [8]   | `useMenuTrigger(props, state, ref)`    | ARIA hook      | trigger ARIA attrs         |
|   [9]   | `useSubmenuTrigger(props, state, ref)` | ARIA hook      | submenu trigger ARIA attrs |

[ENTRYPOINT_SCOPE]: select, combobox, text field hooks
- rail: form

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :----------------------------------------------------------------------- | :------------- | :----------------------------- |
|   [1]   | `useSelect(props, state, ref)`                                           | ARIA hook      | select ARIA attrs              |
|   [2]   | `useComboBox(props, state, inputRef, buttonRef, listBoxRef, popoverRef)` | ARIA hook      | combobox ARIA attrs            |
|   [3]   | `useTextField(props, ref)`                                               | ARIA hook      | input / textarea ARIA attrs    |
|   [4]   | `useSearchField(props, state, ref)`                                      | ARIA hook      | search field ARIA attrs        |
|   [5]   | `useNumberField(props, state, ref)`                                      | ARIA hook      | number field ARIA attrs        |
|   [6]   | `useField(props)`                                                        | ARIA hook      | label + description ARIA attrs |
|   [7]   | `useLabel(props, ref?)`                                                  | ARIA hook      | label element ARIA attrs       |

[ENTRYPOINT_SCOPE]: overlay and dialog hooks
- rail: overlay

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :------------------------------------- | :------------- | :---------------------------------------- |
|   [1]   | `useOverlay(props, ref)`               | ARIA hook      | overlay ARIA attrs                        |
|   [2]   | `useOverlayTrigger(type, state, ref?)` | ARIA hook      | trigger button ARIA attrs                 |
|   [3]   | `useOverlayPosition(props)`            | position hook  | `overlayProps`, `arrowProps`, `placement` |
|   [4]   | `useModalOverlay(props, state, ref)`   | ARIA hook      | modal overlay ARIA attrs                  |
|   [5]   | `usePopover(props, state, ref)`        | ARIA hook      | popover ARIA attrs                        |
|   [6]   | `useDialog(props, ref)`                | ARIA hook      | dialog ARIA attrs                         |
|   [7]   | `usePreventScroll(options?)`           | utility hook   | body scroll lock                          |
|   [8]   | `DismissButton`                        | component      | screen-reader dismiss target              |
|   [9]   | `Overlay`                              | component      | portal host wrapper                       |

[ENTRYPOINT_SCOPE]: i18n hooks
- rail: i18n

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :--------------------------------------------------- | :------------- | :----------------------------- |
|   [1]   | `useLocale()`                                        | hook           | `{ locale, direction }`        |
|   [2]   | `useFilter(options?)`                                | hook           | `Filter` string search         |
|   [3]   | `useCollator(options?)`                              | hook           | `Intl.Collator` instance       |
|   [4]   | `useDateFormatter(options?)`                         | hook           | `Intl.DateTimeFormat` instance |
|   [5]   | `useNumberFormatter(options?)`                       | hook           | `Intl.NumberFormat` instance   |
|   [6]   | `useListFormatter(options?)`                         | hook           | `Intl.ListFormat` instance     |
|   [7]   | `useLocalizedStringFormatter(strings, packageName?)` | hook           | localised string map           |
|   [8]   | `I18nProvider`                                       | component      | locale context provider        |
|   [9]   | `isRTL(locale)`                                      | utility        | boolean RTL test               |

[ENTRYPOINT_SCOPE]: utility exports
- rail: interaction

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :--------------------------- | :------------- | :------------------------------- |
|   [1]   | `mergeProps(...args)`        | utility        | prop merge with chained handlers |
|   [2]   | `mergeRefs(...refs)`         | utility        | ref merge                        |
|   [3]   | `useId(defaultId?)`          | utility        | stable cross-component ID        |
|   [4]   | `useObjectRef(forwardedRef)` | utility        | `RefObject` from `ForwardedRef`  |
|   [5]   | `VisuallyHidden`             | component      | invisible-but-accessible wrapper |
|   [6]   | `useVisuallyHidden(props?)`  | hook           | visually-hidden CSS props        |
|   [7]   | `RouterProvider`             | component      | link-navigation context          |
|   [8]   | `SSRProvider`                | component      | SSR ID stability provider        |
|   [9]   | `useIsSSR()`                 | hook           | boolean SSR detection            |

## [4]-[IMPLEMENTATION_LAW]

[HOOK_TOPOLOGY]:
- every hook returns a props bag (e.g. `buttonProps`, `pressProps`) to spread onto DOM elements
- hooks are stateless — they require state from `react-stately` hooks passed as `state` arguments
- `useButton` is overloaded on `elementType`: `'button'`, `'a'`, `'div'`, `'input'`, `'span'`, or `ElementType`
- `FocusScope` is a React component that wraps its `children` and optionally contains / restores focus
- `useFocusManager()` resolves the nearest parent `FocusScope` context

[LOCAL_ADMISSION]:
- import from `react-aria` barrel or per-capability sub-path (e.g. `react-aria/i18n`)
- `I18nProvider` wraps the application root; `useLocale()` reads the nearest provider locale
- `useFilter` produces locale-aware `startsWith` / `endsWith` / `contains` predicates
- `mergeProps` chains event handlers rather than overwriting; use it at every composition boundary
- `UNSAFE_PortalProvider` / `useUNSAFE_PortalContext` provide a custom portal container

[RAIL_LAW]:
- package: `react-aria`
- owns: DOM attribute generation, normalised interaction events, ARIA attribute application, focus management
- accept: state from `react-stately` hooks, refs from the consuming component
- reject: hand-rolling ARIA attributes, browser-specific pointer normalisation, focus trapping outside `FocusScope`
