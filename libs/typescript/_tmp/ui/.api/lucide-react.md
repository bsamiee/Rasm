# [API_CATALOGUE] lucide-react

`lucide-react` ships 1747 SVG icons as named `ForwardRefExoticComponent` exports, every one the same `LucideIcon` type — the roster is a vocabulary, not an API surface. The collapse is that a design keys a named icon per domain row (a `CommandSpec` → its `LucideIcon`); the icon is the row's identity, never a hand-authored SVG and never `createLucideIcon` per existing glyph. `LucideProps` (`size`/`strokeWidth`/`absoluteStrokeWidth`/`color` + SVG attrs) is uniform across the roster, `LucideProvider`/`useLucideContext` set project-wide defaults once, `Icon` renders a runtime-chosen `iconNode`, and `createLucideIcon` mints a genuinely-custom glyph in the same shape. Named imports are individually tree-shaken (`sideEffects: false`) — importing five icons bundles five, never the roster. It is the icon vocabulary keyed by `CommandAction` row under the `interaction/command.md` `ActionIcon`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lucide-react`
- package: `lucide-react`
- version: `1.23.0`
- license: `ISC`
- module: three barrels — `.` (each icon exported plain `Activity` AND `Activity as ActivityIcon`), `.prefixed` (`LucideActivity`), `.suffixed` (`ActivityIcon`); `sideEffects: false`, so named imports bundle individually
- peer: `react ^16.5.1 || ^17 || ^18 || ^19` (icons are `ForwardRefExoticComponent`, ref → `<svg>`; the React 19 spine here)
- runtime: isomorphic React components — SSR-safe (pure presentational, no effects); tree-shaken per named import
- entry: `dist/lucide-react.d.ts` — 1747 named `LucideIcon` exports + dual `<Name>Icon` aliases, plus `Icon`/`createLucideIcon`/`LucideProvider`/`useLucideContext`; the `.prefixed`/`.suffixed` barrels resolve name collisions with DOM globals (`Menu`/`Link`/`Image`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the one icon shape + its props + dynamic-icon data
- rail: token/icon
- Every named icon and `createLucideIcon` result is exactly `LucideIcon`; `LucideProps` is uniform; `IconNode` is the raw data the dynamic/custom paths consume. `LucideProps`/`LucideIcon`/`IconNode`/`SVGAttributes` are the `export type`d symbols; the rest are structural shapes reached through the exported ones.

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                                        |
| :-----: | :---------------------------------------------------------------- | :---------------- | :-------------------------------------------------------------------------- |
|  [01]   | `LucideIcon` (exported)                                           | icon component     | `ForwardRefExoticComponent<Omit<LucideProps,'ref'> & RefAttributes<SVGSVGElement>>` — every named icon; the `interaction/command.md` `CommandSpec.icon` field type |
|  [02]   | `LucideProps` (exported)                                          | icon props         | `ElementAttributes & { size?: string \| number; absoluteStrokeWidth?: boolean }` — `strokeWidth`/`color`/`className` ride the SVG attrs |
|  [03]   | `IconNode` (exported)                                             | raw node data      | `[elementName: SVGElementType, attrs: Record<string,string>][]` — input to `Icon`/`createLucideIcon` |
|  [04]   | `SVGAttributes` (exported)                                        | svg attr subset    | `Partial<SVGProps<SVGSVGElement>>` — the passthrough SVG surface |
|  [05]   | `SVGElementType` / `ElementAttributes` (structural)              | node/attr shapes   | `'circle'\|'ellipse'\|'g'\|'line'\|'path'\|'polygon'\|'polyline'\|'rect'`; `RefAttributes<SVGSVGElement> & SVGAttributes` — inferred at use, not separately imported |
|  [06]   | `LucideConfig` / `LucideProviderProps` / `IconComponentProps` (structural) | provider + dynamic shapes | `{ size, color, strokeWidth, absoluteStrokeWidth, className }`; `LucideConfig` partial + `children`; `LucideProps & { iconNode }` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: named vocabulary, dynamic + custom icons, and default-prop context
- rail: token/icon
- The named export is the common case (static, tree-shaken, vocabulary-keyed); `Icon`/`createLucideIcon` are the runtime/custom escape hatches; `LucideProvider` is the one place project defaults live.

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                                    |
| :-----: | :---------------------------------------------------------------------------- | :------------- | :---------------------------------------------------------------------- |
|  [01]   | `<IconName {...LucideProps} />` — any of 1747 named `LucideIcon` exports      | named icon     | `interaction/command.md` — the `_Commands` vocabulary keys a named import per row (`PlusSquare`/`Activity`/`Eye`/`Download`/`Layers`); the icon is the row identity |
|  [02]   | `<IconName>Icon` suffixed aliases / `.prefixed` `Lucide<IconName>` barrel     | alias entry    | collision-free imports where an icon name shadows a DOM/global (`Menu`, `Link`, `Image`) |
|  [03]   | `Icon` (`ForwardRefExoticComponent<Omit<IconComponentProps,'ref'> & RefAttributes<SVGSVGElement>>`) | dynamic icon | render an icon chosen at runtime by passing `iconNode` — only for runtime-selected icons, never a static row |
|  [04]   | `createLucideIcon(iconName: string, iconNode: IconNode): LucideIcon`          | custom factory | mint a project glyph in the exact `LucideIcon` shape; participates in `LucideProvider` defaults — only for a glyph genuinely absent from the roster |
|  [05]   | `LucideProvider({ children, size?, color?, strokeWidth?, absoluteStrokeWidth?, className? }): FunctionComponentElement<ProviderProps<LucideProps>>` | provider | SPA root — set project-wide `size`/`strokeWidth`/`color` once (aligned to the `theming/tokens.md` scale); every icon inherits |
|  [06]   | `useLucideContext(): LucideProps`                                             | context read   | read the active provider defaults (e.g. to derive a matching non-icon glyph size) |

## [04]-[IMPLEMENTATION_LAW]

[ICON_TOPOLOGY]:
- one shape, N names: every named export is `LucideIcon`; the roster is domain vocabulary, so a catalog documents the shape and the selection law, never enumerates 1747 rows. Selection is by named import keyed to a design row.
- prop law: `size` (default `24`) drives both `width` and `height`; `strokeWidth` (default `2`); `absoluteStrokeWidth` renders stroke as `strokeWidth / size` so line weight stays constant across sizes; `color` maps to the SVG `stroke`. The ref forwards to the `<svg>` element.
- tree-shaken, no barrel side-effects: `sideEffects: false` means five named imports bundle five icons; a `import * as Icons` or an icon picked from a `Record` of all icons defeats it. Dynamic `Icon` + `iconNode` is the only runtime-selection path and is opt-in per call.
- dual naming: the main barrel exports both `Activity` and `ActivityIcon`; the `.prefixed`/`.suffixed` barrels give a single naming scheme when an icon name collides with a DOM global — pick one convention per app.

[STACKING_LAW]:
- `interaction/command.md` command surface: the `_Commands` `as const satisfies Record<string, CommandSpec>` vocabulary carries a named `LucideIcon` per row (`icon: PlusSquare`), and `ActionIcon` resolves `_Commands[action].icon` via `React.createElement(Icon, { size, "aria-hidden": true })` — the icon is the action's identity keyed by the `CommandAction` `Schema.Literal` domain, never a per-`.tsx` inline SVG and never `createLucideIcon` per action.
- `class-variance-authority.md` + `tailwind-merge.md`: icon `className`/`color`/`size` flow through the one `cn = (...a) => twMerge(cx(...a))` variant recipe (`cx` is cva's `clsx` re-export), so an icon's sizing is a `cva` row read through `cn`, not ad-hoc classes.
- `cmdk.md` + `vaul.md` + `radix-ui-react-slot.md`: icons render inside the `cmdk` `Command.Item` (and the `vaul` `Drawer` mobile surface), composed with `aria-hidden` where the row label carries the accessible name; the `Slot` `asChild` primitive merges the icon-bearing element without a `cloneElement` hand-roll.
- `react-aria.md` + `react-stately.md`: the palette the icons render in is filtered by the shared `useFilter`/`UNSTABLE_useFilteredListState` predicate — the icons are pure presentational leaves inside that scored, keyboard-navigable list.
- universal `libs/typescript/.api/effect.md`: the icon vocabulary is a static `Record` keyed by a `Schema.Literal` action domain — no `Effect` wrapping (icons are pure presentational); the design binds the icon at the same seam the `CommandGateway` binds the intent, so one `CommandAction` row owns label, icon, keybinding, role, and intent key together.
- `theming/tokens.md`: project-wide `size`/`strokeWidth`/`color` are set once through `LucideProvider` at the SPA root, drawing from the OKLCH token scale, so no icon call-site repeats them. Icons are React `ForwardRefExoticComponent`s over `react` (the ref forwards to `<svg>`).

[LOCAL_ADMISSION]:
- Import named icons statically and key them to a design vocabulary row; never `import * as Icons` or index an all-icons `Record` (defeats tree-shaking).
- Set project defaults once via `LucideProvider`; never repeat `size`/`strokeWidth`/`color` at every call site.
- Use `Icon` + `iconNode` only for runtime-chosen icons; use `createLucideIcon` only for a genuinely custom glyph absent from the roster — never for an icon the package already ships.
- Prefer the `.prefixed`/`.suffixed` alias entry when an icon name collides with a DOM global.

[RAIL_LAW]:
- Package: `lucide-react`
- Owns: the 1747-icon `LucideIcon` vocabulary (one shared forwardRef SVG shape), the `LucideProps` size/stroke/color surface, the dynamic `Icon`, the `createLucideIcon` custom factory, and the `LucideProvider`/`useLucideContext` default-prop context
- Accept: static named imports keyed by a design vocabulary row, `LucideProvider` defaults at the root, `Icon` for runtime selection, `createLucideIcon` for genuinely-absent glyphs, the alias entries for collisions, `className`/`size` through the `cn` recipe
- Reject: a hand-crafted SVG for an icon the roster ships, `import * as Icons` / all-icons `Record` indexing, `createLucideIcon` per existing icon, repeating default props at every call site, an icon carried outside its owning domain-vocabulary row
