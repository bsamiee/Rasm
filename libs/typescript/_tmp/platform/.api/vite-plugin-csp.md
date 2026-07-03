# [API_CATALOGUE] vite-plugin-csp

`vite-plugin-csp` generates the `Content-Security-Policy`, hashes inline `<script>`/`<style>` content (via `cheerio` + `css-tree`) during `transformIndexHtml`, injects it, and optionally emits server config (Caddy/Nginx). The runtime surface is exactly two values — `ViteCspPlugin` (the factory, three overloads) and `default` (its alias) — plus the `ViteCspPluginOptions` type; `createViteCspPlugin`, `builtinProcessorFns`, and `DEFAULT_OPTIONS` are internal names, never exported. `policy` is the typed `csp-typed-directives` surface (`DirectivesObj`/`DirectivesParams`), and `processFn` is ONE parameterized server-config axis — a built-in name string, a params object, a custom function, or an array — the six built-in names are seed data, never six emitters. It runs `enforce:"post"` so hashes cover the final HTML, sequences before compression, and its Reporting-API config is the seam to `Observability`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-csp`
- package: `vite-plugin-csp`
- version: `1.1.2`
- license: `MIT`
- module: `"type": "module"` — dual `exports` `{ import: ./dist/index.js, require: ./dist/index.cjs }`, `types: dist/index.d.ts`. Runtime exports are exactly `export { Se as ViteCspPlugin, Ft as default }`; the d.ts additionally type-exports `ViteCspPluginOptions`. `createViteCspPlugin`, `builtinProcessorFns`, and `DEFAULT_OPTIONS` are declared internally and are NOT exported
- asset: build-time Vite plugin returning `Exclude<PluginOption, PluginOption[]>` (a single plugin) — deps `@rollup/pluginutils@^4`, `cheerio@^1` (inline-`<script>` extraction), `csp-typed-directives@^1.1.9` (the typed policy surface, its own dep — not separately admitted), `css-tree@^2` (inline-`<style>` extraction); peer dep `vite >=2.6.5`; no `engines` floor. `enforce:"post"` + `configResolved` + `transformIndexHtml`
- rail: build/security — CSP generation, inline-content hashing, injection, server-config emit

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: importable surface — one type
- rail: build/security
- `ViteCspPluginOptions` (`= typeof DEFAULT_OPTIONS`) is the only exported type; author config as `Partial<ViteCspPluginOptions>`.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]  | [BOUNDARY_NOTE]                                          |
| :-----: | :--------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `ViteCspPluginOptions` | options object | `typeof DEFAULT_OPTIONS` — the full resolved config shape |

[PUBLIC_TYPE_SCOPE]: structural types — reached via option fields / `Parameters<>`, not exported
- rail: build/security
- These are `declare`d internally and reachable through the option shape or `Parameters<typeof ViteCspPlugin>`; none is an `import { … }` symbol. `policy` and `processFn` are the two rich axes.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]  | [BOUNDARY_NOTE]                                                                       |
| :-----: | :----------------- | :------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `PolicyOptions`    | type alias     | `DirectivesParams \| DirectivesObj` (from `csp-typed-directives`) — the policy input   |
|  [02]   | `ProcessOptions`   | type alias     | `ProcessOption \| ProcessOption[]` — the server-config emit axis                       |
|  [03]   | `ProcessOption`    | union          | built-in name `\| InternalProcessFnParams \| ProcessFn` — one emit selection            |
|  [04]   | `ProcessFn`        | function type   | `(ctx: ProcessFnContext, parsed: CspDirectiveHeaders) => ProcessFnReturn \| Promise<…>` |
|  [05]   | `ProcessFnContext` | context object | `{ path, htmlFileName, srvConfDir, builtinProcessorFns }`                              |
|  [06]   | `ProcessFnReturn`  | return type    | `{ name: string; content: string } \| void`                                            |
|  [07]   | `InternalProcessFnParams` | params  | `{ processor: InternalProcessFnNames; outFile? }` — pin a built-in's output file        |
|  [08]   | `ViteCspPluginOpts`| type alias     | `Partial<ViteCspPluginOptions>` (internal; the factory overload authors this)          |

[PUBLIC_TYPE_SCOPE]: `csp-typed-directives` policy surface (the `policy` dep)
- rail: build/security
- `policy` accepts either a `DirectivesObj` (object keyed by directive name -> a source-list of the typed keyword vocabulary) or `DirectivesParams` (the `CspDirectives` constructor tuple). The source vocabulary is closed literal, not free strings.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [BOUNDARY_NOTE]                                                                    |
| :-----: | :-------------------- | :------------ | :--------------------------------------------------------------------------------- |
|  [01]   | `DirectivesObj`       | object type   | `{ 'default-src'?, 'script-src'?, 'style-src'?, 'frame-ancestors'?, sandbox?, … }` |
|  [02]   | `DirectivesParams`    | tuple         | `ConstructorParameters<typeof CspDirectives>` = `[csp?, sendReportsTo?, reportSubset?, referrerHeaderOverride?]` |
|  [03]   | `CspDirectiveHeaders` | object type   | `{ 'Content-Security-Policy', 'Report-To', 'Referrer-Policy' }` — the parsed headers a `ProcessFn` receives |
|  [04]   | `ValidSource` / `ValidHashes` / `ValidCrypto` | union | the source keywords: `'self'`/`'none'`/`'strict-dynamic'`/`` `nonce-${string}` ``/`` `sha256-${string}` ``/scheme (`https:`,`data:`,`blob:`)/host patterns |

[PUBLIC_TYPE_SCOPE]: `ViteCspPluginOptions` fields (`typeof DEFAULT_OPTIONS`)
- rail: build/security

| [INDEX] | [FIELD]                  | [TYPE]                                                       | [DEFAULT]              |
| :-----: | :----------------------- | :----------------------------------------------------------- | :--------------------- |
|  [01]   | `enabled`                | `boolean`                                                    | `true`                 |
|  [02]   | `inject`                 | `boolean`                                                    | `true`                 |
|  [03]   | `injectReporting`        | `boolean`                                                    | `false`                |
|  [04]   | `onDev`                  | `'permissive' \| 'full' \| 'skip'`                           | `'permissive'`         |
|  [05]   | `policy`                 | `PolicyOptions`                                              | permissive (tighten)   |
|  [06]   | `hashingMethod`          | `'sha256' \| 'sha384' \| 'sha512'`                           | `'sha384'`             |
|  [07]   | `hashEnabled`            | `Record<'script-src'\|'style-src'\|'script-src-attr'\|'style-src-attr', boolean>` | all `true`  |
|  [08]   | `nonceEnabled`           | `Record<'script-src'\|'style-src', boolean>` — DEPRECATED    | `false` (needs SSR)    |
|  [09]   | `processFn`              | `ProcessOptions \| undefined`                                | `undefined`            |
|  [10]   | `sendReportsTo`          | Reporting-API group(s) \| `undefined`                        | `undefined`            |
|  [11]   | `reportSubset`           | directive subset for report-only \| `undefined`              | `undefined`            |
|  [12]   | `referrerHeaderOverride` | referrer-policy string \| `undefined`                        | `undefined`            |
|  [13]   | `mapHtmlFiles`           | `Record<string, PolicyOptions> \| undefined`                 | `undefined`            |
|  [14]   | `srvConfDir`             | `string`                                                     | `'.server_config'`     |
|  [15]   | `debugPlugin`            | `boolean`                                                    | `false`                |

[PUBLIC_TYPE_SCOPE]: built-in processor names — the `ProcessOption` string vocabulary
- rail: build/security
- These six strings are the seed values of the `processFn` axis, passed by value; the `builtinProcessorFns` map itself is internal, never imported.

| [INDEX] | [PROCESSOR_NAME]        | [OUTPUT]                         |
| :-----: | :---------------------- | :------------------------------- |
|  [01]   | `CaddyJSON`             | Caddy JSON config with CSP block |
|  [02]   | `CaddyJSON_HeadersOnly` | Caddy JSON headers-only block    |
|  [03]   | `Nginx`                 | Nginx config with CSP header     |
|  [04]   | `Nginx_HeadersOnly`     | Nginx headers-only snippet       |
|  [05]   | `Caddyfile`             | Caddyfile format                 |
|  [06]   | `Caddyfile_HeadersOnly` | Caddyfile headers-only format    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory — `ViteCspPlugin` (three overloads) and its `default` alias
- rail: build/security
- `ViteCspPlugin` carries the overloads (`= typeof createViteCspPlugin`); `default` is the same value. `createViteCspPlugin` the NAME is not importable.

```ts
// dist/index.d.ts — the actual exports
declare function createViteCspPlugin(policy: PolicyOptions, options: ViteCspPluginOpts): Exclude<PluginOption, PluginOption[]>;
declare function createViteCspPlugin(options: Partial<ViteCspPluginOpts>): Exclude<PluginOption, PluginOption[]>;
declare function createViteCspPlugin(): Exclude<PluginOption, PluginOption[]>;
declare type ViteCspPluginOptions = typeof DEFAULT_OPTIONS;

export { ViteCspPlugin, ViteCspPluginOptions, ViteCspPlugin as default };
// runtime: export { Se as ViteCspPlugin, Ft as default }  (ViteCspPlugin === createViteCspPlugin)
```

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [BOUNDARY_NOTE]                                     |
| :-----: | :--------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `ViteCspPlugin(policy, options)`   | plugin factory | policy-first overload                               |
|  [02]   | `ViteCspPlugin(options)`           | plugin factory | options-only overload (policy inside `options.policy`) |
|  [03]   | `ViteCspPlugin()`                  | plugin factory | zero-arg overload (built-in defaults)               |
|  [04]   | `default`                          | alias          | `= ViteCspPlugin` — the default import              |

## [04]-[IMPLEMENTATION_LAW]

[CSP_TOPOLOGY]:
- hashing runs over inline `<script>` (via `cheerio`) and `<style>` (via `css-tree`) content extracted at `transformIndexHtml`; the resulting `'sha256-…'`/`'sha384-…'`/`'sha512-…'` sources are appended to `script-src`/`style-src` per `hashingMethod` × `hashEnabled`.
- `enforce:"post"` places the plugin after other HTML transforms, so the hashes cover the FINAL inline content.
- `onDev: 'permissive'` loosens the policy in dev (adds `'unsafe-inline'`/`'unsafe-eval'`) for Vite/React HMR's un-hashable inline scripts; `'full'` enforces the real policy in dev; `'skip'` disables the plugin in dev.
- the default `policy` and `hashingMethod` are unsafe production values: the shipped `policy` is permissive (`script-src`/`style-src` carry `'unsafe-inline'`/`'unsafe-eval'`) and MUST be replaced with a real `DirectivesObj`; `hashingMethod` defaults to `'sha384'`, so lowering to `'sha256'` is a deliberate downgrade, not the baseline.
- `mapHtmlFiles` applies per-HTML-file policy overrides keyed by file path (multi-page entries); `nonceEnabled` is deprecated and requires SSR.
- `debugPlugin: true` attaches an internal `debugProperties` object to the returned plugin for introspection.

[INTEGRATION_LAW]:
- Stack with `vite.md`: `ViteCspPlugin(policy, options?)` returns a single `PluginOption`, registered in `UserConfig.plugins`. Ordering law within `Shell/build.md` `BuildPipeline` — `enforce:"post"` + `transformIndexHtml` runs AFTER the React/asset/`vite-plugin-pwa` HTML transforms (hashes cover the final inline content) and BEFORE `vite-plugin-compression`'s `closeBundle` (companions match the injected HTML): `[tailwind (pre), react, …asset, pwa, csp (post), compression (closeBundle)]`.
- Typed policy via `csp-typed-directives` (the plugin's own dep): `policy` accepts a `DirectivesObj` (directive -> `ValidSource`/`ValidHashes`/`ValidCrypto` keyword list) or a `DirectivesParams` tuple (`[csp?, sendReportsTo?, reportSubset?, referrerHeaderOverride?]`); the closed source vocabulary means a malformed keyword fails at compile time, not at browser enforcement.
- PROCESSOR AXIS (the collapse): `processFn: ProcessOptions` is ONE parameterized server-config emit, not six emitters — a built-in name string (`'Nginx'`, `'Caddyfile'`, `'CaddyJSON'`, or a `…_HeadersOnly` variant) writes standard config to `srvConfDir`; an `InternalProcessFnParams` (`{ processor, outFile }`) pins the output file; a custom `ProcessFn` receives `ProcessFnContext` + parsed `CspDirectiveHeaders` for a non-standard format; an array emits several at once. The six built-in names are seed data feeding the `ProcessOption` union, never a fixed enumerated mechanism.
- Reporting-API seam with `Observability`: `injectReporting` + `sendReportsTo` (Reporting-Endpoints/Report-To groups) + `reportSubset` (report-only directive subset) + the policy's `report-to`/`report-uri` directives route CSP violation reports to a collection endpoint — the `Observability/crash.md`/`telemetry.md` sink is the natural consumer of violations as a fault signal, correlated server-side, never a browser-side handler here.
- Dev integration with `@vitejs/plugin-react`: keep `onDev: 'permissive'` so Vite/React HMR inline scripts load; the browser's runtime enforcement (and the `require-trusted-types-for: 'script'` directive hardening the DOM sinks) is the runtime consumer, outside this build-time adapter — no Effect stacking.

[RAIL_LAW]:
- Package: `vite-plugin-csp`
- Owns: CSP header generation, inline-`<script>`/`<style>` SHA hashing at `transformIndexHtml`, HTML/`<meta>` injection, the Reporting-API config, and build-time Caddy/Nginx config emission
- Accept: `policy` as `DirectivesObj`/`DirectivesParams` from `csp-typed-directives`; `Partial<ViteCspPluginOptions>` to `ViteCspPlugin`; `processFn` as the parameterized emit axis (built-in name / params / custom `ProcessFn` / array)
- Reject: hand-rolled CSP hash computation or manual `<meta http-equiv>` injection when this plugin owns the pass; importing `createViteCspPlugin`/`builtinProcessorFns`/`DEFAULT_OPTIONS` (internal, not exported); a per-format server-config emitter instead of the `processFn` axis; placing this plugin before the HTML-mutating transforms or after compression
