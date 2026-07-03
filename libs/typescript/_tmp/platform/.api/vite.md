# [API_CATALOGUE] vite

`vite` is the build orchestrator, dev server, and plugin host of the platform bundle — the single surface every sibling build plugin composes into through the `UserConfig.plugins` array. The build is rolldown-backed: `minify` defaults to `'oxc'`, `target` defaults to `'baseline-widely-available'`, and the deprecated `esbuild`/`rollupOptions` fields alias the `oxc`/`rolldownOptions` surface. `defineConfig` authors the typed config; `build`/`createServer`/`preview` are the programmatic entrypoints; the v6+ Environment API (`createBuilder`/`DevEnvironment`/`BuildEnvironment`/`perEnvironmentState`) is the multi-environment build graph the co-hosted SPA bundle folds through; `createFilter`/`withFilter` are the include/exclude helpers every plugin author composes against, and `transformWithOxc`/`transformWithEsbuild` (+ the re-exported `EsbuildTransformOptions`) are the bundler-agnostic JSX/TS transpile helpers a `load`-hook plugin (`vite-plugin-svgr`) dispatches through — the same plugin-author tier as `createFilter`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite`
- package: `vite`
- module: `vite` (`dist/node/index.d.ts`, Node API), `vite/client` (ambient `import.meta.env` client types), `vite/module-runner`, `vite/types/*`
- asset: rolldown-backed build tool, dev server, and plugin host
- rail: build

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: config authoring
- rail: build

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [DESCRIPTION]                                                          |
| :-----: | :-------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `UserConfig`          | config object  | root config; extends `DefaultEnvironmentOptions` with `plugins`/`build`/`server`/`resolve`/`css`/`environments` |
|  [02]   | `InlineConfig`        | config object  | `UserConfig` plus `configFile`, `configLoader`, `envFile`, `forceOptimizeDeps` |
|  [03]   | `ConfigEnv`           | context object | `command: 'build'\|'serve'`, `mode`, `isSsrBuild?`, `isPreview?`       |
|  [04]   | `ResolvedConfig`      | resolved shape | fully resolved config `resolveConfig` returns; passed to plugin hooks  |
|  [05]   | `UserConfigFn`        | function type  | `(env: ConfigEnv) => UserConfig \| Promise<UserConfig>`                |
|  [06]   | `UserConfigFnObject`  | function type  | `(env: ConfigEnv) => UserConfig`                                       |
|  [07]   | `UserConfigFnPromise` | function type  | `(env: ConfigEnv) => Promise<UserConfig>`                             |
|  [08]   | `UserConfigExport`    | type alias     | `UserConfig \| Promise<UserConfig> \| UserConfigFn`                    |

[PUBLIC_TYPE_SCOPE]: plugin pipeline
- rail: build

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [DESCRIPTION]                                                            |
| :-----: | :------------------------ | :------------- | :---------------------------------------------------------------------- |
|  [01]   | `Plugin<A>`               | interface      | `extends Rolldown.Plugin<A>`; adds `enforce: 'pre'\|'post'`, `apply`, Vite hooks (`config`, `configResolved`, `configureServer`, `transformIndexHtml`, `hotUpdate`) |
|  [02]   | `PluginOption`            | type alias     | `Thenable<Plugin \| false \| null \| undefined \| PluginOption[]>`; nested/async plugin arrays flatten |
|  [03]   | `HookHandler<T>`          | utility type   | unwraps a plugin hook to its handler function form                      |
|  [04]   | `FilterPattern`           | type alias     | `ReadonlyArray<string \| RegExp> \| string \| RegExp \| null`           |
|  [05]   | `IndexHtmlTransform`      | hook union     | HTML transform hook; object form carries `order` + `handler`            |
|  [06]   | `IndexHtmlTransformResult`| result union   | `string \| HtmlTagDescriptor[] \| { html, tags }`                       |
|  [07]   | `HtmlTagDescriptor`       | tag shape      | `{ tag, attrs?, children?, injectTo? }` for `transformIndexHtml`        |
|  [08]   | `Alias` / `AliasOptions`  | resolve config | path-alias entry / entry map                                            |

[PUBLIC_TYPE_SCOPE]: build output and library mode
- rail: build

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [DESCRIPTION]                                                          |
| :-----: | :------------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `BuildOptions`             | type alias    | alias of `BuildEnvironmentOptions`                                     |
|  [02]   | `BuildEnvironmentOptions`  | config object | `target` (default `'baseline-widely-available'`), `minify` (default `'oxc'`), `sourcemap`, `rolldownOptions`, `lib`, `modulePreload`, `cssMinify`, `manifest`, `ssrManifest`, `assetsInlineLimit` |
|  [03]   | `LibraryOptions`           | config object | `entry`, `name`, `formats` (default `['es','umd']`), `fileName`, `cssFileName` |
|  [04]   | `LibraryFormats`           | string union  | `'es' \| 'cjs' \| 'umd' \| 'iife'`                                     |
|  [05]   | `ModulePreloadOptions`     | config object | module-preload polyfill and dependency-resolution injection            |
|  [06]   | `OxcOptions`               | config object | the `oxc` transform field (`build.oxc`) replacing deprecated `esbuild`; `extends Omit<Rolldown TransformOptions, …>` |
|  [07]   | `EsbuildTransformOptions`  | transform options | re-exported esbuild `TransformOptions`; the `options?` arg of `transformWithEsbuild` — `vite-plugin-svgr` imports it from `vite` to type its `esbuildOptions` passthrough (the Oxc transform-call options has NO vite-named export, so svgr derives it via `NonNullable<Parameters<typeof transformWithOxc>[2]>`) |
|  [08]   | `Manifest` / `ManifestChunk` | build manifest | `build.manifest` output mapping source → emitted chunk; the precache/`vite-plugin-pwa` input |
|  [09]   | `Rolldown` / `Rollup`      | namespace      | re-exported rolldown/rollup-compat namespaces; `RolldownOutput`/`RolldownOutput[]`/`RolldownWatcher` are the `build()` return surfaced through `Rolldown` — but rolldown's `TransformOptions` (`rolldown/utils`) is NOT surfaced here |

[PUBLIC_TYPE_SCOPE]: server and preview
- rail: build

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [DESCRIPTION]                                                          |
| :-----: | :-------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `ServerOptions`       | config object  | extends `CommonServerOptions` with `hmr`, `warmup`, `watch`, `middlewareMode`, `fs`, `preTransformRequests` |
|  [02]   | `CommonServerOptions` | config object  | `host`, `port`, `strictPort`, `https`, `proxy`, `cors`, `headers`, `open` |
|  [03]   | `PreviewOptions`      | config object  | extends `CommonServerOptions`; static preview of the built bundle      |
|  [04]   | `ViteDevServer`       | server object  | `.listen()`, `.close()`, `.restart()`, `.transformRequest()`, `.reloadModule()`, `moduleGraph`, `environments: Record<'client'\|'ssr', DevEnvironment>`, `pluginContainer` |
|  [05]   | `PreviewServer`       | server object  | static-file preview server with `httpServer` + `resolvedUrls`          |
|  [06]   | `ProxyOptions`        | config object  | per-path proxy with `target`, `rewrite`, `bypass`, `configure`         |
|  [07]   | `HmrOptions`          | config object  | HMR `port`, `host`, `protocol`, `overlay`, `clientPort`                |
|  [08]   | `AppType`             | string union   | `'spa' \| 'mpa' \| 'custom'`; selects history-fallback + index serving |

[PUBLIC_TYPE_SCOPE]: Environment API (v6+ multi-environment build)
- rail: build

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [DESCRIPTION]                                                          |
| :-----: | :-------------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `EnvironmentOptions`        | config object  | per-environment `resolve`/`dev`/`build`/`consumer` under `config.environments` |
|  [02]   | `DevEnvironmentOptions`     | config object  | dev-side environment config (`createEnvironment`, `moduleRunnerTransform`) |
|  [03]   | `Environment`               | union alias    | `DevEnvironment \| BuildEnvironment \| ScanEnvironment \| UnknownEnvironment` |
|  [04]   | `DevEnvironment`            | class          | dev-time module graph + transform pipeline per environment            |
|  [05]   | `BuildEnvironment`          | class          | build-time bundling pipeline per environment                          |
|  [06]   | `RunnableDevEnvironment`    | class          | dev environment with an in-process `ModuleRunner` (`runnerImport`)     |
|  [07]   | `BuilderOptions`            | config object  | `sharedConfigBuild`, `sharedPlugins`, `buildApp` builder hook          |
|  [08]   | `ViteBuilder`               | builder object | orchestrates `build()` across every configured environment            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: config helpers
- rail: build

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [DESCRIPTION]                                                     |
| :-----: | :------------------------------------------- | :--------------- | :--------------------------------------------------------------- |
|  [01]   | `defineConfig(config)`                       | config helper    | 6 overloads: `UserConfig`, `Promise<UserConfig>`, `UserConfigFnObject`, `UserConfigFnPromise`, `UserConfigFn`, `UserConfigExport` — identity helper preserving the input's typed form |
|  [02]   | `mergeConfig(defaults, overrides, isRoot?)`  | utility function | deep-merges two configs; concatenates `plugins`, merges `alias`/`define` |
|  [03]   | `mergeAlias(a?, b?)`                          | utility function | merges alias option maps                                         |

[ENTRYPOINT_SCOPE]: programmatic build/serve
- rail: build

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [DESCRIPTION]                                                     |
| :-----: | :------------------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `build(inlineConfig?)`                             | async function | returns `RolldownOutput \| RolldownOutput[] \| RolldownWatcher` (array when multi-environment, watcher when `build.watch` set) |
|  [02]   | `createServer(inlineConfig?)`                      | async function | returns an initialized `ViteDevServer` (accepts `InlineConfig \| ResolvedConfig`) |
|  [03]   | `preview(inlineConfig?)`                           | async function | returns a `PreviewServer` for the built bundle                   |
|  [04]   | `createBuilder(inlineConfig?, useLegacyBuilder?)`  | async function | returns a `ViteBuilder` that drives `build()` per environment    |
|  [05]   | `resolveConfig(inline, command, defaultMode?, …)`  | async function | resolves an `InlineConfig` to `ResolvedConfig` without building; `patchConfig`/`patchPlugins` hooks |
|  [06]   | `loadConfigFromFile(env, file?, root?, …)`         | async function | loads a config file via `'bundle'\|'runner'\|'native'` loader; returns `{ path, config, dependencies }` |
|  [07]   | `optimizeDeps(config, force?, asCommand?)`         | async function | runs dep pre-bundling; returns `DepOptimizationMetadata`         |

[ENTRYPOINT_SCOPE]: plugin-author utilities
- rail: build

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [DESCRIPTION]                                                     |
| :-----: | :------------------------------------------- | :--------------- | :--------------------------------------------------------------- |
|  [01]   | `createFilter(include?, exclude?, options?)` | filter factory   | picomatch include/exclude resolver every plugin author composes  |
|  [02]   | `withFilter(plugin, filter)`                 | plugin wrapper   | wraps a plugin with a rolldown id/code filter (from `rolldown/filter`) |
|  [03]   | `perEnvironmentState(initial)`               | metadata factory | `WeakMap`-backed per-environment plugin metadata accessor        |
|  [04]   | `perEnvironmentPlugin(name, applyToEnvironment)` | plugin factory | scopes a plugin to matching environments                       |
|  [05]   | `sortUserPlugins(plugins)`                   | ordering utility | splits into `[pre, normal, post]` by `enforce`                   |
|  [06]   | `normalizePath(id)`                          | path utility     | normalizes module-id separators to POSIX                         |
|  [07]   | `loadEnv(mode, envDir, prefixes?)`           | env loader       | loads `.env*` files for `mode`, filtered by `prefixes`           |
|  [08]   | `searchForWorkspaceRoot(current, root?)`     | path utility     | walks up to the monorepo root                                    |
|  [09]   | `createLogger(level?, options?)`             | logger factory   | a `Logger` with `allowClearScreen`/`prefix`/`customLogger`       |

[ENTRYPOINT_SCOPE]: transform helpers and version constants
- rail: build

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [DESCRIPTION]                                                     |
| :-----: | :------------------------------------------- | :--------------- | :--------------------------------------------------------------- |
|  [01]   | `transformWithOxc(code, filename, options?, inMap?, config?, watcher?)` | transform | direct Oxc transform (JSX/TS strip) outside the plugin pipeline; `options?` is rolldown's `TransformOptions` (`rolldown/utils`, no vite-named export nor `Rolldown`-namespace surface) — `vite-plugin-svgr` derives its `oxcOptions` knob as `NonNullable<Parameters<typeof transformWithOxc>[2]>` |
|  [02]   | `transformWithEsbuild(code, filename, options?, inMap?, config?, watcher?, ignoreEsbuildWarning?)` | transform | legacy esbuild transform (the non-rolldown `load`-hook path); `options?` is `EsbuildTransformOptions`; `vite-plugin-svgr` spreads its `esbuildOptions` into it |
|  [03]   | `preprocessCSS(code, filename, config)`      | css transform    | runs the resolved CSS pipeline over a source string              |
|  [04]   | `parseAst(code, opts?)` / `parseAstAsync(…)` | ast parse        | Oxc AST parse surfaced from rolldown                             |
|  [05]   | `minify(code, opts?)` / `minifySync(…)`      | minifier         | direct Oxc minify entrypoints                                    |
|  [06]   | `version` / `rolldownVersion` / `rollupVersion` / `esbuildVersion` | constants | resolved tool versions (`version` = Vite, aliased from `VERSION`) |

```ts contract
// vite (dist/node/index.d.ts)
import * as Rolldown from 'rolldown'          // RolldownOutput, RolldownWatcher, Plugin surfaced here
import { withFilter } from 'rolldown/filter'  // re-exported

interface ConfigEnv { command: 'build' | 'serve'; mode: string; isSsrBuild?: boolean; isPreview?: boolean }
interface Plugin<A = any> extends Rolldown.Plugin<A> { enforce?: 'pre' | 'post'; apply?: 'build' | 'serve' | ((c: UserConfig, e: ConfigEnv) => boolean) }
type PluginOption = Thenable<Plugin | false | null | undefined | PluginOption[]>

declare function defineConfig(config: UserConfig): UserConfig
declare function defineConfig(config: Promise<UserConfig>): Promise<UserConfig>
declare function defineConfig(config: UserConfigFnObject): UserConfigFnObject
declare function defineConfig(config: UserConfigFnPromise): UserConfigFnPromise
declare function defineConfig(config: UserConfigFn): UserConfigFn
declare function defineConfig(config: UserConfigExport): UserConfigExport

declare function build(inlineConfig?: InlineConfig): Promise<RolldownOutput | RolldownOutput[] | RolldownWatcher>
declare function createServer(inlineConfig?: InlineConfig | ResolvedConfig): Promise<ViteDevServer>
declare function preview(inlineConfig?: InlineConfig): Promise<PreviewServer>
declare function createBuilder(inlineConfig?: InlineConfig, useLegacyBuilder?: null | boolean): Promise<ViteBuilder>
declare function resolveConfig(inline: InlineConfig, command: 'build' | 'serve', defaultMode?: string, defaultNodeEnv?: string, isPreview?: boolean): Promise<ResolvedConfig>

declare const createFilter: (include?: FilterPattern, exclude?: FilterPattern, options?: { resolve?: string | false | null }) => (id: string) => boolean
declare function mergeConfig<D extends Record<string, any>, O extends Record<string, any>>(defaults: D, overrides: O, isRoot?: boolean): Record<string, any>
declare function loadEnv(mode: string, envDir: string | false, prefixes?: string | string[]): Record<string, string>

// transpile helpers — the plugin-author transform tier a load-hook plugin (vite-plugin-svgr) dispatches through
declare function transformWithOxc(code: string, filename: string, options?: TransformOptions /* rolldown/utils; not on the Rolldown namespace */, inMap?: object, config?: ResolvedConfig, watcher?: FSWatcher): Promise<Omit<TransformResult, 'errors'>>
declare function transformWithEsbuild(code: string, filename: string, options?: EsbuildTransformOptions, inMap?: object, config?: ResolvedConfig, watcher?: FSWatcher, ignoreEsbuildWarning?: boolean): Promise<ESBuildTransformResult>
type EsbuildTransformOptions = esbuild.TransformOptions   // re-exported; svgr: type OxcTransformOptions = NonNullable<Parameters<typeof transformWithOxc>[2]>

interface LibraryOptions { entry: InputOption; name?: string; formats?: LibraryFormats[]; fileName?: string | ((f: ModuleFormat, e: string) => string); cssFileName?: string }
// build.target default 'baseline-widely-available'; build.minify default 'oxc'; oxc?: OxcOptions | false replaces deprecated esbuild
```

## [04]-[IMPLEMENTATION_LAW]

[BUILD_TOPOLOGY]:
- `build.target` defaults to `'baseline-widely-available'` and `build.minify` to `'oxc'`; `oxc?: OxcOptions | false` is the transform field, and the `esbuild` field is `@deprecated Use oxc option instead`.
- `rolldownOptions` governs bundle customization; `rollupOptions` is the deprecated alias, and `rollupVersion` remains exported for rollup-plugin compat while `rolldownVersion` is the real bundler.
- `build.sourcemap: 'hidden'` emits maps without inline comments; `build.manifest: true` emits the `Manifest` mapping source → emitted chunk that `vite-plugin-pwa` reads to build the precache list.
- `build.lib` unlocks library-mode output (`LibraryFormats` `es`/`cjs`/`umd`/`iife`); the platform SPA bundle is app-mode, not lib-mode.
- `server.middlewareMode` plugs Vite into an existing Node HTTP server without binding a port; `appType: 'spa'` selects the history-fallback the co-hosted SPA needs.

[ENVIRONMENT_API]:
- `config.environments` declares per-target `EnvironmentOptions`; `createBuilder()` returns a `ViteBuilder` that folds `build()` across every environment, returning `RolldownOutput[]`.
- `DevEnvironment`/`BuildEnvironment` are the per-environment pipelines; `RunnableDevEnvironment` + `runnerImport` load a module in-process for config/SSR evaluation.
- `perEnvironmentState(initial)` and `perEnvironmentPlugin(name, apply)` are the plugin-author primitives for environment-scoped metadata and activation — the modern replacement for global plugin state.
- The platform SPA is a single co-hosted client environment; the SSR/module-runner surface (`createServerModuleRunner`, `createServerModuleRunnerTransport`, `fetchModule`) is present-but-unused, never a second render path.

[PLUGIN_PIPELINE]:
- `UserConfig.plugins: PluginOption[]` is the single absorption point for every sibling build plugin — `@vitejs/plugin-react` (spread), `@tailwindcss/vite`, `vite-plugin-pwa` (spread), `vite-plugin-webfont-dl`, `vite-plugin-compression`, `vite-plugin-csp`, `vite-plugin-svgr`, `vite-plugin-image-optimizer`, `vite-plugin-inspect`, `rollup-plugin-visualizer` — ordered by `enforce: 'pre'|'post'` relative to core plugins.
- `createFilter(include, exclude)` and `withFilter(plugin, filter)` are the include/exclude helpers every plugin author composes; `FilterPattern` is the shared include/exclude vocabulary. A downstream plugin row uses `createFilter`, never a hand-rolled `micromatch` loop.
- `transformWithOxc`/`transformWithEsbuild` are the bundler-agnostic JSX/TS transpile helpers a `load`-hook plugin dispatches through: `vite-plugin-svgr` runs `@svgr/core` then transpiles the emitted JSX via `transformWithOxc(code, id, { lang: 'jsx', ...oxcOptions })` under rolldown-vite or `transformWithEsbuild(code, id, { loader: 'jsx', ...esbuildOptions })` otherwise. Its `oxcOptions` type is `NonNullable<Parameters<typeof transformWithOxc>[2]>` (rolldown's `TransformOptions`, no vite-named export) and its `esbuildOptions` type is the re-exported `EsbuildTransformOptions` — a plugin transpiles through these two, never a direct rolldown/esbuild call.
- `transformIndexHtml` (`IndexHtmlTransform` + `HtmlTagDescriptor`) is the shared HTML-injection hook `vite-plugin-webfont-dl`, `vite-plugin-csp`, and `vite-plugin-pwa` each write into; ordering is the `order: 'pre'|'post'` field on the hook object.

[LOCAL_ADMISSION]:
- `Shell/build`'s `BuildPipeline` authors one `defineConfig(({ command, mode }) => …)` folding the plugin set, `build.target` fed by `browserslist`, and the `vite-plugin-pwa` PWA/precache emit; `UserConfig`/`InlineConfig`/`Plugin`/`PluginOption`/`BuildEnvironmentOptions` are the primary planning surfaces.
- `build.target` reads the `browserslist` matrix (the `.api/browserslist.md` target rail) so the Oxc transform and CSS lowering target the same runtime floor as the rest of the pipeline.
- The bundle `Manifest` is the input `vite-plugin-pwa` folds into the Workbox precache manifest; `Shell/build` emits it, `Shell/serviceworker`'s `ServiceWorkerHost` consumes the emitted worker asset (never re-emitting).
- `defineConfig`'s function overload receives `ConfigEnv` so the dev atom-inspector plugin (`vite-plugin-inspect`) is stripped from the production bundle by `command`/`mode`.

[RAIL_LAW]:
- Package: `vite`
- Owns: build orchestration, dev server, the plugin host, HMR, the Environment API, and config resolution
- Accept: `UserConfig`/`InlineConfig` via `defineConfig`; the programmatic `build`/`createServer`/`preview`/`createBuilder`; plugin rows in `plugins`
- Reject: direct rolldown/rollup API calls for build orchestration when Vite owns the pipeline; a hand-rolled include/exclude matcher where `createFilter` exists; a direct rolldown/esbuild transpile call in a `load` hook where `transformWithOxc`/`transformWithEsbuild` own the bundler-agnostic transpile; a parallel build pipeline where a new `Plugin` row suffices
