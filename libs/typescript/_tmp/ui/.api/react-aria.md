# [API_CATALOGUE] react-aria

`react-aria` supplies headless ARIA-compliant interaction hooks that produce `buttonProps`, `pressProps`, `hoverProps`, `focusProps`, and DOM attribute bags for every standard widget family. Consuming owners spread the returned props onto their own markup, keeping styling and accessibility behaviour fully decoupled. The package also re-exports `FocusScope`, `FocusRing`, collection primitives, overlay position utilities, i18n helpers, and DnD coordination hooks for the AppUi interaction spine.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `react-aria`
- package: `react-aria`
- module: `react-aria` (main); `react-aria/i18n`, `react-aria/private/live-announcer/LiveAnnouncer` (sub-paths)
- namespace: `react-aria`
- asset: runtime hook library
- rail: interaction / accessibility

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: interaction result types
- rail: interaction

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]    | [RAIL]                         |
| :-----: | :-------------- | :--------------- | :----------------------------- |
|  [01]   | `ButtonAria<T>` | result interface | `buttonProps: T`, `isPressed`  |
|  [02]   | `PressResult`   | result interface | `pressProps`, `isPressed`      |
|  [03]   | `HoverResult`   | result interface | `hoverProps`, `isHovered`      |
|  [04]   | `FocusRingAria` | result interface | `focusProps`, focus visibility |

[PUBLIC_TYPE_SCOPE]: interaction input props
- rail: interaction

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]     | [RAIL]                             |
| :-----: | :--------------------- | :---------------- | :--------------------------------- |
|  [01]   | `ButtonProps`          | props interface   | press, disabled, children          |
|  [02]   | `AriaButtonProps<T>`   | props interface   | element-typed overload             |
|  [03]   | `AriaButtonOptions<E>` | options interface | omits children, element type       |
|  [04]   | `AriaBaseButtonProps`  | props interface   | aria-* attributes + form attrs     |
|  [05]   | `PressProps`           | props interface   | `PressEvents`, cancel-on-exit      |
|  [06]   | `PressHookProps`       | props interface   | `PressProps` + `ref`               |
|  [07]   | `HoverProps`           | props interface   | `HoverEvents`, `isDisabled`        |
|  [08]   | `AriaFocusRingProps`   | props interface   | `within`, `isTextInput`, autoFocus |

[PUBLIC_TYPE_SCOPE]: focus management types
- rail: focus

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [RAIL]                                 |
| :-----: | :-------------------- | :---------------- | :------------------------------------- |
|  [01]   | `FocusScopeProps`     | props interface   | `contain`, `restoreFocus`, `autoFocus` |
|  [02]   | `FocusManager`        | interface         | `focusNext/Previous/First/Last`        |
|  [03]   | `FocusManagerOptions` | options interface | `from`, `tabbable`, `wrap`, `accept`   |

[PUBLIC_TYPE_SCOPE]: i18n types
- rail: i18n

| [INDEX] | [SYMBOL] | [TYPE_FAMILY]    | [RAIL]                               |
| :-----: | :------- | :--------------- | :----------------------------------- |
|  [01]   | `Filter` | result interface | `startsWith`, `endsWith`, `contains` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: button and toggle hooks
- rail: interaction

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :-------------------------------------------- | :------------- | :------------------------- |
|  [01]   | `useButton(props, ref)`                       | ARIA hook      | element-typed button props |
|  [02]   | `useToggleButton(props, state, ref)`          | ARIA hook      | aria-pressed toggle props  |
|  [03]   | `useToggleButtonGroup(props, ref)`            | ARIA hook      | group ARIA props           |
|  [04]   | `useToggleButtonGroupItem(props, state, ref)` | ARIA hook      | item within toggle group   |

[ENTRYPOINT_SCOPE]: press, hover, keyboard, move interaction hooks
- rail: interaction

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY]   | [RAIL]                       |
| :-----: | :------------------------------- | :--------------- | :--------------------------- |
|  [01]   | `usePress(props)`                | interaction hook | normalised pointer/key press |
|  [02]   | `useHover(props)`                | interaction hook | hover sans touch emulation   |
|  [03]   | `useKeyboard(props)`             | interaction hook | keyboard event delegation    |
|  [04]   | `useLongPress(props)`            | interaction hook | long-press detection         |
|  [05]   | `useMove(props)`                 | interaction hook | pointer/keyboard move events |
|  [06]   | `useFocus(props)`                | interaction hook | focus event normalisation    |
|  [07]   | `useFocusVisible(props?)`        | interaction hook | keyboard-focus visibility    |
|  [08]   | `useFocusWithin(props)`          | interaction hook | focus within containment     |
|  [09]   | `useInteractOutside(props, ref)` | interaction hook | outside-click detection      |

[ENTRYPOINT_SCOPE]: focus management hooks and components
- rail: focus

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :------------------------- | :------------- | :-------------------------------- |
|  [01]   | `FocusScope(props)`        | component      | contain / restore focus scope     |
|  [02]   | `useFocusManager()`        | hook           | returns `FocusManager` from scope |
|  [03]   | `FocusRing(props)`         | component      | visible focus ring wrapper        |
|  [04]   | `useFocusRing(props?)`     | hook           | `isFocused`, `isFocusVisible`     |
|  [05]   | `useFocusable(props, ref)` | hook           | spread focusable props            |
|  [06]   | `Focusable`                | component      | imperative `.focus()` wrapper     |

[ENTRYPOINT_SCOPE]: listbox, gridlist, menu hooks
- rail: collection

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :------------------------------------- | :------------- | :------------------------- |
|  [01]   | `useListBox(props, state, ref)`        | ARIA hook      | listbox ARIA attrs         |
|  [02]   | `useOption(props, state, ref)`         | ARIA hook      | option ARIA attrs          |
|  [03]   | `useListBoxSection(props)`             | ARIA hook      | section ARIA attrs         |
|  [04]   | `useGridList(props, state, ref)`       | ARIA hook      | gridlist ARIA attrs        |
|  [05]   | `useGridListItem(props, state, ref)`   | ARIA hook      | row/item ARIA attrs        |
|  [06]   | `useMenu(props, state, ref)`           | ARIA hook      | menu ARIA attrs            |
|  [07]   | `useMenuItem(props, state, ref)`       | ARIA hook      | menuitem ARIA attrs        |
|  [08]   | `useMenuTrigger(props, state, ref)`    | ARIA hook      | trigger ARIA attrs         |
|  [09]   | `useSubmenuTrigger(props, state, ref)` | ARIA hook      | submenu trigger ARIA attrs |

[ENTRYPOINT_SCOPE]: select, combobox, text field hooks
- rail: form

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :----------------------------------------------------------------------- | :------------- | :----------------------------- |
|  [01]   | `useSelect(props, state, ref)`                                           | ARIA hook      | select ARIA attrs              |
|  [02]   | `useComboBox(props, state, inputRef, buttonRef, listBoxRef, popoverRef)` | ARIA hook      | combobox ARIA attrs            |
|  [03]   | `useTextField(props, ref)`                                               | ARIA hook      | input / textarea ARIA attrs    |
|  [04]   | `useSearchField(props, state, ref)`                                      | ARIA hook      | search field ARIA attrs        |
|  [05]   | `useNumberField(props, state, ref)`                                      | ARIA hook      | number field ARIA attrs        |
|  [06]   | `useField(props)`                                                        | ARIA hook      | label + description ARIA attrs |
|  [07]   | `useLabel(props, ref?)`                                                  | ARIA hook      | label element ARIA attrs       |

[ENTRYPOINT_SCOPE]: overlay and dialog hooks
- rail: overlay

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `useOverlay(props, ref)`               | ARIA hook      | overlay ARIA attrs                        |
|  [02]   | `useOverlayTrigger(type, state, ref?)` | ARIA hook      | trigger button ARIA attrs                 |
|  [03]   | `useOverlayPosition(props)`            | position hook  | `overlayProps`, `arrowProps`, `placement` |
|  [04]   | `useModalOverlay(props, state, ref)`   | ARIA hook      | modal overlay ARIA attrs                  |
|  [05]   | `usePopover(props, state, ref)`        | ARIA hook      | popover ARIA attrs                        |
|  [06]   | `useDialog(props, ref)`                | ARIA hook      | dialog ARIA attrs                         |
|  [07]   | `usePreventScroll(options?)`           | utility hook   | body scroll lock                          |
|  [08]   | `DismissButton`                        | component      | screen-reader dismiss target              |
|  [09]   | `Overlay`                              | component      | portal host wrapper                       |

[ENTRYPOINT_SCOPE]: i18n hooks
- rail: i18n

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :--------------------------------------------------- | :------------- | :----------------------------- |
|  [01]   | `useLocale()`                                        | hook           | `{ locale, direction }`        |
|  [02]   | `useFilter(options?)`                                | hook           | `Filter` string search         |
|  [03]   | `useCollator(options?)`                              | hook           | `Intl.Collator` instance       |
|  [04]   | `useDateFormatter(options?)`                         | hook           | `Intl.DateTimeFormat` instance |
|  [05]   | `useNumberFormatter(options?)`                       | hook           | `Intl.NumberFormat` instance   |
|  [06]   | `useListFormatter(options?)`                         | hook           | `Intl.ListFormat` instance     |
|  [07]   | `useLocalizedStringFormatter(strings, packageName?)` | hook           | localised string map           |
|  [08]   | `I18nProvider`                                       | component      | locale context provider        |
|  [09]   | `isRTL(locale)`                                      | utility        | boolean RTL test               |

[ENTRYPOINT_SCOPE]: utility exports
- rail: interaction

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :--------------------------- | :------------- | :------------------------------- |
|  [01]   | `mergeProps(...args)`        | utility        | prop merge with chained handlers |
|  [02]   | `mergeRefs(...refs)`         | utility        | ref merge                        |
|  [03]   | `useId(defaultId?)`          | utility        | stable cross-component ID        |
|  [04]   | `useObjectRef(forwardedRef)` | utility        | `RefObject` from `ForwardedRef`  |
|  [05]   | `VisuallyHidden`             | component      | invisible-but-accessible wrapper |
|  [06]   | `useVisuallyHidden(props?)`  | hook           | visually-hidden CSS props        |
|  [07]   | `RouterProvider`             | component      | link-navigation context          |
|  [08]   | `SSRProvider`                | component      | SSR ID stability provider        |
|  [09]   | `useIsSSR()`                 | hook           | boolean SSR detection            |

## [04]-[IMPLEMENTATION_LAW]

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
