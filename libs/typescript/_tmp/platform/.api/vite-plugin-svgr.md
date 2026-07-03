# [API_CATALOGUE] vite-plugin-svgr

`vite-plugin-svgr` transforms SVG files into React components during Vite builds, delegating SVG-to-component conversion to `@svgr/core` and transpilation to esbuild or Oxc. The default export `vitePluginSvgr(options?)` returns a `Plugin` ready for the Vite `plugins` array.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-svgr`
- package: `vite-plugin-svgr`
- module: `vite-plugin-svgr` (ESM), `vite-plugin-svgr/client` (ambient declarations)
- asset: Vite plugin (returns `Plugin`)
- rail: asset-transform

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: options family
- rail: asset-transform

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [DESCRIPTION]                                         |
| :-----: | :---------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `VitePluginSvgrOptions` | options object | full plugin configuration                             |
|  [02]   | `OxcTransformOptions`   | type alias     | `NonNullable<Parameters<typeof transformWithOxc>[2]>` |

[PUBLIC_TYPE_SCOPE]: VitePluginSvgrOptions fields
- rail: asset-transform

| [INDEX] | [FIELD]          | [TYPE]                     | [DESCRIPTION]                  |
| :-----: | :--------------- | :------------------------- | :----------------------------- |
|  [01]   | `svgrOptions`    | `Config` (from @svgr/core) | SVGR transform configuration   |
|  [02]   | `esbuildOptions` | `EsbuildTransformOptions`  | esbuild transform options      |
|  [03]   | `oxcOptions`     | `OxcTransformOptions`      | Oxc transform options          |
|  [04]   | `include`        | `FilterPattern`            | glob/regex patterns to include |
|  [05]   | `exclude`        | `FilterPattern`            | glob/regex patterns to exclude |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory
- rail: asset-transform

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [DESCRIPTION]                         |
| :-----: | :------------------------- | :------------- | :------------------------------------ |
|  [01]   | `vitePluginSvgr(options?)` | plugin factory | default export; returns Vite `Plugin` |

## [04]-[IMPLEMENTATION_LAW]

[SVG_TOPOLOGY]:
- SVG imports matching `include` (default: `**/*.svg`) are transformed to React components via `@svgr/core`
- transpilation is delegated to esbuild by default; Oxc is used when `oxcOptions` is provided
- `FilterPattern` accepts `ReadonlyArray<string | RegExp> | string | RegExp | null`

[LOCAL_ADMISSION]:
- Pass `svgrOptions` to control SVGR output (JSX runtime, SVG props, title injection, etc.).
- Use `include`/`exclude` to restrict which SVG files are treated as components vs. static assets.
- Import SVGs as `import ReactComponent from './icon.svg'` after the plugin is registered.

[RAIL_LAW]:
- Package: `vite-plugin-svgr`
- Owns: SVG-to-React-component transformation at Vite build and dev time
- Accept: `VitePluginSvgrOptions` passed to `vitePluginSvgr()`
- Reject: manual SVGR invocation outside this plugin when the build pipeline is Vite-owned
