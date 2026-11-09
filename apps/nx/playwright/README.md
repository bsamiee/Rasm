# [NX_PLAYWRIGHT]

`@rasm/nx-playwright` is an Nx 24 plugin for Playwright suites. It adds `test` to the project holding a `playwright.config.*` beside a `package.json`, reading the config in process as the official `@nx/playwright` 24.0.5 does.

## [01]-[USE]

```json
{ "plugins": ["@rasm/nx-playwright"] }
```

- No options exist, since the config states every folder the run writes
- Built on `@nx/playwright` without what it adds outside one cached `test`: `parallelism: false` on every suite whose web server is not an `nx run` command, the `e2e-ci` atoms and merge target no documented option drops, and the web-server readiness gate; a suite starts its own server through Playwright's `webServer`

## [02]-[INFERENCE]

- The config loads through `loadConfigFile` (`@nx/devkit/internal`) after `process.__pw_initiator__` is cleared, since `@playwright/test` refuses a second import, and `process.env` is restored after each read
- Outputs are the config's `outputDir` (`test-results` when unset), each project's `outputDir`, and each reporter's `outputFile`, else `outputDir`, else `outputFolder`, with `html` at `playwright-report` and `blob` at `blob-report` by default, each relative to the config's folder
- Each project's inference cache entry is keyed through Nx's `calculateHashesForCreateNodes` on its files, the lockfile, its tsconfig chain (`walkTsconfigExtendsChain` from `@nx/js` over the project's and each ancestor's `tsconfig.json`), and the plugin source

## [03]-[TARGETS]

| [INDEX] | [TARGET] | [RUNS]            | [INPUTS]                                                        | [CACHE] |
| :-----: | :------- | :---------------- | :-------------------------------------------------------------- | :-----: |
|  [01]   | `test`   | `playwright test` | `default`, `^default`, tsconfig chain, `CI`, `@playwright/test` |   Yes   |

- `@playwright/test` holds `playwright-core`, so a browser revision moves the hash; debug and UI runs are arguments after `--` (`nx test <project> -- --debug=cli`)

## [04]-[LAYOUT]

| [INDEX] | [PATH]                     | [HOLDS]                                                   |
| :-----: | :------------------------- | :-------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts` | Package entry: `createNodes`, the config read, and `test` |
