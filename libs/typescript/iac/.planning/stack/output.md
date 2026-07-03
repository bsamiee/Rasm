# [IAC_OUTPUT]

The typed stack-output owner: every value a deployed stack publishes decodes once through `StackOutputs` — one `Schema.Class` whose per-plane records (`data`, `object`, `ingress`, `otlp`, `grafana`, `sharding`) are `Option`-carried because arms realize different subsets — and the raw `OutputMap` never travels past this seam. Outputs are coordinates, never material: a secret-flagged entry in the map is refused outright, because every credential lives in Doppler and a stack output that carries one is a leak into state files, receipts, and logs. The `sharding` record is the sole iac/work meeting seam — its pairs land as environment facts through `secret/inject`, and `work`'s `ShardingConfig.layerFromEnv` reads them on the runtime side, so the two altitudes meet at the process boundary and never import each other. The module is `iac/src/stack/output.ts`; a new published plane is one `Option` field plus one `_PLANES` entry — its channels derive from the field record — and an output no field admits fails the decode loudly.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                              | [PUBLIC]       |
| :-----: | :-------------- | :-------------------------------------------------------------------- | :------------- |
|  [01]   | `OUTPUT_DECODE` | the decoded owner, the secret refusal gate, and the `OutputMap` read | `StackOutputs` |
|  [02]   | `WORK_SEAM`     | the channel-pair projection feeding the env assembly                 | `StackOutputs` |

## [2]-[OUTPUT_DECODE]

[OUTPUT_DECODE]:
- Owner: `StackOutputs`, one `Schema.Class` of `Option`-carried plane records — `ingress` (public hostname), `data` (host, port, database, role), `object` (endpoint, bucket), `otlp` (collector ingest endpoint), `grafana` (board URL), `sharding` (runner endpoint) — each an inline `Schema.Struct` block because no plane has a second consumer shape; the arm that realizes a plane returns its keys from the `PulumiFn`, and absence means the arm did not realize it.
- Law: `read(stack, name)` is the one exit from the engine's `OutputMap` — `stack.outputs()` converts at this seam with the `DeployFault` triage, the secret gate refuses any `{ secret: true }` entry naming the leaked keys in the fault detail, the `{ value, secret }` envelope strips to plain values, and the record decodes through the class; the `Object` reads sit inside the boundary because the map is FFI material, and no decoded value is re-checked downstream.
- Law: coordinates, never material — a role name, host, port, or URL is publishable; a password, token, or key is not, and the fix for a refused output is moving the value into `secret/doppler`, never widening the gate.
- Law: decode failure is admission evidence — an output key no field admits, or a malformed plane record, re-spells the `ParseError` as an `input` fault, because the program and this owner are two spellings of one contract and drift between them is a defect at the seam.
- Entry: `StackOutputs.read(stack, spec.name)` after any `up`; the plane records project by field access.
- Growth: a new plane is one `Option` field, its arm return keys, and one `[3]` `_PLANES` entry.
- Boundary: which keys each arm returns is `provider/dispatch.md`'s program body; receipt evidence is `program/automation.md`'s — outputs and receipts never merge.
- Packages: `effect` (`Effect`, `Schema`, `Option`, `Array`, `Record`); `@pulumi/pulumi/automation` (`Stack`); `../program/automation.ts` (`DeployFault`).

```typescript
import type { Stack } from "@pulumi/pulumi/automation"
import { Array, Effect, Option, Record, Schema } from "effect"
import { DeployFault } from "../program/automation.ts"

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
  otlp: Schema.optionalWith(Schema.Struct({ endpoint: Schema.NonEmptyString }), { as: "Option" }),
  grafana: Schema.optionalWith(Schema.Struct({ url: Schema.NonEmptyString }), { as: "Option" }),
  sharding: Schema.optionalWith(Schema.Struct({ host: Schema.NonEmptyString, port: _Port }), { as: "Option" }),
}) {
  get pairs(): ReadonlyArray<StackOutputs.Pair> {
    return Array.flatMap(_PLANES, (plane) =>
      Array.flatMap(Option.toArray(this[plane]), (held: Record<string, string | number>) =>
        Array.map(Record.toEntries(held), ([field, value]) => [`${plane}.${field}`, String(value)] as const)))
  }
  static readonly read = (stack: Stack, name: string): Effect.Effect<StackOutputs, DeployFault> =>
    Effect.tryPromise({ try: () => stack.outputs(), catch: DeployFault.triaged(name) }).pipe(
      Effect.filterOrFail(
        (outputs) => Object.values(outputs).every((entry) => entry.secret === false),
        (outputs) =>
          new DeployFault({
            reason: "input",
            stack: name,
            detail: Object.keys(outputs).filter((key) => outputs[key]?.secret === true).join(","),
          }),
      ),
      Effect.map((outputs) => Object.fromEntries(Object.entries(outputs).map(([key, entry]) => [key, entry.value]))),
      Effect.flatMap((record) =>
        Effect.mapError(
          Schema.decodeUnknown(StackOutputs)(record),
          (parse) => new DeployFault({ reason: "input", stack: name, detail: parse.message }),
        )),
    )
}
```

## [3]-[WORK_SEAM]

[WORK_SEAM]:
- Law: the seam is env-shaped and derived — the `pairs` getter flattens every realized plane into `[channel, value]` rows whose channel spelling computes as `<plane>.<field>` from the owner's own field record (`data.host`, `sharding.port`, `otlp.endpoint`), so no pair row is ever hand-listed and a field rename cannot drift from its channel; `secret/inject.md` owns the channel-to-env-key spelling, so this page never encodes a consumer's variable names and a key rename lands in one map there.
- Law: `sharding` is the sole value crossing back to the runtime graph — `work`'s `ShardingConfig.layerFromEnv` consumes the env rows the sharding channels populate, deployment topology stays `plane:deploy`, and no `work` import exists in either direction; every other plane serves the app's own boot config through the same env assembly.
- Law: the projection is total over presence — absent planes contribute zero rows, numbers render through `String` at this seam exactly once, and a consumer never re-derives a pair from the decoded owner.
- Law: `pairs` is a derived reading and rides the owner as a getter — the module exports one name, and the seam consumer reaches the owner, its planes, and its pairs through it; the widened `Record<string, string | number>` view on the fold is the type-seam bracket posture, since every plane record is flat scalars by construction.
- Growth: a new plane is one `Option` field plus one `_PLANES` entry — its channels derive from the field record with zero listed rows, and the `_Planes` guard rejects an entry naming no real field.
- Boundary: env assembly, the token secret, and the container entrypoint are `secret/inject.md`'s; the cluster package's own env spellings are the work seam's contract, pinned there.

```typescript
const _PLANES = ["ingress", "data", "object", "otlp", "grafana", "sharding"] as const

declare namespace StackOutputs {
  type Pair = readonly [channel: string, value: string]
  type _Planes<K extends keyof StackOutputs = (typeof _PLANES)[number]> = K
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { StackOutputs }
```
