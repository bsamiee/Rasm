# [NX_TYPESCRIPT]

`@rasm/nx-typescript` is an Nx 24 plugin for the TypeScript 7 compiler and the pnpm workspace. It adds type checking, emit, and package checks to the projects Nx core makes from `package.json`, reading each program through the compiler's own `@typescript/native/sync` API, and adds a catalog upgrade to the project holding `pnpm-workspace.yaml`. Vite, Vitest, Storybook, and Cloudflare targets come from `@rasm/nx-vite`, Playwright suites from `@rasm/nx-playwright`, and Adobe UXP and CEP packages from `@rasm/nx-distribution`.

## [01]-[USE]

```json
{ "plugins": ["@rasm/nx-typescript"] }
```

- No options exist, since the compiler and pnpm state every fact the plugin reads: `tsconfig.json` and its `extends` chain, `noEmit`, `outDir`, and `declarationDir` decide checks and emits, `pnpm-workspace.yaml` names the catalog and its holds
- TypeScript installs as Nx's TypeScript 7 guide pairs it: `@typescript/native` (`npm:typescript@7`) is the compiler this plugin reads, runs as `tsc`, and hashes, a peer the workspace supplies, while `typescript` (`npm:@typescript/typescript6`) keeps the 6.0 compiler API that Nx core, `@nx/js`, and Storybook's docgen load by that name

## [02]-[INFERENCE]

| [INDEX] | [SOURCE]                                      | [GIVES]                                                                                                                                                   |
| :-----: | :-------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `tsconfig.json` beside a `package.json`       | One TypeScript program set per project                                                                                                                    |
|  [02]   | `parseConfigFile` project references          | `tsc --project` over the config holding files, else over each config a solution references; `tsc --build` only for a config that itself references others |
|  [03]   | `Program.getConfigFileNames()`                | Every extended layer outside the project, as inputs of `check--tsc` and `build--tsc`                                                                      |
|  [04]   | Parsed `noEmit`, `outDir`, `declarationDir`   | `build--tsc` with those folders as outputs where a config emits                                                                                           |
|  [05]   | An emitting config                            | `check--publint` where `publint` resolves from the workspace root, private packages included                                                              |
|  [06]   | `pnpm-workspace.yaml` beside a `package.json` | `upgrade--pnpm`                                                                                                                                           |

- One compiler API process reads every project whose inference cache misses, each project keyed through Nx's `calculateHashesForCreateNodes` on its own files, the options, and the plugin source, its entry kept while its extended layers hash unchanged
- Solution references are checked one config at a time with `--project`, which writes nothing under `noEmit`; `tsc --build` writes a `.tsbuildinfo` beside each config even with `--incremental false`, and Nx's cache already skips an unchanged check
- `@effect/tsgo` joins the type-check hash when the workspace installs it, since `effect-tsgo patch` replaces the compiler binary
- `publint` packs the package through the package manager it detects, scripts off, and lints the tarball against its `package.json` with `publishConfig` applied; `--strict` turns its warnings into errors, its suggestions print without failing, and the check caches on the project's files and its `build--*` outputs, so a missing `dist` file or a wrong `exports` row fails `check`

## [03]-[TARGETS]

| [INDEX] | [TARGET]                    | [RUNS]                                             | [WHEN]                                         | [CACHE] |
| :-----: | :-------------------------- | :------------------------------------------------- | :--------------------------------------------- | :-----: |
|  [01]   | `check--tsc`                | `tsc --project` per checked config                 | Every program set without emit                 |   Yes   |
|  [02]   | `build--tsc`                | `tsc --project` or `tsc --build`                   | Emitting config; `check--tsc` waits on it      |   Yes   |
|  [03]   | `check--publint`            | `publint --strict` after `build--*`                | Config emits, `publint` installed              |   Yes   |
|  [04]   | `upgrade--pnpm`             | `upgrade` executor, then `pnpm update --recursive` | `pnpm-workspace.yaml` in the project           |   No    |
|  [05]   | `check`, `build`, `upgrade` | `nx:noop` over `<verb>--*`                         | Inputs open with `'...'` and union their parts |   No    |

- `upgrade--pnpm` moves each default `catalog` row naming a version to the highest version among its package's dist-tags (`pnpm view <name> dist-tags`), prereleases included, keeping the file's quotes and comments through `yaml`'s source tokens; rows `update.ignoreDeps` matches stay, as `pnpm update` itself holds them, and named catalogs such as `catalogs.peers` stay, since they state ranges for consumers; `npm:` aliases and Git rows move by hand
- `pnpm update --recursive` then refreshes the lockfile inside the new rows, its stderr passing through

## [04]-[RELEASE]

- Packages emit through their `tsconfig.json` (`declaration`, `outDir` at `dist`, no `noEmit`), private ones included, so `publint` holds every package to the published layout before its `private` flag goes; a project publishing no package states `noEmit` with `declaration` off, since declaration checks run under `noEmit` too; `build--tsc` writes JavaScript and declarations per source file into `dist`, the same `tsc --project` run the editor and a direct build make; `publishConfig.exports` and `publishConfig.imports` (`"#/*": "./dist/*.js"`) name those files and pnpm applies them at pack, so extensionless `#/` self-imports emit unchanged and resolve in `dist`
- `nx-release-publish` comes from Nx core with `dependsOn: ["^nx-release-publish"]` alone, and a plugin target cannot add `build` to it (Nx merges core's `package.json` targets after every plugin), so `nx release` builds through `release.version.preVersionCommand` in `nx.json`; npm packages need no version actions, since Nx core tags each `package.json` project `npm:public` or `npm:private` and versions it with `@nx/js`

## [05]-[LAYOUT]

| [INDEX] | [PATH]                     | [HOLDS]                                                                  |
| :-----: | :------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts` | Package entry: `createNodes`, the compiler read, targets, and aggregates |
|  [02]   | `executors/upgrade/`       | `upgrade` executor and its Nx schema                                     |
