# [IAC_DOPPLER]

The canonical secret owner of the deploy plane: one `Secrets` tier provisions the Doppler hierarchy (`Project → Environment → BranchConfig → Secret`), lands every generated credential in it, scopes one read-access `ServiceToken` for runtime injection, and serves every sibling provider's credential field through one parameterized fan-in read. Generation is provider-tracked entropy — `@pulumi/random` material under one char-class policy shape whose `keepers` map carries the spec `epoch`, so rotation is one field bump audited in state, never a per-resource knob. A value leaves Doppler exactly two ways: an in-graph single-key `Output<string>` bound to a provider credential `Input` (state-encrypted, never process env), or the `ServiceToken.key` as the `DOPPLER_TOKEN` env fact `secret/inject` assembles. External stores are mirrors, never sources — one `integration.<Target>` plus `secretssync.<Target>` pair per destination. The module is `iac/src/secret/doppler.ts`; a new secret is one entries row, a new consumer is one fan-in read, a new destination is one mirror pair row.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                              | [PUBLIC]  |
| :-----: | :---------------- | :-------------------------------------------------------------------- | :-------- |
|  [01]   | `STORE_HIERARCHY` | the parent-chained hierarchy, generated entries, and the token       | `Secrets` |
|  [02]   | `FAN_IN_AND_OUT`  | the single-key provider fan-in and the mirror fan-out pair           | `Secrets` |

## [2]-[STORE_HIERARCHY]

[STORE_HIERARCHY]:
- Owner: `Secrets`, one tier chaining `Project` (named by the spec's `doppler.project`), `Environment` (slugged by the spec's `doppler.config`), `BranchConfig`, then one `Secret` per entries row — every resource `parent`-chained through `child()` so the tier owns the whole store.
- Law: entries are a discriminated record — a row is `{ generate: seed }` (a partial policy the owner completes through `_Policy.make`, minting a `RandomPassword` with `keepers: { epoch }` and storing `pulumi.secret(password.result)`) or `{ value }` (an upstream secret-tracked `Output` — a `tls` CA key, a bootstrap artifact) — discriminated by field presence through `Predicate.hasProperty`, so sensitive-versus-supplied is one dispatch, never two entry surfaces, and a caller states only its policy deltas.
- Law: the char-class policy is one Schema shape — `length`, class toggles, per-class minimums, `overrideSpecial` — decoded once and shared by every generated row; `RandomString` never mints a credential, and the `bcryptHash` projection is available where a consumer stores a digest instead of the value.
- Law: the token is the only egress fact — `ServiceToken` with `access: "read"` scoped to the one config; `read/write` tokens exist only for provisioners and never leave the graph; the `key` output is sensitive by construction and crosses solely into `secret/inject`'s env assembly.
- Law: a missing read key aborts cleanly — `read(key)` resolves the whole-config map once and a key the config does not carry throws `pulumi.RunError` inside the apply, failing the run with the key named, never forging an empty string.
- Entry: `new Secrets("secrets", { spec, entries }, opts)` inside every dispatch arm; `secrets.read("DB_PASSWORD")` at any credential `Input`; `secrets.token` into `Inject.rows`.
- Growth: a new credential is one entries row; a new policy axis is one `_Policy` field with its default.
- Boundary: runtime consumption (`doppler run`, the `security/secret` read path) is `secret/inject.md`'s process boundary; generated-material laws (`keepers`, encodings) follow the random catalogue; the bootstrap `DOPPLER_TOKEN` for the provider plugin itself is deploy-host env under `doppler run`.
- Packages: `@pulumiverse/doppler` (`Project`, `Environment`, `BranchConfig`, `Secret`, `ServiceToken`, `getSecretsOutput`, the `integration`/`secretssync` namespaces); `@pulumi/random` (`RandomPassword`); `@pulumi/pulumi` (`Output`, `secret`, `RunError`); `effect` (`Schema`, `Predicate`, `Record`, `Array`, `Option`); `../program/spec.ts` (`StackSpec`); `../stack/component.ts` (`Tier`).

```typescript
import * as pulumi from "@pulumi/pulumi"
import * as random from "@pulumi/random"
import * as doppler from "@pulumiverse/doppler"
import * as integration from "@pulumiverse/doppler/integration"
import * as secretssync from "@pulumiverse/doppler/secretssync"
import { Array, Option, Predicate, Record, Schema } from "effect"
import type { StackSpec } from "../program/spec.ts"
import { Tier } from "../stack/component.ts"

const _Policy = Schema.Struct({
  length: Schema.optionalWith(Schema.Int.pipe(Schema.between(16, 128)), { default: () => 32 }),
  special: Schema.optionalWith(Schema.Boolean, { default: () => true }),
  minSpecial: Schema.optionalWith(Schema.Int.pipe(Schema.between(0, 8)), { default: () => 1 }),
  minNumeric: Schema.optionalWith(Schema.Int.pipe(Schema.between(0, 8)), { default: () => 1 }),
  minUpper: Schema.optionalWith(Schema.Int.pipe(Schema.between(0, 8)), { default: () => 1 }),
  overrideSpecial: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
})

declare namespace Secrets {
  type Policy = typeof _Policy.Type
  type Entry = { readonly generate: Partial<Policy> } | { readonly value: pulumi.Input<string> }
  type Args = { readonly spec: StackSpec; readonly entries: Record<string, Entry> }
}

const _minted = (key: string, policy: Secrets.Policy, epoch: string, child: pulumi.CustomResourceOptions): random.RandomPassword =>
  new random.RandomPassword(key, {
    length: policy.length,
    special: policy.special,
    minSpecial: policy.minSpecial,
    minNumeric: policy.minNumeric,
    minUpper: policy.minUpper,
    ...Option.match(policy.overrideSpecial, {
      onNone: () => ({}),
      onSome: (overrideSpecial) => ({ overrideSpecial }),
    }),
    keepers: { epoch },
  }, child)

class Secrets extends Tier {
  static readonly mirrored = (owner: Secrets, name: string, row: Secrets.Mirror, child: pulumi.CustomResourceOptions): pulumi.CustomResource =>
    row.target === "awsSecretsManager"
      ? new secretssync.AwsSecretsManager(name, {
          project: owner.project.name,
          config: owner.config.name,
          integration: new integration.AwsSecretsManager(name, { name, assumeRoleArn: row.assumeRoleArn }, child).id,
          region: row.region,
          path: row.path,
        }, child)
      : new secretssync.GithubActions(name, {
          project: owner.project.name,
          config: owner.config.name,
          integration: row.integration,
          syncTarget: "repo",
          repoName: row.repoName,
        }, child)
  readonly project: doppler.Project
  readonly config: doppler.BranchConfig
  readonly token: pulumi.Output<string>
  constructor(name: string, args: Secrets.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Secrets", name, opts)
    this.project = new doppler.Project(name, { name: args.spec.doppler.project }, this.child())
    const environment = new doppler.Environment(name, {
      name: args.spec.doppler.config,
      slug: args.spec.doppler.config,
      project: this.project.name,
    }, this.child())
    this.config = new doppler.BranchConfig(name, {
      project: this.project.name,
      environment: environment.slug,
    }, this.child())
    Array.map(Record.toEntries(args.entries), ([key, entry]) =>
      new doppler.Secret(key, {
        project: this.project.name,
        config: this.config.name,
        name: key,
        value: Predicate.hasProperty(entry, "generate")
          ? pulumi.secret(_minted(key, _Policy.make(entry.generate), args.spec.epoch, this.child()).result)
          : entry.value,
      }, this.child()))
    this.token = new doppler.ServiceToken(name, {
      project: this.project.name,
      config: this.config.name,
      name: `${name}-runtime`,
      access: "read",
    }, this.child()).key
    this.seal({ project: this.project.name, config: this.config.name })
  }
  read(key: string): pulumi.Output<string> {
    return doppler.getSecretsOutput({ project: this.project.name, config: this.config.name }, { parent: this })
      .apply((result) => result.map[key] ?? (() => { throw new pulumi.RunError(`<missing-secret:${key}>`) })())
  }
}
```

## [3]-[FAN_IN_AND_OUT]

[FAN_IN_AND_OUT]:
- Law: one read serves every provider — `read(key)` is the single-key pluck over `getSecretsOutput(...).map`, and the consumer roster is data: `postgresql.Provider.password` ← `DB_ADMIN_PASSWORD` (the CNPG superuser the admin secret governs), `postgresql.Role.password` ← `DB_PASSWORD` (the app role's own entry), `cloudflare.Provider.apiToken` ← `CLOUDFLARE_API_TOKEN`, `gcp.Provider.credentials` ← `GCP_CREDENTIALS`, `grafana.Provider` auth ← `GRAFANA_PASSWORD`, `docker` registry auth ← `REGISTRY_PASSWORD` — a new consuming provider is a key row on the config the store already owns, never a new read path.
- Law: the in-graph value stays in the graph — the plucked `Output<string>` binds a credential `Input`, is state-encrypted, and never touches process env; the env mode exists only for the runtime processes `secret/inject` wraps.
- Law: mirroring is one pair per destination — `Secrets.mirrored` dispatches on the target key: a destination whose credential link this plane owns constructs `integration.<Target>` then `secretssync.<Target>` referencing its id; a GitHub destination's integration is the pre-existing GitHub-App install, so its row carries the integration slug and constructs the sync row alone (`syncTarget: "repo"`, `repoName` naming the repository); targets are rows (`AwsSecretsManager` with `region`/`path`/`kmsKeyId`/`deleteBehavior`, `GithubActions` with the install slug and repo coordinates), and the mirror is always FROM the canonical config outward.
- Law: `mirrored` rides the owner as a static — data-first over the tier, so one dispatch owns every target, the module exports one name, and the tier grows no per-target methods.
- Growth: a new mirror destination is one target row in the `mirrored` dispatch; removing one is deleting the pair.
- Boundary: what a mirrored store's consumers do with the copy is theirs; the canonical write path never routes through a mirror.

```typescript
declare namespace Secrets {
  type Mirror =
    | { readonly target: "awsSecretsManager"; readonly assumeRoleArn: pulumi.Input<string>; readonly region: pulumi.Input<string>; readonly path: pulumi.Input<string> }
    | { readonly target: "githubActions"; readonly integration: pulumi.Input<string>; readonly repoName: pulumi.Input<string> }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Secrets }
```
