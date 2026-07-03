# [API_CATALOGUE] @rolldown/plugin-babel

`@rolldown/plugin-babel` is the Rolldown-side Babel transform host: `babelPlugin(options)` (default, async → `Promise<Plugin>`) runs a Babel preset/plugin set over matched modules, and `defineRolldownBabelPreset` types a `RolldownBabelPreset` — a Babel `PresetItem` wrapped with Rolldown/Vite scoping hooks (`filter`/`optimizeDeps`/`applyToEnvironmentHook`/`configResolvedHook`). It owns NO transform of its own; it delegates entirely to `@babel/core`. Its `PluginOptions` inherit the real Babel `InputOptions` (`targets`, `assumptions`, `plugins`, `comments`, `parserOpts`, …) via `InnerTransformOptions`, so the browserslist `targets` seam and full Babel config flow through it. In `platform` it exists for ONE reason (`Shell/build.md`): when `@vitejs/plugin-react`'s `reactCompilerPreset` activates, the React Compiler runs as a Babel pass and this bridge is the only host that executes it under Rolldown/OXC — a conditional, build-time-only dependency that retires the day Rolldown gains a native React Compiler transform.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@rolldown/plugin-babel`
- package: `@rolldown/plugin-babel`
- version: `0.2.3` (central pin `pnpm-workspace.yaml`)
- license: `MIT`
- runtime: build-time only — a Rolldown/Vite plugin; ESM-only (`type: module`), single entry `dist/index.mjs`, no CJS, no subpaths
- module: `@rolldown/plugin-babel` (default `babelPlugin`); `exports` is a BARE string `"./dist/index.mjs"` with NO `types` condition — TypeScript resolves the sibling `dist/index.d.mts`, which imports from `@babel/core`, `rolldown`, AND `vite` (the preset hooks are Vite-typed)
- peer: `@babel/core ^7.29.0 || ^8.0.0-rc.1` (REQUIRED — the transform engine), `rolldown ^1.0.0-rc.5` (REQUIRED — `Plugin`/`GeneralHookFilter`/`ModuleTypeFilter`), `vite ^8.0.0` (OPTIONAL — `ResolvedConfig`/environment types), `@babel/plugin-transform-runtime` + `@babel/runtime` (OPTIONAL — only for `runtimeVersion`)
- dep: `picomatch ^4.0.4` — backs the preset-level `rolldown.filter.id`/`code` matcher (NOT the top-level `include`/`exclude`, which are Babel matchPatterns)
- side-effects: `false` (tree-shakeable) — but irrelevant: it never enters the runtime bundle
- asset: Rolldown/Vite plugin factory (async, `Promise<Plugin>`)
- rail: babel-transform — the Rolldown-side Babel host and preset bridge
- catalog-verdict: KEEP; build-time-only, admitted at `Shell/build.md` ONLY when `reactCompilerPreset` activates; collapse-watch — retires if Rolldown ships a native React Compiler pass (no Babel host then needed)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: preset + option types
- rail: babel-transform
- EXPORT SURFACE: only `RolldownBabelPreset` (type), `babelPlugin` (default), and `defineRolldownBabelPreset` are exported; `PluginOptions`/`InnerTransformOptions`/`RolldownBabelPresetItem` are the INTERNAL structural shape of `babelPlugin`'s argument (declared, referenced, not importable) — documented here because they define the option surface, not because a consumer imports them.
- Babel types resolve conditionally: `IsAny<babel.InputOptions>` false → the real `@babel/core` (v8) types; else the v7 fallback `InputOptionsFallback` (`{ plugins?: unknown[]; presets?: unknown[] }`). Install `@types/babel__core` under Babel 7 for correct typing.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]        | [CONSUMER / BOUNDARY]                                                        |
| :-----: | :------------------------ | :------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `RolldownBabelPreset`     | exported type alias  | `{ preset: PresetItem; rolldown: {...} }` — a Babel preset + Rolldown/Vite scoping |
|  [02]   | `RolldownBabelPresetItem` | internal type        | `PresetItem \| RolldownBabelPreset` — the `presets` array element           |
|  [03]   | `PluginOptions`           | internal interface   | `babelPlugin` arg; extends `Omit<InnerTransformOptions, 'include'\|'exclude'>` |
|  [04]   | `InnerTransformOptions`   | internal interface   | `Partial<Pick<InputOptions, …>>` + `presets` — the inherited Babel config surface |

[PUBLIC_TYPE_SCOPE]: `PluginOptions` fields — own + inherited-Babel
- rail: babel-transform
- The first block is `PluginOptions`-own; the second is inherited from Babel `InputOptions` through `InnerTransformOptions` (the `targets` = browserslist seam lives here, not in a bespoke option).

| [INDEX] | [FIELD]          | [TYPE]                            | [DEFAULT]                              | [SOURCE]        |
| :-----: | :--------------- | :-------------------------------- | :------------------------------------- | :-------------- |
|  [01]   | `presets`        | `RolldownBabelPresetItem[]`       | `[]`                                   | own (`InnerTransformOptions`) |
|  [02]   | `include`        | Babel matchPattern                | `/\.(?:[jt]sx?\|[cm][jt]s)(?:$\|\?)/`  | own             |
|  [03]   | `exclude`        | Babel matchPattern                | `/[\/\\]node_modules[\/\\]/`           | own             |
|  [04]   | `sourceMap`      | `boolean`                         | `true`                                 | own             |
|  [05]   | `runtimeVersion` | `string`                          | `—`                                    | own             |
|  [06]   | `overrides`      | `InnerTransformOptions[]`         | `—`                                    | own             |
|  [07]   | `targets`        | Babel `targets` (browserslist query/targets object) | `—` (browserslist seam)  | inherited Babel |
|  [08]   | `assumptions`    | Babel `assumptions`               | `—`                                    | inherited Babel |
|  [09]   | `plugins`        | Babel `PluginItem[]`              | `—` (raw Babel plugins beside presets) | inherited Babel |
|  [10]   | `comments`/`compact`/`retainLines` | Babel codegen flags     | `—`                                    | inherited Babel |
|  [11]   | `parserOpts`/`generatorOpts`/`cwd` | Babel parse/gen/cwd     | `—`                                    | inherited Babel |

[PUBLIC_TYPE_SCOPE]: `RolldownBabelPreset` shape — the per-preset scoping axis
- rail: babel-transform

| [INDEX] | [FIELD]                           | [TYPE]                                | [CONSUMER / BOUNDARY]                                    |
| :-----: | :-------------------------------- | :------------------------------------ | :------------------------------------------------------ |
|  [01]   | `preset`                          | `PresetItem`                          | the wrapped Babel preset item                           |
|  [02]   | `rolldown.filter`                 | `{ id?: GeneralHookFilter; moduleType?: ModuleTypeFilter; code?: GeneralHookFilter }` | Rolldown hook filters — `id`/`code` are picomatch-backed (`{ include?, exclude? }`) |
|  [03]   | `rolldown.optimizeDeps`           | `{ include?: string[] }`              | dep-optimizer include hints for the preset              |
|  [04]   | `rolldown.applyToEnvironmentHook` | `(env: PartialEnvironment) => boolean`| Vite per-environment activation (client vs SSR)         |
|  [05]   | `rolldown.configResolvedHook`     | `(config: ResolvedConfig) => boolean` | Vite resolved-config activation predicate               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory + preset helper
- rail: babel-transform

```ts contract
// @rolldown/plugin-babel — types via dist/index.d.mts (exports has no `types` condition)
import type { Plugin, GeneralHookFilter, ModuleTypeFilter } from 'rolldown'
import type { ResolvedConfig } from 'vite'

declare function babelPlugin(rawOptions: PluginOptions): Promise<Plugin>            // default; async, AWAIT before placing in plugins[]
declare function defineRolldownBabelPreset(preset: RolldownBabelPreset): RolldownBabelPreset  // identity typer

export { type RolldownBabelPreset, babelPlugin as default, defineRolldownBabelPreset }
```

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                             |
| :-----: | :---------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `babelPlugin(options)`              | plugin factory | default; async → `await` into the Vite/Rolldown `plugins` array  |
|  [02]   | `defineRolldownBabelPreset(preset)` | preset helper  | types a hand-authored `RolldownBabelPreset` (not needed for `reactCompilerPreset`, which already returns one) |

## [04]-[IMPLEMENTATION_LAW]

[BABEL_BRIDGE_TOPOLOGY]:
- `babelPlugin` is ASYNC and resolves to ONE Rolldown `Plugin`; `await` it before the `plugins` array — never spread, never place the promise. It carries no transform: every effect is the delegated `@babel/core` run over the resolved preset/plugin set.
- TWO filter layers with DIFFERENT syntax: the top-level `include`/`exclude` (which files Babel touches) are Babel matchPatterns (regex/string/function per `babeljs.io/docs/options#matchpattern`, NOT picomatch); the per-preset `rolldown.filter.id`/`code` are Rolldown `GeneralHookFilter`s (picomatch-backed `{ include?, exclude? }`) scoping a preset to specific module ids/content. `moduleType` filters by Rolldown module type.
- per-preset environment scoping: `applyToEnvironmentHook(env)` and `configResolvedHook(config)` are Vite-typed predicates — a preset activates only in a matching Vite environment (e.g. client-only React Compiler, skipped for the SSR graph); `optimizeDeps.include` feeds the dep pre-bundle.
- `runtimeVersion` opts in `@babel/plugin-transform-runtime` so helpers import from `@babel/runtime` instead of inlining per file; it requires both optional peers installed.

[INTEGRATION_LAW]:
- Stack with `@vitejs/plugin-react` `reactCompilerPreset` (the SOLE consumer): `reactCompilerPreset(opts)` returns a `RolldownBabelPreset` wiring `babel-plugin-react-compiler` (`libs/typescript/_tmp/platform/.api/vitejs-plugin-react.md` declares the reciprocal); pass it as `babelPlugin({ presets: [preset] })`, `await`, and place after `viteReact()` in the `Shell/build.md` `plugins` array. The `rolldown.filter` the preset carries scopes the compiler to the React source, so node_modules and non-component files skip the Babel pass.
- Stack with `browserslist` (`libs/typescript/_tmp/platform/.api/browserslist.md`): `PluginOptions.targets` is the Babel-side browserslist seam, but the workspace delegates target resolution to Vite (`target: 'baseline-widely-available'`), so `targets` stays UNSET here — the React Compiler pass is syntax-level and does not need a per-target lowering. Set `targets` only if a preset must down-level beyond Vite's own transform.
- Stack with `vite`/`rolldown` (`Shell/build.md`): the plugin rides Vite's Rolldown bundler; its `Plugin` return is a Rolldown plugin, and the Vite-typed preset hooks integrate with Vite's environment API — it is a build-graph participant, never a runtime `Layer` and never in the shipped bundle.
- Stack with `@babel/core` 8: admit `@babel/core` 8 + `babel-plugin-react-compiler` alongside this bridge before consuming the compiler preset; the bridge's typing follows `@babel/core`'s (v8 native, v7 needs `@types/babel__core`).

[LOCAL_ADMISSION]:
- admit this bridge ONLY when `reactCompilerPreset` activates; it has no standalone use in `platform` (there is no other Babel pass) — a build without the React Compiler drops it entirely.
- construct at the `Shell/build.md` composition of the Vite `plugins` array; `await babelPlugin(...)` there. Never import it into runtime folder code.
- prefer the preset-level `rolldown.filter` for scoping over broad top-level `include`/`exclude` narrowing — the preset knows its own module surface.

[RAIL_LAW]:
- Package: `@rolldown/plugin-babel`
- Owns: the Rolldown-side Babel transform host (`babelPlugin`) and the Rolldown/Vite-scoped preset bridge (`RolldownBabelPreset`/`defineRolldownBabelPreset`)
- Accept: `await babelPlugin({ presets: [reactCompilerPreset(...)] })` into the `Shell/build.md` plugins array; the per-preset `rolldown.filter`/`applyToEnvironmentHook` for scoping; `runtimeVersion` (with both optional peers) for helper extraction
- Reject: synchronous plugin placement or spreading the promise; conflating the two filter layers (top-level `include`/`exclude` = Babel matchPattern, preset `rolldown.filter` = picomatch); standalone use without a consuming Babel preset; importing it into runtime/`Layer` code; a bespoke `targets` where Vite already owns target resolution
