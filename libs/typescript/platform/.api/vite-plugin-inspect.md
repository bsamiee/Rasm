# [API_CATALOGUE] vite-plugin-inspect

`vite-plugin-inspect` exposes an inspector UI that surfaces per-module plugin transform chains, resolve-id timings, plugin metrics, and server middleware metrics for a running Vite instance. The default export `PluginInspect(options?)` returns a `Plugin`. The plugin mounts an `/__inspect/` endpoint in dev mode and optionally emits an inspection report to `.vite-inspect` in build mode.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-inspect`
- package: `vite-plugin-inspect`
- module: `vite-plugin-inspect`
- asset: Vite plugin (returns `Plugin`)
- rail: dev-tooling

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: options family
- rail: dev-tooling

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [DESCRIPTION]                                                 |
| :-----: | :------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `ViteInspectOptions` | options object | full plugin configuration                                     |
|  [02]   | `FilterPattern`      | type alias     | `ReadonlyArray<string \| RegExp> \| string \| RegExp \| null` |

[PUBLIC_TYPE_SCOPE]: ViteInspectOptions fields
- rail: dev-tooling

| [INDEX] | [FIELD]              | [TYPE]          | [DEFAULT]         |
| :-----: | :------------------- | :-------------- | :---------------- |
|  [01]   | `dev`                | `boolean`       | `true`            |
|  [02]   | `build`              | `boolean`       | `false`           |
|  [03]   | `enabled`            | `boolean`       | deprecated        |
|  [04]   | `outputDir`          | `string`        | `'.vite-inspect'` |
|  [05]   | `include`            | `FilterPattern` | —                 |
|  [06]   | `exclude`            | `FilterPattern` | —                 |
|  [07]   | `base`               | `string`        | Vite `base`       |
|  [08]   | `silent`             | `boolean`       | `false`           |
|  [09]   | `open`               | `boolean`       | `false`           |
|  [10]   | `removeVersionQuery` | `boolean`       | `true`            |
|  [11]   | `embedded`           | `boolean`       | `false`           |

[PUBLIC_TYPE_SCOPE]: inspection data types
- rail: dev-tooling

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [DESCRIPTION]                                  |
| :-----: | :-------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `ModuleInfo`          | data object   | per-module plugin timing and dependency info   |
|  [02]   | `ModuleTransformInfo` | data object   | resolved ID plus ordered transform records     |
|  [03]   | `TransformInfo`       | data object   | per-transform name, result, timing, sourcemaps |
|  [04]   | `ParsedError`         | data object   | `message`, `stack: StackFrame[]`, `raw?`       |
|  [05]   | `PluginMetricInfo`    | data object   | per-plugin transform/resolveId invoke counts   |
|  [06]   | `ServerMetrics`       | data object   | per-middleware `self`/`total` timing           |
|  [07]   | `InstanceInfo`        | data object   | root, vite ID, environment names, plugins      |
|  [08]   | `Metadata`            | data object   | `instances: InstanceInfo[]`, `embedded?`       |
|  [09]   | `SerializedPlugin`    | data object   | plugin name, enforce, hook presence flags      |
|  [10]   | `RpcFunctions`        | interface     | async RPC surface for inspector UI             |
|  [11]   | `QueryEnv`            | data object   | `{ vite: string; env: string }` query key      |
|  [12]   | `ViteInspectAPI`      | plugin API    | `{ rpc: RpcFunctions }` exposed on plugin      |
|  [13]   | `ModulesList`         | type alias    | `ModuleInfo[]`                                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory and RPC surface
- rail: dev-tooling

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [DESCRIPTION]                          |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `PluginInspect(options?)`                       | plugin factory | default export; returns Vite `Plugin`  |
|  [02]   | `rpc.getMetadata()`                             | RPC call       | returns `Promise<Metadata>`            |
|  [03]   | `rpc.getModulesList(query)`                     | RPC call       | returns `Promise<ModulesList>`         |
|  [04]   | `rpc.getModuleTransformInfo(query, id, clear?)` | RPC call       | returns `Promise<ModuleTransformInfo>` |
|  [05]   | `rpc.getPluginMetrics(query)`                   | RPC call       | returns `Promise<PluginMetricInfo[]>`  |
|  [06]   | `rpc.getServerMetrics(query)`                   | RPC call       | returns `Promise<ServerMetrics>`       |
|  [07]   | `rpc.resolveId(query, id)`                      | RPC call       | returns `Promise<string>`              |
|  [08]   | `rpc.onModuleUpdated()`                         | RPC call       | returns `Promise<void>` (HMR signal)   |

## [04]-[IMPLEMENTATION_LAW]

[INSPECT_TOPOLOGY]:
- `dev: true` (default) adds the inspector UI at `/__inspect/`; performance overhead is present in dev mode
- `build: true` emits a static inspection report to `outputDir` (`.vite-inspect`) after build completion
- `include`/`exclude` filter which module IDs are tracked; omitting both tracks all modules
- `removeVersionQuery: true` strips `?v=xxx` cache-busting suffixes so the same logical module is not counted as a distinct entry
- `embedded: true` signals the plugin is embedded inside another plugin (e.g., framework meta-plugins)
- `ViteInspectAPI` is accessible via `(plugin as any).api` after the plugin resolves

[LOCAL_ADMISSION]:
- Enable only in dev or non-production builds; do not include in production bundles.
- Use `open: true` to auto-launch the inspector tab after dev server starts.
- Access `ViteInspectAPI.rpc` programmatically to build custom tooling on top of the inspection data.

[RAIL_LAW]:
- Package: `vite-plugin-inspect`
- Owns: Vite plugin pipeline introspection and transform-chain visualization
- Accept: `ViteInspectOptions` passed to `PluginInspect()`
- Reject: production builds with `dev: true` and `build: true` active simultaneously unless intentional report generation
