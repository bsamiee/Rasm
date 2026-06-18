# [API_CATALOGUE] vite-plugin-inspect

`vite-plugin-inspect` exposes an inspector UI that surfaces per-module plugin transform chains, resolve-id timings, plugin metrics, and server middleware metrics for a running Vite instance. The default export `PluginInspect(options?)` returns a `Plugin`. The plugin mounts an `/__inspect/` endpoint in dev mode and optionally emits an inspection report to `.vite-inspect` in build mode.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-inspect`
- package: `vite-plugin-inspect`
- module: `vite-plugin-inspect`
- asset: Vite plugin (returns `Plugin`)
- rail: dev-tooling

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: options family
- rail: dev-tooling

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [DESCRIPTION]                                                 |
| :-----: | :------------------- | :------------- | :------------------------------------------------------------ |
|   [1]   | `ViteInspectOptions` | options object | full plugin configuration                                     |
|   [2]   | `FilterPattern`      | type alias     | `ReadonlyArray<string \| RegExp> \| string \| RegExp \| null` |

[PUBLIC_TYPE_SCOPE]: ViteInspectOptions fields
- rail: dev-tooling

| [INDEX] | [FIELD]              | [TYPE]          | [DEFAULT]         |
| :-----: | :------------------- | :-------------- | :---------------- |
|   [1]   | `dev`                | `boolean`       | `true`            |
|   [2]   | `build`              | `boolean`       | `false`           |
|   [3]   | `enabled`            | `boolean`       | deprecated        |
|   [4]   | `outputDir`          | `string`        | `'.vite-inspect'` |
|   [5]   | `include`            | `FilterPattern` | —                 |
|   [6]   | `exclude`            | `FilterPattern` | —                 |
|   [7]   | `base`               | `string`        | Vite `base`       |
|   [8]   | `silent`             | `boolean`       | `false`           |
|   [9]   | `open`               | `boolean`       | `false`           |
|  [10]   | `removeVersionQuery` | `boolean`       | `true`            |
|  [11]   | `embedded`           | `boolean`       | `false`           |

[PUBLIC_TYPE_SCOPE]: inspection data types
- rail: dev-tooling

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [DESCRIPTION]                                  |
| :-----: | :-------------------- | :------------ | :--------------------------------------------- |
|   [1]   | `ModuleInfo`          | data object   | per-module plugin timing and dependency info   |
|   [2]   | `ModuleTransformInfo` | data object   | resolved ID plus ordered transform records     |
|   [3]   | `TransformInfo`       | data object   | per-transform name, result, timing, sourcemaps |
|   [4]   | `ParsedError`         | data object   | `message`, `stack: StackFrame[]`, `raw?`       |
|   [5]   | `PluginMetricInfo`    | data object   | per-plugin transform/resolveId invoke counts   |
|   [6]   | `ServerMetrics`       | data object   | per-middleware `self`/`total` timing           |
|   [7]   | `InstanceInfo`        | data object   | root, vite ID, environment names, plugins      |
|   [8]   | `Metadata`            | data object   | `instances: InstanceInfo[]`, `embedded?`       |
|   [9]   | `SerializedPlugin`    | data object   | plugin name, enforce, hook presence flags      |
|  [10]   | `RpcFunctions`        | interface     | async RPC surface for inspector UI             |
|  [11]   | `QueryEnv`            | data object   | `{ vite: string; env: string }` query key      |
|  [12]   | `ViteInspectAPI`      | plugin API    | `{ rpc: RpcFunctions }` exposed on plugin      |
|  [13]   | `ModulesList`         | type alias    | `ModuleInfo[]`                                 |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory and RPC surface
- rail: dev-tooling

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [DESCRIPTION]                          |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------- |
|   [1]   | `PluginInspect(options?)`                       | plugin factory | default export; returns Vite `Plugin`  |
|   [2]   | `rpc.getMetadata()`                             | RPC call       | returns `Promise<Metadata>`            |
|   [3]   | `rpc.getModulesList(query)`                     | RPC call       | returns `Promise<ModulesList>`         |
|   [4]   | `rpc.getModuleTransformInfo(query, id, clear?)` | RPC call       | returns `Promise<ModuleTransformInfo>` |
|   [5]   | `rpc.getPluginMetrics(query)`                   | RPC call       | returns `Promise<PluginMetricInfo[]>`  |
|   [6]   | `rpc.getServerMetrics(query)`                   | RPC call       | returns `Promise<ServerMetrics>`       |
|   [7]   | `rpc.resolveId(query, id)`                      | RPC call       | returns `Promise<string>`              |
|   [8]   | `rpc.onModuleUpdated()`                         | RPC call       | returns `Promise<void>` (HMR signal)   |

## [4]-[IMPLEMENTATION_LAW]

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
