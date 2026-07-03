# [API_CATALOGUE] rollup-plugin-visualizer

`rollup-plugin-visualizer` emits a bundle-composition report after a Rollup/Rolldown/Vite build. `visualizer(opts?)` returns a `Plugin` that hooks `generateBundle` and writes one of eight `TemplateType` reports (`treemap` default, plus `sunburst`/`treemap-3d`/`network`/`flamegraph`/`list`/`markdown`/`raw-data`), with optional gzip/brotli size columns and `{ bundle, file }` picomatch filtering. It ships a standalone `visualizer` CLI (`dist/bin/cli.js`) that renders a `raw-data` JSON stats file into any template offline — the seam a CI bundle-budget gate reads without the HTML. In `platform` it is the build-size half of the `Shell/build.md` diagnostics pair (the transform-chain half is `vite-plugin-inspect`), env-flag-gated, never in the runtime plugin array.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rollup-plugin-visualizer`
- package: `rollup-plugin-visualizer`
- version: `7.0.1` (central pin `pnpm-workspace.yaml`)
- license: `MIT`
- runtime: build-time only — ESM-only (`type: module`); a `generateBundle`-phase plugin plus a `visualizer` bin CLI (`dist/bin/cli.js`)
- module: `rollup-plugin-visualizer` (default + named `visualizer`; types `dist/plugin/index.d.ts`, single `.` export)
- peer: `rollup 2.x || 3.x || 4.x` (OPTIONAL) and `rolldown 1.x || ^1.0.0-beta || ^1.0.0-rc` (OPTIONAL) — host-agnostic, binds whichever bundler Vite runs; no hard peer
- side-effects: `false` (tree-shakeable) — irrelevant, never in the runtime bundle
- asset: build plugin factory (`Plugin`) + CLI
- rail: build — post-build bundle-composition diagnostics
- catalog-verdict: KEEP; build-time-only, env-flag-gated at `Shell/build.md`; never in the production plugin array or shipped bundle — no runtime collapse target

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: options + template family
- rail: build

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                                              |
| :-----: | :------------------------ | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `PluginVisualizerOptions` | interface     | the `visualizer` factory options bag                                              |
|  [02]   | `TemplateType`            | string union  | `"sunburst"\|"treemap"\|"treemap-3d"\|"network"\|"raw-data"\|"list"\|"markdown"\|"flamegraph"` — the report-format axis |
|  [03]   | `Filter`                  | type alias    | `{ bundle?: string\|null; file?: string\|null }` — a two-field picomatch matcher, NOT a bare id pattern |

[PUBLIC_TYPE_SCOPE]: `PluginVisualizerOptions` fields
- rail: build
- `open`/`openOptions` use the `open` package (`OpenOptions`); `include`/`exclude` are `Filter` objects fed to `createFilter(include, exclude) => (bundleId, id) => boolean`, matching by BOTH output-chunk (`bundle`) and module (`file`), not a single module-id glob.

| [INDEX] | [FIELD]       | [TYPE]                                  | [DEFAULT]             |
| :-----: | :------------ | :-------------------------------------- | :-------------------- |
|  [01]   | `filename`    | `string`                                | `'stats.html'`        |
|  [02]   | `title`       | `string`                                | `'Rollup Visualizer'` |
|  [03]   | `template`    | `TemplateType`                          | `'treemap'`           |
|  [04]   | `json`        | `boolean` (`@deprecated` → `'raw-data'`)| `false`               |
|  [05]   | `open`        | `boolean`                               | `false`               |
|  [06]   | `openOptions` | `OpenOptions` (`open` pkg)              | `—`                   |
|  [07]   | `gzipSize`    | `boolean`                               | `false`               |
|  [08]   | `brotliSize`  | `boolean`                               | `false`               |
|  [09]   | `sourcemap`   | `boolean`                               | `false`               |
|  [10]   | `projectRoot` | `string \| RegExp`                      | `process.cwd()`       |
|  [11]   | `emitFile`    | `boolean`                               | `false`               |
|  [12]   | `include`     | `Filter \| Filter[]`                    | `—`                   |
|  [13]   | `exclude`     | `Filter \| Filter[]`                    | `—`                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory
- rail: build
- The functional overload lets options depend on the resolved `OutputOptions` — one report per output in a multi-environment (client/SSR) build.

```ts
export declare const visualizer: (
  opts?: PluginVisualizerOptions | ((outputOptions: OutputOptions) => PluginVisualizerOptions),
) => Plugin
export default visualizer
```

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                    |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `visualizer(opts?)`                             | plugin factory | `generateBundle`-phase report emit; place in `plugins`  |
|  [02]   | `visualizer((out) => opts)`                     | plugin factory | per-`OutputOptions` config for multi-output builds      |

[ENTRYPOINT_SCOPE]: standalone `visualizer` CLI (`dist/bin/cli.js`)
- rail: build
- Renders a `raw-data`/`json` stats file into any template WITHOUT re-running the build — the offline lane a CI budget gate or a decoupled report step uses.

| [INDEX] | [FLAG]        | [MAPS TO]                          |
| :-----: | :------------ | :--------------------------------- |
|  [01]   | `filename`    | output report path                 |
|  [02]   | `title`       | report `<title>`                   |
|  [03]   | `template`    | `TemplateType` selection           |
|  [04]   | `sourcemap`   | sourcemap-based sizing             |
|  [05]   | `open`        | open the rendered report           |

## [04]-[IMPLEMENTATION_LAW]

[BUILD_TOPOLOGY]:
- hooks `generateBundle` only; it does NOT affect tree-shaking, code-splitting, or runtime behavior — pure post-build analysis.
- report SINK is a policy, not parallel plugins: `emitFile: true` routes through Rollup's `emitFile` so the report lands in `outDir` beside artifacts (then `filename` must be a bare name, not a path); `emitFile: false` writes `filename` directly; `template: 'raw-data'` (superseding the deprecated `json: true`) emits machine-readable JSON the CLI consumes; `open`/`openOptions` launch the rendered report.
- `gzipSize`/`brotliSize` add compressed-size columns independent of `sourcemap`; `sourcemap: true` sizes modules from source maps (more accurate, and `gzip`/`brotli` sizing then adds little). `projectRoot` strips a shared prefix from module paths — set it to the monorepo root for stable ids across packages.
- `include`/`exclude` are `{ bundle, file }` picomatch objects filtered via `createFilter(include, exclude)(bundleId, id)` — a module is kept when its `(bundleId, id)` matches an include and no exclude, filtering by output chunk AND module, not a single id glob.

[INTEGRATION_LAW]:
- Stack with `vite.md`: one `PluginOption` row in `vite.md`'s `UserConfig.plugins` host, added ONLY under a build-analysis env flag (a diagnostics build), never the default production build; it rides whichever bundler Vite drives (Rollup or Rolldown, both optional peers). Its `generateBundle` hook only reads the finalized bundle, so it carries no `enforce` ordering contention with `vite-plugin-csp`'s `transformIndexHtml`/`enforce:"post"` or `vite-plugin-compression`'s `closeBundle` — it observes, never mutates. `include`/`exclude` compose the bundled `createFilter` over `{ bundle, file }` pairs — the two-field matcher, distinct from `vite.md`'s single-id `createFilter`/`FilterPattern`.
- Stack with `vite-plugin-inspect` (`libs/typescript/_tmp/platform/.api/vite-plugin-inspect.md`) — the diagnostics PAIR: `vite-plugin-inspect` introspects the per-module TRANSFORM CHAIN at dev (`/__inspect/`), this profiles the final BUNDLE COMPOSITION at build. Two orthogonal diagnostics axes on the same `Shell/build.md` gate, never overlapping — inspect answers "why is this module transformed this way", visualizer answers "why is the bundle this size".
- Stack with the multi-environment build: the `(outputOptions) => opts` overload emits a distinct report per output (client vs SSR), each keyed off its `OutputOptions` — one report per environment, never a merged blur.
- Stack with a CI budget gate: emit `template: 'raw-data'` to a JSON stats file, then render/threshold it OFFLINE via the `visualizer` CLI — the gate reads the raw-data JSON without the HTML, so the size check is decoupled from the interactive report.

[LOCAL_ADMISSION]:
- gate on a build-analysis env flag or run as a standalone analysis step; never include in the production plugin array or a shipped bundle.
- prefer `template: 'raw-data'` over the deprecated `json: true` for machine-readable output; the interactive `treemap`/`network`/`flamegraph` templates are for human triage.
- set `projectRoot` to the monorepo root so module ids are stable across packages in the report.

[RAIL_LAW]:
- Package: `rollup-plugin-visualizer`
- Owns: post-build bundle-composition visualization (eight templates), compressed-size columns, `{ bundle, file }` module filtering, and the offline `visualizer` CLI over `raw-data` JSON
- Accept: `visualizer()` in the `Shell/build.md` plugins array gated on an env flag; the `(outputOptions) => opts` overload for per-environment reports; `template: 'raw-data'` + the CLI for a decoupled CI budget gate
- Reject: inclusion in the production plugin array or runtime bundle; the deprecated `json: true` (use `template: 'raw-data'`); treating `include`/`exclude` as bare id globs (they are `{ bundle, file }` objects); omitting the `"list"` template from the format axis; a per-report `projectRoot` fork where the monorepo root gives stable ids
