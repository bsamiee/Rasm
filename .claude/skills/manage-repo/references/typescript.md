# [TYPESCRIPT]

pnpm resolves catalog versions, the Effect compiler checks projects, and Biome lints and formats files.

## [01]-[CATALOG]

- `package.json` rows name `catalog:` versions, and `workspace:*` names a sibling resolving to its workspace copy alone
- `catalogMode: strict` fails a `pnpm add` outside the catalog range
- `overrides` rows rewrite resolved versions and peer ranges across the graph, `<parent>><child>` scopes a row, `catalog:` keeps one version source
- Catalog versions outside a peer range take a `<parent>><peer>: 'catalog:'` row, or each importer resolves its own copy of the parent
- `peerDependencyRules.allowedVersions` silences the unmet peer warning and resolves nothing
- `minimumReleaseAge: 0` lifts pnpm's default 1440-minute release delay
- `allowBuilds` rows name packages with an install script, `true` runs it, `false` skips it, `pnpm install` reports a missing row
- `permissions.<package>.build` outranks an `allowBuilds` row, builds stay in `allowBuilds` alone
- `skills.dirs: []` disables agent skill links, pnpm offers no package skill for approval and removes links it created
- `ncu --target newest` takes the latest publish, prereleases included, `greatest` takes abandoned prerelease lines with a higher number
- `ncu --reject` leaves compiler versions for `eng/typescript/upgrade/typescript.ts` to resolve
- `pnpm install` keeps orphans for `modulesCacheMaxAge`, `pnpm update` and `pnpm prune` remove them, then `playwright install` deletes unused browsers

## [02]-[COMPILER]

- Nx and the editor use the compiler in `.cache/typescript`, which pnpm links to the SDK selected by `@effect/tsgo/lib/getExePath`
- `nx run <project>:typecheck:tsc` checks one project incrementally into `.cache/tsc/<project>` and hashes its `extends` chain and ignored `typeRoots` declarations
- Project `tsconfig.json` extends `tsconfig.base.json`, `tsconfig.web.json` for a web host, or host platform library `tsconfig.json` stating host `lib` and `types`
- Programs with their own package closure (`infra`, `tools/nx`) hold their own `tsconfig.json` outside the root program
- Root `tsconfig.json` checks project `*.config.ts` and `.storybook/main.ts` files, base `exclude` keeps `${configDir}/*.config.ts` out of projects
- Library `exports` list `.` and one entry per runtime or dependency (`./node`, `./uxp`), sync generator `barrels` in `tools/nx` writing each `index.ts`
- `lint:dependency-cruiser` follows a library root entry's static load closure through installed packages and rejects a built-in or unexported module, a non-module file (stylesheet, `.node`, `.wasm`), or another library entry, `process.getBuiltinModule` probes and runtime globals stay out of a load closure
- Packages whose installed load chain ships wasm or a Node addon fail that closure, and any npm package fails an `./extendscript` entry's closure
- Each importer's `typecheck:tsc` checks a root entry's types under its own `lib` and `types`, a hooks plugin's `typecheck:claude` under the engine's types alone and `lint:claude` its load closure through `claude plugin validate`
- ExtendScript code sits in programs listing `types-for-adobe` alone, a CEP panel program holds `dom` and no ExtendScript globals
- Package self-imports are extensionless where `imports` maps each folder's extension (`"#/server/*": "./server/*.ts"` beside `"#/*": "./*.tsx"`)
- Apps whose folders mix `.ts` and `.tsx` map `"#/*": "./*"` and import with extensions
- `tsc` compiles sibling source under importer options, importers list each typings package and `paths` row it needs
- `types` defaults to `[]`, a project lists each typings package declaring a global or module its files read (`node` for a `node:` module)
- Project `types` replaces an extended list, a project adding an entry restates the list it extends
- `paths` maps a module specifier no `types` entry declares to a declaration file an installed package holds
- `lib` names declarations the host runtime has (Node's in `tsconfig.base.json`, a UXP host's level, `esnext` with `dom` in `tsconfig.web.json` for current browsers and Workers)
- Project `include` lists `.storybook/*.stories.ts` or `.stories.tsx` beside `**/*` (skips dot directories), story imports reach `.storybook/preview`
- `tsconfig.base.json` `$schema` names installed `typescript` schema, Schema Store's copy lists an older `lib` set
- `tsconfig.base.json` plugin entry holds every rule severity, its `overrides` scoping a severity to a file glob

## [03]-[EXECUTION]

- Node strips types from `.ts` files with no loader (`process.features.typescript` reads `strip`), `erasableSyntaxOnly` keeps every file strippable
- Targets running a file name `node <file>.ts` with `cwd` at the project
- Shared Vite factories take the project directory through `import.meta.dirname`
- `test:integration` selects `*.integration.test.ts` and `*.integration.test.tsx`, and `test` excludes them
- Bundle configs take `main.ts` beside them as entry
- Claude Code plugins with a `hooks/hooks.json` listing `modules` take tag `host:claude`
- `WorktreeCreate` blocks refuse `EnterWorktree`, `isolation: worktree` agents, and workflow agents at one event
- Sessions keep the hooks module of an installed plugin they loaded, a fresh `claude -p` session runs a changed module

## [04]-[WEB]

[VITE]:
- Browser projects run in current browsers alone, `cloudflareWorker` builds at `esnext` over Vite's `baseline-widely-available` default with `strictExecutionOrder`, chunks of a `lazyBarrel` client build otherwise run out of source order
- Root `vite.shared.ts` `react` holds React Compiler options for apps and Storybook, `hostedLayout` the Node output, Worker state, and ephemeral mode each `cloudflareWorker` app spreads from its directory
- Run `pnpm exec vite` from the app directory for the development server
- Web builds keep project-local `dist`, where `sites()` copies `.openai/hosting.json` with no relocation setting
- From the app directory, `ANALYZE=true pnpm exec vite build --configLoader native` writes the build and Rolldown reports under `node_modules/.rolldown/report`
- Sync generator `hosting-bindings` in `tools/nx` writes `.openai/hosting.json` `d1` and `r2` members from `cloudflare.config.ts`
- Vite caches stay in each project's `node_modules/.vite`, `isInNodeModules(importer)` warns on dynamic imports of a dependency optimized elsewhere
- `@nx/vite` stays out, it infers `serve-static`, a deprecated `serve`, and a `typecheck` beside `typecheck:tsc`

[WORKERS]:
- `cloudflare.config.ts` declares an app's Worker with `entrypoint` at its product module, or a library suite's test Worker with bindings alone
- Worker declarations take `compatibilityDate` from `@rasm/hosting/cloudflare`, whose `hostedWorker` passes every `WorkerConfig` member through over the assets, owner, and access bindings and the `hostingMode` values, a `disposable` Worker holding no domain or fetch route
- Cloudflare plugin hooks (`config`, `configResolved`, `configureServer`) take app type, output, and requests in every Vite server
- `@rasm/hosting/vite` `cloudflareWorker({ plugins, hosts, head, nodeEntry, nodeOutDir, state })` drops every hosting plugin when Storybook sets `STORYBOOK`
- Cloudflare plugin keeps state at `hostedLayout` `state.path`, `.cache/cloudflare/<project path>`, its `state.ephemeralMode`, Playwright's `e2e`, keeping bindings in memory
- Build Output stays in `.cloudflare/output` the specification fixes, `.cloudflare/types` stays where `cf workers types` takes no path setting
- Target `types` writes `Cloudflare.Env` and runtime types for the compatibility date to ignored `.cloudflare/types`, `typecheck:tsc` depends on it
- Worker projects include their generated `.cloudflare/types/index.d.ts` declarations.

[STORYBOOK]:
- Run `pnpm exec storybook dev --ci` from the project directory to serve stories without opening a browser
- `.storybook/main.ts` spreads `@rasm/test-support/main` `framework` and resolved `addons` beside its `stories`, a preset supplies no `framework`
- `.storybook/preview` passes `@rasm/test-support/preview` `addons` to `definePreview`, Storybook reads `addons` and literal `tags` from its source
- Library `viteFinal` adds `react()`, Storybook finds no `vite.config.ts` in a library
- `@storybook/react-vite` importers declare `typescript` from `docgen` catalog for the compiler API
- Storybook caches stay under each project's `node_modules/.cache/storybook`, a `CACHE_DIR` row merges every project's cache into one directory
- `@nx/storybook` stays out, its `storybook` peer range ends below 11

## [05]-[BIOME]

- Group severity strings enable each group rule at their level over a domain's `none`, a `preset` beside one adds none
- `project` domain at `all` enables the scanner its import rules read, a group severity alone leaves import rules silent
- `noUnresolvedImports` and `noUndeclaredVariables` are `off`, `tsc` reads ambient modules the scanner skips and reports unresolved imports and names
- `noNodejsModules` is `off`, Node programs and Vite configurations import `node:` built-ins
- Overrides disable rules where a host requires the form (a loader's default export, CSF stories beside components, `cf-worker` namespace imports)
- `organizeImports` runs by default, every other source action is a row
- `files.ignoreUnknown` silences unknown file types in a directory walk, `files.includes` negations name tracked files of a type Biome processes
- `.claude/workflows` stays out of `files.includes`, Workflow scripts await and return at top level
- `useIgnoreFile` keeps the scanner out of a tree a root ignore row names, a tree ignored only by its own nested `*` file (`.venv`, `.pixi`) still has its package manifests indexed
- `!!` force-ignores a tree no ignore file covers: the `assets` submodule and large typings packages
- Dependency patterns end in `/**/*.ts`, `useBiomeIgnoreFolder` rewrites a trailing `/**` into a form that misses dependency files
- VS Code `json.schemas` names installed Biome and Nx schemas, a relative `$schema` resolves against a diff's `git:` URI and fails

## [06]-[HOST_PACKAGING]

UXP plugins package as one `.ccx` zip per host with `manifest.json` at its root:
- `UnifiedPluginInstallerAgent --install <file>.ccx` installs into the release host matching `host.app` and `minVersion`, answering `-411` when none does
- Beta hosts load the plugins installed for the release host of their `host.app`
- `--remove` hangs a running host and `--install` beside one reports success and writes nothing, `install` fails and apps wait while one runs
- Apps bundle each `.ccx`, their server installs one into an installed host when `PluginsInfo/v1/<host.app>.json` holds no row at its `version`
- Manifest `version` adds MinVer's height to its core patch, so each commit changes it and a release tag states its version exactly
- `install` targets replace a plugin by id at any version, a rebuild at one commit reaches the host through them alone
- Hosts load native addon binaries with a Developer ID signature and notarization alone
- Marketplace listings disclose the companion application a plugin talks to, and the plugin keeps that communication reliable
- Marketplace and independent copies of a plugin take separate plugin IDs

CEP extensions publish as a signed, timestamped `.zxp` a person installs through a ZXP installer, and this Mac extracts each into the per-user extension folder named by its bundle id. Node servers publish as an ESM single executable with `useVfs`, each native addon a signed real file the bundle resolves its package to, and an addon extracting its payload at load needs `disable-library-validation`.
