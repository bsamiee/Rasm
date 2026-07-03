# [API_CATALOGUE] browserslist

`browserslist` resolves browser-selection queries (e.g. `> 0.5%, last 2 versions, not dead`) to a flat list of `"browser version"` strings in Can I Use format, and exposes the underlying Can I Use dataset, node-version table, and market-usage statistics the resolution reads. It is consumed indirectly by the build toolchain — Vite `target`, Lightning CSS, PostCSS Autoprefixer — which derive the supported browser set from `.browserslistrc`, `package.json#browserslist`, or the `BROWSERSLIST` environment override; the `Shell/build.md` transform set reads it as the target-runtime matrix. Direct application use is confined to coverage auditing (`coverage` + `usage`/`data`) and programmatic config loading outside the pipeline.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `browserslist`
- package: `browserslist`
- version: `4.28.4`
- license: `MIT`
- module: CJS `export = browserslist` (`index.d.ts`) — a callable function carrying a static namespace; the `browserslist/error` subpath exports `BrowserslistError` (no `exports` map, so subpaths import freely). Under esModuleInterop `import browserslist from "browserslist"`
- asset: CJS runtime library + a `browserslist` bin CLI; `engines: node ^6…^12 || >=13.7`; build-tool integration is the primary consumer
- rail: browser-target — query resolution, Can I Use data lookup, config discovery

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: configuration, query, and stats types
- rail: browser-target

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]     | [BOUNDARY_NOTE]                                                              |
| :-----: | :------------------ | :---------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Options`           | options interface | `path\|false`, `env`, `stats`, `config`, `ignoreUnknownVersions`, `throwOnMissing`, `dangerousExtend`, `mobileToDesktop` |
|  [02]   | `Query`             | query AST node    | `compose: 'or'\|'and'`, `type: string`, `query: string`, `not?: true`       |
|  [03]   | `Config`            | config map        | `{ defaults: string[]; [section]: string[] \| undefined }`                  |
|  [04]   | `LoadConfigOptions` | load options      | `config`, `path`, `env` for `loadConfig`                                    |
|  [05]   | `Stats`             | usage-stats map   | `{ [browser]: { [version]: number } }` — custom `> 1% in my stats` source   |
|  [06]   | `StatsOptions`      | stats union       | `string \| 'my stats' \| Stats \| { dataByBrowser: Stats }`                  |
|  [07]   | `Usage`             | usage row         | `{ [version]: number }` — a `usage` country/global slice                    |

[PUBLIC_TYPE_SCOPE]: error rail — `browserslist/error`
- rail: browser-target

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [BOUNDARY_NOTE]                                                    |
| :-----: | :------------------ | :------------ | :----------------------------------------------------------------- |
|  [01]   | `BrowserslistError` | error class   | `extends Error`; `name: 'BrowserslistError'`, `browserslist: true` brand — thrown on bad query, missing env (`throwOnMissing`), or unknown direct version (unless `ignoreUnknownVersions`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: default resolver
- rail: browser-target

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [BOUNDARY_NOTE]                                       |
| :-----: | :------------------------------ | :------------- | :--------------------------------------------------- |
|  [01]   | `browserslist(queries?, opts?)` | resolver       | `string[]` of `"browser version"`; `queries` = `string \| readonly string[] \| null` |

[ENTRYPOINT_SCOPE]: static namespace functions
- rail: browser-target

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [BOUNDARY_NOTE]                            |
| :-----: | :---------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `browserslist.coverage(browsers, stats?)` | coverage query | aggregate market-share percentage; `stats` is a `StatsOptions` |
|  [02]   | `browserslist.parse(queries?, opts?)`     | query parser   | `Query[]` AST without execution            |
|  [03]   | `browserslist.loadConfig(options)`        | config loader  | `string[] | undefined` raw queries from the nearest config |
|  [04]   | `browserslist.parseConfig(string)`        | config parser  | `Config` from raw config content           |
|  [05]   | `browserslist.readConfig(file)`           | config reader  | `Config` from a file path                  |
|  [06]   | `browserslist.findConfig(...segments)`    | config finder  | `Config | undefined` from path segments    |
|  [07]   | `browserslist.findConfigFile(...segs)`    | config finder  | config-file path `string | undefined`      |
|  [08]   | `browserslist.clearCaches()`              | cache reset    | clear the internal Can I Use + config caches |

[ENTRYPOINT_SCOPE]: static namespace data — the Can I Use dataset the resolution reads
- rail: browser-target
- Mutable `let`-bound namespace members, not functions: the coverage/audit surface reads them directly (a custom PostCSS plugin or a coverage report queries `data`/`usage`/`nodeVersions` rather than re-parsing the caniuse-lite package).

| [INDEX] | [SYMBOL]                       | [ENTRY_FAMILY] | [BOUNDARY_NOTE]                                       |
| :-----: | :----------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `browserslist.data`            | dataset        | `{ [browser]: { name, versions[], released[], releaseDate } }` Can I Use table |
|  [02]   | `browserslist.usage`           | usage stats    | `{ global?, custom?, [country]: Usage }` market share |
|  [03]   | `browserslist.nodeVersions`    | version list   | `string[]` — the `node`/`current node` query source  |
|  [04]   | `browserslist.defaults`        | query          | `readonly string[]` — the fallback query when none is given |
|  [05]   | `browserslist.cache`           | feature cache  | per-feature version cache; `clearCaches` resets it   |
|  [06]   | `browserslist.aliases` / `versionAliases` / `desktopNames` | name maps | browser-name and joined-version normalization tables |

[ENTRYPOINT_SCOPE]: environment overrides (`NodeJS.ProcessEnv` augmentation)
- rail: browser-target

| [INDEX] | [SYMBOL]                        | [ENTRY_FAMILY] | [BOUNDARY_NOTE]                          |
| :-----: | :------------------------------ | :------------- | :--------------------------------------- |
|  [01]   | `BROWSERSLIST`                  | env override   | overrides the query directly             |
|  [02]   | `BROWSERSLIST_CONFIG`           | env override   | path to an explicit config file          |
|  [03]   | `BROWSERSLIST_ENV`              | env override   | selects a named config environment section |
|  [04]   | `BROWSERSLIST_ROOT_PATH`        | env override   | pins the config search root in monorepos |
|  [05]   | `BROWSERSLIST_STATS`            | env override   | path/object for custom usage stats       |
|  [06]   | `BROWSERSLIST_DISABLE_CACHE`    | env flag       | disables Can I Use data caching          |
|  [07]   | `BROWSERSLIST_IGNORE_OLD_DATA`  | env flag       | suppresses stale-data warnings           |
|  [08]   | `BROWSERSLIST_DANGEROUS_EXTEND` | env flag       | allows third-party config `extends`      |

## [04]-[IMPLEMENTATION_LAW]

[QUERY_TOPOLOGY]:
- `null` or omitted `queries` falls back to the project config or the `defaults` query.
- `opts.env` selects a named config section; omitting it uses `defaults`. `opts.throwOnMissing` promotes a missing env to a `BrowserslistError` rather than a silent `defaults` fallback.
- `opts.path` (or `false`) sets the config-discovery start directory (default `process.cwd()`); `BROWSERSLIST_ROOT_PATH` is the monorepo-safe alternative to per-package `opts.path`.
- `opts.stats` accepts a `Stats` object or a path to a JSON file for `> 1% in my stats`; `opts.config` pins an explicit config file.
- `opts.mobileToDesktop` substitutes desktop data where Can I Use lacks mobile-browser versions; `opts.ignoreUnknownVersions` downgrades an unknown direct version from a `BrowserslistError` throw to a skip; `opts.dangerousExtend` disables the third-party `extends` security check.

[INTEGRATION_LAW]:
- Build-time only, not an `effect` runtime rail: `browserslist` runs inside the Vite/Lightning CSS/PostCSS transform pipeline (`Shell/build.md`), never inside the `platform` `Layer` graph — it never crosses `Effect.tryPromise` and holds no service tag.
- Target-matrix rail into `vite.md`, never a `UserConfig.plugins` row: `browserslist` feeds `vite.md`'s `build.target` (`BuildEnvironmentOptions.target`, default `'baseline-widely-available'`) — the reciprocal of `vite.md`'s "`build.target` reads the `browserslist` matrix" admission — so the Oxc transform and CSS lowering target the same runtime floor as the plugin host, while `browserslist` itself never enters the plugin array. The workspace `target: 'baseline-widely-available'` delegates resolution to Vite's internal browserslist integration, so no folder code calls `browserslist()` on the runtime path.
- Direct use is the escape hatch: authoring a custom PostCSS plugin (`browserslist(queries, opts)` → the target set), auditing coverage (`coverage(browserslist('> 1% in US'), 'US')` reading `usage`/`data`), or programmatic config loading outside the build (`loadConfig`/`readConfig`/`findConfig`). A `BrowserslistError` is caught at that call boundary, never propagated into the app fault family.
- `clearCaches()` invalidates the Can I Use + config caches when a long-lived process rewrites `.browserslistrc` between builds.

[RAIL_LAW]:
- Package: `browserslist`
- Owns: browser-query resolution, Can I Use dataset/usage/node-version lookup, config-file discovery and parsing, the `browserslist` bin CLI
- Accept: Can I Use query strings; `Options` for path/env/stats/config overrides; the env overrides for CI and monorepo pinning; the `data`/`usage`/`nodeVersions` namespace for coverage auditing
- Reject: per-call version pinning; hand-rolled Can I Use data parsing; calling `browserslist()` on the `platform` runtime path (it is a build-time tool); wrapping `BrowserslistError` into the app fault family
