# [TS_TESTS_API_VITEST_UI]

[PACKAGE_SURFACE]:
- package: `@vitest/ui` · version `4.1.10` · license `MIT`
- module: ESM (`type: module`); subpaths `.` (the dashboard Vite-plugin factory `(ctx: Vitest) => Vite.Plugin`) and `./reporter` (the `html` reporter — a `Reporter`). Peer: `vitest 4.1.9`.
- asset: `dist/index.d.ts` (`declare const _default: (ctx: Vitest) => Vite.Plugin`) · `reporter.d.ts` (`declare const reporter: Reporter`).
- runtime: the dashboard is a Vite-served browser app over the runner's WebSocket RPC; the `html` reporter emits a self-contained static bundle (no server) openable offline.
- plane: `plane:dev` — spec inspection; the `tests/typescript/_architecture` suite asserts no `plane:runtime` graph imports it.
- rail: spec-run inspection — an agent/CI face over a run, never a runtime dependency, never a gauge input.

`@vitest/ui` is two surfaces in one package, both reading the same `Vitest` + Reported-Tasks tree (`TestModule`/`TestCase`, see `vitest.md`): (1) the LIVE dashboard — a Vite plugin activated by `--ui` / `test.ui: true`, served at `test.uiBase` (default `/__vitest__/`) on the API port over the runner's `WebSocketRPC`, streaming the module graph, task tree, console output, and (when coverage is on) the istanbul HTML tree; (2) the DURABLE `html` reporter (`reporters: ['html']`) — the same UI frozen to a static artifact under `HTMLOptions.outputFile`, the CI-visible face of a run. In browser mode (`browser.ui: true`) the dashboard embeds and shows the real browser iframe beside the tree. There is no runtime API here to compose against — the package is configured (a config flag, a reporter row), not imported.

## [01]-[DASHBOARD_PLUGIN]

[ENTRYPOINT_SCOPE]: the live UI — a Vite plugin factory the runner mounts; the design enables it by config, not by import.

| [INDEX] | [SURFACE]                               | [FAMILY]       | [CAPABILITY]                                                     |
| :-----: | :-------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `default: (ctx: Vitest) => Vite.Plugin` | plugin factory | the dashboard app; mounted when `test.ui`/`--ui` is set          |
|  [02]   | `test.ui: true` / `--ui`                | activation     | serve the dashboard; requires `test.api` (a server port) open    |
|  [03]   | `test.uiBase` (default `/__vitest__/`)  | mount path     | dashboard route on the API server                                |
|  [04]   | `test.open` (default `!CI`)             | auto-open      | launch the browser at the dashboard on start                     |
|  [05]   | `browser.ui: true`                      | browser mode   | embed the dashboard in browser mode with the live browser iframe |

```ts signature
// index.d.ts — the default export is a Vite plugin factory; vitest mounts it, you never call it. Enable by config.
declare const _default: (ctx: Vitest) => Vite.Plugin; export { _default as default }
// defineConfig({ test: { ui: true, api: { port: 51204 }, uiBase: '/__vitest__/', open: false } })  — reads the runner's WebSocketRPC
```

## [02]-[HTML_REPORTER]

[ENTRYPOINT_SCOPE]: the durable report — the `html` reporter, the UI frozen to a static bundle. This is the CI/artifact face; `reporters` is a `vitest` config row.

| [INDEX] | [SURFACE]                                    | [FAMILY]   | [CAPABILITY]                                                              |
| :-----: | :------------------------------------------- | :--------- | :------------------------------------------------------------------------ |
|  [01]   | `./reporter` `default: Reporter`             | reporter   | the `html` builtin — resolved when `reporters` names `'html'`             |
|  [02]   | `reporters: ['html']` / `['default','html']` | activation | emit the static report alongside other reporters                          |
|  [03]   | `HTMLOptions.outputFile`                     | output     | report path (default `html/index.html`); tuple `['html', { outputFile }]` |
|  [04]   | coverage `htmlDir`                           | embed      | the istanbul coverage HTML the report links (see `vitest-coverage-v8.md`) |

```ts signature
// reporter.d.ts — the html reporter is a Reporter (vitest/node); config names it, never imports it.
declare const reporter: Reporter; export { reporter as default }
interface HTMLOptions { outputFile?: string }
// defineConfig({ test: { reporters: ['default', ['html', { outputFile: 'html/index.html' }]] } })
```

## [03]-[INTEGRATION]

[STACK: `@vitest/ui` ← `vitest`] — the read seam. Both surfaces consume the runner's `WebSocketRPC` (`WebSocketHandlers`/`WebSocketEvents`, see `vitest.md`) and the Reported-Tasks tree — the module graph, task results, console logs, annotations (`recordArtifact`), and unhandled errors; the design enables the UI purely through `test.ui`/`reporters`; there is no programmatic surface to bind.

[STACK: `@vitest/ui` + `@vitest/coverage-v8`] — the coverage embed. With `test.coverage.enabled` + a `reporter: ['html']` coverage row, `htmlDir` holds the istanbul tree; both the live dashboard and the `html` test report link it, so a run's coverage is inspectable inline with its spec tree — the two-artifact face of one run.

[STACK: `@vitest/ui` + `@vitest/browser-playwright`] — the browser embed. `browser.ui: true` (see `vitest-browser-playwright.md`) mounts the dashboard in browser mode with the real browser iframe beside the task tree, so a failing browser-runtime spec is inspected in the actual DOM it ran in — not a reconstruction.

## [04]-[RAIL_LAW]

- Owns: spec-run inspection — the live dashboard (a Vite plugin over the runner's WebSocket RPC) and the durable `html` reporter (the UI frozen to a static offline bundle); both project the same Reported-Tasks tree, and it embeds coverage HTML and the browser-mode iframe.
- Accept: `test.ui`/`--ui` + `test.api` for the live dashboard during a watch/debug session; `reporters: ['html']` (tuple form for `outputFile`) for the durable CI artifact; `browser.ui` in browser mode; the coverage `htmlDir` link when coverage is on.
- Reject: importing this package in a spec or a gauge (it is configured, not composed — there is no runtime API); the `html` reporter as a pass/fail gate (it is a view; the gate is coverage thresholds + mutation score); a bespoke result dashboard where this reads the runner's own RPC; any `plane:runtime` import.
- Boundary: the live dashboard needs `test.api` (a server port) open; it is a dev/debug and CI-artifact surface, never load-bearing on a pass/fail decision; the package exposes no members to import — its whole contract is a config flag and a reporter name; a "stacking" claim beyond config is a category error.
