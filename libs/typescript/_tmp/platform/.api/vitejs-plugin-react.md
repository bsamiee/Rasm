# [API_CATALOGUE] @vitejs/plugin-react

`@vitejs/plugin-react` supplies React Fast Refresh and the Oxc-backed JSX transform as a Vite plugin. The default export `viteReact(opts?)` returns `Plugin[]` — spread into the Vite `plugins` array, never passed as a single element. The `automatic` JSX runtime is the default; classic-runtime React-import skipping is unsupported from v4 onward. `reactCompilerPreset(options?)` builds a Rolldown Babel preset wiring `babel-plugin-react-compiler` for automatic memoization, consumed through the `@rolldown/plugin-babel` bridge. The Fast Refresh runtime is injected via `viteReact.preambleCode` (a `transformIndexHtml` string) and, for module-federation remotes, `reactRefreshHost`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@vitejs/plugin-react`
- package: `@vitejs/plugin-react`
- module: `@vitejs/plugin-react` (default `viteReact`, `dist/index.d.ts`), `@vitejs/plugin-react/preamble` (ambient types-only `export {}`)
- asset: Vite plugin (returns `Plugin[]`)
- rail: react-transform

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exported and referenced types
- rail: react-transform

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]   | [DESCRIPTION]                                                          |
| :-----: | :-------------------------------- | :-------------- | :--------------------------------------------------------------------- |
|  [01]   | `Options`                         | options object  | the sole exported type; JSX transform + Fast Refresh configuration     |
|  [02]   | `RolldownBabelPreset`             | referenced type | `reactCompilerPreset` return; imported from `#optionalTypes` (`@rolldown/plugin-babel`), NOT re-exported |
|  [03]   | `ReactCompilerBabelPluginOptions` | referenced type | `reactCompilerPreset` parameter; imported from `#optionalTypes` (`babel-plugin-react-compiler`), NOT re-exported |

[PUBLIC_TYPE_SCOPE]: `Options` fields
- rail: react-transform

| [INDEX] | [FIELD]            | [TYPE]                                      | [DEFAULT]          |
| :-----: | :----------------- | :------------------------------------------ | :----------------- |
|  [01]   | `include`          | `string \| RegExp \| Array<string\|RegExp>` | `/\.[tj]sx?$/`     |
|  [02]   | `exclude`          | `string \| RegExp \| Array<string\|RegExp>` | `/\/node_modules/` |
|  [03]   | `jsxImportSource`  | `string`                                    | `'react'`          |
|  [04]   | `jsxRuntime`       | `'classic' \| 'automatic'`                  | `'automatic'`      |
|  [05]   | `reactRefreshHost` | `string`                                    | —                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory and compiler preset
- rail: react-transform

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [DESCRIPTION]                                                          |
| :-----: | :------------------------------ | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `viteReact(opts?)`              | plugin factory | default export; returns `Plugin[]` for the Vite `plugins` array        |
|  [02]   | `viteReact.preambleCode`        | static string  | Fast Refresh preamble; injected into HTML before module code via `transformIndexHtml` |
|  [03]   | `reactCompilerPreset(options?)` | preset factory | returns `RolldownBabelPreset`; reads `compilationMode` to scope the `rolldown.filter` and forwards every option to `babel-plugin-react-compiler` |
|  [04]   | `viteReactForCjs`               | CJS interop    | exported as `"module.exports"`; the `this`-bound CJS default, not a normal ESM surface |

```ts contract
// @vitejs/plugin-react (dist/index.d.ts)
import { Plugin } from 'vite'
import { ReactCompilerBabelPluginOptions, RolldownBabelPreset } from '#optionalTypes'

interface Options {
  include?: string | RegExp | Array<string | RegExp>
  exclude?: string | RegExp | Array<string | RegExp>
  jsxImportSource?: string
  jsxRuntime?: 'classic' | 'automatic'
  reactRefreshHost?: string
}

declare function viteReact(opts?: Options): Plugin[]
declare namespace viteReact { var preambleCode: string }
declare const reactCompilerPreset: (options?: ReactCompilerBabelPluginOptions) => RolldownBabelPreset

export { Options, viteReact as default, viteReactForCjs as 'module.exports', reactCompilerPreset }
```

## [04]-[IMPLEMENTATION_LAW]

[TRANSFORM_TOPOLOGY]:
- `viteReact()` returns a `Plugin[]`; spread it into the Vite `plugins` array (`...viteReact()`), never pass it as a single element.
- The Oxc JSX transform is this package's default (`jsxImportSource` maps to the Oxc transformer import-source); the separate `@vitejs/plugin-react-swc` package is a distinct SWC-backed variant and is not admitted in this pipeline.
- `jsxRuntime: 'classic'` cannot skip the React import; classic-runtime import skipping is unsupported from v4 onward, so `automatic` is the only fully-supported runtime.
- `reactRefreshHost` points a module-federation remote at the host application URL so Fast Refresh resolves a single runtime across federated bundles.
- `preambleCode` is the runtime-initialization string exposed for custom Fast Refresh injection; the plugin injects it via `transformIndexHtml` by default, and the string is exposed for SSR/custom-HTML scenarios that hand-inject it.

[REACT_COMPILER_STACK]:
- `reactCompilerPreset(options?)` accepts the full `ReactCompilerBabelPluginOptions` (`babel-plugin-react-compiler`'s `PluginOptions`); it reads `compilationMode` to scope the `rolldown.filter` and forwards every option to the compiler plugin.
- The preset is passed to the `@rolldown/plugin-babel` bridge (the README `BUILD_TOOLCHAIN` admission), which runs Babel only over React-compiler-eligible modules — Oxc owns the general JSX/TS transform, Babel is scoped to the memoization pass alone.
- Both peers are optional: `#optionalTypes` resolves the compiler types via the package `imports` map, and the types are absent until `@rolldown/plugin-babel` and `babel-plugin-react-compiler` are installed (both admitted in the platform manifest).

[LOCAL_ADMISSION]:
- `Shell/build`'s `BuildPipeline` spreads `...viteReact({ jsxRuntime: 'automatic' })` as one row of the `plugins` array; a React Compiler campaign adds `reactCompilerPreset` fed to the `@rolldown/plugin-babel` bridge, never a parallel transform.
- The plugin instruments React 19 (`react`/`react-dom` `VIEW_CORE`); the runtime render-fault companion is `react-error-boundary` (`Observability/boundary`), which catches what Fast Refresh cannot recover.
- React Compiler integration is opt-in; admit `babel-plugin-react-compiler` and `@rolldown/plugin-babel` before consuming `reactCompilerPreset`.

[RAIL_LAW]:
- Package: `@vitejs/plugin-react`
- Owns: the React JSX transform and Fast Refresh instrumentation for the Vite pipeline
- Accept: `...viteReact()` spread into the Vite `plugins` array; `reactCompilerPreset` for automatic memoization via the Babel bridge
- Reject: classic-runtime React-import skipping; the separate SWC-variant plugin; passing `viteReact()` unspread as a single plugin element
