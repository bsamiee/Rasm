# [TS_UI_API_CMDK]

`cmdk` owns the command-palette machine the `view/compose` plane mounts: one compound `Command` drives the search input, scored-and-filtered item list, keyboard navigation, and an internal store, exposed as a slot namespace with flat `Command*` aliases. Every primitive takes `asChild` to merge onto the folder's react-aria pressable spine, the `filter` scorer swaps, and `useCommandState` selects the store for empty and loading rows — cmdk owns list, filter, and keyboard while styling, glyphs, async sources, and the host modal are sibling rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cmdk`
- package: `cmdk` (MIT)
- module: `.` only — `Command` compound + `Command*` flat aliases, `useCommandState`, `defaultFilter`; `command-score` is not a public subpath
- runtime: React client component — DOM + focus management, not universal
- depends: `@radix-ui/react-dialog` (portal Dialog host), `@radix-ui/react-primitive` (`asChild` Slot), `@radix-ui/react-id` (SSR-safe ids), `@radix-ui/react-compose-refs`
- rail: view/command-palette

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the filter scorer and store shape — the seam a design page composes a custom scorer and store selector against, reached through the `filter` prop and the `useCommandState` selector rather than name-imported

[COMMAND_FILTER]: `CommandFilter = (value: string, search: string, keywords?: string[]) => number`
[STATE]: `State.search: string` `State.value: string` `State.selectedItemId?: string` `State.filtered: {count: number; items: Map<string,number>; groups: Set<string>}`

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]  | [CONSUMER]                                                   |
| :-----: | :-------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `CommandFilter` | scorer         | `filter` prop; returns 0..1 relevance, 0 hides the item      |
|  [02]   | `State`         | store snapshot | the shape `useCommandState` selects over                     |
|  [03]   | `defaultFilter` | default scorer | command-score fuzzy ranking; wrap or replace it via `filter` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the compound component and its store hook

- one root owns the machine, children are slots; every primitive is a `ForwardRefExoticComponent` taking `asChild`. `Command` and `CommandDialog` share the full control surface — `CommandDialog` adds the Radix portal shell. Per-component props are the keyed roster below.

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [CONSUMER]                                                        |
| :-----: | :--------------------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `Command` / `CommandRoot`                | root           | the palette state machine; controlled via `value`/`onValueChange` |
|  [02]   | `Command.Input` / `CommandInput`         | search input   | search-value source; controlled for async filtering               |
|  [03]   | `Command.List` / `CommandList`           | result list    | scroll container; animate height by result count                  |
|  [04]   | `Command.Item` / `CommandItem`           | item           | selectable command; `value`=command key, `keywords`=aliases       |
|  [05]   | `Command.Group` / `CommandGroup`         | group          | labeled cohort; shown together when any child matches             |
|  [06]   | `Command.Separator` / `CommandSeparator` | separator      | visible on empty search unless `alwaysRender`                     |
|  [07]   | `Command.Empty` / `CommandEmpty`         | empty row      | auto-renders when the filtered count is 0                         |
|  [08]   | `Command.Loading` / `CommandLoading`     | loading row    | async-source progress indicator                                   |
|  [09]   | `Command.Dialog` / `CommandDialog`       | modal palette  | palette inside a Radix Dialog with a portal target                |
|  [10]   | `useCommandState<T>(selector)`           | store hook     | subscribe to `filtered.count`/`search` for empty/count rows       |

- [01]-[ROOT]: `label`, `shouldFilter`, `filter`, `value`, `defaultValue`, `onValueChange`, `loop`, `disablePointerSelection`, `vimBindings`, `asChild`.
- [02]-[INPUT]: `value`, `onValueChange(search)` (+ `<input>` attrs).
- [03]-[LIST]: `label`; CSS var `--cmdk-list-height`.
- [04]-[ITEM]: `value`, `onSelect(value)`, `keywords`, `disabled`, `forceMount`, `asChild`.
- [05]-[GROUP]: `heading`, `value`, `forceMount`.
- [06]-[SEPARATOR]: `alwaysRender`.
- [08]-[LOADING]: `progress`, `label`.
- [09]-[DIALOG]: `RadixDialog.DialogProps` + `overlayClassName`, `contentClassName`, `container` + all root props.
- [10]-[STATE]: `useCommandState<T>(selector: (state: State) => T): T`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- filtering is automatic and scored: `filter` (default `defaultFilter`, command-score fuzzy) ranks each item against `search` over its `value`, `children` text, and `keywords`; a 0 score hides it and `Command.Empty` renders when `filtered.count` is 0. `shouldFilter={false}` hands matching to the caller, who renders only the items it computed.
- keyboard model: arrows move selection, `loop` wraps ends, `vimBindings` adds ctrl+n/j/p/k, `disablePointerSelection` keeps selection keyboard-driven under a moving pointer; `value`/`onValueChange` control the active item and `Command.Input` `value`/`onValueChange` control the search.
- `value` is an item's identity: pass a stable `value` when visible text changes between renders, or filtering and selection break; `forceMount` keeps an item or group mounted through filtering for enter/exit animation.

[STACKING]:
- `@radix-ui/react-slot`(`.api/radix-ui-react-slot.md`) + react-aria(`.api/react-aria.md`): every primitive merges its behavior onto a child via Slot, so `Command.Item asChild` wraps a react-aria pressable — cmdk owns list+filter+roving-focus, react-aria owns press/hover/focus, no double focus machinery.
- `@effect-atom/atom-react`(`.api/effect-atom-atom-react.md`): async command sources fold `ONE_FOLD_ONE_BINDING` — `shouldFilter={false}`, bind `Command.Input` `onValueChange` to a search atom, run the remote query as an `Effect`, render matched items from the derived atom; `useCommandState` selects `search`/`filtered.count` to drive `Command.Loading` `progress` and the empty row, and `onSelect(value)` dispatches a `ControlIntent`/action where `value` is a `Schema.Literal` command-id union.
- `class-variance-authority`(`.api/class-variance-authority.md`) + `clsx`/`tailwind-merge`(`.api/clsx.md`) + `lucide-react`(`.api/lucide-react.md`): `cva` selectors through the one `cn` rail style the palette shell, items, and groups; item leading glyphs render as `lucide-react` icons; `Command.List` `--cmdk-list-height` animates via `tw-animate-css` keyframes on the token scale.
- `vaul`(`.api/vaul.md`) / `@floating-ui/react`(`.api/floating-ui-react.md`): the host seam — cmdk owns list+filter+keyboard, the overlay only hosts it, picked by modality. Modality selects the host: `CommandDialog` (cmdk's own Radix-Dialog wrapper) or a bare `Command` inside `vaul`'s `Drawer.Content` serves a centered modal, and a bare `Command` inside `@floating-ui/react`'s `useFloating` + `FloatingPortal` + `FloatingFocusManager` (non-modal, `preserveTabOrder`) serves an anchored/combobox palette, floating-ui owning only position/portal/focus-return. floating-ui's `useListNavigation`/`useTypeahead` are the hand-built non-cmdk list and never layer over a cmdk list (the double-keyboard defect); one host is chosen, and `CommandDialog` inside another `vaul`/floating host is the double-portal + double-focus-trap defect.

[LOCAL_ADMISSION]:
- admit only the command-palette list+filter+keyboard machine; `cva`/`cn` styles it, `lucide-react` supplies glyphs, `@effect-atom` drives async data, and `vaul`/`@floating-ui/react` hosts the surrounding modal or sheet unless `CommandDialog` is the deliberate host.
- one `Command` root per palette; a command family is a `Command.Group` row, a command is a `Command.Item` row keyed by a stable `value`.

[RAIL_LAW]:
- Package: `cmdk`
- Owns: the palette search input, scored+filtered item list, keyboard navigation, internal store, and the `Command.Dialog` Radix-portal host
- Accept: the compound `Command` with slot children, `asChild` bridging to react-aria, a swapped `filter` composing `defaultFilter`, `shouldFilter={false}` + `@effect-atom` for async sources, `useCommandState` for empty/count/loading rows, stable item `value` + `keywords`, `cva`/`cn` styling and `lucide-react` glyphs
- Reject: a hand-rolled search-filter or keyboard loop, re-implemented fuzzy scoring instead of `filter`/`defaultFilter`, `commandScore`/`cmdk/command-score` imported as a subpath, unstable item `value`, `CommandDialog` double-wrapped inside another `vaul`/floating host, `@floating-ui/react`'s `useListNavigation`/`useTypeahead` layered over a cmdk list, react-aria focus machinery duplicated instead of `asChild`
