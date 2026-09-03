<!-- Source for monorepo-build-infrastructure references/iac.md and the secrets skill, nothing integrated yet -->
# Secrets for Rasm: Doppler, 1Password, Pulumi, GitHub Actions

Live state, files, and documentation were read on 2026-09-03. Settled decisions:

- Environments `dev` and `prd`
- One CI service token, written by Pulumi into a GitHub Actions secret (Route B)
- The existing GitHub token reused
- The Rhino token added for future headless Rhino
- Doppler owns runtime secrets and 1Password keeps credentials a person uses
- `BUF_TOKEN` removed from 1Password alone
- The directory scope row stays in Forge

## [00]-[SOURCES]

Read in Rasm: `.claude/skills/secrets/SKILL.md`, `.claude/skills/pulumi/SKILL.md`, `nx.json`, `NuGet.config`, `pnpm-workspace.yaml`.

Read in Parametric_Forge: `services/topology.ts`, `services/estate.ts`, `services/driver.ts`, `services/README.md`, `docs/atlas/secrets-and-services.md`, `modules/home/programs/shell-tools/1password.nix`, `pnpm-workspace.yaml`, and the installed `node_modules/@pulumiverse/doppler` and `node_modules/@pulumi/github` type definitions.

By the plan, Rasm's Pulumi program sits under a top-level `infra/` as a pnpm workspace package and Nx project, with a file backend under the XDG state directory and `PULUMI_HOME` under `.cache/`.

## [01]-[LIVE_STATE]

### [01.1]-[DOPPLER]

`doppler --version` prints `v3.76.5`. `doppler me --json` returns workplace `Parametric_Arsenal`, slug `5ea0685de888a078a723`, type `cli`, name `macbook`, created 2026-08-11, the owner's personal CLI login.

`doppler projects --json` returns four projects, the JSON holds `id`, `name`, `description`, `created_at`, and `id` equals `name`:

| [INDEX] | [PROJECT]          | [DESCRIPTION]                                    | [CREATED]  | [TOPOLOGY]          |
| :-----: | :----------------- | :----------------------------------------------- | :--------- | :------------------ |
|  [01]   | `agent-runtime`    | AI agent runtime secrets                         | 2026-07-09 | Yes, origin `mint`  |
|  [02]   | `maghz`            | Maghz VPS runtime secrets                        | 2026-07-08 | No                  |
|  [03]   | `parametric-forge` | macOS machine and Home Manager toolchain secrets | 2026-07-08 | Yes, origin `adopt` |
|  [04]   | `rasm`             | Rasm repo and service secrets                    | 2026-07-08 | Yes, origin `adopt` |

`doppler environments --project rasm --json` returns `dev` (Development), `stg` (Staging), `prd` (Production), all created `2026-07-08T06:04:54.387Z`. `doppler configs --project rasm --json` returns four configs: `dev`, `stg`, `prd` (root, locked) and `dev_repo` (branch of `dev`, created `2026-07-08T06:05:08.830Z`). No config inherits from another.

Every `initial_fetch_at` and `last_fetch_at` field advances on every read, including the reads this verification made, the timestamps say nothing about which config a process uses.

Key inventory, `doppler secrets download --project rasm --config <c> --no-file --format json | jq 'keys'`:

- `rasm/dev`, `rasm/stg`, `rasm/prd`: `DOPPLER_CONFIG`, `DOPPLER_ENVIRONMENT`, `DOPPLER_PROJECT`
- `rasm/dev_repo`: `BUF_TOKEN`, `DOPPLER_CONFIG`, `DOPPLER_ENVIRONMENT`, `DOPPLER_PROJECT`

The three `DOPPLER_*` keys are Doppler's own metadata. The Rasm project holds one secret, `BUF_TOKEN` in `dev_repo`.

`doppler configs tokens --project rasm --config <c> --json` returns `null` for all four configs, no Rasm service token exists. The two declared tokens exist live: `agent-runtime/dev` has `agent-readonly` (read, slug `b31de174-5f65-4e4b-aa37-5aa7d52ce8b4`) and `parametric-forge/dev_machine` has `forge-machine-readonly` (read, slug `01e0d13c-804f-4cfb-a7a7-715436a7f97b`).

The CLI has no `integrations` and no `sync` command (`doppler integrations` returns `unknown command`, and the command list is `activity changelog completion configs configure environments feedback flags help import login logout me mfa oidc open projects run secrets settings setup tui update`). `doppler settings --json` returns `billing_email`, `id`, `name` alone, and `doppler me --json` holds no plan field, the plan tier is not readable from the CLI.

### [01.2]-[DIRECTORY_SCOPES]

`doppler configure --all --json`, root token removed, returns three entries:

```json
{
  "/": { "api-host": "https://api.doppler.com", "dashboard-host": "https://dashboard.doppler.com" },
  "the directory holding the repository/Parametric_Forge": { "enclave.project": "parametric-forge", "enclave.config": "dev_machine" },
  "the directory holding the repository/Rasm": { "enclave.project": "rasm", "enclave.config": "dev_repo" }
}
```

`doppler configure get project config --scope the directory holding the repository/Rasm --json` returns `{"enclave.config":"dev_repo","enclave.project":"rasm"}`. The scope row at `topology.ts` line 113 matches. No `doppler.yaml` exists under either repository (`fd -H -g 'doppler.yaml'`).

### [01.3]-[PULUMI_STATE]

`driver.ts` opens the stack `forge-services/estate` on a file backend under `~/.local/state/forge-services` (lines 23-24, 140-144, 175, 199). `pulumi stack ls --all` against that backend prints one row, `organization/forge-services/estate`, last update one week ago, resource count `0`. The checkpoint `.pulumi/stacks/forge-services/estate.json` is 378 bytes, manifest time `2026-08-21T02:55:01`, Pulumi `v3.255.0` (the installed CLI), a passphrase secrets provider, and no `resources` key. The `.bak` is `{"version":0,"checkpoint":{"stack":"organization/forge-services/estate"}}`.

The Forge stack never applied a resource. Every Doppler and GitHub row in `topology.ts` is a description of live state that Pulumi does not enforce, and the live projects, environments, configs, and tokens were created outside Pulumi. Nothing is handed off from Forge to Rasm: Rasm's program imports the live resources directly and the Forge rows are deleted.

### [01.4]-[ONE_PASSWORD]

`op whoami`: `https://my.1password.com/`, user type `SERVICE_ACCOUNT`, integration ID `IICJISPHBNADBCJN57J2FGYFWE`. Under the service account, `op vault list` shows one vault, `Tokens` (`zw45xqlsszad5uugvd4iqxuuzi`), and the `Personal` vault resolves under `env -u OP_SERVICE_ACCOUNT_TOKEN` alone, as the secrets skill `[03]-[OP_CLI]` states.

`op item list --vault Tokens --format json | jq -r '.[].title'` returns 22 items: `ANTHROPIC_API_KEY`, `BUF_TOKEN`, `CACHIX_AUTH_TOKEN`, `CODERABBIT_API_KEY`, `CONTEXT7_API_KEY`, `DOPPLER_AGENT_READONLY`, `DOPPLER_FORGE_MACHINE_READONLY`, `DOPPLER_IAC_TOKEN`, `DOPPLER_MAGHZ_HOST_READONLY`, `EXA_API_KEY`, `GH_PROJECTS_TOKEN`, `GITHUB_TOKEN`, `GOOGLE_OAUTH_CLIENT_ID`, `GOOGLE_OAUTH_CLIENT_SECRET`, `GREPTILE_API_KEY`, `HOSTINGER_API_TOKEN`, `MAGHZ_MCP__DATABASE_URI`, `OP_SERVICE_ACCOUNT_TOKEN`, `PERPLEXITY_API_KEY`, `PULUMI_FORGE_SERVICES`, `RHINO_TOKEN`, `TAVILY_API_KEY`.

No `DOPPLER_RASM_*` item exists, consistent with no Rasm service token ever issued. `RHINO_TOKEN` exists and `1password.nix` line 138 exports it.

### [01.5]-[BUF_TOKEN_COPIES]

`BUF_TOKEN` is held in `rasm/dev_repo` and in `op://Tokens/BUF_TOKEN/token`, and `1password.nix` line 147 exports it into `~/.config/hm-op-session.sh`, which every interactive shell sources through `forge-session-secrets.sh` (line 125).

All three copies are identical. Hashing each value without a trailing newline gives SHA-256 prefix `72a49a759d09bf2d` for the Doppler value, the 1Password value, and the ambient `$BUF_TOKEN`, and hashing each with a trailing newline gives `5fc7480c1c491514` for all three. Comparing `doppler secrets get` and `op read` output (newline-terminated) against `printf '%s' "$BUF_TOKEN"` (no newline) shows a false divergence. No copy is stale.

By decision, Doppler owns `BUF_TOKEN`: the `Tokens` vault item is deleted and the `export BUF_TOKEN` line leaves `op/env.template` in `1password.nix`, the one change to 1Password.

### [01.6]-[RASM_NEEDS]

- Neither `Rasm/.github` nor `Parametric_Forge/.github` exists, and no GitHub Actions workflow consumes a secret
- `Rasm/nx.json` sets `"neverConnectToCloud": true` with `nxCloudId`, `nxCloudAccessToken`, and `nxCloudUrl` null
- `Rasm/NuGet.config` declares `https://api.nuget.org/v3/index.json` and `.artifacts/nuget`, and restore needs no credential
- `@bufbuild/buf` 1.72.0 is pinned at `pnpm-workspace.yaml` line 280, and no `buf.yaml`, `buf.gen.yaml`, or `buf.lock` exists. `BUF_TOKEN` authenticates pushes to the Buf Schema Registry, and nothing pushes yet

The build needs no secret. The declarations exist so that CI, a second machine, and headless Rhino reach secrets through IaC the day they need them, with no `.env` and no `doppler.yaml`.

## [02]-[DIRECTORY_SCOPES]

### [02.1]-[CONFIG_FILE]

`doppler configure set project=<p> config=<c> --scope <dir>` writes `$DOPPLER_CONFIG_DIR/.doppler.yaml`, default `~/.doppler/.doppler.yaml` (`doppler configure set --help`: `--config-dir string  config directory (default "/Users/bardiasamiee/.doppler")`, `--scope string  the directory to scope your config to (default ".")`).

The live file is mode 600, 470 bytes, last written 2026-08-23:

```yaml
scoped:
  /:
    token: secret-<uuid>
    api-host: https://api.doppler.com
    dashboard-host: https://dashboard.doppler.com
  the directory holding the repository/Parametric_Forge:
    enclave.project: parametric-forge
    enclave.config: dev_machine
  the directory holding the repository/Rasm:
    enclave.project: rasm
    enclave.config: dev_repo
version-check: {}
tui:
  introVersionSeen: 0
```

The `token` under `/` is a keyring handle and not a Doppler token: Doppler tokens are `dp.`-prefixed (`dp.ct.` for CLI, `dp.st.` for service tokens per <https://docs.doppler.com/docs/multiple-workplaces> and <https://docs.doppler.com/docs/service-tokens>), and this value is `secret-<uuid>`. <https://docs.doppler.com/docs/cli-troubleshooting> resolves "Token not found in system keyring" by reading the value under `scoped:` in `~/.doppler/.doppler.yaml` and running `doppler configure unset token --scope <that value>`. The secrets skill `[05]-[STORAGE]` says the same: "Personal `doppler login` is the only credential Doppler keeps in the keychain."

No repository file is needed. Doppler staff on <https://community.doppler.com/t/token-config-scope-documentation/787>: "Doppler doesn't leave or reference any kind of file in the directories you run `doppler setup` in. Instead, when you run `doppler setup` it creates an entry in `~/.doppler/.doppler.yaml` for the path you were at when you performed the setup (note: this file is never meant to be edited by hand)."

A repository `doppler.yaml` is a seed for `doppler setup` (<https://docs.doppler.com/docs/cli>, "you can also create a `doppler.yaml` file that notes which project and config should be set using `doppler setup`"), with a `path` per entry for monorepos (<https://github.com/DopplerHQ/cli/pull/394>: "It lets you specify multiple project+repo combinations and adds a new `path` field"). It produces the same `~/.doppler/.doppler.yaml` entries that `doppler configure set --scope` produces. Keeping `doppler.yaml` out of Rasm costs nothing.

A scope entry is keyed on an absolute path. Moving or renaming the directory orphans it, per the same community answer: "if you move a directory like you mentioned, then you'll need to run `doppler setup` again inside that directory."

### [02.2]-[RESOLUTION_ORDER]

<https://docs.doppler.com/docs/environment-based-configuration>: "CLI configuration is processed in the following manner, with lower numbers given higher priority: 1. Runtime flag 2. Environment variable 3. Configuration file (if supported)." A service token holds its own project and config, which is why the secrets skill lists it first.

<https://docs.doppler.com/docs/multiple-workplaces>: `doppler login` creates the `/` entry that "applies to all sub-directories on your filesystem", and `doppler setup` in a subdirectory "expand[s] this configuration with project and config entries for a specific scope". Settings inherit per key: the Rasm entry supplies `enclave.project` and `enclave.config`, while `token`, `api-host`, and `dashboard-host` come from `/`.

### [02.3]-[DECLARATION]

A directory scope is machine state. The installed `@pulumiverse/doppler` 0.9.0 declares `Project`, `Environment`, `BranchConfig`, `Secret`, `ServiceToken`, `ServiceAccount`, `ServiceAccountToken`, `ProjectRole`, `Group`, `GroupMember`, `GroupMembers`, `Webhook`, `Provider`, the `integration/` family, the `secretssync/` family, and the `projectmember/` family. Nothing declares a CLI scope.

Forge declares scopes as `Topology.scopes` rows (`topology.ts` lines 107-118). `node services/driver.ts scopes apply` runs `doppler configure set` per row and then `doppler configure unset project config --scope <dir>` for every entry under `Topology.scopeRoot` that is not a row (`driver.ts` lines 259-270, 311-323), and `scopes doctor` and `scopes strict` report and fail on divergence (lines 283-341). Because the stray cleanup unsets any scope under the directory holding the repository it cannot see, the Rasm scope row stays in Forge, by decision.

## [03]-[MACHINE_ACCESS]

### [03.1]-[IDENTITIES]

Two identities reach Rasm secrets on this machine:

1. The personal CLI login. `doppler login` wrote the `/` entry. Inside the Rasm directory the project and config resolve from the scope entry, `doppler secrets`, `doppler run --`, and `doppler secrets download` need no `-p` or `-c`
2. The 1Password session cache. `1password.nix` lines 79-108 run `op inject` from `~/.config/op/env.template` into `~/.config/hm-op-session.sh` (mode 600) on every activation, interactive shells source it, and the `gui-op-secrets` launchd agent replays the same names into the GUI domain. The template holds eighteen `export` lines, including `BUF_TOKEN`, `RHINO_TOKEN`, `GITHUB_TOKEN`, `GH_TOKEN`, `GH_PROJECTS_TOKEN`, `TAVILY_API_KEY`, `EXA_API_KEY`, `CONTEXT7_API_KEY`, and `OP_SERVICE_ACCOUNT_TOKEN`

An agent working in Rasm inherits the second path ambiently and reaches the first by running `doppler`. After the `BUF_TOKEN` export leaves the template, a process that needs `BUF_TOKEN` runs under `doppler run` in the Rasm directory.

### [03.2]-[PROJECT_GRANULARITY]

<https://docs.doppler.com/docs/workplace-structure>: "It's assumed that a project is a one-to-one mapping to a specific application or service that needs secrets or config variables." <https://docs.doppler.com/docs/create-project>: "A project in Doppler is where you define the app config and secrets for a single service or application." The one-project-per-team shape is the documented anti-pattern: it "Scales poorly due to the 15 environment limit per project" and "incurs performance penalties … due to comparison operations that happen across environment on save that perform expensive cryptographic operations."

The four live projects sit at the documented granularity: `rasm` and `parametric-forge` are repositories, `maghz` is a host, `agent-runtime` is a runtime. That the Rasm project holds one secret says the estate has few runtime secrets, and not that the shape is wrong.

Limits as published: the workplace-structure page says 1000 projects and 15 environments per project on any plan, and the pricing page (<https://www.doppler.com/pricing>, 2026-08-25) lists Developer at 10 projects, 4 environments, 10 configs per environment, 50 service tokens, 5 config syncs, 5 webhooks. The two pages disagree, and the estate (4 projects, 2 environments per project after `stg` is removed, 1 service token for Rasm) is under both.

### [03.3]-[DELIVERY]

`doppler run --help` on v3.76.5:

- `doppler run [-p P] [-c C] -- <command>` injects secrets into the child process alone, and `--command "…"` runs a shell string
- `--mount <path>` writes "secrets to an ephemeral file, accessible at DOPPLER_CLI_SECRETS_PATH. when enabled, secrets are NOT injected into the environment", with `--mount-template` and `--mount-max-reads <n>`, and the secrets skill `[06]-[RULES]` prefers it
- `--preserve-env` lets an existing value win, `--only-secrets` narrows the set, `--name-transformer` includes `dotnet-env`

`doppler secrets download --no-file --format json` prints to stdout (`--no-file  print the response to stdout`), and without `--no-file` it writes an encrypted file.

The fallback cache is the one file written by default. `doppler run` has `--fallback`, `--no-fallback`, `--fallback-only`, `--fallback-readonly`, `--no-cache`, and `--passphrase`, and `doppler secrets download` has the same set with `--fallback-passphrase` in place of `--passphrase`. `~/.doppler/fallback/` holds six `.secrets-<sha256>.json` and six `.metadata-<sha256>.json` files, mode 400, the newest from 2026-09-03. They are encrypted, outside every repository, and never a `.env`.

## [04]-[GITHUB_ACTIONS]

### [04.1]-[ROUTE]

Doppler's own statement of the two integrations (<https://www.doppler.com/blog/github-actions-and-doppler-streamlining-your-ci-cd-pipelines>): "The primary way is via the Doppler<>GitHub sync integration, which works by syncing secrets from Doppler to GitHub Secrets whenever a change is made… The second way involves storing a Doppler service token (or service account API token) in GitHub Secrets and then using the Doppler CLI or Doppler Secrets Fetch Action to fetch the secrets directly from Doppler from inside the workflow."

Route B is the second: a Doppler service token held as the GitHub Actions secret `DOPPLER_TOKEN`, read in a workflow by `dopplerhq/cli-action@v4` with `doppler run` (<https://github.com/DopplerHQ/cli-action/blob/master/README.md> shows `uses: dopplerhq/cli-action@v4` and `DOPPLER_TOKEN: ${{ secrets.DOPPLER_TOKEN }}`, and the Doppler docs page still prints `@v3`). A service token "provides read-only secrets access to a specific config within a project" (<https://docs.doppler.com/docs/service-tokens>).

The sync integration is not used: it needs a browser step (authorizing the Doppler GitHub App at <https://docs.doppler.com/docs/github-actions>), `@pulumiverse/doppler` 0.9.0 has no GitHub integration resource (`integration/` holds `awsParameterStore`, `awsSecretsManager`, `circleci`, `flyio`, `terraformCloud` alone), and it writes three `DOPPLER_*` secrets on the GitHub side that Pulumi does not own. OIDC service-account identities are a Team-plan feature (<https://docs.doppler.com/docs/service-account-identities>: "This feature is available with our Team and Enterprise plans") and are out.

### [04.2]-[PROVIDER_RESOURCES]

`@pulumiverse/doppler` latest on npm is `0.9.0`, published 2024-08-27, and Forge pins it at `pnpm-workspace.yaml` line 20. From the installed `.d.ts`:

- `doppler.ServiceToken`: args `project`, `config`, `name`, optional `access` ("read" or "read/write"), output `key` is the token
- `doppler.Secret`: an individual secret value held in Pulumi state

`@pulumi/github` 6.14.0 (line 19, npm latest is 6.15.0) has `github.ActionsSecret` with args `repository`, `secretName`, one of `value` (plaintext, encrypted by the provider) or `valueEncrypted` with `keyId`, and `destroyOnDrift`, and `plaintextValue` and `encryptedValue` are marked `@deprecated`. `github.ActionsVariable` takes `repository`, `variableName`, `value`.

The route closes inside one program: `doppler.ServiceToken(...).key` feeds `github.ActionsSecret({ repository: 'Rasm', secretName: 'DOPPLER_TOKEN', value: token.key })`. No person copies the token and no file holds it.

## [05]-[HEADLESS_AND_SECOND_MACHINES]

A second machine, a self-hosted runner, headless Rhino, or the `maghz` VPS reaches Rasm secrets through a service token declared in IaC and delivered once.

Declaration: one `doppler.ServiceToken` row per consuming machine, one project and config, access `read`, name suffixed `-readonly` (`topology.ts` line 86). Replacement is drop the row and apply (revokes), restore the row and apply (issues), and `driver.ts` line 513 maps `<project>/<config>/<name>` onto the resource URN, a targeted apply touches nothing else.

Delivery: for GitHub-hosted runners `github.ActionsSecret` writes the token. For a machine, `node services/driver.ts outputs token:<project>/<config>/<name> --reveal` prints it once (`driver.ts` lines 540-563) and it is stored as a `Tokens` vault item in the shape of `DOPPLER_AGENT_READONLY`, `DOPPLER_FORGE_MACHINE_READONLY`, and `DOPPLER_MAGHZ_HOST_READONLY`. Rasm's program keeps the same `outputs --reveal` path.

Consumption on the far machine, neither a repository file: `export DOPPLER_TOKEN='dp.st.…'` then `doppler run -- <command>` (<https://docs.doppler.com/docs/service-tokens>, "Option 2"), or `echo 'dp.st.…' | doppler configure set token --scope /` (<https://docs.doppler.com/docs/cli>).

Headless Rhino reads `RHINO_TOKEN` from `rasm/dev_repo` through that token. The value is written once with `op read op://Tokens/RHINO_TOKEN/token | doppler secrets set RHINO_TOKEN --project rasm --config dev_repo`, and the 1Password item and its `env.template` export stay, because a person's local Rhino uses them.

## [06]-[STORE_SPLIT]

The secrets skill opens: "`op` owns permanent local and session storage. Doppler owns project configuration and explicit process delivery." `[05]-[STORAGE]`: "Local storage is `op`, never the OS keychain: every service, IaC, and MCP token and the SSH key live in a `Tokens` or `Personal` vault item", with `doppler login` as the one keychain exception. The storage table in `docs/atlas/secrets-and-services.md` `[01]` gives five classes, each with one origin, one movement path, and one boundary.

The decided rule, which the secrets skill will state: Doppler owns runtime secrets, the values a process reads at its execution boundary under a project and config, and 1Password keeps credentials a person uses, the values an interactive shell, a GUI application, or the IaC driver reads. A secret that both a person and a process use (`RHINO_TOKEN`) is held in both by that rule, and `BUF_TOKEN` is a runtime secret and leaves 1Password.

Creating a new secret: a runtime secret is written to its Doppler config with `doppler secrets set` and reached by `doppler run --project <p> --config <c>`, and a person's credential is a `Tokens` item under its published name with one `export NAME="op://Tokens/NAME/<field>"` line in `op/env.template`. Topology (a project, environment, branch config, token, or scope) is a row followed by an apply and never a CLI mutation (secrets skill `[06]-[RULES]`).

Rasm's program brokers its credentials as Forge's does (`driver.ts` lines 132-139, 154-164): a stack passphrase item in the `Tokens` vault named for the Rasm Pulumi project, the existing `op://Tokens/DOPPLER_IAC_TOKEN/token` for the Doppler provider, and the existing `op://Tokens/GITHUB_TOKEN/token` (ambient as `GITHUB_TOKEN` through the session cache) for `@pulumi/github`. The `GITHUB_TOKEN` item drives Forge's repository rows and `gh api` reads on Rasm, it is the reused token.

## [07]-[FORGE_ROWS]

Forge rows against live state:

| [INDEX] | [ROW]                                       | [FILE_LINE]           | [LIVE] | [VERDICT]                 |
| :-----: | :------------------------------------------ | :-------------------- | :----- | :------------------------ |
|  [01]   | `_projects` `rasm`, origin `adopt`          | `topology.ts` 26-30   | Yes    | Correct, never applied    |
|  [02]   | `_environments` `rasm.dev`/`.stg`/`.prd`    | derived, lines 49-54  | Yes    | Correct, never applied    |
|  [03]   | `_configs` `rasm`/`dev`/`dev_repo`, `adopt` | `topology.ts` 76      | Yes    | Correct, never applied    |
|  [04]   | `_scopes` Rasm dir → `rasm`/`dev_repo`      | `topology.ts` 113     | Yes    | Correct and in effect     |
|  [05]   | No `_tokens` row for `rasm`                 | `topology.ts` 87-102  | n/a    | Correct by absence        |
|  [06]   | No `_webhooks` row                          | `topology.ts` 135     | n/a    | Correct by absence        |
|  [07]   | `_repositories` `Rasm`, origin `adopt`      | `topology.ts` 146-150 | Yes    | Correct, never applied    |
|  [08]   | `_rulesets` empty                           | `topology.ts` 233     | n/a    | Correct, matches live     |
|  [09]   | `_appInstallations` Nx Cloud selects `Rasm` | `topology.ts` 216-222 | n/a    | Contradicted by `nx.json` |

`gh api repos/bsamiee/Rasm` returns `description: "AEC/design-geometry workspace"`, `allow_auto_merge: true`, `allow_merge_commit: false`, `allow_squash_merge: true`, `allow_rebase_merge: true`, `allow_update_branch: true`, `delete_branch_on_merge: true`, `squash_merge_commit_title: "PR_TITLE"`, `squash_merge_commit_message: "PR_BODY"`, `has_wiki: false`, `has_issues: true`, `has_projects: false`, `private: false`. Every field matches `_mergeHygiene` (`estate.ts` lines 29-41) and the description at `topology.ts` line 148, the repository row adopts with zero diff. `actions/secrets` and `actions/variables` return `total_count: 0`, and `rulesets` returns `[]`.

`nx.json` (`neverConnectToCloud: true`) contradicts the Nx Cloud installation grant (`topology.ts` line 220, `['Parametric_Portal', 'Rasm']`). The grant is browser-held in Forge's app inventory and is not a Rasm declaration, narrowing it is a Forge item for the removal memory.

`maghz` is live and undeclared in `topology.ts`, and the vault holds `DOPPLER_MAGHZ_HOST_READONLY` and `MAGHZ_MCP__DATABASE_URI`. It is outside Rasm and goes to the Forge memory as the same class of gap.

## [08]-[DECLARATIONS]

### [08.1]-[ADOPT]

| [INDEX] | [RESOURCE]             | [IDENTITY]                  | [IMPORT_ID]         |
| :-----: | :--------------------- | :-------------------------- | :------------------ |
|  [01]   | `doppler.Project`      | `rasm`                      | `rasm`              |
|  [02]   | `doppler.Environment`  | `rasm` / `dev`              | `rasm.dev`          |
|  [03]   | `doppler.Environment`  | `rasm` / `stg`              | `rasm.stg`          |
|  [04]   | `doppler.Environment`  | `rasm` / `prd`              | `rasm.prd`          |
|  [05]   | `doppler.BranchConfig` | `rasm` / `dev` / `dev_repo` | `rasm.dev.dev_repo` |
|  [06]   | `github.Repository`    | `Rasm`                      | `Rasm`              |

The import-ID forms are the ones `estate.ts` builds (lines 66-93 for Doppler, 122-131 for the repository) and the header note at `topology.ts` line 14 states. No adopt ever ran, the first `up --adopt` is the first test of them. Entry 6 has `protect: true` as `estate.ts` line 129 does.

Entry 3 exists to be removed: `stg` is imported in the first apply, its row is deleted, and the next apply destroys the live environment, the removal goes through IaC rather than the CLI. After that the project has `dev` and `prd`.

### [08.2]-[DECLARE]

| [INDEX] | [RESOURCE]             | [IDENTITY]                                          |
| :-----: | :--------------------- | :-------------------------------------------------- |
|  [07]   | `doppler.ServiceToken` | `rasm` / `dev_repo` / `rasm-ci-readonly`            |
|  [08]   | `github.ActionsSecret` | `Rasm` / `DOPPLER_TOKEN`, value from entry 7 `key`  |

The token binds to `dev_repo` because it is the one config with content and the one the directory scope resolves to, a workflow and a local shell read the same set. Declared before a workflow exists, by decision.

Secret values are not Pulumi rows. A `doppler.Secret` row copies the value into Pulumi state, a second store beside Doppler, and values are written with `doppler secrets set` against the declared config (`BUF_TOKEN` is there, `RHINO_TOKEN` is added for headless Rhino).

Not declared: no `doppler.ServiceAccount`, `ServiceAccountToken`, or identity (paid plan), no `doppler.secretssync.GithubActions`, no `github.RepositoryRuleset` (`topology.ts` line 232, live `[]`), no `github.ActionsVariable`, no `doppler.Webhook`, and no directory scope.

### [08.3]-[FORGE_REMOVAL]

The plan keeps the Forge removal list in a memory. Its Doppler and GitHub content:

| [INDEX] | [REMOVE]                                                           | [WHERE]                          |
| :-----: | :----------------------------------------------------------------- | :------------------------------- |
|  [01]   | `_projects` entry `rasm` (drops its three derived `_environments`) | `topology.ts` 26-30              |
|  [02]   | `_configs` entry `rasm`/`dev`/`dev_repo`                           | `topology.ts` 76                 |
|  [03]   | `_repositories` entry `Rasm`                                       | `topology.ts` 146-150            |
|  [04]   | `export BUF_TOKEN="op://Tokens/BUF_TOKEN/token"`                   | `1password.nix` 147              |
|  [05]   | The `BUF_TOKEN` item                                               | `Tokens` vault                   |
|  [06]   | CodeRabbit and Greptile links to Rasm                              | `.coderabbit.yaml`, `.greptile/` |
|  [07]   | `Rasm` from the Nx Cloud installation's `selectedRepositories`     | `topology.ts` 220, then browser  |

Keep in Forge: the `_scopes` row for the Rasm directory (line 113) and the whole scope machinery in `driver.ts`.

Compile-time consequences of removal 3: `_RepositoryName` loses `'Rasm'`, `'Rasm'` joins `'Parametric_Portal'` in `_AppRepositoryName` (line 159) until removal 7 lands, and `_reviewerMatrix` iterates `Topology.repositories` (`driver.ts` lines 420, 432), the matrix stops covering Rasm, which is the decided outcome (no reviewer configuration in Rasm).

Nothing is removed from Pulumi state, because the state holds nothing. Removal is a source edit.

### [08.4]-[ORDER]

1. Stand up the Rasm program under `infra/` with entries 1-6 and a stack passphrase item, and `preview --adopt` must plan six imports and zero property changes, the proof that the import-ID forms and `_mergeHygiene` are right
2. `up --adopt`, then remove the `stg` row and `up` again
3. Add entries 7-8 and `up`, and `preview --refresh --expect-no-changes` then reports `{"same":8}`
4. Write `RHINO_TOKEN` into `rasm/dev_repo`, and delete the `BUF_TOKEN` vault item and its `env.template` line
5. Delete Forge rows 1-3 (and the rest of the memory list), Forge's program never declares a Rasm resource

## [09]-[OPEN_QUESTIONS]

No question stays open, a plan decision or verification closed each one.
