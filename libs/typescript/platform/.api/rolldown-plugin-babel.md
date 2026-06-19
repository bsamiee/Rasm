# [API_CATALOGUE] @rolldown/plugin-babel

`@rolldown/plugin-babel` supplies the Rolldown-Babel bridge: the default `babelPlugin(options)` factory returning a `Promise<Plugin>`, the `defineRolldownBabelPreset` helper, and the `RolldownBabelPreset` type. It is the transform host `@vitejs/plugin-react`'s `reactCompilerPreset` plugs into when `babel-plugin-react-compiler` runs the React Compiler memoization pass.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@rolldown/plugin-babel`
- package: `@rolldown/plugin-babel`
- module: `@rolldown/plugin-babel` (default `babelPlugin`; ESM-only `.mjs`)
- asset: Rolldown plugin (returns `Promise<Plugin>`)
- rail: babel-transform

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: preset and option types
- rail: babel-transform

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [DESCRIPTION]                                             |
| :-----: | :------------------------ | :------------ | :-------------------------------------------------------- |
|   [1]   | `RolldownBabelPreset`     | type alias    | `{ preset, rolldown }` preset with Rolldown filter config |
|   [2]   | `RolldownBabelPresetItem` | type alias    | `PresetItem \| RolldownBabelPreset` preset list element   |
|   [3]   | `PluginOptions`           | interface     | factory options: presets, include/exclude, sourceMap      |

[PUBLIC_TYPE_SCOPE]: `PluginOptions` fields
- rail: babel-transform

| [INDEX] | [FIELD]          | [TYPE]                      | [DEFAULT]                             |
| :-----: | :--------------- | :-------------------------- | :------------------------------------ |
|   [1]   | `presets`        | `RolldownBabelPresetItem[]` | `[]`                                  |
|   [2]   | `plugins`        | `InputOptions['plugins']`   | `—`                                   |
|   [3]   | `include`        | Babel match pattern         | `/\.(?:[jt]sx?\|[cm][jt]s)(?:$\|\?)/` |
|   [4]   | `exclude`        | Babel match pattern         | `/[\/\\]node_modules[\/\\]/`          |
|   [5]   | `sourceMap`      | `boolean`                   | `true`                                |
|   [6]   | `runtimeVersion` | `string`                    | `—`                                   |
|   [7]   | `overrides`      | `InnerTransformOptions[]`   | `—`                                   |

[PUBLIC_TYPE_SCOPE]: `RolldownBabelPreset` shape
- rail: babel-transform

| [INDEX] | [FIELD]                           | [TYPE]                                | [DESCRIPTION]                        |
| :-----: | :-------------------------------- | :------------------------------------ | :----------------------------------- |
|   [1]   | `preset`                          | `PresetItem`                          | the wrapped Babel preset item        |
|   [2]   | `rolldown.filter`                 | `{ id?, moduleType?, code? }`         | Rolldown hook filters for the preset |
|   [3]   | `rolldown.optimizeDeps`           | `{ include?: string[] }`              | dep-optimizer include hints          |
|   [4]   | `rolldown.applyToEnvironmentHook` | `(environment) => boolean`            | per-environment activation predicate |
|   [5]   | `rolldown.configResolvedHook`     | `(config: ResolvedConfig) => boolean` | resolved-config activation predicate |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory and preset helper
- rail: babel-transform

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [DESCRIPTION]                                    |
| :-----: | :---------------------------------- | :------------- | :----------------------------------------------- |
|   [1]   | `babelPlugin(options)`              | plugin factory | default export; async, returns `Promise<Plugin>` |
|   [2]   | `defineRolldownBabelPreset(preset)` | preset helper  | identity helper typing a `RolldownBabelPreset`   |

```ts contract
// @rolldown/plugin-babel
import { Plugin } from 'rolldown'

declare function babelPlugin(rawOptions: PluginOptions): Promise<Plugin>
declare function defineRolldownBabelPreset(preset: RolldownBabelPreset): RolldownBabelPreset
export { type RolldownBabelPreset, babelPlugin as default, defineRolldownBabelPreset }
```

## [4]-[IMPLEMENTATION_LAW]

[BABEL_BRIDGE_TOPOLOGY]:
- `babelPlugin` is async and resolves to a single Rolldown `Plugin`; `await` it before placing it in the `plugins` array, never spread it.
- `presets` accepts plain Babel `PresetItem`s or `RolldownBabelPreset` wrappers; the wrapper form carries Rolldown `filter`/`optimizeDeps`/environment hooks so a preset can scope itself to specific module ids or build environments.
- `include`/`exclude` consume Babel match-pattern syntax (regex, string, or function), not picomatch globs; the default `include` covers `.js/.jsx/.ts/.tsx/.cjs/.mjs` and excludes `node_modules`.
- `runtimeVersion` opts in `@babel/plugin-transform-runtime`, importing helpers from `@babel/runtime`; it requires both packages installed.
- Under Babel 7, install `@types/babel__core` for correct `InputOptions` typing; the bundled types fall back to an unknown-shaped `InputOptionsFallback` otherwise.

[LOCAL_ADMISSION]:
- This bridge is required only when `@vitejs/plugin-react`'s `reactCompilerPreset` activates; pass that returned `RolldownBabelPreset` to `babelPlugin({ presets: [preset] })`.
- Admit `babel-plugin-react-compiler` and `@babel/core` 8 alongside this plugin before consuming the compiler preset; the bridge owns no transform of its own beyond delegating to Babel.
- The package is ESM-only (`.mjs`) and exposes a single default entry; there is no CJS or subpath surface.

[RAIL_LAW]:
- Package: `@rolldown/plugin-babel`
- Owns: the Rolldown-side Babel transform host and preset bridging
- Accept: `babelPlugin` awaited into the Rolldown/Vite `plugins` array; `RolldownBabelPreset` from `reactCompilerPreset`
- Reject: synchronous plugin placement; picomatch globs in `include`/`exclude`; standalone use without a consuming Babel preset
