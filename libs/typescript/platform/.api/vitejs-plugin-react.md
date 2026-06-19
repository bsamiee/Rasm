# [API_CATALOGUE] @vitejs/plugin-react

`@vitejs/plugin-react` supplies React Fast Refresh and the OXC-backed JSX transform as a Vite plugin. The default export `viteReact(opts?)` returns `Plugin[]` for the `plugins` array of `UserConfig`. The `reactCompilerPreset` helper builds a Rolldown Babel preset that wires `babel-plugin-react-compiler` for automatic memoization. The `automatic` JSX runtime is the default; classic-runtime import skipping is unsupported.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@vitejs/plugin-react`
- package: `@vitejs/plugin-react`
- module: `@vitejs/plugin-react` (default `viteReact`), `@vitejs/plugin-react/preamble`
- asset: Vite plugin (returns `Plugin[]`)
- rail: react-transform

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin option and preset types
- rail: react-transform

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]  | [DESCRIPTION]                                |
| :-----: | :-------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `Options`                         | options object | JSX transform and Fast Refresh configuration |
|  [02]   | `RolldownBabelPreset`             | re-export type | `@rolldown/plugin-babel` preset shape        |
|  [03]   | `ReactCompilerBabelPluginOptions` | re-export type | `babel-plugin-react-compiler` plugin options |

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

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [DESCRIPTION]                                      |
| :-----: | :------------------------------ | :------------- | :------------------------------------------------- |
|  [01]   | `viteReact(opts?)`              | plugin factory | default export; returns `Plugin[]` for `plugins`   |
|  [02]   | `viteReact.preambleCode`        | static string  | Fast Refresh preamble injected before module code  |
|  [03]   | `reactCompilerPreset(options?)` | preset factory | returns `RolldownBabelPreset` for the Babel bridge |

```ts contract
// @vitejs/plugin-react
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
export default viteReact

declare const reactCompilerPreset: (
  options?: ReactCompilerBabelPluginOptions
) => RolldownBabelPreset
```

## [04]-[IMPLEMENTATION_LAW]

[TRANSFORM_TOPOLOGY]:
- `viteReact()` returns a `Plugin[]`; spread it into the Vite `plugins` array, never pass it as a single element.
- The OXC JSX transform is the v6 default; `@vitejs/plugin-react-swc` is the retired predecessor surface.
- `jsxRuntime: 'classic'` cannot skip the React import; classic-runtime import skipping is removed from v4 onward.
- `reactRefreshHost` points a module-federation remote at the host application URL so Fast Refresh resolves a single runtime.
- `preambleCode` is the runtime-initialization string exposed for custom Fast Refresh injection scenarios.
- `reactCompilerPreset(options?)` accepts the full `ReactCompilerBabelPluginOptions` (the `babel-plugin-react-compiler` `PluginOptions`); it reads `compilationMode` to scope the `rolldown.filter` and forwards every option to the compiler plugin. Pass the returned preset to the `@rolldown/plugin-babel` bridge.

[LOCAL_ADMISSION]:
- React Compiler integration is opt-in; admit `babel-plugin-react-compiler` and `@rolldown/plugin-babel` before consuming `reactCompilerPreset`.
- `#optionalTypes` resolves the compiler types via the package `imports` map; the peer types are absent until the optional peers are installed.

[RAIL_LAW]:
- Package: `@vitejs/plugin-react`
- Owns: React JSX transform and Fast Refresh instrumentation for the Vite pipeline
- Accept: `react()` in the Vite `plugins` array; `reactCompilerPreset` for automatic memoization
- Reject: classic-runtime import skipping; the retired SWC plugin variant
