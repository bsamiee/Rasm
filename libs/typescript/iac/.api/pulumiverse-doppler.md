# [TS_IAC_API_PULUMIVERSE_DOPPLER]

`@pulumiverse/doppler` is the canonical secret owner of the deploy plane: the `Project → Environment → Config → Secret` hierarchy provisions the secret store, `ServiceToken` scopes a config for `doppler run` runtime injection, `getSecrets` reads a whole config as an `Output` map, and the `integration`/`secretssync` namespaces mirror a config to N external destinations. `iac` composes it as the store where `random`/`tls`-generated material lands canonically and the `ServiceToken.key` that meets the `security/secret` read path at the process boundary — never an import in either direction. The Project→Config→Secret nesting is ONE parent-chained owner pattern; the destination fan-out is ONE integration+sync pair parameterized by target.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumiverse/doppler`
- package: `@pulumiverse/doppler`
- module: `@pulumiverse/doppler` (root), `@pulumiverse/doppler/{secretssync,integration,projectmember}` (namespaces)
- license: `Apache-2.0`
- asset: Doppler project/config/secret provisioning, service tokens, RBAC, webhooks, external sync destinations
- runtime: `node` — Terraform-bridge provider plugin auto-downloads on first registration; bootstrap token via `Provider({ dopplerToken })` or the `DOPPLER_TOKEN` env
- rail: secret

## [02]-[RESOURCE_SURFACE]

Every resource extends `pulumi.CustomResource` with `static get`/`isInstance` + `constructor(name, args, opts?)`. `Secret.value` is state-encrypted sensitive; `ServiceToken.key` is the sensitive token.

[PUBLIC_TYPE_SCOPE]: store hierarchy
- rail: secret

| [INDEX] | [SYMBOL] | [REQUIRED_ARGS] | [KEY_OUTPUTS] | [NOTE] |
|:-----: |:-------------- |:--------------------------------------- |:------------------------------ |:-------------------------------------- |
| [01] | `Project` | `name` | id | top of the hierarchy; `description` |
| [02] | `Environment` | `name`, `project`, `slug` | id | dev/stg/prd stage; `personalConfigs` |
| [03] | `BranchConfig` | `project`, `environment` | id | the "config" — a branch of an environment |
| [04] | `Secret` | `project`, `config`, `name`, `value` | `value` (secret), `computed` | `computed` resolves `${ref}` interpolation; `visibility` |

[PUBLIC_TYPE_SCOPE]: access, identity, RBAC, delivery
- rail: secret

| [INDEX] | [SYMBOL] | [REQUIRED_ARGS] | [NOTE] |
|:-----: |:----------------------------------------- |:--------------------------------------- |:----------------------------------------------- |
| [01] | `ServiceToken` | `project`, `config`, `name` | `key` = the `DOPPLER_TOKEN`; `access` = `read`\|`read/write` |
| [02] | `ServiceAccount` / `ServiceAccountToken` | `name` / (account + token) | machine identity; `workplaceRole`/`workplacePermissions` |
| [03] | `Group` / `GroupMember` / `GroupMembers` | membership args | workplace group + membership rows |
| [04] | `ProjectRole` / `projectmember.{Group,ServiceAccount}` | role/member args | project-scoped RBAC binding |
| [05] | `Webhook` | `url`, `project` | delivery on secret change; `authentication`/`enabledConfigs`/`payload` |
| [06] | `Provider` | — | explicit provider; bootstrap token |

[PUBLIC_TYPE_SCOPE]: reads (data sources)
- rail: secret

| [INDEX] | [SURFACE] | [MODE] | [NOTE] |
|:-----: |:----------------------------------------------- |:-------- |:------------------------------------------- |
| [01] | `getSecrets({project, config})` | `Promise` | whole-config read → `map: {[name]: value}` |
| [02] | `getSecretsOutput(...)` | `Output` | Input-accepting mirror of `getSecrets` |
| [03] | `getUser` / `getUserOutput` | both | workplace user lookup |

## [03]-[SECRET_TOPOLOGY]

Three parameterized patterns own the surface; the namespace rosters are seed data feeding them.

[PATTERN]: nested-owner hierarchy — ONE parent chain
- `Project` → `Environment(project)` → `BranchConfig(project, environment)` → `Secret(project, config)`. Chain them with `{ parent }` so the ComponentResource tier owns the whole store; a per-app store is one `Project` with the environment set its `StackSpec` names.

[PATTERN]: config-scoped access — the injection token family
- `ServiceToken(config, access)` mints `key` (the `DOPPLER_TOKEN`) scoped to ONE config; `read` for consumers, `read/write` only for provisioners. `ServiceAccount`/`ServiceAccountToken` give a durable machine identity. `key` is the sole value that leaves Doppler, and it leaves as an env fact for `doppler run`, never a secret payload.

[PATTERN]: destination fan-out — ONE integration + sync pair per target
- Mirroring a config outward is a pair: `integration.<Target>` creates the credential link (returns an integration id), `secretssync.<Target>` syncs a `config` to the destination referencing that `integration`. The targets are rows, not recipes:

| [INDEX] | [TARGET] | [INTEGRATION_T] | [SECRETSSYNC_T] | [SYNC_ARG_SHAPE] |
|:-----: |:------------------ |:------------------ |:------------------ |:---------------------------------------- |
| [01] | `AwsSecretsManager` | ✓ (`assumeRoleArn`) | ✓ | `region`, `path`, `kmsKeyId`, `deleteBehavior`, `tags` |
| [02] | `AwsParameterStore` | ✓ | ✓ | `region`, `path`, tags |
| [03] | `TerraformCloud` | ✓ | ✓ | workspace/variable-set target |
| [04] | `Circleci` | ✓ | ✓ | project/context target |
| [05] | `Flyio` | ✓ | ✓ | app target |
| [06] | `GithubActions` | — (GitHub-App install; slug pre-exists) | ✓ | `integration` slug REQUIRED + `syncTarget` (`"repo"`\|`"org"`); `repoName`/`environmentName` or `orgScope`, `deleteBehavior` |

Every sync arg carries `config` + `integration` + `project`; document/drive the pair once parameterized by `<Target>`, not six times.

## [04]-[INTEGRATION]

Doppler is the canonical store in the generate → store → inject rail; `effect` owns the bootstrap token, the provider `Layer`, and the config decode. Doppler is equally the SOURCE side of the provider-credential seam every sibling provider catalog names inbound — one config read supplies each `Provider` auth `pulumi.Input<string>`, closing the seam on both sides.

[RAIL]: `doppler → effect + sibling providers`

| [INDEX] | [DOPPLER_SEAM] | [STACKS_WITH] | [COMPOSED_RAIL] |
|:-----: |:------------------------------------ |:------------------------------------------- |:--------------------------------------------------------- |
| [01] | `Secret.value` (secret) | `@pulumi/random` `.result` / `@pulumi/tls` `.privateKeyPem` | `pulumi.secret(generated)` → canonical store — one source of truth |
| [02] | bootstrap + `Provider` | `Config.redacted` + `Layer.effect` | `DOPPLER_TOKEN` from an Effect `Config`, provider wired as a `Layer` |
| [03] | `ServiceToken.key` | `security/secret` read path (`doppler run`) | env injection at the process boundary; never imported |
| [04] | `getSecrets(config).map` | `Schema.decodeUnknown(AppSecrets)` | whole-config read → typed app config |
| [05] | `secretssync.<Target>` | external stores (AWS/CI/Fly) | mirror the canonical config outward; one pair per target |
| [06] | `Secret.computed` (`${ref}`) | `interpolate` / `Output` graph | referenced/composed secrets resolve server-side |

[SEAM]: provider-credential fan-in — ONE Doppler read → every sibling `Provider` `Input<string>`

Doppler is the SOURCE each sibling provider catalog binds its auth field to; ONE parameterized read feeds them all, consumers as rows, never per-provider code. In-graph binders pluck a single key as an `Output<string>` that stays in the Pulumi graph (state-encrypted, never process env): `getSecretsOutput({ project, config }).apply(r => r.map[KEY])` → `Output<string>` → `pulumi.Input<string>`. Env binders read the same config after `doppler run` injects it: `ServiceToken(config, "read").key` → `DOPPLER_TOKEN` → `doppler run -- <host>` → `process.env[KEY]` → `Config.redacted` → `Input<string>`.

| [INDEX] | [CONSUMER] | [CATALOG] | [INJECTION_MODE] |
|:-----: |:----------------------------------------------- |:---------------------------- |:-------------------------------------- |
| [01] | `grafana.Provider.auth` | `.api/pulumiverse-grafana.md` | env (`doppler run` → `Config.redacted`) |
| [02] | `cloudflare.Provider.apiToken` | `.api/pulumi-cloudflare.md` | in-graph `Output` bind |
| [03] | `gcp.Provider.credentials` | `.api/pulumi-gcp.md` | in-graph `Output` bind (SA-key JSON) |
| [04] | `postgresql.Provider.password` | `.api/pulumi-postgresql.md` | in-graph `Output` bind (config read) |
| [05] | `docker` `types.input.Registry.password` | `.api/pulumi-docker.md` | in-graph `Output` bind |
| [06] | `aws.Provider` creds (`profile`/`assumeRoles`) | `.api/pulumi-aws.md` | `StackSpec` Doppler ref |

The `KEY` is a row on the config the store already owns; a new consuming provider is a row here, never a new read path. `getSecretsOutput` returns `Output<GetSecretsResult>` whose `.map` is `{[k]: string}` — the single-key pluck is the sole difference from the row [04] whole-config `Schema` decode.

```ts contract
// iac/secret/doppler.ts — generate → store canonically → scope a token → hand ONE key to a sibling Provider
const cfg = new doppler.BranchConfig("prd", { project: proj.name, environment: env.slug }, { parent })
new doppler.Secret("db-password", {
  project: proj.name, config: cfg.name, name: "DB_PASSWORD",
  value: pulumi.secret(dbPassword.result),              // ← random/tls material, single source of truth
}, { parent })
const token = new doppler.ServiceToken("prd-app", {
  project: proj.name, config: cfg.name, name: "app", access: "read",
}, { parent })
// token.key → DOPPLER_TOKEN env for `doppler run`; meets security/secret at the boundary
const grafanaAuth = doppler.getSecretsOutput({ project: proj.name, config: cfg.name })
  .apply(r => r.map["GRAFANA_TOKEN"])                    // Output<string> → grafana.Provider.auth (Input<string>)
```

## [05]-[IMPLEMENTATION_LAW]

[SECRET_TOPOLOGY]:
- Doppler is the system of record for secret values; `random`/`tls` generate, Doppler stores, apps read via `doppler run` — a value is never duplicated into a second store except through a `secretssync.<Target>` mirror.
- `Secret.value` and `ServiceToken.key` are sensitive; a Doppler value leaves the store two ways only — an in-graph single-key `getSecretsOutput(...).apply(r => r.map[KEY])` `Output<string>` bound to a sibling `Provider` credential `Input` (state-encrypted, stays in the Pulumi graph), or `key` as the `DOPPLER_TOKEN` env for `doppler run` at the process boundary — never a decrypted payload in an import.
- Doppler is the source BOTH sides of the provider-credential seam: every sibling `Provider` auth field (`grafana.auth`, `cloudflare.apiToken`, `gcp.credentials`, `postgresql`/`docker` `password`, `aws` creds) is fed by one config read, the consumers rows on the [04] fan-in, never per-provider read paths.
- Scope `ServiceToken.access` to `read` for consumers; reserve `read/write` for provisioners.

[RAIL_LAW]:
- Package: `@pulumiverse/doppler`
- Owns: project/environment/config/secret provisioning, service tokens, RBAC, webhooks, external sync destinations
- Accept: `pulumi.secret`-tracked generated material as `Secret.value`; the bootstrap `DOPPLER_TOKEN` from `Config.redacted`; `getSecrets.map` decoded through `Schema`; a single-key `getSecretsOutput(...).apply(r => r.map[KEY])` `Output<string>` bound to a sibling `Provider` credential `Input<string>`; the `parent` chain for the hierarchy
- Reject: a plaintext secret value in source; a sibling `Provider` credential as a literal or a second-sourced value where the Doppler read is canonical; a second store as the source of truth (mirror via `secretssync` instead); the `DOPPLER_TOKEN` in state as anything but redacted; six per-target sync (or per-consumer read) code paths where one parameterized pattern dispatches on `<Target>`/`KEY`
