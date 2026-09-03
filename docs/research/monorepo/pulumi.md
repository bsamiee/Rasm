<!-- Source for monorepo-build-infrastructure [04]-[ENGINEERING_DIRECTORY], references/iac.md, and the pulumi skill, nothing integrated yet -->
# Repository-owned Pulumi project for the Rasm GitHub settings

Rasm manages the settings of its own GitHub repository from a Pulumi program in a top-level `infra/` directory. Local facts were read from the Rasm and Parametric_Forge trees on 2026-09-03, and versions from their registries on the same day.

## [00]-[SOURCES]

Local files, with line counts:
- `Parametric_Forge/services/topology.ts` (343 lines), `estate.ts` (176), `driver.ts` (605), `README.md` (48), `package.json`, `pnpm-workspace.yaml`
- `Rasm/.claude/skills/pulumi/SKILL.md` (127 lines)
- Its references `cli-operations.md` (72), `best-practices.md` (172), `components.md` (149), `automation-api.md` (101)
- `Rasm/README.md`, `CLAUDE.md`, `nx.json`, `package.json`, `pnpm-workspace.yaml`, `tsconfig.json`, `eng/project.json`, `tools/nx/tsconfig.json`

External sources: the Pulumi docs and registry, the npm registry, GitHub code search and trees, the mise registry, and the Doppler docs.

## [01]-[VERSIONS]

| [PACKAGE]             | [NEWEST]                   | [RASM CATALOG] | [FORGE CATALOG] | [SOURCE]                                                 |
| :-------------------- | :------------------------- | :------------- | :-------------- | :------------------------------------------------------- |
| `@pulumi/pulumi`      | 3.261.0 (2026-09-02)       | 3.260.0        | 3.251.0         | `https://registry.npmjs.org/@pulumi/pulumi` dist-tags    |
| `pulumi` (the CLI)    | 3.261.0 (2026-09-02)       | Not cataloged  | Not cataloged   | `https://registry.npmjs.org/pulumi` dist-tags            |
| `@pulumi/github`      | 6.15.0                     | 6.15.0         | 6.14.0          | `https://registry.npmjs.org/@pulumi/github/latest`       |
| `@pulumiverse/doppler` | 0.9.0 (updated 2024-08-27) | 0.9.0          | 0.9.0           | `https://registry.npmjs.org/@pulumiverse/doppler/latest` |

The registry page for `github.Repository` states "GitHub v6.15.0 published on Friday, Aug 14, 2026" (`https://www.pulumi.com/registry/packages/github/api-docs/repository/`).

`Rasm/pnpm-workspace.yaml` lines 236-258, under `# Infrastructure as code`, hold 23 entries: `@pulumi/pulumi` 3.260.0, `@pulumi/github` 6.15.0, `@pulumiverse/doppler` 0.9.0, `@pulumi/policy`, `@pulumi/esc-sdk`, `@pulumi/pulumiservice`, providers for kubernetes, aws, awsx, gcp, cloudflare, postgresql, tls, random, command, docker, docker-build, eks, cloudinit, synced-folder, acme, and grafana, with `@grafana/grafana-foundation-sdk`. `Rasm/package.json` lists each as a root `devDependency` with `"catalog:"`, and the root mirror of the catalog is a decision that stays. `catalogMode: strict` and `saveExact: true` are set. `@pulumi/pulumi` moves to 3.261.0 under the newest-stable rule, and `pulumi` 3.261.0 joins the block.

## [02]-[PLACEMENT]

### [02.1]-[CODE_SEARCH]

GitHub code search over `Pulumi.yaml` by directory prefix ran through the GitHub MCP `search_code` tool on 2026-09-03. `total_count` is GitHub's own count and moves between reads, the ordering does not.

| [QUERY]                                            | [FIRST READ] | [SAME-DAY RERUN] |
| :------------------------------------------------- | -----------: | ---------------: |
| `filename:Pulumi.yaml path:infra runtime`          |          778 |             1140 |
| `filename:Pulumi.yaml path:infrastructure runtime` |          415 |              415 |
| `filename:Pulumi.yaml path:deploy runtime`         |          126 |              126 |
| `filename:Pulumi.yaml path:.github runtime`        |           22 |               22 |
| `filename:Pulumi.yaml path:services runtime`       |           10 |               10 |
| `filename:Pulumi.yaml path:eng runtime`            |            3 |                3 |

The `eng/` hits are sample repositories of one author (`TechWatching/SqlDatabaseWithAzureAd`, `TechWatching/AzureFunctionSQLBindings`, `TechWatching/FunctionAppWithoutSecretConnectionString`), and `eng/` is not a Pulumi convention.

The `.github/` hits are CI fixtures and scratch directories: `pulumi/actions` `.github/test-stacks/{golang,python,dotnet,nodejs}/Pulumi.yaml`, `simenandre/pulumi-config` `.github/test-stack/`, `usrbinkat/pulumi-kubernetes-cilium` `.github/hack/`, `johannes9108/examenslabb` `.github/skills/iac-scaffold/assets/`, and similar.

The `services/` hits are per-service subdirectories inside a `services/` source tree (`Kevan-Y/Themoo` `services/{api,core}/infra/Pulumi.yaml`, `marcmodin/kro-platform-pulumi-services` `services/{bucket,account,iam-role,account-baseline}/`). `services/` as the home of a control-plane program for the repository itself is Forge's own naming.

### [02.2]-[REPOSITORY_TREES]

[01] `budimanjojo/home-cluster` is a Pulumi program with one job, the owner's own GitHub repository settings, inside a Kubernetes home-cluster repository. Tree at `main`, 2026-09-03:

```text
infrastructure/pulumi/github/
├── Pulumi.yaml
├── Pulumi.budimanjojo.yaml        # stack named for the GitHub account
├── go.mod
├── go.sum
├── main.go
└── pkg/
    ├── config/
    └── generate/
```

[02] `latitude-dev/latitude-llm` is a pnpm monorepo with `apps/`, `packages/`, `tools/`, and the Pulumi program as a sibling top-level `infra/` that is itself a workspace package. Tree at `development`, 2026-09-03:

```text
infra/
├── Pulumi.yaml
├── Pulumi.production.yaml
├── Pulumi.staging.yaml
├── README.md
├── config.ts
├── index.ts
├── lib/
├── package.json
└── tsconfig.json
```

Its `pnpm-workspace.yaml` lists `infra` outright, between `packages/**` (with exclusion globs) and `tools/*`. `infra/package.json` declares `@pulumi/pulumi`, `@pulumi/aws`, `@pulumi/awsx`, `@pulumi/datadog`, `@pulumi/random` as its own `dependencies` and reads:

```json
{"name": "latitude-infra", "private": true, "type": "module", "scripts": {"build": "tsgo", "preview": "pulumi preview", "up": "pulumi up", "destroy": "pulumi destroy", "typecheck": "tsgo --noEmit"}}
```

Its `Pulumi.yaml` declares `backend: url: s3://latitude-pulumi-state-442420265876` and `runtime: name: nodejs, options: typescript: true`.

[03] `pulumi/docs`, the Pulumi docs monorepo, keeps its program in `infrastructure/` with its own `package.json`, `tsconfig.json`, `yarn.lock`, and one stack file per environment including per-person stacks. Listing on the default branch, 2026-09-03: `Pulumi.yaml`, `Pulumi.www-production.yaml`, `Pulumi.www-testing.yaml`, `Pulumi.staging.yaml`, `Pulumi.reg-staging.yaml`, `Pulumi.christian.yaml`, `Pulumi.sean.yaml`, `README.md`, `index.ts`, `cloudfrontFunctions.ts`, `cloudfrontLambdaAssociations.ts`, `lambdaEdge.ts`, `supportForm.ts`, `supportRedirect.ts`, `support-form/`, `package.json`, `tsconfig.json`, `tslint.json`, `yarn.lock`, `.gitignore`, and `versioned-docs/` (a second nested project).

[04] `hongbo-miao/hongbomiao.com` is a large polyglot monorepo with a top-level `infrastructure/` holding `ansible/`, `docker/`, `hm-pulumi/`, `opentofu/`, and `vagrant/` (listing at `main`, 2026-09-03), and the Pulumi program sits at `infrastructure/hm-pulumi/`.

[05] `macro-inc/macro` uses `infra/stacks/<one-directory-per-stack>/Pulumi.yaml`, 41 stacks by code search on 2026-09-03, including `infra/stacks/doppler-projects/Pulumi.yaml` and `infra/stacks/github-runners/Pulumi.yaml`.

[06] `modelcontextprotocol/registry` is a Go monorepo with its Pulumi program at top-level `deploy/` (`Pulumi.yaml`, `Pulumi.gcpProd.yaml`, `Pulumi.gcpStaging.yaml`, `Pulumi.local.yaml`, `main.go`, `pkg/`, `README.md`).

### [02.3]-[PATTERN]

- The Pulumi program sits in one top-level directory that is not a source package: `infra/`, `infrastructure/`, or `deploy/`
- No repository manages its own infrastructure from `.github/`, and `eng/` holds one author's samples
- In a JavaScript monorepo the program is a workspace member with its own `package.json` and `tsconfig.json` (latitude-llm, `pulumi/docs`)
- `Pulumi.yaml` and every `Pulumi.<stack>.yaml` sit beside the program entrypoint

### [02.4]-[FIT]

`Rasm/README.md` defines `eng/` as "Shared build and release automation", `tools/` as "Custom tools for developing this project", and forbids a `src/` directory or a directory that "exists only to add a level of nesting". Repository settings are none of those, and the decision gives them the directory the sampled repositories use: a top-level `infra/` that is a pnpm workspace package and an Nx project. The package adds one entry in `pnpm-workspace.yaml`, one `tsconfig.json` reference, and its own `package.json` naming `@pulumi/pulumi`, `@pulumi/github`, and `pulumi`, per the direct-reference rule in `CLAUDE.md`. `README.md` gains one layout row. Precedent inside Rasm: `tools/nx/` is TypeScript with its own `tsconfig.json` and a root `tsconfig.json` reference, and `eng/` is an Nx project with a `project.json` and no `package.json`.

## [03]-[PROGRAM]

### [03.1]-[REPOSITORY_PROPERTIES]

`github.Repository` inputs, from the registry page (GitHub v6.15.0) cross-checked with the upstream provider docs the Pulumi provider bridges (`integrations/terraform-provider-github` `docs/resources/repository.md`):

| [INPUT]                              | [TYPE]    | [DOCUMENTED NOTE]                                                                              |
| :----------------------------------- | :-------- | :--------------------------------------------------------------------------------------------- |
| `allowMergeCommit`                   | `boolean` | "Set to `false` to disable merge commits on the repository."                                   |
| `allowSquashMerge`                   | `boolean` | "Set to `false` to disable squash merges on the repository."                                   |
| `allowRebaseMerge`                   | `boolean` | "Set to `false` to disable rebase merges on the repository."                                   |
| `allowAutoMerge`                     | `boolean` | "Set to `true` to allow auto-merging pull requests on the repository."                         |
| `allowUpdateBranch`                  | `boolean` | "Set to `true` to always suggest updating pull request branches."                              |
| `deleteBranchOnMerge`                | `boolean` | "Automatically delete head branch after a pull request is merged. Defaults to `false`."        |
| `squashMergeCommitTitle`             | `string`  | `PR_TITLE` or `COMMIT_OR_PR_TITLE`, "Applicable only if `allow_squash_merge` is `true`."       |
| `squashMergeCommitMessage`           | `string`  | `PR_BODY`, `COMMIT_MESSAGES`, or `BLANK`, same rule                                            |
| `mergeCommitTitle`                   | `string`  | `PR_TITLE` or `MERGE_MESSAGE`, "Applicable only if `allow_merge_commit` is `true`."            |
| `mergeCommitMessage`                 | `string`  | `PR_BODY`, `PR_TITLE`, or `BLANK`, same rule                                                   |
| `hasWiki`, `hasProjects`, `hasIssues` | `boolean` | Features, `hasProjects` `true` where the organization disabled projects "will return an error" |
| `hasDiscussions`                     | `boolean` | "Defaults to `false`."                                                                         |
| `webCommitSignoffRequired`           | `boolean` | Require sign-off on web commits                                                                |
| `archiveOnDestroy`                   | `boolean` | "Set to `true` to archive the repository instead of deleting on destroy."                      |
| `hasDownloads`                       | `boolean` | Deprecated upstream                                                                            |
| `vulnerabilityAlerts`                | `boolean` | Deprecated upstream                                                                            |

"Wiki and projects off" is `hasWiki: false, hasProjects: false`, and "delete branch on merge" is `deleteBranchOnMerge: true`. The Forge `_mergeHygiene` encodes the set, and its comment "deprecated provider inputs stay unbound" holds against the current upstream docs: `hasDownloads` ("This attribute is no longer in use, but it hasn't been removed yet. It will be removed in a future version.") and `vulnerabilityAlerts` ("Use the `github_repository_vulnerability_alerts` resource instead.") stay unset.

Both pages state a caveat: "When used with GitHub App authentication, even GET requests must have the `contents:write` permission. Without it, the following arguments will be ignored, leading to unexpected behavior and confusing diffs: `allowMergeCommit`, `allowSquashMerge`, `allowRebaseMerge`, `mergeCommitTitle`, `mergeCommitMessage`, `squashMergeCommitTitle` and `squashMergeCommitMessage`." The credential for the program is a user token rather than an App installation token.

### [03.2]-[LIVE_STATE]

`gh api repos/bsamiee/Rasm`, read 2026-09-03:

```json
{
  "name": "Rasm", "description": "AEC/design-geometry workspace",
  "visibility": "public", "default_branch": "main", "archived": false,
  "allow_merge_commit": false, "allow_squash_merge": true, "allow_rebase_merge": true,
  "allow_auto_merge": true, "allow_update_branch": true, "delete_branch_on_merge": true,
  "squash_merge_commit_title": "PR_TITLE", "squash_merge_commit_message": "PR_BODY",
  "merge_commit_title": "MERGE_MESSAGE", "merge_commit_message": "PR_TITLE",
  "has_issues": true, "has_projects": false, "has_wiki": false,
  "has_discussions": false, "has_downloads": false,
  "web_commit_signoff_required": false
}
```

`gh api repos/bsamiee/Rasm/rulesets` returns `[]`, and `repos/bsamiee/Rasm/branches/main/protection` returns 404 "Branch not protected". Both match the Forge decision in `services/README.md`: "Rulesets / branch policy — Empty by ruling: `main` takes direct pushes." Rasm adds no gating configuration.

The live values are the Forge `_mergeHygiene` constant, already applied. A correctly written Rasm program adopting the repository plans zero changes on its first `preview`, and a successful `preview --refresh --expect-no-changes` proves the move lossless.

`visibility: "public"` is live and declared by neither program: the Forge `estate.ts` leaves `visibility` unset, the provider treats it as computed, and Rasm leaves it unset.

### [03.3]-[ADOPTION]

`bsamiee/Rasm` exists, and the program adopts it.

The `import` resource option (`https://www.pulumi.com/docs/iac/concepts/resources/options/import/`): "The `import` resource option imports an existing cloud resource so that Pulumi can manage it. Imported resources can have been provisioned by any other method, including manually in the cloud console or with the cloud CLI." The TypeScript form is `{ import: "<id>" }` as the third constructor argument. "If the resource's arguments differ from the imported state, the import will succeed, and the resource will then be modified to reflect the inputs in your Pulumi program." "Once a resource is successfully imported, remove the `import` option because Pulumi is now managing the resource."

The import id for a repository is its name: "Repositories can be imported using the `name`, e.g. `terraform import github_repository.terraform myrepo`" (upstream `repository.md`), the option reads `{ import: "Rasm" }`, and the Forge `estate.ts` uses `importId: (row) => row.name`. For a ruleset the id is `<repository>:<ruleset id>`: "GitHub Repository Rulesets can be imported using the GitHub repository name and ruleset ID e.g. `$ terraform import github_repository_ruleset.example example:12345`" (upstream `repository_ruleset.md`), and Forge encodes it as `` `${row.repository}:${row.importId}` ``.

The `pulumi import` CLI is the alternative. `cli-operations.md` line 68 of the Rasm skill: "import takes the full pkg:mod/type:Type token, not pulumi do's short aws:s3:Bucket", and the command `npx pulumi import github:index/repository:Repository rasm Rasm` generates code.

Choice: the `import` resource option behind a flag, as Forge does (`f.adopt && row.origin === 'adopt'`, `estate.ts` line 58), and the option drops after the first successful `up` without editing resource declarations.

`protect: true` guards against a program edit cascading into repository deletion. Forge sets it on every `github.Repository` (`{ provider: gh, protect: true, ...options }`, `estate.ts` line 129), and its README records "repositories carry protect so a row edit can never cascade into repo destruction". The Rasm skill states the same in `SKILL.md` section 02: "removing a resource from the program deletes it from the cloud on the next `up`, `protect: true` guards anything unaffordable to lose." `archiveOnDestroy: true` is the provider-level second guard.

### [03.4]-[MIGRATION]

`bsamiee/Rasm` is in the Forge `estate` stack state. Deleting the row from `Parametric_Forge/services/topology.ts` and running `up` reads as a delete, `protect: true` turns that into an error, and the Forge stack cannot reach steady state until the resource leaves its checkpoint.

The docs page at `https://www.pulumi.com/docs/iac/cli/commands/pulumi_state_delete/` renders the command as `pulumi state remove`: "Deletes one or more resources from a stack's state", state only, with `--force` ("Force deletion of protected resources"), `--target-dependents`, `--all`, `--yes`, and "Protected resources will not be deleted unless specifically requested using the --force flag". `pulumi state unprotect` "clears the 'protect' bit on one or more resources, allowing those resources to be deleted", with `--all` or a URN (`pulumi_state_unprotect`). URNs come from `pulumi stack --show-urns`.

Ordering that keeps the repository managed throughout:

1. Write and `preview` the Rasm program with `{ import: "Rasm" }` while Forge still owns the resource, a preview writes nothing
2. Run `up` in Rasm to bring the resource into the Rasm stack state
3. In Forge, run `pulumi state unprotect <urn>` then `pulumi state remove <urn>`, which touches the checkpoint and never GitHub
4. Delete the row from `Parametric_Forge/services/topology.ts`, Forge keeps `Parametric_Forge` and every other repository it owns
5. Run `node services/driver.ts preview --refresh --expect-no-changes` in Forge and `preview` in Rasm, both showing no changes

Steps 2 and 3 must run in that order.

### [03.5]-[CREDENTIAL]

`https://www.pulumi.com/registry/packages/github/installation-configuration/`: the token comes from the `GITHUB_TOKEN` environment variable or `pulumi config set github:token --secret`, the owner from `GITHUB_OWNER` or `github:owner`, and "The complete list of configuration parameters is in the GitHub provider README."

`owner` is no secret and belongs in `Pulumi.<stack>.yaml` as `github:owner: bsamiee`, or in an explicit `new github.Provider('github', { owner: 'bsamiee' })` as Forge writes it. `token` arrives as `GITHUB_TOKEN` in the process environment and enters no repository file.

## [04]-[STATE_AND_SECRETS]

### [04.1]-[BACKEND]

From `https://www.pulumi.com/docs/iac/concepts/state-and-backends/` and `https://www.pulumi.com/docs/iac/operations/stack-management/using-a-diy-backend/`: "To use a DIY backend, specify a storage endpoint URL as `pulumi login`'s `<backend-url>` argument: `s3://<bucket-path>`, `azblob://<container-path>`, `gs://<bucket-path>`, or `file://<fs-path>`." "`pulumi login --local` is syntactic sugar for `pulumi login file://~`", which stores state under `~/.pulumi`. `PULUMI_BACKEND_URL` or a `backend: url:` block in `Pulumi.yaml` avoids repeating the login, and `latitude-dev/latitude-llm` commits the block. A DIY backend keeps `meta.yaml`, `stacks/`, `locks/`, and `history/` under `.pulumi`, and "DIY backends also maintain checkpoint history (in the `.pulumi/history/` directory)". "For DIY backends, state management—including backup, sharing, and team access synchronization—is custom and implemented manually."

Cost does not decide it: `https://www.pulumi.com/pricing/` lists an Individual edition, "$0 forever (No credit card required)", one user, IaC state management, "Unlimited projects, stacks, and environments", unlimited updates and history, up to 500 workflow minutes, and Team is $40/month base, Enterprise $400/month base, Business Critical custom. The file backend needs no account, no login, and no network for state, Forge already chose it, and the owner backs up the state directory.

### [04.2]-[STATE_LOCATION]

Forge puts state at `$XDG_STATE_HOME/forge-services` (`driver.ts` `_settings.stateDir`, overridable by `FORGE_SERVICES_STATE_DIR`, created with `chmod 0700` at lines 170-171). `Rasm/README.md` states that "tool work directories that cannot be relocated are ignored and hold no durable output", and with the passphrase provider the checkpoint holds encrypted secret material.

| [LOCATION]                   | [VERDICT] | [REASON]                                                                                          |
| :--------------------------- | :-------- | :------------------------------------------------------------------------------------------------ |
| `<repo>/.cache/pulumi/`      | Wrong     | State is durable output, and losing it means re-importing every resource                          |
| `<repo>/.artifacts/pulumi/`  | Wrong     | `.artifacts/` is regenerable, gitignored build output, and state is neither                       |
| Committed in the repository  | Rejected  | The checkpoint holds resource outputs and secret material and conflicts on every concurrent `up`  |
| `$XDG_STATE_HOME/rasm-infra` | Correct   | Durable, machine-scoped, outside the worktree, the pattern Forge proves                           |

Settled: `file://${XDG_STATE_HOME:-$HOME/.local/state}/rasm-infra`, set as `PULUMI_BACKEND_URL` by the Nx targets. The `backend: url:` form stays out of `Pulumi.yaml`, because a `file://` absolute path in a committed file is machine-specific and `Rasm/README.md` requires portability to Linux and Windows.

### [04.3]-[SECRETS_PROVIDER]

`https://www.pulumi.com/docs/iac/cli/environment-variables/`: `PULUMI_CONFIG_PASSPHRASE` "Set this as an environment variable to protect and unlock your configuration values and secrets. Your passphrase is used to generate a unique key for your stack, and configuration and encrypted state values are then encrypted using `AES-256-GCM`", and `PULUMI_CONFIG_PASSPHRASE_FILE` is "An alternative method... Set this to the path of a file that contains the passphrase value."

Forge passes `secretsProvider: 'passphrase'` (`driver.ts` line 201) and brokers the value from 1Password (`op://Tokens/PULUMI_FORGE_SERVICES/password`, line 133) when `PULUMI_CONFIG_PASSPHRASE` is absent.

The program declares no secret config, and `GITHUB_TOKEN` arrives from the environment and is never written to state. A stack created on a DIY backend still selects a secrets provider, `passphrase` produces the `encryptionsalt` in the stack settings file, and a passphrase is part of the setup. Settled by the decision that every secret centralizes in Doppler: `PULUMI_CONFIG_PASSPHRASE` is a secret in the `rasm`/`dev_repo` Doppler config and reaches the process through `doppler run --`, and a later secret row needs no migration.

### [04.4]-[PULUMI_HOME]

`PULUMI_HOME` "Overrides the folder where the Pulumi CLI stores its artifacts: plugins, workspaces, templates, and credentials file. By default, artifacts are stored next to Pulumi binaries in `~/.pulumi`" (environment variables page). Provider plugin binaries are downloaded and regenerable, which `Rasm/README.md` routes under `.cache/`. `PULUMI_HOME=.cache/pulumi` on the targets keeps plugins in the repository cache tree, and state stays in XDG state. The targets set `PULUMI_SKIP_UPDATE_CHECK` (skips the version update check, a pinned CLI stops nagging) and `PULUMI_BACKEND_URL` ("Set this environment variable to use a specified backend instead of the default backend").

### [04.5]-[GITHUB_TOKEN]

The provider reads `GITHUB_TOKEN` from the process environment with no Pulumi config entry. The Forge resolution order (`driver.ts` `_settings` and `_githubToken`, lines 150-163): ambient `GITHUB_TOKEN`, then ambient `GH_TOKEN`, then a Doppler read of `GITHUB_TOKEN` from `agent-runtime/dev` using the Doppler IaC token, itself brokered from 1Password (`op://Tokens/DOPPLER_IAC_TOKEN/token`) when `DOPPLER_TOKEN` is absent. Rasm reaches the same value with one hop through the machine's `rasm` Doppler scope:

```bash
doppler run -- npx pulumi preview     # injects the scope's secrets, GITHUB_TOKEN included
```

The scope resolution is the `~/.doppler/.doppler.yaml` entry for the repository root that Forge applies, and the file exists on this machine. No file in Rasm holds the token.

### [04.6]-[PROJECT_AND_STACK_FILES]

`Pulumi.yaml` (`https://www.pulumi.com/docs/iac/concepts/projects/project-file/`): required `name` ("Name of the project containing alphanumeric characters, hyphens, underscores, and periods") and `runtime` (`nodejs`, `python`, `go`, `dotnet`, `java`, `yaml`, `hcl`, or `bun`), optional `description`, `main`, `backend`, `config`, `options`. The nodejs runtime takes `options.packagemanager` (`npm`, `pnpm`, `yarn`, or `bun`, "When unset, the package manager is auto-detected from lockfiles in the project directory, falling back to `npm` if none are found"), `options.typescript`, `options.tsconfig`, `options.nodeargs`. In a pnpm workspace the lockfile sits at the workspace root and not in `infra/`, `packagemanager: pnpm` is set explicitly.

`Pulumi.<stack>.yaml` (`https://www.pulumi.com/docs/iac/concepts/projects/stack-settings-file/`): "Every Pulumi stack has a settings file named `Pulumi.<stack-name>.yaml`", with the keys `secretsprovider`, `encryptedkey`, `encryptionsalt`, `config`, `environment`. Provider-namespaced keys take the `provider:key` form. The only entry needed is `github:owner: bsamiee`, and `encryptionsalt` "is automatically generated and managed by Pulumi when you first set up passphrase encryption". The project name is `rasm-infra` and the stack `estate`, matching the Forge stack name.

## [05]-[INVOCATION]

### [05.1]-[NX_TARGETS]

`https://nx.dev/docs/reference/project-configuration`: "Caching is configured by specifying `"cache": true` in a target's configuration", "Each cacheable task needs to define `inputs`", and "In Nx 19.5.0+... setting `"parallelism": false`, will ensure that those tasks will not run in parallel with other tasks on a single machine". Rasm pins `nx: 23.1.3`, and the newest release is 23.2.0.

`Rasm/.claude/skills/monorepo-build-infrastructure/SKILL.md` section 02: "Cache a target when its outputs are a function of its declared inputs alone", with `provision` and `stage` marked "No cache" because their inputs include "Network, host toolchain". A Pulumi target reads live GitHub state over the network and writes a checkpoint outside the workspace:

| [TARGET]    | [COMMAND]                                  | [CACHE] | [PARALLELISM] | [REASON]                            |
| :---------- | :----------------------------------------- | :------ | :------------ | :---------------------------------- |
| `preview`   | `pulumi preview --diff`                    | `false` | `false`       | Reads live GitHub                   |
| `up`        | `pulumi up`                                | `false` | `false`       | Mutates a live control plane        |
| `refresh`   | `pulumi refresh`                           | `false` | `false`       | Re-reads live state                 |
| `typecheck` | Inherited from `targetDefaults.typecheck`  | `true`  |               | `tsc --build`, defined in `nx.json` |

`nx.json` carries a `typecheck` default (`command: "tsc --build --pretty false"`, `cache: true`, `outputs: ["{projectRoot}/dist"]`), and `infra/` inherits typechecking once it has a `tsconfig.json` and joins the root `tsconfig.json` `references`, as `tools/nx` does. `inputs` on the uncached targets keep `nx affected` honest: `["{projectRoot}/**/*", "sharedGlobals"]`, and `outputs` is empty.

Precedent for `preview` and `up` as scripts: `latitude-dev/latitude-llm` `infra/package.json` and the Forge root `package.json` (`"preview": "node services/driver.ts preview"`, `"up": "node services/driver.ts up"`).

The targets set `PULUMI_BACKEND_URL`, `PULUMI_HOME=.cache/pulumi`, and `PULUMI_SKIP_UPDATE_CHECK=1`, and `GITHUB_TOKEN` and `PULUMI_CONFIG_PASSPHRASE` arrive from `doppler run --`.

`up` never runs unattended. `best-practices.md` line 141: "`pulumi up --yes` without a reviewed preview is deploying blind." The `up` target carries no `--yes`, and `preview` is the target agents and CI run.

### [05.2]-[CLI_PIN]

The Pulumi CLI is a separate npm package from `@pulumi/pulumi`. From the registry on 2026-09-03: `@pulumi/pulumi@3.261.0` has `bin: null`, and `pulumi@3.261.0` is "Pulumi Infrastructure as Code CLI" with `"bin": {"pulumi": "run.js"}` and `"engines": {"node": ">=18"}`. That `bin` entry makes `npx pulumi <command>` work, and the Rasm skill mandates the form: "`npx pulumi <command>` is the canonical invocation, the PATH `pulumi` lacks the resource subcommands" (`SKILL.md` section 01).

Settled: `pulumi: 3.261.0` in the `# Infrastructure as code` block of `pnpm-workspace.yaml`, `"pulumi": "catalog:"` in `infra/package.json` and in the root `package.json` mirror. Every target invokes `npx pulumi`. mise carries a `pulumi` shorthand (`aqua:pulumi/pulumi`, `https://github.com/jdx/mise/blob/main/registry/pulumi.toml`) and a `doppler` shorthand (`github:DopplerHQ/cli`), and the catalog pin is chosen because it matches how every other JavaScript tool in Rasm is pinned and moves with the lockfile.

## [06]-[DOPPLER]

### [06.1]-[DIRECTORY_SCOPE]

`https://docs.doppler.com/docs/cli` and `https://docs.doppler.com/docs/multiple-workplaces`: "All Doppler CLI interactions have an inherent scope associated with them. That scope always defaults to the current directory you're in, but it can also be specified explicitly using the `--scope` flag", `doppler login` "creates a new configuration entry with a scope of `/` that applies to all sub-directories on your filesystem", `doppler setup` in a sub-directory adds project and config entries for that scope, and "all sub-directories of the workplace directory will inherit the new scope". The config directory is `--config-dir` (default `/Users/me/.doppler`), and on this machine `~/.doppler/.doppler.yaml` exists. The scope is per-machine, per-user state outside the repository.

### [06.2]-[REPOSITORY_FILE]

`doppler.yaml` is optional: "you can also create a `doppler.yaml` file that notes which project and config should be set using `doppler setup`", applied by `doppler setup --no-interactive`. Its one function is to seed the same entries non-interactively. The Forge `driver.ts` `_applyScopes` (line 311) applies each row directly, and its `_strayYaml` check (line 272) fails the run when a `doppler.yaml` appears under `~/Documents/99.Github`. Forge enforces the prohibition.

### [06.3]-[SCOPE_ROW_OWNER]

The Pulumi Doppler provider (`https://www.pulumi.com/registry/packages/doppler/api-docs/`, v0.9.0 published 2024-08-27) lists resources `BranchConfig`, `Environment`, `Group`, `GroupMember`, `GroupMembers`, `Project`, `ProjectRole`, `Provider`, `Secret`, `ServiceAccount`, `ServiceAccountToken`, `ServiceToken`, `Webhook`, functions `GetSecrets` and `GetUser`, modules `integration`, `projectMember`, `secretsSync`. No resource declares a CLI directory scope, and none can: the provider talks to the Doppler API and the scope is a local file.

The Doppler project `rasm`, its environments, and the `dev_repo` branch config are API resources, the `doppler.md` thread decides which program declares them, and Forge `services/` holds them today. The scope from the repository root to `rasm`/`dev_repo` is machine configuration and stays in Forge permanently, with `_scopeRoot` at the directory holding the repository (`topology.ts` line 105).

Forge holds both today: `topology.ts` `_projects` has `{slug: 'rasm', ..., origin: 'adopt'}` (lines 27-29), `_configs` has `{ project: 'rasm', environment: 'dev', name: 'dev_repo', origin: 'adopt' }` (line 76), and `_scopes` has `{ dir: `${_scopeRoot}/Rasm`, project: 'rasm', config: 'dev_repo' }` (line 113). Moving GitHub settings to Rasm does not move the scope, and Rasm consumes it by sitting at that path.

## [07]-[SKILL_COVERAGE]

The Rasm pulumi skill already covers:
- The L1/L2/L3 table, and this work is L2
- "Match the codebase language when one is present, default to TypeScript otherwise"
- The `protect: true` rule and stack-per-environment
- `npx pulumi <command>` and `npx pulumi <command> --help` for uncertain flags
- "When a `Pulumi.yaml` project already manages a resource, changes go through the program, never `pulumi do`"
- Line 25 on inspecting the filesystem first and asking before a command that requires a login
- The debugging procedure in section 04
- `best-practices.md` on secrets, aliases, the reviewed preview, and component grouping
- `cli-operations.md` on `pulumi import`
- `automation-api.md` line 3: "Single projects with standard deployment needs stay on the CLI."

Gaps the plan fills: `SKILL.md` section 01 says "On authentication failure, ask the operator to run `pulumi login`, never fall back to `pulumi login --local` or set `PULUMI_CONFIG_PASSPHRASE`", scoped to L1 `pulumi do` but absolute at a glance. One line scopes the prohibition to L1 so the file backend design reads as intended, a repository edit for the implementation phase. The skill says nothing about `@pulumi/github` or the `import` resource option, and nothing needs adding, the registry is the source and the skill says to look properties up.

## [08]-[SHAPE]

### [08.1]-[TREE]

```text
Rasm/
├── infra/
│   ├── Pulumi.yaml                # name rasm-infra, runtime nodejs, options packagemanager pnpm, description
│   ├── Pulumi.estate.yaml         # stack settings: github:owner, encryptionsalt
│   ├── package.json               # private, name rasm-infra, deps @pulumi/pulumi @pulumi/github pulumi
│   ├── project.json               # Nx targets: preview, up, refresh
│   ├── tsconfig.json              # extends ../tsconfig.base.json
│   ├── README.md                  # dependency list per CLAUDE.md [DEPENDENCY_SOURCES]
│   ├── index.ts                   # program entry: provider + resource registration
│   └── topology.ts                # typed rows: repository settings
├── pnpm-workspace.yaml            # + packages entry 'infra'; + catalog row pulumi: 3.261.0; @pulumi/pulumi 3.261.0
├── package.json                   # + "pulumi": "catalog:"
├── tsconfig.json                  # + { "path": "./infra" } in references
├── nx.json                        # unchanged
├── README.md                      # + one LAYOUT row for infra/
└── .cache/pulumi/                 # PULUMI_HOME, gitignored, plugin binaries
```

State is outside the tree at `file://$XDG_STATE_HOME/rasm-infra`.

The program is two peer files: `Rasm/README.md` section 07 forbids nesting for its own sake and permits peer files, and the Forge split (rows in `topology.ts`, registration in `estate.ts`) lifts directly. No ruleset rows: the Forge `_rulesets` is empty by decision and its dormant `_rulesetPolicy` stays in Forge, Rasm carries no gating configuration.

### [08.2]-[NX_TARGETS]

```json
{
    "name": "infra",
    "targets": {
        "preview": {
            "command": "doppler run -- npx pulumi preview --diff",
            "options": { "cwd": "{projectRoot}" },
            "cache": false,
            "parallelism": false,
            "inputs": ["{projectRoot}/**/*", "sharedGlobals"]
        },
        "up": {
            "command": "doppler run -- npx pulumi up",
            "options": { "cwd": "{projectRoot}" },
            "cache": false,
            "parallelism": false
        },
        "refresh": {
            "command": "doppler run -- npx pulumi refresh",
            "options": { "cwd": "{projectRoot}" },
            "cache": false,
            "parallelism": false
        }
    }
}
```

| [TARGET]          | [COMMAND]                | [PROVES]                                            |
| :---------------- | :----------------------- | :-------------------------------------------------- |
| `infra:preview`   | `pulumi preview --diff`  | Desired-versus-live diff, steady state is no changes |
| `infra:up`        | `pulumi up`              | Applies the rows, no `--yes`                        |
| `infra:refresh`   | `pulumi refresh`         | Reconciles state against live GitHub                |
| `infra:typecheck` | Inherited from `nx.json` | `tsc --build` over the program                      |

`options.cwd: "{projectRoot}"` is what the `nx.json` `typecheck` default already uses. `up` carries no `dependsOn: ["preview"]`, chaining makes the preview a machine step, and the skill requires a reviewed preview. The drift check Forge proves (`preview --refresh --expect-no-changes`, `driver.ts` `_modes` lines 518-520) becomes another target once the first `up` lands, and `npx pulumi preview --help` confirms the CLI spellings before it is written.
