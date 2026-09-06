# [TYPESCRIPT]

The TypeScript package, compiler, lint, and test tooling of a language area is one workspace catalog, one base compiler configuration, and one root configuration per tool, and each package adds the entries that name it alone. Every TypeScript file type is scanned for scattering, duplication, and misplaced declarations, a manifest exists only where one belongs, and the root holds the catalog entries.

## [01]-[PACKAGES]

`pnpm-workspace.yaml` names the workspace packages by glob and holds every dependency version in its catalog, and a manifest entry states `catalog:` in place of a version:
- `packages` globs name the library, application, and test support directories
- `catalogMode: strict` rejects a dependency version outside the catalog's range, and `saveExact: true` writes exact catalog entries
- `workspace:` dependencies make a project edge in the task graph, and `catalog:` entries make none
- `linkWorkspacePackages: deep` links a workspace package into every dependent in place of a registry copy
- `overrides` and `peerDependencyRules.allowedVersions` hold one row per conflict, and a row that removes a declared dependency (`'-'`) states why
- `allowBuilds` decides per package whether its install script runs, and `false` marks a package the workspace reads as source alone
- `minimumReleaseAge: 0` takes a release the day it appears, and the store and cache sit under `.cache/pnpm/`
- pnpm detects CI and turns frozen mode on, and `pnpm install` fails on lock drift

The `upgrade` target moves the catalog to the newest release of every package with `pnpm update --latest --recursive`, and pnpm writes placeholder rows under `allowBuilds` for new packages with install scripts.

## [02]-[MANIFESTS]

Each package manifest holds the fields the package manager and the bundler read, and no manifest holds a `scripts` field:
- `name`, `private`, `type: "module"`, an `exports` map from each subpath to its `.ts` source, and dependencies as `catalog:` or `workspace:`
- No source manifest holds a `version`, the build writes the manifest under `dist` and the release writes the tag version into that copy
- The root manifest holds the development dependencies, the `browserslist` query, and the `nx` field with the root project's tag and targets
- The local plugin infers a project from each `tsconfig.json` under the library, application, and test directories, tagged by language
- Library packages with a manifest that is not `private` gain the release tag, and their `nx-release-publish` target publishes from `dist`

## [03]-[COMPILER]

`tsconfig.base.json` holds every compiler option, each project `tsconfig.json` extends it with its `outDir`, `types`, and file set, and `tsc --build` checks the projects in dependency order:
- `composite`, `declaration`, `declarationMap`, and `emitDeclarationOnly` make each project a build unit that emits declarations and build info alone
- `isolatedDeclarations`, `isolatedModules`, `erasableSyntaxOnly`, and `verbatimModuleSyntax` keep every file checkable and strippable on its own
- `module: "preserve"` with `allowImportingTsExtensions` keeps import specifiers as written, and `moduleDetection: "force"` makes every file a module
- `noEmitOnError` and the `exactOptionalPropertyTypes`, `noUncheckedIndexedAccess`, `noPropertyAccessFromIndexSignature`, and unused checks are on
- `strict` stays out of the file because the compiler defaults to it, and `include` of `${configDir}/*.ts` reads each project's own root
- Each project's `outDir` is `.cache/typescript/out/<root>`, holds the build info beside the declarations, and is the `typecheck` output
- The root `tsconfig.json` includes the root configuration files, the test support Vitest config, the plugin files, and the infrastructure program

The `typecheck` default runs `tsc --build --pretty false` in the project root and depends on `^typecheck`:
- Its inputs are the `default`, `^production`, and `typescript` named inputs, with the `typescript` package as an external dependency
- The `typescript` named input lists the root Biome, compiler, manifest, lock, and Vite configuration files, with `node --version` as a runtime input
- `pluginsConfig["@nx/js"].analyzeSourceFiles: false` keeps the manifest edges alone, the native compiler package lacks the analyzer's compiler API

## [04]-[LINT]

`biome.json` at the root is the one linter and formatter configuration, the `lint` default runs `biome check --write --error-on-warnings` per project root, and the `format` default runs `biome format --write`:
- `vcs` reads the git ignore file, `files.includes` covers everything but the lock files and HTML, and `ignoreUnknown` skips file types Biome lacks
- The formatter sets four-space indentation and a 150-column width, single quotes, semicolons always, and trailing commas everywhere
- The linter enables every group at `error` under the `all` preset with four domains at `all`, and each nursery rule is named, the preset skips them
- `overrides` hold the per-path exceptions: a rule off for one or every file, a naming convention for a program file, a domain off for a config file
- The GritQL plugins under `tools/biome/` apply to library, application, tool, and infrastructure sources and stay off test and benchmark files
- `assist` actions sort imports, attributes, manifest fields, and object properties on every check
- `javascript.resolver.experimentalPnpmCatalogs` resolves `catalog:` entries when a rule reads a manifest

The tag-filtered `lint` default runs Biome then `ast-grep scan` over each project root, and the root project's own `lint` target covers the root files.

## [05]-[TESTS]

The root `vitest.config.ts` exports a function that builds one project configuration from a directory, and each package's `vitest.config.ts` calls it with its own directory:
- Every `vitest.config.ts` beside a manifest is its own root, and the Vitest plugin infers one `test` target per file, the root config excluded
- The project name comes from the manifest, and the reports, coverage, and benchmark output sit under `.artifacts/typescript/<kind>/<name>`
- Coverage runs on every test run through the V8 provider, and the root configuration merges the per-project reports without cleaning them
- Reporters differ by the `CI` variable, and the blob reporter per project feeds `--merge-reports` from the root configuration
- The `benchmark` configuration of each `test` target runs `vitest bench`, the one producer of `bench/<name>.json`
- The test support package exports each `.ts` module by subpath, holds its own compiler and Vitest configs, and every project runs its `setup.ts`

`stryker.config.json` at the root runs the mutation tests through the Vitest runner, and the root `mutation` target runs the script that invokes it:
- `mutate` names the library sources and excludes generated, built, test, config, and declaration files
- The temporary directory and the incremental state sit under `.cache/stryker/`, and the HTML and JSON reports under `.artifacts/typescript/stryker/`

## [06]-[RELEASE]

The `typescript` release group in `nx.json` versions every package tagged for release from its git tag and publishes the built manifest:
- `versionActions` is `tools/nx/typescript-version-actions.ts`, the JS actions with a `0.0.0` fallback for a source manifest with no version
- `manifestRootsToUpdate` names `{projectRoot}/dist` alone, and `groupPreVersionCommand` builds the group before the version step
- The `nx-release-publish` default for the group sets `packageRoot` to `{projectRoot}/dist`
