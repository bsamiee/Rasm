# [API_CATALOGUE] vite-plugin-csp

`vite-plugin-csp` generates and injects `Content-Security-Policy` headers into HTML output and optionally produces server configuration files (Caddy, Nginx) at build time. It computes SHA hashes for inline scripts and styles, resolves `DirectivesObj`/`CspDirectives` from `csp-typed-directives`, and exposes `createViteCspPlugin` (also exported as `ViteCspPlugin`) as the single factory.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-csp`
- package: `vite-plugin-csp`
- module: `vite-plugin-csp`
- asset: Vite plugin (returns `Exclude<PluginOption, PluginOption[]>`)
- rail: security

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin options family
- rail: security

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]  | [DESCRIPTION]                                            |
| :-----: | :--------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `ViteCspPluginOptions` | options object | `typeof DEFAULT_OPTIONS` — full config                   |
|  [02]   | `ViteCspPluginOpts`    | type alias     | `Partial<ViteCspPluginOptions>`                          |
|  [03]   | `PolicyOptions`        | type alias     | `DirectivesParams \| DirectivesObj`                      |
|  [04]   | `ProcessOptions`       | type alias     | `ProcessOption \| ProcessOption[]`                       |
|  [05]   | `ProcessOption`        | type alias     | built-in name, `InternalProcessFnParams`, or `ProcessFn` |

[PUBLIC_TYPE_SCOPE]: processor types
- rail: security

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [DESCRIPTION]                                                         |
| :-----: | :------------------------ | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `InternalProcessFnNames`  | string union   | keys of `builtinProcessorFns`                                         |
|  [02]   | `InternalProcessFnParams` | params object  | `{ processor, outFile? }`                                             |
|  [03]   | `ProcessFnContext`        | context object | `{ path, htmlFileName, srvConfDir, builtinProcessorFns }`             |
|  [04]   | `ProcessFnReturn`         | return type    | `{ name, content } \| void`                                           |
|  [05]   | `ProcessFn`               | function type  | `(ctx, parsedHeaders) => ProcessFnReturn \| Promise<ProcessFnReturn>` |
|  [06]   | `DirectivesParams`        | type alias     | re-exported from `csp-typed-directives`                               |

[PUBLIC_TYPE_SCOPE]: built-in processor names
- rail: security

| [INDEX] | [PROCESSOR_NAME]        | [OUTPUT]                         |
| :-----: | :---------------------- | :------------------------------- |
|  [01]   | `CaddyJSON`             | Caddy JSON config with CSP block |
|  [02]   | `CaddyJSON_HeadersOnly` | Caddy JSON headers-only block    |
|  [03]   | `Nginx`                 | Nginx config with CSP header     |
|  [04]   | `Nginx_HeadersOnly`     | Nginx headers-only snippet       |
|  [05]   | `Caddyfile`             | Caddyfile format                 |
|  [06]   | `Caddyfile_HeadersOnly` | Caddyfile headers-only format    |

[PUBLIC_TYPE_SCOPE]: ViteCspPluginOptions key fields
- rail: security

| [INDEX] | [FIELD]                  | [TYPE]                                          | [DEFAULT]              |
| :-----: | :----------------------- | :---------------------------------------------- | :--------------------- |
|  [01]   | `enabled`                | `boolean`                                       | `true`                 |
|  [02]   | `inject`                 | `boolean`                                       | `true`                 |
|  [03]   | `injectReporting`        | `boolean`                                       | `true`                 |
|  [04]   | `onDev`                  | `'permissive' \| 'full' \| 'skip'`              | `'permissive'`         |
|  [05]   | `policy`                 | `PolicyOptions`                                 | —                      |
|  [06]   | `hashingMethod`          | `'sha256' \| 'sha384' \| 'sha512'`              | `'sha256'`             |
|  [07]   | `hashEnabled`            | `Record<'script-src'\|'style-src'\|…, boolean>` | per-directive booleans |
|  [08]   | `processFn`              | `ProcessOptions \| undefined`                   | —                      |
|  [09]   | `referrerHeaderOverride` | referrer policy string or `undefined`           | —                      |
|  [10]   | `srvConfDir`             | `string`                                        | —                      |
|  [11]   | `debugPlugin`            | `boolean`                                       | `false`                |
|  [12]   | `mapHtmlFiles`           | `Record<string, PolicyOptions> \| undefined`    | —                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory overloads
- rail: security

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [DESCRIPTION]                       |
| :-----: | :------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `createViteCspPlugin(policy, options)` | plugin factory | policy + options overload           |
|  [02]   | `createViteCspPlugin(options)`         | plugin factory | options-only overload               |
|  [03]   | `createViteCspPlugin()`                | plugin factory | zero-arg overload (uses defaults)   |
|  [04]   | `ViteCspPlugin`                        | alias          | `typeof createViteCspPlugin`        |
|  [05]   | `builtinProcessorFns`                  | constant       | map of built-in `InternalProcessFn` |
|  [06]   | `DEFAULT_OPTIONS`                      | constant       | default options snapshot            |

## [04]-[IMPLEMENTATION_LAW]

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
