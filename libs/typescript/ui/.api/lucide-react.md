# [TS_UI_API_LUCIDE_REACT]

`lucide-react` mints its SVG-icon roster as named exports of one uniform `LucideIcon` forwardRef shape — vocabulary, not an API surface — so a design row keys a named icon as its identity (`icon: PlusSquare`), never a hand-authored SVG. `LucideProps` is uniform across the roster, `LucideProvider` sets project defaults once, `Icon` renders a runtime `iconNode`, `createLucideIcon` mints an absent glyph, and `sideEffects: false` tree-shakes each named import.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the one icon shape, its props, and the raw node data the dynamic and custom paths consume — every named icon and `createLucideIcon` result is exactly `LucideIcon`.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [CAPABILITY]                          |
| :-----: | :-------------------- | :------------- | :------------------------------------ |
|  [01]   | `LucideIcon`          | icon component | every named icon's shape              |
|  [02]   | `LucideProps`         | icon props     | uniform per-icon prop shape           |
|  [03]   | `IconNode`            | node data      | raw `[tag, attrs][]` for `Icon`       |
|  [04]   | `SVGAttributes`       | svg attrs      | passthrough SVG-attr surface          |
|  [05]   | `LucideConfig`        | config record  | provider default props                |
|  [06]   | `LucideProviderProps` | provider props | provider props with `children`        |
|  [07]   | `IconComponentProps`  | dynamic props  | `Icon` prop shape carrying `iconNode` |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the named vocabulary, the dynamic and custom icons, and the default-prop context.

| [INDEX] | [SURFACE]                                          | [SHAPE]   | [CAPABILITY]                     |
| :-----: | :------------------------------------------------- | :-------- | :------------------------------- |
|  [01]   | `<IconName {...LucideProps} />`                    | component | named icon keyed per design row  |
|  [02]   | `<IconName>Icon` / `Lucide<IconName>`              | component | collision-free alias entry       |
|  [03]   | `Icon`                                             | component | render a runtime `iconNode`      |
|  [04]   | `createLucideIcon(string, IconNode) -> LucideIcon` | factory   | mint a roster-absent glyph       |
|  [05]   | `LucideProvider(LucideProviderProps)`              | component | set project defaults at the root |
|  [06]   | `useLucideContext() -> LucideProps`                | hook      | read active provider defaults    |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- every named export is `LucideIcon`; selection is a named import keyed to a design row, so this catalog states the shape and the selection law, and the roster lives in the package barrel.
- `size` (default `24`) drives width and height, `strokeWidth` defaults `2`, `absoluteStrokeWidth` holds line weight constant as `strokeWidth / size`, `color` maps to SVG `stroke`, and `ref` forwards to the `<svg>` element.
- `sideEffects: false` bundles only the icons named-imported; `import * as Icons` or an all-icons `Record` index defeats it, and dynamic `Icon` + `iconNode` is the one opt-in runtime-selection path.
- `.` barrel exports both `Activity` and `ActivityIcon`; the `.prefixed`/`.suffixed` barrels give one scheme where an icon name shadows a DOM global.

[STACKING]:
- `react` + `react-aria`(`.api/react.md`, `.api/react-aria.md`): each icon is a `ForwardRefExoticComponent` composed inside react-aria headless rows — `cmdk` `Command.Item`, buttons, menu items — carrying `aria-hidden` where the label already carries meaning.
- `class-variance-authority` + `tailwind-merge`(`.api/class-variance-authority.md`, `.api/tailwind-merge.md`): icon `className`/`size`/`color` flow through the `cn = twMerge(clsx(...))` fold, so icon sizing is a `cva` variant row read through `cn`, never an ad-hoc class.
- `effect`(`libs/typescript/.api/effect.md`): the icon vocabulary is a static `Record` keyed by a `Schema.Literal` action domain with no `Effect` wrapping, since icons are pure presentational; one `CommandAction` row binds label, icon, keybinding, role, and intent key together.
- `view/overlay` + `LucideProvider` (within-lib): an `as const satisfies Record<string, Overlay.Command>` row carries a named `LucideIcon` as its `icon` identity, and `LucideProvider` at the SPA root sets project-wide `size`/`strokeWidth`/`color` once from the `token/scale`/`token/theme` defaults.

[LOCAL_ADMISSION]:
- import named icons statically, keyed to a design vocabulary row; `import * as Icons` or an all-icons `Record` index defeats tree-shaking.
- `LucideProvider` at the root sets `size`/`strokeWidth`/`color` once, and every call site inherits them.
- `Icon` + `iconNode` renders a runtime-chosen icon, `createLucideIcon` mints only a glyph the roster lacks, and the `.prefixed`/`.suffixed` alias entry resolves a DOM-global collision.
