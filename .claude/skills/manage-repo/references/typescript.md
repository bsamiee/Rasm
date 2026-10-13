# [TYPESCRIPT]

pnpm resolves every version through the workspace catalog, `tsc` checks each project, Biome lints and formats every file.

## [01]-[CATALOG]

- `catalogMode: strict` fails a `pnpm add` outside the catalog range
- `workspace:*` on a sibling package resolves to the workspace copy alone
- `overrides` rows rewrite a resolved version across the graph, `<parent>><child>` scopes a row to one consumer
- `catalog:` as an override value keeps one version source, the catalog
- `allowBuilds` rows name packages with an install script, `true` runs it, `false` skips it, `pnpm install` reports a missing row
- `peerDependencyRules.allowedVersions` silences the unmet peer warning and resolves nothing
- `minimumReleaseAge: 0` lifts the default 1440-minute release delay
- `packages` globs match a directory on disk
- Root `package.json` lists every catalog row except tool plugins as `catalog:`, a project `package.json` lists the rows its files import
- `CI` set makes `pnpm install` frozen, a lock out of step with a `package.json` fails the install in place of a lock rewrite

## [02]-[COMPILER]

- `${configDir}` in a base configuration's path fields resolves to the extending file's directory
- `tsc` under `noEmit` checks a project from its directory and writes no file, packages import each other's `.ts` source through `exports`
- `types` defaults to `[]`, a project lists each typings package declaring a global or module its files read (`node` for a `node:` module)
- `paths` maps a module specifier no `types` entry declares to a declaration file an installed package holds
- `tsconfig.base.json` excludes each project's `vite.config.ts` and `vitest.config.ts`, root `tsconfig.json` includes each one

## [03]-[EXECUTION]

- Node strips types from a `.ts` file it runs (`process.features.typescript` reads `strip`), no loader package joins
- `erasableSyntaxOnly` keeps every file strippable
- Targets running a file name `node <file>.ts` with `cwd` at the project
- Workspace package `exports` entries naming `.ts` files resolve through the pnpm link
- `@nx/vitest` infers `test` from a `vitest.config.ts` beside a `package.json`
- Root Vite and Vitest configs export functions of a project directory, a project config passes `import.meta.dirname`
- Root Vitest config lists `projects` from the workspace globs

## [04]-[BIOME]

- Severity string on a group enables each of its rules at the string's level, `preset` beside it adds none
- `project` at `all` turns on the scanner its import rules read, a group severity alone leaves import rules silent
- `noUnresolvedImports` and `noUndeclaredVariables` are `off`, `tsc` reports unresolved specifiers, missing exports, and undeclared names
- `noNodejsModules` is `off`, `tsc` resolves a `node:` module in projects listing `node` in `types` alone
- Scanner skips ambient `declare module` declarations, `tsc` reads them
- `none` on a domain leaves on each domain rule a group severity enables
- `organizeImports` runs by default, every other source action is a row
- `files.ignoreUnknown` silences unknown file types in a directory walk, `files.includes` negations name tracked files of a type Biome processes
