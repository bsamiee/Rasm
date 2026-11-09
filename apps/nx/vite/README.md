# [NX_VITE]

`@rasm/nx-vite` is an Nx 24 plugin for the Vite toolchain. It adds Vite builds, Vitest suites, Storybook builds, and Cloudflare Worker types to the projects Nx core makes from `package.json`, reading each config in process through the tool's own resolver: Vite's `resolveConfig` and Vitest's `createVitest`.

## [01]-[USE]

```json
{ "plugins": ["@rasm/nx-typescript", { "plugin": "@rasm/nx-vite", "options": { "outputPath": ".artifacts" } }] }
```

| [INDEX] | [OPTION]     | [UNSET]                                | [SET]                                                                       |
| :-----: | :----------- | :------------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `outputPath` | Storybook in `<root>/storybook-static` | Storybook in `<outputPath>/storybook/<project name>` through `--output-dir` |

- `outputPath` exists because Storybook takes its output folder from `--output-dir` alone; every Vite and Vitest folder comes from the resolved config
- List it after `@rasm/nx-typescript`, so its `check--tsc` addition for Cloudflare projects merges onto the compiler's target
- Built on the official `@nx/vite`, `@nx/vitest`, and `@nx/storybook` 24.0.5 logic without their targets outside the verb set: `@nx/vite` always adds `build-deps`, `watch-deps`, `dev`, the deprecated `serve`, `preview`, and `serve-static` (an `@nx/web` executor), `@nx/vitest` adds `test` to any config holding a `test` field whether or not a test file exists, and `@nx/storybook` peers Storybook below 11 and adds `storybook`, `static-storybook`, and `test-storybook`

## [02]-[INFERENCE]

| [INDEX] | [SOURCE]                                                 | [GIVES]                                                                                                                          |
| :-----: | :------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `vite.config.*` beside a `package.json`                  | `build--vite` where the resolved config builds: `build.lib`, `builder.buildApp`, an entry on any environment, or an `index.html` |
|  [02]   | `vitest.config.*`, else a `vite.config.*` holding `test` | `test` where Vitest's `globTestSpecifications` finds a test file                                                                 |
|  [03]   | `.storybook/main.*`                                      | `build--storybook`                                                                                                               |
|  [04]   | `cloudflare.config.ts`                                   | `build--cf`, which the project's `check--tsc` waits on, hashing its declarations                                                 |

- Configs resolve in the mode their target runs: Vite as `vite build` resolves it (`build`, `production`), Vitest under the environment its CLI launches with (`TEST`, `VITEST`, `NODE_ENV` defaulting to `test`); Vite sets `NODE_ENV` to the read's default when unset and plugins set their own variables, so the plugin restores `process.env` after each read and reads one config at a time
- `build--vite` declares the top-level `build.outDir` and every environment's; `test` declares Vitest's `coverage.reportsDirectory`, `attachmentsDir`, each reporter's `outputFile` or `outputDir`, and each browser project's `trace.tracesDir`, nested folders folded into their parent
- Each project's inference cache entry is keyed through Nx's `calculateHashesForCreateNodes` on its files, the lockfile, its tsconfig chain, the options, and the plugin source, and kept while every workspace module its configs loaded hashes unchanged: Vite's bundled config loader keeps package imports external and reports the config file alone in `configFileDependencies`, so the plugin records each module Node resolves through `module.registerHooks` during the reads
- The tsconfig chain is every file `walkTsconfigExtendsChain` (`@nx/js`) reaches from the project's `tsconfig.json` and each ancestor folder's, the files Vite's transform and Vitest read

## [03]-[TARGETS]

| [INDEX] | [TARGET]           | [RUNS]                    | [INPUTS]                                              | [CACHE] |
| :-----: | :----------------- | :------------------------ | :---------------------------------------------------- | :-----: |
|  [01]   | `build--vite`      | `vite build`              | `default`, `^default`, tsconfig chain, `vite`         |   Yes   |
|  [02]   | `build--storybook` | `storybook build`         | `default`, `^default`, tsconfig chain, `storybook`    |   Yes   |
|  [03]   | `build--cf`        | `cf workers types`        | `default`, `^default`, `cf`                           |   Yes   |
|  [04]   | `test`             | `vitest run`              | `default`, `^default`, tsconfig chain, `CI`, `vitest` |   Yes   |
|  [05]   | `build`            | `nx:noop` over `build--*` | `'...'` and the union of its parts                    |   No    |

- `^default` hashes the npm packages the project and its workspace dependencies declare, which hold each runner's plugins, pool, and browser provider; the runner row stops Nx hashing every npm package
- Storybook story tests run inside `test` as a Vitest project through `@storybook/addon-vitest`, so no story-test target exists

## [04]-[LAYOUT]

| [INDEX] | [PATH]                     | [HOLDS]                                                                    |
| :-----: | :------------------------- | :------------------------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts` | Package entry: `createNodes`, the in-process config reads, and the targets |
