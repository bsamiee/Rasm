# [API_CATALOGUE] browserslist

`browserslist` resolves browser queries (e.g. `> 0.5%, last 2 versions, not dead`) to a flat list of `"browser version"` strings in Can I Use format, consumed by build tooling (Vite `target`, Lightning CSS, PostCSS Autoprefixer) to derive the supported browser set from `.browserslistrc`, `package.json#browserslist`, or the `BROWSERSLIST` environment variable.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `browserslist`
- package: `browserslist`
- module: `browserslist` (CJS `export =` default function)
- asset: runtime library + build-tool integration
- rail: browser-target

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: configuration and query types
- rail: browser-target

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]     | [RAIL]                                     |
| :-----: | :------------------ | :---------------- | :----------------------------------------- |
|   [1]   | `Options`           | options interface | query execution options (path, env, stats) |
|   [2]   | `Query`             | query AST node    | `compose`, `type`, `query`, `not` fields   |
|   [3]   | `Config`            | config map        | `defaults` + named environment sections    |
|   [4]   | `LoadConfigOptions` | load options      | `config`, `path`, `env` for `loadConfig`   |
|   [5]   | `StatsOptions`      | stats union       | string path, `'my stats'`, Stats object    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: default function
- rail: browser-target

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :------------------------------ | :------------- | :-------------------------------------- |
|   [1]   | `browserslist(queries?, opts?)` | resolver       | `string[]` of `"browser version"` pairs |

[ENTRYPOINT_SCOPE]: static namespace utilities
- rail: browser-target

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :---------------------------------------- | :------------- | :----------------------------------------- |
|   [1]   | `browserslist.coverage(browsers, stats?)` | coverage query | aggregate market share percentage          |
|   [2]   | `browserslist.parse(queries?, opts?)`     | query parser   | `Query[]` AST without execution            |
|   [3]   | `browserslist.loadConfig(options)`        | config loader  | raw query strings from nearest config file |
|   [4]   | `browserslist.clearCaches()`              | cache reset    | clear internal can-i-use and config caches |
|   [5]   | `browserslist.parseConfig(string)`        | config parser  | `Config` from raw config file content      |
|   [6]   | `browserslist.readConfig(file)`           | config reader  | `Config` from file path                    |
|   [7]   | `browserslist.findConfig(...segments)`    | config finder  | `Config \| undefined` from path segments   |
|   [8]   | `browserslist.findConfigFile(...segs)`    | config finder  | config file path string or `undefined`     |

[ENTRYPOINT_SCOPE]: environment variables (NodeJS.ProcessEnv augmentation)
- rail: browser-target

| [INDEX] | [SYMBOL]                        | [ENTRY_FAMILY] | [RAIL]                                   |
| :-----: | :------------------------------ | :------------- | :--------------------------------------- |
|   [1]   | `BROWSERSLIST`                  | env override   | overrides the query directly             |
|   [2]   | `BROWSERSLIST_CONFIG`           | env override   | path to explicit config file             |
|   [3]   | `BROWSERSLIST_ENV`              | env override   | selects named config environment section |
|   [4]   | `BROWSERSLIST_ROOT_PATH`        | env override   | pins config search root in monorepos     |
|   [5]   | `BROWSERSLIST_DISABLE_CACHE`    | env flag       | disables can-i-use data caching          |
|   [6]   | `BROWSERSLIST_IGNORE_OLD_DATA`  | env flag       | suppresses stale can-i-use data warnings |
|   [7]   | `BROWSERSLIST_DANGEROUS_EXTEND` | env flag       | allows third-party config extends        |
|   [8]   | `BROWSERSLIST_STATS`            | env override   | path or object for custom usage stats    |

## [4]-[IMPLEMENTATION_LAW]

[QUERY_TOPOLOGY]:
- `null` or omitted `queries` falls back to the project config or the `defaults` query
- `opts.env` selects a named section from the config file; omitting it uses `defaults`
- `opts.path` sets the starting directory for config file discovery; defaults to `process.cwd()`
- `opts.stats` accepts a Stats object or a path to a JSON file for `> 1% in my stats` queries
- `opts.mobileToDesktop` substitutes desktop data when Can I Use lacks mobile-browser version data
- `BROWSERSLIST_ROOT_PATH` is the monorepo-safe alternative to per-package `opts.path`

[LOCAL_ADMISSION]:
- Consumed indirectly by Vite, Lightning CSS, and PostCSS; rarely called directly in application code.
- The workspace `target: 'baseline-widely-available'` delegates target resolution to Vite's internal browserslist integration.
- Direct use is needed when authoring custom PostCSS plugins, auditing coverage, or programmatic config loading outside the build pipeline.

[RAIL_LAW]:
- Package: `browserslist`
- Owns: browser query resolution, Can I Use data lookup, config file discovery
- Accept: query strings in Can I Use format; `Options` for path/env/stats overrides
- Reject: per-call version pinning; hand-rolled Can I Use data parsing
