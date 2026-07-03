# [WIRE_CREDENTIAL]

`codec/credential.ts` decodes the `CredentialPemWire` redacted carrier from `Rasm.AppHost`: PEM key material seals into `Redacted` AT the decode transform — the secret never exists raw past its admission expression — and the decoded `Credential` terminates in `security/secret`'s material vocabulary through app-root wiring (`security` and `wire` never import each other; the value crosses at composition). The fingerprint travels beside the sealed material so rotation and audit compare identities without ever unwrapping.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                          |
| :-----: | :----------------- | :------------------------------------------------------------------ |
|   [1]   | `REDACTED_CARRIER` | the sealed `Credential` owner: kind vocabulary, decode, rotation compare |

## [2]-[REDACTED_CARRIER]

- Owner: `Credential` — one `Schema.Class`: the closed `kind` vocabulary row, the `Schema.Redacted`-sealed material field, the content fingerprint, and the validity window; decode statics ride the class.
- Entry: `Credential.FromBytes` composing the proto engine; `Credential.decode(octets)` the one-shot rail; `Credential.rotated(live, next)` the sealed comparison — policy reads never unwrap.
- Receipt: the decoded credential is a sealed carrier — `fingerprint` (the C#-minted key identity), `kind`, `notBefore`/`notAfter` are the readable evidence; `material` prints `<redacted>` on every string, JSON, and inspect channel by construction.
- Growth: a new credential kind is one literal row in `_kinds` — the C# emit and this vocabulary move together; a new lifecycle field mirrors the wire.
- Law: sealed at admission — `Schema.Redacted(Schema.String)` seals inside the decode, so no interior line ever holds the raw PEM; `Redacted.value` unwrap happens exactly once, inside `security`'s consuming boundary, and `Redacted.unsafeWipe` retirement is `security`'s lifecycle verb.
- Law: comparison is sealed — `rotated` rides `Redacted.getEquivalence(Equivalence.string)`; an unwrap-and-compare spelling is the leak defect.
- Law: the fingerprint is identity, not material — audit trails, rotation logs, and telemetry carry the fingerprint alone; a log line that could carry material cannot exist because the field type refuses it.
- Boundary: key parsing, signing, and storage are `security/secret`'s owners; this module lands the sealed value and nothing downstream of the seal.

```typescript
import { Effect, Equivalence, type ParseResult, Redacted, Schema } from "effect"
import { ProtoCodec } from "./proto.ts"

const _kinds = ["signing", "tls", "api"] as const

const _sameMaterial: Equivalence.Equivalence<Redacted.Redacted<string>> = Redacted.getEquivalence(Equivalence.string)

class Credential extends Schema.Class<Credential>("Credential")({
  kind: Schema.Literal(..._kinds),
  material: Schema.Redacted(Schema.String),
  fingerprint: Schema.NonEmptyString,
  notBefore: Schema.DateTimeUtc,
  notAfter: Schema.DateTimeUtc,
}) {
  static readonly FromBytes: Schema.Schema<Credential, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.CredentialPemWire, Credential)
  static readonly decode: (octets: Uint8Array) => Effect.Effect<Credential, ParseResult.ParseError> = Schema.decodeUnknown(Credential.FromBytes)
  static readonly rotated = (live: Credential, next: Credential): boolean => !_sameMaterial(live.material, next.material)
}

declare namespace Credential {
  type Kind = (typeof _kinds)[number]
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Credential }
```
