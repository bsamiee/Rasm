# [API_CATALOGUE] vite-plugin-svgr

`vite-plugin-svgr` is an `enforce: 'pre'` Vite plugin that turns a `*.svg?react` import into a React component: its `load` hook reads the file, runs `@svgr/core` `transform` with the `@svgr/plugin-jsx` default caller, then transpiles the emitted JSX — through `vite` `transformWithOxc` under rolldown-vite or `transformWithEsbuild` otherwise. The default export `vitePluginSvgr(options?)` returns one `Plugin` for the `Shell/build.md` `BuildPipeline` set; the components it emits are consumed by `ui`. The default `include` is `**/*.svg?react`, so only the `?react` query is transformed and a plain `./icon.svg` import stays a static asset URL — partitioning the SVG space against `vite-plugin-image-optimizer` by import shape. The `vite-plugin-svgr/client` subpath ships the ambient `*.svg?react` module declaration `tsc` needs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-svgr`
- package: `vite-plugin-svgr`
- version: `5.2.0`
- license: `MIT`
- module: dual ESM/CJS via `exports` map (`import` → `dist/index.js`/`dist/index.d.ts`, `require` → `dist/index.cjs`/`dist/index.d.cts`), `"type": "module"`; `./client` subpath exports ONLY `client.d.ts` (types-only, the ambient module). One default export `vitePluginSvgr`
- asset: Vite plugin (returns `Plugin`), `enforce: 'pre'`. Peer `vite >=3.0.0`. Deps `@rollup/pluginutils@^5.3.0` (the `FilterPattern` + `createFilter`), `@svgr/core@^8.1.0` (the `Config` + `transform`), `@svgr/plugin-jsx@^8.1.0` (the default JSX caller) are BUNDLED, not peer — the consumer manifest owns only `vite`
- rail: asset-transform — SVG→React-component at load, dev and build
- catalog-verdict: KEEP; the SVG-to-component transform admitted at `Shell/build.md`, one plugin row

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: options and the Oxc-param derivation
- rail: asset-transform

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [BOUNDARY_NOTE]                                                                           |
| :-----: | :---------------------- | :------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `VitePluginSvgrOptions` | options object | the whole config; the only interface export                                               |
|  [02]   | `OxcTransformOptions`   | derived alias  | `NonNullable<Parameters<typeof transformWithOxc>[2]>` — the third arg of vite's Oxc transform |

[PUBLIC_TYPE_SCOPE]: `VitePluginSvgrOptions` fields
- rail: asset-transform

| [INDEX] | [FIELD]          | [TYPE]                                     | [DEFAULT]           | [BOUNDARY_NOTE]                                                        |
| :-----: | :--------------- | :----------------------------------------- | :------------------ | :--------------------------------------------------------------------- |
|  [01]   | `svgrOptions`    | `@svgr/core` `Config`                      | —                   | the full SVGR transform config (fields below)                          |
|  [02]   | `include`        | `@rollup/pluginutils` `FilterPattern`      | `"**/*.svg?react"`  | scopes transform to the `?react` query — NOT `**/*.svg`                |
|  [03]   | `exclude`        | `@rollup/pluginutils` `FilterPattern`      | —                   | exclusion resolved by the same `createFilter`                          |
|  [04]   | `oxcOptions`     | `OxcTransformOptions`                       | —                   | LIVE knob under rolldown-vite; spread over the plugin's `{ lang: 'jsx' }` base into `transformWithOxc` |
|  [05]   | `esbuildOptions` | `vite` `EsbuildTransformOptions`            | —                   | DORMANT under rolldown-vite; spread over `{ loader: 'jsx' }` only on the non-rolldown esbuild path |

[PUBLIC_TYPE_SCOPE]: `svgrOptions` (`@svgr/core` `Config`) — the SVGR transform axis
- rail: asset-transform
- Resolves from the bundled `@svgr/core` peer; the design sets the runtime/output shape here, not on the Vite side.

| [INDEX] | [FIELD]                    | [TYPE]                                 | [BOUNDARY_NOTE]                                                       |
| :-----: | :------------------------- | :------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `jsxRuntime`               | `'classic' \| 'classic-preact' \| 'automatic'` | keep `'automatic'` to match the React 19 automatic runtime and plugin-react |
|  [02]   | `typescript`               | `boolean`                              | emit `.tsx` component source                                          |
|  [03]   | `icon`                     | `boolean \| string \| number`          | size the SVG to `1em`/an explicit dimension for inline icons          |
|  [04]   | `ref`                      | `boolean`                              | forward a `ref` to the root `<svg>`                                   |
|  [05]   | `titleProp` / `descProp`   | `boolean`                              | accept `title`/`titleId`/`desc`/`descId` props (a11y)                 |
|  [06]   | `expandProps`              | `boolean \| 'start' \| 'end'`          | spread caller props onto the `<svg>`                                  |
|  [07]   | `svgProps`                 | `Record<string,string>`                | static attributes injected on the `<svg>`                            |
|  [08]   | `replaceAttrValues`        | `Record<string,string>`                | recolor at build (e.g. `'#000': 'currentColor'`)                     |
|  [09]   | `dimensions`               | `boolean`                              | keep/strip intrinsic width/height                                    |
|  [10]   | `svgo` / `svgoConfig`      | `boolean` / `svgo` `Config`            | run svgo on the SVG before componentization, with this config        |
|  [11]   | `exportType` / `namedExport` | `'named' \| 'default'` / `string`    | shape the emitted export (default `default`, matching `*.svg?react`) |
|  [12]   | `memo`                     | `boolean`                              | wrap the component in `React.memo`                                   |
|  [13]   | `plugins`                  | `ConfigPlugin[]`                       | the SVGR plugin chain; `@svgr/plugin-jsx` is always the default caller |

[PUBLIC_TYPE_SCOPE]: `vite-plugin-svgr/client` ambient module
- rail: asset-transform

| [INDEX] | [MODULE]        | [SHAPE]                                                                                             | [BOUNDARY_NOTE]                                              |
| :-----: | :-------------- | :------------------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `*.svg?react`   | default `React.FunctionComponent<React.ComponentProps<"svg"> & { title?; titleId?; desc?; descId? }>` | add `vite-plugin-svgr/client` to tsconfig `types` to type it |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory
- rail: asset-transform

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [BOUNDARY_NOTE]                                                    |
| :-----: | :------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `vitePluginSvgr(options?)` | plugin factory | default export; `(options?: VitePluginSvgrOptions) => Plugin`     |

```ts contract
// vite-plugin-svgr
import { FilterPattern } from '@rollup/pluginutils'
import { Config } from '@svgr/core'
import { EsbuildTransformOptions, Plugin, transformWithOxc } from 'vite'

type OxcTransformOptions = NonNullable<Parameters<typeof transformWithOxc>[2]>
interface VitePluginSvgrOptions {
  svgrOptions?: Config
  esbuildOptions?: EsbuildTransformOptions
  oxcOptions?: OxcTransformOptions
  include?: FilterPattern
  exclude?: FilterPattern
}
declare function vitePluginSvgr(options?: VitePluginSvgrOptions): Plugin
export { vitePluginSvgr as default }

// consumer usage — the ?react query is required (default include)
import Icon from './icon.svg?react'
```

## [04]-[IMPLEMENTATION_LAW]

[TRANSFORM_TOPOLOGY]:
- The `load` hook fires for ids passing `createFilter(include, exclude)`; it strips the `?react`/`#hash` postfix, reads the file, runs `@svgr/core` `transform(svgCode, svgrOptions, { filePath, caller: { defaultPlugins: [jsx] } })`, then transpiles the JSX.
- Transpile dispatch is on the BUNDLER, not on option presence: `this.meta.rolldownVersion != null` → `transformWithOxc(code, id, { lang: 'jsx', ...oxcOptions })`; otherwise `transformWithEsbuild(code, id, { loader: 'jsx', ...esbuildOptions })`. Under `vite@8.1.3` (rolldown-vite) the Oxc path is live, so `oxcOptions` governs and `esbuildOptions` is dead code for this workspace.
- The default `include` `**/*.svg?react` means an SVG is a component ONLY when imported with the `?react` query; `import url from './icon.svg'` stays a plain asset URL.

[INTEGRATION_LAW]:
- Stack with `vite.md`: one `PluginOption` row in `vite.md`'s `UserConfig.plugins` host at `enforce: 'pre'`, resolving into the pre band ahead of the normal-band `@vitejs/plugin-react` transform. Sits alongside `@vitejs/plugin-react` in `Shell/build.md`: `enforce: 'pre'` runs svgr's `load` before plugin-react's JSX/Fast-Refresh transform sees the module, so svgr emits JSX and `viteReact()` finishes the JSX→JS + Fast Refresh pass. Set `svgrOptions.jsxRuntime: 'automatic'` to match plugin-react's `automatic` default and the `react` (VIEW_CORE) 19 runtime — a `'classic'` mismatch re-imports React redundantly.
- Partitions the SVG asset space against `vite-plugin-image-optimizer`: svgr claims only `?react` imports (→ JS component modules), image-optimizer optimizes static `.svg`/raster assets — the two never double-process one file because they discriminate on the `?react` query, and static `.svg` imported without the query flows to the optimizer.
- `include`/`exclude` are `@rollup/pluginutils` `FilterPattern`, the same shape `vite` `createFilter` consumes; the design narrows the component set with the same filter vocabulary the rest of the plugin pipeline uses, never a bespoke matcher.
- Not an `effect` rail: a build/dev-time source transform, never in the `platform` `Layer` graph; the `ui` side imports the emitted component and only the ambient `vite-plugin-svgr/client` reference makes `tsc` accept the `?react` import.

[RAIL_LAW]:
- Package: `vite-plugin-svgr`
- Owns: `*.svg?react` → React-component transformation at load (dev and build), delegating to `@svgr/core` + `@svgr/plugin-jsx` and transpiling via `vite`'s Oxc/esbuild
- Accept: `VitePluginSvgrOptions` on `vitePluginSvgr()`; `svgrOptions` for output shape; `include`/`exclude` `FilterPattern`; the `?react` import query; the `vite-plugin-svgr/client` ambient reference
- Reject: importing `*.svg` without the `?react` query when a component is intended; a manual `@svgr/core` call in build code; assuming an esbuild transpile under rolldown-vite (`oxcOptions` is the live knob); `jsxRuntime: 'classic'` under the automatic-runtime pipeline
