# [TS_IAC_API_PULUMIVERSE_DOPPLER]

`@pulumiverse/doppler` is the canonical secret owner of the deploy plane: the `Project → Environment → BranchConfig → Secret` chain provisions the store, `ServiceToken` scopes a config to mint the `DOPPLER_TOKEN` for `doppler run` injection, `getSecrets` reads a whole config as an `Output` map, and the `integration`/`secretssync` namespaces mirror a config to N external destinations. `iac` composes it as the store where `random`/`tls`-generated material lands canonically and the source every sibling `Provider` credential binds to — never an import in either direction.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumiverse/doppler`
- package: `@pulumiverse/doppler` (Apache-2.0)
- module: `@pulumiverse/doppler` (root), `@pulumiverse/doppler/{secretssync,integration,projectmember}` (namespaces)
- asset: Doppler project/config/secret provisioning, service tokens, RBAC, webhooks, external sync destinations
- runtime: `node` — Terraform-bridge provider plugin auto-downloads on first registration; bootstrap token via `Provider({ dopplerToken })` or the `DOPPLER_TOKEN` env
- rail: secret

## [02]-[RESOURCE_SURFACE]

Every resource extends `pulumi.CustomResource` with `static get`/`isInstance` + `constructor(name, args, opts?)` and surfaces its `id`. `Secret.value` is state-encrypted sensitive; `ServiceToken.key` is the sensitive token.

[PUBLIC_TYPE_SCOPE]: store hierarchy

| [INDEX] | [SYMBOL]       | [REQUIRED_ARGS]                      | [NOTE]                                     |
| :-----: | :------------- | :----------------------------------- | :----------------------------------------- |
|  [01]   | `Project`      | `name`                               | top of the hierarchy; `description`        |
|  [02]   | `Environment`  | `name`, `project`, `slug`            | dev/stg/prd stage; `personalConfigs`       |
|  [03]   | `BranchConfig` | `project`, `environment`             | the config — a branch of an environment    |
|  [04]   | `Secret`       | `project`, `config`, `name`, `value` | `computed` resolves `${ref}`; `visibility` |

[PUBLIC_TYPE_SCOPE]: access, identity, RBAC, delivery

| [INDEX] | [SYMBOL]                                 | [REQUIRED_ARGS]             | [NOTE]                                                       |
| :-----: | :--------------------------------------- | :-------------------------- | :----------------------------------------------------------- |
|  [01]   | `ServiceToken`                           | `project`, `config`, `name` | `key` = the `DOPPLER_TOKEN`; `access` = `read`\|`read/write` |
|  [02]   | `ServiceAccount` / `ServiceAccountToken` | `name` / (account + token)  | machine identity; `workplaceRole`/`workplacePermissions`     |
|  [03]   | `Group` / `GroupMember` / `GroupMembers` | membership args             | workplace group + membership rows                            |
|  [04]   | `ProjectRole`                            | role args                   | project-scoped RBAC binding                                  |
|  [05]   | `projectmember.{Group,ServiceAccount}`   | member args                 | project-member RBAC binding                                  |
|  [06]   | `Webhook`                                | `url`, `project`            | on change; `authentication`/`enabledConfigs`/`payload`       |
|  [07]   | `Provider`                               | —                           | explicit provider; bootstrap token                           |

[PUBLIC_TYPE_SCOPE]: reads (data sources)

| [INDEX] | [SURFACE]                       | [MODE]    | [NOTE]                                     |
| :-----: | :------------------------------ | :-------- | :----------------------------------------- |
|  [01]   | `getSecrets({project, config})` | `Promise` | whole-config read → `map: {[name]: value}` |
|  [02]   | `getSecretsOutput(...)`         | `Output`  | Input-accepting mirror of `getSecrets`     |
|  [03]   | `getUser` / `getUserOutput`     | both      | workplace user lookup                      |

## [03]-[SECRET_TOPOLOGY]

Three parameterized patterns own the surface; the target roster is seed data feeding them.

[PATTERN]: nested-owner hierarchy — ONE parent chain
- `Project` → `Environment(project)` → `BranchConfig(project, environment)` → `Secret(project, config)`, chained with `{ parent }` so the ComponentResource tier owns the whole store; a per-app store is one `Project` with the environment set its `StackSpec` names.

[PATTERN]: config-scoped access — the injection token family
- `ServiceToken(config, access)` mints `key` (the `DOPPLER_TOKEN`) scoped to ONE config; `read` for consumers, `read/write` for provisioners. `ServiceAccount`/`ServiceAccountToken` give a durable machine identity. `key` is the sole value leaving Doppler, and it leaves as an env fact for `doppler run`, never a secret payload.

[PATTERN]: destination fan-out — ONE integration + sync pair per target
- Mirroring a config outward is a pair: `integration.<Target>` creates the credential link (returns an integration id), `secretssync.<Target>` syncs a `config` to the destination referencing that `integration`. Every target carries both, except `GithubActions` whose integration is a pre-existing GitHub-App slug.

| [INDEX] | [TARGET]            | [INTEGRATION_T]     | [SYNC_ARG_SHAPE]                                           |
| :-----: | :------------------ | :------------------ | :--------------------------------------------------------- |
|  [01]   | `AwsSecretsManager` | ✓ (`assumeRoleArn`) | `region`, `path`, `kmsKeyId`, `deleteBehavior`, `tags`     |
|  [02]   | `AwsParameterStore` | ✓                   | `region`, `path`, `tags`                                   |
|  [03]   | `TerraformCloud`    | ✓                   | workspace/variable-set target                              |
|  [04]   | `Circleci`          | ✓                   | project/context target                                     |
|  [05]   | `Flyio`             | ✓                   | app target                                                 |
|  [06]   | `GithubActions`     | — (GitHub-App slug) | `syncTarget` (`"repo"`\|`"org"`); repo/org scope, see note |

Every sync arg carries `config` + `integration` + `project`. `GithubActions` [06] takes the pre-existing `integration` slug, then `syncTarget` = `"repo"`\|`"org"` selecting `repoName`/`environmentName` or `orgScope`, with `deleteBehavior`.

## [04]-[INTEGRATION]

Doppler is the canonical store in the generate → store → inject rail; `effect` owns the bootstrap token, provider `Layer`, and config decode.

[RAIL]: `doppler → effect + sibling providers`

| [INDEX] | [DOPPLER_SEAM]               | [STACKS_WITH]                      | [COMPOSED_RAIL]                                               |
| :-----: | :--------------------------- | :--------------------------------- | :------------------------------------------------------------ |
|  [01]   | `Secret.value` (secret)      | `@pulumi/random` / `@pulumi/tls`   | `pulumi.secret(`.result`/`.privateKeyPem`)` → canonical store |
|  [02]   | bootstrap + `Provider`       | `Config.redacted` + `Layer.effect` | `DOPPLER_TOKEN` via `Config`; provider as a `Layer`           |
|  [03]   | `ServiceToken.key`           | `security/secret` (`doppler run`)  | env injection at the process boundary; never imported         |
|  [04]   | `getSecrets(config).map`     | `Schema.decodeUnknown(AppSecrets)` | whole-config read → typed app config                          |
|  [05]   | `secretssync.<Target>`       | external stores (AWS/CI/Fly)       | mirror the canonical config outward; one pair per target      |
|  [06]   | `Secret.computed` (`${ref}`) | `interpolate` / `Output` graph     | referenced/composed secrets resolve server-side               |

[SEAM]: provider-credential fan-in — ONE Doppler read feeds every sibling `Provider` auth `Input<string>`

Doppler is the source both ends of the seam each sibling catalog names inbound; one parameterized read binds them all, consumers as rows, never per-provider read paths. `getSecretsOutput({project, config})` returns `Output<GetSecretsResult>` whose `.map` is `{[k]: string}` — a single-key pluck is the sole difference from the whole-config `Schema` decode, and a new consuming provider is a row here.

| [INDEX] | [CONSUMER]                                     | [CATALOG]                     | [INJECTION_MODE]                        |
| :-----: | :--------------------------------------------- | :---------------------------- | :-------------------------------------- |
|  [01]   | `grafana.Provider.auth`                        | `.api/pulumiverse-grafana.md` | env (`doppler run` → `Config.redacted`) |
|  [02]   | `cloudflare.Provider.apiToken`                 | `.api/pulumi-cloudflare.md`   | in-graph `Output` bind                  |
|  [03]   | `gcp.Provider.credentials`                     | `.api/pulumi-gcp.md`          | in-graph `Output` bind (SA-key JSON)    |
|  [04]   | `postgresql.Provider.password`                 | `.api/pulumi-postgresql.md`   | in-graph `Output` bind (config read)    |
|  [05]   | `docker` `types.input.Registry.password`       | `.api/pulumi-docker.md`       | in-graph `Output` bind                  |
|  [06]   | `aws.Provider` creds (`profile`/`assumeRoles`) | `.api/pulumi-aws.md`          | `StackSpec` Doppler ref                 |

[SURFACES]: `cfg = new doppler.BranchConfig("prd",{project:proj.name,environment:env.slug},{parent})` `token = new doppler.ServiceToken("prd-app",{project:proj.name,config:cfg.name,name:"app",access:"read"},{parent})` `grafanaAuth = doppler.getSecretsOutput({project:proj.name,config:cfg.name}).apply(r=>r.map["GRAFANA_TOKEN"])`
[COMPOSITION]: `RandomPassword.result -> doppler.Secret.value`

## [05]-[IMPLEMENTATION_LAW]

[SECRET_TOPOLOGY]:
- `random`/`tls` generate, Doppler stores, apps read via `doppler run`; a value duplicates into a second store only through a `secretssync.<Target>` mirror.
- `Secret.value` and `ServiceToken.key` are sensitive; a value leaves two ways only — an in-graph `getSecretsOutput(...).apply(r => r.map[KEY])` `Output<string>` bound to a sibling `Provider` credential `Input` (state-encrypted, stays in the Pulumi graph), or `key` as the `DOPPLER_TOKEN` env for `doppler run` at the process boundary — never a decrypted payload in an import.
- Scope `ServiceToken.access` to `read` for consumers, `read/write` for provisioners.

[RAIL_LAW]:
- Package: `@pulumiverse/doppler`
- Owns: project/environment/config/secret provisioning, service tokens, RBAC, webhooks, external sync destinations
- Accept: `pulumi.secret`-tracked generated material as `Secret.value`; the bootstrap `DOPPLER_TOKEN` from `Config.redacted`; `getSecrets.map` decoded through `Schema`; a single-key `getSecretsOutput` pluck bound to a sibling `Provider` credential `Input<string>`; the `parent` chain for the hierarchy
- Reject: a plaintext secret value in source; a sibling `Provider` credential as a literal or second-sourced value where the Doppler read is canonical; a second store as source of truth (mirror via `secretssync`); the `DOPPLER_TOKEN` in state as anything but redacted; per-target sync or per-consumer read code paths where one pattern dispatches on `<Target>`/`KEY`
