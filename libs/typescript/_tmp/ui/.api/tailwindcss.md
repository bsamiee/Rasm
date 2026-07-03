# [API_CATALOGUE] tailwindcss

`tailwindcss` v4 is the CSS-first utility engine that owns the class vocabulary the whole `ui` styling stack resolves against — no `tailwind.config.js`, the theme is authored in CSS through `@theme`, utilities and variants are minted through `@utility`/`@custom-variant`, and the Oxide compiler (via the separate `@tailwindcss/vite` build plugin) scans class names at build time with zero runtime. The TypeScript surface is deliberately slim: `tailwindcss/plugin` delivers the `createPlugin`/`createPlugin.withOptions` authoring factory and the `PluginAPI` handler contract, and `tailwindcss/colors`/`tailwindcss/defaultTheme` expose the built-in palette and theme scale as read-only data. tailwindcss owns only the utility class SURFACE — it carries no conflict resolution (that is `tailwind-merge` `twMerge`) and no variant compilation (that is `class-variance-authority` `cva`); the three compose into the one `cn` recipe every `.tsx` reads (`interaction/command.md#COMMAND_SURFACE`), and its `@theme` custom-property layer is the runtime target `theming/tokens.md#THEME_TOKENS` `CssVarSync` writes the OKLCH token scale into.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tailwindcss`
- package / version: `tailwindcss` @ `4.3.2`
- license: `MIT`
- module: CSS runtime (`@import "tailwindcss"` → `dist/index.css`) + dual ESM/CJS JS API; the `.` `types` condition `dist/lib.d.mts` exports `Config` + the compiler API (`compile`/`compileAst`/`Features`/`Polyfills`); the plugin-authoring types come from `./plugin`
- exports: `.` → the utility stylesheet + JS `Config`/compiler types; `./plugin` → `createPlugin` (default) + `withOptions` and the authoring types; `./colors` → palette data; `./defaultTheme` → theme scale data; `./lib/util/flattenColorPalette` → nested-color flatten util; `./theme.css` / `./preflight.css` / `./utilities.css` / `./index.css` + the `./preflight` / `./utilities` / `./index` aliases → CSS entry layers
- peer / dependency: none — self-contained; the build integration is the SEPARATE `@tailwindcss/vite` @ `4.3.2` (or `@tailwindcss/postcss`), not a subpath of this package
- rail: styling

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin authoring types (`tailwindcss/plugin`; `Config` also re-exported from `.`)
- rail: styling

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [NOTE]                                                                                          |
| :-----: | :---------------------------------- | :------------ | :--------------------------------------------------------------------------------------------- |
|  [01]   | `PluginFn` (alias `PluginCreator`)  | fn type       | `(api: PluginAPI) => void` — the handler body a plugin registers utilities/variants inside       |
|  [02]   | `PluginAPI`                         | contract      | the handler argument: `addBase`, `addUtilities`, `matchUtilities`, `addComponents`, `matchComponents`, `addVariant`, `matchVariant`, `theme`, `config`, `prefix` |
|  [03]   | `PluginWithConfig`                  | type alias    | `{ handler: PluginFn; config?: Partial<Config> }` — the `createPlugin` return; the value a `plugins`/`@plugin` entry accepts |
|  [04]   | `PluginWithOptions<T>`              | fn type       | `(options?: T) => PluginWithConfig` carrying `__isOptionsFunction` — the `createPlugin.withOptions` return |
|  [05]   | `Config`                            | config type   | the (optional, legacy) JS config shape `{ theme?, plugins?, content?, darkMode?, prefix?, presets?, … }` loaded via `@config`; `darkMode` takes `'media' \| 'class' \| 'selector' \| ['variant', …] \| false` |
|  [06]   | `PluginsConfig`                     | type alias    | the `Config.plugins` element union — `PluginFn \| PluginWithConfig \| PluginWithOptions<any>`     |
|  [07]   | `ThemeConfig`                       | type alias    | the theme scale record (`extend` merges, bare replaces) — the shape behind the `@theme` layer     |
|  [08]   | `PluginUtils`                       | interface     | `{ theme, colors }` passed to a `theme`-function config value so one token derives from another   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: CSS-first authoring directives (`@import "tailwindcss"` stylesheet)
- rail: styling

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [NOTE]                                                                                          |
| :-----: | :------------------------------------- | :------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `@import "tailwindcss"`                | engine entry   | preflight + theme + utilities + variants; the one line that mounts the engine                   |
|  [02]   | `@theme { --token: … }`                | token declare  | mints design tokens as `:root` CSS custom properties AND their derived utilities (`--color-*` → `bg-*`) |
|  [03]   | `@utility <name> { … }`                | utility mint   | registers a first-class utility that participates in every variant/modifier — replaces v3 `addUtilities` in CSS |
|  [04]   | `@variant <name> { … }` / `@custom-variant` | variant apply/mint | `@variant` applies a variant inside custom CSS; `@custom-variant name (&:where(…))` mints a new variant |
|  [05]   | `@apply <utilities>` / `@reference`    | inline / import | `@apply` inlines utility declarations into custom CSS; `@reference "…"` imports the theme into a scoped `<style>` block without re-emitting CSS |
|  [06]   | `@source "<glob>"` / `@source not`     | scan control   | adds/excludes content paths from the class scan when auto-detection misses a source             |
|  [07]   | `@config "<path>"` / `@plugin "<path>"` | JS bridge      | loads a legacy JS `Config` / registers a JS plugin from CSS — the seam a `createPlugin` result enters |
|  [08]   | `@layer base/components/utilities` / `theme()` / `--alpha()` / `--spacing()` | cascade / fn | explicit layer placement and the CSS helper functions resolving a token, an alpha channel, and a spacing step |

[ENTRYPOINT_SCOPE]: JS plugin factory (`tailwindcss/plugin`)
- rail: styling

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]  | [NOTE]                                                                                     |
| :-----: | :---------------------------------------------- | :-------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `createPlugin(handler, config?)`                | plugin factory  | default export of `tailwindcss/plugin`; wraps a `PluginCreator` into a registrable plugin  |
|  [02]   | `createPlugin.withOptions(pluginFn, configFn?)` | options factory | parameterized plugin; the returned value MUST be invoked (even with no args) before use — carries `__isOptionsFunction = true` |

[ENTRYPOINT_SCOPE]: `PluginAPI` handler methods (inside `createPlugin(handler)`)
- rail: styling

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [NOTE]                                                                                       |
| :-----: | :------------------------------------- | :------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `matchUtilities(utils, { values, type, supportsNegativeValues, modifiers })` | dynamic utility | the value-driven mint: `type` enables `[]`-arbitrary-value syntax, `modifiers` enables `/`-suffix modifiers |
|  [02]   | `matchVariant(name, cb, { values, sort })` | dynamic variant | value-parameterized variant (`data-[state]:`, `group-[…]:`); `sort` orders overlapping arbitrary values |
|  [03]   | `addUtilities(utils, opts?)` / `addComponents` / `addBase` | static register | fixed CSS-in-JS-object utility / component / reset classes                                    |
|  [04]   | `addVariant(name, value)` / `matchComponents` | register       | a fixed variant selector; the value-driven component twin of `matchUtilities`                |
|  [05]   | `theme(path, default?)` / `config(path?, default?)` | lookup         | resolve a `@theme`/config value by dot-path inside the handler                               |
|  [06]   | `prefix(className)`                    | transform      | applies the configured class prefix to a raw class string                                    |

[ENTRYPOINT_SCOPE]: data + CSS layer subpaths
- rail: styling

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [NOTE]                                                                         |
| :-----: | :----------------------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `tailwindcss/colors` (default)                         | data           | the built-in nested color palette object (`{ red: { 50, …, 950 }, … }`)         |
|  [02]   | `tailwindcss/defaultTheme` (default)                   | data           | the full default theme scale (`{ screens, spacing, fontFamily, … }`)            |
|  [03]   | `tailwindcss/lib/util/flattenColorPalette` (default)   | util fn        | `(colors) => Record<string, string>` — flattens a nested palette for a plugin's `matchUtilities` values (the REAL subpath; not `tailwindcss/flattenColorPalette`) |
|  [04]   | `@import "tailwindcss/preflight"` / `/theme.css` / `/utilities` | CSS layer      | the individual layers, importable separately to compose a non-default pipeline  |

## [04]-[IMPLEMENTATION_LAW]

[STYLING_TOPOLOGY]:
- v4 is CSS-first: the primary configuration surface is `@theme`/`@utility`/`@custom-variant`/`@variant` directives in the stylesheet; a `tailwind.config.js` is NEVER required and is loaded only via `@config` for legacy interop
- `@theme` is dual: each `--token` becomes BOTH a `:root` CSS custom property (resolvable at runtime, `bg-[--token]`) AND the source of a generated utility family (`--color-accent` → `bg-accent`/`text-accent`)
- `createPlugin.withOptions` returns a callable that must be invoked before landing in a `plugins:` array or `@plugin`; `matchUtilities`/`matchVariant` are the value-driven mints where one registration owns an unbounded value space, never a hand-enumerated class per value
- `darkMode` accepts `'media'`, `'class'`, `'selector'`, or a `['variant', selector]` pair; `@custom-variant dark (&:where(.dark, .dark *))` is the CSS-first equivalent
- the class scan is automatic over the project's source tree (Oxide); `@source`/`@source not` only correct a missed or over-broad root

[STACKING]:
- the `cn` recipe (canonical): tailwindcss owns the utility class VOCABULARY, `class-variance-authority` `cva`/`cx` compiles variants→classes and joins, and `tailwind-merge` `twMerge` resolves conflicts over that same vocabulary — composed once as `const cn = (...a) => twMerge(cx(...a))` (`interaction/command.md#COMMAND_SURFACE`, `class-variance-authority.md`, `tailwind-merge.md`), mounted at the composition root, never re-stitched per `.tsx`
- the `@theme` runtime target: `theming/tokens.md#THEME_TOKENS` generates a perceptually-uniform OKLCH scale via `colorjs.io` `Color.steps` and writes it onto the document `:root` custom properties through one `effect` `Stream.runForEach` fold over a `SubscriptionRef` (`../.api/effect.md` `Stream`/`SubscriptionRef`) — the `@theme` tokens are the names, the effect Stream is the live sync, so a theme swap is one record push and every utility resolves the new value with no re-render cascade
- the animation layer: `tw-animate-css` is a pure-CSS `@import` landing immediately after `@import "tailwindcss"` (`tw-animate-css.md`, `theming/tokens.md`), supplying `data-[state=open]:animate-in`/`data-[state=closed]:animate-out` utilities keyed off the Radix `data-state` every `@radix-ui/*`-derived overlay emits, with `duration-*`/`ease-*` timing resolving against the `@theme` tokens — declarative enter/exit, never a JS animation controller
- the RAC variant plugin: `tailwindcss-react-aria-components` is a `tailwindcss/plugin` (registered via `@plugin`) that mints `data-*` variants (`open:`, `placement-left:`) for `react-aria-components` DOM state (`tailwindcss-react-aria-components.md`, `interaction/picker.md`) — one registered plugin over the same `PluginAPI`, never hand-authored `[data-rac]` selectors
- effect-tier vocabulary keying: a `cva` variant axis mirrors a domain vocabulary by keying its record off the SAME `Schema.Literal`/`Data.taggedEnum` union the effect owner closes (a `CommandAction` role, a `ThemeTokens` scale step; `../.api/effect.md`) — the variant axis is a projection of the domain vocabulary the utility classes render, not a parallel enum

[LOCAL_ADMISSION]:
- author the theme in CSS via `@theme`; reserve the JS `createPlugin` surface for genuinely dynamic utilities (`matchUtilities` over a token-derived value space), never a static class a `@utility` block owns
- consume `tailwindcss/colors`/`tailwindcss/defaultTheme` as read-only data in a plugin's `values`; never mutate them
- mount the Oxide engine through `@tailwindcss/vite`; no PostCSS config is required — a manual `@tailwindcss/postcss` chain is the fallback, not the default
- reference `tailwindcss/lib/util/flattenColorPalette`, never the non-existent `tailwindcss/flattenColorPalette`

[RAIL_LAW]:
- package: `tailwindcss`
- owns: the CSS-first utility class vocabulary, `@theme` custom-property token generation, the `@utility`/`@custom-variant`/`@variant` authoring directives, and the `tailwindcss/plugin` `PluginAPI`
- accept: `@import "tailwindcss"` + `@theme`/`@utility`/`@custom-variant`, `createPlugin`/`withOptions` for dynamic utilities, `matchUtilities`/`matchVariant` for value-driven mints, `@config`/`@plugin` for JS interop, the `@tailwindcss/vite` build integration
- reject: a hand-rolled utility-class generator duplicating this surface; Tailwind conflict resolution inside `cva` or a plugin (that is `twMerge`); a `tailwind.config.js` where `@theme` suffices; the phantom `tailwindcss/flattenColorPalette` subpath; a JS animation controller for a `data-state` transition the `tw-animate-css` layer drives
