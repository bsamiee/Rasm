# [API_CATALOGUE] rollup-plugin-visualizer

`rollup-plugin-visualizer` emits an interactive HTML bundle-size report after a Rollup/Vite build, with configurable visualization templates, optional compressed-size columns, and picomatch-based module filtering.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rollup-plugin-visualizer`
- package: `rollup-plugin-visualizer`
- module: `rollup-plugin-visualizer`
- asset: build plugin
- rail: build

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin options
- rail: build

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                                                                   |
| :-----: | :------------------------ | :------------ | :----------------------------------------------------------------------- |
|   [1]   | `PluginVisualizerOptions` | interface     | full options bag for the `visualizer` factory                            |
|   [2]   | `TemplateType`            | string union  | `sunburst\|treemap\|treemap-3d\|network\|flamegraph\|markdown\|raw-data` |

[PUBLIC_TYPE_SCOPE]: PluginVisualizerOptions fields
- rail: build

| [INDEX] | [FIELD]       | [TYPE]               | [DEFAULT]             |
| :-----: | :------------ | :------------------- | :-------------------- |
|   [1]   | `filename`    | `string`             | `'stats.html'`        |
|   [2]   | `title`       | `string`             | `'Rollup Visualizer'` |
|   [3]   | `template`    | `TemplateType`       | `'treemap'`           |
|   [4]   | `open`        | `boolean`            | `false`               |
|   [5]   | `openOptions` | `OpenOptions`        |                       |
|   [6]   | `gzipSize`    | `boolean`            | `false`               |
|   [7]   | `brotliSize`  | `boolean`            | `false`               |
|   [8]   | `sourcemap`   | `boolean`            | `false`               |
|   [9]   | `projectRoot` | `string \| RegExp`   | `process.cwd()`       |
|  [10]   | `emitFile`    | `boolean`            | `false`               |
|  [11]   | `include`     | `Filter \| Filter[]` |                       |
|  [12]   | `exclude`     | `Filter \| Filter[]` |                       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory
- rail: build

```ts
export declare const visualizer: (
  opts?: PluginVisualizerOptions | ((outputOptions: OutputOptions) => PluginVisualizerOptions)
) => Plugin

export default visualizer
```

The functional overload `(outputOptions: OutputOptions) => PluginVisualizerOptions` lets options depend on the active output config — useful in multi-output builds producing separate per-environment artifacts.

## [4]-[IMPLEMENTATION_LAW]

[BUILD_TOPOLOGY]:
- The plugin hooks into Rollup's `generateBundle` phase and does not affect tree-shaking, code splitting, or runtime behavior
- `emitFile: true` routes the report through Rollup's `emitFile` API so it lands in `outDir` alongside build artifacts; `emitFile: false` writes directly to `filename`
- `gzipSize` and `brotliSize` add compressed-size columns to the report independently of `sourcemap`
- `include`/`exclude` are picomatch patterns scoped to module IDs, not file paths

[LOCAL_ADMISSION]:
- Add conditionally on a build-analysis env flag or as a standalone analysis script; do not include in production plugin arrays
- `template: 'raw-data'` replaces the deprecated `json: true` option for machine-readable output
- `projectRoot` strips the prefix from module paths in the report; set to the monorepo root for consistent IDs across packages

[RAIL_LAW]:
- Package: `rollup-plugin-visualizer`
- Owns: post-build bundle-size visualization and report emission
- Accept: `visualizer()` in Vite `plugins` gated on an env flag; `template` to select report format
- Reject: inclusion in production plugin arrays; `json: true` (deprecated — use `template: 'raw-data'`)
