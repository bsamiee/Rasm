# [TYPESCRIPT]

pnpm resolves every version through the workspace catalog, `tsc` checks each project, Biome lints and formats every file.

## [01]-[CATALOG]

- `catalogMode: strict` fails a `pnpm add` outside the catalog range
- `workspace:*` on a sibling package resolves to the workspace copy alone
- `overrides` rows rewrite resolved versions and peer ranges across the graph, `<parent>><child>` scopes a row to one consumer
- `catalog:` as an override value keeps one version source, the catalog
- Catalog versions outside a peer range take a `<parent>><peer>: 'catalog:'` row, or each importer resolves its own copy of the parent
- `ncu` ranks a suffixed `github:` tag (`-with-generated-files`) below the plain one, a suffixed row joins `--reject` beside an `sd` step choosing its tag
- `allowBuilds` rows name packages with an install script, `true` runs it, `false` skips it, `pnpm install` reports a missing row
- `peerDependencyRules.allowedVersions` silences the unmet peer warning and resolves nothing
- `minimumReleaseAge: 0` lifts the default 1440-minute release delay
- `packages` globs match a directory on disk, a library glob names the `<group>/<package>` depth alone
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
- `tsconfig.base.json` `$schema` names the installed `typescript` schema, editors validate a file naming `$schema` against it alone and combine a `json.schemas` row with Schema Store's, which lacks current `lib` parts
- Browser projects run in current browsers alone, root `vite.config.ts` `web` builds at `esnext`
- `tsconfig.base.json` excludes each project's `vite.config.ts` and `vitest.config.ts`, root `tsconfig.json` includes each one
- Root `postinstall` patches the installed `tsc` through `effect-tsgo patch`, `typecheck` and `tsc --lsp` report Effect diagnostics in one type check
- Catalog `typescript` holds the build `@effect/tsgo` patches, every `effect-tsgo` command fails on another build
- `tsc` targets name `typescript` and `@effect/tsgo` in `externalDependencies`, a release of either changes diagnostics
- `tsconfig.base.json` plugin entry holds every rule severity
- Project plugin entries merge over the base entry, a project with a host loading no `effect` sets `diagnostics` false

## [03]-[EXECUTION]

- Node strips types from a `.ts` file it runs (`process.features.typescript` reads `strip`), no loader package joins
- `erasableSyntaxOnly` keeps every file strippable
- Targets running a file name `node <file>.ts` with `cwd` at the project
- `@nx/vitest` infers `test` and `@nx/playwright` `e2e` from their config beside a `package.json`, a Playwright `webServer` starting the app's `serve`
- Root Vite and Vitest configs export functions of a project directory, a project config passes `import.meta.dirname`
- Root Vitest config lists `projects` from the workspace globs
- `webServer` stops its process group with SIGKILL, `nx run` leaves its task serving the port, `gracefulShutdown` SIGTERM waits for the task to exit
- Playwright suites hold no `vitest.config.ts`, Vitest collects no `.spec.ts` of theirs

## [04]-[BIOME]

- Severity string on a group enables each of its rules at the string's level, `preset` beside it adds none
- `project` at `all` turns on the scanner its import rules read, a group severity alone leaves import rules silent
- `noUnresolvedImports`, `noUndeclaredVariables`, and `noNodejsModules` are `off`, `tsc` reports unresolved specifiers, missing exports, undeclared names
- Scanner skips ambient `declare module` declarations, `tsc` reads them
- `none` on a domain leaves on each domain rule a group severity enables
- `organizeImports` runs by default, every other source action is a row
- `files.ignoreUnknown` silences unknown file types in a directory walk, `files.includes` negations name tracked files of a type Biome processes
