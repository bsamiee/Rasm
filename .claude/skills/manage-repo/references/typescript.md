# [TYPESCRIPT]

pnpm resolves every version through the workspace catalog, `tsc` checks each project, Biome lints and formats every file.

## [01]-[CATALOG]

- `packages` globs match directories, each holding a `package.json` joins the workspace
- `package.json` rows name `catalog:` versions, and `workspace:*` names a sibling resolving to its workspace copy alone
- `catalogMode: strict` fails a `pnpm add` outside the catalog range
- `overrides` rows rewrite resolved versions and peer ranges across the graph, `<parent>><child>` scopes a row, `catalog:` keeps one version source
- Catalog versions outside a peer range take a `<parent>><peer>: 'catalog:'` row, or each importer resolves its own copy of the parent
- `peerDependencyRules.allowedVersions` silences the unmet peer warning and resolves nothing
- `minimumReleaseAge: 0` lifts pnpm's default 1440-minute release delay
- `allowBuilds` rows name packages with an install script, `true` runs it, `false` skips it, `pnpm install` reports a missing row
- `permissions.<package>.build` outranks an `allowBuilds` row, builds stay in `allowBuilds` alone
- `skills.dirs: []` turns off agent skill links, pnpm offers no package skill for approval and removes links it created
- `CI` set makes `pnpm install` frozen, a lock out of step with a `package.json` fails the install in place of a lock rewrite
- `ncu --target newest` takes the latest publish, prereleases included, `greatest` takes abandoned prerelease lines with a higher number
- `ncu --reject` rows (`typescript`, a suffixed `github:` tag `ncu` ranks below the plain one) take an `sd` step writing their version
- `pnpm install` keeps orphaned packages for `modulesCacheMaxAge`, `pnpm prune` removes them, then `playwright install` deletes unused browser builds

## [02]-[COMPILER]

- `${configDir}` in a base configuration's path fields resolves to the extending file's directory
- `tsc` under `noEmit` checks a project from its directory and writes no file
- Packages import each other's `.ts` source through `exports` entries the pnpm link resolves
- `tsc` compiles sibling source under the importer's options, the importer listing each typings package, `paths` row, and JavaScript depth it needs
- `maxNodeModuleJsDepth` counts a linked sibling package as one level, its untyped JavaScript dependencies take the sibling's depth plus one
- `types` defaults to `[]`, a project lists each typings package declaring a global or module its files read (`node` for a `node:` module)
- Project `types` replaces an extended list, a project restates `types` only to add an entry
- `paths` maps a module specifier no `types` entry declares to a declaration file an installed package holds
- `lib` names declarations the host runtime has (Node's in `tsconfig.base.json`, a UXP host's level, `esnext` with `dom` in a browser)
- `esnext` parts of a Node or UXP `lib` enter once the host runtime defines their members
- Host platform library `tsconfig.json` states host `lib` and `types`, projects for the host extend it
- `tsconfig.base.json` excludes project Vite, Vitest, and UXP configs, root `tsconfig.json` compiles them, `.storybook/main.ts`, and test MCP config
- Project programs name `.storybook/*.stories.tsx` beside `**/*`, which skips dot directories, and reach `.storybook/preview` through story imports
- `tsconfig.base.json` `$schema` names installed `typescript` schema, editors read it alone, a `json.schemas` row adds Schema Store's old `lib` list
- `tsconfig.base.json` plugin entry holds every rule severity, project entries merge over it (`diagnostics: false` where a host loads no `effect`)
- Project `typecheck:tsc` hashes compiler package versions alone, root `typecheck:tsc` keeps Nx's default hash of every installed package

## [03]-[EXECUTION]

- Node strips types from `.ts` files with no loader (`process.features.typescript` reads `strip`), `erasableSyntaxOnly` keeps every file strippable
- Targets running a file name `node <file>.ts` with `cwd` at the project
- Shared Vite and Vitest factories take the project directory through `import.meta.dirname`

## [04]-[WEB]

[VITE]:
- Browser projects run in current browsers alone, `worker` builds at `esnext` over Vite's `baseline-widely-available` default
- Root `vite.config.ts` `react` holds React Compiler options for apps, Vitest projects, and Storybook
- App `vite.config.ts` is its one Vite config, Storybook finds it by default lookup and `createVitestConfig` merges it through `loadConfigFromFile`
- Web builds keep project-local `dist`, where `sites()` copies `.openai/hosting.json` with no relocation setting
- Vite caches stay in each project's `node_modules/.vite`, `isInNodeModules(importer)` warns on dynamic imports of a dependency optimized elsewhere
- `@nx/vite` stays out, it infers `serve-static`, a deprecated `serve`, and a `typecheck` beside `typecheck:tsc`

[WORKERS]:
- `cloudflare.config.ts` declares an app's Worker or a library suite's test Worker with `entrypoint` at a module beside the suite under `tests/`
- Cloudflare plugin hooks (`config`, `configResolved`, `configureServer`) take app type, output, and requests in every Vite server
- `worker(<directory>, <plugins>, <hosting>)` drops `<hosting>` and Cloudflare plugin when Storybook or Vitest CLIs set `STORYBOOK` or `VITEST`
- Cloudflare plugin keeps state in `.cache/cloudflare/<project path>` and Build Output in `.cloudflare/output`, mode `test` keeps bindings in memory
- Target `types` writes `Cloudflare.Env` and runtime types for the compatibility date to ignored `.cloudflare/types`, `typecheck:tsc` depends on it
- Project `tsconfig.json` `types` names `./.cloudflare/types` in place of `@cloudflare/workers-types`
- Durable Object classes of a project's own Worker are `exports` entries reached through `cloudflare:workers` `exports`
- `entrypoint` imports the Worker module `with { type: 'cf-worker' }` where `exports` must type its classes

[STORYBOOK]:
- `.storybook/main.ts` states `framework` and `stories`, which no preset supplies, and lists `@rasm/test-support/preset` resolving its addons
- Storybook reads `addons` and literal `tags` from `.storybook/preview` source, which passes `@rasm/test-support/preview` `addons` to `definePreview`
- Library `viteFinal` adds `react()`, Storybook finds no `vite.config.ts` in a library
- Root `package.json` lists no `@storybook/react-vite`, its `typescript` peer would take root TypeScript 7 over `docgen` catalog's TypeScript 6
- Storybook caches stay under each project's `node_modules/.cache/storybook`, a `CACHE_DIR` row would merge every project's cache into one directory
- `@nx/storybook` stays out, its `storybook` peer range ends below 11 and its `test-storybook` repeats `test`

## [05]-[BIOME]

- Group severity strings enable each group rule at their level over a domain's `none`, a `preset` beside one adds none
- `project` domain at `all` turns on the scanner its import rules read, a group severity alone leaves import rules silent
- `noUnresolvedImports` and `noUndeclaredVariables` are `off`, `tsc` reads ambient modules the scanner skips and reports unresolved imports and names
- `noNodejsModules` is `off`, Node programs and Vite configurations import `node:` built-ins
- Overrides turn off rules where a host requires the form (a loader's default export, CSF stories beside components, `cf-worker` namespace imports)
- `organizeImports` runs by default, every other source action is a row
- `files.ignoreUnknown` silences unknown file types in a directory walk, `files.includes` negations name tracked files of a type Biome processes
