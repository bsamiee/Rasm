# [API_CATALOGUE] cmdk

`cmdk` supplies the `Command` compound component and its subcomponents for building accessible command menus and keyboard-navigable palettes. The package exports both namespace-qualified names (`Command.List`, `Command.Item`, …) via the default `Command` export and flat named exports (`CommandList`, `CommandItem`, …) for direct import. `Command.Dialog` embeds the menu inside a Radix UI `Dialog`. `useCommandState` reads internal filtered state from a hook.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cmdk`
- package: `cmdk`
- namespace: `cmdk`
- asset: runtime component library
- rail: ui-components

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: command menu types
- rail: ui-components

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [RAIL]                                                                   |
| :-----: | :-------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `CommandFilter` | type alias    | `(value, search, keywords?) => number` — 0 = hidden, 1 = best match      |
|  [02]   | `State`         | type alias    | `{ search, value, selectedItemId?, filtered: { count, items, groups } }` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: compound component (namespace exports)
- rail: ui-components

| [INDEX] | [SURFACE]           | [ENTRY_FAMILY] | [RAIL]                                                                                                    |
| :-----: | :------------------ | :------------- | :-------------------------------------------------------------------------------------------------------- |
|  [01]   | `Command`           | root component | menu container; `label?`, `shouldFilter?`, `filter?`, `value?`, `onValueChange?`, `loop?`, `vimBindings?` |
|  [02]   | `Command.List`      | subcomponent   | scrollable results container; `label?`                                                                    |
|  [03]   | `Command.Item`      | subcomponent   | selectable item; `value?`, `keywords?`, `disabled?`, `onSelect?`, `forceMount?`                           |
|  [04]   | `Command.Input`     | subcomponent   | search input; `value?`, `onValueChange?`                                                                  |
|  [05]   | `Command.Group`     | subcomponent   | item group with optional `heading?`; `value?`, `forceMount?`                                              |
|  [06]   | `Command.Separator` | subcomponent   | visual separator; `alwaysRender?`                                                                         |
|  [07]   | `Command.Dialog`    | subcomponent   | `Command` + Radix Dialog; `overlayClassName?`, `contentClassName?`, `container?`                          |
|  [08]   | `Command.Empty`     | subcomponent   | renders when `filtered.count === 0`                                                                       |
|  [09]   | `Command.Loading`   | subcomponent   | async loading indicator; `progress?`, `label?`                                                            |

[ENTRYPOINT_SCOPE]: flat named exports
- rail: ui-components

| [INDEX] | [SURFACE]          | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :----------------- | :------------- | :--------------------------------------------- |
|  [01]   | `CommandRoot`      | alias          | same as `Command` root                         |
|  [02]   | `CommandList`      | alias          | same as `Command.List`                         |
|  [03]   | `CommandItem`      | alias          | same as `Command.Item`                         |
|  [04]   | `CommandInput`     | alias          | same as `Command.Input`                        |
|  [05]   | `CommandGroup`     | alias          | same as `Command.Group`                        |
|  [06]   | `CommandSeparator` | alias          | same as `Command.Separator`                    |
|  [07]   | `CommandDialog`    | alias          | same as `Command.Dialog`                       |
|  [08]   | `CommandEmpty`     | alias          | same as `Command.Empty`                        |
|  [09]   | `CommandLoading`   | alias          | same as `Command.Loading`                      |
|  [10]   | `defaultFilter`    | filter fn      | built-in `CommandFilter` using `command-score` |
|  [11]   | `useCommandState`  | hook           | `<T>(selector: (state: State) => T) => T`      |

## [04]-[IMPLEMENTATION_LAW]

[COMMAND_TOPOLOGY]:
- all subcomponents render `div` elements by default; `asChild` prop delegates to a custom element via Radix Slot
- filtering is automatic when `shouldFilter` is unset or `true`; each `Item.value` is scored against `Input.value` using `command-score`
- `keywords` on `Item` adds additional tokens to match against without appearing in the rendered text
- `CommandFilter` returns a number 0–1; `0` hides the item, `1` is perfect match; intermediate values sort results
- `Command.Dialog` wraps the root in `@radix-ui/react-dialog`; all `Command` props plus `overlayClassName`, `contentClassName`, and `container` are forwarded
- `useCommandState` reads the selector result from the internal store; re-renders only when the selected slice changes
- `--cmdk-list-height` CSS custom property is set on `Command.List` and reflects the current scrollable height for animation

[LOCAL_ADMISSION]:
- Mount `Command` with a controlled `value`/`onValueChange` pair for programmatic item selection.
- Use `Command.Dialog` for modal palette UX; use bare `Command` inside a `Popover` or custom overlay for inline palettes.
- Provide stable, unique `value` on every `Command.Item` when `textContent` can change between renders.

[RAIL_LAW]:
- package: `cmdk`
- owns: keyboard-navigable command palette with automatic filtering, grouping, and Radix Dialog integration
- accept: custom `filter` function, `keywords` on items, controlled `value`/`onValueChange`
- reject: re-implementing command-score filtering or keyboard navigation against this package's item set
