# [IAC_SPEC]

The `StackSpec` vocabulary: ONE decoded value carries everything an app supplies to deploy — target arm, capability profile, region/domain/zone coordinates, the Doppler project ref, the rotation epoch, and the bootstrap connection coordinates. The arm union anchors here and every downstream surface derives from it: `provider/dispatch` keys its closed handler record on `StackSpec.Arm`, `provider/surface` columns its equivalence map by the same union, and the tier vocabulary (`primary | realized | prepared`) is a column of the arm table, so finalizing a prepared cloud row is exactly one `StackSpec` value — never a lib edit, never a fork. The spec carries coordinates, never material: no key, token, password, or credential field exists on the shape, and every secret reaches its consumer through the `secret/doppler` fan-in or the `secret/inject` process boundary. The module is `iac/src/program/spec.ts`; a new arm is one `_arms` entry plus one tier row, a new deployment axis is one field with its default in the declaration.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                        | [PUBLIC]    |
| :-----: | :--------------- | :------------------------------------------------------------- | :---------- |
|  [01]   | `ARM_VOCABULARY` | the closed arm tuple, the tier column, and the derived unions  | `StackSpec` |
|  [02]   | `SPEC_OWNER`     | the app-supplied value: coordinates, profile, and defaults     | `StackSpec` |

## [2]-[ARM_VOCABULARY]

[ARM_VOCABULARY]:
- Owner: the interior `_arms` key tuple plus the `_tiers` column — order, iteration, and the non-empty `Schema.Literal` spread are tuple facts stated once; `StackSpec.Arm` derives on the interior anchor and `StackSpec.arms`/`StackSpec.tiers` ride the class as statics, so the arm roster has one edit site branch-wide.
- Law: the tier column is the promotion vocabulary — `selfhosted-k8s` is `primary` (the first-class arm every capability row realizes), `selfhosted-docker` is `realized` (a live arm over an owned daemon), and `aws`/`gcp`/`cloudflare` are `prepared` (arms whose provider seam is live and whose capability realization lands when an app finalizes with a spec value); `StackSpec.primary` projects the row so dispatch and policy read promotion as data, never a name comparison.
- Growth: a new cloud is one `_arms` entry plus one `_tiers` row — the dispatch record, the surface map, and the `Schema.Literal` admission all break at compile time until their rows land.
- Boundary: which resources an arm composes is `provider/surface.md`'s column; which program body runs is `provider/dispatch.md`'s record row.

## [3]-[SPEC_OWNER]

[SPEC_OWNER]:
- Owner: `StackSpec`, one `Schema.Class` — `name` (a DNS-safe stack slug brand), `app` (the kernel `AppKey` brand composed as `AppIdentity.fields.app`, so app identity has one spelling branch-wide), `target` (the arm literal), the coordinate options (`region` for prepared clouds, `domain`/`zone` for the traffic rows, `connection` for the selfhosted bootstrap, `image` for the app workload ref), the `doppler` project/config ref, the `epoch` rotation trigger, and the `profile` capability record.
- Law: coordinates, never material — `connection` carries host/user/port and no key field; the SSH private key, provider tokens, and generated passwords travel the `Dispatch.material` Config read or the in-graph Doppler fan-in, so a spec value is loggable, diffable, and safe in any receipt.
- Law: `epoch` is the one rotation trigger — it feeds every `@pulumi/random` `keepers` map and every `@pulumi/command` `triggers` list, so bumping one field re-mints credentials and re-runs bootstrap deliberately; per-resource rotation knobs are the named defect.
- Law: the profile is defaults-total — `scale` selects the `kube/workload` sizing row, `extensions` names the `store` matrix subset the data tier finalizes (validated against `Matrix.rows` at `kube/data.md`, never here), `objectEngine` selects the MinIO-versus-Garage row, `exposure` selects the direct-DNS-versus-tunnel traffic row, and `data` carries instance count, storage size, backup cron, and retention — every field defaulted at the declaration so `_Profile.make({})` is a complete standard deployment and an app states only its deltas.
- Law: absence is `Option` admitted by `Schema.optionalWith(..., { as: "Option" })` — a prepared arm demanding an absent `region`, or a selfhosted arm demanding an absent `connection`, fails as a typed `DeployFault` inside its dispatch arm, never as an `undefined` read.
- Entry: `StackSpec.make(...)` at the app seam; `Schema.decodeUnknown(StackSpec)` where the value arrives as data.
- Growth: a new coordinate is one field with its dialect chosen here; a new profile axis is one `_Profile` field with its default.
- Boundary: host facts (backend URL, passphrase, CLI root) are `program/automation.md`'s Config surface — deploy-host environment, never spec fields; extension validation is `kube/data.md`'s; sizing interpretation is `kube/workload.md`'s.
- Packages: `effect` (`Schema`); `@rasm/ts/kernel` (`AppIdentity`).

```typescript
import { AppIdentity } from "@rasm/ts/kernel"
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

const _Connection = Schema.Struct({
  host: Schema.NonEmptyString,
  user: Schema.optionalWith(Schema.NonEmptyString, { default: () => "root" }),
  port: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 65535)), { default: () => 22 }),
})

const _Doppler = Schema.Struct({
  project: Schema.NonEmptyString,
  config: Schema.NonEmptyString,
})

const _Data = Schema.Struct({
  instances: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 9)), { default: () => 2 }),
  storage: Schema.optionalWith(Schema.NonEmptyString, { default: () => "20Gi" }),
  backupCron: Schema.optionalWith(Schema.NonEmptyString, { default: () => "0 3 * * *" }),
  retention: Schema.optionalWith(Schema.NonEmptyString, { default: () => "30d" }),
})

const _Profile = Schema.Struct({
  scale: Schema.optionalWith(Schema.Literal("dev", "standard", "fleet"), { default: () => "standard" as const }),
  extensions: Schema.optionalWith(Schema.Array(Schema.NonEmptyString), { default: () => [] }),
  objectEngine: Schema.optionalWith(Schema.Literal("minio", "garage"), { default: () => "minio" as const }),
  exposure: Schema.optionalWith(Schema.Literal("direct", "tunnel"), { default: () => "direct" as const }),
  data: Schema.optionalWith(_Data, { default: () => _Data.make({}) }),
})

class StackSpec extends Schema.Class<StackSpec>("StackSpec")({
  name: _Name,
  app: AppIdentity.fields.app,
  target: Schema.Literal(..._arms),
  region: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  domain: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  zone: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  connection: Schema.optionalWith(_Connection, { as: "Option" }),
  image: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  doppler: _Doppler,
  epoch: Schema.optionalWith(Schema.NonEmptyString, { default: () => "0" }),
  profile: Schema.optionalWith(_Profile, { default: () => _Profile.make({}) }),
}) {
  static readonly arms = _arms
  static readonly tiers = _tiers
  get primary(): boolean {
    return _tiers[this.target] === "primary"
  }
}

declare namespace StackSpec {
  type Arm = (typeof _arms)[number]
  type Tier = (typeof _tiers)[Arm]
  type Connection = typeof _Connection.Type
  type Profile = typeof _Profile.Type
  type _Tiers<T extends Record<Arm, "primary" | "realized" | "prepared"> = typeof _tiers> = T
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { StackSpec }
```
