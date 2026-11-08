# [TYPESCRIPT]

pnpm resolves catalog versions, the Effect compiler checks projects, and Biome lints and formats files.

## [01]-[CATALOG]

- `overrides` rows rewrite resolved versions and peer ranges across the graph, `<parent>><child>` scopes a row, `catalog:` keeps one version source
- Catalog versions outside a peer range take a `<parent>><peer>: 'catalog:'` row, or each importer resolves its own copy of the parent
- `permissions.<package>.build` outranks an `allowBuilds` row, builds stay in `allowBuilds` alone
- `ncu --target newest` takes the latest publish, prereleases included, `greatest` takes abandoned prerelease lines with a higher number
- `pnpm install` keeps orphans for `modulesCacheMaxAge`, `pnpm update` and `pnpm prune` remove them, then `playwright install` deletes unused browsers

## [02]-[PACKAGES]

- Packages split where runtime, dependency set, or owner differ, and merge where they always change and release together
- Capabilities sit once at the lowest level holding them whole: general libraries, then libraries one host family shares, then apps
- `exports` entries are a package's public API, consumers and test suites import a package by name, code no entry exposes sits in `internal/`
- Subpaths name a runtime, a build tool, or a module subject, one API with per-runtime implementations takes conditional exports on one subpath
- Libraries peer dependencies whose instance or types cross an entry, with ranges from the `peers` catalog, and their consumers declare them

## [03]-[COMPILER]

- `typescript` resolves to `@typescript/typescript6` for the compiler API Nx plugins, Storybook docgen, and dependency-cruiser read
- `@typescript/native` pins the native release `@effect/tsgo` builds
- Root `prepare` runs `effect-tsgo patch`, replacing the `@typescript/native` platform binary with its Effect build
- Targets' `tsc` bin and the editor's `tsdk` package run the patched binary, the `tsc` bin starting Node before each compiler process

## [04]-[EXECUTION]

- Targets call a repository program by its `rasm-` bin, its package declaring `bin` and root `package.json` linking that package through `workspace:*`
- `WorktreeCreate` blocks refuse `EnterWorktree`, `isolation: worktree` agents, and workflow agents at one event
- Sessions keep the hooks module of an installed plugin they loaded, a fresh `claude -p` session runs a changed module

## [05]-[BIOME]

- Group severity strings enable each group rule at their level over a domain's `none`, a `preset` beside one adds none
- `project` domain at `all` enables the scanner its import rules read, a group severity alone leaves import rules silent
- `noUnresolvedImports` and `noUndeclaredVariables` are `off`, `tsc` reads ambient modules the scanner skips and reports unresolved imports and names
- `noNodejsModules` is `off`, Node programs and Vite configurations import `node:` built-ins
- Overrides disable rules where a host requires the form (a loader's default export, CSF stories beside components, `cf-worker` namespace imports)
- `organizeImports` runs by default, every other source action is a row
- `useImportType` stays `auto`, an inline `type` on a types-only import leaving a module load after Node strips it
- `useExportType` takes `inlineType`, a file's local type exports joining its one `export { … }` list
- `noPrivateImports` keeps `public` visibility, `package` flagging every workspace link import and every entry's `internal/` import
- `files.ignoreUnknown` silences unknown file types in a directory walk, `files.includes` negations name tracked files of a type Biome processes
- `.claude/workflows` stays out of `files.includes`, Workflow scripts await and return at top level
- `useIgnoreFile` keeps the scanner out of a tree a root ignore row names, a tree its own nested `*` file ignores (`.venv`, `.pixi`) keeps its manifests indexed
- `!!` force-ignores a tree no ignore file covers: the `assets` submodule and large typings packages
- Dependency patterns end in `/**/*.ts`, `useBiomeIgnoreFolder` rewrites a trailing `/**` into a form that misses dependency files
- VS Code `json.schemas` names installed Biome and Nx schemas, a relative `$schema` resolves against a diff's `git:` URI and fails

## [06]-[HOST_PACKAGING]

UXP plugins package as one `.ccx` zip per host with `manifest.json` at its root:
- `UnifiedPluginInstallerAgent --install <file>.ccx` installs into the release host matching `host.app` and `minVersion`, else answers `-411`
- Beta hosts load the plugins installed for the release host of their `host.app`
- `--remove` hangs a running host and `--install` beside one reports success and writes nothing, `install` fails and apps wait while one runs
- Apps bundle each `.ccx`, their server installs one into an installed host when `PluginsInfo/v1/<host.app>.json` holds no row at its `version`
- Manifest `version` adds MinVer's height to its core patch, so each commit changes it and a release tag states its version exactly
- `install` targets replace a plugin by id at any version, a rebuild at one commit reaches the host through them alone
- Hosts load native addon binaries with a Developer ID signature and notarization alone
- Marketplace listings disclose the companion application a plugin talks to, and the plugin keeps that communication reliable
- Marketplace and independent copies of a plugin take separate plugin IDs

CEP extensions publish as a signed, timestamped `.zxp` a person installs through a ZXP installer, and this Mac extracts each into the per-user extension folder named by its bundle id. Node servers publish as an ESM single executable with `useVfs`, each native addon a signed real file the bundle resolves its package to, and an addon extracting its payload at load needs `disable-library-validation`.
