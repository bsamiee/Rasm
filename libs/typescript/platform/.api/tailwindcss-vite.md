# [API_CATALOGUE] @tailwindcss/vite

`@tailwindcss/vite` supplies the Tailwind CSS v4 Vite plugin that scans source files, generates CSS on demand, and integrates with Vite's HMR pipeline. `tailwindcss(opts?)` returns `Plugin[]` for inclusion in `UserConfig.plugins`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tailwindcss/vite`
- package: `@tailwindcss/vite`
- module: `@tailwindcss/vite`
- asset: Vite plugin (returns `Plugin[]`)
- rail: css

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin options
- rail: css

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]  | [DESCRIPTION]                  |
| :-----: | :-------------- | :------------- | :----------------------------- |
|  [01]   | `PluginOptions` | options object | controls CSS minification pass |

[PUBLIC_TYPE_SCOPE]: PluginOptions fields
- rail: css

| [INDEX] | [FIELD]    | [TYPE]                            | [DESCRIPTION]                                            |
| :-----: | :--------- | :-------------------------------- | :------------------------------------------------------- |
|  [01]   | `optimize` | `boolean \| { minify?: boolean }` | Lightning CSS minification; `true` in build mode default |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory
- rail: css

| [INDEX] | [SURFACE]            | [ENTRY_FAMILY] | [DESCRIPTION]                      |
| :-----: | :------------------- | :------------- | :--------------------------------- |
|  [01]   | `tailwindcss(opts?)` | plugin factory | default export; returns `Plugin[]` |

## [04]-[IMPLEMENTATION_LAW]

[CSS_TOPOLOGY]:
- Tailwind v4 resolves utility classes via source-file scanning with no `tailwind.config.js`; CSS theme tokens are declared in the CSS entry via `@theme`
- `optimize: true` applies the built-in Lightning CSS minifier; `{ minify: false }` disables minification while keeping other optimisation passes
- `tailwindcss` (main package) must be installed alongside this plugin
- `tailwindcss-react-aria-components` provides React Aria Components variant support and is configured independently in the CSS entry

[LOCAL_ADMISSION]:
- Place before `react()` in the `plugins` array: `plugins: [tailwindcss(), react()]`
- The plugin handles both dev-server incremental rebuild and the production bundle pass

[RAIL_LAW]:
- Package: `@tailwindcss/vite`
- Owns: Tailwind CSS v4 scanning, generation, and HMR integration
- Accept: `PluginOptions` passed to `tailwindcss()`
- Reject: separate `tailwind.config.js` or PostCSS-based Tailwind setup when this plugin is active
