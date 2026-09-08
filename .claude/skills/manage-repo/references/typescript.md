# [TYPESCRIPT]

Shared dependencies sit in the workspace catalog, and shared compiler, lint, and test settings at the root. Package manifests and configuration add identity, file set, and overrides.

## [01]-[PACKAGES]

`pnpm-workspace.yaml` names workspace packages by glob and holds every dependency version in its catalog:
- `packages` globs name the library, application, and test support directories
- `catalogMode: strict` rejects a dependency version outside the catalog range
- `saveExact: true` writes exact catalog entries
- `workspace:` dependencies make a project edge in the task graph, `catalog:` entries none
- `linkWorkspacePackages: deep` links a workspace package into every dependent in place of a registry copy
- `overrides` and `peerDependencyRules.allowedVersions` hold one row per conflict
- Rows that remove a declared dependency (`'-'`) state why
- `overrides` rows for a catalog package state `catalog:` with the version in the catalog alone
- `overrides` rows exist for a catalog entry the consumer range excludes
- Duplicate runtime copies of `@pulumi/pulumi` break every resource registration, the reason on the row
- Catalog entries group under one comment per responsibility
- `allowBuilds` holds one row per package with an install script
- `false` in `allowBuilds` marks a package the workspace reads as source alone
- Unimported packages leave the catalog and `allowBuilds`
- `minimumReleaseAge: 0` takes a release the day it appears
- Store and cache sit under `.cache/pnpm/`
- pnpm detects CI, turns frozen mode on, and fails `pnpm install` on lock drift

`pnpm update --latest --recursive` under `upgrade` moves the catalog to the newest release of every package and writes placeholder `allowBuilds` rows for new packages with install scripts.

## [02]-[MANIFESTS]

Each manifest holds the fields the package manager and bundler read:

| [INDEX] | [FILE]                      | [HOLDS]                                             | [NEVER_HOLDS]                                    |
| :-----: | :-------------------------- | :-------------------------------------------------- | :----------------------------------------------- |
|  [01]   | Root `package.json`         | `devDependencies`, `browserslist`, `nx` field       | `version`, `scripts`, a version string           |
|  [02]   | Package `package.json`      | `name`, `private`, `type`, `exports`, dependencies  | `version`, `scripts`, `nx` under the plugin glob |
|  [03]   | `nx` field outside the glob | Tag, `typecheck` and `test` entries, overrides      | Body a tag-filtered default supplies             |

- Dependencies state `catalog:` or `workspace:`
- `exports` maps each subpath to its `.ts` source
- Local plugin infers a project from each `tsconfig.json` under `apps/`, `libs/`, and `tests/`, tagged by language
- Packages outside the glob (`.claude/plugins/*`) declare the language tag and targets in the `nx` field
- Target overrides in the `nx` field hold fields that differ from the default, `cache` or inputs after `"..."`
- Library packages without `private` gain the release tag

## [03]-[COMPILER]

`tsconfig.base.json` and each `tsconfig.json` split the compiler configuration:

| [INDEX] | [FILE]                  | [HOLDS]                                                   | [NEVER_HOLDS]                                  |
| :-----: | :---------------------- | :-------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `tsconfig.base.json`    | Every compiler option, `include` of `${configDir}/*.ts`   | `outDir`, `types`, an implied option           |
|  [02]   | Package `tsconfig.json` | `extends`, `outDir`, `types`, its file set                | Options the base sets, `references`            |
|  [03]   | Root `tsconfig.json`    | `outDir`, `types`, root, tool, and integration files      | `references`, a `tsconfig.json` under `tools/` |

- `composite`, `declarationMap`, and `emitDeclarationOnly` make each project a build unit that emits declarations and build info alone
- `composite` implies `declaration`, the compiler defaults to `strict`, and neither appears in the file
- Files under `tools/nx/` and `infra/` compile under the root `tsconfig.json`
- Package `vitest.config.ts` files compile under the root `tsconfig.json`
- Package `tsconfig.json` excludes `vitest.config.ts`
- function-hooks `tsconfig.json` extends nothing and holds the harness contract, `../../types`, `jsx` with `h`, and `noEmit`
- `isolatedDeclarations`, `isolatedModules`, `erasableSyntaxOnly`, and `verbatimModuleSyntax` keep every file checkable and strippable on its own
- `module: "preserve"` with `allowImportingTsExtensions` keeps import specifiers as written
- `moduleDetection: "force"` makes every file a module
- `noEmitOnError`, `exactOptionalPropertyTypes`, `noUncheckedIndexedAccess`, `noPropertyAccessFromIndexSignature`, and the unused checks are on
- Each project `outDir` is `.cache/typescript/out/<root>`, holds build info beside declarations, and is the `typecheck` output

`typecheck` default runs `tsc --build --pretty false` in the project root and depends on `^typecheck`:
- Inputs are the `default`, `^production`, and `typescript` named inputs, with the `typescript` package as an external dependency
- Named input `typescript` lists the root Biome, compiler, manifest, lock, and Vite configuration files, with `node --version` as a runtime input
- `pluginsConfig["@nx/js"].analyzeSourceFiles: false` keeps the manifest edges alone, the native compiler package lacks the analyzer's compiler API

## [04]-[LINT]

Root `lint` and `format` run `biome check` with root `biome.json` over TypeScript and JSON files:
- `vcs` reads the git ignore file
- `files.includes` covers everything but the lock files and HTML
- `ignoreUnknown` skips file types Biome lacks
- `$schema` points at the installed `configuration_schema.json`
- Formatter sets four-space indentation, 150-column width, and single quotes
- Options at the Biome default (`enabled`, `semicolons`, `trailingCommas`) stay out of the file
- JSON formatting is off, JSON lint and manifest sorting on
- Linter enables every group at `error` under the `all` preset, with nursery rules selected explicitly or through configured domains
- `overrides` hold per-path exceptions (a rule off for one or every file, a naming convention for a program file, a domain off for a config file)
- Override globs are `**/<file>` once, covering the root copy of the file
- Structural rules under `tools/ast-grep/rules/typescript/` scan every TypeScript root at `lint` in place of a Biome `plugins` list
- Rules with a correction that needs the effect package ignore `.claude/plugins/**` and the spec, test, and bench globs, the reason in a comment
- `assist` actions sort imports, attributes, manifest fields, and CSS properties on every check
- `javascript.resolver.experimentalPnpmCatalogs` resolves `catalog:` entries when a rule reads a manifest

## [05]-[BUILD]

Root `vite.config.ts` exports `createViteConfig`, one configuration per kind (`app`, `library`, `server`) decoded from a schema holding the defaults:
- Default lists come from the `vite` exports (`defaultClientConditions`, `defaultServerConditions`)
- Options at their default stay out of the file
- Server bundles target the Node runtime `process.versions.node` names, browser and library bundles their own targets
- Root default export omits the build block
- `@nx/vite/plugin` entry excludes the root file from inference

## [06]-[TESTS]

`createVitestConfig` builds one project configuration from a directory:

| [INDEX] | [FILE]                     | [HOLDS]                                            | [NEVER_HOLDS]                                  |
| :-----: | :------------------------- | :------------------------------------------------- | :--------------------------------------------- |
|  [01]   | Root `vitest.config.ts`    | `createVitestConfig`, projects from the globs      | `configDefaults` items, `retry`, a bench path  |
|  [02]   | Package `vitest.config.ts` | One `createVitestConfig(import.meta.dirname)` call | Options                                        |

- Exclude list spreads `configDefaults.exclude` and adds the output directories
- Each `vitest.config.ts` beside a manifest is a Vitest root with one inferred `test` target, the root config excluded
- Project name comes from the manifest
- Reports, coverage, and benchmark output sit under `.artifacts/typescript/<kind>/<name>`
- Coverage runs on every test run through the V8 provider
- Root configuration merges the per-project coverage reports without cleaning them
- Reporters differ by the `CI` variable
- Blob reporter per project feeds `--merge-reports` from the root configuration
- `benchmark` configuration of each `test` target runs `vitest bench`, the one producer of `bench/<name>.json`
- Test support package exports each `.ts` module by subpath, holds its compiler and Vitest configs, and supplies the `setup.ts` every project runs

Root `stryker.config.json` runs mutation tests through the Vitest runner under root `mutation`:
- `mutate` names the library sources and excludes generated, built, test, config, and declaration files
- Temporary directory and incremental state sit under `.cache/stryker/`, HTML and JSON reports under `.artifacts/typescript/stryker/`

## [07]-[RELEASE]

Version and publish actions of the `typescript` release group consume `{projectRoot}/dist`, an ignored build directory holding the built manifest:
- `versionActions` is `tools/nx/typescript-version-actions.ts`, the JS actions with a `0.0.0` fallback for a source manifest with no version
- `manifestRootsToUpdate` names `{projectRoot}/dist` alone
- `groupPreVersionCommand` builds the group before the version step
- `nx-release-publish` default for the group sets `packageRoot` to `{projectRoot}/dist`
