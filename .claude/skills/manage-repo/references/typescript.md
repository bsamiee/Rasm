# [TYPESCRIPT]

Keep shared TypeScript dependencies in the workspace catalog and shared compiler, lint, and test settings at the root. Package manifests and configuration add the identity, file set, and overrides their project requires.

## [01]-[PACKAGES]

`pnpm-workspace.yaml` names the workspace packages by glob and holds every dependency version in its catalog, and a manifest entry states `catalog:` in place of a version:
- `packages` globs name the library, application, and test support directories
- `catalogMode: strict` rejects a dependency version outside the catalog's range, and `saveExact: true` writes exact catalog entries
- `workspace:` dependencies make a project edge in the task graph, and `catalog:` entries make none
- `linkWorkspacePackages: deep` links a workspace package into every dependent in place of a registry copy
- `overrides` and `peerDependencyRules.allowedVersions` hold one row per conflict, and a row that removes a declared dependency (`'-'`) states why
- An `overrides` row for a catalog package states `catalog:`, and the version stays in the catalog alone
- The catalog groups its entries under one comment per responsibility, and the file holds no fetch retry or cooldown setting
- `allowBuilds` decides per package whether its install script runs, and `false` marks a package the workspace reads as source alone
- `minimumReleaseAge: 0` takes a release the day it appears, and the store and cache sit under `.cache/pnpm/`
- pnpm detects CI and turns frozen mode on, and `pnpm install` fails on lock drift

The `upgrade` target moves the catalog to the newest release of every package with `pnpm update --latest --recursive`, and pnpm writes placeholder rows under `allowBuilds` for new packages with install scripts.

## [02]-[MANIFESTS]

Each manifest holds the fields the package manager and the bundler read, and the build writes the manifest under `dist` with the tag version:

| [INDEX] | [FILE]                      | [HOLDS]                                             | [NEVER_HOLDS]                                    |
| :-----: | :-------------------------- | :-------------------------------------------------- | :----------------------------------------------- |
|  [01]   | Root `package.json`         | `devDependencies`, `browserslist`, the `nx` field   | `version`, `scripts`, a version string           |
|  [02]   | Package `package.json`      | `name`, `private`, `type`, `exports`, dependencies  | `version`, `scripts`, `nx` under the plugin glob |
|  [03]   | `nx` field outside the glob | Tag, empty `lint`, `format`, `check`, overrides     | A body a tag-filtered default supplies           |

- Dependencies state `catalog:` or `workspace:`, and `exports` maps each subpath to its `.ts` source
- The local plugin infers a project from each `tsconfig.json` under `apps/`, `libs/`, and `tests/`, tagged by language
- Packages outside that glob (`.claude/plugins/*`) declare the language tag and the empty targets in the `nx` field
- Target overrides in the `nx` field hold the fields that differ from the default, `cache` or inputs after `"..."`
- Library packages with a manifest that is not `private` gain the release tag, and their `nx-release-publish` target publishes from `dist`

## [03]-[COMPILER]

`tsconfig.base.json` holds every compiler option, and each project `tsconfig.json` adds the paths and the file set of its own root:

| [INDEX] | [FILE]                  | [HOLDS]                                                   | [NEVER_HOLDS]                                  |
| :-----: | :---------------------- | :-------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `tsconfig.base.json`    | Every compiler option, `include` of `${configDir}/*.ts`   | `outDir`, `types`, an implied option           |
|  [02]   | Package `tsconfig.json` | `extends`, `outDir`, `types`, its file set                | An option the base sets, `references`          |
|  [03]   | Root `tsconfig.json`    | `outDir`, `types`, the root, tool, and integration files  | `references`, a `tsconfig.json` under `tools/` |

- `composite`, `declarationMap`, and `emitDeclarationOnly` make each project a build unit that emits declarations and build info alone
- `composite` implies `declaration` and the compiler defaults to `strict`, and neither option appears in the file
- Files under `tools/nx/`, `infra/`, and `tests/typescript/ast-grep/` compile under the root `tsconfig.json`
- A package's `vitest.config.ts` compiles under the root `tsconfig.json`, and the package `tsconfig.json` excludes it
- The function-hooks `tsconfig.json` extends nothing and holds the harness contract, `../../types`, `jsx` with `h`, and `noEmit`
- `isolatedDeclarations`, `isolatedModules`, `erasableSyntaxOnly`, and `verbatimModuleSyntax` keep every file checkable and strippable on its own
- `module: "preserve"` with `allowImportingTsExtensions` keeps import specifiers as written, and `moduleDetection: "force"` makes every file a module
- `noEmitOnError` and the `exactOptionalPropertyTypes`, `noUncheckedIndexedAccess`, `noPropertyAccessFromIndexSignature`, and unused checks are on
- Each project's `outDir` is `.cache/typescript/out/<root>`, holds the build info beside the declarations, and is the `typecheck` output

The `typecheck` default runs `tsc --build --pretty false` in the project root and depends on `^typecheck`:
- Its inputs are the `default`, `^production`, and `typescript` named inputs, with the `typescript` package as an external dependency
- The `typescript` named input lists the root Biome, compiler, manifest, lock, and Vite configuration files, with `node --version` as a runtime input
- `pluginsConfig["@nx/js"].analyzeSourceFiles: false` keeps the manifest edges alone, the native compiler package lacks the analyzer's compiler API

## [04]-[LINT]

Configure Biome in the root `biome.json`. The `format` target runs `biome check --write --error-on-warnings` to apply corrections and formatting together. The `lint` target checks the result without writing:
- `vcs` reads the git ignore file, `files.includes` covers everything but the lock files and HTML, and `ignoreUnknown` skips file types Biome lacks
- `$schema` points at the installed `configuration_schema.json`, and no version appears in the file
- The formatter sets four-space indentation, a 150-column width, and single quotes
- Options at the Biome default (`enabled`, `semicolons`, `trailingCommas`) stay out of the file
- JSON formatting is off, and JSON lint and manifest sorting stay on
- The linter enables every group at `error` under the `all` preset, with nursery rules selected explicitly or through configured domains
- `overrides` hold the per-path exceptions: a rule off for one or every file, a naming convention for a program file, a domain off for a config file
- An override glob is `**/<file>` once, and the root copy of the file needs no second entry
- The structural rules under `tools/ast-grep/rules/typescript/` scan every TypeScript root at `lint`, and the file holds no `plugins` list
- Rules with a correction that needs the effect package ignore `.claude/plugins/**` and the spec, test, and bench globs, the reason in a comment
- `assist` actions sort imports, attributes, manifest fields, and CSS properties on every check
- `javascript.resolver.experimentalPnpmCatalogs` resolves `catalog:` entries when a rule reads a manifest

The tag-filtered `lint` default runs Biome and `ast-grep scan` over each project root. Root `lint` covers shared configuration and tooling.

## [05]-[BUILD]

The root `vite.config.ts` exports `createViteConfig`, one configuration per kind (`app`, `library`, `server`) decoded from a schema that holds the defaults:
- Default lists come from the `vite` exports (`defaultClientConditions`, `defaultServerConditions`), and the file states no option at its default
- Server bundles target the Node runtime `process.versions.node` names, and browsers and libraries keep their own targets
- The root default export omits the build block, and the `@nx/vite/plugin` entry excludes the root file from inference

## [06]-[TESTS]

The root `vitest.config.ts` exports a function that builds one project configuration from a directory, and each package's `vitest.config.ts` calls it with its own directory:

| [INDEX] | [FILE]                     | [HOLDS]                                            | [NEVER_HOLDS]                                  |
| :-----: | :------------------------- | :------------------------------------------------- | :--------------------------------------------- |
|  [01]   | Root `vitest.config.ts`    | `createVitestConfig`, the projects from the globs  | `configDefaults` items, `retry`, a bench path  |
|  [02]   | Package `vitest.config.ts` | One `createVitestConfig(import.meta.dirname)` call | An option                                      |
|  [03]   | `stryker.config.json`      | Runner, `mutate` globs, state and report paths     | A path outside `.cache/` and `.artifacts/`     |

- The exclude list spreads `configDefaults.exclude` and adds the output directories, and tests run once with no `retry`
- Every `vitest.config.ts` beside a manifest is its own root, and the Vitest plugin infers one `test` target per file, the root config excluded
- The project name comes from the manifest, and the reports, coverage, and benchmark output sit under `.artifacts/typescript/<kind>/<name>`
- Coverage runs on every test run through the V8 provider, and the root configuration merges the per-project reports without cleaning them
- Reporters differ by the `CI` variable, and the blob reporter per project feeds `--merge-reports` from the root configuration
- The `benchmark` configuration of each `test` target runs `vitest bench`, the one producer of `bench/<name>.json`
- The test support package exports each `.ts` module by subpath, holds its own compiler and Vitest configs, and every project runs its `setup.ts`

`stryker.config.json` at the root runs the mutation tests through the Vitest runner, and the root `mutation` target runs the script that invokes it:
- `mutate` names the library sources and excludes generated, built, test, config, and declaration files
- The temporary directory and the incremental state sit under `.cache/stryker/`, and the HTML and JSON reports under `.artifacts/typescript/stryker/`

## [07]-[RELEASE]

The `typescript` release group in `nx.json` versions every package tagged for release from its git tag and publishes the built manifest. Its version and publish actions consume `{projectRoot}/dist`, keep that build directory ignored and aligned with the configured release paths:
- `versionActions` is `tools/nx/typescript-version-actions.ts`, the JS actions with a `0.0.0` fallback for a source manifest with no version
- `manifestRootsToUpdate` names `{projectRoot}/dist` alone, and `groupPreVersionCommand` builds the group before the version step
- The `nx-release-publish` default for the group sets `packageRoot` to `{projectRoot}/dist`
