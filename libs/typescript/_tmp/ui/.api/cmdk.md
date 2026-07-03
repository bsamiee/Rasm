# [API_CATALOGUE] cmdk

`cmdk` supplies the `Command` compound component and its subcomponents for accessible, keyboard-navigable command palettes. It exports both namespace-qualified names (`Command.List`, `Command.Item`, …) via the `Command` compound and flat aliases (`CommandList`, `CommandItem`, …) for direct import — same components under two spellings. `Command.Dialog` embeds the menu in a Radix `Dialog` (merging its full `DialogProps`), `defaultFilter` is the built-in scorer, and `useCommandState` reads the internal filtered store through a selector. Filtering is INJECTABLE: the `filter` prop replaces the built-in scorer, which is how `ui` binds one shared `react-aria` `useFilter` algebra across the palette and every collection (`interaction/command.md#COMMAND_SURFACE`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cmdk`
- package / version: `cmdk` @ `1.1.1`
- license: `MIT`
- module: dual ESM `dist/index.mjs` + CJS `dist/index.js`; single `.` export
- peer: `react` `^18 || ^19`, `react-dom` `^18 || ^19` — React `ForwardRefExoticComponent` primitives
- dependencies: `@radix-ui/react-dialog` (the `Command.Dialog` shell), `@radix-ui/react-primitive` (the `asChild`/`Slot` seam), `@radix-ui/react-id`, `@radix-ui/react-compose-refs`
- rail: ui-components

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: command menu types
- rail: ui-components

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [NOTE]                                                                                          |
| :-----: | :-------------- | :------------ | :--------------------------------------------------------------------------------------------- |
|  [01]   | `CommandFilter` | fn type       | `(value: string, search: string, keywords?: string[]) => number` — `0` hides, `1` is a perfect match, intermediate values sort |
|  [02]   | `State`         | store shape   | `{ search, value, selectedItemId?, filtered: { count: number; items: Map<string, number>; groups: Set<string> } }` — the `useCommandState` selector input |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: compound component (namespace exports)
- rail: ui-components

| [INDEX] | [SURFACE]           | [ENTRY_FAMILY] | [PROPS]                                                                                                                             |
| :-----: | :------------------ | :------------- | :--------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Command`           | root           | `label?`, `shouldFilter?`, `filter?: CommandFilter`, `defaultValue?`, `value?`, `onValueChange?(value)`, `loop?`, `disablePointerSelection?`, `vimBindings?`, `asChild?` + `HTMLDivElement` attrs |
|  [02]   | `Command.List`      | subcomponent   | `label?`, `asChild?`; sets the `--cmdk-list-height` CSS var for height animation                                                    |
|  [03]   | `Command.Item`      | subcomponent   | `value?`, `keywords?: string[]`, `disabled?`, `onSelect?(value)`, `forceMount?`, `asChild?`; emits `data-selected`/`data-disabled`  |
|  [04]   | `Command.Input`     | subcomponent   | `value?`, `onValueChange?(search)`, `asChild?` + `HTMLInputElement` attrs (`onChange`/`value`/`type` are owned)                     |
|  [05]   | `Command.Group`     | subcomponent   | `heading?: React.ReactNode`, `value?` (required when no `heading`), `forceMount?`, `asChild?`                                        |
|  [06]   | `Command.Separator` | subcomponent   | `alwaysRender?`, `asChild?`; hidden while a search query is active unless `alwaysRender`                                             |
|  [07]   | `Command.Dialog`    | subcomponent   | all `Command` root props + `RadixDialog.DialogProps` (`open?`, `defaultOpen?`, `onOpenChange?`, `modal?`) + `overlayClassName?`, `contentClassName?`, `container?: HTMLElement` |
|  [08]   | `Command.Empty`     | subcomponent   | `asChild?`; renders only when `filtered.count === 0`                                                                                |
|  [09]   | `Command.Loading`   | subcomponent   | `progress?: number`, `label?`, `asChild?`; conditionally render while loading async items                                            |

[ENTRYPOINT_SCOPE]: flat named exports + filter/hook
- rail: ui-components

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [NOTE]                                                                                     |
| :-----: | :------------------------------------- | :------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `CommandRoot`                          | alias          | the RAW root (`Command` without the compound namespace); `Command` (compound) carries the `.List`/`.Item`/… members |
|  [02]   | `CommandList` … `CommandLoading`       | aliases        | flat spellings of `Command.List`/`Item`/`Input`/`Group`/`Separator`/`Dialog`/`Empty`/`Loading` |
|  [03]   | `defaultFilter`                        | `CommandFilter`| the built-in scorer (wraps the internal `command-score` algorithm); the default when no `filter` prop is passed |
|  [04]   | `useCommandState<T>(selector)`         | hook           | `(selector: (state: State) => T) => T` — subscribes to the internal store, re-renders only when the selected slice changes |

## [04]-[IMPLEMENTATION_LAW]

[COMMAND_TOPOLOGY]:
- every subcomponent renders a `div` (`Input` an `input`) by default; `asChild` delegates rendering to a caller-supplied element via the Radix `Slot` primitive on ALL subcomponents
- filtering is automatic when `shouldFilter` is unset/`true`: each `Item.value` (plus its `keywords`) is scored against `Input.value` by the active `CommandFilter`; `shouldFilter={false}` hands filtering to the caller
- the `filter` prop is the sanctioned injection seam — supply a `CommandFilter` to replace `defaultFilter` wholesale; this is the one place an external scoring algebra takes over
- `keywords` add match tokens that never render as text; `forceMount` keeps an `Item`/`Group` mounted regardless of the filter result
- `Command.Dialog` merges the full Radix `DialogProps`: drive it controlled with `open`/`onOpenChange`, and it emits the `data-state="open|closed"` attribute overlay animations key off
- `useCommandState` reads a selector slice of `State` (search text, active `value`, `filtered.count`/`items`/`groups`) from the internal store without prop-drilling
- styling seam: `--cmdk-list-height` on `Command.List` reflects the live results height for animation; `data-selected`/`data-disabled` on `Command.Item` are the variant hooks a `cva` recipe reads

[STACKING]:
- shared filter algebra: bind `filter={score}` where `score` is derived from `react-aria` `useFilter().contains` so the palette scores IDENTICALLY to the `react-stately` `UNSTABLE_useFilteredListState` collection view — `defaultFilter`/`command-score` is deliberately displaced so no surface hand-rolls a divergent `contains`/`startsWith` (`interaction/command.md#COMMAND_SURFACE`)
- controlled dialog under the effect rail: mount `Command.Dialog` with `open` bound to the `binding/atom.md#ATOM_BINDING` cell; on `Command.Item` `onSelect`, resolve the row's `intentKey` through the `interchange` `IntentRegistry` and dial `CommandGateway.invoke` inside an `Effect`, pre-gated by the `projection` `AvailabilityStore.isEnabled(intentKey)` so a disabled item never fires
- motion seam: the `data-state` the Dialog emits rides the `theming/tokens.md#THEME_TOKENS` `tw-animate-css` `data-[state=open]:animate-in` / `data-[state=closed]:animate-out` layer — enter/exit is declarative CSS resolving against live OKLCH tokens, never a JS controller
- variant recipe: style `Command.Item`/`List` through the one `cn = twMerge(cx(...))` recipe (`class-variance-authority` + `tailwind-merge`), keying `cva` rows off `data-selected`/`data-disabled`; compose an `asChild` item with `@radix-ui/react-slot` `Slot`/`Slottable`
- vocabulary source: render `Command.Item` rows from a closed `Schema.Literal`/`as const satisfies Record<…>` action vocabulary, keying each row's icon off `lucide-react` — never a re-minted command enum beside the gateway registry

[LOCAL_ADMISSION]:
- mount `Command` with a controlled `value`/`onValueChange` pair for programmatic selection; give every `Command.Item` a stable, unique `value` when its `textContent` can change between renders
- use `Command.Dialog` (controlled `open`) for modal palette UX; a bare `Command` inside a `Popover`/custom overlay for an inline palette
- inject `filter` with the shared `useFilter`-derived scorer; do not accept `defaultFilter` where another surface already owns the scoring algebra

[RAIL_LAW]:
- package: `cmdk`
- owns: keyboard-navigable command palette with automatic scoring, grouping, loading/empty states, and a Radix-Dialog shell
- accept: a custom `filter` (the injection seam), `keywords` on items, controlled `value`/`onValueChange`, controlled `Dialog` `open`/`onOpenChange`, `asChild` element delegation
- reject: a per-surface hand-rolled `contains`/`startsWith` predicate diverging from the shared `useFilter` algebra; a re-implemented keyboard-navigation loop; a second command/intent enum beside the gateway `IntentRegistry`
