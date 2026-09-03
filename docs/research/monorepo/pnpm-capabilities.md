<!-- Source for monorepo-build-infrastructure [02]-[TOOLCHAIN] and [03]-[CONFIGURATION], nothing integrated yet -->
# pnpm capabilities for the Rasm TypeScript area

Versions and documentation were read on 2026-09-03.

## [00]-[VERSION_BASELINE]

Newest stable pnpm is 12.3.1, published 2026-09-03 (GitHub Releases API, <https://github.com/pnpm/pnpm/releases>). The 12 line: 12.3.0 (2026-09-02), 12.2.1 (2026-09-01), 12.2.0 (2026-09-01), 12.1.0 (2026-08-29), 12.0.0 (2026-08-26). The 11 line still receives releases: 11.25.0 (2026-08-29), 11.24.0 (2026-08-24), 11.23.0 (2026-08-23). On npm, `latest` is 11.25.0 and `next-12` is 12.3.1.

Every documentation quote is from the current `main` of <https://github.com/pnpm/pnpm.io>, the source of <https://pnpm.io>, read 2026-09-03, and the per-setting "Added in" markers are quoted from that source.

Rasm pins pnpm to the 11 line: `package.json` declares `devEngines.packageManager` `{ "name": "pnpm", "version": ">=11.9.0 <12", "onFail": "error" }`, and the installed pnpm is 11.24.0 under Node v26.7.0. The plan settles the move to 12 ("pnpm 12.3.1 is current", "newest stable tools").

### [00.1]-[CURRENT_FILES]

Read 2026-09-03. `pnpm-workspace.yaml` uses `packages`, `cacheDir: .cache/pnpm/cache`, `storeDir: .cache/pnpm/store`, `minimumReleaseAge: 1440`, `catalog` (one default catalog, 279 entries), `catalogMode: strict`, `fetchRetries`, `fetchRetryFactor`, `fetchRetryMaxtimeout`, `fetchRetryMintimeout`, `linkWorkspacePackages: deep`, `overrides` (9 entries, including the removal `'@nx/vitest>@nx/eslint': '-'`), `peerDependencyRules.allowedVersions` (8 entries), `allowBuilds` (23 entries, 22 `true`, `puppeteer: false`), `preferWorkspacePackages: true`, `saveExact: true`, `strictSsl: true`.

The measured shape: the catalog holds 279 entries, and the root `package.json` holds 279 `devDependencies`, every one the literal `"catalog:"`. The lists hold the same 279 names in different orders (the catalog is grouped by comment headers, the manifest is alphabetical).

No `.npmrc` exists at the root. The root `package.json` has no `scripts` field, no `packageManager` field, and `engines.node: ">=24.15.0"`. Workspace globs: `libs/typescript/*`, `libs/typescript/ui/*`, `apps/*/*`, `tests/typescript/*`.

## [01]-[SETTINGS_FILE]

### [01.1]-[FILE_SPLIT]

`docs/configuring.md`:

> pnpm settings are divided into two categories: - **Authentication and certificate settings** are stored in INI files. These contain sensitive credentials and should not be committed to your repository. - **All other settings** are stored in YAML files: the project `pnpm-workspace.yaml` and the global `config.yaml`. pnpm also no longer reads settings from the `pnpm` field of `package.json`. Settings should be defined in `pnpm-workspace.yaml`.

`docs/settings.md`: "Only auth and registry settings are read from `.npmrc` files. All other settings (like `hoistPattern`, `nodeLinker`, `shamefullyHoist`, etc.) must be configured in `pnpm-workspace.yaml` or the global `~/.config/pnpm/config.yaml`."

Consequences:
- The `pnpm` field of `package.json` is dead, and the Rasm root `package.json` complies
- `pnpm_config_*` / `PNPM_CONFIG_*` environment variables override `pnpm-workspace.yaml` and yield to CLI arguments, and `npm_config_*` is unread
- Config values interpolate environment variables with `${NAME}`, `${NAME-fallback}`, `${NAME:-fallback}`

### [01.2]-[REFUSED_SETTINGS]

`docs/settings.md`:

> Since v11.5.3, env variables are **not** expanded in settings of `pnpm-workspace.yaml` that define registry URLs: `registry` and the URL values of `registries` and `namedRegistries`. Values containing a `${...}` placeholder in these settings are ignored.

> Since v11.22.0, a project's `pnpm-workspace.yaml` cannot choose where pnpm keeps its credentials, its own installation, or other machine-level state: `bin`, `configDir`, `dir`, `globalBinDir`, `globalDir`, `npmrcAuthFile`, `pnpmHomeDir`, `stateDir`, `userconfig`, and `workspaceDir` are ignored there, with a warning. ... `cacheDir` and `storeDir` are unaffected.

The last sentence licenses the Rasm `cacheDir: .cache/pnpm/cache` and `storeDir: .cache/pnpm/store`.

### [01.3]-[PACKAGES_AND_PACKAGE_CONFIGS]

`docs/settings.md`: "If the `packages` field is omitted, only the root package is included in the workspace. ... The root package is always included, even when custom location wildcards are used." Negated globs are supported.

`packageConfigs`, "Added in: v11.0.0": "Allows setting project-specific configuration for individual workspace packages. This replaces workspace project-specific `.npmrc` files." Both shapes sit in the root file:

```yaml
packageConfigs:
  "project-1":
    saveExact: true
```

```yaml
packageConfigs:
  - match: ["project-1", "project-2"]
    saveExact: true
```

A per-package pnpm setting is expressed once, in the root file. Rasm needs none today.

### [01.4]-[CATALOG_SETTINGS]

`catalog` (singular) defines the catalog named `default`, `catalogs` (plural) defines named catalogs, and `catalog:` is shorthand for `catalog:default` (`docs/catalogs.md`).

### [01.5]-[MINIMUM_RELEASE_AGE]

From `docs/settings/dependency-resolution.md`:

| [SETTING]                            | [ADDED IN]                                                 | [DEFAULT]                               |
| :----------------------------------- | :--------------------------------------------------------- | :-------------------------------------- |
| `minimumReleaseAge`                  | v10.16.0                                                   | 1440 since v11, 0 before                |
| `minimumReleaseAgeExclude`           | v10.16.0 (patterns v10.17.0, versions and `\|\|` v10.19.0) | undefined                               |
| `minimumReleaseAgeExcludePrune`      | v11.22.0                                                   | false                                   |
| `minimumReleaseAgeIgnoreMissingTime` | v11.0.0                                                    | true                                    |
| `minimumReleaseAgeStrict`            | v11.0.0                                                    | true if set explicitly, false otherwise |

> The default depends on whether you configured `minimumReleaseAge` yourself: if you set it explicitly (via `pnpm-workspace.yaml`, the CLI, or environment variables), strict mode is on by default so the setting is enforced. The built-in default of `minimumReleaseAge` (1440 minutes) is non-strict for backward compatibility.

Rasm writes `minimumReleaseAge: 1440`, which equals the v11 default and turns `minimumReleaseAgeStrict` on, and a range with no old-enough version fails resolution. The plan excludes gating configuration and the repository RASM0002 rule rejects restated defaults, the line is removed, leaving the built-in non-strict 1440.

### [01.6]-[REGISTRIES]

`registries` (v11.0.0, reshaped v11.23.0 to be keyed by URL with `scopes`, `prefix`, `serverType`, `supportsTimeField`) and `namedRegistries` (v11.1.0, "Deprecated since v11.23.0: declare a `prefix` in `registries` instead"). Rasm uses one registry and declares neither.

### [01.7]-[PACKAGE_MANAGER_FIELDS]

`package.json` fields, from `docs/package_json.md`:

`engines`: "During local development, pnpm will always fail with an error message if its version does not match the one specified in the `engines` field. Unless the user has set the `engineStrict` config flag ... this field is advisory only" when the package is installed as a dependency. `engines.node: ">=24.15.0"` is checked locally, and `engines` installs nothing.

`engines.runtime` (v10.21.0) declares the runtime a dependency requires. `devEngines.runtime` (v10.14) declares the runtime the project uses, `pnpm install` resolves the range, "The exact version (and checksum) is saved in the lockfile", and scripts run on it. Since v12.0.0-rc.2 an unqualified `node` inside the project follows the pin (`globalShims`, `PNPM_SHIM_BYPASS=1`). In Rasm, mise pins Node, `devEngines.runtime` stays unused (one owner per version), and `engines.node` remains the floor check.

`devEngines.packageManager` (v11.0.0): "Unlike the `packageManager` field, this supports version ranges. The resolved version is stored in `pnpm-lock.yaml` under `packageManagerDependencies` and reused if it still satisfies the range." Rasm uses `onFail: "error"`.

`packageManager` (legacy, one exact version): "When pnpm is declared via the legacy `packageManager` field (not `devEngines.packageManager`), its resolution info is **not** written to `pnpm-lock.yaml` — unless the pinned pnpm version is v12 or newer and `pmOnFail` is not set to `ignore`." Since v12.0.0-rc.6 `devEngines.packageManager` can name `npm`, `yarn`, or `bun`.

The plan settles both fields in the root manifest: `packageManager: "pnpm@12.3.1"` for tools that read only the legacy field and `devEngines.packageManager` `{ "name": "pnpm", "version": "12.3.1", "onFail": "download" }` for pnpm's own enforcement, the shape `pnpm/pnpm` itself uses.

### [01.8]-[ALLOW_BUILDS]

`docs/settings/build.md`: "The following settings have been removed in v11 and replaced by `allowBuilds`: `onlyBuiltDependencies`, `onlyBuiltDependenciesFile`, `neverBuiltDependencies`, `ignoredBuiltDependencies`, and `ignoreDepScripts`." The codemod is `pnpx codemod run pnpm-v10-to-v11`. `allowBuilds` (v10.26.0) is "a map of package matchers to explicitly allow (`true`) or disallow (`false`) script execution". Rasm already uses the shape.

- Version-range keys are matchers: `nx@21.6.4 || 21.6.5: true` is a legal key
- "a package name on its own never approves builds for a git or tarball dependency — the name alone does not identify the artifact"
- Approval for those is by resolved path including the commit, or (v11.11.0) by repository URL
- Since v11.19.0 the repository form covers `github:` tarballs too
- Unlisted packages are unreviewed, and with `strictDepBuilds` (default true) the install exits non-zero with `ERR_PNPM_IGNORED_BUILDS`
- `pnpm add --allow-build` and `pnpm approve-builds` write to `pnpm-workspace.yaml`, a file pnpm edits

"During install, dependencies with ignored builds that are not yet listed in `allowBuilds` are automatically added to `pnpm-workspace.yaml` with a placeholder value".

`dangerouslyAllowAllBuilds` (v10.9.0) runs every install script without review and stays unset.

Related build-group settings: `ignoreScripts` ("This flag does not prevent the execution of `.pnpmfile.mjs`"), `childConcurrency`, `verifyDepsBeforeRun` (default `install`, values `install`, `warn`, `error`, `prompt`, `false`, runs on `pnpm run` and `pnpm exec`), `sideEffectsCache` (boolean or `{read, write, remote}`), and `sideEffectsCache.remote` (v11.25.0 and v12.0.0, requires a `pnpr` server, off unless configured).

### [01.9]-[OVERRIDES]

From `docs/settings/dependency-resolution.md`:

- "Note that the overrides field can only be set at the root of the project."
- Selector forms: unqualified name, `bar@^2.1.0`, parent-scoped `qar@1>zoo`, alias value `npm:@myorg/quux@^1.0.0`, removal `-`
- Overrides can use `catalog:`
- Convergence overrides, `"pkg@": <exact>`, rewrite an edge "only when its version satisfies the range that edge declares"

On `catalog:` in overrides: "To keep an overridden version in sync with the version used elsewhere in your workspace, define the version in a catalog and reference it with the `catalog:` protocol. This way the version is maintained in a single place". A convergence value must be an exact version or a `catalog:` reference resolving to one (`ERR_PNPM_INVALID_CONVERGENCE_OVERRIDE` otherwise), plain semver edges alone participate, no parent selector exists, and a regular override wins over a convergence override on the same edge.

The nine Rasm overrides all hard-pin exact versions. `sharp: 0.35.4` appears in both the catalog and `overrides`, and the override becomes `sharp: "catalog:"`. Each remaining override keeps its one-line reason (two carry one today), and the ones that only converge compatible consumers take the `"pkg@"` form.

### [01.10]-[PATCHED_DEPENDENCIES]

`docs/cli/patch.md`: "This field is added/updated automatically when you run `pnpm patch-commit`." Keys are package names with an exact version, a range, or the name alone, and values are relative paths to patch files. Priority: exact versions, then ranges, then name-only, and the range `*` "behaves like a name-only patch but does not ignore patch failures". Workflow: `pnpm patch <pkg>@<version>` (`--edit-dir`, `--ignore-existing`), then `pnpm patch-commit <path>`. "If you want to change the dependencies of a package, don't use patching to modify the `package.json` file of the package. For overriding dependencies, use overrides or a package hook." `allowUnusedPatches` (v10.7.0, previously `allowNonAppliedPatches`), and in v11 "`ignorePatchFailures` ... has been removed".

Rasm has no patches. Patch files are repository content, conventionally `patches/`.

### [01.11]-[NODE_LINKER]

From `docs/settings/node-modules.md`:

- `nodeLinker` defaults to `isolated`
- `publicHoistPattern` (default `[]`) hoists into the root `node_modules`, and `shamefullyHoist: true` equals `publicHoistPattern: '*'`
- `hoistingLimits` (v11.5.0) applies under `nodeLinker: hoisted` alone
- `virtualStoreDir` defaults to `node_modules/.pnpm`
- `virtualStoreType` (v11.23.0, `project` | `global`) is the spelling of `enableGlobalVirtualStore`
- `packageImportMethod` defaults to `auto`, and Linux since v12.0.0 is hardlink → clone → copy
- Linux on v11 and macOS and Windows at all versions are clone → hardlink → copy
- `virtualStoreOnly` (v11.0.0) populates the virtual store without importer symlinks, and `pnpm fetch` uses it
- `nodeExperimentalPackageMap` (v11.8.0) connects Node's package map

The documented reasons for `hoisted` are tooling that breaks on symlinks (React Native), serverless hosts without symlink support (AWS Lambda), `bundledDependencies`, and `--preserve-symlinks`, and none applies to Rasm. `publicHoistPattern` is "useful when dealing with some flawed pluggable tools that don't resolve dependencies properly". On the virtual store: "the virtual store cannot be shared between several projects. Every project should have its own virtual store (except for in workspaces where the root is shared)." It is no relocation candidate. On `virtualStoreType`: "If pnpm detects that it is running in CI, this setting is automatically disabled." Nothing changes on macOS.

### [01.12]-[STORE_CACHE_AND_LOCKFILE]

`docs/settings/store.md`: `storeDir` resolves `$PNPM_HOME/store`, else `$XDG_DATA_HOME/pnpm/store`, else `~/Library/pnpm/store` on macOS. "It is possible to set a store from a different disk but in that case pnpm will copy packages from the store instead of hard-linking them." When no directory above the project accepts a hard link, the store is created at `node_modules/.pnpm-store` since v12.0.0 and `.pnpm-store` in pnpm 11. `verifyStoreIntegrity` (true), `strictStorePkgContentCheck` (true), `frozenStore` (v11.7.0, read-only store for `--offline --frozen-lockfile`). Lockfile settings: `lockfile`, `preferFrozenLockfile` (default true, "a headless installation is performed"), `lockfileIncludeTarballUrl`, `gitBranchLockfile`, `mergeGitBranchLockfilesBranchPattern`, `peersSuffixMaxLength`.

`cacheDir` (`docs/settings/other.md`) defaults to `$XDG_CACHE_HOME/pnpm`, else `~/Library/Caches/pnpm`, and holds "package metadata, dlx cache, and some install verification results".

The Rasm `.cache/pnpm/*` placement satisfies the README rule that caches route under `.cache/` and is settled, and the cost is that store and cache are unshared with other repositories on the machine.

### [01.13]-[OTHER_SETTINGS]

- `savePrefix` (`'^'`, `'~'`, `''`, `'='` since v11.19.0), and Rasm sets `saveExact: true`, the older spelling of the empty prefix
- `ignoreCompatibilityDb`: since v12.0.0 the compatibility database "no longer carries entries derived from static analysis of published packages"
- `resolutionMode` (`highest` default, `time-based`, `lowest-direct`), `extendNodePath` (default true), `dedupeDirectDeps` (default false)
- `optimisticRepeatInstall` (v10.1.0), `requiredScripts` (script names every workspace project must define), `deployAllFiles`
- `ci` (v10.12.1, an explicit CI-detection override), `globalShims` (v12.0.0-rc.6, "only read from locations a project cannot write to")

The compatibility database case named is `@typescript-eslint/types` gaining a `typescript` dependency that put TypeScript 7 under older `@typescript-eslint` versions. The Rasm catalog pins `typescript: 7.0.2`.

### [01.14]-[WORKSPACE_SETTINGS]

From `docs/workspaces.md`:

- `linkWorkspacePackages` (default false, values `true`, `false`, `deep`) links local packages in place of downloading
- `deep` links into subdependencies too, and "Packages are only linked if their versions satisfy the dependency ranges."
- `injectWorkspacePackages` (default false) hard-links in place of symlinks, `dedupeInjectedDeps` (default true)
- `syncInjectedDepsAfterScripts` (v10.5.0) re-synchronizes hard-linked copies after a named script
- `preferWorkspacePackages` (default false): "This setting is only useful if the workspace doesn't use `saveWorkspaceProtocol`."
- `sharedWorkspaceLockfile` (default true), `saveWorkspaceProtocol` (default `rolling`, with `savePrefix: ''` the written spec is `workspace:*`)
- `includeWorkspaceRoot`, `ignoreWorkspaceCycles` ("downgrades an `ERR_PNPM_TASK_CYCLE`")
- `disallowWorkspaceCycles` (installation fails on a cycle, the enforcing counterpart of the README acyclic rule)
- `failIfNoMatch` (non-zero exit when no package matches a filter)

Rasm sets `linkWorkspacePackages: deep`. Rasm sets `preferWorkspacePackages: true` while `saveWorkspaceProtocol` stays at its default `rolling`, which writes `workspace:*` specs, the setting is inert and is removed.

## [02]-[WORKSPACE_PACKAGES]

### [02.1]-[WORKSPACE_PROTOCOL]

`docs/workspaces.md`: "When this protocol is used, pnpm will refuse to resolve to anything other than a local workspace package." Forms: `workspace:*`, `workspace:~`, `workspace:^`, `workspace:<range>`, the alias form `"bar": "workspace:foo@*"` ("Before publish, aliases are converted to regular aliased dependencies"), and the relative-path form `"foo": "workspace:../foo"`. On pack or publish the protocol is replaced by the resolved range.

With `linkWorkspacePackages: deep`, ranges alone link, and `workspace:*` turns a silent registry fetch into an install failure with no version maintenance, the form the private Rasm `libs/typescript/*` packages use.

### [02.2]-[RECURSIVE_AND_FILTER]

`docs/cli/recursive.md`: `-r` covers `install`, `list`, `outdated`, `publish`, `pack`, `rebuild`, `remove`, `unlink`, `update`, `why` on every project, and `exec`, `run`, `test`, `add` on every project "excluding the root project" (`includeWorkspaceRoot: true` puts it back). Options: `--workspace-concurrency` (default 4, `<= 0` means `max(1, cores - abs(n))`), `--[no-]bail` ("This config does not affect the exit code"), `--[no-]sort`, `--reverse`, `--filter`. The doc discourages the CLI form of `--link-workspace-packages`: "it is encouraged instead to use `pnpm-workspace.yaml` for this setting, to enforce the same behaviour in all environments."

Filter selectors (`docs/filtering.md`): `foo`, `@scope/*`, `foo...` (with dependencies), `foo^...` (dependencies only), `...foo` (with dependents), `...^foo`, `./<glob>` or `{<glob>}`, `[<since>]`, `!foo`. `legacyDirFiltering` (default false): "`{<dir>}` is matched as a glob pattern ... pnpm 11 reads this setting; the Rust CLI recognized but ignored it until v12.0.0."

Nx owns change detection and the task graph, `--filter "...[origin/main]"` gives a second answer to what changed, and `--filter` stays confined to pnpm's own commands (`install`, `update`, `why`).

### [02.3]-[DEPLOY]

`docs/cli/deploy.md`: copies the deployed package's files and installs all dependencies, including workspace ones, into an isolated `node_modules` so "The target directory will contain a portable package". "By default, the deploy command only works with workspaces that have the `inject-workspace-packages` setting set to `true`", otherwise `--legacy` or `force-legacy-deploy`. With the global virtual store "pnpm deploy ignores it and always creates a localized virtual store". File selection: `files`, else `.npmignore`, else `.gitignore`, and `deployAllFiles: true` copies everything. Its target directory is build output under `.artifacts/`.

### [02.4]-[MANIFEST_WITHOUT_SCRIPTS]

1. A directory is a workspace project when it has a manifest (`package.json`, `package.json5`, or `package.yaml`)
2. A package with no `scripts` is fine: `pnpm -r run` skips projects lacking the script, and `requiredScripts` is unset
3. `tests/typescript/support/package.json` names the directory `@rasm/test-support` and declares `exports` and `dependencies`

Its `exports` are `./arbitraries`, `./bench`, `./properties`, `./resources`, `./setup`, `./telemetry`. Its `dependencies` use `catalog:` specifiers (`@effect/platform`, `@effect/platform-node`, `@effect/vitest`, `@electric-sql/pglite`, `effect`, `vitest`), which installs them into that package's own `node_modules` under the isolated linker. It declares `"private": true`, no `version`, no `scripts`, no pnpm settings, which is correct.

The rule: a `package.json` in a workspace directory declares identity, exports, and dependencies, and configuration stays out. Package-specific pnpm configuration, if any, goes under `packageConfigs` in the root file.

## [03]-[CATALOGS_AND_LOCKFILE]

### [03.1]-[CATALOG_PLACEMENT]

`docs/catalogs.md`: `package.json` `dependencies`, `devDependencies`, `peerDependencies`, `optionalDependencies`, and `pnpm-workspace.yaml` `overrides`. Since v11.14.0 a `peerDependencies` value can carry a scheme (`work:5.x.x`, `npm:other-lib@^5`). "The `catalog:` protocol is removed when running `pnpm publish` or `pnpm pack`." Named catalogs exist "in a large multi-package repo that's migrating to a newer version of a dependency piecemeal", and the Rasm README fixes one catalog that holds every version.

### [03.2]-[CATALOG_MODE_AND_PRUNE]

`catalogMode` (`docs/settings/_catalogMode.mdx`, v10.12.1, default `manual`, `strict` "only allows dependency versions from the catalog", `prefer` falls back). It "Controls if and how dependencies are added to the default catalog, when running `pnpm add`", governing the `add` path and no lint over existing manifests. Rasm sets `strict`.

`catalogPrune` (v11.22.0, previously `cleanupUnusedCatalogs` since v10.15.0): "When set to `true`, pnpm will remove unused catalog entries during installation." Because the root `package.json` references all 279 entries, nothing is unused today, and the setting matters the moment a name leaves the root list.

`pnpm outdated` and `pnpm update` see only catalog entries some package references. The root list makes all 279 visible to the update path, and removing it without a replacement freezes the unreferenced entries.

### [03.3]-[UPDATE]

`docs/cli/update.md`: "A dependency declared through the `catalog:` protocol is not rewritten in `package.json`. The catalog entry it points at is updated instead, in `pnpm-workspace.yaml`." That makes "every version expressed once" hold under `pnpm update`.

| [COMMAND OR FLAG]                     | [EFFECT]                                                  |
| :------------------------------------ | :-------------------------------------------------------- |
| `pnpm up`                             | Within declared ranges                                    |
| `pnpm up --latest` / `-L`             | To `latest`, crossing majors                              |
| `pnpm up foo@2`                       | Latest v2                                                 |
| `--recursive`                         | Every project                                             |
| `--workspace`                         | Workspace links                                           |
| `--interactive`                       | Choose from the outdated list                             |
| `--no-save`                           | Lockfile only                                             |
| `--changeset` (v11.16.0)              | Writes a changesets file                                  |
| `--include-github-actions` (v11.16.0) | Updates action refs in workflow files too                 |
| `--patches` (v11.25.0 / v12.0.0)      | Refreshes registry revisions without changing any version |

Since v11.23.0, `pnpm update <name>@<version>` fails with `ERR_PNPM_UPDATE_VERSION_ON_INDIRECT_DEP` when the package is not a direct dependency ("There is nowhere to record the version in that case; pin a transitive dependency through `overrides` instead"). `update.ignoreDeps` (v11.16.0, formerly `updateConfig.ignoreDependencies`) suppresses a dependency from `pnpm update --latest` and `pnpm outdated`, patterns allowed. Rasm records its deliberate holds there: `react` and `react-dom` at `19.3.0-canary-711c445b-20260722`, `@duckdb/duckdb-wasm: 1.33.1-dev61.0`, `@duckdb/node-api: 1.5.5-r.4`, each with the one-line reason `CLAUDE.md` allows at the catalog entry (today only `elkjs` carries one).

### [03.4]-[OUTDATED_DEDUPE_WHY]

`pnpm outdated` (`docs/cli/outdated.md`): `--long`, `--format`, `--compatible`, `--sort-by`, `--include-github-actions`. `--format json` is the form an Nx target consumes, and a catalog-wide report comes from one run at the root.

`pnpm dedupe --check` (`docs/cli/dedupe.md`) "Check[s] if running dedupe would result in changes without installing packages or editing the lockfile. Exits with a non-zero status code if changes are possible."

`pnpm why <pkg>` (`docs/cli/why.md`): "The output is a reverse dependency tree ... Duplicate subtrees are deduplicated in the output and shown as 'deduped'." `--only-projects` answers which workspace packages pull a package in.

### [03.5]-[FROZEN_INSTALLS]

`docs/cli/install.md`, `--frozen-lockfile`: "Default: For non-CI: false; For CI: true, if a lockfile is present". CI detection uses `ci-info`. `docs/continuous-integration.md`: "When pnpm detects that it is running in CI, it switches to frozen-lockfile mode automatically. Since v11, pnpm also fails on incompatible lockfiles in CI — if the lockfile was written by a newer pnpm major version, the install will error out instead of silently rewriting it." That is the cost of a version skew, and the reason the pnpm pin applies everywhere.

Since v11.4.0 an integrity mismatch is a hard failure, `ERR_PNPM_TARBALL_INTEGRITY`, `--update-checksums` is the narrow opt-in, and `--force` and `pnpm update` do not bypass it.

Other install flags: `--offline` (store only), `--prefer-offline` ("staleness checks for cached data will be bypassed, but missing data will be requested from the server"), `--lockfile-only`, `--dry-run` (v11.8.0), `--no-runtime` (skips the `devEngines.runtime` fetch). `--resolution-only` exists in pnpm 11 alone, pnpm 12 rejects it, and `pnpm peers check` lists peer issues on either.

## [04]-[NODE_AND_TOOL_PINNING]

### [04.1]-[MECHANISMS]

| [MECHANISM]                 | [FILE]         | [PINS]                           | [ENFORCEMENT]                                                 |
| :-------------------------- | :------------- | :------------------------------- | :------------------------------------------------------------ |
| `engines`                   | `package.json` | Node and pnpm ranges             | Fails locally on mismatch                                     |
| `devEngines.runtime`        | `package.json` | The project's Node, Deno, or Bun | Exact version and checksum in `pnpm-lock.yaml`                |
| `devEngines.packageManager` | `package.json` | pnpm by range or exact           | Recorded under `packageManagerDependencies`, `onFail` decides |
| `packageManager`            | `package.json` | One exact version                | Read by Corepack, CI actions, editors                         |

### [04.2]-[COREPACK]

`docs/continuous-integration.md`: "Earlier versions of this page used Corepack. Corepack installs a JavaScript shim in place of pnpm, so every `pnpm` call starts Node.js to run the shim before pnpm itself starts". The Node distribution policy (`doc/contributing/distribution.md`, commit `67fbf4d371be9bc521e3daa7b2d69ddf498411b4`, 2026-01-01, "doc: note corepack package removal in distribution doc", PR #61207): "corepack was added in Node.js v14.9.0 and v16.9.0. It is no longer distributed as of Node.js v25.0.0." Corepack remains on npm (nodejs/corepack#687 tracks its 1.0.0 release).

`engines.node: ">=24.15.0"` accepts Node 25 and 26, where no bundled `corepack` exists, and the `packageManager` field cannot bootstrap pnpm through Corepack. In Rasm, mise installs pnpm, and the standalone script (`curl -fsSL https://get.pnpm.io/install.sh | sh -`) and `pnpm/setup` remain the documented alternatives.

### [04.3]-[PNPM_11_VERSUS_12]

`docs/installation.md`: "pnpm 12, the Rust rewrite, is stable since v12.0.0 (August 26, 2026). ... Only installation differs: `latest` on npm still points at the pnpm 11 line, so pnpm 12 is installed from the `next-12` tag, and Homebrew, winget, Scoop and Chocolatey don't offer it yet." Ways in: `pnpm self-update next-12`, `npx get-pnpm next-12`, `PNPM_VERSION=next-12` with the script. The mise `aqua:pnpm/pnpm` backend installs the release binary by version, and `pnpm = "12.3.1"` in `mise.toml` needs none of these.

"On pnpm 11, the standalone script does not run on Intel Macs (`darwin-x64`) ... pnpm 12 ships an Intel macOS build again". pnpm 12 is statically linked, and pnpm 11 needs `libatomic1` and `libstdc++6` on minimal Linux images.

### [04.4]-[RUNTIME_COMMAND]

`docs/cli/env.md`: "`pnpm env` is deprecated. Use `pnpm runtime` instead." `pnpm runtime` (`docs/cli/runtime.md`) manages `node`, `deno`, `bun`. Since v11.0.0 a runtime install "does not extract the bundled `npm`, `npx`, and `corepack`", and since v11.22.0 a pinned runtime with metadata fetched once "resolves without any network access". Rasm leaves it unused, mise owns Node.

### [04.5]-[PNX_AND_EXEC]

`docs/cli/pnx.md`: the command is `pnx`, aliases `pnpm dlx` and `pnpx`. `pnx shx@catalog:` is supported ("allowing you to use versions defined in your workspace catalogs"), the one form of `dlx` that keeps the version in the catalog. Since v12.0.0-rc.6, `pnx yarn@4`, `pnx node@22 --version` provision the real tool. `pnpm exec` (`docs/cli/exec.md`): "`node_modules/.bin` is added to the `PATH`", the `exec` keyword is optional when the command does not collide with a builtin, and "Any options for the `exec` command should be listed before the `exec` keyword."

### [04.6]-[TOOL_PINS]

A tool the workspace runs on every build is a devDependency at a catalog version, and `pnx` is for one-shot tools outside the build. Current state:

| [TOOL]      | [CATALOG ENTRY]                              | [ROOT DEVDEPENDENCY] | [NOTE]                                                       |
| :---------- | :------------------------------------------- | :------------------- | :----------------------------------------------------------- |
| Biome       | `@biomejs/biome: 2.5.11` (2.5.12 is current) | Yes                  | `allowBuilds: true`, postinstall fetches the platform binary |
| TypeScript  | `typescript: 7.0.2`                          | Yes                  | Under `peerDependencyRules.allowedVersions` too              |
| ast-grep    | `@ast-grep/cli: 0.45.3`                      | Yes                  | `allowBuilds: true`                                          |
| mermaid-cli | `@mermaid-js/mermaid-cli: 11.16.0`           | Yes                  | Pulls `puppeteer`, `allowBuilds` sets `puppeteer: false`     |

With `puppeteer: false`, the install downloads no Chromium. Every Node tool stays in the catalog, and mise never names one.

## [05]-[TYPESCRIPT_PROJECT_REFERENCES]

### [05.1]-[PNPM_RULES]

`docs/typescript.md` has two rules: "You should not use TypeScript with `preserveSymlinks` set to `true`", and the `@types/` hazard when "a package requires these types without having the type dependency in dependencies", fixed with `packageExtensions` in `pnpm-workspace.yaml`. The Rasm `tsconfig.base.json` leaves `preserveSymlinks` at its default. The catalog carries `@types/react`, `@types/react-dom`, `@types/three`, `@types/node`, `@types/geojson`, `@types/nodemailer`, `@types/papaparse`, and `packageExtensions` is the root-owned fix when a package imports types it does not declare.

### [05.2]-[COMPILER]

npm registry, read 2026-09-03: `typescript` `dist-tags.latest` is 7.0.2, `next` is `7.1.0-dev.20260903.1`, `rc` is `7.0.1-rc`, `beta` is `6.0.0-beta`. The 7.0 beta announcement (<https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/>) names the RC on June 18 and GA on July 8, 2026, and the native port's codename Corsa. The Rasm `typescript: 7.0.2` is the newest stable and is the native compiler.

### [05.3]-[TSGO]

`microsoft/typescript-go` is archived (`archived: true`), its README reading "This was the staging repo for the TypeScript 7.0 release during the native port process, which is now completed!", "This repo will be permanently archived in September 2026.", and "For TypeScript 7.0 RC and later, the command name is `tsc`." Its feature table at archive time: declaration emit done, build mode and project references done, incremental build done, language service in progress, API not ready. `@typescript/native-preview` `dist-tags.latest` is `7.0.0-dev.20260707.2`, a nightly from the day before GA.

Nothing remains to adopt: `typescript@7.0.2` with `tsc` is the Go compiler, as the plan states. The beta announcement adds `--builders` beside `--checkers` and warns "building with `--checkers 4 --builders 4` allows up to 16 type-checkers to run at once, which may be excessive", which interacts with Nx's own parallelism and is measured rather than assumed.

### [05.4]-[LAYOUT]

- `tsconfig.base.json` sets `composite: true`, `declaration: true`, `declarationMap: true`, `outDir: "${configDir}/dist"`, `rootDir: "${configDir}"`
- It sets `tsBuildInfoFile: "${configDir}/dist/tsconfig.tsbuildinfo"` and `include: ["${configDir}/*.ts"]`
- Root `tsconfig.json` overrides with `outDir: ".cache/typescript/out/root"` and `tsBuildInfoFile: ".cache/typescript/root.tsbuildinfo"`
- Root `tsconfig.json` sets `emitDeclarationOnly: true`
- The `tests/typescript/support` and `tools/nx` tsconfigs override with `../../../.cache/typescript/...` and `../../.cache/typescript/...`
- Root `references`: `./tests/typescript/support` and `./tools/nx`, with the comment "One reference per workspace package tsconfig"
- `nx.json` `targetDefaults.typecheck` runs `tsc --build --pretty false` with `cwd: {projectRoot}`
- The same default sets `dependsOn: ["^typecheck"]` and `outputs: ["{projectRoot}/dist"]`

Observations:

1. The `${configDir}` base is inert: every leaf overrides `outDir` and `tsBuildInfoFile` with a relative path with a depth that depends on where the package sits, the one per-package duplication left in the TypeScript layer.
2. `typecheck` declares `outputs: ["{projectRoot}/dist"]` and no project emits there, Nx caches an empty directory. The Nx report carries the defect, and the emit location is the kept question there.
3. `tools/nx` is a TypeScript project and no pnpm workspace package: it has a `tsconfig.json` and a root reference and no `package.json`, and the `packages` globs do not cover `tools/*`. Its `@nx/devkit`, `effect`, `@effect/platform`, `@effect/platform-node`, and `fast-xml-parser` imports (`native-packaging.ts:5-20`) resolve through the root package's own `node_modules` alone. `apps/README.md` fixes the shape ("TypeScript projects are created by hand as `package.json` beside `tsconfig.json`"), `tools/nx` gains a `package.json` declaring those five as `catalog:` dependencies, and `tools/*` joins the workspace globs.

### [05.5]-[TYPESCRIPT_SYNC]

`nx.json` lists `@nx/js:typescript-sync` under `disabledTaskSyncGenerators`, and `@nx/js/typescript` is absent from `plugins`, the generator is never registered and the entry is inert. The generator (<https://github.com/nrwl/nx/blob/main/packages/js/src/generators/typescript-sync/typescript-sync.ts>, <https://nx.dev/docs/concepts/sync-generators>) reconciles the root `references` and each composite project's `references` against the project graph. Whether the plugin is registered so the generator owns the references is the question kept in the Nx report, and the inert entry is removed either way.

### [05.6]-[COMPOSITE_BUILD]

`composite: true` makes every project referenceable and is required for `tsc --build`. `declaration` and `declarationMap` with `emitDeclarationOnly: true` mean `tsc --build` produces `.d.ts` and `.d.ts.map` alone, and Vite owns JS emit. `.d.ts` files another project's typecheck consumes are inputs to a later build rather than disposable cache, and keeping the `.tsbuildinfo` and the `outDir` in the same tree keeps a wholesale clear safe. With `nodeLinker: isolated` and no `preserveSymlinks`, TypeScript follows the symlinks into `node_modules/.pnpm` and resolves each package's real path.

## [06]-[CACHES_AND_STORES]

### [06.1]-[DIRECTORIES]

| [DIRECTORY]   | [SETTING]         | [DEFAULT ON MACOS]                                   | [HOLDS]                                           |
| :------------ | :---------------- | :--------------------------------------------------- | :------------------------------------------------ |
| Store         | `storeDir`        | `$PNPM_HOME/store`, else `~/Library/pnpm/store`      | Content-addressable package files, `index.db`     |
| Cache         | `cacheDir`        | `$XDG_CACHE_HOME/pnpm`, else `~/Library/Caches/pnpm` | Package metadata, dlx cache, verification results |
| Virtual store | `virtualStoreDir` | `node_modules/.pnpm`                                 | Per-project links into the store, unshareable     |

Rasm sets `storeDir: .cache/pnpm/store` and `cacheDir: .cache/pnpm/cache` and leaves `virtualStoreDir` alone.

### [06.2]-[STORE]

`docs/cli/store.md`: `pnpm store status` "Returns exit code 0 if the content of the package is the same as it was at the time of unpacking", `pnpm store prune` "Removes _unreferenced packages_ from the store ... is not harmful and has no side effects on your projects", and "It is best practice to run `pnpm store prune` occasionally to clean up the store, but not too frequently." With the global virtual store enabled, prune garbage-collects `links/` too, and projects are registered under `{storeDir}/v11/projects/`. Because the Rasm store sits inside the repository, "unreferenced" collapses to "unreferenced by Rasm", and a prune discards the branch-switching benefit the doc describes. `pnpm cache path` (v11.22.0, `docs/cli/cache-path.md`) prints the metadata cache directory.

### [06.3]-[CI_CACHING]

`docs/continuous-integration.md`: "In all the provided configuration files the store is cached. However, this is not required, and it is not guaranteed that caching the store will make installation faster." Since v11.22.0, `pnpm cache path` prints the directory that "also holds the lockfile verification log, which lets a job skip re-checking an unchanged lockfile ... the dominant cost of an install in CI once the store is warm." Caching `cacheDir` matters more than caching `storeDir`. Because both sit under `.cache/pnpm/` inside the repository, one `actions/cache` key over `.cache/pnpm/**` covers them.

GitHub Actions uses `pnpm/setup` (v2.0.0): "installs pnpm, then uses it to install the requested runtime, so no separate `actions/setup-node` step is needed. It also runs `pnpm install` for you, and `cache: true` caches the pnpm store between runs. ... The pnpm version comes from the `packageManager` or `devEngines.packageManager` field of your `package.json`". In Rasm, `jdx/mise-action@v4` installs pnpm from `mise.toml`, `pnpm install --frozen-lockfile` runs as a step, and `pnpm/setup` stays out.

### [06.4]-[INSTALL_FLAGS]

`--frozen-lockfile` is already the default under CI detection, and writing it covers the case where detection fails. `--prefer-offline` trusts the cache but can reach the network, `--offline` cannot.

### [06.5]-[FETCH]

`docs/cli/fetch.md`: "Fetch packages from a lockfile into virtual store, package manifest is ignored. ... This command is specifically designed to improve building a docker image." A Dockerfile that copies each package's `package.json` "has to be updated when you add or remove sub-packages", and `pnpm fetch` needs the lockfile and `pnpm-workspace.yaml` alone:

```Dockerfile
COPY pnpm-lock.yaml pnpm-workspace.yaml ./
RUN pnpm fetch --prod
COPY . ./
RUN pnpm install -r --offline --prod
```

"Local `file:` protocol dependencies are skipped during `pnpm fetch`". `frozenStore` (v11.7.0) is the read-only-store counterpart: `pnpm install --frozen-store --offline --frozen-lockfile`.

## [07]-[SCRIPTS]

### [07.1]-[RUN]

`docs/cli/run.md`: `node_modules/.bin` and `<workspace root>/node_modules/.bin` are on `PATH` for scripts, regex selection exists (`pnpm run "/^watch:.*/"`, "Matching is not anchored", flags rejected with `ERR_PNPM_UNSUPPORTED_SCRIPT_COMMAND_FORMAT`), `-r` skips projects lacking the script, `--sequential`/`-s` (v11.14.0, "For `pnpm run`, `-s` is the shorthand for `--sequential`"), `--report-summary` (writes `pnpm-exec-summary.json`), `--resume-from`. Lifecycle scripts see `npm_package_name`, `npm_lifecycle_event`, and "Since v11, pnpm no longer populates `npm_config_*` environment variables from the pnpm configuration." Rasm forgoes all of it.

### [07.2]-[TASKS]

`docs/workspace-task-orchestration.md`: "`pnpm -r run <script>` schedules a graph of workspace tasks. A task is a script in one workspace project, identified as `<project>#<script>`." Declared in `pnpm-workspace.yaml`:

```yaml
tasks:
  build:
    dependsOn:
      - ^build
```

An unconfigured task behaves as `dependsOn: ['^build']`, "Once a task has an entry under `tasks`, an omitted `dependsOn` is the same as `dependsOn: []`", `dependsOn` widens no selection, a project lacking the script is a pass-through, `concurrency` per task is separate from `--workspace-concurrency`, `--dry-run` and `--dry-run --json` print the graph, a cycle fails with `ERR_PNPM_TASK_CYCLE`, `--resume-from` uses a persisted record, with `--bail` pnpm cancels running tasks after the first failure, and recursive `exec`, `--no-sort`, and `--parallel` ignore `tasks`. "Workspace install, rebuild, pack, publish, stage, and lifecycle work uses the same ready-queue scheduling principle" over the package graph, outside `tasks`.

`tasks` is `dependsOn` in a second file with no caching, inputs, or outputs. Nx owns the task graph, Rasm declares no `tasks` and no `scripts`, and `pnpm install`, `rebuild`, and lifecycle work still schedule against the package graph with no configuration.

### [07.3]-[ROOT_SCRIPTS]

The Rasm root `package.json` has no `scripts` field. Nothing in pnpm requires one (`requiredScripts` unset), `pnpm exec <tool>` puts `node_modules/.bin` on `PATH` without a script, and a script is a second name for an Nx target. `verifyDepsBeforeRun` fires on `pnpm run` and `pnpm exec` alone, and `nx:run-commands` spawns the command directly with `node_modules/.bin` on `PATH` (`node_modules/nx/dist/src/executors/run-commands/running-tasks.js:500-514`), the check never runs inside an Nx task and the default `install` value is inert here.

v11 changes (`docs/scripts.md`): "The following built-in commands prefer user scripts: `clean`, `setup`, `deploy`, and `rebuild`" (force the builtin with `pnpm pm <name>`), and scripts starting with `.` are hidden. Having no scripts avoids the collision.

### [07.4]-[EXEC_VERSUS_PNX]

| [FORM]              | [RESOLVES THE TOOL FROM]                  | [VERSION SOURCE]                 | [RIGHT FOR]                      |
| :------------------ | :---------------------------------------- | :------------------------------- | :------------------------------- |
| `pnpm exec <tool>`  | `node_modules/.bin`, including the root's | `pnpm-lock.yaml` via the catalog | Any tool the workspace declares  |
| `pnx <tool>`        | A throwaway install in the dlx cache      | The registry, unless `@catalog:` | One-shot tools outside the build |
| Nx target `command` | `PATH` with `node_modules/.bin` prefixed  | The catalog, for a devDependency | Every repeatable action          |

### [07.5]-[LIFECYCLE]

Dependency install scripts are governed by `allowBuilds` with `strictDepBuilds: true`, everything unlisted fails the install with `ERR_PNPM_IGNORED_BUILDS` rather than running silently, and no `ignoreScripts` is needed. `enablePrePostScripts` is moot without `scripts`. `pnpm:devPreinstall` (`docs/scripts.md`) fires from the root package on local `pnpm install` alone, and Rasm leaves it unused.

## [08]-[SAMPLED_MONOREPOS]

All seven re-read at their default branches on 2026-09-03.

### [08.1]-[PNPM]

`pnpm-workspace.yaml`: `catalogMode: strict`, `catalogPrune: true`, `enablePrePostScripts: false`, `minimumReleaseAge: 1440 # At least a day`, `minimumReleaseAgeExcludePrune: true`, `nodeVersion: 22.13.0`, a 241-entry catalog, and `versioning` settings. Every held-back version carries a one-line reason at the entry (`bin-links: ^6.0.2` because "bin-links 7.x requires Node.js ^22.22.2 || ^24.15.0 || >=26.0.0, above pnpm's supported floor", `typescript: 6.0.3` kept because typescript-eslint lacks TypeScript 7 API support). `overrides` reference the catalog (`hosted-git-info@1: 'catalog:'`, `js-yaml@^4.0.0: 'catalog:'`). `package.json`: `"packageManager": "pnpm@12.3.1"`, `devEngines.packageManager` `{ "name": "pnpm", "version": "12.3.1", "onFail": "download" }`, `devEngines.runtime` `{ "name": "node", "version": "26.8.1", "onFail": "download" }`, 31 scripts, 24 devDependencies of which 21 are `catalog:`.

### [08.2]-[VUE]

`vuejs/core` has an eight-entry catalog (`@babel/parser`, `@babel/types`, `entities`, `estree-walker`, `magic-string`, `source-map-js`, `vite`, `@vitejs/plugin-vue`), every one used by more than one workspace package. Root `package.json`: 52 devDependencies, 4 `catalog:`, `"packageManager": "pnpm@11.19.0"`, `engines.node >=20.0.0`, no `devEngines`, `"preinstall": "npx only-allow pnpm"`, 40 scripts.

### [08.3]-[VITE]

`vitejs/vite` has no catalog. `overrides: { vite: 'workspace:*', debug: 'npm:obug@^1.0.2' }`, `hoistPattern` with a per-entry reason (`postcss # package/vite`, `pug # playground/tailwind`, `eslint-import-resolver-* # eslint-plugin-import-x`), `minimumReleaseAgeExcludePrune: true` with a two-entry exclusion list (`rolldown`, `@rolldown/binding-*`). `package.json`: `"packageManager": "pnpm@12.2.1"`, `engines.node "^20.19.0 || >=22.12.0"`, 21 scripts, 31 devDependencies, 0 `catalog:`.

### [08.4]-[VITEST]

`vitest-dev/vitest` has a catalog of 54 entries, `catalogMode: prefer`, `cleanupUnusedCatalogs: true` (the deprecated spelling). Overrides reference the catalog (`'@types/node': 'catalog:'`, `rollup: 'catalog:'`, `vite: 'catalog:'`) and the workspace (`vitest: workspace:*`). `package.json`: `"packageManager": "pnpm@11.24.0"`, `engines.node "^22.12.0 || ^24.0.0 || >=26.0.0"`, 31 scripts including `"test:ci": "CI=true pnpm -r --reporter-hide-prefix --stream --sequential --filter '@vitest/test-*' --filter !test-browser run test"`, 31 devDependencies, 8 `catalog:`.

### [08.5]-[NUXT]

`nuxt/nuxt` has seven named catalogs split by where the dependency runs (`app-runtime`, `nitro-runtime`, `vue`, `vite`, `webpack`, `build`, `dev`), each with a header comment naming the importing code, and entries carry reasons (`ofetch: 2.0.0-alpha.3 # intentionally pinned to match nitro's dependency`). It sets `catalogPrune: true`, `minimumReleaseAgeExcludePrune: true`, `minimumReleaseAge: 1440`, `verifyDepsBeforeRun: install`, `ignoreWorkspaceCycles: true`, `publicHoistPattern: ['*-loader', 'webpack-*']`. `package.json`: `"packageManager": "pnpm@11.24.0"`, 37 scripts, 67 devDependencies, 63 `catalog:`.

### [08.6]-[ASTRO]

`withastro/astro` has no catalog. `preferWorkspacePackages: true`, `linkWorkspacePackages: true`, `saveWorkspaceProtocol: false # This prevents the examples to have the `workspace:` prefix`, `minimumReleaseAge: 4320`. The documented pairing holds: `preferWorkspacePackages` is useful only without `saveWorkspaceProtocol`, and astro turns it off for the reason it names. `allowBuilds` is a deny-list of ten packages, all `false`. The `'@types/node@22': '^22.19.0'` override carries a five-line comment explaining deduplication. `package.json`: `"packageManager": "pnpm@11.13.1"`, `engines.node >=22.12.0`, 44 scripts, 22 devDependencies, 0 `catalog:`, with a vestigial npm-style `workspaces` array.

### [08.7]-[NX]

`nrwl/nx` has a default catalog with eleven named catalogs (`angular`, `angular-supported-versions`, `css`, `eslint`, `jest`, `react`, `rspack`, `typescript`, `swc`, `tailwind`, `vite`). The line most useful to Rasm:

```yaml
# pnpm 11 defaults this to "install", so `pnpm run <script>` silently installs -
# preinstall/postinstall included - when it thinks node_modules drifted. Nx runs
# script targets through `pnpm run`, so that lands inside unrelated tasks.
verifyDepsBeforeRun: warn
```

That applies to script targets (`nx:run-script`), and the Rasm targets are `command` strings, which never pass through `pnpm run`. Two defects in the same file: `onlyBuiltDependencies: ['@nestjs/core', 'nx']` still declared beside `allowBuilds` although pnpm v11 removed it, and `minimatch` pinned in both `overrides` (`'^10.2.5'`) and the catalog (`'10.2.5'`). `package.json`: `"packageManager": "pnpm@11.22.0"`, 25 scripts, 282 devDependencies, 90 `catalog:`.

### [08.8]-[AGREEMENT]

| [PRACTICE]                  | [VITE]        | [VUE]          | [PNPM]        | [VITEST]       | [NUXT]         | [ASTRO]        | [NX]           |
| :-------------------------- | :------------ | :------------- | :------------ | :------------- | :------------- | :------------- | :------------- |
| Catalog in use              | No            | Yes (8)        | Yes (241)     | Yes (54)       | Yes, named x7  | No             | Yes, named x11 |
| Catalog pruning             | No            | No             | Yes           | Yes            | Yes            | No             | No             |
| `minimumReleaseAge` set     | No            | No             | 1440          | No             | 1440           | 4320           | 1440           |
| Release-age exclude prune   | Yes           | No             | Yes           | No             | Yes            | No             | No             |
| `packageManager` pin        | `pnpm@12.2.1` | `pnpm@11.19.0` | `pnpm@12.3.1` | `pnpm@11.24.0` | `pnpm@11.24.0` | `pnpm@11.13.1` | `pnpm@11.22.0` |
| `devEngines.packageManager` | No            | No             | Yes           | No             | No             | No             | No             |
| `devEngines.runtime`        | No            | No             | Yes           | No             | No             | No             | No             |
| Root `scripts`              | 21            | 40             | 31            | 31             | 37             | 44             | 25             |
| Root `devDependencies`      | 31            | 52             | 24            | 31             | 67             | 22             | 282            |
| Of those, `catalog:`        | 0             | 4              | 21            | 8              | 63             | 0              | 90             |

Catalog pruning is `catalogPrune` or `cleanupUnusedCatalogs`, and release-age exclude prune is `minimumReleaseAgeExcludePrune`. Rasm is 279 devDependencies, 279 `catalog:`, a ratio no sampled repository approaches, and nuxt is nearest at 63/67. Two of the seven run pnpm 12 (vite at 12.2.1, pnpm at 12.3.1).

Conclusions:

1. Pruning tracks catalog size: the three largest catalogs prune (pnpm, nuxt, vitest), the eight-entry vuejs/core catalog does not, and neither does nrwl/nx, with a dead `onlyBuiltDependencies` key in the same file
2. Every catalog entry that deviates from newest stable carries a reason at the entry (pnpm, nuxt)
3. No sampled repository routes every root devDependency through the catalog, and vuejs/core states the rule most clearly, eight catalog entries all shared across workspace packages
4. All seven pin the package manager through `packageManager`, pnpm's own repository alone adds `devEngines.packageManager` and keeps both, and Rasm adopts both
5. Every sampled repository has root `scripts`, and Rasm has none because Nx owns every action

The Rasm catalog has 11 comments, 10 of them group headers and one a reason (`elkjs`), and the React canary and the two DuckDB prereleases are unexplained and become `update.ignoreDeps` entries with reasons.

## [09]-[WARNINGS]

### [09.1]-[WRONG_FILE]

Mistake: pnpm settings in `package.json#pnpm`, non-auth settings in `.npmrc`, per-package `.npmrc` files. Correct form: every non-auth setting in the root `pnpm-workspace.yaml`, per-package settings under `packageConfigs`, credentials in `.npmrc`. `docs/migration.md`: "In v11, pnpm no longer reads configuration from the `pnpm` field in `package.json`. ... Per-subproject `.npmrc` files land under `packageConfigs["<project-name>"]`." Rasm complies.

### [09.2]-[MACHINE_STATE]

Mistake: `bin`, `configDir`, `dir`, `globalBinDir`, `globalDir`, `npmrcAuthFile`, `pnpmHomeDir`, `stateDir`, `userconfig`, `workspaceDir`, or `globalShims` in a project's `pnpm-workspace.yaml`. Correct form: the global config file, a flag, or an environment variable (`docs/settings.md`, "Since v11.22.0 ... ignored there, with a warning").

### [09.3]-[REGISTRY_ENV_VARS]

Mistake: `registry: ${MY_REGISTRY}` in `pnpm-workspace.yaml`, the value is ignored. Correct form: the global config file or a CLI option.

### [09.4]-[REMOVED_SETTINGS]

| [REMOVED OR RENAMED]                                  | [REPLACEMENT]                       | [SOURCE]                                 |
| :---------------------------------------------------- | :---------------------------------- | :--------------------------------------- |
| `onlyBuiltDependencies`, `onlyBuiltDependenciesFile`  | `allowBuilds`                       | `docs/settings/build.md`                 |
| `neverBuiltDependencies`, `ignoredBuiltDependencies`  | `allowBuilds`                       | `docs/settings/build.md`                 |
| `ignoreDepScripts`                                    | `allowBuilds`                       | `docs/settings/build.md`                 |
| `managePackageManagerVersions`                        | `pmOnFail`                          | `docs/migration.md`                      |
| `packageManagerStrict`, `packageManagerStrictVersion` | `pmOnFail`                          | `docs/migration.md`                      |
| `ignorePatchFailures`                                 | Removed, patches always throw       | `docs/cli/patch.md`                      |
| `allowNonAppliedPatches`                              | `allowUnusedPatches`                | `docs/cli/patch.md`                      |
| `updateConfig.ignoreDependencies`                     | `update.ignoreDeps`                 | `docs/settings/dependency-resolution.md` |
| `cleanupUnusedCatalogs`                               | `catalogPrune`                      | `docs/settings/_catalogPrune.mdx`        |
| `enableGlobalVirtualStore`                            | `virtualStoreType`                  | `docs/settings/node-modules.md`          |
| `namedRegistries`                                     | `registries[url].prefix`            | `docs/settings/dependency-resolution.md` |
| `useNodeVersion`                                      | `devEngines.runtime`                | `docs/migration.md`                      |
| `executionEnv.nodeVersion`                            | That package's `devEngines.runtime` | `docs/migration.md`                      |
| `npm_config_*` env vars                               | `pnpm_config_*`                     | `docs/configuring.md`                    |
| `pnpm env`                                            | `pnpm runtime`                      | `docs/cli/env.md`                        |
| `pnpm server`                                         | Removed                             | `docs/migration.md`                      |
| `pnpm install -g` with no args                        | `pnpm add -g <pkg>`                 | `docs/migration.md`                      |
| `--resolution-only` (v12)                             | `pnpm peers check`                  | `docs/cli/install.md`                    |

### [09.5]-[HOISTING]

Mistake: `shamefullyHoist: true`, `publicHoistPattern: '*'`, or `nodeLinker: hoisted` when a package imports something it does not declare. Correct form: declare the dependency, or extend the offending package with `packageExtensions`. `--shamefully-hoist`: "**WARNING**: This is highly discouraged."

### [09.6]-[GIT_BUILD_APPROVAL]

Mistake: `allowBuilds: { foo: true }` for a `git+ssh://` or `github:` dependency. Correct form: the exact resolved path including the commit, or the repository URL form (v11.11.0+).

### [09.7]-[PATCHED_MANIFEST]

Patching a dependency's `package.json`. Correct form: `overrides` or a `readPackage` pnpmfile hook.

### [09.8]-[OVERLAPPING_PATCHES]

Overlapping patch version ranges. Correct form: "explicitly exclude it from the broader range."

### [09.9]-[CONVERGENCE_RANGE]

Mistake: `"form-data@": "^4.0.6"`. Correct form: an exact version or a `catalog:` reference resolving to one, and a parent selector is rejected.

### [09.10]-[DUPLICATED_VERSION]

Mistake: the same literal in `catalog` and `overrides` (the Rasm `sharp: 0.35.4`, the nrwl/nx `minimatch`). Correct form: `catalog:` in `overrides`, as pnpm/pnpm and vitest do.

### [09.11]-[TASKS_FIELD]

Mistake: `tasks: { build: { concurrency: 2 } }`, which loses the topological order. Correct form: restate `dependsOn` explicitly.

### [09.12]-[PRESERVE_SYMLINKS]

`preserveSymlinks` with the isolated linker. Correct form: leave it off, and when required, switch to `nodeLinker: hoisted`.

### [09.13]-[STALE_REGISTRY_ALIAS]

Removing a `prefix` the lockfile still references fails with `ERR_PNPM_MISSING_NAMED_REGISTRY`, and contributors older than v11.20.0 flip the re-keyed lockfile back and forth. Correct form: one pnpm version everywhere, pinned by `packageManager` and `devEngines.packageManager`.

### [09.14]-[INCOMPATIBLE_LOCKFILE]

"if the lockfile was written by a newer pnpm major version, the install will error out". Correct form: one pnpm version everywhere.

### [09.15]-[RESTATED_RELEASE_AGE]

Writing `minimumReleaseAge: 1440` explicitly turns `minimumReleaseAgeStrict` on. Correct form for Rasm: omit the line.

### [09.16]-[SHARED_VIRTUAL_STORE]

Sharing a virtual store between projects. Correct form: leave `virtualStoreDir` per project, `virtualStoreType: global` when machine-wide sharing is wanted.

### [09.17]-[LIMITATIONS]

`docs/limitations.md`: "`npm-shrinkwrap.json` and `package-lock.json` are ignored", and "Binstubs (files in `node_modules/.bin`) are always shell files, not symlinks to JS files."

## [10]-[UNSET_SETTINGS]

| [SETTING]                                                 | [DEFAULT]        | [WHAT IT BUYS]                                                     |
| :-------------------------------------------------------- | :--------------- | :----------------------------------------------------------------- |
| `catalogPrune: true`                                      | false            | A dropped dependency takes its catalog entry with it               |
| `minimumReleaseAgeExcludePrune`                           | false            | Meaningful with an exclusion list alone, and Rasm has none         |
| `disallowWorkspaceCycles: true`                           | false            | Enforces the README acyclic rule at install time                   |
| `failIfNoMatch: true`                                     | false            | A mistyped `--filter` exits non-zero                               |
| `update.ignoreDeps`                                       | undefined        | Records the React canary and DuckDB prerelease holds               |
| `packageExtensions`                                       | undefined        | The documented fix for the `@types/*` missing-peer hazard          |
| `dedupeDirectDeps: true`                                  | false            | Fewer symlinks once `libs/typescript/*` fills in                   |
| `injectWorkspacePackages`, `syncInjectedDepsAfterScripts` | false, undefined | Required for `pnpm deploy`, unused until a Node service app exists |

## [11]-[SETTLED_AND_KEPT]

Settled:

1. pnpm 12.3.1, installed by mise (`aqua:pnpm/pnpm`), pinned by `packageManager: "pnpm@12.3.1"` and `devEngines.packageManager` at 12.3.1
2. `engines.node` stays as the floor, and no `devEngines.runtime` because mise pins Node
3. Every held-back catalog entry carries its one-line reason and an `update.ignoreDeps` entry
4. `sharp` in `overrides` becomes `"catalog:"`, other overrides keep exact pins with reasons, and converging ones take the `"pkg@"` form
5. `preferWorkspacePackages: true` is removed, and `workspace:*` specifiers carry the strictness
6. `.cache/pnpm/store` and `.cache/pnpm/cache` stay
7. `minimumReleaseAge: 1440` is removed
8. `tools/nx` gains a `package.json` and `tools/*` joins the workspace globs
9. No `scripts`, no `tasks`, every action is an Nx target, and `pnpm install --frozen-lockfile` is a step in the CI job before Nx runs
10. One default catalog, no named catalogs
11. `catalogPrune: true`, `disallowWorkspaceCycles: true`, and `failIfNoMatch: true` are added

Kept, because the plan does not decide them and the answer changes the design:

1. The 279/279 mirror: whether the root `devDependencies` block is the installation set that shrinks as `libs/typescript/*` and `apps/*/*` declare their own dependencies, or scaffolding that keeps every approved package visible to `pnpm outdated` and `pnpm update`. The answer decides whether `catalogPrune` is a safety net or a hazard and whether a package entering `libs/typescript/` leaves the root list.
2. `mermaid-cli` with `puppeteer: false`: which browser `mmdc` uses once `PUPPETEER_EXECUTABLE_PATH` leaves Forge, or whether the entry leaves the catalog until a diagram target needs it.
