# [TESTING]

Vitest configuration layout and module resolution of the TypeScript suites Nx tests.

## [01]-[CONFIG_LAYOUT]

Vitest facts that decide the layout:
- Vitest searches no parent folder for a config, and runs start at the workspace root or pass `--config`
- Inline projects in `test.projects` inherit every root option, as `extends` defaults to `true`
- Projects named by file or glob inherit nothing from root and merge shared options themselves
- `name` and `projects` are never inherited, and root `globalSetup` reaches no project
- Options Vitest's reference marks root-only (`reporters`, `passWithNoTests`, `silent`, `attachmentsDir`) act in root config alone
- Root config counts as a project only when `test.projects` lists it
- Inline projects setting `root`, `alias`, `plugins`, or `resolve` each start their own Vite server in place of the shared one
- `mergeConfig` concatenates arrays, and `include`, `exclude`, and `setupFiles` append to shared values
- `--project <name>` runs one project, repeats, and takes `*` and `!` patterns

Nx facts that decide the layout:
- `@rasm/nx-vitest` reads the resolved root config and infers one `test--vitest` target per Nx project holding an inline project's `root`
- Each inferred target runs the inline projects rooted in its Nx project
- Projects holding an enabled browser take tag `host:browser` and their provider's name (`playwright`) as a technology beside `vitest`
- One `nx` run batches targets sharing a config and options into one `vitest run` with one worker pool, and its own projects' test modules decide each target
- Suites with a browser provider each run alone, outside any batch
- Inferred targets hash files root `vitest.config.ts` loads inside their suite's folder or outside every suite's folder
- Edits to shared options invalidate every cached result

Root `vitest.config.ts` derives its inline projects from the tree:
- Workspace packages with `vitest` in `devDependencies` and not in `peerDependencies` are one project each, rooted there and named by manifest `name`
- Suites listing `@cloudflare/vitest-plugin` in `devDependencies` are projects `<name> workers` and `<name> node`, split by file name
- Projects `<name> workers` run `*.workers.test.ts` through `cloudflareTest` over their suite's `cloudflare.config.ts`, `<name> node` other files
- Suites listing `vitest-browser-react` in `devDependencies` run in browser mode on Playwright Chromium
- Packages listing `@storybook/addon-vitest` in `devDependencies` add a browser project through `storybookTest`, named by package
- Suites listing `nx` or `@nx/devkit` in `devDependencies` run with the Nx daemon off and workspace data in `.nx/workspace-data`
- Projects take `--disable-warning=ExperimentalWarning` in `execArgv`, and Node projects `--process-timeout` at twice `testTimeout`
- `--process-timeout` ends a synchronous hang with its stack, where Vitest's timeout timer never fires
- Runs under Vitest's inspector leave `--process-timeout` off, as Node opens no inspector beside it
- Projects write browser screenshots and traces beside the resolved `attachmentsDir`, and a lone `@rasm/nx-vitest` target sets it under `outputPath`
- Target batches write them once beside root `attachmentsDir`, and each project's attachments to its own target's `outputPath`
- Root `test` keeps the default reporter alone, as no task restores a report written for every suite, and projects inherit its `chaiConfig`

## [02]-[SOURCE_RESOLUTION]

Module resolution under Vitest and Node:
- `paths` in tsconfig reach Vite only with `resolve.tsconfigPaths: true`, and Node never reads them
- Vitest uses the `import` and `default` conditions by default
- `ssr.resolve.conditions` sets conditions for the `node` environment, `resolve.conditions` for browser mode
- Modules Node loads natively take a condition through `--conditions=<name>` in `test.execArgv`
- `require()` goes to Node and skips Vite plugins, aliases, transforms, and mocks
- Files both imported and required run twice
