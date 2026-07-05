# [IAC_SPEC]

The program shapes of the deploy plane in one page: `StackSpec` is the decoded value an app supplies to deploy — the closed arm union with its promotion-tier column, the capability profile with its tenancy and compute axes, region/domain/zone coordinates, the backend selector, the Doppler ref, the rotation epoch — and the coordinates-never-material law makes every spec value loggable, diffable, and receipt-safe. `Connection` is the one owner of the SSH coordinate product: host, user, and port decode once and the `ssh` projection is the only spelling of the daemon URL, so no arm hand-builds `ssh://` strings. `Tier` is the one boundary adapter over Pulumi's class-based resource model: every grouped concern is a `Tier` subclass under the `rasm:iac:` token scope with one inherited option fold and one terminal output registration, so the resource DAG is recoverable from the tier tree alone. `StackOutputs` is the typed exit — every value a deployed stack publishes decodes once through one `Schema.Class` of `Option`-carried planes, the secret-refusal gate holds the raw `OutputMap` at this seam, and `pairsOf` is the single channel-flatten owner both the decoded getter and the in-program live-`Output` assembly ride, parameterized on the value renderer, so the `<plane>.<field>` spelling exists exactly once. The `sharding` plane is the sole value crossing back into the runtime graph — `work`'s `ShardingConfig.layerFromEnv` reads the env rows those channels populate — so the deploy plane and the runtime meet at the process boundary and never import each other. The module is `iac/src/program/spec.ts`; a new cloud is one arm entry plus one tier row, a new deployment axis is one profile field with its default, a new published plane is one `Option` field whose channels derive from the field record with nothing else to edit.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                              | [PUBLIC]       |
| :-----: | :--------------- | :------------------------------------------------------------------ | :------------- |
|  [01]   | `ARM_VOCABULARY` | the closed arm tuple, the promotion-tier column, the derived unions | `StackSpec`    |
|  [02]   | `SPEC_OWNER`     | the app-supplied value: coordinates, profile, tenancy, defaults     | `StackSpec`    |
|  [03]   | `TIER_BASE`      | the abstract component owner: token scope, option fold, seal, roster | `Tier`         |
|  [04]   | `OUTPUT_PLANES`  | the decoded exit, the secret gate, the one channel-flatten owner    | `StackOutputs` |

## [2]-[ARM_VOCABULARY]

[ARM_VOCABULARY]:
- Owner: the interior `_arms` key tuple plus the `_tiers` column — order, iteration, and the non-empty `Schema.Literal` spread are tuple facts stated once; `StackSpec.Arm` derives on the interior anchor, `StackSpec.arms`/`StackSpec.tiers` ride the class as statics, and the arm roster has one edit site branch-wide.
- Law: the tier column is the promotion vocabulary — `selfhosted-k8s` is `primary` (the first-class arm every capability row realizes), `selfhosted-docker` is `realized` (a live arm over an owned daemon), and `aws`/`gcp`/`cloudflare` are `prepared` (live provider seams whose capability realization lands when an app finalizes with a spec value); `StackSpec.primary` projects the row so dispatch and policy read promotion as data, never a name comparison.
- Growth: a new cloud is one `_arms` entry plus one `_tiers` row — the provider record, the equivalence map, and the `Schema.Literal` admission all break at compile time until their rows land.
- Boundary: which resources an arm composes and which program body runs are `provider.md`'s record and map.

## [3]-[SPEC_OWNER]

[SPEC_OWNER]:
- Owner: `StackSpec`, one `Schema.Class` — `name` (a DNS-safe stack slug brand), `app` (the core `AppKey` brand composed as `AppIdentity.fields.app`, so app identity has one spelling branch-wide), `target` (the arm literal), `backend` (`self-managed` gates the local drift sweep and ephemeral bracket; `cloud` gates the `operate/cloud.md` control plane and the `RemoteWorkspace` execution row), the coordinate options (`region` for prepared clouds, `domain`/`zone` for the traffic rows, `project` for the gcp project scope, `account` for the Cloudflare account scope, `connection` for the selfhosted bootstrap, `image` for the app workload ref), the `doppler` project/config ref, the `epoch` rotation trigger, and the `profile` capability record.
- Law: coordinates, never material — `Connection` carries host/user/port and no key field; the SSH private key, provider tokens, and generated passwords travel the provider material read or the in-graph Doppler fan-in, so a spec value never leaks into state, receipt, or log; the `ssh` getter on `Connection` is the one spelling of the daemon URL every consumer reads.
- Law: `epoch` is the one rotation trigger — it feeds every `@pulumi/random` `keepers` map and every `@pulumi/command` `triggers` list, so bumping one field re-mints credentials and re-runs bootstrap deliberately; per-resource rotation knobs are the named defect.
- Law: the profile is defaults-total — `scale` selects the `kube/workload` sizing row, `compute` selects the prepared-arm workload posture (`serverless` = the managed container cell, `cluster` = the managed-Kubernetes escalation that reuses the whole `kube/*` roster), `extensions` names the `data` extension-matrix subset the data tier finalizes (validated against `Pg.rows` at `kube/data.md`, never here), `objectEngine` selects a conditional-put-conforming self-host row (`minio` = the maintained continuation image, `ceph` = the RGW row; the engine that cannot CAS has no literal to select), `exposure` selects the direct-DNS-versus-tunnel traffic row, `data` carries instance count, storage, backup cron, and retention, `fanout` carries the NATS replica quorum and stream storage, and `tenancy` carries the isolation posture — every field defaulted at the declaration so `_Profile.make({})` is a complete standard deployment and an app states only its deltas.
- Law: tenancy is data, never code paths — `tenancy.mode` selects the isolation tier (`single` = one app one namespace; `namespace` = Capsule-governed namespace-per-tenant soft isolation; `vcluster` = virtual-control-plane-per-tenant hard isolation), `tenancy.pgTier` selects the data-plane escalation (`shared-rls` = one database with `Tenancy.rls` policy rows; `db-per-tenant` = one CNPG cluster with one `Database` CR per tenant; `cluster-per-tenant` = one CNPG `Cluster` per tenant), and `tenancy.tenants` names the tenant slugs the `kube/tenant.md` owner realizes rows for; a tenancy escalation is a spec delta interpreted by the owning tiers, never a second program body.
- Law: absence is `Option` admitted by `Schema.optionalWith(..., { as: "Option" })` — a prepared arm demanding an absent `region`, or a selfhosted arm demanding an absent `connection`, fails as a typed `DeployFault` inside its provider arm before the `PulumiFn` is entered, never as an `undefined` read and never as a construction-time throw inside a tier.
- Entry: `StackSpec.make(...)` at the app seam; `Schema.decodeUnknown(StackSpec)` where the value arrives as data.
- Growth: a new coordinate is one field with its dialect chosen here; a new profile axis is one `_Profile` field with its default; a new tenancy tier is one literal plus its interpreting row in the owning tier.
- Boundary: deploy-host facts (backend URL, passphrase, CLI root) are `automation.md`'s Config surface; extension validation is `kube/data.md`'s; sizing interpretation is `kube/workload.md`'s; tenant realization is `kube/tenant.md`'s.
- Packages: `effect` (`Schema`); `@rasm/ts/core` (`AppIdentity`).

```typescript
import { AppIdentity } from "@rasm/ts/core"
import { Schema } from "effect"

const _arms = ["selfhosted-k8s", "selfhosted-docker", "aws", "gcp", "cloudflare"] as const
const _tiers = {
  "selfhosted-k8s": "primary",
  "selfhosted-docker": "realized",
  aws: "prepared",
  gcp: "prepared",
  cloudflare: "prepared",
} as const

const _Name = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]{1,39}$/), Schema.brand("StackName"))

class Connection extends Schema.Class<Connection>("Connection")({
  host: Schema.NonEmptyString,
  user: Schema.optionalWith(Schema.NonEmptyString, { default: () => "root" }),
  port: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 65535)), { default: () => 22 }),
}) {
  get ssh(): string {
    return `ssh://${this.user}@${this.host}:${this.port}`
  }
}

const _Doppler = Schema.Struct({
  project: Schema.NonEmptyString,
  config: Schema.NonEmptyString,
})

const _Data = Schema.Struct({
  instances: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 9)), { default: () => 2 }),
  storage: Schema.optionalWith(Schema.NonEmptyString, { default: () => "20Gi" }),
  backupCron: Schema.optionalWith(Schema.NonEmptyString, { default: () => "0 0 3 * * *" }),
  retention: Schema.optionalWith(Schema.NonEmptyString, { default: () => "30d" }),
})

const _Fanout = Schema.Struct({
  replicas: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 5)), { default: () => 3 }),
  storage: Schema.optionalWith(Schema.NonEmptyString, { default: () => "2Gi" }),
})

const _Tenancy = Schema.Struct({
  mode: Schema.optionalWith(Schema.Literal("single", "namespace", "vcluster"), { default: () => "single" as const }),
  pgTier: Schema.optionalWith(Schema.Literal("shared-rls", "db-per-tenant", "cluster-per-tenant"), { default: () => "shared-rls" as const }),
  tenants: Schema.optionalWith(Schema.Array(Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]{1,30}$/))), { default: () => [] }),
})

const _Profile = Schema.Struct({
  scale: Schema.optionalWith(Schema.Literal("dev", "standard", "fleet"), { default: () => "standard" as const }),
  compute: Schema.optionalWith(Schema.Literal("serverless", "cluster"), { default: () => "serverless" as const }),
  extensions: Schema.optionalWith(Schema.Array(Schema.NonEmptyString), { default: () => [] }),
  objectEngine: Schema.optionalWith(Schema.Literal("minio", "ceph"), { default: () => "minio" as const }),
  exposure: Schema.optionalWith(Schema.Literal("direct", "tunnel"), { default: () => "direct" as const }),
  data: Schema.optionalWith(_Data, { default: () => _Data.make({}) }),
  fanout: Schema.optionalWith(_Fanout, { default: () => _Fanout.make({}) }),
  tenancy: Schema.optionalWith(_Tenancy, { default: () => _Tenancy.make({}) }),
})

class StackSpec extends Schema.Class<StackSpec>("StackSpec")({
  name: _Name,
  app: AppIdentity.fields.app,
  target: Schema.Literal(..._arms),
  backend: Schema.optionalWith(Schema.Literal("self-managed", "cloud"), { default: () => "self-managed" as const }),
  region: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  domain: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  zone: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  project: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  account: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  connection: Schema.optionalWith(Connection, { as: "Option" }),
  image: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  doppler: _Doppler,
  epoch: Schema.optionalWith(Schema.NonEmptyString, { default: () => "0" }),
  profile: Schema.optionalWith(_Profile, { default: () => _Profile.make({}) }),
}) {
  static readonly arms: StackSpec.Arms = _arms
  static readonly tiers: StackSpec.Tiers = _tiers
  get primary(): boolean {
    return _tiers[this.target] === "primary"
  }
  get hosted(): boolean {
    return this.backend === "cloud"
  }
  get tenants(): ReadonlyArray<string> {
    return this.profile.tenancy.mode === "single" ? [] : this.profile.tenancy.tenants
  }
}

declare namespace StackSpec {
  type Arms = typeof _arms
  type Tiers = typeof _tiers
  type Arm = (typeof _arms)[number]
  type Tier = (typeof _tiers)[Arm]
  type Connection = InstanceType<typeof Connection>
  type Profile = typeof _Profile.Type
  type Tenancy = typeof _Tenancy.Type
  type _Tiers<T extends { readonly [K in Arm]: "primary" | "realized" | "prepared" } = typeof _tiers> = T
  type _Keys<K extends Arm = keyof typeof _tiers> = K
}
```

## [4]-[TIER_BASE]

[TIER_BASE]:
- Owner: `Tier`, the abstract `pulumi.ComponentResource` subclass every grouped concern extends — the constructor stamps the type token `rasm:iac:<Kind>`, `child(overrides?)` folds `{ parent: this }` into per-resource overrides through `pulumi.mergeOptions` so ownership is inherited and never restated, and `seal(outputs)` is the mandatory terminal `registerOutputs` call.
- Law: the constructor is the platform seam — Pulumi's model is class heritage with field assignment, so `super(...)`, child construction, and readonly field assignment are the exemption's whole extent; a tier member beyond the constructor is an expression-shaped projection over already-constructed outputs.
- Law: options are algebra, not assembly — `child()` is the only way a child receives options: `parent` rides the fold, an explicit `provider`/`providers` set at tier construction flows down the chain, `dependsOn` states only genuine extra-graph edges (an `Output` reference already is one), `protect: true` marks tiers owning irreplaceable state, `aliases` accompany a rename so state survives it, `ignoreChanges` quarantines fields an operator mutates out-of-band, and a `ResourceHook`/`ErrorHook` binding is one more `child()` override row when a tier earns lifecycle interception — the fold is the single channel every option class travels.
- Law: `seal` closes every constructor — an unsealed tier reports no outputs and its dependents race construction; the sealed record is the tier's public evidence and mirrors the readonly fields the class exposes.
- Law: adoption is not composition — a `ComponentResource` has no `static get`; a pre-existing cloud object adopts through its own resource class `get` or `opts.import` inside the owning tier, and the tier remains the sole author thereafter.
- Law: the tier tree is closed and page-owned — `Bootstrap` (`provider.md`), `Secrets`/`Certs` (`operate/secret.md`), `ObjectStore`/`Nats`/`Postgres` (`kube/data.md`), `Workload` (`kube/workload.md`), `Traffic` (`kube/traffic.md`), `Tenants` (`kube/tenant.md`), `Lgtm`/`Boards` (`operate/observe.md`), `CloudPlane` (`operate/cloud.md`), `Source` (`program/source.md`) — each a subclass whose declaration and invariants live on its owning page; a concern with no tier row composes inside an existing tier before a new subclass is minted, and a rename travels as an `aliases` row, never a silent replacement.
- Growth: a new tier is one subclass row on its owning page plus its roster mention here; the base never grows knobs.
- Packages: `@pulumi/pulumi` (`ComponentResource`, `ComponentResourceOptions`, `CustomResourceOptions`, `mergeOptions`, `Inputs`).

```typescript
import * as pulumi from "@pulumi/pulumi"

abstract class Tier extends pulumi.ComponentResource {
  constructor(kind: string, name: string, opts?: pulumi.ComponentResourceOptions) {
    super(`rasm:iac:${kind}`, name, {}, opts)
  }
  protected child(overrides?: pulumi.CustomResourceOptions): pulumi.CustomResourceOptions {
    return pulumi.mergeOptions({ parent: this }, overrides)
  }
  protected seal(outputs: pulumi.Inputs): void {
    this.registerOutputs(outputs)
  }
}
```

## [5]-[OUTPUT_PLANES]

[OUTPUT_PLANES]:
- Owner: `StackOutputs`, one `Schema.Class` of `Option`-carried plane records — `ingress` (public hostname), `data` (host, port, database, role), `object` (endpoint, bucket), `fanout` (the NATS websocket origin), `otlp` (collector ingest endpoint), `grafana` (board URL), `sharding` (runner endpoint), `deploy` (the time-ordered `RandomUuid7` deployment identity) — each an inline `Schema.Struct` block because no plane has a second consumer shape; the arm that realizes a plane returns its keys from the `PulumiFn`, and absence means the arm did not realize it.
- Law: `pairsOf(planes, render)` is the one channel-flatten — the `<plane>.<field>` spelling and the plane iteration exist exactly here, parameterized on the value renderer; the decoded `pairs` getter rides it with `String`, the in-program live assembly rides it with `pulumi.output(value).apply(String)`, and the plane set feeding the getter derives from the class's own field record through `Record.getSomes`, so no hand-listed plane tuple exists, a new field cannot be silently dropped, and the two modalities cannot drift.
- Law: `read(stack, name)` is the one exit from the engine's `OutputMap` — `stack.outputs()` converts at this seam with the `DeployFault` triage, one entries scan yields both the secret-refusal verdict and the leaked-key evidence (the gate refuses any `{ secret: true }` entry naming the keys in the fault detail), the `{ value, secret }` envelope strips to plain values, and the record decodes through the class; the `Object` reads sit inside the boundary because the map is FFI material, and no decoded value is re-checked downstream.
- Law: coordinates, never material — a role name, host, port, origin, or URL is publishable; a password, token, or key is not, and the fix for a refused output is moving the value into the Doppler store, never widening the gate.
- Law: decode failure is admission evidence — the configured decode (`errors: "all"`, `onExcessProperty: "error"`) makes an output key no field admits, or a malformed plane record, fail loudly and re-spell the `ParseError` as an `input` fault, because the program and this owner are two spellings of one contract and drift between them is a defect at the seam.
- Law: `sharding` is the sole value crossing back to the runtime graph — `work`'s `ShardingConfig.layerFromEnv` consumes the env rows the sharding channels populate, deployment topology stays plane-distinct, and no runtime import exists in either direction; every other plane serves the app's own boot config through the same env assembly; `kube/workload.md` owns the channel-to-env-key spelling, so this page never encodes a consumer's variable names.
- Law: the projection is total over presence — absent planes contribute zero rows, values render through the injected renderer at this seam exactly once, and a consumer never re-derives a pair from the decoded owner; the widened `Record<string, string | number>` view on the fold is the type-seam bracket posture, since every plane record is flat scalars by construction.
- Entry: `StackOutputs.read(stack, spec.name)` after any `up`; the plane records project by field access; `outputs.pairs` into the workload env assembly; `StackOutputs.pairsOf(record, render)` inside a program body over live `Output`s.
- Growth: a new plane is one `Option` field plus its arm return keys — the channels derive from the field record and nothing else edits.
- Boundary: which keys each arm returns is `provider.md`'s program body; receipt evidence is `automation.md`'s — outputs and receipts never merge.
- Packages: `effect` (`Effect`, `Schema`, `Option`, `Array`, `Record`); `@pulumi/pulumi/automation` (`Stack`); `./automation.ts` (`DeployFault`).

```typescript
import type { Stack } from "@pulumi/pulumi/automation"
import { Array, Effect, Option, Record, Schema } from "effect"
import { DeployFault } from "./automation.ts"

const _Port = Schema.Int.pipe(Schema.between(1, 65535))

class StackOutputs extends Schema.Class<StackOutputs>("StackOutputs")({
  ingress: Schema.optionalWith(Schema.Struct({ hostname: Schema.NonEmptyString }), { as: "Option" }),
  data: Schema.optionalWith(Schema.Struct({
    host: Schema.NonEmptyString,
    port: _Port,
    database: Schema.NonEmptyString,
    role: Schema.NonEmptyString,
  }), { as: "Option" }),
  object: Schema.optionalWith(Schema.Struct({ endpoint: Schema.NonEmptyString, bucket: Schema.NonEmptyString }), { as: "Option" }),
  fanout: Schema.optionalWith(Schema.Struct({ origin: Schema.NonEmptyString }), { as: "Option" }),
  otlp: Schema.optionalWith(Schema.Struct({ endpoint: Schema.NonEmptyString }), { as: "Option" }),
  grafana: Schema.optionalWith(Schema.Struct({ url: Schema.NonEmptyString }), { as: "Option" }),
  sharding: Schema.optionalWith(Schema.Struct({ host: Schema.NonEmptyString, port: _Port }), { as: "Option" }),
  deploy: Schema.optionalWith(Schema.Struct({ id: Schema.UUID }), { as: "Option" }),
}) {
  static readonly pairsOf = <V, R>(
    planes: Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, V>>,
    render: (value: V) => R,
  ): ReadonlyArray<readonly [channel: string, value: R]> =>
    Array.flatMap(Record.toEntries(planes), ([plane, held]) =>
      Array.map(Record.toEntries(held), ([field, value]) => [`${plane}.${field}`, render(value)] as const))
  static readonly read = (stack: Stack, name: string): Effect.Effect<StackOutputs, DeployFault> =>
    Effect.tryPromise({ try: () => stack.outputs(), catch: DeployFault.triaged(name) }).pipe(
      Effect.flatMap((outputs) => {
        const leaked = Object.entries(outputs).filter(([, entry]) => entry.secret === true).map(([key]) => key)
        return leaked.length === 0
          ? Effect.succeed(Object.fromEntries(Object.entries(outputs).map(([key, entry]) => [key, entry.value])))
          : Effect.fail(new DeployFault({ reason: "input", stack: name, detail: leaked.join(",") }))
      }),
      Effect.flatMap((record) =>
        Effect.mapError(
          Schema.decodeUnknown(StackOutputs, { errors: "all", onExcessProperty: "error" })(record),
          (parse) => new DeployFault({ reason: "input", stack: name, detail: parse.message }),
        )),
    )
  get pairs(): ReadonlyArray<StackOutputs.Pair> {
    const planes: Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, string | number>> =
      Record.getSomes(Record.map(StackOutputs.fields, (_, plane) => this[plane as StackOutputs.Plane]))
    return StackOutputs.pairsOf(planes, String)
  }
}

declare namespace StackOutputs {
  type Pair = readonly [channel: string, value: string]
  type Plane = keyof typeof StackOutputs.fields & keyof StackOutputs
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Connection, StackOutputs, StackSpec, Tier }
```
