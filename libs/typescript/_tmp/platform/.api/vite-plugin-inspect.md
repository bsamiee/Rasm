# [API_CATALOGUE] vite-plugin-inspect

`vite-plugin-inspect` instruments the Vite plugin pipeline: it records every module's ordered transform chain, per-plugin `transform`/`resolveId` invoke-count and time, and per-middleware server timing, then serves them at `/__inspect/` in dev (a `sirv` static UI) or emits a static report to `.vite-inspect` in build. The default export `PluginInspect(options?)` returns one `Plugin` for the `Shell/build.md` `BuildPipeline` set; `Shell/build.md` strips it from the production bundle. Beyond the UI it exposes a `vite-dev-rpc` birpc surface on the plugin's `api.rpc` (`ViteInspectAPI`) — the programmatic seam a CI perf-assertion reads (`getPluginMetrics`, `getModuleTransformInfo`) without the browser. It pairs with `rollup-plugin-visualizer` as orthogonal build diagnostics: inspect answers "which plugin transformed this / which is slow", visualizer answers "what is large".

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-inspect`
- package: `vite-plugin-inspect`
- version: `11.4.1`
- license: `MIT`
- module: ESM-ONLY — `exports` `.` → `dist/index.mjs` (types `dist/index.d.mts`), `./nuxt` → `dist/nuxt.mjs`, `./*` passthrough; NO CJS build, `"type": "module"`. Default export `PluginInspect`; named type exports `ViteInspectOptions`, `FilterPattern`, `ViteInspectAPI`
- asset: Vite plugin (returns `Plugin`) exposing `api.rpc`. Peer `vite: ^6.0.0 || ^7.0.0-0 || ^8.0.0-0` — a hard ABI floor, vite 5 is unsupported. Bundled deps drive the surface: `vite-dev-rpc` (the birpc transport), `sirv` (the `/__inspect/` static server), `open` (the `open` auto-launch), `unplugin-utils` (`createFilter`/`FilterPattern`), `error-stack-parser-es` (the `ParsedError.stack` `StackFrame`)
- rail: dev-tooling — pipeline introspection UI + programmatic RPC
- catalog-verdict: KEEP; the build-graph diagnostics admitted at `Shell/build.md`, dev-only, production-stripped

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: options + filter
- rail: dev-tooling

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [BOUNDARY_NOTE]                                              |
| :-----: | :------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `ViteInspectOptions` | options object | the whole config (fields below)                             |
|  [02]   | `FilterPattern`      | type alias     | `ReadonlyArray<string \| RegExp> \| string \| RegExp \| null` |

[PUBLIC_TYPE_SCOPE]: `ViteInspectOptions` fields
- rail: dev-tooling

| [INDEX] | [FIELD]              | [TYPE]          | [DEFAULT]         | [BOUNDARY_NOTE]                                             |
| :-----: | :------------------- | :-------------- | :---------------- | :---------------------------------------------------------- |
|  [01]   | `dev`                | `boolean`       | `true`            | mount the inspector at `/__inspect/` in dev (has overhead)  |
|  [02]   | `build`              | `boolean`       | `false`           | emit a static report to `outputDir` after build            |
|  [03]   | `enabled`            | `boolean`       | deprecated        | superseded by `dev`/`build`                                 |
|  [04]   | `outputDir`          | `string`        | `'.vite-inspect'` | build-report directory (build mode only)                   |
|  [05]   | `include`            | `FilterPattern` | —                 | module-id filter to inspect                                 |
|  [06]   | `exclude`            | `FilterPattern` | —                 | module-id filter to skip                                    |
|  [07]   | `base`               | `string`        | vite `base`       | inspector UI base URL                                       |
|  [08]   | `silent`             | `boolean`       | `false`           | suppress the URL print                                      |
|  [09]   | `open`               | `boolean`       | `false`           | auto-launch the inspect page via `open`                    |
|  [10]   | `removeVersionQuery` | `boolean`       | `true`            | collapse `?v=xxx` HMR variants into one logical module      |
|  [11]   | `embedded`           | `boolean`       | `false`           | embedded-in-another-plugin mode (meta-plugins)             |

[PUBLIC_TYPE_SCOPE]: inspection data shapes — the RPC payloads
- rail: dev-tooling

| [INDEX] | [SYMBOL]              | [SHAPE]                                                                                             | [BOUNDARY_NOTE]                                          |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `TransformInfo`       | `{ name; result?; start; end; order?; sourcemaps?; error?: ParsedError }`                          | one transform step; `start`/`end` are the timing pair    |
|  [02]   | `ParsedError`         | `{ message; stack: StackFrame[]; raw? }`                                                            | `StackFrame` from `error-stack-parser-es`                |
|  [03]   | `ModuleInfo`          | `{ id; plugins: {name;transform?;resolveId?}[]; deps: string[]; importers: string[]; virtual: boolean; totalTime; invokeCount; sourceSize; distSize }` | per-module plugin timing + graph edges |
|  [04]   | `ModulesList`         | `ModuleInfo[]`                                                                                      | the module roster                                        |
|  [05]   | `ModuleTransformInfo` | `{ resolvedId; transforms: TransformInfo[] }`                                                       | resolved id + its ordered transform chain                |
|  [06]   | `PluginMetricInfo`    | `{ name; enforce?; transform: {invokeCount;totalTime}; resolveId: {invokeCount;totalTime} }`       | per-plugin cost — the CI perf-assertion source           |
|  [07]   | `ServerMetrics`       | `{ middleware?: Record<string, {name;self;total}[]> }`                                              | per-middleware self/total timing                         |
|  [08]   | `SerializedPlugin`    | `{ name; enforce?; resolveId; load; transform; generateBundle; handleHotUpdate; api }`             | hook-presence descriptors per plugin                     |
|  [09]   | `InstanceInfo`        | `{ root; vite; environments: string[]; plugins: SerializedPlugin[]; environmentPlugins: Record<string, number[]> }` | one Vite instance; `environmentPlugins` indexes `plugins` |
|  [10]   | `Metadata`            | `{ instances: InstanceInfo[]; embedded? }`                                                          | `getMetadata()` root                                     |
|  [11]   | `QueryEnv`            | `{ vite: string; env: string }`                                                                    | instance+environment RPC key                             |
|  [12]   | `RpcFunctions`        | interface                                                                                           | the 7 async RPC methods (entrypoints below)              |
|  [13]   | `ViteInspectAPI`      | `{ rpc: RpcFunctions }`                                                                             | exposed on the plugin `api`                              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory
- rail: dev-tooling

| [INDEX] | [SURFACE]                 | [ENTRY_FAMILY] | [BOUNDARY_NOTE]                                          |
| :-----: | :------------------------ | :------------- | :------------------------------------------------------- |
|  [01]   | `PluginInspect(options?)` | plugin factory | default export; `(options?: ViteInspectOptions) => Plugin` |

[ENTRYPOINT_SCOPE]: `ViteInspectAPI.rpc` — the birpc programmatic surface (`plugin.api.rpc`)
- rail: dev-tooling
- All async; the `vite-dev-rpc` client the browser UI and any custom tooling share. `query: QueryEnv` selects the instance+environment.

| [INDEX] | [SURFACE]                                       | [RETURNS]                       | [BOUNDARY_NOTE]                             |
| :-----: | :---------------------------------------------- | :------------------------------ | :------------------------------------------ |
|  [01]   | `getMetadata()`                                 | `Promise<Metadata>`             | instances + plugins roster                  |
|  [02]   | `getModulesList(query)`                         | `Promise<ModulesList>`          | every tracked module                        |
|  [03]   | `getModuleTransformInfo(query, id, clear?)`     | `Promise<ModuleTransformInfo>`  | one module's transform chain; `clear` resets |
|  [04]   | `getPluginMetrics(query)`                       | `Promise<PluginMetricInfo[]>`   | per-plugin cost roster                       |
|  [05]   | `getServerMetrics(query)`                       | `Promise<ServerMetrics>`        | middleware timing                            |
|  [06]   | `resolveId(query, id)`                          | `Promise<string>`               | run the resolve chain for an id             |
|  [07]   | `onModuleUpdated()`                             | `Promise<void>`                 | HMR refresh signal                          |

```ts contract
// vite-plugin-inspect
import { Plugin } from 'vite'

interface QueryEnv { vite: string; env: string }
interface RpcFunctions {
  getMetadata: () => Promise<Metadata>
  getModulesList: (query: QueryEnv) => Promise<ModulesList>
  getModuleTransformInfo: (query: QueryEnv, id: string, clear?: boolean) => Promise<ModuleTransformInfo>
  getPluginMetrics: (query: QueryEnv) => Promise<PluginMetricInfo[]>
  getServerMetrics: (query: QueryEnv) => Promise<ServerMetrics>
  resolveId: (query: QueryEnv, id: string) => Promise<string>
  onModuleUpdated: () => Promise<void>
}
interface ViteInspectAPI { rpc: RpcFunctions }
declare function PluginInspect(options?: ViteInspectOptions): Plugin
export { ViteInspectOptions, PluginInspect as default }
export type { ViteInspectAPI }
```

## [04]-[IMPLEMENTATION_LAW]

[INSPECT_TOPOLOGY]:
- `dev: true` (default) mounts the `sirv`-served `/__inspect/` UI and records transform timing per module — real dev-server overhead, so gate it on a diagnostic flag.
- `build: true` emits a static `.vite-inspect` report post-build; `dev` and `build` are independent switches.
- `removeVersionQuery: true` folds `?v=xxx` HMR cache-bust variants so one logical module is one row, not many.
- `include`/`exclude` (`FilterPattern`, from `unplugin-utils` `createFilter`) scope the tracked module set; both omitted tracks all.

[INTEGRATION_LAW]:
- Stack with `vite.md`: one `PluginOption` row in `vite.md`'s `UserConfig.plugins` host, stripped from the production bundle by `defineConfig`'s `ConfigEnv` (`command`/`mode`) — the reciprocal of `vite.md`'s "the dev atom-inspector plugin (`vite-plugin-inspect`) is stripped from the production bundle by `command`/`mode`" admission. `Shell/build.md`'s stated "strip the development-build atom inspector from the production bundle" is exactly this exclusion — never ship `dev`/`build` inspection in the prod plugin array. `include`/`exclude` compose `unplugin-utils` `createFilter` over the same `FilterPattern` vocabulary `vite.md`'s `createFilter`/`withFilter` own, never a bespoke matcher.
- Orthogonal to `rollup-plugin-visualizer`: inspect owns the transform-chain + plugin/middleware METRICS surface, visualizer owns the post-build bundle-SIZE treemap. Both are build-analysis-flag-gated; they answer different questions and never overlap, so the design admits both as one diagnostic pair, not competing tools.
- The `api.rpc` (`ViteInspectAPI`, a `vite-dev-rpc` birpc client) is the headless seam: a CI check resolves the inspect plugin by `name` from the resolved config, reads `getPluginMetrics(query)`/`getModuleTransformInfo(query, id)`, and asserts a plugin's `transform.totalTime` budget — pipeline-perf regression proof without the browser UI. This is the advanced use the design composes when it wants machine-checked build health.
- Not an `effect` rail: dev/build tooling only, never in the `platform` `Layer` graph, never lifted through `Effect.tryPromise`; its ESM-only + vite 6/7/8 peer is the ABI gate the manifest must satisfy.

[RAIL_LAW]:
- Package: `vite-plugin-inspect`
- Owns: Vite plugin-pipeline introspection — per-module transform chains, plugin/middleware metrics — as a dev UI, a build report, and a birpc programmatic API
- Accept: `ViteInspectOptions` on `PluginInspect()`; `plugin.api.rpc` for headless metric queries; `include`/`exclude` `FilterPattern`
- Reject: shipping it in the production plugin array; hand-parsing the transform pipeline when `api.rpc` exposes it; conflating its transform-chain view with `rollup-plugin-visualizer`'s size treemap; a vite<6 host
