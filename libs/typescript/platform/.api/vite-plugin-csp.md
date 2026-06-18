# [API_CATALOGUE] vite-plugin-csp

`vite-plugin-csp` generates and injects `Content-Security-Policy` headers into HTML output and optionally produces server configuration files (Caddy, Nginx) at build time. It computes SHA hashes for inline scripts and styles, resolves `DirectivesObj`/`CspDirectives` from `csp-typed-directives`, and exposes `createViteCspPlugin` (also exported as `ViteCspPlugin`) as the single factory.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-csp`
- package: `vite-plugin-csp`
- module: `vite-plugin-csp`
- asset: Vite plugin (returns `Exclude<PluginOption, PluginOption[]>`)
- rail: security

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin options family
- rail: security

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]  | [DESCRIPTION]                                            |
| :-----: | :--------------------- | :------------- | :------------------------------------------------------- |
|   [1]   | `ViteCspPluginOptions` | options object | `typeof DEFAULT_OPTIONS` — full config                   |
|   [2]   | `ViteCspPluginOpts`    | type alias     | `Partial<ViteCspPluginOptions>`                          |
|   [3]   | `PolicyOptions`        | type alias     | `DirectivesParams \| DirectivesObj`                      |
|   [4]   | `ProcessOptions`       | type alias     | `ProcessOption \| ProcessOption[]`                       |
|   [5]   | `ProcessOption`        | type alias     | built-in name, `InternalProcessFnParams`, or `ProcessFn` |

[PUBLIC_TYPE_SCOPE]: processor types
- rail: security

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [DESCRIPTION]                                                         |
| :-----: | :------------------------ | :------------- | :-------------------------------------------------------------------- |
|   [1]   | `InternalProcessFnNames`  | string union   | keys of `builtinProcessorFns`                                         |
|   [2]   | `InternalProcessFnParams` | params object  | `{ processor, outFile? }`                                             |
|   [3]   | `ProcessFnContext`        | context object | `{ path, htmlFileName, srvConfDir, builtinProcessorFns }`             |
|   [4]   | `ProcessFnReturn`         | return type    | `{ name, content } \| void`                                           |
|   [5]   | `ProcessFn`               | function type  | `(ctx, parsedHeaders) => ProcessFnReturn \| Promise<ProcessFnReturn>` |
|   [6]   | `DirectivesParams`        | type alias     | re-exported from `csp-typed-directives`                               |

[PUBLIC_TYPE_SCOPE]: built-in processor names
- rail: security

| [INDEX] | [PROCESSOR_NAME]        | [OUTPUT]                         |
| :-----: | :---------------------- | :------------------------------- |
|   [1]   | `CaddyJSON`             | Caddy JSON config with CSP block |
|   [2]   | `CaddyJSON_HeadersOnly` | Caddy JSON headers-only block    |
|   [3]   | `Nginx`                 | Nginx config with CSP header     |
|   [4]   | `Nginx_HeadersOnly`     | Nginx headers-only snippet       |
|   [5]   | `Caddyfile`             | Caddyfile format                 |
|   [6]   | `Caddyfile_HeadersOnly` | Caddyfile headers-only format    |

[PUBLIC_TYPE_SCOPE]: ViteCspPluginOptions key fields
- rail: security

| [INDEX] | [FIELD]                  | [TYPE]                                          | [DEFAULT]              |
| :-----: | :----------------------- | :---------------------------------------------- | :--------------------- |
|   [1]   | `enabled`                | `boolean`                                       | `true`                 |
|   [2]   | `inject`                 | `boolean`                                       | `true`                 |
|   [3]   | `injectReporting`        | `boolean`                                       | `true`                 |
|   [4]   | `onDev`                  | `'permissive' \| 'full' \| 'skip'`              | `'permissive'`         |
|   [5]   | `policy`                 | `PolicyOptions`                                 | —                      |
|   [6]   | `hashingMethod`          | `'sha256' \| 'sha384' \| 'sha512'`              | `'sha256'`             |
|   [7]   | `hashEnabled`            | `Record<'script-src'\|'style-src'\|…, boolean>` | per-directive booleans |
|   [8]   | `processFn`              | `ProcessOptions \| undefined`                   | —                      |
|   [9]   | `referrerHeaderOverride` | referrer policy string or `undefined`           | —                      |
|  [10]   | `srvConfDir`             | `string`                                        | —                      |
|  [11]   | `debugPlugin`            | `boolean`                                       | `false`                |
|  [12]   | `mapHtmlFiles`           | `Record<string, PolicyOptions> \| undefined`    | —                      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory overloads
- rail: security

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [DESCRIPTION]                       |
| :-----: | :------------------------------------- | :------------- | :---------------------------------- |
|   [1]   | `createViteCspPlugin(policy, options)` | plugin factory | policy + options overload           |
|   [2]   | `createViteCspPlugin(options)`         | plugin factory | options-only overload               |
|   [3]   | `createViteCspPlugin()`                | plugin factory | zero-arg overload (uses defaults)   |
|   [4]   | `ViteCspPlugin`                        | alias          | `typeof createViteCspPlugin`        |
|   [5]   | `builtinProcessorFns`                  | constant       | map of built-in `InternalProcessFn` |
|   [6]   | `DEFAULT_OPTIONS`                      | constant       | default options snapshot            |

## [4]-[IMPLEMENTATION_LAW]

[CSP_TOPOLOGY]:
- hashing runs over inline `<script>` and `<style>` content extracted during `transformIndexHtml`; hashes are appended to the `script-src` / `style-src` directive
- `processFn` receives `ProcessFnContext` and the resolved `CspDirectiveHeaders`; use built-in processor names for standard server config output
- `mapHtmlFiles` allows per-HTML-file policy overrides keyed by file path
- `onDev: 'permissive'` loosens the policy in dev (adds `'unsafe-inline'`, `'unsafe-eval'`); `'skip'` disables the plugin entirely in dev

[LOCAL_ADMISSION]:
- Pass `policy` as first argument to `createViteCspPlugin` for the primary directive set; combine with `mapHtmlFiles` for route-specific policies.
- Set `hashingMethod: 'sha384'` or `'sha512'` when the deployment target's CSP Level 3 support requires stronger hashes.
- `processFn` with a built-in name string (e.g., `'Nginx'`) writes server config to `srvConfDir`; a custom `ProcessFn` receives full context for non-standard formats.

[RAIL_LAW]:
- Package: `vite-plugin-csp`
- Owns: CSP header generation, hash computation, HTML injection, and server config emission
- Accept: `PolicyOptions` and `Partial<ViteCspPluginOptions>` via `createViteCspPlugin()` overloads
- Reject: hand-rolled CSP hash computation or manual `<meta http-equiv>` injection when this plugin is active
