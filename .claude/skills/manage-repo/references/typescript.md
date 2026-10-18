# [TYPESCRIPT]

pnpm resolves every version through the workspace catalog, `tsc` checks each project, Biome lints and formats every file.

## [01]-[CATALOG]

- `catalogMode: strict` fails a `pnpm add` outside the catalog range
- `workspace:*` on a sibling package resolves to the workspace copy alone
- `overrides` rows rewrite resolved versions and peer ranges across the graph, `<parent>><child>` scopes a row to one consumer
- `catalog:` as an override value keeps one version source, the catalog
- Catalog versions outside a peer range take a `<parent>><peer>: 'catalog:'` row, or each importer resolves its own copy of the parent
- `ncu --target newest` takes the latest publish, prereleases included, `greatest` takes abandoned prerelease lines with a higher number
- `ncu` ranks a suffixed `github:` tag (`-with-generated-files`) below the plain one, a suffixed row joins `--reject` and an `sd` step picks its tag
- `pnpm install` keeps orphaned packages for `modulesCacheMaxAge`, `pnpm prune` removes them and `playwright install` then deletes builds no installation uses
- `allowBuilds` rows name packages with an install script, `true` runs it, `false` skips it, `pnpm install` reports a missing row
- `permissions.<package>.build` outranks an `allowBuilds` row, builds stay in `allowBuilds` alone
- `skills.dirs: []` turns off agent skill links, pnpm offers no package skill for approval and removes links it created
- `peerDependencyRules.allowedVersions` silences the unmet peer warning and resolves nothing
- `minimumReleaseAge: 0` lifts the default 1440-minute release delay
- `packages` globs match a directory on disk
- `package.json` rows name versions as `catalog:`, a project `package.json` lists the rows its files import
- `CI` set makes `pnpm install` frozen, a lock out of step with a `package.json` fails the install in place of a lock rewrite

## [02]-[COMPILER]

- `${configDir}` in a base configuration's path fields resolves to the extending file's directory
- `tsc` under `noEmit` checks a project from its directory and writes no file
- Packages import each other's `.ts` source through `exports` entries the pnpm link resolves
- `types` defaults to `[]`, a project lists each typings package declaring a global or module its files read (`node` for a `node:` module)
- `paths` maps a module specifier no `types` entry declares to a declaration file an installed package holds
- `tsc` compiles sibling source under the importer's options, the importer listing each typings package, `paths` row, and JavaScript depth it needs
- `maxNodeModuleJsDepth` counts a linked sibling package as one level, its untyped JavaScript dependencies take the sibling's depth plus one
- `lib` names declarations the host runtime ships, the Node row's in `tsconfig.base.json`, a UXP host's level, and `esnext` with `dom` in a browser
- `esnext` parts of a Node or UXP `lib` enter once the host runtime defines their members
- `tsconfig.base.json` `$schema` names the installed `typescript` schema, Schema Store's schema lacks current `lib` parts
- Editors validate a file naming `$schema` against the named schema alone, a `json.schemas` row validates beside Schema Store's
- `tsconfig.base.json` excludes each project's `vite.config.ts` and `vitest.config.ts`, root `tsconfig.json` includes each one
- Root `tsconfig.json` compiles each `.storybook/main.ts` and the test MCP config, a project program reaches `.storybook/preview` through story imports
- `asyncFunction` stays off in Playwright suites and browser `*.test.tsx`, their host APIs take promises
- Root `postinstall` patches installed `tsc` through `effect-tsgo patch`; compiler and language server report Effect diagnostics
- Catalog `typescript` holds the build `@effect/tsgo` patches, every `effect-tsgo` command fails on another build
- Compiler inputs hash tool and consumed package versions; root compilation uses Nx's all-external dependency hash
- `tsconfig.base.json` plugin entry holds every rule severity
- Project plugin entries merge over the base entry, a project with a host loading no `effect` sets `diagnostics` false

## [03]-[EXECUTION]

- Node strips types from a `.ts` file it runs (`process.features.typescript` reads `strip`), no loader package joins
- `erasableSyntaxOnly` keeps every file strippable
- Targets running a file name `node <file>.ts` with `cwd` at the project
- `@nx/vitest` infers `test` and `@nx/playwright` `e2e` from a config beside a `package.json`
- Shared Vite and Vitest factories take the project directory through `import.meta.dirname`
- Root Vitest config lists `projects` from the workspace globs
- Inline projects extend their declaring config by default, `extends` appears only as `false` or a config path
- `@rasm/database` stores reconcile at layer construction and drop or retype only what a catalog declares `retired` or `retype`
- `@rasm/database` `store` takes client constructors and supplies them its column naming, an app builds no SQL client of its own
- Store cases run once per client through `cases.ts`, each test file passes its client constructor and an isolation function
- Workers projects declare their Worker in `cloudflare.config.ts`, a library its test Worker, and `cloudflareTest` reads it through `experimental.newConfig`
- Target `types` runs `cf workers types`, writing `Cloudflare.Env` and runtime types for the compatibility date to `.cloudflare/types`, which `tsconfig.json` `types` names in place of `@cloudflare/workers-types`
- `typecheck:tsc` of a project holding `cloudflare.config.ts` depends on `types`, the file is generated and ignored
- Durable Object classes of a project's own Worker are `exports` entries reached through `cloudflare:workers` `exports`, the Vitest pool resolves no Durable Object binding naming its own Worker
- `entrypoint` imports the Worker module `with { type: 'cf-worker' }` where `exports` must type its classes
- Workers cases read bindings from `cloudflare:workers` `env`, `@rasm/test-support/workers` in `setupFiles` resets storage before each test
- Container cases sit in a Vitest project that `test` leaves out through `--project=!<name>`, the project's `test:integration` runs it
- PGlite cases write the PostgreSQL plans `lint:squawk` reads, `test` names them in `outputs`, PGlite ships the major `.config/squawk.toml` `pg_version` states

## [04]-[WEB]

[VITE]:
- Browser projects run in current browsers alone, `worker` builds at `esnext` over Vite's `baseline-widely-available` default
- Root `vite.config.ts` `react` holds React Compiler options for apps, Vitest projects, and Storybook
- `worker(<directory>, <plugins>, <hosting>)` adds `<hosting>` and the Cloudflare plugin outside Storybook and Vitest, whose CLIs set `STORYBOOK` and `VITEST`
- Cloudflare plugin offers no environment filter, its `config`, `configResolved`, and `configureServer` hooks take app type, output, and requests under any Vite server
- Cloudflare plugin state sits under `.cache/cloudflare/<project path>`, Vite mode `test` keeps every binding in memory
- App `vite.config.ts` is the app's one Vite config, Storybook finds it by default lookup and `createVitestConfig` merges it through `loadConfigFromFile`
- Web builds keep project-local `dist`, `sites()` copies `.openai/hosting.json` there with no relocation setting, the Cloudflare plugin writes its Build Output to `.cloudflare/output`
- Vite caches stay under each project's `node_modules/.vite`, Vite's `isInNodeModules(importer)` check warns on dynamic imports of any dependency it optimizes elsewhere
- `@nx/vite` stays out, it infers `serve-static`, a deprecated `serve`, and a `typecheck` beside `typecheck:tsc`, the workspace plugin infers `build` and `serve`

[STORYBOOK]:
- `.storybook/main.ts` states `framework` and `stories`, which main alone supplies, and lists `@rasm/test-support/preset`, whose addons resolve from `@rasm/test-support`
- `.storybook/preview` passes `@rasm/test-support/preview` `addons` to `definePreview` beside literal `tags`, Storybook reads both from the config directory's preview source
- `@rasm/test-support/preview` sets addon-a11y `test: 'error'`, an axe violation fails its story
- Library `viteFinal` adds `react()`, Storybook finds no `vite.config.ts` in a library
- Storybook `stories` globs name source folders, `storybookTest` replaces Vitest's `exclude` and collects stories of linked packages under a `**` glob
- Override `@storybook/react-vite>typescript` takes the `npm:` alias in named catalog `docgen`, installing TypeScript 6 for `@storybook/react` prop docgen
- Root `package.json` lists no `@storybook/react-vite`, its `typescript` peer would resolve to the root TypeScript 7
- Storybook caches stay under each project's `node_modules/.cache/storybook`, a `CACHE_DIR` row would merge every project's cache into one directory
- `@nx/storybook` stays out, its `storybook` peer range ends below 11 and its `test-storybook` repeats `test`

[BROWSER_TESTS]:
- Root `vitest.config.ts` gives Playwright browser settings to packages listing `@vitest/browser-playwright`, a project turns them on with `browser.enabled`
- Inherited `browser` settings give each inline project its own Vite server, packages without browser tests keep one shared server
- `createVitestConfig` adds an inline `storybook` project to each package holding `.storybook/main.ts`, with `browser.enabled` and the `test.api.allowWrite` `storybookTest` otherwise turns off
- `storybookTest` applies `.storybook/main.ts` presets and `viteFinal` and reads no Vite config file

[PLAYWRIGHT]:
- Suites list the app package as `workspace:*`, the edge `affected` reads
- Suite configs are `config(import.meta.dirname)` from `@rasm/test-support/playwright`, which states trace, locale, snapshot, and `.artifacts` output settings once
- Suites take `app(<app package>)` from `@rasm/test-support/app`, a worker fixture resolving the app from the suite's dependency and serving one instance per worker through its `target` case
- Case `cloudflare` serves the app's Vite config in mode `test` with a per-worker `cacheDir`, another hosting target joins as one case mapping `@rasm/hosting/bindings.ts` types to per-worker stores
- Fixtures open further pages through `browser.newContext()`, `@axe-core/playwright` rejects a page from `browser.newPage()`
- Playwright suites hold no `vitest.config.ts`, Vitest collects no `.spec.ts` of theirs
- `@nx/playwright` runs with `ciTargetName: ""`, target default `e2e` sets `cache: false` and `parallelism: true`
- `@nx/playwright` reads the top-level `outputDir` and reporter outputs `config` states, its inference hashes no imported module
- Test MCP config `tests/typescript/playwright.config.ts` adds one project per `e2e-*` directory over `config` settings, Nx infers no project from it

## [05]-[BIOME]

- Severity string on a group enables each of its rules at the string's level, `preset` beside it adds none
- `project` at `all` turns on the scanner its import rules read, a group severity alone leaves import rules silent
- `noUnresolvedImports` and `noUndeclaredVariables` are `off`, `tsc` reports each unresolved specifier, export, and name
- `noNodejsModules` is `off`, Node programs and Vite configurations import `node:` built-ins
- Scanner skips ambient `declare module` declarations, `tsc` reads them
- `none` on a domain leaves on each domain rule a group severity enables
- `organizeImports` runs by default, every other source action is a row
- `files.ignoreUnknown` silences unknown file types in a directory walk, `files.includes` negations name tracked files of a type Biome processes
- `noDefaultExport` is `off` for `.storybook/` files, Storybook reads their default exports
- `useComponentExportOnlyModules` is `off` for `*.stories.tsx`, CSF files export story objects beside the harness components they render
- `noNamespaceImport` is `off` for `cloudflare.config.ts`, a `with { type: 'cf-worker' }` entrypoint is a namespace import
