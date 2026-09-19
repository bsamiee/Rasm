# [TYPESCRIPT]

pnpm resolves every version through the workspace catalog, `tsc --build` checks each composite project, Biome lints and formats every file.

## [01]-[CATALOG]

- `catalogMode: strict` fails a `pnpm add` outside the catalog range
- `workspace:*` on a sibling package resolves to the workspace copy alone
- `overrides` rows rewrite a resolved version across the graph, `<parent>><child>` scopes a row to one consumer
- `catalog:` as an override value keeps the catalog as the one version
- `allowBuilds` rows name packages with an install script in their `package.json`, `pnpm install` reports packages missing a row
- `peerDependencyRules.allowedVersions` silences the unmet peer warning and resolves nothing
- `pnpm patch <package>` extracts a package for editing, `pnpm patch-commit <dir>` writes the patch file and its `patchedDependencies` row
- `minimumReleaseAge: 0` lifts the default 1440 minute release delay
- `packages` globs match a directory on disk
- Project `package.json` lists the catalog rows its files import as `catalog:`, tools targets run take no row
- `CI` set makes `pnpm install` frozen, a lock out of step with a `package.json` fails the install in place of rewriting the lock

## [02]-[COMPILER]

- `${configDir}` in a base configuration's path fields resolves to the extending file's directory
- `tsc --build` on a `composite` project writes declarations and build info under `outDir`
- `references` names the `composite` projects a package imports, `tsc --build` builds them first
- `types` defaults to `[]`, a project lists `["node"]` where a file reads `import.meta.dirname` or a `node:` module, a UXP plugin its host typings
- `files` or `references` keys suppress TS18003 (no inputs), a project with `references` and no source omits `files`
- `include` globs resolve to TypeScript extensions alone, `files` lists an imported JSON file (`package.json` for `version`) or TS6307 reports it
- `outDir` without `composite` enforces `rootDir` (TS6059), a `noEmit` project with a file outside its root replaces `outDir` with `tsBuildInfoFile`
- Generated declaration files import a package by its specifier through the project's `node_modules` link, a `.pnpm/<hash>` path pins one install

## [03]-[EXECUTION]

- Node strips types from a `.ts` file it runs (`process.features.typescript` reads `strip`), no loader package joins
- `erasableSyntaxOnly` keeps every file strippable
- Target running a file names `node <file>.ts` with `cwd` at the project
- Workspace package's `exports` map to `.ts` files resolves through the pnpm link
- `@nx/vitest` infers `test` from a `vitest.config.ts` beside a `package.json`
- Root Vite config reads the `package.json` in `cwd`, a Vitest project config re-exports the root factory with its directory
- Root Vitest config lists `projects` from the workspace globs

## [04]-[BIOME]

- Severity string on a group enables every rule of the group at that level, `preset` beside it adds none
- `project` at `all` turns on the scanner its import rules read, a group severity alone leaves those rules silent
- `noUnresolvedImports` is `off`, `tsc --build` reports unresolved specifiers and missing exports the rule repeats
- Scanner skips ambient `declare module` declarations, `tsc --build` reads them
- `none` on a domain silences rules of a package the tree lacks
- `organizeImports` runs by default, every other source action is a row
- `useNamingConvention` takes `strictCase: false` for a host's acronym class names
- `noUndeclaredVariables` is `off`, `tsc --build` reports an undeclared name
- Host typings declare every host global, a `declare` statement or an empty `enum` stub in a plugin file restates a typings declaration
- `vcs.useIgnoreFile` and `!!` patterns in `files.includes` keep the scanner out of ignored trees
- Copy of `biome.json` with one row deleted keeps the `vcs` row, `biome check --reporter=json` over the files `git ls-files` prints decides a formatter or severity row
- `adobe:` host modules of a UXP plugin resolve through the `with-protocol` entry of a host typings package named in `types`
- `paths` row maps a host module no `with-protocol` entry declares (`adobe:indesign`) to a generated module
- Host modules a `with-protocol` entry declares take no `paths` row and no generated module
