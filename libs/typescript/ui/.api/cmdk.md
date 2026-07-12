# [TS_UI_API_CMDK]

`cmdk` is the command-palette state machine the `view/compose` plane composes: one compound `Command` component owning the search input, filtered+scored item list, keyboard navigation (arrows, loop, vim bindings), and an internal store, exposed as a namespace (`Command.Input`/`.List`/`.Item`/`.Group`/`.Separator`/`.Empty`/`.Loading`/`.Dialog`) plus flat `Command*` aliases. It builds on Radix (`@radix-ui/react-dialog`/`react-slot`/`react-id`), so every primitive accepts `asChild` to merge onto the folder's react-aria pressable spine, and its `filter` scorer is swappable while `useCommandState` subscribes to the filtered store for empty/loading rows. `cmdk` owns list+filter+keyboard state; `cva`/`cn` styles the shell; `lucide-react` supplies item glyphs; `@effect-atom` drives async command sources — never a hand-rolled filter or keyboard loop.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cmdk`
- package: `cmdk`
- license: `MIT`
- deps: `@radix-ui/react-dialog`, `@radix-ui/react-compose-refs`, `@radix-ui/react-id`, `@radix-ui/react-primitive` (`asChild` Slot, portal Dialog, SSR-safe ids)
- react-peer: React 19 spine (`.api/react.md`); compiled by react-compiler, memoization never hand-written
- catalog-verdict: KEEP
- runtime: React client component — DOM + focus management; not universal
- exports: `.` only (`Command` compound + `Command*` flat aliases, `useCommandState`, `defaultFilter`); `command-score` is internal, not a public subpath

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the filter scorer and the store shape
- rail: view/command-palette
- `CommandFilter` is the swap point for match ranking; `State` is what `useCommandState` selects over. Both are the seam a design page composes — a custom scorer and a store selector for the empty/count rows.

| [INDEX] | [SYMBOL]                                                                                                                                           | [TYPE_FAMILY]  | [CONSUMER_BOUNDARY]                                                   |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `CommandFilter = (value: string, search: string, keywords?: string[]) => number`                                                                   | scorer         | `filter` prop; returns 0..1 relevance, 0 hides the item               |
|  [02]   | `State = { search: string; value: string; selectedItemId?: string; filtered: { count: number; items: Map<string, number>; groups: Set<string> } }` | store snapshot | the shape `useCommandState` selects over                              |
|  [03]   | `defaultFilter: CommandFilter`                                                                                                                     | default scorer | command-score fuzzy ranking; the `filter` default; wrap or replace it |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the compound component and its store hook
- rail: view/command-palette
- one root owns the machine; children are slots. Every primitive is a `ForwardRefExoticComponent` accepting `asChild`. Root and `CommandDialog` share the full control surface; `CommandDialog` adds the Radix portal shell.

| [INDEX] | [SURFACE]                                                                                                                                                                                           | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                                       |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `Command` (compound root; bare alias `CommandRoot`) props: `label`, `shouldFilter`, `filter`, `value`, `defaultValue`, `onValueChange`, `loop`, `disablePointerSelection`, `vimBindings`, `asChild` | root           | the palette state machine; controlled selection via `value`/`onValueChange`               |
|  [02]   | `Command.Input` (`CommandInput`) props: `value`, `onValueChange(search)` (+ `<input>` attrs)                                                                                                        | search input   | search-value source; controlled for async filtering                                       |
|  [03]   | `Command.List` (`CommandList`) props: `label`; CSS var `--cmdk-list-height`                                                                                                                         | result list    | scroll container; animate height by result count                                          |
|  [04]   | `Command.Item` (`CommandItem`) props: `value`, `onSelect(value)`, `keywords`, `disabled`, `forceMount`, `asChild`                                                                                   | item           | selectable command; `value` = stable command key, `keywords` = alias tokens               |
|  [05]   | `Command.Group` (`CommandGroup`) props: `heading`, `value`, `forceMount`                                                                                                                            | group          | labeled cohort; shown together when any child matches                                     |
|  [06]   | `Command.Separator` (`CommandSeparator`) props: `alwaysRender`                                                                                                                                      | separator      | visible on empty search unless `alwaysRender`                                             |
|  [07]   | `Command.Empty` (`CommandEmpty`)                                                                                                                                                                    | empty row      | auto-renders when the filtered count is 0                                                 |
|  [08]   | `Command.Loading` (`CommandLoading`) props: `progress`, `label`                                                                                                                                     | loading row    | async-source progress indicator                                                           |
|  [09]   | `Command.Dialog` (`CommandDialog`) props: `RadixDialog.DialogProps` + `overlayClassName`, `contentClassName`, `container` + all root props                                                          | modal palette  | palette inside a Radix Dialog with a portal target                                        |
|  [10]   | `useCommandState<T>(selector: (state: State) => T): T`                                                                                                                                              | store hook     | subscribe to `filtered.count`/`search` for custom empty/count rows without list re-render |

## [04]-[IMPLEMENTATION_LAW]

[COMMAND_SEMANTICS]:
- filtering is automatic and scored: each item is ranked by `filter` (default `defaultFilter`, command-score fuzzy) against `search`, matched against the item `value`, `children` text, and `keywords`; score 0 hides it, and `Command.Empty` renders when `filtered.count` is 0. Set `shouldFilter={false}` to own matching yourself (render only the items the caller computed).
- keyboard model: arrows move selection, `loop` wraps ends, `vimBindings` enables ctrl+n/j/p/k, `disablePointerSelection` keeps selection keyboard-driven under a moving pointer. `value`/`onValueChange` control the active item; `Command.Input` `value`/`onValueChange` control the search.
- `value` stability: an item's `value` is its identity — when visible text changes between renders, pass a stable `value`, or filtering and selection break. `forceMount` keeps an item/group mounted through filtering for enter/exit animation.
- `command-score` is internal: catalog-bound exports only `.`, so the scorer is reached through `defaultFilter` (compose or replace it via `filter`), never imported as `cmdk/command-score`.

[INTEGRATION_LAW]:
- Stack with `@radix-ui/react-slot` (`asChild`) + react-aria: every primitive merges its behavior onto a child via Slot, so `Command.Item asChild` wraps a react-aria pressable/link — cmdk owns list+filter+roving-focus, react-aria owns press/hover/focus semantics, no double focus machinery.
- Stack with `@effect-atom` for async command sources (`ONE_FOLD_ONE_BINDING`): set `shouldFilter={false}`, bind `Command.Input` `onValueChange` to a search atom, run the remote query as an `Effect`, and render matched items from the derived atom; select `search`/`filtered.count` via `useCommandState` to drive `Command.Loading` `progress` and the empty row. `onSelect(value)` dispatches the command as a `ControlIntent`/action where `value` is a `Schema.Literal` command-id union.
- Stack with `cva`/`cn` + `lucide-react`: style the palette shell, items, and groups with `cva` selectors through the one `cn` rail; render item leading glyphs as `lucide-react` icons. `Command.List` `--cmdk-list-height` animates via `tw-animate-css` keyframes on the token scale.
- Stack with the folder's floating/sheet owners — the host seam (cmdk owns list+filter+keyboard; the overlay is only the host it mounts INTO, picked by modality): a centered-modal palette is `CommandDialog` (cmdk's own Radix-Dialog wrapper) OR a bare `Command` inside `vaul`'s `Drawer.Content`; an anchored/combobox palette (positioned to an input, not centered) is a bare `Command` inside `@floating-ui/react`'s `useFloating` + `FloatingPortal` + `FloatingFocusManager` (non-modal, `preserveTabOrder`), where floating-ui owns ONLY position/portal/focus-return and cmdk owns the whole keyboard+filter inside it. floating-ui's `useListNavigation`/`useTypeahead` are the hand-built NON-cmdk list — they drive a caller-owned `listRef`/`activeIndex` over items the caller render, and `useTypeahead` targets menu buttons not a searchable input — so they are never layered over a cmdk list (that is the double-keyboard defect). Choose exactly one host; never `CommandDialog` inside another `vaul`/floating host (double portal + double focus trap is the named defect).

[LOCAL_ADMISSION]:
- the command-palette list+filter+keyboard machine only; styling is `cva`/`cn`, glyphs are `lucide-react`, async data is `@effect-atom`, the surrounding modal/sheet is `vaul`/`@floating-ui/react` unless `CommandDialog` is the deliberate host.
- one `Command` root per palette; new command families are `Command.Group` rows, new commands are `Command.Item` rows keyed by a stable `value`.

[RAIL_LAW]:
- Package: `cmdk`
- Owns: the palette search input, scored+filtered item list, keyboard navigation, internal store, and the `Command.Dialog` Radix-portal host
- Accept: the compound `Command` with slot children, `asChild` bridging to react-aria, a swapped `filter` (composing `defaultFilter`), `shouldFilter={false}` + `@effect-atom` for async sources, `useCommandState` for empty/count/loading rows, stable item `value` + `keywords`, `cva`/`cn` styling and `lucide-react` glyphs
- Reject: a hand-rolled search-filter or keyboard loop, re-implementing fuzzy scoring instead of `filter`/`defaultFilter`, importing `commandScore`/`cmdk/command-score` as a subpath, unstable item `value`, double-wrapping `CommandDialog` inside another `vaul`/floating host, layering `@floating-ui/react`'s `useListNavigation`/`useTypeahead` over a cmdk list (cmdk owns the keyboard; floating-ui hosts only position/portal/focus via `useFloating`/`FloatingPortal`/`FloatingFocusManager`), duplicating react-aria's focus machinery instead of `asChild`
