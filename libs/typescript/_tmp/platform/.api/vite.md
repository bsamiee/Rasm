# [API_CATALOGUE] vite

`vite` supplies the build orchestration, dev server, plugin system, and configuration surface for the platform bundle. The rolldown-backed build uses OxC as the default minifier with a `baseline-widely-available` default target; `defineConfig` / `UserConfig` wire the plugin pipeline, and `build` / `createServer` are the programmatic entrypoints.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite`
- package: `vite`
- module: `vite` (node)
- asset: build tool and dev server
- rail: build

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: config and environment types
- rail: build

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [DESCRIPTION]                                                       |
| :-----: | :------------------------ | :------------- | :------------------------------------------------------------------ |
|  [01]   | `UserConfig`              | config object  | root Vite configuration; extends `DefaultEnvironmentOptions`        |
|  [02]   | `InlineConfig`            | config object  | extends `UserConfig` with `configFile`, `configLoader`, `envPrefix` |
|  [03]   | `UserConfigFn`            | function type  | `(env: ConfigEnv) => UserConfig \| Promise<UserConfig>`             |
|  [04]   | `UserConfigExport`        | type alias     | `UserConfig \| Promise<UserConfig> \| UserConfigFn`                 |
|  [05]   | `ConfigEnv`               | context object | `command`, `mode`, `isSsrBuild`, `isPreview`                        |
|  [06]   | `BuildOptions`            | type alias     | `BuildEnvironmentOptions`                                           |
|  [07]   | `BuildEnvironmentOptions` | config object  | full build output and bundler configuration                         |
|  [08]   | `ServerOptions`           | config object  | extends `CommonServerOptions` with HMR, warmup, watch               |
|  [09]   | `CommonServerOptions`     | config object  | host, port, https, proxy, cors, headers                             |
|  [10]   | `Plugin`                  | interface      | extends `Rolldown.Plugin` with `enforce` and `apply`                |
|  [11]   | `PluginOption`            | type alias     | `Thenable<Plugin \| false \| null \| undefined \| PluginOption[]>`  |
|  [12]   | `FilterPattern`           | type alias     | `ReadonlyArray<string \| RegExp> \| string \| RegExp \| null`       |

[PUBLIC_TYPE_SCOPE]: server and output types
- rail: build

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [DESCRIPTION]                                        |
| :-----: | :---------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `ViteDevServer`   | server object  | `.listen()`, `.close()`, `.restart()`, module graph  |
|  [02]   | `PreviewServer`   | server object  | static file preview server                           |
|  [03]   | `RolldownOutput`  | output object  | bundle output from rolldown build                    |
|  [04]   | `RolldownWatcher` | watcher object | returned by `build()` when `watch` is configured     |
|  [05]   | `ProxyOptions`    | config object  | per-path proxy with rewrite, bypass, configure hooks |
|  [06]   | `HmrOptions`      | config object  | HMR port, host, protocol, overlay                    |
|  [07]   | `AliasOptions`    | config object  | path alias mappings                                  |
|  [08]   | `LibraryOptions`  | config object  | library-mode output with UMD/CJS/ESM formats         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: config helpers
- rail: build

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [DESCRIPTION]                                  |
| :-----: | :------------------------------------------ | :------------- | :--------------------------------------------- |
|  [01]   | `defineConfig(config: UserConfig)`          | config helper  | identity helper for typed config authoring     |
|  [02]   | `defineConfig(config: Promise<UserConfig>)` | config helper  | async config overload                          |
|  [03]   | `defineConfig(config: UserConfigFn)`        | config helper  | function config overload; receives `ConfigEnv` |
|  [04]   | `defineConfig(config: UserConfigExport)`    | config helper  | union overload                                 |

[ENTRYPOINT_SCOPE]: programmatic API
- rail: build

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY]   | [DESCRIPTION]                                                   |
| :-----: | :--------------------------------- | :--------------- | :-------------------------------------------------------------- |
|  [01]   | `build(inlineConfig?)`             | async function   | returns `RolldownOutput \| RolldownOutput[] \| RolldownWatcher` |
|  [02]   | `createServer(inlineConfig?)`      | async function   | returns fully initialised `ViteDevServer`                       |
|  [03]   | `preview(inlineConfig?)`           | async function   | returns `PreviewServer`                                         |
|  [04]   | `mergeConfig(defaults, overrides)` | utility function | deep-merges `UserConfig`; concatenates `plugins` arrays         |
|  [05]   | `mergeAlias(a?, b?)`               | utility function | merges alias option maps                                        |
|  [06]   | `normalizePath(id)`                | utility function | normalizes module ID separators                                 |
|  [07]   | `createFilter(include?, exclude?)` | utility function | picomatch-based include/exclude resolver for plugin authors     |
|  [08]   | `loadEnv(mode, envDir, prefixes?)` | utility function | loads `.env` files for the given mode                           |
|  [09]   | `searchForWorkspaceRoot(current)`  | utility function | walks up to find the monorepo root                              |

## [04]-[IMPLEMENTATION_LAW]

[BUILD_TOPOLOGY]:
- `target` defaults to `'baseline-widely-available'`; `minify` defaults to `'oxc'`
- `rolldownOptions` governs bundle customisation; `rollupOptions` is a deprecated alias
- `esbuild` config field is deprecated in favour of `oxc`
- `sourcemap: 'hidden'` emits source maps without inline comments
- `lib` unlocks library-mode output; `modulePreload` controls module preload injection
- `plugins` accepts nested `PluginOption[]` and promises; `enforce: 'pre'` / `'post'` selects execution order relative to core plugins
- `middlewareMode` plugs Vite into an existing Node HTTP server without binding its own port

[LOCAL_ADMISSION]:
- `UserConfig`, `Plugin`, `BuildOptions`, `ServerOptions`, and `PluginOption` are the primary planning surfaces
- `build()` returns `RolldownOutput[]` in multi-environment mode and `RolldownWatcher` when `watch` is configured
- `createFilter` is the canonical include/exclude resolver for plugin authors

[RAIL_LAW]:
- Package: `vite`
- Owns: build orchestration, dev server, plugin system, HMR, environment configuration
- Accept: `UserConfig` / `InlineConfig` via `defineConfig` or programmatic API
- Reject: direct Rolldown/Rollup API calls for build orchestration when Vite owns the pipeline
