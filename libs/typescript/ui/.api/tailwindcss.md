# [API_CATALOGUE] tailwindcss

`tailwindcss` v4 supplies a CSS-first utility framework consumed as a PostCSS plugin (`tailwindcss/plugin`) and at-import stylesheet (`tailwindcss`). The TypeScript surface is slim: `plugin` is the default export of `tailwindcss/plugin`, delivering the `createPlugin` factory and `createPlugin.withOptions`; `tailwindcss/colors` and `tailwindcss/defaultTheme` export the built-in color palette and default theme scale objects; `tailwindcss/flattenColorPalette` exports a utility for resolving nested color objects.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tailwindcss`
- package: `tailwindcss`
- namespace: `tailwindcss` (CSS import), `tailwindcss/plugin`, `tailwindcss/colors`, `tailwindcss/defaultTheme`, `tailwindcss/flattenColorPalette`
- asset: CSS runtime + TypeScript plugin API
- rail: styling

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin authoring types
- rail: styling

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                                                                                             |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------------------------------------------------- |
|   [1]   | `Plugin`               | type alias    | `PluginFn \| PluginWithConfig \| PluginWithOptions<any>`                                           |
|   [2]   | `PluginFn`             | type alias    | `(api: PluginAPI) => void`                                                                         |
|   [3]   | `PluginWithConfig`     | type alias    | `{ handler: PluginFn, config?: UserConfig }`                                                       |
|   [4]   | `PluginWithOptions<T>` | type alias    | `(options?: T) => PluginWithConfig`                                                                |
|   [5]   | `PluginAPI`            | type alias    | object with `addBase`, `addUtilities`, `matchUtilities`, `addVariant`, `theme`, `config`, `prefix` |
|   [6]   | `UserConfig`           | interface     | `{ presets?, theme?, plugins?, content?, darkMode?, prefix?, ... }`                                |
|   [7]   | `CssInJs`              | type alias    | `Record<string, string \| string[] \| CssInJs \| CssInJs[]>`                                       |
|   [8]   | `DarkModeStrategy`     | type alias    | `false \| 'media' \| 'class' \| 'selector' \| ...`                                                 |
|   [9]   | `ThemeConfig`          | type alias    | `Record<string, ThemeValue> & { extend? }`                                                         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory (`tailwindcss/plugin`)
- rail: styling

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]  | [RAIL]                                        |
| :-----: | :---------------------------------------------- | :-------------- | :-------------------------------------------- |
|   [1]   | `createPlugin(handler, config?)`                | plugin factory  | creates `PluginWithConfig` from a `PluginFn`  |
|   [2]   | `createPlugin.withOptions(pluginFn, configFn?)` | options factory | creates `PluginWithOptions<T>` with config fn |

[ENTRYPOINT_SCOPE]: PluginAPI methods (available inside `handler`)
- rail: styling

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------- | :------------- | :---------------------------------- |
|   [1]   | `addBase(base: CssInJs)`               | registration   | add base/reset styles               |
|   [2]   | `addUtilities(utilities, options?)`    | registration   | add static utility classes          |
|   [3]   | `matchUtilities(utilities, options?)`  | registration   | add dynamic value-driven utilities  |
|   [4]   | `addComponents(utilities, options?)`   | registration   | add component classes               |
|   [5]   | `matchComponents(utilities, options?)` | registration   | add dynamic value-driven components |
|   [6]   | `addVariant(name, variant)`            | registration   | add custom variant selector         |
|   [7]   | `matchVariant(name, cb, options?)`     | registration   | add dynamic value-driven variant    |
|   [8]   | `theme(path, defaultValue?)`           | lookup         | resolve theme value by dot-path     |
|   [9]   | `config(path?, defaultValue?)`         | lookup         | read config value by dot-path       |
|  [10]   | `prefix(className)`                    | transform      | apply configured class prefix       |

[ENTRYPOINT_SCOPE]: CSS stylesheet entries
- rail: styling

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :------------------------------------------ | :------------- | :-------------------------------------- |
|   [1]   | `@import "tailwindcss"`                     | CSS import     | preflight + utilities + variants        |
|   [2]   | `@import "tailwindcss/preflight"`           | CSS import     | base reset only                         |
|   [3]   | `@import "tailwindcss/theme.css"`           | CSS import     | CSS custom properties for default theme |
|   [4]   | `@import "tailwindcss/utilities"`           | CSS import     | utility classes without preflight       |
|   [5]   | `tailwindcss/colors` (default export)       | data           | nested color palette object             |
|   [6]   | `tailwindcss/defaultTheme` (default)        | data           | full default theme scale object         |
|   [7]   | `tailwindcss/flattenColorPalette` (default) | util fn        | `(colors) => Record<string, string>`    |

## [4]-[IMPLEMENTATION_LAW]

[PLUGIN_TOPOLOGY]:
- v4 is CSS-first: the primary configuration surface is `@theme`, `@layer`, `@variant`, and `@plugin` directives in CSS
- `tailwindcss/plugin` is the TypeScript API for programmatic plugin authoring; no `tailwind.config.js` is required for basic usage
- `createPlugin.withOptions` returns a callable that must be invoked (even with no args) before use in `plugins:`
- `PluginAPI.matchUtilities` accepts a `type` option for enabling `[]`-arbitrary-value syntax
- `UserConfig.darkMode` accepts `'media'`, `'class'`, `['class', selector]`, `'selector'`, or `false`

[LOCAL_ADMISSION]:
- Import the CSS via `@import "tailwindcss"` in the project stylesheet; no PostCSS config is needed with Vite's `@tailwindcss/vite` plugin.
- Consume `tailwindcss/colors` and `tailwindcss/defaultTheme` as read-only data in plugin handlers; never mutate them.
- Use `createPlugin.withOptions` when a plugin must be parameterized; plain `createPlugin` for fixed behavior.

[RAIL_LAW]:
- package: `tailwindcss`
- owns: utility class generation, CSS custom property theme tokens, PostCSS plugin pipeline, and variant authoring
- accept: `@import "tailwindcss"` stylesheet entry, `createPlugin` factory for custom utilities/variants
- reject: hand-rolling CSS utility generation that duplicates this framework's class surface
