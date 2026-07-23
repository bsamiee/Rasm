# [TS_UI_API_LUCIDE_REACT]

`lucide-react` ships its full SVG-icon roster as named `ForwardRefExoticComponent` exports, every one the same `LucideIcon` type — the roster is a vocabulary, not an API surface. Collapse keys a named icon per domain row (a `CommandAction` → its `LucideIcon`); the icon is the row's identity, never a hand-authored SVG. `LucideProps` (`size`/`strokeWidth`/`absoluteStrokeWidth`/`color` + SVG attrs) is uniform across the roster, `LucideProvider`/`useLucideContext` sets project-wide defaults once, `Icon` renders a runtime-chosen `iconNode`, and `createLucideIcon` mints a custom icon in the same shape. Named imports are individually tree-shaken (`sideEffects: false`) — importing five icons bundles five, never the roster.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lucide-react`
- package: `lucide-react` (ISC)
- peer: `react catalog` (`.api/react.md` — icons are `ForwardRefExoticComponent`, ref → `<svg>`; the React 19 spine here)
- catalog-verdict: KEEP
- runtime: isomorphic React components, `sideEffects: false` — named imports bundle individually; SSR-safe (pure presentational, no effects)
- entry: `.` barrel (each icon exported plain `Activity` AND `Activity as ActivityIcon`) + the `Icon`/`createLucideIcon`/`LucideProvider`/`useLucideContext` surface; alternate `.prefixed` (`LucideActivity`) / `.suffixed` (`ActivityIcon`) barrels resolve name collisions (e.g. `Menu`/`Link` vs DOM globals)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the one icon shape + its props + dynamic-icon data
- rail: token/icon
- Every named icon and `createLucideIcon` result is exactly `LucideIcon`; `LucideProps` is uniform; `IconNode` is the raw data the dynamic/custom paths consume. All are exported; the exact type shapes are in the signature fence below.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                                     |
| :-----: | :-------------------- | :---------------- | :------------------------------------------------------ |
|  [01]   | `LucideIcon`          | icon component    | every named icon; the `CommandSpec.icon` field type     |
|  [02]   | `LucideProps`         | icon props        | the uniform per-icon prop shape; SVG attrs ride through |
|  [03]   | `IconNode`            | raw node data     | input to `Icon` / `createLucideIcon`                    |
|  [04]   | `SVGAttributes`       | svg attr subset   | the passthrough SVG surface                             |
|  [05]   | `LucideConfig`        | provider defaults | the `size`/`color`/`stroke` default record              |
|  [06]   | `LucideProviderProps` | provider props    | `LucideConfig` + `children`, inferred at use            |
|  [07]   | `IconComponentProps`  | dynamic prop      | `LucideProps` + `iconNode`, the `Icon` prop shape       |

[LUCIDE_ICON]: `LucideIcon = ForwardRefExoticComponent<Omit<LucideProps,"ref">&RefAttributes<SVGSVGElement>>`
[LUCIDE_PROPS]: `LucideProps = SVGAttributes&{size?:string|number;absoluteStrokeWidth?:boolean}`
[SVGATTRIBUTES]: `SVGAttributes = Partial<SVGProps<SVGSVGElement>>`
[ICON_NODE]: `IconNode = [elementName:SVGElementType,attrs:Record<string,string>][]`
[ICON_COMPONENT_PROPS]: `IconComponentProps = LucideProps&{iconNode:IconNode}`
[LUCIDE_CONFIG]: `LucideConfig.size: unknown` `LucideConfig.color: unknown` `LucideConfig.strokeWidth: unknown` `LucideConfig.absoluteStrokeWidth: unknown` `LucideConfig.className: unknown`
[LUCIDE_PROVIDER_PROPS]: `LucideProviderProps = Partial<LucideConfig>&{children:ReactNode}`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: named vocabulary, dynamic + custom icons, and default-prop context
- rail: token/icon
- Named export is the common case (static, tree-shaken, vocabulary-keyed); `Icon`/`createLucideIcon` are the runtime/custom escape hatches; `LucideProvider` is the one place project defaults live. Every named export is `LucideIcon`-typed with the uniform `LucideProps`.

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                              |
| :-----: | :------------------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `<IconName {...LucideProps} />`                    | named icon     | keyed per `CommandAction` row; the icon is the action's identity |
|  [02]   | `<IconName>Icon` / `.prefixed` `Lucide<IconName>`  | alias entry    | collision-free import where a name shadows a DOM global          |
|  [03]   | `Icon` (`IconComponentProps` forwardRef)           | dynamic icon   | render a runtime-chosen icon via `iconNode` — never a static row |
|  [04]   | `createLucideIcon(iconName, iconNode): LucideIcon` | custom factory | mint a project glyph in the `LucideIcon` shape                   |
|  [05]   | `LucideProvider(LucideProviderProps)`              | provider       | SPA root — set project-wide `size`/`strokeWidth`/`color` once    |
|  [06]   | `useLucideContext(): LucideProps`                  | context read   | read the active provider defaults                                |

## [04]-[IMPLEMENTATION_LAW]

[ICON_TOPOLOGY]:
- one shape, N names: every named export is `LucideIcon`; the roster is domain vocabulary, so a catalog documents the shape and the selection law, never enumerates the roster. Selection is by named import keyed to a design row.
- prop law: `size` (default `24`) drives both `width` and `height`; `strokeWidth` (default `2`); `absoluteStrokeWidth` renders stroke as `strokeWidth / size` so line weight stays constant across sizes; `color` maps to the SVG `stroke`; `ref` forwards to the `<svg>` element.
- tree-shaken, no barrel side-effects: `sideEffects: false` means five named imports bundle five icons; a `import * as Icons` or an icon picked from a `Record` of all icons defeats it. Dynamic `Icon` + `iconNode` is the only runtime-selection path and is opt-in per call.
- dual naming: the main barrel exports both `Activity` and `ActivityIcon`; the `.prefixed`/`.suffixed` barrels give a single naming scheme when an icon name collides with a DOM global — pick one convention per app.

[INTEGRATION_LAW]:
- Stack with `view/compose.md` command surface: the `CommandAction` `as const satisfies Record<string, CommandSpec>` vocabulary carries a named `LucideIcon` per row (`icon: PlusSquare`), and `ActionIcon` resolves it by row — the icon is the action's identity keyed by a `Schema.Literal` domain, never a per-`.tsx` inline SVG and never `createLucideIcon` per action.
- Stack with `react` + `react-aria` (`.api/react.md`, `.api/react-aria.md`): icons are `ForwardRefExoticComponent`s composed inside react-aria headless view rows (`cmdk` `Command.Item`, buttons, menu items) with `aria-hidden` where the label carries meaning.
- Stack with `class-variance-authority` + `tailwind-merge` (`.api/class-variance-authority.md`, `.api/tailwind-merge.md`): icon `className`/`color`/`size` flow through the `cn = (...a) => twMerge(cx(...a))` variant recipe, so an icon's sizing is a `cva` row read through `cn`, not ad-hoc classes.
- Stack with `effect` (`libs/typescript/.api/effect.md`): the icon vocabulary is a static `Record` keyed by a `Schema.Literal` action domain — no `Effect` wrapping (icons are pure presentational); the design binds the icon at the same seam the `CommandGateway` binds the intent, so one `CommandAction` row owns label, icon, keybinding, role, and intent key together.
- Stack with `LucideProvider` at the SPA root: project-wide `size`/`strokeWidth`/`color` are set once through the provider context (the `token/scale.md`/`token/theme.md` defaults), so no icon call-site repeats them.

[LOCAL_ADMISSION]:
- Import named icons statically and key them to a design vocabulary row; never `import * as Icons` or index an all-icons `Record` (defeats tree-shaking).
- Set project defaults once via `LucideProvider`; never repeat `size`/`strokeWidth`/`color` at every call site.
- Use `Icon` + `iconNode` only for runtime-chosen icons; use `createLucideIcon` only for a genuinely custom glyph absent from the roster — never for an icon the package already ships.
- Prefer the `.prefixed`/`.suffixed` alias entry when an icon name collides with a DOM global.

[RAIL_LAW]:
- Package: `lucide-react`
- Owns: the `LucideIcon` icon vocabulary (one shared forwardRef SVG shape), the `LucideProps` size/stroke/color surface, the dynamic `Icon`, the `createLucideIcon` custom factory, and the `LucideProvider`/`useLucideContext` default-prop context
- Accept: static named imports keyed by a design vocabulary row, `LucideProvider` defaults at the root, `Icon` for runtime selection, `createLucideIcon` for genuinely-absent glyphs, the alias entries for collisions, `className`/`size` through the `cn` recipe
- Reject: a hand-crafted SVG for an icon the roster ships, `import * as Icons` / all-icons `Record` indexing, `createLucideIcon` per existing icon, repeating default props at every call site, an icon carried outside its owning domain-vocabulary row
