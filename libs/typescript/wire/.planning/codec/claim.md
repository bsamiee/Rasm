# [WIRE_CLAIM]

`codec/claim.ts` decodes the benchmark identity gate — `BenchmarkClaimWire` and `HostFingerprintWire` from `Rasm.AppHost/Observability`: a benchmark claim is admissible evidence only on the host that minted it, so the decode pairs every claim with its host fingerprint and the gate refuses a claim whose fingerprint diverges from the running `AppIdentity`'s host dimension. Admitted claims feed `ui/viewer` `probe/benchmark` through `#vocab`; a cross-host claim is `parity` evidence, never a silently displayed number.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                            |
| :-----: | :-------------- | :--------------------------------------------------------------------- |
|   [1]   | `IDENTITY_GATE` | the claim + fingerprint owners and the fingerprint-gated admission      |

## [2]-[IDENTITY_GATE]

- Owner: `Claim` — one `Schema.Class` carrying the measurement rows and its `host` fingerprint block as an embedded class at full depth; `HostFingerprint` rides the owner's name, and the gate static is the one admission surface.
- Entry: `Claim.admit(octets, identity)` — decode plus gate in one rail: the claim decodes through the proto engine, the embedded fingerprint compares against the kernel `AppIdentity`'s host dimension, and a divergence refuses with `WireFault` reason `parity` carrying both fingerprints as evidence.
- Receipt: an admitted claim is displayable evidence — suite, metric rows, the mint instant, and the proven-local fingerprint; the probe UI renders it with provenance intact.
- Growth: a new measurement axis is one `_Metric` row field; a new fingerprint dimension is a field on `HostFingerprint` mirroring the C# emit, and the gate compare widens with it.
- Law: the gate is identity, not trust — fingerprint equality is structural over the decoded block; TS never re-derives a host fingerprint (the kernel `AppIdentity` carries the local one, minted at boot), and a claim without a fingerprint cannot decode because the field is total.
- Law: refusal preserves evidence — the `parity` fault carries `{ actual, expected }` fingerprints, so the operator sees WHICH host minted the stale claim; a dropped claim is the rejected form.
- Boundary: benchmark presentation is `ui/viewer` `probe/benchmark`; the `AppIdentity` value arrives from `kernel/identity/appidentity` through the caller, never read ambiently here.

```typescript
import { AppIdentity } from "@rasm/ts/kernel"
import { Effect, Equal, Option, type ParseResult, Schema } from "effect"
import { WireFault } from "../fault/quarantine.ts"
import { ProtoCodec } from "./proto.ts"

class HostFingerprint extends Schema.Class<HostFingerprint>("HostFingerprint")({
  machine: Schema.NonEmptyString,
  arch: Schema.NonEmptyString,
  cores: Schema.Int.pipe(Schema.positive()),
  runtime: Schema.NonEmptyString,
}) {}

const _Metric = Schema.Struct({
  label: Schema.NonEmptyString,
  value: Schema.Number,
  unit: Schema.NonEmptyString,
})

class Claim extends Schema.Class<Claim>("Claim")({
  suite: Schema.NonEmptyString,
  metrics: Schema.NonEmptyArray(_Metric),
  host: HostFingerprint,
  minted: Schema.DateTimeUtc,
}) {
  static readonly Host: typeof HostFingerprint = HostFingerprint
  static readonly FromBytes: Schema.Schema<Claim, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.BenchmarkClaimWire, Claim)
  static readonly fingerprint: Schema.Schema<HostFingerprint, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.HostFingerprintWire, HostFingerprint)
  static readonly admit = (octets: Uint8Array, identity: AppIdentity): Effect.Effect<Claim, ParseResult.ParseError | WireFault> =>
    Effect.gen(function* () {
      const claim = yield* Schema.decodeUnknown(Claim.FromBytes)(octets)
      return Equal.equals(claim.host, identity.host)
        ? claim
        : yield* new WireFault({
            family: "BenchmarkClaimWire",
            reason: "parity",
            detail: "<foreign-host-claim>",
            evidence: Option.some({ actual: claim.host, expected: identity.host }),
          })
    })
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Claim }
```
